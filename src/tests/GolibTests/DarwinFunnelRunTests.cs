using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;

namespace GolibTests;

/// <summary>
/// A pointer to an ELEMENT of a slice of reference-bearing records crossing the darwin libc funnel and the linux
/// keystone: the callee reads and writes the run from that element to the end of the backing, as in Go.
/// </summary>
/// <remarks>
/// <para>
/// darwin's TestGetfsstat killed its host (SIGSEGV on arm64, abort trap on x64; BOARD 2026-10-26, item (b)).
/// The generated Getfsstat passes <c>unsafe.Pointer(&amp;buf[0])</c> with <c>bufsize = 2168 * len(buf)</c>, and
/// Statfs_t's fixed arrays make the element an order token. The funnel encoded ONE record into a buffer of one
/// record's size, and libc wrote len(buf) records into it: a native heap overrun. The windows deep form already
/// copies the run (NativeStructMarshal.CallDeep, addressInRun); the shared marshal now does the same.
/// </para>
/// <para>
/// Red first, without crashing the host: before the fake writes anything it reads the SECOND record's marker,
/// which the run encode places one stride in. Before the fix that read lands past a one-record buffer and
/// cannot hold the marker, so the fake writes nothing and fails the call; nothing is written out of bounds.
/// </para>
/// </remarks>
[TestClass]
public unsafe class DarwinFunnelRunTests
{
    // Statfs_t's shape in miniature: scalars and a fixed array that makes the record reference-bearing.
    // Go layout: Bsize 0, Spare 8 (two int64) -> size 24.
    public struct StatfsLike
    {
        public long Bsize;
        public array<long> Spare;

        public StatfsLike() => Spare = new array<long>(2);
    }

    // Kevent_t's shape: a pointer field (Udata) beside scalars and a reference-bearing array, size 32.
    public struct KeventLike
    {
        public ulong Ident;
        public ж<byte> Udata;
        public array<long> Pad;

        public KeventLike() => Pad = new array<long>(2);
    }

    private const long Stride = 24;
    private const long Marker = 0x0123_4567_89AB_CDEF;

    private static nuint s_received;

    // getfsstat(buf, bufsize, flags): when the second record carries the caller's marker, the run reached libc, so
    // every record in bufsize is written (Bsize = 100 + i, Spare[1] = 0x5A00 + i) and the count is returned.
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nuint FakeGetfsstat(nuint buffer, nuint bufsize, nuint flags)
    {
        s_received = buffer;

        if (ManagedPointerTokens.IsTaggedToken(buffer))
            return unchecked((nuint)(-1));

        byte* p = (byte*)buffer;

        if (bufsize >= 2 * Stride && Unsafe.ReadUnaligned<long>(p + Stride) != Marker)
            return unchecked((nuint)(-1));

        long records = (long)bufsize / Stride;

        for (long i = 0; i < records; i++)
        {
            Unsafe.WriteUnaligned(p + i * Stride, 100 + i);
            Unsafe.WriteUnaligned(p + i * Stride + 16, 0x5A00 + i);
        }

        return (nuint)records;
    }

    private static nuint s_second;

    // Two pointers into one slice: the second is recorded beside the first, and the record it names is written.
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nuint FakeTwoPointers(nuint first, nuint second)
    {
        s_received = first;
        s_second = second;

        if (ManagedPointerTokens.IsTaggedToken(first) || ManagedPointerTokens.IsTaggedToken(second))
            return unchecked((nuint)(-1));

        Unsafe.WriteUnaligned((byte*)second, 0x7777L);
        return 0;
    }

    // kevent's event list: every record's Udata must read 0 (nil), and each Ident is written back as 0xE0 + i.
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nuint FakeKeventList(nuint events, nuint count)
    {
        s_received = events;

        if (ManagedPointerTokens.IsTaggedToken(events))
            return unchecked((nuint)(-1));

        byte* p = (byte*)events;

        for (nuint i = 0; i < count; i++)
        {
            if (Unsafe.ReadUnaligned<ulong>(p + i * 32 + 8) != 0)
                return unchecked((nuint)(-1));
        }

        for (nuint i = 0; i < count; i++)
            Unsafe.WriteUnaligned(p + i * 32, 0xE0UL + i);

        return count;
    }

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

    private static slice<StatfsLike> Records(int n)
    {
        slice<StatfsLike> buf = make<slice<StatfsLike>>(n);

        for (int i = 0; i < n; i++)
            buf[i] = new StatfsLike();

        buf[1].Bsize = Marker;
        return buf;
    }

    [TestMethod]
    public void AnElementPointerCarriesTheRunToTheEndOfTheBackingAndEveryRecordComesBack()
    {
        Assert.AreEqual((nuint?)Stride, NativeStructMarshal.MarshalledSizeOf(typeof(StatfsLike), nilPointersAsZero: true), "StatfsLike's Go size");

        slice<StatfsLike> buf = Records(3);
        nuint token = (nuint)(uintptr)Ꮡ(buf, 0);
        Assert.IsTrue(ManagedPointerTokens.IsTaggedToken(token), "&buf[0] must reach the funnel as a token, as Getfsstat's does");

        delegate* unmanaged[Cdecl]<nuint, nuint, nuint, nuint> fake = &FakeGetfsstat;
        nuint r = Call((nint)fake, token, (nuint)(Stride * len(buf)), 0);

        Assert.IsFalse(ManagedPointerTokens.IsTaggedToken(s_received), "libc must receive a native address, never the token");
        Assert.AreEqual((nuint)3, r, "libc read the second record's marker, so the whole run reached it");

        for (int i = 0; i < 3; i++)
        {
            Assert.AreEqual(100L + i, buf[i].Bsize, $"record {i}'s scalar comes back");
            Assert.AreEqual(0x5A00L + i, buf[i].Spare[1], $"record {i}'s array element comes back");
        }
    }

    [TestMethod]
    public void ARunStartsAtTheElementThePointerNames()
    {
        slice<StatfsLike> buf = Records(4);
        buf[0].Bsize = 0x0F0F;
        buf[2].Bsize = Marker;
        nuint token = (nuint)(uintptr)Ꮡ(buf, 1);

        delegate* unmanaged[Cdecl]<nuint, nuint, nuint, nuint> fake = &FakeGetfsstat;
        nuint r = Call((nint)fake, token, (nuint)(Stride * 3), 0);

        Assert.AreEqual((nuint)3, r, "&buf[1]'s run holds buf[1..4]: its second record is buf[2]");
        Assert.AreEqual(0x0F0FL, buf[0].Bsize, "the element before the run is not touched");

        for (int i = 1; i < 4; i++)
            Assert.AreEqual(100L + (i - 1), buf[i].Bsize, $"buf[{i}] comes back from the run");
    }

    [TestMethod]
    public void ASecondPointerInsideTheRunIsTheRunsOwnElement()
    {
        slice<StatfsLike> buf = Records(3);

        delegate* unmanaged[Cdecl]<nuint, nuint, nuint> fake = &FakeTwoPointers;
        nuint r = Call((nint)fake, (nuint)(uintptr)Ꮡ(buf, 0), (nuint)(uintptr)Ꮡ(buf, 2));

        Assert.AreEqual((nuint)0, r, "both pointers reach libc as native addresses");
        Assert.AreEqual(s_received + (nuint)(2 * Stride), s_second, "&buf[2] is the run's third native record, not a copy of its own");
        Assert.AreEqual(0x7777L, buf[2].Bsize, "the write through the second pointer comes back, not overwritten by a stale copy");
    }

    [TestMethod]
    public void ARunOfRecordsWithNilPointerFieldsCrosses()
    {
        slice<KeventLike> events = make<slice<KeventLike>>(3);

        for (int i = 0; i < 3; i++)
            events[i] = new KeventLike();

        delegate* unmanaged[Cdecl]<nuint, nuint, nuint> fake = &FakeKeventList;
        nuint r = Call((nint)fake, (nuint)(uintptr)Ꮡ(events, 0), 3);

        Assert.AreEqual((nuint)3, r, "every record's nil Udata reads 0");

        for (int i = 0; i < 3; i++)
            Assert.AreEqual(0xE0UL + (ulong)i, events[i].Ident, $"event {i} comes back");
    }

    [TestMethod]
    public void ANonNilPointerFieldAnywhereInTheRunKeepsTheToken()
    {
        slice<KeventLike> events = make<slice<KeventLike>>(3);

        for (int i = 0; i < 3; i++)
            events[i] = new KeventLike();

        heap((byte)1, out ж<byte> udata);
        events[2].Udata = udata;
        nuint token = (nuint)(uintptr)Ꮡ(events, 0);

        delegate* unmanaged[Cdecl]<nuint, nuint, nuint> fake = &FakeKeventList;
        Call((nint)fake, token, 3);

        Assert.AreEqual(token, s_received, "a non-nil Udata in the run leaves the token (EFAULT), as for one record");
        Assert.AreEqual((nuint)EFAULT, s_lastErrno);
        Assert.AreEqual(0UL, events[0].Ident, "nothing is decoded back when the run did not cross");
    }

    [TestMethod]
    public void TheLinuxKeystoneCarriesTheRunToo()
    {
        slice<StatfsLike> buf = Records(3);
        nuint token = (nuint)(uintptr)Ꮡ(buf, 0);
        nuint seen = 0;
        bool sawMarker = false;

        NativeStructMarshal.Call(new uintptr(1), new uintptr(token), new uintptr((nuint)(Stride * 3)), new uintptr(0), new uintptr(0), new uintptr(0), new uintptr(0),
            (num, a1, a2, a3, a4, a5, a6) =>
            {
                seen = (nuint)a1;

                if (ManagedPointerTokens.IsTaggedToken(seen))
                    return (new uintptr(0), new uintptr(0), new uintptr(14));

                byte* p = (byte*)(nuint)a1;
                sawMarker = Unsafe.ReadUnaligned<long>(p + Stride) == Marker;

                if (sawMarker)
                {
                    for (long i = 0; i < 3; i++)
                        Unsafe.WriteUnaligned(p + i * Stride, 200 + i);
                }

                return (new uintptr(3), new uintptr(0), new uintptr(0));
            });

        Assert.IsFalse(ManagedPointerTokens.IsTaggedToken(seen), "the keystone passes a native address");
        Assert.IsTrue(sawMarker, "the second record reached the callee");

        for (int i = 0; i < 3; i++)
            Assert.AreEqual(200L + i, buf[i].Bsize, $"record {i} comes back through the keystone");
    }
}
