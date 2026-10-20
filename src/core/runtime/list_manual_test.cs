// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using sys = @internal.runtime.sys_package;
using Δruntime = runtime_package;
using testing = testing_package;
using @unsafe = unsafe_package;
using @internal.runtime;
using FieldOffsetAttribute = global::System.Runtime.InteropServices.FieldOffsetAttribute;
using LayoutKind = global::System.Runtime.InteropServices.LayoutKind;
using StructLayoutAttribute = global::System.Runtime.InteropServices.StructLayoutAttribute;
using static global::go.runtime_internal_test_package;

partial class runtime_test_package {

// The tests in this file are identical to list_test.go, but for the
// manually-managed variants.
partial struct listedValManual {
    internal nint val;
    internal global::go.runtime_internal_test_package.ListNodeManual aNode;
    internal global::go.runtime_internal_test_package.ListNodeManual bNode;
}

// ListHeadManual is intended to be used with objects where the lifetime of the
// object is managed explicitly, so it is OK to store as uintptr.
//
// This means that our test values must outlive the test, and must not live on
// the stack (which may move).
internal static slice<ж<listedValManual>> allListedValManual;

internal static ж<listedValManual> newListedValManual(nint v) {
    var lv = Ꮡ(new listedValManual(
        val: v
    ));
    allListedValManual = append(allListedValManual, lv);
    return lv;
}

public static void TestListManualPush(ж<testing.T> Ꮡt) {
    global::go.runtime_internal_test_package.ListHeadManual headA = default!;
    headA.Init(/* unsafe.Offsetof(listedValManual{}.aNode) */ (uintptr)8);

    var one = newListedValManual(1);
    headA.Push(@unsafe.Pointer.FromPinnedBox(one));

    var two = newListedValManual(2);
    headA.Push(@unsafe.Pointer.FromPinnedBox(two));

    var three = newListedValManual(3);
    headA.Push(@unsafe.Pointer.FromPinnedBox(three));

    @unsafe.Pointer p = (uintptr)headA.Pop();
    var v = (ж<listedValManual>)(uintptr)(p);
    if (v == nil) {
        Ꮡt.Fatalf("pop got nil want 3"u8);
    }
    if ((~v).val != 3) {
        Ꮡt.Errorf("pop got %d want 3"u8, (~v).val);
    }

    p = (uintptr)headA.Pop();
    v = (ж<listedValManual>)(uintptr)(p);
    if (v == nil) {
        Ꮡt.Fatalf("pop got nil want 2"u8);
    }
    if ((~v).val != 2) {
        Ꮡt.Errorf("pop got %d want 2"u8, (~v).val);
    }

    p = (uintptr)headA.Pop();
    v = (ж<listedValManual>)(uintptr)(p);
    if (v == nil) {
        Ꮡt.Fatalf("pop got nil want 1"u8);
    }
    if ((~v).val != 1) {
        Ꮡt.Errorf("pop got %d want 1"u8, (~v).val);
    }

    p = (uintptr)headA.Pop();
    v = (ж<listedValManual>)(uintptr)(p);
    if (v != nil) {
        Ꮡt.Fatalf("pop got %+v want nil"u8, v.OrTypedNil());
    }

    Δruntime.KeepAlive(one.OrTypedNil());
    Δruntime.KeepAlive(two.OrTypedNil());
    Δruntime.KeepAlive(three.OrTypedNil());
}

internal static void wantValManual(ж<testing.T> Ꮡt, ж<listedValManual> Ꮡv, nint i) {
    ref var v = ref Ꮡv.DerefOrNull();

    Ꮡt.Helper();
    if (Ꮡv == nil) {
        Ꮡt.Fatalf("listedVal got nil want %d"u8, i);
    }
    if (v.val != i) {
        Ꮡt.Errorf("pop got %d want %d"u8, v.val, i);
    }
}

public static void TestListManualRemoveHead(ж<testing.T> Ꮡt) {
    global::go.runtime_internal_test_package.ListHeadManual headA = default!;
    headA.Init(/* unsafe.Offsetof(listedValManual{}.aNode) */ (uintptr)8);

    var one = newListedValManual(1);
    headA.Push(@unsafe.Pointer.FromPinnedBox(one));

    var two = newListedValManual(2);
    headA.Push(@unsafe.Pointer.FromPinnedBox(two));

    var three = newListedValManual(3);
    headA.Push(@unsafe.Pointer.FromPinnedBox(three));

    headA.Remove(@unsafe.Pointer.FromPinnedBox(three));

    @unsafe.Pointer p = (uintptr)headA.Pop();
    var v = (ж<listedValManual>)(uintptr)(p);
    wantValManual(Ꮡt, v, 2);

    p = (uintptr)headA.Pop();
    v = (ж<listedValManual>)(uintptr)(p);
    wantValManual(Ꮡt, v, 1);

    p = (uintptr)headA.Pop();
    v = (ж<listedValManual>)(uintptr)(p);
    if (v != nil) {
        Ꮡt.Fatalf("pop got %+v want nil"u8, v.OrTypedNil());
    }

    Δruntime.KeepAlive(one.OrTypedNil());
    Δruntime.KeepAlive(two.OrTypedNil());
    Δruntime.KeepAlive(three.OrTypedNil());
}

public static void TestListManualRemoveMiddle(ж<testing.T> Ꮡt) {
    global::go.runtime_internal_test_package.ListHeadManual headA = default!;
    headA.Init(/* unsafe.Offsetof(listedValManual{}.aNode) */ (uintptr)8);

    var one = newListedValManual(1);
    headA.Push(@unsafe.Pointer.FromPinnedBox(one));

    var two = newListedValManual(2);
    headA.Push(@unsafe.Pointer.FromPinnedBox(two));

    var three = newListedValManual(3);
    headA.Push(@unsafe.Pointer.FromPinnedBox(three));

    headA.Remove(@unsafe.Pointer.FromPinnedBox(two));

    @unsafe.Pointer p = (uintptr)headA.Pop();
    var v = (ж<listedValManual>)(uintptr)(p);
    wantValManual(Ꮡt, v, 3);

    p = (uintptr)headA.Pop();
    v = (ж<listedValManual>)(uintptr)(p);
    wantValManual(Ꮡt, v, 1);

    p = (uintptr)headA.Pop();
    v = (ж<listedValManual>)(uintptr)(p);
    if (v != nil) {
        Ꮡt.Fatalf("pop got %+v want nil"u8, v.OrTypedNil());
    }

    Δruntime.KeepAlive(one.OrTypedNil());
    Δruntime.KeepAlive(two.OrTypedNil());
    Δruntime.KeepAlive(three.OrTypedNil());
}

public static void TestListManualRemoveTail(ж<testing.T> Ꮡt) {
    global::go.runtime_internal_test_package.ListHeadManual headA = default!;
    headA.Init(/* unsafe.Offsetof(listedValManual{}.aNode) */ (uintptr)8);

    var one = newListedValManual(1);
    headA.Push(@unsafe.Pointer.FromPinnedBox(one));

    var two = newListedValManual(2);
    headA.Push(@unsafe.Pointer.FromPinnedBox(two));

    var three = newListedValManual(3);
    headA.Push(@unsafe.Pointer.FromPinnedBox(three));

    headA.Remove(@unsafe.Pointer.FromPinnedBox(one));

    @unsafe.Pointer p = (uintptr)headA.Pop();
    var v = (ж<listedValManual>)(uintptr)(p);
    wantValManual(Ꮡt, v, 3);

    p = (uintptr)headA.Pop();
    v = (ж<listedValManual>)(uintptr)(p);
    wantValManual(Ꮡt, v, 2);

    p = (uintptr)headA.Pop();
    v = (ж<listedValManual>)(uintptr)(p);
    if (v != nil) {
        Ꮡt.Fatalf("pop got %+v want nil"u8, v.OrTypedNil());
    }

    Δruntime.KeepAlive(one.OrTypedNil());
    Δruntime.KeepAlive(two.OrTypedNil());
    Δruntime.KeepAlive(three.OrTypedNil());
}

public static void TestListManualRemoveAll(ж<testing.T> Ꮡt) {
    global::go.runtime_internal_test_package.ListHeadManual headA = default!;
    headA.Init(/* unsafe.Offsetof(listedValManual{}.aNode) */ (uintptr)8);

    var one = newListedValManual(1);
    headA.Push(@unsafe.Pointer.FromPinnedBox(one));

    var two = newListedValManual(2);
    headA.Push(@unsafe.Pointer.FromPinnedBox(two));

    var three = newListedValManual(3);
    headA.Push(@unsafe.Pointer.FromPinnedBox(three));

    headA.Remove(@unsafe.Pointer.FromPinnedBox(one));
    headA.Remove(@unsafe.Pointer.FromPinnedBox(two));
    headA.Remove(@unsafe.Pointer.FromPinnedBox(three));

    @unsafe.Pointer p = (uintptr)headA.Pop();
    var v = (ж<listedValManual>)(uintptr)(p);
    if (v != nil) {
        Ꮡt.Fatalf("pop got %+v want nil"u8, v.OrTypedNil());
    }

    Δruntime.KeepAlive(one.OrTypedNil());
    Δruntime.KeepAlive(two.OrTypedNil());
    Δruntime.KeepAlive(three.OrTypedNil());
}

// The tests below are identical, but used with a sys.NotInHeap type.
[StructLayout(LayoutKind.Explicit, Size = 40)] partial struct listedValNIH {
    [FieldOffset(0)] internal readonly sys.NotInHeap _;
    [FieldOffset(0)] /*embed*/ internal listedValManual listedValManual;
}

internal static ж<listedValNIH> newListedValNIH(nint v) {
    var l = (ж<listedValNIH>)(uintptr)(runtime_internal_test_package.PersistentAlloc(/* unsafe.Sizeof(listedValNIH{}) */ (uintptr)40, /* unsafe.Alignof(listedValNIH{}) */ (uintptr)8));
    l.Value.val = v;
    return l;
}

internal static ж<global::go.runtime_internal_test_package.ListHeadManual> newListHeadNIH() {
    return (ж<runtime_internal_test_package.ListHeadManual>)(uintptr)(runtime_internal_test_package.PersistentAlloc(/* unsafe.Sizeof(runtime.ListHeadManual{}) */ (uintptr)24, /* unsafe.Alignof(runtime.ListHeadManual{}) */ (uintptr)8));
}

public static void TestListNIHPush(ж<testing.T> Ꮡt) {
    var headA = newListHeadNIH();
    headA.Init(/* unsafe.Offsetof(listedValNIH{}.aNode) */ (uintptr)8);

    var one = newListedValNIH(1);
    headA.Push(@unsafe.Pointer.FromPinnedBox(one));

    var two = newListedValNIH(2);
    headA.Push(@unsafe.Pointer.FromPinnedBox(two));

    var three = newListedValNIH(3);
    headA.Push(@unsafe.Pointer.FromPinnedBox(three));

    @unsafe.Pointer p = (uintptr)headA.Pop();
    var v = (ж<listedValNIH>)(uintptr)(p);
    if (v == nil) {
        Ꮡt.Fatalf("pop got nil want 3"u8);
    }
    if ((~v).val != 3) {
        Ꮡt.Errorf("pop got %d want 3"u8, (~v).val);
    }

    p = (uintptr)headA.Pop();
    v = (ж<listedValNIH>)(uintptr)(p);
    if (v == nil) {
        Ꮡt.Fatalf("pop got nil want 2"u8);
    }
    if ((~v).val != 2) {
        Ꮡt.Errorf("pop got %d want 2"u8, (~v).val);
    }

    p = (uintptr)headA.Pop();
    v = (ж<listedValNIH>)(uintptr)(p);
    if (v == nil) {
        Ꮡt.Fatalf("pop got nil want 1"u8);
    }
    if ((~v).val != 1) {
        Ꮡt.Errorf("pop got %d want 1"u8, (~v).val);
    }

    p = (uintptr)headA.Pop();
    v = (ж<listedValNIH>)(uintptr)(p);
    if (v != nil) {
        Ꮡt.Fatalf("pop got %+v want nil"u8, v.OrTypedNil());
    }
}

internal static void wantValNIH(ж<testing.T> Ꮡt, ж<listedValNIH> Ꮡv, nint i) {
    ref var v = ref Ꮡv.DerefOrNull();

    Ꮡt.Helper();
    if (Ꮡv == nil) {
        Ꮡt.Fatalf("listedVal got nil want %d"u8, i);
    }
    if (v.val != i) {
        Ꮡt.Errorf("pop got %d want %d"u8, v.val, i);
    }
}

public static void TestListNIHRemoveHead(ж<testing.T> Ꮡt) {
    var headA = newListHeadNIH();
    headA.Init(/* unsafe.Offsetof(listedValNIH{}.aNode) */ (uintptr)8);

    var one = newListedValNIH(1);
    headA.Push(@unsafe.Pointer.FromPinnedBox(one));

    var two = newListedValNIH(2);
    headA.Push(@unsafe.Pointer.FromPinnedBox(two));

    var three = newListedValNIH(3);
    headA.Push(@unsafe.Pointer.FromPinnedBox(three));

    headA.Remove(@unsafe.Pointer.FromPinnedBox(three));

    @unsafe.Pointer p = (uintptr)headA.Pop();
    var v = (ж<listedValNIH>)(uintptr)(p);
    wantValNIH(Ꮡt, v, 2);

    p = (uintptr)headA.Pop();
    v = (ж<listedValNIH>)(uintptr)(p);
    wantValNIH(Ꮡt, v, 1);

    p = (uintptr)headA.Pop();
    v = (ж<listedValNIH>)(uintptr)(p);
    if (v != nil) {
        Ꮡt.Fatalf("pop got %+v want nil"u8, v.OrTypedNil());
    }
}

public static void TestListNIHRemoveMiddle(ж<testing.T> Ꮡt) {
    var headA = newListHeadNIH();
    headA.Init(/* unsafe.Offsetof(listedValNIH{}.aNode) */ (uintptr)8);

    var one = newListedValNIH(1);
    headA.Push(@unsafe.Pointer.FromPinnedBox(one));

    var two = newListedValNIH(2);
    headA.Push(@unsafe.Pointer.FromPinnedBox(two));

    var three = newListedValNIH(3);
    headA.Push(@unsafe.Pointer.FromPinnedBox(three));

    headA.Remove(@unsafe.Pointer.FromPinnedBox(two));

    @unsafe.Pointer p = (uintptr)headA.Pop();
    var v = (ж<listedValNIH>)(uintptr)(p);
    wantValNIH(Ꮡt, v, 3);

    p = (uintptr)headA.Pop();
    v = (ж<listedValNIH>)(uintptr)(p);
    wantValNIH(Ꮡt, v, 1);

    p = (uintptr)headA.Pop();
    v = (ж<listedValNIH>)(uintptr)(p);
    if (v != nil) {
        Ꮡt.Fatalf("pop got %+v want nil"u8, v.OrTypedNil());
    }
}

public static void TestListNIHRemoveTail(ж<testing.T> Ꮡt) {
    var headA = newListHeadNIH();
    headA.Init(/* unsafe.Offsetof(listedValNIH{}.aNode) */ (uintptr)8);

    var one = newListedValNIH(1);
    headA.Push(@unsafe.Pointer.FromPinnedBox(one));

    var two = newListedValNIH(2);
    headA.Push(@unsafe.Pointer.FromPinnedBox(two));

    var three = newListedValNIH(3);
    headA.Push(@unsafe.Pointer.FromPinnedBox(three));

    headA.Remove(@unsafe.Pointer.FromPinnedBox(one));

    @unsafe.Pointer p = (uintptr)headA.Pop();
    var v = (ж<listedValNIH>)(uintptr)(p);
    wantValNIH(Ꮡt, v, 3);

    p = (uintptr)headA.Pop();
    v = (ж<listedValNIH>)(uintptr)(p);
    wantValNIH(Ꮡt, v, 2);

    p = (uintptr)headA.Pop();
    v = (ж<listedValNIH>)(uintptr)(p);
    if (v != nil) {
        Ꮡt.Fatalf("pop got %+v want nil"u8, v.OrTypedNil());
    }
}

public static void TestListNIHRemoveAll(ж<testing.T> Ꮡt) {
    var headA = newListHeadNIH();
    headA.Init(/* unsafe.Offsetof(listedValNIH{}.aNode) */ (uintptr)8);

    var one = newListedValNIH(1);
    headA.Push(@unsafe.Pointer.FromPinnedBox(one));

    var two = newListedValNIH(2);
    headA.Push(@unsafe.Pointer.FromPinnedBox(two));

    var three = newListedValNIH(3);
    headA.Push(@unsafe.Pointer.FromPinnedBox(three));

    headA.Remove(@unsafe.Pointer.FromPinnedBox(one));
    headA.Remove(@unsafe.Pointer.FromPinnedBox(two));
    headA.Remove(@unsafe.Pointer.FromPinnedBox(three));

    @unsafe.Pointer p = (uintptr)headA.Pop();
    var v = (ж<listedValNIH>)(uintptr)(p);
    if (v != nil) {
        Ꮡt.Fatalf("pop got %+v want nil"u8, v.OrTypedNil());
    }
}

} // end runtime_test_package
