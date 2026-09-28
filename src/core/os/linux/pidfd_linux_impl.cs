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
// Go's syscall.os_checkClonePidfd verifies clone(CLONE_PIDFD) by ACTUALLY CLONING, and it is the LAST
// of checkPidfd's four steps: the first three already prove, on this process, that pidfd_open works
// (on getpid()), that waitid(P_PIDFD) is supported (it answers ECHILD, "not our own parent"), and
// that pidfd_send_signal works. Go needs the fourth because Go MINTS a child's pidfd through a
// DIFFERENT door -- clone(CLONE_PIDFD) -- which a seccomp filter or an old kernel can refuse while
// pidfd_open passes. The go2cs spawn has no such second door: posix_spawn cannot request
// CLONE_PIDFD, so the seam mints the child's pidfd by pidfd_open(child) right after the spawn
// (syscall/linux/exec_unix.cs), race-free because an unreaped child's pid cannot be reused. The
// capability that path needs is exactly the one step one proved, so the faithful answer here is nil.
//
// NOT by spawning a probe child, which was the first shape of this body and was MEASURED WRONG
// (2026-09-27): Go's probe clones with exit signal 0 (CLONE_VFORK|CLONE_VM|CLONE_PIDFD, reaped with
// WCLONE), so its child dies silently, while a posix_spawn child always raises SIGCHLD -- the extra
// signal failed os/exec's TestSIGCHLD ("too many SIGCHLD signals"), because the first StartProcess
// of a program that Notify's SIGCHLD runs this probe.
//
// Until 2026-09-27 this body returned ENOSYS (DESIGN-linux-exec.md §3.5 + OQ-4: "posix_spawn cannot
// mint a CLONE_PIDFD, so the automatic pidfd path is unsupported"). COORD's ruling of that date
// classed the resulting skip of os's TestFindProcessViaPidfd/TestStartProcessWithPidfd as a FEATURE
// GAP under the owner's ruling #1 and ordered the feature: the pidfd the seam already minted for an
// explicit SysProcAttr.PidFD is the same one the automatic path needs. With the probe passing, os
// takes its pidfd paths for EVERY process operation (ensurePidfd sets SysProcAttr.PidFD on each
// StartProcess; Wait, Signal and FindProcess go through the pidfd).
//
// `ignoreSIGSYS`/`restoreSIGSYS` beside the declaration stay bodyless on purpose: Go calls them
// only on android around the SIGSYS-prone probe, that path is unreachable here, and a throwing
// stub is the loudest honest answer if that ever changes.

namespace go;

partial class os_package
{
    internal static partial error checkClonePidfd() => default!;
}
