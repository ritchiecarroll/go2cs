namespace go;

using fmt = fmt_package;
using reflect = reflect_package;
using strings = strings_package;
using time = time_package;
using System.Runtime.CompilerServices;

partial class main_package {

[GoType] partial struct holder {
    internal channel/*<-*/<@string> send = channel/*<-*/<@string>.SendOnly;
    internal /*<-*/channel<nint> recv = /*<-*/channel<nint>.RecvOnly;
    internal channel<bool> both;
    public channel/*<-*/<byte> Sent = channel/*<-*/<byte>.SendOnly;
}

[GoType("chan @string")] partial struct sink;

internal static void describe(@string label, reflectꓸType t) {
    fmt.Printf("%s: %v | dir=%v | kind=%v\n"u8, label, t, t.ChanDir(), t.Kind());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string notAChannelˢ = "not a channel"u8;
private static readonly @string rangeOverSendOnlyChannelˢ = "range over send-only channel"u8;

internal static @string walkRangeShape(reflectꓸValue v) {
    if (v.Kind() != reflect.Chan) {
        return notAChannelˢ;
    }
    if (v.Type().ChanDir() == reflect.SendDir) {
        return rangeOverSendOnlyChannelˢ;
    }
    slice<@string> @out = default!;
    while (ᐧ) {
        var (elem, ok) = v.Recv();
        if (!ok) {
            break;
        }
        @out = append(@out, fmt.Sprint(elem));
    }
    return "["u8 + strings.Join(@out, " "u8) + "]"u8;
}

internal static partial channel<@string> count(nint n) {
    var ch = new channel<@string>(0);
    var chʗ1 = ch;
    goǃ(() => {
        for (nint i = 0; i < n; i++) {
            chʗ1.ᐸꟷ(fmt.Sprint(i));
        }
        close(chʗ1);
    });
    return ch;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string noPanicˢ = "no panic"u8;

internal static @string /*msg*/ recovered(Action fn) {
    @string msg = default!;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var r = recover(); if (r != default!) {
                    msg = fmt.Sprint(r);
                }
            }
        }, ref ᒐ);
        fn();
        msg = noPanicˢ;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return msg;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string makeChanˢ = "make chan<-"u8;
private static readonly @string makeChanˢ2 = "make <-chan"u8;
private static readonly @string makeChanˢ3 = "make chan"u8;
private static readonly object chanRecvˢ = (@string)"chan->recv:"u8;
private static readonly object recvChanˢ = (@string)"recv->chan:"u8;
private static readonly object chanSendˢ = (@string)"chan->send:"u8;
private static readonly object sendRecvˢ = (@string)"send->recv:"u8;
private static readonly object identicalˢ = (@string)"identical:"u8;
private static readonly object distinctˢ = (@string)"distinct:"u8;
private static readonly @string definedChanˢ = "defined chan"u8;
private static readonly object rangeCount5ˢ = (@string)"range count(5):"u8;
private static readonly object rangeSendOnlyˢ = (@string)"range send-only:"u8;
private static readonly object rangeSendOnlyHungTheˢ = (@string)"range send-only: HUNG -- the direction guard did not fire before Recv"u8;
private static readonly object sendRecvˢ2 = (@string)"send/recv:"u8;
private static readonly object lenˢ = (@string)"len:"u8;
private static readonly object capˢ = (@string)"cap:"u8;
private static readonly object tryrecvˢ = (@string)"tryrecv:"u8;
private static readonly object emptyValidˢ = (@string)"| empty valid:"u8;
private static readonly object trysendˢ = (@string)"trysend:"u8;
private static readonly @string lastˢ = "last"u8;
private static readonly object recvOnSendOnlyˢ = (@string)"recv on send-only:"u8;
private static readonly object sendOnRecvOnlyˢ = (@string)"send on recv-only:"u8;

internal static partial void Main() {
    var send = new channel/*<-*/<nint>(0, GoChanDir.Send);
    var recv = new /*<-*/channel<nint>(2, GoChanDir.Recv);
    var both = new channel<nint>(0);
    describe(makeChanˢ, reflect.TypeOf(send));
    describe(makeChanˢ2, reflect.TypeOf(recv));
    describe(makeChanˢ3, reflect.TypeOf(both));
    var ps = Ꮡ(channel/*<-*/<@string>.SendOnly);
    var pr = Ꮡ(/*<-*/channel<nint>.RecvOnly);
    fmt.Printf("new chan<-: %v -> %v | dir=%v\n"u8, reflect.TypeOf(ps.OrTypedNil()), reflect.TypeOf(ps.OrTypedNil()).Elem(), reflect.TypeOf(ps.OrTypedNil()).Elem().ChanDir());
    fmt.Printf("new <-chan: %v -> %v | dir=%v\n"u8, reflect.TypeOf(pr.OrTypedNil()), reflect.TypeOf(pr.OrTypedNil()).Elem(), reflect.TypeOf(pr.OrTypedNil()).Elem().ChanDir());
    fmt.Printf("elem of new: %v | %v\n"u8, reflect.ValueOf(ps.OrTypedNil()).Elem().Type(), reflect.ValueOf(pr.OrTypedNil()).Elem().Type());
    var ht = reflect.TypeOf(new holder(nil));
    for (nint i = 0; i < ht.NumField(); i++) {
        var f = ht.Field(i);
        fmt.Printf("field %s: %v | dir=%v\n"u8, f.Name, f.Type, f.Type.ChanDir());
    }
    var st = reflect.TypeOf(send);
    var rt = reflect.TypeOf(recv);
    fmt.Printf("zero: %T | %T\n"u8, reflect.Zero(st).Interface(), reflect.Zero(rt).Interface());
    fmt.Printf("new elem: %T | %v\n"u8, reflect.New(st).Elem().Interface(), reflect.New(rt).Type());
    var ci = reflect.TypeOf(@new<channel<nint>>()).Elem();
    var ri = reflect.TypeOf(Ꮡ(/*<-*/channel<nint>.RecvOnly)).Elem();
    var si = reflect.TypeOf(Ꮡ(channel/*<-*/<nint>.SendOnly)).Elem();
    fmt.Println(chanRecvˢ, ci.AssignableTo(ri), recvChanˢ, ri.AssignableTo(ci), chanSendˢ, ci.AssignableTo(si), sendRecvˢ, si.AssignableTo(ri));
    fmt.Println(identicalˢ, AreEqual(ci, reflect.TypeOf(both)), distinctˢ, !AreEqual(ci, ri), !AreEqual(ri, si));
    sink sk = new sink(0);
    describe(definedChanˢ, reflect.TypeOf(sk));
    fmt.Println(rangeCount5ˢ, walkRangeShape(reflect.ValueOf(count(5))));
    var done = new channel<@string>(1);
    var doneʗ1 = done;
    goǃ(() => {
        doneʗ1.ᐸꟷ(walkRangeShape(reflect.ValueOf(new channel/*<-*/<nint>(0, GoChanDir.Send))));
    });
    var selᴛ1 = done;
    var selᴛ2 = time.After((time.Duration)(5000000000L));
    switch (select(ᐸꟷ(selᴛ1, ꓸꓸꓸ), ᐸꟷ(selᴛ2, ꓸꓸꓸ))) {
    case 0 when selᴛ1.ꟷᐳ(out var got): {
        fmt.Println(rangeSendOnlyˢ, got);
        break;
    }
    case 1 when selᴛ2.ꟷᐳ(out _): {
        fmt.Println(rangeSendOnlyHungTheˢ);
        break;
    }}
    var buffered = new channel<nint>(2);
    var bv = reflect.ValueOf(buffered);
    bv.Send(reflect.ValueOf((nint)(41)));
    bv.Send(reflect.ValueOf((nint)(42)));
    var (first, ok1) = bv.Recv();
    fmt.Println(sendRecvˢ2, first.Int(), ok1, lenˢ, bv.Len(), capˢ, bv.Cap());
    var (drained, ok2) = bv.TryRecv();
    var (empty, ok3) = bv.TryRecv();
    fmt.Println(tryrecvˢ, drained.Int(), ok2, emptyValidˢ, empty.IsValid(), ok3);
    fmt.Println(trysendˢ, bv.TrySend(reflect.ValueOf((nint)(7))));
    var closing = new channel<@string>(1);
    closing.ᐸꟷ(lastˢ);
    close(closing);
    var cv = reflect.ValueOf(closing);
    var (drainedVal, okDrain) = cv.Recv();
    var (zeroVal, okZero) = cv.Recv();
    fmt.Printf("closed: %q %v then %q %v\n"u8, drainedVal.String(), okDrain, zeroVal.String(), okZero);
    var sendʗ1 = send;
    fmt.Println(recvOnSendOnlyˢ, recovered(() => {
        reflect.ValueOf(sendʗ1).Recv();
    }));
    var recvʗ1 = recv;
    fmt.Println(sendOnRecvOnlyˢ, recovered(() => {
        reflect.ValueOf(recvʗ1).Send(reflect.ValueOf((nint)(1)));
    }));
}

} // end main_package
