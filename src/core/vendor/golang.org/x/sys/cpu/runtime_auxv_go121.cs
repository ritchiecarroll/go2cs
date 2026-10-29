// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build go1.21
namespace go.vendor.golang.org.x.sys;

/* blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards) */ // for linkname

partial class cpu_package {

//go:linkname runtime_getAuxv runtime.getAuxv
internal static partial slice<uintptr> runtime_getAuxv();

[GoInit] internal static void initΔ1() {
    getAuxvFn = runtime_getAuxv;
}

} // end cpu_package
