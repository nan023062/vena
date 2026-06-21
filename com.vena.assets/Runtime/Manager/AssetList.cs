// -----------------------------------------------------------------------------
// Vena Assets
// Unity asset pipeline toolkit (AssetBundle / VersionControl / OSS / Compression).
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

/**********************************************************************************
 * FileName:    AssetList.cs
 * Description: 资源管理框架---资源和Bundle的名称映射
 * History: 2019-07-09
 *********************************************************************************/

using UnityEngine;
using System;
using UnityEngine.Serialization;

namespace Vena.Assets
{
    public class AssetList : ScriptableObject
    {
        public AssetContext[] contexts;
    }

    [Serializable]
    public class AssetContext
    {
        public string assetName;
        public string bundleName;
    }
}
