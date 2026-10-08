// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using byteorder = go.@internal.byteorder_package;
using sync = sync_package;
using go.@internal;

partial class des_package {

internal static void cryptBlock(slice<uint64> subkeys, slice<byte> dst, slice<byte> src, bool decrypt) {
    var b = byteorder.BEUint64(src);
    b = permuteInitialBlock(b);
    var (left, right) = ((uint32)((b >> (int)(32))), (uint32)b);
    left = (uint32)(((left << (int)(1))) | ((left >> (int)(31))));
    right = (uint32)(((right << (int)(1))) | ((right >> (int)(31))));
    if (decrypt){
        for (nint i = 0; i < 8; i++) {
            (left, right) = feistel(left, right, subkeys[15 - 2 * i], subkeys[15 - (2 * i + 1)]);
        }
    } else {
        for (nint i = 0; i < 8; i++) {
            (left, right) = feistel(left, right, subkeys[2 * i], subkeys[2 * i + 1]);
        }
    }
    left = (uint32)(((left << (int)(31))) | ((left >> (int)(1))));
    right = (uint32)(((right << (int)(31))) | ((right >> (int)(1))));
    // switch left & right and perform final permutation
    var preOutput = (uint64)((((uint64)right << (int)(32))) | (uint64)left);
    byteorder.BEPutUint64(dst, permuteFinalBlock(preOutput));
}

// DES Feistel function. feistelBox must be initialized via
// feistelBoxOnce.Do(initFeistelBox) first.
internal static (uint32 lout, uint32 rout) feistel(uint32 l, uint32 r, uint64 k0, uint64 k1) {
    uint32 t = default!;
    t = (uint32)(r ^ (uint32)((k0 >> (int)(32))));
    l ^= (uint32)((uint32)((uint32)((uint32)(feistelBox[7][(uint32)(t & 0x3f)] ^ feistelBox[5][(uint32)(((t >> (int)(8))) & 0x3f)]) ^ feistelBox[3][(uint32)(((t >> (int)(16))) & 0x3f)]) ^ feistelBox[1][(uint32)(((t >> (int)(24))) & 0x3f)]));
    t = (uint32)(((uint32)(((r << (int)(28))) | ((r >> (int)(4))))) ^ (uint32)k0);
    l ^= (uint32)((uint32)((uint32)((uint32)(feistelBox[6][(uint32)((t) & 0x3f)] ^ feistelBox[4][(uint32)(((t >> (int)(8))) & 0x3f)]) ^ feistelBox[2][(uint32)(((t >> (int)(16))) & 0x3f)]) ^ feistelBox[0][(uint32)(((t >> (int)(24))) & 0x3f)]));
    t = (uint32)(l ^ (uint32)((k1 >> (int)(32))));
    r ^= (uint32)((uint32)((uint32)((uint32)(feistelBox[7][(uint32)(t & 0x3f)] ^ feistelBox[5][(uint32)(((t >> (int)(8))) & 0x3f)]) ^ feistelBox[3][(uint32)(((t >> (int)(16))) & 0x3f)]) ^ feistelBox[1][(uint32)(((t >> (int)(24))) & 0x3f)]));
    t = (uint32)(((uint32)(((l << (int)(28))) | ((l >> (int)(4))))) ^ (uint32)k1);
    r ^= (uint32)((uint32)((uint32)((uint32)(feistelBox[6][(uint32)((t) & 0x3f)] ^ feistelBox[4][(uint32)(((t >> (int)(8))) & 0x3f)]) ^ feistelBox[2][(uint32)(((t >> (int)(16))) & 0x3f)]) ^ feistelBox[0][(uint32)(((t >> (int)(24))) & 0x3f)]));
    return (l, r);
}

// feistelBox[s][16*i+j] contains the output of permutationFunction
// for sBoxes[s][i][j] << 4*(7-s)
internal static array<array<uint32>> feistelBox = new(8, () => new(64));

internal static ж<sync.Once> ᏑfeistelBoxOnce = new StandardBox<sync.Once>(default(sync.Once));
internal static ref sync.Once feistelBoxOnce => ref ᏑfeistelBoxOnce.Value;

// general purpose function to perform DES block permutations.
internal static uint64 /*block*/ permuteBlock(uint64 src, slice<uint8> permutation) {
    uint64 block = default!;

    foreach (var (position, n) in permutation) {
        var bit = (uint64)((src.Rsh((uint64)(n))) & 1);
        block |= (uint64)(bit.Lsh((nuint)((len(permutation) - 1) - position)));
    }
    return block;
}

internal static void initFeistelBox() {
    foreach (var (s, _) in sBoxes) {
        for (nint i = 0; i < 4; i++) {
            for (nint j = 0; j < 16; j++) {
                var f = ((uint64)sBoxes[s][i][j]).Lsh((4 * (7 - (nuint)s)));
                f = permuteBlock(f, permutationFunction[..]);
                // Row is determined by the 1st and 6th bit.
                // Column is the middle four bits.
                var row = (uint8)((nint)(((((nint)(i & 2)) << (int)(4))) | (nint)(i & 1)));
                var col = (uint8)((j << (int)(1)));
                var t = (uint8)(row | col);
                // The rotation was performed in the feistel rounds, being factored out and now mixed into the feistelBox.
                f = (uint64)(((f << (int)(1))) | ((f >> (int)(31))));
                feistelBox[s][t] = (uint32)f;
            }
        }
    }
}

// permuteInitialBlock is equivalent to the permutation defined
// by initialPermutation.
internal static uint64 permuteInitialBlock(uint64 block) {
    // block = b7 b6 b5 b4 b3 b2 b1 b0 (8 bytes)
    var b1 = (block >> (int)(48));
    var b2 = (block << (int)(48));
    block ^= (uint64)((uint64)((uint64)((uint64)(b1 ^ b2) ^ (b1 << (int)(48))) ^ (b2 >> (int)(48))));
    // block = b1 b0 b5 b4 b3 b2 b7 b6
    b1 = (uint64)((block >> (int)(32)) & 0xff00ff);
    b2 = ((uint64)(block & 0xff00ff00U));
    block ^= (uint64)((uint64)((uint64)((uint64)((b1 << (int)(32)) ^ b2) ^ (b1 << (int)(8))) ^ (b2 << (int)(24)))); // exchange b0 b4 with b3 b7
    // block is now b1 b3 b5 b7 b0 b2 b4 b6, the permutation:
    //                  ...  8
    //                  ... 24
    //                  ... 40
    //                  ... 56
    //  7  6  5  4  3  2  1  0
    // 23 22 21 20 19 18 17 16
    //                  ... 32
    //                  ... 48
    // exchange 4,5,6,7 with 32,33,34,35 etc.
    b1 = (uint64)(block & 0x0f0f00000f0f0000UL);
    b2 = (uint64)(block & 0x0000f0f00000f0f0UL);
    block ^= (uint64)((uint64)((uint64)((uint64)(b1 ^ b2) ^ (b1 >> (int)(12))) ^ (b2 << (int)(12))));
    // block is the permutation:
    //
    //   [+8]         [+40]
    //
    //  7  6  5  4
    // 23 22 21 20
    //  3  2  1  0
    // 19 18 17 16    [+32]
    // exchange 0,1,4,5 with 18,19,22,23
    b1 = (uint64)(block & 0x3300330033003300UL);
    b2 = (uint64)(block & 0x00cc00cc00cc00ccUL);
    block ^= (uint64)((uint64)((uint64)((uint64)(b1 ^ b2) ^ (b1 >> (int)(6))) ^ (b2 << (int)(6))));
    // block is the permutation:
    // 15 14
    // 13 12
    // 11 10
    //  9  8
    //  7  6
    //  5  4
    //  3  2
    //  1  0 [+16] [+32] [+64]
    // exchange 0,2,4,6 with 9,11,13,15:
    b1 = (uint64)(block & 0xaaaaaaaa55555555UL);
    block ^= (uint64)((uint64)((uint64)(b1 ^ (b1 >> (int)(33))) ^ (b1 << (int)(33))));
    // block is the permutation:
    // 6 14 22 30 38 46 54 62
    // 4 12 20 28 36 44 52 60
    // 2 10 18 26 34 42 50 58
    // 0  8 16 24 32 40 48 56
    // 7 15 23 31 39 47 55 63
    // 5 13 21 29 37 45 53 61
    // 3 11 19 27 35 43 51 59
    // 1  9 17 25 33 41 49 57
    return block;
}

// permuteFinalBlock is equivalent to the permutation defined
// by finalPermutation.
internal static uint64 permuteFinalBlock(uint64 block) {
    // Perform the same bit exchanges as permuteInitialBlock
    // but in reverse order.
    var b1 = (uint64)(block & 0xaaaaaaaa55555555UL);
    block ^= (uint64)((uint64)((uint64)(b1 ^ (b1 >> (int)(33))) ^ (b1 << (int)(33))));
    b1 = (uint64)(block & 0x3300330033003300UL);
    var b2 = (uint64)(block & 0x00cc00cc00cc00ccUL);
    block ^= (uint64)((uint64)((uint64)((uint64)(b1 ^ b2) ^ (b1 >> (int)(6))) ^ (b2 << (int)(6))));
    b1 = (uint64)(block & 0x0f0f00000f0f0000UL);
    b2 = (uint64)(block & 0x0000f0f00000f0f0UL);
    block ^= (uint64)((uint64)((uint64)((uint64)(b1 ^ b2) ^ (b1 >> (int)(12))) ^ (b2 << (int)(12))));
    b1 = (uint64)((block >> (int)(32)) & 0xff00ff);
    b2 = ((uint64)(block & 0xff00ff00U));
    block ^= (uint64)((uint64)((uint64)((uint64)((b1 << (int)(32)) ^ b2) ^ (b1 << (int)(8))) ^ (b2 << (int)(24))));
    b1 = (block >> (int)(48));
    b2 = (block << (int)(48));
    block ^= (uint64)((uint64)((uint64)((uint64)(b1 ^ b2) ^ (b1 << (int)(48))) ^ (b2 >> (int)(48))));
    return block;
}

// creates 16 28-bit blocks rotated according
// to the rotation schedule.
internal static slice<uint32> /*out*/ ksRotate(uint32 @in) {
    slice<uint32> @out = default!;

    @out = new slice<uint32>(16);
    var last = @in;
    for (nint i = 0; i < 16; i++) {
        // 28-bit circular left shift
        var left = ((last.Lsh((uint64)((uint8)(4 + ksRotations[i])))) >> (int)(4));
        var right = ((last << (int)(4))).Rsh((uint64)((uint8)(32 - ksRotations[i])));
        @out[i] = (uint32)(left | right);
        last = @out[i];
    }
    return @out;
}

// creates 16 56-bit subkeys from the original key.
internal static void generateSubkeys(this ref desCipher c, slice<byte> keyBytes) {
    ᏑfeistelBoxOnce.Do(initFeistelBox);
    // apply PC1 permutation to key
    var key = byteorder.BEUint64(keyBytes);
    var permutedKey = permuteBlock(key, permutedChoice1[..]);
    // rotate halves of permuted key according to the rotation schedule
    var leftRotations = ksRotate((uint32)((permutedKey >> (int)(28))));
    var rightRotations = ksRotate(((uint32)((permutedKey << (int)(4))) >> (int)(4)));
    // generate subkeys
    for (nint i = 0; i < 16; i++) {
        // combine halves to form 56-bit input to PC2
        var pc2Input = (uint64)(((uint64)leftRotations[i] << (int)(28)) | (uint64)rightRotations[i]);
        // apply PC2 permutation to 7 byte input
        c.subkeys[i] = unpack(permuteBlock(pc2Input, permutedChoice2[..]));
    }
}

// Expand 48-bit input to 64-bit, with each 6-bit block padded by extra two bits at the top.
// By doing so, we can have the input blocks (four bits each), and the key blocks (six bits each) well-aligned without
// extra shifts/rotations for alignments.
internal static uint64 unpack(uint64 x) {
    return (uint64)((uint64)((uint64)((uint64)((uint64)((uint64)((uint64)((((uint64)(((x >> (int)((6 * 1)))) & 0xff)) << (int)((8 * 0))) | (((uint64)(((x >> (int)((6 * 3)))) & 0xff)) << (int)((8 * 1)))) | (((uint64)(((x >> (int)((6 * 5)))) & 0xff)) << (int)((8 * 2)))) | (((uint64)(((x >> (int)((6 * 7)))) & 0xff)) << (int)((8 * 3)))) | (((uint64)(((x >> (int)((6 * 0)))) & 0xff)) << (int)((8 * 4)))) | (((uint64)(((x >> (int)((6 * 2)))) & 0xff)) << (int)((8 * 5)))) | (((uint64)(((x >> (int)((6 * 4)))) & 0xff)) << (int)((8 * 6)))) | (((uint64)(((x >> (int)((6 * 6)))) & 0xff)) << (int)((8 * 7))));
}

} // end des_package
