// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !windows && !plan9
namespace go;

using syscall = syscall_package;
using time = time_package;

partial class os_package {

// A fileStat is the implementation of FileInfo returned by Stat and Lstat.
partial struct fileStat {
    internal @string name;
    internal int64 size;
    internal FileMode mode;
    internal time.Time modTime;
    internal syscall.Stat_t sys;
}

internal static int64 Size(this ref fileStat fs) {
    return fs.size;
}

internal static FileMode Mode(this ref fileStat fs) {
    return fs.mode;
}

internal static time.Time ModTime(this ref fileStat fs) {
    return fs.modTime;
}

internal static any Sys(this ж<fileStat> Ꮡfs) {
    return Ꮡfs.of(fileStat.Ꮡsys);
}

internal static bool sameFile(ref fileStat fs1, ref fileStat fs2) {
    return fs1.sys.Dev == fs2.sys.Dev && fs1.sys.Ino == fs2.sys.Ino;
}

} // end os_package
