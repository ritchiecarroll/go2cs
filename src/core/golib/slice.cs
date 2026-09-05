// slice.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable InconsistentNaming

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using go.golib;

namespace go;

// The BACKING STORE of a slice, reachable without knowing its element type.
//
// `IArray.Source` cannot serve this purpose: it materializes a DETACHED COPY (see slice<T>.Source),
// so it answers a fresh object on every read and is useless as an identity. This interface answers
// the real array — the identity two slices share when one is a reslice of the other — and exists
// for exactly one consumer: GoReflect's element-dimension side table, which records an element
// array's LENGTH at the site that still knows it statically, because a slice with no elements
// cannot be asked for it afterwards. It is internal: nothing outside golib may take the backing.
internal interface ISliceBacking
{
    Array? Backing { get; }
}

public interface ISlice : IArray
{
    nint Low { get; }

    nint High { get; }

    nint Capacity { get; }

    nint Available { get; }

    ISlice? Append(object[] elems);
}

public interface ISlice<T> : IArray<T>, ISlice
{
    ISlice<T> Append(params T[] elems);

    ISlice<T> this[Range range] { get; }

    ISlice<T> Slice(int start, int length);

    ISlice<T> Slice(nint start, nint length);
}

// A ref struct option exists for slices that would be restricted to stack-only usage, however, this prevents
// struct from being boxed and/or stored on the heap. The issue with that is that the struct is not allowed to
// be stored in a field of another non-ref struct or an array, or boxed. This is a problem for slices since they
// are often stored in arrays or other structures in Go code. The ref struct option is not used for the standard
// slice here for this reason. In order to make use of a ref struct option, stack-to-heap escape analysis would
// need to determine if the struct is stored in a field of another struct or array, or boxed, and then disallow
// the ref struct option. The Go based go2cs converter already detects heap escapes, so this could be a viable
// option in the future, at least for slices that are private and used with internal package functions only.

[Serializable]
public readonly struct slice<T> : ISlice<T>, IList<T>, IReadOnlyList<T>, IEquatable<ISlice>, IEquatable<IArray>, ISupportMake<slice<T>>, ISliceWrap<slice<T>, T>, IByteSeq<slice<T>, T>, ISliceBacking
{
    // The real backing store, answered WITHOUT copying — `Source` materializes a detached copy and
    // so cannot serve as an identity. Explicitly implemented and internal: this adds no public
    // surface, and its only consumer is GoReflect's element-dimension side table.
    Array? ISliceBacking.Backing => m_array;

    internal readonly T[] m_array;
    private readonly nint m_low;
    private readonly nint m_length;
    private readonly nint m_capacity; // cap(s), relative to m_low — can end before the backing array does

    // The NATIVE backing (DESIGN-native-backed-slice.md, ratified 2026-08-22): 0 = managed (every
    // path that existed before), else the base address of native memory this slice ALIASES — the
    // ж<T> dual-mode precedent applied to the slice header. m_low/m_length/m_capacity keep their
    // exact meanings as ELEMENT offsets against the base; m_array stays a non-null empty array so
    // `== nil` remains false and the nil checks are untouched. Only unmanaged T may carry a native
    // base — enforced by OverNativeMemory, the single creation door — so a managed reference can
    // never be read from or written to kernel-owned bytes (the SiginfoChild corruption class as a
    // constructor precondition).
    private readonly nuint m_nativeBase;

    internal bool IsNativeBacked => m_nativeBase != 0;

    private unsafe void* NativeElementPointer(nint index) =>
        (void*)(m_nativeBase + (nuint)(m_low + index) * (nuint)System.Runtime.CompilerServices.Unsafe.SizeOf<T>());

    // The address of element `index`, for the pointer builders (builtin.Ꮡ, unsafe.SliceData):
    // bounds-checked with Go's own panic so a derived pointer can never name memory outside the
    // window it was derived from.
    internal unsafe nuint NativeElementAddress(nint index)
    {
        if (index < 0 || index >= m_length)
            throw RuntimeErrorPanic.IndexOutOfRange(index, m_length);

        return (nuint)NativeElementPointer(index);
    }

    private slice(nuint nativeBase, nint low, nint high, nint max)
    {
        m_array = [];
        m_nativeBase = nativeBase;
        m_low = low;
        m_length = high - low;
        m_capacity = max - low;
    }

    /// <summary>
    /// The single creation door for a native-backed slice: a window ALIASING <paramref name="length"/>
    /// elements of unmanaged memory at <paramref name="baseAddress"/>. Writes reach the memory, element
    /// addresses are the real ones, and lifetime is the mapping's own — exactly Go's contract for a
    /// slice built over native storage, hazards included.
    /// </summary>
    // The two-word form is Go's `unsafe.Slice` meaning (cap = len) and the arity every existing
    // caller binds; kept as a forwarding overload so a sibling seat cut against it merges at the
    // union without a fix-up (the widened-door rule, ruled 2026-09-05). RETIREMENT CONDITION: a
    // grep-proven zero two-word callers across golib, the corpus and GolibTests, in a later cut —
    // never the one that widened the door. The three-word form below is the primary; Q58's
    // native-backed array<T> reads it too.
    internal static slice<T> OverNativeMemory(nuint baseAddress, nint length) => OverNativeMemory(baseAddress, length, length);

    /// <summary>
    /// The same door with Go's third word: a window of <paramref name="length"/> elements over a
    /// backing that extends to <paramref name="capacity"/> elements — the (array, len, cap) a runtime
    /// slice header carries, so a slice re-based over reserved memory keeps room to grow in place
    /// exactly as Go's <c>append</c> within capacity does (increment 7 of the runtime row, 2026-09-05:
    /// <c>addrRanges</c> grows its native array by doubling and the page allocator's summary levels
    /// are minted at their full extent).
    /// </summary>
    internal static slice<T> OverNativeMemory(nuint baseAddress, nint length, nint capacity)
    {
        if (System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            throw new PanicException($"native-backed slice: element type {typeof(T).Name} contains managed references and cannot alias native memory");

        if (baseAddress == 0 || length < 0)
            throw new PanicException("native-backed slice: nil base or negative length");

        if (capacity < length)
            throw new PanicException($"native-backed slice: capacity {capacity} is smaller than length {length}");

        return new slice<T>(baseAddress, 0, length, capacity);
    }

    public slice()
    {
        m_array = [];
        m_low = m_length = m_capacity = 0;
    }

    public slice(T[]? array)
    {
        // Go converts a nil source to a nil slice, never a fault: `[]T(nil)` (e.g. the
        // `append([]string(nil), …)` copy idiom) is the nil slice.
        if (array is null)
        {
            this = default;
            return;
        }

        m_array = array;
        m_low = 0;
        m_length = array.Length;
        m_capacity = array.Length;
    }

    public slice(Span<T> source)
    {
        m_array = AllocationCounter.CopyOf<T>(source);
        m_low = 0;
        m_length = m_array.Length;
        m_capacity = m_array.Length;
    }

    public slice(ReadOnlySpan<T> source)
    {
        m_array = AllocationCounter.CopyOf(source);
        m_low = 0;
        m_length = m_array.Length;
        m_capacity = m_array.Length;
    }

    public slice(Memory<T> source)
    {
        m_array = AllocationCounter.CopyOf<T>(source.Span);
        m_low = 0;
        m_length = m_array.Length;
        m_capacity = m_array.Length;
    }

    public slice(ReadOnlyMemory<T> source)
    {
        m_array = AllocationCounter.CopyOf(source.Span);
        m_low = 0;
        m_length = m_array.Length;
        m_capacity = m_array.Length;
    }

    public slice(array<T> array) : this((T[])array) { }

    /// <summary>
    /// A <c>slice&lt;T&gt;</c> ALIASING <paramref name="source"/>'s backing storage, with the
    /// element type re-spelled from <typeparamref name="TSrc"/> to <typeparamref name="T"/>. The
    /// window (low / length / capacity) carries across unchanged, so the result is the SAME Go
    /// slice under a different element NAME — a write through either side is visible in the other.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This exists for exactly one Go relation, and it is not a general reinterpret: Go's
    /// <c>reflect.Value.Bytes</c>/<c>SetBytes</c> accept any slice whose element KIND is
    /// <c>Uint8</c> — <c>[]byte</c>, <c>[]renamedByte</c>, <c>type S []Uint8</c> — and reach the
    /// storage by re-typing the slice HEADER, never by copying. A copy would silently break every
    /// writer of a core reflect API, so a caller that needs a <c>[]byte</c> over a
    /// <c>[]DefinedByte</c> needs an alias or nothing.
    /// </para>
    /// <para>
    /// <b>Precondition, enforced by the caller and never here:</b> the two element types must be
    /// ONE representation under two names — both value types, both free of managed references, and
    /// both exactly one byte wide. <see cref="GoReflect.TryByteSliceView"/> is that caller and
    /// gates on precisely those facts (the same three the blessed
    /// <c>ReinterpretAliasesStorage</c> gate asks of a pointee pair). Under them the two backing
    /// objects are byte-for-byte the same shape and differ only in their method table, and every
    /// access golib makes through a <c>slice&lt;T&gt;</c> — element load/store, <c>ToSpan</c>,
    /// <c>CopyTo</c> — addresses the data from the STATIC element type and the array's own length
    /// field, never from that method table (<c>Span&lt;T&gt;</c>'s array constructor skips its
    /// covariance check outright for a value-typed T). What the pun does NOT survive is a runtime
    /// type test on the array OBJECT — <c>Array.Copy</c>'s element-type check,
    /// <c>backing is byte[]</c>, <c>backing.GetType()</c> — so nothing may reach one through the
    /// result, which is why this is an internal bridge primitive and not a public conversion.
    /// </para>
    /// </remarks>
    internal static slice<T> AliasOfElement<TSrc>(in slice<TSrc> source)
    {
        // A nil slice re-spells as the nil slice: there is no storage to alias, and Go's own
        // header re-typing carries the nil data pointer across in exactly this way.
        if (source.m_array is null)
            return default;

        // A native window is re-spelled by carrying the BASE across — the bytes are the same bytes
        // under a different element name, which is exactly what this alias means (and the element
        // types are one-byte-wide by the caller's own precondition).
        if (source.m_nativeBase != 0)
            return new slice<T>(source.m_nativeBase, source.m_low, source.m_low + source.m_length, source.m_low + source.m_capacity);

        T[] aliased = Unsafe.As<T[]>(source.m_array);

        return new slice<T>(aliased, source.m_low, source.m_low + source.m_length, source.m_low + source.m_capacity);
    }

    /// <summary>
    /// Creates a slice over an existing slice VIEW (an <see cref="ISlice{T}"/>-boxed value — a
    /// constrained type parameter or a named-slice wrapper), SHARING its backing storage: a
    /// boxed <see cref="slice{T}"/> unwraps directly; any other implementer is reconstructed
    /// from its source array and window so writes through either view remain visible to both
    /// (Go's aliasing for `S ~[]E` values passed where `[]E` is expected).
    /// </summary>
    /// <param name="view">Slice view to share.</param>
    public slice(ISlice<T> view)
    {
        if (view is slice<T> other)
        {
            this = other;
            return;
        }

        // The full-window interface sub-slice of a golib implementer returns a BOXED slice<T>
        // over the same backing (a named-slice wrapper routes through its wrapped window) —
        // unbox it to share. `Source` cannot be used here: it materializes a detached copy.
        if (view.Slice((nint)0, view.Length) is slice<T> shared)
        {
            this = shared;
            return;
        }

        // Foreign implementer: a detached copy is the only option.
        m_array = AllocationCounter.CopyOf<T>(view.ToSpan());
        m_low = 0;
        m_length = m_array.Length;
        m_capacity = m_array.Length;
    }

    /// <summary>
    /// Creates a slice from a read-only byte-sequence view - the `[]byte(s)` conversion of a
    /// `string | []byte` union-constrained value (time format_rfc3339 parseUint). Matches Go
    /// exactly per instantiation: a boxed slice SHARES its backing (Go `[]byte([]byte)` is the
    /// same slice), while an @string (or foreign) sequence materializes a COPY (Go
    /// `[]byte(string)` copies).
    /// </summary>
    /// <param name="seq">Byte-sequence view.</param>
    public slice(IByteSeq<T> seq)
    {
        if (seq is slice<T> other)
        {
            this = other;
            return;
        }

        // `ꓸꓸꓸ` is the sequence's own window as a span, so this is ONE interface call and one
        // vectorized copy — the same shape ByteSeqExtensions.ToSlice already uses for the
        // constrained-generic route; these constructors are the boxed-interface route to the same
        // conversion. The element loop it replaces paid an interface dispatch and a bounds check
        // per element. The allocation count is unchanged: still exactly one charged copy.
        T[] copy = AllocationCounter.CopyOf<T>(seq.ꓸꓸꓸ);

        m_array = copy;
        m_low = 0;
        m_length = copy.Length;
        m_capacity = copy.Length;
    }

    // Every out-of-bounds throw in the two windowing constructors below raises Go's OWN
    // slice-bounds PANIC (RuntimeErrorPanic.SliceBoundsOutOfRange), not a .NET argument exception.
    // The distinction is behavioral, not cosmetic: a PanicException is recoverable by `recover()`
    // and is never contained by a host policy (Goroutine.CanContain excludes panics), so an
    // out-of-bounds window crashes Go-style with a Go-shaped message. A plain ArgumentException
    // satisfied the containment filter, so a converted test host swallowed it and recorded it on
    // the TestExecution — and when the dying goroutine was the one another goroutine awaited, the
    // record never flushed and the whole package deadline burned with NO output at all. 17 of
    // crypto/tls's 53 measured divergences presented that way (10 as an infrastructure-error line,
    // 7 as silent hangs) where Go would have failed loudly in milliseconds.
    public slice(T[]? array, nint low = 0, nint high = -1)
    {
        // Slicing a nil source is legal in Go while the indices stay within its zero
        // length/capacity (`nil[0:0]` is the nil slice); beyond that Go panics.
        if (array is null)
        {
            if (low != 0 || high > 0)
                throw RuntimeErrorPanic.SliceBoundsOutOfRange(low, high, 0, 0);

            this = default;
            return;
        }

        if (low < 0)
            throw RuntimeErrorPanic.SliceBoundsOutOfRange(low, high, array.Length, array.Length);

        if (high == -1)
            high = array.Length;

        nint length = high - low;

        if (array.Length - low < length)
            throw RuntimeErrorPanic.SliceBoundsOutOfRange(low, high, array.Length, array.Length);

        m_array = array;
        m_low = low;
        m_length = length;
        m_capacity = array.Length - low;
    }

    public slice(array<T> array, nint low = 0, nint high = -1) : this((T[])array, low, high) { }

    // Full-slice-expression view (Go s[low:high:max] over an existing backing array): max bounds the
    // CAPACITY (cap = max - low), which can end before the backing array does. The view SHARES the array.
    public slice(T[]? array, nint low, nint high, nint max)
    {
        // Same nil-source rule as above: legal while every index stays at zero.
        if (array is null)
        {
            if (low != 0 || high > 0 || max > 0)
                throw RuntimeErrorPanic.SliceBoundsOutOfRange(low, high, max, 0);

            this = default;
            return;
        }

        if (low < 0)
            throw RuntimeErrorPanic.SliceBoundsOutOfRange(low, high, max, array.Length);

        // A ZERO-SIZE element type has no backing to bound against — its array is the shared
        // zerobase placeholder (GoZeroSizeFacts) whose length says nothing about the slice — so the
        // `max > array.Length` arm is dropped there. The Go bound it stands in for, `max <= cap(s)`,
        // is enforced by the caller that HAS a capacity: Reslice checks it against m_capacity, and
        // the array/@string slice extensions check it against their own window before reaching here.
        if (high < low || max < high || (max > array.Length && !GoZeroSizeFacts<T>.IsZeroSize))
            throw RuntimeErrorPanic.SliceBoundsOutOfRange(low, high, max, array.Length);

        m_array = array;
        m_low = low;
        m_length = high - low;
        m_capacity = max - low;
    }

    public slice(nint length, nint capacity = -1, nint low = 0)
    {
        // This is the `make([]T, len[, cap])` path: Go panics RECOVERABLY for a negative or
        // over-allocatable length/capacity ("runtime error: makeslice: len/cap out of range");
        // the .NET ArgumentOutOfRange/OverflowException raised here before could not be caught
        // by recover(). .NET's heap ceiling for a T[] is Array.MaxLength.
        //
        // ZERO-SIZE ELEMENTS HAVE NO CEILING, because they have nothing to allocate: Go's makeslice
        // multiplies the element size by the capacity and compares THAT against maxAlloc, so a
        // `make([]struct{}, math.MaxInt)` computes 0 bytes and succeeds. The Array.MaxLength tests
        // below are golib's allocation ceiling standing in for Go's memory ceiling, and applying
        // them to a type that allocates nothing invented a panic Go does not have (slices'
        // TestRepeat and TestConcat_too_large both die on their OWN `make([]struct{}, MaxInt)`
        // before reaching the function under test). Only Go's own len/cap rules apply here.
        if (GoZeroSizeFacts<T>.IsZeroSize)
        {
            if (length < 0)
                throw RuntimeErrorPanic.MakeSliceLenOutOfRange();

            if (low < 0)
                throw new ArgumentOutOfRangeException(nameof(low), "Value is less than zero.");

            if (capacity <= 0)
                capacity = length;

            // The shared zerobase element stands in for the backing: non-null, so `== nil` is false
            // exactly as for any other `make`d slice, and never indexed — every window bound is
            // checked against m_length/m_capacity, which is all a zero-size slice has.
            m_array = GoZeroSizeFacts<T>.Storage;
            m_low = low;
            m_length = length;
            m_capacity = capacity - low;
            return;
        }

        if (length < 0 || length > Array.MaxLength)
            throw RuntimeErrorPanic.MakeSliceLenOutOfRange();

        if (capacity > Array.MaxLength)
            throw RuntimeErrorPanic.MakeSliceCapOutOfRange();

        if (low < 0)
            throw new ArgumentOutOfRangeException(nameof(low), "Value is less than zero.");

        if (capacity <= 0)
            capacity = length;

        m_array = AllocationCounter.NewArray<T>(capacity);
        m_low = low;
        m_length = length;
        m_capacity = capacity - low;
    }

    // A `make([]T, len[, cap])` whose ELEMENT zero value must itself be constructed, because
    // default(T) is not usable storage: a fixed-size array element (`[][hashSize]int` ->
    // slice<array<int>>, whose inner length exists only in the Go type, never in array<T>) or a
    // struct whose own zero value needs construction. The converter supplies the factory since
    // only it knows the element's shape; every other element type keeps the plain length ctor's
    // fill of default(T). Mirrors array<T>'s element-factory constructor.
    //
    // The WHOLE backing is filled, not just the first 'length' elements: Go zeroes the full
    // allocation, so the capacity beyond the length is already valid storage once a re-slice or
    // append exposes it.
    public slice(nint length, Func<T> elementFactory, nint capacity = -1, nint low = 0)
        : this(length, capacity, low)
    {
        for (int i = 0; i < m_array.Length; i++)
            m_array[i] = elementFactory();
    }

    public T[] Source => AllocationCounter.CopyOf<T>(ToSpan());

    public Span<T> ꓸꓸꓸ => ToSpan(); // Spread operator

    // Pinning is a MANAGED-storage concept: native memory does not move, so a native-backed slice
    // has nothing to pin and no PinnedBuffer to offer. Callers that want its address use the
    // element pointers (Ꮡ / SliceData), which answer the real one.
    internal PinnedBuffer buffer => m_nativeBase != 0
        ? throw new PanicException("native-backed slice: pinning is meaningless for native memory — take an element address instead")
        : new(m_array, Length);

    public nint Low => m_low;

    public nint High => m_low + m_length;

    // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
    public nint Length => m_length;

    public nint Capacity => m_capacity;

    public nint Available => m_capacity - m_length;

    // Returning by-ref value allows slice to be a struct instead of a class and still allow read and write
    // Allows for implicit index support: https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-8.0/ranges#implicit-index-support
    public ref T this[int index]
    {
        get
        {
            if (index < 0 || index >= m_length)
                throw RuntimeErrorPanic.IndexOutOfRange(index, m_length);

            // The managed path is the FIRST and only inline statement — measured: putting the
            // native branch (and its `unsafe` block) inline here cost PerfSieve +30% (110.5 →
            // 145.9/144.2 ms, two runs), because the method stopped being an inlinable array
            // access. The rare branch moves behind a NoInlining helper so the hot path JITs
            // exactly as it did before this arc. The zero-size gate is a folded per-T constant,
            // so it disappears entirely from every ordinary element type's codegen.
            if (GoZeroSizeFacts<T>.IsZeroSize)
                return ref ZeroSizeElementRef();

            if (m_nativeBase == 0)
                return ref m_array[m_low + index];

            return ref NativeElementRef(index);
        }
    }

    // The native element access, kept OUT of the indexers' inlinable bodies (see the measurement
    // above). Native-backed slices are rare by construction — one creation door, reached only
    // through unsafe.Slice over a native pointer — so the call costs nothing that matters.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private unsafe ref T NativeElementRef(nint index) => ref Unsafe.AsRef<T>(NativeElementPointer(index));

    // Every element of a zero-size slice IS the same element: Go computes `&s[i]` as
    // `data + i*0`, so each index names the one address the slice was built over — the runtime's
    // global `zerobase` for a `make`d one. Answering the single shared slot is that, exactly, and it
    // is what lets a slice whose length exceeds any array's still be indexed within its bounds. The
    // Go bounds check has already run in the caller; this only supplies the storage.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static ref T ZeroSizeElementRef() => ref GoZeroSizeFacts<T>.Storage[0];

    public ref T this[nint index]
    {
        get
        {
            if (index < 0 || index >= m_length)
                throw RuntimeErrorPanic.IndexOutOfRange(index, m_length);

            // Managed path first and inline (the int overload documents the measurement); the
            // native element IS the memory at the computed address, so reads observe the kernel's
            // writes and writes reach its pages (unmanaged T by construction, see OverNativeMemory).
            if (GoZeroSizeFacts<T>.IsZeroSize)
                return ref ZeroSizeElementRef();

            if (m_nativeBase == 0)
                return ref m_array[m_low + index];

            return ref NativeElementRef(index);
        }
    }

    public ref T this[ulong index] => ref this[(nint)index];

    // Go reslice expression s[low:high]: bounds are RELATIVE to this slice (which may itself be an
    // offset view over its backing array), a from-end index resolves against the slice LENGTH
    // (s[low..] == Go s[low:], high = len(s)), and the result SHARES the backing array — writes
    // through the sub-slice are visible through the original and vice versa. Go reslicing never copies.
    public slice<T> this[Range range]
    {
        get
        {
            nint low = range.Start.IsFromEnd ? m_length - range.Start.Value : range.Start.Value;
            nint high = range.End.IsFromEnd ? m_length - range.End.Value : range.End.Value;
            return Reslice(low, high, m_capacity);
        }
    }

    public slice<T> Slice(int start, int length)
    {
        return Reslice(start, start + length, m_capacity);
    }

    public slice<T> Slice(nint start, nint length)
    {
        return Reslice(start, start + length, m_capacity);
    }

    // Core of every Go slice expression over a slice: bounds are RELATIVE to this slice and checked
    // like Go (0 <= low <= high <= max <= cap(s)); the result is a shared view over the same backing
    // array with length high - low and capacity max - low. Reslicing a NIL slice (legal only at
    // 0:0:0 — the capacity bound excludes everything else) yields the nil slice: Go's result shares
    // the source's (nil) backing pointer, and `nil[0:0] == nil` is observably true. The old
    // `m_array ?? []` fallback laundered the nil backing into a fresh empty array, silently turning
    // a nil slice non-nil.
    internal slice<T> Reslice(nint low, nint high, nint max)
    {
        if (low < 0 || high < low || max < high || max > m_capacity)
            throw RuntimeErrorPanic.SliceBoundsOutOfRange(low, high, max, m_capacity);

        // Zero-size backing: the Go bound check above is the WHOLE check — there is no storage whose
        // extent could disagree with it — so the window is rebuilt directly. Going through the
        // array-taking constructor would have measured the shared zerobase placeholder's length
        // instead of the slice's capacity.
        if (GoZeroSizeFacts<T>.IsZeroSize)
            return new slice<T>(GoZeroSizeFacts<T>.Storage, m_low + low, m_low + high, m_low + max);

        // Native backing: same window arithmetic over the same base — reslicing never copies, so
        // the result aliases the identical memory and Ꮡ over it names the offset addresses.
        if (m_nativeBase != 0)
            return new slice<T>(m_nativeBase, m_low + low, m_low + high, m_low + max);

        if (m_array is null)
            return default;

        return new slice<T>(m_array, m_low + low, m_low + high, m_low + max);
    }

    /// <summary>
    /// Reports whether this slice's window <c>[0, len)</c> and <paramref name="other"/>'s share any
    /// element storage — Go's <c>slices.overlaps</c> and <c>crypto/internal/alias.AnyOverlap</c>,
    /// answered STRUCTURALLY (canonical backing identity + absolute index-range intersection) rather
    /// than by ordering element addresses.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Go writes both predicates as <c>uintptr(unsafe.Pointer(&amp;x[0])) &lt;= uintptr(unsafe.Pointer(&amp;y[len(y)-1]))
    /// &amp;&amp; …</c>, and the converter emits that literally as four <c>(uintptr)Ꮡ(…)</c> takes. Each take
    /// pins its backing through a finalizable holder on a box that is garbage the instant the take
    /// returns, so the pin is released by the FINALIZER, not by the next take — and a collection landing
    /// between two takes relocates an operand whose earlier pin has already been finalized, leaving the
    /// ordering to compare two heap layouts. Measured (2026-09-03, Release, tiering off, 4 cores, 16
    /// threads): the mirrored predicate tore five threads on ONE collection 17 s in, the converted
    /// <c>alias.AnyOverlap</c> answered TRUE for two distinct fresh arrays 9 s in, and the converted GCM
    /// <c>Open</c> raised <c>crypto/aes: invalid buffer overlap</c> 27 s in — the panic that killed the
    /// banked net/http row on two host classes. Backing identity and index ranges cannot tear.
    /// </para>
    /// <para>
    /// Arms: a zero-length side names no memory (Go ignores everything beyond the length); a zero-size
    /// element type never overlaps (Go's <c>elemSize == 0</c> early-out — load-bearing here, because
    /// every zero-size slice shares ONE static backing, <see cref="GoZeroSizeFacts{T}.Storage"/>); two
    /// native-backed windows compare their address ranges exactly; a managed window and a native one
    /// live in different spaces and never overlap; two managed windows overlap iff they share the
    /// canonical backing array and their absolute index ranges intersect.
    /// </para>
    /// </remarks>
    public bool Overlaps(slice<T> other)
    {
        if (m_length <= 0 || other.m_length <= 0)
            return false;

        if (GoZeroSizeFacts<T>.IsZeroSize)
            return false;

        if (m_nativeBase != 0 || other.m_nativeBase != 0)
        {
            if (m_nativeBase == 0 || other.m_nativeBase == 0)
                return false;

            nuint size = (nuint)Unsafe.SizeOf<T>();
            nuint thisStart = m_nativeBase + (nuint)m_low * size;
            nuint thisEnd = thisStart + (nuint)m_length * size;
            nuint otherStart = other.m_nativeBase + (nuint)other.m_low * size;
            nuint otherEnd = otherStart + (nuint)other.m_length * size;

            return thisStart < otherEnd && otherStart < thisEnd;
        }

        if (m_array is null || !ReferenceEquals(m_array, other.m_array))
            return false;

        nint thisLow = m_low, thisHigh = m_low + m_length;
        nint otherLow = other.m_low, otherHigh = other.m_low + other.m_length;

        return thisLow < otherHigh && otherLow < thisHigh;
    }

    public nint IndexOf(in T item)
    {
        // A zero-size type has exactly ONE value, so every element of a non-empty slice equals the
        // item and the first index is the answer. Walking a backing that does not exist would be
        // both wrong and unbounded.
        if (GoZeroSizeFacts<T>.IsZeroSize)
            return m_length > 0 ? 0 : -1;

        if (m_nativeBase != 0)
        {
            Span<T> span = ToSpan();
            EqualityComparer<T> comparer = EqualityComparer<T>.Default;

            for (int i = 0; i < span.Length; i++)
            {
                if (comparer.Equals(span[i], item))
                    return i;
            }

            return -1;
        }

        int index = Array.IndexOf(m_array, item, (int)m_low, (int)m_length);
        return index >= 0 ? index - m_low : -1;
    }

    public bool Contains(in T item)
    {
        return IndexOf(item) >= 0;
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        ToSpan().CopyTo(array.AsSpan(arrayIndex));
    }

    public T[] ToArray()
    {
        return Source;
    }

    public Span<T> ToSpan()
    {
        // A zero-size slice has no storage a span could view, and its LENGTH is a Go `int` — 64-bit —
        // while Span<T>.Length is int32. Both facts are handled here rather than pushed onto callers:
        // the window is materialized as real storage (every element is the zero value, which for a
        // fieldless type is the only value there is, so a fresh array is indistinguishable from an
        // alias), and a length no span can express raises Go's own recoverable panic rather than
        // silently truncating to a shorter one — a shorter span would make `append` produce a slice of
        // the wrong LENGTH, which is the one thing about a zero-size slice that IS observable.
        // golib's bulk operations over such a slice — copy, clear, append — never arrive here: each
        // answers from length arithmetic alone, which is what Go's own zero-size paths do.
        if (GoZeroSizeFacts<T>.IsZeroSize)
            return ZeroSizeSpan();

        // The design's §2.4 unification point: pay the backing discriminant ONCE per bulk
        // operation, then run flat. A native window's span is the mapping itself.
        if (m_nativeBase != 0)
        {
            unsafe
            {
                return new Span<T>(NativeElementPointer(0), (int)m_length);
            }
        }

        return new Span<T>(m_array, (int)m_low, (int)m_length);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private Span<T> ZeroSizeSpan()
    {
        if (m_length == 0)
            return Span<T>.Empty;

        if (m_length > Array.MaxLength)
            throw new PanicException($"runtime error: zero-size slice of length {m_length} exceeds the {Array.MaxLength}-element ceiling a managed span can address");

        return new Span<T>(AllocationCounter.NewArray<T>((int)m_length));
    }

    public slice<T> Append(T[] elems)
    {
        return Append(this, elems);
    }

    public slice<T> Clone()
    {
        return this; // a slice is a view over its backing array; copying the header is Go's s2 := s
    }

    /// <summary>
    /// Gets an allocation-free enumerator over the slice's (index, value) pairs — the shape a
    /// converted <c>for i, v := range s</c> binds.
    /// </summary>
    /// <remarks>
    /// The return type is the concrete <see cref="Enumerator"/> STRUCT, not <c>IEnumerator&lt;(nint, T)&gt;</c>.
    /// C#'s <c>foreach</c> binds <c>GetEnumerator</c> by pattern before it considers any interface, so
    /// every ranged loop in the converted corpus now enumerates with zero heap traffic. The previous
    /// signature returned an interface from an ITERATOR method, which cost two allocations on entry to
    /// every loop — the compiler-generated state machine plus the inner <c>SliceEnumerator</c> class —
    /// and made Go code that allocates nothing allocate per loop in C#. <see cref="Enumerator"/> still
    /// implements the interface, so an explicit <c>IEnumerator&lt;(nint, T)&gt;</c> consumer keeps
    /// working (it boxes, exactly as it did before); only the pattern path avoids the box.
    /// </remarks>
    public Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }

    /// <summary>
    /// Allocation-free (index, value) enumerator over a <see cref="slice{T}"/>.
    /// </summary>
    /// <remarks>
    /// Mutable struct by design — the <c>foreach</c> pattern copies it into a local and drives that
    /// copy, which is how the loop stays allocation-free. It implements
    /// <see cref="IEnumerator{T}"/> so interface-typed and LINQ consumers still bind; those paths box
    /// the struct, which is the same cost they always paid.
    /// </remarks>
    [Serializable]
    public struct Enumerator : IEnumerator<(nint, T)>
    {
        private readonly T[] m_array;
        private readonly nuint m_nativeBase; // 0 = managed backing; else the aliased memory's base
        private readonly nint m_start;
        private readonly nint m_end;
        private nint m_current;

        internal Enumerator(slice<T> slice)
        {
            // A nil slice (null m_array) enumerates as zero elements — Go's `range nil` — via the
            // zero start/end window; the backing array is never indexed.
            m_array = slice.m_array;
            m_nativeBase = slice.m_nativeBase;
            m_start = slice.m_low;
            m_end = m_start + slice.m_length;
            m_current = m_start - 1;
        }

        // The window offsets are element indices against whichever backing the slice carries, so
        // ranging a native-backed slice reads the mapping — the same discriminant the indexer uses
        // (design §2.3's table row for range).
        public readonly (nint, T) Current
        {
            get
            {
                // A zero-size element is the same value at every index (see ZeroSizeElementRef), and
                // its slice has no backing to read — `for range make([]struct{}, n)` is a counted
                // loop in Go and is one here.
                if (GoZeroSizeFacts<T>.IsZeroSize)
                    return (m_current - m_start, GoZeroSizeFacts<T>.Storage[0]);

                if (m_nativeBase != 0)
                {
                    unsafe
                    {
                        return (m_current - m_start,
                            Unsafe.Read<T>((void*)(m_nativeBase + (nuint)m_current * (nuint)Unsafe.SizeOf<T>())));
                    }
                }

                return (m_current - m_start, m_array[m_current]);
            }
        }

        readonly object? IEnumerator.Current => Current;

        public bool MoveNext()
        {
            if (m_current >= m_end)
                return false;

            m_current++;
            return m_current < m_end;
        }

        void IEnumerator.Reset()
        {
            m_current = m_start - 1;
        }

        public readonly void Dispose()
        {
        }
    }

    public override string ToString()
    {
        return $"[{string.Join(" ", ((IEnumerable<T>)this).Take(20))}{(Length > 20 ? " ..." : "")}]";
    }

    public override int GetHashCode()
    {
        // Backing identity: the managed array's, or the native base's — one or the other is always
        // the thing two headers share when they name the same storage.
        return (m_nativeBase != 0 ? m_nativeBase.GetHashCode() : m_array.GetHashCode()) ^ (int)m_low ^ (int)m_length;
    }

    public override bool Equals(object? obj)
    {
        return obj switch
        {
            slice<T> slice => Equals(slice),
            array<T> array => Equals(array),
            ISlice<T> slice => Equals(slice),
            IArray<T> array => Equals(array),
            ISlice slice => Equals(slice),
            IArray array => Equals(array),
            _ => false
        };
    }

    public bool Equals(ISlice? other)
    {
        IStructuralEquatable equatable = Source;
        return equatable.Equals(other?.Source, EqualityComparer<object[]>.Default);
    }

    public bool Equals(ISlice<T>? other)
    {
        IStructuralEquatable equatable = Source;
        return equatable.Equals(other?.Source, EqualityComparer<T[]>.Default);
    }

    public bool Equals(slice<T> other)
    {
        IStructuralEquatable equatable = Source;
        return equatable.Equals(other.Source, EqualityComparer<T[]>.Default);
    }

    public bool Equals(IArray? other)
    {
        IStructuralEquatable equatable = Source;
        return equatable.Equals(other?.Source, EqualityComparer<object[]>.Default);
    }

    public bool Equals(IArray<T>? other)
    {
        IStructuralEquatable equatable = Source;
        return equatable.Equals(other?.Source, EqualityComparer<T[]>.Default);
    }

    #region [ Operators ]

    // Enable implicit conversions between slice<T> and T[]
    public static implicit operator slice<T>(T[] value)
    {
        return new slice<T>(value);
    }

    public static implicit operator slice<T>(Span<T> value)
    {
        return new slice<T>(value);
    }

    public static implicit operator slice<T>(ReadOnlySpan<T> value)
    {
        return new slice<T>(value);
    }

    public static implicit operator slice<T>(Memory<T> value)
    {
        return new slice<T>(value);
    }

    public static implicit operator slice<T>(ReadOnlyMemory<T> value)
    {
        return new slice<T>(value);
    }

    public static implicit operator T[](slice<T> value)
    {
        return value.ToArray();
    }

    // Enable implicit conversions between slice<T> and array<T>
    public static implicit operator slice<T>(array<T> value)
    {
        return new slice<T>(value);
    }

    public static implicit operator array<T>(slice<T> value)
    {
        return new array<T>(value.ToArray());
    }

    // slice<T> to slice<T> comparisons — HEADER identity (same backing array reference, window and
    // capacity), matching Go's slice header. Go forbids comparing two slices, so the only converted
    // code that binds this operator is the nil comparison `s == nil`, emitted as `s == default!`
    // (the nil literal renders `default!` in value contexts): header identity against the default
    // header (null, 0, 0, 0) is exactly Go's nil test, TRUE only for a genuinely nil slice — a
    // zero-length view over a real array (`s[:0]`, `[]byte{}`, `[]byte("")`) stays non-nil, which
    // Go distinguishes observably (bytes TestTrim/TestClone). Structural content equality remains
    // on the Equals overloads for C#-side collection use.
    public static bool operator ==(slice<T> a, slice<T> b)
    {
        // The native base joins the header comparison: two native windows are the same slice when
        // they name the same base and window, and a native-backed slice is never == a managed one
        // (nor == nil, whose base is 0 with an empty backing — the nil test stays exact).
        return a.m_nativeBase == b.m_nativeBase && ReferenceEquals(a.m_array, b.m_array) &&
               a.m_low == b.m_low && a.m_length == b.m_length && a.m_capacity == b.m_capacity;
    }

    public static bool operator !=(slice<T> a, slice<T> b)
    {
        return !(a == b);
    }

    // slice<T> to ISlice comparisons
    public static bool operator ==(ISlice? a, slice<T> b)
    {
        return a?.Equals(b) ?? false;
    }

    public static bool operator !=(ISlice? a, slice<T> b)
    {
        return !(a == b);
    }

    public static bool operator ==(slice<T> a, ISlice? b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(slice<T> a, ISlice? b)
    {
        return !(a == b);
    }

    // slice<T> to nil comparisons — REPRESENTATION nilness: a nil slice is exactly the default
    // header (null backing array). Go distinguishes nil from a non-nil empty slice (`[]byte{}`,
    // `make([]T, 0)`, `s[:0]` — all non-nil with a real backing array), and the previous
    // `Length == 0 && Capacity == 0` test misclassified every zero-length zero-capacity view
    // (`s[len(s):]` of a full slice, an empty literal) as nil. Every golib construction path
    // maintains the invariant nil ⟺ m_array is null — see the nil-vs-empty identity enumeration
    // in docs/ConversionStrategies-Reference.md.
    public static bool operator ==(slice<T> slice, NilType _)
    {
        return slice.m_array is null;
    }

    public static bool operator !=(slice<T> slice, NilType nil)
    {
        return !(slice == nil);
    }

    public static bool operator ==(NilType nil, slice<T> slice)
    {
        return slice == nil;
    }

    public static bool operator !=(NilType nil, slice<T> slice)
    {
        return slice != nil;
    }

    public static implicit operator slice<T>(NilType _)
    {
        return default;
    }

    #endregion

    #region [ Interface Implementations ]

    object ICloneable.Clone()
    {
        return MemberwiseClone();
    }

    ISlice ISlice.Append(object[] elems)
    {
        // The Cast/ToArray materialization is this method's own allocation, distinct from whatever
        // Append then allocates to grow; the LINQ iterator behind it is BCL-internal and uncharged.
        AllocationCounter.Count();
        return Append(elems.Cast<T>().ToArray());
    }

    ISlice<T> ISlice<T>.Append(params T[] elems)
    {
        return Append(elems);
    }

    ISlice<T> ISlice<T>.this[Range range] => this[range];

    // IByteSeq<slice<T>, T> — models Go's `string | []byte` union constraint. Length implicitly
    // implements IByteSeq.Length, and the public slice<T> range indexer implicitly implements the
    // self-referential IByteSeq<slice<T>, T>.this[Range] — so a generic body's sub-slice stays a
    // slice<T> instead of boxing into the interface. Only the element indexer needs an explicit
    // form, to expose slice<T>'s `ref T` indexer as the interface's by-value read.
    T IByteSeq<T>.this[nint index] => this[index];

    ISlice<T> ISlice<T>.Slice(int start, int length) => Slice(start, length);

    ISlice<T> ISlice<T>.Slice(nint start, nint length) => Slice(start, length);

    Array IArray.Source => Source;

    object? IArray.this[nint index]
    {
        get => this[index];
        set => this[index] = (T)value!;
    }

    T IList<T>.this[int index]
    {
        get => this[index];
        set
        {
            if (index < 0 || index >= m_length)
                throw new ArgumentOutOfRangeException(nameof(index));

            // The IList<T> setter reaches the same storage the Go indexer does — a native window
            // writes the mapping (design §2.3).
            this[(nint)index] = value;
        }
    }

    int IList<T>.IndexOf(T item)
    {
        return (int)IndexOf(item);
    }

    void IList<T>.Insert(int index, T item)
    {
        throw new NotSupportedException();
    }

    void IList<T>.RemoveAt(int index)
    {
        throw new NotSupportedException();
    }

    int IReadOnlyCollection<T>.Count => (int)m_length;

    T IReadOnlyList<T>.this[int index] => this[index]; // the ref indexer is already m_low-relative

    bool ICollection<T>.IsReadOnly => false;

    int ICollection<T>.Count => (int)m_length;

    void ICollection<T>.Add(T item)
    {
        throw new NotSupportedException();
    }

    bool ICollection<T>.Contains(T item)
    {
        return Contains(item);
    }

    void ICollection<T>.Clear()
    {
        throw new NotSupportedException();
    }

    bool ICollection<T>.Remove(T item)
    {
        throw new NotSupportedException();
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return new SliceEnumerator(this);
    }

    // IArray<T> derives from IEnumerable<(nint, T)>, which the public GetEnumerator() satisfied
    // implicitly while it returned the interface. It now returns the concrete struct, so the
    // interface contract needs this explicit member — the boxing path, taken only by a consumer that
    // asked for the interface.
    IEnumerator<(nint, T)> IEnumerable<(nint, T)>.GetEnumerator()
    {
        return new Enumerator(this);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return new SliceEnumerator(this);
    }

    [Serializable]
    private sealed class SliceEnumerator : IEnumerator<T>
    {
        private readonly T[] m_array;
        private readonly nuint m_nativeBase; // 0 = managed backing (see the struct Enumerator)
        private readonly nint m_start;
        private readonly nint m_end;
        private nint m_current;

        internal SliceEnumerator(slice<T> slice)
        {
            // A nil slice (null m_array) enumerates as zero elements — Go's `range nil` — via the
            // zero start/end window; the backing array is never indexed. (Non-nil ⟺ m_array is
            // present, so the old corrupt-state guard here had no remaining reachable case.)
            m_array = slice.m_array;
            m_nativeBase = slice.m_nativeBase;
            m_start = slice.m_low;
            m_end = m_start + slice.m_length;
            m_current = m_start - 1;
        }

        public bool MoveNext()
        {
            if (m_current >= m_end)
                return false;

            m_current++;
            return m_current < m_end;
        }

        public T Current
        {
            get
            {
                if (m_current < m_start)
                    throw new InvalidOperationException("slice enumeration not started.");

                if (m_current >= m_end)
                    throw new InvalidOperationException("slice enumeration has ended.");

                // Zero-size elements: the shared value, never a backing read (see the struct
                // Enumerator's Current).
                if (GoZeroSizeFacts<T>.IsZeroSize)
                    return GoZeroSizeFacts<T>.Storage[0];

                if (m_nativeBase != 0)
                {
                    unsafe
                    {
                        return Unsafe.Read<T>((void*)(m_nativeBase + (nuint)m_current * (nuint)Unsafe.SizeOf<T>()));
                    }
                }

                return m_array[m_current];
            }
        }

        object? IEnumerator.Current => Current;

        void IEnumerator.Reset()
        {
            m_current = m_start - 1;
        }

        public void Dispose()
        {
        }
    }

    #endregion

    /// <inheritdoc />
    /// <summary>
    /// Wraps an existing slice as itself — the <see cref="ISliceWrap{TSelf, T}"/> identity for
    /// the base slice type (named-slice wrappers wrap the window in their own type instead).
    /// </summary>
    /// <param name="source">Slice window to wrap.</param>
    /// <returns>The same window.</returns>
    public static slice<T> Wrap(in slice<T> source)
    {
        return source;
    }

    public static slice<T> Make(nint p1 = 0, nint p2 = -1)
    {
        return new slice<T>(p1, p2);
    }

    public static slice<T> From<TSource>(TSource[]? array)
    {
        if (array is null)
            return new slice<T>(Array.Empty<T>());

        if (array is T[] baseTypeArray)
            return new slice<T>(baseTypeArray);

        baseTypeArray = AllocationCounter.NewArray<T>(array.Length);

        for (int i = 0; i < array.Length; i++)
            baseTypeArray[i] = (T)TypeExtensions.ConvertToType((IConvertible)array[i]!);

        return baseTypeArray;
    }

    // ReadOnlySpan, not Span: this body only READS elems (Length, CopyTo, elems[0]), and the wider
    // parameter is what lets the constrained `append` over a ReadOnlySpan reach it directly instead
    // of materializing an array to get here. A Span argument converts implicitly, so every existing
    // caller binds unchanged — and widening the EXISTING overload rather than adding one beside it
    // is deliberate: two params-span overloads would put an ambiguity (CS0121) in front of every
    // collection-expression call site in the corpus, which is a bad trade for a parameter type.
    public static slice<T> Append(in slice<T> slice, params ReadOnlySpan<T> elems)
    {
        // Go's append with nothing to add returns s ITSELF (no growth is needed, so the same
        // header comes back): nil stays nil and a non-nil empty stays that same non-nil empty —
        // `append([]byte{}, b...)` with an empty b (bytes.Clone) must return the non-nil literal,
        // never launder identity through a fresh allocation.
        if (elems.Length == 0)
            return slice;

        T[] newArray;

        // Appending zero-size elements moves no bytes — Go's own growslice special-cases
        // `et.Size_ == 0` to bump the length and return the same (zerobase) data pointer — so the
        // whole operation is the length/capacity arithmetic. Doing it here rather than falling
        // through keeps a `make([]struct{}, n)` slice from ever needing storage it has no reason to
        // own, and keeps the growth rule identical to the backed path so `cap` still agrees with Go
        // wherever Go's own answer is observable.
        if (GoZeroSizeFacts<T>.IsZeroSize)
        {
            nint zeroSizeLength = slice.m_length + elems.Length;

            // Within capacity: the same window, one longer — the in-place arm, with no place to write.
            if (slice != nil && elems.Length <= slice.Available)
                return new slice<T>(GoZeroSizeFacts<T>.Storage, slice.m_low, slice.m_low + zeroSizeLength, slice.m_low + slice.m_capacity);

            // Nil source or beyond capacity: Go reallocates and DETACHES, so the result starts at
            // offset 0 exactly as the backed path's fresh array does.
            nint zeroSizeCapacity = slice == nil ? elems.Length : CalculateNewCapacity(slice, zeroSizeLength);

            return new slice<T>(GoZeroSizeFacts<T>.Storage, 0, zeroSizeLength, zeroSizeCapacity);
        }

        if (slice == nil)
        {
            newArray = AllocationCounter.NewArray<T>(elems.Length);
            elems.CopyTo(newArray);
            return new slice<T>(newArray);
        }

        if (elems.Length <= slice.Available)
        {
            // Within capacity: Go appends IN PLACE into the shared backing — the writes are
            // visible to every slice sharing it — and the result is the same view, one longer.
            // A native backing appends into the mapping itself: the capacity window over native
            // memory is storage like any other (design §2.3).
            if (slice.m_nativeBase != 0)
            {
                unsafe
                {
                    elems.CopyTo(new Span<T>(slice.NativeElementPointer(slice.m_length), elems.Length));
                }

                return new slice<T>(slice.m_nativeBase, slice.m_low, slice.High + elems.Length, slice.m_low + slice.m_capacity);
            }

            // Single-element append is the dominant Go idiom — store directly instead of span-copying.
            if (elems.Length == 1)
                slice.m_array[slice.High] = elems[0];
            else
                elems.CopyTo(new Span<T>(slice.m_array, (int)slice.High, elems.Length));

            return new slice<T>(slice.m_array, slice.m_low, slice.High + elems.Length, slice.m_low + slice.m_capacity);
        }

        // Beyond capacity: reallocate and DETACH from the original backing, like Go — and for a
        // native backing this is the design's §2.3 answer verbatim: the new backing is MANAGED,
        // writes through the grown slice stop reaching the mapping, and the original slice still
        // aliases it. The mapping plays the role of "the old array" in Go's own spec.
        nint newCapacity = CalculateNewCapacity(slice, slice.Length + elems.Length);
        newArray = AllocationCounter.NewArray<T>(newCapacity);

        slice.ToSpan().CopyTo(newArray);
        elems.CopyTo(newArray.AsSpan((int)slice.Length));

        return new slice<T>(newArray, 0, slice.Length + elems.Length);
    }

    // The SLICE-SHAPED SPREAD route (the Span int32-ceiling arc): `append(s, t...)` arrives with
    // the operand AS THE SLICE IT IS instead of projected through `.ꓸꓸꓸ` to a Span at the call
    // boundary. The window's own span still serves the copy wherever a span can express it — a
    // managed backing is int-bounded by T[] by construction — so the allocation count and the
    // in-place/grow behavior are byte-identical to the span core; what changes is the boundary:
    // lengths stay nint end to end, and a window no span can express (a native backing past the
    // int ceiling) meets Go's own `len out of range` panic from the growth arithmetic instead of
    // an ArgumentOutOfRange from span construction. A FOREIGN ISlice<T> (a named-slice wrapper
    // reaching here through the boxed interface) has no zero-copy window to lend and is copied
    // element-by-element through its ref indexer — grow-once, allocation-exact, nint-indexed.
    public static slice<T> Append(in slice<T> slice, ISlice<T>? elems)
    {
        if (elems is null)
            return slice;

        if (elems is slice<T> window)
        {
            // Zero-size elements move no bytes, and their windows are the one place a LENGTH can
            // legitimately exceed every span (`make([]struct{}, math.MaxInt)` is legal Go —
            // slices' own TestConcat_too_large drives exactly these through Concat's Grow): route
            // by arithmetic before any span form, mirroring the span core's own zero-size arm.
            if (GoZeroSizeFacts<T>.IsZeroSize)
            {
                nint zeroAdded = window.m_length;

                if (zeroAdded == 0)
                    return slice;

                nint zeroLength = slice.m_length + zeroAdded;

                if (slice != nil && zeroAdded <= slice.Available)
                    return new slice<T>(GoZeroSizeFacts<T>.Storage, slice.m_low, slice.m_low + zeroLength, slice.m_low + slice.m_capacity);

                nint zeroCapacity = slice == nil ? zeroAdded : CalculateNewCapacity(slice, zeroLength);

                // The growth rule's even-rounding wraps past MaxInt for the giant lengths only
                // zero-size elements can reach — exact need is the honest capacity there.
                if (zeroCapacity < zeroLength)
                    zeroCapacity = zeroLength;

                return new slice<T>(GoZeroSizeFacts<T>.Storage, 0, zeroLength, zeroCapacity);
            }

            if (window.m_nativeBase == 0 || window.m_length <= int.MaxValue)
                return Append(slice, window.ToSpan());

            // A native window past the span ceiling cannot land in any managed backing either —
            // Go's growslice answers the same impossible request with this panic.
            throw new PanicException("runtime error: growslice: len out of range");
        }

        // A named-slice wrapper (or any foreign ISlice<T>): its own spread property is the same
        // zero-copy window projection the call boundary used to make — one interface call, now
        // inside the core, where the slice-shaped fast path above no longer pays it. Managed
        // backings are int-bounded by construction, so the span expresses every wrapper window
        // and the allocation count is byte-identical to the old boundary's.
        return Append(slice, (ReadOnlySpan<T>)elems.ꓸꓸꓸ);
    }

    public static slice<T> Append(in slice<T> slice, params T[] elems)
    {
        return Append(slice, elems.AsSpan());
    }

    private static nint CalculateNewCapacity(in slice<T> slice, nint neededCapacity)
    {
        nint capacity = slice.Capacity;

        if (capacity > 1 && capacity % 2 != 0)
            capacity++;

        nint doubleCapacity = capacity + capacity;

        if (neededCapacity > doubleCapacity)
        {
            if (neededCapacity % 2 != 0)
                neededCapacity++;

            capacity = neededCapacity;
        }
        else
        {
            if (slice.Length < 1024)
            {
                capacity = doubleCapacity;
            }
            else
            {
                while (capacity < neededCapacity)
                    capacity += capacity / 4;
            }
        }

        return capacity;
    }
}

public static class SliceExtensions
{
    // slice of a slice helper function — bounds are RELATIVE to the slice (Go semantics; the source
    // slice may itself be an offset view over its backing array), defaults are Go's (missing high =
    // len(s), missing max = cap(s)), and the result SHARES the backing array:
    //      s = s[2:]    => s = s.slice(2)
    //      s = s[3:5]   => s = s.slice(3, 5);
    //      s = s[:4]    => s = s.slice(high:4)
    //      s = s[1:3:5] => s = s.slice(1, 3, 5) // Full slice expression
    public static slice<T> slice<T>(this in slice<T> slice, nint low = -1, nint high = -1, nint max = -1)
    {
        return slice.Reslice(low == -1 ? 0 : low, high == -1 ? slice.Length : high, max == -1 ? slice.Capacity : max);
    }

    // slice of a C# array helper function — Go slicing always produces a SHARED view over the array
    // (never a copy); max (the full-slice-expression bound) restricts capacity below the array's end
    public static slice<T> slice<T>(this T[] array, nint low = -1, nint high = -1, nint max = -1)
    {
        if (low == -1)
            low = 0;

        if (high == -1)
            high = array.Length;

        if (max == -1)
            max = array.Length;

        if (low < 0 || high < low || max < high || max > array.Length)
            throw RuntimeErrorPanic.SliceBoundsOutOfRange(low, high, max, array.Length);

        return new slice<T>(array, low, high, max);
    }

    // slice from a Span helper function — in practice the one emission for a VARIADIC parameter's
    // incoming pack: the converter renders `func f(xs ...T)` as `params ꓸꓸꓸT xsʗp` over a Span<T>
    // and opens the body with `var xs = xsʗp.slice();` (visitFuncDecl / convFuncLit).
    //
    // NIL-NESS CROSSES THIS BOUNDARY, and used to be lost here. Go's zero-argument variadic call
    // materializes the NIL slice, not an empty one — `func cSeq(o ...uintptr) …; cSeq()` gives
    // `o == nil` — and Go code reads that difference back (`o == nil`, and reflect.DeepEqual, which
    // separates nil from empty; unique/clone_test does exactly this). The copy below laundered it:
    // ReadOnlySpan<T>.ToArray() answers Array.Empty<T>() for an empty span, and a NON-NULL backing
    // array is precisely what makes a slice<T> non-nil, so every zero-argument variadic call — and
    // every spread of a nil slice — produced `[]T{}` where Go produces nil.
    //
    // The discriminant is the span's DATA REFERENCE, which is null exactly when Go's slice header's
    // data pointer is nil. That is not a new rule: it is the same one slice<T> itself uses
    // (`m_array is null` ⟺ nil), read one level down. Measured across every shape that reaches here:
    //
    //   zero-argument variadic call      -> null ref   (Roslyn passes default(Span<T>))   Go: nil
    //   spread of a nil slice            -> null ref   (ToSpan of a null backing)         Go: nil
    //   spread of []T{} / make([]T, 0)   -> real ref   (Array.Empty<T> is an object)      Go: non-nil
    //   spread of x[:0] or x[2:2]        -> real ref   (interior of a real array)         Go: non-nil
    //   any non-empty pack               -> real ref                                      Go: non-nil
    //
    // Roslyn's choice of default(Span<T>) for an empty params pack is not language-guaranteed, but
    // the failure mode if it ever changes is a graceful one: the pack would carry a real reference
    // and answer non-nil — today's pre-fix behavior — never a crash and never a nil where Go has
    // storage. The spread half does not depend on the compiler at all; it is golib's own ToSpan.
    //
    // ⚠ One documented gap, from ToSpan rather than from here: a ZERO-SIZE element type (`[]struct{}`)
    // spans as Span<T>.Empty when the length is 0, which has a null reference, so an empty NON-nil
    // slice of a zero-size type reads as nil across a spread. Nothing in the corpus observes it.
    public static slice<T> slice<T>(this Span<T> source, nint low = -1, nint high = -1, nint max = -1)
    {
        // Route the nil case through slice<T>'s own re-slicer rather than the copy: it already
        // answers Go's `nil[0:0] == nil` and panics on any bound that leaves the nil header.
        if (source.Length == 0 && Unsafe.IsNullRef(ref MemoryMarshal.GetReference(source)))
            return default(slice<T>).slice(low, high, max);

        return AllocationCounter.CopyOf<T>(source).slice(low, high, max);
    }

    // slice of an enumerable helper function
    public static slice<T> slice<T>(this IEnumerable<T> source, nint low = -1, nint high = -1, nint max = -1)
    {
        // Enumerable.ToArray's result is charged; its discarded growth buffers are BCL-internal.
        AllocationCounter.Count();
        return source.ToArray().slice(low, high, max);
    }

    // slice of a Go array helper function — bounds are RELATIVE to the array's own WINDOW, and the
    // window is what bounds them. Every ordinary array spans the whole of its backing store, where
    // that is exactly the T[] overload above; an ALIAS window (array<T>.Alias for `(*[N]T)(s)`,
    // array<T>.AliasPointer for `(*[N]T)(unsafe.Pointer(&s[i]))`) starts at Low, so slicing it
    // through the raw backing addressed the SOURCE's elements rather than the array's — `p[1:3]` of
    // a window over `buf[1:]` yielded buf[1:3] instead of buf[2:4]. The Range indexer already
    // resolved through the window (array<T>.Slice); this is the explicit-bounds path, which the
    // `(*[N]T)(unsafe.Pointer(p))[:n:n]` idiom reaches.
    public static slice<T> slice<T>(this array<T> array, nint low = -1, nint high = -1, nint max = -1)
    {
        nint offset = array.Low;

        if (offset == 0 && array.Length == array.m_array.Length)
            return array.m_array.slice(low, high, max);

        nint start = low == -1 ? 0 : low;
        nint end = high == -1 ? array.Length : high;
        nint bound = max == -1 ? array.Length : max;

        if (start < 0 || end < start || bound < end || bound > array.Length)
            throw RuntimeErrorPanic.SliceBoundsOutOfRange(start, end, bound, array.Length);

        return new slice<T>(array.m_array, offset + start, offset + end, offset + bound);
    }

    // slice of a Go string helper function — bounds are relative to the string's own WINDOW, for the
    // same reason the array<T> overload above bounds them by the array's (see @string.SliceBounds).
    public static slice<byte> slice(this @string source, nint low = -1, nint high = -1, nint max = -1)
    {
        return source.SliceBounds(low, high, max);
    }
}
