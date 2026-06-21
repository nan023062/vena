// -----------------------------------------------------------------------------
// Vena Assets
// Unity asset pipeline toolkit (AssetBundle / VersionControl / OSS / Compression).
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

///**********************************************************************************
// * FileName:    RequestWebRequestBundle.cs
// * Description: 资源管理框架--- 异步任务（本地加载AB请求）
// *      使用WebRequest加载AssetBundle
// * History: 2019-07-09
// *********************************************************************************/
//using UnityEngine;
//using UnityEngine.Networking;

//namespace Vena.Assets
//{
//    public class RequestWebRequestBundle : TaskObject
//    {
//        private string mLoadFileUrl;

//        private UnityWebRequest mWebRequest;

//        public const string LoadPre = "file://";                                         //WWW加载前缀\
//        //获取加载路径 安卓层会自行添加http前缀这个需要规避掉
//        public static string GetLoadPath(string path)
//        {
//#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
//            return LoadPre + path;
//#endif
//            return path;
//        }

//        public AssetBundle assetBundle
//        {
//            get
//            {
//                return DownloadHandlerAssetBundle.GetContent(mWebRequest);
//            }
//        }

//        public override void Start()
//        {
//            m_progress = 0f;
//            mWebRequest = UnityWebRequestAssetBundle.GetAssetBundle(GetLoadPath(mLoadFileUrl));
//            mWebRequest.SendWebRequest();
//        }

//        public override void OnCreate()
//        {

//        }

//        public override void OnSpawn(params object[] args)
//        {
//            mLoadFileUrl = args[0] as string;
//        }

//        public override void OnUnspawn()
//        {
//            mWebRequest = null;
//        }

//        public override void OnDestroy()
//        {

//        }

//        public override float Progress()
//        {
//            return m_progress;
//        }

//        public override bool MoveNext()
//        {
//            if (mWebRequest == null) return true;

//            if (mWebRequest.isDone)
//            {
//                if (!string.IsNullOrEmpty(mWebRequest.error))
//                {
//                    this.LogError("Request Done. But Failed! error = {0} !", mWebRequest.error);
//                    m_progress = 1f;
//                    return false;
//                }
//                else if (m_progress < 1f)
//                {
//                    m_progress = 1f;
//                    return true;
//                }
//                else
//                {
//                    return false;
//                }
//            }
//            else
//            {
//                m_progress = mWebRequest.downloadProgress;
//                m_progress = Mathf.Min(0.999f, m_progress);
//                return true;
//            }
//        }
//    }
//}
