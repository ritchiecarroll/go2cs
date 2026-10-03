// TypeExtensions.ExtensionMethodRegistry.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
// ReSharper disable InconsistentlySynchronizedField

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

#pragma warning disable IL2075
#pragma warning disable IL2067
#pragma warning disable IL2055
#pragma warning disable IL2060
#pragma warning disable IL2080
#pragma warning disable IL2070
#pragma warning disable IL2026

namespace go.golib;

// ---------------------------------------------------------------------------------------------
// EXTENSION-METHOD REGISTRY — the process-wide index of "which methods does this type have?"
//
// WHAT LIVES HERE
//   The assembly scan that finds every C# extension method in the process, the per-type caches
//   over it, the precedence rule for picking one when several match, and the delegate factory
//   that makes a found MethodInfo callable.
//
// WHY A SCAN AT ALL
//   A Go method is declared with its receiver, anywhere in the package, and the converter emits it
//   as a C# EXTENSION method on the receiver type. That is what keeps the generated C# readable —
//   `func (p Point) Abs() float64` stays a free function whose first parameter is the receiver —
//   but it means the method is NOT a member of the type. CLR metadata has no link from a type back
//   to the extension methods written for it (the language resolves them at COMPILE time, from the
//   namespaces in scope at the call site), so nothing in reflection can answer "what is this type's
//   Go method set". Scanning every loaded assembly is the only way to build that link at run time,
//   and the run-time answer is what interface satisfaction, duck-typed asserts and the reflection
//   bridge are all built on.
//
//   Live consumers: `error.cs` binds `Error()` on a dynamic value through GetExtensionMethod; the
//   generated interface shells (go2cs-gen's InterfaceShellEmitter) EMIT calls to
//   CreateStaticDelegate, so that method is on the frozen emitted surface even though golib itself
//   uses it only from error.cs; `builtin`'s equality tail reads GetEqualityOperator.
//
//   The scan skips the framework (`System.*`, `Microsoft.*`, `netstandard`, `WindowsBase`) and
//   golib's own satellite assemblies, and considers only sealed, non-nested, non-generic static
//   classes — which is exactly the shape the converter emits and a large saving over walking
//   every type in the BCL.
//
// CACHE INVALIDATION IS SHARED WITH THE SIBLING FILE — READ THIS BEFORE ADDING A CACHE
//   Assemblies load LATE. A converted program's package assemblies are not all present when the
//   first type question is asked, so any answer computed from the scan can be stale the moment
//   another assembly arrives. `ClearTypeCaches` is hooked to AppDomain.AssemblyLoad for exactly
//   that reason, and it clears caches DECLARED IN TypeExtensions.GoMethodSets.cs as well as the
//   two declared here. Adding a cache derived from the scan means adding it to that clear list:
//   a cache that is never cleared silently keeps an answer computed before the assembly that would
//   have changed it existed, and the failure is a method that "does not exist" only sometimes,
//   depending on load order.
//
// PRECEDENCE, AND WHY IT IS NOT OVERLOAD RESOLUTION
//   GetExtensionMethod matches by NAME alone, then breaks ties with TypePrecedenceComparer, which
//   ranks candidates by inheritance distance from the target type. Name alone is sufficient because
//   Go has no overloading — a Go type cannot have two methods with one name
//   (https://golang.org/doc/faq#overloading) — so the only reason several candidates appear is that
//   the method is declared on more than one type in the target's hierarchy, and the nearest one
//   wins. Do not "improve" this into signature-based resolution: the structural probe in the
//   sibling file is the place signatures are compared, and it needs the whole candidate set, not
//   one winner.
//
// NAME COLLISION WITH go2cs-gen — NOT THE SAME METHODS
//   go2cs-gen declares its own `GetExtensionMethods` and `GetInterfaceMethods` (in
//   StructDeclarationSyntaxExtensions / InterfaceDeclarationSyntaxExtensions). Those operate on
//   ROSLYN SYNTAX at compile time and have nothing to do with these; a search that lands there
//   while reading runtime behavior is looking at the generator, not the runtime.
// ---------------------------------------------------------------------------------------------
public static partial class TypeExtensions
{
    private static (MethodInfo, Type)[]? s_extensionMethods;
    private static readonly ConcurrentDictionary<Type, MethodInfo[]> s_typeExtensionMethods = [];
    private static readonly ConcurrentDictionary<Type, ImmutableHashSet<string>> s_typeExtensionMethodNames = [];
    private static int s_registeredAssemblyLoadEvent;

    // TEST SEAM, GolibTests only (InternalsVisibleTo): called once per assembly the scan reaches, so a
    // test can hold a scan in flight. Null in every program -- no behaviour and no allocation when
    // unset. Never wire product code to it.
    internal static Action<Assembly>? ScanProbeForTest;

    // Bumped by every assembly load (ClearTypeCaches). A scan keeps only a result whose generation did not
    // move while it ran.
    private static int s_extensionMethodsGeneration;

    // A load storm must not spin the scan forever: after this many generations moved under it, the latest
    // complete scan is returned UNCACHED, and the next caller scans again.
    private const int MaxScanAttempts = 4;

    // NO LOCK, on either side -- and that is the rule this file keeps. The runtime raises AssemblyLoad
    // SYNCHRONOUSLY while holding its own type-load lock, and a scan's Assembly.GetTypes() can wait on
    // that same lock. When the scan held a golib lock across GetTypes() and ClearTypeCaches (the
    // AssemblyLoad handler) took it, the two threads deadlocked: rooted 2026-10-03 from six hung
    // published programs (a ReadyToRun publish pinned to one CPU hung in 20 of 23 runs). So the handler
    // only bumps a generation and nulls the cache; the scan runs outside any lock, publishes with
    // CompareExchange, and retracts its OWN result (never a newer scan's) if a load moved the generation
    // meanwhile. Two concurrent scans may both run; that duplicates work and is harmless.
    //
    // THE RULE FOR EVERY HANDLER: a golib AssemblyLoad / TypeResolve / AssemblyResolve handler must NEVER
    // wait on a lock that any code can hold across a type load.
    internal static (MethodInfo, Type)[] GetExtensionMethods()
    {
        if (Volatile.Read(ref s_extensionMethods) is { } cached)
            return cached;

        // Register assembly load event only once, used to clear extension method caches
        if (Interlocked.CompareExchange(ref s_registeredAssemblyLoadEvent, 1, 0) == 0)
            AppDomain.CurrentDomain.AssemblyLoad += ClearTypeCaches;

        (MethodInfo, Type)[] scanned = [];

        for (int attempt = 0; attempt < MaxScanAttempts; attempt++)
        {
            int generation = Volatile.Read(ref s_extensionMethodsGeneration);
            List<(MethodInfo, Type)> extensionMethods = [];

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                ScanProbeForTest?.Invoke(assembly);
                LoadAssemblyExtensionMethods(assembly, extensionMethods);
            }

            scanned = extensionMethods.ToArray();

            // Publish only into an empty slot: a concurrent scan that already published is as fresh as this one.
            (MethodInfo, Type)[] published = Interlocked.CompareExchange(ref s_extensionMethods, scanned, null) ?? scanned;

            if (Volatile.Read(ref s_extensionMethodsGeneration) == generation)
                return published;

            // An assembly loaded while this scan ran: retract this scan's result (only if it is still the
            // one published) and scan again.
            Interlocked.CompareExchange(ref s_extensionMethods, null, scanned);
        }

        return scanned;
    }

    internal static void ClearTypeCaches(object? sender, EventArgs e)
    {
        // Since not all assemblies may be loaded when initial type caches are created, a load clears the
        // caches so they are recreated. Called by the runtime while it holds its type-load lock, so it
        // takes NO lock of golib's (see GetExtensionMethods).
        Interlocked.Increment(ref s_extensionMethodsGeneration);
        Volatile.Write(ref s_extensionMethods, null);

        s_typeExtensionMethods.Clear();
        s_typeExtensionMethodNames.Clear();
        s_interfaceMethodNames.Clear();
        s_goMethodSetCandidates.Clear();
        s_goMethodSetEntries.Clear();
        s_goInterfaceMethodEntries.Clear();
    }

    private static void LoadAssemblyExtensionMethods(Assembly assembly, List<(MethodInfo, Type)> extensionMethods)
    {
        // A DYNAMIC assembly is one the bridge is BUILDING — go2cs.SynthesizedStructs, where
        // reflect.StructOf mints its synthesized struct types (GoStructSynthesis). Calling
        // GetTypes() on it throws ReflectionTypeLoadException whenever any TypeBuilder inside is
        // still incomplete, and there is no catch on the loop below, so ONE half-built synthesized
        // struct aborts the WHOLE extension scan and every method-set question that depends on it.
        //
        // Measured: 29 reflect tests turned into infrastructure-errors this way, all of them
        // ReflectionTypeLoadException raised out of GetGoMethodSetCandidates, none of them related
        // to what the tests were actually exercising. The class is visible on master too (2 tests),
        // so this is a latent fragility rather than a new one — it simply needs more synthesized
        // structs in flight to bite.
        //
        // Skipping is not a workaround but the correct answer: a dynamic assembly the bridge emits
        // holds synthesized STRUCTS and can contain no Go extension method by construction, so
        // there is nothing here to find and the scan loses no candidate by not looking.
        if (assembly.IsDynamic)
            return;

        string? name = assembly.FullName;

        if (string.IsNullOrEmpty(name))
            return;

        // Ignore extensions methods from the .NET framework
        if (name.StartsWith("System.") || name.StartsWith("netstandard") || name.StartsWith("Microsoft.") || name.StartsWith("WindowsBase") || name.StartsWith("go.golib."))
            return;

        Debug.WriteLine($"Scanning extensions for assembly \"{assembly.FullName}\"...");

        foreach (Type type in assembly.GetTypes())
        foreach (MethodInfo extensionMethod in getExtensionMethods(type))
            extensionMethods.Add((extensionMethod, extensionMethod.GetExtensionTargetType()));
        
        return;

        static IEnumerable<MethodInfo> getExtensionMethods(Type type)
        {
            if (!type.IsSealed || type.IsNested || type.IsGenericType)
                return [];

            return type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(methodInfo => methodInfo.IsDefined(typeof(ExtensionAttribute), false));
        }
    }

    private sealed class TypePrecedenceComparer : Comparer<Type>
    {
        private readonly Type m_targetType;

        public TypePrecedenceComparer(Type targetType)
        {
            m_targetType = targetType;
        }

        public override int Compare(Type? x, Type? y)
        {
            return Comparer<int>.Default.Compare(RelationDistance(x), RelationDistance(y));
        }

        private int RelationDistance(Type? type)
        {
            if (type is null)
                return int.MaxValue;

            int distance = 0;

            while (!IsDirectEquivalent(type))
            {
                type = type.BaseType;
                distance++;

                if (type is null || type == typeof(object))
                {
                    // No direct relation exists
                    distance = int.MaxValue;
                    break;
                }
            }

            return distance;
        }

        private bool IsDirectEquivalent(Type type)
        {
            if (m_targetType.IsInterface)
            {
                if (type.IsInterface)
                    return type.ImplementsInterface(m_targetType) || m_targetType.ImplementsInterface(type);

                foreach (Type interfaceType in type.GetInterfaces())
                {
                    if (interfaceType == m_targetType || interfaceType.ImplementsInterface(m_targetType))
                        return true;
                }

                return false;
            }

            if (!type.IsInterface)
                return type == m_targetType;

            foreach (Type interfaceType in m_targetType.GetInterfaces())
            {
                if (interfaceType == type || interfaceType.ImplementsInterface(type))
                    return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Gets the type of the extension target.
    /// </summary>
    /// <param name="methodInfo">Method info.</param>
    /// <returns>
    /// Type of the extension target, i.e., type of the first parameter.
    /// </returns>
    /// <exception cref="InvalidOperationException">Method has no parameters and cannot be an extension method.</exception>
    public static Type GetExtensionTargetType(this MethodInfo methodInfo)
    {
        ParameterInfo[] parameters = methodInfo.GetParameters();

        if (parameters.Length == 0)
            throw new InvalidOperationException("Method has no parameters and cannot be an extension method.");

        return parameters[0].ParameterType;
    }

    /// <summary>
    /// Finds all the extensions methods for <paramref name="targetType"/>.
    /// </summary>
    /// <param name="targetType">Target <see cref="Type"/> to search.</param>
    /// <returns>Enumeration of reflected method metadata of <paramref name="targetType"/> extension methods.</returns>
    public static MethodInfo[] GetExtensionMethods(this Type targetType)
    {
        return s_typeExtensionMethods.GetOrAdd(targetType, _ =>
        {
            (MethodInfo method, Type type)[] extensionMethods = GetExtensionMethods();

            // Fix W (B1 §3.1): normalize a ж box at any base-chain depth to its ж<X> base BEFORE
            // the generic-match split, so a per-kind subclass instance matches the extension
            // methods registered against the box family (a no-op today — every box type IS its
            // own ж<X>). @unsafe.Pointer is exempt and keeps its assignability route.
            if (!typeof(IUnsafePointer).IsAssignableFrom(targetType) &&
                GoReflect.TryBoxPointee(targetType, out Type? boxPointee) &&
                !(targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(ж<>)))
            {
                targetType = typeof(ж<>).MakeGenericType(boxPointee);
            }

            bool isGenericType = (targetType == typeof(ж<>) ? targetType.GetGenericArguments()[0] : targetType).IsGenericType;

            if (isGenericType)
                targetType = targetType.GetGenericTypeDefinition();

            IEnumerable<MethodInfo> methods = isGenericType ?
                extensionMethods.Where(value => isGenericMatch(value.type)).Select(value => value.method) :
                extensionMethods.Where(value => value.type.IsAssignableFrom(targetType)).Select(value => value.method);

            return methods.ToArray();

            bool isGenericMatch(Type methodType)
            {
                if (methodType.IsGenericType)
                    return methodType.GetGenericTypeDefinition() == targetType;

                return methodType == targetType;
            }
        });
    }

    /// <summary>
    /// Gets all the extension method names for <paramref name="targetType"/>.
    /// </summary>
    /// <param name="targetType">Target <see cref="Type"/> to search.</param>
    /// <returns>A collection of extension method names for <paramref name="targetType"/>.</returns>
    public static ImmutableHashSet<string> GetExtensionMethodNames(this Type targetType)
    {
        return s_typeExtensionMethodNames.GetOrAdd(targetType, _ => [.. targetType.GetExtensionMethods().Select(info => info.Name)]);
    }

    /// <summary>
    /// Determines if an extension method with the specified <paramref name="methodName"/> exists for the <paramref name="targetType"/>.
    /// </summary>
    /// <param name="targetType">Target <see cref="Type"/> to search.</param>
    /// <param name="methodName">Name of extension method to find.</param>
    /// <returns><c>true</c> if extension method exists; otherwise, <c>false</c>.</returns>
    public static bool ExtensionMethodExists(this Type targetType, string methodName)
    {
        // Note that match by function name alone is sufficient as Go does not currently support function overloading by adjusting signature:
        // https://golang.org/doc/faq#overloading
        return targetType.GetExtensionMethods().Any(methodInfo => methodInfo.Name == methodName);
    }

    /// <summary>
    /// Attempts to find the best precedence-wise matching extension method called <paramref name="methodName"/> for the <paramref name="targetType"/>.
    /// </summary>
    /// <param name="targetType">Target <see cref="Type"/> to search.</param>
    /// <param name="methodName">Name of extension method to find.</param>
    /// <returns>Method metadata of extension method, <paramref name="methodName"/>, for <paramref name="targetType"/> if found; otherwise, <c>null</c>.</returns>
    public static MethodInfo? GetExtensionMethod(this Type targetType, string methodName)
    {
        // Note that match by function name alone is sufficient as Go does not currently support function overloading by adjusting signature:
        // https://golang.org/doc/faq#overloading
        return targetType.GetExtensionMethods().Where(methodInfo => methodInfo.Name == methodName).MinBy(GetExtensionTargetType, new TypePrecedenceComparer(targetType));
    }

    /// <summary>
    /// Creates a delegate for the given static method metadata.
    /// </summary>
    /// <param name="methodInfo">Method metadata of extension method.</param>
    /// <param name="delegateType">Specific delegate type to apply; otherwise, defaults to an auto-derived Func or Action delegate.</param>
    /// <returns>Callable delegate referencing extension method in <paramref name="methodInfo"/> or <c>null</c> if specified delegate signature does not match.</returns>
    public static Delegate? CreateStaticDelegate(this MethodInfo methodInfo, Type? delegateType = null)
    {
        if (delegateType is null)
            return methodInfo.CreateStaticDelegate(null!, out bool _);

        try
        {
            // A copy-bound `this ref X` method (IsCopyBoundReceiver) asked for as a BY-VALUE delegate
            // — the generated interface shell's `…ByVal(X target, …)`. No delegate can be created
            // over the by-ref method itself, so a thin lambda takes the receiver by value and calls
            // through: the by-ref parameter gets the lambda's own parameter, which is the copy.
            if (IsCopyBoundReceiver(methodInfo, out Type valueReceiver) && !methodInfo.IsGenericMethod &&
                delegateType.GetMethod(nameof(Action.Invoke))?.GetParameters() is { Length: > 0 } invokeParameters &&
                invokeParameters[0].ParameterType == valueReceiver)
            {
                ParameterExpression[] arguments = [.. invokeParameters.Select(static (p, i) => Expression.Parameter(p.ParameterType, p.Name ?? $"arg{i}"))];

                return Expression.Lambda(delegateType, Expression.Call(methodInfo, arguments), arguments).Compile();
            }

            if (!delegateType.IsGenericType || !methodInfo.IsGenericMethod)
                return Delegate.CreateDelegate(delegateType, methodInfo);

            Type extensionTarget = delegateType.GetGenericArguments()[0];

            return Delegate.CreateDelegate(delegateType, extensionTarget.IsGenericType ?
                methodInfo.MakeGenericMethod(extensionTarget.GetGenericArguments()[0]) :
                methodInfo.MakeGenericMethod(extensionTarget));
        }
        catch (ArgumentException)
        {
            return null!;
        }
    }

    /// <summary>
    /// Creates a delegate for the given static method metadata.
    /// </summary>
    /// <param name="methodInfo">Method metadata of extension method.</param>
    /// <param name="delegateType">Specific delegate type to apply; set to <c>null</c> to use an auto-derived Func or Action delegate.</param>
    /// <param name="isByRef">Determines if extension target is accessed by reference.</param>
    /// <returns>Callable delegate referencing extension method in <paramref name="methodInfo"/> or <c>null</c> if specified delegate signature does not match.</returns>
    public static Delegate? CreateStaticDelegate(this MethodInfo methodInfo, Type? delegateType, out bool isByRef)
    {
        Func<Type[], Type> getMethodType;
        List<Type> types = methodInfo.GetParameters().Select(paramInfo => paramInfo.ParameterType).ToList();

        if (delegateType is null)
        {
            if (methodInfo.ReturnType == typeof(void))
            {
                getMethodType = Expression.GetActionType;
            }
            else
            {
                getMethodType = Expression.GetFuncType;
                types.Add(methodInfo.ReturnType);
            }
        }
        else
        {
            getMethodType = _ => delegateType;
        }

        isByRef = types[0].IsByRef;

        try
        {
            return Delegate.CreateDelegate(getMethodType(types.ToArray()), methodInfo);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }
}
