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

internal static @string Network⁔(/*this*/ ref UnixAddr a) { return a.Net; }
internal static @string String⁔(/*this*/ ref UnixAddr a) { return a.Name; }

} // end addrlib_package
