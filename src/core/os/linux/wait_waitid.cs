// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// We used to use this code for Darwin, but according to issue #19314
// waitid returns if the process is stopped, even when using WEXITED.
//go:build linux

// go2cs NATIVE BUFFER (hand-owned; replaces the converted wait_waitid.go output). The converted
// body allocated the 128-byte siginfo as a MANAGED `array<uint64>(16)` heap box and handed the
// kernel `(uintptr)psig` across SYS_WAITID — a call that BLOCKS (WNOWAIT: until the child becomes
// waitable, i.e. for the child's lifetime). A golib box address is transient: GC compaction
// relocates the array during the wait, and when the child exits the kernel writes 128 bytes of
// siginfo_t at the STALE address — into whatever object lives there now. Heap corruption that
// surfaces as a SIGSEGV at an unrelated later point: measured 4-for-4 on os/exec's suite the day
// the exec wall opened (the death point moved between runs — this mechanism's signature), rooted
// from a crash dump whose wait threads sat parked in exactly this call. The wait4 sibling took the
// same fix the same day (zsyscall_linux_amd64_impl.cs).
//
// The remedy is the exec hand-own's own soundness rule: the buffer handed to the native call lives
// in UNMANAGED memory for the duration of the call. Go's comment stands — the values are never
// read — so there is no copy-out, and the buffer is freed in a finally. Everything else in this
// file is the converted output verbatim.

// A -stdlib reconvert preserves this file (containsManualConversionMarker).
[module: go.GoManualConversion]

namespace go;

using System.Runtime.InteropServices;
using Δruntime = runtime_package;
using syscall = syscall_package;

partial class os_package {

internal static UntypedInt _P_PID => 1;

// blockUntilWaitable attempts to block until a call to p.Wait will
// succeed immediately, and reports whether it has done so.
// It does not actually call p.Wait.
internal static (bool, error) blockUntilWaitable(this ж<Process> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    // The waitid system call expects a pointer to a siginfo_t,
    // which is 128 bytes on all Linux systems.
    // On darwin/amd64, it requires 104 bytes.
    // We don't care about the values it returns.
    nint psig = Marshal.AllocHGlobal(128);
    syscall.Errno e = default!;
    try {
        while (ᐧ) {
            (_, _, e) = syscall.Syscall6(syscall.SYS_WAITID, _P_PID, (uintptr)p.Pid, (uintptr)psig, (uintptr)((uintptr)syscall.WEXITED | (uintptr)syscall.WNOWAIT), 0, 0);
            if (e != syscall.EINTR) {
                break;
            }
        }
    }
    finally {
        Marshal.FreeHGlobal(psig);
    }
    Δruntime.KeepAlive(Ꮡp.OrTypedNil());
    if (e != 0) {
        // waitid has been available since Linux 2.6.9, but
        // reportedly is not available in Ubuntu on Windows.
        // See issue 16610.
        if (e == syscall.ENOSYS) {
            return (false, default!);
        }
        return (false, NewSyscallError(waitidˢ, e));
    }
    return (true, default!);
}

} // end os_package
