namespace go.NamedValueTypeClash;

using static mapval_package;

partial class mapval_package {

partial struct Value /*map[@string, nint]*/;

} partial class mapvalᴛValue {

public static nint Count(this Value v) { return len(v); }

public static nint Get(this Value v, @string k) { return v[k]; }

public static void Put(this ref Value v, @string k, nint x) { (v)[k] = x; }

} // end mapvalᴛValue
