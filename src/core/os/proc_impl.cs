// proc_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// Hand-written implementation of os's //go:linkname-into-runtime hook (proc.go's
// `runtime_beforeExit`, provided by runtime.os_beforeExit). go2cs emits it as a bodyless `partial`
// method, and without a body here the PartialStubGenerator fills it with a throwing stub — so EVERY
// os.Exit call died with "runtime_beforeExit: external (assembly or cgo) function is not
// implemented" instead of exiting. That made any converted program that calls os.Exit unusable, and
// in particular made a spawned CHILD process report a panic and exit 2 rather than its own status
// (surfaced by the os/exec child-process work: the child's output carried the stub's message and
// its exit code never reached the parent).
//
// Go's runtime.os_beforeExit does exactly two things, neither of which applies here:
//
//   1. runExitHooks(code) — runs the registered exit hooks, whose only stdlib producer is coverage
//      instrumentation writing out a coverage data file. go2cs does not implement Go's coverage
//      instrumentation, so no hook is ever registered and the call has nothing to run. (os cannot
//      forward to the real runtime.os_beforeExit in any case: os.csproj does not reference the
//      runtime package, and the implementation there is `internal` to that assembly.)
//   2. racefini() when exiting 0 under -race — go2cs has no race detector.
//
// So a no-op is the faithful behavior, not a shortcut: os.Exit proceeds directly to syscall.Exit,
// which is what actually terminates the process. If go2cs ever gains exit hooks or coverage output,
// this is the single place that has to grow a body.

namespace go;

partial class os_package
{
    internal static partial void runtime_beforeExit(nint exitCode)
    {
    }
}
