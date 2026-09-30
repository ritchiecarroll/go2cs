// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix || (js && wasm) || wasip1
namespace go.path;

using strings = strings_package;

partial class filepath_package {

// HasPrefix exists for historical compatibility and should not be used.
//
// Deprecated: HasPrefix does not respect path boundaries and
// does not ignore case when required.
public static bool HasPrefix(@string p, @string prefix) {
    return strings.HasPrefix(p, prefix);
}

internal static slice<@string> splitList(@string path) {
    if (path == ""u8) {
        return new @string[]{}.slice();
    }
    return strings.Split(path, ((@string)(rune)ListSeparator));
}

internal static (@string, error) abs(@string path) {
    return unixAbs(path);
}

internal static @string join(slice<@string> elem) {
    // If there's a bug here, fix the logic in ./path_plan9.go too.
    foreach (var (i, e) in elem) {
        if (e != ""u8) {
            return Clean(strings.Join(elem.slice(i), ((@string)(rune)Separator)));
        }
    }
    return ""u8;
}

internal static bool sameWord(@string a, @string b) {
    return a == b;
}

} // end filepath_package
