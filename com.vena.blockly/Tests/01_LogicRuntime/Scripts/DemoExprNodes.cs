// -----------------------------------------------------------------------------
// Vena Blockly
// Visual scripting block graph runtime for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

namespace Vena.Blockly.Tests.LogicRuntime
{
    #region ConstInt

    /// <summary>常量 int 求值实现：Evaluate 直接吐回字段 value。</summary>
    public sealed class ConstIntImpl : IFunctionImpl<int>
    {
        public int value;

        public int Evaluate() => value;
    }

    /// <summary>ConstInt 的 source + Node 装配。</summary>
    [BlocklySource("示例表达式/Int常量", typeof(ConstIntSource.Node))]
    public sealed class ConstIntSource : Function<ConstIntImpl, int>
    {
        public int value;

        sealed class Node : Block<ConstIntSource>
        {
            protected override void Initialize() { }
            protected override void InitializeProperties(ConstIntImpl impl) { impl.value = source.value; }
            protected override void CleanProperties(ConstIntImpl impl) { }
        }
    }

    #endregion

    #region ConstFloat

    /// <summary>常量 float 求值实现：Evaluate 直接吐回字段 value。</summary>
    public sealed class ConstFloatImpl : IFunctionImpl<float>
    {
        public float value;

        public float Evaluate() => value;
    }

    /// <summary>ConstFloat 的 source + Node 装配。</summary>
    [BlocklySource("示例表达式/Float常量", typeof(ConstFloatSource.Node))]
    public sealed class ConstFloatSource : Function<ConstFloatImpl, float>
    {
        public float value;

        sealed class Node : Block<ConstFloatSource>
        {
            protected override void Initialize() { }
            protected override void InitializeProperties(ConstFloatImpl impl) { impl.value = source.value; }
            protected override void CleanProperties(ConstFloatImpl impl) { }
        }
    }

    #endregion

    #region AddInt

    /// <summary>(int, int) → int 加法求值实现。</summary>
    public sealed class AddIntImpl : IFunctionImpl<int, int, int>
    {
        public int Evaluate(in int a, in int b) => a + b;
    }

    /// <summary>
    /// AddInt 的 source + Node 装配。两个子 Expression（a / b）通过 Blockly 栈帧
    /// 把求值结果 Push 到栈，基类自动 Pop&lt;T1&gt;/Pop&lt;T2&gt; 喂给 impl.Evaluate(in a, in b)。
    /// </summary>
    [BlocklySource("示例表达式/Int加法", typeof(AddIntSource.Node))]
    public sealed class AddIntSource : Function<AddIntImpl, int, int, int>
    {
        public Expression a;

        public Expression b;

        sealed class Node : Block<AddIntSource>
        {
            private ILogicNode _a;

            private ILogicNode _b;

            protected override void Initialize()
            {
                _a = Blockly.CreateBlock(source.a);
                _b = Blockly.CreateBlock(source.b);
            }

            protected override void EvaluateChildren()
            {
                // Function<TImpl, T1, T2, TOutput> 基类 Pop 顺序为先 T2 后 T1，
                // 因此这里先 Evaluate(a) 让其 Push T1，再 Evaluate(b) 让其 Push T2。
                _a.Evaluate();
                _b.Evaluate();
            }

            protected override void InitializeProperties(AddIntImpl impl) { }

            protected override void CleanProperties(AddIntImpl impl) { }

            protected override void OnDestroy()
            {
                Blockly.DestroyBlock(_a);
                Blockly.DestroyBlock(_b);
                _a = null;
                _b = null;
            }
        }
    }

    #endregion

    #region SubtractInt

    /// <summary>(int, int) → int 减法求值实现。</summary>
    public sealed class SubtractIntImpl : IFunctionImpl<int, int, int>
    {
        public int Evaluate(in int a, in int b) => a - b;
    }

    /// <summary>SubtractInt 的 source + Node 装配。结构同 AddIntSource。</summary>
    [BlocklySource("示例表达式/Int减法", typeof(SubtractIntSource.Node))]
    public sealed class SubtractIntSource : Function<SubtractIntImpl, int, int, int>
    {
        [BlocklySourceProperty("a", 1)] public Expression a;
        [BlocklySourceProperty("b", 2)] public Expression b;

        sealed class Node : Block<SubtractIntSource>
        {
            private ILogicNode _a;
            private ILogicNode _b;

            protected override void Initialize()
            {
                _a = Blockly.CreateBlock(source.a);
                _b = Blockly.CreateBlock(source.b);
            }

            protected override void EvaluateChildren()
            {
                _a.Evaluate();
                _b.Evaluate();
            }

            protected override void InitializeProperties(SubtractIntImpl impl) { }

            protected override void CleanProperties(SubtractIntImpl impl) { }

            protected override void OnDestroy()
            {
                Blockly.DestroyBlock(_a); _a = null;
                Blockly.DestroyBlock(_b); _b = null;
            }
        }
    }

    #endregion

    #region MultiplyInt

    /// <summary>(int, int) → int 乘法求值实现。</summary>
    public sealed class MultiplyIntImpl : IFunctionImpl<int, int, int>
    {
        public int Evaluate(in int a, in int b) => a * b;
    }

    /// <summary>MultiplyInt 的 source + Node 装配。结构同 AddIntSource。</summary>
    [BlocklySource("示例表达式/Int乘法", typeof(MultiplyIntSource.Node))]
    public sealed class MultiplyIntSource : Function<MultiplyIntImpl, int, int, int>
    {
        [BlocklySourceProperty("a", 1)] public Expression a;
        [BlocklySourceProperty("b", 2)] public Expression b;

        sealed class Node : Block<MultiplyIntSource>
        {
            private ILogicNode _a;
            private ILogicNode _b;

            protected override void Initialize()
            {
                _a = Blockly.CreateBlock(source.a);
                _b = Blockly.CreateBlock(source.b);
            }

            protected override void EvaluateChildren()
            {
                _a.Evaluate();
                _b.Evaluate();
            }

            protected override void InitializeProperties(MultiplyIntImpl impl) { }

            protected override void CleanProperties(MultiplyIntImpl impl) { }

            protected override void OnDestroy()
            {
                Blockly.DestroyBlock(_a); _a = null;
                Blockly.DestroyBlock(_b); _b = null;
            }
        }
    }

    #endregion

    #region GreaterThanInt

    /// <summary>(int, int) → bool 大于比较求值实现。</summary>
    public sealed class GreaterThanIntImpl : IFunctionImpl<int, int, bool>
    {
        public bool Evaluate(in int a, in int b) => a > b;
    }

    /// <summary>
    /// GreaterThanInt 的 source + Node 装配。返回 bool — Function 基类
    /// Block.Evaluate() 内部会 Push&lt;bool&gt; 结果，调用方（如 LogicBranch.condition
    /// 的 LogicGraph.Call&lt;bool&gt;()）Pop&lt;bool&gt; 取出。
    /// </summary>
    [BlocklySource("示例表达式/Int大于", typeof(GreaterThanIntSource.Node))]
    public sealed class GreaterThanIntSource : Function<GreaterThanIntImpl, int, int, bool>
    {
        [BlocklySourceProperty("a", 1)] public Expression a;
        [BlocklySourceProperty("b", 2)] public Expression b;

        sealed class Node : Block<GreaterThanIntSource>
        {
            private ILogicNode _a;
            private ILogicNode _b;

            protected override void Initialize()
            {
                _a = Blockly.CreateBlock(source.a);
                _b = Blockly.CreateBlock(source.b);
            }

            protected override void EvaluateChildren()
            {
                _a.Evaluate();
                _b.Evaluate();
            }

            protected override void InitializeProperties(GreaterThanIntImpl impl) { }

            protected override void CleanProperties(GreaterThanIntImpl impl) { }

            protected override void OnDestroy()
            {
                Blockly.DestroyBlock(_a); _a = null;
                Blockly.DestroyBlock(_b); _b = null;
            }
        }
    }

    #endregion

    #region LessThanOrEqualInt

    /// <summary>(int, int) → bool 小于等于比较求值实现。</summary>
    public sealed class LessThanOrEqualIntImpl : IFunctionImpl<int, int, bool>
    {
        public bool Evaluate(in int a, in int b) => a <= b;
    }

    /// <summary>LessThanOrEqualInt 的 source + Node 装配。返回 bool。</summary>
    [BlocklySource("示例表达式/Int小于等于", typeof(LessThanOrEqualIntSource.Node))]
    public sealed class LessThanOrEqualIntSource : Function<LessThanOrEqualIntImpl, int, int, bool>
    {
        [BlocklySourceProperty("a", 1)] public Expression a;
        [BlocklySourceProperty("b", 2)] public Expression b;

        sealed class Node : Block<LessThanOrEqualIntSource>
        {
            private ILogicNode _a;
            private ILogicNode _b;

            protected override void Initialize()
            {
                _a = Blockly.CreateBlock(source.a);
                _b = Blockly.CreateBlock(source.b);
            }

            protected override void EvaluateChildren()
            {
                _a.Evaluate();
                _b.Evaluate();
            }

            protected override void InitializeProperties(LessThanOrEqualIntImpl impl) { }

            protected override void CleanProperties(LessThanOrEqualIntImpl impl) { }

            protected override void OnDestroy()
            {
                Blockly.DestroyBlock(_a); _a = null;
                Blockly.DestroyBlock(_b); _b = null;
            }
        }
    }

    #endregion
}
