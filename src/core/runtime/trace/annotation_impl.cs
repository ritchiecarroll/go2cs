// annotation_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// Hand-written bodies for runtime/trace's four user-annotation pushes. annotation.go declares them
// bodyless and Go's runtime supplies them by //go:linkname (runtime/traceruntime.go's
// trace_userTaskCreate, trace_userTaskEnd, trace_userRegion and trace_userLog). That push has no CLR
// counterpart, so go2cs emitted four bodyless `partial` methods and the PartialStubGenerator filled each
// with a throwing stub: trace.NewTask, Task.End, trace.Log, trace.WithRegion and a traced StartRegion
// died on their first call, whether or not tracing was on, where Go only annotates.
//
// TRACING OFF, the bodies are exactly Go's. Each runtime push opens with traceAcquire and returns when
// tracing is off ("the caller won't have it" -- NewTask, Task.End, Log and WithRegion call through
// without checking IsEnabled), so an annotated program does nothing extra and runs on.
//
// TRACING ON, they are a MODEL LIMIT. golib's managed execution tracer (runtime/<goos>/trace_impl.cs,
// docs/phase4/DESIGN-managed-execution-tracer.md) writes the goroutine events the managed runtime
// observes, and its amendment lists the user API's tasks, regions and logs among the events it NEVER
// emits. So while tracing is on these calls are omitted from the trace: the program runs as Go's does,
// and a trace reader finds no EvUserTaskBegin/End, EvUserRegionBegin/End or EvUserLog in it. The one
// corpus test that asserts such an event is runtime's TestCrashWhileTracing (its child's trace.Log),
// skipped host-fatal in runtime/go2cs_test_disclosures.json.

namespace go.runtime;

partial class trace_package
{
    // emits UserTaskCreate event.
    internal static partial void userTaskCreate(uint64 id, uint64 parentID, @string taskType)
    {
        // MODEL LIMIT while tracing: EvUserTaskBegin is never emitted.
    }

    // emits UserTaskEnd event.
    internal static partial void userTaskEnd(uint64 id)
    {
        // MODEL LIMIT while tracing: EvUserTaskEnd is never emitted.
    }

    // emits UserRegion event.
    internal static partial void userRegion(uint64 id, uint64 mode, @string regionType)
    {
        // MODEL LIMIT while tracing: EvUserRegionBegin and EvUserRegionEnd are never emitted.
    }

    // emits UserLog event.
    internal static partial void userLog(uint64 id, @string category, @string message)
    {
        // MODEL LIMIT while tracing: EvUserLog is never emitted.
    }
}
