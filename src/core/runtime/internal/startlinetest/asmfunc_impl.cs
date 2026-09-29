// asmfunc_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// AsmFunc REFUSES BY NAME (coordinator ruling 2026-09-28 22:01, the P1 disclosure seat). Go defines
// it in func_amd64.s, a TEXT block whose whole point is its own start line: TestStartLineAsm asks
// runtime.Caller-based start-line reporting for that assembly line. No assembly converts, so the
// generator stubbed it with a NotImplementedException, which the test host classifies as an
// infrastructure error -- a verdict no disclosure can absorb. A Go panic naming the missing body is
// the honest report, and the runtime row discloses it as runtime-capability.
//
// Hand-owned (no func_amd64_impl.go exists, so a reconvert never regenerates this file).

namespace go.runtime.@internal;

partial class startlinetest_package
{
    public static partial nint AsmFunc() =>
        throw panic("runtime/internal/startlinetest: AsmFunc: an assembly function (func_amd64.s) has no managed body");
}
