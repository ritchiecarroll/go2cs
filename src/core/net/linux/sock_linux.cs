// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using unix = @internal.syscall.unix_package;
using syscall = syscall_package;
using @internal.syscall;

partial class net_package {

// Linux stores the backlog as:
//
//   - uint16 in kernel version < 4.1,
//   - uint32 in kernel version >= 4.1
//
// Truncate number to avoid wrapping.
//
// See issue 5030 and 41470.
internal static nint maxAckBacklog(nint n) {
    var (major, minor) = unix.KernelVersion();
    nint size = 16;
    if (major > 4 || (major == 4 && minor >= 1)) {
        size = 32;
    }
    nuint max = ((nuint)1).Lsh((int64)(size)) - 1;
    if ((nuint)n > max) {
        n = (nint)max;
    }
    return n;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string procSysNetCoreSomaxconnˢ = "/proc/sys/net/core/somaxconn"u8;

internal static nint maxListenerBacklog() {
    GoFrame ᒐ = default;
    try {
        var (fd, err) = open(procSysNetCoreSomaxconnˢ);
        if (err != default!) {
            return syscall.SOMAXCONN;
        }
        var fdʗ1 = fd;
        defer(fdʗ1.close, ref ᒐ);
        var (l, ok) = fd.readLine();
        if (!ok) {
            return syscall.SOMAXCONN;
        }
        var f = getFields(l);
        (var n, _, ok) = dtoi(f[0]);
        if (n == 0 || !ok) {
            return syscall.SOMAXCONN;
        }
        if (n > (1 << (int)(16)) - 1) {
            return maxAckBacklog(n);
        }
        return n;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

} // end net_package
