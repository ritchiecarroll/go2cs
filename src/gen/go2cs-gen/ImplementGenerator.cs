// ImplementGenerator.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

//#define DEBUG_GENERATOR

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using go2cs.Templates.InterfaceImpl;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static go2cs.Common;
using static go2cs.Symbols;

#if DEBUG_GENERATOR
using System.Diagnostics;
#endif

namespace go2cs;

[Generator]
public class ImplementGenerator : ISourceGenerator
{
    private const string Namespace = "go";
    private const string AttributeName = "GoImplement";
    private const string FullAttributeName = $"{Namespace}.{AttributeName}Attribute<TStruct, TInterface>";

    // RS2008 asks for analyzer release-tracking files; go2cs-gen is a source generator that ships
    // with the converter and keeps none.
#pragma warning disable RS2008
    private static readonly DiagnosticDescriptor s_malformedRecord = new(
        id: "GO2CS0002",
        title: "Malformed GoImplement record",
        messageFormat: "Skipped {0}: {1}; no implementation is generated for it",
        category: "go2cs-gen",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);
#pragma warning restore RS2008

    // Renders a namespace for a `using` directive: no `global::`, keyword segments escaped
    // (`go.crypto.@internal`, not the invalid `go.crypto.internal`).
    private static readonly SymbolDisplayFormat s_namespaceUsingFormat = new(
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers);

    public void Initialize(GeneratorInitializationContext context)
    {
    #if DEBUG_GENERATOR
        if (!Debugger.IsAttached)
            Debugger.Launch();
    #endif

        // Register to find "GoImplementAttribute" on assembly attribute declarations
        context.RegisterForSyntaxNotifications(() => new AssemblyAttributeFinder(FullAttributeName));
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxContextReceiver is not AssemblyAttributeFinder { HasAttributes: true } attributeFinder)
            return;

        // Roslyn hintNames are case-insensitive; Go type names differing only by case are legal
        // and common, so disambiguate like the other generators (a collision throws and suppresses
        // ALL interface implementations for the package).
        HashSet<string> emittedHintNames = new(StringComparer.OrdinalIgnoreCase);

        // A (struct, interface) pair can carry Pointer and Promoted on SEPARATE attribute
        // instances (the converter records the ж-form and the embed-promotion independently);
        // the pointer-adapter emission needs to know the pair is promoted, so pre-index.
        HashSet<string> promotedPairs = new(StringComparer.Ordinal);

        // Emit ONE value-form implementation per (struct, interface) pair: the converter can
        // record the same pair both plain AND Promoted (archive/tar's lifted anon struct got
        // a promotion record from its interface embed and a plain record from the conversion
        // site) — the second partial re-emitted the comparison operators (CS0111 ×8). The
        // promotedPairs pre-index already folds the Promoted flag into the first emission.
        HashSet<string> emittedValuePairs = new(StringComparer.Ordinal);

        // Emit each explicit interface MEMBER once per struct across its partial-struct impls.
        // A type implementing both an interface AND a super-interface that EMBEDS it (File :
        // io.Closer, mime/multipart's sectionReadCloser) would emit the inherited member's
        // explicit impl (io_package.Closer.Close()) in BOTH the io.Closer partial and the File
        // partial → CS0111/CS8646. The earlier partial's explicit impl already satisfies the
        // member for the whole struct, so later partials skip it. Keyed by (struct, member) and
        // scoped to the partial-struct path — adapter classes (structNameжInterfaceName) are
        // distinct per interface and must keep their full method set, so they do not consult it.
        HashSet<string> emittedPartialMembers = new(StringComparer.Ordinal);

        // A GENERIC struct is recorded once per INSTANTIATION (nistCurve<ж<P224Point>>,
        // nistCurve<ж<P384Point>>, …) but generates ONE generic adapter over the struct's OPEN
        // form (nistCurveжCurve<Point>). All those closed records collapse to the same open
        // (struct, interface) pair here, so emit the class only for the first — a second would
        // redeclare it (CS0102). Keyed by the open definition + interface.
        HashSet<string> emittedGenericPointerAdapters = new(StringComparer.Ordinal);

        // The VALUE-form sibling of the set above, and it exists for a sharper reason than
        // tidiness: a partial declaration can only extend the OPEN generic definition. Emitting
        // the CONSTRUCTED form (`partial struct G<IntPtr>`, from Go's `var dummy I = G[int]{}`)
        // makes C# read `IntPtr` as a type-PARAMETER NAME, which disagrees with the converter's
        // `partial struct G<T>` (CS0264) and then spills every generated member — operators,
        // Equals, the explicit impl — into the containing STATIC package class
        // (CS0715/CS0708/CS0563). Keyed by the open definition + interface, so `G[int]` and
        // `G[G[int]]` collapse to the one partial that serves every instantiation — which is
        // also what Go means: a method on a generic type is declared for all of them.
        HashSet<string> emittedGenericValueImpls = new(StringComparer.Ordinal);

        HashSet<string> emittedInterfaceAdapters = new(StringComparer.Ordinal);

        foreach ((AttributeSyntax attributeSyntax, GeneratorSyntaxContext syntaxContext, _, _) in attributeFinder.TargetAttributes)
        {
            (string name, string value)[] arguments = attributeSyntax.GetArgumentValues();

            if (!bool.Parse(arguments.FirstOrDefault(arg => arg.name.Equals("Promoted")).value?.Trim() ?? "false"))
                continue;

            (ITypeSymbol? structType, ITypeSymbol? interfaceType) = attributeSyntax.Get2GenericTypeArguments(syntaxContext);

            if (structType is not null && interfaceType is not null)
                promotedPairs.Add($"{structType.ToDisplayString()}|{interfaceType.ToDisplayString()}");
        }

        // A pointer adapter is named "[<pkg>_]<structSimple>ж<ifaceSimple>". The STRUCT side is
        // package-qualified when foreign, but the INTERFACE side composes from its bare simple
        // name — ambiguous as soon as one struct adapts to two interfaces sharing a simple name.
        // compress/flate does: its own `Reader` and `io.Reader`, both reached from *bufio.Reader
        // and *bytes.Reader in its tests, composed one class TWICE (CS0102 + CS0111 + CS8646).
        // Collect the names more than one interface maps to, so the composition below can qualify
        // just those. COLLISION-CONDITIONAL by design: unconditional qualification would rename
        // 644 adapters across 3,688 construction sites, and the production corpus has no
        // collisions at all. Keep the rule in sync with the converter's adapterNameCollisions.go,
        // which resolves the matching cast-site references from these same records.
        Dictionary<string, HashSet<string>> adapterNameGroups = new(StringComparer.Ordinal);

        // The pointer pairs this compilation records, kept so the composition below can name an
        // adapter OTHER than the one the main loop is generating — see localPointerAdapterNames.
        List<(ITypeSymbol Struct, ITypeSymbol Interface, string PackageClass, bool Production)> pointerPairs = [];

        // THE PASS SEPARATION. How many members of each collision group carry the PRODUCTION facet
        // — the records the converter stamped as it seeded a recompile-model test assembly from the
        // production half's package_info.cs. Counted, not merely flagged, because the rule turns on
        // the count: see KeepsProductionAdapterName. Empty for every production compilation and both
        // reference test models, where no record carries the facet at all.
        Dictionary<string, int> productionFacetCounts = new(StringComparer.Ordinal);

        foreach ((AttributeSyntax attributeSyntax, GeneratorSyntaxContext syntaxContext, CompilationUnitSyntax compilationUnit, _) in attributeFinder.TargetAttributes)
        {
            (string name, string value)[] arguments = attributeSyntax.GetArgumentValues();

            if (!bool.Parse(arguments.FirstOrDefault(arg => arg.name.Equals("Pointer")).value?.Trim() ?? "false"))
                continue;

            (ITypeSymbol? structType, ITypeSymbol? interfaceType) = attributeSyntax.Get2GenericTypeArguments(syntaxContext);

            if (structType is null || interfaceType is null)
                continue;

            string packageClass = GetFirstClassName(compilationUnit) ?? string.Empty;
            string unqualified = $"{AdapterStructKey(structType, packageClass)}{PointerPrefix}{GetUnsanitizedIdentifier(GetSimpleName(interfaceType.ToDisplayString()))}";
            bool production = bool.Parse(arguments.FirstOrDefault(arg => arg.name.Equals("Production")).value?.Trim() ?? "false");

            if (!adapterNameGroups.TryGetValue(unqualified, out HashSet<string>? interfaces))
                adapterNameGroups[unqualified] = interfaces = new HashSet<string>(StringComparer.Ordinal);

            interfaces.Add(interfaceType.ToDisplayString());
            pointerPairs.Add((structType, interfaceType, packageClass, production));

            if (production)
                productionFacetCounts[unqualified] = productionFacetCounts.TryGetValue(unqualified, out int count) ? count + 1 : 1;
        }

        HashSet<string> collidingAdapterNames = new(adapterNameGroups.Where(entry => entry.Value.Count > 1).Select(entry => entry.Key), StringComparer.Ordinal);

        // ⚠ THE WRAP TARGETS. A pointer adapter's member is declared with the INTERFACE's own return
        // type and forwards the Go result raw, which is correct until the declared result is itself
        // an interface the Go method does not return — crypto/mlkem's projected
        // `decapsulationKey[encapsulationKey]`, whose `EncapsulationKey() E` binds E to the INTERFACE
        // where the concrete method returns `*EncapsulationKey768`. Go has no return covariance, so
        // the adapter is where the projection is made good and the forwarded result must be wrapped
        // in the RESULT interface's own adapter — which is a DIFFERENT adapter from the one being
        // generated, and therefore the one place this generator must NAME another adapter.
        //
        // ⚠⚠ THE MEMBERSHIP TEST IS "THIS COMPILATION RECORDS THE PAIR", NOT "THE STRUCT IS LOCAL",
        // and the difference is the whole of crypto/mlkem's row. It was the assembly test until
        // 2026-09-20, which is exactly backwards for the WHITE-BOX model that the `-tests` pipeline
        // generates: there the struct is ALWAYS in the PRODUCTION assembly and the interface ALWAYS
        // in the internal-test package, so the one arrangement the corpus needs was the one
        // arrangement excluded. The row read CS0266 ×2 at BUILD, one per key size, while the adapter
        // the wrap wanted to name — `mlkem_EncapsulationKey768жencapsulationKey` — was being minted
        // in that same compilation (mailbox d6d2970a2, ruled at 5347b4aae).
        //
        // `pointerPairs` is collected from THIS compilation's own attributes, so the ruled condition
        // needs no test of its own: membership in that list IS "the pair is recorded here", and the
        // adapter for it is therefore minted by the main loop below.
        //
        // ⚠ THE GENERIC BOUND STAYS. A generic target's adapter name trails its argument list
        // separately where GetSimpleName's `dropGeneric` default would fold it INTO the identifier;
        // that is a different defect with its own owner, and this consumer is not the place to fix it.
        //
        // ⚠⚠ AND LIFTING THE ASSEMBLY BOUND IS NOT A ONE-LINE DELETE, because that bound was what made
        // the NAME right BY CONSTRUCTION. While every pair was local, `GetSimpleName(GetFullTypeName())`
        // was a no-op that happened to equal the main loop's base name. A FOREIGN struct's adapter
        // carries ForeignPackagePrefix, so the value must now be composed through the SAME helper the
        // collision key one line above and the main loop's AdapterName both use — `AdapterStructKey`,
        // which is strip-then-last-segment PLUS the foreign prefix path. Two halves composing one name
        // from two spellings agree until they do not; this file's own collision-key finding (C1,
        // mailbox f89515008 §4) is that lesson, and sharing the helper is how the bound's guarantee
        // survives the bound.
        Dictionary<string, string> localPointerAdapterNames = new(StringComparer.Ordinal);

        foreach ((ITypeSymbol pairStruct, ITypeSymbol pairInterface, string pairPackageClass, bool pairProduction) in pointerPairs)
        {
            if (pairStruct is INamedTypeSymbol { IsGenericType: true })
                continue;

            string pairStructKey = AdapterStructKey(pairStruct, pairPackageClass);
            string pairUnqualified = $"{pairStructKey}{PointerPrefix}{GetUnsanitizedIdentifier(GetSimpleName(pairInterface.ToDisplayString()))}";
            string pairInterfaceName = GlobalQualify(pairInterface.GetFullTypeName(true));

            // The wrap target is named by the SAME rule as the class it names, pass separation
            // included — a production member's adapter is reached by the production name whether the
            // reference is a cast site or another adapter's result wrap.
            bool pairTakesPrefix = collidingAdapterNames.Contains(pairUnqualified) &&
                !KeepsProductionAdapterName(pairUnqualified, pairProduction, productionFacetCounts);

            localPointerAdapterNames[$"{GlobalQualify(pairStruct.ToDisplayString())}|{GlobalQualify(pairInterface.ToDisplayString())}"] =
                $"{pairStructKey}{PointerPrefix}{(pairTakesPrefix ? AdapterInterfacePrefix(pairInterface, pairPackageClass) : "")}{GetUnsanitizedIdentifier(GetSimpleName(pairInterfaceName))}";
        }

        foreach ((AttributeSyntax attributeSyntax, GeneratorSyntaxContext syntaxContext, CompilationUnitSyntax compilationUnit, FileScopedNamespaceDeclarationSyntax? namespaceSyntax) in attributeFinder.TargetAttributes)
        {
            SyntaxTree syntaxTree = attributeSyntax.SyntaxTree;
            SemanticModel semanticModel = context.Compilation.GetSemanticModel(syntaxTree);

            string packageNamespace = GetNamespace(namespaceSyntax) ?? Namespace;
            string packageClassName = GetFirstClassName(compilationUnit) ?? throw new MissingMemberException($"No package class found in same file as [assembly: {AttributeName}]");
            string packageName = packageClassName.EndsWith(PackageSuffix) ? packageClassName[..^PackageSuffix.Length] : packageClassName;
            
            string[] usingStatements = GetFullyQualifiedUsingStatements(syntaxTree, semanticModel);

            // Extract generic type arguments from "GoImplementAttribute"
            (ITypeSymbol? structType, ITypeSymbol? interfaceType) = attributeSyntax.Get2GenericTypeArguments(syntaxContext);
            
            // A malformed record costs exactly its own adapter. Thrown, it made the generator
            // contribute NOTHING, and every other adapter in the compilation went with it (go-cmp's
            // cmpopts: a record for a named empty interface, `GoImplement<S, object>`, CS8785, then
            // an unrelated CS0426 for an adapter never generated). The pre-passes above already
            // skip such a record.
            if (structType is null || interfaceType is null)
            {
                context.ReportDiagnostic(Diagnostic.Create(s_malformedRecord, attributeSyntax.GetLocation(), attributeSyntax.ToString(), "it must name two resolvable type arguments"));
                continue;
            }

            if (interfaceType.TypeKind != TypeKind.Interface)
            {
                context.ReportDiagnostic(Diagnostic.Create(s_malformedRecord, attributeSyntax.GetLocation(), attributeSyntax.ToString(), $"its second type argument, '{interfaceType.ToDisplayString()}', is not an interface"));
                continue;
            }

            string structName = structType.GetFullTypeName();
            string interfaceName = GlobalQualify(interfaceType.GetFullTypeName(true));

            // Get the attribute's Promoted / Pointer / ConstraintProxy argument values, if defined
            (string name, string value)[] arguments = attributeSyntax.GetArgumentValues();
            bool promoted = bool.Parse(arguments.FirstOrDefault(arg => arg.name.Equals("Promoted")).value?.Trim() ?? "false");
            bool pointer = bool.Parse(arguments.FirstOrDefault(arg => arg.name.Equals("Pointer")).value?.Trim() ?? "false");
            bool constraintProxy = bool.Parse(arguments.FirstOrDefault(arg => arg.name.Equals("ConstraintProxy")).value?.Trim() ?? "false");
            bool production = bool.Parse(arguments.FirstOrDefault(arg => arg.name.Equals("Production")).value?.Trim() ?? "false");

            if (structType.TypeKind == TypeKind.Interface)
            {
                List<MethodInfo> interfaceAdapterMethods = ((INamedTypeSymbol)interfaceType).GetAllBaseInterfaces(context.Compilation)
                    .Concat([(INamedTypeSymbol)interfaceType])
                    .SelectMany(iface => iface.GetMembers()
                        .OfType<IMethodSymbol>()
                        .Where(method => method.MethodKind == MethodKind.Ordinary)
                        .Where(method => !method.IsStatic)
                        .Select(method => (name: iface.ToDisplayString(), method)))
                    .Select(info => new MethodInfo
                    {
                        Name = $"{GlobalQualify(info.name)}.{EscapeCsKeyword(info.method.Name)}",
                        ReturnType = GlobalQualify(info.method.ReturnType.ToDisplayString()),
                        Parameters = info.method.Parameters.ToParameterInfos(withRefKind: true),
                        GenericTypes = string.Join(", ", info.method.TypeParameters.Select(type => type.ToDisplayString())),
                        TypeConstraints = info.method.TypeParameters.ToDictionary(type => type.Name, type => type.ConstraintTypes.Select(constraint => constraint.ToDisplayString()).ToArray()),
                        IsInaccessibleMarker = GetScope(info.method.Name) == "internal" &&
                            !SymbolEqualityComparer.Default.Equals(info.method.ContainingAssembly, context.Compilation.Assembly)
                    })
                    .Distinct()
                    .ToList();

                bool interfaceAdapterImplementsFormattable = interfaceAdapterMethods.RemoveAll(m => m.Name.StartsWith("System.IFormattable.")) > 0;
                bool foreignSourceInterface = !SymbolEqualityComparer.Default.Equals(structType.ContainingAssembly, syntaxContext.SemanticModel.Compilation.Assembly);
                string adapterScope = AdapterSidePublic(interfaceType, interfaceName) && AdapterSidePublic(structType, structName) ? "public" : "internal";

                // Compose the class name from UNESCAPED simple names: a keyword-named side arrives
                // "@"-escaped from its display string, and "@" is only legal at the START of an
                // identifier token — an interior marker (`lockᴠ@lock`) lexes as two tokens. The
                // composed name (always containing the infix) is never a keyword, so no marker is
                // needed. Keep in sync with the converter's valueAdapterTypeRef composition.
                string adapterName = $"{(foreignSourceInterface ? ForeignPackagePrefix(structType) : "")}{GetUnsanitizedIdentifier(GetSimpleName(structName))}{ValueAdapterInfix}{GetUnsanitizedIdentifier(GetSimpleName(interfaceName))}";

                if (!emittedInterfaceAdapters.Add($"{adapterName}|{interfaceName}"))
                    continue;

                string interfaceAdapterSource = new InterfaceAdapterImplTemplate
                {
                    PackageNamespace = packageNamespace,
                    PackageName = packageName,
                    SourceInterfaceName = GlobalQualify(structType.GetFullTypeName(true)),
                    InterfaceName = interfaceName,
                    AdapterName = adapterName,
                    AdapterScope = adapterScope,
                    ImplementsFormattable = interfaceAdapterImplementsFormattable,
                    Methods = interfaceAdapterMethods,
                    UsingStatements = usingStatements
                }
                .Generate();

                context.AddSource(GetUniqueHintName(emittedHintNames, GetValidFileName($"{packageNamespace}.{packageClassName}.{structName}-{interfaceName}-iface.g.cs")), interfaceAdapterSource);
                continue;
            }

            // A NAMED FUNC type with methods (flag's `type funcValue func(string) error`
            // implementing Value) arrives as a DELEGATE — it cannot be a partial struct, so
            // it routes to the VALUE adapter below. Anything else non-struct is a converter
            // bug worth failing loudly on — but NOT by throwing, which kills the entire
            // generator run for the package (flag lost all 11 of its adapters, CS0246 ×19).
            if (structType.TypeKind != TypeKind.Struct && structType.TypeKind != TypeKind.Delegate)
                continue;
                
            // A SELF-REFERENTIAL constraint proxy (crypto/elliptic's *P224Point satisfying
            // nistPoint[Point] structurally so nistCurve[*P224Point] can instantiate): emit the
            // proxy class that wraps ж<element> and implements the interface over ITSELF, then move
            // on — none of the normal adapter method-binding below applies.
            if (constraintProxy)
            {
                EmitConstraintProxy(context, structType, (INamedTypeSymbol)interfaceType, packageNamespace, packageName, packageClassName, usingStatements, emittedHintNames, emittedGenericPointerAdapters);
                continue;
            }

            // Get all extension methods for the struct, any directly defined receivers
            // take precedence over promoted interface methods that have the same name
            (StructDeclarationSyntax? structDecl, Compilation? compilation) = context.GetStructDeclaration(structType.ToDisplayString());
            IEnumerable<MethodInfo>? structMethods = structDecl is null ? [] : structDecl.GetExtensionMethods(compilation!);
            HashSet<string> overrides = new(structMethods?.Select(method => method.Name) ?? [], StringComparer.Ordinal);

            // Every method this struct DECLARES in the current compilation, in either receiver form —
            // the value/ref extensions above plus the direct-ж primaries. This is the evidence that the
            // adapter has something real to forward to, and it is what keeps the package-sealing stub
            // below from swallowing a genuine implementation (see IsInaccessibleMarker).
            HashSet<string> localImplNames = new(overrides, StringComparer.Ordinal);

            if (structDecl is not null)
                localImplNames.UnionWith(structDecl.GetBoxReceiverMethodNames(compilation!));

            // A referenced PRODUCTION struct can gain methods from the current compilation's friend
            // bridge — an internal white-box test package declaring `marshal(this ж<T>)` or
            // `[GoRecv] unmarshal(this ref T, …)` for a production T reachable through the test
            // model's InternalsVisibleTo grant. The declaration discovery above hands back
            // (null, null) for exactly that shape, so the local-implementation EVIDENCE must come
            // from a compilation-wide scan, in EVERY receiver form the local path counts (the ref
            // form forwards through its RecvGenerator ж-twin, exactly as IsRefRecv does below).
            // Without this the package-sealing stub reads a bridge-implemented member as an
            // inaccessible marker, and the adapter COMPILES while silently answering defaults —
            // crypto/tls's *SessionState is the measured consumer: its test-declared
            // marshal/unmarshal stubbed to `default!`, so TestMarshalUnmarshal saw an empty marshal
            // and reported "failed to unmarshal" with no diagnostic anywhere.
            HashSet<string> bridgeBoxMethods = [];
            HashSet<string> bridgeRefMethods = [];

            if (structDecl is null)
            {
                bridgeBoxMethods = StructDeclarationSyntaxExtensions.GetBoxReceiverMethodNamesBySimpleName(structType.Name, syntaxContext.SemanticModel.Compilation);
                bridgeRefMethods = StructDeclarationSyntaxExtensions.GetRefReceiverMethodNamesBySimpleName(structType.Name, syntaxContext.SemanticModel.Compilation);
                localImplNames.UnionWith(bridgeBoxMethods);
                localImplNames.UnionWith(bridgeRefMethods);
            }

            // The adapter had no notion that a member's IMPLEMENTED name and its FORWARDED name can
            // differ, and spelled the interface's name at both positions — so once the collision pass
            // Δ-renamed a test-file declarator the forward bound nothing on the box (CS1929 ×10 in
            // `flag`, the compiler naming whatever unrelated extension it could see,
            // `bytes_package.String`, as its nearest candidate). `localImplNames` is already the set
            // of methods this struct DECLARES in either receiver form, so the emitted name is a fact
            // in hand rather than a rule to re-derive — see Common.ResolveForwardMemberName.

            // GetAllBaseInterfaces (not AllInterfaces) recovers a base declared in ANOTHER package
            // class, which is still PRIVATE until this generator emits its access modifier and would
            // otherwise bind to an empty error symbol — see Common.GetAllBaseInterfaces.
            List<MethodInfo> methods = ((INamedTypeSymbol)interfaceType).GetAllBaseInterfaces(context.Compilation)
                .Concat([(INamedTypeSymbol)interfaceType]) // Include the original interface
                .SelectMany(iface => iface.GetMembers()
                    .OfType<IMethodSymbol>()
                    .Where(method => method.MethodKind == MethodKind.Ordinary)
                    .Where(method => !method.IsStatic)
                    .Select(method => (name: iface.ToDisplayString(), method)))
                .Select(info => new MethodInfo
                {
                    // A keyword method name (gob's `string()`) read from the interface symbol is
                    // UNescaped; escape it so both the explicit-interface signature and the
                    // forwarding call emit `@string` (bare `string` is a parse error, CS1525/0539).
                    Name = promoted && !pointer && !overrides.Contains(GetSimpleName(EscapeCsKeyword(info.method.Name))) ? EscapeCsKeyword(info.method.Name) : $"{GlobalQualify(info.name)}.{EscapeCsKeyword(info.method.Name)}",
                    // The DECLARED name to forward to, when the collision pass renamed it away from
                    // the interface member's own name — see the note above.
                    ForwardName = ResolveForwardMemberName(GetSimpleName(EscapeCsKeyword(info.method.Name)), localImplNames),
                    ReturnType = GlobalQualify(info.method.ReturnType.ToDisplayString()),
                    // Carry the parameter REF KIND: an interface member declared with an `in`
                    // param (the hand-finished io stub's Reader.Read(in slice<byte>)) is a
                    // distinct signature - an explicit impl without it is CS0539. Parameter NAMES
                    // are `@`-escaped for the same reason the method name above is — see
                    // ToParameterInfos (sync.Map's `CompareAndSwap(key, old, new any)`).
                    Parameters = info.method.Parameters.ToParameterInfos(withRefKind: true),
                    GenericTypes = string.Join(", ", info.method.TypeParameters.Select(type => type.ToDisplayString())),
                    TypeConstraints = info.method.TypeParameters.ToDictionary(type => type.Name, type => type.ConstraintTypes.Select(constraint => constraint.ToDisplayString()).ToArray()),
                    // A Go-UNEXPORTED interface method (lowercase name → its C# extension impl is
                    // INTERNAL) declared in a DIFFERENT assembly than the one this adapter is generated
                    // into is a package-sealing MARKER (ast.Expr's exprNode(), parse.Node's tree()/writeTo()):
                    // its extension is invisible here, so forwarding is CS1061. Go bars calling it from
                    // outside its package, so the adapter stubs the (still-required, public) member.
                    //
                    // The assembly comparison alone is a PROXY for "there is nothing to forward to", and
                    // it answers wrongly for the one shape where a Go package spans two assemblies: an
                    // INTERNAL (white-box) test package. `package profile`'s proto_test.go declares
                    // packedInts and its `encode`/`decoder` methods for profile's own unexported
                    // `message` interface — same Go package, different C# assembly, and reachable
                    // because the test model mints an InternalsVisibleTo grant. Stubbing there is worse
                    // than a compile error: the adapter COMPILES and silently does nothing, so
                    // marshal(source) returned an empty buffer with no diagnostic anywhere. Require the
                    // absence of a local implementation as well, which leaves the genuine markers (a
                    // FOREIGN struct never declares the sealing method) stubbed exactly as before.
                    IsInaccessibleMarker = GetScope(info.method.Name) == "internal" &&
                        !SymbolEqualityComparer.Default.Equals(info.method.ContainingAssembly, context.Compilation.Assembly) &&
                        !localImplNames.Contains(GetSimpleName(EscapeCsKeyword(info.method.Name)))
                })
                .Distinct()
                .ToList();

            // The interface may inherit System.IFormattable (the hand-finished io stub's
            // Reader does, for the dyn machinery): its ToString(format, provider) cannot
            // forward through the box or an uncast struct (CS1501/CS0030) — the templates
            // emit a canned explicit impl instead when flagged.
            bool implementsFormattable = methods.RemoveAll(m => m.Name.StartsWith("System.IFormattable.")) > 0;

            // An interface member with NO direct struct method may be satisfied by Go method
            // promotion through an embedded POINTER field (`type rtype struct { *abi.Type }`).
            // That promotion is syntax-resolved at Go call sites (the converter emits the hop
            // `t.Type.Value.M()`), so the explicit interface implementation must forward through
            // the same hop — `this.M()` has nothing to bind (CS1929). A SINGLE hop takes every
            // unbound member unconditionally: that member's promotion is what type-checked the
            // cast, so there is nothing to decide. SEVERAL embeds is the case that must be
            // decided per member — see multiEmbedHopPaths below.
            List<(string Name, string TypeName)> embedHops = structDecl?.GetEmbeddedPointerHopNames() ?? [];
            string? embedHop = embedHops.Count == 1 ? embedHops[0].Name : null;

            // With SEVERAL embedded pointers no single hop can take every member, so both forms of the
            // single-hop forwarding below stayed silent and every promoted member fell back to the bare
            // `m_box.M(…)` / `this.M(…)` receiver — which binds nothing on the struct and lets overload
            // resolution reach an unrelated same-named extension elsewhere in scope. net/rpc/jsonrpc's `type pipe
            // struct { *io.PipeReader; *io.PipeWriter }` is the shape: Read and Write come only by
            // promotion, and the adapter's `m_box.Read(p)` bound `io_package.Read(ref LimitedReader, …)`
            // (CS1929 — an error naming LimitedReader from a jsonrpc test is the signature). Index the
            // hop path PER MEMBER instead, routing each to the UNIQUE embed declaring it exactly as Go's
            // depth-1 promotion does; a name two embeds declare is promoted from neither (Go rejects the
            // tie), and the struct's own method, already bound above, wins over both.
            Dictionary<string, string> multiEmbedHopPaths = embedHops.Count > 1
                ? GetMultiEmbedHopPaths(context, syntaxContext.SemanticModel.Compilation, structType, embedHops)
                : [];

            // Hop-target methods that are direct-ж primaries bind on the box FIELD itself
            // (`this.File.Read(p)`) — deref'ing first strands the extension receiver (CS1929).
            HashSet<string> embedHopBoxMethods = embedHops.Count == 1
                ? StructDeclarationSyntaxExtensions.GetBoxReceiverMethodNames(embedHops[0].TypeName, syntaxContext.SemanticModel.Compilation)
                : [];

            // A FOREIGN hop type (net/http's http2timeTimer embeds *time.Timer) declares its
            // ptr-receiver methods as ж-extensions visible only in METADATA — direct-ж primaries
            // and the public RecvGenerator twins — so the syntax scan above finds none and every
            // promoted forwarder deref'd to the value (`this.Timer.Value.Reset(d)`, CS1929).
            // Resolve the hop element's SYMBOL from the struct and union its metadata box methods:
            // those bind on the embedded box field exactly as a local direct-ж primary does.
            if (embedHops.Count == 1)
            {
                INamedTypeSymbol? hopElement = StructDeclarationSyntaxExtensions.GetPointerEmbeds(structType)
                    .FirstOrDefault(embed => embed.Name == embedHops[0].Name).Type;

                if (hopElement is not null &&
                    !SymbolEqualityComparer.Default.Equals(hopElement.ContainingAssembly, syntaxContext.SemanticModel.Compilation.Assembly))
                {
                    embedHopBoxMethods.UnionWith(StructDeclarationSyntaxExtensions.GetForeignBoxReceiverMethodNames(hopElement));
                }
            }

            // A hop-type method may be declared DEEPER - on a VALUE-embedded field of the hop
            // type with a POINTER receiver (net's tcpConnWithoutWriteTo embeds *TCPConn; TCPConn
            // embeds conn by VALUE; Read/Write live on zh<conn>). `this.TCPConn.Value.Read(p)`
            // strands the extension receiver (CS1929 x2); project the field's box through the
            // generated ref accessor instead: `this.TCPConn.of(TCPConn.Rconn).Read(p)`.
            Dictionary<string, string> embedHopDeepPaths = new(StringComparer.Ordinal);
            HashSet<string> embedHopValueMethods = new(StringComparer.Ordinal);

            if (embedHops.Count == 1)
            {
                (StructDeclarationSyntax? hopDecl, Compilation? hopCompilation) = context.GetStructDeclaration(embedHops[0].TypeName);

                if (hopDecl is not null && hopCompilation is not null)
                {
                    string hopSimpleName = GetSimpleName(embedHops[0].TypeName);

                    embedHopValueMethods.UnionWith(hopDecl.GetExtensionMethods(hopCompilation).Select(method => GetSimpleName(method.Name)));

                    foreach ((string fieldName, string fieldTypeName) in hopDecl.GetEmbeddedValueHopNames())
                    {
                        foreach (string deepMethod in StructDeclarationSyntaxExtensions.GetBoxReceiverMethodNames(fieldTypeName, hopCompilation))
                        {
                            if (!embedHopBoxMethods.Contains(deepMethod) && !embedHopDeepPaths.ContainsKey(deepMethod))
                                embedHopDeepPaths[deepMethod] = $".of({packageClassName}.{hopSimpleName}.Ꮡ{fieldName})";
                        }
                    }
                }
            }

            // A pointer embed is not a struct's ONLY promotion source, so "one hop takes every
            // unbound member" holds only while it is the ONLY depth-1 embed. net/http's
            // `type breakableConn struct { net.Conn; *brokenState }` is the counter-example: Read,
            // Close, LocalAddr, RemoteAddr and the three deadline setters promote from the
            // INTERFACE embed, yet the single-hop arm claimed them all and emitted
            // `m_box.Value.brokenState.Value.Read(p)` — CS1929 ×7 naming brokenState from a
            // transport test, which is the several-pointer-embeds signature all over again, one
            // embed KIND further out.
            //
            // Go decides depth-1 promotion PER MEMBER, routing each name to the embed whose method
            // set declares it. So with MIXED embed kinds present, gate the hop on the members it
            // actually provides — box-receiver methods, value-receiver methods, and the deep paths
            // resolved above, plus a FOREIGN hop's metadata forms — and let everything else fall
            // through to the interface-field and value-embed arms below, exactly as it would if
            // there were no pointer embed at all. A member NEITHER arm places still lands on the
            // template's bare `m_box` default: a loud CS1929 naming it, never a silent wrong
            // receiver.
            //
            // With the pointer embed as the SOLE depth-1 embed there is nothing to decide — that
            // member's promotion is what type-checked the cast — so the gate stays null and the
            // unconditional path is byte-identical to what it always emitted.
            HashSet<string>? embedHopMemberNames = null;

            if (embedHops.Count == 1 &&
                ((structDecl?.GetEmbeddedValueHopNames().Count ?? 0) > 0 || GetEmbeddedInterfaceFieldMembers(structType).Count > 0))
            {
                embedHopMemberNames = new HashSet<string>(embedHopBoxMethods, StringComparer.Ordinal);

                embedHopMemberNames.UnionWith(embedHopValueMethods);
                embedHopMemberNames.UnionWith(embedHopDeepPaths.Keys);

                INamedTypeSymbol? gateHopElement = StructDeclarationSyntaxExtensions.GetPointerEmbeds(structType)
                    .FirstOrDefault(embed => embed.Name == embedHops[0].Name).Type;

                if (gateHopElement is not null &&
                    !SymbolEqualityComparer.Default.Equals(gateHopElement.ContainingAssembly, syntaxContext.SemanticModel.Compilation.Assembly))
                {
                    embedHopMemberNames.UnionWith(StructDeclarationSyntaxExtensions.GetForeignValueReceiverMethods(gateHopElement).Keys);
                }
            }

            if (pointer)
            {
                // A POINTER-sourced interface cast (`var s Iface = &t`): the interface value must
                // alias the receiver box (Go's interface holds the *T), so emit the IжAdapter
                // wrapper instead of the value-boxing partial struct — which copies, and cannot
                // bind direct-ж receiver methods from a struct's `this` at all (CS1929).
                Dictionary<string, string> forwardReceivers = new(StringComparer.Ordinal);
                Dictionary<string, string> forwardStaticCalls = new(StringComparer.Ordinal);

                foreach (MethodInfo structMethod in structMethods ?? [])
                {
                    // A [GoRecv] ref extension has a RecvGenerator ж-twin that binds on the box;
                    // a plain value-receiver method needs the deref'd value (Go copies at the call).
                    forwardReceivers[structMethod.Name] = structMethod.IsRefRecv ? "m_box" : "m_box.Value";
                }

                foreach (string boxReceiverName in structDecl?.GetBoxReceiverMethodNames(compilation!) ?? [])
                    forwardReceivers[boxReceiverName] = "m_box"; // direct-ж primary form binds the box itself

                // A referenced production struct can gain pointer-receiver methods from the current
                // test compilation's friend bridge. Those extensions are syntax-local even though the
                // struct declaration is metadata-only — which is exactly when the discovery above
                // hands back (null, null), so the scan (hoisted beside localImplNames, which needs
                // the same evidence for the sealing-marker classification) uses the CURRENT
                // compilation, keyed on the SIMPLE name: the bridge spells its box parameter through
                // whatever qualification its own file needs (`this ж<Replacer>` via an imported
                // alias, but `this ж<global::go.sync_package.poolChain>` once a `go/*` package in
                // the closure shadows the root namespace), never one fixed display form. A bridge
                // [GoRecv] ref extension binds the box through its RecvGenerator ж-twin, exactly as
                // IsRefRecv routes the local form above.
                if (structDecl is null)
                {
                    foreach (string boxReceiverName in bridgeBoxMethods)
                        forwardReceivers[boxReceiverName] = "m_box";

                    foreach (string refReceiverName in bridgeRefMethods)
                        forwardReceivers[refReceiverName] = "m_box";
                }

                // A FOREIGN struct (no local declaration — the pair is recorded locally because
                // the defining package never converts it: os never casts *File to io.Reader, so
                // fmt's Fscan(os.Stdin, …) has no exported adapter to reference — CS1503). Bind
                // forwarding from METADATA: the compiled assembly exposes every converter and
                // sibling-generator form as real symbols, so an extension on ж<T> binds the box
                // and everything else binds the deref'd value (ref extensions bind through the
                // ref-returning .Value).
                bool foreignStruct = structDecl is null && !SymbolEqualityComparer.Default.Equals(structType.ContainingAssembly, syntaxContext.SemanticModel.Compilation.Assembly);

                if (foreignStruct && structType.ContainingType is INamedTypeSymbol packageClass)
                {
                    HashSet<string> boxBound = new(StringComparer.Ordinal);
                    HashSet<string> refBound = new(StringComparer.Ordinal);

                    foreach (IMethodSymbol method in packageClass.GetMembers().OfType<IMethodSymbol>())
                    {
                        if (!method.IsStatic || method.Parameters.Length == 0)
                            continue;

                        // Only a PUBLIC zh-extension binds cross-assembly — the RecvGenerator
                        // twins of unexported methods are internal, visible to this METADATA
                        // scan but not to the consuming compilation (elf's zstd_ReaderzhReader
                        // forwarded m_box.Read to zstd's internal twin, CS1929).
                        if (method.DeclaredAccessibility == Accessibility.Public &&
                            method.Parameters[0].Type is INamedTypeSymbol recvType &&
                            recvType.Name == "ж" &&
                            recvType.TypeArguments.Length == 1 &&
                            IsForeignReceiverOf(recvType.TypeArguments[0], structType))
                        {
                            boxBound.Add(method.Name);
                        }

                        // A [GoRecv] ref extension called STATICALLY needs the ref keyword.
                        if (method.DeclaredAccessibility == Accessibility.Public &&
                            method.Parameters[0].RefKind == RefKind.Ref &&
                            IsForeignReceiverOf(method.Parameters[0].Type, structType))
                        {
                            refBound.Add(method.Name);
                        }
                    }

                    // A foreign package class in ANOTHER namespace segment (zstd_package lives
                    // in go.@internal) is not imported by the generated file, so its extensions
                    // are invisible to extension-method lookup (CS1929, elf's zstd_ReaderzhReader)
                    // — forward via the package-class STATIC call instead.
                    string emittingNamespace = packageNamespace;
                    string foreignNamespace = packageClass.ContainingNamespace?.ToDisplayString() ?? "";
                    string? staticClass = null;

                    if (!string.Equals(foreignNamespace, emittingNamespace, StringComparison.Ordinal))
                        staticClass = packageClass.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

                    foreach (MethodInfo method in methods)
                    {
                        string simpleName = GetSimpleName(method.Name);

                        // A method the FRIEND BRIDGE contributes is not in the referenced assembly's
                        // metadata at all, so the METADATA scan above cannot see it and would fall
                        // this member back to `m_box.Value` — stranding the direct-ж extension
                        // receiver (CS1929). The bridge scan already bound it to the box; keep that.
                        // sync's export_test.go declares PushHead/PopTail on the production
                        // `*poolDequeue`/`*poolChain`, which is exactly this shape — and the ref
                        // form (crypto/tls's `[GoRecv] unmarshal(this ref SessionState, …)`) binds
                        // the box through its RecvGenerator ж-twin the same way.
                        if (bridgeBoxMethods.Contains(simpleName) || bridgeRefMethods.Contains(simpleName))
                            continue;

                        bool viaBox = boxBound.Contains(simpleName);

                        // Only forward through a package-class STATIC when one actually binds this
                        // struct's box/ref. A PROMOTED interface method — debug/buildinfo's *xcoff.Section
                        // → io.ReaderAt, where Section EMBEDS the io.ReaderAt interface so ReadAt is
                        // promoted, not declared — has no such static, so `xcoff_package.ReadAt(m_box,…)`
                        // targets a nonexistent overload (CS1501). Fall to the box VALUE, invoking the
                        // struct's own PUBLIC promoted method (`m_box.Value.ReadAt(…)`).
                        if (staticClass is not null && (viaBox || refBound.Contains(simpleName)))
                        {
                            string recvArg = viaBox ? "m_box" : "ref m_box.Value";
                            forwardStaticCalls[simpleName] = $"{staticClass}.{simpleName}";
                            forwardReceivers[simpleName] = recvArg;
                        }
                        else
                        {
                            forwardReceivers[simpleName] = viaBox ? "m_box" : "m_box.Value";
                        }
                    }

                    // An interface member the foreign struct PROMOTES through a VALUE-embedded field
                    // (parse's `RangeNode` embeds `BranchNode`; the exported `String` lives on BranchNode,
                    // not RangeNode) has no extension on the struct's OWN package class, so the fallback
                    // `m_box.Value.String()` is CS1929. Forward through the embed's package-class STATIC
                    // (`parse_package.String(ref m_box.Value.BranchNode)`) — the embed's namespace is not
                    // imported here (only `using go;`), so the instance form cannot resolve the extension,
                    // exactly as the staticClass arm above handles the struct's own foreign extensions.
                    Dictionary<string, RefKind> structOwnMethods = StructDeclarationSyntaxExtensions.GetForeignValueReceiverMethods((INamedTypeSymbol)structType);

                    foreach ((string embedName, INamedTypeSymbol embedType) in StructDeclarationSyntaxExtensions.GetForeignValueEmbeds((INamedTypeSymbol)structType))
                    {
                        string embedStaticClass = embedType.ContainingType?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ?? "";

                        if (embedStaticClass.Length == 0)
                            continue;

                        Dictionary<string, RefKind> embedMethods = StructDeclarationSyntaxExtensions.GetForeignValueReceiverMethods(embedType);

                        foreach (MethodInfo method in methods)
                        {
                            string simpleName = GetSimpleName(method.Name);

                            // Only reroute a genuinely PROMOTED member still on the plain m_box.Value
                            // fallback: the struct binds it neither directly (box/ref/value) nor via an
                            // already-resolved hop, and the embed publicly declares it.
                            if (!embedMethods.TryGetValue(simpleName, out RefKind embedRefKind) ||
                                boxBound.Contains(simpleName) ||
                                structOwnMethods.ContainsKey(simpleName) ||
                                !forwardReceivers.TryGetValue(simpleName, out string? currentReceiver) ||
                                currentReceiver != "m_box.Value")
                            {
                                continue;
                            }

                            string receiverPrefix = embedRefKind switch
                            {
                                RefKind.Ref => "ref ",
                                RefKind.In => "in ",
                                _ => ""
                            };

                            forwardStaticCalls[simpleName] = $"{embedStaticClass}.{simpleName}";
                            forwardReceivers[simpleName] = $"{receiverPrefix}m_box.Value.{embedName}";
                        }
                    }

                    // An interface member the foreign struct PROMOTES through a POINTER embed —
                    // net/http/internal's FlushAfterChunkWriter embeds *bufio.Writer; bufio.ReadWriter
                    // embeds BOTH *Reader and *Writer (recorded downstream by net/http and httputil):
                    // the member lives on the EMBED's ж-extensions (direct-ж primaries or public
                    // RecvGenerator twins), so the plain m_box.Value fallback binds nothing (CS1061).
                    // Forward through the embedded box FIELD (`m_box.Value.Writer.Write(p)`) — Go
                    // promotes the embed's full pointer method set into *T's. With SEVERAL pointer
                    // embeds each member routes to the UNIQUE embed declaring it (Go's promotion
                    // ambiguity rules reject the rest at depth one). An embed package class outside
                    // this file's extension-lookup reach (not the emitting namespace, the shared root
                    // namespace, or an enclosing segment) forwards via its package-class STATIC with
                    // the box as the receiver argument, mirroring the struct's own foreign arm above.
                    List<(string Name, INamedTypeSymbol Type)> foreignPointerEmbeds = StructDeclarationSyntaxExtensions.GetPointerEmbeds(structType);

                    if (foreignPointerEmbeds.Count > 0)
                    {
                        List<(string Name, INamedTypeSymbol Type, HashSet<string> BoxMethods)> embedBoxMethods = foreignPointerEmbeds
                            .Select(embed => (embed.Name, embed.Type, StructDeclarationSyntaxExtensions.GetForeignBoxReceiverMethodNames(embed.Type)))
                            .ToList();

                        foreach (MethodInfo method in methods)
                        {
                            string simpleName = GetSimpleName(method.Name);

                            // Only reroute a member still on the plain m_box.Value fallback
                            // (mirrors the value-embed arm's gating above).
                            if (boxBound.Contains(simpleName) ||
                                structOwnMethods.ContainsKey(simpleName) ||
                                !forwardReceivers.TryGetValue(simpleName, out string? pointerEmbedReceiver) ||
                                pointerEmbedReceiver != "m_box.Value")
                            {
                                continue;
                            }

                            List<(string Name, INamedTypeSymbol Type, HashSet<string> BoxMethods)> declaringEmbeds = embedBoxMethods
                                .Where(embed => embed.BoxMethods.Contains(simpleName))
                                .ToList();

                            if (declaringEmbeds.Count != 1)
                                continue;

                            (string pointerEmbedName, INamedTypeSymbol pointerEmbedType, _) = declaringEmbeds[0];
                            string pointerEmbedNamespace = pointerEmbedType.ContainingType?.ContainingNamespace?.ToDisplayString() ?? "";

                            if (pointerEmbedNamespace != packageNamespace && pointerEmbedNamespace != Namespace &&
                                !packageNamespace.StartsWith($"{pointerEmbedNamespace}.", StringComparison.Ordinal))
                            {
                                forwardStaticCalls[simpleName] = $"{pointerEmbedType.ContainingType!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}.{simpleName}";
                            }

                            forwardReceivers[simpleName] = $"m_box.Value.{pointerEmbedName}";
                        }
                    }
                }

                // Interface members with NO struct method forward through a single embedded-pointer
                // hop, mirroring the value-form template (Go method promotion through `*abi.Type`).
                if (embedHops.Count == 1)
                {
                    foreach (MethodInfo method in methods)
                    {
                        string simpleName = GetSimpleName(method.Name);

                        // MIXED embed kinds: the hop takes only what it provides (see
                        // embedHopMemberNames); the rest falls to the arms below.
                        if (embedHopMemberNames is not null && !embedHopMemberNames.Contains(simpleName))
                            continue;

                        if (!forwardReceivers.ContainsKey(simpleName))
                        {
                            if (embedHopDeepPaths.TryGetValue(simpleName, out string? deepPath))
                                forwardReceivers[simpleName] = $"m_box.Value.{embedHop}{deepPath}";
                            else
                                forwardReceivers[simpleName] = embedHopBoxMethods.Contains(simpleName) ? $"m_box.Value.{embedHop}" : $"m_box.Value.{embedHop}.Value";
                        }
                    }
                }
                else if (multiEmbedHopPaths.Count > 0)
                {
                    // SEVERAL embedded pointers: only a member the per-embed index resolved forwards
                    // (see GetMultiEmbedHopPaths). One it cannot place stays unbound and falls to the
                    // template's `m_box` default exactly as before — a loud CS1929 naming the member,
                    // never a silent wrong receiver.
                    foreach (MethodInfo method in methods)
                    {
                        string simpleName = GetSimpleName(method.Name);

                        if (!forwardReceivers.ContainsKey(simpleName) && multiEmbedHopPaths.TryGetValue(simpleName, out string? hopPath))
                            forwardReceivers[simpleName] = $"m_box.Value.{hopPath}";
                    }
                }

                // DEPTH-1 promotion through a marker-backed VALUE embed is resolved FIRST, ahead of
                // the embedded-INTERFACE-field arm below. That arm's field test is a NAME heuristic
                // and it cannot, on its own, tell a Go embedded interface from an ordinary named
                // field whose name happens to equal its type's simple name: `type PtrType struct {
                // CommonType; Type Type }` and slogtest's genuine `wrapper` embed of `slog.Handler`
                // BOTH emit a plain `public ΔType Type;` / `public ΔHandler Handler;` field.
                //
                // Where a depth-1 value embed the converter MARKED (`public partial ref CommonType
                // CommonType { get; }`) provides the member, the heuristic's answer is the wrong one
                // by construction: legal Go cannot promote one member from two depth-1 embeds — that
                // is an ambiguity the compiler rejects — so seeing both means the interface "embed"
                // is really a plain field. Yield to the hard marker. Deeper embed levels stay BELOW
                // the interface arm, matching Go's shallower-embed-wins rule.
                //
                // What this cost while the order was the other way round: `dwarf.PtrType.Common()`
                // and `TypedefType.Common()` forwarded through the `Type` FIELD, so they returned
                // the REFERENCED type's CommonType instead of the receiver's own — a silent wrong
                // answer whenever `Type` was non-nil, and a null dereference when it was not.
                bindThroughValueEmbeds(maxDepth: 1);

                // Interface members may also promote through embedded INTERFACE field(s) —
                // zip's `type nopCloser struct { io.Writer }`: Write lives on the FIELD's
                // interface value (Go promotes its method set), Close on the struct. Forward
                // still-unbound members through the field whose interface declares them:
                // `m_box.Value.Writer.Write(…)`. Semantic detection (field name equals its
                // interface type's simple name — the converter names the field after the Go
                // embed, so a Δ-renamed interface TYPE keeps a markerless FIELD: slogtest's
                // `wrapper` embeds slog.ΔHandler as `Handler`) — the converter emits embeds
                // as real fields, so the symbol sees them. With SEVERAL embedded interface
                // fields (httputil's `dumpConn` embeds io.Writer AND io.Reader, adapted to
                // net.Conn) each member routes to the UNIQUE field declaring it — Go's
                // promotion ambiguity rules reject a member two fields declare unless the
                // struct overrides it, which forwardReceivers already resolved above.
                List<(string FieldName, HashSet<string> Members)> embeddedIfaceFieldMembers = GetEmbeddedInterfaceFieldMembers(structType);

                if (embeddedIfaceFieldMembers.Count > 0)
                {
                    foreach (MethodInfo method in methods)
                    {
                        string simpleName = GetSimpleName(method.Name);

                        if (forwardReceivers.ContainsKey(simpleName))
                            continue;

                        List<(string FieldName, HashSet<string> Members)> declaringFields = embeddedIfaceFieldMembers
                            .Where(field => field.Members.Contains(simpleName))
                            .ToList();

                        if (declaringFields.Count == 1)
                            forwardReceivers[simpleName] = $"m_box.Value.{declaringFields[0].FieldName}";
                    }
                }

                // Interface members may instead promote through embedded VALUE struct(s) whose
                // methods are pointer-receiver extensions — dwarf's `type VoidType struct
                // { CommonType }` with `func (c *CommonType) Common()`, including CHAINED embeds
                // (`UintType → BasicType → CommonType`) — CS1929 ×18. The TypeGenerator heap-boxes
                // each embedded field with a public static ref accessor, so the adapter projects
                // the receiver box hop by hop onto the field's box:
                // `m_box.of(UintType.ᏑBasicType).of(BasicType.ᏑCommonType).Common()`. Direct-ж
                // methods bind on the projected box; everything else binds through its deref'd
                // .Value (ref extensions bind on the ref-returning Value, matching the pointer-hop
                // dichotomy above). Each level follows a SINGLE value embed (Go's promotion
                // ambiguity rules make multi-embed satisfaction rare), bounded to 4 hops.
                //
                // Run in TWO passes (see the depth-1 call sited above the interface-field arm): the
                // marker-backed depth-1 level outranks that arm's name heuristic, everything deeper
                // stays below it. A member already bound is skipped, so the second pass only picks up
                // what the first two left.
                bindThroughValueEmbeds(maxDepth: 4);

                void bindThroughValueEmbeds(int maxDepth)
                {
                    foreach (MethodInfo method in methods)
                    {
                        string methodName = GetSimpleName(method.Name);

                        if (forwardReceivers.ContainsKey(methodName))
                            continue;

                        string receiver = "m_box";
                        StructDeclarationSyntax? currentDecl = structDecl;
                        string currentTypeName = structName;
                        INamedTypeSymbol? currentTypeSymbol = structType as INamedTypeSymbol;

                        for (int depth = 0; depth < maxDepth && currentDecl is not null; depth++)
                        {
                            List<(string Name, string TypeName)> valueEmbedHops = currentDecl.GetEmbeddedValueHopNames();

                            if (valueEmbedHops.Count != 1)
                                break;

                            (string embedName, string embedTypeName) = valueEmbedHops[0];

                            // Resolve the embedded field's TYPE SYMBOL from the current struct symbol —
                            // the converter emits the embed as a `partial ref {Type} {Name}` property (and
                            // the TypeGenerator a boxed backing field), so the member named `embedName`
                            // carries the embed type. Needed to probe a FOREIGN embed's methods in
                            // METADATA when it has no local syntax declaration (see below).
                            INamedTypeSymbol? embedTypeSymbol = currentTypeSymbol?
                                .GetMembers(embedName)
                                .Select(member => member switch
                                {
                                    IPropertySymbol property => property.Type,
                                    IFieldSymbol field => field.Type,
                                    _ => null
                                })
                                .OfType<INamedTypeSymbol>()
                                .FirstOrDefault();

                            // The projecting class qualifier is a real identifier position — escape a
                            // keyword-named type (`@fixed.Ꮡn`, not the parse-breaking `fixed.Ꮡn`).
                            receiver = $"{receiver}.of({EscapeCsKeyword(currentTypeName)}.{AddressPrefix}{GetUnsanitizedIdentifier(embedName)})";
                            (currentDecl, Compilation? embedCompilation) = context.GetStructDeclaration(embedTypeName);
                            currentTypeName = embedTypeName;
                            currentTypeSymbol = embedTypeSymbol;

                            if (StructDeclarationSyntaxExtensions.GetBoxReceiverMethodNames(embedTypeName, syntaxContext.SemanticModel.Compilation).Contains(methodName))
                            {
                                forwardReceivers[methodName] = receiver;
                                break;
                            }

                            // The embed method may be a FOREIGN direct-ж extension visible only in
                            // METADATA — a syntax-tree scan cannot see it (database/sql's driverConn
                            // value-embeds sync.Mutex, whose Lock/Unlock are `this ж<Mutex>` extensions in
                            // the compiled sync assembly). Bind the box hop exactly as a local direct-ж
                            // primary does — `m_box.of(driverConn.ᏑMutex).Lock()`, matching the converter's
                            // own call-site form — instead of the bare `m_box.Lock()` fallback (CS1929).
                            if (embedTypeSymbol is not null &&
                                StructDeclarationSyntaxExtensions.GetForeignBoxReceiverMethodNames(embedTypeSymbol).Contains(methodName))
                            {
                                forwardReceivers[methodName] = receiver;
                                break;
                            }

                            if (currentDecl is not null && embedCompilation is not null &&
                                currentDecl.GetExtensionMethods(embedCompilation).Any(m => GetSimpleName(m.Name) == methodName))
                            {
                                forwardReceivers[methodName] = $"{receiver}.Value";
                                break;
                            }

                            // The hop type may promote the member from an embedded INTERFACE of its
                            // OWN — net/http's `closeWriteTestConn` value-embeds `rwTestConn`, which
                            // embeds `io.Reader` and `io.Writer`, so Read and Write live on those
                            // FIELDS' interface values. The depth-1 interface arm above looks at the
                            // adapted struct's own fields only, so both members stayed unbound and
                            // fell to the bare `m_box.Read(p)` default (CS1929 ×2). Ask the same
                            // question at every hop level, keeping the arms in the same order the
                            // outer ladder uses — the hop's own methods first, its embedded
                            // interfaces next, a deeper value embed after that — which is Go's
                            // shallower-wins rule applied one level down. A name SEVERAL of the
                            // hop's interface fields declare is promoted from none of them (Go
                            // rejects the tie), so it is left for the loud default.
                            if (currentTypeSymbol is not null)
                            {
                                List<(string FieldName, HashSet<string> Members)> hopIfaceFields = GetEmbeddedInterfaceFieldMembers(currentTypeSymbol)
                                    .Where(field => field.Members.Contains(methodName))
                                    .ToList();

                                if (hopIfaceFields.Count == 1)
                                {
                                    forwardReceivers[methodName] = $"{receiver}.Value.{hopIfaceFields[0].FieldName}";
                                    break;
                                }
                            }
                        }
                    }
                }

                // PROMOTED members (an embedded INTERFACE field — sort's `type reverse struct
                // { Interface }`) forward through the interface field itself, mirroring the
                // value-form template's promoted arm (`m_box.Len()` has nothing to bind — CS1929).
                // The Promoted flag may live on the pair's SIBLING attribute instance. The FIELD
                // carries the Go embed name — the Δ-stripped simple name when the interface TYPE
                // was collision-renamed (`m_box.Value.ΔHandler` binds nothing, CS1061 — slogtest).
                if (promoted || promotedPairs.Contains($"{structType.ToDisplayString()}|{interfaceType.ToDisplayString()}"))
                {
                    string interfaceFieldName = GetSimpleName(interfaceName, dropCollisionPrefix: true);

                    foreach (MethodInfo method in methods)
                    {
                        string simpleName = GetSimpleName(method.Name);

                        if (!forwardReceivers.ContainsKey(simpleName))
                            forwardReceivers[simpleName] = $"m_box.Value.{interfaceFieldName}";
                    }
                }

                // STRUCT scope by name-exportedness OR declared syntax modifier (the TypeGenerator's
                // `public partial` is a SIBLING generator's output - invisible to this one's symbol,
                // the single-pass limitation); INTERFACE scope by symbol:
                // the golib `error` interface is lowercase yet PUBLIC (its symbol comes from
                // METADATA, so DeclaredAccessibility is reliable), and the name heuristic
                // made io/fs's PathErrorжerror internal - unreachable from os (CS0122 x40).
                // Interface side is symbol-OR-name for the same reason: a SAME-assembly interface
                // (CrossPkgLib.Reporter) gets its public modifier from a sibling generator too.
                string adapterScope = AdapterSidePublic(structType, structName) && AdapterSidePublic(interfaceType, interfaceName) ? "public" : "internal";

                // A LOCAL GENERIC struct (crypto/elliptic's nistCurve[Point nistPoint[Point]]) adapts
                // through ONE generic adapter class over its OPEN type parameters —
                // `nistCurveжCurve<Point> : Curve where Point : nistPoint<Point>` wrapping
                // `ж<nistCurve<Point>>` — that the converter instantiates as
                // `new nistCurveжCurve<ж<P224Point>>(box)`. structName is already the OPEN form
                // (GetFullTypeName spells `nistCurve<Point>`, the type-PARAMETER name), so the
                // class NAME drops to the bare simple name (`nistCurve`) and the `<Point>` list
                // plus the struct's own constraint ride SEPARATELY. The per-instantiation records
                // all collapse to the same open pair here — emit the class once (a second is
                // CS0102).
                //
                // A FOREIGN GENERIC struct is HANDLED, and handled DIFFERENTLY: it takes a
                // NON-generic adapter over the CLOSED instantiation the record names, keyed on that
                // instantiation, with the wrapped type fully qualified. The open-generic route is
                // not merely awkward there, it is unrepresentable — measured on the corpus's first
                // foreign-and-generic pair, `internal/sync.HashTrieMap[any, any]` against sync's own
                // `mapInterface` (Go 1.24.13, sync/map_reference_test.go:32):
                //
                //   * NOT a constraint problem. The converted `partial struct HashTrieMap<K, V>`
                //     declares NO constraint at all, so GetGenericConstraintClause would render "".
                //   * The MEMBER TYPING is what blocks it. mapInterface is NON-generic and every
                //     member is typed at the closed arguments (`Load(any) (any, bool)`), while the
                //     struct's own extensions are typed at the parameters (`Load<K, V>(this
                //     ж<HashTrieMap<K, V>>, K key)`). An adapter generic over `<K, V>` would have to
                //     implement `Load(object)` by passing that `object` where `K` is expected —
                //     CS1503 for every member, for every K that is not object.
                //   * Go's own rule is why this is general rather than incidental: a generic
                //     instantiation satisfies a NON-generic interface only when its substituted
                //     signatures match exactly, so when the interface mentions the type arguments
                //     EXACTLY ONE instantiation can ever satisfy it. A per-instantiation adapter is
                //     what the semantics already describe. (When the interface does NOT mention them
                //     — the nistCurve/Curve shape — every instantiation satisfies it, which is the
                //     local branch above and stays generic.)
                //
                // ⚠ COLLISION HAZARD, named because it is not defended against. The foreign name
                // composes WITHOUT a per-instantiation suffix (`sync_HashTrieMapжmapInterface`), so
                // two DIFFERENT closed instantiations of one foreign generic recorded against one
                // interface compose one class twice — CS0102. A suffix was considered and rejected:
                // the converter must compose the SAME name at the cast site and it spells the
                // arguments in GO-ALIAS form (`any`) where this generator spells them in C# keyword
                // form (`object`), so a name derived from the argument spelling cannot be kept in
                // sync across the two halves — the very failure the name exists to prevent. The key
                // below is therefore the CLOSED instantiation rather than the open definition, so a
                // second instantiation emits a second class and fails LOUDLY at CS0102 instead of
                // silently binding the first instantiation's arguments. Unreachable from Go for an
                // interface that mentions the type arguments (see above); no corpus instance.
                string adapterBaseName = structName;
                string adapterTypeParameters = "";
                string adapterConstraintClause = "";
                string? foreignClosedStructName = null;

                if (structType is INamedTypeSymbol { IsGenericType: true } genericStructType)
                {
                    if (foreignStruct)
                    {
                        if (!emittedGenericPointerAdapters.Add($"closed|{genericStructType.ToDisplayString()}|{interfaceName}"))
                            continue;

                        // The adapter identifier is minted from the symbol's BARE name — never from
                        // a display string, which spells a generic `Name<typeArgs>` and would land
                        // the argument list INSIDE the class identifier (CS0692 plus the
                        // CS0708/CS0540/CS0548/CS0050 cascade, 32 errors in one file).
                        adapterBaseName = genericStructType.Name;

                        // The WRAPPED type takes the symbol's own display string, which carries the
                        // namespace and containing types. GetFullTypeName's generic case renders
                        // `Name<typeArgs>` and drops everything left of the name, so GlobalQualify
                        // found no `go.` prefix to qualify and `ж<HashTrieMap<object, object>>`
                        // resolved to nothing (CS0246). Qualified HERE rather than in the shared
                        // helper so every existing caller of GetFullTypeName stays byte-identical.
                        foreignClosedStructName = GlobalQualify(genericStructType.ToDisplayString());
                    }
                    else
                    {
                        if (!emittedGenericPointerAdapters.Add($"{genericStructType.OriginalDefinition.ToDisplayString()}|{interfaceName}"))
                            continue;

                        adapterBaseName = genericStructType.Name;
                        adapterTypeParameters = $"<{string.Join(", ", genericStructType.TypeParameters.Select(typeParameter => typeParameter.Name))}>";
                        adapterConstraintClause = GetGenericConstraintClause(genericStructType.TypeParameters);
                    }
                }

                // The STRUCT side of a FOREIGN adapter's name: package-qualified, and composed from
                // the bare name on the closed-generic path (where structName still carries the
                // argument list) and from the simple name everywhere else.
                string foreignAdapterBaseName = $"{ForeignPackagePrefix(structType)}{(foreignClosedStructName is null ? GetSimpleName(structName) : adapterBaseName)}";

                // ⚠ THE PROJECTED-RESULT WRAP. A member whose DECLARED result is an interface that
                // the forwarded Go method does not return hands back the receiver box, which is
                // CS0266 inside the generated file — crypto/mlkem's `EncapsulationKey() E` with E
                // bound to the projection. Wrap it in the RESULT interface's own adapter.
                //
                // ⚠ THE TWO SIDES OF THE KEY AGREE BECAUSE THE QUALIFIER DISTRIBUTES OVER THE STRING,
                // which is weaker than what this comment claimed until C1 checked it (mailbox
                // f89515008 §4). The claim was "both sides are composed by the SAME pair of helpers";
                // only the MAP side is. The lookup side is SLICED out of the box's own text —
                // `forwardedReturnType[(boxOpen + 1)..^1]`, the characters between `ж<` and `>`.
                //
                // They match because GlobalQualify is a whole-string regex replace, so it rewrites
                // every root type reference INSIDE the box exactly as it would standing alone. That
                // is a STRING-LEVEL property, not a symbol-level one: were GlobalQualify ever made
                // symbol-aware — a plausible tidy-up — the inner text and the standalone form could
                // differ, the lookup would miss, and the member would fall back to a bare forward.
                //
                // The direction is right either way: every exit from the three gates below is a bare
                // forward, which is CS0266 where a wrap was needed — loud, in the generated file, on
                // the line. A pair the map does not hold (a foreign or generic target) takes that
                // same exit by design.
                Dictionary<string, string> forwardResultWraps = new(StringComparer.Ordinal);

                // ⚠ BOTH forwarding forms are consulted. A direct-ж primary — which is what a Go
                // method needing the real receiver box converts to, and what crypto/mlkem's
                // `EncapsulationKey` is — is invisible to GetExtensionMethods and reaches
                // forwardReceivers through GetBoxReceiverMethodNames, which carries NAMES only. The
                // first cut of this loop read `structMethods` alone, found nothing for the one member
                // it existed for, and left the arm red with every other part of the fix correct.
                // ⚠⚠ THE FOREIGN ARM IS NOT A CONVENIENCE — IT IS THE GATE crypto/mlkem's ROW REACHED.
                // This read `new Dictionary(...)` for a foreign struct until 2026-09-20, so a foreign
                // struct had NO forwarded return types and the wrap below took its first `continue`
                // before the map was ever consulted. The white-box `-tests` model makes the struct
                // foreign ALWAYS, which is why the row read CS0266 ×2 while every local pair wrapped.
                Dictionary<string, string> forwardReturnTypes = structDecl is null
                    ? StructDeclarationSyntaxExtensions.GetForeignBoxReceiverMethodReturnTypes(structType)
                    : StructDeclarationSyntaxExtensions.GetBoxReceiverMethodReturnTypes(structDecl.Identifier.Text, compilation!);

                foreach (MethodInfo structMethod in structMethods ?? [])
                    forwardReturnTypes[structMethod.Name] = structMethod.ReturnType;

                foreach (MethodInfo interfaceMethod in methods)
                {
                    // ⚠ THIS KEY IS ESCAPED AND ITS NEIGHBOURS ARE NOT, and the reason it is benign is
                    // that the two misses CANCEL — which is a worse guarantee than it looks and is why
                    // C2 asked for it in writing (mailbox 788a42262 §4, ruled in at 628ba865c).
                    //
                    // `forwardReceivers`, `forwardStaticCalls` and `forwardReturnTypes` are all keyed
                    // by the struct's RAW declared names, and AdapterImplTemplate reads all of them —
                    // this map included — with the UNESCAPED `GetSimpleName(method.Name)`. So for a
                    // keyword-named member (gob's `string()`) this loop composes `@string` where the
                    // template will later ask for `string`. The lookup on the very next line misses
                    // FIRST, the iteration continues, and nothing is ever registered under the escaped
                    // key: no wrap is emitted, and a wrap that is needed and absent is CS0266 in the
                    // generated file rather than a silent wrong answer.
                    //
                    // ⚠ Benign BY CANCELLATION, not by design: fixing either key alone un-cancels it.
                    // Escape the neighbours and this map would register under a key the template never
                    // asks for; unescape this one and it would register correctly — which is the right
                    // direction, and is the one-line change to make if a keyword-named member ever
                    // needs a projected-result wrap. No corpus record does today.
                    string memberName = GetSimpleName(EscapeCsKeyword(interfaceMethod.Name));
                    string forwardMember = interfaceMethod.ForwardMemberName(memberName);

                    if (!forwardReturnTypes.TryGetValue(forwardMember, out string? forwardedReturnType) ||
                        string.Equals(forwardedReturnType, interfaceMethod.ReturnType, StringComparison.Ordinal))
                        continue;

                    int boxOpen = forwardedReturnType.IndexOf('<');

                    // The forwarded result must be a receiver BOX for this to be the projection's
                    // shape at all: `ж<T>`. Anything else that merely differs from the declared
                    // return type is someone else's defect and is left to the compiler.
                    if (boxOpen <= 0 || !forwardedReturnType.EndsWith(">", StringComparison.Ordinal) ||
                        !forwardedReturnType[..boxOpen].EndsWith(PointerPrefix, StringComparison.Ordinal))
                        continue;

                    string boxedType = forwardedReturnType[(boxOpen + 1)..^1];

                    if (localPointerAdapterNames.TryGetValue($"{boxedType}|{interfaceMethod.ReturnType}", out string? resultAdapter))
                        forwardResultWraps[forwardMember] = resultAdapter;
                }
                // The INTERFACE side of the adapter's NAME, the struct side's rule one operand over: a
                // record naming a CLOSED instantiation of a GENERIC interface must not land the
                // argument list inside the class IDENTIFIER. GetFullTypeName spells a generic
                // `Name<typeArgs>` with the arguments rendered from their display strings, so the
                // last-dot scan inside GetSimpleName runs INSIDE the list and yields the argument's
                // own tail segment (`digestжnamed>` for `keyedLike<go.…​.named>`) — not a name any
                // class can carry, and the same shape the seat fixed on the struct side. Dropped
                // BEFORE the qualifier scan, the order splitAdapterStructReference documents.
                //
                // ⚠ ONLY the minted NAME takes the strip. The two collision KEYS — the pre-pass's
                // grouping key and the lookup below — keep the last-dot-only reduction, because the
                // converter's adapterInterfaceSimpleName keeps it too: both halves garble a generic
                // interface reference IDENTICALLY, which is parity, and stripping on one side alone
                // would manufacture the divergence AdapterStructKey exists to prevent. The
                // consequence is the seat's ruled behaviour, now symmetric across both operands: two
                // records that compose one class name without being seen as a collision fail LOUDLY
                // at CS0102 instead of binding the first one silently.
                string adapterInterfaceName = GetUnsanitizedIdentifier(GetSimpleName(StripGenericTypeArguments(interfaceName)));

                string adapterSource = new AdapterImplTemplate
                {
                    PackageNamespace = packageNamespace,
                    PackageName = packageName,
                    // FULLY-qualified for a foreign struct (no local using guarantees resolution;
                    // mirrors the value adapter's same-named-type shadow rationale). A GENERIC
                    // struct uses its OPEN form (`nistCurve<Point>`) for the wrapped field — the
                    // GoImplement record may carry a CLOSED instantiation (`nistCurve<P224Pointж…>`)
                    // once the proxy makes it constraint-satisfiable, but the adapter class is
                    // generic, so `adapterBaseName + adapterTypeParameters` reconstructs the open form.
                    // A LOCAL name is a bare SYMBOL name — UNescaped, unlike display strings — so a
                    // keyword-named struct must be "@"-escaped here or `ж<fixed>` breaks the parse
                    // (the CS0708 'main_package.' cascade). No-op for every other name.
                    StructName = foreignStruct ? foreignClosedStructName ?? GlobalQualify(structType.GetFullTypeName(true)) : $"{EscapeCsKeyword(adapterBaseName)}{adapterTypeParameters}",
                    InterfaceName = interfaceName,
                    // Adapter class name composes with the shared pointer glyph (CatжAnimal) - always
                    // via Symbols.PointerPrefix so a future symbol change follows automatically.
                    // A FOREIGN struct's name is PACKAGE-QUALIFIED (os_FileжReader): two
                    // same-named foreign structs adapting to one interface otherwise compose
                    // a single colliding class (math/big records both bytes.Reader and
                    // strings.Reader against io.ByteScanner - CS0102/CS0111/CS8646). The
                    // package name comes from the containing package class (bytes_package). A
                    // GENERIC struct uses the bare simple name (`nistCurve`) with the `<Point>`
                    // list supplied via TypeParameters, so the args do not bake into the name.
                    // The interface side composes UNESCAPED: a keyword-named interface arrives
                    // "@"-escaped from its display string, and an interior marker (`fixedж@lock`)
                    // lexes as two tokens; the composed name is never a keyword. Keep in sync
                    // with the converter's adapterTypeRef composition.
                    // The interface side takes a package qualifier ONLY when this name is one the
                    // pre-pass found more than one interface composing (see adapterNameGroups) —
                    // flate's own `Reader` vs `io.Reader`, both reached from *bufio.Reader.
                    //
                    // ⚠ THIS PROBE ASKS THE SET IN A SPELLING THE SET WAS NOT BUILT FROM, and the two
                    // coincide for every non-generic interface but not necessarily for a generic one
                    // (C2, mailbox 788a42262 §2, ruled in at 628ba865c). The pre-pass registers with
                    // `GetSimpleName(interfaceType.ToDisplayString())`; this line asks with
                    // `GetSimpleName(interfaceName)`, where interfaceName is
                    // `GlobalQualify(GetFullTypeName(true))` — a different rendering of the same
                    // symbol. For a generic interface the two can differ, and then this probe misses
                    // a group it belongs to and the qualifier is not applied.
                    //
                    // ⚠ The projected-result map above sides with the REGISTRATION, deliberately. So
                    // if a row ever needs these two unified, the direction is TOWARD the pre-pass's
                    // spelling and this line is the one that moves — the reverse of "make it agree
                    // with the main loop". And it is a PAIRED seat when it comes: the converter's
                    // `adapterInterfaceSimpleName` leaves the same operand unstripped on purpose, so
                    // both halves garble a generic interface reference identically today, and moving
                    // one alone would manufacture the divergence AdapterStructKey exists to prevent.
                    //
                    // ⚠ THE PASS SEPARATION reads the record's own `Production` facet here, and the
                    // group key it asks with is this same expression's — so a production member of a
                    // colliding group composes exactly what the production pass already wrote into
                    // the .cs this assembly recompiles. See KeepsProductionAdapterName.
                    AdapterName = $"{(foreignStruct ? foreignAdapterBaseName : adapterBaseName)}{PointerPrefix}{(collidingAdapterNames.Contains($"{AdapterStructKey(structType, packageClassName)}{PointerPrefix}{GetUnsanitizedIdentifier(GetSimpleName(interfaceName))}") && !KeepsProductionAdapterName($"{AdapterStructKey(structType, packageClassName)}{PointerPrefix}{GetUnsanitizedIdentifier(GetSimpleName(interfaceName))}", production, productionFacetCounts) ? AdapterInterfacePrefix(interfaceType, packageClassName) : "")}{adapterInterfaceName}",
                    TypeParameters = adapterTypeParameters,
                    ConstraintClause = adapterConstraintClause,
                    AdapterScope = adapterScope,
                    Methods = methods,
                    ForwardReceivers = forwardReceivers,
                    ForwardStaticCalls = forwardStaticCalls,
                    ForwardResultWraps = forwardResultWraps,
                    ImplementsFormattable = implementsFormattable,
                    UsingStatements = usingStatements
                }
                .Generate();

                context.AddSource(GetUniqueHintName(emittedHintNames, GetValidFileName($"{packageNamespace}.{packageClassName}.{structName}-{interfaceName}-ptr.g.cs")), adapterSource);
                continue;
            }

            // A FOREIGN struct (no local declaration to partial - it lives in another
            // assembly) with a value conversion takes the VALUE adapter: a class wrapping a
            // COPY (Go value semantics), forwarding through the foreign package's extension
            // methods via the file's usings (os's Signal interface over syscall.Signal -
            // neither assembly can partial the other, exec_posix CS1503). A local NAMED FUNC
            // type (a DELEGATE - flag's funcValue) takes the same route: a delegate cannot
            // be a partial struct, and its Go methods are package extension methods that
            // bind on the wrapped copy.
            if (structType.TypeKind == TypeKind.Delegate ||
                (structDecl is null && !SymbolEqualityComparer.Default.Equals(structType.ContainingAssembly, syntaxContext.SemanticModel.Compilation.Assembly)))
            {
                // Symbol-OR-name on BOTH sides (mirrors the pointer arm): a public adapter
                // whose ctor takes an INTERNAL wrapped type is CS0051 (flag's internal
                // funcValue delegate under the public Value interface).
                string valueAdapterScope = AdapterSidePublic(interfaceType, interfaceName) && AdapterSidePublic(structType, structName) ? "public" : "internal";

                // A FOREIGN struct can satisfy the interface by PROMOTION through an embedded
                // interface FIELD instead of by a method of its own. Index those members here — a
                // member the struct itself declares (in either receiver form) always wins, and a
                // name SEVERAL embedded interfaces declare is promoted from none of them, exactly
                // as the pointer adapter's own interface-field arm decides it.
                Dictionary<string, string> valuePromotedFieldForwards = new(StringComparer.Ordinal);

                if (structType is INamedTypeSymbol valueStructType)
                {
                    List<(string FieldName, HashSet<string> Members)> valueIfaceFields = GetEmbeddedInterfaceFieldMembers(valueStructType);

                    if (valueIfaceFields.Count > 0)
                    {
                        HashSet<string> ownValueMembers = new(StructDeclarationSyntaxExtensions.GetForeignValueReceiverMethods(valueStructType).Keys, StringComparer.Ordinal);

                        ownValueMembers.UnionWith(StructDeclarationSyntaxExtensions.GetForeignBoxReceiverMethodNames(valueStructType));

                        foreach (MethodInfo valueMethod in methods)
                        {
                            string valueSimpleName = GetSimpleName(valueMethod.Name);

                            if (ownValueMembers.Contains(valueSimpleName))
                                continue;

                            List<(string FieldName, HashSet<string> Members)> valueDeclaringFields = valueIfaceFields
                                .Where(field => field.Members.Contains(valueSimpleName))
                                .ToList();

                            if (valueDeclaringFields.Count == 1)
                                valuePromotedFieldForwards[valueSimpleName] = valueDeclaringFields[0].FieldName;
                        }
                    }
                }

                string valueAdapterSource = new ValueAdapterImplTemplate
                {
                    PromotedFieldForwards = valuePromotedFieldForwards,
                    PackageNamespace = packageNamespace,
                    PackageName = packageName,
                    // FULLY-qualified: the bare name resolves to the LOCAL same-named type
                    // inside this package class (os's ΔSignal interface shadowed syscall's
                    // ΔSignal struct - the adapter field/ctor typed the wrong side).
                    StructName = GlobalQualify(structType.GetFullTypeName(true)),
                    InterfaceName = interfaceName,
                    // Composes with Symbols.ValueAdapterInfix - the value sibling of the
                    // PointerPrefix-composed pointer adapters. A FOREIGN struct's name is
                    // PACKAGE-QUALIFIED (syscall_ΔSignalᴠΔSignal), mirroring the pointer arm's
                    // same-simple-name collision guard; a LOCAL delegate stays bare. Both simple
                    // names compose UNESCAPED (an interior "@" marker lexes as two tokens); keep
                    // in sync with the converter's valueAdapterTypeRef composition.
                    AdapterName = $"{(structDecl is null && !SymbolEqualityComparer.Default.Equals(structType.ContainingAssembly, syntaxContext.SemanticModel.Compilation.Assembly) ? ForeignPackagePrefix(structType) : "")}{GetUnsanitizedIdentifier(GetSimpleName(structName))}{ValueAdapterInfix}{GetUnsanitizedIdentifier(GetSimpleName(interfaceName))}",
                    AdapterScope = valueAdapterScope,
                    ImplementsFormattable = implementsFormattable,
                    Methods = methods,
                    UsingStatements = usingStatements
                }
                .Generate();

                context.AddSource(GetUniqueHintName(emittedHintNames, GetValidFileName($"{packageNamespace}.{packageClassName}.{structName}-{interfaceName}-val.g.cs")), valueAdapterSource);
                continue;
            }

            // A GENERIC struct partials at its OPEN definition (see emittedGenericValueImpls) —
            // the declaration name carries the type-PARAMETER names, and every closed record for
            // the same open pair folds into the first. Constraints are deliberately omitted: a
            // partial declaration may leave them off, they merge from the converter's own
            // declaration, and omitting them can never produce CS0265.
            string implStructName = EscapeCsKeyword(structName);
            string implHintName = structName;
            string implTypeKey = structType.ToDisplayString();

            if (structType is INamedTypeSymbol { IsGenericType: true } genericValueStruct)
            {
                string openTypeParameters = $"<{string.Join(", ", genericValueStruct.TypeParameters.Select(typeParameter => typeParameter.Name))}>";
                implStructName = $"{EscapeCsKeyword(genericValueStruct.Name)}{openTypeParameters}";
                implHintName = $"{genericValueStruct.Name}{openTypeParameters}";
                implTypeKey = genericValueStruct.OriginalDefinition.ToDisplayString();

                if (!emittedGenericValueImpls.Add($"{implTypeKey}|{interfaceName}"))
                    continue;
            }

            // One value-form impl per pair — a plain + Promoted duplicate would re-emit the
            // comparison operators (CS0111); the promotedPairs pre-index folds the flag in.
            if (!emittedValuePairs.Add($"{implTypeKey}|{interfaceType.ToDisplayString()}"))
                continue;

            // Drop members already emitted in an earlier partial of the SAME struct (a member
            // inherited from an embedded interface shared by two implemented interfaces — see
            // emittedPartialMembers). The earlier partial's impl satisfies it for the whole struct;
            // re-declaring it here is CS0111/CS8646. An empty resulting partial is valid.
            List<MethodInfo> partialMethods = methods
                .Where(method => emittedPartialMembers.Add($"{implTypeKey}|{method.Name}"))
                .ToList();

            string generatedSource = new InterfaceImplTemplate
            {
                PackageNamespace = packageNamespace,
                PackageName = packageName,
                // A bare SYMBOL name arrives UNescaped (unlike display strings) — a keyword-named
                // struct must be "@"-escaped or `partial struct fixed` parses as a fixed-size
                // buffer (the reported CS0708 'main_package.' cascade). No-op otherwise. A
                // generic struct arrives here in its OPEN form.
                StructName = implStructName,
                InterfaceName = interfaceName,
                // The comparison operators this partial emits take BOTH sides as parameters, so
                // their scope may not out-rank either. The template can only see NAMES, and a name
                // is the wrong oracle for a lifted function-local type — `TestInterfaceSet_s_P`
                // reads public off its enclosing Test while the declaration is `internal`, and a
                // public `operator ==(Point, TestInterfaceSet_s_P)` is CS0057. Resolve both sides
                // here, where the SYMBOLS are, and hand the answer down.
                StructIsPublic = AdapterSidePublic(structType, implStructName),
                InterfaceIsPublic = AdapterSidePublic(interfaceType, interfaceName),
                // Same reason, same remedy: the promoted twins address the embed FIELD, and deriving
                // its name from the interface's addresses nothing when a function-local lift renamed
                // the type out from under it.
                EmbedFieldName = ResolveEmbedFieldName(structType, interfaceType),
                Promoted = promoted || promotedPairs.Contains($"{structType.ToDisplayString()}|{interfaceType.ToDisplayString()}"),
                Overrides = overrides,
                Methods = partialMethods,
                EmbedHop = embedHop,
                EmbedHopBoxMethods = embedHopBoxMethods,
                EmbedHopDeepPaths = embedHopDeepPaths,
                // The value form promotes through embedded pointers exactly as the adapter does — a
                // pointer embed's method set is in the STRUCT's method set too, so `var rw ReadWriter =
                // p` (no `&`) records this pair — and it reached the same bare `this.Read(p)` fallback
                // when several embeds left no single hop to name.
                MultiEmbedHopPaths = multiEmbedHopPaths,
                // An interface member with NO direct struct method and a SINGLE VALUE embed
                // must promote through the embedded field (Go's promotion is what type-checked
                // it) - net's `addrPortUDPAddr struct { netip.AddrPort }`: the bare
                // `this.String()` bound an unrelated same-package extension by NAME (CS1929);
                // `this.AddrPort.String()` binds the embed's value-receiver method.
                ValueEmbedHop = embedHop is null && (structDecl?.GetEmbeddedValueHopNames() is [var singleValueHop]) ? singleValueHop.Name : null,
                // A FOREIGN value embed's extensions live in another namespace segment
                // (netip_package sits in go.net; the source file only ALIASES it, which does
                // not import extensions) - call the package-class static directly:
                // `global::go.net.netip_package.String(this.AddrPort)`. The class is read off the
                // embed's SYMBOL: the spelled type name is already `global::go.…` in an internal test
                // and `go.<module ns>.…` in an external one, and prefixing either again names nothing
                // (`global::go.global::go.…` CS7000, `global::go.go.…` CS0234: golang-jwt's tests).
                ValueEmbedHopStaticClass = embedHop is null && (structDecl?.GetEmbeddedValueHopNames() is [var svh]) && svh.TypeName.Contains('.')
                    ? ResolveValueEmbedPackageClass(structType, svh.Name) ?? "global::go." + svh.TypeName.Substring(0, svh.TypeName.LastIndexOf('.'))
                    : null,
                UsingStatements = usingStatements
            }
            .Generate();

            // Add the source code to the compilation. The hint name uses the same OPEN form the
            // declaration does, so a generic struct's one partial does not take its file name
            // from whichever instantiation the record list happened to list first.
            context.AddSource(GetUniqueHintName(emittedHintNames, GetValidFileName($"{packageNamespace}.{packageClassName}.{implHintName}-{interfaceName}.g.cs")), generatedSource);
        }
    }

    /// <summary>
    /// Indexes, for a struct with SEVERAL embedded POINTER fields, the hop path each promoted method
    /// name forwards through — the embed's ж field itself (<c>PipeReader</c>) for a direct-ж primary,
    /// its deref'd value (<c>Reader.Value</c>) for a value/ref-receiver method. A name TWO embeds
    /// declare is dropped: Go's depth-1 promotion rejects the tie, so nothing is promoted and only a
    /// method the struct declares itself can satisfy the member (jsonrpc's <c>*pipe.Close</c>, over
    /// the <c>Close</c> both <c>*io.PipeReader</c> and <c>*io.PipeWriter</c> declare).
    /// </summary>
    /// <remarks>
    /// The single-embed case does not come here — its one hop takes every unbound member
    /// unconditionally, since that member's promotion is what type-checked the cast, and no per-member
    /// evidence is needed. It is only with several embeds that the receiver must be decided per member,
    /// which is also what the FOREIGN-struct arm does from metadata for a struct declared elsewhere.
    /// Each embed's method set is read from local SYNTAX where its type is declared in this compilation
    /// and from METADATA where it is not — a referenced assembly exposes both the converter's direct-ж
    /// primaries and the public <c>RecvGenerator</c> ж-twins as ordinary symbols, which is the whole
    /// jsonrpc case (<c>*io.PipeReader</c>/<c>*io.PipeWriter</c> live in the compiled io assembly).
    /// A member neither resolution places is left out, so it keeps the caller's existing fallback.
    /// </remarks>
    private static Dictionary<string, string> GetMultiEmbedHopPaths(GeneratorExecutionContext context, Compilation compilation, ITypeSymbol structType, List<(string Name, string TypeName)> embedHops)
    {
        List<(string Name, INamedTypeSymbol Type)> pointerEmbeds = StructDeclarationSyntaxExtensions.GetPointerEmbeds(structType);
        Dictionary<string, string> hopPaths = new(StringComparer.Ordinal);
        HashSet<string> ambiguous = new(StringComparer.Ordinal);

        foreach ((string hopName, string hopTypeName) in embedHops)
        {
            HashSet<string> boxMethods = StructDeclarationSyntaxExtensions.GetBoxReceiverMethodNames(hopTypeName, compilation);
            HashSet<string> valueMethods = new(StringComparer.Ordinal);

            (StructDeclarationSyntax? hopDecl, Compilation? hopCompilation) = context.GetStructDeclaration(hopTypeName);

            if (hopDecl is not null && hopCompilation is not null)
                valueMethods.UnionWith(hopDecl.GetExtensionMethods(hopCompilation).Select(method => GetSimpleName(method.Name)));

            INamedTypeSymbol? hopElement = pointerEmbeds.FirstOrDefault(embed => embed.Name == hopName).Type;

            if (hopElement is not null && !SymbolEqualityComparer.Default.Equals(hopElement.ContainingAssembly, compilation.Assembly))
            {
                boxMethods.UnionWith(StructDeclarationSyntaxExtensions.GetForeignBoxReceiverMethodNames(hopElement));
                valueMethods.UnionWith(StructDeclarationSyntaxExtensions.GetForeignValueReceiverMethods(hopElement).Keys);
            }

            foreach (string methodName in boxMethods.Union(valueMethods))
            {
                // Seen on an EARLIER embed: ambiguous at depth 1, so Go promotes it from neither.
                if (!hopPaths.ContainsKey(methodName))
                    hopPaths[methodName] = boxMethods.Contains(methodName) ? hopName : $"{hopName}.Value";
                else
                    ambiguous.Add(methodName);
            }
        }

        foreach (string methodName in ambiguous)
            hopPaths.Remove(methodName);

        return hopPaths;
    }

    /// <summary>
    /// Gets the disambiguating package prefix ("bytes_") for a FOREIGN struct's local adapter
    /// class name, derived from its containing package class ("bytes_package") — matching the
    /// converter's <c>getSanitizedIdentifier(pkg.Name()) + "_"</c> composition at the cast site.
    /// </summary>
    /// <summary>
    /// Gets the adapter-name key for the STRUCT side: the package-prefixed simple name for a
    /// foreign struct ("bufio_Reader"), the bare simple name for one declared by this package.
    /// Locality is decided by the containing package class, matching the converter's rule that a
    /// LOCAL type reference is written bare in the GoImplement record while a foreign one is
    /// qualified — the two must agree or the collision groups diverge.
    /// </summary>
    /// <remarks>
    /// A generic type-argument list is dropped FIRST and the last path segment taken SECOND, which is
    /// the order <c>splitAdapterStructReference</c> documents on the converter's half
    /// (<c>"bytes_package.Reader&lt;int&gt;"</c> → <c>("bytes_package", "Reader")</c>) and the order
    /// this key must repeat, or the two halves group one struct two ways. Reversed — which is what
    /// <c>GetSimpleName</c> alone does, splitting on the last '.' and dropping generics only when
    /// asked, which it cannot usefully be here because by then the split has run — the last-dot scan
    /// lands INSIDE the argument list and the "simple name" becomes the argument's own tail segment:
    /// <c>nistCurve&lt;P224PointжnistPoint&gt;</c> keyed as <c>P224PointжnistPoint&gt;</c>, and a
    /// nested <c>a.G&lt;b.T&gt;</c> as <c>T&gt;</c>. Composed at the CALL SITE rather than by flipping
    /// <c>GetSimpleName</c>'s internals, for the seat's own reason and one more: that helper
    /// dereferences a <c>ж&lt;T&gt;</c> box form before splitting, and stripping generics inside it
    /// would eat the form instead of an argument list. This key never receives one — a GoImplement's
    /// first type argument is the struct itself, never its box (measured: 0 of 2,752 committed
    /// records spell one) — so the call site is the only safe place for it.
    /// </remarks>
    /// <summary>
    /// Decides whether a colliding pointer adapter KEEPS the unprefixed name its production text
    /// already spells — the pass separation.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Under the recompile test model the production <c>.cs</c> files are compile items of this
    /// assembly, so this generator reads the UNION of both halves' records — but the production text
    /// was rendered in the production pass against the production set alone and cannot be
    /// re-rendered. A test-half record joining a production record's collision group must therefore
    /// not rename the production member. <c>crypto/sha3</c> is the corpus instance: production
    /// records <c>&lt;SHA3, hash.Hash&gt;</c>, the external test half adds
    /// <c>&lt;SHA3, fips140.Hash&gt;</c>, both compose <c>SHA3жHash</c>, and the ordinary rule
    /// prefixes BOTH — so <c>sha3.cs</c>'s four sites name a class never emitted.
    /// </para>
    /// <para>
    /// EXACTLY ONE faceted member is the separable case and the only one this returns true for. TWO
    /// OR MORE is a collision the PRODUCTION pass already saw and already resolved, so the production
    /// text spells the PREFIXED names and the ordinary rule is what reproduces them — exempting one
    /// of them would rename a production site in the opposite direction. ZERO is every production
    /// compilation and both reference test models, where this is inert by construction.
    /// </para>
    /// <para>
    /// ⚠ The caller asks with the SAME group key it tests <c>collidingAdapterNames</c> with, and must:
    /// the two questions are about one group, and composing the key twice from two spellings is the
    /// defect <c>AdapterStructKey</c> exists to prevent. Keep in sync with the converter's
    /// <c>adapterProductionNameKeepers</c> in <c>adapterNameCollisions.go</c>, which resolves the
    /// matching cast-site references by the same rule over the same records.
    /// </para>
    /// </remarks>
    private static bool KeepsProductionAdapterName(string groupKey, bool production, Dictionary<string, int> productionFacetCounts)
    {
        return production && productionFacetCounts.TryGetValue(groupKey, out int facetedMembers) && facetedMembers == 1;
    }

    private static string AdapterStructKey(ITypeSymbol structType, string packageClassName)
    {
        string simpleName = GetUnsanitizedIdentifier(GetSimpleName(StripGenericTypeArguments(structType.ToDisplayString())));
        string? container = structType.ContainingType?.Name;

        if (container is null || !container.EndsWith(PackageSuffix) || container == packageClassName)
            return simpleName;

        return $"{ForeignPackagePrefix(structType)}{simpleName}";
    }

    /// <summary>
    /// Gets the disambiguating package prefix for the INTERFACE side of a COLLIDING adapter name
    /// ("io_" in "bufio_Readerжio_Reader"). Empty for an interface this package declares: at most
    /// one member of a colliding group can be local, so leaving it bare stays unambiguous and
    /// keeps the Go-like short form for the package's own interface.
    /// </summary>
    private static string AdapterInterfacePrefix(ITypeSymbol interfaceType, string packageClassName)
    {
        string? container = interfaceType.ContainingType?.Name;

        if (container is null || !container.EndsWith(PackageSuffix) || container == packageClassName)
            return string.Empty;

        return $"{container.Substring(0, container.Length - PackageSuffix.Length)}_";
    }

    /// <summary>
    /// Decides whether a foreign package-class extension's FIRST parameter names the struct under
    /// adaptation — the box form's <c>ж&lt;T&gt;</c> argument, or a <c>[GoRecv]</c> ref extension's
    /// receiver.
    /// </summary>
    /// <remarks>
    /// Plain symbol equality answers NO for a CONSTRUCTED generic: the record names
    /// <c>HashTrieMap&lt;object, object&gt;</c> while the extensions are declared over the OPEN
    /// <c>Load&lt;K, V&gt;(this ж&lt;HashTrieMap&lt;K, V&gt;&gt;, K key)</c>, so nothing bound and
    /// every member fell back to <c>m_box.Value.&lt;name&gt;</c> — which binds nothing either, because
    /// the converter emits a Go pointer-receiver method as a package-class EXTENSION and not as an
    /// instance member of the struct. Comparing ORIGINAL DEFINITIONS binds the right method: the
    /// extension is generic over the struct's own parameters, so it infers them from the closed box
    /// at the call site and needs no explicit argument list. Inert for a non-generic struct, whose
    /// original definition is itself.
    /// </remarks>
    private static bool IsForeignReceiverOf(ITypeSymbol receiverParameterType, ITypeSymbol structType)
    {
        if (SymbolEqualityComparer.Default.Equals(receiverParameterType, structType))
            return true;

        return structType is INamedTypeSymbol { IsGenericType: true } &&
               SymbolEqualityComparer.Default.Equals(receiverParameterType.OriginalDefinition, structType.OriginalDefinition);
    }

    private static string ForeignPackagePrefix(ITypeSymbol structType)
    {
        string? packageClassName = structType.ContainingType?.Name;

        if (packageClassName is null || !packageClassName.EndsWith(PackageSuffix))
            return string.Empty;

        return $"{packageClassName.Substring(0, packageClassName.Length - PackageSuffix.Length)}_";
    }

    // Renders the C# `where T : …` clauses for a GENERIC struct's adapter from the struct's own
    // type parameters. The adapter wraps `ж<nistCurve<Point>>`, so it must repeat every constraint
    // nistCurve itself declares (`where Point : nistPoint<Point>`) or the wrapped field is CS0314.
    // Constraint ORDER follows C#'s required sequence: the class/struct/unmanaged/notnull primary
    // constraint first, then base + interface types, then new() last. Interface constraints (the
    // Go case) are global::-qualified like every other generated type reference.
    private static string GetGenericConstraintClause(IEnumerable<ITypeParameterSymbol> typeParameters)
    {
        List<string> clauses = new();

        foreach (ITypeParameterSymbol typeParameter in typeParameters)
        {
            List<string> constraints = new();

            if (typeParameter.HasReferenceTypeConstraint)
                constraints.Add("class");
            else if (typeParameter.HasValueTypeConstraint)
                constraints.Add("struct");
            else if (typeParameter.HasUnmanagedTypeConstraint)
                constraints.Add("unmanaged");
            else if (typeParameter.HasNotNullConstraint)
                constraints.Add("notnull");

            foreach (ITypeSymbol constraintType in typeParameter.ConstraintTypes)
                constraints.Add(GlobalQualify(constraintType.ToDisplayString()));

            if (typeParameter.HasConstructorConstraint)
                constraints.Add("new()");

            if (constraints.Count > 0)
                clauses.Add($"where {typeParameter.Name} : {string.Join(", ", constraints)}");
        }

        return clauses.Count > 0 ? $" {string.Join(" ", clauses)}" : string.Empty;
    }

    // Emits a SELF-REFERENTIAL constraint proxy for a GoImplement(ConstraintProxy = true) record:
    // `elementжinterface : interface<itself>` wrapping ж<element>, with implicit ж<element>↔proxy
    // conversions and one forwarder per interface method (the body forwards to the box's like-named
    // extension; the implicit conversions marshal every self-typed T argument/result). See
    // ConstraintProxyImplTemplate for the rationale.
    private static void EmitConstraintProxy(GeneratorExecutionContext context, ITypeSymbol elementType, INamedTypeSymbol interfaceType, string packageNamespace, string packageName, string packageClassName, string[] usingStatements, HashSet<string> emittedHintNames, HashSet<string> emittedProxies)
    {
        INamedTypeSymbol interfaceDef = interfaceType.OriginalDefinition;

        if (interfaceDef.TypeParameters.Length != 1)
            return;

        // One proxy per (element, open-interface) pair — the converter records it at every
        // constrained instantiation site (nistCurve[*P224Point] appears several times).
        if (!emittedProxies.Add($"proxy|{elementType.OriginalDefinition.ToDisplayString()}|{interfaceDef.ToDisplayString()}"))
            return;

        // Proxy name element-simple + ж + interface-simple — MUST match the converter's
        // type-argument rendering at the constrained instantiation.
        string proxyName = $"{elementType.Name}{PointerPrefix}{interfaceDef.Name}";

        // The boxed pointee, fully qualified so a FOREIGN element resolves (nistec.P224Point used
        // from crypto/elliptic).
        string elementName = GlobalQualify(elementType.GetFullTypeName(true));

        // The interface closed over the proxy ITSELF: `nistPoint<P224PointжnistPoint>`.
        string interfaceRef = $"{StripGenericTypeArguments(GlobalQualify(interfaceDef.ToDisplayString()))}<{proxyName}>";

        ITypeParameterSymbol selfParameter = interfaceDef.TypeParameters[0];
        StringBuilder methods = new();

        // A Go constraint interface may EMBED other interfaces, and Go embedding emits as C#
        // interface inheritance (`interface Constrained<T> : Middle`, `interface Middle : Base`).
        // The proxy must forward EVERY member of its method set, not only the ones the constraint
        // declares directly: a member reached through an embed is still part of the interface the
        // proxy claims to implement, and omitting it leaves the proxy not implementing its own
        // interface (CS0535, one per inherited member). net/http's whole test suite sat behind
        // this — `type TBRun[T any] interface { testing.TB; Run(string, func(T)) bool }` gave
        // proxies for *testing.T and *testing.B that each forwarded only Run and were missing all
        // 18 members of the embedded testing.TB (36 diagnostics).
        //
        // C# explicit interface implementation must name the interface that DECLARES the member —
        // `void Derived.M()` is CS0539 when M comes from Base — so each member is qualified by its
        // own declaring interface, closed over the proxy wherever it names the self parameter
        // (`Bar<T>` embedded in `Foo<T>` forwards as `Bar<proxy>.M`).
        //
        // The constraint's OWN members are emitted FIRST, in declaration order, exactly as before,
        // so every proxy that embeds nothing is byte-identical to what this always produced. The
        // embedded interfaces follow, ordered by their rendered reference so the emission is
        // deterministic regardless of the order AllInterfaces happens to report (it is transitive,
        // so a two-level embed needs no recursion of our own).
        List<(string DeclaringRef, INamedTypeSymbol Interface)> methodSets = [(interfaceRef, interfaceDef)];

        methodSets.AddRange(interfaceDef.AllInterfaces
            .Select(embedded => (DeclaringRef: RenderWithProxy(embedded, selfParameter, proxyName), Interface: embedded))
            .OrderBy(entry => entry.DeclaringRef, StringComparer.Ordinal));

        foreach ((string declaringRef, INamedTypeSymbol methodSet) in methodSets)
        {
            foreach (IMethodSymbol method in methodSet.GetMembers().OfType<IMethodSymbol>().Where(member => member.MethodKind == MethodKind.Ordinary && !member.IsStatic))
            {
                string methodName = EscapeCsKeyword(method.Name);
                string returnType = RenderWithProxy(method.ReturnType, selfParameter, proxyName);
                string parameters = string.Join(", ", method.Parameters.Select((parameter, index) => $"{RenderWithProxy(parameter.Type, selfParameter, proxyName)} {SafeParameterName(parameter.Name, index)}"));
                string arguments = string.Join(", ", method.Parameters.Select((parameter, index) => RenderForwardedArgument(parameter.Type, SafeParameterName(parameter.Name, index), selfParameter, elementName)));

                if (methods.Length > 0)
                    methods.Append("\r\n\r\n        ");

                methods.Append($"{returnType} {declaringRef}.{methodName}({parameters}) => m_box.{methodName}({arguments});");
            }
        }

        // Each forwarder calls the boxed element's box extension methods (`m_box.Bytes()`), declared
        // in the element type's PACKAGE class; bring that class's NAMESPACE into scope so a FOREIGN
        // element's extensions resolve. The [GoImplement] attribute sits in package_info.cs, whose
        // usings never cover the element (nistec's P224Point used from crypto/elliptic — without this
        // `m_box.Bytes()`/`.SetBytes()` bind nothing, CS1929/CS1501).
        string[] proxyUsings = usingStatements;

        if (elementType.ContainingNamespace is { IsGlobalNamespace: false } elementNamespace)
            proxyUsings = [.. usingStatements, $"using {elementNamespace.ToDisplayString(s_namespaceUsingFormat)};"];

        string proxySource = new ConstraintProxyImplTemplate
        {
            PackageNamespace = packageNamespace,
            PackageName = packageName,
            ProxyName = proxyName,
            InterfaceRef = interfaceRef,
            ElementName = elementName,
            // The interface adapter's own rule (public when both sides are public): a proxy is a TYPE ARGUMENT of whatever
            // signature closes the constraint, and Go 1.24's crypto/internal/fips140 ecdh/ecdsa export `P224() *Curve[*P224Point]`,
            // so an always-internal proxy made every such public method CS0050 (RED 8). crypto/elliptic's unexported curves keep
            // their proxies internal under the same rule.
            AdapterScope = AdapterSidePublic(interfaceDef, interfaceDef.Name) && AdapterSidePublic(elementType, elementType.Name) ? "public" : "internal",
            MethodsImplementation = methods.ToString(),
            UsingStatements = proxyUsings
        }
        .Generate();

        context.AddSource(GetUniqueHintName(emittedHintNames, GetValidFileName($"{packageNamespace}.{packageClassName}.{proxyName}-proxy.g.cs")), proxySource);
    }

    /// <summary>
    /// Gets a struct's DEPTH-1 embedded INTERFACE fields, each paired with every member name its
    /// interface declares — including everything that interface itself embeds.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Detection is SEMANTIC by name (the field's name equals its interface type's simple name),
    /// because the converter names an embed field after the Go embed and a Δ-renamed interface TYPE
    /// keeps a markerless FIELD (slogtest's <c>wrapper</c> embeds <c>slog.ΔHandler</c> as
    /// <c>Handler</c>). It is a heuristic — an ordinary named field whose name happens to equal its
    /// type's simple name matches too — which is why every caller resolves a hard marker FIRST and
    /// consults this only for what remains. Enumeration order follows <c>GetMembers</c>, i.e.
    /// declaration order, so the emission it feeds is deterministic.
    /// </para>
    /// <para>
    /// A FUNCTION-LOCAL interface lift defeats the bare name test: the converter renames the TYPE to
    /// <c>&lt;Func&gt;_&lt;name&gt;</c> while the field keeps <c>&lt;name&gt;</c>, so reflect's
    /// <c>func TestCallPanic() { type T1 interface{…}; type T2 struct { T1 } }</c> emits field
    /// <c>T1</c> of type <c>TestCallPanic_T1</c>. The third arm reads the lift's
    /// <c>[GoLocalName]</c> stamp — the ORIGINAL Go name the converter records for exactly this kind
    /// of question — so the embed is seen again. Without it the member bound nowhere and promotion
    /// fell back to naming the TYPE as the accessor (<c>recvᴛ.TestCallPanic_T1.Y()</c>), a member
    /// that does not exist: CS1061/CS0120.
    /// </para>
    /// <para>
    /// Reading the stamp rather than matching by TYPE is deliberate, and keeps the discrimination the
    /// bare-name test exists for: <c>type PtrType struct { CommonType; Type Type }</c> has an
    /// ordinary <c>Type</c> field that a type match would call an embed. Only a function-local lift
    /// carries the stamp, so an ordinary field cannot acquire one.
    /// </para>
    /// </remarks>
    private static List<(string FieldName, HashSet<string> Members)> GetEmbeddedInterfaceFieldMembers(ITypeSymbol structType) =>
        structType.GetMembers()
            .OfType<IFieldSymbol>()
            .Where(field => !field.IsStatic && field.Type.TypeKind == TypeKind.Interface &&
                            field.Type is INamedTypeSymbol &&
                            (field.Name == field.Type.Name || ShadowVarMarker + field.Name == field.Type.Name ||
                             GetGoLocalName(field.Type) == field.Name))
            .Select(field => (field.Name, new HashSet<string>(((INamedTypeSymbol)field.Type).AllInterfaces
                .Concat([(INamedTypeSymbol)field.Type])
                .SelectMany(iface => iface.GetMembers().OfType<IMethodSymbol>())
                .Select(method => method.Name), StringComparer.Ordinal)))
            .ToList();

    /// <summary>
    /// Resolves the name of the struct FIELD embedding <paramref name="interfaceType"/> when it
    /// differs from the interface's own simple name, or <c>null</c> when the usual derivation holds.
    /// </summary>
    /// <remarks>
    /// Only a FUNCTION-LOCAL lift diverges — the converter renames the TYPE to
    /// <c>&lt;Func&gt;_&lt;name&gt;</c> and stamps the original Go name on it, while the embed field
    /// keeps that original name. The stamp is required to match a field that actually exists AND
    /// whose type is this interface, so an ordinary field can never be mistaken for an embed:
    /// <c>type PtrType struct { CommonType; Type Type }</c> has no stamped lift and resolves to
    /// <c>null</c>, leaving today's behaviour exactly as it was.
    /// </remarks>
    private static string? ResolveEmbedFieldName(ITypeSymbol structType, ITypeSymbol interfaceType)
    {
        string? localName = GetGoLocalName(interfaceType);

        if (string.IsNullOrEmpty(localName))
            return null;

        bool declared = structType.GetMembers()
            .OfType<IFieldSymbol>()
            .Any(field => !field.IsStatic && field.Name == localName &&
                          SymbolEqualityComparer.Default.Equals(field.Type, interfaceType));

        return declared ? localName : null;
    }

    /// <summary>
    /// The fully qualified package class (<c>global::go.….X_package</c>) declaring the type of the
    /// struct's VALUE embed <paramref name="embedName"/>, or <c>null</c> when the embed member does
    /// not resolve to a type nested in a class.
    /// </summary>
    private static string? ResolveValueEmbedPackageClass(ITypeSymbol structType, string embedName) =>
        structType.GetMembers(embedName)
            .Select(member => member switch { IPropertySymbol property => property.Type, IFieldSymbol field => field.Type, _ => null })
            .OfType<INamedTypeSymbol>()
            .FirstOrDefault()?.ContainingType is INamedTypeSymbol packageClass
            ? packageClass.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
            : null;

    /// <summary>
    /// Reads a lifted type's ORIGINAL Go name from its <c>[GoLocalName]</c> stamp, or <c>null</c>
    /// when it carries none.
    /// </summary>
    /// <remarks>
    /// The converter stamps a function-local NAMED lift — struct (visitStructType) and interface
    /// (visitInterfaceType) alike — because the hoisted <c>&lt;Func&gt;_&lt;name&gt;</c> identifier
    /// no longer carries the Go name anything downstream needs to ask about. An ANONYMOUS lift has
    /// no Go name and is deliberately unstamped.
    /// </remarks>
    private static string? GetGoLocalName(ITypeSymbol type) =>
        type.GetAttributes()
            .FirstOrDefault(attribute => attribute.AttributeClass?.Name is "GoLocalNameAttribute" or "GoLocalName")
            ?.ConstructorArguments.FirstOrDefault().Value as string;

    private static string SafeParameterName(string name, int index) => string.IsNullOrEmpty(name) ? $"arg{index}" : EscapeCsKeyword(name);

    // Renders ONE forwarded argument in a constraint-proxy forwarder body.
    //
    // A parameter whose type mentions the interface's self parameter is DECLARED on the proxy
    // closed over the PROXY (`Action<TжTBRun>`), while the box extension the body forwards to is
    // declared closed over the BOX (`Action<ж<testing.T>>`). At the TOP level the two implicit
    // conversions the proxy emits marshal that boundary for free — a bare `T` argument converts
    // proxy→box, a `T` result converts box→proxy. NESTED inside a delegate they cannot: C#
    // delegate variance requires a REFERENCE conversion and does not lift a user-defined one
    // through `Action<>`/`Func<>`. The forwarder then binds nothing (CS1929, reported against the
    // ref-receiver overload because no ж-twin is applicable). net/http's own constraint is the
    // shape — `type TBRun[T any] interface { testing.TB; Run(string, func(T)) bool }`.
    //
    // Re-wrap in a lambda whose parameters are typed with the self parameter substituted by the
    // BOX, so every crossing takes an implicit conversion in whichever direction that position
    // needs: box→proxy for an argument going IN, proxy→box for a self-typed RESULT coming OUT
    // (the lambda's return type is inferred from the target delegate, where the conversion
    // applies). The lambda parameters are named positionally with the temp marker, which no Go
    // identifier can spell, so they cannot shadow the forwarder's own parameters.
    //
    // Anything that is NOT a delegate is returned untouched — the top-level implicit conversions
    // already cover it — and so is a delegate with a by-ref parameter, which Go cannot express:
    // a guess there would trade a clear diagnostic for a wrong marshalling.
    private static string RenderForwardedArgument(ITypeSymbol parameterType, string parameterName, ITypeParameterSymbol selfParameter, string elementName)
    {
        if (SymbolEqualityComparer.Default.Equals(parameterType, selfParameter) ||
            !ContainsTypeParameter(parameterType, selfParameter) ||
            parameterType is not INamedTypeSymbol { TypeKind: TypeKind.Delegate } delegateType ||
            delegateType.DelegateInvokeMethod is not { } invoke ||
            invoke.Parameters.Any(parameter => parameter.RefKind != RefKind.None))
        {
            return parameterName;
        }

        string boxRef = $"{PointerPrefix}<{elementName}>";

        string lambdaParameters = string.Join(", ", invoke.Parameters.Select((parameter, index) =>
            $"{RenderWithProxy(parameter.Type, selfParameter, boxRef)} {TempVarMarker}{index}"));

        string lambdaArguments = string.Join(", ", invoke.Parameters.Select((_, index) => $"{TempVarMarker}{index}"));

        return $"({lambdaParameters}) => {parameterName}({lambdaArguments})";
    }

    // Reports whether a type mentions the interface's self-type parameter anywhere in its shape.
    private static bool ContainsTypeParameter(ITypeSymbol type, ITypeParameterSymbol typeParameter) => type switch
    {
        _ when SymbolEqualityComparer.Default.Equals(type, typeParameter) => true,
        IArrayTypeSymbol array => ContainsTypeParameter(array.ElementType, typeParameter),
        INamedTypeSymbol named => named.TypeArguments.Any(argument => ContainsTypeParameter(argument, typeParameter)),
        _ => false
    };

    // Renders a type for a proxy method signature with the interface's self-type parameter T
    // rewritten to the proxy itself — `T`→proxy, `(T, error)`→`(proxy, error)`, `[]T`→`proxy[]`,
    // `Foo<T>`→`Foo<proxy>`. A type with no T renders exactly as the interface declares it.
    private static string RenderWithProxy(ITypeSymbol type, ITypeParameterSymbol selfParameter, string proxyName)
    {
        if (SymbolEqualityComparer.Default.Equals(type, selfParameter))
            return proxyName;

        if (!ContainsTypeParameter(type, selfParameter))
            return GlobalQualify(type.ToDisplayString());

        switch (type)
        {
            case IArrayTypeSymbol array:
                return $"{RenderWithProxy(array.ElementType, selfParameter, proxyName)}[]";
            case INamedTypeSymbol { IsTupleType: true } tuple:
                return $"({string.Join(", ", tuple.TupleElements.Select(element => RenderWithProxy(element.Type, selfParameter, proxyName)))})";
            case INamedTypeSymbol named:
                return $"{StripGenericTypeArguments(GlobalQualify(named.ConstructedFrom.ToDisplayString()))}<{string.Join(", ", named.TypeArguments.Select(argument => RenderWithProxy(argument, selfParameter, proxyName)))}>";
            default:
                return GlobalQualify(type.ToDisplayString());
        }
    }

    // Shorthand for the shared rule (Common.EffectiveScopeIsPublic) at this generator's adapter and
    // operator scope decisions, where the name is already in hand.
    private static bool AdapterSidePublic(ITypeSymbol type, string name) => EffectiveScopeIsPublic(type, GetSimpleName(name));

    private static string? GetNamespace(FileScopedNamespaceDeclarationSyntax? namespaceSyntax)
    {
        return namespaceSyntax?.Name.ToString();
    }

    private static string? GetFirstClassName(CompilationUnitSyntax compilationUnit)
    {
        return compilationUnit.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault()?.Identifier.Text;
    }
}
