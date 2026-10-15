// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build amd64 || arm || arm64 || loong64 || mips64le || mips64 || ppc64le || ppc64 || riscv64 || s390x
namespace go.@internal;

partial class bytealg_package {

//go:noescape
public static partial nint Count(slice<byte> b, byte c);

//go:noescape
public static partial nint CountString(@string s, byte c);

// A backup implementation to use by assembly.
internal static nint countGeneric(slice<byte> b, byte c) {
    nint n = 0;
    foreach (var (_, x) in b) {
        if (x == c) {
            n++;
        }
    }
    return n;
}

internal static nint countGenericString(@string s, byte c) {
    nint n = 0;
    for (nint i = 0; i < len(s); i++) {
        if (s[i] == c) {
            n++;
        }
    }
    return n;
}

} // end bytealg_package
