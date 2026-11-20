// Copyright 2024 The Libc Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

using unix = golang.org.x.sys.unix_package;
using runtime = runtime_package;
using golang.org.x.sys;
using syscall = syscall_package;

partial class libc_package {

internal static @long ___syscall_cp(ж<TLS> tls, @long n, @long a, @long b, @long c, @long d, @long e, @long f) {
    switch (n) {
    case var ᴛ when ᴛ == SYS_nanosleep: {
        return sleepSyscall(tls, n, new @long[]{a, b, c, d, e, f}.array(), 0, 1, 0);
    }
    case var ᴛ when ᴛ == SYS_clock_nanosleep || ᴛ == sysClockNanosleepTime64: {
        return sleepSyscall(tls, n, new @long[]{a, b, c, d, e, f}.array(), 2, 3, b);
    }}

    var (r1, _, err) = (unix.Syscall6((uintptr)n, (uintptr)a, (uintptr)b, (uintptr)c, (uintptr)d, (uintptr)e, (uintptr)f));
    if (err != 0) {
        return (@long)(((syscall.Errno)0 - err));
    }

    return (@long)r1;
}

internal static UntypedInt sleepScratchSize => 16;

internal static @long sleepSyscall(ж<TLS> tls, @long n, /*[6]*/ array<@long> args, nint iReq, nint iRem, @long flags) {
    GoFrame ᒐ = default;
    try {
        args = args.Clone();

        if (args[iRem] == 0 && (@long)(flags & (int64)TIMER_ABSTIME) == 0 && tls != nil) {
            var p = tls.Alloc(sleepScratchSize);
            defer(tls.Free, (nint)(sleepScratchSize), ref ᒐ);

            args[iRem] = (@long)p;
        }
        for (;;) {
            var (r1, _, err) = unix.Syscall6((uintptr)n, (uintptr)args[0], (uintptr)args[1], (uintptr)args[2], (uintptr)args[3], (uintptr)args[4], (uintptr)args[5]);
            switch (ᐧ) {
            case {} when err == 0: {
                return (@long)r1;
            }
            case {} when err != unix.EINTR: {
                return (@long)(((syscall.Errno)0 - err));
            }
            case {} when (@long)(flags & (int64)TIMER_ABSTIME) != 0: {
                continue;
                break;
            }
            case {} when args[iRem] is 0: {
                return (@long)(((syscall.Errno)0 - err));
            }}

            args[iReq] = args[iRem];
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

public static @long X__syscall0(ж<TLS> tls, @long n) {
    switch (n) {
    case var ᴛ when ᴛ == __NR_sched_yield: {
        runtime.Gosched();
        return 0;
    }
    default: {
        var (r1, _, err) = unix.Syscall((uintptr)n, 0, 0, 0);
        if (err != 0) {
            return (@long)(((syscall.Errno)0 - err));
        }

        return (@long)r1;
    }}

}

public static @long X__syscall1(ж<TLS> tls, @long n, @long a1) {
    var (r1, _, err) = unix.Syscall((uintptr)n, (uintptr)a1, 0, 0);
    if (err != 0) {
        return (@long)(((syscall.Errno)0 - err));
    }

    return (@long)r1;
}

public static @long X__syscall2(ж<TLS> Ꮡtls, @long n, @long a1, @long a2) {
    ref var tls = ref Ꮡtls.DerefOrNull();

    switch (n) {
    case var ᴛ when ᴛ == SYS_tkill: {
        if ((int32)a1 == tls.tid()) {
            return Ꮡtls.raiseSignal((int32)a2);
        }

        var t = tlsByTid((int32)a1); if (t != nil) {
            return t.killSignal((int32)a2);
        }
        break;
    }}

    var (r1, _, err) = unix.Syscall((uintptr)n, (uintptr)a1, (uintptr)a2, 0);
    if (err != 0) {
        return (@long)(((syscall.Errno)0 - err));
    }

    return (@long)r1;
}

public static @long X__syscall3(ж<TLS> Ꮡtls, @long n, @long a1, @long a2, @long a3) {
    ref var tls = ref Ꮡtls.DerefOrNull();

    switch (n) {
    case var ᴛ when ᴛ == SYS_tgkill: {
        if ((int32)a2 == tls.tid()) {
            return Ꮡtls.raiseSignal((int32)a3);
        }

        var t = tlsByTid((int32)a2); if (t != nil) {
            return t.killSignal((int32)a3);
        }
        break;
    }}

    var (r1, _, err) = unix.Syscall((uintptr)n, (uintptr)a1, (uintptr)a2, (uintptr)a3);
    if (err != 0) {
        return (@long)(((syscall.Errno)0 - err));
    }

    return (@long)r1;
}

public static @long X__syscall4(ж<TLS> tls, @long n, @long a1, @long a2, @long a3, @long a4) {
    var (r1, _, err) = unix.Syscall6((uintptr)n, (uintptr)a1, (uintptr)a2, (uintptr)a3, (uintptr)a4, 0, 0);
    if (err != 0) {
        return (@long)(((syscall.Errno)0 - err));
    }

    return (@long)r1;
}

public static @long X__syscall5(ж<TLS> tls, @long n, @long a1, @long a2, @long a3, @long a4, @long a5) {
    var (r1, _, err) = unix.Syscall6((uintptr)n, (uintptr)a1, (uintptr)a2, (uintptr)a3, (uintptr)a4, (uintptr)a5, 0);
    if (err != 0) {
        return (@long)(((syscall.Errno)0 - err));
    }

    return (@long)r1;
}

public static @long X__syscall6(ж<TLS> tls, @long n, @long a1, @long a2, @long a3, @long a4, @long a5, @long a6) {
    var (r1, _, err) = unix.Syscall6((uintptr)n, (uintptr)a1, (uintptr)a2, (uintptr)a3, (uintptr)a4, (uintptr)a5, (uintptr)a6);
    if (err != 0) {
        return (@long)(((syscall.Errno)0 - err));
    }

    return (@long)r1;
}

} // end libc_package
