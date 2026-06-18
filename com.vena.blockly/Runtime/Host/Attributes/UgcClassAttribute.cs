using System;

namespace Vena.Blockly
{

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class UgcClassAttribute : Attribute
    {
        public string DisplayName { get; }

        public UgcClassAttribute(string displayName)
        {
            DisplayName = displayName;
        }
    }
}
