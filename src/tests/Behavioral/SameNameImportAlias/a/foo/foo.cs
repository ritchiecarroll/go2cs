global using Alias = go.SameNameImportAlias.a.foo_package.Inner;

namespace go.SameNameImportAlias.a;

partial class foo_package {

partial struct Inner {
    public nint N;
}

partial struct Kind /*num:nint*/;

partial struct S {
}

internal static Kind Kind⁔(/*this*/ S _) { return 1; }

} // end foo_package
