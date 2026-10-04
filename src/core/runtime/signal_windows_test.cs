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
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using global::go.os;
using path;
using static global::go.runtime_internal_test_package;
using Δio = io_package;

partial class runtime_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string windowsAmd642012ˢ = "windows-amd64-2012"u8;
internal static readonly @string gccˢ = "gcc"u8;
internal static readonly @string vehDllˢ = "veh.dll"u8;
internal static readonly @string sharedˢ = "-shared"u8;
internal static readonly @string testdataTestwinlibthrowˢ = "testdata/testwinlibthrow/veh.c"u8;
internal static readonly @string testExeˢ = "test.exe"u8;
internal static readonly @string testdataTestwinlibthrowˢ2 = "testdata/testwinlibthrow/main.go"u8;
internal static readonly object errorExpectedˢ = (@string)"error expected"u8;
internal static readonly @string threadˢ = "thread"u8;

public static void TestVectoredHandlerExceptionInNonGoThread(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        if (flagQuick.Value) {
            Ꮡt.Skip(quickˢ);
        }
        if (strings.HasPrefix(testenv.Builder(), windowsAmd642012ˢ)) {
            testenv.SkipFlaky(new runtime_test_package.testing_TжTB(Ꮡt), 49681);
        }
        testenv.MustHaveGoBuild(new runtime_test_package.testing_TжTB(Ꮡt));
        testenv.MustHaveCGO(new runtime_test_package.testing_TжTB(Ꮡt));
        testenv.MustHaveExecPath(new runtime_test_package.testing_TжTB(Ꮡt), gccˢ);
        Ꮡtestprog.of(testprogᴛ1.ᏑMutex).Lock();
        defer(Ꮡtestprog.of(testprogᴛ1.ᏑMutex).Unlock, ref ᒐ);
        @string dir = Ꮡt.TempDir();
        // build c program
        @string dll = filepath.Join(dir, vehDllˢ);
        var cmd = exec.Command(gccˢ, sharedˢ, "-o", dll, testdataTestwinlibthrowˢ);
        var (@out, err) = testenv.CleanCmdEnv(cmd).CombinedOutput();
        if (err != default!) {
            Ꮡt.Fatalf("failed to build c exe: %s\n%s"u8, err, @out);
        }
        // build go exe
        @string exe = filepath.Join(dir, testExeˢ);
        cmd = exec.Command(testenv.GoToolPath(new runtime_test_package.testing_TжTB(Ꮡt)), buildˢ, "-o", exe, testdataTestwinlibthrowˢ2);
        (@out, err) = testenv.CleanCmdEnv(cmd).CombinedOutput();
        if (err != default!) {
            Ꮡt.Fatalf("failed to build go library: %s\n%s"u8, err, @out);
        }
        // run test program in same thread
        cmd = exec.Command(exe);
        (@out, err) = testenv.CleanCmdEnv(cmd).CombinedOutput();
        if (err == default!) {
            Ꮡt.Fatal(errorExpectedˢ);
        }
        {
            var (_, ok) = err._<ж<exec.ExitError>>(ᐧ); if (ok && len(@out) > 0){
                if (!bytes.Contains(@out, slice<byte>("Exception 0x2a"u8))) {
                    Ꮡt.Fatalf("unexpected failure while running executable: %s\n%s"u8, err, @out);
                }
            } else {
                Ꮡt.Fatalf("unexpected error while running executable: %s\n%s"u8, err, @out);
            }
        }
        // run test program in a new thread
        cmd = exec.Command(exe, threadˢ);
        (@out, err) = testenv.CleanCmdEnv(cmd).CombinedOutput();
        if (err == default!) {
            Ꮡt.Fatal(errorExpectedˢ);
        }
        {
            var (errΔ1, ok) = err._<ж<exec.ExitError>>(ᐧ); if (ok){
                if (errΔ1.Value.ProcessState.ExitCode() != 42) {
                    Ꮡt.Fatalf("unexpected failure while running executable: %s\n%s"u8, errΔ1.OrTypedNil(), @out);
                }
            } else {
                Ꮡt.Fatalf("unexpected error while running executable: %s\n%s"u8, errΔ1.OrTypedNil(), @out);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object thisTestCanTRunOnWindowsˢ = (@string)"this test can't run on windows/arm"u8;
internal static readonly @string testwinlibDllˢ = "testwinlib.dll"u8;
internal static readonly @string buildmodeˢ = "-buildmode"u8;
internal static readonly @string cSharedˢ = "c-shared"u8;
internal static readonly @string testdataTestwinlibMainGoˢ = "testdata/testwinlib/main.go"u8;
internal static readonly @string ltestwinlibˢ = "-ltestwinlib"u8;
internal static readonly @string testdataTestwinlibMainCˢ = "testdata/testwinlib/main.c"u8;
internal static readonly @string exceptionCount1ˢ = "exceptionCount: 1\ncontinueCount: 1\nunhandledCount: 0\n"u8;
internal static readonly @string exceptionCount1ˢ2 = "exceptionCount: 1\ncontinueCount: 1\nunhandledCount: 1\n"u8;

public static void TestVectoredHandlerDontCrashOnLibrary(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        if (flagQuick.Value) {
            Ꮡt.Skip(quickˢ);
        }
        if (Δruntime.GOARCH == "arm"u8) {
            //TODO: remove this skip and update testwinlib/main.c
            // once windows/arm supports c-shared buildmode.
            // See go.dev/issues/43800.
            Ꮡt.Skip(thisTestCanTRunOnWindowsˢ);
        }
        testenv.MustHaveGoBuild(new runtime_test_package.testing_TжTB(Ꮡt));
        testenv.MustHaveCGO(new runtime_test_package.testing_TжTB(Ꮡt));
        testenv.MustHaveExecPath(new runtime_test_package.testing_TжTB(Ꮡt), gccˢ);
        Ꮡtestprog.of(testprogᴛ1.ᏑMutex).Lock();
        defer(Ꮡtestprog.of(testprogᴛ1.ᏑMutex).Unlock, ref ᒐ);
        @string dir = Ꮡt.TempDir();
        // build go dll
        @string dll = filepath.Join(dir, testwinlibDllˢ);
        var cmd = exec.Command(testenv.GoToolPath(new runtime_test_package.testing_TжTB(Ꮡt)), buildˢ, "-o", dll, buildmodeˢ, cSharedˢ, testdataTestwinlibMainGoˢ);
        var (@out, err) = testenv.CleanCmdEnv(cmd).CombinedOutput();
        if (err != default!) {
            Ꮡt.Fatalf("failed to build go library: %s\n%s"u8, err, @out);
        }
        // build c program
        @string exe = filepath.Join(dir, testExeˢ);
        cmd = exec.Command(gccˢ, "-L"u8 + dir, "-I" + dir, ltestwinlibˢ, "-o", exe, testdataTestwinlibMainCˢ);
        (@out, err) = testenv.CleanCmdEnv(cmd).CombinedOutput();
        if (err != default!) {
            Ꮡt.Fatalf("failed to build c exe: %s\n%s"u8, err, @out);
        }
        // run test program
        cmd = exec.Command(exe);
        (@out, err) = testenv.CleanCmdEnv(cmd).CombinedOutput();
        if (err != default!) {
            Ꮡt.Fatalf("failure while running executable: %s\n%s"u8, err, @out);
        }
        @string expectedOutput = default!;
        if (Δruntime.GOARCH == "arm64"u8 || Δruntime.GOARCH == "arm"u8){
            // TODO: remove when windows/arm64 and windows/arm support SEH stack unwinding.
            expectedOutput = exceptionCount1ˢ;
        } else {
            expectedOutput = exceptionCount1ˢ2;
        }
        // cleaning output
        @string cleanedOut = strings.ReplaceAll(((@string)@out), "\r\n"u8, "\n"u8);
        if (cleanedOut != expectedOutput) {
            Ꮡt.Errorf("expected output %q, got %q"u8, expectedOutput, cleanedOut);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string kernel32Dllˢ = "kernel32.dll"u8;
internal static readonly @string generateConsoleCtrlEventˢ = "GenerateConsoleCtrlEvent"u8;

internal static error sendCtrlBreak(nint pid) {
    var (kernel32, err) = syscall.LoadDLL(kernel32Dllˢ);
    if (err != default!) {
        return fmt.Errorf("LoadDLL: %v\n"u8, err);
    }
    (var generateEvent, err) = kernel32.FindProc(generateConsoleCtrlEventˢ);
    if (err != default!) {
        return fmt.Errorf("FindProc: %v\n"u8, err);
    }
    (var result, _, err) = generateEvent.Call(syscall.CTRL_BREAK_EVENT, (uintptr)pid);
    if (result == 0) {
        return fmt.Errorf("GenerateConsoleCtrlEvent: %v\n"u8, err);
    }
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataTestwinsignalˢ = "testdata/testwinsignal/main.go"u8;

// TestCtrlHandler tests that Go can gracefully handle closing the console window.
// See https://golang.org/issues/41884.
public static void TestCtrlHandler(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        testenv.MustHaveGoBuild(new runtime_test_package.testing_TжTB(Ꮡt));
        Ꮡt.Parallel();
        // build go program
        @string exe = filepath.Join(Ꮡt.TempDir(), testExeˢ);
        var cmd = exec.Command(testenv.GoToolPath(new runtime_test_package.testing_TжTB(Ꮡt)), buildˢ, "-o", exe, testdataTestwinsignalˢ);
        var (@out, err) = testenv.CleanCmdEnv(cmd).CombinedOutput();
        if (err != default!) {
            Ꮡt.Fatalf("failed to build go exe: %v\n%s"u8, err, @out);
        }
        // run test program
        cmd = exec.Command(exe);
        ref var stdout = ref heap(new strings.Builder(), out var Ꮡstdout);
        ref var stderr = ref heap(new strings.Builder(), out var Ꮡstderr);
        cmd.Value.Stdout = new runtime_test_package.strings_BuilderжWriter(Ꮡstdout);
        cmd.Value.Stderr = new runtime_test_package.strings_BuilderжWriter(Ꮡstderr);
        (var inPipe, err) = cmd.StdinPipe();
        if (err != default!) {
            Ꮡt.Fatalf("Failed to create stdin pipe: %v"u8, err);
        }
        // keep inPipe alive until the end of the test
        var inPipeʗ1 = inPipe;
        defer(() => inPipeʗ1.Close(), ref ᒐ);
        // in a new command window
        const uint32 _CREATE_NEW_CONSOLE = 0x00000010;
        cmd.Value.SysProcAttr = Ꮡ(new syscall.SysProcAttr(
            CreationFlags: _CREATE_NEW_CONSOLE,
            HideWindow: true
        ));
        {
            var errΔ1 = cmd.Start(); if (errΔ1 != default!) {
                Ꮡt.Fatalf("Start failed: %v"u8, errΔ1);
            }
        }
        var cmdʗ1 = cmd;
        defer(() => {
            (~cmdʗ1).Process.Kill();
            cmdʗ1.Wait();
        }, ref ᒐ);
        // check child exited gracefully, did not timeout
        {
            var errΔ2 = cmd.Wait(); if (errΔ2 != default!) {
                Ꮡt.Fatalf("Program exited with error: %v\n%s"u8, errΔ2, Ꮡstderr);
            }
        }
        // check child received, handled SIGTERM
        {
            @string expected = syscall.SIGTERM.String();
            @string got = strings.TrimSpace(stdout.String()); if (expected != got) {
                Ꮡt.Fatalf("Expected '%s' got: %s"u8, expected, got);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object thisTestCanOnlyRunOnˢ = (@string)"this test can only run on windows/amd64"u8;
internal static readonly @string dummyDllˢ = "dummy.dll"u8;
internal static readonly @string testdataTestwinlibsignalˢ = "testdata/testwinlibsignal/dummy.go"u8;
internal static readonly @string testdataTestwinlibsignalˢ2 = "testdata/testwinlibsignal/main.c"u8;

// TestLibraryCtrlHandler tests that Go DLL allows calling program to handle console control events.
// See https://golang.org/issues/35965.
[MethodImpl(MethodImplOptions.NoInlining)] public static void TestLibraryCtrlHandler(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        if (flagQuick.Value) {
            Ꮡt.Skip(quickˢ);
        }
        if (Δruntime.GOARCH != "amd64"u8) {
            Ꮡt.Skip(thisTestCanOnlyRunOnˢ);
        }
        testenv.MustHaveGoBuild(new runtime_test_package.testing_TжTB(Ꮡt));
        testenv.MustHaveCGO(new runtime_test_package.testing_TжTB(Ꮡt));
        testenv.MustHaveExecPath(new runtime_test_package.testing_TжTB(Ꮡt), gccˢ);
        Ꮡtestprog.of(testprogᴛ1.ᏑMutex).Lock();
        defer(Ꮡtestprog.of(testprogᴛ1.ᏑMutex).Unlock, ref ᒐ);
        @string dir = Ꮡt.TempDir();
        // build go dll
        @string dll = filepath.Join(dir, dummyDllˢ);
        var cmd = exec.Command(testenv.GoToolPath(new runtime_test_package.testing_TжTB(Ꮡt)), buildˢ, "-o", dll, buildmodeˢ, cSharedˢ, testdataTestwinlibsignalˢ);
        var (@out, err) = testenv.CleanCmdEnv(cmd).CombinedOutput();
        if (err != default!) {
            Ꮡt.Fatalf("failed to build go library: %s\n%s"u8, err, @out);
        }
        // build c program
        @string exe = filepath.Join(dir, testExeˢ);
        cmd = exec.Command(gccˢ, "-o"u8, exe, testdataTestwinlibsignalˢ2);
        (@out, err) = testenv.CleanCmdEnv(cmd).CombinedOutput();
        if (err != default!) {
            Ꮡt.Fatalf("failed to build c exe: %s\n%s"u8, err, @out);
        }
        // run test program
        cmd = exec.Command(exe);
        ref var stderr = ref heap(new bytes.Buffer(), out var Ꮡstderr);
        cmd.Value.Stderr = new runtime_test_package.bytes_BufferжWriter(Ꮡstderr);
        (var outPipe, err) = cmd.StdoutPipe();
        if (err != default!) {
            Ꮡt.Fatalf("Failed to create stdout pipe: %v"u8, err);
        }
        var outReader = bufio.NewReader(outPipe);
        cmd.Value.SysProcAttr = Ꮡ(new syscall.SysProcAttr(
            CreationFlags: syscall.CREATE_NEW_PROCESS_GROUP
        ));
        {
            var errΔ1 = cmd.Start(); if (errΔ1 != default!) {
                Ꮡt.Fatalf("Start failed: %v"u8, errΔ1);
            }
        }
        var errCh = new channel<error>(1);
        var cmdʗ1 = cmd;
        var errChʗ1 = errCh;
        var outReaderʗ1 = outReader;
        goǃ(() => {
            {
                var (line, errΔ2) = outReaderʗ1.ReadString((rune)'\n'); if (errΔ2 != default!){
                    errChʗ1.ᐸꟷ(fmt.Errorf("could not read stdout: %v"u8, errΔ2));
                } else 
                if (strings.TrimSpace(line) != "ready"u8){
                    errChʗ1.ᐸꟷ(fmt.Errorf("unexpected message: %v"u8, line));
                } else {
                    errChʗ1.ᐸꟷ(sendCtrlBreak((~(~cmdʗ1).Process).Pid));
                }
            }
        });
        {
            var errΔ3 = ᐸꟷ(errCh); if (errΔ3 != default!) {
                Ꮡt.Fatal(errΔ3);
            }
        }
        {
            var errΔ4 = cmd.Wait(); if (errΔ4 != default!) {
                Ꮡt.Fatalf("Program exited with error: %v\n%s"u8, errΔ4, Ꮡstderr);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingWindowsOnlyTestˢ = (@string)"skipping windows only test"u8;
internal static readonly @string envˢ = "env"u8;
internal static readonly @string cgoCflagsˢ = "CGO_CFLAGS"u8;
internal static readonly @string testDllˢ = "test.dll"u8;
internal static readonly @string gotestExeˢ = "gotest.exe"u8;
internal static readonly @string testdataTestwintlsMainGoˢ = "testdata/testwintls/main.go"u8;
internal static readonly @string testdataTestwintlsMainCˢ = "testdata/testwintls/main.c"u8;
internal static readonly @string goFuncˢ = "GoFunc"u8;

public static void TestIssue59213(ж<testing.T> Ꮡt) {
    if (Δruntime.GOOS != "windows"u8) {
        Ꮡt.Skip(skippingWindowsOnlyTestˢ);
    }
    if (flagQuick.Value) {
        Ꮡt.Skip(quickˢ);
    }
    testenv.MustHaveGoBuild(new runtime_test_package.testing_TжTB(Ꮡt));
    testenv.MustHaveCGO(new runtime_test_package.testing_TжTB(Ꮡt));
    @string goEnv(@string arg) {
        var cmdΔ1 = testenv.Command(new runtime_test_package.testing_TжTB(Ꮡt), testenv.GoToolPath(new runtime_test_package.testing_TжTB(Ꮡt)), envˢ, arg);
        cmdΔ1.Value.Stderr = new runtime_test_package.bytes_BufferжWriter(@new<bytes.Buffer>());
        var (line, errΔ1) = cmdΔ1.Output();
        if (errΔ1 != default!) {
            Ꮡt.Fatalf("%v: %v\n%s"u8, cmdΔ1.OrTypedNil(), errΔ1, (~cmdΔ1).Stderr);
        }
        @string outΔ1 = ((@string)bytes.TrimSpace(line));
        Ꮡt.Logf("%v: %q"u8, cmdΔ1.OrTypedNil(), outΔ1);
        return outΔ1;
    }
    @string cc = goEnv("CC"u8);
    @string cgoCflags = goEnv(cgoCflagsˢ);
    Ꮡt.Parallel();
    @string tmpdir = Ꮡt.TempDir();
    @string dllfile = filepath.Join(tmpdir, testDllˢ);
    @string exefile = filepath.Join(tmpdir, gotestExeˢ);
    // build go dll
    var cmd = testenv.Command(new runtime_test_package.testing_TжTB(Ꮡt), testenv.GoToolPath(new runtime_test_package.testing_TжTB(Ꮡt)), buildˢ, "-o", dllfile, buildmodeˢ, cSharedˢ, testdataTestwintlsMainGoˢ);
    var (@out, err) = testenv.CleanCmdEnv(cmd).CombinedOutput();
    if (err != default!) {
        Ꮡt.Fatalf("failed to build go library: %s\n%s"u8, err, @out);
    }
    // build c program
    cmd = testenv.Command(new runtime_test_package.testing_TжTB(Ꮡt), cc, "-o"u8, exefile, testdataTestwintlsMainCˢ);
    testenv.CleanCmdEnv(cmd);
    cmd.Value.Env = append((~cmd).Env, "CGO_CFLAGS="u8 + cgoCflags);
    (@out, err) = cmd.CombinedOutput();
    if (err != default!) {
        Ꮡt.Fatalf("failed to build c exe: %s\n%s"u8, err, @out);
    }
    // run test program
    cmd = testenv.Command(new runtime_test_package.testing_TжTB(Ꮡt), exefile, dllfile, goFuncˢ);
    (@out, err) = testenv.CleanCmdEnv(cmd).CombinedOutput();
    if (err != default!) {
        Ꮡt.Fatalf("failed: %s\n%s"u8, err, @out);
    }
}

} // end runtime_test_package
