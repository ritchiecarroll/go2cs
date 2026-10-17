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
/// <c>[GoFieldAccess(...)]</c> records of a compilation list, so a field reached through a pointer reinterpreted from
/// <c>unsafe.Pointer</c> reads as Go does: <c>buf.SubstituteNameOffset = x</c>.
/// </summary>
/// <remarks>
/// <para>
/// Each property is a C# 14 ref-returning extension property over golib's <c>ж&lt;T&gt;.field</c>, which answers the
/// field at its Go offset when the pointer aliases native memory and at the struct's own slot otherwise, with no box
/// in between. Being a <c>ref</c>, it serves a read, a write, a compound assignment, a nested field
/// (<c>p.In.X = 7</c>) and a <c>ref</c> argument alike.
/// </para>
/// <para>
/// The converter puts a record on the package class of each FILE with a site, naming that file's struct types, and
/// the properties go into the USING package's own compilation, in an <c>internal</c> class: an extension block may extend a foreign type, an exported field's
/// <c>Ꮡ</c> accessor is public, and no other assembly sees the class, so two packages extending one type never meet.
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
/// </remarks>
[Generator]
public class FieldAccessGenerator : ISourceGenerator
{
    private const string AttributeName = "go.GoFieldAccessAttribute";

    private const string ReceiverName = "ʗp";

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
        if (context.SyntaxReceiver is not FieldAccessRecordFinder { Candidates.Count: > 0 } finder)
            return;

        List<INamedTypeSymbol> types = RecordedTypes(context.Compilation, finder.Candidates);

        if (types.Count > 0)
            context.AddSource("GoFieldAccess.g.cs", Source(context.Compilation, types));
    }

    // The struct types the records list, each once, generic ones by their open definition. A record sits on a
    // package class, which is partial across the package's files, so its symbol carries every file's records.
    internal static List<INamedTypeSymbol> RecordedTypes(Compilation compilation, IEnumerable<ClassDeclarationSyntax> candidates)
    {
        List<INamedTypeSymbol> types = [];
        HashSet<INamedTypeSymbol> seen = new(SymbolEqualityComparer.Default);
        HashSet<INamedTypeSymbol> classes = new(SymbolEqualityComparer.Default);
        List<AttributeData> records = [];

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

        foreach (INamedTypeSymbol type in types)
        {
            List<string> properties = Properties(compilation, type);

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

    private static List<string> Properties(Compilation compilation, INamedTypeSymbol type)
    {
        List<string> properties = [];
        string typeName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        bool foreign = !type.Locations.Any(location => location.IsInSource);

        foreach (IFieldSymbol field in type.GetMembers().OfType<IFieldSymbol>())
        {
            if (field.IsStatic || field.IsConst || field.IsImplicitlyDeclared)
                continue;

            string name = field.Name;

            if (IsBlank(name) || s_collisionNames.Contains(name) || !compilation.IsSymbolAccessibleWithin(field, compilation.Assembly))
                continue;

            string accessor = $"{AddressPrefix}{name}";

            // A foreign type's accessors are visible in its metadata; a type of this package gets its accessors
            // from the TypeGenerator in this same run, for every field its source declares.
            if (foreign ? !type.GetMembers(accessor).OfType<IMethodSymbol>().Any(method => method.IsStatic && method.ReturnsByRef)
                        : field.DeclaringSyntaxReferences.Length == 0)
            {
                continue;
            }

            string fieldType = field.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

            properties.Add($"public ref {fieldType} {PropertyName(name)} => ref {ReceiverName}.field({typeName}.{accessor});");
        }

        // Embedded structs, by their ref accessors (see the remarks).
        foreach (IPropertySymbol property in type.GetMembers().OfType<IPropertySymbol>())
        {
            if (property.IsStatic || !property.ReturnsByRef || property.IsIndexer || property.IsImplicitlyDeclared)
                continue;

            string name = property.Name;

            if (s_collisionNames.Contains(name) || !compilation.IsSymbolAccessibleWithin(property, compilation.Assembly))
                continue;

            string accessor = $"{AddressPrefix}{name}";

            if (foreign ? !type.GetMembers(accessor).OfType<IMethodSymbol>().Any(method => method.IsStatic && method.ReturnsByRef)
                        : property.DeclaringSyntaxReferences.Length == 0)
            {
                continue;
            }

            string propertyType = property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

            properties.Add($"public ref {propertyType} {PropertyName(name)} => ref {ReceiverName}.field({typeName}.{accessor});");
        }

        return properties;
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

            constraints.AddRange(parameter.ConstraintTypes.Select(constraint => constraint.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)));

            if (parameter.HasConstructorConstraint && !parameter.HasValueTypeConstraint)
                constraints.Add("new()");

            if (constraints.Count > 0)
                clauses.Append($" where {EscapeCsKeyword(parameter.Name)} : {string.Join(", ", constraints)}");
        }

        return clauses.ToString();
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
/// Collects the class declarations that carry a <c>GoFieldAccess</c> record, by name; the generator confirms the
/// attribute by symbol.
/// </summary>
public sealed class FieldAccessRecordFinder : ISyntaxReceiver
{
    public List<ClassDeclarationSyntax> Candidates { get; } = [];

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is not ClassDeclarationSyntax { AttributeLists.Count: > 0 } declaration)
            return;

        foreach (AttributeListSyntax list in declaration.AttributeLists)
        {
            foreach (AttributeSyntax attribute in list.Attributes)
            {
                string name = attribute.Name is QualifiedNameSyntax qualified ? qualified.Right.Identifier.ValueText : attribute.Name.ToString();

                if (name is "GoFieldAccess" or "GoFieldAccessAttribute")
                {
                    Candidates.Add(declaration);
                    return;
                }
            }
        }
    }
}
