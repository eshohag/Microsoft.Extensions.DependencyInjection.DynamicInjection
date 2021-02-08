using System;

namespace Microsoft.Extensions.DependencyInjection.DynamicInjection
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, AllowMultiple = false)]
    public sealed class TransientServiceAttribute : Attribute
    {
    }
}
