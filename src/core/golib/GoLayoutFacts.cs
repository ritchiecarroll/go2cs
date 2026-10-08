// GoLayoutFacts.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable CheckNamespace
// ReSharper disable StaticMemberInGenericType

using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace go;

/// <summary>
/// Whether <typeparamref name="T"/>'s C# layout DIVERGES from Go's: the question a native-memory view must
/// ask before it strides over Go-laid-out bytes (A17's guard, COORD ruling 2026-09-28).
/// </summary>
/// <remarks>
/// <para>
/// A Go zero-size field occupies no bytes; a C# field always occupies at least one. The converter's
/// zero-size-field layout arc (src/go2cs/zeroSizeFieldLayout.go) restores Go's layout with
/// <c>[StructLayout(LayoutKind.Explicit, Size = GoSize)]</c> and Go's offsets, and a struct carrying that
/// stamp is TRUSTED: its Size is Go's by construction. A struct that carries a zero-size field WITHOUT it
/// (the arc excludes managed structs and embeds) is larger in C# than in Go, so a native view striding by
/// the C# size reads and writes past the elements Go laid out: the A17 root, where
/// <c>atomicScavChunkData</c> measured 16 bytes against Go's 8, every scavenge-index read landed on an
/// unmapped page (linux AccessViolation) or past the end of the reservation (windows, silent), and the
/// stride went unnoticed until the host died.
/// </para>
/// <para>
/// The walk is recursive (a struct field of a diverging struct diverges) and runs ONCE per closed
/// <typeparamref name="T"/> on the cold path; every later query is a static readonly read. No new converter
/// metadata: the explicit-layout stamp already says what the arc laid out (the NO-NEW-METADATA ruling).
/// </para>
/// </remarks>
internal static class GoLayoutFacts<T>
{
    /// <summary>Whether a native view over <typeparamref name="T"/> would stride differently from Go.</summary>
    internal static readonly bool SizeDiverges = Diverges(typeof(T), 0);

    [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2070", Justification = GoTypeRegistry.RegisteredTypeJustification)]
    private static bool Diverges(Type type, int depth)
    {
        if (depth > 16 || !type.IsValueType || type.IsPrimitive || type.IsEnum || type.IsPointer)
            return false;

        FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        // A wholly zero-size struct: Go's stride is 0, C#'s is at least 1.
        if (GoZeroSizeFacts.Classify(type))
            return true;

        bool explicitLayout = type.StructLayoutAttribute?.Value == LayoutKind.Explicit;

        foreach (FieldInfo field in fields)
        {
            // A zero-size field under the arc's explicit layout takes no Go bytes; without it, it takes one.
            if (GoZeroSizeFacts.Classify(field.FieldType))
            {
                if (!explicitLayout)
                    return true;

                continue;
            }

            if (Diverges(field.FieldType, depth + 1))
                return true;
        }

        return false;
    }
}
