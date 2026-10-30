// LiteralSiteBench.Intercept.cs - seat #52, form F4 (plain only): C1 through a hand-written interceptor that takes
// the exact @string parameter and overwrites the argument with a static field. The call site still converts its
// C# literal to @string before the interceptor runs; this arm measures whether the substitution beats plain anyway.

#if !HOISTED
using System.Runtime.CompilerServices;

namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    file sealed class InterceptsLocationAttribute(string filePath, int line, int character) : Attribute;
}

namespace go
{
    using strings = strings_package;

    internal static partial class LiteralSiteBench
    {
        // C1 with its HasPrefix call intercepted (line 29, character 36 of this file: the `HasPrefix` name).
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static nint c1i(int n)
        {
            nint idx = 0, acc = 0;

            for (int i = 0; i < n; i++)
            {
                @string line = lines[idx];
                nint hit = strings.HasPrefix(line, "//go:build") ? 2 : 0;
                acc += hit;
                idx = (idx + 1 + hit) & 15;
            }

            return acc + idx;
        }
    }
}

namespace go2cs.Interceptors
{
    file static class Hand
    {
        private static readonly go.@string goBuildˢ = "//go:build"u8;

        [InterceptsLocation("LiteralSiteBench.Intercept.cs", 29, 36)]
        public static bool HasPrefix(go.@string s, go.@string prefix)
        {
            prefix = goBuildˢ;
            return go.strings_package.HasPrefix(s, prefix);
        }
    }
}
#endif
