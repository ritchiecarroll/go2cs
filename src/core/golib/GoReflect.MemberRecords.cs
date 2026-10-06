// GoReflect.MemberRecords.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Concurrent;
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
}
