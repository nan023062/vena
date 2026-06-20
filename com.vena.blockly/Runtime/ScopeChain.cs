using System;
using System.Collections.Generic;

namespace Vena.Blockly
{

    /// <summary>
    /// 作用域链：一个 Blockly 作用域上的变量逻辑。
    /// 内部持有：
    ///   - 当前作用域的变量后端（按需经 IBlocklyHost.VariableStorageFactory 创建）
    ///   - 父作用域的 ScopeChain 句柄（沿链向上查找 / 重名校验）
    /// 对外暴露的强类型变量 API（GetVariable&lt;T&gt; / SetVariable&lt;T&gt; / HasVariable / ClearVariables）
    /// 由 <see cref="Blockly"/> 委托调用，业务方调用代码零修改。
    /// 重名抛 <see cref="InvalidOperationException"/>（沿用 ScopeVariables 现状）。
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
            Variables.SetValue(name, value);
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
        /// 作用域变量存储：声明时检查整个作用域链是否重名。
        /// 写入路径包装实际后端（来自 Host.VariableStorageFactory），读取路径直接转发。
        /// </summary>
        private sealed class ScopeVariables : IBlocklyVariableStorage
        {
            private readonly ScopeChain _chain;

            private readonly IBlocklyVariableStorage _inner;

            public ScopeVariables(ScopeChain chain)
            {
                _chain = chain;
                _inner = _chain._scope.Host.VariableStorageFactory.Create(_chain._scope);
            }

            public void SetValue<T>(string name, T value)
            {
                if (!_inner.HasValue(name) && ParentHasVariable(name))
                {
                    throw new InvalidOperationException(
                        $"Variable '{name}' already exists in a parent scope. Variable names must be unique across the entire scope chain.");
                }
                _inner.SetValue(name, value);
            }

            public void SetUValue(string name, IBoxedValue value)
            {
                if (!_inner.HasValue(name) && ParentHasVariable(name))
                {
                    throw new InvalidOperationException(
                        $"Variable '{name}' already exists in a parent scope. Variable names must be unique across the entire scope chain.");
                }
                _inner.SetUValue(name, value);
            }

            public T GetValue<T>(string name) => _inner.GetValue<T>(name);

            public void GetValue(string name, IBoxedValue output) => _inner.GetValue(name, output);

            public bool HasValue(string name) => _inner.HasValue(name);

            public void ClearValues() => _inner.ClearValues();

            public IReadOnlyDictionary<string, IBoxedValue> GetAllVariables() => _inner.GetAllVariables();

            private bool ParentHasVariable(string name)
            {
                var parent = _chain.ParentChain();
                return parent != null && parent.HasVariable(name);
            }
        }
    }
}
