// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using fmt = fmt_package;
using diff = go.@internal.diff_package;
using reflect = reflect_package;
using slices = slices_package;
using testing = testing_package;
using time = time_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using go.@internal;
using iter = iter_package;
using static go.@internal.trace_package;

partial class trace_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string wantˢ = "want"u8;
internal static readonly @string gotˢ = "got"u8;
internal static readonly @string metricˢ = "Metric"u8;
internal static readonly @string labelˢ = "Label"u8;
internal static readonly @string rangeˢ = "Range"u8;
internal static readonly @string goroutineTransitionˢ = "GoroutineTransition"u8;
internal static readonly @string procTransitionˢ = "ProcTransition"u8;
internal static readonly @string syncˢ2 = "Sync"u8;
internal static readonly @string taskˢ = "Task"u8;
internal static readonly @string regionˢ = "Region"u8;
internal static readonly @string logˢ = "Log"u8;
internal static readonly @string stackSampleˢ = "StackSample"u8;

internal partial struct TestMakeEvent_stackType /*@string*/;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly TestMakeEvent_stackType schedStackᶜ = "sched stack"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly TestMakeEvent_stackType stStackᶜ = "state transition stack"u8;

internal partial struct TestMakeEvent_tests /*dyn*/ {
    internal @string name;
    internal @string metric;
    internal uint64 val;
    internal global::go.@internal.trace_package.ΔStack stack;
    internal bool valid;
}

internal partial struct TestMakeEvent_testsᴛ1 /*dyn*/ {
    internal global::go.@internal.trace_package.EventKind kind;
    internal @string name;
    internal global::go.@internal.trace_package.ResourceID scope;
    internal bool valid;
}

internal partial struct TestMakeEvent_testsᴛ2 /*dyn*/ {
    internal @string name;
    internal global::go.@internal.trace_package.GoID g;
    internal global::go.@internal.trace_package.ΔStack stack;
    internal global::go.@internal.trace_package.GoID stG;
    internal global::go.@internal.trace_package.GoState from;
    internal global::go.@internal.trace_package.GoState to;
    internal @string reason;
    internal global::go.@internal.trace_package.ΔStack stStack;
    internal bool valid;
}

internal partial struct TestMakeEvent_testsᴛ3 /*dyn*/ {
    internal @string name;
    internal global::go.@internal.trace_package.ProcID proc;
    internal global::go.@internal.trace_package.ProcID schedProc;
    internal global::go.@internal.trace_package.ProcState from;
    internal global::go.@internal.trace_package.ProcState to;
    internal bool valid;
}

internal partial struct TestMakeEvent_testsᴛ4 /*dyn*/ {
    internal @string name;
    internal global::go.@internal.trace_package.EventKind kind;
    internal nint n;
    internal ж<global::go.@internal.trace_package.ClockSnapshot> clock;
    internal map<@string, slice<global::go.@internal.trace_package.ExperimentalBatch>> batches;
    internal bool valid;
}

internal partial struct TestMakeEvent_testsᴛ5 /*dyn*/ {
    internal @string name;
    internal global::go.@internal.trace_package.EventKind kind;
    internal global::go.@internal.trace_package.TaskID id;
    internal global::go.@internal.trace_package.TaskID parent;
    internal @string typ;
    internal bool valid;
}

internal partial struct TestMakeEvent_testsᴛ6 /*dyn*/ {
    internal @string name;
    internal global::go.@internal.trace_package.EventKind kind;
    internal global::go.@internal.trace_package.TaskID task;
    internal @string typ;
    internal bool valid;
}

internal partial struct TestMakeEvent_testsᴛ7 /*dyn*/ {
    internal @string name;
    internal global::go.@internal.trace_package.EventKind kind;
    internal global::go.@internal.trace_package.TaskID task;
    internal @string category;
    internal @string message;
    internal bool valid;
}

internal partial struct TestMakeEvent_testsᴛ8 /*dyn*/ {
    internal @string name;
    internal global::go.@internal.trace_package.EventKind kind;
    internal global::go.@internal.trace_package.ΔStack stack;
    internal bool valid;
}

public static void TestMakeEvent(ж<testing.T> Ꮡt) {
    void checkTime(ж<testing.T> tΔ1, global::go.@internal.trace_package.ΔEvent ev, global::go.@internal.trace_package.ΔTime want) {
        ev = ev.ΔClone();
        tΔ1.Helper();
        if (ev.Time() != want) {
            tΔ1.Errorf("expected time to be %d, got %d"u8, want, ev.Time());
        }
    }
    bool checkValid(ж<testing.T> tΔ2, error err, bool valid) {
        tΔ2.Helper();
        if (valid && err == default!) {
            return true;
        }
        if (valid && err != default!){
            tΔ2.Errorf("expected no error, got %v"u8, err);
        } else 
        if (!valid && err == default!) {
            tΔ2.Errorf("expected error, got %v"u8, err);
        }
        return false;
    }
    TestMakeEvent_stackType schedStack = schedStackᶜ;
    TestMakeEvent_stackType stStack = stStackᶜ;
    void checkStack(ж<testing.T> tΔ3, global::go.@internal.trace_package.ΔStack got, global::go.@internal.trace_package.ΔStack want, TestMakeEvent_stackType which) {
        tΔ3.Helper();
        var diffΔ1 = diff.Diff(wantˢ, slice<byte>(want.String()), gotˢ, slice<byte>(got.String()));
        if (len(diffΔ1) > 0) {
            tΔ3.Errorf("unexpected %s: %s"u8, which, diffΔ1);
        }
    }
    ref var stk1 = ref heap<global::go.@internal.trace_package.ΔStack>(out var Ꮡstk1);
    stk1 = MakeStack(new global::go.@internal.trace_package.StackFrame[]{
        new(PC: 1, Func: "foo"u8, File: "foo.go"u8, Line: 10),
        new(PC: 2, Func: "bar"u8, File: "bar.go"u8, Line: 20)
    }.slice());
    ref var stk2 = ref heap<global::go.@internal.trace_package.ΔStack>(out var Ꮡstk2);
    stk2 = MakeStack(new global::go.@internal.trace_package.StackFrame[]{
        new(PC: 1, Func: "foo"u8, File: "foo.go"u8, Line: 10),
        new(PC: 2, Func: "bar"u8, File: "bar.go"u8, Line: 20)
    }.slice());

    var checkStackʗ1 = checkStack;
    var checkTimeʗ1 = checkTime;
    var checkValidʗ1 = checkValid;
    var stk1ʗ1 = stk1;
    Ꮡt.Run(metricˢ, (ж<testing.T> tΔ4) => {
        var tests = new TestMakeEvent_tests[]{
            new(name: "gomaxprocs"u8, metric: "/sched/gomaxprocs:threads"u8, valid: true, val: 1, stack: NoStack),
            new(name: "gomaxprocs with stack"u8, metric: "/sched/gomaxprocs:threads"u8, valid: true, val: 1, stack: stk1ʗ1),
            new(name: "heap objects"u8, metric: "/memory/classes/heap/objects:bytes"u8, valid: true, val: 2, stack: NoStack),
            new(name: "heap goal"u8, metric: "/gc/heap/goal:bytes"u8, valid: true, val: 3, stack: NoStack),
            new(name: "invalid metric"u8, metric: "/test"u8, valid: false, val: 4, stack: NoStack)
        }.slice();
        foreach (var (i, vᴛ1) in tests) {
            ref var test = ref heap(new TestMakeEvent_tests(), out var Ꮡtest);
            test = vᴛ1;

            var checkStackʗ2 = checkStackʗ1;
            var checkTimeʗ2 = checkTimeʗ1;
            var checkValidʗ2 = checkValidʗ1;
            var testʗ1 = test;
            tΔ4.Run(test.name, (ж<testing.T> tΔ5) => {
                var (ev, err) = MakeEvent(new EventConfig<ΔMetric>(
                    Kind: EventMetric,
                    Time: ((global::go.@internal.trace_package.ΔTime)(int64)(42 + i)),
                    Details: new ΔMetric(Name: testʗ1.metric, Value: Uint64Value(testʗ1.val)),
                    Stack: testʗ1.stack
                ));
                if (!checkValidʗ2(tΔ5, err, testʗ1.valid)) {
                    return;
                }
                checkTimeʗ2(tΔ5, ev, (global::go.@internal.trace_package.ΔTime)(int64)(42 + i));
                checkStackʗ2(tΔ5, ev.Stack(), testʗ1.stack, schedStack);
                var got = ev.Metric();
                if (got.Name != testʗ1.metric) {
                    tΔ5.Errorf("expected name to be %q, got %q"u8, testʗ1.metric, got.Name);
                }
                if (got.Value.Uint64() != testʗ1.val) {
                    tΔ5.Errorf("expected value to be %d, got %d"u8, testʗ1.val, got.Value.Uint64());
                }
            });
        }
    });

    var checkTimeʗ3 = checkTime;
    var checkValidʗ3 = checkValid;
    Ꮡt.Run(labelˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ6) => {
        var (ev, err) = MakeEvent(new EventConfig<ΔLabel>(
            Kind: EventLabel,
            Time: 42,
            Details: new ΔLabel(ΔΔLabel: "test"u8, Resource: MakeResourceID((global::go.@internal.trace_package.GoID)23))
        ));
        if (!checkValidʗ3(tΔ6, err, true)) {
            return;
        }
        var label = ev.Label();
        if (label.ΔΔLabel != "test"u8) {
            tΔ6.Errorf("expected label to be test, got %q"u8, label.ΔΔLabel);
        }
        if (label.Resource.Kind != ResourceGoroutine) {
            tΔ6.Errorf("expected label resource to be goroutine, got %d"u8, label.Resource.Kind);
        }
        if (label.Resource.id != 23) {
            tΔ6.Errorf("expected label resource to be 23, got %d"u8, label.Resource.id);
        }
        checkTimeʗ3(tΔ6, ev, 42);
    });

    var checkTimeʗ4 = checkTime;
    var checkValidʗ4 = checkValid;
    Ꮡt.Run(rangeˢ, (ж<testing.T> tΔ7) => {
        var tests = new TestMakeEvent_testsᴛ1[]{
            new(kind: EventRangeBegin, name: "GC concurrent mark phase"u8, scope: new ResourceID(nil), valid: true),
            new(kind: EventRangeActive, name: "GC concurrent mark phase"u8, scope: new ResourceID(nil), valid: true),
            new(kind: EventRangeEnd, name: "GC concurrent mark phase"u8, scope: new ResourceID(nil), valid: true),
            new(kind: EventMetric, name: "GC concurrent mark phase"u8, scope: new ResourceID(nil), valid: false),
            new(kind: EventRangeBegin, name: "GC concurrent mark phase - INVALID"u8, scope: new ResourceID(nil), valid: false),

            new(kind: EventRangeBegin, name: "GC incremental sweep"u8, scope: MakeResourceID((global::go.@internal.trace_package.ProcID)1), valid: true),
            new(kind: EventRangeActive, name: "GC incremental sweep"u8, scope: MakeResourceID((global::go.@internal.trace_package.ProcID)2), valid: true),
            new(kind: EventRangeEnd, name: "GC incremental sweep"u8, scope: MakeResourceID((global::go.@internal.trace_package.ProcID)3), valid: true),
            new(kind: EventMetric, name: "GC incremental sweep"u8, scope: MakeResourceID((global::go.@internal.trace_package.ProcID)4), valid: false),
            new(kind: EventRangeBegin, name: "GC incremental sweep - INVALID"u8, scope: MakeResourceID((global::go.@internal.trace_package.ProcID)5), valid: false),

            new(kind: EventRangeBegin, name: "GC mark assist"u8, scope: MakeResourceID((global::go.@internal.trace_package.GoID)1), valid: true),
            new(kind: EventRangeActive, name: "GC mark assist"u8, scope: MakeResourceID((global::go.@internal.trace_package.GoID)2), valid: true),
            new(kind: EventRangeEnd, name: "GC mark assist"u8, scope: MakeResourceID((global::go.@internal.trace_package.GoID)3), valid: true),
            new(kind: EventMetric, name: "GC mark assist"u8, scope: MakeResourceID((global::go.@internal.trace_package.GoID)4), valid: false),
            new(kind: EventRangeBegin, name: "GC mark assist - INVALID"u8, scope: MakeResourceID((global::go.@internal.trace_package.GoID)5), valid: false),

            new(kind: EventRangeBegin, name: "stop-the-world (for a good reason)"u8, scope: MakeResourceID((global::go.@internal.trace_package.GoID)1), valid: true),
            new(kind: EventRangeActive, name: "stop-the-world (for a good reason)"u8, scope: MakeResourceID((global::go.@internal.trace_package.GoID)2), valid: false),
            new(kind: EventRangeEnd, name: "stop-the-world (for a good reason)"u8, scope: MakeResourceID((global::go.@internal.trace_package.GoID)3), valid: true),
            new(kind: EventMetric, name: "stop-the-world (for a good reason)"u8, scope: MakeResourceID((global::go.@internal.trace_package.GoID)4), valid: false),
            new(kind: EventRangeBegin, name: "stop-the-world (for a good reason) - INVALID"u8, scope: MakeResourceID((global::go.@internal.trace_package.GoID)5), valid: false)
        }.slice();

        foreach (var (i, vᴛ2) in tests) {
            ref var test = ref heap(new TestMakeEvent_testsᴛ1(), out var Ꮡtest);
            test = vᴛ2;

            @string name = fmt.Sprintf("%s/%s/%s"u8, test.kind, test.name, test.scope);
            var checkTimeʗ5 = checkTimeʗ4;
            var checkValidʗ5 = checkValidʗ4;
            var testʗ2 = test;
            tΔ7.Run(name, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ8) => {
                var (ev, err) = MakeEvent(new EventConfig<ΔRange>(
                    Time: ((global::go.@internal.trace_package.ΔTime)(int64)(42 + i)),
                    Kind: testʗ2.kind,
                    Details: new ΔRange(Name: testʗ2.name, Scope: testʗ2.scope)
                ));
                if (!checkValidʗ5(tΔ8, err, testʗ2.valid)) {
                    return;
                }
                var got = ev.Range();
                if (got.Name != testʗ2.name) {
                    tΔ8.Errorf("expected name to be %q, got %q"u8, testʗ2.name, got.Name);
                }
                if (ev.Kind() != testʗ2.kind) {
                    tΔ8.Errorf("expected kind to be %s, got %s"u8, testʗ2.kind, ev.Kind());
                }
                if (got.Scope.String() != testʗ2.scope.String()) {
                    tΔ8.Errorf("expected scope to be %s, got %s"u8, testʗ2.scope.String(), got.Scope.String());
                }
                checkTimeʗ5(tΔ8, ev, (global::go.@internal.trace_package.ΔTime)(int64)(42 + i));
            });
        }
    });

    var checkStackʗ3 = checkStack;
    var checkTimeʗ6 = checkTime;
    var checkValidʗ6 = checkValid;
    var stk1ʗ2 = stk1;
    var stk2ʗ1 = stk2;
    Ꮡt.Run(goroutineTransitionˢ, (ж<testing.T> tΔ9) => {
        UntypedInt anotherG = 999; // indicates hat sched g is different from transition g
        var tests = new TestMakeEvent_testsᴛ2[]{
            new(
                name: "EvGoCreate"u8,
                g: anotherG,
                stack: stk1ʗ2,
                stG: 1,
                from: GoNotExist,
                to: GoRunnable,
                reason: ""u8,
                stStack: stk2ʗ1,
                valid: true
            ),
            new(
                name: "EvGoCreateBlocked"u8,
                g: anotherG,
                stack: stk1ʗ2,
                stG: 2,
                from: GoNotExist,
                to: GoWaiting,
                reason: ""u8,
                stStack: stk2ʗ1,
                valid: true
            ),
            new(
                name: "EvGoCreateSyscall"u8,
                g: anotherG,
                stack: NoStack,
                stG: 3,
                from: GoNotExist,
                to: GoSyscall,
                reason: ""u8,
                stStack: NoStack,
                valid: true
            ),
            new(
                name: "EvGoStart"u8,
                g: anotherG,
                stack: NoStack,
                stG: 4,
                from: GoRunnable,
                to: GoRunning,
                reason: ""u8,
                stStack: NoStack,
                valid: true
            ),
            new(
                name: "EvGoDestroy"u8,
                g: 5,
                stack: NoStack,
                stG: 5,
                from: GoRunning,
                to: GoNotExist,
                reason: ""u8,
                stStack: NoStack,
                valid: true
            ),
            new(
                name: "EvGoDestroySyscall"u8,
                g: 6,
                stack: NoStack,
                stG: 6,
                from: GoSyscall,
                to: GoNotExist,
                reason: ""u8,
                stStack: NoStack,
                valid: true
            ),
            new(
                name: "EvGoStop"u8,
                g: 7,
                stack: stk1ʗ2,
                stG: 7,
                from: GoRunning,
                to: GoRunnable,
                reason: "preempted"u8,
                stStack: stk1ʗ2,
                valid: true
            ),
            new(
                name: "EvGoBlock"u8,
                g: 8,
                stack: stk1ʗ2,
                stG: 8,
                from: GoRunning,
                to: GoWaiting,
                reason: "blocked"u8,
                stStack: stk1ʗ2,
                valid: true
            ),
            new(
                name: "EvGoUnblock"u8,
                g: 9,
                stack: stk1ʗ2,
                stG: anotherG,
                from: GoWaiting,
                to: GoRunnable,
                reason: ""u8,
                stStack: NoStack,
                valid: true
            ),
            // N.b. EvGoUnblock, EvGoSwitch and EvGoSwitchDestroy cannot be
            // distinguished from each other in Event form, so MakeEvent only
            // produces EvGoUnblock events for Waiting -> Runnable transitions.
            new(
                name: "EvGoSyscallBegin"u8,
                g: 10,
                stack: stk1ʗ2,
                stG: 10,
                from: GoRunning,
                to: GoSyscall,
                reason: ""u8,
                stStack: stk1ʗ2,
                valid: true
            ),
            new(
                name: "EvGoSyscallEnd"u8,
                g: 11,
                stack: NoStack,
                stG: 11,
                from: GoSyscall,
                to: GoRunning,
                reason: ""u8,
                stStack: NoStack,
                valid: true
            ),
            new(
                name: "EvGoSyscallEndBlocked"u8,
                g: 12,
                stack: NoStack,
                stG: 12,
                from: GoSyscall,
                to: GoRunnable,
                reason: ""u8,
                stStack: NoStack,
                valid: true
            ),
            // TODO(felixge): Use coverage testsing to check if we need all these GoStatus/GoStatusStack cases
            new(
                name: "GoStatus Undetermined->Waiting"u8,
                g: anotherG,
                stack: NoStack,
                stG: 13,
                from: GoUndetermined,
                to: GoWaiting,
                reason: ""u8,
                stStack: NoStack,
                valid: true
            ),
            new(
                name: "GoStatus Undetermined->Running"u8,
                g: anotherG,
                stack: NoStack,
                stG: 14,
                from: GoUndetermined,
                to: GoRunning,
                reason: ""u8,
                stStack: NoStack,
                valid: true
            ),
            new(
                name: "GoStatusStack Undetermined->Waiting"u8,
                g: anotherG,
                stack: stk1ʗ2,
                stG: 15,
                from: GoUndetermined,
                to: GoWaiting,
                reason: ""u8,
                stStack: stk1ʗ2,
                valid: true
            ),
            new(
                name: "GoStatusStack Undetermined->Runnable"u8,
                g: anotherG,
                stack: stk1ʗ2,
                stG: 16,
                from: GoUndetermined,
                to: GoRunnable,
                reason: ""u8,
                stStack: stk1ʗ2,
                valid: true
            ),
            new(
                name: "GoStatus Runnable->Runnable"u8,
                g: anotherG,
                stack: NoStack,
                stG: 17,
                from: GoRunnable,
                to: GoRunnable,
                reason: ""u8,
                stStack: NoStack,
                valid: true
            ),
            new(
                name: "GoStatus Runnable->Running"u8,
                g: anotherG,
                stack: NoStack,
                stG: 18,
                from: GoRunnable,
                to: GoRunning,
                reason: ""u8,
                stStack: NoStack,
                valid: true
            ),
            new(
                name: "invalid NotExits->NotExists"u8,
                g: anotherG,
                stack: stk1ʗ2,
                stG: 18,
                from: GoNotExist,
                to: GoNotExist,
                reason: ""u8,
                stStack: NoStack,
                valid: false
            ),
            new(
                name: "invalid Running->Undetermined"u8,
                g: anotherG,
                stack: stk1ʗ2,
                stG: 19,
                from: GoRunning,
                to: GoUndetermined,
                reason: ""u8,
                stStack: NoStack,
                valid: false
            )
        }.slice();

        foreach (var (i, vᴛ3) in tests) {
            ref var test = ref heap(new TestMakeEvent_testsᴛ2(), out var Ꮡtest);
            test = vᴛ3;

            var checkStackʗ4 = checkStackʗ3;
            var checkTimeʗ7 = checkTimeʗ6;
            var checkValidʗ7 = checkValidʗ6;
            var testʗ3 = test;
            tΔ9.Run(test.name, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ10) => {
                var st = MakeGoStateTransition(testʗ3.stG, testʗ3.from, testʗ3.to);
                st.Stack = testʗ3.stStack;
                st.Reason = testʗ3.reason;
                var (ev, err) = MakeEvent(new EventConfig<ΔStateTransition>(
                    Kind: EventStateTransition,
                    Time: ((global::go.@internal.trace_package.ΔTime)(int64)(42 + i)),
                    Goroutine: testʗ3.g,
                    Stack: testʗ3.stack,
                    Details: st
                ));
                if (!checkValidʗ7(tΔ10, err, testʗ3.valid)) {
                    return;
                }
                checkStackʗ4(tΔ10, ev.Stack(), testʗ3.stack, schedStack);
                if (ev.Goroutine() != testʗ3.g) {
                    tΔ10.Errorf("expected goroutine to be %d, got %d"u8, testʗ3.g, ev.Goroutine());
                }
                var got = ev.StateTransition();
                if (got.Resource.Goroutine() != testʗ3.stG) {
                    tΔ10.Errorf("expected resource to be %d, got %d"u8, testʗ3.stG, got.Resource.Goroutine());
                }
                var (from, to) = got.Goroutine();
                if (from != testʗ3.from) {
                    tΔ10.Errorf("from got=%s want=%s"u8, from, testʗ3.from);
                }
                if (to != testʗ3.to) {
                    tΔ10.Errorf("to got=%s want=%s"u8, to, testʗ3.to);
                }
                if (got.Reason != testʗ3.reason) {
                    tΔ10.Errorf("expected reason to be %s, got %s"u8, testʗ3.reason, got.Reason);
                }
                checkStackʗ4(tΔ10, got.Stack, testʗ3.stStack, stStack);
                checkTimeʗ7(tΔ10, ev, (global::go.@internal.trace_package.ΔTime)(int64)(42 + i));
            });
        }
    });

    var checkTimeʗ8 = checkTime;
    var checkValidʗ8 = checkValid;
    Ꮡt.Run(procTransitionˢ, (ж<testing.T> tΔ11) => {
        var tests = new TestMakeEvent_testsᴛ3[]{
            new(name: "ProcStart"u8, proc: 1, schedProc: 99, from: ProcIdle, to: ProcRunning, valid: true),
            new(name: "ProcStop"u8, proc: 2, schedProc: 2, from: ProcRunning, to: ProcIdle, valid: true),
            new(name: "ProcSteal"u8, proc: 3, schedProc: 99, from: ProcRunning, to: ProcIdle, valid: true),
            new(name: "ProcSteal lost info"u8, proc: 4, schedProc: 99, from: ProcIdle, to: ProcIdle, valid: true),
            new(name: "ProcStatus"u8, proc: 5, schedProc: 99, from: ProcUndetermined, to: ProcRunning, valid: true)
        }.slice();
        foreach (var (i, vᴛ4) in tests) {
            ref var test = ref heap(new TestMakeEvent_testsᴛ3(), out var Ꮡtest);
            test = vᴛ4;

            var checkTimeʗ9 = checkTimeʗ8;
            var checkValidʗ9 = checkValidʗ8;
            var testʗ4 = test;
            tΔ11.Run(test.name, (ж<testing.T> tΔ12) => {
                var st = MakeProcStateTransition(testʗ4.proc, testʗ4.from, testʗ4.to);
                var (ev, err) = MakeEvent(new EventConfig<ΔStateTransition>(
                    Kind: EventStateTransition,
                    Time: ((global::go.@internal.trace_package.ΔTime)(int64)(42 + i)),
                    Proc: testʗ4.schedProc,
                    Details: st
                ));
                if (!checkValidʗ9(tΔ12, err, testʗ4.valid)) {
                    return;
                }
                checkTimeʗ9(tΔ12, ev, (global::go.@internal.trace_package.ΔTime)(int64)(42 + i));
                var gotSt = ev.StateTransition();
                var (from, to) = gotSt.Proc();
                if (from != testʗ4.from) {
                    tΔ12.Errorf("from got=%s want=%s"u8, from, testʗ4.from);
                }
                if (to != testʗ4.to) {
                    tΔ12.Errorf("to got=%s want=%s"u8, to, testʗ4.to);
                }
                if (ev.Proc() != testʗ4.schedProc) {
                    tΔ12.Errorf("expected proc to be %d, got %d"u8, testʗ4.schedProc, ev.Proc());
                }
                if (gotSt.Resource.Proc() != testʗ4.proc) {
                    tΔ12.Errorf("expected resource to be %d, got %d"u8, testʗ4.proc, gotSt.Resource.Proc());
                }
            });
        }
    });

    var checkTimeʗ10 = checkTime;
    var checkValidʗ10 = checkValid;
    Ꮡt.Run(syncˢ2, (ж<testing.T> tΔ13) => {
        var tests = new TestMakeEvent_testsᴛ4[]{
            new(
                name: "invalid kind"u8,
                n: 1,
                valid: false
            ),
            new(
                name: "N"u8,
                kind: EventSync,
                n: 1,
                batches: new map<@string, slice<global::go.@internal.trace_package.ExperimentalBatch>>{},
                valid: true
            ),
            new(
                name: "N+ClockSnapshot"u8,
                kind: EventSync,
                n: 1,
                batches: new map<@string, slice<global::go.@internal.trace_package.ExperimentalBatch>>{},
                clock: Ꮡ(new ClockSnapshot(
                    Trace: 1,
                    Wall: time.Unix(59, 123456789),
                    Mono: 2
                )),
                valid: true
            ),
            new(
                name: "N+Batches"u8,
                kind: EventSync,
                n: 1,
                batches: new map<@string, slice<global::go.@internal.trace_package.ExperimentalBatch>>{
                    ["AllocFree"u8] = new global::go.@internal.trace_package.ExperimentalBatch[]{new(Thread: 1, Data: new byte[]{1, 2, 3}.slice())}.slice()
                },
                valid: true
            ),
            new(
                name: "unknown experiment"u8,
                kind: EventSync,
                n: 1,
                batches: new map<@string, slice<global::go.@internal.trace_package.ExperimentalBatch>>{
                    ["does-not-exist"u8] = new global::go.@internal.trace_package.ExperimentalBatch[]{new(Thread: 1, Data: new byte[]{1, 2, 3}.slice())}.slice()
                },
                valid: false
            )
        }.slice();
        foreach (var (i, vᴛ5) in tests) {
            ref var test = ref heap(new TestMakeEvent_testsᴛ4(), out var Ꮡtest);
            test = vᴛ5;

            var checkTimeʗ11 = checkTimeʗ10;
            var checkValidʗ11 = checkValidʗ10;
            var testʗ5 = test;
            tΔ13.Run(test.name, (ж<testing.T> tΔ14) => {
                var (ev, err) = MakeEvent(new EventConfig<ΔSync>(
                    Kind: testʗ5.kind,
                    Time: ((global::go.@internal.trace_package.ΔTime)(int64)(42 + i)),
                    Details: new ΔSync(N: testʗ5.n, ClockSnapshot: testʗ5.clock, ExperimentalBatches: testʗ5.batches)
                ));
                if (!checkValidʗ11(tΔ14, err, testʗ5.valid)) {
                    return;
                }
                var got = ev.Sync();
                checkTimeʗ11(tΔ14, ev, (global::go.@internal.trace_package.ΔTime)(int64)(42 + i));
                if (got.N != testʗ5.n) {
                    tΔ14.Errorf("expected N to be %d, got %d"u8, testʗ5.n, got.N);
                }
                if (testʗ5.clock != nil && got.ClockSnapshot == nil){
                    tΔ14.Fatalf("expected ClockSnapshot to be non-nil"u8);
                } else 
                if (testʗ5.clock == nil && got.ClockSnapshot != nil){
                    tΔ14.Fatalf("expected ClockSnapshot to be nil"u8);
                } else 
                if (testʗ5.clock != nil && got.ClockSnapshot != nil) {
                    if ((~got.ClockSnapshot).Trace != (~testʗ5.clock).Trace) {
                        tΔ14.Errorf("expected ClockSnapshot.Trace to be %d, got %d"u8, (~testʗ5.clock).Trace, (~got.ClockSnapshot).Trace);
                    }
                    if (!(~got.ClockSnapshot).Wall.Equal((~testʗ5.clock).Wall)) {
                        tΔ14.Errorf("expected ClockSnapshot.Wall to be %s, got %s"u8, (~testʗ5.clock).Wall, (~got.ClockSnapshot).Wall);
                    }
                    if ((~got.ClockSnapshot).Mono != (~testʗ5.clock).Mono) {
                        tΔ14.Errorf("expected ClockSnapshot.Mono to be %d, got %d"u8, (~testʗ5.clock).Mono, (~got.ClockSnapshot).Mono);
                    }
                }
                if (!reflect.DeepEqual(got.ExperimentalBatches, testʗ5.batches)) {
                    tΔ14.Errorf("expected ExperimentalBatches to be %#v, got %#v"u8, testʗ5.batches, got.ExperimentalBatches);
                }
            });
        }
    });

    var checkTimeʗ12 = checkTime;
    var checkValidʗ12 = checkValid;
    Ꮡt.Run(taskˢ, (ж<testing.T> tΔ15) => {
        var tests = new TestMakeEvent_testsᴛ5[]{
            new(name: "no task"u8, kind: EventTaskBegin, id: NoTask, parent: 1, typ: "type-0"u8, valid: false),
            new(name: "invalid kind"u8, kind: EventMetric, id: 1, parent: 2, typ: "type-1"u8, valid: false),
            new(name: "EvUserTaskBegin"u8, kind: EventTaskBegin, id: 2, parent: 3, typ: "type-2"u8, valid: true),
            new(name: "EvUserTaskEnd"u8, kind: EventTaskEnd, id: 3, parent: 4, typ: "type-3"u8, valid: true),
            new(name: "no parent"u8, kind: EventTaskBegin, id: 4, parent: NoTask, typ: "type-4"u8, valid: true)
        }.slice();

        foreach (var (i, vᴛ6) in tests) {
            ref var test = ref heap(new TestMakeEvent_testsᴛ5(), out var Ꮡtest);
            test = vᴛ6;

            var checkTimeʗ13 = checkTimeʗ12;
            var checkValidʗ13 = checkValidʗ12;
            var testʗ6 = test;
            tΔ15.Run(test.name, (ж<testing.T> tΔ16) => {
                var (ev, err) = MakeEvent(new EventConfig<ΔTask>(
                    Kind: testʗ6.kind,
                    Time: ((global::go.@internal.trace_package.ΔTime)(int64)(42 + i)),
                    Details: new ΔTask(ID: testʗ6.id, Parent: testʗ6.parent, Type: testʗ6.typ)
                ));
                if (!checkValidʗ13(tΔ16, err, testʗ6.valid)) {
                    return;
                }
                checkTimeʗ13(tΔ16, ev, (global::go.@internal.trace_package.ΔTime)(int64)(42 + i));
                var got = ev.Task();
                if (got.ID != testʗ6.id) {
                    tΔ16.Errorf("expected ID to be %d, got %d"u8, testʗ6.id, got.ID);
                }
                if (got.Parent != testʗ6.parent) {
                    tΔ16.Errorf("expected Parent to be %d, got %d"u8, testʗ6.parent, got.Parent);
                }
                if (got.Type != testʗ6.typ) {
                    tΔ16.Errorf("expected Type to be %s, got %s"u8, testʗ6.typ, got.Type);
                }
            });
        }
    });

    var checkTimeʗ14 = checkTime;
    var checkValidʗ14 = checkValid;
    Ꮡt.Run(regionˢ, (ж<testing.T> tΔ17) => {
        var tests = new TestMakeEvent_testsᴛ6[]{
            new(name: "invalid kind"u8, kind: EventMetric, task: 1, typ: "type-1"u8, valid: false),
            new(name: "EvUserRegionBegin"u8, kind: EventRegionBegin, task: 2, typ: "type-2"u8, valid: true),
            new(name: "EvUserRegionEnd"u8, kind: EventRegionEnd, task: 3, typ: "type-3"u8, valid: true)
        }.slice();

        foreach (var (i, vᴛ7) in tests) {
            ref var test = ref heap(new TestMakeEvent_testsᴛ6(), out var Ꮡtest);
            test = vᴛ7;

            var checkTimeʗ15 = checkTimeʗ14;
            var checkValidʗ15 = checkValidʗ14;
            var testʗ7 = test;
            tΔ17.Run(test.name, (ж<testing.T> tΔ18) => {
                var (ev, err) = MakeEvent(new EventConfig<ΔRegion>(
                    Kind: testʗ7.kind,
                    Time: ((global::go.@internal.trace_package.ΔTime)(int64)(42 + i)),
                    Details: new ΔRegion(Task: testʗ7.task, Type: testʗ7.typ)
                ));
                if (!checkValidʗ15(tΔ18, err, testʗ7.valid)) {
                    return;
                }
                checkTimeʗ15(tΔ18, ev, (global::go.@internal.trace_package.ΔTime)(int64)(42 + i));
                var got = ev.Region();
                if (got.Task != testʗ7.task) {
                    tΔ18.Errorf("expected Task to be %d, got %d"u8, testʗ7.task, got.Task);
                }
                if (got.Type != testʗ7.typ) {
                    tΔ18.Errorf("expected Type to be %s, got %s"u8, testʗ7.typ, got.Type);
                }
            });
        }
    });

    var checkTimeʗ16 = checkTime;
    var checkValidʗ16 = checkValid;
    Ꮡt.Run(logˢ, (ж<testing.T> tΔ19) => {
        var tests = new TestMakeEvent_testsᴛ7[]{
            new(name: "invalid kind"u8, kind: EventMetric, task: 1, category: "category-1"u8, message: "message-1"u8, valid: false),
            new(name: "basic"u8, kind: EventLog, task: 2, category: "category-2"u8, message: "message-2"u8, valid: true)
        }.slice();

        foreach (var (i, vᴛ8) in tests) {
            ref var test = ref heap(new TestMakeEvent_testsᴛ7(), out var Ꮡtest);
            test = vᴛ8;

            var checkTimeʗ17 = checkTimeʗ16;
            var checkValidʗ17 = checkValidʗ16;
            var testʗ8 = test;
            tΔ19.Run(test.name, (ж<testing.T> tΔ20) => {
                var (ev, err) = MakeEvent(new EventConfig<ΔLog>(
                    Kind: testʗ8.kind,
                    Time: ((global::go.@internal.trace_package.ΔTime)(int64)(42 + i)),
                    Details: new ΔLog(Task: testʗ8.task, Category: testʗ8.category, Message: testʗ8.message)
                ));
                if (!checkValidʗ17(tΔ20, err, testʗ8.valid)) {
                    return;
                }
                checkTimeʗ17(tΔ20, ev, (global::go.@internal.trace_package.ΔTime)(int64)(42 + i));
                var got = ev.Log();
                if (got.Task != testʗ8.task) {
                    tΔ20.Errorf("expected Task to be %d, got %d"u8, testʗ8.task, got.Task);
                }
                if (got.Category != testʗ8.category) {
                    tΔ20.Errorf("expected Category to be %s, got %s"u8, testʗ8.category, got.Category);
                }
                if (got.Message != testʗ8.message) {
                    tΔ20.Errorf("expected Message to be %s, got %s"u8, testʗ8.message, got.Message);
                }
            });
        }
    });

    var checkStackʗ5 = checkStack;
    var checkTimeʗ18 = checkTime;
    var checkValidʗ18 = checkValid;
    var stk1ʗ3 = stk1;
    Ꮡt.Run(stackSampleˢ, (ж<testing.T> tΔ21) => {
        var tests = new TestMakeEvent_testsᴛ8[]{
            new(name: "invalid kind"u8, kind: EventMetric, stack: stk1ʗ3, valid: false),
            new(name: "basic"u8, kind: EventStackSample, stack: stk1ʗ3, valid: true)
        }.slice();

        foreach (var (i, vᴛ9) in tests) {
            ref var test = ref heap(new TestMakeEvent_testsᴛ8(), out var Ꮡtest);
            test = vᴛ9;

            var checkStackʗ6 = checkStackʗ5;
            var checkTimeʗ19 = checkTimeʗ18;
            var checkValidʗ19 = checkValidʗ18;
            var testʗ9 = test;
            tΔ21.Run(test.name, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ22) => {
                var (ev, err) = MakeEvent(new EventConfig<StackSample>(
                    Kind: testʗ9.kind,
                    Time: ((global::go.@internal.trace_package.ΔTime)(int64)(42 + i)),
                    Stack: testʗ9.stack
                    // N.b. Details defaults to StackSample{}, so we can
                    // omit it here.
                ));
                if (!checkValidʗ19(tΔ22, err, testʗ9.valid)) {
                    return;
                }
                checkTimeʗ19(tΔ22, ev, (global::go.@internal.trace_package.ΔTime)(int64)(42 + i));
                var got = ev.Stack();
                checkStackʗ6(tΔ22, got, testʗ9.stack, schedStack);
            });
        }
    });
}

public static void TestMakeStack(ж<testing.T> Ꮡt) {
    var frames = new global::go.@internal.trace_package.StackFrame[]{
        new(PC: 1, Func: "foo"u8, File: "foo.go"u8, Line: 10),
        new(PC: 2, Func: "bar"u8, File: "bar.go"u8, Line: 20)
    }.slice();
    var got = slices.Collect(MakeStack(frames).Frames());
    if (len(got) != len(frames)) {
        Ꮡt.Errorf("got=%d want=%d"u8, len(got), len(frames));
    }
    foreach (var i in range(got)) {
        if (got[i] != frames[i]) {
            Ꮡt.Errorf("got=%v want=%v"u8, got[i], frames[i]);
        }
    }
}

public static void TestPanicEvent(ж<testing.T> Ꮡt) {
    // Use a sync event for this because it doesn't have any extra metadata.
    ref var ev = ref heap<global::go.@internal.trace_package.ΔEvent>(out var Ꮡev);
    ev = syncEvent(nil, 0, 0);

    var evʗ1 = ev;
    mustPanic(Ꮡt, () => {
        _ = evʗ1.Range();
    });
    var evʗ2 = ev;
    mustPanic(Ꮡt, () => {
        _ = evʗ2.Metric();
    });
    var evʗ3 = ev;
    mustPanic(Ꮡt, () => {
        _ = evʗ3.Log();
    });
    var evʗ4 = ev;
    mustPanic(Ꮡt, () => {
        _ = evʗ4.Task();
    });
    var evʗ5 = ev;
    mustPanic(Ꮡt, () => {
        _ = evʗ5.Region();
    });
    var evʗ6 = ev;
    mustPanic(Ꮡt, () => {
        _ = evʗ6.Label();
    });
    var evʗ7 = ev;
    mustPanic(Ꮡt, () => {
        _ = evʗ7.RangeAttributes();
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object failedToPanicˢ = (@string)"failed to panic"u8;

internal static void mustPanic(ж<testing.T> Ꮡt, Action f) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var r = recover(); if (r == default!) {
                    Ꮡt.Fatal(failedToPanicˢ);
                }
            }
        }, ref ᒐ);
        f();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end trace_internal_test_package
