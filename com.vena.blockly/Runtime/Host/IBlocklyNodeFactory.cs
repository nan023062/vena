namespace Vena.Blockly
{

    public interface IBlocklyNodeFactory
    {
        T Create<T>(IBlocklySource source) where T : class;
    }
}
