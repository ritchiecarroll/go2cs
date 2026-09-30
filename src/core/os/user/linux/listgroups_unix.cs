// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build ((darwin || dragonfly || freebsd || (js && wasm) || wasip1 || (!android && linux) || netbsd || openbsd || solaris) && ((!cgo && !darwin) || osusergo)) || aix || illumos
namespace go.os;

using bufio = bufio_package;
using bytes = bytes_package;
using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using os = os_package;
using strconv = strconv_package;

partial class user_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string userListGroupsEmptyˢ = "user: list groups: empty username"u8;

internal static (slice<@string>, error) listGroupsFromReader(ref User u, io.Reader r) {
    if (u.Username == ""u8) {
        return (default!, errors.New(userListGroupsEmptyˢ));
    }
    var (primaryGid, err) = strconv.Atoi(u.Gid);
    if (err != default!) {
        return (default!, fmt.Errorf("user: list groups for %s: invalid gid %q"u8, u.Username, u.Gid));
    }
    var userCommas = slice<byte>("," + u.Username + ","); // ,john,
    var userFirst = userCommas[1..]; // john,
    var userLast = userCommas.slice(0, len(userCommas) - 1); // ,john
    var userOnly = userCommas.slice(1, len(userCommas) - 1); // john
    // Add primary Gid first.
    var groups = new @string[]{u.Gid}.slice();
    var rd = bufio.NewReader(r);
    var done = false;
    while (!done) {
        var (line, errΔ1) = rd.ReadBytes((rune)'\n');
        if (errΔ1 != default!) {
            if (AreEqual(errΔ1, io.EOF)){
                done = true;
            } else {
                return (groups, errΔ1);
            }
        }
        // Look for username in the list of users. If user is found,
        // append the GID to the groups slice.
        // There's no spec for /etc/passwd or /etc/group, but we try to follow
        // the same rules as the glibc parser, which allows comments and blank
        // space at the beginning of a line.
        line = bytes.TrimSpace(line);
        if (len(line) == 0 || line[0] == (rune)'#' || line[0] == (rune)'+' || line[0] == (rune)'-') {
            // If you search for a gid in a row where the group
            // name (the first field) starts with "+" or "-",
            // glibc fails to find the record, and so should we.
            continue;
        }
        // Format of /etc/group is
        // 	groupname:password:GID:user_list
        // for example
        // 	wheel:x:10:john,paul,jack
        //	tcpdump:x:72:
        nint listIdx = bytes.LastIndexByte(line, (rune)':');
        if (listIdx == -1 || listIdx == len(line) - 1) {
            // No commas, or empty group list.
            continue;
        }
        if (bytes.Count(line.slice(0, listIdx), colon) != 2) {
            // Incorrect number of colons.
            continue;
        }
        var list = line.slice(listIdx + 1);
        // Check the list for user without splitting or copying.
        if (!(bytes.Equal(list, userOnly) || bytes.HasPrefix(list, userFirst) || bytes.HasSuffix(list, userLast) || bytes.Contains(list, userCommas))) {
            continue;
        }
        // groupname:password:GID
        var parts = bytes.Split(line.slice(0, listIdx), colon);
        if (len(parts) != 3 || len(parts[0]) == 0) {
            continue;
        }
        @string gid = ((@string)parts[2]);
        // Make sure it's numeric and not the same as primary GID.
        (var numGid, errΔ1) = strconv.Atoi(gid);
        if (errΔ1 != default! || numGid == primaryGid) {
            continue;
        }
        groups = append(groups, gid);
    }
    return (groups, default!);
}

internal static (slice<@string>, error) listGroups(ref User u) {
    GoFrame ᒐ = default;
    try {
        var (f, err) = os.Open(groupFile);
        if (err != default!) {
            return (default!, err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        return listGroupsFromReader(ref u, new os_FileжReader(f));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

} // end user_package
