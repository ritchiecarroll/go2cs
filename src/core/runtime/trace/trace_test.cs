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

internal static ж<bool> dumpTraces = flag.Bool("dump-traces"u8, false, "dump traces to a file, even on success"u8);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object skippingBecauseTestTraceˢ = (@string)"skipping because -test.trace is set"u8;

// This file just contains smoke tests and tests of runtime/trace logic only.
// It doesn't validate the resulting traces. See the internal/trace package for
// more comprehensive end-to-end tests.
public static void TestTraceStartStop(ж<testing.T> Ꮡt) {
    if (IsEnabled()) {
        Ꮡt.Skip(skippingBecauseTestTraceˢ);
    }
    var buf = @new<bytes.Buffer>();
    {
        var err = Start(new bytes_BufferжWriter(buf)); if (err != default!) {
            Ꮡt.Fatalf("failed to start tracing: %v"u8, err);
        }
    }
    Stop();
    nint size = buf.Len();
    if (size == 0) {
        Ꮡt.Fatalf("trace is empty"u8);
    }
    time.Sleep(100 * time.Millisecond);
    if (size != buf.Len()) {
        Ꮡt.Fatalf("trace writes after stop: %v -> %v"u8, size, buf.Len());
    }
}

public static void TestTraceDoubleStart(ж<testing.T> Ꮡt) {
    if (IsEnabled()) {
        Ꮡt.Skip(skippingBecauseTestTraceˢ);
    }
    Stop();
    var buf = @new<bytes.Buffer>();
    {
        var err = Start(new bytes_BufferжWriter(buf)); if (err != default!) {
            Ꮡt.Fatalf("failed to start tracing: %v"u8, err);
        }
    }
    {
        var err = Start(new bytes_BufferжWriter(buf)); if (err == default!) {
            Ꮡt.Fatalf("succeed to start tracing second time"u8);
        }
    }
    Stop();
    Stop();
}

} // end trace_test_package
