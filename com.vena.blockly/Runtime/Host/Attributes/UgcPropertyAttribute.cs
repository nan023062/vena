using System;

namespace Vena.Blockly
{

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class UgcPropertyAttribute : Attribute
    {
        public string DisplayName { get; }

        public UgcPropertyAttribute(string displayName)
        {
            DisplayName = displayName;
        }
    }
}
