using System;

namespace Vena.Blockly
{

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class UgcSourcePropertyAttribute : Attribute
    {
        public string DisplayName { get; }

        public int Order { get; }

        public UgcSourcePropertyAttribute(string displayName, int order)
        {
            DisplayName = displayName;
            Order = order;
        }
    }
}
