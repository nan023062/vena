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
    /// Scene A — 嵌套数学表达式。
    /// 验证多层 Function 嵌套 + 5 步协议 + 5 参 Sum5IntsSource。
    ///
    /// 等价 C#：((3 + 4) * (10 - 6)) + Sum5(1, 2, 3, 4, 5)
    ///         = (7 * 4) + 15
    ///         = 43
    ///
    /// 表达式树（root = AddIntSource）：
    ///   Add
    ///   ├── Multiply
    ///   │   ├── Add ( Const 3, Const 4 )       → 7
    ///   │   └── Subtract ( Const 10, Const 6 )  → 4
    ///   └── Sum5 ( 1, 2, 3, 4, 5 )              → 15
    /// </summary>
    public sealed class NestedExpressionDemo : MonoBehaviour
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
            var src = new LogicGraph
            {
                root = new AddIntSource
                {
                    // (3 + 4) * (10 - 6)
                    a = new MultiplyIntSource
                    {
                        a = new AddIntSource
                        {
                            a = new ConstIntSource { value = 3 },
                            b = new ConstIntSource { value = 4 },
                        },
                        b = new SubtractIntSource
                        {
                            a = new ConstIntSource { value = 10 },
                            b = new ConstIntSource { value = 6 },
                        },
                    },
                    // Sum5(1, 2, 3, 4, 5)
                    b = new Sum5IntsSource
                    {
                        a = new ConstIntSource { value = 1 },
                        b = new ConstIntSource { value = 2 },
                        c = new ConstIntSource { value = 3 },
                        d = new ConstIntSource { value = 4 },
                        e = new ConstIntSource { value = 5 },
                    },
                },
            };

            var graph = new LogicGraph.Blockly();
            graph.Set(subject: null, host: _host);
            graph.SetSource(src);
            lastResult = graph.Call<int>();
            graph.Destroy();

            Debug.Log($"[NestedExpressionDemo] ((3+4)*(10-6)) + Sum5(1..5) = {lastResult} (expected 43)");
        }

        void OnDestroy()
        {
            _host = null;
        }
    }
}
