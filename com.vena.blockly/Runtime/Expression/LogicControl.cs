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
        public LogicGraph[] statements;

        internal sealed class Node : ILogicNode
        {
            public LogicGraph.Blockly Blockly { get; private set; }
            Blockly IBlock.scope => Blockly;
            private LogicSequence _source;
            private LogicGraph.Blockly[] _statements;

            void ILogicNode.Init(LogicGraph.Blockly blockly, Expression source)
            {
                Blockly = blockly;
                _source = (LogicSequence)source;
                Initialize();
            }

            void ILogicNode.Evaluate()
            {
                if (_statements == null) return;
                for (int i = 0; i < _statements.Length; i++)
                {
                    _statements[i]?.Invoke();
                }
            }

            void IBlock.Destroy()
            {
                OnDestroy();
                Blockly = null;
                _source = null;
            }

            private void Initialize()
            {
                if (_source.statements != null)
                {
                    _statements = new LogicGraph.Blockly[_source.statements.Length];
                    for (int i = 0; i < _source.statements.Length; i++)
                    {
                        _statements[i] = Blockly.CreateBlockly(_source.statements[i]);
                    }
                }
            }

            private void OnDestroy()
            {
                if (_statements != null)
                {
                    for (int i = 0; i < _statements.Length; i++)
                    {
                        Blockly.DestroyBlockly(_statements[i]);
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
        public LogicGraph condition;

        [BlocklySourceSlot("为真时", 2)]
        public LogicGraph trueBranch;

        [BlocklySourceSlot("为假时", 3)]
        public LogicGraph falseBranch;

        internal sealed class Node : ILogicNode
        {
            public LogicGraph.Blockly Blockly { get; private set; }
            Blockly IBlock.scope => Blockly;
            private LogicBranch _source;
            private LogicGraph.Blockly _condition;
            private LogicGraph.Blockly _trueBranch;
            private LogicGraph.Blockly _falseBranch;

            void ILogicNode.Init(LogicGraph.Blockly blockly, Expression source)
            {
                Blockly = blockly;
                _source = (LogicBranch)source;
                Initialize();
            }

            void ILogicNode.Evaluate()
            {
                bool result = _condition.Call<bool>();
                if (result)
                    _trueBranch?.Invoke();
                else
                    _falseBranch?.Invoke();
            }

            void IBlock.Destroy()
            {
                OnDestroy();
                Blockly = null;
                _source = null;
            }

            private void Initialize()
            {
                _condition = Blockly.CreateBlockly(_source.condition);
                _trueBranch = Blockly.CreateBlockly(_source.trueBranch);
                _falseBranch = Blockly.CreateBlockly(_source.falseBranch);
            }

            private void OnDestroy()
            {
                Blockly.DestroyBlockly(_condition);
                Blockly.DestroyBlockly(_trueBranch);
                Blockly.DestroyBlockly(_falseBranch);
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
        public LogicGraph condition;

        [BlocklySourceSlot("循环体", 2)]
        public LogicGraph body;

        [BlocklySourceSlot("最大迭代次数", 3)]
        public int maxIterations = 10000;

        internal sealed class Node : ILogicNode
        {
            public LogicGraph.Blockly Blockly { get; private set; }
            Blockly IBlock.scope => Blockly;
            private LogicWhile _source;
            private LogicGraph.Blockly _condition;
            private LogicGraph.Blockly _body;

            void ILogicNode.Init(LogicGraph.Blockly blockly, Expression source)
            {
                Blockly = blockly;
                _source = (LogicWhile)source;
                Initialize();
            }

            void ILogicNode.Evaluate()
            {
                int max = _source.maxIterations;
                int count = 0;
                while (_condition.Call<bool>())
                {
                    _body?.Invoke();
                    if (++count >= max)
                        throw new InvalidOperationException(
                            $"LogicWhile exceeded maxIterations={max}, possible infinite loop");
                }
            }

            void IBlock.Destroy()
            {
                OnDestroy();
                Blockly = null;
                _source = null;
            }

            private void Initialize()
            {
                _condition = Blockly.CreateBlockly(_source.condition);
                _body = Blockly.CreateBlockly(_source.body);
            }

            private void OnDestroy()
            {
                Blockly.DestroyBlockly(_condition);
                Blockly.DestroyBlockly(_body);
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

        protected internal sealed class Node : ILogicNode
        {
            public LogicGraph.Blockly Blockly { get; private set; }
            Blockly IBlock.scope => Blockly;
            private LogicGetVariable<T> _source;

            void ILogicNode.Init(LogicGraph.Blockly blockly, Expression source)
            {
                Blockly = blockly;
                _source = (LogicGetVariable<T>)source;
            }

            void ILogicNode.Evaluate()
            {
                Blockly.Push<T>(Blockly.GetVariable<T>(_source.variableName));
            }

            void IBlock.Destroy()
            {
                Blockly = null;
                _source = null;
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
        public LogicGraph value;

        protected internal sealed class Node : ILogicNode
        {
            public LogicGraph.Blockly Blockly { get; private set; }
            Blockly IBlock.scope => Blockly;
            private LogicSetVariable<T> _source;
            private LogicGraph.Blockly _value;

            void ILogicNode.Init(LogicGraph.Blockly blockly, Expression source)
            {
                Blockly = blockly;
                _source = (LogicSetVariable<T>)source;
                Initialize();
            }

            void ILogicNode.Evaluate()
            {
                T result = _value.Call<T>();
                Blockly.SetVariable<T>(_source.variableName, result);
            }

            void IBlock.Destroy()
            {
                OnDestroy();
                Blockly = null;
                _source = null;
            }

            private void Initialize()
            {
                _value = Blockly.CreateBlockly(_source.value);
            }

            private void OnDestroy()
            {
                Blockly.DestroyBlockly(_value);
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
