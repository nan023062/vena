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

namespace Vena.Assets
{
    public class CheckSameAssetWin : ScriptableWizard
    {
        public UnityEngine.Object[] sameAssetArray;

        private Dictionary<string, List<UnityEngine.Object>> dic;

        private void OnWizardUpdate()
        {
            helpString = "按照命名规范修改Asset命名，确保不能重名！";
            isValid = dic==null || sameAssetArray == null || sameAssetArray.Length == 0;
        }

        private void OnWizardCreate()
        {
            foreach (var assetList in dic.Values)
            {
                for (int i = 1; i < assetList.Count; i++)
                {
                    UnityEngine.Object asset = assetList[i];
                    string path = AssetDatabase.GetAssetPath(asset);
                    AssetDatabase.RenameAsset(path, asset.name + "_" + i);
                }
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void CheckSameFileName()
        {
            string assetPath = BundleEditorUtil.GetSelectedPathOrFallback();
            string inPackagePath = Utility.GetRelativePath(assetPath, Utility.AssetGameAssets);
            var bundleTreeNode = AssetBuildTree.GetOrCreateBuildNode(inPackagePath);
            if (bundleTreeNode == null) return;

            var sameDic = bundleTreeNode.GetAssetNameRepeated();
            if (sameDic != null && sameDic.Count > 0)
            {
                var checker = ScriptableWizard.DisplayWizard<CheckSameAssetWin>("重名资源列表","一键重命名");
                List<UnityEngine.Object> tmpList = new List<UnityEngine.Object>();
                foreach (var assetImps in sameDic.Values)
                {
                    tmpList.AddRange(assetImps);
                }
                checker.dic = sameDic;
                checker.sameAssetArray = tmpList.ToArray();
            }
            else
            {
                Debug.LogFormat("{0} 路径下没有重名资源...", bundleTreeNode.PackagePath);
            }
        }
    }

}