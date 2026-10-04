global using Alias = go.SiblingPackageNames.teststructs.foo2.foo_package.Triple;

namespace go.SiblingPackageNames.teststructs.foo2;

partial class foo_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string foo2ˢ = "foo2"u8;

public static @string Name() {
    return foo2ˢ;
}

[GoType] partial struct Triple {
    public @string A, B, C;
}

public static @string Join(this Triple t) {
    return t.A + t.B + t.C;
}

} // end foo_package
