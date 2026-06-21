// -----------------------------------------------------------------------------
// Vena Assets
// Unity asset pipeline toolkit (AssetBundle / VersionControl / OSS / Compression).
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

/**********************************************************************************
 * FileName:    ResourceChecker.cs
 * Description: 资源管理框架---资源导入检查工具
 * 说明：该文件为非打包框架内容，是根据当前项目的需要，自定义的资源格式、目录结构及
 *       AB打包策略。针对新项目可以删除和修改
 * History: 2019-07-09
 *********************************************************************************/

using UnityEditor;
using System;
using UnityEngine;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace Vena.Assets
{
    public static class ResourceChecker
    {
        //模型导入之前调用  
        public static void OnPreprocessModel(string assetPath, AssetImporter assetImporter)
        {
            Debug.Log("OnPreprocessModel=" + assetPath);
        }

        //模型导入之前调用  
        public static void OnPostprocessModel(string assetPath, AssetImporter assetImporter, GameObject go)
        {
            Debug.Log("OnPostprocessModel=" + go.name);
        }

        //纹理导入之前调用，针对入到的纹理进行设置  
        public static void OnPreprocessTexture(string assetPath, AssetImporter assetImporter)
        {
            Debug.Log("OnPreProcessTexture=" + assetPath);
        }

        //文理导入之后
        public static void OnPostprocessTexture(string assetPath, AssetImporter assetImporter, Texture2D tex)
        {
            Debug.Log("OnPostProcessTexture=" + assetPath);
            /*TextureImporter impor = assetImporter as TextureImporter;
            impor.textureCompression = TextureImporterCompression.Compressed;
            impor.maxTextureSize = 512;
            impor.textureType = TextureImporterType.Sprite;
            //impor.textureFormat = TextureImporterFormat.ETC2_RGB4;
            impor.mipmapEnabled = false;*/
        }

        //音频导入之前
        public static void OnPreprocessAudio(string assetPath, AssetImporter assetImporter)
        {
            Debug.Log("OnPreprocessAudio");
            AudioImporter audio = assetImporter as AudioImporter;
        }

        //音频导入之后
        public static void OnPostprocessAudio(string assetPath, AssetImporter assetImporter, AudioClip clip)
        {
            Debug.Log("OnPostprocessAudio=" + clip.name);
        }

        public static void OnPostprocessAllAssets(List<PostAssetInfo> fileList, List<PostAssetInfo> floderList)
        {

        }

        //所有的资源的导入
        public static void OnPostprocessAllAssets(string[] importedAsset, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            UIResChecker.OnUIResUpdate(movedFromAssetPaths, importedAsset);

            ModelResChecker.OnModelResUpdate(movedFromAssetPaths, importedAsset);

            EffectResChecker.OnEffectResUpdate(movedFromAssetPaths, importedAsset);
        }


        #region 工具

        public static void CheckFloderUnuseAsset()
        {
            string assetPath = BundleEditorUtil.GetSelectedPathOrFallback();
            string inPackagePath = Utility.GetRelativePath(assetPath, Utility.AssetGameAssets);
            var bundleTreeNode = AssetBuildTree.GetOrCreateBuildNode(inPackagePath);
            if (bundleTreeNode == null) return;

            
        }


        #endregion
    }
}
