namespace Vena.Blockly.Samples
{

    #region 测试行为1

    public class SampleBehaviorImpl1 : IBehaviorImpl
    {
        public void Start(BehaviorGraph.Blockly graph)
        {
            // DebugSystem.LogWarning( LogCategory.Framework, "测试行为/测试行为1... Start");
        }

        public bool Tick(BehaviorGraph.Blockly graph, float deltaTime)
        {
            // DebugSystem.LogWarning( LogCategory.Framework, "测试行为/测试行为1... Tick");
            return true;
        }

        public void LateTick(BehaviorGraph.Blockly graph, float deltaTime)
        {
            // DebugSystem.LogWarning( LogCategory.Framework, "测试行为/测试行为1... LateTick");
        }

        public void Finish(BehaviorGraph.Blockly graph)
        {
            // DebugSystem.LogWarning( LogCategory.Framework, "测试行为/测试行为1... Finish");
        }
    }

    /// <summary>
    /// TODO: 所有IBehaviorImpl 参考此代码  生成BehaviorSource代码的示例
    /// </summary>
    [UgcSource("测试行为/测试行为1", typeof(SampleBehaviorImpl1Source.Node))]
    public sealed class SampleBehaviorImpl1Source : BehaviorNodeSource<SampleBehaviorImpl1>
    {
        sealed class Node : BehaviorNode<SampleBehaviorImpl1Source, SampleBehaviorImpl1>
        {
            protected override void Initialize()
            {
            }

            protected override void InitializeProperties(SampleBehaviorImpl1 behaviorImpl)
            {
            }

            protected override void CleanProperties(SampleBehaviorImpl1 behaviorImpl)
            {
            }

            protected override void OnBeforeDestroy()
            {

            }
        }
    }

    #endregion

    #region 测试行为2

    public class SampleBehaviorImpl2 : IBehaviorImpl
    {
        public string message;

        public void Start(BehaviorGraph.Blockly graph)
        {
            // DebugSystem.LogWarning( LogCategory.Framework, $"测试行为/测试行为2... Start, {message}");
        }

        public bool Tick(BehaviorGraph.Blockly graph, float deltaTime)
        {
            // DebugSystem.LogWarning( LogCategory.Framework, $"测试行为/测试行为2... Tick, {message}");
            return true;
        }

        public void LateTick(BehaviorGraph.Blockly graph, float deltaTime)
        {
            // DebugSystem.LogWarning( LogCategory.Framework, $"测试行为/测试行为2... LateTick, {message}");
        }

        public void Finish(BehaviorGraph.Blockly graph)
        {
            // DebugSystem.LogWarning( LogCategory.Framework, $"测试行为/测试行为2... Finish, {message}");
        }
    }

    /// <summary>
    /// TODO: 所有IBehaviorImpl 参考此代码  生成BehaviorSource代码的示例
    /// </summary>
    [UgcSource("测试行为/测试行为2", typeof(SampleBehaviorImpl2Source.Node))]
    public sealed class SampleBehaviorImpl2Source : BehaviorNodeSource<SampleBehaviorImpl2>
    {
        [ExpressionSignature(typeof(string))]
        [UgcSourceProperty("消息", 1)]
        public LogicGraph message;

        sealed class Node : BehaviorNode<SampleBehaviorImpl2Source, SampleBehaviorImpl2>
        {
            private LogicGraph.Blockly _message;

            protected override void Initialize()
            {
                _message = blockly.CreateBlockly(source.message);
            }

            protected override void InitializeProperties(SampleBehaviorImpl2 behaviorImpl)
            {
                behaviorImpl.message = _message.Call<string>();
            }

            protected override void CleanProperties(SampleBehaviorImpl2 behaviorImpl)
            {
                behaviorImpl.message = null;
            }

            protected override void OnBeforeDestroy()
            {
                blockly.DestroyBlockly(_message);
                _message = null;
            }
        }
    }

    #endregion
}
