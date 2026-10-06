// ImplicitConvGenerator.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

//#define DEBUG_GENERATOR

using System;
using System.Collections.Generic;
using System.Linq;
using go2cs.Templates.ImplicitConv;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static go2cs.Common;
using static go2cs.Symbols;

#if DEBUG_GENERATOR
using System.Diagnostics;
#endif

namespace go2cs;

[Generator]
public class ImplicitConvGenerator : ISourceGenerator
{
    private const string Namespace = "go";
    private const string AttributeName = "GoImplicitConv";
    private const string FullAttributeName = $"{Namespace}.{AttributeName}Attribute<TSource, TTarget>";

    // Fully-qualified, keyword-escaped, special-types display format used to reference a FOREIGN named
    // type unambiguously from a generated file (e.g. `global::go.@internal.abi_package.NameOff`) and to
    // render an underlying basic as its C# keyword (`int`, `ulong`).
    private static readonly SymbolDisplayFormat s_qualifiedFormat = new(
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes | SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers);

    public void Initialize(GeneratorInitializationContext context)
    {
#if DEBUG_GENERATOR
        if (!Debugger.IsAttached)
            Debugger.Launch();
#endif

        // Register to find "GoImplicitConv" on assembly attribute declarations
        context.RegisterForSyntaxNotifications(() => new AssemblyAttributeFinder(FullAttributeName));
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxContextReceiver is not AssemblyAttributeFinder { HasAttributes: true } attributeFinder)
            return;

        HashSet<string> emittedHintNames = new(StringComparer.OrdinalIgnoreCase);
        HashSet<string> emittedConversions = new(StringComparer.Ordinal);

        foreach ((AttributeSyntax attributeSyntax, GeneratorSyntaxContext syntaxContext, CompilationUnitSyntax compilationUnit, FileScopedNamespaceDeclarationSyntax? namespaceSyntax) in attributeFinder.TargetAttributes)
        {
            SyntaxTree syntaxTree = attributeSyntax.SyntaxTree;
            SemanticModel semanticModel = context.Compilation.GetSemanticModel(syntaxTree);

            string packageNamespace = GetNamespace(namespaceSyntax) ?? Namespace;
            // A record this generator cannot use costs its own conversion only, never the whole
            // generator's output (a throw contributed nothing). A WARNING: a missing conversion
            // operator cannot pass silently, it fails to compile at the conversion it was for.
            string? packageClassName = GetFirstClassName(compilationUnit);

            if (packageClassName is null)
            {
                context.ReportDiagnostic(Diagnostic.Create(GeneratorDiagnostics.MalformedRecord, attributeSyntax.GetLocation(), attributeSyntax.ToString(), "its file declares no package class to hold the conversion"));
                continue;
            }

            string packageName = packageClassName.EndsWith(PackageSuffix) ? packageClassName[..^PackageSuffix.Length] : packageClassName;

            string[] usingStatements = GetFullyQualifiedUsingStatements(syntaxTree, semanticModel);

            // Extract generic type arguments from "GoImplicitConv"
            (ITypeSymbol? sourceType, ITypeSymbol? targetType) = attributeSyntax.Get2GenericTypeArguments(syntaxContext);

            if (sourceType is null || targetType is null)
            {
                context.ReportDiagnostic(Diagnostic.Create(GeneratorDiagnostics.MalformedRecord, attributeSyntax.GetLocation(), attributeSyntax.ToString(), "it must name two resolvable type arguments"));
                continue;
            }

            if (sourceType.TypeKind != TypeKind.Struct)
                // Source is not a struct (e.g. a defined type over a non-struct underlying). This one
                // conversion can't be generated; skip it rather than aborting ALL of this generator's
                // output for the whole compilation.
                continue;

            // Keyword-escape the type names: a Go defined type named after a C# keyword (`type short
            // int16` in github.com/mattn/go-colorable) is declared `partial struct @short`, so the
            // operator's host, return, and `new` must use `@short` too — the raw symbol name `short`
            // yields `partial struct short`, which parses the operator into the enclosing static class
            // (CS0715/CS0057). EscapeCsKeyword is a no-op for every non-keyword (and generic) name.
            string sourceTypeName = EscapeCsKeyword(sourceType.GetFullTypeName());
            string targetTypeName = EscapeCsKeyword(targetType.GetFullTypeName());

            // Get the attribute's argument values, if defined
            (string name, string value)[] arguments = attributeSyntax.GetArgumentValues();
            bool inverted = bool.Parse(arguments.FirstOrDefault(arg => arg.name.Equals("Inverted")).value?.Trim() ?? "false");
            bool indirect = bool.Parse(arguments.FirstOrDefault(arg => arg.name.Equals("Indirect")).value?.Trim() ?? "false");
            string? valueType = arguments.FirstOrDefault(arg => arg.name.Equals("ValueType")).value?.Trim();

            List<(string typeName, string memberName)> structMembers;
            bool foreignStructTarget = false;

            if (string.IsNullOrWhiteSpace(valueType))
            {
                StructDeclarationSyntax? structDeclaration = GetStructDeclaration(syntaxContext, targetTypeName);

                // A struct target declared in a REFERENCED assembly has no declaration in syntax. Go
                // converts between struct types only when their fields are identical in name, type and
                // order, so a LOCAL source's declaration enumerates the same members, in the order the
                // target's constructor takes them: `structs.AssignB(struct{ A int }{3})`, whose lifted
                // source is local (go-cmp's teststructs, CS0030 when this record was skipped).
                if (structDeclaration is null && !inverted && targetType.TypeKind == TypeKind.Struct &&
                    !SymbolEqualityComparer.Default.Equals(targetType.ContainingAssembly, context.Compilation.Assembly) &&
                    SymbolEqualityComparer.Default.Equals(sourceType.ContainingAssembly, context.Compilation.Assembly))
                {
                    structDeclaration = GetStructDeclaration(syntaxContext, sourceTypeName);
                    foreignStructTarget = structDeclaration is not null;
                }

                if (structDeclaration is null)
                    // The target type has no local struct declaration to enumerate members from — e.g. a
                    // golib generic box such as `ж<Type>` produced by an embedded cross-package pointer.
                    // Skip just this one conversion rather than throwing, which would suppress ALL of this
                    // generator's output for the whole compilation.
                    continue;

                structMembers = structDeclaration
                    .GetStructMembers(context.Compilation, true)
                    .Select(member => (member.typeName, member.memberName))
                    .ToList();
            }
            else
            {
                valueType = valueType![1..^1];
                structMembers = [];
                targetTypeName = EscapeCsKeyword(targetType.GetFullTypeName(true));
            }

            // Cross-package numeric conversion whose operator constructs a FOREIGN named-numeric type
            // (declared in another assembly). Two ways this arises and both are broken by default:
            //   • foreign SOURCE via a local alias (`GoImplicitConv<nameOff, Δhex>(Inverted = true)`,
            //     where runtime's `nameOff` aliases `internal/abi.NameOff`): the operator is hosted in
            //     `partial struct {sourceTypeName}` — a phantom LOCAL type, since a foreign type can't
            //     be extended here (CS1729) — and constructs the foreign source.
            //   • foreign TARGET via a qualified reference (`GoImplicitConv<Hx, pkg.Off>`): the host is
            //     local, but the body still `new pkg.Off(...)`s a foreign type.
            // In both, casting `src.Value` straight to the foreign named type has no cross-assembly route
            // (`ulong`→`NameOff` ⇒ CS0030). The constructed type is whichever side the operator builds
            // via `new` (the LH type: source when inverted, else target). When it is foreign, construct
            // it through its UNDERLYING basic — `new global::…NameOff((int)src.Value)` — mirroring the
            // converter's through-underlying inline cast; and if the default host (the source type) is
            // itself foreign, relocate the operator into the LOCAL type so it can be declared at all.
            string? hostTypeNameOverride = null, lhTypeNameOverride = null, rhTypeNameOverride = null, convExprOverride = null;

            // A foreign struct target is returned and constructed by its QUALIFIED name: its simple
            // name resolves only inside its own package class, not in this one (CS0246).
            if (foreignStructTarget)
                lhTypeNameOverride = targetType.ToDisplayString(s_qualifiedFormat);

            if (!string.IsNullOrWhiteSpace(valueType))
            {
                ITypeSymbol constructedType = inverted ? sourceType : targetType; // the type built via `new`
                bool constructedIsForeign = !SymbolEqualityComparer.Default.Equals(constructedType.ContainingAssembly, context.Compilation.Assembly);

                if (constructedIsForeign)
                {
                    string? constructedUnderlying = GetUnderlyingBasicName(constructedType);

                    if (constructedUnderlying is not null)
                    {
                        string qualifiedConstructed = constructedType.ToDisplayString(s_qualifiedFormat);

                        lhTypeNameOverride = qualifiedConstructed;
                        convExprOverride = $"new {qualifiedConstructed}(({constructedUnderlying})src.Value)";

                        // The default host is the SOURCE type's partial struct. If that is itself
                        // foreign it can't be extended here (CS1729 phantom); relocate into the LOCAL
                        // target type and reference the param (RH) type fully-qualified.
                        bool sourceIsForeign = !SymbolEqualityComparer.Default.Equals(sourceType.ContainingAssembly, context.Compilation.Assembly);
                        bool targetIsLocal = SymbolEqualityComparer.Default.Equals(targetType.ContainingAssembly, context.Compilation.Assembly);

                        if (sourceIsForeign && targetIsLocal)
                        {
                            hostTypeNameOverride = targetType.Name;
                            rhTypeNameOverride = targetType.ToDisplayString(s_qualifiedFormat);
                        }
                    }
                }
            }

            // A MIXED-accessibility local pair (public ΔKind ↔ internal flag — reflect's
            // GoImplicitConv<ΔKind, flag>(Inverted)): C# operators are necessarily public, so
            // hosting in the MORE accessible type makes the less-accessible parameter fail
            // CS0057. Relocate into the LESS accessible side — a public operator inside an
            // internal struct is legal, and both operand types are visible there.
            // Accessibility comes from EffectiveScopeIsPublic: a modifier the converter WROTE wins,
            // and the GO export rule (GetScope on the Δ-stripped name) is the fallback for a bare
            // [GoType] partial, whose public/internal modifier lives on the TypeGenerator's OWN
            // output — invisible to a single-pass sibling generator, which reads Private for it.
            // The written half is what a lifted function-local type needs: `TestExported_BigP`
            // reads public off its enclosing Test while the declaration is `internal`, so the name
            // alone put the operator on the wrong side of the pair.
            if (hostTypeNameOverride is null &&
                EffectiveScopeIsPublic(sourceType, GetSimpleName(sourceType.Name, dropCollisionPrefix: true)) &&
                !EffectiveScopeIsPublic(targetType, GetSimpleName(targetType.Name, dropCollisionPrefix: true)) &&
                SymbolEqualityComparer.Default.Equals(targetType.ContainingAssembly, context.Compilation.Assembly))
            {
                hostTypeNameOverride = targetType.Name;
            }

            // A mixed-accessibility pair whose LESS accessible side lives in ANOTHER assembly has no
            // legal form at all, so skip it rather than emit an operator that cannot compile. The
            // relocation above is the only remedy for a mixed pair — a C# user-defined conversion
            // operator is necessarily public AND must be declared in one of its two operand types —
            // and a FOREIGN type cannot host anything. Hosting in the local, more accessible side then
            // exposes a type less accessible than the operator: CS0056 when the foreign side is the
            // return type, CS0057 when it is the parameter, so neither direction is expressible.
            // Reachable only under the -tests white-box model, where a package's own `_test.go`
            // declares an EXPORTED defined type over an UNEXPORTED production one — time's
            // export_test.go `type RuleKind int` beside zoneinfo.go's `type ruleKind int`, which
            // become public RuleKind in the test assembly and internal ruleKind in the referenced
            // production assembly. Nothing is lost by skipping: the converter renders such a
            // conversion site as an explicit through-underlying cast (`(RuleKind)(nint)r.kind`),
            // which needs no operator at all.
            if (hostTypeNameOverride is null && ForeignSideIsLessAccessible(sourceType, targetType, context.Compilation.Assembly))
                continue;

            // The operator's body casts `src.Value` to the constructed type; a uintptr-BACKED
            // src wrapper's Value is the golib uintptr STRUCT, and struct→ΔKind chains two user
            // conversions (CS0030 — reflect's flag→ΔKind). Hop through nuint. The backing kind
            // comes from the src side's [GoType("num:uintptr")] tag — the generated Value
            // property is invisible to a single-pass sibling generator.
            //
            // `valueType` is the CONSTRUCTED type's backing primitive, so it is the cast target, not
            // the type to construct — the type to construct is the LH type, exactly as in the
            // template's default body and in the local-pair arm below. (It named the constructed
            // type itself until the converter was corrected to record the primitive; reading it as a
            // type name here would have emitted `new nuint((nuint)(nuint)src.Value)` for reflect's
            // `ΔKind`←`flag`.)
            if (convExprOverride is null && !string.IsNullOrWhiteSpace(valueType))
            {
                ITypeSymbol srcSide = inverted ? targetType : sourceType;
                StructDeclarationSyntax? srcDecl = GetStructDeclaration(syntaxContext, srcSide.Name);

                string? goTypeTag = srcDecl?.GetGoTypeDefinitionText();

                if (goTypeTag == "num:uintptr")
                {
                    string lhName = inverted ? sourceTypeName : targetTypeName;
                    convExprOverride = $"new {lhName}(({valueType})(nuint)src.Value)";
                }
            }

            // A LOCAL numeric pair whose SOURCE underlying does not implicitly convert to the
            // CONSTRUCTED type's underlying — internal/trace's Inverted `timestamp`(uint64) →
            // `Time`(int64): the default `(TargetWrapper)src.Value` cast routes `ulong`→(the
            // wrapper's `long`)→wrapper, but `ulong`→`long` is not an implicit C# conversion, so it
            // is CS0030. Construct through the constructed type's underlying basic with an explicit
            // cast instead: `new ΔTime((long)src.Value)`. Each side's underlying comes from its
            // [GoType("num:X")] tag (the generated Value property is invisible to a single-pass
            // sibling generator, so the implicit-conversion test runs over the corresponding
            // SpecialTypes). Because the default cast compiles IFF that same source→constructed basic
            // conversion is implicit, this fires ONLY on cases that do not currently compile — every
            // already-compiling conversion stays byte-identical. (uintptr-backed pairs are handled by
            // the block above and keep their nuint hop; `int`/`uint` native-width wrappers are left
            // out where the classification is version-sensitive.)
            if (convExprOverride is null && !string.IsNullOrWhiteSpace(valueType))
            {
                ITypeSymbol constructedType = inverted ? sourceType : targetType;
                ITypeSymbol srcSideType = inverted ? targetType : sourceType;

                string? constructedBasic = GetNumBasic(syntaxContext, constructedType.Name);
                string? srcBasic = GetNumBasic(syntaxContext, srcSideType.Name);

                if (constructedBasic is not null && srcBasic is not null)
                {
                    string? constructedKeyword = NumBasicToKeyword(constructedBasic);

                    if (constructedKeyword is not null && NumBasicToKeyword(srcBasic) is not null &&
                        !IsImplicitNumericConversion(srcBasic, constructedBasic))
                    {
                        string lhName = inverted ? sourceTypeName : targetTypeName;
                        convExprOverride = $"new {lhName}(({constructedKeyword})src.Value)";
                    }
                }
            }

            // The emitted user-defined conversion operator's signature is (sourceTypeName,
            // targetTypeName, inverted) — `direct` vs `indirect` only changes the body. The same
            // pair can be recorded as BOTH a direct and an indirect conversion (e.g. `g` ↔ `ж<g>`),
            // which would emit two identical operators (CS0557). Skip an exact-signature duplicate.
            if (!emittedConversions.Add($"{sourceTypeName}->{targetTypeName}|{inverted}"))
                continue;

            string generatedSource = new ImplicitConvTemplate
            {
                PackageNamespace = packageNamespace,
                PackageName = packageName,
                SourceTypeName = sourceTypeName,
                TargetTypeName = targetTypeName,
                Inverted = inverted,
                Indirect = indirect,
                ValueType = valueType,
                StructMembers = structMembers,
                UsingStatements = usingStatements,
                HostTypeNameOverride = hostTypeNameOverride,
                LHTypeNameOverride = lhTypeNameOverride,
                RHTypeNameOverride = rhTypeNameOverride,
                ConvExprOverride = convExprOverride
            }
            .Generate();

            // Add the source code to the compilation
            context.AddSource(GetUniqueHintName(emittedHintNames, GetValidFileName($"{packageNamespace}.{packageClassName}.{sourceTypeName}-{targetTypeName}{(inverted ? "-inv" : "")}.g.cs")), generatedSource);
        }
    }

    // True when exactly one side of the pair is declared in ANOTHER assembly and that foreign side is
    // less accessible than the local side — the only side that could host the operator. The local
    // side goes through EffectiveScopeIsPublic: a modifier the converter WROTE wins, and the GO
    // export rule is the fallback for a bare [GoType] partial, whose modifier lives on the
    // TypeGenerator's own output and is invisible to a single-pass sibling generator (see the
    // relocation block above). The FOREIGN side is read from metadata, where it is already final.
    private static bool ForeignSideIsLessAccessible(ITypeSymbol sourceType, ITypeSymbol targetType, IAssemblySymbol thisAssembly)
    {
        bool sourceIsForeign = !SymbolEqualityComparer.Default.Equals(sourceType.ContainingAssembly, thisAssembly);
        bool targetIsForeign = !SymbolEqualityComparer.Default.Equals(targetType.ContainingAssembly, thisAssembly);

        if (sourceIsForeign == targetIsForeign)
            return false;

        (ITypeSymbol foreignType, ITypeSymbol localType) = sourceIsForeign ? (sourceType, targetType) : (targetType, sourceType);

        return foreignType.DeclaredAccessibility != Accessibility.Public &&
               EffectiveScopeIsPublic(localType, GetSimpleName(localType.Name, dropCollisionPrefix: true));
    }

    private static string? GetNamespace(FileScopedNamespaceDeclarationSyntax? namespaceSyntax)
    {
        return namespaceSyntax?.Name.ToString();
    }

    private static string? GetFirstClassName(CompilationUnitSyntax compilationUnit)
    {
        return compilationUnit.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault()?.Identifier.Text;
    }

    // A generated named-numeric struct exposes its underlying basic value through a public `Value`
    // property (see InheritedTypeTemplate). That basic is the through-underlying cast target needed to
    // construct a foreign named-numeric type cross-assembly. Returns null when no such property exists.
    private static string? GetUnderlyingBasicName(ITypeSymbol type)
    {
        IPropertySymbol? valProperty = type.GetMembers("Value").OfType<IPropertySymbol>().FirstOrDefault();
        return valProperty?.Type.ToDisplayString(s_qualifiedFormat);
    }

    // Reads a LOCAL named-numeric struct's [GoType("num:X")] underlying basic ("int64", "uint64", …)
    // from syntax — the generated Value property is invisible to a single-pass sibling generator, so
    // the tag is the only place the underlying is knowable here. Returns null when the type has no
    // local declaration or no numeric GoType tag.
    private static string? GetNumBasic(GeneratorSyntaxContext context, string typeName)
    {
        string? tag = GetStructDeclaration(context, typeName)?.GetGoTypeDefinitionText();

        return tag is not null && tag.StartsWith("num:") ? tag["num:".Length..] : null;
    }

    // The C# implicit numeric conversions among the fixed-width integer and float basics — the exact
    // set that decides whether the default `(Wrapper)src.Value` cast compiles. `int`/`uint`/`uintptr`
    // (native-width) are intentionally absent (NumBasicToKeyword returns null for them, so the caller
    // leaves them to the default / uintptr paths). Identity counts as convertible.
    private static bool IsImplicitNumericConversion(string src, string dst)
    {
        if (src == dst)
            return true;

        return src switch
        {
            "int8" => dst is "int16" or "int32" or "rune" or "int64" or "float32" or "float64",
            "int16" => dst is "int32" or "rune" or "int64" or "float32" or "float64",
            "int32" or "rune" => dst is "int64" or "float32" or "float64",
            "int64" => dst is "float32" or "float64",
            "uint8" or "byte" => dst is "int16" or "uint16" or "int32" or "rune" or "uint32" or "int64" or "uint64" or "float32" or "float64",
            "uint16" => dst is "int32" or "rune" or "uint32" or "int64" or "uint64" or "float32" or "float64",
            "uint32" => dst is "int64" or "uint64" or "float32" or "float64",
            "uint64" => dst is "float32" or "float64",
            "float32" => dst is "float64",
            _ => false
        };
    }

    // The C# keyword for a Go numeric basic, used as the explicit through-underlying cast target.
    private static string? NumBasicToKeyword(string basic) => basic switch
    {
        "int8" => "sbyte",
        "int16" => "short",
        "int32" or "rune" => "int",
        "int64" => "long",
        "uint8" or "byte" => "byte",
        "uint16" => "ushort",
        "uint32" => "uint",
        "uint64" => "ulong",
        "float32" => "float",
        "float64" => "double",
        _ => null
    };

    private static StructDeclarationSyntax? GetStructDeclaration(GeneratorSyntaxContext context, string structName)
    {
        if (PointerExpr.IsMatch(structName))
            structName = structName[(structName.IndexOf('<') + 1)..^1];

        // Match on ValueText (the identifier WITHOUT any `@` escape) against the `@`-stripped name, so a
        // keyword-named struct declared `partial struct @short` is found whether the caller passes the
        // symbol name `short` or the escaped `@short` — otherwise its [GoType("num:…")] tag is missed and
        // the numeric-conversion body falls back to a broken `(@short)src.Value` cast (CS0030).
        //
        // The DEFINING part wins over the condensed accessibility-pinning declaration package_info.cs's
        // TypeAccessibility section contributes for the same type — this lookup exists to read the
        // [GoType] tag and the members, neither of which that part carries (see IsGoTypeDefinition).
        StructDeclarationSyntax[] matches = context.SemanticModel.Compilation.SyntaxTrees
            .SelectMany(tree => tree.GetRoot().DescendantNodes().OfType<StructDeclarationSyntax>())
            .Where(structDeclaration => structDeclaration.Identifier.ValueText == structName.TrimStart('@'))
            .ToArray();

        return matches.FirstOrDefault(structDeclaration => structDeclaration.IsGoTypeDefinition()) ?? matches.FirstOrDefault();
    }
}
