namespace go.ForeignEmbedPromotedPointer;

partial class lib_package {

partial interface Named {
    @string Name();
}

partial interface Counter {
    @string Name();
    nint Inc();
}

partial struct T {
    internal nint n;
}

internal static @string Name⁔(/*this*/ T t) { return "lib.T"; }

internal static nint Inc⁔(/*this*/ ref T t) {
    t.n++;
    return t.n;
}

} // end lib_package
