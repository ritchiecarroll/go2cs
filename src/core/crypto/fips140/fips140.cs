// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package fips140 provides information about the FIPS 140-3 Go Cryptographic
// Module and FIPS 140-3 mode.
//
// For more details, see the [FIPS 140-3 documentation].
//
// [FIPS 140-3 documentation]: https://go.dev/doc/security/fips140
namespace go.crypto;

using fips140 = go.crypto.@internal.fips140_package;
using check = go.crypto.@internal.fips140.check_package;
using go.crypto.@internal;
using go.crypto.@internal.fips140;

partial class fips140_package {

// Enabled reports whether the cryptography libraries are operating in FIPS
// 140-3 mode.
//
// It can be controlled at runtime using the GODEBUG setting "fips140". If set
// to "on", FIPS 140-3 mode is enabled. If set to "only", non-approved
// cryptography functions will additionally return errors or panic.
//
// This can't be changed after the program has started.
public static bool Enabled() {
    if (fips140.Enabled && !check.Verified) {
        throw panic("crypto/fips140: FIPS 140-3 mode enabled, but integrity check didn't pass");
    }
    return fips140.Enabled;
}

// Version returns the FIPS 140-3 Go Cryptographic Module version (such as
// "v1.0.0"), as referenced in the Security Policy for the module, if building
// against a frozen module with GOFIPS140. Otherwise, it returns "latest". If an
// alias is in use (such as "inprogress") the actual resolved version is
// returned.
//
// The returned version may not uniquely identify the frozen module which was
// used to build the program, if there are multiple copies of the frozen module
// at the same version. The uniquely identifying version suffix can be found by
// checking the value of the GOFIPS140 setting in
// runtime/debug.BuildInfo.Settings.
public static @string Version() {
    return fips140.Version();
}

} // end fips140_package
