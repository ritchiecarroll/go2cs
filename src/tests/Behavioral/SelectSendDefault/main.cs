namespace go;

using fmt = fmt_package;
using time = time_package;
using System.Runtime.CompilerServices;

partial class main_package {

internal static void fullBuffered() {
    var ch = new channel<nint>(1);
    ch.ᐸꟷ(1);

    var selᴛ1 = ch.ᐸꟷ(2, ꓸꓸꓸ);
    switch (trySelect(selᴛ1)) {
    case 0: {
        fmt.Println("full buffered: sent");
        break;
    }
    default: {
        fmt.Println("full buffered: default");
        break;
    }}

    fmt.Println("full buffered: held =", ᐸꟷ(ch));

    var selᴛ2 = ch.ᐸꟷ(2, ꓸꓸꓸ);
    switch (trySelect(selᴛ2)) {
    case 0: {
        fmt.Println("drained buffered: sent");
        break;
    }
    default: {
        fmt.Println("drained buffered: default");
        break;
    }}

    fmt.Println("drained buffered: held =", ᐸꟷ(ch));
}

internal static void freeBuffered() {
    var ch = new channel<nint>(2);
    ch.ᐸꟷ(1);
    var selᴛ3 = ch.ᐸꟷ(2, ꓸꓸꓸ);
    switch (trySelect(selᴛ3)) {
    case 0: {
        fmt.Println("free buffered: sent");
        break;
    }
    default: {
        fmt.Println("free buffered: default");
        break;
    }}
    fmt.Println("free buffered: len =", len(ch));
    fmt.Println("free buffered: drained", ᐸꟷ(ch), ᐸꟷ(ch));
}

internal static partial void unbufferedWithReceiver() {
    var ch = new channel<nint>(0);
    var got = new channel<nint>(1);

    var chʗ1 = ch;
    var gotʗ1 = got;
    goǃ(() => {
        gotʗ1.ᐸꟷ(ᐸꟷ(chʗ1));
    }, GoStack.Bounded);

    var sent = false;

    for (nint i = 0; i < 1000 && !sent; i++) {
        var selᴛ4 = ch.ᐸꟷ(7, ꓸꓸꓸ);
        switch (trySelect(selᴛ4)) {
        case 0: {
            sent = true;
            break;
        }
        default: {
            time.Sleep(time.Millisecond);
            break;
        }}
    }

    fmt.Println("unbuffered with receiver: sent =", sent, "received =", ᐸꟷ(got));
}

internal static void nilChannel() {
    channel<nint> ch = default!;
    var selᴛ5 = ch.ᐸꟷ(1, ꓸꓸꓸ);
    switch (trySelect(selᴛ5)) {
    case 0: {
        fmt.Println("nil channel: sent");
        break;
    }
    default: {
        fmt.Println("nil channel: default");
        break;
    }}
}

internal static void closedChannel() {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var r = recover(); if (r != default!) {
                    fmt.Println("closed channel: recovered:", r);
                }
            }
        }, ref ᒐ);

        var ch = new channel<nint>(1);
        close(ch);

        var selᴛ6 = ch.ᐸꟷ(1, ꓸꓸꓸ);
        switch (trySelect(selᴛ6)) {
        case 0: {
            fmt.Println("closed channel: sent");
            break;
        }
        default: {
            fmt.Println("closed channel: default");
            break;
        }}

        fmt.Println("closed channel: NOT REACHED");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void oneOfManyReady() {
    var full = new channel<nint>(1);
    full.ᐸꟷ(1);
    var open = new channel<nint>(1);

    var selᴛ7 = full.ᐸꟷ(2, ꓸꓸꓸ);
    var selᴛ8 = open.ᐸꟷ(3, ꓸꓸꓸ);
    switch (trySelect(selᴛ7, selᴛ8)) {
    case 0: {
        fmt.Println("one of many: chose full");
        break;
    }
    case 1: {
        fmt.Println("one of many: chose open");
        break;
    }
    default: {
        fmt.Println("one of many: default");
        break;
    }}

    fmt.Println("one of many: full held", ᐸꟷ(full), "open got", ᐸꟷ(open));
}

internal static void neitherReady() {
    var full = new channel<nint>(1);
    full.ᐸꟷ(1);
    var empty = new channel<nint>(1);

    var selᴛ9 = full.ᐸꟷ(2, ꓸꓸꓸ);
    var selᴛ10 = empty;
    switch (trySelect(selᴛ9, ᐸꟷ(selᴛ10, ꓸꓸꓸ))) {
    case 0: {
        fmt.Println("neither ready: sent");
        break;
    }
    case 1 when selᴛ10.ꟷᐳ(out var v): {
        fmt.Println("neither ready: received", v);
        break;
    }
    default: {
        fmt.Println("neither ready: default");
        break;
    }}
}

internal static void exactlyOneSend() {
    var a = new channel<nint>(2);
    var b = new channel<nint>(2);

    var selᴛ11 = a.ᐸꟷ(1, ꓸꓸꓸ);
    var selᴛ12 = b.ᐸꟷ(2, ꓸꓸꓸ);
    switch (trySelect(selᴛ11, selᴛ12)) {
    case 0: {
        break;
    }
    case 1: {
        break;
    }
    default: {
        fmt.Println("exactly one send: default");
        break;
    }}

    fmt.Println("exactly one send: total =", len(a) + len(b));
}

internal static partial void blockingSendStillBlocks() {
    var ch = new channel<nint>(0);
    var got = new channel<nint>(1);

    var chʗ1 = ch;
    var gotʗ1 = got;
    goǃ(() => {
        time.Sleep(20 * time.Millisecond);
        gotʗ1.ᐸꟷ(ᐸꟷ(chʗ1));
    }, GoStack.Bounded);

    var selᴛ13 = ch.ᐸꟷ(42, ꓸꓸꓸ);
    switch (select(selᴛ13)) {
    case 0: {
        fmt.Println("blocking send: sent");
        break;
    }}

    fmt.Println("blocking send: received", ᐸꟷ(got));
}

partial struct queue /*chan nint*/;

internal static void namedChannelType() {
    var q = new queue(1);

    var selᴛ14 = q.ᐸꟷ(1, ꓸꓸꓸ);
    switch (trySelect(selᴛ14)) {
    case 0: {
        fmt.Println("named channel: sent");
        break;
    }
    default: {
        fmt.Println("named channel: default");
        break;
    }}

    var selᴛ15 = q.ᐸꟷ(2, ꓸꓸꓸ);
    switch (trySelect(selᴛ15)) {
    case 0: {
        fmt.Println("named channel full: sent");
        break;
    }
    default: {
        fmt.Println("named channel full: default");
        break;
    }}

    fmt.Println("named channel: held =", ᐸꟷ<nint>(q));
}

internal static void Main() {
    fullBuffered();
    freeBuffered();
    unbufferedWithReceiver();
    nilChannel();
    closedChannel();
    oneOfManyReady();
    neitherReady();
    exactlyOneSend();
    blockingSendStillBlocks();
    namedChannelType();
}

} // end main_package
