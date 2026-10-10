// GoTypeOps.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace go;

/// <summary>
/// The object-form operations reflect needs for a Go type it knows only at run time and holds no value of: a new heap
/// box (<c>reflect.New</c>) and the canonical typed nil pointer (<c>reflect.Zero</c> of a pointer type, with or without
/// an array's dims).
/// </summary>
/// <remarks>
/// <para>
/// Trim stage 3c-2b (docs/PLAN-golib-full-trim.md, section 9): the bridge closed a generic helper over the type with
/// MakeGenericMethod, which Native AOT cannot do for a VALUE-type instantiation it never compiled. <see cref="GoTypeOps{T}"/>
/// is compiled for every value type whose ops some compiled code names: go2cs-gen names a struct's on its generated part
/// (<see cref="IGoTypeOpsSource"/>) when the compilation spells a pointer to it, and the builtins are named here.
/// <see cref="GoTypeOps.Of"/> finds them.
/// </para>
/// <para>
/// golib's containers (array, slice, map, chan) carry NO face. A face on a generic container names the container's
/// operations, those make its box live, and the box builds a container of the container, whose face names the next level:
/// under Native AOT's default partial trim ILC expanded without bound and died (System.OverflowException in its type-system
/// hashtable, after about 100 minutes; measured 2026-10-09, rows D and E of the stage-3 table, and gone in row G, which
/// removed only the container faces). A container's operations are named by the escape registry instead (trim stage
/// 3c-2b(iii)), or come from the fallback.
/// </para>
/// <para>
/// MEASURED SHAPE (2026-10-09, package consumer, Native AOT full trim, win-x64): the first prototype also named a
/// pointer's ops (<c>GoTypeOps&lt;ж&lt;S&gt;&gt;</c>) and a field-alias box per type, and grew the executable +87.8% over trim
/// stage 3c-2a; this shape grows it +1.9%. A pointer's ops are a REFERENCE-type instantiation, which the fallback loads
/// under Native AOT (measured: <c>reflect.New</c> of <c>**T</c> and of an interface equal Go's output), so naming them per
/// type bought nothing; and a field-alias box cannot be reached under Native AOT until family E (the DynamicMethod field
/// accessors) is answered, so <c>GoReflect.FieldAliasBox</c> keeps its own dynamic site until then. Naming every
/// struct's ops then grew an fmt/reflect program +41% (each one makes the whole <c>ж&lt;S&gt;</c> machinery live); naming them
/// only for a struct whose pointer the compilation spells halved that, and dropping the container faces halved it again
/// (row G: worst program +10.8% over master under a full trim). Size is reported, never a gate: the owner's ruling
/// (2026-10-10) puts Go behavioural parity first.
/// </para>
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IGoTypeOps
{
    /// <summary>The type these are the operations of.</summary>
    Type Type { get; }

    /// <summary>A fresh heap box holding <paramref name="value"/>, or the type's zero value when it is null.</summary>
    object NewBox(object? value);

    /// <summary>The canonical typed nil pointer to this type (<c>ж&lt;T&gt;.NilBox</c>).</summary>
    object NilBox();

    /// <summary>The canonical typed nil pointer to this ARRAY type carrying its dims (<c>ж&lt;T&gt;.NilBoxOfDims</c>).</summary>
    object NilBoxOfDims(long[] dims);
}

/// <summary>The operations of <typeparamref name="T"/>: each names its own instantiation, so a compiled reference compiles them.</summary>
/// <typeparam name="T">The Go type.</typeparam>
/// <remarks>
/// No member builds anything that carries <see cref="IGoTypeOpsSource"/> over <typeparamref name="T"/> (a box,
/// <see cref="ж{T}"/>, carries none), so naming <c>GoTypeOps&lt;T&gt;</c> never makes ILC expand a further level: the
/// self-nesting a face over a generic container would cause (trim stage 3c, section 9's hazard rule).
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class GoTypeOps<T> : IGoTypeOps
{
    /// <summary>The one instance.</summary>
    public static readonly GoTypeOps<T> Instance = new();

    private GoTypeOps() { }

    /// <inheritdoc/>
    public Type Type => typeof(T);

    /// <inheritdoc/>
    public object NewBox(object? value) => value is null ? new StandardBox<T>(default(T)!) : new StandardBox<T>((T)value);

    /// <inheritdoc/>
    public object NilBox() => ж<T>.NilBox;

    /// <inheritdoc/>
    public object NilBoxOfDims(long[] dims) => ж<T>.NilBoxOfDims(dims);
}

/// <summary>
/// Names a Go VALUE type's operations: implemented by go2cs-gen on the generated part of a struct (or struct-kind named
/// type) whose pointer the compilation spells, <c>ж&lt;S&gt;</c> or <c>@new&lt;S&gt;</c> (trim stage 3c-2b).
/// </summary>
/// <remarks>
/// Only a value type carries it: its zero value is how golib reaches the member, and a reference type's operations are an
/// instantiation the fallback loads under Native AOT. <see cref="ж{T}"/> in particular does not: a box's own source would
/// name <c>GoTypeOps&lt;ж&lt;ж&lt;T&gt;&gt;&gt;</c>, and that level's the next, without bound. Nor do golib's generic
/// containers, for the same reason one level up (see <see cref="IGoTypeOps"/>); GolibTests pins both.
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IGoTypeOpsSource
{
    /// <summary>This type's operations.</summary>
    IGoTypeOps TypeOps { get; }
}

/// <summary>Finds the <see cref="IGoTypeOps"/> of a type known only at run time (trim stage 3c-2b).</summary>
public static class GoTypeOps
{
    private static readonly ConcurrentDictionary<Type, IGoTypeOps> s_ops = new();

    static GoTypeOps()
    {
        // The builtins have no generated part to name their ops; these name them, and a pointer to each (a pointer to a
        // builtin is common enough under reflect.New to be worth the table's few entries).
        register<bool>();
        register<sbyte>();
        register<short>();
        register<int>();
        register<long>();
        register<byte>();
        register<ushort>();
        register<uint>();
        register<ulong>();
        register<nint>();
        register<nuint>();
        register<uintptr>();
        register<float>();
        register<double>();
        register<complex64>();
        register<Complex>();
        register<@string>();
        register<object>();
        register<error>();
    }

    private static void register<T>()
    {
        s_ops[typeof(T)] = GoTypeOps<T>.Instance;
        s_ops[typeof(ж<T>)] = GoTypeOps<ж<T>>.Instance;
    }

    /// <summary>The operations of <paramref name="type"/>.</summary>
    /// <remarks>
    /// In order: a builtin's (and a pointer to one); a value type's own source, reached through a zero value of it (its
    /// members read nothing of the value); and otherwise the fallback, which closes the operations over the type at run
    /// time: a pointer, an interface, a func type -- reference types, whose instantiation Native AOT loads (measured). That
    /// is the one dynamic-code site left on these paths, the boundary trim stage 3d annotates.
    /// </remarks>
    public static IGoTypeOps Of(Type type) => s_ops.GetOrAdd(type, static t => FromSource(t) ?? dynamicOps(t));

    // The operations compiled code names for this type, or null when only the fallback can answer (GolibTests pins which
    // types answer here: under the JIT the fallback answers the same instance, so the route is invisible without this).
    internal static IGoTypeOps? FromSource(Type type) =>
        isSourceCandidate(type) && uninitialized(type) is IGoTypeOpsSource source && source.TypeOps.Type == type ? source.TypeOps : null;

    private static bool isSourceCandidate(Type type) => type.IsValueType && !type.IsPrimitive && !type.IsEnum && !type.ContainsGenericParameters;

    [UnconditionalSuppressMessage("Trimming", "IL2067", Justification =
        "No constructor runs: the zero value only carries its type's IGoTypeOpsSource member, which reads nothing of it. " +
        "The types are the program's generated Go types, which Native AOT has constructed " +
        "(measured on the package consumer and the 3c-2a reflect probe).")]
    private static object uninitialized(Type type) => RuntimeHelpers.GetUninitializedObject(type);

    // The one dynamic-code site on these paths (trim stage 3d's boundary): a type no compiled code names the operations of.
    [UnconditionalSuppressMessage("Trimming", "IL2055", Justification = "GoTypeOps<T> has no member requirements on T.")]
    [UnconditionalSuppressMessage("Trimming", "IL2072", Justification = "GoTypeOps<T>'s private constructor is kept by its Instance field.")]
    private static IGoTypeOps dynamicOps(Type type) =>
        (IGoTypeOps)typeof(GoTypeOps<>).MakeGenericType(type).GetField(nameof(GoTypeOps<int>.Instance))!.GetValue(null)!;
}
