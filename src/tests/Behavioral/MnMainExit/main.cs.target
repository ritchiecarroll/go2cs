namespace go;

using bufio = bufio_package;
using bytes = bytes_package;
using fmt = fmt_package;
using Δos = os_package;
using exec = go.os.exec_package;
using runtime = runtime_package;
using strings = strings_package;
using atomic = sync.atomic_package;
using time = time_package;
using go.os;
using sync;
using Δio = io_package;

partial class main_package {


partial struct armsᴛ1 /*dyn*/ {
    internal @string name;
    internal bool header;
}
[GoLazy] partial class lazyᴛarms { internal static slice<armsᴛ1> arms = [
    new("a: main returns while two goroutines are parked", false),
    new("b: main calls Goexit, a goroutine finishes, then no goroutines", false),
    new("c: a goroutine calls os.Exit(0) after main's Goexit", false),
    new("d: a goroutine panics after main's Goexit", false),
    new("e: the Goexit is raised two parking frames below main", false),
    new("f: main and the goroutine left both block forever in select{}", false),
    new("g: main panics", true),
    new("h: main blocks forever in select{} with no goroutine left", false),
    new("i: init locked main to its thread, main parks and returns", false)
]; }

[GoInit] internal static void init() {
    if (Δos.Getenv("MN_MAIN_EXIT_ARM") == "i"u8) {
        runtime.LockOSThread();
    }
}

internal static void main() {
    var exprᴛ1 = Δos.Getenv("MN_MAIN_EXIT_ARM");
    switch (ᐧ) {
    case {} when exprᴛ1 == ""u8: {
        foreach (var (_, a) in range(arms)) {
            parent(a.name, a.header);
        }
        return;
    }
    case {} when exprᴛ1 == "a"u8: {
        armA();
        break;
    }
    case {} when exprᴛ1 == "b"u8: {
        armB();
        break;
    }
    case {} when exprᴛ1 == "c"u8: {
        armC();
        break;
    }
    case {} when exprᴛ1 == "d"u8: {
        armD();
        break;
    }
    case {} when exprᴛ1 == "e"u8: {
        armE();
        break;
    }
    case {} when exprᴛ1 == "f"u8: {
        armF();
        break;
    }
    case {} when exprᴛ1 == "g"u8: {
        armG();
        break;
    }
    case {} when exprᴛ1 == "h"u8: {
        armH();
        break;
    }
    case {} when exprᴛ1 == "i"u8: {
        armI();
        break;
    }}

    fmt.Println("main returned");
}

internal static void wake(channel<nint> c) {
    time.Sleep(20 * time.Millisecond);
    c.ᐸꟷ(1);
}

internal static void parked(channel<nint> started, channel<nint> never, nint id) {
    started.ᐸꟷ(id);
    ᐸꟷ(never);
}

internal static partial void armA() {
    var c = new channel<nint>(0);
    goǃ(wake, c);
    ᐸꟷ(c);

    var started = new channel<nint>(0);
    var never = new channel<nint>(0);
    goǃ((ᴛ1, ᴛ2) => parked(ᴛ1, ᴛ2, 1), started, never);
    goǃ((ᴛ1, ᴛ2) => parked(ᴛ1, ᴛ2, 2), started, never);
    fmt.Println("main received", ᐸꟷ(started) + ᐸꟷ(started));
}

internal static void finish(channel<EmptyStruct> ready) {
    ᐸꟷ(ready);
    fmt.Println("goroutine finished its work");
}

internal static partial void armB() {
    GoFrame ᒐ = default;
    try {
        var c = new channel<nint>(0);
        goǃ(wake, c);
        ᐸꟷ(c);

        var ready = new channel<EmptyStruct>(0);
        defer(ᴛ1 => close(ᴛ1), ready, ref ᒐ);
        goǃ(finish, ready);

        defer(() => {
            fmt.Println("main's deferred call ran; recover() =", recover());
        }, ref ᒐ);
        runtime.Goexit();
        fmt.Println("unreachable: after Goexit");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void exitZero(channel<EmptyStruct> ready) {
    ᐸꟷ(ready);
    fmt.Println("goroutine calls os.Exit(0)");
    Δos.Exit(0);
}

internal static partial void armC() {
    GoFrame ᒐ = default;
    try {
        var c = new channel<nint>(0);
        goǃ(wake, c);
        ᐸꟷ(c);

        var ready = new channel<EmptyStruct>(0);
        defer(ᴛ1 => close(ᴛ1), ready, ref ᒐ);
        goǃ(exitZero, ready);

        defer(() => fmt.Println("main's deferred call ran"), ref ᒐ);
        runtime.Goexit();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void panics(channel<EmptyStruct> ready) {
    ᐸꟷ(ready);
    throw panic("a goroutine panics after main's Goexit");
}

internal static partial void armD() {
    GoFrame ᒐ = default;
    try {
        var c = new channel<nint>(0);
        goǃ(wake, c);
        ᐸꟷ(c);

        var ready = new channel<EmptyStruct>(0);
        defer(ᴛ1 => close(ᴛ1), ready, ref ᒐ);
        goǃ(panics, ready);

        defer(() => fmt.Println("main's deferred call ran"), ref ᒐ);
        runtime.Goexit();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static partial void armE() {
    GoFrame ᒐ = default;
    try {
        defer(() => fmt.Println("main's deferred call ran"), ref ᒐ);
        var c = new channel<nint>(0);
        goǃ(wake, c);
        ᐸꟷ(c);
        outer();
        fmt.Println("unreachable: outer returned");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static partial void outer() {
    GoFrame ᒐ = default;
    try {
        defer(() => fmt.Println("outer's deferred call ran"), ref ᒐ);
        var c = new channel<nint>(0);
        goǃ(wake, c);
        ᐸꟷ(c);
        inner();
        fmt.Println("unreachable: inner returned");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static partial void inner() {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            fmt.Println("inner's deferred call ran; recover() =", recover());
        }, ref ᒐ);
        var c = new channel<nint>(0);
        goǃ(wake, c);
        ᐸꟷ(c);
        runtime.Goexit();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void forever(channel<EmptyStruct> started) {
    close(started);
    select();
}

internal static partial void armF() {
    var c = new channel<nint>(0);
    goǃ(wake, c);
    ᐸꟷ(c);

    var started = new channel<EmptyStruct>(0);
    goǃ(forever, started);
    ᐸꟷ(started);
    fmt.Println("main blocks forever beside a goroutine that does");
    select();
}

internal static partial void armG() {
    var c = new channel<nint>(0);
    goǃ(wake, c);
    ᐸꟷ(c);
    throw panic("main panics after it parked");
}

internal static partial void armH() {
    var c = new channel<nint>(0);
    goǃ(wake, c);
    ᐸꟷ(c);
    fmt.Println("main blocks forever alone");
    select();
}

internal static partial void armI() {
    var c = new channel<nint>(0);
    goǃ(wake, c);
    fmt.Println("main received", ᐸꟷ(c));
}

internal static void parent(@string name, bool header) {
    var cmd = exec.Command(Δos.Args[0]);
    cmd.Env = append(Δos.Environ(), "MN_MAIN_EXIT_ARM=" + name[..1]);
    ref var stdout = ref heap(new bytes.Buffer(), out var Ꮡstdout);
    ref var stderr = ref heap(new bytes.Buffer(), out var Ꮡstderr);
    cmd.Stdout = new bytes_BufferжWriter(Ꮡstdout);
    cmd.Stderr = new bytes_BufferжWriter(Ꮡstderr);

    ref var timedOut = ref heap(new atomic.Bool(), out var ᏑtimedOut);
    var err = cmd.Start();

    if (err == nil) {
        var cmdʗ1 = cmd;
        var timer = time.AfterFunc((time.Duration)(10000000000L), () => {
            ᏑtimedOut.Store(true);
            cmdʗ1.Process.Kill();
        });
        err = cmd.Wait();
        timer.Stop();
    }

    nint code = 0;
    var (exit, ok) = err._<ж<exec.ExitError>>(ᐧ); if (ok) {
        code = exit.Value.ProcessState.ExitCode();
    } else if (err != nil) {
        code = -1;
    }

    fmt.Println("== arm", name);
    foreach (var (_, line) in range(strings.Split(strings.TrimRight(Ꮡstdout.String(), "\n"), "\n"))) {
        if (line != ""u8) {
            fmt.Println("  stdout:", strings.TrimRight(line, "\r"));
        }
    }

    if (ᏑtimedOut.Load()) {
        fmt.Println("  TIMEOUT");
        return;
    }

    @string first = "";
    @string trace = "";
    var scanner = bufio.NewScanner(new bytes_BufferжReader(Ꮡstderr));
    while (scanner.Scan()) {
        @string line = scanner.Text();
        if (first == ""u8) {
            first = line;
        }
        if (strings.HasPrefix(line, "goroutine ") && strings.Contains(line, " [")) {
            trace = line;
            break;
        }
    }
    fmt.Println("  stderr first line:", first);
    if (header) {
        fmt.Println("  first goroutine header:", trace);
    }
    fmt.Println("  exit status:", code);
}

} // end main_package
