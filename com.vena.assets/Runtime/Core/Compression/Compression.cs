// -----------------------------------------------------------------------------
// Vena Assets
// Unity asset pipeline toolkit (AssetBundle / VersionControl / OSS / Compression).
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

/**********************************************************************************
 * FileName:    Compress.cs
 * Description: 资源管理框架---压缩和解压工具
 * History: 2019-07-09
 *********************************************************************************/
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Vena.Assets
{ 
    public static class Compression
    {
        #region 压缩和解压函数

        public static void DoZipCompress(string rootDirectory, string[] doZipFileList, string toZipFile)
        {
            using (ZipOutputStream compressStream = new ZipOutputStream(File.Create(toZipFile)))
            {
                compressStream.SetLevel(9);    // 9 - means best compression
                                               //Crc32 crc = new Crc32();
                int count = doZipFileList.Length;
                for (int i = 0; i < count; i++)
                {
                    string doZipFile = doZipFileList[i];
                    FileStream fs = File.OpenRead(doZipFile);
                    byte[] buffer = new byte[fs.Length];
                    fs.Read(buffer, 0, buffer.Length);
                    string relativePath = doZipFile.Substring(rootDirectory.Length);
                    ZipEntry entry = new ZipEntry(relativePath);
                    entry.DateTime = DateTime.Now;
                    entry.Size = fs.Length;
                    fs.Close();
                    compressStream.PutNextEntry(entry);
                    compressStream.Write(buffer, 0, buffer.Length);
                }
            }
        }

        public static void DoZipDecompress(string zipFile, string saveToPath)
        {
            FileInfo fileInfo = new FileInfo(zipFile);
            long fileSize = fileInfo.Length;

            long completeSize = 0;

            using (ZipInputStream s = new ZipInputStream(File.OpenRead(zipFile)))
            {
                if (string.IsNullOrEmpty(saveToPath)) return;

                if (!saveToPath.EndsWith("/")) saveToPath += "/";

                if (!Directory.Exists(saveToPath))
                    Directory.CreateDirectory(saveToPath);

                ZipEntry entry;
                while ((entry = s.GetNextEntry()) != null)
                {
                    string directoryName = Path.GetDirectoryName(entry.Name);
                    string fileName = Path.GetFileName(entry.Name);
                    if (!string.IsNullOrEmpty(directoryName))
                    {
                        Directory.CreateDirectory(saveToPath + directoryName);
                    }
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        using (FileStream streamWriter = File.Create(saveToPath + entry.Name))
                        {
                            int size = 1024;
                            byte[] data = new byte[size];
                            while (true)
                            {
                                size = s.Read(data, 0, data.Length);
                                if (size > 0)
                                {
                                    streamWriter.Write(data, 0, size);
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }
                    completeSize += entry.Size;
                }
            }
        }

        #endregion

        private static readonly Queue<CompressionTask> MWaitingTasks = new Queue<CompressionTask>();
        
        private static Thread _thread;

        /// <summary>
        /// Compress the directory
        /// </summary>
        public static CompressionTask StartCompress(string directory, string output, CompressionWay way)
        {
            directory = directory.Replace("\\", "/");
            output = output.Replace("\\", "/");
            if (!directory.EndsWith("/"))
            {
                directory += "/";
            }
            CompressionTask task = new CompressionTask();
            task.SetTask(directory, output, CompressionTaskType.Compress, way);
            AddTask(task);
            return task;
        }

#if UNITY_EDITOR
        public static CompressionTask StartCompressAtEditor(string directory, string output, CompressionWay way)
        {
            //Debuger.DebugLog("Add compression task");
            directory = directory.Replace("\\", "/");
            output = output.Replace("\\", "/");
            if (!directory.EndsWith("/"))
            {
                directory += "/";
            }
            CompressionTask task = new CompressionTask();
            task.SetTask(directory, output, CompressionTaskType.Compress, way);
            return task;
        }
#endif

        public static CompressionTask StartDecompress(string zipPath, string outFile, CompressionWay way)
        {
            zipPath = zipPath.Replace("\\", "/");
            CompressionTask task = new CompressionTask();
            task.SetTask(zipPath, outFile, CompressionTaskType.Decompress, way);
            AddTask(task);
            return task;
        }

        private static void AddTask(CompressionTask task)
        {
            lock (MWaitingTasks)
            {
                MWaitingTasks.Enqueue(task);
            }
            if (_thread == null)
            {
                _thread = new Thread(new ThreadStart(Compressing));
            }
            _thread.Start();
        }

        static void Compressing()
        {
            CompressionTask task = null;
            lock (MWaitingTasks)
            {
                if (MWaitingTasks.Count > 0)
                {
                    task = MWaitingTasks.Dequeue();
                }
                else
                {
                    return;
                }
            }
            task.Start();
            Compressing();
        }
    }
}
