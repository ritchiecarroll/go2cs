namespace go.NamedValueTypeClash;

partial class mapval_package {

partial struct Value /*map[@string, nint]*/;

public static nint Count(this Value v) { return len(v); }

public static nint Get(this Value v, @string k) { return v[k]; }

public static void Put(this ref Value v, @string k, nint x) { (v)[k] = x; }

} // end mapval_package
