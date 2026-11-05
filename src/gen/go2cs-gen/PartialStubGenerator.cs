// PartialStubGenerator.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static go2cs.Common;
using static go2cs.Symbols;

namespace go2cs;

// Finds bodyless `partial` method declarations — go2cs emits these for Go functions with
// no body (assembly/cgo implemented). Their implementation is provided either by a
// hand-written companion (e.g. sync/atomic's doc_impl.cs) or, when none exists, by the
// PartialStubGenerator below.
public sealed class BodylessPartialMethodFinder : ISyntaxReceiver
{
    public List<MethodDeclarationSyntax> Candidates { get; } = [];

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is MethodDeclarationSyntax { Body: null, ExpressionBody: null } method &&
            method.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.PartialKeyword)))
        {
            Candidates.Add(method);
        }
    }
}

// Emits a throwing implementation for every bodyless `partial` method that has no other
// implementing part in the compilation. This lets the converter emit asm/cgo functions as
// partial declarations: packages that ship a hand-written implementation companion use
// those bodies, while companion-less packages get a default stub so the code compiles
// (instead of CS8795 "partial method must have an implementation").
[Generator]
public class PartialStubGenerator : ISourceGenerator
{
    // Attributes that oblige a DIFFERENT source generator to implement the partial method they are
    // applied to. A declaration carrying one is not an unimplemented Go function and gets no stub;
    // see the skip in Execute for why the compilation cannot be asked instead. Today the converted
    // corpus uses exactly one — hand-owned FFI declarations under [LibraryImport] — and a second
    // (JSImport, GeneratedComInterface, …) would be added here rather than worked around at the
    // call site.
    private const string GeneratorImplementedPartialAttribute = "System.Runtime.InteropServices.LibraryImportAttribute";

    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new BodylessPartialMethodFinder());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not BodylessPartialMethodFinder finder)
            return;

        int index = 0;

        foreach (MethodDeclarationSyntax methodSyntax in finder.Candidates)
        {
            SemanticModel semanticModel = context.Compilation.GetSemanticModel(methodSyntax.SyntaxTree);

            if (semanticModel.GetDeclaredSymbol(methodSyntax) is not IMethodSymbol symbol)
                continue;

            // Only a partial DEFINITION that has no implementing part needs a stub. A
            // hand-written companion (e.g. sync/atomic doc_impl.cs) supplies the real body
            // for these asm functions and is detected here as PartialImplementationPart.
            if (!symbol.IsPartialDefinition || symbol.PartialImplementationPart is not null)
                continue;

            string packageNamespace = methodSyntax.GetNamespaceName();
            string packageClassName = methodSyntax.GetParentClassName();
            string identifier = methodSyntax.Identifier.Text;

            if (packageNamespace.Length == 0 || packageClassName.Length == 0)
                continue;

            // The -tests package-init hook is DESIGNED to erase when unimplemented (a classic
            // partial method; the production assembly excludes the test-side implementation) —
            // a stub here would throw from the package class's static ctor for every consumer
            // of the production assembly. It is go2cs init machinery, never an asm/cgo function.
            if (identifier == PackageTestInitHookMethod)
                continue;

            // A partial declaration whose body ANOTHER source generator is contractually obliged to
            // supply is not a bodyless Go function, and must not be stubbed.
            //
            // PartialImplementationPart above cannot answer this: source generators do not observe
            // each other's output, so from here the [LibraryImport] declaration looks exactly like an
            // asm function with no companion. Stubbing it produced TWO implementing parts and the
            // whole package failed with CS0757 — for every P/Invoke at once, and only once a hand-own
            // adopted the source-generated form. The test is therefore on the ATTRIBUTE, which is the
            // obligation itself, and it is a semantic lookup so a using-alias cannot hide it.
            if (symbol.GetAttributes().Any(attribute =>
                    attribute.AttributeClass?.ToDisplayString() is GeneratorImplementedPartialAttribute))
                continue;

            // Reuse the declaration's exact signature (modifiers, return type, type params,
            // parameters, constraints) so the implementing part matches the definition, then
            // give it a throwing expression body.
            MethodDeclarationSyntax stub = methodSyntax
                .WithAttributeLists(default)
                .WithBody(null)
                .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(
                    SyntaxFactory.ParseExpression(
                        $"throw new global::System.NotImplementedException(\"{identifier}: no implementation reached this compilation (assembly, cgo, or a linkname whose push did not arrive)\")")))
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                .WithLeadingTrivia()
                .WithTrailingTrivia()
                .NormalizeWhitespace();

            string[] usingStatements = GetFullyQualifiedUsingStatements(methodSyntax.SyntaxTree, semanticModel);

            // The package's using aliases go AFTER the file-scoped namespace declaration —
            // namespace-scoped, matching the converter's own file layout. A FILE-level alias
            // loses simple-name lookup to an enclosing namespace SEGMENT: in
            // `namespace go.@internal.syscall`, a file-level `using syscall = …` is shadowed by
            // the namespace's own `syscall` segment, so the stub's `ж<syscall.WSABuf>` resolved
            // as `go.@internal.syscall.WSABuf` (CS0234 + CS0759/CS8795 signature mismatch —
            // internal/syscall/windows WSASendtoInet4/6).
            // The [go.GoExternalStub] marker records what only this generator knows: that NOTHING
            // in the compilation implements this method. The two checks above are what make that
            // exact — a partial another generator is obliged to implement is skipped, and so is one
            // a hand-written *_impl.cs companion supplies. That is ALL it records: nothing here implements
            // it. It may be assembly, cgo, a dylib trampoline or an unarrived linkname -- the generator sees
            // ONE compilation and cannot tell which, so neither the marker nor the message says.
            //
            // internal/abi's FuncPCABI0/FuncPCABIInternal are the consumer: handed a method group,
            // they must answer either a synthetic PC (the function exists and can be symbolized) or
            // a loud failure (it does not exist, and no number is honest). Neither available proxy
            // decides that. A bodyless partial DECLARATION covers assembly routines and darwin's
            // dylib trampolines alike, and [GeneratedCode] is minted once for every generator in
            // this analyzer (Common.cs), so RecvGenerator's ж-overloads carry it too — runtime's
            // time.cs passes exactly one of those, (*timers).run, a real function with a real body.
            // See docs/phase4/DESIGN-synthetic-pc-registry.md.
            string generatedSource =
                $$"""
                // <auto-generated/>
                #nullable enable
                using System.CodeDom.Compiler;

                namespace {{packageNamespace}};

                {{string.Join("\r\n", usingStatements)}}

                partial class {{packageClassName}}
                {
                    [{{GeneratedCodeAttribute}}]
                    [global::go.GoExternalStub]
                    {{stub}}
                }
                """;

            context.AddGeneratedSource(GetValidFileName($"{packageNamespace}.{packageClassName}.{identifier}.{index++}.stub.g.cs"), generatedSource);
        }
    }
}
