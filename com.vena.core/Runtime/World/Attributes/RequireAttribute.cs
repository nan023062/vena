using System;

namespace Vena
{
    /// <summary>
    /// 用于主动定义组件的依赖关系
    /// </summary>
    [AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class RequireAttribute : Attribute
    {
        public readonly Type[] requireTypes;

        public RequireAttribute(params Type[] requireTypes)
        {
            this.requireTypes = requireTypes;
        }
    }
}