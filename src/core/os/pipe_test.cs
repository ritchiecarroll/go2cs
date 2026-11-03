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

public static void TestEPIPE(ж<Δtesting.T> t) {
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
    if (err != nil) {
        t.Fatal(err);
    }
    {
        var errΔ1 = r.Close(); if (errΔ1 != nil) {
            t.Fatal(errΔ1);
        }
    }

    var expect = syscall.EPIPE;
    if (Δruntime.GOOS == "windows"u8) {
        // 232 is Windows error code ERROR_NO_DATA, "The pipe is being closed".
        expect = (syscall.Errno)232;
    }
    // Every time we write to the pipe we should get an EPIPE.
    for (nint i = 0; i < 20; i++) {
        (_, err) = w.Write(slice<byte>("hi"u8));
        if (err == nil) {
            t.Fatal(unexpectedSuccessOfWriteˢ);
        }
        {
            var (pe, ok) = err._<ж<fs.PathError>>(ᐧ); if (ok) {
                err = pe.Err;
            }
        }
        {
            var (se, ok) = err._<ж<Δos.SyscallError>>(ᐧ); if (ok) {
                err = se.Err;
            }
        }
        if (!AreEqual(err, expect)) {
            t.Errorf("iteration %d: got %v, expected %v", i, err, expect);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object windowsDoesnTSupportˢ = (@string)"Windows doesn't support SIGPIPE"u8;
internal static ref readonly @string goTestStdPipeHelperˢ => ref goTestStdPipeHelperˢᵇ;
private static readonly @string goTestStdPipeHelperˢᵇ = "GO_TEST_STD_PIPE_HELPER"u8;
internal static ref readonly @string goTestStdPipeHelperˢ2 => ref goTestStdPipeHelperˢ2ᵇ;
private static readonly @string goTestStdPipeHelperˢ2ᵇ = "GO_TEST_STD_PIPE_HELPER_SIGNAL"u8;
internal static ref readonly @string testRunˢ3 => ref testRunˢ3ᵇ;
private static readonly @string testRunˢ3ᵇ = "-test.run"u8;
internal static ref readonly @string testStdPipeˢ => ref testStdPipeˢᵇ;
private static readonly @string testStdPipeˢᵇ = "TestStdPipe"u8;

public static void TestStdPipe(ж<Δtesting.T> t) {
    var exprᴛ1 = Δruntime.GOOS;
    switch (ᐧ) {
    case {} when exprᴛ1 == "windows"u8: {
        t.Skip(windowsDoesnTSupportˢ);
        break;
    }}

    if (Δos.Getenv(goTestStdPipeHelperˢ) != ""u8) {
        if (Δos.Getenv(goTestStdPipeHelperˢ2) != ""u8) {
            signal.Notify(new channel<osꓸSignal>(1).AsSendOnly(), new os_test_package.syscall_ΔSignalᴠΔSignal(syscall.SIGPIPE));
        }
        var exprᴛ2 = Δos.Getenv(goTestStdPipeHelperˢ);
        switch (ᐧ) {
        case {} when exprᴛ2 == "1"u8: {
            Δos.Stdout.Write(slice<byte>("stdout"u8));
            break;
        }
        case {} when exprᴛ2 == "2"u8: {
            Δos.Stderr.Write(slice<byte>("stderr"u8));
            break;
        }
        case {} when exprᴛ2 == "3"u8: {
            {
                var (_, errΔ2) = Δos.NewFile(3, "3").Write(slice<byte>("3"u8)); if (errΔ2 == nil) {
                    Δos.Exit(3);
                }
            }
            break;
        }
        default: {
            throw panic("unrecognized value for GO_TEST_STD_PIPE_HELPER");
            break;
        }}
        // For stdout/stderr, we should have crashed with a broken pipe error.
        // The caller will be looking for that exit status,
        // so just exit normally here to cause a failure in the caller.
        // For descriptor 3, a normal exit is expected.
        Δos.Exit(0);
    }

    testenv.MustHaveExec(new os_test_package.testing_TжTB(t));
    // This test cannot be run in parallel due to the same race as for TestEPIPE.
    // (We expect a write to a closed pipe can fail, but a concurrent fork of a
    // child process can cause the pipe to unexpectedly remain open.)

    var (r, w, err) = Δos.Pipe();
    if (err != nil) {
        t.Fatal(err);
    }
    {
        var errΔ3 = r.Close(); if (errΔ3 != nil) {
            t.Fatal(errΔ3);
        }
    }
    // Invoke the test program to run the test and write to a closed pipe.
    // If sig is false:
    // writing to stdout or stderr should cause an immediate SIGPIPE;
    // writing to descriptor 3 should fail with EPIPE and then exit 0.
    // If sig is true:
    // all writes should fail with EPIPE and then exit 0.
    foreach (var (_, sig) in slice<bool>([false, true])) {
        for (nint dest = 1; dest < 4; dest++) {
            var cmdΔ1 = testenv.Command(new os_test_package.testing_TжTB(t), testenv.Executable(new os_test_package.testing_TжTB(t)), testRunˢ3, testStdPipeˢ);
            cmdΔ1.Stdout = new Δos.FileжWriter(w);
            cmdΔ1.Stderr = new Δos.FileжWriter(w);
            cmdΔ1.ExtraFiles = slice<ж<Δos.File>>([w]);
            cmdΔ1.Env = append(Δos.Environ(), fmt.Sprintf("GO_TEST_STD_PIPE_HELPER=%d"u8, dest));
            if (sig) {
                cmdΔ1.Env = append(cmdΔ1.Env, (@string)"GO_TEST_STD_PIPE_HELPER_SIGNAL=1");
            }
            {
                var errΔ4 = cmdΔ1.Run(); if (errΔ4 == nil) {
                    if (!sig && dest < 3) {
                        t.Errorf("unexpected success of write to closed pipe %d sig %t in child", dest, sig);
                    }
                } else {
                    var (ee, ok) = errΔ4._<ж<exec.ExitError>>(ᐧ); if (!ok) {
                        t.Errorf("unexpected exec error type %T: %v", errΔ4, errΔ4);
                    } else {
                        var (ws, okΔ1) = ee.Value.ProcessState.Value.Sys()._<syscall.WaitStatus>(ᐧ); if (!okΔ1) {
                            t.Errorf("unexpected wait status type %T: %v", ee.Value.ProcessState.Value.Sys(), ee.Value.ProcessState.Value.Sys());
                        } else if (ws.Signaled() && ws.Signal() == syscall.SIGPIPE) {
                            if (sig || dest > 2) {
                                t.Errorf("unexpected SIGPIPE signal for descriptor %d sig %t", dest, sig);
                            }
                        } else {
                            t.Errorf("unexpected exit status %v for descriptor %d sig %t", errΔ4, dest, sig);
                        }
                    }
                }
            }
        }
    }

    // Test redirecting stdout but not stderr.  Issue 40076.
    var cmd = testenv.Command(new os_test_package.testing_TжTB(t), testenv.Executable(new os_test_package.testing_TжTB(t)), testRunˢ3, testStdPipeˢ);
    cmd.Stdout = new Δos.FileжWriter(w);
    ref var stderr = ref heap(new bytes.Buffer(), out var Ꮡstderr);
    cmd.Stderr = new os_test_package.bytes_BufferжWriter(Ꮡstderr);
    cmd.Env = append(cmd.Environ(), (@string)"GO_TEST_STD_PIPE_HELPER=1");
    {
        var errΔ5 = cmd.Run(); if (errΔ5 == nil) {
            t.Errorf("unexpected success of write to closed stdout");
        } else {
            var (ee, ok) = errΔ5._<ж<exec.ExitError>>(ᐧ); if (!ok) {
                t.Errorf("unexpected exec error type %T: %v", errΔ5, errΔ5);
            } else {
                var (ws, okΔ1) = ee.Value.ProcessState.Value.Sys()._<syscall.WaitStatus>(ᐧ); if (!okΔ1) {
                    t.Errorf("unexpected wait status type %T: %v", ee.Value.ProcessState.Value.Sys(), ee.Value.ProcessState.Value.Sys());
                } else if (!ws.Signaled() || ws.Signal() != syscall.SIGPIPE) {
                    t.Errorf("unexpected exit status %v for write to closed stdout", errΔ5);
                }
            }
        }
    }
    {
        var output = stderr.Bytes(); if (len(output) > 0) {
            t.Errorf("unexpected output on stderr: %s", output);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object iOOnClosedPipeˢ = (@string)"I/O on closed pipe unexpectedly succeeded"u8;

internal static partial void testClosedPipeRace(ж<Δtesting.T> t, bool read) {
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
                var (bΔ1, errΔ1) = Δos.ReadFile(procSysFsPipeMaxSizeˢ); if (errΔ1 == nil) {
                    {
                        var (i, errΔ2) = strconv.Atoi(strings.TrimSpace((@string)bΔ1)); if (errΔ2 == nil) {
                            limit = i + 1;
                        }
                    }
                }
            }
            t.Logf("using pipe write limit of %d", limit);
        }

        var (r, w, err) = Δos.Pipe();
        if (err != nil) {
            t.Fatal(err);
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

            error errΔ3 = default;
            if (read) {
                errΔ3 = rʗ2.Close();
            } else {
                errΔ3 = wʗ2.Close();
            }
            if (errΔ3 != nil) {
                t.Error(errΔ3);
            }
        });

        var b = new slice<byte>(limit);
        if (read) {
            (_, err) = r.Read(b[..]);
        } else {
            (_, err) = w.Write(b[..]);
        }
        if (err == nil) {
            t.Error(iOOnClosedPipeˢ);
        } else {
            var (pe, ok) = err._<ж<fs.PathError>>(ᐧ); if (!ok) {
                t.Errorf("I/O on closed pipe returned unexpected error type %T; expected fs.PathError", pe.OrTypedNil());
            } else if (!AreEqual(pe.Err, fs.ErrClosed)) {
                t.Errorf("got error %q but expected %q", pe.Err, fs.ErrClosed);
            } else {
                t.Logf("I/O returned expected error %q", err);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestClosedPipeRaceRead(ж<Δtesting.T> t) {
    testClosedPipeRace(t, true);
}

public static void TestClosedPipeRaceWrite(ж<Δtesting.T> t) {
    testClosedPipeRace(t, false);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object windowsDoesnTSupportˢ2 = (@string)"Windows doesn't support SetNonblock"u8;
internal static ref readonly @string goWantReadNonblockingFdˢ => ref goWantReadNonblockingFdˢᵇ;
private static readonly @string goWantReadNonblockingFdˢᵇ = "GO_WANT_READ_NONBLOCKING_FD"u8;

// Issue 20915: Reading on nonblocking fd should not return "waiting
// for unsupported file type." Currently it returns EAGAIN; it is
// possible that in the future it will simply wait for data.
public static void TestReadNonblockingFd(ж<Δtesting.T> t) {
    GoFrame ᒐ = default;
    try {
        var exprᴛ1 = Δruntime.GOOS;
        switch (ᐧ) {
        case {} when exprᴛ1 == "windows"u8: {
            t.Skip(windowsDoesnTSupportˢ2);
            break;
        }}
        if (Δos.Getenv(goWantReadNonblockingFdˢ) == "1"u8) {
            var fd = (syscallꓸHandle)Δos.Stdin.Fd();
            syscall.SetNonblock(fd, true);
            defer(syscall.SetNonblock, fd, (bool)false, ref ᒐ);
            var (_, errΔ1) = Δos.Stdin.Read(new slice<byte>(1));
            if (errΔ1 != nil) {
                {
                    var (perr, ok) = errΔ1._<ж<fs.PathError>>(ᐧ); if (!ok || !AreEqual(perr.Err, syscall.EAGAIN)) {
                        t.Fatalf("read on nonblocking stdin got %q, should have gotten EAGAIN", errΔ1);
                    }
                }
            }
            Δos.Exit(0);
        }

        testenv.MustHaveExec(new os_test_package.testing_TжTB(t));
        t.Parallel();

        var (r, w, err) = Δos.Pipe();
        if (err != nil) {
            t.Fatal(err);
        }
        var rʗ1 = r;
        defer(() => rʗ1.Close(), ref ᒐ);
        var wʗ1 = w;
        defer(() => wʗ1.Close(), ref ᒐ);
        var cmd = testenv.Command(new os_test_package.testing_TжTB(t), testenv.Executable(new os_test_package.testing_TжTB(t)), "-test.run=^"u8 + t.Name() + "$"u8);
        cmd.Env = append(cmd.Environ(), (@string)"GO_WANT_READ_NONBLOCKING_FD=1");
        cmd.Stdin = new os_test_package.os_FileжReader(r);
        (var output, err) = cmd.CombinedOutput();
        t.Logf("%s", output);
        if (err != nil) {
            t.Errorf("child process failed: %v", err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string readerˢ => ref readerˢᵇ;
private static readonly @string readerˢᵇ = "reader"u8;
internal static ref readonly @string writerˢ => ref writerˢᵇ;
private static readonly @string writerˢᵇ = "writer"u8;

public static void TestCloseWithBlockingReadByNewFile(ж<Δtesting.T> t) {
    t.Parallel();

    array<syscallDescriptor> p = new(2);
    var err = syscall.Pipe(p[..]);
    if (err != nil) {
        t.Fatal(err);
    }
    // os.NewFile returns a blocking mode file.
    testCloseWithBlockingRead(t, Δos.NewFile((uintptr)p[0], readerˢ), Δos.NewFile((uintptr)p[1], writerˢ));
}

public static void TestCloseWithBlockingReadByFd(ж<Δtesting.T> t) {
    t.Parallel();

    var (r, w, err) = Δos.Pipe();
    if (err != nil) {
        t.Fatal(err);
    }
    // Calling Fd will put the file into blocking mode.
    _ = r.Fd();
    testCloseWithBlockingRead(t, r, w);
}

// Test that we don't let a blocking read prevent a close.
internal static partial void testCloseWithBlockingRead(ж<Δtesting.T> t, ж<Δos.File> r, ж<Δos.File> w) {
    channel<EmptyStruct> enteringRead = new channel<EmptyStruct>(0);
    channel<EmptyStruct> done = new channel<EmptyStruct>(0);
    var doneʗ1 = done;
    var enteringReadʗ1 = enteringRead;
    goǃ(() => {
        ref var b = ref heap(new array<byte>(1), out var Ꮡb);
        close(enteringReadʗ1);
        var (_, err) = r.Read(b[..]);
        if (err == nil) {
            t.Error(iOOnClosedPipeˢ);
        }

        {
            var (pe, ok) = err._<ж<fs.PathError>>(ᐧ); if (ok) {
                err = pe.Err;
            }
        }
        if (!AreEqual(err, Δio.EOF) && !AreEqual(err, fs.ErrClosed)) {
            t.Errorf("got %v, expected EOF or closed", err);
        }
        close(doneʗ1);
    });

    // Give the goroutine a chance to enter the Read
    // or Write call. This is sloppy but the test will
    // pass even if we close before the read/write.
    ᐸꟷ(enteringRead);
    time.Sleep(20 * time.Millisecond);

    {
        var err = r.Close(); if (err != nil) {
            t.Error(err);
        }
    }
    // r.Close has completed, but since we assume r is in blocking mode that
    // probably didn't unblock the call to r.Read. Close w to unblock it.
    w.Close();
    ᐸꟷ(done);
}

public static void TestPipeEOF(ж<Δtesting.T> t) {
    t.Parallel();

    var (r, w, err) = Δos.Pipe();
    if (err != nil) {
        t.Fatal(err);
    }

    testPipeEOF(t, new os_test_package.os_FileжReadCloser(r), new os_test_package.os_FileжWriteCloser(w));
}

// testPipeEOF tests that when the write side of a pipe or FIFO is closed,
// a blocked Read call on the reader side returns io.EOF.
//
// This scenario previously failed to unblock the Read call on darwin.
// (See https://go.dev/issue/24164.)
internal static partial void testPipeEOF(ж<Δtesting.T> t, Δio.ReadCloser r, Δio.WriteCloser w) {
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
                var errΔ1 = r.Close(); if (errΔ1 != nil) {
                    t.Errorf("error closing reader: %v", errΔ1);
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
                    if (errΔ2 != nil) {
                        t.Errorf("error writing to fifo: %v", errΔ2);
                        return;
                    }
                }

                time.Sleep(parkDelay);
                {
                    var errΔ3 = w.Close(); if (errΔ3 != nil) {
                        t.Errorf("error closing writer: %v", errΔ3);
                    }
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });

        var rbuf = bufio.NewReader(r);
        for (nint i = 0; i < 3; i++) {
            write.ᐸꟷ(i);
            var (bΔ1, errΔ4) = rbuf.ReadBytes((byte)'\n');
            if (errΔ4 != nil) {
                t.Fatal(errΔ4);
            }
            t.Logf("%s\n", bytes.TrimSpace(bΔ1));
        }

        close(write);
        var (b, err) = rbuf.ReadBytes((byte)'\n');
        if (!AreEqual(err, Δio.EOF) || len(b) != 0) {
            t.Errorf(@"ReadBytes: %q, %v; want """", io.EOF", b, err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Issue 24481.
public static partial void TestFdRace(ж<Δtesting.T> t) {
    GoFrame ᒐ = default;
    try {
        // This test starts 100 simultaneous goroutines, which could bury a more
        // interesting stack if this or some other test happens to panic. It is also
        // nearly instantaneous, so any latency benefit from running it in parallel
        // would be minimal.

        var (r, w, err) = Δos.Pipe();
        if (err != nil) {
            t.Fatal(err);
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

public static partial void TestFdReadRace(ж<Δtesting.T> t) {
    GoFrame ᒐ = default;
    try {
        t.Parallel();

        var (r, w, err) = Δos.Pipe();
        if (err != nil) {
            t.Fatal(err);
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
                        t.Error(readTimedOutˢ);
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
