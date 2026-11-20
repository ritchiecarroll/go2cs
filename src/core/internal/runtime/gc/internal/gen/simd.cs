// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.runtime.gc.@internal;

using static gen_package;

partial class gen_package {

partial struct Uint8x64 {
    /*embed*/ internal valAny valAny;
}

internal static ж<kind> kindUint8x64 = Ꮡ(new kind(typ: "Uint8x64", reg: regClassZ));

public static Uint8x64 /*y*/ ConstUint8x64(/*[64]*/ array<uint8> c, @string name) {
    Uint8x64 y = new(nil);
    c = c.Clone();

    y.valAny.initOp(Ꮡ(new op(Δop: "const", kind: y.kind(), c: c.Clone(), name: name)));
    return y;
}

} partial class gen_package {

internal static ж<kind> kindᴍ(Uint8x64 _) {
    return kindUint8x64;
}

internal static Uint8x64 wrapᴍ(Uint8x64 _, ж<op> x) {
    Uint8x64 y = new(nil);
    y.valAny.initOp(x);
    return y;
}

internal static Uint64x8 /*z*/ ToUint64x8ᴍ(Uint8x64 x) {
    Uint64x8 z = new(nil);

    z.op = x.op;
    return z;
}

internal static Uint8x64 /*y*/ Shuffleᴍ(Uint8x64 x, Uint8x64 shuf) {
    Uint8x64 y = new(nil);

    if (shuf.op.Δop == "const"u8) {
        // TODO: There are often patterns we can take advantage of here. Sometimes
        // we can do a broadcast. Sometimes we can at least do a quadword
        // permutation instead of a full byte permutation.

        // Range check the shuffle
        foreach (var (i, inp) in range(shuf.op.c._<array<uint8>>())) {
            // 0xff is a special "don't care" value
            if (!(inp == 0xff || inp < 64)) {
                fatalf("shuffle[%d] = %d out of range [0, %d) or 0xff", i, inp, (nint)64);
            }
        }
    }

    var args = slice<ж<op>>([x.op, shuf.op]);
    y.valAny.initOp(Ꮡ(new op(Δop: "VPERMB", kind: y.kind(), args: args)));
    return y;
}

internal static Uint8x64 /*y*/ ShuffleZeroedᴍ(Uint8x64 x, Uint8x64 shuf, Mask64 mask) {
    Uint8x64 y = new(nil);

    var args = slice<ж<op>>([x.op, shuf.op, mask.op]);
    y.valAny.initOp(Ꮡ(new op(Δop: "VPERMB.Z", kind: y.kind(), args: args)));
    return y;
}

internal static Uint8x64 /*y*/ ShuffleMaskedᴍ(Uint8x64 x, Uint8x64 shuf, Mask64 mask) {
    Uint8x64 y = new(nil);

    var args = slice<ж<op>>([x.op, shuf.op, mask.op]);
    y.valAny.initOp(Ꮡ(new op(Δop: "VPERMB.mask", kind: y.kind(), args: args)));
    return y;
}

// TODO: The two-argument shuffle is a little weird. You almost want the
// receiver to be the shuffle and the two arguments to be the two inputs, but
// that's almost certainly *not* what you want for the single input shuffle.
internal static Uint8x64 /*z*/ Shuffle2ᴍ(Uint8x64 x, Uint8x64 y, Uint8x64 shuf) {
    Uint8x64 z = new(nil);

    // Confusingly, the inputs are in the opposite order from what you'd expect.
    var args = slice<ж<op>>([y.op, x.op, shuf.op]);
    z.valAny.initOp(Ꮡ(new op(Δop: "VPERMI2B", kind: z.kind(), args: args)));
    return z;
}

internal static Uint8x64 /*z*/ Shuffle2Zeroedᴍ(Uint8x64 x, Uint8x64 y, Uint8x64 shuf, Mask64 mask) {
    Uint8x64 z = new(nil);

    // Confusingly, the inputs are in the opposite order from what you'd expect.
    var args = slice<ж<op>>([y.op, x.op, mask.op, shuf.op]);
    z.valAny.initOp(Ꮡ(new op(Δop: "VPERMI2B.Z", kind: z.kind(), args: args)));
    return z;
}

internal static Uint8x64 /*z*/ Shuffle2Maskedᴍ(Uint8x64 x, Uint8x64 y, Uint8x64 shuf, Mask64 mask) {
    Uint8x64 z = new(nil);

    // Confusingly, the inputs are in the opposite order from what you'd expect.
    var args = slice<ж<op>>([y.op, x.op, mask.op, shuf.op]);
    z.valAny.initOp(Ꮡ(new op(Δop: "VPERMI2B.mask", kind: z.kind(), args: args)));
    return z;
}

} partial class gen_package {

partial struct Uint64x8 {
    /*embed*/ internal valAny valAny;
}

internal static ж<kind> kindUint64x8 = Ꮡ(new kind(typ: "Uint64x8", reg: regClassZ));

public static Uint64x8 /*y*/ ConstUint64x8(/*[8]*/ array<uint64> c, @string name) {
    Uint64x8 y = new(nil);
    c = c.Clone();

    // TODO: Sometimes these can be optimized into broadcast loads.
    y.valAny.initOp(Ꮡ(new op(Δop: "const", kind: y.kind(), c: c.Clone(), name: name)));
    return y;
}

public static Uint64x8 /*z*/ BroadcastUint64x8Zeroed(Uint64 src, Mask8 mask) {
    Uint64x8 z = new(nil);

    z.valAny.initOp(Ꮡ(new op(Δop: "VPBROADCASTQ.Z", kind: z.kind(), args: [src.op, mask.op])));
    return z;
}

} partial class gen_package {

internal static Uint64x8 /*z*/ BroadcastMaskedᴍ(Uint64x8 x, Uint64 src, Mask8 mask) {
    Uint64x8 z = new(nil);

    z.valAny.initOp(Ꮡ(new op(Δop: "VPBROADCASTQ.mask", kind: z.kind(), args: [src.op, mask.op, x.op])));
    return z;
}

internal static ж<kind> kindᴍ(Uint64x8 _) {
    return kindUint64x8;
}

internal static Uint64x8 wrapᴍ(Uint64x8 _, ж<op> x) {
    Uint64x8 y = new(nil);
    y.valAny.initOp(x);
    return y;
}

internal static Uint64x8 /*z*/ Orᴍ(Uint64x8 x, Uint64x8 y) {
    Uint64x8 z = new(nil);

    z.valAny.initOp(Ꮡ(new op(Δop: "VPORQ", kind: z.kind(), args: [y.op, x.op])));
    return z;
}

internal static Uint64x8 /*z*/ Subᴍ(Uint64x8 x, Uint64x8 y) {
    Uint64x8 z = new(nil);

    // Arguments are backwards
    z.valAny.initOp(Ꮡ(new op(Δop: "VPSUBQ", kind: z.kind(), args: [y.op, x.op])));
    return z;
}

internal static Uint8x64 /*z*/ ToUint8x64ᴍ(Uint64x8 x) {
    Uint8x64 z = new(nil);

    z.op = x.op;
    return z;
}

internal static Uint8x64 /*z*/ GF2P8Affineᴍ(Uint64x8 x, Uint8x64 y) {
    Uint8x64 z = new(nil);

    // matrix, vector
    z.valAny.initOp(Ꮡ(new op(Δop: "VGF2P8AFFINEQB", kind: z.kind(), args: [x.op, y.op])));
    return z;
}

internal static Mask64 /*z*/ ShuffleBitsᴍ(Uint64x8 x, Uint8x64 y) {
    Mask64 z = new(nil);

    z.valAny.initOp(Ꮡ(new op(Δop: "VPSHUFBITQMB", kind: z.kind(), args: [y.op, x.op])));
    return z;
}

internal static Mask64 /*z*/ ShuffleBitsMaskedᴍ(Uint64x8 x, Uint8x64 y, Mask64 mask) {
    Mask64 z = new(nil);

    // This is always zeroing if the mask is provided.
    z.valAny.initOp(Ꮡ(new op(Δop: "VPSHUFBITQMB", kind: z.kind(), args: [y.op, x.op, mask.op])));
    return z;
}

} partial class gen_package {

partial struct Mask8 {
    /*embed*/ internal valAny valAny;
}

internal static ж<kind> kindMask8 = Ꮡ(new kind(typ: "Mask8", reg: regClassK));

public static Mask8 /*y*/ ConstMask8(uint8 c) {
    Mask8 y = new(nil);

    Uint64 tmp = new(nil);
    tmp.valGP.valAny.initOp(Ꮡ(new op(Δop: "MOVQ", kind: tmp.kind(), args: slice<ж<op>>([imm(c)]))));
    y.valAny.initOp(Ꮡ(new op(Δop: "KMOVB", kind: y.kind(), args: [tmp.op])));
    return y;
}

} partial class gen_package {

internal static ж<kind> kindᴍ(Mask8 _) {
    return kindMask8;
}

internal static Mask8 wrapᴍ(Mask8 _, ж<op> x) {
    Mask8 y = new(nil);
    y.valAny.initOp(x);
    return y;
}

internal static Uint64 /*z*/ ToUint8ᴍ(Mask8 x) {
    Uint64 z = new(nil);

    z.valGP.valAny.initOp(Ꮡ(new op(Δop: "KMOVB", kind: z.kind(), args: [x.op])));
    return z;
}

internal static Mask8 /*z*/ Orᴍ(Mask8 x, Mask8 y) {
    Mask8 z = new(nil);

    z.valAny.initOp(Ꮡ(new op(Δop: "KORQ", kind: z.kind(), args: [y.op, x.op])));
    return z;
}

internal static Mask8 /*z*/ ShiftLeftᴍ(Mask8 x, uint8 c) {
    Mask8 z = new(nil);

    if (c == 0) {
        z = x;
    } else {
        z.valAny.initOp(Ꮡ(new op(Δop: "KSHIFTLB", kind: z.kind(), args: slice<ж<op>>([imm(c), x.op]))));
    }
    return z;
}

} partial class gen_package {

partial struct Mask64 {
    /*embed*/ internal valAny valAny;
}

internal static ж<kind> kindMask64 = Ꮡ(new kind(typ: "Mask64", reg: regClassK));

public static Mask64 /*y*/ ConstMask64(uint64 c) {
    Mask64 y = new(nil);

    Uint64 tmp = new(nil);
    tmp.valGP.valAny.initOp(Ꮡ(new op(Δop: "MOVQ", kind: tmp.kind(), args: slice<ж<op>>([imm(c)]))));
    y.valAny.initOp(Ꮡ(new op(Δop: "KMOVQ", kind: y.kind(), args: [tmp.op])));
    return y;
}

} partial class gen_package {

internal static ж<kind> kindᴍ(Mask64 _) {
    return kindMask64;
}

internal static Mask64 wrapᴍ(Mask64 _, ж<op> x) {
    Mask64 y = new(nil);
    y.valAny.initOp(x);
    return y;
}

internal static Uint64 /*z*/ ToUint64ᴍ(Mask64 x) {
    Uint64 z = new(nil);

    z.valGP.valAny.initOp(Ꮡ(new op(Δop: "KMOVQ", kind: z.kind(), args: [x.op])));
    return z;
}

internal static Mask64 /*z*/ Orᴍ(Mask64 x, Mask64 y) {
    Mask64 z = new(nil);

    z.valAny.initOp(Ꮡ(new op(Δop: "KORQ", kind: z.kind(), args: [y.op, x.op])));
    return z;
}

internal static Mask64 /*z*/ ShiftLeftᴍ(Mask64 x, uint8 c) {
    Mask64 z = new(nil);

    if (c == 0) {
        z = x;
    } else {
        z.valAny.initOp(Ꮡ(new op(Δop: "KSHIFTLQ", kind: z.kind(), args: slice<ж<op>>([imm(c), x.op]))));
    }
    return z;
}

internal static Mask64 /*z*/ ShiftRightᴍ(Mask64 x, uint8 c) {
    Mask64 z = new(nil);

    if (c == 0) {
        z = x;
    } else {
        z.valAny.initOp(Ꮡ(new op(Δop: "KSHIFTRQ", kind: z.kind(), args: slice<ж<op>>([imm(c), x.op]))));
    }
    return z;
}

}

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies above)
partial class genᴛUint8x64 {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<kind> kind(this Uint8x64 _) => gen_package.kindᴍ(_);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Uint8x64 wrap(this Uint8x64 _, ж<op> x) => gen_package.wrapᴍ(_, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Uint64x8 /*z*/ ToUint64x8(this Uint8x64 x) => gen_package.ToUint64x8ᴍ(x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Uint8x64 /*y*/ Shuffle(this Uint8x64 x, Uint8x64 shuf) => gen_package.Shuffleᴍ(x, shuf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Uint8x64 /*y*/ ShuffleZeroed(this Uint8x64 x, Uint8x64 shuf, Mask64 mask) => gen_package.ShuffleZeroedᴍ(x, shuf, mask);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Uint8x64 /*y*/ ShuffleMasked(this Uint8x64 x, Uint8x64 shuf, Mask64 mask) => gen_package.ShuffleMaskedᴍ(x, shuf, mask);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Uint8x64 /*z*/ Shuffle2(this Uint8x64 x, Uint8x64 y, Uint8x64 shuf) => gen_package.Shuffle2ᴍ(x, y, shuf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Uint8x64 /*z*/ Shuffle2Zeroed(this Uint8x64 x, Uint8x64 y, Uint8x64 shuf, Mask64 mask) => gen_package.Shuffle2Zeroedᴍ(x, y, shuf, mask);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Uint8x64 /*z*/ Shuffle2Masked(this Uint8x64 x, Uint8x64 y, Uint8x64 shuf, Mask64 mask) => gen_package.Shuffle2Maskedᴍ(x, y, shuf, mask);
}
partial class genᴛUint64x8 {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Uint64x8 /*z*/ BroadcastMasked(this Uint64x8 x, Uint64 src, Mask8 mask) => gen_package.BroadcastMaskedᴍ(x, src, mask);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<kind> kind(this Uint64x8 _) => gen_package.kindᴍ(_);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Uint64x8 wrap(this Uint64x8 _, ж<op> x) => gen_package.wrapᴍ(_, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Uint64x8 /*z*/ Or(this Uint64x8 x, Uint64x8 y) => gen_package.Orᴍ(x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Uint64x8 /*z*/ Sub(this Uint64x8 x, Uint64x8 y) => gen_package.Subᴍ(x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Uint8x64 /*z*/ ToUint8x64(this Uint64x8 x) => gen_package.ToUint8x64ᴍ(x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Uint8x64 /*z*/ GF2P8Affine(this Uint64x8 x, Uint8x64 y) => gen_package.GF2P8Affineᴍ(x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Mask64 /*z*/ ShuffleBits(this Uint64x8 x, Uint8x64 y) => gen_package.ShuffleBitsᴍ(x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Mask64 /*z*/ ShuffleBitsMasked(this Uint64x8 x, Uint8x64 y, Mask64 mask) => gen_package.ShuffleBitsMaskedᴍ(x, y, mask);
}
partial class genᴛMask8 {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<kind> kind(this Mask8 _) => gen_package.kindᴍ(_);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Mask8 wrap(this Mask8 _, ж<op> x) => gen_package.wrapᴍ(_, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Uint64 /*z*/ ToUint8(this Mask8 x) => gen_package.ToUint8ᴍ(x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Mask8 /*z*/ Or(this Mask8 x, Mask8 y) => gen_package.Orᴍ(x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Mask8 /*z*/ ShiftLeft(this Mask8 x, uint8 c) => gen_package.ShiftLeftᴍ(x, c);
}
partial class genᴛMask64 {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<kind> kind(this Mask64 _) => gen_package.kindᴍ(_);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Mask64 wrap(this Mask64 _, ж<op> x) => gen_package.wrapᴍ(_, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Uint64 /*z*/ ToUint64(this Mask64 x) => gen_package.ToUint64ᴍ(x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Mask64 /*z*/ Or(this Mask64 x, Mask64 y) => gen_package.Orᴍ(x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Mask64 /*z*/ ShiftLeft(this Mask64 x, uint8 c) => gen_package.ShiftLeftᴍ(x, c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Mask64 /*z*/ ShiftRight(this Mask64 x, uint8 c) => gen_package.ShiftRightᴍ(x, c);
}
// </GlyphForwarders>
