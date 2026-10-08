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
using XpkgPromotedInnerLib = XpkgPromotedInnerLib_package;
using XpkgPromotedMidLib = XpkgPromotedMidLib_package;
using encoding;
using ꓸꓸꓸany = Span<any>;

partial class main_package {

partial struct Direct {
    public partial ref XpkgPromotedInnerLib_package.Inner Inner { get; }
}

partial struct PtrDirect {
    public partial ref ж<XpkgPromotedInnerLib_package.Inner> Inner { get; }
}

partial struct Outer {
    public partial ref XpkgPromotedMidLib_package.Mid Mid { get; }
}

partial struct loc {
}

internal static inner.Token Tok(this loc _) {
    return new inner.Token(N: 7);
}

partial struct Five {
    public partial ref XpkgPromotedMidLib_package.Mid Mid { get; }
    internal partial ref loc loc { get; }
}

partial struct local {
    public partial ref XpkgPromotedInnerLib_package.Inner Inner { get; }
}

partial struct GCounter {
    internal partial ref local local { get; }
    public partial ref XpkgPromotedMidLib_package.Mid Mid { get; }
}

partial struct local2 {
    public partial ref XpkgPromotedInnerLib_package.Inner Inner { get; }
}

partial struct S6 {
    internal partial ref loc loc { get; }
    internal partial ref local2 local2 { get; }
}

partial struct Within {
    public partial ref XpkgPromotedMidLib_package.Mid2 Mid2 { get; }
}

partial struct Shadow {
    public partial ref XpkgPromotedInnerLib_package.Inner Inner { get; }
    public nint Tok;
}

partial struct localMid {
    public partial ref XpkgPromotedInnerLib_package.Inner Inner { get; }
}

partial struct Mixed {
    internal partial ref localMid localMid { get; }
}

public static nint Name = 0;

partial struct brokenState {
    public partial ref sync_package.Mutex Mutex { get; }
}

partial struct Broken {
    /*embed*/ public io_package.Reader Reader;
    internal partial ref ж<brokenState> brokenState { get; }
}

partial struct Stamp {
    public partial ref time_package.Time Time { get; }
}

partial struct PtrStamp {
    public partial ref ж<time_package.Time> Time { get; }
}

partial struct BufP {
    public partial ref ж<bytes_package.Buffer> Buffer { get; }
}

partial struct deepTok {
}

internal static inner.Token Tok(this deepTok _) {
    return new inner.Token(N: 5);
}

partial struct localTok {
    internal partial ref deepTok deepTok { get; }
}

partial struct OuterS5 {
    public partial ref XpkgPromotedMidLib_package.Mid3 Mid3 { get; }
    internal partial ref localTok localTok { get; }
}

partial struct OuterFD {
    public partial ref XpkgPromotedMidLib_package.MidF MidF { get; }
}

partial struct OuterFS {
    public partial ref XpkgPromotedMidLib_package.PS PS { get; }
}

partial struct TT {
    public partial ref ж<testing_package.T> T { get; }
}

partial struct HideC {
    public partial ref bytes_package.Buffer Buffer { get; }
    /*embed*/ public io_package.WriterTo WriterTo;
}

internal static @string methods(reflectꓸType t) {
    slice<@string> names = default!;
    for (nint i = 0; i < t.NumMethod(); i++) {
        names = append(names, t.Method(i).Name);
    }
    sort.Strings(names);
    return fmt.Sprintf("%d [%s]"u8, t.NumMethod(), strings.Join(names, " "u8));
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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string directˢ = "Direct"u8;
private static readonly @string ptrDirectˢ = "PtrDirect"u8;
private static readonly @string outerˢ = "Outer"u8;
private static readonly @string fiveˢ = "Five"u8;
private static readonly @string gCounterˢ = "GCounter"u8;
private static readonly @string withinˢ = "Within"u8;
private static readonly @string shadowˢ = "Shadow"u8;
private static readonly @string mixedˢ = "Mixed"u8;
private static readonly @string brokenˢ = "Broken"u8;
private static readonly @string stampˢ = "Stamp"u8;
private static readonly @string ptrStampˢ = "PtrStamp"u8;
private static readonly @string bufPˢ = "BufP"u8;
private static readonly @string hideCˢ = "HideC"u8;
private static readonly @string outerS5ˢ = "OuterS5"u8;
private static readonly @string outerFDˢ = "OuterFD"u8;
private static readonly @string outerFSˢ = "OuterFS"u8;
private static readonly @string s5DispatchTokˢ = "S5 dispatch Tok"u8;
private static readonly @string ttValueHasErrorˢ = "TT value has Error"u8;
private static readonly object fiveTokNˢ = (@string)"Five Tok N:"u8;
private static readonly object s6TokNˢ = (@string)"S6 Tok N:"u8;
private static readonly object stampValueIsJsonˢ = (@string)"Stamp value is json.Unmarshaler:"u8;
private static readonly object stampIsJsonUnmarshalerˢ = (@string)"*Stamp is json.Unmarshaler:"u8;
private static readonly object ptrStampValueIsJsonˢ = (@string)"PtrStamp value is json.Unmarshaler:"u8;
private static readonly @string setˢ = "Set"u8;
private static readonly object directValueMethodByNameˢ = (@string)"Direct value MethodByName Set:"u8;
private static readonly object ptrDirectValueˢ = (@string)"PtrDirect value MethodByName Set:"u8;
private static readonly object brokenValueIsSyncLockerˢ = (@string)"Broken value is sync.Locker:"u8;
private static readonly object nameVarˢ = (@string)"Name var:"u8;
private static readonly @string whereDirectCallˢ = "Where direct call"u8;
private static readonly @string whereViaAssertDirectˢ = "Where via assert Direct"u8;
private static readonly @string whereViaAssertOuterˢ = "Where via assert Outer"u8;
private static readonly object directAfterSetThroughˢ = (@string)"Direct after Set through *Direct:"u8;
private static readonly object ptrDirectAfterSetThroughˢ = (@string)"PtrDirect after Set through the value:"u8;
private static readonly @string samePValueMethodsˢ = "SameP value methods"u8;
private static readonly @string samePPtrMethodsˢ = "SameP ptr methods"u8;
private static readonly @string sameVValueMethodsˢ = "SameV value methods"u8;
private static readonly @string sameVPtrMethodsˢ = "SameV ptr methods"u8;
private static readonly @string samePValueMethod0Callˢ = "SameP value Method(0) call"u8;
private static readonly @string samePValueAssertCallPingˢ = "SameP value assert+call Ping"u8;
private static readonly @string notSatisfiedˢ = "not satisfied"u8;
private static readonly @string sameVValueAssertPingˢ = "SameV value assert Ping"u8;
private static readonly @string sameVAssertCallPingˢ = "*SameV assert+call Ping"u8;

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
    report(directˢ, new Direct(nil));
    report(ptrDirectˢ, new PtrDirect(nil));
    report(outerˢ, new Outer(nil));
    report(fiveˢ, new Five(nil));
    report(gCounterˢ, new GCounter(nil));
    report("S6"u8, new S6(nil));
    report(withinˢ, new Within(nil));
    report(shadowˢ, new Shadow(nil));
    report(mixedˢ, new Mixed(nil));
    report(brokenˢ, new Broken(nil));
    report(stampˢ, new Stamp(nil));
    report(ptrStampˢ, new PtrStamp(nil));
    report(bufPˢ, new BufP(nil));
    report(hideCˢ, new HideC(nil));
    report(outerS5ˢ, new OuterS5(nil));
    report(outerFDˢ, new OuterFD(nil));
    report(outerFSˢ, new OuterFS(nil));
    report("TT"u8, new TT(nil));
    probe(s5DispatchTokˢ, () => fmt.Sprint(((any)new OuterS5(nil))._<main_type>().Tok().N));
    probe(ttValueHasErrorˢ, () => {
        var (_, okΔ1) = ((any)new TT(nil))._<main_typeᴛ1>(ᐧ);
        return fmt.Sprint(okΔ1);
    });
    fmt.Println(new Direct(nil), new Outer(nil), new Mixed(nil));
    any f = new Five(nil);
    {
        var (t, okΔ2) = f._<main_type>(ᐧ); if (okΔ2) {
            fmt.Println(fiveTokNˢ, t.Tok().N);
        }
    }
    any s6 = new S6(nil);
    {
        var (t, okΔ3) = s6._<main_type>(ᐧ); if (okΔ3) {
            fmt.Println(s6TokNˢ, t.Tok().N);
        }
    }
    var (_, ok) = ((any)new Stamp(nil))._<json.Unmarshaler>(ᐧ);
    fmt.Println(stampValueIsJsonˢ, ok);
    (_, ok) = ((any)(Ꮡ(new Stamp(nil))))._<json.Unmarshaler>(ᐧ);
    fmt.Println(stampIsJsonUnmarshalerˢ, ok);
    (_, ok) = ((any)new PtrStamp(nil))._<json.Unmarshaler>(ᐧ);
    fmt.Println(ptrStampValueIsJsonˢ, ok);
    (_, ok) = reflect.TypeOf(new Direct(nil)).MethodByName(setˢ);
    fmt.Println(directValueMethodByNameˢ, ok);
    (_, ok) = reflect.TypeOf(new PtrDirect(nil)).MethodByName(setˢ);
    fmt.Println(ptrDirectValueˢ, ok);
    any l = new Broken(nil);
    (_, ok) = l._<Δsync.Locker>(ᐧ);
    fmt.Println(brokenValueIsSyncLockerˢ, ok);
    fmt.Println(nameVarˢ, Name);
    probe(whereDirectCallˢ, () => fmt.Sprint(new Direct(nil).Inner.Where()));
    probe(whereViaAssertDirectˢ, () => fmt.Sprint(((any)new Direct(nil))._<main_typeᴛ2>().Where()));
    probe(whereViaAssertOuterˢ, () => fmt.Sprint(((any)new Outer(nil))._<main_typeᴛ2>().Where()));
    var d = Ꮡ(new Direct(nil));
    ((any)d.OrTypedNil())._<main_typeᴛ3>().Set(4);
    fmt.Println(directAfterSetThroughˢ, (~d).X);
    var pd = new PtrDirect(Ꮡ(new inner.Inner(nil)));
    ((any)pd)._<main_typeᴛ3>().Set(6);
    fmt.Println(ptrDirectAfterSetThroughˢ, pd.X);
    probe(samePValueMethodsˢ, () => methods(reflect.TypeOf(mid.NewSameP())));
    probe(samePPtrMethodsˢ, () => methods(reflect.TypeOf(Ꮡ(new mid.SameP(nil)))));
    probe(sameVValueMethodsˢ, () => methods(reflect.TypeOf(new mid.SameV(nil))));
    probe(sameVPtrMethodsˢ, () => methods(reflect.TypeOf(Ꮡ(new mid.SameV(nil)))));
    probe(samePValueMethod0Callˢ, () => fmt.Sprint(reflect.ValueOf(mid.NewSameP()).Method(0).Call(default!)[0].Int()));
    probe(samePValueAssertCallPingˢ, () => {
        var (p, okΔ4) = ((any)mid.NewSameP())._<main_typeᴛ4>(ᐧ);
        if (!okΔ4) {
            return notSatisfiedˢ;
        }
        return fmt.Sprint(p.Ping());
    });
    probe(sameVValueAssertPingˢ, () => {
        var (_, okΔ5) = ((any)new mid.SameV(nil))._<main_typeᴛ4>(ᐧ);
        return fmt.Sprint(okΔ5);
    });
    probe(sameVAssertCallPingˢ, () => {
        var (p, okΔ6) = ((any)(Ꮡ(new mid.SameV(nil))))._<main_typeᴛ4>(ᐧ);
        if (!okΔ6) {
            return notSatisfiedˢ;
        }
        return fmt.Sprint(p.Ping());
    });
}

} // end main_package
