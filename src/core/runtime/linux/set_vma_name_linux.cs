// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build linux
namespace go;

using atomic = @internal.runtime.atomic_package;
using linux = @internal.runtime.syscall.linux_package;
using @unsafe = unsafe_package;
using @internal.runtime;
using @internal.runtime.syscall;

partial class runtime_package {

internal static ж<atomic.Bool> ᏑprSetVMAUnsupported = new StandardBox<atomic.Bool>(default(atomic.Bool));
internal static ref atomic.Bool prSetVMAUnsupported => ref ᏑprSetVMAUnsupported.Value;

internal static bool setVMANameSupported() {
    return !ᏑprSetVMAUnsupported.Load();
}

// setVMAName calls prctl(PR_SET_VMA, PR_SET_VMA_ANON_NAME, start, len, name)
internal static void setVMAName(@unsafe.Pointer start, uintptr length, @string name) {
    if (debug.decoratemappings == 0 || !setVMANameSupported()) {
        return;
    }

    ref var sysName = ref heap(new array<byte>(80), out var ᏑsysName);
    nint n = copy(sysName[..], " Go: "u8);
    copy(sysName.slice(n, 79), name); // leave final byte zero

    var (_, _, err) = linux.Syscall6(linux.SYS_PRCTL, linux.PR_SET_VMA, linux.PR_SET_VMA_ANON_NAME, (uintptr)start, length, (uintptr)ᏑsysName.at<byte>(0), 0);
    if (err == _EINVAL) {
        ᏑprSetVMAUnsupported.Store(true);
    }
    // ignore other errors
}

} // end runtime_package
