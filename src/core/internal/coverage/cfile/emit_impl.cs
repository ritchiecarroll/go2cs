// emit_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

namespace go.@internal.coverage;

using rtcov = go.@internal.coverage.rtcov_package;

partial class cfile_package
{
    // getCovCounterList is a runtime linkname push (runtime/covercounter.go:
    // `//go:linkname coverage_getCovCounterList internal/coverage/cfile.getCovCounterList`). Go's body
    // walks every module's linker-placed coverage counter section and returns one blob per non-empty
    // section, starting from an EMPTY, non-nil slice. A converted program is never built with -cover, so
    // no module carries a counter section and Go's walk returns that empty slice unchanged; this returns
    // the same value. It is Go's coverage-off answer, as testing.CoverMode() returning "" is, not a
    // stand-in. The push itself stays unlinked (linknamePushTargets records why: the section walk is not
    // something the managed model runs), and before this the PartialStubGenerator's throwing body
    // answered instead.
    //
    // What it decides: runtime/coverage.ClearCounters calls this first and returns Go's
    // "program not built with -cover" on an empty list. WriteCounters calls it only after the covermode
    // check, which a non-cover binary always fails first, so its answer was already Go's.
    // WriteCountersDir, WriteMeta and WriteMetaDir never reach it. Guarded by the Phase5CoverageAPIs
    // behavioral test (all five functions, output-compared against go run).
    internal static partial slice<rtcov.CovCounterBlob> getCovCounterList() => new rtcov.CovCounterBlob[] { }.slice();
}
