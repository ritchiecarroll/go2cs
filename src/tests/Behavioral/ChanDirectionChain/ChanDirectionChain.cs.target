namespace go;

using fmt = fmt_package;
using reflect = reflect_package;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object chanChanIntˢ = (@string)"1 chan chan int    :"u8;
private static readonly object chanChanIntˢ2 = (@string)"2 chan<- chan int  :"u8;
private static readonly object chanChanIntˢ3 = (@string)"3 chan (<-chan int):"u8;
private static readonly object chanChanIntˢ4 = (@string)"4 chan chan<- int  :"u8;
private static readonly object chanChanIntˢ5 = (@string)"5 <-chan chan int  :"u8;
private static readonly object chanChanIntˢ6 = (@string)"6 chan<- <-chan int:"u8;
private static readonly object chanChanIntˢ7 = (@string)"7 <-chan <-chan int:"u8;
private static readonly object elemOfChanChanIntˢ = (@string)"elem of chan (<-chan int):"u8;
private static readonly object elemDirˢ = (@string)"elem dir                 :"u8;
private static readonly object identityChanOfBothIntˢ = (@string)"identity ChanOf(Both,int) == TypeOf((chan int)(nil)):"u8;
private static readonly object identityChanOfBothChanˢ = (@string)"identity ChanOf(Both,chan int) == TypeOf((chan chan int)(nil)):"u8;
private static readonly object valueZeroVarChanChanIntˢ = (@string)"value zero-var chan<- chan int   :"u8;
private static readonly object valueZeroVarChanChanIntˢ2 = (@string)"value zero-var chan (<-chan int) :"u8;
private static readonly object valueConstructedLeftˢ = (@string)"value == constructed, left       :"u8;
private static readonly object valueConstructedRightˢ = (@string)"value == constructed, right      :"u8;
private static readonly object valueMakeChanChanIntˢ = (@string)"value make chan chan<- int       :"u8;
private static readonly object valueNilConvChanChanIntˢ = (@string)"value nil-conv chan (<-chan int) :"u8;
private static readonly object valueFieldChanChanIntˢ = (@string)"value field chan<- <-chan int    :"u8;
private static readonly object valueNewChanChanIntElemˢ = (@string)"value new(chan (<-chan int)).Elem:"u8;
private static readonly object valueMakeChan3Intˢ = (@string)"value make chan [3]int           :"u8;
private static readonly object valueZeroVarChan24Byteˢ = (@string)"value zero-var chan<- [2][4]byte :"u8;
private static readonly object valueNilConvChan5Intˢ = (@string)"value nil-conv chan [5]int       :"u8;
private static readonly object assignableChan3IntChan4ˢ = (@string)"assignable chan [3]int -> chan [4]int:"u8;
private static readonly object assignableChan3IntChan3ˢ = (@string)"assignable chan [3]int -> chan [3]int:"u8;
private static readonly object convertibleChan3IntChan4ˢ = (@string)"convertible chan [3]int -> chan [4]int:"u8;

internal partial struct main_i /*dyn*/ {
    internal channel/*<-*/</*<-*/channel<nint>> x = channel/*<-*/</*<-*/channel<nint>>.Nil(ChanCargo.Of(new GoChanDir[] { GoChanDir.Send, GoChanDir.Recv }, null));
}

internal static void Main() {
    var intT = reflect.TypeOf((nint)(0));
    var both = reflect.ChanOf(reflect.BothDir, intT);
    var recv = reflect.ChanOf(reflect.RecvDir, intT);
    var send = reflect.ChanOf(reflect.SendDir, intT);
    fmt.Println(chanChanIntˢ, reflect.ChanOf(reflect.BothDir, both).String());
    fmt.Println(chanChanIntˢ2, reflect.ChanOf(reflect.SendDir, both).String());
    fmt.Println(chanChanIntˢ3, reflect.ChanOf(reflect.BothDir, recv).String());
    fmt.Println(chanChanIntˢ4, reflect.ChanOf(reflect.BothDir, send).String());
    fmt.Println(chanChanIntˢ5, reflect.ChanOf(reflect.RecvDir, both).String());
    fmt.Println(chanChanIntˢ6, reflect.ChanOf(reflect.SendDir, recv).String());
    fmt.Println(chanChanIntˢ7, reflect.ChanOf(reflect.RecvDir, recv).String());
    fmt.Println(elemOfChanChanIntˢ, reflect.ChanOf(reflect.BothDir, recv).Elem().String());
    fmt.Println(elemDirˢ, reflect.ChanOf(reflect.BothDir, recv).Elem().ChanDir());
    channel<nint> nilChan = default!;
    fmt.Println(identityChanOfBothIntˢ,
        AreEqual(reflect.ChanOf(reflect.BothDir, intT), reflect.TypeOf(nilChan)));
    channel<channel<nint>> nilNested = default!;
    fmt.Println(identityChanOfBothChanˢ,
        AreEqual(reflect.ChanOf(reflect.BothDir, both), reflect.TypeOf(nilNested)));
    channel/*<-*/<channel<nint>> vLeft = channel/*<-*/<channel<nint>>.SendOnly;
    channel</*<-*/channel<nint>> vRight = channel</*<-*/channel<nint>>.Nil(ChanCargo.Of(new GoChanDir[] { GoChanDir.Both, GoChanDir.Recv }, null));
    fmt.Println(valueZeroVarChanChanIntˢ, reflect.TypeOf(vLeft).String());
    fmt.Println(valueZeroVarChanChanIntˢ2, reflect.TypeOf(vRight).String());
    fmt.Println(valueConstructedLeftˢ, AreEqual(reflect.TypeOf(vLeft), reflect.ChanOf(reflect.SendDir, both)));
    fmt.Println(valueConstructedRightˢ, AreEqual(reflect.TypeOf(vRight), reflect.ChanOf(reflect.BothDir, recv)));
    var mk = new channel<channel/*<-*/<nint>>(0, ChanCargo.Of(new GoChanDir[] { GoChanDir.Both, GoChanDir.Send }, null));
    fmt.Println(valueMakeChanChanIntˢ, reflect.TypeOf(mk).String());
    var nc = channel</*<-*/channel<nint>>.Nil(ChanCargo.Of(new GoChanDir[] { GoChanDir.Both, GoChanDir.Recv }, null));
    fmt.Println(valueNilConvChanChanIntˢ, reflect.TypeOf(nc).String());
    fmt.Println(valueFieldChanChanIntˢ, reflect.TypeOf(new main_i()).Field(0).Type.String());
    var p = Ꮡ(channel</*<-*/channel<nint>>.Nil(ChanCargo.Of(new GoChanDir[] { GoChanDir.Both, GoChanDir.Recv }, null)));
    fmt.Println(valueNewChanChanIntElemˢ, reflect.TypeOf(p.OrTypedNil()).Elem().String());
    var ca = new channel<array<nint>>(0, ChanCargo.Of(null, new nint[] { 3 }));
    fmt.Println(valueMakeChan3Intˢ, reflect.TypeOf(ca).String(), reflect.TypeOf(ca).Elem().Len());
    channel/*<-*/<array<array<byte>>> cb = channel/*<-*/<array<array<byte>>>.Nil(ChanCargo.Of(new GoChanDir[] { GoChanDir.Send }, new nint[] { 2, 4 }));
    fmt.Println(valueZeroVarChan24Byteˢ, reflect.TypeOf(cb).String(), reflect.TypeOf(cb).Elem().Len(), reflect.TypeOf(cb).Elem().Elem().Len());
    var na = channel<array<nint>>.Nil(ChanCargo.Of(null, new nint[] { 5 }));
    fmt.Println(valueNilConvChan5Intˢ, reflect.TypeOf(na).String(), reflect.TypeOf(na).Elem().Len());
    var c3 = reflect.TypeOf(new channel<array<nint>>(0, ChanCargo.Of(null, new nint[] { 3 })));
    fmt.Println(assignableChan3IntChan4ˢ, c3.AssignableTo(reflect.ChanOf(reflect.BothDir, reflect.ArrayOf(4, intT))));
    fmt.Println(assignableChan3IntChan3ˢ, c3.AssignableTo(reflect.ChanOf(reflect.BothDir, reflect.ArrayOf(3, intT))));
    fmt.Println(convertibleChan3IntChan4ˢ, c3.ConvertibleTo(reflect.ChanOf(reflect.BothDir, reflect.ArrayOf(4, intT))));
}

} // end main_package
