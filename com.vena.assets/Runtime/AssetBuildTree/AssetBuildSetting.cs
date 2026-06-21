// -----------------------------------------------------------------------------
// Vena Assets
// Unity asset pipeline toolkit (AssetBundle / VersionControl / OSS / Compression).
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System;
using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Vena.Assets
{
    [Serializable]
    public class AssetBuildSetting : ScriptableObject
    {
        public bool useAssetBundle;
        public string assetRootPath;
        public string bundleOutputPath;
        public string localStorePath;
        
        public string GetAssetRootPath()
        {
            return Utility.UnityRelativePathToAbsolutePath(assetRootPath);
        }

        public string GetBundleOutputPath()
        {
            return Utility.UnityRelativePathToAbsolutePath(bundleOutputPath);
        }
    
        public string GetLocalStorePath()
        {
            return Utility.UnityRelativePathToAbsolutePath(localStorePath);
        }

        public AssetPackageData[] assetPackages;

        public AssetPackageData GetPackageData(string packageName)
        {
            if (assetPackages == null) return null;
            
            foreach (var assetPackage in assetPackages)
            {
                if (assetPackage.packageName == packageName)
                {
                    return assetPackage;
                }
            }
            return null;
        }

        public PathToStrategy GetAssetBundleStrategy(string packageName, string bundlePath)
        {
            var assetPackage = GetPackageData(packageName);
            
            return assetPackage?.GetAssetStrategyData(bundlePath);
        }

        private static AssetBuildSetting _instance;

        public static AssetBuildSetting Instance
        {
            get
            {
                _instance ??= Resources.Load<AssetBuildSetting>("AssetBuildSetting");
#if UNITY_EDITOR
                if (null == _instance) CreateAssetProductSettings();
#endif
                return _instance;
            }
        } 
        
        public const string SettingsPath = "Assets/Resources/AssetBuildSetting.asset";

#if UNITY_EDITOR
        [MenuItem("Assets/Create/AssetBuildSetting")]
        private static void CreateAssetProductSettings()
        {
            _instance = CreateInstance<AssetBuildSetting>();
            
            AssetDatabase.CreateAsset(_instance, SettingsPath);
            
            AssetDatabase.SaveAssets();
            
            EditorUtility.FocusProjectWindow();
            
            Selection.activeObject = _instance;
        }

        public static void Save(bool withTree = false)
        {
            if(withTree)
            {
                var packageDataMap = new Dictionary<string, AssetPackageData>();
                var assetTreeRoot = AssetBuildTree.Root;
                foreach (var assetTreeNode in assetTreeRoot.Children)
                {
                    var packageData = new AssetPackageData(assetTreeNode.PackageName, assetTreeNode.BuildType);
                    packageDataMap.Add(assetTreeNode.PackageName, packageData);

                    var bundleStrategyMapping = new Dictionary<string, PathToStrategy>();
                    if (assetTreeNode.BuildType == BuildPackageType.Bundles)
                        AssetBuildTree.GetBundleStrategyRecursively(assetTreeNode, ref bundleStrategyMapping);
                    packageData.strategyMappings = new PathToStrategy[bundleStrategyMapping.Count];
                    bundleStrategyMapping.Values.CopyTo(packageData.strategyMappings, 0);
                }
                Instance.assetPackages = new AssetPackageData[packageDataMap.Count];
                packageDataMap.Values.CopyTo(Instance.assetPackages, 0);
            }

            EditorUtility.SetDirty(Instance);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("AssetProduceSettings Saved ...");
        }
#endif
    }

    [System.Serializable]
    public class PathToStrategy
    {
        public string bundlePath;
        public StrategyMode mode;

        public PathToStrategy(string bundlePath, StrategyMode mode)
        {
            this.bundlePath = bundlePath;
            this.mode = mode;
        }
    }

    [System.Serializable]
    public class AssetPackageData
    {
        public string packageName;

        public BuildPackageType buildType;

        public PathToStrategy[] strategyMappings;

        public PathToStrategy GetAssetStrategyData(string bundlePath)
        {
            if (strategyMappings == null) return null;
            foreach (var strategyMapping in strategyMappings)
            {
                if (strategyMapping.bundlePath == bundlePath)
                {
                    return strategyMapping;
                }
            }
            return null;
        }

        public AssetPackageData(string packageName, BuildPackageType buildType)
        {
            this.packageName = packageName;
            this.buildType = buildType;
        }
    }

}
