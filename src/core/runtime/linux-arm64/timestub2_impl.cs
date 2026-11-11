// timestub2_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// runtime.walltime at linux/arm64: the wall clock as (Unix seconds, nanoseconds), which Go reads from the vDSO's
// clock_gettime(CLOCK_REALTIME) in assembly (sys_linux_arm64.s). timestub2.go declares it bodyless at every
// target where time_now is Go code (timestub.go), which in this universe is linux/arm64 alone. linux/amd64,
// windows and darwin take time_now from assembly or libc instead.
//
// WHY IT WAS OWED (S3 site 9, the arm64-only stubbed census). It was bodyless, so PartialStubGenerator threw, and
// timestub.go's time_now calls it first. time.Now does NOT come here: the time package's own hand-own
// (time/time_impl.cs, now and runtimeNow) reads the clock itself. runtime's own time_now callers would have thrown,
// though. That is the same reach as linux/amd64's bodyless time_now (linux-amd64/timeasm.cs), and this file does
// not change that one.
//
// The clock is DateTime.UtcNow, read as time/time_impl.cs's now() reads it, so runtime and time agree on the wall
// clock. Its granularity is 100 ns, the unit DateTime keeps; nsec is the sub-second remainder in nanoseconds.
//
// This file's principal is timestub2.cs, which only linux/arm64 emits in this universe, so the hand-own router keeps
// it in linux-arm64/.
//
// Hand-owned (no timestub2_impl.go exists, so a reconvert never regenerates this file).
[module: go.GoManualConversion]

namespace go;

partial class runtime_package
{
    internal static partial (int64 sec, int32 nsec) walltime()
    {
        int64 unixTicks = System.DateTime.UtcNow.Ticks - System.DateTime.UnixEpoch.Ticks;
        int64 sec = unixTicks / System.TimeSpan.TicksPerSecond;
        int32 nsec = ((int32)(unixTicks % System.TimeSpan.TicksPerSecond)) * 100;
        return (sec, nsec);
    }
}
