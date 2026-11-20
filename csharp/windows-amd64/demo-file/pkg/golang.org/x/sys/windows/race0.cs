// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.golang.org.x.sys;

using @unsafe = unsafe_package;

partial class windows_package {

internal const bool raceenabled = false;

internal static void raceAcquire(@unsafe.Pointer addr) {
}

internal static void raceReleaseMerge(@unsafe.Pointer addr) {
}

internal static void raceReadRange(@unsafe.Pointer addr, nint len) {
}

internal static void raceWriteRange(@unsafe.Pointer addr, nint len) {
}

} // end windows_package
