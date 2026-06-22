// -----------------------------------------------------------------------------
// Vena Blockly
// Visual scripting block graph runtime for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using Vena.Blockly;
// ReSharper disable InconsistentNaming
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value

namespace Vena.Blockly.Tests.BehaviorRuntime
{

    public class TestClip : IUClip
    {
        public float time;

        public void Begin(Timeline timeline)
        {

        }

        public void OnFrame(Timeline timeline, in UFrameInfo frameInfo)
        {

        }

        public void End(Timeline timeline, in UFrameInfo frameInfo)
        {

        }
    }

    [BlocklySource( "时间线/测试剪辑源1", typeof( TestClipSource.UClip ) )]
    public class TestClipSource : UClipSource<TestClip>
    {
        [BlocklySourceSlot("时间", 1)]
        private LogicGraph _time;

        class UClip : UClip<TestClipSource, TestClip>
        {
            private LogicGraph.Blockly time;

            protected override void Initialize()
            {
                time = timeline.blockly.CreateBlockly(source._time);
            }

            protected override void InitializeProperties(TestClip behaviorImpl)
            {
                behaviorImpl.time = time.Call<float>();
            }

            protected override void CleanProperties(TestClip behaviorImpl)
            {
            }

            protected override void OnBeforeDestroy()
            {
                timeline.blockly.DestroyBlockly(time);
                time = null;
            }
        }
    }


    public class TestClip2 : IUClip
    {
        public float seconds;

        public void Begin(Timeline timeline)
        {

        }

        public void OnFrame(Timeline timeline, in UFrameInfo frameInfo)
        {

        }

        public void End(Timeline timeline, in UFrameInfo frameInfo)
        {

        }
    }

    [BlocklySource( "时间线/测试剪辑源2", typeof( TestClip2Source.UClip ) )]
    public class TestClip2Source : UClipSource<TestClip2>
    {
        [ExpressionSignature(typeof(float))]
        [BlocklySourceSlot("秒数", 1)]
        public LogicGraph seconds;

        class UClip : UClip<TestClip2Source, TestClip2>
        {
            private LogicGraph.Blockly _seconds;

            protected override void Initialize()
            {
                _seconds = timeline.blockly.CreateBlockly(source.seconds);
            }

            protected override void InitializeProperties(TestClip2 behaviorImpl)
            {
                behaviorImpl.seconds = _seconds.Call<float>();
            }

            protected override void CleanProperties(TestClip2 behaviorImpl)
            {
                behaviorImpl.seconds = 0;
            }

            protected override void OnBeforeDestroy()
            {
                timeline.blockly.DestroyBlockly(_seconds);
                _seconds = null;
            }
        }
    }
}
