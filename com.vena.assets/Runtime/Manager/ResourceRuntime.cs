// -----------------------------------------------------------------------------
// Vena Assets
// Unity asset pipeline toolkit (AssetBundle / VersionControl / OSS / Compression).
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Vena.Assets
{
    public static partial class ResourceRuntime
    {
        private static readonly Behaviour behaviour;
        private static bool _initialized;

        private static int _assignStringHashCode;
        private static short _assignPackageId;
        private static readonly Dictionary<string, int> StringHashCodes = new Dictionary<string, int>();
        private static readonly Dictionary<string, short> PackageNameIds = new Dictionary<string, short>();
        private static readonly Dictionary<int, Package> Packages = new Dictionary<int, Package>();
        private static readonly Dictionary<IResLoader, HashSet<Handle>> Loaders = new Dictionary<IResLoader, HashSet<Handle>>();
        
        static ResourceRuntime()
        {
            var go = new GameObject("Asset Toolkit", typeof(Behaviour));
            behaviour = go.GetComponent<Behaviour>();
            go.hideFlags = HideFlags.NotEditable;
            UnityEngine.Object.DontDestroyOnLoad(go);
            _initialized = false;
            _assignStringHashCode = _assignPackageId = 0;
            StringHashCodes.Clear();
            
            if (Utility.UseAssetBundle)
                Log($"AB方式加载 ! 路径：{Utility.GetPersistentDataPath()}");
            else
                Log($"Editor加载 ! 路径：{Utility.GameAssetInputPath()}");
        }
        
        private class Behaviour : MonoBehaviour
        {
            private void LateUpdate()
            {
                foreach (var keyValue in Packages)
                {
                    keyValue.Value.Update();
                }
            }
        }
        
        public static IResLoader[] GetAllLoaders() => Loaders.Keys.ToArray();
        
        private static Package GetPackage(string package)
        {
            return GetPackage(PackageNameToID(package));
        }
        
        private static Package GetPackage(int package)
        {
            Packages.TryGetValue(package, out var bundlePackage);
            return bundlePackage;
        }
        
        public static int StringToHash(string str)
        {
            if (!StringHashCodes.TryGetValue(str, out int hashCode))
            {
                hashCode = ++_assignStringHashCode;
                StringHashCodes.Add(str, hashCode);
            }
            return hashCode;
        }
        
        public static short PackageNameToID(string name)
        {
            if (!PackageNameIds.TryGetValue(name, out short hashCode))
            {
                hashCode = ++_assignPackageId;
                PackageNameIds.Add(name, hashCode);
            }
            return hashCode;
        }
        
        public static string PackageIDToName(short packageId)
        {
            if (Packages.TryGetValue(packageId, out var package))
            {
                return package.name;
            }
            return "null";
        }
        
        internal static Coroutine StartCoroutine(IEnumerator enumerator)
        {
            return behaviour.StartCoroutine(enumerator);
        }
        
        internal static void Log(string content)
        {
            Debug.Log($"[AssetToolKit] : {content}");
        }
        
        internal static void LogError(string content)
        {
            Debug.LogError($"[AssetToolKit] : {content}");
        }
        
        public static void Initialize()
        {
            if (_initialized)
            {
                _initialized = false;
                
                foreach (var package in Packages.Values)
                    package.Dispose();
            }
            
            Packages.Clear();
            Loaders.Clear();
            PackageNameIds.Clear();
            
            var settings = AssetBuildSetting.Instance;
           
            float beginTime = Time.realtimeSinceStartup;
            foreach (var packageInfo in settings.assetPackages)
            {
                float startTime = Time.realtimeSinceStartup;
                short packageId = PackageNameToID(packageInfo.packageName);
                var package = new Package(packageInfo.packageName, packageId);
                Packages.Add(package.id, package);
                Log($"init {package.name}_{package.id} ...  cost time:{Time.realtimeSinceStartup - startTime} ");
            }
            
            Log($"initialized , cost total time:{Time.realtimeSinceStartup - beginTime}");
            _initialized = true;
        }
    }
}
