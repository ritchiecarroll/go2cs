// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build windows
namespace go.@internal.syscall.windows;

using static go.@internal.syscall.windows.registry_package;
using static registry_internal_test_package;

partial class registry_internal_test_package {

} partial class registry_internal_testᴛKey {

internal static error SetValue(this global::go.@internal.syscall.windows.registry_package.Key k, @string name, uint32 valtype, slice<byte> data) {
    return k.setValue(name, valtype, data);
}

} // end registry_internal_testᴛKey
