// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !msan
// Dummy MSan support API, used when not built with -msan.
namespace go;

using @unsafe = unsafe_package;

partial class runtime_package {

internal const bool msanenabled = false;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string msanˢ => ref msanˢᵇ;
private static readonly @string msanˢᵇ = "msan"u8;

// Because msanenabled is false, none of these functions should be called.
internal static void msanread(@unsafe.Pointer addr, uintptr sz) { @throw(msanˢ); }
internal static void msanwrite(@unsafe.Pointer addr, uintptr sz) { @throw(msanˢ); }
internal static void msanmalloc(@unsafe.Pointer addr, uintptr sz) { @throw(msanˢ); }
internal static void msanfree(@unsafe.Pointer addr, uintptr sz) { @throw(msanˢ); }
internal static void msanmove(@unsafe.Pointer dst, @unsafe.Pointer src, uintptr sz) { @throw(msanˢ); }

} // end runtime_package
