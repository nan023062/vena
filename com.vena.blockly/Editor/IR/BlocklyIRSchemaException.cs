using System;

namespace Vena.Blockly.Editor
{

    /// <summary>
    /// IR Schema 校验失败抛出（合约 §4.7）。
    /// §4.2–§4.6 任何不变量失败均经此异常上报；不静默修复。
    /// </summary>
    public sealed class BlocklyIRSchemaException : Exception
    {
        public BlocklyIRSchemaException(string message) : base(message) { }
        public BlocklyIRSchemaException(string message, Exception inner) : base(message, inner) { }
    }
}
