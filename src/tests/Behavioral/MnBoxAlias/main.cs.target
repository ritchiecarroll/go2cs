namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct inner {
    internal nint n;
}

partial struct acct {
    internal nint bal;
    internal inner sub;
}

partial struct cell {
    internal nint v;
}

partial struct holder {
    internal cell c;
}

partial struct bag {
    internal slice<cell> s;
}

internal static nint waitAt(this ref bag b, nint i) {
    return b.s[i].waitAndRead();
}

internal static channel<EmptyStruct> ready = new channel<EmptyStruct>(0);
internal static channel<EmptyStruct> resume = new channel<EmptyStruct>(0);

internal static nint waitAndRead(this ref acct a) {
    ready.ᐸꟷ(new EmptyStruct());
    ᐸꟷ(resume);
    return a.bal;
}

internal static nint waitAndRead(this ref inner s) {
    ready.ᐸꟷ(new EmptyStruct());
    ᐸꟷ(resume);
    return s.n;
}

internal static nint waitAndRead(this ref cell c) {
    ready.ᐸꟷ(new EmptyStruct());
    ᐸꟷ(resume);
    return c.v;
}

internal static nint valueRead(this acct a) {
    ready.ᐸꟷ(new EmptyStruct());
    ᐸꟷ(resume);
    nint got = a.bal;
    a.bal = 777;
    return got;
}

internal static void writer(Action write) {
    ᐸꟷ(ready);
    write();
    resume.ᐸꟷ(new EmptyStruct());
}

internal static @string /*out*/ nilRoot(ж<acct> Ꮡnp) {
    @string @out = default;
    GoFrame ᒐ = default;
    try {
        ref var np = ref Ꮡnp.DerefOrNull();

        defer(() => {
            var r = recover(); if (r != nil) {
                @out = fmt.Sprint("panic: ", r);
            }
        }, ref ᒐ);
        nint v = np.waitAndRead();
        @out = fmt.Sprint(v);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return @out;
}

internal static @string /*out*/ nilField(ж<holder> Ꮡnh) {
    @string @out = default;
    GoFrame ᒐ = default;
    try {
        ref var nh = ref Ꮡnh.DerefOrNull();

        defer(() => {
            var r = recover(); if (r != nil) {
                @out = fmt.Sprint("panic: ", r);
            }
        }, ref ᒐ);
        nint v = nh.c.waitAndRead();
        @out = fmt.Sprint(v);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return @out;
}

internal static partial void main() {
    var a = Ꮡ(new acct(bal: 1, sub: new inner(n: 1)));

    var aʗ1 = a;
    goǃ(writer, () => {
        aʗ1.bal = 2;
    });
    nint root = a.waitAndRead();
    fmt.Println("root", root);

    var aʗ2 = a;
    goǃ(writer, () => {
        aʗ2.sub.n = 7;
    });
    nint field = a.Ꮡsub.waitAndRead();
    fmt.Println("field", field);

    var cells = slice<cell>([new(v: 1)]);
    var old = cells;
    cells = append(cells, new cell(v: 9));
    var oldʗ1 = old;
    goǃ(writer, () => {
        oldʗ1[0].v = 4;
    });
    nint element = old[0].waitAndRead();
    fmt.Println("element", element);
    fmt.Println("element new-array", cells[0].v, "old-array", old[0].v);

    var b = Ꮡ(new bag(s: [new(v: 1), new(v: 1)]));
    var bʗ1 = b;
    goǃ(writer, () => {
        bʗ1.s[1].v = 3;
    });
    nint ownElement = b.waitAt(1);
    fmt.Println("own-element", ownElement);

    var aʗ3 = a;
    goǃ(writer, () => {
        aʗ3.bal = 50;
    });
    nint value = a.valueRead();
    fmt.Println("value", value);
    fmt.Println("value after", a.bal);

    goǃ(writer, () => {
    });
    fmt.Println("nil", nilRoot(nil));

    fmt.Println("nil-field", nilField(nil));
}

} // end main_package
