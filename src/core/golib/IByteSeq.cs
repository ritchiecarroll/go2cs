// IByteSeq.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;

namespace go;

// IByteSeq models Go's `string | []byte` union constraint, which C# cannot express directly
// (C# generic constraints are conjunctive — "and", never "or"). It exposes only the read
// operations common to both members — length, indexing, and sub-slicing — which is all a
// `string | []byte`-constrained function body can legally use (the union is neither comparable
// nor additive, so no operator interfaces are implied). The converter emits this interface
// (instantiated as IByteSeq<byte>) for such constraints; see getGenericDefinition.
//
// It is generic so the generic slice<T> can implement it (a byte-specific interface could not be
// conditionally implemented only when T is byte). @string implements IByteSeq<byte>; slice<T>
// implements IByteSeq<T>, so slice<byte> satisfies the IByteSeq<byte> constraint as well.
[EditorBrowsable(EditorBrowsableState.Never)] public interface IByteSeq
{
    // Length is the element count (Go's len). nint so it matches the indexer/slice domain.
    nint Length { get; }
}

[EditorBrowsable(EditorBrowsableState.Never)] public interface IByteSeq<T> : IByteSeq
{
    // Read-only element access (Go's s[i]). A ref-returning indexer (e.g. slice<T>'s) satisfies
    // this through an explicit implementation that reads the element by value.
    T this[nint index] { get; }

    // Spread source (Go's s...). A `string | []byte`-constrained body may spread the value into a
    // variadic — `append(dst, src[lo:hi]...)` in encoding/json's generic appendString — where the
    // sub-slice is typed as the constraint type parameter again. A type parameter has no members of
    // its own, so the spread `ꓸꓸꓸ` must live on the constraint interface for `src[lo..hi].ꓸꓸꓸ` to
    // bind.
    //
    // READ-ONLY, because one member is a string: a `string | []byte` body may only read what it
    // spreads (Go gives it no way to write through a string), and an @string's bytes may be shared
    // with every other string windowing the same backing, so a writable span here would let one
    // write corrupt them all. slice<T> keeps its own writable Span<T> spread and implements this
    // member explicitly.
    ReadOnlySpan<T> ꓸꓸꓸ { get; }

    // Which member of the union this value IS, for the two conversions (ByteSeqExtensions). Go's `[]byte(s)` over a
    // value whose type is a []byte - `[]byte` itself or a named one, jsontext's `type Value []byte` - is that SAME
    // slice: it shares the backing, and a write through the result is seen through s. So a byte-slice member hands out
    // its own window (golib's slice<T>; go2cs-gen's named []byte wrapper, its m_value) and the conversion returns it;
    // a string member answers false, and its bytes are copied, as Go's `[]byte(string)` does. TryGetString is the
    // mirror for `string(s)`: a string member (@string, or a named string wrapper's @string) IS the result, immutable,
    // so the conversion is free, as Go's is. These are members rather than type tests because the named wrappers are
    // types golib cannot name, and a member reached through the constraint is a direct call on the value type: no box.
    //
    // DEFAULT BODIES, because these two members were added after go.lib 1.26.10.3 shipped. A package built against .3
    // implements IByteSeq<byte> without them (go2cs-gen emits the implementation into every converted package that
    // declares a named []byte or named string), and an ABSTRACT member it lacks would make the type fail to load under
    // a later go.lib: the binary-compatibility guard refuses that as CP0006. With a default, such an implementer
    // answers false to both, so ByteSeqExtensions copies, which is what .3's conversions did for it: the values are
    // right, and only the sharing of `[]byte(v)` with v waits for the package to be rebuilt. Every implementer
    // golib and go2cs-gen write today implements both explicitly, so none of them reaches these bodies. Reaching one
    // through the constraint boxes a struct implementer, a cost only the old-built packages pay.
    bool TryGetSlice(out slice<T> window)
    {
        window = default;
        return false;
    }

    bool TryGetString(out @string value)
    {
        value = default;
        return false;
    }
}

// The SELF-REFERENTIAL form, and the one the converter emits as the constraint
// (`where bytes : IByteSeq<bytes, byte>`). TSelf is the implementing type itself — @string
// implements IByteSeq<@string, byte>, slice<T> implements IByteSeq<slice<T>, T> — so the
// sub-slice indexer returns the CONCRETE sequence type rather than the interface.
//
// That is the whole point of the extra type parameter. A Go body constrained on
// `string | []byte` sub-slices constantly (time's parseRFC3339 takes seven sub-slices per
// call), and Go types each result as the type parameter again. When the indexer returned
// `IByteSeq<T>`, every one of those sub-slices BOXED the struct result — 48 bytes for
// slice<T>, 24 for @string — and a converter-emitted `((bytes)(…))` cast then unboxed it, so a
// Go function that allocates nothing allocated per sub-slice in C#. Returning TSelf makes the
// call a `constrained.` direct call on the value type: no box, and the converter emits the
// range expression bare, because it already IS the type parameter. See GolibTests
// ByteSeqAllocationTests, which measures the difference against a deliberately boxed control.
//
// Sub-slicing is the only member that needs TSelf, so it lives alone on this interface and
// IByteSeq<T> keeps the members whose signatures do not mention the sequence type. Consumers
// that legitimately hold a byte sequence as an interface continue to bind IByteSeq<T> unchanged.
[EditorBrowsable(EditorBrowsableState.Never)] public interface IByteSeq<TSelf, T> : IByteSeq<T> where TSelf : IByteSeq<TSelf, T>
{
    // Sub-slice (Go's s[lo:hi]), returned as the implementing type so a chain stays concrete.
    // For @string this is an @string, for slice<T> a slice<T> — both already return exactly
    // that from their public Range indexer, so this is satisfied implicitly.
    TSelf this[Range range] { get; }

    // Sub-slice with bounds that are not constants that fit int32 (Go's s[lo:] and s[lo:hi]; `s[:hi]` is
    // `slice(0, hi)`). A C# Range cannot carry a negative bound or one past int32, so these take the bounds as nint
    // and panic as Go does (S-c R1-A). @string implements them publicly; slice<T> explicitly.
    TSelf slice(nint low);

    TSelf slice(nint low, nint high);
}

// The `[]byte(s)` and `string(s)` CONVERSIONS of a union-constrained value, which the converter
// emits as `s.ToSlice()` and `s.ToGoString()`.
//
// These are extension methods over the constrained type parameter rather than constructors or
// static factories, for two independent reasons:
//
//   1. Allocation. A constructor can only take the sequence as an interface — C# has no generic
//      constructor — so `new slice<byte>(seq)` boxes the caller's struct on every conversion.
//      Here TSeq is the caller's CONCRETE type, so the argument passes unboxed and the type
//      tests below fold to constants per instantiation.
//   2. Name binding. Converted code carries `using static go.builtin`, which brings the builtin
//      `slice<T>(…)` and `@string(…)` conversion METHODS into scope and shadows the type names.
//      `slice<byte>.From(s)` therefore does not compile (CS0119 — "is a method, which is not
//      valid in the given context"), and neither would any other static-factory spelling. An
//      extension call is member access on the receiver, so it is unaffected.
//
// Both keep Go's per-instantiation conversion semantics exactly: `[]byte([]byte)` is the same
// slice, `[]byte(string)` copies, `string(string)` is free, `string([]byte)` copies.
[EditorBrowsable(EditorBrowsableState.Never)] public static class ByteSeqExtensions
{
    // Go's `[]byte(s)` over a `string | []byte`-constrained value — time's parseRFC3339 ranges
    // `[]byte(s)` inside parseUint, six times per parse.
    public static slice<byte> ToSlice<TSeq>(this TSeq seq) where TSeq : IByteSeq<TSeq, byte>
    {
        // A byte-slice source IS the result — Go's []byte([]byte) shares its backing, and so does []byte(v) over a
        // NAMED []byte (TS readiness C16). Each implementation is a constant answer, so the JIT folds the test per
        // instantiation and compiles only one arm.
        if (seq.TryGetSlice(out slice<byte> window))
            return window;

        // Any other source materializes its own bytes, as Go's []byte(string) does — one object,
        // charged here because this extension IS the conversion the converter emits for it.
        return new slice<byte>(AllocationCounter.CopyOf<byte>(seq.ꓸꓸꓸ));
    }

    // Go's `string(s)` over the same — bytealg's Rabin-Karp compares `string(s[i:j])`.
    public static @string ToGoString<TSeq>(this TSeq seq) where TSeq : IByteSeq<TSeq, byte>
    {
        // A string source IS the result — `string(s)` over a string (or a named string) is the identity
        // conversion, and @string is immutable, so there is nothing to copy or rebuild. (This used to
        // reconstruct the value from its backing array; now that @string is a WINDOW over shared
        // storage, that would silently widen a sub-string back to its whole backing.)
        if (seq.TryGetString(out @string value))
            return value;

        return new @string(AllocationCounter.CopyOf<byte>(seq.ꓸꓸꓸ));
    }
}
