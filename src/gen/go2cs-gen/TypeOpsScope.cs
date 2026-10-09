// TypeOpsScope.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace go2cs;

/// <summary>
/// 3c-2b(b), a SIZE VARIANT for the owner's stage-3 table (COORD 1x:xx), not a seat: the generated operations face
/// (golib's IGoTypeOpsSource) only on a type whose POINTER this compilation uses, so a type no code ever points to does
/// not pull the whole ж&lt;S&gt; machinery into a Native AOT executable (measured: that machinery, not GoTypeOps itself,
/// was the +41% on an fmt/reflect program).
/// </summary>
/// <remarks>
/// "Uses its pointer" is read from SYNTAX, once per compilation: a <c>ж&lt;X&gt;</c> type spelling or an <c>@new&lt;X&gt;</c>
/// call, keyed by X's simple name. It is an approximation the converter could make exact: a pointer reached only through
/// <c>var</c> inference is missed, and a simple-name collision across packages over-includes (the conservative direction).
/// A type without the face falls to golib's dynamic fallback for reflect.New / Zero(*S), which Native AOT cannot run for a
/// value type it never compiled.
/// </remarks>
internal static class TypeOpsScope
{
    [ThreadStatic] private static HashSet<string>? s_pointerUsed;

    internal static void Begin(Compilation compilation)
    {
        HashSet<string> used = new(StringComparer.Ordinal);

        foreach (SyntaxTree tree in compilation.SyntaxTrees)
        {
            foreach (GenericNameSyntax generic in tree.GetRoot().DescendantNodes().OfType<GenericNameSyntax>())
            {
                string name = generic.Identifier.ValueText;

                if ((name == "ж" || name == "new") && generic.TypeArgumentList.Arguments.Count == 1 && SimpleName(generic.TypeArgumentList.Arguments[0]) is { } target)
                    used.Add(target);
            }
        }

        s_pointerUsed = used;
    }

    internal static bool Wants(string typeName)
    {
        int angle = typeName.IndexOf('<');
        string simple = angle < 0 ? typeName : typeName.Substring(0, angle);

        return s_pointerUsed is null || s_pointerUsed.Contains(simple);
    }

    private static string? SimpleName(TypeSyntax type) => type switch
    {
        QualifiedNameSyntax qualified => SimpleName(qualified.Right),
        AliasQualifiedNameSyntax alias => SimpleName(alias.Name),
        GenericNameSyntax generic => generic.Identifier.ValueText,
        IdentifierNameSyntax identifier => identifier.Identifier.ValueText,
        _ => null
    };
}
