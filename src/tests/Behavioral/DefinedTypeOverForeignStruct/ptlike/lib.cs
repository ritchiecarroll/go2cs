namespace go.DefinedTypeOverForeignStruct;

using ꓸꓸꓸnint = Span<nint>;
using static ptlike_package;

partial class ptlike_package {

partial struct Inner {
    public slice<nint> Vals;
}

} partial class ptlikeᴛInner {

public static nint Len(this Inner i) {
    return len(i.Vals);
}

public static void Push(this ref Inner i, nint v) {
    i.Vals = append(i.Vals, v);
}

} partial class ptlike_package {

partial struct Outer {
    public @string Name;
    public Inner In;
}

public static Outer New(@string name, params ꓸꓸꓸnint valsʗp) {
    var vals = valsʗp.slice();

    return new Outer(Name: name, In: new Inner(Vals: vals));
}

} // end ptlike_package
