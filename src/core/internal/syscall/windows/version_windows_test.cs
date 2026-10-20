// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.syscall;

using errors = errors_package;
using Δwindows = go.@internal.syscall.windows_package;
using syscall = syscall_package;
using testing = testing_package;
using go.@internal.syscall;

partial class windows_test_package {

public static void TestSupportUnixSocket(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var d = ref heap(new syscall.WSAData(), out var Ꮡd);
        {
            var errΔ1 = syscall.WSAStartup((uint32)0x202, Ꮡd); if (errΔ1 != default!) {
                Ꮡt.Fatal(errΔ1);
            }
        }
        defer(() => syscall.WSACleanup(), ref ᒐ);

        // Test that SupportUnixSocket returns true if WSASocket succeeds with AF_UNIX.
        var got = Δwindows.SupportUnixSocket();
        var (s, err) = Δwindows.WSASocket(syscall.AF_UNIX, syscall.SOCK_STREAM, 0, nil, 0, Δwindows.WSA_FLAG_NO_HANDLE_INHERIT);
        if (err == default!) {
            syscall.Closesocket(s);
        }
        var want = !errors.Is(err, Δwindows.WSAEAFNOSUPPORT) && !errors.Is(err, Δwindows.WSAEINVAL);
        if (want != got) {
            Ꮡt.Errorf("SupportUnixSocket = %v; want %v"u8, got, want);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end windows_test_package
