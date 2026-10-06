namespace go;

using context = context_package;
using fmt = fmt_package;
using reflect = reflect_package;
using time = time_package;

partial class main_package {

[GoType] partial struct R {
}

public static bool Equal(this R _, /*<-*/channel<bool> y) {
    return true;
}

[GoType] partial struct S {
    internal nint n;
}

[GoRecv] public static void Feed(this ref S s, channel/*<-*/<nint> c) {
    s.n++;
}

[GoType] partial interface Notifier {
    /*<-*/channel<EmptyStruct> Done();
}

[GoType("chan bool")] [GoChanDir(GoChanDir.Recv)] partial struct AssignD;

public static bool Equal(this AssignD x, /*<-*/channel<bool> y) {
    return true;
}

internal static void recvLocal(/*<-*/channel<nint> c) {
}

internal static void bidiLocal(channel<nint> c) {
}

internal static void sendOnly(channel/*<-*/<@string> c) {
}

internal static (/*<-*/channel<nint>, error) two() {
    return (default!, default!);
}

internal static /*<-*/channel<bool> mixed(nint a, channel/*<-*/<nint> c) {
    return default!;
}

internal static void nestedDir(channel/*<-*/</*<-*/channel<nint>> c) {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object funcValueˢ = (@string)"func value:"u8;
private static readonly object recvParamˢ = (@string)"recv param:"u8;
private static readonly object sendParamˢ = (@string)"send param:"u8;
private static readonly object resultsˢ = (@string)"results:"u8;
private static readonly @string doneˢ = "Done"u8;
private static readonly object interfaceMethodˢ = (@string)"interface method:"u8;
private static readonly object ownInterfaceMethodˢ = (@string)"own interface method:"u8;
private static readonly @string equalˢ = "Equal"u8;
private static readonly object valueMethodˢ = (@string)"value method:"u8;
private static readonly @string feedˢ = "Feed"u8;
private static readonly object pointerMethodˢ = (@string)"pointer method:"u8;
private static readonly object identicalˢ = (@string)"identical:"u8;
private static readonly object assignableˢ = (@string)"assignable:"u8;
private static readonly object convertibleˢ = (@string)"convertible:"u8;
private static readonly object namedRecvToParamˢ = (@string)"named recv to param:"u8;
private static readonly object makefuncˢ = (@string)"makefunc:"u8;

internal static void Main() {
    Func<time.Duration, /*<-*/channel<time.Time>> after = time.After;
    fmt.Println(funcValueˢ, reflect.TypeOf((after).OrTypedNilFunc()), reflect.TypeOf((after).OrTypedNilFunc()).Out(0).ChanDir());
    fmt.Println(recvParamˢ, reflect.TypeOf(recvLocal), reflect.TypeOf(recvLocal).In(0).ChanDir());
    fmt.Println(sendParamˢ, reflect.TypeOf(sendOnly), reflect.TypeOf(sendOnly).In(0).ChanDir());
    fmt.Println(resultsˢ, reflect.TypeOf(two), (@string)"|"u8, reflect.TypeOf(mixed));
    var (done, _) = reflect.TypeOf(((ж<context.Context>)nil)).Elem().MethodByName(doneˢ);
    fmt.Println(interfaceMethodˢ, done.Type, done.Type.Out(0).ChanDir());
    var (nd, _) = reflect.TypeOf(((ж<Notifier>)nil)).Elem().MethodByName(doneˢ);
    fmt.Println(ownInterfaceMethodˢ, nd.Type, nd.Type.Out(0).ChanDir());
    var (eq, _) = reflect.TypeOf(new R(nil)).MethodByName(equalˢ);
    fmt.Println(valueMethodˢ, eq.Type, eq.Type.In(1).ChanDir());
    var (feed, _) = reflect.TypeOf(Ꮡ(new S(nil))).MethodByName(feedˢ);
    fmt.Println(pointerMethodˢ, feed.Type, feed.Type.In(1).ChanDir());
    fmt.Println(identicalˢ, AreEqual(reflect.TypeOf(recvLocal), reflect.TypeOf(bidiLocal)));
    fmt.Println(assignableˢ, reflect.TypeOf(bidiLocal).AssignableTo(reflect.TypeOf(recvLocal)));
    fmt.Println(convertibleˢ, reflect.TypeOf(bidiLocal).ConvertibleTo(reflect.TypeOf(recvLocal)));
    var (ad, _) = reflect.TypeOf(((AssignD)default!)).MethodByName(equalˢ);
    fmt.Println(namedRecvToParamˢ, reflect.TypeOf(((AssignD)default!)).AssignableTo(ad.Type.In(1)));
    Func</*<-*/channel<nint>> h = default!;
    h = reflect.MakeFunc(reflect.TypeOf((h).OrTypedNilFunc()), (slice<reflectꓸValue> _Δp0) => new reflectꓸValue[]{reflect.ValueOf(new channel<nint>(0))}.slice()).Interface()._<Func</*<-*/channel<nint>>>();
    fmt.Println(makefuncˢ, h() != default!);
    _ = (Action<channel/*<-*/</*<-*/channel<nint>>>)(nestedDir);
}

} // end main_package
