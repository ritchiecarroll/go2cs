namespace go.TypeAssertionSource;

using io = io_package;
using lib = go.TypeAssertionSource.lib_package;
using go.TypeAssertionSource;

partial class mid_package {

public static lib.RW Get() {
    return new lib.ImplжRW(Ꮡ(new lib.Impl(nil)));
}

partial struct Other {
}

public static (nint, error) Read(this ref Other _, slice<byte> p) {
    return (0, io.EOF);
}

public static (nint, error) Write(this ref Other _, slice<byte> p) {
    return (len(p), default);
}

} // end mid_package
