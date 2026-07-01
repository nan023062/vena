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
    [BlocklySource("表达式图", typeof(ExpressionBlockly.Blockly))]
    public sealed class ExpressionBlockly : IBlocklySource
    {
        // 包心 plain 路径自动分配进程内单调递增 InstanceId（构造时即赋值）。
        // setter 保留 public：GraphLoader.TrySetInstanceId 通过反射用 IR Guid 折叠值覆盖（公共 BindingFlags）。
        public ulong InstanceId { get; set; } = InstanceIdAllocator.Next();

        [BlocklySourceSlot("根表达式", 1)]
        public Expression root;

        /// <summary>
        /// 统一的 Expression 作用域。类似于一个函数调用：
        /// - <see cref="Invoke"/>(...) 执行无返回值过程
        /// - <see cref="Call{TResult}"/>(...) 执行有返回值函数
        /// 参数直接传入、返回值直接获取。
        ///
        /// 值栈职责已下沉到 <see cref="Expression.Stack"/>（<c>[ThreadStatic]</c> 静态共享），
        /// 本类不再持有 <c>_valueStack</c> 字段、也不再暴露实例 <c>Push</c> / <c>Pop</c>。
        /// 入口 <see cref="Invoke"/> / <see cref="Call{TResult}"/> 用「深度快照 + finally 兜底」保证：
        /// 异常路径下栈回到调用前深度，不污染外层调用。
        /// </summary>
        public sealed class Blockly : Vena.Blockly.Blockly
        {
            private IExpressionBlock _entry;

            public void SetSource(ExpressionBlockly source)
            {
                if (source?.root == null) return;
                _entry = CreateBlock(source.root);
            }

            #region Invoke（无返回值 = Procedure）

            public void Invoke()
            {
                int depth = Expression.Stack.CurrentDepth();
                try
                {
                    _entry?.Evaluate();
                }
                finally
                {
                    while (Expression.Stack.CurrentDepth() > depth)
                    {
                        Expression.Stack.Pop().Dispose();
                    }
                }
            }

            public void Invoke<T1>(in T1 arg1)
            {
                int depth = Expression.Stack.CurrentDepth();
                try
                {
                    Expression.Stack.Push(arg1);
                    _entry?.Evaluate();
                }
                finally
                {
                    while (Expression.Stack.CurrentDepth() > depth)
                    {
                        Expression.Stack.Pop().Dispose();
                    }
                }
            }

            public void Invoke<T1, T2>(in T1 arg1, in T2 arg2)
            {
                int depth = Expression.Stack.CurrentDepth();
                try
                {
                    Expression.Stack.Push(arg1);
                    Expression.Stack.Push(arg2);
                    _entry?.Evaluate();
                }
                finally
                {
                    while (Expression.Stack.CurrentDepth() > depth)
                    {
                        Expression.Stack.Pop().Dispose();
                    }
                }
            }

            #endregion

            #region Call（有返回值 = Function）

            public TResult Call<TResult>()
            {
                int depth = Expression.Stack.CurrentDepth();
                try
                {
                    _entry?.Evaluate();
                    return Expression.Stack.Pop<TResult>();
                }
                finally
                {
                    while (Expression.Stack.CurrentDepth() > depth)
                    {
                        Expression.Stack.Pop().Dispose();
                    }
                }
            }

            public TResult Call<T1, TResult>(in T1 arg1)
            {
                int depth = Expression.Stack.CurrentDepth();
                try
                {
                    Expression.Stack.Push(arg1);
                    _entry?.Evaluate();
                    return Expression.Stack.Pop<TResult>();
                }
                finally
                {
                    while (Expression.Stack.CurrentDepth() > depth)
                    {
                        Expression.Stack.Pop().Dispose();
                    }
                }
            }

            public TResult Call<T1, T2, TResult>(in T1 arg1, in T2 arg2)
            {
                int depth = Expression.Stack.CurrentDepth();
                try
                {
                    Expression.Stack.Push(arg1);
                    Expression.Stack.Push(arg2);
                    _entry?.Evaluate();
                    return Expression.Stack.Pop<TResult>();
                }
                finally
                {
                    while (Expression.Stack.CurrentDepth() > depth)
                    {
                        Expression.Stack.Pop().Dispose();
                    }
                }
            }

            #endregion

            public override void Destroy()
            {
                DestroyBlock(_entry);
                _entry = null;
                base.Destroy();
            }

            #region Expression Block Create / Destroy

            public IExpressionBlock CreateBlock(Expression source)
            {
                var block = Host.NodeFactory.Create<IExpressionBlock>(source);
                block.Init(this, source);
                RegisterInstanceInternal(source.InstanceId, block);
                return block;
            }

            #endregion
        }
    }
}
