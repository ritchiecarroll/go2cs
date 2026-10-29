namespace go;

using fmt = fmt_package;
using os = os_package;
using reflect = reflect_package;
using lib = AotReflectGaps.lib_package;
using AotReflectGaps;

partial class main_package {

partial struct Holder {
    public ж<lib.Hidden> P;
    public nint N;
}

partial struct Num {
    public nint N;
}

public static (nint, @string) Add(this Num n, nint d, @string label) {
    return (n.N + d, label);
}

public static float64 Scale(this Num n, float64 f) {
    return (float64)n.N * f;
}

partial interface Shaper {
    complex64 Area(int8 a, uint16 b);
}

partial struct Ghost {
    public nint X;
}

public static uint32 Haunt(this Ghost g, bool b, rune r) {
    return (uint32)g.X + (uint32)r;
}

internal static void run(@string name, Action f) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var r = recover(); if (r != nil) {
                    fmt.Printf("%s: recovered: %v\n"u8, name, r);
                }
            }
        }, ref ᒐ);
        f();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void Main() {
    @string only = "";
    if (len(os.Args) > 1) {
        only = os.Args[1];
    }
    if (only == ""u8 || only == "1"u8) {
        typeOps();
    }
    if (only == ""u8 || only == "2"u8) {
        methodTypes();
    }
    if (only == ""u8 || only == "3"u8) {
        run("3a interface Method(0).Type", () => {
            var m = reflect.TypeOf((ж<Shaper>)nil).Elem().Method(0);
            fmt.Println("3a", m.Name, m.Type);
        });
    }
    if (only == ""u8 || only == "4"u8) {
        var gt = reflect.TypeOf((ж<Ghost>)nil);
        var gtʗ1 = gt;
        run("4a never-built Method(0).Type value", () => {
            var m = gtʗ1.Elem().Method(0);
            fmt.Println("4a", m.Name, m.Type);
        });
        var gtʗ2 = gt;
        run("4b never-built Method(0).Type pointer", () => {
            var m = gtʗ2.Method(0);
            fmt.Println("4b", m.Name, m.Type);
        });
    }
}

internal static void typeOps() {
    ref var h = ref heap(new Holder(), out var Ꮡh);
    h.N = 3;
    var ft = reflect.TypeOf(h).Field(0).Type;

    run("1a fmt %v nil field", () => {
        fmt.Printf("1a %v\n"u8, Ꮡh.Value);
    });
    run("1b fmt %+v nil field", () => {
        fmt.Printf("1b %+v\n"u8, Ꮡh.Value);
    });
    var ftʗ1 = ft;
    run("1c field type", () => {
        fmt.Println("1c", ftʗ1, ftʗ1.Elem(), ftʗ1.Elem().Kind());
    });
    var ftʗ2 = ft;
    run("1d reflect.New elem", () => {
        var p = reflect.New(ftʗ2.Elem());
        p.Elem().Field(0).SetInt(7);
        fmt.Printf("1d %v %T\n"u8, p.Elem().Interface(), p.Interface());
    });
    var ftʗ3 = ft;
    run("1e reflect.Zero elem", () => {
        fmt.Printf("1e %+v\n"u8, reflect.Zero(ftʗ3.Elem()).Interface());
    });
    var ftʗ4 = ft;
    run("1f reflect.Zero ptr", () => {
        fmt.Printf("1f %v\n"u8, reflect.Zero(ftʗ4).IsNil());
    });
    var ftʗ5 = ft;
    run("1g set field through New", () => {
        var v = reflect.ValueOf(Ꮡh).Elem();
        v.Field(0).Set(reflect.New(ftʗ5.Elem()));
        fmt.Printf("1g %+v\n"u8, Ꮡh.Value.P.Value);
    });
}

internal static void methodTypes() {
    var nt = reflect.TypeOf(new Num());
    var ntʗ1 = nt;
    run("2a Method(0).Type value", () => {
        var m = ntʗ1.Method(0);
        fmt.Println("2a", m.Name, m.Type);
    });
    var ntʗ2 = nt;
    run("2b Method(1).Type value", () => {
        var m = ntʗ2.Method(1);
        fmt.Println("2b", m.Name, m.Type);
    });
    var ntʗ3 = nt;
    run("2c Method(0).Type pointer", () => {
        var m = reflect.PtrTo(ntʗ3).Method(0);
        fmt.Println("2c", m.Name, m.Type);
    });
    run("2d Method call", () => {
        var @out = reflect.ValueOf(new Num(N: 2)).Method(0).Call(new reflectꓸValue[]{reflect.ValueOf((nint)5), reflect.ValueOf("x")}.slice());
        fmt.Println("2d", @out[0].Int(), @out[1].String());
    });
}

} // end main_package
