// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !windows
namespace go;

partial class os_package {

internal static (@string, error) rootCleanPath(@string s, slice<@string> prefix, slice<@string> suffix) {
    return (s, default);
}

} // end os_package
