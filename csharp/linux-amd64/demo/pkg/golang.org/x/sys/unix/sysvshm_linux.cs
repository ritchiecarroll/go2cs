// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.golang.org.x.sys;

using runtime = runtime_package;

partial class unix_package {

public static (nint result, error err) SysvShmCtl(nint id, nint cmd, ж<SysvShmDesc> desc) {
    if (runtime.GOARCH == "arm"u8 || runtime.GOARCH == "mips64"u8 || runtime.GOARCH == "mips64le"u8) {
        cmd |= (nint)(ipc_64);
    }

    return shmctl(id, cmd, desc);
}

} // end unix_package
