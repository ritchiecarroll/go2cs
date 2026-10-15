// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.runtime;

using goarch = go.@internal.goarch_package;
using go.@internal;

partial class gc_package {

// ObjMask is a bitmap where each bit corresponds to an object in a span.
//
// It is sized to accommodate all size classes.
/* [MaxObjsPerSpan / (goarch.PtrSize * 8)]uintptr */
partial struct ObjMask /*[16]uintptr*/;

// PtrMask is a bitmap where each bit represents a pointer-word in a single runtime page.
/* [PageSize / goarch.PtrSize / (goarch.PtrSize * 8)]uintptr */
partial struct PtrMask /*[16]uintptr*/;

} // end gc_package
