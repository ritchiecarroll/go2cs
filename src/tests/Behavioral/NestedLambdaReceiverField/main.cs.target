namespace go;

using fmt = fmt_package;
using static main_package;

partial class main_package {

partial interface grabber {
    @string tag();
}

partial struct connGrab {
    internal @string name;
}

} partial class mainᴛconnGrab {

internal static @string tag(this ref connGrab c) { return c.name; }

} partial class main_package {

partial struct dep {
    internal @string label;
}

} partial class mainᴛdep {

internal static void note(this ref dep d, any x) { fmt.Println("note:", d.label, x != nil); }

} partial class main_package {

partial struct stmt {
    internal ж<dep> d;
    internal grabber cg;
    internal nint id;
}

} partial class mainᴛstmt {

internal static void runWith(this ref stmt s, Action fn) { fn(); }

internal static void exec(this ж<stmt> Ꮡs) {
    ref var s = ref Ꮡs.DerefOrNull();

    s.runWith(() => {
        Ꮡs.Value.d.note(Ꮡs.OrTypedNil());
        Ꮡs.Value.runWith(() => {
            Ꮡs.Value.d.note(Ꮡs.OrTypedNil());
        });
        if (Ꮡs.Value.cg != nil) {
            fmt.Println("cg:", Ꮡs.Value.cg.tag(), Ꮡs.Value.id);
        }
    });
}

} partial class main_package {

internal static void main() {
    var s = Ꮡ(new stmt(d: Ꮡ(new dep(label: "d1")), cg: new connGrabжgrabber(Ꮡ(new connGrab(name: "g1"))), id: 7));
    s.exec();
    fmt.Println("done");
}

} // end main_package
