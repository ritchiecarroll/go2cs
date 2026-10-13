// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build goexperiment.swissmap
namespace go;

using static global::go.runtime_package;

partial class runtime_internal_test_package {

public static void MapTombstoneCheck(map<nint, nint> m) {
    // TODO
}

} // end runtime_internal_test_package
