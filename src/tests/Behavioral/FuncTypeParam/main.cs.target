namespace go;

using fmt = fmt_package;
using atomic = sync.atomic_package;
using sync;

partial class main_package {

internal static nint apply(ж<atomic.Int32> v, Func<ж<atomic.Int32>, nint> f) {
    return f(v);
}

internal static void consume(ж<atomic.Int32> v, Action<ж<atomic.Int32>> sink) {
    sink(v);
}

partial struct runner {
    internal Func<ж<atomic.Int32>, nint> gen;
}

internal static nint invoke(this runner r, ж<atomic.Int32> v) {
    return r.gen(v);
}

// type loadFunc is a methodless func type — rendered inline as its base delegate

// type readFunc is a methodless func type — rendered inline as its base delegate

internal static int32 loadTwice(Func<ж<int32>, int32> f, ж<int32> p) {
    return f(p) + f(p);
}

internal static nint readPlusOne(ж<atomic.Int32> v) {
    return (nint)v.Load() + 1;
}

internal static void Main() {
    ref var v = ref heap(new atomic.Int32(), out var Ꮡv);
    Ꮡv.Store(10);

    nint n = apply(Ꮡv, (ж<atomic.Int32> x) => (nint)x.Load() + 5);
    fmt.Println(n);

    consume(Ꮡv, (ж<atomic.Int32> x) => {
        x.Store(99);
    });
    fmt.Println(Ꮡv.Load());

    var @double = new runner(gen: (ж<atomic.Int32> x) => (nint)x.Load() * 2
    );
    fmt.Println(@double.invoke(Ꮡv));

    var seven = new runner(gen: (ж<atomic.Int32> x) => 7
    );
    fmt.Println(seven.invoke(Ꮡv));

    ref var raw = ref heap(new int32(), out var Ꮡraw);
    raw = 21;
    var ld = atomic.LoadInt32;
    fmt.Println(ld(Ꮡraw), loadTwice(new Func<ж<int32>, int32>(ld), Ꮡraw));

    var rd = readPlusOne;
    fmt.Println(rd(Ꮡv), apply(Ꮡv, rd));
}

} // end main_package
