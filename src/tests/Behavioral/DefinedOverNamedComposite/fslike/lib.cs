namespace go.DefinedOverNamedComposite;

partial class fslike_package {

partial struct MapFS /*map[@string, nint]*/;

public static nint Get(this MapFS m, @string k) {
    return m[k];
}

public static nint Size(this MapFS m) {
    return len(m);
}

partial struct List /*[]nint*/;

public static nint Sum(this List l) {
    nint t = 0;
    foreach (var (_, v) in l) {
        t += v;
    }
    return t;
}

partial struct Buf /*[2]nint*/;

public static nint First(this Buf b) {
    b = b.Clone();

    return b[0];
}

} // end fslike_package
