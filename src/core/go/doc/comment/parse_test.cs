// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go.doc;

using testing = testing_package;
using static global::go.go.doc.comment_package;

partial class comment_internal_test_package {

// See https://golang.org/issue/52353
public static void Test52353(ж<testing.T> t) {
    ident("𫕐ﯯ");
}

} // end comment_internal_test_package
