// -----------------------------------------------------------------------------
// Vena Assets
// Unity asset pipeline toolkit (AssetBundle / VersionControl / OSS / Compression).
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Vena.Assets
{
    public enum CompressionTaskState
    {
        /// <summary>
        /// 压缩解压采取队列方式一个一个处理
        /// 所以解压不能立即执行，会处于等待状态
        /// </summary>
        Waiting,

        /// <summary>
        /// 处理中
        /// </summary>
        Porcessing,

        /// <summary>
        /// 出现错误
        /// 当解压出错时并得到错误信息后需要手动调用dispose方法进行回收
        /// </summary>
        Error,

        /// <summary>
        /// 解压结束
        /// 当解压结束时会自动回收任务对象，不需要手动调用dispose方法回收
        /// </summary>
        End,        //结束
    }

    public enum CompressionTaskType
    {
        Compress = 0,
        Decompress = 1,
    }

    public enum CompressionWay
    {
        Zip = 0,
        /// <summary>
        /// 7Z方式先把所有文件流写入到一个temp文件，再压缩这个temp文件，解压则反过来
        /// </summary
        SevenZip = 1,
    }

    public class CompressionTask : IEnumerator
    {
        public CompressionTaskState State { get; private set; }
        public float Progress;
        public string Error { get; private set; }

        public object Current
        {
            get { return null; }
        }
        public bool MoveNext()
        {
            return Progress < 1f;
        }
        public void Reset()
        {
            Progress = 0f;
        }

        private bool m_Disposed;
        private CompressionTaskType m_TaskType;
        private CompressionWay m_Way;

        private string m_FilePath;
        private string m_OutputPath;

        private int m_TotalCount;
        private int m_CompleteCount;

        public void Dispose()
        {
            m_OutputPath = null;
            m_FilePath = null;
            m_CompleteCount = 0;
        }

        public void SetTask(string sourcePath, string outputPath, CompressionTaskType type, CompressionWay way)
        {
            m_FilePath = sourcePath;
            m_OutputPath = outputPath;
            m_TaskType = type;
            m_Way = way;
        }

        public void Start()
        {
            try
            {
                switch (m_TaskType)
                {
                    case CompressionTaskType.Compress:
                        Compress();
                        break;
                    case CompressionTaskType.Decompress:             //1是解压
                        Decompress();
                        break;
                }
                State = CompressionTaskState.End;
            }
            catch (Exception e)
            {
                OnErrorOccurred(e.Message);
            }
        }

        private void Compress()
        {
            try
            {
                if (File.Exists(m_OutputPath))
                {
                    File.Delete(m_OutputPath);
                }
                switch (m_Way)
                {
                    case CompressionWay.Zip:
                        DoZipCompress();
                        break;
                    case CompressionWay.SevenZip:
                        Do7ZCompress();
                        break;
                }
            }
            catch (Exception e)
            {
                if (File.Exists(m_OutputPath))
                {
                    File.Delete(m_OutputPath);
                }
                throw e;
            }
        }

        private void Decompress()
        {
            try
            {
                switch (m_Way)
                {
                    case CompressionWay.Zip:
                        DoZipDecompress();
                        break;
                    case CompressionWay.SevenZip:
                        break;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void OnErrorOccurred(string msg)
        {
            State = CompressionTaskState.Error;
            Error = msg;
        }

        #region 普通的zip方式
        private void DoZipCompress()
        {
            using (ZipOutputStream compressStream = new ZipOutputStream(File.Create(m_OutputPath)))
            {
                compressStream.SetLevel(9);    // 9 - means best compression
                                               //Crc32 crc = new Crc32();
                string[] fileList = FileIO.GetAllFilesAtDirectory(m_FilePath);
                m_TotalCount = fileList.Length;
                for (int i = 0; i < fileList.Length; i++)
                {
                    string fileFullName = fileList[i];
                    FileStream fs = File.OpenRead(fileFullName);
                    byte[] buffer = new byte[fs.Length];
                    fs.Read(buffer, 0, buffer.Length);
                    string relativeFileName = fileFullName.Substring(m_FilePath.Length);
                    ZipEntry entry = new ZipEntry(relativeFileName);
                    entry.DateTime = DateTime.Now;
                    entry.Size = fs.Length;
                    fs.Close();
                    compressStream.PutNextEntry(entry);
                    compressStream.Write(buffer, 0, buffer.Length);
                    m_CompleteCount++;
                }
            }
        }

        private void DoZipDecompress()
        {
            FileInfo fileInfo = new FileInfo(m_FilePath);
            long fileSize = fileInfo.Length;

            long completeSize = 0;
            //Debuger.DebugLog("Decompress {0} to {1} ", m_FilePath, m_OutputPath);
            using (ZipInputStream s = new ZipInputStream(File.OpenRead(m_FilePath)))
            {
                //Create Decompress Directory 
                if (string.IsNullOrEmpty(m_OutputPath))
                {
                    return;
                }
                if (!m_OutputPath.EndsWith("/"))
                {
                    m_OutputPath += "/";
                }
                if (!Directory.Exists(m_OutputPath))
                {
                    Directory.CreateDirectory(m_OutputPath);
                }

                ZipEntry entry;
                while ((entry = s.GetNextEntry()) != null)
                {
                    string directoryName = Path.GetDirectoryName(entry.Name);
                    string fileName = Path.GetFileName(entry.Name);
                    if (!string.IsNullOrEmpty(directoryName))
                    {
                        Directory.CreateDirectory(m_OutputPath + directoryName);
                    }
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        using (FileStream streamWriter = File.Create(m_OutputPath + entry.Name))
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
                    Progress = (float)completeSize / fileSize;

                }
            }
        }
        #endregion

        #region 7Z方式

        private void Do7ZCompress()
        {
        }

        private void Do7ZDecompress()
        {

        }

        #endregion

    }
}