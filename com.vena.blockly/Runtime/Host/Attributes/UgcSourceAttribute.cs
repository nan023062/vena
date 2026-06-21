// -----------------------------------------------------------------------------
// Vena Blockly
// Visual scripting block graph runtime for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System;

namespace Vena.Blockly
{

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class UgcSourceAttribute : Attribute
    {
        public string MenuPath { get; }

        public Type ImplementationType { get; }

        public UgcSourceAttribute(string menuPath, Type implementationType)
        {
            MenuPath = menuPath;
            ImplementationType = implementationType;
        }

        public static Type GetObjectType(Type sourceType)
        {
            var attr = (UgcSourceAttribute)Attribute.GetCustomAttribute(sourceType, typeof(UgcSourceAttribute));
            return attr?.ImplementationType;
        }
    }
}
