// -----------------------------------------------------------------------------
// Vena World
// Engine-agnostic world and actor runtime primitives for Vena.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System;

namespace Vena
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    internal sealed class AfterSystemAttribute : Attribute
    {
        internal readonly Type SystemType;

        internal AfterSystemAttribute(Type systemType)
        {
            SystemType = systemType;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    internal sealed class BeforeSystemAttribute : Attribute
    {
        internal readonly Type SystemType;

        internal BeforeSystemAttribute(Type systemType)
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
    internal sealed class SystemOrderAttribute : Attribute
    {
        internal readonly int Order;

        internal SystemOrderAttribute(int order)
        {
            Order = order;
        }
    }
}
