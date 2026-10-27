// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using static go.@internal.sysinfo_package;

partial class sysinfo_internal_test_package {

public static readonly ж<Func<@string>> ᏑXosCPUInfoName = new StaticBox<Func<@string>>(osCPUInfoName);
public static ref Func<@string> XosCPUInfoName => ref ((StaticBox<Func<@string>>)ᏑXosCPUInfoName).ValueSlot;

} // end sysinfo_internal_test_package
