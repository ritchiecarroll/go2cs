// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.golang.org.x.sys;

partial class cpu_package {

internal static Func<slice<uintptr>> getAuxvFn;

internal static slice<uintptr> getAuxv() {
    if (getAuxvFn == nil) {
        return default;
    }
    return getAuxvFn();
}

} // end cpu_package
