// -----------------------------------------------------------------------------
// Vena Assets
// Unity asset pipeline toolkit (AssetBundle / VersionControl / OSS / Compression).
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

//****************************************************************************
// File: AssetMenuEvent.cs
// Author: Li Nan
// Date: 2022-11-26 12:00
// Version: 1.0
//****************************************************************************
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Vena.Assets
{
    public abstract class AssetProcessCommand : ToggleListButton
    {
        private string _name;

        private bool _isShowProgressBar;
        
        public override string name
        {
            get
            {
                if (string.IsNullOrEmpty(_name))
                {
                    var attribute = GetType().GetCustomAttribute<ProcessorNameAttribute>();
                    _name = attribute?.name ?? GetType().Name;
                }

                return _name;
            }
        }

        public override void OnClicked()
        {
            OnExecute(out string error);
        }

        protected abstract bool OnExecute(out string error);
        
        
        public override void OnDrawGUI(int width, int height)
        {
            if (GUILayout.Button(name, GUILayout.Width(width), GUILayout.Height(height)))
            {
                _isShowProgressBar = true;
                
                EditorUtility.DisplayProgressBar(name, " Start ...", 0);
                if (OnExecute(out var error))
                {
                    EditorUtility.ClearProgressBar();
                    EditorUtility.DisplayDialog(name, $" Success ！", "OK");
                }
                else
                {
                    EditorUtility.ClearProgressBar();
                    EditorUtility.DisplayDialog(name, $" Failed ！\nError：\n{error}", "OK");
                }

                _isShowProgressBar = false;
            }
        }
        
        /// <summary>
        /// 显示进度条
        /// </summary>
        protected void DisplayProgressBar(string content, float progress)
        {
            if (_isShowProgressBar)
            {
                EditorUtility.DisplayProgressBar(name, content, progress);
            }
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class ProcessorNameAttribute : Attribute
    {
        public readonly string name;

        public ProcessorNameAttribute(string name)
        {
            this.name = name;
        }
    }

    public abstract class AssetProcessCommand<TSource, TDestination> : AssetProcessCommand 
        where TSource : class where TDestination : class
    {
        protected sealed override bool OnExecute(out string error)
        {
            try
            {
                TSource[] sources = PrepareSources();
                
                TDestination[] destinations = ProcessSources(sources);
                
                AfterProcess(destinations);

                error = null;
                return true;
            }
            catch (Exception e)
            {
                error = $"error={e.Message}!! \nstack={e.StackTrace}";
                return false;
            }
        }
            
        protected abstract TSource[] PrepareSources();
        
        protected abstract TDestination[] ProcessSources(TSource[] sources);
        
        protected abstract void AfterProcess(TDestination[] destinations);
    }
    
    [ProcessorName("测试处理1")]
    public class TestCommand : AssetProcessCommand
    {
        protected sealed override bool OnExecute(out string error)
        {
            int count = 9999;
            for (int i = 0; i < count; i++)
            {
                Debug.Log($"====={name}============={(i + 1)}/{count} !!!");
                DisplayProgressBar($"测试处理中...{(i + 1)}/{count}", ((i + 1f) / count));
            }
            
            error = null;
            return true;
        }
    }
    
    [ProcessorName("测试处理2")]
    public class TestCommand2 : AssetProcessCommand
    {
        protected sealed override bool OnExecute(out string error)
        {
            int count = 99;
            for (int i = 0; i < count; i++)
            {
                Debug.Log($"====={name}============={(i + 1)}/{count} !!!");
                DisplayProgressBar($"测试处理中...{(i + 1)}/{count}", ((i + 1f) / count));
            }
            
            error = " TestCommand2.Execute failed !";
            return false;
        }
    }
}