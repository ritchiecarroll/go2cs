// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using version = go.@internal.trace.version_package;
using go.@internal.trace;
using static go.@internal.trace_package;
using static trace_internal_test_package;

partial class trace_internal_test_package {

} partial class trace_internal_testᴛReader {

// GoVersion is the version set in the trace header.
internal static version.Version GoVersion(this ref global::go.@internal.trace_package.Reader r) {
    return r.version;
}

} // end trace_internal_testᴛReader
