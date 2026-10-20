// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.runtime;

using fmt = fmt_package;
using log = log_package;
using os = os_package;
using trace = go.runtime.trace_package;
using go.runtime;
using io = io_package;

partial class trace_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string traceOutˢ = "trace.out"u8;

// Example demonstrates the use of the trace package to trace
// the execution of a Go program. The trace output will be
// written to the file trace.out
public static void Example() {
    GoFrame ᒐ = default;
    try {
        var (f, err) = os.Create(traceOutˢ);
        if (err != default!) {
            log.Fatalf("failed to create trace output file: %v"u8, err);
        }
        var fʗ1 = f;
        defer(() => {
            {
                var errΔ1 = fʗ1.Close(); if (errΔ1 != default!) {
                    log.Fatalf("failed to close trace file: %v"u8, errΔ1);
                }
            }
        }, ref ᒐ);

        {
            var errΔ2 = trace.Start(new os.FileжWriter(f)); if (errΔ2 != default!) {
                log.Fatalf("failed to start trace: %v"u8, errΔ2);
            }
        }
        defer(trace.Stop, ref ᒐ);

        // your program here
        RunMyProgram();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void RunMyProgram() {
    fmt.Printf("this function will be traced"u8);
}

} // end trace_test_package
