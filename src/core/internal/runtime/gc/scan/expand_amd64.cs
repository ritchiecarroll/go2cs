// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.runtime.gc;

using gc = go.@internal.runtime.gc_package;
using go.@internal.runtime;

partial class scan_package {

// ExpandAVX512 expands each bit in packed into f consecutive bits in unpacked,
// where f is the word size of objects in sizeClass.
//
// This is a testing entrypoint to the expanders used by scanSpanPacked*.
//
//go:noescape
public static partial void ExpandAVX512(nint sizeClass, ж<gc.ObjMask> packed, ж<gc.PtrMask> unpacked);

// gcExpandersAVX512 is the PCs of expander functions. These cannot be called directly
// as they don't follow the Go ABI, but you can use this to check if a given
// expander PC is 0.
//
// It is defined in assembly.
internal static array<uintptr> gcExpandersAVX512 = new(68);

} // end scan_package
