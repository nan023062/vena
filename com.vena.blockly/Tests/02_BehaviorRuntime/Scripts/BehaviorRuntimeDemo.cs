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
    /// 最小 BehaviorGraph 演示：Sequence ( Hello("Hello"), Hello("World") )，
    /// Inspector 上调旋钮：tick 步数 / 固定 deltaTime；Console 看 Start / Tick / Finish 日志。
    /// </summary>
    public sealed class BehaviorRuntimeDemo : MonoBehaviour
    {
        [Header("Demo Config")]
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

            // 源树：Sequence ( Hello("Hello"), Hello("World") )
            var rootSrc = new BehaviorGraph
            {
                root = new SequenceNode
                {
                    sources = new BehaviorNodeSource[]
                    {
                        new HelloBehaviorSource { greeting = "Hello" },
                        new HelloBehaviorSource { greeting = "World" },
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
            Debug.Log("[BehaviorDemo] graph started");
        }

        void Update()
        {
            if (_graph != null && _graph.playing && tickedFrames < tickCount)
            {
                _graph.Update(fixedDeltaTime);
                tickedFrames++;
                isPlaying = _graph.playing;
                Debug.Log($"[BehaviorDemo] tick {tickedFrames}, playing={_graph.playing}");
                if (!_graph.playing) Debug.Log("[BehaviorDemo] graph done");
            }
        }

        void OnDestroy()
        {
            _graph?.Destroy();
            _graph = null;
        }
    }
}
