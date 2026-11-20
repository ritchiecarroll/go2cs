// Copyright 2026 The Libc Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

using @unsafe = unsafe_package;
using unix = golang.org.x.sys.unix_package;
using golang.org.x.sys;
using syscall = syscall_package;

partial class libc_package {

partial struct kernelSigaction {
    internal uintptr handler;
    internal uintptr flags;
    internal uintptr restorer;
    internal uint64 mask;
}

internal static void resetSigDfl(int32 sig) {
    ref var sa = ref heap<kernelSigaction>(new kernelSigaction(handler: SIG_DFL), out var Ꮡsa);
    unix.RawSyscall6(unix.SYS_RT_SIGACTION, (uintptr)sig, (uintptr)Ꮡsa, 0, /* unsafe.Sizeof(sa.mask) */ (uintptr)8, 0, 0);
    ref var set = ref heap<uint64>(((uint64)1).Lsh(((nuint)sig - 1)), out var Ꮡset);
    unix.RawSyscall6(unix.SYS_RT_SIGPROCMASK, SIG_UNBLOCK, (uintptr)Ꮡset, 0, /* unsafe.Sizeof(set) */ (uintptr)8, 0, 0);
}

} // end libc_package
