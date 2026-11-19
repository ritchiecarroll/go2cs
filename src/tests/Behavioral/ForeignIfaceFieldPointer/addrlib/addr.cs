namespace go.ForeignIfaceFieldPointer;

using static addrlib_package;

partial class addrlib_package {

partial interface Addr {
    @string Network();
    @string String();
}

partial struct UnixAddr {
    public @string Name;
    public @string Net;
}

} partial class addrlibᴛUnixAddr {

public static @string Network(this ref UnixAddr a) { return a.Net; }
public static @string String(this ref UnixAddr a) { return a.Name; }

} // end addrlibᴛUnixAddr
