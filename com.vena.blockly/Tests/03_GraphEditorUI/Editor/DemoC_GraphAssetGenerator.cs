// -----------------------------------------------------------------------------
// Vena Blockly
// Visual scripting block graph runtime for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System.IO;
using UnityEditor;
using UnityEngine;
using Vena.Blockly.Editor;

namespace Vena.Blockly.Tests.GraphEditorUI
{
    /// <summary>
    /// Demo C — Editor 期 GraphAsset 演示。
    ///
    /// 触发：菜单 Tools/Vena/Blockly/Demos/03 GraphEditorUI/Generate Demo Graph Asset
    /// 行为：
    ///   1) 在 Assets/VenaBlocklyDemos/ 下创建（或复用）一个空 GraphAsset，
    ///   2) 写入一个最小的 placeholder JSON 字串（不走真实 IR 序列化器，仅为可视化演示），
    ///   3) Ping 资产 + 选中。
    /// </summary>
    public static class DemoC_GraphAssetGenerator
    {
        private const string MenuPath = "Tools/Vena/Blockly/Demos/03 GraphEditorUI/Generate Demo Graph Asset";

        private const string OutputDir = "Assets/VenaBlocklyDemos";

        private const string AssetFileName = "DemoC_GraphAsset.asset";

        private const string PlaceholderJson =
            "{\"version\":1,\"kind\":\"placeholder\",\"note\":\"Demo C placeholder — not produced by IBlocklyGraphSerializer.\"}";

        [MenuItem(MenuPath)]
        public static void Generate()
        {
            if (!AssetDatabase.IsValidFolder(OutputDir))
            {
                CreateFolderRecursive(OutputDir);
            }

            string assetPath = OutputDir + "/" + AssetFileName;

            var asset = AssetDatabase.LoadAssetAtPath<GraphAsset>(assetPath);
            bool created = false;
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<GraphAsset>();
                AssetDatabase.CreateAsset(asset, assetPath);
                created = true;
            }

            asset.SetJson(PlaceholderJson);
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);

            Debug.Log(created
                ? $"[Vena.Blockly][Demo C] Created GraphAsset at {assetPath}."
                : $"[Vena.Blockly][Demo C] Updated existing GraphAsset at {assetPath}.");
        }

        private static void CreateFolderRecursive(string path)
        {
            if (string.IsNullOrEmpty(path) || AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            string leaf = Path.GetFileName(path);
            if (string.IsNullOrEmpty(parent))
            {
                return;
            }

            CreateFolderRecursive(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
