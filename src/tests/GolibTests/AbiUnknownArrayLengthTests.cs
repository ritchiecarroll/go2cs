using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using abi = go.@internal.abi_package;
using static go.@internal.abi_package;
using unique = go.unique_package;

namespace GolibTests;

// C# spells every Go [N]T as the one array<T>, so a descriptor synthesized from the TYPE alone (abi.TypeFor[T] for an
// UNNAMED array type argument, the typed-nil route of TypeOf((*T)(nil)).Elem()) has no length. Its size could not be
// derived, and Size_ was left at 0: abi.TypeFor[[2]string]() answered Size() == 0, Len() == 0 and ArrayType() == nil.
// unique.Make([2]string{a, b}) then took Go's zero-size arm and returned the shared zerobase handle, so two DIFFERENT
// pairs compared EQUAL. That was a wrong answer, not a panic.
//
// A descriptor whose size is unknown now refuses by name at each accessor that would otherwise report a plausible
// number: Size(), Len() and ArrayType(). The same array type from a LIVE value carries its length and still answers.
[TestClass]
public class AbiUnknownArrayLengthTests
{
    private static void AssertRefusesByName(System.Action read, string what)
    {
        PanicException panic = Assert.ThrowsException<PanicException>(read, $"{what} answered instead of refusing");
        StringAssert.StartsWith(panic.Message, "runtime error: ", what);
        StringAssert.Contains(panic.Message, "[?]string", $"{what}: the refusal does not name the array type");
    }

    [TestMethod]
    public void TypeForAnUnnamedArrayRefusesItsSize()
    {
        ж<abi.Type> descriptor = abi.TypeFor<array<@string>>();

        Assert.AreEqual(abi.Array, descriptor.Value.Kind(), "abi.TypeFor[[N]string]() is not an array descriptor");
        AssertRefusesByName(() => descriptor.Size(), "Size()");
        Assert.AreEqual(
            "runtime error: the size of [?]string is not known (the length of [?]string is not carried for an unnamed array type argument)",
            Assert.ThrowsException<PanicException>(() => descriptor.Size()).Message, "the refusal's text");
        AssertRefusesByName(() => abi.Len(descriptor), "Len()");
        AssertRefusesByName(() => abi.ArrayType(descriptor), "ArrayType()");
    }

    [TestMethod]
    public void UniqueMakeOfAnUnnamedArrayNeverAnswersTheZeroSizeHandle()
    {
        array<@string> ab = new(new @string[] { "a", "b" });
        array<@string> cd = new(new @string[] { "c", "d" });

        // Before: both were the shared zerobase handle, so two different pairs compared equal.
        AssertRefusesByName(() => unique.Make(ab), "unique.Make([2]string)");
        AssertRefusesByName(() => unique.Make(cd), "unique.Make([2]string)");
    }

    [TestMethod]
    public void ALiveArrayValueStillCarriesItsLength()
    {
        ж<abi.Type> descriptor = abi.TypeOf(new array<@string>(new @string[] { "a", "b" }));

        Assert.AreEqual((uintptr)32, descriptor.Size(), "a live [2]string's descriptor lost its size");
        Assert.AreEqual((nint)2, abi.Len(descriptor), "a live [2]string's descriptor lost its length");
        Assert.IsNotNull(abi.ArrayType(descriptor), "a live [2]string's descriptor lost its ArrayType");
        Assert.AreEqual((uintptr)8, abi.TypeFor<int64>().Size(), "a scalar's size moved");
    }
}
