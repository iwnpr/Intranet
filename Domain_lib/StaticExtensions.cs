using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace Domain_lib
{
    public static class StaticExtensions
    {
        public static string? GetCurrentMethodName([CallerMemberName] string? methodName = null)
        {
            return methodName;
        }
    }
}
