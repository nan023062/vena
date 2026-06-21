// -----------------------------------------------------------------------------
// Vena Blockly
// Visual scripting block graph runtime for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System;

namespace Vena.Blockly
{

    /// <summary>
    /// 逻辑节点源数据基类
    /// </summary>
    public abstract class Expression : IBlocklySource
    {
        public ulong Guid { get; set; } = 0;
    }

    /// <summary>
    /// 逻辑节点接口
    /// </summary>
    public interface ILogicNode : IBlock
    {
        void Init(LogicGraph.Blockly blockly, Expression source);

        void Evaluate();
    }

    #region ProcedureImpl（无返回值）

    public interface IProcedureImpl
    {
        void Evaluate();
    }

    public interface IProcedureImpl<T1>
    {
        void Evaluate(in T1 arg1);
    }

    public interface IProcedureImpl<T1, T2>
    {
        void Evaluate(in T1 arg1, in T2 arg2);
    }

    public interface IProcedureImpl<T1, T2, T3>
    {
        void Evaluate(in T1 arg1, in T2 arg2, in T3 arg3);
    }

    public interface IProcedureImpl<T1, T2, T3, T4>
    {
        void Evaluate(in T1 arg1, in T2 arg2, in T3 arg3, in T4 arg4);
    }

    public interface IProcedureImpl<T1, T2, T3, T4, T5>
    {
        void Evaluate(in T1 arg1, in T2 arg2, in T3 arg3, in T4 arg4, in T5 arg5);
    }

    #endregion

    #region FunctionImpl（有返回值）

    public interface IFunctionImpl<out TOutput>
    {
        TOutput Evaluate();
    }

    public interface IFunctionImpl<T1, out TOutput>
    {
        TOutput Evaluate(in T1 arg1);
    }

    public interface IFunctionImpl<T1, T2, out TOutput>
    {
        TOutput Evaluate(in T1 arg1, in T2 arg2);
    }

    public interface IFunctionImpl<T1, T2, T3, out TOutput>
    {
        TOutput Evaluate(in T1 arg1, in T2 arg2, in T3 arg3);
    }

    public interface IFunctionImpl<T1, T2, T3, T4, out TOutput>
    {
        TOutput Evaluate(in T1 arg1, in T2 arg2, in T3 arg3, in T4 arg4);
    }

    public interface IFunctionImpl<T1, T2, T3, T4, T5, out TOutput>
    {
        TOutput Evaluate(in T1 arg1, in T2 arg2, in T3 arg3, in T4 arg4, in T5 arg5);
    }

    #endregion

    #region ProcedureSource + FunctionSource Block 基类（底层封装栈逻辑）

    /// <summary>
    /// 0 参数，无返回值
    /// </summary>
    public abstract class Procedure<TImpl> : Expression where TImpl : class, IProcedureImpl, new()
    {
        protected abstract class Block<TSource> : ILogicNode where TSource : Expression
        {
            private readonly TImpl _impl = new TImpl();
            public LogicGraph.Blockly Blockly { get; private set; }
            Blockly IBlock.scope => Blockly;
            public TSource source { get; private set; }

            void ILogicNode.Init(LogicGraph.Blockly b, Expression s)
            {
                if (!(s is TSource ts)) throw new ArgumentException($"{GetType().Name}.Init: source type mismatch.");
                Blockly = b; source = ts;
                Initialize();
            }

            void ILogicNode.Evaluate()
            {
                InitializeProperties(_impl);
                _impl.Evaluate();
            }

            void IBlock.Destroy()
            {
                CleanProperties(_impl);
                OnDestroy();
                Blockly = null; source = null;
            }

            protected abstract void Initialize();
            protected abstract void InitializeProperties(TImpl impl);
            protected abstract void CleanProperties(TImpl impl);
            protected virtual void OnDestroy() { }
        }
    }

    /// <summary>
    /// 0 参数，有返回值（底层自动 Push 返回值到栈）
    /// </summary>
    public abstract class Function<TImpl, TOutput> : Expression where TImpl : class, IFunctionImpl<TOutput>, new()
    {
        protected abstract class Block<TSource> : ILogicNode where TSource : Expression
        {
            private readonly TImpl _impl = new TImpl();
            public LogicGraph.Blockly Blockly { get; private set; }
            Blockly IBlock.scope => Blockly;
            public TSource source { get; private set; }

            void ILogicNode.Init(LogicGraph.Blockly b, Expression s)
            {
                if (!(s is TSource ts)) throw new ArgumentException($"{GetType().Name}.Init: source type mismatch.");
                Blockly = b;
                source = ts;
                Initialize();
            }

            void ILogicNode.Evaluate()
            {
                InitializeProperties(_impl);
                TOutput result = _impl.Evaluate();
                Blockly.Push(result);
            }

            void IBlock.Destroy()
            {
                CleanProperties(_impl);
                OnDestroy();
                Blockly = null; source = null;
            }

            protected abstract void Initialize();
            protected abstract void InitializeProperties(TImpl impl);
            protected abstract void CleanProperties(TImpl impl);
            protected virtual void OnDestroy() { }
        }
    }

    /// <summary>
    /// 1 参数，无返回值（底层自动 Pop 参数）
    /// </summary>
    public abstract class Procedure<TImpl, T1> : Expression where TImpl : class, IProcedureImpl<T1>, new()
    {
        protected abstract class Block<TSource> : ILogicNode where TSource : Expression
        {
            private readonly TImpl _impl = new TImpl();
            public LogicGraph.Blockly Blockly { get; private set; }
            Blockly IBlock.scope => Blockly;
            public TSource source { get; private set; }

            void ILogicNode.Init(LogicGraph.Blockly b, Expression s)
            {
                if (!(s is TSource ts)) throw new ArgumentException($"{GetType().Name}.Init: source type mismatch.");
                Blockly = b; source = ts;
                Initialize();
            }

            void ILogicNode.Evaluate()
            {
                T1 arg1 = Blockly.Pop<T1>();
                InitializeProperties(_impl);
                _impl.Evaluate(arg1);
            }

            void IBlock.Destroy()
            {
                CleanProperties(_impl); OnDestroy();
                Blockly = null; source = null;
            }

            protected abstract void Initialize();
            protected abstract void InitializeProperties(TImpl impl);
            protected abstract void CleanProperties(TImpl impl);
            protected virtual void OnDestroy() { }
        }
    }

    /// <summary>
    /// 1 参数，有返回值（底层自动 Pop 参数 + Push 返回值）
    /// </summary>
    public abstract class Function<TImpl, T1, TOutput> : Expression
        where TImpl : class, IFunctionImpl<T1, TOutput>, new()
    {
        protected abstract class Block<TSource> : ILogicNode where TSource : Expression
        {
            private readonly TImpl _impl = new TImpl();
            public LogicGraph.Blockly Blockly { get; private set; }
            Blockly IBlock.scope => Blockly;
            public TSource source { get; private set; }

            void ILogicNode.Init(LogicGraph.Blockly b, Expression s)
            {
                if (!(s is TSource ts)) throw new ArgumentException($"{GetType().Name}.Init: source type mismatch.");
                Blockly = b; source = ts;
                Initialize();
            }

            void ILogicNode.Evaluate()
            {
                T1 arg1 = Blockly.Pop<T1>();
                InitializeProperties(_impl);
                TOutput result = _impl.Evaluate(arg1);
                Blockly.Push(result);
            }

            void IBlock.Destroy()
            {
                CleanProperties(_impl); OnDestroy();
                Blockly = null; source = null;
            }

            protected abstract void Initialize();
            protected abstract void InitializeProperties(TImpl impl);
            protected abstract void CleanProperties(TImpl impl);
            protected virtual void OnDestroy() { }
        }
    }

    /// <summary>
    /// 2 参数，无返回值（底层自动 Pop 参数）
    /// </summary>
    public abstract class Procedure<TImpl, T1, T2> : Expression where TImpl : class, IProcedureImpl<T1, T2>, new()
    {
        protected abstract class Block<TSource> : ILogicNode where TSource : Expression
        {
            private readonly TImpl _impl = new TImpl();
            public LogicGraph.Blockly Blockly { get; private set; }
            Blockly IBlock.scope => Blockly;
            public TSource source { get; private set; }

            void ILogicNode.Init(LogicGraph.Blockly b, Expression s)
            {
                if (!(s is TSource ts)) throw new ArgumentException($"{GetType().Name}.Init: source type mismatch.");
                Blockly = b; source = ts;
                Initialize();
            }

            void ILogicNode.Evaluate()
            {
                T2 arg2 = Blockly.Pop<T2>();
                T1 arg1 = Blockly.Pop<T1>();
                InitializeProperties(_impl);
                _impl.Evaluate(arg1, arg2);
            }

            void IBlock.Destroy()
            {
                CleanProperties(_impl);
                OnDestroy();
                Blockly = null; source = null;
            }

            protected abstract void Initialize();
            protected abstract void InitializeProperties(TImpl impl);
            protected abstract void CleanProperties(TImpl impl);
            protected virtual void OnDestroy() { }
        }
    }

    /// <summary>
    /// 2 参数，有返回值（底层自动 Pop 参数 + Push 返回值）
    /// </summary>
    public abstract class Function<TImpl, T1, T2, TOutput> : Expression where TImpl : class, IFunctionImpl<T1, T2, TOutput>, new()
    {
        protected abstract class Block<TSource> : ILogicNode where TSource : Expression
        {
            private readonly TImpl _impl = new TImpl();
            public LogicGraph.Blockly Blockly { get; private set; }
            Blockly IBlock.scope => Blockly;
            public TSource source { get; private set; }

            void ILogicNode.Init(LogicGraph.Blockly b, Expression s)
            {
                if (!(s is TSource ts)) throw new ArgumentException($"{GetType().Name}.Init: source type mismatch.");
                Blockly = b; source = ts;
                Initialize();
            }

            void ILogicNode.Evaluate()
            {
                T2 arg2 = Blockly.Pop<T2>();
                T1 arg1 = Blockly.Pop<T1>();
                InitializeProperties(_impl);
                TOutput result = _impl.Evaluate(arg1, arg2);
                Blockly.Push(result);
            }

            void IBlock.Destroy()
            {
                CleanProperties(_impl); OnDestroy();
                Blockly = null; source = null;
            }

            protected abstract void Initialize();
            protected abstract void InitializeProperties(TImpl impl);
            protected abstract void CleanProperties(TImpl impl);
            protected virtual void OnDestroy() { }
        }
    }

    /// <summary>
    /// 3 参数，无返回值（底层自动 Pop 参数）
    /// </summary>
    public abstract class Procedure<TImpl, T1, T2, T3> : Expression where TImpl : class, IProcedureImpl<T1, T2, T3>, new()
    {
        protected abstract class Block<TSource> : ILogicNode where TSource : Expression
        {
            private readonly TImpl _impl = new TImpl();
            public LogicGraph.Blockly Blockly { get; private set; }
            Blockly IBlock.scope => Blockly;
            public TSource source { get; private set; }

            void ILogicNode.Init(LogicGraph.Blockly b, Expression s)
            {
                if (!(s is TSource ts)) throw new ArgumentException($"{GetType().Name}.Init: source type mismatch.");
                Blockly = b; source = ts;
                Initialize();
            }

            void ILogicNode.Evaluate()
            {
                T3 arg3 = Blockly.Pop<T3>();
                T2 arg2 = Blockly.Pop<T2>();
                T1 arg1 = Blockly.Pop<T1>();
                InitializeProperties(_impl);
                _impl.Evaluate(arg1, arg2, arg3);
            }

            void IBlock.Destroy()
            {
                CleanProperties(_impl); OnDestroy();
                Blockly = null; source = null;
            }

            protected abstract void Initialize();
            protected abstract void InitializeProperties(TImpl impl);
            protected abstract void CleanProperties(TImpl impl);
            protected virtual void OnDestroy() { }
        }
    }

    /// <summary>
    /// 3 参数，有返回值（底层自动 Pop 参数 + Push 返回值）
    /// </summary>
    public abstract class Function<TImpl, T1, T2, T3, TOutput> : Expression where TImpl : class, IFunctionImpl<T1, T2, T3, TOutput>, new()
    {
        protected abstract class Block<TSource> : ILogicNode where TSource : Expression
        {
            private readonly TImpl _impl = new TImpl();
            public LogicGraph.Blockly Blockly { get; private set; }
            Blockly IBlock.scope => Blockly;
            public TSource source { get; private set; }

            void ILogicNode.Init(LogicGraph.Blockly b, Expression s)
            {
                if (!(s is TSource ts)) throw new ArgumentException($"{GetType().Name}.Init: source type mismatch.");
                Blockly = b; source = ts;
                Initialize();
            }

            void ILogicNode.Evaluate()
            {
                T3 arg3 = Blockly.Pop<T3>();
                T2 arg2 = Blockly.Pop<T2>();
                T1 arg1 = Blockly.Pop<T1>();
                InitializeProperties(_impl);
                TOutput result = _impl.Evaluate(arg1, arg2, arg3);
                Blockly.Push(result);
            }

            void IBlock.Destroy()
            {
                CleanProperties(_impl); OnDestroy();
                Blockly = null; source = null;
            }

            protected abstract void Initialize();
            protected abstract void InitializeProperties(TImpl impl);
            protected abstract void CleanProperties(TImpl impl);
            protected virtual void OnDestroy() { }
        }
    }

    /// <summary>
    /// 4 参数，无返回值（底层自动 Pop 参数）
    /// </summary>
    public abstract class Procedure<TImpl, T1, T2, T3, T4> : Expression where TImpl : class, IProcedureImpl<T1, T2, T3, T4>, new()
    {
        protected abstract class Block<TSource> : ILogicNode where TSource : Expression
        {
            private readonly TImpl _impl = new TImpl();
            public LogicGraph.Blockly Blockly { get; private set; }
            Blockly IBlock.scope => Blockly;
            public TSource source { get; private set; }

            void ILogicNode.Init(LogicGraph.Blockly b, Expression s)
            {
                if (!(s is TSource ts)) throw new ArgumentException($"{GetType().Name}.Init: source type mismatch.");
                Blockly = b; source = ts;
                Initialize();
            }

            void ILogicNode.Evaluate()
            {
                T4 arg4 = Blockly.Pop<T4>();
                T3 arg3 = Blockly.Pop<T3>();
                T2 arg2 = Blockly.Pop<T2>();
                T1 arg1 = Blockly.Pop<T1>();
                InitializeProperties(_impl);
                _impl.Evaluate(arg1, arg2, arg3, arg4);
            }

            void IBlock.Destroy()
            {
                CleanProperties(_impl); OnDestroy();
                Blockly = null; source = null;
            }

            protected abstract void Initialize();
            protected abstract void InitializeProperties(TImpl impl);
            protected abstract void CleanProperties(TImpl impl);
            protected virtual void OnDestroy() { }
        }
    }

    /// <summary>
    /// 4 参数，有返回值（底层自动 Pop 参数 + Push 返回值）
    /// </summary>
    public abstract class Function<TImpl, T1, T2, T3, T4, TOutput> : Expression where TImpl : class, IFunctionImpl<T1, T2, T3, T4, TOutput>, new()
    {
        protected abstract class Block<TSource> : ILogicNode where TSource : Expression
        {
            private readonly TImpl _impl = new TImpl();
            public LogicGraph.Blockly Blockly { get; private set; }
            Blockly IBlock.scope => Blockly;
            public TSource source { get; private set; }

            void ILogicNode.Init(LogicGraph.Blockly b, Expression s)
            {
                if (!(s is TSource ts)) throw new ArgumentException($"{GetType().Name}.Init: source type mismatch.");
                Blockly = b; source = ts;
                Initialize();
            }

            void ILogicNode.Evaluate()
            {
                T4 arg4 = Blockly.Pop<T4>();
                T3 arg3 = Blockly.Pop<T3>();
                T2 arg2 = Blockly.Pop<T2>();
                T1 arg1 = Blockly.Pop<T1>();
                InitializeProperties(_impl);
                TOutput result = _impl.Evaluate(arg1, arg2, arg3, arg4);
                Blockly.Push(result);
            }

            void IBlock.Destroy()
            {
                CleanProperties(_impl); OnDestroy();
                Blockly = null; source = null;
            }

            protected abstract void Initialize();
            protected abstract void InitializeProperties(TImpl impl);
            protected abstract void CleanProperties(TImpl impl);
            protected virtual void OnDestroy() { }
        }
    }

    /// <summary>
    /// 5 参数，无返回值（底层自动 Pop 参数）
    /// </summary>
    public abstract class Procedure<TImpl, T1, T2, T3, T4, T5> : Expression where TImpl : class, IProcedureImpl<T1, T2, T3, T4, T5>, new()
    {
        protected abstract class Block<TSource> : ILogicNode where TSource : Expression
        {
            private readonly TImpl _impl = new TImpl();
            public LogicGraph.Blockly Blockly { get; private set; }
            Blockly IBlock.scope => Blockly;
            public TSource source { get; private set; }

            void ILogicNode.Init(LogicGraph.Blockly b, Expression s)
            {
                if (!(s is TSource ts)) throw new ArgumentException($"{GetType().Name}.Init: source type mismatch.");
                Blockly = b; source = ts;
                Initialize();
            }

            void ILogicNode.Evaluate()
            {
                T5 arg5 = Blockly.Pop<T5>();
                T4 arg4 = Blockly.Pop<T4>();
                T3 arg3 = Blockly.Pop<T3>();
                T2 arg2 = Blockly.Pop<T2>();
                T1 arg1 = Blockly.Pop<T1>();
                InitializeProperties(_impl);
                _impl.Evaluate(arg1, arg2, arg3, arg4, arg5);
            }

            void IBlock.Destroy()
            {
                CleanProperties(_impl); OnDestroy();
                Blockly = null; source = null;
            }

            protected abstract void Initialize();
            protected abstract void InitializeProperties(TImpl impl);
            protected abstract void CleanProperties(TImpl impl);
            protected virtual void OnDestroy() { }
        }
    }

    /// <summary>
    /// 5 参数，有返回值（底层自动 Pop 参数 + Push 返回值）
    /// </summary>
    public abstract class Function<TImpl, T1, T2, T3, T4, T5, TOutput> : Expression where TImpl : class, IFunctionImpl<T1, T2, T3, T4, T5, TOutput>, new()
    {
        protected abstract class Block<TSource> : ILogicNode where TSource : Expression
        {
            private readonly TImpl _impl = new TImpl();
            public LogicGraph.Blockly Blockly { get; private set; }
            Blockly IBlock.scope => Blockly;
            public TSource source { get; private set; }

            void ILogicNode.Init(LogicGraph.Blockly b, Expression s)
            {
                if (!(s is TSource ts)) throw new ArgumentException($"{GetType().Name}.Init: source type mismatch.");
                Blockly = b; source = ts;
                Initialize();
            }

            void ILogicNode.Evaluate()
            {
                T5 arg5 = Blockly.Pop<T5>();
                T4 arg4 = Blockly.Pop<T4>();
                T3 arg3 = Blockly.Pop<T3>();
                T2 arg2 = Blockly.Pop<T2>();
                T1 arg1 = Blockly.Pop<T1>();
                InitializeProperties(_impl);
                TOutput result = _impl.Evaluate(arg1, arg2, arg3, arg4, arg5);
                Blockly.Push(result);
            }

            void IBlock.Destroy()
            {
                CleanProperties(_impl); OnDestroy();
                Blockly = null; source = null;
            }

            protected abstract void Initialize();
            protected abstract void InitializeProperties(TImpl impl);
            protected abstract void CleanProperties(TImpl impl);
            protected virtual void OnDestroy() { }
        }
    }

    #endregion

}
