// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using @unsafe = unsafe_package;

partial class runtime_package {

// Called from compiled code; declared for vet; do NOT call from Go.
internal static partial void gcWriteBarrierCX();
internal static partial void gcWriteBarrierDX();
internal static partial void gcWriteBarrierBX();
internal static partial void gcWriteBarrierBP();
internal static partial void gcWriteBarrierSI();
internal static partial void gcWriteBarrierR8();
internal static partial void gcWriteBarrierR9();

// stackcheck checks that SP is in range [g->stack.lo, g->stack.hi).
internal static partial void stackcheck();

// Called from assembly only; declared for go vet.
internal static partial void settls();

// argument in DI

// Retpolines, used by -spectre=ret flag in cmd/asm, cmd/compile.
internal static partial void retpolineAX();
internal static partial void retpolineCX();
internal static partial void retpolineDX();
internal static partial void retpolineBX();
internal static partial void retpolineBP();
internal static partial void retpolineSI();
internal static partial void retpolineDI();
internal static partial void retpolineR8();
internal static partial void retpolineR9();
internal static partial void retpolineR10();
internal static partial void retpolineR11();
internal static partial void retpolineR12();
internal static partial void retpolineR13();
internal static partial void retpolineR14();
internal static partial void retpolineR15();

//go:noescape
internal static partial void asmcgocall_no_g(@unsafe.Pointer fn, @unsafe.Pointer arg);

//go:systemstack
internal static partial void asmcgocall_landingpad();

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
