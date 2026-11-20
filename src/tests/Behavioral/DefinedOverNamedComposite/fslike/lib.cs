namespace go.DefinedOverNamedComposite;

using static fslike_package;

partial class fslike_package {

partial struct MapFS /*map[@string, nint]*/;

} partial class fslikeᴛMapFS {

public static nint Get(this MapFS m, @string k) { return m[k]; }

public static nint Size(this MapFS m) { return len(m); }

} partial class fslike_package {

partial struct List /*[]nint*/;

} partial class fslikeᴛList {

public static nint Sum(this List l) {
    nint t = 0;
    foreach (var (_, v) in range<nint>(l)) {
        t += v;
    }
    return t;
}

} partial class fslike_package {

partial struct Buf /*[2]nint*/;

} partial class fslikeᴛBuf {

public static nint First(this Buf b) {
    b = b.Clone();

    return b[0];
}

} // end fslikeᴛBuf
