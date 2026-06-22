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
    /// 标注 Blockly runtime 节点源类（Source）上的槽位字段：声明槽位显示名与 UI / IR / Pop 顺序号。
    /// 与 <see cref="BlocklySourceAttribute"/> 同属 Source 族，描述节点源上的可见输入。
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
