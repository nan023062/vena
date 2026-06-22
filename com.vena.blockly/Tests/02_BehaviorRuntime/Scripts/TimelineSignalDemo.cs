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
    /// Timeline + LogicClip + Signal 全套 demo：
    ///   - 一个 LogicClip：onBegin / onFrame / onEnd 各挂一条 LogSignalSource
    ///   - 一个 Signal：在 frame=5 触发，挂另一条 LogSignalSource
    ///
    /// duration = 0.5s @ 30fps → clipFrameCount = 15。Signal frame=5 落在 clip 跨度内。
    /// 期望 Console 顺序（粗略）：
    ///   [Signal] clip begin
    ///   [Signal] clip frame   ×4    (frame 1..4 — frame 1 同时跑 Begin + OnFrame)
    ///   [Signal] signal at frame 5
    ///   [Signal] clip frame   ×N    (frame 5..15 的 OnFrame)
    ///   [Signal] clip end
    /// </summary>
    public sealed class TimelineSignalDemo : MonoBehaviour
    {
        [Header("Inputs")]
        [SerializeField] float clipDuration = 0.5f;
        [SerializeField] int signalFrame = 5;
        [SerializeField] int totalUpdateTicks = 30;
        [SerializeField] float fixedDeltaTime = 1f / 30f;

        [Header("Runtime State (read-only)")]
        [SerializeField] bool isPlaying;
        [SerializeField] int tickedFrames;

        DemoSampleHost _host;
        BehaviorGraph.Blockly _graph;

        void Awake()
        {
            _host = new DemoSampleHost();

            // ----- 构造 LogicClip：onBegin/onFrame/onEnd 各挂 LogSignalSource -----
            var clipSource = new LogicClip
            {
                duration = clipDuration,
                onBegin = new LogicGraph
                {
                    root = new LogSignalSource { message = "clip begin" },
                },
                onFrame = new LogicGraph
                {
                    root = new LogSignalSource { message = "clip frame" },
                },
                onEnd = new LogicGraph
                {
                    root = new LogSignalSource { message = "clip end" },
                },
            };

            // ----- 直接构造 TrackSource<LogicClip, LogicClip>（LogicClip 既是 source 也是 impl）-----
            var trackSource = new TimelineSource.TrackSource<LogicClip, LogicClip>();
            trackSource.AddClip(1, clipSource);

            var signalSource = new Signal
            {
                source = new LogicGraph
                {
                    root = new LogSignalSource { message = $"signal at frame {signalFrame}" },
                },
            };
            trackSource.Add(signalFrame, signalSource);

            // ----- 拼装 GroupSource → TimelineSource -----
            var timelineSrc = new TimelineSource { frameRate = 30 };
            timelineSrc.group.TrackList.Add(trackSource);

            // ----- BehaviorGraph 包装 + 起飞 -----
            var rootSrc = new BehaviorGraph { root = timelineSrc };

            _graph = new BehaviorGraph.Blockly();
            _graph.Set(subject: null, host: _host);
            _graph.SetSource(rootSrc);
        }

        void Start()
        {
            _graph.Start(subject: null, host: _host);
            isPlaying = _graph.playing;
            Debug.Log($"[TimelineSignalDemo] graph started, clipDuration={clipDuration}s, signalFrame={signalFrame}");
        }

        void Update()
        {
            if (_graph != null && _graph.playing && tickedFrames < totalUpdateTicks)
            {
                _graph.Update(fixedDeltaTime);
                tickedFrames++;
                isPlaying = _graph.playing;
                if (!_graph.playing)
                {
                    Debug.Log($"[TimelineSignalDemo] graph done after {tickedFrames} ticks");
                }
            }
        }

        void OnDestroy()
        {
            _graph?.Destroy();
            _graph = null;
        }
    }
}
