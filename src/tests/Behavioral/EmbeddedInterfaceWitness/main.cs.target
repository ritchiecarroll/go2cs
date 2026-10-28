namespace go;

using fmt = fmt_package;
using reflect = reflect_package;
using iolike = EmbeddedInterfaceWitness.iolike_package;
using EmbeddedInterfaceWitness;

partial class main_package {

partial struct wrapper {
    /*embed*/ public EmbeddedInterfaceWitness.iolike_package.Reader Reader;
    internal @string prefix;
}

internal static @string Write(this wrapper w, @string s) {
    return w.prefix + s;
}

partial struct plain {
    internal @string tag;
}

internal static @string Read(this plain p) {
    return "read:"u8 + p.tag;
}

internal static @string Write(this plain p, @string s) {
    return "p:"u8 + s;
}

partial struct holder {
    public iolike.Reader Reader;
    internal @string prefix;
}

internal static @string Write(this holder h, @string s) {
    return h.prefix + s;
}

internal static void check(@string label, any value) {
    {
        var (rw, ok) = value._<iolike.ReadWriter>(ᐧ); if (ok) {
            fmt.Println(label, "ReadWriter: yes", rw.Read(), rw.Write("x"));
        } else {
            fmt.Println(label, "ReadWriter: no");
        }
    }

    {
        var (r, ok) = value._<iolike.Reader>(ᐧ); if (ok) {
            fmt.Println(label, "Reader: yes", r.Read());
        } else {
            fmt.Println(label, "Reader: no");
        }
    }

    fmt.Println(label, "NumMethod:", reflect.TypeOf(value).NumMethod());
}

internal static void Main() {
    check("wrapper", new wrapper(Reader: new iolike.Base(Tag: "base"), prefix: "w:"));
    check("plain", new plain(tag: "p"));
    check("holder", new holder(Reader: new iolike.Base(Tag: "held"), prefix: "h:"));
    LocalPromotion();
    checkConflicted();
    checkPointerOnly();
}

internal partial struct LocalPromotion_inner /*dyn*/ {
    /*embed*/ public EmbeddedInterfaceWitness.iolike_package.Reader Reader;
}

public static void LocalPromotion() {
    var w = new LocalPromotion_inner(Reader: new iolike.Base(Tag: "local"));
    fmt.Println("local direct:", w.Reader.Read());

    any v = w;

    {
        var (r, ok) = v._<iolike.Reader>(ᐧ); if (ok) {
            fmt.Println("local assert:", r.Read());
        } else {
            fmt.Println("local assert: no");
        }
    }
}

partial struct conflicted {
    /*embed*/ public EmbeddedInterfaceWitness.iolike_package.Base Base;
    /*embed*/ public EmbeddedInterfaceWitness.iolike_package.Reader Reader;
}

internal static void checkConflicted() {
    any v = new conflicted(Base: new iolike.Base(Tag: "conf"));

    {
        var (_, ok) = v._<iolike.Reader>(ᐧ); if (ok) {
            fmt.Println("conflicted Reader: yes");
        } else {
            fmt.Println("conflicted Reader: no");
        }
    }

    fmt.Println("conflicted NumMethod:", reflect.TypeOf(v).NumMethod());
}

partial struct pointerBase {
    internal @string tag;
}

internal static @string Write(this ref pointerBase b, @string s) {
    return "pb:"u8 + s + b.tag;
}

partial struct pointerOnly {
    /*embed*/ public EmbeddedInterfaceWitness.iolike_package.ReadWriter ReadWriter;
    /*embed*/ internal pointerBase pointerBase;
}

// Go method set entry for the promoted 'ReadWriter.Read()' - provided ONLY by the embedded
// interface field in *pointerOnly's method set; see the pointer-only satisfaction record.
internal static @string Read(this pointerOnly recvᴛ) => recvᴛ.ReadWriter.Read();

internal static @string Write(this ref pointerOnly p, @string s) {
    return "po:"u8 + s;
}

internal static void checkPointerOnly() {
    any ptr = Ꮡ(new pointerOnly(nil));

    {
        var (rw, ok) = ptr._<iolike.ReadWriter>(ᐧ); if (ok) {
            fmt.Println("pointerOnly ptr: yes", rw.Write("x"));
        } else {
            fmt.Println("pointerOnly ptr: no");
        }
    }

    any val = new pointerOnly(nil);

    {
        var (_, ok) = val._<iolike.ReadWriter>(ᐧ); if (ok) {
            fmt.Println("pointerOnly val: yes");
        } else {
            fmt.Println("pointerOnly val: no");
        }
    }

    fmt.Println("pointerOnly NumMethod ptr:", reflect.TypeOf(ptr).NumMethod(), "val:", reflect.TypeOf(val).NumMethod());
}

} // end main_package
