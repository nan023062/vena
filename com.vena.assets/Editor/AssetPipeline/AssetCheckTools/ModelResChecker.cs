// -----------------------------------------------------------------------------
// Vena Assets
// Unity asset pipeline toolkit (AssetBundle / VersionControl / OSS / Compression).
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

/**************************************************************************
 *  说明：该文件为非打包框架内容，是根据当前项目的需要，自定义的“组合”打包
 *  策略。针对新项目可以删除和修改
 *      1 Model资源检查工具
 *      2 Model资源打包策略设置工具
 *  write by linan 2019-07-09
 * ***********************************************************************/

using UnityEngine;
using System.Collections;
using System.IO;
using UnityEditor;
using System.Collections.Generic;

namespace Vena.Assets
{
    public static class ModelResChecker
    {
        public readonly static string ModelPackage = "Model";
        public readonly static string ActorModelPath = "Model/ActorModel";
        public readonly static string MainCityModelPath = "Model/MainCityModel";

        public static void OnModelResUpdate(string[] floderPaths, string[] assetPaths)
        {

        }
    }
}
