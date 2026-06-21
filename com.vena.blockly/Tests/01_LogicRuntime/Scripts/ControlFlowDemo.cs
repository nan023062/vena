// -----------------------------------------------------------------------------
// Vena Blockly
// Visual scripting block graph runtime for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using UnityEngine;

namespace Vena.Blockly.Tests.LogicRuntime
{
    /// <summary>
    /// Scene B — 条件分支 + 共享变量。
    ///
    /// 等价 C#：
    ///     int x = 10;
    ///     int result;
    ///     if (x &gt; 5) result = x * 2;
    ///     else        result = x;
    ///     // result == 20
    ///
    /// 表达式树（root = LogicSequence）：
    ///   Sequence
    ///   ├── SetVariableInt("x", Const 10)
    ///   └── Branch
    ///       ├── condition  : GreaterThanInt ( GetVariableInt("x"), Const 5 )
    ///       ├── trueBranch : SetVariableInt ( "result", MultiplyInt ( GetVariableInt("x"), Const 2 ) )
    ///       └── falseBranch: SetVariableInt ( "result", GetVariableInt("x") )
    ///
    /// 共享变量方案（option A — host 预声明）：
    ///   `LogicSequence.Node` 为每个 statement 创建一个独立 child `LogicGraph.Blockly`，
    ///   sibling statement 之间没有可见性 —— 步骤 0 写到自己 child 的 storage 后，
    ///   步骤 1 走 parent chain 找不到那个 sibling。
    ///   这里通过 root `LogicGraph.Blockly` 上预先调用 `SetVariable&lt;T&gt;` 把 "x" / "result"
    ///   注册到 root scope，sibling statements 内的 `LogicSetVariable&lt;T&gt;` 经
    ///   `ScopeChain.ResolveWriteTarget` 沿父链命中 root → 写穿透到 root；后续 sibling
    ///   读取也沿父链命中 root。
    ///   这是一个临时绕路 —— UGC 玩家造图无法触发；正解是 `LogicSequence` 共享 host
    ///   scope 而非创建 per-statement child Blockly（task #19 backlog）。
    /// </summary>
    public sealed class ControlFlowDemo : MonoBehaviour
    {
        [Header("Runtime State (read-only)")]
        [SerializeField] int lastResult;

        DemoSampleHost _host;

        void Awake()
        {
            _host = new DemoSampleHost();
        }

        void Start()
        {
            // x = 10
            var setX = new LogicGraph
            {
                root = new LogicSetVariableInt
                {
                    variableName = "x",
                    value = new LogicGraph { root = new ConstIntSource { value = 10 } },
                },
            };

            // x > 5
            var condition = new LogicGraph
            {
                root = new GreaterThanIntSource
                {
                    a = new LogicGetVariableInt { variableName = "x" },
                    b = new ConstIntSource { value = 5 },
                },
            };

            // result = x * 2
            var trueBranch = new LogicGraph
            {
                root = new LogicSetVariableInt
                {
                    variableName = "result",
                    value = new LogicGraph
                    {
                        root = new MultiplyIntSource
                        {
                            a = new LogicGetVariableInt { variableName = "x" },
                            b = new ConstIntSource { value = 2 },
                        },
                    },
                },
            };

            // result = x
            var falseBranch = new LogicGraph
            {
                root = new LogicSetVariableInt
                {
                    variableName = "result",
                    value = new LogicGraph { root = new LogicGetVariableInt { variableName = "x" } },
                },
            };

            var branch = new LogicGraph
            {
                root = new LogicBranch
                {
                    condition = condition,
                    trueBranch = trueBranch,
                    falseBranch = falseBranch,
                },
            };

            var src = new LogicGraph
            {
                root = new LogicSequence
                {
                    statements = new[] { setX, branch },
                },
            };

            var graph = new LogicGraph.Blockly();
            graph.Set(subject: null, host: _host);
            graph.SetSource(src);

            // option A — host 预声明：把 sibling 共享变量注册到 root scope。
            // 占位值会被 sibling statement 的写穿透更新。
            graph.SetVariable<int>("x", 0);
            graph.SetVariable<int>("result", 0);

            graph.Invoke();

            lastResult = graph.GetVariable<int>("result");
            graph.Destroy();

            Debug.Log($"[ControlFlowDemo] if (x>5) result = x*2 else result = x;  result = {lastResult} (expected 20)");
        }

        void OnDestroy()
        {
            _host = null;
        }
    }
}
