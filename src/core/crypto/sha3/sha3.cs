// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package sha3 implements the SHA-3 hash algorithms and the SHAKE extendable
// output functions defined in FIPS 202.
namespace go.crypto;

using crypto = crypto_package;
using sha3 = go.crypto.@internal.fips140.sha3_package;
using hash = hash_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
using go.crypto.@internal.fips140;

partial class sha3_package {

[GoInit] internal static void init() {
    crypto.RegisterHash(crypto.SHA3_224, () => new SHA3жHash(New224()));
    crypto.RegisterHash(crypto.SHA3_256, () => new SHA3жHash(New256()));
    crypto.RegisterHash(crypto.SHA3_384, () => new SHA3жHash(New384()));
    crypto.RegisterHash(crypto.SHA3_512, () => new SHA3жHash(New512()));
}

// Sum224 returns the SHA3-224 hash of data.
public static array<byte> Sum224(slice<byte> data) {
    array<byte> @out = new(28);
    var h = sha3.New224();
    h.Write(data);
    h.Sum(@out[..0]);
    return @out.Clone();
}

// Sum256 returns the SHA3-256 hash of data.
public static array<byte> Sum256(slice<byte> data) {
    array<byte> @out = new(32);
    var h = sha3.New256();
    h.Write(data);
    h.Sum(@out[..0]);
    return @out.Clone();
}

// Sum384 returns the SHA3-384 hash of data.
public static array<byte> Sum384(slice<byte> data) {
    array<byte> @out = new(48);
    var h = sha3.New384();
    h.Write(data);
    h.Sum(@out[..0]);
    return @out.Clone();
}

// Sum512 returns the SHA3-512 hash of data.
public static array<byte> Sum512(slice<byte> data) {
    array<byte> @out = new(64);
    var h = sha3.New512();
    h.Write(data);
    h.Sum(@out[..0]);
    return @out.Clone();
}

// SumSHAKE128 applies the SHAKE128 extendable output function to data and
// returns an output of the given length in bytes.
public static slice<byte> SumSHAKE128(slice<byte> data, nint length) {
    // Outline the allocation for up to 256 bits of output to the caller's stack.
    var @out = new slice<byte>(32);
    return sumSHAKE128(@out, data, length);
}

internal static slice<byte> sumSHAKE128(slice<byte> @out, slice<byte> data, nint length) {
    if (len(@out) < length){
        @out = new slice<byte>(length);
    } else {
        @out = @out.slice(0, length);
    }
    var h = sha3.NewShake128();
    h.Write(data);
    h.Read(@out);
    return @out;
}

// SumSHAKE256 applies the SHAKE256 extendable output function to data and
// returns an output of the given length in bytes.
public static slice<byte> SumSHAKE256(slice<byte> data, nint length) {
    // Outline the allocation for up to 512 bits of output to the caller's stack.
    var @out = new slice<byte>(64);
    return sumSHAKE256(@out, data, length);
}

internal static slice<byte> sumSHAKE256(slice<byte> @out, slice<byte> data, nint length) {
    if (len(@out) < length){
        @out = new slice<byte>(length);
    } else {
        @out = @out.slice(0, length);
    }
    var h = sha3.NewShake256();
    h.Write(data);
    h.Read(@out);
    return @out;
}

// SHA3 is an instance of a SHA-3 hash. It implements [hash.Hash].
[GoType] partial struct SHA3 {
    internal sha3.Digest s;
}

//go:linkname fips140hash_sha3Unwrap crypto/internal/fips140hash.sha3Unwrap
public static ж<sha3.Digest> fips140hash_sha3Unwrap(ж<SHA3> Ꮡsha3) {
    return Ꮡsha3.of(SHA3.Ꮡs);
}

// New224 creates a new SHA3-224 hash.
public static ж<SHA3> New224() {
    return Ꮡ(new SHA3(sha3.New224().Value.ΔClone()));
}

// New256 creates a new SHA3-256 hash.
public static ж<SHA3> New256() {
    return Ꮡ(new SHA3(sha3.New256().Value.ΔClone()));
}

// New384 creates a new SHA3-384 hash.
public static ж<SHA3> New384() {
    return Ꮡ(new SHA3(sha3.New384().Value.ΔClone()));
}

// New512 creates a new SHA3-512 hash.
public static ж<SHA3> New512() {
    return Ꮡ(new SHA3(sha3.New512().Value.ΔClone()));
}

// Write absorbs more data into the hash's state.
public static (nint n, error err) Write(this ж<SHA3> Ꮡs, slice<byte> p) {
    return Ꮡs.of(SHA3.Ꮡs).Write(p);
}

// Sum appends the current hash to b and returns the resulting slice.
[GoRecv] public static slice<byte> Sum(this ref SHA3 s, slice<byte> b) {
    return s.s.Sum(b);
}

// Reset resets the hash to its initial state.
[GoRecv] public static void Reset(this ref SHA3 s) {
    s.s.Reset();
}

// Size returns the number of bytes Sum will produce.
[GoRecv] public static nint Size(this ref SHA3 s) {
    return s.s.Size();
}

// BlockSize returns the hash's rate.
[GoRecv] public static nint BlockSize(this ref SHA3 s) {
    return s.s.BlockSize();
}

// MarshalBinary implements [encoding.BinaryMarshaler].
[GoRecv] public static (slice<byte>, error) MarshalBinary(this ref SHA3 s) {
    return s.s.MarshalBinary();
}

// AppendBinary implements [encoding.BinaryAppender].
[GoRecv] public static (slice<byte>, error) AppendBinary(this ref SHA3 s, slice<byte> p) {
    return s.s.AppendBinary(p);
}

// UnmarshalBinary implements [encoding.BinaryUnmarshaler].
[GoRecv] public static error UnmarshalBinary(this ref SHA3 s, slice<byte> data) {
    return s.s.UnmarshalBinary(data);
}

// SHAKE is an instance of a SHAKE extendable output function.
[GoType] partial struct SHAKE {
    internal sha3.SHAKE s;
}

// NewSHAKE128 creates a new SHAKE128 XOF.
public static ж<SHAKE> NewSHAKE128() {
    return Ꮡ(new SHAKE(sha3.NewShake128().Value.ΔClone()));
}

// NewSHAKE256 creates a new SHAKE256 XOF.
public static ж<SHAKE> NewSHAKE256() {
    return Ꮡ(new SHAKE(sha3.NewShake256().Value.ΔClone()));
}

// NewCSHAKE128 creates a new cSHAKE128 XOF.
//
// N is used to define functions based on cSHAKE, it can be empty when plain
// cSHAKE is desired. S is a customization byte string used for domain
// separation. When N and S are both empty, this is equivalent to NewSHAKE128.
public static ж<SHAKE> NewCSHAKE128(slice<byte> N, slice<byte> S) {
    return Ꮡ(new SHAKE(sha3.NewCShake128(N, S).Value.ΔClone()));
}

// NewCSHAKE256 creates a new cSHAKE256 XOF.
//
// N is used to define functions based on cSHAKE, it can be empty when plain
// cSHAKE is desired. S is a customization byte string used for domain
// separation. When N and S are both empty, this is equivalent to NewSHAKE256.
public static ж<SHAKE> NewCSHAKE256(slice<byte> N, slice<byte> S) {
    return Ꮡ(new SHAKE(sha3.NewCShake256(N, S).Value.ΔClone()));
}

// Write absorbs more data into the XOF's state.
//
// It panics if any output has already been read.
public static (nint n, error err) Write(this ж<SHAKE> Ꮡs, slice<byte> p) {
    return Ꮡs.of(SHAKE.Ꮡs).Write(p);
}

// Read squeezes more output from the XOF.
//
// Any call to Write after a call to Read will panic.
public static (nint n, error err) Read(this ж<SHAKE> Ꮡs, slice<byte> p) {
    return Ꮡs.of(SHAKE.Ꮡs).Read(p);
}

// Reset resets the XOF to its initial state.
public static void Reset(this ж<SHAKE> Ꮡs) {
    Ꮡs.of(SHAKE.Ꮡs).Reset();
}

// BlockSize returns the rate of the XOF.
[GoRecv] public static nint BlockSize(this ref SHAKE s) {
    return s.s.BlockSize();
}

// MarshalBinary implements [encoding.BinaryMarshaler].
[GoRecv] public static (slice<byte>, error) MarshalBinary(this ref SHAKE s) {
    return s.s.MarshalBinary();
}

// AppendBinary implements [encoding.BinaryAppender].
[GoRecv] public static (slice<byte>, error) AppendBinary(this ref SHAKE s, slice<byte> p) {
    return s.s.AppendBinary(p);
}

// UnmarshalBinary implements [encoding.BinaryUnmarshaler].
[GoRecv] public static error UnmarshalBinary(this ref SHAKE s, slice<byte> data) {
    return s.s.UnmarshalBinary(data);
}

} // end sha3_package
