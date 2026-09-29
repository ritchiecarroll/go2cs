using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;
using static go.builtin;
using Δruntime = go.runtime_package;

namespace GolibTests;

// The slice-bounds seat's two golib follow-ups (COORD's review of claude/i9-slice-bounds-escape):
//
//   - ElementAddressUnchecked is unsafe.SliceData's `&s[:1][0]`, the underlying array's first element even
//     when len(s) is 0 and cap(s) is not. Its native-backed arm went through NativeElementAddress, which
//     checks the index against the LENGTH, so SliceData over an empty native window panicked where Go
//     answers. The checked element address `&s[0]` must still panic there: that is the control.
//   - A string slice expression out of range panicked with a plain STRING; Go panics with
//     runtime.boundsError (boundsSliceAlen for the high bound past the length, boundsSliceB for low past
//     high), which recover() sees as a runtime.Error. The TEXT was already Go's.
[TestClass]
public class SliceBoundsFollowupTests
{
    [ClassInitialize]
    public static void RunRuntimeModuleInitializer(TestContext _)
    {
        // runtime registers the boundsError hook in its module initializer (RuntimeErrorPanicValueTests).
        RuntimeHelpers.RunModuleConstructor(typeof(Δruntime).Module.ModuleHandle);
    }

    private static object? PanicValue(Action fn)
    {
        object? recovered = null;
        GoFrame frame = default;

        try
        {
            frame.Push(() => recovered = recover());
            fn();
        }
        catch (Exception ex) when (GoFrame.IsPanic(ex, out PanicException? p))
        {
            GoFrame.Capture(p);
        }
        finally
        {
            frame.Run();
        }

        return recovered;
    }

    private static void AssertRuntimeError(object? recovered, string want, string site)
    {
        Assert.IsNotNull(recovered, $"{site}: did not panic");

        Assert.IsTrue(recovered._<Δruntime.ΔError>(out Δruntime.ΔError? error),
            $"{site}: recovered value {recovered} (type {recovered.GetType().FullName}) does not implement runtime.Error");

        Assert.AreEqual(want, error!.Error().ToString(), $"{site}: runtime.Error text");
    }

    [TestMethod]
    public void SliceDataOfAnEmptyNativeWindowAddressesItsFirstElement()
    {
        const int capacity = 4;
        nuint addr = (nuint)(nint)Marshal.AllocHGlobal(capacity * sizeof(ulong));

        try
        {
            unsafe
            {
                ((ulong*)addr)[0] = 0xC0FFEE;
            }

            global::go.slice<ulong> empty = global::go.slice<ulong>.OverNativeMemory(addr, 0, capacity);

            ж<ulong> first = ElementAddressUnchecked(empty, 0);
            Assert.AreEqual(0xC0FFEEUL, first.Value, "SliceData's &s[:1][0] reads the window's first element");

            // Control: the CHECKED element address is Go's &s[0], which panics on a length-0 slice.
            Assert.IsNotNull(PanicValue(() => _ = Ꮡ(empty, 0)), "&s[0] on an empty slice must panic");
        }
        finally
        {
            Marshal.FreeHGlobal((nint)addr);
        }
    }

    [TestMethod]
    public void AStringSliceBoundPastTheLengthRecoversAsRuntimeError()
    {
        @string s = "abc";

        AssertRuntimeError(PanicValue(() => _ = s[0..5]), "runtime error: slice bounds out of range [:5] with length 3", "s[:5]");
    }

    [TestMethod]
    public void AStringSliceLowPastHighRecoversAsRuntimeError()
    {
        @string s = "abc";

        AssertRuntimeError(PanicValue(() => _ = s[2..1]), "runtime error: slice bounds out of range [2:1]", "s[2:1]");
    }
}
