// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using static go.@internal.abi_package;

partial class abi_internal_test_package {

public static partial void FuncPCTestFn();

public static uintptr FuncPCTestFnAddr; // address of FuncPCTestFn, directly retrieved from assembly

//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] public static uintptr FuncPCTest() {
    return FuncPCABI0(FuncPCTestFn);
}

} // end abi_internal_test_package
