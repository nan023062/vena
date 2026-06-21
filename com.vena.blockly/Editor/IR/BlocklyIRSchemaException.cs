using System;

namespace Vena.Blockly.Editor
{

    /// <summary>
    /// IR Schema 校验失败抛出。
    /// 任何 schema 不变量失败均经此异常上报；不静默修复。
    /// </summary>
    public sealed class BlocklyIRSchemaException : Exception
    {
        public BlocklyIRSchemaException(string message) : base(message) { }
        public BlocklyIRSchemaException(string message, Exception inner) : base(message, inner) { }
    }
}
