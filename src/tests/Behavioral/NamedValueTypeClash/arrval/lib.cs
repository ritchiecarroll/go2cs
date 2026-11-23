namespace go.NamedValueTypeClash;

partial class arrval_package {

partial struct Value /*[3]nint*/;

internal static nint First⁔(/*this*/ Value v) {
    v = v.Clone();

    return v[0];
}

internal static void Set⁔(/*this*/ ref Value v, nint i, nint x) { v.Value‿[i] = x; }

internal static void Bump⁔(/*this*/ ref Value v) {
    foreach (var i in range(3)) {
        v.Value‿[i]++;
    }
}

internal static nint /*s*/ Sum⁔(/*this*/ ref Value v) {
    nint s = default;

    foreach (var (_, x) in range(v.Value‿)) {
        s += x;
    }
    return s;
}

internal static slice<nint> Head⁔(/*this*/ ref Value v) { return v.Value‿[..2]; }

} // end arrval_package
