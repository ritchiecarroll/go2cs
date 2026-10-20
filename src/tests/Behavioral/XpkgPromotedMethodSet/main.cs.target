namespace go;

using bytes = bytes_package;
using json = encoding.json_package;
using fmt = fmt_package;
using Δio = io_package;
using reflect = reflect_package;
using sort = sort_package;
using strings = strings_package;
using Δsync = sync_package;
using testing = testing_package;
using time = time_package;
using inner = XpkgPromotedInnerLib_package;
using mid = XpkgPromotedMidLib_package;
using System.Runtime.InteropServices;
using XpkgPromotedInnerLib = XpkgPromotedInnerLib_package;
using XpkgPromotedMidLib = XpkgPromotedMidLib_package;
using encoding;
using ꓸꓸꓸany = Span<any>;

partial class main_package {

partial struct Direct {
    /*embed*/ public XpkgPromotedInnerLib_package.Inner Inner;
}

partial struct PtrDirect {
    /*embed*/ public ж<XpkgPromotedInnerLib_package.Inner> Inner;
}

partial struct Outer {
    /*embed*/ public XpkgPromotedMidLib_package.Mid Mid;
}

partial struct loc {
}

internal static inner.Token Tok(this loc _) {
    return new inner.Token(N: 7);
}

[StructLayout(LayoutKind.Explicit, Size = 16)] partial struct Five {
    [FieldOffset(0)] /*embed*/ public XpkgPromotedMidLib_package.Mid Mid;
    [FieldOffset(8)] /*embed*/ internal readonly loc loc;
}

partial struct local {
    /*embed*/ public XpkgPromotedInnerLib_package.Inner Inner;
}

partial struct GCounter {
    /*embed*/ internal local local;
    /*embed*/ public XpkgPromotedMidLib_package.Mid Mid;
}

partial struct local2 {
    /*embed*/ public XpkgPromotedInnerLib_package.Inner Inner;
}

[StructLayout(LayoutKind.Explicit, Size = 8)] partial struct S6 {
    [FieldOffset(0)] /*embed*/ internal readonly loc loc;
    [FieldOffset(0)] /*embed*/ internal local2 local2;
}

partial struct Within {
    /*embed*/ public XpkgPromotedMidLib_package.Mid2 Mid2;
}

partial struct Shadow {
    /*embed*/ public XpkgPromotedInnerLib_package.Inner Inner;
    public nint Tok;
}

partial struct localMid {
    /*embed*/ public XpkgPromotedInnerLib_package.Inner Inner;
}

partial struct Mixed {
    /*embed*/ internal localMid localMid;
}

public static nint Name = 0;

partial struct brokenState {
    /*embed*/ public sync_package.Mutex Mutex;
}

partial struct Broken {
    /*embed*/ public io_package.Reader Reader;
    /*embed*/ internal ж<brokenState> brokenState;
}

partial struct Stamp {
    /*embed*/ public time_package.Time Time;
}

partial struct PtrStamp {
    /*embed*/ public ж<time_package.Time> Time;
}

partial struct BufP {
    /*embed*/ public ж<bytes_package.Buffer> Buffer;
}

partial struct deepTok {
}

internal static inner.Token Tok(this deepTok _) {
    return new inner.Token(N: 5);
}

partial struct localTok {
    /*embed*/ internal deepTok deepTok;
}

[StructLayout(LayoutKind.Explicit, Size = 16)] partial struct OuterS5 {
    [FieldOffset(0)] /*embed*/ public XpkgPromotedMidLib_package.Mid3 Mid3;
    [FieldOffset(8)] /*embed*/ internal readonly localTok localTok;
}

partial struct OuterFD {
    /*embed*/ public XpkgPromotedMidLib_package.MidF MidF;
}

partial struct OuterFS {
    /*embed*/ public XpkgPromotedMidLib_package.PS PS;
}

partial struct TT {
    /*embed*/ public ж<testing_package.T> T;
}

partial struct HideC {
    /*embed*/ public bytes_package.Buffer Buffer;
    /*embed*/ public io_package.WriterTo WriterTo;
}

internal static @string methods(reflectꓸType t) {
    var namesᴷ = new stackAppend<@string>();

    slice<@string> names = default!;

    for (nint i = 0; i < t.NumMethod(); i++) {
        names = append(namesᴷ, names, t.Method(i).Name);
    }

    sort.Strings(names);

    return fmt.Sprintf("%d [%s]"u8, t.NumMethod(), strings.Join(names, " "));
}

internal static void report(@string label, any v) {
    var t = reflect.TypeOf(v);
    fmt.Printf("%-10s value %s\n"u8, label, methods(t));
    fmt.Printf("%-10s ptr   %s\n"u8, label, methods(reflect.PointerTo(t)));
}

internal static void probe(@string label, Func<@string> f) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var r = recover(); if (r != default!) {
                    fmt.Printf("%s: PANIC %v\n"u8, label, r);
                }
            }
        }, ref ᒐ);

        fmt.Printf("%s: %s\n"u8, label, f());
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial interface main_type /*dyn*/ {
    inner.Token Tok();
}

internal partial interface main_typeᴛ1 /*dyn*/ {
    void Error(params ꓸꓸꓸany ʗp);
}

internal partial interface main_typeᴛ2 /*dyn*/ {
    bool Where();
}

internal partial interface main_typeᴛ3 /*dyn*/ {
    void Set(nint _Δp0);
}

internal partial interface main_typeᴛ4 /*dyn*/ {
    nint Ping();
}

internal static void Main() {
    report("Direct", new Direct(nil));
    report("PtrDirect", new PtrDirect(nil));
    report("Outer", new Outer(nil));
    report("Five", new Five(nil));
    report("GCounter", new GCounter(nil));
    report("S6", new S6(nil));
    report("Within", new Within(nil));
    report("Shadow", new Shadow(nil));
    report("Mixed", new Mixed(nil));
    report("Broken", new Broken(nil));
    report("Stamp", new Stamp(nil));
    report("PtrStamp", new PtrStamp(nil));
    report("BufP", new BufP(nil));
    report("HideC", new HideC(nil));
    report("OuterS5", new OuterS5(nil));
    report("OuterFD", new OuterFD(nil));
    report("OuterFS", new OuterFS(nil));
    report("TT", new TT(nil));

    probe("S5 dispatch Tok", () => fmt.Sprint(((any)new OuterS5(nil))._<main_type>().Tok().N));
    probe("TT value has Error", () => {
        var (_, okΔ1) = ((any)new TT(nil))._<main_typeᴛ1>(ᐧ);
        return fmt.Sprint(okΔ1);
    });

    fmt.Println(new Direct(nil), new Outer(nil), new Mixed(nil));

    any f = new Five(nil);

    {
        var (t, okΔ2) = f._<main_type>(ᐧ); if (okΔ2) {
            fmt.Println("Five Tok N:", t.Tok().N);
        }
    }

    any s6 = new S6(nil);

    {
        var (t, okΔ3) = s6._<main_type>(ᐧ); if (okΔ3) {
            fmt.Println("S6 Tok N:", t.Tok().N);
        }
    }

    var (_, ok) = ((any)new Stamp(nil))._<json.Unmarshaler>(ᐧ);
    fmt.Println("Stamp value is json.Unmarshaler:", ok);
    (_, ok) = ((any)(Ꮡ(new Stamp(nil))))._<json.Unmarshaler>(ᐧ);
    fmt.Println("*Stamp is json.Unmarshaler:", ok);
    (_, ok) = ((any)new PtrStamp(nil))._<json.Unmarshaler>(ᐧ);
    fmt.Println("PtrStamp value is json.Unmarshaler:", ok);
    (_, ok) = reflect.TypeOf(new Direct(nil)).MethodByName("Set");
    fmt.Println("Direct value MethodByName Set:", ok);
    (_, ok) = reflect.TypeOf(new PtrDirect(nil)).MethodByName("Set");
    fmt.Println("PtrDirect value MethodByName Set:", ok);

    any l = new Broken(nil);
    (_, ok) = l._<Δsync.Locker>(ᐧ);
    fmt.Println("Broken value is sync.Locker:", ok);
    fmt.Println("Name var:", Name);

    probe("Where direct call", () => fmt.Sprint(new Direct(nil).Inner.Where()));
    probe("Where via assert Direct", () => fmt.Sprint(((any)new Direct(nil))._<main_typeᴛ2>().Where()));
    probe("Where via assert Outer", () => fmt.Sprint(((any)new Outer(nil))._<main_typeᴛ2>().Where()));

    var d = Ꮡ(new Direct(nil));
    ((any)d.OrTypedNil())._<main_typeᴛ3>().Set(4);
    fmt.Println("Direct after Set through *Direct:", (~d).X);

    var pd = new PtrDirect(Ꮡ(new inner.Inner(nil)));
    ((any)pd)._<main_typeᴛ3>().Set(6);
    fmt.Println("PtrDirect after Set through the value:", pd.X);

    probe("SameP value methods", () => methods(reflect.TypeOf(mid.NewSameP())));
    probe("SameP ptr methods", () => methods(reflect.TypeOf(Ꮡ(new mid.SameP(nil)))));
    probe("SameV value methods", () => methods(reflect.TypeOf(new mid.SameV(nil))));
    probe("SameV ptr methods", () => methods(reflect.TypeOf(Ꮡ(new mid.SameV(nil)))));
    probe("SameP value Method(0) call", () => fmt.Sprint(reflect.ValueOf(mid.NewSameP()).Method(0).Call(default!)[0].Int()));
    probe("SameP value assert+call Ping", () => {
        var (p, okΔ4) = ((any)mid.NewSameP())._<main_typeᴛ4>(ᐧ);

        if (!okΔ4) {
            return "not satisfied";
        }

        return fmt.Sprint(p.Ping());
    });
    probe("SameV value assert Ping", () => {
        var (_, okΔ5) = ((any)new mid.SameV(nil))._<main_typeᴛ4>(ᐧ);

        return fmt.Sprint(okΔ5);
    });
    probe("*SameV assert+call Ping", () => {
        var (p, okΔ6) = ((any)(Ꮡ(new mid.SameV(nil))))._<main_typeᴛ4>(ᐧ);

        if (!okΔ6) {
            return "not satisfied";
        }

        return fmt.Sprint(p.Ping());
    });
}

} // end main_package
