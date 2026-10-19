namespace go;

using fmt = fmt_package;
using strings = strings_package;

partial class main_package {

partial struct Env /*map[@string, @string]*/;

public static void Set(this Env e, @string k, @string v) {
    e[strings.ToLower(k)] = v;
}

partial struct Bag /*map[@string, nint]*/;

public static void Add(this Bag b, @string k, nint v) {
    b[k] += v;
}

partial struct Cache /*map[@string, nint]*/;

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

partial struct PEnv /*map[@string, @string]*/;

public static void Set(this ref PEnv p, @string k, @string v) {
    (p)[strings.ToUpper(k)] = v;
}

partial struct Uniq<T> /*map[T, EmptyStruct]*/;

public static bool Remove<T>(this Uniq<T> u, T item) {
    var (_, ok) = u[item, ꟷ];
    delete(u, item);
    return !ok;
}

partial struct Pipe /*chan nint*/;

internal static nint sends;

public static void Send(this Pipe p, nint v) {
    sends++;
    p.ᐸꟷ(v * 10);
}

partial struct ΔSet /*map[@string, nint]*/;

partial struct Hdr /*map[@string, slice<@string>]*/;

public static void Set(this Hdr h, @string k, @string v) {
    h[strings.ToUpper(k)] = new @string[]{v}.slice();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object exactSetRanˢ = (@string)"exact Set ran:"u8;
private static readonly object exactAddAccumulatedˢ = (@string)"exact Add accumulated:"u8;
private static readonly object exactRemoveˢ = (@string)"exact Remove:"u8;
private static readonly object exactClearˢ = (@string)"exact Clear:"u8;
private static readonly object exactContainsKeyˢ = (@string)"exact ContainsKey:"u8;
private static readonly object controlSetˢ = (@string)"control Set:"u8;
private static readonly object nestedAssignRawˢ = (@string)"nested assign raw:"u8;
private static readonly object nestedAssignInFieldˢ = (@string)"nested assign in field:"u8;
private static readonly object pointerSetˢ = (@string)"pointer Set:"u8;
private static readonly object nestedRawˢ = (@string)"nested raw:"u8;
private static readonly object genericRemoveˢ = (@string)"generic Remove:"u8;
private static readonly object exactSendˢ = (@string)"exact Send:"u8;
private static readonly object namedSetNestedˢ = (@string)"named Set nested:"u8;

internal partial struct main_named /*dyn*/ {
    internal map<@string, Env> m;
}

internal static void Main() {
    var e = new Env(new map<@string, @string>{});
    e.Set("Key", "v");
    var (_, lower) = e["key", ꟷ];
    var (_, raw) = e["Key", ꟷ];
    fmt.Println(exactSetRanˢ, lower, raw);

    var b = new Bag(new map<@string, nint>{});
    b.Add("n", 2);
    b.Add("n", 3);
    fmt.Println(exactAddAccumulatedˢ, b["n"]);

    var c = new Cache(new map<@string, nint>{["a"] = 1});
    fmt.Println(exactRemoveˢ, c.Remove("a"), c.Remove("a"), len(c));
    c["b"] = 2;
    c.Clear();
    fmt.Println(exactClearˢ, cleared, len(c));
    fmt.Println(exactContainsKeyˢ, c.ContainsKey("always"), c.ContainsKey("b"));

    var h = new Hdr(new map<@string, slice<@string>>{});
    h.Set("accept", "x");
    fmt.Println(controlSetˢ, len(h["ACCEPT"]), len(h["accept"]));

    var envs = new map<@string, Env>{["x"] = new map<@string, @string>{}};
    envs["x"].Set‿("Mixed", "m");
    var (_, nestedRaw) = envs["x"]["Mixed", ꟷ];
    var (_, nestedLower) = envs["x"]["mixed", ꟷ];
    fmt.Println(nestedAssignRawˢ, nestedRaw, nestedLower);
    var named = new main_named(m: new map<@string, Env>{["y"] = new map<@string, @string>{}});
    named.m["y"].Set‿("Q", "q");
    fmt.Println(nestedAssignInFieldˢ, named.m["y"]["Q"]);

    var pe = new PEnv(new map<@string, @string>{});
    pe.Set("low", "p");
    var penvs = new map<@string, PEnv>{["z"] = new map<@string, @string>{}};
    penvs["z"].Set‿("low", "r");
    fmt.Println(pointerSetˢ, pe["LOW"], len(pe), nestedRawˢ, penvs["z"]["low"], len(penvs["z"]));

    var u = new Uniq<@string>(new map<@string, EmptyStruct>{["a"] = new()});
    fmt.Println(genericRemoveˢ, u.Remove("a"u8), u.Remove("a"u8), len(u));

    var p = new Pipe(1);
    p.Send(4);
    fmt.Println(exactSendˢ, sends, ᐸꟷ<nint>(p));

    var sets = new map<@string, ΔSet>{["a"] = new map<@string, nint>{}};
    sets["a"].Set‿("k", 7);
    fmt.Println(namedSetNestedˢ, sets["a"]["k"], len(sets["a"]));
}

} // end main_package
