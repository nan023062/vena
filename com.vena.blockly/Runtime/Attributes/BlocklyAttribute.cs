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
    /// 标注玩法代码作为 Blockly codegen 输入：可贴在宿主类 / 方法 / 属性 / 字段上，
    /// 由 Editor codegen 工具扫描并生成对应的节点源（Source）三件套。
    /// 与运行期 Source 族（<see cref="BlocklySourceAttribute"/> / <see cref="BlocklySourceSlotAttribute"/>）互斥，
    /// 同一类上不允许同存。
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field,
        Inherited = false,
        AllowMultiple = false)]
    public sealed class BlocklyAttribute : Attribute
    {
        public string DisplayName { get; }
        public bool IsStatic { get; }
        public string[] ParameterNames { get; }

        public BlocklyAttribute(string displayName)
        {
            DisplayName = displayName;
            IsStatic = false;
            ParameterNames = Array.Empty<string>();
        }

        public BlocklyAttribute(string displayName, bool isStatic, params string[] parameterNames)
        {
            DisplayName = displayName;
            IsStatic = isStatic;
            ParameterNames = parameterNames ?? Array.Empty<string>();
        }
    }
}
