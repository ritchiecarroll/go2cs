namespace go;

using fmt = fmt_package;
using arrval = NamedValueTypeClash.arrval_package;
using mapval = NamedValueTypeClash.mapval_package;
using numval = NamedValueTypeClash.numval_package;
using NamedValueTypeClash;
using static main_package;

partial class main_package {

partial struct Value /*[]byte*/;

} partial class mainᴛValue {

public static nint Len(this Value v) { return len(v); }

public static byte At(this Value v, nint i) { return v[i]; }

public static void Add(this ref Value v, byte b) { v = append(v, b); }

public static void Set(this ref Value v, nint i, byte b) { (v)[i] = b; }

} partial class main_package {

internal static void main() {
    var v = (Value)slice<byte>((@string)"abc"u8);
    v.Add((byte)'d');
    v.Set(0, (byte)'A');
    fmt.Println(v.Len(), (@string)(slice<byte>)v, v.At(3), (@string)(slice<byte>)v[1..3]);

    numval.Value n = (numval.Value)3;
    n.Inc();
    fmt.Println(n, n.Double(), numval.Conv((numval.Other)4), numval.Back(6));

    var a = new arrval.Value(new nint[]{1, 2, 3}.array());
    a.Set(0, 5);
    a.Bump();
    fmt.Println(a, a.First(), a.Sum(), a.Head());

    var m = new mapval.Value(new map<@string, nint>{["a"] = 1});
    m.Put("b", 2);
    fmt.Println(m.Count(), m.Get("b"), m.Get("z"));
}

} // end main_package
