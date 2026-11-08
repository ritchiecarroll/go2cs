// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using bufio = bufio_package;
using bytes = bytes_package;
using fmt = fmt_package;
using testenv = @internal.testenv_package;
using exec = global::go.os.exec_package;
using filepath = path.filepath_package;
using Δruntime = runtime_package;
using strings = strings_package;
using syscall = syscall_package;
using testing = testing_package;
using @internal;
using global::go.os;
using path;
using static global::go.runtime_internal_test_package;
using Δio = io_package;

partial class runtime_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string windowsAmd642012ˢ => ref windowsAmd642012ˢᵇ;
private static readonly @string windowsAmd642012ˢᵇ = "windows-amd64-2012"u8;
internal static ref readonly @string gccˢ => ref gccˢᵇ;
private static readonly @string gccˢᵇ = "gcc"u8;
internal static ref readonly @string vehDllˢ => ref vehDllˢᵇ;
private static readonly @string vehDllˢᵇ = "veh.dll"u8;
internal static ref readonly @string sharedˢ => ref sharedˢᵇ;
private static readonly @string sharedˢᵇ = "-shared"u8;
internal static ref readonly @string testdataTestwinlibthrowˢ => ref testdataTestwinlibthrowˢᵇ;
private static readonly @string testdataTestwinlibthrowˢᵇ = "testdata/testwinlibthrow/veh.c"u8;
internal static ref readonly @string testExeˢ => ref testExeˢᵇ;
private static readonly @string testExeˢᵇ = "test.exe"u8;
internal static ref readonly @string testdataTestwinlibthrowˢ2 => ref testdataTestwinlibthrowˢ2ᵇ;
private static readonly @string testdataTestwinlibthrowˢ2ᵇ = "testdata/testwinlibthrow/main.go"u8;
internal static readonly object errorExpectedˢ = (@string)"error expected"u8;
internal static ref readonly @string threadˢ => ref threadˢᵇ;
private static readonly @string threadˢᵇ = "thread"u8;

public static void TestVectoredHandlerExceptionInNonGoThread(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        if (flagQuick.Value) {
            t.Skip(quickˢ);
        }
        if (strings.HasPrefix(testenv.Builder(), windowsAmd642012ˢ)) {
            testenv.SkipFlaky(new runtime_test_package.testing_TжTB(t), 49681);
        }
        testenv.MustHaveGoBuild(new runtime_test_package.testing_TжTB(t));
        testenv.MustHaveCGO(new runtime_test_package.testing_TжTB(t));
        testenv.MustHaveExecPath(new runtime_test_package.testing_TжTB(t), gccˢ);
        Ꮡtestprog.of(testprogᴛ1.ᏑMutex).Lock();
        defer(Ꮡtestprog.of(testprogᴛ1.ᏑMutex).Unlock, ref ᒐ);
        @string dir = t.TempDir();

        // build c program
        @string dll = filepath.Join(dir, vehDllˢ);
        var cmd = exec.Command(gccˢ, sharedˢ, "-o", dll, testdataTestwinlibthrowˢ);
        var (@out, err) = testenv.CleanCmdEnv(cmd).CombinedOutput();
        if (err != nil) {
            t.Fatalf("failed to build c exe: %s\n%s", err, @out);
        }

        // build go exe
        @string exe = filepath.Join(dir, testExeˢ);
        cmd = exec.Command(testenv.GoToolPath(new runtime_test_package.testing_TжTB(t)), buildˢ, "-o", exe, testdataTestwinlibthrowˢ2);
        (@out, err) = testenv.CleanCmdEnv(cmd).CombinedOutput();
        if (err != nil) {
            t.Fatalf("failed to build go library: %s\n%s", err, @out);
        }

        // run test program in same thread
        cmd = exec.Command(exe);
        (@out, err) = testenv.CleanCmdEnv(cmd).CombinedOutput();
        if (err == nil) {
            t.Fatal(errorExpectedˢ);
        }
        var (_, ok) = err._<ж<exec.ExitError>>(ᐧ); if (ok && len(@out) > 0) {
            if (!bytes.Contains(@out, slice<byte>("Exception 0x2a"u8))) {
                t.Fatalf("unexpected failure while running executable: %s\n%s", err, @out);
            }
        } else {
            t.Fatalf("unexpected error while running executable: %s\n%s", err, @out);
        }
        // run test program in a new thread
        cmd = exec.Command(exe, threadˢ);
        (@out, err) = testenv.CleanCmdEnv(cmd).CombinedOutput();
        if (err == nil) {
            t.Fatal(errorExpectedˢ);
        }
        if (err._<ж<exec.ExitError>>(out var errΔ1)) {
            if (errΔ1.Value.ProcessState.ExitCode() != 42) {
                t.Fatalf("unexpected failure while running executable: %s\n%s", errΔ1.OrTypedNil(), @out);
            }
        } else {
            t.Fatalf("unexpected error while running executable: %s\n%s", errΔ1.OrTypedNil(), @out);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testwinlibDllˢ => ref testwinlibDllˢᵇ;
private static readonly @string testwinlibDllˢᵇ = "testwinlib.dll"u8;
internal static ref readonly @string buildmodeˢ => ref buildmodeˢᵇ;
private static readonly @string buildmodeˢᵇ = "-buildmode"u8;
internal static ref readonly @string cSharedˢ => ref cSharedˢᵇ;
private static readonly @string cSharedˢᵇ = "c-shared"u8;
internal static ref readonly @string testdataTestwinlibMainGoˢ => ref testdataTestwinlibMainGoˢᵇ;
private static readonly @string testdataTestwinlibMainGoˢᵇ = "testdata/testwinlib/main.go"u8;
internal static ref readonly @string ltestwinlibˢ => ref ltestwinlibˢᵇ;
private static readonly @string ltestwinlibˢᵇ = "-ltestwinlib"u8;
internal static ref readonly @string testdataTestwinlibMainCˢ => ref testdataTestwinlibMainCˢᵇ;
private static readonly @string testdataTestwinlibMainCˢᵇ = "testdata/testwinlib/main.c"u8;
internal static ref readonly @string exceptionCount1ˢ => ref exceptionCount1ˢᵇ;
private static readonly @string exceptionCount1ˢᵇ = "exceptionCount: 1\ncontinueCount: 1\nunhandledCount: 0\n"u8;
internal static ref readonly @string exceptionCount1ˢ2 => ref exceptionCount1ˢ2ᵇ;
private static readonly @string exceptionCount1ˢ2ᵇ = "exceptionCount: 1\ncontinueCount: 1\nunhandledCount: 1\n"u8;

public static void TestVectoredHandlerDontCrashOnLibrary(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        if (flagQuick.Value) {
            t.Skip(quickˢ);
        }

        testenv.MustHaveGoBuild(new runtime_test_package.testing_TжTB(t));
        testenv.MustHaveCGO(new runtime_test_package.testing_TжTB(t));
        testenv.MustHaveExecPath(new runtime_test_package.testing_TжTB(t), gccˢ);
        Ꮡtestprog.of(testprogᴛ1.ᏑMutex).Lock();
        defer(Ꮡtestprog.of(testprogᴛ1.ᏑMutex).Unlock, ref ᒐ);
        @string dir = t.TempDir();

        // build go dll
        @string dll = filepath.Join(dir, testwinlibDllˢ);
        var cmd = exec.Command(testenv.GoToolPath(new runtime_test_package.testing_TжTB(t)), buildˢ, "-o", dll, buildmodeˢ, cSharedˢ, testdataTestwinlibMainGoˢ);
        var (@out, err) = testenv.CleanCmdEnv(cmd).CombinedOutput();
        if (err != nil) {
            t.Fatalf("failed to build go library: %s\n%s", err, @out);
        }

        // build c program
        @string exe = filepath.Join(dir, testExeˢ);
        cmd = exec.Command(gccˢ, "-L"u8 + dir, "-I" + dir, ltestwinlibˢ, "-o", exe, testdataTestwinlibMainCˢ);
        (@out, err) = testenv.CleanCmdEnv(cmd).CombinedOutput();
        if (err != nil) {
            t.Fatalf("failed to build c exe: %s\n%s", err, @out);
        }

        // run test program
        cmd = exec.Command(exe);
        (@out, err) = testenv.CleanCmdEnv(cmd).CombinedOutput();
        if (err != nil) {
            t.Fatalf("failure while running executable: %s\n%s", err, @out);
        }
        @string expectedOutput = default;
        if (Δruntime.GOARCH == "arm64"u8) {
            // TODO: remove when windows/arm64 support SEH stack unwinding.
            expectedOutput = exceptionCount1ˢ;
        } else {
            expectedOutput = exceptionCount1ˢ2;
        }
        // cleaning output
        @string cleanedOut = strings.ReplaceAll((@string)@out, "\r\n", "\n");
        if (cleanedOut != expectedOutput) {
            t.Errorf("expected output %q, got %q", expectedOutput, cleanedOut);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string kernel32Dllˢ => ref kernel32Dllˢᵇ;
private static readonly @string kernel32Dllˢᵇ = "kernel32.dll"u8;
internal static ref readonly @string generateConsoleCtrlEventˢ => ref generateConsoleCtrlEventˢᵇ;
private static readonly @string generateConsoleCtrlEventˢᵇ = "GenerateConsoleCtrlEvent"u8;

internal static error sendCtrlBreak(nint pid) {
    var (kernel32, err) = syscall.LoadDLL(kernel32Dllˢ);
    if (err != nil) {
        return fmt.Errorf("LoadDLL: %v\n"u8, err);
    }
    (var generateEvent, err) = kernel32.FindProc(generateConsoleCtrlEventˢ);
    if (err != nil) {
        return fmt.Errorf("FindProc: %v\n"u8, err);
    }
    (var result, _, err) = generateEvent.Call(syscall.CTRL_BREAK_EVENT, (uintptr)pid);
    if (result == 0) {
        return fmt.Errorf("GenerateConsoleCtrlEvent: %v\n"u8, err);
    }
    return default;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testdataTestwinsignalˢ => ref testdataTestwinsignalˢᵇ;
private static readonly @string testdataTestwinsignalˢᵇ = "testdata/testwinsignal/main.go"u8;

// TestCtrlHandler tests that Go can gracefully handle closing the console window.
// See https://golang.org/issues/41884.
public static void TestCtrlHandler(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        testenv.MustHaveGoBuild(new runtime_test_package.testing_TжTB(t));
        t.Parallel();

        // build go program
        @string exe = filepath.Join(t.TempDir(), testExeˢ);
        var cmd = exec.Command(testenv.GoToolPath(new runtime_test_package.testing_TжTB(t)), buildˢ, "-o", exe, testdataTestwinsignalˢ);
        var (@out, err) = testenv.CleanCmdEnv(cmd).CombinedOutput();
        if (err != nil) {
            t.Fatalf("failed to build go exe: %v\n%s", err, @out);
        }

        // run test program
        cmd = exec.Command(exe);
        ref var stdout = ref heap(new strings.Builder(), out var Ꮡstdout);
        ref var stderr = ref heap(new strings.Builder(), out var Ꮡstderr);
        cmd.Stdout = new runtime_test_package.strings_BuilderжWriter(Ꮡstdout);
        cmd.Stderr = new runtime_test_package.strings_BuilderжWriter(Ꮡstderr);
        (var inPipe, err) = cmd.StdinPipe();
        if (err != nil) {
            t.Fatalf("Failed to create stdin pipe: %v", err);
        }
        // keep inPipe alive until the end of the test
        var inPipeʗ1 = inPipe;
        defer(() => inPipeʗ1.Close(), ref ᒐ);

        // in a new command window
        const uint32 _CREATE_NEW_CONSOLE = 0x00000010;
        cmd.SysProcAttr = Ꮡ(new syscall.SysProcAttr(
            CreationFlags: _CREATE_NEW_CONSOLE,
            HideWindow: true
        ));
        var errΔ1 = cmd.Start(); if (errΔ1 != nil) {
            t.Fatalf("Start failed: %v", errΔ1);
        }
        var cmdʗ1 = cmd;
        defer(() => {
            cmdʗ1.Process.Kill();
            cmdʗ1.Wait();
        }, ref ᒐ);

        // check child exited gracefully, did not timeout
        var errΔ2 = cmd.Wait(); if (errΔ2 != nil) {
            t.Fatalf("Program exited with error: %v\n%s", errΔ2, Ꮡstderr);
        }

        // check child received, handled SIGTERM
        @string expected = syscall.SIGTERM.String();
        @string got = strings.TrimSpace(stdout.String()); if (expected != got) {
            t.Fatalf("Expected '%s' got: %s", expected, got);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object thisTestCanOnlyRunOnˢ = (@string)"this test can only run on windows/amd64"u8;
internal static ref readonly @string dummyDllˢ => ref dummyDllˢᵇ;
private static readonly @string dummyDllˢᵇ = "dummy.dll"u8;
internal static ref readonly @string testdataTestwinlibsignalˢ => ref testdataTestwinlibsignalˢᵇ;
private static readonly @string testdataTestwinlibsignalˢᵇ = "testdata/testwinlibsignal/dummy.go"u8;
internal static ref readonly @string testdataTestwinlibsignalˢ2 => ref testdataTestwinlibsignalˢ2ᵇ;
private static readonly @string testdataTestwinlibsignalˢ2ᵇ = "testdata/testwinlibsignal/main.c"u8;

// TestLibraryCtrlHandler tests that Go DLL allows calling program to handle console control events.
// See https://golang.org/issues/35965.
public static partial void TestLibraryCtrlHandler(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        if (flagQuick.Value) {
            t.Skip(quickˢ);
        }
        if (Δruntime.GOARCH != "amd64"u8) {
            t.Skip(thisTestCanOnlyRunOnˢ);
        }
        testenv.MustHaveGoBuild(new runtime_test_package.testing_TжTB(t));
        testenv.MustHaveCGO(new runtime_test_package.testing_TжTB(t));
        testenv.MustHaveExecPath(new runtime_test_package.testing_TжTB(t), gccˢ);
        Ꮡtestprog.of(testprogᴛ1.ᏑMutex).Lock();
        defer(Ꮡtestprog.of(testprogᴛ1.ᏑMutex).Unlock, ref ᒐ);
        @string dir = t.TempDir();

        // build go dll
        @string dll = filepath.Join(dir, dummyDllˢ);
        var cmd = exec.Command(testenv.GoToolPath(new runtime_test_package.testing_TжTB(t)), buildˢ, "-o", dll, buildmodeˢ, cSharedˢ, testdataTestwinlibsignalˢ);
        var (@out, err) = testenv.CleanCmdEnv(cmd).CombinedOutput();
        if (err != nil) {
            t.Fatalf("failed to build go library: %s\n%s", err, @out);
        }

        // build c program
        @string exe = filepath.Join(dir, testExeˢ);
        cmd = exec.Command(gccˢ, "-o", exe, testdataTestwinlibsignalˢ2);
        (@out, err) = testenv.CleanCmdEnv(cmd).CombinedOutput();
        if (err != nil) {
            t.Fatalf("failed to build c exe: %s\n%s", err, @out);
        }

        // run test program
        cmd = exec.Command(exe);
        ref var stderr = ref heap(new bytes.Buffer(), out var Ꮡstderr);
        cmd.Stderr = new runtime_test_package.bytes_BufferжWriter(Ꮡstderr);
        (var outPipe, err) = cmd.StdoutPipe();
        if (err != nil) {
            t.Fatalf("Failed to create stdout pipe: %v", err);
        }
        var outReader = bufio.NewReader(outPipe);

        cmd.SysProcAttr = Ꮡ(new syscall.SysProcAttr(
            CreationFlags: syscall.CREATE_NEW_PROCESS_GROUP
        ));
        var errΔ1 = cmd.Start(); if (errΔ1 != nil) {
            t.Fatalf("Start failed: %v", errΔ1);
        }

        var errCh = new channel<error>(1);
        var cmdʗ1 = cmd;
        var errChʗ1 = errCh;
        var outReaderʗ1 = outReader;
        goǃ(() => {
            var (line, errΔ2) = outReaderʗ1.ReadString((byte)'\n'); if (errΔ2 != nil) {
                errChʗ1.ᐸꟷ(fmt.Errorf("could not read stdout: %v"u8, errΔ2));
            } else if (strings.TrimSpace(line) != "ready"u8) {
                errChʗ1.ᐸꟷ(fmt.Errorf("unexpected message: %v"u8, line));
            } else {
                errChʗ1.ᐸꟷ(sendCtrlBreak(cmdʗ1.Process.Pid));
            }
        });

        var errΔ3 = ᐸꟷ(errCh); if (errΔ3 != nil) {
            t.Fatal(errΔ3);
        }
        var errΔ4 = cmd.Wait(); if (errΔ4 != nil) {
            t.Fatalf("Program exited with error: %v\n%s", errΔ4, Ꮡstderr);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingWindowsOnlyTestˢ = (@string)"skipping windows only test"u8;
internal static ref readonly @string envˢ => ref envˢᵇ;
private static readonly @string envˢᵇ = "env"u8;
internal static ref readonly @string cgoCflagsˢ => ref cgoCflagsˢᵇ;
private static readonly @string cgoCflagsˢᵇ = "CGO_CFLAGS"u8;
internal static ref readonly @string testDllˢ => ref testDllˢᵇ;
private static readonly @string testDllˢᵇ = "test.dll"u8;
internal static ref readonly @string gotestExeˢ => ref gotestExeˢᵇ;
private static readonly @string gotestExeˢᵇ = "gotest.exe"u8;
internal static ref readonly @string testdataTestwintlsMainGoˢ => ref testdataTestwintlsMainGoˢᵇ;
private static readonly @string testdataTestwintlsMainGoˢᵇ = "testdata/testwintls/main.go"u8;
internal static ref readonly @string testdataTestwintlsMainCˢ => ref testdataTestwintlsMainCˢᵇ;
private static readonly @string testdataTestwintlsMainCˢᵇ = "testdata/testwintls/main.c"u8;
internal static ref readonly @string goFuncˢ => ref goFuncˢᵇ;
private static readonly @string goFuncˢᵇ = "GoFunc"u8;

public static void TestIssue59213(ж<testing.T> t) {
    if (Δruntime.GOOS != "windows"u8) {
        t.Skip(skippingWindowsOnlyTestˢ);
    }
    if (flagQuick.Value) {
        t.Skip(quickˢ);
    }
    testenv.MustHaveGoBuild(new runtime_test_package.testing_TжTB(t));
    testenv.MustHaveCGO(new runtime_test_package.testing_TжTB(t));

    @string goEnv(@string arg) {
        var cmdΔ1 = testenv.Command(new runtime_test_package.testing_TжTB(t), testenv.GoToolPath(new runtime_test_package.testing_TжTB(t)), envˢ, arg);
        cmdΔ1.Stderr = new runtime_test_package.bytes_BufferжWriter(@new<bytes.Buffer>());

        var (line, errΔ1) = cmdΔ1.Output();
        if (errΔ1 != nil) {
            t.Fatalf("%v: %v\n%s", cmdΔ1.OrTypedNil(), errΔ1, cmdΔ1.Stderr);
        }
        @string outΔ1 = (@string)bytes.TrimSpace(line);
        t.Logf("%v: %q", cmdΔ1.OrTypedNil(), outΔ1);
        return outΔ1;
    }

    @string cc = goEnv("CC");
    @string cgoCflags = goEnv(cgoCflagsˢ);

    t.Parallel();

    @string tmpdir = t.TempDir();
    @string dllfile = filepath.Join(tmpdir, testDllˢ);
    @string exefile = filepath.Join(tmpdir, gotestExeˢ);

    // build go dll
    var cmd = testenv.Command(new runtime_test_package.testing_TжTB(t), testenv.GoToolPath(new runtime_test_package.testing_TжTB(t)), buildˢ, "-o", dllfile, buildmodeˢ, cSharedˢ, testdataTestwintlsMainGoˢ);
    var (@out, err) = testenv.CleanCmdEnv(cmd).CombinedOutput();
    if (err != nil) {
        t.Fatalf("failed to build go library: %s\n%s", err, @out);
    }

    // build c program
    cmd = testenv.Command(new runtime_test_package.testing_TжTB(t), cc, "-o", exefile, testdataTestwintlsMainCˢ);
    testenv.CleanCmdEnv(cmd);
    cmd.Env = append(cmd.Env, "CGO_CFLAGS="u8 + cgoCflags);
    (@out, err) = cmd.CombinedOutput();
    if (err != nil) {
        t.Fatalf("failed to build c exe: %s\n%s", err, @out);
    }

    // run test program
    cmd = testenv.Command(new runtime_test_package.testing_TжTB(t), exefile, dllfile, goFuncˢ);
    (@out, err) = testenv.CleanCmdEnv(cmd).CombinedOutput();
    if (err != nil) {
        t.Fatalf("failed: %s\n%s", err, @out);
    }
}

} // end runtime_test_package
