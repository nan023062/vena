// -----------------------------------------------------------------------------
// Vena Assets
// Unity asset pipeline toolkit (AssetBundle / VersionControl / OSS / Compression).
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System;

namespace Vena.Assets.Editor
{
    /// <summary>
    /// Editor-side injection surface for <see cref="IOssClient"/>. Business
    /// projects call <see cref="SetOssClientFactory"/> in an
    /// <c>[InitializeOnLoad]</c> handler to wire a real implementation
    /// (Aliyun OSS / Amazon S3 / Azure Blob / your own HTTP server). Without
    /// an injected factory, <see cref="CreateOssClient"/> falls back to
    /// <see cref="InMemoryOssClient"/> — suitable for CI / local verification
    /// only.
    /// </summary>
    public static class AssetToolkitProvider
    {
        private static Func<IOssClient> _factory = () => new InMemoryOssClient();

        /// <summary>
        /// Register the factory used by editor tabs (e.g. version-control upload)
        /// to obtain an <see cref="IOssClient"/>. The factory is invoked once per
        /// upload transaction; lifetime / Dispose is the factory's responsibility.
        /// Passing <c>null</c> resets to the in-memory default.
        /// </summary>
        public static void SetOssClientFactory(Func<IOssClient> factory)
        {
            if (factory == null)
            {
                _factory = () => new InMemoryOssClient();
                HasCustomFactory = false;
            }
            else
            {
                _factory = factory;
                HasCustomFactory = true;
            }
        }

        /// <summary>
        /// Materialise an <see cref="IOssClient"/> via the registered factory
        /// (or the in-memory default).
        /// </summary>
        public static IOssClient CreateOssClient() => _factory();

        /// <summary>
        /// True iff a non-default factory has been registered. Editor tabs use
        /// this to display a "Production OSS not configured" banner when the
        /// in-memory fallback is active. Not part of the public contract — may
        /// move/disappear; do not rely on this from business code.
        /// </summary>
        internal static bool HasCustomFactory { get; private set; }
    }
}
