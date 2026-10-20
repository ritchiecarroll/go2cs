namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct Kind /*@string*/;

partial struct parser {
    internal nint n;
}

internal static @string greeting = makeGreeting("world");

internal static @string makeGreeting(@string who) {
    @string hello = "hello, ";
    return hello + who;
}

internal static @string atoi(@string s) {
    @string fnAtoi = "Atoi";
    if (s == ""u8) {
        return fnAtoi + ": empty";
    }
    return fnAtoi + "(" + s + ")";
}

internal static Kind named() {
    Kind k = "kind-value"u8;
    return k;
}

internal partial struct localKind_tag /*@string*/;

internal static @string localKind() {
    localKind_tag t = "local-tag"u8;
    return (@string)t;
}

internal static @string multi() {
    @string a = "first-a";
    @string b = "second-b";
    @string c = "group-c";
    @string d = "group-d";
    return a + "|" + b + "|" + c + "|" + d;
}

internal static @string labelOne() {
    @string label = "label-one";
    return label;
}

internal static @string labelTwo() {
    @string label = "label-two";
    return label;
}

internal static @string shadow() {
    @string s = "outer-scope";
    @string inner() {
        @string sΔ1 = "inner-scope";
        return sΔ1;
    }
    return s + "/" + inner();
}

internal static Func<@string> closure() {
    @string captured = "captured-const";
    return () => captured;
}

internal static nint empty() {
    @string e = "";
    return len(e);
}

internal static @string concatenated() {
    @string whole = "concatenated";
    return whole;
}

internal static @string classify(@string x) {
    @string yes = "matched";
    @string no = "unmatched";
    var exprᴛ1 = x;
    if (exprᴛ1 == yes) {
        return "case:" + yes;
    }

    return no;
}

internal static nint bytesConst() {
    @string raw = ((@string)(new byte[]{0xff, 0x00, 0x7f}));
    return len(raw) + (nint)raw[0];
}

internal static @string genericLabel<T>(T v) {
    @string prefix = "generic:";
    return fmt.Sprint(prefix, (v).OrTypedNilOf());
}

internal static @string name(this ref parser p) {
    @string method = "parser-method";
    p.n++;
    return fmt.Sprintf("%s#%d"u8, method, p.n);
}

internal static nint loop() {
    nint total = 0;
    for (nint i = 0; i < 3; i++) {
        @string step = "step";
        total += len(step);
    }
    return total;
}

internal static @string initValue;

[GoInit] internal static void init() {
    @string fromInit = "init-const";
    initValue = fromInit;
}

internal static void Main() {
    fmt.Println(greeting);
    fmt.Println(atoi(""), atoi("42"));
    fmt.Println(named());
    fmt.Println(localKind());
    fmt.Println(multi());
    fmt.Println(labelOne(), labelTwo());
    fmt.Println(shadow());
    fmt.Println(closure()());
    fmt.Println(empty());
    fmt.Println(concatenated());
    fmt.Println(classify("matched"), classify("other"));
    fmt.Println(bytesConst());
    fmt.Println(genericLabel<nint>(7), genericLabel((@string)"x"));
    var p = Ꮡ(new parser(nil));
    fmt.Println(p.name(), p.name());
    fmt.Println(loop());
    fmt.Println(initValue);
}

} // end main_package
