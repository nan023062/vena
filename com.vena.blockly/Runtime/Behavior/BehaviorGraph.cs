using System;
using System.Runtime.CompilerServices;

namespace Vena.Blockly
{

    /// <summary>
    /// Behavior 图源数据
    /// </summary>
    [UgcSource("逻辑图", typeof(BehaviorGraph.Blockly))]
    public sealed class BehaviorGraph : IBlocklySource
    {
        public ulong Guid { get; set; } = 0;

        [UgcSourceProperty("根节点", 1)]
        public BehaviorNodeSource root;

        /// <summary>
        /// Behavior 作用域。装载 Behavior 节点。
        /// Expression 节点通过基类 InstanceManagement 的 CreateExprBlock 创建。
        /// 所有实例通过基类统一的 _instances + _instanceMap 管理。
        /// </summary>
        public sealed class Blockly : Vena.Blockly.Blockly
        {
            private IBehaviorNode _start;

            private float _elapsedTime;

            private int _frameCount;

            private bool _playing;

            public bool playing
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _playing;
            }

            #region Source

            public void SetSource(BehaviorGraph source)
            {
                if (_playing)
                {
                    throw new InvalidOperationException("Cannot set source while playing.");
                }

                if (source != null && source.root != null)
                {
                    _start = CreateBlock(source.root);
                }
                else
                {
                    Host?.Logger.Warning("[BehaviorGraph] 无效的行为图。");
                }
            }

            #endregion

            #region Scheduling

            public void Start(object subject, IBlocklyHost host)
            {
                if (!_playing)
                {
                    Set(subject, host);

                    _playing = true;
                    _frameCount = 0;
                    _elapsedTime = 0;

                    _start?.Start();
                }
            }

            public void Restart()
            {
                _start?.Start();
            }

            public bool Update(float deltaTime)
            {
                if (!_playing || null == _start)
                {
                    return true;
                }

                using var capture = new ExceptionCapture(this);

                _elapsedTime += deltaTime;
                _frameCount++;

                if (_start.Tick(deltaTime) != BehaviorResult.Running)
                {
                    StopUnsafe();
                }

                return !_playing;
            }

            public void LateUpdate(float deltaTime)
            {
                if (!_playing)
                {
                    return;
                }

                using var capture = new ExceptionCapture(this);

                _start?.LateTick(deltaTime);
            }

            public void Finish()
            {
                if (_playing)
                {
                    using var capture = new ExceptionCapture(this);

                    StopUnsafe();
                }
            }

            private void StopUnsafe()
            {
                _playing = false;
                _frameCount = 0;
                _elapsedTime = 0;

                _start?.Finish();

                ClearVariables();
                ClearHostAndSubject();
            }

            #endregion

            #region Lifecycle

            public override void Destroy()
            {
                using var capture = new ExceptionCapture(this);

                if (_playing)
                {
                    StopUnsafe();
                }


                DestroyBlock(_start);
                _start = null;

                base.Destroy();
            }

            #endregion

            #region Behavior Block Create / Destroy

            public IBehaviorNode CreateBlock(BehaviorNodeSource source)
            {
                var behavior = Host.NodeFactory.Create<IBehaviorNode>(source);
                if (behavior != null)
                {
                    behavior.Init(this, source);
                    RegisterInstanceInternal(source.Guid, behavior);
                    return behavior;
                }
                return null;
            }

            #endregion
        }
    }
}
