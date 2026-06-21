// -----------------------------------------------------------------------------
// Vena Blockly
// Visual scripting block graph runtime for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Vena.Blockly.Tests.BehaviorRuntime
{
    /// <summary>
    /// 最小 Timeline 演示 (Part B)：把一个 Timeline 装进 BehaviorGraph，
    /// 跑一拍 →
    ///   1) Timeline 内置 Group + Track + Clip 体系展开 TestClip2Source.UClip,
    ///   2) clip 在 frame=1 进入 Begin →
    ///        UClip&lt;TestClip2Source, TestClip2&gt;.InitializeProperties 调用
    ///        timeline.blockly.CreateBlockly(source.seconds).Call&lt;float&gt;()
    ///        把 LogicGraph 求值结果写回 TestClip2.seconds,
    ///   3) Console 打印 seconds 值。
    ///
    /// 为什么用 TestClip2Source 而不是 TestClipSource：
    ///   TestClipSource 的 _time 是 private 字段（设计上由 codegen / inspector 注入），
    ///   外部 demo 代码无法直接赋值；TestClip2Source.seconds 是 public，可直接构造。
    ///
    /// 为什么用反射构造 TrackSource&lt;,&gt;：
    ///   TestClip2Source.UClip 是嵌套 + private 类型（同样是 codegen 路径产物）；
    ///   `TrackSource&lt;TClip, TInput&gt;` 的 TClip 必须是该私有类型，
    ///   只能用 UgcSourceAttribute.GetObjectType 走反射拿到 + MakeGenericType 实例化。
    /// </summary>
    public sealed class TimelineRuntimeDemo : MonoBehaviour
    {
        [Header("Inputs")]
        [SerializeField] float secondsValue = 0.5f;
        [SerializeField] int totalUpdateTicks = 10;
        [SerializeField] float fixedDeltaTime = 0.1f;

        [Header("Runtime State (read-only)")]
        [SerializeField] bool isPlaying;
        [SerializeField] int tickedFrames;

        DemoSampleHost _host;
        BehaviorGraph.Blockly _graph;

        void Awake()
        {
            _host = new DemoSampleHost();

            // ----- 构造 LogicGraph 常量源（为 TestClip2Source.seconds 喂 float） -----
            var secondsLogic = new LogicGraph
            {
                root = new DemoConstFloatSource { value = secondsValue },
            };

            // ----- 构造一个 TestClip2Source 实例，duration = 0.1s（≈3 帧 @ 30fps） -----
            var clipSource = new TestClip2Source
            {
                duration = 0.1f,
                seconds = secondsLogic,
            };

            // ----- 反射拿 UClip 嵌套类型 + 构造 TrackSource<UClip, TestClip2Source> -----
            Type uclipType = UgcSourceAttribute.GetObjectType(typeof(TestClip2Source));
            if (uclipType == null)
            {
                Debug.LogError("[TimelineDemo] UgcSource lookup failed for TestClip2Source.");
                return;
            }
            Type trackSourceType = typeof(TimelineSource.TrackSource<,>)
                .MakeGenericType(uclipType, typeof(TestClip2Source));
            var trackSource = Activator.CreateInstance(trackSourceType);

            // 反射调用 AddClip(int, TestClip2Source)
            MethodInfo addClipMi = trackSourceType.GetMethod(
                "AddClip", BindingFlags.Public | BindingFlags.Instance);
            addClipMi.Invoke(trackSource, new object[] { 1, clipSource });

            // ----- 把 trackSource 接到 GroupSource → TimelineSource -----
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
            Debug.Log($"[TimelineDemo] graph started, secondsValue input = {secondsValue}");
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
                    Debug.Log($"[TimelineDemo] graph done after {tickedFrames} ticks");
                }
            }
        }

        void OnDestroy()
        {
            _graph?.Destroy();
            _graph = null;
        }
    }

    // -------- Demo-local LogicGraph const float source --------

    /// <summary>
    /// 本 demo 程序集内的最小 IFunctionImpl&lt;float&gt; —— 只为了给 TestClip2Source.seconds
    /// 喂一个能 Call&lt;float&gt;() 的 LogicGraph。和 Demo 01 (LogicRuntime) 的 ConstFloatSource
    /// 等价，但跨 asmdef 不可见，所以这里就地复刻一份。
    /// </summary>
    public sealed class DemoConstFloatImpl : IFunctionImpl<float>
    {
        public float value;

        public float Evaluate() => value;
    }

    /// <summary>DemoConstFloatImpl 的 source + Node 装配。</summary>
    [UgcSource("Demo常量/Float", typeof(DemoConstFloatSource.Node))]
    public sealed class DemoConstFloatSource : Function<DemoConstFloatImpl, float>
    {
        public float value;

        sealed class Node : Block<DemoConstFloatSource>
        {
            protected override void Initialize() { }
            protected override void InitializeProperties(DemoConstFloatImpl impl) { impl.value = source.value; }
            protected override void CleanProperties(DemoConstFloatImpl impl) { }
        }
    }
}
