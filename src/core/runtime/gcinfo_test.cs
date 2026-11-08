// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using bytes = bytes_package;
using Δruntime = runtime_package;
using testing = testing_package;
using static global::go.runtime_internal_test_package;

partial class runtime_test_package {

internal static UntypedInt typeScalar => 0;
internal static UntypedInt typePointer => 1;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string bssPtrˢ => ref bssPtrˢᵇ;
private static readonly @string bssPtrˢᵇ = "bss Ptr"u8;
internal static ref readonly @string bssScalarPtrˢ => ref bssScalarPtrˢᵇ;
private static readonly @string bssScalarPtrˢᵇ = "bss ScalarPtr"u8;
internal static ref readonly @string bssPtrScalarˢ => ref bssPtrScalarˢᵇ;
private static readonly @string bssPtrScalarˢᵇ = "bss PtrScalar"u8;
internal static ref readonly @string bssBigStructˢ => ref bssBigStructˢᵇ;
private static readonly @string bssBigStructˢᵇ = "bss BigStruct"u8;
internal static ref readonly @string bssStringˢ => ref bssStringˢᵇ;
private static readonly @string bssStringˢᵇ = "bss string"u8;
internal static ref readonly @string bssSliceˢ => ref bssSliceˢᵇ;
private static readonly @string bssSliceˢᵇ = "bss slice"u8;
internal static ref readonly @string bssEfaceˢ => ref bssEfaceˢᵇ;
private static readonly @string bssEfaceˢᵇ = "bss eface"u8;
internal static ref readonly @string bssIfaceˢ => ref bssIfaceˢᵇ;
private static readonly @string bssIfaceˢᵇ = "bss iface"u8;
internal static ref readonly @string dataPtrˢ => ref dataPtrˢᵇ;
private static readonly @string dataPtrˢᵇ = "data Ptr"u8;
internal static ref readonly @string dataScalarPtrˢ => ref dataScalarPtrˢᵇ;
private static readonly @string dataScalarPtrˢᵇ = "data ScalarPtr"u8;
internal static ref readonly @string dataPtrScalarˢ => ref dataPtrScalarˢᵇ;
private static readonly @string dataPtrScalarˢᵇ = "data PtrScalar"u8;
internal static ref readonly @string dataBigStructˢ => ref dataBigStructˢᵇ;
private static readonly @string dataBigStructˢᵇ = "data BigStruct"u8;
internal static ref readonly @string dataStringˢ => ref dataStringˢᵇ;
private static readonly @string dataStringˢᵇ = "data string"u8;
internal static ref readonly @string dataSliceˢ => ref dataSliceˢᵇ;
private static readonly @string dataSliceˢᵇ = "data slice"u8;
internal static ref readonly @string dataEfaceˢ => ref dataEfaceˢᵇ;
private static readonly @string dataEfaceˢᵇ = "data eface"u8;
internal static ref readonly @string dataIfaceˢ => ref dataIfaceˢᵇ;
private static readonly @string dataIfaceˢᵇ = "data iface"u8;
internal static ref readonly @string stackPtrˢ => ref stackPtrˢᵇ;
private static readonly @string stackPtrˢᵇ = "stack Ptr"u8;
internal static ref readonly @string stackScalarPtrˢ => ref stackScalarPtrˢᵇ;
private static readonly @string stackScalarPtrˢᵇ = "stack ScalarPtr"u8;
internal static ref readonly @string stackPtrScalarˢ => ref stackPtrScalarˢᵇ;
private static readonly @string stackPtrScalarˢᵇ = "stack PtrScalar"u8;
internal static ref readonly @string stackBigStructˢ => ref stackBigStructˢᵇ;
private static readonly @string stackBigStructˢᵇ = "stack BigStruct"u8;
internal static ref readonly @string stackStringˢ => ref stackStringˢᵇ;
private static readonly @string stackStringˢᵇ = "stack string"u8;
internal static ref readonly @string stackSliceˢ => ref stackSliceˢᵇ;
private static readonly @string stackSliceˢᵇ = "stack slice"u8;
internal static ref readonly @string stackEfaceˢ => ref stackEfaceˢᵇ;
private static readonly @string stackEfaceˢᵇ = "stack eface"u8;
internal static ref readonly @string stackIfaceˢ => ref stackIfaceˢᵇ;
private static readonly @string stackIfaceˢᵇ = "stack iface"u8;
internal static ref readonly @string heapPtrˢ => ref heapPtrˢᵇ;
private static readonly @string heapPtrˢᵇ = "heap Ptr"u8;
internal static ref readonly @string heapPtrSliceˢ => ref heapPtrSliceˢᵇ;
private static readonly @string heapPtrSliceˢᵇ = "heap PtrSlice"u8;
internal static ref readonly @string heapScalarPtrˢ => ref heapScalarPtrˢᵇ;
private static readonly @string heapScalarPtrˢᵇ = "heap ScalarPtr"u8;
internal static ref readonly @string heapScalarPtrSliceˢ => ref heapScalarPtrSliceˢᵇ;
private static readonly @string heapScalarPtrSliceˢᵇ = "heap ScalarPtrSlice"u8;
internal static ref readonly @string heapPtrScalarˢ => ref heapPtrScalarˢᵇ;
private static readonly @string heapPtrScalarˢᵇ = "heap PtrScalar"u8;
internal static ref readonly @string heapBigStructˢ => ref heapBigStructˢᵇ;
private static readonly @string heapBigStructˢᵇ = "heap BigStruct"u8;
internal static ref readonly @string heapStringˢ => ref heapStringˢᵇ;
private static readonly @string heapStringˢᵇ = "heap string"u8;
internal static ref readonly @string heapEfaceˢ => ref heapEfaceˢᵇ;
private static readonly @string heapEfaceˢᵇ = "heap eface"u8;
internal static ref readonly @string heapIfaceˢ => ref heapIfaceˢᵇ;
private static readonly @string heapIfaceˢᵇ = "heap iface"u8;

// TestGCInfo tests that various objects in heap, data and bss receive correct GC pointer type info.
public static void TestGCInfo(ж<testing.T> t) {
    verifyGCInfo(t, bssPtrˢ, ᏑbssPtr, infoPtr);
    verifyGCInfo(t, bssScalarPtrˢ, ᏑbssScalarPtr, infoScalarPtr);
    verifyGCInfo(t, bssPtrScalarˢ, ᏑbssPtrScalar, infoPtrScalar);
    verifyGCInfo(t, bssBigStructˢ, ᏑbssBigStruct, infoBigStruct());
    verifyGCInfo(t, bssStringˢ, ᏑbssString, infoString);
    verifyGCInfo(t, bssSliceˢ, ᏑbssSlice, infoSlice);
    verifyGCInfo(t, bssEfaceˢ, ᏑbssEface, infoEface);
    verifyGCInfo(t, bssIfaceˢ, ᏑbssIface, infoIface);

    verifyGCInfo(t, dataPtrˢ, ᏑdataPtr, infoPtr);
    verifyGCInfo(t, dataScalarPtrˢ, ᏑdataScalarPtr, infoScalarPtr);
    verifyGCInfo(t, dataPtrScalarˢ, ᏑdataPtrScalar, infoPtrScalar);
    verifyGCInfo(t, dataBigStructˢ, ᏑdataBigStruct, infoBigStruct());
    verifyGCInfo(t, dataStringˢ, ᏑdataString, infoString);
    verifyGCInfo(t, dataSliceˢ, ᏑdataSlice, infoSlice);
    verifyGCInfo(t, dataEfaceˢ, ᏑdataEface, infoEface);
    verifyGCInfo(t, dataIfaceˢ, ᏑdataIface, infoIface);

    {
        ref var x = ref heap(new Ptr(), out var Ꮡx);
        verifyGCInfo(t, stackPtrˢ, Ꮡx, infoPtr);
        Δruntime.KeepAlive(x);
    }
    {
        ref var x = ref heap(new ScalarPtr(), out var Ꮡx);
        verifyGCInfo(t, stackScalarPtrˢ, Ꮡx, infoScalarPtr);
        Δruntime.KeepAlive(x);
    }
    {
        ref var x = ref heap(new PtrScalar(), out var Ꮡx);
        verifyGCInfo(t, stackPtrScalarˢ, Ꮡx, infoPtrScalar);
        Δruntime.KeepAlive(x);
    }
    {
        ref var x = ref heap(new BigStruct(), out var Ꮡx);
        verifyGCInfo(t, stackBigStructˢ, Ꮡx, infoBigStruct());
        Δruntime.KeepAlive(x);
    }
    {
        ref var x = ref heap(new @string(), out var Ꮡx);
        verifyGCInfo(t, stackStringˢ, Ꮡx, infoString);
        Δruntime.KeepAlive(x);
    }
    {
        ref var x = ref heap<slice<@string>>(out var Ꮡx);
        verifyGCInfo(t, stackSliceˢ, Ꮡx, infoSlice);
        Δruntime.KeepAlive(x);
    }
    {
        ref var x = ref heap<any>(out var Ꮡx);
        verifyGCInfo(t, stackEfaceˢ, Ꮡx, infoEface);
        Δruntime.KeepAlive(x);
    }
    {
        ref var x = ref heap<Iface>(out var Ꮡx);
        verifyGCInfo(t, stackIfaceˢ, Ꮡx, infoIface);
        Δruntime.KeepAlive(x);
    }

    for (nint i = 0; i < 10; i++) {
        verifyGCInfo(t, heapPtrˢ, runtime_internal_test_package.Escape(@new<Ptr>()).OrTypedNil(), trimDead(infoPtr));
        verifyGCInfo(t, heapPtrSliceˢ, runtime_internal_test_package.Escape(Ꮡ(new slice<ж<byte>>(10), 0)).OrTypedNil(), trimDead(infoPtr10));
        verifyGCInfo(t, heapScalarPtrˢ, runtime_internal_test_package.Escape(@new<ScalarPtr>()).OrTypedNil(), trimDead(infoScalarPtr));
        verifyGCInfo(t, heapScalarPtrSliceˢ, runtime_internal_test_package.Escape(Ꮡ(new slice<ScalarPtr>(4), 0)).OrTypedNil(), trimDead(infoScalarPtr4));
        verifyGCInfo(t, heapPtrScalarˢ, runtime_internal_test_package.Escape(@new<PtrScalar>()).OrTypedNil(), trimDead(infoPtrScalar));
        verifyGCInfo(t, heapBigStructˢ, runtime_internal_test_package.Escape(@new<BigStruct>()).OrTypedNil(), trimDead(infoBigStruct()));
        verifyGCInfo(t, heapStringˢ, runtime_internal_test_package.Escape(@new<@string>()).OrTypedNil(), trimDead(infoString));
        verifyGCInfo(t, heapEfaceˢ, runtime_internal_test_package.Escape(@new<any>()).OrTypedNil(), trimDead(infoEface));
        verifyGCInfo(t, heapIfaceˢ, runtime_internal_test_package.Escape(@new<Iface>()).OrTypedNil(), trimDead(infoIface));
    }
}

internal static void verifyGCInfo(ж<testing.T> t, @string name, any p, slice<byte> mask0) {
    var mask = runtime_internal_test_package.PointerMask(p);
    if (bytes.HasPrefix(mask, mask0)) {
        // Just the prefix matching is OK.
        //
        // The Go runtime's pointer/scalar iterator generates pointers beyond
        // the size of the type, up to the size of the size class. This space
        // is safe for the GC to scan since it's zero, and GCBits checks to
        // make sure that's true. But we need to handle the fact that the bitmap
        // may be larger than we expect.
        return;
    }
    t.Errorf("bad GC program for %v:\nwant %+v\ngot  %+v", name, mask0, mask);
}

internal static slice<byte> trimDead(slice<byte> mask) {
    while (len(mask) > 0 && mask[len(mask) - 1] == typeScalar) {
        mask = mask.slice(0, len(mask) - 1);
    }
    return mask;
}

internal static slice<byte> infoPtr = [typePointer];

partial struct Ptr {
    /*embed*/ internal ж<byte> @byte;
}

internal static slice<byte> infoPtr10 = [typePointer, typePointer, typePointer, typePointer, typePointer, typePointer, typePointer, typePointer, typePointer, typePointer];

partial struct ScalarPtr {
    internal nint q;
    internal ж<nint> w;
    internal nint e;
    internal ж<nint> r;
    internal nint t;
    internal ж<nint> y;
}

internal static slice<byte> infoScalarPtr = [typeScalar, typePointer, typeScalar, typePointer, typeScalar, typePointer];

internal static slice<byte> infoScalarPtr4 = appendꓸꓸꓸ(appendꓸꓸꓸ(appendꓸꓸꓸ(appendꓸꓸꓸ(slice<byte>(default), infoScalarPtr), infoScalarPtr), infoScalarPtr), infoScalarPtr);

partial struct PtrScalar {
    internal ж<nint> q;
    internal nint w;
    internal ж<nint> e;
    internal nint r;
    internal ж<nint> t;
    internal nint y;
}

internal static slice<byte> infoPtrScalar = [typePointer, typeScalar, typePointer, typeScalar, typePointer, typeScalar];

partial struct BigStruct {
    internal ж<nint> q;
    internal byte w;
    internal array<byte> e = new(17);
    internal slice<byte> r;
    internal nint t;
    internal uint16 y;
    internal uint64 u;
    internal @string i;
}

internal static slice<byte> infoBigStruct() {
    var exprᴛ1 = Δruntime.GOARCH;
    switch (ᐧ) {
    case {} when exprᴛ1 == "386"u8 || exprᴛ1 == "arm"u8 || exprᴛ1 == "mips"u8 || exprᴛ1 == "mipsle"u8: {
        return [
            typePointer,                                                // q *int
            typeScalar, typeScalar, typeScalar, typeScalar, typeScalar, // w byte; e [17]byte
            typePointer, typeScalar, typeScalar, // r []byte
            typeScalar, typeScalar, typeScalar, typeScalar, // t int; y uint16; u uint64
            typePointer, typeScalar // i string
        ];
    }
    case {} when exprᴛ1 == "arm64"u8 || exprᴛ1 == "amd64"u8 || exprᴛ1 == "loong64"u8 || exprᴛ1 == "mips64"u8 || exprᴛ1 == "mips64le"u8 || exprᴛ1 == "ppc64"u8 || exprᴛ1 == "ppc64le"u8 || exprᴛ1 == "riscv64"u8 || exprᴛ1 == "s390x"u8 || exprᴛ1 == "wasm"u8: {
        return [
            typePointer,                        // q *int
            typeScalar, typeScalar, typeScalar, // w byte; e [17]byte
            typePointer, typeScalar, typeScalar, // r []byte
            typeScalar, typeScalar, typeScalar, // t int; y uint16; u uint64
            typePointer, typeScalar // i string
        ];
    }
    default: {
        throw panic("unknown arch");
        break;
    }}
}

partial interface Iface {
    void f();
}

partial struct IfaceImpl /*num:nint*/;

internal static void f(this IfaceImpl _) {
}

// BSS
internal static readonly ж<Ptr> ᏑbssPtr = new StaticBox<Ptr>(default(Ptr));
internal static ref Ptr bssPtr => ref ((StaticBox<Ptr>)ᏑbssPtr).ValueSlot;
internal static readonly ж<ScalarPtr> ᏑbssScalarPtr = new StaticBox<ScalarPtr>(default(ScalarPtr));
internal static ref ScalarPtr bssScalarPtr => ref ((StaticBox<ScalarPtr>)ᏑbssScalarPtr).ValueSlot;
internal static readonly ж<PtrScalar> ᏑbssPtrScalar = new StaticBox<PtrScalar>(default(PtrScalar));
internal static ref PtrScalar bssPtrScalar => ref ((StaticBox<PtrScalar>)ᏑbssPtrScalar).ValueSlot;
internal static readonly ж<BigStruct> ᏑbssBigStruct = new StaticBox<BigStruct>(new BigStruct());
internal static ref BigStruct bssBigStruct => ref ((StaticBox<BigStruct>)ᏑbssBigStruct).ValueSlot;
internal static readonly ж<@string> ᏑbssString = new StaticBox<@string>(default(@string));
internal static ref @string bssString => ref ((StaticBox<@string>)ᏑbssString).ValueSlot;
internal static readonly ж<slice<@string>> ᏑbssSlice = new StaticBox<slice<@string>>(default(slice<@string>));
internal static ref slice<@string> bssSlice => ref ((StaticBox<slice<@string>>)ᏑbssSlice).ValueSlot;
internal static readonly ж<any> ᏑbssEface = new StaticBox<any>(default(any));
internal static ref any bssEface => ref ((StaticBox<any>)ᏑbssEface).ValueSlot;
internal static readonly ж<Iface> ᏑbssIface = new StaticBox<Iface>(default(Iface));
internal static ref Iface bssIface => ref ((StaticBox<Iface>)ᏑbssIface).ValueSlot;

// DATA
internal static readonly ж<Ptr> ᏑdataPtr = new StaticBox<Ptr>(new Ptr(@new<byte>()));
internal static ref Ptr dataPtr => ref ((StaticBox<Ptr>)ᏑdataPtr).ValueSlot;
internal static readonly ж<ScalarPtr> ᏑdataScalarPtr = new StaticBox<ScalarPtr>(new ScalarPtr(q: 1));
internal static ref ScalarPtr dataScalarPtr => ref ((StaticBox<ScalarPtr>)ᏑdataScalarPtr).ValueSlot;
internal static readonly ж<PtrScalar> ᏑdataPtrScalar = new StaticBox<PtrScalar>(new PtrScalar(w: 1));
internal static ref PtrScalar dataPtrScalar => ref ((StaticBox<PtrScalar>)ᏑdataPtrScalar).ValueSlot;
internal static readonly ж<BigStruct> ᏑdataBigStruct = new StaticBox<BigStruct>(new BigStruct(w: 1));
internal static ref BigStruct dataBigStruct => ref ((StaticBox<BigStruct>)ᏑdataBigStruct).ValueSlot;
internal static readonly ж<@string> ᏑdataString = new StaticBox<@string>("foo");
internal static ref @string dataString => ref ((StaticBox<@string>)ᏑdataString).ValueSlot;
internal static readonly ж<slice<@string>> ᏑdataSlice = new StaticBox<slice<@string>>(["foo"]);
internal static ref slice<@string> dataSlice => ref ((StaticBox<slice<@string>>)ᏑdataSlice).ValueSlot;
internal static readonly ж<any> ᏑdataEface = new StaticBox<any>((nint)42);
internal static ref any dataEface => ref ((StaticBox<any>)ᏑdataEface).ValueSlot;
internal static readonly ж<Iface> ᏑdataIface = new StaticBox<Iface>(((IfaceImpl)42));
internal static ref Iface dataIface => ref ((StaticBox<Iface>)ᏑdataIface).ValueSlot;

internal static slice<byte> infoString = [typePointer, typeScalar];
internal static slice<byte> infoSlice = [typePointer, typeScalar, typeScalar];
internal static slice<byte> infoEface = [typeScalar, typePointer];
internal static slice<byte> infoIface = [typeScalar, typePointer];

} // end runtime_test_package
