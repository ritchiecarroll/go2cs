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
    globalPointer = default!;
}

internal static ж<nint> globalPointer;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string objdumpˢ = "objdump"u8;
internal static readonly @string setGlobalPointerˢ = "setGlobalPointer"u8;
internal static readonly @string unsafepointTestGoˢ = "unsafepoint_test.go:"u8;

public static void TestUnsafePoint(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    testenv.MustHaveExec(new runtime_test_package.testing_TжTB(Ꮡt));
    var exprᴛ1 = Δruntime.GOARCH;
    if (exprᴛ1 == "amd64"u8 || exprᴛ1 == "arm64"u8) {
    }
    else { /* default: */
        Ꮡt.Skipf("test not enabled for %s"u8, Δruntime.GOARCH);
    }

    // Get a reference we can use to ask the runtime about
    // which of its instructions are unsafe preemption points.
    var f = Δruntime.FuncForPC(reflect.ValueOf(setGlobalPointer).Pointer());

    // Disassemble the test function.
    // Note that normally "go test runtime" would strip symbols
    // and prevent this step from working. So there's a hack in
    // cmd/go/internal/test that exempts runtime tests from
    // symbol stripping.
    var cmd = exec.Command(testenv.GoToolPath(new runtime_test_package.testing_TжTB(Ꮡt)), toolˢ, objdumpˢ, "-s", setGlobalPointerˢ, Δos.Args[0]);
    var (@out, err) = cmd.CombinedOutput();
    if (err != default!) {
        Ꮡt.Fatalf("can't objdump %v:\n%s"u8, err, @out);
    }
    var lines = strings.Split((@string)@out, "\n"u8)[1..];

    // Walk through assembly instructions, checking preemptible flags.
    uint64 entry = default!;
    bool startedWB = default!;
    bool doneWB = default!;
    nint instructionCount = 0;
    nint unsafeCount = 0;
    foreach (var (_, vᴛ1) in lines) {
        var line = vᴛ1;

        line = strings.TrimSpace(line);
        Ꮡt.Logf("%s"u8, line);
        var parts = strings.Fields(line);
        if (len(parts) < 4) {
            continue;
        }
        if (!strings.HasPrefix(parts[0], unsafepointTestGoˢ)) {
            continue;
        }
        var (pc, errΔ1) = strconv.ParseUint(parts[1][2..], 16, 64);
        if (errΔ1 != default!) {
            Ꮡt.Fatalf("can't parse pc %s: %v"u8, parts[1], errΔ1);
        }
        if (entry == 0) {
            entry = pc;
        }
        // Note that some platforms do ASLR, so the PCs in the disassembly
        // don't match PCs in the address space. Only offsets from function
        // entry make sense.
        var @unsafe = runtime_internal_test_package.UnsafePoint(f.Entry() + (uintptr)(pc - entry));
        Ꮡt.Logf("unsafe: %v\n"u8, @unsafe);
        instructionCount++;
        if (@unsafe) {
            unsafeCount++;
        }

        // All the instructions inside the write barrier must be unpreemptible.
        if (startedWB && !doneWB && !@unsafe) {
            Ꮡt.Errorf("instruction %s must be marked unsafe, but isn't"u8, parts[1]);
        }

        // Detect whether we're in the write barrier.
        var exprᴛ2 = Δruntime.GOARCH;
        if (exprᴛ2 == "arm64"u8) {
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
        }
        else if (exprᴛ2 == "amd64"u8) {
            if (parts[3] == "CMPL") {
                startedWB = true;
            }
            if (parts[3] == "MOVQ" && parts[4] == "$0x0,") {
                doneWB = true;
            }
        }
    }

    if (instructionCount == 0) {
        Ꮡt.Errorf("no instructions"u8);
    }
    if (unsafeCount == instructionCount) {
        Ꮡt.Errorf("no interruptible instructions"u8);
    }
    // Note that there are other instructions marked unpreemptible besides
    // just the ones required by the write barrier. Those include possibly
    // the preamble and postamble, as well as bleeding out from the
    // write barrier proper into adjacent instructions (in both directions).
    // Hopefully we can clean up the latter at some point.
}

} // end runtime_test_package
