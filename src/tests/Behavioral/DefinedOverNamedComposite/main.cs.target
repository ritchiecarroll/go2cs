namespace go;

using fmt = fmt_package;
using fslike = DefinedOverNamedComposite.fslike_package;
using DefinedOverNamedComposite;

partial class main_package {

partial struct shuffledFS /*global::go.DefinedOverNamedComposite.fslike_package.MapFS*/;

internal static nint get(this shuffledFS f, @string k) {
    return ((fslike.MapFS)f).Get(k);
}

partial struct shuffledList /*global::go.DefinedOverNamedComposite.fslike_package.List*/;

partial struct shuffledBuf /*global::go.DefinedOverNamedComposite.fslike_package.Buf*/;

partial struct localMap /*global::go.DefinedOverNamedComposite.fslike_package.MapFS*/;

partial struct headerA /*map[@string, nint]*/;

partial struct headerB /*map[@string, nint]*/;

internal static void Main() {
    var m = new fslike.MapFS(new map<@string, nint>{["a"u8] = 1, ["b"u8] = 2});
    var s = ((shuffledFS)m);
    fmt.Println(s.get("a"u8), s.get("b"u8));
    var back = ((fslike.MapFS)s);
    fmt.Println(back.Get("b"u8), back.Size());
    var lm = ((localMap)m);
    fmt.Println(((fslike.MapFS)lm).Get("a"u8), ((fslike.MapFS)lm).Size());
    var l = new fslike.List(new nint[]{3, 1, 2}.slice());
    var sl = ((shuffledList)l);
    fmt.Println(((fslike.List)sl).Sum(), (((fslike.List)sl))[0], len(((fslike.List)sl)));
    var b = new fslike.Buf(new nint[]{7, 8}.array());
    var sb = ((shuffledBuf)b);
    fmt.Println(((fslike.Buf)sb).First(), (((fslike.Buf)sb))[1]);
    var ha = new headerA(new map<@string, nint>{["x"u8] = 9});
    var hb = ((headerB)(map<@string, nint>)ha);
    fmt.Println(hb["x"u8], len(hb), len(((headerA)(map<@string, nint>)hb)));
}

} // end main_package
