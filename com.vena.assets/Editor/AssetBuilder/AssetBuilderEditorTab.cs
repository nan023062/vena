// -----------------------------------------------------------------------------
// Vena Assets
// Unity asset pipeline toolkit (AssetBundle / VersionControl / OSS / Compression).
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

/**********************************************************************************
 * FileName:    BundleEditorWin.cs
 * Description: 资源管理框架---AssetBundle打包编辑器窗口
 *              1 预览打包策略
 *              2 打包输出操作
 *              3 创建1个版本的资源包
 *              4 提供一键上传操作
 * History: 2019-07-09
 *********************************************************************************/
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Vena.Assets
{
    /// <summary>
    /// AssetBundle窗口
    /// </summary>
    public sealed class AssetBuilderEditorTab : AssetToolkitTab
    {
        private BuildTreeNode _selectBundleNode;
        private BuildTreeNode _buildTree;
        
        public override string TabName => "Asset Builder";

        private HorizontalButtonList _menuButtonList;
        
        protected override void OnEnterDraw()
        {
            autoRepaintOnSceneChange = true;
            minSize = new Vector2(650, 600);
            _buildTree = AssetBuildTree.Root;
         
            //获取当前目录树结构图
            AssetDatabase.RemoveUnusedAssetBundleNames();

            _menuButtonList = new HorizontalButtonList(120, 50, 10,
                new HorizontalButton("Save Settings", SaveBuildStrategy),
                new HorizontalButton("Build All",BuildAllPackagesAB),
                new HorizontalButton("Delete All",DeleteAllPackageAB),
                new HorizontalButton("Reset Ab Names", ResetAllAssetBundleNames),
                new HorizontalButton("Clear Ab Names", ClearAllAssetBundleNames));
        }
        
        protected override void OnExitDraw()
        {
        }
        
        protected override void OnDrawGUI()
        {
            CommonGUI.SeparatorLine("Switch Setting :");
            DrawEditorToggleOption("Use AssetBundle", ref AssetBuildSetting.Instance.useAssetBundle);

            //0 编辑器路径设置
            CommonGUI.SeparatorLine("Path Setting :");
            DrawEditorPathOption("Raw Asset Path", ref AssetBuildSetting.Instance.assetRootPath);
            DrawEditorPathOption("Build Output Path", ref AssetBuildSetting.Instance.bundleOutputPath);
            
            CommonGUI.SeparatorLine("Assets Tree：");
            _menuButtonList.OnDrawGUI();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            //2 打包资源树
            EditorGUILayout.BeginHorizontal("box");

            //2.1 Package列表
            EditorGUILayout.BeginVertical("box", GUILayout.Width(300));
            if (GUILayout.Button("Package List (click refresh)", GUILayout.Height(30)))
            {
                _buildTree = AssetBuildTree.RefreshAssetBuildTree(true);
            }
            if (null != _buildTree)
            {
                foreach (var buildNode in _buildTree.Children) DrawOnePackageOptions(buildNode);
            }
            EditorGUILayout.EndVertical();
            
            //2.2 选中目录的子目录
            EditorGUILayout.BeginVertical("box");
            DrawCurrentSelectNode();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }
        
        #region Toggle Editor

        private void DrawEditorToggleOption(string name, ref bool toggle)
        {
            toggle = EditorGUILayout.Toggle($"{name} : ", toggle);
        }

        #endregion

        #region Path Editor

        private void DrawEditorPathOption(string name, ref string path)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label($"   < {name} > : ", GUILayout.Width(160));
            GUILayout.Label(path, GUILayout.ExpandWidth(true));
            if (GUILayout.Button("...", GUILayout.Width(30)))
            {
                var newPath = EditorUtility.OpenFolderPanel("Selected", Application.dataPath, "");
                if(newPath != path)
                {
                    path =  Utility.AbsolutePathToUnityRelativePath(newPath);
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        #endregion

        #region Menu Buttons

        /// <summary>
        /// 绘制主菜单按钮
        /// </summary>
        public void DrawMenuButton(string buttonName, Action onClick)
        {
            if (GUILayout.Button(buttonName, GUILayout.Width(120), GUILayout.Height(50)))
            {
                onClick?.Invoke();
            }
        }

        private void SaveBuildStrategy()
        {
            if(null == _buildTree) return;
            
            bool result = true;
            var assetBuildDataMap = new Dictionary<string, BuildDependInfo>();

            //优先检查一下策略配置是否正确
            for (int i = 0; i < AssetBuildTree.Root.Children.Count; i++)
            {
                assetBuildDataMap.Clear();
                var bundleInfo = AssetBuildTree.Root.Children[i];
                if (bundleInfo.BuildType == BuildPackageType.Bundles &&
                    !bundleInfo.GetAssetBuildDataRecurively(ref assetBuildDataMap))
                {
                    result = false;
                    break;
                }
                string content = $"检查资源包 [{bundleInfo.PackageName}]...";
                EditorUtility.DisplayProgressBar("检查策略", content, (i + 1) * 1.0f / AssetBuildTree.Root.Children.Count);
            }
            EditorUtility.ClearProgressBar();
            if (result)
            {
                AssetBuildSetting.Save(true);
                EditorUtility.DisplayDialog("SaveBundleTree", "BuildBundle 策略保存成功！", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("SaveBundleTree", "BuildBundle 策略保存失败！具体问题请看控制台日志。", "OK");
            }
        }

        private void BuildAllPackagesAB()
        {
            if(null == _buildTree) return;
            
            for (int i = 0; i < _buildTree.Children.Count; i++)
            {
                var bundleInfo = _buildTree.Children[i];
                BundleEditorUtil.BuildAssetBundles(bundleInfo, false);
                string content = $"Build Package [{bundleInfo.PackageName}]...";
                EditorUtility.DisplayProgressBar("Build AssetBundle", content, (i + 1) * 1.0f / AssetBuildTree.Root.Children.Count);
            }
            EditorUtility.ClearProgressBar();
            if(EditorUtility.DisplayDialog("Build AssetBundle", "所有资源包Build完成！", "OK"))
            {
                string outputPath = Utility.GetAssetBundleOutputPath();
                EditorUtility.RevealInFinder(outputPath);
            }
        }

        private void DeleteAllPackageAB()
        {
            if(null == _buildTree) return;
            
            BundleEditorUtil.DeleteAssetBundles(_buildTree);
        }

        private void ResetAllAssetBundleNames()
        {
            if(null == _buildTree) return;
            
            foreach (var buildTreeNode in _buildTree.Children)
            {
                ResetAllBundleNames(buildTreeNode);
            }
            EditorUtility.DisplayDialog("ResetAllBundleNames", "重设所有BundleName 完成！", "OK");
        }
        
        private static void ResetAllBundleNames(BuildTreeNode assetNode)
        {
            var strategy = BuildStrategy.Get(assetNode.Strategy);
            
            strategy.SetAllAssetBundleName(assetNode);
            
            foreach (var bundleNode in assetNode.Children)
            {
                ResetAllBundleNames(bundleNode);
            }
        }

        private void ClearAllAssetBundleNames()
        {
            foreach (var assetBuildName in AssetDatabase.GetAllAssetBundleNames().ToArray())
            {
                AssetDatabase.RemoveAssetBundleName(assetBuildName, true);
            }
            
            AssetDatabase.RemoveUnusedAssetBundleNames();
            
            EditorUtility.DisplayDialog("ClearAllBundleNames", "清空 BundleName 完成！", "OK");
        }

        #endregion

        #region Build Tree Node
        
        private void DrawOnePackageOptions(BuildTreeNode packageRoot)
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(packageRoot.FolderName, GUILayout.Width(125)))
                _selectBundleNode = packageRoot;
            
            if (GUILayout.Button("Build", GUILayout.Width(40)))
                BundleEditorUtil.BuildAssetBundles(packageRoot);

            if (GUILayout.Button("Clear", GUILayout.Width(40)))
                BundleEditorUtil.DeleteAssetBundles(packageRoot);

            var curBuildType = EditorGUILayout.EnumPopup(packageRoot.BuildType, GUILayout.Width(80));
            if ((BuildPackageType)curBuildType != packageRoot.BuildType) packageRoot.BuildType = (BuildPackageType)curBuildType;

            EditorGUILayout.EndHorizontal();
        }

        private void DrawCurrentSelectNode()
        {
            if (_selectBundleNode != null)
            {
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("←", GUILayout.Height(30), GUILayout.Width(30)))
                {
                    _selectBundleNode = _selectBundleNode.Parent;
                    if (_selectBundleNode == AssetBuildTree.Root) _selectBundleNode = null;
                    return;
                }
                GUILayout.Button(_selectBundleNode.PackagePath, GUILayout.Height(30));

                EditorGUILayout.EndHorizontal();

                //显示当前选中目录的子列表
                foreach (var bundleNode in _selectBundleNode.Children)
                {
                    EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

                    if (GUILayout.Button(bundleNode.FolderName, GUILayout.Width(140)))  _selectBundleNode = bundleNode;

                    StrategyMode newStrategy = (StrategyMode)EditorGUILayout.EnumPopup(bundleNode.Strategy, GUILayout.ExpandWidth(true));
                    if (bundleNode.Strategy != newStrategy) bundleNode.SetBundleStrategy(newStrategy, true);

                    EditorGUILayout.EndHorizontal();
                }
            }
            else
            {
                GUILayout.Button("None Selected", GUILayout.Height(30));
            }
        }



        #endregion
        
        #region Asset Option Api
        
        [MenuItem("Assets/AssetToolkit/Asset Builder/应用模板策略", false, 0)]
        private static void BuildBundleCloneSample()
        {
            SetSelectBundleNodeStrategy(StrategyMode.Template, false);
        }

        [MenuItem("Assets/AssetToolkit/Asset Builder/单文件打包", false, 1)]
        private static void BuildBundleOneFile()
        {
            SetSelectBundleNodeStrategy(StrategyMode.OneFile, false);
        }

        [MenuItem("Assets/AssetToolkit/Asset Builder/单文件打包（递归）", false, 2)]
        private static void BuildBundleOneFileRecursively()
        {
            SetSelectBundleNodeStrategy(StrategyMode.OneFile, true);
        }

        [MenuItem("Assets/AssetToolkit/Asset Builder/子文件打包", false, 3)]
        private static void BuildBundleAllFile()
        {
            SetSelectBundleNodeStrategy(StrategyMode.AllFile, false);
        }

        [MenuItem("Assets/AssetToolkit/Asset Builder/子文件打包（递归）", false, 4)]
        private static void BuildBundleAllFileRecursively()
        {
            SetSelectBundleNodeStrategy(StrategyMode.AllFile, true);
        }

        [MenuItem("Assets/AssetToolkit/Asset Builder/文件夹打包（递归）", false, 5)]
        private static void BuildBundleAllFolder()
        {
            SetSelectBundleNodeStrategy(StrategyMode.AllFolder, true);
        }

        [MenuItem("Assets/AssetToolkit/Asset Builder/子文件不打包", false, 6)]
        private static void BuildBundleNoBuild()
        {
            SetSelectBundleNodeStrategy(StrategyMode.NoBuild, false);
        }

        [MenuItem("Assets/AssetToolkit/Asset Builder/子文件不打包（递归）", false, 7)]
        private static void BuildBundleNoBuildRecursively()
        {
            SetSelectBundleNodeStrategy(StrategyMode.NoBuild, true);
        }

        private static void SetSelectBundleNodeStrategy(StrategyMode mode, bool recursively)
        {
            string assetPath = BundleEditorUtil.GetSelectedPathOrFallback();
            
            string inPackagePath = Utility.GetRelativePath(assetPath, Utility.AssetGameAssets);
            
            var bundleTreeNode = AssetBuildTree.GetOrCreateBuildNode(inPackagePath);
            
            if (bundleTreeNode == null) return;
            
            bundleTreeNode.SetBundleStrategy(mode, recursively);
            
            AssetBuildSetting.Save(true);
        }

        #endregion
    }
}
