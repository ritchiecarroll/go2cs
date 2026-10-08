namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct fieldElement /*num:uint16*/;

partial struct ringElement /*[4]fieldElement*/;

partial struct nttElement /*[4]fieldElement*/;

internal static T /*s*/ addPoly<T>(T a, T b)
    where T : /* ~[4]go2cs/GenericArrayConstraint.fieldElement */ IArray<fieldElement>, new()
{
    T s = GoZero<T>();

    foreach (var (i, _) in s) {
        s[i] = (fieldElement)(a[i] + b[i]);
    }
    return s;
}

internal static T scalePoly<T>(T a, fieldElement k)
    where T : /* ~[4]go2cs/GenericArrayConstraint.fieldElement */ IArray<fieldElement>, new()
{
    T f = GoZero<T>();
    foreach (var (i, _) in a) {
        f[i] = (fieldElement)(a[i] * k);
    }
    return f;
}

internal static fieldElement sum<T>(T a)
    where T : /* ~[4]go2cs/GenericArrayConstraint.fieldElement */ IArray<fieldElement>, new()
{
    fieldElement total = default!;
    foreach (var (_, x) in a) {
        total += x;
    }
    return total;
}

internal static void Main() {
    var r1 = new ringElement(new fieldElement[]{1, 2, 3, 4}.array());
    var r2 = new ringElement(new fieldElement[]{10, 20, 30, 40}.array());
    var rs = addPoly(r1, r2);
    fmt.Println(rs[0], rs[1], rs[2], rs[3], sum(rs));
    var n1 = new nttElement(new fieldElement[]{5, 6, 7, 8}.array());
    var n2 = new nttElement(new fieldElement[]{50, 60, 70, 80}.array());
    var ns = addPoly(n1, n2);
    fmt.Println(ns[0], ns[1], ns[2], ns[3], sum(ns));
    var scaled = scalePoly(r1, 3);
    fmt.Println(scaled[0], scaled[1], scaled[2], scaled[3]);
}

} // end main_package
