// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !netgo && darwin
global using _C_char = byte;
global using _C_int = int;
global using _C_uchar = byte;
global using _C_uint = uint;
global using _C_socklen_t = nint;
global using _C_struct___res_state = go.@internal.syscall.unix_package.ResState;
global using _C_struct_addrinfo = go.@internal.syscall.unix_package.Addrinfo;
global using _C_struct_sockaddr = go.syscall_package.RawSockaddr;

namespace go;

using unix = @internal.syscall.unix_package;
using runtime = runtime_package;
using syscall = syscall_package;
using @unsafe = unsafe_package;
using @internal.syscall;

partial class net_package {

internal static UntypedInt _C_AF_INET => /* syscall.AF_INET */ 2;
internal static UntypedInt _C_AF_INET6 => /* syscall.AF_INET6 */ 30;
internal static UntypedInt _C_AF_UNSPEC => /* syscall.AF_UNSPEC */ 0;
internal static UntypedInt _C_EAI_ADDRFAMILY => /* unix.EAI_ADDRFAMILY */ 1;
internal static UntypedInt _C_EAI_AGAIN => /* unix.EAI_AGAIN */ 2;
internal static UntypedInt _C_EAI_NONAME => /* unix.EAI_NONAME */ 8;
internal static UntypedInt _C_EAI_SERVICE => /* unix.EAI_SERVICE */ 9;
internal static UntypedInt _C_EAI_NODATA => /* unix.EAI_NODATA */ 7;
internal static UntypedInt _C_EAI_OVERFLOW => /* unix.EAI_OVERFLOW */ 14;
internal static UntypedInt _C_EAI_SYSTEM => /* unix.EAI_SYSTEM */ 11;
internal static UntypedInt _C_IPPROTO_TCP => /* syscall.IPPROTO_TCP */ 6;
internal static UntypedInt _C_IPPROTO_UDP => /* syscall.IPPROTO_UDP */ 17;
internal static UntypedInt _C_SOCK_DGRAM => /* syscall.SOCK_DGRAM */ 2;
internal static UntypedInt _C_SOCK_STREAM => /* syscall.SOCK_STREAM */ 1;

internal static void _C_free(@unsafe.Pointer p) {
    runtime.KeepAlive(@unsafe.Pointer.OrTypedNil(p));
}

internal static @unsafe.Pointer _C_malloc(uintptr n) {
    if (n <= 0) {
        n = 1;
    }
    return @unsafe.Pointer.FromPinnedBox(Ꮡ(new slice<byte>((nint)(n)), 0));
}

internal static ж<ж<_C_struct_sockaddr>> _C_ai_addr(ж<_C_struct_addrinfo> ai) {
    return ai.of(_C_struct_addrinfo.ᏑAddr);
}

internal static ж<_C_int> _C_ai_family(ж<_C_struct_addrinfo> ai) {
    return ai.of(_C_struct_addrinfo.ᏑFamily);
}

internal static ж<_C_int> _C_ai_flags(ж<_C_struct_addrinfo> ai) {
    return ai.of(_C_struct_addrinfo.ᏑFlags);
}

internal static ж<ж<_C_struct_addrinfo>> _C_ai_next(ж<_C_struct_addrinfo> ai) {
    return ai.of(_C_struct_addrinfo.ᏑNext);
}

internal static ж<_C_int> _C_ai_protocol(ж<_C_struct_addrinfo> ai) {
    return ai.of(_C_struct_addrinfo.ᏑProtocol);
}

internal static ж<_C_int> _C_ai_socktype(ж<_C_struct_addrinfo> ai) {
    return ai.of(_C_struct_addrinfo.ᏑSocktype);
}

internal static void _C_freeaddrinfo(ж<_C_struct_addrinfo> ai) {
    unix.Freeaddrinfo(ai);
}

internal static @string _C_gai_strerror(_C_int eai) {
    return unix.GaiStrerror((nint)eai);
}

internal static (nint, error) _C_getaddrinfo(ж<byte> hostname, ж<byte> servname, ж<_C_struct_addrinfo> hints, ж<ж<_C_struct_addrinfo>> res) {
    return unix.Getaddrinfo(hostname, servname, hints, res);
}

internal static error _C_res_ninit(ж<_C_struct___res_state> state) {
    unix.ResNinit(state);
    return default;
}

internal static nint _C_res_nsearch(ж<_C_struct___res_state> state, ж<_C_char> dname, nint @class, nint typ, ж<_C_char> ans, nint anslen) {
    var (x, _) = unix.ResNsearch(state, dname, @class, typ, ans, anslen);
    return x;
}

internal static void _C_res_nclose(ж<_C_struct___res_state> state) {
    unix.ResNclose(state);
}

internal static (nint, error) cgoNameinfoPTR(slice<byte> b, ж<syscall.RawSockaddr> sa, nint salen) {
    return unix.Getnameinfo(sa, salen, Ꮡ(b, 0), len(b), nil, 0, unix.NI_NAMEREQD);
}

internal static ж<syscall.RawSockaddr> cgoSockaddrInet4(IP ip) {
    ref var sa = ref heap<syscall.RawSockaddrInet4>(new syscall.RawSockaddrInet4(Len: syscall.SizeofSockaddrInet4, Family: syscall.AF_INET), out var Ꮡsa);
    copy(sa.Addr[..], ip);
    return Ꮡsa.Reinterpret<syscall.RawSockaddrInet4, syscall.RawSockaddr>();
}

internal static ж<syscall.RawSockaddr> cgoSockaddrInet6(IP ip, nint zone) {
    ref var sa = ref heap<syscall.RawSockaddrInet6>(new syscall.RawSockaddrInet6(Len: syscall.SizeofSockaddrInet6, Family: syscall.AF_INET6, Scope_id: (uint32)zone), out var Ꮡsa);
    copy(sa.Addr[..], ip);
    return Ꮡsa.Reinterpret<syscall.RawSockaddrInet6, syscall.RawSockaddr>();
}

} // end net_package
