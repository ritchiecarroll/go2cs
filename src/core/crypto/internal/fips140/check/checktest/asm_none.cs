// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build (!386 && !amd64 && !arm && !arm64) || purego
namespace go.crypto.@internal.fips140.check;

using @unsafe = unsafe_package;

partial class checktest_package {

public static ж<uint32> PtrStaticData() {
    return default;
}

public static @unsafe.Pointer PtrStaticText() {
    return default;
}

} // end checktest_package
