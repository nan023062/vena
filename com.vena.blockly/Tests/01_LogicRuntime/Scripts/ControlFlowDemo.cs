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
    /// 共享变量方案（host 预声明）：
    ///   `LogicSequence.Node` 为每个 statement 创建独立 child `LogicGraph.Blockly`，
    ///   sibling statements 之间互不可见。这里通过 root `LogicGraph.Blockly.SetVariable&lt;T&gt;`
    ///   将共享变量注册到 root scope，sibling 的 `LogicSetVariable&lt;T&gt;` 经
    ///   `ScopeChain.ResolveWriteTarget` 沿父链命中 root，写穿透生效，后续读取同理。
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

            // host 预声明：把 sibling 共享变量注册到 root scope；占位值被 sibling 写穿透更新。
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
