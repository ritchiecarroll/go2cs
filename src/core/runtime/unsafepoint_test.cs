// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using testenv = @internal.testenv_package;
using Δos = os_package;
using exec = global::go.os.exec_package;
using reflect = reflect_package;
using Δruntime = runtime_package;
using strconv = strconv_package;
using strings = strings_package;
using testing = testing_package;
using @internal;
using global::go.os;
using static global::go.runtime_internal_test_package;

partial class runtime_test_package {

// This is the function we'll be testing.
// It has a simple write barrier in it.
internal static void setGlobalPointer() {
    globalPointer = default;
}

internal static ж<nint> globalPointer;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string objdumpˢ => ref objdumpˢᵇ;
private static readonly @string objdumpˢᵇ = "objdump"u8;
internal static ref readonly @string setGlobalPointerˢ => ref setGlobalPointerˢᵇ;
private static readonly @string setGlobalPointerˢᵇ = "setGlobalPointer"u8;
internal static ref readonly @string unsafepointTestGoˢ => ref unsafepointTestGoˢᵇ;
private static readonly @string unsafepointTestGoˢᵇ = "unsafepoint_test.go:"u8;

public static void TestUnsafePoint(ж<testing.T> t) {
    testenv.MustHaveExec(new runtime_test_package.testing_TжTB(t));
    var exprᴛ1 = Δruntime.GOARCH;
    switch (ᐧ) {
    case {} when exprᴛ1 == "amd64"u8 || exprᴛ1 == "arm64"u8: {
        break;
    }
    default: {
        t.Skipf("test not enabled for %s", Δruntime.GOARCH);
        break;
    }}

    // Get a reference we can use to ask the runtime about
    // which of its instructions are unsafe preemption points.
    var f = Δruntime.FuncForPC(reflect.ValueOf(setGlobalPointer).Pointer());

    // Disassemble the test function.
    // Note that normally "go test runtime" would strip symbols
    // and prevent this step from working. So there's a hack in
    // cmd/go/internal/test that exempts runtime tests from
    // symbol stripping.
    var cmd = exec.Command(testenv.GoToolPath(new runtime_test_package.testing_TжTB(t)), toolˢ, objdumpˢ, "-s", setGlobalPointerˢ, Δos.Args[0]);
    var (@out, err) = cmd.CombinedOutput();
    if (err != nil) {
        t.Fatalf("can't objdump %v:\n%s", err, @out);
    }
    var lines = strings.Split((@string)@out, "\n")[1..];

    // Walk through assembly instructions, checking preemptible flags.
    uint64 entry = default;
    bool startedWB = default;
    bool doneWB = default;
    nint instructionCount = 0;
    nint unsafeCount = 0;
    foreach (var (_, vᴛ1) in range(lines)) {
        var line = vᴛ1;

        line = strings.TrimSpace(line);
        t.Logf("%s", line);
        var parts = strings.Fields(line);
        if (len(parts) < 4) {
            continue;
        }
        if (!strings.HasPrefix(parts[0], unsafepointTestGoˢ)) {
            continue;
        }
        var (pc, errΔ1) = strconv.ParseUint(parts[1][2..], 16, 64);
        if (errΔ1 != nil) {
            t.Fatalf("can't parse pc %s: %v", parts[1], errΔ1);
        }
        if (entry == 0) {
            entry = pc;
        }
        // Note that some platforms do ASLR, so the PCs in the disassembly
        // don't match PCs in the address space. Only offsets from function
        // entry make sense.
        var @unsafe = runtime_internal_test_package.UnsafePoint(f.Entry() + (uintptr)(pc - entry));
        t.Logf("unsafe: %v\n", @unsafe);
        instructionCount++;
        if (@unsafe) {
            unsafeCount++;
        }

        // All the instructions inside the write barrier must be unpreemptible.
        if (startedWB && !doneWB && !@unsafe) {
            t.Errorf("instruction %s must be marked unsafe, but isn't", parts[1]);
        }

        // Detect whether we're in the write barrier.
        var exprᴛ2 = Δruntime.GOARCH;
        switch (ᐧ) {
        case {} when exprᴛ2 == "arm64"u8: {
            if (parts[3] == "MOVWU") {
                // The unpreemptible region starts after the
                // load of runtime.writeBarrier.
                startedWB = true;
            }
            if (parts[3] == "MOVD" && parts[4] == "ZR,") {
                // The unpreemptible region ends after the
                // write of nil.
                doneWB = true;
            }
            break;
        }
        case {} when exprᴛ2 == "amd64"u8: {
            if (parts[3] == "CMPL") {
                startedWB = true;
            }
            if (parts[3] == "MOVQ" && parts[4] == "$0x0,") {
                doneWB = true;
            }
            break;
        }}
    }

    if (instructionCount == 0) {
        t.Errorf("no instructions");
    }
    if (unsafeCount == instructionCount) {
        t.Errorf("no interruptible instructions");
    }
    // Note that there are other instructions marked unpreemptible besides
    // just the ones required by the write barrier. Those include possibly
    // the preamble and postamble, as well as bleeding out from the
    // write barrier proper into adjacent instructions (in both directions).
    // Hopefully we can clean up the latter at some point.
}

} // end runtime_test_package
