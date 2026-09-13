// cputicks_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// runtime.cputicks -- Go's tick clock. It is assembly on every supported platform (RDTSC on x86,
// a cycle counter elsewhere), so the converter emits a bodyless partial and PartialStubGenerator
// fills it with a throw.
//
// That throw is not dormant. cputicks is read by sema.cs on every block-profiled semaphore
// acquisition, by tracetime.cs as the trace clock, and -- the reachable one today -- by
// ticksPerSecond, which runtime/pprof reaches through pprof_cyclesPerSecond for the
// `cycles/second=` header of every CPU, block and mutex profile.
//
// THE PART THAT HAS TO BE RIGHT IS THE EPOCH, NOT THE UNIT. Go does not declare the tick rate, it
// DERIVES it: ticksPerSecond computes (nowTicks - startTicks) * 1e9 / (nowTime - startTime) from a
// pair ticks.init writes down. ticks.init is called from schedinit -- read at runtime/<goos>/proc.cs,
// and schedinit is not reached in this corpus -- so startTicks and startTime are both zero and the
// expression collapses to cputicks() * 1e9 / nanotime(). nanotime1 is MonotonicClock.Nanoseconds(),
// so sourcing this from MonotonicClock.Ticks() makes that quotient exactly Stopwatch.Frequency: the
// clock's real rate, derived rather than asserted. A second, independently-originated tick source
// would satisfy every local property -- monotonic, advancing, well-scaled -- and still make the
// derived rate an arbitrary number, with every duration pprof converts through it wrong by the ratio
// of the two epochs. That is why this is one line to golib rather than a Stopwatch call here.
//
// It also holds if ticks.init ever becomes reachable: a real paired sample gives the same ratio,
// because the two readings advance together by construction. Guarded in GolibTests
// (MonotonicClockTests: the shared-epoch assertion IS the ticksPerSecond arithmetic).
//
// Declared FLAT in cputicks.cs -- one declaration for windows, linux and darwin alike -- so this
// body serves all three. Only the linux flavour has been run.
//
// Hand-owned (no cputicks_impl.go exists, so a reconvert never regenerates this file).
[module: go.GoManualConversion]

namespace go;

using go.golib;

partial class runtime_package {

internal static partial int64 cputicks() {
    return MonotonicClock.Ticks();
}

} // end runtime_package
