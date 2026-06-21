using Vena.Blockly;

namespace Vena.Blockly.Samples
{

    #region Impl

    public class ConstBoolImpl : IFunctionImpl<bool>
    {
        public bool value;

        public bool Evaluate() => value;
    }

    public class ConstIntImpl : IFunctionImpl<int>
    {
        public int value;

        public int Evaluate() => value;
    }

    public class ConstUIntImpl : IFunctionImpl<uint>
    {
        public uint value;

        public uint Evaluate() => value;
    }

    public class ConstFloatImpl : IFunctionImpl<float>
    {
        public float value;

        public float Evaluate() => value;
    }

    public class ConstDoubleImpl : IFunctionImpl<double>
    {
        public double value;

        public double Evaluate() => value;
    }

    public class ConstStringImpl : IFunctionImpl<string>
    {
        public string value;

        public string Evaluate() => value;
    }

    #endregion

    #region Source

    [UgcSource("程序节点/变量/Bool常量", typeof(ValueBool.Node))]
    public sealed class ValueBool : Function<ConstBoolImpl, bool>
    {
        [UgcSourceProperty("常量值", 1)]
        public bool value;

        sealed class Node : Block<ValueBool>
        {
            protected override void Initialize() { }
            protected override void InitializeProperties(ConstBoolImpl impl) { impl.value = source.value; }
            protected override void CleanProperties(ConstBoolImpl impl) { }
        }
    }

    [UgcSource("程序节点/变量/Int常量", typeof(ValueInt.Node))]
    public sealed class ValueInt : Function<ConstIntImpl, int>
    {
        [UgcSourceProperty("常量值", 1)]
        public int value;

        sealed class Node : Block<ValueInt>
        {
            protected override void Initialize() { }
            protected override void InitializeProperties(ConstIntImpl impl) { impl.value = source.value; }
            protected override void CleanProperties(ConstIntImpl impl) { }
        }
    }

    [UgcSource("程序节点/变量/UInt常量", typeof(ValueUInt.Node))]
    public sealed class ValueUInt : Function<ConstUIntImpl, uint>
    {
        [UgcSourceProperty("常量值", 1)]
        public uint value;

        sealed class Node : Block<ValueUInt>
        {
            protected override void Initialize() { }
            protected override void InitializeProperties(ConstUIntImpl impl) { impl.value = source.value; }
            protected override void CleanProperties(ConstUIntImpl impl) { }
        }
    }

    [UgcSource("程序节点/变量/Float常量", typeof(ValueFloat.Node))]
    public sealed class ValueFloat : Function<ConstFloatImpl, float>
    {
        [UgcSourceProperty("常量值", 1)]
        public float value;

        sealed class Node : Block<ValueFloat>
        {
            protected override void Initialize() { }
            protected override void InitializeProperties(ConstFloatImpl impl) { impl.value = source.value; }
            protected override void CleanProperties(ConstFloatImpl impl) { }
        }
    }

    [UgcSource("程序节点/变量/Double常量", typeof(ValueDouble.Node))]
    public sealed class ValueDouble : Function<ConstDoubleImpl, double>
    {
        [UgcSourceProperty("常量值", 1)]
        public double value;

        sealed class Node : Block<ValueDouble>
        {
            protected override void Initialize() { }
            protected override void InitializeProperties(ConstDoubleImpl impl) { impl.value = source.value; }
            protected override void CleanProperties(ConstDoubleImpl impl) { }
        }
    }

    [UgcSource("程序节点/变量/String常量", typeof(ValueString.Node))]
    public sealed class ValueString : Function<ConstStringImpl, string>
    {
        [UgcSourceProperty("常量值", 1)]
        public string value;

        sealed class Node : Block<ValueString>
        {
            protected override void Initialize() { }
            protected override void InitializeProperties(ConstStringImpl impl) { impl.value = source.value; }
            protected override void CleanProperties(ConstStringImpl impl) { impl.value = null; }
        }
    }

    #endregion
}
