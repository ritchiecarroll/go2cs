global using Alias = go.SameNameImportAlias.a.foo_package.Inner;

namespace go.SameNameImportAlias.a;

partial class foo_package {

partial struct Inner {
    public nint N;
}

partial struct ΔKind /*num:nint*/;

partial struct S {
}

public static ΔKind Kind(this S _) {
    return 1;
}

} // end foo_package
