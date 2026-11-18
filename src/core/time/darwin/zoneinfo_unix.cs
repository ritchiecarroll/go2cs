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
internal static slice<@string> platformZoneSources = [
    "/usr/share/zoneinfo/",
    "/usr/share/lib/zoneinfo/",
    "/usr/lib/locale/TZ/",
    "/etc/zoneinfo"
];

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string localtimeˢ => ref localtimeˢᵇ;
private static readonly @string localtimeˢᵇ = "localtime"u8;
internal static ref readonly @string localˢ => ref localˢᵇ;
private static readonly @string localˢᵇ = "Local"u8;
internal static ref readonly @string utcˢ => ref utcˢᵇ;
private static readonly @string utcˢᵇ = "UTC"u8;

internal static void initLocal() {
    // consult $TZ to find the time zone to use.
    // no $TZ means use the system default /etc/localtime.
    // $TZ="" means use UTC.
    // $TZ="foo" or $TZ=":foo" if foo is an absolute path, then the file pointed
    // by foo will be used to initialize timezone; otherwise, file
    // /usr/share/zoneinfo/foo will be used.

    var (tz, ok) = syscall.Getenv("TZ");
    switch (ᐧ) {
    case {} when !ok: {
        var (z, err) = loadLocation(localtimeˢ, ["/etc"]);
        if (err == nil) {
            localLoc = z.Value;
            localLoc.name = localˢ;
            return;
        }
        break;
    }
    case {} when tz != ""u8: {
        if (tz[0] == ':') {
            tz = tz[1..];
        }
        if (tz != ""u8 && tz[0] == '/') {
            var (z, err) = loadLocation(tz, [""]); if (err == nil) {
                localLoc = z.Value;
                if (tz == "/etc/localtime"u8) {
                    localLoc.name = localˢ;
                } else {
                    localLoc.name = tz;
                }
                return;
            }
        } else if (tz != ""u8 && tz != "UTC"u8) {
            var (z, err) = loadLocation(tz, platformZoneSources); if (err == nil) {
                localLoc = z.Value;
                return;
            }
        }
        break;
    }}

    // Fall back to UTC.
    localLoc.name = utcˢ;
}

} // end time_package
