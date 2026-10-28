namespace go;

partial class main_package {


internal partial struct varFirst_type /*dyn*/ {
    /*embed*/ public Greeter Greeter;
}
internal static Greeter varFirst = func(Greeter (@string s) => {
    return new varFirst_type(new namedGreeter(s));
})("package-scope");

internal static @string varFirstLabel = "varfirst";

} // end main_package
