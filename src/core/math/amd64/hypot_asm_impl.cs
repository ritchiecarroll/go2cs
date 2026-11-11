// hypot_asm_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// math.archHypot's body at amd64, where Go writes it in assembly (hypot_asm.go, `//go:build 386 || amd64`:
// haveArchHypot is true, and archHypot is the bodyless partial in hypot_asm.cs). It delegates to Go's own pure-Go
// hypot, as math_impl.cs's family does (that file's header has the reason).
//
// WHY HERE AND NOT FLAT (S3 site 9, M2's wall). The body sat in the flat math_impl.cs, which every build
// compiles. At linux/arm64 Go selects hypot_noasm.go instead (haveArchHypot false, and an archHypot that panics,
// never called), which the converter emits as linux-arm64/hypot_noasm.cs. So the flat body was a second
// definition: CS0111 archHypot, which stopped math, and with it every project above it, at that flavour. This
// file's principal is hypot_asm.cs, which only the amd64 targets emit, so the hand-own router keeps it in amd64/.
//
// Hand-owned (no hypot_asm_impl.go exists, so a reconvert never regenerates this file).
[module: go.GoManualConversion]

namespace go;

partial class math_package
{
    internal static partial float64 archHypot(float64 p, float64 q) => hypot(p, q);
}
