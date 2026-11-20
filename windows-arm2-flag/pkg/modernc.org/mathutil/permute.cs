// Copyright (c) 2014 The mathutil Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

using sort = sort_package;

partial class mathutil_package {

public static void PermutationFirst(sort.Interface data) {
    sort.Sort(data);
}

public static bool PermutationNext(sort.Interface data) {
    nint k = default;
    nint l = default;
    for (k = data.Len() - 2; ; k--) {
        if (k < 0) {
            return false;
        }

        if (data.Less(k, k + 1)) {
            break;
        }
    }
    for (l = data.Len() - 1; !data.Less(k, l); l--) {
    }
    data.Swap(k, l);
    for ((nint i, nint j) = (k + 1, data.Len() - 1); i < j; i++) {
        data.Swap(i, j);
        j--;
    }
    return true;
}

} // end mathutil_package
