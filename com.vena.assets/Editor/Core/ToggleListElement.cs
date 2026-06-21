// -----------------------------------------------------------------------------
// Vena Assets
// Unity asset pipeline toolkit (AssetBundle / VersionControl / OSS / Compression).
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

//****************************************************************************
// File: CommonButton.cs
// Author: Li Nan
// Date: 2022-11-26 12:00
// Version: 1.0
//****************************************************************************

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Vena.Assets
{
    public class ToggleList<T> where T : ToggleListButton
    {
        private T[] _toggleList;
        
        private bool[] _selectedList;

        private HorizontalButtonList _horizontalButtonList;
        
        private readonly int width, height;
        
        public ToggleList(T[] toggleListButtons, int width, int height)
        {
            this.width = width;
            
            this.height = height;
            
            _toggleList = new T[toggleListButtons.Length];
            
            _selectedList = new bool[toggleListButtons.Length];
            
            for (int i = 0; i < toggleListButtons.Length; i++)
            {
                _selectedList[i] = false;
                
                _toggleList[i] = toggleListButtons[i];
            }

            int count = 4, delta = 5;
            int bw = (width - (count - 1) * delta) / count;
            _horizontalButtonList = new HorizontalButtonList(bw , height, delta,
                new HorizontalButton("Execute All", ExecuteAll),
                new HorizontalButton("Execute Selected",ExecuteSelected),
                new HorizontalButton("Select All",SelectAll),
                new HorizontalButton("Deselect All", DeselectAll));
        }

        public void OnDrawGUI()
        {
            if(_toggleList == null) return;

            EditorGUILayout.BeginVertical("box", GUILayout.Width(width));
            
            _horizontalButtonList.OnDrawGUI();
            
            for (int i = 0; i < _toggleList.Length; i++)
            {
                EditorGUILayout.BeginHorizontal(GUILayout.Height(height));
                
                _selectedList[i] = GUILayout.Toggle(_selectedList[i], GUIContent.none);
                
                _toggleList[i].OnDrawGUI(width-height, height);
                
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndVertical();
        }
        
        public void ExecuteAll()
        {
            foreach (var toggleListButton in _toggleList)
            {
                toggleListButton.OnClicked();
            }
        }
        
        public void ExecuteSelected()
        {
            for (int i = 0; i < _toggleList.Length; i++)
            {
                if (_selectedList[i])
                {
                    _toggleList[i].OnClicked();
                }
            }
        }
        
        public void SelectAll()
        {
            for (int i = 0; i < _toggleList.Length; i++)
            {
                _selectedList[i] = true;
            }
        }
        
        public void DeselectAll()
        {
            for (int i = 0; i < _toggleList.Length; i++)
            {
                _selectedList[i] = false;
            }
        }
    }
    
    public abstract class ToggleListButton
    {
        public abstract string name { get; }
        
        public abstract void OnClicked();
        
        public abstract void OnDrawGUI(int width, int height);
    }

}