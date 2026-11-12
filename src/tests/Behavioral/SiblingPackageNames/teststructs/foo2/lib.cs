namespace go.SiblingPackageNames.teststructs.foo2;

using static foo_package;

partial class foo_package {

public static @string Name() {
    return "foo2";
}

partial struct Triple {
    public @string A, B, C;
}

} partial class fooᴛTriple {

public static @string Join(this Triple t) {
    return t.A + t.B + t.C;
}

} // end fooᴛTriple
