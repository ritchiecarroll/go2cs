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

internal static void Main() {
    /*<-*/channel<nint> vr = new channel<nint>(0).WithDirection(GoChanDir.Recv);
    channel/*<-*/<nint> vs = new channel<nint>(0).WithDirection(GoChanDir.Send);
    fmt.Println("var-init recv:", dirOf(vr), "| send:", dirOf(vs));

    /*<-*/channel<nint> ar = /*<-*/channel<nint>.RecvOnly;
    channel/*<-*/<nint> @as = channel/*<-*/<nint>.SendOnly;
    var bidi = new channel<nint>(0);
    ar = bidi.WithDirection(GoChanDir.Recv);
    @as = bidi.WithDirection(GoChanDir.Send);
    fmt.Println("assign recv:", dirOf(ar), "| send:", dirOf(@as));

    fmt.Println("arg recv:", takesRecv(new channel<nint>(0).WithDirection(GoChanDir.Recv)), "| send:", takesSend(new channel<nint>(0).WithDirection(GoChanDir.Send)));

    fmt.Println("return recv:", dirOf(returnsRecv()), "| send:", dirOf(returnsSend()));

    channel<nint> cb = new channel<nint>(0);
    fmt.Println("control bidi var:", dirOf(cb), "| bidi return:", dirOf(returnsBoth()));

    /*<-*/channel<nint> again = vr;
    fmt.Println("control recv<-recv:", dirOf(again));

    var src = new channel<nint>(1);
    /*<-*/channel<nint> narrowed = src.WithDirection(GoChanDir.Recv);
    src.ᐸꟷ(99);
    var (got, ok) = reflect.ValueOf(narrowed).Recv();
    fmt.Println("identity: same channel ->", got.Int(), ok, "| len now:", reflect.ValueOf(src).Len());

    var recvT = reflect.TypeOf(vr);
    var bidiT = reflect.TypeOf(cb);
    fmt.Println("recv->chan assignable:", recvT.AssignableTo(bidiT), "| chan->recv assignable:", bidiT.AssignableTo(recvT));
}

} // end main_package
