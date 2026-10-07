namespace go;

using fmt = fmt_package;
using strings = strings_package;

partial class main_package {

[GoType("map[@string, @string]")] partial struct Env;

public static void Set(this Env e, @string k, @string v) {
    e[strings.ToLower(k)] = v;
}

[GoType("map[@string, nint]")] partial struct Bag;

public static void Add(this Bag b, @string k, nint v) {
    b[k] += v;
}

[GoType("map[@string, nint]")] partial struct Cache;

internal static nint cleared;

public static bool Remove(this Cache c, @string k) {
    var (_, ok) = c[k, ꟷ];
    delete(c, k);
    return !ok;
}

public static void Clear(this Cache c) {
    cleared++;
    foreach (var (k, _) in c) {
        delete(c, k);
    }
}

public static bool ContainsKey(this Cache c, @string k) {
    return k == "always"u8;
}

[GoType("map[@string, @string]")] partial struct PEnv;

[GoRecv] public static void Set(this ref PEnv p, @string k, @string v) {
    (p)[strings.ToUpper(k)] = v;
}

[GoType("map[T, EmptyStruct]")] partial struct Uniq<T>;

public static bool Remove<T>(this Uniq<T> u, T item) {
    var (_, ok) = u[item, ꟷ];
    delete(u, item);
    return !ok;
}

[GoType("chan nint")] partial struct Pipe;

internal static nint sends;

public static void Send(this Pipe p, nint v) {
    sends++;
    p.ᐸꟷ(v * 10);
}

[GoType("map[@string, nint]")] partial struct ΔSet;

[GoType("map[@string, slice<@string>]")] partial struct Hdr;

public static void Set(this Hdr h, @string k, @string v) {
    h[strings.ToUpper(k)] = new @string[]{v}.slice();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string keyˢ = "Key"u8;
private static readonly @string keyˢ2 = "key"u8;
private static readonly object exactSetRanˢ = (@string)"exact Set ran:"u8;
private static readonly object exactAddAccumulatedˢ = (@string)"exact Add accumulated:"u8;
private static readonly object exactRemoveˢ = (@string)"exact Remove:"u8;
private static readonly object exactClearˢ = (@string)"exact Clear:"u8;
private static readonly object exactContainsKeyˢ = (@string)"exact ContainsKey:"u8;
private static readonly @string alwaysˢ = "always"u8;
private static readonly @string acceptˢ = "accept"u8;
private static readonly object controlSetˢ = (@string)"control Set:"u8;
private static readonly @string acceptˢ2 = "ACCEPT"u8;
private static readonly @string mixedˢ = "Mixed"u8;
private static readonly @string mixedˢ2 = "mixed"u8;
private static readonly object nestedAssignRawˢ = (@string)"nested assign raw:"u8;
private static readonly object nestedAssignInFieldˢ = (@string)"nested assign in field:"u8;
private static readonly @string lowˢ = "low"u8;
private static readonly object pointerSetˢ = (@string)"pointer Set:"u8;
private static readonly @string lowˢ2 = "LOW"u8;
private static readonly object nestedRawˢ = (@string)"nested raw:"u8;
private static readonly object genericRemoveˢ = (@string)"generic Remove:"u8;
private static readonly object exactSendˢ = (@string)"exact Send:"u8;
private static readonly object namedSetNestedˢ = (@string)"named Set nested:"u8;

[GoType("dyn")] internal partial struct main_named {
    internal map<@string, Env> m;
}

internal static void Main() {
    var e = new Env(new map<@string, @string>{});
    e.Set(keyˢ, "v"u8);
    var (_, lower) = e[keyˢ2, ꟷ];
    var (_, raw) = e[keyˢ, ꟷ];
    fmt.Println(exactSetRanˢ, lower, raw);
    var b = new Bag(new map<@string, nint>{});
    b.Add("n"u8, 2);
    b.Add("n"u8, 3);
    fmt.Println(exactAddAccumulatedˢ, b["n"u8]);
    var c = new Cache(new map<@string, nint>{["a"u8] = 1});
    fmt.Println(exactRemoveˢ, c.Remove("a"u8), c.Remove("a"u8), len(c));
    c["b"u8] = 2;
    c.Clear();
    fmt.Println(exactClearˢ, cleared, len(c));
    fmt.Println(exactContainsKeyˢ, c.ContainsKey(alwaysˢ), c.ContainsKey("b"u8));
    var h = new Hdr(new map<@string, slice<@string>>{});
    h.Set(acceptˢ, "x"u8);
    fmt.Println(controlSetˢ, len(h[acceptˢ2]), len(h[acceptˢ]));
    var envs = new map<@string, Env>{["x"u8] = new map<@string, @string>{}};
    envs["x"u8].Set‿(mixedˢ, "m"u8);
    var (_, nestedRaw) = envs["x"u8][mixedˢ, ꟷ];
    var (_, nestedLower) = envs["x"u8][mixedˢ2, ꟷ];
    fmt.Println(nestedAssignRawˢ, nestedRaw, nestedLower);
    var named = new main_named(m: new map<@string, Env>{["y"u8] = new map<@string, @string>{}});
    named.m["y"u8].Set‿("Q"u8, "q"u8);
    fmt.Println(nestedAssignInFieldˢ, named.m["y"u8]["Q"u8]);
    var pe = new PEnv(new map<@string, @string>{});
    pe.Set(lowˢ, "p"u8);
    var penvs = new map<@string, PEnv>{["z"u8] = new map<@string, @string>{}};
    penvs["z"u8].Set‿(lowˢ, "r"u8);
    fmt.Println(pointerSetˢ, pe[lowˢ2], len(pe), nestedRawˢ, penvs["z"u8][lowˢ], len(penvs["z"u8]));
    var u = new Uniq<@string>(new map<@string, EmptyStruct>{["a"u8] = new()});
    fmt.Println(genericRemoveˢ, u.Remove("a"u8), u.Remove("a"u8), len(u));
    var p = new Pipe(1);
    p.Send(4);
    fmt.Println(exactSendˢ, sends, ᐸꟷ<nint>(p));
    var sets = new map<@string, ΔSet>{["a"u8] = new map<@string, nint>{}};
    sets["a"u8].Set‿("k"u8, 7);
    fmt.Println(namedSetNestedˢ, sets["a"u8]["k"u8], len(sets["a"u8]));
}

} // end main_package
