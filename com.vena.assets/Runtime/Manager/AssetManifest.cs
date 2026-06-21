// -----------------------------------------------------------------------------
// Vena Assets
// Unity asset pipeline toolkit (AssetBundle / VersionControl / OSS / Compression).
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;

namespace Vena.Assets
{
    public sealed class AssetManifest 
    {
        public readonly string name;
        private readonly Dictionary<string, string> _bundleNameMap;
        private readonly Dictionary<string, string[]> _bundleDependencies;
        private readonly Dictionary<string, string[]> _bundleRelationes;
        
#if UNITY_EDITOR
        private readonly Dictionary<string, string> _assetNameToPaths;
#endif

        public AssetManifest(string packageName, AssetBundleManifest manifest, AssetList mapping)
        {
            name = packageName;
            _bundleDependencies = new Dictionary<string, string[]>();
            _bundleRelationes = new Dictionary<string, string[]>();
            _bundleNameMap = new Dictionary<string, string>();
            
            //初始化依赖映射
            if (manifest != null)
            {
                string[] allBundleNames = manifest.GetAllAssetBundles();
                foreach (string bundle in allBundleNames)
                {
                    string[] dependentArray = manifest.GetAllDependencies(bundle);

                    string bundleName = bundle;
                    int indexof = bundle.IndexOf('.');
                    if (indexof > 0) bundleName = bundleName.Substring(0, indexof);
                    string[] dependenceNames = new string[dependentArray.Length];
                    _bundleDependencies.Add(bundleName, dependenceNames);

                    for (int i = 0; i < dependentArray.Length; i++)
                    {
                        string dependentBundle = dependentArray[i];
                        indexof = dependentBundle.IndexOf('.');
                        if (indexof > 0) dependentBundle = dependentBundle.Substring(0, indexof);
                        dependenceNames[i] = dependentBundle;
                    }
                }

                //初始化asset 和 bundle 的映射关系
                if (mapping != null)
                {
                    foreach (var context in mapping.contexts)
                    {
                        _bundleNameMap.Add(context.assetName, context.bundleName);
                    }
                }
            }
            else
            {
#if UNITY_EDITOR
                _assetNameToPaths = AssetBuildTree.GetPackageAssetNameToPaths(name);
#endif
            }
        }

        public void Dispose()
        {
            _bundleDependencies.Clear();
            _bundleRelationes.Clear();
            _bundleNameMap.Clear();

#if UNITY_EDITOR
            _assetNameToPaths.Clear();
#endif
        }

        public string GetBundleName(string assetName)
        {
            _bundleNameMap.TryGetValue(assetName, out string bundleName);
            return bundleName;
        }

        public string[] GetDependencies(string bundleName)
        {
            _bundleDependencies.TryGetValue(bundleName, out string[] dependentArray);
            return dependentArray;
        }

        public void GetDependenciesRecursively(string bundleName, int maxDepth, ref List<string> dependencyList)
        {
            if (maxDepth <= 0)
            {
                Debug.LogError("GetDependenciesRecursively() 递归深度超限！");
                return;
            }
            foreach (var dependentName in GetDependencies(bundleName))
            {
                if (!dependencyList.Contains(dependentName))
                {
                    dependencyList.Add(dependentName);
                    GetDependenciesRecursively(dependentName, maxDepth - 1, ref dependencyList);
                }
            }
        }

#if UNITY_EDITOR
        public string AssetNameToPath(string assetName)
        {
            _assetNameToPaths.TryGetValue(assetName, out string assetPath);
            return assetPath;
        }
#endif
    }
}
