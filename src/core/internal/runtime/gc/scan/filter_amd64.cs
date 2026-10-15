// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.runtime.gc;

partial class scan_package {

// FilterNilAVX512 is the simd version of FilterNil,
// it is implemented in assembly.
public static partial int32 FilterNilAVX512(ж<uintptr> bufp, int32 n);

} // end scan_package
