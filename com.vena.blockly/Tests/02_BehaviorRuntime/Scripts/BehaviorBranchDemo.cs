// -----------------------------------------------------------------------------
// Vena Blockly
// Visual scripting block graph runtime for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using UnityEngine;
using Vena.Blockly.Tests.BehaviorRuntime.Generated;

namespace Vena.Blockly.Tests.BehaviorRuntime
{
    /// <summary>
    /// BranchNode (if-else) demo：condition 是一个常量 LogicGraph&lt;bool&gt;，
    /// trueSource / falseSource 各挂一个 HelloBehaviorSource。
    ///
    /// Inspector conditionInput 决定走哪一支，Awake 重建 source 树时直接把
    /// DemoConstBoolSource.value 设为 conditionInput —— 不依赖 host 端 SetVariable。
    /// </summary>
    public sealed class BehaviorBranchDemo : MonoBehaviour
    {
        [Header("Inputs")]
        [SerializeField] bool conditionInput = true;
        [SerializeField] int tickCount = 5;
        [SerializeField] float fixedDeltaTime = 0.016f;

        [Header("Runtime State (read-only)")]
        [SerializeField] bool isPlaying;
        [SerializeField] int tickedFrames;

        DemoSampleHost _host;
        BehaviorGraph.Blockly _graph;

        void Awake()
        {
            _host = new DemoSampleHost();

            // 源树：BranchNode { condition = constBool(conditionInput),
            //                    trueSource = Hello("alive"),
            //                    falseSource = Hello("dying") }
            var rootSrc = new BehaviorGraph
            {
                root = new BranchNode
                {
                    condition = new LogicGraph
                    {
                        root = new DemoConstBoolSource { value = conditionInput },
                    },
                    trueSource = new HelloBehaviorSource
                    {
                        greeting = new LogicGraph { root = new DemoConstStringSource { value = "alive" } },
                    },
                    falseSource = new HelloBehaviorSource
                    {
                        greeting = new LogicGraph { root = new DemoConstStringSource { value = "dying" } },
                    },
                },
            };

            _graph = new BehaviorGraph.Blockly();
            _graph.Set(subject: null, host: _host);
            _graph.SetSource(rootSrc);
        }

        void Start()
        {
            _graph.Start(subject: null, host: _host);
            isPlaying = _graph.playing;
            Debug.Log($"[BranchDemo] graph started, conditionInput={conditionInput}");
        }

        void Update()
        {
            if (_graph != null && _graph.playing && tickedFrames < tickCount)
            {
                _graph.Update(fixedDeltaTime);
                tickedFrames++;
                isPlaying = _graph.playing;
                if (!_graph.playing) Debug.Log($"[BranchDemo] graph done after {tickedFrames} ticks");
            }
        }

        void OnDestroy()
        {
            _graph?.Destroy();
            _graph = null;
        }
    }
}
