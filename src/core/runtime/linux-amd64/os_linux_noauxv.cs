// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build linux && !arm && !arm64 && !loong64 && !mips && !mipsle && !mips64 && !mips64le && !s390x && !ppc64 && !ppc64le
namespace go;

partial class runtime_package {

internal static void archauxv(uintptr tag, uintptr val) {
}

} // end runtime_package
