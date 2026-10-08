// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package check implements the FIPS 140 load-time code+data verification.
// Every FIPS package providing cryptographic functionality except hmac and sha256
// must import crypto/internal/fips140/check, so that the verification happens
// before initialization of package global variables.
// The hmac and sha256 packages are used by this package, so they cannot import it.
// Instead, those packages must be careful not to change global variables during init.
// (If necessary, we could have check call a PostCheck function in those packages
// after the check has completed.)
namespace go.crypto.@internal.fips140;

using fips140 = go.crypto.@internal.fips140_package;
using hmac = go.crypto.@internal.fips140.hmac_package;
using sha256 = go.crypto.@internal.fips140.sha256_package;
using byteorder = go.crypto.@internal.fips140deps.byteorder_package;
using godebug = go.crypto.@internal.fips140deps.godebug_package;
using io = io_package;
using @unsafe = unsafe_package;
using go.crypto.@internal;
using go.crypto.@internal.fips140;
using go.crypto.@internal.fips140deps;

partial class check_package {

// Verified is set when verification succeeded. It can be expected to always be
// true when [fips140.Enabled] is true, or init would have panicked.
public static bool Verified;

// Linkinfo holds the go:fipsinfo symbol prepared by the linker.
// See cmd/link/internal/ld/fips.go for details.
//
//go:linkname Linkinfo go:fipsinfo

partial struct Linkinfoᴛ1_Sects /*dyn*/ {
    // Note: These must be unsafe.Pointer, not uintptr,
    // or else checkptr panics about turning uintptrs
    // into pointers into the data segment during
    // go test -race.
    public @unsafe.Pointer Start;
    public @unsafe.Pointer End;
}

partial struct Linkinfoᴛ1 /*dyn*/ {
    public array<byte> Magic = new(16);
    public array<byte> Sum = new(32);
    public uintptr Self;
    public array<Linkinfoᴛ1_Sects> Sects = new(4);
}
public static Linkinfoᴛ1 Linkinfo = new();

// "\xff"+fipsMagic is the expected linkinfo.Magic.
// We avoid writing that explicitly so that the string does not appear
// elsewhere in normal binaries, just as a precaution.
internal static readonly @string fipsMagic = ((@string)(new byte[]{0x20, 0x47, 0x6f, 0x20, 0x66, 0x69, 0x70, 0x73, 0x69, 0x6e, 0x66, 0x6f, 0x20, 0xff, 0x00}));

internal static array<byte> zeroSum = new(32);

// go2cs generated this placeholder — func init is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

} // end check_package
