// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Test broken pipes on Unix systems.
//
//go:build !plan9 && !js && !wasip1
namespace go;

using bufio = bufio_package;
using bytes = bytes_package;
using fmt = fmt_package;
using testenv = @internal.testenv_package;
using Δio = io_package;
using fs = go.io.fs_package;
using Δos = os_package;
using exec = go.os.exec_package;
using signal = go.os.signal_package;
using Δruntime = runtime_package;
using strconv = strconv_package;
using strings = strings_package;
using Δsync = sync_package;
using syscall = syscall_package;
using Δtesting = testing_package;
using time = time_package;
using @internal;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using go.io;
using go.os;
using static go.os_internal_test_package;

partial class os_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object unexpectedSuccessOfWriteˢ = (@string)"unexpected success of Write to broken pipe"u8;

public static void TestEPIPE(ж<Δtesting.T> Ꮡt) {
    // This test cannot be run in parallel because of a race similar
    // to the one reported in https://go.dev/issue/22315.
    //
    // Even though the pipe is opened with O_CLOEXEC, if another test forks in
    // between the call to os.Pipe and the call to r.Close, that child process can
    // retain an open copy of r's file descriptor until it execs. If one of our
    // Write calls occurs during that interval it can spuriously succeed,
    // buffering the write to the child's copy of the pipe (even though the child
    // will not actually read the buffered bytes).
    var (r, w, err) = Δos.Pipe();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    {
        var errΔ1 = r.Close(); if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
    }
    var expect = syscall.EPIPE;
    if (Δruntime.GOOS == "windows"u8) {
        // 232 is Windows error code ERROR_NO_DATA, "The pipe is being closed".
        expect = ((syscall.Errno)232);
    }
    // Every time we write to the pipe we should get an EPIPE.
    for (nint i = 0; i < 20; i++) {
        (_, err) = w.Write(slice<byte>("hi"u8));
        if (err == default!) {
            Ꮡt.Fatal(unexpectedSuccessOfWriteˢ);
        }
        {
            var (pe, ok) = err._<ж<fs.PathError>>(ᐧ); if (ok) {
                err = pe.Value.Err;
            }
        }
        {
            var (se, ok) = err._<ж<Δos.SyscallError>>(ᐧ); if (ok) {
                err = se.Value.Err;
            }
        }
        if (!AreEqual(err, expect)) {
            Ꮡt.Errorf("iteration %d: got %v, expected %v"u8, i, err, expect);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object windowsDoesnTSupportˢ = (@string)"Windows doesn't support SIGPIPE"u8;
internal static readonly @string goTestStdPipeHelperˢ = "GO_TEST_STD_PIPE_HELPER"u8;
internal static readonly @string goTestStdPipeHelperˢ2 = "GO_TEST_STD_PIPE_HELPER_SIGNAL"u8;
internal static readonly @string testRunˢ2 = "-test.run"u8;
internal static readonly @string testStdPipeˢ = "TestStdPipe"u8;

public static void TestStdPipe(ж<Δtesting.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var exprᴛ1 = Δruntime.GOOS;
    if (exprᴛ1 == "windows"u8) {
        Ꮡt.Skip(windowsDoesnTSupportˢ);
    }

    if (Δos.Getenv(goTestStdPipeHelperˢ) != ""u8) {
        if (Δos.Getenv(goTestStdPipeHelperˢ2) != ""u8) {
            signal.Notify(new channel<osꓸSignal>(1).WithDirection(GoChanDir.Send), new os_test_package.syscall_ΔSignalᴠΔSignal(syscall.SIGPIPE));
        }
        var exprᴛ2 = Δos.Getenv(goTestStdPipeHelperˢ);
        if (exprᴛ2 == "1"u8) {
            Δos.Stdout.Write(slice<byte>("stdout"u8));
        }
        else if (exprᴛ2 == "2"u8) {
            Δos.Stderr.Write(slice<byte>("stderr"u8));
        }
        else if (exprᴛ2 == "3"u8) {
            {
                var (_, errΔ2) = Δos.NewFile(3, "3"u8).Write(slice<byte>("3"u8)); if (errΔ2 == default!) {
                    Δos.Exit(3);
                }
            }
        }
        else { /* default: */
            throw panic("unrecognized value for GO_TEST_STD_PIPE_HELPER");
        }

        // For stdout/stderr, we should have crashed with a broken pipe error.
        // The caller will be looking for that exit status,
        // so just exit normally here to cause a failure in the caller.
        // For descriptor 3, a normal exit is expected.
        Δos.Exit(0);
    }
    testenv.MustHaveExec(new os_test_package.testing_TжTB(Ꮡt));
    // This test cannot be run in parallel due to the same race as for TestEPIPE.
    // (We expect a write to a closed pipe can fail, but a concurrent fork of a
    // child process can cause the pipe to unexpectedly remain open.)
    var (r, w, err) = Δos.Pipe();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    {
        var errΔ3 = r.Close(); if (errΔ3 != default!) {
            Ꮡt.Fatal(errΔ3);
        }
    }
    // Invoke the test program to run the test and write to a closed pipe.
    // If sig is false:
    // writing to stdout or stderr should cause an immediate SIGPIPE;
    // writing to descriptor 3 should fail with EPIPE and then exit 0.
    // If sig is true:
    // all writes should fail with EPIPE and then exit 0.
    foreach (var (_, sig) in new bool[]{false, true}.slice()) {
        for (nint dest = 1; dest < 4; dest++) {
            var cmdΔ1 = testenv.Command(new os_test_package.testing_TжTB(Ꮡt), Δos.Args[0], testRunˢ2, testStdPipeˢ);
            cmdΔ1.Value.Stdout = new Δos.FileжWriter(w);
            cmdΔ1.Value.Stderr = new Δos.FileжWriter(w);
            cmdΔ1.Value.ExtraFiles = new ж<Δos.File>[]{w}.slice();
            cmdΔ1.Value.Env = append(Δos.Environ(), fmt.Sprintf("GO_TEST_STD_PIPE_HELPER=%d"u8, dest));
            if (sig) {
                cmdΔ1.Value.Env = append((~cmdΔ1).Env, "GO_TEST_STD_PIPE_HELPER_SIGNAL=1"u8);
            }
            {
                var errΔ4 = cmdΔ1.Run(); if (errΔ4 == default!){
                    if (!sig && dest < 3) {
                        Ꮡt.Errorf("unexpected success of write to closed pipe %d sig %t in child"u8, dest, sig);
                    }
                } else 
                {
                    var (ee, ok) = errΔ4._<ж<exec.ExitError>>(ᐧ); if (!ok){
                        Ꮡt.Errorf("unexpected exec error type %T: %v"u8, errΔ4, errΔ4);
                    } else 
                    {
                        var (ws, okΔ1) = ee.Value.ProcessState.Value.Sys()._<syscall.WaitStatus>(ᐧ); if (!okΔ1){
                            Ꮡt.Errorf("unexpected wait status type %T: %v"u8, ee.Value.ProcessState.Value.Sys(), ee.Value.ProcessState.Value.Sys());
                        } else 
                        if (ws.Signaled() && ws.Signal() == syscall.SIGPIPE){
                            if (sig || dest > 2) {
                                Ꮡt.Errorf("unexpected SIGPIPE signal for descriptor %d sig %t"u8, dest, sig);
                            }
                        } else {
                            Ꮡt.Errorf("unexpected exit status %v for descriptor %d sig %t"u8, errΔ4, dest, sig);
                        }
                    }
                }
            }
        }
    }
    // Test redirecting stdout but not stderr.  Issue 40076.
    var cmd = testenv.Command(new os_test_package.testing_TжTB(Ꮡt), Δos.Args[0], testRunˢ2, testStdPipeˢ);
    cmd.Value.Stdout = new Δos.FileжWriter(w);
    ref var stderr = ref heap(new bytes.Buffer(), out var Ꮡstderr);
    cmd.Value.Stderr = new os_test_package.bytes_BufferжWriter(Ꮡstderr);
    cmd.Value.Env = append(cmd.Environ(), "GO_TEST_STD_PIPE_HELPER=1"u8);
    {
        var errΔ5 = cmd.Run(); if (errΔ5 == default!){
            Ꮡt.Errorf("unexpected success of write to closed stdout"u8);
        } else 
        {
            var (ee, ok) = errΔ5._<ж<exec.ExitError>>(ᐧ); if (!ok){
                Ꮡt.Errorf("unexpected exec error type %T: %v"u8, errΔ5, errΔ5);
            } else 
            {
                var (ws, okΔ1) = ee.Value.ProcessState.Value.Sys()._<syscall.WaitStatus>(ᐧ); if (!okΔ1){
                    Ꮡt.Errorf("unexpected wait status type %T: %v"u8, ee.Value.ProcessState.Value.Sys(), ee.Value.ProcessState.Value.Sys());
                } else 
                if (!ws.Signaled() || ws.Signal() != syscall.SIGPIPE) {
                    Ꮡt.Errorf("unexpected exit status %v for write to closed stdout"u8, errΔ5);
                }
            }
        }
    }
    {
        var output = stderr.Bytes(); if (len(output) > 0) {
            Ꮡt.Errorf("unexpected output on stderr: %s"u8, output);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object iOOnClosedPipeˢ = (@string)"I/O on closed pipe unexpectedly succeeded"u8;

[MethodImpl(MethodImplOptions.NoInlining)] internal static void testClosedPipeRace(ж<Δtesting.T> Ꮡt, bool read) {
    GoFrame ᒐ = default;
    try {
        // This test cannot be run in parallel due to the same race as for TestEPIPE.
        // (We expect a write to a closed pipe can fail, but a concurrent fork of a
        // child process can cause the pipe to unexpectedly remain open.)
        nint limit = 1;
        if (!read) {
            // Get the amount we have to write to overload a pipe
            // with no reader.
            limit = 131073;
            {
                var (bΔ1, errΔ1) = Δos.ReadFile(procSysFsPipeMaxSizeˢ); if (errΔ1 == default!) {
                    {
                        var (i, errΔ2) = strconv.Atoi(strings.TrimSpace(((@string)bΔ1))); if (errΔ2 == default!) {
                            limit = i + 1;
                        }
                    }
                }
            }
            Ꮡt.Logf("using pipe write limit of %d"u8, limit);
        }
        var (r, w, err) = Δos.Pipe();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var rʗ1 = r;
        defer(() => rʗ1.Close(), ref ᒐ);
        var wʗ1 = w;
        defer(() => wʗ1.Close(), ref ᒐ);
        // Close the read end of the pipe in a goroutine while we are
        // writing to the write end, or vice-versa.
        var rʗ2 = r;
        var wʗ2 = w;
        goǃ(() => {
            // Give the main goroutine a chance to enter the Read or
            // Write call. This is sloppy but the test will pass even
            // if we close before the read/write.
            time.Sleep(20 * time.Millisecond);
            error errΔ3 = default!;
            if (read){
                errΔ3 = rʗ2.Close();
            } else {
                errΔ3 = wʗ2.Close();
            }
            if (errΔ3 != default!) {
                Ꮡt.Error(errΔ3);
            }
        });
        var b = new slice<byte>(limit);
        if (read){
            (_, err) = r.Read(b[..]);
        } else {
            (_, err) = w.Write(b[..]);
        }
        if (err == default!){
            Ꮡt.Error(iOOnClosedPipeˢ);
        } else 
        {
            var (pe, ok) = err._<ж<fs.PathError>>(ᐧ); if (!ok){
                Ꮡt.Errorf("I/O on closed pipe returned unexpected error type %T; expected fs.PathError"u8, pe.OrTypedNil());
            } else 
            if (!AreEqual((~pe).Err, fs.ErrClosed)){
                Ꮡt.Errorf("got error %q but expected %q"u8, (~pe).Err, fs.ErrClosed);
            } else {
                Ꮡt.Logf("I/O returned expected error %q"u8, err);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestClosedPipeRaceRead(ж<Δtesting.T> Ꮡt) {
    testClosedPipeRace(Ꮡt, true);
}

public static void TestClosedPipeRaceWrite(ж<Δtesting.T> Ꮡt) {
    testClosedPipeRace(Ꮡt, false);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object windowsDoesnTSupportˢ2 = (@string)"Windows doesn't support SetNonblock"u8;
internal static readonly @string goWantReadNonblockingFdˢ = "GO_WANT_READ_NONBLOCKING_FD"u8;

// Issue 20915: Reading on nonblocking fd should not return "waiting
// for unsupported file type." Currently it returns EAGAIN; it is
// possible that in the future it will simply wait for data.
public static void TestReadNonblockingFd(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var exprᴛ1 = Δruntime.GOOS;
        if (exprᴛ1 == "windows"u8) {
            Ꮡt.Skip(windowsDoesnTSupportˢ2);
        }

        if (Δos.Getenv(goWantReadNonblockingFdˢ) == "1"u8) {
            var fd = ((syscallꓸHandle)Δos.Stdin.Fd());
            syscall.SetNonblock(fd, true);
            defer(syscall.SetNonblock, fd, (bool)false, ref ᒐ);
            var (_, errΔ1) = Δos.Stdin.Read(new slice<byte>(1));
            if (errΔ1 != default!) {
                {
                    var (perr, ok) = errΔ1._<ж<fs.PathError>>(ᐧ); if (!ok || !AreEqual((~perr).Err, syscall.EAGAIN)) {
                        Ꮡt.Fatalf("read on nonblocking stdin got %q, should have gotten EAGAIN"u8, errΔ1);
                    }
                }
            }
            Δos.Exit(0);
        }
        testenv.MustHaveExec(new os_test_package.testing_TжTB(Ꮡt));
        Ꮡt.Parallel();
        var (r, w, err) = Δos.Pipe();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var rʗ1 = r;
        defer(() => rʗ1.Close(), ref ᒐ);
        var wʗ1 = w;
        defer(() => wʗ1.Close(), ref ᒐ);
        var cmd = testenv.Command(new os_test_package.testing_TжTB(Ꮡt), Δos.Args[0], "-test.run=^"u8 + Ꮡt.Name() + "$"u8);
        cmd.Value.Env = append(cmd.Environ(), "GO_WANT_READ_NONBLOCKING_FD=1"u8);
        cmd.Value.Stdin = new os_test_package.os_FileжReader(r);
        (var output, err) = cmd.CombinedOutput();
        Ꮡt.Logf("%s"u8, output);
        if (err != default!) {
            Ꮡt.Errorf("child process failed: %v"u8, err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string readerˢ = "reader"u8;
internal static readonly @string writerˢ = "writer"u8;

public static void TestCloseWithBlockingReadByNewFile(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    array<syscallDescriptor> p = new(2);
    var err = syscall.Pipe(p[..]);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    // os.NewFile returns a blocking mode file.
    testCloseWithBlockingRead(Ꮡt, Δos.NewFile((uintptr)p[0], readerˢ), Δos.NewFile((uintptr)p[1], writerˢ));
}

public static void TestCloseWithBlockingReadByFd(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    var (r, w, err) = Δos.Pipe();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    // Calling Fd will put the file into blocking mode.
    _ = r.Fd();
    testCloseWithBlockingRead(Ꮡt, r, w);
}

// Test that we don't let a blocking read prevent a close.
[MethodImpl(MethodImplOptions.NoInlining)] internal static void testCloseWithBlockingRead(ж<Δtesting.T> Ꮡt, ж<Δos.File> Ꮡr, ж<Δos.File> Ꮡw) {
    ref var t = ref Ꮡt.DerefOrNull();
    ref var r = ref Ꮡr.DerefOrNull();
    ref var w = ref Ꮡw.DerefOrNull();

    channel<EmptyStruct> enteringRead = new channel<EmptyStruct>(0);
    channel<EmptyStruct> done = new channel<EmptyStruct>(0);
    var doneʗ1 = done;
    var enteringReadʗ1 = enteringRead;
    goǃ(() => {
        ref var b = ref heap(new array<byte>(1), out var Ꮡb);
        close(enteringReadʗ1);
        var (_, err) = Ꮡr.Read(b[..]);
        if (err == default!) {
            Ꮡt.Error(iOOnClosedPipeˢ);
        }
        {
            var (pe, ok) = err._<ж<fs.PathError>>(ᐧ); if (ok) {
                err = pe.Value.Err;
            }
        }
        if (!AreEqual(err, Δio.EOF) && !AreEqual(err, fs.ErrClosed)) {
            Ꮡt.Errorf("got %v, expected EOF or closed"u8, err);
        }
        close(doneʗ1);
    });
    // Give the goroutine a chance to enter the Read
    // or Write call. This is sloppy but the test will
    // pass even if we close before the read/write.
    ᐸꟷ(enteringRead);
    time.Sleep(20 * time.Millisecond);
    {
        var err = Ꮡr.Close(); if (err != default!) {
            Ꮡt.Error(err);
        }
    }
    // r.Close has completed, but since we assume r is in blocking mode that
    // probably didn't unblock the call to r.Read. Close w to unblock it.
    Ꮡw.Close();
    ᐸꟷ(done);
}

public static void TestPipeEOF(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    var (r, w, err) = Δos.Pipe();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    testPipeEOF(Ꮡt, new os_test_package.os_FileжReadCloser(r), new os_test_package.os_FileжWriteCloser(w));
}

// testPipeEOF tests that when the write side of a pipe or FIFO is closed,
// a blocked Read call on the reader side returns io.EOF.
//
// This scenario previously failed to unblock the Read call on darwin.
// (See https://go.dev/issue/24164.)
[MethodImpl(MethodImplOptions.NoInlining)] internal static void testPipeEOF(ж<Δtesting.T> Ꮡt, Δio.ReadCloser r, Δio.WriteCloser w) {
    GoFrame ᒐ = default;
    try {
        // parkDelay is an arbitrary delay we wait for a pipe-reader goroutine to park
        // before issuing the corresponding write. The test should pass no matter what
        // delay we use, but with a longer delay is has a higher chance of detecting
        // poller bugs.
        var parkDelay = 10 * time.Millisecond;
        if (Δtesting.Short()) {
            parkDelay = 100 * time.Microsecond;
        }
        var writerDone = new channel<EmptyStruct>(0);
        var writerDoneʗ1 = writerDone;
        defer(() => {
            {
                var errΔ1 = r.Close(); if (errΔ1 != default!) {
                    Ꮡt.Errorf("error closing reader: %v"u8, errΔ1);
                }
            }
            ᐸꟷ(writerDoneʗ1);
        }, ref ᒐ);
        var write = new channel<nint>(1);
        var writeʗ1 = write;
        var writerDoneʗ2 = writerDone;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(ᴛ1 => close(ᴛ1), writerDoneʗ2, ref ᒐ);
                foreach (var i in writeʗ1) {
                    time.Sleep(parkDelay);
                    var (_, errΔ2) = fmt.Fprintf(w, "line %d\n"u8, i);
                    if (errΔ2 != default!) {
                        Ꮡt.Errorf("error writing to fifo: %v"u8, errΔ2);
                        return;
                    }
                }
                time.Sleep(parkDelay);
                {
                    var errΔ3 = w.Close(); if (errΔ3 != default!) {
                        Ꮡt.Errorf("error closing writer: %v"u8, errΔ3);
                    }
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
        var rbuf = bufio.NewReader(r);
        for (nint i = 0; i < 3; i++) {
            write.ᐸꟷ(i);
            var (bΔ1, errΔ4) = rbuf.ReadBytes((rune)'\n');
            if (errΔ4 != default!) {
                Ꮡt.Fatal(errΔ4);
            }
            Ꮡt.Logf("%s\n"u8, bytes.TrimSpace(bΔ1));
        }
        close(write);
        var (b, err) = rbuf.ReadBytes((rune)'\n');
        if (!AreEqual(err, Δio.EOF) || len(b) != 0) {
            Ꮡt.Errorf(@"ReadBytes: %q, %v; want """", io.EOF"u8, b, err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Issue 24481.
[MethodImpl(MethodImplOptions.NoInlining)] public static void TestFdRace(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        // This test starts 100 simultaneous goroutines, which could bury a more
        // interesting stack if this or some other test happens to panic. It is also
        // nearly instantaneous, so any latency benefit from running it in parallel
        // would be minimal.
        var (r, w, err) = Δos.Pipe();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var rʗ1 = r;
        defer(() => rʗ1.Close(), ref ᒐ);
        var wʗ1 = w;
        defer(() => wʗ1.Close(), ref ᒐ);
        ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
        var wʗ2 = w;
        void call() {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                wʗ2.Fd();
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }
        const nint tries = 100;
        for (nint i = 0; i < tries; i++) {
            Ꮡwg.Add(1);
            var callʗ1 = call;
            goǃ(callʗ1);
        }
        Ꮡwg.Wait();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object readTimedOutˢ = (@string)"read timed out"u8;

[MethodImpl(MethodImplOptions.NoInlining)] public static void TestFdReadRace(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        Ꮡt.Parallel();
        var (r, w, err) = Δos.Pipe();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var rʗ1 = r;
        defer(() => rʗ1.Close(), ref ᒐ);
        var wʗ1 = w;
        defer(() => wʗ1.Close(), ref ᒐ);
        UntypedInt count = 10;
        var c = new channel<bool>(1);
        ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
        Ꮡwg.Add(1);
        var cʗ1 = c;
        var rʗ2 = r;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                ref var buf = ref heap(new array<byte>(10), out var Ꮡbuf);
                rʗ2.SetReadDeadline(time.Now().Add(time.ΔMinute));
                cʗ1.ᐸꟷ(true);
                {
                    var (_, errΔ1) = rʗ2.Read(buf[..]); if (Δos.IsTimeout(errΔ1)) {
                        Ꮡt.Error(readTimedOutˢ);
                    }
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
        Ꮡwg.Add(1);
        var cʗ2 = c;
        var rʗ3 = r;
        var wʗ2 = w;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                ᐸꟷ(cʗ2);
                // Give the other goroutine a chance to enter the Read.
                // It doesn't matter if this occasionally fails, the test
                // will still pass, it just won't test anything.
                time.Sleep(10 * time.Millisecond);
                rʗ3.Fd();
                // The bug was that Fd would hang until Read timed out.
                // If the bug is fixed, then writing to w and closing r here
                // will cause the Read to exit before the timeout expires.
                wʗ2.Write(new slice<byte>(count));
                rʗ3.Close();
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
        Ꮡwg.Wait();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end os_test_package
