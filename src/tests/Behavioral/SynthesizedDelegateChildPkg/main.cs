namespace go;

using fmt = fmt_package;
using inner = SynthesizedDelegateChildPkg.inner_package;
using SynthesizedDelegateChildPkg;

partial class main_package {

partial struct registry {
    internal map<@string, ж<inner.Record>> cache;
    internal Func<map<@string, ж<inner.Record>>, @string, Func<@string, (@string, error)>, (ж<inner.Record>, error)> load;
    internal map<@string, Action<ж<inner.Record>, @string>> notify;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object notifyˢ = (@string)"notify:"u8;
private static readonly @string alphaˢ = "alpha"u8;
private static readonly @string hitˢ = "hit"u8;

internal static void Main() {
    var r = new registry(
        cache: new map<@string, ж<inner.Record>>{},
        load: (map<@string, ж<inner.Record>> cache, @string name, Func<@string, (@string, error)> lookup) => {
            var (suffix, errΔ1) = lookup(name);
            if (errΔ1 != default!) {
                return (default!, errΔ1);
            }
            var recΔ1 = Ꮡ(new inner.Record(Name: name + suffix, Hits: 1));
            cache[name] = recΔ1;
            return (recΔ1, default!);
        },
        notify: new map<@string, Action<ж<inner.Record>, @string>>{
            ["hit"u8] = (ж<inner.Record> recΔ2, @string tag) => {
                recΔ2.Value.Hits++;
                fmt.Println(notifyˢ, tag, (~recΔ2).Name, (~recΔ2).Hits);
            }
        }
    );
    var (rec, err) = r.load(r.cache, alphaˢ, (@string s) => ("-" + s, default!));
    fmt.Println((~rec).Name, (~rec).Hits, err);
    r.notify[hitˢ](rec, "t1"u8);
    fmt.Println(len(r.cache), (~r.cache[alphaˢ]).Hits);
}

} // end main_package
