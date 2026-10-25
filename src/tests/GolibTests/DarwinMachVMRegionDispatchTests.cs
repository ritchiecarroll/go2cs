// DarwinMachVMRegionDispatchTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;
using @unsafe = go.unsafe_package;

namespace GolibTests;

/// <summary>
/// The two runtime/pprof libc calls darwin's libcCall dispatches by type (runtime/darwin/libccall_impl.cs,
/// the mach_vm_region and proc_regionfilename arms), driven host-neutrally.
/// </summary>
/// <remarks>
/// <para>
/// Each arm reads its args struct and hands <see cref="GoLibcCall.Call"/> the numbers of the pointers in
/// it. These tests build the same numbers from the same kinds of boxes pprof's machVMInfo and
/// regionFilename pass, and call a FAKE libc function (an [UnmanagedCallersOnly] method standing in for
/// the resolved libSystem symbol, as in <see cref="DarwinFunnelMarshalTests"/>) that writes what libSystem
/// would. No fleet host runs darwin: the arms themselves are compiled by a GoTargetOS=darwin build and
/// run by the os-matrix mac legs.
/// </para>
/// <para>
/// What each green proves: proc_regionfilename's buffer, unsafe.SliceData of a []byte, crosses as the
/// pinned address of the slice's own backing store, so the file name libc writes is what the caller's
/// slice holds afterwards; and mach_vm_region's info block, which holds two fixed arrays and so is
/// reference-bearing, crosses as a Go-layout native copy (36 bytes, vm_region_basic_info_data_64_t's
/// layout) and is decoded back, while the reference-free scalar boxes cross as their own addresses.
/// </para>
/// </remarks>
[TestClass]
public unsafe class DarwinMachVMRegionDispatchTests
{
    // runtime/pprof's machVMRegionBasicInfoData (defs_darwin_amd64.go): Go layout Protection 0,
    // Max_protection 4, Inheritance 8, Shared 12, Reserved 16, Offset 20 ([8]byte), Behavior 28,
    // User_wired_count 32, Pad_cgo_1 34 ([2]byte) -> size 36, vm_region_basic_info_data_64_t's.
    public struct RegionBasicInfoLike
    {
        public int Protection;
        public int Max_protection;
        public uint Inheritance;
        public uint Shared;
        public uint Reserved;
        public array<byte> Offset;
        public int Behavior;
        public ushort User_wired_count;
        public array<byte> Pad_cgo_1;

        public RegionBasicInfoLike()
        {
            Offset = new array<byte>(8);
            Pad_cgo_1 = new array<byte>(2);
        }
    }

    private const uint FakeTask = 0x103;
    private const uint VM_REGION_BASIC_INFO_64 = 9;
    private const uint VM_REGION_BASIC_INFO_COUNT_64 = 9;
    private const nuint KERN_FAILURE = 5;
    private const ulong RegionStart = 0x1_0000_0000UL;
    private const ulong RegionSize = 0x4000UL;
    private const string RegionFile = "/private/var/folders/runtime.pprof.tests";

    private static readonly byte[] s_regionFileBytes = Encoding.UTF8.GetBytes(RegionFile);

    private static nuint s_receivedBuffer;
    private static nuint s_receivedInfo;

    // kern_return_t mach_vm_region(task, *address, *size, flavor, info, *infoCnt, *object_name): fails
    // with KERN_FAILURE on anything libSystem could not use, otherwise writes what the kernel would.
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nuint FakeMachVMRegion(nuint task, nuint address, nuint size, nuint flavor, nuint info, nuint count, nuint objectName)
    {
        s_receivedInfo = info;

        if (task != FakeTask || flavor != VM_REGION_BASIC_INFO_64)
            return KERN_FAILURE;

        foreach (nuint pointer in (ReadOnlySpan<nuint>)[address, size, info, count, objectName])
        {
            if (pointer == 0 || ManagedPointerTokens.IsTaggedToken(pointer))
                return KERN_FAILURE;
        }

        if (*(uint*)count != VM_REGION_BASIC_INFO_COUNT_64 || *(ulong*)address != 1)
            return KERN_FAILURE;

        *(ulong*)address = RegionStart;
        *(ulong*)size = RegionSize;
        *(uint*)objectName = 0;

        byte* p = (byte*)info;
        Unsafe.WriteUnaligned(p + 0, 5);            // VM_PROT_READ | VM_PROT_EXECUTE
        Unsafe.WriteUnaligned(p + 4, 7);
        Unsafe.WriteUnaligned(p + 20, 0x1817161514131211UL);
        Unsafe.WriteUnaligned(p + 28, 0x42);
        Unsafe.WriteUnaligned(p + 32, (ushort)0x0102);
        return 0;
    }

    // int proc_regionfilename(pid, address, buffer, buffersize): copies the path without a terminator and
    // returns its length, 0 on failure.
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nuint FakeProcRegionFilename(nuint pid, nuint address, nuint buffer, nuint bufferSize)
    {
        s_receivedBuffer = buffer;

        if (buffer == 0 || ManagedPointerTokens.IsTaggedToken(buffer) || address != RegionStart || bufferSize < (nuint)s_regionFileBytes.Length)
            return 0;

        s_regionFileBytes.CopyTo(new Span<byte>((void*)buffer, s_regionFileBytes.Length));
        return (nuint)s_regionFileBytes.Length;
    }

    [TestMethod]
    public void TheRegionFilenameBufferIsTheByteSlicesPinnedBackingStore()
    {
        // pprof's regionFilename: buf := make([]byte, _MAXPATHLEN); proc_regionfilename(..., unsafe.SliceData(buf), int64(cap(buf))).
        slice<byte> buf = new slice<byte>(0x400);
        ж<byte> data = @unsafe.SliceData(buf);
        nuint address = (nuint)(uintptr)data;

        Assert.IsFalse(ManagedPointerTokens.IsTaggedToken(address), "a []byte element is reference-free: its number must be an address, not an order token");

        // The args box holds the element pointer across the call, so the pin must survive a collection.
        GC.Collect();
        GC.WaitForPendingFinalizers();

        delegate* unmanaged[Cdecl]<nuint, nuint, nuint, nuint, nuint> fake = &FakeProcRegionFilename;
        nuint r = GoLibcCall.Call((nint)fake, [(nuint)Environment.ProcessId, (nuint)RegionStart, address, (nuint)buf.Capacity], GoLibcErrnoRule.None, 0, out _);
        GC.KeepAlive(data);

        Assert.AreEqual(address, s_receivedBuffer, "libc must receive the buffer's address unchanged");
        Assert.AreEqual((nuint)s_regionFileBytes.Length, r, "the length libc reports");
        Assert.AreEqual(RegionFile, Encoding.UTF8.GetString(buf.ToSpan()[..(int)r]), "the name libc wrote must be in the caller's slice");
    }

    [TestMethod]
    public void TheRegionCallPlacesTheScalarAddressesAndMarshalsTheInfoBlock()
    {
        Assert.AreEqual((nuint?)36, NativeStructMarshal.MarshalledSizeOf(typeof(RegionBasicInfoLike), nilPointersAsZero: true), "vm_region_basic_info_data_64_t's size");

        // pprof's machVMInfo: addr, memRegionSize and info are heap locals; runtime's mach_vm_region adds count and object_name.
        ref ulong addr = ref heap(1UL, out ж<ulong> Ꮡaddr);
        ref ulong size = ref heap(0UL, out ж<ulong> Ꮡsize);
        ref RegionBasicInfoLike info = ref heap(new RegionBasicInfoLike(), out ж<RegionBasicInfoLike> Ꮡinfo);
        heap(VM_REGION_BASIC_INFO_COUNT_64, out ж<uint> Ꮡcount);
        heap(0U, out ж<uint> ᏑobjectName);

        @unsafe.Pointer infoPointer = @unsafe.Pointer.FromPinnedBox(Ꮡinfo);
        nuint infoNumber = (nuint)(uintptr)infoPointer;
        Assert.IsTrue(ManagedPointerTokens.IsTaggedToken(infoNumber), "the info block holds fixed arrays, so it must reach the call as a token, as pprof's does");

        delegate* unmanaged[Cdecl]<nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint> fake = &FakeMachVMRegion;
        nuint r = GoLibcCall.Call((nint)fake, [
            FakeTask,
            (uintptr)Ꮡaddr,
            (uintptr)Ꮡsize,
            VM_REGION_BASIC_INFO_64,
            infoNumber,
            (uintptr)Ꮡcount,
            (uintptr)ᏑobjectName
        ], GoLibcErrnoRule.None, 0, out _);

        Assert.IsFalse(ManagedPointerTokens.IsTaggedToken(s_receivedInfo), "libc must receive a native copy of the info block, never the token");
        Assert.AreEqual((nuint)0, r, "KERN_SUCCESS");
        Assert.AreEqual(RegionStart, addr, "the region start libc wrote comes back through the address box");
        Assert.AreEqual(RegionSize, size, "the region size libc wrote comes back through the size box");
        Assert.AreEqual(5, info.Protection);
        Assert.AreEqual(7, info.Max_protection);
        Assert.AreEqual(0x11, info.Offset[0], "Offset is at 20, the first byte");
        Assert.AreEqual(0x18, info.Offset[7], "Offset is at 20, the last byte");
        Assert.AreEqual(0x42, info.Behavior, "Behavior is at 28");
        Assert.AreEqual((ushort)0x0102, info.User_wired_count, "User_wired_count is at 32");
    }
}
