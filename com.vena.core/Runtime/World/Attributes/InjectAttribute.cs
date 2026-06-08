using System;

namespace Vena
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class InjectAttribute : Attribute
    {
    }
}