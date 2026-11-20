namespace go;

using fmt = fmt_package;
using wrap = RenamedTypeAliasReexport.wrap_package;
using RenamedTypeAliasReexport;
using lib = RenamedTypeAliasReexport.lib_package;

partial class main_package {

partial struct state {
    internal ж<lib.Encoder> enc;
    internal slice<lib.Kind> kinds;
    internal map<@string, lib.Tok> byKey;
}

internal static nint use(ж<lib.Encoder> Ꮡe) {
    ref var e = ref Ꮡe.DerefOrNull();

    return e.N();
}

internal static lib.Kind makeKind(nint n) { return (lib.Kind)(byte)n; }

internal static void main() {
    var s = new state(enc: wrap.NewEncoder(2), byKey: new map<@string, lib.Tok>{});
    s.enc.Bump();
    s.kinds = append(s.kinds, makeKind(1), (lib.Kind)4);
    s.byKey["a"] = wrap.NewTok(7);
    fmt.Println(use(s.enc), s.kinds, s.byKey["a"].Kind(), s.kinds[1] == ((lib.Kind)4));

    ref var e = ref heap(new lib.Encoder(), out var Ꮡe);
    e.Bump();
    var bump = Ꮡe.Bump;
    bump();
    fmt.Println(e.N());

    any anyv = ((lib.Kind)9);
    var (k, ok) = anyv._<wrapꓸKind>(ᐧ); if (ok) {
        fmt.Println("kind", k);
    }
    switch (anyv.type()) {
    case ж<wrapꓸEncoder> v: {
        fmt.Println("encoder", v.N());
        break;
    }
    case wrapꓸKind v: {
        fmt.Println("switch kind", v);
        break;
    }}

    any t = wrap.Read();
    fmt.Println(t, new wrapꓸTok() == wrap.NewTok(0));
}

} // end main_package
