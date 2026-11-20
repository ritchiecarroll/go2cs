// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.golang.org.x.sys;

partial class unix_package {

public static (nint n, error err) ReadDirent(nint fd, slice<byte> buf) {
    return Getdents(fd, buf);
}

} // end unix_package
