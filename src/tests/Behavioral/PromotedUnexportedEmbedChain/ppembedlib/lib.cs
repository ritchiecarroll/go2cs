namespace go.go;

using static ppembedlib_package;

partial class ppembedlib_package {

partial struct BaseNode {
    internal slice<@string> children;
}

} partial class ppembedlibᴛBaseNode {

public static void Add(this ref BaseNode n, @string s) { n.children = append(n.children, s); }
public static nint Count(this ref BaseNode n) { return len(n.children); }
public static @string Last(this ref BaseNode n) { return n.children[len(n.children) - 1]; }

} partial class ppembedlib_package {

partial struct BaseInline {
    /*embed*/ public BaseNode BaseNode;
}

partial struct baseLink {
    /*embed*/ public BaseInline BaseInline;
    public @string Dest;
}

partial struct Link {
    /*embed*/ internal baseLink baseLink;
}

partial struct inner {
    /*embed*/ public BaseNode BaseNode;
}

partial struct Inline {
    /*embed*/ internal inner inner;
}

partial struct Image {
    /*embed*/ public Inline Inline;
}

partial struct Para {
    /*embed*/ public BaseNode BaseNode;
}

partial struct emphBase {
    /*embed*/ public BaseNode BaseNode;
}

partial struct Emph {
    /*embed*/ internal emphBase emphBase;
}

partial struct tagBase {
    internal nint bumps;
}

} partial class ppembedlibᴛtagBase {

internal static nint Bump(this ref tagBase t) {
    t.bumps++;
    return t.bumps;
}

} partial class ppembedlib_package {

partial struct Tag {
    /*embed*/ internal tagBase tagBase;
}

public static ж<Link> NewLink(@string dest) {
    var l = Ꮡ(new Link());
    l.Dest = dest;
    return l;
}

} // end ppembedlib_package
