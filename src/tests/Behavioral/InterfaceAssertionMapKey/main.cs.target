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

internal static void Main() {
    var objMap = new map<Object, nint>();
    objMap[new myVarжObject(Ꮡ(new myVar("var-a")))] = 1;
    objMap[new myVarжObject(Ꮡ(new myVar("var-b")))] = 2;
    objMap[new myConst("const-c")] = 4;
    objMap[new myConst("const-d")] = 8;
    objMap[new myFuncжObject(Ꮡ(new myFunc("func-e")))] = 16;

    var M = new map<dependency, bool>();
    foreach (var (obj, _) in objMap) {
        {
            var (objΔ1, _) = obj._<dependency>(ᐧ); if (objΔ1 != default!) {
                M[objΔ1] = true;
            }
        }
    }
    fmt.Println("objMap:", len(objMap), "M:", len(M));

    nint hits = 0;
    nint sum = 0;
    foreach (var (d, _) in M) {
        var (v, ok) = objMap[d, ꟷ];
        if (!ok) {
            fmt.Println("MISS:", d.Name());
            continue;
        }
        hits++;
        sum += v;
    }
    fmt.Println("hits:", hits, "sum:", sum);

    foreach (var (d, _) in M) {
        Object o = d;
        {
            var (_, ok) = objMap[o, ꟷ]; if (!ok) {
                fmt.Println("MISS after re-widening:", d.Name());
            }
        }
    }

    foreach (var (obj, _) in objMap) {
        var (d, ok) = obj._<dependency>(ᐧ);
        if (!ok) {
            continue;
        }
        if (!AreEqual(((Object)d), obj)) {
            fmt.Println("IDENTITY LOST:", obj.Name());
        }
    }
    fmt.Println("done");
}

} // end main_package
