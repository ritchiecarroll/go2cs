// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140deps;

using windows = go.@internal.syscall.windows_package;
using go.@internal.syscall;

partial class time_package {

// HighPrecisionNow returns a high-resolution timestamp suitable for measuring
// small time differences. It uses Windows' QueryPerformanceCounter.
//
// Its unit, epoch, and resolution are unspecified, and may change, but can be
// assumed to be sufficiently precise to measure time differences on the order
// of tens to hundreds of nanoseconds.
public static int64 HighPrecisionNow() {
    return windows.QueryPerformanceCounter();
}

} // end time_package
