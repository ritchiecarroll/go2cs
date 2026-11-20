// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.runtime.gc.@internal;

using static gen_package;

partial class gen_package {

partial struct Uint64 {
    /*embed*/ internal valGP valGP;
}

internal static ж<kind> kindUint64 = Ꮡ(new kind(typ: "Uint64", reg: regClassGP));

public static Uint64 /*y*/ ConstUint64(uint64 c, @string name) {
    Uint64 y = new(nil);

    y.valGP.valAny.initOp(Ꮡ(new op(Δop: "const", kind: y.kind(), c: c, name: name)));
    return y;
}

} partial class gen_package {

internal static ж<kind> kindᴍ(Uint64 _) {
    return kindUint64;
}

internal static Uint64 wrapᴍ(Uint64 _, ж<op> x) {
    Uint64 y = new(nil);
    y.valGP.valAny.initOp(x);
    return y;
}

}

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies above)
partial class genᴛUint64 {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<kind> kind(this Uint64 _) => gen_package.kindᴍ(_);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Uint64 wrap(this Uint64 _, ж<op> x) => gen_package.wrapᴍ(_, x);
}
// </GlyphForwarders>
