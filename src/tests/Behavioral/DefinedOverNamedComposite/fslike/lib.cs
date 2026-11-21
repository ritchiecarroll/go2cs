namespace go.DefinedOverNamedComposite;

partial class fslike_package {

partial struct MapFS /*map[@string, nint]*/;

internal static nint Get⁔(/*this*/ MapFS m, @string k) { return m[k]; }

internal static nint Size⁔(/*this*/ MapFS m) { return len(m); }

partial struct List /*[]nint*/;

internal static nint Sum⁔(/*this*/ List l) {
    nint t = 0;
    foreach (var (_, v) in range<nint>(l)) {
        t += v;
    }
    return t;
}

partial struct Buf /*[2]nint*/;

internal static nint First⁔(/*this*/ Buf b) {
    b = b.Clone();

    return b[0];
}

} // end fslike_package
