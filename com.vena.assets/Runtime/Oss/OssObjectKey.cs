// -----------------------------------------------------------------------------
// Vena Assets
// Unity asset pipeline toolkit (AssetBundle / VersionControl / OSS / Compression).
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

namespace Vena.Assets
{
    public readonly struct OssObjectKey
    {
        public string Value { get; }

        public OssObjectKey(string value)
        {
            Value = value;
        }

        public static OssObjectKey Compose(string subDir, string platformVer, string fileName)
            => new OssObjectKey($"{subDir}/{platformVer}/{System.IO.Path.GetFileName(fileName)}");

        public override string ToString() => Value;

        public static implicit operator string(OssObjectKey k) => k.Value;
    }
}
