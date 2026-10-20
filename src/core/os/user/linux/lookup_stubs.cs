// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build (!cgo && !darwin && !windows && !plan9) || android || (osusergo && !windows && !plan9)
namespace go.os;

using fmt = fmt_package;
using os = os_package;
using runtime = runtime_package;
using strconv = strconv_package;

partial class user_package {

// unused variables (in this implementation)
// modified during test to exercise code paths in the cgo implementation.
internal static nint userBuffer = 0;
internal static nint groupBuffer = 0;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string userˢ = "USER"u8;
internal static readonly @string androidˢ = "android"u8;
internal static readonly @string userˢ2 = "$USER"u8;

internal static (ж<User>, error) current() {
    ref var uid = ref heap<@string>(out var Ꮡuid);
    uid = currentUID();
    // $USER and /etc/passwd may disagree; prefer the latter if we can get it.
    // See issue 27524 for more information.
    var (u, err) = lookupUserId(uid);
    if (err == default!) {
        return (u, default!);
    }

    ref var homeDir = ref heap<@string>(out var ᏑhomeDir);
    (homeDir, _) = os.UserHomeDir();
    u = Ꮡ(new User(
        Uid: uid,
        Gid: currentGID(),
        Username: os.Getenv(userˢ),
        Name: ""u8, // ignored
        HomeDir: homeDir
    ));
    // On Android, return a dummy user instead of failing.
    var exprᴛ1 = runtime.GOOS;
    if (exprᴛ1 == "android"u8) {
        if ((~u).Uid == ""u8) {
            u.Value.Uid = "1"u8;
        }
        if ((~u).Username == ""u8) {
            u.Value.Username = androidˢ;
        }
    }
    // cgo isn't available, but if we found the minimum information
    // without it, use it:
    if ((~u).Uid != ""u8 && (~u).Username != ""u8 && (~u).HomeDir != ""u8) {
        return (u, default!);
    }
    @string missing = default!;
    if ((~u).Username == ""u8) {
        missing = userˢ2;
    }
    if ((~u).HomeDir == ""u8) {
        if (missing != ""u8) {
            missing += ", "u8;
        }
        missing += "$HOME"u8;
    }
    return (u, fmt.Errorf("user: Current requires cgo or %s set in environment"u8, missing));
}

internal static @string currentUID() {
    {
        nint id = os.Getuid(); if (id >= 0) {
            return strconv.Itoa(id);
        }
    }
    // Note: Windows returns -1, but this file isn't used on
    // Windows anyway, so this empty return path shouldn't be
    // used.
    return ""u8;
}

internal static @string currentGID() {
    {
        nint id = os.Getgid(); if (id >= 0) {
            return strconv.Itoa(id);
        }
    }
    return ""u8;
}

} // end user_package
