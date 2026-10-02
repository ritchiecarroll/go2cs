namespace go;

partial class main_package {


[GoType("dyn")] internal partial struct varFirst_type {
    [GoEmbedded] public Greeter Greeter;
}
internal static Greeter varFirst = ((Func<@string, Greeter>)(s => {
    return new varFirst_type(new namedGreeter(s));
}))("package-scope");

internal static @string varFirstLabel = "varfirst"u8;

} // end main_package
