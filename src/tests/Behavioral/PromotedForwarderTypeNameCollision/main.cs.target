namespace go;

using fmt = fmt_package;
using reflect = reflect_package;

partial class main_package {

partial struct ΔDriver {
    internal @string name;
}

partial interface Connector {
    ΔDriver Driver();
    @string Name();
}

partial struct @base {
    internal @string tag;
}

internal static ΔDriver Driver(this ref @base b) { return new ΔDriver(name: "driver:"u8 + b.tag); }
internal static @string Name(this ref @base b) { return "base:"u8 + b.tag; }

partial struct counting {
    /*embed*/ public Connector Connector;
    internal nint hits;
}

// Go method set entry for the promoted 'Connector.Driver()' - provided ONLY by the embedded
// interface field in *counting's method set; see the pointer-only satisfaction record.
internal static ΔDriver Driver(this counting recvᴛ) => recvᴛ.Connector.Driver();

internal static @string Name(this ref counting c) {
    c.hits++;
    return fmt.Sprintf("counting(%d):%s"u8, c.hits, c.Connector.Name());
}

internal static @string use(Connector c) { return c.Name() + " / "u8 + c.Driver().name; }

internal static void main() {
    var c = Ꮡ(new counting(Connector: new baseжConnector(Ꮡ(new @base(tag: "t")))));
    fmt.Println(use(new countingжConnector(c)));
    fmt.Println((~c).Connector.Driver().name);
    
    var cʗ1 = c;
    var driver = cʗ1.Driver;
    fmt.Println(driver().name);

    var (_, valueIsConnector) = ((any)(c.Value))._<Connector>(ᐧ);
    fmt.Println(valueIsConnector);

    var (pt, vt) = (reflect.TypeOf(c.OrTypedNil()), reflect.TypeOf(c.Value));
    fmt.Println("methods:", pt.NumMethod(), vt.NumMethod());
    foreach (var i in range(pt.NumMethod())) {
        fmt.Println(" *counting:", pt.Method(i).Name);
    }
    foreach (var i in range(vt.NumMethod())) {
        fmt.Println("  counting:", vt.Method(i).Name);
    }
}

} // end main_package
