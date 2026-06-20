namespace Vena.Blockly.Editor
{

    /// <summary>
    /// IR 编解码原语（Editor 顶层合约 §4.7）。
    /// 归属：Editor 子模块。Editor.UI 与 Runtime IR 加载器均消费。
    /// 不复用 Runtime <see cref="IBlocklySerializer"/>（字节流原语，输入域 / 输出域 / 变更原因均不同）。
    /// 不进父合约 §6 聚合门面（IBlocklyHost 不持有此接口）。
    /// </summary>
    public interface IBlocklyGraphSerializer
    {
        /// <summary>GraphIR → canonical JSON 串（合约 §4.5 不变量 2）。</summary>
        string ToJson(GraphIR ir);

        /// <summary>canonical / 非 canonical JSON 串 → GraphIR（合约 §4.2–§4.6 校验）。</summary>
        GraphIR FromJson(string json);
    }
}
