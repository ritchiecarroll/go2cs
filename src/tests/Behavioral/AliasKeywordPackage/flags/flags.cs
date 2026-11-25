global using Flag = go.AliasKeywordPackage.internal_package.Flag;

namespace go.AliasKeywordPackage;

using @checked = go.AliasKeywordPackage.checked_package;
using @internal = go.AliasKeywordPackage.internal_package;
using partial = go.AliasKeywordPackage.partial_package;
using @record = go.AliasKeywordPackage.record_package;
using @union = go.AliasKeywordPackage.union_package;
using @lock = go.AliasKeywordPackage.tokens_package;
using go.AliasKeywordPackage;
using tokens = go.AliasKeywordPackage.tokens_package;

partial class flags_package {

partial struct Bools /*num:uint64*/;

internal static uint64 Options⁔(/*this*/ Bools b, @internal.NotForPublicUse _) { return (uint64)b; }

internal static @checked.Token Check⁔(/*this*/ Bools b, @checked.Token t) { return t.Twice() + ((@checked.Token)(nint)b); }

internal static bool Has⁔(/*this*/ Bools b, Flag f) { return (Bools)(b & ((Bools)((uint64)1 << (int)(f)))) != 0; }

internal static @lock.Kind Lock⁔(/*this*/ Bools b, @lock.Kind k) { return k + ((@lock.Kind)(nint)((Bools)(b & 1))); }

internal static @union.Set Union⁔(/*this*/ Bools b, @union.Set s) { return (@union.Set)(s | ((@union.Set)(uint8)b)); }

internal static @record.Entry Record⁔(/*this*/ Bools b, @record.Entry e) { return e * ((@record.Entry)(nint)b); }

internal static partial.Part Partial⁔(/*this*/ Bools b, partial.Part p) { return p - ((partial.Part)(nint)b); }

} // end flags_package
