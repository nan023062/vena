// -----------------------------------------------------------------------------
// Vena Assets
// Unity asset pipeline toolkit (AssetBundle / VersionControl / OSS / Compression).
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Vena.Assets
{
    /// <summary>
    /// In-process Mock implementation of <see cref="IOssClient"/>. Stores objects
    /// in a dictionary keyed by object key. Intended for CI / unit tests / local
    /// editor verification only — never wire this into production.
    /// </summary>
    public sealed class InMemoryOssClient : IOssClient
    {
        private readonly Dictionary<string, byte[]> _store = new Dictionary<string, byte[]>();
        private readonly object _lock = new object();

        public Task<bool> ExistsAsync(string objectKey, CancellationToken ct = default)
        {
            lock (_lock)
            {
                return Task.FromResult(_store.ContainsKey(objectKey));
            }
        }

        public Task<Stream> DownloadAsync(string objectKey, CancellationToken ct = default)
        {
            byte[] bytes;
            lock (_lock)
            {
                if (!_store.TryGetValue(objectKey, out bytes))
                {
                    throw new FileNotFoundException($"InMemoryOssClient: object not found '{objectKey}'.");
                }
            }
            // Caller disposes the returned stream (per IOssClient contract).
            return Task.FromResult<Stream>(new MemoryStream(bytes, writable: false));
        }

        public async Task UploadAsync(string objectKey, Stream content, string contentType = null, CancellationToken ct = default)
        {
            if (content == null) throw new System.ArgumentNullException(nameof(content));

            using (var ms = new MemoryStream())
            {
                await content.CopyToAsync(ms, 81920, ct).ConfigureAwait(false);
                byte[] bytes = ms.ToArray();
                lock (_lock)
                {
                    _store[objectKey] = bytes;
                }
            }
        }

        public async Task UploadFileAsync(string objectKey, string localFilePath, CancellationToken ct = default)
        {
            if (string.IsNullOrEmpty(localFilePath))
                throw new System.ArgumentException("localFilePath is required.", nameof(localFilePath));
            if (!File.Exists(localFilePath))
                throw new FileNotFoundException("InMemoryOssClient: local file not found.", localFilePath);

            byte[] bytes;
            using (var fs = new FileStream(localFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true))
            using (var ms = new MemoryStream())
            {
                await fs.CopyToAsync(ms, 81920, ct).ConfigureAwait(false);
                bytes = ms.ToArray();
            }
            lock (_lock)
            {
                _store[objectKey] = bytes;
            }
        }

        public Task DeleteAsync(string objectKey, CancellationToken ct = default)
        {
            lock (_lock)
            {
                _store.Remove(objectKey);
            }
            return Task.CompletedTask;
        }

        public string GetObjectUrl(string objectKey) => $"mem://oss/{objectKey}";
    }
}
