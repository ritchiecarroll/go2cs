// pinner_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// go2cs HAND-OWNED companion — runtime.Pinner over the CLR heap (Q45).
// Design and prediction on record: docs/phase4/DESIGN-runtime-pinner.md.
//
// WHAT THIS FILE OWNS. The five bodies manualConversionFuncs["runtime"] displaces for the Pinner
// seam — Pinner.Pin, Pinner.Unpin, isPinned, pinnerGetPinCounter and cgoCheckPointer — plus the
// pin table they share. Everything the converted bodies reached underneath (setPinned, unpin,
// pinnerGetPtr, cgoCheckArg, cgoCheckUnknownPointer, cgoIsGoPointer, the span's pinnerBits and
// the specialPinCounter records) stays converted in pinner.cs / cgocall.cs and is DEAD behind
// these five: it walked mheap_.arenas, which the managed host never allocates, so isPinned
// nil-dereferenced in spanOf on its first read — and cgoCheckPointer never got that far, because
// its gate `debug.cgocheck == 0` is the zero value: Go sets the default of 1 in parsedebugvars,
// on the schedinit path this host never runs (the same silently-unreached init as internal/cpu).
//
// WHAT A PIN MEANS HERE. Go's Pinner makes one promise with two observables. The promise — "not
// moved or freed until Unpin" — exists so an address can be handed to non-GC-aware code, and the
// ADDRESS half is already unconditional in golib: an address is only ever minted by the ж<T>
// uintptr/void* conversions, which pin the storage for the box's whole life (EnsureStableAddress),
// and a reachable box is never freed. That is why the previous hand-own was a no-op — and it was
// right about the half no test measures. The two observables Go's suite DOES measure are:
//
//   1. the pin BIT — isPinned, read by the cgo argument check. Go keeps two bits per object in
//      the span's pinnerBits, keyed by OBJECT index: pinning &sl[0] pins the whole backing array,
//      so isPinned(&sl[1]) and isPinned(slice.array) both read true. Here the bit is a COUNT in a
//      ConditionalWeakTable keyed by the pointer's ReferentObject — a standard box is its own
//      referent, an element reference's referent is its canonical backing, a field reference's is
//      its source allocation — which is Go's object index one level down. TestPinnerCgoCheckSlice
//      (pin &sl[0], check &sl) and TestPinnerInterface (pin the interface CELL, assert its pointee
//      is NOT pinned) both depend on exactly that keying.
//   2. the lifetime HOLD — a pinned object stays alive until Unpin. Go's pinner.refs holds
//      unsafe.Pointers, which are GC roots; here the pinner holds the referent objects themselves.
//
// The table is weak (a CWT is never the reason an allocation stays alive — the token record's
// LIFETIME rule, applied here); the HOLD lives on the Pinner, exactly as Go's refs do. No CLR pin
// is taken by Pin: the address-take already pins for the box's life and the Pinner's hold keeps
// that box — and so its pin — alive for the Go-visible pin's duration; a second GCHandle per Pin
// would double the measured per-pin cost for no observable. And the pin does NOT ride on
// ManagedPointerTokens: that record is weak (a pin is a strong hold), keyed per PROJECTION (a
// pin is per ALLOCATION), and its validate-on-read answers "is this number the CLR-pinned address
// of this box" — a fact about the address-take, not about a Pinner; TestPinnerSimple takes
// unsafe.Pointer(p) (which registers) BEFORE asserting !IsPinned, and would read `already marked
// as pinned` if the two were conflated. The record is CONSUMED read-only, through Resolve, for a
// bare number (§ pinnerReferentOf below); nothing here writes into it.
//
// THE CGO CHECK. Go's cgoCheckPointer(ptr, arg) resolves to one rule: every Go pointer WORD found
// at level 1 must be pinned, every Go pointer word found at level 2 must be pinned, and level 3 is
// not inspected. Level 1 is the argument's pointee (with arg == true the element's fields; with
// arg == nil the pointee walked as an unknown object; with a slice or array arg, that container
// instead of the pointer); level 2 is, for each level-1 pointer, the pointer words of ITS pointee
// (cgoCheckUnknownPointer — the object's words, no recursion). The managed walk below is over the
// pointee's VALUE by its GoType structure: checkArg is Go's cgoCheckArg, checkWords is Go's
// cgoCheckUnknownPointer. Two stated divergences, both in the RODATA direction: a string LITERAL's
// bytes are a heap byte[] here (Go: RODATA, never a Go pointer), and a boxed scalar inside an
// interface is a heap copy here (Go: RODATA for small integers) — both read "unpinned Go pointer"
// where Go reads "not a Go pointer". Neither is reached by runtime's own suite; the literal case
// is the TestPinnerConstStringData disclosure's subject (design §6.1).
//
// ONE CONVERTER SHAPE TOLERATED, ROUTED TO Q49 (C2's bridge class). internal/fmtsort's test init
// pins `reflect.ValueOf(cs[i]).UnsafePointer()`, and the converter emits that unsafe.Pointer-typed
// call result wrapped in `(uintptr)` on its way into Pin's `any` parameter — the retained source
// dropped, the dynamic type now uintptr. A faithful kind check would panic `argument is not a
// pointer: uintptr` and turn a banked row red. Pin therefore accepts a uintptr as the projected
// form of a pointer: it resolves the number through ManagedPointerTokens and pins the referent
// when one answers, and no-ops when none does (a bare number IS a non-Go pointer under Go's own
// rule). When the Q49 fix retires the `(uintptr)` wrap, the `case uintptr` arm in
// pinnerReferentOf is one deleted arm and this paragraph is its reason.
//
// GODEBUG. The check is ON by default (Go's cgocheck=1) and off only under GODEBUG=cgocheck=0,
// read once here rather than through the converted `debug` struct that parsedebugvars would have
// filled: a module initializer writing one field of a struct whose other readers are init-path-
// only would be a second policy for one variable. The `cgocheck > 1` mode Go 1.23 rejects at
// startup is not reproduced.
//
// TEST SEAMS. runtime's own suite reaches isPinned / pinnerGetPinCounter / cgoCheckPointer /
// pinnerLeakPanic through export_test.go (the internal-test assembly, which the csproj already
// grants InternalsVisibleTo). GolibTests is not in that grant, so the Go-prefixed PUBLIC helpers
// at the end of the file expose the same five operations for the guard — the "one public helper
// per operation, native mirrors private" pattern the syscall seams use.
[module: go.GoManualConversion]

namespace go;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using @unsafe = unsafe_package;

partial class runtime_package {

// The pinner's own state — Go's `refs`, one level down: the REFERENT objects this Pinner holds
// strongly until Unpin. A partial part on the converted struct (the runtime2_impl.cs precedent)
// rather than a side table keyed by the box, because the leak finalizer reads this AFTER the box
// has become unreachable and been resurrected for the call: a side table's dependent handle would
// have let the state go the moment the key did, and the finalizer would then count zero pins on a
// pinner that leaked. A field rides with the box through resurrection; a table entry does not.
partial struct pinner {
    internal List<object>? pinnedReferents;
}

// Pin pins a Go object, preventing it from being moved or freed by the garbage
// collector until the [Pinner.Unpin] method has been called.
//
// The argument must be a pointer of any type or an [unsafe.Pointer].
// It's safe to call Pin on non-Go pointers, in which case Pin will do nothing.
[GoRecv] public static void Pin(this ref Pinner Δp, any pointer)
{
    if (Δp.pinner == nil)
    {
        // Go's per-P pinnerCache is not reproduced (there is no P); a fresh pinner per Pinner is
        // the cost the design states. The leak finalizer is set ONCE per pinner, exactly as Go
        // does, through the hand-owned SetFinalizer bridge: the sentinel hands the finalizer its
        // target — this box, resurrected — on the finalizer runner thread. A static method group,
        // so the delegate captures nothing that could keep the box alive.
        ж<pinner> box = @new<pinner>();
        box.Value.pinnedReferents = new List<object>();
        Δp.pinner = box;
        SetFinalizer(box, (Action<ж<pinner>>)pinnerFinalizer);
    }

    object? referent = pinnerReferentOf(pointer);

    // Go's setPinned: a non-Go pointer (nil, a global, RODATA, a native address) is silently
    // ignored and never enters refs.
    if (referent is null)
        return;

    PinTable.Pin(referent);
    Δp.pinner.Value.pinnedReferents!.Add(referent);
}

// Unpin unpins all pinned objects of the [Pinner].
[GoRecv] public static void Unpin(this ref Pinner Δp)
{
    // Go's unpin: `if p == nil || p.refs == nil { return }` — a Pinner that never pinned, or
    // one already unpinned, is a no-op (TestPinnerEmptyUnpin). The pinner box is kept, as Go
    // keeps the backing store when application code reuses a Pinner.
    if (Δp.pinner == nil)
        return;

    unpinManaged(Δp.pinner);
}

private static void unpinManaged(ж<pinner> box)
{
    List<object>? refs = box.Value.pinnedReferents;

    if (refs is null || refs.Count == 0)
        return;

    foreach (object referent in refs)
        PinTable.Unpin(referent);

    refs.Clear();
}

// The finalizer Go sets on every pinner: a pinner that dies with pins outstanding is a leak, and
// the (test-swappable) pinnerLeakPanic variable is what reports it. The unpin first is Go's own
// line — "only required to make the test idempotent".
private static void pinnerFinalizer(ж<pinner> i)
{
    if (i.Value.pinnedReferents is { Count: > 0 })
    {
        unpinManaged(i);
        pinnerLeakPanic();
    }
}

// pinnerGetPtr, over managed values: the object a Pin argument names, or null when the argument
// is a nil pointer or not a Go pointer at all. Panics exactly where Go's pinnerGetPtr does.
private static object? pinnerReferentOf(any pointer)
{
    switch (pointer)
    {
        case null:
        case NilType:
            throw panic((errorString)(@string)"runtime.Pinner: argument is nil"u8);

        // Before the INilPointer arm: an unsafe.Pointer IS a box (a StandardBox<uintptr>), but its
        // referent is what it was minted from, never itself.
        case @unsafe.Pointer up:
            return goReferentOfUnsafePointer(up);

        case INilPointer box:
            return goReferentOfBox(box);

        // The Q49 accommodation (file header): a projected pointer arriving as its number.
        case uintptr number:
            return goReferentOfNumber(number.Value);

        default:
            throw panic((errorString)((@string)"runtime.Pinner: argument is not a pointer: "u8 + (@string)GetGoTypeName(pointer)));
    }
}

// The Go allocation a managed box names — null for nil and for a native alias (Go: "not a Go
// pointer"). A standard box is its own referent, an element reference's is its canonical backing,
// a field reference's is its source allocation: INilPointer.ReferentObject, which is also the key
// SetFinalizer registers under, so the two mechanisms agree on what "the object" is.
private static object? goReferentOfBox(INilPointer box)
{
    if (box.IsNilPointer)
        return null;

    if (isNativeAlias(box))
        return null;

    return box.ReferentObject;
}

// NativeBox<T> is the one box kind that aliases a native address (the only override of
// ж<T>.NativeAddress in golib); a generic-definition test answers without naming T and without
// the boxed-nuint allocation a reflection read of the property would cost on every pin and every
// check (measured: 24 B per repeat pin before this was a type test).
private static bool isNativeAlias(object box)
{
    Type type = box.GetType();
    return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(NativeBox<>);
}

// The retained source first (a Pointer minted from a box carries it — FromPinnedBox / FromBox),
// else the provenance/token record for the bare number, validate-on-read protected. A number
// nothing resolves is a native or dead address: not a Go pointer.
private static object? goReferentOfUnsafePointer(@unsafe.Pointer up)
{
    if (up.RetainedSource is { } source)
        return source is INilPointer box ? goReferentOfBox(box) : source;

    if (up.IsNilPointer)
        return null;

    return goReferentOfNumber(up.Value.Value);
}

private static object? goReferentOfNumber(nuint number)
{
    if (number == 0)
        return null;

    object? resolved = ManagedPointerTokens.Resolve(number);

    if (resolved is null)
        return null;

    return resolved is INilPointer box ? goReferentOfBox(box) : resolved;
}

// isPinned checks if a Go pointer is pinned. Go answers TRUE for any pointer outside a heap span
// (nil, a linker-allocated global, RODATA) — "this code is only called for Go pointer, so this
// must be a linker-allocated global object" — and that arm is the non-Go-pointer arm here.
internal static bool isPinned(@unsafe.Pointer ptr)
{
    object? referent = goReferentOfUnsafePointer(ptr);
    return referent is null || PinTable.IsPinned(referent);
}

// only for tests — the ADDITIONAL pins on the object (Go's specialPinCounter), nil when the object
// is pinned once or not at all. A fresh snapshot box: Go hands back a pointer into the special
// record, and nothing writes through it.
internal static ж<uintptr> pinnerGetPinCounter(@unsafe.Pointer addr)
{
    object? referent = goReferentOfUnsafePointer(addr);
    int count = referent is null ? 0 : PinTable.Count(referent);

    if (count <= 1)
        return default!;

    return new StandardBox<uintptr>((uintptr)(count - 1));
}

// ---- the pin table: Go's pinnerBits, keyed by the allocation rather than the span index ----

private static class PinTable
{
    private sealed class PinRecord
    {
        public int Count;
    }

    // Weak keys: the table is never the reason an allocation stays alive. The HOLD is the
    // Pinner's own list (Go's refs). Reads take no lock (CWT reads are thread-safe); count
    // transitions take one, where Go takes span.speciallock.
    private static readonly ConditionalWeakTable<object, PinRecord> s_records = new();
    private static readonly object s_lock = new();

    public static void Pin(object referent)
    {
        lock (s_lock)
        {
            if (s_records.TryGetValue(referent, out PinRecord? record))
                record.Count++;
            else
                s_records.Add(referent, new PinRecord { Count = 1 });
        }
    }

    public static void Unpin(object referent)
    {
        lock (s_lock)
        {
            if (!s_records.TryGetValue(referent, out PinRecord? record))
                @throw("runtime.Pinner: object already unpinned"u8);

            if (--record.Count == 0)
                s_records.Remove(referent);
        }
    }

    public static bool IsPinned(object referent)
    {
        return s_records.TryGetValue(referent, out _);
    }

    public static int Count(object referent)
    {
        return s_records.TryGetValue(referent, out PinRecord? record) ? record.Count : 0;
    }
}

// ---- the cgo argument check over managed values ----

private static readonly bool s_cgoCheckEnabled = readCgoCheckSetting();

private static bool readCgoCheckSetting()
{
    string? godebug = Environment.GetEnvironmentVariable("GODEBUG");

    if (string.IsNullOrEmpty(godebug))
        return true;

    bool enabled = true;

    // Last setting wins, as Go's parser reads the list.
    foreach (string setting in godebug.Split(','))
    {
        string trimmed = setting.Trim();

        if (trimmed.StartsWith("cgocheck=", StringComparison.Ordinal))
            enabled = trimmed.Substring("cgocheck=".Length) != "0";
    }

    return enabled;
}

// cgoCheckPointer checks if the argument contains a Go pointer that
// points to an unpinned Go pointer, and panics if it does.
internal static void cgoCheckPointer(any ptr, any arg)
{
    if (!s_cgoCheckEnabled)
        return;

    if (ptr is null || ptr is NilType)
        return;

    if (arg is not null && arg is not NilType && (ptr is INilPointer || ptr is @unsafe.Pointer))
    {
        object? referent = ptr is @unsafe.Pointer up ? goReferentOfUnsafePointer(up) : goReferentOfBox((INilPointer)ptr);

        // Go: `if p == nil || !cgoIsGoPointer(p) { return }`.
        if (referent is null)
            return;

        switch (arg)
        {
            case bool:
                // Go: an unsafe.Pointer argument has no element type ("We don't know the type of
                // the element"), so it breaks out to the whole-object walk below.
                if (ptr is @unsafe.Pointer)
                    break;

                // Go: cgoCheckArg(pt.Elem, p, indir: true, top: false) — the POINTEE, below top.
                if (ptr is IUntypedSlotAccess slot && slot.TryLoadThrough(out object? pointee))
                    checkArg(pointee, pointeeTypeOf(ptr), top: false);

                return;

            case ISlice:
                // "Check the slice rather than the pointer."
                checkArg(arg, arg.GetType(), top: true);
                return;

            case IArray:
                // "Check the array rather than the pointer. Pass top as false since we have a
                // pointer to the array."
                checkArg(arg, arg.GetType(), top: false);
                return;

            default:
                @throw("can't happen"u8);
                return;
        }
    }

    checkArg(ptr, ptr?.GetType() ?? typeof(object), top: true);
}

private static PanicException cgoCheckFailure()
{
    return panic((errorString)cgoCheckPointerFail);
}

// Go's cgoCheckArg: p is a value of static type t; top is whether we are at the top level, where
// Go pointers are allowed unpinned. A Go pointer to a pinned object is allowed as long as it does
// not reference other unpinned pointers (checkWords).
private static void checkArg(object? value, Type staticType, bool top)
{
    value = unwrapAdapter(value);

    if (value is null || value is NilType)
        return;

    // An interface slot (Go's abi.Interface arm): the DATA WORD must be pinned below top level, and
    // the dynamic value is then walked below top level whatever it is — a pointer walks as a
    // pointer, a struct copy walks its fields.
    if (isInterfaceType(staticType))
    {
        object? word = interfaceDataReferent(value);

        if (word is null)
            return;

        if (!top && !PinTable.IsPinned(word))
            throw cgoCheckFailure();

        checkArg(value, value.GetType(), top: false);
        return;
    }

    switch (value)
    {
        case @unsafe.Pointer up:
            checkPointer(goReferentOfUnsafePointer(up), top);
            return;

        case INilPointer box:
            checkPointer(goReferentOfBox(box), top);
            return;

        case @string str:
        {
            object? data = stringBacking(str);

            if (data is not null && !top && !PinTable.IsPinned(data))
                throw cgoCheckFailure();

            return;
        }

        case ISlice slice:
        {
            object? backing = sliceBacking(slice);

            if (backing is null)
                return;

            if (!top && !PinTable.IsPinned(backing))
                throw cgoCheckFailure();

            Type elementType = elementTypeOf(slice.GetType());

            if (!hasPointerWords(elementType))
                return;

            // Go walks the slice's CAP, not its len.
            foreach (object? element in sliceElementsOverCap(slice))
                checkArg(element, elementType, top: false);

            return;
        }

        case IChannel:
        case IMap:
            // "These types contain internal pointers that will always be allocated in the Go
            // heap. It's never OK to pass them to C." A nil channel or map is a nil word.
            if (!isDefaultStruct(value))
                throw cgoCheckFailure();

            return;

        case Delegate function:
            // A closure is a heap funcval (a Go pointer); a plain function's funcval is RODATA.
            if (function.Target is not null)
                throw cgoCheckFailure();

            return;

        case IArray array:
        {
            Type elementType = elementTypeOf(array.GetType());

            if (!hasPointerWords(elementType))
                return;

            for (nint i = 0; i < array.Length; i++)
                checkArg(array[i], elementType, top);

            return;
        }

        default:
        {
            Type type = value.GetType();

            if (!type.IsValueType || !hasPointerWords(type))
                return;

            foreach (FieldInfo field in pointerBearingFields(type))
                checkArg(field.GetValue(value), field.FieldType, top);

            return;
        }
    }
}

// The abi.Pointer / abi.UnsafePointer arm: below top level the pointer itself must be pinned;
// then its pointee's words are the next (and last) level.
private static void checkPointer(object? referent, bool top)
{
    if (referent is null)
        return;

    if (!top && !PinTable.IsPinned(referent))
        throw cgoCheckFailure();

    checkWords(referent);
}

// Go's cgoCheckUnknownPointer: every Go pointer WORD inside the object must be pinned — the
// object's words only, no descent through them.
private static void checkWords(object referent)
{
    switch (referent)
    {
        case Array backing:
        {
            // An element reference's referent is its whole backing array: Go's findObject answers
            // the allocation, and the walk covers every word of it.
            Type elementType = backing.GetType().GetElementType()!;

            if (!hasPointerWords(elementType))
                return;

            foreach (object? element in backing)
                checkWordsOfValue(element, elementType);

            return;
        }

        case INilPointer box:
            if (box is IUntypedSlotAccess slot && slot.TryLoadThrough(out object? pointee))
                checkWordsOfValue(pointee, pointeeTypeOf(box));

            return;

        default:
            // A referent that is neither a box nor an array (a channel's inner object recovered
            // through a token) has no Go-typed words to walk.
            return;
    }
}

private static void requirePinned(object? referent)
{
    if (referent is not null && !PinTable.IsPinned(referent))
        throw cgoCheckFailure();
}

// The pointer words of one value, INLINE: nested structs and fixed arrays are part of the same
// object and are walked; a pointer, a slice's backing, a string's bytes, an interface's data word
// and a channel/map/closure are words, and a word is checked, never followed.
private static void checkWordsOfValue(object? value, Type staticType)
{
    value = unwrapAdapter(value);

    if (value is null || value is NilType)
        return;

    if (isInterfaceType(staticType))
    {
        requirePinned(interfaceDataReferent(value));
        return;
    }

    switch (value)
    {
        case @unsafe.Pointer up:
            requirePinned(goReferentOfUnsafePointer(up));
            return;

        case INilPointer box:
            requirePinned(goReferentOfBox(box));
            return;

        case @string str:
            requirePinned(stringBacking(str));
            return;

        case ISlice slice:
            requirePinned(sliceBacking(slice));
            return;

        case IChannel:
        case IMap:
            if (!isDefaultStruct(value))
                throw cgoCheckFailure();

            return;

        case Delegate function:
            if (function.Target is not null)
                throw cgoCheckFailure();

            return;

        case IArray array:
        {
            Type elementType = elementTypeOf(array.GetType());

            if (!hasPointerWords(elementType))
                return;

            for (nint i = 0; i < array.Length; i++)
                checkWordsOfValue(array[i], elementType);

            return;
        }

        default:
        {
            Type type = value.GetType();

            if (!type.IsValueType || !hasPointerWords(type))
                return;

            foreach (FieldInfo field in pointerBearingFields(type))
                checkWordsOfValue(field.GetValue(value), field.FieldType);

            return;
        }
    }
}

// ---- type and value helpers for the walk ----

// A generated interface adapter stands in for the pointer or value it wraps.
private static object? unwrapAdapter(object? value)
{
    return value switch
    {
        IжAdapter adapter => adapter.Box,
        IValueAdapter adapter => adapter.Value,
        IInterfaceAdapter adapter => adapter.Value,
        _ => value,
    };
}

private static bool isInterfaceType(Type type)
{
    return type == typeof(object) || type.IsInterface;
}

// The object an interface's data word names: a pointer's referent, else the boxed value itself
// (Go: the heap copy an interface holds for a non-pointer dynamic type).
private static object? interfaceDataReferent(object value)
{
    return value switch
    {
        @unsafe.Pointer up => goReferentOfUnsafePointer(up),
        INilPointer box => goReferentOfBox(box),
        _ => value,
    };
}

private static object? stringBacking(@string str)
{
    if (str.Length == 0)
        return null;

    // StringData is an element reference into the string's own backing (unsafe.cs), and its
    // referent is that backing array — the object Go's findObject would answer for &s[0].
    return goReferentOfBox(@unsafe.StringData(str));
}

private static readonly ConcurrentDictionary<Type, (FieldInfo? array, PropertyInfo? nativeBacked)> s_sliceAccessors = new();

private static (FieldInfo? array, PropertyInfo? nativeBacked) sliceAccessorsOf(Type type)
{
    return s_sliceAccessors.GetOrAdd(type, static t => (
        t.GetField("m_array", BindingFlags.Instance | BindingFlags.NonPublic),
        t.GetProperty("IsNativeBacked", BindingFlags.Instance | BindingFlags.NonPublic)));
}

// The slice's backing array — null for a nil slice and for a native-backed one (not a Go pointer).
private static object? sliceBacking(ISlice slice)
{
    (FieldInfo? array, PropertyInfo? nativeBacked) = sliceAccessorsOf(slice.GetType());

    if (nativeBacked?.GetValue(slice) is true)
        return null;

    return array?.GetValue(slice);
}

private static IEnumerable<object?> sliceElementsOverCap(ISlice slice)
{
    (FieldInfo? arrayField, _) = sliceAccessorsOf(slice.GetType());

    if (arrayField?.GetValue(slice) is not Array backing)
        yield break;

    nint end = slice.Low + slice.Capacity;

    for (nint i = slice.Low; i < end && i < backing.Length; i++)
        yield return backing.GetValue(i);
}

private static Type elementTypeOf(Type containerType)
{
    for (Type? t = containerType; t is not null; t = t.BaseType)
    {
        if (t.IsGenericType && t.GetGenericArguments().Length == 1 &&
            (t.GetGenericTypeDefinition() == typeof(slice<>) || t.GetGenericTypeDefinition() == typeof(array<>)))
        {
            return t.GetGenericArguments()[0];
        }
    }

    return typeof(object);
}

private static Type pointeeTypeOf(object box)
{
    for (Type? t = box.GetType(); t is not null; t = t.BaseType)
    {
        if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(ж<>))
            return t.GetGenericArguments()[0];
    }

    return typeof(object);
}

private static bool isDefaultStruct(object value)
{
    Type type = value.GetType();

    if (!type.IsValueType)
        return false;

    // map<K,V> answers nil-ness directly; a channel<T> is nil when it is the default struct.
    if (type.GetProperty("IsNil", BindingFlags.Instance | BindingFlags.Public) is { } isNil && isNil.PropertyType == typeof(bool))
        return (bool)isNil.GetValue(value)!;

    return value.Equals(Activator.CreateInstance(type));
}

private static readonly ConcurrentDictionary<Type, bool> s_pointerWords = new();
private static readonly ConcurrentDictionary<Type, FieldInfo[]> s_pointerFields = new();

// Go's t.Pointers(): does a value of this type contain any pointer word? Decided over the managed
// shape of the emitted types — a box, an unsafe.Pointer, a slice, a string, a channel, a map, a
// delegate and an interface slot are words; a fixed array or struct carries its elements' or
// fields' answer; every other reference type is conservatively a word.
private static bool hasPointerWords(Type type)
{
    if (s_pointerWords.TryGetValue(type, out bool known))
        return known;

    // Register a provisional answer first so a self-referential struct terminates.
    s_pointerWords[type] = false;
    bool answer = computeHasPointerWords(type);
    s_pointerWords[type] = answer;
    return answer;
}

private static bool computeHasPointerWords(Type type)
{
    if (type.IsPrimitive || type.IsEnum || type.IsPointer)
        return false;

    if (type == typeof(uintptr))
        return false;

    if (isInterfaceType(type) || typeof(INilPointer).IsAssignableFrom(type) || typeof(Delegate).IsAssignableFrom(type))
        return true;

    if (type == typeof(@string) || typeof(ISlice).IsAssignableFrom(type) || typeof(IChannel).IsAssignableFrom(type) || typeof(IMap).IsAssignableFrom(type))
        return true;

    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(array<>))
        return hasPointerWords(type.GetGenericArguments()[0]);

    if (!type.IsValueType)
        return true;

    foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
    {
        if (hasPointerWords(field.FieldType))
            return true;
    }

    return false;
}

private static FieldInfo[] pointerBearingFields(Type type)
{
    return s_pointerFields.GetOrAdd(type, static t =>
    {
        List<FieldInfo> fields = new();

        foreach (FieldInfo field in t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            if (hasPointerWords(field.FieldType))
                fields.Add(field);
        }

        return fields.ToArray();
    });
}

// ---- public test seams (GolibTests; runtime's own suite reaches the internals via export_test) ----

public static bool GoIsPinned(@unsafe.Pointer pointer) => isPinned(pointer);

public static ж<uintptr> GoPinCounter(@unsafe.Pointer pointer) => pinnerGetPinCounter(pointer);

public static void GoCgoCheckPointer(any ptr, any arg) => cgoCheckPointer(ptr, arg);

public static Action GoPinnerLeakPanic
{
    get => pinnerLeakPanic;
    set => pinnerLeakPanic = value;
}

} // end runtime_package
