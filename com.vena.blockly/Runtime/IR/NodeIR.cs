using System;
using System.Collections.Generic;

namespace Vena.Blockly
{

    /// <summary>
    /// 节点 IR —— 一个画布节点的内存形态（Editor 顶层合约 §4.3）。
    /// 固定 4 字段：guid / sourceType / properties / position；其他字段禁。
    /// </summary>
    public sealed class NodeIR
    {
        /// <summary>节点稳定身份；新建分配、跨 round-trip 保留（§4.5 不变量 1）。</summary>
        public Guid Guid;

        /// <summary>
        /// Source 类的程序集限定名（AQN 稳定子集：`Namespace.TypeName, AssemblyName`）。
        /// AOT 不变量 1：仅限 codegen 产物 *Source 与手写 Source；不允许运行期反射构造任意 Type。
        /// </summary>
        public string SourceType;

        /// <summary>
        /// [UgcSourceProperty] 槽位的字面值或子节点引用。
        /// 顺序按 [UgcSourceProperty.order] 升序（§4.3 / §1 / §2 三者一致原则）。
        /// 顺序 round-trip 等价（§4.5 不变量 3）。
        /// </summary>
        public List<NodePropertyIR> Properties;

        /// <summary>编辑器画布坐标；运行期忽略（§4.3）。JSON 出 {x,y}。</summary>
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
    /// NodeIR.Properties 的单条目 —— 表达一个 [UgcSourceProperty] 槽位。
    /// JSON 形态：`{key:string, value:json}`。键名固定（§4.3）。
    /// </summary>
    public sealed class NodePropertyIR
    {
        /// <summary>
        /// 槽位 key —— 与 [UgcSourceProperty] 字段名严格对齐。
        /// AOT 不变量 2：必须能在 sourceType 槽位集合内静态匹配。
        /// </summary>
        public string Key;

        /// <summary>
        /// 槽位 value —— 字面值或子节点引用。
        /// 字面值：`PropertyValueIR.Type = "literal"`、Value = 原始 JSON 标量 / 对象。
        /// 子节点引用：`PropertyValueIR.Type = "nodeRef"`、Value = `{nodeGuid: Guid}`。
        /// AOT 不变量 3：字面值类型由 sourceType 槽位字段类型静态锁定。
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
    /// 二维坐标，用于 NodeIR.Position（合约 §4.3 描述为 Vector2）。
    /// 这里独立 plain struct，不依赖 UnityEngine.Vector2，保 Runtime asmdef noEngineReferences=true。
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
