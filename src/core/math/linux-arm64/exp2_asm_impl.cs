// exp2_asm_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// math.archExp2's body at linux/arm64, where Go writes it in assembly (exp2_asm.go, `//go:build arm64 || loong64`:
// haveArchExp2 is true, and archExp2 is the bodyless partial in exp2_asm.cs). It delegates to Go's own pure-Go
// exp2 (exp.go), as math_impl.cs's family does (that file's header has the reason).
//
// WHY IT WAS OWED (S3 site 9). Nothing bodied it. The flat math_impl.cs supplies only the members every build
// declares bodyless, and amd64 selects exp2_noasm.go (haveArchExp2 false), so no amd64 reading could see the gap.
// At linux/arm64 the PartialStubGenerator filled it with a throwing stub, and since Exp2 calls archExp2 whenever
// haveArchExp2 is true, every math.Exp2 call would have thrown there instead of computing. A compile could not
// show this: the throw is a body.
//
// This file's principal is exp2_asm.cs, which at this universe only linux/arm64 emits, so the hand-own router
// keeps it in linux-arm64/.
//
// Hand-owned (no exp2_asm_impl.go exists, so a reconvert never regenerates this file).
[module: go.GoManualConversion]

namespace go;

partial class math_package
{
    internal static partial float64 archExp2(float64 x) => exp2(x);
}
