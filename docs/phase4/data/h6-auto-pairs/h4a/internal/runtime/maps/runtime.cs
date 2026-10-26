// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.runtime;

using abi = go.@internal.abi_package;
using @unsafe = unsafe_package;
using go.@internal;

partial class maps_package {

// Functions below pushed from runtime.

//go:linkname fatal
internal static partial void fatal(@string s);

//go:linkname rand
internal static partial uint64 rand();

//go:linkname typedmemmove
internal static partial void typedmemmove(ж<abi.Type> typ, @unsafe.Pointer dst, @unsafe.Pointer src);

//go:linkname typedmemclr
internal static partial void typedmemclr(ж<abi.Type> typ, @unsafe.Pointer ptr);

//go:linkname newarray
internal static partial @unsafe.Pointer newarray(ж<abi.Type> typ, nint n);

//go:linkname newobject
internal static partial @unsafe.Pointer newobject(ж<abi.Type> typ);

} // end maps_package
