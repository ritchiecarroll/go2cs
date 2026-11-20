// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using reflect = reflect_package;
using static go.@internal.fmtsort_package;

partial class fmtsort_internal_test_package {

public static nint Compare(reflect.Value a, reflect.Value b) {
    return compare(a, b);
}

} // end fmtsort_internal_test_package
