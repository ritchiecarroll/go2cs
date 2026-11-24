<#
.SYNOPSIS
    Generates the binary-compatibility suppressions of four kinds of announced break: a public method that left a
    package class for a class marked [GoPromotedHost(typeof(S))] or [GoMethodHost(typeof(S))], unchanged in every
    other respect; a name that dropped go2cs's Delta prefix (OQ2), unchanged in every other respect; a struct
    that no longer needs a value clone, which loses go.IGoValueClone and its Clone() and nothing else; and a variadic
    parameter whose type went from System.Span<T> to golib's go.variadic<T>, unchanged in every other respect.

.DESCRIPTION
    1.26.11.1 moved every struct's same-package promoted-method forwarders out of the package class into a class of
    the struct's own (docs/phase4/DESIGN-promoted-forwarder-placement.md), which go2cs-gen marks GoPromotedHost, and
    every method a package declares into its receiver type's method holder (OQ2, docs/phase4/SCOUT-method-holder.md),
    which the converter marks GoMethodHost. A compiled consumer binds a forwarder by
    its declaring class, so each moved public forwarder is a CP0002 the release must announce, one entry per asset.
    They are too many to write by hand, and a hand-written list would also hide any member that left for another
    reason. This module writes the entries from the two assemblies themselves:

      a member is REMOVED when the baseline's public surface has its documentation id and the current one has not;
      a removed method is a MOVE when the current assembly holds a class marked [go.GoPromotedHost(typeof(S))] or
      [go.GoMethodHost(typeof(S))], S being the type the method's first parameter takes (S, or go.zh<S> for the pointer form), and that class
      declares a method of the same name, signature and attributes.

    Nullable annotations are compared DECODED. The compiler writes one NullableContext on a class, the value most
    of its members share, and repeats the attribute only on a member that differs; so the attributes a method
    itself carries depend on the other members of its class, and a method that moves between classes changes them
    while its annotations stay what they were. Each position of a signature is therefore read as its own Nullable
    attribute or, without one, the context of the nearest enclosing method, class or module, and a position whose
    type takes no annotation is read as none. A method that moved AND changed an annotation is refused.

    THE DELTA DROP is the second kind. go2cs prefixed U+0394 (Delta) to a type, constant or variable whose Go name a
    method of its package shared, because both were members of the package class; with the methods in their holders
    (OQ2) the prefix is gone. A removed type or member is a RENAME only when its documentation id with every Delta
    dropped is in the current surface exactly, or, for a method, when it is a move once its receiver, name and
    signature drop their Delta and the result equals the current method's signature exactly. The same one-way test
    gives the drop's other diagnostics (RenamedShapes): a member whose return or value type was renamed, an interface
    member added under the Go-named id, a type that implements a renamed interface, a renamed constraint. Each rename
    is the diagnostic the engine reports for it, with its target in the engine's form.

    THE CLONE DROP is the third kind. A struct whose C# value copy is incomplete on its own (a fixed-size array field,
    directly or through another such struct) implements go.IGoValueClone and has go2cs-gen's Delta-named Clone(). A
    zero-length array holds nothing to copy, so from 1.26.11.1 a struct whose only such field is one (syscall's
    InotifyEvent ends Name [0]uint8) has neither. A struct that lost exactly that interface (and ICloneable, which it
    extends) and that method, and nothing else, gets the two diagnostics the engine reports: CP0008 for the type and
    CP0002 for the method.

    THE VARIADIC TAIL is the fourth kind (wall E, inbox/P2/20261120T121230Z-COORD.md; the surface ruling,
    inbox/P2/20261120T135029Z-COORD.md). A Go variadic parameter was a params System.Span<T>, which a spread f(s...)
    could fill only with a copy of the slice; it is now a params go.variadic<T>, a pack that carries the caller's
    slice itself, as Go hands it over. A removed method is that change when its type declares, under the same name, a
    method whose return type, type parameters and every other parameter are the same, and whose one changed parameter
    went from System.Span<X> to go.variadic<X>, X unchanged, and is params (the one waiver is a delegate's
    BeginInvoke, which repeats the tail without the mark just before its callback and state; Invoke shows the mark):
    CP0002 for the old method, and CP0006 for the new one where an interface declares it; the new method must be one
    the baseline lacks. A get-only property whose type went from System.Span<X> to go.variadic<X> under the same id
    (the spread property of golib's slice<T> and of a named slice type) is a CP0002 by its getter, and a CP0006 too
    where an interface declares it; a get-only property an interface ADDS that hides a base interface's spread
    property of the baseline, with that change and nothing else (golib's ISlice<T> over IArray<T>), is a CP0006.
    A METHOD whose return type made that change is refused (P3 145156Z, COORD
    inbox/P2/20261120T145250Z-COORD.md).

    Only a move, a rename, a clone drop or a variadic tail gets an entry. Every other removed or changed member is returned as REFUSED with its
    reason, and the guard (GoApiCompat.psm1) still reads it red: a break nobody announced.

    The surface is read with the Roslyn that PowerShell 7 ships for Add-Type, so the documentation ids are the ones
    the ApiCompat engine prints. Windows PowerShell 5.1 has no Roslyn: Test-GoApiCompatMovesAvailable says so, and
    the generator is a pwsh tool. The guard, not this module, decides a release; it runs under either shell.
#>
#Requires -Version 5.1
Set-StrictMode -Version 2

Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem

$script:AssetPattern = '^(lib/[^/]+|runtimes/[^/]+/lib/[^/]+)/[^/]+\.dll$'

$script:FinderSource = @'
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Srm = System.Reflection.Metadata;

namespace Go2cs.ApiCompatMoves
{
    public sealed class Move
    {
        public string Target;
        public string Host;
    }

    public sealed class Refusal
    {
        public string Target;
        public string Reason;
    }

    // One entry of the Delta drop: the diagnostic the engine reports, its target, and the documentation id the member
    // or type has now.
    public sealed class Rename
    {
        public string DiagnosticId;
        public string Target;
        public string Now;
    }

    public sealed class Result
    {
        public List<Move> Moves = new List<Move>();
        public List<Rename> Renames = new List<Rename>();
        public List<Rename> Clones = new List<Rename>();
        public List<Rename> Variadics = new List<Rename>();
        public List<Refusal> Refused = new List<Refusal>();
    }

    public static class Finder
    {
        // The two marks of a class a method may move into: a struct's promoted-method forwarders (go2cs-gen) and a
        // receiver type's method holder (the converter). Both take the type as typeof, stored as its name.
        private static readonly string[] HostAttributes = { "go.GoPromotedHostAttribute", "go.GoMethodHostAttribute" };

        // The prefix (U+0394, Delta) go2cs gave a type, constant or variable whose Go name a method of its package
        // shared (ShadowVarMarker in the converter). With the methods in their holders the names no longer meet, and
        // the prefix is gone.
        private const string ShadowMarker = "\u0394";

        private const string NullableAttribute = "System.Runtime.CompilerServices.NullableAttribute";
        private const string NullableContextAttribute = "System.Runtime.CompilerServices.NullableContextAttribute";

        // Types are written without their nullable annotations: Nullability reads those from the tables.
        private static readonly SymbolDisplayFormat s_type = SymbolDisplayFormat.FullyQualifiedFormat;

        private static readonly Annotated s_annotated = new Annotated();

        // An assembly's metadata tables, beside the symbols Load makes of the same file.
        private sealed class Tables : IDisposable
        {
            private readonly FileStream m_stream;
            private readonly PEReader m_image;

            public readonly Srm.MetadataReader Reader;

            public Tables(string path)
            {
                m_stream = File.OpenRead(path);
                m_image = new PEReader(m_stream);
                Reader = Srm.PEReaderExtensions.GetMetadataReader(m_image);
            }

            public void Dispose()
            {
                m_image.Dispose();
                m_stream.Dispose();
            }
        }

        public static Result Find(string baselinePath, string currentPath)
        {
            IAssemblySymbol baseline = Load(baselinePath), current = Load(currentPath);

            using (Tables baselineTables = new Tables(baselinePath))
            using (Tables currentTables = new Tables(currentPath))
            {
                Dictionary<string, ISymbol> before = Surface(baseline), after = Surface(current);
                Dictionary<string, List<INamedTypeSymbol>> hosts = Hosts(currentTables.Reader, current);
                Result result = new Result();

                foreach (KeyValuePair<string, ISymbol> entry in before.OrderBy(pair => pair.Key, StringComparer.Ordinal))
                {
                    if (after.ContainsKey(entry.Key))
                        continue;

                    // The engine reports a removed type once and none of its members.
                    if (entry.Value.ContainingType != null && !after.ContainsKey(Id(entry.Value.ContainingType)))
                        continue;

                    string host, reason, now;

                    if (CloneMethodDropped(entry.Value, after))
                    {
                        result.Clones.Add(new Rename { DiagnosticId = "CP0002", Target = entry.Key, Now = Id(entry.Value.ContainingType) });
                        continue;
                    }

                    if (Moved(entry.Value, hosts, baselineTables.Reader, currentTables.Reader, out host, out reason))
                        result.Moves.Add(new Move { Target = entry.Key, Host = host });
                    else if (Renamed(entry.Key, entry.Value, after, hosts, baselineTables.Reader, currentTables.Reader, out now))
                    {
                        ISymbol successor;
                        IPropertySymbol property = entry.Value as IPropertySymbol;
                        IPropertySymbol renamed = after.TryGetValue(now, out successor) ? successor as IPropertySymbol : null;

                        // The engine names a removed property by its accessors.
                        if (property != null && renamed != null)
                        {
                            if (property.GetMethod != null)
                                result.Renames.Add(new Rename { DiagnosticId = "CP0002", Target = Id(property.GetMethod), Now = renamed.GetMethod == null ? "" : Id(renamed.GetMethod) });

                            if (property.SetMethod != null)
                                result.Renames.Add(new Rename { DiagnosticId = "CP0002", Target = Id(property.SetMethod), Now = renamed.SetMethod == null ? "" : Id(renamed.SetMethod) });
                        }
                        else
                            result.Renames.Add(new Rename { DiagnosticId = entry.Value is INamedTypeSymbol ? "CP0001" : "CP0002", Target = entry.Key, Now = now });
                    }
                    else if (VariadicTail(entry.Value, before, after, out now))
                    {
                        result.Variadics.Add(new Rename { DiagnosticId = "CP0002", Target = entry.Key, Now = now });

                        if (entry.Value.ContainingType.TypeKind == TypeKind.Interface)
                            result.Variadics.Add(new Rename { DiagnosticId = "CP0006", Target = now, Now = now });
                    }
                    else
                        result.Refused.Add(new Refusal { Target = entry.Key, Reason = reason });
                }

                RenamedShapes(before, after, result);

                return result;
            }
        }

        private static IAssemblySymbol Load(string path)
        {
            MetadataReference reference = MetadataReference.CreateFromImage(File.ReadAllBytes(path));
            CSharpCompilation compilation = CSharpCompilation.Create("moves", references: new[] { reference });
            IAssemblySymbol assembly = compilation.GetAssemblyOrModuleSymbol(reference) as IAssemblySymbol;

            if (assembly == null)
                throw new InvalidOperationException(path + " is not an assembly");

            return assembly;
        }

        private static string Id(ISymbol symbol)
        {
            return symbol.GetDocumentationCommentId() ?? "";
        }

        private static bool Visible(ISymbol symbol)
        {
            for (ISymbol each = symbol; each != null && !(each is INamespaceSymbol); each = each.ContainingSymbol)
            {
                Accessibility access = each.DeclaredAccessibility;

                if (access != Accessibility.Public && access != Accessibility.Protected && access != Accessibility.ProtectedOrInternal)
                    return false;
            }

            return true;
        }

        private static IEnumerable<INamedTypeSymbol> Types(INamespaceOrTypeSymbol container)
        {
            foreach (ISymbol member in container.GetMembers())
            {
                INamespaceSymbol space = member as INamespaceSymbol;
                INamedTypeSymbol type = member as INamedTypeSymbol;

                if (space != null)
                {
                    foreach (INamedTypeSymbol nested in Types(space))
                        yield return nested;
                }
                else if (type != null)
                {
                    yield return type;

                    foreach (INamedTypeSymbol nested in Types(type))
                        yield return nested;
                }
            }
        }

        // Every type and member visible outside the assembly, by documentation id.
        private static Dictionary<string, ISymbol> Surface(IAssemblySymbol assembly)
        {
            Dictionary<string, ISymbol> surface = new Dictionary<string, ISymbol>(StringComparer.Ordinal);

            foreach (INamedTypeSymbol type in Types(assembly.GlobalNamespace))
            {
                if (!Visible(type))
                    continue;

                surface[Id(type)] = type;

                foreach (ISymbol member in type.GetMembers())
                {
                    if (member is INamedTypeSymbol || member.IsImplicitlyDeclared || !Visible(member))
                        continue;

                    IMethodSymbol method = member as IMethodSymbol;

                    // An accessor is reported through its property or event.
                    if (method != null && method.AssociatedSymbol != null)
                        continue;

                    surface[Id(member)] = member;
                }
            }

            return surface;
        }

        private static string MetadataName(INamedTypeSymbol type)
        {
            string name = type.MetadataName;

            if (type.ContainingType != null)
                return MetadataName(type.ContainingType) + "+" + name;

            string space = Namespace(type.ContainingNamespace);

            return space.Length == 0 ? name : space + "." + name;
        }

        // A namespace as metadata writes it. Not ToDisplayString: that escapes a segment that is a C# keyword
        // (go.@internal.abi), and the name an attribute stores is not escaped.
        private static string Namespace(INamespaceSymbol space)
        {
            if (space == null || space.IsGlobalNamespace)
                return "";

            string outer = Namespace(space.ContainingNamespace);

            return outer.Length == 0 ? space.MetadataName : outer + "." + space.MetadataName;
        }

        // The classes marked [go.GoPromotedHost(typeof(S))] or [go.GoMethodHost(typeof(S))], by S. The attribute is read from the metadata tables and
        // not through the compilation: each assembly is loaded alone, as the guard compares it, so neither the
        // attribute's class (go.lib's) nor System.Type resolves, and the compiler decodes no argument of such an
        // attribute. The argument of typeof is stored as the type's name, which is all that is needed here.
        private static Dictionary<string, List<INamedTypeSymbol>> Hosts(Srm.MetadataReader reader, IAssemblySymbol assembly)
        {
            Dictionary<string, List<INamedTypeSymbol>> hosts = new Dictionary<string, List<INamedTypeSymbol>>(StringComparer.Ordinal);

            foreach (Srm.TypeDefinitionHandle handle in reader.TypeDefinitions)
            {
                Srm.TypeDefinition definition = reader.GetTypeDefinition(handle);

                // A marked class is a top-level class.
                if (!definition.GetDeclaringType().IsNil)
                    continue;

                foreach (Srm.CustomAttributeHandle attributeHandle in definition.GetCustomAttributes())
                {
                    Srm.CustomAttribute attribute = reader.GetCustomAttribute(attributeHandle);

                    if (Array.IndexOf(HostAttributes, AttributeClass(reader, attribute)) < 0)
                        continue;

                    Srm.BlobReader value = reader.GetBlobReader(attribute.Value);

                    if (value.Length < 3 || value.ReadUInt16() != 1)
                        continue;

                    // A type of another assembly is written with that assembly's name after a comma. A struct and
                    // its class are in one assembly, so such an argument marks nothing.
                    string key = value.ReadSerializedString();

                    if (string.IsNullOrEmpty(key) || key.IndexOf(',') >= 0)
                        continue;

                    INamedTypeSymbol type = assembly.GetTypeByMetadataName(Qualified(reader.GetString(definition.Namespace), reader.GetString(definition.Name)));

                    if (type == null || !type.IsStatic || !Visible(type))
                        continue;

                    List<INamedTypeSymbol> list;

                    if (!hosts.TryGetValue(key, out list))
                        hosts[key] = list = new List<INamedTypeSymbol>();

                    list.Add(type);
                }
            }

            return hosts;
        }

        private static string Qualified(string space, string name)
        {
            return space.Length == 0 ? name : space + "." + name;
        }

        // The class of an attribute, from its constructor: a reference into another assembly, or a definition here.
        private static string AttributeClass(Srm.MetadataReader reader, Srm.CustomAttribute attribute)
        {
            if (attribute.Constructor.Kind == Srm.HandleKind.MemberReference)
            {
                Srm.EntityHandle parent = reader.GetMemberReference((Srm.MemberReferenceHandle)attribute.Constructor).Parent;

                if (parent.Kind == Srm.HandleKind.TypeReference)
                {
                    Srm.TypeReference reference = reader.GetTypeReference((Srm.TypeReferenceHandle)parent);
                    return Qualified(reader.GetString(reference.Namespace), reader.GetString(reference.Name));
                }

                if (parent.Kind != Srm.HandleKind.TypeDefinition)
                    return "";

                Srm.TypeDefinition inAssembly = reader.GetTypeDefinition((Srm.TypeDefinitionHandle)parent);
                return Qualified(reader.GetString(inAssembly.Namespace), reader.GetString(inAssembly.Name));
            }

            if (attribute.Constructor.Kind != Srm.HandleKind.MethodDefinition)
                return "";

            Srm.TypeDefinition declaring = reader.GetTypeDefinition(reader.GetMethodDefinition((Srm.MethodDefinitionHandle)attribute.Constructor).GetDeclaringType());
            return Qualified(reader.GetString(declaring.Namespace), reader.GetString(declaring.Name));
        }

        private static string Strip(string text)
        {
            return text.Replace(ShadowMarker, "");
        }

        // A type as the engine names one in a target: T: and the documentation id, a constructed generic type's
        // arguments in braces, a method's type parameter as ``n and a type's as `n (wrap{``1}).
        private static string ReferenceId(ITypeSymbol type)
        {
            return "T:" + TypeName(type);
        }

        private static string TypeName(ITypeSymbol type)
        {
            ITypeParameterSymbol parameter = type as ITypeParameterSymbol;

            if (parameter != null)
                return (parameter.TypeParameterKind == TypeParameterKind.Method ? "``" : "`") + parameter.Ordinal;

            INamedTypeSymbol named = type as INamedTypeSymbol;
            string id = Id(type.OriginalDefinition);

            if (id.StartsWith("T:", StringComparison.Ordinal))
                id = id.Substring(2);

            if (named == null || !named.IsGenericType || ReferenceEquals(named, named.OriginalDefinition))
                return id.Length > 0 ? id : type.ToDisplayString(s_type);

            int arity = id.LastIndexOf('`');

            return (arity < 0 ? id : id.Substring(0, arity)) + "{" + string.Join(",", named.TypeArguments.Select(TypeName)) + "}";
        }

        // The type a receiver parameter names: S for S, and S for the pointer form, go.zh<S>.
        private static INamedTypeSymbol Receiver(IMethodSymbol method)
        {
            INamedTypeSymbol receiver = method.Parameters[0].Type as INamedTypeSymbol;

            if (receiver != null && receiver.Arity == 1 && receiver.MetadataName == "\u0436`1" && receiver.ContainingType == null &&
                Namespace(receiver.ContainingNamespace) == "go")
            {
                receiver = receiver.TypeArguments[0] as INamedTypeSymbol;
            }

            return receiver;
        }

        // THE DELTA DROP, the second announced kind. A removed type or member is a rename only when its documentation id
        // with every Delta dropped is in the current surface exactly, or, for a method, when it is a move once its
        // receiver, its name and both signatures drop their Delta: the method of a renamed receiver (syscall's Handle,
        // now in the holder marked GoMethodHost(typeof(Handle))) or one whose signature names a renamed type. Anything
        // else is refused.
        private static bool Renamed(string id, ISymbol removed, Dictionary<string, ISymbol> after, Dictionary<string, List<INamedTypeSymbol>> hosts,
                                    Srm.MetadataReader baseline, Srm.MetadataReader current, out string now)
        {
            now = Strip(id);

            if (now != id && after.ContainsKey(now))
                return true;

            now = null;
            IMethodSymbol method = removed as IMethodSymbol;

            if (method == null || method.MethodKind != MethodKind.Ordinary || !method.IsStatic || method.Parameters.Length == 0)
                return false;

            INamedTypeSymbol receiver = Receiver(method);

            if (receiver == null)
                return false;

            // A parameter may keep its Delta (trace's timeDelta1 still shadows an import) or lose it (runtime's Pinner
            // receiver, Deltap while it shared the scope of the type p, is p in its holder): both are the drop.
            string signature = Signature(baseline, method), dropped = Signature(baseline, method, true),
                   droppedNames = Signature(baseline, method, true, true), key = MetadataName(receiver.OriginalDefinition);

            // Nothing of it was renamed: the move test already refused it as it stands.
            if (droppedNames == signature && Strip(key) == key)
                return false;

            List<INamedTypeSymbol> candidates;

            if (!hosts.TryGetValue(Strip(key), out candidates))
                return false;

            foreach (INamedTypeSymbol candidate in candidates)
            {
                foreach (IMethodSymbol other in candidate.GetMembers(Strip(method.Name)).OfType<IMethodSymbol>())
                {
                    string found = Signature(current, other);

                    if (found == dropped || found == droppedNames)
                    {
                        now = Id(other);
                        return true;
                    }
                }
            }

            return false;
        }

        // What a caller compiled against a member binds besides its documentation id, which leaves out a method's
        // return type and a field's, property's or event's type: those, by their full names.
        private static string Shape(ISymbol symbol)
        {
            IMethodSymbol method = symbol as IMethodSymbol;
            IPropertySymbol property = symbol as IPropertySymbol;
            IFieldSymbol field = symbol as IFieldSymbol;
            IEventSymbol @event = symbol as IEventSymbol;

            if (method != null)
                return method.RefKind + " " + method.ReturnType.ToDisplayString(s_type);

            if (property != null)
                return property.RefKind + " " + property.Type.ToDisplayString(s_type);

            if (field != null)
                return field.Type.ToDisplayString(s_type);

            return @event != null ? @event.Type.ToDisplayString(s_type) : "";
        }

        private static HashSet<string> Interfaces(INamedTypeSymbol type)
        {
            return new HashSet<string>(type.AllInterfaces.Select(ReferenceId), StringComparer.Ordinal);
        }

        // The Delta drop's diagnostics on a type or member both builds declare under one id, each entered only when the
        // difference is the dropped Delta and nothing else:
        //   CP0002  a member whose return or value type now names a renamed type by its Go name (the id leaves the type
        //           out, so the engine sees the old member removed), and CP0006 too where an interface declares it;
        //   CP0006  an interface member added under the Go-named id of one the baseline declared;
        //   CP0008  a type that no longer implements an interface a renamed interface replaced (go/doc/comment's
        //           Italic implemented the Delta-named Text and implements Text);
        //   CP0021  a method type parameter's constraint that names a renamed type by its Go name.
        // Every test reads one way: the baseline's form with its Delta dropped is the current form exactly. A difference
        // the drop does not explain, the reverse included, is refused: the guard reads it red.
        private static void RenamedShapes(Dictionary<string, ISymbol> before, Dictionary<string, ISymbol> after, Result result)
        {
            Dictionary<string, List<string>> removedByStripped = new Dictionary<string, List<string>>(StringComparer.Ordinal);

            foreach (KeyValuePair<string, ISymbol> entry in before)
            {
                if (after.ContainsKey(entry.Key) || entry.Value.ContainingType == null || entry.Value.ContainingType.TypeKind != TypeKind.Interface)
                    continue;

                List<string> list;

                if (!removedByStripped.TryGetValue(Strip(entry.Key), out list))
                    removedByStripped[Strip(entry.Key)] = list = new List<string>();

                list.Add(entry.Key);
            }

            foreach (KeyValuePair<string, ISymbol> entry in after.OrderBy(pair => pair.Key, StringComparer.Ordinal))
            {
                ISymbol now = entry.Value, was;
                bool inInterface = now.ContainingType != null && now.ContainingType.TypeKind == TypeKind.Interface;

                if (!before.TryGetValue(entry.Key, out was))
                {
                    if (inInterface && !(now is INamedTypeSymbol) && before.ContainsKey(Id(now.ContainingType)) && removedByStripped.ContainsKey(entry.Key))
                        result.Renames.Add(new Rename { DiagnosticId = "CP0006", Target = entry.Key, Now = entry.Key });
                    else if (inInterface && before.ContainsKey(Id(now.ContainingType)) && HidesSpread(now, before))
                        result.Variadics.Add(new Rename { DiagnosticId = "CP0006", Target = entry.Key, Now = entry.Key });

                    continue;
                }

                if (!(now is INamedTypeSymbol))
                {
                    string had = Shape(was), has = Shape(now);

                    if (had != has)
                    {
                        // The Delta drop, or the variadic tail's spread property: the same accessor entries, each
                        // under its own kind.
                        List<Rename> kind = Strip(had) == has ? result.Renames : PackShape(was) == has ? result.Variadics : null;

                        if (kind != null)
                        {
                            // The engine names a property by its accessors: the getter keeps its id, the setter's
                            // takes the value's type.
                            IPropertySymbol oldProperty = was as IPropertySymbol, newProperty = now as IPropertySymbol;
                            List<KeyValuePair<string, string>> targets = new List<KeyValuePair<string, string>>();

                            if (oldProperty == null)
                                targets.Add(new KeyValuePair<string, string>(entry.Key, entry.Key));
                            else
                            {
                                if (oldProperty.GetMethod != null)
                                    targets.Add(new KeyValuePair<string, string>(Id(oldProperty.GetMethod), newProperty.GetMethod == null ? "" : Id(newProperty.GetMethod)));

                                if (oldProperty.SetMethod != null)
                                    targets.Add(new KeyValuePair<string, string>(Id(oldProperty.SetMethod), newProperty.SetMethod == null ? "" : Id(newProperty.SetMethod)));
                            }

                            foreach (KeyValuePair<string, string> target in targets)
                            {
                                kind.Add(new Rename { DiagnosticId = "CP0002", Target = target.Key, Now = target.Value });

                                if (inInterface && target.Value.Length > 0)
                                    kind.Add(new Rename { DiagnosticId = "CP0006", Target = target.Value, Now = target.Value });
                            }
                        }
                        else
                        {
                            result.Refused.Add(new Refusal { Target = entry.Key, Reason = "changed its type from " + had + " to " + has });
                        }
                    }
                }

                INamedTypeSymbol type = now as INamedTypeSymbol, oldType = was as INamedTypeSymbol;

                if (type != null && oldType != null)
                {
                    HashSet<string> implemented = Interfaces(type);
                    List<string> lost = Interfaces(oldType).Where(each => !implemented.Contains(each)).OrderBy(each => each, StringComparer.Ordinal).ToList();

                    if (lost.Count > 0)
                    {
                        if (lost.All(each => Strip(each) != each && implemented.Contains(Strip(each)) && !Interfaces(oldType).Contains(Strip(each))))
                            result.Renames.Add(new Rename { DiagnosticId = "CP0008", Target = entry.Key, Now = entry.Key });
                        else if (CloneDropped(oldType, type, lost))
                            result.Clones.Add(new Rename { DiagnosticId = "CP0008", Target = entry.Key, Now = entry.Key });
                        else
                            result.Refused.Add(new Refusal { Target = entry.Key, Reason = "no longer implements " + string.Join(", ", lost) });
                    }
                }

                IMethodSymbol method = now as IMethodSymbol, oldMethod = was as IMethodSymbol;

                if (method != null && oldMethod != null && method.TypeParameters.Length == oldMethod.TypeParameters.Length)
                {
                    for (int i = 0; i < method.TypeParameters.Length; i++)
                    {
                        HashSet<string> had = new HashSet<string>(oldMethod.TypeParameters[i].ConstraintTypes.Select(ReferenceId), StringComparer.Ordinal);
                        HashSet<string> has = new HashSet<string>(method.TypeParameters[i].ConstraintTypes.Select(ReferenceId), StringComparer.Ordinal);

                        foreach (string added in has.Where(each => !had.Contains(each)).OrderBy(each => each, StringComparer.Ordinal))
                        {
                            if (had.Any(each => !has.Contains(each) && each != added && Strip(each) == added))
                                result.Renames.Add(new Rename { DiagnosticId = "CP0021", Target = entry.Key + "``" + i + ":" + added, Now = entry.Key });
                            else
                                result.Refused.Add(new Refusal { Target = entry.Key, Reason = "type parameter " + i + " gained the constraint " + added });
                        }
                    }
                }
            }
        }

        // THE VARIADIC TAIL. Each assembly is loaded alone, so neither System.Span<T> nor go.variadic<T> resolves; both
        // are compared by their full names, which the unresolved types keep.
        private const string SpanPrefix = "global::System.Span<";
        private const string PackPrefix = "global::go.variadic<";

        // A type's name with System.Span<X> made go.variadic<X>; null for any other type.
        private static string AsPack(string type)
        {
            return type.StartsWith(SpanPrefix, StringComparison.Ordinal) ? PackPrefix + type.Substring(SpanPrefix.Length) : null;
        }

        // A get-only property's Shape with a System.Span<X> type made go.variadic<X>, the spread property's change;
        // null for anything else, a method returning a span included (P3 145156Z, COORD 145250Z).
        // The variadic tail's spread property added to an interface: a get-only property that hides, under its own name,
        // a base interface's spread property the baseline declared, and is that property with System.Span<X> made
        // go.variadic<X> and nothing else (golib's ISlice<T> hides IArray<T>'s Span<T> spread with a variadic<T> one).
        // The engine reports the added interface member, CP0006; the base property is unchanged and reports nothing.
        private static bool HidesSpread(ISymbol added, Dictionary<string, ISymbol> before)
        {
            IPropertySymbol property = added as IPropertySymbol;

            if (property == null || property.GetMethod == null || property.SetMethod != null || property.IsIndexer)
                return false;

            string shape = Shape(property);

            foreach (INamedTypeSymbol baseInterface in property.ContainingType.AllInterfaces)
                foreach (IPropertySymbol hidden in baseInterface.GetMembers(property.Name).OfType<IPropertySymbol>())
                    // A generic base is reached constructed (IArray<T> of ISlice<T>'s T): its member is named by its
                    // definition, and its shape read constructed, in the hiding interface's own type parameters.
                    if (before.ContainsKey(Id(hidden.OriginalDefinition)) && PackShape(hidden) == shape)
                        return true;

            return false;
        }

        private static string PackShape(ISymbol symbol)
        {
            IPropertySymbol property = symbol as IPropertySymbol;

            if (property == null || property.GetMethod == null || property.SetMethod != null)
                return null;

            string pack = AsPack(property.Type.ToDisplayString(s_type));

            return pack == null ? null : property.RefKind + " " + pack;
        }

        // A delegate by its base type's name, for the reason IsStruct gives.
        private static bool IsDelegate(INamedTypeSymbol type)
        {
            return type.TypeKind == TypeKind.Delegate || (type.BaseType != null && type.BaseType.ToDisplayString(s_type) == "global::System.MulticastDelegate");
        }

        // A removed method is the variadic tail when its type, still in the surface, declares a NEW method (one the
        // baseline did not have) of the same name, kind, accessibility, return type and type parameters whose parameters
        // keep every name, ref kind and type but one, a System.Span<X> that is now a go.variadic<X> and is params. The
        // one waiver is a delegate's BeginInvoke, which repeats its Invoke's tail without the mark, just before its
        // AsyncCallback and state: a delegate's Invoke must show the mark like any method. Two changed parameters,
        // another element type, another span type, a span that is not params or anything else changed is refused.
        private static bool VariadicTail(ISymbol removed, Dictionary<string, ISymbol> before, Dictionary<string, ISymbol> after, out string now)
        {
            now = null;
            IMethodSymbol method = removed as IMethodSymbol;
            ISymbol owner;

            if (method == null || method.ContainingType == null || !after.TryGetValue(Id(method.ContainingType), out owner) || !(owner is INamedTypeSymbol))
                return false;

            // BeginInvoke(..., tail, AsyncCallback callback, object @object): the tail is third from the end.
            int beginInvokeTail = IsDelegate(method.ContainingType) && method.Name == "BeginInvoke" ? method.Parameters.Length - 3 : -1;

            foreach (IMethodSymbol other in ((INamedTypeSymbol)owner).GetMembers(method.Name).OfType<IMethodSymbol>())
            {
                if (other.IsStatic != method.IsStatic || other.MethodKind != method.MethodKind || other.DeclaredAccessibility != method.DeclaredAccessibility ||
                    other.TypeParameters.Length != method.TypeParameters.Length || other.Parameters.Length != method.Parameters.Length || Shape(other) != Shape(method) ||
                    before.ContainsKey(Id(other)))
                {
                    continue;
                }

                int changed = 0;
                bool same = true;

                for (int i = 0; i < method.Parameters.Length && same; i++)
                {
                    IParameterSymbol was = method.Parameters[i], has = other.Parameters[i];
                    string had = was.Type.ToDisplayString(s_type), hasType = has.Type.ToDisplayString(s_type);

                    if (was.Name != has.Name || was.RefKind != has.RefKind)
                        same = false;
                    else if (had != hasType)
                    {
                        if (AsPack(had) == hasType && (has.IsParams || i == beginInvokeTail))
                            changed++;
                        else
                            same = false;
                    }
                }

                if (same && changed == 1)
                {
                    now = Id(other);
                    return true;
                }
            }

            return false;
        }

        // THE CLONE DROP. A struct whose C# value copy is incomplete on its own implements golib's IGoValueClone (which
        // extends ICloneable), and go2cs-gen writes its field-precise Delta-named Clone(). A struct whose copy needs no
        // clone has neither: a zero-length array, which holds nothing to copy, no longer makes a struct need one
        // (1.26.11.1). A struct that lost exactly the interface and the method, and nothing else, is that drop.
        private const string CloneInterface = "T:go.IGoValueClone";
        private const string CloneableInterface = "T:System.ICloneable";
        private const string CloneMethod = ShadowMarker + "Clone";

        // Each assembly is loaded alone, as the guard compares it, so neither interface resolves and each id reads
        // T:!:<name>; the name is compared.
        private static bool CloneDropped(INamedTypeSymbol was, INamedTypeSymbol now, List<string> lost)
        {
            List<string> names = lost.Select(each => each.StartsWith("T:!:", StringComparison.Ordinal) ? "T:" + each.Substring(4) : each).ToList();

            return IsStruct(was) && IsStruct(now) &&
                   names.Contains(CloneInterface) && names.All(each => each == CloneInterface || each == CloneableInterface) &&
                   HasCloneMethod(was) && now.GetMembers(CloneMethod).IsEmpty;
        }

        // A struct by its base type's name: with the core library unresolved the compiler cannot tell a struct from a
        // class, and reads every type as a class.
        private static bool IsStruct(INamedTypeSymbol type)
        {
            return type.TypeKind == TypeKind.Struct || (type.BaseType != null && type.BaseType.ToDisplayString(s_type) == "global::System.ValueType");
        }

        private static bool HasCloneMethod(INamedTypeSymbol type)
        {
            return type.GetMembers(CloneMethod).OfType<IMethodSymbol>().Any(method =>
                method.MethodKind == MethodKind.Ordinary && !method.IsStatic && method.Parameters.Length == 0 &&
                SymbolEqualityComparer.Default.Equals(method.ReturnType, type));
        }

        // The removed Clone() of a struct that dropped its clone whole (CloneDropped); the engine reports it as a CP0002.
        private static bool CloneMethodDropped(ISymbol removed, Dictionary<string, ISymbol> after)
        {
            IMethodSymbol method = removed as IMethodSymbol;
            INamedTypeSymbol was = removed.ContainingType;
            ISymbol successor;

            if (method == null || was == null || method.Name != CloneMethod || !after.TryGetValue(Id(was), out successor))
                return false;

            INamedTypeSymbol now = successor as INamedTypeSymbol;

            if (now == null || !SymbolEqualityComparer.Default.Equals(method.ReturnType, was) || method.Parameters.Length != 0 || method.IsStatic)
                return false;

            HashSet<string> implemented = Interfaces(now);

            return CloneDropped(was, now, Interfaces(was).Where(each => !implemented.Contains(each)).ToList());
        }

        private static bool Moved(ISymbol removed, Dictionary<string, List<INamedTypeSymbol>> hosts, Srm.MetadataReader baseline,
                                  Srm.MetadataReader current, out string host, out string reason)
        {
            host = null;
            IMethodSymbol method = removed as IMethodSymbol;

            if (method == null || method.MethodKind != MethodKind.Ordinary || !method.IsStatic || method.Parameters.Length == 0)
            {
                reason = "removed, and not a static method with a receiver parameter";
                return false;
            }

            // The pointer form takes the struct's box.
            INamedTypeSymbol receiver = Receiver(method);

            if (receiver == null)
            {
                reason = "removed, and its first parameter is not a named type";
                return false;
            }

            string key = MetadataName(receiver.OriginalDefinition);
            List<INamedTypeSymbol> candidates;

            if (!hosts.TryGetValue(key, out candidates))
            {
                reason = "removed, and no class is marked GoPromotedHost(typeof(" + key + ")) or GoMethodHost(typeof(" + key + "))";
                return false;
            }

            string signature = Signature(baseline, method);
            List<string> declared = new List<string>();

            foreach (INamedTypeSymbol candidate in candidates)
            {
                foreach (IMethodSymbol other in candidate.GetMembers(method.Name).OfType<IMethodSymbol>())
                {
                    string found = Signature(current, other);

                    if (found == signature)
                    {
                        host = MetadataName(candidate);
                        reason = null;
                        return true;
                    }

                    declared.Add(found);
                }
            }

            reason = "removed, and " + string.Join(", ", candidates.Select(MetadataName)) + " declares no " + method.Name +
                     " of the same signature and attributes (" + signature + ")" +
                     (declared.Count == 0 ? "" : "; declared there: " + string.Join(" | ", declared));
            return false;
        }

        // The two attributes that ENCODE nullable annotations. Where the compiler writes them depends on the other
        // members of the class, so they are not compared as attributes: Nullability compares what they say.
        private static bool EncodesNullability(AttributeData attribute)
        {
            INamedTypeSymbol type = attribute.AttributeClass;

            return type != null && Namespace(type.ContainingNamespace) == "System.Runtime.CompilerServices" &&
                   (type.MetadataName == "NullableAttribute" || type.MetadataName == "NullableContextAttribute");
        }

        private static string Attributes(IEnumerable<AttributeData> attributes)
        {
            return string.Join(";", attributes.Where(attribute => !EncodesNullability(attribute)).Select(attribute => attribute.ToString())
                .OrderBy(text => text, StringComparer.Ordinal));
        }

        // Whether a type in a signature takes a nullable annotation: every type but a primitive, a non-generic value
        // type and a System.Nullable<T> of one. A signature says class or value type itself, so no type has to
        // resolve. 2 is System.Nullable<T> before its argument is known.
        private sealed class Annotated : Srm.ISignatureTypeProvider<int, object>
        {
            private const byte ValueType = 0x11;

            public int GetPrimitiveType(Srm.PrimitiveTypeCode code)
            {
                return code == Srm.PrimitiveTypeCode.String || code == Srm.PrimitiveTypeCode.Object ? 1 : 0;
            }

            public int GetTypeFromDefinition(Srm.MetadataReader reader, Srm.TypeDefinitionHandle handle, byte rawTypeKind)
            {
                return rawTypeKind == ValueType ? 0 : 1;
            }

            public int GetTypeFromReference(Srm.MetadataReader reader, Srm.TypeReferenceHandle handle, byte rawTypeKind)
            {
                if (rawTypeKind != ValueType)
                    return 1;

                Srm.TypeReference reference = reader.GetTypeReference(handle);

                return reader.GetString(reference.Name) == "Nullable`1" && reader.GetString(reference.Namespace) == "System" ? 2 : 0;
            }

            public int GetTypeFromSpecification(Srm.MetadataReader reader, object context, Srm.TypeSpecificationHandle handle, byte rawTypeKind)
            {
                return reader.GetTypeSpecification(handle).DecodeSignature(this, context);
            }

            public int GetGenericInstantiation(int genericType, ImmutableArray<int> typeArguments)
            {
                return genericType == 2 ? Math.Min(typeArguments[0], 1) : 1;
            }

            public int GetSZArrayType(int elementType) { return 1; }

            public int GetArrayType(int elementType, Srm.ArrayShape shape) { return 1; }

            public int GetByReferenceType(int elementType) { return elementType; }

            public int GetPointerType(int elementType) { return elementType; }

            public int GetPinnedType(int elementType) { return elementType; }

            public int GetModifiedType(int modifier, int unmodifiedType, bool isRequired) { return unmodifiedType; }

            public int GetGenericMethodParameter(object context, int index) { return 1; }

            public int GetGenericTypeParameter(object context, int index) { return 1; }

            public int GetFunctionPointerType(Srm.MethodSignature<int> signature) { return 1; }
        }

        // What one nullable attribute among these says, or null without one: a byte for every annotated type of the
        // position, or a counted list of them, one each.
        private static string NullableBytes(Srm.MetadataReader reader, Srm.CustomAttributeHandleCollection attributes, string attributeClass)
        {
            foreach (Srm.CustomAttributeHandle handle in attributes)
            {
                Srm.CustomAttribute attribute = reader.GetCustomAttribute(handle);

                if (AttributeClass(reader, attribute) != attributeClass)
                    continue;

                // The prolog (2 bytes), the argument, the count of named arguments (2 bytes).
                byte[] blob = reader.GetBlobBytes(attribute.Value);

                if (blob.Length == 5)
                    return blob[2].ToString();

                if (blob.Length >= 8)
                    return "[" + string.Join(",", blob.Skip(6).Take(blob.Length - 8).Select(each => each.ToString())) + "]";

                return "unread";
            }

            return null;
        }

        // The context a method's positions take without an attribute of their own: the method's, else the nearest
        // enclosing class's, else the module's, else oblivious.
        private static string NullableContext(Srm.MetadataReader reader, Srm.MethodDefinition method)
        {
            string context = NullableBytes(reader, method.GetCustomAttributes(), NullableContextAttribute);
            Srm.TypeDefinitionHandle handle = method.GetDeclaringType();

            while (context == null && !handle.IsNil)
            {
                Srm.TypeDefinition type = reader.GetTypeDefinition(handle);

                context = NullableBytes(reader, type.GetCustomAttributes(), NullableContextAttribute);
                handle = type.GetDeclaringType();
            }

            if (context == null)
                context = NullableBytes(reader, reader.GetCustomAttributes(Srm.EntityHandle.ModuleDefinition), NullableContextAttribute);

            return context ?? "0";
        }

        // The nullable annotations of a method, decoded: the return, each parameter, then each type parameter with
        // its constraints. Equal for one method declared in two classes, whatever each class's own context is.
        private static string Nullability(Srm.MetadataReader reader, IMethodSymbol method)
        {
            Srm.EntityHandle handle = Srm.Ecma335.MetadataTokens.EntityHandle(method.MetadataToken);

            if (handle.IsNil || handle.Kind != Srm.HandleKind.MethodDefinition)
                return "unread";

            Srm.MethodDefinition definition = reader.GetMethodDefinition((Srm.MethodDefinitionHandle)handle);
            string context = NullableContext(reader, definition);
            Srm.MethodSignature<int> shape = definition.DecodeSignature(s_annotated, null);
            string[] positions = new string[shape.ParameterTypes.Length + 1];

            foreach (Srm.ParameterHandle parameterHandle in definition.GetParameters())
            {
                Srm.Parameter parameter = reader.GetParameter(parameterHandle);

                if (parameter.SequenceNumber < positions.Length)
                    positions[parameter.SequenceNumber] = NullableBytes(reader, parameter.GetCustomAttributes(), NullableAttribute);
            }

            for (int i = 0; i < positions.Length; i++)
            {
                int annotated = i == 0 ? shape.ReturnType : shape.ParameterTypes[i - 1];

                positions[i] = annotated == 0 ? "-" : positions[i] ?? context;
            }

            List<string> typeParameters = new List<string>();

            foreach (Srm.GenericParameterHandle parameterHandle in definition.GetGenericParameters())
            {
                Srm.GenericParameter parameter = reader.GetGenericParameter(parameterHandle);
                bool valueType = (parameter.Attributes & System.Reflection.GenericParameterAttributes.NotNullableValueTypeConstraint) != 0;
                string text = valueType ? "-" : NullableBytes(reader, parameter.GetCustomAttributes(), NullableAttribute) ?? context;

                foreach (Srm.GenericParameterConstraintHandle constraintHandle in parameter.GetConstraints())
                {
                    Srm.GenericParameterConstraint constraint = reader.GetGenericParameterConstraint(constraintHandle);

                    text += ":" + (NullableBytes(reader, constraint.GetCustomAttributes(), NullableAttribute) ?? context);
                }

                typeParameters.Add(text);
            }

            return string.Join(" ", positions) + " <" + string.Join(",", typeParameters) + ">";
        }

        // Everything a caller compiled against the method depends on, the declaring type left out.
        // dropDelta reads the baseline as the Delta drop would have written it: every type, attribute and the method's
        // name without the prefix. A parameter's name keeps any it has unless dropNames says otherwise.
        private static string Signature(Srm.MetadataReader reader, IMethodSymbol method, bool dropDelta = false, bool dropNames = false)
        {
            Func<string, string> types = text => dropDelta ? Strip(text) : text;
            Func<string, string> names = text => dropNames ? Strip(text) : text;

            IEnumerable<string> parameters = method.Parameters.Select(parameter =>
                "[" + types(Attributes(parameter.GetAttributes())) + "]" + parameter.RefKind + " " + types(parameter.Type.ToDisplayString(s_type)) + " " + names(parameter.Name) +
                (parameter.IsParams ? " params" : "") + (parameter.HasExplicitDefaultValue ? " = " + (parameter.ExplicitDefaultValue ?? "null") : ""));

            IEnumerable<string> typeParameters = method.TypeParameters.Select(parameter =>
                parameter.Name + ":" + string.Join("&", parameter.ConstraintTypes.Select(type => types(type.ToDisplayString(s_type)))) +
                (parameter.HasReferenceTypeConstraint ? "&class" : "") + (parameter.HasValueTypeConstraint ? "&struct" : "") +
                (parameter.HasUnmanagedTypeConstraint ? "&unmanaged" : "") + (parameter.HasNotNullConstraint ? "&notnull" : "") +
                (parameter.HasConstructorConstraint ? "&new()" : ""));

            return "[" + types(Attributes(method.GetAttributes())) + "][return: " + types(Attributes(method.GetReturnTypeAttributes())) + "] " +
                   method.DeclaredAccessibility + (method.IsExtensionMethod ? " extension " : " static ") + method.RefKind + " " +
                   types(method.ReturnType.ToDisplayString(s_type)) + " " + types(method.Name) + "<" + string.Join(",", typeParameters) + ">(" + string.Join(", ", parameters) +
                   ") nullable " + Nullability(reader, method);
        }
    }
}
'@

# Whether this PowerShell can read an assembly's surface: PowerShell 7 ships Roslyn for Add-Type, Windows PowerShell
# 5.1 does not.
function Test-GoApiCompatMovesAvailable {
    [bool]('Microsoft.CodeAnalysis.CSharp.CSharpCompilation' -as [type])
}

function Initialize-GoApiCompatMoves {
    if ('Go2cs.ApiCompatMoves.Finder' -as [type]) { return }
    if (-not (Test-GoApiCompatMovesAvailable)) {
        throw "This PowerShell ($($PSVersionTable.PSVersion)) ships no Roslyn (Microsoft.CodeAnalysis.CSharp): generate suppressions with pwsh, PowerShell 7"
    }
    # Roslyn by path; the framework by simple name, which Add-Type resolves in PowerShell's own reference
    # assemblies. Roslyn is built against an older System.Runtime than those, which the compiler reports as
    # warning CS1701 on every reference, so warnings are not errors here.
    $roslyn = @([Microsoft.CodeAnalysis.Compilation].Assembly.Location, [Microsoft.CodeAnalysis.CSharp.CSharpCompilation].Assembly.Location)
    $references = $roslyn + @('System.Runtime', 'System.Collections', 'System.Collections.Immutable', 'System.Linq', 'System.Reflection.Metadata', 'netstandard')
    Add-Type -TypeDefinition $script:FinderSource -ReferencedAssemblies $references -IgnoreWarnings -WarningAction SilentlyContinue
}

# The moves and the refused removals between two builds of one assembly.
function Get-GoApiCompatMoves([Parameter(Mandatory)][string]$Baseline, [Parameter(Mandatory)][string]$Current) {
    Initialize-GoApiCompatMoves
    [Go2cs.ApiCompatMoves.Finder]::Find((Resolve-Path -LiteralPath $Baseline).ProviderPath, (Resolve-Path -LiteralPath $Current).ProviderPath)
}

function Get-MovesAssets([string]$Package) {
    $zip = [System.IO.Compression.ZipFile]::OpenRead($Package)
    try { @($zip.Entries | ForEach-Object { $_.FullName -replace '\\', '/' } | Where-Object { $_ -match $script:AssetPattern } | Sort-Object) }
    finally { $zip.Dispose() }
}

function Expand-MovesAsset([string]$Package, [string]$Asset, [string]$Path) {
    $zip = [System.IO.Compression.ZipFile]::OpenRead($Package)
    try {
        $entry = @($zip.Entries | Where-Object { ($_.FullName -replace '\\', '/') -ceq $Asset })[0]
        New-Item -ItemType Directory -Force (Split-Path -Parent $Path) | Out-Null
        [System.IO.Compression.ZipFileExtensions]::ExtractToFile($entry, $Path, $true)
    }
    finally { $zip.Dispose() }
}

<#
    The moves between a baseline package and the current one, asset by asset, as the guard compares them. Returns:
      Entries  one per moved member per asset: Asset, Target (its documentation id), Host (the class that has it now)
      Refused  one per other removed member per asset: Asset, Target, Reason
    An asset only one of the packages carries is not read: the guard reports a missing one itself. $Successors maps a
    baseline asset the current package lacks to the asset the guard compares it with instead, the declared assembly
    rename (GoApiCompat.psm1, Get-GoApiCompatRenameSuccessors); its entries name the baseline asset, as the guard's do.
#>
function Get-GoApiCompatPackageMoves {
    param(
        [Parameter(Mandatory)][string]$Baseline,
        [Parameter(Mandatory)][string]$Current,
        [Parameter(Mandatory)][string]$WorkDir,
        [hashtable]$Successors = @{}
    )

    $entries = New-Object System.Collections.Generic.List[object]
    $refused = New-Object System.Collections.Generic.List[object]
    $currentAssets = @(Get-MovesAssets $Current)
    $work = Join-Path $WorkDir ('moves-' + [guid]::NewGuid().ToString('N'))

    try {
        foreach ($asset in @(Get-MovesAssets $Baseline)) {
            $currentAsset = $asset
            if ($currentAssets -notcontains $asset) {
                if (-not $Successors.ContainsKey($asset) -or $currentAssets -notcontains $Successors[$asset]) { continue }
                $currentAsset = $Successors[$asset]
            }
            $left = Join-Path (Join-Path $work 'left') $asset
            $right = Join-Path (Join-Path $work 'right') $asset
            Expand-MovesAsset $Baseline $asset $left
            Expand-MovesAsset $Current $currentAsset $right
            $found = Get-GoApiCompatMoves -Baseline $left -Current $right
            foreach ($move in $found.Moves) { $entries.Add([pscustomobject]@{ Asset = $asset; DiagnosticId = 'CP0002'; Kind = 'move'; Target = $move.Target; Host = $move.Host }) }
            foreach ($rename in $found.Renames) { $entries.Add([pscustomobject]@{ Asset = $asset; DiagnosticId = $rename.DiagnosticId; Kind = 'rename'; Target = $rename.Target; Host = $rename.Now }) }
            foreach ($clone in $found.Clones) { $entries.Add([pscustomobject]@{ Asset = $asset; DiagnosticId = $clone.DiagnosticId; Kind = 'clone'; Target = $clone.Target; Host = $clone.Now }) }
            foreach ($tail in $found.Variadics) { $entries.Add([pscustomobject]@{ Asset = $asset; DiagnosticId = $tail.DiagnosticId; Kind = 'variadic'; Target = $tail.Target; Host = $tail.Now }) }
            foreach ($refusal in $found.Refused) { $refused.Add([pscustomobject]@{ Asset = $asset; Target = $refusal.Target; Reason = $refusal.Reason }) }
        }
    }
    finally { if (Test-Path -LiteralPath $work) { Remove-Item -LiteralPath $work -Recurse -Force } }

    [pscustomobject]@{ Entries = $entries.ToArray(); Refused = $refused.ToArray() }
}

<#
    Writes a package's CompatibilitySuppressions.xml: the guard's assembly-mode entries (Left and Right name the asset,
    no IsBaselineSuppression), sorted by asset and then by target, under a comment that says what announced them.
    UTF-8 without a byte-order mark and LF line ends, as the files before it. $Comment is the header's text; it must
    not hold two hyphens in a row.
#>
function Write-GoApiCompatMoveSuppressions {
    param(
        [Parameter(Mandatory)][AllowEmptyCollection()][object[]]$Entries,
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][string]$Comment
    )

    if ($Comment.Contains('--')) { throw 'The comment holds "--", which an XML comment cannot' }
    $lines = New-Object System.Collections.Generic.List[string]
    $lines.Add('<?xml version="1.0" encoding="utf-8"?>')
    $lines.Add('<!--')
    foreach ($line in ($Comment -split "`r?`n")) { $lines.Add(("  " + $line).TrimEnd()) }
    $lines.Add('-->')
    $lines.Add('<Suppressions xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">')
    $sorted = @($Entries | Sort-Object -Property @{ Expression = { $_.Asset } }, @{ Expression = { $_.Target } }, @{ Expression = { if ($_.PSObject.Properties['DiagnosticId']) { $_.DiagnosticId } else { 'CP0002' } } } -CaseSensitive -Culture '')
    foreach ($entry in $sorted) {
        $lines.Add('  <Suppression>')
        $diagnostic = if ($entry.PSObject.Properties['DiagnosticId'] -and $entry.DiagnosticId) { $entry.DiagnosticId } else { 'CP0002' }
        $lines.Add("    <DiagnosticId>$diagnostic</DiagnosticId>")
        $lines.Add("    <Target>$([System.Security.SecurityElement]::Escape($entry.Target))</Target>")
        $lines.Add("    <Left>$($entry.Asset)</Left>")
        $lines.Add("    <Right>$($entry.Asset)</Right>")
        $lines.Add('  </Suppression>')
    }
    $lines.Add('</Suppressions>')
    [System.IO.File]::WriteAllText($Path, (($lines -join "`n") + "`n"), (New-Object System.Text.UTF8Encoding $false))
}

Export-ModuleMember -Function Test-GoApiCompatMovesAvailable, Get-GoApiCompatMoves, Get-GoApiCompatPackageMoves, Write-GoApiCompatMoveSuppressions
