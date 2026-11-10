using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using go;
using go.golib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static go.builtin;

namespace GolibTests;

// cgo Phase 2 increment 3a (docs/phase4/DESIGN-cgo-phase2-pinvoke.md §5.2, read through §11.1 and §11.4): the argument
// rules a generated cgo wrapper consults, and the native-call bracket it wraps the call in.
//
// GoCgoArgs.StructPointer answers how C receives a pointer to a C struct. One whole struct in managed memory is
// mirrored, a genuinely native struct passes its address, and the two shapes the review found unsafe are refused
// before any native code runs: an ELEMENT of a Go array (cgo lets C touch the whole array, and a one-struct stack mirror
// would be smashed), and a native box minted from managed storage (its address is a managed address or a token, and C
// would write through it).
//
// The bracket arms read the goroutine's status and the seam's depth from INSIDE a native call, through libc's qsort
// calling back an [UnmanagedCallersOnly] comparator (§11.4's ruled route). qsort exists in every C library, so this runs
// on every golib-tests leg and on a host with no C compiler.
[TestClass]
public unsafe partial class GoCgoArgsTests
{
    private struct cgoPoint
    {
        public int32 X;
        public int32 Y;
    }

    private struct cgoHost
    {
        public cgoPoint Point;
    }

    private static readonly FieldRefFunc<cgoPoint> s_hostPoint = static p => ref ((ж<cgoHost>)p).Value.Point;

    private static string RefusalOf(Action call)
    {
        try
        {
            call();
        }
        catch (PanicException refused)
        {
            return refused.Message;
        }

        Assert.Fail("the call was not refused");
        return "";
    }

    [TestMethod]
    public void AWholeStructInManagedMemoryIsMirrored()
    {
        ж<cgoPoint> standard = new StandardBox<cgoPoint>(default(cgoPoint));
        Assert.AreEqual(GoCgoStructPointer.Mirror, GoCgoArgs.StructPointer(standard, "f", out nuint address));
        Assert.AreEqual((nuint)0, address, "a mirrored pointer has no address of its own to hand over");

        ж<cgoHost> host = new StandardBox<cgoHost>(default(cgoHost));
        ж<cgoPoint> field = new FieldRefBox<cgoPoint>(host, s_hostPoint);
        Assert.AreEqual(GoCgoStructPointer.Mirror, GoCgoArgs.StructPointer(field, "f", out _), "a struct field is one whole struct");

        Assert.AreEqual(GoCgoStructPointer.Nil, GoCgoArgs.StructPointer<cgoPoint>(null, "f", out _));
    }

    [TestMethod]
    public void AGenuinelyNativeStructPassesItsAddress()
    {
        void* memory = NativeMemory.AllocZeroed((nuint)sizeof(cgoPoint));

        try
        {
            ж<cgoPoint> native = new NativeBox<cgoPoint>((nuint)memory);
            Assert.AreEqual(GoCgoStructPointer.Native, GoCgoArgs.StructPointer(native, "f", out nuint address));
            Assert.AreEqual((nuint)memory, address);
        }
        finally
        {
            NativeMemory.Free(memory);
        }
    }

    // §11.1, the review's BLOCKER: C.f(&xs[0], C.int(len(xs))) is legal cgo, and C may write every element.
    [TestMethod]
    public void AnElementOfAGoArrayIsRefusedBeforeTheCall()
    {
        ж<cgoPoint> element = new ElemRefBox<cgoPoint>(new array<cgoPoint>(3), 1);
        string message = RefusalOf(() => GoCgoArgs.StructPointer(element, "fill", out _));

        StringAssert.Contains(message, "C.fill");
        StringAssert.Contains(message, "an element of a Go array or slice");
    }

    // §11.1, PROMOTED: a native box minted from managed storage carries a managed address or a token.
    [TestMethod]
    public void ANativeBoxMintedFromManagedStorageIsRefusedBeforeTheCall()
    {
        ж<cgoPoint> source = new StandardBox<cgoPoint>(default(cgoPoint));
        ж<cgoPoint> reinterpreted = new NativeBox<cgoPoint>(0x1000, retainedSource: source);
        ж<cgoPoint> token = new NativeBox<cgoPoint>(0x2000, aliasesAnOrderToken: true);

        foreach (ж<cgoPoint> minted in new[] { reinterpreted, token })
        {
            string message = RefusalOf(() => GoCgoArgs.StructPointer(minted, "fill", out _));

            StringAssert.Contains(message, "C.fill");
            StringAssert.Contains(message, "minted from managed storage");
        }
    }

    // §5.2's copy-in rule: a pointer member into Go memory is cgocheck's refusal, always on.
    [TestMethod]
    public void APointerMemberIntoGoMemoryPanicsWithCgosOwnMessage()
    {
        Assert.AreEqual((nuint)0, GoCgoArgs.PointerMember<cgoPoint>(null));

        void* memory = NativeMemory.AllocZeroed((nuint)sizeof(cgoPoint));

        try
        {
            Assert.AreEqual((nuint)memory, GoCgoArgs.PointerMember(new NativeBox<cgoPoint>((nuint)memory)));
        }
        finally
        {
            NativeMemory.Free(memory);
        }

        ж<cgoPoint> managed = new StandardBox<cgoPoint>(default(cgoPoint));
        StringAssert.Contains(RefusalOf(() => GoCgoArgs.PointerMember(managed)), "argument of cgo function has Go pointer to unpinned Go pointer");
    }

    // Copy-out keeps identity where C left a member alone, and mints a native pointer where C wrote one.
    [TestMethod]
    public void APointerMemberCopiedBackKeepsItsIdentityOrBecomesNative()
    {
        ж<cgoPoint> original = new NativeBox<cgoPoint>(0x3000);

        Assert.IsNull(GoCgoArgs.PointerMemberBack<cgoPoint>(0, original, 0x3000));
        Assert.AreSame(original, GoCgoArgs.PointerMemberBack(0x3000, original, 0x3000));

        ж<cgoPoint>? written = GoCgoArgs.PointerMemberBack(0x4000, original, 0x3000);
        Assert.IsInstanceOfType(written, typeof(NativeBox<cgoPoint>));
        Assert.AreEqual((nuint)0x4000, written!.NativeAddress);
    }

    // ---- The bracket, read from inside a native call (§11.4 as RULED: libc's qsort) ----

    private const uint Grunning = 2;

    [ThreadStatic]
    private static List<(uint status, int depth)>? t_insideReadings;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static int CompareReadingTheTransition(void* left, void* right)
    {
        (t_insideReadings ??= []).Add((runtime_package.GoCurrentGStatus(), GoNativeCall.TransitionDepth));
        return (*(int*)left).CompareTo(*(int*)right);
    }

    // The library differs per OS, and the binding is a test's own, so §11.11's resolver rule does not reach it.
    [LibraryImport("libc.so.6", EntryPoint = "qsort")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void QsortLinux(void* items, nuint count, nuint size, delegate* unmanaged[Cdecl]<void*, void*, int> compare);

    [LibraryImport("/usr/lib/libSystem.B.dylib", EntryPoint = "qsort")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void QsortDarwin(void* items, nuint count, nuint size, delegate* unmanaged[Cdecl]<void*, void*, int> compare);

    [LibraryImport("ucrtbase.dll", EntryPoint = "qsort")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void QsortWindows(void* items, nuint count, nuint size, delegate* unmanaged[Cdecl]<void*, void*, int> compare);

    private static void Qsort(int* items, nuint count)
    {
        delegate* unmanaged[Cdecl]<void*, void*, int> compare = &CompareReadingTheTransition;

        if (OperatingSystem.IsWindows())
            QsortWindows(items, count, sizeof(int), compare);
        else if (OperatingSystem.IsMacOS())
            QsortDarwin(items, count, sizeof(int), compare);
        else
            QsortLinux(items, count, sizeof(int), compare);
    }

    // Runs sort on a goroutine (the bracket is inert without one) and returns every reading the comparator took.
    private static List<(uint status, int depth)> ReadingsInside(Action<nint> sort)
    {
        List<(uint status, int depth)>? readings = null;
        int[] sorted = new int[2];
        Exception? thrown = null;
        using ManualResetEventSlim done = new();

        goǃ(() =>
        {
            try
            {
                int* items = stackalloc int[] { 2, 1 };
                t_insideReadings = [];
                sort((nint)items);
                readings = t_insideReadings;
                sorted[0] = items[0];
                sorted[1] = items[1];
            }
            catch (Exception ex)
            {
                thrown = ex;
            }
            finally
            {
                done.Set();
            }
        });

        Assert.IsTrue(done.Wait(TimeSpan.FromSeconds(30)), "the goroutine did not finish");
        Assert.IsNull(thrown, $"the call threw on a goroutine: {thrown}");
        CollectionAssert.AreEqual(new[] { 1, 2 }, sorted, "premise: qsort ran, calling the comparator");
        Assert.IsTrue(readings is { Count: >= 1 }, "premise: the comparator was called at least once");

        return readings!;
    }

    // The generated wrapper's bracket (§11.4): the runtime's entersyscall family moves the goroutine to _Gsyscall and
    // opens the seam exactly once, so every comparator call inside qsort reads both.
    [TestMethod]
    public void AWrappedCallRunsInsideGsyscallAndOneSeamTransition()
    {
        List<(uint status, int depth)> readings = ReadingsInside(items =>
        {
            GoNativeCall.Enter();

            try
            {
                Qsort((int*)items, 2);
            }
            finally
            {
                GoNativeCall.Exit();
            }
        });

        foreach ((uint status, int depth) in readings)
        {
            Assert.AreEqual(runtime_package.GoGSyscallStatus(), status, "inside the wrapped call the goroutine reads _Gsyscall, as Go's cgocall makes it");
            Assert.AreEqual(1, depth, "and the seam saw exactly one transition");
        }
    }
}
