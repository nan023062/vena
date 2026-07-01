// -----------------------------------------------------------------------------
// Vena Blockly
// Visual scripting block graph runtime for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Vena.Blockly
{

    /// <summary>
    /// 逻辑节点源数据基类
    /// </summary>
    public abstract class Expression : IBlocklySource
    {
        // 包心 plain 路径自动分配进程内单调递增 InstanceId（构造时即赋值）。
        // setter 保留 public：GraphLoader.TrySetInstanceId 通过反射用 IR Guid 折叠值覆盖（公共 BindingFlags）。
        public ulong InstanceId { get; set; } = InstanceIdAllocator.Next();

        /// <summary>
        /// 值栈静态宿主（非泛型）。承载 <c>[ThreadStatic] Stack&lt;IBoxedValue&gt;</c>。
        /// 所有 <see cref="Block{TSource}"/> 派生 Push / Pop 通过转发方法（<see cref="Block{TSource}.Push{T}"/> 等）
        /// 落回该单一栈——若把栈挂在泛型 <c>Block&lt;TSource&gt;</c> 内，每个封闭泛型类型会各拥独立静态字段，
        /// 破坏「所有 Expression Node 共享调用栈」的契约。
        /// </summary>
        internal static class Stack
        {
            [ThreadStatic]
            private static System.Collections.Generic.Stack<IBoxedValue> _stack;
            
            private static System.Collections.Generic.Stack<IBoxedValue> S
                => _stack ??= new System.Collections.Generic.Stack<IBoxedValue>(8);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static void Push<T>(in T value) => S.Push(BoxedValue<T>.Create(value));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static void Push(in IBoxedValue value) => S.Push(value);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static T Pop<T>()
            {
                using var value = S.Pop();
                return ((BoxedValue<T>)value).value;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static IBoxedValue Pop() => S.Pop();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static int CurrentDepth() => S.Count;
        }

        /// <summary>
        /// Expression 运行时节点基类。<typeparamref name="TSource"/> = 具体 <see cref="Expression"/> 派生源类。
        /// Push / Pop 通过静态转发方法落到 <see cref="Stack"/>（非泛型宿主）保证「所有派生共享同一栈」。
        /// </summary>
        public abstract class Block<TSource> : ILogicNode where TSource : class, IBlocklySource
        {
            // Push / Pop / CurrentStackDepth / PopBoxed 语义 = 「派生 Block<TSource> 可直接继承调用、
            // 非派生的图外用户不可访问」；因此 protected internal（不是纯 internal —— tests / 用户
            // codegen 产物都在独立 asmdef，纯 internal 会阻断跨 assembly 的派生调用）。
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            protected internal static void Push<T>(in T value) => Expression.Stack.Push(value);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            protected internal static void Push(in IBoxedValue value) => Expression.Stack.Push(value);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            protected internal static T Pop<T>() => Expression.Stack.Pop<T>();
            
            public ExpressionBlockly.Blockly Blockly { get; private set; }

            Blockly IBlock.scope => Blockly;

            protected TSource source { get; private set; }

            void ILogicNode.Init(ExpressionBlockly.Blockly blockly, Expression s)
            {
                if (!(s is TSource ts))
                {
                    throw new ArgumentException($"{GetType().Name}.Init: source type mismatch (expected {typeof(TSource).Name}, got {s?.GetType().Name ?? "null"}).");
                }
                Blockly = blockly;
                source = ts;
                Initialize();
            }

            void ILogicNode.Evaluate() => Evaluate();

            void IBlock.Destroy()
            {
                OnDestroy();
                Blockly = null;
                source = null;
            }

            protected virtual void Initialize() { }

            public abstract void Evaluate();

            protected virtual void OnDestroy() { }
        }
    }

    /// <summary>
    /// 逻辑节点接口
    /// </summary>
    public interface ILogicNode : IBlock
    {
        void Init(ExpressionBlockly.Blockly blockly, Expression source);

        void Evaluate();
    }

}
