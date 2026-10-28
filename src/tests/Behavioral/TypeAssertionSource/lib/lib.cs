namespace go.TypeAssertionSource;

using io = io_package;

partial class lib_package {

partial interface RW :
    io.Reader,
    io.Writer
{
}

partial struct Impl {
}

public static (nint, error) Read(this ref Impl _, slice<byte> p) {
    return (0, io.EOF);
}

public static (nint, error) Write(this ref Impl _, slice<byte> p) {
    return (len(p), default);
}

} // end lib_package
