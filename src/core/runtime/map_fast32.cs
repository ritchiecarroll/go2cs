// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using abi = @internal.abi_package;
using maps = @internal.runtime.maps_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;

partial class runtime_package {

// Functions below pushed from internal/runtime/maps.

//go:linkname mapaccess1_fast32
internal static partial @unsafe.Pointer mapaccess1_fast32(ж<abiꓸMapType> t, ж<mapsꓸMap> m, uint32 key);

// mapaccess2_fast32 should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/ugorji/go/codec
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname mapaccess2_fast32
internal static partial (@unsafe.Pointer, bool) mapaccess2_fast32(ж<abiꓸMapType> t, ж<mapsꓸMap> m, uint32 key);

// mapassign_fast32 should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/bytedance/sonic
//   - github.com/ugorji/go/codec
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname mapassign_fast32
internal static partial @unsafe.Pointer mapassign_fast32(ж<abiꓸMapType> t, ж<mapsꓸMap> m, uint32 key);

// mapassign_fast32ptr should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/ugorji/go/codec
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname mapassign_fast32ptr
internal static partial @unsafe.Pointer mapassign_fast32ptr(ж<abiꓸMapType> t, ж<mapsꓸMap> m, @unsafe.Pointer key);

//go:linkname mapdelete_fast32
internal static partial void mapdelete_fast32(ж<abiꓸMapType> t, ж<mapsꓸMap> m, uint32 key);

} // end runtime_package
