namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct Header /*map[@string, slice<@string>]*/;

partial struct Request {
    public Header Header;
    public map<@string, nint> Count;
    public slice<@string> Body;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string acceptˢ = "Accept"u8;
private static readonly @string xTagˢ = "X-Tag"u8;
private static readonly @string hitsˢ = "hits"u8;

internal static void Main() {
    var req = Ꮡ(new Request(Header: new Header(new map<@string, slice<@string>>{}), Count: new map<@string, nint>{}, Body: new @string[]{"a"u8, "b"u8}.slice()));
    req.Value.Header[acceptˢ] = new @string[]{"text/html"u8, "text/plain"u8}.slice();
    req.Value.Header[xTagˢ] = new @string[]{"a"u8}.slice();
    req.Value.Count[hitsˢ] = (~req).Count[hitsˢ] + 1;
    req.Value.Count[hitsˢ] += 2;
    req.Value.Count[hitsˢ]++;
    req.Value.Body[1] = "z"u8;
    fmt.Println((~req).Header[acceptˢ]);
    fmt.Println((~req).Header[xTagˢ]);
    fmt.Println((~req).Count[hitsˢ]);
    fmt.Println((~req).Body[0], (~req).Body[1]);
}

} // end main_package
