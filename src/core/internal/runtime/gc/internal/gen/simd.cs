// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.runtime.gc.@internal;

partial class gen_package {

partial struct Uint8x64 {
    /*embed*/ internal valAny valAny;
}

internal static ж<Δkind> kindUint8x64 = Ꮡ(new Δkind(typ: "Uint8x64"u8, reg: regClassZ));

public static Uint8x64 /*y*/ ConstUint8x64(/*[64]*/ array<uint8> c, @string name) {
    Uint8x64 y = new(nil);
    c = c.Clone();

    y.valAny.initOp(Ꮡ(new op(Δop: "const"u8, kind: y.kind(), c: c.Clone(), name: name)));
    return y;
}

internal static ж<Δkind> kind(this Uint8x64 _) {
    return kindUint8x64;
}

internal static Uint8x64 wrap(this Uint8x64 _, ж<op> Ꮡx) {
    Uint8x64 y = new(nil);
    y.valAny.initOp(Ꮡx);
    return y;
}

public static Uint64x8 /*z*/ ToUint64x8(this Uint8x64 x) {
    Uint64x8 z = new(nil);

    z.op = x.op;
    return z;
}

public static Uint8x64 /*y*/ Shuffle(this Uint8x64 x, Uint8x64 shuf) {
    Uint8x64 y = new(nil);

    if ((~shuf.op).Δop == "const"u8) {
        // TODO: There are often patterns we can take advantage of here. Sometimes
        // we can do a broadcast. Sometimes we can at least do a quadword
        // permutation instead of a full byte permutation.
        // Range check the shuffle
        foreach (var (i, inp) in (~shuf.op).c._<array<uint8>>()) {
            // 0xff is a special "don't care" value
            if (!(inp == 0xff || inp < 64)) {
                fatalf("shuffle[%d] = %d out of range [0, %d) or 0xff"u8, i, inp, (nint)(64));
            }
        }
    }

    var args = new ж<op>[]{x.op, shuf.op}.slice();
    y.valAny.initOp(Ꮡ(new op(Δop: "VPERMB"u8, kind: y.kind(), args: args)));
    return y;
}

public static Uint8x64 /*y*/ ShuffleZeroed(this Uint8x64 x, Uint8x64 shuf, Mask64 mask) {
    Uint8x64 y = new(nil);

    var args = new ж<op>[]{x.op, shuf.op, mask.op}.slice();
    y.valAny.initOp(Ꮡ(new op(Δop: "VPERMB.Z"u8, kind: y.kind(), args: args)));
    return y;
}

public static Uint8x64 /*y*/ ShuffleMasked(this Uint8x64 x, Uint8x64 shuf, Mask64 mask) {
    Uint8x64 y = new(nil);

    var args = new ж<op>[]{x.op, shuf.op, mask.op}.slice();
    y.valAny.initOp(Ꮡ(new op(Δop: "VPERMB.mask"u8, kind: y.kind(), args: args)));
    return y;
}

// TODO: The two-argument shuffle is a little weird. You almost want the
// receiver to be the shuffle and the two arguments to be the two inputs, but
// that's almost certainly *not* what you want for the single input shuffle.
public static Uint8x64 /*z*/ Shuffle2(this Uint8x64 x, Uint8x64 y, Uint8x64 shuf) {
    Uint8x64 z = new(nil);

    // Confusingly, the inputs are in the opposite order from what you'd expect.
    var args = new ж<op>[]{y.op, x.op, shuf.op}.slice();
    z.valAny.initOp(Ꮡ(new op(Δop: "VPERMI2B"u8, kind: z.kind(), args: args)));
    return z;
}

public static Uint8x64 /*z*/ Shuffle2Zeroed(this Uint8x64 x, Uint8x64 y, Uint8x64 shuf, Mask64 mask) {
    Uint8x64 z = new(nil);

    // Confusingly, the inputs are in the opposite order from what you'd expect.
    var args = new ж<op>[]{y.op, x.op, mask.op, shuf.op}.slice();
    z.valAny.initOp(Ꮡ(new op(Δop: "VPERMI2B.Z"u8, kind: z.kind(), args: args)));
    return z;
}

public static Uint8x64 /*z*/ Shuffle2Masked(this Uint8x64 x, Uint8x64 y, Uint8x64 shuf, Mask64 mask) {
    Uint8x64 z = new(nil);

    // Confusingly, the inputs are in the opposite order from what you'd expect.
    var args = new ж<op>[]{y.op, x.op, mask.op, shuf.op}.slice();
    z.valAny.initOp(Ꮡ(new op(Δop: "VPERMI2B.mask"u8, kind: z.kind(), args: args)));
    return z;
}

partial struct Uint64x8 {
    /*embed*/ internal valAny valAny;
}

internal static ж<Δkind> kindUint64x8 = Ꮡ(new Δkind(typ: "Uint64x8"u8, reg: regClassZ));

public static Uint64x8 /*y*/ ConstUint64x8(/*[8]*/ array<uint64> c, @string name) {
    Uint64x8 y = new(nil);
    c = c.Clone();

    // TODO: Sometimes these can be optimized into broadcast loads.
    y.valAny.initOp(Ꮡ(new op(Δop: "const"u8, kind: y.kind(), c: c.Clone(), name: name)));
    return y;
}

public static Uint64x8 /*z*/ BroadcastUint64x8Zeroed(Uint64 src, Mask8 mask) {
    Uint64x8 z = new(nil);

    z.valAny.initOp(Ꮡ(new op(Δop: "VPBROADCASTQ.Z"u8, kind: z.kind(), args: new ж<op>[]{src.op, mask.op}.slice())));
    return z;
}

public static Uint64x8 /*z*/ BroadcastMasked(this Uint64x8 x, Uint64 src, Mask8 mask) {
    Uint64x8 z = new(nil);

    z.valAny.initOp(Ꮡ(new op(Δop: "VPBROADCASTQ.mask"u8, kind: z.kind(), args: new ж<op>[]{src.op, mask.op, x.op}.slice())));
    return z;
}

internal static ж<Δkind> kind(this Uint64x8 _) {
    return kindUint64x8;
}

internal static Uint64x8 wrap(this Uint64x8 _, ж<op> Ꮡx) {
    Uint64x8 y = new(nil);
    y.valAny.initOp(Ꮡx);
    return y;
}

public static Uint64x8 /*z*/ Or(this Uint64x8 x, Uint64x8 y) {
    Uint64x8 z = new(nil);

    z.valAny.initOp(Ꮡ(new op(Δop: "VPORQ"u8, kind: z.kind(), args: new ж<op>[]{y.op, x.op}.slice())));
    return z;
}

public static Uint64x8 /*z*/ Sub(this Uint64x8 x, Uint64x8 y) {
    Uint64x8 z = new(nil);

    // Arguments are backwards
    z.valAny.initOp(Ꮡ(new op(Δop: "VPSUBQ"u8, kind: z.kind(), args: new ж<op>[]{y.op, x.op}.slice())));
    return z;
}

public static Uint8x64 /*z*/ ToUint8x64(this Uint64x8 x) {
    Uint8x64 z = new(nil);

    z.op = x.op;
    return z;
}

public static Uint8x64 /*z*/ GF2P8Affine(this Uint64x8 x, Uint8x64 y) {
    Uint8x64 z = new(nil);

    // matrix, vector
    z.valAny.initOp(Ꮡ(new op(Δop: "VGF2P8AFFINEQB"u8, kind: z.kind(), args: new ж<op>[]{x.op, y.op}.slice())));
    return z;
}

public static Mask64 /*z*/ ShuffleBits(this Uint64x8 x, Uint8x64 y) {
    Mask64 z = new(nil);

    z.valAny.initOp(Ꮡ(new op(Δop: "VPSHUFBITQMB"u8, kind: z.kind(), args: new ж<op>[]{y.op, x.op}.slice())));
    return z;
}

public static Mask64 /*z*/ ShuffleBitsMasked(this Uint64x8 x, Uint8x64 y, Mask64 mask) {
    Mask64 z = new(nil);

    // This is always zeroing if the mask is provided.
    z.valAny.initOp(Ꮡ(new op(Δop: "VPSHUFBITQMB"u8, kind: z.kind(), args: new ж<op>[]{y.op, x.op, mask.op}.slice())));
    return z;
}

partial struct Mask8 {
    /*embed*/ internal valAny valAny;
}

internal static ж<Δkind> kindMask8 = Ꮡ(new Δkind(typ: "Mask8"u8, reg: regClassK));

public static Mask8 /*y*/ ConstMask8(uint8 c) {
    Mask8 y = new(nil);

    Uint64 tmp = new(nil);
    tmp.valGP.valAny.initOp(Ꮡ(new op(Δop: "MOVQ"u8, kind: tmp.kind(), args: new ж<op>[]{imm(c)}.slice())));
    y.valAny.initOp(Ꮡ(new op(Δop: "KMOVB"u8, kind: y.kind(), args: new ж<op>[]{tmp.op}.slice())));
    return y;
}

internal static ж<Δkind> kind(this Mask8 _) {
    return kindMask8;
}

internal static Mask8 wrap(this Mask8 _, ж<op> Ꮡx) {
    Mask8 y = new(nil);
    y.valAny.initOp(Ꮡx);
    return y;
}

public static Uint64 /*z*/ ToUint8(this Mask8 x) {
    Uint64 z = new(nil);

    z.valGP.valAny.initOp(Ꮡ(new op(Δop: "KMOVB"u8, kind: z.kind(), args: new ж<op>[]{x.op}.slice())));
    return z;
}

public static Mask8 /*z*/ Or(this Mask8 x, Mask8 y) {
    Mask8 z = new(nil);

    z.valAny.initOp(Ꮡ(new op(Δop: "KORQ"u8, kind: z.kind(), args: new ж<op>[]{y.op, x.op}.slice())));
    return z;
}

public static Mask8 /*z*/ ShiftLeft(this Mask8 x, uint8 c) {
    Mask8 z = new(nil);

    if (c == 0){
        z = x;
    } else {
        z.valAny.initOp(Ꮡ(new op(Δop: "KSHIFTLB"u8, kind: z.kind(), args: new ж<op>[]{imm(c), x.op}.slice())));
    }
    return z;
}

partial struct Mask64 {
    /*embed*/ internal valAny valAny;
}

internal static ж<Δkind> kindMask64 = Ꮡ(new Δkind(typ: "Mask64"u8, reg: regClassK));

public static Mask64 /*y*/ ConstMask64(uint64 c) {
    Mask64 y = new(nil);

    Uint64 tmp = new(nil);
    tmp.valGP.valAny.initOp(Ꮡ(new op(Δop: "MOVQ"u8, kind: tmp.kind(), args: new ж<op>[]{imm(c)}.slice())));
    y.valAny.initOp(Ꮡ(new op(Δop: "KMOVQ"u8, kind: y.kind(), args: new ж<op>[]{tmp.op}.slice())));
    return y;
}

internal static ж<Δkind> kind(this Mask64 _) {
    return kindMask64;
}

internal static Mask64 wrap(this Mask64 _, ж<op> Ꮡx) {
    Mask64 y = new(nil);
    y.valAny.initOp(Ꮡx);
    return y;
}

public static Uint64 /*z*/ ToUint64(this Mask64 x) {
    Uint64 z = new(nil);

    z.valGP.valAny.initOp(Ꮡ(new op(Δop: "KMOVQ"u8, kind: z.kind(), args: new ж<op>[]{x.op}.slice())));
    return z;
}

public static Mask64 /*z*/ Or(this Mask64 x, Mask64 y) {
    Mask64 z = new(nil);

    z.valAny.initOp(Ꮡ(new op(Δop: "KORQ"u8, kind: z.kind(), args: new ж<op>[]{y.op, x.op}.slice())));
    return z;
}

public static Mask64 /*z*/ ShiftLeft(this Mask64 x, uint8 c) {
    Mask64 z = new(nil);

    if (c == 0){
        z = x;
    } else {
        z.valAny.initOp(Ꮡ(new op(Δop: "KSHIFTLQ"u8, kind: z.kind(), args: new ж<op>[]{imm(c), x.op}.slice())));
    }
    return z;
}

public static Mask64 /*z*/ ShiftRight(this Mask64 x, uint8 c) {
    Mask64 z = new(nil);

    if (c == 0){
        z = x;
    } else {
        z.valAny.initOp(Ꮡ(new op(Δop: "KSHIFTRQ"u8, kind: z.kind(), args: new ж<op>[]{imm(c), x.op}.slice())));
    }
    return z;
}

} // end gen_package
