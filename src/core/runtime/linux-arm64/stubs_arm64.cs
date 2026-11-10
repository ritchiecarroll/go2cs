// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using @unsafe = unsafe_package;

partial class runtime_package {

// Called from assembly only; declared for go vet.
internal static partial void load_g();
internal static partial void save_g();

//go:noescape
internal static partial void asmcgocall_no_g(@unsafe.Pointer fn, @unsafe.Pointer arg);

internal static partial void emptyfunc();

// Used by reflectcall and the reflect package.
//
// Spills/loads arguments in registers to/from an internal/abi.RegArgs
// respectively. Does not follow the Go ABI.
internal static partial void spillArgs();
internal static partial void unspillArgs();

// getfp returns the frame pointer register of its caller or 0 if not implemented.
// TODO: Make this a compiler intrinsic
internal static partial uintptr getfp();

} // end runtime_package
