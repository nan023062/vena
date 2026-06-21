# Vena.Assets — Contract

本包对外暴露的公开 surface。业务工程仅需遵守以下接口；未列出的任何类不是合同、不保证稳定。

## Public Namespaces

- `Vena.Assets` — Runtime API（`Vena.Assets.asmdef`）
- `Vena.Assets.Editor` — Editor 扩展 + 注入入口（`Vena.Assets.Editor.asmdef`）
- `ICSharpCode.SharpZipLib.*` — 第三方嵌入源码，原命名空间按上游仓库保留；业务代码**不依赖**该命名空间，调用压缩能力走 `Vena.Assets.Compression` facade。

## Interfaces

### `IOssClient`（Runtime，由业务工程实现 + 注入）

```csharp
namespace Vena.Assets
{
    public interface IOssClient
    {
        // 存在性 / 元数据
        Task<bool> ExistsAsync(string objectKey, CancellationToken ct = default);

        // 下载。返回的 Stream 由调用方 Dispose。
        Task<Stream> DownloadAsync(string objectKey, CancellationToken ct = default);

        // 上传。content 所有权仍在调用方（本接口不 Dispose）。
        Task UploadAsync(string objectKey, Stream content, string contentType = null, CancellationToken ct = default);

        // 上传本地文件。高频调用路径（VersionControl 量产上传）。
        Task UploadFileAsync(string objectKey, string localFilePath, CancellationToken ct = default);

        // 删除。
        Task DeleteAsync(string objectKey, CancellationToken ct = default);

        // 获取对象公网 URL（可选能力；若不支持返回 null 或抛 NotSupportedException）。
        string GetObjectUrl(string objectKey);
    }
}
```

**职责边界**：`IOssClient` 只负责「对象存储读写 + URL 获取」这四件事。以下能力**不**入接口，留给业务实现内部后面隐藏或另起接口：STS Token / 临时凭证、分片上传、签名 URL、ACL / Bucket 管理、多区域切换、存储类别（热冷）。如未来需要补「分片上传」，走「追加非破坏性成员」路径（参 `com.vena.blockly` Key Decision #10 同型纪律）：默认实现为 `UploadAsync`、可选 override `UploadMultipartAsync`。

### `OssObjectKey`（value object，包内提供拼接逻辑）

```csharp
namespace Vena.Assets
{
    public readonly struct OssObjectKey
    {
        public string Value { get; }
        public OssObjectKey(string value);
        public static OssObjectKey Compose(string subDir, string platformVer, string fileName);
        public override string ToString();
        public static implicit operator string(OssObjectKey k);
    }
}
```

## Editor Injection Surface

`AssetToolkitProvider`（Editor）提供业务工程注入面：

```csharp
namespace Vena.Assets.Editor
{
    public static class AssetToolkitProvider
    {
        // 业务工程在 [InitializeOnLoad] 期调用。factory 返回的 IOssClient 身份生命周期为单次 Upload 事务。
        public static void SetOssClientFactory(Func<IOssClient> factory);

        // 读取当前工厂（未注入返回 InMemoryOssClient）。
        public static IOssClient CreateOssClient();
    }
}
```

## Test Doubles

- `Vena.Assets.InMemoryOssClient` — 包内默认 Mock。语义：在进程内 `Dictionary<string, byte[]>` 存储。`GetObjectUrl` 返回伪 URL `mem://oss/<key>`。仅用于 CI / unit tests / unity-project 本地验证；业务生产环境**禁用**。

## Stability

- `IOssClient` 接口只可「追加非破坏性成员」，不可修改现有方法签名。
- `OssObjectKey` 为 struct，只追加静态工厂方法、不可添实例字段（避免 break ABI）。
- `AssetToolkitProvider.SetOssClientFactory` 签名冻结；业务工程依赖该入口。

## Events

（本包运行期不发任何 Editor / Runtime 事件；Editor 期需交互反馈走 `EditorUtility.DisplayProgressBar` 等 Unity 内置设施，不在合同 surface 内。）
