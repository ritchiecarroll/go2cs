// arena_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// User arenas (arena.go) over managed allocations.
//
// Go carves an arena's objects out of 8 MiB chunks it takes from its own heap, keeps every chunk
// alive through the arena's refs until free, and after free hands the chunks back and makes their
// addresses FAULT. There is no Go heap here (mallocinit never runs, so the converted body reached
// fixalloc before FixAlloc_Init and threw), but everything the arena API promises except the fault is
// expressible over the CLR's own collector:
//
//   - an allocation is an ordinary managed object, made through the same reflect bridge reflect.New
//     and reflect.MakeSlice use (GoReflect.NewPointerBox / MakeContainer, off the descriptor's
//     System.Type), so the collector traces everything it references;
//   - the arena keeps a strong list of what it handed out, which is Go's "an arena's objects live
//     until the arena is freed": an object reachable only from arena memory stays alive while the
//     arena does (TestUserArenaLiveness);
//   - free drops that list, so each object then lives exactly as long as something else refers to
//     it, and the collector reclaims the rest;
//   - "is this in an arena" (arena_heapify, behind arena.Clone) is a membership test on the
//     allocation's REFERENT object -- the box for new, the backing array for slice -- recorded in a
//     ConditionalWeakTable, so membership never keeps an allocation alive. A string built over arena
//     bytes with unsafe.String aliases that backing array, and so does a sub-slice, so both resolve
//     to the arena exactly as their addresses do in Go.
//
// An allocation larger than userArenaChunkMaxAllocBytes goes to the heap and is not the arena's,
// as in Go (userArenaNextFree's redirect), so Clone returns it unchanged.
//
// THE ONE REPRESENTATIONAL DIFFERENCE: using an arena object after free stays valid memory. Go makes
// the chunk fault (setUserArenaChunkToFault), so a use-after-free crashes; here the object is an
// ordinary managed object and a stale reference to it keeps it alive. No test in runtime's
// arena_test.go asserts the fault (the assertions that do live in package arena's tests, which build
// only under GOEXPERIMENT=arenas).
//
// Go's newUserArena also sets a finalizer that frees an arena dropped without free. It is not set:
// its only effect is to return the chunks, and an unreachable arena's list is unreachable with it.
//
// The converter drops the auto forms of newUserArena, userArena.new, userArena.slice, userArena.free
// and arena_heapify (manualConversionFuncs["runtime"]), leaving a placeholder comment at each site.
// The rest of arena.go (chunk refill, heap bits, the fault list) stays converted and unreachable.
//
// Hand-owned: there is no arena_impl.go, so a -stdlib reconvert never regenerates this file.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
// The plain namespace using brings internal/runtime/atomic's [GoRecv] extension methods (Bool.Load
// and Store) into scope; an alias alone does not participate in extension lookup.
using go.@internal.runtime;
using abi = go.@internal.abi_package;
using @unsafe = go.unsafe_package;

[module: go.GoManualConversion]

namespace go;

partial class runtime_package
{
    partial struct userArena
    {
        // Every allocation this arena handed out: the role Go's refs (the chunk references) play.
        // Null once the arena is freed.
        [GoReflectCompanion] internal List<object>? allocations;
    }

    // The referent object of every live arena allocation. The value is unused; a weak key is the
    // point, so a membership entry dies with its allocation.
    private static readonly ConditionalWeakTable<object, object> s_userArenaReferents = new();

    private static readonly object s_userArenaMember = new();

    // A slice's referent is its backing storage, reached through unsafe.SliceData as Go reaches it
    // (&s[0]); closed per element type once, as reflect's bridge closes its box makers.
    private static readonly ConcurrentDictionary<System.Type, Func<object, object?>> s_sliceReferents = new();

    // newUserArena creates a new userArena ready to be used.
    internal static ж<userArena> newUserArena()
    {
        ж<userArena> a = Ꮡ(new userArena());
        a.Value.allocations = [];
        return a;
    }

    // new allocates a new object of the provided type into the arena, and returns its pointer.
    [GoRecv] internal static @unsafe.Pointer @new(this ref userArena a, ж<_type> Ꮡtyp) =>
        new(userArenaNew(ref a, Ꮡtyp));

    // userArenaNew is new's managed form: the allocation itself, a pointer box whose static type is
    // *T, for a caller (UserArena.New) that stores it into an interface.
    internal static INilPointer userArenaNew(ref userArena a, ж<_type> Ꮡtyp)
    {
        System.Type elem = userArenaManagedType(Ꮡtyp);
        object box = GoReflect.NewPointerBox(elem, GoReflect.ZeroValueOf(elem, Ꮡtyp.Value.arrayDims));

        userArenaKeep(ref a, box, box, Ꮡtyp.Value.Size_);
        return (INilPointer)box;
    }

    // slice allocates a new slice backing store. slice must be a pointer to a slice (i.e. *[]T),
    // because userArenaSlice will update the slice directly.
    //
    // cap determines the capacity of the slice backing store and must be non-negative.
    [GoRecv] internal static void Δslice(this ref userArena a, any sl, nint cap)
    {
        if (cap < 0) {
            throw panic("userArena.slice: negative cap");
        }

        ж<_type> typ = abi.TypeOf(sl);

        if (typ == nil || (abiꓸKind)((~typ).Kind_ & abi.KindMask) != abi.Pointer) {
            throw panic("slice result of non-ptr type");
        }

        typ = abi.Elem(typ);

        if ((abiꓸKind)((~typ).Kind_ & abi.KindMask) != abi.Slice) {
            throw panic("slice of non-ptr-to-slice type");
        }

        // t is now the slice type; its element type sizes the allocation.
        ж<_type> et = abi.Elem(typ);
        object slice = GoReflect.MakeContainer(userArenaManagedType(typ), cap, cap);

        // A zero-size element allocates nothing (Go hands back zerobase), and neither does cap 0.
        if (cap > 0 && (~et).Size_ != 0) {
            userArenaKeep(ref a, slice, userArenaSliceReferent(slice), (~et).Size_ * (uintptr)cap);
        }

        if (sl is not IUntypedSlotAccess slot || !slot.TryStoreThrough(slice)) {
            throw panic("slice of non-ptr-to-slice type");
        }
    }

    // free returns the userArena's allocations to the collector and marks it as defunct.
    //
    // Must be called at most once for any given arena.
    internal static void free(this ж<userArena> Ꮡa)
    {
        // Check for a double-free.
        if (Ꮡa.of(userArena.Ꮡdefunct).Load()) {
            throw panic("arena double free");
        }

        // Mark ourselves as defunct.
        Ꮡa.of(userArena.Ꮡdefunct).Store(true);

        // Dropping the list is Go's freeing of the chunks: nothing the arena allocated is kept alive
        // by the arena any more.
        Ꮡa.Value.allocations = null;
    }

    // arena_heapify takes a value that lives in an arena and makes a copy of it on the heap.
    // Values that don't live in an arena are returned unmodified.
    internal static any arena_heapify(any s)
    {
        ж<_type> t = abi.TypeOf(s);
        abiꓸKind kind = t == nil ? abi.Invalid : (abiꓸKind)((~t).Kind_ & abi.KindMask);

        if (kind == abi.ΔString) {
            if (s is not @string str || str.Length == 0 || !userArenaIsMember(((INilPointer)@unsafe.StringData(str)).ReferentObject)) {
                return s;
            }

            return new @string(str.ToSpan());
        }

        if (kind == abi.Slice) {
            if (userArenaSliceReferent(s) is not { } backing || !userArenaIsMember(backing)) {
                return s;
            }

            IArray source = (IArray)s;
            IArray copy = (IArray)GoReflect.MakeContainer(s.GetType(), source.Length, source.Length);

            for (nint i = 0; i < source.Length; i++) {
                copy[i] = userArenaValueCopy(source[i]);
            }

            return copy;
        }

        if (kind == abi.Pointer) {
            if (s is not INilPointer pointer || pointer.IsNilPointer || !userArenaIsMember(pointer.ReferentObject)) {
                return s;
            }

            ((IUntypedSlotAccess)pointer).TryLoadThrough(out object? value);
            return GoReflect.NewPointerBox(userArenaManagedType(abi.Elem(t)), userArenaValueCopy(value));
        }

        throw panic("arena: Clone only supports pointers, slices, and strings");
    }

    // userArenaKeep records one allocation: kept alive by the arena's list, and a member for Clone.
    // Over userArenaChunkMaxAllocBytes Go allocates from the heap instead, so neither applies.
    private static void userArenaKeep(ref userArena a, object allocation, object? referent, uintptr size)
    {
        if (size > userArenaChunkMaxAllocBytes) {
            return;
        }

        List<object> allocations = a.allocations ?? throw panic("arena: allocation from a freed arena");
        allocations.Add(allocation);

        if (referent is not null) {
            s_userArenaReferents.AddOrUpdate(referent, s_userArenaMember);
        }
    }

    private static bool userArenaIsMember(object referent) =>
        s_userArenaReferents.TryGetValue(referent, out _);

    // The System.Type a synthesized descriptor stands for. Every descriptor the arena sees comes from
    // an interface value's dynamic type (abi.TypeOf), which is always synthesized.
    private static System.Type userArenaManagedType(ж<_type> Ꮡtyp) =>
        Ꮡtyp.Value.sysType ?? throw panic("arena: type descriptor carries no managed type");

    // A Go value copy (typedmemmove) of one allocation's value: an array, or a struct carrying
    // arrays, copies its storage; everything else is copied by the boxed value's own copy.
    private static object? userArenaValueCopy(object? value) =>
        value is IGoValueClone || value is IArray and not ISlice ? ((ICloneable)value).Clone() : value;

    // The backing storage of a slice value (or of a named slice type's underlying slice), or null for
    // one with no storage.
    private static object? userArenaSliceReferent(object value)
    {
        if (GoReflect.TryUnwrapWrapperValue(value, out object? inner)) {
            value = inner;
        }

        System.Type type = value.GetType();

        if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(slice<>)) {
            return null;
        }

        return s_sliceReferents.GetOrAdd(type.GetGenericArguments()[0], static elem =>
            typeof(runtime_package).GetMethod(nameof(userArenaSliceReferentOf), BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(elem).CreateDelegate<Func<object, object?>>())(value);
    }

    private static object? userArenaSliceReferentOf<T>(object value)
    {
        slice<T> s = (slice<T>)value;
        return cap(s) == 0 ? null : ((INilPointer)@unsafe.SliceData(s)).ReferentObject;
    }

    // GolibTests' probes (RuntimeUserArenaTests): the arena's surface, reached without runtime's
    // internal types.

    /// <summary>A new user arena, as runtime's NewUserArena export makes one.</summary>
    public static object GoUserArenaNew() => newUserArena();

    /// <summary>A zeroed <c>*T</c> allocated in <paramref name="arena"/>, as UserArena.New makes one.</summary>
    public static object GoUserArenaAlloc(object arena, System.Type elem) =>
        userArenaNew(ref ((ж<userArena>)arena).Value, abi.synthType(elem));

    /// <summary>Allocates a slice of capacity <paramref name="cap"/> into the <c>*[]T</c> <paramref name="slicePointer"/>.</summary>
    public static void GoUserArenaSlice(object arena, object slicePointer, nint cap) =>
        ((ж<userArena>)arena).Value.Δslice(slicePointer, cap);

    /// <summary>Frees <paramref name="arena"/>.</summary>
    public static void GoUserArenaFree(object arena) => ((ж<userArena>)arena).free();

    /// <summary>arena.Clone: a heap copy of an arena value, or the value itself.</summary>
    public static object? GoUserArenaClone(object? value) => arena_heapify(value);
}
