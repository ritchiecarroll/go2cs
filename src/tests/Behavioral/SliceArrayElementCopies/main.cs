namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct pair {
    internal array<nint> a = new(2);
    internal nint n;
}

partial struct grid /*[]array<nint>*/;

internal static void main() {
    copyArms();
    overlapArms();
    appendArms();
    structArms();
    otherArms();
}

internal static void copyArms() {
    var src = GoReflect.WithElemDims(new array<nint>[]{array<nint>([1, 2]), array<nint>([3, 4])}.slice(), 2);
    var dst = GoReflect.WithElemDims(new slice<array<nint>>(2, () => new(2)), 2);
    copy(dst, src);
    src[0].Assign(new nint[]{}.array(2));
    src[1][1] = 9;
    fmt.Println("copy, then writes to src:", dst, src);

    dst[0][0] = 7;
    fmt.Println("copy, then a write to dst:", dst, src);

    var @in = GoReflect.WithElemDims(new slice<array<nint>>(1, () => new(2)), 2);
    var q = @in[0][..];
    copy(@in, GoReflect.WithElemDims(new array<nint>[]{array<nint>([5, 6])}.slice(), 2));
    fmt.Println("slice of an element taken before the copy:", q, @in);
    @in[0][1] = 8;
    fmt.Println("...and after a write through the element:", q, @in);

    var n1 = GoReflect.WithElemDims(new array<array<nint>>[]{new array<nint>[]{array<nint>([1, 2]), array<nint>([3, 4])}.array()}.slice(), 2, 2);
    var n2 = GoReflect.WithElemDims(new slice<array<array<nint>>>(1, () => new(2, () => new(2))), 2, 2);
    copy(n2, n1);
    n1[0][1][0] = 9;
    fmt.Println("nested arrays:", n2, n1);

    array<array<nint>> arr = new(2, () => new(2));
    arr[0].Assign(array<nint>([1, 1]));
    var from = GoReflect.WithElemDims(new slice<array<nint>>(2, () => new(2)), 2);
    copy(from, GoReflect.WithElemDims(arr[..], 2));
    arr[0][0] = 5;
    fmt.Println("from an array:", from, arr);

    var g = new grid(GoReflect.WithElemDims(new array<nint>[]{array<nint>([1, 2])}.slice(), 2));
    var h = new grid(GoReflect.WithElemDims(new slice<array<nint>>(1, () => new(2)), 2));
    copy(h, g);
    g[0][0] = 9;
    fmt.Println("named slice:", h, g);
}

internal static void overlapArms() {
    var up = GoReflect.WithElemDims(new array<nint>[]{array<nint>([1, 2]), array<nint>([3, 4]), array<nint>([5, 6])}.slice(), 2);
    copy(GoReflect.WithElemDims(up[1..], 2), up);
    fmt.Println("overlap, destination above:", up);
    up[0][0] = 9;
    up[1][1] = 8;
    fmt.Println("...then writes to two elements:", up);

    var down = GoReflect.WithElemDims(new array<nint>[]{array<nint>([1, 2]), array<nint>([3, 4]), array<nint>([5, 6])}.slice(), 2);
    copy(down, GoReflect.WithElemDims(down[1..], 2));
    fmt.Println("overlap, destination below:", down);
    down[2][0] = 9;
    down[1][1] = 8;
    fmt.Println("...then writes to two elements:", down);
}

internal static void appendArms() {
    var src = GoReflect.WithElemDims(new array<nint>[]{array<nint>([3, 4]), array<nint>([5, 6])}.slice(), 2);
    var cl = appendꓸꓸꓸ(slice<array<nint>>(default), src);
    src[0].Assign(new nint[]{}.array(2));
    src[1][0] = 9;
    fmt.Println("append to nil, then writes to src:", cl, src);

    var old = GoReflect.WithElemDims(new array<nint>[]{array<nint>([1, 2])}.slice(), 2);
    var grown = append(old, array<nint>([3, 4]));
    old[0][1] = 9;
    fmt.Println("growth, then a write to the old slice:", grown, old);
    grown[0][0] = 7;
    fmt.Println("...and a write to the grown one:", grown, old);

    var more = GoReflect.WithElemDims(new array<nint>[]{array<nint>([5, 6])}.slice(), 2);
    var grown2 = appendꓸꓸꓸ(old, more);
    more[0][1] = 0;
    old[0][0] = 0;
    fmt.Println("growth by a spread, then writes to both sources:", grown2, old, more);

    var @base = GoReflect.WithElemDims(new slice<array<nint>>(1, () => new(2), 4), 2);
    var add = GoReflect.WithElemDims(new array<nint>[]{array<nint>([1, 2])}.slice(), 2);
    var @in = appendꓸꓸꓸ(@base, add);
    add[0][0] = 9;
    fmt.Println("in capacity, then a write to the spread:", @in, add);

    var packed = addAll(default, src.ꓸꓸꓸ);
    src[0][1] = 5;
    fmt.Println("variadic pack appended:", packed, src);

    var filled = GoReflect.WithElemDims(new slice<array<nint>>(2, () => new(2)), 2);
    fill(filled, src.ꓸꓸꓸ);
    src[1][1] = 4;
    fmt.Println("variadic pack copied:", filled, src);

    var one = array<nint>([1, 2]);
    var single = append(GoReflect.WithElemDims(new array<nint>[]{}.slice(), 2), one.Clone());
    one[0] = 9;
    fmt.Println("one element:", single, one);
}

internal static slice<array<nint>> addAll(slice<array<nint>> dst, params variadic<array<nint>> moreʗp) {
    var more = moreʗp.sslice();

    return appendꓸꓸꓸ(dst, more);
}

internal static void fill(slice<array<nint>> dst, params variadic<array<nint>> vʗp) {
    var v = vʗp.sslice();

    copy(dst, v);
}

internal static void structArms() {
    var src = slice<pair>([new(array<nint>([1, 2]), 1), new(array<nint>([3, 4]), 2)]);
    var dst = new slice<pair>(2, () => new());
    copy(dst, src);
    src[0].a[1] = 9;
    src[1].AssignFrom(new pair());
    fmt.Println("structs copied, then writes to src:", dst, src);

    var cl = appendꓸꓸꓸ(slice<pair>(default), dst);
    dst[0].a[0] = 7;
    fmt.Println("structs appended to nil, then a write to the source:", cl, dst);

    var old = slice<pair>([new(array<nint>([1, 2]), 1)]);
    var grown = append(old, new pair(array<nint>([3, 4]), 2));
    old[0].a[1] = 9;
    fmt.Println("structs grown, then a write to the old slice:", grown, old);
}

internal static void otherArms() {
    var s = GoReflect.WithElemDims(new array<nint>[]{array<nint>([1, 2]), array<nint>([3, 4])}.slice(), 2);
    var a = new array<array<nint>>(s, 2);
    s[0][0] = 9;
    fmt.Println("slice to array:", a, s);

    var dst = GoReflect.WithElemDims(new slice<array<nint>>(1, () => new(2)), 2);
    var p = Ꮡ(dst, 0);
    copy(dst, GoReflect.WithElemDims(new array<nint>[]{array<nint>([4, 5])}.slice(), 2));
    p.Value[1] = 6;
    fmt.Println("element pointer:", p.Value, dst);
}

} // end main_package
