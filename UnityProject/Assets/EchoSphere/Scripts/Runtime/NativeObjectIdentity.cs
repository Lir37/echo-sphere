namespace EchoSphere
{
    internal static class NativeObjectIdentity
    {
        // Keep existing integer identity contracts and save representation while
        // using Unity 6.6's EntityId API. This matches that Editor's legacy public
        // GetInstanceID conversion; it does not change entity selection or RNG.
        internal static int ExistingObjectId(this UnityEngine.Object value)=>unchecked((int)(UnityEngine.EntityId.ToULong(value.GetEntityId())&uint.MaxValue));
        internal static T FirstInExistingOrder<T>() where T:UnityEngine.Object
        {
            var objects=UnityEngine.Object.FindObjectsByType<T>();if(objects.Length==0)return null;
            var first=objects[0];for(int i=1;i<objects.Length;i++)if(objects[i].ExistingObjectId()<first.ExistingObjectId())first=objects[i];return first;
        }
    }
}
