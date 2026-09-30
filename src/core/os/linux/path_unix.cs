// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix || (js && wasm) || wasip1
namespace go;

partial class os_package {

public static UntypedInt PathSeparator => /* '/' */ 47; // OS-specific path separator
public static UntypedInt PathListSeparator => /* ':' */ 58; // OS-specific path list separator

// IsPathSeparator reports whether c is a directory separator character.
public static bool IsPathSeparator(uint8 c) {
    return PathSeparator == c;
}

// splitPath returns the base name and parent directory.
internal static (@string, @string) splitPath(@string path) {
    // if no better parent is found, the path is relative from "here"
    @string dirname = "."u8;
    // Remove all but one leading slash.
    while (len(path) > 1 && path[0] == (rune)'/' && path[1] == (rune)'/') {
        path = path[1..];
    }
    nint i = len(path) - 1;
    // Remove trailing slashes.
    for (; i > 0 && path[i] == (rune)'/'; i--) {
        path = path.slice(0, i);
    }
    // if no slashes in path, base is path
    @string basename = path;
    // Remove leading directory path
    for (i--; i >= 0; i--) {
        if (path[i] == (rune)'/') {
            if (i == 0){
                dirname = path[..1];
            } else {
                dirname = path.slice(0, i);
            }
            basename = path.slice(i + 1);
            break;
        }
    }
    return (dirname, basename);
}

} // end os_package
