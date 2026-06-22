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
    /// 标注 Blockly runtime 节点源类（Source）上的输入字段 / 属性：声明显示名与 UI / IR / Push 顺序号
    /// （Pop 顺序 = Push 顺序的反序，详见 Editor 合约 §2）。
    /// 与 <see cref="BlocklySourceAttribute"/> 同属 Source 族，描述节点源上的可见输入 slot。
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class BlocklySourceSlotAttribute : Attribute
    {
        public string DisplayName { get; }
        public int Order { get; }

        public BlocklySourceSlotAttribute(string displayName, int order)
        {
            DisplayName = displayName;
            Order = order;
        }
    }
}
