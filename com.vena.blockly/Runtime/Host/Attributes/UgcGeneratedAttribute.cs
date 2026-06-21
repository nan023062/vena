using System;

namespace Vena.Blockly
{

    /// <summary>
    /// 标记 codegen 三件套产物（*Impl / *Source / *Source.Node）。
    /// 重扫时识别为「可覆写」依据；运行期不依赖该注解语义。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class UgcGeneratedAttribute : Attribute
    {
    }
}
