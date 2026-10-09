// GoReflect.MemberRecords.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace go;

// -------- member records ([GoMemberRecord], the comment-carried member facts) --------

public static partial class GoReflect
{
    private const BindingFlags DeclaredFields = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

    private static readonly ConcurrentDictionary<Type, GoMemberRecordAttribute[]> s_memberRecords = new();

    /// <summary>
    /// The <see cref="GoMemberRecordAttribute"/>s go2cs-gen generated on <paramref name="type"/>, each checked
    /// against the member it names when first read: a record naming a member the type does not declare, or one
    /// that cannot hold its fact, is refused BY NAME rather than applied.
    /// </summary>
    internal static GoMemberRecordAttribute[] MemberRecords(Type type) =>
        s_memberRecords.GetOrAdd(type, static declaring =>
        {
            GoMemberRecordAttribute[] records = (GoMemberRecordAttribute[])declaring.GetCustomAttributes(typeof(GoMemberRecordAttribute), false);

            foreach (GoMemberRecordAttribute record in records)
            {
                switch (record.Fact)
                {
                    case GoMemberFact.Embedded:
                        if (declaring.GetField(record.Member, DeclaredFields) is null)
                            throw new InvalidOperationException($"go2cs: [GoMemberRecord] on {declaring.FullName} records '{record.Member}' as an embedded field, but {declaring.Name} declares no field of that name");

                        break;

                    case GoMemberFact.Tag:
                        if (declaring.GetField(record.Member, DeclaredFields) is null && declaring.GetProperty(record.Member, DeclaredFields) is null)
                            throw new InvalidOperationException($"go2cs: [GoMemberRecord] on {declaring.FullName} records a struct tag for '{record.Member}', but {declaring.Name} declares no field or embedded-field property of that name");

                        if (record.Value is null)
                            throw new InvalidOperationException($"go2cs: [GoMemberRecord] on {declaring.FullName} records a struct tag for '{record.Member}' with no tag");

                        break;

                    case GoMemberFact.Dims:
                        if (declaring.GetField(record.Member, DeclaredFields) is null)
                            throw new InvalidOperationException($"go2cs: [GoMemberRecord] on {declaring.FullName} records array dims for '{record.Member}', but {declaring.Name} declares no field of that name");

                        if (record.Dims is not { Length: > 0 })
                            throw new InvalidOperationException($"go2cs: [GoMemberRecord] on {declaring.FullName} records array dims for '{record.Member}' with no dims");

                        break;

                    case GoMemberFact.KeyDims:
                        if (declaring.GetField(record.Member, DeclaredFields) is null)
                            throw new InvalidOperationException($"go2cs: [GoMemberRecord] on {declaring.FullName} records map key dims for '{record.Member}', but {declaring.Name} declares no field of that name");

                        if (record.Dims is not { Length: > 0 })
                            throw new InvalidOperationException($"go2cs: [GoMemberRecord] on {declaring.FullName} records map key dims for '{record.Member}' with no dims");

                        break;

                    case GoMemberFact.Descriptor:
                        if (declaring.GetField(record.Member, DeclaredFields) is null)
                            throw new InvalidOperationException($"go2cs: [GoMemberRecord] on {declaring.FullName} records a descriptor carrier for '{record.Member}', but {declaring.Name} declares no field of that name");

                        if (record.Carrier is not { IsInterface: true })
                            throw new InvalidOperationException($"go2cs: [GoMemberRecord] on {declaring.FullName} records a descriptor carrier for '{record.Member}' that is not an interface ({record.Carrier?.FullName ?? "none"})");

                        break;

                    default:
                        throw new InvalidOperationException($"go2cs: [GoMemberRecord] on {declaring.FullName} for '{record.Member}' carries an unknown fact ({(byte)record.Fact})");
                }
            }

            return records;
        });

    /// <summary>
    /// Whether <paramref name="field"/> is a Go EMBEDDED field stated by hand ([GoEmbedded]) or by the
    /// converter's <c>/*embed*/</c> comment, which go2cs-gen records on the declaring type.
    /// </summary>
    internal static bool FieldIsEmbedded(FieldInfo field)
    {
        if (field.IsDefined(typeof(GoEmbeddedAttribute), false))
            return true;

        if (field.DeclaringType is not { } declaring)
            return false;

        foreach (GoMemberRecordAttribute record in MemberRecords(declaring))
        {
            if (record.Fact == GoMemberFact.Embedded && record.Member == field.Name)
                return true;
        }

        return false;
    }

    /// <summary>
    /// The Go struct tag <paramref name="member"/> of <paramref name="declaring"/> carries in the converter's tag
    /// comment, which go2cs-gen records on the type, or null when there is no such record.
    /// </summary>
    internal static string? RecordedTag(Type declaring, string member)
    {
        foreach (GoMemberRecordAttribute record in MemberRecords(declaring))
        {
            if (record.Fact == GoMemberFact.Tag && record.Member == member)
                return record.Value;
        }

        return null;
    }

    /// <summary>
    /// The Go array dims <paramref name="field"/> carries in the converter's dims comment, which go2cs-gen
    /// records on its declaring type, or null when there is no such record.
    /// </summary>
    internal static long[]? RecordedDims(FieldInfo field)
    {
        if (field.DeclaringType is not { } declaring)
            return null;

        foreach (GoMemberRecordAttribute record in MemberRecords(declaring))
        {
            if (record.Fact == GoMemberFact.Dims && record.Member == field.Name)
                return record.Dims;
        }

        return null;
    }

    /// <summary>
    /// The Go array dims of <paramref name="field"/>'s map KEY, which the converter's key dims comment states and
    /// go2cs-gen records on its declaring type, or null when there is no such record.
    /// </summary>
    internal static long[]? RecordedKeyDims(FieldInfo field)
    {
        if (field.DeclaringType is not { } declaring)
            return null;

        foreach (GoMemberRecordAttribute record in MemberRecords(declaring))
        {
            if (record.Fact == GoMemberFact.KeyDims && record.Member == field.Name)
                return record.Dims;
        }

        return null;
    }

    /// <summary>
    /// The DESCRIPTOR CARRIER <paramref name="field"/>'s Go type has, which the converter records on its declaring
    /// type (<see cref="GoMemberFact.Descriptor"/>), or null when there is no such record.
    /// </summary>
    internal static Type? RecordedDescriptorCarrier(FieldInfo field)
    {
        if (field.DeclaringType is not { } declaring)
            return null;

        foreach (GoMemberRecordAttribute record in MemberRecords(declaring))
        {
            if (record.Fact == GoMemberFact.Descriptor && record.Member == field.Name)
                return record.Carrier;
        }

        return null;
    }

    private const BindingFlags DeclaredMethods = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

    private static readonly ConcurrentDictionary<Type, Dictionary<MethodInfo, long[]?[]>> s_paramDimsRecords = new();

    /// <summary>
    /// The <see cref="GoParamDimsAttribute"/>s go2cs-gen generated on <paramref name="type"/>, each resolved to
    /// the one method it names when the type is first read: per method, one entry per parameter, null where a
    /// parameter has no record.
    /// </summary>
    /// <remarks>
    /// A record is refused BY NAME, never applied, when it matches no method <paramref name="type"/> declares or
    /// more than one, when its position is not one of the method's parameters, or when that parameter's type is
    /// not an array or a pointer to one: the comment the generator read and the signature the compiler built
    /// have drifted apart, and a dims-less answer would be silent.
    /// </remarks>
    [UnconditionalSuppressMessage("Trimming", "IL2070",
        Justification = "Every method a [GoParamDims] record names is kept by go2cs-gen's MemberRecordGenerator: an empty module " +
                        "initializer in the same package class carries a DynamicDependency for each recorded method (trim stage 3b, " +
                        "docs/PLAN-golib-full-trim.md section 9.6), and GenTests' MemberRecordGeneratorTests.EveryParamDimsRecordedMethodIsKeptForTrimming checks every record has one. " +
                        "A method no record names is never looked up here.")]
    internal static Dictionary<MethodInfo, long[]?[]> ParamDimsRecords(Type type) =>
        s_paramDimsRecords.GetOrAdd(type, static declaring =>
        {
            Dictionary<MethodInfo, long[]?[]> resolved = new();

            foreach (GoParamDimsAttribute record in (GoParamDimsAttribute[])declaring.GetCustomAttributes(typeof(GoParamDimsAttribute), false))
            {
                MethodInfo? target = null;

                foreach (MethodInfo method in declaring.GetMethods(DeclaredMethods))
                {
                    if (!SignatureMatches(method, record.Method, record.ParameterTypes))
                        continue;

                    if (target is not null)
                        throw new InvalidOperationException($"go2cs: [GoParamDims] on {declaring.FullName} for '{record.Method}' matches more than one method of that name and signature");

                    target = method;
                }

                if (target is null)
                    throw new InvalidOperationException($"go2cs: [GoParamDims] on {declaring.FullName} records array dims for '{record.Method}', but {declaring.Name} declares no method of that name and signature");

                ParameterInfo[] parameters = target.GetParameters();

                if (record.Position < 0 || record.Position >= parameters.Length)
                    throw new InvalidOperationException($"go2cs: [GoParamDims] on {declaring.FullName} records array dims for parameter {record.Position} of '{record.Method}', which has {parameters.Length} parameters");

                Type parameterType = parameters[record.Position].ParameterType;

                if (parameterType.IsByRef)
                    parameterType = parameterType.GetElementType()!;

                if (record.Dims.Length == 0 || !(KindOf(parameterType) == Array || parameterType.IsGenericType && parameterType.GetGenericTypeDefinition() == typeof(ж<>) && KindOf(parameterType.GetGenericArguments()[0]) == Array))
                    throw new InvalidOperationException($"go2cs: [GoParamDims] on {declaring.FullName} records array dims for parameter {record.Position} of '{record.Method}', whose type {parameterType.FullName} is not an array or a pointer to one");

                if (!resolved.TryGetValue(target, out long[]?[]? dims))
                {
                    dims = new long[]?[parameters.Length];
                    resolved[target] = dims;
                }

                dims[record.Position] = record.Dims;
            }

            return resolved;
        });

    /// <summary>
    /// Whether <paramref name="method"/> is the one named <paramref name="name"/> with <paramref name="parameterTypes"/>
    /// (receiver included; a <c>ref</c> parameter by its element type): the one key by which a generated record names a
    /// method (<see cref="GoParamDimsAttribute"/>, <see cref="GoSigChanDirAttribute"/>).
    /// </summary>
    /// <remarks>
    /// An attribute argument cannot name a type parameter, so a generic method's key spells a type built from one by
    /// its open definition (<c>typeof(array&lt;&gt;)</c>, matching any <c>array&lt;X&gt;</c>) and a bare type parameter
    /// as null (matching any type); every other entry matches by Type identity. Go has no overloading, so the methods
    /// of one name in a declaring type differ by receiver type, which the key keeps; go2cs-gen refuses at compile time
    /// a key that would still match two of them (face lift D2).
    /// </remarks>
    internal static bool SignatureMatches(MethodInfo method, string name, Type?[] parameterTypes)
    {
        if (method.Name != name)
            return false;

        ParameterInfo[] parameters = method.GetParameters();

        if (parameters.Length != parameterTypes.Length)
            return false;

        for (int i = 0; i < parameters.Length; i++)
        {
            Type parameterType = parameters[i].ParameterType;

            if (parameterType.IsByRef)
                parameterType = parameterType.GetElementType()!;

            Type? keyed = parameterTypes[i];

            if (keyed is null)
                continue;

            if (keyed.IsGenericTypeDefinition)
            {
                if (!parameterType.IsGenericType || parameterType.GetGenericTypeDefinition() != keyed)
                    return false;

                continue;
            }

            if (parameterType != keyed)
                return false;
        }

        return true;
    }

    // A func value bound to a constructed generic method (`first<int>`) finds the records of its definition, which is
    // what a declaring type's methods are.
    internal static MethodInfo RecordedMethod(MethodInfo method) =>
        method.IsGenericMethod && !method.IsGenericMethodDefinition ? method.GetGenericMethodDefinition() : method;
}
