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
/// box (<c>reflect.New</c>), the canonical typed nil pointer (<c>reflect.Zero</c> of a pointer type, with or without an
/// array's dims), and a field-alias box over a parent box's field of this type (<c>reflect.Value.Field</c>).
/// </summary>
/// <remarks>
/// Trim stage 3c-2b (docs/PLAN-golib-full-trim.md, section 9): the bridge closed a generic helper over the type with
/// MakeGenericMethod, which Native AOT cannot do for an instantiation it never compiled. <see cref="GoTypeOps{T}"/> is
/// compiled for every type whose ops some compiled code names: go2cs-gen names a struct's and its pointer's on the
/// struct's generated part (<see cref="IGoTypeOpsSource"/>), golib's containers name their own, and the builtins are
/// named here. <see cref="GoTypeOps.Of"/> finds them.
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

    /// <summary>A field-alias box over <paramref name="parentBox"/>'s field of this type, through <paramref name="accessor"/>.</summary>
    object FieldBox(object parentBox, Delegate accessor);
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

    /// <inheritdoc/>
    public object FieldBox(object parentBox, Delegate accessor) => new FieldRefBox<T>(parentBox, (FieldRefFunc<T>)accessor, accessor);
}

/// <summary>
/// Names a Go type's operations and its pointer's, at FIXED depth: implemented on every struct and named type's generated
/// part by go2cs-gen, and by golib's containers (trim stage 3c-2b).
/// </summary>
/// <remarks>
/// <see cref="ж{T}"/> does not implement it: a box's own source would name <c>GoTypeOps&lt;ж&lt;ж&lt;T&gt;&gt;&gt;</c>, and
/// that level's the next, without bound. A pointer to a pointer is the fallback's (<see cref="GoTypeOps.Of"/>).
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IGoTypeOpsSource
{
    /// <summary>This type's operations.</summary>
    IGoTypeOps TypeOps { get; }

    /// <summary>The operations of a pointer to this type (<c>ж&lt;T&gt;</c> as a pointee or a field's type).</summary>
    IGoTypeOps BoxTypeOps { get; }
}

/// <summary>Finds the <see cref="IGoTypeOps"/> of a type known only at run time (trim stage 3c-2b).</summary>
public static class GoTypeOps
{
    private static readonly ConcurrentDictionary<Type, IGoTypeOps> s_ops = new();

    static GoTypeOps()
    {
        // The builtins have no generated part to name their ops; these name them, and a pointer to each.
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
    /// members read nothing of the value); a pointer's, through its pointee's source; and otherwise the fallback, which
    /// closes the operations over the type at run time: an interface, a func type, a pointer to a pointer. That is the
    /// one dynamic-code site left on these paths, the boundary trim stage 3d annotates.
    /// </remarks>
    public static IGoTypeOps Of(Type type) => s_ops.GetOrAdd(type, static t => FromSource(t) ?? dynamicOps(t));

    // The operations compiled code names for this type, or null when only the fallback can answer (GolibTests pins which
    // types answer here: under the JIT the fallback answers the same instance, so the route is invisible without this).
    internal static IGoTypeOps? FromSource(Type type)
    {
        if (isSourceCandidate(type) && uninitialized(type) is IGoTypeOpsSource source && source.TypeOps.Type == type)
            return source.TypeOps;

        if (GoReflect.TryBoxPointee(type, out Type? pointee) && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ж<>) &&
            isSourceCandidate(pointee) && uninitialized(pointee) is IGoTypeOpsSource pointeeSource && pointeeSource.BoxTypeOps.Type == type)
            return pointeeSource.BoxTypeOps;

        return null;
    }

    private static bool isSourceCandidate(Type type) => type.IsValueType && !type.IsPrimitive && !type.IsEnum && !type.ContainsGenericParameters;

    [UnconditionalSuppressMessage("Trimming", "IL2067", Justification =
        "No constructor runs: the zero value only carries its type's IGoTypeOpsSource members, which read nothing of it. " +
        "The types are the program's generated Go types and golib's containers, which Native AOT has constructed " +
        "(measured on the package consumer and the 3c-2a reflect probe).")]
    private static object uninitialized(Type type) => RuntimeHelpers.GetUninitializedObject(type);

    // The one dynamic-code site on these paths (trim stage 3d's boundary): a type no compiled code names the operations of.
    [UnconditionalSuppressMessage("Trimming", "IL2055", Justification = "GoTypeOps<T> has no member requirements on T.")]
    [UnconditionalSuppressMessage("Trimming", "IL2072", Justification = "GoTypeOps<T>'s private constructor is kept by its Instance field.")]
    private static IGoTypeOps dynamicOps(Type type) =>
        (IGoTypeOps)typeof(GoTypeOps<>).MakeGenericType(type).GetField(nameof(GoTypeOps<int>.Instance))!.GetValue(null)!;
}
