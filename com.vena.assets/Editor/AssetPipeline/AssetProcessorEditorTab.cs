// -----------------------------------------------------------------------------
// Vena Assets
// Unity asset pipeline toolkit (AssetBundle / VersionControl / OSS / Compression).
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Vena.Assets
{
    public sealed class AssetProcessorEditorTab : AssetToolkitTab
    {
        public override string TabName => "Asset Processor";

        private ToggleList<AssetProcessCommand> _processCommandList;
        
        protected override void OnEnterDraw()
        {
            List<AssetProcessCommand> commandList = new List<AssetProcessCommand>();
            
            foreach (var type in GetType().Assembly.GetTypes().Where(t=> t.IsSubclassOf(typeof(AssetProcessCommand)) && !t.IsAbstract))
            {
                commandList.Add(Activator.CreateInstance(type) as AssetProcessCommand);   
            }
            
            _processCommandList = new ToggleList<AssetProcessCommand>(commandList.ToArray(), 760, 30);
        }
        
        protected override void OnExitDraw()
        {
            _processCommandList = null;
        }

        protected override void OnDrawGUI()
        {
            if(_processCommandList == null) return;

            _processCommandList.OnDrawGUI();
        }

        #region Asset Option Api

        [MenuItem("Assets/AssetToolkit/Asset Processor/查找未使用资源", false, 1)]
        private static void CheckUnusedAsset()
        {
            CheckUnusedAssetWin.CheckUnusedAsset();
        }

        [MenuItem("Assets/AssetToolkit/Asset Processor/查找引用的资源", false, 2)]
        private static void CheckOnAssetRef()
        {
            CheckAssetReferencesWin.CheckOnAssetRefs();
        }

        [MenuItem("Assets/AssetToolkit/Asset Processor/查找依赖资源", false, 3)]
        private static void Test()
        {
            CheckAssetDependenciesWin.CheckOnAssetRefs();            
        }

        [MenuItem("Assets/AssetToolkit/Asset Processor/检查路径下资源重名", false, 4)]
        private static void CheckSameFileName()
        {
            CheckSameAssetWin.CheckSameFileName();
        }

        [MenuItem("Assets/AssetToolkit/Asset Processor/所有图片纹理格式设置Sprite", false, 5)]
        private static void BatchSelectFolderImgToSprite()
        {
            UIResChecker.BatchImgToSpriteFormat(BundleEditorUtil.GetSelectedPathOrFallback());
        }
        
        #endregion
    }
}
