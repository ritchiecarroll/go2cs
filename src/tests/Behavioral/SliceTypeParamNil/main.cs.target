namespace go;

using fmt = fmt_package;

partial class main_package {

internal static bool isNilSlice<S, E>(S s)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, ISliceWrap<S, E>, new()
{
    return s.IsNil;
}

internal static bool isSetSlice<S, E>(S s)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, ISliceWrap<S, E>, new()
{
    return !s.IsNil;
}

internal static S cloneOrNil<S, E>(S s)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, ISliceWrap<S, E>, new()
{
    if (s.IsNil) {
        return default!;
    }
    return appendꓸꓸꓸ<S, E>(new S{}, s);
}

[GoType("[]nint")] partial struct Named;

internal static void Main() {
    slice<nint> nilSlice = default!;
    var emptyNotNil = new nint[]{}.slice();
    var filled = new nint[]{3, 1, 2}.slice();
    fmt.Println(isNilSlice<slice<nint>, nint>(nilSlice), isNilSlice<slice<nint>, nint>(emptyNotNil), isNilSlice<slice<nint>, nint>(filled));
    fmt.Println(isSetSlice<slice<nint>, nint>(nilSlice), isSetSlice<slice<nint>, nint>(emptyNotNil), isSetSlice<slice<nint>, nint>(filled));
    fmt.Println(cloneOrNil<slice<nint>, nint>(nilSlice) == default!, cloneOrNil<slice<nint>, nint>(emptyNotNil) == default!, len(cloneOrNil<slice<nint>, nint>(filled)));
    Named nilNamed = default!;
    fmt.Println(isNilSlice<Named, nint>(nilNamed), isNilSlice<Named, nint>(new Named(new nint[]{}.slice())), isSetSlice<Named, nint>(new Named(new nint[]{7}.slice())));
}

} // end main_package
