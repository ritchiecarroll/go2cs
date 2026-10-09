// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix || windows
// Export guts for testing on posix.
// Since testing imports os and os imports internal/poll,
// the internal/poll tests can not be in package poll.
namespace go.@internal;

using static go.@internal.poll_package;

partial class poll_internal_test_package {

internal static error EOFError(this ref global::go.@internal.poll_package.FD fd, nint n, error err) {
    return fd.eofError(n, err);
}

} // end poll_internal_test_package
