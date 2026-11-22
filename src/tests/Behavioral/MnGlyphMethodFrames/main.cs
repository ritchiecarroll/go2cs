namespace go;

using fmt = fmt_package;
using reflect = reflect_package;
using runtime = runtime_package;
using glyphlib = MnGlyphMethodFrames.glyphlib_package;
using MnGlyphMethodFrames;

partial class main_package {

partial struct local {
    internal channel<nint> ch;
    internal slice<@string> trail;
}

internal static nint Take(this ref local l) {
    nint v = ᐸꟷ(l.ch);
    l.trail = append(l.trail, glyphlib.Frames("local Take"));

    return v;
}

internal static partial void main() {
    var done = new channel<nint>(0);

    var b = glyphlib.New();
    var bʗ1 = b;
    var doneʗ1 = done;
    goǃ(() => {
        doneʗ1.ᐸꟷ(bʗ1.Take());
    });
    b.Put(7);
    fmt.Println(ᐸꟷ(done));

    foreach (var (_, line) in range(b.Trail())) {
        fmt.Println(line);
    }

    var l = Ꮡ(new local(ch: new channel<nint>(0)));
    var doneʗ2 = done;
    var lʗ1 = l;
    goǃ(() => {
        doneʗ2.ᐸꟷ(lʗ1.Take());
    });
    l.ch.ᐸꟷ(8);
    fmt.Println(ᐸꟷ(done));

    foreach (var (_, line) in range(l.trail)) {
        fmt.Println(line);
    }

    var recvʗ1 = ~b;
    
    var bʗ2 = b;
    var pass = (nint p1) => recvʗ1.Pass(p1);
    fmt.Println(pass(1));
    
    var expr = ((Func<glyphlib.Box, nint, @string>)(global::go.MnGlyphMethodFrames.glyphlibᴛBox.Pass));
    fmt.Println(expr(b.Value, 2));

    fmt.Println(b.Pass(3));

    fmt.Println("method expression:", runtime.FuncForPC(reflect.ValueOf(((Func<glyphlib.Box, nint, @string>)(global::go.MnGlyphMethodFrames.glyphlibᴛBox.Pass))).Pointer()).Name());
    var recvʗ2 = ~b;
    fmt.Println("method value:", runtime.FuncForPC(reflect.ValueOf((nint p1) => recvʗ2.Pass(p1)).Pointer()).Name());
}

} // end main_package
