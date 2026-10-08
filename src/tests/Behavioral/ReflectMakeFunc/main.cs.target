namespace go;

using fmt = fmt_package;
using reflect = reflect_package;

partial class main_package {

internal static void makeSwap(any fptr) {
    var swap = (slice<reflectꓸValue> @in) => new reflectꓸValue[]{@in[1], @in[0]}.slice();
    var fn = reflect.ValueOf(fptr).Elem();
    var v = reflect.MakeFunc(fn.Type(), swap);
    fn.Set(v);
}

partial struct traceHooks {
    public Action<@string> OnEvent;
    public Func<nint, nint, (nint, @string)> Sum;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object intSwapˢ = (@string)"intSwap:"u8;
private static readonly @string helloˢ = "hello"u8;
private static readonly @string worldˢ = "world"u8;
private static readonly object stringSwapˢ = (@string)"stringSwap:"u8;
private static readonly @string dnsˢ = "dns"u8;
private static readonly @string connectˢ = "connect"u8;
private static readonly object eventsˢ = (@string)"events:"u8;
private static readonly object sumˢ = (@string)"sum:"u8;
private static readonly object typeMatchˢ = (@string)"type match:"u8;
private static readonly object callˢ = (@string)"call:"u8;
private static readonly object describeˢ = (@string)"describe:"u8;
private static readonly object isNilˢ = (@string)"isNil:"u8;
private static readonly object sum4ˢ = (@string)"sum4:"u8;

internal static void Main() {
    ref var intSwap = ref heap<Func<nint, nint, (nint, nint)>>(out var ᏑintSwap);
    makeSwap(ᏑintSwap);
    var (a, b) = intSwap(3, 7);
    fmt.Println(intSwapˢ, a, b);
    ref var stringSwap = ref heap<Func<@string, @string, (@string, @string)>>(out var ᏑstringSwap);
    makeSwap(ᏑstringSwap);
    var (s1, s2) = stringSwap(helloˢ, worldˢ);
    fmt.Println(stringSwapˢ, s1, s2);
    ref var events = ref heap<slice<@string>>(out var Ꮡevents);
    ref var t1 = ref heap<traceHooks>(out var Ꮡt1);
    t1 = new traceHooks(OnEvent: (@string s) => {
        Ꮡevents.ValueSlot = append(Ꮡevents.ValueSlot, "new:"u8 + s);
    });
    ref var t2 = ref heap<traceHooks>(out var Ꮡt2);
    t2 = new traceHooks(OnEvent: (@string s) => {
        Ꮡevents.ValueSlot = append(Ꮡevents.ValueSlot, "old:"u8 + s);
    });
    var tv = reflect.ValueOf(Ꮡt1).Elem();
    var ov = reflect.ValueOf(Ꮡt2).Elem();
    var tf = tv.Field(0);
    ref var of = ref heap<reflectꓸValue>(out var Ꮡof);
    of = ov.Field(0);
    ref var tfCopy = ref heap<reflectꓸValue>(out var ᏑtfCopy);
    tfCopy = reflect.ValueOf(tf.Interface());
    var ofʗ1 = of;
    var tfCopyʗ1 = tfCopy;
    var newFunc = reflect.MakeFunc(tf.Type(), (slice<reflectꓸValue> args) => {
        tfCopyʗ1.Call(args);
        return ofʗ1.Call(args);
    });
    tv.Field(0).Set(newFunc);
    t1.OnEvent(dnsˢ);
    t1.OnEvent(connectˢ);
    fmt.Println(eventsˢ, events);
    ref var h = ref heap<traceHooks>(out var Ꮡh);
    h = new traceHooks(nil);
    var hv = reflect.ValueOf(Ꮡh).Elem();
    var sumField = hv.Field(1);
    var made = reflect.MakeFunc(sumField.Type(), (slice<reflectꓸValue> args) => {
        var total = args[0].Int() + args[1].Int();
        return new reflectꓸValue[]{reflect.ValueOf((nint)total), reflect.ValueOf(fmt.Sprintf("sum=%d"u8, total))}.slice();
    });
    sumField.Set(made);
    var (n, msg) = h.Sum(4, 5);
    fmt.Println(sumˢ, n, msg);
    fmt.Println(typeMatchˢ, AreEqual(made.Type(), sumField.Type()));
    var @out = made.Call(new reflectꓸValue[]{reflect.ValueOf((nint)(10)), reflect.ValueOf((nint)(20))}.slice());
    fmt.Println(callˢ, @out[0].Int(), @out[1].String());
    ref var describe = ref heap<Func<any, @string>>(out var Ꮡdescribe);
    var dv = reflect.ValueOf(Ꮡdescribe).Elem();
    dv.Set(reflect.MakeFunc(dv.Type(), (slice<reflectꓸValue> args) => {
        var arg = args[0];
        return new reflectꓸValue[]{reflect.ValueOf(fmt.Sprintf("%v/%v"u8, arg.Kind(), arg.Elem().Kind()))}.slice();
    }));
    fmt.Println(describeˢ, describe((nint)(42)));
    ref var isNil = ref heap<Func<ж<nint>, bool>>(out var ᏑisNil);
    var nv = reflect.ValueOf(ᏑisNil).Elem();
    nv.Set(reflect.MakeFunc(nv.Type(), (slice<reflectꓸValue> args) => new reflectꓸValue[]{reflect.ValueOf(args[0].IsNil())}.slice()));
    ref var x = ref heap<nint>(out var Ꮡx);
    x = 5;
    fmt.Println(isNilˢ, isNil(nil), isNil(Ꮡx));
    ref var sum4 = ref heap<Func<array<byte>, nint>>(out var Ꮡsum4);
    var sv = reflect.ValueOf(Ꮡsum4).Elem();
    sv.Set(reflect.MakeFunc(sv.Type(), (slice<reflectꓸValue> args) => {
        nint total = 0;
        for (nint i = 0; i < args[0].Len(); i++) {
            total += (nint)args[0].Index(i).Uint();
        }
        return new reflectꓸValue[]{reflect.ValueOf(total)}.slice();
    }));
    fmt.Println(sum4ˢ, sum4(new byte[]{1, 2, 3, 4}.array()));
}

} // end main_package
