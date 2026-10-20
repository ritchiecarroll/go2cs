namespace go;

using fmt = fmt_package;
using reflect = reflect_package;

partial class main_package {

internal partial struct main_i /*dyn*/ {
    internal channel/*<-*/</*<-*/channel<nint>> x = channel/*<-*/</*<-*/channel<nint>>.Nil(ChanCargo.Of(new GoChanDir[] { GoChanDir.Send, GoChanDir.Recv }, null));
}

internal static void Main() {
    var intT = reflect.TypeOf((nint)0);

    var both = reflect.ChanOf(reflect.BothDir, intT);
    var recv = reflect.ChanOf(reflect.RecvDir, intT);
    var send = reflect.ChanOf(reflect.SendDir, intT);

    fmt.Println("1 chan chan int    :", reflect.ChanOf(reflect.BothDir, both).String());
    fmt.Println("2 chan<- chan int  :", reflect.ChanOf(reflect.SendDir, both).String());
    fmt.Println("3 chan (<-chan int):", reflect.ChanOf(reflect.BothDir, recv).String());
    fmt.Println("4 chan chan<- int  :", reflect.ChanOf(reflect.BothDir, send).String());
    fmt.Println("5 <-chan chan int  :", reflect.ChanOf(reflect.RecvDir, both).String());
    fmt.Println("6 chan<- <-chan int:", reflect.ChanOf(reflect.SendDir, recv).String());
    fmt.Println("7 <-chan <-chan int:", reflect.ChanOf(reflect.RecvDir, recv).String());

    fmt.Println("elem of chan (<-chan int):", reflect.ChanOf(reflect.BothDir, recv).Elem().String());
    fmt.Println("elem dir                 :", reflect.ChanOf(reflect.BothDir, recv).Elem().ChanDir());

    channel<nint> nilChan = default!;
    fmt.Println("identity ChanOf(Both,int) == TypeOf((chan int)(nil)):",
        AreEqual(reflect.ChanOf(reflect.BothDir, intT), reflect.TypeOf(nilChan)));

    channel<channel<nint>> nilNested = default!;
    fmt.Println("identity ChanOf(Both,chan int) == TypeOf((chan chan int)(nil)):",
        AreEqual(reflect.ChanOf(reflect.BothDir, both), reflect.TypeOf(nilNested)));

    channel/*<-*/<channel<nint>> vLeft = channel/*<-*/<channel<nint>>.SendOnly;
    channel</*<-*/channel<nint>> vRight = channel</*<-*/channel<nint>>.Nil(ChanCargo.Of(new GoChanDir[] { GoChanDir.Both, GoChanDir.Recv }, null));
    fmt.Println("value zero-var chan<- chan int   :", reflect.TypeOf(vLeft).String());
    fmt.Println("value zero-var chan (<-chan int) :", reflect.TypeOf(vRight).String());
    fmt.Println("value == constructed, left       :", AreEqual(reflect.TypeOf(vLeft), reflect.ChanOf(reflect.SendDir, both)));
    fmt.Println("value == constructed, right      :", AreEqual(reflect.TypeOf(vRight), reflect.ChanOf(reflect.BothDir, recv)));
    var mk = new channel<channel/*<-*/<nint>>(0, ChanCargo.Of(new GoChanDir[] { GoChanDir.Both, GoChanDir.Send }, null));
    fmt.Println("value make chan chan<- int       :", reflect.TypeOf(mk).String());
    var nc = channel</*<-*/channel<nint>>.Nil(ChanCargo.Of(new GoChanDir[] { GoChanDir.Both, GoChanDir.Recv }, null));
    fmt.Println("value nil-conv chan (<-chan int) :", reflect.TypeOf(nc).String());
    fmt.Println("value field chan<- <-chan int    :", reflect.TypeOf(new main_i()).Field(0).Type.String());
    var p = Ꮡ(channel</*<-*/channel<nint>>.Nil(ChanCargo.Of(new GoChanDir[] { GoChanDir.Both, GoChanDir.Recv }, null)));
    fmt.Println("value new(chan (<-chan int)).Elem:", reflect.TypeOf(p.OrTypedNil()).Elem().String());

    var ca = new channel<array<nint>>(0, ChanCargo.Of(null, new nint[] { 3 }));
    fmt.Println("value make chan [3]int           :", reflect.TypeOf(ca).String(), reflect.TypeOf(ca).Elem().Len());
    channel/*<-*/<array<array<byte>>> cb = channel/*<-*/<array<array<byte>>>.Nil(ChanCargo.Of(new GoChanDir[] { GoChanDir.Send }, new nint[] { 2, 4 }));
    fmt.Println("value zero-var chan<- [2][4]byte :", reflect.TypeOf(cb).String(), reflect.TypeOf(cb).Elem().Len(), reflect.TypeOf(cb).Elem().Elem().Len());
    var na = channel<array<nint>>.Nil(ChanCargo.Of(null, new nint[] { 5 }));
    fmt.Println("value nil-conv chan [5]int       :", reflect.TypeOf(na).String(), reflect.TypeOf(na).Elem().Len());

    var c3 = reflect.TypeOf(new channel<array<nint>>(0, ChanCargo.Of(null, new nint[] { 3 })));
    fmt.Println("assignable chan [3]int -> chan [4]int:", c3.AssignableTo(reflect.ChanOf(reflect.BothDir, reflect.ArrayOf(4, intT))));
    fmt.Println("assignable chan [3]int -> chan [3]int:", c3.AssignableTo(reflect.ChanOf(reflect.BothDir, reflect.ArrayOf(3, intT))));
    fmt.Println("convertible chan [3]int -> chan [4]int:", c3.ConvertibleTo(reflect.ChanOf(reflect.BothDir, reflect.ArrayOf(4, intT))));
}

} // end main_package
