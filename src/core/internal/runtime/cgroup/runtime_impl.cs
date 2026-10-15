// runtime_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// Hand-written body for internal/runtime/cgroup's one runtime bridge. runtime.cs carries Go's
// "Functions below pushed from runtime." and the bodyless `throw` under a one-arg handle; Go 1.26's
// runtime pushes it from panic.go (`//go:linkname cgroup_throw internal/runtime/cgroup.throw`, body
// `throw(s)`). Without this the PartialStubGenerator filled it with a throwing
// NotImplementedException, which any frame above can catch.
//
// WHY A COMPANION AND NOT A //go:linkname REGISTRY ROW: runtime imports internal/runtime/cgroup (the
// cgroup CPU limit read at startup), so a forwarder from cgroup into runtime would close an assembly
// cycle. internal/runtime/maps' fatal and internal/sync's throw take the same route for the same
// reason (their runtime_impl.cs).
//
// userFault is Go's throwType axis, read from the push: cgroup_throw calls throw, which is
// throwTypeRuntime, so userFault is FALSE. FatalReport.Fatal writes `fatal error: <text>` and a
// traceback and exits with status 2; nothing can recover it, as in Go.

// Aliased rather than imported wholesale: this file needs exactly one golib type.
using FatalReport = go.golib.FatalReport;

// Hand-owned (no runtime_impl.go exists, so a reconvert never regenerates it).
[module: go.GoManualConversion]

namespace go.@internal.runtime;

partial class cgroup_package
{
    internal static partial void @throw(@string s) => FatalReport.Fatal(s, userFault: false);
}
