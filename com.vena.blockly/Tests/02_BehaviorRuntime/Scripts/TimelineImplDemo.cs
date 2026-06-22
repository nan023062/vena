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
    /// IClip C# Impl 业务扩展 demo：HelloClipImpl 通过 codegen 出的 HelloClipSource 接入 Timeline。
    /// `greeting` 在 Source 端为 LogicGraph 槽，Clip Begin 时 Call&lt;string&gt;() 求值后赋回 Impl 字段。
    /// 期望 Console 顺序：
    ///   [HelloClip] Begin: TimelineHello
    ///   [HelloClip] End: TimelineHello
    /// </summary>
    public sealed class TimelineImplDemo : MonoBehaviour
    {
        [Header("Inputs")]
        [SerializeField] float clipDuration = 0.5f;
        [SerializeField] string greetingValue = "TimelineHello";
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

            // ----- 构造 HelloClipSource：greeting 槽挂 DemoConstStringSource 输出 -----
            var clipSource = new HelloClipSource
            {
                duration = clipDuration,
                greeting = new LogicGraph
                {
                    root = new DemoConstStringSource { value = greetingValue },
                },
            };

            // ----- 构造 TrackSource：TClip 用公开基类 Clip<HelloClipSource>（产物 Node 为 private nested，
            //       Track 内部走 Activator.CreateInstance + [BlocklySource] attribute 反射拿真实 Node 类型；
            //       这里 TClip 泛型实参只需满足 ITimelineClip 约束）-----
            var trackSource = new TimelineSource.TrackSource<Clip<HelloClipSource>, HelloClipSource>();
            trackSource.AddClip(1, clipSource);

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
            Debug.Log($"[TimelineImplDemo] graph started, clipDuration={clipDuration}s, greeting={greetingValue}");
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
                    Debug.Log($"[TimelineImplDemo] graph done after {tickedFrames} ticks");
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
