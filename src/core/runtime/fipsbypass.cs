// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)

partial class runtime_package {

//go:linkname fips140_setBypass crypto/fips140.setBypass
public static void fips140_setBypass() {
    getg().Value.fipsOnlyBypass = true;
}

//go:linkname fips140_unsetBypass crypto/fips140.unsetBypass
public static void fips140_unsetBypass() {
    getg().Value.fipsOnlyBypass = false;
}

//go:linkname fips140_isBypassed crypto/fips140.isBypassed
public static bool fips140_isBypassed() {
    return (~getg()).fipsOnlyBypass;
}

} // end runtime_package
