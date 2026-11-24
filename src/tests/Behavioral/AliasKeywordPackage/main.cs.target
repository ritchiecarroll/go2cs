namespace go;

using fmt = fmt_package;
using @checked = AliasKeywordPackage.checked_package;
using flags = AliasKeywordPackage.flags_package;
using Δinternal = AliasKeywordPackage.internal_package;
using AliasKeywordPackage;

partial class main_package {

internal static void main() {
    var b = (flags.Bools)5;
    fmt.Println("options:", b.Options(new Δinternal.NotForPublicUse()));
    fmt.Println("check:", b.Check((@checked.Token)3));
    fmt.Println("has 0:", b.Has((Δinternal.Flag)0), "has 1:", b.Has(1), "has 2:", b.Has((Δinternal.Flag)2));
}

} // end main_package
