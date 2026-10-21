// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix && !ios && !android
// Parse "zoneinfo" time zone file.
// This is a fairly standard file format used on OS X, Linux, BSD, Sun, and others.
// See tzfile(5), https://en.wikipedia.org/wiki/Zoneinfo,
// and ftp://munnari.oz.au/pub/oldtz/
namespace go;

using syscall = syscall_package;

partial class time_package {

// Many systems use /usr/share/zoneinfo, Solaris 2 has
// /usr/share/lib/zoneinfo, IRIX 6 has /usr/lib/locale/TZ,
// NixOS has /etc/zoneinfo.
internal static slice<@string> platformZoneSources = new @string[]{
    "/usr/share/zoneinfo/"u8,
    "/usr/share/lib/zoneinfo/"u8,
    "/usr/lib/locale/TZ/"u8,
    "/etc/zoneinfo"u8
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string localtimeˢ = "localtime"u8;
internal static readonly @string localˢ = "Local"u8;

internal static void initLocal() {
    // consult $TZ to find the time zone to use.
    // no $TZ means use the system default /etc/localtime.
    // $TZ="" means use UTC.
    // $TZ="foo" or $TZ=":foo" if foo is an absolute path, then the file pointed
    // by foo will be used to initialize timezone; otherwise, file
    // /usr/share/zoneinfo/foo will be used.

    var (tz, ok) = syscall.Getenv("TZ"u8);
    switch (ᐧ) {
    case {} when !ok: {
        var (z, err) = loadLocation(localtimeˢ, new @string[]{"/etc"u8}.slice());
        if (err == default!) {
            localLoc = z.Value;
            localLoc.name = localˢ;
            return;
        }
        break;
    }
    case {} when tz != ""u8: {
        if (tz[0] == (rune)':') {
            tz = tz[1..];
        }
        if (tz != ""u8 && tz[0] == (rune)'/'){
            {
                var (z, err) = loadLocation(tz, new @string[]{""u8}.slice()); if (err == default!) {
                    localLoc = z.Value;
                    if (tz == "/etc/localtime"u8){
                        localLoc.name = localˢ;
                    } else {
                        localLoc.name = tz;
                    }
                    return;
                }
            }
        } else 
        if (tz != ""u8 && tz != "UTC"u8) {
            {
                var (z, err) = loadLocation(tz, platformZoneSources); if (err == default!) {
                    localLoc = z.Value;
                    return;
                }
            }
        }
        break;
    }}

    // Fall back to UTC.
    localLoc.name = utcˢ;
}

} // end time_package
