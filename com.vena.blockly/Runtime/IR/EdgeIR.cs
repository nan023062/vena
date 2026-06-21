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
    /// 边 IR —— 一根连线（控制流 / 值流）的内存形态。
    /// 固定 3 字段：from / to / wireKind；其他字段禁。
    /// </summary>
    public sealed class EdgeIR
    {
        /// <summary>出端口。</summary>
        public PortRef From;

        /// <summary>入端口。</summary>
        public PortRef To;

        /// <summary>
        /// wire 种类（必填、无默认值）：
        /// - <see cref="WireKind.Control"/> 控制流：仅 Behavior 节点之间。
        /// - <see cref="WireKind.Value"/>   值流：Logic 节点 → 任意端口；不可与 Control 交叉。
        /// 缺字段 / 其他取值 → 反序列化报错。
        /// </summary>
        public WireKind WireKind;

        public EdgeIR()
        {
            From = default;
            To = default;
            WireKind = WireKind.Control;
        }
    }

    /// <summary>
    /// 端口引用 —— 由 (nodeGuid, port 名) 双字段唯一定位。
    /// port 字段语义：
    ///   Control 端口 = 节点声明的具名出/入口（如 "next" / "true" / "false"）。
    ///   Value   端口 = [UgcSourceProperty] 槽位名（与 NodeIR.Properties 对齐）。
    /// </summary>
    public struct PortRef : IEquatable<PortRef>
    {
        /// <summary>关联的节点 Guid（与 NodeIR.Guid 对齐）。</summary>
        public Guid NodeGuid;

        /// <summary>端口名 —— 控制端口为节点声明出入口名、值端口为槽位名。</summary>
        public string Port;

        public PortRef(Guid nodeGuid, string port)
        {
            NodeGuid = nodeGuid;
            Port = port ?? string.Empty;
        }

        public bool Equals(PortRef other) => NodeGuid == other.NodeGuid && Port == other.Port;
        public override bool Equals(object obj) => obj is PortRef p && Equals(p);
        public override int GetHashCode() => unchecked(NodeGuid.GetHashCode() * 397 ^ (Port?.GetHashCode() ?? 0));
        public static bool operator ==(PortRef a, PortRef b) => a.Equals(b);
        public static bool operator !=(PortRef a, PortRef b) => !a.Equals(b);
    }

    /// <summary>
    /// wire 种类。JSON 形态：字符串字面 "Control" | "Value"。
    /// 不可缺字段、不可其他取值。
    /// </summary>
    public enum WireKind
    {
        Control,
        Value,
    }
}
