// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package sha512 implements the SHA-384, SHA-512, SHA-512/224, and SHA-512/256
// hash algorithms as defined in FIPS 180-4.
//
// All the hash.Hash implementations returned by this package also
// implement encoding.BinaryMarshaler and encoding.BinaryUnmarshaler to
// marshal and unmarshal the internal state of the hash.
namespace go.crypto;

using crypto = crypto_package;
using boring = go.crypto.@internal.boring_package;
using sha512 = go.crypto.@internal.fips140.sha512_package;
using hash = hash_package;
using go.crypto.@internal;
using go.crypto.@internal.fips140;

partial class sha512_package {

[GoInit] internal static void init() {
    crypto.RegisterHash(crypto.SHA384, New384);
    crypto.RegisterHash(crypto.SHA512, New);
    crypto.RegisterHash(crypto.SHA512_224, New512_224);
    crypto.RegisterHash(crypto.SHA512_256, New512_256);
}

// Size is the size, in bytes, of a SHA-512 checksum.
public static UntypedInt Size => 64;

// Size224 is the size, in bytes, of a SHA-512/224 checksum.
public static UntypedInt Size224 => 28;

// Size256 is the size, in bytes, of a SHA-512/256 checksum.
public static UntypedInt Size256 => 32;

// Size384 is the size, in bytes, of a SHA-384 checksum.
public static UntypedInt Size384 => 48;

// BlockSize is the block size, in bytes, of the SHA-512/224,
// SHA-512/256, SHA-384 and SHA-512 hash functions.
public static UntypedInt BlockSize => 128;

// New returns a new [hash.Hash] computing the SHA-512 checksum. The Hash
// also implements [encoding.BinaryMarshaler], [encoding.BinaryAppender] and
// [encoding.BinaryUnmarshaler] to marshal and unmarshal the internal
// state of the hash.
public static hash.Hash New() {
    if (boring.Enabled) {
        return boring.NewSHA512();
    }
    return new sha512_DigestжHash(sha512.New());
}

// New512_224 returns a new [hash.Hash] computing the SHA-512/224 checksum. The Hash
// also implements [encoding.BinaryMarshaler], [encoding.BinaryAppender] and
// [encoding.BinaryUnmarshaler] to marshal and unmarshal the internal
// state of the hash.
public static hash.Hash New512_224() {
    return new sha512_DigestжHash(sha512.New512_224());
}

// New512_256 returns a new [hash.Hash] computing the SHA-512/256 checksum. The Hash
// also implements [encoding.BinaryMarshaler], [encoding.BinaryAppender] and
// [encoding.BinaryUnmarshaler] to marshal and unmarshal the internal
// state of the hash.
public static hash.Hash New512_256() {
    return new sha512_DigestжHash(sha512.New512_256());
}

// New384 returns a new [hash.Hash] computing the SHA-384 checksum. The Hash
// also implements [encoding.BinaryMarshaler], [encoding.BinaryAppender] and
// [encoding.BinaryUnmarshaler] to marshal and unmarshal the internal
// state of the hash.
public static hash.Hash New384() {
    if (boring.Enabled) {
        return boring.NewSHA384();
    }
    return new sha512_DigestжHash(sha512.New384());
}

// Sum512 returns the SHA512 checksum of the data.
public static array<byte> Sum512(slice<byte> data) {
    if (boring.Enabled) {
        return boring.SHA512(data);
    }
    var h = New();
    h.Write(data);
    array<byte> sum = new(64); /* Size */
    h.Sum(sum[..0]);
    return sum.Clone();
}

// Sum384 returns the SHA384 checksum of the data.
public static array<byte> Sum384(slice<byte> data) {
    if (boring.Enabled) {
        return boring.SHA384(data);
    }
    var h = New384();
    h.Write(data);
    array<byte> sum = new(48); /* Size384 */
    h.Sum(sum[..0]);
    return sum.Clone();
}

// Sum512_224 returns the Sum512/224 checksum of the data.
public static array<byte> Sum512_224(slice<byte> data) {
    var h = New512_224();
    h.Write(data);
    array<byte> sum = new(28); /* Size224 */
    h.Sum(sum[..0]);
    return sum.Clone();
}

// Sum512_256 returns the Sum512/256 checksum of the data.
public static array<byte> Sum512_256(slice<byte> data) {
    var h = New512_256();
    h.Write(data);
    array<byte> sum = new(32); /* Size256 */
    h.Sum(sum[..0]);
    return sum.Clone();
}

} // end sha512_package
