// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

/* blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards) */ // for go:linkname

partial class runtime_package {

//go:linkname boring_runtime_arg0 crypto/internal/boring.runtime_arg0
internal static @string boring_runtime_arg0() {
    // On Windows, argslice is not set, and it's too much work to find argv0.
    if (len(argslice) == 0) {
        return ""u8;
    }
    return argslice[0];
}

} // end runtime_package
