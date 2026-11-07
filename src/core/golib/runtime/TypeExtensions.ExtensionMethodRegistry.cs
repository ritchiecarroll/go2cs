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
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
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
//   The scan skips the framework (`System.*`, `Microsoft.*`, `netstandard`, `WindowsBase`), golib
//   itself and golib's satellite assemblies, and considers only sealed, non-nested, non-generic
//   static classes — which is exactly the shape the converter emits and a large saving over
//   walking every type in the BCL. It finds those classes in the assembly's METADATA TABLES
//   (ExtensionHostTypes): C# marks a class that declares an extension method with [Extension]
//   itself, so only those classes are ever loaded as types. Assembly.GetTypes() — which loads every
//   type an assembly declares, 16,185 of them for the crypto/tls test host, about 85% of the scan's
//   cold cost — is the fallback for an assembly without raw metadata (Native AOT). The two must find
//   the same methods in the same order; GolibTests' ExtensionScanMetadataTests holds them to it.
//   Between the two sits the BUILD-TIME LIST (ListedExtensionHosts): go2cs-gen writes one
//   [assembly: GoExtensionHost(typeof(X))] per host class and a GoExtensionHostList marker that says the
//   list is complete, and an assembly without raw metadata that carries the marker is read from its
//   list instead of GetTypes(). It is read ONLY there: wherever the metadata walk exists it is exact by
//   construction, and the list would be a second source that can go stale. GolibTests'
//   ExtensionHostListTests holds the list to the walk's methods (docs/phase4/DESIGN-extension-host-list.md).
//   golib is skipped by IDENTITY, not by name: its extension methods
//   are C# helpers (GoShift's Rsh/Lsh on the builtin integers, slice and Must helpers), never Go
//   methods, and Go's predeclared types have empty method sets. The name prefix `go.golib.` alone
//   missed golib's own assembly name, `golib`, so a builtin uint64 "had" Rsh.
//
// A QUESTION READS ONLY THE HOMES IT CAN MATCH
//   The walk finds the host classes and, for each, the HOMES its methods extend: the assembly each
//   receiver belongs to (ExtensionHome), read from the method signatures without loading a type. A
//   question about one type reads only the hosts of the homes a receiver matching it can have
//   (ExtensionHomesOf: the type's, its bases', its interfaces' and the core library's), so start-up
//   pays GetParameters for the asked packages' methods, not for every method of every package. Both
//   consumers then filter exactly as they did over the whole scan, so each answer is the same subset in
//   the same order (GolibTests' ExtensionReceiverScopeTests). The home is the RECEIVER's, never the
//   declaring assembly's: a `-tests` host declares methods on its package's types (whitebox test files),
//   and those stay in that package's answers. Under Native AOT the build-time list carries each host's
//   foreign homes (GoExtensionHost.Homes) under a GoExtensionHostList(HomesListed) marker; a host whose
//   homes are not listed is read at scan time (docs/phase4/DESIGN-extension-receiver-scope.md).
//
// CACHE INVALIDATION IS SHARED WITH THE SIBLING FILE — READ THIS BEFORE ADDING A CACHE
//   Assemblies load LATE. A converted program's package assemblies are not all present when the
//   first type question is asked, so any answer computed from the scan can be stale the moment
//   another assembly arrives. `ClearTypeCaches` is hooked to AppDomain.AssemblyLoad for exactly
//   that reason, and it clears caches DECLARED IN TypeExtensions.GoMethodSets.cs as well as the
//   two declared here. The scan itself is NOT dropped: it records every assembly it has visited,
//   and the next question after a load visits only the assemblies loaded since, appending what they
//   declare. An assembly's extension methods never change after it loads, so re-reading the ones
//   already visited could only find the same methods again; the per-type caches are what a new
//   assembly can make stale, and those are cleared. Adding a cache derived from the scan means
//   adding it to that clear list:
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
    // One class that can declare extension methods, found by the scan's walk, and the HOMES its methods extend: the
    // assemblies their receivers belong to (ExtensionHome). Its methods are read only when a question asks one of
    // those homes, then kept: an assembly's extension methods never change after it loads.
    internal sealed class ExtensionHost
    {
        private readonly Module? m_module;
        private readonly int m_token;
        private Type? m_type;
        private (MethodInfo method, Type receiver, Assembly? home)[]? m_methods;

        // A class found in the metadata tables, loaded as a type only when its methods are read.
        public ExtensionHost(Assembly assembly, Module module, int token, Assembly[] homes, bool anyHome)
        {
            Assembly = assembly;
            m_module = module;
            m_token = token;
            Homes = homes;
            AnyHome = anyHome;
        }

        // A class the scan already holds as a type, with the homes its methods extend; or, given its methods, a class
        // read now.
        public ExtensionHost(Type type, Assembly[] homes, bool anyHome, (MethodInfo, Type, Assembly?)[]? methods = null)
        {
            Assembly = type.Assembly;
            m_type = type;
            Homes = homes;
            AnyHome = anyHome;
            m_methods = methods;
        }

        /// <summary>The assembly that declares the class.</summary>
        public Assembly Assembly { get; }

        /// <summary>The assemblies the class's receivers belong to; a class listed without its methods' homes known
        /// names its own.</summary>
        public Assembly[] Homes { get; }

        /// <summary>Whether a receiver belongs to no assembly (a type parameter, an array): read for every question.</summary>
        public bool AnyHome { get; }

        public Type Type => m_type ??= m_module!.ResolveType(m_token);

        // Whether a question that can match only receivers homed in `homes` must read this class.
        public bool Extends(IReadOnlySet<Assembly> homes)
        {
            if (AnyHome)
                return true;

            foreach (Assembly home in Homes)
            {
                if (homes.Contains(home))
                    return true;
            }

            return false;
        }

        // The class's extension methods, each with its receiver and the receiver's home, in GetMethods order.
        public (MethodInfo method, Type receiver, Assembly? home)[] Methods
        {
            get
            {
                if (Volatile.Read(ref m_methods) is { } methods)
                    return methods;

                Type type = Type;
                HostReadProbeForTest?.Invoke(type);
                methods = ReadExtensionMethods(type);

                // Two threads may read one class; both read the same methods, and the first published is kept.
                return Interlocked.CompareExchange(ref m_methods, methods, null) ?? methods;
            }
        }

        // A class read now, its homes taken from its methods; null when it declares none.
        public static ExtensionHost? Read(Type type)
        {
            (MethodInfo method, Type receiver, Assembly? home)[] methods = ReadExtensionMethods(type);

            if (methods.Length == 0)
                return null;

            Assembly[] homes = [.. methods.Where(static entry => entry.home is not null).Select(static entry => entry.home!).Distinct()];

            return new ExtensionHost(type, homes, methods.Any(static entry => entry.home is null), methods);
        }

        private static (MethodInfo method, Type receiver, Assembly? home)[] ReadExtensionMethods(Type type) =>
        [
            .. DeclaredExtensionMethods(type).Select(static method =>
            {
                Type receiver = method.GetExtensionTargetType();
                return (method, receiver, ExtensionHome(receiver));
            })
        ];
    }

    // The published scan: the extension hosts found, every assembly visited to find them (skipped ones included, so
    // none is visited twice), and the load generation the scan covers.
    private sealed class ExtensionScan(ExtensionHost[] hosts, ImmutableHashSet<Assembly> visited, int generation)
    {
        private (MethodInfo, Type)[]? m_methods;

        public readonly ExtensionHost[] Hosts = hosts;
        public readonly ImmutableHashSet<Assembly> Visited = visited;
        public readonly int Generation = generation;

        // Every extension method of every host, in scan order: what a question no home narrows reads.
        public (MethodInfo, Type)[] Methods
        {
            get
            {
                if (Volatile.Read(ref m_methods) is { } methods)
                    return methods;

                methods = [.. Hosts.SelectMany(static host => host.Methods.Select(static entry => (entry.method, entry.receiver)))];

                return Interlocked.CompareExchange(ref m_methods, methods, null) ?? methods;
            }
        }
    }

    private static ExtensionScan? s_extensionScan;
    private static readonly ConcurrentDictionary<Type, MethodInfo[]> s_typeExtensionMethods = [];
    private static readonly ConcurrentDictionary<Type, ImmutableHashSet<string>> s_typeExtensionMethodNames = [];
    private static int s_registeredAssemblyLoadEvent;

    // TEST SEAM, GolibTests only (InternalsVisibleTo): called once per assembly the scan reaches, so a
    // test can hold a scan in flight. Null in every program -- no behaviour and no allocation when
    // unset. Never wire product code to it.
    internal static Action<Assembly>? ScanProbeForTest;

    // TEST SEAM, GolibTests only: called when the scan reads a host class's methods, so a test can see which classes a
    // question read. Null in every program. Never wire product code to it.
    internal static Action<Type>? HostReadProbeForTest;

    // TEST SEAM, GolibTests only: the assemblies the scan sees in place of AppDomain.GetAssemblies(), so a test
    // can stage an assembly "loading" mid-scan. Null in every program. Never wire product code to it.
    internal static Func<Assembly[]>? AssemblySourceForTest;

    // Bumped by every assembly load (ClearTypeCaches). A published scan covers the generation it read before
    // it looked at the loaded assemblies; when the generation has moved since, the next question visits the
    // assemblies loaded meanwhile.
    private static int s_extensionMethodsGeneration;

    /// <summary>
    /// The type-cache generation: bumped by every assembly load (<see cref="ClearTypeCaches"/>), together with the
    /// clearing of every cache derived from the extension scan. A cache kept OUTSIDE this file records the value it
    /// was computed under and recomputes when it moves, which is how golib's GoMethodText keeps one int compare on its
    /// hot path and still obeys the rule above.
    /// </summary>
    internal static int TypeCacheGeneration => Volatile.Read(ref s_extensionMethodsGeneration);

    // A load storm must not spin the scan forever: after this many generations moved under it, the latest
    // published scan is returned, and the next caller visits what loaded meanwhile.
    private const int MaxScanAttempts = 4;

    /// <summary>
    /// Every extension method of every converted assembly loaded, with its receiver type: the whole scan, which reads
    /// every host class. A question about one type reads only the hosts its homes need
    /// (<see cref="GetExtensionMethodsHomedIn"/>).
    /// </summary>
    internal static (MethodInfo, Type)[] GetExtensionMethods()
    {
        ExtensionScan scan = WalkExtensionHosts();

        // Reading a host's methods resolves their parameter types, which can load an assembly: read again from a
        // walk that visited it.
        for (int attempt = 1; attempt < MaxScanAttempts; attempt++)
        {
            int generation = scan.Generation;
            (MethodInfo, Type)[] methods = scan.Methods;

            if (Volatile.Read(ref s_extensionMethodsGeneration) == generation)
                return methods;

            scan = WalkExtensionHosts();
        }

        return scan.Methods;
    }

    /// <summary>
    /// The extension methods whose receiver belongs to one of <paramref name="homes"/> (<see cref="ExtensionHome"/>)
    /// or to no assembly, with their receiver types, in scan order: the subset of <see cref="GetExtensionMethods()"/>
    /// a question about a type whose homes are <paramref name="homes"/> can match (<see cref="ExtensionHomesOf"/>),
    /// read without reading any other host class.
    /// </summary>
    internal static IReadOnlyList<(MethodInfo method, Type receiver)> GetExtensionMethodsHomedIn(IReadOnlySet<Assembly> homes)
    {
        List<(MethodInfo method, Type receiver)> found = [];
        ExtensionScan scan = WalkExtensionHosts();

        for (int attempt = 1; ; attempt++)
        {
            foreach (ExtensionHost host in scan.Hosts)
            {
                if (!host.Extends(homes))
                    continue;

                foreach ((MethodInfo method, Type receiver, Assembly? home) in host.Methods)
                {
                    if (home is null || homes.Contains(home))
                        found.Add((method, receiver));
                }
            }

            // Reading a host's methods can load an assembly, as above.
            if (attempt == MaxScanAttempts || Volatile.Read(ref s_extensionMethodsGeneration) == scan.Generation)
                return found;

            scan = WalkExtensionHosts();
            found.Clear();
        }
    }

    // NO LOCK, on either side -- and that is the rule this file keeps. The runtime raises AssemblyLoad
    // SYNCHRONOUSLY while holding its own type-load lock, and a scan's Assembly.GetTypes() can wait on
    // that same lock. When the scan held a golib lock across GetTypes() and ClearTypeCaches (the
    // AssemblyLoad handler) took it, the two threads deadlocked: rooted 2026-10-03 from six hung
    // published programs (a ReadyToRun publish pinned to one CPU hung in 20 of 23 runs). So the handler
    // only bumps a generation and clears the per-type caches; the scan runs outside any lock and
    // publishes with CompareExchange, only over the scan it extended (a scan that lost that race extends
    // the winner on its next attempt). Two concurrent scans may both run; that duplicates work and is
    // harmless. A host's methods are read the same way (ExtensionHost.Methods).
    //
    // THE RULE FOR EVERY HANDLER: a golib AssemblyLoad / TypeResolve / AssemblyResolve handler must NEVER
    // wait on a lock that any code can hold across a type load.
    private static ExtensionScan WalkExtensionHosts()
    {
        if (Volatile.Read(ref s_extensionScan) is { } cached &&
            cached.Generation == Volatile.Read(ref s_extensionMethodsGeneration))
            return cached;

        // Register assembly load event only once, used to clear the per-type caches and move the generation
        if (Interlocked.CompareExchange(ref s_registeredAssemblyLoadEvent, 1, 0) == 0)
            AppDomain.CurrentDomain.AssemblyLoad += ClearTypeCaches;

        ExtensionScan? latest = null;

        for (int attempt = 0; attempt < MaxScanAttempts; attempt++)
        {
            int generation = Volatile.Read(ref s_extensionMethodsGeneration);
            ExtensionScan? extended = Volatile.Read(ref s_extensionScan);

            if (extended is not null && extended.Generation == generation)
                return extended;

            ImmutableHashSet<Assembly> visited = extended?.Visited ?? [];
            ImmutableHashSet<Assembly>.Builder? nowVisited = null;
            List<ExtensionHost> found = [];

            foreach (Assembly assembly in AssemblySourceForTest?.Invoke() ?? AppDomain.CurrentDomain.GetAssemblies())
            {
                if (visited.Contains(assembly))
                    continue;

                ScanProbeForTest?.Invoke(assembly);
                LoadAssemblyExtensionHosts(assembly, found);
                (nowVisited ??= visited.ToBuilder()).Add(assembly);
            }

            ExtensionHost[] hosts = extended?.Hosts ?? [];

            if (found.Count > 0)
                hosts = [.. hosts, .. found];

            latest = new ExtensionScan(hosts, nowVisited?.ToImmutable() ?? visited, generation);

            // Publish only over the scan this one extended: one that another caller published meanwhile is
            // extended on the next attempt instead.
            if (Interlocked.CompareExchange(ref s_extensionScan, latest, extended) != extended)
                continue;

            if (Volatile.Read(ref s_extensionMethodsGeneration) == generation)
                return latest;

            // An assembly loaded while this scan ran: the next attempt visits only what loaded meanwhile.
        }

        return latest!;
    }

    // TEST SEAM, GolibTests only: forget the published scan, so the next question scans from nothing.
    internal static void ResetExtensionScanForTest()
    {
        Volatile.Write(ref s_extensionScan, null);
        ClearTypeCaches(null, EventArgs.Empty);
    }

    internal static void ClearTypeCaches(object? sender, EventArgs e)
    {
        // Since not all assemblies may be loaded when initial type caches are created, a load clears the
        // caches so they are recreated, and moves the generation so the next question visits the new
        // assembly. Called by the runtime while it holds its type-load lock, so it takes NO lock of golib's
        // and does no scanning (see WalkExtensionHosts).
        Interlocked.Increment(ref s_extensionMethodsGeneration);

        s_typeExtensionMethods.Clear();
        s_typeExtensionMethodNames.Clear();
        s_interfaceMethodNames.Clear();
        s_goMethodSetCandidates.Clear();
        s_goMethodSetEntries.Clear();
        s_goInterfaceMethodEntries.Clear();
    }

    private static void LoadAssemblyExtensionHosts(Assembly assembly, List<ExtensionHost> hosts)
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

        // Ignore extension methods from the .NET framework, and golib's own: they are C# helpers, not Go methods
        if (assembly == typeof(TypeExtensions).Assembly)
            return;

        if (name.StartsWith("System.") || name.StartsWith("netstandard") || name.StartsWith("Microsoft.") || name.StartsWith("WindowsBase") || name.StartsWith("go.golib."))
            return;

        Debug.WriteLine($"Scanning extensions for assembly \"{assembly.FullName}\"...");

        if ((WalkedExtensionHosts(assembly) ?? ListedExtensionHostEntries(assembly)) is { } known)
        {
            hosts.AddRange(known);
            return;
        }

        foreach (Type type in assembly.GetTypes())
        {
            if (ExtensionHost.Read(type) is { } host)
                hosts.Add(host);
        }
    }

    // The extension methods a type declares, in the shape the converter emits them: a sealed, non-nested,
    // non-generic (static) class.
    internal static IEnumerable<MethodInfo> DeclaredExtensionMethods(Type type)
    {
        if (!type.IsSealed || type.IsNested || type.IsGenericType)
            return [];

        return type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(methodInfo => methodInfo.IsDefined(typeof(ExtensionAttribute), false));
    }

    /// <summary>
    /// The assembly an extension method's receiver belongs to, its HOME: the assembly of the receiver type, read
    /// through a by-ref and through golib's <c>ж&lt;X&gt;</c> box to <c>X</c>, and of a generic type's definition.
    /// A receiver no assembly holds — a type parameter, an array, a pointer — has none (<c>null</c>), and is read for
    /// every question.
    /// </summary>
    /// <remarks>
    /// Scoping by the RECEIVER's home, never by the declaring assembly, is what keeps a <c>-tests</c> host's methods
    /// on its package's types (a whitebox test's <c>Generate</c> on <c>clientHelloMsg</c>) in that package's answers.
    /// </remarks>
    internal static Assembly? ExtensionHome(Type receiver)
    {
        if (receiver.IsByRef)
            receiver = receiver.GetElementType()!;

        while (receiver.IsGenericType && receiver.GetGenericTypeDefinition() == typeof(ж<>))
            receiver = receiver.GetGenericArguments()[0];

        if (receiver.IsGenericParameter || receiver.HasElementType || receiver.IsFunctionPointer)
            return null;

        return receiver.Assembly;
    }

    /// <summary>
    /// The homes a question about <paramref name="type"/> can match: those of the type, of each type it derives from
    /// and of each interface it implements, and the core library's (<c>object</c>, which every type converts to). An
    /// extension method's receiver matches the type only if it is the type, shares its generic definition, or is
    /// assignable from it, and every such receiver's <see cref="ExtensionHome"/> is among these.
    /// </summary>
    internal static HashSet<Assembly> ExtensionHomesOf(Type type)
    {
        HashSet<Assembly> homes = [typeof(object).Assembly];

        for (Type? current = type; current is not null; current = current.BaseType)
            add(current);

        foreach (Type interfaceType in type.GetInterfaces())
            add(interfaceType);

        return homes;

        void add(Type candidate)
        {
            if (ExtensionHome(candidate) is { } home)
                homes.Add(home);
        }
    }

    /// <summary>
    /// The extension hosts of <paramref name="assembly"/>, read from its metadata tables without loading any type: the
    /// sealed, non-nested, non-generic classes that carry <see cref="ExtensionAttribute"/>, which C# puts on every
    /// class that declares an extension method, in metadata order (the order <see cref="Assembly.GetTypes"/> returns
    /// them in), each with the homes its methods extend.
    /// </summary>
    /// <returns>
    /// The hosts, or <c>null</c> when the assembly has no raw metadata (Native AOT); the caller then reads the
    /// build-time list (<see cref="ListedExtensionHostEntries"/>), or <see cref="Assembly.GetTypes"/>.
    /// </returns>
    /// <remarks>
    /// Each extension method's receiver is read from the first parameter of its signature. One the assembly defines
    /// (read through a by-ref, golib's <c>ж&lt;X&gt;</c> and a generic instantiation) is homed here with no type load.
    /// Any other is resolved through the runtime's binder, which follows type forwarders, and homed where its type
    /// loads from. Only a <c>-tests</c> host's methods on its package's types take that path.
    /// </remarks>
    internal static unsafe IReadOnlyList<ExtensionHost>? WalkedExtensionHosts(Assembly assembly)
    {
        if (!assembly.TryGetRawMetadata(out byte* blob, out int length))
            return null;

        MetadataReader reader = new(blob, length);
        Module module = assembly.ManifestModule;
        SignatureDecoder<ReceiverShape, object?> decoder = new(ReceiverShapeProvider.Instance, reader, null);
        List<ExtensionHost> hosts = [];
        List<Assembly> homes = [];

        foreach (TypeDefinitionHandle handle in reader.TypeDefinitions)
        {
            TypeDefinition definition = reader.GetTypeDefinition(handle);

            if ((definition.Attributes & TypeAttributes.Sealed) == 0 ||
                !definition.GetDeclaringType().IsNil ||
                definition.GetGenericParameters().Count != 0 ||
                !CarriesExtensionAttribute(reader, definition.GetCustomAttributes()))
                continue;

            bool anyHome = false;
            homes.Clear();

            foreach (MethodDefinitionHandle methodHandle in definition.GetMethods())
            {
                MethodDefinition method = reader.GetMethodDefinition(methodHandle);

                if ((method.Attributes & MethodAttributes.Static) == 0 || !CarriesExtensionAttribute(reader, method.GetCustomAttributes()))
                    continue;

                BlobReader signature = reader.GetBlobReader(method.Signature);

                if (signature.ReadSignatureHeader().IsGeneric)
                    signature.ReadCompressedInteger();

                // No receiver: not a method C# writes. Reading the class for every question leaves it to fail there as
                // it did before the walk read homes (GetExtensionTargetType).
                if (signature.ReadCompressedInteger() == 0)
                {
                    anyHome = true;
                    continue;
                }

                decoder.DecodeType(ref signature); // the return type

                if (decoder.DecodeType(ref signature) == ReceiverShape.Defined)
                {
                    addHome(assembly);
                    continue;
                }

                Type receiver = ((MethodInfo)module.ResolveMethod(MetadataTokens.GetToken(methodHandle))!).GetExtensionTargetType();

                if (ExtensionHome(receiver) is { } home)
                    addHome(home);
                else
                    anyHome = true;
            }

            hosts.Add(new ExtensionHost(assembly, module, MetadataTokens.GetToken(handle), [.. homes], anyHome));
        }

        return hosts;

        void addHome(Assembly home)
        {
            if (!homes.Contains(home))
                homes.Add(home);
        }
    }

    /// <summary>
    /// The types of <paramref name="assembly"/> that can declare an extension method, from its metadata tables
    /// (<see cref="WalkedExtensionHosts"/>).
    /// </summary>
    /// <returns>
    /// The types, or <c>null</c> when the assembly has no raw metadata (Native AOT).
    /// </returns>
    internal static IReadOnlyList<Type>? ExtensionHostTypes(Assembly assembly) =>
        WalkedExtensionHosts(assembly)?.Select(static host => host.Type).ToArray();

    // Whether a metadata attribute list holds C#'s ExtensionAttribute.
    private static bool CarriesExtensionAttribute(MetadataReader reader, CustomAttributeHandleCollection attributes)
    {
        foreach (CustomAttributeHandle attributeHandle in attributes)
        {
            EntityHandle constructor = reader.GetCustomAttribute(attributeHandle).Constructor;

            // ExtensionAttribute lives in the core library, so another assembly reaches its constructor
            // through a member reference on a type reference. Only the core library itself defines it.
            EntityHandle parent = constructor.Kind switch
            {
                HandleKind.MemberReference =>
                    reader.GetMemberReference((MemberReferenceHandle)constructor).Parent,
                HandleKind.MethodDefinition =>
                    reader.GetMethodDefinition((MethodDefinitionHandle)constructor).GetDeclaringType(),
                _ => default
            };

            StringHandle name, space;

            switch (parent.Kind)
            {
                case HandleKind.TypeReference:
                    TypeReference reference = reader.GetTypeReference((TypeReferenceHandle)parent);
                    (name, space) = (reference.Name, reference.Namespace);
                    break;
                case HandleKind.TypeDefinition:
                    TypeDefinition local = reader.GetTypeDefinition((TypeDefinitionHandle)parent);
                    (name, space) = (local.Name, local.Namespace);
                    break;
                default:
                    continue;
            }

            if (reader.StringComparer.Equals(name, nameof(ExtensionAttribute)) &&
                reader.StringComparer.Equals(space, typeof(ExtensionAttribute).Namespace!))
                return true;
        }

        return false;
    }

    // What the metadata walk needs to know of a receiver's signature type: whether the assembly itself defines it.
    private enum ReceiverShape : byte
    {
        Other,
        Defined,
        Box // golib's ж<T>, before its instantiation is read through
    }

    // Reads a signature type as a ReceiverShape, loading no type: by-refs, modifiers and golib's ж<X> are read
    // through, and a generic instantiation is its definition.
    private sealed class ReceiverShapeProvider : ISignatureTypeProvider<ReceiverShape, object?>
    {
        public static readonly ReceiverShapeProvider Instance = new();

        public ReceiverShape GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind) => ReceiverShape.Defined;

        public ReceiverShape GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind)
        {
            TypeReference reference = reader.GetTypeReference(handle);

            return reference.ResolutionScope.Kind == HandleKind.AssemblyReference &&
                   reader.StringComparer.Equals(reference.Name, "ж`1") &&
                   reader.StringComparer.Equals(reference.Namespace, "go") &&
                   reader.StringComparer.Equals(reader.GetAssemblyReference((AssemblyReferenceHandle)reference.ResolutionScope).Name, "golib")
                ? ReceiverShape.Box
                : ReceiverShape.Other;
        }

        public ReceiverShape GetGenericInstantiation(ReceiverShape genericType, ImmutableArray<ReceiverShape> typeArguments) =>
            genericType == ReceiverShape.Box && typeArguments.Length == 1 ? typeArguments[0] :
            genericType == ReceiverShape.Defined ? ReceiverShape.Defined : ReceiverShape.Other;

        public ReceiverShape GetByReferenceType(ReceiverShape elementType) => elementType;

        public ReceiverShape GetModifiedType(ReceiverShape modifier, ReceiverShape unmodifiedType, bool isRequired) => unmodifiedType;

        public ReceiverShape GetPinnedType(ReceiverShape elementType) => elementType;

        public ReceiverShape GetTypeFromSpecification(MetadataReader reader, object? genericContext, TypeSpecificationHandle handle, byte rawTypeKind) => ReceiverShape.Other;

        public ReceiverShape GetPrimitiveType(PrimitiveTypeCode typeCode) => ReceiverShape.Other;

        public ReceiverShape GetSZArrayType(ReceiverShape elementType) => ReceiverShape.Other;

        public ReceiverShape GetArrayType(ReceiverShape elementType, ArrayShape shape) => ReceiverShape.Other;

        public ReceiverShape GetPointerType(ReceiverShape elementType) => ReceiverShape.Other;

        public ReceiverShape GetFunctionPointerType(MethodSignature<ReceiverShape> signature) => ReceiverShape.Other;

        public ReceiverShape GetGenericMethodParameter(object? genericContext, int index) => ReceiverShape.Other;

        public ReceiverShape GetGenericTypeParameter(object? genericContext, int index) => ReceiverShape.Other;
    }

    /// <summary>
    /// The classes of <paramref name="assembly"/> that go2cs-gen listed at build time as declaring extension methods,
    /// each once, in the order first listed.
    /// </summary>
    /// <returns>
    /// The classes, or <c>null</c> when the assembly does not carry <see cref="GoExtensionHostListAttribute"/> and so
    /// has no complete list; the caller then falls back to <see cref="Assembly.GetTypes"/>.
    /// </returns>
    internal static IReadOnlyList<Type>? ListedExtensionHosts(Assembly assembly)
    {
        if (!assembly.IsDefined(typeof(GoExtensionHostListAttribute), false))
            return null;

        List<Type> hosts = [];
        HashSet<Type> listed = [];

        foreach (GoExtensionHostAttribute attribute in assembly.GetCustomAttributes<GoExtensionHostAttribute>())
        {
            if (listed.Add(attribute.Host))
                hosts.Add(attribute.Host);
        }

        return hosts;
    }

    /// <summary>
    /// The build-time list's hosts (<see cref="ListedExtensionHosts"/>), each with the homes its methods extend: its
    /// own assembly and the homes its listing names. A class whose homes the list does not give — the whole list from
    /// an older generator, or a class with a receiver no assembly holds — is read now, and its homes taken from its
    /// methods.
    /// </summary>
    /// <returns>
    /// The hosts, or <c>null</c> when the assembly carries no complete list.
    /// </returns>
    internal static IReadOnlyList<ExtensionHost>? ListedExtensionHostEntries(Assembly assembly)
    {
        if (assembly.GetCustomAttribute<GoExtensionHostListAttribute>() is not { } list)
            return null;

        List<ExtensionHost> hosts = [];
        HashSet<Type> listed = [];

        foreach (GoExtensionHostAttribute attribute in assembly.GetCustomAttributes<GoExtensionHostAttribute>())
        {
            if (!listed.Add(attribute.Host))
                continue;

            if (list.HomesListed && !attribute.AnyHome)
            {
                Assembly[] homes = [assembly, .. attribute.Homes.Select(static type => type.Assembly).Where(home => home != assembly).Distinct()];
                hosts.Add(new ExtensionHost(attribute.Host, homes, false));
            }
            else if (ExtensionHost.Read(attribute.Host) is { } read)
            {
                hosts.Add(read);
            }
        }

        return hosts;
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
        return s_typeExtensionMethods.GetOrAdd(targetType, static targetType => ExtensionMethodsOf(targetType, null));
    }

    /// <summary>
    /// The extension methods of <paramref name="targetType"/> (<see cref="GetExtensionMethods(Type)"/>), uncached, read
    /// from <paramref name="source"/>, or, when it is <c>null</c>, from the hosts of the homes the target can match.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("AOT", "IL3050", Justification = global::go.GoTypeRegistry.AotReferenceTypeJustification)]
    internal static MethodInfo[] ExtensionMethodsOf(Type targetType, IReadOnlyList<(MethodInfo, Type)>? source)
    {
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

        // A box's definition matches every box receiver, whatever its pointee's home: that question reads the
        // whole scan.
        IEnumerable<(MethodInfo method, Type type)> extensionMethods =
            source ?? (targetType == typeof(ж<>) ? GetExtensionMethods() : GetExtensionMethodsHomedIn(ExtensionHomesOf(targetType)));

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

            // A generic type's method arrives CLOSED over the receiver's type arguments: the method-set candidate
            // source closes it (CloseGoMethod), and the generated shells bind what AdapterBinder resolves from there.
            // An open method has no delegate, which CreateDelegate reports as the ArgumentException caught below. (This
            // once closed an open method over the receiver's FIRST type argument, right for one type parameter and
            // wrong for two: `Pair[string, int]` never bound fmt.Stringer.)
            return Delegate.CreateDelegate(delegateType, methodInfo);
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
    [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("AOT", "IL3050", Justification = global::go.GoTypeRegistry.AotNotReachedJustification)]
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
