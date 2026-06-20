using System.Collections.Generic;

namespace Vena.Blockly
{

    /// <summary>
    /// 节点元数据查询入口。Editor codegen 阶段填充 / 运行期消费。
    /// 单向落点：Editor → Runtime；具体实现可由 codegen 一并产出（Phase 2 PR-3）。
    /// </summary>
    public interface INodeMetadataProvider
    {
        /// <summary>按源类（带 [UgcSource] 的类）查询元数据；命中返回 true。</summary>
        bool TryGet(System.Type sourceType, out NodeMetadata metadata);

        /// <summary>枚举所有已注册的 NodeMetadata。</summary>
        IReadOnlyList<NodeMetadata> All();
    }
}
