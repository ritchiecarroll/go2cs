// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build (unix && !linux) || (js && wasm) || wasip1 || windows
namespace go;

using syscall = syscall_package;

partial class os_package {

internal static (ж<syscall.SysProcAttr>, bool) ensurePidfd(ж<syscall.SysProcAttr> ᏑsysAttr) {
    return (ᏑsysAttr, false);
}

internal static (uintptr, bool) getPidfd(ж<syscall.SysProcAttr> _Δp0, bool _Δp1) {
    return (0, false);
}

internal static (uintptr, error) pidfdFind(nint _) {
    return (0, syscall.ENOSYS);
}

internal static (ж<ProcessState>, error) pidfdWait(this ref Process _) {
    throw panic("unreachable");
}

internal static error pidfdSendSignal(this ref Process _Δp0, syscallꓸSignal _Δp1) {
    throw panic("unreachable");
}

} // end os_package
