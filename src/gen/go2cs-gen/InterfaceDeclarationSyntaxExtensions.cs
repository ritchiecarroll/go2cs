// InterfaceDeclarationSyntaxExtensions.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static go2cs.Common;

namespace go2cs;

public static class InterfaceDeclarationSyntaxExtensions
{
    public static MethodInfo[] GetInterfaceMethods(
        this InterfaceDeclarationSyntax interfaceDeclaration,
        GeneratorExecutionContext context)
    {
        // Get the semantic model to access symbol information
        SemanticModel semanticModel = context.Compilation.GetSemanticModel(interfaceDeclaration.SyntaxTree);

        // Get the symbol for the interface
        if (semanticModel.GetDeclaredSymbol(interfaceDeclaration) is not INamedTypeSymbol interfaceSymbol)
            return [];

        // Get all methods, including inherited ones
        List<MethodInfo> allMethods = [];

        // First, collect methods from this interface
        IEnumerable<MethodInfo> directMethods = interfaceDeclaration
            .Members
            .OfType<MethodDeclarationSyntax>()
            .Select(method => method.GetMethodInfo(context.Compilation));

        allMethods.AddRange(directMethods);

        // Next, collect methods from base interfaces. GetAllBaseInterfaces (not AllInterfaces) so a
        // base in ANOTHER package class — which is still PRIVATE until this generator emits its
        // access modifier — contributes its members instead of binding to an empty error symbol.
        foreach (INamedTypeSymbol? baseInterface in interfaceSymbol.GetAllBaseInterfaces(context.Compilation))
        {
            foreach (IMethodSymbol? member in baseInterface.GetMembers().OfType<IMethodSymbol>())
            {
                // Only a base interface's INSTANCE, ORDINARY methods belong to the Go method set
                // being forwarded. A base's STATIC members are not part of any Go interface: golib's
                // hand-written core interfaces carry static duck-typing conversion helpers
                // (`error.As<T>`, `fmt.Stringer.As<T>`). Let those through and an interface EMBEDDING
                // one — an anonymous `interface{ error; Temporary() bool }` — forwards the statics
                // into its Δ wrapper, emitting duplicate `AsByPtr`/`s_AsByPtr` delegate+field pairs
                // for the overloads (CS0102 ×6) plus a forwarding method for a member no Go type can
                // implement. Non-ordinary members (property/event accessors, constructors) are
                // likewise not Go interface methods.
                if (member.IsStatic || member.MethodKind != MethodKind.Ordinary)
                    continue;

                // Skip methods that might be overridden in the derived interface
                if (!allMethods.Any(method => method.IsSameSignature(member)))
                    allMethods.Add(member.GetMethodInfo(context.Compilation));
            }
        }

        return allMethods.ToArray();
    }
}
