// -----------------------------------------------------------------------------
// Vena Framework
// Unity game framework layer built on Vena Core.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Vena.UnityExtensions
{
    [Serializable]
    public sealed class ComponentViewer : MonoBehaviour
    {
        [ReadOnly] public UnityEngine.Component component;
                
        [ReadOnly] public string type;

        [ReadOnly] public int order;
    }
        
#if UNITY_EDITOR 
    [CustomEditor(typeof(ComponentViewer))]
    public class ComponentViewInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
#endif
}
