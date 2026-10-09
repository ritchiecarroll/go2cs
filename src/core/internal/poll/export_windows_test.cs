// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Export guts for testing on windows.
// Since testing imports os and os imports internal/poll,
// the internal/poll tests can not be in package poll.
namespace go.@internal;

using static go.@internal.poll_package;

partial class poll_internal_test_package {

public static ж<Action<@string, ж<global::go.@internal.poll_package.FD>, error>> LogInitFD;
internal static void initᴛLogInitFD() { LogInitFD = ᏑlogInitFD; }

internal static bool IsPartOfNetpoll(this ref global::go.@internal.poll_package.FD fd) {
    return fd.pd.runtimeCtx != 0;
}

} // end poll_internal_test_package
