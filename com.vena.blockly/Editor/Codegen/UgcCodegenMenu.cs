// -----------------------------------------------------------------------------
// Vena Blockly
// Visual scripting block graph runtime for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using UnityEditor;
using UnityEngine;

namespace Vena.Blockly.Editor
{

    /// <summary>
    /// UGC codegen 菜单触发入口。扫描白名单内带 [BlocklyCodeGen] 的源类、生成三件套并写盘到 OutputRoot。
    /// </summary>
    public static class UgcCodegenMenu
    {
        private const string MenuRunCodegen = "Tools/Vena/Blockly/Run UGC Codegen";

        private const string MenuLocateConfig = "Tools/Vena/Blockly/Locate UGC Codegen Config";

        [MenuItem(MenuRunCodegen, priority = 100)]
        public static void RunCodegen()
        {
            var config = LoadConfig();
            if (config == null)
            {
                Debug.LogError("[Vena.Blockly] UgcCodegenConfig.asset not found. Create one via Assets > Create > Vena > Blockly > UGC Codegen Config.");
                return;
            }

            try
            {
                var sources = UgcAnnotationScanner.Scan(config);
                if (sources.Count == 0)
                {
                    Debug.LogWarning(
                        $"[Vena.Blockly] UGC codegen: 扫描结果为空。检查白名单配置 — Assemblies={config.AssemblyWhitelist?.Length ?? 0}, " +
                        $"Types={config.TypeWhitelist?.Length ?? 0}.");
                    return;
                }

                var report = UgcCodeWriter.Emit(sources, config.OutputRoot);
                AssetDatabase.Refresh();

                Debug.Log(
                    $"[Vena.Blockly] UGC codegen done. sources={sources.Count}, " +
                    $"written={report.Written.Count}, unchanged={report.Unchanged.Count}, " +
                    $"OutputRoot={config.OutputRoot}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[Vena.Blockly] UGC codegen failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        [MenuItem(MenuLocateConfig, priority = 101)]
        public static void LocateConfig()
        {
            var config = LoadConfig();
            if (config == null)
            {
                Debug.LogError("[Vena.Blockly] UgcCodegenConfig.asset not found.");
                return;
            }

            Selection.activeObject = config;
            EditorGUIUtility.PingObject(config);
        }

        /// <summary>查找项目内首个 <see cref="UgcCodegenConfig"/> 资产。无则返回 null。</summary>
        internal static UgcCodegenConfig LoadConfig()
        {
            var guids = AssetDatabase.FindAssets("t:UgcCodegenConfig");
            if (guids == null || guids.Length == 0)
            {
                return null;
            }

            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<UgcCodegenConfig>(path);
        }
    }
}
