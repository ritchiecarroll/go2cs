// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build (cgo || darwin) && !osusergo && (darwin || dragonfly || freebsd || (linux && !android) || netbsd || openbsd || (solaris && !illumos))
namespace go.os;

using fmt = fmt_package;
using strconv = strconv_package;
using @unsafe = unsafe_package;

partial class user_package {

internal static UntypedInt maxGroups => 2048;

internal static (slice<@string>, error) listGroups(ref User u) {
    var (ug, err) = strconv.Atoi(u.Gid);
    if (err != default!) {
        return (default!, fmt.Errorf("user: list groups for %s: invalid gid %q"u8, u.Username, u.Gid));
    }
    var userGID = ((_C_gid_t)ug);
    var nameC = new slice<byte>(len(u.Username) + 1);
    copy(nameC, u.Username);
    ref var n = ref heap<_C_int>(out var Ꮡn);
    n = ((_C_int)256);
    ref var gidsC = ref heap<slice<_C_gid_t>>(out var ᏑgidsC);
    gidsC = new slice<_C_gid_t>(n);
    var rv = getGroupList(Ꮡ(nameC, 0), userGID, Ꮡ(gidsC, 0), Ꮡn);
    if (rv == -1) {
        // Mac is the only Unix that does not set n properly when rv == -1, so
        // we need to use different logic for Mac vs. the other OS's.
        {
            var errΔ1 = groupRetry(u.Username, nameC, userGID, ᏑgidsC, Ꮡn); if (errΔ1 != default!) {
                return (default!, errΔ1);
            }
        }
    }
    gidsC = gidsC.slice(0, n);
    var gids = new slice<@string>(0, n);
    foreach (var (_, g) in gidsC.slice(0, n)) {
        gids = append(gids, strconv.Itoa((nint)g));
    }
    return (gids, default!);
}

// groupRetry retries getGroupList with much larger size for n. The result is
// stored in gids.
internal static error groupRetry(@string username, slice<byte> name, _C_gid_t userGID, ж<slice<_C_gid_t>> Ꮡgids, ж<_C_int> Ꮡn) {
    ref var gids = ref Ꮡgids.DerefOrNull();
    ref var n = ref Ꮡn.DerefOrNull();

    // More than initial buffer, but now n contains the correct size.
    if (n > maxGroups) {
        return fmt.Errorf("user: %q is a member of more than %d groups"u8, username, (nint)(maxGroups));
    }
    gids = new slice<_C_gid_t>(n);
    var rv = getGroupList(Ꮡ(name, 0), userGID, Ꮡ((gids), 0), Ꮡn);
    if (rv == -1) {
        return fmt.Errorf("user: list groups for %s failed"u8, username);
    }
    return default!;
}

} // end user_package
