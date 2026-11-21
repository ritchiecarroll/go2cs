namespace go;

using fmt = fmt_package;
using ast = global::go.go.ptrunion_package;
using ptrunion = global::go.go.ptrunion_package;

partial class main_package {

partial struct Type {
    public nint ID;
}

partial interface entity<ΔT> {
    //  Type constraints: *ast.Node | *ast.Symbol | *Type
    // Derived operators: none
}

internal static @string describe<T>(T x) {
    if (GoReflect.IsNilGoValue(x)) {
        return "nil";
    }
    switch (((any)(x).OrTypedNilOf()).type()) {
    case ж<ast.Node> v: {
        return fmt.Sprintf("node:%d"u8, v.Kind);
    }
    case ж<ast.Symbol> v: {
        return "symbol:"u8 + v.Name;
    }
    case ж<Type> v: {
        return fmt.Sprintf("type:%d"u8, v.ID);
    }}
    return "?";
}

internal static bool present<T>(T x) { return !GoReflect.IsNilGoValue(x); }

internal static T zero<T>() {
    T z = GoZero<T>();
    return z;
}

internal static bool eq<T>(T a, T b) { return AreEqual(a, b); }

partial struct link<T> {
    internal T from, to;
}

internal static @string String<T>(this link<T> l) {
    if (GoReflect.IsNilGoValue(l.to)) {
        return describe(l.from) + "->(none)"u8;
    }
    return describe(l.from) + "->"u8 + describe(l.to);
}

partial struct holder<P> {
    internal P p;
}

internal static bool empty<P>(this holder<P> h) { return GoReflect.IsNilGoValue(h.p); }

partial struct holderOf<P, T> {
    internal P p;
}

internal static bool empty<P, T>(this holderOf<P, T> h) { return GoReflect.IsNilGoValue(h.p); }

internal static void main() {
    var n = Ꮡ(new ast.Node(Kind: 4));
    fmt.Println(describe(n), describe(Ꮡ(new ast.Symbol(Name: "x"))), describe(Ꮡ(new Type(ID: 9))));
    fmt.Println(describe(zero<ж<Type>>()), describe<ж<ast.Node>>(nil), present(n), present<ж<ast.Symbol>>(nil));
    fmt.Println(new link<ж<ast.Node>>(n, n), new link<ж<ast.Node>>(n, nil), new link<ж<Type>>());
    fmt.Println(eq(n, n), eq(n, Ꮡ(new ast.Node(Kind: 4))), eq<ж<Type>>(nil, nil));
    fmt.Println(new holder<ж<Type>>().empty(), new holder<ж<Type>>(Ꮡ(new Type(1))).empty());
    fmt.Println(new holderOf<ж<nint>, nint>().empty(), new holderOf<ж<nint>, nint>(@new<nint>()).empty());
}

} // end main_package
