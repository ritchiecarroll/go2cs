namespace go;

using fmt = fmt_package;
using arrval = NamedValueTypeClash.arrval_package;
using mapval = NamedValueTypeClash.mapval_package;
using numval = NamedValueTypeClash.numval_package;
using NamedValueTypeClash;

partial class main_package {

partial struct Value /*[]byte*/;

public static nint Len(this Value v) { return len(v); }

public static byte At(this Value v, nint i) { return v[i]; }

public static void Add(this ref Value v, byte b) { v = append(v, b); }

public static void Set(this ref Value v, nint i, byte b) { (v)[i] = b; }

internal static void main() {
    var v = (Value)slice<byte>((@string)"abc"u8);
    v.Add((byte)'d');
    v.Set(0, (byte)'A');
    var evalᴛ1 = v.Len();
    var evalᴛ2 = v.At(3);
    fmt.Println(evalᴛ1, (@string)(slice<byte>)v, evalᴛ2, (@string)(slice<byte>)v[1..3]);

    numval.Value n = (numval.Value)3;
    n.Inc();
    var evalᴛ3 = n.Double();
    var evalᴛ4 = numval.Conv((numval.Other)4);
    var evalᴛ5 = numval.Back(6);
    fmt.Println(n, evalᴛ3, evalᴛ4, evalᴛ5);

    var a = new arrval.Value(new nint[]{1, 2, 3}.array());
    a.Set(0, 5);
    a.Bump();
    var evalᴛ6 = a.First();
    var evalᴛ7 = a.Sum();
    var evalᴛ8 = a.Head();
    fmt.Println(a, evalᴛ6, evalᴛ7, evalᴛ8);

    var m = new mapval.Value(new map<@string, nint>{["a"] = 1});
    m.Put("b", 2);
    fmt.Println(m.Count(), m.Get("b"), m.Get("z"));
}

} // end main_package
