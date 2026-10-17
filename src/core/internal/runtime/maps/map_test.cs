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
    if (cs != abi.MapGroupSlots) {
        Ꮡt.Errorf("ctrlGroup size got %d want abi.MapGroupSlots %d"u8, cs, (nint)(abi.MapGroupSlots));
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

    // Don't check length: the number of new elements we'll see is
    // unspecified.
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string growˢ = "grow"u8;
internal static readonly @string splitˢ = "split"u8;

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

/* [abi.MapMaxKeyBytes + abi.MapMaxElemBytes]byte */
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

internal static bool alwaysFalse;

internal static any escapeSink;

internal static T escape<T>(T x) {
    if (alwaysFalse) {
        escapeSink = (x).OrTypedNilOf();
    }
    return x;
}

internal static UntypedInt belowMax => /* abi.MapGroupSlots * 3 / 2 */ 12;                         // 1.5 * group max = 2 groups @ 75%
internal static UntypedInt atMax => /* (2 * abi.MapGroupSlots * maps.MaxAvgGroupLoad) / abi.MapGroupSlots */ 14; // 2 groups at 7/8 full.

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string mapliteralˢ = "mapliteral"u8;
internal static readonly @string escapeˢ = "escape"u8;
internal static readonly @string nohintˢ = "nohint"u8;
internal static readonly @string makemapˢ = "makemap"u8;
internal static readonly @string makemap64ˢ = "makemap64"u8;

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
    internal nint n;    // n is the number of map elements
    internal TestTableGroupCount_mapCase escape; // expected values for escaping map
}

public static void TestTableGroupCount(ж<testing.T> Ꮡt) {
    // Test that maps of different sizes have the right number of
    // tables/groups.

    slice<TestTableGroupCount_type> testCases = new TestTableGroupCount_type[]{
        new(
            n: -((1 << (int)(30))),
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
    }.slice();

    void testMap(ж<testing.T> tΔ1, map<nint, nint> mʗp, nint n, TestTableGroupCount_mapCount initial, TestTableGroupCount_mapCount after) {
        ref var m = ref heap(mʗp, out var Ꮡm);
        var mm = ~Ꮡm.Reinterpret<map<nint, nint>, ж<mapsꓸMap>>();

        nint gotTab = mm.TableCount();
        if (gotTab != initial.tables) {
            tΔ1.Errorf("initial TableCount got %d want %d"u8, gotTab, initial.tables);
        }

        var gotGroup = mm.GroupCount();
        if (gotGroup != initial.groups) {
            tΔ1.Errorf("initial GroupCount got %d want %d"u8, gotGroup, initial.groups);
        }

        for (nint i = 0; i < n; i++) {
            m[i] = i;
        }

        gotTab = mm.TableCount();
        if (gotTab != after.tables) {
            tΔ1.Errorf("after TableCount got %d want %d"u8, gotTab, after.tables);
        }

        gotGroup = mm.GroupCount();
        if (gotGroup != after.groups) {
            tΔ1.Errorf("after GroupCount got %d want %d"u8, gotGroup, after.groups);
        }
    }

    var testCasesʗ1 = testCases;
    var testMapʗ1 = testMap;
    Ꮡt.Run(mapliteralˢ, (ж<testing.T> tΔ2) => {
        foreach (var (_, vᴛ1) in testCasesʗ1) {
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
    Ꮡt.Run(nohintˢ, (ж<testing.T> tΔ5) => {
        foreach (var (_, vᴛ2) in testCasesʗ2) {
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
    Ꮡt.Run(makemapˢ, (ж<testing.T> tΔ8) => {
        foreach (var (_, vᴛ3) in testCasesʗ3) {
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
    Ꮡt.Run(makemap64ˢ, (ж<testing.T> tΔ11) => {
        foreach (var (_, vᴛ4) in testCasesʗ4) {
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

public static void TestTombstoneGrow(ж<testing.T> Ꮡt) {
    var tableSizes = new nint[]{16, 32, 64, 128, 256}.slice();
    foreach (var (_, tableSize) in tableSizes) {
        foreach (var (_, load) in new @string[]{"low"u8, "mid"u8, "high"u8}.slice()) {
            nint capacity = tableSize * 7 / 8;
            nint initialElems = default!;
            var exprᴛ1 = load;
            if (exprᴛ1 == "low"u8) {
                initialElems = capacity / 8;
            }
            else if (exprᴛ1 == "mid"u8) {
                initialElems = capacity / 2;
            }
            else if (exprᴛ1 == "high"u8) {
                initialElems = capacity;
            }
            Ꮡt.Run(fmt.Sprintf("tableSize=%d/elems=%d/load=%0.3f"u8, tableSize, initialElems, (float64)initialElems / (float64)tableSize), (ж<testing.T> tΔ1) => {
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
                    tΔ1.Fatalf("got %v allocations, allowed %v"u8, allocs, allowed);
                }
            });
        }
    }
}

} // end maps_test_package
