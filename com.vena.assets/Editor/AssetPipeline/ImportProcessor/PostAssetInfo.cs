// -----------------------------------------------------------------------------
// Vena Assets
// Unity asset pipeline toolkit (AssetBundle / VersionControl / OSS / Compression).
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

/**********************************************************************************
 * FileName:    PostAssetInfo.cs
 * Description: 资源管理框架
 * History: 2019-07-09
 *********************************************************************************/
using System;
using System.Collections.Generic;

namespace Vena.Assets
{
    public class PostAssetInfo : IComparable<PostAssetInfo>
    {
        public string assetPath;
        public bool isNewAsset;
        
        public int CompareTo(PostAssetInfo other)
        {
            if (isNewAsset == other.isNewAsset)
            {
                int length = assetPath.Length;
                int otherLength = other.assetPath.Length;
                if (length > otherLength) return 1;
                else if (length < otherLength) return -1;
                else return 0;
            }
            return isNewAsset ? 1 : -1;
        }
    }
}