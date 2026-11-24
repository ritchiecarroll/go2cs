namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct cell {
    internal nint v;
}

internal static channel<EmptyStruct> ready = new channel<EmptyStruct>(0);
internal static channel<EmptyStruct> resume = new channel<EmptyStruct>(0);

internal static nint readWith(ref cell c, nint k) {
    ready.ᐸꟷ(new EmptyStruct());
    ᐸꟷ(resume);
    return c.v * 100 + k;
}

internal static void writer(Action write) {
    ᐸꟷ(ready);
    write();
    resume.ᐸꟷ(new EmptyStruct());
}

internal static partial void main() {
    var (pc1, pc2) = (Ꮡ(new cell(v: 1)), Ꮡ(new cell(v: 2)));
    ref var pp = ref heap<ж<cell>>(pc1, out var Ꮡpp);
    var kc = new channel<nint>(0);
    var kcʗ1 = kc;
    var pc2ʗ1 = pc2;
    goǃ([GoFunc("main.func1")] () => {
        Ꮡpp.ValueSlot = pc2ʗ1;
        kcʗ1.ᐸꟷ(5);
    });
    var pc1ʗ1 = pc1;
    var pc2ʗ2 = pc2;
    goǃ(writer, [GoFunc("main.func2")] () => {
        pc1ʗ1.v = 15;
        pc2ʗ2.v = 14;
    });
    fmt.Println("param-later-arg", readWith(ref (pp).DerefOrNull(), ᐸꟷ(kc)));
}

} // end main_package
