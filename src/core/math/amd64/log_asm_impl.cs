// log_asm_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// math.archLog's body at amd64, where Go writes it in assembly (log_asm.go, `//go:build amd64 || s390x`: haveArchLog
// is true, and archLog is the bodyless partial in log_asm.cs). It delegates to Go's own pure-Go log, as
// math_impl.cs's family does (that file's header has the reason).
//
// WHY HERE AND NOT FLAT (S3 site 9, M2's wall). The body sat in the flat math_impl.cs, which every build
// compiles. At linux/arm64 Go selects log_stub.go instead (haveArchLog false, and an archLog that panics, never
// called), which the converter emits as linux-arm64/log_stub.cs. So the flat body was a second definition:
// CS0111 archLog, which stopped math, and with it every project above it, at that flavour. This file's principal
// is log_asm.cs, which only the amd64 targets emit, so the hand-own router keeps it in amd64/.
//
// Hand-owned (no log_asm_impl.go exists, so a reconvert never regenerates this file).
[module: go.GoManualConversion]

namespace go;

partial class math_package
{
    internal static partial float64 archLog(float64 x) => log(x);
}
