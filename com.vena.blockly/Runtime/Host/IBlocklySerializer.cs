namespace Vena.Blockly
{

    public interface IBlocklySerializer
    {
        int ReadInt32();
        void WriteInt32(int value);
        string ReadString();
        void WriteString(string value);
        bool ReadBoolean();
        void WriteBoolean(bool value);
    }
}
