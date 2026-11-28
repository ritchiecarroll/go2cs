// syscall_windows_callback_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// The managed `syscall.NewCallback` seam. Design of record:
// docs/phase4/DESIGN-managed-newcallback.md (§§1-15).
//
// ⚠ THIS FILE IS UNCOMPILED BY ITS AUTHOR. C1 has no C# toolchain of any kind; the i7 builds it.
// The constructs most likely to need a compile fix are named at the bottom of this header.
//
// WHY A MANAGED BODY AND NOT THE LINKNAME PUSH (§1, ruled). Go linknames runtime's
// `compileCallback(eface, bool)` onto syscall's `compileCallback(any, bool)` because `any`'s runtime
// representation IS `eface`. Ours are two different C# types, and bridging them would connect a
// signature to a body that cannot run: `efaceOf` returns an INERT NIL eface (runtime2.cs:141, its own
// comment records why), so runtime's own body panics on its first check -- including at its one
// internal caller, os_windows.cs:314 -- and even given a descriptor it resolves a code address into
// `callbackasm`, which is a bodyless partial on every target. So the push was DECLINED and this is
// the remedy.
//
// THE MECHANISM IS GO'S OWN (§15, ruled after §14's `Expression.Convert` was declined -- there is no
// user-defined operator to resolve for a struct parameter, so a conversion cannot be the bridge).
// Go's `compileCallback` builds an `abiDesc` via `assignArg` and the callback path COPIES BYTES; it
// performs no conversion anywhere, which is exactly why Go's contract can be "uintptr-sized or
// smaller" and nothing more. Here: each native word is REINTERPRETED as the parameter type over its
// own low `sizeof(T)` bytes, and the result is reinterpreted back into a zero-extended word.
//
// Little-endian is assumed and stated: "the low sizeof(T) bytes" is a little-endian reading, and every
// Windows target this row reaches (x64, arm64) is little-endian. The rule needs re-deriving on a
// big-endian target, which does not exist here.
//
// THE SHIM'S OWN DELEGATE TYPE IS NON-GENERIC, one per arity, `nuint` per argument (§8, MEASURED on
// the i7): `Marshal.GetFunctionPointerForDelegate` REFUSES a generic delegate type, and the converter
// emits a Go func value as exactly that. The generic part is the BINDER -- one class per arity,
// constructed once per func value over the target's parameter and return types.
//
// THE TABLE IS NOT AN OPTIMISATION (§13.1, measured). Pointer identity is per delegate INSTANCE, so
// without a table two `NewCallback` calls on one func value would yield two DIFFERENT pointers --
// neither Go's behaviour (`cbs.index` returns the same `callbackasmAddr(n)`) nor a safe divergence.
// The table is simultaneously the identity cache, the ROOTING that keeps the shim and the pointer
// valid for the process lifetime (Go never frees callbacks), and the amortisation of the bind.
//
// ⚠ STATED DIVERGENCE (§5, ruled ACCEPTED by COORD). Go keys its cache on the funcval POINTER;
// this keys on C# delegate equality (method + target). For a closure the two agree. For a STATIC
// METHOD GROUP C# is more aggressive: two separately-created delegates over one static method compare
// equal, so we return the SAME pointer where Go might mint two. That is the safe direction -- the
// pointer works and a native caller cannot tell -- and a test asserting two DISTINCT pointers for one
// static function would be asserting Go's allocator rather than its contract. If one of the eleven
// consumers does, it is a disclosure by signature with this paragraph as its reason.
//
// SCOPE. Arities 0, 1, 2 and 4 are implemented: §12 measured those as everything the consumers reach
// without gcc, since the 2..10 range lives entirely inside `TestStdcallAndCDeclCallbacks`, which
// SKIPS when gcc is missing. The remaining arities are a bounded, enumerated follow-on (11 total),
// and an unimplemented arity refuses BY NAME rather than silently.
//
// NOT COVERED, deliberately, each refused with Go's own text: float arguments, float results,
// oversized arguments, a non-function argument, and a wrong result count. `NewCallbackCDecl` shares
// this path because `compileCallback` forces `cdecl = false` off 386 and 386 is not a target, so on
// every target here both entry points are the one Windows ABI.
//
// LIKELY COMPILE-FIX POINTS, named so the first build is cheap: the `MakeGenericType` construction
// and its `Bind` lookup; whether `Delegate.CreateDelegate` accepts each converted delegate shape;
// the `uintptr`/`nuint` conversions at the boundary; and whether `Unsafe.ReadUnaligned` needs an
// `unsafe` context here (the sibling companions declare `[module: go.GoRequiresUnsafe]`; this file
// does not, because it takes no raw pointers -- if the build disagrees, add it).

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// NO `using any = System.Object;` HERE. The generated csproj already carries that alias
// GLOBALLY, so declaring it again is CS1537 -- measured by the i7 on 193af90f5. The partial
// below therefore spells the parameter `any` and binds it from the project-wide alias.

[module: go.GoManualConversion]

namespace go;

partial class syscall_package
{
    // ---- the reinterpret, which is the whole mechanism ----

    // A native word -> a parameter of type T, over the word's OWN low sizeof(T) bytes. No conversion
    // is asked of the type system, which is why a struct parameter works here and could not work
    // through a cast.
    private static T goCallbackFromWord<T>(nuint word) =>
        Unsafe.ReadUnaligned<T>(ref Unsafe.As<nuint, byte>(ref word));

    // The result, zero-extended back into a word: Go's result is uintptr-sized or smaller, and the
    // zeroed local is what supplies the extension.
    private static nuint goCallbackToWord<T>(T value)
    {
        nuint word = 0;
        Unsafe.WriteUnaligned(ref Unsafe.As<nuint, byte>(ref word), value);
        return word;
    }

    // Go's contract, enforced where the type is a real type parameter rather than a reflected Type:
    // an argument is uintptr-sized or SMALLER, a result is uintptr-sized. Both refuse with Go's text.
    private static void goCallbackCheckArg<T>()
    {
        if (Unsafe.SizeOf<T>() > Unsafe.SizeOf<nuint>())
            throw panic("compileCallback: argument size is larger than uintptr");
    }

    private static void goCallbackCheckResult<T>()
    {
        if (Unsafe.SizeOf<T>() != Unsafe.SizeOf<nuint>())
            throw panic("compileCallback: expected function with one uintptr-sized result");
    }

    // ---- the shim delegate types: NON-GENERIC, one per arity, a native word per argument ----

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate nuint GoCallbackShim0();

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate nuint GoCallbackShim1(nuint a1);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate nuint GoCallbackShim2(nuint a1, nuint a2);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate nuint GoCallbackShim4(nuint a1, nuint a2, nuint a3, nuint a4);

    // ---- the binders: GENERIC per arity, constructed once per func value ----
    //
    // `Bind` takes the func already TYPED as a `Func<…>` and returns a non-generic, marshallable shim
    // whose lambda does the reinterprets. Two ways reach it: a shape go2cs-gen registered from the call
    // site (`GoNewCallbackShapes`, below), whose typed adapter is a cast or a forwarding lambda and whose
    // closed binder Native AOT compiles; and, where dynamic code is supported, `BindReflective`, which
    // retargets whatever converted delegate shape the caller passed with `Delegate.CreateDelegate`
    // (docs/phase4/DESIGN-newcallback-native-aot.md).
    //
    // Each shim crosses back into Go as cgocallbackg does, through the runtime's callback pair
    // (syscallCallbackEnter / syscallCallbackExit; DESIGN-native-call-seam §6.3, F-3): the Go function runs
    // _Grunning, one transition level inside the native call that called it back. On a thread with no goroutine
    // identity it runs as a goroutine attached for the callback's span, as Go's needm/dropm do (S-c2).

    private static class GoCallbackBinder0<TR>
    {
        public static Delegate BindReflective(Delegate d) =>
            Bind((Func<TR>)Delegate.CreateDelegate(typeof(Func<TR>), d.Target, d.Method));

        public static Delegate Bind(Func<TR> f)
        {
            goCallbackCheckResult<TR>();
            return new GoCallbackShim0(() =>
            {
                runtime_package.GoCallbackCrossing entered = runtime_package.syscallCallbackEnter();

                try { return goCallbackToWord(f()); }
                finally { runtime_package.syscallCallbackExit(entered); }
            });
        }
    }

    private static class GoCallbackBinder1<T1, TR>
    {
        public static Delegate BindReflective(Delegate d) =>
            Bind((Func<T1, TR>)Delegate.CreateDelegate(typeof(Func<T1, TR>), d.Target, d.Method));

        public static Delegate Bind(Func<T1, TR> f)
        {
            goCallbackCheckArg<T1>();
            goCallbackCheckResult<TR>();
            return new GoCallbackShim1(a1 =>
            {
                runtime_package.GoCallbackCrossing entered = runtime_package.syscallCallbackEnter();

                try { return goCallbackToWord(f(goCallbackFromWord<T1>(a1))); }
                finally { runtime_package.syscallCallbackExit(entered); }
            });
        }
    }

    private static class GoCallbackBinder2<T1, T2, TR>
    {
        public static Delegate BindReflective(Delegate d) =>
            Bind((Func<T1, T2, TR>)Delegate.CreateDelegate(typeof(Func<T1, T2, TR>), d.Target, d.Method));

        public static Delegate Bind(Func<T1, T2, TR> f)
        {
            goCallbackCheckArg<T1>(); goCallbackCheckArg<T2>();
            goCallbackCheckResult<TR>();
            return new GoCallbackShim2((a1, a2) =>
            {
                runtime_package.GoCallbackCrossing entered = runtime_package.syscallCallbackEnter();

                try { return goCallbackToWord(f(goCallbackFromWord<T1>(a1), goCallbackFromWord<T2>(a2))); }
                finally { runtime_package.syscallCallbackExit(entered); }
            });
        }
    }

    private static class GoCallbackBinder4<T1, T2, T3, T4, TR>
    {
        public static Delegate BindReflective(Delegate d) =>
            Bind((Func<T1, T2, T3, T4, TR>)Delegate.CreateDelegate(
                typeof(Func<T1, T2, T3, T4, TR>), d.Target, d.Method));

        public static Delegate Bind(Func<T1, T2, T3, T4, TR> f)
        {
            goCallbackCheckArg<T1>(); goCallbackCheckArg<T2>();
            goCallbackCheckArg<T3>(); goCallbackCheckArg<T4>();
            goCallbackCheckResult<TR>();
            return new GoCallbackShim4((a1, a2, a3, a4) =>
            {
                runtime_package.GoCallbackCrossing entered = runtime_package.syscallCallbackEnter();

                try
                {
                    return goCallbackToWord(f(goCallbackFromWord<T1>(a1), goCallbackFromWord<T2>(a2),
                                              goCallbackFromWord<T3>(a3), goCallbackFromWord<T4>(a4)));
                }
                finally { runtime_package.syscallCallbackExit(entered); }
            });
        }
    }

    // ---- the shapes: registered from the call site by go2cs-gen, looked up before any reflection ----
    //
    // Native AOT compiles no code for a generic instantiation it cannot see at build time, so the binder
    // for a func type is closed where that type IS seen: go2cs-gen emits, in each compilation that calls
    // NewCallback (or a forwarder, below), a module initializer registering every delegate type passed
    // there. The entry is keyed by the func's runtime delegate type; `Check` runs Go's kind checks in the
    // reflective path's order (float result, float arguments) and `Bind` builds the shim through the
    // closed binder. A refusal entry, for a shape no binder takes, has only `Check`.
    internal sealed record GoCallbackShape(Action Check, Func<Delegate, Delegate>? Bind);

    /// <summary>
    /// The callback shapes go2cs-gen registers from the call site. For go2cs-gen's module initializers only.
    /// </summary>
    /// <remarks>
    /// A NESTED class with its own table and lock, never members of <c>syscall_package</c> itself: that class has
    /// an explicit static constructor (the package's Go initialization), so any call into it from a consumer's
    /// module initializer would run syscall's package init at that consumer's module load, ahead of the order
    /// Go's initialization gives it. Registration touches only this class; the table is read under
    /// <c>s_goCallbackLock</c>, taken first, so the two locks are always taken in one order.
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class GoNewCallbackShapes
    {
        private static readonly Dictionary<Type, GoCallbackShape> s_shapes = new();
        private static readonly object s_lock = new();

        // The first entry for a type stays: two assemblies registering one Func type is normal.
        private static void add(Type funcType, GoCallbackShape shape)
        {
            lock (s_lock)
                s_shapes.TryAdd(funcType, shape);
        }

        internal static GoCallbackShape? Find(Type funcType)
        {
            lock (s_lock)
                return s_shapes.GetValueOrDefault(funcType);
        }

        /// <summary>Registers a no-argument callback shape.</summary>
        public static void Register<TR>(Type funcType, Func<Delegate, Func<TR>> typed) =>
            add(funcType, new GoCallbackShape(
                static () => goCallbackCheckFloatResult<TR>(),
                d => GoCallbackBinder0<TR>.Bind(typed(d))));

        /// <summary>Registers a one-argument callback shape.</summary>
        public static void Register<T1, TR>(Type funcType, Func<Delegate, Func<T1, TR>> typed) =>
            add(funcType, new GoCallbackShape(
                static () => { goCallbackCheckFloatResult<TR>(); goCallbackCheckFloatArg<T1>(); },
                d => GoCallbackBinder1<T1, TR>.Bind(typed(d))));

        /// <summary>Registers a two-argument callback shape.</summary>
        public static void Register<T1, T2, TR>(Type funcType, Func<Delegate, Func<T1, T2, TR>> typed) =>
            add(funcType, new GoCallbackShape(
                static () =>
                {
                    goCallbackCheckFloatResult<TR>();
                    goCallbackCheckFloatArg<T1>(); goCallbackCheckFloatArg<T2>();
                },
                d => GoCallbackBinder2<T1, T2, TR>.Bind(typed(d))));

        /// <summary>Registers a four-argument callback shape.</summary>
        public static void Register<T1, T2, T3, T4, TR>(Type funcType,
            Func<Delegate, Func<T1, T2, T3, T4, TR>> typed) =>
            add(funcType, new GoCallbackShape(
                static () =>
                {
                    goCallbackCheckFloatResult<TR>();
                    goCallbackCheckFloatArg<T1>(); goCallbackCheckFloatArg<T2>();
                    goCallbackCheckFloatArg<T3>(); goCallbackCheckFloatArg<T4>();
                },
                d => GoCallbackBinder4<T1, T2, T3, T4, TR>.Bind(typed(d))));

        /// <summary>
        /// Registers a callback shape no binder takes (no result, or an arity other than 0, 1, 2 and 4), with the
        /// facts go2cs-gen read from it.
        /// </summary>
        public static void Refusal(Type funcType, int arity, bool hasResult, bool floatResult, bool floatArgument) =>
            add(funcType, new GoCallbackShape(
                () => goCallbackRefuse(arity, hasResult, floatResult, floatArgument), null));
    }

    private static void goCallbackCheckFloatResult<T>()
    {
        if (typeof(T) == typeof(float) || typeof(T) == typeof(double))
            throw panic("compileCallback: float results not supported");
    }

    private static void goCallbackCheckFloatArg<T>()
    {
        if (typeof(T) == typeof(float) || typeof(T) == typeof(double))
            throw panic("compileCallback: float arguments not supported");
    }

    // Go's refusals in the reflective path's order, from facts rather than from a reflected Type, so a
    // registered refusal and the reflective path read the same text.
    private static void goCallbackRefuse(int arity, bool hasResult, bool floatResult, bool floatArgument)
    {
        if (!hasResult)
            throw panic("compileCallback: expected function with one uintptr-sized result");

        if (floatResult)
            throw panic("compileCallback: float results not supported");

        if (floatArgument)
            throw panic("compileCallback: float arguments not supported");

        if (arity is not (0 or 1 or 2 or 4))
            throw goCallbackArityRefusal(arity);
    }

    // §12: arities 0/1/2/4 are everything the consumers reach without gcc; the 2..10 range is a bounded,
    // enumerated follow-on. This refusal deliberately does NOT borrow Go's "type … is currently not
    // supported" text: Go has no unsupported-arity case at all (any arity fitting the frame works there),
    // so this is OUR limitation and it says so rather than dressing itself as Go's contract.
    private static PanicException goCallbackArityRefusal(int arity) =>
        panic("compileCallback: " + arity +
              "-argument callbacks are not implemented in this port; arities 0, 1, 2 and 4 are " +
              "(docs/phase4/DESIGN-managed-newcallback.md §12)");

    /// <summary>
    /// Marks a method that passes its own <c>any</c> parameter straight to <c>NewCallback</c> or
    /// <c>NewCallbackCDecl</c>, so go2cs-gen treats its calls, in the assemblies that reference this one, as
    /// <c>NewCallback</c> sites. go2cs-gen emits it; it is not written by hand.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class GoNewCallbackForwarderAttribute(Type holder, string method, int parameter) : Attribute
    {
        public Type Holder { get; } = holder;

        public string Method { get; } = method;

        public int Parameter { get; } = parameter;
    }

    // ---- the table: identity, rooting and amortisation in one object (§13.1) ----
    //
    // It holds the SHIM (which roots the Go delegate it closes over) and the pointer. Nothing is ever
    // removed: Go's callbacks are process-lifetime and its own exhaustion path is a fatal
    // `throw("too many callback functions")`, not a reclaim.
    private static readonly Dictionary<Delegate, (Delegate Shim, uintptr Code)> s_goCallbacks = new();
    private static readonly object s_goCallbackLock = new();

    // Go's own ceiling, so exhaustion is fatal with Go's text rather than unbounded growth.
    // VERIFIED at the pinned source rather than recalled: `const cb_max = 2000` in
    // runtime/zcallback_windows.go:5, indexing `cbs.ctxt [cb_max]winCallback`. The number is Go's.
    private const int goCallbackMax = 2000;

    /// <summary>
    /// The managed <c>syscall.compileCallback</c>. Completes the bodyless partial at
    /// <c>syscall_windows.cs:223</c>; see this file's header and
    /// <c>docs/phase4/DESIGN-managed-newcallback.md</c>.
    /// </summary>
    internal static partial uintptr compileCallback(any fn, bool cleanstack)
    {
        // `cleanstack` (Go's `cdecl`) is deliberately unused: Go's own compileCallback forces it false
        // off 386 -- "cdecl is only meaningful on 386" -- and 386 is not a target here, so both entry
        // points are the one Windows ABI and share one table entry per func value, exactly as Go's
        // winCallbackKey{fn, cdecl} collapses when cdecl is always false.
        _ = cleanstack;

        if (fn is not Delegate d)
            throw panic("compileCallback: expected function with one uintptr-sized result");

        lock (s_goCallbackLock)
        {
            if (s_goCallbacks.TryGetValue(d, out var existing))
                return existing.Code;

            Delegate shim;

            // A shape go2cs-gen registered binds through its closed binder, before `d.Method` (itself
            // reflection), `MakeGenericType` or `Invoke` is reached.
            if (GoNewCallbackShapes.Find(d.GetType()) is { } shape)
            {
                shape.Check();

                if (s_goCallbacks.Count >= goCallbackMax)
                    throw panic("too many callback functions");

                shim = shape.Bind!(d);
            }
            else if (!RuntimeFeature.IsDynamicCodeSupported)
            {
                // Native AOT (or an application that switched dynamic code off): no closed binder exists
                // for an unregistered type, so refuse by name before anything is built. A go2cs limit,
                // said as one, never Go's text.
                throw panic("compileCallback: no native code for callback type " + d.GetType() +
                            " under Native AOT; the project that calls NewCallback must reference go2cs-gen " +
                            "and pass a value of that func type");
            }
            else
            {
                shim = goCallbackBindReflective(d);
            }

            uintptr code = (nuint)(nint)Marshal.GetFunctionPointerForDelegate(shim);
            s_goCallbacks[d] = (shim, code);
            return code;
        }
    }

    // The reflective path, where dynamic code is supported: hand-written C#, or a func reached only
    // through `any`, whose type no call site showed go2cs-gen. Called under s_goCallbackLock.
    private static Delegate goCallbackBindReflective(Delegate d)
    {
        MethodInfo mi = d.Method;
        ParameterInfo[] ps = mi.GetParameters();
        Type ret = mi.ReturnType;

        if (ret == typeof(void))
            throw panic("compileCallback: expected function with one uintptr-sized result");

        if (goCallbackIsFloat(ret))
            throw panic("compileCallback: float results not supported");

        foreach (ParameterInfo p in ps)
        {
            if (goCallbackIsFloat(p.ParameterType))
                throw panic("compileCallback: float arguments not supported");
        }

        Type[] targs;
        Type open;
        switch (ps.Length)
        {
            case 0: open = typeof(GoCallbackBinder0<>);       targs = [ret]; break;
            case 1: open = typeof(GoCallbackBinder1<,>);      targs = [ps[0].ParameterType, ret]; break;
            case 2: open = typeof(GoCallbackBinder2<,,>);     targs = [ps[0].ParameterType, ps[1].ParameterType, ret]; break;
            case 4: open = typeof(GoCallbackBinder4<,,,,>);   targs = [ps[0].ParameterType, ps[1].ParameterType,
                                                                       ps[2].ParameterType, ps[3].ParameterType, ret]; break;
            default:
                throw goCallbackArityRefusal(ps.Length);
        }

        if (s_goCallbacks.Count >= goCallbackMax)
            throw panic("too many callback functions");

        // ⚠ DoNotWrapExceptions IS LOAD-BEARING, not tidiness. The binder's own refusals --
        // goCallbackCheckArg / goCallbackCheckResult, which raise Go's text as a panic -- run
        // INSIDE this reflective call, and MethodInfo.Invoke wraps anything they throw in a
        // TargetInvocationException. A wrapped panic is not a panic: `recover()` would not see
        // it and the caller would get an infrastructure error where Go gives a fatal with its
        // own message. Measured by the i7 on 193af90f5 as a latent defect behind refusals d2/d3.
        return (Delegate)open.MakeGenericType(targs)
            .GetMethod("BindReflective", BindingFlags.Public | BindingFlags.Static)!
            .Invoke(null, BindingFlags.DoNotWrapExceptions, null, [d], null)!;
    }

    // Go refuses float arguments and float results by KIND. The converted float types alias the CLR's,
    // so identity against them is the same test.
    private static bool goCallbackIsFloat(Type t) => t == typeof(float) || t == typeof(double);
}
