// -----------------------------------------------------------------------------
// Vena Assets
// Unity asset pipeline toolkit (AssetBundle / VersionControl / OSS / Compression).
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

//****************************************************************************
// File: HorizontalButtonList.cs
// Author: Li Nan
// Date: 2022-11-26 12:00
// Version: 1.0
//****************************************************************************

using System;
using UnityEditor;
using UnityEngine;

namespace Vena.Assets
{
    public class HorizontalButtonList
    {
        private HorizontalButton[] _buttonList;
        
        private readonly int width, height, delta;
        
        public HorizontalButtonList(int width, int height, int delta, params HorizontalButton[] buttons)
        {
            this.width = width;
            
            this.height = height;
            
            this.delta = delta;
            
            _buttonList = buttons;
        }

        public void OnDrawGUI()
        {
            int count = _buttonList.Length;
            if (count > 0)
            {
                int w = width * count + (count - 1) * delta;
                
                EditorGUILayout.BeginHorizontal();
                
                for (int i = 0; i < count; i++)
                {
                    ref var button = ref _buttonList[i];
                    
                    if (GUILayout.Button(button.name, GUILayout.Height(height), GUILayout.Width(width)))
                    {
                        button.onClicked();
                    }
                }
                
                EditorGUILayout.EndHorizontal();
            }
        }
    }
    
    public struct HorizontalButton
    {
        public string name;
        
        public Action onClicked;

        public HorizontalButton(string name, Action onClicked)
        {
            this.name = name;
            
            this.onClicked = onClicked;
        }
    }
}