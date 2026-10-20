// IGoEquatable.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;

namespace go;

/// <summary>
/// Go's <c>==</c> for a value whose static type is lost: <c>builtin.AreEqual(object?, object?)</c> compares two
/// interface values of one dynamic type through it, in compiled code.
/// </summary>
/// <remarks>
/// Implemented EXPLICITLY, so no member is added to an implementing type's surface: by go2cs-gen's struct and
/// inherited-type templates for every generated type, and by golib's comparable types. Every implementation is the
/// type's own <c>==</c> (<c>other is T v &amp;&amp; this == v</c>), never <see cref="object.Equals(object?)"/>, which a
/// named float's generated wrapper answers NaN-equal.
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IGoEquatable
{
    /// <summary>
    /// Whether <paramref name="other"/>, a value of this same dynamic type, equals this one by the type's <c>==</c>.
    /// </summary>
    bool GoEquals(object? other);
}

// The comparisons builtin.AreEqual(object?, object?) still answers by reflection: types with no IGoEquatable
// implementation and no primitive arm. Counted always (a test reads the count); tallied per type and written at process
// exit only when GO2CS_EQUALITY_FALLBACK_LOG names a file, so a -tests row can report which types remain.
internal static class EqualityFallbacks
{
    private static long s_count;
    private static readonly string? s_logPath = Environment.GetEnvironmentVariable("GO2CS_EQUALITY_FALLBACK_LOG");
    private static readonly ConcurrentDictionary<Type, long>? s_byType = s_logPath is null ? null : new();
    private static int s_exitHooked;

    // This thread's share of Count: a test reads it, so a comparison another test makes in parallel cannot move it.
    [ThreadStatic]
    private static long t_count;

    internal static long Count => Interlocked.Read(ref s_count);

    internal static long ThreadCount => t_count;

    internal static void Record(Type type)
    {
        Interlocked.Increment(ref s_count);
        t_count++;

        if (s_byType is null)
            return;

        s_byType.AddOrUpdate(type, 1, static (_, n) => n + 1);

        if (Interlocked.Exchange(ref s_exitHooked, 1) == 0)
            AppDomain.CurrentDomain.ProcessExit += static (_, _) => write();
    }

    private static void write()
    {
        try
        {
            File.AppendAllLines(s_logPath!, s_byType!.OrderByDescending(entry => entry.Value)
                .Select(entry => $"{entry.Value}\t{entry.Key.FullName}"));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // A diagnostic log must never be the thing that fails the process.
        }
    }
}
