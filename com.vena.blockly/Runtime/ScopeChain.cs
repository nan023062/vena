// -----------------------------------------------------------------------------
// Vena Blockly
// Visual scripting block graph runtime for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Vena.Blockly
{

    /// <summary>
    /// 作用域链：一个 Blockly 作用域上的变量逻辑。
    /// 内部持有：
    ///   - 当前作用域的变量后端（按需经 IBlocklyHost.VariableStorageFactory 创建）
    ///   - 父作用域的 ScopeChain 句柄（沿链向上查找 / 写穿透）
    /// 对外暴露的强类型变量 API（GetVariable&lt;T&gt; / SetVariable&lt;T&gt; / HasVariable / ClearVariables）
    /// 由 <see cref="Blockly"/> 委托调用，业务方调用代码零修改。
    /// 写语义：lexical resolve —— 命中已声明变量时写最近一层；全链未声明时在当前层创建。
    /// </summary>
    public sealed class ScopeChain
    {
        private readonly Blockly _scope;

        private IBlocklyVariableStorage _variables;

        public ScopeChain(Blockly scope)
        {
            _scope = scope ?? throw new ArgumentNullException(nameof(scope));
        }

        /// <summary>
        /// 当前作用域的变量后端。首次访问时按需创建。
        /// 暴露给 <see cref="Blockly.Variables"/> 用于既有外部读路径。
        /// </summary>
        public IBlocklyVariableStorage Variables => _variables ??= new ScopeVariables(this);

        /// <summary>变量后端是否已实例化（避免初始化期触发 Host 访问）。</summary>
        internal bool HasStorage => _variables != null;

        public T GetVariable<T>(string name)
        {
            if (_variables != null && _variables.HasValue(name))
            {
                return _variables.GetValue<T>(name);
            }

            var parentChain = ParentChain();
            if (parentChain != null)
            {
                return parentChain.GetVariable<T>(name);
            }

            return default;
        }

        public void SetVariable<T>(string name, T value)
        {
            ResolveWriteTarget(name).Variables.SetValue(name, value);
        }

        /// <summary>
        /// 解析 lexical write 目标作用域：
        ///   1. 当前层 storage 已存在该变量 → 写当前层；
        ///   2. 沿父链向上找最近一层 storage 命中 → 写那一层；
        ///   3. 全链均未声明 → 在当前层创建。
        /// </summary>
        private ScopeChain ResolveWriteTarget(string name)
        {
            if (_variables != null && _variables.HasValue(name))
            {
                return this;
            }

            var parent = ParentChain();
            while (parent != null)
            {
                if (parent._variables != null && parent._variables.HasValue(name))
                {
                    return parent;
                }
                parent = parent.ParentChain();
            }

            return this;
        }

        public bool HasVariable(string name)
        {
            if (_variables != null && _variables.HasValue(name))
            {
                return true;
            }

            var parentChain = ParentChain();
            return parentChain != null && parentChain.HasVariable(name);
        }

        public void ClearVariables()
        {
            _variables?.ClearValues();
        }

        /// <summary>
        /// 销毁本环上的变量后端。由 <see cref="Blockly.Destroy"/> 调用。
        /// </summary>
        internal void DropStorage()
        {
            _variables = null;
        }

        private ScopeChain ParentChain() => _scope.parent?.Scope;

        /// <summary>
        /// 作用域变量存储：单层后端的薄包装。
        /// 跨层 lexical resolve 由 <see cref="ScopeChain.ResolveWriteTarget"/> 完成；
        /// 这里只负责"写当前层 storage 或在当前层创建"。
        /// </summary>
        private sealed class ScopeVariables : IBlocklyVariableStorage
        {
            private readonly IBlocklyVariableStorage _inner;

            public ScopeVariables(ScopeChain chain)
            {
                _inner = chain._scope.Host.VariableStorageFactory.Create(chain._scope);
            }

            public void SetValue<T>(string name, T value) => _inner.SetValue(name, value);

            public void SetUValue(string name, IBoxedValue value) => _inner.SetUValue(name, value);

            public T GetValue<T>(string name) => _inner.GetValue<T>(name);

            public void GetValue(string name, IBoxedValue output) => _inner.GetValue(name, output);

            public bool HasValue(string name) => _inner.HasValue(name);

            public void ClearValues() => _inner.ClearValues();

            public IReadOnlyDictionary<string, IBoxedValue> GetAllVariables() => _inner.GetAllVariables();
        }
    }
}
