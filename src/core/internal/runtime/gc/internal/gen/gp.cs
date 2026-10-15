// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.runtime.gc.@internal;

partial class gen_package {

partial struct Uint64 {
    internal partial ref valGP valGP { get; }
}

internal static ж<Δkind> kindUint64 = Ꮡ(new Δkind(typ: "Uint64"u8, reg: regClassGP));

public static Uint64 /*y*/ ConstUint64(uint64 c, @string name) {
    Uint64 y = new(nil);

    y.valGP.valAny.initOp(Ꮡ(new op(Δop: "const"u8, kind: y.kind(), c: c, name: name)));
    return y;
}

internal static ж<Δkind> kind(this Uint64 _) {
    return kindUint64;
}

internal static Uint64 wrap(this Uint64 _, ж<op> Ꮡx) {
    Uint64 y = new(nil);
    y.valGP.valAny.initOp(Ꮡx);
    return y;
}

} // end gen_package
