// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.vendor.golang.org.x.sys;

partial class cpu_package {

// getAuxvFn is non-nil on Go 1.21+ (via runtime_auxv_go121.go init)
// on platforms that use auxv.
internal static Func<slice<uintptr>> getAuxvFn;

internal static slice<uintptr> getAuxv() {
    if (getAuxvFn == nil) {
        return default;
    }
    return getAuxvFn();
}

} // end cpu_package
