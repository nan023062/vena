#if UNITY_EDITOR
#define DEBUG_WORLD
#endif
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Vena
{
    public static class Extensions
    {
        static readonly StringBuilder sb = new StringBuilder();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Destroy(this Actor actor)
        {
            var world = actor.world;

            world.DestroyActor(actor.Id);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Destroy(this InstanceID instanceID)
        {
            var world = World.Get((short)instanceID.world);

            world?.DestroyActor(instanceID.id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Destroy(this Component component)
        {
            var actor = component.actor;

            if (null == actor)
            {
                throw new InvalidOperationException("Component is already destroyed.");
            }

            ((IActor)actor).InternalRemoveComponent(component);
        }

        public static string GetTypeName(this Type type)
        {
            if (!type.IsGenericType)
            {
                return type.Name;
            }

            sb.Clear();

            string name = type.GetGenericTypeDefinition().FullName;
            if (string.IsNullOrEmpty(name)) return type.Name;
            int nameSpaceIndex = name.LastIndexOf('.');
            if (nameSpaceIndex > 0)
                name = name.Substring(nameSpaceIndex + 1);

            Type[] genericArguments = type.GetGenericArguments();
            sb.Append($"{name}<{string.Join(",", genericArguments.Select(t => t.Name))}>");
            return sb.ToString();
        }
    }
}