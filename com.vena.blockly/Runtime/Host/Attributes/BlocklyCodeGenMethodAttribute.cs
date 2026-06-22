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
    /// 标注宿主类方法作为 Blockly codegen 输入（CodeGen）：声明显示名、是否静态、各参数显示名。
    /// codegen 工具据此为该方法生成对应的节点源（Source）三件套。
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class BlocklyCodeGenMethodAttribute : Attribute
    {
        public string DisplayName { get; }

        public bool IsStatic { get; }

        public string[] ParameterNames { get; }

        public BlocklyCodeGenMethodAttribute(string displayName, bool isStatic, params string[] parameterNames)
        {
            DisplayName = displayName;
            IsStatic = isStatic;
            ParameterNames = parameterNames;
        }
    }
}
