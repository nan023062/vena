namespace Vena.Blockly
{

    public interface IBlocklyPool
    {
        T Get<T>() where T : class, new();
        void Return(object instance);
    }
}
