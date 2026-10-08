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

partial struct traceExperiment /*num:uint8*/;

internal static traceExperiment traceNoExperiment => /* iota */ 0;
internal static traceExperiment traceExperimentAllocFree => 1;
internal static traceExperiment traceNumExperiments => 2;

// Experimental events.
internal static traceEv _ᴛ2ʗ => /* 127 + iota */ 127;
// Experimental events for ExperimentAllocFree.

internal static traceEv traceEvSpan => 128; // heap span exists [timestamp, id, npages, type/class]

internal static traceEv traceEvSpanAlloc => 129; // heap span alloc [timestamp, id, npages, type/class]

internal static traceEv traceEvSpanFree => 130; // heap span free [timestamp, id]

internal static traceEv traceEvHeapObject => 131; // heap object exists [timestamp, id, type]

internal static traceEv traceEvHeapObjectAlloc => 132; // heap object alloc [timestamp, id, type]

internal static traceEv traceEvHeapObjectFree => 133; // heap object free [timestamp, id]

internal static traceEv traceEvGoroutineStack => 134; // stack exists [timestamp, id, order]

internal static traceEv traceEvGoroutineStackAlloc => 135; // stack alloc [timestamp, id, order]

internal static traceEv traceEvGoroutineStackFree => 136; // stack free [timestamp, id]

} // end runtime_package
