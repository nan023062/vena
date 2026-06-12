// -----------------------------------------------------------------------------
// Vena World
// Engine-agnostic world and actor runtime primitives for Vena.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System;

namespace Vena
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class InjectAttribute : Attribute
    {
    }
}