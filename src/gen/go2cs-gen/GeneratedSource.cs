// GeneratedSource.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace go2cs;

/// <summary>
/// The one door every generator's file goes through: <c>AddGeneratedSource</c> adds it with the whitespace half of the
/// project's C# style applied (docs/PLAN-generated-code-format.md, classes D to G). Line endings are CRLF, no line ends
/// in a space or tab, no two blank lines are adjacent, the file neither starts nor ends with a blank line, and it ends
/// with exactly one line break.
/// </summary>
/// <remarks>
/// The templates stay as they are written: a raw-string line that only holds interpolations, or a blank line
/// indented past its closing quotes, leaves a whitespace-only line, and this pass removes it in one place rather than
/// contorting every template around it. The pass is lexical, so it could change a program only where a line break
/// falls inside a string literal, which takes a verbatim or raw literal. A text that contains either opener is added
/// unchanged, and GenTests' format guard names such a file if it leaves the style.
/// </remarks>
public static class GeneratedSource
{
    /// <summary>
    /// Adds <paramref name="source"/> as <paramref name="hintName"/>, normalized by <see cref="Normalize"/>.
    /// </summary>
    public static void AddGeneratedSource(this GeneratorExecutionContext context, string hintName, string source) =>
        context.AddSource(hintName, Normalize(source));

    /// <summary>
    /// Adds <paramref name="source"/> as <paramref name="hintName"/>, normalized by <see cref="Normalize"/>.
    /// </summary>
    public static void AddGeneratedSource(this SourceProductionContext context, string hintName, string source) =>
        context.AddSource(hintName, Normalize(source));

    /// <summary>
    /// M:N SPIKE (R5b, B2), never lands. Adds <paramref name="source"/> exactly as written: a twin is a copy of a
    /// source method under a <c>#line</c> directive, and it keeps the source's line count only if no blank run is folded.
    /// </summary>
    public static void AddLineExactSource(this GeneratorExecutionContext context, string hintName, string source) =>
        context.AddSource(hintName, source);

    /// <summary>
    /// The metadata name of golib's <c>GoExtensionHostAttribute</c>; a compilation that cannot resolve it gets no list.
    /// </summary>
    public const string ExtensionHostAttributeMetadataName = "go.GoExtensionHostAttribute";

    /// <summary>
    /// The assembly attribute that marks this assembly's extension-host list complete, written once per assembly by
    /// <see cref="ExtensionHostListGenerator"/>.
    /// </summary>
    public const string ExtensionHostListDeclaration = "[assembly: global::go.GoExtensionHostList(HomesListed = true)]";

    /// <summary>
    /// The list marker an assembly gets when its golib's <c>GoExtensionHostListAttribute</c> predates
    /// <c>HomesListed</c>: its hosts are then listed without their homes.
    /// </summary>
    public const string ExtensionHostListDeclarationWithoutHomes = "[assembly: global::go.GoExtensionHostList]";

    /// <summary>
    /// Whether <paramref name="compilation"/>'s golib can take each host's homes
    /// (<c>GoExtensionHostListAttribute.HomesListed</c>, docs/phase4/DESIGN-extension-receiver-scope.md).
    /// </summary>
    public static bool CanListExtensionHomes(Compilation compilation) =>
        compilation.GetTypeByMetadataName("go.GoExtensionHostListAttribute")?.GetMembers("HomesListed").Length > 0;

    /// <summary>
    /// The assembly attribute that lists <paramref name="fullyQualifiedTypeName"/> (a <c>global::</c> name) as a class
    /// that declares extension methods, which golib's extension-method scan reads under Native AOT
    /// (docs/phase4/DESIGN-extension-host-list.md). A generator that adds such a class writes this line in the same
    /// file, before its namespace: C# takes assembly attributes only there (CS1730), and only that file can name a
    /// <c>file</c> class.
    /// </summary>
    public static string ExtensionHostDeclaration(string fullyQualifiedTypeName) =>
        $"[assembly: global::go.GoExtensionHost(typeof({fullyQualifiedTypeName}))]";

    /// <summary>
    /// The assembly attribute that lists <paramref name="fullyQualifiedTypeName"/> as a class that declares extension
    /// methods with <paramref name="receivers"/>, naming the homes beyond <paramref name="compilation"/>'s own assembly
    /// those receivers belong to (<see cref="ExtensionHomes"/>): a class whose receivers are all its own assembly's types
    /// is listed as <see cref="ExtensionHostDeclaration(string)"/> lists it.
    /// </summary>
    public static string ExtensionHostDeclaration(string fullyQualifiedTypeName, IEnumerable<ITypeSymbol> receivers, Compilation compilation)
    {
        (string[] homes, bool anyHome) = ExtensionHomes(receivers, compilation);

        if (anyHome)
            return $"[assembly: global::go.GoExtensionHost(typeof({fullyQualifiedTypeName}), AnyHome = true)]";

        if (homes.Length == 0)
            return ExtensionHostDeclaration(fullyQualifiedTypeName);

        return $"[assembly: global::go.GoExtensionHost(typeof({fullyQualifiedTypeName}), Homes = new[] {{ {string.Join(", ", homes.Select(home => $"typeof({home})"))} }})]";
    }

    /// <summary>
    /// The homes of extension methods with <paramref name="receivers"/> beyond <paramref name="compilation"/>'s own
    /// assembly, as golib's <c>TypeExtensions.ExtensionHome</c> reads them at run time: the assembly of the receiver,
    /// read through golib's <c>ж&lt;X&gt;</c> box to <c>X</c>, and of a generic type's definition. One type names each
    /// home, the first by name, and the homes are ordered by assembly name. <c>anyHome</c> is whether a receiver belongs
    /// to no assembly (a type parameter, an array, a pointer).
    /// </summary>
    public static (string[] homes, bool anyHome) ExtensionHomes(IEnumerable<ITypeSymbol> receivers, Compilation compilation)
    {
        SortedDictionary<string, string> homes = new(StringComparer.Ordinal);
        bool anyHome = false;

        foreach (ITypeSymbol receiver in receivers)
        {
            ITypeSymbol type = receiver;

            while (type is INamedTypeSymbol { Name: "ж", Arity: 1, ContainingNamespace: { Name: "go", ContainingNamespace.IsGlobalNamespace: true } } box &&
                   box.ContainingAssembly?.Name == "golib")
                type = box.TypeArguments[0];

            if (type.TypeKind == TypeKind.Dynamic)
                type = compilation.ObjectType;

            if (type is not INamedTypeSymbol named || named.TypeKind == TypeKind.Error)
            {
                anyHome |= type.TypeKind != TypeKind.Error;
                continue;
            }

            IAssemblySymbol? home = named.OriginalDefinition.ContainingAssembly;

            if (home is null || SymbolEqualityComparer.Default.Equals(home, compilation.Assembly))
                continue;

            INamedTypeSymbol definition = named.OriginalDefinition;
            string name = (definition.IsGenericType ? definition.ConstructUnboundGenericType() : definition).ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

            if (!homes.TryGetValue(home.Name, out string? first) || StringComparer.Ordinal.Compare(name, first) < 0)
                homes[home.Name] = name;
        }

        return ([.. homes.Values], anyHome);
    }

    /// <summary>
    /// <paramref name="source"/> with CRLF line endings, no trailing whitespace, no run of blank lines, no leading or
    /// trailing blank line, and one final line break; unchanged when it holds a verbatim or raw string literal.
    /// </summary>
    public static string Normalize(string source)
    {
        if (source.Contains("@\"") || source.Contains("@$\"") || source.Contains("\"\"\""))
            return source;

        string[] lines = source.Replace("\r\n", "\n").Split('\n');
        StringBuilder result = new(source.Length + 64);
        bool pendingBlank = false;

        foreach (string line in lines)
        {
            string trimmed = line.TrimEnd(' ', '\t', '\r');

            if (trimmed.Length == 0)
            {
                pendingBlank = result.Length > 0;
                continue;
            }

            if (pendingBlank)
                result.Append("\r\n");

            result.Append(trimmed).Append("\r\n");
            pendingBlank = false;
        }

        return result.ToString();
    }
}
