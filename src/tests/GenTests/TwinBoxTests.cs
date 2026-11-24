// TwinBoxTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

extern alias golib;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using go2cs;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenTests;

/// <summary>
/// M:N (experimental, opt-in), N2, the BOX TWIN (docs/phase4/DESIGN-mn-box-twin.md, revision 1; COORD's GO after the re-review): a
/// `this ref T` method's twin takes the receiver's box, `Mꞏtwin(this ж<T> Ꮡt)`, and each call names a box without a
/// copy (9.1's rows) or stays plain. The two must-fixes of the re-review are pinned: a ref local crossing a ж-typed
/// member refuses the twin by name; a VALUE receiver's box overload (which copies) is never followed.
/// </summary>
[TestClass]
public class TwinBoxTests
{
    private const string Header = """
        using System;
        using static go.builtin;

        namespace go;

        public static partial class box_package
        {
            internal static channel<nint> ch;

            public partial struct S { public nint y; }

            public partial struct T { public nint x; public S inner; public slice<S> items; }

            public partial struct N { public nint v; public ж<N> next; }

            internal static void b(this ref T t) { ᐸꟷ(ch); t.x = 1; }

            internal static void sPark(this ref S s) { ᐸꟷ(ch); s.y = 2; }

        """;

    private const string Footer = "}\n";

    // The box twin's own receiver, and a caller's ref local over a box.
    private const string OwnAndLocal = """
            internal static void a(this ref T t) { t.b(); t.x++; }

            public static void run(ж<T> Ꮡx)
            {
                ref var x = ref Ꮡx.DerefOrNull();
                x.a();
            }

        """;

    // A call through a ж<T> expression binds RecvGenerator's box overload; its receiver is the box.
    private const string ViaOverload = """
            public static void runP(ж<T> Ꮡp)
            {
                Ꮡp.b();
            }

        """;

    // The currency guard (9.4): the root reassigned other than in the re-alias form, so `x` no longer names Ꮡx's object.
    private const string NotCurrent = """
            public static void stale(ж<T> Ꮡx)
            {
                ref var x = ref Ꮡx.DerefOrNull();
                Ꮡx = Ꮡ(new T());
                x.b();
                ᐸꟷ(ch);
            }

        """;

    // Must-fix 1: a ref local whose source crosses a ж-typed member.
    private const string SharedMember = """
            internal static void walk(this ref N n)
            {
                ref var m = ref n.next.DerefOrNull();
                ᐸꟷ(ch);
                m.v = 1;
            }

        """;

    // Closure arm O: a HEAP local initialized from a ж-typed member is derived again from its own box, never through the
    // member: no shared-member refusal (runtime's `ref var block = ref heap<ж<…>>(…a.current…, out var Ꮡblock)`).
    private const string HeapFromMember = """
            internal static void hw(this ref N n)
            {
                ref var m = ref heap<ж<N>>(n.next, out var Ꮡm);
                ᐸꟷ(ch);
                m = default!;
            }

        """;

    // A [GoCopyBound] receiver is bound through a copy: never boxed (9.3).
    private const string CopyBound = """
            [GoCopyBound]
            internal static void cb(this ref T t) { ᐸꟷ(ch); }

        """;

    // Must-fix 2: a VALUE receiver's box overload copies (`var t = ~Ꮡt;`): never followed.
    private const string ValueReceiver = """
            internal static nint v(this T t) { ᐸꟷ(ch); return t.x; }

            public static void runV(ж<T> Ꮡp)
            {
                Ꮡp.v();
            }

        """;

    // A field of a boxed receiver, and a slice element of one (9.1's field and element rows).
    private const string FieldAndElement = """
            public static void runF(ж<T> Ꮡx, nint i)
            {
                ref var x = ref Ꮡx.DerefOrNull();
                x.inner.sPark();
                x.items[i].sPark();
            }

        """;

    // A function whose ONLY park is a call with no box to name (a stack local, named-plain as ruled): it parks nowhere,
    // gets no twin, and no twin's rewriter sees the call.
    private const string OnlyUnboxed = """
            public static void onlyF()
            {
                S v = default;
                Action keepV = () => v.y++; // PB-5 (12.5): captured, so v stays unboxable
                v.sPark();
            }

        """;

    // A field of a type with no [GoFieldAccess] pointer but the field token every converted struct has (TypeGenerator's
    // `Field‿Tokens`): the design's field row, `Ꮡx.of(T.Ꮡf)` (the probe's `nh.c`), here on the box twin's OWN receiver,
    // whose `Ꮡt` SN-1 binds through its typed stand-in.
    private const string FieldToken = """
            internal static void own(this ref T t) { t.inner.sPark(); }

        """;

    // Closure arm N's CS9217 (runtime's gcWork.putObjBatch): in a box twin the receiver is a REF LOCAL, though the plain
    // method's model binds it as the `this ref` parameter. A store through it with an awaited call on the right
    // (`(w.wbuf1, w.wbuf2) = (w.wbuf2, getempty())`, and the plain store `d.a = pk() + d.b`) must spill or stay plain.
    private const string StoreThroughReceiver = """
            public partial struct D { public nint a; public nint b; }

            internal static nint pk() { return ᐸꟷ(ch); }

            internal static void swapPark(this ref D d) { (d.a, d.b) = (d.b, pk()); }

            internal static void addPark(this ref D d) { d.a = pk() + d.b; }

        """;

    // Closure arm N's report: a REFUSED caller (a ref-struct parameter: reflike, as fmt's `ss.consume(…, sstring ok)`)
    // never has a twin, so its calls are plain under its own refusal's name, not named-plain box calls (SN-7).
    private const string RefusedCaller = """
            internal static void rq(this ref S q, ReadOnlySpan<byte> r) { q.sPark(); }

        """;

    // N2 code review, finding 1: a ref local over a PACKAGE-LEVEL pointer `gp` would be derived again from `gp` after a
    // park, though another goroutine may have replaced it (Go read it once). Its twin is refused; its call names no box.
    private const string StaticRoot = """
            internal static ж<T> gp = Ꮡ(new T());

            public static void runG()
            {
                ref var x = ref gp.DerefOrNull();
                x.b();
                ᐸꟷ(ch);
            }

        """;

    // Finding 2: the box-overload guards, pinned by the `// V` row of each followed call. Two same-name methods of
    // different signatures; a [GoCopyBound] method and a value method behind their box overloads (never followed).
    private const string OverloadGuards = """
            internal static void ov(this ref T t, nint a) { ᐸꟷ(ch); }

            internal static void ov(this ref T t, @string s) { ᐸꟷ(ch); }

            [GoCopyBound]
            internal static void cbp(this ref T t) { ᐸꟷ(ch); }

            internal static nint vv(this T t) { ᐸꟷ(ch); return t.x; }

            public static void runOv(ж<T> Ꮡp)
            {
                Ꮡp.ov((nint)1);
                Ꮡp.ov((@string)"x");
                Ꮡp.cbp();
                Ꮡp.vv();
            }

        """;

    // Finding 3: a receiver named by a C# keyword (`func (in *T) M()` converts to `this ref T @in`).
    private const string KeywordReceiver = """
            internal static void kw(this ref T @in) { ᐸꟷ(ch); @in.x = 1; }

        """;

    // Finding 4: a GENERIC receiver's box overload is not followed (its type parameters are the overload's own); the
    // call stays plain and is named `generic-unfollowed`.
    private const string GenericReceiver = """
            public partial struct GG<P> { public P v; }

            internal static void gm<P>(this ref GG<P> g) { ᐸꟷ(ch); }

            public static void runGen(ж<GG<nint>> Ꮡg)
            {
                Ꮡg.gm();
            }

        """;

    // Finding 5: with the switch off the shared-member detector is off, so a non-receiver function the base twins keeps
    // its twin (the base by construction).
    private const string SharedMemberLocal = """
            public static void smf(ж<N> Ꮡn)
            {
                ref var n = ref Ꮡn.DerefOrNull();
                ref var m = ref n.next.DerefOrNull();
                ᐸꟷ(ch);
                m.v = 1;
            }

        """;

    // Finding 7: a ref local RE-POINTED at another box names no box (it would hand the callee the declared root's object).
    private const string RePointed = """
            public static void rp(ж<T> Ꮡx, ж<T> Ꮡy)
            {
                ref var x = ref Ꮡx.DerefOrNull();
                x = ref Ꮡy.DerefOrNull();
                x.b();
                ᐸꟷ(ch);
            }

        """;

    // Finding 6: a referenced box-SHAPED twin whose return is not a task is no box twin (display strings matched it).
    private const string LibraryBadTwin = """
        using System;

        namespace go;

        public static partial class lib_package
        {
            public partial struct M
            {
                public nint v;

                public static bool operator ==(M a, M b) => a.v == b.v;
                public static bool operator !=(M a, M b) => a.v != b.v;
                public override bool Equals(object? obj) => obj is M other && other == this;
                public override int GetHashCode() => v.GetHashCode();
            }

            public static void rt(this ref M m) { }
            public static int rtꞏtwin(this ж<M> Ꮡm) => 0;
        }
        """;

    // P3's read of 80c099ee33: a referenced assembly whose twins are INTERNAL, with a friend grant to the caller (as
    // InternalsVisibleTo hands sync's and time's internal hand twins to a friend). The caller must bind plain: a twin
    // outside the compilation's own assembly binds only when it is public. TWINS is replaced by the twins' accessibility.
    private const string LibraryFriendTwins = """
        using System;

        [assembly: System.Runtime.CompilerServices.InternalsVisibleTo("twinbox")]

        namespace go;

        public static partial class lib_package
        {
            public partial struct M
            {
                public nint v;

                public static bool operator ==(M a, M b) => a.v == b.v;
                public static bool operator !=(M a, M b) => a.v != b.v;
                public override bool Equals(object? obj) => obj is M other && other == this;
                public override int GetHashCode() => v.GetHashCode();
            }

            public static void wt() { }
            TWINS static global::go.golib.GoTask wtꞏtwin() => default;

            public static void bk(this ref M m) { }
            public static void bk(this ж<M> Ꮡm) { }
            TWINS static global::go.golib.GoTask bkꞏtwin(this ж<M> Ꮡm) => default;
        }
        """;

    private const string UsesFriendTwins = """
            public partial struct F { public lib_package.M m; }

            public static void useFriend(ж<F> Ꮡf)
            {
                ref var f = ref Ꮡf.DerefOrNull();
                lib_package.wt();
                f.m.bk();
                ᐸꟷ(ch);
            }

        """;

    private const string UsesBadTwin = """
            public partial struct H { public lib_package.M m; }

            public static void useRt(ж<H> Ꮡh)
            {
                ref var h = ref Ꮡh.DerefOrNull();
                h.m.rt();
                ᐸꟷ(ch);
            }

        """;

    // byref-param (DESIGN-mn-box-twin.md 12): a ref-lowered pointer parameter `ref T p` gets the box, as a receiver does.
    private const string ByRefParams = """
            internal static void pp(ref T p) { ᐸꟷ(ch); p.x = 1; }

            internal static void fw(ref T p) { pp(ref p); }

            internal static void two(ref T a, ref T b) { ᐸꟷ(ch); a.x = b.x; }

            public static void runDeref(ж<T> px) { pp(ref (px).DerefOrNull()); }

            public static void runLocal(ж<T> Ꮡx)
            {
                ref var x = ref Ꮡx.DerefOrNull();
                pp(ref x);
            }

            public static void runTwo(ж<T> px) { two(ref (px).DerefOrNull(), ref (px).DerefOrNull()); }

            public static void runEl(slice<T> xs, nint i) { pp(ref xs[i]); }

            public static void runStack()
            {
                T st = default;
                Action keepSt = () => st.x++; // PB-5 (12.5): captured, so st stays a stack local and the call plain
                pp(ref st);
                ᐸꟷ(ch);
            }

        """;

    // P3's review of the byref-param cut: findings 1, 2, 5 and 7, each form pinned by its own unit.
    private const string ReviewPins = """
            internal static void mixed2(ref T a, ref T b) { ᐸꟷ(ch); a.x = b.x; }

            public static void runMixed(ж<T> px)
            {
                T st = default;
                Action keepSt = () => st.x++; // PB-5 (12.5): captured, so st stays unboxable
                mixed2(ref (px).DerefOrNull(), ref st);
                ᐸꟷ(ch);
            }

            internal static ж<T> Ꮡpk = Ꮡ(new T());
            internal static ref T pk => ref Ꮡpk.Value;

            public static void runPkg() { pp(ref pk); }

            public static void runPkgShadow(ж<T> Ꮡpk) { pp(ref pk); }

            public static void runValue(ж<T> px) { pp(ref px.Value); ᐸꟷ(ch); }

            public static void runTilde(ж<ж<T>> ppx) { pp(ref (~ppx).DerefOrNull()); }

            internal static void kwp(ref T @in) { ᐸꟷ(ch); @in.x = 1; }

            internal static void kwf(ref T @in) { kwp(ref @in); }

            public static void runKw(ж<T> px) { kwp(ref (px).DerefOrNull()); }

            public static void runHeapStore()
            {
                ref var hs = ref heap(new T(), out var Ꮡhs);
                hs = new T();
                pp(ref hs);
            }

            public static void runHeapBoxStore()
            {
                ref var hb = ref heap(new T(), out var Ꮡhb);
                Ꮡhb = Ꮡ(new T());
                pp(ref hb);
                ᐸꟷ(ch);
            }

            public static void runHeapRepoint()
            {
                ref var hr = ref heap(new T(), out var Ꮡhr);
                hr = ref pk;
                pp(ref hr);
                ᐸꟷ(ch);
            }

        """;

    // byref-param, 12.1's other enumerated forms (P4's cut): a heap local, a field of a current ref local, ruling 10.4's
    // nil-checked field, a generic callee, a `this ref` method with another `ref` (byref-mixed), and a COPY box
    // (`Ꮡ(v)`, never handed).
    private const string ByRefForms = """
            internal static void ppS(ref S s) { ᐸꟷ(ch); s.y = 1; }

            internal static void gp<TV>(ref TV p) { ᐸꟷ(ch); }

            public static void runHeap()
            {
                ref var v = ref heap(new T(), out var Ꮡv);
                pp(ref v);
            }

            public static void runField(ж<T> Ꮡy)
            {
                ref var y = ref Ꮡy.DerefOrNull();
                ppS(ref y.inner);
            }

            public static void runNonnil(ж<T> pe) { ppS(ref nonnil(ref (pe).DerefOrNull()).inner); }

            public static void runGeneric(ж<nint> pn) { gp(ref (pn).DerefOrNull()); }

            public static void runCopyBox(T st)
            {
                pp(ref Ꮡ(st).DerefOrNull());
                ᐸꟷ(ch);
            }

        """;

    // G's next step 1 (11-16 16:19Z): the two-level field chain `&e.f.g` (x509's `nonnil(ref in).TBSCSR.PublicKey`): the
    // field pointer of a field pointer, the nil check on the first level only, as Go panics at the outer nil. A chain
    // through a POINTER field is not this shape (its second level is a dereference, not a field of e's object).
    private const string TwoLevelField = """
            public partial struct U { public nint z; public T t; public ж<T> pt; }

            public static void runChain(ж<U> pu) { ppS(ref nonnil(ref (pu).DerefOrNull()).t.inner); }

            public static void runChainPtr(ж<U> pu) { ppS(ref nonnil(ref (nonnil(ref (pu).DerefOrNull()).pt).DerefOrNull()).inner); }

        """;

    // G's ruling 1 (a): a later argument that parks, after a box argument (`f(p, <-ch)`), is NOT moved ahead of the call's
    // statement; it stays a blocking call in place, so the box is read where the plain emission reads it.
    private const string LaterParkingArgument = """
            internal static nint rw(ref T c, nint k) { ᐸꟷ(ch); return c.x + k; }

            public static void runLater(ж<T> px) { rw(ref (px).DerefOrNull(), ᐸꟷ(ch)); }

        """;

    // The spill (12.1 item 5, section 12.4): a parking CALL in a later argument of a box call. Its box is a value, so
    // C# evaluates it once, ahead of the await, and the inner call awaits in place. A named-plain call keeps its `ref`
    // and the inner call stays plain (CS8178); a ref local read after the call keeps the existing spill rule.
    private const string LaterParkingCall = """
            internal static nint parksN() { ᐸꟷ(ch); return 1; }

            internal static nint add(nint a, nint b) => a + b;

            internal static void rwv(ref T c, nint k) { ᐸꟷ(ch); c.x = k; }

            internal static void rwv2(nint j, ref T c, nint k) { ᐸꟷ(ch); c.x = j + k; }

            public static void runCall(ж<T> px) { rwv(ref (px).DerefOrNull(), parksN()); }

            public static void runNested(ж<T> px) { rwv(ref (px).DerefOrNull(), add(1, parksN())); }

            public static void runMid(ж<T> px) { rwv2(parksN(), ref (px).DerefOrNull(), parksN()); }

            public static void runStack()
            {
                T v = default;
                Action keepV = () => v.x++; // PB-5 (12.5): captured, so v stays unboxable
                rwv(ref v, parksN());
            }

        """;

    // The spill's second rule (section 12.4): the box call's value is STORED through a ref local, so the box call moves
    // ahead of its statement; the later parking call inside it is judged against the moved `var ᴛ = await …`, where no
    // store remains (crypto/tls's `hs.masterSecret = extMasterFromPreMasterSecret(…, ref (hs.suite)…, hs.finishedHash.Sum())`).
    private const string LaterCallUnderStore = """
            internal static nint parksM() { ᐸꟷ(ch); return 1; }

            internal static nint rvs(ref T c, nint k) { ᐸꟷ(ch); return c.x + k; }

            internal static nint pv(nint j, nint k) { ᐸꟷ(ch); return j + k; }

            public static void runStore(ж<T> px, ж<T> Ꮡq)
            {
                ref var q = ref Ꮡq.DerefOrNull();
                q.x = rvs(ref (px).DerefOrNull(), parksM());
            }

            public static void runStorePlain(ж<T> Ꮡq, nint j)
            {
                ref var q = ref Ꮡq.DerefOrNull();
                q.x = pv(j, parksM());
            }

            public static void runStoreReadAfter(ж<T> px, ж<T> Ꮡq)
            {
                ref var q = ref Ꮡq.DerefOrNull();
                q.x = rvs(ref (px).DerefOrNull(), parksM()) + q.x;
            }

        """;

    // PB-5 (section 12.5): a STACK local whose reference reaches a box twin. The twin declares it as the converter declares
    // a heap local, `ref var x = ref heap<T>(init, out var Ꮡx)`, and hands Ꮡx; Go heap-allocates such an escaping
    // address too. Refused, named-plain with a reason: captured, not a single block-level declaration, in a loop, or a
    // name Ꮡx already taken.
    private const string StackLocals = """
            internal static void bump(ref nint n) { ᐸꟷ(ch); n++; }

            public static nint runSwaps()
            {
                nint swaps = default;
                bump(ref swaps);
                bump(ref swaps);
                return swaps;
            }

            public static void runStackRecv()
            {
                var tl = new T();
                tl.b();
            }

            public static void runCaptured()
            {
                nint cp = default;
                Action a = () => cp++;
                bump(ref cp);
                a();
            }

            public static void runLoopLocal(nint n)
            {
                for (nint i = 0; i < n; i++)
                {
                    nint lp = default;
                    bump(ref lp);
                }
            }

            public static void runDeconstructed()
            {
                var (dc, ec) = (default(nint), default(nint));
                bump(ref dc);
            }

            public static void runLoopDeconstructing()
            {
                foreach (var (kd, wd) in new (nint, nint)[] { (1, 2) })
                {
                    nint ld = kd;
                    bump(ref ld);
                }
            }

            public static void runLoopRecv(nint n)
            {
                for (nint i = 0; i < n; i++)
                {
                    var tr = new T();
                    tr.b();
                }
            }

            public static void runNameTaken()
            {
                nint nm = default;
                ж<nint> Ꮡnm = default;
                bump(ref nm);
            }

            internal static nint parksI() { ᐸꟷ(ch); return 3; }

            public static nint runParkingInit()
            {
                nint pi = parksI();
                bump(ref pi);
                return pi;
            }

            public static void runShadow(bool f)
            {
                if (f)
                {
                    nint sh = default;
                    bump(ref sh);
                }
                else
                {
                    nint sh = 1;
                    sh++;
                }
            }

        """;

    // A hand-written `this ref` method with another `ref` parameter (the converter emits none: ref-lowering skips methods).
    // Beside it, each modifier RecvGenerator's box overload carries, once: `out` and `in` on a ref receiver, a `ref` on a
    // VALUE receiver (the second construction site), and a defaulted parameter, whose default the call must not carry.
    private const string MixedRef = """
            internal static void mx(this ref T t, ref T u) { ᐸꟷ(ch); }
            internal static void mo(this ref T t, out T u, in T w) { u = w; }
            internal static void mv(this T t, ref T u) { u = t; }
            internal static nint md(this ref T t, nint u = 5) => u;

        """;

    // `out` stays refused: Go has no out parameter; the four parking `out` functions are hand-owned.
    private const string OutParam = """
            internal static void op(out T p) { p = default; ᐸꟷ(ch); }

        """;

    // The review's nit: an element under a package variable is named-plain in the first cut.
    private const string PackageVarElement = """
            internal static ж<T> Ꮡpv = Ꮡ(new T());
            internal static ref T pv => ref Ꮡpv.Value;

            public static void runPv(nint i)
            {
                pv.items[i].sPark();
                ᐸꟷ(ch);
            }

        """;

    // Closure arm O: a REFERENCED method with a twin of its own shape (sync's hand-written `Lockꞏtwin(this ref Mutex)`)
    // beside a box-shaped one (stage (c)'s interface-forwarder twin, `*Mutex` implements Locker), and one with ONLY a box
    // twin (an N2 box twin of another package). Compiled first, without generators, and referenced by image.
    private const string Library = """
        using System;

        namespace go;

        public static partial class lib_package
        {
            public partial struct M
            {
                public nint v;

                // A converted struct's equality, as TypeGenerator gives it: the consumer's struct H compares its field.
                public static bool operator ==(M a, M b) => a.v == b.v;
                public static bool operator !=(M a, M b) => a.v != b.v;
                public override bool Equals(object? obj) => obj is M other && other == this;
                public override int GetHashCode() => v.GetHashCode();
            }

            public static void lk(this ref M m) { }
            public static void lk(this ж<M> Ꮡm) { }
            public static global::go.golib.GoTask lkꞏtwin(this ref M m) => default;
            public static global::go.golib.GoTask lkꞏtwin(this ж<M> Ꮡm) => default;

            public static void bx(this ref M m) { }
            public static global::go.golib.GoTask bxꞏtwin(this ж<M> Ꮡm) => default;
        }
        """;

    private const string UsesLibrary = """
            public partial struct H { public lib_package.M m; }

            public static void useLk(ж<H> Ꮡh)
            {
                ref var h = ref Ꮡh.DerefOrNull();
                h.m.lk();
            }

            public static void useLkBox(ж<lib_package.M> Ꮡm)
            {
                Ꮡm.lk();
            }

            public static void useBx(ж<H> Ꮡh)
            {
                ref var h = ref Ꮡh.DerefOrNull();
                h.m.bx();
            }

        """;

    private static (string twins, Diagnostic[] errors) Run(Dictionary<string, string>? extra, params string[] shapes) =>
        RunWith(null, extra, shapes);

    internal static (string twins, Diagnostic[] errors) RunWith(string? library, Dictionary<string, string>? extra, params string[] shapes)
    {
        CSharpParseOptions options = new(LanguageVersion.CSharp13);
        string source = Header + string.Concat(shapes) + Footer;
        IEnumerable<MetadataReference> references = References();
        byte[]? libraryImage = null;

        if (library is not null)
        {
            Compilation lib = CSharpCompilation.Create("twinboxlib", [CSharpSyntaxTree.ParseText(library, options, path: @"C:\twinboxlib\lib.cs")], References(),
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

            // A library built WITH twins (LibraryTwins, set by the row): its generated twins, as a twin-built package ships them.
            if (LibraryTwins)
            {
                Dictionary<string, string> libraryProperties = new() { ["build_property.GoTwins"] = "true" };

                foreach (KeyValuePair<string, string> pair in LibraryProperties ?? [])
                    libraryProperties[pair.Key] = pair.Value;

                CSharpGeneratorDriver.Create([new RecvGenerator(), new FieldAccessGenerator(), new TypeGenerator(), new TwinGenerator()], parseOptions: options,
                        optionsProvider: new BuildProperties(libraryProperties))
                    .RunGeneratorsAndUpdateCompilation(lib, out lib, out _);
            }

            using MemoryStream image = new();
            Microsoft.CodeAnalysis.Emit.EmitResult built = lib.Emit(image);
            Assert.IsTrue(built.Success, "library: " + Errors(built.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray()));
            libraryImage = image.ToArray();
            references = references.Append(MetadataReference.CreateFromImage(libraryImage)).ToArray();
        }

        CSharpCompilation compilation = CSharpCompilation.Create("twinbox",
            [CSharpSyntaxTree.ParseText(source, options, path: @"C:\twinbox\main.cs")],
            references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

        Dictionary<string, string> properties = new() { ["build_property.GoTwins"] = "true" };

        foreach (KeyValuePair<string, string> pair in extra ?? [])
            properties[pair.Key] = pair.Value;

        // RecvGenerator (the box overloads), FieldAccessGenerator (the field pointers) and TypeGenerator (every converted
        // struct's field tokens, by rule) beside the twin generator, as a converted project builds: the twin generator
        // binds against its siblings' output, so the final compilation must carry it too.
        CSharpGeneratorDriver.Create([new RecvGenerator(), new FieldAccessGenerator(), new TypeGenerator(), new TwinGenerator()], parseOptions: options, optionsProvider: new BuildProperties(properties))
            .RunGeneratorsAndUpdateCompilation(compilation, out Compilation output, out var generated);

        LastGeneratorDiagnostics = [.. generated];

        global::go2cs.Tests.TwinStampTests.AssertStampsHold(output, libraryImage); // 7d: every stamp agrees with the compiler

        string twins = string.Join("\n", output.SyntaxTrees.Skip(compilation.SyntaxTrees.Length).Select(tree => tree.ToString()));

        if (Environment.GetEnvironmentVariable("GO2CS_DUMP_TWINS") is { Length: > 0 } dump)
            File.WriteAllText(dump, twins);

        // EMIT, not GetDiagnostics: an async lowering error is reported only while the state machine is built.
        using MemoryStream emittedImage = new();
        Microsoft.CodeAnalysis.Emit.EmitResult emitted = output.Emit(emittedImage);
        LastImage = emitted.Success ? emittedImage.ToArray() : null;

        return (twins, emitted.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray());
    }

    // The image RunWith last emitted on this thread, for a row that RUNS a twin (TwinSurfaceTests F4).
    [ThreadStatic] internal static byte[]? LastImage;

    // When set, RunWith builds its library with the twin generator on (TwinSurfaceTests, the refused-parker row).
    [ThreadStatic] internal static bool LibraryTwins;

    // Extra build properties for a LibraryTwins build (N1-4b: the library's own GoTwinsWeak).
    [ThreadStatic] internal static Dictionary<string, string>? LibraryProperties;

    // The diagnostics the generators of RunWith's last run reported (the guard's rows, TwinSurfaceTests F9 and F10).
    [ThreadStatic] internal static Diagnostic[]? LastGeneratorDiagnostics;

    internal static string Errors(Diagnostic[] errors) => string.Join("\n", errors.Select(error => error.ToString()));

    [TestMethod]
    public void ABoxTwinTakesTheBoxAndItsCallersHandItTheirs()
    {
        (string twins, Diagnostic[] errors) = Run(null, OwnAndLocal);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        StringAssert.Contains(twins, "bꞏtwin(this global::go.ж<T> Ꮡt)", "b's twin takes the box");
        StringAssert.Contains(twins, "ref var t = ref Ꮡt.DerefOrNull();", "the prologue derives the receiver's ref");
        StringAssert.Contains(twins, "await Ꮡt.bꞏtwin()", "a's twin hands b its OWN box");
        StringAssert.Contains(twins, "await Ꮡx.aꞏtwin()", "run's twin hands a the box its ref local is over");
    }

    [TestMethod]
    public void ACallThroughABoxExpressionAwaitsTheBoxTwin()
    {
        (string twins, Diagnostic[] errors) = Run(null, ViaOverload);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        StringAssert.Contains(twins, "await Ꮡp.bꞏtwin()", "the box overload's receiver is the box");
    }

    [TestMethod]
    public void AStaleRefLocalNamesNoBoxAndTheCallStaysPlain()
    {
        (string twins, Diagnostic[] errors) = Run(null, NotCurrent);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        Assert.IsFalse(twins.Contains("Ꮡx.bꞏtwin(", StringComparison.Ordinal), "a stale ref local hands no box");
        StringAssert.Contains(twins, "|b|x", "the plain call is named in the report");
    }

    [TestMethod]
    public void ARefLocalCrossingABoxTypedMemberRefusesTheTwinByName()
    {
        (string twins, Diagnostic[] errors) = Run(null, SharedMember);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        Assert.IsFalse(twins.Contains("walkꞏtwin(", StringComparison.Ordinal), "walk is refused a twin");
        StringAssert.Contains(twins, "|walk|ref-shared-member", "the refusal names it");
    }

    [TestMethod]
    public void AHeapLocalFromABoxTypedMemberIsNoSharedMemberChain()
    {
        (string twins, Diagnostic[] errors) = Run(null, HeapFromMember);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        Assert.IsFalse(twins.Contains("|hw|ref-shared-member", StringComparison.Ordinal), "a heap local is not refused");
        StringAssert.Contains(twins, "hwꞏtwin(this global::go.ж<N> Ꮡn)", "hw is a box twin");
    }

    [TestMethod]
    public void ACopyBoundReceiverIsNeverBoxed()
    {
        (string twins, Diagnostic[] errors) = Run(null, CopyBound);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        Assert.IsFalse(twins.Contains("cbꞏtwin(", StringComparison.Ordinal), "cb gets no box twin");
        StringAssert.Contains(twins, "|cb|byref-recv", "it keeps its by-ref refusal");
    }

    [TestMethod]
    public void AValueReceiversBoxOverloadIsNeverFollowed()
    {
        (string twins, Diagnostic[] errors) = Run(null, ValueReceiver);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        Assert.IsFalse(twins.Contains("Ꮡp.vꞏtwin(", StringComparison.Ordinal), "the copying box overload is not followed to a twin");

        // The danger itself: a VALUE receiver given a box twin would SHARE where Go copies. Its twin keeps `this T`.
        StringAssert.Contains(twins, "vꞏtwin(this T t)", "a value receiver's twin takes its value, as Go copies it");
        Assert.IsFalse(twins.Contains("vꞏtwin(this global::go.ж<T>", StringComparison.Ordinal), "a value receiver never gets a box twin");
    }

    [TestMethod]
    public void AFieldAndAnElementOfABoxedReceiverNameTheirPointers()
    {
        (string twins, Diagnostic[] errors) = Run(null, FieldAndElement);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        StringAssert.Contains(twins, "await global::go.builtin.Ꮡ(x.items, i).sParkꞏtwin()", "the element pointer, index once");

        // The fixture records no [GoFieldAccess] type, so `Ꮡx.Ꮡinner` does not bind; the field token does.
        StringAssert.Contains(twins, "await Ꮡx.of(global::go.box_package.T.Field‿Tokens.Ꮡinner‿).sParkꞏtwin()", "the field pointer, by its token");
    }

    [TestMethod]
    public void AFieldWithOnlyItsTokenNamesThePointerThroughIt()
    {
        (string twins, Diagnostic[] errors) = Run(null, FieldToken);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        StringAssert.Contains(twins, "ownꞏtwin(this global::go.ж<T> Ꮡt)", "own's twin takes the box");
        StringAssert.Contains(twins, "await Ꮡt.of(global::go.box_package.T.Field‿Tokens.Ꮡinner‿).sParkꞏtwin()", "the field token's pointer");
        Assert.IsFalse(twins.Contains("|sPark|t.inner", StringComparison.Ordinal), "no named-plain row for it");
    }

    [TestMethod]
    public void AStoreThroughTheBoxReceiverHoldsNoRefAcrossAnAwait()
    {
        (string twins, Diagnostic[] errors) = Run(null, StoreThroughReceiver);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        StringAssert.Contains(twins, "addParkꞏtwin(this global::go.ж<D> Ꮡd)", "addPark is a box twin");
        StringAssert.Contains(twins, "= await pkꞏtwin(); d = ref Ꮡd.DerefOrNull();", "the plain store's call spills ahead, the receiver derived again");
    }

    [TestMethod]
    public void AReferencedMethodWithItsOwnRefTwinKeepsTheBaseForm()
    {
        (string twins, Diagnostic[] errors) = RunWith(Library, null, UsesLibrary);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        StringAssert.Contains(twins, "await h.m.lkꞏtwin()", "its own-shape twin is awaited in place, as at the base");
        Assert.IsFalse(twins.Split('\n').Any(line => line.StartsWith("// P ", StringComparison.Ordinal) && line.Contains("|lk|", StringComparison.Ordinal)),
                       "and never named-plain");
        StringAssert.Contains(twins, "await Ꮡm.lkꞏtwin()", "a box-overload call parks as at the base, through the overload's own twin");
        StringAssert.Contains(twins, ".bxꞏtwin()", "a box-only twin is reached through the box");
        Assert.IsFalse(twins.Contains("await h.m.bxꞏtwin()", StringComparison.Ordinal), "never through the ref");
    }

    [TestMethod]
    public void WithTheSwitchOffAReferencedBoxTwinIsNotATarget()
    {
        (string twins, Diagnostic[] errors) = RunWith(Library, new() { ["build_property.GoTwinsBoxRecv"] = "false" }, UsesLibrary);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        StringAssert.Contains(twins, "await h.m.lkꞏtwin()", "the base form");
        StringAssert.Contains(twins, "await Ꮡm.lkꞏtwin()", "the base form through the box overload");
        Assert.IsFalse(twins.Contains("bxꞏtwin(", StringComparison.Ordinal), "a box-only twin is unknown with the switch off: bx is no parker");
        Assert.IsFalse(twins.Contains("// P ", StringComparison.Ordinal), "and nothing is named-plain");
    }

    [TestMethod]
    public void ARefusedCallersCallsAreNotNamedPlain()
    {
        (string twins, Diagnostic[] errors) = Run(null, RefusedCaller);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        Assert.IsFalse(twins.Contains("rqꞏtwin(", StringComparison.Ordinal), "rq is refused (a ref-struct parameter)");
        Assert.IsFalse(twins.Contains("|sPark|q", StringComparison.Ordinal), "its call is no named-plain row");
    }

    [TestMethod]
    public void R1_ARefLocalOverAPackageLevelPointerIsRefused()
    {
        (string twins, Diagnostic[] errors) = Run(null, StaticRoot);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        Assert.IsFalse(twins.Contains("gp.bꞏtwin(", StringComparison.Ordinal), "no box from a package-level root");
        StringAssert.Contains(twins, "|runG|ref-shared-member", "the twin is refused by name");
    }

    [TestMethod]
    public void R2_TheBoxOverloadGuardsArePinnedByTheirRows()
    {
        (string twins, Diagnostic[] errors) = Run(null, OverloadGuards);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        StringAssert.Contains(twins, "|ov|go.box_package.ov(ref go.box_package.T, nint)", "ov(1) follows to the nint method");
        StringAssert.Contains(twins, "|ov|go.box_package.ov(ref go.box_package.T, go.@string)", "ov(\"x\") follows to the string method");
        Assert.IsFalse(twins.Split('\n').Any(line => line.StartsWith("// V ", StringComparison.Ordinal) && line.Contains("|cbp|", StringComparison.Ordinal)),
                       "a [GoCopyBound] method is never followed");
        Assert.IsFalse(twins.Split('\n').Any(line => line.StartsWith("// V ", StringComparison.Ordinal) && line.Contains("|vv|", StringComparison.Ordinal)),
                       "a value method is never followed");
    }

    [TestMethod]
    public void R3_AKeywordNamedReceiverKeepsItsEscape()
    {
        (string twins, Diagnostic[] errors) = Run(null, KeywordReceiver);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        StringAssert.Contains(twins, "kwꞏtwin(this global::go.ж<T> Ꮡin)", "the box parameter");
        StringAssert.Contains(twins, "ref var @in = ref Ꮡin.DerefOrNull();", "the prologue keeps the @");
    }

    [TestMethod]
    public void R4_AGenericReceiversUnfollowedCallIsNamed()
    {
        (string twins, Diagnostic[] errors) = Run(null, GenericReceiver);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        StringAssert.Contains(twins, "|gm|Ꮡg|generic-unfollowed", "the plain call is named, with its reason");
    }

    [TestMethod]
    public void R5_WithTheSwitchOffNoSharedMemberRefusal()
    {
        (string on, Diagnostic[] errorsOn) = Run(null, SharedMemberLocal);
        (string off, Diagnostic[] errorsOff) = Run(new() { ["build_property.GoTwinsBoxRecv"] = "false" }, SharedMemberLocal);

        Assert.AreEqual(0, errorsOn.Length + errorsOff.Length, Errors([.. errorsOn, .. errorsOff]));
        StringAssert.Contains(on, "|smf|ref-shared-member", "refused with N2 on");
        Assert.IsFalse(off.Contains("|smf|ref-shared-member", StringComparison.Ordinal), "the base twins it, switch off");
        StringAssert.Contains(off, "smfꞏtwin(", "its base twin");
    }

    [TestMethod]
    public void R6_AReferencedBoxShapedTwinThatReturnsNoTaskIsNoBoxTwin()
    {
        (string twins, Diagnostic[] errors) = RunWith(LibraryBadTwin, null, UsesBadTwin);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        Assert.IsFalse(twins.Contains(".rtꞏtwin()", StringComparison.Ordinal), "never awaited");
    }

    [TestMethod]
    public void R6b_AnInternalTwinOfAReferencedAssemblyIsNotBoundThroughAFriendGrant()
    {
        (string twins, Diagnostic[] errors) = RunWith(LibraryFriendTwins.Replace("TWINS", "internal"), null, UsesFriendTwins);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        Assert.IsFalse(twins.Contains("wtꞏtwin(", StringComparison.Ordinal), "an internal function twin of another assembly is never awaited");
        Assert.IsFalse(twins.Contains("bkꞏtwin(", StringComparison.Ordinal), "an internal box twin of another assembly is never awaited");
    }

    [TestMethod]
    public void R6c_APublicTwinOfAReferencedAssemblyIsBound_TheControl()
    {
        (string twins, Diagnostic[] errors) = RunWith(LibraryFriendTwins.Replace("TWINS", "public"), null, UsesFriendTwins);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        StringAssert.Contains(twins, "wtꞏtwin(", "the same shape, public, is awaited");
        StringAssert.Contains(twins, "bkꞏtwin(", "the same box shape, public, is awaited");
    }

    [TestMethod]
    public void R7_ARePointedRefLocalNamesNoBox()
    {
        (string twins, Diagnostic[] errors) = Run(null, RePointed);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        Assert.IsFalse(twins.Contains("Ꮡx.bꞏtwin(", StringComparison.Ordinal), "never the declared root's box");
        StringAssert.Contains(twins, "|b|x", "the plain call is named");
    }

    [TestMethod]
    public void P1_ARefParameterTakesItsBox()
    {
        (string twins, Diagnostic[] errors) = Run(null, ByRefParams);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        StringAssert.Contains(twins, "ppꞏtwin(global::go.ж<T> Ꮡp)", "the parameter's box");
        StringAssert.Contains(twins, "ref var p = ref Ꮡp.DerefOrNull();", "its prologue");
        StringAssert.Contains(twins, "await ppꞏtwin(px)", "a `(x).DerefOrNull()` argument hands x, read once");
        StringAssert.Contains(twins, "await ppꞏtwin(Ꮡp)", "a forwarded box parameter hands its own box");
        StringAssert.Contains(twins, "await ppꞏtwin(Ꮡx)", "a current ref local hands its root");
        StringAssert.Contains(twins, "await ppꞏtwin(global::go.builtin.Ꮡ(xs, i))", "an element hands its pointer, index once");
    }

    [TestMethod]
    public void P2_TwoParametersEachTakeABox()
    {
        (string twins, Diagnostic[] errors) = Run(null, ByRefParams);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        StringAssert.Contains(twins, "twoꞏtwin(global::go.ж<T> Ꮡa, global::go.ж<T> Ꮡb)", "two box parameters");
        StringAssert.Contains(twins, "await twoꞏtwin(px, px)", "two arguments over one box, as two Go pointers may");
    }

    [TestMethod]
    public void P3_AStackLocalArgumentStaysPlainAndNamed()
    {
        (string twins, Diagnostic[] errors) = Run(null, ByRefParams);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        Assert.IsFalse(twins.Contains("ppꞏtwin(st", StringComparison.Ordinal), "no box for a stack local");
        StringAssert.Contains(twins, "|pp|st", "the plain call is named");
    }

    [TestMethod]
    public void P4_AnOutParameterStaysRefused()
    {
        (string twins, Diagnostic[] errors) = Run(null, OutParam);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        Assert.IsFalse(twins.Contains("opꞏtwin(", StringComparison.Ordinal), "no twin");
        StringAssert.Contains(twins, "|op|byref-out", "refused by name");
    }

    [TestMethod]
    public void P5_TheParamSwitchOffKeepsByRefParamRefused()
    {
        (string twins, Diagnostic[] errors) = Run(new() { ["build_property.GoTwinsBoxParam"] = "false" }, ByRefParams);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        Assert.IsFalse(twins.Contains("ppꞏtwin(", StringComparison.Ordinal), "no parameter box twin with the switch off");
        StringAssert.Contains(twins, "|pp|byref-param", "pp is refused as before");
    }

    [TestMethod]
    public void P6_TheCopyControlCopiesParametersToo()
    {
        (string twins, Diagnostic[] errors) = Run(new() { ["build_property.GoTwinsBoxCopy"] = "true" }, ByRefParams);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        StringAssert.Contains(twins, "var ᴛcopyp = Ꮡp.DerefOrNull(); ref var p = ref ᴛcopyp;", "the control copies, on purpose");
    }

    [TestMethod]
    public void P7_AHeapLocalHandsItsOwnBox()
    {
        (string twins, Diagnostic[] errors) = Run(null, ByRefParams, ByRefForms);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        StringAssert.Contains(twins, "await ppꞏtwin(Ꮡv)", "a heap local's own box");
    }

    [TestMethod]
    public void P8_AFieldArgumentHandsTheFieldPointer()
    {
        (string twins, Diagnostic[] errors) = Run(null, ByRefParams, ByRefForms);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        StringAssert.Contains(twins, "await ppSꞏtwin(Ꮡy.", "a field of a current ref local: the field pointer from its root");
        StringAssert.Contains(twins, "await ppSꞏtwin(pe.", "ruling 10.4's nil-checked field: the field pointer from e's box");
    }

    [TestMethod]
    public void P9_AGenericCalleeTakesItsBox()
    {
        (string twins, Diagnostic[] errors) = Run(null, ByRefParams, ByRefForms);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        StringAssert.Contains(twins, "gpꞏtwin<TV>(global::go.ж<TV> Ꮡp)", "the generic twin");
        StringAssert.Contains(twins, "await gpꞏtwin(pn)", "its call binds");
    }

    [TestMethod]
    public void P10_ARefMethodWithAnotherRefIsRefusedMixed()
    {
        (string twins, Diagnostic[] errors) = Run(null, MixedRef);

        // RecvGenerator's box overload keeps the second parameter's `ref` in its signature and its call (it was CS1620, the
        // `ref` passed bare), so the whole compilation, sibling generators included, is held to zero errors.
        Assert.AreEqual(0, errors.Length, Errors(errors));
        StringAssert.Contains(twins, "mx(this ж<global::go.box_package.T> Ꮡt, ref global::go.box_package.T u)", "the box overload takes u by ref");
        StringAssert.Contains(twins, "t.mx(ref u)", "and passes it by ref");
        StringAssert.Contains(twins, "mo(this ж<global::go.box_package.T> Ꮡt, out global::go.box_package.T u, in global::go.box_package.T w)", "out and in in the signature");
        StringAssert.Contains(twins, "t.mo(out u, in w)", "and in the call");
        StringAssert.Contains(twins, "mv(this ж<global::go.box_package.T> Ꮡt, ref global::go.box_package.T u)", "a value receiver's overload takes u by ref");
        StringAssert.Contains(twins, "t.mv(ref u)", "and passes it by ref");
        StringAssert.Contains(twins, "md(this ж<global::go.box_package.T> Ꮡt, nint u = 5)", "the default stays on the declaration");
        StringAssert.Contains(twins, "t.md(u)", "the call passes the caller's argument, not `u = 5`");
        Assert.IsFalse(twins.Contains("t.md(u = 5)", StringComparison.Ordinal), "no assignment in the call");
        Assert.IsFalse(twins.Contains("mxꞏtwin(", StringComparison.Ordinal), "no twin");
        StringAssert.Contains(twins, "|mx|byref-mixed", "refused by name");
    }

    [TestMethod]
    public void P11_ACopyBoxIsNeverHanded()
    {
        (string twins, Diagnostic[] errors) = Run(null, ByRefParams, ByRefForms);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        Assert.IsFalse(twins.Contains("ppꞏtwin(Ꮡ(st)", StringComparison.Ordinal), "builtin.Ꮡ(v) copies v: never a box argument");
        StringAssert.Contains(twins, "|pp|Ꮡ(st).DerefOrNull()", "the plain call is named");
    }

    [TestMethod]
    public void P12_TheArgCopyControlHandsCopies()
    {
        (string twins, Diagnostic[] errors) = Run(new() { ["build_property.GoTwinsBoxArgCopy"] = "true" }, ByRefParams);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        StringAssert.Contains(twins, "await ppꞏtwin(global::go.builtin.Ꮡ((px).DerefOrNull()))", "the call-site control copies, on purpose");
        StringAssert.Contains(twins, "ARGCOPY(control)", "and says so in the report");
    }

    [TestMethod]
    public void P14_AMixedCallWithOneUnboxableArgumentStaysPlainAndNamed()
    {
        (string twins, Diagnostic[] errors) = Run(null, ByRefParams, ReviewPins);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        Assert.IsFalse(twins.Contains("await mixed2ꞏtwin(", StringComparison.Ordinal), "all or nothing: one stack local keeps the call plain");
        StringAssert.Contains(twins, "|mixed2|st", "named by its first unboxable argument");
    }

    [TestMethod]
    public void P15_APackageVariableHandsItsStaticBox()
    {
        (string twins, Diagnostic[] errors) = Run(null, ByRefParams, ReviewPins);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        StringAssert.Contains(twins, "await ppꞏtwin(Ꮡpk)", "a package variable's own static box");
    }

    [TestMethod]
    public void P16_AValueArgumentStaysPlainAndNamed()
    {
        (string twins, Diagnostic[] errors) = Run(null, ByRefParams, ReviewPins);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        string twin = twins.Split('\n').Single(line => line.Contains("runValueꞏtwin(", StringComparison.Ordinal));

        // `ref E.Value` faults at the call when E is nil; a handed box would fault later, at the callee's first
        // dereference, possibly after a park.
        Assert.IsFalse(twin.Contains("ppꞏtwin(", StringComparison.Ordinal), "no box for `ref px.Value`: " + twin);
        StringAssert.Contains(twins, "|pp|px.Value", "named-plain");
    }

    [TestMethod]
    public void P17_ADereferencedPointerHandsTheInnerBoxReadOnce()
    {
        (string twins, Diagnostic[] errors) = Run(null, ByRefParams, ReviewPins);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        StringAssert.Contains(twins, "await ppꞏtwin(~ppx)", "`~pp` read once at the call, as Go reads *pp");
    }

    [TestMethod]
    public void P18_AKeywordNamedParameterKeepsItsEscape()
    {
        (string twins, Diagnostic[] errors) = Run(null, ByRefParams, ReviewPins);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        StringAssert.Contains(twins, "kwpꞏtwin(global::go.ж<T> Ꮡin)", "the box parameter");
        StringAssert.Contains(twins, "ref var @in = ref Ꮡin.DerefOrNull();", "the prologue keeps the keyword escape");
        StringAssert.Contains(twins, "await kwpꞏtwin(Ꮡin)", "a forwarded keyword-named parameter hands its box");
    }

    [TestMethod]
    public void P19_AHeapLocalStoredWholeStillHandsItsBox()
    {
        (string twins, Diagnostic[] errors) = Run(null, ByRefParams, ReviewPins);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        StringAssert.Contains(twins, "await ppꞏtwin(Ꮡhs)", "`v = value` writes through the box, which still names v");
    }

    [TestMethod]
    public void P20_AHeapLocalWhoseBoxIsReassignedStaysPlainAndNamed()
    {
        (string twins, Diagnostic[] errors) = Run(null, ByRefParams, ReviewPins);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        Assert.IsFalse(twins.Contains("await ppꞏtwin(Ꮡhb)", StringComparison.Ordinal), "`Ꮡv = ...` re-points the box away from v");
        StringAssert.Contains(twins, "|pp|hb", "named-plain");
    }

    [TestMethod]
    public void P23_APackageVariableShadowedByAParameterNamedItsBoxStaysPlain()
    {
        (string twins, Diagnostic[] errors) = Run(null, ByRefParams, ReviewPins);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        // P3's re-read, nit 1: `Ꮡpk` binds to runPkgShadow's PARAMETER at the call, not to pk's static box.
        Assert.AreEqual(1, twins.Split("await ppꞏtwin(Ꮡpk)").Length - 1, "only runPkg hands the static box");
        StringAssert.Contains(twins, "|pp|pk", "the shadowed call is named-plain");
    }

    [TestMethod]
    public void P24_AHeapLocalRePointedStaysPlainAndNamed()
    {
        (string twins, Diagnostic[] errors) = Run(null, ByRefParams, ReviewPins);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        // P3's re-read, nit 3: `v = ref …` re-points v away from its heap box.
        Assert.IsFalse(twins.Contains("ppꞏtwin(Ꮡhr)", StringComparison.Ordinal), "`hr = ref pk` no longer names Ꮡhr's object");
        StringAssert.Contains(twins, "|pp|hr", "named-plain");
    }

    [TestMethod]
    public void P21_ATwoLevelFieldChainHandsTheFieldPointerOfAFieldPointer()
    {
        (string twins, Diagnostic[] errors) = Run(null, ByRefParams, ByRefForms, TwoLevelField);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        StringAssert.Contains(twins, "await ppSꞏtwin(pu.of(global::go.box_package.U.Field‿Tokens.Ꮡt‿).of(global::go.box_package.T.Field‿Tokens.Ꮡinner‿))",
            "FieldBox twice over e's box; the first level's token faults at a nil e, as Go's nonnil does");
        Assert.IsFalse(twins.Contains("|ppS|nonnil(ref (pu).DerefOrNull()).t.inner", StringComparison.Ordinal), "no named-plain row for it");
    }

    [TestMethod]
    public void P22_AFieldChainThroughAPointerFieldStaysNamedPlain()
    {
        (string twins, Diagnostic[] errors) = Run(null, ByRefParams, ByRefForms, TwoLevelField);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        // `&e.pt.inner`: Go reads e.pt at the call (nil-checking e), then takes inner's address in *e.pt (nil-checking
        // e.pt); the handed root is the pointer read once, never a field pointer of e's object.
        Assert.IsFalse(twins.Contains("await ppSꞏtwin(pu.of(global::go.box_package.U.Field‿Tokens.Ꮡpt‿).of(", StringComparison.Ordinal),
            "never the field pointer of the POINTER field");
        StringAssert.Contains(twins, "|ppS|nonnil(ref (nonnil(ref (pu).DerefOrNull()).pt).DerefOrNull()).inner", "named-plain");
    }

    [TestMethod]
    public void P13_ALaterParkingArgumentIsNotMovedAheadOfABoxCall()
    {
        (string twins, Diagnostic[] errors) = Run(null, ByRefParams, LaterParkingArgument);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        // Amended by the spill (12.4): the receive now awaits in place; the box px is still read first, where the plain
        // emission reads it, so the f(p, <-ch) divergence (MnLaterArg) is unchanged and nothing moves ahead of the statement.
        StringAssert.Contains(twins, "await rwꞏtwin(px, (await ᐸꟷꞏtwin(ch)))", "the inner receive stays in place, after the box, and awaits");
        Assert.IsFalse(twins.Contains("runLaterꞏtwin(ж<T> px) { var ᴛtwin", StringComparison.Ordinal), "nothing spilled ahead of the statement");
    }

    [TestMethod]
    public void P25_ALaterParkingCallUnderABoxCallAwaitsInPlace()
    {
        (string twins, Diagnostic[] errors) = Run(null, LaterParkingCall);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        StringAssert.Contains(twins, "await rwvꞏtwin(px, (await parksNꞏtwin()))", "the later call awaits; the box px is read once, ahead of it");
    }

    [TestMethod]
    public void P26_ANestedLaterParkingCallUnderABoxCallAwaitsInPlace()
    {
        (string twins, Diagnostic[] errors) = Run(null, LaterParkingCall);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        StringAssert.Contains(twins, "await rwvꞏtwin(px, add(1, (await parksNꞏtwin())))", "a call nested in a later argument awaits too");
    }

    [TestMethod]
    public void P27_ParkingCallsOnBothSidesOfABoxArgumentAwaitInArgumentOrder()
    {
        (string twins, Diagnostic[] errors) = Run(null, LaterParkingCall);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        StringAssert.Contains(twins, "await rwv2ꞏtwin((await parksNꞏtwin()), px, (await parksNꞏtwin()))", "left to right: call, box, call");
    }

    [TestMethod]
    public void P28_ANamedPlainCallsLaterParkingCallMovesAheadOfItsStackRef()
    {
        (string twins, Diagnostic[] errors) = Run(null, LaterParkingCall);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        // Row 7's Go-order move (DATA-mn-row7-refblock.md): `ref v` to a stack local (no box) was evaluated before the
        // parking call, so the call stayed plain (CS8178). Taking v's address is not ordered against a call in Go, so
        // the call now moves ahead of the statement and parks; the plain rwv takes `ref v` after it.
        StringAssert.Contains(twins, "var ᴛtwin1 = await parksNꞏtwin(); rwv(ref v, ᴛtwin1);", "the inner call moves ahead and awaits");
    }

    [TestMethod]
    public void P29_TheCallSiteCopyControlAlsoAwaitsTheLaterCall()
    {
        (string twins, Diagnostic[] errors) = Run(new() { ["build_property.GoTwinsBoxArgCopy"] = "true" }, LaterParkingCall);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        StringAssert.Contains(twins, "await rwvꞏtwin(global::go.builtin.Ꮡ((px).DerefOrNull()), (await parksNꞏtwin()))", "the copy is a value too");
    }

    [TestMethod]
    public void P30_ALaterParkingCallInsideAMovedBoxCallAwaits()
    {
        (string twins, Diagnostic[] errors) = Run(null, LaterCallUnderStore);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        StringAssert.Contains(twins, "= await rvsꞏtwin(px, (await parksMꞏtwin()));", "the box call moves ahead of the store, and the inner call awaits inside it");
    }

    [TestMethod]
    public void P31_ALaterParkingCallInsideAMovedPlainArgumentCallAwaits()
    {
        (string twins, Diagnostic[] errors) = Run(null, LaterCallUnderStore);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        StringAssert.Contains(twins, "= await pvꞏtwin(j, (await parksMꞏtwin()));", "the rule is not limited to box calls: any moved call");
    }

    [TestMethod]
    public void P32_ARefLocalReadAfterTheMovedCallKeepsItsOwnRule()
    {
        (string twins, Diagnostic[] errors) = Run(null, LaterCallUnderStore);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        Assert.IsFalse(twins.Contains("rvsꞏtwin(px, (await parksMꞏtwin())) + q.x", StringComparison.Ordinal), "q.x is read after the call: never awaited in place before it");
    }

    [TestMethod]
    public void P33_AStackLocalHandedToABoxTwinBecomesAHeapLocalInTheTwin()
    {
        (string twins, Diagnostic[] errors) = Run(null, StackLocals);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        StringAssert.Contains(twins, "ref var swaps = ref global::go.builtin.heap<nint>(default, out var Ꮡswaps);", "declared as a heap local, in the twin only");
        StringAssert.Contains(twins, "await bumpꞏtwin(Ꮡswaps)", "the call hands the box");
        StringAssert.Contains(twins, "swaps = ref Ꮡswaps.ValueSlot;", "Rule R derives it again after the await");
        Assert.IsFalse(twins.Contains("|bump|swaps", StringComparison.Ordinal), "no longer named-plain");
    }

    [TestMethod]
    public void P34_AStackLocalReceiverOfABoxTwinBecomesAHeapLocalInTheTwin()
    {
        (string twins, Diagnostic[] errors) = Run(null, StackLocals);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        StringAssert.Contains(twins, "ref var tl = ref global::go.builtin.heap<global::go.box_package.T>(new T(), out var Ꮡtl);", "the receiver local, a heap local in the twin");
        StringAssert.Contains(twins, "await Ꮡtl.bꞏtwin()", "the receiver's box");
    }

    [TestMethod]
    public void P35_ACapturedStackLocalStaysPlainWithItsReason()
    {
        (string twins, Diagnostic[] errors) = Run(null, StackLocals);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        Assert.IsFalse(twins.Contains("Ꮡcp", StringComparison.Ordinal), "a ref local cannot be captured");
        StringAssert.Contains(twins, "|bump|cp (captured)", "named, with the reason");
    }

    [TestMethod]
    public void P36_ALoopBodyLocalStaysPlainWithItsReason()
    {
        (string twins, Diagnostic[] errors) = Run(null, StackLocals);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        Assert.IsFalse(twins.Contains("Ꮡlp", StringComparison.Ordinal), "first cut: no box per iteration");
        StringAssert.Contains(twins, "|bump|lp (loop)", "named, with the reason");
    }

    [TestMethod]
    public void P37_ADeconstructedLocalStaysPlainWithItsReason()
    {
        (string twins, Diagnostic[] errors) = Run(null, StackLocals);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        Assert.IsFalse(twins.Contains("Ꮡdc", StringComparison.Ordinal), "not a single local declaration");
        StringAssert.Contains(twins, "|bump|dc (declaration)", "named, with the reason");
    }

    [TestMethod]
    public void P39_ARefusedReceiverLocalNamesItsReasonToo()
    {
        (string twins, Diagnostic[] errors) = Run(null, StackLocals);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        Assert.IsFalse(twins.Contains("Ꮡtr", StringComparison.Ordinal), "a loop-body receiver stays a stack local");
        StringAssert.Contains(twins, "|b|tr (loop)", "the receiver's P row names the reason as an argument's does");
    }

    [TestMethod]
    public void P40_ALocalInADeconstructingForeachIsALoopLocal()
    {
        (string twins, Diagnostic[] errors) = Run(null, StackLocals);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        Assert.IsFalse(twins.Contains("Ꮡld", StringComparison.Ordinal), "a `foreach (var (k, v) in …)` body is a loop body too");
        StringAssert.Contains(twins, "|bump|ld (loop)", "named loop, as the converter's range loops are");
    }

    [TestMethod]
    public void P38_ATakenBoxNameStaysPlainWithItsReason()
    {
        (string twins, Diagnostic[] errors) = Run(null, StackLocals);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        Assert.IsFalse(twins.Contains("heap<nint>(default, out var Ꮡnm)", StringComparison.Ordinal), "Ꮡnm is taken");
        StringAssert.Contains(twins, "|bump|nm (name)", "named, with the reason");
    }

    [TestMethod]
    public void P41_APromotedLocalWhoseInitializerParksAwaitsItInTheHeapDeclaration()
    {
        (string twins, Diagnostic[] errors) = Run(null, StackLocals);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        StringAssert.Contains(twins, "ref var pi = ref global::go.builtin.heap<nint>((await parksIꞏtwin()), out var Ꮡpi);", "the parking initializer awaits inside the heap declaration, before the ref exists");
        StringAssert.Contains(twins, "await bumpꞏtwin(Ꮡpi)", "the call hands the box");
        StringAssert.Contains(twins, "pi = ref Ꮡpi.ValueSlot;", "Rule R derives it again after the await");
    }

    [TestMethod]
    public void P42_AShadowedLocalStaysPlainWithItsReason()
    {
        (string twins, Diagnostic[] errors) = Run(null, StackLocals);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        Assert.IsFalse(twins.Contains("Ꮡsh", StringComparison.Ordinal), "two locals named sh in sibling scopes: neither becomes a heap local");
        StringAssert.Contains(twins, "|bump|sh (shadow)", "named, with the reason");
    }

    [TestMethod]
    public void AnElementUnderAPackageVariableStaysPlainAndNamed()
    {
        (string twins, Diagnostic[] errors) = Run(null, PackageVarElement);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        Assert.IsFalse(twins.Contains("Ꮡ(pv.items, i)", StringComparison.Ordinal), "no element pointer under a package variable");
        StringAssert.Contains(twins, "|sPark|pv.items[i]", "the plain call is named");
    }

    [TestMethod]
    public void AFunctionThatParksOnlyThroughAnUnboxedCallIsStillNamed()
    {
        (string twins, Diagnostic[] errors) = Run(null, OnlyUnboxed);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        TwinSurfaceTests.AssertForwarderOnly(twins, "onlyF", "onlyF parks nowhere");
        StringAssert.Contains(twins, "|sPark|v", "its plain call to a box twin is named all the same (SN-7)");
    }

    [TestMethod]
    public void TheSwitchOffKeepsEveryByRefReceiverRefused()
    {
        (string twins, Diagnostic[] errors) = Run(new() { ["build_property.GoTwinsBoxRecv"] = "false" }, OwnAndLocal);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        Assert.IsFalse(twins.Contains("bꞏtwin(", StringComparison.Ordinal), "no box twin with the switch off");
        StringAssert.Contains(twins, "|b|byref-recv", "b is refused as before");
    }

    [TestMethod]
    public void TheCopyControlCopiesTheReceiver()
    {
        (string twins, Diagnostic[] errors) = Run(new() { ["build_property.GoTwinsBoxCopy"] = "true" }, OwnAndLocal);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        StringAssert.Contains(twins, "var ᴛcopyt = Ꮡt.DerefOrNull(); ref var t = ref ᴛcopyt;", "the control copies, on purpose");
    }

    internal static IEnumerable<MetadataReference> References()
    {
        string trusted = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!;

        return trusted.Split(Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path))
            .Append(MetadataReference.CreateFromFile(typeof(golib::go.IGoEquatable).Assembly.Location));
    }

    internal sealed class BuildProperties(Dictionary<string, string> properties) : Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptionsProvider
    {
        private readonly Options m_global = new(properties);

        public override Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptions GlobalOptions => m_global;

        public override Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptions GetOptions(SyntaxTree tree) => m_global;

        public override Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptions GetOptions(AdditionalText textFile) => m_global;

        private sealed class Options(Dictionary<string, string> values) : Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptions
        {
            public override bool TryGetValue(string key, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out string? value) => values.TryGetValue(key, out value);
        }
    }

    // The glyph seat's shape (C1, train 4): the body is `M⁔`, a plain function over `ref T`, and the method is an
    // expression-bodied `this ref T` forwarder to it. The forwarder must get a box twin, or a twinned caller holding the box
    // calls it plain and everything under it runs plain (DATA-mn-closure-cure.md: the closure's parks fell 36 -> 0).
    private const string GlyphForwarder = """
            internal static void g⁔(ref T t) { ᐸꟷ(ch); t.x = 3; }

            internal static void g(this ref T t) => g⁔(ref t);

            internal static nint h⁔(ref T t, nint k) { ᐸꟷ(ch); return t.x + k; }

            internal static nint h(this ref T t, nint k) => h⁔(ref t, k);

            internal static (nint, bool) q⁔(ref T t) { ᐸꟷ(ch); return (t.x, true); }

            internal static (nint, bool) q(this ref T t) => box_package.q⁔(ref t);

            public static nint runGlyph(ж<T> Ꮡx)
            {
                ref var x = ref Ꮡx.DerefOrNull();
                x.g();
                var (qa, _) = x.q();
                return x.h(1) + qa;
            }

        """;

    [TestMethod]
    public void AGlyphForwarderOverARefReceiverGetsABoxTwinAndItsCallerAwaitsIt()
    {
        (string twins, Diagnostic[] errors) = Run(null, GlyphForwarder);

        Assert.AreEqual(0, errors.Length, Errors(errors));
        StringAssert.Contains(twins, "gꞏtwin(this global::go.ж<T> Ꮡt)", "the void forwarder's twin takes the box");
        StringAssert.Contains(twins, "hꞏtwin(this global::go.ж<T> Ꮡt, nint k)", "the valued forwarder's twin takes the box");
        StringAssert.Contains(twins, "await Ꮡx.gꞏtwin()", "the caller hands the void forwarder its box");
        StringAssert.Contains(twins, "await Ꮡx.hꞏtwin(1)", "the caller hands the valued forwarder its box");
        StringAssert.Contains(twins, "await g⁔ꞏtwin(Ꮡt);", "the void forwarder's twin awaits the body's twin with its box");
        StringAssert.Contains(twins, "return await h⁔ꞏtwin(Ꮡt, k);", "the valued forwarder's twin returns the body's twin, with its box");
        StringAssert.Contains(twins, "qꞏtwin(this global::go.ж<T> Ꮡt)", "a tuple-returning forwarder's twin takes the box");
        StringAssert.Contains(twins, "await box_package.q⁔ꞏtwin(Ꮡt)", "the qualified spelling `pkg.M⁔(ref t)` reaches the box twin");
        StringAssert.Contains(twins, "await Ꮡx.qꞏtwin()", "the caller hands the tuple forwarder its box");
    }
}
