// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

using go;
using go.golib;

// Hand-finished conversion of trace.go's StartTrace, StopTrace and ReadTrace — the platform-neutral
// managed tracer seam. This file exists as TWO copies, runtime/windows/trace_impl.cs and
// runtime/linux/trace_impl.cs, and the copies are byte-identical BY CONTRACT: a change to one is a
// change to both (the routing note at the end of this header says why).
//
// Go's execution tracer is a serialization of the scheduler: the converted StartTrace stops the
// world through semacquire, whose first step is getg, and the converted ReadTrace parks the reader
// on the tracer's own buffers -- machinery the CLR does not have. Since 2026-09-27 (Q28,
// docs/phase4/DESIGN-managed-execution-tracer.md and its amendment) the three entry points drive
// golib's ExecutionTracer instead: Go's v2 trace format ("go 1.23 trace", the version the go1.24.13
// runtime writes), carrying the goroutine facts the managed runtime observes -- create, start, block
// with Go's trace block reason, unblock, destroy -- on a model of one P per goroutine thread, with no
// GC, heap, syscall, steal or CPU-sample events and no stacks. Go's own parser (`go tool trace
// -d=parsed`) is the acceptance oracle. Until then StartTrace answered a named tracing-not-supported
// error (the measured consumers were runtime's TestCrashWhileTracing on windows and os/signal's
// TestSignalTrace on linux, whose readings move with this change).
//
// Registration and routing. The three names are registered goosWindowsLinux in manualConversionFuncs
// (manualTypeOperations.go): StartTrace and StopTrace since 2026-09-02, ReadTrace since 2026-09-27. On
// each of those two targets the -stdlib emission drops the auto bodies in <goos>/trace.cs to
// placeholders and this file supplies them; the darwin emission displaces none of them (darwin/trace.cs
// keeps the auto bodies), so no darwin copy exists. Layout L3 routes a hand-own by DISPLACEMENT, not by
// where its principal is built (handOwnEmitters in platformHandOwn.go, pinned by
// platformHandOwn_test.go): trace.cs is emitted per-GOOS on all three targets, but a target needs this
// companion exactly when its own trace.cs carries the placeholders -- windows and linux -- so the
// three-target merge places one copy in each of those two folders and refuses, by raw byte comparison,
// to choose between two hand-maintained copies that differ. Nothing here is platform-specific, which is
// why one header serves both copies verbatim.

[module: GoManualConversion]

namespace go;

partial class runtime_package
{
    // StartTrace enables tracing for the current process.
    // While tracing, the data will be buffered and available via [runtime.ReadTrace].
    // StartTrace returns an error if tracing is already enabled.
    // Most clients should use the [runtime/trace] package or the [testing] package's
    // -test.trace flag instead of calling StartTrace directly.
    public static error StartTrace()
    {
        // Go's own text for the one refusal: a trace is running (or its data is still being read). Not a
        // conditional expression: `ok ? default! : (errorString)...` types as errorString, and its default
        // boxes to a NON-nil error whose text is empty -- the first cut did exactly that.
        if (ExecutionTracer.Start())
            return default!;

        return ((errorString)("tracing is already enabled"u8));
    }

    // StopTrace stops tracing, if it was previously enabled.
    // StopTrace only returns after all the reads for the trace have completed.
    public static void StopTrace()
    {
        ExecutionTracer.Stop();
    }

    // ReadTrace returns the next chunk of binary tracing data, blocking until data
    // is available. If tracing is turned off and all the data accumulated while it
    // was on has been returned, ReadTrace returns nil. The caller must copy the
    // returned data before calling ReadTrace again.
    // ReadTrace must be called from one goroutine at a time.
    public static slice<byte> ReadTrace()
    {
        return ExecutionTracer.Read() is { } chunk ? new slice<byte>(chunk) : default!;
    }
}
