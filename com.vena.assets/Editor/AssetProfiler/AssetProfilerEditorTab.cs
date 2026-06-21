// -----------------------------------------------------------------------------
// Vena Assets
// Unity asset pipeline toolkit (AssetBundle / VersionControl / OSS / Compression).
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using UnityEngine;
using UnityEditor;

namespace Vena.Assets
{
    public sealed class AssetProfilerEditorTab : AssetToolkitTab
    {
        public override string TabName => "Asset Profiler";
        
        private void Awake()
        {
        }

        protected override void OnEnterDraw()
        {
            _objectRef = null;
            _selectedLoader = null;
            _toggleReference = true;
        }

        protected override void OnExitDraw()
        {
        }

        protected override void OnDrawGUI()
        {
            if (!Application.isPlaying)
            {
                CommonGUI.SeparatorLine(" no profiler info ...");
                return;
            }
            
            CommonGUI.SeparatorLine(" profiler info : ");
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            
            EditorGUILayout.BeginHorizontal();
            
            // ResourceLoader List
            EditorGUILayout.BeginVertical("box", GUILayout.Width(200));
            DrawResourceList();
            EditorGUILayout.EndVertical();
            
            // Handle List
            EditorGUILayout.BeginVertical("box", GUILayout.Width(300));
            DrawResourceHandleList();
            EditorGUILayout.EndVertical();
            
            // Select ObjectRef
            EditorGUILayout.BeginVertical("box");
            DrawSelectedObjectRef();
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.EndHorizontal();
        }

        private IResLoader _selectedLoader;
        private Package.ObjectRef _objectRef;
        private bool _toggleReference;
        
        private void DrawResourceList()
        {
           IResLoader[] loaders = ResourceRuntime.GetAllLoaders();
           GUILayout.Button($"Loader List\ntotal count = {loaders.Length}", GUILayout.Height(50));
           EditorGUILayout.Space();
           
           EditorGUILayout.BeginVertical();
           foreach (var loader in loaders)
           {
               if (GUILayout.Button(loader.ToString())) 
                   _selectedLoader = loader;
           }
           EditorGUILayout.EndVertical();
        }
        
        private void DrawResourceHandleList()
        {
            if (_selectedLoader != null)
            {
                GUILayout.Button($"Loader : {_selectedLoader}\nresource handles", GUILayout.Height(50));
                EditorGUILayout.Space();
                EditorGUILayout.Space();
                
                EditorGUILayout.BeginVertical();
                
                foreach (Handle handle in _selectedLoader.GetAllResourceHandles())
                {
                    if (GUILayout.Button(handle.ToString())) 
                        _objectRef = handle.GetObjectRef();
                }
                EditorGUILayout.EndVertical();
            }
            else
            {
                GUILayout.Button("No Loader ...", GUILayout.Height(50));
            }
        }

        private void DrawSelectedObjectRef()
        {
            if (_objectRef == null)
            {
                GUILayout.Button("No Resource ... ", GUILayout.Height(50));
            }
            else
            {
                GUILayout.Button(_objectRef.ToString(), GUILayout.Height(50));
                EditorGUILayout.Space();

                ulong[] refList = _objectRef.GetReferences();
                ulong[] depList = _objectRef.GetDependencies();
                
                EditorGUILayout.BeginHorizontal();
                _toggleReference = GUILayout.Toggle(_toggleReference, $"References - {refList.Length}", EditorStyles.toolbarButton);
                _toggleReference = !GUILayout.Toggle(!_toggleReference,$"Dependencies - {depList.Length}", EditorStyles.toolbarButton);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();
                EditorGUILayout.Space();
                
                // refList
                ulong[] objectList = _toggleReference ? refList : depList;
                EditorGUILayout.BeginVertical();
                foreach (ulong objectId in objectList)
                {
                    var objectRef = ResourceRuntime.GetObjectRef(_objectRef.package.id, objectId);
                    if (GUILayout.Button(objectRef.ToString()))
                    {
                        _objectRef = objectRef;
                    }
                }
                EditorGUILayout.EndVertical();
            }
        }
    }
}
