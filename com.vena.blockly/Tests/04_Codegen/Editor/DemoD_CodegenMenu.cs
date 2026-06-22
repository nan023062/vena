// -----------------------------------------------------------------------------
// Vena Blockly
// Visual scripting block graph runtime for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Vena.Blockly.Editor;

namespace Vena.Blockly.Tests.Codegen
{
    /// <summary>
    /// Demo D — Editor 期 codegen 演示。
    ///
    /// 双 step 流程（KD9 强制反例：Editor demo 走菜单/资产，禁 sample scene）：
    ///
    ///   Step 1：菜单 Tools/Vena/Blockly/Demos/04 Codegen/Setup Demo D Config
    ///     在 Assets/VenaBlocklyDemos/04_Codegen/ 下创建 DemoD_CodegenConfig.asset，
    ///     OutputRoot 写入 ${PackagePath}/Tests/04_Codegen/Generated 解析后的实际路径，
    ///     AssemblyWhitelist=["Vena.Blockly.Tests.Codegen"]，
    ///     TypeWhitelist=空（接受白名单 asmdef 内全部 [BlocklySource] 源类）。
    ///
    ///   Step 2：菜单 Tools/Vena/Blockly/Demos/04 Codegen/Run Demo D Codegen
    ///     调用包心 CodegenMenu.RunCodegen()——它内部 LoadConfig(t:CodegenConfig)
    ///     找到 Demo D config，扫白名单 asmdef、Emit 三件套到 OutputRoot。
    ///     Demo D 的 InstanceMethod.cs（位于 Tests/04_Codegen/Scripts/，已删 #region Impl/#region Source 仅留源类）
    ///     被扫描，产出 InstanceMethod.g.cs + GeneratedNodeMetadataProvider.g.cs 到 Generated/。
    ///
    /// ${PackagePath} 占位符：
    ///   值 = 包根的 Unity 路径（典型 `Packages/com.vena.blockly`）。
    ///   解析在 Setup Step 写盘前完成；写入 .asset 的字符串是已解析的 concrete 路径。
    ///   理由：CodegenConfig.OutputRoot 是 string 字段，无内置 substitution；这里就地解析。
    /// </summary>
    public static class DemoD_CodegenMenu
    {
        private const string PackagePath = "Packages/com.vena.blockly";

        private const string DemoConfigDir = "Assets/VenaBlocklyDemos/04_Codegen";

        private const string DemoConfigAssetName = "DemoD_CodegenConfig.asset";

        private const string OutputRootPlaceholder = "${PackagePath}/Tests/04_Codegen/Generated";

        private const string DemoAssemblyName = "Vena.Blockly.Tests.Codegen";

        private const string MenuSetup = "Tools/Vena/Blockly/Demos/04 Codegen/Setup Demo D Config";

        private const string MenuRun = "Tools/Vena/Blockly/Demos/04 Codegen/Run Demo D Codegen";

        [MenuItem(MenuSetup)]
        public static void SetupConfig()
        {
            string configPath = DemoConfigDir + "/" + DemoConfigAssetName;
            EnsureFolder(DemoConfigDir);

            var existing = AssetDatabase.LoadAssetAtPath<CodegenConfig>(configPath);
            CodegenConfig config = existing != null ? existing : ScriptableObject.CreateInstance<CodegenConfig>();

            string resolvedOutputRoot = ResolveOutputRoot(OutputRootPlaceholder);
            config.OutputRoot = resolvedOutputRoot;
            config.AssemblyWhitelist = new[] { DemoAssemblyName };
            config.TypeWhitelist = Array.Empty<string>();

            if (existing == null)
            {
                AssetDatabase.CreateAsset(config, configPath);
            }
            else
            {
                EditorUtility.SetDirty(config);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = config;
            EditorGUIUtility.PingObject(config);

            Debug.Log(
                $"[Vena.Blockly][Demo D] Config ready at {configPath}. " +
                $"OutputRoot = {resolvedOutputRoot} (resolved from {OutputRootPlaceholder}). " +
                $"AssemblyWhitelist = [{DemoAssemblyName}].");
        }

        [MenuItem(MenuRun)]
        public static void RunCodegen()
        {
            string configPath = DemoConfigDir + "/" + DemoConfigAssetName;
            var config = AssetDatabase.LoadAssetAtPath<CodegenConfig>(configPath);
            if (config == null)
            {
                Debug.LogError(
                    $"[Vena.Blockly][Demo D] Demo D config not found at {configPath}. " +
                    $"Run '{MenuSetup}' first.");
                return;
            }

            // 委托给包心 CodegenMenu.RunCodegen()。它的 LoadConfig 是 t:CodegenConfig
            // 取首个；只要项目内此前没有别的 CodegenConfig，就会命中 Demo D 的。
            CodegenMenu.RunCodegen();
        }

        // ------- 占位符解析 -------

        /// <summary>
        /// 解析 OutputRoot 占位符。当前支持 ${PackagePath} → 包根 Unity 路径。
        /// 未来可扩展更多占位符（如 ${ProjectPath}），保持就地替换语义。
        /// </summary>
        private static string ResolveOutputRoot(string template)
        {
            return string.IsNullOrEmpty(template)
                ? string.Empty
                : template.Replace("${PackagePath}", PackagePath);
        }

        // ------- 路径辅助 -------

        private static void EnsureFolder(string path)
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

            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
