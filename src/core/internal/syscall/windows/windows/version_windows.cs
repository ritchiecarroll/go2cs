// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.syscall;

using errors = errors_package;
using sync = go.sync_package;
using syscall = syscall_package;
using @unsafe = unsafe_package;
using go;

partial class windows_package {

// https://learn.microsoft.com/en-us/windows-hardware/drivers/ddi/wdm/ns-wdm-_osversioninfow
partial struct _OSVERSIONINFOW {
    internal uint32 osVersionInfoSize;
    internal uint32 majorVersion;
    internal uint32 minorVersion;
    internal uint32 buildNumber;
    internal uint32 platformId;
    internal array<uint16> csdVersion = new(128);
}

// According to documentation, RtlGetVersion function always succeeds.
//sys	rtlGetVersion(info *_OSVERSIONINFOW) = ntdll.RtlGetVersion

// Version retrieves the major, minor, and build version numbers
// of the current Windows OS from the RtlGetVersion API.
public static (uint32 major, uint32 minor, uint32 build) Version() {
    ref var info = ref heap<_OSVERSIONINFOW>(out var Ꮡinfo);
    info = new _OSVERSIONINFOW(nil);
    info.osVersionInfoSize = (uint32)/* unsafe.Sizeof(info) */ (uintptr)276;
    rtlGetVersion(Ꮡinfo);
    return (info.majorVersion, info.minorVersion, info.buildNumber);
}

internal static bool supportTCPKeepAliveIdle;
internal static bool supportTCPKeepAliveInterval;
internal static bool supportTCPKeepAliveCount;

// Fallback to checking the Windows version.
internal static Action initTCPKeepAlive;
internal static void initᴛinitTCPKeepAlive() { initTCPKeepAlive = sync.OnceFunc(() => {
    GoFrame ᒐ = default;
    try {
        var (s, err) = WSASocket(syscall.AF_INET, syscall.SOCK_STREAM, syscall.IPPROTO_TCP, nil, 0, WSA_FLAG_NO_HANDLE_INHERIT);
        if (err != default!) {
            var (major, _, build) = Version();
            supportTCPKeepAliveIdle = major >= 10 && build >= 16299;
            supportTCPKeepAliveInterval = major >= 10 && build >= 16299;
            supportTCPKeepAliveCount = major >= 10 && build >= 15063;
            return;
        }
        defer(syscall.Closesocket, s, ref ᒐ);
        Func<nint, bool> optSupported = (nint opt) => {
            var errΔ1 = syscall.SetsockoptInt(s, syscall.IPPROTO_TCP, opt, 1);
            return !errors.Is(errΔ1, syscall.WSAENOPROTOOPT);
        };
        supportTCPKeepAliveIdle = optSupported(TCP_KEEPIDLE);
        supportTCPKeepAliveInterval = optSupported(TCP_KEEPINTVL);
        supportTCPKeepAliveCount = optSupported(TCP_KEEPCNT);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}); }

// SupportTCPKeepAliveIdle indicates whether TCP_KEEPIDLE is supported.
// The minimal requirement is Windows 10.0.16299.
public static bool SupportTCPKeepAliveIdle() {
    initTCPKeepAlive();
    return supportTCPKeepAliveIdle;
}

// SupportTCPKeepAliveInterval indicates whether TCP_KEEPINTVL is supported.
// The minimal requirement is Windows 10.0.16299.
public static bool SupportTCPKeepAliveInterval() {
    initTCPKeepAlive();
    return supportTCPKeepAliveInterval;
}

// SupportTCPKeepAliveCount indicates whether TCP_KEEPCNT is supported.
// supports TCP_KEEPCNT.
// The minimal requirement is Windows 10.0.15063.
public static bool SupportTCPKeepAliveCount() {
    initTCPKeepAlive();
    return supportTCPKeepAliveCount;
}

// SupportTCPInitialRTONoSYNRetransmissions indicates whether the current
// Windows version supports the TCP_INITIAL_RTO_NO_SYN_RETRANSMISSIONS.
// The minimal requirement is Windows 10.0.16299.
public static Func<bool> SupportTCPInitialRTONoSYNRetransmissions;
internal static void initᴛSupportTCPInitialRTONoSYNRetransmissions() { SupportTCPInitialRTONoSYNRetransmissions = sync.OnceValue(bool () => {
    var (major, _, build) = Version();
    return major >= 10 && build >= 16299;
}); }

// First call to get the required buffer size in bytes.
// Ignore the error, it will always fail.
// Second call to get the actual protocols.
// SupportUnixSocket indicates whether the current Windows version supports
// Unix Domain Sockets.
// The minimal requirement is Windows 10.0.17063.
public static Func<bool> SupportUnixSocket = sync.OnceValue(bool () => {
    ref var size = ref heap(new uint32(), out var Ꮡsize);
    (_, _) = syscall.WSAEnumProtocols(nil, nil, Ꮡsize);
    var n = (int32)size / (int32)/* unsafe.Sizeof(syscall.WSAProtocolInfo{}) */ (uintptr)628;
    var buf = new slice<syscall.WSAProtocolInfo>(n, () => new());
    (n, var err) = syscall.WSAEnumProtocols(nil, Ꮡ(buf, 0), Ꮡsize);
    if (err != default!) {
        return false;
    }
    for (var i = (int32)0; i < n; i++) {
        if (buf[i].AddressFamily == syscall.AF_UNIX) {
            return true;
        }
    }
    return false;
});

} // end windows_package
