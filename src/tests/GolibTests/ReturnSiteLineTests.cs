using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

// A Go-source fixture (namespace go, a *_package class) in the shape of internal/godebug's
// TestBisectTestCase: consecutive statements, each a call whose result is compared with a u8 literal
// and stored. AggressiveOptimization skips tier-0, so the method is jitted exactly as the Release
// TieredCompilation=0 default jits it.
namespace go
{
    internal static class returnsiteline_package
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static @string probe(nint[] got, nint[] want, int index, [CallerLineNumber] int line = 0)
        {
            (_, _, nint callerLine, bool ok) = runtime_package.Caller(1);
            got[index] = ok ? callerLine : -1;
            want[index] = line;
            return "0"u8;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static nint shape(nint[] got, nint[] want)
        {
            nint hits = 0;
            for (nint i = 0; i < 2; i++)
            {
                var a = probe(got, want, 0) == "1"u8;
                var b = probe(got, want, 1) == "1"u8;
                var c = probe(got, want, 2) == "1"u8;
                var d = probe(got, want, 3) == "1"u8;
                var e = probe(got, want, 4) == "1"u8;
                if (a) hits++;
                if (b) hits++;
                if (c) hits++;
                if (d && e) hits++;
            }
            return hits;
        }
    }
}

namespace GolibTests
{
    // A Go PC from runtime.Callers is a RETURN address, and Go resolves it at pc-1 -- the call
    // instruction -- so a frame's line is the line of the call it is suspended in. The CLR resolves a
    // non-leaf frame's return address to the IL offset of the last JIT mapping at or before it, and the
    // full-opt JIT keeps no per-statement boundaries: measured 2026-10-01 on internal/godebug at TC0,
    // every TestBisectTestCase frame read the PREVIOUS statement's call (IL 37, the compare call of
    // statement N-1, for a frame suspended in statement N's Value call at IL 45), so the line was one
    // early (have 145-147, want 146-148) while tier-0 read the right one.
    [TestClass]
    public class ReturnSiteLineTests
    {
        [TestMethod]
        public void ACallerFrameNamesTheLineOfTheCallItIsSuspendedIn()
        {
            nint[] got = new nint[5], want = new nint[5];
            _ = returnsiteline_package.shape(got, want);

            for (int i = 0; i < 5; i++)
                Assert.AreEqual(want[i], got[i], $"statement {i}: runtime.Caller(1) named line {got[i]}, the call is on line {want[i]} (got=[{string.Join(",", got)}] want=[{string.Join(",", want)}])");
        }
    }
}
