// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build aix || darwin
namespace go;

partial class runtime_package {

internal static (int32 r, int32 w, int32 errno) nonblockingPipe() {
    int32 r = default;
    int32 w = default;
    int32 errno = default;

    (r, w, errno) = pipe();
    if (errno != 0) {
        return (-1, -1, errno);
    }
    closeonexec(r);
    setNonblock(r);
    closeonexec(w);
    setNonblock(w);
    return (r, w, errno);
}

} // end runtime_package
