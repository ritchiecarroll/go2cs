using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;

namespace GolibTests;

// Go's contract for a pointer into NATIVE memory is ADDRESS identity: an unsafe.Pointer round-tripped
// through uintptr and back is the same pointer. runtime's lfnodeValidate asserts exactly that --
// `lfstackUnpack(lfstackPack(node, ^uintptr(0))) != node` -- over `&n.LFNode`, a FIELD reference into a
// persistentalloc'd node. The unpacked pointer is a NativeBox over the same address; equality compared
// only within a kind, so `!=` read true and Go's FATAL throw("bad lfnode address") ended the runtime
// row's test host after 186 of 10,891 results (measured 2026-09-22, once sysAllocOS returned real pages).
[TestClass]
public class NativeRootedFieldIdentityTests
{
    // A corpus struct with generated field accessors (go2cs-gen does not run for a type declared in
    // this assembly); syscall.Timespec stands in for runtime's lfnode, which is internal. It is
    // `{Sec, Nsec int64}` on every GOOS the tests build for, so its offset-0 field has one type
    // everywhere. RawSockaddrInet4, used before, starts with a uint16 Family on windows and linux but
    // a byte Len on darwin, and GolibTests did not compile under GoTargetOS=darwin (CS0029).
    // taggedPointerPack / taggedPointer.pointer() on amd64: the address' low 48 bits shifted up, the
    // count in the low bits, then shifted back (runtime/tagptr_64bit.go).
    // amd64: addrBits 48, tagBits 64-48+3 = 19 -- the address' low 3 bits are the node's guaranteed
    // 8-alignment and are carried by the shift, not stored.
    private const int TagBits = 19;

    private static ulong Pack(nuint address, ulong tag) => ((ulong)address << 16) | (tag & ((1UL << TagBits) - 1));

    private static nuint Unpack(ulong packed) => (nuint)(packed >> TagBits << 3);

    [TestMethod]
    public unsafe void ANativeRootedFieldReferenceRoundTripsThroughUintptrAsTheSamePointer()
    {
        nint block = Marshal.AllocHGlobal(sizeof(ulong) * 8);

        try
        {
            NativeMemory.Clear((void*)block, (nuint)(sizeof(ulong) * 8));

            var node = (ж<syscall_package.Timespec>)(uintptr)(nuint)block;   // the persistentalloc'd node
            ж<long> field = node.of(syscall_package.Timespec.ᏑSec);         // a FIELD reference at offset 0,
            //                                                             8-aligned as an lfnode is

            nuint address = ((uintptr)field).Value;
            Assert.AreNotEqual((nuint)0, address, "a native-rooted field reference has a real address");
            Assert.AreEqual((nuint)0, address % 8, "the fixture's field is 8-aligned, as Go requires of an lfnode");

            ж<long> rebuilt = (ж<long>)(uintptr)address;                     // what lfstackUnpack rebuilds
            Assert.IsTrue(field == rebuilt, "same address, so ONE Go pointer (lfnodeValidate's assertion)");
            Assert.IsTrue(rebuilt == field, "and the comparison is symmetric");
            Assert.AreEqual(field.GetHashCode(), rebuilt.GetHashCode(), "equal pointers hash equally");

            // The whole of Go's check, with the packing runtime performs on amd64.
            ж<long> unpacked = (ж<long>)(uintptr)Unpack(Pack(address, 0x7ffff));
            Assert.IsTrue(unpacked == field, "lfstackUnpack(lfstackPack(node, tag)) == node");

            // The pointer still names the same storage: a write through one is read through the other.
            field.Value = 0x5eed;
            Assert.AreEqual(0x5eedL, rebuilt.Value);
        }
        finally
        {
            Marshal.FreeHGlobal(block);
        }
    }

    [TestMethod]
    public void AManagedRootedFieldReferenceKeepsItsSourceAndTokenIdentity()
    {
        // The CONTROL: nothing changes for a field of managed storage -- two `&x.field` boxes over one
        // heap struct are equal, and a different field of the same struct is not.
        var heap = new StandardBox<syscall_package.Timespec>(new syscall_package.Timespec());
        ж<long> first = heap.of(syscall_package.Timespec.ᏑNsec);
        ж<long> second = heap.of(syscall_package.Timespec.ᏑNsec);
        ж<long> other = heap.of(syscall_package.Timespec.ᏑSec);

        Assert.IsTrue(first == second, "same source and field: one pointer");
        Assert.AreEqual(first.GetHashCode(), second.GetHashCode());
        Assert.IsFalse(ReferenceEquals(first, other), "different fields are different pointers");
    }
}
