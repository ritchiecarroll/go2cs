namespace go.NamedSliceChildPkg;

using IoLike = IoLike_package;
using FsLike = go.IoLike.FsLike_package;
using go.IoLike;

partial class netlike_package {

partial struct InfoList /*[]global::go.IoLike.FsLike_package.Info*/;

public static @string Describe() {
    return IoLike.Version();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string alphaˢ = "alpha"u8;
private static readonly @string betaˢ = "beta"u8;

public static InfoList Build() {
    return new InfoList(new FsLike.Info[]{
        FsLike.NewInfo(alphaˢ, 3),
        FsLike.NewInfo(betaˢ, 5)
    }.slice());
}

public static @string ElementName(InfoList infos, nint i) {
    return infos[i].Name;
}

public static nint TotalSize(InfoList infos) {
    nint sum = 0;
    foreach (var (_, info) in infos) {
        sum += info.Size;
    }
    return sum;
}

} // end netlike_package
