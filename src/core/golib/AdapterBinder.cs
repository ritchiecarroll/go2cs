// AdapterBinder.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable InconsistentNaming

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using GoTypeExtensions = go.golib.TypeExtensions;
using static go2cs.Symbols;

#pragma warning disable IL2055 // MakeGenericType is belted by the object-shell fallback below
#pragma warning disable IL2070
#pragma warning disable IL2072
#pragma warning disable IL2075

namespace go;

/// <summary>
/// Builds a runtime duck-typing implementation of a converted NAMED interface over a Go dynamic
/// value whose (type, interface) pair has no compile-time adapter.
/// </summary>
/// <remarks>
/// <para>
/// <b>Why this exists.</b> Go's interface satisfaction is structural and resolved at run time; C#'s
/// is nominal and resolved at compile time. go2cs closes most of that gap with generated adapters
/// recorded at conversion time, but the recorders are a structurally incomplete approximation: a
/// dynamic type may live in an assembly converted AFTER the interface's own, and then no record can
/// exist by construction. <c>io/fs</c> is converted before <c>os</c>, so <c>fs</c> can never record
/// <c>os.dirFS</c> — every <c>fsys.(ReadDirFS)</c> against a <c>DirFS</c> silently missed.
/// </para>
/// <para>
/// <b>Two tiers.</b> With no dynamic code generation available, the per-interface compile-time shell
/// is the irreducible minimum; go2cs-gen emits two of them per interface (see
/// <see cref="GoInterfaceShellAttribute"/>) and this class picks one:
/// </para>
/// <list type="number">
/// <item>
/// <b>Delegate-bound generic shell</b> for a REFERENCE-typed dynamic value — every receiver box
/// <c>ж&lt;X&gt;</c>, i.e. every pointer-sourced Go interface value, the dominant case. Each interface
/// method is a pre-bound delegate, so a forwarded call is an ordinary delegate invocation
/// (single-digit ns) rather than a reflective one. This is the tier a duck-typed <c>io.Reader</c>
/// inside <c>io.Copy</c>, or a wrapped <c>sort.Interface</c>, pays PER CALL — the whole reason the
/// reflective shape is not used everywhere.
/// </item>
/// <item>
/// <b>Reflective object shell</b> for a VALUE-typed dynamic value (the <c>os.dirFS</c> forcing case —
/// a <c>[GoType("@string")]</c> struct). It holds the value as <c>object</c> and dispatches through a
/// cached <see cref="MethodInvoker"/>, so it needs no generic instantiation at all. Under Native AOT,
/// a <see cref="Type.MakeGenericType"/> driven by <see cref="object.GetType"/> can only succeed for an
/// instantiation the compiler already rooted, and a value-type close reached from a run-time
/// <c>GetType()</c> is never one — the object shell is the tier that is unconditionally available.
/// </item>
/// </list>
/// <para>
/// The kind branch is on <see cref="Type.IsValueType"/>, and EITHER tier belts to the other when
/// construction fails: AOT rooting is source-shape sensitive, so "we never hit that instantiation"
/// cannot be asserted, only constructed. (Note the honest limit: the generic shell closes over the
/// ELEMENT type — the pointee — because that is what makes both receiver forms bindable, and a
/// pointee is usually a struct. So the pointer tier is AOT-GRACEFUL, not AOT-guaranteed; when the
/// instantiation is unavailable it degrades to the object shell rather than to a miss.)
/// </para>
/// <para>
/// <b>Fail-soft.</b> <see cref="TryCreate"/> answers <c>false</c> for anything it cannot build. A MISS
/// is normal control flow at every emitted assertion and type-switch site, and the structural probe
/// that gates this call is deliberately NAME-ONLY for an open-generic receiver method — a Go
/// <c>box[int]</c> whose <c>Get() T</c> returns an <c>int</c> matches <c>interface{ Get() string }</c>
/// under that weaker rule. Go answers <c>ok=false</c> there, so a construction failure must reproduce
/// today's harmless miss, never escape as an exception.
/// </para>
/// <para>
/// <b>Method-set discipline.</b> Binding resolves through
/// <see cref="go.golib.TypeExtensions.GetGoMethodSetCandidates"/>, the same receiver rule
/// <see cref="go.golib.TypeExtensions.StructurallyImplements"/> applies — so a value-sourced shell binds only
/// value-receiver methods and can never widen a Go method set.
/// </para>
/// </remarks>
public static class AdapterBinder
{
    private static readonly ConcurrentDictionary<Type, GoInterfaceShellAttribute?> s_shellSpecs = [];

    private const BindingFlags CtorFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    private const BindingFlags ShellStateFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

    // Static-field names the generated delegate shell exposes so the binder can tell, WITHOUT
    // forwarding a call first, whether every interface method bound for the receiver form it is
    // about to use. Reading either one runs the shell's static binder, so a binding failure surfaces
    // here (as a TypeInitializationException the caller catches) instead of as a
    // NullReferenceException on the first forwarded call. OPTIONAL: a hand-written shell that
    // predates the flags signals the same failure by throwing from its static binder.
    private const string BoundByPtrField = $"{TempVarMarker}BoundByPtr";
    private const string BoundByValField = $"{TempVarMarker}BoundByVal";

    /// <summary>
    /// Attempts to build a runtime duck-typing implementation of <paramref name="interfaceType"/>
    /// over Go dynamic value <paramref name="dynamicValue"/>.
    /// </summary>
    /// <param name="dynamicValue">Go dynamic value — a receiver box <c>ж&lt;X&gt;</c> or a plain value.</param>
    /// <param name="interfaceType">Target converted interface type.</param>
    /// <param name="shell">Constructed implementation, if one could be built.</param>
    /// <returns><c>true</c> when a shell was built; otherwise, <c>false</c> (a MISS).</returns>
    public static bool TryCreate(object dynamicValue, Type interfaceType, [NotNullWhen(true)] out object? shell)
    {
        return TryCreate(dynamicValue, dynamicValue.GetType(), interfaceType, out shell);
    }

    /// <summary>
    /// Attempts to build a runtime duck-typing implementation of <paramref name="interfaceType"/>
    /// over a Go dynamic value whose type is named explicitly.
    /// </summary>
    /// <param name="dynamicValue">Go dynamic value, which may be <c>null</c> for a typed nil func.</param>
    /// <param name="valueType">Go dynamic type of <paramref name="dynamicValue"/>.</param>
    /// <param name="interfaceType">Target converted interface type.</param>
    /// <param name="shell">Constructed implementation, if one could be built.</param>
    /// <returns><c>true</c> when a shell was built; otherwise, <c>false</c> (a MISS).</returns>
    /// <remarks>
    /// A typed nil func (<see cref="NilFuncValue"/>) has a dynamic type and no instance to read it
    /// from: <c>any(Fn(nil))</c> holds type <c>Fn</c> and a nil value, and <c>Fn</c>'s methods run
    /// on that nil receiver. The shell wraps the null delegate for exactly that call.
    /// </remarks>
    public static bool TryCreate(object? dynamicValue, Type valueType, Type interfaceType, [NotNullWhen(true)] out object? shell)
    {
        shell = null;

        if (!AdapterRegistry.TryGetShellFactory(valueType, interfaceType, out Func<object, object>? factory))
        {
            factory = BuildFactory(valueType, interfaceType);

            // Memoized unconditionally, including the negative. Go fixes a type's method set at
            // COMPILE time, so a pair that cannot bind now can never bind later; and the extension
            // methods that carry a type's method set live in that type's own assembly, which is
            // loaded by definition — we are holding an instance of it. Extension-method discovery
            // caches ARE invalidated on AssemblyLoad (see GoTypeExtensions.ClearTypeCaches); this
            // decision cache deliberately is NOT.
            AdapterRegistry.MemoizeShellFactory(valueType, interfaceType, factory);
        }

        if (factory is null)
            return false;

        try
        {
            shell = factory(dynamicValue!);
        }
        catch (Exception ex) when (IsBindingFailure(ex))
        {
            return false;
        }

        return shell is not null;
    }

    /// <summary>
    /// Resolves the extension methods implementing Go method <paramref name="name"/> on element type
    /// <paramref name="element"/>, split by receiver form.
    /// </summary>
    /// <param name="element">Go element type — the pointee for a pointer-sourced value.</param>
    /// <param name="name">Bare Go method name.</param>
    /// <param name="byPtr">The <c>ж&lt;element&gt;</c>-receiver overload, if any.</param>
    /// <param name="byVal">The by-value <c>element</c>-receiver method, if any.</param>
    /// <remarks>
    /// Called from the generated delegate shell's static binder — it is public for that reason. The
    /// <c>[GoRecv] this ref X</c> primary form is skipped: it is a POINTER-receiver method whose
    /// by-ref receiver no delegate or reflective invoker can carry, and the RecvGenerator always
    /// emits its <c>ж&lt;X&gt;</c> twin, which is the form bound here.
    /// <para>
    /// The EXACT emitted name is a full first pass; only when it resolves NEITHER receiver form does
    /// the Go-name projection run, matching the two-pass rule the structural probe applies (see
    /// <see cref="go.golib.TypeExtensions.GoMethodNameMatches"/>). The two must agree — a shell that
    /// bound a method the probe would not have counted is exactly the disagreement
    /// <see cref="go.golib.TypeExtensions.GetGoMethodSetCandidates"/> exists to prevent.
    /// </para>
    /// </remarks>
    public static void ResolveReceiverMethods(Type element, string name, out MethodInfo? byPtr, out MethodInfo? byVal)
    {
        ResolveReceiverMethods(element, name, false, out byPtr, out byVal);

        if (byPtr is null && byVal is null)
            ResolveReceiverMethods(element, name, true, out byPtr, out byVal);
    }

    private static void ResolveReceiverMethods(Type element, string name, bool projectGoName, out MethodInfo? byPtr, out MethodInfo? byVal)
    {
        byPtr = null;
        byVal = null;

        MethodInfo? copyBound = null;

        // The *X method set — value AND pointer receivers. A VALUE-sourced shell narrows this to the
        // by-value half by simply never using byPtr (see BuildObjectShellFactory and the generated
        // delegate shell's dispatch), which is exactly Go's rule.
        foreach (MethodInfo candidate in GoTypeExtensions.GetGoMethodSetCandidates(element, true))
        {
            if (!GoTypeExtensions.GoMethodNameMatches(candidate, name, projectGoName))
                continue;

            Type receiver = candidate.GetParameters()[0].ParameterType;

            if (receiver.IsByRef)
            {
                // `this ref X` marked [GoCopyBound] is a VALUE-set method whose receiver is bound
                // through a copy (GoTypeExtensions.IsCopyBoundReceiver): the probe counts it for a value
                // source, so the shell must bind it, or the two disagree and an assertion Go ACCEPTS
                // misses — bufio.ReadWriter held by value, asserted to io.Reader. Every other by-ref
                // receiver stays skipped: it is a pointer-receiver method, bound through its ж<X> twin.
                if (GoTypeExtensions.IsCopyBoundReceiver(candidate, out _))
                    copyBound ??= candidate;

                continue;
            }

            if (receiver.IsGenericType && receiver.GetGenericTypeDefinition() == typeof(ж<>))
                byPtr ??= candidate;
            else if (candidate.GetCustomAttribute<GoRecvAttribute>() is null)
                byVal ??= candidate;
        }

        // A true by-value method wins; Go has one method per name, so both existing would be the
        // converter's own doing and the by-value form is the one every binder already carries.
        byVal ??= copyBound;
    }

    // Resolves the shell spec for an interface, or null when it carries none (an empty, generic,
    // constraint or non-converted interface — all of which keep today's nominal-only resolution).
    private static GoInterfaceShellAttribute? ShellSpec(Type interfaceType)
    {
        return s_shellSpecs.GetOrAdd(interfaceType, static type => type.GetCustomAttribute<GoInterfaceShellAttribute>());
    }

    private static Func<object, object>? BuildFactory(Type valueType, Type interfaceType)
    {
        GoInterfaceShellAttribute? spec = ShellSpec(interfaceType);

        if (spec is null)
            return null;

        bool isValueType = valueType.IsValueType;

        // Reference-typed (every ж<X> box): the delegate tier, belting to the reflective one.
        // Value-typed: the reflective tier first — it needs no generic instantiation, which is the
        // only form guaranteed available under Native AOT — belting to the delegate tier when the
        // interface has a member the invoker cannot forward and no object shell was emitted.
        return isValueType
            ? BuildObjectShellFactory(valueType, spec) ?? BuildGenericShellFactory(valueType, spec)
            : BuildGenericShellFactory(valueType, spec) ?? BuildObjectShellFactory(valueType, spec);
    }

    [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2059", Justification = GoTypeRegistry.RegisteredGenericJustification)]
    private static Func<object, object>? BuildGenericShellFactory(Type valueType, GoInterfaceShellAttribute spec)
    {
        GoTypeExtensions.ResolveReceiverElement(valueType, out Type element, out bool isPointer);

        try
        {
            Type closed = spec.GenericShell.MakeGenericType(element);
            FieldInfo? boundFlag = closed.GetField(isPointer ? BoundByPtrField : BoundByValField, ShellStateFlags);

            if (boundFlag is null)
            {
                // A HAND-WRITTEN shell (golib's error, the baseline stubs' fmt.Stringer and
                // io.Reader) carries no bound-flags: it signals an unbindable pair the older way, by
                // throwing from its static binder. Force that binder now so the failure is decided —
                // and memoized — HERE, rather than being rediscovered on every construction.
                RuntimeHelpers.RunClassConstructor(closed.TypeHandle);
            }
            else if (boundFlag.GetValue(null) is not true)
            {
                // Reading the flag runs the generated shell's static binder; false means at least one
                // interface method has no receiver method for this form, so it would null-dispatch.
                return null;
            }

            ConstructorInfo? ctor = closed.GetConstructor(CtorFlags, null, [valueType], null);

            if (ctor is null)
                return null;

            // ConstructorInvoker, not ConstructorInfo.Invoke: the memoized factory runs on EVERY
            // assert of the pair, and the reflective invoke dominated the measurement (~90 ns of a
            // ~120 ns memoized assert). Same AOT-safety, no dynamic code.
            ConstructorInvoker invoker = ConstructorInvoker.Create(ctor);

            return value => invoker.Invoke(value);
        }
        catch (Exception ex) when (IsBindingFailure(ex))
        {
            return null;
        }
    }

    private static Func<object, object>? BuildObjectShellFactory(Type valueType, GoInterfaceShellAttribute spec)
    {
        if (spec.ObjectShell is null)
            return null;

        try
        {
            if (!TryBindMethodSet(valueType, spec.MethodNames, out GoShellBinding? binding))
                return null;

            ConstructorInfo? ctor = spec.ObjectShell.GetConstructor(CtorFlags, null, [typeof(object), typeof(GoShellBinding)], null);

            if (ctor is null)
                return null;

            ConstructorInvoker invoker = ConstructorInvoker.Create(ctor);

            return value => invoker.Invoke(value, binding);
        }
        catch (Exception ex) when (IsBindingFailure(ex))
        {
            return null;
        }
    }

    // Resolves one receiver method per interface method, honoring Go's receiver rule: a pointer
    // source prefers its ж-receiver overload and otherwise calls the value-receiver method on the
    // DEREFERENCED pointee; a value source has only the by-value half to draw from.
    private static bool TryBindMethodSet(Type valueType, string[] methodNames, [NotNullWhen(true)] out GoShellBinding? binding)
    {
        GoTypeExtensions.ResolveReceiverElement(valueType, out Type element, out bool isPointer);

        binding = null;

        MethodInvoker[] invokers = new MethodInvoker[methodNames.Length];
        bool[] dereference = new bool[methodNames.Length];

        for (int i = 0; i < methodNames.Length; i++)
        {
            ResolveReceiverMethods(element, methodNames[i], out MethodInfo? byPtr, out MethodInfo? byVal);

            MethodInfo? bound = isPointer ? byPtr ?? byVal : byVal;

            if (bound is null || bound.IsGenericMethodDefinition || bound.ContainsGenericParameters)
                return false;

            invokers[i] = MethodInvoker.Create(bound);
            dereference[i] = isPointer && ReferenceEquals(bound, byVal);
        }

        binding = new GoShellBinding(invokers, dereference, isPointer ? valueType : null);
        return true;
    }

    // Every way a shell can fail to materialize is a MISS: an AOT-unavailable instantiation
    // (NotSupportedException / PlatformNotSupportedException / TypeLoadException), a constraint or
    // argument mismatch, a static binder that threw (TypeInitializationException), a constructor that
    // threw (TargetInvocationException), or an inaccessible member.
    private static bool IsBindingFailure(Exception ex)
    {
        return ex is NotSupportedException
            or PlatformNotSupportedException
            or TypeLoadException
            or ArgumentException
            or InvalidOperationException
            or TypeInitializationException
            or TargetInvocationException
            or MemberAccessException;
    }
}

/// <summary>
/// The per-(dynamic type, interface) method binding a generated reflective object shell forwards
/// through.
/// </summary>
/// <remarks>
/// Resolved once by <see cref="AdapterBinder"/> and shared by every shell instance built for the
/// pair, so the reflective cost is paid at binding time, not per instance. Indices are the positions
/// of <see cref="GoInterfaceShellAttribute.MethodNames"/>, which is the order the generated shell's
/// forwarders were emitted in.
/// </remarks>
public sealed class GoShellBinding
{
    private readonly MethodInvoker[] m_invokers;
    private readonly bool[] m_dereference;
    private readonly PropertyInfo? m_pointee;

    internal GoShellBinding(MethodInvoker[] invokers, bool[] dereference, Type? boxType)
    {
        m_invokers = invokers;
        m_dereference = dereference;
        m_pointee = boxType?.GetProperty(nameof(ж<int>.Value));
    }

    /// <summary>
    /// Invokes the receiver method bound at <paramref name="index"/> against <paramref name="target"/>.
    /// </summary>
    /// <param name="index">Interface method index.</param>
    /// <param name="target">The Go dynamic value as held by the shell — a receiver box or a boxed value.</param>
    /// <param name="args">Forwarded call arguments.</param>
    /// <returns>The method's result, or <c>null</c> for a void method.</returns>
    public object? Invoke(int index, object target, params object?[] args)
    {
        // A value-receiver method reached through a pointer source runs on the CURRENT pointee: Go
        // copies the value at the call, so the deref has to happen per call, never once at binding.
        if (m_dereference[index] && m_pointee is not null)
            target = m_pointee.GetValue(target)!;

        MethodInvoker invoker = m_invokers[index];

        // The bound members are static extension methods, so MethodInvoker's `obj` is always null
        // and the RECEIVER occupies the first invoker argument -- which is why each arm below is
        // offset by one, and why arity 3 is the last fixed one (the BCL tops out at four arguments).
        // Taking those overloads avoids allocating and zeroing a fresh object?[] on every forwarded
        // call. A Go method with no parameters -- Len, Error, String, Less -- is the common case and
        // now allocates nothing here at all. It matters most under Native AOT, where the binder's
        // belt degrades BOTH shell tiers to this reflective one, so the cost is paid twice per
        // interface value rather than once.
        switch (args.Length)
        {
            case 0:
                return invoker.Invoke(null, target);
            case 1:
                return invoker.Invoke(null, target, args[0]);
            case 2:
                return invoker.Invoke(null, target, args[0], args[1]);
            case 3:
                return invoker.Invoke(null, target, args[0], args[1], args[2]);
        }

        object?[] arguments = new object?[args.Length + 1];
        arguments[0] = target;
        args.CopyTo(arguments, 1);

        // Span overload chosen explicitly: the object?[] would otherwise be ambiguous with the
        // single-argument overload.
        return invoker.Invoke(null, (Span<object?>)arguments);
    }
}
