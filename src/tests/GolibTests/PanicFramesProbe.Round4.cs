using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using go.golib;
using static go.builtin;
using static go.runtime_package;

// The shapes COORD's check of round 3 named (ledger 02:24), each with Go's go1.24.13 answer. The principle
// they guard: every MODELLED splice keys on an EMITTER-provided marker; anything unmarked is refused.
namespace go;

internal static partial class panicframesprobe_package
{
    internal struct OuterV
    {
        public T t;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static nint IM(OuterV o) => o.t.M();

    // A PROMOTED value method off the outer pointer, in the emission shape that carries no marker.
    // Go for (*OuterV).IM(nil): gopanic | panicmem | sigpanic | (*OuterV).IM | owner (no panicwrap: cmd/compile
    // emits it only for a direct method of the element type). Unmarked: NO splice.
    internal static readonly Func<ж<OuterV>, nint> promotedWrapperUnmarked =
        ((Func<ж<OuterV>, nint>)([GoWrapper("(*OuterV).IM")] (p0) => IM(p0.Value)));

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static List<string> nilThroughPromotedWrapper()
    {
        List<string> got = [];
        GoFrame ᒐ = default;
        try {
            defer(() => { recover(); got = callersHere(); }, ref ᒐ);
            got = [promotedWrapperUnmarked(null!).ToString()];
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
        return got;
    }

    internal struct F
    {
        public ж<nint> p;
    }

    // A small value method the JIT inlines into the wrapper, faulting on a NON-nil *F. Go: gopanic | panicmem |
    // sigpanic | F.Deref | owner (the wrapper elided: its callee is not the panic machinery).
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static nint Deref(F f) => f.p.Value;

    internal static readonly Func<ж<F>, nint> derefWrapper =
        ((Func<ж<F>, nint>)([GoWrapper("(*F).Deref")] [MethodImpl(MethodImplOptions.AggressiveOptimization)] (p0) => Deref(p0.Value)));

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static List<string> nonNilFaultingCalleeThroughWrapper()
    {
        List<string> got = [];
        GoFrame ᒐ = default;
        try {
            defer(() => { recover(); got = callersHere(); }, ref ᒐ);
            got = [derefWrapper(new StandardBox<F>(nil)).ToString()];
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
        return got;
    }

    // An intrinsified atomic on a nil address. Go (intrinsic): gopanic | panicmem | sigpanic | owner, but a
    // func-value call keeps the atomic's frame: NO splice.
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static List<string> atomicOnNil()
    {
        List<string> got = [];
        GoFrame ᒐ = default;
        try {
            defer(() => { recover(); got = callersHere(); }, ref ᒐ);
            got = [sync.atomic_package.AddInt32(null!, 1).ToString()];
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
        return got;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void seqPlain(Func<nint, bool> yield) => yield(1);

    // A panic in a range-over-func loop BODY, recovered by the ranger's own deferred call. Go:
    // rangerL.func1 | gopanic | rangerL-range1 | seqPlain | rangerL. The body's closure and seq are not
    // modelled: NO splice.
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static List<string> rangerL()
    {
        List<string> got = [];
        GoFrame ᒐ = default;
        try {
            defer(() => { recover(); got = callersHere(); }, ref ᒐ);
            foreach (nint _ in range<nint>(seqPlain))
                throw panic("body");
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
        return got;
    }

    // The ADAPTER shape: a nil receiver box dereferenced inside a go2cs-gen interface adapter (spelled by
    // hand, as the generator emits it). Refused: Go's answer depends on devirtualization.
    internal interface IAdapted
    {
        nint M();
    }

    internal sealed class adapterOverNilBox(ж<T> box) : IAdapted, IGoAdapter
    {
        private readonly ж<T> m_box = box;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public nint M() => m_box.Value.M();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static List<string> nilThroughAdapter()
    {
        List<string> got = [];
        GoFrame ᒐ = default;
        try {
            defer(() => { recover(); got = callersHere(); }, ref ᒐ);
            IAdapted i = new adapterOverNilBox(null!);
            got = [i.M().ToString()];
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
        return got;
    }
}
