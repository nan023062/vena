// -----------------------------------------------------------------------------
// Vena Blockly
// Visual scripting block graph runtime for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System;
using System.Reflection;
using UnityEngine;

namespace Vena.Blockly.Tests.BehaviorRuntime
{
    /// <summary>
    /// Timeline + ExpressionClip + Signal 全套 demo：
    ///   - 一个 ExpressionClip：onBegin / onFrame / onEnd 各挂一条 LogSignalSource
    ///   - 一个 Signal：在 frame=5 触发，挂另一条 LogSignalSource
    ///
    /// duration = 0.5s @ 30fps → clipFrameCount = 15。Signal frame=5 落在 clip 跨度内。
    /// 期望 Console 顺序（粗略）：
    ///   [Signal] clip begin
    ///   [Signal] clip frame   ×4    (frame 1..4 — frame 1 同时跑 Begin + OnFrame)
    ///   [Signal] signal at frame 5
    ///   [Signal] clip frame   ×N    (frame 5..15 的 OnFrame)
    ///   [Signal] clip end
    ///
    /// 注：ExpressionClip.Object 是 private nested 类型，TrackSource&lt;TClip,TSource&gt; 的
    /// 第一个泛型参数必须是它，只能反射构造（同 TimelineRuntimeDemo 套路）。
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

            // ----- 构造 ExpressionClip：onBegin/onFrame/onEnd 各挂 LogSignalSource -----
            var clipSource = new ExpressionClip
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

            // ----- 反射拿 ExpressionClip.Object 嵌套类型 + 构造 TrackSource<Object, ExpressionClip> -----
            Type clipObjectType = BlocklySourceAttribute.GetNodeType(typeof(ExpressionClip));
            if (clipObjectType == null)
            {
                Debug.LogError("[TimelineSignalDemo] BlocklySource lookup failed for ExpressionClip.");
                return;
            }
            Type trackSourceType = typeof(TimelineSource.TrackSource<,>)
                .MakeGenericType(clipObjectType, typeof(ExpressionClip));
            var trackSource = Activator.CreateInstance(trackSourceType);

            // 反射调用 AddClip(int, ExpressionClip)
            MethodInfo addClipMi = trackSourceType.GetMethod(
                "AddClip", BindingFlags.Public | BindingFlags.Instance);
            addClipMi.Invoke(trackSource, new object[] { 1, clipSource });

            // 反射调用 Add(int, Signal)
            var signalSource = new Signal
            {
                source = new LogicGraph
                {
                    root = new LogSignalSource { message = $"signal at frame {signalFrame}" },
                },
            };
            MethodInfo addSignalMi = trackSourceType.GetMethod(
                "Add", new[] { typeof(int), typeof(Signal) });
            addSignalMi.Invoke(trackSource, new object[] { signalFrame, signalSource });

            // ----- 拼装 GroupSource → TimelineSource -----
            var timelineSrc = new TimelineSource { frameRate = 30 };
            timelineSrc.group.TrackList.Add((TimelineSource.ISource)trackSource);

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
