// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.os;

using fs = go.io.fs_package;
using syscall = syscall_package;
using go.io;

partial class exec_package {

// skipStdinCopyError optionally specifies a function which reports
// whether the provided stdin copy error should be ignored.
internal static bool skipStdinCopyError(error err) {
    // Ignore ERROR_BROKEN_PIPE and ERROR_NO_DATA errors copying
    // to stdin if the program completed successfully otherwise.
    // See Issue 20445.
    syscall.Errno _ERROR_NO_DATA = /* syscall.Errno(0xe8) */ 232;
    var (pe, ok) = err._<ж<fs.PathError>>(ᐧ);
    return ok && pe.Op == "write"u8 && pe.Path == "|1"u8 && (AreEqual(pe.Err, syscall.ERROR_BROKEN_PIPE) || AreEqual(pe.Err, _ERROR_NO_DATA));
}

} // end exec_package
