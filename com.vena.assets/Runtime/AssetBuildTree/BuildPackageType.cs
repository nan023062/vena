// -----------------------------------------------------------------------------
// Vena Assets
// Unity asset pipeline toolkit (AssetBundle / VersionControl / OSS / Compression).
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Xml;

namespace Vena.Assets
{
    public enum BuildPackageType
    {
        /// <summary>
        /// 多AB包
        /// </summary>
        Bundles = 0,

        /// <summary>
        /// 多Byte文件包
        /// </summary>
        Bytes = 1,//
    }
}
