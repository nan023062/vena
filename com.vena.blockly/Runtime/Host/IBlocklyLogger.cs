namespace Vena.Blockly
{

    public interface IBlocklyLogger
    {
        void Debug(string message);
        void Warning(string message);
        void Error(string message);
    }
}
