namespace go;

using fmt = fmt_package;
using reflect = reflect_package;
using runtime = runtime_package;
using framelib = GlyphLibraryFrameNames.framelib_package;
using GlyphLibraryFrameNames;

partial class main_package {

partial struct local {
}

internal static @string Closure(this local l) {
    @string f() => framelib.Here();

    return f();
}

internal static @string funcName(any f) {
    return runtime.FuncForPC(reflect.ValueOf(f).Pointer()).Name();
}

internal static void main() {
    ref var c = ref heap(new framelib.Counter(), out var Ꮡc);
    var p = Ꮡc;

    fmt.Println("method:", c.Name());
    fmt.Println("closure in value method:", c.Closure());
    fmt.Println("closure in pointer method:", p.Bump());
    fmt.Println("local closure:", new local().Closure());

    var cʗ1 = c;
    fmt.Println("value method value:", funcName(() => cʗ1.Name()));
    fmt.Println("value method expression:", funcName(((Func<framelib.Counter, @string>)(global::go.GlyphLibraryFrameNames.framelibᴛCounter.Name))));
    fmt.Println("pointer method value:", funcName(p.Bump));
    fmt.Println("pointer method expression:", funcName(((Func<ж<framelib.Counter>, @string>)(global::go.GlyphLibraryFrameNames.framelibᴛCounter.Bump))));
    fmt.Println("local method value:", funcName(() => new local().Closure()));
}

} // end main_package
