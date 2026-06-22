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
    /// 节点 IR —— 一个画布节点的内存形态。
    /// 固定 4 字段：guid / sourceType / properties / position；其他字段禁。
    /// </summary>
    public sealed class NodeIR
    {
        /// <summary>节点稳定身份；新建分配、跨 round-trip 保留。</summary>
        public Guid Guid;

        /// <summary>
        /// Source 类的程序集限定名（AQN 稳定子集：`Namespace.TypeName, AssemblyName`）。
        /// AOT 不变量：仅限 codegen 产物 *Source 与手写 Source；不允许运行期反射构造任意 Type。
        /// </summary>
        public string SourceType;

        /// <summary>
        /// [BlocklySourceProperty] 槽位的字面值或子节点引用。
        /// 顺序按 [BlocklySourceProperty.order] 升序；顺序 round-trip 等价。
        /// </summary>
        public List<NodePropertyIR> Properties;

        /// <summary>编辑器画布坐标；运行期忽略。JSON 出 {x,y}。</summary>
        public Vec2 Position;

        public NodeIR()
        {
            Guid = Guid.Empty;
            SourceType = string.Empty;
            Properties = new List<NodePropertyIR>();
            Position = default;
        }
    }

    /// <summary>
    /// NodeIR.Properties 的单条目 —— 表达一个 [BlocklySourceProperty] 槽位。
    /// JSON 形态：`{key:string, value:json}`。键名固定。
    /// </summary>
    public sealed class NodePropertyIR
    {
        /// <summary>
        /// 槽位 key —— 与 [BlocklySourceProperty] 字段名严格对齐。
        /// AOT 不变量：必须能在 sourceType 槽位集合内静态匹配。
        /// </summary>
        public string Key;

        /// <summary>
        /// 槽位 value —— 字面值或子节点引用。
        /// 字面值：`PropertyValueIR.Type = "literal"`、Value = 原始 JSON 标量 / 对象。
        /// 子节点引用：`PropertyValueIR.Type = "nodeRef"`、Value = `{nodeGuid: Guid}`。
        /// AOT 不变量：字面值类型由 sourceType 槽位字段类型静态锁定。
        /// </summary>
        public PropertyValueIR Value;

        public NodePropertyIR()
        {
            Key = string.Empty;
            Value = null;
        }

        public NodePropertyIR(string key, PropertyValueIR value)
        {
            Key = key ?? string.Empty;
            Value = value;
        }
    }

    /// <summary>
    /// 二维坐标，用于 NodeIR.Position。
    /// 独立 plain struct，不依赖 UnityEngine.Vector2，保 Runtime asmdef noEngineReferences=true。
    /// JSON 形态：`{x:float, y:float}`。
    /// </summary>
    public struct Vec2 : IEquatable<Vec2>
    {
        public float X;
        public float Y;

        public Vec2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public bool Equals(Vec2 other) => X == other.X && Y == other.Y;
        public override bool Equals(object obj) => obj is Vec2 v && Equals(v);
        public override int GetHashCode() => unchecked(X.GetHashCode() * 397 ^ Y.GetHashCode());
        public static bool operator ==(Vec2 a, Vec2 b) => a.Equals(b);
        public static bool operator !=(Vec2 a, Vec2 b) => !a.Equals(b);
    }
}
