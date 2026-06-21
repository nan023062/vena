# Changelog

## [1.0.0] - 2026-06-21

### Added
- Initial release migrated from external repo `github.com/nan023062/vena-asset-toolkit`.
- Manager, VersionControl, AssetBuildTree, Compression (with embedded SharpZipLib MIT source), Editor toolkit.
- New `IOssClient` abstraction + `InMemoryOssClient` test double.

### Changed
- Renamed package id `com.vana.asset-toolkit` -> `com.vena.assets`.
- Renamed C# namespace `Vena.AssetToolkit` -> `Vena.Assets`.
- Renamed asmdef `Vana.AssetToolkit` -> `Vena.Assets` (corrected `Vana` typo).
- Switched license MIT -> MulanPSL-2.0; SharpZipLib embedded source remains MIT.

### Removed
- `Plugins/Aliyun.OSS.dll` - replaced by `IOssClient` abstraction.
- `Runtime/Core/Oss/OssTool.cs` and `OssUploadObject.cs` - superseded.
