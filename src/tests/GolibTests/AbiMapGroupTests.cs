using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using abi = go.@internal.abi_package;
using static go.@internal.abi_package;

namespace GolibTests;

// The synthesized SwissMapType's Group (internal/abi type_impl.cs, synthesizeMapType). The projection
// left Group nil, so runtime's TestGroupSizeZero died dereferencing it. Go lays a group out as
// struct { ctrl uint64; slots [8]struct { key; elem } } (reflect.groupAndSlotOf, and the compiler's
// map_swiss.go), with a pad byte after a trailing zero-size field, and a key or elem wider than 128
// bytes held by pointer. The expected sizes below are unsafe.Sizeof of that struct under Go 1.24.
[TestClass]
public class AbiMapGroupTests
{
    private struct Big { internal long a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p, q; }

    private static ж<abi.ΔMapType> MapTypeOf<K, V>()
        where K : notnull
    {
        map<K, V> m = default!;
        return abi.TypeOf(m).MapType();
    }

    private static uintptr GroupSizeOf<K, V>()
        where K : notnull
    {
        ж<abi.ΔMapType> mt = MapTypeOf<K, V>();

        Assert.IsTrue(mt != nil, "premise: the map descriptor projects");
        Assert.IsTrue(mt.Value.Group != nil, "the descriptor carries a group type");
        Assert.AreEqual(mt.Value.Group.Value.Size_, mt.Value.GroupSize, "GroupSize is Group.Size_, as abi documents it");

        return mt.Value.Group.Value.Size_;
    }

    [TestMethod]
    public void ZeroSizeSlotsStillReserveAnExtraWord()
    {
        // TestGroupSizeZero's own case: 8 + 8*0 would be exactly 8, and the trailing zero-size field adds a pad.
        Assert.AreEqual((uintptr)16, GroupSizeOf<EmptyStruct, EmptyStruct>());
    }

    [TestMethod]
    public void ScalarAndStringSlots()
    {
        Assert.AreEqual((uintptr)136, GroupSizeOf<nint, nint>());
        Assert.AreEqual((uintptr)136, GroupSizeOf<sbyte, long>());
        Assert.AreEqual((uintptr)72, GroupSizeOf<int, sbyte>());
        Assert.AreEqual((uintptr)200, GroupSizeOf<@string, bool>());
    }

    [TestMethod]
    public void ATrailingZeroSizeElemPadsTheSlot()
    {
        Assert.AreEqual((uintptr)136, GroupSizeOf<long, EmptyStruct>());
    }

    [TestMethod]
    public void AKeyOverOneHundredTwentyEightBytesIsHeldByPointer()
    {
        // Go's SwissMapMaxKeyBytes: the slot holds an 8-byte pointer, so the group matches a pointer key's.
        Assert.AreEqual((uintptr)136, GroupSizeOf<Big, nint>());
    }
}
