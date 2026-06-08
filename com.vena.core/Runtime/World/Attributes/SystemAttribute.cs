using System;

namespace Vena
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class AfterSystemAttribute : Attribute
    {
        public readonly Type SystemType;

        public AfterSystemAttribute(Type systemType)
        {
            SystemType = systemType;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class BeforeSystemAttribute : Attribute
    {
        public readonly Type SystemType;

        public BeforeSystemAttribute(Type systemType)
        {
            SystemType = systemType;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class SystemGroupAttribute : Attribute
    {
        public readonly Type GroupType;

        public SystemGroupAttribute(Type groupType)
        {
            GroupType = groupType;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class SystemOrderAttribute : Attribute
    {
        public readonly int Order;

        public SystemOrderAttribute(int order)
        {
            Order = order;
        }
    }
}