// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal;

using io = io_package;

partial class fips140_package {

// Hash is the common interface implemented by all hash functions. It is a copy
// of [hash.Hash] from the standard library, to avoid depending on security
// definitions from outside of the module.
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

} // end fips140_package
