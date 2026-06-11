// -----------------------------------------------------------------------------
// Vena Framework
// Unity game framework layer built on Vena Core.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;

namespace Vena
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

