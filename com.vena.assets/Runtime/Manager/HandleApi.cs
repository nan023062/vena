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
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Vena.Assets
{
    public static partial class ResourceRuntime
    {
        public static Handle[] GetAllResourceHandles(this IResLoader loader)
        {
            if (Loaders.TryGetValue(loader, out HashSet<Handle> handles))
                return handles.ToArray();
            return Array.Empty<Handle>();
        }
        
        public static void UnloadAllResources(this IResLoader loader)
        {
            if (Loaders.TryGetValue(loader, out HashSet<Handle> handles))
            {
                Loaders.Remove(loader);
                
                foreach (var handle in handles)
                {
                    try
                    {
                        handle.CancelOrUnload();
                    }
                    catch (Exception e)
                    {
                        LogError($"{handle}.CancelOrUnload()! e={e.Message},stack={e.StackTrace}!!");
                    }
                }
            }
        }

        public static Handle InstantiateSync(this IResLoader loader, int packageId, string assetName)
        {
            if (!Loaders.TryGetValue(loader, out HashSet<Handle> handles))
            {
                handles = new HashSet<Handle>();
                Loaders.Add(loader, handles);
            }

            Package package = GetPackage(packageId);
            Handle handle = package.InstantiateSync(loader, assetName);
            handles.Add(handle);
            return handle;
        }

        public static Handle InstantiateAsync(this IResAsyncLoader loader, int packageId, string assetName)
        {
            if (!Loaders.TryGetValue(loader, out HashSet<Handle> handles))
            {
                handles = new HashSet<Handle>();
                Loaders.Add(loader, handles);
            }

            Package package = GetPackage(packageId);
            Handle handle = package.InstantiateAsync(loader, assetName);
            handles.Add(handle);
            return handle;
        }

        public static Handle LoadAssetSync(this IResLoader loader, int packageId, string assetName)
        {
            if (!Loaders.TryGetValue(loader, out HashSet<Handle> handles))
            {
                handles = new HashSet<Handle>();
                Loaders.Add(loader, handles);
            }

            Package package = GetPackage(packageId);
            Handle handle = package.LoadAssetSync(loader, assetName);
            handles.Add(handle);
            return handle;
        }
        
        public static Handle LoadAssetAsync(this IResAsyncLoader loader, int packageId, string assetName)
        {
            if (!Loaders.TryGetValue(loader, out HashSet<Handle> handles))
            {
                handles = new HashSet<Handle>();
                Loaders.Add(loader, handles);
            }

            Package package = GetPackage(packageId);
            Handle handle = package.LoadAssetAsync(loader, assetName);
            handles.Add(handle);
            return handle;
        }
        
        public static Handle LoadBundleSync(this IResLoader loader, int packageId, string bundle)
        {
            if (!Loaders.TryGetValue(loader, out HashSet<Handle> handles))
            {
                handles = new HashSet<Handle>();
                Loaders.Add(loader, handles);
            }

            Package package = GetPackage(packageId);
            Handle handle = package.LoadBundleSync(loader, bundle);
            handles.Add(handle);
            return handle;
        }

        public static Handle LoadBundleAsync(this IResLoader loader, int packageId, string bundle)
        {
            if (!Loaders.TryGetValue(loader, out HashSet<Handle> handles))
            {
                handles = new HashSet<Handle>();
                Loaders.Add(loader, handles);
            }

            Package package = GetPackage(packageId);
            Handle handle = package.LoadBundleAsync(loader, bundle);
            handles.Add(handle);
            return handle;
        }

        public static T Get<T>(this Handle handle) where T : UnityEngine.Object
        {
            Package package = GetPackage(handle.package);
            if (null != package)
            {
                return package.Get<T>(handle.token);
            }

            return default;
        }
        
        public static bool IsLoaded(this Handle handle)
        {
            Package package = GetPackage(handle.package);
            if (null != package)
            {
                return package.IsLoaded(handle.token);
            }
            return true;
        }

        public static void CancelOrUnload(this Handle handle)
        {
            Package package = GetPackage(handle.package);
            if (null != package)
            {
                IResLoader loader = package.CancelOrUnload(handle);
                if (null != loader && Loaders.TryGetValue(loader, out HashSet<Handle> handles))
                {
                    handles.Remove(handle);
                    if (handles.Count <= 0)
                        Loaders.Remove(loader);
                }
            }
        }
        
        public static Package.ObjectRef GetObjectRef(this Handle handle)
        {
            Package package = GetPackage(handle.package);
            return package.GetObjectRefByToken(handle.token);
        }
        
        public static Package.ObjectRef GetObjectRef(short packageId, ulong objectId)
        {
            Package package = GetPackage(packageId);
            return package.GetObjectRef(objectId);
        }
    }

    public enum AssetErrorCode
    {
        Success = 0,
        BeCancel,
    }

    [StructLayout(LayoutKind.Explicit)]
    public readonly struct Handle : IEnumerator
    {
        [FieldOffset(0)] public readonly short package;

        [FieldOffset(2)] public readonly short type;

        [FieldOffset(4)] public readonly uint token;

        public Handle(short package, int type, uint token)
        {
            this.package = package;
            this.type = (short)type;
            this.token = token;
        }
        
        public override string ToString()
        {
            var objectRef = this.GetObjectRef();
            return $"Handle:{ResourceRuntime.PackageIDToName(package)}_{token} <{objectRef}>";
        }
        
        public static implicit operator ulong(Handle handle)
        {
            ulong package = (ulong)handle.package << 48;
            ulong type = (ulong)handle.type << 32;
            return package | type | handle.token;
        }

        public static implicit operator Handle(ulong handleId)
        {
            short package = (short)(handleId >> 48);
            short type = (short)((handleId >> 32) & 0xffff);
            uint token = (uint)(handleId & 0xffffffff);
            return new Handle(package, type, token);
        }

        public static bool operator ==(Handle lhs, Handle rhs)
        {
            ulong l1 = lhs;
            ulong l2 = rhs;
            return l1 == l2;
        }

        public static bool operator !=(Handle lhs, Handle rhs)
        {
            ulong l1 = lhs;
            ulong l2 = rhs;
            return l1 != l2;
        }
        
        public bool Equals(Handle other)
        {
            return package == other.package && type == other.type && token == other.token;
        }

        public override bool Equals(object obj)
        {
            return obj is Handle other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + package.GetHashCode();
                hash = hash * 31 + type.GetHashCode();
                hash = hash * 31 + token.GetHashCode();
                return hash;
            }
        }

        bool IEnumerator.MoveNext() => !this.IsLoaded();
        
        void IEnumerator.Reset() { }

        object IEnumerator.Current => this.Get<UnityEngine.Object>();
    }

    public interface IResLoader
    {
    }
    
    public interface IResAsyncLoader : IResLoader
    {
        void OnResLoaded(AssetErrorCode code, Handle handle);
    }
}