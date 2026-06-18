namespace Vena.Blockly
{

    public interface IBlocklySerializable
    {
        void Serialize(IBlocklySerializer writer);

        void Deserialize(IBlocklySerializer reader);
    }
}
