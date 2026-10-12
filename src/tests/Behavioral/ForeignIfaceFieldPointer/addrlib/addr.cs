namespace go.ForeignIfaceFieldPointer;

partial class addrlib_package {

partial interface Addr {
    @string Network();
    @string String();
}

partial struct UnixAddr {
    public @string Name;
    public @string Net;
}

public static @string Network(this ref UnixAddr a) => a.Net;
public static @string String(this ref UnixAddr a) => a.Name;

} // end addrlib_package
