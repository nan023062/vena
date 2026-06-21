// -----------------------------------------------------------------------------
// Vena Assets
// Unity asset pipeline toolkit (AssetBundle / VersionControl / OSS / Compression).
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Vena.Assets
{
    public interface IOssClient
    {
        Task<bool>   ExistsAsync     (string objectKey, CancellationToken ct = default);
        Task<Stream> DownloadAsync   (string objectKey, CancellationToken ct = default);
        Task         UploadAsync     (string objectKey, Stream content, string contentType = null, CancellationToken ct = default);
        Task         UploadFileAsync (string objectKey, string localFilePath, CancellationToken ct = default);
        Task         DeleteAsync     (string objectKey, CancellationToken ct = default);
        string       GetObjectUrl    (string objectKey);
    }
}
