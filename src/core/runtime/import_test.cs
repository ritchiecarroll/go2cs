// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file and importx_test.go make it possible to write tests in the runtime
// package, which is generally more convenient for testing runtime internals.
// For tests that mostly touch public APIs, it's generally easier to write them
// in the runtime_test package and export any runtime internals via
// export_test.go.
//
// There are a few limitations on runtime package tests that this bridges:
//
// 1. Tests use the signature "XTest<name>(t TestingT)". Since runtime can't import
// testing, test functions can't use testing.T, so instead we have the T
// interface, which *testing.T satisfies. And we start names with "XTest"
// because otherwise go test will complain about Test functions with the wrong
// signature. To actually expose these as test functions, this file contains
// trivial wrappers.
//
// 2. Runtime package tests can't directly import other std packages, so we
// inject any necessary functions from std.
// TODO: Generate this
namespace go;

using fmt = fmt_package;
using testenv = @internal.testenv_package;
using Δruntime = runtime_package;
using testing = testing_package;
using @internal;
using static global::go.runtime_internal_test_package;

partial class runtime_test_package {

[GoInit] internal static void initΔ3() {
    runtime_internal_test_package.FmtSprintf = fmt.Sprintfᶠ;
    runtime_internal_test_package.TestenvOptimizationOff = testenv.OptimizationOff;
}

public static void TestInlineUnwinder(ж<testing.T> t) {
    runtime_internal_test_package.XTestInlineUnwinder(new runtime_test_package.testing_TжTestingT(t));
}

public static void TestSPWrite(ж<testing.T> t) {
    runtime_internal_test_package.XTestSPWrite(new runtime_test_package.testing_TжTestingT(t));
}

} // end runtime_test_package
