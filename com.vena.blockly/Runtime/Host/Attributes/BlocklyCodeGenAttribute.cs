// -----------------------------------------------------------------------------
// Vena Blockly
// Visual scripting block graph runtime for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System;

namespace Vena.Blockly
{

    /// <summary>
    /// 标注宿主类作为 Blockly codegen 输入（CodeGen）：声明其在节点目录中的显示名。
    /// 配合 <see cref="BlocklyCodeGenMethodAttribute"/> / <see cref="BlocklyCodeGenMemberAttribute"/> 共同标注成员，供 codegen 工具扫描并产出节点源（Source）。
    /// 与运行期 Source 族区分 —— 此族描述「待生成」的输入面，产物由 <see cref="BlocklyCodeGeneratedAttribute"/> 标记。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class BlocklyCodeGenAttribute : Attribute
    {
        public string DisplayName { get; }

        public BlocklyCodeGenAttribute(string displayName)
        {
            DisplayName = displayName;
        }
    }
}
