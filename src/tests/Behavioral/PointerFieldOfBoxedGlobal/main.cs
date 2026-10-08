namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct profBuf {
    internal slice<nint> data;
}

internal static void push(this ж<profBuf> Ꮡb, nint v) {
    ref var b = ref Ꮡb.DerefOrNull();

    appendInt(ref nonnil(ref b).data, v);
}

internal static void appendInt(ref slice<nint> s, nint v) {
    s = append(s, v);
}

internal static nint sum(this ref profBuf b) {
    nint t = 0;
    foreach (var (_, x) in b.data) {
        t += x;
    }
    return t;
}

partial struct cpuProfile {
    internal nint count;
    internal ж<profBuf> log;
}

internal static ж<cpuProfile> Ꮡcpuprof = new StandardBox<cpuProfile>(default(cpuProfile));
internal static ref cpuProfile cpuprof => ref Ꮡcpuprof.Value;

internal static void incr(ref nint p) {
    p++;
}

internal static void run() {
    incr(ref cpuprof.count);
    cpuprof.log.push(cpuprof.count);
    cpuprof.log.push(cpuprof.count);
}

partial struct holder {
    internal ж<cpuProfile> span;
}

internal static nint viaLocal(ref holder h) {
    var s = h.span;
    (~s).log.push(10);
    return (~s).log.sum();
}

internal static void Main() {
    cpuprof.log = Ꮡ(new profBuf(nil));
    run();
    fmt.Println(cpuprof.count);
    fmt.Println(cpuprof.log.sum());
    var h = Ꮡ(new holder(span: Ꮡ(new cpuProfile(log: Ꮡ(new profBuf(nil))))));
    fmt.Println(viaLocal(ref (h).DerefOrNull()));
}

} // end main_package
