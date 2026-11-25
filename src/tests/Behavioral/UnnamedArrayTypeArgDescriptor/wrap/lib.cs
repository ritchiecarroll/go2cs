namespace go.UnnamedArrayTypeArgDescriptor;

using unique = unique_package;

partial class wrap_package {

public static unique.Handle<T> Wrap<T>(T v) => Wrap<T, ᴀnone>(v);
[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static unique.Handle<T> Wrap<T, Tᴀ>(T v)
    where Tᴀ : IGoArrayDims
{
    return unique.Make<T, Tᴀ>(v);
}

public static bool Same<T>(T a, T b) => Same<T, ᴀnone>(a, b);
[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static bool Same<T, Tᴀ>(T a, T b)
    where Tᴀ : IGoArrayDims
{
    return Wrap<T, Tᴀ>(a) == Wrap<T, Tᴀ>(b);
}

} // end wrap_package
