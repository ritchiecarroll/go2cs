namespace go;

using fmt = fmt_package;

partial class main_package {

partial interface Object {
    @string Name();
}

partial interface dependency :
    Object
{
    void isDependency();
}

partial struct myVar {
    internal @string n;
}

internal static @string Name(this ref myVar o) {
    return o.n;
}

internal static void isDependency(this ref myVar o) {
}

partial struct myConst {
    internal @string n;
}

internal static @string Name(this myConst o) {
    return o.n;
}

internal static void isDependency(this myConst o) {
}

partial struct myFunc {
    internal @string n;
}

internal static @string Name(this ref myFunc o) {
    return o.n;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object objMapˢ = (@string)"objMap:"u8;
private static readonly object missˢ = (@string)"MISS:"u8;
private static readonly object hitsˢ = (@string)"hits:"u8;
private static readonly object sumˢ = (@string)"sum:"u8;
private static readonly object missAfterReWideningˢ = (@string)"MISS after re-widening:"u8;
private static readonly object identityLostˢ = (@string)"IDENTITY LOST:"u8;
private static readonly object doneˢ = (@string)"done"u8;

internal static void Main() {
    var objMap = new map<Object, nint>();
    objMap[new myVarжObject(Ꮡ(new myVar("var-a"u8)))] = 1;
    objMap[new myVarжObject(Ꮡ(new myVar("var-b"u8)))] = 2;
    objMap[new myConst("const-c"u8)] = 4;
    objMap[new myConst("const-d"u8)] = 8;
    objMap[new myFuncжObject(Ꮡ(new myFunc("func-e"u8)))] = 16;
    var M = new map<dependency, bool>();
    foreach (var (obj, _) in objMap) {
        {
            var (objΔ1, _) = obj._<dependency>(ᐧ); if (objΔ1 != default!) {
                M[objΔ1] = true;
            }
        }
    }
    fmt.Println(objMapˢ, len(objMap), (@string)"M:"u8, len(M));
    nint hits = 0;
    nint sum = 0;
    foreach (var (d, _) in M) {
        var (v, ok) = objMap[d, ꟷ];
        if (!ok) {
            fmt.Println(missˢ, d.Name());
            continue;
        }
        hits++;
        sum += v;
    }
    fmt.Println(hitsˢ, hits, sumˢ, sum);
    foreach (var (d, _) in M) {
        Object o = d;
        {
            var (_, ok) = objMap[o, ꟷ]; if (!ok) {
                fmt.Println(missAfterReWideningˢ, d.Name());
            }
        }
    }
    foreach (var (obj, _) in objMap) {
        var (d, ok) = obj._<dependency>(ᐧ);
        if (!ok) {
            continue;
        }
        if (!AreEqual(((Object)d), obj)) {
            fmt.Println(identityLostˢ, obj.Name());
        }
    }
    fmt.Println(doneˢ);
}

} // end main_package
