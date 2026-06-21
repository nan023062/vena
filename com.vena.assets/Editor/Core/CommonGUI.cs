// -----------------------------------------------------------------------------
// Vena Assets
// Unity asset pipeline toolkit (AssetBundle / VersionControl / OSS / Compression).
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System;
using UnityEngine;
using UnityEditor;

namespace Vena.Assets
{
    public static class CommonGUI
    {
        private static GUIStyle _boldLabelStyle = null;
        
        public static void SeparatorLine(string title, int space = 5)
        {
            EditorGUILayout.Separator();
            if (!string.IsNullOrEmpty(title))
            {
                _boldLabelStyle ??= new GUIStyle(EditorStyles.boldLabel);
                EditorGUILayout.LabelField(title, _boldLabelStyle);
            }
            EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 1), EditorStyles.label.normal.textColor);
            GUILayout.Space(space);
        }
    }
}
