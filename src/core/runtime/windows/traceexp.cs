// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

partial class runtime_package {

// expWriter returns a traceWriter that writes into the current M's stream for
// the given experiment.
internal static traceWriter expWriter(this traceLocker tl, traceExperiment exp) {
    return new traceWriter(traceLocker: tl, traceBuf: (~tl.mp).trace.buf[tl.gen % 2][exp], exp: exp);
}

// unsafeTraceExpWriter produces a traceWriter for experimental trace batches
// that doesn't lock the trace. Data written to experimental batches need not
// conform to the standard trace format.
//
// It should only be used in contexts where either:
// - Another traceLocker is held.
// - trace.gen is prevented from advancing.
//
// This does not have the same stack growth restrictions as traceLocker.writer.
//
// buf may be nil.
internal static traceWriter unsafeTraceExpWriter(uintptr gen, ж<traceBuf> Ꮡbuf, traceExperiment exp) {
    return new traceWriter(traceLocker: new traceLocker(gen: gen), traceBuf: Ꮡbuf, exp: exp);
}

// traceExperiment is an enumeration of the different kinds of experiments supported for tracing.
partial struct traceExperiment /*num:uint8*/;

// traceNoExperiment indicates no experiment.
internal static traceExperiment traceNoExperiment => /* iota */ 0;

// traceExperimentAllocFree is an experiment to add alloc/free events to the trace.
internal static traceExperiment traceExperimentAllocFree => 1;

// traceNumExperiments is the number of trace experiments (and 1 higher than
// the highest numbered experiment).
internal static traceExperiment traceNumExperiments => 2;

// Experimental events.
internal static traceEv _ᴛ4ʗ => /* 127 + iota */ 127;

// Experimental events for ExperimentAllocFree.

// Experimental heap span events. IDs map reversibly to base addresses.
internal static traceEv traceEvSpan => 128; // heap span exists [timestamp, id, npages, type/class]
internal static traceEv traceEvSpanAlloc => 129; // heap span alloc [timestamp, id, npages, type/class]
internal static traceEv traceEvSpanFree => 130; // heap span free [timestamp, id]

// Experimental heap object events. IDs map reversibly to addresses.
internal static traceEv traceEvHeapObject => 131; // heap object exists [timestamp, id, type]
internal static traceEv traceEvHeapObjectAlloc => 132; // heap object alloc [timestamp, id, type]
internal static traceEv traceEvHeapObjectFree => 133; // heap object free [timestamp, id]

// Experimental goroutine stack events. IDs map reversibly to addresses.
internal static traceEv traceEvGoroutineStack => 134; // stack exists [timestamp, id, order]
internal static traceEv traceEvGoroutineStackAlloc => 135; // stack alloc [timestamp, id, order]
internal static traceEv traceEvGoroutineStackFree => 136; // stack free [timestamp, id]

} // end runtime_package
