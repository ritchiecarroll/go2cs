namespace go;

using fmt = fmt_package;
using reflect = reflect_package;
using host = global::go.go.varhost_package;
using global::go.go;
using varhost = global::go.go.varhost_package;
using ꓸꓸꓸany = Span<any>;

partial class main_package {

partial struct cached {
    /*embed*/ public global::go.go.varhost_package.Host Host;
    internal nint hits;
}

// Go method set entry for the promoted 'Host.Trace()' - provided ONLY by the embedded
// interface field in *cached's method set; see the pointer-only satisfaction record.
internal static @string Trace(this cached recvᴛ, @string format, params ꓸꓸꓸany argsʗp) => recvᴛ.Host.Trace(format, argsʗp);

internal static @string Name(this ref cached c) {
    c.hits++;
    return fmt.Sprintf("cached(%d):%s"u8, c.hits, c.Host.Name());
}

internal static void main() {
    var c = Ꮡ(new cached(Host: new host.BaseжHost(Ꮡ(new host.Base(Tag: "t")))));
    fmt.Println(host.Use(new cachedжHost(c)));
    fmt.Println((~c).Host.Trace("direct %v", true));
    
    var cʗ1 = c;
    Funcꓸꓸꓸ<@string, any, @string> trace = cʗ1.Trace;
    fmt.Println(trace("method value %d", (nint)7));
    fmt.Println(trace("no args"));

    host.Host h = new cachedжHost(c);
    var (_, valueIsHost) = ((any)(c.Value))._<host.Host>(ᐧ);
    fmt.Println(h.Name(), valueIsHost);

    var (pt, vt) = (reflect.TypeOf(c.OrTypedNil()), reflect.TypeOf(c.Value));
    fmt.Println("methods:", pt.NumMethod(), vt.NumMethod());
    foreach (var i in range(pt.NumMethod())) {
        fmt.Println(" *cached:", pt.Method(i).Name);
    }
    foreach (var i in range(vt.NumMethod())) {
        fmt.Println("  cached:", vt.Method(i).Name);
    }
    var (m, ok) = pt.MethodByName("Trace"); if (ok) {
        var @out = m.Func.Call(slice<reflectꓸValue>([reflect.ValueOf(c.OrTypedNil()), reflect.ValueOf("via %s"), reflect.ValueOf("reflect")]));
        fmt.Println(@out[0].String());
    } else {
        fmt.Println("MethodByName(Trace) missing");
    }
}

} // end main_package
