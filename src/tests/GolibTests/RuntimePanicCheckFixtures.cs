using System;
using System.Runtime.CompilerServices;
using go.golib;

namespace go;

// Fixtures for GolibTests.RuntimePanicCheckTests: classes named exactly as the converter names package
// runtime's internal-test bridge and package runtime_test, because golib's panicCheck1 marker reads a
// frame's declaring type by full name. Each Raise throws a real index panic from its own frame and
// hands back what was caught, so the test reads the trace a recover() would.

// Package runtime's own _test.go files, as a -tests build declares them.
internal static class runtime_internal_test_package
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static Exception Raise()
    {
        try
        {
            throw RuntimeErrorPanic.IndexOutOfRange(1L, 0L);
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    // Thrown INSIDE golib (the slice indexer's bounds check) on behalf of this frame, as converted runtime code's
    // `s[i]` is: the trace's first frame is golib's, which the walk must skip to reach this one.
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static Exception RaiseInGolib()
    {
        try
        {
            slice<nint> s = new nint[1].slice();
            nint i = 1;
            return new InvalidOperationException($"no panic: {s[i]}");
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    // Built here and handed back unthrown: whoever THROWS it owns the trace.
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static PanicException Build() => RuntimeErrorPanic.IndexOutOfRange(1L, 0L);

    internal static class Nested
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static Exception Raise()
        {
            try
            {
                throw RuntimeErrorPanic.IndexOutOfRange(1L, 0L);
            }
            catch (Exception ex)
            {
                return ex;
            }
        }
    }
}

// Package runtime_test: Go names its functions runtime_test.*, which panicCheck1's prefix excludes.
internal static class runtime_test_package
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static Exception Raise()
    {
        try
        {
            throw RuntimeErrorPanic.IndexOutOfRange(1L, 0L);
        }
        catch (Exception ex)
        {
            return ex;
        }
    }
}
