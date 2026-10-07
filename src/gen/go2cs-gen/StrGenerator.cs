// StrGenerator.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Linq;
using go2cs.Templates.Str;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static go2cs.Common;
using static go2cs.Symbols;

namespace go2cs;

// The back end of an sstring TWIN (docs/phase4/DESIGN-sstring-twin-pilot.md). The converter emits only
// the member that carries the Go body: each twinned parameter typed `sstring`, marked [GoStr].
// This generator emits its two companions, so the visible file keeps one method per Go function:
//
//   - the @string FORWARDER: the same name and signature with each sstring parameter typed @string,
//     forwarding to the body member under [OverloadResolutionPriority(-1)]. The negative priority on
//     the forwarder does what +1 on the body member would: wherever both are applicable, which is every
//     call with an @string, a C# string or (through golib's implicit operator) a u8 literal, the body
//     member binds. The forwarder is reached only where its exact signature is required: reflection,
//     interface binding, and a deferred call, whose arguments are @string generic type arguments;
//   - for a PACKAGE-LEVEL function, the canonical value delegate `<Name>ᶠ`. A twin has no single method
//     group (a method-group conversion to a delegate typed on @string is CS0123), so the converter names
//     this field at every func-value site. Its lambda records the Go name ([GoTwinForwarder]) for
//     GoNameOf / FuncForPC, and its body is an ordinary call that binds the body member.
//
// A [GoRecv] twin needs no ж overload for its forwarder: RecvGenerator gives the visible body member
// one, and a pointer-receiver call with an @string argument binds it through the implicit view. (It
// could not anyway: a source generator never sees another generator's output.) The forwarder carries
// [GeneratedCode], so a traceback skips it exactly as it skips RecvGenerator's forwarders.
[Generator]
public class StrGenerator : ISourceGenerator
{
    private const string FullAttributeName = "go.GoStrAttribute";

    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new AttributeFinder<MethodDeclarationSyntax>(FullAttributeName));
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxContextReceiver is not AttributeFinder<MethodDeclarationSyntax> { HasAttributes: true } attributeFinder)
            return;

        HashSet<string> emittedHintNames = new(StringComparer.OrdinalIgnoreCase);

        foreach ((MethodDeclarationSyntax methodSyntax, List<AttributeSyntax> _) in attributeFinder.TargetAttributes)
        {
            SemanticModel semanticModel = context.Compilation.GetSemanticModel(methodSyntax.SyntaxTree);

            if (semanticModel.GetDeclaredSymbol(methodSyntax) is not IMethodSymbol method)
                continue;

            string packageNamespace = methodSyntax.GetNamespaceName();
            string packageClassName = methodSyntax.GetParentClassName();

            if (packageNamespace.Length == 0 || packageClassName.Length == 0)
                continue;

            string packageName = packageClassName.EndsWith(PackageSuffix) ? packageClassName[..^PackageSuffix.Length] : packageClassName;

            StrTemplate template = new()
            {
                PackageNamespace = packageNamespace,
                PackageName = packageName,
                Method = method,
                ParameterNames = methodSyntax.ParameterList.Parameters.Select(parameter => parameter.Identifier.Text).ToArray(),
                NoInlining = RecvGenerator.HasNoInliningMark(methodSyntax, semanticModel)
            };

            // A twin whose shape the forwarder cannot re-declare is not emitted: the converter refuses
            // every such registration first (validateSStringTwin), so this only guards a hand-written use.
            if (!template.IsRenderable)
                continue;

            context.AddSource(GetUniqueHintName(emittedHintNames, GetValidFileName($"{packageNamespace}.{packageClassName}.{method.Name}.str.g.cs")), template.Generate());
        }
    }
}
