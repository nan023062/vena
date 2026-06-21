// -----------------------------------------------------------------------------
// Vena Assets
// Unity asset pipeline toolkit (AssetBundle / VersionControl / OSS / Compression).
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System;
using UnityEngine;
using System.IO;
using UnityEditor;
using System.Collections.Generic;
using UObj = UnityEngine.Object;
using System.Linq;

namespace Vena.Assets
{
    /// <summary>
    /// 资源引用关系查询工具
    /// </summary>
    public static class PackageDependenciesTool
    {
        /// <summary>
        /// 收集当前选中文件所在Package的引用关系信心
        /// </summary>
        public static PackageReferenceInfo CollectPackageDependencies(string packageName)
        {
            var packageTreeNode = AssetBuildTree.FindBuildTreeNodeByPath(AssetBuildTree.Root, packageName);
            if (packageTreeNode == null)
            {
                Debug.LogError($"没有找到{packageName}的Package包！");
                return null;
            }
            var reference = new PackageReferenceInfo(packageTreeNode);
            CollectPackageDependencies(ref reference, packageTreeNode);
            return reference;
        }
        
        private static void CollectPackageDependencies(ref PackageReferenceInfo reference, BuildTreeNode buildNode)
        {
            foreach (var fileInfo in buildNode.DirectoryInfo.GetFiles())
            {
                try
                {
                    if (Utility.IgnoreSuffix.Contains(fileInfo.Extension)) continue;
                    string assetPath = fileInfo.FullName.Substring(fileInfo.FullName.IndexOf("Assets", StringComparison.Ordinal));
                    assetPath = assetPath.Replace('\\', '/');
                    if (!reference.allAssets.TryGetValue(assetPath, out UObj asset))
                    {
                        asset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(UObj));
                        reference.allAssets.Add(assetPath, asset);
                    }

                    string[] dependencies = AssetDatabase.GetDependencies(assetPath);
                    foreach (var dpAssetPath in dependencies)
                    {
                        string extension = Path.GetExtension(dpAssetPath);
                        if (Utility.IgnoreSuffix.Contains(extension)) continue;
                        var dpAsset = AssetDatabase.LoadAssetAtPath(dpAssetPath, typeof(UObj));
                        if (dpAsset == asset) continue;
                        if (dpAsset != null)
                        {
                            reference.AddDependence(assetPath, dpAsset);
                            reference.AddReference(dpAssetPath, asset);
                        }
                        else
                        {
                            Debug.LogError($"LoadAssetAtPath Failed, path = {dpAssetPath} !");
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"error={ex.Message}, stack={ex.StackTrace}!!");
                }
            }
            
            if (buildNode.Children != null && buildNode.Children.Count > 0)
            {
                foreach (var childNode in buildNode.Children)
                {
                    CollectPackageDependencies(ref reference, childNode);
                }
            }
        }

        /// <summary>
        /// 收集代码加载资源清单
        /// </summary>
        public static void CollectScriptLoadAssets()
        {
            ScriptReferenceInfo refs = new ScriptReferenceInfo();
            
        }
    }
    
    public sealed class PackageReferenceInfo
    {
        public readonly BuildTreeNode package;

        public readonly Dictionary<string, UObj> allAssets;

        public readonly Dictionary<string, List<UObj>> dependences;

        public readonly Dictionary<string, List<UObj>> references;
        
        public void AddDependence(string target, UObj dpAssset)
        {
            List<UObj> assetList = null;
            if (!dependences.TryGetValue(target, out assetList))
            {
                assetList = new List<UObj>();
                dependences.Add(target, assetList);
            }
            assetList.Add(dpAssset);
        }

        public void AddReference(string target, UObj refAsset)
        {
            if (!references.TryGetValue(target, out List<UObj> assetList))
            {
                assetList = new List<UObj>();
                references.Add(target, assetList);
            }
            assetList.Add(refAsset);
        }

        public PackageReferenceInfo(BuildTreeNode package)
        {
            this.package = package;
            allAssets = new Dictionary<string, UObj>();
            dependences = new Dictionary<string, List<UObj>>();
            references = new Dictionary<string, List<UObj>>();
        }
    }

    public sealed class ScriptReferenceInfo
    {
        public readonly Dictionary<string, List<string>> CSharpRefs;
        public readonly Dictionary<string, List<string>> LuaScriptRefs;
        public readonly Dictionary<string, List<string>> ConfigRefs;

        public ScriptReferenceInfo()
        {
            CSharpRefs = new Dictionary<string, List<string>>();
            LuaScriptRefs = new Dictionary<string, List<string>>();
            ConfigRefs = new Dictionary<string, List<string>>();
        }
    }
}