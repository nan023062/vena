// -----------------------------------------------------------------------------
// Vena Assets
// Unity asset pipeline toolkit (AssetBundle / VersionControl / OSS / Compression).
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

/**********************************************************************************
 * FileName:    Utils.cs
 * Description: 资源管理框架---工具类
 * History: 2019-07-09
 *********************************************************************************/

using System;
using UnityEngine;
using System.IO;
using System.Text;

namespace Vena.Assets
{
    public static class Utility
    {
        public static readonly string AssetBundleSuffix = "";
        
        public static readonly string AssetBundleManifest = "AssetBundleManifest";

        public static readonly string AssetBundleMapping = "-mapping";
        
        private static readonly StringBuilder StringBuilder = new StringBuilder();
        
        public static string AssetGameAssets
        {
            get
            {
                string sourceRootPath = AssetBuildSetting.Instance.GetAssetRootPath();
                int index = sourceRootPath.IndexOf(Application.dataPath, StringComparison.Ordinal);
                if (index < 0) return sourceRootPath;
                return sourceRootPath.Substring(Application.dataPath.Length - 6);
            }
        }

        public static bool UseAssetBundle
        {
            get
            {
#if UNITY_EDITOR
                return AssetBuildSetting.Instance.useAssetBundle;
#else
                return true;
#endif
            }
        }

        public static string GameAssetInputPath()
        {
            string sourceRootPath = AssetBuildSetting.Instance.GetAssetRootPath();
            return Path.Combine(Application.dataPath, sourceRootPath);
        }

        public static string GetAssetBundleOutputPath()
        {
            return Path.Combine(AssetBuildSetting.Instance.GetBundleOutputPath(), GetPlatformName());
        }

        public static string GetAssetsStoragePath()
        {
            return Path.Combine(AssetBuildSetting.Instance.GetLocalStorePath(), GetPlatformName());
        }

        public static string GetAssetToBundleMapAssetPath(string root)
        {
            string sourceRootPath = AssetBuildSetting.Instance.GetAssetRootPath();
            return Path.Combine(sourceRootPath, root + "/" + root + AssetBundleMapping + ".asset");
        }

        public static string GetAssetToBundleMappingPath(string root)
        {
            return Path.Combine(GameAssetInputPath(), root + "/" + root + "_AssetToBundle.asset");
        }

        public static string GetBundleFullPath(string path, string bundleName, bool withExt)
        {
            path = Path.Combine(path, bundleName);
            return path;
        }

        private static string _bundleDataPath = string.Empty;
        
        public static string GetPersistentDataPath()
        {
            if (string.IsNullOrEmpty(_bundleDataPath))
            {
                switch (Application.platform)
                {
                    case RuntimePlatform.WindowsEditor:
                    case RuntimePlatform.OSXEditor:
                        _bundleDataPath = GetAssetBundleOutputPath();
                        break;
                    case RuntimePlatform.Android:
                    case RuntimePlatform.IPhonePlayer:
                    case RuntimePlatform.WindowsPlayer:
                        _bundleDataPath = Application.persistentDataPath;
                        break;
                }
            }
            return _bundleDataPath;
        }

        public static string GetPersistentDataPathURL(string fileName)
        {
            StringBuilder.Clear();

            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    StringBuilder.Append("jar:file://");
                    break;

                case RuntimePlatform.IPhonePlayer:
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                    StringBuilder.Append("file://");
                    break;
            }
            StringBuilder.Append(GetPersistentDataPath()).Append("/.").Append(fileName);
            return StringBuilder.ToString();
        }

        private static string _bundleApplicationPath = string.Empty;
        
        public static string GetStreamingAssetDataPath()
        {
            if (string.IsNullOrEmpty(_bundleApplicationPath))
            {
                _bundleApplicationPath = Path.Combine(Application.streamingAssetsPath, "GameAssets");
            }
            return _bundleApplicationPath;
        }

        public static string GetStreamingAssetDataPathURL(string fileName)
        {
            StringBuilder.Clear();

            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    StringBuilder.Append("jar:file://");
                    break;

                case RuntimePlatform.IPhonePlayer:
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                    StringBuilder.Append("file://");
                    break;
            }
            StringBuilder.Append(GetStreamingAssetDataPath()).Append("/.").Append(fileName);
            return StringBuilder.ToString();
        }

        public static string GetAssetDatabasePath(string fullPath)
        {
            int index = fullPath.IndexOf("Assets", StringComparison.Ordinal);
            return fullPath.Substring(index);
        }

        public static string GetPlatformName()
        {
#if UNITY_ANDROID
            return "Android";
#elif UNITY_IOS
            return "IPhone";
#else
            return "Windows";
#endif
        }
            
        public static string GetAssetBundleURL(string rootPath, string bundleName)
        {
            StringBuilder.Clear();

            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    StringBuilder.Append("jar:file://");
                    break;

                case RuntimePlatform.IPhonePlayer:
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                    StringBuilder.Append("file://");
                    break;
            }

            StringBuilder.Append(rootPath).Append("/")
            .Append(bundleName).Append(".").Append(AssetBundleSuffix);
            return StringBuilder.ToString();
        }

        public static string GetRelativePath(string fullPath, string root)
        {
            fullPath = fullPath.Replace("\\", "/");
            int index = fullPath.IndexOf(root, StringComparison.Ordinal) + root.Length;
            if (fullPath.Length > index + 1)
            {
                index += 1;
            }
            return fullPath.Substring(index);
        }

        public static string GetAssetPackagePath(string packageName)
        {
            string packagePath = UseAssetBundle ? GetPersistentDataPath() : GameAssetInputPath();
            return Path.Combine(packagePath, packageName);
        }

        public static string GetAssetPackageUrl(string packageName)
        {
            if (UseAssetBundle)
            {
                return GetPersistentDataPathURL(packageName);
            }
            string packagePath = GameAssetInputPath();
            return Path.Combine(packagePath, packageName);
        }

        public static int VersionCompare(string great, string less)
        {
            int nX = 0, nY = 0;
            int.TryParse(great.Replace(".", ""), out nX);
            int.TryParse(less.Replace(".", ""), out nY);
            if (nX == nY) return 0;
            return nX > nY ? -1 : 1;
        }

        public static string ConvertToWWWPath(string path)
        {
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    if(!path.StartsWith("jar:file://"))
                        path = "jar:file://" + path;
                    break;

                case RuntimePlatform.IPhonePlayer:
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                    if (!path.StartsWith("file://"))
                        path = "file://" + path;
                    break;
            }
            return path;
        }
        
        //忽略的文件类型
        public static readonly string[] IgnoreSuffix = {
             ".meta",".tmp",".cs",".iso",".io",".vs",".sv",".sln",".luaproj",
        };
        
        public static string AbsolutePathToUnityRelativePath(string absolutePath)
        {
            try
            {
                absolutePath = absolutePath.Replace("\\", "/");
                int indexOf = absolutePath.IndexOf(Application.dataPath + "/", StringComparison.Ordinal);
                //如果是子目录
                if (indexOf == 0)
                {
                    return "{0}/" + absolutePath.Substring(Application.dataPath.Length + 1);
                }
                
                var destDirectory = Directory.CreateDirectory(absolutePath);
                var assetDirectory = Directory.CreateDirectory(Application.dataPath);

                //如果在同一磁盤-使用相對路徑
                StringBuilder.Clear();
                StringBuilder.Append("{0}/");

                for (var parent = assetDirectory.Parent; parent != null; parent = parent.Parent)
                {
                    StringBuilder.Append("../");
                    for (var dest = destDirectory.Parent; dest != null; dest = dest.Parent)
                    {
                        if (dest.FullName == parent.FullName)
                        {
                            string parentPath = parent.FullName.Replace("\\", "/");
                            string path = absolutePath.Substring(parentPath.Length + 1);
                            StringBuilder.Append(path);
                            return StringBuilder.ToString();
                        }
                    }
                }

                //否則使用絕對路徑
                return absolutePath;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        public static string UnityRelativePathToAbsolutePath(string relativePath)
        {
            if (relativePath.StartsWith("{0}/"))
            {
                return string.Format(relativePath, Application.dataPath);
            }
            return relativePath;
        }

        public static bool GenAssetMapping(StrategyMode strategy)
        {
            return strategy >= StrategyMode.OneFile && strategy < StrategyMode.NoBuild && (int)strategy % 2 == 0;
        }
    }
}
