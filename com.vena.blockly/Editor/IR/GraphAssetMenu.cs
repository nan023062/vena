// -----------------------------------------------------------------------------
// Vena Blockly
// Visual scripting block graph runtime for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System.IO;
using UnityEditor;
using UnityEngine;

namespace Vena.Blockly.Editor
{

    /// <summary>
    /// 资产创建菜单：
    ///   Assets/Create/Vena/Blockly/Behavior Graph → 新建 GraphAsset，kind="Behavior"。
    ///   Assets/Create/Vena/Blockly/Logic Graph    → 同上、kind="Logic"。
    /// `_json` 初始化为最小骨架（schema / kind / rootNodeGuid / nodes:[] / edges:[]）。
    /// </summary>
    public static class GraphAssetMenu
    {
        private const string BehaviorMenu = "Assets/Create/Vena/Blockly/Behavior Graph";
        private const string LogicMenu    = "Assets/Create/Vena/Blockly/Logic Graph";

        [MenuItem(BehaviorMenu, priority = 200)]
        public static void CreateBehaviorBlockly() => CreateAt(GraphKind.Behavior, "BehaviorBlockly");

        [MenuItem(LogicMenu, priority = 201)]
        public static void CreateExpressionBlockly() => CreateAt(GraphKind.Logic, "ExpressionBlockly");

        private static void CreateAt(GraphKind kind, string defaultName)
        {
            var ir = new GraphIR { Schema = GraphIR.CurrentSchema, Kind = kind };
            var serializer = new JsonGraphSerializer();
            string json = serializer.ToJson(ir);

            var asset = ScriptableObject.CreateInstance<GraphAsset>();
            asset.SetJson(json);

            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(path)) path = "Assets";
            else if (!Directory.Exists(path)) path = Path.GetDirectoryName(path);

            string assetPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(path, $"{defaultName}.asset"));
            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }
    }
}
