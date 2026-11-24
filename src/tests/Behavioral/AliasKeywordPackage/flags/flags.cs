global using Flag = go.AliasKeywordPackage.internal_package.Flag;

namespace go.AliasKeywordPackage;

using @checked = go.AliasKeywordPackage.checked_package;
using @internal = go.AliasKeywordPackage.internal_package;
using go.AliasKeywordPackage;

partial class flags_package {

partial struct Bools /*num:uint64*/;

internal static uint64 Options⁔(/*this*/ Bools b, @internal.NotForPublicUse _) { return (uint64)b; }

internal static @checked.Token Check⁔(/*this*/ Bools b, @checked.Token t) { return t.Twice() + ((@checked.Token)(nint)b); }

internal static bool Has⁔(/*this*/ Bools b, Flag f) { return (Bools)(b & ((Bools)((uint64)1 << (int)(f)))) != 0; }

} // end flags_package
