namespace go;

using fmt = fmt_package;
using inner = SynthesizedDelegateChildPkg.inner_package;
using SynthesizedDelegateChildPkg;

partial class main_package {

partial struct registry {
    internal map<@string, ж<inner.Record>> cache;
    internal inner.Loader load;
    internal map<@string, Action<ж<inner.Record>, @string>> notify;
}

internal static void main() {
    var r = new registry(
        cache: new map<@string, ж<inner.Record>>{},
        load: [GoFunc("main.func1")] (map<@string, ж<inner.Record>> cache, @string name, Func<@string, (@string, error)> lookup) => {
            var (suffix, errΔ1) = lookup(name);
            if (errΔ1 != nil) {
                return (default, errΔ1);
            }
            var recΔ1 = Ꮡ(new inner.Record(Name: name + suffix, Hits: 1));
            cache[name] = recΔ1;
            return (recΔ1, default);
        },
        notify: new map<@string, Action<ж<inner.Record>, @string>>{
            ["hit"] = [GoFunc("main.func2")] (ж<inner.Record> recΔ2, @string tag) => {
                recΔ2.Hits++;
                fmt.Println("notify:", tag, recΔ2.Name, recΔ2.Hits);
            }
        }
    );

    var (rec, err) = r.load(r.cache, "alpha", [GoFunc("main.func3")] (@string s) => ("-" + s, default));
    fmt.Println(rec.Name, rec.Hits, err);

    r.notify["hit"](rec, "t1");
    fmt.Println(len(r.cache), r.cache["alpha"].Hits);
}

} // end main_package
