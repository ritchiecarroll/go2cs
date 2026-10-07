// CallSiteKeyTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

// go.runtime interns a captured call site (runtime.Caller, runtime.Callers, a panic's frames) under a key naming its
// METHOD. It named the method by its metadata token, and Native AOT gives a member no token: reading one throws
// "There is no metadata token available for the given member", so a converted program published with Native AOT
// died at its first runtime.Caller (measured 2026-10-06 on windows: the package-consumer fixture exits 2 with that
// stack, from internCallerFrame). A method whose token cannot be read must intern exactly like the method it is.
[TestClass]
public class CallSiteKeyTests
{
    // The real method in every respect but one: its metadata token cannot be read, as under Native AOT.
    private sealed class TokenlessMethod(MethodInfo real) : MethodInfo
    {
        public override int MetadataToken => throw new InvalidOperationException("There is no metadata token available for the given member.");
        public override RuntimeMethodHandle MethodHandle => real.MethodHandle;
        public override Module Module => real.Module;
        public override string Name => real.Name;
        public override Type? DeclaringType => real.DeclaringType;
        public override Type? ReflectedType => real.ReflectedType;
        public override MethodAttributes Attributes => real.Attributes;
        public override ICustomAttributeProvider ReturnTypeCustomAttributes => real.ReturnTypeCustomAttributes;
        public override Type ReturnType => real.ReturnType;
        public override bool IsGenericMethod => real.IsGenericMethod;
        public override bool IsGenericMethodDefinition => real.IsGenericMethodDefinition;
        public override Type[] GetGenericArguments() => real.GetGenericArguments();
        public override MethodInfo GetBaseDefinition() => real.GetBaseDefinition();
        public override MethodImplAttributes GetMethodImplementationFlags() => real.GetMethodImplementationFlags();
        public override ParameterInfo[] GetParameters() => real.GetParameters();
        public override object? Invoke(object? obj, BindingFlags invokeAttr, Binder? binder, object?[]? parameters, CultureInfo? culture) => real.Invoke(obj, invokeAttr, binder, parameters, culture);
        public override object[] GetCustomAttributes(bool inherit) => real.GetCustomAttributes(inherit);
        public override object[] GetCustomAttributes(Type attributeType, bool inherit) => real.GetCustomAttributes(attributeType, inherit);
        public override bool IsDefined(Type attributeType, bool inherit) => real.IsDefined(attributeType, inherit);
    }

    private static readonly MethodInfo s_intern = typeof(runtime_package).GetMethod("internCallerFrame", BindingFlags.NonPublic | BindingFlags.Static)
        ?? throw new InvalidOperationException("go.runtime_package.internCallerFrame is gone: this guard names it");

    private static object Intern(MethodBase method, StackFrame frame)
    {
        try
        {
            return s_intern.Invoke(null, [method, frame, true])!;
        }
        catch (TargetInvocationException ex) when (ex.InnerException is not null)
        {
            throw ex.InnerException;
        }
    }

    [TestMethod]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public void AMethodWithoutAMetadataTokenInternsLikeTheMethodItIs()
    {
        StackFrame frame = new(0, false);
        MethodInfo real = (MethodInfo)frame.GetMethod()!;
        TokenlessMethod tokenless = new(real);

        Assert.ThrowsException<InvalidOperationException>(() => _ = tokenless.MetadataToken, "premise: the stand-in has no token");

        object viaReal = Intern(real, frame);
        object viaTokenless;

        try
        {
            viaTokenless = Intern(tokenless, frame);
        }
        catch (InvalidOperationException ex)
        {
            Assert.Fail($"interning a call site read the method's metadata token, which Native AOT does not have: {ex.Message}");
            return;
        }

        Assert.AreEqual(viaReal, viaTokenless, "one method at one offset is one call site, whatever answers for its token");
        Assert.AreEqual(viaReal, Intern(real, frame), "and the site keeps its pc");
    }
}
