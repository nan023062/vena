// -----------------------------------------------------------------------------
// Vena Blockly
// Visual scripting block graph runtime for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using UnityEngine;

namespace Vena.Blockly.Editor
{

    /// <summary>
    /// Codegen 白名单 + 输出根目录配置。
    /// 资产路径约定：项目内 `Assets/.../BlocklyCodegenConfig.asset` 或包内 `Editor/Config/BlocklyCodegenConfig.asset`。
    /// </summary>
    [CreateAssetMenu(fileName = "BlocklyCodegenConfig", menuName = "Vena/Blockly/Codegen Config", order = 0)]
    public sealed class CodegenConfig : ScriptableObject
    {
        [Tooltip("被扫描的程序集名清单（asmdef name）。空 = 不扫描。")]
        public string[] AssemblyWhitelist;

        [Tooltip("被扫描的类型全名清单（namespace.Type）。空 = 接受程序集内全部带 [Blockly] 的类。")]
        public string[] TypeWhitelist;

        [Tooltip("Provider 产物输出根目录（相对项目根 / Assets 的 Unity 路径）。三件套不再使用此字段——其落点 = 各源类 `Generated/` 子目录；本字段仅 Provider 用。")]
        public string OutputRoot = "Packages/com.vena.blockly/Runtime/Generated";
    }
}
