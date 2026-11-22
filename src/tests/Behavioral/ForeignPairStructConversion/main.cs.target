global using P = go.ForeignPairStructConversion.lib_package.Pitch;

namespace go;

using fmt = fmt_package;
using lib = ForeignPairStructConversion.lib_package;
using other = ForeignPairStructConversion.other_package;
using ForeignPairStructConversion;

partial class main_package {

partial struct holder {
    public other.Pitch O;
    public lib.Pitch L;
}

internal static nint calls;

internal static other.Pitch next() {
    calls++;
    return new other.Pitch(Hz: 100 + calls);
}

internal static lib.Pitch concert = libꓸPitch(new other.Pitch(Hz: 440));

internal static nint @double(lib.Pitch p) { return p.Double(); }

internal static nint octave(other.Pitch o) { return o.Octave(); }

internal static lib.Pitch result(other.Pitch o) { return libꓸPitch(o); }

internal static other.Pitch back(lib.Pitch p) { return otherꓸPitch(p); }

internal static lib.Pitch converted(this holder h) { return libꓸPitch(h.O); }

internal static void report(@string label, lib.Pitch p) { fmt.Println(label, p.Hz); }

internal static partial void main() {
    GoFrame ᒐ = default;
    try {
        ref var o = ref heap<other.Pitch>(new other.Pitch(Hz: 220), out var Ꮡo);
        var p = new lib.Pitch(Hz: 330);

        var lp = libꓸPitch(o);
        var op = otherꓸPitch(p);
        fmt.Println("bound:", lp.Hz, op.Hz);
        fmt.Printf("types: %T %T\n"u8, lp, op);

        fmt.Println("selector:", libꓸPitch(o).Hz, otherꓸPitch(p).Hz);
        fmt.Println("method:", libꓸPitch(o).Double(), otherꓸPitch(p).Octave());

        fmt.Println("argument:", @double(libꓸPitch(o)), octave(otherꓸPitch(p)));
        fmt.Println("result:", result(o).Hz, back(p).Hz);
        fmt.Println("receiver:", new holder(O: o).converted().Hz);

        var twin = libꓸTwin(p);
        fmt.Println("same package:", twin.Hz, twin.Half(), libꓸPitch(twin).Double());

        fmt.Println("overloads:", libꓸPitch(o).Hz, libꓸPitch(twin).Hz, libꓸPitch(new other.Tagged(Hz: 55)).Hz);
        fmt.Println("tag ignored:", otherꓸTagged(o).Hz, otherꓸPitch(new other.Tagged(Hz: 56)).Hz);

        fmt.Println("round trip:", otherꓸPitch(libꓸPitch(o)) == o, libꓸPitch(otherꓸPitch(p)) == p);

        P aliased = libꓸPitch(o);
        fmt.Println("alias:", aliased.Hz, libꓸPitch(new other.Pitch(Hz: 5)).Double());

        fmt.Println("package variable:", concert.Hz);

        var h = new holder(O: o, L: libꓸPitch(o));
        var s = slice<lib.Pitch>([libꓸPitch(o), libꓸPitch(h.O)]);
        var a = array<other.Pitch>([otherꓸPitch(p), otherꓸPitch(s[0])]);
        var m = new map<@string, lib.Pitch>{["a"] = libꓸPitch(o)};
        var k = new map<lib.Pitch, @string>{[libꓸPitch(o)] = "key"};
        fmt.Println("field:", h.L.Hz);
        fmt.Println("elements:", s[0].Hz, s[1].Hz, a[0].Hz, a[1].Hz);
        fmt.Println("map:", m["a"].Hz, k[new lib.Pitch(Hz: 220)], len(k));

        var po = Ꮡo;
        fmt.Println("operands:", libꓸPitch(h.O).Hz, libꓸPitch(a[0]).Hz, libꓸPitch(po.Value).Hz, libꓸPitch(other.Low()).Hz, otherꓸPitch(lib.Middle()).Hz);

        var once = libꓸPitch(next());
        fmt.Println("once:", once.Hz, calls);
        fmt.Println("once more:", libꓸPitch(next()).Double(), calls);

        var copied = libꓸPitch(o);
        copied.Hz = 1;
        o.Hz = 221;
        fmt.Println("copy:", copied.Hz, o.Hz);
        o.Hz = 220;

        ref var @ref = ref heap<lib.Pitch>(new lib.Pitch(Hz: 7), out var Ꮡref);
        var on = new other.Note(Name: "A", Pitch: new lib.Pitch(Hz: 440), Marks: [1, 2, 3], Ref: Ꮡref, Weight: 1.5D);
        var ln = libꓸNote(on);
        ln.Name = "B";
        ln.Pitch.Hz = 441;
        ln.Marks[0] = 9;
        ln.Ref.Hz = 8;
        fmt.Println("note:", ln.Name, ln.Pitch.Hz, ln.Marks, ln.Ref.Hz, ln.Weight);
        fmt.Println("source:", on.Name, on.Pitch.Hz, on.Marks, on.Ref.Hz, on.Weight);
        fmt.Println("note back:", otherꓸNote(ln).Name, otherꓸNote(ln).Ref == on.Ref);

        fmt.Println("empty:", libꓸRest(new other.Rest()) == new lib.Rest(), otherꓸRest(new lib.Rest()) == new other.Rest());

        fmt.Println("equal:", libꓸPitch(o) == new lib.Pitch(Hz: 220), libꓸPitch(o) != p);

        switch (libꓸPitch(o)) {
        case var ᴛ when ᴛ == p: {
            fmt.Println("switch: p");
            break;
        }
        case var ᴛ when ᴛ == new lib.Pitch(Hz: 220): {
            fmt.Println("switch: 220");
            break;
        }
        default: {
            fmt.Println("switch: none");
            break;
        }}

        [GoFunc("main.func1")] lib.Pitch convert(other.Pitch v) => libꓸPitch(v);
        fmt.Println("literal:", convert(o).Hz, convert(new other.Pitch(Hz: 3)).Double());

        var c = new channel<lib.Pitch>(2);
        c.ᐸꟷ(libꓸPitch(o));

        var cʗ1 = c;
        goǃ([GoFunc("main.func2")] (lib.Pitch v) => {
            cʗ1.ᐸꟷ(v);
        }, libꓸPitch(new other.Pitch(Hz: 12)));

        var (first, second) = (ᐸꟷ(c), ᐸꟷ(c));
        fmt.Println("channel:", first.Hz, second.Hz);

        foreach (var (i, v) in range(slice<other.Pitch>([new(Hz: 1), new(Hz: 2)]))) {
            fmt.Println("range:", i, libꓸPitch(v).Double());
        }

        var tag = (other.Tag)p;
        fmt.Println("written over:", tag.Hz, tag.Triple(), ((lib.Pitch)tag).Hz);

        defer(ᴛ1 => report("deferred:", ᴛ1), libꓸPitch(o), ref ᒐ);

        o.Hz = 0;
        fmt.Println("last:", o.Hz);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end main_package
