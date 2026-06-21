// -----------------------------------------------------------------------------
// Vena Assets
// Unity asset pipeline toolkit (AssetBundle / VersionControl / OSS / Compression).
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System.Collections.Generic;

namespace Vena.Assets
{
    public enum StrategyMode
    {
        /// <summary>
        /// 默认状态 无策略用于安全检测
        /// </summary>
        Null = 0,

        /// <summary>
        /// 当前文件夹----单文件打包
        /// 文件夹下文件都单独打成Bundle
        /// </summary>
        OneFile = 1,
        OneFileGenMapping = 2,

        /// <summary>
        /// 当前文件夹----文件夹下文件全部打包
        /// </summary>
        AllFile = 3,
        AllFileGenMapping = 4,

        /// <summary>
        /// 当前文件夹----根据文件的大小打包（多个文件一起打包，确保AB不大于指定bytes）
        /// </summary>
        BySize = 5,
        BySizeGenMapping = 6,

        /// <summary>
        /// 当前文件夹----整个文件夹（包含子目录，递归）
        /// </summary>
        AllFolder = 7,
        AllFolderGenMapping = 8,

        /// <summary>
        /// 当前文件夹----不打AB，不设置Bundle名称，属于被依赖打包的资源
        /// </summary>
        NoBuild = 101,

        /// <summary>
        /// 所有子文件夹----使用模板策略
        /// 这样的策略，必须有一个模板文件夹"00Templet"来定义策略的模板
        /// 那么在该文件下创建的新子文件夹都会克隆这个策略模板
        /// </summary>
        Template = 102,
        //Count,
    }

    /// <summary>
    /// 打包策略数据
    /// </summary>
    public class BuildDependInfo
    {
        public StrategyMode mode;

        public string bundleName;

        public Dictionary<string, string> name2path;

        public BuildDependInfo()
        {
            name2path = new Dictionary<string, string>();
        }
    }
}

