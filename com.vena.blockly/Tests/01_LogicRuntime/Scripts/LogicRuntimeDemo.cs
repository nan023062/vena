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
    /// 最小 LogicGraph 演示：每帧重建源 → Call&lt;int&gt;(AddInt(ConstInt(a), ConstInt(b)))，
    /// 同时 Call&lt;float&gt;(ConstFloat(floatOperand))，把结果回写到 Inspector。
    /// 改 Inspector 上的 operandA / operandB / floatOperand，下一帧就能在
    /// lastIntSum / lastFloatValue 上看到反映。
    /// </summary>
    public sealed class LogicRuntimeDemo : MonoBehaviour
    {
        [Header("Inputs")]
        [SerializeField] int operandA = 3;
        [SerializeField] int operandB = 4;
        [SerializeField] float floatOperand = 1.5f;

        [Header("Runtime State (read-only)")]
        [SerializeField] int lastIntSum;
        [SerializeField] float lastFloatValue;
        [SerializeField] int evaluationCount;

        DemoSampleHost _host;

        void Awake()
        {
            _host = new DemoSampleHost();
        }

        void Start()
        {
            // 首帧立刻求一次，避免 Inspector 在 Update 跑前显示初值 0。
            EvaluateOnce();
        }

        void Update()
        {
            EvaluateOnce();
        }

        void OnDestroy()
        {
            _host = null;
        }

        void EvaluateOnce()
        {
            // ----- int 路径：AddInt ( ConstInt(operandA), ConstInt(operandB) ) -----
            var intSrc = new LogicGraph
            {
                root = new AddIntSource
                {
                    a = new ConstIntSource { value = operandA },
                    b = new ConstIntSource { value = operandB },
                },
            };
            var intGraph = new LogicGraph.Blockly();
            intGraph.Set(subject: null, host: _host);
            intGraph.SetSource(intSrc);
            lastIntSum = intGraph.Call<int>();
            intGraph.Destroy();

            // ----- float 路径：ConstFloat(floatOperand) -----
            var floatSrc = new LogicGraph
            {
                root = new ConstFloatSource { value = floatOperand },
            };
            var floatGraph = new LogicGraph.Blockly();
            floatGraph.Set(subject: null, host: _host);
            floatGraph.SetSource(floatSrc);
            lastFloatValue = floatGraph.Call<float>();
            floatGraph.Destroy();

            evaluationCount++;
        }
    }
}
