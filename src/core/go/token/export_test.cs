// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using static global::go.go.token_package;

partial class token_internal_test_package {

// exports for tests
public static nint SearchInts(slice<nint> a, nint x) {
    return searchInts(a, x);
}

} // end token_internal_test_package
