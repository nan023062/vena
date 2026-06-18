using System.Collections.Generic;

namespace Vena.Blockly
{

    public interface IBlocklyVariableStorage
    {
        void SetValue<T>(string name, T value);
        void SetUValue(string name, IBoxedValue value);
        T GetValue<T>(string name);
        void GetValue(string name, IBoxedValue output);
        bool HasValue(string name);
        void ClearValues();
        IReadOnlyDictionary<string, IBoxedValue> GetAllVariables();
    }
}
