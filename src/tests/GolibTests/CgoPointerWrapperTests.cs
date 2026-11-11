using System;
using System.Runtime.InteropServices;
using go;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using C = go.cgotestꓸC_package;

namespace GolibTests;

// cgo Phase 2 increment 3b2b (docs/phase4/DESIGN-cgo-phase2-pinvoke.md §5.2, §7 as amended 2026-11-10, read through
// §11.1): pointer MEMBERS of C structs and pointers to ONE C scalar, from the same generated golden as 3b1 and 3b2a,
// compiled and RUN against the C library on a goroutine.
//
// A pointer member is checked as cgo checks it: nil crosses as NULL, a native pointer as its address, and a pointer into
// Go memory panics with cgo's own message before the call. A member C writes comes back as C memory, minted native. A
// pointer to one scalar crosses as a struct pointer does: copied in, handed over by address, copied back; a native one
// passes its address; an element of a Go array and a minted native box are refused before the call. And a pointer
// result the call was handed IS that pointer, as in Go, never a box over the wrapper's own stack copy.
[TestClass]
public unsafe class CgoPointerWrapperTests
{
    private struct exponentHost
    {
        public int32 Before;
        public C.@int Exponent;
    }

    private static T OnAGoroutine<T>(Func<T> call) => CgoScalarWrapperTests.OnAGoroutine(call);

    private static string RefusalOf(Action call)
    {
        string? message = OnAGoroutine(() =>
        {
            try
            {
                call();
            }
            catch (PanicException refused)
            {
                return refused.Message;
            }

            return null;
        });

        Assert.IsNotNull(message, "the call was not refused");
        return message;
    }

    // frexp(8, &e) is 0.5 with e = 4: the int crosses as a stack copy, by address, and is copied back.
    [TestMethod]
    public void APointerToOneScalarIsCopiedInAndBack()
    {
        (float64 fraction, int32 exponent) = OnAGoroutine(() =>
        {
            ж<C.@int> e = new StandardBox<C.@int>(default(C.@int));
            float64 f = C.frexp(8.0, e);
            return (f, (int32)e.Value);
        });

        Assert.AreEqual(0.5, fraction, "frexp(8) is 0.5 x 2^4");
        Assert.AreEqual(4, exponent, "and the exponent is copied back through the pointer");
    }

    // A pointer to a scalar FIELD is that field alone: cgo scopes the memory C may touch to it (§7, amended).
    [TestMethod]
    public void APointerToAScalarFieldIsCopiedBackIntoTheField()
    {
        (int32 before, int32 exponent) = OnAGoroutine(() =>
        {
            ж<exponentHost> host = new StandardBox<exponentHost>(new exponentHost { Before = 42 });
            ж<C.@int> field = new FieldRefBox<C.@int>(host, static p => ref ((ж<exponentHost>)p).Value.Exponent);
            C.frexp(8.0, field);
            return (host.Value.Before, (int32)host.Value.Exponent);
        });

        Assert.AreEqual(42, before, "the host's other field is untouched");
        Assert.AreEqual(4, exponent, "and the field holds the exponent");
    }

    [TestMethod]
    public void ANativeScalarPassesItsOwnAddress()
    {
        int* memory = (int*)NativeMemory.AllocZeroed(sizeof(int));

        try
        {
            OnAGoroutine(() => (float64)C.frexp(8.0, new NativeBox<C.@int>((nuint)memory)));
            Assert.AreEqual(4, *memory, "frexp wrote native memory through its own address");
        }
        finally
        {
            NativeMemory.Free(memory);
        }
    }

    // §7, amended: element pointers remain Phase 3, for scalars as for structs. &xs[1] is refused before the call.
    [TestMethod]
    public void AnElementOfAGoScalarArrayIsRefusedBeforeTheCall()
    {
        array<C.@int> exponents = new(3);
        string message = RefusalOf(() => C.frexp(8.0, new ElemRefBox<C.@int>(exponents, 1)));

        StringAssert.Contains(message, "C.frexp");
        StringAssert.Contains(message, "an element of a Go array or slice");
        Assert.AreEqual(0, (int32)exponents[1], "C never ran: the element is unwritten");
    }

    [TestMethod]
    public void ANativeScalarBoxMintedFromManagedStorageIsRefusedBeforeTheCall()
    {
        int* memory = (int*)NativeMemory.Alloc(sizeof(int));

        try
        {
            *memory = -77;
            ж<C.@int> source = new StandardBox<C.@int>(default(C.@int));
            string message = RefusalOf(() => C.frexp(8.0, new NativeBox<C.@int>((nuint)memory, retainedSource: source)));

            StringAssert.Contains(message, "C.frexp");
            StringAssert.Contains(message, "minted from managed storage");
            Assert.AreEqual(-77, *memory, "C never ran: the memory behind the box is unwritten");
        }
        finally
        {
            NativeMemory.Free(memory);
        }
    }

#if GOCGO_FLAVOUR_LINUX || GOCGO_FLAVOUR_DARWIN
    private static string CString(ж<C.@char> chars)
    {
        Assert.IsInstanceOfType(chars, typeof(NativeBox<C.@char>), "a member C wrote is C memory, minted native");
        return Marshal.PtrToStringAnsi((nint)chars.NativeAddress)!;
    }

    // gmtime_r(&t, &tm): a scalar pointer in, a struct pointer copied back with C's pointer MEMBER tm_zone in it, and the
    // result is the tm the call was handed, as Go's gmtime_r(&t, &tm) == &tm, not a box over the wrapper's stack copy.
    [TestMethod]
    public void GmtimeReturnsTheTmItWasHandedWithItsZoneMintedNative()
    {
        (bool same, int32 year, string zone) = OnAGoroutine(() =>
        {
            ж<C.@long> t = new StandardBox<C.@long>((C.@long)0L);
            ж<C.struct_tm> tm = new StandardBox<C.struct_tm>(default(C.struct_tm));
            ж<C.struct_tm> result = C.gmtime_r(t, tm);
            return (ReferenceEquals(result, tm), (int32)tm.Value.tm_year, CString(tm.Value.tm_zone));
        });

        Assert.IsTrue(same, "gmtime_r returns the pointer it was handed");
        Assert.AreEqual(70, year, "the epoch is 1970");
        Assert.IsTrue(zone is "GMT" or "UTC", $"tm_zone names the zone, as C wrote it ({zone})");
    }

    // §5.2's copy-in rule, cgocheck's own and always on: a pointer member into Go memory panics before the call.
    [TestMethod]
    public void AManagedPointerMemberPanicsWithCgosOwnMessageBeforeTheCall()
    {
        ж<C.struct_tm> tm = new StandardBox<C.struct_tm>(new C.struct_tm { tm_year = 999, tm_zone = new StandardBox<C.@char>((C.@char)(int8)'Z') });
        ж<C.@long> t = new StandardBox<C.@long>((C.@long)0L);

        string message = RefusalOf(() => C.gmtime_r(t, tm));

        StringAssert.Contains(message, "argument of cgo function has Go pointer to unpinned Go pointer");
        Assert.AreEqual(999, (int32)tm.Value.tm_year, "C never ran: the struct is unwritten");
    }
#else
    // _gmtime64_s(&tm, &t): a struct pointer and a pointer to one long long; windows' struct tm has no pointer member.
    [TestMethod]
    public void Gmtime64FillsTheTmThroughItsPointer()
    {
        (int32 result, int32 year) = OnAGoroutine(() =>
        {
            ж<C.struct_tm> tm = new StandardBox<C.struct_tm>(default(C.struct_tm));
            ж<C.@longlong> t = new StandardBox<C.@longlong>((C.@longlong)0L);
            int32 r = C._gmtime64_s(tm, t);
            return (r, (int32)tm.Value.tm_year);
        });

        Assert.AreEqual(0, result, "_gmtime64_s succeeds");
        Assert.AreEqual(70, year, "the epoch is 1970");
    }
#endif
}
