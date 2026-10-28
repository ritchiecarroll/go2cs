// TypeExtensions.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

#pragma warning disable IL2075
#pragma warning disable IL2067
#pragma warning disable IL2055
#pragma warning disable IL2060
#pragma warning disable IL2080
#pragma warning disable IL2070
#pragma warning disable IL2026

namespace go.golib;

/// <summary>
/// Runtime answers to questions asked of a <see cref="Type"/> that the C# type system settles at
/// compile time but converted Go code has to settle at run time.
/// </summary>
/// <remarks>
/// <para>
/// This primary file holds the questions a single <see cref="Type"/> can answer ALONE: which
/// operators it declares, whether it nominally implements an interface, and whether it is a
/// converted <c>dyn</c> type. Everything here is memoized per type, because each answer is an
/// immutable fact about the type while READING it is a reflection lookup that allocates.
/// </para>
/// <para>
/// The class is <c>partial</c> because the rest of it is three concerns that share nothing but the
/// name of this class, and burying any one inside the others is what made the single file hard to
/// read. Each sibling carries a banner explaining itself:
/// </para>
/// <list type="bullet">
/// <item><c>TypeExtensions.ExtensionMethodRegistry.cs</c> — the process-wide scan that discovers
/// converted Go methods (emitted as C# extension methods) and makes them callable.</item>
/// <item><c>TypeExtensions.GoMethodSets.cs</c> — Go's value-vs-pointer receiver rule and the
/// structural interface-satisfaction probe built on it.</item>
/// <item><c>TypeExtensions.NumericConversions.cs</c> — boxed scalar to Go scalar conversions.</item>
/// </list>
/// <para>
/// The one cross-file coupling to know about: the registry's assembly-load hook clears caches
/// declared in the method-sets file, because both are derived from the same scan. Its banner
/// spells out the rule for adding another.
/// </para>
/// <para>
/// <see cref="ImplementsInterface"/> stays HERE, next to the operator lookups, rather than with the
/// method sets — and the contrast is worth keeping in view. It is the NOMINAL test: does this CLR
/// type declare that interface, directly or through a base. Go's test is structural, and that one
/// lives in the method-sets file. A type can pass one and fail the other in both directions.
/// </para>
/// </remarks>
public static partial class TypeExtensions
{
    private static readonly ConcurrentDictionary<Type, bool> s_dynamicTypes = [];
    private static readonly ConcurrentDictionary<Type, MethodInfo?> s_typeEqualityOperators = [];
    private static readonly ConcurrentDictionary<Type, MethodInfo?> s_onesComplementOperators = [];

    /// <summary>
    /// Get the "==" equality operator for the specified <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Type to search for equality operator.</param>
    /// <returns>Equality operator for <paramref name="type"/> if found; otherwise, <c>null</c>.</returns>
    /// <remarks>
    /// Go's <c>==</c> on two interface values compares their DYNAMIC values, whose type is not known
    /// until run time, so the equality tail in <c>builtin</c> has to find the right <c>op_Equality</c>
    /// by reflection and invoke it. Only the exact <c>(type, type)</c> overload counts: a converted
    /// type routinely declares comparisons against <c>NilType</c> and against its underlying type as
    /// well, and picking one of those would compare the wrong pair.
    /// </remarks>
    public static MethodInfo? GetEqualityOperator(this Type type)
    {
        return s_typeEqualityOperators.GetOrAdd(type, _ => type.GetMethod("op_Equality", BindingFlags.Static | BindingFlags.Public, [type, type]));
    }

    /// <summary>
    /// Get the "~" ones complement operator for the specified <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Type to search for the ones complement operator.</param>
    /// <returns>Ones complement operator for <paramref name="type"/> if found; otherwise, <c>null</c>.</returns>
    /// <remarks>
    /// For go2cs pointers, <see cref="go.ж{T}"/>, the ones complement operator is used to dereference
    /// the pointer, i.e., to get the value that the pointer points to, its element. Since the pointer
    /// class is a generic type, this function can be used to get the pointer value using the static
    /// ones complement operator without needing to know the pointer type.
    /// </remarks>
    public static MethodInfo? GetOnesComplementOperator(this Type type)
    {
        return s_onesComplementOperators.GetOrAdd(type, _ => type.GetMethod("op_OnesComplement", BindingFlags.Static | BindingFlags.Public, [type]));
    }

    /// <summary>
    /// Determines if <paramref name="targetType"/> implements specified <paramref name="interfaceType"/>.
    /// </summary>
    /// <param name="targetType">Target type to test.</param>
    /// <param name="interfaceType">Interface to search.</param>
    /// <returns>
    /// <c>true</c> if <paramref name="targetType"/> implements specified <paramref name="interfaceType"/>;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// The NOMINAL test — the CLR's own "does this type declare that interface", walked up the base
    /// chain and through inherited interfaces. It is NOT the Go question, and the two disagree in
    /// both directions: a converted type satisfies a Go interface it has never heard of merely by
    /// having the methods, and golib's own core interfaces carry static members no Go method set
    /// contains. <c>StructurallyImplements</c> (TypeExtensions.GoMethodSets.cs) is the Go answer.
    /// This one is used where CLR assignability really is the question — ranking extension-method
    /// candidates by how closely their receiver relates to a target type.
    /// </remarks>
    public static bool ImplementsInterface(this Type? targetType, Type interfaceType)
    {
        if (!interfaceType.IsInterface)
            return false;

        while (targetType is not null)
        {
            if (targetType.GetInterfaces().Any(targetInterface => targetInterface == interfaceType || targetInterface.ImplementsInterface(interfaceType)))
                return true;

            targetType = targetType.BaseType;
        }

        return false;
    }

    /// <summary>
    /// Finds the explicit conversion operator for converting <paramref name="targetType"/> to
    /// <paramref name="genericType"/> of <paramref name="targetType"/>.
    /// </summary>
    /// <param name="genericType">Generic type to search.</param>
    /// <param name="targetType">Target type of generic type.</param>
    /// <returns>Explicit conversion operator, if found; otherwise <c>null</c>.</returns>
    [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("AOT", "IL3050", Justification = global::go.GoTypeRegistry.AotNotReachedJustification)]
    public static MethodInfo GetExplicitGenericConversionOperator(this Type genericType, Type targetType)
    {
        Type genericOfType = genericType.MakeGenericType(targetType);

        return genericOfType
               .GetMethods(BindingFlags.Static | BindingFlags.Public)
               .FirstOrDefault(method => IsConversionOperator(method, genericOfType, targetType))!;
    }

    /// <summary>
    /// Determines if the specified <paramref name="valueType"/> is a dynamic type.
    /// </summary>
    /// <param name="valueType">Type to check.</param>
    /// <returns><c>true</c> if <paramref name="valueType"/> is a dynamic type; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// Memoized per type, like <see cref="GetStructFieldNames"/> beside it: the answer is an
    /// immutable fact about the type, but reading it is a custom-attribute lookup that materializes
    /// a fresh attribute instance on every call — measured at ~780 ns and 368 bytes under the JIT.
    /// This sits on the type-assert miss path (<c>builtin.TryTypeAssert</c>), so an uncached lookup
    /// is paid by every <c>x.(SomeStruct)</c> that does not match.
    /// </remarks>
    public static bool IsDynamicType(this Type valueType)
    {
        return s_dynamicTypes.GetOrAdd(valueType, static type =>
        {
            GoTypeAttribute? goType = type.GetCustomAttribute<GoTypeAttribute>();
            return goType is not null && goType.Definition == "dyn";
        });
    }

    private static bool IsConversionOperator(MethodInfo method, Type genericOfType, Type targetType)
    {
        ParameterInfo[] parameters = method.GetParameters();

        return method.Name == "op_Explicit" &&
               method.ReturnType == genericOfType &&
               parameters.Length == 1 &&
               parameters[0].ParameterType == targetType;
    }
}
