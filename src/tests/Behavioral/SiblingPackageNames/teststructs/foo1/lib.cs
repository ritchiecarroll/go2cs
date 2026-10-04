global using Alias = go.SiblingPackageNames.teststructs.foo1.foo_package.Pair;

namespace go.SiblingPackageNames.teststructs.foo1;

partial class foo_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string foo1ˢ = "foo1"u8;

public static @string Name() {
    return foo1ˢ;
}

[GoType] partial struct Pair {
    public nint Left, Right;
}

public static nint Sum(this Pair p) {
    return p.Left + p.Right;
}

} // end foo_package
