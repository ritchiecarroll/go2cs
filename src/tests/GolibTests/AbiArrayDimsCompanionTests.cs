using go;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using abi = go.@internal.abi_package;
using unique = go.unique_package;

namespace GolibTests;

// abi.TypeFor<T, Tᴀ>(): the ARRAY-DIMS COMPANION overload of the type-only sink. A type argument alone
// cannot carry an unnamed array's length here (C# spells every Go [N]E as the one array<E>; see
// AbiUnknownArrayLengthTests), so the converter supplies a dims phantom at a call site whose type
// argument is an unnamed array, and golib's ᴀnone everywhere else. Identity is the property that
// matters: unique.Make stores under the descriptor this answers, so it must be the box a live [N]E value
// answers, a different box for a different length, and ONE box for two phantoms with equal dims (two
// assemblies each declare their own ᴀ2). docs/phase4/DESIGN-array-type-argument-length.md, 3.1 and 3.5.
[TestClass]
public class AbiArrayDimsCompanionTests
{
    private sealed class ᴀ2 : IGoArrayDims { public static nint[] Dims { get; } = [2]; }

    private sealed class ᴀ3 : IGoArrayDims { public static nint[] Dims { get; } = [3]; }

    // A second assembly's phantom for the same length: a different C# type with equal Dims.
    private sealed class OtherTwo : IGoArrayDims { public static nint[] Dims { get; } = [2]; }

    [TestMethod]
    public void DimsReachTheBoxALiveArrayValueAnswers()
    {
        ж<abi.Type> fromDims = abi.TypeFor<array<@string>, ᴀ2>();
        ж<abi.Type> fromValue = abi.TypeOf(new array<@string>(new @string[] { "a", "b" }));

        Assert.AreSame(fromValue, fromDims, "abi.TypeFor<[2]string, ᴀ2>() is not the box a live [2]string answers");
        Assert.AreEqual((uintptr)32, fromDims.Size(), "a [2]string descriptor's size");
        Assert.AreEqual((nint)2, fromDims.Len(), "a [2]string descriptor's length");
    }

    [TestMethod]
    public void ADifferentLengthIsADifferentBox()
    {
        ж<abi.Type> two = abi.TypeFor<array<@string>, ᴀ2>();
        ж<abi.Type> three = abi.TypeFor<array<@string>, ᴀ3>();

        Assert.AreNotSame(two, three, "[2]string and [3]string share a descriptor");
        Assert.AreEqual((nint)3, three.Len(), "a [3]string descriptor's length");
    }

    [TestMethod]
    public void TwoPhantomsWithEqualDimsReachOneBox()
    {
        Assert.AreSame(abi.TypeFor<array<@string>, ᴀ2>(), abi.TypeFor<array<@string>, OtherTwo>(),
            "two phantoms with dims [2] reached two descriptors: unique.Make from two assemblies would key two maps");
    }

    [TestMethod]
    public void NoDimsAnswersWhatTheArityOneSinkAnswers()
    {
        Assert.AreSame(abi.TypeFor<@string>(), abi.TypeFor<@string, ᴀnone>(), "ᴀnone moved a non-array descriptor");
        Assert.AreSame(abi.TypeFor<int64>(), abi.TypeFor<int64, ᴀnone>(), "ᴀnone moved a scalar descriptor");
    }

    [TestMethod]
    public void DimsForANonArrayRefuseByName()
    {
        PanicException panic = Assert.ThrowsException<PanicException>(() => abi.TypeFor<nint, ᴀ2>(),
            "dims for a non-array type argument answered a descriptor");

        StringAssert.StartsWith(panic.Message, "abi.TypeFor: array dims [2] for ", "the refusal's text");
        StringAssert.Contains(panic.Message, "which is not an array", "the refusal's text");
    }

    [TestMethod]
    public void UniqueMakeWithDimsKeysOnTheValue()
    {
        array<@string> ab = new(new @string[] { "a", "b" });
        array<@string> ab2 = new(new @string[] { "a", "b" });
        array<@string> ac = new(new @string[] { "a", "c" });

        unique.Handle<array<@string>> h1 = unique.Make<array<@string>, ᴀ2>(ab);
        unique.Handle<array<@string>> h2 = unique.Make<array<@string>, OtherTwo>(ab2);
        unique.Handle<array<@string>> h3 = unique.Make<array<@string>, ᴀ2>(ac);

        Assert.IsTrue(h1 == h2, "two equal [2]string values made two handles");
        Assert.IsFalse(h1 == h3, "two different [2]string values made one handle (the zero-size handle)");
        Assert.AreEqual("b", h1.Value()[1].ToString(), "the handle's value");
    }
}
