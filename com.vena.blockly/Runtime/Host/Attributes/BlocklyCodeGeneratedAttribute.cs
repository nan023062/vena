// -----------------------------------------------------------------------------
// Vena Blockly
// Visual scripting block graph runtime for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System;

namespace Vena.Blockly
{

    /// <summary>
    /// 标记 Blockly codegen 三件套产物（CodeGen 输入 → *Impl / *Source / *Source.Node）。
    /// 与 <see cref="BlocklyCodeGenAttribute"/> 同属 CodeGen 族，二者分别标注「输入面」与「产物面」；
    /// 重扫时识别为「可覆写」依据，运行期不依赖该注解语义。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class BlocklyCodeGeneratedAttribute : Attribute
    {
    }
}
