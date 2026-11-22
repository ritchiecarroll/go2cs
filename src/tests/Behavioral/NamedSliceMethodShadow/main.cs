namespace go;

using fmt = fmt_package;
using static main_package;

partial class main_package {

partial struct Stack /*[]nint*/;

} partial class mainᴛStack {

public static Stack Slice(this Stack s, nint i, nint n) { return s.slice(i, n); }

public static @string ToSpan(this Stack s) { return fmt.Sprint("span of ", len(s)); }

public static nint GetEnumerator(this Stack s) { return -len(s); }

public static nint Append(this Stack s, slice<nint> xs) { return len(s) + len(xs); }

public static nint Length(this Stack s) { return 100 + len(s); }
public static nint Capacity(this Stack s) { return 200 + cap(s); }
public static nint Low(this Stack s) { return -1; }
public static nint High(this Stack s) { return -2; }
public static nint Available(this Stack s) { return -3; }
public static @string IsNil(this Stack s) { return "never nil"; }
public static @string Source(this Stack s) { return "stack source"; }

public static Stack Make(this Stack s, nint n) { return new Stack(n, n * 2); }
public static @string Wrap(this Stack s) { return "wrapped"; }

} partial class main_package {

partial struct Vec /*[4]nint*/;

} partial class mainᴛVec {

public static slice<nint> Slice(this Vec v, nint i, nint n) {
    v = v.Clone();

    return v.slice(i, n);
}

public static @string ToSpan(this Vec v) {
    v = v.Clone();

    return "vec span";
}

public static @string GetEnumerator(this Vec v) {
    v = v.Clone();

    return "vec enumerator";
}

public static nint Length(this Vec v) {
    v = v.Clone();

    return 40;
}

public static @string Source(this Vec v) {
    v = v.Clone();

    return "vec source";
}

public static nint Make(this Vec v) {
    v = v.Clone();

    return 4;
}

} partial class main_package {

partial struct V /*Vec*/;

public static V Clone(this V v) {
    v = v.ΔClone();

    v[0] = -1;
    return v.ΔClone();
}

public static slice<nint> Slice(this V v, nint i, nint n) {
    v = v.ΔClone();

    return v.slice(i, n);
}

partial struct Near /*[]nint*/;

public static nint Lengths(this Near n) { return 1000 + len(n); }
public static nint Slices(this Near n) { return 2000; }
public static nint Sourced(this Near n) { return 3000; }
public static nint Appends(this Near n) { return 4000; }
public static nint Capacitys(this Near n) { return 5000; }
public static nint ToSpans(this Near n) { return 6000; }

partial struct NearArr /*[3]nint*/;

public static nint Slicer(this NearArr a) {
    a = a.Clone();

    return 7000;
}

public static nint Len(this NearArr a) {
    a = a.Clone();

    return 8000;
}

partial struct Capacity /*[]@string*/;
partial struct Length /*[2]nint*/;

internal static void main() {
    var s = new Stack(new nint[]{10, 20, 30, 40, 50}.slice());
    var t = s.Slice(1, 3);
    fmt.Println("Slice:", t, len(t), cap(t));
    fmt.Println("ToSpan:", s.ToSpan());
    fmt.Println("GetEnumerator:", s.GetEnumerator());
    fmt.Println("Append:", s.Append([1, 2]));
    fmt.Println("Length/Capacity:", s.Length(), s.Capacity());
    fmt.Println("Low/High/Available:", s.Low(), s.High(), s.Available());
    fmt.Println("IsNil/Source:", s.IsNil(), s.Source());
    var m = s.Make(3);
    fmt.Println("Make:", len(m), cap(m));
    fmt.Println("Wrap:", s.Wrap());
    
    var sʗ1 = s;
    var lf = () => sʗ1.Length();
    
    var sʗ2 = sʗ1;
    var sf = () => sʗ2.Source();
    
    var sʗ3 = sʗ2;
    var cf = () => sʗ3.Capacity();
    fmt.Println("method values:", lf(), sf(), cf());

    fmt.Println("builtin len/cap:", len(s), cap(s), s[2], s[1..4]);
    foreach (var (i, x) in range<nint>(s[3..])) {
        fmt.Println("range:", i, x);
    }

    var v = new Vec(new nint[]{1, 2, 3, 4}.array());
    fmt.Println("Vec Slice:", v.Slice(1, 3));
    fmt.Println("Vec ToSpan/GetEnumerator:", v.ToSpan(), v.GetEnumerator());
    fmt.Println("Vec Length/Source/Make:", v.Length(), v.Source(), v.Make());
    
    var vʗ1 = v;
    var vl = () => vʗ1.Length();
    fmt.Println("Vec method value:", vl(), len(v));

    var w = new V(new nint[]{5, 6, 7, 8}.array());
    var w2 = w.ΔClone();
    w2[0] = 50;
    fmt.Println("V value copy:", w[0], w2[0]);
    fmt.Println("V Slice:", w.Slice(1, 3), "V Clone:", w.Clone()[0], w[0]);

    var nr = new Near(new nint[]{1, 2, 3}.slice());
    nr = append(nr, (nint)4);
    fmt.Println("near-miss methods:", nr.Lengths(), nr.Slices(), nr.Sourced(), nr.Appends(), nr.Capacitys(), nr.ToSpans());
    fmt.Println("near-miss wrapper:", len(nr), cap(nr) >= 4, nr[1..3], nr[len(nr) - 1]);
    foreach (var (i, x) in range<nint>(nr[2..])) {
        fmt.Println("near-miss range:", i, x);
    }
    var na = new NearArr(new nint[]{9, 8, 7}.array());
    fmt.Println("near-miss array:", na.Slicer(), na.Len(), len(na), na[1..]);

    var c = new Capacity(new @string[]{"a", "b"}.slice());
    c = append(c, (@string)"c");
    fmt.Println("type Capacity:", c, len(c));
    Length l = default;
    l[1] = 9;
    fmt.Println("type Length:", l, len(l));
}

} // end main_package
