// testenv_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.
//
// This file is inspired by Go, whose notice it carries (license of record: LICENSES/BSD-3-Clause.txt):
//
// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//
// PROVENANCE:
// - CPUProfilingBroken: INSPIRED BY src/internal/testenv/testenv.go, Go 1.26.11, func CPUProfilingBroken. The answer
//   Go gives on a port with no CPU sampler (its plan9 arm).

// internal/testenv.CPUProfilingBroken, hand-owned by manualConversionFuncs["internal/testenv"] under
// COORD's ruling of 2026-09-22.
//
// Go answers true where CPU profiling has known issues, and its first arm is plan9 ("Profiling
// unimplemented"): a port with no CPU sampler. The managed runtime has none either. Its Windows setters
// take plan9's shape (runtime/windows/cpuprof_windows_impl.cs), so a CPU profile starts, stops and is
// valid with ZERO samples, and the answer is true on every target.
//
// What the converted false cost: runtime/pprof's testCPUProfile doubles its duration (5 s, 10, 20, ...)
// until time.Until(deadline) is shorter, so with zero samples each CPU test ran the whole series against
// the package deadline (315 / 155 / 35 s measured) and the row timed out. With true, Go gives the test a
// 10-second deadline and ends it in its own Skipf naming golang.org/issue/13841. That turns each such row
// pass -> skip, the structural "no CPU sampler in the managed runtime" disclosure candidate, rather than a
// failure or a starved deadline.
//
// Hand-owned (no testenv_impl.go exists, so a reconvert never regenerates this file).
[module: go.GoManualConversion]

namespace go.@internal;

partial class testenv_package {

// CPUProfilingBroken returns true if CPU profiling has known issues on this
// platform.
public static bool CPUProfilingBroken() => true;

} // end testenv_package
