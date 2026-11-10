using System;
using System.Runtime.InteropServices;
using go;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using C = go.cgotestꓸC_package;

namespace GolibTests;

// cgo Phase 2 increment 3b2a (docs/phase4/DESIGN-cgo-phase2-pinvoke.md §5.2, read through §11.1 and §11.7): the bindings
// of C functions that take and return C structs with no pointer members, from the same generated golden as 3b1's
// scalars (GoCgoWrappers.<os>.golden.cs, held equal to the generator in src/go2cs), compiled and RUN against the C
// library on a goroutine.
//
// A struct crosses BY VALUE as its blittable mirror, so these arms rest on the CLR classifying the mirror for the native
// ABI as the C compiler classifies the struct; each arm names the ABI rule it relies on. BY POINTER a whole managed
// struct crosses as a stack mirror, copied in, handed over by address and copied back; a native struct passes its own
// address; and the two shapes §11.1 refuses are refused BEFORE the call, which each refusal arm proves by holding the
// memory C would have written unchanged.
[TestClass]
public unsafe class CgoStructWrapperTests
{
    private struct utsHost
    {
        public int32 Before;
#if GOCGO_FLAVOUR_LINUX || GOCGO_FLAVOUR_DARWIN
        public C.struct_utsname Name;
#else
        public C.SYSTEMTIME Name;
#endif
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

    // div_t is 8 bytes of two ints. It comes back in RAX on win-x64 (a struct of 1, 2, 4 or 8 bytes is returned in
    // RAX), in RAX on SysV x64 (one eightbyte of class INTEGER) and in X0 on arm64 (a composite of 16 bytes or less is
    // returned in registers). A mismatch reads a garbage quotient, or faults.
    [TestMethod]
    public void AStructResultComesBackByValue()
    {
        (int32 quot, int32 rem) = OnAGoroutine(() =>
        {
            C.div_t r = C.div(7, 2);
            return ((int32)r.quot, (int32)r.rem);
        });

        Assert.AreEqual(3, quot, "div(7, 2).quot");
        Assert.AreEqual(1, rem, "div(7, 2).rem");

        (int32 negQuot, int32 negRem) = OnAGoroutine(() =>
        {
            C.div_t r = C.div(-7, 2);
            return ((int32)r.quot, (int32)r.rem);
        });

        Assert.AreEqual(-3, negQuot, "C truncates toward zero");
        Assert.AreEqual(-1, negRem, "and the remainder takes the dividend's sign");
    }

#if GOCGO_FLAVOUR_LINUX || GOCGO_FLAVOUR_DARWIN
    // struct in_addr is one 4-byte in_addr_t. It is passed in RDI on SysV x64 (one eightbyte of class INTEGER) and in X0
    // on arm64 (a composite of 16 bytes or less, in registers). inet_ntoa's char* result is C's static buffer, so the
    // wrapper mints it native.
    [TestMethod]
    public void AStructArgumentCrossesByValue()
    {
        string? text = OnAGoroutine(() =>
        {
            ж<C.@char> result = C.inet_ntoa(new C.struct_in_addr { s_addr = 0x0100007Fu });
            Assert.IsInstanceOfType(result, typeof(NativeBox<C.@char>), "C's memory, minted native");
            return Marshal.PtrToStringAnsi((nint)result.NativeAddress);
        });

        Assert.AreEqual("127.0.0.1", text, "127.0.0.1 is 0x7F000001 in network order: bytes 7F 00 00 01");
    }

    private static string CString(array<C.@char> chars)
    {
        System.Text.StringBuilder text = new();

        for (int i = 0; i < chars.Length && (int8)chars[i] != 0; i++)
            text.Append((char)(byte)(int8)chars[i]);

        return text.ToString();
    }

    private static string ExpectedSysname =>
#if GOCGO_FLAVOUR_LINUX
        "Linux";
#else
        "Darwin";
#endif

    // uname writes the whole struct through its pointer: the mirror is copied back into the Go struct, inline arrays
    // element by element.
    [TestMethod]
    public void AStructPointerIsMirroredAndCopiedBack()
    {
        (int32 result, string sysname) = OnAGoroutine(() =>
        {
            ж<C.struct_utsname> name = new StandardBox<C.struct_utsname>(new C.struct_utsname());
            int32 r = C.uname(name);
            return (r, CString(name.Value.sysname));
        });

        Assert.AreEqual(0, result, "uname succeeds");
        Assert.AreEqual(ExpectedSysname, sysname, "and its sysname is copied back");
    }

    // A struct FIELD is one whole struct: cgo makes the field's own bytes the memory C may touch.
    [TestMethod]
    public void AStructFieldIsMirroredLikeAWholeStruct()
    {
        (int32 before, string sysname) = OnAGoroutine(() =>
        {
            ж<utsHost> host = new StandardBox<utsHost>(new utsHost { Before = 42, Name = new C.struct_utsname() });
            ж<C.struct_utsname> field = new FieldRefBox<C.struct_utsname>(host, static p => ref ((ж<utsHost>)p).Value.Name);
            C.uname(field);
            return (host.Value.Before, CString(host.Value.Name.sysname));
        });

        Assert.AreEqual(42, before, "the host's other field is untouched");
        Assert.AreEqual(ExpectedSysname, sysname, "and the field is copied back");
    }

    // A genuinely native struct passes its own address: C writes native memory, and nothing is copied.
    [TestMethod]
    public void ANativeStructPassesItsOwnAddress()
    {
        nuint size = 5 * 256;
        byte* memory = (byte*)NativeMemory.AllocZeroed(size);

        try
        {
            int32 result = OnAGoroutine(() => (int32)C.uname(new NativeBox<C.struct_utsname>((nuint)memory)));

            Assert.AreEqual(0, result, "uname succeeds");
            Assert.AreEqual(ExpectedSysname, Marshal.PtrToStringAnsi((nint)memory), "sysname, at offset 0, written in native memory");
        }
        finally
        {
            NativeMemory.Free(memory);
        }
    }

#if GOCGO_FLAVOUR_LINUX
    // nil crosses as NULL; uname(NULL) is EFAULT on linux, a refusal by the kernel rather than a fault.
    [TestMethod]
    public void ANilStructPointerCrossesAsNull() =>
        Assert.AreEqual(-1, OnAGoroutine(() => (int32)C.uname(null!)), "uname(NULL) fails");
#endif

    // §11.1: an ELEMENT of a Go array is refused before the call (cgo lets C touch the whole array).
    [TestMethod]
    public void AnElementOfAGoArrayIsRefusedBeforeTheCall()
    {
        array<C.struct_utsname> names = new(3);

        for (int i = 0; i < 3; i++)
            names[i] = new C.struct_utsname();

        string message = RefusalOf(() => C.uname(new ElemRefBox<C.struct_utsname>(names, 1)));

        StringAssert.Contains(message, "C.uname");
        StringAssert.Contains(message, "an element of a Go array or slice");
        Assert.AreEqual("", CString(names[1].sysname), "C never ran: the element is unwritten");
    }

    // §11.1: a native box minted from managed storage is refused before the call (C must not write through it).
    [TestMethod]
    public void ANativeBoxMintedFromManagedStorageIsRefusedBeforeTheCall()
    {
        nuint size = 5 * 256;
        byte* memory = (byte*)NativeMemory.Alloc(size);

        try
        {
            NativeMemory.Fill(memory, size, 0xAA);
            ж<C.struct_utsname> source = new StandardBox<C.struct_utsname>(new C.struct_utsname());
            string message = RefusalOf(() => C.uname(new NativeBox<C.struct_utsname>((nuint)memory, retainedSource: source)));

            StringAssert.Contains(message, "C.uname");
            StringAssert.Contains(message, "minted from managed storage");
            Assert.AreEqual((byte)0xAA, memory[0], "C never ran: the memory behind the box is unwritten");
        }
        finally
        {
            NativeMemory.Free(memory);
        }
    }
#else
    private static C.RECT Rect(int32 left, int32 top, int32 right, int32 bottom) =>
        new() { left = left, top = top, right = right, bottom = bottom };

    // POINT is 8 bytes of two LONGs, passed in a register on win-x64 (a struct of 1, 2, 4 or 8 bytes is passed by
    // value in one; here RDX, PtInRect's second argument). RECT goes by pointer, mirrored.
    [TestMethod]
    public void AStructArgumentCrossesByValue()
    {
        (int32 inside, int32 outside, int32 right) = OnAGoroutine(() =>
        {
            ж<C.RECT> rect = new StandardBox<C.RECT>(Rect(0, 0, 10, 10));
            int32 a = C.PtInRect(rect, new C.POINT { x = 5, y = 5 });
            int32 b = C.PtInRect(rect, new C.POINT { x = 20, y = 20 });
            return (a, b, (int32)rect.Value.right);
        });

        Assert.AreNotEqual(0, inside, "{5,5} is in {0,0,10,10}");
        Assert.AreEqual(0, outside, "{20,20} is not");
        Assert.AreEqual(10, right, "the RECT is copied back unchanged");
    }

    // GetSystemTime writes the whole struct through its pointer: the mirror is copied back into the Go struct.
    [TestMethod]
    public void AStructPointerIsMirroredAndCopiedBack()
    {
        (int32 year, int32 month) = OnAGoroutine(() =>
        {
            ж<C.SYSTEMTIME> time = new StandardBox<C.SYSTEMTIME>(default(C.SYSTEMTIME));
            C.GetSystemTime(time);
            return ((int32)(uint16)time.Value.wYear, (int32)(uint16)time.Value.wMonth);
        });

        Assert.IsTrue(year >= 2026, $"the year is copied back ({year})");
        Assert.IsTrue(month is >= 1 and <= 12, $"and the month ({month})");
    }

    // A struct FIELD is one whole struct: cgo makes the field's own bytes the memory C may touch.
    [TestMethod]
    public void AStructFieldIsMirroredLikeAWholeStruct()
    {
        (int32 before, int32 year) = OnAGoroutine(() =>
        {
            ж<utsHost> host = new StandardBox<utsHost>(new utsHost { Before = 42 });
            ж<C.SYSTEMTIME> field = new FieldRefBox<C.SYSTEMTIME>(host, static p => ref ((ж<utsHost>)p).Value.Name);
            C.GetSystemTime(field);
            return (host.Value.Before, (int32)(uint16)host.Value.Name.wYear);
        });

        Assert.AreEqual(42, before, "the host's other field is untouched");
        Assert.IsTrue(year >= 2026, $"and the field is copied back ({year})");
    }

    // A genuinely native struct passes its own address: C writes native memory, and nothing is copied.
    [TestMethod]
    public void ANativeStructPassesItsOwnAddress()
    {
        ushort* memory = (ushort*)NativeMemory.AllocZeroed(16);

        try
        {
            OnAGoroutine(() => C.GetSystemTime(new NativeBox<C.SYSTEMTIME>((nuint)memory)));
            Assert.IsTrue(memory[0] >= 2026, $"wYear, at offset 0, written in native memory ({memory[0]})");
        }
        finally
        {
            NativeMemory.Free(memory);
        }
    }

    // Two pointers to one Go struct are one object to C: UnionRect(&r, &r, &o) writes the union into r and reads r
    // through the same mirror. Two mirrors would copy r's stale second mirror back over the union.
    [TestMethod]
    public void TwoPointersToOneStructShareOneMirror()
    {
        (int32 result, C.RECT union) = OnAGoroutine(() =>
        {
            ж<C.RECT> r = new StandardBox<C.RECT>(Rect(0, 0, 10, 10));
            ж<C.RECT> o = new StandardBox<C.RECT>(Rect(5, 5, 20, 20));
            int32 ok = C.UnionRect(r, r, o);
            return (ok, r.Value);
        });

        Assert.AreNotEqual(0, result, "UnionRect succeeds");
        Assert.AreEqual((0, 0, 20, 20), ((int32)union.left, (int32)union.top, (int32)union.right, (int32)union.bottom), "r is the union");
    }

    // §11.1: an ELEMENT of a Go array is refused before the call (cgo lets C touch the whole array).
    [TestMethod]
    public void AnElementOfAGoArrayIsRefusedBeforeTheCall()
    {
        array<C.SYSTEMTIME> times = new(3);
        string message = RefusalOf(() => C.GetSystemTime(new ElemRefBox<C.SYSTEMTIME>(times, 1)));

        StringAssert.Contains(message, "C.GetSystemTime");
        StringAssert.Contains(message, "an element of a Go array or slice");
        Assert.AreEqual(0, (int32)(uint16)times[1].wYear, "C never ran: the element is unwritten");
    }

    // §11.1: a native box minted from managed storage is refused before the call (C must not write through it).
    [TestMethod]
    public void ANativeBoxMintedFromManagedStorageIsRefusedBeforeTheCall()
    {
        byte* memory = (byte*)NativeMemory.Alloc(16);

        try
        {
            NativeMemory.Fill(memory, 16, 0xAA);
            ж<C.SYSTEMTIME> source = new StandardBox<C.SYSTEMTIME>(default(C.SYSTEMTIME));
            string message = RefusalOf(() => C.GetSystemTime(new NativeBox<C.SYSTEMTIME>((nuint)memory, retainedSource: source)));

            StringAssert.Contains(message, "C.GetSystemTime");
            StringAssert.Contains(message, "minted from managed storage");
            Assert.AreEqual((byte)0xAA, memory[0], "C never ran: the memory behind the box is unwritten");
        }
        finally
        {
            NativeMemory.Free(memory);
        }
    }
#endif
}
