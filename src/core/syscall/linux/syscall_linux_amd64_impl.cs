// syscall_linux_amd64_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// Hand-written implementation of the ONE generated linux/amd64 declaration that carries no body and
// no assembly to borrow: gettimeofday.
//
// Go declares it as a //go:noescape assembly entry point (syscall_linux_amd64.go):
//
//     //go:noescape
//     func gettimeofday(tv *Timeval) (err Errno)
//
// backed by sys_linux_amd64.s, which reads the vDSO's clock_gettime(CLOCK_REALTIME) and converts
// the timespec to a timeval. There is no Go body to convert, so the converted declaration stays a
// bodyless partial and the PartialStubGenerator supplies a throwing one. Measured on the Linux
// roster: syscall's own TestGettimeofday reports `infrastructure-error` rather than a divergence —
// the host throws NotImplementedException out of the generated stub instead of answering — and two
// of syscall's rows ride on it (Gettimeofday and Time are its only callers).
//
// This is NOT the struct-passing class. Timeval on linux/amd64 is
//
//     [GoType] partial struct Timeval { public int64 Sec; public int64 Usec; }
//
// two inline int64s with no managed reference anywhere, so the blittable-mirror remedy that Fstat,
// fstatat and Uname needed does not apply: nothing crosses a native boundary here at all. The
// implementation only has to produce the same NUMBERS Go's assembly produces.
//
// It can, exactly: on Linux the CLR's own wall clock IS clock_gettime(CLOCK_REALTIME) — the same
// source Go's vDSO path reads — so DateTime.UtcNow and Go's gettimeofday observe one clock, and the
// only work is the epoch shift and the 100ns-to-microsecond division Go's assembly also performs.
// The alternative (a P/Invoke to libc gettimeofday) would add a native dependency to read the clock
// the runtime has already read, for no fidelity the numbers do not already have.
//
// The nil case answers EFAULT rather than faulting, which is what the kernel does with a bad tv
// pointer and what Go's callers are written against.

using System;

[module: go.GoManualConversion]

namespace go;

partial class syscall_package
{
    // The declaration this supplies a body for lives in syscall_linux_amd64.cs:
    //     internal static partial Errno /*err*/ gettimeofday(ж<Timeval> tv);
    internal static partial Errno /*err*/ gettimeofday(ж<Timeval> tv)
    {
        if (tv == nil)
        {
            // The kernel's answer to a bad address; Gettimeofday and Time both test errno != 0.
            return EFAULT;
        }

        // CLOCK_REALTIME, in the CLR's 100-nanosecond ticks, shifted to the Unix epoch.
        long ticks = DateTime.UtcNow.Ticks - DateTime.UnixEpoch.Ticks;

        ref var timeval = ref tv.DerefOrNull();
        timeval.Sec = (int64)(ticks / TimeSpan.TicksPerSecond);
        // 100ns units -> microseconds, the same truncation Go's timespec-to-timeval conversion does.
        timeval.Usec = (int64)(ticks % TimeSpan.TicksPerSecond / 10);

        return 0;
    }
}
