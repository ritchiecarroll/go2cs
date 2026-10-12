namespace go;

using fmt = fmt_package;
using reflect = reflect_package;

partial class main_package {

internal static reflectꓸChanDir dirOf(any v) {
    return reflect.TypeOf(v).ChanDir();
}

internal static reflectꓸChanDir takesRecv(/*<-*/channel<nint> c) {
    return dirOf(c);
}

internal static reflectꓸChanDir takesSend(channel/*<-*/<nint> c) {
    return dirOf(c);
}

internal static /*<-*/channel<nint> returnsRecv() {
    return new channel<nint>(0).WithDirection(GoChanDir.Recv);
}

internal static channel/*<-*/<nint> returnsSend() {
    return new channel<nint>(0).WithDirection(GoChanDir.Send);
}

internal static channel<nint> returnsBoth() {
    return new channel<nint>(0);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object varInitRecvˢ = (@string)"var-init recv:"u8;
private static readonly object sendˢ = (@string)"| send:"u8;
private static readonly object assignRecvˢ = (@string)"assign recv:"u8;
private static readonly object argRecvˢ = (@string)"arg recv:"u8;
private static readonly object returnRecvˢ = (@string)"return recv:"u8;
private static readonly object controlBidiVarˢ = (@string)"control bidi var:"u8;
private static readonly object bidiReturnˢ = (@string)"| bidi return:"u8;
private static readonly object controlRecvRecvˢ = (@string)"control recv<-recv:"u8;
private static readonly object identitySameChannelˢ = (@string)"identity: same channel ->"u8;
private static readonly object lenNowˢ = (@string)"| len now:"u8;
private static readonly object recvChanAssignableˢ = (@string)"recv->chan assignable:"u8;
private static readonly object chanRecvAssignableˢ = (@string)"| chan->recv assignable:"u8;

internal static void Main() {
    /*<-*/channel<nint> vr = new channel<nint>(0).WithDirection(GoChanDir.Recv);
    channel/*<-*/<nint> vs = new channel<nint>(0).WithDirection(GoChanDir.Send);
    fmt.Println(varInitRecvˢ, dirOf(vr), sendˢ, dirOf(vs));

    /*<-*/channel<nint> ar = /*<-*/channel<nint>.RecvOnly;
    channel/*<-*/<nint> @as = channel/*<-*/<nint>.SendOnly;
    var bidi = new channel<nint>(0);
    ar = bidi.WithDirection(GoChanDir.Recv);
    @as = bidi.WithDirection(GoChanDir.Send);
    fmt.Println(assignRecvˢ, dirOf(ar), sendˢ, dirOf(@as));

    fmt.Println(argRecvˢ, takesRecv(new channel<nint>(0).WithDirection(GoChanDir.Recv)), sendˢ, takesSend(new channel<nint>(0).WithDirection(GoChanDir.Send)));

    fmt.Println(returnRecvˢ, dirOf(returnsRecv()), sendˢ, dirOf(returnsSend()));

    channel<nint> cb = new channel<nint>(0);
    fmt.Println(controlBidiVarˢ, dirOf(cb), bidiReturnˢ, dirOf(returnsBoth()));

    /*<-*/channel<nint> again = vr;
    fmt.Println(controlRecvRecvˢ, dirOf(again));

    var src = new channel<nint>(1);
    /*<-*/channel<nint> narrowed = src.WithDirection(GoChanDir.Recv);
    src.ᐸꟷ(99);
    var (got, ok) = reflect.ValueOf(narrowed).Recv();
    fmt.Println(identitySameChannelˢ, got.Int(), ok, lenNowˢ, reflect.ValueOf(src).Len());

    var recvT = reflect.TypeOf(vr);
    var bidiT = reflect.TypeOf(cb);
    fmt.Println(recvChanAssignableˢ, recvT.AssignableTo(bidiT), chanRecvAssignableˢ, bidiT.AssignableTo(recvT));
}

} // end main_package
