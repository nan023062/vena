// -----------------------------------------------------------------------------
// Vena Assets
// Unity asset pipeline toolkit (AssetBundle / VersionControl / OSS / Compression).
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace Vena.Assets
{
    public class VersionPackageData : IComparable<VersionPackageData>
    {
        public DirectoryInfo directory;

        public VersionManifest manifest;

        public int CompareTo(VersionPackageData other)
        {
            return manifest.version.CompareTo(other.manifest.version) * -1;
        }
    }
}
