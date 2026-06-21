// -----------------------------------------------------------------------------
// Vena Blockly
// Visual scripting block graph runtime for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

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
