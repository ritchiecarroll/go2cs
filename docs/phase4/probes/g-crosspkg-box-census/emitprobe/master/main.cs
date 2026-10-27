namespace go;

using fmt = fmt_package;
using lib = xpprobe.lib_package;
using xpprobe;
using Δsync = sync_package;

partial class main_package {

internal static void Main() {
    var p = Ꮡ(lib.N);
    p.Value = 5;
    var f = Ꮡ(lib.V).Inc;
    f();
    lib.V.Inc();
    var q = Ꮡ(lib.S).of(lib.Holder.ᏑN);
    q.Value = 3;
    Ꮡ(lib.S).of(lib.Holder.ᏑWG).Add(1);
    Ꮡ(lib.S).of(lib.Holder.ᏑWG).Done();
    var e = Ꮡlib.Arr.at<nint>(1);
    e.Value = 7;
    var r = lib.ᏑRW;
    r.Lock();
    r.Unlock();
    var t = Ꮡ(lib.Table);
    t.ValueSlot = append(t.ValueSlot, (nint)4);
    fmt.Println(lib.ReadN(), lib.V.Value(), lib.S.N, lib.Arr[1], len(lib.Table), Ꮡ(lib.N) == Ꮡ(lib.N));
}

} // end main_package
