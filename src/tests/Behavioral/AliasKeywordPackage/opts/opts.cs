namespace go.AliasKeywordPackage;

using flags = go.AliasKeywordPackage.flags_package;
using @internal = go.AliasKeywordPackage.internal_package;
using go.AliasKeywordPackage;

partial class opts_package {

partial struct Mode /*num:uint8*/;

internal static bool Pick⁔(/*this*/ Mode m, flags.Bools b, @internal.Flag f) { return m != 0 && b.Has(f); }

} // end opts_package
