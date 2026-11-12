// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)

partial class types_package {

// This should properly be in infer.go, but that file is auto-generated.

// infer should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/goplus/gox
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname badlinkname_Checker_infer go/types.(*Checker).infer
internal static partial slice<Type> badlinkname_Checker_infer(ж<Checker> _Δp0, positioner _Δp1, slice<ж<TypeParam>> _Δp2, slice<Type> _Δp3, ж<Tuple> _Δp4, slice<ж<operand>> _Δp5, bool _Δp6, ж<error_> _Δp7);

} // end types_package
