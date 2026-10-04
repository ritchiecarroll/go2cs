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
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
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
    @string godebug = os.Getenv("GODEBUG"u8);
    if (godebug != ""u8) {
        godebug += ","u8;
    }
    godebug += "execwait=2"u8;
    os.Setenv("GODEBUG"u8, godebug);
    if (os.Getenv("GO_EXEC_TEST_PID"u8) != ""u8) {
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
internal static readonly @string goExecTestPidˢ = "GO_EXEC_TEST_PID"u8;
internal static readonly @string trueˢ = "TRUE"u8;
internal static readonly @string testRunˢ = "test.run"u8;
internal static readonly @string testListˢ = "test.list"u8;

// TestMain allows the test binary to impersonate many other binaries,
// some of which may manipulate os.Stdin, os.Stdout, and/or os.Stderr
// (and thus cannot run as an ordinary Test function, since the testing
// package monkey-patches those variables before running tests).
public static void TestMain(ж<testing.M> Ꮡm) {
    ref var m = ref Ꮡm.DerefOrNull();

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
            os.Setenv("NoDefaultCurrentDirectoryInExePath"u8, trueˢ);
        }
        nint code = Ꮡm.Run();
        if (code == 0 && (~flag.Lookup(testRunˢ)).Value.String() == ""u8 && (~flag.Lookup(testListˢ)).Value.String() == ""u8) {
            foreach (var (cmdΔ1, _) in helperCommands) {
                {
                    var (_, okΔ1) = ᏑhelperCommandUsed.Load(cmdΔ1); if (!okΔ1) {
                        fmt.Fprintf(new os.FileжWriter(os.Stderr), "helper command unused: %q\n"u8, cmdΔ1);
                        code = 1;
                    }
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
    if (helperCommands[name] != default!) {
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
internal static ж<Δexec.Cmd> helperCommand(ж<testing.T> Ꮡt, @string name, params ꓸꓸꓸstring argsʗp) {
    var args = argsʗp.sslice();

    Ꮡt.Helper();
    return helperCommandContext(Ꮡt, default!, name, args.ꓸꓸꓸ);
}

// helperCommandContext is like helperCommand, but also accepts a Context under
// which to run the command.
internal static ж<Δexec.Cmd> /*cmd*/ helperCommandContext(ж<testing.T> Ꮡt, context.Context ctx, @string name, params ꓸꓸꓸstring argsʗp) {
    ж<Δexec.Cmd> cmd = default!;
    var args = argsʗp.sslice();

    ᏑhelperCommandUsed.LoadOrStore(name, true);
    Ꮡt.Helper();
    @string exe = testenv.Executable(new exec_test_package.testing_TжTB(Ꮡt));
    var cs = appendꓸꓸꓸ(new @string[]{name}.slice(), args);
    if (ctx != default!){
        cmd = Δexec.CommandContext(ctx, exe, cs.ꓸꓸꓸ);
    } else {
        cmd = Δexec.Command(exe, cs.ꓸꓸꓸ);
    }
    return cmd;
}

internal static ж<sync.Map> ᏑhelperCommandUsed = new StandardBox<sync.Map>(default(sync.Map));
internal static ref sync.Map helperCommandUsed => ref ᏑhelperCommandUsed.Value;

internal static map<@string, Actionꓸꓸꓸ<@string>> helperCommands;
internal static void initᴛhelperCommands() { helperCommands = new map<@string, Actionꓸꓸꓸ<@string>>{
    ["echo"u8] = cmdEcho,
    ["echoenv"u8] = cmdEchoEnv,
    ["cat"u8] = cmdCat,
    ["pipetest"u8] = cmdPipeTest,
    ["stdinClose"u8] = cmdStdinClose,
    ["exit"u8] = cmdExit,
    ["describefiles"u8] = cmdDescribeFiles,
    ["stderrfail"u8] = cmdStderrFail,
    ["yes"u8] = cmdYes,
    ["hang"u8] = cmdHang
}; }

internal static void cmdEcho(params ꓸꓸꓸstring argsʗp) {
    var args = argsʗp.sslice();

    var iargs = new any[]{}.slice();
    foreach (var (_, s) in args) {
        iargs = append(iargs, (any)(s));
    }
    fmt.Println(iargs.ꓸꓸꓸ);
}

internal static void cmdEchoEnv(params ꓸꓸꓸstring argsʗp) {
    var args = argsʗp.sslice();

    foreach (var (_, s) in args) {
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
        foreach (var (_, fn) in args) {
            var (f, err) = os.Open(fn);
            if (err != default!){
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
    while (ᐧ) {
        var (line, _, err) = bufr.ReadLine();
        if (AreEqual(err, io.EOF)){
            break;
        } else 
        if (err != default!) {
            os.Exit(1);
        }
        if (bytes.HasPrefix(line, slice<byte>("O:"u8))){
            os.Stdout.Write(line);
            os.Stdout.Write(new byte[]{(rune)'\n'}.slice());
        } else 
        if (bytes.HasPrefix(line, slice<byte>("E:"u8))){
            os.Stderr.Write(line);
            os.Stderr.Write(new byte[]{(rune)'\n'}.slice());
        } else {
            os.Exit(1);
        }
    }
}

internal static void cmdStdinClose(params ꓸꓸꓸstring ʗp) {
    var (b, err) = io.ReadAll(new exec_test_package.os_FileжReader(os.Stdin));
    if (err != default!) {
        fmt.Fprintf(new os.FileжWriter(os.Stderr), "Error: %v\n"u8, err);
        os.Exit(1);
    }
    {
        @string s = ((@string)b); if (s != stdinCloseTestString) {
            fmt.Fprintf(new os.FileжWriter(os.Stderr), "Error: Read %q, want %q"u8, s, stdinCloseTestString);
            os.Exit(1);
        }
    }
}

internal static void cmdExit(params ꓸꓸꓸstring argsʗp) {
    var args = argsʗp.sslice();

    var (n, _) = strconv.Atoi(args[0]);
    os.Exit(n);
}

internal static void cmdDescribeFiles(params ꓸꓸꓸstring argsʗp) {
    var args = argsʗp.sslice();

    var f = os.NewFile(3, "fd3"u8);
    var (ln, err) = net.FileListener(f);
    if (err == default!) {
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
        args = new @string[]{"y"u8}.slice();
    }
    @string s = strings.Join(args, " "u8) + "\n"u8;
    while (ᐧ) {
        var (_, err) = os.Stdout.WriteString(s);
        if (err != default!) {
            os.Exit(1);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string echoˢ = "echo"u8;
internal static readonly @string fooBarˢ = "foo bar"u8;
internal static readonly @string bazˢ = "baz"u8;
internal static readonly @string fooBarBazˢ = "foo bar baz\n"u8;

public static void TestEcho(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
    var (bs, err) = helperCommand(Ꮡt, echoˢ, fooBarˢ, bazˢ).Output();
    if (err != default!) {
        Ꮡt.Errorf("echo: %v"u8, err);
    }
    {
        @string g = ((@string)bs);
        @string e = fooBarBazˢ; if (g != e) {
            Ꮡt.Errorf("echo: want %q, got %q"u8, e, g);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fooˢ = "foo"u8;
internal static readonly object skippingRunningTestAtˢ = (@string)"skipping; running test at root somehow"u8;
internal static readonly @string fooˢ2 = "foo\n"u8;

public static void TestCommandRelativeName(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
    var cmd = helperCommand(Ꮡt, echoˢ, fooˢ);
    // Run our own binary as a relative path
    // (e.g. "_test/exec.test") our parent directory.
    @string @base = filepath.Base(os.Args[0]); // "exec.test"
    @string dir = filepath.Dir(os.Args[0]); // "/tmp/go-buildNNNN/os/exec/_test"
    if (dir == "."u8) {
        Ꮡt.Skip(skippingRunningTestAtˢ);
    }
    @string parentDir = filepath.Dir(dir); // "/tmp/go-buildNNNN/os/exec"
    @string dirBase = filepath.Base(dir); // "_test"
    if (dirBase == "."u8) {
        Ꮡt.Skipf("skipping; unexpected shallow dir of %q"u8, dir);
    }
    cmd.Value.Path = filepath.Join(dirBase, @base);
    cmd.Value.Dir = parentDir;
    var (@out, err) = cmd.Output();
    if (err != default!) {
        Ꮡt.Errorf("echo: %v"u8, err);
    }
    {
        @string g = ((@string)@out);
        @string e = fooˢ2; if (g != e) {
            Ꮡt.Errorf("echo: want %q, got %q"u8, e, g);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string inputStringLine2ˢ = "Input string\nLine 2"u8;
internal static readonly @string catˢ = "cat"u8;

public static void TestCatStdin(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
    // Cat, testing stdin and stdout.
    @string input = inputStringLine2ˢ;
    var p = helperCommand(Ꮡt, catˢ);
    p.Value.Stdin = new exec_test_package.strings_ReaderжReader(strings.NewReader(input));
    var (bs, err) = p.Output();
    if (err != default!) {
        Ꮡt.Errorf("cat: %v"u8, err);
    }
    @string s = ((@string)bs);
    if (s != input) {
        Ꮡt.Errorf("cat: want %q, got %q"u8, input, s);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object echoˢ2 = (@string)"echo\n"u8;

[MethodImpl(MethodImplOptions.NoInlining)] public static void TestEchoFileRace(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
    var cmd = helperCommand(Ꮡt, echoˢ);
    var (stdin, err) = cmd.StdinPipe();
    if (err != default!) {
        Ꮡt.Fatalf("StdinPipe: %v"u8, err);
    }
    {
        var errΔ1 = cmd.Start(); if (errΔ1 != default!) {
            Ꮡt.Fatalf("Start: %v"u8, errΔ1);
        }
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
    {
        var errΔ2 = cmd.Wait(); if (errΔ2 != default!) {
            Ꮡt.Fatalf("Wait: %v"u8, errΔ2);
        }
    }
    ᐸꟷ(wrote);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string bogusFileFooˢ = "/bogus/file.foo"u8;
internal static readonly @string execTestGoˢ = "exec_test.go"u8;
internal static readonly @string errorOpenBogusFileFooˢ = "Error: open /bogus/file.foo"u8;
internal static readonly @string funcˢ = "func TestCatGoodAndBadFile(t *testing.T)"u8;

public static void TestCatGoodAndBadFile(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
    // Testing combined output and error values.
    var (bs, err) = helperCommand(Ꮡt, catˢ, bogusFileFooˢ, execTestGoˢ).CombinedOutput();
    {
        var (_, okΔ1) = err._<ж<Δexec.ExitError>>(ᐧ); if (!okΔ1) {
            Ꮡt.Errorf("expected *exec.ExitError from cat combined; got %T: %v"u8, err, err);
        }
    }
    var (errLine, body, ok) = strings.Cut(((@string)bs), "\n"u8);
    if (!ok) {
        Ꮡt.Fatalf("expected two lines from cat; got %q"u8, bs);
    }
    if (!strings.HasPrefix(errLine, errorOpenBogusFileFooˢ)) {
        Ꮡt.Errorf("expected stderr to complain about file; got %q"u8, errLine);
    }
    if (!strings.Contains(body, funcˢ)) {
        Ꮡt.Errorf("expected test code; got %q (len %d)"u8, body, len(body));
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string noExistExecutableˢ = "/no-exist-executable"u8;
internal static readonly object expectedErrorFromNoExistˢ = (@string)"expected error from /no-exist-executable"u8;

public static void TestNoExistExecutable(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    Ꮡt.Parallel();
    // Can't run a non-existent executable
    var err = Δexec.Command(noExistExecutableˢ).Run();
    if (err == default!) {
        Ꮡt.Error(expectedErrorFromNoExistˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string exitˢ = "exit"u8;
internal static readonly @string exitStatus42ˢ = "exit status 42"u8;

public static void TestExitStatus(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
    // Test that exit values are returned correctly
    var cmd = helperCommand(Ꮡt, exitˢ, "42"u8);
    var err = cmd.Run();
    @string want = exitStatus42ˢ;
    var exprᴛ1 = runtime.GOOS;
    if (exprᴛ1 == "plan9"u8) {
        want = fmt.Sprintf("exit status: '%s %d: 42'"u8, filepath.Base((~cmd).Path), (~cmd).ProcessState.Pid());
    }

    {
        var (werr, ok) = err._<ж<Δexec.ExitError>>(ᐧ); if (ok){
            {
                @string s = werr.Error(); if (s != want) {
                    Ꮡt.Errorf("from exit 42 got exit %q, want %q"u8, s, want);
                }
            }
        } else {
            Ꮡt.Fatalf("expected *exec.ExitError from exit 42; got %T: %v"u8, err, err);
        }
    }
}

public static void TestExitCode(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
    // Test that exit code are returned correctly
    var cmd = helperCommand(Ꮡt, exitˢ, "42"u8);
    cmd.Run();
    nint want = 42;
    if (runtime.GOOS == "plan9"u8) {
        want = 1;
    }
    nint got = (~cmd).ProcessState.ExitCode();
    if (want != got) {
        Ꮡt.Errorf("ExitCode got %d, want %d"u8, got, want);
    }
    cmd = helperCommand(Ꮡt, noExistExecutableˢ);
    cmd.Run();
    want = 2;
    if (runtime.GOOS == "plan9"u8) {
        want = 1;
    }
    got = (~cmd).ProcessState.ExitCode();
    if (want != got) {
        Ꮡt.Errorf("ExitCode got %d, want %d"u8, got, want);
    }
    cmd = helperCommand(Ꮡt, exitˢ, "255"u8);
    cmd.Run();
    want = 255;
    if (runtime.GOOS == "plan9"u8) {
        want = 1;
    }
    got = (~cmd).ProcessState.ExitCode();
    if (want != got) {
        Ꮡt.Errorf("ExitCode got %d, want %d"u8, got, want);
    }
    cmd = helperCommand(Ꮡt, catˢ);
    cmd.Run();
    want = 0;
    got = (~cmd).ProcessState.ExitCode();
    if (want != got) {
        Ꮡt.Errorf("ExitCode got %d, want %d"u8, got, want);
    }
    // Test when command does not call Run().
    cmd = helperCommand(Ꮡt, catˢ);
    want = -1;
    got = (~cmd).ProcessState.ExitCode();
    if (want != got) {
        Ꮡt.Errorf("ExitCode got %d, want %d"u8, got, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string pipetestˢ = "pipetest"u8;
internal static readonly @string stdinPipeˢ = "StdinPipe"u8;
internal static readonly @string stdoutPipeˢ = "StdoutPipe"u8;
internal static readonly @string stderrPipeˢ = "StderrPipe"u8;
internal static readonly @string startˢ = "Start"u8;
internal static readonly @string firstStdinWriteˢ = "first stdin Write"u8;
internal static readonly @string firstOutputLineˢ = "first output line"u8;
internal static readonly @string oIAmOutputˢ = "O:I am output"u8;
internal static readonly @string secondStdinWriteˢ = "second stdin Write"u8;
internal static readonly @string firstErrorLineˢ = "first error line"u8;
internal static readonly @string eIAmErrorˢ = "E:I am error"u8;
internal static readonly @string thirdStdinWrite3ˢ = "third stdin Write 3"u8;
internal static readonly @string secondOutputLineˢ = "second output line"u8;
internal static readonly @string oIAmOutput2ˢ = "O:I am output2"u8;
internal static readonly @string waitˢ = "Wait"u8;

public static void TestPipes(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
    void check(@string what, error errΔ1) {
        if (errΔ1 != default!) {
            Ꮡt.Fatalf("%s: %v"u8, what, errΔ1);
        }
    }
    // Cat, testing stdin and stdout.
    var c = helperCommand(Ꮡt, pipetestˢ);
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
        if (errΔ2 != default!) {
            Ꮡt.Fatalf("%s: %v"u8, what, errΔ2);
        }
        return ((@string)lineΔ1);
    }
    err = c.Start();
    check(startˢ, err);
    (_, err) = stdin.Write(slice<byte>("O:I am output\n"u8));
    check(firstStdinWriteˢ, err);
    {
        @string g = line(firstOutputLineˢ, outbr);
        @string e = oIAmOutputˢ; if (g != e) {
            Ꮡt.Errorf("got %q, want %q"u8, g, e);
        }
    }
    (_, err) = stdin.Write(slice<byte>("E:I am error\n"u8));
    check(secondStdinWriteˢ, err);
    {
        @string g = line(firstErrorLineˢ, errbr);
        @string e = eIAmErrorˢ; if (g != e) {
            Ꮡt.Errorf("got %q, want %q"u8, g, e);
        }
    }
    (_, err) = stdin.Write(slice<byte>("O:I am output2\n"u8));
    check(thirdStdinWrite3ˢ, err);
    {
        @string g = line(secondOutputLineˢ, outbr);
        @string e = oIAmOutput2ˢ; if (g != e) {
            Ꮡt.Errorf("got %q, want %q"u8, g, e);
        }
    }
    stdin.Close();
    err = c.Wait();
    check(waitˢ, err);
}

internal static readonly @string stdinCloseTestString = "Some test string."u8;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string stdinCloseˢ = "stdinClose"u8;
internal static readonly object canTAccessMethodsOfˢ = (@string)"can't access methods of underlying *os.File"u8;
internal static readonly @string copyˢ = "Copy"u8;

[GoType("dyn")] internal partial interface TestStdinClose_type {
    uintptr Fd();
}

// Issue 6270.
[MethodImpl(MethodImplOptions.NoInlining)] public static void TestStdinClose(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        Ꮡt.Parallel();
        void check(@string what, error errΔ1) {
            if (errΔ1 != default!) {
                Ꮡt.Fatalf("%s: %v"u8, what, errΔ1);
            }
        }
        var cmd = helperCommand(Ꮡt, stdinCloseˢ);
        var (stdin, err) = cmd.StdinPipe();
        check(stdinPipeˢ, err);
        // Check that we can access methods of the underlying os.File.`
        {
            var (_, ok) = stdin._<TestStdinClose_type>(ᐧ); if (!ok) {
                Ꮡt.Error(canTAccessMethodsOfˢ);
            }
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
                {
                    var errΔ3 = stdinʗ1.Close(); if (errΔ3 != default! && !errors.Is(errΔ3, os.ErrClosed)) {
                        Ꮡt.Errorf("Close: %v"u8, errΔ3);
                    }
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
internal static readonly @string unexpectedStringˢ = "unexpected string"u8;

// Issue 17647.
// It used to be the case that TestStdinClose, above, would fail when
// run under the race detector. This test is a variant of TestStdinClose
// that also used to fail when run under the race detector.
// This test is run by cmd/dist under the race detector to verify that
// the race detector no longer reports any problems.
[MethodImpl(MethodImplOptions.NoInlining)] public static void TestStdinCloseRace(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        Ꮡt.Parallel();
        var cmd = helperCommand(Ꮡt, stdinCloseˢ);
        var (stdin, err) = cmd.StdinPipe();
        if (err != default!) {
            Ꮡt.Fatalf("StdinPipe: %v"u8, err);
        }
        {
            var errΔ1 = cmd.Start(); if (errΔ1 != default!) {
                Ꮡt.Fatalf("Start: %v"u8, errΔ1);
            }
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
                (~cmdʗ1).Process.Kill();
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
                {
                    var errΔ2 = stdinʗ1.Close(); if (errΔ2 != default! && !errors.Is(errΔ2, os.ErrClosed)) {
                        Ꮡt.Errorf("stdin.Close: %v"u8, errΔ2);
                    }
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
        {
            var errΔ3 = cmd.Wait(); if (errΔ3 == default!) {
                Ꮡt.Fatalf("Wait: succeeded unexpectedly"u8);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object weDonTCurrentlySupporeˢ = (@string)"we don't currently suppore counting open handles on windows"u8;
internal static readonly @string somethingThatDoesNotˢ = "something-that-does-not-exist-executable"u8;
internal static readonly object unexpectedSuccessˢ = (@string)"unexpected success"u8;

// Issue 5071
public static void TestPipeLookPathLeak(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (runtime.GOOS == "windows"u8) {
        Ꮡt.Skip(weDonTCurrentlySupporeˢ);
    }
    // Not parallel: checks for leaked file descriptors
    slice<uintptr> openFDs() {
        slice<uintptr> fds = default!;
        for (var i = (uintptr)0; i < 100; i++) {
            if (fdtest.Exists(i)) {
                fds = append(fds, i);
            }
        }
        return fds;
    }
    var old = new map<uintptr, bool>{};
    foreach (var (_, fd) in openFDs()) {
        old[fd] = true;
    }
    for (nint i = 0; i < 6; i++) {
        var cmd = Δexec.Command(somethingThatDoesNotˢ);
        cmd.StdoutPipe();
        cmd.StderrPipe();
        cmd.StdinPipe();
        {
            var err = cmd.Run(); if (err == default!) {
                Ꮡt.Fatal(unexpectedSuccessˢ);
            }
        }
    }
    // Since this test is not running in parallel, we don't expect any new file
    // descriptors to be opened while it runs. However, if there are additional
    // FDs present at the start of the test (for example, opened by libc), those
    // may be closed due to a timeout of some sort. Allow those to go away, but
    // check that no new FDs are added.
    foreach (var (_, fd) in openFDs()) {
        if (!old[fd]) {
            Ꮡt.Errorf("leaked file descriptor %v"u8, fd);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingTestBecauseTestˢ = (@string)"skipping test because test was run with FDs open"u8;
internal static readonly @string tcpˢ = "tcp"u8;
internal static readonly @string read3Exeˢ = "read3.exe"u8;
internal static readonly @string buildˢ = "build"u8;
internal static readonly @string read3Goˢ = "read3.go"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string textᶜ = "Hello, fd 3!"u8;

public static void TestExtraFiles(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        if (testing.Short()) {
            Ꮡt.Skipf("skipping test in short mode that would build a helper binary"u8);
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
        testenv.MustInternalLink(new exec_test_package.testing_TжTB(Ꮡt), false);
        if (runtime.GOOS == "windows"u8) {
            Ꮡt.Skipf("skipping test on %q"u8, runtime.GOOS);
        }
        // Force network usage, to verify the epoll (or whatever) fd
        // doesn't leak to the child,
        var (ln, err) = net.Listen(tcpˢ, "127.0.0.1:0"u8);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);
        // Make sure duplicated fds don't leak to the child.
        (var f, err) = ln._<ж<net.TCPListener>>().File();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        (var ln2, err) = net.FileListener(f);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var ln2ʗ1 = ln2;
        defer(() => ln2ʗ1.Close(), ref ᒐ);
        // Force TLS root certs to be loaded (which might involve
        // cgo), to make sure none of that potential C code leaks fds.
        var ts = httptest.NewUnstartedServer(new exec_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
        })));
        // quiet expected TLS handshake error "remote error: bad certificate"
        ts.Value.Config.Value.ErrorLog = log.New(io.Discard, ""u8, 0);
        ts.StartTLS();
        var tsʗ1 = ts;
        defer(tsʗ1.Close, ref ᒐ);
        (_, err) = http.Get((~ts).URL);
        if (err == default!) {
            Ꮡt.Errorf("success trying to fetch %s; want an error"u8, (~ts).URL);
        }
        (var tf, err) = os.CreateTemp(""u8, ""u8);
        if (err != default!) {
            Ꮡt.Fatalf("TempFile: %v"u8, err);
        }
        defer(os.Remove, tf.Name(), ref ᒐ);
        var tfʗ1 = tf;
        defer(() => tfʗ1.Close(), ref ᒐ);
        @string text = textᶜ;
        (_, err) = tf.Write(slice<byte>(text));
        if (err != default!) {
            Ꮡt.Fatalf("Write: %v"u8, err);
        }
        (_, err) = tf.Seek(0, io.SeekStart);
        if (err != default!) {
            Ꮡt.Fatalf("Seek: %v"u8, err);
        }
        @string tempdir = Ꮡt.TempDir();
        @string exe = filepath.Join(tempdir, read3Exeˢ);
        var c = testenv.Command(new exec_test_package.testing_TжTB(Ꮡt), testenv.GoToolPath(new exec_test_package.testing_TжTB(Ꮡt)), buildˢ, "-o", exe, read3Goˢ);
        // Build the test without cgo, so that C library functions don't
        // open descriptors unexpectedly. See issue 25628.
        c.Value.Env = append(os.Environ(), "CGO_ENABLED=0"u8);
        {
            var (output, errΔ1) = c.CombinedOutput(); if (errΔ1 != default!) {
                Ꮡt.Logf("go build -o %s read3.go\n%s"u8, exe, output);
                Ꮡt.Fatalf("go build failed: %v"u8, errΔ1);
            }
        }
        // Use a deadline to try to get some output even if the program hangs.
        var ctx = context.Background();
        {
            var (deadline, ok) = t.Deadline(); if (ok) {
                // Leave a 20% grace period to flush output, which may be large on the
                // linux/386 builders because we're running the subprocess under strace.
                deadline = deadline.Add(-time.Until(deadline) / 5);
                Action cancel = default!;
                (ctx, cancel) = context.WithDeadline(ctx, deadline);
                var cancelʗ1 = cancel;
                defer(() => cancelʗ1(), ref ᒐ);
            }
        }
        c = Δexec.CommandContext(ctx, exe);
        ref var stdout = ref heap(new strings.Builder(), out var Ꮡstdout);
        ref var stderr = ref heap(new strings.Builder(), out var Ꮡstderr);
        c.Value.Stdout = new exec_test_package.strings_BuilderжWriter(Ꮡstdout);
        c.Value.Stderr = new exec_test_package.strings_BuilderжWriter(Ꮡstderr);
        c.Value.ExtraFiles = new ж<os.File>[]{tf}.slice();
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
            c.Value.Env = append(os.Environ(), "GOMAXPROCS=1"u8);
        }
        err = c.Run();
        if (err != default!) {
            Ꮡt.Fatalf("Run: %v\n--- stdout:\n%s--- stderr:\n%s"u8, err, stdout.String(), stderr.String());
        }
        if (stdout.String() != text) {
            Ꮡt.Errorf("got stdout %q, stderr %q; want %q on stdout"u8, stdout.String(), stderr.String(), text);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string describefilesˢ = "describefiles"u8;
internal static readonly object noOperatingSystemSupportˢ = (@string)"no operating system support; skipping"u8;

[MethodImpl(MethodImplOptions.NoInlining)] public static void TestExtraFilesRace(ж<testing.T> Ꮡt) {
    if (runtime.GOOS == "windows"u8) {
        maySkipHelperCommand(describefilesˢ);
        Ꮡt.Skip(noOperatingSystemSupportˢ);
    }
    Ꮡt.Parallel();
    net.Listener listen() {
        var (ln, err) = net.Listen(tcpˢ, "127.0.0.1:0"u8);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        return ln;
    }
    ж<os.File> listenerFile(net.Listener ln) {
        var (f, err) = ln._<ж<net.TCPListener>>().File();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        return f;
    }
    void runCommand(ж<Δexec.Cmd> c, channel/*<-*/<@string> @out) {
        var (bout, err) = c.CombinedOutput();
        if (err != default!){
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
        var ca = helperCommand(Ꮡt, describefilesˢ);
        ca.Value.ExtraFiles = new ж<os.File>[]{listenerFile(la)}.slice();
        var lb = listen();
        var cb = helperCommand(Ꮡt, describefilesˢ);
        cb.Value.ExtraFiles = new ж<os.File>[]{listenerFile(lb)}.slice();
        var ares = new channel<@string>(0);
        var bres = new channel<@string>(0);
        var runCommandʗ1 = runCommand;
        goǃ(runCommandʗ1, ca, ares.WithDirection(GoChanDir.Send));
        var runCommandʗ2 = runCommand;
        goǃ(runCommandʗ2, cb, bres.WithDirection(GoChanDir.Send));
        {
            @string got = ᐸꟷ(ares);
            @string want = fmt.Sprintf("fd3: listener %s\n"u8, la.Addr()); if (got != want) {
                Ꮡt.Errorf("iteration %d, process A got:\n%s\nwant:\n%s\n"u8, i, got, want);
            }
        }
        {
            @string got = ᐸꟷ(bres);
            @string want = fmt.Sprintf("fd3: listener %s\n"u8, lb.Addr()); if (got != want) {
                Ꮡt.Errorf("iteration %d, process B got:\n%s\nwant:\n%s\n"u8, i, got, want);
            }
        }
        la.Close();
        lb.Close();
        foreach (var (_, f) in (~ca).ExtraFiles) {
            f.Close();
        }
        foreach (var (_, f) in (~cb).ExtraFiles) {
            f.Close();
        }
    }
}

[GoType] partial struct delayedInfiniteReader {
}

internal static (nint, error) Read(this delayedInfiniteReader _, slice<byte> b) {
    time.Sleep(100 * time.Millisecond);
    foreach (var (i, _) in b) {
        b[i] = (rune)'x';
    }
    return (len(b), default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string infiniteˢ = "Infinite"u8;

// Issue 9173: ignore stdin pipe writes if the program completes successfully.
public static void TestIgnorePipeErrorOnSuccess(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
    Action<ж<testing.T>> testWith(io.Reader r) => (ж<testing.T> tΔ1) => {
            tΔ1.Parallel();
            var cmd = helperCommand(tΔ1, echoˢ, fooˢ);
            ref var @out = ref heap(new strings.Builder(), out var Ꮡout);
            cmd.Value.Stdin = r;
            cmd.Value.Stdout = new exec_test_package.strings_BuilderжWriter(Ꮡout);
            {
                var err = cmd.Run(); if (err != default!) {
                    tΔ1.Fatal(err);
                }
            }
            {
                @string got = @out.String();
                @string want = fooˢ2; if (got != want) {
                    tΔ1.Errorf("output = %q; want %q"u8, got, want);
                }
            }
        };
    Ꮡt.Run("10MB"u8, testWith(new exec_test_package.strings_ReaderжReader(strings.NewReader(strings.Repeat("x"u8, (10 << (int)(20)))))));
    Ꮡt.Run(infiniteˢ, testWith(new delayedInfiniteReader(nil)));
}

[GoType] partial struct badWriter {
}

[GoRecv] internal static (nint, error) Write(this ref badWriter w, slice<byte> data) {
    return (0, io.ErrUnexpectedEOF);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string yesˢ = "yes"u8;

public static void TestClosePipeOnCopyError(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
    var cmd = helperCommand(Ꮡt, yesˢ);
    cmd.Value.Stdout = new exec_test_package.badWriterжWriter(@new<badWriter>());
    var err = cmd.Run();
    if (err == default!) {
        Ꮡt.Errorf("yes unexpectedly completed successfully"u8);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string stderrfailˢ = "stderrfail"u8;
internal static readonly @string someStderrTextˢ = "some stderr text\n"u8;

public static void TestOutputStderrCapture(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
    var cmd = helperCommand(Ꮡt, stderrfailˢ);
    var (_, err) = cmd.Output();
    var (ee, ok) = err._<ж<Δexec.ExitError>>(ᐧ);
    if (!ok) {
        Ꮡt.Fatalf("Output error type = %T; want ExitError"u8, err);
    }
    @string got = ((@string)(~ee).Stderr);
    @string want = someStderrTextˢ;
    if (got != want) {
        Ꮡt.Errorf("ExitError.Stderr = %q; want %q"u8, got, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedWaitFailureˢ = (@string)"expected Wait failure"u8;

[MethodImpl(MethodImplOptions.NoInlining)] public static void TestContext(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
    var (ctx, cancel) = context.WithCancel(context.Background());
    var c = helperCommandContext(Ꮡt, ctx, pipetestˢ);
    var (stdin, err) = c.StdinPipe();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (var stdout, err) = c.StdoutPipe();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    {
        var errΔ1 = c.Start(); if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
    }
    {
        var (_, errΔ2) = stdin.Write(slice<byte>("O:hi\n"u8)); if (errΔ2 != default!) {
            Ꮡt.Fatal(errΔ2);
        }
    }
    var buf = new slice<byte>(5);
    (var n, err) = io.ReadFull(stdout, buf);
    if (n != len(buf) || err != default! || ((sstring)buf) != "O:hi\n"u8) {
        Ꮡt.Fatalf("ReadFull = %d, %v, %q"u8, n, err, buf.slice(0, n));
    }
    var cancelʗ1 = cancel;
    goǃ(() => cancelʗ1());
    {
        var errΔ3 = c.Wait(); if (errΔ3 == default!) {
            Ꮡt.Fatal(expectedWaitFailureˢ);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string systemˢ = "system"u8;
internal static readonly object programUnexpectedlyˢ = (@string)"program unexpectedly exited successfully"u8;

public static void TestContextCancel(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        if (runtime.GOOS == "netbsd"u8 && runtime.GOARCH == "arm64"u8) {
            maySkipHelperCommand(catˢ);
            testenv.SkipFlaky(new exec_test_package.testing_TжTB(Ꮡt), 42061);
        }
        // To reduce noise in the final goroutine dump,
        // let other parallel tests complete if possible.
        Ꮡt.Parallel();
        var (ctx, cancel) = context.WithCancel(context.Background());
        var cancelʗ1 = cancel;
        defer(() => cancelʗ1(), ref ᒐ);
        var c = helperCommandContext(Ꮡt, ctx, catˢ);
        var (stdin, err) = c.StdinPipe();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var stdinʗ1 = stdin;
        defer(() => stdinʗ1.Close(), ref ᒐ);
        {
            var errΔ1 = c.Start(); if (errΔ1 != default!) {
                Ꮡt.Fatal(errΔ1);
            }
        }
        // At this point the process is alive. Ensure it by sending data to stdin.
        {
            var (_, errΔ2) = io.WriteString(stdin, echoˢ); if (errΔ2 != default!) {
                Ꮡt.Fatal(errΔ2);
            }
        }
        cancel();
        // Calling cancel should have killed the process, so writes
        // should now fail.  Give the process a little while to die.
        var start = time.Now();
        var delay = 1 * time.Millisecond;
        while (ᐧ) {
            {
                var (_, errΔ3) = io.WriteString(stdin, echoˢ); if (errΔ3 != default!) {
                    break;
                }
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
        {
            var errΔ4 = c.Wait(); if (errΔ4 == default!){
                Ꮡt.Error(programUnexpectedlyˢ);
            } else {
                Ꮡt.Logf("exit status: %v"u8, errΔ4);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string echoenvˢ = "echoenv"u8;
internal static readonly @string fooˢ3 = "FOO"u8;
internal static readonly @string goodˢ = "good"u8;

// test that environment variables are de-duped.
public static void TestDedupEnvEcho(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
    var cmd = helperCommand(Ꮡt, echoenvˢ, fooˢ3);
    cmd.Value.Env = append(cmd.Environ(), "FOO=bad"u8, "FOO=good");
    var (@out, err) = cmd.CombinedOutput();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    {
        @string got = strings.TrimSpace(((@string)@out));
        @string want = goodˢ; if (got != want) {
            Ꮡt.Errorf("output = %q; want %q"u8, got, want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object plan9ExplicitlyAllowsNulˢ = (@string)"plan9 explicitly allows NUL in the environment"u8;
internal static readonly @string barˢ = "BAR"u8;

public static void TestEnvNULCharacter(ж<testing.T> Ꮡt) {
    if (runtime.GOOS == "plan9"u8) {
        Ꮡt.Skip(plan9ExplicitlyAllowsNulˢ);
    }
    var cmd = helperCommand(Ꮡt, echoenvˢ, fooˢ3, barˢ);
    cmd.Value.Env = append(cmd.Environ(), ((@string)(new byte[]{0x46, 0x4f, 0x4f, 0x3d, 0x66, 0x6f, 0x6f, 0x00, 0x42, 0x41, 0x52, 0x3d, 0x62, 0x61, 0x72})));
    var (@out, err) = cmd.CombinedOutput();
    if (err == default!) {
        Ꮡt.Errorf("output = %q; want error"u8, ((@string)@out));
    }
}

[GoType("dyn")] internal partial struct TestString_tests {
    internal @string path;
    internal slice<@string> args;
    internal @string want;
}

public static void TestString(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
    var (echoPath, err) = Δexec.LookPath(echoˢ);
    if (err != default!) {
        Ꮡt.Skip(err);
    }
    var tests = new TestString_tests[]{
        new("echo"u8, default!, echoPath),
        new("echo"u8, new @string[]{"a"u8}.slice(), echoPath + " a"u8),
        new("echo"u8, new @string[]{"a"u8, "b"u8}.slice(), echoPath + " a b"u8)
    }.array();
    foreach (var (_, test) in tests.ΔRangeSnapshot()) {
        var cmd = Δexec.Command(test.path, test.args.ꓸꓸꓸ);
        {
            @string got = cmd.String(); if (got != test.want) {
                Ꮡt.Errorf("String(%q, %q) = %q, want %q"u8, test.path, test.args, got, test.want);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string makemeasandwichˢ = "makemeasandwich"u8;
internal static readonly object wowThanksˢ = (@string)"wow, thanks"u8;
internal static readonly @string lettuceˢ = "-lettuce"u8;
internal static readonly @string makemeasandwichLettuceˢ = "makemeasandwich -lettuce"u8;

public static void TestStringPathNotResolved(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
    var (_, err) = Δexec.LookPath(makemeasandwichˢ);
    if (err == default!) {
        Ꮡt.Skip(wowThanksˢ);
    }
    var cmd = Δexec.Command(makemeasandwichˢ, lettuceˢ);
    @string want = makemeasandwichLettuceˢ;
    {
        @string got = cmd.String(); if (got != want) {
            Ꮡt.Errorf("String(%q, %q) = %q, want %q"u8, makemeasandwichˢ, lettuceˢ, got, want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string execNoCommandˢ = "exec: no command"u8;

public static void TestNoPath(ж<testing.T> Ꮡt) {
    var err = @new<Δexec.Cmd>().Start();
    @string want = execNoCommandˢ;
    if (err == default! || err.Error() != want) {
        Ꮡt.Errorf("new(Cmd).Start() = %v, want %q"u8, err, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string alreadyStartedˢ = "already started"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string msgᶜ = "O:Hello, pipe!\n"u8;

// TestDoubleStartLeavesPipesOpen checks for a regression in which calling
// Start twice, which returns an error on the second call, would spuriously
// close the pipes established in the first call.
[MethodImpl(MethodImplOptions.NoInlining)] public static void TestDoubleStartLeavesPipesOpen(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
    var cmd = helperCommand(Ꮡt, pipetestˢ);
    var (@in, err) = cmd.StdinPipe();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (var @out, err) = cmd.StdoutPipe();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    {
        var errΔ1 = cmd.Start(); if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
    }
    var cmdʗ1 = cmd;
    Ꮡt.Cleanup(() => {
        {
            var errΔ2 = cmdʗ1.Wait(); if (errΔ2 != default!) {
                Ꮡt.Error(errΔ2);
            }
        }
    });
    {
        var errΔ3 = cmd.Start(); if (errΔ3 == default! || !strings.HasSuffix(errΔ3.Error(), alreadyStartedˢ)) {
            Ꮡt.Fatalf("second call to Start returned a nil; want an 'already started' error"u8);
        }
    }
    var outc = new channel<slice<byte>>(1);
    var outʗ1 = @out;
    var outcʗ1 = outc;
    goǃ(() => {
        var (bΔ1, errΔ4) = io.ReadAll(outʗ1);
        if (errΔ4 != default!) {
            Ꮡt.Error(errΔ4);
        }
        outcʗ1.ᐸꟷ(bΔ1);
    });
    @string msg = msgᶜ;
    (_, err) = io.WriteString(@in, msg);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    @in.Close();
    var b = ᐸꟷ(outc);
    if (!bytes.Equal(b, slice<byte>(msg))) {
        Ꮡt.Fatalf("read %q from stdout pipe; want %q"u8, b, msg);
    }
}

[MethodImpl(MethodImplOptions.NoInlining)] internal static void cmdHang(params ꓸꓸꓸstring argsʗp) {
    var args = argsʗp.slice();

    var (sleep, err) = time.ParseDuration(args[0]);
    if (err != default!) {
        throw panic(err);
    }
    var fs = flag.NewFlagSet("hang"u8, flag.ExitOnError);
    var exitOnInterrupt = fs.Bool("interrupt"u8, false, "if true, commands should exit 0 on os.Interrupt"u8);
    var subsleep = fs.Duration("subsleep"u8, 0, "amount of time for the 'hang' helper to leave an orphaned subprocess sleeping with stderr open"u8);
    var probe = fs.Duration("probe"u8, 0, "if nonzero, the 'hang' helper should write to stderr at this interval, and exit nonzero if a write fails"u8);
    var read = fs.Bool("read"u8, false, "if true, the 'hang' helper should read stdin to completion before sleeping"u8);
    fs.Parse(args[1..]);
    nint pid = os.Getpid();
    if (subsleep.Value != 0) {
        var cmd = Δexec.Command(testenv.Executable(default!), "hang"u8, (~subsleep).String(), "-read=true", "-probe=" + (~probe).String());
        cmd.Value.Stdin = new exec_test_package.os_FileжReader(os.Stdin);
        cmd.Value.Stderr = new os.FileжWriter(os.Stderr);
        var (@out, errΔ1) = cmd.StdoutPipe();
        if (errΔ1 != default!) {
            fmt.Fprintln(new os.FileжWriter(os.Stderr), errΔ1);
            os.Exit(1);
        }
        cmd.Start();
        var buf = @new<strings.Builder>();
        {
            var (_, errΔ2) = io.Copy(new exec_test_package.strings_BuilderжWriter(buf), @out); if (errΔ2 != default!) {
                fmt.Fprintln(new os.FileжWriter(os.Stderr), errΔ2);
                (~cmd).Process.Kill();
                cmd.Wait();
                os.Exit(1);
            }
        }
        fmt.Fprintf(new os.FileжWriter(os.Stderr), "%d: started %d: %v\n"u8, pid, (~(~cmd).Process).Pid, cmd.OrTypedNil());
        var cmdʗ1 = cmd;
        goǃ(() => cmdʗ1.Wait()); // Release resources if cmd happens not to outlive this process.
    }
    if (exitOnInterrupt.Value){
        var c = new channel<osꓸSignal>(1);
        signal.Notify(c.WithDirection(GoChanDir.Send), os.Interrupt);
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
        if (pipeSignal != default!) {
            signal.Ignore(pipeSignal);
        }
        var r = bufio.NewReader(new exec_test_package.os_FileжReader(os.Stdin));
        while (ᐧ) {
            var (line, errΔ3) = r.ReadBytes((rune)'\n');
            if (len(line) > 0) {
                // Ignore write errors: we want to keep reading even if stderr is closed.
                fmt.Fprintf(new os.FileжWriter(os.Stderr), "%d: read %s"u8, pid, line);
            }
            if (errΔ3 != default!) {
                fmt.Fprintf(new os.FileжWriter(os.Stderr), "%d: finished read: %v"u8, pid, errΔ3);
                break;
            }
        }
    }
    if (probe.Value != 0) {
        var ticker = time.NewTicker(probe.Value);
        var tickerʗ1 = ticker;
        goǃ(() => {
            foreach (var _ᴛ1 in (~tickerʗ1).C) {
                {
                    var (_, errΔ4) = fmt.Fprintf(new os.FileжWriter(os.Stderr), "%d: ok\n"u8, pid); if (errΔ4 != default!) {
                        os.Exit(1);
                    }
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
[GoType] partial struct tickReader {
    internal time.Duration interval;
    internal time.Time lastTick;
    internal @string s;
}

internal static ж<tickReader> newTickReader(time.Duration interval) {
    return Ꮡ(new tickReader(interval: interval));
}

[GoRecv] internal static (nint n, error err) Read(this ref tickReader r, slice<byte> p) {
    nint n = default!;

    if (len(r.s) == 0) {
        {
            var d = r.interval - time.Since(r.lastTick); if (d > 0) {
                time.Sleep(d);
            }
        }
        r.lastTick = time.Now();
        r.s = r.lastTick.Format(time.RFC3339Nano + "\n");
    }
    n = copy(p, r.s);
    r.s = r.s.slice(n);
    return (n, default!);
}

internal static ж<Δexec.Cmd> startHang(ж<testing.T> Ꮡt, context.Context ctx, time.Duration hangTime, osꓸSignal interrupt, time.Duration waitDelay, params ꓸꓸꓸstring flagsʗp) {
    var flags = flagsʗp.sslice();

    Ꮡt.Helper();
    var args = appendꓸꓸꓸ(new @string[]{hangTime.String()}.slice(), flags);
    var cmd = helperCommandContext(Ꮡt, ctx, "hang"u8, args.ꓸꓸꓸ);
    cmd.Value.Stdin = new exec_test_package.tickReaderжReader(newTickReader(1 * time.Millisecond));
    cmd.Value.Stderr = new exec_test_package.strings_BuilderжWriter(@new<strings.Builder>());
    if (interrupt == default!){
        cmd.Value.Cancel = default!;
    } else {
        var cmdʗ1 = cmd;
        cmd.Value.Cancel = () => (~cmdʗ1).Process.Signal(interrupt);
    }
    cmd.Value.WaitDelay = waitDelay;
    var (@out, err) = cmd.StdoutPipe();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    Ꮡt.Log(cmd.OrTypedNil());
    {
        var errΔ1 = cmd.Start(); if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
    }
    // Wait for cmd to close stdout to signal that its handlers are installed.
    var buf = @new<strings.Builder>();
    {
        var (_, errΔ2) = io.Copy(new exec_test_package.strings_BuilderжWriter(buf), @out); if (errΔ2 != default!) {
            Ꮡt.Error(errΔ2);
            (~cmd).Process.Kill();
            cmd.Wait();
            Ꮡt.FailNow();
        }
    }
    if (buf.Len() > 0) {
        Ꮡt.Logf("stdout %v:\n%s"u8, (~cmd).Args, buf.OrTypedNil());
    }
    return cmd;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string waitDelayˢ = "WaitDelay"u8;
internal static readonly @string interruptTrueˢ = "-interrupt=true"u8;
internal static readonly @string sigkillHangˢ = "SIGKILL-hang"u8;
internal static readonly @string subsleep10mˢ = "-subsleep=10m"u8;
internal static readonly @string probe1msˢ = "-probe=1ms"u8;
internal static readonly @string exitHangˢ = "Exit-hang"u8;
internal static readonly @string sigintIgnoredˢ = "SIGINT-ignored"u8;
internal static readonly @string interruptFalseˢ = "-interrupt=false"u8;
internal static readonly @string sigintHandledˢ = "SIGINT-handled"u8;
internal static readonly @string sigquitˢ = "SIGQUIT"u8;
internal static readonly @string goroutineˢ = "\n\ngoroutine "u8;

public static void TestWaitInterrupt(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    Ꮡt.Parallel();
    // tooLong is an arbitrary duration that is expected to be much longer than
    // the test runs, but short enough that leaked processes will eventually exit
    // on their own.
    time.Duration tooLong = /* 10 * time.Minute */ 600000000000;
    // Control case: with no cancellation and no WaitDelay, we should wait for the
    // process to exit.
    Ꮡt.Run(waitˢ, (ж<testing.T> tΔ1) => {
        tΔ1.Parallel();
        var cmd = startHang(tΔ1, context.Background(), 1 * time.Millisecond, os.ΔKill, 0);
        var err = cmd.Wait();
        tΔ1.Logf("stderr:\n%s"u8, (~cmd).Stderr);
        tΔ1.Logf("[%d] %v"u8, (~(~cmd).Process).Pid, err);
        if (err != default!) {
            tΔ1.Errorf("Wait: %v; want <nil>"u8, err);
        }
        {
            var ps = cmd.Value.ProcessState; if (!ps.Exited()){
                tΔ1.Errorf("cmd did not exit: %v"u8, ps.OrTypedNil());
            } else 
            {
                nint code = ps.ExitCode(); if (code != 0) {
                    tΔ1.Errorf("cmd.ProcessState.ExitCode() = %v; want 0"u8, code);
                }
            }
        }
    });
    // With a very long WaitDelay and no Cancel function, we should wait for the
    // process to exit even if the command's Context is canceled.
    Ꮡt.Run(waitDelayˢ, (ж<testing.T> tΔ2) => {
        if (runtime.GOOS == "windows"u8) {
            tΔ2.Skipf("skipping: os.Interrupt is not implemented on Windows"u8);
        }
        tΔ2.Parallel();
        var (ctx, cancel) = context.WithCancel(context.Background());
        var cmd = startHang(tΔ2, ctx, tooLong, default!, tooLong, interruptTrueˢ);
        cancel();
        time.Sleep(1 * time.Millisecond);
        // At this point cmd should still be running (because we passed nil to
        // startHang for the cancel signal). Sending it an explicit Interrupt signal
        // should succeed.
        {
            var errΔ1 = (~cmd).Process.Signal(os.Interrupt); if (errΔ1 != default!) {
                tΔ2.Error(errΔ1);
            }
        }
        var err = cmd.Wait();
        tΔ2.Logf("stderr:\n%s"u8, (~cmd).Stderr);
        tΔ2.Logf("[%d] %v"u8, (~(~cmd).Process).Pid, err);
        // This program exits with status 0,
        // but pretty much always does so during the wait delay.
        // Since the Cmd itself didn't do anything to stop the process when the
        // context expired, a successful exit is valid (even if late) and does
        // not merit a non-nil error.
        if (err != default!) {
            tΔ2.Errorf("Wait: %v; want nil"u8, err);
        }
        {
            var ps = cmd.Value.ProcessState; if (!ps.Exited()){
                tΔ2.Errorf("cmd did not exit: %v"u8, ps.OrTypedNil());
            } else 
            {
                nint code = ps.ExitCode(); if (code != 0) {
                    tΔ2.Errorf("cmd.ProcessState.ExitCode() = %v; want 0"u8, code);
                }
            }
        }
    });
    // If the context is canceled and the Cancel function sends os.Kill,
    // the process should be terminated immediately, and its output
    // pipes should be closed (causing Wait to return) after WaitDelay
    // even if a child process is still writing to them.
    Ꮡt.Run(sigkillHangˢ, (ж<testing.T> tΔ3) => {
        tΔ3.Parallel();
        var (ctx, cancel) = context.WithCancel(context.Background());
        var cmd = startHang(tΔ3, ctx, tooLong, os.ΔKill, 10 * time.Millisecond, subsleep10mˢ, probe1msˢ);
        cancel();
        var err = cmd.Wait();
        tΔ3.Logf("stderr:\n%s"u8, (~cmd).Stderr);
        tΔ3.Logf("[%d] %v"u8, (~(~cmd).Process).Pid, err);
        // This test should kill the child process after 10ms,
        // leaving a grandchild process writing probes in a loop.
        // The child process should be reported as failed,
        // and the grandchild will exit (or die by SIGPIPE) once the
        // stderr pipe is closed.
        {
            var ee = @new<ж<Δexec.ExitError>>(); if (!errors.As(err, ee.OrTypedNil())) {
                tΔ3.Errorf("Wait error = %v; want %T"u8, err, ee.ValueSlot.OrTypedNil());
            }
        }
    });
    // If the process exits with status 0 but leaves a child behind writing
    // to its output pipes, Wait should only wait for WaitDelay before
    // closing the pipes and returning.  Wait should return ErrWaitDelay
    // to indicate that the piped output may be incomplete even though the
    // command returned a “success” code.
    Ꮡt.Run(exitHangˢ, (ж<testing.T> tΔ4) => {
        tΔ4.Parallel();
        var cmd = startHang(tΔ4, context.Background(), 1 * time.Millisecond, default!, 10 * time.Millisecond, subsleep10mˢ, probe1msˢ);
        var err = cmd.Wait();
        tΔ4.Logf("stderr:\n%s"u8, (~cmd).Stderr);
        tΔ4.Logf("[%d] %v"u8, (~(~cmd).Process).Pid, err);
        // This child process should exit immediately,
        // leaving a grandchild process writing probes in a loop.
        // Since the child has no ExitError to report but we did not
        // read all of its output, Wait should return ErrWaitDelay.
        if (!errors.Is(err, Δexec.ErrWaitDelay)) {
            tΔ4.Errorf("Wait error = %v; want %T"u8, err, Δexec.ErrWaitDelay);
        }
    });
    // If the Cancel function sends a signal that the process can handle, and it
    // handles that signal without actually exiting, then it should be terminated
    // after the WaitDelay.
    Ꮡt.Run(sigintIgnoredˢ, (ж<testing.T> tΔ5) => {
        if (runtime.GOOS == "windows"u8) {
            tΔ5.Skipf("skipping: os.Interrupt is not implemented on Windows"u8);
        }
        tΔ5.Parallel();
        var (ctx, cancel) = context.WithCancel(context.Background());
        var cmd = startHang(tΔ5, ctx, tooLong, os.Interrupt, 10 * time.Millisecond, interruptFalseˢ);
        cancel();
        var err = cmd.Wait();
        tΔ5.Logf("stderr:\n%s"u8, (~cmd).Stderr);
        tΔ5.Logf("[%d] %v"u8, (~(~cmd).Process).Pid, err);
        // This command ignores SIGINT, sleeping until it is killed.
        // Wait should return the usual error for a killed process.
        {
            var ee = @new<ж<Δexec.ExitError>>(); if (!errors.As(err, ee.OrTypedNil())) {
                tΔ5.Errorf("Wait error = %v; want %T"u8, err, ee.ValueSlot.OrTypedNil());
            }
        }
    });
    // If the process handles the cancellation signal and exits with status 0,
    // Wait should report a non-nil error (because the process had to be
    // interrupted), and it should be a context error (because there is no error
    // to report from the child process itself).
    Ꮡt.Run(sigintHandledˢ, (ж<testing.T> tΔ6) => {
        if (runtime.GOOS == "windows"u8) {
            tΔ6.Skipf("skipping: os.Interrupt is not implemented on Windows"u8);
        }
        tΔ6.Parallel();
        var (ctx, cancel) = context.WithCancel(context.Background());
        var cmd = startHang(tΔ6, ctx, tooLong, os.Interrupt, 0, interruptTrueˢ);
        cancel();
        var err = cmd.Wait();
        tΔ6.Logf("stderr:\n%s"u8, (~cmd).Stderr);
        tΔ6.Logf("[%d] %v"u8, (~(~cmd).Process).Pid, err);
        if (!errors.Is(err, ctx.Err())) {
            tΔ6.Errorf("Wait error = %v; want %v"u8, err, ctx.Err());
        }
        {
            var ps = cmd.Value.ProcessState; if (!ps.Exited()){
                tΔ6.Errorf("cmd did not exit: %v"u8, ps.OrTypedNil());
            } else 
            {
                nint code = ps.ExitCode(); if (code != 0) {
                    tΔ6.Errorf("cmd.ProcessState.ExitCode() = %v; want 0"u8, code);
                }
            }
        }
    });
    // If the Cancel function sends SIGQUIT, it should be handled in the usual
    // way: a Go program should dump its goroutines and exit with non-success
    // status. (We expect SIGQUIT to be a common pattern in real-world use.)
    Ꮡt.Run(sigquitˢ, (ж<testing.T> tΔ7) => {
        if (quitSignal == default!) {
            tΔ7.Skipf("skipping: SIGQUIT is not supported on %v"u8, runtime.GOOS);
        }
        tΔ7.Parallel();
        var (ctx, cancel) = context.WithCancel(context.Background());
        var cmd = startHang(tΔ7, ctx, tooLong, quitSignal, 0);
        cancel();
        var err = cmd.Wait();
        tΔ7.Logf("stderr:\n%s"u8, (~cmd).Stderr);
        tΔ7.Logf("[%d] %v"u8, (~(~cmd).Process).Pid, err);
        {
            var ee = @new<ж<Δexec.ExitError>>(); if (!errors.As(err, ee.OrTypedNil())) {
                tΔ7.Errorf("Wait error = %v; want %v"u8, err, ctx.Err());
            }
        }
        {
            var ps = cmd.Value.ProcessState; if (!ps.Exited()){
                tΔ7.Errorf("cmd did not exit: %v"u8, ps.OrTypedNil());
            } else 
            {
                nint code = ps.ExitCode(); if (code != 2) {
                    // The default os/signal handler exits with code 2.
                    tΔ7.Errorf("cmd.ProcessState.ExitCode() = %v; want 2"u8, code);
                }
            }
        }
        if (!strings.Contains(fmt.Sprint((~cmd).Stderr), goroutineˢ)) {
            tΔ7.Errorf("cmd.Stderr does not contain a goroutine dump"u8);
        }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string successAfterErrorˢ = "success after error"u8;
internal static readonly @string arbitraryErrorˢ = "arbitrary error"u8;
internal static readonly @string successAfterˢ = "success after ErrProcessDone"u8;
internal static readonly @string killedAfterErrorˢ = "killed after error"u8;
internal static readonly @string killedAfterSpuriousˢ = "killed after spurious ErrProcessDone"u8;
internal static readonly @string nonzeroExitAfterErrorˢ = "nonzero exit after error"u8;

public static void TestCancelErrors(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    Ꮡt.Parallel();
    // If Cancel returns a non-ErrProcessDone error and the process
    // exits successfully, Wait should wrap the error from Cancel.
    Ꮡt.Run(successAfterErrorˢ, (ж<testing.T> tΔ1) => {
        GoFrame ᒐ = default;
        try {
            tΔ1.Parallel();
            var (ctx, cancel) = context.WithCancel(context.Background());
            var cancelʗ1 = cancel;
            defer(() => cancelʗ1(), ref ᒐ);
            var cmd = helperCommandContext(tΔ1, ctx, pipetestˢ);
            var (stdin, err) = cmd.StdinPipe();
            if (err != default!) {
                tΔ1.Fatal(err);
            }
            var errArbitrary = errors.New(arbitraryErrorˢ);
            var errArbitraryʗ1 = errArbitrary;
            var stdinʗ1 = stdin;
            cmd.Value.Cancel = () => {
                stdinʗ1.Close();
                tΔ1.Logf("Cancel returning %v"u8, errArbitraryʗ1);
                return errArbitraryʗ1;
            };
            {
                var errΔ1 = cmd.Start(); if (errΔ1 != default!) {
                    tΔ1.Fatal(errΔ1);
                }
            }
            cancel();
            err = cmd.Wait();
            tΔ1.Logf("[%d] %v"u8, (~(~cmd).Process).Pid, err);
            if (!errors.Is(err, errArbitrary) || AreEqual(err, errArbitrary)) {
                tΔ1.Errorf("Wait error = %v; want an error wrapping %v"u8, err, errArbitrary);
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
    // If Cancel returns an error equivalent to ErrProcessDone,
    // Wait should ignore that error. (ErrProcessDone indicates that the
    // process was already done before we tried to interrupt it — maybe we
    // just didn't notice because Wait hadn't been called yet.)
    Ꮡt.Run(successAfterˢ, (ж<testing.T> tΔ2) => {
        GoFrame ᒐ = default;
        try {
            tΔ2.Parallel();
            var (ctx, cancel) = context.WithCancel(context.Background());
            var cancelʗ2 = cancel;
            defer(() => cancelʗ2(), ref ᒐ);
            var cmd = helperCommandContext(tΔ2, ctx, pipetestˢ);
            var (stdin, err) = cmd.StdinPipe();
            if (err != default!) {
                tΔ2.Fatal(err);
            }
            (var stdout, err) = cmd.StdoutPipe();
            if (err != default!) {
                tΔ2.Fatal(err);
            }
            // We intentionally race Cancel against the process exiting,
            // but ensure that the process wins the race (and return ErrProcessDone
            // from Cancel to report that).
            var interruptCalled = new channel<EmptyStruct>(0);
            var done = new channel<EmptyStruct>(0);
            var doneʗ1 = done;
            var interruptCalledʗ1 = interruptCalled;
            cmd.Value.Cancel = () => {
                close(interruptCalledʗ1);
                ᐸꟷ(doneʗ1);
                tΔ2.Logf("Cancel returning an error wrapping ErrProcessDone"u8);
                return fmt.Errorf("%w: stdout closed"u8, os.ErrProcessDone);
            };
            {
                var errΔ1 = cmd.Start(); if (errΔ1 != default!) {
                    tΔ2.Fatal(errΔ1);
                }
            }
            cancel();
            ᐸꟷ(interruptCalled);
            stdin.Close();
            io.Copy(io.Discard, stdout); // reaches EOF when the process exits
            close(done);
            err = cmd.Wait();
            tΔ2.Logf("[%d] %v"u8, (~(~cmd).Process).Pid, err);
            if (err != default!) {
                tΔ2.Errorf("Wait error = %v; want nil"u8, err);
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
    // If Cancel returns an error and the process is killed after
    // WaitDelay, Wait should report the usual SIGKILL ExitError, not the
    // error from Cancel.
    Ꮡt.Run(killedAfterErrorˢ, (ж<testing.T> tΔ3) => {
        GoFrame ᒐ = default;
        try {
            tΔ3.Parallel();
            var (ctx, cancel) = context.WithCancel(context.Background());
            var cancelʗ3 = cancel;
            defer(() => cancelʗ3(), ref ᒐ);
            var cmd = helperCommandContext(tΔ3, ctx, pipetestˢ);
            var (stdin, err) = cmd.StdinPipe();
            if (err != default!) {
                tΔ3.Fatal(err);
            }
            var stdinʗ2 = stdin;
            defer(() => stdinʗ2.Close(), ref ᒐ);
            var errArbitrary = errors.New(arbitraryErrorˢ);
            ref var interruptCalled = ref heap(new atomic.Bool(), out var ᏑinterruptCalled);
            var errArbitraryʗ2 = errArbitrary;
            cmd.Value.Cancel = () => {
                tΔ3.Logf("Cancel called"u8);
                ᏑinterruptCalled.Store(true);
                return errArbitraryʗ2;
            };
            cmd.Value.WaitDelay = 1 * time.Millisecond;
            {
                var errΔ1 = cmd.Start(); if (errΔ1 != default!) {
                    tΔ3.Fatal(errΔ1);
                }
            }
            cancel();
            err = cmd.Wait();
            tΔ3.Logf("[%d] %v"u8, (~(~cmd).Process).Pid, err);
            // Ensure that Cancel actually had the opportunity to
            // return the error.
            if (!ᏑinterruptCalled.Load()) {
                tΔ3.Errorf("Cancel was not called when the context was canceled"u8);
            }
            // This test should kill the child process after 1ms,
            // To maximize compatibility with existing uses of exec.CommandContext, the
            // resulting error should be an exec.ExitError without additional wrapping.
            {
                var (_, ok) = err._<ж<Δexec.ExitError>>(ᐧ); if (!ok) {
                    tΔ3.Errorf("Wait error = %v; want *exec.ExitError"u8, err);
                }
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
    // If Cancel returns ErrProcessDone but the process is not actually done
    // (and has to be killed), Wait should report the usual SIGKILL ExitError,
    // not the error from Cancel.
    Ꮡt.Run(killedAfterSpuriousˢ, (ж<testing.T> tΔ4) => {
        GoFrame ᒐ = default;
        try {
            tΔ4.Parallel();
            var (ctx, cancel) = context.WithCancel(context.Background());
            var cancelʗ4 = cancel;
            defer(() => cancelʗ4(), ref ᒐ);
            var cmd = helperCommandContext(tΔ4, ctx, pipetestˢ);
            var (stdin, err) = cmd.StdinPipe();
            if (err != default!) {
                tΔ4.Fatal(err);
            }
            var stdinʗ3 = stdin;
            defer(() => stdinʗ3.Close(), ref ᒐ);
            ref var interruptCalled = ref heap(new atomic.Bool(), out var ᏑinterruptCalled);
            cmd.Value.Cancel = () => {
                tΔ4.Logf("Cancel returning an error wrapping ErrProcessDone"u8);
                ᏑinterruptCalled.Store(true);
                return fmt.Errorf("%w: stdout closed"u8, os.ErrProcessDone);
            };
            cmd.Value.WaitDelay = 1 * time.Millisecond;
            {
                var errΔ1 = cmd.Start(); if (errΔ1 != default!) {
                    tΔ4.Fatal(errΔ1);
                }
            }
            cancel();
            err = cmd.Wait();
            tΔ4.Logf("[%d] %v"u8, (~(~cmd).Process).Pid, err);
            // Ensure that Cancel actually had the opportunity to
            // return the error.
            if (!ᏑinterruptCalled.Load()) {
                tΔ4.Errorf("Cancel was not called when the context was canceled"u8);
            }
            // This test should kill the child process after 1ms,
            // To maximize compatibility with existing uses of exec.CommandContext, the
            // resulting error should be an exec.ExitError without additional wrapping.
            {
                var (ee, ok) = err._<ж<Δexec.ExitError>>(ᐧ); if (!ok) {
                    tΔ4.Errorf("Wait error of type %T; want %T"u8, err, ee.OrTypedNil());
                }
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
    // If Cancel returns an error and the process exits with an
    // unsuccessful exit code, the process error should take precedence over the
    // Cancel error.
    Ꮡt.Run(nonzeroExitAfterErrorˢ, (ж<testing.T> tΔ5) => {
        GoFrame ᒐ = default;
        try {
            tΔ5.Parallel();
            var (ctx, cancel) = context.WithCancel(context.Background());
            var cancelʗ5 = cancel;
            defer(() => cancelʗ5(), ref ᒐ);
            var cmd = helperCommandContext(tΔ5, ctx, stderrfailˢ);
            var (stderr, err) = cmd.StderrPipe();
            if (err != default!) {
                tΔ5.Fatal(err);
            }
            var errArbitrary = errors.New(arbitraryErrorˢ);
            var interrupted = new channel<EmptyStruct>(0);
            var errArbitraryʗ3 = errArbitrary;
            var interruptedʗ1 = interrupted;
            cmd.Value.Cancel = () => {
                close(interruptedʗ1);
                return errArbitraryʗ3;
            };
            {
                var errΔ1 = cmd.Start(); if (errΔ1 != default!) {
                    tΔ5.Fatal(errΔ1);
                }
            }
            cancel();
            ᐸꟷ(interrupted);
            io.Copy(io.Discard, stderr);
            err = cmd.Wait();
            tΔ5.Logf("[%d] %v"u8, (~(~cmd).Process).Pid, err);
            {
                var (ee, ok) = err._<ж<Δexec.ExitError>>(ᐧ); if (!ok || (~ee).ProcessState.ExitCode() != 1) {
                    tΔ5.Errorf("Wait error = %v; want exit status 1"u8, err);
                }
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
[MethodImpl(MethodImplOptions.NoInlining)] public static void TestConcurrentExec(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

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
                var cmd = helperCommandContext(Ꮡt, ctxʗ1, pipetestˢ);
                var (stdin, err) = cmd.StdinPipe();
                if (err != default!) {
                    Ꮡready.Done();
                    Ꮡt.Error(err);
                    return;
                }
                var stdinʗ1 = stdin;
                                cmd.Value.Cancel = stdinʗ1.Close;
                Ꮡready.Done();
                Ꮡready.Wait();
                {
                    var errΔ1 = cmd.Start(); if (errΔ1 != default!) {
                        if (!errors.Is(errΔ1, context.Canceled)) {
                            Ꮡt.Error(errΔ1);
                        }
                        return;
                    }
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
                var cmd = helperCommandContext(Ꮡt, ctxʗ2, exitˢ, "0"u8);
                Ꮡready.Done();
                Ꮡready.Wait();
                {
                    var err = cmd.Run(); if (err != default!) {
                        Ꮡt.Error(err);
                    }
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
[MethodImpl(MethodImplOptions.NoInlining)] public static void TestPathRace(ж<testing.T> Ꮡt) {
    var cmd = helperCommand(Ꮡt, exitˢ, "0"u8);
    var done = new channel<EmptyStruct>(0);
    var cmdʗ1 = cmd;
    var doneʗ1 = done;
    goǃ(() => {
        var (@out, err) = cmdʗ1.CombinedOutput();
        Ꮡt.Logf("%v: %v\n%s"u8, cmdʗ1.OrTypedNil(), err, @out);
        close(doneʗ1);
    });
    Ꮡt.Logf("running in background: %v"u8, cmd.OrTypedNil());
    ᐸꟷ(done);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string binGofmtˢ = "bin/gofmt"u8;
internal static readonly @string modifiedˢ = "modified"u8;
internal static readonly @string binGoˢ = "bin/go"u8;
internal static readonly object testCaseNeedsUpdatingToˢ = (@string)"test case needs updating to verify fix for go.dev/issue/68314"u8;
internal static readonly object ranWrongBinaryˢ = (@string)"ran wrong binary"u8;

public static void TestAbsPathExec(ж<testing.T> Ꮡt) {
    testenv.MustHaveExec(new exec_test_package.testing_TжTB(Ꮡt));
    testenv.MustHaveGoBuild(new exec_test_package.testing_TжTB(Ꮡt)); // must have GOROOT/bin/{go,gofmt}
    // A simple exec of a full path should work.
    // Go 1.22 broke this on Windows, requiring ".exe"; see #66586.
    ref var exe = ref heap<@string>(out var Ꮡexe);
    exe = filepath.Join(testenv.GOROOT(new exec_test_package.testing_TжTB(Ꮡt)), binGofmtˢ);
    var cmd = Δexec.Command(exe);
    if ((~cmd).Path != exe) {
        Ꮡt.Errorf("exec.Command(%#q) set Path=%#q"u8, exe, (~cmd).Path);
    }
    var err = cmd.Run();
    if (err != default!) {
        Ꮡt.Errorf("using exec.Command(%#q): %v"u8, exe, err);
    }
    cmd = Ꮡ(new Δexec.Cmd(Path: exe));
    err = cmd.Run();
    if (err != default!) {
        Ꮡt.Errorf("using exec.Cmd{Path: %#q}: %v"u8, (~cmd).Path, err);
    }
    cmd = Ꮡ(new Δexec.Cmd(Path: "gofmt"u8, Dir: "/"u8));
    err = cmd.Run();
    if (err == default!) {
        Ꮡt.Errorf("using exec.Cmd{Path: %#q}: unexpected success"u8, (~cmd).Path);
    }
    // A simple exec after modifying Cmd.Path should work.
    // This broke on Windows. See go.dev/issue/68314.
    Ꮡt.Run(modifiedˢ, (ж<testing.T> tΔ1) => {
        if (Δexec.Command(filepath.Join(testenv.GOROOT(new exec_test_package.testing_TжTB(tΔ1)), binGoˢ)).Run() == default!) {
            // The implementation of the test case below relies on the go binary
            // exiting with a non-zero exit code when run without any arguments.
            // In the unlikely case that changes, we need to use another binary.
            tΔ1.Fatal(testCaseNeedsUpdatingToˢ);
        }
        @string exe1 = filepath.Join(testenv.GOROOT(new exec_test_package.testing_TжTB(tΔ1)), binGoˢ);
        @string exe2 = filepath.Join(testenv.GOROOT(new exec_test_package.testing_TжTB(tΔ1)), binGofmtˢ);
        var cmdΔ1 = Δexec.Command(exe1);
        cmdΔ1.Value.Path = exe2;
        cmdΔ1.Value.Args = new @string[]{(~cmdΔ1).Path}.slice();
        var errΔ1 = cmdΔ1.Run();
        if (errΔ1 != default!) {
            tΔ1.Error(ranWrongBinaryˢ);
        }
    });
}

} // end exec_test_package
