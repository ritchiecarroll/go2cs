// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !linux
namespace go.@internal;

using errors = errors_package;
using os = os_package;
using Δruntime = runtime_package;
using static go.@internal.poll_internal_test_package;

partial class poll_test_package {

internal static (ж<os.File>, error) badStateFile() {
    return (default, errors.New("not supported on "u8 + Δruntime.GOOS));
}

internal static (@string, bool) isBadStateFileError(error err) {
    return ("", false);
}

} // end poll_test_package
