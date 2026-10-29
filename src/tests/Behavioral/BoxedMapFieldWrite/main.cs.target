namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct Header /*map[@string, slice<@string>]*/;

partial struct Request {
    public Header Header;
    public map<@string, nint> Count;
    public slice<@string> Body;
}

internal static void Main() {
    var req = Ꮡ(new Request(Header: new Header(new map<@string, slice<@string>>{}), Count: new map<@string, nint>{}, Body: slice<@string>(["a", "b"])));

    req.Value.Header["Accept"] = slice<@string>(["text/html", "text/plain"]);
    req.Value.Header["X-Tag"] = slice<@string>(["a"]);

    req.Value.Count["hits"] = (~req).Count["hits"] + 1;

    req.Value.Count["hits"] += 2;
    req.Value.Count["hits"]++;

    req.Value.Body[1] = "z";

    fmt.Println((~req).Header["Accept"]);
    fmt.Println((~req).Header["X-Tag"]);
    fmt.Println((~req).Count["hits"]);
    fmt.Println((~req).Body[0], (~req).Body[1]);
}

} // end main_package
