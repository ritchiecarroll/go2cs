namespace go;

using fmt = fmt_package;
using System.Runtime.CompilerServices;

partial class main_package {

internal static void expectPanic(@string name, Action f) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            fmt.Println(name, (@string)"->"u8, recover());
        }, ref ᒐ);
        f();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string senderCompletedWrongˢ = "sender completed (wrong)"u8;
private static readonly @string selectSendCompletedWrongˢ = "select send completed (wrong)"u8;
private static readonly @string closeOfClosedˢ = "close of closed"u8;
private static readonly @string closeOfNilˢ = "close of nil"u8;
private static readonly @string sendOnClosedˢ = "send on closed"u8;
private static readonly @string selectSendOnClosedWithˢ = "select send on closed with default"u8;
private static readonly object sentWrongˢ = (@string)"sent (wrong)"u8;
private static readonly object defaultWrongˢ = (@string)"default (wrong)"u8;

internal static partial void Main() {
    var ch = new channel<nint>(0);
    var res = new channel<@string>(3);
    for (nint i = 0; i < 3; i++) {
        var chʗ1 = ch;
        var resʗ1 = res;
        goǃ(() => {
            var (v, ok) = ᐸꟷ(chʗ1, ꟷ);
            resʗ1.ᐸꟷ(fmt.Sprintf("recv %d %t"u8, v, ok));
        });
    }
    close(ch);
    for (nint i = 0; i < 3; i++) {
        fmt.Println(ᐸꟷ(res));
    }
    var ch2 = new channel<nint>(0);
    var res2 = new channel<@string>(1);
    var ch2ʗ1 = ch2;
    var res2ʗ1 = res2;
    goǃ(() => {
        GoFrame ᒐ = default;
        try {
            var res2ʗ2 = res2ʗ1;
            defer(() => {
                {
                    var r = recover(); if (r != default!) {
                        res2ʗ2.ᐸꟷ(fmt.Sprintf("sender panicked: %v"u8, r));
                    }
                }
            }, ref ᒐ);
            ch2ʗ1.ᐸꟷ(1);
            res2ʗ1.ᐸꟷ(senderCompletedWrongˢ);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
    close(ch2);
    fmt.Println(ᐸꟷ(res2));
    var ch3 = new channel<nint>(0);
    var other = new channel<nint>(0);
    var res3 = new channel<@string>(1);
    var ch3ʗ1 = ch3;
    var otherʗ1 = other;
    var res3ʗ1 = res3;
    goǃ(() => {
        var selᴛ1 = ch3ʗ1;
        var selᴛ2 = otherʗ1;
        switch (select(ᐸꟷ(selᴛ1, ꓸꓸꓸ), ᐸꟷ(selᴛ2, ꓸꓸꓸ))) {
        case 0 when selᴛ1.ꟷᐳ(out var v, out var ok): {
            res3ʗ1.ᐸꟷ(fmt.Sprintf("select recv %d %t"u8, v, ok));
            break;
        }
        case 1 when selᴛ2.ꟷᐳ(out var v): {
            res3ʗ1.ᐸꟷ(fmt.Sprintf("other %d (wrong)"u8, v));
            break;
        }}
    });
    close(ch3);
    fmt.Println(ᐸꟷ(res3));
    var ch4 = new channel<nint>(0);
    var other2 = new channel<nint>(0);
    var res4 = new channel<@string>(1);
    var ch4ʗ1 = ch4;
    var other2ʗ1 = other2;
    var res4ʗ1 = res4;
    goǃ(() => {
        GoFrame ᒐ = default;
        try {
            var res4ʗ2 = res4ʗ1;
            defer(() => {
                {
                    var r = recover(); if (r != default!) {
                        res4ʗ2.ᐸꟷ(fmt.Sprintf("select send panicked: %v"u8, r));
                    }
                }
            }, ref ᒐ);
            var selᴛ3 = ch4ʗ1.ᐸꟷ(99, ꓸꓸꓸ);
            var selᴛ4 = other2ʗ1;
            switch (select(selᴛ3, ᐸꟷ(selᴛ4, ꓸꓸꓸ))) {
            case 0: {
                res4ʗ1.ᐸꟷ(selectSendCompletedWrongˢ);
                break;
            }
            case 1 when selᴛ4.ꟷᐳ(out var v): {
                res4ʗ1.ᐸꟷ(fmt.Sprintf("other %d (wrong)"u8, v));
                break;
            }}
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
    close(ch4);
    fmt.Println(ᐸꟷ(res4));
    expectPanic(closeOfClosedˢ, () => {
        var cc = new channel<nint>(0);
        close(cc);
        close(cc);
    });
    expectPanic(closeOfNilˢ, () => {
        channel<nint> nc = default!;
        close(nc);
    });
    expectPanic(sendOnClosedˢ, () => {
        var sc = new channel<nint>(1);
        close(sc);
        sc.ᐸꟷ(1);
    });
    expectPanic(selectSendOnClosedWithˢ, () => {
        var sd = new channel<nint>(1);
        close(sd);
        var selᴛ5 = sd.ᐸꟷ(1, ꓸꓸꓸ);
        switch (trySelect(selᴛ5)) {
        case 0: {
            fmt.Println(sentWrongˢ);
            break;
        }
        default: {
            fmt.Println(defaultWrongˢ);
            break;
        }}
    });
}

} // end main_package
