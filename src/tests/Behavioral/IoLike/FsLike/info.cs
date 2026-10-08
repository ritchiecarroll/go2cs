namespace go.IoLike;

partial class FsLike_package {

partial struct Info {
    public @string Name;
    public nint Size;
}

public static Info NewInfo(@string name, nint size) {
    return new Info(Name: name, Size: size);
}

} // end FsLike_package
