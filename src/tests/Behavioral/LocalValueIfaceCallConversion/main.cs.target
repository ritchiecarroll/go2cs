namespace go;

using fmt = fmt_package;
using static main_package;

partial class main_package {

partial struct LocalKey /*[]byte*/;

} partial class mainᴛLocalKey {

public static @string String(this LocalKey k) { return "key:"u8 + ((@string)(slice<byte>)k); }

} partial class main_package {

partial struct Label {
    public @string Text;
}

} partial class mainᴛLabel {

public static @string String(this Label l) { return "label:"u8 + l.Text; }

} partial class main_package {

partial interface unexpIface {
    @string f();
}

partial struct embedWithUnexpMeth {
}

} partial class mainᴛembedWithUnexpMeth {

internal static @string f(this embedWithUnexpMeth _) { return "f"; }

} partial class main_package {

internal static unexpIface pinUnexpMethI = ((unexpIface)new embedWithUnexpMeth());

partial interface LocalIface {
    @string G();
}

partial struct localImpl {
    public nint N;
}

} partial class mainᴛlocalImpl {

internal static @string G(this localImpl l) { return fmt.Sprint("g", l.N); }

} partial class main_package {

internal delegate nint meter();

} partial class mainᴛmeter {

internal static nint Value(this meter m) { return m(); }

} partial class main_package {

partial struct gauge /*num:nint*/;

} partial class mainᴛgauge {

internal static nint Value(this gauge g) { return (nint)g * 2; }

} partial class main_package {

partial interface valued {
    nint Value();
}

internal static void main() {
    var k = (LocalKey)slice<byte>((@string)"abc"u8);
    var s = ((fmt.Stringer)k);
    fmt.Println("foreign-slice:", s.String());

    fmt.Printf("foreign-slice-type: %T\n"u8, s);

    var ls = ((fmt.Stringer)new Label(Text: "x"));
    fmt.Println("foreign-struct:", ls.String());
    fmt.Printf("foreign-struct-type: %T %v\n"u8, ls, ls);
    var (lv, lok) = ls._<Label>(ᐧ);
    fmt.Println("foreign-struct-assert:", lok, lv.Text, AreEqual(ls, ((fmt.Stringer)new Label(Text: "x"))));

    fmt.Println("unexported-iface:", pinUnexpMethI.f());
    var (_, uok) = pinUnexpMethI._<embedWithUnexpMeth>(ᐧ);
    fmt.Println("unexported-iface-assert:", uok);

    var li = ((LocalIface)new localImpl(N: 2));
    fmt.Println("local-iface:", li.G());
    fmt.Println("local-iface-eq:", AreEqual(li, ((LocalIface)new localImpl(N: 2))), AreEqual(li, ((LocalIface)new localImpl(N: 3))));

    var mv = ((valued)new meterᴠvalued(new meter(() => 11)));
    var gv = ((valued)((gauge)4));
    fmt.Println("func-source:", mv.Value(), "numeric-source:", gv.Value());

    switch (gv.type()) {
    case meter t: {
        fmt.Println("switch:", "meter", t());
        break;
    }
    case gauge t: {
        fmt.Println("switch:", "gauge", (nint)t);
        break;
    }
    default: {
        var t = gv;
        fmt.Println("switch:", "default");
        break;
    }}

    var seen = new map<valued, @string>{};
    seen[gv] = "call-syntax";
    valued byAssign = ((gauge)4);
    fmt.Println("map:", seen[byAssign], len(seen));
}

} // end main_package
