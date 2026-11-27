// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

partial class abi_package {

// See abi_generic.go.

// R0 - R15.
public static UntypedInt IntArgRegs => 16;

// F0 - F15.
public static UntypedInt FloatArgRegs => 16;

public static UntypedInt EffectiveFloatRegSize => 8;

} // end abi_package
