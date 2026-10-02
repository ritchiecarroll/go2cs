// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using Δio = io_package;
using runtime = runtime_package;
using syscall = syscall_package;
using @unsafe = unsafe_package;
using fs = go.io.fs_package;

partial class os_package {

// Auxiliary information if the File describes a directory
[GoType] partial struct dirInfo {
    internal uintptr dir; // Pointer to DIR structure from dirent.h
}

[GoRecv] internal static void close(this ref dirInfo d) {
    if (d.dir == 0) {
        return;
    }
    closedir(d.dir);
    d.dir = 0;
}

// go2cs generated this placeholder — func readdir is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

internal static FileMode dtToType(uint8 typ) {
    var exprᴛ1 = typ;
    if (exprᴛ1 == syscall.DT_BLK) {
        return ModeDevice;
    }
    if (exprᴛ1 == syscall.DT_CHR) {
        return (fs.FileMode)(ModeDevice | ModeCharDevice);
    }
    if (exprᴛ1 == syscall.DT_DIR) {
        return ModeDir;
    }
    if (exprᴛ1 == syscall.DT_FIFO) {
        return ModeNamedPipe;
    }
    if (exprᴛ1 == syscall.DT_LNK) {
        return ModeSymlink;
    }
    if (exprᴛ1 == syscall.DT_REG) {
        return 0;
    }
    if (exprᴛ1 == syscall.DT_SOCK) {
        return ModeSocket;
    }

    return ~((fs.FileMode)((fs.FileMode)0));
}

// Implemented in syscall/syscall_darwin.go.

//go:linkname closedir syscall.closedir
[global::System.Diagnostics.StackTraceHidden] internal static error /*err*/ closedir(uintptr dir) {
    return syscall.closedir((uintptr)dir);
}

//go:linkname readdir_r syscall.readdir_r
internal static partial syscall.Errno /*res*/ readdir_r(uintptr dir, ж<syscall.Dirent> entry, ж<ж<syscall.Dirent>> result);

} // end os_package
