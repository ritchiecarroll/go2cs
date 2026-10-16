using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using abi = go.@internal.abi_package;
using static go.@internal.abi_package;
using reflect = go.reflect_package;

namespace GolibTests;

// A DEFINED array type (`type A [4]int`) emits as a go2cs-gen wrapper struct whose backing array<T>
// carries its length in the INSTANCE. Its [GoType("[4]nint")] marker is the one place the TYPE spells
// the length, and the reflection bridge did not read it: a descriptor built from the type alone
// (abi.TypeFor[A](), a pointer's Elem()) had no dims, so Size_ read 0 and ArrayType() read nil.
// unique.Make took its zero-size arm for such a T (every value one zerobase handle, "v0.Value not
// unique.testIntArray{7, 77, 777, 7777}") and makeCloneSeq dereferenced the nil ArrayType.
//
// The second half is the pointer conversion the canonMap hashes a key through
// (unsafe.Pointer(&key)): its fixed-array arm pins the array's data, and GCHandle refuses an array
// whose element holds a managed reference ("Object contains references"), so a `[3]string` key
// threw before it was ever hashed.
//
// The wrappers below are hand-written in the shape go2cs-gen emits (an IArray<T> struct over a
// lazily allocated array<T>, beside its [GoType] marker), because this project runs no generator.
[TestClass]
public class DefinedArrayDescriptorTests
{
    [TestMethod]
    public void TypeForADefinedArrayCarriesItsDeclaredLength()
    {
        ж<abi.Type> descriptor = abi.TypeFor<DefinedIntArray4>();

        Assert.AreEqual(abi.Array, descriptor.Value.Kind(), "abi.TypeFor[A]() is not an array descriptor");
        Assert.AreEqual((uintptr)(4 * IntPtr.Size), descriptor.Value.Size_, "abi.TypeFor[A]().Size_ for type A [4]int");

        ж<abi.ΔArrayType> array = abi.ArrayType(descriptor);

        Assert.IsNotNull(array, "ArrayType() of a defined array descriptor answered nil");
        Assert.AreEqual((uintptr)4, array.Value.Len, "ArrayType().Len for type A [4]int");

        reflect.ΔType t = reflect.TypeFor<DefinedIntArray4>();

        Assert.AreEqual(reflect.Array, t.Kind(), "reflect.TypeFor[A]().Kind()");
        Assert.AreEqual((nint)4, t.Len(), "reflect.TypeFor[A]().Len() with no value in hand");
        Assert.AreEqual((uintptr)(4 * IntPtr.Size), t.Size(), "reflect.TypeFor[A]().Size() with no value in hand");
    }

    [TestMethod]
    public void ANestedNamedElementAnswersItsOwnLength()
    {
        // type E [3]int; type N [2]E -- N's own length is 2, and E answers its 3 from its own marker.
        ж<abi.Type> descriptor = abi.TypeFor<DefinedNestedArray2>();
        ж<abi.ΔArrayType> array = abi.ArrayType(descriptor);

        Assert.IsNotNull(array, "ArrayType() of type N [2]E answered nil");
        Assert.AreEqual((uintptr)2, array.Value.Len, "N's own length");
        Assert.AreEqual((uintptr)(2 * 3 * IntPtr.Size), descriptor.Value.Size_, "N's size is 2 x 3 x sizeof(int)");

        ж<abi.ΔArrayType> element = abi.ArrayType(array.Value.Elem);

        Assert.IsNotNull(element, "ArrayType() of N's element E answered nil");
        Assert.AreEqual((uintptr)3, element.Value.Len, "E's own length, read from E");

        // A carried chain that reaches into the named element agrees on dims[0], which is all the
        // declared length decides; the element's 3 comes from E itself either way.
        ж<abi.ΔArrayType> carried = abi.ArrayType(abi.synthType(typeof(DefinedNestedArray2), new nint[] { 2, 3 }));

        Assert.IsNotNull(carried, "a carried [2,3] chain for N was refused");
        Assert.AreEqual((uintptr)2, carried.Value.Len, "N's length under a carried [2,3] chain");

        // A carried length that contradicts the declaration is refused by name, as a stamp's is.
        PanicException refused = Assert.ThrowsException<PanicException>(() => abi.synthType(typeof(DefinedNestedArray2), new nint[] { 5 }));
        StringAssert.Contains(refused.Message, "disagrees with its declared length");
    }

    [TestMethod]
    public void AStringElementArrayConvertsToAPointerWithoutPinning()
    {
        // unique's canonMap hashes a [3]string key through unsafe.Pointer(&key).
        ж<DefinedStringArray3> named = new StandardBox<DefinedStringArray3>(new DefinedStringArray3(new @string[] { "a", "b", "c" }.array()));
        uintptr namedAddress = named;

        Assert.AreNotEqual((uintptr)0, namedAddress, "a [3]string wrapper's pointer converted to 0");
        Assert.AreSame(named, (ж<DefinedStringArray3>)namedAddress, "a [3]string wrapper's pointer does not resolve back to its box");

        ж<array<@string>> bare = new StandardBox<array<@string>>(new @string[] { "x", "y" }.array());
        uintptr bareAddress = bare;

        Assert.AreSame(bare, (ж<array<@string>>)bareAddress, "a [2]string pointer does not resolve back to its box");

        // The data-pin arm still serves a reference-free element: the address is element 0's, so a
        // write through it lands in the array.
        array<long> values = new long[] { 1, 2, 3 }.array();
        ж<array<long>> pinned = new StandardBox<array<long>>(values);
        uintptr dataAddress = pinned;

        unsafe
        {
            *(long*)(nuint)dataAddress = 42;
        }

        Assert.AreEqual(42L, pinned.Value[0], "the fixed-array data arm no longer addresses element 0");
    }
}

// The go2cs-gen shape of `type A [N]E`: an IArray<E> struct over a lazily allocated array<E>.
internal static class DefinedArrayBacking
{
    internal static array<T> Of<T>(array<T> value, nint length) => value.Source is null ? new array<T>(length) : value;
}

[GoType("[4]nint")]
public readonly struct DefinedIntArray4(array<nint> value) : IArray<nint>
{
    private array<nint> Value => DefinedArrayBacking.Of(value, 4);
    public nint[] Source => Value.Source;
    Array? IArray.Source => Value.Source;
    public nint Length => Value.Length;
    public ref nint this[nint index] => ref Value[index];
    object? IArray.this[nint index] { get => Value[index]; set => Value[index] = (nint)value!; }
    public Span<nint> ꓸꓸꓸ => Value.ꓸꓸꓸ;
    public Span<nint> ToSpan() => Value.ToSpan();
    public object Clone() => new DefinedIntArray4((array<nint>)Value.Clone());
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Value).GetEnumerator();
    IEnumerator<(nint, nint)> IEnumerable<(nint, nint)>.GetEnumerator() => ((IEnumerable<(nint, nint)>)Value).GetEnumerator();
}

[GoType("[3]nint")]
public readonly struct DefinedIntArray3(array<nint> value) : IArray<nint>
{
    private array<nint> Value => DefinedArrayBacking.Of(value, 3);
    public nint[] Source => Value.Source;
    Array? IArray.Source => Value.Source;
    public nint Length => Value.Length;
    public ref nint this[nint index] => ref Value[index];
    object? IArray.this[nint index] { get => Value[index]; set => Value[index] = (nint)value!; }
    public Span<nint> ꓸꓸꓸ => Value.ꓸꓸꓸ;
    public Span<nint> ToSpan() => Value.ToSpan();
    public object Clone() => new DefinedIntArray3((array<nint>)Value.Clone());
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Value).GetEnumerator();
    IEnumerator<(nint, nint)> IEnumerable<(nint, nint)>.GetEnumerator() => ((IEnumerable<(nint, nint)>)Value).GetEnumerator();
}

[GoType("[2]DefinedIntArray3")]
public readonly struct DefinedNestedArray2(array<DefinedIntArray3> value) : IArray<DefinedIntArray3>
{
    private array<DefinedIntArray3> Value => DefinedArrayBacking.Of(value, 2);
    public DefinedIntArray3[] Source => Value.Source;
    Array? IArray.Source => Value.Source;
    public nint Length => Value.Length;
    public ref DefinedIntArray3 this[nint index] => ref Value[index];
    object? IArray.this[nint index] { get => Value[index]; set => Value[index] = (DefinedIntArray3)value!; }
    public Span<DefinedIntArray3> ꓸꓸꓸ => Value.ꓸꓸꓸ;
    public Span<DefinedIntArray3> ToSpan() => Value.ToSpan();
    public object Clone() => new DefinedNestedArray2((array<DefinedIntArray3>)Value.Clone());
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Value).GetEnumerator();
    IEnumerator<(nint, DefinedIntArray3)> IEnumerable<(nint, DefinedIntArray3)>.GetEnumerator() => ((IEnumerable<(nint, DefinedIntArray3)>)Value).GetEnumerator();
}

[GoType("[3]@string")]
public readonly struct DefinedStringArray3(array<@string> value) : IArray<@string>
{
    private array<@string> Value => DefinedArrayBacking.Of(value, 3);
    public @string[] Source => Value.Source;
    Array? IArray.Source => Value.Source;
    public nint Length => Value.Length;
    public ref @string this[nint index] => ref Value[index];
    object? IArray.this[nint index] { get => Value[index]; set => Value[index] = (@string)value!; }
    public Span<@string> ꓸꓸꓸ => Value.ꓸꓸꓸ;
    public Span<@string> ToSpan() => Value.ToSpan();
    public object Clone() => new DefinedStringArray3((array<@string>)Value.Clone());
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Value).GetEnumerator();
    IEnumerator<(nint, @string)> IEnumerable<(nint, @string)>.GetEnumerator() => ((IEnumerable<(nint, @string)>)Value).GetEnumerator();
}
