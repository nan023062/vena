// -----------------------------------------------------------------------------
// Vena Assets
// Unity asset pipeline toolkit (AssetBundle / VersionControl / OSS / Compression).
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using UnityObject = UnityEngine.Object;

namespace Vena.Assets
{
    public sealed partial class Package
    {
        const int MAX_RECURSUVE_DEPTH = 5;
        const int MAX_LOADING_COUNT = 10;

        public readonly string rootPath, bundlePath;
        public readonly string name;
        public readonly short id;
        public readonly bool useBundle;
        
        private readonly AssetManifest _manifest;
        private readonly PriorityQueue<ObjectRef> _loadQueue;
        private ObjectRef[] _loadingList;
        private uint _assignToken;
        
        //构造函数
        public Package(string name, short id)
        {
            this.id = id;
            this.name = name;
            useBundle = Utility.UseAssetBundle;
            _assignToken = 0;

            rootPath = Utility.GetAssetPackagePath(name);
            bundlePath = Path.Combine(Utility.GetPersistentDataPath(), name);
            
            _objectCache = new Dictionary<ulong, ObjectRef>();
            _loadQueue = new PriorityQueue<ObjectRef>();
            
            _tokenToObjectIds = new Dictionary<uint, ulong>();
            _loadingList = new ObjectRef[MAX_LOADING_COUNT];
            
            //加载manifest文件和mapping文件
            AssetBundleManifest abManifest = null;
            AssetList mapping = null;
            if (useBundle)
            {
                //加载资源依赖清单文件
                string manifestFileUrl = Path.Combine(rootPath, name);

                AssetBundle bundle = AssetBundle.LoadFromFile(manifestFileUrl);

                abManifest = bundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");

                bundle.Unload(false);

                string mappingAssetName = (name + Utility.AssetBundleMapping);

                string bundleName = mappingAssetName.ToLower();

                string mappingAssetURL = Utility.GetBundleFullPath(rootPath, bundleName, true);

                bundle = AssetBundle.LoadFromFile(mappingAssetURL);

                mapping = bundle.LoadAsset<AssetList>(mappingAssetName);

                bundle.Unload(false);
            }

            _manifest = new AssetManifest(name, abManifest, mapping);

            if (abManifest) Resources.UnloadAsset(abManifest);

            if (mapping) Resources.UnloadAsset(mapping);
        }

        internal void Update()
        {
            for (int i = 0; i < MAX_LOADING_COUNT; i++)
            {
                var objectRef = _loadingList[i];
                bool addNew = false;
                if (null == objectRef)
                {
                    addNew = true;
                }
                else if (objectRef.state == State.Loaded)
                {
                    objectRef.OnExecuteLoaded();
                    addNew = true;
                }
                
                if (addNew && _loadQueue.Count > 0)
                {
                    objectRef = _loadQueue.Dequeue();
                    _loadingList[i] = objectRef;
                    ResourceRuntime.StartCoroutine(objectRef.LoadAsync());
                }
            }
        }

        internal void Dispose()
        {
            _tokenToObjectIds.Clear();

            ObjectRef[] objectArray = _objectCache.Values.ToArray();
            _objectCache.Clear();
            foreach (var objectRef in objectArray)
            {
                objectRef.Dispose();
            }
            
            _loadingList = new ObjectRef[MAX_LOADING_COUNT];
            
            _manifest.Dispose();
            _assignToken = 0;
        }
        
        internal Handle InstantiateSync(IResLoader owner, string prefabName)
        {
            GameObjectRef gameObjectRef = CreateGameObjectRef(prefabName, ++_assignToken, 0);
            Handle handle = gameObjectRef.AddReference(owner, false);
            gameObjectRef.LoadSync();
            gameObjectRef.OnExecuteLoaded();
            return handle;
        }
        
        internal Handle InstantiateAsync(IResLoader owner, string prefabName, int order = 0)
        {
            GameObjectRef gameObjectRef = CreateGameObjectRef(prefabName, ++_assignToken, order);
            Handle handle = gameObjectRef.AddReference(owner, true);
            //todo: youhua
            _loadQueue.Enqueue(gameObjectRef);
            return handle;
        }
        
        internal Handle LoadAssetSync(IResLoader owner, string assetName)
        {
            AssetRef assetRef = GetOrCreateAssetRef(assetName, 0);
            Handle handle = assetRef.AddReference(owner, false);
            assetRef.LoadSync();
            assetRef.OnExecuteLoaded();
            return handle;
        }

        internal Handle LoadAssetAsync(IResLoader owner, string assetName, int order = 0)
        {
            AssetRef assetRef = GetOrCreateAssetRef(assetName, order);
            Handle handle = assetRef.AddReference(owner, true);
            //todo: youhua
            _loadQueue.Enqueue(assetRef);
            return handle;
        }

        internal Handle LoadBundleSync(IResLoader owner, string bundle)
        {
            BundleRef bundleRef = GetOrCreateBundleRef(bundle, true);
            Handle handle = bundleRef.AddReference(owner, false);
            bundleRef.LoadSync();
            bundleRef.OnExecuteLoaded();
            return handle;
        }

        internal Handle LoadBundleAsync(IResLoader owner, string bundle)
        {
            BundleRef bundleRef = GetOrCreateBundleRef(bundle, true);
            Handle handle = bundleRef.AddReference(owner, true);
            //todo: youhua
            _loadQueue.Enqueue(bundleRef);
            return handle;
        }

        internal IResLoader CancelOrUnload(in Handle handle)
        {
            IResLoader resourceLoader = null;
            
            if (_tokenToObjectIds.TryGetValue(handle.token, out ulong objectId))
            {
                if (_objectCache.TryGetValue(objectId, out var objectRef))
                {
                    resourceLoader = objectRef.DeleteReference(handle);
                    
                    if (objectRef.ZeroReference)
                    {
                        objectRef.Dispose();
                    }
                }
            }

            return resourceLoader;
        }

        internal T Get<T>(uint refToken) where T : UnityEngine.Object
        {
            if (_tokenToObjectIds.TryGetValue(refToken, out ulong objectId))
            {
                if (_objectCache.TryGetValue(objectId, out var objectRef))
                    return objectRef.unityObject as T;
                
                _tokenToObjectIds.Remove(refToken);
            }
            
            return null;
        }
        
        internal bool IsLoaded(uint refToken)
        {
            if(_tokenToObjectIds.TryGetValue(refToken, out ulong objectId) && _objectCache.TryGetValue(objectId, out var objectRef))
                return objectRef.state == State.Loaded;
            return true;
        }
        
        internal ObjectRef GetObjectRefByToken(uint refToken)
        {
            _tokenToObjectIds.TryGetValue(refToken, out ulong objectId);
            return GetObjectRef(objectId);
        }

        internal ObjectRef GetObjectRef(ulong objectId)
        {
            _objectCache.TryGetValue(objectId, out var objectRef);
            return objectRef;
        }
    }
}