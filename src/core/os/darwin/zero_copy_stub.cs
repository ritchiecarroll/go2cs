// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !freebsd && !linux && !solaris
namespace go;

using Δio = io_package;
using static os_package;

partial class os_package {

} partial class os_package {

[GoMethodBody("writeTo")] internal static (int64 written, bool handled, error err) writeToᴍ(ref File f, Δio.Writer w) {
    return (0, false, default);
}

[GoMethodBody("readFrom")] internal static (int64 n, bool handled, error err) readFromᴍ(ref File f, Δio.Reader r) {
    return (0, false, default);
}

}
