namespace go;

using fmt = fmt_package;

partial class main_package {

partial interface grabber {
    @string tag();
}

partial struct connGrab {
    internal @string name;
}

internal static @string tag(this ref connGrab c) {
    return c.name;
}

partial struct dep {
    internal @string label;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object noteˢ = (@string)"note:"u8;

internal static void note(this ref dep d, any x) {
    fmt.Println(noteˢ, d.label, x != default!);
}

partial struct stmt {
    internal ж<dep> d;
    internal grabber cg;
    internal nint id;
}

internal static void runWith(this ref stmt s, Action fn) {
    fn();
}

internal static void exec(this ж<stmt> Ꮡs) {
    ref var s = ref Ꮡs.DerefOrNull();

    s.runWith(() => {
        Ꮡs.Value.d.note(Ꮡs.OrTypedNil());
        Ꮡs.Value.runWith(() => {
            Ꮡs.Value.d.note(Ꮡs.OrTypedNil());
        });
        if (Ꮡs.Value.cg != default!) {
            fmt.Println((@string)"cg:"u8, Ꮡs.Value.cg.tag(), Ꮡs.Value.id);
        }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object doneˢ = (@string)"done"u8;

internal static void Main() {
    var s = Ꮡ(new stmt(d: Ꮡ(new dep(label: "d1"u8)), cg: new connGrabжgrabber(Ꮡ(new connGrab(name: "g1"u8))), id: 7));
    s.exec();
    fmt.Println(doneˢ);
}

} // end main_package
