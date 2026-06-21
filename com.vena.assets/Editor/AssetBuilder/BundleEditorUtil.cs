// -----------------------------------------------------------------------------
// Vena Assets
// Unity asset pipeline toolkit (AssetBundle / VersionControl / OSS / Compression).
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

/**********************************************************************************
 * FileName:    BundleEditorUtil.cs
 * Description: 资源管理框架---编辑器 工具接口
 * History: 2019-07-09
 *********************************************************************************/
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Vena.Assets
{
    public static class BundleEditorUtil
    {
        /// <summary>
        /// 获取当前选中的Asset文件夹
        /// </summary>
        public static string GetSelectedPathOrFallback()
        {
            string path = "Assets";
            foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
            {
                path = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    path = Path.GetDirectoryName(path);
                    break;
                }
            }
            return path;
        }

        /// <summary>
        /// 获取当前选中的Asset资源
        /// </summary>
        public static string GetSelectedAssetPathOrFallback()
        {
            string path = "Assets";
            foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
            {
                path = AssetDatabase.GetAssetPath(obj);
            }
            return path;
        }

        #region 资源打包

        public static BuildTarget GetBuildTarget()
        {
#if UNITY_ANDROID
            return BuildTarget.Android;
#elif UNITY_IOS
            return BuildTarget.iOS;
#else
            return BuildTarget.StandaloneWindows64;
#endif
        }

        /// <summary>
        /// 输出AssetBundle包文件
        /// </summary>
        public static void BuildAssetBundles(BuildTreeNode assetTreeNode, bool dialogue = true)
        {
            string result = "失败！！！！！Failed, 具体细节请看控制台日志。";
            var builder = AssetPackageBuilder.GetBuilder(assetTreeNode.BuildType);
            
            if (builder.BuildPipeline(assetTreeNode, GetBuildTarget()))
                result = "成功！！！！OK";
            
            AssetDatabase.Refresh();
            
            result = $"[{assetTreeNode.FolderName}]资源打包{result}！";
            
            if (dialogue)
            {
                if (EditorUtility.DisplayDialog("BuildAssetBundles", result, "OK"))
                {
                    string outputPath = Utility.GetAssetBundleOutputPath();
                    
                    outputPath = Path.Combine(outputPath, assetTreeNode.PackagePath);
                    
                    EditorUtility.RevealInFinder(outputPath);
                }
            }
        }

        /// <summary>
        /// 删除指定目录下所有的AssetBundle包文件
        /// </summary>
        public static void DeleteAssetBundles(BuildTreeNode buildNode)
        {
            //AB包所在目录
            string assetBundlePath = Utility.GetAssetBundleOutputPath();
            
            string buildPackageName = buildNode.PackageName;
            
            if (!string.IsNullOrEmpty(buildNode.PackagePath))
            {
                assetBundlePath = Path.Combine(assetBundlePath, buildNode.PackagePath);
                
                buildPackageName = $"AB Package-{buildPackageName}";
            }
            else
            {
                buildPackageName = "All AB Package";
            }

            if (Directory.Exists(assetBundlePath))
            {
                //true 表示可以删除非空目录
                Directory.Delete(assetBundlePath, true);
                
                AssetDatabase.Refresh();
            }
            
            EditorUtility.DisplayDialog("DeleteAssetBundles", $"[{buildPackageName}] 已经清理干净！", "OK");
        }

        #endregion

        #region AssetTree 

        public static bool GetAssetBuildDataRecurively(this BuildTreeNode buildTreeNode, ref Dictionary<string, BuildDependInfo> buildDataMap)
        {
            bool result = buildTreeNode.GetAssetBuildData(ref buildDataMap);
            
            foreach (var buildNode in buildTreeNode.Children)
            {
                var isOk = buildNode.GetAssetBuildDataRecurively(ref buildDataMap);
                
                result = result && isOk;
            }
            return result;
        }

        public static bool GetAssetBuildData(this BuildTreeNode treeNode, ref Dictionary<string, BuildDependInfo> buildDataMap)
        {
            if (treeNode.Strategy == StrategyMode.Null)
            {
                Debug.LogError($"发现{treeNode.PackagePath} 的打包策略为Null，请设置并重新打【{treeNode.PackageName}】包 !");
                
                return false;
            }
            if (treeNode.Strategy != StrategyMode.NoBuild)
            {
                var strategyObj = BuildStrategy.Get(treeNode.Strategy);
                
                strategyObj.GetAllAssetBuildData(treeNode, ref buildDataMap);
            }
            return true;
        }

        public static Dictionary<string, List<UnityEngine.Object>> GetAssetNameRepeated(this BuildTreeNode treeNode)
        {
            var dic = new Dictionary<string, List<UnityEngine.Object>>();

            var buildDependInfos = new Dictionary<string, BuildDependInfo>();
            
            if (treeNode.GetAssetBuildDataRecurively(ref buildDependInfos))
            {
                foreach (var buildInfo in buildDependInfos.Values)
                {
                    //asset 与 bundle的映射
                    foreach (var keyValue in buildInfo.name2path)
                    {
                        string assetName = keyValue.Key;
                        
                        string assetPath = keyValue.Value;
                        
                        if (!dic.TryGetValue(assetName, out var assetList))
                        {
                            assetList = new List<UnityEngine.Object>();
                            
                            dic.Add(assetName, assetList);
                        }
                        
                        assetList.Add(AssetDatabase.LoadAssetAtPath(assetPath, typeof(UnityEngine.Object)));
                    }
                }

                var assetNames = dic.Keys.ToArray();
                
                foreach (var assetName in assetNames)
                {
                    var assetList = dic[assetName];
                    
                    if (assetList.Count <= 1) dic.Remove(assetName);
                }
            }
            
            return dic;
        }

        #endregion

        #region 监听资源的变更

        public static void OnPostprocessBundleTree(List<PostAssetInfo> fileList, List<PostAssetInfo> floderList)
        {
            if (AssetBuildTree.Root == null) return;
            
            //优化处理文件夹， 更新资源树
            bool result = ProcessFloderAsset(floderList);

            //再次处理文件，更新文件AB设置
            //result = result || ProcessFileAsset(fileList);

            if (result)
            {
                AssetDatabase.Refresh();
                Debug.Log("BundleTree 检测的更新...");
            }
        }

        private static bool ProcessFloderAsset(List<PostAssetInfo> floderList)
        {
            bool result = false;
            if (floderList.Count > 0)
            {
                List<PostAssetInfo> tmpFloderList = new List<PostAssetInfo>();
                tmpFloderList.AddRange(floderList);
                result = ProcessRootFloder(ref tmpFloderList);
            }

            //BundelTree变更，主动保存到文件
            if (result) AssetBuildSetting.Save(true);

            return result;
        }

        /// <summary>
        /// 修改了很多目录，但是只需要处理跟目录就可以了
        /// </summary>
        private static bool ProcessRootFloder(ref List<PostAssetInfo> floderList)
        {
            bool result = false;

            if (floderList.Count > 0)
            {
                floderList.Sort();
                PostAssetInfo asset = floderList[0];
                floderList.RemoveAt(0);

                //去掉子目录
                for (int i = floderList.Count - 1; i >= 0; i--)
                {
                    if (floderList[i].assetPath.StartsWith(asset.assetPath))
                    {
                        floderList.RemoveAt(i);
                    }
                }

                //处理根目录
                string assetPath = asset.assetPath;
                if (assetPath.StartsWith(Utility.AssetGameAssets) && assetPath.IndexOf(".") == -1)
                {
                    string inPackagePath = Utility.GetRelativePath(assetPath, Utility.AssetGameAssets);
                    if (string.IsNullOrEmpty(inPackagePath))
                    {
                        Debug.LogError($"监听到资源根目录操作！ path = {assetPath} !");
                    }
                    else if (asset.isNewAsset)
                    {
                        var bundleTreeNode = AssetBuildTree.CreateNewBundleTreeNode(inPackagePath);
                        if (bundleTreeNode != null)
                        {
                            if (bundleTreeNode.Parent.Strategy == StrategyMode.Template)
                            {
                                //如果是新增在 模板策略下，需要Copy一下策略
                                bundleTreeNode.CloneStrategyFormTemplate();
                            }
                            result = true;
                        }
                    }
                    else
                    {
                        result = AssetBuildTree.DeleteOneBuildTreeNode(inPackagePath);
                    }
                }

                //递归处理剩余目录
                result = result || ProcessRootFloder(ref floderList);
            }
            return result;
        }

        private static bool ProcessFileAsset(List<PostAssetInfo> fileList)
        {
            BuildTreeNode assetTree = AssetBuildTree.Root;
            bool result = false;

            for (int i = 0; i < fileList.Count; i++)
            {
                PostAssetInfo asset = fileList[i];
                string assetPath = asset.assetPath;

                if (!assetPath.StartsWith(Utility.AssetGameAssets)) continue;

                string inPackagePath = Utility.GetRelativePath(assetPath, Utility.AssetGameAssets);
                if (string.IsNullOrEmpty(inPackagePath))
                {
                    Debug.LogError($"监听到资源根目录操作！ path = {assetPath} !");
                    continue;
                }

                //文件变更，只对新增文件刷新BundleName
                if (asset.isNewAsset)
                {
                    int indexOf = inPackagePath.LastIndexOf('/');
                    string parenPackagePath = inPackagePath.Substring(0, indexOf);
                    var assetNode = AssetBuildTree.FindBuildTreeNodeByPath(assetTree, parenPackagePath);
                    if (assetNode == null)
                    {
                        Debug.LogError($"在BundleTree中没有找到<{inPackagePath}>节点 ! ");
                        continue;
                    }
                    //1 根据文件夹，判断打包策略
                    var StrategyObj = BuildStrategy.Get(assetNode.Strategy);
                    StrategyObj.ResetOneAssetBundleName(assetNode, assetPath);
                    result = true;
                }
            }
            return result;
        }

        #endregion
    }
}
