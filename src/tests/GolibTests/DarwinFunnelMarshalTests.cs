using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;

namespace GolibTests;

/// <summary>
/// The darwin libc funnel's token marshal (<see cref="GoLibcCall.Call"/>), driven host-neutrally.
/// </summary>
/// <remarks>
/// <para>
/// Every darwin syscall reaches libc through one funnel, syscall/darwin/syscall_darwin_impl.cs
/// <c>call</c> -> <see cref="GoLibcCall.Call"/>, and a pointer to a Go struct holding managed references
/// (Stat_t's Qspare, Timeval's padding, RawSockaddrAny's data) arrives there as an ORDER TOKEN: libc
/// received the token number and answered EFAULT. No fleet host runs darwin, so these arms hand the REAL
/// funnel entry a FAKE libc function -- an [UnmanagedCallersOnly] method whose address stands in for the
/// resolved libc symbol -- and read what it received. They run on windows and linux alike; the darwin
/// wiring itself is proven only by H7 darwin compiling it.
/// </para>
/// <para>
/// Red first: before the marshal, the fake received the token (the arms asserting a real address fail
/// on exactly that). The non-nil and string arms were green before and must stay green: COORD's option
/// (b) keeps the token (EFAULT) there rather than the linux keystone's panic.
/// </para>
/// </remarks>
[TestClass]
public unsafe class DarwinFunnelMarshalTests
{
    public struct Timespec
    {
        public long Sec;
        public long Nsec;
    }

    // Stat_t's shape in miniature: scalars, a nested struct, and a fixed array that makes it reference-bearing.
    // Go layout: Dev 0, Atime 8 (Sec 8, Nsec 16), Qspare 24 (two int64) -> size 40.
    public struct StatLike
    {
        public long Dev;
        public Timespec Atime;
        public array<long> Qspare;

        public StatLike() => Qspare = new array<long>(2);
    }

    // Kevent_t's shape: a pointer field (Udata) beside scalars and a reference-bearing array.
    // Go layout: Ident 0, Udata 8, Pad 16 (two int64) -> size 32.
    public struct KeventLike
    {
        public ulong Ident;
        public ж<byte> Udata;
        public array<long> Pad;

        public KeventLike() => Pad = new array<long>(2);
    }

    public struct Named
    {
        public long N;
        public @string S;
        public array<byte> B;

        public Named() => B = new array<byte>(2);
    }

    private static nuint s_received;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nuint FakeStat(nuint path, nuint buffer)
    {
        s_received = buffer;

        if (ManagedPointerTokens.IsTaggedToken(buffer))
            return unchecked((nuint)(-1));

        byte* p = (byte*)buffer;
        Unsafe.WriteUnaligned(p + 0, 0x1122L);
        Unsafe.WriteUnaligned(p + 8, 1_700_000_000L);
        Unsafe.WriteUnaligned(p + 16, 123_456_789L);
        Unsafe.WriteUnaligned(p + 32, 0x5A5AL);
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nuint FakeKevent(nuint events)
    {
        s_received = events;

        if (ManagedPointerTokens.IsTaggedToken(events))
            return unchecked((nuint)(-1));

        byte* p = (byte*)events;

        // The kernel reads a nil Udata as 0 and writes Ident back.
        if (Unsafe.ReadUnaligned<ulong>(p + 8) != 0)
            return 2;

        Unsafe.WriteUnaligned(p + 0, 0xBEEFUL);
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nuint FakeNine(nuint a1, nuint a2, nuint a3, nuint a4, nuint a5, nuint a6, nuint a7, nuint a8, nuint a9)
    {
        s_received = a9;

        if (ManagedPointerTokens.IsTaggedToken(a9))
            return unchecked((nuint)(-1));

        Unsafe.WriteUnaligned((byte*)a9, 0x0909L);
        return 0;
    }

    // darwin's __error stand-in: a native int holding EFAULT, what libc sets for a token address.
    private const int EFAULT = 14;
    private static readonly int* s_errno = initErrno();

    private static int* initErrno()
    {
        int* errno = (int*)NativeMemory.Alloc(sizeof(int));
        *errno = EFAULT;
        return errno;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static int* FakeErrorLocation() => s_errno;

    private static nuint s_lastErrno;

    private static nuint Call(nint fn, params ReadOnlySpan<nuint> args)
    {
        delegate* unmanaged[Cdecl]<int*> reader = &FakeErrorLocation;
        nuint r = GoLibcCall.Call(fn, args, GoLibcErrnoRule.Int32MinusOne, (nint)reader, out nuint errno);
        s_lastErrno = errno;
        return r;
    }

    [TestMethod]
    public void AReferenceBearingStructReachesLibcAsItsGoLayoutAndComesBackDecoded()
    {
        Assert.AreEqual((nuint?)40, NativeStructMarshal.MarshalledSizeOf(typeof(StatLike), nilPointersAsZero: true), "StatLike's Go size");

        ref StatLike stat = ref heap(new StatLike(), out ж<StatLike> box);
        nuint token = (nuint)(uintptr)box;
        Assert.IsTrue(ManagedPointerTokens.IsTaggedToken(token), "the fixture must reach the funnel as a token, as Stat_t does");

        delegate* unmanaged[Cdecl]<nuint, nuint, nuint> fake = &FakeStat;
        nuint r = Call((nint)fake, 0, token);

        Assert.IsFalse(ManagedPointerTokens.IsTaggedToken(s_received), "libc must receive a native address, never the token");
        Assert.AreEqual((nuint)0, r);
        Assert.AreEqual(0x1122L, stat.Dev);
        Assert.AreEqual(1_700_000_000L, stat.Atime.Sec);
        Assert.AreEqual(123_456_789L, stat.Atime.Nsec);
        Assert.AreEqual(0x5A5AL, stat.Qspare[1], "the array element libc wrote comes back through the box");
    }

    [TestMethod]
    public void ANilPointerFieldCrossesAsZero()
    {
        ref KeventLike ev = ref heap(new KeventLike(), out ж<KeventLike> box);
        ev.Ident = 7;

        delegate* unmanaged[Cdecl]<nuint, nuint> fake = &FakeKevent;
        nuint r = Call((nint)fake, (nuint)(uintptr)box);

        Assert.IsFalse(ManagedPointerTokens.IsTaggedToken(s_received), "a struct whose only pointer field is nil must cross");
        Assert.AreEqual((nuint)0, r, "libc must read the nil Udata as 0");
        Assert.AreEqual(0xBEEFUL, ev.Ident);
        Assert.IsNull(ev.Udata, "a nil pointer field stays nil");
    }

    [TestMethod]
    public void ANonNilPointerFieldKeepsTheTokenAndDoesNotPanic()
    {
        ref KeventLike ev = ref heap(new KeventLike(), out ж<KeventLike> box);
        heap((byte)1, out ж<byte> udata);
        ev.Udata = udata;
        nuint token = (nuint)(uintptr)box;

        delegate* unmanaged[Cdecl]<nuint, nuint> fake = &FakeKevent;
        nuint r = Call((nint)fake, token);

        Assert.AreEqual(token, s_received, "a non-nil pointer field leaves the token in place (EFAULT), never the linux panic");
        Assert.AreEqual(unchecked((nuint)(-1)), r);
        Assert.AreEqual((nuint)EFAULT, s_lastErrno, "the caller sees libc's EFAULT, as before the marshal");
    }

    [TestMethod]
    public void AStringFieldKeepsTheTokenAndDoesNotPanic()
    {
        heap(new Named(), out ж<Named> box);
        nuint token = (nuint)(uintptr)box;

        delegate* unmanaged[Cdecl]<nuint, nuint> fake = &FakeKevent;
        Call((nint)fake, token);

        Assert.AreEqual(token, s_received, "a string field has no native word: the token stays, and EFAULT");
        Assert.AreEqual((nuint)EFAULT, s_lastErrno);
    }

    [TestMethod]
    public void ATokenInTheNinthArgumentIsMarshalled()
    {
        ref StatLike stat = ref heap(new StatLike(), out ж<StatLike> box);

        delegate* unmanaged[Cdecl]<nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint> fake = &FakeNine;
        nuint r = Call((nint)fake, 1, 2, 3, 4, 5, 6, 7, 8, (nuint)(uintptr)box);

        Assert.IsFalse(ManagedPointerTokens.IsTaggedToken(s_received), "Syscall9's widest argument is marshalled too");
        Assert.AreEqual((nuint)0, r);
        Assert.AreEqual(0x0909L, stat.Dev);
    }
}
