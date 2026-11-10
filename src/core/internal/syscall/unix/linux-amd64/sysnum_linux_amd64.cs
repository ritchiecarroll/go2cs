// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.syscall;

partial class unix_package {

internal static uintptr getrandomTrap => 318;
internal static uintptr copyFileRangeTrap => 326;
internal static uintptr pidfdSendSignalTrap => 424;
internal static uintptr pidfdOpenTrap => 434;
internal static uintptr openat2Trap => 437;

} // end unix_package
