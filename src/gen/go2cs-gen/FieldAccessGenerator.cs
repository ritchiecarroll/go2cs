// FieldAccessGenerator.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static go2cs.Common;
using static go2cs.Symbols;

namespace go2cs;

/// <summary>
/// Emits a field property on <c>ж&lt;T&gt;</c> for every field of each struct type the
/// <c>[GoFieldAccess(...)]</c> records of a compilation list, so a field reached through a pointer reads as Go spells
/// it: <c>p.f</c>, and <c>buf.SubstituteNameOffset = x</c> through a pointer reinterpreted from <c>unsafe.Pointer</c>.
/// </summary>
/// <remarks>
/// <para>
/// Each property is a C# 14 ref-returning extension property with golib's <c>ж&lt;T&gt;.field</c> semantics: the
/// field at its Go offset when the pointer is a <c>NativeBox&lt;T&gt;</c> over native memory, and the struct's own slot
/// otherwise, with no box in between. Its body is specialized (<see cref="Specialized"/>): the managed arm reaches the
/// field directly, <c>ref ʗp.Value.f</c>, with no accessor delegate, which a field access through any pointer (parity
/// seat #31) cannot afford on every read. Being a <c>ref</c>, it serves a read, a write, a compound assignment, a
/// nested field (<c>p.In.X = 7</c>) and a <c>ref</c> argument alike. A field that shadows a promoted method gets none
/// (<see cref="ShadowsPromotedMethod"/>).
/// </para>
/// <para>
/// The converter writes the package's records in its package_info.cs, one assembly-level attribute per struct type
/// (an earlier converter put them on the package class of each FILE, which is still read; see
/// <see cref="RecordedTypes"/>), and the properties go into the USING package's own compilation, in an <c>internal</c> class: an extension block may extend a foreign type, an exported field's
/// <c>Ꮡ</c> accessor is public, and no other assembly sees the class, so two packages extending one type never meet.
/// The exception is a test assembly that references its package's assembly: it sees the package's class through
/// <c>InternalsVisibleTo</c>, so a property that class already declares is not declared again (see
/// <see cref="VisibleProperties"/>).
/// A field is skipped when its name is in <see cref="CollisionNames"/> (a member of <c>ж&lt;T&gt;</c> would bind
/// first, silently), when it is blank, or when it is not accessible here; a field of a FOREIGN type is skipped too
/// when the type carries no <c>Ꮡ</c> accessor for it. A generic struct, named by its open definition, gets a
/// generic extension block with its own type parameters and constraints.
/// </para>
/// <para>
/// An embedded struct gets a property too, its <c>ref</c> accessor's, so a field promoted through value embeds reads
/// as Go spells the path, <c>p.E.f</c>: in source, the embed is the converter's <c>partial ref E E</c> declaration; in
/// metadata, any <c>ref</c> property the type pairs with a <c>Ꮡ</c> accessor (a promoted field's too).
/// </para>
/// <para>
/// A HEADER-SHAPED struct is the one exception to <c>ref</c>: an <c>unsafe.Pointer</c> field and then one or two
/// <c>int</c> fields, Go's slice and string headers (<see cref="IsHeaderShaped"/>). Its fields get GET/SET properties,
/// and the setter calls <c>ж&lt;T&gt;.OnHeaderStore()</c> after the store. golib's header boxes view a slice or string
/// variable as such a header and must commit each store to the variable, which a <c>ref</c> cannot be intercepted to
/// do: the header's last store is followed by no access of the box at all. A read, a store and a compound assignment
/// bind the property as before; a <c>ref</c> to such a field does not compile, by design, since its stores would bypass
/// the commit.
/// </para>
/// </remarks>
[Generator]
public class FieldAccessGenerator : ISourceGenerator
{
    private const string AttributeName = "go.GoFieldAccessAttribute";

    private const string ReceiverName = "ʗp";

    private const string ClassName = "go.GoFieldAccess";

    /// <summary>
    /// The names a generated property must never take, read from the converter's <c>fieldAccessCollisions.txt</c>
    /// (embedded): the one list the converter, this generator and the GenTests guard all read.
    /// </summary>
    public static IReadOnlyCollection<string> CollisionNames => s_collisionNames;

    private static readonly HashSet<string> s_collisionNames = LoadCollisionNames();

    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new FieldAccessRecordFinder());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not FieldAccessRecordFinder finder || finder.Candidates.Count == 0 && !finder.AssemblyRecords)
            return;

        List<INamedTypeSymbol> types = RecordedTypes(context.Compilation, finder.Candidates);

        if (types.Count > 0)
            context.AddSource("GoFieldAccess.g.cs", Source(context.Compilation, types));
    }

    // The struct types the records list, each once, generic ones by their open definition. The converter writes the
    // records on the compilation's assembly, in package_info.cs (parity seat #31). An earlier converter wrote them on
    // the package class of each file, which is partial across the package's files, so its symbol carries every file's
    // records; the committed corpus carries that form until its next regeneration, so both are read.
    internal static List<INamedTypeSymbol> RecordedTypes(Compilation compilation, IEnumerable<ClassDeclarationSyntax> candidates)
    {
        List<INamedTypeSymbol> types = [];
        HashSet<INamedTypeSymbol> seen = new(SymbolEqualityComparer.Default);
        HashSet<INamedTypeSymbol> classes = new(SymbolEqualityComparer.Default);
        List<AttributeData> records = [.. compilation.Assembly.GetAttributes()];

        foreach (ClassDeclarationSyntax candidate in candidates)
        {
            if (compilation.GetSemanticModel(candidate.SyntaxTree).GetDeclaredSymbol(candidate) is INamedTypeSymbol packageClass && classes.Add(packageClass))
                records.AddRange(packageClass.GetAttributes());
        }

        foreach (AttributeData attribute in records)
        {
            if (attribute.AttributeClass?.ToDisplayString() != AttributeName)
                continue;

            foreach (TypedConstant argument in attribute.ConstructorArguments)
            {
                IEnumerable<TypedConstant> items = argument.Kind == TypedConstantKind.Array ? argument.Values : [argument];

                foreach (TypedConstant item in items)
                {
                    if (item.Value is INamedTypeSymbol { TypeKind: TypeKind.Struct } type && seen.Add(type.OriginalDefinition))
                        types.Add(type.OriginalDefinition);
                }
            }
        }

        return types;
    }

    internal static string Source(Compilation compilation, IEnumerable<INamedTypeSymbol> types)
    {
        StringBuilder source = new();
        source.Append("// <auto-generated/>\r\n#nullable enable\r\n\r\n");
        source.Append("namespace go;\r\n\r\n");
        source.Append("internal static class GoFieldAccess\r\n{");

        bool first = true;
        List<INamedTypeSymbol> visibleClasses = VisibleClasses(compilation);

        foreach (INamedTypeSymbol type in types)
        {
            List<string> properties = Properties(compilation, type, VisibleProperties(visibleClasses, type));

            if (properties.Count == 0)
                continue;

            string typeName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

            source.Append(first ? "\r\n" : "\r\n\r\n");
            source.Append($"    extension{TypeParameterList(type)}(global::go.{PointerPrefix}<{typeName}> {ReceiverName}){Constraints(type)}\r\n    {{");

            foreach (string property in properties)
                source.Append("\r\n        ").Append(property);

            source.Append("\r\n    }");
            first = false;
        }

        source.Append("\r\n}\r\n");

        return source.ToString();
    }

    // The GoFieldAccess classes of REFERENCED assemblies this compilation can see. Only a grant reaches one, since the
    // class is internal: a test assembly that references its package's assembly, which grants InternalsVisibleTo.
    private static List<INamedTypeSymbol> VisibleClasses(Compilation compilation) =>
        compilation.GetTypesByMetadataName(ClassName)
            .Where(type => !SymbolEqualityComparer.Default.Equals(type.ContainingAssembly, compilation.Assembly) &&
                           compilation.IsSymbolAccessibleWithin(type, compilation.Assembly))
            .ToList();

    /// <summary>
    /// The names of the properties a visible referenced class already declares on <c>ж&lt;T&gt;</c> for this type.
    /// </summary>
    /// <remarks>
    /// Declaring one again would make the two ambiguous, which C# reports as no member at all (CS9286): the runtime
    /// test assembly's <c>s.Δarray</c> on a <c>ж&lt;Δsliceᴛ&gt;</c> that the runtime's own <c>alg.cs</c> records too.
    /// A referenced block is read through the static implementation method each of its properties leaves on the
    /// class, <c>get_&lt;name&gt;(ж&lt;T&gt; ʗp)</c>; a generic block's is generic over the same open type.
    /// </remarks>
    private static HashSet<string> VisibleProperties(List<INamedTypeSymbol> visibleClasses, INamedTypeSymbol type)
    {
        HashSet<string> names = new(StringComparer.Ordinal);

        foreach (INamedTypeSymbol visibleClass in visibleClasses)
        {
            foreach (IMethodSymbol method in visibleClass.GetMembers().OfType<IMethodSymbol>())
            {
                if (method.IsStatic && method.Name.StartsWith("get_", StringComparison.Ordinal) && method.Parameters.Length == 1 &&
                    method.Parameters[0].Type is INamedTypeSymbol { Name: PointerPrefix, TypeArguments.Length: 1 } receiver &&
                    receiver.TypeArguments[0] is INamedTypeSymbol target &&
                    SymbolEqualityComparer.Default.Equals(target.OriginalDefinition, type))
                {
                    names.Add(method.Name.Substring("get_".Length));
                }
            }
        }

        return names;
    }

    private static List<string> Properties(Compilation compilation, INamedTypeSymbol type, HashSet<string> declared)
    {
        List<string> properties = [];
        string typeName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        bool foreign = !type.Locations.Any(location => location.IsInSource);
        bool header = IsHeaderShaped(type);

        foreach (IFieldSymbol field in type.GetMembers().OfType<IFieldSymbol>())
        {
            if (field.IsStatic || field.IsConst || field.IsImplicitlyDeclared)
                continue;

            string name = field.Name;

            if (IsBlank(name) || s_collisionNames.Contains(name) || !compilation.IsSymbolAccessibleWithin(field, compilation.Assembly) ||
                ShadowsPromotedMethod(type, name))
            {
                continue;
            }

            string accessor = $"{AddressPrefix}{name}";

            // A foreign type's accessors are visible in its metadata; a type of this package gets its accessors
            // from the TypeGenerator in this same run, for every field its source declares. A foreign field whose type
            // does not resolve is typed from an assembly this compilation does not reference, and no spelling of it
            // compiles here (see TypeName).
            if (foreign ? ContainsErrorType(field.Type) || !type.GetMembers(accessor).OfType<IMethodSymbol>().Any(method => method.IsStatic && method.ReturnsByRef)
                        : field.DeclaringSyntaxReferences.Length == 0)
            {
                continue;
            }

            string fieldType = TypeName(field.Type, type);
            string slot = $"{ReceiverName}.field({typeName}.{accessor})";

            // A readonly field (a zero-size layout field) has no writable slot of its own: its accessor answers the
            // shared one, which only the accessor reaches.
            if (!declared.Contains(name))
            {
                properties.Add(header
                    ? $"public {fieldType} {PropertyName(name)} {{ [{GetterAttributes}] get => {slot}; [{GetterAttributes}] set {{ {slot} = value; {ReceiverName}.OnHeaderStore(); }} }}"
                    : $"public ref {fieldType} {PropertyName(name)} {{ [{GetterAttributes}] get => ref {(field.IsReadOnly ? slot : Specialized(typeName, accessor, name))}; }}");
            }

            AddAddress(properties, type, typeName, fieldType, accessor, foreign, declared);
        }

        // Embedded structs, by their ref accessors (see the remarks).
        foreach (IPropertySymbol property in type.GetMembers().OfType<IPropertySymbol>())
        {
            if (property.IsStatic || !property.ReturnsByRef || property.IsIndexer || property.IsImplicitlyDeclared)
                continue;

            string name = property.Name;

            if (s_collisionNames.Contains(name) || !compilation.IsSymbolAccessibleWithin(property, compilation.Assembly) ||
                ShadowsPromotedMethod(type, name))
            {
                continue;
            }

            string accessor = $"{AddressPrefix}{name}";

            if (foreign ? ContainsErrorType(property.Type) || !type.GetMembers(accessor).OfType<IMethodSymbol>().Any(method => method.IsStatic && method.ReturnsByRef)
                        : property.DeclaringSyntaxReferences.Length == 0)
            {
                continue;
            }

            string propertyType = TypeName(property.Type, type);

            if (!declared.Contains(name))
                properties.Add($"public ref {propertyType} {PropertyName(name)} {{ [{GetterAttributes}] get => ref {Specialized(typeName, accessor, name)}; }}");

            AddAddress(properties, type, typeName, propertyType, accessor, foreign, declared);
        }

        return properties;
    }

    /// <summary>
    /// The field's ADDRESS property, <c>ж&lt;F&gt; Ꮡf</c> (parity seat #32): the converter's spelling of Go's
    /// <c>&amp;p.f</c>, the field's view over the struct's one accessor token <c>T.Ꮡf‿</c>.
    /// </summary>
    /// <remarks>
    /// The token is a <c>static readonly FieldRefFunc&lt;T, F&gt;</c> the TypeGenerator emits beside the accessor, in
    /// the struct's own assembly, so every site in every assembly hands golib's view cache one delegate object and its
    /// compare is <c>ReferenceEquals</c> on both runtimes. A foreign assembly built before the token has none in its
    /// metadata; its address property passes the accessor's method group, today's <c>.of(T.Ꮡf)</c> delegate, which the
    /// cache still matches by its method. The property is decided apart from the field's own: a referenced package
    /// built before the token declares <c>f</c> and not <c>Ꮡf</c>. A name on the collision list (<c>Ꮡelem</c>, ж&lt;T&gt;'s
    /// own method) is never declared: an instance member wins over an extension, and the site would bind the member.
    /// </remarks>
    private static void AddAddress(List<string> properties, INamedTypeSymbol type, string typeName, string fieldType, string accessor, bool foreign, HashSet<string> declared)
    {
        if (s_collisionNames.Contains(accessor) || declared.Contains(accessor))
            return;

        string token = $"{accessor}{FieldAccessTokenSuffix}";

        if (foreign && !type.GetMembers(token).OfType<IFieldSymbol>().Any(field => field.IsStatic))
            token = accessor;

        properties.Add($"public global::go.{PointerPrefix}<{fieldType}> {accessor} {{ [{GetterAttributes}] get => {ReceiverName}.of({typeName}.{token}); }}");
    }

    // The accessors' attributes. A getter is go2cs machinery, not Go code: inlined at Release, and where the JIT keeps
    // its frame (a Debug build, a refused inline) the runtime's frame filter skips it, so a traceback and
    // runtime.Caller see the user's frame either way (design Q4). Two things skip it, each alone sufficient (measured
    // by PointerFieldSelectors' nil read in a Debug build): its class is not a package class, and its [GeneratedCode].
    private static readonly string GetterAttributes =
        $"{GeneratedCodeAttribute}, global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)";

    // The specialized body (design Q1, COORD 2026-10-27): a managed box reaches the field directly, `ref ʗp.Value.f`,
    // with Value's own nil check and no delegate; a NativeBox<T> answers it at its Go offset through the accessor,
    // where a struct whose CLR layout is not Go's holds the wrong bytes in its own slot (ж<T>.field's two arms).
    private static string Specialized(string typeName, string accessor, string name) =>
        $"{ReceiverName} is global::go.NativeBox<{typeName}> ? ref {ReceiverName}.field({typeName}.{accessor}) : ref {ReceiverName}.Value.{PropertyName(name)}";

    /// <summary>
    /// Whether a field <paramref name="name"/> of <paramref name="type"/> shadows a method an embedded type promotes,
    /// as the converter's <c>shadowsPromotedMethod</c> reads it from Go's types.
    /// </summary>
    /// <remarks>
    /// Go selects the field, the shallower member, but the box carries the promoted method as an extension member, and a
    /// property of the same name beside it makes every <c>p.name</c> ambiguous: abi.ArrayType's field <c>Len</c>
    /// against the <c>Len</c> method its embedded abi.Type promotes. The converter keeps such a site's box form, so the
    /// property would serve nothing. An embed is a Go embedded field (<see cref="MemberMarkers.IsGoEmbedded"/>), a value
    /// or a pointer; its type's methods are extension members of its package class over the value, a ref to it or its
    /// box, and its own embeds promote theirs in turn.
    /// </remarks>
    internal static bool ShadowsPromotedMethod(INamedTypeSymbol type, string name)
    {
        HashSet<INamedTypeSymbol> seen = new(SymbolEqualityComparer.Default);

        return Walk(type);

        bool Walk(INamedTypeSymbol owner)
        {
            if (!seen.Add(owner.OriginalDefinition))
                return false;

            foreach (IFieldSymbol field in owner.GetMembers().OfType<IFieldSymbol>())
            {
                if (field.IsStatic || !MemberMarkers.IsGoEmbedded(field) || !StructDeclarationSyntaxExtensions.IsPromotingEmbedType(field.Type))
                    continue;

                ITypeSymbol embedded = field.Type is INamedTypeSymbol { Name: PointerPrefix, TypeArguments: [var element] } ? element : field.Type;

                if (embedded is INamedTypeSymbol named && (HasMethod(named, name) || Walk(named)))
                    return true;
            }

            return false;
        }

        static bool HasMethod(INamedTypeSymbol type, string name) =>
            type.ContainingType is { } package && package.GetMembers(name).OfType<IMethodSymbol>().Any(method =>
                method.IsExtensionMethod && method.Parameters.Length > 0 && IsReceiverOf(method.Parameters[0].Type, type));

        static bool IsReceiverOf(ITypeSymbol receiver, INamedTypeSymbol type)
        {
            if (receiver is INamedTypeSymbol { Name: PointerPrefix, TypeArguments: [var element] })
                receiver = element;

            return SymbolEqualityComparer.Default.Equals(receiver.OriginalDefinition, type.OriginalDefinition);
        }
    }

    /// <summary>
    /// Whether <paramref name="type"/> is shaped as a Go slice or string header: exactly an <c>unsafe.Pointer</c> field
    /// (the one class implementing golib's <c>IUnsafePointer</c>) and then one or two <c>int</c> fields, in declaration
    /// order, as golib's SliceHeaderBox and StringHeaderBox require.
    /// </summary>
    internal static bool IsHeaderShaped(INamedTypeSymbol type)
    {
        IFieldSymbol[] fields = type.GetMembers().OfType<IFieldSymbol>()
            .Where(field => !field.IsStatic && !field.IsConst && !field.IsImplicitlyDeclared)
            .ToArray();

        if (fields.Length is not (2 or 3))
            return false;

        if (fields[0].Type is not INamedTypeSymbol { TypeKind: TypeKind.Class } pointer ||
            !pointer.AllInterfaces.Any(face => face.ToDisplayString() == "go.IUnsafePointer"))
        {
            return false;
        }

        return fields.Skip(1).All(field => field.Type.SpecialType == SpecialType.System_IntPtr);
    }

    // A reserved word takes its `@`, and so do the two contextual words that mean something inside an extension
    // block's property: `extension` and `field`.
    private static string PropertyName(string name) =>
        name is "extension" or "field" ? $"@{name}" : EscapeCsKeyword(name);

    // Go's blank field and the converter's uniquified repeats of it (`_`, `__`, ...) have no address and no accessor.
    private static bool IsBlank(string name) => name.Length > 0 && name.All(c => c == '_');

    private static string TypeParameterList(INamedTypeSymbol type) =>
        type.TypeParameters.Length == 0 ? "" : $"<{string.Join(", ", type.TypeParameters.Select(parameter => EscapeCsKeyword(parameter.Name)))}>";

    private static string Constraints(INamedTypeSymbol type)
    {
        StringBuilder clauses = new();

        foreach (ITypeParameterSymbol parameter in type.TypeParameters)
        {
            List<string> constraints = [];

            if (parameter.HasUnmanagedTypeConstraint)
                constraints.Add("unmanaged");
            else if (parameter.HasValueTypeConstraint)
                constraints.Add("struct");
            else if (parameter.HasReferenceTypeConstraint)
                constraints.Add("class");
            else if (parameter.HasNotNullConstraint)
                constraints.Add("notnull");

            constraints.AddRange(parameter.ConstraintTypes.Select(constraint => TypeName(constraint, type)));

            if (parameter.HasConstructorConstraint && !parameter.HasValueTypeConstraint)
                constraints.Add("new()");

            if (constraints.Count > 0)
                clauses.Append($" where {EscapeCsKeyword(parameter.Name)} : {string.Join(", ", constraints)}");
        }

        return clauses.ToString();
    }

    /// <summary>
    /// A type as the generated extension class, outside the package class, must spell it.
    /// </summary>
    /// <remarks>
    /// A resolved type is fully qualified. An UNRESOLVED one, at any depth (a type argument, an array element), is a
    /// type another generator emits into the package class in this same run, which this generator cannot see: the
    /// ImplementGenerator's constraint proxy (SelfRefGenerics' `gnodeжGraphNode`). The struct's own declaration names
    /// it bare from inside the package class, so it is rooted at the package class that holds <paramref name="owner"/>.
    /// That holds only for an owner declared in this compilation's SOURCE. A foreign owner's field types come from
    /// metadata, where an unresolved one is a type of an assembly this compilation does not reference (httptest.Server's
    /// TLS, a *tls.Config, in encoding/json's tests): rooted locally it named a type that does not exist (seat #45), and
    /// <see cref="Properties"/> gives such a field no property.
    /// </remarks>
    private static string TypeName(ITypeSymbol type, INamedTypeSymbol owner)
    {
        if (!ContainsErrorType(type))
            return type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        switch (type)
        {
            case IArrayTypeSymbol array:
                return $"{TypeName(array.ElementType, owner)}[{new string(',', array.Rank - 1)}]";
            case INamedTypeSymbol { TypeKind: TypeKind.Error } error:
                return $"{PackageClassName(owner)}.{EscapeCsKeyword(error.Name)}{TypeArguments(error, owner)}";
            case INamedTypeSymbol named:
                string container = named.ContainingType is { } containingType
                    ? TypeName(containingType, owner)
                    : named.ContainingNamespace is { IsGlobalNamespace: false } ns
                        ? $"global::{ns.ToDisplayString()}"
                        : "global::";
                return $"{(container == "global::" ? container : container + ".")}{EscapeCsKeyword(named.Name)}{TypeArguments(named, owner)}";
            default:
                return type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        }
    }

    private static string TypeArguments(INamedTypeSymbol type, INamedTypeSymbol owner) =>
        type.TypeArguments.Length == 0 ? "" : $"<{string.Join(", ", type.TypeArguments.Select(argument => TypeName(argument, owner)))}>";

    private static bool ContainsErrorType(ITypeSymbol type) => type switch
    {
        IArrayTypeSymbol array => ContainsErrorType(array.ElementType),
        INamedTypeSymbol { TypeKind: TypeKind.Error } => true,
        INamedTypeSymbol named => named.TypeArguments.Any(ContainsErrorType),
        _ => false
    };

    // The outermost type that holds owner, its package class (the converter nests every package type in it).
    private static string PackageClassName(INamedTypeSymbol owner)
    {
        INamedTypeSymbol packageClass = owner;

        while (packageClass.ContainingType is { } containingType)
            packageClass = containingType;

        return packageClass.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    }

    private static HashSet<string> LoadCollisionNames()
    {
        HashSet<string> names = new(StringComparer.Ordinal);

        using Stream? stream = typeof(FieldAccessGenerator).Assembly.GetManifestResourceStream("go2cs.fieldAccessCollisions.txt");

        if (stream is null)
            throw new InvalidOperationException("go2cs-gen was built without its embedded fieldAccessCollisions.txt");

        using StreamReader reader = new(stream);

        while (reader.ReadLine() is { } line)
        {
            int comment = line.IndexOf('#');
            string name = (comment >= 0 ? line.Substring(0, comment) : line).Trim();

            if (name.Length > 0)
                names.Add(name);
        }

        return names;
    }
}

/// <summary>
/// Collects the class declarations that carry a <c>GoFieldAccess</c> record, and notes whether any assembly-level record
/// exists, by name; the generator confirms the attribute by symbol.
/// </summary>
public sealed class FieldAccessRecordFinder : ISyntaxReceiver
{
    public List<ClassDeclarationSyntax> Candidates { get; } = [];

    /// <summary>Whether an <c>[assembly: GoFieldAccess(…)]</c> record was seen (package_info.cs).</summary>
    public bool AssemblyRecords { get; private set; }

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is AttributeListSyntax { Target.Identifier.ValueText: "assembly" } assemblyList)
        {
            AssemblyRecords |= assemblyList.Attributes.Any(IsRecord);
            return;
        }

        if (syntaxNode is not ClassDeclarationSyntax { AttributeLists.Count: > 0 } declaration)
            return;

        if (declaration.AttributeLists.Any(list => list.Attributes.Any(IsRecord)))
            Candidates.Add(declaration);
    }

    private static bool IsRecord(AttributeSyntax attribute) =>
        (attribute.Name is QualifiedNameSyntax qualified ? qualified.Right.Identifier.ValueText : attribute.Name.ToString()) is "GoFieldAccess" or "GoFieldAccessAttribute";
}
