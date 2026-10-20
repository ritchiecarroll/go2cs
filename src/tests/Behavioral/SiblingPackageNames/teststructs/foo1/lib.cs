namespace go.SiblingPackageNames.teststructs.foo1;

partial class foo_package {

public static @string Name() {
    return "foo1";
}

partial struct Pair {
    public nint Left, Right;
}

public static nint Sum(this Pair p) {
    return p.Left + p.Right;
}

} // end foo_package
