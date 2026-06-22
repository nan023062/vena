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
    /// 标注 Blockly runtime 节点源类（Source）：声明在编辑器菜单中的路径，并绑定其运行期 Node 实现类型。
    /// 与 codegen 输入族 <see cref="BlocklyCodeGenAttribute"/> 区分 —— 此族描述运行期已就位的节点源。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class BlocklySourceAttribute : Attribute
    {
        public string MenuPath { get; }

        public Type NodeType { get; }

        public BlocklySourceAttribute(string menuPath, Type nodeType)
        {
            MenuPath = menuPath;
            NodeType = nodeType;
        }

        public static Type GetNodeType(Type sourceType)
        {
            var attr = (BlocklySourceAttribute)Attribute.GetCustomAttribute(sourceType, typeof(BlocklySourceAttribute));
            return attr?.NodeType;
        }
    }
}
