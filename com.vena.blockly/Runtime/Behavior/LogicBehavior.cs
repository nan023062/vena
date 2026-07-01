// -----------------------------------------------------------------------------
// Vena Blockly
// Visual scripting block graph runtime for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

namespace Vena.Blockly
{

    /// <summary>
    /// Expression 驱动的行为源数据。
    /// </summary>
    [BlocklySource("行为节点/表达式行为", typeof(LogicBehavior))]
    public sealed class LogicBehaviorSource : BehaviorNodeSource
    {
        [ExpressionSignature]
        [BlocklySourceSlot("启动时", 1)]
        public ExpressionBlockly onStart;

        [ExpressionSignature(typeof(bool))]
        [BlocklySourceSlot("每帧", 2)]
        public ExpressionBlockly onTick;

        [ExpressionSignature]
        [BlocklySourceSlot("延迟帧", 3)]
        public ExpressionBlockly onLateTick;

        [ExpressionSignature]
        [BlocklySourceSlot("结束时", 4)]
        public ExpressionBlockly onFinish;
    }

    /// <summary>
    /// Expression 驱动的叶子行为运行时节点。
    /// 通过 UGCWorld 工厂创建 ExpressionBlockly 作为局部作用域。
    /// </summary>
    sealed class LogicBehavior : CompositeBehavior<LogicBehaviorSource>
    {
        private ExpressionBlockly.Blockly _onStartScope;
        private ExpressionBlockly.Blockly _onTickScope;
        private ExpressionBlockly.Blockly _onLateTickScope;
        private ExpressionBlockly.Blockly _onFinishScope;

        protected override void Initialize()
        {
            _onStartScope = blockly.CreateBlockly(source.onStart);
            _onTickScope = blockly.CreateBlockly(source.onTick);
            _onLateTickScope = blockly.CreateBlockly(source.onLateTick);
            _onFinishScope = blockly.CreateBlockly(source.onFinish);
        }

        protected override void OnStart()
        {
            _onStartScope?.Invoke();
        }

        protected override BehaviorResult OnTick(float deltaTime)
        {
            // 叶子时长门控：onTick (bool 表达式) 求值 —— true 表示本帧完事、false 表示再来一帧。
            // 这是叶子对自身活动周期的时长控制，与节点间分支决策正交。
            if (_onTickScope == null) return BehaviorResult.Done;
            return _onTickScope.Call<bool>() ? BehaviorResult.Done : BehaviorResult.Running;
        }

        protected override void OnLateTick(float deltaTime)
        {
            _onLateTickScope?.Invoke();
        }

        protected override void OnFinish()
        {
            _onFinishScope?.Invoke();
        }

        protected override void OnResetData()
        {
        }

        protected override void OnDestroy()
        {
            blockly.DestroyBlockly(_onStartScope);
            blockly.DestroyBlockly(_onTickScope);
            blockly.DestroyBlockly(_onLateTickScope);
            blockly.DestroyBlockly(_onFinishScope);

            _onStartScope = null;
            _onTickScope = null;
            _onLateTickScope = null;
            _onFinishScope = null;
        }
    }
}
