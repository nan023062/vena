// -----------------------------------------------------------------------------
// Vena Blockly
// Visual scripting block graph runtime for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using Vena.Blockly;

namespace Vena.Blockly.Tests.Codegen
{

    /// <summary>
    /// 实例方法 codegen 示例：源类只保留手写部分；Impl + Source + Source.Node 三件套
    /// 由 Demo 04 的 codegen 流程在 Tests/04_Codegen/Scripts/Generated/ 下生成。
    /// </summary>
    [Blockly("测试对象")]
    public class InstanceMethod
    {
        [Blockly("加法", false, "参数1", "参数2")]
        public int TestMethod(int a, int b)
        {
            return a + b;
        }

        [Blockly("打印消息", false, "消息内容")]
        public void PrintMessage(string message)
        {
            System.Diagnostics.Debug.WriteLine($"InstanceMethod.PrintMessage: {message}");
        }
    }
}
