// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package randutil contains internal randomness utilities for various
// crypto packages.
namespace go.crypto.@internal;

using io = io_package;
using rand = math.rand.rand_package;
using math.rand;

partial class randutil_package {

// MaybeReadByte reads a single byte from r with 50% probability. This is used
// to ensure that callers do not depend on non-guaranteed behaviour, e.g.
// assuming that rsa.GenerateKey is deterministic w.r.t. a given random stream.
//
// This does not affect tests that pass a stream of fixed bytes as the random
// source (e.g. a zeroReader).
public static void MaybeReadByte(io.Reader r) {
    if ((rand.Uint64() & 1) == 1) {
        return;
    }
    array<byte> buf = new(1);
    r.Read(buf[..]);
}

} // end randutil_package
