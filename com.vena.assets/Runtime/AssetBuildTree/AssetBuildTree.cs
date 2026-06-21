// -----------------------------------------------------------------------------
// Vena Assets
// Unity asset pipeline toolkit (AssetBundle / VersionControl / OSS / Compression).
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace Vena.Assets
{
    public static class AssetBuildTree
    {
        /// <summary>
        /// 资源树的跟节点
        /// </summary>
        private static BuildTreeNode _buildTreeRoot;
        
        private static AssetBuildSetting _currAssetPackageSetting;
        
        public static BuildTreeNode Root => _buildTreeRoot ??= RefreshAssetBuildTree();

        public static BuildTreeNode RefreshAssetBuildTree(bool showProgress = false)
        {
            _buildTreeRoot = null;
            
            _currAssetPackageSetting = AssetBuildSetting.Instance;
                    
            if (_currAssetPackageSetting == null)
            {
                Debug.LogError("没有发现策略文件！");
            }
                    
            string gameAssetDirPath = Utility.GameAssetInputPath();
                    
            if (!Directory.Exists(gameAssetDirPath)) return _buildTreeRoot;

            //创建资源树根节点
            var directoryInfo = new DirectoryInfo(gameAssetDirPath);
                    
            _buildTreeRoot = new BuildTreeNode(directoryInfo, null, string.Empty);
                    
            _buildTreeRoot.SetBundleStrategy(StrategyMode.NoBuild, false);
                    
            //设置一级目录（分包目录）
            foreach (var directory in directoryInfo.GetDirectories())
            {
                string packageName = directory.Name;
                        
                var assetTreeNode = new BuildTreeNode(directory, _buildTreeRoot, packageName);
                        
                assetTreeNode.SetBundleStrategy(StrategyMode.OneFileGenMapping, true);
                        
                var packageDate = _currAssetPackageSetting.GetPackageData(packageName);
                        
                assetTreeNode.BuildType = packageDate?.buildType ?? BuildPackageType.Bundles;
                        
                GenerateBuildNodesRecursively(assetTreeNode);
            }

            return _buildTreeRoot;
        }
        
        /// <summary>
        /// 递归获取生成子文件夹策略节点
        /// </summary>
        private static void GenerateBuildNodesRecursively(BuildTreeNode parentNode)
        {
            foreach (var directoryInfo in parentNode.DirectoryInfo.GetDirectories())
            {
                BuildTreeNode treeNode = CreateNewBuildTreeNode(directoryInfo, parentNode);
                
                GenerateBuildNodesRecursively(treeNode);
            }
        }

        /// <summary>
        /// 创建一个新的 资源节点（递归子文件夹）
        /// </summary>
        public static BuildTreeNode CreateNewBundleTreeNode(string fullPackagePath)
        {
            BuildTreeNode newBundleTreeNode = FindBuildTreeNodeByPath(Root, fullPackagePath);

            //确定不存在
            if (newBundleTreeNode == null)
            {
                BuildTreeNode parentNode = Root;
                
                int indexOf = fullPackagePath.LastIndexOf('/');
                
                //如果是子目录
                if (indexOf > 0)
                {
                    string parenPackagePath = fullPackagePath.Substring(0, indexOf);
                    
                    parentNode = FindBuildTreeNodeByPath(Root, parenPackagePath);
                    
                    if (parentNode == null)
                    {
                        Debug.LogError($"在BundleTree中没有找到<{fullPackagePath}>的父节点 ! ");
                        
                        return null; 
                    }
                }

                foreach (var directoryInfo in parentNode.DirectoryInfo.GetDirectories())
                {
                    string fullName = directoryInfo.FullName.Replace('\\', '/');
                    
                    if (fullName.EndsWith(fullPackagePath))
                    {
                        newBundleTreeNode = CreateNewBuildTreeNode(directoryInfo, parentNode);
                        
                        GenerateBuildNodesRecursively(newBundleTreeNode);
                    }
                }
                return newBundleTreeNode;
            }
            else
            {
                Debug.LogFormat("Bundle TreeNode 已经存在了 {0} ！", fullPackagePath);
                
                return null;
            }
        }

        /// <summary>
        /// 创建一个节点，由策略文件初始化
        /// </summary>
        public static BuildTreeNode CreateNewBuildTreeNode(DirectoryInfo directory, BuildTreeNode parentNode)
        {
            string packageName = parentNode.PackageName;
            
            if (string.IsNullOrEmpty(packageName)) packageName = directory.Name;
            
            var newTreeNode = new BuildTreeNode(directory, parentNode, packageName);

            //已经设置了的策略，直接读取缓存数据
            var strategyData = _currAssetPackageSetting.GetAssetBundleStrategy(packageName, newTreeNode.PackagePath);
            
            if (strategyData != null) newTreeNode.SetBundleStrategy(strategyData.mode, false);
            
            return newTreeNode;
        }

        public static bool DeleteOneBuildTreeNode(string fullPackagePath)
        {
            var bundleTreeNode = FindBuildTreeNodeByPath(Root, fullPackagePath);
            
            return DeleteOneBundleTreeNode(bundleTreeNode);
        }
        
        public static bool DeleteOneBundleTreeNode(BuildTreeNode assetTreeNode)
        {
            if (assetTreeNode == null) return false;

            if (assetTreeNode.Parent != null)
            {
                assetTreeNode.Parent.Children.Remove(assetTreeNode);
            }
            return true;
        }

        public static void GetBundleStrategyRecursively(BuildTreeNode assetTreeNode,
            ref Dictionary<string, PathToStrategy> bundleStrategyMapping)
        {
            var strategyData = new PathToStrategy(assetTreeNode.PackagePath, assetTreeNode.Strategy);
            
            bundleStrategyMapping.Add(assetTreeNode.PackagePath, strategyData);
            
            foreach (var childBundleNode in assetTreeNode.Children)
            {
                GetBundleStrategyRecursively(childBundleNode, ref bundleStrategyMapping);
            }
        }

        /// <summary>
        /// 根据相对路径查找BundleNode
        /// </summary>
        public static BuildTreeNode FindBuildTreeNodeByPath(BuildTreeNode assetTreeNode, string fullPackagePath)
        {
            if (assetTreeNode.PackagePath == fullPackagePath)
            {
                return assetTreeNode;
            }

            foreach (var treeNode in assetTreeNode.Children)
            {
                var result = FindBuildTreeNodeByPath(treeNode, fullPackagePath);
                
                if (result != null) return result;
            }

            return null;
        }

        public static BuildTreeNode GetOrCreateBuildNode(string inPackagePath)
        {
            BuildTreeNode buildRootNode = Root;
            
            var buildNode = FindBuildTreeNodeByPath(buildRootNode, inPackagePath);
            
            if (buildNode == null)
            {
                BuildTreeNode parentNode = buildRootNode;
                
                int indexOf = inPackagePath.LastIndexOf('/');
                
                if (indexOf > 0)
                {
                    string parenPackagePath = inPackagePath.Substring(0, indexOf);
                    
                    parentNode = FindBuildTreeNodeByPath(buildRootNode, parenPackagePath);
                    
                    if (parentNode == null)
                    {
                        Debug.LogError($"在BundleTree中没有找到<{inPackagePath}>的父节点 ! ");
                        
                        return null;
                    }
                }

                foreach (var directory in parentNode.DirectoryInfo.GetDirectories())
                {
                    string fullName = directory.FullName.Replace('\\', '/');
                    
                    if (fullName.EndsWith(inPackagePath))
                    {
                        buildNode = CreateNewBuildTreeNode(directory, parentNode);
                        
                        break;
                    }
                }
            }

            return buildNode;
        }

        /// <summary>
        /// 获取知道Package包的asset 与 bundle的映射
        /// </summary>
        public static Dictionary<string, string> GetPackageAssetNameToPaths(string packageName)
        {
            var assetNameToPaths = new Dictionary<string, string>();

            foreach (var treeNode in Root.Children)
            {
                if (treeNode.FolderName == packageName)
                {
                    treeNode.GetAssetToAssetPathRecursively(ref assetNameToPaths);
                    break;
                }
            }
            return assetNameToPaths;
        }
    }
}