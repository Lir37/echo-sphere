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
        public string name, kind, text;
        public List<SnapshotValue> items = new List<SnapshotValue>();
    }

    // JsonUtility-compatible representation of the source's JSON run snapshot.
    // Only declared value fields and collections are stored. Engine object references,
    // identity-based hit sets and renderer resources are intentionally excluded.
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
            var node = new SnapshotValue();
            if (value == null) { node.kind = "null"; return node; }
            var type = value.GetType();
            if (type == typeof(string) || type.IsPrimitive || type.IsEnum)
            {
                node.kind = "scalar";
                node.text = value is float f ? f.ToString("R", CultureInfo.InvariantCulture) : value is double d ? d.ToString("R", CultureInfo.InvariantCulture) : Convert.ToString(value, CultureInfo.InvariantCulture);
                return node;
            }
            if (value is IDictionary map)
            {
                node.kind = "map";
                foreach (DictionaryEntry entry in map)
                { node.items.Add(Capture(entry.Key)); node.items.Add(Capture(entry.Value)); }
                return node;
            }
            if (value is IEnumerable sequence)
            {
                node.kind = "list";
                foreach (var item in sequence) node.items.Add(Capture(item));
                return node;
            }
            node.kind = "object";
            foreach (var field in type.GetFields(Fields))
            {
                if (field.IsStatic || !CanStore(field.FieldType) || (select != null && !select(field))) continue;
                var item = Capture(field.GetValue(value)); item.name = field.Name; node.items.Add(item);
            }
            return node;
        }
        public static SnapshotValue Field(SnapshotValue node, string name) => node?.items?.Find(x => x.name == name);
        public static T Read<T>(SnapshotValue node, string field, T fallback = default)
        {
            var value = Field(node, field);
            return value == null ? fallback : (T)Restore(value, typeof(T));
        }
        public static object Restore(SnapshotValue node, Type type, object existing = null)
        {
            if (node == null || node.kind == "null") return null;
            var nullable = Nullable.GetUnderlyingType(type);
            if (nullable != null) type = nullable;
            if (node.kind == "scalar") return type.IsEnum ? Enum.Parse(type, node.text) : Convert.ChangeType(node.text, type, CultureInfo.InvariantCulture);
            if (type.IsArray)
            {
                var element = type.GetElementType(); var array = Array.CreateInstance(element, node.items.Count);
                for (var i = 0; i < node.items.Count; i++) array.SetValue(Restore(node.items[i], element), i);
                return array;
            }
            if (node.kind == "map")
            {
                var map = (IDictionary)(existing ?? Activator.CreateInstance(type, true)); map.Clear();
                var args = type.GetGenericArguments();
                for (var i = 0; i < node.items.Count; i += 2) map.Add(Restore(node.items[i], args[0]), Restore(node.items[i + 1], args[1]));
                return map;
            }
            if (node.kind == "list")
            {
                var sequence = existing ?? Activator.CreateInstance(type, true);
                type.GetMethod("Clear").Invoke(sequence, null);
                var argument = type.GetGenericArguments()[0]; var add = type.GetMethod("Add");
                foreach (var item in node.items) add.Invoke(sequence, new[] { Restore(item, argument) });
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
            foreach (var item in node.items)
            {
                var field = type.GetField(item.name, Fields);
                if (field == null || !CanStore(field.FieldType)) continue;
                field.SetValue(existing, Restore(item, field.FieldType, field.GetValue(existing)));
            }
            return existing;
        }
    }
}
