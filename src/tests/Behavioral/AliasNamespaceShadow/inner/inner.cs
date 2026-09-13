namespace go.AliasNamespaceShadow;

using sort = go.sort_package;
using local = go.AliasNamespaceShadow.sort_package;
using go;

partial class inner_package {

public static slice<nint> SortThree() {
    var s = new nint[]{3, 1, 2}.slice();
    sort.Ints(s);
    return s;
}

public static @string LocalTag() {
    return local.Tag();
}

} // end inner_package
