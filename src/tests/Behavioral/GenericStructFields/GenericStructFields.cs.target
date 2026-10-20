namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct Result<T> {
    public T Value;
    public error Error;
}

partial struct Container {
    public Result<nint> IntResult;
    public Result<@string> StringResult;
    public slice<Result<float64>> FloatValues;
    public map<@string, Result<bool>> Mappings;
}

partial struct tag<T> {
    internal T label;
}

internal static T show<T>(this tag<T> t) {
    return t.label;
}

partial struct wrapped<T> {
    /*embed*/ internal tag<T> tag;
    internal nint count;
}

internal static void Main() {
    var container = new Container(
        IntResult: new Result<nint>(
            Value: 42,
            Error: default!
        ),
        StringResult: new Result<@string>(
            Value: "success",
            Error: default!
        ),
        FloatValues: new Result<float64>[]{
            new(Value: 3.14D, Error: default!),
            new(Value: 2.71D, Error: default!)
        }.slice(),
        Mappings: new map<@string, Result<bool>>{
            ["completed"] = new(Value: true, Error: default!),
            ["verified"] = new(Value: false, Error: fmt.Errorf("verification pending"u8))
        }
    );

    fmt.Printf("Int result: %d\n"u8, container.IntResult.Value);
    fmt.Printf("String result: %s\n"u8, container.StringResult.Value);
    fmt.Printf("First float value: %f\n"u8, container.FloatValues[0].Value);
    fmt.Printf("Completion status: %t\n"u8, container.Mappings["completed"].Value);

    Δpool<nint> pl = default!;
    pl.items = append(pl.items, (nint)7, (nint)8);
    var (v, ok) = pl.take();
    fmt.Println(v, ok, new keeper(nil).pool());

    var w = new wrapped<@string>(tag: new tag<@string>(label: "gen"), count: 2);
    fmt.Println(w.label, w.tag.show(), w.count);
}

partial struct Δpool<T> {
    internal slice<T> items;
}

internal static (T, bool) take<T>(this ref Δpool<T> p) {
    if (len(p.items) == 0) {
        T zero = GoZero<T>();
        return (zero, false);
    }
    var v = p.items[len(p.items) - 1];
    p.items = p.items.slice(0, len(p.items) - 1);
    return (v, true);
}

partial struct keeper {
}

internal static @string pool(this keeper k) {
    return "kept";
}

} // end main_package
