// -----------------------------------------------------------------------------
// Vena Blockly
// Visual scripting block graph runtime for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System;
using System.Diagnostics;

namespace Vena.Blockly
{

    /// <summary>
    /// 行为源数据基类
    /// </summary>
    public abstract class BehaviorNodeSource : IBlocklySource
    {
        // 包心 plain 路径自动分配进程内单调递增 InstanceId（构造时即赋值）。
        // setter 保留 public：GraphLoader.TrySetInstanceId 通过反射用 IR Guid 折叠值覆盖（公共 BindingFlags）。
        public ulong InstanceId { get; set; } = InstanceIdAllocator.Next();
    }

    /// <summary>
    /// 行为源数据基类（带实现类型）
    /// </summary>
    public abstract class BehaviorNodeSource<T> : BehaviorNodeSource where T : IBehavior, new()
    {
    }

    /// <summary>
    /// IBehaviorNode is an interface that represents a node in a graph.
    /// </summary>
    public interface IBehaviorNode : IBlock
    {
        BehaviorGraph.Blockly blockly { get; }

        void Init(BehaviorGraph.Blockly blockly, BehaviorNodeSource source);

        void Start();

        BehaviorResult Tick(float deltaTime);

        void LateTick(float deltaTime);

        void Finish();

        void OnDestroy();
    }

    /// <summary>
    /// IBehavior is an interface that defines the implementation of a behavior.
    /// </summary>
    public interface IBehavior
    {
        void Start(BehaviorGraph.Blockly blockly);

        BehaviorResult Tick(BehaviorGraph.Blockly blockly, float deltaTime);

        void LateTick(BehaviorGraph.Blockly blockly, float deltaTime);

        void Finish(BehaviorGraph.Blockly blockly);
    }

    internal interface ICompositeBehaviorNode : IBehaviorNode
    {

    }

    /// <summary>
    /// BehaviorNode is an abstract class that represents a node in a graph.
    /// </summary>
    public abstract class BehaviorNode<TSource, TImpl> : IBehaviorNode where TSource : BehaviorNodeSource<TImpl> where TImpl : IBehavior, new()
    {
        public BehaviorGraph.Blockly blockly { get; private set; }

        Blockly IBlock.scope => blockly;

        public float elapsedTime { private set; get; }

        public int frame { private set; get; }

        public bool isPlaying
        {
            get;
            private set;
        }

        public TSource source { get; private set; }

        private readonly TImpl _impl = new TImpl();

        public void Destroy()
        {
            if( null != blockly)
            {
                blockly.DestroyBlock(this);
            }
        }

        #region behavior

        void IBehaviorNode.Init(BehaviorGraph.Blockly blockly, BehaviorNodeSource source)
        {
            if (isPlaying)
            {
                throw new Exception($"{GetType().Name}.SetSource({source}) error, is already running !");
            }

            if (!(source is TSource tSource))
            {
                throw new ArgumentException(
                    $"{GetType().Name}.SetSource({source}) error, source must be of type {typeof(TSource).Name}!");
            }

            this.blockly = blockly;

            this.source = tSource;

            Initialize();
        }

        void IBehaviorNode.Start()
        {
            if (!isPlaying)
            {
                try
                {
                    Log("Start");
                    elapsedTime = 0f;
                    frame = 0;
                    isPlaying = true;
                    InitializeProperties(_impl);
                    _impl.Start(blockly);
                }
                catch (Exception e)
                {
                    blockly.Host?.Logger?.Error(
                        $"{GetType().Name}.Start() : error = {e.Message}\n{e.StackTrace}");
                }
            }
            else
            {
                throw new Exception($"{GetType().Name}.Start() error, is already running !");
            }
        }

        BehaviorResult IBehaviorNode.Tick(float deltaTime)
        {
            if (!isPlaying) return BehaviorResult.Done;

            elapsedTime += deltaTime;
            frame++;

            try
            {
                return _impl.Tick(blockly, deltaTime);
            }
            catch (Exception e)
            {
                // 异常路径：Logger.Error 旁路报告 + Done 收尾，不承载于 Tick 返回值。
                blockly.Host?.Logger?.Error(
                    $"{GetType().Name}.Tick() : error = {e.Message}\n{e.StackTrace}");
                return BehaviorResult.Done;
            }
        }

        void IBehaviorNode.LateTick(float deltaTime)
        {
            if (!isPlaying) return;

            try
            {
                _impl.LateTick(blockly, deltaTime);
            }
            catch (Exception e)
            {
                blockly.Host?.Logger?.Error(
                    $"{GetType().Name}.LateTick() : error = {e.Message}\n{e.StackTrace}");
                return;
            }
        }

        void IBehaviorNode.Finish()
        {
            if (isPlaying)
            {
                try
                {
                    Log("Finish");
                    _impl.Finish(blockly);
                }
                catch (Exception e)
                {
                    blockly.Host?.Logger?.Error(
                        $"{GetType().Name}.Finish() : error = {e.Message}\n{e.StackTrace}");
                }
                finally
                {
                    Reset();
                }
            }
        }

        private void Reset()
        {
            CleanProperties(_impl);
            elapsedTime = 0f;
            frame = 0;
            isPlaying = false;
            blockly = default;
        }

        void IBehaviorNode.OnDestroy()
        {
            OnBeforeDestroy();
        }

        #endregion

        protected abstract void Initialize();

        protected abstract void InitializeProperties(TImpl behaviorImpl);

        protected abstract void CleanProperties(TImpl behaviorImpl);

        protected abstract void OnBeforeDestroy();

        protected void Log(string func)
        {
        }
    }

    /// <summary>
    /// CompositeBehavior is an abstract class for control-flow behavior nodes.
    /// </summary>
    public abstract class CompositeBehavior<TSource> : ICompositeBehaviorNode where TSource : class, IBlocklySource
    {
        public BehaviorGraph.Blockly blockly { get; private set; }

        Blockly IBlock.scope => blockly;

        public void Destroy()
        {
            if( null != blockly)
            {
                blockly.DestroyBlock(this);
            }
        }

        public float elapsedTime { private set; get; }

        public int frame { private set; get; }

        public bool isPlaying
        {
            get;
            private set;
        }

        public TSource source { get; private set; }


        #region behavior

        void IBehaviorNode.Init(BehaviorGraph.Blockly blockly,BehaviorNodeSource source)
        {
            if (isPlaying)
            {
                throw new Exception($"{GetType().Name}.SetSource({source}) error, is already running !");
            }

            if (!(source is TSource tSource))
            {
                throw new ArgumentException(
                    $"{GetType().Name}.SetSource({source}) error, source must be of type {typeof(TSource).Name}!");
            }

            this.blockly = blockly;

            this.source = tSource;

            Initialize();
        }

        void IBehaviorNode.Start()
        {
            if (!isPlaying)
            {
                try
                {
                    Log("Start");
                    OnStart();
                    elapsedTime = 0f;
                    frame = 0;
                    isPlaying = true;
                }
                catch (Exception e)
                {
                    blockly.Host?.Logger?.Error(
                        $"{GetType().Name}.Start() : error = {e.Message}\n{e.StackTrace}");
                }
            }
            else
            {
                throw new Exception($"{GetType().Name}.Start() error, is already running !");
            }
        }

        BehaviorResult IBehaviorNode.Tick(float deltaTime)
        {
            if (!isPlaying) return BehaviorResult.Done;

            elapsedTime += deltaTime;
            frame++;

            try
            {
                return OnTick(deltaTime);
            }
            catch (Exception e)
            {
                // 异常路径：Logger.Error 旁路报告 + Done 收尾，不承载于 Tick 返回值。
                blockly.Host?.Logger?.Error(
                    $"{GetType().Name}.Tick() : error = {e.Message}\n{e.StackTrace}");
                return BehaviorResult.Done;
            }
        }

        void IBehaviorNode.LateTick(float deltaTime)
        {
            if (!isPlaying) return;

            try
            {
                OnLateTick(deltaTime);
            }
            catch (Exception e)
            {
                blockly.Host?.Logger?.Error(
                    $"{GetType().Name}.LateTick() : error = {e.Message}\n{e.StackTrace}");
                return;
            }
        }

        void IBehaviorNode.Finish()
        {
            if (isPlaying)
            {
                try
                {
                    Log("Finish");
                    OnFinish();
                }
                catch (Exception e)
                {
                    blockly.Host?.Logger?.Error(
                        $"{GetType().Name}.Finish() : error = {e.Message}\n{e.StackTrace}");
                }
                finally
                {
                    Reset();
                }
            }
        }

        private void Reset()
        {
            OnResetData();
            elapsedTime = 0f;
            frame = 0;
            isPlaying = false;
            blockly = default;
        }

        void IBehaviorNode.OnDestroy()
        {
            OnDestroy();
        }

        #endregion

        #region abstract funcs

        protected abstract void Initialize();

        protected abstract void OnStart();

        protected abstract BehaviorResult OnTick(float deltaTime);

        protected abstract void OnLateTick(float deltaTime);

        protected abstract void OnFinish();

        protected abstract void OnResetData();

        protected abstract void OnDestroy();

        #endregion

        [Conditional(BlocklyDebug.DEBUG_CONDITION)]
        protected void Log(string log)
        {
        }

        [Conditional(BlocklyDebug.DEBUG_CONDITION)]
        protected void LogError(string log)
        {
            blockly.Host?.Logger?.Warning($"### {GetType().Name}.{log} !");
        }
    }
}
