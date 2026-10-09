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

public static void TestCtrlSize(ж<testing.T> Ꮡt) {
    var cs = /* unsafe.Sizeof(maps.CtrlGroup(0)) */ (uintptr)8;
    if (cs != abi.SwissMapGroupSlots) {
        Ꮡt.Errorf("ctrlGroup size got %d want abi.SwissMapGroupSlots %d"u8, cs, (nint)(abi.SwissMapGroupSlots));
    }
}

public static void TestMapPut(ж<testing.T> Ꮡt) {
    var (m, typ) = maps_internal_test_package.NewTestMap<uint32, uint64>(8);
    ref var key = ref heap<uint32>(out var Ꮡkey);
    key = (uint32)0;
    ref var elem = ref heap<uint64>(out var Ꮡelem);
    elem = (uint64)(256 + 0);
    for (nint i = 0; i < 31; i++) {
        key += 1;
        elem += 1;
        m.Put(typ, @unsafe.Pointer.FromPinnedBox(Ꮡkey), @unsafe.Pointer.FromPinnedBox(Ꮡelem));
        if (maps_internal_test_package.DebugLog) {
            fmt.Printf("After put %d: %v\n"u8, key, m.OrTypedNil());
        }
    }
    if (m.Used() != 31) {
        Ꮡt.Errorf("Used() used got %d want 31"u8, m.Used());
    }
    key = (uint32)0;
    elem = (uint64)(256 + 0);
    for (nint i = 0; i < 31; i++) {
        key += 1;
        elem += 1;
        var (got, ok) = m.Get(typ, @unsafe.Pointer.FromPinnedBox(Ꮡkey));
        if (!ok) {
            Ꮡt.Errorf("Get(%d) got ok false want true"u8, key);
        }
        var gotElem = ~(ж<uint64>)(uintptr)(got);
        if (gotElem != elem) {
            Ꮡt.Errorf("Get(%d) got elem %d want %d"u8, key, gotElem, elem);
        }
    }
}

// Grow enough to cause a table split.
public static void TestMapSplit(ж<testing.T> Ꮡt) {
    var (m, typ) = maps_internal_test_package.NewTestMap<uint32, uint64>(0);
    ref var key = ref heap<uint32>(out var Ꮡkey);
    key = (uint32)0;
    ref var elem = ref heap<uint64>(out var Ꮡelem);
    elem = (uint64)(256 + 0);
    for (nint i = 0; i < (nint)(2 * maps_internal_test_package.MaxTableCapacity); i++) {
        key += 1;
        elem += 1;
        m.Put(typ, @unsafe.Pointer.FromPinnedBox(Ꮡkey), @unsafe.Pointer.FromPinnedBox(Ꮡelem));
        if (maps_internal_test_package.DebugLog) {
            fmt.Printf("After put %d: %v\n"u8, key, m.OrTypedNil());
        }
    }
    if (m.Used() != (uint64)(2 * maps_internal_test_package.MaxTableCapacity)) {
        Ꮡt.Errorf("Used() used got %d want 31"u8, m.Used());
    }
    key = (uint32)0;
    elem = (uint64)(256 + 0);
    for (nint i = 0; i < (nint)(2 * maps_internal_test_package.MaxTableCapacity); i++) {
        key += 1;
        elem += 1;
        var (got, ok) = m.Get(typ, @unsafe.Pointer.FromPinnedBox(Ꮡkey));
        if (!ok) {
            Ꮡt.Errorf("Get(%d) got ok false want true"u8, key);
        }
        var gotElem = ~(ж<uint64>)(uintptr)(got);
        if (gotElem != elem) {
            Ꮡt.Errorf("Get(%d) got elem %d want %d"u8, key, gotElem, elem);
        }
    }
}

public static void TestMapDelete(ж<testing.T> Ꮡt) {
    var (m, typ) = maps_internal_test_package.NewTestMap<uint32, uint64>(32);
    ref var key = ref heap<uint32>(out var Ꮡkey);
    key = (uint32)0;
    ref var elem = ref heap<uint64>(out var Ꮡelem);
    elem = (uint64)(256 + 0);
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
        Ꮡt.Errorf("Used() used got %d want 0"u8, m.Used());
    }
    key = (uint32)0;
    elem = (uint64)(256 + 0);
    for (nint i = 0; i < 31; i++) {
        key += 1;
        elem += 1;
        var (_, ok) = m.Get(typ, @unsafe.Pointer.FromPinnedBox(Ꮡkey));
        if (ok) {
            Ꮡt.Errorf("Get(%d) got ok true want false"u8, key);
        }
    }
}

public static void TestTableClear(ж<testing.T> Ꮡt) {
    var (m, typ) = maps_internal_test_package.NewTestMap<uint32, uint64>(32);
    ref var key = ref heap<uint32>(out var Ꮡkey);
    key = (uint32)0;
    ref var elem = ref heap<uint64>(out var Ꮡelem);
    elem = (uint64)(256 + 0);
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
        Ꮡt.Errorf("Clear() used got %d want 0"u8, m.Used());
    }
    key = (uint32)0;
    elem = (uint64)(256 + 0);
    for (nint i = 0; i < 31; i++) {
        key += 1;
        elem += 1;
        var (_, ok) = m.Get(typ, @unsafe.Pointer.FromPinnedBox(Ꮡkey));
        if (ok) {
            Ꮡt.Errorf("Get(%d) got ok true want false"u8, key);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object itKeyGotNilWantKeyˢ = (@string)"it.Key() got nil want key"u8;

// +0.0 and -0.0 compare equal, but we must still must update the key slot when
// overwriting.
public static void TestTableKeyUpdate(ж<testing.T> Ꮡt) {
    var (m, typ) = maps_internal_test_package.NewTestMap<float64, uint64>(8);
    ref var zero = ref heap<float64>(out var Ꮡzero);
    zero = (float64)0.0D;
    ref var negZero = ref heap<float64>(out var ᏑnegZero);
    negZero = math.Copysign(zero, -1.0D);
    ref var elem = ref heap<uint64>(out var Ꮡelem);
    elem = (uint64)0;
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
        Ꮡt.Errorf("Used() used got %d want 1"u8, m.Used());
    }
    var it = @new<maps.Iter>();
    it.Init(typ, m);
    it.Next();
    @unsafe.Pointer keyPtr = (uintptr)it.Key();
    @unsafe.Pointer elemPtr = (uintptr)it.Elem();
    if (keyPtr == nil) {
        Ꮡt.Fatal(itKeyGotNilWantKeyˢ);
    }
    var key = ~(ж<float64>)(uintptr)(keyPtr);
    elem = ~(ж<uint64>)(uintptr)(elemPtr);
    if (math.Copysign(1.0D, key) > 0D) {
        Ꮡt.Errorf("map key %f has positive sign"u8, key);
    }
    if (elem != 1) {
        Ꮡt.Errorf("map elem got %d want 1"u8, elem);
    }
}

// Put should reuse a deleted slot rather than consuming an empty slot.
public static void TestTablePutDelete(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // Put will reuse the first deleted slot it encounters.
    //
    // This is awkward to test because Delete will only install ctrlDeleted
    // if the group is full, otherwise it goes straight to empty.
    //
    // So first we must add to the table continuously until we happen to
    // fill a group.
    // Avoid small maps, they have no tables.
    var (m, typ) = maps_internal_test_package.NewTestMap<uint32, uint32>(16);
    ref var key = ref heap<uint32>(out var Ꮡkey);
    key = (uint32)0;
    ref var elem = ref heap<uint32>(out var Ꮡelem);
    elem = (uint32)(256 + 0);
    while (ᐧ) {
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
        Ꮡt.Errorf("Put(%d) grew table got %v want %v map %v"u8, key, tabGot.OrTypedNil(), tabWant.OrTypedNil(), m.OrTypedNil());
    }
    if (growthLeftGot != growthLeftWant) {
        Ꮡt.Errorf("GrowthLeft got %d want %d: map %v tab %v"u8, growthLeftGot, growthLeftWant, m.OrTypedNil(), tabGot.OrTypedNil());
    }
}

public static void TestTableIteration(ж<testing.T> Ꮡt) {
    var (m, typ) = maps_internal_test_package.NewTestMap<uint32, uint64>(8);
    ref var key = ref heap<uint32>(out var Ꮡkey);
    key = (uint32)0;
    ref var elem = ref heap<uint64>(out var Ꮡelem);
    elem = (uint64)(256 + 0);
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
    while (ᐧ) {
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
        Ꮡt.Errorf("Iteration got %d entries, want 31: %+v"u8, len(got), got);
    }
    key = (uint32)0;
    elem = (uint64)(256 + 0);
    for (nint i = 0; i < 31; i++) {
        key += 1;
        elem += 1;
        var (gotElem, ok) = got[key, ꟷ];
        if (!ok) {
            Ꮡt.Errorf("Iteration missing key %d"u8, key);
            continue;
        }
        if (gotElem != elem) {
            Ꮡt.Errorf("Iteration key %d got elem %d want %d"u8, key, gotElem, elem);
        }
    }
}

// Deleted keys shouldn't be visible in iteration.
public static void TestTableIterationDelete(ж<testing.T> Ꮡt) {
    var (m, typ) = maps_internal_test_package.NewTestMap<uint32, uint64>(8);
    ref var key = ref heap<uint32>(out var Ꮡkey);
    key = (uint32)0;
    ref var elem = ref heap<uint64>(out var Ꮡelem);
    elem = (uint64)(256 + 0);
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
    ref var deletedKey = ref heap<uint32>(out var ᏑdeletedKey);
    deletedKey = (uint32)1;
    var it = @new<maps.Iter>();
    it.Init(typ, m);
    while (ᐧ) {
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
        Ꮡt.Errorf("Iteration got %d entries, want 30: %+v"u8, len(got), got);
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
            Ꮡt.Errorf("Iteration key %d got ok %v want ok %v"u8, key, gotOK, wantOK);
            continue;
        }
        if (wantOK && gotElem != elem) {
            Ꮡt.Errorf("Iteration key %d got elem %d want %d"u8, key, gotElem, elem);
        }
    }
}

// Deleted keys shouldn't be visible in iteration even after a grow.
public static void TestTableIterationGrowDelete(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var (m, typ) = maps_internal_test_package.NewTestMap<uint32, uint64>(8);
    ref var key = ref heap<uint32>(out var Ꮡkey);
    key = (uint32)0;
    ref var elem = ref heap<uint64>(out var Ꮡelem);
    elem = (uint64)(256 + 0);
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
    ref var deletedKey = ref heap<uint32>(out var ᏑdeletedKey);
    deletedKey = (uint32)1;
    var it = @new<maps.Iter>();
    it.Init(typ, m);
    while (ᐧ) {
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
            ref var keyΔ2 = ref heap<uint32>(out var ᏑkeyΔ2);
            keyΔ2 = (uint32)32;
            ref var elemΔ2 = ref heap<uint64>(out var ᏑelemΔ2);
            elemΔ2 = (uint64)(256 + 32);
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
            Ꮡt.Errorf("Iteration key %d got ok %v want ok %v"u8, key, gotOK, wantOK);
            continue;
        }
        if (wantOK && gotElem != elem) {
            Ꮡt.Errorf("Iteration key %d got elem %d want %d"u8, key, gotElem, elem);
        }
    }
}

internal static void testTableIterationGrowDuplicate(ж<testing.T> Ꮡt, nint grow) {
    var (m, typ) = maps_internal_test_package.NewTestMap<uint32, uint64>(8);
    ref var key = ref heap<uint32>(out var Ꮡkey);
    key = (uint32)0;
    ref var elem = ref heap<uint64>(out var Ꮡelem);
    elem = (uint64)(256 + 0);
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
    for (nint i = 0; ᐧ ; i++) {
        it.Next();
        @unsafe.Pointer keyPtr = (uintptr)it.Key();
        @unsafe.Pointer elemPtr = (uintptr)it.Elem();
        if (keyPtr == nil) {
            break;
        }
        var keyΔ1 = ~(ж<uint32>)(uintptr)(keyPtr);
        var elemΔ1 = ~(ж<uint64>)(uintptr)(elemPtr);
        if (elemΔ1 != 256 + (uint64)keyΔ1) {
            Ꮡt.Errorf("iteration got key %d elem %d want elem %d"u8, keyΔ1, elemΔ1, 256 + (uint64)keyΔ1);
        }
        {
            var (_, ok) = got[keyΔ1, ꟷ]; if (ok) {
                Ꮡt.Errorf("iteration got key %d more than once"u8, keyΔ1);
            }
        }
        got[keyΔ1] = elemΔ1;
        // Grow halfway through iteration.
        if (i == 16) {
            ref var keyΔ2 = ref heap<uint32>(out var ᏑkeyΔ2);
            keyΔ2 = (uint32)32;
            ref var elemΔ2 = ref heap<uint64>(out var ᏑelemΔ2);
            elemΔ2 = (uint64)(256 + 32);
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
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string growˢ = "grow"u8;
internal static readonly @string splitˢ = "split"u8;

// Don't check length: the number of new elements we'll see is
// unspecified.

// Grow should not allow duplicate keys to appear.
public static void TestTableIterationGrowDuplicate(ж<testing.T> Ꮡt) {
    // Small grow, only enough to cause table grow.
    Ꮡt.Run(growˢ, (ж<testing.T> tΔ1) => {
        testTableIterationGrowDuplicate(tΔ1, 32);
    });
    // Large grow, to cause table split.
    Ꮡt.Run(splitˢ, (ж<testing.T> tΔ2) => {
        testTableIterationGrowDuplicate(tΔ2, 2 * maps_internal_test_package.MaxTableCapacity);
    });
}

internal partial struct TestAlignUpPow2_tests /*dyn*/ {
    internal uint64 @in;
    internal uint64 want;
    internal bool overflow;
}

public static void TestAlignUpPow2(ж<testing.T> Ꮡt) {
    var tests = new TestAlignUpPow2_tests[]{
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
            @in: ((uint64)1 << (int)(63)),
            want: ((uint64)1 << (int)(63))
        ),
        new(
            @in: 9223372036854775807UL,
            want: ((uint64)1 << (int)(63))
        ),
        new(
            @in: 9223372036854775809UL,
            overflow: true
        )
    }.slice();
    foreach (var (_, tc) in tests) {
        var (got, overflow) = maps_internal_test_package.AlignUpPow2(tc.@in);
        if (got != tc.want) {
            Ꮡt.Errorf("alignUpPow2(%d) got %d, want %d"u8, tc.@in, got, tc.want);
        }
        if (overflow != tc.overflow) {
            Ꮡt.Errorf("alignUpPow2(%d) got overflow %v, want %v"u8, tc.@in, overflow, tc.overflow);
        }
    }
}

// Verify that a map with zero-size slot is safe to use.
public static void TestMapZeroSizeSlot(ж<testing.T> Ꮡt) {
    var (m, typ) = maps_internal_test_package.NewTestMap<EmptyStruct, EmptyStruct>(16);
    ref var key = ref heap<EmptyStruct>(out var Ꮡkey);
    key = new EmptyStruct();
    ref var elem = ref heap<EmptyStruct>(out var Ꮡelem);
    elem = new EmptyStruct();
    m.Put(typ, @unsafe.Pointer.FromPinnedBox(Ꮡkey), @unsafe.Pointer.FromPinnedBox(Ꮡelem));
    if (maps_internal_test_package.DebugLog) {
        fmt.Printf("After put %d: %v\n"u8, key, m.OrTypedNil());
    }
    var (got, ok) = m.Get(typ, @unsafe.Pointer.FromPinnedBox(Ꮡkey));
    if (!ok) {
        Ꮡt.Errorf("Get(%d) got ok false want true"u8, key);
    }
    var gotElem = ((ж<EmptyStruct>)(uintptr)(got)).Value;
    if (gotElem != elem) {
        Ꮡt.Errorf("Get(%d) got elem %d want %d"u8, key, gotElem, elem);
    }
    var tab = m.TableFor(typ, @unsafe.Pointer.FromPinnedBox(Ꮡkey));
    @unsafe.Pointer start = (uintptr)tab.GroupsStart();
    var length = tab.GroupsLength();
    @unsafe.Pointer end = (@unsafe.Pointer)((uintptr)start + length * (~typ).GroupSize - 1); // inclusive to ensure we have a valid pointer
    if ((uintptr)got < (uintptr)start || (uintptr)got > (uintptr)end) {
        Ꮡt.Errorf("elem address outside groups allocation; got %p want [%p, %p]"u8, @unsafe.Pointer.OrTypedNil(got), @unsafe.Pointer.OrTypedNil(start), @unsafe.Pointer.OrTypedNil(end));
    }
}

/* [abi.SwissMapMaxKeyBytes + abi.SwissMapMaxElemBytes]byte */
internal partial struct TestMapIndirect_big /*[256]byte*/;

public static void TestMapIndirect(ж<testing.T> Ꮡt) {
    var (m, typ) = maps_internal_test_package.NewTestMap<TestMapIndirect_big, TestMapIndirect_big>(8);
    ref var key = ref heap<TestMapIndirect_big>(out var Ꮡkey);
    key = new TestMapIndirect_big(new byte[256].array());
    ref var elem = ref heap<TestMapIndirect_big>(out var Ꮡelem);
    elem = new TestMapIndirect_big(new byte[256].array());
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
        Ꮡt.Errorf("Used() used got %d want 31"u8, m.Used());
    }
    key = new TestMapIndirect_big(new byte[256].array());
    elem = new TestMapIndirect_big(new byte[256].array());
    elem[0] = 128;
    for (nint i = 0; i < 31; i++) {
        key[0] += 1;
        elem[0] += 1;
        var (got, ok) = m.Get(typ, @unsafe.Pointer.FromPinnedBox(Ꮡkey));
        if (!ok) {
            Ꮡt.Errorf("Get(%v) got ok false want true"u8, key);
        }
        var gotElem = (~(ж<TestMapIndirect_big>)(uintptr)(got)).Clone();
        if (gotElem != elem) {
            Ꮡt.Errorf("Get(%v) got elem %v want %v"u8, key, gotElem, elem);
        }
    }
}

// Delete should clear element. See https://go.dev/issue/25936.
public static void TestMapDeleteClear(ж<testing.T> Ꮡt) {
    var (m, typ) = maps_internal_test_package.NewTestMap<int64, int64>(8);
    ref var key = ref heap<int64>(out var Ꮡkey);
    key = (int64)0;
    ref var elem = ref heap<int64>(out var Ꮡelem);
    elem = (int64)128;
    m.Put(typ, @unsafe.Pointer.FromPinnedBox(Ꮡkey), @unsafe.Pointer.FromPinnedBox(Ꮡelem));
    if (maps_internal_test_package.DebugLog) {
        fmt.Printf("After put %d: %v\n"u8, key, m.OrTypedNil());
    }
    var (got, ok) = m.Get(typ, @unsafe.Pointer.FromPinnedBox(Ꮡkey));
    if (!ok) {
        Ꮡt.Errorf("Get(%d) got ok false want true"u8, key);
    }
    var gotElem = ~(ж<int64>)(uintptr)(got);
    if (gotElem != elem) {
        Ꮡt.Errorf("Get(%d) got elem %d want %d"u8, key, gotElem, elem);
    }
    m.Delete(typ, @unsafe.Pointer.FromPinnedBox(Ꮡkey));
    gotElem = ~(ж<int64>)(uintptr)(got);
    if (gotElem != 0) {
        Ꮡt.Errorf("Delete(%d) failed to clear element. got %d want 0"u8, key, gotElem);
    }
}

} // end maps_test_package
