namespace go;

using context = context_package;
using fmt = fmt_package;
using reflect = reflect_package;
using time = time_package;

partial class main_package {

partial struct R {
}

public static bool Equal(this R _, /*<-*/channel<bool> y) {
    return true;
}

partial struct S {
    internal nint n;
}

public static void Feed(this ref S s, channel/*<-*/<nint> c) {
    s.n++;
}

partial interface Notifier {
    /*<-*/channel<EmptyStruct> Done();
}

partial struct AssignD /*<-chan bool*/;

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
    return (default, default);
}

internal static /*<-*/channel<bool> mixed(nint a, channel/*<-*/<nint> c) {
    return default;
}

internal static void nestedDir(channel/*<-*/</*<-*/channel<nint>> c) {
}

internal static void Main() {
    Func<time.Duration, /*<-*/channel<time.Time>> after = time.After;
    fmt.Println("func value:", reflect.TypeOf((after).OrTypedNilFunc()), reflect.TypeOf((after).OrTypedNilFunc()).Out(0).ChanDir());

    fmt.Println("recv param:", reflect.TypeOf(recvLocal), reflect.TypeOf(recvLocal).In(0).ChanDir());
    fmt.Println("send param:", reflect.TypeOf(sendOnly), reflect.TypeOf(sendOnly).In(0).ChanDir());
    fmt.Println("results:", reflect.TypeOf(two), "|", reflect.TypeOf(mixed));

    var (done, _) = reflect.TypeOf((ж<context.Context>)nil).Elem().MethodByName("Done");
    fmt.Println("interface method:", done.Type, done.Type.Out(0).ChanDir());

    var (nd, _) = reflect.TypeOf((ж<Notifier>)nil).Elem().MethodByName("Done");
    fmt.Println("own interface method:", nd.Type, nd.Type.Out(0).ChanDir());

    var (eq, _) = reflect.TypeOf(new R()).MethodByName("Equal");
    fmt.Println("value method:", eq.Type, eq.Type.In(1).ChanDir());

    var (feed, _) = reflect.TypeOf(Ꮡ(new S())).MethodByName("Feed");
    fmt.Println("pointer method:", feed.Type, feed.Type.In(1).ChanDir());

    fmt.Println("identical:", AreEqual(reflect.TypeOf(recvLocal), reflect.TypeOf(bidiLocal)));
    fmt.Println("assignable:", reflect.TypeOf(bidiLocal).AssignableTo(reflect.TypeOf(recvLocal)));
    fmt.Println("convertible:", reflect.TypeOf(bidiLocal).ConvertibleTo(reflect.TypeOf(recvLocal)));

    var (ad, _) = reflect.TypeOf((AssignD)default).MethodByName("Equal");
    fmt.Println("named recv to param:", reflect.TypeOf((AssignD)default).AssignableTo(ad.Type.In(1)));

    Func</*<-*/channel<nint>> h = default;
    h = reflect.MakeFunc(reflect.TypeOf((h).OrTypedNilFunc()), (slice<reflectꓸValue> _Δp0) => new reflectꓸValue[]{reflect.ValueOf(new channel<nint>(0))}.slice()).Interface()._<Func</*<-*/channel<nint>>>();
    fmt.Println("makefunc:", h() != default);
    _ = (Action<channel/*<-*/</*<-*/channel<nint>>>)(nestedDir);
}

} // end main_package
