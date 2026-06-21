// -----------------------------------------------------------------------------
// Vena Blockly
// Visual scripting block graph runtime for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

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
