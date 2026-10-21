using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;

namespace EchoSphere.Core
{
    [Serializable]
    public sealed class SnapshotValue
    {
        // Flat node storage avoids Unity JsonUtility's nested-serialization depth limit.
        // parentIndex points to another entry in nodes; -1 identifies the root.
        public List<SnapshotNode> nodes = new List<SnapshotNode>();
        public int rootIndex;
    }

    [Serializable]
    public sealed class SnapshotNode
    {
        public string name, kind, text;
        public int parentIndex;
        public List<int> children = new List<int>();
    }

    // JsonUtility-compatible representation of the source's JSON run snapshot.
    // The serialized representation is flat; capture/restore still follows the
    // original object/list/map hierarchy without recursively serializing objects.
    public static class RunSnapshotCodec
    {
        private const BindingFlags Fields = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

        public static bool IsEngineObject(Type type)
        {
            for (var current = type; current != null; current = current.BaseType)
                if (current.FullName == "UnityEngine.Object") return true;
            return false;
        }

        private static bool CanStore(Type type)
        {
            if (IsEngineObject(type) || typeof(Delegate).IsAssignableFrom(type)) return false;
            if (type.IsArray) return CanStore(type.GetElementType());
            if (type.IsGenericType)
                foreach (var argument in type.GetGenericArguments()) if (!CanStore(argument)) return false;
            return type == typeof(string) || type.IsPrimitive || type.IsEnum || type.IsValueType ||
                type.Namespace == "EchoSphere.Core" || type.Namespace == "EchoSphere.Runtime" ||
                type.Namespace == "System.Collections.Generic";
        }

        public static SnapshotValue Capture(object value, Func<FieldInfo, bool> select = null)
        {
            var snapshot = new SnapshotValue();
            snapshot.nodes.Add(new SnapshotNode { parentIndex = -1 });
            CaptureInto(snapshot, 0, value, select);
            return snapshot;
        }

        private static void CaptureInto(SnapshotValue snapshot, int index, object value, Func<FieldInfo, bool> select)
        {
            var node = snapshot.nodes[index];
            if (value == null) { node.kind = "null"; return; }

            var type = value.GetType();
            if (type == typeof(string) || type.IsPrimitive || type.IsEnum)
            {
                node.kind = "scalar";
                node.text = value is float f ? f.ToString("R", CultureInfo.InvariantCulture) :
                    value is double d ? d.ToString("R", CultureInfo.InvariantCulture) :
                    Convert.ToString(value, CultureInfo.InvariantCulture);
                return;
            }

            if (value is IDictionary map)
            {
                node.kind = "map";
                foreach (DictionaryEntry entry in map)
                {
                    var keyIndex = AddNode(snapshot, index, null);
                    CaptureInto(snapshot, keyIndex, entry.Key, null);
                    var valueIndex = AddNode(snapshot, index, null);
                    CaptureInto(snapshot, valueIndex, entry.Value, null);
                }
                return;
            }

            if (value is IEnumerable sequence)
            {
                node.kind = "list";
                foreach (var item in sequence)
                {
                    var itemIndex = AddNode(snapshot, index, null);
                    CaptureInto(snapshot, itemIndex, item, null);
                }
                return;
            }

            node.kind = "object";
            foreach (var field in type.GetFields(Fields))
            {
                if (field.IsStatic || !CanStore(field.FieldType) || (select != null && !select(field))) continue;
                var childIndex = AddNode(snapshot, index, field.Name);
                CaptureInto(snapshot, childIndex, field.GetValue(value), null);
            }
        }

        private static int AddNode(SnapshotValue snapshot, int parentIndex, string name)
        {
            var index = snapshot.nodes.Count;
            snapshot.nodes.Add(new SnapshotNode { name = name, parentIndex = parentIndex });
            snapshot.nodes[parentIndex].children.Add(index);
            return index;
        }

        public static SnapshotValue Field(SnapshotValue node, string name)
        {
            if (node == null || node.nodes == null || node.rootIndex < 0 || node.rootIndex >= node.nodes.Count) return null;
            for (var i = 0; i < node.nodes.Count; i++)
                if (node.nodes[i].parentIndex == node.rootIndex && node.nodes[i].name == name)
                    return View(node, i);
            return null;
        }

        public static T Read<T>(SnapshotValue node, string field, T fallback = default)
        {
            var value = Field(node, field);
            return value == null ? fallback : (T)Restore(value, typeof(T));
        }

        private static SnapshotValue View(SnapshotValue source, int rootIndex) =>
            new SnapshotValue { nodes = source.nodes, rootIndex = rootIndex };

        public static object Restore(SnapshotValue node, Type type, object existing = null, Func<FieldInfo, bool> select = null)
        {
            if (node == null || node.nodes == null || node.rootIndex < 0 || node.rootIndex >= node.nodes.Count) return null;
            return RestoreNode(node, node.rootIndex, type, existing, select);
        }

        private static object RestoreNode(SnapshotValue snapshot, int index, Type type, object existing, Func<FieldInfo, bool> select = null)
        {
            var node = snapshot.nodes[index];
            if (node.kind == "null") return null;
            var nullable = Nullable.GetUnderlyingType(type);
            if (nullable != null) type = nullable;
            if (node.kind == "scalar")
                return type.IsEnum ? Enum.Parse(type, node.text) : Convert.ChangeType(node.text, type, CultureInfo.InvariantCulture);

            if (type.IsArray)
            {
                var element = type.GetElementType();
                var children = Children(snapshot, index);
                var array = Array.CreateInstance(element, children.Count);
                for (var i = 0; i < children.Count; i++) array.SetValue(RestoreNode(snapshot, children[i], element, null), i);
                return array;
            }

            if (node.kind == "map")
            {
                var map = (IDictionary)(existing ?? Activator.CreateInstance(type, true));
                map.Clear();
                var args = type.GetGenericArguments();
                var children = Children(snapshot, index);
                for (var i = 0; i + 1 < children.Count; i += 2)
                    map.Add(RestoreNode(snapshot, children[i], args[0], null), RestoreNode(snapshot, children[i + 1], args[1], null));
                return map;
            }

            if (node.kind == "list")
            {
                var sequence = existing ?? Activator.CreateInstance(type, true);
                type.GetMethod("Clear").Invoke(sequence, null);
                var argument = type.GetGenericArguments()[0];
                var add = type.GetMethod("Add");
                foreach (var child in Children(snapshot, index))
                    add.Invoke(sequence, new[] { RestoreNode(snapshot, child, argument, null) });
                return sequence;
            }

            if (existing == null)
            {
                if (type.IsValueType) existing = Activator.CreateInstance(type);
                else
                {
                    // Snapshot types are fixed by the runtime field schema, never by saved input.
#pragma warning disable SYSLIB0050
                    existing = FormatterServices.GetUninitializedObject(type);
#pragma warning restore SYSLIB0050
                }
            }

            foreach (var childIndex in Children(snapshot, index))
            {
                var child = snapshot.nodes[childIndex];
                var field = type.GetField(child.name, Fields);
                // Selection applies to the owning object only, matching Capture.
                if (field == null || !CanStore(field.FieldType) || (select != null && !select(field))) continue;
                field.SetValue(existing, RestoreNode(snapshot, childIndex, field.FieldType, field.GetValue(existing)));
            }
            return existing;
        }

        private static List<int> Children(SnapshotValue snapshot, int parentIndex) =>
            snapshot.nodes[parentIndex].children;
    }
}
