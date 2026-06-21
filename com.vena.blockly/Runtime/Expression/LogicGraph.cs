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
    /// Expression 签名约束。标注在 ExpressionBlocklySource 字段上，声明该槽位期望的签名。
    /// 编辑器在连线/配置时读取此 Attribute 进行校验。
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ExpressionSignatureAttribute : Attribute
    {
        public Type ReturnType { get; }
        public Type[] ParameterTypes { get; }

        public ExpressionSignatureAttribute() { }

        public ExpressionSignatureAttribute(Type returnType)
        {
            ReturnType = returnType;
        }

        public ExpressionSignatureAttribute(Type returnType, params Type[] parameterTypes)
        {
            ReturnType = returnType;
            ParameterTypes = parameterTypes;
        }
    }

    /// <summary>
    /// Expression Blockly 源数据。对应可视化编辑器中的一个 Expression 图。
    /// 内含 Blockly 内部类作为运行时实例。
    /// </summary>
    [UgcSource("表达式图", typeof(LogicGraph.Blockly))]
    public sealed class LogicGraph : IBlocklySource
    {
        public ulong InstanceId { get; set; } = 0;

        [UgcSourceProperty("根表达式", 1)]
        public Expression root;

        /// <summary>
        /// 统一的 Expression 作用域。
        /// 类似于一个函数调用：
        /// - 调用 Invoke(...) 执行无返回值过程
        /// - 调用 Call&lt;T&gt;(...) 执行有返回值函数
        /// - 参数直接传入，返回值直接获取，栈帧对调用方透明
        /// </summary>
        public sealed class Blockly : Vena.Blockly.Blockly
        {
            private ILogicNode _entry;

            private Stack<IBoxedValue> _valueStack;

            private Stack<IBoxedValue> ValueStack => _valueStack ??= new Stack<IBoxedValue>();

            public void SetSource(LogicGraph source)
            {
                if (source?.root == null) return;
                _entry = CreateBlock(source.root);
            }

            #region 栈帧（Expression Blockly 专属）

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Push<T>(in T value)
            {
                ValueStack.Push(BoxedValue<T>.Create(value));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public T Pop<T>()
            {
                if (ValueStack.Count > 0)
                {
                    using var uValue = (BoxedValue<T>)ValueStack.Pop();
                    return uValue.value;
                }

    #if UNITY_EDITOR
                throw new Exception("Value stack is empty. No value to pop.");
    #else
                return default;
    #endif
            }

            private void ClearStack()
            {
                if (_valueStack != null)
                {
                    while (_valueStack.Count > 0)
                    {
                        _valueStack.Pop().Dispose();
                    }
                }
            }

            #endregion

            #region Invoke（无返回值 = Procedure）

            public void Invoke()
            {
                try { _entry?.Evaluate(); }
                finally { ClearStack(); }
            }

            public void Invoke<T1>(in T1 arg1)
            {
                try { Push(arg1); _entry?.Evaluate(); }
                finally { ClearStack(); }
            }

            public void Invoke<T1, T2>(in T1 arg1, in T2 arg2)
            {
                try { Push(arg1); Push(arg2); _entry?.Evaluate(); }
                finally { ClearStack(); }
            }

            #endregion

            #region Call（有返回值 = Function）

            public TResult Call<TResult>()
            {
                try { _entry?.Evaluate(); return Pop<TResult>(); }
                finally { ClearStack(); }
            }

            public TResult Call<T1, TResult>(in T1 arg1)
            {
                try { Push(arg1); _entry?.Evaluate(); return Pop<TResult>(); }
                finally { ClearStack(); }
            }

            public TResult Call<T1, T2, TResult>(in T1 arg1, in T2 arg2)
            {
                try { Push(arg1); Push(arg2); _entry?.Evaluate(); return Pop<TResult>(); }
                finally { ClearStack(); }
            }

            #endregion

            public override void Destroy()
            {
                ClearStack();
                DestroyBlock(_entry);
                _entry = null;
                base.Destroy();
            }

            #region Expression Block Create / Destroy

            public ILogicNode CreateBlock(Expression source)
            {
                var block = Host.NodeFactory.Create<ILogicNode>(source);
                block.Init(this, source);
                RegisterInstanceInternal(source.InstanceId, block);
                return block;
            }

            #endregion
        }
    }
}
