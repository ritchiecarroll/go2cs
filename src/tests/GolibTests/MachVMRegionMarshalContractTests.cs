using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;

namespace GolibTests;

/// <summary>
/// Pins the golib contract darwin's <c>libcCall</c> arm for <c>mach_vm_region</c>
/// (runtime/darwin/libccall_impl.cs, machVMRegionDispatch) is written against: a SEVEN-argument
/// <see cref="GoLibcCall.Call"/> whose fifth argument is the ORDER TOKEN of a reference-bearing box
/// reaches libc as the address of that box's Go-layout native image, and the image libc writes comes
/// back decoded into the box.
/// </summary>
/// <remarks>
/// <para>
/// <b>Why a contract and not a red test of the arm.</b> The arm compiles only under
/// <c>$(GoTargetOS)=darwin</c> and calls Mach, which no fleet host has, so nothing here can execute it.
/// Its red is the darwin pprof sweep: net/http/pprof's heap handler on both mac legs, refused by field
/// at the default dispatch ("field 'address' of mach_vm_region_args is a ж`1", run 38521951876).
/// This file is green at its parent by construction. It pins the two facts the arm adds nothing to and
/// relies on, so a later golib change that breaks either reads here and not only on a mac.
/// </para>
/// <para>
/// <b>The two facts.</b> runtime/pprof's machVMRegionBasicInfoData carries <c>Offset [8]byte</c> and
/// <c>Pad_cgo_1 [2]byte</c>, which convert to <c>array&lt;byte&gt;</c>, so its box answers a token. Its Go
/// layout is the kernel's packed 36-byte vm_region_basic_info_data_64_t: Go spells Offset as [8]byte
/// precisely so that it sits at offset 20 unaligned. The mirror below has the same fields in the same
/// order. And the arm passes the token as the FIFTH of seven arguments, where the 1.24.13 funnel's own
/// callers pass at most six, so the seven-argument arm of the dispatch is pinned here too.
/// </para>
/// <para>
/// <b>Made to fail</b> before it was kept: with the mirror's Offset declared as <c>new array&lt;byte&gt;(9)</c>,
/// both arms went red, the size read 40 against 36 and Behavior decoded as 0xABCD, the wired count four bytes
/// on. A SHORTER Offset is not a usable break: at 7 bytes, alignment pads byte 27 and every later offset and
/// the size are unchanged, so both assertions stayed green (only this file's own eight-byte read went out of
/// range). The layout this pins is the one where Offset's length moves what follows it.
/// </para>
/// </remarks>
[TestClass]
public unsafe class MachVMRegionMarshalContractTests
{
    // runtime/pprof's defs_darwin_amd64.go machVMRegionBasicInfoData, field for field.
    // Go layout: Protection 0, Max_protection 4, Inheritance 8, Shared 12, Reserved 16, Offset 20..27,
    // Behavior 28, User_wired_count 32, Pad_cgo_1 34..35 -> size 36.
    public struct BasicInfoMirror
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

        public BasicInfoMirror()
        {
            Offset = new array<byte>(8);
            Pad_cgo_1 = new array<byte>(2);
        }
    }

    private const int NativeImageBytes = 36;

    private static nuint s_task;
    private static nuint s_flavor;
    private static nuint s_info;

    // mach_vm_region(target_task, *address, *size, flavor, info, *infoCnt, *object_name), as the arm calls it.
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nuint FakeMachVMRegion(nuint task, nuint address, nuint size, nuint flavor, nuint info, nuint count, nuint objectName)
    {
        s_task = task;
        s_flavor = flavor;
        s_info = info;

        if (ManagedPointerTokens.IsTaggedToken(info))
            return 1; // KERN_INVALID_ADDRESS: the kernel would refuse the token

        Unsafe.WriteUnaligned((byte*)address, 0x1_0000_4000UL);
        Unsafe.WriteUnaligned((byte*)size, 0x8000UL);
        Unsafe.WriteUnaligned((byte*)count, 9u);
        Unsafe.WriteUnaligned((byte*)objectName, 0x77u);

        byte* p = (byte*)info;
        Unsafe.WriteUnaligned(p + 0, 5);                     // Protection: VM_PROT_READ | VM_PROT_EXECUTE
        Unsafe.WriteUnaligned(p + 4, 7);                     // Max_protection
        Unsafe.WriteUnaligned(p + 20, 0x1122334455667788UL); // Offset, unaligned at 20
        Unsafe.WriteUnaligned(p + 28, 3);                    // Behavior
        Unsafe.WriteUnaligned(p + 32, (ushort)0xABCD);       // User_wired_count

        return 0; // KERN_SUCCESS
    }

    [TestMethod]
    public void TheRegionInfoRecordMarshalsAtGosPacked36ByteLayout()
    {
        Assert.AreEqual((nuint?)NativeImageBytes, NativeStructMarshal.MarshalledSizeOf(typeof(BasicInfoMirror), nilPointersAsZero: true),
            "machVMRegionBasicInfoData's Go size is the kernel's packed vm_region_basic_info_data_64_t");
    }

    [TestMethod]
    public void ASevenArgumentCallCarriesTheInfoTokenAsANativeImageAndDecodesItBack()
    {
        ref BasicInfoMirror info = ref heap(new BasicInfoMirror(), out ж<BasicInfoMirror> box);
        nuint token = (nuint)(uintptr)box;
        Assert.IsTrue(ManagedPointerTokens.IsTaggedToken(token), "the record must reach the call as a token, as runtime/pprof's does");

        nint cells = Marshal.AllocHGlobal(24);

        try
        {
            delegate* unmanaged[Cdecl]<nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint> fake = &FakeMachVMRegion;

            nuint r = GoLibcCall.Call((nint)fake, [0x103, (nuint)cells, (nuint)(cells + 8), 9, token, (nuint)(cells + 16), (nuint)(cells + 20)],
                GoLibcErrnoRule.None, 0, out _);

            Assert.IsFalse(ManagedPointerTokens.IsTaggedToken(s_info), "libc must receive the record's native image, never the token");
            Assert.AreEqual((nuint)0, r, "KERN_SUCCESS");
            Assert.AreEqual((nuint)0x103, s_task, "the first argument arrives as passed");
            Assert.AreEqual((nuint)9, s_flavor, "the fourth argument arrives as passed");

            Assert.AreEqual(0x1_0000_4000L, Marshal.ReadInt64(cells, 0), "address cell");
            Assert.AreEqual(0x8000L, Marshal.ReadInt64(cells, 8), "size cell");
            Assert.AreEqual(9, Marshal.ReadInt32(cells, 16), "count cell, the sixth argument");
            Assert.AreEqual(0x77, Marshal.ReadInt32(cells, 20), "object_name cell, the SEVENTH argument");

            Assert.AreEqual(5, info.Protection);
            Assert.AreEqual(7, info.Max_protection);
            Assert.AreEqual(3, info.Behavior, "Behavior at 28, after the unaligned Offset");
            Assert.AreEqual((ushort)0xABCD, info.User_wired_count, "User_wired_count at 32");

            ulong offset = 0;

            for (int i = 0; i < 8; i++)
                offset |= (ulong)info.Offset[i] << (8 * i);

            Assert.AreEqual(0x1122334455667788UL, offset, "Offset's eight bytes at 20, little-endian, as runtime/pprof's read64 reads them");
        }
        finally
        {
            Marshal.FreeHGlobal(cells);
        }
    }
}
