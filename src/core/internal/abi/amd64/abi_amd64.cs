// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

partial class abi_package {

// See abi_generic.go.

// RAX, RBX, RCX, RDI, RSI, R8, R9, R10, R11.
public static UntypedInt IntArgRegs => 9;

// X0 -> X14.
public static UntypedInt FloatArgRegs => 15;

// We use SSE2 registers which support 64-bit float operations.
public static UntypedInt EffectiveFloatRegSize => 8;

} // end abi_package
