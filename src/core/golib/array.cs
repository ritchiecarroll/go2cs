// array.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming

using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using go.golib;

namespace go;

public interface IArray : IEnumerable, ICloneable
{
    Array? Source { get; }

    nint Length { get; }

    object? this[nint index] { get; set; }

    public bool IndexIsValid(nint index)
    {
        return index > -1 && index < Length;
    }
}

public interface IArray<T> : IArray, IEnumerable<(nint, T)>
{
    new T[] Source { get; }
    
    new ref T this[nint index] { get; }

    Span<T> ꓸꓸꓸ { get; }

    Span<T> ToSpan();
}

[Serializable]
public readonly struct array<T> : IArray<T>, IList<T>, IReadOnlyList<T>, IEquatable<IArray>, IGoZeroShaped
{
    internal readonly T[] m_array;

    // The WINDOW this array occupies inside m_array. Every ordinary construction spans the whole
    // backing (m_low = 0, m_length = m_array.Length) — the window exists ONLY so that Go's two
    // array-POINTER conversions can ALIAS existing storage instead of snapshotting it: the slice
    // form `(*[N]T)(s)` (see Alias) and the element-pointer form `(*[N]T)(unsafe.Pointer(&s[i]))`
    // (see AliasPointer). Reads and writes therefore go through m_low/m_length, never through
    // m_array's own bounds.
    private readonly int m_low;
    private readonly int m_length;

    public array()
    {
        m_array = [];
        m_length = 0;
    }

    public array(int length)
    {
        m_array = AllocationCounter.NewArray<T>(length);
        m_length = length;
    }

    public array(nint length)
    {
        m_array = AllocationCounter.NewArray<T>(length);
        m_length = (int)length;
    }

    public array(ulong length)
    {
        m_array = AllocationCounter.NewArray<T>(length);
        m_length = (int)length;
    }

    // Fixed-size array whose ELEMENT zero value must itself be constructed, because default(T)
    // is not usable storage: a nested fixed array (`[2][4]byte` -> array<array<byte>>, whose
    // inner length exists only in the Go type, never in array<T>) or a struct whose own zero
    // value needs construction. The converter supplies the factory since only it knows the
    // element's shape; every other element type keeps the plain length ctors' fill of default(T).
    //
    // Mirrors the int/nint/ulong length overloads above: a Go array length can be a NAMED integer
    // constant (`quant [nQuantIndex][blockSize]byte` - image/jpeg), which reaches this ctor as its
    // wrapper's underlying nint rather than an int (CS1503 against an int-only overload).
    public array(nint length, Func<T> elementFactory)
    {
        m_array = AllocationCounter.NewArray<T>(length);
        m_length = (int)length;

        for (nint i = 0; i < length; i++)
            m_array[i] = elementFactory();
    }

    public array(int length, Func<T> elementFactory) : this((nint)length, elementFactory)
    {
    }

    public array(ulong length, Func<T> elementFactory) : this((nint)length, elementFactory)
    {
    }

    public array(T[]? array)
    {
        m_array = array ?? [];
        m_length = m_array.Length;
    }

    // Go 1.20 slice-to-array VALUE conversion (`[4]byte(s)`): copies exactly 'length' elements,
    // panicking like Go when the slice is too short. The POINTER form (`(*[4]byte)(s)`, Go 1.17)
    // is a different conversion with different semantics and takes Alias below.
    public array(slice<T> source, nint length)
    {
        checkArrayConversionLength(source.Length, length);

        m_array = AllocationCounter.CopyOf<T>(source.ToSpan()[..(int)length]);
        m_length = (int)length;
    }

    // Private window constructor — see m_low/m_length and Alias.
    private array(T[] backing, int low, int length)
    {
        m_array = backing;
        m_low = low;
        m_length = length;
    }

    /// <summary>
    /// Go's slice-to-array-POINTER conversion, <c>(*[N]T)(s)</c> (Go 1.17): an array that ALIASES
    /// the slice's own backing storage rather than a snapshot of it.
    /// </summary>
    /// <param name="source">Slice whose storage the array addresses.</param>
    /// <param name="length">Go array length <c>N</c>; the slice must be at least this long.</param>
    /// <returns>An <c>array&lt;T&gt;</c> window over <paramref name="source"/>'s first
    /// <paramref name="length"/> elements.</returns>
    /// <remarks>
    /// <para>
    /// The distinction from the <c>array(slice&lt;T&gt;, nint)</c> constructor is Go's, not an
    /// implementation detail: <c>[N]T(s)</c> yields a VALUE (a copy) while <c>(*[N]T)(s)</c> yields
    /// a POINTER INTO <c>s</c> — "the slice and array share their underlying array" (Go spec,
    /// Conversions from slice to array or array pointer). A copy behind the pointer form is not a
    /// performance detail but a silent wrong answer: every write through the pointer is discarded.
    /// image/png's encoder is the corpus witness — its <c>cbTCA8</c> row loop converts each 4-byte
    /// destination window with <c>d := (*[4]byte)(dst)</c> and writes the un-premultiplied pixel
    /// through <c>d</c>, so against a copy it emitted an all-zero image for every non-opaque RGBA
    /// source (TestWriteRGBA).
    /// </para>
    /// <para>
    /// A window never escapes into an ordinary array: <see cref="Clone"/> (Go's by-value array copy)
    /// materializes the window's own storage, so the copy is a full, offset-free array again.
    /// </para>
    /// </remarks>
    public static array<T> Alias(slice<T> source, nint length)
    {
        checkArrayConversionLength(source.Length, length);

        // A nil/zero slice with length 0 has no backing to window — the empty array is the whole
        // of what the conversion can address, and its (absent) storage is shared vacuously.
        return source.m_array is null
            ? new array<T>([], 0, 0)
            : new array<T>(source.m_array, (int)source.Low, (int)length);
    }

    /// <summary>
    /// Go's element-pointer-to-array reinterpret, <c>(*[N]T)(unsafe.Pointer(p))</c> where <c>p</c>
    /// is a <c>*T</c>: an array pointer that ALIASES the storage <paramref name="element"/> is an
    /// element of, rather than a snapshot of it.
    /// </summary>
    /// <param name="element">Pointer to the element the array starts at.</param>
    /// <param name="length">Go array length <c>N</c>.</param>
    /// <returns>A pointer to an <c>array&lt;T&gt;</c> window that begins at
    /// <paramref name="element"/>, when it addresses managed array/slice storage; otherwise a
    /// pointer over its raw address.</returns>
    /// <remarks>
    /// <para>
    /// This is the same aliasing requirement <see cref="Alias"/> answers for the slice form, reached
    /// from a POINTER instead: Go's array is a view of the bytes at <c>p</c>, so a write through it
    /// must land in the caller's buffer. os's <c>TestReadStdin</c> is the corpus witness — its
    /// <c>poll.ReadConsole</c> fake fills internal/poll's read buffer with
    /// <c>copy((*[10000]uint16)(unsafe.Pointer(buf))[:n:n], s16)</c>, and against a snapshot every
    /// one of the test's 462 subtests read back zeros.
    /// </para>
    /// <para>
    /// <paramref name="length"/> is CLAMPED to the storage that actually exists from the element.
    /// Go's <c>N</c> in this idiom is a promise ("at least this many"), not a length — the huge
    /// constants it is spelled with (<c>10000</c>, <c>1&lt;&lt;16</c>, <c>0xffff</c>) never describe
    /// a real allocation, and the result is always immediately re-sliced to the real count. Honoring
    /// <c>N</c> literally would put the window's own bounds past its backing store, where a
    /// full-slice expression is an <c>ArgumentException</c> rather than the window Go asks for; a
    /// clamped window addresses exactly the storage that is there, so an overrun surfaces as a
    /// Go-style index panic instead of the silent corruption the same overrun is in Go.
    /// </para>
    /// <para>
    /// A pointer with no managed element storage behind it — a heap box, a struct field, a native
    /// address — keeps the raw-address route: no <c>T[]</c> exists to window, and an
    /// <c>array&lt;T&gt;</c> can neither view native memory nor be fabricated from a scalar's bytes.
    /// That is the raw-metal fork, unchanged here.
    /// </para>
    /// </remarks>
    public static ж<array<T>> AliasPointer(ж<T>? element, nint length)
    {
        if (length >= 0 && element is not null && element.TryGetElementStorage(out T[]? backing, out nint index))
        {
            nint available = backing.Length - index;

            return new StandardBox<array<T>>(new array<T>(backing, (int)index, (int)(length < available ? length : available)));
        }

        return (ж<array<T>>)(uintptr)element!;
    }

    private static void checkArrayConversionLength(nint sourceLength, nint length)
    {
        // A PANIC, not an IndexOutOfRangeException: Go's `[N]T(s)` on a short slice is a
        // recoverable runtime panic, and only a PanicException is both visible to `recover()` and
        // excluded from a host containment policy (see the note on slice<T>'s windowing ctors).
        // The message text is unchanged — it was already Go's.
        if (sourceLength < length)
            throw RuntimeErrorPanic.ArrayConversionLength(sourceLength, length);
    }

    public array(Span<T> source)
    {
        m_array = AllocationCounter.CopyOf((ReadOnlySpan<T>)source);
        m_length = m_array.Length;
    }

    public array(ReadOnlySpan<T> source)
    {
        m_array = AllocationCounter.CopyOf(source);
        m_length = m_array.Length;
    }

    public array(Memory<T> source)
    {
        m_array = AllocationCounter.CopyOf((ReadOnlySpan<T>)source.Span);
        m_length = m_array.Length;
    }

    public array(ReadOnlyMemory<T> source)
    {
        m_array = AllocationCounter.CopyOf(source.Span);
        m_length = m_array.Length;
    }

    // Source intentionally stays the RAW backing reference: a null result is the discriminator
    // for a never-constructed zero value (see the generated struct constructors, which use it to
    // keep an array field's `= new(N)` initializer when the incoming argument is a zero value).
    // For an ALIAS window it is likewise the raw storage — the identity every address question is
    // asked about — so a consumer walking elements through it must add Low (ж<T>.CanonicalElement
    // does; everything else in the corpus holds a full-window array, where Low is 0).
    public T[] Source => m_array;

    /// <summary>
    /// Gets the offset of this array's first element inside <see cref="Source"/> — nonzero only for
    /// an <see cref="Alias"/> window over a slice's storage.
    /// </summary>
    public nint Low => m_low;

    // Null-safe view of the backing store: `default(array<T>)` runs no constructor, so m_array is
    // null; treat that zero value as an EMPTY array for all reads (length, index, enumerate,
    // print, compare) instead of throwing NRE — mirroring @string's null-safe zero value. (Go's
    // true zero `[N]T` has N zeroed elements, but the declared length only exists where a
    // constructor or field initializer ran; empty is the closest zero-value behavior a bare
    // `default` can have, and any index into it panics Go-style rather than crashing the host.)
    private T[] Backing => m_array ?? [];

    public Span<T> ꓸꓸꓸ => ToSpan(); // Spread operator

    public nint Length => m_length;

    // Returning by-ref value allows array to be a struct instead of a class and still allow read and write
    // Allows for implicit index support: https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-8.0/ranges#implicit-index-support
    public ref T this[int index]
    {
        get
        {
            if (index < 0 || index >= m_length)
                throw RuntimeErrorPanic.IndexOutOfRange(index, m_length);

            return ref Backing[m_low + index];
        }
    }

    public ref T this[nint index]
    {
        get
        {
            if (index < 0 || index >= m_length)
                throw RuntimeErrorPanic.IndexOutOfRange(index, m_length);

            return ref Backing[m_low + (int)index];
        }
    }

    public ref T this[ulong index] => ref this[(nint)index];

    public slice<T> this[Range range] => Slice(range.Start.GetOffset(m_length), range.End.GetOffset(m_length) - range.Start.GetOffset(m_length));

    public slice<T> Slice(int start, int length)
    {
        // Capacity stops at the ARRAY's end, never the backing store's: Go's `a[:]` on a `[4]byte`
        // has cap 4. Identical to the previous unbounded form for every full-window array (there
        // the array IS the backing); it is an Alias window that would otherwise report the rest of
        // the slice it aliases as spare capacity.
        return new slice<T>(Backing, m_low + start, m_low + start + length, m_low + m_length);
    }

    public slice<T> Slice(nint start, nint length)
    {
        return Slice((int)start, (int)length);
    }

    public int IndexOf(in T item)
    {
        int index = Array.IndexOf(Backing, item, m_low, m_length);
        return index >= 0 ? index - m_low : -1;
    }

    public bool Contains(in T item)
    {
        return IndexOf(item) >= 0;
    }

    // The window's own storage — the RAW backing whenever this array spans all of it, which every
    // ordinary construction does (so the structural paths below keep their exact previous behavior
    // and allocate nothing); only an Alias window materializes its elements.
    private T[] WindowArray
    {
        get
        {
            T[] backing = Backing;
            return m_low == 0 && m_length == backing.Length ? backing : AllocationCounter.CopyOf<T>(backing.AsSpan(m_low, m_length));
        }
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        ToSpan().CopyTo(array.AsSpan(arrayIndex));
    }

    public T[] ToArray()
    {
        return AllocationCounter.CopyOf<T>(ToSpan());
    }

    public Span<T> ToSpan()
    {
        return new Span<T>(Backing, m_low, m_length);
    }

    // Whether a by-value copy of this array must RE-COPY each element, decided once per T. An
    // element that is itself an array wrapper, or a struct carrying array fields, is a struct over a
    // shared T[] backing that a shallow element copy would leave aliased; a SLICE element is not —
    // see the reasoning at the use site in Clone below, which is the definition of record. Shared by
    // Clone and the range snapshot so the two can never drift into disagreeing about what a Go array
    // copy copies.
    private static readonly bool s_elementNeedsDeepCopy =
        (typeof(IArray).IsAssignableFrom(typeof(T)) && !typeof(ISlice).IsAssignableFrom(typeof(T))) ||
        typeof(IGoValueClone).IsAssignableFrom(typeof(T));

    public array<T> Clone()
    {
        // ToSpan().ToArray() rather than Backing.Clone(): a Go array copy is of the ARRAY, and an
        // Alias window's array is its window, not the slice storage behind it — so the copy is a
        // full, offset-free array again.
        T[] copy = AllocationCounter.CopyOf<T>(ToSpan());

        // Go array copy semantics are DEEP for nested arrays: assigning a [2][3]int copies the
        // inner arrays too. An element that is itself an array wrapper (array<T> or a generated
        // named-array wrapper — both implement IArray) is a struct over a shared T[] backing, so
        // the shallow element copy above would leave every nested backing aliased; re-clone each
        // element through its ICloneable surface, which returns the properly-wrapped clone for
        // unboxing back to T (recursing through deeper nestings). The typeof test is a JIT-time
        // constant, so non-nested element types keep the single shallow copy with no per-element
        // work.
        //
        // A STRUCT element carrying array fields (IGoValueClone — see GoValueCloneAttribute) has
        // exactly the same problem one level down: Go's `[2]digest` copy copies both digests'
        // arrays inline, while the shallow element copy shares their backing. It clones through
        // the same ICloneable surface.
        //
        // A SLICE element is excluded, and the exclusion is load-bearing in BOTH directions.
        // Semantically: Go's `[8]Bits` copy (math/big's bitsList, `type Bits []int`) copies eight
        // slice HEADERS and leaves the backing shared — exactly what the shallow element copy above
        // already did — so re-cloning would be wrong even if it worked. Mechanically it does NOT
        // work: `ISlice<T>` derives from `IArray<T>`, so a named-slice wrapper passes the IArray
        // test, while its generated `Clone()` forwards to the UNDERLYING `slice<T>`'s
        // `ICloneable.Clone()`, which hands back a boxed `slice<T>` — and the `(T)` cast below is
        // then `slice<int>` to `ΔBits`: InvalidCastException. Latent until something first cloned
        // an array whose element is a slice; the range-expression snapshot is that first caller,
        // and math/big's TestFloatAdd/TestFloatMul are the rows that met it.
        if (s_elementNeedsDeepCopy)
        {
            for (int i = 0; i < copy.Length; i++)
                copy[i] = (T)((ICloneable)copy[i]!).Clone();
        }

        return new array<T>(copy);
    }

    /// <summary>
    /// Go by-value copy under the UNIFORM member name generated code uses
    /// (<c>go2cs.Symbols.ValueCloneMethod</c>) — an alias of <see cref="Clone"/>.
    /// </summary>
    /// <remarks>
    /// A generated struct clone (see <see cref="GoValueCloneAttribute"/>) copies each of its
    /// clone-needing fields with ONE call form. A nested struct field declares this name itself; the
    /// array kinds — this type and the generated named-array / array-view wrappers — alias it here so
    /// the generated body needs no per-field branch. Copy SITES keep the public <c>Clone()</c>.
    /// </remarks>
    public array<T> ΔClone()
    {
        return Clone();
    }

    /// <summary>
    /// Gets a NEW array of this one's Go LENGTH whose elements are all the Go zero value — the
    /// shaped zero <see cref="builtin.GoZero{T}"/> hands back for a built-in like <c>clear</c>.
    /// </summary>
    /// <remarks>
    /// The Go length of a fixed-size array lives only in the instance ([4]int32 and [8]int32 are
    /// both <c>array&lt;int32&gt;</c>), so <c>default</c> would leave a LENGTH-ZERO array behind.
    /// The fill recurses through <see cref="builtin.GoZero{T}"/> so a NESTED shape
    /// (<c>[2][3]int32</c>) keeps its inner lengths too; the check is a per-T constant, so a plain
    /// element type allocates the zeroed backing and stops.
    /// </remarks>
    object IGoZeroShaped.GoZeroLike()
    {
        array<T> zero = new((nint)m_length);

        if (!builtin.ZeroIsDefault<T>())
        {
            for (int i = 0; i < m_length; i++)
                zero.m_array[i] = builtin.GoZero(Backing[m_low + i]);
        }

        return zero;
    }

    /// <summary>
    /// Gets an allocation-free enumerator over the array's (index, value) pairs — the shape a
    /// converted <c>for i, v := range a</c> binds.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The return type is the concrete <see cref="Enumerator"/> STRUCT, not
    /// <c>IEnumerator&lt;(nint, T)&gt;</c>: C#'s <c>foreach</c> binds <c>GetEnumerator</c> by pattern
    /// before it considers any interface, so a ranged loop over an array enumerates with zero heap
    /// traffic. The previous signature returned an interface from an ITERATOR method, which cost the
    /// compiler-generated state machine on entry to every such loop — the same shape and the same
    /// ~136 B/loop <see cref="slice{T}.Enumerator"/> shed. <see cref="Enumerator"/> still implements
    /// the interface, so an explicit <c>IEnumerator&lt;(nint, T)&gt;</c> consumer keeps working (it
    /// boxes, exactly as it did before); only the pattern path avoids the box.
    /// </para>
    /// <para>
    /// The enumerator reads the array's LIVE backing — it does not snapshot. Go's <c>range</c> over an
    /// array VALUE does iterate a copy, but that copy is the range EXPRESSION's, taken once before the
    /// loop, and the converter emits it as the same explicit <see cref="Clone"/> every other Go array
    /// value-copy site takes (see visitRangeStmt's array-snapshot arm). Snapshotting here instead would
    /// be wrong in both directions: it would copy for <c>range p</c> over a POINTER-to-array and for
    /// <c>for i := range a</c>, where Go copies nothing.
    /// </para>
    /// </remarks>
    public Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }

    /// <summary>
    /// Allocation-free (index, value) enumerator over an <see cref="array{T}"/>.
    /// </summary>
    /// <remarks>
    /// Mutable struct by design — the <c>foreach</c> pattern copies it into a local and drives that
    /// copy, which is how the loop stays allocation-free. It implements <see cref="IEnumerator{T}"/> so
    /// interface-typed and LINQ consumers still bind; those paths box the struct, which is the same
    /// cost they always paid.
    /// </remarks>
    [Serializable]
    public struct Enumerator : IEnumerator<(nint, T)>
    {
        private readonly T[] m_backing;
        private readonly nint m_low;
        private readonly nint m_length;
        private nint m_current;

        internal Enumerator(array<T> array)
        {
            // Backing, not m_array: `default(array<T>)` ran no constructor, so its backing is null and
            // its length is 0 — the same null-safe empty view every other read takes.
            m_backing = array.Backing;
            m_low = array.m_low;
            m_length = array.m_length;
            m_current = -1;
        }

        public readonly (nint, T) Current => (m_current, m_backing[m_low + m_current]);

        readonly object? IEnumerator.Current => Current;

        public bool MoveNext()
        {
            if (m_current >= m_length)
                return false;

            m_current++;
            return m_current < m_length;
        }

        void IEnumerator.Reset()
        {
            m_current = -1;
        }

        public readonly void Dispose()
        {
        }
    }

    /// <summary>
    /// Go's range-expression COPY of this array — the snapshot a <c>for i, v := range a</c> over an
    /// array VALUE iterates — as an enumerable that allocates NOTHING on the managed heap.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Why this is not <see cref="Clone"/>.</b> Both take Go's copy, and semantically the range
    /// site could have used <c>Clone()</c> — it did first. But a Go array copy lives INLINE, on the
    /// stack when the destination is a local, so Go charges it zero mallocs and zero
    /// <c>TotalAlloc</c>; <c>Clone()</c> charges a real managed array through
    /// <see cref="AllocationCounter"/>, which is the correct accounting for a copy that OUTLIVES the
    /// statement (an assignment, a return, a field) and the wrong accounting for one that cannot.
    /// Emitting the counted form at the range site would have made every allocation assertion around
    /// a range over an array value disagree with Go's own numbers — golib's counter is documented as
    /// the structural MIRROR of <c>runtime.MemStats.Mallocs</c>, and a mirror that charges what Go
    /// does not is wrong by construction, whether or not a row observes it today.
    /// </para>
    /// <para>
    /// <b>How the copy is free.</b> The snapshot's lifetime is exactly the loop — the tuple yields
    /// elements BY VALUE, so nothing the body keeps can point into it — which is the one shape a
    /// pooled buffer fits. <see cref="SnapshotEnumerator"/> rents from
    /// <see cref="ArrayPool{T}.Shared"/> and returns the buffer in <c>Dispose</c>, which C#'s
    /// <c>foreach</c> calls in a <c>finally</c>, so <c>break</c>, <c>return</c> and an exception all
    /// release it. Steady state is zero managed allocations, so BOTH instruments stay quiet: the
    /// object counter has nothing to charge, and <c>GC.GetTotalAllocatedBytes</c> — which the
    /// converted <c>runtime.ReadMemStats</c> reads and which no counter could hide from — sees no
    /// bytes. The instruments are untouched; the allocation simply stops happening.
    /// </para>
    /// <para>
    /// <b>Stated cost, three residuals.</b> (1) The FIRST rent of a given size class allocates, once
    /// per pool bucket per process — invisible to a warmed <c>AllocsPerRun</c>, visible once in a
    /// cumulative byte total. (2) An array longer than <see cref="ArrayPool{T}.Shared"/>'s largest
    /// bucket allocates per rent and is dropped on return; that is Go's behaviour too, which moves
    /// an array of that size off the stack. (3) An element type that needs a DEEP copy
    /// (<c>s_elementNeedsDeepCopy</c> — a nested array, or a struct carrying one) still allocates per
    /// element, because a nested <c>array&lt;T&gt;</c>'s own backing is a heap object in this model
    /// and Go's inline copy has no managed equivalent. That residual is unreachable from the corpus's
    /// allocation-asserting rows and is named rather than hidden.
    /// </para>
    /// </remarks>
    public RangeSnapshot ΔRangeSnapshot()
    {
        return new RangeSnapshot(this);
    }

    /// <summary>
    /// The enumerable half of <see cref="ΔRangeSnapshot"/> — it holds only the SOURCE window, so
    /// producing it is free and the buffer is rented no earlier than the loop that consumes it.
    /// </summary>
    public readonly struct RangeSnapshot
    {
        private readonly T[] m_backing;
        private readonly int m_low;
        private readonly int m_length;

        internal RangeSnapshot(array<T> array)
        {
            m_backing = array.Backing;
            m_low = array.m_low;
            m_length = array.m_length;
        }

        public SnapshotEnumerator GetEnumerator()
        {
            return new SnapshotEnumerator(m_backing, m_low, m_length);
        }
    }

    /// <summary>
    /// Allocation-free (index, value) enumerator over a POOLED copy of an array's elements.
    /// </summary>
    /// <remarks>
    /// The rented buffer is owned by exactly one enumerator instance and released by its
    /// <c>Dispose</c>. C#'s <c>foreach</c> creates one copy of this struct and disposes that copy, so
    /// the ownership is single by construction on the only path the converter emits; a caller that
    /// copies the struct and disposes both halves would return one buffer twice, which is why nothing
    /// but <c>foreach</c> should drive it. Disposing twice is harmless (the field is cleared first);
    /// disposing two COPIES is not.
    /// </remarks>
    public struct SnapshotEnumerator : IEnumerator<(nint, T)>
    {
        private T[]? m_buffer;
        private readonly nint m_length;
        private nint m_current;

        internal SnapshotEnumerator(T[] backing, int low, int length)
        {
            m_length = length;
            m_current = -1;

            if (length == 0)
            {
                // Nothing to snapshot, so nothing to rent — `for range` over a zero-length or
                // zero-VALUE array is a zero-iteration loop and must not touch the pool.
                m_buffer = null;
                return;
            }

            T[] buffer = ArrayPool<T>.Shared.Rent(length);

            new ReadOnlySpan<T>(backing, low, length).CopyTo(buffer);

            // The same deep element copy Clone takes, for the same reason and under the same
            // per-T decision: a nested array element is a struct over shared backing, so a shallow
            // element copy would let a write through the loop variable reach the source array.
            if (s_elementNeedsDeepCopy)
            {
                for (int i = 0; i < length; i++)
                    buffer[i] = (T)((ICloneable)buffer[i]!).Clone();
            }

            m_buffer = buffer;
        }

        public readonly (nint, T) Current => (m_current, m_buffer![m_current]);

        readonly object? IEnumerator.Current => Current;

        public bool MoveNext()
        {
            if (m_current >= m_length)
                return false;

            m_current++;
            return m_current < m_length;
        }

        void IEnumerator.Reset()
        {
            m_current = -1;
        }

        public void Dispose()
        {
            T[]? buffer = m_buffer;

            if (buffer is null)
                return;

            // Cleared first, so a second Dispose on THIS instance is a no-op rather than a second
            // Return of the same buffer.
            m_buffer = null;

            // A reference-bearing element type must not keep its objects alive through the pool;
            // an unmanaged one skips the clear, which is the whole point of the discriminant.
            ArrayPool<T>.Shared.Return(buffer, RuntimeHelpers.IsReferenceOrContainsReferences<T>());
        }
    }

    public override string ToString()
    {
        return $"[{string.Join(" ", ((IEnumerable<T>)this).Take(200))}{(Length > 200 ? " ..." : "")}]";
    }

    public override int GetHashCode()
    {
        // Structural (content-based) hash to match the structural Equals below: Go arrays are
        // comparable VALUES — a `map[[2]int]V` key must be found again by an equal array with
        // different backing storage (e.g. the defensive `.Clone()` a stored key takes). The
        // previous `m_array.GetHashCode()` hashed the backing REFERENCE, so every structural-
        // equal key missed.
        IStructuralEquatable equatable = WindowArray;
        return equatable.GetHashCode(EqualityComparer<T>.Default);
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

    // Structural equality comparers are called per-ELEMENT by IStructuralEquatable (each side
    // boxed), so they must be typed to the element — the previous container-typed comparers
    // (EqualityComparer<T[]>/<object[]>) threw "Type of argument is not compatible with the
    // generic comparer" on the first equal-length comparison of arrays with distinct backing
    // stores (e.g. a map<array<nint>, V> key lookup). An element that is itself an array
    // wrapper recurses through its own Equals(object) via the element comparer, so nested
    // arrays compare by content Go-style.
    public bool Equals(IArray? other)
    {
        IStructuralEquatable equatable = WindowArray;
        return equatable.Equals(otherWindow(other), EqualityComparer<object>.Default);
    }

    public bool Equals(IArray<T>? other)
    {
        IStructuralEquatable equatable = WindowArray;
        return equatable.Equals(otherWindow(other), EqualityComparer<T>.Default);
    }

    public bool Equals(array<T> other)
    {
        IStructuralEquatable equatable = WindowArray;
        return equatable.Equals(other.WindowArray, EqualityComparer<T>.Default);
    }

    // The comparand's own elements: Source is the RAW backing, which for an Alias window is wider
    // than the array it stands for (see Low).
    private static Array? otherWindow(IArray? other)
    {
        return other switch
        {
            null => null,
            array<T> arr => arr.WindowArray,
            _ => other.Source
        };
    }

    #region [ Operators ]

    // Enable implicit conversions between array<T> and T[]
    public static implicit operator array<T>(T[] value)
    {
        return new array<T>(value);
    }

    public static implicit operator array<T>(Span<T> value)
    {
        return new array<T>(value);
    }

    public static implicit operator array<T>(ReadOnlySpan<T> value)
    {
        return new array<T>(value);
    }

    public static implicit operator array<T>(Memory<T> value)
    {
        return new array<T>(value);
    }

    public static implicit operator array<T>(ReadOnlyMemory<T> value)
    {
        return new array<T>(value);
    }

    public static implicit operator T[](array<T> value)
    {
        return value.WindowArray;
    }

    // array<T> to array<T> comparisons
    public static bool operator ==(array<T> a, array<T> b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(array<T> a, array<T> b)
    {
        return !(a == b);
    }

    // Slice<T> to IArray comparisons
    public static bool operator ==(IArray a, array<T> b)
    {
        return b.Equals(a);
    }

    public static bool operator !=(IArray a, array<T> b)
    {
        return !(a == b);
    }

    public static bool operator ==(array<T> a, IArray b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(array<T> a, IArray b)
    {
        return !(a == b);
    }

    // array<T> to nil comparisons
    public static bool operator ==(array<T> array, NilType _)
    {
        return array.Length == 0;
    }

    public static bool operator !=(array<T> array, NilType nil)
    {
        return !(array == nil);
    }

    public static bool operator ==(NilType nil, array<T> array)
    {
        return array == nil;
    }

    public static bool operator !=(NilType nil, array<T> array)
    {
        return array != nil;
    }

    public static implicit operator array<T>(NilType _)
    {
        return default;
    }

    #endregion

    #region [ Interface Implementations ]

    object ICloneable.Clone()
    {
        // Returns the boxed array<T> wrapper (not the raw T[]): the deep-clone element pass in
        // Clone() above — and the generated named-array wrappers — unbox this result back to the
        // element type, which requires the exact wrapped form. Clone() reads through Backing,
        // so the zero-value (null m_array) case stays null-safe here too.
        return Clone();
    }

    Array IArray.Source => m_array;

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
            if (index < 0 || index >= Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            this[index] = value;
        }
    }

    int IList<T>.IndexOf(T item)
    {
        return IndexOf(item);
    }

    void IList<T>.Insert(int index, T item)
    {
        throw new NotSupportedException();
    }

    void IList<T>.RemoveAt(int index)
    {
        throw new NotSupportedException();
    }

    int IReadOnlyCollection<T>.Count => (int)Length;

    T IReadOnlyList<T>.this[int index] => this[index];

    bool ICollection<T>.IsReadOnly => false;

    int ICollection<T>.Count => (int)Length;

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
        return WindowArray.AsEnumerable().GetEnumerator();
    }

    // IArray<T> derives from IEnumerable<(nint, T)>, which the public GetEnumerator() satisfied
    // implicitly while it returned the interface. Now that it returns the concrete struct — so
    // `foreach` binds the pattern and allocates nothing — the interface member becomes explicit:
    // the boxing path, taken only when a consumer asks for the interface.
    IEnumerator<(nint, T)> IEnumerable<(nint, T)>.GetEnumerator()
    {
        return new Enumerator(this);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return WindowArray.GetEnumerator();
    }

    #endregion
}

public static class ArrayExtensions
{
    // array initializer from C# array
    public static array<T> array<T>(this T[] array)
    {
        return new array<T>(array);
    }

    // array initializer from a C# array that is SHORT of the Go array's declared length - the
    // `[8]byte{1, 2}` composite-literal form, where Go zero-fills the omitted elements. Emitted
    // by the converter only when the literal supplies fewer elements than the declared length,
    // so a full literal keeps the plain `.array()` projection.
    //
    // The padded copy is built HERE rather than in an `array<T>(T[], int)` constructor on purpose:
    // such a constructor is ambiguous with the `array(slice<T>, nint)` slice-to-array CONVERSION
    // ctor for a call like `new array<byte>(s, 4)` (CS0121 - `slice<T>` is the exact match on the
    // source but `int` is the exact match on the length, so neither overload is better;
    // NamedPointerReinterpret's `sliceToArray` hit exactly this). Keeping the padding out of the
    // constructor set leaves array<T>'s ctor overloads untouched.
    public static array<T> array<T>(this T[] array, int length)
    {
        T[] padded = AllocationCounter.NewArray<T>(length);

        // An over-long source cannot arise from a valid Go literal (the compiler rejects an index
        // past the declared length), so it is truncated rather than checked.
        array.AsSpan(0, Math.Min(array.Length, length)).CopyTo(padded);

        return new array<T>(padded);
    }

    // The same padding for a SHORT literal whose ELEMENT zero value must itself be constructed,
    // because default(T) is not usable storage - a nested fixed array (`[2][3]uint8{}`, whose inner
    // length exists only in the Go type) or a struct whose own zero value needs construction. This
    // is the composite-literal twin of the `array(nint, Func<T>)` constructor, and it exists for the
    // same reason the padding above does: the length overload alone fixed the OUTER dimension while
    // filling it with default(T), so `[2][3]uint8{}` was 2 long with two ZERO-length elements -
    // `len(x[0])` reported 0 where Go says 3, and the first indexed write into one panicked. The
    // declared form (`var x [2][3]uint8`) routes through the zero-value construction ladder instead
    // and was always right, so the two spellings of one Go type disagreed.
    //
    // The converter supplies the factory, since only it knows the element's shape, and only when
    // the element needs one - every other element type keeps the two-argument padding above.
    public static array<T> array<T>(this T[] array, int length, Func<T> elementFactory)
    {
        T[] padded = AllocationCounter.NewArray<T>(length);
        int written = Math.Min(array.Length, length);

        array.AsSpan(0, written).CopyTo(padded);

        // Only the ZERO-FILLED tail is constructed: the literal's own elements are already whatever
        // it wrote, and re-running the factory over them would discard them.
        for (int i = written; i < length; i++)
            padded[i] = elementFactory();

        return new array<T>(padded);
    }

    // Same padding for the SparseArray projection of an INDEX-KEYED literal whose highest key
    // falls short of the declared length (`[8]byte{i: 1}` with a non-literal constant index):
    // SparseArray's own Count is `max index + 1`, which is the literal's extent, not the array's.
    public static array<T> array<T>(this IEnumerable<T> source, int length)
    {
        // Enumerable.ToArray's own result is charged here; the growth buffers it discards on the
        // way are BCL-internal and, like every other BCL internal, deliberately uncharged.
        AllocationCounter.Count();
        return source.ToArray().array(length);
    }

    // …and the needy-element form of that projection. This one is NOT the T[] overload's story with
    // a different receiver: a sparse literal's zero values are its GAPS, which can sit anywhere, not
    // only in a tail — `[4][3]uint8{1: {…}}` needs indices 0, 2 and 3 constructed. Enumerating a
    // SparseArray yields `default!` for a gap, indistinguishable afterwards from an element the
    // literal genuinely wrote, so the set positions are asked for directly instead.
    public static array<T> array<T>(this SparseArray<T> source, int length, Func<T> elementFactory)
    {
        T[] padded = AllocationCounter.NewArray<T>(length);

        for (int i = 0; i < length; i++)
            padded[i] = source.TryGetItem(i, out T value) ? value : elementFactory();

        return new array<T>(padded);
    }

    // The general enumerable form, for a needy-element projection whose source is not a
    // SparseArray. Every position past the source's own extent is constructed.
    public static array<T> array<T>(this IEnumerable<T> source, int length, Func<T> elementFactory)
    {
        AllocationCounter.Count();
        return source.ToArray().array(length, elementFactory);
    }

    // array initializer from Span
    public static array<T> array<T>(this Span<T> source)
    {
        return new array<T>(source);
    }

    // array initializer from an enumerable
    public static array<T> array<T>(this IEnumerable<T> source)
    {
        // As above: the materialized array is charged, its BCL-internal growth buffers are not.
        AllocationCounter.Count();
        return new array<T>(source.ToArray());
    }
}
