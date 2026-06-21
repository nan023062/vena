using Vena.Blockly;

namespace Vena.Blockly.Samples
{

    /*
      TODO: 实例的属性设置/获取  代码生成示例
     */
    [UgcClass("测试对象")]
    public class InstanceProperty
    {
        [UgcProperty("年龄")]
        public int age;

        [UgcProperty("位置")]
        public Vector3 position;

        [UgcProperty("生命值")]
        public uint Hp
        {
            get;
            set;
        }

        [UgcProperty("魔法值")]
        public uint Mp
        {
            get;
            private set;
        }
    }

    #region Setter Impl

    public class SetAgeImpl : IProcedureImpl
    {
        public InstanceProperty instance;

        public int value;

        public void Evaluate()
        {
            instance.age = value;
        }
    }

    #endregion

    #region Getter Impl

    public class GetAgeImpl : IFunctionImpl<int>
    {
        public InstanceProperty instance;

        public int Evaluate()
        {
            return instance.age;
        }
    }
    #endregion

    #region Setter Source

    [UgcSource("测试对象/设置年龄", typeof(InstancePropertySetter_age.Node))]
    public sealed class InstancePropertySetter_age : Procedure<SetAgeImpl>
    {
        [UgcSourceProperty("实例", 1)]
        public Expression instance;

        [UgcSourceProperty("值", 2)]
        public Expression value;

        sealed class Node : Block<InstancePropertySetter_age>
        {
            private ILogicNode _instance;

            private ILogicNode _value;

            protected override void Initialize()
            {
                _instance = Blockly.CreateBlock(source.instance);

                _value = Blockly.CreateBlock(source.value);
            }

            protected override void InitializeProperties(SetAgeImpl impl)
            {
                _instance.Evaluate();
                impl.instance = Blockly.Pop<InstanceProperty>();

                _value.Evaluate();
                impl.value = Blockly.Pop<int>();
            }

            protected override void CleanProperties(SetAgeImpl impl)
            {
                impl.instance = null;
            }

            protected override void OnDestroy()
            {
                Blockly.DestroyBlock(_instance);
                _instance = null;

                Blockly.DestroyBlock(_value);
                _value = null;
            }
        }
    }

    #endregion

    #region Getter Source

    [UgcSource("测试对象/获取年龄", typeof(InstancePropertyGetter_age.Node))]
    public sealed class InstancePropertyGetter_age : Function<GetAgeImpl, int>
    {
        [UgcSourceProperty("实例", 1)]
        public Expression instance;

        sealed class Node : Block<InstancePropertyGetter_age>
        {
            private ILogicNode _instance;

            protected override void Initialize()
            {
                _instance = Blockly.CreateBlock(source.instance);
            }

            protected override void InitializeProperties(GetAgeImpl impl)
            {
                _instance.Evaluate();
                impl.instance = Blockly.Pop<InstanceProperty>();
            }

            protected override void CleanProperties(GetAgeImpl impl)
            {
                impl.instance = null;
            }

            protected override void OnDestroy()
            {
                Blockly.DestroyBlock(_instance);
                _instance = null;
            }
        }
    }

    #endregion
}
