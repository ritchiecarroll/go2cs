// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using fmt = fmt_package;
using version = global::go.go.version_package;
using goversion = @internal.goversion_package;
using @internal;
using global::go.go;
using ꓸꓸꓸany = Span<any>;

partial class types_package {

partial struct goVersion /*@string*/;

// asGoVersion returns v as a goVersion (e.g., "go1.20.1" becomes "go1.20").
// If v is not a valid Go version, the result is the empty string.
internal static goVersion asGoVersion(@string v) {
    return ((goVersion)version.Lang(v));
}

// isValid reports whether v is a valid Go version.
internal static bool isValid(this goVersion v) {
    return v != ""u8;
}

// cmp returns -1, 0, or +1 depending on whether x < y, x == y, or x > y,
// interpreted as Go versions.
internal static nint cmp(this goVersion x, goVersion y) {
    return version.Compare(((@string)x), ((@string)y));
}

internal static goVersion go1_9 = asGoVersion("go1.9"u8);
internal static goVersion go1_13 = asGoVersion("go1.13"u8);
internal static goVersion go1_14 = asGoVersion("go1.14"u8);
internal static goVersion go1_17 = asGoVersion("go1.17"u8);
internal static goVersion go1_18 = asGoVersion("go1.18"u8);
internal static goVersion go1_20 = asGoVersion("go1.20"u8);
internal static goVersion go1_21 = asGoVersion("go1.21"u8);
internal static goVersion go1_22 = asGoVersion("go1.22"u8);
internal static goVersion go1_23 = asGoVersion("go1.23"u8);
internal static goVersion go_current = asGoVersion(fmt.Sprintf("go1.%d"u8, (nint)(goversion.Version)));

// allowVersion reports whether the current effective Go version
// (which may vary from one file to another) is allowed to use the
// feature version (want).
internal static bool allowVersion(this ref Checker check, goVersion want) {
    return !check.version.isValid() || check.version.cmp(want) >= 0;
}

// verifyVersionf is like allowVersion but also accepts a format string and arguments
// which are used to report a version error if allowVersion returns false.
internal static bool verifyVersionf(this ж<Checker> Ꮡcheck, positioner at, goVersion v, @string format, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    ref var check = ref Ꮡcheck.DerefOrNull();
    if (!check.allowVersion(v)) {
        Ꮡcheck.versionErrorf(at, v, format, args.ꓸꓸꓸ);
        return false;
    }
    return true;
}

} // end types_package
