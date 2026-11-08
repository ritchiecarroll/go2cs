// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.runtime;

using bytes = bytes_package;
using flag = flag_package;
using static go.runtime.trace_package;
using testing = testing_package;
using time = time_package;
using io = io_package;

partial class trace_test_package {

internal static ж<bool> dumpTraces = flag.Bool("dump-traces", false, "dump traces to a file, even on success");

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object skippingBecauseTestTraceˢ = (@string)"skipping because -test.trace is set"u8;

// This file just contains smoke tests and tests of runtime/trace logic only.
// It doesn't validate the resulting traces. See the internal/trace package for
// more comprehensive end-to-end tests.
public static void TestTraceStartStop(ж<testing.T> t) {
    if (IsEnabled()) {
        t.Skip(skippingBecauseTestTraceˢ);
    }
    var buf = @new<bytes.Buffer>();
    var err = Start(new bytes_BufferжWriter(buf)); if (err != nil) {
        t.Fatalf("failed to start tracing: %v", err);
    }
    Stop();
    nint size = buf.Len();
    if (size == 0) {
        t.Fatalf("trace is empty");
    }
    time.Sleep(100 * time.Millisecond);
    if (size != buf.Len()) {
        t.Fatalf("trace writes after stop: %v -> %v", size, buf.Len());
    }
}

public static void TestTraceDoubleStart(ж<testing.T> t) {
    if (IsEnabled()) {
        t.Skip(skippingBecauseTestTraceˢ);
    }
    Stop();
    var buf = @new<bytes.Buffer>();
    {
        var err = Start(new bytes_BufferжWriter(buf)); if (err != nil) {
            t.Fatalf("failed to start tracing: %v", err);
        }
    }
    {
        var err = Start(new bytes_BufferжWriter(buf)); if (err == nil) {
            t.Fatalf("succeed to start tracing second time");
        }
    }
    Stop();
    Stop();
}

} // end trace_test_package
