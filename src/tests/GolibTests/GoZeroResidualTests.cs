// GoZeroResidualTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;
using sha3 = go.crypto.sha3_package;

namespace GolibTests;

/// <summary>
/// Pins what <see cref="builtin.GoZero{T}()"/> constructs and what it does not: the registry option (B)
/// COORD ruled for Go zero values produced where only a type parameter is known (<c>var z T</c>, a named
/// result, a map miss, a closed-channel receive, a failed comma-ok assertion).
/// </summary>
/// <remarks>
/// go2cs-gen registers a factory for every NON-generic struct whose <c>default</c> is not its Go zero value.
/// A GENERIC one has no closed type a module initializer could register, so it keeps <c>default</c>. That is
/// the residual the ruling states, with no reflection fallback. At go1.24.13 the converted std has one such
/// type, <c>unique.uniqueMap[T]</c>, and it is needy only through the generator's conservative
/// promoted-embed rule. Its embeds are inline value slots, so its <c>default</c> is a usable Go zero value.
/// No std path zeroes it by value: it is only ever reached through a pointer (the GoZero census3 reading).
/// The residual arm pins the golib half with a local generic needy struct, and the generator half with
/// the real <c>uniqueMap</c>. If either arm starts failing, the residual has changed and the ruling
/// needs re-reading.
/// </remarks>
[TestClass]
public class GoZeroResidualTests
{
    private const BindingFlags InstanceFields = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    // A generic struct with a fixed-array field: `default` gives the array a null backing and length 0.
    // Nothing registers it, which is exactly the residual's shape.
    private struct GenericNeedy<T>
    {
        public array<T> vals = new(4);

        public GenericNeedy() { }
    }

    // The registration a type's module initializer made, read through the same reflection for every arm
    // so the positive arm shows the instrument can report one.
    private static object RegisteredFactory(Type type) =>
        typeof(GoZeroFactory<>).MakeGenericType(type).GetField(nameof(GoZeroFactory<int>.Create))!.GetValue(null);

    // A converted package's module initializer is its Go package init, and a Go program that holds one of
    // the package's values has run it. A test reaching the type only by `typeof` has not, so it runs it
    // explicitly; without this, a missing registration could only mean "not loaded yet".
    private static void RunPackageInit(Type type) =>
        RuntimeHelpers.RunModuleConstructor(type.Module.ModuleHandle);

    // SHA3 holds a fips140 sha3.Digest, whose `a [200]byte` is the fixed-array field.
    private static nint DigestStateLength(sha3.SHA3 value)
    {
        object digest = typeof(sha3.SHA3).GetField("s", InstanceFields)!.GetValue(value)!;
        object state = digest.GetType().GetField("a", InstanceFields)!.GetValue(digest)!;

        return ((array<byte>)state).Length;
    }

    [TestMethod]
    public void ARegisteredNeedyStructIsConstructed()
    {
        RunPackageInit(typeof(sha3.SHA3));

        Assert.IsNotNull(RegisteredFactory(typeof(sha3.SHA3)), "go2cs-gen registers a non-generic needy struct");
        Assert.AreEqual((nint)200, DigestStateLength(GoZero<sha3.SHA3>()), "GoZero constructs the fixed array at its Go length");

        // Control: the instrument can read the broken zero it exists to replace.
        Assert.AreEqual((nint)0, DigestStateLength(default), "default(SHA3) leaves the array at length 0");
    }

    [TestMethod]
    public void AnUnregisteredGenericNeedyStructKeepsDefault()
    {
        Assert.IsNull(GoZeroFactory<GenericNeedy<int>>.Create, "nothing registers a generic struct");
        Assert.AreEqual((nint)0, GoZero<GenericNeedy<int>>().vals.Length, "the stated residual: GoZero is default(T)");

        // Control: the same read sees a constructed zero, so the 0 above is default and not the instrument.
        Assert.AreEqual((nint)4, new GenericNeedy<int>().vals.Length, "the constructor gives the array its length");
    }

    [TestMethod]
    public void TheGeneratorRegistersNoGenericStruct()
    {
        Type uniqueMap = typeof(unique_package).Assembly.GetType("go.unique_package+uniqueMap`1", throwOnError: true)!;

        RunPackageInit(uniqueMap);

        Assert.IsNull(RegisteredFactory(uniqueMap.MakeGenericType(typeof(nint))), "unique.uniqueMap[T] keeps default");
    }
}
