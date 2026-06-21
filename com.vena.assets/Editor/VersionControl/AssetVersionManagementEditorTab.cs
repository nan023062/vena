// -----------------------------------------------------------------------------
// Vena Assets
// Unity asset pipeline toolkit (AssetBundle / VersionControl / OSS / Compression).
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using Vena.Assets.Editor;

namespace Vena.Assets
{
    public sealed class AssetVersionManagementEditorTab : AssetToolkitTab
    {
        public override string TabName => "Version Management";
        
        private List<VersionPackageData> versionList;
        
        private Version m_CreateNewVersion;

        private string m_localStorePath = string.Empty;
        
        private string m_remoteAddress = string.Empty;

        private void Awake()
        {
            versionList = new List<VersionPackageData>();
        }

        protected override void OnEnterDraw()
        {
            var editorConf = AssetBuildSetting.Instance;
            var usageConf = AssetUsageSetting.Instance;
            m_localStorePath = editorConf.localStorePath;
            m_remoteAddress = usageConf.remoteAddress;
            TryLoadLocalVersionList();
        }

        protected override void OnExitDraw()
        {
        }

        protected override void OnDrawGUI()
        {
            //1 平台版本设置信息
            CommonGUI.SeparatorLine("Path Setting ：");
            DrawVersionPathAndStoreAddress();

            //2 本地版本列表
            DrawAllVersionList();
        }
        
        private void DrawVersionPathAndStoreAddress()
        {
            var editorConf = AssetBuildSetting.Instance;
            var usageConf = AssetUsageSetting.Instance;
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("   < Local Path > : ", GUILayout.Width(160));
            GUILayout.Label(m_localStorePath, GUILayout.ExpandWidth(true));
            if (GUILayout.Button("...", GUILayout.Width(30)))
            {
                var path = Application.dataPath;
                if(!string.IsNullOrEmpty(m_localStorePath))
                    path = Utility.UnityRelativePathToAbsolutePath(m_localStorePath);
                var newPath = EditorUtility.OpenFolderPanel("Select Dir", path, "");
                if (!string.IsNullOrEmpty(newPath) && newPath != m_localStorePath)
                {
                    m_localStorePath = Utility.AbsolutePathToUnityRelativePath(newPath);
                    if (m_localStorePath != editorConf.localStorePath)
                    {
                        editorConf.localStorePath = m_localStorePath;
                        AssetBuildSetting.Save();
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("   < Remote Url > : ", GUILayout.Width(160));
            var newAdress = EditorGUILayout.TextField(m_remoteAddress, GUILayout.ExpandWidth(true));
            if(newAdress != m_remoteAddress)
            {
                m_remoteAddress = newAdress;
                if (m_remoteAddress != usageConf.remoteAddress)
                {
                    usageConf.remoteAddress = m_remoteAddress;
                    AssetUsageSetting.Save();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawAllVersionList()
        {
            CommonGUI.SeparatorLine("Version List ：");

            EditorGUILayout.BeginVertical();
            //版本列表
            for (int i = 0; i < versionList.Count; i++)
            {
                DrawOneVersionOptions(versionList[i]);
            }

            //操作按钮
            if (m_CreateNewVersion == null)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("+ New Version"))
                {
                    m_CreateNewVersion = GetNextMaxVersion();
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                DrawCreateNewVersionLine();
            }

            EditorGUILayout.EndVertical();
        }

        private Version GetNextMaxVersion()
        {
            if (versionList.Count > 0)
            {
                versionList.Sort();
                var ver = versionList[0].manifest.version;
                return new Version(ver.Major, ver.Minor, ver.Build + 1);
            }
            return new Version(DateTime.Now.Year, DateTime.Now.Month * 100 + DateTime.Now.Day, 0);
        }

        private void AddNewVersion()
        {
            if (versionList.Count > 0)
            {
                versionList.Sort();
                var masVer = versionList[0];
                if (masVer.manifest.version >= m_CreateNewVersion)
                {
                    EditorUtility.DisplayDialog("VersionCompare", "版本号过低！", "OK");
                    return;
                }
            }

            string path = Utility.GetAssetsStoragePath();
            var directoryInfo = Directory.CreateDirectory(path + "/" + m_CreateNewVersion.ToString());
            versionList.Insert(0, CreateNewVersion(directoryInfo));
            m_CreateNewVersion = null;
        }

        private void DelOneVersion(VersionPackageData data)
        {
            if (data == null) return;
            versionList.Remove(data);
            Directory.Delete(data.directory.FullName,true);
        }

        private void TryLoadLocalVersionList()
        {
            versionList.Clear();
            string path = Utility.GetAssetsStoragePath();
            DirectoryInfo directoryInfo = Directory.CreateDirectory(path);
            foreach (var dir in directoryInfo.GetDirectories())
            {
                try
                {
                    var versionData = CreateNewVersion(dir);
                    versionList.Add(versionData);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            versionList.Sort();
        }

        private VersionPackageData CreateNewVersion(DirectoryInfo directory)
        {
            var versionData = new VersionPackageData();
            versionData.directory = directory;
            versionData.manifest = new VersionManifest();
            string manifestPath = Path.Combine(directory.FullName, VersionManifest.FileName);
            if (!File.Exists(manifestPath))
            {
                versionData.manifest.version = new Version(directory.Name);
                versionData.manifest.dateTime = DateTime.Now;
                versionData.manifest.WriteToFile(directory.FullName);
            }
            else
            {
                versionData.manifest.ReadFromFileIO(directory.FullName);
            }
            return versionData;
        }

        private void DrawOneVersionOptions(VersionPackageData data)
        {
            var editorConf = AssetBuildSetting.Instance;
            var usageConf = AssetUsageSetting.Instance;
            var version = data.manifest.version;
            var dir = data.directory;
            EditorGUILayout.BeginHorizontal();

            GUILayout.Label(string.Format("Version: {0}", version), GUILayout.Width(160));

            if (GUILayout.Button("-",GUILayout.Width(20))) DelOneVersion(data);

            if (GUILayout.Button("Import Local")) MakeAssetVersionPackages(data);

            if (GUILayout.Button("Make Zip")) MakeOneVersionAppZipPackages(data);

            if (GUILayout.Button("Upload Remote"))
            {
                _ = PackageUploader.UploadPackageAsync(dir.FullName);
            }

            if (GUILayout.Button("Download Remote")) { }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawCreateNewVersionLine()
        {   
            EditorGUILayout.BeginHorizontal();

            GUILayout.Label(" Version : ", GUILayout.Width(160));
            int major = m_CreateNewVersion.Major;
            int minor = m_CreateNewVersion.Minor;
            int build = m_CreateNewVersion.Build;
            GUILayout.Label("Major =", GUILayout.Width(50));
            major = EditorGUILayout.IntField(major, GUILayout.Width(100));
            GUILayout.Label("Minor =", GUILayout.Width(50));
            minor = EditorGUILayout.IntField(minor, GUILayout.Width(100));
            GUILayout.Label("Build =", GUILayout.Width(50));
            build = EditorGUILayout.IntField(build, GUILayout.Width(100));

            if(major != m_CreateNewVersion.Major || minor != m_CreateNewVersion.Minor ||
                build != m_CreateNewVersion.Build)
            {
                m_CreateNewVersion = new Version(major, minor, build);
            }
            
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Confirm")) AddNewVersion();

            if (GUILayout.Button("Cancle")) m_CreateNewVersion = null;

            EditorGUILayout.EndHorizontal();
        }

        #region 生成资源版本包

        //1 从打包输出目录拷贝packages
        public void MakeAssetVersionPackages(VersionPackageData data, bool showDailoge = true)
        {
            var packagesDir = Directory.CreateDirectory(Utility.GetAssetBundleOutputPath());

            VersionManifest manifest = data.manifest;
            var packageArray = packagesDir.GetDirectories();
            for (int i = 0; i < packageArray.Length; i++)
            {
                var packageDir = packageArray[i];
                GenerateAllAssetData(packageDir.Name, packageDir, ref manifest);
                string content = string.Format("Generatea Package:[{0}] Manifest File ...", packageDir.Name);
                EditorUtility.DisplayProgressBar("Make Version Packages", content, (i + 1) * 1.0f / packageArray.Length);
            }
            EditorUtility.ClearProgressBar();
            manifest.WriteToFile(packagesDir.FullName);

            //CopyTo New Version Floder
            FileIO.CopyDictionary(packagesDir.FullName, data.directory.FullName, ".manifest");
            if (showDailoge) EditorUtility.DisplayDialog("Make Version Packages", "资源版本文件生成完成！", "OK");
        }

        private void GenerateAllAssetData(string packageName, DirectoryInfo directory, ref VersionManifest manifest)
        {
            foreach (var fileInfo in directory.GetFiles())
            {
                string fullName = fileInfo.FullName.Replace("\\", "/");
                int indexof = fullName.IndexOf(packageName);
                string name = fullName.Substring(indexof);
                if (Path.GetExtension(name).Equals(".manifest")) continue;
                long size = fileInfo.Length;
                string md5 = FileIO.GetMD5(fileInfo.FullName);
                manifest.Add(name, md5, size);
            }
            foreach (var dirInfo in directory.GetDirectories())
            {
                GenerateAllAssetData(packageName, dirInfo, ref manifest);
            }
        }

        #endregion

        #region 生成版本的App压缩包

        public void MakeOneVersionAppZipPackages(VersionPackageData data, bool showDailoge = true)
        {
            //2 压缩方案Copy 文件清单
            string assetZipFile = string.Format("asset-{0}.zip", data.manifest.version);
            string outFilePath = Path.Combine(data.directory.FullName, assetZipFile).Replace('\\', '/');
            CompressBuildContent(data.directory.FullName, outFilePath);
            EditorUtility.DisplayDialog("Make Version AppPackages", "App资源版本文件生成完成！", "OK");
        }

        private static void CompressBuildContent(string buildDirPath, string outFilePath)
        {
            try
            {
                var task = Compression.StartCompressAtEditor(
                    buildDirPath, outFilePath, CompressionWay.Zip);
                task.Start();
                task.Dispose();
                task = null;
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("Compress Builds", e.Message, "OK");
                return;
            }
            EditorUtility.DisplayDialog("Compress Builds", "压缩包完成！", "OK");
        }

        #endregion
    }
}
