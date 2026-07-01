// -----------------------------------------------------------------------------
// Vena Blockly
// Visual scripting block graph runtime for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Vena.Blockly
{

    /// <summary>
    /// 节点元数据 —— Editor 扫描期收集、运行期通过 <c>INodeMetadataProvider</c> 查询。
    /// 字段值由 <c>[BlocklySource]</c> / <c>[BlocklySourceSlot]</c> 注解原值透传。
    /// </summary>
    public sealed class NodeMetadata
    {
        /// <summary>源类（带 [BlocklySource] 的类，可能是 codegen 产物 *Source）。</summary>
        public Type SourceType { get; }

        /// <summary>运行期 IExpressionBlock / IBehaviorNode 实现类（=`BlocklySource.NodeType`）。</summary>
        public Type NodeType { get; }

        /// <summary>调色板菜单路径，原值透传，可含 `/` 分层。</summary>
        public string MenuPath { get; }

        /// <summary>UI / IR / Pop 顺序锁的字段集（按 order 升序）。</summary>
        public IReadOnlyList<NodePropertyMetadata> Properties { get; }

        public NodeMetadata(Type sourceType, Type nodeType, string menuPath, IReadOnlyList<NodePropertyMetadata> properties)
        {
            SourceType = sourceType ?? throw new ArgumentNullException(nameof(sourceType));
            NodeType = nodeType ?? throw new ArgumentNullException(nameof(nodeType));
            MenuPath = menuPath ?? string.Empty;
            Properties = properties ?? Array.Empty<NodePropertyMetadata>();
        }
    }

    /// <summary>NodeMetadata 子条目：源类上一个 [BlocklySourceSlot] 字段/属性的固化形态。</summary>
    public sealed class NodePropertyMetadata
    {
        /// <summary>UI 显示名（原值透传）。</summary>
        public string DisplayName { get; }

        /// <summary>顺序锁：UI / IR / Pop 三者一致。</summary>
        public int Order { get; }

        /// <summary>字段名（C# 标识符）。</summary>
        public string FieldName { get; }

        /// <summary>字段静态类型。</summary>
        public Type FieldType { get; }

        public NodePropertyMetadata(string displayName, int order, string fieldName, Type fieldType)
        {
            DisplayName = displayName ?? string.Empty;
            Order = order;
            FieldName = fieldName ?? throw new ArgumentNullException(nameof(fieldName));
            FieldType = fieldType ?? throw new ArgumentNullException(nameof(fieldType));
        }
    }
}
