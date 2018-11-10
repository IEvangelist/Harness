using System.Diagnostics;
using System.Linq;
using System.Reflection;
using IEvangelist.Harness.Interfaces;
using Xunit;

namespace IEvangelist.Harness.Extensions
{
    internal static class HarnessExtensions
    {
        internal static (string className, string methodName) GetTestClassAndMethodNames(this IHarness harness)
        {
            MethodBase methodInfo = null;

            try
            {
                var st = new StackTrace();
                var frames = st.GetFrames();

                methodInfo =
                    frames?.Select(frame => frame.GetMethod())
                          .FirstOrDefault(method => method.IsFactOrTheoryTestMethod());
            }
            catch
            {
            }

            return methodInfo is null
                ? (null, null)
                : (methodInfo.DeclaringType?.Name, methodInfo.Name);
        }

        private static bool IsFactOrTheoryTestMethod(this MemberInfo method)
            => method.GetCustomAttribute<FactAttribute>() != null
            || method.GetCustomAttribute<TheoryAttribute>() != null;
    }
}