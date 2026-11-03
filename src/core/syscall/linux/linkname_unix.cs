// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix
namespace go;

/* blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards) */ // for linkname

partial class syscall_package {

// mmap should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - modernc.org/memory
//   - github.com/ncruces/go-sqlite3
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname mmap

} // end syscall_package
