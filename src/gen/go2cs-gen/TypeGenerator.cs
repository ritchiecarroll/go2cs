// TypeGenerator.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

//#define DEBUG_GENERATOR

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using go2cs.Templates.InheritedType;
using go2cs.Templates.InterfaceType;
using go2cs.Templates.StructType;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static go2cs.Common;
using static go2cs.Symbols;

#if DEBUG_GENERATOR
using System.Diagnostics;
#endif

namespace go2cs;

[Generator]
public class TypeGenerator : ISourceGenerator
{
    private const string Namespace = "go";
    private const string AttributeName = "GoType";
    private const string FullAttributeName = $"{Namespace}.{AttributeName}Attribute";
    private const string ValueCloneAttributeName = "GoValueClone";
    private const string ArrayDimsAttributeName = "GoArrayDims";

    public void Initialize(GeneratorInitializationContext context)
    {
    #if DEBUG_GENERATOR
        if (!Debugger.IsAttached)
            Debugger.Launch();
    #endif

        // Register to find "GoTypeAttribute" on type declarations
        context.RegisterForSyntaxNotifications(() => new AttributeFinder<BaseTypeDeclarationSyntax>(FullAttributeName));
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxContextReceiver is not AttributeFinder<BaseTypeDeclarationSyntax> { HasAttributes: true } attributeFinder)
            return;

        HashSet<string> emittedHintNames = new(StringComparer.OrdinalIgnoreCase);

        // Shared by every named fixed-array wrapper this Execute emits (see the Array arm's element
        // factory). Its lifetime is ONE compilation, which is what makes a bare type NAME a safe key:
        // `Pointer` names a different type in half a dozen assemblies, so a longer-lived cache would
        // answer one assembly's question with another's answer.
        Dictionary<string, bool> needsConstructionCache = new(StringComparer.Ordinal);

        foreach ((BaseTypeDeclarationSyntax targetSyntax, List<AttributeSyntax> attributes) in attributeFinder.TargetAttributes)
        {
            SyntaxTree syntaxTree = targetSyntax.SyntaxTree;
            SemanticModel semanticModel = context.Compilation.GetSemanticModel(syntaxTree);

            string packageNamespace = targetSyntax.GetNamespaceName();
            string packageClassName = targetSyntax.GetParentClassName();
            string packageName = packageClassName.EndsWith(PackageSuffix) ? packageClassName[..^PackageSuffix.Length] : packageClassName;
            string identifier = targetSyntax.Identifier.Text;
            bool hasEqualityOperators = true;

            // Add generic type parameters to the identifier
            if (targetSyntax is TypeDeclarationSyntax { TypeParameterList.Parameters.Count: > 0 } typeDecl)
            {
                IEnumerable<string> typeParamNames = typeDecl.TypeParameterList.Parameters.Select(p => p.Identifier.Text);
                identifier += $"<{string.Join(", ", typeParamNames)}>";
                hasEqualityOperators = typeDecl.AllGenericTypesHaveConstraint(semanticModel, "System.Numerics.IEqualityOperators`3");
            }

            string fullyQualifiedIdentifier = semanticModel.GetDeclaredSymbol(targetSyntax)?.ToDisplayString() ?? $"{packageNamespace}.{packageClassName}.{identifier}";
            
            // Since many types are referenced by assembly attributes outside namespace,
            // "internal" scope is used so types can be referenced instead of "private".
            // An explicit modifier on the converter's partial declaration wins (e.g. an
            // unexported type publicized because it is an exported field's type — CS0051/CS0052).
            string scope = GetExplicitAccessModifier(targetSyntax) ?? GetScope(identifier);

            string[] usingStatements = GetFullyQualifiedUsingStatements(syntaxTree, semanticModel);

            // Fields the converter marked as needing a DEEP copy on a Go by-value struct copy
            // (see GoValueCloneAttribute / StructTypeTemplate.ValueCloneImplementation).
            string[] valueCloneFields = GetValueCloneFields(targetSyntax, semanticModel);

            foreach (AttributeSyntax attribute in attributes)
            {
                // Get the attribute's argument values
                (string _, string value)[] arguments = attribute.GetArgumentValues();

                // Get the attribute's first constructor argument value, the type definition
                string typeDefinition = string.Empty;

                if (arguments.Length > 0)
                {
                    string value = arguments[0].value;
                    
                    if (!string.IsNullOrWhiteSpace(value) && value.Length > 2)
                        typeDefinition = value[1..^1].Trim();
                }

                string generatedSource, typeName;

                switch (targetSyntax)
                {
                    case StructDeclarationSyntax structDeclaration when string.IsNullOrWhiteSpace(typeDefinition) || typeDefinition.Equals("dyn"):
                        generatedSource = new StructTypeTemplate
                        {
                            PackageNamespace = packageNamespace,
                            PackageName = packageName,
                            Scope = scope,
                            Context = context,
                            StructName = identifier,
                            FullyQualifiedStructType = fullyQualifiedIdentifier,
                            StructMembers = structDeclaration.GetStructMembers(context.Compilation, true),
                            ChanDirInitializerMembers = structDeclaration.GetChanDirInitializerMembers(),
                            HasEqualityOperators = hasEqualityOperators,
                            // A generic struct that failed the whole-struct constraint gate still
                            // gets a real memberwise Equals: each member whose type supports ==
                            // independent of the unconstrained type parameters compares with ==,
                            // and only the rest fall back to golib's AreEqual (never a blanket
                            // `false`, which broke ==-independent structs like unique.Handle<T>).
                            // An INTERFACE-typed member joins that set for every struct, generic or
                            // not, and for the opposite reason: its == compiles and is REFERENCE
                            // identity, where Go compares interface values by dynamic type and
                            // value. See GetInterfaceValueMembers.
                            EqualityFallbackMembers = CombineEqualityFallbacks(
                                hasEqualityOperators ? null : structDeclaration.GetEqualityFallbackMembers(context.Compilation),
                                structDeclaration.GetInterfaceValueMembers(context.Compilation)),
                            ValueCloneFields = valueCloneFields,
                            UsingStatements = usingStatements
                        }
                        .Generate();

                        break;
                    
                    case StructDeclarationSyntax when typeDefinition.StartsWith("[]"): // slice
                        typeName = QualifySourceAliasReferences(typeDefinition[2..].Trim(), syntaxTree, semanticModel);

                        // m_value stays MUTABLE for a named-slice wrapper: a Go pointer-reinterpret to
                        // the underlying slice — `(*[][]byte)(buf)` with `buf *Buffers`, net
                        // fd_windows.go — projects a ж<slice<T>> VIEW over the wrapper's own field
                        // (`Ꮡbuf.of(Buffers.Ꮡm_value)`), so header writes through the view (poll
                        // FD.Writev's consume reslicing) land on the original (a readonly field would
                        // force a defensive copy and lose them).
                        generatedSource = new InheritedTypeTemplate
                        {
                            PackageNamespace = packageNamespace,
                            PackageName = packageName,
                            ObjectName = identifier,
                            Scope = scope,
                            TypeName = $"slice<{typeName}>",
                            TargetTypeName = typeName,
                            TypeClass = "Slice",
                            ReadOnlyValue = false
                        }
                        .Generate();

                        break;

                    case StructDeclarationSyntax when typeDefinition.StartsWith("map["):
                        (string keyTypeName, string valueTypeName) = SplitMapTypes(typeDefinition);
                        keyTypeName = QualifySourceAliasReferences(keyTypeName, syntaxTree, semanticModel);
                        valueTypeName = QualifySourceAliasReferences(valueTypeName, syntaxTree, semanticModel);

                        generatedSource = new InheritedTypeTemplate
                        {
                            PackageNamespace = packageNamespace,
                            PackageName = packageName,
                            Scope = scope,
                            ObjectName = identifier,
                            TypeName = $"map<{keyTypeName}, {valueTypeName}>",
                            TargetTypeName = keyTypeName,
                            TargetValueTypeName = valueTypeName,
                            TypeClass = "Map",
                            UsingStatements = usingStatements

                        }
                        .Generate();

                        break;

                    case StructDeclarationSyntax when typeDefinition.StartsWith("chan "):
                        typeName = QualifySourceAliasReferences(typeDefinition[5..].Trim(), syntaxTree, semanticModel);

                        generatedSource = new InheritedTypeTemplate
                        {
                            PackageNamespace = packageNamespace,
                            PackageName = packageName,
                            ObjectName = identifier,
                            Scope = scope,
                            TypeName = $"channel<{typeName}>",
                            TargetTypeName = typeName,
                            TypeClass = "Channel",
                            UsingStatements = usingStatements
                        }
                        .Generate();

                        break;
                    
                    case StructDeclarationSyntax when typeDefinition.StartsWith("["): // array
                        int sizeStart = typeDefinition.IndexOf('[') + 1;
                        int sizeEnd = typeDefinition.IndexOf(']');
                        string arraySize = typeDefinition[sizeStart..sizeEnd].Trim();
                        typeName = QualifySourceAliasReferences(typeDefinition[(sizeEnd + 1)..].Trim(), syntaxTree, semanticModel);

                        generatedSource = new InheritedTypeTemplate
                        {
                            PackageNamespace = packageNamespace,
                            PackageName = packageName,
                            ObjectName = identifier,
                            ReadOnlyValue = false,
                            Scope = scope,
                            TypeName = $"array<{typeName}>",
                            TargetTypeName = typeName,
                            TargetTypeSize = arraySize,
                            TypeClass = "Array",
                            ElementZeroFactory = ArrayElementZeroFactory(context, targetSyntax, semanticModel, typeName, needsConstructionCache),
                            UsingStatements = usingStatements
                        }
                        .Generate();

                        break;

                    case StructDeclarationSyntax when typeDefinition.StartsWith("num:"): // numeric
                        typeName = typeDefinition[4..].Trim();

                        generatedSource = new InheritedTypeTemplate
                        {
                            PackageNamespace = packageNamespace,
                            PackageName = packageName,
                            ObjectName = identifier,
                            Scope = $"{scope} readonly",
                            TypeName = typeName,
                            TargetTypeName = identifier,
                            TypeClass = "Numeric",
                            UsingStatements = usingStatements
                        }
                        .Generate();

                        break;

                    case StructDeclarationSyntax when !string.IsNullOrWhiteSpace(typeDefinition):
                        typeName = typeDefinition;

                        // A defined type whose underlying is a STRUCT (`type winlibcall libcall`) exposes
                        // the underlying struct's fields in Go (`w.fn`). Resolve the underlying struct
                        // (same-package or a source-referenced package) and forward its members as get/set
                        // properties over a MUTABLE m_value, so `box.Value.fn = x` (a write through a
                        // ж<T>.Value ref) persists. Non-struct underlyings (a named type over an interface or
                        // another named type) resolve to null and keep the plain wrapper (no churn).
                        List<(string typeName, string memberName, bool isReferenceType, bool isProperty, bool isPublic)>? forwardedMembers = null;
                        string? underlyingArrayElem = null;
                        bool mutableValue = false;

                        (StructDeclarationSyntax? underlyingStruct, Compilation? underlyingCompilation) = context.GetStructDeclaration(typeDefinition);

                        // Captured for the W3a wrapper-scaffolding fix below — the SAME symbol
                        // GetForeignStructMembers already resolves in the `else if` arm, needed again
                        // once execution is past that arm's own scope. Null under the ordinary
                        // same-compilation path (underlyingStruct is not null then instead).
                        INamedTypeSymbol? foreignUnderlyingSymbol = null;

                        if (underlyingStruct is not null && underlyingCompilation is not null)
                        {
                            List<(string typeName, string memberName, bool isReferenceType, bool isProperty, bool isPublic)> members = underlyingStruct.GetStructMembers(underlyingCompilation, false);

                            // Only forward + go mutable when the underlying actually contributes fields.
                            // An empty result (e.g. a named type over an array-typed named struct whose
                            // members are generated, not declared) keeps the plain readonly wrapper — no ripple.
                            if (members.Count > 0)
                            {
                                forwardedMembers = members;
                                mutableValue = true;
                            }
                            else
                            {
                                // A defined type over an ARRAY-backed [GoType] wrapper — `type pallocBits
                                // pageBits` where `type pageBits [8]uint64` — is len()'d / indexed directly
                                // in Go, which needs IArray on THIS wrapper (golib `len(IArray)`, CS1503
                                // otherwise; runtime mpallocbits.go). The chain can run more than one hop
                                // deep (pallocBits's OWN [GoType] spells "pageBits", a plain name, not the
                                // array literal — only pageBits's carries that), so the walk is recursive,
                                // not a single read. Detect it from wherever the chain bottoms out at a
                                // `[N]elem` definition (never a `[]` slice) and implement IArray<elem> as
                                // a view over m_value (IArrayViewTypeTemplate).
                                underlyingArrayElem = ResolveArrayElementType(context, typeDefinition, []);

                                if (underlyingArrayElem is not null)
                                {
                                    // The view's ref accessor must ensure the underlying's lazily-
                                    // allocated backing lands on THIS wrapper's own m_value (a
                                    // readonly field would force a defensive copy and lose writes).
                                    mutableValue = true;
                                }
                            }
                        }
                        else if (context.FindUnderlyingStructSymbol(typeDefinition) is { } underlyingSymbol)
                        {
                            // The underlying struct's SOURCE is not in this compilation — the normal
                            // shape in a real MSBuild build, where a <ProjectReference> arrives as
                            // compiled METADATA and the referenced-compilations walk above finds
                            // nothing. Resolve it by SYMBOL instead: a defined type over a FOREIGN
                            // struct exposes that struct's fields in Go exactly as a same-package one
                            // does (`type index Index` in a white-box _test.go reading `x.sa`;
                            // `type P otherpkg.Point` reading `p.X`), so it needs the same forwarding.
                            List<(string typeName, string memberName, bool isReferenceType, bool isProperty, bool isPublic)> members =
                                StructDeclarationSyntaxExtensions.GetForeignStructMembers(underlyingSymbol, context.Compilation);

                            if (members.Count > 0)
                            {
                                forwardedMembers = members;
                                mutableValue = true;
                            }
                            else
                            {
                                // The METADATA twin of the array-backed-wrapper walk above (same chain,
                                // same runtime example — a `-tests` wrapper's underlying-of-underlying can
                                // cross the assembly seam at either hop, so the recursive walk tries both
                                // source and metadata resolution at every step regardless of which one
                                // resolved THIS hop).
                                underlyingArrayElem = ResolveArrayElementType(context, typeDefinition, []);

                                if (underlyingArrayElem is not null)
                                    mutableValue = true;
                            }

                            foreignUnderlyingSymbol = underlyingSymbol;
                        }

                        // W3a wrapper-scaffolding fix (docs/phase4/DESIGN-w3a-wrapper-scaffolding.md,
                        // ruled 2026-08-30). The wrapper's OWN constructor/.Value/conversion operators
                        // reference the WRAPPED type directly (TypeName), independent of whether the
                        // WRAPPER itself (ObjectName) is public — a public MSpan wrapping an unexported
                        // mspan still needs an internal constructor/`.Value`, exactly the
                        // ForwardedMembers question one level up (see InheritedTypeTemplate.MemberScope).
                        //
                        // Scoped to a TEST-FILE-declared wrapper specifically (checked via the emitted
                        // C# file's own path — every `-tests` emission preserves the "_test" suffix,
                        // `export_test.go` -> `export_test.cs`, with no new stamping mechanism needed).
                        // An UNSCOPED first attempt at this fix downgraded ORDINARY PRODUCTION wrapper
                        // operators too (measured: unsafe_package.ArbitraryType, internal/goarch's
                        // ArchFamilyType both newly failed CS0558) — production conversion is one
                        // coordinated pass where a wrapper and its underlying are emitted together, so
                        // nothing there needs this question asked; only the `-tests` bridge case, where
                        // the wrapper's underlying can come from a separately-converted assembly with
                        // its own accessibility, does.
                        bool isTestFileDeclaration = syntaxTree.FilePath.EndsWith("_test.cs", StringComparison.OrdinalIgnoreCase);

                        // Two sources for the underlying's own symbol, matching the two resolution
                        // arms above: same-compilation source (measured: NOT what fires for runtime's
                        // whitebox-reference wrappers — production is a referenced assembly there, so
                        // underlyingStruct/underlyingCompilation are null) or the foreign/metadata
                        // symbol GetForeignStructMembers already resolved. Either answers the same
                        // question; only one is ever non-null for a given wrapper.
                        ITypeSymbol? underlyingStructSymbol = !isTestFileDeclaration ? null :
                            underlyingStruct is not null && underlyingCompilation is not null
                                ? underlyingCompilation.GetSemanticModel(underlyingStruct.SyntaxTree).GetDeclaredSymbol(underlyingStruct) as ITypeSymbol
                                : foreignUnderlyingSymbol;
                        bool wrappedTypeIsPublic = underlyingStructSymbol is null || StructDeclarationSyntaxExtensions.IsMemberTypePublic(underlyingStructSymbol);

                        generatedSource = new InheritedTypeTemplate
                        {
                            PackageNamespace = packageNamespace,
                            PackageName = packageName,
                            ObjectName = identifier,
                            Scope = scope,
                            ReadOnlyValue = !mutableValue,
                            TypeName = typeName,
                            TargetTypeName = typeName,
                            TypeClass = typeDefinition,
                            ForwardedStructMembers = forwardedMembers,
                            UnderlyingArrayElementType = underlyingArrayElem,
                            ValueClone = valueCloneFields.Length > 0,
                            WrappedTypeIsPublic = wrappedTypeIsPublic,
                            UsingStatements = usingStatements
                        }
                        .Generate();

                        break;

                    case StructDeclarationSyntax:
                        throw new NotSupportedException($"Unsupported [{AttributeName}] definition \"{typeDefinition}\" on struct \"{identifier}\".");

                    case InterfaceDeclarationSyntax interfaceDeclaration:
                        string[]? operatorConstraints = null;

                        if (!string.IsNullOrWhiteSpace(typeDefinition))
                        {
                            string[] keys = typeDefinition.Split([';'], StringSplitOptions.RemoveEmptyEntries);

                            foreach (string key in keys)
                            {
                                string[] parts = key.Split(["="], StringSplitOptions.RemoveEmptyEntries);

                                if (parts.Length > 1 && parts[0].Trim().Equals("operators", StringComparison.OrdinalIgnoreCase))
                                    operatorConstraints = parts[1].Split([','], StringSplitOptions.RemoveEmptyEntries).Select(part => part.Trim()).ToArray();
                            }
                        }

                        usingStatements = usingStatements.Append("using System.Numerics;").ToArray();

                        // A CONSTRAINT interface (operators=…) exists to carry C# operator constraints
                        // and a GENERIC one has no single runtime instantiation to bind against, so
                        // neither has a Go method set a runtime shell could satisfy — and an interface
                        // with no methods is satisfied by every value nominally already.
                        //
                        // The "dyn" key is NOT read here: an anonymous interface takes exactly the same
                        // shells a named one does. The key once selected a second renderer — the ᴛAs
                        // conversion methods and their Δ wrapper — which is retired now that dyn rides
                        // on the shells, so the ONLY remaining reader of "dyn" is the runtime's
                        // Type.IsDynamicType (struct-to-struct dynamic conversion), and it reads the
                        // [GoType] attribute directly rather than going through here.
                        bool shellEligible = operatorConstraints is null &&
                            interfaceDeclaration.TypeParameterList is null or { Parameters.Count: 0 };

                        MethodInfo[] interfaceMethods = shellEligible ?
                            interfaceDeclaration.GetInterfaceMethods(context) :
                            [];

                        generatedSource = new InterfaceTypeTemplate
                        {
                            PackageNamespace = packageNamespace,
                            PackageName = packageName,
                            Scope = scope,
                            InterfaceName = identifier,
                            OperatorConstraints = operatorConstraints ?? [],
                            Methods = interfaceMethods,
                            // A member declared with a ref-kind modifier cannot be re-declared
                            // faithfully from the recorded parameter types, so an interface carrying
                            // one gets no shell rather than one that fails to implement it (CS0535).
                            EmitShells = shellEligible && interfaceMethods.Length > 0 &&
                                interfaceMethods.All(method => method.IsSignatureRenderable),
                            UsingStatements = usingStatements
                        }
                        .Generate();

                        break;

                    case ClassDeclarationSyntax when typeDefinition.StartsWith($"{PointerPrefix}<"): // pointer
                        typeName = typeDefinition[2..^1];
                        
                        generatedSource = new InheritedTypeTemplate
                        {
                            PackageNamespace = packageNamespace,
                            PackageName = packageName,
                            ObjectName = identifier,
                            ObjectKind = "class",
                            Scope = scope,
                            TypeName = $"{PointerPrefix}<{typeName}>",
                            TargetTypeName = typeName,
                            TypeClass = "Pointer",
                            UsingStatements = usingStatements
                        }
                        .Generate();

                        break;

                    default:
                        throw new NotSupportedException($"Unsupported [{AttributeName}] on {targetSyntax.GetType().Name} type \"{identifier}\".");
                }

                // Add the source code to the compilation
                context.AddSource(GetUniqueHintName(emittedHintNames, GetValidFileName($"{packageNamespace}.{packageClassName}.{identifier}.g.cs")), generatedSource);
            }
        }
    }

    // Reads the field names out of the converter's [GoValueClone("f1", "f2")] stamp — the fields a
    // Go by-value copy of this struct must DEEP-copy (see GoValueCloneAttribute). Matched by syntax
    // name, like every other attribute this generator reads; the converter emits it unqualified.
    //
    // Scanned over EVERY partial declaration of the type, not just the [GoType] one this generator
    // was handed: the converter writes the stamp on the package_info.cs accessibility record so the
    // mainline declaration reads like the Go original, and a hand-owned conversion — which gets no
    // such record — keeps it inline. C# unions a partial type's attributes, so both are the same
    // stamp on the same type.
    // The construction expression for ONE element of a named fixed-size array's lazy backing, or null
    // when `default(element)` is already the element's Go zero value (nearly always — 57 of the 59
    // named array wrappers in the corpus, measured).
    //
    // Two element shapes are not, and each is answered by whichever half of the toolchain HAS the
    // fact — which is the whole reason this is split rather than spelled once:
    //
    //   - a nested UNNAMED array (`type nn [2][3]int`). The inner length reaches C# nowhere: the
    //     GoType descriptor is `[2]array<nint>`, and `array<T>` carries its length in the INSTANCE,
    //     never in the type, so neither this generator nor golib can recover the 3. Only the
    //     converter knows it, and it hands it over as `[GoArrayDims(2, 3)]` — the SAME cargo (and
    //     the same outermost-first meaning) it already stamps on a parameter or field whose array
    //     length would otherwise be lost to the reflection bridge. Dimension 0 is the wrapper's own
    //     length; the element factory is built from the rest.
    //
    //   - a STRUCT whose zero value needs construction (`type semTable [251]struct{ root semaRoot;
    //     pad [40]byte }`), where `default` skips the generated constructor that runs the `pad =
    //     new(40)` initializer. Here the CONVERTER needs no cargo at all, because this generator
    //     already owns the predicate — and owns the half the converter could not supply anyway: a
    //     cross-ASSEMBLY element resolves by metadata symbol rather than syntax, which is what
    //     `bcache.cacheTable`'s `atomic.Pointer<…>` element (declared in sync/atomic) requires.
    //     `new E(nil)` is the same NilType construction a needy struct FIELD already takes, and
    //     StructTypeNeedsConstruction only answers true for a type that actually exposes that
    //     constructor.
    //
    // A NAMED array element needs nothing: its own wrapper allocates its own backing lazily, by
    // exactly the property this expression feeds. That is measured, not assumed — `type no [2]ni`
    // over `type ni [3]int` prints correctly at master while `[2][3]int` does not.
    private static string? ArrayElementZeroFactory(GeneratorExecutionContext context, BaseTypeDeclarationSyntax targetSyntax, SemanticModel semanticModel, string elementTypeName, Dictionary<string, bool> needsConstructionCache)
    {
        long[] dims = GetGoArrayDims(targetSyntax, semanticModel);

        // dims[0] is this array's own length; anything beyond it describes the ELEMENT.
        if (dims.Length > 1)
            return RenderArrayDimsFactory(dims, 1);

        return ElementNeedsConstruction(context, elementTypeName, needsConstructionCache) ?
            $"new {elementTypeName}(nil)" :
            null;
    }

    // The predicate, asked with the element name in BOTH the spellings it can arrive in.
    //
    // A struct FIELD reaches StructTypeNeedsConstruction already fully rooted, because
    // GetStructMembers produces `global::go.…` names. A `[GoType("[N]E")]` descriptor's element does
    // not: it is package-alias-qualified (`sync.atomic_package.Pointer<cacheEntry<K, V>>`), which is
    // not a CLR name at all — every converted package class lives under the `go` namespace. Asking
    // with that spelling alone made every CROSS-ASSEMBLY element answer false, and answer it
    // SILENTLY: the wrapper simply kept its bare-length backing, which is the exact state this change
    // exists to remove. Measured on `bcache.cacheTable` — the generated backing came back
    // `new array<sync.atomic_package.Pointer<cacheEntry<K, V>>>(1021)` with no factory — and the axis
    // was then isolated on a two-arm probe as CROSS-ASSEMBLY rather than generic, because a
    // cross-package NON-generic needy element declined identically. The corpus's own control says the
    // symbol path is not at fault: `math/rand/v2`'s ChaCha8 constructs its cross-package
    // `chacha8rand.State` field today, and it does so from a rooted name.
    //
    // FindUnderlyingStructSymbol already documents and performs this normalization for the same
    // reason one hop over, so the retry goes through it rather than re-deriving the rooting rule.
    // The EMISSION keeps the descriptor's own spelling either way: that is the name `array<E>` is
    // already written with in the generated file, so it is the name that resolves there.
    private static bool ElementNeedsConstruction(GeneratorExecutionContext context, string elementTypeName, Dictionary<string, bool> cache)
    {
        if (StructTypeTemplate.NeedsConstruction(context, elementTypeName, cache))
            return true;

        string? rooted = context.FindUnderlyingStructSymbol(elementTypeName)
            ?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        return rooted is not null &&
               !string.Equals(rooted, elementTypeName, StringComparison.Ordinal) &&
               StructTypeTemplate.NeedsConstruction(context, rooted, cache);
    }

    // `new(3)` / `new(3, static () => new(4))` — target-typed against golib's
    // `array<T>(int length[, Func<T> elementFactory])`, the same argument list the converter's
    // arrayLengthArgs renders for the sites IT can spell.
    //
    // A dimension that does not fit an `int` yields NO factory rather than a wrong one. Go's
    // pointer-to-unbounded-array idiom (`*[1<<50 - 1]byte`, runtime/vdso_linux) puts such a length in
    // the type system deliberately, and no such array is allocatable on either runtime — so declining
    // leaves today's behaviour exactly where it is instead of emitting a literal that would not bind.
    private static string? RenderArrayDimsFactory(long[] dims, int index)
    {
        if (index >= dims.Length)
            return null;

        long dimension = dims[index];

        if (dimension < 0 || dimension > int.MaxValue)
            return null;

        string? inner = RenderArrayDimsFactory(dims, index + 1);

        return inner is null ?
            $"new({dimension})" :
            $"new({dimension}, static () => {inner})";
    }

    // The `[GoArrayDims(...)]` cargo on a type declaration, outermost first, or empty when absent.
    // Mirrors GetValueCloneFields' shape: the stamp can sit on any of the type's partial
    // declarations, and the converter writes it on the one carrying [GoType].
    private static long[] GetGoArrayDims(BaseTypeDeclarationSyntax targetSyntax, SemanticModel semanticModel)
    {
        foreach (BaseTypeDeclarationSyntax declaration in GetPartialDeclarations(targetSyntax, semanticModel))
        {
            foreach (AttributeListSyntax attributeList in declaration.AttributeLists)
            {
                foreach (AttributeSyntax attribute in attributeList.Attributes)
                {
                    string attributeName = GetSimpleName(attribute.Name.ToString());

                    if (attributeName != ArrayDimsAttributeName && attributeName != $"{ArrayDimsAttributeName}Attribute")
                        continue;

                    List<long> dims = [];

                    foreach ((string _, string value) in attribute.GetArgumentValues())
                    {
                        // Any argument that is not a plain integer literal abandons the whole stamp:
                        // a partially read dims list would describe an array shape that does not
                        // exist, which is worse than the length-zero backing this exists to fix.
                        if (!long.TryParse(value.Trim().TrimEnd('L', 'l'), out long dim))
                            return [];

                        dims.Add(dim);
                    }

                    return dims.ToArray();
                }
            }
        }

        return [];
    }

    private static string[] GetValueCloneFields(BaseTypeDeclarationSyntax targetSyntax, SemanticModel semanticModel)
    {
        foreach (BaseTypeDeclarationSyntax declaration in GetPartialDeclarations(targetSyntax, semanticModel))
        {
            foreach (AttributeListSyntax attributeList in declaration.AttributeLists)
            {
                foreach (AttributeSyntax attribute in attributeList.Attributes)
                {
                    string attributeName = GetSimpleName(attribute.Name.ToString());

                    if (attributeName != ValueCloneAttributeName && attributeName != $"{ValueCloneAttributeName}Attribute")
                        continue;

                    return attribute.GetArgumentValues()
                        .Select(argument => argument.value.Trim())
                        .Where(value => value.Length > 2 && value[0] == '"' && value[value.Length - 1] == '"')
                        .Select(value => value[1..^1])
                        .ToArray();
                }
            }
        }

        return [];
    }

    // Every declaration that makes up the (partial) type, starting with the one the syntax receiver
    // matched. A type whose symbol cannot be resolved yields just that declaration, which is what
    // this generator read before the accessibility record existed to carry attributes.
    private static IEnumerable<BaseTypeDeclarationSyntax> GetPartialDeclarations(BaseTypeDeclarationSyntax targetSyntax, SemanticModel semanticModel)
    {
        yield return targetSyntax;

        if (semanticModel.GetDeclaredSymbol(targetSyntax) is not INamedTypeSymbol typeSymbol)
            yield break;

        foreach (SyntaxReference reference in typeSymbol.DeclaringSyntaxReferences)
        {
            if (reference.GetSyntax() is not BaseTypeDeclarationSyntax declaration)
                continue;

            // Skip the one already yielded. Identity is tree + span: a SyntaxReference materializes
            // its node on demand, so reference equality is not guaranteed to hold against the node
            // the receiver matched.
            if (declaration.SyntaxTree == targetSyntax.SyntaxTree && declaration.Span == targetSyntax.Span)
                continue;

            yield return declaration;
        }
    }

    private static (string keyTypeName, string valueTypeName) SplitMapTypes(string typeDefinition)
    {
        string mapTypes = typeDefinition[4..^1];
        int depth = 0;

        for (int i = 0; i < mapTypes.Length; i++)
        {
            char ch = mapTypes[i];

            switch (ch)
            {
                case '<':
                case '[':
                case '(':
                    depth++;
                    break;
                case '>':
                case ']':
                case ')':
                    if (depth > 0)
                        depth--;
                    break;
                case ',' when depth == 0:
                    return (mapTypes[..i].Trim(), mapTypes[(i + 1)..].Trim());
            }
        }

        return (mapTypes.Trim(), string.Empty);
    }

    private static string QualifySourceAliasReferences(string typeName, SyntaxTree syntaxTree, SemanticModel semanticModel)
    {
        if (string.IsNullOrWhiteSpace(typeName))
            return typeName;

        string result = typeName;

        foreach (UsingDirectiveSyntax directive in syntaxTree.GetRoot().DescendantNodes().OfType<UsingDirectiveSyntax>())
        {
            if (directive is not { Alias: not null, Name: not null } || !directive.GlobalKeyword.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.None))
                continue;

            string alias = directive.Alias.Name.Identifier.Text;

            if (result.IndexOf($"{alias}.", StringComparison.Ordinal) < 0)
                continue;

            ISymbol? symbol = semanticModel.GetSymbolInfo(directive.Name).Symbol;
            string target = symbol?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ?? directive.Name.ToString();

            // Two descriptor conventions coexist: the SOURCE-ALIAS form ("CrossPkgLib.Ticks",
            // the map key/value emitter) that this substitution resolves, and the NAMESPACE-
            // QUALIFIED form ("io.fs_package.FileInfo", the slice/array element and defined-
            // over-selector emitters) that must pass through untouched. They are told apart by
            // the segment AFTER the leading identifier: a real alias maps to a package CLASS,
            // so the next segment is a TYPE name — a "_package"-suffixed next segment means the
            // leading identifier is a namespace segment that merely COLLIDES with a file alias
            // (net/http's fs.go aliases `io` while `[]io.fs_package.FileInfo` roots io/fs), and
            // substituting it mangles the reference (CS0426 ×48). The negative lookahead skips
            // exactly those occurrences.
            result = Regex.Replace(result, $@"(^|[<,\s\(\[]){Regex.Escape(alias)}\.(?![^.<>,\s\(\)\[\]]*{Regex.Escape(PackageSuffix)}\.)", $"$1{target}.");
        }

        return GlobalQualify(result);
    }
    // Walks a defined-type-over-defined-type chain to its ARRAY-backed bottom, recursively — Go
    // permits arbitrary depth (`type pallocBits pageBits`, `type pageBits [8]uint64`: pallocBits's
    // OWN [GoType] spells the plain name "pageBits", not an array literal; only pageBits's does,
    // one hop further), so a single GetGoTypeDefinition read only ever sees whichever hop it was
    // pointed at. Each hop tries SOURCE resolution first, then METADATA — the whitebox `-tests`
    // bridge can cross the assembly seam at any point in the chain, independent of where an
    // earlier or later hop resolved. Returns the array's ELEMENT type, or null once the chain
    // bottoms out at something else (a struct with real fields, an interface, a slice, an
    // unresolvable name) before ever reaching a `[N]elem` definition. `seen` guards against a
    // malformed cycle looping forever; Go itself forbids one, same posture as the embed walk above.
    private static string? ResolveArrayElementType(GeneratorExecutionContext context, string typeName, HashSet<string> seen)
    {
        if (!seen.Add(typeName))
            return null;

        INamedTypeSymbol? containingType = null;
        string? definition;
        (StructDeclarationSyntax? structDecl, Compilation? structCompilation) = context.GetStructDeclaration(typeName);

        if (structDecl is not null)
        {
            definition = GetGoTypeDefinition(structDecl);
            containingType = (structCompilation?.GetSemanticModel(structDecl.SyntaxTree).GetDeclaredSymbol(structDecl) as INamedTypeSymbol)?.ContainingType;
        }
        else if (context.FindUnderlyingStructSymbol(typeName) is { } symbol)
        {
            definition = GetGoTypeDefinition(symbol);
            containingType = symbol.ContainingType;
        }
        else
            return null;

        if (definition is null)
            return null;

        if (definition.StartsWith("[") && !definition.StartsWith("[]"))
        {
            int closeBracket = definition.IndexOf(']');
            return closeBracket > 0 && closeBracket < definition.Length - 1 ? definition[(closeBracket + 1)..].Trim() : null;
        }

        // A BARE name (no `.` at all) is Go's own same-package convention — `type pallocBits
        // pageBits` needs no `runtime_package.` prefix because pageBits lives in the identical
        // package, so its [GoType] spells just "pageBits". Neither GetStructDeclaration nor
        // FindUnderlyingStructSymbol's "go.-rooted" retry reaches that far — the retry only
        // restores a missing `go.` ROOT, not an entirely missing PACKAGE segment — so a bare
        // recursion target is qualified with the CURRENT hop's own containing package before the
        // next lookup, reusing the exact qualified form that already resolves everywhere else.
        if (!definition.Contains('.') && containingType is not null)
            definition = $"{containingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}.{definition}";

        return ResolveArrayElementType(context, definition, seen);
    }

    // Reads a struct declaration's own [GoType("…")] definition string (first constructor argument,
    // quotes stripped) — used to inspect the UNDERLYING type of a defined-over-defined chain
    // (`type pallocBits pageBits`: pageBits' definition is "[8]uint64"). Null when the struct has no
    // GoType attribute or no argument.
    private static string? GetGoTypeDefinition(StructDeclarationSyntax structDeclaration)
    {
        foreach (AttributeListSyntax attributeList in structDeclaration.AttributeLists)
        {
            foreach (AttributeSyntax attribute in attributeList.Attributes)
            {
                string name = attribute.Name.ToString();

                if (name != AttributeName && name != $"{AttributeName}Attribute")
                    continue;

                (string _, string value)[] arguments = attribute.GetArgumentValues();

                if (arguments.Length > 0)
                {
                    string value = arguments[0].value;

                    if (!string.IsNullOrWhiteSpace(value) && value.Length > 2)
                        return value[1..^1].Trim();
                }
            }
        }

        return null;
    }

    // METADATA counterpart to the overload above: reads a foreign symbol's own [GoType] attribute
    // via Roslyn's symbol-level AttributeData rather than syntax (a real MSBuild build resolves a
    // cross-assembly underlying-of-underlying — e.g. `-tests` whitebox PallocBits -> pallocBits ->
    // pageBits, where pallocBits itself is only reachable as metadata — by symbol, never syntax).
    // AttributeData.ConstructorArguments already holds the resolved constant, unlike
    // AttributeSyntax's raw quoted token text, so no quote-stripping is needed here.
    private static string? GetGoTypeDefinition(ITypeSymbol typeSymbol)
    {
        string? value = typeSymbol.GetAttributes()
            .FirstOrDefault(attribute => attribute.AttributeClass?.Name is $"{AttributeName}Attribute" or AttributeName)
            ?.ConstructorArguments.FirstOrDefault().Value as string;

        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    // Unions the two reasons a member cannot compare with C# `==` into the one set the template
    // consults. `null` is meaningful downstream — with no fallback set AND no whole-struct equality
    // constraint, the template emits a constant-false Equals — so an EMPTY interface-member set must
    // leave a null fallback null rather than turning it into an empty one.
    private static HashSet<string>? CombineEqualityFallbacks(HashSet<string>? fallbackMembers, HashSet<string> interfaceMembers)
    {
        if (interfaceMembers.Count == 0)
            return fallbackMembers;

        if (fallbackMembers is null)
            return interfaceMembers;

        fallbackMembers.UnionWith(interfaceMembers);
        return fallbackMembers;
    }
}
