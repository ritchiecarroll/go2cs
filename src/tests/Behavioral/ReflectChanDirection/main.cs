namespace go;

using fmt = fmt_package;
using reflect = reflect_package;
using strings = strings_package;
using time = time_package;
using System.Runtime.CompilerServices;

partial class main_package {

partial struct holder {
    internal channel/*<-*/<@string> send = channel/*<-*/<@string>.SendOnly;
    internal /*<-*/channel<nint> recv = /*<-*/channel<nint>.RecvOnly;
    internal channel<bool> both;
    public channel/*<-*/<byte> Sent = channel/*<-*/<byte>.SendOnly;
}

partial struct sink /*chan @string*/;

internal static void describe(@string label, reflectꓸType t) {
    fmt.Printf("%s: %v | dir=%v | kind=%v\n"u8, label, t, t.ChanDir(), t.Kind());
}

internal static @string walkRangeShape(reflectꓸValue v) {
    var outᴷ = new stackAppend<@string>();

    if (v.Kind() != reflect.Chan) {
        return "not a channel";
    }

    if (v.Type().ChanDir() == reflect.SendDir) {
        return "range over send-only channel";
    }

    slice<@string> @out = default;

    while (ᐧ) {
        var (elem, ok) = v.Recv();
        if (!ok) {
            break;
        }
        @out = append(outᴷ, @out, fmt.Sprint(elem));
    }

    return "["u8 + strings.Join(@out, " ") + "]"u8;
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

internal static @string /*msg*/ recovered(Action fn) {
    @string msg = default;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var r = recover(); if (r != nil) {
                    msg = fmt.Sprint(r);
                }
            }
        }, ref ᒐ);
        fn();
        msg = "no panic";
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return msg;
}

internal static partial void Main() {
    var send = new channel/*<-*/<nint>(0, GoChanDir.Send);
    var recv = new /*<-*/channel<nint>(2, GoChanDir.Recv);
    var both = new channel<nint>(0);

    describe("make chan<-", reflect.TypeOf(send));
    describe("make <-chan", reflect.TypeOf(recv));
    describe("make chan", reflect.TypeOf(both));

    var ps = Ꮡ(channel/*<-*/<@string>.SendOnly);
    var pr = Ꮡ(/*<-*/channel<nint>.RecvOnly);

    fmt.Printf("new chan<-: %v -> %v | dir=%v\n"u8, reflect.TypeOf(ps.OrTypedNil()), reflect.TypeOf(ps.OrTypedNil()).Elem(), reflect.TypeOf(ps.OrTypedNil()).Elem().ChanDir());
    fmt.Printf("new <-chan: %v -> %v | dir=%v\n"u8, reflect.TypeOf(pr.OrTypedNil()), reflect.TypeOf(pr.OrTypedNil()).Elem(), reflect.TypeOf(pr.OrTypedNil()).Elem().ChanDir());
    fmt.Printf("elem of new: %v | %v\n"u8, reflect.ValueOf(ps.OrTypedNil()).Elem().Type(), reflect.ValueOf(pr.OrTypedNil()).Elem().Type());

    var ht = reflect.TypeOf(new holder());
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
    fmt.Println("chan->recv:", ci.AssignableTo(ri), "recv->chan:", ri.AssignableTo(ci), "chan->send:", ci.AssignableTo(si), "send->recv:", si.AssignableTo(ri));
    fmt.Println("identical:", AreEqual(ci, reflect.TypeOf(both)), "distinct:", !AreEqual(ci, ri), !AreEqual(ri, si));

    sink sk = new sink(0);
    describe("defined chan", reflect.TypeOf(sk));

    fmt.Println("range count(5):", walkRangeShape(reflect.ValueOf(count(5))));

    var done = new channel<@string>(1);
    var doneʗ1 = done;
    goǃ(() => {
        doneʗ1.ᐸꟷ(walkRangeShape(reflect.ValueOf(new channel/*<-*/<nint>(0, GoChanDir.Send))));
    });
    var selᴛ1 = done;
    var selᴛ2 = time.After((time.Duration)(5000000000L));
    switch (select(ᐸꟷ(selᴛ1, ꓸꓸꓸ), ᐸꟷ(selᴛ2, ꓸꓸꓸ))) {
    case 0 when selᴛ1.ꟷᐳ(out var got): {
        fmt.Println("range send-only:", got);
        break;
    }
    case 1 when selᴛ2.ꟷᐳ(out _): {
        fmt.Println("range send-only: HUNG -- the direction guard did not fire before Recv");
        break;
    }}

    var buffered = new channel<nint>(2);
    var bv = reflect.ValueOf(buffered);
    bv.Send(reflect.ValueOf((nint)41));
    bv.Send(reflect.ValueOf((nint)42));
    var (first, ok1) = bv.Recv();
    fmt.Println("send/recv:", first.Int(), ok1, "len:", bv.Len(), "cap:", bv.Cap());

    var (drained, ok2) = bv.TryRecv();
    var (empty, ok3) = bv.TryRecv();
    fmt.Println("tryrecv:", drained.Int(), ok2, "| empty valid:", empty.IsValid(), ok3);
    fmt.Println("trysend:", bv.TrySend(reflect.ValueOf((nint)7)));

    var closing = new channel<@string>(1);
    closing.ᐸꟷ("last");
    close(closing);
    var cv = reflect.ValueOf(closing);
    var (drainedVal, okDrain) = cv.Recv();
    var (zeroVal, okZero) = cv.Recv();
    fmt.Printf("closed: %q %v then %q %v\n"u8, drainedVal.String(), okDrain, zeroVal.String(), okZero);

    var sendʗ1 = send;
    fmt.Println("recv on send-only:", recovered(() => {
        reflect.ValueOf(sendʗ1).Recv();
    }));
    var recvʗ1 = recv;
    fmt.Println("send on recv-only:", recovered(() => {
        reflect.ValueOf(recvʗ1).Send(reflect.ValueOf((nint)1));
    }));
}

} // end main_package
