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
    /// 调试通道 —— 编辑器可视化目标（节点高亮 / 端口数值 tooltip）。
    /// 物理落 Runtime 以保 Editor → Runtime 单向依赖。
    ///
    /// 范围：仅三事件、单线程、同步回调。
    /// 不做：时间轴 / 历史回放 / 远程调试。
    ///
    /// 与 IBlocklyLogger 分离：
    ///   - IBlocklyLogger = 文本流目标（Info / Warn / Error），变更原因 = 日志聚合 / 转发策略。
    ///   - IBlocklyDebugChannel = 编辑器可视化目标，变更原因 = 编辑器调试 UX。
    /// 两者不复用、不继承、不合并。
    ///
    /// **不进 IBlocklyHost 聚合门面**：调试通道由 BlocklyEditorWindow
    /// 在编辑器期通过 host 之外的注入路径（静态注入点 <see cref="BlocklyDebugChannelRegistry"/>）挂入 Runtime 节点执行链。
    /// </summary>
    public interface IBlocklyDebugChannel
    {
        /// <summary>节点开始执行（Behavior 节点 Tick 入口、Logic 节点 Evaluate 入口）。</summary>
        void OnNodeEnter(Guid nodeGuid);

        /// <summary>节点结束执行；附 Behavior 节点 Tick 结果。Logic 节点退出时填 BehaviorResult.Done。</summary>
        void OnNodeExit(Guid nodeGuid, BehaviorResult result);

        /// <summary>表达式节点产值 —— 用于 Inspector 端口数值预览。</summary>
        void OnValueProduced(Guid nodeGuid, IBoxedValue value);
    }

    /// <summary>
    /// 静态注入点 —— Editor 在窗口打开时设入；Runtime 节点执行链按需可读。
    /// 单线程编辑器场景下足以；非编辑器期保持 null（由 [Conditional] 或调用方自行守护）。
    /// 不进 IBlocklyHost 聚合门面。
    /// </summary>
    public static class BlocklyDebugChannelRegistry
    {
        public static IBlocklyDebugChannel Current { get; set; }
    }
}
