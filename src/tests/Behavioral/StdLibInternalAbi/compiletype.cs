// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go;

partial class main_package {

public static nint CommonSize(nint ptrSize) {
    return 4 * ptrSize + 8 + 8;
}

public static nint StructFieldSize(nint ptrSize) {
    return 3 * ptrSize;
}

public static uint64 UncommonSize() {
    return 4 + 2 + 2 + 4 + 4;
}

public static nint TFlagOff(nint ptrSize) {
    return 2 * ptrSize + 4;
}

public static nint ITabTypeOff(nint ptrSize) {
    return ptrSize;
}

} // end main_package
