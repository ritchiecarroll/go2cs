// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !osusergo && darwin
namespace go.os;

using unix = @internal.syscall.unix_package;
using @internal.syscall;

partial class user_package {

internal static _C_int getGroupList(ж<_C_char> name, _C_gid_t userGID, ж<_C_gid_t> gids, ж<_C_int> n) {
    var err = unix.Getgrouplist(name, userGID, gids, n);
    if (err != nil) {
        return -1;
    }
    return 0;
}

} // end user_package
