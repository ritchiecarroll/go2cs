namespace go;

using fmt = fmt_package;
using os = os_package;
using runtime = runtime_package;
using Δsync = sync_package;
using time = time_package;
using Δio = io_package;

partial class main_package {

internal static UntypedInt items => 500;
internal static UntypedInt adds => 1000;
internal static UntypedInt picks => 10000;
internal static UntypedInt stages => 4;

internal static bool reading = os.Getenv("MN_GEN_READING") == "1"u8;

internal static void report(@string phase, time.Time began) {
    if (reading) {
        fmt.Fprintf(new os.FileжWriter(os.Stderr), "reading: %s elapsed_ms=%.1f\n"u8, phase, (float64)time.Since(began).Microseconds() / 1000D);
    }
}

partial struct point {
    internal nint x, y;
}

partial interface number<ΔT> /*operators = Sum, Arithmetic, Comparable, Ordered*/ {
    //  Type constraints: ~int | ~float64
    // Derived operators: +, -, *, /, ==, !=, <, <=, >, >=
}

internal static void relay<T>(/*<-*/channel<EmptyStruct> gate, /*<-*/channel<T> @in, channel/*<-*/<T> @out) {
    ᐸꟷ(gate);
    for (;;) {
        var (v, ok) = ᐸꟷ(@in, ꟷ);
        if (!ok) {
            close(@out);
            return;
        }
        @out.ᐸꟷ(v);
    }
}

internal static void source<T>(/*<-*/channel<EmptyStruct> gate, channel/*<-*/<T> @out, Func<nint, T> mk) {
    ᐸꟷ(gate);
    for (nint i = 0; i < items; i++) {
        @out.ᐸꟷ(mk(i));
    }
    close(@out);
}

internal static void sink<T>(/*<-*/channel<T> @in, channel/*<-*/<slice<T>> done) {
    slice<T> got = default;
    for (;;) {
        var (v, ok) = ᐸꟷ(@in, ꟷ);
        if (!ok) {
            done.ᐸꟷ(got);
            return;
        }
        got = append(got, v);
    }
}

internal static partial bool pipelines<T>(nint n, Func<nint, T> mk) {
    var gate = new channel<EmptyStruct>(0);
    var done = new channel<slice<T>>(n);

    for (nint p = 0; p < n; p++) {
        var head = new channel<T>(0);
        goǃ(source, gate.AsRecvOnly(), head.AsSendOnly(), mk);
        var @in = head;
        for (nint s = 0; s < stages; s++) {
            var @out = new channel<T>(0);
            goǃ(relay, gate.AsRecvOnly(), @in.AsRecvOnly(), @out.AsSendOnly());
            @in = @out;
        }
        goǃ(sink, @in.AsRecvOnly(), done.AsSendOnly());
    }

    close(gate);
    var ok = true;
    for (nint p = 0; p < n; p++) {
        var got = ᐸꟷ(done);
        ok = ok && len(got) == items;
        for (nint i = 0; ok && i < items; i++) {
            ok = AreEqual(got[i], mk(i));
        }
    }
    return ok;
}

partial struct counter<T>
    where T : /* number */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IIncrementOperators<T>, IDecrementOperators<T>, IUnaryNegationOperators<T, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    internal Δsync.Mutex mu;
    internal T v;
}

internal static void add<T>(this ж<counter<T>> Ꮡc, T x)
    where T : /* number */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IIncrementOperators<T>, IDecrementOperators<T>, IUnaryNegationOperators<T, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    ref var c = ref Ꮡc.DerefOrNull();

    c.mu.Lock();
    c.v += x;
    c.mu.Unlock();
}

internal static void adder<T>(/*<-*/channel<EmptyStruct> gate, ж<counter<T>> c, T x, ж<Δsync.WaitGroup> wg)
    where T : /* number */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IIncrementOperators<T>, IDecrementOperators<T>, IUnaryNegationOperators<T, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new() {
    GoFrame ᒐ = default;
    try
    {
        defer(wg.Done, ref ᒐ);
        ᐸꟷ(gate);
        for (nint i = 0; i < adds; i++) {
            c.add(x);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static partial T contend<T>(nint n, T x)
    where T : /* number */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IIncrementOperators<T>, IDecrementOperators<T>, IUnaryNegationOperators<T, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    ref var c = ref heap(new counter<T>(), out var Ꮡc);
    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
    var gate = new channel<EmptyStruct>(0);
    for (nint g = 0; g < n; g++) {
        Ꮡwg.Add(1);
        goǃ(adder, gate.AsRecvOnly(), Ꮡc, x, Ꮡwg);
    }
    close(gate);
    Ꮡwg.Wait();
    return c.v;
}

internal static (T, bool) pick<T>(channel<T> a, channel<T> b) {
    var aᴛ1 = a;
    var bᴛ1 = b;
    switch (select(ᐸꟷ(aᴛ1, ꓸꓸꓸ), ᐸꟷ(bᴛ1, ꓸꓸꓸ))) {
    case 0 when aᴛ1.ꟷᐳ(out var v): {
        a.ᐸꟷ(v);
        return (v, true);
    }
    case 1 when bᴛ1.ꟷᐳ(out var v): {
        b.ᐸꟷ(v);
        return (v, false);
    }}
    return default;
}

internal static bool selectFair<T>(T va, T vb) {
    var (a, b) = (new channel<T>(1), new channel<T>(1));
    a.ᐸꟷ(va);
    b.ᐸꟷ(vb);
    nint first = 0;
    for (nint i = 0; i < picks; i++) {
        var (_, fromA) = pick(a, b); if (fromA) {
            first++;
        }
    }
    return first >= (nint)(picks / 10) && (nint)picks - first >= (nint)(picks / 10);
}

internal static void main() {
    nint procs = runtime.GOMAXPROCS(0);
    nint n = 2 * procs;

    var began = time.Now();
    fmt.Println("pipelines int:", pipelines(n, [GoFunc("main.func1")] nint (nint i) => i * 3));
    fmt.Println("pipelines string:", pipelines(n, [GoFunc("main.func2")] @string (nint i) => fmt.Sprint("s", i)));
    fmt.Println("pipelines struct:", pipelines(n, [GoFunc("main.func3")] point (nint i) => new point(i, -i)));
    fmt.Println("pipelines array:", pipelines(n, [GoFunc("main.func4")] array<int64> (nint i) => array<int64>([(int64)i, 1, 2, 3])));
    report("pipelines", began);

    began = time.Now();
    fmt.Println("counter int:", contend<nint>(n, 2) == 2 * n * (nint)adds);
    fmt.Println("counter float64:", contend(n, 0.5D) == 0.5D * (float64)(n * (nint)adds));
    report("counter", began);

    began = time.Now();
    fmt.Println("select int:", selectFair<nint>(1, 2));
    fmt.Println("select struct:", selectFair(new point(1, 1), new point(2, 2)));
    report("select", began);
}

} // end main_package
