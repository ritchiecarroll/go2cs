// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using filepathlite = @internal.filepathlite_package;
using syscall = syscall_package;
using time = time_package;
using @internal;
using fs = go.io.fs_package;
using go.io;

partial class os_package {

internal static void fillFileStatFromSys(ж<fileStat> Ꮡfs, @string name) {
    ref var fs = ref Ꮡfs.DerefOrNull();

    fs.name = filepathlite.Base(name);
    fs.size = fs.sys.Size;
    var (ᴛ1, ᴛ2) = fs.sys.Mtimespec.Unix();
    fs.modTime = time.Unix(ᴛ1, ᴛ2);
    fs.mode = (FileMode)(uint32)((uint16)(fs.sys.Mode & 511));
    var exprᴛ1 = (uint16)(fs.sys.Mode & (uint16)syscall.S_IFMT);
    if (exprᴛ1 == syscall.S_IFBLK || exprᴛ1 == syscall.S_IFWHT) {
        fs.mode |= (fs.FileMode)(ModeDevice);
    }
    else if (exprᴛ1 == syscall.S_IFCHR) {
        fs.mode |= (fs.FileMode)((fs.FileMode)(ModeDevice | ModeCharDevice));
    }
    else if (exprᴛ1 == syscall.S_IFDIR) {
        fs.mode |= (fs.FileMode)(ModeDir);
    }
    else if (exprᴛ1 == syscall.S_IFIFO) {
        fs.mode |= (fs.FileMode)(ModeNamedPipe);
    }
    else if (exprᴛ1 == syscall.S_IFLNK) {
        fs.mode |= (fs.FileMode)(ModeSymlink);
    }
    else if (exprᴛ1 == syscall.S_IFREG) {
        // nothing to do
    }
    else if (exprᴛ1 == syscall.S_IFSOCK) {
        fs.mode |= (fs.FileMode)(ModeSocket);
    }
    if ((uint16)(fs.sys.Mode & (uint16)syscall.S_ISGID) != 0) {
        fs.mode |= (fs.FileMode)(ModeSetgid);
    }
    if ((uint16)(fs.sys.Mode & (uint16)syscall.S_ISUID) != 0) {
        fs.mode |= (fs.FileMode)(ModeSetuid);
    }
    if ((uint16)(fs.sys.Mode & (uint16)syscall.S_ISVTX) != 0) {
        fs.mode |= (fs.FileMode)(ModeSticky);
    }
}

// For testing.
internal static time.Time atime(FileInfo fi) {
    var (ᴛ3, ᴛ4) = fi.Sys()._<ж<syscall.Stat_t>>().of(syscall.Stat_t.ᏑAtimespec).Unix();
    return time.Unix(ᴛ3, ᴛ4);
}

} // end os_package
