namespace go.SiblingPackageNames.teststructs.foo2;

partial class foo_package {

public static @string Name() {
    return "foo2";
}

partial struct Triple {
    public @string A, B, C;
}

public static @string Join(this Triple t) {
    return t.A + t.B + t.C;
}

} // end foo_package
