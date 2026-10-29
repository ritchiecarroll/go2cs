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
    var (ᴛ1, ᴛ2) = fs.sys.Mtim.Unix();
    fs.modTime = time.Unix(ᴛ1, ᴛ2);
    fs.mode = (FileMode)(fs.sys.Mode & 511);
    switch ((fs.sys.Mode & (uint32)syscall.S_IFMT)) {
    case var ᴛ when ᴛ == syscall.S_IFBLK: {
        fs.mode |= (fs.FileMode)(ModeDevice);
        break;
    }
    case var ᴛ when ᴛ == syscall.S_IFCHR: {
        fs.mode |= (fs.FileMode)((fs.FileMode)(ModeDevice | ModeCharDevice));
        break;
    }
    case var ᴛ when ᴛ == syscall.S_IFDIR: {
        fs.mode |= (fs.FileMode)(ModeDir);
        break;
    }
    case var ᴛ when ᴛ == syscall.S_IFIFO: {
        fs.mode |= (fs.FileMode)(ModeNamedPipe);
        break;
    }
    case var ᴛ when ᴛ == syscall.S_IFLNK: {
        fs.mode |= (fs.FileMode)(ModeSymlink);
        break;
    }
    case var ᴛ when ᴛ == syscall.S_IFREG: {
        // nothing to do
        break;
    }
    case var ᴛ when ᴛ == syscall.S_IFSOCK: {
        fs.mode |= (fs.FileMode)(ModeSocket);
        break;
    }}
    if ((fs.sys.Mode & (uint32)syscall.S_ISGID) != 0) {
        fs.mode |= (fs.FileMode)(ModeSetgid);
    }
    if ((fs.sys.Mode & (uint32)syscall.S_ISUID) != 0) {
        fs.mode |= (fs.FileMode)(ModeSetuid);
    }
    if ((fs.sys.Mode & (uint32)syscall.S_ISVTX) != 0) {
        fs.mode |= (fs.FileMode)(ModeSticky);
    }
}

// For testing.
internal static time.Time atime(FileInfo fi) {
    var (ᴛ3, ᴛ4) = fi.Sys()._<ж<syscall.Stat_t>>().of(syscall.Stat_t.ᏑAtim).Unix();
    return time.Unix(ᴛ3, ᴛ4);
}

} // end os_package
