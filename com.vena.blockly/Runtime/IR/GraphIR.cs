using System;
using System.Collections.Generic;

namespace Vena.Blockly
{

    /// <summary>
    /// IR 顶层 Schema —— 一张 Behavior 或 Logic 图的内存形态。
    /// 字段顺序 = 序列化字段顺序（Editor 顶层合约 §4.2 锁）：
    /// schema / kind / rootNodeGuid / nodes / edges。
    /// 其他字段（version / meta / comment）禁止扩展。
    /// </summary>
    public sealed class GraphIR
    {
        /// <summary>当前 Schema 版本号（合约 §4.2 锁定 = 1）。</summary>
        public const int CurrentSchema = 1;

        /// <summary>IR 版本号，breaking 变更须升版（§4.2）。</summary>
        public int Schema;

        /// <summary>图类型（§4.2）：Behavior 或 Logic；其他取值反序列化报错。</summary>
        public GraphKind Kind;

        /// <summary>根节点 Guid（§4.2）：Behavior=入口、Logic=求值终点。</summary>
        public Guid RootNodeGuid;

        /// <summary>节点列表（§4.3）；顺序 round-trip 等价。</summary>
        public List<NodeIR> Nodes;

        /// <summary>边列表（§4.4）；顺序 round-trip 等价。</summary>
        public List<EdgeIR> Edges;

        public GraphIR()
        {
            Schema = CurrentSchema;
            Kind = GraphKind.Behavior;
            RootNodeGuid = Guid.Empty;
            Nodes = new List<NodeIR>();
            Edges = new List<EdgeIR>();
        }
    }

    /// <summary>
    /// 图类型枚举。JSON 形态：字符串字面 "Behavior" | "Logic"（合约 §4.2）。
    /// 其他取值 → 反序列化报错（不容错回填默认）。
    /// </summary>
    public enum GraphKind
    {
        Behavior,
        Logic,
    }
}
