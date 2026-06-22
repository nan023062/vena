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
    /// LoopNode demo：loopCount 是常量 LogicGraph&lt;int&gt;，
    /// behavior 是 Sequence(Hello("iter-A"), Hello("iter-B"))。
    ///
    /// Inspector loopCountInput = 3 → 期望 6 次 Hello tick（A,B 各 3 次）。
    /// </summary>
    public sealed class BehaviorLoopDemo : MonoBehaviour
    {
        [Header("Inputs")]
        [SerializeField] int loopCountInput = 3;
        [SerializeField] int tickCount = 20;
        [SerializeField] float fixedDeltaTime = 0.016f;

        [Header("Runtime State (read-only)")]
        [SerializeField] bool isPlaying;
        [SerializeField] int tickedFrames;

        DemoSampleHost _host;
        BehaviorGraph.Blockly _graph;

        void Awake()
        {
            _host = new DemoSampleHost();

            var rootSrc = new BehaviorGraph
            {
                root = new LoopNode
                {
                    loopCount = new LogicGraph
                    {
                        root = new DemoConstIntSource { value = loopCountInput },
                    },
                    behavior = new SequenceNode
                    {
                        sources = new BehaviorNodeSource[]
                        {
                            new HelloBehaviorSource
                            {
                                greeting = new LogicGraph { root = new DemoConstStringSource { value = "iter-A" } },
                            },
                            new HelloBehaviorSource
                            {
                                greeting = new LogicGraph { root = new DemoConstStringSource { value = "iter-B" } },
                            },
                        },
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
            Debug.Log($"[LoopDemo] graph started, loopCountInput={loopCountInput}");
        }

        void Update()
        {
            if (_graph != null && _graph.playing && tickedFrames < tickCount)
            {
                _graph.Update(fixedDeltaTime);
                tickedFrames++;
                isPlaying = _graph.playing;
                if (!_graph.playing) Debug.Log($"[LoopDemo] graph done after {tickedFrames} ticks");
            }
        }

        void OnDestroy()
        {
            _graph?.Destroy();
            _graph = null;
        }
    }
}
