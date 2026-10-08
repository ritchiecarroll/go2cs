// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using fmt = fmt_package;

partial class types_package {

// A Type represents a type of Go.
// All types implement the Type interface.
partial interface ΔType :
    fmt.Stringer
{
    // Underlying returns the underlying type of a type.
    // Underlying types are never Named, TypeParam, or Alias types.
    //
    // See https://go.dev/ref/spec#Underlying_types.
    ΔType Underlying();
}

} // end types_package
