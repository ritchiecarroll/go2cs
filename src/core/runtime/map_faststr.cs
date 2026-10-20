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

//go:linkname mapaccess1_faststr
internal static partial @unsafe.Pointer mapaccess1_faststr(ж<abiꓸMapType> t, ж<mapsꓸMap> m, @string ky);

// mapaccess2_faststr should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/ugorji/go/codec
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname mapaccess2_faststr
internal static partial (@unsafe.Pointer, bool) mapaccess2_faststr(ж<abiꓸMapType> t, ж<mapsꓸMap> m, @string ky);

// mapassign_faststr should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/bytedance/sonic
//   - github.com/ugorji/go/codec
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname mapassign_faststr
internal static partial @unsafe.Pointer mapassign_faststr(ж<abiꓸMapType> t, ж<mapsꓸMap> m, @string s);

//go:linkname mapdelete_faststr
internal static partial void mapdelete_faststr(ж<abiꓸMapType> t, ж<mapsꓸMap> m, @string ky);

} // end runtime_package
