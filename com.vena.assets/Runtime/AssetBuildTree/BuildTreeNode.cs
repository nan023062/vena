// -----------------------------------------------------------------------------
// Vena Assets
// Unity asset pipeline toolkit (AssetBundle / VersionControl / OSS / Compression).
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Vena.Assets
{
    public class BuildTreeNode
    {
        const string TemplateName = "00Template";
        
        /// <summary>
        /// 对应文件夹名称
        /// </summary>
        public readonly string FolderName;
        
        /// <summary>
        /// 对应的Bundle Package包名称
        /// </summary>
        public readonly string PackageName;

        /// <summary>
        /// 相对于Package的路径
        /// </summary>
        public readonly string RelativePath;

        /// <summary>
        /// Package的路径
        /// </summary>
        public readonly string PackagePath;

        /// <summary>
        /// 相对应的文件夹目录对象
        /// </summary>
        public readonly DirectoryInfo DirectoryInfo;

        /// <summary>
        /// 文件夹资源
        /// </summary>
        public readonly UnityEngine.Object asset;
        
        /// <summary>
        /// 在Bundle树中的父节点
        /// </summary>
        public readonly BuildTreeNode Parent = null;

        /// <summary>
        /// 在Bundle树中的子节点
        /// </summary>
        public readonly List<BuildTreeNode> Children;
        
        /// <summary>
        /// AB策略类型
        /// </summary>
        public StrategyMode Strategy { get; private set; }

        /// <summary>
        /// 资源包类型
        /// </summary>
        public BuildPackageType BuildType;

        /// <summary>
        /// 构造一个Bundle树节点
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="parent"></param>
        /// <param name="packageName"></param>
        public BuildTreeNode(DirectoryInfo directory, BuildTreeNode parent, string packageName)
        {
            FolderName = directory.Name;
            DirectoryInfo = directory;
            Strategy = StrategyMode.Null;
            PackageName = packageName;
            Parent = parent;
            Children = new List<BuildTreeNode>();
            
            if (!string.IsNullOrEmpty(packageName))
            {
                RelativePath = Utility.GetRelativePath(directory.FullName, PackageName);
                RelativePath = RelativePath.Replace('\\', '/');
                PackagePath = Path.Combine(PackageName, RelativePath);
                PackagePath = PackagePath.Replace('\\', '/');
            }
            
#if UNITY_EDITOR
            if (PackagePath != null)
            {
                string assetPath = Path.Combine(Utility.AssetGameAssets, PackagePath);
                asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            }
#endif
            parent?.Children.Add(this);
        }

        #region 设置AB策略相关

        /// <summary>
        /// 设置树节点的打包策略 可选递归子节点
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="recursively"></param>
        public void SetBundleStrategy(StrategyMode mode, bool recursively)
        {
            if (mode == StrategyMode.Template)
            {
                SetTemplateStrategy();
            }
            else
            {
                SetSingleStrategyRecursively(mode, recursively);
            }
        }

        private BuildTreeNode FindTemplateNode()
        {
            foreach (var childNode in Parent.Children)
            {
                if (childNode.FolderName.Equals(TemplateName))
                {
                    return childNode;
                }
            }
            
            Debug.LogError($"BundleNode = {Parent.PackagePath} FindTemplateNode() 失败 ！！");
            return null;
        }

        /// <summary>
        /// 从模板中克隆策略
        /// </summary>
        public void CloneStrategyFormTemplate()
        {
            if (Parent.Strategy == StrategyMode.Template)
            {
                BuildTreeNode template = Parent.FindTemplateNode();
                if (template != null)
                {
                    CloneStrategyRecursively(template, this);
                }
            }
            else
            {
                Debug.LogError($"BundleNode = {Parent.PackagePath} 的策略类型不是Template，不能调用此函数！！");
            }
        }

        /// <summary>
        /// 设置模板策略
        /// 1 克隆模板策略到所有子文件夹
        /// 2 同时检查 所有子文件夹结构
        /// </summary>
        private void SetTemplateStrategy()
        {
            Strategy = StrategyMode.Template;
            
            BuildTreeNode template = FindTemplateNode();
            if (template != null)
            {
                foreach (var childNode in Children)
                {
                    if (childNode.FolderName.Equals(TemplateName)) continue;
                    CloneStrategyRecursively(template, childNode);
                }
            }
        }

        /// <summary>
        /// 设置单一文件夹策略（可递归）
        /// </summary>
        private void SetSingleStrategyRecursively(StrategyMode strategy, bool recursively)
        {
            if (Strategy != strategy)
            {
                Strategy = strategy;
                //var strategyObj = AssetBundleStrategy.Get(new_strategy);
                //strategyObj.SetAllAssetBundleName(this);
            }

            if (recursively)
            {
                foreach (var childNode in Children)
                    childNode.SetSingleStrategyRecursively(strategy, true);
            }
        }

        /// <summary>
        /// 递归克隆模板策略
        /// </summary>
        private void CloneStrategyRecursively(BuildTreeNode template, BuildTreeNode target)
        {
            target.SetBundleStrategy(template.Strategy, false);
            
            //文件夹数量不一样
            if (template.Children.Count != target.Children.Count)
            {
                Debug.LogError($"克隆AB策略时，目标{target.PackagePath}结构与模板不一致！");
                return;
            }

            //文件数量一样
            foreach (var templateChild in template.Children)
            {
                bool result = false;
                foreach (var targetChild in target.Children)
                {
                    if (targetChild.FolderName.Equals(templateChild.FolderName))
                    {
                        CloneStrategyRecursively(templateChild, targetChild);
                        result = true;
                        break;
                    }
                }
                
                if (!result)
                {
                    Debug.LogError($"克隆AB策略时，模板{template.PackagePath} 与 目标{target.PackagePath} 结构不一致！请检查资源！");
                }
            }
        }

        #endregion

        public void GetAssetToAssetPathRecursively(ref Dictionary<string, string> mappings)
        {
            GetAssetToAssetPath(ref mappings);

            foreach (var childNode in Children)
            {
                childNode.GetAssetToAssetPathRecursively(ref mappings);
            }
        }

        public void GetAssetToAssetPath(ref Dictionary<string, string> mappings)
        {
            if (Utility.GenAssetMapping(Strategy))
            {
                //生成映射的文件才需要生成名字路径映射
                foreach (var fileInfo in DirectoryInfo.GetFiles())
                {
                    if (Utility.IgnoreSuffix.Contains(fileInfo.Extension)) continue;
                    string fileName = Path.GetFileNameWithoutExtension(fileInfo.Name);
                    string assetPath = fileInfo.FullName.Substring(fileInfo.FullName.IndexOf("Assets", StringComparison.Ordinal));
                    if (mappings.ContainsKey(fileName)) Debug.LogWarning($"存在资源名重复，可能导致资源加载问题。{assetPath}");
                    mappings[fileName] = assetPath;
                }
            }
        }
    }
}