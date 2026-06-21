// -----------------------------------------------------------------------------
// Vena Blockly
// Visual scripting block graph runtime for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

namespace Vena.Blockly
{

    /// <summary>
    /// 行为节点 Tick 双态返回值。
    ///
    /// 双态语义：仅表达「未完成 / 已完成」两种正常推进状态，**不**承载成败。
    /// 异常路径由基类 try/catch 经 IBlocklyLogger.Error 旁路报告，并以 Done 收尾本节点本次活动周期。
    /// 节点间分支选择由 LogicGraph 条件表达式承担；叶子内部「再 tick 一帧 / 收尾」属时长门控，与分支语义正交。
    /// </summary>
    public enum BehaviorResult
    {
        /// <summary>未完成、下一帧再 tick。</summary>
        Running,

        /// <summary>已完成、本节点本次活动周期结束。</summary>
        Done,
    }
}
