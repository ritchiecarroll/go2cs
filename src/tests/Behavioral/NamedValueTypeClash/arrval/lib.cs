namespace go.NamedValueTypeClash;

using static arrval_package;

partial class arrval_package {

partial struct Value /*[3]nint*/;

} partial class arrvalᴛValue {

public static nint First(this Value v) {
    v = v.Clone();

    return v[0];
}

public static void Set(this ref Value v, nint i, nint x) { v.Value‿[i] = x; }

public static void Bump(this ref Value v) {
    foreach (var i in range(3)) {
        v.Value‿[i]++;
    }
}

public static nint /*s*/ Sum(this ref Value v) {
    nint s = default;

    foreach (var (_, x) in range(v.Value‿)) {
        s += x;
    }
    return s;
}

public static slice<nint> Head(this ref Value v) { return v.Value‿[..2]; }

} // end arrvalᴛValue
