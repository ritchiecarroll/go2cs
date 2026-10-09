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
internal static readonly @string bssPtrˢ = "bss Ptr"u8;
internal static readonly @string bssScalarPtrˢ = "bss ScalarPtr"u8;
internal static readonly @string bssPtrScalarˢ = "bss PtrScalar"u8;
internal static readonly @string bssBigStructˢ = "bss BigStruct"u8;
internal static readonly @string bssStringˢ = "bss string"u8;
internal static readonly @string bssSliceˢ = "bss slice"u8;
internal static readonly @string bssEfaceˢ = "bss eface"u8;
internal static readonly @string bssIfaceˢ = "bss iface"u8;
internal static readonly @string dataPtrˢ = "data Ptr"u8;
internal static readonly @string dataScalarPtrˢ = "data ScalarPtr"u8;
internal static readonly @string dataPtrScalarˢ = "data PtrScalar"u8;
internal static readonly @string dataBigStructˢ = "data BigStruct"u8;
internal static readonly @string dataStringˢ = "data string"u8;
internal static readonly @string dataSliceˢ = "data slice"u8;
internal static readonly @string dataEfaceˢ = "data eface"u8;
internal static readonly @string dataIfaceˢ = "data iface"u8;
internal static readonly @string stackPtrˢ = "stack Ptr"u8;
internal static readonly @string stackScalarPtrˢ = "stack ScalarPtr"u8;
internal static readonly @string stackPtrScalarˢ = "stack PtrScalar"u8;
internal static readonly @string stackBigStructˢ = "stack BigStruct"u8;
internal static readonly @string stackStringˢ = "stack string"u8;
internal static readonly @string stackSliceˢ = "stack slice"u8;
internal static readonly @string stackEfaceˢ = "stack eface"u8;
internal static readonly @string stackIfaceˢ = "stack iface"u8;
internal static readonly @string heapPtrˢ = "heap Ptr"u8;
internal static readonly @string heapPtrSliceˢ = "heap PtrSlice"u8;
internal static readonly @string heapScalarPtrˢ = "heap ScalarPtr"u8;
internal static readonly @string heapScalarPtrSliceˢ = "heap ScalarPtrSlice"u8;
internal static readonly @string heapPtrScalarˢ = "heap PtrScalar"u8;
internal static readonly @string heapBigStructˢ = "heap BigStruct"u8;
internal static readonly @string heapStringˢ = "heap string"u8;
internal static readonly @string heapEfaceˢ = "heap eface"u8;
internal static readonly @string heapIfaceˢ = "heap iface"u8;

// TestGCInfo tests that various objects in heap, data and bss receive correct GC pointer type info.
public static void TestGCInfo(ж<testing.T> Ꮡt) {
    verifyGCInfo(Ꮡt, bssPtrˢ, ᏑbssPtr, infoPtr);
    verifyGCInfo(Ꮡt, bssScalarPtrˢ, ᏑbssScalarPtr, infoScalarPtr);
    verifyGCInfo(Ꮡt, bssPtrScalarˢ, ᏑbssPtrScalar, infoPtrScalar);
    verifyGCInfo(Ꮡt, bssBigStructˢ, ᏑbssBigStruct, infoBigStruct());
    verifyGCInfo(Ꮡt, bssStringˢ, ᏑbssString, infoString);
    verifyGCInfo(Ꮡt, bssSliceˢ, ᏑbssSlice, infoSlice);
    verifyGCInfo(Ꮡt, bssEfaceˢ, ᏑbssEface, infoEface);
    verifyGCInfo(Ꮡt, bssIfaceˢ, ᏑbssIface, infoIface);
    verifyGCInfo(Ꮡt, dataPtrˢ, ᏑdataPtr, infoPtr);
    verifyGCInfo(Ꮡt, dataScalarPtrˢ, ᏑdataScalarPtr, infoScalarPtr);
    verifyGCInfo(Ꮡt, dataPtrScalarˢ, ᏑdataPtrScalar, infoPtrScalar);
    verifyGCInfo(Ꮡt, dataBigStructˢ, ᏑdataBigStruct, infoBigStruct());
    verifyGCInfo(Ꮡt, dataStringˢ, ᏑdataString, infoString);
    verifyGCInfo(Ꮡt, dataSliceˢ, ᏑdataSlice, infoSlice);
    verifyGCInfo(Ꮡt, dataEfaceˢ, ᏑdataEface, infoEface);
    verifyGCInfo(Ꮡt, dataIfaceˢ, ᏑdataIface, infoIface);
    {
        ref var x = ref heap(new Ptr(), out var Ꮡx);
        verifyGCInfo(Ꮡt, stackPtrˢ, Ꮡx, infoPtr);
        Δruntime.KeepAlive(x);
    }
    {
        ref var x = ref heap(new ScalarPtr(), out var Ꮡx);
        verifyGCInfo(Ꮡt, stackScalarPtrˢ, Ꮡx, infoScalarPtr);
        Δruntime.KeepAlive(x);
    }
    {
        ref var x = ref heap(new PtrScalar(), out var Ꮡx);
        verifyGCInfo(Ꮡt, stackPtrScalarˢ, Ꮡx, infoPtrScalar);
        Δruntime.KeepAlive(x);
    }
    {
        ref var x = ref heap(new BigStruct(), out var Ꮡx);
        verifyGCInfo(Ꮡt, stackBigStructˢ, Ꮡx, infoBigStruct());
        Δruntime.KeepAlive(x);
    }
    {
        ref var x = ref heap(new @string(), out var Ꮡx);
        verifyGCInfo(Ꮡt, stackStringˢ, Ꮡx, infoString);
        Δruntime.KeepAlive(x);
    }
    {
        ref var x = ref heap<slice<@string>>(out var Ꮡx);
        verifyGCInfo(Ꮡt, stackSliceˢ, Ꮡx, infoSlice);
        Δruntime.KeepAlive(x);
    }
    {
        ref var x = ref heap<any>(out var Ꮡx);
        verifyGCInfo(Ꮡt, stackEfaceˢ, Ꮡx, infoEface);
        Δruntime.KeepAlive(x);
    }
    {
        ref var x = ref heap<Iface>(out var Ꮡx);
        verifyGCInfo(Ꮡt, stackIfaceˢ, Ꮡx, infoIface);
        Δruntime.KeepAlive(x);
    }
    for (nint i = 0; i < 10; i++) {
        verifyGCInfo(Ꮡt, heapPtrˢ, runtime_internal_test_package.Escape(@new<Ptr>()).OrTypedNil(), trimDead(infoPtr));
        verifyGCInfo(Ꮡt, heapPtrSliceˢ, runtime_internal_test_package.Escape(Ꮡ(new slice<ж<byte>>(10), 0)).OrTypedNil(), trimDead(infoPtr10));
        verifyGCInfo(Ꮡt, heapScalarPtrˢ, runtime_internal_test_package.Escape(@new<ScalarPtr>()).OrTypedNil(), trimDead(infoScalarPtr));
        verifyGCInfo(Ꮡt, heapScalarPtrSliceˢ, runtime_internal_test_package.Escape(Ꮡ(new slice<ScalarPtr>(4), 0)).OrTypedNil(), trimDead(infoScalarPtr4));
        verifyGCInfo(Ꮡt, heapPtrScalarˢ, runtime_internal_test_package.Escape(@new<PtrScalar>()).OrTypedNil(), trimDead(infoPtrScalar));
        verifyGCInfo(Ꮡt, heapBigStructˢ, runtime_internal_test_package.Escape(@new<BigStruct>()).OrTypedNil(), trimDead(infoBigStruct()));
        verifyGCInfo(Ꮡt, heapStringˢ, runtime_internal_test_package.Escape(@new<@string>()).OrTypedNil(), trimDead(infoString));
        verifyGCInfo(Ꮡt, heapEfaceˢ, runtime_internal_test_package.Escape(@new<any>()).OrTypedNil(), trimDead(infoEface));
        verifyGCInfo(Ꮡt, heapIfaceˢ, runtime_internal_test_package.Escape(@new<Iface>()).OrTypedNil(), trimDead(infoIface));
    }
}

internal static void verifyGCInfo(ж<testing.T> Ꮡt, @string name, any p, slice<byte> mask0) {
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
    Ꮡt.Errorf("bad GC program for %v:\nwant %+v\ngot  %+v"u8, name, mask0, mask);
}

internal static slice<byte> trimDead(slice<byte> mask) {
    while (len(mask) > 0 && mask[len(mask) - 1] == typeScalar) {
        mask = mask.slice(0, len(mask) - 1);
    }
    return mask;
}

internal static slice<byte> infoPtr = new byte[]{typePointer}.slice();

partial struct Ptr {
    /*embed*/ internal ж<byte> @byte;
}

internal static slice<byte> infoPtr10 = new byte[]{typePointer, typePointer, typePointer, typePointer, typePointer, typePointer, typePointer, typePointer, typePointer, typePointer}.slice();

partial struct ScalarPtr {
    internal nint q;
    internal ж<nint> w;
    internal nint e;
    internal ж<nint> r;
    internal nint t;
    internal ж<nint> y;
}

internal static slice<byte> infoScalarPtr = new byte[]{typeScalar, typePointer, typeScalar, typePointer, typeScalar, typePointer}.slice();

internal static slice<byte> infoScalarPtr4 = appendꓸꓸꓸ(appendꓸꓸꓸ(appendꓸꓸꓸ(appendꓸꓸꓸ(slice<byte>(default!), infoScalarPtr), infoScalarPtr), infoScalarPtr), infoScalarPtr);

partial struct PtrScalar {
    internal ж<nint> q;
    internal nint w;
    internal ж<nint> e;
    internal nint r;
    internal ж<nint> t;
    internal nint y;
}

internal static slice<byte> infoPtrScalar = new byte[]{typePointer, typeScalar, typePointer, typeScalar, typePointer, typeScalar}.slice();

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
    if (exprᴛ1 == "386"u8 || exprᴛ1 == "arm"u8 || exprᴛ1 == "mips"u8 || exprᴛ1 == "mipsle"u8) {
        return new byte[]{
            typePointer, // q *int

            typeScalar, typeScalar, typeScalar, typeScalar, typeScalar, // w byte; e [17]byte

            typePointer, typeScalar, typeScalar, // r []byte

            typeScalar, typeScalar, typeScalar, typeScalar, // t int; y uint16; u uint64

            typePointer, typeScalar
        }.slice();
    }
    if (exprᴛ1 == "arm64"u8 || exprᴛ1 == "amd64"u8 || exprᴛ1 == "loong64"u8 || exprᴛ1 == "mips64"u8 || exprᴛ1 == "mips64le"u8 || exprᴛ1 == "ppc64"u8 || exprᴛ1 == "ppc64le"u8 || exprᴛ1 == "riscv64"u8 || exprᴛ1 == "s390x"u8 || exprᴛ1 == "wasm"u8) {
        return new byte[]{ // i string

            typePointer, // q *int

            typeScalar, typeScalar, typeScalar, // w byte; e [17]byte

            typePointer, typeScalar, typeScalar, // r []byte

            typeScalar, typeScalar, typeScalar, // t int; y uint16; u uint64

            typePointer, typeScalar
        }.slice();
    }
    { /* default: */
        throw panic("unknown arch");
    }

}

// i string
partial interface Iface {
    void f();
}

partial struct IfaceImpl /*num:nint*/;

internal static void f(this IfaceImpl _) {
}

internal static ж<Ptr> ᏑbssPtr = new StandardBox<Ptr>(default(Ptr));
internal static ref Ptr bssPtr => ref ᏑbssPtr.Value;
internal static ж<ScalarPtr> ᏑbssScalarPtr = new StandardBox<ScalarPtr>(default(ScalarPtr));
internal static ref ScalarPtr bssScalarPtr => ref ᏑbssScalarPtr.Value;
internal static ж<PtrScalar> ᏑbssPtrScalar = new StandardBox<PtrScalar>(default(PtrScalar));
internal static ref PtrScalar bssPtrScalar => ref ᏑbssPtrScalar.Value;
internal static ж<BigStruct> ᏑbssBigStruct = new StandardBox<BigStruct>(new BigStruct());
internal static ref BigStruct bssBigStruct => ref ᏑbssBigStruct.Value;
internal static ж<@string> ᏑbssString = new StandardBox<@string>(default(@string));
internal static ref @string bssString => ref ᏑbssString.Value;
internal static ж<slice<@string>> ᏑbssSlice = new StandardBox<slice<@string>>(default(slice<@string>));
internal static ref slice<@string> bssSlice => ref ᏑbssSlice.ValueSlot;
internal static ж<any> ᏑbssEface = new StandardBox<any>(default(any));
internal static ref any bssEface => ref ᏑbssEface.ValueSlot;
internal static ж<Iface> ᏑbssIface = new StandardBox<Iface>(default(Iface));
internal static ref Iface bssIface => ref ᏑbssIface.ValueSlot;
internal static ж<Ptr> ᏑdataPtr = new StandardBox<Ptr>(new Ptr(@new<byte>()));
internal static ref Ptr dataPtr => ref ᏑdataPtr.Value;
internal static ж<ScalarPtr> ᏑdataScalarPtr = new StandardBox<ScalarPtr>(new ScalarPtr(q: 1));
internal static ref ScalarPtr dataScalarPtr => ref ᏑdataScalarPtr.Value;
internal static ж<PtrScalar> ᏑdataPtrScalar = new StandardBox<PtrScalar>(new PtrScalar(w: 1));
internal static ref PtrScalar dataPtrScalar => ref ᏑdataPtrScalar.Value;
internal static ж<BigStruct> ᏑdataBigStruct = new StandardBox<BigStruct>(new BigStruct(w: 1));
internal static ref BigStruct dataBigStruct => ref ᏑdataBigStruct.Value;
internal static ж<@string> ᏑdataString = new StandardBox<@string>("foo"u8);
internal static ref @string dataString => ref ᏑdataString.Value;
internal static ж<slice<@string>> ᏑdataSlice = new StandardBox<slice<@string>>(new @string[]{"foo"u8}.slice());
internal static ref slice<@string> dataSlice => ref ᏑdataSlice.ValueSlot;
internal static ж<any> ᏑdataEface = new StandardBox<any>((nint)(42));
internal static ref any dataEface => ref ᏑdataEface.ValueSlot;
internal static ж<Iface> ᏑdataIface = new StandardBox<Iface>(((IfaceImpl)42));
internal static ref Iface dataIface => ref ᏑdataIface.ValueSlot;
internal static slice<byte> infoString = new byte[]{typePointer, typeScalar}.slice();
internal static slice<byte> infoSlice = new byte[]{typePointer, typeScalar, typeScalar}.slice();
internal static slice<byte> infoEface = new byte[]{typeScalar, typePointer}.slice();
internal static slice<byte> infoIface = new byte[]{typeScalar, typePointer}.slice();

} // end runtime_test_package
