// pidfd_linux_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// Hand-written implementation of os's //go:linkname-into-syscall hook on the Linux flavor
// (pidfd_linux.go's `checkClonePidfd`, provided in Go by syscall.os_checkClonePidfd). go2cs emits
// it as a bodyless `partial` method, and without a body here the PartialStubGenerator fills it
// with a throwing stub — so the pidfd feature PROBE, not any spawn, was what killed the exec path:
// `os.startProcess` → `ensurePidfd` → `checkPidfdOnce` (a sync.OnceValue) → `checkPidfd` →
// `checkClonePidfd` → NotImplementedException. JOB-024 measured that shape on `flag.TestExitCode`
// and across `os/exec`'s suite (the exec-wall design's §8 amendment): the probe threw before the
// landed posix_spawn hand-own in syscall's exec_unix.cs was ever reached, and — before the OQ-6
// replay fix — every caller of the once after the first saw the throw masked as `panic: nil`.
//
// Go's syscall.os_checkClonePidfd verifies clone(CLONE_PIDFD) by ACTUALLY CLONING: it forks a
// child with CLONE_PIDFD, reads the pidfd back, and waits the child. That probe cannot be run
// here by the same rule that displaced forkAndExecInChild (DESIGN-linux-exec.md §2, ratified
// 2026-08-22): the child side between clone() and the probe's exit runs code, and no managed
// instruction is async-signal-safe in a multithreaded CLR process. The design ruled the answer
// for exactly this spot (§3.5 + OQ-4, re-affirmed by the JOB-024 amendment): the go2cs spawn maps
// onto posix_spawn(3), which cannot request CLONE_PIDFD, so the AUTOMATIC pidfd path is honestly
// UNSUPPORTED — one probe result, not a stub. Returning ENOSYS here is the same answer a
// pidfd-less kernel gives Go, and it routes `ensurePidfd`/`pidfdWorks` onto Go's own complete
// fallback: the wait4/waitid(WNOWAIT) road the landed keystone already carries.
//
// This deliberately does NOT report what the three live checks before it prove (pidfd_open,
// waitid(P_PIDFD) and pidfd_send_signal all work on this kernel through the keystone): those
// verify the KERNEL, but Go's automatic pidfd path begins at clone(CLONE_PIDFD) in the spawn
// itself, and posix_spawn cannot mint one. A caller that explicitly requests a pidfd via
// SysProcAttr.PidFD is a different, user-opted door — the spawn hand-own fills it post-spawn via
// pidfd_open(pid) (OQ-4's v2 door, opened there) — and is unaffected by this probe's answer.
// If the pidfd path is ever billed by a roster row, this single body is where the revisit starts.
//
// `ignoreSIGSYS`/`restoreSIGSYS` beside the declaration stay bodyless on purpose: Go calls them
// only on android around the SIGSYS-prone probe, that path is unreachable here, and a throwing
// stub is the loudest honest answer if that ever changes.

namespace go;

partial class os_package
{
    internal static partial error checkClonePidfd()
    {
        // ENOSYS in Go's own error currency: the probe's callers test non-nil and fall back;
        // anything that prints the reason names the unsupported syscall rather than a stub.
        return NewSyscallError("clone(CLONE_PIDFD)"u8, (syscall_package.Errno)syscall_package.ENOSYS);
    }
}
