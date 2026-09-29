// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.runtime;

using godebugs = @internal.godebugs_package;
using @internal;

partial class metrics_package {

// Description describes a runtime metric.
[GoType] partial struct Description {
    // Name is the full name of the metric which includes the unit.
    //
    // The format of the metric may be described by the following regular expression.
    //
    // 	^(?P<name>/[^:]+):(?P<unit>[^:*/]+(?:[*/][^:*/]+)*)$
    //
    // The format splits the name into two components, separated by a colon: a path which always
    // starts with a /, and a machine-parseable unit. The name may contain any valid Unicode
    // codepoint in between / characters, but by convention will try to stick to lowercase
    // characters and hyphens. An example of such a path might be "/memory/heap/free".
    //
    // The unit is by convention a series of lowercase English unit names (singular or plural)
    // without prefixes delimited by '*' or '/'. The unit names may contain any valid Unicode
    // codepoint that is not a delimiter.
    // Examples of units might be "seconds", "bytes", "bytes/second", "cpu-seconds",
    // "byte*cpu-seconds", and "bytes/second/second".
    //
    // For histograms, multiple units may apply. For instance, the units of the buckets and
    // the count. By convention, for histograms, the units of the count are always "samples"
    // with the type of sample evident by the metric's name, while the unit in the name
    // specifies the buckets' unit.
    //
    // A complete name might look like "/memory/heap/free:bytes".
    public @string Name;
    // Description is an English language sentence describing the metric.
    public @string ΔDescription;
    // Kind is the kind of value for this metric.
    //
    // The purpose of this field is to allow users to filter out metrics whose values are
    // types which their application may not understand.
    public ValueKind Kind;
    // Cumulative is whether or not the metric is cumulative. If a cumulative metric is just
    // a single number, then it increases monotonically. If the metric is a distribution,
    // then each bucket count increases monotonically.
    //
    // This flag thus indicates whether or not it's useful to compute a rate from this value.
    public bool Cumulative;
}

// The English language descriptions below must be kept in sync with the
// descriptions of each metric in doc.go by running 'go generate'.
internal static slice<Description> allDesc = new Description[]{
    new(
        Name: "/cgo/go-to-c-calls:calls"u8,
        ΔDescription: "Count of calls made from Go to C by the current process."u8,
        Kind: KindUint64,
        Cumulative: true
    ),
    new(
        Name: "/cpu/classes/gc/mark/assist:cpu-seconds"u8,
        ΔDescription: "Estimated total CPU time goroutines spent performing GC tasks "u8 + "to assist the GC and prevent it from falling behind the application. "u8 + "This metric is an overestimate, and not directly comparable to "u8 + "system CPU time measurements. Compare only with other /cpu/classes "u8 + "metrics."u8,
        Kind: KindFloat64,
        Cumulative: true
    ),
    new(
        Name: "/cpu/classes/gc/mark/dedicated:cpu-seconds"u8,
        ΔDescription: "Estimated total CPU time spent performing GC tasks on "u8 + "processors (as defined by GOMAXPROCS) dedicated to those tasks. "u8 + "This metric is an overestimate, and not directly comparable to "u8 + "system CPU time measurements. Compare only with other /cpu/classes "u8 + "metrics."u8,
        Kind: KindFloat64,
        Cumulative: true
    ),
    new(
        Name: "/cpu/classes/gc/mark/idle:cpu-seconds"u8,
        ΔDescription: "Estimated total CPU time spent performing GC tasks on "u8 + "spare CPU resources that the Go scheduler could not otherwise find "u8 + "a use for. This should be subtracted from the total GC CPU time to "u8 + "obtain a measure of compulsory GC CPU time. "u8 + "This metric is an overestimate, and not directly comparable to "u8 + "system CPU time measurements. Compare only with other /cpu/classes "u8 + "metrics."u8,
        Kind: KindFloat64,
        Cumulative: true
    ),
    new(
        Name: "/cpu/classes/gc/pause:cpu-seconds"u8,
        ΔDescription: "Estimated total CPU time spent with the application paused by "u8 + "the GC. Even if only one thread is running during the pause, this is "u8 + "computed as GOMAXPROCS times the pause latency because nothing else "u8 + "can be executing. This is the exact sum of samples in "u8 + "/sched/pauses/total/gc:seconds if each sample is multiplied by "u8 + "GOMAXPROCS at the time it is taken. This metric is an overestimate, "u8 + "and not directly comparable to system CPU time measurements. Compare "u8 + "only with other /cpu/classes metrics."u8,
        Kind: KindFloat64,
        Cumulative: true
    ),
    new(
        Name: "/cpu/classes/gc/total:cpu-seconds"u8,
        ΔDescription: "Estimated total CPU time spent performing GC tasks. "u8 + "This metric is an overestimate, and not directly comparable to "u8 + "system CPU time measurements. Compare only with other /cpu/classes "u8 + "metrics. Sum of all metrics in /cpu/classes/gc."u8,
        Kind: KindFloat64,
        Cumulative: true
    ),
    new(
        Name: "/cpu/classes/idle:cpu-seconds"u8,
        ΔDescription: "Estimated total available CPU time not spent executing any Go or Go runtime code. "u8 + "In other words, the part of /cpu/classes/total:cpu-seconds that was unused. "u8 + "This metric is an overestimate, and not directly comparable to "u8 + "system CPU time measurements. Compare only with other /cpu/classes "u8 + "metrics."u8,
        Kind: KindFloat64,
        Cumulative: true
    ),
    new(
        Name: "/cpu/classes/scavenge/assist:cpu-seconds"u8,
        ΔDescription: "Estimated total CPU time spent returning unused memory to the "u8 + "underlying platform in response eagerly in response to memory pressure. "u8 + "This metric is an overestimate, and not directly comparable to "u8 + "system CPU time measurements. Compare only with other /cpu/classes "u8 + "metrics."u8,
        Kind: KindFloat64,
        Cumulative: true
    ),
    new(
        Name: "/cpu/classes/scavenge/background:cpu-seconds"u8,
        ΔDescription: "Estimated total CPU time spent performing background tasks "u8 + "to return unused memory to the underlying platform. "u8 + "This metric is an overestimate, and not directly comparable to "u8 + "system CPU time measurements. Compare only with other /cpu/classes "u8 + "metrics."u8,
        Kind: KindFloat64,
        Cumulative: true
    ),
    new(
        Name: "/cpu/classes/scavenge/total:cpu-seconds"u8,
        ΔDescription: "Estimated total CPU time spent performing tasks that return "u8 + "unused memory to the underlying platform. "u8 + "This metric is an overestimate, and not directly comparable to "u8 + "system CPU time measurements. Compare only with other /cpu/classes "u8 + "metrics. Sum of all metrics in /cpu/classes/scavenge."u8,
        Kind: KindFloat64,
        Cumulative: true
    ),
    new(
        Name: "/cpu/classes/total:cpu-seconds"u8,
        ΔDescription: "Estimated total available CPU time for user Go code "u8 + "or the Go runtime, as defined by GOMAXPROCS. In other words, GOMAXPROCS "u8 + "integrated over the wall-clock duration this process has been executing for. "u8 + "This metric is an overestimate, and not directly comparable to "u8 + "system CPU time measurements. Compare only with other /cpu/classes "u8 + "metrics. Sum of all metrics in /cpu/classes."u8,
        Kind: KindFloat64,
        Cumulative: true
    ),
    new(
        Name: "/cpu/classes/user:cpu-seconds"u8,
        ΔDescription: "Estimated total CPU time spent running user Go code. This may "u8 + "also include some small amount of time spent in the Go runtime. "u8 + "This metric is an overestimate, and not directly comparable to "u8 + "system CPU time measurements. Compare only with other /cpu/classes "u8 + "metrics."u8,
        Kind: KindFloat64,
        Cumulative: true
    ),
    new(
        Name: "/gc/cycles/automatic:gc-cycles"u8,
        ΔDescription: "Count of completed GC cycles generated by the Go runtime."u8,
        Kind: KindUint64,
        Cumulative: true
    ),
    new(
        Name: "/gc/cycles/forced:gc-cycles"u8,
        ΔDescription: "Count of completed GC cycles forced by the application."u8,
        Kind: KindUint64,
        Cumulative: true
    ),
    new(
        Name: "/gc/cycles/total:gc-cycles"u8,
        ΔDescription: "Count of all completed GC cycles."u8,
        Kind: KindUint64,
        Cumulative: true
    ),
    new(
        Name: "/gc/gogc:percent"u8,
        ΔDescription: "Heap size target percentage configured by the user, otherwise 100. This "u8 + "value is set by the GOGC environment variable, and the runtime/debug.SetGCPercent "u8 + "function."u8,
        Kind: KindUint64
    ),
    new(
        Name: "/gc/gomemlimit:bytes"u8,
        ΔDescription: "Go runtime memory limit configured by the user, otherwise "u8 + "math.MaxInt64. This value is set by the GOMEMLIMIT environment variable, and "u8 + "the runtime/debug.SetMemoryLimit function."u8,
        Kind: KindUint64
    ),
    new(
        Name: "/gc/heap/allocs-by-size:bytes"u8,
        ΔDescription: "Distribution of heap allocations by approximate size. "u8 + "Bucket counts increase monotonically. "u8 + "Note that this does not include tiny objects as defined by "u8 + "/gc/heap/tiny/allocs:objects, only tiny blocks."u8,
        Kind: KindFloat64Histogram,
        Cumulative: true
    ),
    new(
        Name: "/gc/heap/allocs:bytes"u8,
        ΔDescription: "Cumulative sum of memory allocated to the heap by the application."u8,
        Kind: KindUint64,
        Cumulative: true
    ),
    new(
        Name: "/gc/heap/allocs:objects"u8,
        ΔDescription: "Cumulative count of heap allocations triggered by the application. "u8 + "Note that this does not include tiny objects as defined by "u8 + "/gc/heap/tiny/allocs:objects, only tiny blocks."u8,
        Kind: KindUint64,
        Cumulative: true
    ),
    new(
        Name: "/gc/heap/frees-by-size:bytes"u8,
        ΔDescription: "Distribution of freed heap allocations by approximate size. "u8 + "Bucket counts increase monotonically. "u8 + "Note that this does not include tiny objects as defined by "u8 + "/gc/heap/tiny/allocs:objects, only tiny blocks."u8,
        Kind: KindFloat64Histogram,
        Cumulative: true
    ),
    new(
        Name: "/gc/heap/frees:bytes"u8,
        ΔDescription: "Cumulative sum of heap memory freed by the garbage collector."u8,
        Kind: KindUint64,
        Cumulative: true
    ),
    new(
        Name: "/gc/heap/frees:objects"u8,
        ΔDescription: "Cumulative count of heap allocations whose storage was freed "u8 + "by the garbage collector. "u8 + "Note that this does not include tiny objects as defined by "u8 + "/gc/heap/tiny/allocs:objects, only tiny blocks."u8,
        Kind: KindUint64,
        Cumulative: true
    ),
    new(
        Name: "/gc/heap/goal:bytes"u8,
        ΔDescription: "Heap size target for the end of the GC cycle."u8,
        Kind: KindUint64
    ),
    new(
        Name: "/gc/heap/live:bytes"u8,
        ΔDescription: "Heap memory occupied by live objects that were marked by the previous GC."u8,
        Kind: KindUint64
    ),
    new(
        Name: "/gc/heap/objects:objects"u8,
        ΔDescription: "Number of objects, live or unswept, occupying heap memory."u8,
        Kind: KindUint64
    ),
    new(
        Name: "/gc/heap/tiny/allocs:objects"u8,
        ΔDescription: "Count of small allocations that are packed together into blocks. "u8 + "These allocations are counted separately from other allocations "u8 + "because each individual allocation is not tracked by the runtime, "u8 + "only their block. Each block is already accounted for in "u8 + "allocs-by-size and frees-by-size."u8,
        Kind: KindUint64,
        Cumulative: true
    ),
    new(
        Name: "/gc/limiter/last-enabled:gc-cycle"u8,
        ΔDescription: "GC cycle the last time the GC CPU limiter was enabled. "u8 + "This metric is useful for diagnosing the root cause of an out-of-memory "u8 + "error, because the limiter trades memory for CPU time when the GC's CPU "u8 + "time gets too high. This is most likely to occur with use of SetMemoryLimit. "u8 + "The first GC cycle is cycle 1, so a value of 0 indicates that it was never enabled."u8,
        Kind: KindUint64
    ),
    new(
        Name: "/gc/pauses:seconds"u8,
        ΔDescription: "Deprecated. Prefer the identical /sched/pauses/total/gc:seconds."u8,
        Kind: KindFloat64Histogram,
        Cumulative: true
    ),
    new(
        Name: "/gc/scan/globals:bytes"u8,
        ΔDescription: "The total amount of global variable space that is scannable."u8,
        Kind: KindUint64
    ),
    new(
        Name: "/gc/scan/heap:bytes"u8,
        ΔDescription: "The total amount of heap space that is scannable."u8,
        Kind: KindUint64
    ),
    new(
        Name: "/gc/scan/stack:bytes"u8,
        ΔDescription: "The number of bytes of stack that were scanned last GC cycle."u8,
        Kind: KindUint64
    ),
    new(
        Name: "/gc/scan/total:bytes"u8,
        ΔDescription: "The total amount space that is scannable. Sum of all metrics in /gc/scan."u8,
        Kind: KindUint64
    ),
    new(
        Name: "/gc/stack/starting-size:bytes"u8,
        ΔDescription: "The stack size of new goroutines."u8,
        Kind: KindUint64,
        Cumulative: false
    ),
    new(
        Name: "/memory/classes/heap/free:bytes"u8,
        ΔDescription: "Memory that is completely free and eligible to be returned to the underlying system, "u8 + "but has not been. This metric is the runtime's estimate of free address space that is backed by "u8 + "physical memory."u8,
        Kind: KindUint64
    ),
    new(
        Name: "/memory/classes/heap/objects:bytes"u8,
        ΔDescription: "Memory occupied by live objects and dead objects that have not yet been marked free by the garbage collector."u8,
        Kind: KindUint64
    ),
    new(
        Name: "/memory/classes/heap/released:bytes"u8,
        ΔDescription: "Memory that is completely free and has been returned to the underlying system. This "u8 + "metric is the runtime's estimate of free address space that is still mapped into the process, "u8 + "but is not backed by physical memory."u8,
        Kind: KindUint64
    ),
    new(
        Name: "/memory/classes/heap/stacks:bytes"u8,
        ΔDescription: "Memory allocated from the heap that is reserved for stack space, whether or not it is currently in-use. "u8 + "Currently, this represents all stack memory for goroutines. It also includes all OS thread stacks in non-cgo programs. "u8 + "Note that stacks may be allocated differently in the future, and this may change."u8,
        Kind: KindUint64
    ),
    new(
        Name: "/memory/classes/heap/unused:bytes"u8,
        ΔDescription: "Memory that is reserved for heap objects but is not currently used to hold heap objects."u8,
        Kind: KindUint64
    ),
    new(
        Name: "/memory/classes/metadata/mcache/free:bytes"u8,
        ΔDescription: "Memory that is reserved for runtime mcache structures, but not in-use."u8,
        Kind: KindUint64
    ),
    new(
        Name: "/memory/classes/metadata/mcache/inuse:bytes"u8,
        ΔDescription: "Memory that is occupied by runtime mcache structures that are currently being used."u8,
        Kind: KindUint64
    ),
    new(
        Name: "/memory/classes/metadata/mspan/free:bytes"u8,
        ΔDescription: "Memory that is reserved for runtime mspan structures, but not in-use."u8,
        Kind: KindUint64
    ),
    new(
        Name: "/memory/classes/metadata/mspan/inuse:bytes"u8,
        ΔDescription: "Memory that is occupied by runtime mspan structures that are currently being used."u8,
        Kind: KindUint64
    ),
    new(
        Name: "/memory/classes/metadata/other:bytes"u8,
        ΔDescription: "Memory that is reserved for or used to hold runtime metadata."u8,
        Kind: KindUint64
    ),
    new(
        Name: "/memory/classes/os-stacks:bytes"u8,
        ΔDescription: "Stack memory allocated by the underlying operating system. "u8 + "In non-cgo programs this metric is currently zero. This may change in the future."u8 + "In cgo programs this metric includes OS thread stacks allocated directly from the OS. "u8 + "Currently, this only accounts for one stack in c-shared and c-archive build modes, "u8 + "and other sources of stacks from the OS are not measured. This too may change in the future."u8,
        Kind: KindUint64
    ),
    new(
        Name: "/memory/classes/other:bytes"u8,
        ΔDescription: "Memory used by execution trace buffers, structures for debugging the runtime, finalizer and profiler specials, and more."u8,
        Kind: KindUint64
    ),
    new(
        Name: "/memory/classes/profiling/buckets:bytes"u8,
        ΔDescription: "Memory that is used by the stack trace hash map used for profiling."u8,
        Kind: KindUint64
    ),
    new(
        Name: "/memory/classes/total:bytes"u8,
        ΔDescription: "All memory mapped by the Go runtime into the current process as read-write. Note that this does not include memory mapped by code called via cgo or via the syscall package. Sum of all metrics in /memory/classes."u8,
        Kind: KindUint64
    ),
    new(
        Name: "/sched/gomaxprocs:threads"u8,
        ΔDescription: "The current runtime.GOMAXPROCS setting, or the number of operating system threads that can execute user-level Go code simultaneously."u8,
        Kind: KindUint64
    ),
    new(
        Name: "/sched/goroutines:goroutines"u8,
        ΔDescription: "Count of live goroutines."u8,
        Kind: KindUint64
    ),
    new(
        Name: "/sched/latencies:seconds"u8,
        ΔDescription: "Distribution of the time goroutines have spent in the scheduler in a runnable state before actually running. Bucket counts increase monotonically."u8,
        Kind: KindFloat64Histogram,
        Cumulative: true
    ),
    new(
        Name: "/sched/pauses/stopping/gc:seconds"u8,
        ΔDescription: "Distribution of individual GC-related stop-the-world stopping latencies. This is the time it takes from deciding to stop the world until all Ps are stopped. This is a subset of the total GC-related stop-the-world time (/sched/pauses/total/gc:seconds). During this time, some threads may be executing. Bucket counts increase monotonically."u8,
        Kind: KindFloat64Histogram,
        Cumulative: true
    ),
    new(
        Name: "/sched/pauses/stopping/other:seconds"u8,
        ΔDescription: "Distribution of individual non-GC-related stop-the-world stopping latencies. This is the time it takes from deciding to stop the world until all Ps are stopped. This is a subset of the total non-GC-related stop-the-world time (/sched/pauses/total/other:seconds). During this time, some threads may be executing. Bucket counts increase monotonically."u8,
        Kind: KindFloat64Histogram,
        Cumulative: true
    ),
    new(
        Name: "/sched/pauses/total/gc:seconds"u8,
        ΔDescription: "Distribution of individual GC-related stop-the-world pause latencies. This is the time from deciding to stop the world until the world is started again. Some of this time is spent getting all threads to stop (this is measured directly in /sched/pauses/stopping/gc:seconds), during which some threads may still be running. Bucket counts increase monotonically."u8,
        Kind: KindFloat64Histogram,
        Cumulative: true
    ),
    new(
        Name: "/sched/pauses/total/other:seconds"u8,
        ΔDescription: "Distribution of individual non-GC-related stop-the-world pause latencies. This is the time from deciding to stop the world until the world is started again. Some of this time is spent getting all threads to stop (measured directly in /sched/pauses/stopping/other:seconds). Bucket counts increase monotonically."u8,
        Kind: KindFloat64Histogram,
        Cumulative: true
    ),
    new(
        Name: "/sync/mutex/wait/total:seconds"u8,
        ΔDescription: "Approximate cumulative time goroutines have spent blocked on a sync.Mutex, sync.RWMutex, or runtime-internal lock. This metric is useful for identifying global changes in lock contention. Collect a mutex or block profile using the runtime/pprof package for more detailed contention data."u8,
        Kind: KindFloat64,
        Cumulative: true
    )
}.slice();

[GoInit] internal static void init() {
    // Insert all the non-default-reporting GODEBUGs into the table,
    // preserving the overall sort order.
    nint i = 0;
    while (i < len(allDesc) && allDesc[i].Name < "/godebug/"u8) {
        i++;
    }
    var more = new slice<Description>(i, len(allDesc) + len(godebugs.All));
    copy(more, allDesc);
    foreach (var (_, info) in godebugs.All) {
        if (!info.Opaque) {
            more = append(more, new Description(
                Name: "/godebug/non-default-behavior/"u8 + info.Name + ":events"u8,
                ΔDescription: "The number of non-default behaviors executed by the "u8 + info.Package + " package "u8 + "due to a non-default "u8 + "GODEBUG="u8 + info.Name + "=... setting."u8,
                Kind: KindUint64,
                Cumulative: true
            ));
        }
    }
    allDesc = appendꓸꓸꓸ(more, allDesc.slice(i));
}

// All returns a slice of containing metric descriptions for all supported metrics.
public static slice<Description> All() {
    return allDesc;
}

} // end metrics_package
