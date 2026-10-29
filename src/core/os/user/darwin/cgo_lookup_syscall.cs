// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !osusergo && darwin
global using _C_char = byte;
global using _C_int = int;
global using _C_gid_t = uint;
global using _C_uid_t = uint;
global using _C_size_t = go.uintptr;
global using _C_struct_group = go.@internal.syscall.unix_package.Group;
global using _C_struct_passwd = go.@internal.syscall.unix_package.Passwd;
global using _C_long = long;

namespace go.os;

using unix = @internal.syscall.unix_package;
using syscall = syscall_package;
using @internal.syscall;

partial class user_package {

internal static _C_uid_t _C_pw_uid(ref _C_struct_passwd p) {
    return p.Uid;
}

internal static ж<_C_uid_t> _C_pw_uidp(ж<_C_struct_passwd> p) {
    return p.of(_C_struct_passwd.ᏑUid);
}

internal static _C_gid_t _C_pw_gid(ref _C_struct_passwd p) {
    return p.Gid;
}

internal static ж<_C_gid_t> _C_pw_gidp(ж<_C_struct_passwd> p) {
    return p.of(_C_struct_passwd.ᏑGid);
}

internal static ж<_C_char> _C_pw_name(ref _C_struct_passwd p) {
    return p.Name;
}

internal static ж<_C_char> _C_pw_gecos(ref _C_struct_passwd p) {
    return p.Gecos;
}

internal static ж<_C_char> _C_pw_dir(ref _C_struct_passwd p) {
    return p.Dir;
}

internal static _C_gid_t _C_gr_gid(ref _C_struct_group g) {
    return g.Gid;
}

internal static ж<_C_char> _C_gr_name(ref _C_struct_group g) {
    return g.Name;
}

internal static @string _C_GoString(ж<_C_char> p) {
    return unix.GoString(p);
}

internal static (_C_struct_passwd pwd, bool found, syscall.Errno errno) _C_getpwnam_r(ж<_C_char> name, ж<_C_char> buf, _C_size_t size) {
    ref var pwd = ref heap(new _C_struct_passwd(), out var Ꮡpwd);
    syscall.Errno errno = default;

    ref var result = ref heap<ж<_C_struct_passwd>>(out var Ꮡresult);
    errno = unix.Getpwnam(name, Ꮡpwd, buf, size, Ꮡresult);
    return (pwd, result != nil, errno);
}

internal static (_C_struct_passwd pwd, bool found, syscall.Errno errno) _C_getpwuid_r(_C_uid_t uid, ж<_C_char> buf, _C_size_t size) {
    ref var pwd = ref heap(new _C_struct_passwd(), out var Ꮡpwd);
    syscall.Errno errno = default;

    ref var result = ref heap<ж<_C_struct_passwd>>(out var Ꮡresult);
    errno = unix.Getpwuid(uid, Ꮡpwd, buf, size, Ꮡresult);
    return (pwd, result != nil, errno);
}

internal static (_C_struct_group grp, bool found, syscall.Errno errno) _C_getgrnam_r(ж<_C_char> name, ж<_C_char> buf, _C_size_t size) {
    ref var grp = ref heap(new _C_struct_group(), out var Ꮡgrp);
    syscall.Errno errno = default;

    ref var result = ref heap<ж<_C_struct_group>>(out var Ꮡresult);
    errno = unix.Getgrnam(name, Ꮡgrp, buf, size, Ꮡresult);
    return (grp, result != nil, errno);
}

internal static (_C_struct_group grp, bool found, syscall.Errno errno) _C_getgrgid_r(_C_gid_t gid, ж<_C_char> buf, _C_size_t size) {
    ref var grp = ref heap(new _C_struct_group(), out var Ꮡgrp);
    syscall.Errno errno = default;

    ref var result = ref heap<ж<_C_struct_group>>(out var Ꮡresult);
    errno = unix.Getgrgid(gid, Ꮡgrp, buf, size, Ꮡresult);
    return (grp, result != nil, errno);
}

internal static UntypedInt _C__SC_GETPW_R_SIZE_MAX => /* unix.SC_GETPW_R_SIZE_MAX */ 71;
internal static UntypedInt _C__SC_GETGR_R_SIZE_MAX => /* unix.SC_GETGR_R_SIZE_MAX */ 70;

internal static _C_long _C_sysconf(_C_int key) {
    return unix.Sysconf(key);
}

} // end user_package
