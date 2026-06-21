// -----------------------------------------------------------------------------
// Vena Assets
// Unity asset pipeline toolkit (AssetBundle / VersionControl / OSS / Compression).
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Vena.Assets
{
    /// <summary>
    /// 资源打AB的策略对象
    /// </summary>
    public class BuildStrategy
    {
        #region 策略对象的实例 工厂

        //策略对象的实例
        static Dictionary<int, BuildStrategy> strategys = null;

        //获得一个指定类策略对象
        public static BuildStrategy Get(StrategyMode mode)
        {
            BuildStrategy result = null;

            if (strategys == null)
            {
                strategys = new Dictionary<int, BuildStrategy>();
            }

            if (!strategys.TryGetValue((int)mode, out result))
            {
                switch (mode)
                {
                    case StrategyMode.NoBuild:
                    case StrategyMode.Template:
                        result = new BuildStrategy();
                        break;
                    case StrategyMode.OneFile:
                    case StrategyMode.OneFileGenMapping:
                        result = new Strategy_OneFile();
                        break;
                    case StrategyMode.AllFile:
                    case StrategyMode.AllFileGenMapping:
                        result = new Strategy_AllFile();
                        break;
                    case StrategyMode.BySize:
                    case StrategyMode.BySizeGenMapping:
                        result = new Strategy_ByFileSize();
                        break;
                    case StrategyMode.AllFolder:
                    case StrategyMode.AllFolderGenMapping:
                        result = new Strategy_AllFloder();
                        break;
                    case StrategyMode.Null:
                        throw new System.Exception("没有初始化打包策略!");
                }
                strategys.Add((int)mode, result);
            }
            return result;
        }

        #endregion

        public virtual void SetAllAssetBundleName(BuildTreeNode assetNode)
        {
            if (!assetNode.DirectoryInfo.Exists) return;

            FileInfo[] fileInfos = assetNode.DirectoryInfo.GetFiles();
            foreach (var fileInfo in fileInfos)
            {
                //文件扩展名检查
                if (Utility.IgnoreSuffix.Contains(fileInfo.Extension)) continue;
                string assetPath = fileInfo.FullName.Substring(fileInfo.FullName.IndexOf("Assets"));
                ResetOneAssetBundleName(assetNode, assetPath);
            }
        }

        public void GetAllAssetBuildData(BuildTreeNode assetNode, ref Dictionary<string, BuildDependInfo> buildStrategyMap)
        {
            if (!assetNode.DirectoryInfo.Exists)
            {
                Debug.LogError("当前文件并不存在！");
                return;
            }

            FileInfo[] fileInfos = assetNode.DirectoryInfo.GetFiles();
            foreach (var fileInfo in fileInfos)
            {
                //文件扩展名检查
                if (Utility.IgnoreSuffix.Contains(fileInfo.Extension)) continue;
                string assetPath = fileInfo.FullName.Substring(fileInfo.FullName.IndexOf("Assets"));
                assetPath = assetPath.Replace('\\', '/');

                string bundleName = GetBundleName(assetPath, assetNode);
                string assetName = Path.GetFileNameWithoutExtension(assetPath);
                bundleName = ConvertPathToBundleName(bundleName);
                if (string.IsNullOrEmpty(bundleName))
                {
                    throw new System.Exception($"{assetPath}没有获取到BundleName ！请检查！");
                }       

                //添加到BundleBuild Map
                if (!buildStrategyMap.TryGetValue(bundleName, out var assetBundleBuild))
                {
                    assetBundleBuild = new BuildDependInfo();
                    assetBundleBuild.mode = assetNode.Strategy;
                    buildStrategyMap.Add(bundleName, assetBundleBuild);
                    assetBundleBuild.bundleName = bundleName;
                }
                assetBundleBuild.name2path[assetName] = assetPath;
            }
        }

        public void ResetOneAssetBundleName(BuildTreeNode assetNode, string assetPath)
        {
            string fileName = Path.GetFileNameWithoutExtension(assetPath);
            string assetBundleName = GetBundleName(assetPath, assetNode);
            assetBundleName = ConvertPathToBundleName(assetBundleName);
            AssetImporter importer = AssetImporter.GetAtPath(assetPath);
            if (!string.IsNullOrEmpty(assetBundleName))
            {
                importer.assetBundleName = assetBundleName;
                importer.assetBundleVariant = Utility.AssetBundleSuffix;
            }
            else
            {
                importer.assetBundleName = string.Empty;
                //importer.assetBundleVariant = string.Empty;
            }
        }

        protected virtual string GetBundleName(string assetPath, BuildTreeNode assetNode)
        {
            return string.Empty;
        }

        private string ConvertPathToBundleName(string bundlePath)
        {
            bundlePath = bundlePath.Replace('\\', '/');
            bundlePath = bundlePath.Replace('/', '_');
            return bundlePath.ToLower();
        }
    }

    public class Strategy_OneFile : BuildStrategy
    {
        protected override string GetBundleName(string assetPath, BuildTreeNode assetNode)
        {
            string fileName = Path.GetFileNameWithoutExtension(assetPath);
            string assetBundleName = Path.Combine(assetNode.RelativePath, fileName);
            return assetBundleName.Replace('\\', '/');
        }
    }

    public class Strategy_AllFile : BuildStrategy
    {
        protected override string GetBundleName(string assetPath, BuildTreeNode assetNode)
        {
            string bundleName = assetNode.RelativePath;
            if (string.IsNullOrEmpty(bundleName))
            {
                Debug.LogWarning("BundleStrategyAllFile.GetBundleName()，根据策略得到的BundleaName为空!\n" +
                "assetPath = " + assetPath);
            }
            return bundleName;
        }
    }

    public class Strategy_AllFloder : BuildStrategy
    {
        protected override string GetBundleName(string assetPath, BuildTreeNode assetNode)
        {
            BuildTreeNode next = assetNode.Parent;
            while (next != null && next.Strategy == StrategyMode.AllFolder)
            {
                assetNode = next;
                next = assetNode.Parent;
            }
            string bundleName = assetNode.RelativePath;
            if (string.IsNullOrEmpty(bundleName))
            {
                Debug.LogWarning("BundleStrategyAllFloder.GetBundleName()，根据策略得到的BundleaName为空!\n" +
                "assetPath = " + assetPath);
            }
            return bundleName;
        }
    }

    public class Strategy_ByFileSize : BuildStrategy
    {
        public static long bytes = 1 * 1024 * 1024; //默认1M

        public override void SetAllAssetBundleName(BuildTreeNode assetNode)
        {
            if (!assetNode.DirectoryInfo.Exists) return;

            long __fileSize = 0;
            int __index = 1;
            string assetBundleName = assetNode.RelativePath + "-" + __index;

            FileInfo[] fileInfos = assetNode.DirectoryInfo.GetFiles();

            foreach (var fileInfo in fileInfos)
            {
                //文件扩展名检查
                if (Utility.IgnoreSuffix.Contains(fileInfo.Extension))
                    continue;

                __fileSize += fileInfo.Length;
                if (__fileSize > bytes)
                {
                    __index++;
                    __fileSize = 0;
                    assetBundleName = assetNode.RelativePath + "-" + __index;
                }
                string assetPath = fileInfo.FullName.Substring(fileInfo.FullName.IndexOf("Assets"));

                //设置bundlename
                AssetImporter importer = AssetImporter.GetAtPath(assetPath);
                importer.assetBundleName = assetBundleName.ToLower();
                importer.assetBundleVariant = Utility.AssetBundleSuffix;
            }
        }

        protected override string GetBundleName(string assetPath, BuildTreeNode assetNode)
        {
            Debug.LogError("!!!!!!! 按照大小的策略不允许单个设置bundle！！！！");
            return string.Empty;
        }
    }
}

