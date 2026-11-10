// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.syscall;

using syscall = syscall_package;

partial class unix_package {

internal static uintptr unlinkatTrap => /* syscall.SYS_UNLINKAT */ 35;
internal static uintptr openatTrap => /* syscall.SYS_OPENAT */ 56;
internal static uintptr readlinkatTrap => /* syscall.SYS_READLINKAT */ 78;
internal static uintptr mkdiratTrap => /* syscall.SYS_MKDIRAT */ 34;
internal static uintptr fchownatTrap => /* syscall.SYS_FCHOWNAT */ 54;
internal static uintptr linkatTrap => /* syscall.SYS_LINKAT */ 37;
internal static uintptr symlinkatTrap => /* syscall.SYS_SYMLINKAT */ 36;

public static UntypedInt AT_EACCESS => 0x200;
public static UntypedInt AT_FDCWD => /* -0x64 */ -100;
public static UntypedInt AT_REMOVEDIR => 0x200;
public static UntypedInt AT_SYMLINK_NOFOLLOW => 0x100;

public static UntypedInt UTIME_OMIT => 0x3ffffffe;

public static UntypedInt O_PATH => 0x200000;

} // end unix_package
