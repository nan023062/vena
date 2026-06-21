// -----------------------------------------------------------------------------
// Vena Blockly
// Visual scripting block graph runtime for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System;

namespace Vena.Blockly
{

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class UgcMethodAttribute : Attribute
    {
        public string DisplayName { get; }

        public bool IsStatic { get; }

        public string[] ParameterNames { get; }

        public UgcMethodAttribute(string displayName, bool isStatic, params string[] parameterNames)
        {
            DisplayName = displayName;
            IsStatic = isStatic;
            ParameterNames = parameterNames;
        }
    }
}
