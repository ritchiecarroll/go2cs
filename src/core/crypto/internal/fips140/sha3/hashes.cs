// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140;

partial class sha3_package {

// New224 returns a new Digest computing the SHA3-224 hash.
public static ж<Digest> New224() {
    return Ꮡ(new Digest(rate: rateK448, outputLen: 28, dsbyte: dsbyteSHA3));
}

// New256 returns a new Digest computing the SHA3-256 hash.
public static ж<Digest> New256() {
    return Ꮡ(new Digest(rate: rateK512, outputLen: 32, dsbyte: dsbyteSHA3));
}

// New384 returns a new Digest computing the SHA3-384 hash.
public static ж<Digest> New384() {
    return Ꮡ(new Digest(rate: rateK768, outputLen: 48, dsbyte: dsbyteSHA3));
}

// New512 returns a new Digest computing the SHA3-512 hash.
public static ж<Digest> New512() {
    return Ꮡ(new Digest(rate: rateK1024, outputLen: 64, dsbyte: dsbyteSHA3));
}

internal static UntypedInt dsbyteSHA3 => 0b00000110;
internal static UntypedInt dsbyteKeccak => 0b00000001;
internal static UntypedInt dsbyteShake => 0b00011111;
internal static UntypedInt dsbyteCShake => 0b00000100;

// rateK[c] is the rate in bytes for Keccak[c] where c is the capacity in
// bits. Given the sponge size is 1600 bits, the rate is 1600 - c bits.
internal static UntypedInt rateK256 => /* (1600 - 256) / 8 */ 168;
internal static UntypedInt rateK448 => /* (1600 - 448) / 8 */ 144;
internal static UntypedInt rateK512 => /* (1600 - 512) / 8 */ 136;
internal static UntypedInt rateK768 => /* (1600 - 768) / 8 */ 104;
internal static UntypedInt rateK1024 => /* (1600 - 1024) / 8 */ 72;

// NewLegacyKeccak256 returns a new Digest computing the legacy, non-standard
// Keccak-256 hash.
public static ж<Digest> NewLegacyKeccak256() {
    return Ꮡ(new Digest(rate: rateK512, outputLen: 32, dsbyte: dsbyteKeccak));
}

// NewLegacyKeccak512 returns a new Digest computing the legacy, non-standard
// Keccak-512 hash.
public static ж<Digest> NewLegacyKeccak512() {
    return Ꮡ(new Digest(rate: rateK1024, outputLen: 64, dsbyte: dsbyteKeccak));
}

} // end sha3_package
