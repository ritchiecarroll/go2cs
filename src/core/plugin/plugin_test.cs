// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

// blank import: plugin_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
using testing = testing_package;

partial class plugin_test_package {

public static void TestPlugin(ж<testing.T> Ꮡt) {
    // This test makes sure that executable that imports plugin
    // package can actually run. See issue #28789 for details.
}

} // end plugin_test_package
