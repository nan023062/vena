// -----------------------------------------------------------------------------
// Vena Blockly
// Visual scripting block graph runtime for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

namespace Vena.Blockly.Editor
{

    /// <summary>
    /// IR 编解码原语。
    /// 归属：Editor 子模块。Editor.UI 与 Runtime IR 加载器均消费。
    /// 不复用 Runtime <see cref="IBlocklySerializer"/>（字节流原语，输入域 / 输出域 / 变更原因均不同）。
    /// 不进 IBlocklyHost 聚合门面。
    /// </summary>
    internal interface IBlocklyGraphSerializer
    {
        /// <summary>GraphIR → canonical JSON 串。</summary>
        string ToJson(GraphIR ir);

        /// <summary>canonical / 非 canonical JSON 串 → GraphIR；schema 校验失败抛 <see cref="BlocklyIRSchemaException"/>。</summary>
        GraphIR FromJson(string json);
    }
}
