// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using bytes = bytes_package;
using fmt = fmt_package;
using abi = @internal.abi_package;
using goexperiment = @internal.goexperiment_package;
using profile = @internal.profile_package;
using testenv = @internal.testenv_package;
using Δos = os_package;
using reflect = reflect_package;
using Δruntime = runtime_package;
using Δdebug = global::go.runtime.debug_package;
using metrics = global::go.runtime.metrics_package;
using pprof = global::go.runtime.pprof_package;
using trace = global::go.runtime.trace_package;
using slices = slices_package;
using sort = sort_package;
using strings = strings_package;
using Δsync = sync_package;
using atomic = global::go.sync.atomic_package;
using testing = testing_package;
using time = time_package;
using @unsafe = unsafe_package;
using @internal;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using global::go.runtime;
using global::go.sync;
using static global::go.runtime_internal_test_package;
using Δio = io_package;

partial class runtime_test_package {

internal static (map<@string, metrics.Description>, slice<metrics.Sample>) prepareAllMetricsSamples() {
    var all = metrics.All();
    var samples = new slice<metrics.Sample>(len(all));
    var descs = new map<@string, metrics.Description>();
    foreach (var (i, _) in all) {
        samples[i].Name = all[i].Name;
        descs[all[i].Name] = all[i];
    }
    return (descs, samples);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object liveBytesIs0ˢ = (@string)"live bytes is 0"u8;
internal static readonly @string gcHeapTinyAllocsObjectsˢ = "/gc/heap/tiny/allocs:objects"u8;
internal static readonly @string gcHeapAllocsObjectsˢ = "/gc/heap/allocs:objects"u8;
internal static readonly @string gcHeapFreesObjectsˢ = "/gc/heap/frees:objects"u8;

public static void TestReadMetrics(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        // Run a GC cycle to get some of the stats to be non-zero.
        Δruntime.GC();
        // Set an arbitrary memory limit to check the metric for it
        var limit = (int64)(512 * 1024 * 1024);
        var oldLimit = Δdebug.SetMemoryLimit(limit);
        defer(Δdebug.SetMemoryLimit, oldLimit, ref ᒐ);
        // Set a GC percent to check the metric for it
        nint gcPercent = 99;
        nint oldGCPercent = Δdebug.SetGCPercent(gcPercent);
        defer(Δdebug.SetGCPercent, oldGCPercent, ref ᒐ);
        // Tests whether readMetrics produces values aligning
        // with ReadMemStats while the world is stopped.
        ref var mstats = ref heap(new Δruntime.MemStats(), out var Ꮡmstats);
        var (_, samples) = prepareAllMetricsSamples();
        runtime_internal_test_package.ReadMetricsSlow(Ꮡmstats, @unsafe.Pointer.FromPinnedBox(Ꮡ(samples, 0)), len(samples), cap(samples));
        void checkUint64(ж<testing.T> tΔ1, @string m, uint64 got, uint64 want) {
            tΔ1.Helper();
            if (got != want) {
                tΔ1.Errorf("metric %q: got %d, want %d"u8, m, got, want);
            }
        }
        // Check to make sure the values we read line up with other values we read.
        ж<metricsꓸFloat64Histogram> allocsBySize = default!;
        ж<metricsꓸFloat64Histogram> gcPauses = default!;
        ж<metricsꓸFloat64Histogram> schedPausesTotalGC = default!;
        uint64 tinyAllocs = default!;
        uint64 mallocs = default!;
        uint64 frees = default!;
        foreach (var (i, _) in samples) {
            {
                @string name = samples[i].Name;
                var exprᴛ1 = name;
                if (exprᴛ1 == "/cgo/go-to-c-calls:calls"u8) {
                    checkUint64(Ꮡt, name, samples[i].Value.Uint64(), (uint64)Δruntime.NumCgoCall());
                }
                else if (exprᴛ1 == "/memory/classes/heap/free:bytes"u8) {
                    checkUint64(Ꮡt, name, samples[i].Value.Uint64(), mstats.HeapIdle - mstats.HeapReleased);
                }
                else if (exprᴛ1 == "/memory/classes/heap/released:bytes"u8) {
                    checkUint64(Ꮡt, name, samples[i].Value.Uint64(), mstats.HeapReleased);
                }
                else if (exprᴛ1 == "/memory/classes/heap/objects:bytes"u8) {
                    checkUint64(Ꮡt, name, samples[i].Value.Uint64(), mstats.HeapAlloc);
                }
                else if (exprᴛ1 == "/memory/classes/heap/unused:bytes"u8) {
                    checkUint64(Ꮡt, name, samples[i].Value.Uint64(), mstats.HeapInuse - mstats.HeapAlloc);
                }
                else if (exprᴛ1 == "/memory/classes/heap/stacks:bytes"u8) {
                    checkUint64(Ꮡt, name, samples[i].Value.Uint64(), mstats.StackInuse);
                }
                else if (exprᴛ1 == "/memory/classes/metadata/mcache/free:bytes"u8) {
                    checkUint64(Ꮡt, name, samples[i].Value.Uint64(), mstats.MCacheSys - mstats.MCacheInuse);
                }
                else if (exprᴛ1 == "/memory/classes/metadata/mcache/inuse:bytes"u8) {
                    checkUint64(Ꮡt, name, samples[i].Value.Uint64(), mstats.MCacheInuse);
                }
                else if (exprᴛ1 == "/memory/classes/metadata/mspan/free:bytes"u8) {
                    checkUint64(Ꮡt, name, samples[i].Value.Uint64(), mstats.MSpanSys - mstats.MSpanInuse);
                }
                else if (exprᴛ1 == "/memory/classes/metadata/mspan/inuse:bytes"u8) {
                    checkUint64(Ꮡt, name, samples[i].Value.Uint64(), mstats.MSpanInuse);
                }
                else if (exprᴛ1 == "/memory/classes/metadata/other:bytes"u8) {
                    checkUint64(Ꮡt, name, samples[i].Value.Uint64(), mstats.GCSys);
                }
                else if (exprᴛ1 == "/memory/classes/os-stacks:bytes"u8) {
                    checkUint64(Ꮡt, name, samples[i].Value.Uint64(), mstats.StackSys - mstats.StackInuse);
                }
                else if (exprᴛ1 == "/memory/classes/other:bytes"u8) {
                    checkUint64(Ꮡt, name, samples[i].Value.Uint64(), mstats.OtherSys);
                }
                else if (exprᴛ1 == "/memory/classes/profiling/buckets:bytes"u8) {
                    checkUint64(Ꮡt, name, samples[i].Value.Uint64(), mstats.BuckHashSys);
                }
                else if (exprᴛ1 == "/memory/classes/total:bytes"u8) {
                    checkUint64(Ꮡt, name, samples[i].Value.Uint64(), mstats.Sys);
                }
                else if (exprᴛ1 == "/gc/heap/allocs-by-size:bytes"u8) {
                    var hist = samples[i].Value.Float64Histogram();
                    foreach (var (iΔ3, sc) in mstats.BySize[1..]) {
                        // Skip size class 0 in BySize, because it's always empty and not represented
                        // in the histogram.
                        {
                            var (b, s) = ((~hist).Buckets[iΔ3 + 1], (float64)(sc.Size + 1)); if (b != s) {
                                Ꮡt.Errorf("bucket does not match size class: got %f, want %f"u8, b, s);
                                // The rest of the checks aren't expected to work anyway.
                                continue;
                            }
                        }
                        {
                            var (c, m) = ((~hist).Counts[iΔ3], sc.Mallocs); if (c != m) {
                                Ꮡt.Errorf("histogram counts do not much BySize for class %d: got %d, want %d"u8, iΔ3, c, m);
                            }
                        }
                    }
                    allocsBySize = hist;
                }
                else if (exprᴛ1 == "/gc/heap/allocs:bytes"u8) {
                    checkUint64(Ꮡt, name, samples[i].Value.Uint64(), mstats.TotalAlloc);
                }
                else if (exprᴛ1 == "/gc/heap/frees-by-size:bytes"u8) {
                    var hist = samples[i].Value.Float64Histogram();
                    foreach (var (iΔ4, sc) in mstats.BySize[1..]) {
                        // Skip size class 0 in BySize, because it's always empty and not represented
                        // in the histogram.
                        {
                            var (b, s) = ((~hist).Buckets[iΔ4 + 1], (float64)(sc.Size + 1)); if (b != s) {
                                Ꮡt.Errorf("bucket does not match size class: got %f, want %f"u8, b, s);
                                // The rest of the checks aren't expected to work anyway.
                                continue;
                            }
                        }
                        {
                            var (c, f) = ((~hist).Counts[iΔ4], sc.Frees); if (c != f) {
                                Ꮡt.Errorf("histogram counts do not match BySize for class %d: got %d, want %d"u8, iΔ4, c, f);
                            }
                        }
                    }
                }
                else if (exprᴛ1 == "/gc/heap/frees:bytes"u8) {
                    checkUint64(Ꮡt, name, samples[i].Value.Uint64(), mstats.TotalAlloc - mstats.HeapAlloc);
                }
                else if (exprᴛ1 == "/gc/heap/tiny/allocs:objects"u8) {
                    tinyAllocs = samples[i].Value.Uint64();
                }
                else if (exprᴛ1 == "/gc/heap/allocs:objects"u8) {
                    mallocs = samples[i].Value.Uint64();
                }
                else if (exprᴛ1 == "/gc/heap/frees:objects"u8) {
                    frees = samples[i].Value.Uint64();
                }
                else if (exprᴛ1 == "/gc/heap/live:bytes"u8) {
                    {
                        var live = samples[i].Value.Uint64(); if (live > mstats.HeapSys){
                            // Currently, MemStats adds tiny alloc count to both Mallocs AND Frees.
                            // The reason for this is because MemStats couldn't be extended at the time
                            // but there was a desire to have Mallocs at least be a little more representative,
                            // while having Mallocs - Frees still represent a live object count.
                            // Unfortunately, MemStats doesn't actually export a large allocation count,
                            // so it's impossible to pull this number out directly.
                            //
                            // Check tiny allocation count outside of this loop, by using the allocs-by-size
                            // histogram in order to figure out how many large objects there are.
                            // Because the next two metrics tests are checking against Mallocs and Frees,
                            // we can't check them directly for the same reason: we need to account for tiny
                            // allocations included in Mallocs and Frees.
                            // Check for "obviously wrong" values. We can't check a stronger invariant,
                            // such as live <= HeapAlloc, because live is not 100% accurate. It's computed
                            // under racy conditions, and some objects may be double-counted (this is
                            // intentional and necessary for GC performance).
                            //
                            // Instead, check against a much more reasonable upper-bound: the amount of
                            // mapped heap memory. We can't possibly overcount to the point of exceeding
                            // total mapped heap memory, except if there's an accounting bug.
                            Ꮡt.Errorf("live bytes: %d > heap sys: %d"u8, live, mstats.HeapSys);
                        } else 
                        if (live == 0) {
                            // Might happen if we don't call runtime.GC() above.
                            Ꮡt.Error(liveBytesIs0ˢ);
                        }
                    }
                }
                else if (exprᴛ1 == "/gc/gomemlimit:bytes"u8) {
                    checkUint64(Ꮡt, name, samples[i].Value.Uint64(), (uint64)limit);
                }
                else if (exprᴛ1 == "/gc/heap/objects:objects"u8) {
                    checkUint64(Ꮡt, name, samples[i].Value.Uint64(), mstats.HeapObjects);
                }
                else if (exprᴛ1 == "/gc/heap/goal:bytes"u8) {
                    checkUint64(Ꮡt, name, samples[i].Value.Uint64(), mstats.NextGC);
                }
                else if (exprᴛ1 == "/gc/gogc:percent"u8) {
                    checkUint64(Ꮡt, name, samples[i].Value.Uint64(), (uint64)gcPercent);
                }
                else if (exprᴛ1 == "/gc/cycles/automatic:gc-cycles"u8) {
                    checkUint64(Ꮡt, name, samples[i].Value.Uint64(), (uint64)(mstats.NumGC - mstats.NumForcedGC));
                }
                else if (exprᴛ1 == "/gc/cycles/forced:gc-cycles"u8) {
                    checkUint64(Ꮡt, name, samples[i].Value.Uint64(), (uint64)mstats.NumForcedGC);
                }
                else if (exprᴛ1 == "/gc/cycles/total:gc-cycles"u8) {
                    checkUint64(Ꮡt, name, samples[i].Value.Uint64(), (uint64)mstats.NumGC);
                }
                else if (exprᴛ1 == "/gc/pauses:seconds"u8) {
                    gcPauses = samples[i].Value.Float64Histogram();
                }
                else if (exprᴛ1 == "/sched/pauses/total/gc:seconds"u8) {
                    schedPausesTotalGC = samples[i].Value.Float64Histogram();
                }
            }

        }
        // Check tinyAllocs.
        var nonTinyAllocs = (uint64)0;
        foreach (var (_, c) in (~allocsBySize).Counts) {
            nonTinyAllocs += c;
        }
        checkUint64(Ꮡt, gcHeapTinyAllocsObjectsˢ, tinyAllocs, mstats.Mallocs - nonTinyAllocs);
        // Check allocation and free counts.
        checkUint64(Ꮡt, gcHeapAllocsObjectsˢ, mallocs, mstats.Mallocs - tinyAllocs);
        checkUint64(Ꮡt, gcHeapFreesObjectsˢ, frees, mstats.Frees - tinyAllocs);
        // Verify that /gc/pauses:seconds is a copy of /sched/pauses/total/gc:seconds
        if (!slices.Equal<slice<float64>, float64>((~gcPauses).Buckets, (~schedPausesTotalGC).Buckets)) {
            Ꮡt.Errorf("/gc/pauses:seconds buckets %v do not match /sched/pauses/total/gc:seconds buckets %v"u8, (~gcPauses).Buckets, (~schedPausesTotalGC).Counts);
        }
        if (!slices.Equal<slice<uint64>, uint64>((~gcPauses).Counts, (~schedPausesTotalGC).Counts)) {
            Ꮡt.Errorf("/gc/pauses:seconds counts %v do not match /sched/pauses/total/gc:seconds counts %v"u8, (~gcPauses).Counts, (~schedPausesTotalGC).Counts);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string memoryClassesˢ = "/memory/classes"u8;
internal static readonly object numberOfGoroutinesIsLessˢ = (@string)"number of goroutines is less than one"u8;
internal static readonly object allocsBySizeAndFreesByˢ = (@string)"allocs-by-size and frees-by-size buckets don't match in length"u8;
internal static readonly object allocsBySizeAndFreesByˢ2 = (@string)"allocs-by-size and frees-by-size counts don't match in length"u8;

[GoType("dyn")] internal partial struct TestReadMetricsConsistency_totalVirtual {
    internal uint64 got, want;
}

[GoType("dyn")] internal partial struct TestReadMetricsConsistency_objects {
    internal ж<metricsꓸFloat64Histogram> alloc, free;
    internal uint64 allocs, frees;
    internal uint64 allocdBytes, freedBytes;
    internal uint64 total, totalBytes;
}

[GoType("dyn")] internal partial struct TestReadMetricsConsistency_gc {
    internal uint64 numGC;
    internal uint64 pauses;
}

[GoType("dyn")] internal partial struct TestReadMetricsConsistency_cpu {
    internal float64 gcAssist;
    internal float64 gcDedicated;
    internal float64 gcIdle;
    internal float64 gcPause;
    internal float64 gcTotal;
    internal float64 idle;
    internal float64 user;
    internal float64 scavengeAssist;
    internal float64 scavengeBg;
    internal float64 scavengeTotal;
    internal float64 total;
}

public static void TestReadMetricsConsistency(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // Tests whether readMetrics produces consistent, sensible values.
    // The values are read concurrently with the runtime doing other
    // things (e.g. allocating) so what we read can't reasonably compared
    // to other runtime values (e.g. MemStats).
    // Run a few GC cycles to get some of the stats to be non-zero.
    Δruntime.GC();
    Δruntime.GC();
    Δruntime.GC();
    // Set GOMAXPROCS high then sleep briefly to ensure we generate
    // some idle time.
    nint oldmaxprocs = Δruntime.GOMAXPROCS(10);
    time.Sleep(time.Millisecond);
    Δruntime.GOMAXPROCS(oldmaxprocs);
    // Read all the supported metrics through the metrics package.
    var (descs, samples) = prepareAllMetricsSamples();
    metrics.Read(samples);
    // Check to make sure the values we read make sense.
    TestReadMetricsConsistency_totalVirtual totalVirtual = default!;
    TestReadMetricsConsistency_objects objects = default!;
    TestReadMetricsConsistency_gc gc = default!;
    TestReadMetricsConsistency_totalVirtual totalScan = default!;
    TestReadMetricsConsistency_cpu cpu = default!;
    foreach (var (i, _) in samples) {
        metrics.ValueKind kind = samples[i].Value.Kind();
        {
            metrics.ValueKind want = descs[samples[i].Name].Kind; if (kind != want) {
                Ꮡt.Errorf("supported metric %q has unexpected kind: got %d, want %d"u8, samples[i].Name, kind, want);
                continue;
            }
        }
        if (samples[i].Name != "/memory/classes/total:bytes"u8 && strings.HasPrefix(samples[i].Name, memoryClassesˢ)) {
            var v = samples[i].Value.Uint64();
            totalVirtual.want += v;
            // None of these stats should ever get this big.
            // If they do, there's probably overflow involved,
            // usually due to bad accounting.
            if ((int64)v < 0) {
                Ꮡt.Errorf("%q has high/negative value: %d"u8, samples[i].Name, v);
            }
        }
        var exprᴛ1 = samples[i].Name;
        if (exprᴛ1 == "/cpu/classes/gc/mark/assist:cpu-seconds"u8) {
            cpu.gcAssist = samples[i].Value.Float64();
        }
        else if (exprᴛ1 == "/cpu/classes/gc/mark/dedicated:cpu-seconds"u8) {
            cpu.gcDedicated = samples[i].Value.Float64();
        }
        else if (exprᴛ1 == "/cpu/classes/gc/mark/idle:cpu-seconds"u8) {
            cpu.gcIdle = samples[i].Value.Float64();
        }
        else if (exprᴛ1 == "/cpu/classes/gc/pause:cpu-seconds"u8) {
            cpu.gcPause = samples[i].Value.Float64();
        }
        else if (exprᴛ1 == "/cpu/classes/gc/total:cpu-seconds"u8) {
            cpu.gcTotal = samples[i].Value.Float64();
        }
        else if (exprᴛ1 == "/cpu/classes/idle:cpu-seconds"u8) {
            cpu.idle = samples[i].Value.Float64();
        }
        else if (exprᴛ1 == "/cpu/classes/scavenge/assist:cpu-seconds"u8) {
            cpu.scavengeAssist = samples[i].Value.Float64();
        }
        else if (exprᴛ1 == "/cpu/classes/scavenge/background:cpu-seconds"u8) {
            cpu.scavengeBg = samples[i].Value.Float64();
        }
        else if (exprᴛ1 == "/cpu/classes/scavenge/total:cpu-seconds"u8) {
            cpu.scavengeTotal = samples[i].Value.Float64();
        }
        else if (exprᴛ1 == "/cpu/classes/total:cpu-seconds"u8) {
            cpu.total = samples[i].Value.Float64();
        }
        else if (exprᴛ1 == "/cpu/classes/user:cpu-seconds"u8) {
            cpu.user = samples[i].Value.Float64();
        }
        else if (exprᴛ1 == "/memory/classes/total:bytes"u8) {
            totalVirtual.got = samples[i].Value.Uint64();
        }
        else if (exprᴛ1 == "/memory/classes/heap/objects:bytes"u8) {
            objects.totalBytes = samples[i].Value.Uint64();
        }
        else if (exprᴛ1 == "/gc/heap/objects:objects"u8) {
            objects.total = samples[i].Value.Uint64();
        }
        else if (exprᴛ1 == "/gc/heap/allocs:bytes"u8) {
            objects.allocdBytes = samples[i].Value.Uint64();
        }
        else if (exprᴛ1 == "/gc/heap/allocs:objects"u8) {
            objects.allocs = samples[i].Value.Uint64();
        }
        else if (exprᴛ1 == "/gc/heap/allocs-by-size:bytes"u8) {
            objects.alloc = samples[i].Value.Float64Histogram();
        }
        else if (exprᴛ1 == "/gc/heap/frees:bytes"u8) {
            objects.freedBytes = samples[i].Value.Uint64();
        }
        else if (exprᴛ1 == "/gc/heap/frees:objects"u8) {
            objects.frees = samples[i].Value.Uint64();
        }
        else if (exprᴛ1 == "/gc/heap/frees-by-size:bytes"u8) {
            objects.free = samples[i].Value.Float64Histogram();
        }
        else if (exprᴛ1 == "/gc/cycles:gc-cycles"u8) {
            gc.numGC = samples[i].Value.Uint64();
        }
        else if (exprᴛ1 == "/gc/pauses:seconds"u8) {
            var h = samples[i].Value.Float64Histogram();
            gc.pauses = 0;
            foreach (var (iΔ2, _) in (~h).Counts) {
                gc.pauses += (~h).Counts[iΔ2];
            }
        }
        else if (exprᴛ1 == "/gc/scan/heap:bytes"u8) {
            totalScan.want += samples[i].Value.Uint64();
        }
        else if (exprᴛ1 == "/gc/scan/globals:bytes"u8) {
            totalScan.want += samples[i].Value.Uint64();
        }
        else if (exprᴛ1 == "/gc/scan/stack:bytes"u8) {
            totalScan.want += samples[i].Value.Uint64();
        }
        else if (exprᴛ1 == "/gc/scan/total:bytes"u8) {
            totalScan.got = samples[i].Value.Uint64();
        }
        else if (exprᴛ1 == "/sched/gomaxprocs:threads"u8) {
            {
                var (got, want) = (samples[i].Value.Uint64(), (uint64)Δruntime.GOMAXPROCS(-1)); if (got != want) {
                    Ꮡt.Errorf("gomaxprocs doesn't match runtime.GOMAXPROCS: got %d, want %d"u8, got, want);
                }
            }
        }
        else if (exprᴛ1 == "/sched/goroutines:goroutines"u8) {
            if (samples[i].Value.Uint64() < 1) {
                Ꮡt.Error(numberOfGoroutinesIsLessˢ);
            }
        }

    }
    // Only check this on Linux where we can be reasonably sure we have a high-resolution timer.
    if (Δruntime.GOOS == "linux"u8) {
        if (cpu.gcDedicated <= 0D && cpu.gcAssist <= 0D && cpu.gcIdle <= 0D) {
            Ꮡt.Errorf("found no time spent on GC work: %#v"u8, cpu);
        }
        if (cpu.gcPause <= 0D) {
            Ꮡt.Errorf("found no GC pauses: %f"u8, cpu.gcPause);
        }
        if (cpu.idle <= 0D) {
            Ꮡt.Errorf("found no idle time: %f"u8, cpu.idle);
        }
        {
            var total = cpu.gcDedicated + cpu.gcAssist + cpu.gcIdle + cpu.gcPause; if (!withinEpsilon(cpu.gcTotal, total, 0.001D)) {
                Ꮡt.Errorf("calculated total GC CPU time not within %%0.1 of total: %f vs. %f"u8, total, cpu.gcTotal);
            }
        }
        {
            var total = cpu.scavengeAssist + cpu.scavengeBg; if (!withinEpsilon(cpu.scavengeTotal, total, 0.001D)) {
                Ꮡt.Errorf("calculated total scavenge CPU not within %%0.1 of total: %f vs. %f"u8, total, cpu.scavengeTotal);
            }
        }
        if (cpu.total <= 0D) {
            Ꮡt.Errorf("found no total CPU time passed"u8);
        }
        if (cpu.user <= 0D) {
            Ꮡt.Errorf("found no user time passed"u8);
        }
        {
            var total = cpu.gcTotal + cpu.scavengeTotal + cpu.user + cpu.idle; if (!withinEpsilon(cpu.total, total, 0.001D)) {
                Ꮡt.Errorf("calculated total CPU not within %%0.1 of total: %f vs. %f"u8, total, cpu.total);
            }
        }
    }
    if (totalVirtual.got != totalVirtual.want) {
        Ꮡt.Errorf(@"""/memory/classes/total:bytes"" does not match sum of /memory/classes/**: got %d, want %d"u8, totalVirtual.got, totalVirtual.want);
    }
    {
        var (got, want) = (objects.allocs - objects.frees, objects.total); if (got != want) {
            Ꮡt.Errorf("mismatch between object alloc/free tallies and total: got %d, want %d"u8, got, want);
        }
    }
    {
        var (got, want) = (objects.allocdBytes - objects.freedBytes, objects.totalBytes); if (got != want) {
            Ꮡt.Errorf("mismatch between object alloc/free tallies and total: got %d, want %d"u8, got, want);
        }
    }
    {
        nint b = len((~objects.alloc).Buckets);
        nint c = len((~objects.alloc).Counts); if (b != c + 1) {
            Ꮡt.Errorf("allocs-by-size has wrong bucket or counts length: %d buckets, %d counts"u8, b, c);
        }
    }
    {
        nint b = len((~objects.free).Buckets);
        nint c = len((~objects.free).Counts); if (b != c + 1) {
            Ꮡt.Errorf("frees-by-size has wrong bucket or counts length: %d buckets, %d counts"u8, b, c);
        }
    }
    if (len((~objects.alloc).Buckets) != len((~objects.free).Buckets)){
        Ꮡt.Error(allocsBySizeAndFreesByˢ);
    } else 
    if (len((~objects.alloc).Counts) != len((~objects.free).Counts)){
        Ꮡt.Error(allocsBySizeAndFreesByˢ2);
    } else {
        foreach (var (i, _) in (~objects.alloc).Buckets) {
            var ba = (~objects.alloc).Buckets[i];
            var bf = (~objects.free).Buckets[i];
            if (ba != bf) {
                Ꮡt.Errorf("bucket %d is different for alloc and free hists: %f != %f"u8, i, ba, bf);
            }
        }
        if (!Ꮡt.Failed()) {
            uint64 gotAlloc = default!;
            uint64 gotFree = default!;
            var want = objects.total;
            foreach (var (i, _) in (~objects.alloc).Counts) {
                if ((~objects.alloc).Counts[i] < (~objects.free).Counts[i]) {
                    Ꮡt.Errorf("found more allocs than frees in object dist bucket %d"u8, i);
                    continue;
                }
                gotAlloc += (~objects.alloc).Counts[i];
                gotFree += (~objects.free).Counts[i];
            }
            {
                var got = gotAlloc - gotFree; if (got != want) {
                    Ꮡt.Errorf("object distribution counts don't match count of live objects: got %d, want %d"u8, got, want);
                }
            }
            if (gotAlloc != objects.allocs) {
                Ꮡt.Errorf("object distribution counts don't match total allocs: got %d, want %d"u8, gotAlloc, objects.allocs);
            }
            if (gotFree != objects.frees) {
                Ꮡt.Errorf("object distribution counts don't match total allocs: got %d, want %d"u8, gotFree, objects.frees);
            }
        }
    }
    // The current GC has at least 2 pauses per GC.
    // Check to see if that value makes sense.
    if (gc.pauses < gc.numGC * 2) {
        Ꮡt.Errorf("fewer pauses than expected: got %d, want at least %d"u8, gc.pauses, gc.numGC * 2);
    }
    if (totalScan.got <= 0) {
        Ꮡt.Errorf("scannable GC space is empty: %d"u8, totalScan.got);
    }
    if (totalScan.got != totalScan.want) {
        Ꮡt.Errorf("/gc/scan/total:bytes doesn't line up with sum of /gc/scan*: total %d vs. sum %d"u8, totalScan.got, totalScan.want);
    }
}

public static void BenchmarkReadMetricsLatency(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var stop = applyGCLoad(Ꮡb);
    // Spend this much time measuring latencies.
    var latencies = new slice<time.Duration>(0, 1024);
    var (_, samples) = prepareAllMetricsSamples();
    // Hit metrics.Read continuously and measure.
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        var start = time.Now();
        metrics.Read(samples);
        latencies = append(latencies, time.Since(start));
    }
    // Make sure to stop the timer before we wait! The load created above
    // is very heavy-weight and not easy to stop, so we could end up
    // confusing the benchmarking framework for small b.N.
    b.StopTimer();
    stop();
    // Disable the default */op metrics.
    // ns/op doesn't mean anything because it's an average, but we
    // have a sleep in our b.N loop above which skews this significantly.
    b.ReportMetric(0D, nsOpˢ);
    b.ReportMetric(0D, bOpˢ);
    b.ReportMetric(0D, allocsOpˢ);
    // Sort latencies then report percentiles.
    var latenciesʗ1 = latencies;
    sort.Slice(latencies, (nint i, nint j) => latenciesʗ1[i] < latenciesʗ1[j]);
    b.ReportMetric((float64)(int64)latencies[len(latencies) * 50 / 100], p50Nsˢ);
    b.ReportMetric((float64)(int64)latencies[len(latencies) * 90 / 100], p90Nsˢ);
    b.ReportMetric((float64)(int64)latencies[len(latencies) * 99 / 100], p99Nsˢ);
}

internal static array<any> readMetricsSink = new(1024);

public static partial void TestReadMetricsCumulative(ж<testing.T> Ꮡt) {
    // Set up the set of metrics marked cumulative.
    var descs = metrics.All();
    array<slice<metrics.Sample>> samples = new(2);
    samples[0] = new slice<metrics.Sample>(len(descs));
    samples[1] = new slice<metrics.Sample>(len(descs));
    nint total = 0;
    foreach (var (i, _) in samples[0]) {
        if (!descs[i].Cumulative) {
            continue;
        }
        samples[0][total].Name = descs[i].Name;
        total++;
    }
    samples[0] = samples[0].slice(0, total);
    samples[1] = samples[1].slice(0, total);
    copy(samples[1], samples[0]);
    // Start some noise in the background.
    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
    Ꮡwg.Add(1);
    var done = new channel<EmptyStruct>(0);
    var doneʗ1 = done;
    goǃ(() => {
        GoFrame ᒐ = default;
        try {
            defer(Ꮡwg.Done, ref ᒐ);
            while (ᐧ) {
                // Add more things here that could influence metrics.
                for (nint i = 0; i < len(readMetricsSink); i++) {
                    readMetricsSink[i] = new slice<byte>(1024);
                    var selᴛ72 = doneʗ1;
                    switch (trySelect(ᐸꟷ(selᴛ72, ꓸꓸꓸ))) {
                    case 0 when selᴛ72.ꟷᐳ(out _): {
                        return;
                    }
                    default: {
                        break;
                    }}
                }
                Δruntime.GC();
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
    uint64 sum(slice<uint64> us) {
        var totalΔ1 = (uint64)0;
        foreach (var (_, u) in us) {
            totalΔ1 += u;
        }
        return totalΔ1;
    }
    // Populate the first generation.
    metrics.Read(samples[0]);
    // Check to make sure that these metrics only grow monotonically.
    for (nint gen = 1; gen < 10; gen++) {
        metrics.Read(samples[gen % 2]);
        foreach (var (i, _) in samples[gen % 2]) {
            @string name = samples[gen % 2][i].Name;
            var (vNew, vOld) = (samples[gen % 2][i].Value, samples[1 - (gen % 2)][i].Value);
            var exprᴛ1 = vNew.Kind();
            if (exprᴛ1 == metrics.KindUint64) {
                var @new = vNew.Uint64();
                var old = vOld.Uint64();
                if (@new < old) {
                    Ꮡt.Errorf("%s decreased: %d < %d"u8, name, @new, old);
                }
            }
            else if (exprᴛ1 == metrics.KindFloat64) {
                var @new = vNew.Float64();
                var old = vOld.Float64();
                if (@new < old) {
                    Ꮡt.Errorf("%s decreased: %f < %f"u8, name, @new, old);
                }
            }
            else if (exprᴛ1 == metrics.KindFloat64Histogram) {
                var @new = sum((~vNew.Float64Histogram()).Counts);
                var old = sum((~vOld.Float64Histogram()).Counts);
                if (@new < old) {
                    Ꮡt.Errorf("%s counts decreased: %d < %d"u8, name, @new, old);
                }
            }

        }
    }
    close(done);
    Ꮡwg.Wait();
}

internal static bool withinEpsilon(float64 v1, float64 v2, float64 e) {
    return v2 - v2 * e <= v1 && v1 <= v2 + v2 * e;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string syncMutexWaitTotalˢ = "/sync/mutex/wait/total:seconds"u8;

public static void TestMutexWaitTimeMetric(ж<testing.T> Ꮡt) {
    ref var sample = ref heap(new array<metrics.Sample>(1), out var Ꮡsample);
    sample[0].Name = syncMutexWaitTotalˢ;
    var locks = new locker2[]{new runtime_test_package.mutexжlocker2(@new<mutex>()), new runtime_test_package.rwmutexWriteжlocker2(@new<rwmutexWrite>()), new runtime_test_package.rwmutexReadWriteжlocker2(@new<rwmutexReadWrite>()), new runtime_test_package.rwmutexWriteReadжlocker2(@new<rwmutexWriteRead>())
    }.slice();
    foreach (var (_, @lock) in locks) {
        var lockʗ1 = @lock;
        var sampleʗ1 = sample;
        Ꮡt.Run(reflect.TypeOf(@lock).Elem().Name(), (ж<testing.T> tΔ1) => {
            metrics.Read(sampleʗ1[..]);
            var before = ((time.Duration)(int64)(sampleʗ1[0].Value.Float64() * 1e9D));
            var minMutexWaitTime = generateMutexWaitTime(lockʗ1);
            metrics.Read(sampleʗ1[..]);
            var after = ((time.Duration)(int64)(sampleʗ1[0].Value.Float64() * 1e9D));
            {
                var wt = after - before; if (wt < minMutexWaitTime) {
                    tΔ1.Errorf("too little mutex wait time: got %s, want %s"u8, wt, minMutexWaitTime);
                }
            }
        });
    }
}

// locker2 represents an API surface of two concurrent goroutines
// locking the same resource, but through different APIs. It's intended
// to abstract over the relationship of two Lock calls or an RLock
// and a Lock call.
[GoType] partial interface locker2 {
    void Lock1();
    void Unlock1();
    void Lock2();
    void Unlock2();
}

[GoType] partial struct mutex {
    internal Δsync.Mutex mu;
}

internal static void Lock1(this ж<mutex> Ꮡm) {
    ref var m = ref Ꮡm.DerefOrNull();

    m.mu.Lock();
}

internal static void Unlock1(this ж<mutex> Ꮡm) {
    ref var m = ref Ꮡm.DerefOrNull();

    m.mu.Unlock();
}

internal static void Lock2(this ж<mutex> Ꮡm) {
    ref var m = ref Ꮡm.DerefOrNull();

    m.mu.Lock();
}

internal static void Unlock2(this ж<mutex> Ꮡm) {
    ref var m = ref Ꮡm.DerefOrNull();

    m.mu.Unlock();
}

[GoType] partial struct rwmutexWrite {
    internal Δsync.RWMutex mu;
}

internal static void Lock1(this ж<rwmutexWrite> Ꮡm) {
    Ꮡm.of(rwmutexWrite.Ꮡmu).Lock();
}

internal static void Unlock1(this ж<rwmutexWrite> Ꮡm) {
    Ꮡm.of(rwmutexWrite.Ꮡmu).Unlock();
}

internal static void Lock2(this ж<rwmutexWrite> Ꮡm) {
    Ꮡm.of(rwmutexWrite.Ꮡmu).Lock();
}

internal static void Unlock2(this ж<rwmutexWrite> Ꮡm) {
    Ꮡm.of(rwmutexWrite.Ꮡmu).Unlock();
}

[GoType] partial struct rwmutexReadWrite {
    internal Δsync.RWMutex mu;
}

internal static void Lock1(this ж<rwmutexReadWrite> Ꮡm) {
    Ꮡm.of(rwmutexReadWrite.Ꮡmu).RLock();
}

internal static void Unlock1(this ж<rwmutexReadWrite> Ꮡm) {
    Ꮡm.of(rwmutexReadWrite.Ꮡmu).RUnlock();
}

internal static void Lock2(this ж<rwmutexReadWrite> Ꮡm) {
    Ꮡm.of(rwmutexReadWrite.Ꮡmu).Lock();
}

internal static void Unlock2(this ж<rwmutexReadWrite> Ꮡm) {
    Ꮡm.of(rwmutexReadWrite.Ꮡmu).Unlock();
}

[GoType] partial struct rwmutexWriteRead {
    internal Δsync.RWMutex mu;
}

internal static void Lock1(this ж<rwmutexWriteRead> Ꮡm) {
    Ꮡm.of(rwmutexWriteRead.Ꮡmu).Lock();
}

internal static void Unlock1(this ж<rwmutexWriteRead> Ꮡm) {
    Ꮡm.of(rwmutexWriteRead.Ꮡmu).Unlock();
}

internal static void Lock2(this ж<rwmutexWriteRead> Ꮡm) {
    Ꮡm.of(rwmutexWriteRead.Ꮡmu).RLock();
}

internal static void Unlock2(this ж<rwmutexWriteRead> Ꮡm) {
    Ꮡm.of(rwmutexWriteRead.Ꮡmu).RUnlock();
}

// generateMutexWaitTime causes a couple of goroutines
// to block a whole bunch of times on a sync.Mutex, returning
// the minimum amount of time that should be visible in the
// /sync/mutex-wait:seconds metric.
internal static partial time.Duration generateMutexWaitTime(locker2 mu) {
    // Set up the runtime to always track casgstatus transitions for metrics.
    runtime_internal_test_package.CasGStatusAlwaysTrack.Value = true;
    mu.Lock1();
    // Start up a goroutine to wait on the lock.
    var gc = new channel<ж<G>>(0);
    var done = new channel<bool>(0);
    var doneʗ1 = done;
    var gcʗ1 = gc;
    goǃ(() => {
        gcʗ1.ᐸꟷ(runtime_internal_test_package.Getg());
        while (ᐧ) {
            mu.Lock2();
            mu.Unlock2();
            if (ᐸꟷ(doneʗ1)) {
                return;
            }
        }
    });
    var gp = ᐸꟷ(gc);
    // Set the block time high enough so that it will always show up, even
    // on systems with coarse timer granularity.
    time.Duration blockTime = /* 100 * time.Millisecond */ 100000000;
    // Make sure the goroutine spawned above actually blocks on the lock.
    while (ᐧ) {
        if (runtime_internal_test_package.GIsWaitingOnMutex(gp)) {
            break;
        }
        Δruntime.Gosched();
    }
    // Let some amount of time pass.
    time.Sleep(blockTime);
    // Let the other goroutine acquire the lock.
    mu.Unlock1();
    done.ᐸꟷ(true);
    // Reset flag.
    runtime_internal_test_package.CasGStatusAlwaysTrack.Value = false;
    return blockTime;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object wasip1CurrentlyBusyWaitsˢ = (@string)"wasip1 currently busy-waits in idle time; test not applicable"u8;

// See issue #60276.
public static void TestCPUMetricsSleep(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (Δruntime.GOOS == "wasip1"u8) {
        // Since wasip1 busy-waits in the scheduler, there's no meaningful idle
        // time. This is accurately reflected in the metrics, but it means this
        // test is basically meaningless on this platform.
        Ꮡt.Skip(wasip1CurrentlyBusyWaitsˢ);
    }
    var names = new @string[]{
        "/cpu/classes/idle:cpu-seconds"u8,
        "/cpu/classes/gc/mark/assist:cpu-seconds"u8,
        "/cpu/classes/gc/mark/dedicated:cpu-seconds"u8,
        "/cpu/classes/gc/mark/idle:cpu-seconds"u8,
        "/cpu/classes/gc/pause:cpu-seconds"u8,
        "/cpu/classes/gc/total:cpu-seconds"u8,
        "/cpu/classes/scavenge/assist:cpu-seconds"u8,
        "/cpu/classes/scavenge/background:cpu-seconds"u8,
        "/cpu/classes/scavenge/total:cpu-seconds"u8,
        "/cpu/classes/total:cpu-seconds"u8,
        "/cpu/classes/user:cpu-seconds"u8
    }.slice();
    var namesʗ1 = names;
    slice<metrics.Sample> prep() {
        var mm = new slice<metrics.Sample>(len(namesʗ1));
        foreach (var (i, _) in namesʗ1) {
            mm[i].Name = namesʗ1[i];
        }
        return mm;
    }
    var (m1, m2) = (prep(), prep());
    time.Duration dur = /* 100 * time.Millisecond */ 100000000;
    const nint maxFailures = 10;
    var failureIdleTimes = new slice<float64>(0, maxFailures);
    // If the bug we expect is happening, then the Sleep CPU time will be accounted for
    // as user time rather than idle time. In an ideal world we'd expect the whole application
    // to go instantly idle the moment this goroutine goes to sleep, and stay asleep for that
    // duration. However, the Go runtime can easily eat into idle time while this goroutine is
    // blocked in a sleep. For example, slow platforms might spend more time expected in the
    // scheduler. Another example is that a Go runtime background goroutine could run while
    // everything else is idle. Lastly, if a running goroutine is descheduled by the OS, enough
    // time may pass such that the goroutine is ready to wake, even though the runtime couldn't
    // observe itself as idle with nanotime.
    //
    // To deal with all this, we give a half-proc's worth of leniency.
    //
    // We also retry multiple times to deal with the fact that the OS might deschedule us before
    // we yield and go idle. That has a rare enough chance that retries should resolve it.
    // If the issue we expect is happening, it should be persistent.
    var minIdleCPUSeconds = dur.Seconds() * ((float64)Δruntime.GOMAXPROCS(-1) - 0.5D);
    // Let's make sure there's no background scavenge work to do.
    //
    // The runtime.GC calls below ensure the background sweeper
    // will not run during the idle period.
    Δdebug.FreeOSMemory();
    for (nint retries = 0; retries < maxFailures; retries++) {
        // Read 1.
        Δruntime.GC(); // Update /cpu/classes metrics.
        metrics.Read(m1);
        // Sleep.
        time.Sleep(dur);
        // Read 2.
        Δruntime.GC(); // Update /cpu/classes metrics.
        metrics.Read(m2);
        var dt = m2[0].Value.Float64() - m1[0].Value.Float64();
        if (dt >= minIdleCPUSeconds) {
            // All is well. Test passed.
            return;
        }
        failureIdleTimes = append(failureIdleTimes, dt);
    }
    // Try again.
    // We couldn't observe the expected idle time even once.
    foreach (var (i, dt) in failureIdleTimes) {
        Ꮡt.Logf("try %2d: idle time = %.5fs\n"u8, i + 1, dt);
    }
    Ꮡt.Logf("try %d breakdown:\n"u8, len(failureIdleTimes));
    foreach (var (i, _) in names) {
        if (m1[i].Value.Kind() == metrics.KindBad) {
            continue;
        }
        Ꮡt.Logf("\t%s %0.3f\n"u8, names[i], m2[i].Value.Float64() - m1[i].Value.Float64());
    }
    Ꮡt.Errorf(@"time.Sleep did not contribute enough to ""idle"" class: minimum idle time = %.5fs"u8, minIdleCPUSeconds);
}

// Call f() and verify that the correct STW metrics increment. If isGC is true,
// fn triggers a GC STW. Otherwise, fn triggers an other STW.
internal static void testSchedPauseMetrics(ж<testing.T> Ꮡt, Action<ж<testing.T>> fn, bool isGC) {
    var m = new metrics.Sample[]{
        new(Name: "/sched/pauses/stopping/gc:seconds"u8),
        new(Name: "/sched/pauses/stopping/other:seconds"u8),
        new(Name: "/sched/pauses/total/gc:seconds"u8),
        new(Name: "/sched/pauses/total/other:seconds"u8)
    }.slice();
    var stoppingGC = Ꮡ(m, 0);
    var stoppingOther = Ꮡ(m, 1);
    var totalGC = Ꮡ(m, 2);
    var totalOther = Ꮡ(m, 3);
    uint64 sampleCount(ж<metrics.Sample> s) {
        var h = (~s).Value.Float64Histogram();
        uint64 n = default!;
        foreach (var (_, c) in (~h).Counts) {
            n += c;
        }
        return n;
    }
    // Read baseline.
    metrics.Read(m);
    var baselineStartGC = sampleCount(stoppingGC);
    var baselineStartOther = sampleCount(stoppingOther);
    var baselineTotalGC = sampleCount(totalGC);
    var baselineTotalOther = sampleCount(totalOther);
    fn(Ꮡt);
    metrics.Read(m);
    if (isGC){
        {
            var got = sampleCount(stoppingGC); if (got <= baselineStartGC) {
                Ꮡt.Errorf("/sched/pauses/stopping/gc:seconds sample count %d did not increase from baseline of %d"u8, got, baselineStartGC);
            }
        }
        {
            var got = sampleCount(totalGC); if (got <= baselineTotalGC) {
                Ꮡt.Errorf("/sched/pauses/total/gc:seconds sample count %d did not increase from baseline of %d"u8, got, baselineTotalGC);
            }
        }
        {
            var got = sampleCount(stoppingOther); if (got != baselineStartOther) {
                Ꮡt.Errorf("/sched/pauses/stopping/other:seconds sample count %d changed from baseline of %d"u8, got, baselineStartOther);
            }
        }
        {
            var got = sampleCount(totalOther); if (got != baselineTotalOther) {
                Ꮡt.Errorf("/sched/pauses/stopping/other:seconds sample count %d changed from baseline of %d"u8, got, baselineTotalOther);
            }
        }
    } else {
        {
            var got = sampleCount(stoppingGC); if (got != baselineStartGC) {
                Ꮡt.Errorf("/sched/pauses/stopping/gc:seconds sample count %d changed from baseline of %d"u8, got, baselineStartGC);
            }
        }
        {
            var got = sampleCount(totalGC); if (got != baselineTotalGC) {
                Ꮡt.Errorf("/sched/pauses/total/gc:seconds sample count %d changed from baseline of %d"u8, got, baselineTotalGC);
            }
        }
        {
            var got = sampleCount(stoppingOther); if (got <= baselineStartOther) {
                Ꮡt.Errorf("/sched/pauses/stopping/other:seconds sample count %d did not increase from baseline of %d"u8, got, baselineStartOther);
            }
        }
        {
            var got = sampleCount(totalOther); if (got <= baselineTotalOther) {
                Ꮡt.Errorf("/sched/pauses/stopping/other:seconds sample count %d did not increase from baseline of %d"u8, got, baselineTotalOther);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object gomaxprocs1NotSupportedˢ = (@string)"GOMAXPROCS >1 not supported on wasm"u8;
internal static readonly object writeHeapDumpNotˢ = (@string)"WriteHeapDump not supported on js"u8;
internal static readonly @string heapdumptestˢ = "heapdumptest"u8;
internal static readonly object tracingAlreadyEnabledˢ = (@string)"tracing already enabled"u8;

[GoType("dyn")] internal partial struct TestSchedPauseMetrics_tests {
    internal @string name;
    internal bool isGC;
    internal Action<ж<testing.T>> fn;
}

public static void TestSchedPauseMetrics(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        var tests = new TestSchedPauseMetrics_tests[]{
            new(
                name: "runtime.GC"u8,
                isGC: true,
                fn: (ж<testing.T> tΔ1) => {
                    Δruntime.GC();
                }
            ),
            new(
                name: "runtime.GOMAXPROCS"u8,
                fn: (ж<testing.T> tΔ2) => {
                    GoFrame ᒐ = default;
                    try {
                        if (Δruntime.GOARCH == "wasm"u8) {
                            tΔ2.Skip(gomaxprocs1NotSupportedˢ);
                        }
                        nint n = Δruntime.GOMAXPROCS(0);
                        defer(Δruntime.GOMAXPROCS, n, ref ᒐ);
                        Δruntime.GOMAXPROCS(n + 1);
                    }
                    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                    finally { ᒐ.Run(); }
                }
            ),
            new(
                name: "runtime.GoroutineProfile"u8,
                fn: (ж<testing.T> tΔ3) => {
                    array<Δruntime.StackRecord> s = new(1, () => new());
                    Δruntime.GoroutineProfile(s[..]);
                }
            ),
            new(
                name: "runtime.ReadMemStats"u8,
                fn: (ж<testing.T> tΔ4) => {
                    ref var mstats = ref heap(new Δruntime.MemStats(), out var Ꮡmstats);
                    Δruntime.ReadMemStats(Ꮡmstats);
                }
            ),
            new(
                name: "runtime.Stack"u8,
                fn: [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ5) => {
                    array<byte> b = new(64);
                    Δruntime.Stack(b[..], true);
                }
            ),
            new(
                name: "runtime/debug.WriteHeapDump"u8,
                fn: (ж<testing.T> tΔ6) => {
                    GoFrame ᒐ = default;
                    try {
                        if (Δruntime.GOOS == "js"u8) {
                            tΔ6.Skip(writeHeapDumpNotˢ);
                        }
                        var (f, err) = Δos.CreateTemp(tΔ6.TempDir(), heapdumptestˢ);
                        if (err != default!) {
                            tΔ6.Fatalf("os.CreateTemp failed: %v"u8, err);
                        }
                        defer(Δos.Remove, f.Name(), ref ᒐ);
                        var fʗ1 = f;
                        defer(() => fʗ1.Close(), ref ᒐ);
                        Δdebug.WriteHeapDump(f.Fd());
                    }
                    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                    finally { ᒐ.Run(); }
                }
            ),
            new(
                name: "runtime/trace.Start"u8,
                fn: (ж<testing.T> tΔ7) => {
                    if (trace.IsEnabled()) {
                        tΔ7.Skip(tracingAlreadyEnabledˢ);
                    }
                    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
                    {
                        var err = trace.Start(new runtime_test_package.bytes_BufferжWriter(Ꮡbuf)); if (err != default!) {
                            tΔ7.Errorf("trace.Start err got %v want nil"u8, err);
                        }
                    }
                    trace.Stop();
                }
            )
        }.slice();
        // These tests count STW pauses, classified based on whether they're related
        // to the GC or not. Disable automatic GC cycles during the test so we don't
        // have an incidental GC pause when we're trying to observe only
        // non-GC-related pauses. This is especially important for the
        // runtime/trace.Start test, since (as of this writing) that will block
        // until any active GC mark phase completes.
        defer(Δdebug.SetGCPercent, Δdebug.SetGCPercent(-1), ref ᒐ);
        Δruntime.GC();
        foreach (var (_, vᴛ1) in tests) {
            ref var tc = ref heap(new TestSchedPauseMetrics_tests(), out var Ꮡtc);
            tc = vᴛ1;

            var tcʗ1 = tc;
            Ꮡt.Run(tc.name, (ж<testing.T> tΔ8) => {
                testSchedPauseMetrics(tΔ8, tcʗ1.fn, tcʗ1.isGC);
            });
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string godebugˢ = "GODEBUG"u8;
internal static readonly @string runtimecontentionstacksˢ = "runtimecontentionstacks="u8;
internal static readonly @string mutexˢ = "mutex"u8;
internal static readonly @string runtimeLockˢ = "runtime.lock"u8;
internal static readonly @string sample1ˢ = "sample-1"u8;
internal static readonly @string metricˢ = "metric"u8;
internal static readonly @string compareTimersˢ = "compare timers"u8;
internal static readonly @string sample2ˢ = "sample-2"u8;
internal static readonly @string runtimeSemreleaseˢ = "runtime.semrelease"u8;

public static void TestRuntimeLockMetricsAndProfile(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        nint old = Δruntime.SetMutexProfileFraction(0); // enabled during sub-tests
        defer(Δruntime.SetMutexProfileFraction, old, ref ᒐ);
        if (old != 0) {
            Ꮡt.Fatalf("need MutexProfileRate 0, got %d"u8, old);
        }
        {
            @string before = Δos.Getenv(godebugˢ);
            foreach (var (_, s) in strings.Split(before, ","u8)) {
                if (strings.HasPrefix(s, runtimecontentionstacksˢ)) {
                    Ꮡt.Logf("GODEBUG includes explicit setting %q"u8, s);
                }
            }
            defer(() => {
                Δos.Setenv(godebugˢ, before);
            }, ref ᒐ);
            Δos.Setenv(godebugˢ, fmt.Sprintf("%s,runtimecontentionstacks=1"u8, before));
        }
        Ꮡt.Logf("NumCPU %d"u8, Δruntime.NumCPU());
        Ꮡt.Logf("GOMAXPROCS %d"u8, Δruntime.GOMAXPROCS(0));
        {
            nint minCPU = 2; if (Δruntime.NumCPU() < minCPU) {
                Ꮡt.Skipf("creating and observing contention on runtime-internal locks requires NumCPU >= %d"u8, minCPU);
            }
        }
        ж<profile.Profile> loadProfile(ж<testing.T> tΔ1) {
            ref var w = ref heap(new bytes.Buffer(), out var Ꮡw);
            pprof.Lookup(mutexˢ).WriteTo(new runtime_test_package.bytes_BufferжWriter(Ꮡw), 0);
            var (p, err) = profile.Parse(new runtime_test_package.bytes_BufferжReader(Ꮡw));
            if (err != default!) {
                tΔ1.Fatalf("failed to parse profile: %v"u8, err);
            }
            {
                var errΔ1 = p.CheckValid(); if (errΔ1 != default!) {
                    tΔ1.Fatalf("invalid profile: %v"u8, errΔ1);
                }
            }
            return p;
        }
        var loadProfileʗ1 = loadProfile;
        (float64 metricGrowth, float64 profileGrowth, ж<profile.Profile> p) measureDelta(ж<testing.T> tΔ2, Action fn) {
            float64 metricGrowth = default!;
            float64 profileGrowth = default!;
            ж<profile.Profile> p = default!;
            var beforeProfile = loadProfileʗ1(tΔ2);
            var beforeMetrics = new metrics.Sample[]{new(Name: "/sync/mutex/wait/total:seconds"u8)}.slice();
            metrics.Read(beforeMetrics);
            fn();
            var afterProfile = loadProfileʗ1(tΔ2);
            var afterMetrics = new metrics.Sample[]{new(Name: "/sync/mutex/wait/total:seconds"u8)}.slice();
            metrics.Read(afterMetrics);
            int64 sumSamples(ж<profile.Profile> pΔ1, nint i) {
                int64 sum = default!;
                foreach (var (_, s) in (~pΔ1).Sample) {
                    sum += (~s).Value[i];
                }
                return sum;
            }
            metricGrowth = afterMetrics[0].Value.Float64() - beforeMetrics[0].Value.Float64();
            profileGrowth = (float64)(sumSamples(afterProfile, 1) - sumSamples(beforeProfile, 1)) * time.ΔNanosecond.Seconds();
            // The internal/profile package does not support compaction; this delta
            // profile will include separate positive and negative entries.
            p = afterProfile.Copy();
            if (len((~beforeProfile).Sample) > 0) {
                var err = p.Merge(beforeProfile, -1D);
                if (err != default!) {
                    tΔ2.Fatalf("Merge profiles: %v"u8, err);
                }
            }
            return (metricGrowth, profileGrowth, p);
        }
        var measureDeltaʗ1 = measureDelta;
        Func<ж<testing.T>, (float64, float64, int64, int64)> testcase(bool strictTiming, slice<slice<@string>> acceptStacks, nint workers, Func<bool> fn) {
            var measureDeltaʗ2 = measureDeltaʗ1;
            return (ж<testing.T> tΔ3) => {
                float64 metricGrowth = default!;
                float64 profileGrowth = default!;
                int64 n = default!;
                int64 value = default!;
                (metricGrowth, profileGrowth, var p) = measureDeltaʗ2(tΔ3, [MethodImpl(MethodImplOptions.NoInlining)] () => {
                    ref var started = ref heap(new Δsync.WaitGroup(), out var Ꮡstarted);
                    ref var stopped = ref heap(new Δsync.WaitGroup(), out var Ꮡstopped);
                    Ꮡstarted.Add(workers);
                    Ꮡstopped.Add(workers);
                    for (nint i = 0; i < workers; i++) {

                        var w = Ꮡ(new contentionWorker(
                            before: () => {
                                Ꮡstarted.Done();
                                Ꮡstarted.Wait();
                            },
                            after: () => {
                                Ꮡstopped.Done();
                            },
                            fn: fn
                        ));
                        var wʗ1 = w;
                        goǃ(wʗ1.run);
                    }
                    Ꮡstopped.Wait();
                });
                if (profileGrowth == 0D) {
                    tΔ3.Errorf("no increase in mutex profile"u8);
                }
                if (metricGrowth == 0D && strictTiming) {
                    // If the critical section is very short, systems with low timer
                    // resolution may be unable to measure it via nanotime.
                    //
                    // This is sampled at 1 per gTrackingPeriod, but the explicit
                    // runtime.mutex tests create 200 contention events. Observing
                    // zero of those has a probability of (7/8)^200 = 2.5e-12 which
                    // is acceptably low (though the calculation has a tenuous
                    // dependency on cheaprandn being a good-enough source of
                    // entropy).
                    tΔ3.Errorf("no increase in /sync/mutex/wait/total:seconds metric"u8);
                }
                // This comparison is possible because the time measurements in support of
                // runtime/pprof and runtime/metrics for runtime-internal locks are so close
                // together. It doesn't work as well for user-space contention, where the
                // involved goroutines are not _Grunnable the whole time and so need to pass
                // through the scheduler.
                tΔ3.Logf("lock contention growth in runtime/pprof's view  (%fs)"u8, profileGrowth);
                tΔ3.Logf("lock contention growth in runtime/metrics' view (%fs)"u8, metricGrowth);
                acceptStacks = appendꓸꓸꓸ(slice<slice<@string>>(default!), acceptStacks);
                foreach (var (i, vᴛ1) in acceptStacks) {
                    var stk = vᴛ1;

                    if (goexperiment.StaticLockRanking) {
                        if (!slices.ContainsFunc(stk, (@string s) => s == "runtime.systemstack"u8 || s == "runtime.mcall"u8 || s == "runtime.mstart"u8)) {
                            // stk is a call stack that is still on the user stack when
                            // it calls runtime.unlock. Add the extra function that
                            // we'll see, when the static lock ranking implementation of
                            // runtime.unlockWithRank switches to the system stack.
                            stk = appendꓸꓸꓸ(new @string[]{"runtime.unlockWithRank"u8}.slice(), stk);
                        }
                    }
                    acceptStacks[i] = stk;
                }
                slice<slice<@string>> stks = default!;
                var values = GoReflect.WithElemDims(new slice<array<int64>>(len(acceptStacks), () => new(2)), 2);
                foreach (var (_, s) in (~p).Sample) {
                    slice<@string> have = default!;
                    foreach (var (_, loc) in (~s).Location) {
                        foreach (var (_, line) in (~loc).Line) {
                            have = append(have, (~line.Function).Name);
                        }
                    }
                    stks = append(stks, have);
                    foreach (var (i, stk) in acceptStacks) {
                        if (slices.Equal<slice<@string>, @string>(have, stk)) {
                            values[i][0] += (~s).Value[0];
                            values[i][1] += (~s).Value[1];
                        }
                    }
                }
                foreach (var (i, stk) in acceptStacks) {
                    n += values[i][0];
                    value += values[i][1];
                    tΔ3.Logf("stack %v has samples totaling n=%d value=%d"u8, stk, values[i][0], values[i][1]);
                }
                if (n == 0 && value == 0) {
                    tΔ3.Logf("profile:\n%s"u8, p.OrTypedNil());
                    foreach (var (_, have) in stks) {
                        tΔ3.Logf("have stack %v"u8, have);
                    }
                    foreach (var (_, stk) in acceptStacks) {
                        tΔ3.Errorf("want stack %v"u8, stk);
                    }
                }
                return (metricGrowth, profileGrowth, n, value);
            };
        }
        @string name = Ꮡt.Name();
        var testcaseʗ1 = testcase;
        Ꮡt.Run(runtimeLockˢ, (ж<testing.T> tΔ4) => {
            GoFrame ᒐ = default;
            try {
                var mus = new slice<Mutex>(200, () => new(nil));
                ref var needContention = ref heap(new atomic.Int64(), out var ᏑneedContention);
                var delay = 100 * time.Microsecond; // large relative to system noise, for comparison between clocks
                var delayMicros = delay.Microseconds();
                // The goroutine that acquires the lock will only proceed when it
                // detects that its partner is contended for the lock. That will lead to
                // live-lock if anything (such as a STW) prevents the partner goroutine
                // from running. Allowing the contention workers to pause and restart
                // (to allow a STW to proceed) makes it harder to confirm that we're
                // counting the correct number of contention events, since some locks
                // will end up contended twice. Instead, disable the GC.
                defer(Δdebug.SetGCPercent, Δdebug.SetGCPercent(-1), ref ᒐ);
                const nint workers = 2;
                if (Δruntime.GOMAXPROCS(0) < workers) {
                    tΔ4.Skipf("contention on runtime-internal locks requires GOMAXPROCS >= %d"u8, (nint)(workers));
                }
                var musʗ1 = mus;
                var fn = () => {
                    nint n = (nint)ᏑneedContention.Load();
                    if (n < 0) {
                        return false;
                    }
                    var mu = Ꮡ(musʗ1, n);
                    runtime_internal_test_package.ΔLock(mu);
                    while ((nint)ᏑneedContention.Load() == n) {
                        if (runtime_internal_test_package.MutexContended(mu)) {
                            // make them wait a little while
                            for (var start = runtime_internal_test_package.Nanotime(); (runtime_internal_test_package.Nanotime() - start) / 1000 < delayMicros; ) {
                                runtime_internal_test_package.Usleep((uint32)delayMicros);
                            }
                            break;
                        }
                    }
                    runtime_internal_test_package.ΔUnlock(mu);
                    ᏑneedContention.Store((int64)(n - 1));
                    return true;
                };
                var stks = new slice<@string>[]{new @string[]{
                    "runtime.unlock"u8,
                    "runtime_test."u8 + name + ".func5.1"u8,
                    "runtime_test.(*contentionWorker).run"u8}.slice()
                }.slice();
                var fnʗ1 = fn;
                var musʗ2 = mus;
                var stksʗ1 = stks;
                var testcaseʗ2 = testcaseʗ1;
                tΔ4.Run(sample1ˢ, (ж<testing.T> tΔ5) => {
                    GoFrame ᒐ = default;
                    try {
                        nint oldΔ1 = Δruntime.SetMutexProfileFraction(1);
                        defer(Δruntime.SetMutexProfileFraction, oldΔ1, ref ᒐ);
                        ᏑneedContention.Store((int64)(len(musʗ2) - 1));
                        var (metricGrowth, profileGrowth, n, _) = testcaseʗ2(true, stksʗ1, workers, fnʗ1)(tΔ5);
                        var musʗ3 = musʗ2;
                        tΔ5.Run(metricˢ, (ж<testing.T> tΔ6) => {
                            // The runtime/metrics view may be sampled at 1 per
                            // gTrackingPeriod, so we don't have a hard lower bound here.
                            testenv.SkipFlaky(new runtime_test_package.testing_TжTB(tΔ6), 64253);
                            {
                                var (have, want) = (metricGrowth, delay.Seconds() * (float64)len(musʗ3)); if (have < want) {
                                    // The test imposes a delay with usleep, verified with calls to
                                    // nanotime. Compare against the runtime/metrics package's view
                                    // (based on nanotime) rather than runtime/pprof's view (based
                                    // on cputicks).
                                    tΔ6.Errorf("runtime/metrics reported less than the known minimum contention duration (%fs < %fs)"u8, have, want);
                                }
                            }
                        });
                        {
                            var (have, want) = (n, (int64)len(musʗ2)); if (have != want) {
                                tΔ5.Errorf("mutex profile reported contention count different from the known true count (%d != %d)"u8, have, want);
                            }
                        }
                        const float64 slop = 1.5; // account for nanotime vs cputicks
                        tΔ5.Run(compareTimersˢ, (ж<testing.T> tΔ7) => {
                            testenv.SkipFlaky(new runtime_test_package.testing_TжTB(tΔ7), 64253);
                            if (profileGrowth > slop * metricGrowth || metricGrowth > slop * profileGrowth) {
                                tΔ7.Errorf("views differ by more than %fx"u8, (float64)(slop));
                            }
                        });
                    }
                    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                    finally { ᒐ.Run(); }
                });
                var fnʗ2 = fn;
                var musʗ4 = mus;
                var stksʗ2 = stks;
                var testcaseʗ3 = testcaseʗ1;
                tΔ4.Run(sample2ˢ, (ж<testing.T> tΔ8) => {
                    GoFrame ᒐ = default;
                    try {
                        testenv.SkipFlaky(new runtime_test_package.testing_TжTB(tΔ8), 64253);
                        nint oldΔ2 = Δruntime.SetMutexProfileFraction(2);
                        defer(Δruntime.SetMutexProfileFraction, oldΔ2, ref ᒐ);
                        ᏑneedContention.Store((int64)(len(musʗ4) - 1));
                        var (metricGrowth, profileGrowth, n, _) = testcaseʗ3(true, stksʗ2, workers, fnʗ2)(tΔ8);
                        // With 100 trials and profile fraction of 2, we expect to capture
                        // 50 samples. Allow the test to pass if we get at least 20 samples;
                        // the CDF of the binomial distribution says there's less than a
                        // 1e-9 chance of that, which is an acceptably low flakiness rate.
                        UntypedFloat samplingSlop = 2.5;
                        {
                            var (have, want) = (metricGrowth, delay.Seconds() * (float64)len(musʗ4)); if ((float64)samplingSlop * have < want) {
                                // The test imposes a delay with usleep, verified with calls to
                                // nanotime. Compare against the runtime/metrics package's view
                                // (based on nanotime) rather than runtime/pprof's view (based
                                // on cputicks).
                                tΔ8.Errorf("runtime/metrics reported less than the known minimum contention duration (%f * %fs < %fs)"u8, (float64)(samplingSlop), have, want);
                            }
                        }
                        {
                            var (have, want) = (n, (int64)len(musʗ4)); if ((float64)have > (float64)want * (float64)samplingSlop || (float64)want > (float64)have * (float64)samplingSlop) {
                                tΔ8.Errorf("mutex profile reported contention count too different from the expected count (%d far from %d)"u8, have, want);
                            }
                        }
                        const float64 timerSlop = /* 1.5 * samplingSlop */ 3.75; // account for nanotime vs cputicks, plus the two views' independent sampling
                        if (profileGrowth > timerSlop * metricGrowth || metricGrowth > timerSlop * profileGrowth) {
                            tΔ8.Errorf("views differ by more than %fx"u8, (float64)(timerSlop));
                        }
                    }
                    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                    finally { ᒐ.Run(); }
                });
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
        var testcaseʗ4 = testcase;
        Ꮡt.Run(runtimeSemreleaseˢ, (ж<testing.T> tΔ9) => {
            GoFrame ᒐ = default;
            try {
                testenv.SkipFlaky(new runtime_test_package.testing_TжTB(tΔ9), 64253);
                nint oldΔ3 = Δruntime.SetMutexProfileFraction(1);
                defer(Δruntime.SetMutexProfileFraction, oldΔ3, ref ᒐ);
                const nint workers = 3;
                if (Δruntime.GOMAXPROCS(0) < workers) {
                    tΔ9.Skipf("creating and observing contention on runtime-internal semaphores requires GOMAXPROCS >= %d"u8, (nint)(workers));
                }
                ref var sem = ref heap(new uint32(), out var Ꮡsem);
                sem = 1;
                ref var tries = ref heap(new atomic.Int32(), out var Ꮡtries);
                Ꮡtries.Store(10_000_000); // prefer controlled failure to timeout
                ref var sawContention = ref heap(new atomic.Int32(), out var ᏑsawContention);
                int32 need = 1;
                var fn = () => {
                    if (ᏑsawContention.Load() >= need) {
                        return false;
                    }
                    if (Ꮡtries.Add(-1) < 0) {
                        return false;
                    }
                    runtime_internal_test_package.Semacquire(Ꮡsem);
                    runtime_internal_test_package.Semrelease1(Ꮡsem, false, 0);
                    if (runtime_internal_test_package.MutexContended(runtime_internal_test_package.SemRootLock(Ꮡsem))) {
                        ᏑsawContention.Add(1);
                    }
                    return true;
                };
                var stks = new slice<@string>[]{
                    new @string[]{
                        "runtime.unlock"u8,
                        "runtime.semrelease1"u8,
                        "runtime_test.TestRuntimeLockMetricsAndProfile.func6.1"u8,
                        "runtime_test.(*contentionWorker).run"u8}.slice(),
                    new @string[]{
                        "runtime.unlock"u8,
                        "runtime.semacquire1"u8,
                        "runtime.semacquire"u8,
                        "runtime_test.TestRuntimeLockMetricsAndProfile.func6.1"u8,
                        "runtime_test.(*contentionWorker).run"u8}.slice()
                }.slice();
                // Verify that we get call stack we expect, with anything more than zero
                // cycles / zero samples. The duration of each contention event is too
                // small relative to the expected overhead for us to verify its value
                // more directly. Leave that to the explicit lock/unlock test.
                testcaseʗ4(false, stks, workers, fn)(tΔ9);
                {
                    var remaining = Ꮡtries.Load(); if (remaining >= 0) {
                        tΔ9.Logf("finished test early (%d tries remaining)"u8, remaining);
                    }
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// contentionWorker provides cleaner call stacks for lock contention profile tests
[GoType] partial struct contentionWorker {
    internal Action before;
    internal Func<bool> fn;
    internal Action after;
}

internal static void run(this ж<contentionWorker> Ꮡw) {
    GoFrame ᒐ = default;
    try {
        ref var w = ref Ꮡw.DerefOrNull();

        defer(Ꮡw.Value.after, ref ᒐ);
        w.before();
        while (w.fn()) {
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object totalTimeIsZeroˢ = (@string)"total time is zero"u8;
internal static readonly object gcTotalTimeIsZeroˢ = (@string)"GC total time is zero"u8;
internal static readonly object idleTimeIsZeroˢ = (@string)"idle time is zero"u8;

public static void TestCPUStats(ж<testing.T> Ꮡt) {
    // Run a few GC cycles to get some of the stats to be non-zero.
    Δruntime.GC();
    Δruntime.GC();
    Δruntime.GC();
    // Set GOMAXPROCS high then sleep briefly to ensure we generate
    // some idle time.
    nint oldmaxprocs = Δruntime.GOMAXPROCS(10);
    time.Sleep(time.Millisecond);
    Δruntime.GOMAXPROCS(oldmaxprocs);
    var stats = runtime_internal_test_package.ReadCPUStats();
    var gcTotal = stats.GCAssistTime + stats.GCDedicatedTime + stats.GCIdleTime + stats.GCPauseTime;
    if (gcTotal != stats.GCTotalTime) {
        Ꮡt.Errorf("manually computed total does not match GCTotalTime: %d cpu-ns vs. %d cpu-ns"u8, gcTotal, stats.GCTotalTime);
    }
    var scavTotal = stats.ScavengeAssistTime + stats.ScavengeBgTime;
    if (scavTotal != stats.ScavengeTotalTime) {
        Ꮡt.Errorf("manually computed total does not match ScavengeTotalTime: %d cpu-ns vs. %d cpu-ns"u8, scavTotal, stats.ScavengeTotalTime);
    }
    var total = gcTotal + scavTotal + stats.IdleTime + stats.UserTime;
    if (total != stats.TotalTime) {
        Ꮡt.Errorf("manually computed overall total does not match TotalTime: %d cpu-ns vs. %d cpu-ns"u8, total, stats.TotalTime);
    }
    if (total == 0) {
        Ꮡt.Error(totalTimeIsZeroˢ);
    }
    if (gcTotal == 0) {
        Ꮡt.Error(gcTotalTimeIsZeroˢ);
    }
    if (stats.IdleTime == 0) {
        Ꮡt.Error(idleTimeIsZeroˢ);
    }
}

public static partial void TestMetricHeapUnusedLargeObjectOverflow(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // This test makes sure /memory/classes/heap/unused:bytes
    // doesn't overflow when allocating and deallocating large
    // objects. It is a regression test for #67019.
    var done = new channel<EmptyStruct>(0);
    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
    Ꮡwg.Add(1);
    var doneʗ1 = done;
    goǃ(() => {
        GoFrame ᒐ = default;
        try {
            defer(Ꮡwg.Done, ref ᒐ);
            while (ᐧ) {
                foreach (var _ᴛ1 in range(10)) {
                    abi.Escape(new slice<byte>((1 << (int)(20))));
                }
                Δruntime.GC();
                var selᴛ73 = doneʗ1;
                switch (trySelect(ᐸꟷ(selᴛ73, ꓸꓸꓸ))) {
                case 0 when selᴛ73.ꟷᐳ(out _): {
                    return;
                }
                default: {
                    break;
                }}
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
    var s = new metrics.Sample[]{
        new(Name: "/memory/classes/heap/unused:bytes"u8)
    }.slice();
    foreach (var _ᴛ2 in range(1000)) {
        metrics.Read(s);
        if (s[0].Value.Uint64() > ((uint64)1 << (int)(40))) {
            Ꮡt.Errorf("overflow"u8);
            break;
        }
    }
    done.ᐸꟷ(new EmptyStruct());
    Ꮡwg.Wait();
}

} // end runtime_test_package
