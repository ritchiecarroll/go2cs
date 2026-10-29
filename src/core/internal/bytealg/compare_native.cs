// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build 386 || amd64 || s390x || arm || arm64 || loong64 || ppc64 || ppc64le || mips || mipsle || wasm || mips64 || mips64le || riscv64
namespace go.@internal;

/* blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards) */ // For go:linkname

partial class bytealg_package {

//go:noescape
public static partial nint Compare(slice<byte> a, slice<byte> b);

public static nint CompareString(@string a, @string b) {
    return abigen_runtime_cmpstring(a, b);
}

// The declaration below generates ABI wrappers for functions
// implemented in assembly in this package but declared in another
// package.

//go:linkname abigen_runtime_cmpstring runtime.cmpstring
internal static partial nint abigen_runtime_cmpstring(@string a, @string b);

} // end bytealg_package
