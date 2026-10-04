global using Alias = go.SameNameImportAlias.a.foo_package.Inner;

namespace go.SameNameImportAlias.a;

partial class foo_package {

[GoType] partial struct Inner {
    public nint N;
}

[GoType("num:nint")] partial struct ΔKind;

[GoType] partial struct S {
}

public static ΔKind Kind(this S _) {
    return 1;
}

} // end foo_package
