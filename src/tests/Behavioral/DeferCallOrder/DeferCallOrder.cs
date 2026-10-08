namespace go;

using fmt = fmt_package;
using System.Runtime.CompilerServices;
using ꓸꓸꓸnint = Span<nint>;

partial class main_package {

// type stopFn is a methodless func type — rendered inline as its base delegate

internal static Action makeStop(@string tag, channel/*<-*/<@string> @out) {
    var outʗ1 = @out;
    return () => {
        outʗ1.ᐸꟷ(tag);
    };
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object firstˢ = (@string)"First"u8;
private static readonly object secondˢ = (@string)"Second"u8;
private static readonly object thirdˢ = (@string)"Third"u8;
private static readonly object fourthˢ = (@string)"Fourth"u8;
private static readonly @string stoppedˢ = "stopped"u8;
private static readonly @string goStoppedˢ = "go-stopped"u8;
private static readonly object afterCloseˢ = (@string)"after close:"u8;
private static readonly @string fifthˢ = "Fifth"u8;
private static readonly object afterˢ = (@string)"after:"u8;
private static readonly object sentˢ = (@string)"sent:"u8;
private static readonly object heldˢ = (@string)"| held:"u8;
private static readonly object notifyˢ = (@string)"notify:"u8;
private static readonly object mainFunctionˢ = (@string)"Main function"u8;

internal static partial void Main() {
    GoFrame ᒐ = default;
    try {
        defer(ᴛ1 => fmt.Println(ᴛ1), firstˢ, ref ᒐ);
        defer(ᴛ1 => fmt.Println(ᴛ1), secondˢ, ref ᒐ);
        defer(ᴛ1 => fmt.Println(ᴛ1), thirdˢ, ref ᒐ);
        Funcꓸꓸꓸ<any, (nint, error)> f1 = fmt.Println;
        var f1ʗ1 = f1;
        defer(ᴛ1 => f1ʗ1(ᴛ1), fourthˢ, ref ᒐ);
        var msgs = new channel<@string>(2);
        var cancel = makeStop(stoppedˢ, msgs.WithDirection(GoChanDir.Send));
        var cancelʗ1 = cancel;
        defer(() => cancelʗ1(), ref ᒐ);
        var msgsʗ1 = msgs;
        goǃ(() => makeStop(goStoppedˢ, msgsʗ1.WithDirection(GoChanDir.Send))());
        fmt.Println(ᐸꟷ(msgs));
        var drained = new channel<nint>(1);
        var drainedʗ1 = drained;
        defer(() => {
            var (v, open) = ᐸꟷ(drainedʗ1, ꟷ);
            fmt.Println(afterCloseˢ, v, open);
        }, ref ᒐ);
        defer(ᴛ1 => close(ᴛ1), drained, ref ᒐ);
        defer(GetPrintLn(), fifthˢ, ref ᒐ);
        var c = Ꮡ(new acc(nil));
        var (s1, e1) = c.add(5);
        fmt.Println(s1, e1);
        var (s2, e2) = c.add(-1);
        fmt.Println(s2, e2, (~c).total);
        var sm = Ꮡ(new sema(nil));
        acquireAndWork(sm);
        fmt.Println(afterˢ, (~sm).held);
        watchAndSend(sm);
        fmt.Println(sentˢ, ᐸꟷ((~sm).@out), heldˢ, (~sm).held);
        fmt.Println(notifyˢ, notifyAll(1, 2, 3));
        fmt.Println(mainFunctionˢ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

partial struct sema {
    internal bool held;
    internal channel<nint> @out;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object semaReleasedˢ = (@string)"sema released"u8;

internal static void release(this ref sema s) {
    s.held = false;
    fmt.Println(semaReleasedˢ);
}

internal static void send(this ref sema s, nint n) {
    s.@out.ᐸꟷ(n);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object sentFirstˢ = (@string)"sent-first:"u8;
private static readonly object watchingHeldˢ = (@string)"watching, held:"u8;

internal static partial void watchAndSend(ж<sema> Ꮡs) {
    GoFrame ᒐ = default;
    try {
        ref var s = ref Ꮡs.DerefOrNull();

        s.@out = new channel<nint>(2);
        s.held = true;
        goǃ(Ꮡs.send, (nint)(7));
        fmt.Println(sentFirstˢ, ᐸꟷ(s.@out));
        defer(Ꮡs.send, (nint)(9), ref ᒐ);
        defer(Ꮡs.release, ref ᒐ);
        fmt.Println(watchingHeldˢ, s.held);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object workingHeldˢ = (@string)"working, held:"u8;

internal static void acquireAndWork(ж<sema> Ꮡs) {
    GoFrame ᒐ = default;
    try {
        ref var s = ref Ꮡs.DerefOrNull();

        s.held = true;
        defer(Ꮡs.release, ref ᒐ);
        fmt.Println(workingHeldˢ, s.held);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object notifiedˢ = (@string)"notified"u8;

internal static nint notifyAll(params ꓸꓸꓸnint valsʗp) {
    GoFrame ᒐ = default;
    try {
        var vals = valsʗp.sslice();

        defer(ᴛ1 => fmt.Println(ᴛ1), notifiedˢ, ref ᒐ);
        nint total = 0;
        foreach (var (_, v) in vals) {
            total += v;
        }
        return total;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

partial struct acc {
    internal nint total;
}

internal static (nint sum, error err) add(this ж<acc> Ꮡa, nint n) {
    nint sum = default!;
    error err = default!;
    GoFrame ᒐ = default;
    try {
        ref var a = ref Ꮡa.DerefOrNull();

        defer(() => {
            {
                var e = recover(); if (e != default!) {
                    err = fmt.Errorf("boom"u8);
                }
            }
        }, ref ᒐ);
        a.total += n;
        if (n < 0) {
            throw panic("negative");
        }
        sum = a.total;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return (sum, err);
}

public static Action<@string> GetPrintLn() {
    return (@string src) => {
        fmt.Println(src);
    };
}

} // end main_package
