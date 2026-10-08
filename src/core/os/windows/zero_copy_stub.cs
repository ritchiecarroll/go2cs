// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !freebsd && !linux && !solaris
namespace go;

using Δio = io_package;

partial class os_package {

internal static (int64 written, bool handled, error err) writeTo(this ref File f, Δio.Writer w) {
    return (0, false, default!);
}

internal static (int64 n, bool handled, error err) readFrom(this ref File f, Δio.Reader r) {
    return (0, false, default!);
}

} // end os_package
