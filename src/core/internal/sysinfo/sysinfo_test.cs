// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using static go.@internal.sysinfo_package;
using testing = testing_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using static go.@internal.sysinfo_internal_test_package;

partial class sysinfo_test_package {

public static partial void TestCPUName(ж<testing.T> Ꮡt) {
    Ꮡt.Logf("CPUName: %s"u8, CPUName());
    Ꮡt.Logf("osCPUInfoName: %s"u8, sysinfo_internal_test_package.XosCPUInfoName());
}

} // end sysinfo_test_package
