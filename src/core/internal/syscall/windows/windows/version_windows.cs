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

// https://learn.microsoft.com/en-us/windows-hardware/drivers/ddi/wdm/ns-wdm-_osversioninfoexw
partial struct _OSVERSIONINFOEXW {
    internal uint32 osVersionInfoSize;
    internal uint32 majorVersion;
    internal uint32 minorVersion;
    internal uint32 buildNumber;
    internal uint32 platformId;
    internal array<uint16> csdVersion = new(128);
    internal uint16 servicePackMajor;
    internal uint16 servicePackMinor;
    internal uint16 suiteMask;
    internal byte productType;
    internal byte reserved;
}

// According to documentation, RtlGetVersion function always succeeds.
//sys	rtlGetVersion(info *_OSVERSIONINFOEXW) = ntdll.RtlGetVersion

// Retrieves version information of the current Windows OS
// from the RtlGetVersion API.
internal static ж<_OSVERSIONINFOEXW> getVersionInfo() {
    ref var info = ref heap<_OSVERSIONINFOEXW>(new _OSVERSIONINFOEXW(), out var Ꮡinfo);
    info.osVersionInfoSize = (uint32)/* unsafe.Sizeof(info) */ (uintptr)284;
    rtlGetVersion(Ꮡinfo);
    return Ꮡinfo;
}

// Version retrieves the major, minor, and build version numbers
// of the current Windows OS from the RtlGetVersion API.
public static (uint32 major, uint32 minor, uint32 build) Version() {
    var info = getVersionInfo();
    return (info.majorVersion, info.minorVersion, info.buildNumber);
}

// SupportUnlimitedTransmitFile indicates whether the current
// Windows version's TransmitFile function imposes any
// concurrent operation limits.
// Workstation and client versions of Windows limit the number
// of concurrent TransmitFile operations allowed on the system
// to a maximum of two. Please see:
// https://learn.microsoft.com/en-us/windows/win32/api/mswsock/nf-mswsock-transmitfile
// https://golang.org/issue/73746
public static readonly ж<Func<bool>> ᏑSupportUnlimitedTransmitFile = new StaticBox<Func<bool>>(default(Func<bool>));
public static ref Func<bool> SupportUnlimitedTransmitFile => ref ((StaticBox<Func<bool>>)ᏑSupportUnlimitedTransmitFile).ValueSlot;
internal static void initᴛSupportUnlimitedTransmitFile() { SupportUnlimitedTransmitFile = sync.OnceValue(bool () => {
    var info = getVersionInfo();
    return info.productType != VER_NT_WORKSTATION;
}); }

internal static bool supportTCPKeepAliveIdle;
internal static bool supportTCPKeepAliveInterval;
internal static bool supportTCPKeepAliveCount;

internal static Action initTCPKeepAlive;
internal static void initᴛinitTCPKeepAlive() { initTCPKeepAlive = sync.OnceFunc(() => {
    GoFrame ᒐ = default;
    try {
        var (s, err) = WSASocket(syscall.AF_INET, syscall.SOCK_STREAM, syscall.IPPROTO_TCP, nil, 0, WSA_FLAG_NO_HANDLE_INHERIT);
        if (err != nil) {
            // Fallback to checking the Windows version.
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
public static readonly ж<Func<bool>> ᏑSupportTCPInitialRTONoSYNRetransmissions = new StaticBox<Func<bool>>(default(Func<bool>));
public static ref Func<bool> SupportTCPInitialRTONoSYNRetransmissions => ref ((StaticBox<Func<bool>>)ᏑSupportTCPInitialRTONoSYNRetransmissions).ValueSlot;
internal static void initᴛSupportTCPInitialRTONoSYNRetransmissions() { SupportTCPInitialRTONoSYNRetransmissions = sync.OnceValue(bool () => {
    var (major, _, build) = Version();
    return major >= 10 && build >= 16299;
}); }

// SupportUnixSocket indicates whether the current Windows version supports
// Unix Domain Sockets.
// The minimal requirement is Windows 10.0.17063.
public static readonly ж<Func<bool>> ᏑSupportUnixSocket = new StaticBox<Func<bool>>(sync.OnceValue(bool () => {
    ref var size = ref heap(new uint32(), out var Ꮡsize);
    // First call to get the required buffer size in bytes.
    // Ignore the error, it will always fail.
    (_, _) = syscall.WSAEnumProtocols(nil, nil, Ꮡsize);
    var n = (int32)size / (int32)/* unsafe.Sizeof(syscall.WSAProtocolInfo{}) */ (uintptr)628;
    // Second call to get the actual protocols.
    var buf = new slice<syscall.WSAProtocolInfo>(n, () => new());
    (n, var err) = syscall.WSAEnumProtocols(nil, Ꮡ(buf, 0), Ꮡsize);
    if (err != nil) {
        return false;
    }
    for (var i = (int32)0; i < n; i++) {
        if (buf[i].AddressFamily == syscall.AF_UNIX) {
            return true;
        }
    }
    return false;
}));
public static ref Func<bool> SupportUnixSocket => ref ((StaticBox<Func<bool>>)ᏑSupportUnixSocket).ValueSlot;

} // end windows_package
