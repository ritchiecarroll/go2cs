// GoReflect.TypeLayout.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable InconsistentNaming

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using go.golib;
using static go2cs.Symbols;

namespace go;

// ---------------------------------------------------------------------------------------------
// TYPE LAYOUT — the SHAPE facts a Go type descriptor carries: size, alignment, array dimensions,
// and a func's parameter and result lists.
//
// WHAT LIVES HERE
//   `GoSizeOf`/`GoAlignOf` (what gets stamped into a synthesized descriptor's `Size_`/`Align_`),
//   the array-dimension recovery those need, and `TryFuncShape` (`rtype.NumIn`/`In`/`NumOut`/
//   `Out`/`IsVariadic`, and `Value.Call`'s argument marshalling).
//
// THESE ARE GO'S NUMBERS, NOT THE CLR'S — THAT IS THE WHOLE POINT
//   `GoSizeOf` returns the amd64 size of the GO type, computed from Go's own rules, and it has to,
//   because the managed representation's real size is unrelated: a Go `[]T` is 24 bytes while
//   `slice<T>` is 32; a Go `[2]byte` is 2 while `array<byte>` is one reference to a backing store.
//   (`@string` happens to be 16 bytes like a Go `string` now that it carries an offset/length
//   window, but that is a coincidence of the window's shape, not something a caller may rely on.)
//   Anything that reads a size and expects Go's answer — encoding/binary's `sizeof` is the
//   demonstrated consumer — would get nonsense from `Marshal.SizeOf` or `Unsafe.SizeOf`. Struct
//   sizes follow Go's alignment rules over the PROJECTED Go fields (see GoReflect.FieldAccess.cs),
//   which is best-effort composite fidelity and recorded as such.
//
// THE WALK DESCENDS ONLY THROUGH STRUCTS AND ARRAYS, WHICH IS WHAT MAKES IT FINITE
//   Every other kind is a fixed-size header and is answered without looking inside it — a pointer,
//   slice, map, chan, interface or func field is 8/24/8/8/16/8 bytes whatever it refers to. That is
//   Go's own layout rule, and it is the entire termination argument: only value types recurse, and
//   C# forbids a value type from containing itself (CS0523). It held only once `KindOf` stopped
//   calling an unrecognized managed REFERENCE a struct — until 2026-08-15 a `sync.Mutex`'s
//   `SemaphoreSlim` gate sent this walk into the BCL's own object graph (`SemaphoreSlim` →
//   `TaskNode` → `TaskNode`), where it exhausted the stack and killed the process.
//
//   UNIFIED 2026-08-09 (r56a): `unsafe.Sizeof` now answers through THIS rule too (see
//   core/unsafe/unsafe.cs), so a Go size has one definition in the runtime rather than two. The
//   named consumer the deferral (I2.R R-14) was waiting for arrived as three packages at once —
//   debug/macho, internal/xcoff and go/internal/gccgoimporter all reach `unsafe.Sizeof` through
//   `internal/saferio.SliceCap[E]` with E bound to a managed type, where the old `Marshal.SizeOf`
//   rule does not merely disagree with Go, it throws.
//
// WHY DIMENSIONS ARE RECOVERED FROM A VALUE AND NOT READ FROM THE TYPE
//   `array<T>` carries its element type and not its LENGTH, so the managed type alone cannot tell
//   `[4]T` from `[]T` — which is why size, name and zero-construction all take an optional dims
//   vector rather than deriving one. Dims come from a live value (walking the first element for the
//   nested case) or, for a struct FIELD, from a cached zero instance of the declaring struct: the
//   converter emits the Go dimension as a field initializer (`= new(4)`, nested
//   `new(128, () => new(4))`) that the generated parameterless constructor runs, so the dimension
//   is already sitting in the emitted C# and needs no attribute. An empty outer array is the one
//   case that stays unknowable — there is no first element to ask — and it answers `null` rather
//   than guessing.
//
//   A func PARAMETER is the position where neither source exists — no value, no initializer — and
//   the emitted delegate type is a bare `Func<array<byte>, bool>` that `func([32]byte) bool` and
//   `func([64]byte) bool` share. That one position therefore DOES need an attribute, and gets it:
//   the converter stamps `[GoArrayDims(32)]` on the parameter and `FuncParamDims` reads it back off
//   the delegate INSTANCE. See GoArrayDimsAttribute.
//
// FUNC SHAPE IS READ OFF `Invoke`, AND THE MULTI-RETURN RULE IS UNAMBIGUOUS
//   A `void` return is zero Go results; a `ValueTuple` return is Go's multi-return, unpacked to one
//   result per element; anything else is one result. That rule is safe precisely because a
//   converted Go struct is NEVER emitted as a `ValueTuple` — the converter mints a named struct —
//   so a tuple in return position can only have come from a multi-return signature. Variadic is
//   detected from the golib variadic delegate families, whose `params Span<T>` tail is reported as
//   Go's `[]T`.
// ---------------------------------------------------------------------------------------------
public static partial class GoReflect
{
    // -------- Go sizes (descriptor Size_/Align_ stamping; binary's sizeof reads scalars only) --------

    /// <summary>
    /// The Go (amd64) size of the type <paramref name="t"/> represents, or -1 when it cannot be
    /// known (an array whose length the managed type does not carry). Struct sizes follow Go's
    /// alignment rules over the PROJECTED Go fields — best-effort composite fidelity, recorded;
    /// the demonstrated consumer (encoding/binary's sizeof) reads only the scalar kinds.
    /// <c>unsafe.Sizeof</c> answers through this same rule (unified 2026-08-09), so a descriptor's
    /// stamped size and an <c>unsafe.Sizeof</c> of the same type can never disagree.
    /// </summary>
    public static nint GoSizeOf(Type t, nint[]? arrayDims = null)
    {
        return TryGoSizeOf(t, arrayDims, out nuint size) && size <= (nuint)nint.MaxValue ? (nint)size : -1;
    }

    /// <summary>
    /// The Go (amd64) size of the type <paramref name="t"/> represents, as the UNSIGNED width Go
    /// itself uses, with derivability answered by the return value rather than by the size.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="GoSizeOf"/>'s <c>-1</c> answers TWO different questions, and they stop being the
    /// same question at 2^63: Go's size is a <c>uintptr</c> and a legal Go struct really does reach
    /// 2^64-3 (<c>reflect.TestStructOfTooLarge</c> builds exactly that from two half-address-space
    /// arrays), where a signed answer goes NEGATIVE and is indistinguishable from "not derivable".
    /// Every caller of the signed form read the sentinel as "unknown", so a huge type did not merely
    /// lose precision — <c>unsafe.Sizeof</c> fell back to the managed marshalled size, a synthesized
    /// descriptor left <c>Size_</c> unstamped, and <c>reflect.StructOf</c>/<c>ArrayOf</c> skipped the
    /// address-space guard the huge type was the whole point of.
    /// </para>
    /// <para>
    /// A size that overflows the address space is reported as NOT derivable rather than saturated:
    /// Go answers that case with a panic inside the type constructor, so the panic stays where Go
    /// puts it and this reports only what it can describe.
    /// </para>
    /// </remarks>
    public static bool TryGoSizeOf(Type t, nint[]? arrayDims, out nuint size)
    {
        return tryGoSizeOf(t, arrayDims, 0, out size);
    }

    private static bool tryGoSizeOf(Type t, nint[]? arrayDims, int depth, out nuint size)
    {
        size = 0;

        if (depth > MaxLayoutDepth)
            return false;

        switch (KindOf(t))
        {
            case Bool or Int8 or Uint8: size = 1; return true;
            case Int16 or Uint16: size = 2; return true;
            case Int32 or Uint32 or Float32: size = 4; return true;
            case Int or Uint or Int64 or Uint64 or Uintptr or Float64 or Complex64: size = 8; return true;
            case Complex128 or String or Interface: size = 16; return true;
            case Slice: size = 24; return true;
            case Pointer or UnsafePointer or Map or Chan or Func: size = 8; return true;
            case Array:
            {
                if (arrayDims is not { Length: > 0 } || arrayDims[0] < 0)
                    return false;

                if (!tryGoSizeOf(ElementType(t)!, arrayDims.Length > 1 ? arrayDims[1..] : null, depth + 1, out nuint elemSize))
                    return false;

                nuint length = (nuint)arrayDims[0];

                if (elemSize != 0 && length > nuint.MaxValue / elemSize)
                    return false;

                size = elemSize * length;
                return true;
            }
            case Struct:
            {
                StructLayout layout = structLayoutOf(t, depth);
                size = layout.Size;
                return layout.Known;
            }
            default:
                return false;
        }
    }

    /// <summary>
    /// The Go (amd64) <c>PtrBytes</c> of the type <paramref name="t"/> represents — "the number of
    /// (prefix) bytes in the type that can contain pointers", which is what
    /// <c>abi.Type.Pointers()</c> tests and what the garbage collector would scan. 0 for a type that
    /// holds no pointer at all, and -1 when it cannot be known (the same unknowable-array case
    /// <see cref="GoSizeOf"/> reports, since one unknown element size makes the prefix a guess).
    /// </summary>
    /// <remarks>
    /// <para>
    /// It reads the SAME memoized struct pass as <see cref="GoSizeOf"/>, <see cref="GoAlignOf"/> and
    /// <see cref="GoFieldOffsets"/>, so a descriptor's stamped <c>Size_</c> and <c>PtrBytes</c> can
    /// never disagree about one type — the property that made this a sibling of those rather than a
    /// second layout model.
    /// </para>
    /// <para>
    /// The per-kind values are Go's own memory shapes, not C#'s: a <c>string</c> is {ptr, len} so its
    /// pointer prefix is ONE word while its size is two; a slice is {ptr, len, cap} so likewise 8 of
    /// 24; an interface is {type, data} and BOTH words are scanned, so 16 of 16. An array's prefix
    /// runs to the last element that can hold a pointer — <c>(n-1)*elemSize + elemPtrBytes</c>, and 0
    /// when the element holds none, which is why a <c>[1000]int</c> costs the collector nothing.
    /// A struct's prefix ends at its last pointer-bearing field, so it is the max of
    /// <c>offset + fieldPtrBytes</c> and 0 when no field qualifies.
    /// </para>
    /// <para>
    /// The demonstrated consumer is reflect's <c>funcLayout</c>: <c>addTypeBits</c> builds a frame's
    /// pointer bitmap from each parameter's <c>Kind()</c>, but returns immediately unless
    /// <c>Pointers()</c> is true, and <c>Pointers()</c> is exactly <c>PtrBytes != 0</c>. With
    /// PtrBytes unstamped every synthesized descriptor answered "I hold no pointers", so the stack
    /// bitmap came out empty and the frame's own PtrBytes — which funcLayout derives FROM that
    /// bitmap — was then zero as well, emptying the GC bitmap too.
    /// </para>
    /// </remarks>
    public static nint GoPtrBytesOf(Type t, nint[]? arrayDims = null)
    {
        // Memoized for the dims-less case, which is every synthType call that is not an array —
        // i.e. the hot one. It has to be: synthType runs per descriptor synthesis, and an
        // unmemoized walk here made crypto/internal/nistec go from 354s to past its 600s deadline
        // (measured 2026-09-01 by the reflect-consumer canary set, which is what that gate is for).
        // structLayoutOf's own memo does not cover this: it caches a struct's offsets and size, not
        // the pointer-prefix walk over them, and the recursion into field types is the expensive part.
        // A dims-carrying array is NOT cached — nint[] has reference identity, not value identity,
        // so it cannot key a memo correctly, and those calls are rare.
        if (arrayDims is null)
            return s_ptrBytes.GetOrAdd(t, static key => goPtrBytesOf(key, null, 0));

        return goPtrBytesOf(t, arrayDims, 0);
    }

    private static readonly ConcurrentDictionary<Type, nint> s_ptrBytes = new();

    /// <summary>
    /// Reports whether a Go type is POINTER-SHAPED — Go's <c>isDirectIface</c> — i.e. whether an
    /// interface holding it stores the value itself in the data word rather than a pointer to a copy.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Go's rule is mechanical: a pointer, chan, map, func or unsafe.Pointer is pointer-shaped; an
    /// array is pointer-shaped only when it has EXACTLY one element and that element is itself
    /// pointer-shaped; a struct only when it has EXACTLY one field, likewise. Everything else is not.
    /// The one-element condition is the whole distinction reflect's own TestArrayOfDirectIface and
    /// TestStructOfDirectIface turn on: <c>[1]*byte</c> is pointer-shaped and <c>[0]*byte</c> is not,
    /// however pointer-shaped the element type is.
    /// </para>
    /// <para>
    /// This is a CLASSIFICATION, not a memory-layout claim. It answers what SHAPE a Go type has; it
    /// does not assert that the managed bridge stores such a value inline, because it does not — a
    /// bridge <c>reflect.Value</c> carries a boxed managed object and its <c>ptr</c> word is unused.
    /// Callers that need the classification (reflect's <c>InterfaceData</c>) may read it; callers
    /// that would use it to SELECT a memory model must not. See the note at
    /// <see cref="GoPtrBytesOf"/> for the sibling that does describe layout.
    /// </para>
    /// <para>
    /// An array whose dimensions are unknown answers FALSE (not pointer-shaped) rather than
    /// guessing, which is the conservative direction: it is what every descriptor already reported
    /// before this predicate existed, so an unknown-dims type cannot change behavior by consulting it.
    /// </para>
    /// </remarks>
    public static bool GoIsDirectIface(Type t, nint[]? arrayDims = null)
    {
        // Memoized on the dims-less path for the same reason GoPtrBytesOf is: the struct arm
        // recurses through GoFields, and this is consulted per descriptor synthesis.
        if (arrayDims is null)
            return s_directIface.GetOrAdd(t, static key => goIsDirectIface(key, null, 0));

        return goIsDirectIface(t, arrayDims, 0);
    }

    private static readonly ConcurrentDictionary<Type, bool> s_directIface = new();

    private static bool goIsDirectIface(Type t, nint[]? arrayDims, int depth)
    {
        if (depth > MaxLayoutDepth)
            return false;

        switch (KindOf(t))
        {
            case Pointer or UnsafePointer or Map or Chan or Func:
                return true;

            case Array:
            {
                // Exactly one element, and that element pointer-shaped in turn. Unknown dims
                // answer false — see the remarks: the conservative direction.
                if (arrayDims is not { Length: > 0 } || arrayDims[0] != 1)
                    return false;

                Type? elem = ElementType(t);

                if (elem is null)
                    return false;

                return goIsDirectIface(elem, arrayDims.Length > 1 ? arrayDims[1..] : null, depth + 1);
            }

            case Struct:
            {
                GoFieldInfo[] fields = GoFields(t);

                return fields.Length == 1 && goIsDirectIface(fields[0].Type, fields[0].ArrayDims, depth + 1);
            }

            default:
                return false;
        }
    }

    /// <summary>
    /// For a pointer-shaped type (one <see cref="GoIsDirectIface"/> answers true for), reports
    /// whether the single pointer the value reduces to is nil.
    /// </summary>
    /// <remarks>
    /// A pointer-shaped type reduces, by the same one-element array / one-field struct rule
    /// <see cref="GoIsDirectIface"/> walks, to exactly ONE pointer — which is why an interface can
    /// hold it in the data word at all. This answers that pointer's nilness, so a caller
    /// reconstructing an interface data word can distinguish the zero value (a nil pointer, word 0)
    /// from a live one. It lives beside the predicate because the two share the reduction; it
    /// answers a VALUE question where the predicate answers a TYPE one.
    /// </remarks>
    public static bool GoDirectIfaceWordIsNil(object? value, Type t, nint[]? arrayDims = null)
    {
        return goDirectIfaceWordIsNil(value, t, arrayDims, 0);
    }

    private static bool goDirectIfaceWordIsNil(object? value, Type t, nint[]? arrayDims, int depth)
    {
        // A missing value reduces to a nil word, and so does anything we cannot walk: the caller
        // asked about a type it had already classified pointer-shaped, so an unwalkable shape here
        // is a bridge gap, and "nil" is the answer that reports the zero value rather than
        // inventing a live pointer.
        if (value is null || depth > MaxLayoutDepth)
            return true;

        // Unwrap adapter carriers FIRST, and unwrap them exactly as GoDynamicTypeOf does — the
        // caller pairs this value with the type GoDynamicTypeOf reported, so the two walks have to
        // agree about what the value IS or the type says "array" while the value is a shell. Note
        // IsNilGoValue does NOT unwrap (it probes the value's own type for `== nil`), which is why
        // reflectPointerToken unwraps before calling it and why this must too: an adapter-wrapped
        // nil pointer answers its SHELL's nilness otherwise, i.e. "not nil", inverting the word.
        while (value is IInterfaceAdapter { Value: not null } interfaceAdapter)
            value = interfaceAdapter.Value;

        if (value is IжAdapter { Box: not null } pointerAdapter)
            value = pointerAdapter.Box;
        else if (value is IValueAdapter { Value: not null } valueAdapter)
            value = valueAdapter.Value;

        switch (KindOf(t))
        {
            case Pointer or UnsafePointer or Map or Chan or Func:
                return IsNilGoValue(value);

            case Array:
            {
                Type? elem = ElementType(t);

                if (elem is null)
                    return true;

                return goDirectIfaceWordIsNil(firstArrayElement(value, elem), elem,
                    arrayDims is { Length: > 1 } ? arrayDims[1..] : null, depth + 1);
            }

            case Struct:
            {
                GoFieldInfo[] fields = GoFields(t);

                return fields.Length != 1 ||
                       goDirectIfaceWordIsNil(fields[0].Read(value), fields[0].Type, fields[0].ArrayDims, depth + 1);
            }

            default:
                return true;
        }
    }

    private static nint goPtrBytesOf(Type t, nint[]? arrayDims, int depth)
    {
        if (depth > MaxLayoutDepth)
            return -1;

        switch (KindOf(t))
        {
            // Scalars hold no pointer, so the collector scans none of them.
            case Bool or Int8 or Uint8 or Int16 or Uint16 or Int32 or Uint32 or Float32
                 or Int or Uint or Int64 or Uint64 or Uintptr or Float64
                 or Complex64 or Complex128:
                return 0;

            // One pointer word, and it is first: {ptr, len} and {ptr, len, cap}.
            case String or Slice:
                return 8;

            // Both words of an interface are scanned.
            case Interface:
                return 16;

            case Pointer or UnsafePointer or Map or Chan or Func:
                return 8;

            case Array:
            {
                if (arrayDims is not { Length: > 0 })
                    return -1;

                Type? elem = ElementType(t);

                if (elem is null)
                    return -1;

                nint[]? elemDims = arrayDims.Length > 1 ? arrayDims[1..] : null;
                nint elemPtrBytes = goPtrBytesOf(elem, elemDims, depth + 1);

                // A pointer-free element makes the whole array pointer-free however long it is.
                if (elemPtrBytes <= 0)
                    return elemPtrBytes;

                if (arrayDims[0] <= 0)
                    return 0;

                // The prefix arithmetic below is nint, so an element too large to name there is
                // an unanswerable PtrBytes rather than a truncated one.
                if (!tryGoSizeOf(elem, elemDims, depth + 1, out nuint elemSizeBytes) || elemSizeBytes > (nuint)nint.MaxValue)
                    return -1;

                return (arrayDims[0] - 1) * (nint)elemSizeBytes + elemPtrBytes;
            }

            case Struct:
            {
                StructLayout layout = structLayoutOf(t, depth);

                if (!layout.Known)
                    return -1;

                GoFieldInfo[] fields = GoFields(t);

                if (fields.Length != layout.Offsets.Length)
                    return -1;

                nint ptrBytes = 0;

                for (int i = 0; i < fields.Length; i++)
                {
                    nint fieldPtrBytes = goPtrBytesOf(fields[i].Type, fields[i].ArrayDims, depth + 1);

                    if (fieldPtrBytes < 0)
                        return -1;

                    if (fieldPtrBytes == 0)
                        continue;

                    nint end = layout.Offsets[i] + fieldPtrBytes;

                    if (end > ptrBytes)
                        ptrBytes = end;
                }

                return ptrBytes;
            }

            default:
                return -1;
        }
    }

    /// <summary>
    /// The Go (amd64) byte OFFSET of each projected Go field of the struct type
    /// <paramref name="t"/>, in <see cref="GoFields"/> order — or <c>null</c> when
    /// <paramref name="t"/> is not a struct kind, or when any field's Go size cannot be known
    /// (an array whose length the managed type does not carry), since one unknown size makes
    /// every LATER offset a guess rather than an answer.
    /// </summary>
    /// <remarks>
    /// Offsets, <see cref="GoSizeOf"/> and <see cref="GoAlignOf"/> all read the SAME memoized layout
    /// pass, so a descriptor's stamped <c>Size_</c>, its <c>Align_</c> and its fields'
    /// <c>Offset</c>s can never disagree about one struct. The demonstrated consumer
    /// is internal/abi's synthesized <c>StructType()</c> specialization, which is what
    /// <c>unique.buildStructCloneSeq</c> walks to find the string offsets inside a value.
    /// </remarks>
    public static nint[]? GoFieldOffsets(Type t)
    {
        return KindOf(t) == Struct && structLayoutOf(t, 0) is { Known: true } layout ? layout.Offsets : null;
    }

    /// <summary>The Go (amd64) alignment of a type (struct = max field alignment; array = element alignment).</summary>
    public static nint GoAlignOf(Type t)
    {
        return goAlignOf(t, 0);
    }

    private static nint goAlignOf(Type t, int depth)
    {
        if (depth > MaxLayoutDepth)
            return 8;

        switch (KindOf(t))
        {
            case Bool or Int8 or Uint8: return 1;
            case Int16 or Uint16: return 2;
            case Int32 or Uint32 or Float32 or Complex64: return 4;
            case Array: return ElementType(t) is { } elem ? goAlignOf(elem, depth + 1) : 8;
            case Struct: return structLayoutOf(t, depth).Align;
            default: return 8;
        }
    }

    // -------- the one struct layout walk (offsets, size and alignment from a single pass) --------

    /// <summary>A struct's Go layout: per-field offsets, the aligned total size (-1 when unknowable), and the struct's own alignment.</summary>
    // Size is UNSIGNED and derivability is its own flag rather than a negative sentinel: a legal
    // Go struct reaches 2^64-3, where a signed size is negative and indistinguishable from
    // "unknown" (see TryGoSizeOf). The pass already computed this bool as a local; it now carries
    // it instead of encoding it.
    private readonly record struct StructLayout(nint[] Offsets, nuint Size, nint Align, bool Known);

    private static readonly ConcurrentDictionary<Type, StructLayout> s_structLayouts = new();

    // A recursion ceiling no LEGAL graph can reach, kept as a safety net rather than an algorithm.
    // Only Struct and Array recurse, Struct is answered for value types alone, and C# forbids a value
    // type from containing itself transitively (CS0523) — so the depth of this walk is bounded by a
    // real nesting depth the compiler already had to accept. Tripping the cap therefore means the
    // CLASSIFICATION is wrong somewhere, and the honest answer to that is "size unknown" (the r39d
    // rule: a descriptor field that cannot be read truthfully stays unpopulated), never a stack
    // overflow — which takes the whole process, and with it every verdict the run had not yet
    // produced. That is exactly what a managed reference classified as Struct once cost here.
    private const int MaxLayoutDepth = 128;

    // Go's goarch.PtrSize on the platforms this corpus targets. Named rather than spelled 8 at each
    // use so the GC-mask walk's granularity reads as "one entry per pointer word" and not as an
    // arbitrary divisor -- the distinction the mask's own doc comment turns on.
    private const nint GoWordSize = 8;

    // The one Go struct layout walk. Offsets, size and alignment come out of a SINGLE pass and are
    // memoized per type, so no two of them can describe different shapes, and a struct reached once
    // per field of every enclosing struct is walked once in total.
    //
    // Alignment is accumulated over every field even after a size becomes unknowable, because the two
    // questions are independent: an array whose dims the managed type does not carry has no knowable
    // size, while its alignment is its element's and stays an answer.
    private static StructLayout structLayoutOf(Type t, int depth)
    {
        if (s_structLayouts.TryGetValue(t, out StructLayout cached))
            return cached;

        if (depth > MaxLayoutDepth)
            return new StructLayout([], 0, 8, false);

        GoFieldInfo[] fields = GoFields(t);
        nint[] offsets = new nint[fields.Length];
        nuint size = 0;
        nint maxAlign = 1;
        bool sizeKnown = true;

        for (int i = 0; i < fields.Length; i++)
        {
            GoFieldInfo field = fields[i];
            nint align = goAlignOf(field.Type, depth + 1);
            maxAlign = align > maxAlign ? align : maxAlign;

            if (!sizeKnown)
                continue;

            nint[]? dims = KindOf(field.Type) == Array ? field.ArrayDims : null;

            if (!tryGoSizeOf(field.Type, dims, depth + 1, out nuint fieldSize))
            {
                sizeKnown = false;
                continue;
            }

            nuint alignment = (nuint)align;
            nuint aligned = (size + alignment - 1) / alignment * alignment;

            // Rounding up, adding a field, or naming an offset past the address space describes no
            // layout this can report. Go answers each of them with a panic inside StructOf, so the
            // panic stays there and the QUERY simply says it cannot tell. The offset ceiling is the
            // narrower of the three -- Offsets is nint, and only a struct already past 2^63 can
            // reach it, which is the same population as the other two.
            if (aligned < size || aligned > (nuint)nint.MaxValue)
            {
                sizeKnown = false;
                continue;
            }

            offsets[i] = (nint)aligned;
            size = aligned + fieldSize;

            if (size < aligned)
            {
                sizeKnown = false;
            }
        }

        nuint totalAlign = (nuint)maxAlign;
        nuint total = sizeKnown ? (size + totalAlign - 1) / totalAlign * totalAlign : 0;

        if (sizeKnown && total < size)
            sizeKnown = false;

        StructLayout layout = sizeKnown
            ? new StructLayout(offsets, total, maxAlign, true)
            : new StructLayout([], 0, maxAlign, false);

        s_structLayouts[t] = layout;
        return layout;
    }

    // -------- array dimension recovery (descriptor cargo; canonType interning is NOT widened) --------

    private static readonly ConcurrentDictionary<Type, object?> s_zeroInstances = new();

    /// <summary>
    /// The array dims of a LIVE array value (nested dims walk the first element), or null when
    /// unknown (a null/zero-length backing cannot reveal nested dims).
    /// </summary>
    public static nint[]? ArrayDimsOfValue(object? value)
    {
        // An ISlice is an IArray but its Length is not a dimension: refuse it here so no caller can reach
        // the hole (defense in depth beside elemArrayDims).
        if (value is ISlice || value is not IArray arr)
            return null;

        nint length = arr.Length;
        Type? elem = ElementType(value.GetType());

        if (elem is null || KindOf(elem) != Array)
            return [length];

        if (length == 0)
            return null; // nested dims unknowable from an empty outer

        object? first = firstArrayElement(value, elem);
        nint[]? inner = ArrayDimsOfValue(first);

        return inner is null ? null : [length, .. inner];
    }

    /// <summary>
    /// The array dims of the value BEHIND a live pointer — <c>*[3]int</c> reports <c>[3]</c> — or
    /// null when <paramref name="value"/> is not a pointer to an array whose length a source knows.
    /// </summary>
    /// <remarks>
    /// A POINTER descriptor carries its POINTEE's dims unshifted (a pointer has no length of its
    /// own), which is the rule <c>abi.Type.Elem</c> and <c>rtype.Elem</c> already apply when they
    /// hand the cargo down. Nothing was populating it: <c>abi.TypeOf</c> measured dims for an ARRAY
    /// value only, so <c>reflect.TypeOf(new([3]int)).Elem()</c> described a dimension-LESS
    /// <c>[N]int</c> and <c>reflect.New</c> of it allocated a ZERO-length array. That is not a
    /// cosmetic loss — the fresh value then has a different Type from the one it is supposed to
    /// mirror, so <c>reflect.DeepEqual(new([3]int), reflect.New(typ).Interface())</c> is false, which
    /// is the precondition encoding/json's whole TestUnmarshal table checks before every subtest.
    /// </remarks>
    // A CONTAINER value's element cargo, measured off a PRESENT element or entry -- the arm abi.TypeOf
    // already uses one kind over (ArrayDimsOfValue reads a nested array through its first element).
    // null when the container is nil or empty: an empty [][6]uint8 has nothing to measure, and that
    // is increment B's stated boundary (DESIGN-descriptor-cargo.md section 12.2), not a value to invent.
    // Increment C lifts that boundary for slices by RECORDING the length at the creation site instead
    // of measuring it here; observation below remains the fallback.

    // ==== the element-dimension side table (descriptor cargo, increment C) ==========================
    //
    // A Go [][3]uint8 and a [][4]uint8 are ONE managed type, slice<array<uint8>>: an array's LENGTH is
    // a constructor argument here, not a type parameter. The length is therefore recovered by
    // OBSERVING an element -- and a slice with no elements has none to observe, so
    // reflect.TypeOf([][3]uint8{}) could not tell its own element length and stopped being
    // identity-equal to reflect.SliceOf(reflect.ArrayOf(3, byteT)) (the ReflectArrayOf regression,
    // 2026-09-03). The fact is not lost, it is DROPPED: the converter knows the length statically at
    // every site that creates such a slice. This table carries it from there to here.
    //
    // Keyed on the BACKING ARRAY, which is the identity the dims belong to: a reslice shares it and so
    // inherits the dims for free. Weak, so recording a length never keeps a backing store alive. The
    // cost falls on the TypeOf path only -- no slice operation touches it, and no slice grows by a
    // byte (the +8 B-per-slice-header alternative was measured and declined: 130 creation sites in the
    // stdlib would need it, 27,143 would pay for it).
    //
    // KNOWN BOUNDARY, recorded rather than papered over: a NIL slice has no backing object to key on,
    // and neither does a slice whose backing was replaced by a reallocating append. Both keep the
    // observation-only answer. No case in the corpus or the roster reaches either; the remedy the day
    // one does is the +8 B field on the slice header.
    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<object, nint[]> s_sliceElemDims = new();

    // Records the element array dims of a freshly created slice-of-array, and answers the slice they
    // were recorded ON -- usually the one passed in.
    //
    // The one substitution: Array.Empty<T>() is a SINGLETON shared by every length, so recording
    // against it would make [][3]uint8 and [][4]uint8 collide on one key and answer each other's
    // length. A fresh zero-length backing is substituted instead of refusing -- make([][3]uint8, 0) is
    // a legal Go program and must not throw. Measured on this runtime: new T[0] allocates a distinct
    // object per call and is NOT folded to the singleton, so two empty slices key apart.
    public static slice<T> WithElemDims<T>(slice<T> s, params nint[] dims)
    {
        if (dims is null || dims.Length == 0)
            return s;

        T[]? backing = s.m_array;

        // A nil slice has no backing to key on -- the recorded boundary above. Substituting one here
        // would make it non-nil, so `x == nil` would answer differently: leave it exactly as it is.
        if (backing is null)
            return s;

        if (s.Length == 0 && ReferenceEquals(backing, System.Array.Empty<T>()))
        {
            s = new slice<T>(new T[0]);
            backing = s.m_array;
        }

        if (backing is not null)
            s_sliceElemDims.AddOrUpdate(backing, dims);

        return s;
    }

    public static nint[]? SliceElemArrayDims(object? value)
    {
        object? box = unwrapAdapters(value);

        // The recorded length first, observation as the fallback: a slice that HAS an element still
        // answers from it, so nothing that was already right becomes wrong.
        if (box is ISliceBacking { Backing: { } backing } && s_sliceElemDims.TryGetValue(backing, out nint[]? recorded))
            return recorded;

        return box is ISlice { Length: > 0 } s ? elemArrayDims(((IArray)s)[0]) : null;
    }

    public static nint[]? MapKeyArrayDims(object? value)
    {
        return firstMapEntry(unwrapAdapters(value), out object? key, out _) ? elemArrayDims(key) : null;
    }

    public static nint[]? MapElemArrayDims(object? value)
    {
        return firstMapEntry(unwrapAdapters(value), out _, out object? elem) ? elemArrayDims(elem) : null;
    }

    // The element's own dims: an array measures itself, a pointer measures its pointee (any depth).
    private static nint[]? elemArrayDims(object? element)
    {
        // A SLICE contributes no dims: its length is a property of the VALUE, not of the type -- stamping
        // it made TypeOf(map[string][]string) depend on the first enumerated entry's length (net/http's
        // http.Header DeepEqual regression, 2026-09-03). Only an array<T> measures itself; ISlice : IArray,
        // so the slice test comes first -- the same predicate trap the array-range Clone defect hit in
        // array.cs this week.
        if (element is ISlice)
            return null;
        return element is IArray ? ArrayDimsOfValue(element) : PointeeArrayDims(element);
    }

    // First entry of a map through its non-generic enumerator; the boxed KeyValuePair is read by
    // reflection so IMap (more than one implementer) need not widen for a path only TypeOf takes.
    private static bool firstMapEntry(object? map, out object? key, out object? value)
    {
        key = null;
        value = null;

        if (map is not IMap { Length: > 0 } || map is not IEnumerable entries)
            return false;

        foreach (object? entry in entries)
        {
            if (entry is null)
                continue;

            Type pair = entry.GetType();
            key = pair.GetProperty("Key")?.GetValue(entry);
            value = pair.GetProperty("Value")?.GetValue(entry);
            return true;
        }

        return false;
    }

    public static nint[]? PointeeArrayDims(object? value)
    {
        object? box = unwrapAdapters(value);

        // A nil pointer minted by a `(*[N]E)(nil)` conversion CARRIES its dims, because the
        // construction is the only place they could ride: there is no pointee to measure and no
        // attribute slot at an expression position. Consulted ahead of the nil refusal below, and
        // ONLY that shape answers here — a live pointer still measures its pointee, and a plain
        // typed nil still answers "nothing to measure".
        if (box is IGoNilArrayPointer { Dims.Length: > 0 } nilArray)
            return toNintDims(nilArray.Dims);

        // A nil pointer has nothing to measure, and an opaque managed handle has no pointee at all
        // (the descent rule’s value-side twin — see TryPointerBoxElement).
        if (box is null || box is INilPointer { IsNilPointer: true } ||
            !TryPointerBoxElement(box.GetType(), out Type? pointee) || KindOf(pointee) != Array)
        {
            return null;
        }

        return ArrayDimsOfValue(ReadPointerSlot(box));
    }

    /// <summary>
    /// The POINTEE type of a boxed Go pointer value — the managed answer to <c>(*T)</c>'s
    /// <c>T</c> — or null when <paramref name="value"/> is not a Go pointer at all.
    /// </summary>
    /// <remarks>
    /// Unlike <see cref="PointeeArrayDims"/> this asks the TYPE, so a nil pointer answers
    /// normally: <c>ж&lt;T&gt;</c> is a class and a nil one still reports <c>T</c>. Nilness only
    /// bars READING the pointee, which is why the nil test lives in the dims path and not here.
    /// </remarks>
    public static Type? PointeeTypeOfValue(object? value)
    {
        object? box = unwrapAdapters(value);

        return box is not null && TryPointerBoxElement(box.GetType(), out Type? pointee) ? pointee : null;
    }

    // The adapter hops a boxed Go value may carry before the pointer box itself is reached: an
    // interface value wraps its dynamic value, and a named-pointer adapter wraps the ж box. Both
    // value-side descents above need the same unwrap, so it is written once rather than twice.
    private static object? unwrapAdapters(object? value)
    {
        object? box = value;

        while (box is IInterfaceAdapter { Value: not null } interfaceAdapter)
            box = interfaceAdapter.Value;

        if (box is IжAdapter { Box: not null } pointerAdapter)
            box = pointerAdapter.Box;

        return box;
    }

    private static object? firstArrayElement(object arrayValue, Type elemType)
    {
        MethodInfo reader = typeof(GoReflect).GetMethod(nameof(readFirstElement), BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(elemType);
        return reader.Invoke(null, [arrayValue]);
    }

    private static object? readFirstElement<E>(object arrayValue)
    {
        return arrayValue is IArray<E> typed ? typed[0] : null;
    }

    /// <summary>
    /// The array dims of an array-typed STRUCT FIELD — from the field's <c>[GoArrayDims]</c> STAMP
    /// when it carries one, and otherwise recovered from a cached zero instance of the declaring
    /// struct, because the converter emits the Go dimension as a field initializer
    /// (<c>= new(4)</c>, nested <c>new(128, () =&gt; new(4))</c>) that the generated parameterless
    /// constructor runs, so a CONVERTED struct's dims are already in the emitted C# with no
    /// attribute needed.
    /// </summary>
    /// <remarks>
    /// The stamp is consulted FIRST, and the order is the whole point rather than an optimization.
    /// A struct minted by <see cref="GoStructSynthesis"/> — every <c>reflect.StructOf</c> result —
    /// knows its array fields' dims at MINT time and now stamps them; asking for those dims used to
    /// mean instantiating the struct and MEASURING the field, which allocates. Go computes such a
    /// length rather than allocating it, so a `StructOf` with a 2^63-element array field answered
    /// Go's question instantly and killed the process here: reflect's <c>TestStructOfTooLarge</c>,
    /// reported as an infrastructure-error because no failure survives to be reported.
    ///
    /// The zero-instance route stays for the converted case, where the datum genuinely lives in the
    /// initializer and there is no stamp — and note that the cache itself is shared with
    /// <see cref="FieldChanDir"/>, which recovers a channel field's direction the same way and is
    /// untouched by this.
    /// </remarks>
    public static nint[]? FieldArrayDims(Type declaringType, FieldInfo field)
    {
        if (FieldStampedDims(field) is { Length: > 0 } stamped)
            return stamped;

        if (!declaringType.IsValueType)
            return null;

        object? zero = s_zeroInstances.GetOrAdd(declaringType, static t => Activator.CreateInstance(t));
        return zero is null ? null : ArrayDimsOfValue(field.GetValue(zero));
    }

    /// <summary>
    /// Go's GC pointer bitmap for the type <paramref name="t"/> represents: one byte per POINTER
    /// WORD of the object, <c>1</c> where that word holds a pointer the collector must scan and
    /// <c>0</c> where it does not, from the object's base upward. Null when the type's size or
    /// layout is not derivable.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the same truth <see cref="GoPtrBytesOf"/> reports at coarser resolution — that one
    /// answers where the LAST pointer word ends, this one answers WHICH words they are — and it
    /// reads the same memoized layout pass, so a type's mask and its <c>PtrBytes</c> can never
    /// disagree. The per-kind pointer words are Go's own memory shapes, not C#'s: a string is
    /// {ptr,len} so word 0 only; a slice is {ptr,len,cap}, likewise word 0; an interface is
    /// {type,data} and BOTH words are scanned; an array repeats its element's mask; a struct places
    /// each field's mask at that field's offset.
    /// </para>
    /// <para>
    /// The GRANULARITY is one entry per pointer-word, taken from <c>runtime.getgcmask</c>'s own
    /// construction (<c>make([]byte, n/goarch.PtrSize)</c>, indexed <c>[i/goarch.PtrSize]</c>) and
    /// NOT from reflect's doc comment, which says "one entry per byte" about the bitmap's storage.
    /// The distinction is load-bearing: <c>verifyGCBits</c> accepts a mask LONGER than expected
    /// (Go's iterator runs out to the size class) but compares by prefix, so a byte-vs-word
    /// transposition fails everywhere while a longer answer passes.
    /// </para>
    /// </remarks>
    public static byte[]? GoGCMaskOf(Type t, nint[]? arrayDims = null)
    {
        // A mask is one BYTE per pointer word, so a type large enough to need more words than an
        // array can hold has no representable mask -- reported as no mask at all, which is what Go
        // reports for a noscan span. That was already the outcome through GoSizeOf's negative
        // sentinel; it is now the stated reason rather than an accident of the signed width.
        if (!TryGoSizeOf(t, arrayDims, out nuint size) || size / (nuint)GoWordSize > int.MaxValue)
            return null;

        byte[] mask = new byte[size / (nuint)GoWordSize];

        return fillGCMask(mask, 0, t, arrayDims, 0) ? mask : null;
    }

    // Sets the pointer words of `t` into `mask`, at `wordBase` words from the object's base.
    // Returns false when the type's layout is not derivable, which makes the whole mask unanswerable
    // rather than silently short — a mask that is WRONG passes no prefix check, and one that is
    // merely absent is an honest "cannot say".
    private static bool fillGCMask(byte[] mask, nint wordBase, Type t, nint[]? arrayDims, int depth)
    {
        if (depth > MaxLayoutDepth)
            return false;

        void set(nint word)
        {
            if (word >= 0 && word < mask.Length)
                mask[word] = 1;
        }

        switch (KindOf(t))
        {
            case Bool or Int8 or Uint8 or Int16 or Uint16 or Int32 or Uint32 or Float32
                 or Int or Uint or Int64 or Uint64 or Uintptr or Float64
                 or Complex64 or Complex128:
                return true;

            // {ptr, len} and {ptr, len, cap}: the pointer is first and it is the only one.
            case String or Slice:
                set(wordBase);
                return true;

            // {type, data} — both words are scanned.
            case Interface:
                set(wordBase);
                set(wordBase + 1);
                return true;

            case Pointer or UnsafePointer or Map or Chan or Func:
                set(wordBase);
                return true;

            case Array:
            {
                if (arrayDims is not { Length: > 0 })
                    return false;

                Type? elem = ElementType(t);

                if (elem is null)
                    return false;

                nint[]? elemDims = arrayDims.Length > 1 ? arrayDims[1..] : null;

                // A pointer-free element makes the whole array pointer-free however long it is,
                // which is also what keeps a [1_000_000]int from costing a million iterations here.
                nint elemPtrBytes = goPtrBytesOf(elem, elemDims, depth + 1);

                if (elemPtrBytes < 0)
                    return false;

                if (elemPtrBytes == 0)
                    return true;

                if (!tryGoSizeOf(elem, elemDims, depth + 1, out nuint elemSizeBytes) ||
                    elemSizeBytes == 0 || elemSizeBytes > (nuint)nint.MaxValue)
                {
                    return false;
                }

                nint elemSize = (nint)elemSizeBytes;

                for (nint i = 0; i < arrayDims[0]; i++)
                {
                    nint at = wordBase + i * elemSize / GoWordSize;

                    if (at >= mask.Length)
                        break;

                    if (!fillGCMask(mask, at, elem, elemDims, depth + 1))
                        return false;
                }

                return true;
            }

            case Struct:
            {
                StructLayout layout = structLayoutOf(t, depth);

                if (!layout.Known)
                    return false;

                GoFieldInfo[] fields = GoFields(t);

                if (fields.Length != layout.Offsets.Length)
                    return false;

                for (int i = 0; i < fields.Length; i++)
                {
                    if (!fillGCMask(mask, wordBase + layout.Offsets[i] / GoWordSize, fields[i].Type, fields[i].ArrayDims, depth + 1))
                        return false;
                }

                return true;
            }

            default:
                return false;
        }
    }

    /// <summary>
    /// The array dims a STRUCT FIELD carries as a converter STAMP — <c>[GoArrayDims]</c>, the dims
    /// the field's descriptor hands down through <c>Elem()</c> — or null when the field carries none.
    /// </summary>
    /// <remarks>
    /// The stamp exists for the two hops <see cref="FieldArrayDims"/>' zero-instance route cannot
    /// see: a nil <c>ж&lt;array&lt;T&gt;&gt;</c> has no pointee to measure, and a nil map has no
    /// entry whose element could reveal a length. Both are ordinary shapes at a DECODE target —
    /// encoding/gob reaches <c>*[3]float64</c> and <c>map[[2]string][2]*float64</c> fields through
    /// <c>reflect.Type.Field(i)</c> on a struct nothing has populated yet — so the datum has to be in
    /// the emitted C#, exactly as it is for a func parameter.
    /// </remarks>
    public static nint[]? FieldStampedDims(FieldInfo field)
    {
        return field.GetCustomAttributes(typeof(GoArrayDimsAttribute), false) is [GoArrayDimsAttribute { Dims.Length: > 0 } stamped]
            ? toNintDims(stamped.Dims)
            : null;
    }

    // -------- TYPE-level descriptor cargo (a DEFINED type's stamp beside its [GoType] marker) --------

    private static readonly ConcurrentDictionary<Type, nint[]?> s_typeStampedDims = new();

    /// <summary>
    /// The array dims a DEFINED type carries on itself -- <c>[GoArrayDims(N)]</c> on the wrapper class of
    /// <c>type P *[N]T</c> (increment E3 follow-up 7g) -- or null when the type carries none. The twin of
    /// <see cref="TypeStampedChanDirChain"/>, read the same way.
    /// </summary>
    public static nint[]? TypeStampedDims(Type t)
    {
        return s_typeStampedDims.GetOrAdd(t, static type =>
            type.GetCustomAttributes(typeof(GoArrayDimsAttribute), false) is [GoArrayDimsAttribute { Dims.Length: > 0 } stamped]
                ? toNintDims(stamped.Dims)
                : null);
    }

    private static readonly ConcurrentDictionary<Type, GoChanDir[]?> s_typeStampedChanDirChains = new();

    /// <summary>
    /// The channel direction chain a DEFINED channel type carries on itself -- <c>[GoChanDir(...)]</c> on the
    /// wrapper struct of <c>type R &lt;-chan T</c> (increment E3 follow-up 7e-b) -- or null when the type
    /// carries none. Memoized per type like <c>goTypeMarkerOf</c>, for the same reason: <c>abi.synthType</c>
    /// asks on every descriptor, and a type's attributes are an immutable fact.
    /// </summary>
    public static GoChanDir[]? TypeStampedChanDirChain(Type t)
    {
        return s_typeStampedChanDirChains.GetOrAdd(t, static type =>
            type.GetCustomAttributes(typeof(GoChanDirAttribute), false) is [GoChanDirAttribute { DirChain.Length: > 0 } stamped]
                ? stamped.DirChain
                : null);
    }

    /// <summary>
    /// The DESCRIPTOR CARRIER a struct FIELD carries as a converter stamp — the uninhabited
    /// interface that holds the Go name of a defined-over-interface type the emission erased to a
    /// <c>using</c> alias — or null when the field carries none.
    /// </summary>
    /// <remarks>
    /// Read off the <see cref="FieldInfo"/> exactly as <see cref="FieldStampedDims"/> reads
    /// <c>[GoArrayDims]</c>, and for the same reason: the datum cannot live in the managed field
    /// type, so the converter puts it in the emitted C#. Only <c>Self</c> is read — the
    /// <c>Elem</c>/<c>Key</c> slots need the carrier on the descriptor rather than at the access,
    /// which is a descriptor-shape change sequenced after this one.
    /// </remarks>
    public static Type? FieldDescriptorType(FieldInfo field)
    {
        return field.GetCustomAttributes(typeof(GoDescriptorTypeAttribute), false) is [GoDescriptorTypeAttribute { Self: { } carrier }]
            ? carrier
            : null;
    }

    /// <summary>
    /// The array dims of a map-typed STRUCT FIELD's KEY, from the converter's
    /// <c>[GoMapKeyDims]</c> stamp — what <c>reflect.Type.Key()</c> hands down — or null when the
    /// field carries none.
    /// </summary>
    public static nint[]? FieldMapKeyDims(FieldInfo field)
    {
        return field.GetCustomAttributes(typeof(GoMapKeyDimsAttribute), false) is [GoMapKeyDimsAttribute { Dims.Length: > 0 } stamped]
            ? toNintDims(stamped.Dims)
            : null;
    }

    private static nint[] toNintDims(int[] dims)
    {
        nint[] result = new nint[dims.Length];

        for (int i = 0; i < dims.Length; i++)
            result[i] = dims[i];

        return result;
    }

    // The 64-bit twin, for [GoArrayDims]. A Go array length is Go's `int`, 64-bit on a 64-bit
    // platform, and the standard library uses the full range — runtime's `*[1<<50 - 1]byte`
    // unbounded-array idiom. nint is already the destination width here, so the widening costs
    // nothing: this overload exists because the CARRIER widened, not because the target did.
    private static nint[] toNintDims(long[] dims)
    {
        nint[] result = new nint[dims.Length];

        for (int i = 0; i < dims.Length; i++)
            result[i] = (nint)dims[i];

        return result;
    }

    // -------- channel direction (abi.Type.ChanDir; rtype.String; assignability's chan arm) --------
    //
    // The same cargo shape as the array dims above, at the same finite set of positions, and for
    // the same reason: a Go channel's DIRECTION is part of its type, and `chan T`, `chan<- T` and
    // `<-chan T` all emit as one `channel<T>`. Each source below answers for the one position the
    // others cannot reach — a live value, the value behind a pointer, and a struct field's
    // initializer-borne zero. Unstamped is not a failure: it is a channel nothing narrowed, and
    // the bridge reports it as bidirectional, which is what it has always reported.

    /// <summary>
    /// The Go channel direction carried by a LIVE channel value, or
    /// <see cref="GoChanDir.Unstamped"/> when the value is not a channel or no source stamped one.
    /// </summary>
    public static GoChanDir ChanDirOfValue(object? value)
    {
        object? box = value;

        while (box is IInterfaceAdapter { Value: not null } interfaceAdapter)
            box = interfaceAdapter.Value;

        return box is IChannel channel ? channel.Direction : GoChanDir.Unstamped;
    }

    /// <summary>
    /// The Go channel direction of the value BEHIND a live pointer — <c>*chan&lt;- string</c>
    /// reports <see cref="GoChanDir.Send"/> — or <see cref="GoChanDir.Unstamped"/> when there is
    /// no such pointee.
    /// </summary>
    /// <remarks>
    /// A POINTER descriptor carries its pointee's direction unshifted, exactly as it carries the
    /// pointee's array dims (a pointer has no direction of its own), so <c>Elem()</c> hands the
    /// cargo straight down. This is the position <c>new(chan&lt;- string)</c> occupies, and the
    /// reason a NIL channel has to be able to carry a direction at all: the pointee is the zero
    /// value of a directional type, so there is no core to read and no length to measure — only
    /// the direction the converter seeded into the value.
    /// </remarks>
    public static GoChanDir PointeeChanDir(object? value)
    {
        object? box = value;

        while (box is IInterfaceAdapter { Value: not null } interfaceAdapter)
            box = interfaceAdapter.Value;

        if (box is IжAdapter { Box: not null } pointerAdapter)
            box = pointerAdapter.Box;

        if (box is null || box is INilPointer { IsNilPointer: true } ||
            !TryPointerBoxElement(box.GetType(), out Type? pointee) || KindOf(pointee) != Chan)
        {
            return GoChanDir.Unstamped;
        }

        return ChanDirOfValue(ReadPointerSlot(box));
    }

    /// <summary>
    /// The Go channel direction of a channel-typed STRUCT FIELD, recovered from a cached zero
    /// instance of the declaring struct — the converter emits the direction as a field initializer
    /// (<c>= channel&lt;@string&gt;.SendOnly</c>) that the generated parameterless constructor runs,
    /// which is the same route <see cref="FieldArrayDims"/> takes for an array field's length.
    /// </summary>
    public static GoChanDir FieldChanDir(Type declaringType, FieldInfo field)
    {
        if (!declaringType.IsValueType)
            return GoChanDir.Unstamped;

        object? zero = s_zeroInstances.GetOrAdd(declaringType, static t => Activator.CreateInstance(t));
        return zero is null ? GoChanDir.Unstamped : ChanDirOfValue(field.GetValue(zero));
    }

    /// <summary>The unified channel-value cargo of a LIVE channel value (increment D), or <c>null</c>.</summary>
    public static ChanCargo? ChanCargoOfValue(object? value)
    {
        object? box = value;

        while (box is IInterfaceAdapter { Value: not null } interfaceAdapter)
            box = interfaceAdapter.Value;

        return box is IChannel channel ? channel.Cargo : null;
    }

    /// <summary>The cargo of the channel BEHIND a live pointer, the <c>new(chan (&lt;-chan T))</c> position, or <c>null</c>.</summary>
    public static ChanCargo? PointeeChanCargo(object? value)
    {
        object? box = value;

        while (box is IInterfaceAdapter { Value: not null } interfaceAdapter)
            box = interfaceAdapter.Value;

        if (box is IжAdapter { Box: not null } pointerAdapter)
            box = pointerAdapter.Box;

        if (box is null || box is INilPointer { IsNilPointer: true } ||
            !TryPointerBoxElement(box.GetType(), out Type? pointee) || KindOf(pointee) != Chan)
        {
            return null;
        }

        return ChanCargoOfValue(ReadPointerSlot(box));
    }

    /// <summary>The cargo of a channel-typed STRUCT FIELD, off the declaring struct's cached zero instance: the <c>typeTests</c> position.</summary>
    public static ChanCargo? FieldChanCargo(Type declaringType, FieldInfo field)
    {
        if (!declaringType.IsValueType)
            return null;

        object? zero = s_zeroInstances.GetOrAdd(declaringType, static t => Activator.CreateInstance(t));
        return zero is null ? null : ChanCargoOfValue(field.GetValue(zero));
    }

    // -------- func shape (rtype.NumIn/In/NumOut/Out/IsVariadic; Value.Call) --------

    /// <summary>
    /// The Go func shape of a converted delegate type, derived from its <c>Invoke</c> signature:
    /// a <c>void</c> return is zero results, a <c>ValueTuple</c> return is Go's multi-return
    /// (a converted Go struct is never a ValueTuple, so the rule is unambiguous), anything else
    /// one result. A golib variadic family delegate (<c>Funcꓸꓸꓸ</c>/<c>Actionꓸꓸꓸ</c>, whose tail
    /// is <c>params Span&lt;T&gt;</c>) reports variadic with the tail as Go's <c>[]T</c>.
    /// </summary>
    public static bool TryFuncShape(Type delegateType, [NotNullWhen(true)] out Type[]? ins, [NotNullWhen(true)] out Type[]? outs, out bool isVariadic)
    {
        ins = null;
        outs = null;
        isVariadic = false;

        if (!typeof(Delegate).IsAssignableFrom(delegateType))
            return false;

        MethodInfo? invoke = delegateType.GetMethod("Invoke");

        if (invoke is null)
            return false;

        ParameterInfo[] parameters = invoke.GetParameters();

        ins = new Type[parameters.Length];

        for (int i = 0; i < parameters.Length; i++)
            ins[i] = parameters[i].ParameterType;

        // A trailing `Span<T>` IS the variadic tail, and testing for it is what makes this exact.
        // The golib variadic delegate families (`Funcꓸꓸꓸ`/`Actionꓸꓸꓸ`) are only ONE of the shapes a
        // converted variadic func value takes: a declared `func(string, ...int)` used as a method
        // group in an `any` position acquires C#'s NATURAL delegate type instead, whose name carries
        // no family marker at all — so the name test reported it non-variadic and `In(1)` handed back
        // a raw `Span<int>`, which then rendered as `func(string, Span\`1)`. A `Span<T>` parameter
        // cannot arise any other way in converted code (Go has no such type and the converter emits
        // one only for a variadic tail), so the shape test subsumes the name test rather than
        // widening it.
        bool spanTail = ins.Length > 0 && ins[^1] is { IsGenericType: true } tail &&
                        tail.GetGenericTypeDefinition() == typeof(Span<>);

        string name = delegateType.Name;
        isVariadic = spanTail ||
                     name.StartsWith("Func" + EllipsisOperator, StringComparison.Ordinal) ||
                     name.StartsWith("Action" + EllipsisOperator, StringComparison.Ordinal);

        if (spanTail)
            ins[^1] = typeof(slice<>).MakeGenericType(ins[^1].GetGenericArguments()[0]);

        Type ret = invoke.ReturnType;

        if (ret == typeof(void))
            outs = Type.EmptyTypes;
        else if (IsValueTuple(ret))
            outs = FlattenValueTuple(ret);
        else
            outs = [ret];

        return true;
    }

    /// <summary>
    /// The delegate type for a Go func signature — <see cref="TryFuncShape"/>'s INVERSE, and what
    /// <c>reflect.FuncOf</c> constructs.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Composed to be exactly what <see cref="TryFuncShape"/> reads back, so a type built here
    /// round-trips through <c>NumIn</c>/<c>In</c>/<c>NumOut</c>/<c>Out</c>/<c>IsVariadic</c>
    /// unchanged. That is the whole contract, and it is why the two live together: parameters become
    /// the delegate's <c>Invoke</c> parameters; a variadic tail becomes the trailing
    /// <c>Span&lt;T&gt;</c> that the read side tests for (never a <c>slice&lt;T&gt;</c>, which it
    /// would report non-variadic); and the results become <c>void</c>, the single type, or a
    /// ValueTuple that nests past seven exactly as <c>FlattenValueTuple</c> unnests it.
    /// </para>
    /// <para>
    /// Go's own <c>FuncOf</c> builds a funcType record by reinterpreting a prototype func value's
    /// memory. There is no such record here — a Go func value IS a managed delegate — so the type is
    /// composed rather than reconstructed, the same substitution ChanOf and MapOf make.
    /// </para>
    /// </remarks>
    public static Type MakeGoFuncType(Type[] ins, Type[] outs, bool isVariadic)
    {
        Type[] parameters = (Type[])ins.Clone();

        if (isVariadic)
        {
            if (parameters.Length == 0)
                throw new ArgumentException("a variadic Go func needs at least one parameter", nameof(isVariadic));

            Type tail = parameters[^1];

            Type? elem = tail.IsGenericType && tail.GetGenericTypeDefinition() == typeof(slice<>)
                ? tail.GetGenericArguments()[0]
                : ElementType(tail);

            if (elem is null)
                throw new ArgumentException($"variadic tail '{tail.Name}' is not a slice", nameof(ins));

            parameters[^1] = typeof(Span<>).MakeGenericType(elem);
        }

        return MakeDelegateType(parameters, makeGoResultType(outs));
    }

    // Go's result list as ONE managed return type: none is void, one is itself, and several are the
    // ValueTuple the converter already returns from a multi-result func.
    private static Type makeGoResultType(Type[] outs)
    {
        switch (outs.Length)
        {
            case 0:
                return typeof(void);
            case 1:
                return outs[0];
        }

        if (outs.Length <= 7)
            return valueTupleDefinition(outs.Length).MakeGenericType(outs);

        // Past seven the eighth argument is TRest and holds the remainder — the chain
        // FlattenValueTuple walks, built here in the same shape.
        return valueTupleDefinition(8).MakeGenericType([.. outs[..7], makeGoResultType(outs[7..])]);
    }

    private static Type valueTupleDefinition(int arity)
    {
        return arity switch
        {
            2 => typeof(ValueTuple<,>),
            3 => typeof(ValueTuple<,,>),
            4 => typeof(ValueTuple<,,,>),
            5 => typeof(ValueTuple<,,,,>),
            6 => typeof(ValueTuple<,,,,,>),
            7 => typeof(ValueTuple<,,,,,,>),
            8 => typeof(ValueTuple<,,,,,,,>),
            _ => throw new ArgumentOutOfRangeException(nameof(arity))
        };
    }

    /// <summary>Whether <paramref name="type"/> is a <c>System.ValueTuple</c> instantiation.</summary>
    private static bool IsValueTuple(Type type)
    {
        return type.IsGenericType && type.FullName?.StartsWith("System.ValueTuple`", StringComparison.Ordinal) == true;
    }

    /// <summary>
    /// The Go RESULT list a multi-return delegate's tuple carries, flattened through any nesting.
    /// </summary>
    /// <remarks>
    /// A ValueTuple holds at most SEVEN values inline; an eighth generic argument is TRest, itself a
    /// ValueTuple carrying the remainder, and the nesting repeats. <c>GetGenericArguments</c> alone
    /// therefore answers the tuple's SHAPE, not Go's result list: for a nine-result func it returns
    /// eight entries whose last is <c>ValueTuple&lt;T8, T9&gt;</c> — one short, and with a type in the
    /// final slot that is not a Go result at all.
    ///
    /// That is a wrong ANSWER rather than a refusal, which is what made it costly: reflect's
    /// <c>NumOut</c>/<c>Out</c> read this list, and MakeFunc compares <c>len(results)</c> against it,
    /// so a Go func returning nine values met "reflect: wrong return count from function created by
    /// MakeFunc" — an error naming the caller's own return statement for a miscount this made.
    /// Measured on reflect's TestReflectMakeFuncCallABI, the suite's largest mismatch family.
    ///
    /// Walking TRest is the whole fix, and it is the exact mirror of the packing side in
    /// reflect/makefunc_impl.cs — one side reads the shape, the other builds it, and they have to
    /// agree about where the seam is.
    /// </remarks>
    private static Type[] FlattenValueTuple(Type tuple)
    {
        // Count first, then fill: two cheap walks over a chain that is at most a few links long,
        // and no collection type this file does not already import.
        int count = 0;
        Type level = tuple;

        while (true)
        {
            Type[] elements = level.GetGenericArguments();

            // Eight arguments means the last is TRest — but only when it is itself a tuple. A
            // legitimate eight-element shape whose final argument is an ordinary type is not a nest
            // and must be taken whole.
            if (elements.Length == 8 && IsValueTuple(elements[7]))
            {
                count += 7;
                level = elements[7];
                continue;
            }

            count += elements.Length;
            break;
        }

        Type[] flattened = new Type[count];
        int next = 0;
        level = tuple;

        while (true)
        {
            Type[] elements = level.GetGenericArguments();

            if (elements.Length == 8 && IsValueTuple(elements[7]))
            {
                for (int i = 0; i < 7; i++)
                    flattened[next++] = elements[i];

                level = elements[7];
                continue;
            }

            for (int i = 0; i < elements.Length; i++)
                flattened[next++] = elements[i];

            return flattened;
        }
    }

    // -------- variadic invocation (Value.Call over a `params Span<T>` tail) --------

    /// <summary>
    /// The most fixed parameters a variadic func value can carry ahead of its tail — golib's
    /// <c>Actionꓸꓸꓸ</c>/<c>Funcꓸꓸꓸ</c> families stop there, mirroring the BCL Action/Func arities.
    /// </summary>
    public const int MaxVariadicFixedParameters = 8;

    // The two families, indexed BY FIXED-PARAMETER COUNT: entry `n` takes n fixed parameters plus
    // the tail (and, for Func, the result). See golib variadic.cs for the declarations.
    private static readonly Type[] s_variadicActionFamily =
    [
        typeof(Actionꓸꓸꓸ<>), typeof(Actionꓸꓸꓸ<,>), typeof(Actionꓸꓸꓸ<,,>), typeof(Actionꓸꓸꓸ<,,,>),
        typeof(Actionꓸꓸꓸ<,,,,>), typeof(Actionꓸꓸꓸ<,,,,,>), typeof(Actionꓸꓸꓸ<,,,,,,>),
        typeof(Actionꓸꓸꓸ<,,,,,,,>), typeof(Actionꓸꓸꓸ<,,,,,,,,>)
    ];

    private static readonly Type[] s_variadicFuncFamily =
    [
        typeof(Funcꓸꓸꓸ<,>), typeof(Funcꓸꓸꓸ<,,>), typeof(Funcꓸꓸꓸ<,,,>), typeof(Funcꓸꓸꓸ<,,,,>),
        typeof(Funcꓸꓸꓸ<,,,,,>), typeof(Funcꓸꓸꓸ<,,,,,,>), typeof(Funcꓸꓸꓸ<,,,,,,,>),
        typeof(Funcꓸꓸꓸ<,,,,,,,,>), typeof(Funcꓸꓸꓸ<,,,,,,,,,>)
    ];

    // One per delegate TYPE: the family type its instances rebind onto, and the closed trampoline
    // that performs the typed call. Both are functions of the type alone, so they cache together.
    private sealed record VariadicInvoker(Type FamilyType, Func<Delegate, object?[], Array, object?> Call);

    private static readonly ConcurrentDictionary<Type, VariadicInvoker> s_variadicInvokers = new();

    /// <summary>
    /// Calls a converted Go VARIADIC func value whose tail is already packed into
    /// <paramref name="tail"/> (a <c>TArg[]</c> of the tail's element type), and returns the raw
    /// delegate result — <c>null</c> for a no-result func, a <c>ValueTuple</c> for a Go
    /// multi-return, exactly the shape the non-variadic path receives.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This exists because NO reflective invoke path can call one. A converted Go variadic lowers
    /// its tail to <c>params Span&lt;TArg&gt;</c> (see golib variadic.cs), <c>Span&lt;T&gt;</c> is a
    /// ref struct, and both <see cref="Delegate.DynamicInvoke"/> and <see cref="MethodBase.Invoke"/>
    /// marshal their arguments through <c>object?[]</c> — which a ref struct cannot enter. Nor can
    /// an expression tree stand in: <c>System.Linq.Expressions</c> rejects a byref-like type
    /// outright, so the sibling method-value binder's <c>Expression.Lambda</c> approach
    /// (GoReflect.MethodSets.cs) does not generalize here.
    /// </para>
    /// <para>
    /// So the call is made in TYPED code — one small generic trampoline per family arity, closed
    /// over the delegate's own parameter types by <c>MakeGenericMethod</c> and cached as an ordinary
    /// delegate (the <c>elementBoxViaAt</c> idiom in GoReflect.FieldAccess.cs). Inside a trampoline
    /// the tail is a <c>TArg[]</c> and its conversion to <c>Span&lt;TArg&gt;</c> is an ordinary
    /// one, so nothing is ever boxed.
    /// </para>
    /// <para>
    /// The trampoline casts to the golib FAMILY delegate, and the value being called is not always
    /// one: a variadic func literal in an <c>any</c> slot, or a declared variadic used as a method
    /// group there, acquires C#'s NATURAL delegate type instead — the same shape difference
    /// <see cref="TryFuncShape"/> had to stop reading off the type NAME. Those rebind onto the
    /// family, which is exact rather than a best-effort match: the family's type arguments are
    /// constructed FROM this delegate's own <c>Invoke</c> signature, so the two agree by
    /// construction and only the nominal type differs. A delegate that already IS its family type
    /// skips the rebind.
    /// </para>
    /// <para>
    /// The rebind RETARGETS through <c>Invoke</c> — the family delegate closes over the original
    /// delegate as its receiver — rather than re-binding the original's own target and method. That
    /// is what makes it total. A delegate the BRIDGE ITSELF built is expression-compiled
    /// (<c>Value.Method</c> binds a receiver that way, and a variadic method value lands here), and
    /// a compiled lambda's <c>Method</c> is not a runtime <see cref="MethodInfo"/>, which
    /// <see cref="Delegate.CreateDelegate(Type, object, MethodInfo)"/> rejects outright
    /// ("MethodInfo must be a runtime MethodInfo object"). <c>Invoke</c> is always a real method on
    /// a real delegate type, so this form has no such blind spot — and it carries a multicast
    /// invocation list intact, which re-binding a single target and method silently would not.
    /// </para>
    /// <para>
    /// A direct typed call also means a panic inside the callee propagates natively, rather than
    /// arriving wrapped in a <see cref="TargetInvocationException"/> the caller must unwrap.
    /// </para>
    /// </remarks>
    public static object? InvokeVariadic(Delegate del, object?[] fixedArgs, Array tail)
    {
        Type delegateType = del.GetType();
        VariadicInvoker invoker = s_variadicInvokers.GetOrAdd(delegateType, static t => buildVariadicInvoker(t));
        Delegate bound = delegateType == invoker.FamilyType ? del : rebindToVariadicFamily(del, invoker.FamilyType);

        return invoker.Call(bound, fixedArgs, tail);
    }

    private static VariadicInvoker buildVariadicInvoker(Type delegateType)
    {
        MethodInfo? invoke = delegateType.GetMethod("Invoke");
        ParameterInfo[] parameters = invoke?.GetParameters() ?? [];

        if (invoke is null || parameters.Length == 0 ||
            parameters[^1].ParameterType is not { IsGenericType: true } tailParameter ||
            tailParameter.GetGenericTypeDefinition() != typeof(Span<>))
        {
            throw new NotImplementedException(
                $"reflect: '{GoTypeName(delegateType)}' is not a variadic func value (its Invoke has no Span<T> tail)");
        }

        int fixedCount = parameters.Length - 1;
        bool hasResult = invoke.ReturnType != typeof(void);

        if (fixedCount > MaxVariadicFixedParameters)
        {
            throw new NotImplementedException(
                $"reflect: calling a variadic func value with {fixedCount} fixed parameters is not implemented — golib's " +
                $"Actionꓸꓸꓸ/Funcꓸꓸꓸ families stop at {MaxVariadicFixedParameters} (no demonstrated consumer beyond that)");
        }

        // <T1..Tn, TArg[, TResult]> — the family's type arguments ARE this delegate's own parameter
        // types, which is what makes the rebind above exact rather than a signature search.
        Type[] typeArguments = new Type[fixedCount + (hasResult ? 2 : 1)];

        for (int i = 0; i < fixedCount; i++)
            typeArguments[i] = parameters[i].ParameterType;

        typeArguments[fixedCount] = tailParameter.GetGenericArguments()[0];

        if (hasResult)
            typeArguments[^1] = invoke.ReturnType;

        Type familyType = (hasResult ? s_variadicFuncFamily : s_variadicActionFamily)[fixedCount].MakeGenericType(typeArguments);

        MethodInfo trampoline = typeof(GoReflect)
            .GetMethod($"{(hasResult ? "callVariadicFunc" : "callVariadicAction")}{fixedCount}", BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(typeArguments);

        return new VariadicInvoker(familyType, trampoline.CreateDelegate<Func<Delegate, object?[], Array, object?>>());
    }

    private static Delegate rebindToVariadicFamily(Delegate del, Type familyType)
    {
        try
        {
            return Delegate.CreateDelegate(familyType, del, "Invoke");
        }
        catch (Exception ex) when (ex is ArgumentException or MethodAccessException or MissingMethodException)
        {
            throw new NotImplementedException(
                $"reflect: the variadic func value '{GoTypeName(del.GetType())}' could not be rebound onto golib's " +
                $"'{familyType.Name}' family delegate", ex);
        }
    }

    // The eighteen trampolines. Each makes the same three moves — cast to the family type, unbox the
    // fixed arguments, hand the tail array over as a Span — and differs only in arity, so they are
    // written out rather than generated: the closed set IS golib's variadic family (variadic.cs).

    private static object? callVariadicAction0<TArg>(Delegate d, object?[] a, Array t)
    { ((Actionꓸꓸꓸ<TArg>)d)(new Span<TArg>((TArg[])t)); return null; }

    private static object? callVariadicAction1<T1, TArg>(Delegate d, object?[] a, Array t)
    { ((Actionꓸꓸꓸ<T1, TArg>)d)((T1)a[0]!, new Span<TArg>((TArg[])t)); return null; }

    private static object? callVariadicAction2<T1, T2, TArg>(Delegate d, object?[] a, Array t)
    { ((Actionꓸꓸꓸ<T1, T2, TArg>)d)((T1)a[0]!, (T2)a[1]!, new Span<TArg>((TArg[])t)); return null; }

    private static object? callVariadicAction3<T1, T2, T3, TArg>(Delegate d, object?[] a, Array t)
    { ((Actionꓸꓸꓸ<T1, T2, T3, TArg>)d)((T1)a[0]!, (T2)a[1]!, (T3)a[2]!, new Span<TArg>((TArg[])t)); return null; }

    private static object? callVariadicAction4<T1, T2, T3, T4, TArg>(Delegate d, object?[] a, Array t)
    { ((Actionꓸꓸꓸ<T1, T2, T3, T4, TArg>)d)((T1)a[0]!, (T2)a[1]!, (T3)a[2]!, (T4)a[3]!, new Span<TArg>((TArg[])t)); return null; }

    private static object? callVariadicAction5<T1, T2, T3, T4, T5, TArg>(Delegate d, object?[] a, Array t)
    { ((Actionꓸꓸꓸ<T1, T2, T3, T4, T5, TArg>)d)((T1)a[0]!, (T2)a[1]!, (T3)a[2]!, (T4)a[3]!, (T5)a[4]!, new Span<TArg>((TArg[])t)); return null; }

    private static object? callVariadicAction6<T1, T2, T3, T4, T5, T6, TArg>(Delegate d, object?[] a, Array t)
    { ((Actionꓸꓸꓸ<T1, T2, T3, T4, T5, T6, TArg>)d)((T1)a[0]!, (T2)a[1]!, (T3)a[2]!, (T4)a[3]!, (T5)a[4]!, (T6)a[5]!, new Span<TArg>((TArg[])t)); return null; }

    private static object? callVariadicAction7<T1, T2, T3, T4, T5, T6, T7, TArg>(Delegate d, object?[] a, Array t)
    { ((Actionꓸꓸꓸ<T1, T2, T3, T4, T5, T6, T7, TArg>)d)((T1)a[0]!, (T2)a[1]!, (T3)a[2]!, (T4)a[3]!, (T5)a[4]!, (T6)a[5]!, (T7)a[6]!, new Span<TArg>((TArg[])t)); return null; }

    private static object? callVariadicAction8<T1, T2, T3, T4, T5, T6, T7, T8, TArg>(Delegate d, object?[] a, Array t)
    { ((Actionꓸꓸꓸ<T1, T2, T3, T4, T5, T6, T7, T8, TArg>)d)((T1)a[0]!, (T2)a[1]!, (T3)a[2]!, (T4)a[3]!, (T5)a[4]!, (T6)a[5]!, (T7)a[6]!, (T8)a[7]!, new Span<TArg>((TArg[])t)); return null; }

    private static object? callVariadicFunc0<TArg, TResult>(Delegate d, object?[] a, Array t)
    { return ((Funcꓸꓸꓸ<TArg, TResult>)d)(new Span<TArg>((TArg[])t)); }

    private static object? callVariadicFunc1<T1, TArg, TResult>(Delegate d, object?[] a, Array t)
    { return ((Funcꓸꓸꓸ<T1, TArg, TResult>)d)((T1)a[0]!, new Span<TArg>((TArg[])t)); }

    private static object? callVariadicFunc2<T1, T2, TArg, TResult>(Delegate d, object?[] a, Array t)
    { return ((Funcꓸꓸꓸ<T1, T2, TArg, TResult>)d)((T1)a[0]!, (T2)a[1]!, new Span<TArg>((TArg[])t)); }

    private static object? callVariadicFunc3<T1, T2, T3, TArg, TResult>(Delegate d, object?[] a, Array t)
    { return ((Funcꓸꓸꓸ<T1, T2, T3, TArg, TResult>)d)((T1)a[0]!, (T2)a[1]!, (T3)a[2]!, new Span<TArg>((TArg[])t)); }

    private static object? callVariadicFunc4<T1, T2, T3, T4, TArg, TResult>(Delegate d, object?[] a, Array t)
    { return ((Funcꓸꓸꓸ<T1, T2, T3, T4, TArg, TResult>)d)((T1)a[0]!, (T2)a[1]!, (T3)a[2]!, (T4)a[3]!, new Span<TArg>((TArg[])t)); }

    private static object? callVariadicFunc5<T1, T2, T3, T4, T5, TArg, TResult>(Delegate d, object?[] a, Array t)
    { return ((Funcꓸꓸꓸ<T1, T2, T3, T4, T5, TArg, TResult>)d)((T1)a[0]!, (T2)a[1]!, (T3)a[2]!, (T4)a[3]!, (T5)a[4]!, new Span<TArg>((TArg[])t)); }

    private static object? callVariadicFunc6<T1, T2, T3, T4, T5, T6, TArg, TResult>(Delegate d, object?[] a, Array t)
    { return ((Funcꓸꓸꓸ<T1, T2, T3, T4, T5, T6, TArg, TResult>)d)((T1)a[0]!, (T2)a[1]!, (T3)a[2]!, (T4)a[3]!, (T5)a[4]!, (T6)a[5]!, new Span<TArg>((TArg[])t)); }

    private static object? callVariadicFunc7<T1, T2, T3, T4, T5, T6, T7, TArg, TResult>(Delegate d, object?[] a, Array t)
    { return ((Funcꓸꓸꓸ<T1, T2, T3, T4, T5, T6, T7, TArg, TResult>)d)((T1)a[0]!, (T2)a[1]!, (T3)a[2]!, (T4)a[3]!, (T5)a[4]!, (T6)a[5]!, (T7)a[6]!, new Span<TArg>((TArg[])t)); }

    private static object? callVariadicFunc8<T1, T2, T3, T4, T5, T6, T7, T8, TArg, TResult>(Delegate d, object?[] a, Array t)
    { return ((Funcꓸꓸꓸ<T1, T2, T3, T4, T5, T6, T7, T8, TArg, TResult>)d)((T1)a[0]!, (T2)a[1]!, (T3)a[2]!, (T4)a[3]!, (T5)a[4]!, (T6)a[5]!, (T7)a[6]!, (T8)a[7]!, new Span<TArg>((TArg[])t)); }

    /// <summary>
    /// The per-parameter Go array dimensions of a converted func VALUE — one entry per parameter,
    /// null where that parameter is not a fixed-size array — or null when nothing is carried.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The dimension of a `[32]byte` parameter cannot be read from the delegate TYPE (see the file
    /// header), so the converter stamps it on the parameter as <see cref="GoArrayDimsAttribute"/>
    /// and this reads it back off the delegate's target method. <c>Delegate.Method</c> resolves to
    /// the real declaration for every shape go2cs emits — a declared func used as a method group, a
    /// non-capturing lambda, a capturing lambda's display-class method, and a natural-typed lambda —
    /// so one read covers them all.
    /// </para>
    /// <para>
    /// The arity guard is what keeps it honest. A delegate whose target method's parameter list does
    /// not line up one-for-one with <c>Invoke</c>'s — an OPEN instance delegate carries the receiver
    /// as an extra leading parameter, and the bridge's own method values are expression-compiled
    /// closures with no attributes at all — is answered <c>null</c> rather than mis-indexed. That is
    /// the r39d rule in its usual form: a descriptor field that cannot be read truthfully stays
    /// unpopulated, and a dims-less array descriptor is a state the bridge already handles.
    /// </para>
    /// </remarks>
    public static nint[]?[]? FuncParamDims(object? funcValue)
    {
        if (funcValue is not Delegate d || d.Method is not { } method)
            return null;

        ParameterInfo[] declared = method.GetParameters();

        if (declared.Length == 0 || d.GetType().GetMethod("Invoke") is not { } invoke ||
            invoke.GetParameters().Length != declared.Length)
        {
            return null;
        }

        return paramDims(declared);
    }

    /// <summary>
    /// The per-parameter Go array dimensions of the <paramref name="index"/>'th method of
    /// <paramref name="t"/>'s method set — the same cargo <see cref="FuncParamDims"/> reads for a
    /// func value, for the func type <c>reflect.Type.Method(i).Type</c> reports.
    /// </summary>
    /// <remarks>
    /// A method needs its own reader because its func type is built from the method TABLE
    /// (<see cref="GoMethodFuncType"/> over the <c>MethodInfo</c>'s parameters) and never passes
    /// through a delegate instance, so the delegate route above has nothing to read. No arity guard
    /// is owed here for the same reason: the delegate type is synthesized FROM this parameter list,
    /// receiver included, so the indices line up with <c>In(i)</c> by construction — which is Go's
    /// own shape for a method type, receiver first.
    /// </remarks>
    public static nint[]?[]? MethodParamDims(Type? t, int index)
    {
        return paramDims(MethodAt(t, index).Method.GetParameters());
    }

    private static nint[]?[]? paramDims(ParameterInfo[] declared)
    {
        nint[]?[]? dims = null;

        for (int i = 0; i < declared.Length; i++)
        {
            if (declared[i].GetCustomAttributes(typeof(GoArrayDimsAttribute), false) is not [GoArrayDimsAttribute { Dims.Length: > 0 } stamped])
                continue;

            dims ??= new nint[]?[declared.Length];
            dims[i] = toNintDims(stamped.Dims);
        }

        return dims;
    }
}
