namespace go;

using fmt = fmt_package;
using runtime = runtime_package;
using strings = strings_package;
using @unsafe = unsafe_package;
using System.Runtime.CompilerServices;

partial class main_package {

partial struct point {
    public nint X, Y;
}

partial struct node {
    internal nint x;
    internal ж<node> prev, next;
}

partial struct boxed {
    public nint Value;
    internal @string label;
}

partial struct @base {
    internal nint n;
}

internal static nint Len(this ref @base b) {
    return b.n;
}

partial struct sized {
    /*embed*/ internal @base @base;
    public nint Len;
}

partial struct pair<T> {
    public T First;
    public nint Count;
}

internal static T bump<T>(ref pair<T> p, T v) {
    p.First = v;
    p.Count++;
    return p.First;
}

partial struct inner {
    public nint Z;
}

partial struct outer {
    /*embed*/ internal inner inner;
    public nint W;
}

partial struct wire {
    public uint16 Tag;
    public uint16 Size;
    public array<byte> Body = new(4);
    public uint32 Flag;
}

partial struct holder {
    internal ж<wire> w;
}

internal static ж<point> mk(nint x) {
    return Ꮡ(new point(x, x + 1));
}

internal static partial @string goFrames() {
    var namesᴷ = new stackAppend<@string>();

    var pcs = new slice<uintptr>(32);
    var frames = runtime.CallersFrames(pcs.slice(0, runtime.Callers(2, pcs)));
    slice<@string> names = default;

    for (;;) {
        var (frame, more) = frames.Next();

        if (!strings.HasPrefix(frame.Function, "runtime.")) {
            names = append(namesᴷ, names, frame.Function);
        }

        if (!more || frame.Function == "main.main"u8) {
            break;
        }
    }

    return strings.Join(names, " ");
}

internal static void Main() {
    var p = Ꮡ(new point(1, 2));
    p.X = 10;
    p.Y += 5;
    p.X++;
    var q = p;
    q.Y = p.X * 2;
    fmt.Println("point:", p.X, p.Y, p.Value);

    var o = Ꮡ(new outer(new inner(3), 4));
    o.Value.Z = 7;
    var @in = o.inner;
    @in.Z = 99;
    fmt.Println("outer:", (~o).Z, o.W, @in.Z, o.inner.Z);

    var (a, b, c) = (Ꮡ(new node(x: 1)), Ꮡ(new node(x: 2)), Ꮡ(new node(x: 3)));
    (a.next, b.prev, b.next, c.prev) = (b, a, c, b);
    c.prev.prev.next.x = 20;
    fmt.Println("chain:", a.next.x, b.x, c.prev.x);

    (p.X, p.Y) = (p.Y, p.X);
    fmt.Println("swap:", p.X, p.Y);

    (a.next, a.prev) = (a.prev, a.next);
    fmt.Println("swap pointers:", a.next == nil, a.prev.x);

    var v = Ꮡ(new boxed(Value: 5, label: "x"));
    v.Value.Value += 1;
    v.label += "y"u8;
    fmt.Println("collision:", (~v).Value, v.label);

    var s = Ꮡ(new sized(new @base(3), 9));
    s.Value.Len++;
    fmt.Println("shadow:", (~s).Len, s.Ꮡbase.Len());

    var g = Ꮡ(new pair<@string>(First: "a"));
    fmt.Println("generic:", bump(ref (g).DerefOrNull(), (@string)"b"), g.First, g.Count);

    var m = new map<@string, ж<point>>{["k"] = Ꮡ(new point(1, 1))};
    m["k"].X = 4;
    var ps = slice<ж<point>>([Ꮡ(new point(2, 2))]);
    ps[0].Y *= 3;
    fmt.Println("containers:", m["k"].X, ps[0].Y, mk(5).Y);

    var px = p.ᏑX;
    px.Value = 42;
    fmt.Println("address:", p.X);

    var buf = new slice<byte>(16);
    var h = new holder(w: Ꮡ(buf, 0).Reinterpret<byte, wire>());
    h.w.Tag = 0x0102;
    h.w.Size = 3;
    h.w.Flag = 0x0a0b0c0d;
    h.w.Flag++;
    fmt.Println("native bytes:", buf[..12]);
    fmt.Println("native fields:", h.w.Tag, h.w.Size, h.w.Flag);

    func(() => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                fmt.Println("nil read:", recover(), goFrames());
            }, ref ᒐ);

            ж<point> np = default;
            fmt.Println(np.X);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    })();

    func(() => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                fmt.Println("nil write:", recover());
            }, ref ᒐ);

            ж<node> nn = default;
            nn.x = 1;
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    })();
}

} // end main_package
