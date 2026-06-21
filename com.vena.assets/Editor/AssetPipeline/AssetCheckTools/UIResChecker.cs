// -----------------------------------------------------------------------------
// Vena Assets
// Unity asset pipeline toolkit (AssetBundle / VersionControl / OSS / Compression).
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

/**************************************************************************
 *  说明：该文件为非打包框架内容，是根据当前项目的需要，自定义的“组合”打包
 *  策略。针对新项目可以删除和修改
 *      1 UI资源检查工具
 *      2 UI资源打包策略设置工具
 *  write by linan 2019-07-09
 * ***********************************************************************/

using UnityEngine;
using System.Collections;
using System.IO;
using UnityEditor;
using System.Collections.Generic;

namespace Vena.Assets
{
    public static class UIResChecker
    {
        public static void OnUIResUpdate(string[] floderPaths, string[] assetPaths)
        {

        }

        #region 其他批处理工具

        public static void BatchImgToSpriteFormat(string assetFloderPath)
        {
            assetFloderPath = assetFloderPath.Replace('\\', '/');
            assetFloderPath = assetFloderPath.Substring(7, assetFloderPath.Length - 7);
            string DIR = Path.Combine(Application.dataPath, assetFloderPath);
            DirectoryInfo dirInfo = Directory.CreateDirectory(DIR);
            BatchDirectoryFilesImgToSprite(dirInfo);
            AssetDatabase.Refresh();
        }

        private static void BatchDirectoryFilesImgToSprite(DirectoryInfo dirInfo)
        {
            foreach (var systemInfo in dirInfo.GetFileSystemInfos())
            {
                if (systemInfo is FileInfo)
                {
                    string assetPath = systemInfo.FullName.Substring(systemInfo.FullName.IndexOf("Assets"));
                    AssetImporter importer = AssetImporter.GetAtPath(assetPath);
                    TextureImporter impor = importer as TextureImporter;
                    if (impor != null)
                    {
                        impor.textureType = TextureImporterType.Sprite;
                    }
                }
                else if (systemInfo is DirectoryInfo)
                {
                    BatchDirectoryFilesImgToSprite(systemInfo as DirectoryInfo);
                }
            }
        }

        #endregion
    }
}