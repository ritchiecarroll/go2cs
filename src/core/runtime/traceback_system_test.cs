// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

// This test of GOTRACEBACK=system has its own file,
// to minimize line-number perturbation.
using bytes = bytes_package;
using fmt = fmt_package;
using testenv = @internal.testenv_package;
using Δio = io_package;
using Δos = os_package;
using filepath = path.filepath_package;
using reflect = reflect_package;
using Δruntime = runtime_package;
using Δdebug = global::go.runtime.debug_package;
using strconv = strconv_package;
using strings = strings_package;
using testing = testing_package;
using @internal;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using exec = global::go.os.exec_package;
using global::go.os;
using global::go.runtime;
using path;
using static global::go.runtime_internal_test_package;

partial class runtime_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string systemˢ = "system"u8;

// This is the entrypoint of the child process used by
// TestTracebackSystem/panic. It prints a crash report to stdout.
internal static partial void crashViaPanic() {
    // Ensure that we get pc=0x%x values in the traceback.
    Δdebug.SetTraceback(systemˢ);
    writeSentinel(new Δos.FileжWriter(Δos.Stdout));
    Δdebug.SetCrashOutput(Δos.Stdout, new Δdebug.CrashOptions(nil));
    goǃ(() => {
        // This call is typically inlined.
        child1();
    });
    select();
}

// This is the entrypoint of the child process used by
// TestTracebackSystem/trap. It prints a crash report to stdout.
internal static partial void crashViaTrap() {
    // Ensure that we get pc=0x%x values in the traceback.
    Δdebug.SetTraceback(systemˢ);
    writeSentinel(new Δos.FileжWriter(Δos.Stdout));
    Δdebug.SetCrashOutput(Δos.Stdout, new Δdebug.CrashOptions(nil));
    goǃ(() => {
        // This call is typically inlined.
        trap1();
    });
    select();
}

internal static void child1() {
    child2();
}

internal static void child2() {
    child3();
}

internal static void child3() {
    child4();
}

internal static void child4() {
    child5();
}

//go:noinline
internal static partial void child5() {
    // test trace through second of two call instructions
    child6bad();
    child6(); // appears in stack trace
}

//go:noinline
internal static partial void child6bad() {
}

//go:noinline
internal static partial void child6() {
    // test trace through first of two call instructions
    child7(); // appears in stack trace
    child7bad();
}

//go:noinline
internal static partial void child7bad() {
}

//go:noinline
internal static partial void child7() {
    // Write runtime.Caller's view of the stack to stderr, for debugging.
    array<uintptr> pcs = new(16);
    nint n = Δruntime.Callers(1, pcs[..]);
    fmt.Fprintf(new Δos.FileжWriter(Δos.Stderr), "Callers: %#x\n"u8, pcs.slice(0, n));
    Δio.WriteString(new Δos.FileжWriter(Δos.Stderr), formatStack(pcs.slice(0, n)));
    // Cause the crash report to be written to stdout.
    throw panic("oops");
}

internal static void trap1() {
    trap2();
}

internal static ж<nint> sinkPtr;

internal static void trap2() {
    trap3(sinkPtr);
}

internal static void trap3(ж<nint> Ꮡi) {
    ref var i = ref Ꮡi.DerefOrNull();

    i = 42;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object canTReadSourceCodeForˢ = (@string)"Can't read source code for this file on Android"u8;

internal partial struct TestTracebackSystem_tests /*dyn*/ {
    internal @string name;
    internal @string want;
}

// TestTracebackSystem tests that the syntax of crash reports produced
// by GOTRACEBACK=system (see traceback2) contains a complete,
// parseable list of program counters for the running goroutine that
// can be parsed and fed to runtime.CallersFrames to obtain accurate
// information about the logical call stack, even in the presence of
// inlining.
//
// The test is a distillation of the crash monitor in
// golang.org/x/telemetry/crashmonitor.
public static void TestTracebackSystem(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    testenv.MustHaveExec(new runtime_test_package.testing_TжTB(Ꮡt));
    if (Δruntime.GOOS == "android"u8) {
        Ꮡt.Skip(canTReadSourceCodeForˢ);
    }
    var tests = new TestTracebackSystem_tests[]{
        new(
            name: "panic"u8,
            want: """
redacted.go:0: runtime.gopanic
traceback_system_test.go:100: runtime_test.child7: 	panic("oops")
traceback_system_test.go:83: runtime_test.child6: 	child7() // appears in stack trace
traceback_system_test.go:74: runtime_test.child5: 	child6() // appears in stack trace
traceback_system_test.go:68: runtime_test.child4: 	child5()
traceback_system_test.go:64: runtime_test.child3: 	child4()
traceback_system_test.go:60: runtime_test.child2: 	child3()
traceback_system_test.go:56: runtime_test.child1: 	child2()
traceback_system_test.go:35: runtime_test.crashViaPanic.func1: 		child1()
redacted.go:0: runtime.goexit

"""u8
        ),
        new(
            name: "trap"u8, // Test panic via trap. x/telemetry is aware that trap
 // PCs follow runtime.sigpanic and need to be
 // incremented to offset the decrement done by
 // CallersFrames.

            want: """
redacted.go:0: runtime.gopanic
redacted.go:0: runtime.panicmem
redacted.go:0: runtime.sigpanic
traceback_system_test.go:114: runtime_test.trap3: 	*i = 42
traceback_system_test.go:110: runtime_test.trap2: 	trap3(sinkPtr)
traceback_system_test.go:104: runtime_test.trap1: 	trap2()
traceback_system_test.go:50: runtime_test.crashViaTrap.func1: 		trap1()
redacted.go:0: runtime.goexit

"""u8
        )
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var tc = ref heap(new TestTracebackSystem_tests(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        Ꮡt.Run(tc.name, (ж<testing.T> tΔ1) => {
            // Fork+exec the crashing process.
            var (exe, err) = Δos.Executable();
            if (err != default!) {
                tΔ1.Fatal(err);
            }
            var cmd = testenv.Command(new runtime_test_package.testing_TжTB(tΔ1), exe);
            cmd.Value.Env = append(cmd.Environ(), entrypointVar + "=" + tcʗ1.name);
            ref var stdout = ref heap(new bytes.Buffer(), out var Ꮡstdout);
            ref var stderr = ref heap(new bytes.Buffer(), out var Ꮡstderr);
            cmd.Value.Stdout = new runtime_test_package.bytes_BufferжWriter(Ꮡstdout);
            cmd.Value.Stderr = new runtime_test_package.bytes_BufferжWriter(Ꮡstderr);
            cmd.Run(); // expected to crash
            tΔ1.Logf("stderr:\n%s\nstdout: %s\n"u8, stderr.Bytes(), stdout.Bytes());
            @string crash = Ꮡstdout.String();
            // If the only line is the sentinel, it wasn't a crash.
            if (strings.Count(crash, "\n"u8) < 2) {
                tΔ1.Fatalf("child process did not produce a crash report"u8);
            }
            // Parse the PCs out of the child's crash report.
            (var pcs, err) = parseStackPCs(crash);
            if (err != default!) {
                tΔ1.Fatal(err);
            }
            // Unwind the stack using this executable's symbol table.
            @string got = formatStack(pcs);
            if (strings.TrimSpace(got) != strings.TrimSpace(tcʗ1.want)) {
                tΔ1.Errorf("got:\n%swant:\n%s"u8, got, tcʗ1.want);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string sentinelˢ = "sentinel "u8;
internal static readonly @string runningˢ = " [running]:"u8;
internal static readonly @string createdByˢ = "created by "u8;

// parseStackPCs parses the parent process's program counters for the
// first running goroutine out of a GOTRACEBACK=system traceback,
// adjusting them so that they are valid for the child process's text
// segment.
//
// This function returns only program counter values, ensuring that
// there is no possibility of strings from the crash report (which may
// contain PII) leaking into the telemetry system.
//
// (Copied from golang.org/x/telemetry/crashmonitor.parseStackPCs.)
internal static (slice<uintptr>, error) parseStackPCs(@string crash) {
    // getSymbol parses the symbol name out of a line of the form:
    // SYMBOL(ARGS)
    //
    // Note: SYMBOL may contain parens "pkg.(*T).method". However, type
    // parameters are always replaced with ..., so they cannot introduce
    // more parens. e.g., "pkg.(*T[...]).method".
    //
    // ARGS can contain parens. We want the first paren that is not
    // immediately preceded by a ".".
    //
    // TODO(prattmic): This is mildly complicated and is only used to find
    // runtime.sigpanic, so perhaps simplify this by checking explicitly
    // for sigpanic.
    (@string, error) getSymbol(@string line) {
        rune prev = default!;
        foreach (var (i, c) in line) {
            if (line[i] != (rune)'(') {
                prev = c;
                continue;
            }
            if (prev == (rune)'.') {
                prev = c;
                continue;
            }
            return (line.slice(0, i), default!);
        }
        return ("", fmt.Errorf("no symbol for stack frame: %s"u8, line));
    }
    // getPC parses the PC out of a line of the form:
    //     \tFILE:LINE +0xRELPC sp=... fp=... pc=...
    (uint64, error) getPC(@string line) {
        var (_, pcstr, ok) = strings.Cut(line, " pc="u8); // e.g. pc=0x%x
        if (!ok) {
            return ((uint64)(0), fmt.Errorf("no pc= for stack frame: %s"u8, line));
        }
        return strconv.ParseUint(pcstr, 0, 64); // 0 => allow 0x prefix
    }
    slice<uintptr> pcs = default!;
    ref var parentSentinel = ref heap(new uint64(), out var ᏑparentSentinel);
    uint64 childSentinel = sentinel();
    bool on = false;            // are we in the first running goroutine?
    slice<@string> lines = strings.Split(crash, "\n"u8);
    bool symLine = true;       // within a goroutine, every other line is a symbol or file/line/pc location, starting with symbol.
    @string currSymbol = default!;
    @string prevSymbol = default!;        // symbol of the most recent previous frame with a PC.
    for (nint i = 0; i < len(lines); i++) {
        @string line = lines[i];
        // Read sentinel value.
        if (parentSentinel == 0 && strings.HasPrefix(line, sentinelˢ)) {
            var (_, err) = fmt.Sscanf(line, "sentinel %x"u8, ᏑparentSentinel);
            if (err != default!) {
                return (default!, fmt.Errorf("can't read sentinel line"u8));
            }
            continue;
        }
        // Search for "goroutine GID [STATUS]"
        if (!on) {
            if (strings.HasPrefix(line, goroutineˢ) && strings.Contains(line, runningˢ)) {
                on = true;
                if (parentSentinel == 0) {
                    return (default!, fmt.Errorf("no sentinel value in crash report"u8));
                }
            }
            continue;
        }
        // A blank line marks end of a goroutine stack.
        if (line == ""u8) {
            break;
        }
        // Skip the final "created by SYMBOL in goroutine GID" part.
        if (strings.HasPrefix(line, createdByˢ)) {
            break;
        }
        // Expect a pair of lines:
        //   SYMBOL(ARGS)
        //   \tFILE:LINE +0xRELPC sp=0x%x fp=0x%x pc=0x%x
        // Note: SYMBOL may contain parens "pkg.(*T).method"
        // The RELPC is sometimes missing.
        if (symLine){
            error err = default!;
            (currSymbol, err) = getSymbol(line);
            if (err != default!) {
                return (default!, fmt.Errorf("error extracting symbol: %v"u8, err));
            }
            symLine = false; // Next line is FILE:LINE.
        } else {
            // Parse the PC, and correct for the parent and child's
            // different mappings of the text section.
            var (pc, err) = getPC(line);
            if (err != default!) {
                // Inlined frame, perhaps; skip it.
                // Done with this frame. Next line is a new frame.
                //
                // Don't update prevSymbol; we only want to
                // track frames with a PC.
                currSymbol = ""u8;
                symLine = true;
                continue;
            }
            pc = pc - parentSentinel + childSentinel;
            // If the previous frame was sigpanic, then this frame
            // was a trap (e.g., SIGSEGV).
            //
            // Typically all middle frames are calls, and report
            // the "return PC". That is, the instruction following
            // the CALL where the callee will eventually return to.
            //
            // runtime.CallersFrames is aware of this property and
            // will decrement each PC by 1 to "back up" to the
            // location of the CALL, which is the actual line
            // number the user expects.
            //
            // This does not work for traps, as a trap is not a
            // call, so the reported PC is not the return PC, but
            // the actual PC of the trap.
            //
            // runtime.Callers is aware of this and will
            // intentionally increment trap PCs in order to correct
            // for the decrement performed by
            // runtime.CallersFrames. See runtime.tracebackPCs and
            // runtume.(*unwinder).symPC.
            //
            // We must emulate the same behavior, otherwise we will
            // report the location of the instruction immediately
            // prior to the trap, which may be on a different line,
            // or even a different inlined functions.
            //
            // TODO(prattmic): The runtime applies the same trap
            // behavior for other "injected calls", see injectCall
            // in runtime.(*unwinder).next. Do we want to handle
            // those as well? I don't believe we'd ever see
            // runtime.asyncPreempt or runtime.debugCallV2 in a
            // typical crash.
            if (prevSymbol == "runtime.sigpanic"u8) {
                pc++;
            }
            pcs = append(pcs, (uintptr)pc);
            // Done with this frame. Next line is a new frame.
            prevSymbol = currSymbol;
            currSymbol = ""u8;
            symLine = true;
        }
    }
    return (pcs, default!);
}

// The sentinel function returns its address. The difference between
// this value as observed by calls in two different processes of the
// same executable tells us the relative offset of their text segments.
//
// It would be nice if SetCrashOutput took care of this as it's fiddly
// and likely to confuse every user at first.
internal static uint64 sentinel() {
    return (uint64)reflect.ValueOf(sentinel).Pointer();
}

internal static void writeSentinel(Δio.Writer @out) {
    fmt.Fprintf(@out, "sentinel %x\n"u8, sentinel());
}

// formatStack formats a stack of PC values using the symbol table,
// redacting information that cannot be relied upon in the test.
internal static @string formatStack(slice<uintptr> pcs) {
    // When debugging, show file/line/content of files other than this one.
    const bool debug = false;
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    nint i = 0;
    var frames = Δruntime.CallersFrames(pcs);
    while (ᐧ) {
        var (fr, more) = frames.Next();
        if (debug) {
            fmt.Fprintf(new runtime_test_package.strings_BuilderжWriter(Ꮡbuf), "pc=%x "u8, pcs[i]);
            i++;
        }
        {
            @string @base = filepath.Base(fr.File); if (@base == "traceback_system_test.go"u8 || debug){
                var (content, err) = Δos.ReadFile(fr.File);
                if (err != default!) {
                    throw panic(err);
                }
                var lines = bytes.Split(content, slice<byte>("\n"u8));
                fmt.Fprintf(new runtime_test_package.strings_BuilderжWriter(Ꮡbuf), "%s:%d: %s: %s\n"u8, @base, fr.Line, fr.Function, lines[fr.Line - 1]);
            } else {
                // For robustness, don't show file/line for functions from other files.
                fmt.Fprintf(new runtime_test_package.strings_BuilderжWriter(Ꮡbuf), "redacted.go:0: %s\n"u8, fr.Function);
            }
        }
        if (!more) {
            break;
        }
    }
    return buf.String();
}

} // end runtime_test_package
