using System.Collections.Generic;
using UnityEngine;

namespace Vena.UnityExtensions
{
    public static class GameObjectExtension
    {
        public static T EnsureComponent<T>(this GameObject gameObject) where T : UnityEngine.Component
        {
            T component = gameObject.GetComponent<T>();
            if (null == component) component = gameObject.AddComponent<T>();
            return component;
        }
        
        public static List<GameObject> FindAllChild(this GameObject gameObject, string name)
        {
            return null;
        }
    }
}

