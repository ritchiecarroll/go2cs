namespace go;

using fmt = fmt_package;
using runtime = runtime_package;
using Δsync = sync_package;
using System.Runtime.CompilerServices;

partial class main_package {

partial struct T {
    internal nint n;
}

internal static partial void where(@string tag) {
    var (_, _, line, _) = runtime.Caller(1);
    fmt.Printf("%s: %d\n"u8, tag, line);
}

internal static partial void whereV(this T t, @string tag) {
    where2(tag);
}

internal static partial void whereP(this ref T t, @string tag) {
    where2(tag);
}

internal static partial void where2(@string tag) {
    var (_, _, line, _) = runtime.Caller(2);
    fmt.Printf("%s: %d\n"u8, tag, line);
}

internal static partial nint result(@string tag) {
    var (_, _, line, _) = runtime.Caller(1);
    fmt.Printf("%s: %d\n"u8, tag, line);
    return 1;
}

internal static void constArg() {
    GoFrame ᒐ = default;
    try {
        defer(() => where("defer, constant argument"), ref ᒐ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void varArg() {
    GoFrame ᒐ = default;
    try {
        @string s = "defer, variable argument";
        defer(where, s, ref ᒐ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void valueMethod() {
    GoFrame ᒐ = default;
    try {
        ref var t = ref heap<T>(new T(1), out var Ꮡt);
        var tʗ1 = t;
        defer((ᴛ0, ᴛ1) => ᴛ0.whereV(ᴛ1), tʗ1, (@string)"defer, value-receiver method", ref ᒐ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void pointerMethod() {
    GoFrame ᒐ = default;
    try {
        var t = Ꮡ(new T(2));
        @string s = "defer, pointer-receiver method";
        var tʗ1 = t;
        defer(tʗ1.whereP, s, ref ᒐ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void droppedResult() {
    GoFrame ᒐ = default;
    try {
        defer(() => result("defer, dropped result"), ref ᒐ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static nint endsInReturn() {
    GoFrame ᒐ = default;
    try {
        defer(() => where("defer, function ends in return"), ref ᒐ);
        nint x = 3;
        return x;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

internal static void funcLit() {
    GoFrame ᒐ = default;
    try {
        defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
            where("defer, func literal (control)");
        }, ref ᒐ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void oneLiner() {
    GoFrame ᒐ = default;
    try {
        defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
            where("defer, one-line func literal (control)");
        }, ref ᒐ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void shared() {
    GoFrame ᒐ = default;
    try {
        defer(() => where("defer, thunk sharing a line with a literal"), ref ᒐ);
        defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
            where("defer, literal sharing a line with a thunk (control)");
        }, ref ᒐ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static partial void spawned(ж<Δsync.WaitGroup> wg, @string tag) {
    GoFrame ᒐ = default;
    try {
        defer(wg.Done, ref ᒐ);
        var (pc, _, _, _) = runtime.Caller(1);
        fmt.Printf("%s: %s\n"u8, tag, runtime.FuncForPC(pc).Name());
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static partial void Main() {
    constArg();
    varArg();
    valueMethod();
    pointerMethod();
    droppedResult();
    endsInReturn();
    funcLit();
    oneLiner();
    shared();

    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
    Ꮡwg.Add(1);
    goǃ(ᴛ1 => spawned(ᴛ1, "go, constant argument: caller"), Ꮡwg);
    Ꮡwg.Wait();

    Ꮡwg.Add(1);
    goǃ(() => {
        GoFrame ᒐ = default;
        try {
            defer(Ꮡwg.Done, ref ᒐ);
            where("go, func literal (control)");
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
    Ꮡwg.Wait();
}

} // end main_package
