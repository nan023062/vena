namespace Vena.Blockly
{

    /// <summary>
    /// 变量访问器：保留 Blockly 主类的变量公开 API（GetVariable / SetVariable / HasVariable / ClearVariables），
    /// 内部全部委托给 <see cref="ScopeChain"/> —— 业务方调用代码零修改。
    /// </summary>
    public abstract partial class Blockly
    {
        public T GetVariable<T>(string name) => Scope.GetVariable<T>(name);

        public void SetVariable<T>(string name, T value) => Scope.SetVariable(name, value);

        public bool HasVariable(string name) => Scope.HasVariable(name);

        public void ClearVariables() => _scope?.ClearVariables();
    }
}
