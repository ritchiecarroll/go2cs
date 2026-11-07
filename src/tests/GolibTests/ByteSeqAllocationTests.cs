// ByteSeqAllocationTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

/// <summary>
/// Guards the allocation-free `string | []byte` union constraint — golib's
/// <see cref="IByteSeq{TSelf, T}"/> and the conversion extensions that go with it.
/// </summary>
/// <remarks>
/// <para>
/// The converter renders Go's `string | []byte` union constraint as
/// <c>where bytes : IByteSeq&lt;bytes, byte&gt;</c>. That interface used to be
/// <c>IByteSeq&lt;byte&gt;</c>, whose sub-slice indexer returned the INTERFACE — so every
/// <c>s[a:b]</c> inside a union-constrained body boxed the concrete struct (48 bytes for
/// slice&lt;byte&gt;, 24 for @string) and the converter's <c>((bytes)(…))</c> cast unboxed it
/// again. <c>len(s)</c> and the <c>[]byte(s)</c> / <c>string(s)</c> conversions boxed too,
/// because each took the sequence as an interface parameter. Go allocates nothing on any of
/// these paths; C# was allocating on all of them.
/// </para>
/// <para>
/// The shape measured here is <c>time.parseRFC3339</c>'s: seven sub-slices, eight
/// <c>len</c> calls and six <c>[]byte(s)</c> conversions per parse. That is the function
/// behind <c>time.TestUnmarshalTextAllocations</c>, which asserts ZERO allocations and
/// therefore admits no disclosure. Measured before the redesign: <b>720 B/run</b> on the
/// slice&lt;byte&gt; instantiation, <b>776 B/run</b> on @string.
/// </para>
/// <para>
/// The regression is silent and cheap to reintroduce — widening any of these members back to
/// an interface still compiles and still produces correct results, it just allocates again.
/// So the assertions are on measured bytes, with a deliberately-boxed control alongside to
/// prove the measurement can still see a box when there is one.
/// </para>
/// </remarks>
[TestClass]
public class ByteSeqAllocationTests
{
    // High enough that a per-call box is unmistakable against measurement noise, low enough to stay fast.
    private const int LoopCount = 1000;

    private const string Timestamp = "2006-01-02T15:04:05Z";

    // Sum of the six parsed fields (2006 + 1 + 2 + 15 + 4 + 5), the five checked separator bytes
    // ('-' '-' 'T' ':' ':') and the length of the remainder ("Z").
    private const nint Expected = 2006 + 1 + 2 + 15 + 4 + 5 + '-' + '-' + 'T' + ':' + ':' + 1;

    // ---- the shape under test: exactly what the converter emits for time.parseRFC3339 ----

    private static nint ParseShape<TSeq>(TSeq s)
        where TSeq : IByteSeq<TSeq, byte>, new()
    {
        nint total = 0;

        if (len(s) < 19)
            return -1;

        // Six field extractions — sub-slice, then scan. parseRFC3339's parseUint calls.
        total += Digits(((TSeq)(s[0..4])));
        total += Digits(((TSeq)(s[5..7])));
        total += Digits(((TSeq)(s[8..10])));
        total += Digits(((TSeq)(s[11..13])));
        total += Digits(((TSeq)(s[14..16])));
        total += Digits(((TSeq)(s[17..19])));

        // Separator checks — plain element reads.
        total += s[4] + s[7] + s[10] + s[13] + s[16];

        // The tail re-slice, assigned back to the type parameter (`s = s[19:]` in Go).
        TSeq rest = ((TSeq)(s[19..]));

        return total + len(rest);
    }

    // parseRFC3339's parseUint lambda: Go writes `for _, c := range []byte(s)`, which the
    // converter emits as a range over the `[]byte(s)` conversion of the constrained value.
    private static nint Digits<TSeq>(TSeq s)
        where TSeq : IByteSeq<TSeq, byte>, new()
    {
        nint x = 0;

        foreach (var (_, c) in range(s.ToSlice()))
            x = x * 10 + c - '0';

        return x;
    }

    // ---- the control: the same shape, deliberately routed through the interface ----

    // Reproduces the pre-redesign emission: every sub-slice result and every conversion argument
    // passes through an IByteSeq-typed slot, which is exactly what boxed before.
    private static nint ParseShapeBoxed<TSeq>(TSeq s)
        where TSeq : IByteSeq<TSeq, byte>, new()
    {
        nint total = 0;

        if (BoxedLen(s) < 19)
            return -1;

        total += DigitsBoxed(((TSeq)BoxedSub(s, 0..4)));
        total += DigitsBoxed(((TSeq)BoxedSub(s, 5..7)));
        total += DigitsBoxed(((TSeq)BoxedSub(s, 8..10)));
        total += DigitsBoxed(((TSeq)BoxedSub(s, 11..13)));
        total += DigitsBoxed(((TSeq)BoxedSub(s, 14..16)));
        total += DigitsBoxed(((TSeq)BoxedSub(s, 17..19)));

        total += s[4] + s[7] + s[10] + s[13] + s[16];

        TSeq rest = ((TSeq)BoxedSub(s, 19..));

        return total + BoxedLen(rest);
    }

    // The old `IByteSeq<T> IByteSeq<T>.this[Range] => this[range];` explicit implementation: the
    // box is created inside a CALLEE and escapes through its return, which is why the JIT could
    // never elide it. Writing `(TSeq)(IByteSeq<byte>)s[a..b]` inline here instead would put the
    // box and its unbox next to each other in one method, where the JIT does remove the pair —
    // the control would then measure 384 B/parse rather than the 720 B/parse really paid, and
    // would understate what the redesign is worth. NoInlining pins that boundary in place.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static IByteSeq<byte> BoxedSub<TSeq>(TSeq s, Range range)
        where TSeq : IByteSeq<TSeq, byte> => s[range];

    private static nint DigitsBoxed<TSeq>(TSeq s)
        where TSeq : IByteSeq<TSeq, byte>, new()
    {
        nint x = 0;

        // The old `new slice<byte>(seq)` constructor — an interface parameter, so the argument boxes.
        foreach (var (_, c) in range(new slice<byte>((IByteSeq<byte>)s)))
            x = x * 10 + c - '0';

        return x;
    }

    // The old `len<T>(IByteSeq<T> seq)` overload's parameter shape.
    private static nint BoxedLen<TSeq>(TSeq s) where TSeq : IByteSeq<TSeq, byte> => ((IByteSeq)s).Length;

    // ---- measurement ----

    private static long Measure<TSeq>(Func<TSeq, nint> shape, TSeq s, out nint result)
    {
        return Measure(shape, s, out result, out _);
    }

    private static long Measure<TSeq>(Func<TSeq, nint> shape, TSeq s, out nint result, out long objects)
    {
        nint acc = 0;

        // Warm up so tiering/JIT allocations land outside the measurement window.
        for (int warm = 0; warm < 32; warm++)
            acc = shape(s);

        long beforeCount = AllocationCounter.CurrentThreadCount;
        long before = GC.GetAllocatedBytesForCurrentThread();

        for (int run = 0; run < LoopCount; run++)
            acc = shape(s);

        long after = GC.GetAllocatedBytesForCurrentThread();

        objects = AllocationCounter.CurrentThreadCount - beforeCount;
        result = acc;
        return after - before;
    }

    private static slice<byte> SliceInput() => new(System.Text.Encoding.ASCII.GetBytes(Timestamp));

    private static @string StringInput() => new(Timestamp);

    // ---- the guards ----

    [TestMethod]
    public void UnionConstrainedParseOverSliceAllocatesNothing()
    {
        long bytes = Measure(ParseShape, SliceInput(), out nint result);

        Assert.AreEqual(Expected, result, "the parse shape did not read the expected fields");

        Console.WriteLine($"slice<byte>: {bytes / (double)LoopCount:F1} B/parse");

        Assert.AreEqual(0L, bytes,
            $"a parseRFC3339-shaped body over slice<byte> allocated {bytes} bytes across {LoopCount} " +
            $"parses ({bytes / (double)LoopCount:F1} B/parse) — IByteSeq's sub-slice indexer must " +
            "return TSelf, len must take the constrained type parameter, and []byte(s) must go " +
            "through the unboxed ToSlice extension. It was 720 B/parse before the redesign.");
    }

    [TestMethod]
    public void UnionConstrainedParseOverStringAllocatesOnlyItsCopies()
    {
        // The counter is what makes the assertion below a COUNT rather than a byte budget; it is
        // process-global, one-way and idempotent, so enabling it here costs nothing anyone else owns.
        AllocationCounter.Enable();

        long bytes = Measure(ParseShape, StringInput(), out nint result, out long objects);

        Assert.AreEqual(Expected, result, "the parse shape did not read the expected fields");

        Console.WriteLine($"@string: {bytes / (double)LoopCount:F1} B/parse, " +
                          $"{objects / (double)LoopCount:F1} objects/parse");

        // EXACTLY the six `[]byte(s)` conversions the shape performs — one per parseUint call.
        // Everything else in the body is now allocation-free: since r57c an @string is a WINDOW
        // (backing+offset+length), so the seven sub-slices allocate nothing, and the IByteSeq
        // redesign before it removed the boxing on len/conversion. The history of this one figure
        // is the history of both fixes: 776 B/parse originally, 416 B after the boxing went, and
        // 192 B once sub-slicing stopped copying.
        //
        // Asserted as an object COUNT, not a byte bound, because the count is the quantity that is
        // actually invariant here — the byte figure moves with the input's length, the number of
        // copies does not.
        Assert.AreEqual(6L * LoopCount, objects,
            $"a parseRFC3339-shaped body over @string allocated {objects / (double)LoopCount:F1} " +
            "objects/parse; it must be exactly the six []byte(s) conversions parseUint performs. " +
            "More means a sub-slice started copying again or a conversion started boxing; fewer " +
            "means golib's allocation census stopped seeing this path.");

        // The byte bound is kept as the cross-check on the count — with 4-byte-ish copies the six
        // conversions cost ~32 B each, so anything near the old 600 B ceiling means the copies grew.
        Assert.IsTrue(bytes / (double)LoopCount < 250,
            $"a parseRFC3339-shaped body over @string allocated {bytes / (double)LoopCount:F1} B/parse, " +
            "against 192 B measured when the @string window landed (r57c).");
    }

    [TestMethod]
    public void BoxedControlStillAllocates()
    {
        // The measurement's positive control. If this reads zero, the probe has stopped being able
        // to see a box and the guards above prove nothing.
        long bytes = Measure(ParseShapeBoxed, SliceInput(), out nint result);

        Assert.AreEqual(Expected, result, "the boxed control diverged from the fast path");

        Assert.IsTrue(bytes / (double)LoopCount >= 700,
            $"the deliberately-boxed control allocated only {bytes / (double)LoopCount:F1} B/parse; " +
            "it is supposed to reproduce the pre-redesign 720 B/parse. A control that does not " +
            "allocate cannot validate the zero-allocation assertions.");

        Console.WriteLine($"boxed control: {bytes / (double)LoopCount:F1} B/parse");
    }

    // ---- fidelity ----

    [TestMethod]
    public void BothInstantiationsAndBothShapesAgree()
    {
        // The whole point of the union constraint is that ONE body serves both member types.
        Assert.AreEqual(Expected, ParseShape(SliceInput()), "slice<byte> instantiation");
        Assert.AreEqual(Expected, ParseShape(StringInput()), "@string instantiation");
        Assert.AreEqual(Expected, ParseShapeBoxed(SliceInput()), "boxed control, slice<byte>");
        Assert.AreEqual(Expected, ParseShapeBoxed(StringInput()), "boxed control, @string");
    }

    [TestMethod]
    public void SliceFromSharesBackingAndStringFromCopies()
    {
        // Go's per-instantiation conversion semantics, which slice<byte>.From must keep:
        // `[]byte([]byte)` is the SAME slice, `[]byte(string)` is a copy.
        slice<byte> source = SliceInput();
        slice<byte> viaExtension = source.ToSlice();

        viaExtension[0] = (byte)'X';
        Assert.AreEqual((byte)'X', source[0], "[]byte([]byte) must share its backing, as in Go");

        @string text = StringInput();
        slice<byte> copied = text.ToSlice();

        copied[0] = (byte)'Y';
        Assert.AreEqual((byte)'2', text[0], "[]byte(string) must copy — a Go string is immutable");

        // string(s) over both members reproduces the original text either way.
        Assert.AreEqual(Timestamp, SliceInput().ToGoString().ToString(), "string([]byte)");
        Assert.AreEqual(Timestamp, StringInput().ToGoString().ToString(), "string(string)");
    }

    [TestMethod]
    public void SubSliceChainsStayConcrete()
    {
        // A chain of sub-slices must keep its concrete type all the way down — that is what
        // makes `s = s[19:]` assign back to the type parameter without a cast at runtime.
        Assert.AreEqual("01-02", Chain(StringInput()).ToString());
        Assert.AreEqual("01-02", Chain(SliceInput()).ToGoString().ToString());
    }

    private static TSeq Chain<TSeq>(TSeq s) where TSeq : IByteSeq<TSeq, byte>, new() => s[5..][..5];
}
