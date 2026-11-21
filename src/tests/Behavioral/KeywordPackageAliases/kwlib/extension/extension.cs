namespace go.go.kwlib;

partial class extension_package {

public static @string Name() { return "extension"; }

partial struct Kind /*num:nint*/;

internal static Kind Double⁔(/*this*/ Kind k) { return k * 2; }

partial struct Δextension {
    internal nint n;
}

public static nint Scaled(nint n) {
    var v = new Δextension(n);
    return v.n * 10;
}

} // end extension_package
