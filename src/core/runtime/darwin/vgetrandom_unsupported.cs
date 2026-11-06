// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !(linux && (amd64 || arm64 || arm64be || ppc64 || ppc64le || loong64 || s390x))
namespace go;

// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)

partial class runtime_package {

//go:linkname vgetrandom
public static (nint ret, bool supported) vgetrandom(slice<byte> Δp, uint32 flags) {
    return (-1, false);
}

internal static void vgetrandomDestroy(ref m mp) { }

internal static void vgetrandomInit() { }

} // end runtime_package
