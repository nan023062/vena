// -----------------------------------------------------------------------------
// Vena Blockly
// Visual scripting block graph runtime for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

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
