namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct LocalKey /*[]byte*/;

public static @string String(this LocalKey k) {
    return "key:"u8 + ((@string)(slice<byte>)k);
}

partial struct Label {
    public @string Text;
}

public static @string String(this Label l) {
    return "label:"u8 + l.Text;
}

partial interface unexpIface {
    @string f();
}

partial struct embedWithUnexpMeth {
}

internal static @string f(this embedWithUnexpMeth _) {
    return "f"u8;
}

internal static unexpIface pinUnexpMethI = ((unexpIface)new embedWithUnexpMeth(nil));

partial interface LocalIface {
    @string G();
}

partial struct localImpl {
    public nint N;
}

internal static @string G(this localImpl l) {
    return fmt.Sprint((@string)"g"u8, l.N);
}

internal delegate nint meter();

internal static nint Value(this meter m) {
    return m();
}

partial struct gauge /*num:nint*/;

internal static nint Value(this gauge g) {
    return (nint)g * 2;
}

partial interface valued {
    nint Value();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object foreignSliceˢ = (@string)"foreign-slice:"u8;
private static readonly object foreignStructˢ = (@string)"foreign-struct:"u8;
private static readonly object foreignStructAssertˢ = (@string)"foreign-struct-assert:"u8;
private static readonly object unexportedIfaceˢ = (@string)"unexported-iface:"u8;
private static readonly object unexportedIfaceAssertˢ = (@string)"unexported-iface-assert:"u8;
private static readonly object localIfaceˢ = (@string)"local-iface:"u8;
private static readonly object localIfaceEqˢ = (@string)"local-iface-eq:"u8;
private static readonly object funcSourceˢ = (@string)"func-source:"u8;
private static readonly object numericSourceˢ = (@string)"numeric-source:"u8;
private static readonly object switchˢ = (@string)"switch:"u8;
private static readonly object meterˢ = (@string)"meter"u8;
private static readonly object gaugeˢ = (@string)"gauge"u8;
private static readonly object defaultˢ = (@string)"default"u8;
private static readonly @string callSyntaxˢ = "call-syntax"u8;
private static readonly object mapˢ = (@string)"map:"u8;

internal static void Main() {
    var k = ((LocalKey)slice<byte>((@string)"abc"u8));
    var s = ((fmt.Stringer)k);
    fmt.Println(foreignSliceˢ, s.String());
    fmt.Printf("foreign-slice-type: %T\n"u8, s);
    var ls = ((fmt.Stringer)new Label(Text: "x"u8));
    fmt.Println(foreignStructˢ, ls.String());
    fmt.Printf("foreign-struct-type: %T %v\n"u8, ls, ls);
    var (lv, lok) = ls._<Label>(ᐧ);
    fmt.Println(foreignStructAssertˢ, lok, lv.Text, AreEqual(ls, ((fmt.Stringer)new Label(Text: "x"u8))));
    fmt.Println(unexportedIfaceˢ, pinUnexpMethI.f());
    var (_, uok) = pinUnexpMethI._<embedWithUnexpMeth>(ᐧ);
    fmt.Println(unexportedIfaceAssertˢ, uok);
    var li = ((LocalIface)new localImpl(N: 2));
    fmt.Println(localIfaceˢ, li.G());
    fmt.Println(localIfaceEqˢ, AreEqual(li, ((LocalIface)new localImpl(N: 2))), AreEqual(li, ((LocalIface)new localImpl(N: 3))));
    var mv = ((valued)new meterᴠvalued(new meter(() => 11)));
    var gv = ((valued)((gauge)4));
    fmt.Println(funcSourceˢ, mv.Value(), numericSourceˢ, gv.Value());
    switch (gv.type()) {
    case meter t: {
        fmt.Println(switchˢ, meterˢ, t());
        break;
    }
    case gauge t: {
        fmt.Println(switchˢ, gaugeˢ, (nint)t);
        break;
    }
    default: {
        var t = gv;
        fmt.Println(switchˢ, defaultˢ);
        break;
    }}
    var seen = new map<valued, @string>{};
    seen[gv] = callSyntaxˢ;
    valued byAssign = ((gauge)4);
    fmt.Println(mapˢ, seen[byAssign], len(seen));
}

} // end main_package
