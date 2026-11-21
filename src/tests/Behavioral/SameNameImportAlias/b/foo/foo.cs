global using Alias = go.SameNameImportAlias.b.foo_package.Other;

namespace go.SameNameImportAlias.b;

partial class foo_package {

partial struct Other {
    public @string S;
}

partial struct Kind /*@string*/;

partial struct S {
}

internal static Kind Kind⁔(/*this*/ S _) { return (@string)"b"; }

} // end foo_package
