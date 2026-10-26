// DarwinPointerArgumentMirrorTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;
using @unsafe = go.unsafe_package;

namespace GolibTests;

/// <summary>
/// The two darwin libc arguments syscall's writev and Getdirentries now build natively
/// (syscall/darwin/writev_darwin_impl.cs, getdirentries_darwin_impl.cs), driven host-neutrally.
/// </summary>
/// <remarks>
/// <para>
/// Each test calls a FAKE libc function (an [UnmanagedCallersOnly] method standing in for the
/// resolved libSystem symbol, as in <see cref="DarwinFunnelMarshalTests"/>) through the same funnel
/// the darwin trampolines use, <see cref="GoLibcCall.Call"/>, twice: once with the argument the
/// GENERATED wrapper built, which must reach libc as the order token that failed on the mac legs, and
/// once with the argument the mirror builds, which must reach libc as real addresses carrying the
/// caller's own data. No fleet host runs darwin: the mirrors themselves are compiled by a
/// GoTargetOS=darwin build and run by the os-matrix mac legs (net's TestBuffers_WriteTo, syscall's
/// TestDirent, Behavioral WritevIovecSeam).
/// </para>
/// </remarks>
[TestClass]
public unsafe class DarwinPointerArgumentMirrorTests
{
    // syscall.Iovec on darwin/amd64: Base *byte, Len uint64. Base is a ж<byte>, so the element is
    // reference-bearing and &iovecs[0] is an order token.
    public struct IovecLike
    {
        public ж<byte> Base;
        public ulong Len;
    }

    // syscall.Dirent on darwin/amd64: Ino 0, Seekoff 8, Reclen 16, Namlen 18, Type 20, Name 21
    // ([1024]int8), Pad_cgo_0 1045 ([3]byte) -> 1048.
    public struct DirentLike
    {
        public ulong Ino;
        public ulong Seekoff;
        public ushort Reclen;
        public ushort Namlen;
        public byte Type;
        public array<sbyte> Name;
        public array<byte> Pad_cgo_0;

        public DirentLike()
        {
            Name = new array<sbyte>(1024);
            Pad_cgo_0 = new array<byte>(3);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeIovec
    {
        public byte* Base;
        public nuint Len;
    }

    private const int Chunks = 10;
    private const int NativeDirentSize = 1048;
    private const ushort RecordLength = 32;
    private static readonly nuint EFAULT = unchecked((nuint)(-14));

    private static readonly byte[] s_written = new byte[Chunks];
    private static int s_writtenLength;
    private static nuint s_receivedSlot;

    // ssize_t writev(fd, iov, iovcnt): EFAULT for an address the kernel's copyin would refuse,
    // otherwise gathers every element's bytes, as the kernel does.
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nuint FakeWritev(nuint fd, nuint iov, nuint count)
    {
        s_writtenLength = 0;

        if (iov == 0 || ManagedPointerTokens.IsTaggedToken(iov))
            return EFAULT;

        NativeIovec* vector = (NativeIovec*)iov;

        for (nuint i = 0; i < count; i++)
        {
            if (vector[i].Base == null || ManagedPointerTokens.IsTaggedToken((nuint)vector[i].Base))
                return EFAULT;

            for (nuint j = 0; j < vector[i].Len; j++)
                s_written[s_writtenLength++] = vector[i].Base[j];
        }

        return (nuint)s_writtenLength;
    }

    // int readdir_r(DIR*, struct dirent *entry, struct dirent **result): fills the record and
    // stores *result = entry. libSystem does that store in userspace, so a token here is the SIGBUS
    // the mac legs died of; the fake refuses it with a distinct value instead of faulting.
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nuint FakeReaddirR(nuint dir, nuint entry, nuint result)
    {
        s_receivedSlot = result;

        if (ManagedPointerTokens.IsTaggedToken(entry) || ManagedPointerTokens.IsTaggedToken(result))
            return 0xB05; // the store a real libc would fault on

        byte* record = (byte*)entry;
        new Span<byte>(record, NativeDirentSize).Clear();
        Unsafe.WriteUnaligned(record + 0, 0x1234UL);
        Unsafe.WriteUnaligned(record + 16, RecordLength);
        Unsafe.WriteUnaligned(record + 18, (ushort)3);
        record[20] = 8; // DT_REG
        record[21] = (byte)'a';
        record[22] = (byte)'b';
        record[23] = (byte)'c';
        *(nuint*)result = entry;
        return 0;
    }

    private static (slice<IovecLike> iovecs, slice<byte>[] chunks) TenOneByteChunks()
    {
        // WritevIovecSeam's shape: ten one-byte buffers carrying 0x00..0x09, so a layout fault shows
        // as wrong CONTENT at the right length.
        slice<IovecLike> iovecs = new slice<IovecLike>(Chunks);
        slice<byte>[] chunks = new slice<byte>[Chunks];

        for (int i = 0; i < Chunks; i++)
        {
            chunks[i] = new slice<byte>(1);
            chunks[i][0] = (byte)i;
            iovecs[i] = new IovecLike { Base = Ꮡ(chunks[i], 0), Len = 1 };
        }

        return (iovecs, chunks);
    }

    [TestMethod]
    public void TheGeneratedWritevArgumentReachesLibcAsAToken()
    {
        (slice<IovecLike> iovecs, slice<byte>[] chunks) = TenOneByteChunks();

        // zsyscall_darwin_amd64.go: _p0 = unsafe.Pointer(&iovecs[0]).
        nuint generated = (nuint)(uintptr)@unsafe.Pointer.FromPinnedBox(Ꮡ(iovecs, 0));

        delegate* unmanaged[Cdecl]<nuint, nuint, nuint, nuint> fake = &FakeWritev;
        nuint r = GoLibcCall.Call((nint)fake, [3, generated, Chunks], GoLibcErrnoRule.None, 0, out _);
        GC.KeepAlive(chunks);

        Assert.IsTrue(ManagedPointerTokens.IsTaggedToken(generated), "an Iovec holds a pointer, so &iovecs[0] is an order token");
        Assert.AreEqual(EFAULT, r, "the funnel leaves a token whose pointee holds a non-nil pointer: the mac legs' EFAULT");
    }

    [TestMethod]
    public void TheMirroredWritevVectorCarriesEveryBaseAsAPinnedAddress()
    {
        (slice<IovecLike> iovecs, slice<byte>[] chunks) = TenOneByteChunks();

        // writev_darwin_impl.cs: one NativeIovec per element, each Base the pinned address of the
        // caller's buffer for as long as its box is reachable.
        NativeIovec* native = stackalloc NativeIovec[Chunks];

        for (int i = 0; i < Chunks; i++)
        {
            native[i].Base = (byte*)(nint)(uintptr)iovecs[i].Base;
            native[i].Len = (nuint)iovecs[i].Len;
        }

        // The pins must survive a collection between building the vector and the call.
        GC.Collect();
        GC.WaitForPendingFinalizers();

        delegate* unmanaged[Cdecl]<nuint, nuint, nuint, nuint> fake = &FakeWritev;
        nuint r = GoLibcCall.Call((nint)fake, [3, (nuint)native, Chunks], GoLibcErrnoRule.None, 0, out _);
        GC.KeepAlive(iovecs);
        GC.KeepAlive(chunks);

        Assert.AreEqual((nuint)Chunks, r, "every element's byte is written");
        CollectionAssert.AreEqual(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, s_written, "the bytes libc gathers are the caller's, in order");
    }

    [TestMethod]
    public void TheGeneratedReaddirResultSlotReachesLibcAsAToken()
    {
        Assert.AreEqual((nuint?)NativeDirentSize, NativeStructMarshal.MarshalledSizeOf(typeof(DirentLike), nilPointersAsZero: true), "the entry itself marshals: 1048 bytes, Go's layout");
        Assert.IsNull(NativeStructMarshal.MarshalledSizeOf(typeof(ж<DirentLike>), nilPointersAsZero: true), "a pointer pointee is not something the marshaller classifies");

        // syscall_darwin.go: readdir_r(d, &entry, &entryp).
        heap(new DirentLike(), out ж<DirentLike> Ꮡentry);
        heap<ж<DirentLike>>(out ж<ж<DirentLike>> Ꮡentryp);

        delegate* unmanaged[Cdecl]<nuint, nuint, nuint, nuint> fake = &FakeReaddirR;
        nuint r = GoLibcCall.Call((nint)fake, [7, (nuint)(uintptr)Ꮡentry, (nuint)(uintptr)Ꮡentryp], GoLibcErrnoRule.None, 0, out _);

        Assert.IsTrue(ManagedPointerTokens.IsTaggedToken(s_receivedSlot), "&entryp reaches libc as the raw token");
        Assert.AreEqual((nuint)0xB05, r, "libc's *result = entry store would go through the token: the mac legs' SIGBUS");
    }

    [TestMethod]
    public void TheMirroredReaddirBlockPublishesTheEntryAndCopiesTheRecordVerbatim()
    {
        // getdirentries_darwin_impl.cs: one unmanaged block, the record then the dirent* slot; the
        // record's first reclen bytes go to buf unchanged.
        byte* block = (byte*)NativeMemory.Alloc((nuint)(NativeDirentSize + sizeof(nint)));

        try
        {
            byte* entry = block;
            nint* entryp = (nint*)(block + NativeDirentSize);
            *entryp = 0;

            delegate* unmanaged[Cdecl]<nuint, nuint, nuint, nuint> fake = &FakeReaddirR;
            nuint r = GoLibcCall.Call((nint)fake, [7, (nuint)entry, (nuint)entryp], GoLibcErrnoRule.None, 0, out _);

            Assert.AreEqual((nuint)0, r, "readdir_r succeeds");
            Assert.AreEqual((nint)entry, *entryp, "libc publishes the entry's address through the slot, so the EOF test can see a non-nil entry");

            int reclen = *(ushort*)(entry + 16);
            slice<byte> buf = new slice<byte>(64);
            new ReadOnlySpan<byte>(entry, reclen).CopyTo(buf.ToSpan());

            Assert.AreEqual((int)RecordLength, reclen, "d_reclen at 16");
            Assert.AreEqual(0x1234UL, BitConverter.ToUInt64(buf.ToSpan()[..8]), "d_ino at 0");
            Assert.AreEqual((ushort)3, BitConverter.ToUInt16(buf.ToSpan()[18..20]), "d_namlen at 18");
            Assert.AreEqual("abc", System.Text.Encoding.ASCII.GetString(buf.ToSpan()[21..24]), "d_name at 21");
            Assert.AreEqual((byte)0, buf[reclen], "nothing past reclen is copied");
        }
        finally
        {
            NativeMemory.Free(block);
        }
    }
}
