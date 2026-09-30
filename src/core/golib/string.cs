// string.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable CheckNamespace
// ReSharper disable SpecifyACultureInStringConversionExplicitly
// ReSharper disable InconsistentNaming
// ReSharper disable LoopCanBeConvertedToQuery
// ReSharper disable ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
// ReSharper disable BuiltInTypeReferenceStyle
// ReSharper disable UseSymbolAlias

using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using go.golib;

namespace go;

/// <summary>
/// Represents a structure with heap allocated data that behaves like a Go string.
/// </summary>
public readonly struct @string : 
    IConvertible, 
    IEquatable<@string>, 
    IComparable<@string>, 
    IReadOnlyList<byte>, 
    IEnumerable<rune>, 
    IEnumerable<(nint, rune)>, 
    IEnumerable<char>, 
    ICloneable, 
    IComparisonOperators<@string, @string, bool>,
    IAdditionOperators<@string, @string, @string>,
    IByteSeq<@string, byte>
{
    // A Go string header is a POINTER PLUS LENGTH into shared immutable storage, which is what makes
    // `s[i:j]` an O(1) WINDOW rather than a copy. Modeling @string as a bare byte[] made that slice
    // O(n)-with-an-allocation, so the most ordinary Go idiom there is —
    //
    //      for i := 0; i < len(s); { r, size := utf8.DecodeRuneInString(s[i:]); i += size }
    //
    // — was ACCIDENTALLY QUADRATIC. archive/zip's detectUTF8 walks exactly that loop over a
    // 65,535-byte file name and copied ~2.1 GB per call; its TestZip64LargeDirectory (13.2 s in Go)
    // had not finished in 45 minutes (r57c). Carrying the offset/length window here restores Go's
    // cost model: slicing a string allocates nothing and copies nothing.
    //
    // Sharing the backing array is safe for precisely the reason it is safe in Go — @string is
    // IMMUTABLE, and every conversion OUT to storage the receiver may mutate (`[]byte(s)`, the
    // byte[] operator) already copies. m_value is deliberately PRIVATE: a consumer reading the raw
    // array instead of the window would silently see the whole backing, and privacy makes that a
    // compile error rather than a wrong answer.
    private readonly byte[] m_value;
    private readonly int m_offset;
    private readonly int m_length;

    // Null-safe view of this string's bytes: `default(@string)` runs no constructor, so m_value is
    // null; treat that zero value as Go's empty string ("") for all reads (length, index, concat,
    // print, range) instead of throwing NRE. Mirrors the nil-map / nil-slice null-safe approach.
    internal ReadOnlySpan<byte> Bytes => m_value is null ? default : new ReadOnlySpan<byte>(m_value, m_offset, m_length);

    // The canonical window constructor — every other constructor funnels here or assigns the three
    // fields directly. Not public: a caller handing in a backing array it can still write through
    // would break the immutability the sharing depends on.
    private @string(byte[] backing, int offset, int length)
    {
        m_value = backing;
        m_offset = offset;
        m_length = length;
    }

    public @string()
    {
        m_value = [];
        m_offset = 0;
        m_length = 0;
    }

    public @string(byte[]? bytes)
    {
        m_value = bytes ?? [];
        m_offset = 0;
        m_length = m_value.Length;
    }

    public @string(in ReadOnlySpan<byte> bytes)
    {
        m_value = AllocationCounter.CopyOf(bytes);
        m_offset = 0;
        m_length = m_value.Length;
    }

    // Charges the intermediate UTF-16 string on the way through; the UTF-8 backing the delegated
    // constructor then encodes is charged there, so this shape costs two objects and says so.
    public @string(char[] value) : this(AllocationCounter.NewString(value)) { }

    public @string(in ReadOnlySpan<rune> value) : this(value.ToUTF8Bytes()) { }

    // Go's `string(b)` for a []byte. A one-byte result reads a static table and allocates nothing —
    // slicebytetostring's `n == 1` branch, which returns a view into runtime.staticuint64s
    // (runtime/string.go:144-150). Scoped to this conversion only: `string(byte)` and `string(rune)`
    // are Go's intstring, which allocates when its result escapes, so they do not come here. Sharing
    // one backing per byte value is sound because no @string hands out a writable view of its bytes
    // (ToSpan, Slice and the spread are read-only; unsafe.StringData's pointer carries Go's own
    // must-not-modify contract, the same one staticuint64s relies on).
    public @string(in slice<byte> value)
    {
        if (value.Length == 1)
        {
            m_value = s_oneByteStrings[value[0]];
            m_offset = 0;
            m_length = 1;
            return;
        }

        // Any other length copies: the detached copy is charged through the door slice<T>.Source uses.
        m_value = AllocationCounter.CopyOf<byte>(value.ToSpan());
        m_offset = 0;
        m_length = m_value.Length;
    }

    // The 256 one-byte backings, built once. Plain allocations, not charged to AllocationCounter: they
    // are process-wide static data, as staticuint64s is in Go, not an allocation of any conversion.
    private static readonly byte[][] s_oneByteStrings = BuildOneByteStrings();

    private static byte[][] BuildOneByteStrings()
    {
        byte[][] table = new byte[256][];

        for (int i = 0; i < table.Length; i++)
            table[i] = [(byte)i];

        return table;
    }

    /// <summary>
    /// Creates a TRANSIENT @string that ALIASES <paramref name="value"/>'s backing bytes without
    /// copying — go2cs's mirror of the Go compiler's <c>m[string(b)]</c> special case, reached
    /// through <see cref="builtin.tmpstring"/>. Zero allocation, exactly like Go's
    /// <c>runtime.slicebytetostringtmp</c>.
    /// </summary>
    /// <remarks>
    /// The alias shares MUTABLE storage with the slice, so it is sound only under the same contract
    /// Go's optimization holds its temporaries to: the value is consumed before the slice can next be
    /// written and is never stored. The converter emits it solely for map-index READ keys, where the
    /// lookup hashes and compares the key but never retains it; the miss/return paths keep the
    /// copying conversion. Internal so no hand-written consumer can reach the raw factory — the
    /// documented public surface is <see cref="builtin.tmpstring"/>, which carries the contract.
    /// </remarks>
    internal static @string TransientAliasOf(in slice<byte> value) => AliasOf(value);

    /// <summary>
    /// Creates an @string that ALIASES <paramref name="value"/>'s backing bytes at its own window,
    /// without copying — the Go string HEADER over storage the caller already owns.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The factory behind both aliasing contracts golib holds, which share this body and differ
    /// only in what the caller promises: <see cref="TransientAliasOf"/> is the converter's
    /// map-index temporary (consumed immediately, never stored), and <c>unsafe.String</c> is Go's
    /// own <c>unsafe.String(ptr, len)</c>, whose returned value is long-lived and whose contract is
    /// the mirror prohibition — the bytes must not be modified while the string exists. Both are
    /// sound for the same reason: @string does not COPY on construction here, so the string simply
    /// IS the Go header (backing + offset + length) over the caller's storage, at no per-string
    /// byte cost.
    /// </para>
    /// <para>
    /// Internal, and deliberately so: a caller handing in a backing array it can still write
    /// through breaks the immutability every other @string path depends on, and privacy makes
    /// reaching it a compile error outside golib's two <c>InternalsVisibleTo</c> grants.
    /// </para>
    /// </remarks>
    internal static @string AliasOf(in slice<byte> value)
    {
        // A native-backed window has no managed array — the string conversion COPIES by Go's own
        // contract either way, so the span path serves both backings byte-exactly. (unsafe.String
        // never reaches this branch: its native pointers are answered by their own arm upstream,
        // and an element window is managed by construction.)
        if (value.IsNativeBacked)
            return new @string(value.ToSpan());

        return new @string(value.m_array ?? [], (int)value.Low, (int)value.Length);
    }

    public @string(in slice<char> value) : this(AllocationCounter.CopyOf<char>(value.ToSpan())) { }

    public @string(in slice<rune> value) : this(value.ToSpan()) { }

    // ToSpan(), not Source: Source is the RAW backing, which for an array<T> ALIAS window
    // (`(*[N]T)(s)`) is wider than the array it stands for.
    public @string(in IArray<byte> value) : this((ReadOnlySpan<byte>)value.ToSpan()) { }

    // From a `string | []byte`-constrained byte sequence (IByteSeq<byte>): a generic body slices
    // such a value and wraps the result in @string (e.g. `string(s[a:b])` in bytealg's Rabin-Karp).
    // A concrete slice<byte>/@string argument still prefers its own more-specific constructor.
    public @string(IByteSeq<byte> value)
    {
        // One interface call and one vectorized copy — see the matching slice<T> constructor.
        byte[] bytes = AllocationCounter.CopyOf<byte>(value.ꓸꓸꓸ);

        m_value = bytes;
        m_offset = 0;
        m_length = bytes.Length;
    }

    public @string(string? value)
    {
        m_value = AllocationCounter.Utf8ToBytes(value);
        m_offset = 0;
        m_length = m_value.Length;
    }

    // Shares the source's window rather than copying it — @string is immutable, so a "copy" of one
    // is indistinguishable from the original. (This is what the byte[]-backed form did too, by
    // handing its backing array straight to the byte[] constructor.)
    public @string(@string value)
    {
        m_value = value.m_value ?? [];
        m_offset = value.m_offset;
        m_length = value.m_length;
    }

    public int Length => m_length;

    public byte this[int index]
    {
        get
        {
            if (index < 0 || index >= m_length)
                throw RuntimeErrorPanic.IndexOutOfRange(index, m_length);

            return m_value![m_offset + index];
        }
    }

    public byte this[nint index]
    {
        get
        {
            if (index < 0 || index >= m_length)
                throw RuntimeErrorPanic.IndexOutOfRange(index, m_length);

            return m_value![m_offset + (int)index];
        }
    }

    // An UNSIGNED index is checked before any narrowing, as slice's is (goPanicIndexU); the converter
    // emits every unsigned string index bare onto this overload, so the one unsigned compare is the
    // whole bounds check.
    public byte this[ulong index]
    {
        get
        {
            if (index >= (ulong)m_length)
                throw RuntimeErrorPanic.IndexOutOfRange(index, m_length);

            return m_value![m_offset + (int)index];
        }
    }

    // Slicing a Go string yields a string (e.g. `s[a:b]`), so the range indexer
    // returns @string. Returning slice<byte> here would break string comparisons
    // (slice<byte> != string) and put a ref-struct-convertible value into tuples.
    //
    // The result WINDOWS the same backing array — no allocation, no copy, exactly as in Go. An
    // out-of-range bound is Go's runtime panic, measured against the receiver's window: CLR
    // ArgumentExceptions here escaped recover(). (A NEGATIVE bound cannot arrive: System.Index
    // refuses it in the caller's conversion, before this runs. A bound that can be negative or past
    // int32 takes slice(low, high) below instead: S-c R1-A.)
    public @string this[Range range] => slice(range.Start.GetOffset(m_length), range.End.GetOffset(m_length));

    // Go's 2-index s[low:] and s[low:high] (`s[:high]` is `slice(0, high)`) with NO sentinel: high is checked
    // against the length, then low against high, both unsigned, so a negative bound fails the first check
    // that reads it (goPanicSliceAlen, goPanicSliceB), and a bound past int32 is checked at its full value
    // (S-c R1-A, docs/phase4/DESIGN-slice-bounds-r1a.md). An instance member, so it binds ahead of the
    // `builtin.slice(this @string, …)` extension, which stays golib's sentinel form over the raw bytes.
    public @string slice(nint low)
    {
        return slice(low, m_length);
    }

    public @string slice(nint low, nint high)
    {
        if ((nuint)high > (nuint)m_length || (nuint)low > (nuint)high)
            throw RuntimeErrorPanic.LengthSliceBoundsOutOfRange(low, high, m_length);

        return new @string(m_value ?? [], m_offset + (int)low, (int)(high - low));
    }

    // IByteSeq<@string, byte> — models Go's `string | []byte` union constraint. The byte indexer
    // (this[nint]) implicitly implements IByteSeq<byte>.this[nint], and the @string range indexer
    // above implicitly implements IByteSeq<@string, byte>.this[Range] — self-referential, so a
    // generic body's sub-slice stays an @string instead of boxing into the interface; the slice pair
    // above implements its sentinel-free sub-slice the same way. Only Length needs an explicit form,
    // to widen @string's int Length to the interface's nint.
    nint IByteSeq.Length => m_length;

    public ReadOnlySpan<byte> Slice(int start, int length)
    {
        return Bytes.Slice(start, length);
    }

    public ReadOnlySpan<byte> Slice(nint start, nint length)
    {
        return Bytes.Slice((int)start, (int)length);
    }

    // The explicit-bounds slice path behind `builtin.slice(s, low, high, max)`, bounded by this
    // string's WINDOW rather than by its whole backing array — the same correction array<T>.slice
    // already carries for an alias window. Shares the backing, as Go's slicing always does.
    // The string's own window over its backing array, as the slice<byte> an element reference needs:
    // `unsafe.StringData` is DEFINED as `&str[0]`, an interior pointer into the string's storage, and
    // Go's contract is that those bytes must not be modified. Every other view of an @string is
    // read-only (ToSpan, Slice, the spread, SliceBounds), because a backing may be shared by every
    // string windowing it, a hoisted literal, or a one-byte string of the process-wide table. So this
    // is internal, reached only through unsafe's InternalsVisibleTo grant, and named for what it is.
    internal slice<byte> UnsafeBackingWindow()
    {
        return new slice<byte>(m_value ?? [], m_offset, m_offset + m_length);
    }

    // No bound is a sentinel (S-c R1-A): the callers pass every bound explicitly.
    internal ReadOnlySpan<byte> SliceBounds(nint low, nint high, nint max)
    {
        if (low < 0 || high < low || max < high || max > m_length)
            throw RuntimeErrorPanic.LengthSliceBoundsOutOfRange(low, high, max, m_length);

        return Bytes.Slice((int)low, (int)(high - low));
    }

    public ReadOnlySpan<byte> ToSpan()
    {
        return Bytes;
    }

    public ReadOnlySpan<byte> ꓸꓸꓸ => ToSpan(); // Spread operator

    // NOTE: there is deliberately no pinned-view accessor here. `unsafe.StringData` was this
    // string's only pinning consumer, and its pin was a defect rather than a service: a
    // `GCHandle.Alloc(…, Pinned)` handle is an unconditional strong root, so a pointer handed to
    // `runtime.SetFinalizer` kept the very allocation the finalizer was waiting on permanently
    // alive (unique's TestMakeClonesStrings — see the rationale at that call site). A caller that
    // genuinely needs the backing held still asks ж for it, which pins ON DEMAND and only when the
    // pointer is converted to an address; a caller that wants the bytes takes `Slice`.
    public override string ToString()
    {
        return AllocationCounter.Utf8ToString(Bytes);
    }

    public bool Equals(@string other)
    {
        return Bytes.SequenceEqual(other.Bytes);
    }

    // Go compares strings as raw bytes; for valid UTF-8 this is also code-point order. Comparing the
    // backing bytes directly avoids transcoding both sides to UTF-16 strings per comparison.
    public int CompareTo(@string other)
    {
        return Bytes.SequenceCompareTo(other.Bytes);
    }

    public override bool Equals(object? obj)
    {
        return obj switch
        {
            null          => false,
            @string gostr => Equals(gostr),
            string str    => Equals(str),
            _             => false
        };
    }

    public override int GetHashCode()
    {
        System.HashCode hash = new();
        hash.AddBytes(Bytes);
        return hash.ToHashCode();
    }

    public string ToString(IFormatProvider? provider)
    {
        return ToString().ToString(provider);
    }

    public TypeCode GetTypeCode()
    {
        return TypeCode.String;
    }

    public @string Clone()
    {
        return new @string(this);
    }

    public IEnumerator<(nint, rune)> GetEnumerator()
    {
        // RuneSpanEnumerator is a class: `for range s` over a Go string allocates nothing, ours
        // allocates the enumerator, and the count is where that difference becomes visible.
        AllocationCounter.Count();

        return new RuneSpanEnumerator(m_value ?? [], m_offset, m_length);
    }

    // Holds the backing array plus this string's window: a class cannot hold the ReadOnlySpan the
    // rest of the type reads through, and re-materializing the window as its own array would put an
    // allocation back into `for i, r := range s`.
    private class RuneSpanEnumerator(byte[] bytes, int offset, int length) : IEnumerator<(nint, rune)>
    {
        private readonly byte[] m_bytes = bytes;
        private readonly int m_offset = offset;
        private readonly int m_length = length;
        private int m_byteIndex;
        private (nint, rune) m_current;

        public (nint, rune) Current => m_current;

        object IEnumerator.Current => Current;

        void IDisposable.Dispose() { }

        public bool MoveNext()
        {
            if (m_byteIndex >= m_length)
                return false;

            ReadOnlySpan<byte> remainingBytes = new(m_bytes, m_offset + m_byteIndex, m_length - m_byteIndex);
            OperationStatus status = Rune.DecodeFromUtf8(remainingBytes, out Rune rune, out int bytesConsumed);

            if (status == OperationStatus.Done)
            {
                // Go `for i, r := range s` yields the BYTE index of each rune's first byte, not
                // the rune ordinal — a multi-byte rune advances the next index by its encoded
                // length (unicode/utf8 TestSequencing walks exactly this contract).
                m_current = (m_byteIndex, rune.Value);
            }
            else
            {
                // Invalid sequence: Go yields U+FFFD at the byte's index and advances a SINGLE
                // byte (spec: range and DecodeRune consume one byte per invalid sequence) — never
                // .NET's maximal-subpart consumption, which can swallow several bytes at once.
                m_current = (m_byteIndex, RuneReplacementChar);
                bytesConsumed = 1;
            }

            m_byteIndex += bytesConsumed;
            return true;
        }

        public void Reset()
        {
            m_byteIndex = 0;
            m_current = default;
        }
    }

    public rune[] ToRunes()
    {
        // Estimate the rune length (1 rune per byte as worst case)
        int estimatedLength = m_length;

        Span<rune> runes = estimatedLength <= StackAllocThreshold / 4 ?
            stackalloc rune[estimatedLength] :
            AllocationCounter.NewArray<rune>(estimatedLength);

        int runesDecoded = DecodeRunes(runes);
        return AllocationCounter.CopyOf<rune>(runes[..runesDecoded]);
    }

    private int DecodeRunes(Span<rune> runes)
    {
        if (m_length == 0)
            return 0;

        int index = 0;
        ReadOnlySpan<byte> bytes = Bytes;

        while (!bytes.IsEmpty)
        {
            OperationStatus status = Rune.DecodeFromUtf8(bytes, out Rune rune, out int bytesConsumed);

            if (status == OperationStatus.Done)
            {
                runes[index++] = rune.Value;
            }
            else
            {
                // Invalid sequence: Go's []rune(string) yields one U+FFFD PER INVALID BYTE
                // (same single-byte advance as range/DecodeRune) — never .NET's maximal-subpart
                // consumption, which can swallow several bytes as one replacement.
                runes[index++] = RuneReplacementChar;
                bytesConsumed = 1;
            }

            bytes = bytes[bytesConsumed..];
        }

        return index;
    }

    public static @string Default => new("");

    #region [ Operators ]

    // Enable implicit conversions between string and @string struct
    public static implicit operator @string(string value)
    {
        return new @string(value);
    }

    public static implicit operator string(@string value)
    {
        return value.ToString();
    }

    public static implicit operator @string(ReadOnlySpan<byte> value)
    {
        return new @string(value);
    }

    public static implicit operator @string(slice<byte> value)
    {
        return new @string(value);
    }

    // COPIES, exactly like the `byte[]` operator below and for the same reason: Go's `[]byte(s)`
    // yields storage the receiver may freely mutate, and handing out the string's own backing array
    // let a converted `b := []byte(m.str); b[0] = 0x80` write THROUGH into the string (unicode/utf8's
    // TestDecodeRune corrupted the package's utf8map table for every later test). Emitted code binds
    // the copying `builtin.slice<byte>(s)` route, so this implicit form is reached from hand-written
    // golib/consumer code only — where the sharing was a latent instance of the same defect. It also
    // has to hold now that one @string value can be SHARED by every evaluation of a hoisted literal
    // (see the string-literal-allocation arc): a shared backing array must never become writable.
    public static implicit operator slice<byte>(@string value)
    {
        return new slice<byte>(AllocationCounter.CopyOf(value.Bytes));
    }

    public static implicit operator @string(slice<rune> value)
    {
        return new @string(value);
    }

    public static implicit operator slice<rune>(@string value)
    {
        // ToRunes decodes straight into an exactly-sized array, and that array is freshly built
        // and exclusively owned here, so the slice WRAPS it rather than copying it again.
        //
        // This used to go `((IEnumerable<rune>)value).ToArray()`, which decoded the string into an
        // array (inside the rune enumerator, which calls ToRunes itself), yielded that array back
        // one element at a time through an iterator, and let LINQ re-materialize it through its
        // growth buffers — three passes and several allocations for a conversion Go performs with
        // one. The explicit Count() went with it: ToRunes charges the copy it makes, so charging
        // again here would report an object that is no longer allocated.
        return new slice<rune>(value.ToRunes());
    }

    public static implicit operator @string(rune value)
    {
        return new @string([value]);
    }

    public static explicit operator rune(@string value)
    {
        // Only the FIRST rune is wanted, so decode only the first rune — this used to decode the
        // whole string into an array and take element zero, which is unbounded work and an
        // allocation for a fixed-size answer.
        //
        // The two branches mirror DecodeRunes exactly, because they have to agree: an empty string
        // yields the zero rune (what FirstOrDefault returned over the empty array), and an invalid
        // leading sequence yields U+FFFD — Go's own answer, and the reason .NET's maximal-subpart
        // consumption is not consulted here either.
        ReadOnlySpan<byte> bytes = value.Bytes;

        if (bytes.IsEmpty)
            return 0;

        return Rune.DecodeFromUtf8(bytes, out Rune rune, out _) == OperationStatus.Done ?
            rune.Value :
            RuneReplacementChar;
    }

    public static implicit operator @string(slice<char> value)
    {
        return new @string(value);
    }

    public static implicit operator slice<char>(@string value)
    {
        return new slice<char>(AllocationCounter.Materialize((IEnumerable<char>)value));
    }

    public static implicit operator byte[](@string value)
    {
        // Go: `[]byte(s)` COPIES — strings are immutable and the receiver may freely mutate the
        // result. Returning the backing array let a converted `b := []byte(m.str); b[0] = 0x80`
        // write THROUGH into the string (unicode/utf8's TestDecodeRune corrupted the package's
        // utf8map string table for every later test). Zero-copy read-only access uses sstring
        // views / ToSpan() internally — never this conversion.
        return AllocationCounter.CopyOf(value.Bytes);
    }

    // NOTE: stores WITHOUT copying — every LIVE Go []byte value is a slice<byte> in converted
    // code, so `string(b)` conversions route through the copying slice<byte> constructor above;
    // a raw byte[] reaches this operator only as a freshly allocated array (emitted literals,
    // golib internals), where a defensive copy would be pure waste.
    public static implicit operator @string(byte[] value)
    {
        return new @string(value);
    }

    public static implicit operator rune[](@string value)
    {
        return value.ToRunes();
    }

    public static implicit operator @string(rune[] value)
    {
        return new @string(new ReadOnlySpan<rune>(value));
    }

    public static implicit operator ReadOnlySpan<rune>(@string value)
    {
        return value.ToRunes();
    }

    public static implicit operator @string(ReadOnlySpan<rune> value)
    {
        return new @string(value);
    }

    public static explicit operator char[](@string value)
    {
        return AllocationCounter.Materialize((IEnumerable<char>)value);
    }

    public static implicit operator @string(char[] value)
    {
        return new @string(value);
    }

    // Enable comparisons between nil and @string struct
    public static bool operator ==(@string value, NilType _)
    {
        return value.Equals(Default);
    }

    public static bool operator !=(@string value, NilType nil)
    {
        return !(value == nil);
    }

    public static bool operator ==(NilType nil, @string value)
    {
        return value == nil;
    }

    public static bool operator !=(NilType nil, @string value)
    {
        return value != nil;
    }

    // Enable @string to @string comparisons
    public static implicit operator @string(NilType _)
    {
        return Default;
    }

    public static bool operator ==(@string a, @string b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(@string a, @string b)
    {
        return !a.Equals(b);
    }

    public static bool operator <(@string a, @string b)
    {
        return a.CompareTo(b) < 0;
    }

    public static bool operator <=(@string a, @string b)
    {
        return a.CompareTo(b) <= 0;
    }

    public static bool operator >(@string a, @string b)
    {
        return a.CompareTo(b) > 0;
    }

    public static bool operator >=(@string a, @string b)
    {
        return a.CompareTo(b) >= 0;
    }

    // Comparisons directly against a `ReadOnlySpan<byte>` — most importantly a `u8` string literal
    // (`s == "…"u8`), which is zero-allocation static ROM. Go keeps its string literals in RODATA and
    // `s == "true"` allocates NOTHING; without these operators the span had to convert to @string
    // first (`implicit operator @string(ReadOnlySpan<byte>)`), materializing a fresh backing byte[]
    // on EVERY evaluation — 408 bytes per strconv.ParseBool call, once per operand of its lowered
    // switch chain. These compare the backing bytes in place instead, so a literal comparison costs
    // nothing at all. Both operand orders are declared: the operators bind by EXACT match, which
    // beats the user-defined span→@string conversion the same-type operators would need, and the
    // literal is as likely to be written on the left (`"…"u8 == s`, a lowered case test) as on the
    // right. These mirror sstring's set (see sstring.cs) — the stack-string and heap-string forms
    // now have the same literal-comparison cost model.
    public static bool operator ==(@string a, ReadOnlySpan<byte> b)
    {
        return a.Bytes.SequenceEqual(b);
    }

    public static bool operator !=(@string a, ReadOnlySpan<byte> b)
    {
        return !a.Bytes.SequenceEqual(b);
    }

    public static bool operator ==(ReadOnlySpan<byte> a, @string b)
    {
        return a.SequenceEqual(b.Bytes);
    }

    public static bool operator !=(ReadOnlySpan<byte> a, @string b)
    {
        return !a.SequenceEqual(b.Bytes);
    }

    public static bool operator <(@string a, ReadOnlySpan<byte> b)
    {
        return a.Bytes.SequenceCompareTo(b) < 0;
    }

    public static bool operator <=(@string a, ReadOnlySpan<byte> b)
    {
        return a.Bytes.SequenceCompareTo(b) <= 0;
    }

    public static bool operator >(@string a, ReadOnlySpan<byte> b)
    {
        return a.Bytes.SequenceCompareTo(b) > 0;
    }

    public static bool operator >=(@string a, ReadOnlySpan<byte> b)
    {
        return a.Bytes.SequenceCompareTo(b) >= 0;
    }

    public static bool operator <(ReadOnlySpan<byte> a, @string b)
    {
        return a.SequenceCompareTo(b.Bytes) < 0;
    }

    public static bool operator <=(ReadOnlySpan<byte> a, @string b)
    {
        return a.SequenceCompareTo(b.Bytes) <= 0;
    }

    public static bool operator >(ReadOnlySpan<byte> a, @string b)
    {
        return a.SequenceCompareTo(b.Bytes) > 0;
    }

    public static bool operator >=(ReadOnlySpan<byte> a, @string b)
    {
        return a.SequenceCompareTo(b.Bytes) >= 0;
    }

    // Go's concatstrings (runtime/string.go:46-51): when only one operand is non-empty, the result IS
    // that operand — no allocation, no copy — and when none is, the result is "". That holds for an
    // operand that aliases other storage too (AliasOf, unsafe.String): Go returns it as it is, and so
    // does this (COORD's verification note F8 retired revision 4's copy-on-alias precondition). The
    // stack-data exception Go makes for an escaping result cannot arise here: no @string lives on a
    // stack. The span-operand overloads below follow the same rule for an empty span.
    public static @string operator +(@string a, @string b)
    {
        if (b.m_length == 0)
            return a;

        if (a.m_length == 0)
            return b;

        ReadOnlySpan<byte> sa = a.Bytes, sb = b.Bytes;
        byte[] bytes = AllocationCounter.NewArray<byte>(sa.Length + sb.Length);

        sa.CopyTo(new Span<byte>(bytes, 0, sa.Length));
        sb.CopyTo(new Span<byte>(bytes, sa.Length, sb.Length));

        return new @string(bytes);
    }

    // Concatenation directly against a `ReadOnlySpan<byte>` operand — most importantly a `u8` string
    // literal (`s + "-"u8`), which is zero-allocation static ROM. Binding these overloads avoids the
    // intermediate `@string` the span would otherwise be copied into (via the implicit
    // `@string(ReadOnlySpan<byte>)` conversion) before `operator +(@string, @string)` ran: the
    // literal's bytes are block-copied straight into the single result buffer instead. Only the
    // per-concat path is affected — no change to the far hotter `[]byte`→`@string` conversion path.
    public static @string operator +(@string a, ReadOnlySpan<byte> b)
    {
        // An empty span operand leaves the @string, which is returned as itself (G8). An empty @string
        // with a non-empty span still copies the span once: Go would return the literal's own static
        // bytes, but a span has no @string to return until the literal is materialized.
        if (b.Length == 0)
            return a;

        ReadOnlySpan<byte> a1 = a.Bytes;
        byte[] bytes = AllocationCounter.NewArray<byte>(a1.Length + b.Length);

        a1.CopyTo(new Span<byte>(bytes, 0, a1.Length));
        b.CopyTo(new Span<byte>(bytes, a1.Length, b.Length));

        return new @string(bytes);
    }

    public static @string operator +(ReadOnlySpan<byte> a, @string b)
    {
        if (a.Length == 0)
            return b;

        ReadOnlySpan<byte> b1 = b.Bytes;
        byte[] bytes = AllocationCounter.NewArray<byte>(a.Length + b1.Length);

        a.CopyTo(new Span<byte>(bytes, 0, a.Length));
        b1.CopyTo(new Span<byte>(bytes, a.Length, b1.Length));

        return new @string(bytes);
    }

    #endregion

    #region [ Interface Implementations ]

    object ICloneable.Clone()
    {
        return Clone();
    }

    int IReadOnlyCollection<byte>.Count => Length;

    bool IConvertible.ToBoolean(IFormatProvider? provider)
    {
        return ((IConvertible)ToString()).ToBoolean(provider);
    }

    char IConvertible.ToChar(IFormatProvider? provider)
    {
        return ((IConvertible)ToString()).ToChar(provider);
    }

    sbyte IConvertible.ToSByte(IFormatProvider? provider)
    {
        return ((IConvertible)ToString()).ToSByte(provider);
    }

    byte IConvertible.ToByte(IFormatProvider? provider)
    {
        return ((IConvertible)ToString()).ToByte(provider);
    }

    short IConvertible.ToInt16(IFormatProvider? provider)
    {
        return ((IConvertible)ToString()).ToInt16(provider);
    }

    ushort IConvertible.ToUInt16(IFormatProvider? provider)
    {
        return ((IConvertible)ToString()).ToUInt16(provider);
    }

    int IConvertible.ToInt32(IFormatProvider? provider)
    {
        return ((IConvertible)ToString()).ToInt32(provider);
    }

    uint IConvertible.ToUInt32(IFormatProvider? provider)
    {
        return ((IConvertible)ToString()).ToUInt32(provider);
    }

    long IConvertible.ToInt64(IFormatProvider? provider)
    {
        return ((IConvertible)ToString()).ToInt64(provider);
    }

    ulong IConvertible.ToUInt64(IFormatProvider? provider)
    {
        return ((IConvertible)ToString()).ToUInt64(provider);
    }

    float IConvertible.ToSingle(IFormatProvider? provider)
    {
        return ((IConvertible)ToString()).ToSingle(provider);
    }

    double IConvertible.ToDouble(IFormatProvider? provider)
    {
        return ((IConvertible)ToString()).ToDouble(provider);
    }

    decimal IConvertible.ToDecimal(IFormatProvider? provider)
    {
        return ((IConvertible)ToString()).ToDecimal(provider);
    }

    DateTime IConvertible.ToDateTime(IFormatProvider? provider)
    {
        return ((IConvertible)ToString()).ToDateTime(provider);
    }

    object IConvertible.ToType(Type conversionType, IFormatProvider? provider)
    {
        return ((IConvertible)ToString()).ToType(conversionType, provider);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable<byte>)this).GetEnumerator();
    }

    // Walks the backing array over this string's window: an iterator method cannot hold the
    // ReadOnlySpan the rest of the type reads through (CS4013).
    IEnumerator<byte> IEnumerable<byte>.GetEnumerator()
    {
        byte[] backing = m_value ?? [];
        int end = m_offset + m_length;

        for (int i = m_offset; i < end; i++)
            yield return backing[i];
    }

    IEnumerator<rune> IEnumerable<rune>.GetEnumerator()
    {
        AllocationCounter.Count();

        foreach (rune codePoint in ToRunes())
            yield return codePoint;
    }

    IEnumerator<char> IEnumerable<char>.GetEnumerator()
    {
        AllocationCounter.Count();

        return ToString().GetEnumerator();
    }

    private const rune RuneReplacementChar = 0xFFFD;

    #endregion
}
