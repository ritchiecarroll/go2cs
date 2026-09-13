// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

// Metrics implementation exported to runtime/metrics.
using godebugs = @internal.godebugs_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;
using ꓸꓸꓸstatDep = Span<runtime_package.statDep>;

partial class runtime_package {

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸinternalꓸgodebugs() {
    builtin.initPackage(typeof(@internal.godebugs_package));
}

internal static ж<uint32> ᏑmetricsSema = new StandardBox<uint32>(1);
internal static ref uint32 metricsSema => ref ᏑmetricsSema.Value;
internal static bool metricsInit;
internal static map<@string, metricData> metrics;
internal static slice<float64> sizeClassBuckets;
internal static slice<float64> timeHistBuckets;

[GoType] partial struct metricData {
    // deps is the set of runtime statistics that this metric
    // depends on. Before compute is called, the statAggregate
    // which will be passed must ensure() these dependencies.
    internal statDepSet deps;
    // compute is a function that populates a metricValue
    // given a populated statAggregate structure.
    internal Action<ж<statAggregate>, ж<metricValue>> compute;
}

// go2cs generated this placeholder — func metricsLock is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func metricsUnlock is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// Acquire the metricsSema but with handoff. Operations are typically
// expensive enough that queueing up goroutines and handing off between
// them will be noticeably better-behaved.

// initMetrics initializes the metrics map if it hasn't been yet.
//
// metricsSema must be held.
internal static void initMetrics() {
    if (metricsInit) {
        return;
    }
    sizeClassBuckets = new slice<float64>(_NumSizeClasses, _NumSizeClasses + 1);
    // Skip size class 0 which is a stand-in for large objects, but large
    // objects are tracked separately (and they actually get placed in
    // the last bucket, not the first).
    sizeClassBuckets[0] = 1D; // The smallest allocation is 1 byte in size.
    for (nint i = 1; i < _NumSizeClasses; i++) {
        // Size classes have an inclusive upper-bound
        // and exclusive lower bound (e.g. 48-byte size class is
        // (32, 48]) whereas we want and inclusive lower-bound
        // and exclusive upper-bound (e.g. 48-byte size class is
        // [33, 49)). We can achieve this by shifting all bucket
        // boundaries up by 1.
        //
        // Also, a float64 can precisely represent integers with
        // value up to 2^53 and size classes are relatively small
        // (nowhere near 2^48 even) so this will give us exact
        // boundaries.
        sizeClassBuckets[i] = (float64)(class_to_size[i] + 1);
    }
    sizeClassBuckets = append(sizeClassBuckets, float64Inf());
    timeHistBuckets = timeHistogramMetricsBuckets();
    metrics = new map<@string, metricData>{
        ["/cgo/go-to-c-calls:calls"u8] = new(
            compute: (ж<statAggregate> _, ж<metricValue> @out) => {
                @out.Value.kind = metricKindUint64;
                @out.Value.scalar = (uint64)NumCgoCall();
            }
        ),
        ["/cpu/classes/gc/mark/assist:cpu-seconds"u8] = new(
            deps: makeStatDepSet(cpuStatsDep),
            compute: (ж<statAggregate> @in, ж<metricValue> @out) => {
                @out.Value.kind = metricKindFloat64;
                @out.Value.scalar = float64bits(nsToSec((~@in).cpuStats.GCAssistTime));
            }
        ),
        ["/cpu/classes/gc/mark/dedicated:cpu-seconds"u8] = new(
            deps: makeStatDepSet(cpuStatsDep),
            compute: (ж<statAggregate> @in, ж<metricValue> @out) => {
                @out.Value.kind = metricKindFloat64;
                @out.Value.scalar = float64bits(nsToSec((~@in).cpuStats.GCDedicatedTime));
            }
        ),
        ["/cpu/classes/gc/mark/idle:cpu-seconds"u8] = new(
            deps: makeStatDepSet(cpuStatsDep),
            compute: (ж<statAggregate> @in, ж<metricValue> @out) => {
                @out.Value.kind = metricKindFloat64;
                @out.Value.scalar = float64bits(nsToSec((~@in).cpuStats.GCIdleTime));
            }
        ),
        ["/cpu/classes/gc/pause:cpu-seconds"u8] = new(
            deps: makeStatDepSet(cpuStatsDep),
            compute: (ж<statAggregate> @in, ж<metricValue> @out) => {
                @out.Value.kind = metricKindFloat64;
                @out.Value.scalar = float64bits(nsToSec((~@in).cpuStats.GCPauseTime));
            }
        ),
        ["/cpu/classes/gc/total:cpu-seconds"u8] = new(
            deps: makeStatDepSet(cpuStatsDep),
            compute: (ж<statAggregate> @in, ж<metricValue> @out) => {
                @out.Value.kind = metricKindFloat64;
                @out.Value.scalar = float64bits(nsToSec((~@in).cpuStats.GCTotalTime));
            }
        ),
        ["/cpu/classes/idle:cpu-seconds"u8] = new(
            deps: makeStatDepSet(cpuStatsDep),
            compute: (ж<statAggregate> @in, ж<metricValue> @out) => {
                @out.Value.kind = metricKindFloat64;
                @out.Value.scalar = float64bits(nsToSec((~@in).cpuStats.IdleTime));
            }
        ),
        ["/cpu/classes/scavenge/assist:cpu-seconds"u8] = new(
            deps: makeStatDepSet(cpuStatsDep),
            compute: (ж<statAggregate> @in, ж<metricValue> @out) => {
                @out.Value.kind = metricKindFloat64;
                @out.Value.scalar = float64bits(nsToSec((~@in).cpuStats.ScavengeAssistTime));
            }
        ),
        ["/cpu/classes/scavenge/background:cpu-seconds"u8] = new(
            deps: makeStatDepSet(cpuStatsDep),
            compute: (ж<statAggregate> @in, ж<metricValue> @out) => {
                @out.Value.kind = metricKindFloat64;
                @out.Value.scalar = float64bits(nsToSec((~@in).cpuStats.ScavengeBgTime));
            }
        ),
        ["/cpu/classes/scavenge/total:cpu-seconds"u8] = new(
            deps: makeStatDepSet(cpuStatsDep),
            compute: (ж<statAggregate> @in, ж<metricValue> @out) => {
                @out.Value.kind = metricKindFloat64;
                @out.Value.scalar = float64bits(nsToSec((~@in).cpuStats.ScavengeTotalTime));
            }
        ),
        ["/cpu/classes/total:cpu-seconds"u8] = new(
            deps: makeStatDepSet(cpuStatsDep),
            compute: (ж<statAggregate> @in, ж<metricValue> @out) => {
                @out.Value.kind = metricKindFloat64;
                @out.Value.scalar = float64bits(nsToSec((~@in).cpuStats.TotalTime));
            }
        ),
        ["/cpu/classes/user:cpu-seconds"u8] = new(
            deps: makeStatDepSet(cpuStatsDep),
            compute: (ж<statAggregate> @in, ж<metricValue> @out) => {
                @out.Value.kind = metricKindFloat64;
                @out.Value.scalar = float64bits(nsToSec((~@in).cpuStats.UserTime));
            }
        ),
        ["/gc/cycles/automatic:gc-cycles"u8] = new(
            deps: makeStatDepSet(sysStatsDep),
            compute: (ж<statAggregate> @in, ж<metricValue> @out) => {
                @out.Value.kind = metricKindUint64;
                @out.Value.scalar = (~@in).sysStats.gcCyclesDone - (~@in).sysStats.gcCyclesForced;
            }
        ),
        ["/gc/cycles/forced:gc-cycles"u8] = new(
            deps: makeStatDepSet(sysStatsDep),
            compute: (ж<statAggregate> @in, ж<metricValue> @out) => {
                @out.Value.kind = metricKindUint64;
                @out.Value.scalar = @in.Value.sysStats.gcCyclesForced;
            }
        ),
        ["/gc/cycles/total:gc-cycles"u8] = new(
            deps: makeStatDepSet(sysStatsDep),
            compute: (ж<statAggregate> @in, ж<metricValue> @out) => {
                @out.Value.kind = metricKindUint64;
                @out.Value.scalar = @in.Value.sysStats.gcCyclesDone;
            }
        ),
        ["/gc/scan/globals:bytes"u8] = new(
            deps: makeStatDepSet(gcStatsDep),
            compute: (ж<statAggregate> @in, ж<metricValue> @out) => {
                @out.Value.kind = metricKindUint64;
                @out.Value.scalar = @in.Value.gcStats.globalsScan;
            }
        ),
        ["/gc/scan/heap:bytes"u8] = new(
            deps: makeStatDepSet(gcStatsDep),
            compute: (ж<statAggregate> @in, ж<metricValue> @out) => {
                @out.Value.kind = metricKindUint64;
                @out.Value.scalar = @in.Value.gcStats.heapScan;
            }
        ),
        ["/gc/scan/stack:bytes"u8] = new(
            deps: makeStatDepSet(gcStatsDep),
            compute: (ж<statAggregate> @in, ж<metricValue> @out) => {
                @out.Value.kind = metricKindUint64;
                @out.Value.scalar = @in.Value.gcStats.stackScan;
            }
        ),
        ["/gc/scan/total:bytes"u8] = new(
            deps: makeStatDepSet(gcStatsDep),
            compute: (ж<statAggregate> @in, ж<metricValue> @out) => {
                @out.Value.kind = metricKindUint64;
                @out.Value.scalar = @in.Value.gcStats.totalScan;
            }
        ),
        ["/gc/heap/allocs-by-size:bytes"u8] = new(
            deps: makeStatDepSet(heapStatsDep),
            compute: (ж<statAggregate> @in, ж<metricValue> @out) => {
                var hist = @out.float64HistOrInit(sizeClassBuckets);
                hist.Value.counts[len((~hist).counts) - 1] = @in.Value.heapStats.largeAllocCount;
                // Cut off the first index which is ostensibly for size class 0,
                // but large objects are tracked separately so it's actually unused.
                foreach (var (i, count) in (~@in).heapStats.smallAllocCount[1..]) {
                    hist.Value.counts[i] = count;
                }
            }
        ),
        ["/gc/heap/allocs:bytes"u8] = new(
            deps: makeStatDepSet(heapStatsDep),
            compute: (ж<statAggregate> @in, ж<metricValue> @out) => {
                @out.Value.kind = metricKindUint64;
                @out.Value.scalar = @in.Value.heapStats.totalAllocated;
            }
        ),
        ["/gc/heap/allocs:objects"u8] = new(
            deps: makeStatDepSet(heapStatsDep),
            compute: (ж<statAggregate> @in, ж<metricValue> @out) => {
                @out.Value.kind = metricKindUint64;
                @out.Value.scalar = @in.Value.heapStats.totalAllocs;
            }
        ),
        ["/gc/heap/frees-by-size:bytes"u8] = new(
            deps: makeStatDepSet(heapStatsDep),
            compute: (ж<statAggregate> @in, ж<metricValue> @out) => {
                var hist = @out.float64HistOrInit(sizeClassBuckets);
                hist.Value.counts[len((~hist).counts) - 1] = @in.Value.heapStats.largeFreeCount;
                // Cut off the first index which is ostensibly for size class 0,
                // but large objects are tracked separately so it's actually unused.
                foreach (var (i, count) in (~@in).heapStats.smallFreeCount[1..]) {
                    hist.Value.counts[i] = count;
                }
            }
        ),
        ["/gc/heap/frees:bytes"u8] = new(
            deps: makeStatDepSet(heapStatsDep),
            compute: (ж<statAggregate> @in, ж<metricValue> @out) => {
                @out.Value.kind = metricKindUint64;
                @out.Value.scalar = @in.Value.heapStats.totalFreed;
            }
        ),
        ["/gc/heap/frees:objects"u8] = new(
            deps: makeStatDepSet(heapStatsDep),
            compute: (ж<statAggregate> @in, ж<metricValue> @out) => {
                @out.Value.kind = metricKindUint64;
                @out.Value.scalar = @in.Value.heapStats.totalFrees;
            }
        ),
        ["/gc/heap/goal:bytes"u8] = new(
            deps: makeStatDepSet(sysStatsDep),
            compute: (ж<statAggregate> @in, ж<metricValue> @out) => {
                @out.Value.kind = metricKindUint64;
                @out.Value.scalar = @in.Value.sysStats.heapGoal;
            }
        ),
        ["/gc/gomemlimit:bytes"u8] = new(
            compute: (ж<statAggregate> @in, ж<metricValue> @out) => {
                @out.Value.kind = metricKindUint64;
                @out.Value.scalar = (uint64)ᏑgcController.of(gcControllerState.ᏑmemoryLimit).Load();
            }
        ),
        ["/gc/gogc:percent"u8] = new(
            compute: (ж<statAggregate> @in, ж<metricValue> @out) => {
                @out.Value.kind = metricKindUint64;
                @out.Value.scalar = (uint64)ᏑgcController.of(gcControllerState.ᏑgcPercent).Load();
            }
        ),
        ["/gc/heap/live:bytes"u8] = new(
            deps: makeStatDepSet(heapStatsDep),
            compute: (ж<statAggregate> @in, ж<metricValue> @out) => {
                @out.Value.kind = metricKindUint64;
                @out.Value.scalar = gcController.heapMarked;
            }
        ),
        ["/gc/heap/objects:objects"u8] = new(
            deps: makeStatDepSet(heapStatsDep),
            compute: (ж<statAggregate> @in, ж<metricValue> @out) => {
                @out.Value.kind = metricKindUint64;
                @out.Value.scalar = @in.Value.heapStats.numObjects;
            }
        ),
        ["/gc/heap/tiny/allocs:objects"u8] = new(
            deps: makeStatDepSet(heapStatsDep),
            compute: (ж<statAggregate> @in, ж<metricValue> @out) => {
                @out.Value.kind = metricKindUint64;
                @out.Value.scalar = @in.Value.heapStats.tinyAllocCount;
            }
        ),
        ["/gc/limiter/last-enabled:gc-cycle"u8] = new(
            compute: (ж<statAggregate> _, ж<metricValue> @out) => {
                @out.Value.kind = metricKindUint64;
                @out.Value.scalar = (uint64)ᏑgcCPULimiter.of(gcCPULimiterState.ᏑlastEnabledCycle).Load();
            }
        ),
        ["/gc/pauses:seconds"u8] = new(
            compute: (ж<statAggregate> _, ж<metricValue> @out) => {
                // N.B. this is identical to /sched/pauses/total/gc:seconds.
                Ꮡsched.of(schedt.ᏑstwTotalTimeGC).write(@out);
            }
        ),
        ["/gc/stack/starting-size:bytes"u8] = new(
            compute: (ж<statAggregate> @in, ж<metricValue> @out) => {
                @out.Value.kind = metricKindUint64;
                @out.Value.scalar = (uint64)startingStackSize;
            }
        ),
        ["/memory/classes/heap/free:bytes"u8] = new(
            deps: makeStatDepSet(heapStatsDep),
            compute: (ж<statAggregate> @in, ж<metricValue> @out) => {
                @out.Value.kind = metricKindUint64;
                @out.Value.scalar = (uint64)((~@in).heapStats.committed - (~@in).heapStats.inHeap - (~@in).heapStats.inStacks - (~@in).heapStats.inWorkBufs - (~@in).heapStats.inPtrScalarBits);
            }
        ),
        ["/memory/classes/heap/objects:bytes"u8] = new(
            deps: makeStatDepSet(heapStatsDep),
            compute: (ж<statAggregate> @in, ж<metricValue> @out) => {
                @out.Value.kind = metricKindUint64;
                @out.Value.scalar = @in.Value.heapStats.inObjects;
            }
        ),
        ["/memory/classes/heap/released:bytes"u8] = new(
            deps: makeStatDepSet(heapStatsDep),
            compute: (ж<statAggregate> @in, ж<metricValue> @out) => {
                @out.Value.kind = metricKindUint64;
                @out.Value.scalar = (uint64)(~@in).heapStats.released;
            }
        ),
        ["/memory/classes/heap/stacks:bytes"u8] = new(
            deps: makeStatDepSet(heapStatsDep),
            compute: (ж<statAggregate> @in, ж<metricValue> @out) => {
                @out.Value.kind = metricKindUint64;
                @out.Value.scalar = (uint64)(~@in).heapStats.inStacks;
            }
        ),
        ["/memory/classes/heap/unused:bytes"u8] = new(
            deps: makeStatDepSet(heapStatsDep),
            compute: (ж<statAggregate> @in, ж<metricValue> @out) => {
                @out.Value.kind = metricKindUint64;
                @out.Value.scalar = (uint64)(~@in).heapStats.inHeap - (~@in).heapStats.inObjects;
            }
        ),
        ["/memory/classes/metadata/mcache/free:bytes"u8] = new(
            deps: makeStatDepSet(sysStatsDep),
            compute: (ж<statAggregate> @in, ж<metricValue> @out) => {
                @out.Value.kind = metricKindUint64;
                @out.Value.scalar = (~@in).sysStats.mCacheSys - (~@in).sysStats.mCacheInUse;
            }
        ),
        ["/memory/classes/metadata/mcache/inuse:bytes"u8] = new(
            deps: makeStatDepSet(sysStatsDep),
            compute: (ж<statAggregate> @in, ж<metricValue> @out) => {
                @out.Value.kind = metricKindUint64;
                @out.Value.scalar = @in.Value.sysStats.mCacheInUse;
            }
        ),
        ["/memory/classes/metadata/mspan/free:bytes"u8] = new(
            deps: makeStatDepSet(sysStatsDep),
            compute: (ж<statAggregate> @in, ж<metricValue> @out) => {
                @out.Value.kind = metricKindUint64;
                @out.Value.scalar = (~@in).sysStats.mSpanSys - (~@in).sysStats.mSpanInUse;
            }
        ),
        ["/memory/classes/metadata/mspan/inuse:bytes"u8] = new(
            deps: makeStatDepSet(sysStatsDep),
            compute: (ж<statAggregate> @in, ж<metricValue> @out) => {
                @out.Value.kind = metricKindUint64;
                @out.Value.scalar = @in.Value.sysStats.mSpanInUse;
            }
        ),
        ["/memory/classes/metadata/other:bytes"u8] = new(
            deps: makeStatDepSet(heapStatsDep, sysStatsDep),
            compute: (ж<statAggregate> @in, ж<metricValue> @out) => {
                @out.Value.kind = metricKindUint64;
                @out.Value.scalar = (uint64)((~@in).heapStats.inWorkBufs + (~@in).heapStats.inPtrScalarBits) + (~@in).sysStats.gcMiscSys;
            }
        ),
        ["/memory/classes/os-stacks:bytes"u8] = new(
            deps: makeStatDepSet(sysStatsDep),
            compute: (ж<statAggregate> @in, ж<metricValue> @out) => {
                @out.Value.kind = metricKindUint64;
                @out.Value.scalar = @in.Value.sysStats.stacksSys;
            }
        ),
        ["/memory/classes/other:bytes"u8] = new(
            deps: makeStatDepSet(sysStatsDep),
            compute: (ж<statAggregate> @in, ж<metricValue> @out) => {
                @out.Value.kind = metricKindUint64;
                @out.Value.scalar = @in.Value.sysStats.otherSys;
            }
        ),
        ["/memory/classes/profiling/buckets:bytes"u8] = new(
            deps: makeStatDepSet(sysStatsDep),
            compute: (ж<statAggregate> @in, ж<metricValue> @out) => {
                @out.Value.kind = metricKindUint64;
                @out.Value.scalar = @in.Value.sysStats.buckHashSys;
            }
        ),
        ["/memory/classes/total:bytes"u8] = new(
            deps: makeStatDepSet(heapStatsDep, sysStatsDep),
            compute: (ж<statAggregate> @in, ж<metricValue> @out) => {
                @out.Value.kind = metricKindUint64;
                @out.Value.scalar = (uint64)((~@in).heapStats.committed + (~@in).heapStats.released) + (~@in).sysStats.stacksSys + (~@in).sysStats.mSpanSys + (~@in).sysStats.mCacheSys + (~@in).sysStats.buckHashSys + (~@in).sysStats.gcMiscSys + (~@in).sysStats.otherSys;
            }
        ),
        ["/sched/gomaxprocs:threads"u8] = new(
            compute: (ж<statAggregate> _, ж<metricValue> @out) => {
                @out.Value.kind = metricKindUint64;
                @out.Value.scalar = (uint64)gomaxprocs;
            }
        ),
        ["/sched/goroutines:goroutines"u8] = new(
            compute: (ж<statAggregate> _, ж<metricValue> @out) => {
                @out.Value.kind = metricKindUint64;
                @out.Value.scalar = (uint64)gcount();
            }
        ),
        ["/sched/latencies:seconds"u8] = new(
            compute: (ж<statAggregate> _, ж<metricValue> @out) => {
                Ꮡsched.of(schedt.ᏑtimeToRun).write(@out);
            }
        ),
        ["/sched/pauses/stopping/gc:seconds"u8] = new(
            compute: (ж<statAggregate> _, ж<metricValue> @out) => {
                Ꮡsched.of(schedt.ᏑstwStoppingTimeGC).write(@out);
            }
        ),
        ["/sched/pauses/stopping/other:seconds"u8] = new(
            compute: (ж<statAggregate> _, ж<metricValue> @out) => {
                Ꮡsched.of(schedt.ᏑstwStoppingTimeOther).write(@out);
            }
        ),
        ["/sched/pauses/total/gc:seconds"u8] = new(
            compute: (ж<statAggregate> _, ж<metricValue> @out) => {
                Ꮡsched.of(schedt.ᏑstwTotalTimeGC).write(@out);
            }
        ),
        ["/sched/pauses/total/other:seconds"u8] = new(
            compute: (ж<statAggregate> _, ж<metricValue> @out) => {
                Ꮡsched.of(schedt.ᏑstwTotalTimeOther).write(@out);
            }
        ),
        ["/sync/mutex/wait/total:seconds"u8] = new(
            compute: (ж<statAggregate> _, ж<metricValue> @out) => {
                @out.Value.kind = metricKindFloat64;
                @out.Value.scalar = float64bits(nsToSec(totalMutexWaitTimeNanos()));
            }
        )
    };
    foreach (var (_, info) in godebugs.All) {
        if (!info.Opaque) {
            metrics["/godebug/non-default-behavior/"u8 + info.Name + ":events"u8] = new metricData(compute: compute0);
        }
    }
    metricsInit = true;
}

internal static void compute0(ж<statAggregate> _, ж<metricValue> Ꮡout) {
    ref var @out = ref Ꮡout.DerefOrNull();

    @out.kind = metricKindUint64;
    @out.scalar = 0;
}

internal delegate uint64 metricReader();

internal static void compute(this metricReader f, ж<statAggregate> _, ж<metricValue> Ꮡout) {
    ref var @out = ref Ꮡout.DerefOrNull();

    @out.kind = metricKindUint64;
    @out.scalar = f();
}

//go:linkname godebug_registerMetric internal/godebug.registerMetric
internal static void godebug_registerMetric(@string name, Func<uint64> read) {
    metricsLock();
    initMetrics();
    var (d, ok) = metrics[name, ꟷ];
    if (!ok) {
        @throw("runtime: unexpected metric registration for "u8 + name);
    }
    d.compute = (ж<statAggregate> p1, ж<metricValue> p2) => NilSafeDelegateConversion<metricReader, Func<uint64>>(read).compute(p1, p2);
    metrics[name] = d.ΔClone();
    metricsUnlock();
}

[GoType("num:nuint")] partial struct statDep;

internal static statDep heapStatsDep => /* iota */ 0; // corresponds to heapStatsAggregate
internal static statDep sysStatsDep => 1; // corresponds to sysStatsAggregate
internal static statDep cpuStatsDep => 2; // corresponds to cpuStatsAggregate
internal static statDep gcStatsDep => 3;  // corresponds to gcStatsAggregate
internal static statDep numStatsDeps => 4;

[GoType("[1]uint64")] partial struct statDepSet;

// makeStatDepSet creates a new statDepSet from a list of statDeps.
internal static statDepSet makeStatDepSet(params ꓸꓸꓸstatDep depsʗp) {
    var deps = depsʗp.sslice();

    statDepSet s = default!;
    foreach (var (_, d) in deps) {
        s[d / 64] |= (uint64)(((uint64)1 << (int)(nuint)((d % 64))));
    }
    return s.Clone();
}

// difference returns set difference of s from b as a new set.
internal static statDepSet difference(this statDepSet s, statDepSet b) {
    s = s.Clone();
    b = b.Clone();

    statDepSet c = default!;
    foreach (var (i, _) in s) {
        c[i] = (uint64)(s[i] & ~b[i]);
    }
    return c.Clone();
}

// union returns the union of the two sets as a new set.
internal static statDepSet union(this statDepSet s, statDepSet b) {
    s = s.Clone();
    b = b.Clone();

    statDepSet c = default!;
    foreach (var (i, _) in s) {
        c[i] = (uint64)(s[i] | b[i]);
    }
    return c.Clone();
}

// empty returns true if there are no dependencies in the set.
[GoRecv] internal static bool empty(this ref statDepSet s) {
    foreach (var (_, c) in s.Value) {
        if (c != 0) {
            return false;
        }
    }
    return true;
}

// has returns true if the set contains a given statDep.
[GoRecv] internal static bool has(this ref statDepSet s, statDep d) {
    return (uint64)(s.Value[d / 64] & (((uint64)1 << (int)(nuint)((d % 64))))) != 0;
}

// heapStatsAggregate represents memory stats obtained from the
// runtime. This set of stats is grouped together because they
// depend on each other in some way to make sense of the runtime's
// current heap memory use. They're also sharded across Ps, so it
// makes sense to grab them all at once.
[GoType] partial struct heapStatsAggregate {
    internal partial ref heapStatsDelta heapStatsDelta { get; }
// Derived from values in heapStatsDelta.

    // inObjects is the bytes of memory occupied by objects,
    internal uint64 inObjects;
    // numObjects is the number of live objects in the heap.
    internal uint64 numObjects;
    // totalAllocated is the total bytes of heap objects allocated
    // over the lifetime of the program.
    internal uint64 totalAllocated;
    // totalFreed is the total bytes of heap objects freed
    // over the lifetime of the program.
    internal uint64 totalFreed;
    // totalAllocs is the number of heap objects allocated over
    // the lifetime of the program.
    internal uint64 totalAllocs;
    // totalFrees is the number of heap objects freed over
    // the lifetime of the program.
    internal uint64 totalFrees;
}

// compute populates the heapStatsAggregate with values from the runtime.
internal static void compute(this ж<heapStatsAggregate> Ꮡa) {
    ref var a = ref Ꮡa.DerefOrNull();

    Ꮡmemstats.of(mstats.ᏑheapStats).read(Ꮡa.of(heapStatsAggregate.ᏑheapStatsDelta));
    // Calculate derived stats.
    a.totalAllocs = a.largeAllocCount;
    a.totalFrees = a.largeFreeCount;
    a.totalAllocated = a.largeAlloc;
    a.totalFreed = a.largeFree;
    foreach (var (i, _) in a.smallAllocCount) {
        var na = a.smallAllocCount[i];
        var nf = a.smallFreeCount[i];
        a.totalAllocs += na;
        a.totalFrees += nf;
        a.totalAllocated += na * (uint64)class_to_size[i];
        a.totalFreed += nf * (uint64)class_to_size[i];
    }
    a.inObjects = a.totalAllocated - a.totalFreed;
    a.numObjects = a.totalAllocs - a.totalFrees;
}

// sysStatsAggregate represents system memory stats obtained
// from the runtime. This set of stats is grouped together because
// they're all relatively cheap to acquire and generally independent
// of one another and other runtime memory stats. The fact that they
// may be acquired at different times, especially with respect to
// heapStatsAggregate, means there could be some skew, but because of
// these stats are independent, there's no real consistency issue here.
[GoType] partial struct sysStatsAggregate {
    internal uint64 stacksSys;
    internal uint64 mSpanSys;
    internal uint64 mSpanInUse;
    internal uint64 mCacheSys;
    internal uint64 mCacheInUse;
    internal uint64 buckHashSys;
    internal uint64 gcMiscSys;
    internal uint64 otherSys;
    internal uint64 heapGoal;
    internal uint64 gcCyclesDone;
    internal uint64 gcCyclesForced;
}

// compute populates the sysStatsAggregate with values from the runtime.
internal static void compute(this ж<sysStatsAggregate> Ꮡa) {
    ref var a = ref Ꮡa.DerefOrNull();

    a.stacksSys = Ꮡmemstats.of(mstats.Ꮡstacks_sys).load();
    a.buckHashSys = Ꮡmemstats.of(mstats.Ꮡbuckhash_sys).load();
    a.gcMiscSys = Ꮡmemstats.of(mstats.ᏑgcMiscSys).load();
    a.otherSys = Ꮡmemstats.of(mstats.Ꮡother_sys).load();
    a.heapGoal = ᏑgcController.heapGoal();
    a.gcCyclesDone = (uint64)memstats.numgc;
    a.gcCyclesForced = (uint64)memstats.numforcedgc;
    systemstack(() => {
        @lock(Ꮡmheap_.of(mheap.Ꮡlock));
        Ꮡa.Value.mSpanSys = Ꮡmemstats.of(mstats.Ꮡmspan_sys).load();
        Ꮡa.Value.mSpanInUse = (uint64)mheap_.spanalloc.inuse;
        Ꮡa.Value.mCacheSys = Ꮡmemstats.of(mstats.Ꮡmcache_sys).load();
        Ꮡa.Value.mCacheInUse = (uint64)mheap_.cachealloc.inuse;
        unlock(Ꮡmheap_.of(mheap.Ꮡlock));
    });
}

// cpuStatsAggregate represents CPU stats obtained from the runtime
// acquired together to avoid skew and inconsistencies.
[GoType] partial struct cpuStatsAggregate {
    internal partial ref cpuStats cpuStats { get; }
}

// compute populates the cpuStatsAggregate with values from the runtime.
[GoRecv] internal static void compute(this ref cpuStatsAggregate a) {
    a.cpuStats = work.cpuStats;
}

// TODO(mknyszek): Update the CPU stats again so that we're not
// just relying on the STW snapshot. The issue here is that currently
// this will cause non-monotonicity in the "user" CPU time metric.
//
// a.cpuStats.accumulate(nanotime(), gcphase == _GCmark)

// gcStatsAggregate represents various GC stats obtained from the runtime
// acquired together to avoid skew and inconsistencies.
[GoType] partial struct gcStatsAggregate {
    internal uint64 heapScan;
    internal uint64 stackScan;
    internal uint64 globalsScan;
    internal uint64 totalScan;
}

// compute populates the gcStatsAggregate with values from the runtime.
[GoRecv] internal static void compute(this ref gcStatsAggregate a) {
    a.heapScan = ᏑgcController.of(gcControllerState.ᏑheapScan).Load();
    a.stackScan = ᏑgcController.of(gcControllerState.ᏑlastStackScan).Load();
    a.globalsScan = ᏑgcController.of(gcControllerState.ᏑglobalsScan).Load();
    a.totalScan = a.heapScan + a.stackScan + a.globalsScan;
}

// nsToSec takes a duration in nanoseconds and converts it to seconds as
// a float64.
internal static float64 nsToSec(int64 ns) {
    return (float64)ns / 1e9D;
}

// statAggregate is the main driver of the metrics implementation.
//
// It contains multiple aggregates of runtime statistics, as well
// as a set of these aggregates that it has populated. The aggregates
// are populated lazily by its ensure method.
[GoType] partial struct statAggregate {
    internal statDepSet ensured;
    internal heapStatsAggregate heapStats;
    internal sysStatsAggregate sysStats;
    internal cpuStatsAggregate cpuStats;
    internal gcStatsAggregate gcStats;
}

// ensure populates statistics aggregates determined by deps if they
// haven't yet been populated.
internal static void ensure(this ж<statAggregate> Ꮡa, ж<statDepSet> Ꮡdeps) {
    ref var a = ref Ꮡa.DerefOrNull();
    ref var deps = ref Ꮡdeps.DerefOrNull();

    var missing = deps.difference(a.ensured);
    if (missing.empty()) {
        return;
    }
    for (statDep i = ((statDep)0); i < numStatsDeps; i++) {
        if (!missing.has(i)) {
            continue;
        }
        var exprᴛ1 = i;
        if (exprᴛ1 == heapStatsDep) {
            Ꮡa.of(statAggregate.ᏑheapStats).compute();
        }
        else if (exprᴛ1 == sysStatsDep) {
            Ꮡa.of(statAggregate.ᏑsysStats).compute();
        }
        else if (exprᴛ1 == cpuStatsDep) {
            a.cpuStats.compute();
        }
        else if (exprᴛ1 == gcStatsDep) {
            a.gcStats.compute();
        }

    }
    a.ensured = a.ensured.union(missing);
}

[GoType("num:nint")] partial struct metricKind;

internal static metricKind metricKindBad => /* iota */ 0;
internal static metricKind metricKindUint64 => 1;
internal static metricKind metricKindFloat64 => 2;
internal static metricKind metricKindFloat64Histogram => 3;

// metricSample is a runtime copy of runtime/metrics.Sample and
// must be kept structurally identical to that type.
[GoType] partial struct metricSample {
    internal @string name;
    internal metricValue value;
}

// metricValue is a runtime copy of runtime/metrics.Sample and
// must be kept structurally identical to that type.
[GoType] partial struct metricValue {
    internal metricKind kind;
    internal uint64 scalar;         // contains scalar values for scalar Kinds.
    internal @unsafe.Pointer pointer; // contains non-scalar values.
}

// float64HistOrInit tries to pull out an existing float64Histogram
// from the value, but if none exists, then it allocates one with
// the given buckets.
[GoRecv] internal static ж<metricFloat64Histogram> float64HistOrInit(this ref metricValue v, slice<float64> buckets) {
    ж<metricFloat64Histogram> hist = default!;
    if (v.kind == metricKindFloat64Histogram && v.pointer != nil){
        hist = (ж<metricFloat64Histogram>)(uintptr)(v.pointer);
    } else {
        v.kind = metricKindFloat64Histogram;
        hist = @new<metricFloat64Histogram>();
        v.pointer = new @unsafe.Pointer(hist);
    }
    hist.Value.buckets = buckets;
    if (len((~hist).counts) != len((~hist).buckets) - 1) {
        hist.Value.counts = new slice<uint64>(len(buckets) - 1);
    }
    return hist;
}

// metricFloat64Histogram is a runtime copy of runtime/metrics.Float64Histogram
// and must be kept structurally identical to that type.
[GoType] partial struct metricFloat64Histogram {
    internal slice<uint64> counts;
    internal slice<float64> buckets;
}

// agg is used by readMetrics, and is protected by metricsSema.
//
// Managed as a global variable because its pointer will be
// an argument to a dynamically-defined function, and we'd
// like to avoid it escaping to the heap.
internal static ж<statAggregate> Ꮡagg = new StandardBox<statAggregate>(new statAggregate());
internal static ref statAggregate agg => ref Ꮡagg.Value;

[GoType] partial struct metricName {
    internal @string name;
    internal metricKind kind;
}

// readMetricNames is the implementation of runtime/metrics.readMetricNames,
// used by the runtime/metrics test and otherwise unreferenced.
//
//go:linkname readMetricNames runtime/metrics_test.runtime_readMetricNames
public static slice<@string> readMetricNames() {
    metricsLock();
    initMetrics();
    nint n = len(metrics);
    metricsUnlock();
    var list = new slice<@string>(0, n);
    metricsLock();
    foreach (var (name, _) in metrics) {
        list = append(list, name);
    }
    metricsUnlock();
    return list;
}

// readMetrics is the implementation of runtime/metrics.Read.
//
//go:linkname readMetrics runtime/metrics.runtime_readMetrics
internal static void readMetrics(@unsafe.Pointer samplesp, nint len, nint cap) {
    metricsLock();
    // Ensure the map is initialized.
    initMetrics();
    // Read the metrics.
    readMetricsLocked(samplesp, len, cap);
    metricsUnlock();
}

// readMetricsLocked is the internal, locked portion of readMetrics.
//
// Broken out for more robust testing. metricsLock must be held and
// initMetrics must have been called already.
internal static void readMetricsLocked(@unsafe.Pointer samplesp, nint len, nint cap) {
    // Construct a slice from the args.
    ref var sl = ref heap<Δsliceᴛ>(out var Ꮡsl);
    sl = new Δsliceᴛ(samplesp.Value, len, cap);
    var samples = ~Ꮡsl.Reinterpret<Δsliceᴛ, slice<metricSample>>();
    // Clear agg defensively.
    agg = new statAggregate(nil);
    // Sample.
    foreach (var (i, _) in samples) {
        var sample = Ꮡ(samples, i);
        ref var data = ref heap<metricData>(out var Ꮡdata);
        (data, var ok) = metrics[(~sample).name, ꟷ];
        if (!ok) {
            sample.Value.value.kind = metricKindBad;
            continue;
        }
        // Ensure we have all the stats we need.
        // agg is populated lazily.
        Ꮡagg.ensure(Ꮡdata.of(metricData.Ꮡdeps));
        // Compute the value based on the stats we have.
        data.compute(Ꮡagg, sample.of(metricSample.Ꮡvalue));
    }
}

} // end runtime_package
