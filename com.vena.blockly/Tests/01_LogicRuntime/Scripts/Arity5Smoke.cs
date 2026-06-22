// -----------------------------------------------------------------------------
// Vena Blockly
// Visual scripting block graph runtime for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

namespace Vena.Blockly.Tests.LogicRuntime
{

    /// <summary>
    /// 5 参 smoke 用例 —— 验证 IProcedureImpl / IFunctionImpl 高 arity 接口与基类包装编译/调用对齐。
    /// </summary>

    #region 5 参 IFunctionImpl smoke

    /// <summary>5 整数求和（int * 5 → int）。</summary>
    public sealed class Sum5IntsImpl : IFunctionImpl<int, int, int, int, int, int>
    {
        public int Evaluate(in int a, in int b, in int c, in int d, in int e)
            => a + b + c + d + e;
    }

    [BlocklySource("smoke/5参函数/求和", typeof(Sum5IntsSource.Node))]
    public sealed class Sum5IntsSource : Function<Sum5IntsImpl, int, int, int, int, int, int>
    {
        [BlocklySourceSlot("a", 1)] public Expression a;
        [BlocklySourceSlot("b", 2)] public Expression b;
        [BlocklySourceSlot("c", 3)] public Expression c;
        [BlocklySourceSlot("d", 4)] public Expression d;
        [BlocklySourceSlot("e", 5)] public Expression e;

        sealed class Node : Block<Sum5IntsSource>
        {
            private ILogicNode _a, _b, _c, _d, _e;

            protected override void Initialize()
            {
                _a = Blockly.CreateBlock(source.a);
                _b = Blockly.CreateBlock(source.b);
                _c = Blockly.CreateBlock(source.c);
                _d = Blockly.CreateBlock(source.d);
                _e = Blockly.CreateBlock(source.e);
            }

            protected override void EvaluateChildren()
            {
                // 求值顺序：a、b、c、d、e 依次 Evaluate（每个内部 Push）
                // 调用方 Block 包装层会自上而下 Pop：T5 先 Pop（即 e），T1 最后 Pop（即 a），
                // 与压栈顺序对称。
                _a.Evaluate();
                _b.Evaluate();
                _c.Evaluate();
                _d.Evaluate();
                _e.Evaluate();
            }

            protected override void InitializeProperties(Sum5IntsImpl impl) { }

            protected override void CleanProperties(Sum5IntsImpl impl) { }

            protected override void OnDestroy()
            {
                Blockly.DestroyBlock(_a); _a = null;
                Blockly.DestroyBlock(_b); _b = null;
                Blockly.DestroyBlock(_c); _c = null;
                Blockly.DestroyBlock(_d); _d = null;
                Blockly.DestroyBlock(_e); _e = null;
            }
        }
    }

    #endregion

    #region 5 参 IProcedureImpl smoke

    /// <summary>把 5 个数累加写入 sink（5 参无返回值）。</summary>
    public sealed class AccumulateInto5Impl : IProcedureImpl<Accumulator, int, int, int, int>
    {
        public void Evaluate(in Accumulator sink, in int a, in int b, in int c, in int d)
        {
            sink.value += a + b + c + d;
        }
    }

    public sealed class Accumulator
    {
        public int value;
    }

    [BlocklySource("smoke/5参过程/累加到容器", typeof(AccumulateInto5Source.Node))]
    public sealed class AccumulateInto5Source : Procedure<AccumulateInto5Impl, Accumulator, int, int, int, int>
    {
        [BlocklySourceSlot("sink", 1)] public Expression sink;
        [BlocklySourceSlot("a", 2)] public Expression a;
        [BlocklySourceSlot("b", 3)] public Expression b;
        [BlocklySourceSlot("c", 4)] public Expression c;
        [BlocklySourceSlot("d", 5)] public Expression d;

        sealed class Node : Block<AccumulateInto5Source>
        {
            private ILogicNode _sink, _a, _b, _c, _d;

            protected override void Initialize()
            {
                _sink = Blockly.CreateBlock(source.sink);
                _a = Blockly.CreateBlock(source.a);
                _b = Blockly.CreateBlock(source.b);
                _c = Blockly.CreateBlock(source.c);
                _d = Blockly.CreateBlock(source.d);
            }

            protected override void EvaluateChildren()
            {
                _sink.Evaluate();
                _a.Evaluate();
                _b.Evaluate();
                _c.Evaluate();
                _d.Evaluate();
            }

            protected override void InitializeProperties(AccumulateInto5Impl impl) { }

            protected override void CleanProperties(AccumulateInto5Impl impl) { }

            protected override void OnDestroy()
            {
                Blockly.DestroyBlock(_sink); _sink = null;
                Blockly.DestroyBlock(_a); _a = null;
                Blockly.DestroyBlock(_b); _b = null;
                Blockly.DestroyBlock(_c); _c = null;
                Blockly.DestroyBlock(_d); _d = null;
            }
        }
    }

    #endregion
}
