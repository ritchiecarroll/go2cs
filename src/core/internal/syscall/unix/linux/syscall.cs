// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.syscall;

partial class unix_package {

// Single-word zero for use when we need a valid pointer to 0 bytes.
internal static ж<uintptr> Ꮡ_zero = new StaticBox<uintptr>(default(uintptr));
internal static ref uintptr _zero => ref Ꮡ_zero.Value;

} // end unix_package
