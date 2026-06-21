using Vena.Blockly;

namespace Vena.Blockly.Samples
{

    /*
      TODO: 实例方法 代码生成示例
     */
    [UgcClass("测试对象")]
    public class InstanceMethod
    {
        [UgcMethod("加法", false, "参数1", "参数2")]
        public int TestMethod(int a, int b)
        {
            return a + b;
        }

        [UgcMethod("打印消息", false, "消息内容")]
        public void PrintMessage(string message)
        {
            System.Diagnostics.Debug.WriteLine($"InstanceMethod.PrintMessage: {message}");
        }
    }

    #region Impl

    [UgcGenerated]
    public sealed class InstanceMethodTestMethodImpl : IFunctionImpl<int>
    {
        public Vena.Blockly.Samples.InstanceMethod instance;

        public int a;

        public int b;

        public int Evaluate()
        {
            return instance.TestMethod(a, b);
        }
    }

    [UgcGenerated]
    public sealed class InstanceMethodPrintMessageImpl : IProcedureImpl
    {
        public Vena.Blockly.Samples.InstanceMethod instance;

        public string message;

        public void Evaluate()
        {
            instance.PrintMessage(message);
        }
    }
    #endregion

    #region Source

    [UgcGenerated]
    [UgcSource("测试对象/加法", typeof(InstanceMethodTestMethod.Node))]
    public sealed class InstanceMethodTestMethod : Function<InstanceMethodTestMethodImpl, int>
    {
        [UgcSourceProperty("实例", 1)]
        public Expression instance;

        [UgcSourceProperty("参数1", 2)]
        public Expression a;

        [UgcSourceProperty("参数2", 3)]
        public Expression b;

        [UgcGenerated]
        sealed class Node : Block<InstanceMethodTestMethod>
        {
            private ILogicNode _instance;
            private ILogicNode _a;
            private ILogicNode _b;

            protected override void Initialize()
            {
                _instance = Blockly.CreateBlock(source.instance);
                _a = Blockly.CreateBlock(source.a);
                _b = Blockly.CreateBlock(source.b);
            }

            protected override void InitializeProperties(InstanceMethodTestMethodImpl impl)
            {
                _instance.Evaluate();
                impl.instance = Blockly.Pop<Vena.Blockly.Samples.InstanceMethod>();
                _a.Evaluate();
                impl.a = Blockly.Pop<int>();
                _b.Evaluate();
                impl.b = Blockly.Pop<int>();
            }

            protected override void CleanProperties(InstanceMethodTestMethodImpl impl)
            {
                impl.instance = null;
            }

            protected override void OnDestroy()
            {
                Blockly.DestroyBlock(_instance);
                _instance = null;
                Blockly.DestroyBlock(_a);
                _a = null;
                Blockly.DestroyBlock(_b);
                _b = null;
            }
        }
    }

    [UgcGenerated]
    [UgcSource("测试对象/打印消息", typeof(InstanceMethodPrintMessage.Node))]
    public sealed class InstanceMethodPrintMessage : Procedure<InstanceMethodPrintMessageImpl>
    {
        [UgcSourceProperty("实例", 1)]
        public Expression instance;

        [UgcSourceProperty("消息内容", 2)]
        public Expression message;

        [UgcGenerated]
        sealed class Node : Block<InstanceMethodPrintMessage>
        {
            private ILogicNode _instance;
            private ILogicNode _message;

            protected override void Initialize()
            {
                _instance = Blockly.CreateBlock(source.instance);
                _message = Blockly.CreateBlock(source.message);
            }

            protected override void InitializeProperties(InstanceMethodPrintMessageImpl impl)
            {
                _instance.Evaluate();
                impl.instance = Blockly.Pop<Vena.Blockly.Samples.InstanceMethod>();
                _message.Evaluate();
                impl.message = Blockly.Pop<string>();
            }

            protected override void CleanProperties(InstanceMethodPrintMessageImpl impl)
            {
                impl.instance = null;
                impl.message = null;
            }

            protected override void OnDestroy()
            {
                Blockly.DestroyBlock(_instance);
                _instance = null;
                Blockly.DestroyBlock(_message);
                _message = null;
            }
        }
    }
    #endregion
}
