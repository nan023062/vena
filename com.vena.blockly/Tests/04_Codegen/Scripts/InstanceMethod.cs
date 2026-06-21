// -----------------------------------------------------------------------------
// Vena Blockly
// Visual scripting block graph runtime for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using Vena.Blockly;

namespace Vena.Blockly.Tests.Codegen
{

    /*
      TODO: 实例方法 代码生成示例

      源类只保留手写部分；Impl + Source + Source.Node 三件套由 Demo 04 的 codegen 流程
      在 Tests/04_Codegen/Generated/ 下生成（菜单：Tools/Vena/Blockly/Demo D Run Codegen）。
      若要重新观察生成产物，删除 Generated/ 目录后重跑菜单即可。
     */
    [UgcClass("测试对象")]
    public class InstanceMethod
    {
        [UgcMethod("加法", false, "参数1", "参数2")]
        public int TestMethod(int a, int b)
        {
            return a + b;
        }

        [UgcMethod("打印消息", false, "消息内容")]
        public void PrintMessage(string message)
        {
            System.Diagnostics.Debug.WriteLine($"InstanceMethod.PrintMessage: {message}");
        }
    }
}
