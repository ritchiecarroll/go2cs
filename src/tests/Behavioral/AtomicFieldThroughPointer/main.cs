namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct atom {
    internal int64 v;
}

internal static void add(this ref atom a, int64 d) {
    a.v += d;
}

internal static int64 get(this ref atom a) {
    return a.v;
}

partial struct profile {
    internal atom wait;
}

partial struct holder {
    internal atom wait;
    internal profile prof;
}

partial struct owner {
    internal ж<holder> h;
    internal ж<holder> deep;
}

internal static void bump(ж<owner> Ꮡo, int64 d) {
    ref var o = ref Ꮡo.DerefOrNull();

    o.h.of(holder.Ꮡwait).add(d);
    o.deep.of(holder.Ꮡprof).of(profile.Ꮡwait).add(d);
}

internal static int64 viaPointerLocal(ref owner o, int64 d) {
    var mp = o.h;
    mp.of(holder.Ꮡprof).of(profile.Ꮡwait).add(d);
    mp.of(holder.Ꮡprof).of(profile.Ꮡwait).add(d);
    return mp.of(holder.Ꮡprof).of(profile.Ꮡwait).get();
}

internal static void Main() {
    var o = Ꮡ(new owner(h: Ꮡ(new holder(nil)), deep: Ꮡ(new holder(nil))));
    bump(o, 5);
    bump(o, 5);
    fmt.Println((~o).h.of(holder.Ꮡwait).get());
    fmt.Println((~o).deep.of(holder.Ꮡprof).of(profile.Ꮡwait).get());
    var o2 = Ꮡ(new owner(h: Ꮡ(new holder(nil))));
    fmt.Println(viaPointerLocal(ref (o2).DerefOrNull(), 4));
}

} // end main_package
