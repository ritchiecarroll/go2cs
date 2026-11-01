// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !(cgo && (aix || dragonfly || freebsd || (linux && !android) || netbsd || openbsd)) && !darwin
namespace go.@internal;

using os = os_package;

partial class testpty_package {

internal static (ж<os.File> pty, @string processTTY, error err) open() {
    return (default, "", ErrNotSupported);
}

} // end testpty_package
