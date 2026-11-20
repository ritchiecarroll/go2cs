// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.golang.org.x.sys;

// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)

partial class cpu_package {

internal static partial slice<uintptr> runtime_getAuxv();

[GoInit] internal static void initΔ1() {
    getAuxvFn = runtime_getAuxv;
}

} // end cpu_package
