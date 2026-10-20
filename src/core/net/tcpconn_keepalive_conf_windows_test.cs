// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build windows
global using fdType = go.syscall_package.ΔHandle;

namespace go;

using windows = @internal.syscall.windows_package;
using syscall = syscall_package;
using testing = testing_package;
using @internal.syscall;
using static go.net_package;

partial class net_internal_test_package {

internal static UntypedInt syscall_TCP_KEEPIDLE => /* windows.TCP_KEEPIDLE */ 3;
internal static UntypedInt syscall_TCP_KEEPCNT => /* windows.TCP_KEEPCNT */ 16;
internal static UntypedInt syscall_TCP_KEEPINTVL => /* windows.TCP_KEEPINTVL */ 17;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingOnWindowsˢ = (@string)"skipping on windows"u8;

internal static void maybeSkipKeepAliveTest(ж<testing.T> Ꮡt) {
    // TODO(panjf2000): Unlike Unix-like OS's, old Windows (prior to Windows 10, version 1709)
    // 	doesn't provide any ways to retrieve the current TCP keep-alive settings, therefore
    // 	we're not able to run the test suite similar to Unix-like OS's on Windows.
    //  Try to find another proper approach to test the keep-alive settings on old Windows.
    if (!windows.SupportTCPKeepAliveIdle() || !windows.SupportTCPKeepAliveInterval() || !windows.SupportTCPKeepAliveCount()) {
        Ꮡt.Skip(skippingOnWindowsˢ);
    }
}

} // end net_internal_test_package
