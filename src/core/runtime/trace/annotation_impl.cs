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
// without checking IsEnabled), so an annotated program does nothing extra and runs on. golib's
// ExecutionTracer makes the same check first.
//
// TRACING ON, they write Go's events: golib's managed execution tracer (runtime/trace_impl.cs,
// docs/phase4/DESIGN-managed-execution-tracer.md) writes EvUserTaskBegin/End, EvUserRegionBegin/End and
// EvUserLog on the calling goroutine, with the Go strings' bytes, exactly as the runtime pushes would.
// Until 2026-10-21 these were a MODEL LIMIT (the design listed the user API's events among those it never
// emits); the design's amendment of that date reverses it, because they are facts the program states,
// and runtime/trace's flight recorder test (TestFlightRecorderLog) reads a trace.Log back from its snapshot.

using go.golib;

namespace go.runtime;

partial class trace_package
{
    // emits UserTaskCreate event.
    internal static partial void userTaskCreate(uint64 id, uint64 parentID, @string taskType) =>
        ExecutionTracer.OnUserTaskBegin(id, parentID, taskType.ToSpan());

    // emits UserTaskEnd event.
    internal static partial void userTaskEnd(uint64 id) =>
        ExecutionTracer.OnUserTaskEnd(id);

    // emits UserRegion event.
    internal static partial void userRegion(uint64 id, uint64 mode, @string regionType) =>
        ExecutionTracer.OnUserRegion(id, mode, regionType.ToSpan());

    // emits UserLog event.
    internal static partial void userLog(uint64 id, @string category, @string message) =>
        ExecutionTracer.OnUserLog(id, category.ToSpan(), message.ToSpan());
}
