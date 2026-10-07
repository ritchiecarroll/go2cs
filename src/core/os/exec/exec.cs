// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package exec runs external commands. It wraps os.StartProcess to make it
// easier to remap stdin and stdout, connect I/O with pipes, and do other
// adjustments.
//
// Unlike the "system" library call from C and other languages, the
// os/exec package intentionally does not invoke the system shell and
// does not expand any glob patterns or handle other expansions,
// pipelines, or redirections typically done by shells. The package
// behaves more like C's "exec" family of functions. To expand glob
// patterns, either call the shell directly, taking care to escape any
// dangerous input, or use the [path/filepath] package's Glob function.
// To expand environment variables, use package os's ExpandEnv.
//
// Note that the examples in this package assume a Unix system.
// They may not run on Windows, and they do not run in the Go Playground
// used by golang.org and godoc.org.
//
// # Executables in the current directory
//
// The functions [Command] and [LookPath] look for a program
// in the directories listed in the current path, following the
// conventions of the host operating system.
// Operating systems have for decades included the current
// directory in this search, sometimes implicitly and sometimes
// configured explicitly that way by default.
// Modern practice is that including the current directory
// is usually unexpected and often leads to security problems.
//
// To avoid those security problems, as of Go 1.19, this package will not resolve a program
// using an implicit or explicit path entry relative to the current directory.
// That is, if you run [LookPath]("go"), it will not successfully return
// ./go on Unix nor .\go.exe on Windows, no matter how the path is configured.
// Instead, if the usual path algorithms would result in that answer,
// these functions return an error err satisfying [errors.Is](err, [ErrDot]).
//
// For example, consider these two program snippets:
//
//	path, err := exec.LookPath("prog")
//	if err != nil {
//		log.Fatal(err)
//	}
//	use(path)
//
// and
//
//	cmd := exec.Command("prog")
//	if err := cmd.Run(); err != nil {
//		log.Fatal(err)
//	}
//
// These will not find and run ./prog or .\prog.exe,
// no matter how the current path is configured.
//
// Code that always wants to run a program from the current directory
// can be rewritten to say "./prog" instead of "prog".
//
// Code that insists on including results from relative path entries
// can instead override the error using an errors.Is check:
//
//	path, err := exec.LookPath("prog")
//	if errors.Is(err, exec.ErrDot) {
//		err = nil
//	}
//	if err != nil {
//		log.Fatal(err)
//	}
//	use(path)
//
// and
//
//	cmd := exec.Command("prog")
//	if errors.Is(cmd.Err, exec.ErrDot) {
//		cmd.Err = nil
//	}
//	if err := cmd.Run(); err != nil {
//		log.Fatal(err)
//	}
//
// Setting the environment variable GODEBUG=execerrdot=0
// disables generation of ErrDot entirely, temporarily restoring the pre-Go 1.19
// behavior for programs that are unable to apply more targeted fixes.
// A future version of Go may remove support for this variable.
//
// Before adding such overrides, make sure you understand the
// security implications of doing so.
// See https://go.dev/blog/path-security for more information.
namespace go.os;

using bytes = bytes_package;
using context = context_package;
using errors = errors_package;
using godebug = @internal.godebug_package;
using execenv = @internal.syscall.execenv_package;
using io = io_package;
using os = os_package;
using filepath = path.filepath_package;
using runtime = runtime_package;
using strconv = strconv_package;
using strings = strings_package;
using syscall = syscall_package;
using time = time_package;
using @internal;
using @internal.syscall;
using System.Runtime.CompilerServices;
using fs = go.io.fs_package;
using path;
using ꓸꓸꓸstring = Span<@string>;

partial class exec_package {

// Error is returned by [LookPath] when it fails to classify a file as an
// executable.
[GoType] partial struct ΔError {
    // Name is the file name for which the error occurred.
    public @string Name;
    // Err is the underlying error.
    public error Err;
}

[GoRecv] public static @string Error(this ref ΔError e) {
    return "exec: "u8 + strconv.Quote(e.Name) + ": "u8 + e.Err.Error();
}

[GoRecv] public static error Unwrap(this ref ΔError e) {
    return e.Err;
}

// ErrWaitDelay is returned by [Cmd.Wait] if the process exits with a
// successful status code but its output pipes are not closed before the
// command's WaitDelay expires.
public static error ErrWaitDelay = errors.New("exec: WaitDelay expired before I/O complete"u8);

// wrappedError wraps an error without relying on fmt.Errorf.
[GoType] partial struct wrappedError {
    internal @string prefix;
    internal error err;
}

internal static @string Error(this wrappedError w) {
    return w.prefix + ": "u8 + w.err.Error();
}

internal static error Unwrap(this wrappedError w) {
    return w.err;
}

[GoType("dyn")] partial struct Cmd_cachedLookExtensions {
    internal @string @in, @out;
}

// Cmd represents an external command being prepared or run.
//
// A Cmd cannot be reused after calling its [Cmd.Run], [Cmd.Output] or [Cmd.CombinedOutput]
// methods.
[GoType] partial struct Cmd {
    // Path is the path of the command to run.
    //
    // This is the only field that must be set to a non-zero
    // value. If Path is relative, it is evaluated relative
    // to Dir.
    public @string Path;
    // Args holds command line arguments, including the command as Args[0].
    // If the Args field is empty or nil, Run uses {Path}.
    //
    // In typical use, both Path and Args are set by calling Command.
    public slice<@string> Args;
    // Env specifies the environment of the process.
    // Each entry is of the form "key=value".
    // If Env is nil, the new process uses the current process's
    // environment.
    // If Env contains duplicate environment keys, only the last
    // value in the slice for each duplicate key is used.
    // As a special case on Windows, SYSTEMROOT is always added if
    // missing and not explicitly set to the empty string.
    //
    // See also the Dir field, which may set PWD in the environment.
    public slice<@string> Env;
    // Dir specifies the working directory of the command.
    // If Dir is the empty string, Run runs the command in the
    // calling process's current directory.
    //
    // On Unix systems, the value of Dir also determines the
    // child process's PWD environment variable if not otherwise
    // specified. A Unix process represents its working directory
    // not by name but as an implicit reference to a node in the
    // file tree. So, if the child process obtains its working
    // directory by calling a function such as C's getcwd, which
    // computes the canonical name by walking up the file tree, it
    // will not recover the original value of Dir if that value
    // was an alias involving symbolic links. However, if the
    // child process calls Go's [os.Getwd] or GNU C's
    // get_current_dir_name, and the value of PWD is an alias for
    // the current directory, those functions will return the
    // value of PWD, which matches the value of Dir.
    public @string Dir;
    // Stdin specifies the process's standard input.
    //
    // If Stdin is nil, the process reads from the null device (os.DevNull).
    //
    // If Stdin is an *os.File, the process's standard input is connected
    // directly to that file.
    //
    // Otherwise, during the execution of the command a separate
    // goroutine reads from Stdin and delivers that data to the command
    // over a pipe. In this case, Wait does not complete until the goroutine
    // stops copying, either because it has reached the end of Stdin
    // (EOF or a read error), or because writing to the pipe returned an error,
    // or because a nonzero WaitDelay was set and expired.
    public io.Reader Stdin;
    // Stdout and Stderr specify the process's standard output and error.
    //
    // If either is nil, Run connects the corresponding file descriptor
    // to the null device (os.DevNull).
    //
    // If either is an *os.File, the corresponding output from the process
    // is connected directly to that file.
    //
    // Otherwise, during the execution of the command a separate goroutine
    // reads from the process over a pipe and delivers that data to the
    // corresponding Writer. In this case, Wait does not complete until the
    // goroutine reaches EOF or encounters an error or a nonzero WaitDelay
    // expires.
    //
    // If Stdout and Stderr are the same writer, and have a type that can
    // be compared with ==, at most one goroutine at a time will call Write.
    public io.Writer Stdout;
    public io.Writer Stderr;
    // ExtraFiles specifies additional open files to be inherited by the
    // new process. It does not include standard input, standard output, or
    // standard error. If non-nil, entry i becomes file descriptor 3+i.
    //
    // ExtraFiles is not supported on Windows.
    public slice<ж<os.File>> ExtraFiles;
    // SysProcAttr holds optional, operating system-specific attributes.
    // Run passes it to os.StartProcess as the os.ProcAttr's Sys field.
    public ж<syscall.SysProcAttr> SysProcAttr;
    // Process is the underlying process, once started.
    public ж<os.Process> Process;
    // ProcessState contains information about an exited process.
    // If the process was started successfully, Wait or Run will
    // populate its ProcessState when the command completes.
    public ж<os.ProcessState> ProcessState;
    // ctx is the context passed to CommandContext, if any.
    internal context.Context ctx;
    public error Err; // LookPath error, if any.
    // If Cancel is non-nil, the command must have been created with
    // CommandContext and Cancel will be called when the command's
    // Context is done. By default, CommandContext sets Cancel to
    // call the Kill method on the command's Process.
    //
    // Typically a custom Cancel will send a signal to the command's
    // Process, but it may instead take other actions to initiate cancellation,
    // such as closing a stdin or stdout pipe or sending a shutdown request on a
    // network socket.
    //
    // If the command exits with a success status after Cancel is
    // called, and Cancel does not return an error equivalent to
    // os.ErrProcessDone, then Wait and similar methods will return a non-nil
    // error: either an error wrapping the one returned by Cancel,
    // or the error from the Context.
    // (If the command exits with a non-success status, or Cancel
    // returns an error that wraps os.ErrProcessDone, Wait and similar methods
    // continue to return the command's usual exit status.)
    //
    // If Cancel is set to nil, nothing will happen immediately when the command's
    // Context is done, but a nonzero WaitDelay will still take effect. That may
    // be useful, for example, to work around deadlocks in commands that do not
    // support shutdown signals but are expected to always finish quickly.
    //
    // Cancel will not be called if Start returns a non-nil error.
    public Func<error> Cancel;
    // If WaitDelay is non-zero, it bounds the time spent waiting on two sources
    // of unexpected delay in Wait: a child process that fails to exit after the
    // associated Context is canceled, and a child process that exits but leaves
    // its I/O pipes unclosed.
    //
    // The WaitDelay timer starts when either the associated Context is done or a
    // call to Wait observes that the child process has exited, whichever occurs
    // first. When the delay has elapsed, the command shuts down the child process
    // and/or its I/O pipes.
    //
    // If the child process has failed to exit — perhaps because it ignored or
    // failed to receive a shutdown signal from a Cancel function, or because no
    // Cancel function was set — then it will be terminated using os.Process.Kill.
    //
    // Then, if the I/O pipes communicating with the child process are still open,
    // those pipes are closed in order to unblock any goroutines currently blocked
    // on Read or Write calls.
    //
    // If pipes are closed due to WaitDelay, no Cancel call has occurred,
    // and the command has otherwise exited with a successful status, Wait and
    // similar methods will return ErrWaitDelay instead of nil.
    //
    // If WaitDelay is zero (the default), I/O pipes will be read until EOF,
    // which might not occur until orphaned subprocesses of the command have
    // also closed their descriptors for the pipes.
    public time.Duration WaitDelay;
    // childIOFiles holds closers for any of the child process's
    // stdin, stdout, and/or stderr files that were opened by the Cmd itself
    // (not supplied by the caller). These should be closed as soon as they
    // are inherited by the child process.
    internal slice<io.Closer> childIOFiles;
    // parentIOPipes holds closers for the parent's end of any pipes
    // connected to the child's stdin, stdout, and/or stderr streams
    // that were opened by the Cmd itself (not supplied by the caller).
    // These should be closed after Wait sees the command and copying
    // goroutines exit, or after WaitDelay has expired.
    internal slice<io.Closer> parentIOPipes;
    // goroutine holds a set of closures to execute to copy data
    // to and/or from the command's I/O pipes.
    internal slice<Func<error>> goroutine;
    // If goroutineErr is non-nil, it receives the first error from a copying
    // goroutine once all such goroutines have completed.
    // goroutineErr is set to nil once its error has been received.
    internal /*<-*/channel<error> goroutineErr = /*<-*/channel<error>.RecvOnly;
    // If ctxResult is non-nil, it receives the result of watchCtx exactly once.
    internal /*<-*/channel<ctxResult> ctxResult = /*<-*/channel<ctxResult>.RecvOnly;
    // The stack saved when the Command was created, if GODEBUG contains
    // execwait=2. Used for debugging leaks.
    internal slice<byte> createdByStack;
    // For a security release long ago, we created x/sys/execabs,
    // which manipulated the unexported lookPathErr error field
    // in this struct. For Go 1.19 we exported the field as Err error,
    // above, but we have to keep lookPathErr around for use by
    // old programs building against new toolchains.
    // The String and Start methods look for an error in lookPathErr
    // in preference to Err, to preserve the errors that execabs sets.
    //
    // In general we don't guarantee misuse of reflect like this,
    // but the misuse of reflect was by us, the best of various bad
    // options to fix the security problem, and people depend on
    // those old copies of execabs continuing to work.
    // The result is that we have to leave this variable around for the
    // rest of time, a compatibility scar.
    //
    // See https://go.dev/blog/path-security
    // and https://go.dev/issue/43724 for more context.
    internal error lookPathErr;
    // cachedLookExtensions caches the result of calling lookExtensions.
    // It is set when Command is called with an absolute path, letting it do
    // the work of resolving the extension, so Start doesn't need to do it again.
    // This is only used on Windows.
    internal Cmd_cachedLookExtensions cachedLookExtensions;
}

// A ctxResult reports the result of watching the Context associated with a
// running command (and sending corresponding signals if needed).
[GoType] partial struct ctxResult {
    internal error err;
    // If timer is non-nil, it expires after WaitDelay has elapsed after
    // the Context is done.
    //
    // (If timer is nil, that means that the Context was not done before the
    // command completed, or no WaitDelay was set, or the WaitDelay already
    // expired and its effect was already applied.)
    internal ж<time.Timer> timer;
}

internal static ж<godebug.Setting> execwait = godebug.New("#execwait"u8);

internal static ж<godebug.Setting> execerrdot = godebug.New("execerrdot"u8);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string setGodebugExecwait2Toˢ = " (set GODEBUG=execwait=2 to capture stacks for debugging)"u8;
internal static readonly @string godebugExecwait2Detectedˢ = "GODEBUG=execwait=2 detected a leaked exec.Cmd created by:\n"u8;

// Command returns the [Cmd] struct to execute the named program with
// the given arguments.
//
// It sets only the Path and Args in the returned structure.
//
// If name contains no path separators, Command uses [LookPath] to
// resolve name to a complete path if possible. Otherwise it uses name
// directly as Path.
//
// The returned Cmd's Args field is constructed from the command name
// followed by the elements of arg, so arg should not include the
// command name itself. For example, Command("echo", "hello").
// Args[0] is always name, not the possibly resolved Path.
//
// On Windows, processes receive the whole command line as a single string
// and do their own parsing. Command combines and quotes Args into a command
// line string with an algorithm compatible with applications using
// CommandLineToArgvW (which is the most common way). Notable exceptions are
// msiexec.exe and cmd.exe (and thus, all batch files), which have a different
// unquoting algorithm. In these or other similar cases, you can do the
// quoting yourself and provide the full command line in SysProcAttr.CmdLine,
// leaving Args empty.
public static partial ж<Cmd> Command(@string name, params ꓸꓸꓸstring argʗp) {
    var arg = argʗp.sslice();

    var cmd = Ꮡ(new Cmd(
        Path: name,
        Args: appendꓸꓸꓸ(new @string[]{name}.slice(), arg)
    ));
    {
        @string v = execwait.Value(); if (v != ""u8) {
            if (v == "2"u8) {
                // Obtain the caller stack. (This is equivalent to runtime/debug.Stack,
                // copied to avoid importing the whole package.)
                var stack = new slice<byte>(1024);
                while (ᐧ) {
                    nint n = runtime.Stack(stack, false);
                    if (n < len(stack)) {
                        stack = stack.slice(0, n);
                        break;
                    }
                    stack = new slice<byte>(2 * len(stack));
                }
                {
                    nint i = bytes.Index(stack, slice<byte>("\nos/exec.Command("u8)); if (i >= 0) {
                        stack = stack.slice(i + 1);
                    }
                }
                cmd.Value.createdByStack = stack;
            }
            runtime.SetFinalizer(cmd.OrTypedNil(), (ж<Cmd> c) => {
                if ((~c).Process != nil && (~c).ProcessState == nil) {
                    @string debugHint = ""u8;
                    if ((~c).createdByStack == default!){
                        debugHint = setGodebugExecwait2Toˢ;
                    } else {
                        os.Stderr.WriteString(godebugExecwait2Detectedˢ);
                        os.Stderr.Write((~c).createdByStack);
                        os.Stderr.WriteString("\n"u8);
                        debugHint = ""u8;
                    }
                    throw panic("exec: Cmd started a Process but leaked without a call to Wait" + debugHint);
                }
            });
        }
    }
    if (filepath.Base(name) == name){
        var (lp, err) = LookPath(name);
        if (lp != ""u8) {
            // Update cmd.Path even if err is non-nil.
            // If err is ErrDot (especially on Windows), lp may include a resolved
            // extension (like .exe or .bat) that should be preserved.
            cmd.Value.Path = lp;
        }
        if (err != default!) {
            cmd.Value.Err = err;
        }
    } else 
    if (runtime.GOOS == "windows"u8 && filepath.IsAbs(name)) {
        // We may need to add a filename extension from PATHEXT
        // or verify an extension that is already present.
        // Since the path is absolute, its extension should be unambiguous
        // and independent of cmd.Dir, and we can go ahead and cache the lookup now.
        //
        // Note that we don't cache anything here for relative paths, because
        // cmd.Dir may be set after we return from this function and that may
        // cause the command to resolve to a different extension.
        {
            var (lp, err) = lookExtensions(name, ""u8); if (err == default!){
                (cmd.Value.cachedLookExtensions.@in, cmd.Value.cachedLookExtensions.@out) = (name, lp);
            } else {
                cmd.Value.Err = err;
            }
        }
    }
    return cmd;
}

// CommandContext is like [Command] but includes a context.
//
// The provided context is used to interrupt the process
// (by calling cmd.Cancel or [os.Process.Kill])
// if the context becomes done before the command completes on its own.
//
// CommandContext sets the command's Cancel function to invoke the Kill method
// on its Process, and leaves its WaitDelay unset. The caller may change the
// cancellation behavior by modifying those fields before starting the command.
public static ж<Cmd> CommandContext(context.Context ctx, @string name, params ꓸꓸꓸstring argʗp) {
    var arg = argʗp.sslice();

    if (ctx == default!) {
        throw panic("nil Context");
    }
    var cmd = Command(name, arg.ꓸꓸꓸ);
    cmd.Value.ctx = ctx;
    var cmdʗ1 = cmd;
    cmd.Value.Cancel = () => (~cmdʗ1).Process.Kill();
    return cmd;
}

// String returns a human-readable description of c.
// It is intended only for debugging.
// In particular, it is not suitable for use as input to a shell.
// The output of String may vary across Go releases.
[GoRecv] public static @string String(this ref Cmd c) {
    if (c.Err != default! || c.lookPathErr != default!) {
        // failed to resolve path; report the original requested path (plus args)
        return strings.Join(c.Args, " "u8);
    }
    // report the exact executable path (plus args)
    var b = @new<strings.Builder>();
    b.WriteString(c.Path);
    foreach (var (_, a) in c.Args[1..]) {
        b.WriteByte((rune)' ');
        b.WriteString(a);
    }
    return b.String();
}

// interfaceEqual protects against panics from doing equality tests on
// two interfaces with non-comparable underlying types.
internal static bool interfaceEqual(any a, any b) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            recover();
        }, ref ᒐ);
        return AreEqual(a, b);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

[GoRecv] internal static slice<@string> argv(this ref Cmd c) {
    if (len(c.Args) > 0) {
        return c.Args;
    }
    return new @string[]{c.Path}.slice();
}

internal static (ж<os.File>, error) childStdin(this ж<Cmd> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    if (c.Stdin == default!) {
        var (f, errΔ1) = os.Open(os.DevNull);
        if (errΔ1 != default!) {
            return (default!, errΔ1);
        }
        c.childIOFiles = append(c.childIOFiles, (io.Closer)(new os_FileжCloser(f)));
        return (f, default!);
    }
    {
        var (f, ok) = c.Stdin._<ж<os.File>>(ᐧ); if (ok) {
            return (f, default!);
        }
    }
    var (pr, pw, err) = os.Pipe();
    if (err != default!) {
        return (default!, err);
    }
    c.childIOFiles = append(c.childIOFiles, (io.Closer)(new os_FileжCloser(pr)));
    c.parentIOPipes = append(c.parentIOPipes, (io.Closer)(new os_FileжCloser(pw)));
    var pwʗ1 = pw;
    c.goroutine = append(c.goroutine, () => {
        var (_, errΔ2) = io.Copy(new os.FileжWriter(pwʗ1), Ꮡc.Value.Stdin);
        if (skipStdinCopyError(errΔ2)) {
            errΔ2 = default!;
        }
        {
            var err1 = pwʗ1.Close(); if (errΔ2 == default!) {
                errΔ2 = err1;
            }
        }
        return errΔ2;
    });
    return (pr, default!);
}

[GoRecv] internal static (ж<os.File>, error) childStdout(this ref Cmd c) {
    return c.writerDescriptor(c.Stdout);
}

[GoRecv] internal static (ж<os.File>, error) childStderr(this ref Cmd c, ж<os.File> ᏑchildStdout) {
    if (c.Stderr != default! && interfaceEqual(c.Stderr, c.Stdout)) {
        return (ᏑchildStdout, default!);
    }
    return c.writerDescriptor(c.Stderr);
}

// writerDescriptor returns an os.File to which the child process
// can write to send data to w.
//
// If w is nil, writerDescriptor returns a File that writes to os.DevNull.
[GoRecv] internal static (ж<os.File>, error) writerDescriptor(this ref Cmd c, io.Writer w) {
    if (w == default!) {
        var (f, errΔ1) = os.OpenFile(os.DevNull, os.O_WRONLY, 0);
        if (errΔ1 != default!) {
            return (default!, errΔ1);
        }
        c.childIOFiles = append(c.childIOFiles, (io.Closer)(new os_FileжCloser(f)));
        return (f, default!);
    }
    {
        var (f, ok) = w._<ж<os.File>>(ᐧ); if (ok) {
            return (f, default!);
        }
    }
    var (pr, pw, err) = os.Pipe();
    if (err != default!) {
        return (default!, err);
    }
    c.childIOFiles = append(c.childIOFiles, (io.Closer)(new os_FileжCloser(pw)));
    c.parentIOPipes = append(c.parentIOPipes, (io.Closer)(new os_FileжCloser(pr)));
    var prʗ1 = pr;
    c.goroutine = append(c.goroutine, () => {
        var (_, errΔ2) = io.Copy(w, new os_FileжReader(prʗ1));
        prʗ1.Close(); // in case io.Copy stopped due to write error
        return errΔ2;
    });
    return (pw, default!);
}

internal static void closeDescriptors(slice<io.Closer> closers) {
    foreach (var (_, fd) in closers) {
        fd.Close();
    }
}

// Run starts the specified command and waits for it to complete.
//
// The returned error is nil if the command runs, has no problems
// copying stdin, stdout, and stderr, and exits with a zero exit
// status.
//
// If the command starts but does not complete successfully, the error is of
// type [*ExitError]. Other error types may be returned for other situations.
//
// If the calling goroutine has locked the operating system thread
// with [runtime.LockOSThread] and modified any inheritable OS-level
// thread state (for example, Linux or Plan 9 name spaces), the new
// process will inherit the caller's thread state.
public static error Run(this ж<Cmd> Ꮡc) {
    {
        var err = Ꮡc.Start(); if (err != default!) {
            return err;
        }
    }
    return Ꮡc.Wait();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string execAlreadyStartedˢ = "exec: already started"u8;
internal static readonly @string execNoCommandˢ = "exec: no command"u8;
internal static readonly @string execCommandWithANonNilˢ = "exec: command with a non-nil Cancel was not created with CommandContext"u8;

[GoType("dyn")] internal partial struct Start_goroutineStatus {
    internal nint running;
    internal error firstErr;
}

// Start starts the specified command but does not wait for it to complete.
//
// If Start returns successfully, the c.Process field will be set.
//
// After a successful call to Start the [Cmd.Wait] method must be called in
// order to release associated system resources.
public static partial error Start(this ж<Cmd> Ꮡc) {
    GoFrame ᒐ = default;
    try {
        ref var c = ref Ꮡc.DerefOrNull();

        // Check for doubled Start calls before we defer failure cleanup. If the prior
        // call to Start succeeded, we don't want to spuriously close its pipes.
        if (c.Process != nil) {
            return errors.New(execAlreadyStartedˢ);
        }
        var started = false;
        defer(() => {
            closeDescriptors(Ꮡc.Value.childIOFiles);
            Ꮡc.Value.childIOFiles = default!;
            if (!started) {
                closeDescriptors(Ꮡc.Value.parentIOPipes);
                Ꮡc.Value.parentIOPipes = default!;
            }
        }, ref ᒐ);
        if (c.Path == ""u8 && c.Err == default! && c.lookPathErr == default!) {
            c.Err = errors.New(execNoCommandˢ);
        }
        if (c.Err != default! || c.lookPathErr != default!) {
            if (c.lookPathErr != default!) {
                return c.lookPathErr;
            }
            return c.Err;
        }
        @string lp = c.Path;
        if (runtime.GOOS == "windows"u8) {
            if (c.Path == c.cachedLookExtensions.@in){
                // If Command was called with an absolute path, we already resolved
                // its extension and shouldn't need to do so again (provided c.Path
                // wasn't set to another value between the calls to Command and Start).
                lp = c.cachedLookExtensions.@out;
            } else {
                // If *Cmd was made without using Command at all, or if Command was
                // called with a relative path, we had to wait until now to resolve
                // it in case c.Dir was changed.
                //
                // Unfortunately, we cannot write the result back to c.Path because programs
                // may assume that they can call Start concurrently with reading the path.
                // (It is safe and non-racy to do so on Unix platforms, and users might not
                // test with the race detector on all platforms;
                // see https://go.dev/issue/62596.)
                //
                // So we will pass the fully resolved path to os.StartProcess, but leave
                // c.Path as is: missing a bit of logging information seems less harmful
                // than triggering a surprising data race, and if the user really cares
                // about that bit of logging they can always use LookPath to resolve it.
                error errΔ1 = default!;
                (lp, errΔ1) = lookExtensions(c.Path, c.Dir);
                if (errΔ1 != default!) {
                    return errΔ1;
                }
            }
        }
        if (c.Cancel != default! && c.ctx == default!) {
            return errors.New(execCommandWithANonNilˢ);
        }
        if (c.ctx != default!) {
            var selᴛ1 = c.ctx.Done();
            switch (trySelect(ᐸꟷ(selᴛ1, ꓸꓸꓸ))) {
            case 0 when selᴛ1.ꟷᐳ(out _): {
                return c.ctx.Err();
            }
            default: {
                break;
            }}
        }
        var childFiles = new slice<ж<os.File>>(0, 3 + len(c.ExtraFiles));
        var (stdin, err) = Ꮡc.childStdin();
        if (err != default!) {
            return err;
        }
        childFiles = append(childFiles, stdin);
        (var stdout, err) = c.childStdout();
        if (err != default!) {
            return err;
        }
        childFiles = append(childFiles, stdout);
        (var stderr, err) = c.childStderr(stdout);
        if (err != default!) {
            return err;
        }
        childFiles = append(childFiles, stderr);
        childFiles = appendꓸꓸꓸ(childFiles, c.ExtraFiles);
        (var env, err) = c.environ();
        if (err != default!) {
            return err;
        }
        (c.Process, err) = os.StartProcess(lp, c.argv(), Ꮡ(new os.ProcAttr(
            Dir: c.Dir,
            Files: childFiles,
            Env: env,
            Sys: c.SysProcAttr
        )));
        if (err != default!) {
            return err;
        }
        started = true;
        // Don't allocate the goroutineErr channel unless there are goroutines to start.
        if (len(c.goroutine) > 0) {
            var goroutineErr = new channel<error>(1);
            c.goroutineErr = goroutineErr.WithDirection(GoChanDir.Recv);
            var statusc = new channel<Start_goroutineStatus>(1);
            statusc.ᐸꟷ(new Start_goroutineStatus(running: len(c.goroutine)));
            foreach (var (_, fn) in c.goroutine) {
                var goroutineErrʗ1 = goroutineErr;
                var statuscʗ1 = statusc;
                goǃ((Func<error> fnΔ1) => {
                    var errΔ2 = fnΔ1();
                    ref var status = ref heap<Start_goroutineStatus>(out var Ꮡstatus);
                    status = ᐸꟷ(statuscʗ1);
                    if (status.firstErr == default!) {
                        status.firstErr = errΔ2;
                    }
                    status.running--;
                    if (status.running == 0){
                        goroutineErrʗ1.ᐸꟷ(status.firstErr);
                    } else {
                        statuscʗ1.ᐸꟷ(status);
                    }
                }, fn);
            }
            c.goroutine = default!; // Allow the goroutines' closures to be GC'd when they complete.
        }
        // If we have anything to do when the command's Context expires,
        // start a goroutine to watch for cancellation.
        //
        // (Even if the command was created by CommandContext, a helper library may
        // have explicitly set its Cancel field back to nil, indicating that it should
        // be allowed to continue running after cancellation after all.)
        if ((c.Cancel != default! || c.WaitDelay != 0) && c.ctx != default! && c.ctx.Done() != default!) {
            var resultc = new channel<ctxResult>(0);
            c.ctxResult = resultc.WithDirection(GoChanDir.Recv);
            goǃ(Ꮡc.watchCtx, resultc.WithDirection(GoChanDir.Send));
        }
        return default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// watchCtx watches c.ctx until it is able to send a result to resultc.
//
// If c.ctx is done before a result can be sent, watchCtx calls c.Cancel,
// and/or kills cmd.Process it after c.WaitDelay has elapsed.
//
// watchCtx manipulates c.goroutineErr, so its result must be received before
// c.awaitGoroutines is called.
[GoRecv] internal static void watchCtx(this ref Cmd c, channel/*<-*/<ctxResult> resultc) {
    var selᴛ2 = resultc.ᐸꟷ(new ctxResult(nil), ꓸꓸꓸ);
    var selᴛ3 = c.ctx.Done();
    switch (select(selᴛ2, ᐸꟷ(selᴛ3, ꓸꓸꓸ))) {
    case 0: {
        return;
    }
    case 1 when selᴛ3.ꟷᐳ(out _): {
        break;
    }}
    error err = default!;
    if (c.Cancel != default!) {
        {
            var interruptErr = c.Cancel(); if (interruptErr == default!){
                // We appear to have successfully interrupted the command, so any
                // program behavior from this point may be due to ctx even if the
                // command exits with code 0.
                err = c.ctx.Err();
            } else 
            if (errors.Is(interruptErr, os.ErrProcessDone)){
            } else {
                // The process already finished: we just didn't notice it yet.
                // (Perhaps c.Wait hadn't been called, or perhaps it happened to race with
                // c.ctx being canceled.) Don't inject a needless error.
                err = new wrappedError(
                    prefix: "exec: canceling Cmd"u8,
                    err: interruptErr
                );
            }
        }
    }
    if (c.WaitDelay == 0) {
        resultc.ᐸꟷ(new ctxResult(err: err));
        return;
    }
    var timer = time.NewTimer(c.WaitDelay);
    var selᴛ4 = resultc.ᐸꟷ(new ctxResult(err: err, timer: timer), ꓸꓸꓸ);
    var selᴛ5 = (~timer).C;
    switch (select(selᴛ4, ᐸꟷ(selᴛ5, ꓸꓸꓸ))) {
    case 0: {
        return;
    }
    case 1 when selᴛ5.ꟷᐳ(out _): {
        break;
    }}
    // c.Process.Wait returned and we've handed the timer off to c.Wait.
    // It will take care of goroutine shutdown from here.
    var killed = false;
    {
        var killErr = c.Process.Kill(); if (killErr == default!){
            // We appear to have killed the process. c.Process.Wait should return a
            // non-nil error to c.Wait unless the Kill signal races with a successful
            // exit, and if that does happen we shouldn't report a spurious error,
            // so don't set err to anything here.
            killed = true;
        } else 
        if (!errors.Is(killErr, os.ErrProcessDone)) {
            err = new wrappedError(
                prefix: "exec: killing Cmd"u8,
                err: killErr
            );
        }
    }
    if (c.goroutineErr != default!) {
        var selᴛ6 = c.goroutineErr;
        switch (trySelect(ᐸꟷ(selᴛ6, ꓸꓸꓸ))) {
        case 0 when selᴛ6.ꟷᐳ(out var goroutineErr): {
            if (err == default! && !killed) {
                // Forward goroutineErr only if we don't have reason to believe it was
                // caused by a call to Cancel or Kill above.
                err = goroutineErr;
            }
            break;
        }
        default: {
            closeDescriptors(c.parentIOPipes);
            _ = ᐸꟷ(c.goroutineErr);
            if (err == default!) {
                // Close the child process's I/O pipes, in case it abandoned some
                // subprocess that inherited them and is still holding them open
                // (see https://go.dev/issue/23019).
                //
                // We close the goroutine pipes only after we have sent any signals we're
                // going to send to the process (via Signal or Kill above): if we send
                // SIGKILL to the process, we would prefer for it to die of SIGKILL, not
                // SIGPIPE. (However, this may still cause any orphaned subprocesses to
                // terminate with SIGPIPE.)
                // Wait for the copying goroutines to finish, but report ErrWaitDelay for
                // the error: any other error here could result from closing the pipes.
                err = ErrWaitDelay;
            }
            break;
        }}
        // Since we have already received the only result from c.goroutineErr,
        // set it to nil to prevent awaitGoroutines from blocking on it.
        c.goroutineErr = default!;
    }
    resultc.ᐸꟷ(new ctxResult(err: err));
}

// An ExitError reports an unsuccessful exit by a command.
[GoType] partial struct ExitError {
    public partial ref ж<os_package.ProcessState> ProcessState { get; }
    // Stderr holds a subset of the standard error output from the
    // Cmd.Output method if standard error was not otherwise being
    // collected.
    //
    // If the error output is long, Stderr may contain only a prefix
    // and suffix of the output, with the middle replaced with
    // text about the number of omitted bytes.
    //
    // Stderr is provided for debugging, for inclusion in error messages.
    // Users with other needs should redirect Cmd.Stderr as needed.
    public slice<byte> Stderr;
}

[GoRecv] public static @string Error(this ref ExitError e) {
    return e.ProcessState.String();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string execNotStartedˢ = "exec: not started"u8;
internal static readonly @string execWaitWasAlreadyCalledˢ = "exec: Wait was already called"u8;

// Wait waits for the command to exit and waits for any copying to
// stdin or copying from stdout or stderr to complete.
//
// The command must have been started by [Cmd.Start].
//
// The returned error is nil if the command runs, has no problems
// copying stdin, stdout, and stderr, and exits with a zero exit
// status.
//
// If the command fails to run or doesn't complete successfully, the
// error is of type [*ExitError]. Other error types may be
// returned for I/O problems.
//
// If any of c.Stdin, c.Stdout or c.Stderr are not an [*os.File], Wait also waits
// for the respective I/O loop copying to or from the process to complete.
//
// Wait releases any resources associated with the [Cmd].
public static error Wait(this ж<Cmd> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    if (c.Process == nil) {
        return errors.New(execNotStartedˢ);
    }
    if (c.ProcessState != nil) {
        return errors.New(execWaitWasAlreadyCalledˢ);
    }
    var (state, err) = c.Process.Wait();
    if (err == default! && !state.Success()) {
        err = new ExitErrorжerror(Ꮡ(new ExitError(ProcessState: state)));
    }
    c.ProcessState = state;
    ж<time.Timer> timer = default!;
    if (c.ctxResult != default!) {
        var watch = ᐸꟷ(c.ctxResult);
        timer = watch.timer;
        // If c.Process.Wait returned an error, prefer that.
        // Otherwise, report any error from the watchCtx goroutine,
        // such as a Context cancellation or a WaitDelay overrun.
        if (err == default! && watch.err != default!) {
            err = watch.err;
        }
    }
    {
        var goroutineErr = Ꮡc.awaitGoroutines(timer); if (err == default!) {
            // Report an error from the copying goroutines only if the program otherwise
            // exited normally on its own. Otherwise, the copying error may be due to the
            // abnormal termination.
            err = goroutineErr;
        }
    }
    closeDescriptors(c.parentIOPipes);
    c.parentIOPipes = default!;
    return err;
}

// awaitGoroutines waits for the results of the goroutines copying data to or
// from the command's I/O pipes.
//
// If c.WaitDelay elapses before the goroutines complete, awaitGoroutines
// forcibly closes their pipes and returns ErrWaitDelay.
//
// If timer is non-nil, it must send to timer.C at the end of c.WaitDelay.
internal static error awaitGoroutines(this ж<Cmd> Ꮡc, ж<time.Timer> Ꮡtimer) {
    GoFrame ᒐ = default;
    try {
        ref var c = ref Ꮡc.DerefOrNull();
        ref var timer = ref Ꮡtimer.DerefOrNull();

        defer(() => {
            if (Ꮡtimer != nil) {
                Ꮡtimer.Stop();
            }
            Ꮡc.Value.goroutineErr = default!;
        }, ref ᒐ);
        if (c.goroutineErr == default!) {
            return default!; // No running goroutines to await.
        }
        if (Ꮡtimer == nil) {
            if (c.WaitDelay == 0) {
                return ᐸꟷ(c.goroutineErr);
            }
            var selᴛ7 = c.goroutineErr;
            switch (trySelect(ᐸꟷ(selᴛ7, ꓸꓸꓸ))) {
            case 0 when selᴛ7.ꟷᐳ(out var err): {
                return err;
            }
            default: {
                break;
            }}
            // Avoid the overhead of starting a timer.
            // No existing timer was started: either there is no Context associated with
            // the command, or c.Process.Wait completed before the Context was done.
            Ꮡtimer = time.NewTimer(c.WaitDelay); timer = ref Ꮡtimer.DerefOrNull();
        }
        var selᴛ8 = timer.C;
        var selᴛ9 = c.goroutineErr;
        switch (select(ᐸꟷ(selᴛ8, ꓸꓸꓸ), ᐸꟷ(selᴛ9, ꓸꓸꓸ))) {
        case 0 when selᴛ8.ꟷᐳ(out _): {
            closeDescriptors(c.parentIOPipes);
            _ = ᐸꟷ(c.goroutineErr);
            return ErrWaitDelay;
        }
        case 1 when selᴛ9.ꟷᐳ(out var err): {
            return err;
        }}
        return default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string execStdoutAlreadySetˢ = "exec: Stdout already set"u8;

// Wait for the copying goroutines to finish, but ignore any error
// (since it was probably caused by closing the pipes).

// Output runs the command and returns its standard output.
// Any returned error will usually be of type [*ExitError].
// If c.Stderr was nil and the returned error is of type
// [*ExitError], Output populates the Stderr field of the
// returned error.
public static (slice<byte>, error) Output(this ж<Cmd> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    if (c.Stdout != default!) {
        return (default!, errors.New(execStdoutAlreadySetˢ));
    }
    ref var stdout = ref heap(new bytes.Buffer(), out var Ꮡstdout);
    c.Stdout = new bytes_BufferжWriter(Ꮡstdout);
    var captureErr = c.Stderr == default!;
    if (captureErr) {
        c.Stderr = new prefixSuffixSaverжWriter(Ꮡ(new prefixSuffixSaver(N: (32 << (int)(10)))));
    }
    var err = Ꮡc.Run();
    if (err != default! && captureErr) {
        {
            var (ee, ok) = err._<ж<ExitError>>(ᐧ); if (ok) {
                ee.Value.Stderr = c.Stderr._<ж<prefixSuffixSaver>>().Bytes();
            }
        }
    }
    return (stdout.Bytes(), err);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string execStderrAlreadySetˢ = "exec: Stderr already set"u8;

// CombinedOutput runs the command and returns its combined standard
// output and standard error.
public static (slice<byte>, error) CombinedOutput(this ж<Cmd> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    if (c.Stdout != default!) {
        return (default!, errors.New(execStdoutAlreadySetˢ));
    }
    if (c.Stderr != default!) {
        return (default!, errors.New(execStderrAlreadySetˢ));
    }
    ref var b = ref heap(new bytes.Buffer(), out var Ꮡb);
    c.Stdout = new bytes_BufferжWriter(Ꮡb);
    c.Stderr = new bytes_BufferжWriter(Ꮡb);
    var err = Ꮡc.Run();
    return (b.Bytes(), err);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string execStdinAlreadySetˢ = "exec: Stdin already set"u8;
internal static readonly @string execStdinPipeAfterˢ = "exec: StdinPipe after process started"u8;

// StdinPipe returns a pipe that will be connected to the command's
// standard input when the command starts.
// The pipe will be closed automatically after [Cmd.Wait] sees the command exit.
// A caller need only call Close to force the pipe to close sooner.
// For example, if the command being run will not exit until standard input
// is closed, the caller must close the pipe.
[GoRecv] public static (io.WriteCloser, error) StdinPipe(this ref Cmd c) {
    if (c.Stdin != default!) {
        return (default!, errors.New(execStdinAlreadySetˢ));
    }
    if (c.Process != nil) {
        return (default!, errors.New(execStdinPipeAfterˢ));
    }
    var (pr, pw, err) = os.Pipe();
    if (err != default!) {
        return (default!, err);
    }
    c.Stdin = new os_FileжReader(pr);
    c.childIOFiles = append(c.childIOFiles, (io.Closer)(new os_FileжCloser(pr)));
    c.parentIOPipes = append(c.parentIOPipes, (io.Closer)(new os_FileжCloser(pw)));
    return (new os_FileжWriteCloser(pw), default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string execStdoutPipeAfterˢ = "exec: StdoutPipe after process started"u8;

// StdoutPipe returns a pipe that will be connected to the command's
// standard output when the command starts.
//
// [Cmd.Wait] will close the pipe after seeing the command exit, so most callers
// need not close the pipe themselves. It is thus incorrect to call Wait
// before all reads from the pipe have completed.
// For the same reason, it is incorrect to call [Cmd.Run] when using StdoutPipe.
// See the example for idiomatic usage.
[GoRecv] public static (io.ReadCloser, error) StdoutPipe(this ref Cmd c) {
    if (c.Stdout != default!) {
        return (default!, errors.New(execStdoutAlreadySetˢ));
    }
    if (c.Process != nil) {
        return (default!, errors.New(execStdoutPipeAfterˢ));
    }
    var (pr, pw, err) = os.Pipe();
    if (err != default!) {
        return (default!, err);
    }
    c.Stdout = new os.FileжWriter(pw);
    c.childIOFiles = append(c.childIOFiles, (io.Closer)(new os_FileжCloser(pw)));
    c.parentIOPipes = append(c.parentIOPipes, (io.Closer)(new os_FileжCloser(pr)));
    return (new os_FileжReadCloser(pr), default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string execStderrPipeAfterˢ = "exec: StderrPipe after process started"u8;

// StderrPipe returns a pipe that will be connected to the command's
// standard error when the command starts.
//
// [Cmd.Wait] will close the pipe after seeing the command exit, so most callers
// need not close the pipe themselves. It is thus incorrect to call Wait
// before all reads from the pipe have completed.
// For the same reason, it is incorrect to use [Cmd.Run] when using StderrPipe.
// See the StdoutPipe example for idiomatic usage.
[GoRecv] public static (io.ReadCloser, error) StderrPipe(this ref Cmd c) {
    if (c.Stderr != default!) {
        return (default!, errors.New(execStderrAlreadySetˢ));
    }
    if (c.Process != nil) {
        return (default!, errors.New(execStderrPipeAfterˢ));
    }
    var (pr, pw, err) = os.Pipe();
    if (err != default!) {
        return (default!, err);
    }
    c.Stderr = new os.FileжWriter(pw);
    c.childIOFiles = append(c.childIOFiles, (io.Closer)(new os_FileжCloser(pw)));
    c.parentIOPipes = append(c.parentIOPipes, (io.Closer)(new os_FileжCloser(pr)));
    return (new os_FileжReadCloser(pr), default!);
}

// prefixSuffixSaver is an io.Writer which retains the first N bytes
// and the last N bytes written to it. The Bytes() methods reconstructs
// it with a pretty error message.
[GoType] partial struct prefixSuffixSaver {
    public nint N; // max size of prefix or suffix
    internal slice<byte> prefix;
    internal slice<byte> suffix; // ring buffer once len(suffix) == N
    internal nint suffixOff;   // offset to write into suffix
    internal int64 skipped;
}

// TODO(bradfitz): we could keep one large []byte and use part of it for
// the prefix, reserve space for the '... Omitting N bytes ...' message,
// then the ring buffer suffix, and just rearrange the ring buffer
// suffix when Bytes() is called, but it doesn't seem worth it for
// now just for error messages. It's only ~64KB anyway.
internal static (nint n, error err) Write(this ж<prefixSuffixSaver> Ꮡw, slice<byte> p) {
    ref var w = ref Ꮡw.DerefOrNull();

    nint lenp = len(p);
    p = w.fill(Ꮡw.of(prefixSuffixSaver.Ꮡprefix), p);
    // Only keep the last w.N bytes of suffix data.
    {
        nint overage = len(p) - w.N; if (overage > 0) {
            p = p.slice(overage);
            w.skipped += (int64)overage;
        }
    }
    p = w.fill(Ꮡw.of(prefixSuffixSaver.Ꮡsuffix), p);
    // w.suffix is full now if p is non-empty. Overwrite it in a circle.
    while (len(p) > 0) {
        // 0, 1, or 2 iterations.
        nint nΔ1 = copy(w.suffix.slice(w.suffixOff), p);
        p = p.slice(nΔ1);
        w.skipped += (int64)nΔ1;
        w.suffixOff += nΔ1;
        if (w.suffixOff == w.N) {
            w.suffixOff = 0;
        }
    }
    return (lenp, default!);
}

// fill appends up to len(p) bytes of p to *dst, such that *dst does not
// grow larger than w.N. It returns the un-appended suffix of p.
[GoRecv] internal static slice<byte> /*pRemain*/ fill(this ref prefixSuffixSaver w, ж<slice<byte>> Ꮡdst, slice<byte> p) {
    ref var dst = ref Ꮡdst.DerefOrNull();

    {
        nint remain = w.N - len(dst); if (remain > 0) {
            nint add = min(len(p), remain);
            dst = appendꓸꓸꓸ(dst, p.slice(0, add));
            p = p.slice(add);
        }
    }
    return p;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string omittingˢ = "\n... omitting "u8;
internal static readonly @string bytesˢ = " bytes ...\n"u8;

[GoRecv] internal static slice<byte> Bytes(this ref prefixSuffixSaver w) {
    if (w.suffix == default!) {
        return w.prefix;
    }
    if (w.skipped == 0) {
        return appendꓸꓸꓸ(w.prefix, w.suffix);
    }
    bytes.Buffer buf = default!;
    buf.Grow(len(w.prefix) + len(w.suffix) + 50);
    buf.Write(w.prefix);
    buf.WriteString(omittingˢ);
    buf.WriteString(strconv.FormatInt(w.skipped, 10));
    buf.WriteString(bytesˢ);
    buf.Write(w.suffix.slice(w.suffixOff));
    buf.Write(w.suffix.slice(0, w.suffixOff));
    return buf.Bytes();
}

// environ returns a best-effort copy of the environment in which the command
// would be run as it is currently configured. If an error occurs in computing
// the environment, it is returned alongside the best-effort copy.
[GoRecv] internal static (slice<@string>, error) environ(this ref Cmd c) {
    error err = default!;
    var env = c.Env;
    if (env == default!) {
        (env, err) = execenv.Default(c.SysProcAttr);
        if (err != default!) {
            env = os.Environ();
        }
        // Note that the non-nil err is preserved despite env being overridden.
        if (c.Dir != ""u8) {
            var exprᴛ1 = runtime.GOOS;
            if (exprᴛ1 == "windows"u8 || exprᴛ1 == "plan9"u8) {
            }
            else { /* default: */
                {
                    var (pwd, absErr) = filepath.Abs(c.Dir); if (absErr == default!){
                        // Windows and Plan 9 do not use the PWD variable, so we don't need to
                        // keep it accurate.
                        // On POSIX platforms, PWD represents “an absolute pathname of the
                        // current working directory.” Since we are changing the working
                        // directory for the command, we should also update PWD to reflect that.
                        //
                        // Unfortunately, we didn't always do that, so (as proposed in
                        // https://go.dev/issue/50599) to avoid unintended collateral damage we
                        // only implicitly update PWD when Env is nil. That way, we're much
                        // less likely to override an intentional change to the variable.
                        env = append(env, "PWD="u8 + pwd);
                    } else 
                    if (err == default!) {
                        err = absErr;
                    }
                }
            }

        }
    }
    (env, var dedupErr) = dedupEnv(env);
    if (err == default!) {
        err = dedupErr;
    }
    return (addCriticalEnv(env), err);
}

// Environ returns a copy of the environment in which the command would be run
// as it is currently configured.
[GoRecv] public static slice<@string> Environ(this ref Cmd c) {
    //  Intentionally ignore errors: environ returns a best-effort environment no matter what.
    var (env, _) = c.environ();
    return env;
}

// dedupEnv returns a copy of env with any duplicates removed, in favor of
// later values.
// Items not of the normal environment "key=value" form are preserved unchanged.
// Except on Plan 9, items containing NUL characters are removed, and
// an error is returned along with the remaining values.
internal static (slice<@string>, error) dedupEnv(slice<@string> env) {
    return dedupEnvCase(runtime.GOOS == "windows"u8, runtime.GOOS == "plan9"u8, env);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string execEnvironmentVariableˢ = "exec: environment variable contains NUL"u8;

// dedupEnvCase is dedupEnv with a case option for testing.
// If caseInsensitive is true, the case of keys is ignored.
// If nulOK is false, items containing NUL characters are allowed.
internal static (slice<@string>, error) dedupEnvCase(bool caseInsensitive, bool nulOK, slice<@string> env) {
    // Construct the output in reverse order, to preserve the
    // last occurrence of each key.
    error err = default!;
    var @out = new slice<@string>(0, len(env));
    var saw = new map<@string, bool>(len(env));
    for (nint n = len(env); n > 0; n--) {
        @string kv = env[n - 1];
        // Reject NUL in environment variables to prevent security issues (#56284);
        // except on Plan 9, which uses NUL as os.PathListSeparator (#56544).
        if (!nulOK && strings.IndexByte(kv, 0) != -1) {
            err = errors.New(execEnvironmentVariableˢ);
            continue;
        }
        nint i = strings.Index(kv, "="u8);
        if (i == 0) {
            // We observe in practice keys with a single leading "=" on Windows.
            // TODO(#49886): Should we consume only the first leading "=" as part
            // of the key, or parse through arbitrarily many of them until a non-"="?
            i = strings.Index(kv[1..], "="u8) + 1;
        }
        if (i < 0) {
            if (kv != ""u8) {
                // The entry is not of the form "key=value" (as it is required to be).
                // Leave it as-is for now.
                // TODO(#52436): should we strip or reject these bogus entries?
                @out = append(@out, kv);
            }
            continue;
        }
        @string k = kv.slice(0, i);
        if (caseInsensitive) {
            k = strings.ToLower(k);
        }
        if (saw[k]) {
            continue;
        }
        saw[k] = true;
        @out = append(@out, kv);
    }
    // Now reverse the slice to restore the original order.
    for (nint i = 0; i < len(@out) / 2; i++) {
        nint j = len(@out) - i - 1;
        (@out[i], @out[j]) = (@out[j], @out[i]);
    }
    return (@out, err);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string systemrootˢ = "SYSTEMROOT"u8;

// addCriticalEnv adds any critical environment variables that are required
// (or at least almost always required) on the operating system.
// Currently this is only used for Windows.
internal static slice<@string> addCriticalEnv(slice<@string> env) {
    if (runtime.GOOS != "windows"u8) {
        return env;
    }
    foreach (var (_, kv) in env) {
        var (k, _, ok) = strings.Cut(kv, "="u8);
        if (!ok) {
            continue;
        }
        if (strings.EqualFold(k, systemrootˢ)) {
            // We already have it.
            return env;
        }
    }
    return append(env, "SYSTEMROOT="u8 + os.Getenv(systemrootˢ));
}

// ErrDot indicates that a path lookup resolved to an executable
// in the current directory due to ‘.’ being in the path, either
// implicitly or explicitly. See the package documentation for details.
//
// Note that functions in this package do not return ErrDot directly.
// Code should use errors.Is(err, ErrDot), not err == ErrDot,
// to test whether a returned error err is due to this condition.
public static error ErrDot = errors.New("cannot run executable found relative to current directory"u8);

// validateLookPath excludes paths that can't be valid
// executable names. See issue #74466 and CVE-2025-47906.
internal static error validateLookPath(@string s) {
    var exprᴛ1 = s;
    if (exprᴛ1 == ""u8 || exprᴛ1 == "."u8 || exprᴛ1 == ".."u8) {
        return ErrNotFound;
    }

    return default!;
}

} // end exec_package
