// -----------------------------------------------------------------------------
// Vena Assets
// Unity asset pipeline toolkit (AssetBundle / VersionControl / OSS / Compression).
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

//using System;
//using UnityEngine;
//using Nave;
//using System.Net;
//using System.ComponentModel;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine.Networking;

//namespace Vena.Assets
//{
//    /// <summary>
//    /// 下载资源数据
//    /// </summary>
//    public class DownloadAssetData
//    {
//        public int index;
//        public string fileName;
//        public string downloadUrl;
//        public string saveToPath;
//        public ulong nBytes;
//    }

//    /// <summary>
//    /// AB下载器
//    /// 1 即支持本地，也支持服务器
//    /// 2 支持多文件下载
//    /// </summary>
//    public class AssetDownLoader
//    {
//        //下载的数据
//        private List<DownloadAssetBundleData> m_downloadInfos = null;
//        //未下载的资源索引
//        private List<int> m_leftUnloadIndeices = null;
//        //需要下载总字节数
//        private ulong m_downloadTotalBytes = 0;
//        //需要下载文件总数
//        public int totalNum
//        {
//            private set;
//            get;
//        }
//        //当前下载文件数量
//        public int current
//        {
//            private set;
//            get;
//        }
//        //默认下载速度（xxB/s）
//        const ulong c_downloadSpeed = 1024 * 1024;//300kb/s
//        //当前预估下载字节数（用于平滑下载进度条）
//        private float m_preDownloadedByts = 0f;
//        //开始下载时间
//        private float m_beginDownloadTime = 0f;
//        //当前平均速度
//        public ulong downloadSpeed { get; private set; }
//        //当前已经下载的字节数
//        public ulong downloadedBytes
//        {
//            private set;
//            get;
//        }
//        //当前正在下载的字节
//        private ulong m_downloadingBytes = 0;
//        //最大同步激活的下载器数
//        private const int c_max_loader = 10;
//        //当前运行的下载器
//        private List<UnityWebDownLoadBundle> m_runningDownloaders = null;

//        public float progress = 1f;

//        //当前已经下载的字节数描述
//        public string downloadBytesDesc
//        {
//            get
//            {
//                return GameUtility.GetByteString((ulong)(progress * m_downloadTotalBytes));
//            }
//        }

//        public MultiBundleDownLoader()
//        {
//            m_runningDownloaders = new List<UnityWebDownLoadBundle>();
//            m_downloadInfos = null;
//        }

//        //开始下载
//        public MultiBundleDownLoader StartDownload(List<DownloadAssetBundleData> downloadInfos)
//        {
//            m_downloadInfos = downloadInfos;
//            totalNum = downloadInfos.Count;
//            m_downloadTotalBytes = 0;
//            m_preDownloadedByts = 0f;
//            m_downloadingBytes = 0;
//            downloadedBytes = 0;
//            current = 0;
//            progress = 0;

//            m_leftUnloadIndeices = new List<int>();

//            for (int i = 0; i < downloadInfos.Count; i++)
//            {
//                var downloadInfo = downloadInfos[i];
//                downloadInfo.index = i;
//                m_leftUnloadIndeices.Add(i);
//                m_downloadTotalBytes += downloadInfo.nBytes;
//            }
//            m_beginDownloadTime = Time.time;
//            return this;
//        }

//        protected bool Complete()
//        {
//            //尝试开启新的下载器
//            if (c_max_loader > m_runningDownloaders.Count)
//            {
//                if (m_downloadInfos.Count > 0)
//                {
//                    var downloadInfo = m_downloadInfos[0];
//                    m_downloadInfos.RemoveAt(0);


//                    //var task = ResourceManager.Instance.Spawn<FileUnityWebDownloader>();
//                    //m_downloadingBytes += task.downloadBytes;
//                    //task.StartDownload(downloadInfo);
//                    //m_runningDownloaders.Add(task);
//                }
//            }

//            //检测运行的加载器
//            int count = m_runningDownloaders.Count;
//            for (int i = count - 1; i >= 0; i--)
//            {
//                var task = m_runningDownloaders[i];
//                if (task.InDone())
//                {
//                    m_downloadingBytes -= task.downloadBytes;
//                    downloadedBytes += task.downloadBytes;

//                    m_leftUnloadIndeices.Remove(task.index);
//                    current = totalNum - m_leftUnloadIndeices.Count;

//                    this.Log("done bytes : name = {0}, {1} ! ({2} / {3})", task.fileName, GameUtility.GetByteString(task.downloadBytes), current, totalNum);
//                    m_runningDownloaders.RemoveAt(i);
//                    task.Dispose();
//                }
//            }

//            //预计算下载数
//            m_preDownloadedByts = m_preDownloadedByts + Time.deltaTime * c_downloadSpeed;
//            m_preDownloadedByts = Mathf.Clamp(m_preDownloadedByts, downloadedBytes, m_downloadingBytes + downloadedBytes);

//            //当前平均下载速速
//            if (Time.time > m_beginDownloadTime)
//                downloadSpeed = (ulong)(m_preDownloadedByts / (Time.time - m_beginDownloadTime));

//            //平滑过渡进度值
//            float acc = (m_downloadingBytes > 0) ? (1 - ((m_downloadingBytes + downloadedBytes - m_preDownloadedByts) / (float)m_downloadingBytes)) : 0;
//            progress = Mathf.Lerp(progress, (m_preDownloadedByts / m_downloadTotalBytes), Time.deltaTime * Mathf.Pow(acc, 3) * 5f);

//            //全部下载完成的条件
//            if (current >= totalNum && progress > 0.99995f)
//            {
//                progress = 1f;
//                return true;
//            }
//            return false;
//        }

//        public void Dispose()
//        {
//            foreach (var download in m_runningDownloaders)
//                download.Dispose();
//            m_runningDownloaders.Clear();

//            current = 0;
//            totalNum = 0;
//            m_downloadInfos.Clear();
//        }
//    }
//}
