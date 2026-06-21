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
using UObject = UnityEngine.Object;

namespace Vena.Assets
{
    public class CheckSameTextureWin : ScriptableWizard
    {
        public UObject[][] similarTexturesArray;

        private void OnWizardUpdate()
        {
            isValid = similarTexturesArray == null || similarTexturesArray.Length == 0;
        }

        private void OnWizardCreate()
        {
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
                var checker = ScriptableWizard.DisplayWizard<CheckSameTextureWin>("相似图片列表", "确定");
                List<UObject[]> tmpList = new List<UObject[]>();
                checker.similarTexturesArray = tmpList.ToArray();
                checker.helpString = "相似";
            }
            else
            {
                Debug.LogFormat("{0} 路径下没有重名资源...", bundleTreeNode.PackagePath);
            }
        }


        public static void CheckTextureSame()
        {
            //CheckTwoTexture
        }

        /// <summary>
        /// 检查图片是否一样
        /// </summary>
        private static bool CheckTwoTexture(Texture2D texA, Texture2D texB, int accuracy = 16)
        {
            Color32[] pixelsA = texA.GetPixels32();
            Color32[] pixelsB = texB.GetPixels32();

            if (pixelsA.Length != pixelsB.Length) return false;

            int delta = Mathf.Max(1,pixelsA.Length / accuracy);

            for (int i = 0; i < pixelsA.Length; i+= delta)
            {
                if (ColorToInt(pixelsA[i]) != ColorToInt(pixelsB[i])) return false;
            }
            return true;
        }

        private static int ColorToInt(Color color)
        {
            int r = Mathf.CeilToInt(color.r * 255);
            int g = Mathf.CeilToInt(color.g * 255);
            int b = Mathf.CeilToInt(color.b * 255);
            int a = Mathf.CeilToInt(color.a * 255);
            return r << 24 + g << 16 + b << 6 + a;
        }
    }

}