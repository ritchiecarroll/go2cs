global using Alias = go.SameNameImportAlias.b.foo_package.Other;

namespace go.SameNameImportAlias.b;

partial class foo_package {

partial struct Other {
    public @string S;
}

partial struct ΔKind /*@string*/;

partial struct S {
}

public static ΔKind Kind(this S _) {
    return (@string)"b"u8;
}

} // end foo_package
