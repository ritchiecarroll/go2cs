// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Package windows provides the syscall primitives required for the runtime.
namespace go.@internal.runtime.syscall;

using abi = go.@internal.abi_package;
using go.@internal;

partial class windows_package {

// MaxArgs should be divisible by 2, as Windows stack
// must be kept 16-byte aligned on syscall entry.
//
// Although it only permits maximum 42 parameters, it
// is arguably large enough.
public static UntypedInt MaxArgs => 42;

// StdCallInfo is a structure used to pass parameters to the system call.
partial struct StdCallInfo {
    public uintptr Fn;
    public uintptr N;    // number of parameters
    public uintptr Args; // parameters
    public uintptr R1;   // return values
    public uintptr R2;
    public uintptr Err; // error number
}

// StdCall calls a function using Windows' stdcall convention.
//
//go:noescape
public static partial void StdCall(ж<StdCallInfo> fn);

// asmstdcall is the function pointer for [AsmStdCallAddr].
internal static partial void asmstdcall(ж<StdCallInfo> fn);

// AsmStdCallAddr is the address of a function that accepts a pointer
// to [StdCallInfo] stored on the stack following the C calling convention,
// and calls the function using Windows' stdcall calling convention.
// Shouldn't be called directly from Go.
public static uintptr AsmStdCallAddr() {
    return abi.FuncPCABI0(asmstdcall);
}

} // end windows_package
