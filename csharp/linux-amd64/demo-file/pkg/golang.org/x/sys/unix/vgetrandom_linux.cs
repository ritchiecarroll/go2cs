// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.golang.org.x.sys;

// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)

partial class unix_package {

/*linkname*/ internal static partial (nint ret, bool supported) vgetrandom(slice<byte> p, uint32 flags) {
    var (ᴛ1, ᴛ2) = go.runtime_package.vgetrandom(p, flags);
    return (ᴛ1, ᴛ2);
}

} // end unix_package
