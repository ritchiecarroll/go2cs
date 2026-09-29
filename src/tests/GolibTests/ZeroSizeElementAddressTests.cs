using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;
using @unsafe = go.unsafe_package;

namespace GolibTests;

/// <summary>
/// Element addresses that name NO storage: an element of a zero-size slice past the first, and the data
/// pointer of a non-nil zero-CAPACITY slice. Go answers both with a real, usable address (runtime's
/// zerobase for a <c>make</c>), so taking one, reading through it and converting it to a number never
/// fault. Both used to raise <see cref="IndexOutOfRangeException"/> — a host exception Go has no
/// equivalent of, escaping <c>recover()</c> — the moment the pointer was read or converted.
/// </summary>
[TestClass]
public class ZeroSizeElementAddressTests
{
    private struct Void
    {
    }

    // D1. A zero-size slice carries ONE shared element (GoZeroSizeFacts<T>.Storage) whatever its
    // length, so `&s[i]` for i >= 1 indexed that one-slot array at i. Go's &s[i] is data + i*0.
    [TestMethod]
    public void AnElementPastTheFirstOfAZeroSizeSliceIsReadableAndConvertible()
    {
        slice<Void> s = new(10);
        ж<Void> p = Ꮡ(s, 5);

        Assert.AreEqual(default(Void), p.Value, "reading through &s[5] reads the zero value");
        Assert.AreNotEqual((nuint)0, (nuint)(uintptr)p, "and converting it yields a non-nil address");
        Assert.IsFalse(@unsafe.Pointer.FromPinnedBox(p) == nil, "unsafe.Pointer(&s[5]) is non-nil");
    }

    [TestMethod]
    public void TheFirstElementOfAnOffsetZeroSizeWindowIsReadable()
    {
        slice<Void> tail = new slice<Void>(10).slice(3);
        ж<Void> p = Ꮡ(tail, 0);

        Assert.AreEqual(default(Void), p.Value, "&s[3:][0] reads the zero value");
        _ = (uintptr)p;
    }

    [TestMethod]
    public void EveryElementOfAZeroSizeSliceIsOneAddress()
    {
        // Go: &s[1] == &s[2] == &s[i] for every in-range i of a zero-size slice (data + i*0).
        slice<Void> s = new(10);

        Assert.IsTrue(Ꮡ(s, 1) == Ꮡ(s, 7), "two element addresses of a zero-size slice are one pointer");
        Assert.IsTrue(Ꮡ(s.slice(4), 0) == Ꮡ(s, 2), "an offset window's element is that same pointer");
    }

    [TestMethod]
    public void AnOutOfRangeZeroSizeElementStillPanicsLikeGo()
    {
        slice<Void> s = new(10);

        Assert.ThrowsException<PanicException>(() => Ꮡ(s, 10), "&s[len(s)] is Go's index panic, not a host fault");
    }

    // D2. unsafe.SliceData over a non-nil slice of capacity 0 is "a non-nil pointer to an unspecified
    // memory address" — Go's make([]T, 0) answers zerobase. It minted element 0 of a backing with no
    // element 0.
    [TestMethod]
    public void SliceDataOfAMadeZeroLengthSliceIsNonNilAndConvertible()
    {
        ж<nint> ints = @unsafe.SliceData(new slice<nint>(0));
        ж<byte> bytes = @unsafe.SliceData(new slice<byte>(0));

        Assert.IsFalse(ints == nil, "SliceData(make([]int, 0)) is non-nil");
        Assert.IsFalse(bytes == nil, "SliceData(make([]byte, 0)) is non-nil");
        Assert.IsFalse(@unsafe.Pointer.FromPinnedBox(ints) == nil, "unsafe.Pointer(unsafe.SliceData(make([]int, 0)))");
        Assert.IsFalse(@unsafe.Pointer.FromPinnedBox(bytes) == nil, "unsafe.Pointer(unsafe.SliceData(make([]byte, 0)))");
    }

    [TestMethod]
    public void SliceDataOfAZeroCapacityResliceIsNonNilAndConvertible()
    {
        slice<byte> full = new(8);
        ж<byte> end = @unsafe.SliceData(full.slice(8, 8));

        Assert.IsFalse(end == nil, "SliceData(s[len:len]) at cap 0 is non-nil");
        Assert.IsFalse(@unsafe.Pointer.FromPinnedBox(end) == nil);
    }

    [TestMethod]
    public void SliceDataOfAZeroCapacityReferenceSliceIsConvertible()
    {
        // A reference-bearing element converts through its order token rather than a pinned address;
        // the zero-capacity answer must survive that arm too.
        ж<@string> strings = @unsafe.SliceData(new slice<@string>(0));

        Assert.IsFalse(strings == nil);
        Assert.IsFalse(@unsafe.Pointer.FromPinnedBox(strings) == nil);
    }

    [TestMethod]
    public void SliceDataOfANonZeroCapacityEmptySliceStillNamesItsBacking()
    {
        // The control: Go's `&s[:1][0]` when cap(s) > 0 is the backing's first element even at len 0,
        // so a write through it is visible once the slice is extended — the zero-capacity answer must
        // not widen to this case.
        slice<byte> s = new(0, 4);
        ж<byte> data = @unsafe.SliceData(s);

        data.Value = 42;
        Assert.AreEqual((byte)42, s.slice(0, 1)[0], "SliceData of a cap > 0 slice aliases its backing");
    }

    [TestMethod]
    public void SliceDataOfANilSliceIsStillNil()
    {
        Assert.IsTrue(@unsafe.SliceData(default(slice<byte>)) == nil);
    }

    [TestMethod]
    public void SliceDataOfAZeroCapacityZeroSizeSliceIsTheSharedElement()
    {
        // For a zero-size element type every element of every slice is the one shared element, so
        // make([]struct{}, 0)'s data pointer is &t[0] of any other []struct{} — as zerobase makes it in Go.
        ж<Void> empty = @unsafe.SliceData(new slice<Void>(0));

        Assert.IsTrue(empty == Ꮡ(new slice<Void>(3), 0));
    }
}
