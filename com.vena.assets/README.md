# Vena Assets

Unity asset pipeline toolkit. AssetBundle build & runtime loading, asset
versioning, compression (Zip/GZip/Tar/BZip2/Lzw), remote object-storage
sync via a pluggable `IOssClient` abstraction, plus an asset profiler and
pipeline checks. UPM package `com.vena.assets`.

Migrated from `github.com/nan023062/vena-asset-toolkit`.

## Features

| Area | What it gives you |
|---|---|
| Manager | `ResourceRuntime` — typed AssetBundle load / async load / unload, ref-counted `Package` aggregate, `ObjectRef` / `AssetManifest` / `AssetList` / `HandleApi`. |
| VersionControl | `VersionManifest` (file-IO read/write), `VersionPackageData`, `AssetDownLoader` (async, depends on `IOssClient`), `AssetUsageSettings`. |
| AssetBuildTree | Editor-time build-tree model — `AssetBuildSetting`, `BuildTreeNode`, `BuildDependInfo`, `BuildPackageType`. |
| Compression | `Compression` facade over Zip / GZip / Tar / BZip2 / Lzw, `CompressionTask` async wrapper. SharpZipLib MIT source embedded under `Runtime/Core/CSharpZipLib/`. |
| Oss | `IOssClient` abstraction + `OssObjectKey` value object + `InMemoryOssClient` test double. **No** cloud-vendor SDK is bundled. |
| Editor | `AssetToolkitProvider` settings provider with tabs for Builder / Profiler / Pipeline / VersionControl. |

## Install

UPM, add to `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.vena.assets": "https://github.com/nan023062/vena.git?path=com.vena.assets"
  }
}
```

Unity 2021.3+.

## OSS injection

The package ships **no** cloud-vendor SDK. Connect any backend (Aliyun OSS,
Amazon S3, Azure Blob, MinIO, your own HTTP server, ...) by implementing
`IOssClient` and injecting it once at editor startup:

```csharp
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using Vena.Assets;
using Vena.Assets.Editor;

[InitializeOnLoad]
internal static class GameOssWiring
{
    static GameOssWiring()
    {
        AssetToolkitProvider.SetOssClientFactory(() => new MyAliyunOssClient(/* config */));
    }
}

internal sealed class MyAliyunOssClient : IOssClient
{
    public Task<bool>   ExistsAsync     (string key, CancellationToken ct = default) { /* ... */ }
    public Task<Stream> DownloadAsync   (string key, CancellationToken ct = default) { /* ... */ }
    public Task         UploadAsync     (string key, Stream content, string contentType = null, CancellationToken ct = default) { /* ... */ }
    public Task         UploadFileAsync (string key, string localFilePath, CancellationToken ct = default) { /* ... */ }
    public Task         DeleteAsync     (string key, CancellationToken ct = default) { /* ... */ }
    public string       GetObjectUrl    (string key) { /* ... */ }
}
```

Without an injected factory the toolkit falls back to `InMemoryOssClient`
(in-process `Dictionary<string, byte[]>`), suitable for CI / local
verification only.

Object keys are composed via the `OssObjectKey` value object:

```csharp
var key = OssObjectKey.Compose(subDir: "game-assets",
                               platformVer: "Android_1.2.0",
                               fileName: "asset-bundle.unity3d");
await client.UploadFileAsync(key, localPath);
```

## License

- Package source: **MulanPSL-2.0** — see `LICENSE`.
- Embedded SharpZipLib source under `Runtime/Core/CSharpZipLib/`: **MIT** — see `LICENSE.SharpZipLib`.
- Third-party attribution: see `THIRD-PARTY-NOTICES.md`.
