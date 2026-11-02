// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !plan9 && !windows
namespace go.os;

using fs = go.io.fs_package;
using syscall = syscall_package;
using go.io;

partial class exec_package {

// skipStdinCopyError optionally specifies a function which reports
// whether the provided stdin copy error should be ignored.
internal static bool skipStdinCopyError(error err) {
    // Ignore EPIPE errors copying to stdin if the program
    // completed successfully otherwise.
    // See Issue 9173.
    var (pe, ok) = err._<ж<fs.PathError>>(ᐧ);
    return ok && pe.Op == "write"u8 && pe.Path == "|1"u8 && AreEqual(pe.Err, syscall.EPIPE);
}

} // end exec_package
