// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140;

using byteorder = go.crypto.@internal.fips140deps.byteorder_package;
using cpu = go.crypto.@internal.fips140deps.cpu_package;
using bits = math.bits_package;
using @unsafe = unsafe_package;
using go.crypto.@internal.fips140deps;
using math;

partial class sha3_package {

// rc stores the round constants for use in the ι step.
[GoLazy] partial class lazyᴛrc { internal static array<uint64> rc = new uint64[]{
    0x0000000000000001,
    0x0000000000008082,
    0x800000000000808AUL,
    0x8000000080008000UL,
    0x000000000000808B,
    0x0000000080000001U,
    0x8000000080008081UL,
    0x8000000000008009UL,
    0x000000000000008A,
    0x0000000000000088,
    0x0000000080008009U,
    0x000000008000000AU,
    0x000000008000808BU,
    0x800000000000008BUL,
    0x8000000000008089UL,
    0x8000000000008003UL,
    0x8000000000008002UL,
    0x8000000000000080UL,
    0x000000000000800A,
    0x800000008000000AUL,
    0x8000000080008081UL,
    0x8000000000008080UL,
    0x0000000080000001U,
    0x8000000080008008UL
}.array(); }

// go2cs generated this placeholder — func keccakF1600Generic is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

} // end sha3_package
