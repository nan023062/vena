namespace Vena.Blockly
{

    public interface IBlocklyHost
    {
        IBlocklyLogger Logger { get; }
        IBlocklyNodeFactory NodeFactory { get; }
        IBlocklyPool Pool { get; }
        IBlocklySerializer Serializer { get; }
        IBlocklyVariableStorageFactory VariableStorageFactory { get; }
    }
}
