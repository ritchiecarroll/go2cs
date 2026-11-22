namespace go;

using context = context_package;
using driver = database.sql.driver_package;
using fmt = fmt_package;
using reflect = reflect_package;
using database.sql;

partial class main_package {

partial struct Driver {
    internal @string name;
}

public static (driver.Conn, error) Open(this ref Driver d, @string _) { return (default, default); }

partial interface Connector {
    Driver Driver();
    @string Name();
}

partial struct @base {
    internal @string tag;
}

internal static Driver Driver⁔(/*this*/ ref @base b) { return new Driver(name: "driver:"u8 + b.tag); }
internal static @string Name⁔(/*this*/ ref @base b) { return "base:"u8 + b.tag; }

partial struct counting {
    /*embed*/ public Connector Connector;
    internal nint hits;
}

// Go method set entry for the promoted 'Connector.Driver()' - provided ONLY by the embedded
// interface field in *counting's method set; see the pointer-only satisfaction record.
internal static Driver Driver⁔(/*this*/ counting recvᴛ) => recvᴛ.Connector.Driver();

internal static @string Name⁔(/*this*/ ref counting c) {
    c.hits++;
    return fmt.Sprintf("counting(%d):%s"u8, c.hits, c.Connector.Name());
}

partial struct fbase {
}

internal static (driver.Conn, error) Connect⁔(/*this*/ fbase _Δp0, context.Context _Δp1) { return (default, default); }
internal static driver.Driver Driver⁔(/*this*/ fbase _) { return new DriverжDriver(Ꮡ(new Driver(name: "foreign"))); }

partial struct fcounting {
    /*embed*/ public database.sql.driver_package.Connector Connector;
    internal nint n;
}

// Go method set entry for the promoted 'Connector.Driver()' - provided ONLY by the embedded
// interface field in *fcounting's method set; see the pointer-only satisfaction record.
internal static driver.Driver Driver⁔(/*this*/ fcounting recvᴛ) => recvᴛ.Connector.Driver();

internal static (driver.Conn, error) Connect⁔(/*this*/ ref fcounting c, context.Context ctx) {
    c.n++;
    return c.Connector.Connect(ctx);
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

    foreign();
}

internal static void foreign() {
    var fc = Ꮡ(new fcounting(Connector: new fbase()));
    driver.Connector k = new fcountingжConnector(fc);
    k.Connect(context.Background());
    var (d, ok) = k.Driver()._<ж<Driver>>(ᐧ);
    fmt.Println("foreign:", ok, fc.n, d.name, (~fc).Connector.Driver() != nil);
}

} // end main_package
