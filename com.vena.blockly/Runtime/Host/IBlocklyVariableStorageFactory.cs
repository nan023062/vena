namespace Vena.Blockly
{

    public interface IBlocklyVariableStorageFactory
    {
        IBlocklyVariableStorage Create(Blockly scope);
    }
}
