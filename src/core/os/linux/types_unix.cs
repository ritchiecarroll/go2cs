// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !windows && !plan9
namespace go;

using syscall = syscall_package;
using time = time_package;
using static os_package;

partial class os_package {

// A fileStat is the implementation of FileInfo returned by Stat and Lstat.
partial struct fileStat {
    internal @string name;
    internal int64 size;
    internal FileMode mode;
    internal time.Time modTime;
    internal syscall.Stat_t sys;
}

} partial class os_package {

internal static int64 Sizeᴍ(ref fileStat fs) { return fs.size; }
internal static FileMode Modeᴍ(ref fileStat fs) { return fs.mode; }
internal static time.Time ModTimeᴍ(ref fileStat fs) { return fs.modTime; }
internal static any Sysᴍ(ж<fileStat> Ꮡfs) { return Ꮡfs.Ꮡsys; }

} partial class os_package {

internal static bool sameFile(ref fileStat fs1, ref fileStat fs2) {
    return fs1.sys.Dev == fs2.sys.Dev && fs1.sys.Ino == fs2.sys.Ino;
}

} // end os_package

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies above)
partial class osᴛfileStat {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static int64 Size(this ref fileStat fs) => os_package.Sizeᴍ(ref fs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static FileMode Mode(this ref fileStat fs) => os_package.Modeᴍ(ref fs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static time.Time ModTime(this ref fileStat fs) => os_package.ModTimeᴍ(ref fs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static any Sys(this ж<fileStat> Ꮡfs) => os_package.Sysᴍ(Ꮡfs);
}
// </GlyphForwarders>
