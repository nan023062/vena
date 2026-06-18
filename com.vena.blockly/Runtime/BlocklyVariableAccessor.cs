using System;

namespace Vena.Blockly
{

    /// <summary>
    /// 变量访问器扩展方法。
    /// 通过作用域链实现变量的读写：GetVariable 向上查找，SetVariable 写入当前作用域。
    /// </summary>
    public abstract partial class Blockly
    {
        public T GetVariable<T>(string name)
        {
            if (_variables != null && _variables.HasValue(name))
            {
                return _variables.GetValue<T>(name);
            }
            
            if (_parent != null)
            {
                return _parent.GetVariable<T>(name);
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
            
            return _parent != null && _parent.HasVariable(name);
        }

        public void ClearVariables()
        {
            _variables?.ClearValues();
        }

        /// <summary>
        /// 作用域变量存储，声明时检查整个作用域链是否重名
        /// </summary>
        sealed class ScopeVariables : IBlocklyVariableStorage
        {
            private readonly Blockly _scope;

            private readonly IBlocklyVariableStorage _inner;

            public ScopeVariables(Blockly scope)
            {
                _scope = scope;
                _inner = _scope.Host.VariableStorageFactory.Create(scope);
            }

            public void SetValue<T>(string name, T value)
            {
                if (!_inner.HasValue(name) && _scope._parent != null && _scope._parent.HasVariable(name))
                {
                    throw new InvalidOperationException(
                        $"Variable '{name}' already exists in a parent scope. Variable names must be unique across the entire scope chain.");
                }
                _inner.SetValue(name, value);
            }

            public void SetUValue(string name, IBoxedValue value)
            {
                if (!_inner.HasValue(name) && _scope._parent != null && _scope._parent.HasVariable(name))
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

            public System.Collections.Generic.IReadOnlyDictionary<string, IBoxedValue> GetAllVariables() => _inner.GetAllVariables();
        }
    }
}
