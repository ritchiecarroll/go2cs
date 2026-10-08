namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct Kind /*@string*/;

partial struct parser {
    internal nint n;
}

internal static @string greeting;
internal static void initᴛgreeting() { greeting = makeGreeting("world"u8); }

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
private static readonly @string helloᶜ = "hello, "u8;

internal static @string makeGreeting(@string who) {
    @string hello = helloᶜ;
    return hello + who;
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
private static readonly @string fnAtoiᶜ = "Atoi"u8;

internal static @string atoi(@string s) {
    @string fnAtoi = fnAtoiᶜ;
    if (s == ""u8) {
        return fnAtoi + ": empty";
    }
    return fnAtoi + "(" + s + ")";
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
private static readonly Kind kᶜ = "kind-value"u8;

internal static Kind named() {
    Kind k = kᶜ;
    return k;
}

internal partial struct localKind_tag /*@string*/;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
private static readonly localKind_tag tᶜ = "local-tag"u8;

internal static @string localKind() {
    localKind_tag t = tᶜ;
    return ((@string)t);
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
private static readonly @string aᶜ = "first-a"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
private static readonly @string bᶜ = "second-b"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
private static readonly @string cᶜ = "group-c"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
private static readonly @string dᶜ = "group-d"u8;

internal static @string multi() {
    @string a = aᶜ;
    @string b = bᶜ;
    @string c = cᶜ;
    @string d = dᶜ;
    return a + "|" + b + "|" + c + "|" + d;
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
private static readonly @string labelᶜ = "label-one"u8;

internal static @string labelOne() {
    @string label = labelᶜ;
    return label;
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
private static readonly @string labelᶜ1 = "label-two"u8;

internal static @string labelTwo() {
    @string label = labelᶜ1;
    return label;
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
private static readonly @string sᶜ = "outer-scope"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
private static readonly @string sΔ1ᶜ = "inner-scope"u8;

internal static @string shadow() {
    @string s = sᶜ;
    @string inner() {
        @string sΔ1 = sΔ1ᶜ;
        return sΔ1;
    }
    return s + "/" + inner();
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
private static readonly @string capturedᶜ = "captured-const"u8;

internal static Func<@string> closure() {
    @string captured = capturedᶜ;
    return () => captured;
}

internal static nint empty() {
    @string e = ""u8;
    return len(e);
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
private static readonly @string wholeᶜ = "concatenated";

internal static @string concatenated() {
    @string whole = wholeᶜ;
    return whole;
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
private static readonly @string yesᶜ = "matched"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
private static readonly @string noᶜ = "unmatched"u8;

internal static @string classify(@string x) {
    @string yes = yesᶜ;
    @string no = noᶜ;
    var exprᴛ1 = x;
    if (exprᴛ1 == yes) {
        return "case:" + yes;
    }

    return no;
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
private static readonly @string rawᶜ = ((@string)(new byte[]{0xff, 0x00, 0x7f}));

internal static nint bytesConst() {
    @string raw = rawᶜ;
    return len(raw) + (nint)raw[0];
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
private static readonly @string prefixᶜ = "generic:"u8;

internal static @string genericLabel<T>(T v) {
    @string prefix = prefixᶜ;
    return fmt.Sprint(prefix, v);
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
private static readonly @string methodᶜ = "parser-method"u8;

internal static @string name(this ref parser p) {
    @string method = methodᶜ;
    p.n++;
    return fmt.Sprintf("%s#%d"u8, method, p.n);
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
private static readonly @string stepᶜ = "step"u8;

internal static nint loop() {
    nint total = 0;
    for (nint i = 0; i < 3; i++) {
        @string step = stepᶜ;
        total += len(step);
    }
    return total;
}

internal static @string initValue;

[GoInit] internal static void init() {
    @string fromInit = "init-const"u8;
    initValue = fromInit;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string matchedˢ = "matched"u8;
private static readonly @string otherˢ = "other"u8;

internal static void Main() {
    fmt.Println(greeting);
    fmt.Println(atoi(""u8), atoi("42"u8));
    fmt.Println(named());
    fmt.Println(localKind());
    fmt.Println(multi());
    fmt.Println(labelOne(), labelTwo());
    fmt.Println(shadow());
    fmt.Println(closure()());
    fmt.Println(empty());
    fmt.Println(concatenated());
    fmt.Println(classify(matchedˢ), classify(otherˢ));
    fmt.Println(bytesConst());
    fmt.Println(genericLabel(7), genericLabel((@string)"x"));
    var p = Ꮡ(new parser(nil));
    fmt.Println(p.name(), p.name());
    fmt.Println(loop());
    fmt.Println(initValue);
}

} // end main_package
