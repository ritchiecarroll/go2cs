// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !windows
namespace go.crypto.@internal.fips140deps;

using time = go.time_package;
using go;

partial class time_package {

internal static time.Time start = time.Now();

// HighPrecisionNow returns a high-resolution timestamp suitable for measuring
// small time differences. It uses the time package's monotonic clock.
//
// Its unit, epoch, and resolution are unspecified, and may change, but can be
// assumed to be sufficiently precise to measure time differences on the order
// of tens to hundreds of nanoseconds.
public static int64 HighPrecisionNow() {
    return (int64)time.Since(start);
}

} // end time_package
