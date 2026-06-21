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
    /// ParallelNode + 多帧 Running 叶子 demo：
    ///   Parallel(Countdown("A", aTicksInput), Countdown("B", bTicksInput))
    ///
    /// 每次 Update 推一帧 fixedDeltaTime。CountdownBehaviorImpl.Tick 在 _remaining 归零之前
    /// 一直返回 Running，所以 Parallel 父节点也保持 Running，直到两个子节点都 Done。
    ///
    /// Inspector aTicksInput=3, bTicksInput=5 时期望：
    ///   - 帧 1..3：Console 同时见 [Countdown:A] Tick + [Countdown:B] Tick
    ///   - 帧 4..5：只剩 [Countdown:B] Tick
    ///   - 帧 5 末：B 归零 → Parallel Done → graph !playing
    /// </summary>
    public sealed class BehaviorParallelMultiTickDemo : MonoBehaviour
    {
        [Header("Inputs")]
        [SerializeField] int aTicksInput = 3;
        [SerializeField] int bTicksInput = 5;
        [SerializeField] int tickCount = 10;
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
                root = new ParallelNode
                {
                    sources = new BehaviorNodeSource[]
                    {
                        new CountdownBehaviorSource { ticksToRun = aTicksInput, label = "A" },
                        new CountdownBehaviorSource { ticksToRun = bTicksInput, label = "B" },
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
            Debug.Log($"[ParallelDemo] graph started, A={aTicksInput} ticks, B={bTicksInput} ticks");
        }

        void Update()
        {
            if (_graph != null && _graph.playing && tickedFrames < tickCount)
            {
                _graph.Update(fixedDeltaTime);
                tickedFrames++;
                isPlaying = _graph.playing;
                Debug.Log($"[ParallelDemo] tick {tickedFrames}, playing={_graph.playing}");
                if (!_graph.playing) Debug.Log($"[ParallelDemo] graph done after {tickedFrames} ticks");
            }
        }

        void OnDestroy()
        {
            _graph?.Destroy();
            _graph = null;
        }
    }
}
