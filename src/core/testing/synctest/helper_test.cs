// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.testing;

using testing = testing_package;

partial class synctest_test_package {

// helperLog is a t.Helper which logs.
// Since it is a helper, the log prefix should contain
// the caller's file, not helper_test.go.
internal static partial void helperLog(ж<testing.T> t, @string s) {
    t.Helper();
    t.Log(s);
}

} // end synctest_test_package
