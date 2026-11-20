// Copyright (c) 2014 The mathutil Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

partial class mathutil_package {

public static nint QCmpUint32(uint32 a, uint32 b, uint32 c, uint32 d) {
    var (x, y) = ((uint64)a * (uint64)d, (uint64)b * (uint64)c);
    switch (ᐧ) {
    case {} when x < y: {
        return -1;
    }
    case {} when x == y: {
        return 0;
    }
    default: {
        return 1;
    }}

}

public static uint64 /*a*/ QScaleUint32(uint32 b, uint32 c, uint32 d) {
    return 1 + ((uint64)b * (uint64)c) / (uint64)d;
}

} // end mathutil_package
