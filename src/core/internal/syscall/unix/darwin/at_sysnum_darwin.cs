// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.syscall;

partial class unix_package {

public static UntypedInt AT_EACCESS => 0x10;
public static UntypedInt AT_FDCWD => /* -0x2 */ -2;
public static UntypedInt AT_REMOVEDIR => 0x80;
public static UntypedInt AT_SYMLINK_NOFOLLOW => 0x0020;

public static UntypedInt UTIME_OMIT => /* -0x2 */ -2;

} // end unix_package
