namespace go;

using fmt = fmt_package;
using time = time_package;
using System.Runtime.CompilerServices;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object fullBufferedSentˢ = (@string)"full buffered: sent"u8;
private static readonly object fullBufferedDefaultˢ = (@string)"full buffered: default"u8;
private static readonly object fullBufferedHeldˢ = (@string)"full buffered: held ="u8;
private static readonly object drainedBufferedSentˢ = (@string)"drained buffered: sent"u8;
private static readonly object drainedBufferedDefaultˢ = (@string)"drained buffered: default"u8;
private static readonly object drainedBufferedHeldˢ = (@string)"drained buffered: held ="u8;

internal static void fullBuffered() {
    var ch = new channel<nint>(1);
    ch.ᐸꟷ(1);
    var selᴛ1 = ch.ᐸꟷ(2, ꓸꓸꓸ);
    switch (trySelect(selᴛ1)) {
    case 0: {
        fmt.Println(fullBufferedSentˢ);
        break;
    }
    default: {
        fmt.Println(fullBufferedDefaultˢ);
        break;
    }}
    fmt.Println(fullBufferedHeldˢ, ᐸꟷ(ch));
    var selᴛ2 = ch.ᐸꟷ(2, ꓸꓸꓸ);
    switch (trySelect(selᴛ2)) {
    case 0: {
        fmt.Println(drainedBufferedSentˢ);
        break;
    }
    default: {
        fmt.Println(drainedBufferedDefaultˢ);
        break;
    }}
    fmt.Println(drainedBufferedHeldˢ, ᐸꟷ(ch));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object freeBufferedSentˢ = (@string)"free buffered: sent"u8;
private static readonly object freeBufferedDefaultˢ = (@string)"free buffered: default"u8;
private static readonly object freeBufferedLenˢ = (@string)"free buffered: len ="u8;
private static readonly object freeBufferedDrainedˢ = (@string)"free buffered: drained"u8;

internal static void freeBuffered() {
    var ch = new channel<nint>(2);
    ch.ᐸꟷ(1);
    var selᴛ3 = ch.ᐸꟷ(2, ꓸꓸꓸ);
    switch (trySelect(selᴛ3)) {
    case 0: {
        fmt.Println(freeBufferedSentˢ);
        break;
    }
    default: {
        fmt.Println(freeBufferedDefaultˢ);
        break;
    }}
    fmt.Println(freeBufferedLenˢ, len(ch));
    fmt.Println(freeBufferedDrainedˢ, ᐸꟷ(ch), ᐸꟷ(ch));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object unbufferedWithReceiverˢ = (@string)"unbuffered with receiver: sent ="u8;
private static readonly object receivedˢ = (@string)"received ="u8;

[MethodImpl(MethodImplOptions.NoInlining)] internal static void unbufferedWithReceiver() {
    var ch = new channel<nint>(0);
    var got = new channel<nint>(1);
    var chʗ1 = ch;
    var gotʗ1 = got;
    goǃ(() => {
        gotʗ1.ᐸꟷ(ᐸꟷ(chʗ1));
    });
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
    fmt.Println(unbufferedWithReceiverˢ, sent, receivedˢ, ᐸꟷ(got));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object nilChannelSentˢ = (@string)"nil channel: sent"u8;
private static readonly object nilChannelDefaultˢ = (@string)"nil channel: default"u8;

internal static void nilChannel() {
    channel<nint> ch = default!;
    var selᴛ5 = ch.ᐸꟷ(1, ꓸꓸꓸ);
    switch (trySelect(selᴛ5)) {
    case 0: {
        fmt.Println(nilChannelSentˢ);
        break;
    }
    default: {
        fmt.Println(nilChannelDefaultˢ);
        break;
    }}
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object closedChannelRecoveredˢ = (@string)"closed channel: recovered:"u8;
private static readonly object closedChannelSentˢ = (@string)"closed channel: sent"u8;
private static readonly object closedChannelDefaultˢ = (@string)"closed channel: default"u8;
private static readonly object closedChannelNotReachedˢ = (@string)"closed channel: NOT REACHED"u8;

internal static void closedChannel() {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var r = recover(); if (r != default!) {
                    fmt.Println(closedChannelRecoveredˢ, r);
                }
            }
        }, ref ᒐ);
        var ch = new channel<nint>(1);
        close(ch);
        var selᴛ6 = ch.ᐸꟷ(1, ꓸꓸꓸ);
        switch (trySelect(selᴛ6)) {
        case 0: {
            fmt.Println(closedChannelSentˢ);
            break;
        }
        default: {
            fmt.Println(closedChannelDefaultˢ);
            break;
        }}
        fmt.Println(closedChannelNotReachedˢ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object oneOfManyChoseFullˢ = (@string)"one of many: chose full"u8;
private static readonly object oneOfManyChoseOpenˢ = (@string)"one of many: chose open"u8;
private static readonly object oneOfManyDefaultˢ = (@string)"one of many: default"u8;
private static readonly object oneOfManyFullHeldˢ = (@string)"one of many: full held"u8;
private static readonly object openGotˢ = (@string)"open got"u8;

internal static void oneOfManyReady() {
    var full = new channel<nint>(1);
    full.ᐸꟷ(1);
    var open = new channel<nint>(1);
    var selᴛ7 = full.ᐸꟷ(2, ꓸꓸꓸ);
    var selᴛ8 = open.ᐸꟷ(3, ꓸꓸꓸ);
    switch (trySelect(selᴛ7, selᴛ8)) {
    case 0: {
        fmt.Println(oneOfManyChoseFullˢ);
        break;
    }
    case 1: {
        fmt.Println(oneOfManyChoseOpenˢ);
        break;
    }
    default: {
        fmt.Println(oneOfManyDefaultˢ);
        break;
    }}
    fmt.Println(oneOfManyFullHeldˢ, ᐸꟷ(full), openGotˢ, ᐸꟷ(open));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object neitherReadySentˢ = (@string)"neither ready: sent"u8;
private static readonly object neitherReadyReceivedˢ = (@string)"neither ready: received"u8;
private static readonly object neitherReadyDefaultˢ = (@string)"neither ready: default"u8;

internal static void neitherReady() {
    var full = new channel<nint>(1);
    full.ᐸꟷ(1);
    var empty = new channel<nint>(1);
    var selᴛ9 = full.ᐸꟷ(2, ꓸꓸꓸ);
    var selᴛ10 = empty;
    switch (trySelect(selᴛ9, ᐸꟷ(selᴛ10, ꓸꓸꓸ))) {
    case 0: {
        fmt.Println(neitherReadySentˢ);
        break;
    }
    case 1 when selᴛ10.ꟷᐳ(out var v): {
        fmt.Println(neitherReadyReceivedˢ, v);
        break;
    }
    default: {
        fmt.Println(neitherReadyDefaultˢ);
        break;
    }}
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object exactlyOneSendDefaultˢ = (@string)"exactly one send: default"u8;
private static readonly object exactlyOneSendTotalˢ = (@string)"exactly one send: total ="u8;

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
        fmt.Println(exactlyOneSendDefaultˢ);
        break;
    }}
    fmt.Println(exactlyOneSendTotalˢ, len(a) + len(b));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object blockingSendSentˢ = (@string)"blocking send: sent"u8;
private static readonly object blockingSendReceivedˢ = (@string)"blocking send: received"u8;

[MethodImpl(MethodImplOptions.NoInlining)] internal static void blockingSendStillBlocks() {
    var ch = new channel<nint>(0);
    var got = new channel<nint>(1);
    var chʗ1 = ch;
    var gotʗ1 = got;
    goǃ(() => {
        time.Sleep(20 * time.Millisecond);
        gotʗ1.ᐸꟷ(ᐸꟷ(chʗ1));
    });
    var selᴛ13 = ch.ᐸꟷ(42, ꓸꓸꓸ);
    switch (select(selᴛ13)) {
    case 0: {
        fmt.Println(blockingSendSentˢ);
        break;
    }}
    fmt.Println(blockingSendReceivedˢ, ᐸꟷ(got));
}

[GoType("chan nint")] partial struct queue;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object namedChannelSentˢ = (@string)"named channel: sent"u8;
private static readonly object namedChannelDefaultˢ = (@string)"named channel: default"u8;
private static readonly object namedChannelFullSentˢ = (@string)"named channel full: sent"u8;
private static readonly object namedChannelFullDefaultˢ = (@string)"named channel full: default"u8;
private static readonly object namedChannelHeldˢ = (@string)"named channel: held ="u8;

internal static void namedChannelType() {
    var q = new queue(1);
    var selᴛ14 = q.ᐸꟷ(1, ꓸꓸꓸ);
    switch (trySelect(selᴛ14)) {
    case 0: {
        fmt.Println(namedChannelSentˢ);
        break;
    }
    default: {
        fmt.Println(namedChannelDefaultˢ);
        break;
    }}
    var selᴛ15 = q.ᐸꟷ(2, ꓸꓸꓸ);
    switch (trySelect(selᴛ15)) {
    case 0: {
        fmt.Println(namedChannelFullSentˢ);
        break;
    }
    default: {
        fmt.Println(namedChannelFullDefaultˢ);
        break;
    }}
    fmt.Println(namedChannelHeldˢ, ᐸꟷ<nint>(q));
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
