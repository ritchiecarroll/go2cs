namespace go;

using bufio = bufio_package;
using bytes = bytes_package;
using fmt = fmt_package;
using iter = iter_package;
using Δos = os_package;
using exec = go.os.exec_package;
using runtime = runtime_package;
using strings = strings_package;
using go.os;
using Δio = io_package;

partial class main_package {


partial struct armsᴛ1 /*dyn*/ {
    internal @string name;
    internal Action run;
}
[GoLazy] partial class lazyᴛarms { internal static slice<armsᴛ1> arms = slice<armsᴛ1>([
    new("a: main's defers run, recover reads nil, a goroutine finishes, then no goroutines", armA),
    new("b: a goroutine calls os.Exit(0) after main's Goexit", armB),
    new("c: a goroutine panics after main's Goexit", armC),
    new("d: the goroutine left blocks forever in select{}", armD),
    new("e: the Goexit is raised two deferring frames below main", armE),
    new("f: the Goexit reaches main through iter.Pull's next", armF)
]); }

internal static void Main() {
    @string arm = Δos.Getenv("GOEXIT_MAIN_ARM"); if (arm != ""u8) {
        foreach (var (_, a) in arms) {
            if (a.name[..1] == arm) {
                a.run();
            }
        }
        fmt.Println("unreachable: main returned");
        return;
    }

    foreach (var (_, a) in arms) {
        parent(a.name);
    }
}

internal static partial void armA() {
    GoFrame ᒐ = default;
    try {
        var ready = new channel<EmptyStruct>(0);
        defer(ᴛ1 => close(ᴛ1), ready, ref ᒐ);

        var readyʗ1 = ready;
        goǃ(() => {
            ᐸꟷ(readyʗ1);
            fmt.Println("goroutine finished its work");
        });

        defer(() => {
            fmt.Println("main's deferred call ran; recover() =", recover());
        }, ref ᒐ);

        runtime.Goexit();
        fmt.Println("unreachable: after Goexit");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static partial void armB() {
    GoFrame ᒐ = default;
    try {
        var ready = new channel<EmptyStruct>(0);
        defer(ᴛ1 => close(ᴛ1), ready, ref ᒐ);

        var readyʗ1 = ready;
        goǃ(() => {
            ᐸꟷ(readyʗ1);
            fmt.Println("goroutine calls os.Exit(0)");
            Δos.Exit(0);
        });

        defer(() => fmt.Println("main's deferred call ran"), ref ᒐ);
        runtime.Goexit();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static partial void armC() {
    GoFrame ᒐ = default;
    try {
        var ready = new channel<EmptyStruct>(0);
        defer(ᴛ1 => close(ᴛ1), ready, ref ᒐ);

        var readyʗ1 = ready;
        goǃ(() => {
            ᐸꟷ(readyʗ1);
            throw panic("a goroutine panics after main's Goexit");
        });

        defer(() => fmt.Println("main's deferred call ran"), ref ᒐ);
        runtime.Goexit();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static partial void armD() {
    GoFrame ᒐ = default;
    try {
        var started = new channel<EmptyStruct>(0);

        var startedʗ1 = started;
        goǃ(() => {
            fmt.Println("goroutine blocks forever");
            close(startedʗ1);
            select();
        });

        ᐸꟷ(started);

        defer(() => fmt.Println("main's deferred call ran"), ref ᒐ);
        runtime.Goexit();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void armE() {
    GoFrame ᒐ = default;
    try {
        defer(() => fmt.Println("main's deferred call ran"), ref ᒐ);
        outer();
        fmt.Println("unreachable: outer returned");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void outer() {
    GoFrame ᒐ = default;
    try {
        defer(() => fmt.Println("outer's deferred call ran"), ref ᒐ);
        inner();
        fmt.Println("unreachable: inner returned");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void inner() {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            fmt.Println("inner's deferred call ran; recover() =", recover());
        }, ref ᒐ);
        runtime.Goexit();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void armF() {
    GoFrame ᒐ = default;
    try {
        defer(() => fmt.Println("main's deferred call ran"), ref ᒐ);

        var seq = (Func<nint, bool> yield) => {
            if (!yield(1)) {
                return;
            }
            runtime.Goexit();
        };

        var (next, stop) = iter.Pull(NilSafeDelegateConversion<iter.Seq<nint>, Action<Func<nint, bool>>>(seq));
        var stopʗ1 = stop;
        defer(stopʗ1, ref ᒐ);

        var (v, ok) = next();
        fmt.Println("first value:", v, ok);
        next();
        fmt.Println("unreachable: next returned after the seq's Goexit");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void parent(@string name) {
    var cmd = exec.Command(Δos.Args[0]);
    cmd.Env = append(Δos.Environ(), "GOEXIT_MAIN_ARM=" + name[..1]);
    ref var stdout = ref heap(new bytes.Buffer(), out var Ꮡstdout);
    ref var stderr = ref heap(new bytes.Buffer(), out var Ꮡstderr);
    cmd.Stdout = new bytes_BufferжWriter(Ꮡstdout);
    cmd.Stderr = new bytes_BufferжWriter(Ꮡstderr);
    var err = cmd.Run();

    nint code = 0;
    var (exit, ok) = err._<ж<exec.ExitError>>(ᐧ); if (ok) {
        code = exit.Value.ProcessState.ExitCode();
    } else if (err != nil) {
        code = -1;
    }

    fmt.Println("== arm", name);
    foreach (var (_, line) in strings.Split(strings.TrimRight(Ꮡstdout.String(), "\n"), "\n")) {
        if (line != ""u8) {
            fmt.Println("  stdout:", line);
        }
    }

    @string first = "";
    var scanner = bufio.NewScanner(new bytes_BufferжReader(Ꮡstderr)); if (scanner.Scan()) {
        first = scanner.Text();
    }
    fmt.Println("  stderr first line:", first);
    fmt.Println("  exit status:", code);
}

} // end main_package
