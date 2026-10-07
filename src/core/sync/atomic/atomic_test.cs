// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.sync;

using fmt = fmt_package;
using reflect = reflect_package;
using runtime = runtime_package;
using debug = go.runtime.debug_package;
using strings = strings_package;
using static go.sync.atomic_package;
using testing = testing_package;
using @unsafe = unsafe_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using atomic = go.sync.atomic_package;
using go.runtime;
using go.sync;

partial class atomic_test_package {

// Tests of correct behavior, without contention.
// (Does the function work as advertised?)
//
// Test that the Add functions add correctly.
// Test that the CompareAndSwap functions actually
// do the comparison and the swap correctly.
//
// The loop over power-of-two values is meant to
// ensure that the operations apply to the full word size.
// The struct fields x.before and x.after check that the
// operations do not extend past the full word size.
internal static UntypedInt magic32 => 0xdedbeef;
internal static UntypedInt magic64 => 0xdeddeadbeefbeef;

[GoType("dyn")] internal partial struct TestSwapInt32_x {
    internal int32 before;
    internal int32 i;
    internal int32 after;
}

public static void TestSwapInt32(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestSwapInt32_x(), out var Ꮡx);
    x.before = magic32;
    x.after = magic32;
    int32 j = default!;
    for (var delta = (int32)1; delta + delta > delta; delta += delta) {
        var k = SwapInt32(Ꮡx.of(TestSwapInt32_x.Ꮡi), delta);
        if (x.i != delta || k != j) {
            Ꮡt.Fatalf("delta=%d i=%d j=%d k=%d"u8, delta, x.i, j, k);
        }
        j = delta;
    }
    if (x.before != magic32 || x.after != magic32) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, (nint)(magic32), (nint)(magic32));
    }
}

[GoType("dyn")] internal partial struct TestSwapInt32Method_x {
    internal int32 before;
    internal atomic.Int32 i;
    internal int32 after;
}

public static void TestSwapInt32Method(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestSwapInt32Method_x(), out var Ꮡx);
    x.before = magic32;
    x.after = magic32;
    int32 j = default!;
    for (var delta = (int32)1; delta + delta > delta; delta += delta) {
        var k = Ꮡx.of(TestSwapInt32Method_x.Ꮡi).Swap(delta);
        if (Ꮡx.of(TestSwapInt32Method_x.Ꮡi).Load() != delta || k != j) {
            Ꮡt.Fatalf("delta=%d i=%d j=%d k=%d"u8, delta, Ꮡx.of(TestSwapInt32Method_x.Ꮡi).Load(), j, k);
        }
        j = delta;
    }
    if (x.before != magic32 || x.after != magic32) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, (nint)(magic32), (nint)(magic32));
    }
}

[GoType("dyn")] internal partial struct TestSwapUint32_x {
    internal uint32 before;
    internal uint32 i;
    internal uint32 after;
}

public static void TestSwapUint32(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestSwapUint32_x(), out var Ꮡx);
    x.before = magic32;
    x.after = magic32;
    uint32 j = default!;
    for (var delta = (uint32)1; delta + delta > delta; delta += delta) {
        var k = SwapUint32(Ꮡx.of(TestSwapUint32_x.Ꮡi), delta);
        if (x.i != delta || k != j) {
            Ꮡt.Fatalf("delta=%d i=%d j=%d k=%d"u8, delta, x.i, j, k);
        }
        j = delta;
    }
    if (x.before != magic32 || x.after != magic32) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, (nint)(magic32), (nint)(magic32));
    }
}

[GoType("dyn")] internal partial struct TestSwapUint32Method_x {
    internal uint32 before;
    internal atomic.Uint32 i;
    internal uint32 after;
}

public static void TestSwapUint32Method(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestSwapUint32Method_x(), out var Ꮡx);
    x.before = magic32;
    x.after = magic32;
    uint32 j = default!;
    for (var delta = (uint32)1; delta + delta > delta; delta += delta) {
        var k = Ꮡx.of(TestSwapUint32Method_x.Ꮡi).Swap(delta);
        if (Ꮡx.of(TestSwapUint32Method_x.Ꮡi).Load() != delta || k != j) {
            Ꮡt.Fatalf("delta=%d i=%d j=%d k=%d"u8, delta, Ꮡx.of(TestSwapUint32Method_x.Ꮡi).Load(), j, k);
        }
        j = delta;
    }
    if (x.before != magic32 || x.after != magic32) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, (nint)(magic32), (nint)(magic32));
    }
}

[GoType("dyn")] internal partial struct TestSwapInt64_x {
    internal int64 before;
    internal int64 i;
    internal int64 after;
}

public static void TestSwapInt64(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestSwapInt64_x(), out var Ꮡx);
    var magic64 = (int64)atomic_test_package.magic64;
    x.before = magic64;
    x.after = magic64;
    int64 j = default!;
    for (var delta = (int64)1; delta + delta > delta; delta += delta) {
        var k = SwapInt64(Ꮡx.of(TestSwapInt64_x.Ꮡi), delta);
        if (x.i != delta || k != j) {
            Ꮡt.Fatalf("delta=%d i=%d j=%d k=%d"u8, delta, x.i, j, k);
        }
        j = delta;
    }
    if (x.before != magic64 || x.after != magic64) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, magic64, magic64);
    }
}

[GoType("dyn")] internal partial struct TestSwapInt64Method_x {
    internal int64 before;
    internal atomic.Int64 i;
    internal int64 after;
}

public static void TestSwapInt64Method(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestSwapInt64Method_x(), out var Ꮡx);
    var magic64 = (int64)atomic_test_package.magic64;
    x.before = magic64;
    x.after = magic64;
    int64 j = default!;
    for (var delta = (int64)1; delta + delta > delta; delta += delta) {
        var k = Ꮡx.of(TestSwapInt64Method_x.Ꮡi).Swap(delta);
        if (Ꮡx.of(TestSwapInt64Method_x.Ꮡi).Load() != delta || k != j) {
            Ꮡt.Fatalf("delta=%d i=%d j=%d k=%d"u8, delta, Ꮡx.of(TestSwapInt64Method_x.Ꮡi).Load(), j, k);
        }
        j = delta;
    }
    if (x.before != magic64 || x.after != magic64) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, magic64, magic64);
    }
}

[GoType("dyn")] internal partial struct TestSwapUint64_x {
    internal uint64 before;
    internal uint64 i;
    internal uint64 after;
}

public static void TestSwapUint64(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestSwapUint64_x(), out var Ꮡx);
    var magic64 = (uint64)atomic_test_package.magic64;
    x.before = magic64;
    x.after = magic64;
    uint64 j = default!;
    for (var delta = (uint64)1; delta + delta > delta; delta += delta) {
        var k = SwapUint64(Ꮡx.of(TestSwapUint64_x.Ꮡi), delta);
        if (x.i != delta || k != j) {
            Ꮡt.Fatalf("delta=%d i=%d j=%d k=%d"u8, delta, x.i, j, k);
        }
        j = delta;
    }
    if (x.before != magic64 || x.after != magic64) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, magic64, magic64);
    }
}

[GoType("dyn")] internal partial struct TestSwapUint64Method_x {
    internal uint64 before;
    internal atomic.Uint64 i;
    internal uint64 after;
}

public static void TestSwapUint64Method(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestSwapUint64Method_x(), out var Ꮡx);
    var magic64 = (uint64)atomic_test_package.magic64;
    x.before = magic64;
    x.after = magic64;
    uint64 j = default!;
    for (var delta = (uint64)1; delta + delta > delta; delta += delta) {
        var k = Ꮡx.of(TestSwapUint64Method_x.Ꮡi).Swap(delta);
        if (Ꮡx.of(TestSwapUint64Method_x.Ꮡi).Load() != delta || k != j) {
            Ꮡt.Fatalf("delta=%d i=%d j=%d k=%d"u8, delta, Ꮡx.of(TestSwapUint64Method_x.Ꮡi).Load(), j, k);
        }
        j = delta;
    }
    if (x.before != magic64 || x.after != magic64) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, magic64, magic64);
    }
}

[GoType("dyn")] internal partial struct TestSwapUintptr_x {
    internal uintptr before;
    internal uintptr i;
    internal uintptr after;
}

public static void TestSwapUintptr(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestSwapUintptr_x(), out var Ꮡx);
    uint64 m = magic64;
    var magicptr = (uintptr)m;
    x.before = magicptr;
    x.after = magicptr;
    uintptr j = default!;
    for (var delta = (uintptr)1; delta + delta > delta; delta += delta) {
        var k = SwapUintptr(Ꮡx.of(TestSwapUintptr_x.Ꮡi), delta);
        if (x.i != delta || k != j) {
            Ꮡt.Fatalf("delta=%d i=%d j=%d k=%d"u8, delta, x.i, j, k);
        }
        j = delta;
    }
    if (x.before != magicptr || x.after != magicptr) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, magicptr, magicptr);
    }
}

[GoType("dyn")] internal partial struct TestSwapUintptrMethod_x {
    internal uintptr before;
    internal atomic.Uintptr i;
    internal uintptr after;
}

public static void TestSwapUintptrMethod(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestSwapUintptrMethod_x(), out var Ꮡx);
    uint64 m = magic64;
    var magicptr = (uintptr)m;
    x.before = magicptr;
    x.after = magicptr;
    uintptr j = default!;
    for (var delta = (uintptr)1; delta + delta > delta; delta += delta) {
        var k = Ꮡx.of(TestSwapUintptrMethod_x.Ꮡi).Swap(delta);
        if (Ꮡx.of(TestSwapUintptrMethod_x.Ꮡi).Load() != delta || k != j) {
            Ꮡt.Fatalf("delta=%d i=%d j=%d k=%d"u8, delta, Ꮡx.of(TestSwapUintptrMethod_x.Ꮡi).Load(), j, k);
        }
        j = delta;
    }
    if (x.before != magicptr || x.after != magicptr) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, magicptr, magicptr);
    }
}

internal static ж<array<byte>> Ꮡglobal = new StandardBox<array<byte>>(new array<byte>(1024));
internal static ref array<byte> global => ref Ꮡglobal.Value;

internal static slice<@unsafe.Pointer> testPointers() {
    slice<@unsafe.Pointer> pointers = default!;
    // globals
    for (nint i = 0; i < 10; i++) {
        pointers = append(pointers, @unsafe.Pointer.FromPinnedBox(Ꮡglobal.at<byte>(((nint)1).Lsh((int64)(i)) - 1)));
    }
    // heap
    pointers = append(pointers, @unsafe.Pointer.FromPinnedBox(@new<byte>()));
    // nil
    pointers = append(pointers, (@unsafe.Pointer)(nil));
    return pointers;
}

[GoType("dyn")] internal partial struct TestSwapPointer_x {
    internal uintptr before;
    internal @unsafe.Pointer i;
    internal uintptr after;
}

public static void TestSwapPointer(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestSwapPointer_x(), out var Ꮡx);
    uint64 m = magic64;
    var magicptr = (uintptr)m;
    x.before = magicptr;
    x.after = magicptr;
    @unsafe.Pointer j = default!;
    foreach (var (_, p) in testPointers()) {
        @unsafe.Pointer k = (uintptr)SwapPointer(Ꮡx.of(TestSwapPointer_x.Ꮡi), p);
        if (x.i != p || k != j) {
            Ꮡt.Fatalf("p=%p i=%p j=%p k=%p"u8, @unsafe.Pointer.OrTypedNil(p), @unsafe.Pointer.OrTypedNil(x.i), @unsafe.Pointer.OrTypedNil(j), @unsafe.Pointer.OrTypedNil(k));
        }
        j = p;
    }
    if (x.before != magicptr || x.after != magicptr) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, magicptr, magicptr);
    }
}

[GoType("dyn")] internal partial struct TestSwapPointerMethod_x {
    internal uintptr before;
    internal atomic.Pointer<byte> i;
    internal uintptr after;
}

public static void TestSwapPointerMethod(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestSwapPointerMethod_x(), out var Ꮡx);
    uint64 m = magic64;
    var magicptr = (uintptr)m;
    x.before = magicptr;
    x.after = magicptr;
    ж<byte> j = default!;
    foreach (var (_, p) in testPointers()) {
        var pΔ1 = (ж<byte>)(uintptr)(p);
        var k = Ꮡx.of(TestSwapPointerMethod_x.Ꮡi).Swap(pΔ1);
        if (Ꮡx.of(TestSwapPointerMethod_x.Ꮡi).Load() != pΔ1 || k != j) {
            Ꮡt.Fatalf("p=%p i=%p j=%p k=%p"u8, pΔ1.OrTypedNil(), Ꮡx.of(TestSwapPointerMethod_x.Ꮡi).Load().OrTypedNil(), j.OrTypedNil(), k.OrTypedNil());
        }
        j = pΔ1;
    }
    if (x.before != magicptr || x.after != magicptr) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, magicptr, magicptr);
    }
}

[GoType("dyn")] internal partial struct TestAddInt32_x {
    internal int32 before;
    internal int32 i;
    internal int32 after;
}

public static void TestAddInt32(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestAddInt32_x(), out var Ꮡx);
    x.before = magic32;
    x.after = magic32;
    int32 j = default!;
    for (var delta = (int32)1; delta + delta > delta; delta += delta) {
        var k = AddInt32(Ꮡx.of(TestAddInt32_x.Ꮡi), delta);
        j += delta;
        if (x.i != j || k != j) {
            Ꮡt.Fatalf("delta=%d i=%d j=%d k=%d"u8, delta, x.i, j, k);
        }
    }
    if (x.before != magic32 || x.after != magic32) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, (nint)(magic32), (nint)(magic32));
    }
}

[GoType("dyn")] internal partial struct TestAddInt32Method_x {
    internal int32 before;
    internal atomic.Int32 i;
    internal int32 after;
}

public static void TestAddInt32Method(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestAddInt32Method_x(), out var Ꮡx);
    x.before = magic32;
    x.after = magic32;
    int32 j = default!;
    for (var delta = (int32)1; delta + delta > delta; delta += delta) {
        var k = Ꮡx.of(TestAddInt32Method_x.Ꮡi).Add(delta);
        j += delta;
        if (Ꮡx.of(TestAddInt32Method_x.Ꮡi).Load() != j || k != j) {
            Ꮡt.Fatalf("delta=%d i=%d j=%d k=%d"u8, delta, Ꮡx.of(TestAddInt32Method_x.Ꮡi).Load(), j, k);
        }
    }
    if (x.before != magic32 || x.after != magic32) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, (nint)(magic32), (nint)(magic32));
    }
}

[GoType("dyn")] internal partial struct TestAddUint32_x {
    internal uint32 before;
    internal uint32 i;
    internal uint32 after;
}

public static void TestAddUint32(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestAddUint32_x(), out var Ꮡx);
    x.before = magic32;
    x.after = magic32;
    uint32 j = default!;
    for (var delta = (uint32)1; delta + delta > delta; delta += delta) {
        var k = AddUint32(Ꮡx.of(TestAddUint32_x.Ꮡi), delta);
        j += delta;
        if (x.i != j || k != j) {
            Ꮡt.Fatalf("delta=%d i=%d j=%d k=%d"u8, delta, x.i, j, k);
        }
    }
    if (x.before != magic32 || x.after != magic32) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, (nint)(magic32), (nint)(magic32));
    }
}

[GoType("dyn")] internal partial struct TestAddUint32Method_x {
    internal uint32 before;
    internal atomic.Uint32 i;
    internal uint32 after;
}

public static void TestAddUint32Method(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestAddUint32Method_x(), out var Ꮡx);
    x.before = magic32;
    x.after = magic32;
    uint32 j = default!;
    for (var delta = (uint32)1; delta + delta > delta; delta += delta) {
        var k = Ꮡx.of(TestAddUint32Method_x.Ꮡi).Add(delta);
        j += delta;
        if (Ꮡx.of(TestAddUint32Method_x.Ꮡi).Load() != j || k != j) {
            Ꮡt.Fatalf("delta=%d i=%d j=%d k=%d"u8, delta, Ꮡx.of(TestAddUint32Method_x.Ꮡi).Load(), j, k);
        }
    }
    if (x.before != magic32 || x.after != magic32) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, (nint)(magic32), (nint)(magic32));
    }
}

[GoType("dyn")] internal partial struct TestAddInt64_x {
    internal int64 before;
    internal int64 i;
    internal int64 after;
}

public static void TestAddInt64(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestAddInt64_x(), out var Ꮡx);
    var magic64 = (int64)atomic_test_package.magic64;
    x.before = magic64;
    x.after = magic64;
    int64 j = default!;
    for (var delta = (int64)1; delta + delta > delta; delta += delta) {
        var k = AddInt64(Ꮡx.of(TestAddInt64_x.Ꮡi), delta);
        j += delta;
        if (x.i != j || k != j) {
            Ꮡt.Fatalf("delta=%d i=%d j=%d k=%d"u8, delta, x.i, j, k);
        }
    }
    if (x.before != magic64 || x.after != magic64) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, magic64, magic64);
    }
}

[GoType("dyn")] internal partial struct TestAddInt64Method_x {
    internal int64 before;
    internal atomic.Int64 i;
    internal int64 after;
}

public static void TestAddInt64Method(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestAddInt64Method_x(), out var Ꮡx);
    var magic64 = (int64)atomic_test_package.magic64;
    x.before = magic64;
    x.after = magic64;
    int64 j = default!;
    for (var delta = (int64)1; delta + delta > delta; delta += delta) {
        var k = Ꮡx.of(TestAddInt64Method_x.Ꮡi).Add(delta);
        j += delta;
        if (Ꮡx.of(TestAddInt64Method_x.Ꮡi).Load() != j || k != j) {
            Ꮡt.Fatalf("delta=%d i=%d j=%d k=%d"u8, delta, Ꮡx.of(TestAddInt64Method_x.Ꮡi).Load(), j, k);
        }
    }
    if (x.before != magic64 || x.after != magic64) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, magic64, magic64);
    }
}

[GoType("dyn")] internal partial struct TestAddUint64_x {
    internal uint64 before;
    internal uint64 i;
    internal uint64 after;
}

public static void TestAddUint64(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestAddUint64_x(), out var Ꮡx);
    var magic64 = (uint64)atomic_test_package.magic64;
    x.before = magic64;
    x.after = magic64;
    uint64 j = default!;
    for (var delta = (uint64)1; delta + delta > delta; delta += delta) {
        var k = AddUint64(Ꮡx.of(TestAddUint64_x.Ꮡi), delta);
        j += delta;
        if (x.i != j || k != j) {
            Ꮡt.Fatalf("delta=%d i=%d j=%d k=%d"u8, delta, x.i, j, k);
        }
    }
    if (x.before != magic64 || x.after != magic64) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, magic64, magic64);
    }
}

[GoType("dyn")] internal partial struct TestAddUint64Method_x {
    internal uint64 before;
    internal atomic.Uint64 i;
    internal uint64 after;
}

public static void TestAddUint64Method(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestAddUint64Method_x(), out var Ꮡx);
    var magic64 = (uint64)atomic_test_package.magic64;
    x.before = magic64;
    x.after = magic64;
    uint64 j = default!;
    for (var delta = (uint64)1; delta + delta > delta; delta += delta) {
        var k = Ꮡx.of(TestAddUint64Method_x.Ꮡi).Add(delta);
        j += delta;
        if (Ꮡx.of(TestAddUint64Method_x.Ꮡi).Load() != j || k != j) {
            Ꮡt.Fatalf("delta=%d i=%d j=%d k=%d"u8, delta, Ꮡx.of(TestAddUint64Method_x.Ꮡi).Load(), j, k);
        }
    }
    if (x.before != magic64 || x.after != magic64) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, magic64, magic64);
    }
}

[GoType("dyn")] internal partial struct TestAddUintptr_x {
    internal uintptr before;
    internal uintptr i;
    internal uintptr after;
}

public static void TestAddUintptr(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestAddUintptr_x(), out var Ꮡx);
    uint64 m = magic64;
    var magicptr = (uintptr)m;
    x.before = magicptr;
    x.after = magicptr;
    uintptr j = default!;
    for (var delta = (uintptr)1; delta + delta > delta; delta += delta) {
        var k = AddUintptr(Ꮡx.of(TestAddUintptr_x.Ꮡi), delta);
        j += delta;
        if (x.i != j || k != j) {
            Ꮡt.Fatalf("delta=%d i=%d j=%d k=%d"u8, delta, x.i, j, k);
        }
    }
    if (x.before != magicptr || x.after != magicptr) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, magicptr, magicptr);
    }
}

[GoType("dyn")] internal partial struct TestAddUintptrMethod_x {
    internal uintptr before;
    internal atomic.Uintptr i;
    internal uintptr after;
}

public static void TestAddUintptrMethod(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestAddUintptrMethod_x(), out var Ꮡx);
    uint64 m = magic64;
    var magicptr = (uintptr)m;
    x.before = magicptr;
    x.after = magicptr;
    uintptr j = default!;
    for (var delta = (uintptr)1; delta + delta > delta; delta += delta) {
        var k = Ꮡx.of(TestAddUintptrMethod_x.Ꮡi).Add(delta);
        j += delta;
        if (Ꮡx.of(TestAddUintptrMethod_x.Ꮡi).Load() != j || k != j) {
            Ꮡt.Fatalf("delta=%d i=%d j=%d k=%d"u8, delta, Ꮡx.of(TestAddUintptrMethod_x.Ꮡi).Load(), j, k);
        }
    }
    if (x.before != magicptr || x.after != magicptr) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, magicptr, magicptr);
    }
}

[GoType("dyn")] internal partial struct TestAndInt32_x {
    internal int32 before;
    internal int32 i;
    internal int32 after;
}

public static void TestAndInt32(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestAndInt32_x(), out var Ꮡx);
    x.before = magic32;
    x.after = magic32;
    x.i = -1;
    var j = x.i;
    for (var mask = (int32)1; mask != 0; mask <<= (int)(1)) {
        var old = x.i;
        var k = AndInt32(Ꮡx.of(TestAndInt32_x.Ꮡi), ~mask);
        j &= (int32)(~mask);
        if (x.i != j || k != old) {
            Ꮡt.Fatalf("mask=%d i=%d j=%d k=%d old=%d"u8, mask, x.i, j, k, old);
        }
    }
    if (x.before != magic32 || x.after != magic32) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, (nint)(magic32), (nint)(magic32));
    }
}

[GoType("dyn")] internal partial struct TestAndInt32Method_x {
    internal int32 before;
    internal atomic.Int32 i;
    internal int32 after;
}

public static void TestAndInt32Method(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestAndInt32Method_x(), out var Ꮡx);
    x.before = magic32;
    x.after = magic32;
    Ꮡx.of(TestAndInt32Method_x.Ꮡi).Store(-1);
    var j = Ꮡx.of(TestAndInt32Method_x.Ꮡi).Load();
    for (var mask = (int32)1; mask != 0; mask <<= (int)(1)) {
        var old = Ꮡx.of(TestAndInt32Method_x.Ꮡi).Load();
        var k = Ꮡx.of(TestAndInt32Method_x.Ꮡi).And(~mask);
        j &= (int32)(~mask);
        if (Ꮡx.of(TestAndInt32Method_x.Ꮡi).Load() != j || k != old) {
            Ꮡt.Fatalf("mask=%d i=%d j=%d k=%d old=%d"u8, mask, Ꮡx.of(TestAndInt32Method_x.Ꮡi).Load(), j, k, old);
        }
    }
    if (x.before != magic32 || x.after != magic32) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, (nint)(magic32), (nint)(magic32));
    }
}

[GoType("dyn")] internal partial struct TestAndUint32_x {
    internal uint32 before;
    internal uint32 i;
    internal uint32 after;
}

public static void TestAndUint32(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestAndUint32_x(), out var Ꮡx);
    x.before = magic32;
    x.after = magic32;
    x.i = 0xffffffffU;
    var j = x.i;
    for (var mask = (uint32)1; mask != 0; mask <<= (int)(1)) {
        var old = x.i;
        var k = AndUint32(Ꮡx.of(TestAndUint32_x.Ꮡi), ~mask);
        j &= (uint32)(~mask);
        if (x.i != j || k != old) {
            Ꮡt.Fatalf("mask=%d i=%d j=%d k=%d old=%d"u8, mask, x.i, j, k, old);
        }
    }
    if (x.before != magic32 || x.after != magic32) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, (nint)(magic32), (nint)(magic32));
    }
}

[GoType("dyn")] internal partial struct TestAndUint32Method_x {
    internal uint32 before;
    internal atomic.Uint32 i;
    internal uint32 after;
}

public static void TestAndUint32Method(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestAndUint32Method_x(), out var Ꮡx);
    x.before = magic32;
    x.after = magic32;
    Ꮡx.of(TestAndUint32Method_x.Ꮡi).Store(0xffffffffU);
    var j = Ꮡx.of(TestAndUint32Method_x.Ꮡi).Load();
    for (var mask = (uint32)1; mask != 0; mask <<= (int)(1)) {
        var old = Ꮡx.of(TestAndUint32Method_x.Ꮡi).Load();
        var k = Ꮡx.of(TestAndUint32Method_x.Ꮡi).And(~mask);
        j &= (uint32)(~mask);
        if (Ꮡx.of(TestAndUint32Method_x.Ꮡi).Load() != j || k != old) {
            Ꮡt.Fatalf("mask=%d i=%d j=%d k=%d old=%d"u8, mask, Ꮡx.of(TestAndUint32Method_x.Ꮡi).Load(), j, k, old);
        }
    }
    if (x.before != magic32 || x.after != magic32) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, (nint)(magic32), (nint)(magic32));
    }
}

[GoType("dyn")] internal partial struct TestAndInt64_x {
    internal int64 before;
    internal int64 i;
    internal int64 after;
}

public static void TestAndInt64(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestAndInt64_x(), out var Ꮡx);
    var magic64 = (int64)atomic_test_package.magic64;
    x.before = magic64;
    x.after = magic64;
    x.i = -1;
    var j = x.i;
    for (var mask = (int64)1; mask != 0; mask <<= (int)(1)) {
        var old = x.i;
        var k = AndInt64(Ꮡx.of(TestAndInt64_x.Ꮡi), ~mask);
        j &= (int64)(~mask);
        if (x.i != j || k != old) {
            Ꮡt.Fatalf("mask=%d i=%d j=%d k=%d old=%d"u8, mask, x.i, j, k, old);
        }
    }
    if (x.before != magic64 || x.after != magic64) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, magic64, magic64);
    }
}

[GoType("dyn")] internal partial struct TestAndInt64Method_x {
    internal int64 before;
    internal atomic.Int64 i;
    internal int64 after;
}

public static void TestAndInt64Method(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestAndInt64Method_x(), out var Ꮡx);
    var magic64 = (int64)atomic_test_package.magic64;
    x.before = magic64;
    x.after = magic64;
    Ꮡx.of(TestAndInt64Method_x.Ꮡi).Store(-1);
    var j = Ꮡx.of(TestAndInt64Method_x.Ꮡi).Load();
    for (var mask = (int64)1; mask != 0; mask <<= (int)(1)) {
        var old = Ꮡx.of(TestAndInt64Method_x.Ꮡi).Load();
        var k = Ꮡx.of(TestAndInt64Method_x.Ꮡi).And(~mask);
        j &= (int64)(~mask);
        if (Ꮡx.of(TestAndInt64Method_x.Ꮡi).Load() != j || k != old) {
            Ꮡt.Fatalf("mask=%d i=%d j=%d k=%d old=%d"u8, mask, Ꮡx.of(TestAndInt64Method_x.Ꮡi).Load(), j, k, old);
        }
    }
    if (x.before != magic64 || x.after != magic64) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, magic64, magic64);
    }
}

[GoType("dyn")] internal partial struct TestAndUint64_x {
    internal uint64 before;
    internal uint64 i;
    internal uint64 after;
}

public static void TestAndUint64(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestAndUint64_x(), out var Ꮡx);
    var magic64 = (uint64)atomic_test_package.magic64;
    x.before = magic64;
    x.after = magic64;
    x.i = 0xfffffffffffffffUL;
    var j = x.i;
    for (var mask = (uint64)1; mask != 0; mask <<= (int)(1)) {
        var old = x.i;
        var k = AndUint64(Ꮡx.of(TestAndUint64_x.Ꮡi), ~mask);
        j &= (uint64)(~mask);
        if (x.i != j || k != old) {
            Ꮡt.Fatalf("mask=%d i=%d j=%d k=%d old=%d"u8, mask, x.i, j, k, old);
        }
    }
    if (x.before != magic64 || x.after != magic64) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, magic64, magic64);
    }
}

[GoType("dyn")] internal partial struct TestAndUint64Method_x {
    internal uint64 before;
    internal atomic.Uint64 i;
    internal uint64 after;
}

public static void TestAndUint64Method(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestAndUint64Method_x(), out var Ꮡx);
    var magic64 = (uint64)atomic_test_package.magic64;
    x.before = magic64;
    x.after = magic64;
    Ꮡx.of(TestAndUint64Method_x.Ꮡi).Store(0xfffffffffffffffUL);
    var j = Ꮡx.of(TestAndUint64Method_x.Ꮡi).Load();
    for (var mask = (uint64)1; mask != 0; mask <<= (int)(1)) {
        var old = Ꮡx.of(TestAndUint64Method_x.Ꮡi).Load();
        var k = Ꮡx.of(TestAndUint64Method_x.Ꮡi).And(~mask);
        j &= (uint64)(~mask);
        if (Ꮡx.of(TestAndUint64Method_x.Ꮡi).Load() != j || k != old) {
            Ꮡt.Fatalf("mask=%d i=%d j=%d k=%d old=%d"u8, mask, Ꮡx.of(TestAndUint64Method_x.Ꮡi).Load(), j, k, old);
        }
    }
    if (x.before != magic64 || x.after != magic64) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, magic64, magic64);
    }
}

[GoType("dyn")] internal partial struct TestAndUintptr_x {
    internal uintptr before;
    internal uintptr i;
    internal uintptr after;
}

public static void TestAndUintptr(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestAndUintptr_x(), out var Ꮡx);
    uint64 m = magic64;
    var magicptr = (uintptr)m;
    x.before = magicptr;
    x.after = magicptr;
    x.i = ~(uintptr)0;
    var j = x.i;
    for (var mask = (uintptr)1; mask != 0; mask <<= (int)(1)) {
        var old = x.i;
        var k = AndUintptr(Ꮡx.of(TestAndUintptr_x.Ꮡi), ~mask);
        j &= (uintptr)(~mask);
        if (x.i != j || k != old) {
            Ꮡt.Fatalf("mask=%d i=%d j=%d k=%d old=%d"u8, mask, x.i, j, k, old);
        }
    }
    if (x.before != magicptr || x.after != magicptr) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, magicptr, magicptr);
    }
}

[GoType("dyn")] internal partial struct TestAndUintptrMethod_x {
    internal uintptr before;
    internal atomic.Uintptr i;
    internal uintptr after;
}

public static void TestAndUintptrMethod(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestAndUintptrMethod_x(), out var Ꮡx);
    uint64 m = magic64;
    var magicptr = (uintptr)m;
    x.before = magicptr;
    x.after = magicptr;
    Ꮡx.of(TestAndUintptrMethod_x.Ꮡi).Store(~(uintptr)0);
    var j = Ꮡx.of(TestAndUintptrMethod_x.Ꮡi).Load();
    for (var mask = (uintptr)1; mask != 0; mask <<= (int)(1)) {
        var old = Ꮡx.of(TestAndUintptrMethod_x.Ꮡi).Load();
        var k = Ꮡx.of(TestAndUintptrMethod_x.Ꮡi).And(~mask);
        j &= (uintptr)(~mask);
        if (Ꮡx.of(TestAndUintptrMethod_x.Ꮡi).Load() != j || k != old) {
            Ꮡt.Fatalf("mask=%d i=%d j=%d k=%d old=%d"u8, mask, Ꮡx.of(TestAndUintptrMethod_x.Ꮡi).Load(), j, k, old);
        }
    }
    if (x.before != magicptr || x.after != magicptr) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, magicptr, magicptr);
    }
}

[GoType("dyn")] internal partial struct TestOrInt32_x {
    internal int32 before;
    internal int32 i;
    internal int32 after;
}

public static void TestOrInt32(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestOrInt32_x(), out var Ꮡx);
    x.before = magic32;
    x.after = magic32;
    int32 j = default!;
    for (var mask = (int32)1; mask != 0; mask <<= (int)(1)) {
        var old = x.i;
        var k = OrInt32(Ꮡx.of(TestOrInt32_x.Ꮡi), mask);
        j |= (int32)(mask);
        if (x.i != j || k != old) {
            Ꮡt.Fatalf("mask=%d i=%d j=%d k=%d old=%d"u8, mask, x.i, j, k, old);
        }
    }
    if (x.before != magic32 || x.after != magic32) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, (nint)(magic32), (nint)(magic32));
    }
}

[GoType("dyn")] internal partial struct TestOrInt32Method_x {
    internal int32 before;
    internal atomic.Int32 i;
    internal int32 after;
}

public static void TestOrInt32Method(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestOrInt32Method_x(), out var Ꮡx);
    x.before = magic32;
    x.after = magic32;
    int32 j = default!;
    for (var mask = (int32)1; mask != 0; mask <<= (int)(1)) {
        var old = Ꮡx.of(TestOrInt32Method_x.Ꮡi).Load();
        var k = Ꮡx.of(TestOrInt32Method_x.Ꮡi).Or(mask);
        j |= (int32)(mask);
        if (Ꮡx.of(TestOrInt32Method_x.Ꮡi).Load() != j || k != old) {
            Ꮡt.Fatalf("mask=%d i=%d j=%d k=%d old=%d"u8, mask, Ꮡx.of(TestOrInt32Method_x.Ꮡi).Load(), j, k, old);
        }
    }
    if (x.before != magic32 || x.after != magic32) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, (nint)(magic32), (nint)(magic32));
    }
}

[GoType("dyn")] internal partial struct TestOrUint32_x {
    internal uint32 before;
    internal uint32 i;
    internal uint32 after;
}

public static void TestOrUint32(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestOrUint32_x(), out var Ꮡx);
    x.before = magic32;
    x.after = magic32;
    uint32 j = default!;
    for (var mask = (uint32)1; mask != 0; mask <<= (int)(1)) {
        var old = x.i;
        var k = OrUint32(Ꮡx.of(TestOrUint32_x.Ꮡi), mask);
        j |= (uint32)(mask);
        if (x.i != j || k != old) {
            Ꮡt.Fatalf("mask=%d i=%d j=%d k=%d old=%d"u8, mask, x.i, j, k, old);
        }
    }
    if (x.before != magic32 || x.after != magic32) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, (nint)(magic32), (nint)(magic32));
    }
}

[GoType("dyn")] internal partial struct TestOrUint32Method_x {
    internal uint32 before;
    internal atomic.Uint32 i;
    internal uint32 after;
}

public static void TestOrUint32Method(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestOrUint32Method_x(), out var Ꮡx);
    x.before = magic32;
    x.after = magic32;
    uint32 j = default!;
    for (var mask = (uint32)1; mask != 0; mask <<= (int)(1)) {
        var old = Ꮡx.of(TestOrUint32Method_x.Ꮡi).Load();
        var k = Ꮡx.of(TestOrUint32Method_x.Ꮡi).Or(mask);
        j |= (uint32)(mask);
        if (Ꮡx.of(TestOrUint32Method_x.Ꮡi).Load() != j || k != old) {
            Ꮡt.Fatalf("mask=%d i=%d j=%d k=%d old=%d"u8, mask, Ꮡx.of(TestOrUint32Method_x.Ꮡi).Load(), j, k, old);
        }
    }
    if (x.before != magic32 || x.after != magic32) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, (nint)(magic32), (nint)(magic32));
    }
}

[GoType("dyn")] internal partial struct TestOrInt64_x {
    internal int64 before;
    internal int64 i;
    internal int64 after;
}

public static void TestOrInt64(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestOrInt64_x(), out var Ꮡx);
    var magic64 = (int64)atomic_test_package.magic64;
    x.before = magic64;
    x.after = magic64;
    int64 j = default!;
    for (var mask = (int64)1; mask != 0; mask <<= (int)(1)) {
        var old = x.i;
        var k = OrInt64(Ꮡx.of(TestOrInt64_x.Ꮡi), mask);
        j |= (int64)(mask);
        if (x.i != j || k != old) {
            Ꮡt.Fatalf("mask=%d i=%d j=%d k=%d old=%d"u8, mask, x.i, j, k, old);
        }
    }
    if (x.before != magic64 || x.after != magic64) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, magic64, magic64);
    }
}

[GoType("dyn")] internal partial struct TestOrInt64Method_x {
    internal int64 before;
    internal atomic.Int64 i;
    internal int64 after;
}

public static void TestOrInt64Method(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestOrInt64Method_x(), out var Ꮡx);
    var magic64 = (int64)atomic_test_package.magic64;
    x.before = magic64;
    x.after = magic64;
    int64 j = default!;
    for (var mask = (int64)1; mask != 0; mask <<= (int)(1)) {
        var old = Ꮡx.of(TestOrInt64Method_x.Ꮡi).Load();
        var k = Ꮡx.of(TestOrInt64Method_x.Ꮡi).Or(mask);
        j |= (int64)(mask);
        if (Ꮡx.of(TestOrInt64Method_x.Ꮡi).Load() != j || k != old) {
            Ꮡt.Fatalf("mask=%d i=%d j=%d k=%d old=%d"u8, mask, Ꮡx.of(TestOrInt64Method_x.Ꮡi).Load(), j, k, old);
        }
    }
    if (x.before != magic64 || x.after != magic64) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, magic64, magic64);
    }
}

[GoType("dyn")] internal partial struct TestOrUint64_x {
    internal uint64 before;
    internal uint64 i;
    internal uint64 after;
}

public static void TestOrUint64(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestOrUint64_x(), out var Ꮡx);
    var magic64 = (uint64)atomic_test_package.magic64;
    x.before = magic64;
    x.after = magic64;
    uint64 j = default!;
    for (var mask = (uint64)1; mask != 0; mask <<= (int)(1)) {
        var old = x.i;
        var k = OrUint64(Ꮡx.of(TestOrUint64_x.Ꮡi), mask);
        j |= (uint64)(mask);
        if (x.i != j || k != old) {
            Ꮡt.Fatalf("mask=%d i=%d j=%d k=%d old=%d"u8, mask, x.i, j, k, old);
        }
    }
    if (x.before != magic64 || x.after != magic64) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, magic64, magic64);
    }
}

[GoType("dyn")] internal partial struct TestOrUint64Method_x {
    internal uint64 before;
    internal atomic.Uint64 i;
    internal uint64 after;
}

public static void TestOrUint64Method(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestOrUint64Method_x(), out var Ꮡx);
    var magic64 = (uint64)atomic_test_package.magic64;
    x.before = magic64;
    x.after = magic64;
    uint64 j = default!;
    for (var mask = (uint64)1; mask != 0; mask <<= (int)(1)) {
        var old = Ꮡx.of(TestOrUint64Method_x.Ꮡi).Load();
        var k = Ꮡx.of(TestOrUint64Method_x.Ꮡi).Or(mask);
        j |= (uint64)(mask);
        if (Ꮡx.of(TestOrUint64Method_x.Ꮡi).Load() != j || k != old) {
            Ꮡt.Fatalf("mask=%d i=%d j=%d k=%d old=%d"u8, mask, Ꮡx.of(TestOrUint64Method_x.Ꮡi).Load(), j, k, old);
        }
    }
    if (x.before != magic64 || x.after != magic64) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, magic64, magic64);
    }
}

[GoType("dyn")] internal partial struct TestOrUintptr_x {
    internal uintptr before;
    internal uintptr i;
    internal uintptr after;
}

public static void TestOrUintptr(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestOrUintptr_x(), out var Ꮡx);
    uint64 m = magic64;
    var magicptr = (uintptr)m;
    x.before = magicptr;
    x.after = magicptr;
    uintptr j = default!;
    for (var mask = (uintptr)1; mask != 0; mask <<= (int)(1)) {
        var old = x.i;
        var k = OrUintptr(Ꮡx.of(TestOrUintptr_x.Ꮡi), mask);
        j |= (uintptr)(mask);
        if (x.i != j || k != old) {
            Ꮡt.Fatalf("mask=%d i=%d j=%d k=%d old=%d"u8, mask, x.i, j, k, old);
        }
    }
    if (x.before != magicptr || x.after != magicptr) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, magicptr, magicptr);
    }
}

[GoType("dyn")] internal partial struct TestOrUintptrMethod_x {
    internal uintptr before;
    internal atomic.Uintptr i;
    internal uintptr after;
}

public static void TestOrUintptrMethod(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestOrUintptrMethod_x(), out var Ꮡx);
    uint64 m = magic64;
    var magicptr = (uintptr)m;
    x.before = magicptr;
    x.after = magicptr;
    uintptr j = default!;
    for (var mask = (uintptr)1; mask != 0; mask <<= (int)(1)) {
        var old = Ꮡx.of(TestOrUintptrMethod_x.Ꮡi).Load();
        var k = Ꮡx.of(TestOrUintptrMethod_x.Ꮡi).Or(mask);
        j |= (uintptr)(mask);
        if (Ꮡx.of(TestOrUintptrMethod_x.Ꮡi).Load() != j || k != old) {
            Ꮡt.Fatalf("mask=%d i=%d j=%d k=%d old=%d"u8, mask, Ꮡx.of(TestOrUintptrMethod_x.Ꮡi).Load(), j, k, old);
        }
    }
    if (x.before != magicptr || x.after != magicptr) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, magicptr, magicptr);
    }
}

[GoType("dyn")] internal partial struct TestCompareAndSwapInt32_x {
    internal int32 before;
    internal int32 i;
    internal int32 after;
}

public static void TestCompareAndSwapInt32(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestCompareAndSwapInt32_x(), out var Ꮡx);
    x.before = magic32;
    x.after = magic32;
    for (var val = (int32)1; val + val > val; val += val) {
        x.i = val;
        if (!CompareAndSwapInt32(Ꮡx.of(TestCompareAndSwapInt32_x.Ꮡi), val, val + 1)) {
            Ꮡt.Fatalf("should have swapped %#x %#x"u8, val, val + 1);
        }
        if (x.i != val + 1) {
            Ꮡt.Fatalf("wrong x.i after swap: x.i=%#x val+1=%#x"u8, x.i, val + 1);
        }
        x.i = val + 1;
        if (CompareAndSwapInt32(Ꮡx.of(TestCompareAndSwapInt32_x.Ꮡi), val, val + 2)) {
            Ꮡt.Fatalf("should not have swapped %#x %#x"u8, val, val + 2);
        }
        if (x.i != val + 1) {
            Ꮡt.Fatalf("wrong x.i after swap: x.i=%#x val+1=%#x"u8, x.i, val + 1);
        }
    }
    if (x.before != magic32 || x.after != magic32) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, (nint)(magic32), (nint)(magic32));
    }
}

[GoType("dyn")] internal partial struct TestCompareAndSwapInt32Method_x {
    internal int32 before;
    internal atomic.Int32 i;
    internal int32 after;
}

public static void TestCompareAndSwapInt32Method(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestCompareAndSwapInt32Method_x(), out var Ꮡx);
    x.before = magic32;
    x.after = magic32;
    for (var val = (int32)1; val + val > val; val += val) {
        Ꮡx.of(TestCompareAndSwapInt32Method_x.Ꮡi).Store(val);
        if (!Ꮡx.of(TestCompareAndSwapInt32Method_x.Ꮡi).CompareAndSwap(val, val + 1)) {
            Ꮡt.Fatalf("should have swapped %#x %#x"u8, val, val + 1);
        }
        if (Ꮡx.of(TestCompareAndSwapInt32Method_x.Ꮡi).Load() != val + 1) {
            Ꮡt.Fatalf("wrong x.i after swap: x.i=%#x val+1=%#x"u8, Ꮡx.of(TestCompareAndSwapInt32Method_x.Ꮡi).Load(), val + 1);
        }
        Ꮡx.of(TestCompareAndSwapInt32Method_x.Ꮡi).Store(val + 1);
        if (Ꮡx.of(TestCompareAndSwapInt32Method_x.Ꮡi).CompareAndSwap(val, val + 2)) {
            Ꮡt.Fatalf("should not have swapped %#x %#x"u8, val, val + 2);
        }
        if (Ꮡx.of(TestCompareAndSwapInt32Method_x.Ꮡi).Load() != val + 1) {
            Ꮡt.Fatalf("wrong x.i after swap: x.i=%#x val+1=%#x"u8, Ꮡx.of(TestCompareAndSwapInt32Method_x.Ꮡi).Load(), val + 1);
        }
    }
    if (x.before != magic32 || x.after != magic32) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, (nint)(magic32), (nint)(magic32));
    }
}

[GoType("dyn")] internal partial struct TestCompareAndSwapUint32_x {
    internal uint32 before;
    internal uint32 i;
    internal uint32 after;
}

public static void TestCompareAndSwapUint32(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestCompareAndSwapUint32_x(), out var Ꮡx);
    x.before = magic32;
    x.after = magic32;
    for (var val = (uint32)1; val + val > val; val += val) {
        x.i = val;
        if (!CompareAndSwapUint32(Ꮡx.of(TestCompareAndSwapUint32_x.Ꮡi), val, val + 1)) {
            Ꮡt.Fatalf("should have swapped %#x %#x"u8, val, val + 1);
        }
        if (x.i != val + 1) {
            Ꮡt.Fatalf("wrong x.i after swap: x.i=%#x val+1=%#x"u8, x.i, val + 1);
        }
        x.i = val + 1;
        if (CompareAndSwapUint32(Ꮡx.of(TestCompareAndSwapUint32_x.Ꮡi), val, val + 2)) {
            Ꮡt.Fatalf("should not have swapped %#x %#x"u8, val, val + 2);
        }
        if (x.i != val + 1) {
            Ꮡt.Fatalf("wrong x.i after swap: x.i=%#x val+1=%#x"u8, x.i, val + 1);
        }
    }
    if (x.before != magic32 || x.after != magic32) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, (nint)(magic32), (nint)(magic32));
    }
}

[GoType("dyn")] internal partial struct TestCompareAndSwapUint32Method_x {
    internal uint32 before;
    internal atomic.Uint32 i;
    internal uint32 after;
}

public static void TestCompareAndSwapUint32Method(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestCompareAndSwapUint32Method_x(), out var Ꮡx);
    x.before = magic32;
    x.after = magic32;
    for (var val = (uint32)1; val + val > val; val += val) {
        Ꮡx.of(TestCompareAndSwapUint32Method_x.Ꮡi).Store(val);
        if (!Ꮡx.of(TestCompareAndSwapUint32Method_x.Ꮡi).CompareAndSwap(val, val + 1)) {
            Ꮡt.Fatalf("should have swapped %#x %#x"u8, val, val + 1);
        }
        if (Ꮡx.of(TestCompareAndSwapUint32Method_x.Ꮡi).Load() != val + 1) {
            Ꮡt.Fatalf("wrong x.i after swap: x.i=%#x val+1=%#x"u8, Ꮡx.of(TestCompareAndSwapUint32Method_x.Ꮡi).Load(), val + 1);
        }
        Ꮡx.of(TestCompareAndSwapUint32Method_x.Ꮡi).Store(val + 1);
        if (Ꮡx.of(TestCompareAndSwapUint32Method_x.Ꮡi).CompareAndSwap(val, val + 2)) {
            Ꮡt.Fatalf("should not have swapped %#x %#x"u8, val, val + 2);
        }
        if (Ꮡx.of(TestCompareAndSwapUint32Method_x.Ꮡi).Load() != val + 1) {
            Ꮡt.Fatalf("wrong x.i after swap: x.i=%#x val+1=%#x"u8, Ꮡx.of(TestCompareAndSwapUint32Method_x.Ꮡi).Load(), val + 1);
        }
    }
    if (x.before != magic32 || x.after != magic32) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, (nint)(magic32), (nint)(magic32));
    }
}

[GoType("dyn")] internal partial struct TestCompareAndSwapInt64_x {
    internal int64 before;
    internal int64 i;
    internal int64 after;
}

public static void TestCompareAndSwapInt64(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestCompareAndSwapInt64_x(), out var Ꮡx);
    var magic64 = (int64)atomic_test_package.magic64;
    x.before = magic64;
    x.after = magic64;
    for (var val = (int64)1; val + val > val; val += val) {
        x.i = val;
        if (!CompareAndSwapInt64(Ꮡx.of(TestCompareAndSwapInt64_x.Ꮡi), val, val + 1)) {
            Ꮡt.Fatalf("should have swapped %#x %#x"u8, val, val + 1);
        }
        if (x.i != val + 1) {
            Ꮡt.Fatalf("wrong x.i after swap: x.i=%#x val+1=%#x"u8, x.i, val + 1);
        }
        x.i = val + 1;
        if (CompareAndSwapInt64(Ꮡx.of(TestCompareAndSwapInt64_x.Ꮡi), val, val + 2)) {
            Ꮡt.Fatalf("should not have swapped %#x %#x"u8, val, val + 2);
        }
        if (x.i != val + 1) {
            Ꮡt.Fatalf("wrong x.i after swap: x.i=%#x val+1=%#x"u8, x.i, val + 1);
        }
    }
    if (x.before != magic64 || x.after != magic64) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, magic64, magic64);
    }
}

[GoType("dyn")] internal partial struct TestCompareAndSwapInt64Method_x {
    internal int64 before;
    internal atomic.Int64 i;
    internal int64 after;
}

public static void TestCompareAndSwapInt64Method(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestCompareAndSwapInt64Method_x(), out var Ꮡx);
    var magic64 = (int64)atomic_test_package.magic64;
    x.before = magic64;
    x.after = magic64;
    for (var val = (int64)1; val + val > val; val += val) {
        Ꮡx.of(TestCompareAndSwapInt64Method_x.Ꮡi).Store(val);
        if (!Ꮡx.of(TestCompareAndSwapInt64Method_x.Ꮡi).CompareAndSwap(val, val + 1)) {
            Ꮡt.Fatalf("should have swapped %#x %#x"u8, val, val + 1);
        }
        if (Ꮡx.of(TestCompareAndSwapInt64Method_x.Ꮡi).Load() != val + 1) {
            Ꮡt.Fatalf("wrong x.i after swap: x.i=%#x val+1=%#x"u8, Ꮡx.of(TestCompareAndSwapInt64Method_x.Ꮡi).Load(), val + 1);
        }
        Ꮡx.of(TestCompareAndSwapInt64Method_x.Ꮡi).Store(val + 1);
        if (Ꮡx.of(TestCompareAndSwapInt64Method_x.Ꮡi).CompareAndSwap(val, val + 2)) {
            Ꮡt.Fatalf("should not have swapped %#x %#x"u8, val, val + 2);
        }
        if (Ꮡx.of(TestCompareAndSwapInt64Method_x.Ꮡi).Load() != val + 1) {
            Ꮡt.Fatalf("wrong x.i after swap: x.i=%#x val+1=%#x"u8, Ꮡx.of(TestCompareAndSwapInt64Method_x.Ꮡi).Load(), val + 1);
        }
    }
    if (x.before != magic64 || x.after != magic64) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, magic64, magic64);
    }
}

[GoType("dyn")] internal partial struct testCompareAndSwapUint64_x {
    internal uint64 before;
    internal uint64 i;
    internal uint64 after;
}

internal static void testCompareAndSwapUint64(ж<testing.T> Ꮡt, Func<ж<uint64>, uint64, uint64, bool> cas) {
    ref var x = ref heap(new testCompareAndSwapUint64_x(), out var Ꮡx);
    var magic64 = (uint64)atomic_test_package.magic64;
    x.before = magic64;
    x.after = magic64;
    for (var val = (uint64)1; val + val > val; val += val) {
        x.i = val;
        if (!cas(Ꮡx.of(testCompareAndSwapUint64_x.Ꮡi), val, val + 1)) {
            Ꮡt.Fatalf("should have swapped %#x %#x"u8, val, val + 1);
        }
        if (x.i != val + 1) {
            Ꮡt.Fatalf("wrong x.i after swap: x.i=%#x val+1=%#x"u8, x.i, val + 1);
        }
        x.i = val + 1;
        if (cas(Ꮡx.of(testCompareAndSwapUint64_x.Ꮡi), val, val + 2)) {
            Ꮡt.Fatalf("should not have swapped %#x %#x"u8, val, val + 2);
        }
        if (x.i != val + 1) {
            Ꮡt.Fatalf("wrong x.i after swap: x.i=%#x val+1=%#x"u8, x.i, val + 1);
        }
    }
    if (x.before != magic64 || x.after != magic64) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, magic64, magic64);
    }
}

public static void TestCompareAndSwapUint64(ж<testing.T> Ꮡt) {
    testCompareAndSwapUint64(Ꮡt, CompareAndSwapUint64);
}

[GoType("dyn")] internal partial struct TestCompareAndSwapUint64Method_x {
    internal uint64 before;
    internal atomic.Uint64 i;
    internal uint64 after;
}

public static void TestCompareAndSwapUint64Method(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestCompareAndSwapUint64Method_x(), out var Ꮡx);
    var magic64 = (uint64)atomic_test_package.magic64;
    x.before = magic64;
    x.after = magic64;
    for (var val = (uint64)1; val + val > val; val += val) {
        Ꮡx.of(TestCompareAndSwapUint64Method_x.Ꮡi).Store(val);
        if (!Ꮡx.of(TestCompareAndSwapUint64Method_x.Ꮡi).CompareAndSwap(val, val + 1)) {
            Ꮡt.Fatalf("should have swapped %#x %#x"u8, val, val + 1);
        }
        if (Ꮡx.of(TestCompareAndSwapUint64Method_x.Ꮡi).Load() != val + 1) {
            Ꮡt.Fatalf("wrong x.i after swap: x.i=%#x val+1=%#x"u8, Ꮡx.of(TestCompareAndSwapUint64Method_x.Ꮡi).Load(), val + 1);
        }
        Ꮡx.of(TestCompareAndSwapUint64Method_x.Ꮡi).Store(val + 1);
        if (Ꮡx.of(TestCompareAndSwapUint64Method_x.Ꮡi).CompareAndSwap(val, val + 2)) {
            Ꮡt.Fatalf("should not have swapped %#x %#x"u8, val, val + 2);
        }
        if (Ꮡx.of(TestCompareAndSwapUint64Method_x.Ꮡi).Load() != val + 1) {
            Ꮡt.Fatalf("wrong x.i after swap: x.i=%#x val+1=%#x"u8, Ꮡx.of(TestCompareAndSwapUint64Method_x.Ꮡi).Load(), val + 1);
        }
    }
    if (x.before != magic64 || x.after != magic64) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, magic64, magic64);
    }
}

[GoType("dyn")] internal partial struct TestCompareAndSwapUintptr_x {
    internal uintptr before;
    internal uintptr i;
    internal uintptr after;
}

public static void TestCompareAndSwapUintptr(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestCompareAndSwapUintptr_x(), out var Ꮡx);
    uint64 m = magic64;
    var magicptr = (uintptr)m;
    x.before = magicptr;
    x.after = magicptr;
    for (var val = (uintptr)1; val + val > val; val += val) {
        x.i = val;
        if (!CompareAndSwapUintptr(Ꮡx.of(TestCompareAndSwapUintptr_x.Ꮡi), val, val + 1)) {
            Ꮡt.Fatalf("should have swapped %#x %#x"u8, val, val + 1);
        }
        if (x.i != val + 1) {
            Ꮡt.Fatalf("wrong x.i after swap: x.i=%#x val+1=%#x"u8, x.i, val + 1);
        }
        x.i = val + 1;
        if (CompareAndSwapUintptr(Ꮡx.of(TestCompareAndSwapUintptr_x.Ꮡi), val, val + 2)) {
            Ꮡt.Fatalf("should not have swapped %#x %#x"u8, val, val + 2);
        }
        if (x.i != val + 1) {
            Ꮡt.Fatalf("wrong x.i after swap: x.i=%#x val+1=%#x"u8, x.i, val + 1);
        }
    }
    if (x.before != magicptr || x.after != magicptr) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, magicptr, magicptr);
    }
}

[GoType("dyn")] internal partial struct TestCompareAndSwapUintptrMethod_x {
    internal uintptr before;
    internal atomic.Uintptr i;
    internal uintptr after;
}

public static void TestCompareAndSwapUintptrMethod(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestCompareAndSwapUintptrMethod_x(), out var Ꮡx);
    uint64 m = magic64;
    var magicptr = (uintptr)m;
    x.before = magicptr;
    x.after = magicptr;
    for (var val = (uintptr)1; val + val > val; val += val) {
        Ꮡx.of(TestCompareAndSwapUintptrMethod_x.Ꮡi).Store(val);
        if (!Ꮡx.of(TestCompareAndSwapUintptrMethod_x.Ꮡi).CompareAndSwap(val, val + 1)) {
            Ꮡt.Fatalf("should have swapped %#x %#x"u8, val, val + 1);
        }
        if (Ꮡx.of(TestCompareAndSwapUintptrMethod_x.Ꮡi).Load() != val + 1) {
            Ꮡt.Fatalf("wrong x.i after swap: x.i=%#x val+1=%#x"u8, Ꮡx.of(TestCompareAndSwapUintptrMethod_x.Ꮡi).Load(), val + 1);
        }
        Ꮡx.of(TestCompareAndSwapUintptrMethod_x.Ꮡi).Store(val + 1);
        if (Ꮡx.of(TestCompareAndSwapUintptrMethod_x.Ꮡi).CompareAndSwap(val, val + 2)) {
            Ꮡt.Fatalf("should not have swapped %#x %#x"u8, val, val + 2);
        }
        if (Ꮡx.of(TestCompareAndSwapUintptrMethod_x.Ꮡi).Load() != val + 1) {
            Ꮡt.Fatalf("wrong x.i after swap: x.i=%#x val+1=%#x"u8, Ꮡx.of(TestCompareAndSwapUintptrMethod_x.Ꮡi).Load(), val + 1);
        }
    }
    if (x.before != magicptr || x.after != magicptr) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, (uintptr)magicptr, (uintptr)magicptr);
    }
}

[GoType("dyn")] internal partial struct TestCompareAndSwapPointer_x {
    internal uintptr before;
    internal @unsafe.Pointer i;
    internal uintptr after;
}

public static void TestCompareAndSwapPointer(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestCompareAndSwapPointer_x(), out var Ꮡx);
    uint64 m = magic64;
    var magicptr = (uintptr)m;
    x.before = magicptr;
    x.after = magicptr;
    @unsafe.Pointer q = @unsafe.Pointer.FromPinnedBox(@new<byte>());
    foreach (var (_, p) in testPointers()) {
        x.i = p;
        if (!CompareAndSwapPointer(Ꮡx.of(TestCompareAndSwapPointer_x.Ꮡi), p, q)) {
            Ꮡt.Fatalf("should have swapped %p %p"u8, @unsafe.Pointer.OrTypedNil(p), @unsafe.Pointer.OrTypedNil(q));
        }
        if (x.i != q) {
            Ꮡt.Fatalf("wrong x.i after swap: x.i=%p want %p"u8, @unsafe.Pointer.OrTypedNil(x.i), @unsafe.Pointer.OrTypedNil(q));
        }
        if (CompareAndSwapPointer(Ꮡx.of(TestCompareAndSwapPointer_x.Ꮡi), p, nil)) {
            Ꮡt.Fatalf("should not have swapped %p nil"u8, @unsafe.Pointer.OrTypedNil(p));
        }
        if (x.i != q) {
            Ꮡt.Fatalf("wrong x.i after swap: x.i=%p want %p"u8, @unsafe.Pointer.OrTypedNil(x.i), @unsafe.Pointer.OrTypedNil(q));
        }
    }
    if (x.before != magicptr || x.after != magicptr) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, magicptr, magicptr);
    }
}

[GoType("dyn")] internal partial struct TestCompareAndSwapPointerMethod_x {
    internal uintptr before;
    internal atomic.Pointer<byte> i;
    internal uintptr after;
}

public static void TestCompareAndSwapPointerMethod(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestCompareAndSwapPointerMethod_x(), out var Ꮡx);
    uint64 m = magic64;
    var magicptr = (uintptr)m;
    x.before = magicptr;
    x.after = magicptr;
    var q = @new<byte>();
    foreach (var (_, p) in testPointers()) {
        var pΔ1 = (ж<byte>)(uintptr)(p);
        Ꮡx.of(TestCompareAndSwapPointerMethod_x.Ꮡi).Store(pΔ1);
        if (!Ꮡx.of(TestCompareAndSwapPointerMethod_x.Ꮡi).CompareAndSwap(pΔ1, q)) {
            Ꮡt.Fatalf("should have swapped %p %p"u8, pΔ1.OrTypedNil(), q.OrTypedNil());
        }
        if (Ꮡx.of(TestCompareAndSwapPointerMethod_x.Ꮡi).Load() != q) {
            Ꮡt.Fatalf("wrong x.i after swap: x.i=%p want %p"u8, Ꮡx.of(TestCompareAndSwapPointerMethod_x.Ꮡi).Load().OrTypedNil(), q.OrTypedNil());
        }
        if (Ꮡx.of(TestCompareAndSwapPointerMethod_x.Ꮡi).CompareAndSwap(pΔ1, nil)) {
            Ꮡt.Fatalf("should not have swapped %p nil"u8, pΔ1.OrTypedNil());
        }
        if (Ꮡx.of(TestCompareAndSwapPointerMethod_x.Ꮡi).Load() != q) {
            Ꮡt.Fatalf("wrong x.i after swap: x.i=%p want %p"u8, Ꮡx.of(TestCompareAndSwapPointerMethod_x.Ꮡi).Load().OrTypedNil(), q.OrTypedNil());
        }
    }
    if (x.before != magicptr || x.after != magicptr) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, magicptr, magicptr);
    }
}

[GoType("dyn")] internal partial struct TestLoadInt32_x {
    internal int32 before;
    internal int32 i;
    internal int32 after;
}

public static void TestLoadInt32(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestLoadInt32_x(), out var Ꮡx);
    x.before = magic32;
    x.after = magic32;
    for (var delta = (int32)1; delta + delta > delta; delta += delta) {
        var k = LoadInt32(Ꮡx.of(TestLoadInt32_x.Ꮡi));
        if (k != x.i) {
            Ꮡt.Fatalf("delta=%d i=%d k=%d"u8, delta, x.i, k);
        }
        x.i += delta;
    }
    if (x.before != magic32 || x.after != magic32) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, (nint)(magic32), (nint)(magic32));
    }
}

[GoType("dyn")] internal partial struct TestLoadInt32Method_x {
    internal int32 before;
    internal atomic.Int32 i;
    internal int32 after;
}

public static void TestLoadInt32Method(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestLoadInt32Method_x(), out var Ꮡx);
    x.before = magic32;
    x.after = magic32;
    var want = (int32)0;
    for (var delta = (int32)1; delta + delta > delta; delta += delta) {
        var k = Ꮡx.of(TestLoadInt32Method_x.Ꮡi).Load();
        if (k != want) {
            Ꮡt.Fatalf("delta=%d i=%d k=%d want=%d"u8, delta, Ꮡx.of(TestLoadInt32Method_x.Ꮡi).Load(), k, want);
        }
        Ꮡx.of(TestLoadInt32Method_x.Ꮡi).Store(k + delta);
        want = k + delta;
    }
    if (x.before != magic32 || x.after != magic32) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, (nint)(magic32), (nint)(magic32));
    }
}

[GoType("dyn")] internal partial struct TestLoadUint32_x {
    internal uint32 before;
    internal uint32 i;
    internal uint32 after;
}

public static void TestLoadUint32(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestLoadUint32_x(), out var Ꮡx);
    x.before = magic32;
    x.after = magic32;
    for (var delta = (uint32)1; delta + delta > delta; delta += delta) {
        var k = LoadUint32(Ꮡx.of(TestLoadUint32_x.Ꮡi));
        if (k != x.i) {
            Ꮡt.Fatalf("delta=%d i=%d k=%d"u8, delta, x.i, k);
        }
        x.i += delta;
    }
    if (x.before != magic32 || x.after != magic32) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, (nint)(magic32), (nint)(magic32));
    }
}

[GoType("dyn")] internal partial struct TestLoadUint32Method_x {
    internal uint32 before;
    internal atomic.Uint32 i;
    internal uint32 after;
}

public static void TestLoadUint32Method(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestLoadUint32Method_x(), out var Ꮡx);
    x.before = magic32;
    x.after = magic32;
    var want = (uint32)0;
    for (var delta = (uint32)1; delta + delta > delta; delta += delta) {
        var k = Ꮡx.of(TestLoadUint32Method_x.Ꮡi).Load();
        if (k != want) {
            Ꮡt.Fatalf("delta=%d i=%d k=%d want=%d"u8, delta, Ꮡx.of(TestLoadUint32Method_x.Ꮡi).Load(), k, want);
        }
        Ꮡx.of(TestLoadUint32Method_x.Ꮡi).Store(k + delta);
        want = k + delta;
    }
    if (x.before != magic32 || x.after != magic32) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, (nint)(magic32), (nint)(magic32));
    }
}

[GoType("dyn")] internal partial struct TestLoadInt64_x {
    internal int64 before;
    internal int64 i;
    internal int64 after;
}

public static void TestLoadInt64(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestLoadInt64_x(), out var Ꮡx);
    var magic64 = (int64)atomic_test_package.magic64;
    x.before = magic64;
    x.after = magic64;
    for (var delta = (int64)1; delta + delta > delta; delta += delta) {
        var k = LoadInt64(Ꮡx.of(TestLoadInt64_x.Ꮡi));
        if (k != x.i) {
            Ꮡt.Fatalf("delta=%d i=%d k=%d"u8, delta, x.i, k);
        }
        x.i += delta;
    }
    if (x.before != magic64 || x.after != magic64) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, magic64, magic64);
    }
}

[GoType("dyn")] internal partial struct TestLoadInt64Method_x {
    internal int64 before;
    internal atomic.Int64 i;
    internal int64 after;
}

public static void TestLoadInt64Method(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestLoadInt64Method_x(), out var Ꮡx);
    var magic64 = (int64)atomic_test_package.magic64;
    x.before = magic64;
    x.after = magic64;
    var want = (int64)0;
    for (var delta = (int64)1; delta + delta > delta; delta += delta) {
        var k = Ꮡx.of(TestLoadInt64Method_x.Ꮡi).Load();
        if (k != want) {
            Ꮡt.Fatalf("delta=%d i=%d k=%d want=%d"u8, delta, Ꮡx.of(TestLoadInt64Method_x.Ꮡi).Load(), k, want);
        }
        Ꮡx.of(TestLoadInt64Method_x.Ꮡi).Store(k + delta);
        want = k + delta;
    }
    if (x.before != magic64 || x.after != magic64) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, magic64, magic64);
    }
}

[GoType("dyn")] internal partial struct TestLoadUint64_x {
    internal uint64 before;
    internal uint64 i;
    internal uint64 after;
}

public static void TestLoadUint64(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestLoadUint64_x(), out var Ꮡx);
    var magic64 = (uint64)atomic_test_package.magic64;
    x.before = magic64;
    x.after = magic64;
    for (var delta = (uint64)1; delta + delta > delta; delta += delta) {
        var k = LoadUint64(Ꮡx.of(TestLoadUint64_x.Ꮡi));
        if (k != x.i) {
            Ꮡt.Fatalf("delta=%d i=%d k=%d"u8, delta, x.i, k);
        }
        x.i += delta;
    }
    if (x.before != magic64 || x.after != magic64) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, magic64, magic64);
    }
}

[GoType("dyn")] internal partial struct TestLoadUint64Method_x {
    internal uint64 before;
    internal atomic.Uint64 i;
    internal uint64 after;
}

public static void TestLoadUint64Method(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestLoadUint64Method_x(), out var Ꮡx);
    var magic64 = (uint64)atomic_test_package.magic64;
    x.before = magic64;
    x.after = magic64;
    var want = (uint64)0;
    for (var delta = (uint64)1; delta + delta > delta; delta += delta) {
        var k = Ꮡx.of(TestLoadUint64Method_x.Ꮡi).Load();
        if (k != want) {
            Ꮡt.Fatalf("delta=%d i=%d k=%d want=%d"u8, delta, Ꮡx.of(TestLoadUint64Method_x.Ꮡi).Load(), k, want);
        }
        Ꮡx.of(TestLoadUint64Method_x.Ꮡi).Store(k + delta);
        want = k + delta;
    }
    if (x.before != magic64 || x.after != magic64) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, magic64, magic64);
    }
}

[GoType("dyn")] internal partial struct TestLoadUintptr_x {
    internal uintptr before;
    internal uintptr i;
    internal uintptr after;
}

public static void TestLoadUintptr(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestLoadUintptr_x(), out var Ꮡx);
    uint64 m = magic64;
    var magicptr = (uintptr)m;
    x.before = magicptr;
    x.after = magicptr;
    for (var delta = (uintptr)1; delta + delta > delta; delta += delta) {
        var k = LoadUintptr(Ꮡx.of(TestLoadUintptr_x.Ꮡi));
        if (k != x.i) {
            Ꮡt.Fatalf("delta=%d i=%d k=%d"u8, delta, x.i, k);
        }
        x.i += delta;
    }
    if (x.before != magicptr || x.after != magicptr) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, magicptr, magicptr);
    }
}

[GoType("dyn")] internal partial struct TestLoadUintptrMethod_x {
    internal uintptr before;
    internal atomic.Uintptr i;
    internal uintptr after;
}

public static void TestLoadUintptrMethod(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestLoadUintptrMethod_x(), out var Ꮡx);
    uint64 m = magic64;
    var magicptr = (uintptr)m;
    x.before = magicptr;
    x.after = magicptr;
    var want = (uintptr)0;
    for (var delta = (uintptr)1; delta + delta > delta; delta += delta) {
        var k = Ꮡx.of(TestLoadUintptrMethod_x.Ꮡi).Load();
        if (k != want) {
            Ꮡt.Fatalf("delta=%d i=%d k=%d want=%d"u8, delta, Ꮡx.of(TestLoadUintptrMethod_x.Ꮡi).Load(), k, want);
        }
        Ꮡx.of(TestLoadUintptrMethod_x.Ꮡi).Store(k + delta);
        want = k + delta;
    }
    if (x.before != magicptr || x.after != magicptr) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, magicptr, magicptr);
    }
}

[GoType("dyn")] internal partial struct TestLoadPointer_x {
    internal uintptr before;
    internal @unsafe.Pointer i;
    internal uintptr after;
}

public static void TestLoadPointer(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestLoadPointer_x(), out var Ꮡx);
    uint64 m = magic64;
    var magicptr = (uintptr)m;
    x.before = magicptr;
    x.after = magicptr;
    foreach (var (_, p) in testPointers()) {
        x.i = p;
        @unsafe.Pointer k = (uintptr)LoadPointer(Ꮡx.of(TestLoadPointer_x.Ꮡi));
        if (k != p) {
            Ꮡt.Fatalf("p=%x k=%x"u8, @unsafe.Pointer.OrTypedNil(p), @unsafe.Pointer.OrTypedNil(k));
        }
    }
    if (x.before != magicptr || x.after != magicptr) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, magicptr, magicptr);
    }
}

[GoType("dyn")] internal partial struct TestLoadPointerMethod_x {
    internal uintptr before;
    internal atomic.Pointer<byte> i;
    internal uintptr after;
}

public static void TestLoadPointerMethod(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestLoadPointerMethod_x(), out var Ꮡx);
    uint64 m = magic64;
    var magicptr = (uintptr)m;
    x.before = magicptr;
    x.after = magicptr;
    foreach (var (_, p) in testPointers()) {
        var pΔ1 = (ж<byte>)(uintptr)(p);
        Ꮡx.of(TestLoadPointerMethod_x.Ꮡi).Store(pΔ1);
        var k = Ꮡx.of(TestLoadPointerMethod_x.Ꮡi).Load();
        if (k != pΔ1) {
            Ꮡt.Fatalf("p=%x k=%x"u8, pΔ1.OrTypedNil(), k.OrTypedNil());
        }
    }
    if (x.before != magicptr || x.after != magicptr) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, magicptr, magicptr);
    }
}

[GoType("dyn")] internal partial struct TestStoreInt32_x {
    internal int32 before;
    internal int32 i;
    internal int32 after;
}

public static void TestStoreInt32(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestStoreInt32_x(), out var Ꮡx);
    x.before = magic32;
    x.after = magic32;
    var v = (int32)0;
    for (var delta = (int32)1; delta + delta > delta; delta += delta) {
        StoreInt32(Ꮡx.of(TestStoreInt32_x.Ꮡi), v);
        if (x.i != v) {
            Ꮡt.Fatalf("delta=%d i=%d v=%d"u8, delta, x.i, v);
        }
        v += delta;
    }
    if (x.before != magic32 || x.after != magic32) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, (nint)(magic32), (nint)(magic32));
    }
}

[GoType("dyn")] internal partial struct TestStoreInt32Method_x {
    internal int32 before;
    internal atomic.Int32 i;
    internal int32 after;
}

public static void TestStoreInt32Method(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestStoreInt32Method_x(), out var Ꮡx);
    x.before = magic32;
    x.after = magic32;
    var v = (int32)0;
    for (var delta = (int32)1; delta + delta > delta; delta += delta) {
        Ꮡx.of(TestStoreInt32Method_x.Ꮡi).Store(v);
        if (Ꮡx.of(TestStoreInt32Method_x.Ꮡi).Load() != v) {
            Ꮡt.Fatalf("delta=%d i=%d v=%d"u8, delta, Ꮡx.of(TestStoreInt32Method_x.Ꮡi).Load(), v);
        }
        v += delta;
    }
    if (x.before != magic32 || x.after != magic32) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, (nint)(magic32), (nint)(magic32));
    }
}

[GoType("dyn")] internal partial struct TestStoreUint32_x {
    internal uint32 before;
    internal uint32 i;
    internal uint32 after;
}

public static void TestStoreUint32(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestStoreUint32_x(), out var Ꮡx);
    x.before = magic32;
    x.after = magic32;
    var v = (uint32)0;
    for (var delta = (uint32)1; delta + delta > delta; delta += delta) {
        StoreUint32(Ꮡx.of(TestStoreUint32_x.Ꮡi), v);
        if (x.i != v) {
            Ꮡt.Fatalf("delta=%d i=%d v=%d"u8, delta, x.i, v);
        }
        v += delta;
    }
    if (x.before != magic32 || x.after != magic32) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, (nint)(magic32), (nint)(magic32));
    }
}

[GoType("dyn")] internal partial struct TestStoreUint32Method_x {
    internal uint32 before;
    internal atomic.Uint32 i;
    internal uint32 after;
}

public static void TestStoreUint32Method(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestStoreUint32Method_x(), out var Ꮡx);
    x.before = magic32;
    x.after = magic32;
    var v = (uint32)0;
    for (var delta = (uint32)1; delta + delta > delta; delta += delta) {
        Ꮡx.of(TestStoreUint32Method_x.Ꮡi).Store(v);
        if (Ꮡx.of(TestStoreUint32Method_x.Ꮡi).Load() != v) {
            Ꮡt.Fatalf("delta=%d i=%d v=%d"u8, delta, Ꮡx.of(TestStoreUint32Method_x.Ꮡi).Load(), v);
        }
        v += delta;
    }
    if (x.before != magic32 || x.after != magic32) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, (nint)(magic32), (nint)(magic32));
    }
}

[GoType("dyn")] internal partial struct TestStoreInt64_x {
    internal int64 before;
    internal int64 i;
    internal int64 after;
}

public static void TestStoreInt64(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestStoreInt64_x(), out var Ꮡx);
    var magic64 = (int64)atomic_test_package.magic64;
    x.before = magic64;
    x.after = magic64;
    var v = (int64)0;
    for (var delta = (int64)1; delta + delta > delta; delta += delta) {
        StoreInt64(Ꮡx.of(TestStoreInt64_x.Ꮡi), v);
        if (x.i != v) {
            Ꮡt.Fatalf("delta=%d i=%d v=%d"u8, delta, x.i, v);
        }
        v += delta;
    }
    if (x.before != magic64 || x.after != magic64) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, magic64, magic64);
    }
}

[GoType("dyn")] internal partial struct TestStoreInt64Method_x {
    internal int64 before;
    internal atomic.Int64 i;
    internal int64 after;
}

public static void TestStoreInt64Method(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestStoreInt64Method_x(), out var Ꮡx);
    var magic64 = (int64)atomic_test_package.magic64;
    x.before = magic64;
    x.after = magic64;
    var v = (int64)0;
    for (var delta = (int64)1; delta + delta > delta; delta += delta) {
        Ꮡx.of(TestStoreInt64Method_x.Ꮡi).Store(v);
        if (Ꮡx.of(TestStoreInt64Method_x.Ꮡi).Load() != v) {
            Ꮡt.Fatalf("delta=%d i=%d v=%d"u8, delta, Ꮡx.of(TestStoreInt64Method_x.Ꮡi).Load(), v);
        }
        v += delta;
    }
    if (x.before != magic64 || x.after != magic64) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, magic64, magic64);
    }
}

[GoType("dyn")] internal partial struct TestStoreUint64_x {
    internal uint64 before;
    internal uint64 i;
    internal uint64 after;
}

public static void TestStoreUint64(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestStoreUint64_x(), out var Ꮡx);
    var magic64 = (uint64)atomic_test_package.magic64;
    x.before = magic64;
    x.after = magic64;
    var v = (uint64)0;
    for (var delta = (uint64)1; delta + delta > delta; delta += delta) {
        StoreUint64(Ꮡx.of(TestStoreUint64_x.Ꮡi), v);
        if (x.i != v) {
            Ꮡt.Fatalf("delta=%d i=%d v=%d"u8, delta, x.i, v);
        }
        v += delta;
    }
    if (x.before != magic64 || x.after != magic64) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, magic64, magic64);
    }
}

[GoType("dyn")] internal partial struct TestStoreUint64Method_x {
    internal uint64 before;
    internal atomic.Uint64 i;
    internal uint64 after;
}

public static void TestStoreUint64Method(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestStoreUint64Method_x(), out var Ꮡx);
    var magic64 = (uint64)atomic_test_package.magic64;
    x.before = magic64;
    x.after = magic64;
    var v = (uint64)0;
    for (var delta = (uint64)1; delta + delta > delta; delta += delta) {
        Ꮡx.of(TestStoreUint64Method_x.Ꮡi).Store(v);
        if (Ꮡx.of(TestStoreUint64Method_x.Ꮡi).Load() != v) {
            Ꮡt.Fatalf("delta=%d i=%d v=%d"u8, delta, Ꮡx.of(TestStoreUint64Method_x.Ꮡi).Load(), v);
        }
        v += delta;
    }
    if (x.before != magic64 || x.after != magic64) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, magic64, magic64);
    }
}

[GoType("dyn")] internal partial struct TestStoreUintptr_x {
    internal uintptr before;
    internal uintptr i;
    internal uintptr after;
}

public static void TestStoreUintptr(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestStoreUintptr_x(), out var Ꮡx);
    uint64 m = magic64;
    var magicptr = (uintptr)m;
    x.before = magicptr;
    x.after = magicptr;
    var v = (uintptr)0;
    for (var delta = (uintptr)1; delta + delta > delta; delta += delta) {
        StoreUintptr(Ꮡx.of(TestStoreUintptr_x.Ꮡi), v);
        if (x.i != v) {
            Ꮡt.Fatalf("delta=%d i=%d v=%d"u8, delta, x.i, v);
        }
        v += delta;
    }
    if (x.before != magicptr || x.after != magicptr) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, magicptr, magicptr);
    }
}

[GoType("dyn")] internal partial struct TestStoreUintptrMethod_x {
    internal uintptr before;
    internal atomic.Uintptr i;
    internal uintptr after;
}

public static void TestStoreUintptrMethod(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestStoreUintptrMethod_x(), out var Ꮡx);
    uint64 m = magic64;
    var magicptr = (uintptr)m;
    x.before = magicptr;
    x.after = magicptr;
    var v = (uintptr)0;
    for (var delta = (uintptr)1; delta + delta > delta; delta += delta) {
        Ꮡx.of(TestStoreUintptrMethod_x.Ꮡi).Store(v);
        if (Ꮡx.of(TestStoreUintptrMethod_x.Ꮡi).Load() != v) {
            Ꮡt.Fatalf("delta=%d i=%d v=%d"u8, delta, Ꮡx.of(TestStoreUintptrMethod_x.Ꮡi).Load(), v);
        }
        v += delta;
    }
    if (x.before != magicptr || x.after != magicptr) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, magicptr, magicptr);
    }
}

[GoType("dyn")] internal partial struct TestStorePointer_x {
    internal uintptr before;
    internal @unsafe.Pointer i;
    internal uintptr after;
}

public static void TestStorePointer(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestStorePointer_x(), out var Ꮡx);
    uint64 m = magic64;
    var magicptr = (uintptr)m;
    x.before = magicptr;
    x.after = magicptr;
    foreach (var (_, p) in testPointers()) {
        StorePointer(Ꮡx.of(TestStorePointer_x.Ꮡi), p);
        if (x.i != p) {
            Ꮡt.Fatalf("x.i=%p p=%p"u8, @unsafe.Pointer.OrTypedNil(x.i), @unsafe.Pointer.OrTypedNil(p));
        }
    }
    if (x.before != magicptr || x.after != magicptr) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, magicptr, magicptr);
    }
}

[GoType("dyn")] internal partial struct TestStorePointerMethod_x {
    internal uintptr before;
    internal atomic.Pointer<byte> i;
    internal uintptr after;
}

public static void TestStorePointerMethod(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestStorePointerMethod_x(), out var Ꮡx);
    uint64 m = magic64;
    var magicptr = (uintptr)m;
    x.before = magicptr;
    x.after = magicptr;
    foreach (var (_, p) in testPointers()) {
        var pΔ1 = (ж<byte>)(uintptr)(p);
        Ꮡx.of(TestStorePointerMethod_x.Ꮡi).Store(pΔ1);
        if (Ꮡx.of(TestStorePointerMethod_x.Ꮡi).Load() != pΔ1) {
            Ꮡt.Fatalf("x.i=%p p=%p"u8, Ꮡx.of(TestStorePointerMethod_x.Ꮡi).Load().OrTypedNil(), pΔ1.OrTypedNil());
        }
    }
    if (x.before != magicptr || x.after != magicptr) {
        Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, magicptr, magicptr);
    }
}

// Tests of correct behavior, with contention.
// (Is the function atomic?)
//
// For each function, we write a "hammer" function that repeatedly
// uses the atomic operation to add 1 to a value. After running
// multiple hammers in parallel, check that we end with the correct
// total.
// Swap can't add 1, so it uses a different scheme.
// The functions repeatedly generate a pseudo-random number such that
// low bits are equal to high bits, swap, check that the old value
// has low and high bits equal.
internal static map<@string, Action<ж<uint32>, nint>> hammer32 = new map<@string, Action<ж<uint32>, nint>>{
    ["SwapInt32"u8] = hammerSwapInt32,
    ["SwapUint32"u8] = hammerSwapUint32,
    ["SwapUintptr"u8] = hammerSwapUintptr32,
    ["AddInt32"u8] = hammerAddInt32,
    ["AddUint32"u8] = hammerAddUint32,
    ["AddUintptr"u8] = hammerAddUintptr32,
    ["CompareAndSwapInt32"u8] = hammerCompareAndSwapInt32,
    ["CompareAndSwapUint32"u8] = hammerCompareAndSwapUint32,
    ["CompareAndSwapUintptr"u8] = hammerCompareAndSwapUintptr32,
    ["SwapInt32Method"u8] = hammerSwapInt32Method,
    ["SwapUint32Method"u8] = hammerSwapUint32Method,
    ["SwapUintptrMethod"u8] = hammerSwapUintptr32Method,
    ["AddInt32Method"u8] = hammerAddInt32Method,
    ["AddUint32Method"u8] = hammerAddUint32Method,
    ["AddUintptrMethod"u8] = hammerAddUintptr32Method,
    ["CompareAndSwapInt32Method"u8] = hammerCompareAndSwapInt32Method,
    ["CompareAndSwapUint32Method"u8] = hammerCompareAndSwapUint32Method,
    ["CompareAndSwapUintptrMethod"u8] = hammerCompareAndSwapUintptr32Method
};

[GoInit] internal static void init() {
    uint64 v = ((uint64)1 << (int)(50));
    if ((uintptr)v != 0) {
        // 64-bit system; clear uintptr tests
        delete(hammer32, "SwapUintptr"u8);
        delete(hammer32, "AddUintptr"u8);
        delete(hammer32, "CompareAndSwapUintptr"u8);
        delete(hammer32, "SwapUintptrMethod"u8);
        delete(hammer32, "AddUintptrMethod"u8);
        delete(hammer32, "CompareAndSwapUintptrMethod"u8);
    }
}

internal static void hammerSwapInt32(ж<uint32> Ꮡuaddr, nint countʗp) {
    ref var count = ref heap(countʗp, out var Ꮡcount);

    var addr = Ꮡuaddr.Reinterpret<uint32, int32>();
    nint seed = (nint)(uintptr)Ꮡcount;
    for (nint i = 0; i < count; i++) {
        var @new = (uint32)(((uint32)(seed + i) << (int)(16)) | (((uint32)(seed + i) << (int)(16)) >> (int)(16)));
        var old = (uint32)SwapInt32(addr, (int32)@new);
        if ((old >> (int)(16)) != ((old << (int)(16)) >> (int)(16))) {
            throw panic(fmt.Sprintf("SwapInt32 is not atomic: %v"u8, old));
        }
    }
}

internal static void hammerSwapInt32Method(ж<uint32> Ꮡuaddr, nint countʗp) {
    ref var count = ref heap(countʗp, out var Ꮡcount);

    var addr = Ꮡuaddr.Reinterpret<uint32, atomic.Int32>();
    nint seed = (nint)(uintptr)Ꮡcount;
    for (nint i = 0; i < count; i++) {
        var @new = (uint32)(((uint32)(seed + i) << (int)(16)) | (((uint32)(seed + i) << (int)(16)) >> (int)(16)));
        var old = (uint32)addr.Swap((int32)@new);
        if ((old >> (int)(16)) != ((old << (int)(16)) >> (int)(16))) {
            throw panic(fmt.Sprintf("SwapInt32 is not atomic: %v"u8, old));
        }
    }
}

internal static void hammerSwapUint32(ж<uint32> Ꮡaddr, nint countʗp) {
    ref var count = ref heap(countʗp, out var Ꮡcount);

    nint seed = (nint)(uintptr)Ꮡcount;
    for (nint i = 0; i < count; i++) {
        var @new = (uint32)(((uint32)(seed + i) << (int)(16)) | (((uint32)(seed + i) << (int)(16)) >> (int)(16)));
        var old = SwapUint32(Ꮡaddr, @new);
        if ((old >> (int)(16)) != ((old << (int)(16)) >> (int)(16))) {
            throw panic(fmt.Sprintf("SwapUint32 is not atomic: %v"u8, old));
        }
    }
}

internal static void hammerSwapUint32Method(ж<uint32> Ꮡuaddr, nint countʗp) {
    ref var count = ref heap(countʗp, out var Ꮡcount);

    var addr = Ꮡuaddr.Reinterpret<uint32, atomic.Uint32>();
    nint seed = (nint)(uintptr)Ꮡcount;
    for (nint i = 0; i < count; i++) {
        var @new = (uint32)(((uint32)(seed + i) << (int)(16)) | (((uint32)(seed + i) << (int)(16)) >> (int)(16)));
        var old = addr.Swap(@new);
        if ((old >> (int)(16)) != ((old << (int)(16)) >> (int)(16))) {
            throw panic(fmt.Sprintf("SwapUint32 is not atomic: %v"u8, old));
        }
    }
}

internal static void hammerSwapUintptr32(ж<uint32> Ꮡuaddr, nint countʗp) {
    ref var count = ref heap(countʗp, out var Ꮡcount);

    // only safe when uintptr is 32-bit.
    // not called on 64-bit systems.
    var addr = Ꮡuaddr.Reinterpret<uint32, uintptr>();
    nint seed = (nint)(uintptr)Ꮡcount;
    for (nint i = 0; i < count; i++) {
        var @new = (uintptr)(((uintptr)(seed + i) << (int)(16)) | (((uintptr)(seed + i) << (int)(16)) >> (int)(16)));
        var old = SwapUintptr(addr, @new);
        if ((old >> (int)(16)) != ((old << (int)(16)) >> (int)(16))) {
            throw panic(fmt.Sprintf("SwapUintptr is not atomic: %#08x"u8, old));
        }
    }
}

internal static void hammerSwapUintptr32Method(ж<uint32> Ꮡuaddr, nint countʗp) {
    ref var count = ref heap(countʗp, out var Ꮡcount);

    // only safe when uintptr is 32-bit.
    // not called on 64-bit systems.
    var addr = Ꮡuaddr.Reinterpret<uint32, atomic.Uintptr>();
    nint seed = (nint)(uintptr)Ꮡcount;
    for (nint i = 0; i < count; i++) {
        var @new = (uintptr)(((uintptr)(seed + i) << (int)(16)) | (((uintptr)(seed + i) << (int)(16)) >> (int)(16)));
        var old = addr.Swap(@new);
        if ((old >> (int)(16)) != ((old << (int)(16)) >> (int)(16))) {
            throw panic(fmt.Sprintf("Uintptr.Swap is not atomic: %#08x"u8, old));
        }
    }
}

internal static void hammerAddInt32(ж<uint32> Ꮡuaddr, nint count) {
    var addr = Ꮡuaddr.Reinterpret<uint32, int32>();
    for (nint i = 0; i < count; i++) {
        AddInt32(addr, 1);
    }
}

internal static void hammerAddInt32Method(ж<uint32> Ꮡuaddr, nint count) {
    var addr = Ꮡuaddr.Reinterpret<uint32, atomic.Int32>();
    for (nint i = 0; i < count; i++) {
        addr.Add(1);
    }
}

internal static void hammerAddUint32(ж<uint32> Ꮡaddr, nint count) {
    for (nint i = 0; i < count; i++) {
        AddUint32(Ꮡaddr, 1);
    }
}

internal static void hammerAddUint32Method(ж<uint32> Ꮡuaddr, nint count) {
    var addr = Ꮡuaddr.Reinterpret<uint32, atomic.Uint32>();
    for (nint i = 0; i < count; i++) {
        addr.Add(1);
    }
}

internal static void hammerAddUintptr32(ж<uint32> Ꮡuaddr, nint count) {
    // only safe when uintptr is 32-bit.
    // not called on 64-bit systems.
    var addr = Ꮡuaddr.Reinterpret<uint32, uintptr>();
    for (nint i = 0; i < count; i++) {
        AddUintptr(addr, 1);
    }
}

internal static void hammerAddUintptr32Method(ж<uint32> Ꮡuaddr, nint count) {
    // only safe when uintptr is 32-bit.
    // not called on 64-bit systems.
    var addr = Ꮡuaddr.Reinterpret<uint32, atomic.Uintptr>();
    for (nint i = 0; i < count; i++) {
        addr.Add(1);
    }
}

internal static void hammerCompareAndSwapInt32(ж<uint32> Ꮡuaddr, nint count) {
    var addr = Ꮡuaddr.Reinterpret<uint32, int32>();
    for (nint i = 0; i < count; i++) {
        while (ᐧ) {
            var v = LoadInt32(addr);
            if (CompareAndSwapInt32(addr, v, v + 1)) {
                break;
            }
        }
    }
}

internal static void hammerCompareAndSwapInt32Method(ж<uint32> Ꮡuaddr, nint count) {
    var addr = Ꮡuaddr.Reinterpret<uint32, atomic.Int32>();
    for (nint i = 0; i < count; i++) {
        while (ᐧ) {
            var v = addr.Load();
            if (addr.CompareAndSwap(v, v + 1)) {
                break;
            }
        }
    }
}

internal static void hammerCompareAndSwapUint32(ж<uint32> Ꮡaddr, nint count) {
    for (nint i = 0; i < count; i++) {
        while (ᐧ) {
            var v = LoadUint32(Ꮡaddr);
            if (CompareAndSwapUint32(Ꮡaddr, v, v + 1)) {
                break;
            }
        }
    }
}

internal static void hammerCompareAndSwapUint32Method(ж<uint32> Ꮡuaddr, nint count) {
    var addr = Ꮡuaddr.Reinterpret<uint32, atomic.Uint32>();
    for (nint i = 0; i < count; i++) {
        while (ᐧ) {
            var v = addr.Load();
            if (addr.CompareAndSwap(v, v + 1)) {
                break;
            }
        }
    }
}

internal static void hammerCompareAndSwapUintptr32(ж<uint32> Ꮡuaddr, nint count) {
    // only safe when uintptr is 32-bit.
    // not called on 64-bit systems.
    var addr = Ꮡuaddr.Reinterpret<uint32, uintptr>();
    for (nint i = 0; i < count; i++) {
        while (ᐧ) {
            var v = LoadUintptr(addr);
            if (CompareAndSwapUintptr(addr, v, v + 1)) {
                break;
            }
        }
    }
}

internal static void hammerCompareAndSwapUintptr32Method(ж<uint32> Ꮡuaddr, nint count) {
    // only safe when uintptr is 32-bit.
    // not called on 64-bit systems.
    var addr = Ꮡuaddr.Reinterpret<uint32, atomic.Uintptr>();
    for (nint i = 0; i < count; i++) {
        while (ᐧ) {
            var v = addr.Load();
            if (addr.CompareAndSwap(v, v + 1)) {
                break;
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string swapˢ = "Swap"u8;

public static partial void TestHammer32(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        UntypedInt p = 4;
        nint n = 100000;
        if (testing.Short()) {
            n = 1000;
        }
        defer(runtime.GOMAXPROCS, runtime.GOMAXPROCS(p), ref ᒐ);
        foreach (var (name, testf) in hammer32) {
            var c = new channel<nint>(0);
            ref var val = ref heap(new uint32(), out var Ꮡval);
            for (nint i = 0; i < p; i++) {
                var cʗ1 = c;
                var testfʗ1 = testf;
                goǃ(() => {
                    GoFrame ᒐ = default;
                    try {
                        var cʗ2 = cʗ1;
                        defer(() => {
                            {
                                var err = recover(); if (err != default!) {
                                    Ꮡt.Error(err._<@string>());
                                }
                            }
                            cʗ2.ᐸꟷ(1);
                        }, ref ᒐ);
                        testfʗ1(Ꮡval, n);
                    }
                    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                    finally { ᒐ.Run(); }
                });
            }
            for (nint i = 0; i < p; i++) {
                ᐸꟷ(c);
            }
            if (!strings.HasPrefix(name, swapˢ) && val != (uint32)n * (uint32)p) {
                Ꮡt.Fatalf("%s: val=%d want %d"u8, name, val, n * (nint)p);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static map<@string, Action<ж<uint64>, nint>> hammer64 = new map<@string, Action<ж<uint64>, nint>>{
    ["SwapInt64"u8] = hammerSwapInt64,
    ["SwapUint64"u8] = hammerSwapUint64,
    ["SwapUintptr"u8] = hammerSwapUintptr64,
    ["AddInt64"u8] = hammerAddInt64,
    ["AddUint64"u8] = hammerAddUint64,
    ["AddUintptr"u8] = hammerAddUintptr64,
    ["CompareAndSwapInt64"u8] = hammerCompareAndSwapInt64,
    ["CompareAndSwapUint64"u8] = hammerCompareAndSwapUint64,
    ["CompareAndSwapUintptr"u8] = hammerCompareAndSwapUintptr64,
    ["SwapInt64Method"u8] = hammerSwapInt64Method,
    ["SwapUint64Method"u8] = hammerSwapUint64Method,
    ["SwapUintptrMethod"u8] = hammerSwapUintptr64Method,
    ["AddInt64Method"u8] = hammerAddInt64Method,
    ["AddUint64Method"u8] = hammerAddUint64Method,
    ["AddUintptrMethod"u8] = hammerAddUintptr64Method,
    ["CompareAndSwapInt64Method"u8] = hammerCompareAndSwapInt64Method,
    ["CompareAndSwapUint64Method"u8] = hammerCompareAndSwapUint64Method,
    ["CompareAndSwapUintptrMethod"u8] = hammerCompareAndSwapUintptr64Method
};

[GoInit] internal static void initΔ1() {
    uint64 v = ((uint64)1 << (int)(50));
    if ((uintptr)v == 0) {
        // 32-bit system; clear uintptr tests
        delete(hammer64, "SwapUintptr"u8);
        delete(hammer64, "SwapUintptrMethod"u8);
        delete(hammer64, "AddUintptr"u8);
        delete(hammer64, "AddUintptrMethod"u8);
        delete(hammer64, "CompareAndSwapUintptr"u8);
        delete(hammer64, "CompareAndSwapUintptrMethod"u8);
    }
}

internal static void hammerSwapInt64(ж<uint64> Ꮡuaddr, nint countʗp) {
    ref var count = ref heap(countʗp, out var Ꮡcount);

    var addr = Ꮡuaddr.Reinterpret<uint64, int64>();
    nint seed = (nint)(uintptr)Ꮡcount;
    for (nint i = 0; i < count; i++) {
        var @new = (uint64)(((uint64)(seed + i) << (int)(32)) | (((uint64)(seed + i) << (int)(32)) >> (int)(32)));
        var old = (uint64)SwapInt64(addr, (int64)@new);
        if ((old >> (int)(32)) != ((old << (int)(32)) >> (int)(32))) {
            throw panic(fmt.Sprintf("SwapInt64 is not atomic: %v"u8, old));
        }
    }
}

internal static void hammerSwapInt64Method(ж<uint64> Ꮡuaddr, nint countʗp) {
    ref var count = ref heap(countʗp, out var Ꮡcount);

    var addr = Ꮡuaddr.Reinterpret<uint64, atomic.Int64>();
    nint seed = (nint)(uintptr)Ꮡcount;
    for (nint i = 0; i < count; i++) {
        var @new = (uint64)(((uint64)(seed + i) << (int)(32)) | (((uint64)(seed + i) << (int)(32)) >> (int)(32)));
        var old = (uint64)addr.Swap((int64)@new);
        if ((old >> (int)(32)) != ((old << (int)(32)) >> (int)(32))) {
            throw panic(fmt.Sprintf("SwapInt64 is not atomic: %v"u8, old));
        }
    }
}

internal static void hammerSwapUint64(ж<uint64> Ꮡaddr, nint countʗp) {
    ref var count = ref heap(countʗp, out var Ꮡcount);

    nint seed = (nint)(uintptr)Ꮡcount;
    for (nint i = 0; i < count; i++) {
        var @new = (uint64)(((uint64)(seed + i) << (int)(32)) | (((uint64)(seed + i) << (int)(32)) >> (int)(32)));
        var old = SwapUint64(Ꮡaddr, @new);
        if ((old >> (int)(32)) != ((old << (int)(32)) >> (int)(32))) {
            throw panic(fmt.Sprintf("SwapUint64 is not atomic: %v"u8, old));
        }
    }
}

internal static void hammerSwapUint64Method(ж<uint64> Ꮡuaddr, nint countʗp) {
    ref var count = ref heap(countʗp, out var Ꮡcount);

    var addr = Ꮡuaddr.Reinterpret<uint64, atomic.Uint64>();
    nint seed = (nint)(uintptr)Ꮡcount;
    for (nint i = 0; i < count; i++) {
        var @new = (uint64)(((uint64)(seed + i) << (int)(32)) | (((uint64)(seed + i) << (int)(32)) >> (int)(32)));
        var old = addr.Swap(@new);
        if ((old >> (int)(32)) != ((old << (int)(32)) >> (int)(32))) {
            throw panic(fmt.Sprintf("SwapUint64 is not atomic: %v"u8, old));
        }
    }
}

internal const bool arch32 = /* unsafe.Sizeof(uintptr(0)) == 4 */ false;

internal static void hammerSwapUintptr64(ж<uint64> Ꮡuaddr, nint countʗp) {
    ref var count = ref heap(countʗp, out var Ꮡcount);

    // only safe when uintptr is 64-bit.
    // not called on 32-bit systems.
    if (!arch32) {
        var addr = Ꮡuaddr.Reinterpret<uint64, uintptr>();
        nint seed = (nint)(uintptr)Ꮡcount;
        for (nint i = 0; i < count; i++) {
            var @new = (uintptr)(((uintptr)(seed + i) << (int)(32)) | (((uintptr)(seed + i) << (int)(32)) >> (int)(32)));
            var old = SwapUintptr(addr, @new);
            if ((old >> (int)(32)) != ((old << (int)(32)) >> (int)(32))) {
                throw panic(fmt.Sprintf("SwapUintptr is not atomic: %v"u8, old));
            }
        }
    }
}

internal static void hammerSwapUintptr64Method(ж<uint64> Ꮡuaddr, nint countʗp) {
    ref var count = ref heap(countʗp, out var Ꮡcount);

    // only safe when uintptr is 64-bit.
    // not called on 32-bit systems.
    if (!arch32) {
        var addr = Ꮡuaddr.Reinterpret<uint64, atomic.Uintptr>();
        nint seed = (nint)(uintptr)Ꮡcount;
        for (nint i = 0; i < count; i++) {
            var @new = (uintptr)(((uintptr)(seed + i) << (int)(32)) | (((uintptr)(seed + i) << (int)(32)) >> (int)(32)));
            var old = addr.Swap(@new);
            if ((old >> (int)(32)) != ((old << (int)(32)) >> (int)(32))) {
                throw panic(fmt.Sprintf("SwapUintptr is not atomic: %v"u8, old));
            }
        }
    }
}

internal static void hammerAddInt64(ж<uint64> Ꮡuaddr, nint count) {
    var addr = Ꮡuaddr.Reinterpret<uint64, int64>();
    for (nint i = 0; i < count; i++) {
        AddInt64(addr, 1);
    }
}

internal static void hammerAddInt64Method(ж<uint64> Ꮡuaddr, nint count) {
    var addr = Ꮡuaddr.Reinterpret<uint64, atomic.Int64>();
    for (nint i = 0; i < count; i++) {
        addr.Add(1);
    }
}

internal static void hammerAddUint64(ж<uint64> Ꮡaddr, nint count) {
    for (nint i = 0; i < count; i++) {
        AddUint64(Ꮡaddr, 1);
    }
}

internal static void hammerAddUint64Method(ж<uint64> Ꮡuaddr, nint count) {
    var addr = Ꮡuaddr.Reinterpret<uint64, atomic.Uint64>();
    for (nint i = 0; i < count; i++) {
        addr.Add(1);
    }
}

internal static void hammerAddUintptr64(ж<uint64> Ꮡuaddr, nint count) {
    // only safe when uintptr is 64-bit.
    // not called on 32-bit systems.
    var addr = Ꮡuaddr.Reinterpret<uint64, uintptr>();
    for (nint i = 0; i < count; i++) {
        AddUintptr(addr, 1);
    }
}

internal static void hammerAddUintptr64Method(ж<uint64> Ꮡuaddr, nint count) {
    // only safe when uintptr is 64-bit.
    // not called on 32-bit systems.
    var addr = Ꮡuaddr.Reinterpret<uint64, atomic.Uintptr>();
    for (nint i = 0; i < count; i++) {
        addr.Add(1);
    }
}

internal static void hammerCompareAndSwapInt64(ж<uint64> Ꮡuaddr, nint count) {
    var addr = Ꮡuaddr.Reinterpret<uint64, int64>();
    for (nint i = 0; i < count; i++) {
        while (ᐧ) {
            var v = LoadInt64(addr);
            if (CompareAndSwapInt64(addr, v, v + 1)) {
                break;
            }
        }
    }
}

internal static void hammerCompareAndSwapInt64Method(ж<uint64> Ꮡuaddr, nint count) {
    var addr = Ꮡuaddr.Reinterpret<uint64, atomic.Int64>();
    for (nint i = 0; i < count; i++) {
        while (ᐧ) {
            var v = addr.Load();
            if (addr.CompareAndSwap(v, v + 1)) {
                break;
            }
        }
    }
}

internal static void hammerCompareAndSwapUint64(ж<uint64> Ꮡaddr, nint count) {
    for (nint i = 0; i < count; i++) {
        while (ᐧ) {
            var v = LoadUint64(Ꮡaddr);
            if (CompareAndSwapUint64(Ꮡaddr, v, v + 1)) {
                break;
            }
        }
    }
}

internal static void hammerCompareAndSwapUint64Method(ж<uint64> Ꮡuaddr, nint count) {
    var addr = Ꮡuaddr.Reinterpret<uint64, atomic.Uint64>();
    for (nint i = 0; i < count; i++) {
        while (ᐧ) {
            var v = addr.Load();
            if (addr.CompareAndSwap(v, v + 1)) {
                break;
            }
        }
    }
}

internal static void hammerCompareAndSwapUintptr64(ж<uint64> Ꮡuaddr, nint count) {
    // only safe when uintptr is 64-bit.
    // not called on 32-bit systems.
    var addr = Ꮡuaddr.Reinterpret<uint64, uintptr>();
    for (nint i = 0; i < count; i++) {
        while (ᐧ) {
            var v = LoadUintptr(addr);
            if (CompareAndSwapUintptr(addr, v, v + 1)) {
                break;
            }
        }
    }
}

internal static void hammerCompareAndSwapUintptr64Method(ж<uint64> Ꮡuaddr, nint count) {
    // only safe when uintptr is 64-bit.
    // not called on 32-bit systems.
    var addr = Ꮡuaddr.Reinterpret<uint64, atomic.Uintptr>();
    for (nint i = 0; i < count; i++) {
        while (ᐧ) {
            var v = addr.Load();
            if (addr.CompareAndSwap(v, v + 1)) {
                break;
            }
        }
    }
}

public static partial void TestHammer64(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        UntypedInt p = 4;
        nint n = 100000;
        if (testing.Short()) {
            n = 1000;
        }
        defer(runtime.GOMAXPROCS, runtime.GOMAXPROCS(p), ref ᒐ);
        foreach (var (name, testf) in hammer64) {
            var c = new channel<nint>(0);
            ref var val = ref heap(new uint64(), out var Ꮡval);
            for (nint i = 0; i < p; i++) {
                var cʗ1 = c;
                var testfʗ1 = testf;
                goǃ(() => {
                    GoFrame ᒐ = default;
                    try {
                        var cʗ2 = cʗ1;
                        defer(() => {
                            {
                                var err = recover(); if (err != default!) {
                                    Ꮡt.Error(err._<@string>());
                                }
                            }
                            cʗ2.ᐸꟷ(1);
                        }, ref ᒐ);
                        testfʗ1(Ꮡval, n);
                    }
                    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                    finally { ᒐ.Run(); }
                });
            }
            for (nint i = 0; i < p; i++) {
                ᐸꟷ(c);
            }
            if (!strings.HasPrefix(name, swapˢ) && val != (uint64)n * (uint64)p) {
                Ꮡt.Fatalf("%s: val=%d want %d"u8, name, val, n * (nint)p);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void hammerStoreLoadInt32(ж<testing.T> Ꮡt, @unsafe.Pointer paddr) {
    var addr = (ж<int32>)(uintptr)(paddr);
    var v = LoadInt32(addr);
    var vlo = (int32)(v & (((1 << (int)(16))) - 1));
    var vhi = (v >> (int)(16));
    if (vlo != vhi) {
        Ꮡt.Fatalf("Int32: %#x != %#x"u8, vlo, vhi);
    }
    var @new = v + 1 + (int32)(1 << (int)(16));
    if (vlo == 10000) {
        @new = 0;
    }
    StoreInt32(addr, @new);
}

internal static void hammerStoreLoadInt32Method(ж<testing.T> Ꮡt, @unsafe.Pointer paddr) {
    var addr = (ж<int32>)(uintptr)(paddr);
    var v = LoadInt32(addr);
    var vlo = (int32)(v & (((1 << (int)(16))) - 1));
    var vhi = (v >> (int)(16));
    if (vlo != vhi) {
        Ꮡt.Fatalf("Int32: %#x != %#x"u8, vlo, vhi);
    }
    var @new = v + 1 + (int32)(1 << (int)(16));
    if (vlo == 10000) {
        @new = 0;
    }
    StoreInt32(addr, @new);
}

internal static void hammerStoreLoadUint32(ж<testing.T> Ꮡt, @unsafe.Pointer paddr) {
    var addr = (ж<uint32>)(uintptr)(paddr);
    var v = LoadUint32(addr);
    var vlo = (uint32)(v & (((1 << (int)(16))) - 1));
    var vhi = (v >> (int)(16));
    if (vlo != vhi) {
        Ꮡt.Fatalf("Uint32: %#x != %#x"u8, vlo, vhi);
    }
    var @new = v + 1 + ((uint32)1 << (int)(16));
    if (vlo == 10000) {
        @new = 0;
    }
    StoreUint32(addr, @new);
}

internal static void hammerStoreLoadUint32Method(ж<testing.T> Ꮡt, @unsafe.Pointer paddr) {
    var addr = (ж<atomic.Uint32>)(uintptr)(paddr);
    var v = addr.Load();
    var vlo = (uint32)(v & (((1 << (int)(16))) - 1));
    var vhi = (v >> (int)(16));
    if (vlo != vhi) {
        Ꮡt.Fatalf("Uint32: %#x != %#x"u8, vlo, vhi);
    }
    var @new = v + 1 + ((uint32)1 << (int)(16));
    if (vlo == 10000) {
        @new = 0;
    }
    addr.Store(@new);
}

internal static void hammerStoreLoadInt64(ж<testing.T> Ꮡt, @unsafe.Pointer paddr) {
    var addr = (ж<int64>)(uintptr)(paddr);
    var v = LoadInt64(addr);
    var vlo = (int64)(v & (4294967295L));
    var vhi = (v >> (int)(32));
    if (vlo != vhi) {
        Ꮡt.Fatalf("Int64: %#x != %#x"u8, vlo, vhi);
    }
    var @new = v + 1 + 4294967296L;
    StoreInt64(addr, @new);
}

internal static void hammerStoreLoadInt64Method(ж<testing.T> Ꮡt, @unsafe.Pointer paddr) {
    var addr = (ж<atomic.Int64>)(uintptr)(paddr);
    var v = addr.Load();
    var vlo = (int64)(v & (4294967295L));
    var vhi = (v >> (int)(32));
    if (vlo != vhi) {
        Ꮡt.Fatalf("Int64: %#x != %#x"u8, vlo, vhi);
    }
    var @new = v + 1 + 4294967296L;
    addr.Store(@new);
}

internal static void hammerStoreLoadUint64(ж<testing.T> Ꮡt, @unsafe.Pointer paddr) {
    var addr = (ж<uint64>)(uintptr)(paddr);
    var v = LoadUint64(addr);
    var vlo = (uint64)(v & ((uint64)((4294967296L) - 1)));
    var vhi = (v >> (int)(32));
    if (vlo != vhi) {
        Ꮡt.Fatalf("Uint64: %#x != %#x"u8, vlo, vhi);
    }
    var @new = v + 1 + ((uint64)1 << (int)(32));
    StoreUint64(addr, @new);
}

internal static void hammerStoreLoadUint64Method(ж<testing.T> Ꮡt, @unsafe.Pointer paddr) {
    var addr = (ж<atomic.Uint64>)(uintptr)(paddr);
    var v = addr.Load();
    var vlo = (uint64)(v & ((uint64)((4294967296L) - 1)));
    var vhi = (v >> (int)(32));
    if (vlo != vhi) {
        Ꮡt.Fatalf("Uint64: %#x != %#x"u8, vlo, vhi);
    }
    var @new = v + 1 + ((uint64)1 << (int)(32));
    addr.Store(@new);
}

internal static void hammerStoreLoadUintptr(ж<testing.T> Ꮡt, @unsafe.Pointer paddr) {
    var addr = (ж<uintptr>)(uintptr)(paddr);
    var v = LoadUintptr(addr);
    var @new = v;
    if (arch32){
        var vlo = (uintptr)(v & (uintptr)(((1 << (int)(16))) - 1));
        var vhi = (v >> (int)(16));
        if (vlo != vhi) {
            Ꮡt.Fatalf("Uintptr: %#x != %#x"u8, vlo, vhi);
        }
        @new = v + 1 + ((uintptr)1 << (int)(16));
        if (vlo == 10000) {
            @new = 0;
        }
    } else {
        var vlo = (uintptr)(v & (uintptr)((uintptr)((4294967296L) - 1)));
        var vhi = (v >> (int)(32));
        if (vlo != vhi) {
            Ꮡt.Fatalf("Uintptr: %#x != %#x"u8, vlo, vhi);
        }
        var inc = (uint64)(1 + 4294967296L);
        @new = v + (uintptr)inc;
    }
    StoreUintptr(addr, @new);
}

//go:nocheckptr
internal static void hammerStoreLoadUintptrMethod(ж<testing.T> Ꮡt, @unsafe.Pointer paddr) {
    var addr = (ж<atomic.Uintptr>)(uintptr)(paddr);
    var v = addr.Load();
    var @new = v;
    if (arch32){
        var vlo = (uintptr)(v & (uintptr)(((1 << (int)(16))) - 1));
        var vhi = (v >> (int)(16));
        if (vlo != vhi) {
            Ꮡt.Fatalf("Uintptr: %#x != %#x"u8, vlo, vhi);
        }
        @new = v + 1 + ((uintptr)1 << (int)(16));
        if (vlo == 10000) {
            @new = 0;
        }
    } else {
        var vlo = (uintptr)(v & (uintptr)((uintptr)((4294967296L) - 1)));
        var vhi = (v >> (int)(32));
        if (vlo != vhi) {
            Ꮡt.Fatalf("Uintptr: %#x != %#x"u8, vlo, vhi);
        }
        var inc = (uint64)(1 + 4294967296L);
        @new = v + (uintptr)inc;
    }
    addr.Store(@new);
}

// This code is just testing that LoadPointer/StorePointer operate
// atomically; it's not actually calculating pointers.
//
//go:nocheckptr
internal static void hammerStoreLoadPointer(ж<testing.T> Ꮡt, @unsafe.Pointer paddr) {
    var addr = (ж<@unsafe.Pointer>)(uintptr)(paddr);
    var v = (uintptr)(uintptr)LoadPointer(addr);
    var @new = v;
    if (arch32){
        var vlo = (uintptr)(v & (uintptr)(((1 << (int)(16))) - 1));
        var vhi = (v >> (int)(16));
        if (vlo != vhi) {
            Ꮡt.Fatalf("Pointer: %#x != %#x"u8, vlo, vhi);
        }
        @new = v + 1 + ((uintptr)1 << (int)(16));
        if (vlo == 10000) {
            @new = 0;
        }
    } else {
        var vlo = (uintptr)(v & (uintptr)((uintptr)((4294967296L) - 1)));
        var vhi = (v >> (int)(32));
        if (vlo != vhi) {
            Ꮡt.Fatalf("Pointer: %#x != %#x"u8, vlo, vhi);
        }
        var inc = (uint64)(1 + 4294967296L);
        @new = v + (uintptr)inc;
    }
    StorePointer(addr, (@unsafe.Pointer)@new);
}

// This code is just testing that LoadPointer/StorePointer operate
// atomically; it's not actually calculating pointers.
//
//go:nocheckptr
internal static void hammerStoreLoadPointerMethod(ж<testing.T> Ꮡt, @unsafe.Pointer paddr) {
    var addr = (ж<atomic.Pointer<byte>>)(uintptr)(paddr);
    var v = (uintptr)addr.Load();
    var @new = v;
    if (arch32){
        var vlo = (uintptr)(v & (uintptr)(((1 << (int)(16))) - 1));
        var vhi = (v >> (int)(16));
        if (vlo != vhi) {
            Ꮡt.Fatalf("Pointer: %#x != %#x"u8, vlo, vhi);
        }
        @new = v + 1 + ((uintptr)1 << (int)(16));
        if (vlo == 10000) {
            @new = 0;
        }
    } else {
        var vlo = (uintptr)(v & (uintptr)((uintptr)((4294967296L) - 1)));
        var vhi = (v >> (int)(32));
        if (vlo != vhi) {
            Ꮡt.Fatalf("Pointer: %#x != %#x"u8, vlo, vhi);
        }
        var inc = (uint64)(1 + 4294967296L);
        @new = v + (uintptr)inc;
    }
    addr.Store((ж<byte>)(uintptr)((@unsafe.Pointer)@new));
}

public static partial void TestHammerStoreLoad(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        var tests = new Action<ж<testing.T>, @unsafe.Pointer>[]{
            hammerStoreLoadInt32, hammerStoreLoadUint32,
            hammerStoreLoadUintptr, hammerStoreLoadPointer,
            hammerStoreLoadInt32Method, hammerStoreLoadUint32Method,
            hammerStoreLoadUintptrMethod, hammerStoreLoadPointerMethod,
            hammerStoreLoadInt64, hammerStoreLoadUint64,
            hammerStoreLoadInt64Method, hammerStoreLoadUint64Method
        }.slice();
        nint n = (nint)1000000;
        if (testing.Short()) {
            n = (nint)10000;
        }
        const nint procs = 8;
        defer(runtime.GOMAXPROCS, runtime.GOMAXPROCS(procs), ref ᒐ);
        // Disable the GC because hammerStoreLoadPointer invokes
        // write barriers on values that aren't real pointers.
        defer(debug.SetGCPercent, debug.SetGCPercent(-1), ref ᒐ);
        // Ensure any in-progress GC is finished.
        runtime.GC();
        foreach (var (_, tt) in tests) {
            var c = new channel<nint>(0);
            ref var val = ref heap(new uint64(), out var Ꮡval);
            for (nint p = 0; p < procs; p++) {
                var cʗ1 = c;
                var ttʗ1 = tt;
                goǃ(() => {
                    for (nint i = 0; i < n; i++) {
                        ttʗ1(Ꮡt, @unsafe.Pointer.FromPinnedBox(Ꮡval));
                    }
                    cʗ1.ᐸꟷ(1);
                });
            }
            for (nint p = 0; p < procs; p++) {
                ᐸꟷ(c);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static partial void TestStoreLoadSeqCst32(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        if (runtime.NumCPU() == 1) {
            Ꮡt.Skipf("Skipping test on %v processor machine"u8, runtime.NumCPU());
        }
        defer(runtime.GOMAXPROCS, runtime.GOMAXPROCS(4), ref ᒐ);
        var N = (int32)1000;
        if (testing.Short()) {
            N = (int32)100;
        }
        var c = new channel<bool>(2);
        ref var X = ref heap<array<int32>>(out var ᏑX);
        X = new int32[]{}.array(2);
        ref var ack = ref heap<array<array<int32>>>(out var Ꮡack);
        ack = new array<int32>[]{new int32[]{-1, -1, -1}.array(), new int32[]{-1, -1, -1}.array()}.array();
        for (nint p = 0; p < 2; p++) {
            var cʗ1 = c;
            goǃ((nint me) => {
                nint he = 1 - me;
                for (var i = (int32)1; i < N; i++) {
                    StoreInt32(ᏑX.at<int32>(me), i);
                    var my = LoadInt32(ᏑX.at<int32>(he));
                    StoreInt32(Ꮡack.at<array<int32>>(me).at<int32>((nint)(i % 3)), my);
                    for (nint w = 1; LoadInt32(Ꮡack.at<array<int32>>(he).at<int32>((nint)(i % 3))) == -1; w++) {
                        if (w % 1000 == 0) {
                            runtime.Gosched();
                        }
                    }
                    var his = LoadInt32(Ꮡack.at<array<int32>>(he).at<int32>((nint)(i % 3)));
                    if ((my != i && my != i - 1) || (his != i && his != i - 1)) {
                        Ꮡt.Errorf("invalid values: %d/%d (%d)"u8, my, his, i);
                        break;
                    }
                    if (my != i && his != i) {
                        Ꮡt.Errorf("store/load are not sequentially consistent: %d/%d (%d)"u8, my, his, i);
                        break;
                    }
                    StoreInt32(Ꮡack.at<array<int32>>(me).at<int32>((nint)((i - 1) % 3)), -1);
                }
                cʗ1.ᐸꟷ(true);
            }, p);
        }
        ᐸꟷ(c);
        ᐸꟷ(c);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static partial void TestStoreLoadSeqCst64(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        if (runtime.NumCPU() == 1) {
            Ꮡt.Skipf("Skipping test on %v processor machine"u8, runtime.NumCPU());
        }
        defer(runtime.GOMAXPROCS, runtime.GOMAXPROCS(4), ref ᒐ);
        var N = (int64)1000;
        if (testing.Short()) {
            N = (int64)100;
        }
        var c = new channel<bool>(2);
        ref var X = ref heap<array<int64>>(out var ᏑX);
        X = new int64[]{}.array(2);
        ref var ack = ref heap<array<array<int64>>>(out var Ꮡack);
        ack = new array<int64>[]{new int64[]{-1, -1, -1}.array(), new int64[]{-1, -1, -1}.array()}.array();
        for (nint p = 0; p < 2; p++) {
            var cʗ1 = c;
            goǃ((nint me) => {
                nint he = 1 - me;
                for (var i = (int64)1; i < N; i++) {
                    StoreInt64(ᏑX.at<int64>(me), i);
                    var my = LoadInt64(ᏑX.at<int64>(he));
                    StoreInt64(Ꮡack.at<array<int64>>(me).at<int64>((nint)(i % 3)), my);
                    for (nint w = 1; LoadInt64(Ꮡack.at<array<int64>>(he).at<int64>((nint)(i % 3))) == -1; w++) {
                        if (w % 1000 == 0) {
                            runtime.Gosched();
                        }
                    }
                    var his = LoadInt64(Ꮡack.at<array<int64>>(he).at<int64>((nint)(i % 3)));
                    if ((my != i && my != i - 1) || (his != i && his != i - 1)) {
                        Ꮡt.Errorf("invalid values: %d/%d (%d)"u8, my, his, i);
                        break;
                    }
                    if (my != i && his != i) {
                        Ꮡt.Errorf("store/load are not sequentially consistent: %d/%d (%d)"u8, my, his, i);
                        break;
                    }
                    StoreInt64(Ꮡack.at<array<int64>>(me).at<int64>((nint)((i - 1) % 3)), -1);
                }
                cʗ1.ᐸꟷ(true);
            }, p);
        }
        ᐸꟷ(c);
        ᐸꟷ(c);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoType("dyn")] internal partial struct TestStoreLoadRelAcq32_Data {
    internal int32 signal;
    internal array<int8> pad1 = new(128);
    internal int32 data1;
    internal array<int8> pad2 = new(128);
    internal float32 data2;
}

public static partial void TestStoreLoadRelAcq32(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        if (runtime.NumCPU() == 1) {
            Ꮡt.Skipf("Skipping test on %v processor machine"u8, runtime.NumCPU());
        }
        defer(runtime.GOMAXPROCS, runtime.GOMAXPROCS(4), ref ᒐ);
        var N = (int32)1000;
        if (testing.Short()) {
            N = (int32)100;
        }
        var c = new channel<bool>(2);
        ref var X = ref heap(new TestStoreLoadRelAcq32_Data(), out var ᏑX);
        for (var p = (int32)0; p < 2; p++) {
            var cʗ1 = c;
            goǃ((int32 pΔ1) => {
                for (var i = (int32)1; i < N; i++) {
                    if ((i + pΔ1) % 2 == 0){
                        ᏑX.Value.data1 = i;
                        ᏑX.Value.data2 = (float32)i;
                        StoreInt32(ᏑX.of(TestStoreLoadRelAcq32_Data.Ꮡsignal), i);
                    } else {
                        for (nint w = 1; LoadInt32(ᏑX.of(TestStoreLoadRelAcq32_Data.Ꮡsignal)) != i; w++) {
                            if (w % 1000 == 0) {
                                runtime.Gosched();
                            }
                        }
                        var d1 = ᏑX.Value.data1;
                        var d2 = ᏑX.Value.data2;
                        if (d1 != i || d2 != (float32)i) {
                            Ꮡt.Errorf("incorrect data: %d/%g (%d)"u8, d1, d2, i);
                            break;
                        }
                    }
                }
                cʗ1.ᐸꟷ(true);
            }, p);
        }
        ᐸꟷ(c);
        ᐸꟷ(c);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoType("dyn")] internal partial struct TestStoreLoadRelAcq64_Data {
    internal int64 signal;
    internal array<int8> pad1 = new(128);
    internal int64 data1;
    internal array<int8> pad2 = new(128);
    internal float64 data2;
}

public static partial void TestStoreLoadRelAcq64(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        if (runtime.NumCPU() == 1) {
            Ꮡt.Skipf("Skipping test on %v processor machine"u8, runtime.NumCPU());
        }
        defer(runtime.GOMAXPROCS, runtime.GOMAXPROCS(4), ref ᒐ);
        var N = (int64)1000;
        if (testing.Short()) {
            N = (int64)100;
        }
        var c = new channel<bool>(2);
        ref var X = ref heap(new TestStoreLoadRelAcq64_Data(), out var ᏑX);
        for (var p = (int64)0; p < 2; p++) {
            var cʗ1 = c;
            goǃ((int64 pΔ1) => {
                for (var i = (int64)1; i < N; i++) {
                    if ((i + pΔ1) % 2 == 0){
                        ᏑX.Value.data1 = i;
                        ᏑX.Value.data2 = (float64)i;
                        StoreInt64(ᏑX.of(TestStoreLoadRelAcq64_Data.Ꮡsignal), i);
                    } else {
                        for (nint w = 1; LoadInt64(ᏑX.of(TestStoreLoadRelAcq64_Data.Ꮡsignal)) != i; w++) {
                            if (w % 1000 == 0) {
                                runtime.Gosched();
                            }
                        }
                        var d1 = ᏑX.Value.data1;
                        var d2 = ᏑX.Value.data2;
                        if (d1 != i || d2 != (float64)i) {
                            Ꮡt.Errorf("incorrect data: %d/%g (%d)"u8, d1, d2, i);
                            break;
                        }
                    }
                }
                cʗ1.ᐸꟷ(true);
            }, p);
        }
        ᐸꟷ(c);
        ᐸꟷ(c);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string unaligned64BitAtomicˢ = "unaligned 64-bit atomic operation"u8;

internal static void shouldPanic(ж<testing.T> Ꮡt, @string name, Action f) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            // Check that all GC maps are sane.
            runtime.GC();
            var err = recover();
            @string want = unaligned64BitAtomicˢ;
            if (err == default!){
                Ꮡt.Errorf("%s did not panic"u8, name);
            } else 
            {
                var (s, _) = err._<@string>(ᐧ); if (s != want) {
                    Ꮡt.Errorf("%s: wanted panic %q, got %q"u8, name, want, err);
                }
            }
        }, ref ᒐ);
        f();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object testOnlyRunsOn32Bitˢ = (@string)"test only runs on 32-bit systems"u8;
private static readonly @string loadUint64ˢ = "LoadUint64"u8;
private static readonly @string loadUint64Methodˢ = "LoadUint64Method"u8;
private static readonly @string storeUint64ˢ = "StoreUint64"u8;
private static readonly @string storeUint64Methodˢ = "StoreUint64Method"u8;
private static readonly @string compareAndSwapUint64ˢ = "CompareAndSwapUint64"u8;
private static readonly @string addUint64ˢ = "AddUint64"u8;
private static readonly @string addUint64Methodˢ = "AddUint64Method"u8;

public static void TestUnaligned64(ж<testing.T> Ꮡt) {
    // Unaligned 64-bit atomics on 32-bit systems are
    // a continual source of pain. Test that on 32-bit systems they crash
    // instead of failing silently.
    if (!arch32) {
        Ꮡt.Skip(testOnlyRunsOn32Bitˢ);
    }
    var x = new slice<uint32>(4);
    var p = Ꮡ(x, 1).Reinterpret<uint32, uint64>(); // misaligned
    var pʗ1 = p;
    shouldPanic(Ꮡt, loadUint64ˢ, () => {
        LoadUint64(pʗ1);
    });
    var pʗ2 = p;
    shouldPanic(Ꮡt, loadUint64Methodˢ, () => {
        (pʗ2.Reinterpret<uint64, atomic.Uint64>()).Load();
    });
    var pʗ3 = p;
    shouldPanic(Ꮡt, storeUint64ˢ, () => {
        StoreUint64(pʗ3, 1);
    });
    var pʗ4 = p;
    shouldPanic(Ꮡt, storeUint64Methodˢ, () => {
        (pʗ4.Reinterpret<uint64, atomic.Uint64>()).Store(1);
    });
    var pʗ5 = p;
    shouldPanic(Ꮡt, compareAndSwapUint64ˢ, () => {
        CompareAndSwapUint64(pʗ5, 1, 2);
    });
    var pʗ6 = p;
    shouldPanic(Ꮡt, "CompareAndSwapUint64Method"u8, () => {
        (pʗ6.Reinterpret<uint64, atomic.Uint64>()).CompareAndSwap(1, 2);
    });
    var pʗ7 = p;
    shouldPanic(Ꮡt, addUint64ˢ, () => {
        AddUint64(pʗ7, 3);
    });
    var pʗ8 = p;
    shouldPanic(Ꮡt, addUint64Methodˢ, () => {
        (pʗ8.Reinterpret<uint64, atomic.Uint64>()).Add(3);
    });
}

[GoType("dyn")] internal partial struct TestAutoAligned64_signed {
    internal uint32 _;
    internal atomic.Int64 i;
}

[GoType("dyn")] internal partial struct TestAutoAligned64_unsigned {
    internal uint32 _;
    internal atomic.Uint64 i;
}

public static void TestAutoAligned64(ж<testing.T> Ꮡt) {
    ref var signed = ref heap(new TestAutoAligned64_signed(), out var Ꮡsigned);
    {
        var o = reflect.TypeOf(Ꮡsigned).Elem().Field(1).Offset; if (o != 8) {
            Ꮡt.Fatalf("Int64 offset = %d, want 8"u8, o);
        }
    }
    {
        var p = reflect.ValueOf(Ꮡsigned).Elem().Field(1).Addr().Pointer(); if ((uintptr)(p & 7) != 0) {
            Ꮡt.Fatalf("Int64 pointer = %#x, want 8-aligned"u8, p);
        }
    }
    ref var unsigned = ref heap(new TestAutoAligned64_unsigned(), out var Ꮡunsigned);
    {
        var o = reflect.TypeOf(Ꮡunsigned).Elem().Field(1).Offset; if (o != 8) {
            Ꮡt.Fatalf("Uint64 offset = %d, want 8"u8, o);
        }
    }
    {
        var p = reflect.ValueOf(Ꮡunsigned).Elem().Field(1).Addr().Pointer(); if ((uintptr)(p & 7) != 0) {
            Ꮡt.Fatalf("Int64 pointer = %#x, want 8-aligned"u8, p);
        }
    }
}

public static void TestNilDeref(ж<testing.T> Ꮡt) {
    var funcs = new Action[]{
        () => {
            CompareAndSwapInt32(nil, 0, 0);
        },
        () => {
            (((ж<atomic.Int32>)nil)).CompareAndSwap(0, 0);
        },
        () => {
            CompareAndSwapInt64(nil, 0, 0);
        },
        () => {
            (((ж<atomic.Int64>)nil)).CompareAndSwap(0, 0);
        },
        () => {
            CompareAndSwapUint32(nil, 0, 0);
        },
        () => {
            (((ж<atomic.Uint32>)nil)).CompareAndSwap(0, 0);
        },
        () => {
            CompareAndSwapUint64(nil, 0, 0);
        },
        () => {
            (((ж<atomic.Uint64>)nil)).CompareAndSwap(0, 0);
        },
        () => {
            CompareAndSwapUintptr(nil, 0, 0);
        },
        () => {
            (((ж<atomic.Uintptr>)nil)).CompareAndSwap(0, 0);
        },
        () => {
            CompareAndSwapPointer(nil, nil, nil);
        },
        () => {
            (((ж<atomic.Pointer<byte>>)nil)).CompareAndSwap(nil, nil);
        },
        () => {
            SwapInt32(nil, 0);
        },
        () => {
            (((ж<atomic.Int32>)nil)).Swap(0);
        },
        () => {
            SwapUint32(nil, 0);
        },
        () => {
            (((ж<atomic.Uint32>)nil)).Swap(0);
        },
        () => {
            SwapInt64(nil, 0);
        },
        () => {
            (((ж<atomic.Int64>)nil)).Swap(0);
        },
        () => {
            SwapUint64(nil, 0);
        },
        () => {
            (((ж<atomic.Uint64>)nil)).Swap(0);
        },
        () => {
            SwapUintptr(nil, 0);
        },
        () => {
            (((ж<atomic.Uintptr>)nil)).Swap(0);
        },
        () => {
            SwapPointer(nil, nil);
        },
        () => {
            (((ж<atomic.Pointer<byte>>)nil)).Swap(nil);
        },
        () => {
            AddInt32(nil, 0);
        },
        () => {
            (((ж<atomic.Int32>)nil)).Add(0);
        },
        () => {
            AddUint32(nil, 0);
        },
        () => {
            (((ж<atomic.Uint32>)nil)).Add(0);
        },
        () => {
            AddInt64(nil, 0);
        },
        () => {
            (((ж<atomic.Int64>)nil)).Add(0);
        },
        () => {
            AddUint64(nil, 0);
        },
        () => {
            (((ж<atomic.Uint64>)nil)).Add(0);
        },
        () => {
            AddUintptr(nil, 0);
        },
        () => {
            (((ж<atomic.Uintptr>)nil)).Add(0);
        },
        () => {
            LoadInt32(nil);
        },
        () => {
            (((ж<atomic.Int32>)nil)).Load();
        },
        () => {
            LoadInt64(nil);
        },
        () => {
            (((ж<atomic.Int64>)nil)).Load();
        },
        () => {
            LoadUint32(nil);
        },
        () => {
            (((ж<atomic.Uint32>)nil)).Load();
        },
        () => {
            LoadUint64(nil);
        },
        () => {
            (((ж<atomic.Uint64>)nil)).Load();
        },
        () => {
            LoadUintptr(nil);
        },
        () => {
            (((ж<atomic.Uintptr>)nil)).Load();
        },
        () => {
            LoadPointer(nil);
        },
        () => {
            (((ж<atomic.Pointer<byte>>)nil)).Load();
        },
        () => {
            StoreInt32(nil, 0);
        },
        () => {
            (((ж<atomic.Int32>)nil)).Store(0);
        },
        () => {
            StoreInt64(nil, 0);
        },
        () => {
            (((ж<atomic.Int64>)nil)).Store(0);
        },
        () => {
            StoreUint32(nil, 0);
        },
        () => {
            (((ж<atomic.Uint32>)nil)).Store(0);
        },
        () => {
            StoreUint64(nil, 0);
        },
        () => {
            (((ж<atomic.Uint64>)nil)).Store(0);
        },
        () => {
            StoreUintptr(nil, 0);
        },
        () => {
            (((ж<atomic.Uintptr>)nil)).Store(0);
        },
        () => {
            StorePointer(nil, nil);
        },
        () => {
            (((ж<atomic.Pointer<byte>>)nil)).Store(nil);
        }
    }.array();
    foreach (var (_, f) in funcs.ΔRangeSnapshot()) {
        var fʗ1 = f;
        ((Action)(() => {
            GoFrame ᒐ = default;
            try {
                defer(() => {
                    runtime.GC();
                    recover();
                }, ref ᒐ);
                fʗ1();
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }))();
    }
}

// Test that this compiles.
// When atomic.Pointer used _ [0]T, it did not.
[GoType] partial struct List {
    public atomic.Pointer<List> Next;
}

} // end atomic_test_package
