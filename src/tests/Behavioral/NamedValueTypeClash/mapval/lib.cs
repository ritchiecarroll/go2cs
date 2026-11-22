namespace go.NamedValueTypeClash;

partial class mapval_package {

partial struct Value /*map[@string, nint]*/;

internal static nint Count⁔(/*this*/ Value v) { return len(v); }

internal static nint Get⁔(/*this*/ Value v, @string k) { return v[k]; }

internal static void Put⁔(/*this*/ ref Value v, @string k, nint x) { (v)[k] = x; }

} // end mapval_package
