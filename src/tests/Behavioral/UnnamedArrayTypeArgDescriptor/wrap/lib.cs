namespace go.UnnamedArrayTypeArgDescriptor;

using unique = unique_package;

partial class wrap_package {

public static unique.Handle<T> Wrap<T>(T v) {
    return unique.Make<T>(v);
}

public static bool Same<T>(T a, T b) {
    return Wrap<T>(a) == Wrap<T>(b);
}

} // end wrap_package
