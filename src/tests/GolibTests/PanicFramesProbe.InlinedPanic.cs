using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using go.golib;
using static go.builtin;

// The shapes of COORD's frame-hint ruling (2026-10-22): an explicit panic raised inside a small Go function that the
// JIT inlined into its caller. Go records inlined frames, so its traceback always shows the function the panic was
// raised in. The helpers are AggressiveInlining so the optimized JIT (Release, TieredCompilation=0) inlines them
// every time: left to its heuristics it inlined math/bits.Div32 but not these (measured: each read as a real frame),
// and an arm whose shape the JIT may or may not produce proves nothing. Tier-0 inlines nothing, attribute or not, and
// shows their frames as real ones, which is Go's answer.
namespace go;

internal static partial class panicframesprobe_package
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static nint inlinedInner(nint x)
    {
        if (x > 0) {
            throw panic("inner");
        }
        return x;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static nint inlinedOuter(nint x) => inlinedInner(x) + 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static nint inlinedQuiet(nint x)
    {
        if (x > 100) {
            throw panic("quiet");
        }
        return x;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static nint inlinedLoud(nint x)
    {
        if (x > 0) {
            throw panic("loud");
        }
        return x;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static nint inlinedTwoSites(nint x)
    {
        if (x > 100) {
            throw panic("first");
        }
        if (x > 0) {
            throw panic("second");
        }
        return x;
    }

    // Two inlined levels. Go: gopanic | inlinedInner | inlinedOuter | owner.
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static List<string> panicTwoLevels(nint x)
    {
        List<string> got = [];
        GoFrame ᒐ = default;
        try {
            defer(() => { recover(); got = callersHere(); }, ref ᒐ);
            var r = inlinedOuter(x);
            got = [r.ToString()];
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
        return got;
    }

    // CONTROL: the statement panics itself; its callee could panic but returns. Go: gopanic | owner.
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static List<string> panicInOwnStatement(nint x)
    {
        List<string> got = [];
        GoFrame ᒐ = default;
        try {
            defer(() => { recover(); got = callersHere(); }, ref ᒐ);
            throw panic(inlinedQuiet(x));
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
        return got;
    }

    // CONTROL: two candidate calls in one statement; the second panics. Go: gopanic | inlinedLoud | owner.
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static List<string> panicTwoCandidates(nint x)
    {
        List<string> got = [];
        GoFrame ᒐ = default;
        try {
            defer(() => { recover(); got = callersHere(); }, ref ᒐ);
            var r = inlinedQuiet(x) + inlinedLoud(x);
            got = [r.ToString()];
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
        return got;
    }

    // CONTROL: one candidate with two panic sites. Go: gopanic | inlinedTwoSites | owner.
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static List<string> panicTwoSites(nint x)
    {
        List<string> got = [];
        GoFrame ᒐ = default;
        try {
            defer(() => { recover(); got = callersHere(); }, ref ᒐ);
            var r = inlinedTwoSites(x);
            got = [r.ToString()];
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
        return got;
    }

    // The crash renderer's side: runtime.Stack inside the deferred call, before recover, renders the panic site from
    // PanicTrace. Go prints math/bits.Div32 above the owner.
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static string stackInDiv32Panic(uint hi)
    {
        string stack = "";
        GoFrame ᒐ = default;
        try {
            defer(() => {
                byte[] storage = new byte[64 * 1024];
                nint n = runtime_package.Stack(new slice<byte>(storage), false);
                stack = System.Text.Encoding.UTF8.GetString(storage, 0, (int)n);
                recover();
            }, ref ᒐ);
            var (q, _) = math.bits_package.Div32(hi, 0, 1);
            stack = q.ToString();
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
        return stack;
    }
}
