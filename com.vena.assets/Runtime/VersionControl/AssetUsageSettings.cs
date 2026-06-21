// -----------------------------------------------------------------------------
// Vena Assets
// Unity asset pipeline toolkit (AssetBundle / VersionControl / OSS / Compression).
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Vena.Assets
{
    public class AssetUsageSetting : ScriptableObject
    {
        /// <summary>
        /// 服务器资源地址
        /// </summary>
        public string remoteAddress;

        private static AssetUsageSetting _instance;

        public static AssetUsageSetting Instance
        {
            get
            {
                _instance ??= Resources.Load<AssetUsageSetting>("AssetUsageSetting");
#if UNITY_EDITOR     
                if (_instance == null) CreateAssetProductSettings();
#endif
                return _instance;
            }
        }

        public const string SettingsPath = "Assets/Resources/AssetUsageSetting.asset";

#if UNITY_EDITOR
        [MenuItem("Assets/Create/AssetUsageSetting")]
        private static void CreateAssetProductSettings()
        {
            _instance = CreateInstance<AssetUsageSetting>();
            
            AssetDatabase.CreateAsset(_instance, SettingsPath);
            
            AssetDatabase.SaveAssets();
            
            EditorUtility.FocusProjectWindow();
            
            Selection.activeObject = _instance;
        }

        public static void Save()
        {
            if(null == _instance) return;
            
            EditorUtility.SetDirty(_instance);
            
            AssetDatabase.SaveAssets();
            
            AssetDatabase.Refresh();
            
            Debug.Log("AssetUsageSettings Saved ...");
        }
#endif
    }
}
