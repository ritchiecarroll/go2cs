// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.testing;

using testing = testing_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;

partial class synctest_test_package {

// helperLog is a t.Helper which logs.
// Since it is a helper, the log prefix should contain
// the caller's file, not helper_test.go.
internal static partial void helperLog(ж<testing.T> Ꮡt, @string s) {
    Ꮡt.Helper();
    Ꮡt.Log(s);
}

} // end synctest_test_package
