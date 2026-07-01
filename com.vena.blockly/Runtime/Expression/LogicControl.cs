// -----------------------------------------------------------------------------
// Vena Blockly
// Visual scripting block graph runtime for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System;

namespace Vena.Blockly
{

    #region LogicSequence

    [BlocklySource("程序节点/控制/顺序执行", typeof(LogicSequence.Node))]
    public sealed class LogicSequence : Expression
    {
        [BlocklySourceSlot("语句列表", 1)]
        public Expression[] statements;

        internal sealed class Node : Block<LogicSequence>
        {
            private ILogicNode[] _statements;

            protected override void Initialize()
            {
                if (source.statements == null)
                {
                    _statements = null;
                    return;
                }
                _statements = new ILogicNode[source.statements.Length];
                for (int i = 0; i < source.statements.Length; i++)
                {
                    var s = source.statements[i];
                    _statements[i] = s != null ? Blockly.CreateBlock(s) : null;
                }
            }

            public override void Evaluate()
            {
                if (_statements == null) return;
                for (int i = 0; i < _statements.Length; i++)
                {
                    _statements[i]?.Evaluate();
                }
            }

            protected override void OnDestroy()
            {
                if (_statements != null)
                {
                    for (int i = 0; i < _statements.Length; i++)
                    {
                        Blockly.DestroyBlock(_statements[i]);
                        _statements[i] = null;
                    }
                    _statements = null;
                }
            }
        }
    }

    #endregion

    #region LogicBranch

    [BlocklySource("程序节点/控制/条件分支", typeof(LogicBranch.Node))]
    public sealed class LogicBranch : Expression
    {
        [ExpressionSignature(typeof(bool))]
        [BlocklySourceSlot("条件", 1)]
        public Expression condition;

        [BlocklySourceSlot("为真时", 2)]
        public Expression trueBranch;

        [BlocklySourceSlot("为假时", 3)]
        public Expression falseBranch;

        internal sealed class Node : Block<LogicBranch>
        {
            private ILogicNode _condition;
            private ILogicNode _trueBranch;
            private ILogicNode _falseBranch;

            protected override void Initialize()
            {
                _condition = source.condition != null ? Blockly.CreateBlock(source.condition) : null;
                _trueBranch = source.trueBranch != null ? Blockly.CreateBlock(source.trueBranch) : null;
                _falseBranch = source.falseBranch != null ? Blockly.CreateBlock(source.falseBranch) : null;
            }

            public override void Evaluate()
            {
                _condition.Evaluate();
                bool result = Pop<bool>();
                if (result)
                {
                    _trueBranch?.Evaluate();
                }
                else
                {
                    _falseBranch?.Evaluate();
                }
            }

            protected override void OnDestroy()
            {
                Blockly.DestroyBlock(_condition);
                Blockly.DestroyBlock(_trueBranch);
                Blockly.DestroyBlock(_falseBranch);
                _condition = null;
                _trueBranch = null;
                _falseBranch = null;
            }
        }
    }

    #endregion

    #region LogicWhile

    [BlocklySource("程序节点/控制/While循环", typeof(LogicWhile.Node))]
    public sealed class LogicWhile : Expression
    {
        [ExpressionSignature(typeof(bool))]
        [BlocklySourceSlot("条件", 1)]
        public Expression condition;

        [BlocklySourceSlot("循环体", 2)]
        public Expression body;

        [BlocklySourceSlot("最大迭代次数", 3)]
        public int maxIterations = 10000;

        internal sealed class Node : Block<LogicWhile>
        {
            private ILogicNode _condition;
            private ILogicNode _body;

            protected override void Initialize()
            {
                _condition = source.condition != null ? Blockly.CreateBlock(source.condition) : null;
                _body = source.body != null ? Blockly.CreateBlock(source.body) : null;
            }

            public override void Evaluate()
            {
                int max = source.maxIterations;
                int count = 0;
                while (true)
                {
                    _condition.Evaluate();
                    if (!Pop<bool>()) break;
                    _body?.Evaluate();
                    if (++count >= max)
                    {
                        throw new InvalidOperationException(
                            $"LogicWhile exceeded maxIterations={max}, possible infinite loop");
                    }
                }
            }

            protected override void OnDestroy()
            {
                Blockly.DestroyBlock(_condition);
                Blockly.DestroyBlock(_body);
                _condition = null;
                _body = null;
            }
        }
    }

    #endregion

    #region LogicGetVariable<T>

    public abstract class LogicGetVariable<T> : Expression
    {
        [BlocklySourceSlot("变量名", 1)]
        public string variableName;

        protected internal sealed class Node : Block<LogicGetVariable<T>>
        {
            public override void Evaluate()
            {
                Push<T>(Blockly.GetVariable<T>(source.variableName));
            }
        }
    }

    [BlocklySource("程序节点/变量/获取Int变量", typeof(LogicGetVariable<int>.Node))]
    public sealed class LogicGetVariableInt : LogicGetVariable<int> { }

    [BlocklySource("程序节点/变量/获取Bool变量", typeof(LogicGetVariable<bool>.Node))]
    public sealed class LogicGetVariableBool : LogicGetVariable<bool> { }

    [BlocklySource("程序节点/变量/获取Float变量", typeof(LogicGetVariable<float>.Node))]
    public sealed class LogicGetVariableFloat : LogicGetVariable<float> { }

    [BlocklySource("程序节点/变量/获取String变量", typeof(LogicGetVariable<string>.Node))]
    public sealed class LogicGetVariableString : LogicGetVariable<string> { }

    #endregion

    #region LogicSetVariable<T>

    public abstract class LogicSetVariable<T> : Expression
    {
        [BlocklySourceSlot("变量名", 1)]
        public string variableName;

        [BlocklySourceSlot("值", 2)]
        public Expression value;

        protected internal sealed class Node : Block<LogicSetVariable<T>>
        {
            private ILogicNode _value;

            protected override void Initialize()
            {
                _value = source.value != null ? Blockly.CreateBlock(source.value) : null;
            }

            public override void Evaluate()
            {
                _value.Evaluate();
                T result = Pop<T>();
                Blockly.SetVariable<T>(source.variableName, result);
            }

            protected override void OnDestroy()
            {
                Blockly.DestroyBlock(_value);
                _value = null;
            }
        }
    }

    [BlocklySource("程序节点/变量/设置Int变量", typeof(LogicSetVariable<int>.Node))]
    public sealed class LogicSetVariableInt : LogicSetVariable<int> { }

    [BlocklySource("程序节点/变量/设置Bool变量", typeof(LogicSetVariable<bool>.Node))]
    public sealed class LogicSetVariableBool : LogicSetVariable<bool> { }

    [BlocklySource("程序节点/变量/设置Float变量", typeof(LogicSetVariable<float>.Node))]
    public sealed class LogicSetVariableFloat : LogicSetVariable<float> { }

    [BlocklySource("程序节点/变量/设置String变量", typeof(LogicSetVariable<string>.Node))]
    public sealed class LogicSetVariableString : LogicSetVariable<string> { }

    #endregion

}
