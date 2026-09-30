// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.vendor.golang.org.x.sys;

using strconv = strconv_package;

partial class cpu_package {

// parseRelease parses a dot-separated version number. It follows the semver
// syntax, but allows the minor and patch versions to be elided.
//
// This is a copy of the Go runtime's parseRelease from
// https://golang.org/cl/209597.
internal static (nint major, nint minor, nint patch, bool ok) parseRelease(@string rel) {
    nint major = default!;
    nint minor = default!;
    nint patch = default!;
    bool ok = default!;

    // Strip anything after a dash or plus.
    for (nint i = 0; i < len(rel); i++) {
        if (rel[i] == (rune)'-' || rel[i] == (rune)'+') {
            rel = rel.slice(0, i);
            break;
        }
    }
    (nint, bool) next() {
        for (nint i = 0; i < len(rel); i++) {
            if (rel[i] == (rune)'.') {
                var (verΔ1, errΔ1) = strconv.Atoi(rel.slice(0, i));
                rel = rel.slice(i + 1);
                return (verΔ1, errΔ1 == default!);
            }
        }
        var (ver, err) = strconv.Atoi(rel);
        rel = ""u8;
        return (ver, err == default!);
    }
    {
        (major, ok) = next(); if (!ok || rel == ""u8) {
            return (major, minor, patch, ok);
        }
    }
    {
        (minor, ok) = next(); if (!ok || rel == ""u8) {
            return (major, minor, patch, ok);
        }
    }
    (patch, ok) = next();
    return (major, minor, patch, ok);
}

} // end cpu_package
