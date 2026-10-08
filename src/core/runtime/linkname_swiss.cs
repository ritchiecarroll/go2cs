// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build goexperiment.swissmap
namespace go;

using abi = @internal.abi_package;
using maps = @internal.runtime.maps_package;
using sys = @internal.runtime.sys_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;

partial class runtime_package {

// Legacy //go:linkname compatibility shims
//
// The functions below are unused by the toolchain, and exist only for
// compatibility with existing //go:linkname use in the ecosystem (and in
// map_noswiss.go for normal use via GOEXPERIMENT=noswissmap).

// linknameIter is the it argument to mapiterinit and mapiternext.
//
// Callers of mapiterinit allocate their own iter structure, which has the
// layout of the pre-Go 1.24 hiter structure, shown here for posterity:
//
//	type hiter struct {
//		key         unsafe.Pointer
//		elem        unsafe.Pointer
//		t           *maptype
//		h           *hmap
//		buckets     unsafe.Pointer
//		bptr        *bmap
//		overflow    *[]*bmap
//		oldoverflow *[]*bmap
//		startBucket uintptr
//		offset      uint8
//		wrapped     bool
//		B           uint8
//		i           uint8
//		bucket      uintptr
//		checkBucket uintptr
//	}
//
// Our structure must maintain compatibility with the old structure. This
// means:
//
//   - Our structure must be the same size or smaller than hiter. Otherwise we
//     may write outside the caller's hiter allocation.
//   - Our structure must have the same pointer layout as hiter, so that the GC
//     tracks pointers properly.
//
// Based on analysis of the "hall of shame" users of these linknames:
//
//   - The key and elem fields must be kept up to date with the current key/elem.
//     Some users directly access the key and elem fields rather than calling
//     reflect.mapiterkey/reflect.mapiterelem.
//   - The t field must be non-nil after mapiterinit. gonum.org/v1/gonum uses
//     this to verify the iterator is initialized.
//   - github.com/segmentio/encoding and github.com/RomiChan/protobuf check if h
//     is non-nil, but the code has no effect. Thus the value of h does not
//     matter. See internal/runtime_reflect/map.go.
partial struct linknameIter {
    // Fields from hiter.
    internal @unsafe.Pointer key;
    internal @unsafe.Pointer elem;
    internal ж<abi.SwissMapType> typ;
    // The real iterator.
    internal ж<maps.Iter> it;
}

// mapiterinit is a compatibility wrapper for map iterator for users of
// //go:linkname from before Go 1.24. It is not used by Go itself. New users
// should use reflect or the maps package.
//
// mapiterinit should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/bytedance/sonic
//   - github.com/goccy/go-json
//   - github.com/RomiChan/protobuf
//   - github.com/segmentio/encoding
//   - github.com/ugorji/go/codec
//   - github.com/wI2L/jettison
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname mapiterinit
internal static void mapiterinit(ж<abi.SwissMapType> Ꮡt, ж<mapsꓸMap> Ꮡm, ж<linknameIter> Ꮡit) {
    ref var it = ref Ꮡit.DerefOrNull();

    if (raceenabled && Ꮡm != nil) {
        var callerpc = sys.GetCallerPC();
        racereadpc(@unsafe.Pointer.FromPinnedBox(Ꮡm), callerpc, abi.FuncPCABIInternal(mapiterinit));
    }
    it.typ = Ꮡt;
    it.it = @new<maps.Iter>();
    it.it.Init(Ꮡt, Ꮡm);
    it.it.Next();
    it.key = (uintptr)it.it.Key();
    it.elem = (uintptr)it.it.Elem();
}

// reflect_mapiterinit is a compatibility wrapper for map iterator for users of
// //go:linkname from before Go 1.24. It is not used by Go itself. New users
// should use reflect or the maps package.
//
// reflect_mapiterinit should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/modern-go/reflect2
//   - gitee.com/quant1x/gox
//   - github.com/v2pro/plz
//   - github.com/wI2L/jettison
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname reflect_mapiterinit reflect.mapiterinit
internal static void reflect_mapiterinit(ж<abi.SwissMapType> Ꮡt, ж<mapsꓸMap> Ꮡm, ж<linknameIter> Ꮡit) {
    mapiterinit(Ꮡt, Ꮡm, Ꮡit);
}

// mapiternext is a compatibility wrapper for map iterator for users of
// //go:linkname from before Go 1.24. It is not used by Go itself. New users
// should use reflect or the maps package.
//
// mapiternext should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/bytedance/sonic
//   - github.com/RomiChan/protobuf
//   - github.com/segmentio/encoding
//   - github.com/ugorji/go/codec
//   - gonum.org/v1/gonum
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname mapiternext
internal static void mapiternext(ж<linknameIter> Ꮡit) {
    ref var it = ref Ꮡit.DerefOrNull();

    if (raceenabled) {
        var callerpc = sys.GetCallerPC();
        racereadpc(@unsafe.Pointer.FromPinnedBox(it.it.Map()), callerpc, abi.FuncPCABIInternal(mapiternext));
    }
    it.it.Next();
    it.key = (uintptr)it.it.Key();
    it.elem = (uintptr)it.it.Elem();
}

// reflect_mapiternext is a compatibility wrapper for map iterator for users of
// //go:linkname from before Go 1.24. It is not used by Go itself. New users
// should use reflect or the maps package.
//
// reflect_mapiternext is for package reflect,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - gitee.com/quant1x/gox
//   - github.com/modern-go/reflect2
//   - github.com/goccy/go-json
//   - github.com/v2pro/plz
//   - github.com/wI2L/jettison
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname reflect_mapiternext reflect.mapiternext
internal static void reflect_mapiternext(ж<linknameIter> Ꮡit) {
    mapiternext(Ꮡit);
}

// reflect_mapiterkey is a compatibility wrapper for map iterator for users of
// //go:linkname from before Go 1.24. It is not used by Go itself. New users
// should use reflect or the maps package.
//
// reflect_mapiterkey should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/goccy/go-json
//   - gonum.org/v1/gonum
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname reflect_mapiterkey reflect.mapiterkey
internal static @unsafe.Pointer reflect_mapiterkey(ref linknameIter it) {
    return (uintptr)it.it.Key();
}

// reflect_mapiterelem is a compatibility wrapper for map iterator for users of
// //go:linkname from before Go 1.24. It is not used by Go itself. New users
// should use reflect or the maps package.
//
// reflect_mapiterelem should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/goccy/go-json
//   - gonum.org/v1/gonum
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname reflect_mapiterelem reflect.mapiterelem
internal static @unsafe.Pointer reflect_mapiterelem(ref linknameIter it) {
    return (uintptr)it.it.Elem();
}

} // end runtime_package
