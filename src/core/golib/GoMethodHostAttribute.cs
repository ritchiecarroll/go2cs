// GoMethodHostAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Concurrent;

namespace go;

/// <summary>
/// Marks the class that holds the Go methods one type declares, and names that type.
/// </summary>
/// <remarks>
/// <para>
/// A Go method is an extension method on its receiver type. The methods a type declares are in a class of
/// that type's own, beside the package class in the package's namespace. The package class then holds what
/// the Go package declares at package level, and a method shares no class with a type, constant, variable or
/// function of its name, as it shares no scope with one in Go.
/// </para>
/// <para>
/// A reader that wants a type's methods from metadata finds the class by this attribute, never by deriving
/// its name: <c>[GoMethodHost(typeof(T))]</c> for a type <c>T</c>, and <c>typeof(T&lt;&gt;)</c> for a generic one.
/// The type's package class is the class the type is nested in.
/// </para>
/// </remarks>
/// <param name="receiver">The type whose declared methods the class holds.</param>
[AttributeUsage(AttributeTargets.Class)]
public sealed class GoMethodHostAttribute(Type receiver) : Attribute
{
    /// <summary>
    /// Gets the type whose declared methods the class holds.
    /// </summary>
    public Type Receiver { get; } = receiver;

    private static readonly ConcurrentDictionary<Type, Type> s_packageClasses = new();

    /// <summary>
    /// Gets the package class <paramref name="class"/> stands under for Go: for a class marked
    /// <c>[GoMethodHost]</c>, the class its receiver type is nested in; for any other class, itself.
    /// </summary>
    /// <remarks>
    /// Go names a method by its receiver type's package (<c>time.Time.Add</c>), and a function literal in a
    /// method by that method. A reader that derives a Go name from the class that declares a method reads
    /// the holder through this first. Computed once per class.
    /// </remarks>
    /// <param name="class">A top-level class.</param>
    public static Type PackageClassOf(Type @class)
    {
        ArgumentNullException.ThrowIfNull(@class);

        return s_packageClasses.GetOrAdd(@class, static type =>
        {
            if (!type.IsAbstract || !type.IsSealed || type.GetCustomAttributes(typeof(GoMethodHostAttribute), inherit: false) is not [GoMethodHostAttribute host])
                return type;

            Type package = host.Receiver;

            while (package.DeclaringType is { } outer)
                package = outer;

            return ReferenceEquals(package, host.Receiver) ? type : package;
        });
    }

    /// <summary>
    /// Gets whether <paramref name="class"/> is marked <c>[GoMethodHost]</c> for a type nested in a class.
    /// </summary>
    /// <param name="class">A top-level class.</param>
    public static bool IsHost(Type @class) => !ReferenceEquals(PackageClassOf(@class), @class);
}
