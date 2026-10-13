namespace go;

using fmt = fmt_package;
using netlike = NamedSliceChildPkg.netlike_package;
using FsLike = IoLike.FsLike_package;
using NamedSliceChildPkg;

partial class main_package {

internal static void Main() {
    fmt.Println(netlike.Describe());

    var infos = netlike.Build();
    fmt.Println(len(infos));
    fmt.Println(netlike.ElementName(infos, 0));
    fmt.Println(netlike.TotalSize(infos));

    var tail = infos[1..];
    fmt.Println(netlike.TotalSize(tail));
}

} // end main_package
