// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

partial class runtime_package {

//go:nosplit
internal static int64 cputicks() {
    // runtime·nanotime() is a poor approximation of CPU ticks that is enough for the profiler.
    return nanotime();
}

} // end runtime_package
