// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package boring provides access to BoringCrypto implementation functions.
// Check the constant Enabled to find out whether BoringCrypto is available.
// If BoringCrypto is not available, the functions in this package all panic.
namespace go.crypto.@internal;

partial class boring_package {

// Enabled reports whether BoringCrypto is available.
// When enabled is false, all functions in this package panic.
//
// BoringCrypto is only available on linux/amd64 and linux/arm64 systems.
public const bool Enabled = /* available */ false;

partial struct BigInt /*[]nuint*/;

} // end boring_package
