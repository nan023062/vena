namespace Vena.Blockly
{

    public interface IBlocklyNodeFactory
    {
        T Create<T>(IBlocklySource source) where T : class;

        /// <summary>
        /// per-host 一次性反射注册入口；多次调用幂等。
        /// 父合约 §6 解冻追加：实现方应在此扫描程序集 / 加载 <see cref="INodeMetadataProvider"/> 实现，
        /// 把 <see cref="UgcSourceAttribute"/> 标记的源类与其 NodeType 注册进工厂内部映射。
        /// </summary>
        void Initialize();
    }
}
