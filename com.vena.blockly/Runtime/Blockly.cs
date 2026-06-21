// -----------------------------------------------------------------------------
// Vena Blockly
// Visual scripting block graph runtime for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

namespace Vena.Blockly
{

    /// <summary>
    /// 逻辑节点接口，所有逻辑节点（Expression、Procedure、Function）都实现该接口
    /// 行为节点接口, 所有行为节点（BehaviorGraph、BehaviorClip）都实现该接口
    /// </summary>
    public interface IBlock
    {
        Blockly scope { get; }
        
        void Destroy();
    }

    /// <summary>
    /// Blockly 作用域基类。类似于编程语言中的 { } —— 管理局部变量和节点实例的生命周期。
    /// 子作用域通过 parent 链向上访问父作用域变量，父作用域不能访问子作用域变量。
    /// 整个作用域链上变量名不能重名。
    /// 所有作用域都能实例化 Expression 节点（因为 Expression 是通用逻辑基元）。
    /// </summary>
    public abstract partial class Blockly
    {
        private IBlocklyHost _host;

        private Blockly _parent;

        private object _subject;

        private ScopeChain _scope;

        public IBlocklyHost Host
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _host ?? _parent?.Host;
        }

        public Blockly parent
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _parent;
        }

        public object Subject
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _subject ?? _parent?.Subject;
        }

        /// <summary>本作用域上的作用域链句柄。变量逻辑全部委托至此。</summary>
        public ScopeChain Scope => _scope ??= new ScopeChain(this);

        public IBlocklyVariableStorage Variables => Scope.Variables;

        public void Set(object subject, IBlocklyHost host = null)
        {
            if (_parent == null && host == null)
            {
                throw new InvalidOperationException("Root Blockly scope requires a non-null host.");
            }

            _subject = subject;
            _host = host;
        }

        public void SetParent(Blockly parent)
        {
            _parent = parent;
        }

        protected void ClearHostAndSubject()
        {
            _host = null;
            _subject = null;
        }
        
        #region Lifecycle

        public virtual void Destroy()
        {
            ClearVariables();
            _scope?.DropStorage();
            _scope = null;

            ClearAllInstances();
            _host = null;
            _subject = null;
            _parent = null;
        }

        #endregion

        #region ExceptionCapture

        private ExceptionDispatchInfo _raisedException;
        private bool _capturing;

        protected readonly struct ExceptionCapture : IDisposable
        {
            private readonly Blockly _owner;

            public ExceptionCapture(Blockly scope)
            {
                _owner = scope;

                if (_owner._capturing)
                {
                    throw new InvalidOperationException("Cannot start an ExceptionCapture while it is capturing.");
                }

                _owner._capturing = true;
            }

            public void Dispose()
            {
                _owner._capturing = false;

                var exception = _owner._raisedException;
                _owner._raisedException = null;
                exception?.Throw();
            }
        }

        internal void RaiseException(Exception e)
        {
            if (_raisedException == null)
            {
                _raisedException = ExceptionDispatchInfo.Capture(e);
            }
        }

        #endregion
    }
}
