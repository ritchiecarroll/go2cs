// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build ((unix && !android) || (js && wasm) || wasip1) && ((!cgo && !darwin) || osusergo)
namespace go.os;

using bufio = bufio_package;
using bytes = bytes_package;
using errors = errors_package;
using io = io_package;
using os = os_package;
using strconv = strconv_package;
using strings = strings_package;

partial class user_package {

// lineFunc returns a value, an error, or (nil, nil) to skip the row.
// type lineFunc is a methodless func type — rendered inline as its base delegate

// readColonFile parses r as an /etc/group or /etc/passwd style file, running
// fn for each row. readColonFile returns a value, an error, or (nil, nil) if
// the end of the file is reached without a match.
//
// readCols is the minimum number of colon-separated fields that will be passed
// to fn; in a long line additional fields may be silently discarded.
internal static (any v, error err) readColonFile(io.Reader r, Func<slice<byte>, (any, error)> fn, nint readCols) {
    any v = default;
    error err = default;

    var rd = bufio.NewReader(r);

    // Read the file line-by-line.
    for (;;) {
        bool isPrefix = default;
        slice<byte> wholeLine = default;

        // Read the next line. We do so in chunks (as much as reader's
        // buffer is able to keep), check if we read enough columns
        // already on each step and store final result in wholeLine.
        for (;;) {
            slice<byte> line = default;
            (line, isPrefix, err) = rd.ReadLine();

            if (err != nil) {
                // We should return (nil, nil) if EOF is reached
                // without a match.
                if (AreEqual(err, io.EOF)) {
                    err = default;
                }
                return (default, err);
            }

            // Simple common case: line is short enough to fit in a
            // single reader's buffer.
            if (!isPrefix && len(wholeLine) == 0) {
                wholeLine = line;
                break;
            }

            wholeLine = appendꓸꓸꓸ(wholeLine, line);

            // Check if we read the whole line (or enough columns)
            // already.
            if (!isPrefix || bytes.Count(wholeLine, slice<byte>([(byte)':'])) >= readCols) {
                break;
            }
        }

        // There's no spec for /etc/passwd or /etc/group, but we try to follow
        // the same rules as the glibc parser, which allows comments and blank
        // space at the beginning of a line.
        wholeLine = bytes.TrimSpace(wholeLine);
        if (len(wholeLine) == 0 || wholeLine[0] == '#') {
            continue;
        }
        (v, err) = fn(wholeLine);
        if (v != nil || err != nil) {
            return (v, err);
        }

        // If necessary, skip the rest of the line
        for (; isPrefix; (_, isPrefix, err) = rd.ReadLine()) {
            if (err != nil) {
                // We should return (nil, nil) if EOF is reached without a match.
                if (AreEqual(err, io.EOF)) {
                    err = default;
                }
                return (default, err);
            }
        }
    }
}

internal static Func<slice<byte>, (any, error)> matchGroupIndexValue(@string value, nint idx) {
    @string leadColon = default;
    if (idx > 0) {
        leadColon = ":"u8;
    }
    var substr = slice<byte>(leadColon + value + ":");
    var substrʗ1 = substr;
    return (slice<byte> line) => {
        any v = default;
        error err = default;
        if (!bytes.Contains(line, substrʗ1) || bytes.Count(line, colon) < 3) {
            return (v, err);
        }
        // wheel:*:0:root
        var parts = strings.SplitN((@string)line, ":"u8, 4);
        if (len(parts) < 4 || parts[0] == "" || parts[idx] != value ||
            // If the file contains +foo and you search for "foo", glibc
            // returns an "invalid argument" error. Similarly, if you search
            // for a gid for a row where the group name starts with "+" or "-",
            // glibc fails to find the record.
            parts[0][0] == '+' || parts[0][0] == '-') {
            return (v, err);
        }
        {
            var (_, errΔ1) = strconv.Atoi(parts[2]); if (errΔ1 != nil) {
                return (default, default);
            }
        }
        return (Ꮡ(new Group(Name: parts[0], Gid: parts[2])), default);
    };
}

internal static (ж<Group>, error) findGroupId(@string id, io.Reader r) {
    {
        var (v, err) = readColonFile(r, matchGroupIndexValue(id, 2), 3); if (err != nil) {
            return (default, err);
        } else if (v != nil) {
            return (v._<ж<Group>>(), default);
        }
    }
    return (default, ((UnknownGroupIdError)id));
}

internal static (ж<Group>, error) findGroupName(@string name, io.Reader r) {
    {
        var (v, err) = readColonFile(r, matchGroupIndexValue(name, 0), 3); if (err != nil) {
            return (default, err);
        } else if (v != nil) {
            return (v._<ж<Group>>(), default);
        }
    }
    return (default, ((UnknownGroupError)name));
}

// returns a *User for a row if that row's has the given value at the
// given index.
internal static Func<slice<byte>, (any, error)> matchUserIndexValue(@string value, nint idx) {
    @string leadColon = default;
    if (idx > 0) {
        leadColon = ":"u8;
    }
    var substr = slice<byte>(leadColon + value + ":");
    var substrʗ1 = substr;
    return (slice<byte> line) => {
        any v = default;
        error err = default;
        if (!bytes.Contains(line, substrʗ1) || bytes.Count(line, colon) < 6) {
            return (v, err);
        }
        // kevin:x:1005:1006::/home/kevin:/usr/bin/zsh
        var parts = strings.SplitN((@string)line, ":"u8, 7);
        if (len(parts) < 6 || parts[idx] != value || parts[0] == "" || parts[0][0] == '+' || parts[0][0] == '-') {
            return (v, err);
        }
        {
            var (_, errΔ1) = strconv.Atoi(parts[2]); if (errΔ1 != nil) {
                return (default, default);
            }
        }
        {
            var (_, errΔ2) = strconv.Atoi(parts[3]); if (errΔ2 != nil) {
                return (default, default);
            }
        }
        var u = Ꮡ(new User(
            Username: parts[0],
            Uid: parts[2],
            Gid: parts[3],
            Name: parts[4],
            HomeDir: parts[5]
        ));
        // The pw_gecos field isn't quite standardized. Some docs
        // say: "It is expected to be a comma separated list of
        // personal data where the first item is the full name of the
        // user."
        (u.Name, _, _) = strings.Cut(u.Name, ","u8);
        return (u.OrTypedNil(), default);
    };
}

internal static (ж<User>, error) findUserId(@string uid, io.Reader r) {
    var (i, e) = strconv.Atoi(uid);
    if (e != nil) {
        return (default, errors.New("user: invalid userid "u8 + uid));
    }
    {
        var (v, err) = readColonFile(r, matchUserIndexValue(uid, 2), 6); if (err != nil) {
            return (default, err);
        } else if (v != nil) {
            return (v._<ж<User>>(), default);
        }
    }
    return (default, ((UnknownUserIdError)i));
}

internal static (ж<User>, error) findUsername(@string name, io.Reader r) {
    {
        var (v, err) = readColonFile(r, matchUserIndexValue(name, 0), 6); if (err != nil) {
            return (default, err);
        } else if (v != nil) {
            return (v._<ж<User>>(), default);
        }
    }
    return (default, ((UnknownUserError)name));
}

internal static (ж<Group>, error) lookupGroup(@string groupname) {
    GoFrame ᒐ = default;
    try {
        var (f, err) = os.Open(groupFile);
        if (err != nil) {
            return (default, err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        return findGroupName(groupname, new os_FileжReader(f));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

internal static (ж<Group>, error) lookupGroupId(@string id) {
    GoFrame ᒐ = default;
    try {
        var (f, err) = os.Open(groupFile);
        if (err != nil) {
            return (default, err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        return findGroupId(id, new os_FileжReader(f));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

internal static (ж<User>, error) lookupUser(@string username) {
    GoFrame ᒐ = default;
    try {
        var (f, err) = os.Open(userFile);
        if (err != nil) {
            return (default, err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        return findUsername(username, new os_FileжReader(f));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

internal static (ж<User>, error) lookupUserId(@string uid) {
    GoFrame ᒐ = default;
    try {
        var (f, err) = os.Open(userFile);
        if (err != nil) {
            return (default, err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        return findUserId(uid, new os_FileжReader(f));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

} // end user_package
