using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using syscall = go.syscall_package;

namespace GolibTests;

// A Go struct in NATIVE memory is read at Go's layout, or not at all. A converted struct's CLR layout is not always
// Go's: an array, pointer or string field is a managed reference (syscall's RawSockaddrInet4 carries Addr and Zero
// as array<byte>), and a value embed's storage is declared after the converter's fields (runtime's lfstack node,
// `struct{ LFNode; data int }`, guarded by runtime's own TestLFStack). So a FIELD of a native struct is a native box
// at the field's GO offset (ж.of), and the struct as a whole refuses by name when its layouts provably differ.
[TestClass]
public unsafe class NativeStructLayoutTests
{
    [TestMethod]
    public void AFieldOfANativeStructIsANativeBoxAtItsGoOffset()
    {
        byte* memory = (byte*)NativeMemory.AllocZeroed(16);

        try
        {
            *(ushort*)(memory + 0) = 2;
            *(ushort*)(memory + 2) = 0x1234;

            ж<syscall.RawSockaddrInet4> sa = (ж<syscall.RawSockaddrInet4>)(uintptr)(nuint)memory;
            ж<uint16> port = sa.of(syscall.RawSockaddrInet4.ᏑPort);

            Assert.IsTrue(port.IsNative, "the field view of a native struct is not native");
            Assert.AreEqual((nuint)(memory + 2), ((uintptr)port).Value, "Port is not viewed at its Go offset, 2");
            Assert.AreEqual((ushort)0x1234, port.Value, "Port does not read the bytes Go laid at offset 2");

            port.Value = 0x5678;
            Assert.AreEqual((ushort)0x5678, *(ushort*)(memory + 2), "a write through Port does not land at Go offset 2");
            Assert.AreEqual((ushort)2, sa.of(syscall.RawSockaddrInet4.ᏑFamily).Value, "Family does not read Go offset 0");
        }
        finally
        {
            NativeMemory.Free(memory);
        }
    }

    [TestMethod]
    public void ANativeStructWhoseLayoutDiffersRefusesByName()
    {
        byte* memory = (byte*)NativeMemory.AllocZeroed(16);

        try
        {
            ж<syscall.RawSockaddrInet4> sa = (ж<syscall.RawSockaddrInet4>)(uintptr)(nuint)memory;

            PanicException panic = Assert.ThrowsException<PanicException>(() => sa.Value.Family,
                "a whole read of a struct whose CLR layout is not Go's answered instead of refusing");

            StringAssert.StartsWith(panic.Message, "runtime error: cannot read native memory as ");
            StringAssert.Contains(panic.Message, "RawSockaddrInet4");
            StringAssert.Contains(panic.Message, "managed reference");
        }
        finally
        {
            NativeMemory.Free(memory);
        }
    }

    // Marshal.OffsetOf answers the MARSHALED layout, where a bool is a 4-byte BOOL: 0, 4, 8 for this struct, whose
    // managed layout is Go's (0, 1, 4). A bool-bearing struct must not be refused on that number.
    [TestMethod]
    public void ABoolBearingStructWhoseManagedLayoutIsGosIsNotRefused()
    {
        Assert.IsNull(GoReflect.ClrLayoutMismatch(typeof(LayoutTwoBoolsThenInt)), "a bool-bearing struct was refused on its marshaled layout");

        byte* memory = (byte*)NativeMemory.AllocZeroed(8);

        try
        {
            memory[1] = 1;
            *(int*)(memory + 4) = 7;

            ж<LayoutTwoBoolsThenInt> p = (ж<LayoutTwoBoolsThenInt>)(uintptr)(nuint)memory;

            Assert.IsFalse(p.Value.a, "a does not read Go offset 0");
            Assert.IsTrue(p.Value.b, "b does not read Go offset 1");
            Assert.AreEqual(7, p.Value.c, "c does not read Go offset 4");
        }
        finally
        {
            NativeMemory.Free(memory);
        }
    }

    [TestMethod]
    public void ANativeStructWhoseLayoutMatchesStillReadsWhole()
    {
        long* memory = (long*)NativeMemory.AllocZeroed(16);

        try
        {
            memory[0] = 7;
            memory[1] = 9;

            ж<syscall.Timespec> ts = (ж<syscall.Timespec>)(uintptr)(nuint)memory;

            Assert.AreEqual(7L, (long)ts.Value.Sec, "a matching layout no longer reads as a whole");
            Assert.AreEqual(9L, (long)ts.Value.Nsec, "a matching layout no longer reads as a whole");
        }
        finally
        {
            NativeMemory.Free(memory);
        }
    }
}

// Go: struct{ a, b bool; c int32 } at 0, 1, 4.
[GoType] internal partial struct LayoutTwoBoolsThenInt { public bool a; public bool b; public int c; }
