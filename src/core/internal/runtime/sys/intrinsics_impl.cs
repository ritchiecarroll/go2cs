// intrinsics_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// GetCallerPC and GetCallerSP REFUSE BY NAME (coordinator ruling 2026-09-29: refuse at the representational
// point; re-confirmed for the go1.26 hop, claude/g-hop-lost-refusals). Both are compiler intrinsics with no Go
// body: the compiler rewrites each call into instructions that read the caller's return PC and stack pointer.
// go2cs emits no Go machine code, so there is no Go PC or SP to report. Bodyless, the generator stubbed them
// with a NotImplementedException, which the test host classifies as an infrastructure error -- a verdict no
// disclosure can absorb. A Go panic naming the intrinsic is the honest report: it replaces the exception with
// Go's panic shape at the same call sites, so nothing that works today changes.
//
// Go 1.26's runtime-seh_windows_test.go builds its machine context with sys.GetCallerPC/GetCallerSP directly
// (newCtx), where Go 1.24's went through runtime.NewContextStub and its refusal; the five TestSeh* rows are
// disclosed runtime-capability against these texts. A GetCallerPC answered from the Callers token machinery
// would be honest alone, but its callers pair it with GetCallerSP, which has none (that body is Q53's).
//
// Hand-owned (no intrinsics_impl.go exists, so a reconvert never regenerates this file).

namespace go.@internal.runtime;

partial class sys_package
{
    public static partial uintptr GetCallerPC() =>
        throw panic("internal/runtime/sys: GetCallerPC: a compiler intrinsic reading the caller's machine PC; go2cs emits no Go machine code");

    public static partial uintptr GetCallerSP() =>
        throw panic("internal/runtime/sys: GetCallerSP: a compiler intrinsic reading the caller's machine SP; go2cs emits no Go machine code");
}
