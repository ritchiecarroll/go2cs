// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.golang.org.x.sys;

using strconv = strconv_package;

partial class cpu_package {

internal static (nint major, nint minor, nint patch, bool ok) parseRelease(@string rel) {
    nint major = default;
    nint minor = default;
    nint patch = default;
    bool ok = default;

    (nint n, bool more, bool ok) next() {
        nint n = default;
        nint i = 0;
        while (i < len(rel) && rel[i] >= '0' && rel[i] <= '9') {
            i++;
        }
        if (i == 0) {
            return (0, false, false);
        }
        (n, var err) = strconv.Atoi(rel.slice(0, i));
        if (err != nil) {
            return (0, false, false);
        }
        if (i < len(rel) && rel[i] == '.') {
            rel = rel.slice(i + 1);
            return (n, true, true);
        }
        rel = "";
        return (n, false, true);
    }

    bool more = default;
    (major, more, ok) = next(); if (!ok || !more) {
        return (0, 0, 0, false);
    }
    (minor, more, ok) = next(); if (!ok) {
        return (0, 0, 0, false);
    }
    if (!more) {
        return (major, minor, 0, true);
    }
    (patch, _, _) = next();
    return (major, minor, patch, true);
}

} // end cpu_package
