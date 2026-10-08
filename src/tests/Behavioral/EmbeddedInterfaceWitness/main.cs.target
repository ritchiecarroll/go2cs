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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object readWriterYesˢ = (@string)"ReadWriter: yes"u8;
private static readonly object readWriterNoˢ = (@string)"ReadWriter: no"u8;
private static readonly object readerYesˢ = (@string)"Reader: yes"u8;
private static readonly object readerNoˢ = (@string)"Reader: no"u8;
private static readonly object numMethodˢ = (@string)"NumMethod:"u8;

internal static void check(@string label, any value) {
    {
        var (rw, ok) = value._<iolike.ReadWriter>(ᐧ); if (ok){
            fmt.Println(label, readWriterYesˢ, rw.Read(), rw.Write("x"u8));
        } else {
            fmt.Println(label, readWriterNoˢ);
        }
    }
    {
        var (r, ok) = value._<iolike.Reader>(ᐧ); if (ok){
            fmt.Println(label, readerYesˢ, r.Read());
        } else {
            fmt.Println(label, readerNoˢ);
        }
    }
    fmt.Println(label, numMethodˢ, reflect.TypeOf(value).NumMethod());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string wrapperˢ = "wrapper"u8;
private static readonly @string plainˢ = "plain"u8;
private static readonly @string holderˢ = "holder"u8;

internal static void Main() {
    check(wrapperˢ, new wrapper(Reader: new iolike.Base(Tag: "base"u8), prefix: "w:"u8));
    check(plainˢ, new plain(tag: "p"u8));
    check(holderˢ, new holder(Reader: new iolike.Base(Tag: "held"u8), prefix: "h:"u8));
    LocalPromotion();
    checkConflicted();
    checkPointerOnly();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object localDirectˢ = (@string)"local direct:"u8;
private static readonly object localAssertˢ = (@string)"local assert:"u8;
private static readonly object localAssertNoˢ = (@string)"local assert: no"u8;

internal partial struct LocalPromotion_inner /*dyn*/ {
    /*embed*/ public EmbeddedInterfaceWitness.iolike_package.Reader Reader;
}

public static void LocalPromotion() {
    var w = new LocalPromotion_inner(Reader: new iolike.Base(Tag: "local"u8));
    fmt.Println(localDirectˢ, w.Reader.Read());
    any v = w;
    {
        var (r, ok) = v._<iolike.Reader>(ᐧ); if (ok){
            fmt.Println(localAssertˢ, r.Read());
        } else {
            fmt.Println(localAssertNoˢ);
        }
    }
}

partial struct conflicted {
    public partial ref EmbeddedInterfaceWitness.iolike_package.Base Base { get; }
    /*embed*/ public EmbeddedInterfaceWitness.iolike_package.Reader Reader;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object conflictedReaderYesˢ = (@string)"conflicted Reader: yes"u8;
private static readonly object conflictedReaderNoˢ = (@string)"conflicted Reader: no"u8;
private static readonly object conflictedNumMethodˢ = (@string)"conflicted NumMethod:"u8;

internal static void checkConflicted() {
    any v = new conflicted(Base: new iolike.Base(Tag: "conf"u8));
    {
        var (_, ok) = v._<iolike.Reader>(ᐧ); if (ok){
            fmt.Println(conflictedReaderYesˢ);
        } else {
            fmt.Println(conflictedReaderNoˢ);
        }
    }
    fmt.Println(conflictedNumMethodˢ, reflect.TypeOf(v).NumMethod());
}

partial struct pointerBase {
    internal @string tag;
}

internal static @string Write(this ref pointerBase b, @string s) {
    return "pb:"u8 + s + b.tag;
}

partial struct pointerOnly {
    /*embed*/ public EmbeddedInterfaceWitness.iolike_package.ReadWriter ReadWriter;
    internal partial ref pointerBase pointerBase { get; }
}

// Go method set entry for the promoted 'ReadWriter.Read()' - provided ONLY by the embedded
// interface field in *pointerOnly's method set; see the pointer-only satisfaction record.
internal static @string Read(this pointerOnly recvᴛ) => recvᴛ.ReadWriter.Read();

internal static @string Write(this ref pointerOnly p, @string s) {
    return "po:"u8 + s;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object pointerOnlyPtrYesˢ = (@string)"pointerOnly ptr: yes"u8;
private static readonly object pointerOnlyPtrNoˢ = (@string)"pointerOnly ptr: no"u8;
private static readonly object pointerOnlyValYesˢ = (@string)"pointerOnly val: yes"u8;
private static readonly object pointerOnlyValNoˢ = (@string)"pointerOnly val: no"u8;
private static readonly object pointerOnlyNumMethodPtrˢ = (@string)"pointerOnly NumMethod ptr:"u8;
private static readonly object valˢ = (@string)"val:"u8;

internal static void checkPointerOnly() {
    any ptr = Ꮡ(new pointerOnly(nil));
    {
        var (rw, ok) = ptr._<iolike.ReadWriter>(ᐧ); if (ok){
            fmt.Println(pointerOnlyPtrYesˢ, rw.Write("x"u8));
        } else {
            fmt.Println(pointerOnlyPtrNoˢ);
        }
    }
    any val = new pointerOnly(nil);
    {
        var (_, ok) = val._<iolike.ReadWriter>(ᐧ); if (ok){
            fmt.Println(pointerOnlyValYesˢ);
        } else {
            fmt.Println(pointerOnlyValNoˢ);
        }
    }
    fmt.Println(pointerOnlyNumMethodPtrˢ, reflect.TypeOf(ptr).NumMethod(), valˢ, reflect.TypeOf(val).NumMethod());
}

} // end main_package
