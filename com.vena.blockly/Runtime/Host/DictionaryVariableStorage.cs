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

    public sealed class DictionaryVariableStorage : IBlocklyVariableStorage
    {
        private readonly Dictionary<string, IBoxedValue> _boxed = new Dictionary<string, IBoxedValue>();
        private readonly Dictionary<string, object> _raw = new Dictionary<string, object>();

        public void SetValue<T>(string name, T value)
        {
            _raw[name] = value;
        }

        public void SetBoxedValue(string name, IBoxedValue value)
        {
            _boxed[name] = value;
        }

        public T GetValue<T>(string name)
        {
            if (_raw.TryGetValue(name, out var v) && v is T t)
            {
                return t;
            }
            return default;
        }

        public void GetValue(string name, IBoxedValue output)
        {
            throw new NotImplementedException();
        }

        public bool HasValue(string name)
        {
            return _raw.ContainsKey(name) || _boxed.ContainsKey(name);
        }

        public void ClearValues()
        {
            _raw.Clear();
            _boxed.Clear();
        }

        public IReadOnlyDictionary<string, IBoxedValue> GetAllVariables()
        {
            return _boxed;
        }
    }

    public sealed class DictionaryVariableStorageFactory : IBlocklyVariableStorageFactory
    {
        public IBlocklyVariableStorage Create(Blockly scope)
        {
            return new DictionaryVariableStorage();
        }
    }
}
