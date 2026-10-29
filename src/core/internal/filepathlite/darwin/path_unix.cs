// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix || (js && wasm) || wasip1
namespace go.@internal;

using bytealg = go.@internal.bytealg_package;
using stringslite = go.@internal.stringslite_package;
using go.@internal;

partial class filepathlite_package {

public static UntypedInt Separator => /* '/' */ 47;     // OS-specific path separator
public static UntypedInt ListSeparator => /* ':' */ 58; // OS-specific path list separator

public static bool IsPathSeparator(uint8 c) {
    return Separator == c;
}

internal static bool isLocal(@string path) {
    return unixIsLocal(path);
}

internal static (@string, error) localize(@string path) {
    if (bytealg.IndexByteString(path, 0) >= 0) {
        return ("", errInvalidPath);
    }
    return (path, default);
}

// IsAbs reports whether the path is absolute.
public static bool IsAbs(@string path) {
    return stringslite.HasPrefix(path, "/"u8);
}

// volumeNameLen returns length of the leading volume name on Windows.
// It returns 0 elsewhere.
internal static nint volumeNameLen(@string path) {
    return 0;
}

} // end filepathlite_package
