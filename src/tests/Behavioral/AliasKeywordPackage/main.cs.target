namespace go;

using fmt = fmt_package;
using @checked = AliasKeywordPackage.checked_package;
using flags = AliasKeywordPackage.flags_package;
using Δinternal = AliasKeywordPackage.internal_package;
using opts = AliasKeywordPackage.opts_package;
using partial = AliasKeywordPackage.partial_package;
using @record = AliasKeywordPackage.record_package;
using tokens = AliasKeywordPackage.tokens_package;
using @union = AliasKeywordPackage.union_package;
using AliasKeywordPackage;

partial class main_package {

internal static void main() {
    var b = (flags.Bools)5;
    fmt.Println("options:", b.Options(new Δinternal.NotForPublicUse()));
    fmt.Println("check:", b.Check((@checked.Token)3));
    fmt.Println("has 0:", b.Has((Δinternal.Flag)0), "has 1:", b.Has(1), "has 2:", b.Has((Δinternal.Flag)2));
    fmt.Println("lock:", b.Lock((tokens.Kind)7), "union:", b.Union((@union.Set)2), "record:", b.Record((@record.Entry)4), "partial:", b.Partial((partial.Part)9));
    fmt.Println("pick 2:", ((opts.Mode)1).Pick(b, 2), "pick 1:", ((opts.Mode)1).Pick(b, 1));
}

} // end main_package
