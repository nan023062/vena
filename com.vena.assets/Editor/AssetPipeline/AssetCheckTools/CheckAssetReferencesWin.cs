// -----------------------------------------------------------------------------
// Vena Assets
// Unity asset pipeline toolkit (AssetBundle / VersionControl / OSS / Compression).
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------


using UnityEngine;
using System.Collections;
using System.IO;
using UnityEditor;
using System.Collections.Generic;
using UObj = UnityEngine.Object;
using System.Linq;

namespace Vena.Assets
{
    /// <summary>
    /// 检查资源的被引用关系
    /// </summary>
    public class CheckAssetReferencesWin : ScriptableWizard
    {
        public string targetAssetPath;
        public UObj target = null;
        public UObj[] refAssetArray;

        private void OnWizardUpdate()
        {
            helpString = "1. 不为空说明被其他资源使用。\n" +
                "2. 为空说明没有被使用。\n" +
                "3. 注意：请确认是否是被动态加载的资源！";
            isValid = refAssetArray == null || refAssetArray.Length <= 0;
        }

        private void OnWizardCreate()
        {
            AssetDatabase.DeleteAsset(targetAssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void CheckOnAssetRefs()
        {
            string assetPath = BundleEditorUtil.GetSelectedAssetPathOrFallback();
            var asset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(UObj));

            //查找资源的Package节点
            string inPackagePath = Utility.GetRelativePath(assetPath, Utility.AssetGameAssets);
            int indexOf = inPackagePath.IndexOf('/');
            if (indexOf != -1) inPackagePath = inPackagePath.Substring(0, indexOf);

            var package = PackageDependenciesTool.CollectPackageDependencies(inPackagePath);

            var checker = ScriptableWizard.DisplayWizard<CheckAssetReferencesWin>("资源被引用列表", "删除");
            List<UObj> refList = null;
            if (package.references.TryGetValue(assetPath, out refList))
            {
                checker.refAssetArray = refList.ToArray();
            }
            else
            {
                checker.refAssetArray = null;
            }
            checker.targetAssetPath = assetPath;
            checker.target = asset;
        }

    }
    /// <summary>
     /// 检查资源的被引用关系
     /// </summary>
    public class CheckAssetDependenciesWin : ScriptableWizard
    {
        public UObj[] 选择的资源;
        public UObj[] 依赖资源;

        private void OnWizardUpdate()
        {
            helpString = "1. 不为空说明包含依赖资源。\n" +
                "2. 为空说明没有依赖资源。\n";
        }

        static void ParseDependencies(string path, HashSet<string> refList)
        {
            string strPath = path.Replace("\\", "/");
            string rootPath = Path.GetDirectoryName(Application.dataPath).Replace("\\", "/");
            string fullPath = rootPath + "/" + strPath;
            if (Directory.Exists(fullPath))
            {
                string[] files =Directory.GetFiles(fullPath, "*.*", System.IO.SearchOption.AllDirectories);
                if(files != null)
                {
                    for (int i = 0; i < files.Length; ++i)
                    {
                        string filePath = files[i].Replace("\\", "/");
                        if (filePath.Contains(".svn") || filePath.Contains(".meta"))
                        {
                            continue;
                        }
                        string objPath = filePath.Replace(rootPath + "/", string.Empty);
                        ParseDependencies(objPath, refList);
                    }
                }
            }
            else
            {
                string[] array = AssetDatabase.GetDependencies(path);
                for(int i = 0; i < array.Length; ++i)
                {
                    string strFile = array[i].Replace("\\", "/");
                    if (refList.Contains(strFile) || path == strFile)
                        continue;
                    refList.Add(strFile);
                }
            }
        }

        public static void CheckOnAssetRefs()
        {
            UnityEngine.Object[] objs = Selection.GetFiltered(typeof(object), SelectionMode.Assets);
            if (objs == null)
                return;
            
            HashSet<string> refSet = new HashSet<string>();
            for (int i = 0; i < objs.Length; ++i)
            {
                string strPath = AssetDatabase.GetAssetPath(objs[i]).Replace("\\", "/");
                ParseDependencies(strPath, refSet);
            }
            List<UObj> refList = new List<UObj>();
            foreach (string path in refSet)
            {
                refList.Add(AssetDatabase.LoadAssetAtPath(path, typeof(UObj)));
            }

            var checker = ScriptableWizard.DisplayWizard<CheckAssetDependenciesWin>("所有依赖资源", "删除");
            checker.依赖资源 = refList.ToArray();
            checker.选择的资源 = objs;
        }
    }

    /// <summary>
    /// 检查为被使用的资源
    /// </summary>
    public class CheckUnusedAssetWin : ScriptableWizard
    {
        public string packageName;

        [Header("未使用的资源--依赖加载")]
        public UObj[] 未使用静态资源;

        [Header("未使用的动态资源--代码加载")]
        public UObj[] 动态资源列表;

       private const string helpStr = "【{0}】未使用资源清单：\n" +
            "\t1. 以下是包内未被使用资源的清单。\n" +
            "\t2. 注意含有代码动态加载的资源。";

        private void OnWizardUpdate()
        {
            //isValid = false;// (unusedNoMapping != null && unusedNoMapping.Length > 0 );
        }

        private void OnWizardCreate()
        {
            //string TempDir = Utils.TempGameAssets + "/" + packageName + "/";
            foreach (var asset in 未使用静态资源)
            {
                var assetPath = AssetDatabase.GetAssetPath(asset);
                //var fileName = assetPath.Substring(packagePath.Length);
                //var fileName = Path.GetFileName(assetPath);
                //AssetDatabase.MoveAsset(assetPath, TempDir + fileName);
                AssetDatabase.DeleteAsset(assetPath);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void CheckUnusedAsset()
        {
            string assetPath = BundleEditorUtil.GetSelectedPathOrFallback();
            var asset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(UObj));

            //查找资源的Package节点
            string inPackagePath = Utility.GetRelativePath(assetPath, Utility.AssetGameAssets);
            int indexOf = inPackagePath.IndexOf('/');
            if (indexOf != -1) inPackagePath = inPackagePath.Substring(0, indexOf);

            var package = PackageDependenciesTool.CollectPackageDependencies(inPackagePath);

            var checker = ScriptableWizard.DisplayWizard<CheckUnusedAssetWin>("未使用资源列表", "删除");
            List<UObj> unusedNoMapping = new List<UObj>();
            List<UObj> unusedMapping = new List<UObj>();
            CollectUnusedAsset(package, ref unusedNoMapping, ref unusedMapping);
            checker.isValid = true;
            checker.packageName = package.package.FolderName;
            checker.动态资源列表 = unusedMapping.ToArray();
            checker.未使用静态资源 = unusedNoMapping.ToArray();
            checker.helpString = string.Format(helpStr, inPackagePath);
        }

        private static void CollectUnusedAsset(PackageReferenceInfo package, 
            ref List<UObj> unusedNoMapping, ref List<UObj> unusedMapping)
        {
            foreach (var assetPathToObj in package.allAssets)
            {
                if (!package.references.ContainsKey(assetPathToObj.Key))
                {
                    //查找资源的树节点
                    string inPackagePath = Utility.GetRelativePath(assetPathToObj.Key, Utility.AssetGameAssets);
                    inPackagePath = inPackagePath.Substring(0,inPackagePath.LastIndexOf('/'));
                    var assetBuildTree = AssetBuildTree.Root;
                    var assetTreeNode = AssetBuildTree.FindBuildTreeNodeByPath(assetBuildTree, inPackagePath);
                    
                    if(Utility.GenAssetMapping(assetTreeNode.Strategy))
                    {
                        unusedMapping.Add(assetPathToObj.Value);
                    }
                    else
                    {
                        unusedNoMapping.Add(assetPathToObj.Value);
                    }
                }
            }
        }

    }
}