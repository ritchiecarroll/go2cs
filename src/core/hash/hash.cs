// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package hash provides interfaces for hash functions.
namespace go;

using io = io_package;

partial class hash_package {

// Hash is the common interface implemented by all hash functions.
//
// Hash implementations in the standard library (e.g. [hash/crc32] and
// [crypto/sha256]) implement the [encoding.BinaryMarshaler], [encoding.BinaryAppender]
// and [encoding.BinaryUnmarshaler] interfaces. Marshaling a hash implementation
// allows its internal state to be saved and used for additional processing
// later, without having to re-write the data previously written to the hash.
// The hash state may contain portions of the input in its original form,
// which users are expected to handle for any possible security implications.
//
// Compatibility: Any future changes to hash or crypto packages will endeavor
// to maintain compatibility with state encoded using previous versions.
// That is, any released versions of the packages should be able to
// decode data written with any previously released version,
// subject to issues such as security fixes.
// See the Go compatibility document for background: https://golang.org/doc/go1compat
partial interface Hash :
    io.Writer
{
    // Sum appends the current hash to b and returns the resulting slice.
    // It does not change the underlying hash state.
    slice<byte> Sum(slice<byte> b);
    // Reset resets the Hash to its initial state.
    void Reset();
    // Size returns the number of bytes Sum will return.
    nint Size();
    // BlockSize returns the hash's underlying block size.
    // The Write method must be able to accept any amount
    // of data, but it may operate more efficiently if all writes
    // are a multiple of the block size.
    nint BlockSize();
}

// Hash32 is the common interface implemented by all 32-bit hash functions.
partial interface Hash32 :
    Hash
{
    uint32 Sum32();
}

// Hash64 is the common interface implemented by all 64-bit hash functions.
partial interface Hash64 :
    Hash
{
    uint64 Sum64();
}

} // end hash_package
