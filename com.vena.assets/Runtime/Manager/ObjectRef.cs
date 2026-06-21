// -----------------------------------------------------------------------------
// Vena Assets
// Unity asset pipeline toolkit (AssetBundle / VersionControl / OSS / Compression).
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityObject = UnityEngine.Object;
// ReSharper disable Unity.PerformanceCriticalCodeInvocation

namespace Vena.Assets
{
    public sealed partial class Package
    {
        #region ObjectRef Management
        
        private readonly Dictionary<uint, ulong> _tokenToObjectIds;
        private readonly Dictionary<ulong, ObjectRef> _objectCache;
        
        internal static byte ObjectRefTypeId(Type type)
        {
            if (type == typeof(BundleRef)) return 0;
            if (type == typeof(AssetRef)) return 1;
            if (type == typeof(GameObjectRef)) return 2;
            throw new Exception($"Package.ObjectRefTypeId( {type.Name} ) !!!");
        }
        
        internal static Type ObjectRefIdType(int typeId)
        {
            if (typeId == 0) return typeof(BundleRef);
            if (typeId == 1) return typeof(AssetRef);
            return typeof(GameObjectRef);
        }
        
        internal static uint ObjectRefTypeHash(Type type, string name)
        {
            byte typeId = ObjectRefTypeId(type);
            int nameHash = ResourceRuntime.StringToHash(name);
            return ((uint)typeId) << 28 | (uint)nameHash;
        }
        
        private static ulong ToObjectRefId(Type type, string name, uint minId)
        {
            uint typeHash = ObjectRefTypeHash(type, name);
            return (ulong)typeHash << 32 | minId;
        }
        
        private T GetAssetOrBundleRef<T>(string nameStr) where T : ObjectRef
        {
#if UNITY_EDITOR
            if (typeof(T) == typeof(GameObjectRef))
                throw new Exception($"GetAssetOrBundleRef<{typeof(T).Name}>( {nameStr} ) !!!");
#endif
            ulong refId = ToObjectRefId(typeof(T), nameStr, 0);
            _objectCache.TryGetValue(refId, out var objectRef);
            return objectRef as T;
        }
        
        private T GetObjectRef<T>(ulong objectRefId) where T : ObjectRef
        {
            _objectCache.TryGetValue(objectRefId, out var objectRef);
            return objectRef as T;
        }

        private void Register(ObjectRef baseRef)
        {
            _objectCache.Add(baseRef.id, baseRef);
        }

        private bool UnRegister(ObjectRef baseRef)
        {
            return _objectCache.Remove(baseRef.id);
        }

        private GameObjectRef CreateGameObjectRef(string prefabName, uint minId, int order)
        {
            AssetRef assetRef = GetOrCreateAssetRef(prefabName, order);
            var gameObjectRef = new GameObjectRef(this, assetRef, minId, order);
            return gameObjectRef;
        }
        
        private AssetRef GetOrCreateAssetRef(string assetName, int order)
        {
            AssetRef assetRef = GetAssetOrBundleRef<AssetRef>(assetName);
            if (null != assetRef) return assetRef;
            
            if (useBundle)
            {
                string bundleName = _manifest.GetBundleName(assetName);
                if (string.IsNullOrEmpty(bundleName))
                {
                    var bundleRef = GetOrCreateBundleRef(BundleRef.EDIT_BUNDLE, false);
                    Debug.LogError($"没有找到{assetName} - {bundleName} 资源！");
                    assetRef = new AssetRef(this, assetName, bundleRef);
                }
                else
                {
                    BundleRef bundleRef = GetOrCreateBundleRef(bundleName, true);
                    assetRef = new AssetRef(this, assetName, bundleRef, order);
                }
            }
            else
            {
                var bundleRef = GetOrCreateBundleRef(BundleRef.EDIT_BUNDLE, false);
                assetRef = new AssetRef(this, assetName, bundleRef);
            }

            return assetRef;
        }

        private BundleRef GetOrCreateBundleRef(string bundleName, bool bundleMode)
        {
            int indexof = bundleName.IndexOf('.');
            if (indexof > 0) bundleName = bundleName.Substring(0, indexof);
            
            var bundleRef = GetAssetOrBundleRef<BundleRef>(bundleName);
            if (null == bundleRef)
            {
                bundleRef = new BundleRef(this, bundleName, bundleMode);
                if (bundleMode)
                {
                    CreateBundleDeps(bundleRef, bundleRef, 1);
                }
            }

            return bundleRef;
        }

        private void CreateBundleDeps(BundleRef startRef, BundleRef bundleRef, int depth = 1)
        {
            if (depth >= MAX_RECURSUVE_DEPTH)
            {
                ResourceRuntime.LogError(
                    $"AB[{startRef.name}]对于[{bundleRef.name}]依赖深度超过{MAX_RECURSUVE_DEPTH}，请修改资源策略！");
                return;
            }

            string[] bundleDeps = _manifest.GetDependencies(bundleRef.name);

            if (bundleDeps != null && bundleDeps.Length > 0)
            {
                foreach (string bundleName in bundleDeps)
                {
                    var bundleDep = GetAssetOrBundleRef<BundleRef>(bundleName);
                    if (null == bundleDep)
                    {
                        bundleDep = new BundleRef(this, bundleName, true);
                        CreateBundleDeps(startRef, bundleDep, depth + 1);
                    }

                    bundleDep.AddReference(bundleRef);
                }
            }
        }
        
        #endregion
        
        #region define

        public enum State { Default, Loading, Loaded, Unloading, }
        
        public abstract class ObjectRef : IComparable<ObjectRef>
        {
            public readonly ulong id;
            public readonly string name;
            public readonly Package package;
            public readonly int order;
            
            private bool _disposed;
            
            protected readonly Dictionary<ulong, (IResLoader, bool)> handleRefs;
            
            public State state { protected set; get; }
            
            internal abstract UnityObject unityObject { get; }
            
            protected ObjectRef(Package package, string name, uint minId,  int order)
            {
                id = ToObjectRefId(GetType(), name, minId);
                this.name = name;
                this.order = order;
                this.package = package;
                _disposed = false;
                handleRefs = new Dictionary<ulong, (IResLoader, bool)>();
                this.package.Register(this);
            }

            internal abstract IEnumerator LoadAsync();

            internal abstract void LoadSync();
            
            internal void OnExecuteLoaded()
            {
                if (handleRefs.Count > 0)
                {
                    KeyValuePair<ulong,(IResLoader,bool)>[] keyValues = handleRefs.ToArray();
                    handleRefs.Clear();
                    foreach (var keyValue in keyValues)
                    {
                        handleRefs.Add(keyValue.Key,(keyValue.Value.Item1, false));
                        if (keyValue.Value.Item2)
                        {
                            Handle handle = keyValue.Key;
                            if(keyValue.Value.Item1 is IResAsyncLoader asyncLoader)
                                asyncLoader.OnResLoaded(AssetErrorCode.Success, handle);
                        }
                    }
                }
            }

            internal Handle AddReference(IResLoader loader, bool call)
            {
                Handle handle = new Handle(package.id, ObjectRefTypeId(GetType()), ++package._assignToken);
                handleRefs.Add(handle, (loader, call));
                package._tokenToObjectIds.Add(handle.token, id);
                return handle;
            }

            internal IResLoader DeleteReference(in Handle handle)
            {
                package._tokenToObjectIds.Remove(handle.token);
                
                if (handleRefs.TryGetValue(handle, out var tuple))
                {
                    handleRefs.Remove(handle);
                    return tuple.Item1;
                }

                throw new Exception($"DeleteReference({handle}) 异常!!");
            }
            
            public int CompareTo(ObjectRef other) => order.CompareTo(other.order);
            
            
            public override string ToString()
            {
                return $"{name} <{GetType().Name}>";
            }
            
            internal void Dispose()
            {
                if (_disposed) return;
                _disposed = true;

                state = State.Default;
                package.UnRegister(this);
                
                if (handleRefs.Count > 0)
                {
                    KeyValuePair<ulong,(IResLoader,bool)>[] keyValues = handleRefs.ToArray();
                    handleRefs.Clear();
                    foreach (var keyValue in keyValues)
                    {
                        if (keyValue.Value.Item2)
                        {
                            Handle handle = keyValue.Key;
                            if(keyValue.Value.Item1 is IResAsyncLoader asyncLoader)
                                asyncLoader.OnResLoaded(AssetErrorCode.BeCancel, handle);
                        }
                    }
                }
                
                OnDispose();
            }

            internal abstract void AddReference(ObjectRef objectRef);
            
            internal abstract bool DeleteReference(ObjectRef objectRef);
            
            internal abstract bool ZeroReference { get; }

            protected abstract void OnDispose();

            public abstract ulong[] GetReferences();
            
            public abstract ulong[] GetDependencies();
        }

        class BundleRef : ObjectRef
        {
            public static readonly string EDIT_BUNDLE = "ASSET_TOOL_EDIT_BUNDLE";
            private AssetBundle _assetBundle;
            private bool _bundleMode;
            
            private readonly HashSet<ulong> _objectRefs, _bundleDeps;
            
            internal override UnityObject unityObject => _assetBundle;
            
            public BundleRef(Package package, string name, bool bundleMode) : base(package,  name, 0, 0)
            {
                _assetBundle = null;
                _bundleMode = bundleMode;
                state = State.Default;
                _objectRefs = new HashSet<ulong>();
                _bundleDeps = new HashSet<ulong>();
            }
            
            internal override void AddReference(ObjectRef objectRef)
            {
                _objectRefs.Add(objectRef.id);
                
                if (objectRef is BundleRef bundleRef)
                {
                    bundleRef._bundleDeps.Add(id);
                }
                else if (objectRef is AssetRef assetRef)
                {
                    
                }
            }
            
            internal override bool DeleteReference(ObjectRef objectRef)
            {
                if (_objectRefs.Remove(objectRef.id))
                {
                    if (objectRef is BundleRef bundleRef)
                    {
                        bundleRef._bundleDeps.Remove(id);
                    }
                    else if (objectRef is AssetRef assetRef)
                    {
                        
                    }
                    
                    return true;
                }
                
                return false;
            }

            internal override bool ZeroReference => _objectRefs.Count == 0 && handleRefs.Count == 0;

            protected override void OnDispose()
            {
                _objectRefs.Clear();
                if (_bundleDeps.Count > 0)
                {
                    ulong[] bundleDeps = _bundleDeps.ToArray();
                    _bundleDeps.Clear();
                    
                    foreach (ulong bundleDep in bundleDeps)
                    {
                        var bundleRef = package.GetObjectRef<BundleRef>(bundleDep);
                        bundleRef.DeleteReference(this);
                        if(bundleRef.ZeroReference)
                            bundleRef.Dispose();
                    }
                }
                
                if (null != _assetBundle)
                {
                    _assetBundle.Unload(true);
                    _assetBundle = null;
                }
            }
            
            internal override IEnumerator LoadAsync()
            {
                if (_bundleMode)
                {
                    //1 first load deps bundles
                    if (_bundleDeps.Count > 0)
                    {
                        foreach (ulong bundleDep in _bundleDeps)
                        {
                            var bundleRef = package.GetObjectRef<BundleRef>(bundleDep);
                            if (bundleRef.state != State.Loaded)
                                yield return bundleRef.LoadAsync();
                        }
                    }

                    //2  load from file async ...
                    if (state == State.Default)
                    {
                        state = State.Loading;
                        string bundleUrl = Utility.GetBundleFullPath(package.bundlePath, name, true);
                        var bundleRequest = AssetBundle.LoadFromFileAsync(bundleUrl);
                        yield return new WaitUntil(() => bundleRequest.isDone);
                        _assetBundle = bundleRequest.assetBundle;
                        if (_assetBundle == null) ResourceRuntime.LogError($"加载 [{bundleUrl}] 失败！");
                        state = State.Loaded;
                    }
                }
                else
                {
                    state = State.Loaded;
                }
            }
            
            internal override void LoadSync()
            {
                if (_bundleMode)
                {
                    //1 first load deps bundles
                    if (_bundleDeps.Count > 0)
                    {
                        foreach (ulong bundleDep in _bundleDeps)
                        {
                            var bundleRef = package.GetObjectRef<BundleRef>(bundleDep);
                            if (bundleRef.state != State.Loaded)
                                bundleRef.LoadSync();
                        }
                    }

                    //2  load from file
                    //加载当前目标Bundle
                    state = State.Loading;
                    string bundlePath = Utility.GetBundleFullPath(package.bundlePath, name, true);
                    _assetBundle = AssetBundle.LoadFromFile(bundlePath);
                    if (_assetBundle == null) ResourceRuntime.LogError($"加载 [{bundlePath}] 失败！");
                    state = State.Loaded;
                }
                else
                {
                    state = State.Loaded;
                }
            }
            
            public UnityObject GetAsset(string assetName)
            {
                if (_bundleMode)
                {
                    return _assetBundle.LoadAsset(assetName);
                }
                
                UnityEngine.Object asset = null;
#if UNITY_EDITOR
                string assetPath = package._manifest.AssetNameToPath(assetName);
                
                if (!String.IsNullOrEmpty(assetPath))
                {
                    asset = UnityEditor.AssetDatabase.LoadAssetAtPath(assetPath, typeof(UnityEngine.Object));
                }
                else
                {
                    ResourceRuntime.LogError($"{name}.LoadEditorAsset({assetName}) failed !!！");
                }
#endif
                return asset;
            }

            public override ulong[] GetReferences()
            {
                return _objectRefs.ToArray();
            }

            public override ulong[] GetDependencies()
            {
                return _bundleDeps.ToArray();
            }
        }

        class AssetRef : ObjectRef
        {
            private BundleRef _depBundleRef;
            
            private UnityObject _asset;

            private readonly HashSet<ulong> _gameObjectRefs;
            
            internal override UnityObject unityObject => _asset;
            
            public AssetRef(Package package, string name, BundleRef depBundleRef, int order = 0) 
                : base(package, name, 0, order)
            {
                _depBundleRef = depBundleRef;
                state = State.Default;
                _gameObjectRefs = new HashSet<ulong>();
                _depBundleRef.AddReference(this);
            }

            internal override void AddReference(ObjectRef objectRef)
            {
                if (objectRef is GameObjectRef gameObjectRef)
                {
                    _gameObjectRefs.Add(gameObjectRef.id);
                }
            }

            internal override bool DeleteReference(ObjectRef objectRef)
            {
                if (objectRef is GameObjectRef gameObjectRef)
                {
                    return _gameObjectRefs.Remove(gameObjectRef.id);
                }
            
                return false;
            }

            internal override bool ZeroReference => _gameObjectRefs.Count == 0 && handleRefs.Count == 0;

            protected override void OnDispose()
            {
                _depBundleRef.DeleteReference(this);
                if(_depBundleRef.ZeroReference)
                    _depBundleRef.Dispose();
                _depBundleRef = null;
                state = State.Default;
                _asset = null;
            }
            
            internal override IEnumerator LoadAsync()
            {
                if (state == State.Default)
                {
                    state = State.Loading;
                    yield return _depBundleRef.LoadAsync();
                    _asset = _depBundleRef.GetAsset(name);
                    state = State.Loaded;
                }
            }
            
            internal override void LoadSync()
            {
                if (state != State.Loaded)
                {
                    state = State.Loading;
                    _depBundleRef.LoadSync();
                    _asset = _depBundleRef.GetAsset(name);
                    state = State.Loaded;
                }
            }
            
            public override ulong[] GetReferences()
            {
                return _gameObjectRefs.ToArray();
            }

            public override ulong[] GetDependencies()
            {
                return new [] { _depBundleRef.id };
            }
        }

        class GameObjectRef : ObjectRef
        {
            private AssetRef _depAssetRef;
            
            private GameObject _gameObject;
            
            internal override UnityObject unityObject => _gameObject;
            
            public GameObjectRef(Package package, AssetRef depAssetRef, uint minId, int order = 0) 
                : base(package, depAssetRef.name, minId, order)
            {
                _depAssetRef = depAssetRef;
                _depAssetRef.AddReference(this);
                state = State.Default;
            }
            
            internal override IEnumerator LoadAsync()
            {
                if (state == State.Default)
                {
                    state = State.Loading;
                    yield return _depAssetRef.LoadAsync();
                    if (_depAssetRef.unityObject is GameObject go)
                        _gameObject = UnityObject.Instantiate(go);
                    state = State.Loaded;
                }
            }

            internal override void LoadSync()
            {
                if (state != State.Loaded)
                {
                    state = State.Loading;
                    _depAssetRef.LoadSync();
                    if (_depAssetRef.unityObject is GameObject go)
                        _gameObject = UnityObject.Instantiate(go);
                    state = State.Loaded;
                }
            }
            
            internal override void AddReference(ObjectRef objectRef)
            {
                throw new NotImplementedException();
            }

            internal override bool DeleteReference(ObjectRef objectRef)
            {
                throw new NotImplementedException();
            }
            
            internal override bool ZeroReference => handleRefs.Count == 0;

            protected override void OnDispose()
            {
                if (_gameObject != null)
                {
                    UnityObject.Destroy(_gameObject);
                    _gameObject = null;
                }
                
                _depAssetRef.DeleteReference(this);
                if (_depAssetRef.ZeroReference)
                {
                    _depAssetRef.Dispose();
                    _depAssetRef = null;
                }
            }
            
            public override ulong[] GetReferences()
            {
                return Array.Empty<ulong>();
            }

            public override ulong[] GetDependencies()
            {
                return new [] { _depAssetRef.id };
            }
        }
        
        #endregion
    }
}