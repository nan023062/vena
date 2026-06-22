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
    /// 最小叶子 IBehaviorImpl：Start / Tick / Finish 各打一行带 greeting 的日志，
    /// Tick 当帧返回 Done（一帧式叶子），LateTick 留空。
    /// `*Source` + `Node` 由 Path B codegen 产出于 Generated/HelloBehaviorImpl.g.cs（Scenario Y：
    /// `greeting` 在 Source 端为 LogicGraph 槽，Init 时 Call&lt;string&gt;() 求值后赋回本 Impl 字段）。
    /// </summary>
    public sealed class HelloBehaviorImpl : IBehaviorImpl
    {
        [BlocklySourceSlot("问候语", 1)]
        public string greeting;

        public void Start(BehaviorGraph.Blockly graph)
        {
            Debug.Log($"[HelloBehavior] Start: {greeting}");
        }

        public BehaviorResult Tick(BehaviorGraph.Blockly graph, float deltaTime)
        {
            Debug.Log($"[HelloBehavior] Tick: {greeting}");
            return BehaviorResult.Done;
        }

        public void LateTick(BehaviorGraph.Blockly graph, float deltaTime)
        {
        }

        public void Finish(BehaviorGraph.Blockly graph)
        {
            Debug.Log($"[HelloBehavior] Finish: {greeting}");
        }
    }
}
