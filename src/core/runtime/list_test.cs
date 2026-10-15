// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using Δruntime = runtime_package;
using testing = testing_package;
using @unsafe = unsafe_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using static global::go.runtime_internal_test_package;

partial class runtime_test_package {

partial struct listedVal {
    internal nint val;
    internal global::go.runtime_internal_test_package.ListNode aNode;
    internal global::go.runtime_internal_test_package.ListNode bNode;
}

internal static partial ж<listedVal> newListedVal(nint v) {
    return Ꮡ(new listedVal(
        val: v
    ));
}

public static void TestListPush(ж<testing.T> Ꮡt) {
    global::go.runtime_internal_test_package.ListHead headA = default!;
    headA.Init(/* unsafe.Offsetof(listedVal{}.aNode) */ (uintptr)8);

    var one = newListedVal(1);
    headA.Push(@unsafe.Pointer.FromPinnedBox(one));

    var two = newListedVal(2);
    headA.Push(@unsafe.Pointer.FromPinnedBox(two));

    var three = newListedVal(3);
    headA.Push(@unsafe.Pointer.FromPinnedBox(three));

    @unsafe.Pointer p = (uintptr)headA.Pop();
    var v = (ж<listedVal>)(uintptr)(p);
    if (v == nil) {
        Ꮡt.Fatalf("pop got nil want 3"u8);
    }
    if ((~v).val != 3) {
        Ꮡt.Errorf("pop got %d want 3"u8, (~v).val);
    }

    p = (uintptr)headA.Pop();
    v = (ж<listedVal>)(uintptr)(p);
    if (v == nil) {
        Ꮡt.Fatalf("pop got nil want 2"u8);
    }
    if ((~v).val != 2) {
        Ꮡt.Errorf("pop got %d want 2"u8, (~v).val);
    }

    p = (uintptr)headA.Pop();
    v = (ж<listedVal>)(uintptr)(p);
    if (v == nil) {
        Ꮡt.Fatalf("pop got nil want 1"u8);
    }
    if ((~v).val != 1) {
        Ꮡt.Errorf("pop got %d want 1"u8, (~v).val);
    }

    p = (uintptr)headA.Pop();
    v = (ж<listedVal>)(uintptr)(p);
    if (v != nil) {
        Ꮡt.Fatalf("pop got %+v want nil"u8, v.OrTypedNil());
    }
}

internal static void wantVal(ж<testing.T> Ꮡt, ж<listedVal> Ꮡv, nint i) {
    ref var v = ref Ꮡv.DerefOrNull();

    Ꮡt.Helper();
    if (Ꮡv == nil) {
        Ꮡt.Fatalf("listedVal got nil want %d"u8, i);
    }
    if (v.val != i) {
        Ꮡt.Errorf("pop got %d want %d"u8, v.val, i);
    }
}

public static void TestListRemoveHead(ж<testing.T> Ꮡt) {
    global::go.runtime_internal_test_package.ListHead headA = default!;
    headA.Init(/* unsafe.Offsetof(listedVal{}.aNode) */ (uintptr)8);

    var one = newListedVal(1);
    headA.Push(@unsafe.Pointer.FromPinnedBox(one));

    var two = newListedVal(2);
    headA.Push(@unsafe.Pointer.FromPinnedBox(two));

    var three = newListedVal(3);
    headA.Push(@unsafe.Pointer.FromPinnedBox(three));

    headA.Remove(@unsafe.Pointer.FromPinnedBox(three));

    @unsafe.Pointer p = (uintptr)headA.Pop();
    var v = (ж<listedVal>)(uintptr)(p);
    wantVal(Ꮡt, v, 2);

    p = (uintptr)headA.Pop();
    v = (ж<listedVal>)(uintptr)(p);
    wantVal(Ꮡt, v, 1);

    p = (uintptr)headA.Pop();
    v = (ж<listedVal>)(uintptr)(p);
    if (v != nil) {
        Ꮡt.Fatalf("pop got %+v want nil"u8, v.OrTypedNil());
    }
}

public static void TestListRemoveMiddle(ж<testing.T> Ꮡt) {
    global::go.runtime_internal_test_package.ListHead headA = default!;
    headA.Init(/* unsafe.Offsetof(listedVal{}.aNode) */ (uintptr)8);

    var one = newListedVal(1);
    headA.Push(@unsafe.Pointer.FromPinnedBox(one));

    var two = newListedVal(2);
    headA.Push(@unsafe.Pointer.FromPinnedBox(two));

    var three = newListedVal(3);
    headA.Push(@unsafe.Pointer.FromPinnedBox(three));

    headA.Remove(@unsafe.Pointer.FromPinnedBox(two));

    @unsafe.Pointer p = (uintptr)headA.Pop();
    var v = (ж<listedVal>)(uintptr)(p);
    wantVal(Ꮡt, v, 3);

    p = (uintptr)headA.Pop();
    v = (ж<listedVal>)(uintptr)(p);
    wantVal(Ꮡt, v, 1);

    p = (uintptr)headA.Pop();
    v = (ж<listedVal>)(uintptr)(p);
    if (v != nil) {
        Ꮡt.Fatalf("pop got %+v want nil"u8, v.OrTypedNil());
    }
}

public static void TestListRemoveTail(ж<testing.T> Ꮡt) {
    global::go.runtime_internal_test_package.ListHead headA = default!;
    headA.Init(/* unsafe.Offsetof(listedVal{}.aNode) */ (uintptr)8);

    var one = newListedVal(1);
    headA.Push(@unsafe.Pointer.FromPinnedBox(one));

    var two = newListedVal(2);
    headA.Push(@unsafe.Pointer.FromPinnedBox(two));

    var three = newListedVal(3);
    headA.Push(@unsafe.Pointer.FromPinnedBox(three));

    headA.Remove(@unsafe.Pointer.FromPinnedBox(one));

    @unsafe.Pointer p = (uintptr)headA.Pop();
    var v = (ж<listedVal>)(uintptr)(p);
    wantVal(Ꮡt, v, 3);

    p = (uintptr)headA.Pop();
    v = (ж<listedVal>)(uintptr)(p);
    wantVal(Ꮡt, v, 2);

    p = (uintptr)headA.Pop();
    v = (ж<listedVal>)(uintptr)(p);
    if (v != nil) {
        Ꮡt.Fatalf("pop got %+v want nil"u8, v.OrTypedNil());
    }
}

public static void TestListRemoveAll(ж<testing.T> Ꮡt) {
    global::go.runtime_internal_test_package.ListHead headA = default!;
    headA.Init(/* unsafe.Offsetof(listedVal{}.aNode) */ (uintptr)8);

    var one = newListedVal(1);
    headA.Push(@unsafe.Pointer.FromPinnedBox(one));

    var two = newListedVal(2);
    headA.Push(@unsafe.Pointer.FromPinnedBox(two));

    var three = newListedVal(3);
    headA.Push(@unsafe.Pointer.FromPinnedBox(three));

    headA.Remove(@unsafe.Pointer.FromPinnedBox(one));
    headA.Remove(@unsafe.Pointer.FromPinnedBox(two));
    headA.Remove(@unsafe.Pointer.FromPinnedBox(three));

    @unsafe.Pointer p = (uintptr)headA.Pop();
    var v = (ж<listedVal>)(uintptr)(p);
    if (v != nil) {
        Ꮡt.Fatalf("pop got %+v want nil"u8, v.OrTypedNil());
    }
}

public static void BenchmarkListPushPop(ж<testing.B> Ꮡb) {
    global::go.runtime_internal_test_package.ListHead head = default!;
    head.Init(/* unsafe.Offsetof(listedVal{}.aNode) */ (uintptr)8);

    var vals = new slice<listedVal>(10000);
    nint i = 0;
    while (Ꮡb.Loop()) {
        if (i == len(vals)) {
            foreach (var _ᴛ1 in range(len(vals))) {
                head.Pop();
            }
            i = 0;
        }

        head.Push(@unsafe.Pointer.FromPinnedBox(Ꮡ(vals, i)));

        i++;
    }
}

} // end runtime_test_package
