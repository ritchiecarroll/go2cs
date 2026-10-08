// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

partial class trace_package {

// Frame is a frame in stack traces.
partial struct Frame {
    public uint64 PC;
    public @string Fn;
    public @string File;
    public nint Line;
}

public static UntypedInt FakeP => /* 1000000 + iota */ 1000000;
public static UntypedInt TimerP => 1000001; // depicts timer unblocks
public static UntypedInt NetpollP => 1000002; // depicts network unblocks
public static UntypedInt SyscallP => 1000003; // depicts returns from syscalls
public static UntypedInt GCP => 1000004; // depicts GC state
public static UntypedInt ProfileP => 1000005; // depicts recording of CPU profile samples

// Event types in the trace.
// Verbatim copy from src/runtime/trace.go with the "trace" prefix removed.
public static UntypedInt EvNone => 0; // unused

public static UntypedInt EvBatch => 1; // start of per-P batch of events [pid, timestamp]

public static UntypedInt EvFrequency => 2; // contains tracer timer frequency [frequency (ticks per second)]

public static UntypedInt EvStack => 3; // stack [stack id, number of PCs, array of {PC, func string ID, file string ID, line}]

public static UntypedInt EvGomaxprocs => 4; // current value of GOMAXPROCS [timestamp, GOMAXPROCS, stack id]

public static UntypedInt EvProcStart => 5; // start of P [timestamp, thread id]

public static UntypedInt EvProcStop => 6; // stop of P [timestamp]

public static UntypedInt EvGCStart => 7; // GC start [timestamp, seq, stack id]

public static UntypedInt EvGCDone => 8; // GC done [timestamp]

public static UntypedInt EvSTWStart => 9; // GC mark termination start [timestamp, kind]

public static UntypedInt EvSTWDone => 10; // GC mark termination done [timestamp]

public static UntypedInt EvGCSweepStart => 11; // GC sweep start [timestamp, stack id]

public static UntypedInt EvGCSweepDone => 12; // GC sweep done [timestamp, swept, reclaimed]

public static UntypedInt EvGoCreate => 13; // goroutine creation [timestamp, new goroutine id, new stack id, stack id]

public static UntypedInt EvGoStart => 14; // goroutine starts running [timestamp, goroutine id, seq]

public static UntypedInt EvGoEnd => 15; // goroutine ends [timestamp]

public static UntypedInt EvGoStop => 16; // goroutine stops (like in select{}) [timestamp, stack]

public static UntypedInt EvGoSched => 17; // goroutine calls Gosched [timestamp, stack]

public static UntypedInt EvGoPreempt => 18; // goroutine is preempted [timestamp, stack]

public static UntypedInt EvGoSleep => 19; // goroutine calls Sleep [timestamp, stack]

public static UntypedInt EvGoBlock => 20; // goroutine blocks [timestamp, stack]

public static UntypedInt EvGoUnblock => 21; // goroutine is unblocked [timestamp, goroutine id, seq, stack]

public static UntypedInt EvGoBlockSend => 22; // goroutine blocks on chan send [timestamp, stack]

public static UntypedInt EvGoBlockRecv => 23; // goroutine blocks on chan recv [timestamp, stack]

public static UntypedInt EvGoBlockSelect => 24; // goroutine blocks on select [timestamp, stack]

public static UntypedInt EvGoBlockSync => 25; // goroutine blocks on Mutex/RWMutex [timestamp, stack]

public static UntypedInt EvGoBlockCond => 26; // goroutine blocks on Cond [timestamp, stack]

public static UntypedInt EvGoBlockNet => 27; // goroutine blocks on network [timestamp, stack]

public static UntypedInt EvGoSysCall => 28; // syscall enter [timestamp, stack]

public static UntypedInt EvGoSysExit => 29; // syscall exit [timestamp, goroutine id, seq, real timestamp]

public static UntypedInt EvGoSysBlock => 30; // syscall blocks [timestamp]

public static UntypedInt EvGoWaiting => 31; // denotes that goroutine is blocked when tracing starts [timestamp, goroutine id]

public static UntypedInt EvGoInSyscall => 32; // denotes that goroutine is in syscall when tracing starts [timestamp, goroutine id]

public static UntypedInt EvHeapAlloc => 33; // gcController.heapLive change [timestamp, heap live bytes]

public static UntypedInt EvHeapGoal => 34; // gcController.heapGoal change [timestamp, heap goal bytes]

public static UntypedInt EvTimerGoroutine => 35; // denotes timer goroutine [timer goroutine id]

public static UntypedInt EvFutileWakeup => 36; // denotes that the previous wakeup of this goroutine was futile [timestamp]

public static UntypedInt EvString => 37; // string dictionary entry [ID, length, string]

public static UntypedInt EvGoStartLocal => 38; // goroutine starts running on the same P as the last event [timestamp, goroutine id]

public static UntypedInt EvGoUnblockLocal => 39; // goroutine is unblocked on the same P as the last event [timestamp, goroutine id, stack]

public static UntypedInt EvGoSysExitLocal => 40; // syscall exit on the same P as the last event [timestamp, goroutine id, real timestamp]

public static UntypedInt EvGoStartLabel => 41; // goroutine starts running with label [timestamp, goroutine id, seq, label string id]

public static UntypedInt EvGoBlockGC => 42; // goroutine blocks on GC assist [timestamp, stack]

public static UntypedInt EvGCMarkAssistStart => 43; // GC mark assist start [timestamp, stack]

public static UntypedInt EvGCMarkAssistDone => 44; // GC mark assist done [timestamp]

public static UntypedInt EvUserTaskCreate => 45; // trace.NewTask [timestamp, internal task id, internal parent id, name string, stack]

public static UntypedInt EvUserTaskEnd => 46; // end of task [timestamp, internal task id, stack]

public static UntypedInt EvUserRegion => 47; // trace.WithRegion [timestamp, internal task id, mode(0:start, 1:end), name string, stack]

public static UntypedInt EvUserLog => 48; // trace.Log [timestamp, internal id, key string id, stack, value string]

public static UntypedInt EvCPUSample => 49; // CPU profiling sample [timestamp, real timestamp, real P id (-1 when absent), goroutine id, stack]

public static UntypedInt EvCount => 50;

} // end trace_package
