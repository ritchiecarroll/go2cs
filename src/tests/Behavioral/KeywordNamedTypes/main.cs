namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct @fixed {
    internal nint n;
}

partial interface sizer {
    nint size(@string of);
}

partial interface @lock {
    bool held();
}

partial struct @short /*num:int16*/;

partial struct dword /*num:uint32*/;

internal static @short toShort(dword d) {
    return (@short)(int16)d;
}

internal static nint size(this @fixed f, @string of) {
    return f.n + len(of);
}

internal static void grow(this ref @fixed f, nint by) {
    f.n += by;
}

internal static bool held(this @fixed f) {
    return f.n > 3;
}

internal partial struct keywordLocalStruct_params /*dyn*/ {
    internal uintptr size;
    internal uint32 flags;
}

internal static uintptr keywordLocalStruct() {
    keywordLocalStruct_params @params = default;
    @params.size = 8;
    @params.flags = 3;
    return @params.size + (uintptr)@params.flags;
}

internal partial interface keywordLocalIface_params /*dyn*/ {
    bool held();
}

internal static nint keywordLocalIface() {
    keywordLocalIface_params @params = default;
    if (@params == nil) {
        return -1;
    }
    if (@params.held()) {
        return 1;
    }
    return 0;
}

internal partial struct keywordLocalRef_ref /*dyn*/ {
    internal nint n;
}

internal static nint keywordLocalRef() {
    keywordLocalRef_ref @ref = default;
    @ref.n = 7;
    return @ref.n;
}

internal partial struct plainLocalStruct_sizes /*dyn*/ {
    internal uintptr size;
}

internal static uintptr plainLocalStruct() {
    plainLocalStruct_sizes sizes = default;
    sizes.size = 4;
    return sizes.size;
}

internal static sizer std = new @fixed(n: 3);

internal static void Main() {
    fmt.Println(std.size("hello"));

    ref var f = ref heap<@fixed>(new @fixed(n: 1), out var Ꮡf);
    fmt.Println(f.size("ab"));

    f.grow(4);
    fmt.Println(f.size(""));

    sizer p = new fixedжsizer(Ꮡf);
    fmt.Println(p.size("ptr"));

    @lock l = f;
    fmt.Println(l.held());

    @lock lp = new fixedжlock(Ꮡf);
    fmt.Println(lp.held());

    dword d = 40000;
    var s = toShort(d);
    fmt.Println((nint)s, (nint)((dword)(uint32)s));

    fmt.Println(keywordLocalStruct());
    fmt.Println(keywordLocalIface());
    fmt.Println(keywordLocalRef());
    fmt.Println(plainLocalStruct());
}

} // end main_package
