// corefoundation_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

namespace go.crypto.x509.@internal;

partial class macos_package
{
    // syscall is Go's runtime·syscall_x509 (runtime/sys_darwin.go pushes it here by //go:linkname; the
    // body is runtime/sys_darwin_{amd64,arm64}.s): one call of a C function whose address `fn` is, taking
    // five pointer-sized integers and one float64, returning one value and checking nothing. Every
    // CoreFoundation and Security.framework call crypto/x509 makes on macOS goes through it, with `fn`
    // being FuncPCABI0(x509_<sym>_trampoline), which the class-B resolver turns into the framework
    // export's real address from this assembly's GoCgoImportDynamic records (package_info.cs names each
    // framework binary by path).
    //
    // The push never arrived, so until this companion the PartialStubGenerator's throwing body answered,
    // and Certificate.Verify with system roots threw NotImplementedException on both mac chips (the
    // Phase 5 exported-reach census). GoLibcCall.CallFiveAndDouble places the float64 in the first
    // floating-point register as Go's entry does, and marshals the out-parameters these wrappers pass as
    // unsafe.Pointer(&local). Guarded by the X509SystemVerify behavioral test.
    internal static partial uintptr syscall(uintptr fn, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, float64 f1) =>
        (uintptr)GoLibcCall.CallFiveAndDouble(unchecked((nint)(nuint)fn), a1, a2, a3, a4, a5, f1);
}
