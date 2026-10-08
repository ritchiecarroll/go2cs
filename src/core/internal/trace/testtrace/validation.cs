// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.trace;

using errors = errors_package;
using fmt = fmt_package;
using trace = go.@internal.trace_package;
using slices = slices_package;
using strings = strings_package;
using go.@internal;
using iter = iter_package;
using ꓸꓸꓸany = Span<any>;

partial class testtrace_package {

// Validator is a type used for validating a stream of trace.Events.
partial struct Validator {
    internal traceꓸTime lastTs;
    internal map<trace.GoID, ж<goState>> gs;
    internal map<trace.ProcID, ж<procState>> ps;
    internal map<trace.ThreadID, ж<schedContext>> ms;
    internal map<trace.ResourceID, slice<@string>> ranges;
    internal map<trace.TaskID, @string> tasks;
    internal bool seenSync;
    public bool Go121;
}

partial struct schedContext {
    public trace.ThreadID M;
    public trace.ProcID P;
    public trace.GoID G;
}

partial struct goState {
    internal trace.GoState state;
    internal ж<schedContext> binding;
}

partial struct procState {
    internal trace.ProcState state;
    internal ж<schedContext> binding;
}

// NewValidator creates a new Validator.
public static ж<Validator> NewValidator() {
    return Ꮡ(new Validator(
        gs: new map<trace.GoID, ж<goState>>(),
        ps: new map<trace.ProcID, ж<procState>>(),
        ms: new map<trace.ThreadID, ж<schedContext>>(),
        ranges: new map<trace.ResourceID, slice<@string>>(),
        tasks: new map<trace.TaskID, @string>()
    ));
}

// Event validates ev as the next event in a stream of trace.Events.
//
// Returns an error if validation fails.
public static error Event(this ж<Validator> Ꮡv, traceꓸEvent ev) {
    ev = ev.ΔClone();

    ref var v = ref Ꮡv.DerefOrNull();
    var e = @new<errAccumulator>();
    // Validate timestamp order.
    if (v.lastTs != 0){
        if (ev.Time() <= v.lastTs){
            e.Errorf("timestamp out-of-order for %+v"u8, ev);
        } else {
            v.lastTs = ev.Time();
        }
    } else {
        v.lastTs = ev.Time();
    }
    // Validate event stack.
    checkStack(e, ev.Stack());
    var exprᴛ1 = ev.Kind();
    if (exprᴛ1 == trace.EventSync) {
        v.seenSync = true;
    }
    else if (exprᴛ1 == trace.EventMetric) {
        var m = ev.Metric();
        if (!strings.Contains(m.Name, // Just record that we've seen a Sync at some point.
 ":"u8)) {
            // Should have a ":" as per runtime/metrics convention.
            e.Errorf("invalid metric name %q"u8, m.Name);
        }
        if (m.Value.Kind() == trace.ValueBad) {
            // Make sure the value is OK.
            e.Errorf("invalid value"u8);
        }
        var exprᴛ2 = m.Value.Kind();
        if (exprᴛ2 == trace.ValueUint64) {
            _ = m.Value.Uint64();
        }

    }
    else if (exprᴛ1 == trace.EventLabel) {
        var l = ev.Label();
        if (l.ΔΔLabel == ""u8) {
            // Just make sure it doesn't panic.
            // Check label.
            e.Errorf("invalid label %q"u8, l.ΔΔLabel);
        }
        if (l.Resource.Kind == trace.ResourceNone) {
            // Check label resource.
            e.Errorf("label resource none"u8);
        }
        var exprᴛ3 = l.Resource.Kind;
        if (exprᴛ3 == trace.ResourceGoroutine) {
            var id = l.Resource.Goroutine();
            {
                var (_, ok) = v.gs[id, ꟷ]; if (!ok) {
                    e.Errorf("label for invalid goroutine %d"u8, id);
                }
            }
        }
        else if (exprᴛ3 == trace.ResourceProc) {
            var id = l.Resource.Proc();
            {
                var (_, ok) = v.ps[id, ꟷ]; if (!ok) {
                    e.Errorf("label for invalid proc %d"u8, id);
                }
            }
        }
        else if (exprᴛ3 == trace.ResourceThread) {
            var id = l.Resource.Thread();
            {
                var (_, ok) = v.ms[id, ꟷ]; if (!ok) {
                    e.Errorf("label for invalid thread %d"u8, id);
                }
            }
        }

    }
    else if (exprᴛ1 == trace.EventStackSample) {
    }
    else if (exprᴛ1 == trace.EventStateTransition) {
        var tr = ev.StateTransition();
        checkStack(e, // Not much to check here. It's basically a sched context and a stack.
 // The sched context is also not guaranteed to align with other events.
 // We already checked the stack above.
 // Validate state transitions.
 //
 // TODO(mknyszek): A lot of logic is duplicated between goroutines and procs.
 // The two are intentionally handled identically; from the perspective of the
 // API, resources all have the same general properties. Consider making this
 // code generic over resources and implementing validation just once.
 tr.Stack);
        var exprᴛ4 = tr.Resource.Kind;
        if (exprᴛ4 == trace.ResourceGoroutine) {
            var id = tr.Resource.Goroutine();
            ref var newΔ5 = ref heap<trace.GoState>(out var ᏑnewΔ5);
            (var old, newΔ5) = tr.Goroutine();
            if (newΔ5 == trace.GoUndetermined) {
                // Basic state transition validation.
                e.Errorf("transition to undetermined state for goroutine %d"u8, id);
            }
            if (v.seenSync && old == trace.GoUndetermined) {
                e.Errorf("undetermined goroutine %d after first global sync"u8, id);
            }
            if (newΔ5 == trace.GoNotExist && v.hasAnyRange(trace.MakeResourceID(id))) {
                e.Errorf("goroutine %d died with active ranges"u8, id);
            }
            var (state, ok) = v.gs[id, ꟷ];
            if (ok){
                if (old != (~state).state) {
                    e.Errorf("bad old state for goroutine %d: got %s, want %s"u8, id, old, (~state).state);
                }
                state.Value.state = newΔ5;
            } else {
                if (old != trace.GoUndetermined && old != trace.GoNotExist) {
                    e.Errorf("bad old state for unregistered goroutine %d: %s"u8, id, old);
                }
                state = Ꮡ(new goState(state: newΔ5));
                v.gs[id] = state;
            }
            if (newΔ5.Executing()){
                // Validate sched context.
                var ctx = Ꮡv.getOrCreateThread(e, ev, ev.Thread());
                if (ctx != nil) {
                    if ((~ctx).G != trace.NoGoroutine && (~ctx).G != id) {
                        e.Errorf("tried to run goroutine %d when one was already executing (%d) on thread %d"u8, id, (~ctx).G, ev.Thread());
                    }
                    ctx.Value.G = id;
                    state.Value.binding = ctx;
                }
            } else 
            if (old.Executing() && !newΔ5.Executing()) {
                if (tr.Stack != ev.Stack()) {
                    // This is a case where the transition is happening to a goroutine that is also executing, so
                    // these two stacks should always match.
                    e.Errorf("StateTransition.Stack doesn't match Event.Stack"u8);
                }
                var ctx = state.Value.binding;
                if (ctx != nil){
                    if ((~ctx).G != id) {
                        e.Errorf("tried to stop goroutine %d when it wasn't currently executing (currently executing %d) on thread %d"u8, id, (~ctx).G, ev.Thread());
                    }
                    ctx.Value.G = trace.NoGoroutine;
                    state.Value.binding = default!;
                } else {
                    e.Errorf("stopping goroutine %d not bound to any active context"u8, id);
                }
            }
        }
        else if (exprᴛ4 == trace.ResourceProc) {
            var id = tr.Resource.Proc();
            ref var newΔ6 = ref heap<trace.ProcState>(out var ᏑnewΔ6);
            (var old, newΔ6) = tr.Proc();
            if (newΔ6 == trace.ProcUndetermined) {
                // Basic state transition validation.
                e.Errorf("transition to undetermined state for proc %d"u8, id);
            }
            if (v.seenSync && old == trace.ProcUndetermined) {
                e.Errorf("undetermined proc %d after first global sync"u8, id);
            }
            if (newΔ6 == trace.ProcNotExist && v.hasAnyRange(trace.MakeResourceID(id))) {
                e.Errorf("proc %d died with active ranges"u8, id);
            }
            var (state, ok) = v.ps[id, ꟷ];
            if (ok){
                if (old != (~state).state) {
                    e.Errorf("bad old state for proc %d: got %s, want %s"u8, id, old, (~state).state);
                }
                state.Value.state = newΔ6;
            } else {
                if (old != trace.ProcUndetermined && old != trace.ProcNotExist) {
                    e.Errorf("bad old state for unregistered proc %d: %s"u8, id, old);
                }
                state = Ꮡ(new procState(state: newΔ6));
                v.ps[id] = state;
            }
            if (newΔ6.Executing()){
                // Validate sched context.
                var ctx = Ꮡv.getOrCreateThread(e, ev, ev.Thread());
                if (ctx != nil) {
                    if ((~ctx).P != trace.NoProc && (~ctx).P != id) {
                        e.Errorf("tried to run proc %d when one was already executing (%d) on thread %d"u8, id, (~ctx).P, ev.Thread());
                    }
                    ctx.Value.P = id;
                    state.Value.binding = ctx;
                }
            } else 
            if (old.Executing() && !newΔ6.Executing()) {
                var ctx = state.Value.binding;
                if (ctx != nil){
                    if ((~ctx).P != id) {
                        e.Errorf("tried to stop proc %d when it wasn't currently executing (currently executing %d) on thread %d"u8, id, (~ctx).P, (~ctx).M);
                    }
                    ctx.Value.P = trace.NoProc;
                    state.Value.binding = default!;
                } else {
                    e.Errorf("stopping proc %d not bound to any active context"u8, id);
                }
            }
        }

    }
    else if (exprᴛ1 == trace.EventRangeBegin || exprᴛ1 == trace.EventRangeActive || exprᴛ1 == trace.EventRangeEnd) {
        var r = ev.Range();
        var exprᴛ5 = ev.Kind();
        if (exprᴛ5 == trace.EventRangeBegin) {
            if (v.hasRange(r.Scope, // Validate ranges.
 r.Name)) {
                e.Errorf("already active range %q on %v begun again"u8, r.Name, r.Scope);
            }
            v.addRange(r.Scope, r.Name);
        }
        else if (exprᴛ5 == trace.EventRangeActive) {
            if (!v.hasRange(r.Scope, r.Name)) {
                v.addRange(r.Scope, r.Name);
            }
        }
        else if (exprᴛ5 == trace.EventRangeEnd) {
            if (!v.hasRange(r.Scope, r.Name)) {
                e.Errorf("inactive range %q on %v ended"u8, r.Name, r.Scope);
            }
            v.deleteRange(r.Scope, r.Name);
        }

    }
    else if (exprᴛ1 == trace.EventTaskBegin) {
        var t = ev.Task();
        if (t.ID == trace.NoTask || t.ID == trace.BackgroundTask) {
            // Validate task begin.
            // The background task should never have an event emitted for it.
            e.Errorf("found invalid task ID for task of type %s"u8, t.Type);
        }
        if (t.Parent == trace.BackgroundTask) {
            // It's not possible for a task to be a subtask of the background task.
            e.Errorf("found background task as the parent for task of type %s"u8, t.Type);
        }
        v.tasks[t.ID] = t.Type;
    }
    else if (exprᴛ1 == trace.EventTaskEnd) {
        var t = ev.Task();
        {
            var (typ, ok) = v.tasks[t.ID, ꟷ]; if (ok) {
                // N.B. Don't check the task type. Empty string is a valid task type.
                // Validate task end.
                // We can see a task end without a begin, so ignore a task without information.
                // Instead, if we've seen the task begin, just make sure the task end lines up.
                if (t.Type != typ) {
                    e.Errorf("task end type %q doesn't match task start type %q for task %d"u8, t.Type, typ, t.ID);
                }
                delete(v.tasks, t.ID);
            }
        }
    }
    else if (exprᴛ1 == trace.EventLog) {
        _ = ev.Log();
    }

    // There's really not much here to check, except that we can
    // generate a Log. The category and message are entirely user-created,
    // so we can't make any assumptions as to what they are. We also
    // can't validate the task, because proving the task's existence is very
    // much best-effort.
    return e.Errors();
}

internal static bool hasRange(this ref Validator v, trace.ResourceID r, @string name) {
    var (ranges, ok) = v.ranges[r, ꟷ];
    return ok && slices.Contains(ranges, name);
}

internal static void addRange(this ref Validator v, trace.ResourceID r, @string name) {
    var (ranges, _) = v.ranges[r, ꟷ];
    ranges = append(ranges, name);
    v.ranges[r] = ranges;
}

internal static bool hasAnyRange(this ref Validator v, trace.ResourceID r) {
    var (ranges, ok) = v.ranges[r, ꟷ];
    return ok && len(ranges) != 0;
}

internal static void deleteRange(this ref Validator v, trace.ResourceID r, @string name) {
    var (ranges, ok) = v.ranges[r, ꟷ];
    if (!ok) {
        return;
    }
    nint i = slices.Index(ranges, name);
    if (i < 0) {
        return;
    }
    v.ranges[r] = slices.Delete<slice<@string>, @string>(ranges, i, i + 1);
}

internal static ж<schedContext> getOrCreateThread(this ж<Validator> Ꮡv, ж<errAccumulator> Ꮡe, traceꓸEvent ev, trace.ThreadID m) {
    ev = ev.ΔClone();

    ref var v = ref Ꮡv.DerefOrNull();
    ref var e = ref Ꮡe.DerefOrNull();
    var evʗ1 = ev;
    bool lenient() {
        // Be lenient about GoUndetermined -> GoSyscall transitions if they
        // originate from an old trace. These transitions lack thread
        // information in trace formats older than 1.22.
        if (!Ꮡv.Value.Go121) {
            return false;
        }
        if (evʗ1.Kind() != trace.EventStateTransition) {
            return false;
        }
        var tr = evʗ1.StateTransition();
        if (tr.Resource.Kind != trace.ResourceGoroutine) {
            return false;
        }
        var (from, to) = tr.Goroutine();
        return from == trace.GoUndetermined && to == trace.GoSyscall;
    }
    if (m == trace.NoThread && !lenient()) {
        e.Errorf("must have thread, but thread ID is none"u8);
        return default!;
    }
    var (s, ok) = v.ms[m, ꟷ];
    if (!ok) {
        s = Ꮡ(new schedContext(M: m, P: trace.NoProc, G: trace.NoGoroutine));
        v.ms[m] = s;
        return s;
    }
    return s;
}

internal static void checkStack(ж<errAccumulator> Ꮡe, traceꓸStack stk) {
    ref var e = ref Ꮡe.DerefOrNull();

    // Check for non-empty values, but we also check for crashes due to incorrect validation.
    foreach (var (i, f) in slices.Collect(stk.Frames())) {
        if (i == 0) {
            // Allow for one fully zero stack.
            //
            // TODO(mknyszek): Investigate why that happens.
            continue;
        }
        if (f.Func == ""u8 || f.File == ""u8 || f.PC == 0 || f.Line == 0) {
            e.Errorf("invalid stack frame %#v: missing information"u8, f);
        }
    }
}

partial struct errAccumulator {
    internal slice<error> errs;
}

internal static void Errorf(this ref errAccumulator e, @string f, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    e.errs = append(e.errs, fmt.Errorf(f, args.ꓸꓸꓸ));
}

internal static error Errors(this ref errAccumulator e) {
    return errors.Join(e.errs.ꓸꓸꓸ);
}

} // end testtrace_package
