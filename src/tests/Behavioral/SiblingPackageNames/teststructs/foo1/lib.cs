namespace go.SiblingPackageNames.teststructs.foo1;

using static foo_package;

partial class foo_package {

public static @string Name() {
    return "foo1";
}

partial struct Pair {
    public nint Left, Right;
}

} partial class fooᴛPair {

public static nint Sum(this Pair p) {
    return p.Left + p.Right;
}

} // end fooᴛPair
