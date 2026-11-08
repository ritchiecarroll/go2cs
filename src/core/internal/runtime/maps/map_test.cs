// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.runtime;

using fmt = fmt_package;
using abi = go.@internal.abi_package;
using maps = go.@internal.runtime.maps_package;
using math = go.math_package;
using testing = testing_package;
using @unsafe = unsafe_package;
using go;
using go.@internal;
using go.@internal.runtime;
using static go.@internal.runtime.maps_internal_test_package;

partial class maps_test_package {

public static partial void TestCtrlSize(ж<testing.T> t) {
    var cs = /* unsafe.Sizeof(maps.CtrlGroup(0)) */ (uintptr)8;
    if (cs != abi.MapGroupSlots) {
        t.Errorf("ctrlGroup size got %d want abi.MapGroupSlots %d", cs, (nint)(abi.MapGroupSlots));
    }
}

public static void TestMapPut(ж<testing.T> t) {
    var (m, typ) = maps_internal_test_package.NewTestMap<uint32, uint64>(8);

    ref var key = ref heap<uint32>((uint32)0, out var Ꮡkey);
    ref var elem = ref heap<uint64>((uint64)(256 + 0), out var Ꮡelem);

    for (nint i = 0; i < 31; i++) {
        key += 1;
        elem += 1;
        m.Put(typ, @unsafe.Pointer.FromPinnedBox(Ꮡkey), @unsafe.Pointer.FromPinnedBox(Ꮡelem));

        if (maps_internal_test_package.DebugLog) {
            fmt.Printf("After put %d: %v\n"u8, key, m.OrTypedNil());
        }
    }

    if (m.Used() != 31) {
        t.Errorf("Used() used got %d want 31", m.Used());
    }

    key = (uint32)0;
    elem = (uint64)(256 + 0);

    for (nint i = 0; i < 31; i++) {
        key += 1;
        elem += 1;
        var (got, ok) = m.Get(typ, @unsafe.Pointer.FromPinnedBox(Ꮡkey));
        if (!ok) {
            t.Errorf("Get(%d) got ok false want true", key);
        }
        var gotElem = ~(ж<uint64>)(uintptr)(got);
        if (gotElem != elem) {
            t.Errorf("Get(%d) got elem %d want %d", key, gotElem, elem);
        }
    }
}

// Grow enough to cause a table split.
public static void TestMapSplit(ж<testing.T> t) {
    var (m, typ) = maps_internal_test_package.NewTestMap<uint32, uint64>(0);

    ref var key = ref heap<uint32>((uint32)0, out var Ꮡkey);
    ref var elem = ref heap<uint64>((uint64)(256 + 0), out var Ꮡelem);

    for (nint i = 0; i < (nint)(2 * maps_internal_test_package.MaxTableCapacity); i++) {
        key += 1;
        elem += 1;
        m.Put(typ, @unsafe.Pointer.FromPinnedBox(Ꮡkey), @unsafe.Pointer.FromPinnedBox(Ꮡelem));

        if (maps_internal_test_package.DebugLog) {
            fmt.Printf("After put %d: %v\n"u8, key, m.OrTypedNil());
        }
    }

    if (m.Used() != (uint64)(2 * maps_internal_test_package.MaxTableCapacity)) {
        t.Errorf("Used() used got %d want 31", m.Used());
    }

    key = (uint32)0;
    elem = (uint64)(256 + 0);

    for (nint i = 0; i < (nint)(2 * maps_internal_test_package.MaxTableCapacity); i++) {
        key += 1;
        elem += 1;
        var (got, ok) = m.Get(typ, @unsafe.Pointer.FromPinnedBox(Ꮡkey));
        if (!ok) {
            t.Errorf("Get(%d) got ok false want true", key);
        }
        var gotElem = ~(ж<uint64>)(uintptr)(got);
        if (gotElem != elem) {
            t.Errorf("Get(%d) got elem %d want %d", key, gotElem, elem);
        }
    }
}

public static void TestMapDelete(ж<testing.T> t) {
    var (m, typ) = maps_internal_test_package.NewTestMap<uint32, uint64>(32);

    ref var key = ref heap<uint32>((uint32)0, out var Ꮡkey);
    ref var elem = ref heap<uint64>((uint64)(256 + 0), out var Ꮡelem);

    for (nint i = 0; i < 31; i++) {
        key += 1;
        elem += 1;
        m.Put(typ, @unsafe.Pointer.FromPinnedBox(Ꮡkey), @unsafe.Pointer.FromPinnedBox(Ꮡelem));

        if (maps_internal_test_package.DebugLog) {
            fmt.Printf("After put %d: %v\n"u8, key, m.OrTypedNil());
        }
    }

    key = (uint32)0;
    elem = (uint64)(256 + 0);

    for (nint i = 0; i < 31; i++) {
        key += 1;
        m.Delete(typ, @unsafe.Pointer.FromPinnedBox(Ꮡkey));
    }

    if (m.Used() != 0) {
        t.Errorf("Used() used got %d want 0", m.Used());
    }

    key = (uint32)0;
    elem = (uint64)(256 + 0);

    for (nint i = 0; i < 31; i++) {
        key += 1;
        elem += 1;
        var (_, ok) = m.Get(typ, @unsafe.Pointer.FromPinnedBox(Ꮡkey));
        if (ok) {
            t.Errorf("Get(%d) got ok true want false", key);
        }
    }
}

public static void TestTableClear(ж<testing.T> t) {
    var (m, typ) = maps_internal_test_package.NewTestMap<uint32, uint64>(32);

    ref var key = ref heap<uint32>((uint32)0, out var Ꮡkey);
    ref var elem = ref heap<uint64>((uint64)(256 + 0), out var Ꮡelem);

    for (nint i = 0; i < 31; i++) {
        key += 1;
        elem += 1;
        m.Put(typ, @unsafe.Pointer.FromPinnedBox(Ꮡkey), @unsafe.Pointer.FromPinnedBox(Ꮡelem));

        if (maps_internal_test_package.DebugLog) {
            fmt.Printf("After put %d: %v\n"u8, key, m.OrTypedNil());
        }
    }

    m.Clear(typ);

    if (m.Used() != 0) {
        t.Errorf("Clear() used got %d want 0", m.Used());
    }

    key = (uint32)0;
    elem = (uint64)(256 + 0);

    for (nint i = 0; i < 31; i++) {
        key += 1;
        elem += 1;
        var (_, ok) = m.Get(typ, @unsafe.Pointer.FromPinnedBox(Ꮡkey));
        if (ok) {
            t.Errorf("Get(%d) got ok true want false", key);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object itKeyGotNilWantKeyˢ = (@string)"it.Key() got nil want key"u8;

// +0.0 and -0.0 compare equal, but we must still must update the key slot when
// overwriting.
public static void TestTableKeyUpdate(ж<testing.T> t) {
    var (m, typ) = maps_internal_test_package.NewTestMap<float64, uint64>(8);

    ref var zero = ref heap<float64>((float64)0.0D, out var Ꮡzero);
    ref var negZero = ref heap<float64>(math.Copysign(zero, -1.0D), out var ᏑnegZero);
    ref var elem = ref heap<uint64>((uint64)0, out var Ꮡelem);

    m.Put(typ, @unsafe.Pointer.FromPinnedBox(Ꮡzero), @unsafe.Pointer.FromPinnedBox(Ꮡelem));
    if (maps_internal_test_package.DebugLog) {
        fmt.Printf("After put %f: %v\n"u8, zero, m.OrTypedNil());
    }

    elem = 1;
    m.Put(typ, @unsafe.Pointer.FromPinnedBox(ᏑnegZero), @unsafe.Pointer.FromPinnedBox(Ꮡelem));
    if (maps_internal_test_package.DebugLog) {
        fmt.Printf("After put %f: %v\n"u8, negZero, m.OrTypedNil());
    }

    if (m.Used() != 1) {
        t.Errorf("Used() used got %d want 1", m.Used());
    }

    var it = @new<maps.Iter>();
    it.Init(typ, m);
    it.Next();
    @unsafe.Pointer keyPtr = (uintptr)it.Key();
    @unsafe.Pointer elemPtr = (uintptr)it.Elem();
    if (keyPtr == nil) {
        t.Fatal(itKeyGotNilWantKeyˢ);
    }

    var key = ~(ж<float64>)(uintptr)(keyPtr);
    elem = ~(ж<uint64>)(uintptr)(elemPtr);
    if (math.Copysign(1.0D, key) > 0D) {
        t.Errorf("map key %f has positive sign", key);
    }
    if (elem != 1) {
        t.Errorf("map elem got %d want 1", elem);
    }
}

// Put should reuse a deleted slot rather than consuming an empty slot.
public static void TestTablePutDelete(ж<testing.T> t) {
    // Put will reuse the first deleted slot it encounters.
    //
    // This is awkward to test because Delete will only install ctrlDeleted
    // if the group is full, otherwise it goes straight to empty.
    //
    // So first we must add to the table continuously until we happen to
    // fill a group.

    // Avoid small maps, they have no tables.
    var (m, typ) = maps_internal_test_package.NewTestMap<uint32, uint32>(16);

    ref var key = ref heap<uint32>((uint32)0, out var Ꮡkey);
    ref var elem = ref heap<uint32>((uint32)(256 + 0), out var Ꮡelem);

    for (;;) {
        key += 1;
        elem += 1;

        m.Put(typ, @unsafe.Pointer.FromPinnedBox(Ꮡkey), @unsafe.Pointer.FromPinnedBox(Ꮡelem));

        // Normally a Put that fills a group would fill it with the
        // inserted key, so why search the whole map for a potentially
        // different key in a full group?
        //
        // Put may grow/split a table. Initial construction of the new
        // table(s) could result in a full group consisting of
        // arbitrary keys.
        @unsafe.Pointer fullKeyPtr = (uintptr)m.KeyFromFullGroup(typ);
        if (fullKeyPtr != nil) {
            // Found a full group.
            key = ~(ж<uint32>)(uintptr)(fullKeyPtr);
            elem = 256 + key;
            break;
        }
    }

    // Key is in a full group. Deleting it will result in a ctrlDeleted
    // slot.
    m.Delete(typ, @unsafe.Pointer.FromPinnedBox(Ꮡkey));

    // Re-insert key. This should reuse the deleted slot rather than
    // consuming space.
    var tabWant = m.TableFor(typ, @unsafe.Pointer.FromPinnedBox(Ꮡkey));
    var growthLeftWant = tabWant.GrowthLeft();

    m.Put(typ, @unsafe.Pointer.FromPinnedBox(Ꮡkey), @unsafe.Pointer.FromPinnedBox(Ꮡelem));

    var tabGot = m.TableFor(typ, @unsafe.Pointer.FromPinnedBox(Ꮡkey));
    var growthLeftGot = tabGot.GrowthLeft();

    if (tabGot != tabWant) {
        // There shouldn't be a grow, as replacing a deleted slot
        // doesn't require more space.
        t.Errorf("Put(%d) grew table got %v want %v map %v", key, tabGot.OrTypedNil(), tabWant.OrTypedNil(), m.OrTypedNil());
    }

    if (growthLeftGot != growthLeftWant) {
        t.Errorf("GrowthLeft got %d want %d: map %v tab %v", growthLeftGot, growthLeftWant, m.OrTypedNil(), tabGot.OrTypedNil());
    }
}

public static void TestTableIteration(ж<testing.T> t) {
    var (m, typ) = maps_internal_test_package.NewTestMap<uint32, uint64>(8);

    ref var key = ref heap<uint32>((uint32)0, out var Ꮡkey);
    ref var elem = ref heap<uint64>((uint64)(256 + 0), out var Ꮡelem);

    for (nint i = 0; i < 31; i++) {
        key += 1;
        elem += 1;
        m.Put(typ, @unsafe.Pointer.FromPinnedBox(Ꮡkey), @unsafe.Pointer.FromPinnedBox(Ꮡelem));

        if (maps_internal_test_package.DebugLog) {
            fmt.Printf("After put %d: %v\n"u8, key, m.OrTypedNil());
        }
    }

    var got = new map<uint32, uint64>();

    var it = @new<maps.Iter>();
    it.Init(typ, m);
    for (;;) {
        it.Next();
        @unsafe.Pointer keyPtr = (uintptr)it.Key();
        @unsafe.Pointer elemPtr = (uintptr)it.Elem();
        if (keyPtr == nil) {
            break;
        }

        var keyΔ1 = ~(ж<uint32>)(uintptr)(keyPtr);
        var elemΔ1 = ~(ж<uint64>)(uintptr)(elemPtr);
        got[keyΔ1] = elemΔ1;
    }

    if (len(got) != 31) {
        t.Errorf("Iteration got %d entries, want 31: %+v", len(got), got);
    }

    key = (uint32)0;
    elem = (uint64)(256 + 0);

    for (nint i = 0; i < 31; i++) {
        key += 1;
        elem += 1;
        var (gotElem, ok) = got[key, ꟷ];
        if (!ok) {
            t.Errorf("Iteration missing key %d", key);
            continue;
        }
        if (gotElem != elem) {
            t.Errorf("Iteration key %d got elem %d want %d", key, gotElem, elem);
        }
    }
}

// Deleted keys shouldn't be visible in iteration.
public static void TestTableIterationDelete(ж<testing.T> t) {
    var (m, typ) = maps_internal_test_package.NewTestMap<uint32, uint64>(8);

    ref var key = ref heap<uint32>((uint32)0, out var Ꮡkey);
    ref var elem = ref heap<uint64>((uint64)(256 + 0), out var Ꮡelem);

    for (nint i = 0; i < 31; i++) {
        key += 1;
        elem += 1;
        m.Put(typ, @unsafe.Pointer.FromPinnedBox(Ꮡkey), @unsafe.Pointer.FromPinnedBox(Ꮡelem));

        if (maps_internal_test_package.DebugLog) {
            fmt.Printf("After put %d: %v\n"u8, key, m.OrTypedNil());
        }
    }

    var got = new map<uint32, uint64>();
    var first = true;
    ref var deletedKey = ref heap<uint32>((uint32)1, out var ᏑdeletedKey);
    var it = @new<maps.Iter>();
    it.Init(typ, m);
    for (;;) {
        it.Next();
        @unsafe.Pointer keyPtr = (uintptr)it.Key();
        @unsafe.Pointer elemPtr = (uintptr)it.Elem();
        if (keyPtr == nil) {
            break;
        }

        var keyΔ1 = ~(ж<uint32>)(uintptr)(keyPtr);
        var elemΔ1 = ~(ж<uint64>)(uintptr)(elemPtr);
        got[keyΔ1] = elemΔ1;

        if (first) {
            first = false;

            // If the key we intended to delete was the one we just
            // saw, pick another to delete.
            if (keyΔ1 == deletedKey) {
                deletedKey++;
            }
            m.Delete(typ, @unsafe.Pointer.FromPinnedBox(ᏑdeletedKey));
        }
    }

    if (len(got) != 30) {
        t.Errorf("Iteration got %d entries, want 30: %+v", len(got), got);
    }

    key = (uint32)0;
    elem = (uint64)(256 + 0);

    for (nint i = 0; i < 31; i++) {
        key += 1;
        elem += 1;

        var wantOK = true;
        if (key == deletedKey) {
            wantOK = false;
        }

        var (gotElem, gotOK) = got[key, ꟷ];
        if (gotOK != wantOK) {
            t.Errorf("Iteration key %d got ok %v want ok %v", key, gotOK, wantOK);
            continue;
        }
        if (wantOK && gotElem != elem) {
            t.Errorf("Iteration key %d got elem %d want %d", key, gotElem, elem);
        }
    }
}

// Deleted keys shouldn't be visible in iteration even after a grow.
public static void TestTableIterationGrowDelete(ж<testing.T> t) {
    var (m, typ) = maps_internal_test_package.NewTestMap<uint32, uint64>(8);

    ref var key = ref heap<uint32>((uint32)0, out var Ꮡkey);
    ref var elem = ref heap<uint64>((uint64)(256 + 0), out var Ꮡelem);

    for (nint i = 0; i < 31; i++) {
        key += 1;
        elem += 1;
        m.Put(typ, @unsafe.Pointer.FromPinnedBox(Ꮡkey), @unsafe.Pointer.FromPinnedBox(Ꮡelem));

        if (maps_internal_test_package.DebugLog) {
            fmt.Printf("After put %d: %v\n"u8, key, m.OrTypedNil());
        }
    }

    var got = new map<uint32, uint64>();
    var first = true;
    ref var deletedKey = ref heap<uint32>((uint32)1, out var ᏑdeletedKey);
    var it = @new<maps.Iter>();
    it.Init(typ, m);
    for (;;) {
        it.Next();
        @unsafe.Pointer keyPtr = (uintptr)it.Key();
        @unsafe.Pointer elemPtr = (uintptr)it.Elem();
        if (keyPtr == nil) {
            break;
        }

        var keyΔ1 = ~(ж<uint32>)(uintptr)(keyPtr);
        var elemΔ1 = ~(ж<uint64>)(uintptr)(elemPtr);
        got[keyΔ1] = elemΔ1;

        if (first) {
            first = false;

            // If the key we intended to delete was the one we just
            // saw, pick another to delete.
            if (keyΔ1 == deletedKey) {
                deletedKey++;
            }

            // Double the number of elements to force a grow.
            ref var keyΔ2 = ref heap<uint32>((uint32)32, out var ᏑkeyΔ2);
            ref var elemΔ2 = ref heap<uint64>((uint64)(256 + 32), out var ᏑelemΔ2);

            for (nint i = 0; i < 31; i++) {
                keyΔ2 += 1;
                elemΔ2 += 1;
                m.Put(typ, @unsafe.Pointer.FromPinnedBox(ᏑkeyΔ2), @unsafe.Pointer.FromPinnedBox(ᏑelemΔ2));

                if (maps_internal_test_package.DebugLog) {
                    fmt.Printf("After put %d: %v\n"u8, keyΔ2, m.OrTypedNil());
                }
            }

            // Then delete from the grown map.
            m.Delete(typ, @unsafe.Pointer.FromPinnedBox(ᏑdeletedKey));
        }
    }

    // Don't check length: the number of new elements we'll see is
    // unspecified.

    // Check values only of the original pre-iteration entries.
    key = (uint32)0;
    elem = (uint64)(256 + 0);

    for (nint i = 0; i < 31; i++) {
        key += 1;
        elem += 1;

        var wantOK = true;
        if (key == deletedKey) {
            wantOK = false;
        }

        var (gotElem, gotOK) = got[key, ꟷ];
        if (gotOK != wantOK) {
            t.Errorf("Iteration key %d got ok %v want ok %v", key, gotOK, wantOK);
            continue;
        }
        if (wantOK && gotElem != elem) {
            t.Errorf("Iteration key %d got elem %d want %d", key, gotElem, elem);
        }
    }
}

internal static void testTableIterationGrowDuplicate(ж<testing.T> t, nint grow) {
    var (m, typ) = maps_internal_test_package.NewTestMap<uint32, uint64>(8);

    ref var key = ref heap<uint32>((uint32)0, out var Ꮡkey);
    ref var elem = ref heap<uint64>((uint64)(256 + 0), out var Ꮡelem);

    for (nint i = 0; i < 31; i++) {
        key += 1;
        elem += 1;
        m.Put(typ, @unsafe.Pointer.FromPinnedBox(Ꮡkey), @unsafe.Pointer.FromPinnedBox(Ꮡelem));

        if (maps_internal_test_package.DebugLog) {
            fmt.Printf("After put %d: %v\n"u8, key, m.OrTypedNil());
        }
    }

    var got = new map<uint32, uint64>();
    var it = @new<maps.Iter>();
    it.Init(typ, m);
    for (nint i = 0; ; i++) {
        it.Next();
        @unsafe.Pointer keyPtr = (uintptr)it.Key();
        @unsafe.Pointer elemPtr = (uintptr)it.Elem();
        if (keyPtr == nil) {
            break;
        }

        var keyΔ1 = ~(ж<uint32>)(uintptr)(keyPtr);
        var elemΔ1 = ~(ж<uint64>)(uintptr)(elemPtr);
        if (elemΔ1 != 256 + (uint64)keyΔ1) {
            t.Errorf("iteration got key %d elem %d want elem %d", keyΔ1, elemΔ1, 256 + (uint64)keyΔ1);
        }
        var (_, ok) = got[keyΔ1, ꟷ]; if (ok) {
            t.Errorf("iteration got key %d more than once", keyΔ1);
        }
        got[keyΔ1] = elemΔ1;

        // Grow halfway through iteration.
        if (i == 16) {
            ref var keyΔ2 = ref heap<uint32>((uint32)32, out var ᏑkeyΔ2);
            ref var elemΔ2 = ref heap<uint64>((uint64)(256 + 32), out var ᏑelemΔ2);

            for (nint iΔ1 = 0; iΔ1 < grow; iΔ1++) {
                keyΔ2 += 1;
                elemΔ2 += 1;
                m.Put(typ, @unsafe.Pointer.FromPinnedBox(ᏑkeyΔ2), @unsafe.Pointer.FromPinnedBox(ᏑelemΔ2));

                if (maps_internal_test_package.DebugLog) {
                    fmt.Printf("After put %d: %v\n"u8, keyΔ2, m.OrTypedNil());
                }
            }
        }
    }

    // Don't check length: the number of new elements we'll see is
    // unspecified.
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string growˢ => ref growˢᵇ;
private static readonly @string growˢᵇ = "grow"u8;
internal static ref readonly @string splitˢ => ref splitˢᵇ;
private static readonly @string splitˢᵇ = "split"u8;

// Grow should not allow duplicate keys to appear.
public static void TestTableIterationGrowDuplicate(ж<testing.T> t) {
    // Small grow, only enough to cause table grow.
    t.Run(growˢ, (ж<testing.T> tΔ1) => {
        testTableIterationGrowDuplicate(tΔ1, 32);
    });

    // Large grow, to cause table split.
    t.Run(splitˢ, (ж<testing.T> tΔ2) => {
        testTableIterationGrowDuplicate(tΔ2, 2 * maps_internal_test_package.MaxTableCapacity);
    });
}

internal partial struct TestAlignUpPow2_tests /*dyn*/ {
    internal uint64 @in;
    internal uint64 want;
    internal bool overflow;
}

public static void TestAlignUpPow2(ж<testing.T> t) {
    var tests = slice<TestAlignUpPow2_tests>([
        new(
            @in: 0,
            want: 0
        ),
        new(
            @in: 3,
            want: 4
        ),
        new(
            @in: 4,
            want: 4
        ),
        new(
            @in: ((uint64)1 << 63),
            want: ((uint64)1 << 63)
        ),
        new(
            @in: 9223372036854775807UL,
            want: ((uint64)1 << 63)
        ),
        new(
            @in: 9223372036854775809UL,
            overflow: true
        )
    ]);

    foreach (var (_, tc) in range(tests)) {
        var (got, overflow) = maps_internal_test_package.AlignUpPow2(tc.@in);
        if (got != tc.want) {
            t.Errorf("alignUpPow2(%d) got %d, want %d", tc.@in, got, tc.want);
        }
        if (overflow != tc.overflow) {
            t.Errorf("alignUpPow2(%d) got overflow %v, want %v", tc.@in, overflow, tc.overflow);
        }
    }
}

// Verify that a map with zero-size slot is safe to use.
public static void TestMapZeroSizeSlot(ж<testing.T> t) {
    var (m, typ) = maps_internal_test_package.NewTestMap<EmptyStruct, EmptyStruct>(16);

    ref var key = ref heap<EmptyStruct>(new EmptyStruct(), out var Ꮡkey);
    ref var elem = ref heap<EmptyStruct>(new EmptyStruct(), out var Ꮡelem);

    m.Put(typ, @unsafe.Pointer.FromPinnedBox(Ꮡkey), @unsafe.Pointer.FromPinnedBox(Ꮡelem));

    if (maps_internal_test_package.DebugLog) {
        fmt.Printf("After put %d: %v\n"u8, key, m.OrTypedNil());
    }

    var (got, ok) = m.Get(typ, @unsafe.Pointer.FromPinnedBox(Ꮡkey));
    if (!ok) {
        t.Errorf("Get(%d) got ok false want true", key);
    }
    var gotElem = ((ж<EmptyStruct>)(uintptr)(got)).Value;
    if (gotElem != elem) {
        t.Errorf("Get(%d) got elem %d want %d", key, gotElem, elem);
    }

    var tab = m.TableFor(typ, @unsafe.Pointer.FromPinnedBox(Ꮡkey));
    @unsafe.Pointer start = (uintptr)tab.GroupsStart();
    var length = tab.GroupsLength();
    @unsafe.Pointer end = (@unsafe.Pointer)((uintptr)start + length * typ.GroupSize - 1); // inclusive to ensure we have a valid pointer
    if ((uintptr)got < (uintptr)start || (uintptr)got > (uintptr)end) {
        t.Errorf("elem address outside groups allocation; got %p want [%p, %p]", @unsafe.Pointer.OrTypedNil(got), @unsafe.Pointer.OrTypedNil(start), @unsafe.Pointer.OrTypedNil(end));
    }
}

/* [abi.MapMaxKeyBytes + abi.MapMaxElemBytes]byte */
internal partial struct TestMapIndirect_big /*[256]byte*/;

public static void TestMapIndirect(ж<testing.T> t) {
    var (m, typ) = maps_internal_test_package.NewTestMap<TestMapIndirect_big, TestMapIndirect_big>(8);

    ref var key = ref heap<TestMapIndirect_big>(new TestMapIndirect_big(new byte[256].array()), out var Ꮡkey);
    ref var elem = ref heap<TestMapIndirect_big>(new TestMapIndirect_big(new byte[256].array()), out var Ꮡelem);
    elem[0] = 128;

    for (nint i = 0; i < 31; i++) {
        key[0] += 1;
        elem[0] += 1;
        m.Put(typ, @unsafe.Pointer.FromPinnedBox(Ꮡkey), @unsafe.Pointer.FromPinnedBox(Ꮡelem));

        if (maps_internal_test_package.DebugLog) {
            fmt.Printf("After put %v: %v\n"u8, key, m.OrTypedNil());
        }
    }

    if (m.Used() != 31) {
        t.Errorf("Used() used got %d want 31", m.Used());
    }

    key = new TestMapIndirect_big(new byte[256].array());
    elem = new TestMapIndirect_big(new byte[256].array());
    elem[0] = 128;

    for (nint i = 0; i < 31; i++) {
        key[0] += 1;
        elem[0] += 1;
        var (got, ok) = m.Get(typ, @unsafe.Pointer.FromPinnedBox(Ꮡkey));
        if (!ok) {
            t.Errorf("Get(%v) got ok false want true", key);
        }
        var gotElem = (~(ж<TestMapIndirect_big>)(uintptr)(got)).Clone();
        if (gotElem != elem) {
            t.Errorf("Get(%v) got elem %v want %v", key, gotElem, elem);
        }
    }
}

// Delete should clear element. See https://go.dev/issue/25936.
public static void TestMapDeleteClear(ж<testing.T> t) {
    var (m, typ) = maps_internal_test_package.NewTestMap<int64, int64>(8);

    ref var key = ref heap<int64>((int64)0, out var Ꮡkey);
    ref var elem = ref heap<int64>((int64)128, out var Ꮡelem);

    m.Put(typ, @unsafe.Pointer.FromPinnedBox(Ꮡkey), @unsafe.Pointer.FromPinnedBox(Ꮡelem));

    if (maps_internal_test_package.DebugLog) {
        fmt.Printf("After put %d: %v\n"u8, key, m.OrTypedNil());
    }

    var (got, ok) = m.Get(typ, @unsafe.Pointer.FromPinnedBox(Ꮡkey));
    if (!ok) {
        t.Errorf("Get(%d) got ok false want true", key);
    }
    var gotElem = ~(ж<int64>)(uintptr)(got);
    if (gotElem != elem) {
        t.Errorf("Get(%d) got elem %d want %d", key, gotElem, elem);
    }

    m.Delete(typ, @unsafe.Pointer.FromPinnedBox(Ꮡkey));

    gotElem = ~(ж<int64>)(uintptr)(got);
    if (gotElem != 0) {
        t.Errorf("Delete(%d) failed to clear element. got %d want 0", key, gotElem);
    }
}

internal static bool alwaysFalse;
internal static any escapeSink;

internal static T escape<T>(T x) {
    if (alwaysFalse) {
        escapeSink = (x).OrTypedNilOf();
    }
    return x;
}

internal static UntypedInt belowMax => /* abi.MapGroupSlots * 3 / 2 */ 12;                                       // 1.5 * group max = 2 groups @ 75%
internal static UntypedInt atMax => /* (2 * abi.MapGroupSlots * maps.MaxAvgGroupLoad) / abi.MapGroupSlots */ 14; // 2 groups at 7/8 full.

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string mapliteralˢ => ref mapliteralˢᵇ;
private static readonly @string mapliteralˢᵇ = "mapliteral"u8;
internal static ref readonly @string escapeˢ => ref escapeˢᵇ;
private static readonly @string escapeˢᵇ = "escape"u8;
internal static ref readonly @string nohintˢ => ref nohintˢᵇ;
private static readonly @string nohintˢᵇ = "nohint"u8;
internal static ref readonly @string makemapˢ => ref makemapˢᵇ;
private static readonly @string makemapˢᵇ = "makemap"u8;
internal static ref readonly @string makemap64ˢ => ref makemap64ˢᵇ;
private static readonly @string makemap64ˢᵇ = "makemap64"u8;

internal partial struct TestTableGroupCount_mapCount /*dyn*/ {
    internal nint tables;
    internal uint64 groups;
}

internal partial struct TestTableGroupCount_mapCase /*dyn*/ {
    internal TestTableGroupCount_mapCount initialLit;
    internal TestTableGroupCount_mapCount initialHint;
    internal TestTableGroupCount_mapCount after;
}

internal partial struct TestTableGroupCount_type /*dyn*/ {
    internal nint n;                             // n is the number of map elements
    internal TestTableGroupCount_mapCase escape; // expected values for escaping map
}

public static void TestTableGroupCount(ж<testing.T> t) {
    // Test that maps of different sizes have the right number of
    // tables/groups.

    slice<TestTableGroupCount_type> testCases = slice<TestTableGroupCount_type>([
        new(
            n: -(1 << 30),
            escape: new TestTableGroupCount_mapCase(
                initialLit: new TestTableGroupCount_mapCount(0, 0),
                initialHint: new TestTableGroupCount_mapCount(0, 0),
                after: new TestTableGroupCount_mapCount(0, 0)
            )
        ),
        new(
            n: -1,
            escape: new TestTableGroupCount_mapCase(
                initialLit: new TestTableGroupCount_mapCount(0, 0),
                initialHint: new TestTableGroupCount_mapCount(0, 0),
                after: new TestTableGroupCount_mapCount(0, 0)
            )
        ),
        new(
            n: 0,
            escape: new TestTableGroupCount_mapCase(
                initialLit: new TestTableGroupCount_mapCount(0, 0),
                initialHint: new TestTableGroupCount_mapCount(0, 0),
                after: new TestTableGroupCount_mapCount(0, 0)
            )
        ),
        new(
            n: 1,
            escape: new TestTableGroupCount_mapCase(
                initialLit: new TestTableGroupCount_mapCount(0, 0),
                initialHint: new TestTableGroupCount_mapCount(0, 0),
                after: new TestTableGroupCount_mapCount(0, 1)
            )
        ),
        new(
            n: abi.MapGroupSlots,
            escape: new TestTableGroupCount_mapCase(
                initialLit: new TestTableGroupCount_mapCount(0, 0),
                initialHint: new TestTableGroupCount_mapCount(0, 0),
                after: new TestTableGroupCount_mapCount(0, 1)
            )
        ),
        new(
            n: abi.MapGroupSlots + 1,
            escape: new TestTableGroupCount_mapCase(
                initialLit: new TestTableGroupCount_mapCount(0, 0),
                initialHint: new TestTableGroupCount_mapCount(1, 2),
                after: new TestTableGroupCount_mapCount(1, 2)
            )
        ),
        new(
            n: belowMax, // 1.5 group max = 2 groups @ 75%
            escape: new TestTableGroupCount_mapCase(
                initialLit: new TestTableGroupCount_mapCount(0, 0),
                initialHint: new TestTableGroupCount_mapCount(1, 2),
                after: new TestTableGroupCount_mapCount(1, 2)
            )
        ),
        new(
            n: atMax, // 2 groups at max
            escape: new TestTableGroupCount_mapCase(
                initialLit: new TestTableGroupCount_mapCount(0, 0),
                initialHint: new TestTableGroupCount_mapCount(1, 2),
                after: new TestTableGroupCount_mapCount(1, 2)
            )
        ),
        new(
            n: atMax + 1, // 2 groups at max + 1 -> grow to 4 groups
            escape: new TestTableGroupCount_mapCase(
                initialLit: new TestTableGroupCount_mapCount(0, 0),
                initialHint: new TestTableGroupCount_mapCount(1, 4),
                after: new TestTableGroupCount_mapCount(1, 4)
            )
        ),
        new(
            n: 2 * belowMax, // 3 * group max = 4 groups @75%
            escape: new TestTableGroupCount_mapCase(
                initialLit: new TestTableGroupCount_mapCount(0, 0),
                initialHint: new TestTableGroupCount_mapCount(1, 4),
                after: new TestTableGroupCount_mapCount(1, 4)
            )
        ),
        new(
            n: 2 * atMax + 1, // 4 groups at max + 1 -> grow to 8 groups
            escape: new TestTableGroupCount_mapCase(
                initialLit: new TestTableGroupCount_mapCount(0, 0),
                initialHint: new TestTableGroupCount_mapCount(1, 8),
                after: new TestTableGroupCount_mapCount(1, 8)
            )
        )
    ]);

    void testMap(ж<testing.T> tΔ1, map<nint, nint> mʗp, nint n, TestTableGroupCount_mapCount initial, TestTableGroupCount_mapCount after) {
        ref var m = ref heap(mʗp, out var Ꮡm);
        var mm = ~Ꮡm.Reinterpret<map<nint, nint>, ж<mapsꓸMap>>();

        nint gotTab = mm.TableCount();
        if (gotTab != initial.tables) {
            tΔ1.Errorf("initial TableCount got %d want %d", gotTab, initial.tables);
        }

        var gotGroup = mm.GroupCount();
        if (gotGroup != initial.groups) {
            tΔ1.Errorf("initial GroupCount got %d want %d", gotGroup, initial.groups);
        }

        for (nint i = 0; i < n; i++) {
            m[i] = i;
        }

        gotTab = mm.TableCount();
        if (gotTab != after.tables) {
            tΔ1.Errorf("after TableCount got %d want %d", gotTab, after.tables);
        }

        gotGroup = mm.GroupCount();
        if (gotGroup != after.groups) {
            tΔ1.Errorf("after GroupCount got %d want %d", gotGroup, after.groups);
        }
    }

    var testCasesʗ1 = testCases;
    var testMapʗ1 = testMap;
    t.Run(mapliteralˢ, (ж<testing.T> tΔ2) => {
        foreach (var (_, vᴛ1) in range(testCasesʗ1)) {
            ref var tc = ref heap(new TestTableGroupCount_type(), out var Ꮡtc);
            tc = vᴛ1;

            var tcʗ1 = tc;
            var testMapʗ2 = testMapʗ1;
            tΔ2.Run(fmt.Sprintf("n=%d"u8, tc.n), (ж<testing.T> tΔ3) => {
                var tcʗ2 = tcʗ1;
                var testMapʗ3 = testMapʗ2;
                tΔ3.Run(escapeˢ, (ж<testing.T> tΔ4) => {
                    var m = escape(new map<nint, nint>{});
                    testMapʗ3(tΔ4, m, tcʗ2.n, tcʗ2.escape.initialLit, tcʗ2.escape.after);
                });
            });
        }
    });
    var testCasesʗ2 = testCases;
    var testMapʗ4 = testMap;
    t.Run(nohintˢ, (ж<testing.T> tΔ5) => {
        foreach (var (_, vᴛ2) in range(testCasesʗ2)) {
            ref var tc = ref heap(new TestTableGroupCount_type(), out var Ꮡtc);
            tc = vᴛ2;

            var tcʗ3 = tc;
            var testMapʗ5 = testMapʗ4;
            tΔ5.Run(fmt.Sprintf("n=%d"u8, tc.n), (ж<testing.T> tΔ6) => {
                var tcʗ4 = tcʗ3;
                var testMapʗ6 = testMapʗ5;
                tΔ6.Run(escapeˢ, (ж<testing.T> tΔ7) => {
                    var m = escape(new map<nint, nint>());
                    testMapʗ6(tΔ7, m, tcʗ4.n, tcʗ4.escape.initialLit, tcʗ4.escape.after);
                });
            });
        }
    });
    var testCasesʗ3 = testCases;
    var testMapʗ7 = testMap;
    t.Run(makemapˢ, (ж<testing.T> tΔ8) => {
        foreach (var (_, vᴛ3) in range(testCasesʗ3)) {
            ref var tc = ref heap(new TestTableGroupCount_type(), out var Ꮡtc);
            tc = vᴛ3;

            var tcʗ5 = tc;
            var testMapʗ8 = testMapʗ7;
            tΔ8.Run(fmt.Sprintf("n=%d"u8, tc.n), (ж<testing.T> tΔ9) => {
                var tcʗ6 = tcʗ5;
                var testMapʗ9 = testMapʗ8;
                tΔ9.Run(escapeˢ, (ж<testing.T> tΔ10) => {
                    var m = escape(new map<nint, nint>(tcʗ6.n));
                    testMapʗ9(tΔ10, m, tcʗ6.n, tcʗ6.escape.initialHint, tcʗ6.escape.after);
                });
            });
        }
    });
    var testCasesʗ4 = testCases;
    var testMapʗ10 = testMap;
    t.Run(makemap64ˢ, (ж<testing.T> tΔ11) => {
        foreach (var (_, vᴛ4) in range(testCasesʗ4)) {
            ref var tc = ref heap(new TestTableGroupCount_type(), out var Ꮡtc);
            tc = vᴛ4;

            var tcʗ7 = tc;
            var testMapʗ11 = testMapʗ10;
            tΔ11.Run(fmt.Sprintf("n=%d"u8, tc.n), (ж<testing.T> tΔ12) => {
                var tcʗ8 = tcʗ7;
                var testMapʗ12 = testMapʗ11;
                tΔ12.Run(escapeˢ, (ж<testing.T> tΔ13) => {
                    var m = escape(new map<nint, nint>((nint)((int64)tcʗ8.n)));
                    testMapʗ12(tΔ13, m, tcʗ8.n, tcʗ8.escape.initialHint, tcʗ8.escape.after);
                });
            });
        }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object lenMShouldRemainConstantˢ = (@string)"len(m) should remain constant"u8;

public static void TestTombstoneGrow(ж<testing.T> t) {
    var tableSizes = slice<nint>([16, 32, 64, 128, 256]);
    foreach (var (_, tableSize) in range(tableSizes)) {
        foreach (var (_, load) in range(slice<@string>(["low", "mid", "high"]))) {
            nint capacity = tableSize * 7 / 8;
            nint initialElems = default;
            var exprᴛ1 = load;
            switch (ᐧ) {
            case {} when exprᴛ1 == "low"u8: {
                initialElems = capacity / 8;
                break;
            }
            case {} when exprᴛ1 == "mid"u8: {
                initialElems = capacity / 2;
                break;
            }
            case {} when exprᴛ1 == "high"u8: {
                initialElems = capacity;
                break;
            }}
            t.Run(fmt.Sprintf("tableSize=%d/elems=%d/load=%0.3f"u8, tableSize, initialElems, (float64)initialElems / (float64)tableSize), (ж<testing.T> tΔ1) => {
                var allocs = testing.AllocsPerRun(1, () => {
                    // Fill the map with elements.
                    var m = new map<nint, nint>(capacity);
                    foreach (var i in range(initialElems)) {
                        m[i] = i;
                    }

                    // This is the heart of our test.
                    // Loop over the map repeatedly, deleting a key then adding a not-yet-seen key
                    // while keeping the map at a ~constant number of elements (+/-1).
                    nint nextKey = initialElems;
                    foreach (var _ᴛ1 in range(100000)) {
                        foreach (var (k, _) in m) {
                            delete(m, k);
                            break;
                        }
                        m[nextKey] = nextKey;
                        nextKey++;
                        if (len(m) != initialElems) {
                            tΔ1.Fatal(lenMShouldRemainConstantˢ);
                        }
                    }
                });

                // The make has 4 allocs (map, directory, table, groups).
                // Each growth has 2 allocs (table, groups).
                // We allow two growths if we start full, 1 otherwise.
                // Fail (somewhat arbitrarily) if there are more than that.
                var allowed = (float64)(4D + 1D * 2D);
                if (initialElems == capacity) {
                    allowed += 2D;
                }
                if (allocs > allowed) {
                    tΔ1.Fatalf("got %v allocations, allowed %v", allocs, allowed);
                }
            });
        }
    }
}

} // end maps_test_package
