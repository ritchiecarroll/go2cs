// AdapterRegistry.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace go;

/// <summary>
/// Runtime registry of generated interface-implementation adapters, keyed by the Go dynamic
/// value's type (a box by its <c>ж&lt;T&gt;</c>, whatever its kind) and the target interface type.
/// </summary>
/// <remarks>
/// <para>
/// A Go type switch or type assert against a NAMED interface must match by Go METHOD-SET
/// semantics: a raw receiver box <c>ж&lt;T&gt;</c> held in an interface matches
/// <c>case Iface:</c> whenever <c>*T</c> implements the interface. The box cannot nominally
/// implement the C# interface — the generated pointer adapter (<c>TжIface</c>) does — so each
/// generated adapter registers a wrapping factory here from a module initializer, and the golib
/// type-assert machinery re-wraps the box on demand. This stays reflection-free and
/// Native-AOT-safe: factories are compiled lambdas over closed generic types, and the module
/// initializer itself roots the adapter class against trimming.
/// </para>
/// <para>
/// The same pair can be registered by several assemblies (each assembly generates adapters for
/// its own recorded conversion sites); the first registration wins — all adapters for a pair
/// are behaviorally identical, forwarding to the same receiver methods.
/// </para>
/// </remarks>
public static class AdapterRegistry
{
    private static readonly ConcurrentDictionary<(Type, Type), Func<object, object>> s_factories = new();

    // Runtime duck-typing shell decisions, keyed the same way. A null value is a cached NEGATIVE —
    // the pair was probed and cannot bind. Kept separate from s_factories because that dictionary is
    // the NOMINAL tier consulted first by the type-assert machinery and cannot represent a miss; and
    // because a memoized shell must never shadow a generated adapter registered later by a
    // lazily-loaded assembly's module initializer.
    private static readonly ConcurrentDictionary<(Type, Type), Func<object, object>?> s_shellFactories = new();

    private static int s_epoch;

    /// <summary>
    /// Registers a factory that wraps a Go dynamic value of runtime type <paramref name="valueType"/>
    /// in its generated adapter implementing <paramref name="interfaceType"/>.
    /// </summary>
    /// <param name="valueType">Runtime type of the Go dynamic value, e.g., <c>ж&lt;T&gt;</c> for a pointer-sourced implementation.</param>
    /// <param name="interfaceType">Interface type the adapter implements.</param>
    /// <param name="factory">Factory that wraps a value of <paramref name="valueType"/> in the adapter.</param>
    public static void Register(Type valueType, Type interfaceType, Func<object, object> factory)
    {
        // Only a registration that actually ADDS a pair can invalidate a memoized decision: the
        // dictionary is first-wins, so a duplicate changes nothing an itab entry could have observed.
        if (s_factories.TryAdd((valueType, interfaceType), factory))
            Interlocked.Increment(ref s_epoch);
    }

    /// <summary>
    /// Monotonic counter bumped by every nominal adapter registration that adds a new pair.
    /// </summary>
    /// <remarks>
    /// The type-assert machinery projects both tiers into one per-interface itab cache
    /// (<c>builtin.Itab&lt;TInterface&gt;</c>), which reintroduces the hazard the two separate
    /// dictionaries here were written to avoid: a shell — or a decided MISS — memoized for a pair
    /// <em>before</em> a lazily-loaded assembly's module initializer registers the generated adapter
    /// for that same pair. Every itab entry carries the epoch it was decided under, so one
    /// registration invalidates all of them at once and each pair is re-formed against this
    /// registry on next use. Registration happens from module initializers and this dictionary is
    /// append-only, so the steady state never re-forms anything.
    /// </remarks>
    internal static int Epoch => Volatile.Read(ref s_epoch);

    /// <summary>
    /// Attempts to wrap Go dynamic <paramref name="value"/> in its registered adapter implementing
    /// <paramref name="interfaceType"/>.
    /// </summary>
    /// <param name="value">Go dynamic value, e.g., a receiver box <c>ж&lt;T&gt;</c>.</param>
    /// <param name="interfaceType">Target interface type.</param>
    /// <param name="wrapped">Adapter instance implementing <paramref name="interfaceType"/>, if registered.</param>
    /// <returns><c>true</c> if an adapter factory was registered for the pair; otherwise, <c>false</c>.</returns>
    public static bool TryWrap(object value, Type interfaceType, [NotNullWhen(true)] out object? wrapped)
    {
        if (TryGetAdapterFactory(value.GetType(), interfaceType, out Func<object, object>? factory))
        {
            wrapped = factory(value);
            return true;
        }

        wrapped = null;
        return false;
    }

    /// <summary>
    /// Looks up the generated adapter factory registered for a (dynamic type, interface) pair,
    /// without constructing anything.
    /// </summary>
    /// <param name="valueType">Runtime type of the Go dynamic value.</param>
    /// <param name="interfaceType">Target interface type.</param>
    /// <param name="factory">Registered wrapping factory, if the pair has one.</param>
    /// <returns><c>true</c> if an adapter factory was registered for the pair; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// The type-assert machinery memoizes the factory itself rather than re-hashing the
    /// <c>(Type, Type)</c> key on every assert, so it needs the lookup separated from the wrap.
    /// </remarks>
    internal static bool TryGetAdapterFactory(Type valueType, Type interfaceType, [NotNullWhen(true)] out Func<object, object>? factory)
    {
        (Type, Type) key = (AdapterKey(valueType), interfaceType);

        // A miss runs the adapters loaded assemblies deferred (GoDeferredRegistrations.Adapters), then looks again.
        return s_factories.TryGetValue(key, out factory) ||
               GoDeferredRegistrations.Adapters.Run() && s_factories.TryGetValue(key, out factory);
    }

    // The registration key of a Go dynamic value's runtime type. A generated pointer adapter registers
    // typeof(ж<T>), but no box has that runtime type: since the box kind split (36b7e9d96b) ж<T> is abstract, and
    // a box is one of its kinds (StandardBox<T>, SlottedStandardBox<T>, FieldRefBox<T>, ElemRefBox<T>,
    // NativeBox<T>, ...). Keyed by its runtime type, a box never found its adapter, and every assert of a raw box
    // to a named interface fell to the run-time shell tier: slower under the JIT, and a MISS under Native AOT,
    // where the shell's MakeGenericType over the pointee is not available (fmt printed a *PathError as
    // `&{read |0 0x8000000500000000}`). So a box is keyed by the ж<T> in its base chain; any other type is its
    // own key. Memoized per runtime type: a T has a handful of kinds, each resolved once.
    private static readonly ConcurrentDictionary<Type, Type> s_adapterKeys = new();

    internal static Type AdapterKey(Type valueType) =>
        valueType.IsClass ? s_adapterKeys.GetOrAdd(valueType, static type => BoxTypeOf(type) ?? type) : valueType;

    private static Type? BoxTypeOf(Type type)
    {
        for (Type? candidate = type; candidate is not null; candidate = candidate.BaseType)
        {
            if (candidate.IsGenericType && candidate.GetGenericTypeDefinition() == typeof(ж<>))
                return candidate;
        }

        return null;
    }

    /// <summary>
    /// Looks up the memoized runtime duck-typing shell decision for a (dynamic type, interface) pair.
    /// </summary>
    /// <param name="valueType">Runtime type of the Go dynamic value.</param>
    /// <param name="interfaceType">Target interface type.</param>
    /// <param name="factory">Shell factory when the pair binds; <c>null</c> when it was probed and cannot.</param>
    /// <returns><c>true</c> when the pair has already been decided either way.</returns>
    /// <remarks>
    /// Building a shell costs on the order of a microsecond, and <c>fmt</c> probes three interfaces
    /// for every formatted value — memoization is not an optimization here, it is what keeps the
    /// runtime tier off the hot path. See <see cref="AdapterBinder"/> for why the negative is cached
    /// too and why this cache is deliberately NOT cleared on assembly load.
    /// </remarks>
    internal static bool TryGetShellFactory(Type valueType, Type interfaceType, out Func<object, object>? factory)
    {
        return s_shellFactories.TryGetValue((valueType, interfaceType), out factory);
    }

    /// <summary>
    /// Records the runtime duck-typing shell decision for a (dynamic type, interface) pair.
    /// </summary>
    /// <param name="valueType">Runtime type of the Go dynamic value.</param>
    /// <param name="interfaceType">Target interface type.</param>
    /// <param name="factory">Shell factory, or <c>null</c> to record that the pair cannot bind.</param>
    internal static void MemoizeShellFactory(Type valueType, Type interfaceType, Func<object, object>? factory)
    {
        s_shellFactories[(valueType, interfaceType)] = factory;
    }
}
