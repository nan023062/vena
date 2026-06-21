// -----------------------------------------------------------------------------
// Vena Assets
// Unity asset pipeline toolkit (AssetBundle / VersionControl / OSS / Compression).
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using UnityEngine;
using System.IO;
using System;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Text;
using System.Collections;
#pragma warning disable CS0618

namespace Vena.Assets
{
    public static class FileIO
    {
        public const string AssetsFolderName = "Assets";

        public static string FormatToUnityPath(string path)
        {
            return path.Replace("\\", "/");
        }

        public static string FormatToSysFilePath(string path)
        {
            return path.Replace("/", "\\");
        }

        public static string FullPathToAssetPath(string full_path)
        {
            full_path = FormatToUnityPath(full_path);
            if (!full_path.StartsWith(Application.dataPath))
            {
                return null;
            }
            string ret_path = full_path.Replace(Application.dataPath, "");
            return AssetsFolderName + ret_path;
        }

        public static string GetFileExtension(string path)
        {
            return Path.GetExtension(path).ToLower();
        }

        public static void CheckFileAndCreateDirWhenNeeded(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            FileInfo file_info = new FileInfo(filePath);
            DirectoryInfo dir_info = file_info.Directory;
            if (!dir_info.Exists)
            {
                Directory.CreateDirectory(dir_info.FullName);
            }
        }

        public static void CheckDirAndCreateWhenNeeded(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath))
            {
                return;
            }

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
        }

        public static bool SafeWriteAllBytes(string outFile, byte[] outBytes)
        {
            try
            {
                if (string.IsNullOrEmpty(outFile))
                {
                    return false;
                }

                CheckFileAndCreateDirWhenNeeded(outFile);
                if (File.Exists(outFile))
                {
                    File.SetAttributes(outFile, FileAttributes.Normal);
                }
                File.WriteAllBytes(outFile, outBytes);
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError(string.Format("SafeWriteAllBytes failed! path = {0} with err = {1}", outFile, ex.Message));
                return false;
            }
        }

        public static bool SafeWriteAllLines(string outFile, string[] outLines)
        {
            try
            {
                if (string.IsNullOrEmpty(outFile))
                {
                    return false;
                }

                CheckFileAndCreateDirWhenNeeded(outFile);
                if (File.Exists(outFile))
                {
                    File.SetAttributes(outFile, FileAttributes.Normal);
                }
                File.WriteAllLines(outFile, outLines);
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError(string.Format("SafeWriteAllLines failed! path = {0} with err = {1}", outFile, ex.Message));
                return false;
            }
        }

        public static bool SafeWriteAllText(string outFile, string text)
        {
            try
            {
                if (string.IsNullOrEmpty(outFile))
                {
                    return false;
                }

                CheckFileAndCreateDirWhenNeeded(outFile);
                if (File.Exists(outFile))
                {
                    File.SetAttributes(outFile, FileAttributes.Normal);
                }
                File.WriteAllText(outFile, text);

                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError(string.Format("SafeWriteAllText failed! path = {0} with err = {1}", outFile, ex.Message));
                return false;
            }
        }

        public static byte[] SafeReadAllBytes(string inFile)
        {
            try
            {
                if (string.IsNullOrEmpty(inFile))
                {
                    return null;
                }

                if (!File.Exists(inFile))
                {
                    return null;
                }

                File.SetAttributes(inFile, FileAttributes.Normal);
                return File.ReadAllBytes(inFile);
            }
            catch (System.Exception ex)
            {
                Debug.LogError(string.Format("SafeReadAllBytes failed! path = {0} with err = {1}", inFile, ex.Message));
                return null;
            }
        }

        public static string[] SafeReadAllLines(string inFile)
        {
            try
            {
                if (string.IsNullOrEmpty(inFile))
                {
                    return null;
                }

                if (!File.Exists(inFile))
                {
                    return null;
                }

                File.SetAttributes(inFile, FileAttributes.Normal);
                return File.ReadAllLines(inFile);
            }
            catch (System.Exception ex)
            {
                Debug.LogError(string.Format("SafeReadAllLines failed! path = {0} with err = {1}", inFile, ex.Message));
                return null;
            }
        }

        public static string SafeReadAllText(string inFile)
        {
            try
            {
                if (string.IsNullOrEmpty(inFile)) return null;

                if (!File.Exists(inFile)) return null;

                File.SetAttributes(inFile, FileAttributes.Normal);
                return File.ReadAllText(inFile);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"SafeReadAllText failed! path = {inFile} with err = {ex.Message}");
                return null;
            }
        }

        public static IEnumerator SafeReadAllBytes(string inFileUrl, Action<byte[]> onReaded)
        {
            using WWW www = new WWW(inFileUrl);
            yield return www;

            if (string.IsNullOrEmpty(www.error))
            {
                onReaded?.Invoke(www.bytes);
            }
            else
            {
                Debug.LogError($"SafeCopyFile Error : {www.error}");
            }
        }

        public static void DeleteDirectory(string dirPath)
        {
            string[] files = Directory.GetFiles(dirPath);
            string[] dirs = Directory.GetDirectories(dirPath);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(dirPath, false);
        }

        public static bool SafeDeleteDir(string folderPath)
        {
            try
            {
                if (string.IsNullOrEmpty(folderPath))
                {
                    return true;
                }

                if (Directory.Exists(folderPath))
                {
                    DeleteDirectory(folderPath);
                }
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError(string.Format("SafeDeleteDir failed! path = {0} with err: {1}", folderPath, ex.Message));
                return false;
            }
        }

        public static bool SafeDeleteFile(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    return true;
                }

                if (!File.Exists(filePath))
                {
                    return true;
                }
                File.SetAttributes(filePath, FileAttributes.Normal);
                File.Delete(filePath);
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError(string.Format("SafeDeleteFile failed! path = {0} with err: {1}", filePath, ex.Message));
                return false;
            }
        }

        public static bool SafeRenameFile(string sourceFileName, string destFileName)
        {
            try
            {
                if (string.IsNullOrEmpty(sourceFileName))
                {
                    return false;
                }

                if (!File.Exists(sourceFileName))
                {
                    return true;
                }
                File.SetAttributes(sourceFileName, FileAttributes.Normal);
                File.Move(sourceFileName, destFileName);
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError(string.Format("SafeRenameFile failed! path = {0} with err: {1}", sourceFileName, ex.Message));
                return false;
            }
        }

        public static bool SafeCopyFile(string fromFile, string toFile)
        {
            try
            {
                if (string.IsNullOrEmpty(fromFile))
                    return false;

                if (!File.Exists(fromFile)) return false;

                CheckFileAndCreateDirWhenNeeded(toFile);

                if (File.Exists(toFile))
                {
                    File.SetAttributes(toFile, FileAttributes.Normal);
                }
                File.Copy(fromFile, toFile, true);
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError(string.Format("SafeCopyFile failed! formFile = {0}, toFile = {1}, with err = {2}",
                    fromFile, toFile, ex.Message));
                return false;
            }
        }

        public static IEnumerator SafeCopyDirectory(string fromFile, string toFile, string fileListPath)
        {
            string content = "";
            yield return SafeReadAllBytes(fileListPath, (bytes) =>
            {
                content = UTF8Encoding.UTF8.GetString(bytes);
            });

            string[] allLines = content.Split('\n');
            for (int i = 1; i < allLines.Length; i++)
            {
                string copyFilePath = allLines[i].Split('|')[0];
                string copyFrom = Path.Combine(fromFile, copyFilePath);
                string copyTo = Path.Combine(toFile, copyFilePath);
                yield return SafeReadAllBytes(copyFrom, (bytes) =>
                      { 
                            SafeWriteAllBytes(copyTo, bytes);
                        });
            }
        }

        public static string GetMD5(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return string.Empty;
            }
            try
            {
                FileStream fs = new FileStream(filePath, FileMode.Open);
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(fs);
                fs.Close();
                fs.Dispose();
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                Debug.LogError($"FileIO.GetMD5(): {ex.Message}");
                return string.Empty;
            }
        }

        public static string[] GetAllFilesAtDirectory(string dirRoot, string[] ignoreExts = null)
        {
            List<string> arrays = new List<string>();
            string[] files = Directory.GetFiles(dirRoot);
            string[] dirs = Directory.GetDirectories(dirRoot);

            for (int i = 0; i < files.Length; i++)
            {
                string ext = Path.GetExtension(files[i]);
                bool ignored = false;
                if (ignoreExts != null)
                {
                    foreach (var _ext in ignoreExts)
                    {
                        if (ext.Equals(_ext))
                        {
                            ignored = true;
                            break;
                        }
                    }
                }
                if (ignored) continue;
                arrays.Add(files[i].Replace('\\', '/'));
            }

            for (int i = 0; i < dirs.Length; i++)
            {
                arrays.AddRange(GetAllFilesAtDirectory(dirs[i], ignoreExts));
            }
            return arrays.ToArray();
        }

        public static void CopyDictionary(string from, string to, params string[] ignoreExts)
        {
            if (!from.EndsWith("/"))
            {
                from += "/";
            }
            if (!to.EndsWith("/"))
            {
                to += "/";
            }
            if (Directory.Exists(to))
            {
                Directory.CreateDirectory(to);
            }

            string[] files = GetAllFilesAtDirectory(from, ignoreExts);
            int totalFileCount = files.Length;
            string outFile = string.Empty;
            string outPath = string.Empty;
            string fileName = string.Empty;
            for (int i = 0; i < totalFileCount; i++)
            {
                fileName = files[i].Substring(from.Length);           //相对路径
                byte[] temp = File.ReadAllBytes(files[i]);
                outFile = to + fileName;

                //创建路径
                outPath = outFile.Substring(0, outFile.LastIndexOf("/"));
                Directory.CreateDirectory(outPath);
                File.WriteAllBytes(outFile, temp);
            }
        }

        //获取字节数的描述
        public static string GetByteString(ulong nBytes)
        {
            if ((nBytes >> 20) > 0)
            {
                return string.Format("{0:0.0}MB", (nBytes / 1024f / 1024f));
            }
            else if ((nBytes >> 10) > 0)
            {
                return string.Format("{0:0.0}KB", (nBytes / 1024f));
            }
            else
            {
                return string.Format("{0}B", nBytes);
            }
        }

        //获取字节数的描述
        public static string GetByteStringNoPoint(ulong nBytes)
        {
            if ((nBytes >> 20) > 0)
            {
                return string.Format("{0}MB", (nBytes >> 20));
            }
            else if ((nBytes >> 10) > 0)
            {
                return string.Format("{0}KB", (nBytes >> 10));
            }
            else
            {
                return string.Format("{0}B", nBytes);
            }
        }

        public static void CopyBytes(byte[] copyTo, int offsetTo, byte[] copyFrom, int offsetFrom, int count)
        {
            Array.Copy(copyFrom, offsetFrom, copyTo, offsetTo, count);
        }

        public static byte[] StringToBytes(string str)
        {
            return System.Text.Encoding.Default.GetBytes(str);
        }

        public static byte[] StringToUTFBytes(string str)
        {
            return System.Text.Encoding.UTF8.GetBytes(str);
        }

        public static string BytesToString(byte[] bytes)
        {
            return System.Text.Encoding.Default.GetString(bytes).Trim();
        }

        public static Hashtable HttpGetInfo(string info)
        {
            if (string.IsNullOrEmpty(info))
            {
                return null;
            }

            Hashtable table = new Hashtable();
            string[] paramList = info.Split('&');
            for (int i = 0; i < paramList.Length; i++)
            {
                string[] keyAndValue = paramList[i].Split('=');
                if (keyAndValue.Length >= 2)
                {
                    if (!table.ContainsKey(keyAndValue[0]))
                    {
                        table.Add(keyAndValue[0], keyAndValue[1]);
                    }
                }
            }

            return table;
        }
    }
}
