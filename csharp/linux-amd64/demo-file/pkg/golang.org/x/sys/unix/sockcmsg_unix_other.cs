// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.golang.org.x.sys;

using runtime = runtime_package;

partial class unix_package {

internal static nint cmsgAlignOf(nint salen) {
    nint salign = SizeofPtr;

    var exprᴛ1 = runtime.GOOS;
    switch (ᐧ) {
    case {} when exprᴛ1 == "aix"u8: {
        salign = 1;
        break;
    }
    case {} when exprᴛ1 == "darwin"u8 || exprᴛ1 == "ios"u8 || exprᴛ1 == "illumos"u8 || exprᴛ1 == "solaris"u8: {
        if (SizeofPtr == 8) {
            salign = 4;
        }
        break;
    }
    case {} when exprᴛ1 == "netbsd"u8 || exprᴛ1 == "openbsd"u8: {
        if (runtime.GOARCH == "arm"u8) {
            salign = 8;
        }
        if (runtime.GOOS == "netbsd"u8 && runtime.GOARCH == "arm64"u8) {
            salign = 16;
        }
        break;
    }
    case {} when exprᴛ1 == "zos"u8: {
        salign = SizeofInt;
        break;
    }}

    return (salen + salign - 1) & ~(salign - 1);
}

} // end unix_package
