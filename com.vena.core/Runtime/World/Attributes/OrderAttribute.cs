using System;

namespace Vena
{
    /// <summary>
    /// The smaller the order value, the higher the priority
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class OrderAttribute : Attribute
    {
        public readonly int Order;

        public OrderAttribute(int order = 0)
        {
            Order = order;
        }
    }

    /// <summary>
    /// the order of the life cycle method is reversed
    /// such as OnDisable, OnDestroy
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
    public sealed class RevertOrderAttribute : Attribute
    {
    }
}