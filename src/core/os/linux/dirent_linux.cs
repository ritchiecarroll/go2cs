// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using syscall = syscall_package;
using @unsafe = unsafe_package;
using fs = go.io.fs_package;

partial class os_package {

internal static (uint64, bool) direntIno(slice<byte> buf) {
    return readInt(buf, /* unsafe.Offsetof(syscall.Dirent{}.Ino) */ (uintptr)0, /* unsafe.Sizeof(syscall.Dirent{}.Ino) */ (uintptr)8);
}

internal static (uint64, bool) direntReclen(slice<byte> buf) {
    return readInt(buf, /* unsafe.Offsetof(syscall.Dirent{}.Reclen) */ (uintptr)16, /* unsafe.Sizeof(syscall.Dirent{}.Reclen) */ (uintptr)2);
}

internal static (uint64, bool) direntNamlen(slice<byte> buf) {
    var (reclen, ok) = direntReclen(buf);
    if (!ok) {
        return (0, false);
    }
    return (reclen - (uint64)/* unsafe.Offsetof(syscall.Dirent{}.Name) */ (uintptr)19, true);
}

internal static FileMode direntType(slice<byte> buf) {
    var off = /* unsafe.Offsetof(syscall.Dirent{}.Type) */ (uintptr)18;
    if (off >= (uintptr)len(buf)) {
        return ~((fs.FileMode)((fs.FileMode)0)); // unknown
    }
    var typ = buf[off];
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
    return ~((fs.FileMode)((fs.FileMode)0)); // unknown
}

} // end os_package
