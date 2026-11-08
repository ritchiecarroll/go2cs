namespace go.NamedValueTypeClash;

partial class numval_package {

partial struct Value /*num:nint*/;

public static Value Double(this Value v) { return v * 2; }

public static void Inc(this ref Value v) { v++; }

partial struct Other /*num:nint*/;

public static Value Conv(Other o) { return (Value)(nint)o; }

public static Other Back(Value v) { return (Other)(nint)v; }

} // end numval_package
