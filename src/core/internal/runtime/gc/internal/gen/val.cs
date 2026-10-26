// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.runtime.gc.@internal;

using sync = go.sync_package;
using go;

partial class gen_package {

partial interface Value {
    ж<Δkind> kind();
    ж<op> getOp();
}

partial interface Word :
    Value
{
    void isWord();
}

// wrap is an unfortunate necessity so that we can pass Value types around as
// values (not pointers), but still have generic functions that can construct a
// new Value. Ideally we would just have a method on Value to initialize its op,
// but that needs to have a non-pointer receiver to satisfy the interface and
// then it can't mutate the Value.
public partial interface Δwrap<T> :
    Value

    where T : Value{
    T wrap(ж<op> x);
}

public partial struct Δkind {
    internal @string typ;
    internal regClass reg;
}

partial struct @void {
    /*embed*/ internal valAny valAny;
}

internal static ж<Δkind> voidKind = Ꮡ(new Δkind(typ: "void"u8, reg: regClassNone));

internal static ж<Δkind> kind(this @void _) {
    return voidKind;
}

partial struct Ptr<T>
    where T : Value
{
    /*embed*/ internal valGP valGP;
}

// Ptr is a Word
internal static Word _ᴛ1ʗ = new Ptr<@void>(nil);

internal static ж<sync.Map> ᏑptrKinds = new StandardBox<sync.Map>(new sync.Map(nil));
internal static ref sync.Map ptrKinds => ref ᏑptrKinds.Value; // *kind -> *kind

internal static ж<Δkind> kind<T>(this Ptr<T> _Δp0)
    where T : Value
{
    T x = GoZero<T>();
    var xk = x.kind();
    var (pk, ok) = ᏑptrKinds.Load(xk.OrTypedNil());
    if (!ok) {
        var k = Ꮡ(new Δkind(typ: "Ptr["u8 + (~x.kind()).typ + "]"u8, reg: regClassGP));
        (pk, _) = ᏑptrKinds.LoadOrStore(xk.OrTypedNil(), k.OrTypedNil());
    }
    return pk._<ж<Δkind>>();
}

internal static Ptr<T> wrap<T>(this Ptr<T> _, ж<op> Ꮡx)
    where T : Value
{
    Ptr<T> y = new(nil);
    y.valGP.valAny.initOp(Ꮡx);
    return y;
}

public static Ptr<T> /*y*/ AddConst<T>(this Ptr<T> x, nint off)
    where T : Value
{
    Ptr<T> y = new(nil);

    var @base = x.op;
    while ((~@base).Δop == "addConst"u8) {
        off += (~(~@base).args[1]).c._<nint>();
        @base = (~@base).args[0];
    }
    y.valGP.valAny.initOp(Ꮡ(new op(Δop: "addConst"u8, kind: y.kind(), args: new ж<op>[]{@base, imm(off)}.slice())));
    return y;
}

public static T Deref<W, T>(Ptr<W> ptr)
    where W : Δwrap<T>
    where T : Value
{
    ref var off = ref heap(new nint(), out var Ꮡoff);
    var @base = ptr.op;
    while ((~@base).Δop == "addConst"u8) {
        off += (~(~@base).args[1]).c._<nint>();
        @base = (~@base).args[0];
    }

    W y = GoZero<W>();
    return y.wrap(Ꮡ(new op(Δop: "deref"u8, kind: y.kind(), args: new ж<op>[]{@base}.slice(), c: off)));
}

partial struct Array<T>
    where T : Value
{
    /*embed*/ internal valAny valAny;
}

public static Array<T> /*y*/ ConstArray<T>(slice<T> vals, @string name)
    where T : Value
{
    Array<T> y = new(nil);

    // TODO: This probably doesn't actually work because emitConst won't
    // understand vals.
    y.valAny.initOp(Ꮡ(new op(Δop: "const"u8, kind: y.kind(), c: vals, name: name)));
    return y;
}

internal static ж<Δkind> kind<T>(this Array<T> _)
    where T : Value
{
    // TODO: Cache this like Ptr.kind.
    T x = GoZero<T>();
    return Ꮡ(new Δkind(typ: "Array["u8 + (~x.kind()).typ + "]"u8, reg: regClassNone));
}

partial struct valGP {
    /*embed*/ internal valAny valAny;
}

internal static void isWord(this valGP _) {
}

partial struct valAny {
    /*embed*/ internal ж<op> op;
}

internal static void initOp(this ref valAny v, ж<op> Ꮡx) {
    ref var x = ref Ꮡx.DerefOrNull();

    if (v.op != nil) {
        throw panic("double init of val");
    }
    if (x.kind == nil) {
        throw panic("val missing kind");
    }
    v.op = Ꮡx;

    // Figure out this value's function.
    foreach (var (_, arg) in x.args) {
        {
            var fn = arg.Value.fn; if (fn != nil) {
                fn.attach(Ꮡx);
                break;
            }
        }
    }
}

internal static ж<op> getOp(this valAny v) {
    return v.op;
}

} // end gen_package
