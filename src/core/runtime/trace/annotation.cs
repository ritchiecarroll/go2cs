// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.runtime;

using context = context_package;
using fmt = fmt_package;
using atomic = go.sync.atomic_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
using go.sync;
using ꓸꓸꓸany = Span<any>;

partial class trace_package {

partial struct traceContextKey {
}

// NewTask creates a task instance with the type taskType and returns
// it along with a Context that carries the task.
// If the input context contains a task, the new task is its subtask.
//
// The taskType is used to classify task instances. Analysis tools
// like the Go execution tracer may assume there are only a bounded
// number of unique task types in the system.
//
// The returned Task's [Task.End] method is used to mark the task's end.
// The trace tool measures task latency as the time between task creation
// and when the End method is called, and provides the latency
// distribution per task type.
// If the End method is called multiple times, only the first
// call is used in the latency measurement.
//
//	ctx, task := trace.NewTask(ctx, "awesomeTask")
//	trace.WithRegion(ctx, "preparation", prepWork)
//	// preparation of the task
//	go func() {  // continue processing the task in a separate goroutine.
//	    defer task.End()
//	    trace.WithRegion(ctx, "remainingWork", remainingWork)
//	}()
public static (context.Context ctx, ж<Task> task) NewTask(context.Context pctx, @string taskType) {
    var pid = fromContext(pctx).Value.id;
    ref var id = ref heap<uint64>(out var Ꮡid);
    id = newID();
    userTaskCreate(id, pid, taskType);
    var s = Ꮡ(new Task(id: id));
    return (context.WithValue(pctx, new traceContextKey(nil), s.OrTypedNil()), s);
}

// We allocate a new task even when
// the tracing is disabled because the context and task
// can be used across trace enable/disable boundaries,
// which complicates the problem.
//
// For example, consider the following scenario:
//   - trace is enabled.
//   - trace.WithRegion is called, so a new context ctx
//     with a new region is created.
//   - trace is disabled.
//   - trace is enabled again.
//   - trace APIs with the ctx is called. Is the ID in the task
//   a valid one to use?
//
// TODO(hyangah): reduce the overhead at least when
// tracing is disabled. Maybe the id can embed a tracing
// round number and ignore ids generated from previous
// tracing round.
internal static ж<Task> fromContext(context.Context ctx) {
    {
        var (s, ok) = ctx.Value(new traceContextKey(nil))._<ж<Task>>(ᐧ); if (ok) {
            return s;
        }
    }
    return ᏑbgTask;
}

// Task is a data type for tracing a user-defined, logical operation.
partial struct Task {
    internal uint64 id;
}

// TODO(hyangah): record parent id?

// End marks the end of the operation represented by the [Task].
public static void End(this ref Task t) {
    userTaskEnd(t.id);
}

internal static ж<uint64> ᏑlastTaskID = new StandardBox<uint64>(0);
internal static ref uint64 lastTaskID => ref ᏑlastTaskID.Value; // task id issued last time

internal static uint64 newID() {
    // TODO(hyangah): use per-P cache
    return atomic.AddUint64(ᏑlastTaskID, 1);
}

internal static ж<Task> ᏑbgTask = new StandardBox<Task>(new Task(id: (uint64)0));
internal static ref Task bgTask => ref ᏑbgTask.Value;

// Log emits a one-off event with the given category and message.
// Category can be empty and the API assumes there are only a handful of
// unique categories in the system.
public static void Log(context.Context ctx, @string category, @string message) {
    var id = fromContext(ctx).Value.id;
    userLog(id, category, message);
}

// Logf is like [Log], but the value is formatted using the specified format spec.
public static void Logf(context.Context ctx, @string category, @string format, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    if (IsEnabled()) {
        // Ideally this should be just Log, but that will
        // add one more frame in the stack trace.
        var id = fromContext(ctx).Value.id;
        userLog(id, category, fmt.Sprintf(format, args.ꓸꓸꓸ));
    }
}

internal const uint64 regionStartCode = /* uint64(0) */ 0;
internal const uint64 regionEndCode = /* uint64(1) */ 1;

// WithRegion starts a region associated with its calling goroutine, runs fn,
// and then ends the region. If the context carries a task, the region is
// associated with the task. Otherwise, the region is attached to the background
// task.
//
// The regionType is used to classify regions, so there should be only a
// handful of unique region types.
public static void WithRegion(context.Context ctx, @string regionType, Action fn) {
    GoFrame ᒐ = default;
    try {
        // NOTE:
        // WithRegion helps avoiding misuse of the API but in practice,
        // this is very restrictive:
        // - Use of WithRegion makes the stack traces captured from
        //   region start and end are identical.
        // - Refactoring the existing code to use WithRegion is sometimes
        //   hard and makes the code less readable.
        //     e.g. code block nested deep in the loop with various
        //          exit point with return values
        // - Refactoring the code to use this API with closure can
        //   cause different GC behavior such as retaining some parameters
        //   longer.
        // This causes more churns in code than I hoped, and sometimes
        // makes the code less readable.
        var id = fromContext(ctx).Value.id;
        userRegion(id, regionStartCode, regionType);
        defer(userRegion, id, regionEndCode, regionType, ref ᒐ);
        fn();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// StartRegion starts a region and returns it.
// The returned Region's [Region.End] method must be called
// from the same goroutine where the region was started.
// Within each goroutine, regions must nest. That is, regions started
// after this region must be ended before this region can be ended.
// Recommended usage is
//
//	defer trace.StartRegion(ctx, "myTracedRegion").End()
public static ж<Region> StartRegion(context.Context ctx, @string regionType) {
    if (!IsEnabled()) {
        return noopRegion;
    }
    ref var id = ref heap<uint64>(out var Ꮡid);
    id = fromContext(ctx).Value.id;
    userRegion(id, regionStartCode, regionType);
    return Ꮡ(new Region(id, regionType));
}

// Region is a region of code whose execution time interval is traced.
partial struct Region {
    internal uint64 id;
    internal @string regionType;
}

internal static ж<Region> noopRegion = Ꮡ(new Region(nil));

// End marks the end of the traced code region.
public static void End(this ж<Region> Ꮡr) {
    ref var r = ref Ꮡr.DerefOrNull();

    if (Ꮡr == noopRegion) {
        return;
    }
    userRegion(r.id, regionEndCode, r.regionType);
}

// IsEnabled reports whether tracing is enabled.
// The information is advisory only. The tracing status
// may have changed by the time this function returns.
public static bool IsEnabled() {
    return Ꮡtracing.of(tracingᴛ1.Ꮡenabled).Load();
}

//
// Function bodies are defined in runtime/trace.go
//

// emits UserTaskCreate event.
internal static partial void userTaskCreate(uint64 id, uint64 parentID, @string taskType);

// emits UserTaskEnd event.
internal static partial void userTaskEnd(uint64 id);

// emits UserRegion event.
internal static partial void userRegion(uint64 id, uint64 mode, @string regionType);

// emits UserLog event.
internal static partial void userLog(uint64 id, @string category, @string message);

} // end trace_package
