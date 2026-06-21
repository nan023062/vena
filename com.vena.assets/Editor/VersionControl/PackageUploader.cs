// -----------------------------------------------------------------------------
// Vena Assets
// Unity asset pipeline toolkit (AssetBundle / VersionControl / OSS / Compression).
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;

namespace Vena.Assets.Editor
{
    /// <summary>
    /// Editor-time package upload flow. Resolves an <see cref="IOssClient"/>
    /// via <see cref="AssetToolkitProvider"/> and pushes a version package
    /// (manifest + every file listed in the manifest) to the configured
    /// remote. Replaces the legacy static upload helper from the original repo.
    /// </summary>
    public static class PackageUploader
    {
        // OSS sub-directory used for asset packages. Kept as a constant so the
        // package has zero hard-coded endpoint / credential / bucket info; the
        // physical bucket is whatever the injected IOssClient is bound to.
        private const string OssSubDir = "assets";

        public static async Task UploadPackageAsync(string accessLocalstorageDir)
        {
            if (string.IsNullOrEmpty(accessLocalstorageDir))
                throw new ArgumentException("accessLocalstorageDir is required.", nameof(accessLocalstorageDir));

            string platform = Utility.GetPlatformName();
            string localPath = FileIO.FormatToUnityPath(accessLocalstorageDir);
            int idx = localPath.IndexOf(platform, StringComparison.Ordinal);
            if (idx < 0)
            {
                EditorUtility.DisplayDialog("Upload Version Packages",
                    $"Local path '{accessLocalstorageDir}' does not contain platform segment '{platform}'.",
                    "OK");
                return;
            }
            string platformVer = localPath.Substring(idx).Replace("/", "_");

            var manifest = new VersionManifest();
            manifest.ReadFromFileIO(accessLocalstorageDir);

            int totalNum = manifest.Lines.Count + 1;
            int index = 1;

            IOssClient client = AssetToolkitProvider.CreateOssClient();

            try
            {
                // 1. manifest itself
                string manifestFile = Path.Combine(accessLocalstorageDir, VersionManifest.FileName);
                EditorUtility.DisplayProgressBar(
                    "Upload Version Package",
                    "Upload... [VersionManifest.txt]",
                    index * 1.0f / totalNum);

                var manifestKey = OssObjectKey.Compose(OssSubDir, platformVer, VersionManifest.FileName);
                await client.UploadFileAsync(manifestKey, FileIO.FormatToSysFilePath(manifestFile));
                index++;

                // 2. every file referenced by the manifest
                foreach (var entry in manifest.Lines)
                {
                    string filePath = Path.Combine(accessLocalstorageDir, entry.Key);
                    EditorUtility.DisplayProgressBar(
                        "Upload Version Package",
                        $"Upload... [{entry.Key}]",
                        index * 1.0f / totalNum);

                    var key = OssObjectKey.Compose(OssSubDir, platformVer, entry.Key);
                    await client.UploadFileAsync(key, FileIO.FormatToSysFilePath(filePath));
                    index++;
                }

                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog(
                    "Upload Version Packages",
                    $"Upload completed.\nTotal count: {totalNum}",
                    "OK");
            }
            catch (Exception ex)
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog(
                    "Upload Version Packages",
                    $"Upload failed.\nMessage: {ex.Message}\nStackTrace: {ex.StackTrace}",
                    "OK");
            }
            finally
            {
                (client as IDisposable)?.Dispose();
            }
        }
    }
}
