namespace go;

using fmt = fmt_package;
using static main_package;

partial class main_package {

partial struct Kind /*num:int32*/;

partial struct Set<T> {
    internal map<T, EmptyStruct> m;
}

} partial class mainᴛSet {

public static bool AddIfAbsent<T>(this ref Set<T> s, T v) {
    var (_, ok) = s.m[v, ꟷ]; if (ok) {
        return false;
    }

    if (s.m == nil) {
        s.m = new map<T, EmptyStruct>{};
    }

    s.m[v] = new EmptyStruct();
    return true;
}

public static nint Len<T>(this ref Set<T> s) { return len(s.m); }

} partial class main_package {

partial struct Pair<K, V> {
    public K Key;
    public V Val;
}

internal partial struct symbols_seen /*dyn*/ {
    internal @string name;
    internal Kind kind;
}

internal static void symbols() {
    Set<symbols_seen> seen = default;

    foreach (var (_, n) in range(slice<@string>(["a", "b", "a"]))) {
        var key = new symbols_seen(n, 1);

        fmt.Println("add", n, seen.AddIfAbsent(key));
    }

    fmt.Println("symbols:", seen.Len());
}

internal static void again() {
    Set<symbols_seen> other = default;

    var key = new symbols_seen("z", 2);

    other.AddIfAbsent(key);

    fmt.Println("again:", other.Len());
}

internal partial struct composed_ps /*dyn*/ {
    internal nint id;
}

internal partial struct composed_p /*dyn*/ {
    internal nint x, y;
}

internal static void composed() {
    ж<Set<composed_ps>> ps = Ꮡ(new Set<composed_ps>());
    ps.AddIfAbsent(new composed_ps(7));
    ps.AddIfAbsent(new composed_ps(7));
    fmt.Println("pointer:", ps.Len());

    Pair<@string, composed_p> p = default;

    p.Key = "origin";
    p.Val.x = 3;
    fmt.Printf("pair: %s %d %d %+v\n"u8, p.Key, p.Val.x, p.Val.y, p);
}


partial struct registryᴛ1 /*dyn*/ {
    internal @string pkg;
    internal nint line;
}
internal static readonly ж<Set<registryᴛ1>> Ꮡregistry = new StaticBox<Set<registryᴛ1>>(default(Set<registryᴛ1>));
internal static ref Set<registryᴛ1> registry => ref ((StaticBox<Set<registryᴛ1>>)Ꮡregistry).ValueSlot;

internal static void main() {
    symbols();
    again();
    composed();

    var entry = new registryᴛ1("main", 1);

    registry.AddIfAbsent(entry);

    fmt.Println("registry:", registry.Len());
}

} // end main_package
