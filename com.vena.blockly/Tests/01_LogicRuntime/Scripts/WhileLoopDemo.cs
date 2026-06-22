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
    /// Scene C — While 循环 + 共享变量。
    ///
    /// 等价 C#：
    ///     int sum = 0;
    ///     int i = 1;
    ///     int n = 10;
    ///     while (i &lt;= n) { sum = sum + i; i = i + 1; }
    ///     // sum == 55
    ///
    /// 表达式树（root = LogicSequence，仅一条语句：LogicWhile）：
    ///   Sequence
    ///   └── While
    ///       ├── condition : LessThanOrEqualInt ( GetVariableInt("i"), GetVariableInt("n") )
    ///       └── body      : Sequence
    ///                       ├── SetVariableInt ( "sum", AddInt ( GetVariableInt("sum"), GetVariableInt("i") ) )
    ///                       └── SetVariableInt ( "i",   AddInt ( GetVariableInt("i"),   Const 1 ) )
    ///
    /// 共享变量方案（host 预声明）：见 ControlFlowDemo 顶部 doc。
    /// sum / i / n 通过 root `LogicGraph.Blockly.SetVariable&lt;T&gt;` 预声明，
    /// While body 内 sibling statements 的 `LogicSetVariable&lt;T&gt;` 写穿透到 root。
    /// </summary>
    public sealed class WhileLoopDemo : MonoBehaviour
    {
        [Header("Runtime State (read-only)")]
        [SerializeField] int lastSum;

        DemoSampleHost _host;

        void Awake()
        {
            _host = new DemoSampleHost();
        }

        void Start()
        {
            // condition: i <= n
            var condition = new LogicGraph
            {
                root = new LessThanOrEqualIntSource
                {
                    a = new LogicGetVariableInt { variableName = "i" },
                    b = new LogicGetVariableInt { variableName = "n" },
                },
            };

            // body[0]: sum = sum + i
            var bodyAccumulate = new LogicGraph
            {
                root = new LogicSetVariableInt
                {
                    variableName = "sum",
                    value = new LogicGraph
                    {
                        root = new AddIntSource
                        {
                            a = new LogicGetVariableInt { variableName = "sum" },
                            b = new LogicGetVariableInt { variableName = "i" },
                        },
                    },
                },
            };

            // body[1]: i = i + 1
            var bodyIncrement = new LogicGraph
            {
                root = new LogicSetVariableInt
                {
                    variableName = "i",
                    value = new LogicGraph
                    {
                        root = new AddIntSource
                        {
                            a = new LogicGetVariableInt { variableName = "i" },
                            b = new ConstIntSource { value = 1 },
                        },
                    },
                },
            };

            var body = new LogicGraph
            {
                root = new LogicSequence
                {
                    statements = new[] { bodyAccumulate, bodyIncrement },
                },
            };

            var whileLoop = new LogicGraph
            {
                root = new LogicWhile
                {
                    condition = condition,
                    body = body,
                },
            };

            var src = new LogicGraph
            {
                root = new LogicSequence
                {
                    statements = new[] { whileLoop },
                },
            };

            var graph = new LogicGraph.Blockly();
            graph.Set(subject: null, host: _host);
            graph.SetSource(src);

            // host 预声明：把循环用到的 sum / i / n 注册到 root scope。
            graph.SetVariable<int>("sum", 0);
            graph.SetVariable<int>("i", 1);
            graph.SetVariable<int>("n", 10);

            graph.Invoke();

            lastSum = graph.GetVariable<int>("sum");
            graph.Destroy();

            Debug.Log($"[WhileLoopDemo] sum 1..10 = {lastSum} (expected 55)");
        }

        void OnDestroy()
        {
            _host = null;
        }
    }
}
