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
    /// 标注宿主类字段 / 属性作为 Blockly codegen 输入（CodeGen）：声明显示名。
    /// codegen 工具据此为该成员生成对应的节点源（Source）三件套。
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class BlocklyCodeGenMemberAttribute : Attribute
    {
        public string DisplayName { get; }

        public BlocklyCodeGenMemberAttribute(string displayName)
        {
            DisplayName = displayName;
        }
    }
}
