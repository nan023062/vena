// -----------------------------------------------------------------------------
// Vena Blockly
// Visual scripting block graph runtime for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using UnityEngine;

namespace Vena.Blockly.Tests.BehaviorRuntime
{
    /// <summary>
    /// 最小叶子 IClip：Begin / End 各打一行带 greeting 的日志，OnFrame 留空。
    /// `*Source` + `Node` 由 codegen 产出于 Generated/HelloClipImpl.g.cs：
    /// `greeting` 在 Source 端为 LogicGraph 槽，Begin 时 Call&lt;string&gt;() 求值后赋回 Impl 字段，
    /// End 后 CleanProperties 置 null。
    /// </summary>
    public sealed class HelloClipImpl : IClip
    {
        [BlocklySourceSlot("问候语", 1)]
        public string greeting;

        public void Begin()
        {
            Debug.Log($"[HelloClip] Begin: {greeting}");
        }

        public void OnFrame(in FrameInfo frameInfo)
        {
        }

        public void End(in FrameInfo frameInfo)
        {
            Debug.Log($"[HelloClip] End: {greeting}");
        }
    }
}
