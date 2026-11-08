// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Use an external test to avoid os/exec -> net/http -> crypto/x509 -> os/exec
// circular dependency on non-cgo darwin.
namespace go.os;

using bufio = bufio_package;
using bytes = bytes_package;
using context = context_package;
using errors = errors_package;
using flag = flag_package;
using fmt = fmt_package;
using poll = @internal.poll_package;
using testenv = @internal.testenv_package;
using io = io_package;
using log = log_package;
using net = net_package;
using http = go.net.http_package;
using httptest = go.net.http.httptest_package;
using os = os_package;
using Δexec = go.os.exec_package;
using fdtest = go.os.exec.@internal.fdtest_package;
using signal = go.os.signal_package;
using filepath = path.filepath_package;
using runtime = runtime_package;
using debug = go.runtime.debug_package;
using strconv = strconv_package;
using strings = strings_package;
using sync = sync_package;
using atomic = go.sync.atomic_package;
using testing = testing_package;
using time = time_package;
using @internal;
using go.net;
using go.net.http;
using go.os;
using go.os.exec.@internal;
using go.runtime;
using go.sync;
using path;
using static go.os.exec_internal_test_package;
using ꓸꓸꓸstring = Span<@string>;

partial class exec_test_package {

// haveUnexpectedFDs is set at init time to report whether any file descriptors
// were open at program start.
internal static bool haveUnexpectedFDs;

[GoInit] internal static void init() {
    @string godebug = os.Getenv("GODEBUG");
    if (godebug != ""u8) {
        godebug += ","u8;
    }
    godebug += "execwait=2"u8;
    os.Setenv("GODEBUG", godebug);

    if (os.Getenv("GO_EXEC_TEST_PID") != ""u8) {
        return;
    }
    if (runtime.GOOS == "windows"u8) {
        return;
    }
    for (var fd = (uintptr)3; fd <= 100; fd++) {
        if (poll.IsPollDescriptor(fd)) {
            continue;
        }

        if (fdtest.Exists(fd)) {
            haveUnexpectedFDs = true;
            return;
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string goExecTestPidˢ => ref goExecTestPidˢᵇ;
private static readonly @string goExecTestPidˢᵇ = "GO_EXEC_TEST_PID"u8;
internal static ref readonly @string trueˢ => ref trueˢᵇ;
private static readonly @string trueˢᵇ = "TRUE"u8;
internal static ref readonly @string testRunˢ => ref testRunˢᵇ;
private static readonly @string testRunˢᵇ = "test.run"u8;
internal static ref readonly @string testListˢ => ref testListˢᵇ;
private static readonly @string testListˢᵇ = "test.list"u8;

// TestMain allows the test binary to impersonate many other binaries,
// some of which may manipulate os.Stdin, os.Stdout, and/or os.Stderr
// (and thus cannot run as an ordinary Test function, since the testing
// package monkey-patches those variables before running tests).
public static void TestMain(ж<testing.M> m) {
    flag.Parse();

    nint pid = os.Getpid();
    if (os.Getenv(goExecTestPidˢ) == ""u8) {
        os.Setenv(goExecTestPidˢ, strconv.Itoa(pid));

        if (runtime.GOOS == "windows"u8) {
            // Normalize environment so that test behavior is consistent.
            // (The behavior of LookPath varies depending on this variable.)
            //
            // Ideally we would test both with the variable set and with it cleared,
            // but I (bcmills) am not sure that that's feasible: it may already be set
            // in the Windows registry, and I'm not sure if it is possible to remove
            // a registry variable in a program's environment.
            //
            // Per https://learn.microsoft.com/en-us/windows/win32/api/processenv/nf-processenv-needcurrentdirectoryforexepathw#remarks,
            // “the existence of the NoDefaultCurrentDirectoryInExePath environment
            // variable is checked, and not its value.”
            os.Setenv("NoDefaultCurrentDirectoryInExePath", trueˢ);
        }

        nint code = m.Run();
        if (code == 0 && (~flag.Lookup(testRunˢ)).Value.String() == ""u8 && (~flag.Lookup(testListˢ)).Value.String() == ""u8) {
            foreach (var (cmdΔ1, _) in helperCommands) {
                var (_, okΔ1) = ᏑhelperCommandUsed.Load(cmdΔ1); if (!okΔ1) {
                    fmt.Fprintf(new os.FileжWriter(os.Stderr), "helper command unused: %q\n"u8, cmdΔ1);
                    code = 1;
                }
            }
        }

        if (!testing.Short()) {
            // Run a couple of GC cycles to increase the odds of detecting
            // process leaks using the finalizers installed by GODEBUG=execwait=2.
            runtime.GC();
            runtime.GC();
        }

        os.Exit(code);
    }

    var args = flag.Args();
    if (len(args) == 0) {
        fmt.Fprintf(new os.FileжWriter(os.Stderr), "No command\n"u8);
        os.Exit(2);
    }

    @string cmd = args[0];
    args = args[1..];
    var (f, ok) = helperCommands[cmd, ꟷ];
    if (!ok) {
        fmt.Fprintf(new os.FileжWriter(os.Stderr), "Unknown command %q\n"u8, cmd);
        os.Exit(2);
    }
    f(args.ꓸꓸꓸ);
    os.Exit(0);
}

// registerHelperCommand registers a command that the test process can impersonate.
// A command should be registered in the same source file in which it is used.
// If all tests are run and pass, all registered commands must be used.
// (This prevents stale commands from accreting if tests are removed or
// refactored over time.)
internal static void registerHelperCommand(@string name, Actionꓸꓸꓸ<@string> f) {
    if (helperCommands[name] != nil) {
        throw panic("duplicate command registered: " + name);
    }
    helperCommands[name] = f;
}

// maySkipHelperCommand records that the test that uses the named helper command
// was invoked, but may call Skip on the test before actually calling
// helperCommand.
internal static void maySkipHelperCommand(@string name) {
    ᏑhelperCommandUsed.Store(name, true);
}

// helperCommand returns an exec.Cmd that will run the named helper command.
internal static partial ж<Δexec.Cmd> helperCommand(ж<testing.T> t, @string name, params ꓸꓸꓸstring argsʗp) {
    var args = argsʗp.sslice();

    t.Helper();
    return helperCommandContext(t, default, name, args.ꓸꓸꓸ);
}

// helperCommandContext is like helperCommand, but also accepts a Context under
// which to run the command.
internal static ж<Δexec.Cmd> /*cmd*/ helperCommandContext(ж<testing.T> t, context.Context ctx, @string name, params ꓸꓸꓸstring argsʗp) {
    ж<Δexec.Cmd> cmd = default;
    var args = argsʗp.sslice();

    ᏑhelperCommandUsed.LoadOrStore(name, true);

    t.Helper();
    @string exe = testenv.Executable(new exec_test_package.testing_TжTB(t));
    var cs = appendꓸꓸꓸ(slice<@string>([name]), args);
    if (ctx != nil) {
        cmd = Δexec.CommandContext(ctx, exe, cs.ꓸꓸꓸ);
    } else {
        cmd = Δexec.Command(exe, cs.ꓸꓸꓸ);
    }
    return cmd;
}

internal static readonly ж<sync.Map> ᏑhelperCommandUsed = new StaticBox<sync.Map>(default(sync.Map));
internal static ref sync.Map helperCommandUsed => ref ((StaticBox<sync.Map>)ᏑhelperCommandUsed).ValueSlot;

internal static map<@string, Actionꓸꓸꓸ<@string>> helperCommands;
internal static void initᴛhelperCommands() { helperCommands = new map<@string, Actionꓸꓸꓸ<@string>>{
    ["echo"] = cmdEcho,
    ["echoenv"] = cmdEchoEnv,
    ["cat"] = cmdCat,
    ["pipetest"] = cmdPipeTest,
    ["stdinClose"] = cmdStdinClose,
    ["exit"] = cmdExit,
    ["describefiles"] = cmdDescribeFiles,
    ["stderrfail"] = cmdStderrFail,
    ["yes"] = cmdYes,
    ["hang"] = cmdHang
}; }

internal static void cmdEcho(params ꓸꓸꓸstring argsʗp) {
    var args = argsʗp.sslice();
    var iargsᴷ = new stackAppend<any>();

    var iargs = slice<any>([]);
    foreach (var (_, s) in range(args)) {
        iargs = append(iargsᴷ, iargs, (any)(s));
    }
    fmt.Println(iargs.ꓸꓸꓸ);
}

internal static void cmdEchoEnv(params ꓸꓸꓸstring argsʗp) {
    var args = argsʗp.sslice();

    foreach (var (_, s) in range(args)) {
        fmt.Println(os.Getenv(s));
    }
}

internal static void cmdCat(params ꓸꓸꓸstring argsʗp) {
    GoFrame ᒐ = default;
    try {
        var args = argsʗp.sslice();

        if (len(args) == 0) {
            io.Copy(new os.FileжWriter(os.Stdout), new exec_test_package.os_FileжReader(os.Stdin));
            return;
        }
        nint exit = 0;
        foreach (var (_, fn) in range(args)) {
            var (f, err) = os.Open(fn);
            if (err != nil) {
                fmt.Fprintf(new os.FileжWriter(os.Stderr), "Error: %v\n"u8, err);
                exit = 2;
            } else {
                var fʗ1 = f;
                defer(() => fʗ1.Close(), ref ᒐ);
                io.Copy(new os.FileжWriter(os.Stdout), new exec_test_package.os_FileжReader(f));
            }
        }
        os.Exit(exit);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void cmdPipeTest(params ꓸꓸꓸstring ʗp) {
    var bufr = bufio.NewReader(new exec_test_package.os_FileжReader(os.Stdin));
    for (;;) {
        var (line, _, err) = bufr.ReadLine();
        if (AreEqual(err, io.EOF)) {
            break;
        } else if (err != nil) {
            os.Exit(1);
        }
        if (bytes.HasPrefix(line, slice<byte>("O:"u8))) {
            os.Stdout.Write(line);
            os.Stdout.Write([(byte)'\n']);
        } else if (bytes.HasPrefix(line, slice<byte>("E:"u8))) {
            os.Stderr.Write(line);
            os.Stderr.Write([(byte)'\n']);
        } else {
            os.Exit(1);
        }
    }
}

internal static void cmdStdinClose(params ꓸꓸꓸstring ʗp) {
    var (b, err) = io.ReadAll(new exec_test_package.os_FileжReader(os.Stdin));
    if (err != nil) {
        fmt.Fprintf(new os.FileжWriter(os.Stderr), "Error: %v\n"u8, err);
        os.Exit(1);
    }
    @string s = (@string)b; if (s != stdinCloseTestString) {
        fmt.Fprintf(new os.FileжWriter(os.Stderr), "Error: Read %q, want %q"u8, s, stdinCloseTestString);
        os.Exit(1);
    }
}

internal static void cmdExit(params ꓸꓸꓸstring argsʗp) {
    var args = argsʗp.sslice();

    var (n, _) = strconv.Atoi(args[0]);
    os.Exit(n);
}

internal static void cmdDescribeFiles(params ꓸꓸꓸstring argsʗp) {
    var args = argsʗp.sslice();

    var f = os.NewFile(3, "fd3");
    var (ln, err) = net.FileListener(f);
    if (err == nil) {
        fmt.Printf("fd3: listener %s\n"u8, ln.Addr());
        ln.Close();
    }
}

internal static void cmdStderrFail(params ꓸꓸꓸstring ʗp) {
    fmt.Fprintf(new os.FileжWriter(os.Stderr), "some stderr text\n"u8);
    os.Exit(1);
}

internal static void cmdYes(params ꓸꓸꓸstring argsʗp) {
    var args = argsʗp.slice();

    if (len(args) == 0) {
        args = ["y"];
    }
    @string s = strings.Join(args, " ") + "\n"u8;
    for (;;) {
        var (_, err) = os.Stdout.WriteString(s);
        if (err != nil) {
            os.Exit(1);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string echoˢ => ref echoˢᵇ;
private static readonly @string echoˢᵇ = "echo"u8;
internal static ref readonly @string fooBarˢ => ref fooBarˢᵇ;
private static readonly @string fooBarˢᵇ = "foo bar"u8;
internal static ref readonly @string bazˢ => ref bazˢᵇ;
private static readonly @string bazˢᵇ = "baz"u8;
internal static ref readonly @string fooBarBazˢ => ref fooBarBazˢᵇ;
private static readonly @string fooBarBazˢᵇ = "foo bar baz\n"u8;

public static void TestEcho(ж<testing.T> t) {
    t.Parallel();

    var (bs, err) = helperCommand(t, echoˢ, fooBarˢ, bazˢ).Output();
    if (err != nil) {
        t.Errorf("echo: %v", err);
    }
    @string g = (@string)bs;
    @string e = fooBarBazˢ; if (g != e) {
        t.Errorf("echo: want %q, got %q", e, g);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fooˢ => ref fooˢᵇ;
private static readonly @string fooˢᵇ = "foo"u8;
internal static readonly object skippingRunningTestAtˢ = (@string)"skipping; running test at root somehow"u8;
internal static ref readonly @string fooˢ2 => ref fooˢ2ᵇ;
private static readonly @string fooˢ2ᵇ = "foo\n"u8;

public static void TestCommandRelativeName(ж<testing.T> t) {
    t.Parallel();

    var cmd = helperCommand(t, echoˢ, fooˢ);

    // Run our own binary as a relative path
    // (e.g. "_test/exec.test") our parent directory.
    @string @base = filepath.Base(os.Args[0]); // "exec.test"
    @string dir = filepath.Dir(os.Args[0]);    // "/tmp/go-buildNNNN/os/exec/_test"
    if (dir == "."u8) {
        t.Skip(skippingRunningTestAtˢ);
    }
    @string parentDir = filepath.Dir(dir); // "/tmp/go-buildNNNN/os/exec"
    @string dirBase = filepath.Base(dir);  // "_test"
    if (dirBase == "."u8) {
        t.Skipf("skipping; unexpected shallow dir of %q", dir);
    }

    cmd.Path = filepath.Join(dirBase, @base);
    cmd.Dir = parentDir;

    var (@out, err) = cmd.Output();
    if (err != nil) {
        t.Errorf("echo: %v", err);
    }
    @string g = (@string)@out;
    @string e = fooˢ2; if (g != e) {
        t.Errorf("echo: want %q, got %q", e, g);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string inputStringLine2ˢ => ref inputStringLine2ˢᵇ;
private static readonly @string inputStringLine2ˢᵇ = "Input string\nLine 2"u8;
internal static ref readonly @string catˢ => ref catˢᵇ;
private static readonly @string catˢᵇ = "cat"u8;

public static void TestCatStdin(ж<testing.T> t) {
    t.Parallel();

    // Cat, testing stdin and stdout.
    @string input = inputStringLine2ˢ;
    var p = helperCommand(t, catˢ);
    p.Stdin = new exec_test_package.strings_ReaderжReader(strings.NewReader(input));
    var (bs, err) = p.Output();
    if (err != nil) {
        t.Errorf("cat: %v", err);
    }
    @string s = (@string)bs;
    if (s != input) {
        t.Errorf("cat: want %q, got %q", input, s);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object echoˢ2 = (@string)"echo\n"u8;

public static partial void TestEchoFileRace(ж<testing.T> t) {
    t.Parallel();

    var cmd = helperCommand(t, echoˢ);
    var (stdin, err) = cmd.StdinPipe();
    if (err != nil) {
        t.Fatalf("StdinPipe: %v", err);
    }
    var errΔ1 = cmd.Start(); if (errΔ1 != nil) {
        t.Fatalf("Start: %v", errΔ1);
    }
    var wrote = new channel<bool>(0);
    var stdinʗ1 = stdin;
    var wroteʗ1 = wrote;
    goǃ(() => {
        GoFrame ᒐ = default;
        try {
            defer(ᴛ1 => close(ᴛ1), wroteʗ1, ref ᒐ);
            fmt.Fprint(stdinʗ1, echoˢ2);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
    var errΔ2 = cmd.Wait(); if (errΔ2 != nil) {
        t.Fatalf("Wait: %v", errΔ2);
    }
    ᐸꟷ(wrote);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string bogusFileFooˢ => ref bogusFileFooˢᵇ;
private static readonly @string bogusFileFooˢᵇ = "/bogus/file.foo"u8;
internal static ref readonly @string execTestGoˢ => ref execTestGoˢᵇ;
private static readonly @string execTestGoˢᵇ = "exec_test.go"u8;
internal static ref readonly @string errorOpenBogusFileFooˢ => ref errorOpenBogusFileFooˢᵇ;
private static readonly @string errorOpenBogusFileFooˢᵇ = "Error: open /bogus/file.foo"u8;
internal static ref readonly @string funcˢ => ref funcˢᵇ;
private static readonly @string funcˢᵇ = "func TestCatGoodAndBadFile(t *testing.T)"u8;

public static void TestCatGoodAndBadFile(ж<testing.T> t) {
    t.Parallel();

    // Testing combined output and error values.
    var (bs, err) = helperCommand(t, catˢ, bogusFileFooˢ, execTestGoˢ).CombinedOutput();
    var (_, okΔ1) = err._<ж<Δexec.ExitError>>(ᐧ); if (!okΔ1) {
        t.Errorf("expected *exec.ExitError from cat combined; got %T: %v", err, err);
    }
    var (errLine, body, ok) = strings.Cut((@string)bs, "\n");
    if (!ok) {
        t.Fatalf("expected two lines from cat; got %q", bs);
    }
    if (!strings.HasPrefix(errLine, errorOpenBogusFileFooˢ)) {
        t.Errorf("expected stderr to complain about file; got %q", errLine);
    }
    if (!strings.Contains(body, funcˢ)) {
        t.Errorf("expected test code; got %q (len %d)", body, len(body));
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string noExistExecutableˢ => ref noExistExecutableˢᵇ;
private static readonly @string noExistExecutableˢᵇ = "/no-exist-executable"u8;
internal static readonly object expectedErrorFromNoExistˢ = (@string)"expected error from /no-exist-executable"u8;

public static void TestNoExistExecutable(ж<testing.T> t) {
    t.Parallel();

    // Can't run a non-existent executable
    var err = Δexec.Command(noExistExecutableˢ).Run();
    if (err == nil) {
        t.Error(expectedErrorFromNoExistˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string exitˢ => ref exitˢᵇ;
private static readonly @string exitˢᵇ = "exit"u8;
internal static ref readonly @string exitStatus42ˢ => ref exitStatus42ˢᵇ;
private static readonly @string exitStatus42ˢᵇ = "exit status 42"u8;

public static void TestExitStatus(ж<testing.T> t) {
    t.Parallel();

    // Test that exit values are returned correctly
    var cmd = helperCommand(t, exitˢ, "42");
    var err = cmd.Run();
    @string want = exitStatus42ˢ;
    var exprᴛ1 = runtime.GOOS;
    switch (ᐧ) {
    case {} when exprᴛ1 == "plan9"u8: {
        want = fmt.Sprintf("exit status: '%s %d: 42'"u8, filepath.Base(cmd.Path), cmd.ProcessState.Pid());
        break;
    }}
    var (werr, ok) = err._<ж<Δexec.ExitError>>(ᐧ); if (ok) {
        @string s = werr.Error(); if (s != want) {
            t.Errorf("from exit 42 got exit %q, want %q", s, want);
        }
    } else {
        t.Fatalf("expected *exec.ExitError from exit 42; got %T: %v", err, err);
    }
}

public static void TestExitCode(ж<testing.T> t) {
    t.Parallel();

    // Test that exit code are returned correctly
    var cmd = helperCommand(t, exitˢ, "42");
    cmd.Run();
    nint want = 42;
    if (runtime.GOOS == "plan9"u8) {
        want = 1;
    }
    nint got = cmd.ProcessState.ExitCode();
    if (want != got) {
        t.Errorf("ExitCode got %d, want %d", got, want);
    }

    cmd = helperCommand(t, noExistExecutableˢ);
    cmd.Run();
    want = 2;
    if (runtime.GOOS == "plan9"u8) {
        want = 1;
    }
    got = cmd.ProcessState.ExitCode();
    if (want != got) {
        t.Errorf("ExitCode got %d, want %d", got, want);
    }

    cmd = helperCommand(t, exitˢ, "255");
    cmd.Run();
    want = 255;
    if (runtime.GOOS == "plan9"u8) {
        want = 1;
    }
    got = cmd.ProcessState.ExitCode();
    if (want != got) {
        t.Errorf("ExitCode got %d, want %d", got, want);
    }

    cmd = helperCommand(t, catˢ);
    cmd.Run();
    want = 0;
    got = cmd.ProcessState.ExitCode();
    if (want != got) {
        t.Errorf("ExitCode got %d, want %d", got, want);
    }

    // Test when command does not call Run().
    cmd = helperCommand(t, catˢ);
    want = -1;
    got = cmd.ProcessState.ExitCode();
    if (want != got) {
        t.Errorf("ExitCode got %d, want %d", got, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string pipetestˢ => ref pipetestˢᵇ;
private static readonly @string pipetestˢᵇ = "pipetest"u8;
internal static ref readonly @string stdinPipeˢ => ref stdinPipeˢᵇ;
private static readonly @string stdinPipeˢᵇ = "StdinPipe"u8;
internal static ref readonly @string stdoutPipeˢ => ref stdoutPipeˢᵇ;
private static readonly @string stdoutPipeˢᵇ = "StdoutPipe"u8;
internal static ref readonly @string stderrPipeˢ => ref stderrPipeˢᵇ;
private static readonly @string stderrPipeˢᵇ = "StderrPipe"u8;
internal static ref readonly @string startˢ => ref startˢᵇ;
private static readonly @string startˢᵇ = "Start"u8;
internal static ref readonly @string firstStdinWriteˢ => ref firstStdinWriteˢᵇ;
private static readonly @string firstStdinWriteˢᵇ = "first stdin Write"u8;
internal static ref readonly @string firstOutputLineˢ => ref firstOutputLineˢᵇ;
private static readonly @string firstOutputLineˢᵇ = "first output line"u8;
internal static ref readonly @string oIAmOutputˢ => ref oIAmOutputˢᵇ;
private static readonly @string oIAmOutputˢᵇ = "O:I am output"u8;
internal static ref readonly @string secondStdinWriteˢ => ref secondStdinWriteˢᵇ;
private static readonly @string secondStdinWriteˢᵇ = "second stdin Write"u8;
internal static ref readonly @string firstErrorLineˢ => ref firstErrorLineˢᵇ;
private static readonly @string firstErrorLineˢᵇ = "first error line"u8;
internal static ref readonly @string eIAmErrorˢ => ref eIAmErrorˢᵇ;
private static readonly @string eIAmErrorˢᵇ = "E:I am error"u8;
internal static ref readonly @string thirdStdinWrite3ˢ => ref thirdStdinWrite3ˢᵇ;
private static readonly @string thirdStdinWrite3ˢᵇ = "third stdin Write 3"u8;
internal static ref readonly @string secondOutputLineˢ => ref secondOutputLineˢᵇ;
private static readonly @string secondOutputLineˢᵇ = "second output line"u8;
internal static ref readonly @string oIAmOutput2ˢ => ref oIAmOutput2ˢᵇ;
private static readonly @string oIAmOutput2ˢᵇ = "O:I am output2"u8;
internal static ref readonly @string waitˢ => ref waitˢᵇ;
private static readonly @string waitˢᵇ = "Wait"u8;

public static void TestPipes(ж<testing.T> t) {
    t.Parallel();

    void check(@string what, error errΔ1) {
        if (errΔ1 != nil) {
            t.Fatalf("%s: %v", what, errΔ1);
        }
    }
    // Cat, testing stdin and stdout.
    var c = helperCommand(t, pipetestˢ);
    var (stdin, err) = c.StdinPipe();
    check(stdinPipeˢ, err);
    (var stdout, err) = c.StdoutPipe();
    check(stdoutPipeˢ, err);
    (var stderr, err) = c.StderrPipe();
    check(stderrPipeˢ, err);

    var outbr = bufio.NewReader(stdout);
    var errbr = bufio.NewReader(stderr);
    @string line(@string what, ж<bufio.Reader> br) {
        var (lineΔ1, _, errΔ2) = br.ReadLine();
        if (errΔ2 != nil) {
            t.Fatalf("%s: %v", what, errΔ2);
        }
        return (@string)lineΔ1;
    }

    err = c.Start();
    check(startˢ, err);

    (_, err) = stdin.Write(slice<byte>("O:I am output\n"u8));
    check(firstStdinWriteˢ, err);
    {
        @string g = line(firstOutputLineˢ, outbr);
        @string e = oIAmOutputˢ; if (g != e) {
            t.Errorf("got %q, want %q", g, e);
        }
    }

    (_, err) = stdin.Write(slice<byte>("E:I am error\n"u8));
    check(secondStdinWriteˢ, err);
    {
        @string g = line(firstErrorLineˢ, errbr);
        @string e = eIAmErrorˢ; if (g != e) {
            t.Errorf("got %q, want %q", g, e);
        }
    }

    (_, err) = stdin.Write(slice<byte>("O:I am output2\n"u8));
    check(thirdStdinWrite3ˢ, err);
    {
        @string g = line(secondOutputLineˢ, outbr);
        @string e = oIAmOutput2ˢ; if (g != e) {
            t.Errorf("got %q, want %q", g, e);
        }
    }

    stdin.Close();
    err = c.Wait();
    check(waitˢ, err);
}

internal static ref readonly @string stdinCloseTestString => ref stdinCloseTestStringᵇ;
private static readonly @string stdinCloseTestStringᵇ = "Some test string."u8;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string stdinCloseˢ => ref stdinCloseˢᵇ;
private static readonly @string stdinCloseˢᵇ = "stdinClose"u8;
internal static readonly object canTAccessMethodsOfˢ = (@string)"can't access methods of underlying *os.File"u8;
internal static ref readonly @string copyˢ => ref copyˢᵇ;
private static readonly @string copyˢᵇ = "Copy"u8;

internal partial interface TestStdinClose_type /*dyn*/ {
    uintptr Fd();
}

// Issue 6270.
public static partial void TestStdinClose(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        t.Parallel();

        void check(@string what, error errΔ1) {
            if (errΔ1 != nil) {
                t.Fatalf("%s: %v", what, errΔ1);
            }
        }
        var cmd = helperCommand(t, stdinCloseˢ);
        var (stdin, err) = cmd.StdinPipe();
        check(stdinPipeˢ, err);
        // Check that we can access methods of the underlying os.File.`
        var (_, ok) = stdin._<TestStdinClose_type>(ᐧ); if (!ok) {
            t.Error(canTAccessMethodsOfˢ);
        }
        check(startˢ, cmd.Start());

        ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
        Ꮡwg.Add(1);
        defer(Ꮡwg.Wait, ref ᒐ);
        var checkʗ1 = check;
        var stdinʗ1 = stdin;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);

                var (_, errΔ2) = io.Copy(stdinʗ1, new exec_test_package.strings_ReaderжReader(strings.NewReader(stdinCloseTestString)));
                checkʗ1(copyˢ, errΔ2);

                // Before the fix, this next line would race with cmd.Wait.
                var errΔ3 = stdinʗ1.Close(); if (errΔ3 != nil && !errors.Is(errΔ3, os.ErrClosed)) {
                    t.Errorf("Close: %v", errΔ3);
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });

        check(waitˢ, cmd.Wait());
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string unexpectedStringˢ => ref unexpectedStringˢᵇ;
private static readonly @string unexpectedStringˢᵇ = "unexpected string"u8;

// Issue 17647.
// It used to be the case that TestStdinClose, above, would fail when
// run under the race detector. This test is a variant of TestStdinClose
// that also used to fail when run under the race detector.
// This test is run by cmd/dist under the race detector to verify that
// the race detector no longer reports any problems.
public static partial void TestStdinCloseRace(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        t.Parallel();

        var cmd = helperCommand(t, stdinCloseˢ);
        var (stdin, err) = cmd.StdinPipe();
        if (err != nil) {
            t.Fatalf("StdinPipe: %v", err);
        }
        var errΔ1 = cmd.Start(); if (errΔ1 != nil) {
            t.Fatalf("Start: %v", errΔ1);
        }

        ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
        Ꮡwg.Add(2);
        defer(Ꮡwg.Wait, ref ᒐ);

        var cmdʗ1 = cmd;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                // We don't check the error return of Kill. It is
                // possible that the process has already exited, in
                // which case Kill will return an error "process
                // already finished". The purpose of this test is to
                // see whether the race detector reports an error; it
                // doesn't matter whether this Kill succeeds or not.
                cmdʗ1.Process.Kill();
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });

        var stdinʗ1 = stdin;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                // Send the wrong string, so that the child fails even
                // if the other goroutine doesn't manage to kill it first.
                // This test is to check that the race detector does not
                // falsely report an error, so it doesn't matter how the
                // child process fails.
                io.Copy(stdinʗ1, new exec_test_package.strings_ReaderжReader(strings.NewReader(unexpectedStringˢ)));
                var errΔ2 = stdinʗ1.Close(); if (errΔ2 != nil && !errors.Is(errΔ2, os.ErrClosed)) {
                    t.Errorf("stdin.Close: %v", errΔ2);
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });

        var errΔ3 = cmd.Wait(); if (errΔ3 == nil) {
            t.Fatalf("Wait: succeeded unexpectedly");
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object weDonTCurrentlySupporeˢ = (@string)"we don't currently suppore counting open handles on windows"u8;
internal static ref readonly @string somethingThatDoesNotˢ => ref somethingThatDoesNotˢᵇ;
private static readonly @string somethingThatDoesNotˢᵇ = "something-that-does-not-exist-executable"u8;
internal static readonly object unexpectedSuccessˢ = (@string)"unexpected success"u8;

// Issue 5071
public static void TestPipeLookPathLeak(ж<testing.T> t) {
    if (runtime.GOOS == "windows"u8) {
        t.Skip(weDonTCurrentlySupporeˢ);
    }
    // Not parallel: checks for leaked file descriptors

    slice<uintptr> openFDs() {
        var fdsᴷ = new stackAppend<uintptr>(moves: true);
        slice<uintptr> fds = default;
        for (var i = (uintptr)0; i < 100; i++) {
            if (fdtest.Exists(i)) {
                fds = append(fdsᴷ, fds, i);
            }
        }
        fds = move2heap(fdsᴷ, fds);
        return fds;
    }

    var old = new map<uintptr, bool>{};
    foreach (var (_, fd) in range(openFDs())) {
        old[fd] = true;
    }

    for (nint i = 0; i < 6; i++) {
        var cmd = Δexec.Command(somethingThatDoesNotˢ);
        cmd.StdoutPipe();
        cmd.StderrPipe();
        cmd.StdinPipe();
        var err = cmd.Run(); if (err == nil) {
            t.Fatal(unexpectedSuccessˢ);
        }
    }

    // Since this test is not running in parallel, we don't expect any new file
    // descriptors to be opened while it runs. However, if there are additional
    // FDs present at the start of the test (for example, opened by libc), those
    // may be closed due to a timeout of some sort. Allow those to go away, but
    // check that no new FDs are added.
    foreach (var (_, fd) in range(openFDs())) {
        if (!old[fd]) {
            t.Errorf("leaked file descriptor %v", fd);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingTestBecauseTestˢ = (@string)"skipping test because test was run with FDs open"u8;
internal static ref readonly @string tcpˢ => ref tcpˢᵇ;
private static readonly @string tcpˢᵇ = "tcp"u8;
internal static ref readonly @string read3Exeˢ => ref read3Exeˢᵇ;
private static readonly @string read3Exeˢᵇ = "read3.exe"u8;
internal static ref readonly @string buildˢ => ref buildˢᵇ;
private static readonly @string buildˢᵇ = "build"u8;
internal static ref readonly @string read3Goˢ => ref read3Goˢᵇ;
private static readonly @string read3Goˢᵇ = "read3.go"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string textᶜ => ref textᶜᵇ;
private static readonly @string textᶜᵇ = "Hello, fd 3!"u8;

public static void TestExtraFiles(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        if (testing.Short()) {
            Ꮡt.Skipf("skipping test in short mode that would build a helper binary");
        }

        if (haveUnexpectedFDs) {
            // The point of this test is to make sure that any
            // descriptors we open are marked close-on-exec.
            // If haveUnexpectedFDs is true then there were other
            // descriptors open when we started the test,
            // so those descriptors are clearly not close-on-exec,
            // and they will confuse the test. We could modify
            // the test to expect those descriptors to remain open,
            // but since we don't know where they came from or what
            // they are doing, that seems fragile. For example,
            // perhaps they are from the startup code on this
            // system for some reason. Also, this test is not
            // system-specific; as long as most systems do not skip
            // the test, we will still be testing what we care about.
            Ꮡt.Skip(skippingTestBecauseTestˢ);
        }

        testenv.MustHaveExec(new exec_test_package.testing_TжTB(Ꮡt));
        testenv.MustHaveGoBuild(new exec_test_package.testing_TжTB(Ꮡt));

        // This test runs with cgo disabled. External linking needs cgo, so
        // it doesn't work if external linking is required.
        //
        // N.B. go build below explictly doesn't pass through
        // -asan/-msan/-race, so we don't care about those.
        testenv.MustInternalLink(new exec_test_package.testing_TжTB(Ꮡt), testenv.NoSpecialBuildTypes);

        if (runtime.GOOS == "windows"u8) {
            Ꮡt.Skipf("skipping test on %q", runtime.GOOS);
        }

        // Force network usage, to verify the epoll (or whatever) fd
        // doesn't leak to the child,
        var (ln, err) = net.Listen(tcpˢ, "127.0.0.1:0");
        if (err != nil) {
            Ꮡt.Fatal(err);
        }
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);

        // Make sure duplicated fds don't leak to the child.
        (var f, err) = ln._<ж<net.TCPListener>>().File();
        if (err != nil) {
            Ꮡt.Fatal(err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        (var ln2, err) = net.FileListener(f);
        if (err != nil) {
            Ꮡt.Fatal(err);
        }
        var ln2ʗ1 = ln2;
        defer(() => ln2ʗ1.Close(), ref ᒐ);

        // Force TLS root certs to be loaded (which might involve
        // cgo), to make sure none of that potential C code leaks fds.
        var ts = httptest.NewUnstartedServer(new exec_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
        })));
        // quiet expected TLS handshake error "remote error: bad certificate"
        ts.Config.ErrorLog = log.New(io.Discard, ""u8, 0);
        ts.StartTLS();
        var tsʗ1 = ts;
        defer(tsʗ1.Close, ref ᒐ);
        (_, err) = http.Get(ts.URL);
        if (err == nil) {
            Ꮡt.Errorf("success trying to fetch %s; want an error", ts.URL);
        }

        (var tf, err) = os.CreateTemp(""u8, ""u8);
        if (err != nil) {
            Ꮡt.Fatalf("TempFile: %v", err);
        }
        defer(os.Remove, tf.Name(), ref ᒐ);
        var tfʗ1 = tf;
        defer(() => tfʗ1.Close(), ref ᒐ);

        @string text = textᶜ;
        (_, err) = tf.Write(slice<byte>(text));
        if (err != nil) {
            Ꮡt.Fatalf("Write: %v", err);
        }
        (_, err) = tf.Seek(0, io.SeekStart);
        if (err != nil) {
            Ꮡt.Fatalf("Seek: %v", err);
        }

        @string tempdir = Ꮡt.TempDir();
        @string exe = filepath.Join(tempdir, read3Exeˢ);

        var c = testenv.Command(new exec_test_package.testing_TжTB(Ꮡt), testenv.GoToolPath(new exec_test_package.testing_TжTB(Ꮡt)), buildˢ, "-o", exe, read3Goˢ);
        // Build the test without cgo, so that C library functions don't
        // open descriptors unexpectedly. See issue 25628.
        c.Env = append(os.Environ(), (@string)"CGO_ENABLED=0");
        var (output, errΔ1) = c.CombinedOutput(); if (errΔ1 != nil) {
            Ꮡt.Logf("go build -o %s read3.go\n%s", exe, output);
            Ꮡt.Fatalf("go build failed: %v", errΔ1);
        }

        // Use a deadline to try to get some output even if the program hangs.
        var ctx = context.Background();
        {
            var (deadline, ok) = t.Deadline(); if (ok) {
                // Leave a 20% grace period to flush output, which may be large on the
                // linux/386 builders because we're running the subprocess under strace.
                deadline = deadline.Add(-time.Until(deadline) / 5);

                Action cancel = default;
                (ctx, cancel) = context.WithDeadline(ctx, deadline);
                var cancelʗ1 = cancel;
                defer(() => cancelʗ1(), ref ᒐ);
            }
        }

        c = Δexec.CommandContext(ctx, exe);
        ref var stdout = ref heap(new strings.Builder(), out var Ꮡstdout);
        ref var stderr = ref heap(new strings.Builder(), out var Ꮡstderr);
        c.Stdout = new exec_test_package.strings_BuilderжWriter(Ꮡstdout);
        c.Stderr = new exec_test_package.strings_BuilderжWriter(Ꮡstderr);
        c.ExtraFiles = [tf];
        if (runtime.GOOS == "illumos"u8) {
            // Some facilities in illumos are implemented via access
            // to /proc by libc; such accesses can briefly occupy a
            // low-numbered fd.  If this occurs concurrently with the
            // test that checks for leaked descriptors, the check can
            // become confused and report a spurious leaked descriptor.
            // (See issue #42431 for more detailed analysis.)
            //
            // Attempt to constrain the use of additional threads in the
            // child process to make this test less flaky:
            c.Env = append(os.Environ(), (@string)"GOMAXPROCS=1");
        }
        err = c.Run();
        if (err != nil) {
            Ꮡt.Fatalf("Run: %v\n--- stdout:\n%s--- stderr:\n%s", err, stdout.String(), stderr.String());
        }
        if (stdout.String() != text) {
            Ꮡt.Errorf("got stdout %q, stderr %q; want %q on stdout", stdout.String(), stderr.String(), text);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string describefilesˢ => ref describefilesˢᵇ;
private static readonly @string describefilesˢᵇ = "describefiles"u8;
internal static readonly object noOperatingSystemSupportˢ = (@string)"no operating system support; skipping"u8;

public static partial void TestExtraFilesRace(ж<testing.T> t) {
    if (runtime.GOOS == "windows"u8) {
        maySkipHelperCommand(describefilesˢ);
        t.Skip(noOperatingSystemSupportˢ);
    }
    t.Parallel();

    net.Listener listen() {
        var (ln, err) = net.Listen(tcpˢ, "127.0.0.1:0");
        if (err != nil) {
            t.Fatal(err);
        }
        return ln;
    }
    ж<os.File> listenerFile(net.Listener ln) {
        var (f, err) = ln._<ж<net.TCPListener>>().File();
        if (err != nil) {
            t.Fatal(err);
        }
        return f;
    }
    void runCommand(ж<Δexec.Cmd> c, channel/*<-*/<@string> @out) {
        var (bout, err) = c.CombinedOutput();
        if (err != nil) {
            @out.ᐸꟷ("ERROR:"u8 + err.Error());
        } else {
            @out.ᐸꟷ(((@string)bout));
        }
    }

    for (nint i = 0; i < 10; i++) {
        if (testing.Short() && i >= 3) {
            break;
        }
        var la = listen();
        var ca = helperCommand(t, describefilesˢ);
        ca.ExtraFiles = slice<ж<os.File>>([listenerFile(la)]);
        var lb = listen();
        var cb = helperCommand(t, describefilesˢ);
        cb.ExtraFiles = slice<ж<os.File>>([listenerFile(lb)]);
        var ares = new channel<@string>(0);
        var bres = new channel<@string>(0);
        var runCommandʗ1 = runCommand;
        goǃ(runCommandʗ1, ca, ares.AsSendOnly());
        var runCommandʗ2 = runCommand;
        goǃ(runCommandʗ2, cb, bres.AsSendOnly());
        {
            @string got = ᐸꟷ(ares);
            @string want = fmt.Sprintf("fd3: listener %s\n"u8, la.Addr()); if (got != want) {
                t.Errorf("iteration %d, process A got:\n%s\nwant:\n%s\n", i, got, want);
            }
        }
        {
            @string got = ᐸꟷ(bres);
            @string want = fmt.Sprintf("fd3: listener %s\n"u8, lb.Addr()); if (got != want) {
                t.Errorf("iteration %d, process B got:\n%s\nwant:\n%s\n", i, got, want);
            }
        }
        la.Close();
        lb.Close();
        foreach (var (_, f) in range(ca.ExtraFiles)) {
            f.Close();
        }
        foreach (var (_, f) in range(cb.ExtraFiles)) {
            f.Close();
        }
    }
}

partial struct delayedInfiniteReader {
}

internal static (nint, error) Read(this delayedInfiniteReader _, slice<byte> b) {
    time.Sleep(100 * time.Millisecond);
    foreach (var i in range(len(b))) {
        b[i] = (byte)'x';
    }
    return (len(b), default);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string infiniteˢ => ref infiniteˢᵇ;
private static readonly @string infiniteˢᵇ = "Infinite"u8;

// Issue 9173: ignore stdin pipe writes if the program completes successfully.
public static void TestIgnorePipeErrorOnSuccess(ж<testing.T> t) {
    t.Parallel();

    Action<ж<testing.T>> testWith(io.Reader r) => (ж<testing.T> tΔ1) => {
            tΔ1.Parallel();

            var cmd = helperCommand(tΔ1, echoˢ, fooˢ);
            ref var @out = ref heap(new strings.Builder(), out var Ꮡout);
            cmd.Stdin = r;
            cmd.Stdout = new exec_test_package.strings_BuilderжWriter(Ꮡout);
            var err = cmd.Run(); if (err != nil) {
                tΔ1.Fatal(err);
            }
            @string got = @out.String();
            @string want = fooˢ2; if (got != want) {
                tΔ1.Errorf("output = %q; want %q", got, want);
            }
        };
    t.Run("10MB", testWith(new exec_test_package.strings_ReaderжReader(strings.NewReader(strings.Repeat("x", 10 << 20)))));
    t.Run(infiniteˢ, testWith(new delayedInfiniteReader()));
}

partial struct badWriter {
}

internal static (nint, error) Write(this ref badWriter w, slice<byte> data) {
    return (0, io.ErrUnexpectedEOF);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string yesˢ => ref yesˢᵇ;
private static readonly @string yesˢᵇ = "yes"u8;

public static void TestClosePipeOnCopyError(ж<testing.T> t) {
    t.Parallel();

    var cmd = helperCommand(t, yesˢ);
    cmd.Stdout = new exec_test_package.badWriterжWriter(@new<badWriter>());
    var err = cmd.Run();
    if (err == nil) {
        t.Errorf("yes unexpectedly completed successfully");
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string stderrfailˢ => ref stderrfailˢᵇ;
private static readonly @string stderrfailˢᵇ = "stderrfail"u8;
internal static ref readonly @string someStderrTextˢ => ref someStderrTextˢᵇ;
private static readonly @string someStderrTextˢᵇ = "some stderr text\n"u8;

public static void TestOutputStderrCapture(ж<testing.T> t) {
    t.Parallel();

    var cmd = helperCommand(t, stderrfailˢ);
    var (_, err) = cmd.Output();
    var (ee, ok) = err._<ж<Δexec.ExitError>>(ᐧ);
    if (!ok) {
        t.Fatalf("Output error type = %T; want ExitError", err);
    }
    @string got = (@string)ee.Stderr;
    @string want = someStderrTextˢ;
    if (got != want) {
        t.Errorf("ExitError.Stderr = %q; want %q", got, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedWaitFailureˢ = (@string)"expected Wait failure"u8;

public static partial void TestContext(ж<testing.T> t) {
    t.Parallel();

    var (ctx, cancel) = context.WithCancel(context.Background());
    var c = helperCommandContext(t, ctx, pipetestˢ);
    var (stdin, err) = c.StdinPipe();
    if (err != nil) {
        t.Fatal(err);
    }
    (var stdout, err) = c.StdoutPipe();
    if (err != nil) {
        t.Fatal(err);
    }
    var errΔ1 = c.Start(); if (errΔ1 != nil) {
        t.Fatal(errΔ1);
    }

    var (_, errΔ2) = stdin.Write(slice<byte>("O:hi\n"u8)); if (errΔ2 != nil) {
        t.Fatal(errΔ2);
    }
    var buf = new slice<byte>(5);
    (var n, err) = io.ReadFull(stdout, buf);
    if (n != len(buf) || err != nil || ((sstring)buf) != "O:hi\n"u8) {
        t.Fatalf("ReadFull = %d, %v, %q", n, err, buf.slice(0, n));
    }
    var cancelʗ1 = cancel;
    goǃ(() => cancelʗ1());

    var errΔ3 = c.Wait(); if (errΔ3 == nil) {
        t.Fatal(expectedWaitFailureˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string systemˢ => ref systemˢᵇ;
private static readonly @string systemˢᵇ = "system"u8;
internal static readonly object programUnexpectedlyˢ = (@string)"program unexpectedly exited successfully"u8;

public static void TestContextCancel(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        if (runtime.GOOS == "netbsd"u8 && runtime.GOARCH == "arm64"u8) {
            maySkipHelperCommand(catˢ);
            testenv.SkipFlaky(new exec_test_package.testing_TжTB(t), 42061);
        }

        // To reduce noise in the final goroutine dump,
        // let other parallel tests complete if possible.
        t.Parallel();

        var (ctx, cancel) = context.WithCancel(context.Background());
        var cancelʗ1 = cancel;
        defer(() => cancelʗ1(), ref ᒐ);
        var c = helperCommandContext(t, ctx, catˢ);

        var (stdin, err) = c.StdinPipe();
        if (err != nil) {
            t.Fatal(err);
        }
        var stdinʗ1 = stdin;
        defer(() => stdinʗ1.Close(), ref ᒐ);

        var errΔ1 = c.Start(); if (errΔ1 != nil) {
            t.Fatal(errΔ1);
        }

        // At this point the process is alive. Ensure it by sending data to stdin.
        var (_, errΔ2) = io.WriteString(stdin, echoˢ); if (errΔ2 != nil) {
            t.Fatal(errΔ2);
        }

        cancel();

        // Calling cancel should have killed the process, so writes
        // should now fail.  Give the process a little while to die.
        var start = time.Now();
        var delay = 1 * time.Millisecond;
        for (;;) {
            var (_, errΔ3) = io.WriteString(stdin, echoˢ); if (errΔ3 != nil) {
                break;
            }

            if (time.Since(start) > time.ΔMinute) {
                // Panic instead of calling t.Fatal so that we get a goroutine dump.
                // We want to know exactly what the os/exec goroutines got stuck on.
                debug.SetTraceback(systemˢ);
                throw panic("canceling context did not stop program");
            }

            // Back off exponentially (up to 1-second sleeps) to give the OS time to
            // terminate the process.
            delay *= 2;
            if (delay > 1 * time.ΔSecond) {
                delay = 1 * time.ΔSecond;
            }
            time.Sleep(delay);
        }

        var errΔ4 = c.Wait(); if (errΔ4 == nil) {
            t.Error(programUnexpectedlyˢ);
        } else {
            t.Logf("exit status: %v", errΔ4);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string echoenvˢ => ref echoenvˢᵇ;
private static readonly @string echoenvˢᵇ = "echoenv"u8;
internal static ref readonly @string fooˢ3 => ref fooˢ3ᵇ;
private static readonly @string fooˢ3ᵇ = "FOO"u8;
internal static ref readonly @string goodˢ => ref goodˢᵇ;
private static readonly @string goodˢᵇ = "good"u8;

// test that environment variables are de-duped.
public static void TestDedupEnvEcho(ж<testing.T> t) {
    t.Parallel();

    var cmd = helperCommand(t, echoenvˢ, fooˢ3);
    cmd.Env = append(cmd.Environ(), (@string)"FOO=bad", (@string)"FOO=good");
    var (@out, err) = cmd.CombinedOutput();
    if (err != nil) {
        t.Fatal(err);
    }
    @string got = strings.TrimSpace((@string)@out);
    @string want = goodˢ; if (got != want) {
        t.Errorf("output = %q; want %q", got, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object plan9ExplicitlyAllowsNulˢ = (@string)"plan9 explicitly allows NUL in the environment"u8;
internal static ref readonly @string barˢ => ref barˢᵇ;
private static readonly @string barˢᵇ = "BAR"u8;

public static void TestEnvNULCharacter(ж<testing.T> t) {
    if (runtime.GOOS == "plan9"u8) {
        t.Skip(plan9ExplicitlyAllowsNulˢ);
    }
    var cmd = helperCommand(t, echoenvˢ, fooˢ3, barˢ);
    cmd.Env = append(cmd.Environ(), ((@string)(new byte[]{0x46, 0x4f, 0x4f, 0x3d, 0x66, 0x6f, 0x6f, 0x00, 0x42, 0x41, 0x52, 0x3d, 0x62, 0x61, 0x72})));
    var (@out, err) = cmd.CombinedOutput();
    if (err == nil) {
        t.Errorf("output = %q; want error", (@string)@out);
    }
}

internal partial struct TestString_tests /*dyn*/ {
    internal @string path;
    internal slice<@string> args;
    internal @string want;
}

public static void TestString(ж<testing.T> t) {
    t.Parallel();

    var (echoPath, err) = Δexec.LookPath(echoˢ);
    if (err != nil) {
        t.Skip(err);
    }
    var tests = new TestString_tests[]{
        new("echo", default, echoPath),
        new("echo", ["a"], echoPath + " a"u8),
        new("echo", ["a", "b"], echoPath + " a b"u8)
    }.array();
    foreach (var (_, test) in range(tests.ΔRangeSnapshot())) {
        var cmd = Δexec.Command(test.path, test.args.ꓸꓸꓸ);
        @string got = cmd.String(); if (got != test.want) {
            t.Errorf("String(%q, %q) = %q, want %q", test.path, test.args, got, test.want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string makemeasandwichˢ => ref makemeasandwichˢᵇ;
private static readonly @string makemeasandwichˢᵇ = "makemeasandwich"u8;
internal static readonly object wowThanksˢ = (@string)"wow, thanks"u8;
internal static ref readonly @string lettuceˢ => ref lettuceˢᵇ;
private static readonly @string lettuceˢᵇ = "-lettuce"u8;
internal static ref readonly @string makemeasandwichLettuceˢ => ref makemeasandwichLettuceˢᵇ;
private static readonly @string makemeasandwichLettuceˢᵇ = "makemeasandwich -lettuce"u8;

public static void TestStringPathNotResolved(ж<testing.T> t) {
    t.Parallel();

    var (_, err) = Δexec.LookPath(makemeasandwichˢ);
    if (err == nil) {
        t.Skip(wowThanksˢ);
    }

    var cmd = Δexec.Command(makemeasandwichˢ, lettuceˢ);
    @string want = makemeasandwichLettuceˢ;
    @string got = cmd.String(); if (got != want) {
        t.Errorf("String(%q, %q) = %q, want %q", makemeasandwichˢ, lettuceˢ, got, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string execNoCommandˢ => ref execNoCommandˢᵇ;
private static readonly @string execNoCommandˢᵇ = "exec: no command"u8;

public static void TestNoPath(ж<testing.T> t) {
    var err = @new<Δexec.Cmd>().Start();
    @string want = execNoCommandˢ;
    if (err == nil || err.Error() != want) {
        t.Errorf("new(Cmd).Start() = %v, want %q", err, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string alreadyStartedˢ => ref alreadyStartedˢᵇ;
private static readonly @string alreadyStartedˢᵇ = "already started"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string msgᶜ => ref msgᶜᵇ;
private static readonly @string msgᶜᵇ = "O:Hello, pipe!\n"u8;

// TestDoubleStartLeavesPipesOpen checks for a regression in which calling
// Start twice, which returns an error on the second call, would spuriously
// close the pipes established in the first call.
public static partial void TestDoubleStartLeavesPipesOpen(ж<testing.T> t) {
    t.Parallel();

    var cmd = helperCommand(t, pipetestˢ);
    var (@in, err) = cmd.StdinPipe();
    if (err != nil) {
        t.Fatal(err);
    }
    (var @out, err) = cmd.StdoutPipe();
    if (err != nil) {
        t.Fatal(err);
    }

    var errΔ1 = cmd.Start(); if (errΔ1 != nil) {
        t.Fatal(errΔ1);
    }
    var cmdʗ1 = cmd;
    t.Cleanup(() => {
        var errΔ2 = cmdʗ1.Wait(); if (errΔ2 != nil) {
            t.Error(errΔ2);
        }
    });

    var errΔ3 = cmd.Start(); if (errΔ3 == nil || !strings.HasSuffix(errΔ3.Error(), alreadyStartedˢ)) {
        t.Fatalf("second call to Start returned a nil; want an 'already started' error");
    }

    var outc = new channel<slice<byte>>(1);
    var outʗ1 = @out;
    var outcʗ1 = outc;
    goǃ(() => {
        var (bΔ1, errΔ4) = io.ReadAll(outʗ1);
        if (errΔ4 != nil) {
            t.Error(errΔ4);
        }
        outcʗ1.ᐸꟷ(bΔ1);
    });

    @string msg = msgᶜ;

    (_, err) = io.WriteString(@in, msg);
    if (err != nil) {
        t.Fatal(err);
    }
    @in.Close();

    var b = ᐸꟷ(outc);
    if (!bytes.Equal(b, slice<byte>(msg))) {
        t.Fatalf("read %q from stdout pipe; want %q", b, msg);
    }
}

internal static partial void cmdHang(params ꓸꓸꓸstring argsʗp) {
    var args = argsʗp.slice();

    var (sleep, err) = time.ParseDuration(args[0]);
    if (err != nil) {
        throw panic(err);
    }

    var fs = flag.NewFlagSet("hang", flag.ExitOnError);
    var exitOnInterrupt = fs.Bool("interrupt", false, "if true, commands should exit 0 on os.Interrupt");
    var subsleep = fs.Duration("subsleep", 0, "amount of time for the 'hang' helper to leave an orphaned subprocess sleeping with stderr open");
    var probe = fs.Duration("probe", 0, "if nonzero, the 'hang' helper should write to stderr at this interval, and exit nonzero if a write fails");
    var read = fs.Bool("read", false, "if true, the 'hang' helper should read stdin to completion before sleeping");
    fs.Parse(args[1..]);

    nint pid = os.Getpid();

    if (subsleep.Value != 0) {
        var cmd = Δexec.Command(testenv.Executable(default), "hang", subsleep.String(), "-read=true", "-probe=" + probe.String());
        cmd.Stdin = new exec_test_package.os_FileжReader(os.Stdin);
        cmd.Stderr = new os.FileжWriter(os.Stderr);
        var (@out, errΔ1) = cmd.StdoutPipe();
        if (errΔ1 != nil) {
            fmt.Fprintln(new os.FileжWriter(os.Stderr), errΔ1);
            os.Exit(1);
        }
        cmd.Start();

        var buf = @new<strings.Builder>();
        var (_, errΔ2) = io.Copy(new exec_test_package.strings_BuilderжWriter(buf), @out); if (errΔ2 != nil) {
            fmt.Fprintln(new os.FileжWriter(os.Stderr), errΔ2);
            cmd.Process.Kill();
            cmd.Wait();
            os.Exit(1);
        }
        fmt.Fprintf(new os.FileжWriter(os.Stderr), "%d: started %d: %v\n"u8, pid, cmd.Process.Pid, cmd.OrTypedNil());
        var cmdʗ1 = cmd;
        goǃ(() => cmdʗ1.Wait()); // Release resources if cmd happens not to outlive this process.
    }

    if (exitOnInterrupt.Value) {
        var c = new channel<osꓸSignal>(1);
        signal.Notify(c.AsSendOnly(), os.Interrupt);
        var cʗ1 = c;
        goǃ(() => {
            var sig = ᐸꟷ(cʗ1);
            fmt.Fprintf(new os.FileжWriter(os.Stderr), "%d: received %v\n"u8, pid, sig);
            os.Exit(0);
        });
    } else {
        signal.Ignore(os.Interrupt);
    }

    // Signal that the process is set up by closing stdout.
    os.Stdout.Close();

    if (read.Value) {
        if (pipeSignal != nil) {
            signal.Ignore(pipeSignal);
        }
        var r = bufio.NewReader(new exec_test_package.os_FileжReader(os.Stdin));
        for (;;) {
            var (line, errΔ3) = r.ReadBytes((byte)'\n');
            if (len(line) > 0) {
                // Ignore write errors: we want to keep reading even if stderr is closed.
                fmt.Fprintf(new os.FileжWriter(os.Stderr), "%d: read %s"u8, pid, line);
            }
            if (errΔ3 != nil) {
                fmt.Fprintf(new os.FileжWriter(os.Stderr), "%d: finished read: %v"u8, pid, errΔ3);
                break;
            }
        }
    }

    if (probe.Value != 0) {
        var ticker = time.NewTicker(probe.Value);
        var tickerʗ1 = ticker;
        goǃ(() => {
            foreach (var _ᴛ1 in tickerʗ1.C) {
                var (_, errΔ4) = fmt.Fprintf(new os.FileжWriter(os.Stderr), "%d: ok\n"u8, pid); if (errΔ4 != nil) {
                    os.Exit(1);
                }
            }
        });
    }

    if (sleep != 0) {
        time.Sleep(sleep);
        fmt.Fprintf(new os.FileжWriter(os.Stderr), "%d: slept %v\n"u8, pid, sleep);
    }
}

// A tickReader reads an unbounded sequence of timestamps at no more than a
// fixed interval.
partial struct tickReader {
    internal time.Duration interval;
    internal time.Time lastTick;
    internal @string s;
}

internal static ж<tickReader> newTickReader(time.Duration interval) {
    return Ꮡ(new tickReader(interval: interval));
}

internal static (nint n, error err) Read(this ref tickReader r, slice<byte> p) {
    nint n = default;

    if (len(r.s) == 0) {
        var d = r.interval - time.Since(r.lastTick); if (d > 0) {
            time.Sleep(d);
        }
        r.lastTick = time.Now();
        r.s = r.lastTick.Format(time.RFC3339Nano + "\n");
    }

    n = copy(p, r.s);
    r.s = r.s.slice(n);
    return (n, default);
}

internal static ж<Δexec.Cmd> startHang(ж<testing.T> t, context.Context ctx, time.Duration hangTime, osꓸSignal interrupt, time.Duration waitDelay, params ꓸꓸꓸstring flagsʗp) {
    var flags = flagsʗp.sslice();

    t.Helper();

    var args = appendꓸꓸꓸ(slice<@string>([hangTime.String()]), flags);
    var cmd = helperCommandContext(t, ctx, "hang"u8, args.ꓸꓸꓸ);
    cmd.Stdin = new exec_test_package.tickReaderжReader(newTickReader(1 * time.Millisecond));
    cmd.Stderr = new exec_test_package.strings_BuilderжWriter(@new<strings.Builder>());
    if (interrupt == nil) {
        cmd.Cancel = default;
    } else {
        var cmdʗ1 = cmd;
        cmd.Cancel = () => cmdʗ1.Process.Signal(interrupt);
    }
    cmd.WaitDelay = waitDelay;
    var (@out, err) = cmd.StdoutPipe();
    if (err != nil) {
        t.Fatal(err);
    }

    t.Log(cmd.OrTypedNil());
    var errΔ1 = cmd.Start(); if (errΔ1 != nil) {
        t.Fatal(errΔ1);
    }

    // Wait for cmd to close stdout to signal that its handlers are installed.
    var buf = @new<strings.Builder>();
    var (_, errΔ2) = io.Copy(new exec_test_package.strings_BuilderжWriter(buf), @out); if (errΔ2 != nil) {
        t.Error(errΔ2);
        cmd.Process.Kill();
        cmd.Wait();
        t.FailNow();
    }
    if (buf.Len() > 0) {
        t.Logf("stdout %v:\n%s", cmd.Args, buf.OrTypedNil());
    }

    return cmd;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string waitDelayˢ => ref waitDelayˢᵇ;
private static readonly @string waitDelayˢᵇ = "WaitDelay"u8;
internal static ref readonly @string interruptTrueˢ => ref interruptTrueˢᵇ;
private static readonly @string interruptTrueˢᵇ = "-interrupt=true"u8;
internal static ref readonly @string sigkillHangˢ => ref sigkillHangˢᵇ;
private static readonly @string sigkillHangˢᵇ = "SIGKILL-hang"u8;
internal static ref readonly @string subsleep10mˢ => ref subsleep10mˢᵇ;
private static readonly @string subsleep10mˢᵇ = "-subsleep=10m"u8;
internal static ref readonly @string probe1msˢ => ref probe1msˢᵇ;
private static readonly @string probe1msˢᵇ = "-probe=1ms"u8;
internal static ref readonly @string exitHangˢ => ref exitHangˢᵇ;
private static readonly @string exitHangˢᵇ = "Exit-hang"u8;
internal static ref readonly @string sigintIgnoredˢ => ref sigintIgnoredˢᵇ;
private static readonly @string sigintIgnoredˢᵇ = "SIGINT-ignored"u8;
internal static ref readonly @string interruptFalseˢ => ref interruptFalseˢᵇ;
private static readonly @string interruptFalseˢᵇ = "-interrupt=false"u8;
internal static ref readonly @string sigintHandledˢ => ref sigintHandledˢᵇ;
private static readonly @string sigintHandledˢᵇ = "SIGINT-handled"u8;
internal static ref readonly @string sigquitˢ => ref sigquitˢᵇ;
private static readonly @string sigquitˢᵇ = "SIGQUIT"u8;
internal static ref readonly @string goroutineˢ => ref goroutineˢᵇ;
private static readonly @string goroutineˢᵇ = "\n\ngoroutine "u8;

public static void TestWaitInterrupt(ж<testing.T> t) {
    t.Parallel();

    // tooLong is an arbitrary duration that is expected to be much longer than
    // the test runs, but short enough that leaked processes will eventually exit
    // on their own.
    time.Duration tooLong = /* 10 * time.Minute */ 600000000000;

    // Control case: with no cancellation and no WaitDelay, we should wait for the
    // process to exit.
    t.Run(waitˢ, (ж<testing.T> tΔ1) => {
        tΔ1.Parallel();
        var cmd = startHang(tΔ1, context.Background(), 1 * time.Millisecond, os.ΔKill, 0);
        var err = cmd.Wait();
        tΔ1.Logf("stderr:\n%s", cmd.Stderr);
        tΔ1.Logf("[%d] %v", cmd.Process.Pid, err);

        if (err != nil) {
            tΔ1.Errorf("Wait: %v; want <nil>", err);
        }
        var ps = cmd.ProcessState; if (!ps.Exited()) {
            tΔ1.Errorf("cmd did not exit: %v", ps.OrTypedNil());
        } else {
            nint code = ps.ExitCode(); if (code != 0) {
                tΔ1.Errorf("cmd.ProcessState.ExitCode() = %v; want 0", code);
            }
        }
    });

    // With a very long WaitDelay and no Cancel function, we should wait for the
    // process to exit even if the command's Context is canceled.
    t.Run(waitDelayˢ, (ж<testing.T> tΔ2) => {
        if (runtime.GOOS == "windows"u8) {
            tΔ2.Skipf("skipping: os.Interrupt is not implemented on Windows");
        }
        tΔ2.Parallel();

        var (ctx, cancel) = context.WithCancel(context.Background());
        var cmd = startHang(tΔ2, ctx, tooLong, default, tooLong, interruptTrueˢ);
        cancel();

        time.Sleep(1 * time.Millisecond);
        // At this point cmd should still be running (because we passed nil to
        // startHang for the cancel signal). Sending it an explicit Interrupt signal
        // should succeed.
        var errΔ1 = cmd.Process.Signal(os.Interrupt); if (errΔ1 != nil) {
            tΔ2.Error(errΔ1);
        }

        var err = cmd.Wait();
        tΔ2.Logf("stderr:\n%s", cmd.Stderr);
        tΔ2.Logf("[%d] %v", cmd.Process.Pid, err);

        // This program exits with status 0,
        // but pretty much always does so during the wait delay.
        // Since the Cmd itself didn't do anything to stop the process when the
        // context expired, a successful exit is valid (even if late) and does
        // not merit a non-nil error.
        if (err != nil) {
            tΔ2.Errorf("Wait: %v; want nil", err);
        }
        var ps = cmd.ProcessState; if (!ps.Exited()) {
            tΔ2.Errorf("cmd did not exit: %v", ps.OrTypedNil());
        } else {
            nint code = ps.ExitCode(); if (code != 0) {
                tΔ2.Errorf("cmd.ProcessState.ExitCode() = %v; want 0", code);
            }
        }
    });

    // If the context is canceled and the Cancel function sends os.Kill,
    // the process should be terminated immediately, and its output
    // pipes should be closed (causing Wait to return) after WaitDelay
    // even if a child process is still writing to them.
    t.Run(sigkillHangˢ, (ж<testing.T> tΔ3) => {
        tΔ3.Parallel();

        var (ctx, cancel) = context.WithCancel(context.Background());
        var cmd = startHang(tΔ3, ctx, tooLong, os.ΔKill, 10 * time.Millisecond, subsleep10mˢ, probe1msˢ);
        cancel();
        var err = cmd.Wait();
        tΔ3.Logf("stderr:\n%s", cmd.Stderr);
        tΔ3.Logf("[%d] %v", cmd.Process.Pid, err);

        // This test should kill the child process after 10ms,
        // leaving a grandchild process writing probes in a loop.
        // The child process should be reported as failed,
        // and the grandchild will exit (or die by SIGPIPE) once the
        // stderr pipe is closed.
        var (ee, ok) = errors.AsType<ж<Δexec.ExitError>>(err); if (!ok) {
            tΔ3.Errorf("Wait error = %v; want %T", err, ee.OrTypedNil());
        }
    });

    // If the process exits with status 0 but leaves a child behind writing
    // to its output pipes, Wait should only wait for WaitDelay before
    // closing the pipes and returning.  Wait should return ErrWaitDelay
    // to indicate that the piped output may be incomplete even though the
    // command returned a “success” code.
    t.Run(exitHangˢ, (ж<testing.T> tΔ4) => {
        tΔ4.Parallel();

        var cmd = startHang(tΔ4, context.Background(), 1 * time.Millisecond, default, 10 * time.Millisecond, subsleep10mˢ, probe1msˢ);
        var err = cmd.Wait();
        tΔ4.Logf("stderr:\n%s", cmd.Stderr);
        tΔ4.Logf("[%d] %v", cmd.Process.Pid, err);

        // This child process should exit immediately,
        // leaving a grandchild process writing probes in a loop.
        // Since the child has no ExitError to report but we did not
        // read all of its output, Wait should return ErrWaitDelay.
        if (!errors.Is(err, Δexec.ErrWaitDelay)) {
            tΔ4.Errorf("Wait error = %v; want %T", err, Δexec.ErrWaitDelay);
        }
    });

    // If the Cancel function sends a signal that the process can handle, and it
    // handles that signal without actually exiting, then it should be terminated
    // after the WaitDelay.
    t.Run(sigintIgnoredˢ, (ж<testing.T> tΔ5) => {
        if (runtime.GOOS == "windows"u8) {
            tΔ5.Skipf("skipping: os.Interrupt is not implemented on Windows");
        }
        tΔ5.Parallel();

        var (ctx, cancel) = context.WithCancel(context.Background());
        var cmd = startHang(tΔ5, ctx, tooLong, os.Interrupt, 10 * time.Millisecond, interruptFalseˢ);
        cancel();
        var err = cmd.Wait();
        tΔ5.Logf("stderr:\n%s", cmd.Stderr);
        tΔ5.Logf("[%d] %v", cmd.Process.Pid, err);

        // This command ignores SIGINT, sleeping until it is killed.
        // Wait should return the usual error for a killed process.
        var (ee, ok) = errors.AsType<ж<Δexec.ExitError>>(err); if (!ok) {
            tΔ5.Errorf("Wait error = %v; want %T", err, ee.OrTypedNil());
        }
    });

    // If the process handles the cancellation signal and exits with status 0,
    // Wait should report a non-nil error (because the process had to be
    // interrupted), and it should be a context error (because there is no error
    // to report from the child process itself).
    t.Run(sigintHandledˢ, (ж<testing.T> tΔ6) => {
        if (runtime.GOOS == "windows"u8) {
            tΔ6.Skipf("skipping: os.Interrupt is not implemented on Windows");
        }
        tΔ6.Parallel();

        var (ctx, cancel) = context.WithCancel(context.Background());
        var cmd = startHang(tΔ6, ctx, tooLong, os.Interrupt, 0, interruptTrueˢ);
        cancel();
        var err = cmd.Wait();
        tΔ6.Logf("stderr:\n%s", cmd.Stderr);
        tΔ6.Logf("[%d] %v", cmd.Process.Pid, err);

        if (!errors.Is(err, ctx.Err())) {
            tΔ6.Errorf("Wait error = %v; want %v", err, ctx.Err());
        }
        var ps = cmd.ProcessState; if (!ps.Exited()) {
            tΔ6.Errorf("cmd did not exit: %v", ps.OrTypedNil());
        } else {
            nint code = ps.ExitCode(); if (code != 0) {
                tΔ6.Errorf("cmd.ProcessState.ExitCode() = %v; want 0", code);
            }
        }
    });

    // If the Cancel function sends SIGQUIT, it should be handled in the usual
    // way: a Go program should dump its goroutines and exit with non-success
    // status. (We expect SIGQUIT to be a common pattern in real-world use.)
    t.Run(sigquitˢ, (ж<testing.T> tΔ7) => {
        if (quitSignal == nil) {
            tΔ7.Skipf("skipping: SIGQUIT is not supported on %v", runtime.GOOS);
        }
        tΔ7.Parallel();

        var (ctx, cancel) = context.WithCancel(context.Background());
        var cmd = startHang(tΔ7, ctx, tooLong, quitSignal, 0);
        cancel();
        var err = cmd.Wait();
        tΔ7.Logf("stderr:\n%s", cmd.Stderr);
        tΔ7.Logf("[%d] %v", cmd.Process.Pid, err);

        var (_, ok) = errors.AsType<ж<Δexec.ExitError>>(err); if (!ok) {
            tΔ7.Errorf("Wait error = %v; want %v", err, ctx.Err());
        }

        var ps = cmd.ProcessState; if (!ps.Exited()) {
            tΔ7.Errorf("cmd did not exit: %v", ps.OrTypedNil());
        } else {
            nint code = ps.ExitCode(); if (code != 2) {
                // The default os/signal handler exits with code 2.
                tΔ7.Errorf("cmd.ProcessState.ExitCode() = %v; want 2", code);
            }
        }

        if (!strings.Contains(fmt.Sprint(cmd.Stderr), goroutineˢ)) {
            tΔ7.Errorf("cmd.Stderr does not contain a goroutine dump");
        }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string successAfterErrorˢ => ref successAfterErrorˢᵇ;
private static readonly @string successAfterErrorˢᵇ = "success after error"u8;
internal static ref readonly @string arbitraryErrorˢ => ref arbitraryErrorˢᵇ;
private static readonly @string arbitraryErrorˢᵇ = "arbitrary error"u8;
internal static ref readonly @string successAfterˢ => ref successAfterˢᵇ;
private static readonly @string successAfterˢᵇ = "success after ErrProcessDone"u8;
internal static ref readonly @string killedAfterErrorˢ => ref killedAfterErrorˢᵇ;
private static readonly @string killedAfterErrorˢᵇ = "killed after error"u8;
internal static ref readonly @string killedAfterSpuriousˢ => ref killedAfterSpuriousˢᵇ;
private static readonly @string killedAfterSpuriousˢᵇ = "killed after spurious ErrProcessDone"u8;
internal static ref readonly @string nonzeroExitAfterErrorˢ => ref nonzeroExitAfterErrorˢᵇ;
private static readonly @string nonzeroExitAfterErrorˢᵇ = "nonzero exit after error"u8;

public static void TestCancelErrors(ж<testing.T> t) {
    t.Parallel();

    // If Cancel returns a non-ErrProcessDone error and the process
    // exits successfully, Wait should wrap the error from Cancel.
    t.Run(successAfterErrorˢ, (ж<testing.T> tΔ1) => {
        GoFrame ᒐ = default;
        try {
            tΔ1.Parallel();

            var (ctx, cancel) = context.WithCancel(context.Background());
            var cancelʗ1 = cancel;
            defer(() => cancelʗ1(), ref ᒐ);

            var cmd = helperCommandContext(tΔ1, ctx, pipetestˢ);
            var (stdin, err) = cmd.StdinPipe();
            if (err != nil) {
                tΔ1.Fatal(err);
            }

            var errArbitrary = errors.New(arbitraryErrorˢ);
            var errArbitraryʗ1 = errArbitrary;
            var stdinʗ1 = stdin;
            cmd.Cancel = () => {
                stdinʗ1.Close();
                tΔ1.Logf("Cancel returning %v", errArbitraryʗ1);
                return errArbitraryʗ1;
            };
            var errΔ1 = cmd.Start(); if (errΔ1 != nil) {
                tΔ1.Fatal(errΔ1);
            }
            cancel();

            err = cmd.Wait();
            tΔ1.Logf("[%d] %v", cmd.Process.Pid, err);
            if (!errors.Is(err, errArbitrary) || AreEqual(err, errArbitrary)) {
                tΔ1.Errorf("Wait error = %v; want an error wrapping %v", err, errArbitrary);
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });

    // If Cancel returns an error equivalent to ErrProcessDone,
    // Wait should ignore that error. (ErrProcessDone indicates that the
    // process was already done before we tried to interrupt it — maybe we
    // just didn't notice because Wait hadn't been called yet.)
    t.Run(successAfterˢ, (ж<testing.T> tΔ2) => {
        GoFrame ᒐ = default;
        try {
            tΔ2.Parallel();

            var (ctx, cancel) = context.WithCancel(context.Background());
            var cancelʗ2 = cancel;
            defer(() => cancelʗ2(), ref ᒐ);

            var cmd = helperCommandContext(tΔ2, ctx, pipetestˢ);
            var (stdin, err) = cmd.StdinPipe();
            if (err != nil) {
                tΔ2.Fatal(err);
            }

            (var stdout, err) = cmd.StdoutPipe();
            if (err != nil) {
                tΔ2.Fatal(err);
            }

            // We intentionally race Cancel against the process exiting,
            // but ensure that the process wins the race (and return ErrProcessDone
            // from Cancel to report that).
            var interruptCalled = new channel<EmptyStruct>(0);
            var done = new channel<EmptyStruct>(0);
            var doneʗ1 = done;
            var interruptCalledʗ1 = interruptCalled;
            cmd.Cancel = () => {
                close(interruptCalledʗ1);
                ᐸꟷ(doneʗ1);
                tΔ2.Logf("Cancel returning an error wrapping ErrProcessDone");
                return fmt.Errorf("%w: stdout closed"u8, os.ErrProcessDone);
            };

            var errΔ1 = cmd.Start(); if (errΔ1 != nil) {
                tΔ2.Fatal(errΔ1);
            }

            cancel();
            ᐸꟷ(interruptCalled);
            stdin.Close();
            io.Copy(io.Discard, stdout); // reaches EOF when the process exits
            close(done);

            err = cmd.Wait();
            tΔ2.Logf("[%d] %v", cmd.Process.Pid, err);
            if (err != nil) {
                tΔ2.Errorf("Wait error = %v; want nil", err);
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });

    // If Cancel returns an error and the process is killed after
    // WaitDelay, Wait should report the usual SIGKILL ExitError, not the
    // error from Cancel.
    t.Run(killedAfterErrorˢ, (ж<testing.T> tΔ3) => {
        GoFrame ᒐ = default;
        try {
            tΔ3.Parallel();

            var (ctx, cancel) = context.WithCancel(context.Background());
            var cancelʗ3 = cancel;
            defer(() => cancelʗ3(), ref ᒐ);

            var cmd = helperCommandContext(tΔ3, ctx, pipetestˢ);
            var (stdin, err) = cmd.StdinPipe();
            if (err != nil) {
                tΔ3.Fatal(err);
            }
            var stdinʗ2 = stdin;
            defer(() => stdinʗ2.Close(), ref ᒐ);

            var errArbitrary = errors.New(arbitraryErrorˢ);
            ref var interruptCalled = ref heap(new atomic.Bool(), out var ᏑinterruptCalled);
            var errArbitraryʗ2 = errArbitrary;
            cmd.Cancel = () => {
                tΔ3.Logf("Cancel called");
                ᏑinterruptCalled.Store(true);
                return errArbitraryʗ2;
            };
            cmd.WaitDelay = 1 * time.Millisecond;
            var errΔ1 = cmd.Start(); if (errΔ1 != nil) {
                tΔ3.Fatal(errΔ1);
            }
            cancel();

            err = cmd.Wait();
            tΔ3.Logf("[%d] %v", cmd.Process.Pid, err);

            // Ensure that Cancel actually had the opportunity to
            // return the error.
            if (!ᏑinterruptCalled.Load()) {
                tΔ3.Errorf("Cancel was not called when the context was canceled");
            }

            // This test should kill the child process after 1ms,
            // To maximize compatibility with existing uses of exec.CommandContext, the
            // resulting error should be an exec.ExitError without additional wrapping.
            var (_, ok) = err._<ж<Δexec.ExitError>>(ᐧ); if (!ok) {
                tΔ3.Errorf("Wait error = %v; want *exec.ExitError", err);
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });

    // If Cancel returns ErrProcessDone but the process is not actually done
    // (and has to be killed), Wait should report the usual SIGKILL ExitError,
    // not the error from Cancel.
    t.Run(killedAfterSpuriousˢ, (ж<testing.T> tΔ4) => {
        GoFrame ᒐ = default;
        try {
            tΔ4.Parallel();

            var (ctx, cancel) = context.WithCancel(context.Background());
            var cancelʗ4 = cancel;
            defer(() => cancelʗ4(), ref ᒐ);

            var cmd = helperCommandContext(tΔ4, ctx, pipetestˢ);
            var (stdin, err) = cmd.StdinPipe();
            if (err != nil) {
                tΔ4.Fatal(err);
            }
            var stdinʗ3 = stdin;
            defer(() => stdinʗ3.Close(), ref ᒐ);

            ref var interruptCalled = ref heap(new atomic.Bool(), out var ᏑinterruptCalled);
            cmd.Cancel = () => {
                tΔ4.Logf("Cancel returning an error wrapping ErrProcessDone");
                ᏑinterruptCalled.Store(true);
                return fmt.Errorf("%w: stdout closed"u8, os.ErrProcessDone);
            };
            cmd.WaitDelay = 1 * time.Millisecond;
            var errΔ1 = cmd.Start(); if (errΔ1 != nil) {
                tΔ4.Fatal(errΔ1);
            }
            cancel();

            err = cmd.Wait();
            tΔ4.Logf("[%d] %v", cmd.Process.Pid, err);

            // Ensure that Cancel actually had the opportunity to
            // return the error.
            if (!ᏑinterruptCalled.Load()) {
                tΔ4.Errorf("Cancel was not called when the context was canceled");
            }

            // This test should kill the child process after 1ms,
            // To maximize compatibility with existing uses of exec.CommandContext, the
            // resulting error should be an exec.ExitError without additional wrapping.
            var (ee, ok) = err._<ж<Δexec.ExitError>>(ᐧ); if (!ok) {
                tΔ4.Errorf("Wait error of type %T; want %T", err, ee.OrTypedNil());
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });

    // If Cancel returns an error and the process exits with an
    // unsuccessful exit code, the process error should take precedence over the
    // Cancel error.
    t.Run(nonzeroExitAfterErrorˢ, (ж<testing.T> tΔ5) => {
        GoFrame ᒐ = default;
        try {
            tΔ5.Parallel();

            var (ctx, cancel) = context.WithCancel(context.Background());
            var cancelʗ5 = cancel;
            defer(() => cancelʗ5(), ref ᒐ);

            var cmd = helperCommandContext(tΔ5, ctx, stderrfailˢ);
            var (stderr, err) = cmd.StderrPipe();
            if (err != nil) {
                tΔ5.Fatal(err);
            }

            var errArbitrary = errors.New(arbitraryErrorˢ);
            var interrupted = new channel<EmptyStruct>(0);
            var errArbitraryʗ3 = errArbitrary;
            var interruptedʗ1 = interrupted;
            cmd.Cancel = () => {
                close(interruptedʗ1);
                return errArbitraryʗ3;
            };
            var errΔ1 = cmd.Start(); if (errΔ1 != nil) {
                tΔ5.Fatal(errΔ1);
            }
            cancel();
            ᐸꟷ(interrupted);
            io.Copy(io.Discard, stderr);

            err = cmd.Wait();
            tΔ5.Logf("[%d] %v", cmd.Process.Pid, err);

            var (ee, ok) = err._<ж<Δexec.ExitError>>(ᐧ); if (!ok || ee.ProcessState.ExitCode() != 1) {
                tΔ5.Errorf("Wait error = %v; want exit status 1", err);
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
}

// TestConcurrentExec is a regression test for https://go.dev/issue/61080.
//
// Forking multiple child processes concurrently would sometimes hang on darwin.
// (This test hung on a gomote with -count=100 after only a few iterations.)
public static partial void TestConcurrentExec(ж<testing.T> t) {
    var (ctx, cancel) = context.WithCancel(context.Background());

    // This test will spawn nHangs subprocesses that hang reading from stdin,
    // and nExits subprocesses that exit immediately.
    //
    // When issue #61080 was present, a long-lived "hang" subprocess would
    // occasionally inherit the fork/exec status pipe from an "exit" subprocess,
    // causing the parent process (which expects to see an EOF on that pipe almost
    // immediately) to unexpectedly block on reading from the pipe.
    nint nHangs = runtime.GOMAXPROCS(0);
    nint nExits = runtime.GOMAXPROCS(0);
    ref var hangs = ref heap(new sync.WaitGroup(), out var Ꮡhangs);
    ref var exits = ref heap(new sync.WaitGroup(), out var Ꮡexits);
    Ꮡhangs.Add(nHangs);
    Ꮡexits.Add(nExits);

    // ready is done when the goroutines have done as much work as possible to
    // prepare to create subprocesses. It isn't strictly necessary for the test,
    // but helps to increase the repro rate by making it more likely that calls to
    // syscall.StartProcess for the "hang" and "exit" goroutines overlap.
    ref var ready = ref heap(new sync.WaitGroup(), out var Ꮡready);
    Ꮡready.Add(nHangs + nExits);

    for (nint i = 0; i < nHangs; i++) {
        var ctxʗ1 = ctx;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡhangs.Done, ref ᒐ);

                var cmd = helperCommandContext(t, ctxʗ1, pipetestˢ);
                var (stdin, err) = cmd.StdinPipe();
                if (err != nil) {
                    Ꮡready.Done();
                    t.Error(err);
                    return;
                }
                var stdinʗ1 = stdin;
                                cmd.Cancel = stdinʗ1.Close;
                Ꮡready.Done();

                Ꮡready.Wait();
                var errΔ1 = cmd.Start(); if (errΔ1 != nil) {
                    if (!errors.Is(errΔ1, context.Canceled)) {
                        t.Error(errΔ1);
                    }
                    return;
                }

                cmd.Wait();
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }

    for (nint i = 0; i < nExits; i++) {
        var ctxʗ2 = ctx;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡexits.Done, ref ᒐ);

                var cmd = helperCommandContext(t, ctxʗ2, exitˢ, "0");
                Ꮡready.Done();

                Ꮡready.Wait();
                var err = cmd.Run(); if (err != nil) {
                    t.Error(err);
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }

    Ꮡexits.Wait();
    cancel();
    Ꮡhangs.Wait();
}

// TestPathRace tests that [Cmd.String] can be called concurrently
// with [Cmd.Start].
public static partial void TestPathRace(ж<testing.T> t) {
    var cmd = helperCommand(t, exitˢ, "0");

    var done = new channel<EmptyStruct>(0);
    var cmdʗ1 = cmd;
    var doneʗ1 = done;
    goǃ(() => {
        var (@out, err) = cmdʗ1.CombinedOutput();
        t.Logf("%v: %v\n%s", cmdʗ1.OrTypedNil(), err, @out);
        close(doneʗ1);
    });

    t.Logf("running in background: %v", cmd.OrTypedNil());
    ᐸꟷ(done);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string binGofmtˢ => ref binGofmtˢᵇ;
private static readonly @string binGofmtˢᵇ = "bin/gofmt"u8;
internal static ref readonly @string modifiedˢ => ref modifiedˢᵇ;
private static readonly @string modifiedˢᵇ = "modified"u8;
internal static ref readonly @string binGoˢ => ref binGoˢᵇ;
private static readonly @string binGoˢᵇ = "bin/go"u8;
internal static readonly object testCaseNeedsUpdatingToˢ = (@string)"test case needs updating to verify fix for go.dev/issue/68314"u8;
internal static readonly object ranWrongBinaryˢ = (@string)"ran wrong binary"u8;

public static void TestAbsPathExec(ж<testing.T> t) {
    testenv.MustHaveExec(new exec_test_package.testing_TжTB(t));
    testenv.MustHaveGoBuild(new exec_test_package.testing_TжTB(t)); // must have GOROOT/bin/{go,gofmt}

    // A simple exec of a full path should work.
    // Go 1.22 broke this on Windows, requiring ".exe"; see #66586.
    ref var exe = ref heap<@string>(filepath.Join(testenv.GOROOT(new exec_test_package.testing_TжTB(t)), binGofmtˢ), out var Ꮡexe);
    var cmd = Δexec.Command(exe);
    if (cmd.Path != exe) {
        t.Errorf("exec.Command(%#q) set Path=%#q", exe, cmd.Path);
    }
    var err = cmd.Run();
    if (err != nil) {
        t.Errorf("using exec.Command(%#q): %v", exe, err);
    }

    cmd = Ꮡ(new Δexec.Cmd(Path: exe));
    err = cmd.Run();
    if (err != nil) {
        t.Errorf("using exec.Cmd{Path: %#q}: %v", cmd.Path, err);
    }

    cmd = Ꮡ(new Δexec.Cmd(Path: "gofmt", Dir: "/"));
    err = cmd.Run();
    if (err == nil) {
        t.Errorf("using exec.Cmd{Path: %#q}: unexpected success", cmd.Path);
    }

    // A simple exec after modifying Cmd.Path should work.
    // This broke on Windows. See go.dev/issue/68314.
    t.Run(modifiedˢ, (ж<testing.T> tΔ1) => {
        if (Δexec.Command(filepath.Join(testenv.GOROOT(new exec_test_package.testing_TжTB(tΔ1)), binGoˢ)).Run() == nil) {
            // The implementation of the test case below relies on the go binary
            // exiting with a non-zero exit code when run without any arguments.
            // In the unlikely case that changes, we need to use another binary.
            tΔ1.Fatal(testCaseNeedsUpdatingToˢ);
        }
        @string exe1 = filepath.Join(testenv.GOROOT(new exec_test_package.testing_TжTB(tΔ1)), binGoˢ);
        @string exe2 = filepath.Join(testenv.GOROOT(new exec_test_package.testing_TжTB(tΔ1)), binGofmtˢ);
        var cmdΔ1 = Δexec.Command(exe1);
        cmdΔ1.Path = exe2;
        cmdΔ1.Args = [cmdΔ1.Path];
        var errΔ1 = cmdΔ1.Run();
        if (errΔ1 != nil) {
            tΔ1.Error(ranWrongBinaryˢ);
        }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string binNonesuchˢ => ref binNonesuchˢᵇ;
private static readonly @string binNonesuchˢᵇ = "/bin/nonesuch"u8;
internal static ref readonly @string execAlreadyStartedˢ => ref execAlreadyStartedˢᵇ;
private static readonly @string execAlreadyStartedˢᵇ = "exec: already started"u8;

// Calling Start twice is an error, regardless of outcome.
public static void TestStart_twice(ж<testing.T> t) {
    testenv.MustHaveExec(new exec_test_package.testing_TжTB(t));

    var cmd = Δexec.Command(binNonesuchˢ);
    var errΔ1 = cmd.Start(); if (errΔ1 == nil) {
        t.Fatalf("running invalid command succeeded");
    }
    var err = cmd.Start();
    @string got = fmt.Sprint(err);
    @string want = execAlreadyStartedˢ;
    if (got != want) {
        t.Fatalf("Start call returned err %q, want %q", got, want);
    }
}

} // end exec_test_package
