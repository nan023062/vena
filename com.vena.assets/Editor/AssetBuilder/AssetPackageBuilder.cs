// -----------------------------------------------------------------------------
// Vena Assets
// Unity asset pipeline toolkit (AssetBundle / VersionControl / OSS / Compression).
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

/**********************************************************************************
 * FileName:    AssetPackageBuilder.cs
 * Description: 资源管理框架---资源包打包器
 *              1 针对不同的资源包，定义不同的打包方式
 *              如：AB打包、Byte打包等等
 * History: 2019-07-09
 *********************************************************************************/

using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Vena.Assets
{
    public abstract class AssetPackageBuilder
    {
        public abstract bool BuildPipeline(BuildTreeNode packageRoot,BuildTarget buildTarget);
        
        protected bool CheckUniquenessOfFileName(AssetContext[] mappingArray, string name)
        {
            HashSet<string> checkHash = new HashSet<string>();
            bool result = true;
            foreach (var assetNameToBundle in mappingArray)
            {
                if (checkHash.Contains(assetNameToBundle.assetName))
                {
                    Debug.LogError($"TreeNode\"{name}\" 有重名文件 \"{assetNameToBundle.assetName}\"!");
                    result = false;
                }
                else
                {
                    checkHash.Add(assetNameToBundle.assetName);
                }
            }
            return result;
        }

        public static AssetPackageBuilder GetBuilder(BuildPackageType buildType)
        {
            AssetPackageBuilder builder = null;
            switch (buildType)
            {
                case BuildPackageType.Bundles:
                    builder = new BundlePackageBuilder();
                    break;
                case BuildPackageType.Bytes:
                    builder = new BytePackageBuilder();
                    break;
                default:
                    break;
            }
            return builder;
        }
    }

    public class BundlePackageBuilder : AssetPackageBuilder
    {
        public override bool BuildPipeline(BuildTreeNode packageRoot, BuildTarget buildTarget)
        {
            //1.AB输出路径
            string outputPath = Utility.GetAssetBundleOutputPath();
            outputPath = Path.Combine(outputPath, packageRoot.PackagePath);
            if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);

            //2.构建Build AssetBundle文件
            var assetBuildDataMap = new Dictionary<string, BuildDependInfo>();
            if (!packageRoot.GetAssetBuildDataRecurively(ref assetBuildDataMap))
                return false;

            List<AssetBundleBuild> buildList = new List<AssetBundleBuild>();
            List<AssetContext> mappingLst = new List<AssetContext>();
            foreach (var buildInfo in assetBuildDataMap.Values)
            {
                //2.1 构建Unity Build Bundle的信息
                var build = new AssetBundleBuild();
                build.assetBundleName = buildInfo.bundleName;
                build.assetBundleVariant = Utility.AssetBundleSuffix;
                int count = buildInfo.name2path.Keys.Count;
                build.assetNames = new string[count];
                buildInfo.name2path.Values.CopyTo(build.assetNames, 0);
                buildList.Add(build);

                //2.2 构建asset 与 bundle的映射信息
                if (Utility.GenAssetMapping(buildInfo.mode))
                {
                    foreach (var assetName in buildInfo.name2path.Keys)
                    {
                        var mapping = new AssetContext();
                        mapping.assetName = assetName;
                        mapping.bundleName = build.assetBundleName;
                        mappingLst.Add(mapping);
                    }
                }
            }

            //3. 检查并生成映射文件、输出AB文件
            bool result = false;
            AssetContext[] mappingArray = mappingLst.ToArray();
            if (CheckUniquenessOfFileName(mappingArray, packageRoot.PackagePath))
            {
                //3.1 生成映射文件
                var mappingAsset = ScriptableObject.CreateInstance<AssetList>();
                mappingAsset.contexts = mappingArray;
                string mappingAssetPath = Utility.GetAssetToBundleMapAssetPath(packageRoot.FolderName);
                mappingAssetPath = mappingAssetPath.Substring(Application.dataPath.Length - 6);
                AssetDatabase.CreateAsset(mappingAsset, mappingAssetPath);

                //3.2 Build AssetBundles
                UnityEditor.BuildPipeline.BuildAssetBundles(outputPath, buildList.ToArray(), BuildAssetBundleOptions.None, buildTarget);
                AssetDatabase.Refresh();
                result = true;
            }
            else
            {
                Debug.LogError($"Pakcage 【{packageRoot.PackageName}】 输出Bundle失败！");
            }

            return result;
        }
    }

    public class BytePackageBuilder : AssetPackageBuilder
    {
        static readonly string[] FileExtensions = { ".lua",".txt",".byte" };

        private BuildTreeNode _packageRoot;
        
        private string mOutputPath = string.Empty;

        public override bool BuildPipeline(BuildTreeNode packageRoot, BuildTarget buildTarget)
        {
            _packageRoot = packageRoot;
            mOutputPath = Utility.GetAssetBundleOutputPath();
            mOutputPath = Path.Combine(mOutputPath, packageRoot.PackagePath);
            mOutputPath = mOutputPath.Replace('\\', '/');
            if (!Directory.Exists(mOutputPath)) Directory.CreateDirectory(mOutputPath);
            CopyAllFileToByte(packageRoot.DirectoryInfo);
            return true;
        }

        private void CopyAllFileToByte(DirectoryInfo dirInfo)
        {
            foreach (var systemInfo in dirInfo.GetFileSystemInfos())
            {
                if (systemInfo is FileInfo)
                {
                    FileInfo fileInfo = systemInfo as FileInfo;
                    if (!FileExtensions.Contains(fileInfo.Extension)) continue;
                    if (fileInfo.Name.Contains(Utility.AssetBundleMapping)) continue;
                    string assetPath = fileInfo.FullName.Replace('\\', '/');
                    int indexOf = assetPath.IndexOf(_packageRoot.DirectoryInfo.Name)+ _packageRoot.DirectoryInfo.Name.Length;
                    string relativePath = assetPath.Substring(indexOf);
                    relativePath = relativePath.Substring(0, relativePath.LastIndexOf('.'));
                    byte[] bytes = FileIO.SafeReadAllBytes(assetPath);
                    ProcessBytes(ref bytes);
                    string saveFilePath = mOutputPath + relativePath + ".byte";
                    FileIO.SafeWriteAllBytes(saveFilePath, bytes);
                }
                else if (systemInfo is DirectoryInfo)
                {
                    CopyAllFileToByte(systemInfo as DirectoryInfo);
                }
            }
        }

        private void ProcessBytes(ref byte[] bytes)
        {
            //TO DO:
            //bytes = EditorFileEncrypt.EncryptByByte(bytes, EditorFileEncrypt.EncryptKey);
            //return bytes;
        }
    }
}

