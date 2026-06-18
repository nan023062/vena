namespace Vena.Blockly
{

    /// <summary>
    /// Expression 驱动的行为源数据。
    /// </summary>
    [UgcSource("行为节点/表达式行为", typeof(LogicBehavior))]
    public sealed class LogicBehaviorSource : BehaviorNodeSource
    {
        [ExpressionSignature]
        [UgcSourceProperty("启动时", 1)]
        public LogicGraph onStart;

        [ExpressionSignature(typeof(bool))]
        [UgcSourceProperty("每帧", 2)]
        public LogicGraph onTick;

        [ExpressionSignature]
        [UgcSourceProperty("延迟帧", 3)]
        public LogicGraph onLateTick;

        [ExpressionSignature]
        [UgcSourceProperty("结束时", 4)]
        public LogicGraph onFinish;
    }

    /// <summary>
    /// Expression 驱动的叶子行为运行时节点。
    /// 通过 UGCWorld 工厂创建 LogicGraph 作为局部作用域。
    /// </summary>
    sealed class LogicBehavior : CompositeBehavior<LogicBehaviorSource>
    {
        private LogicGraph.Blockly _onStartScope;
        private LogicGraph.Blockly _onTickScope;
        private LogicGraph.Blockly _onLateTickScope;
        private LogicGraph.Blockly _onFinishScope;

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

        protected override bool OnTick(float deltaTime)
        {
            return _onTickScope?.Call<bool>() ?? true;
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
