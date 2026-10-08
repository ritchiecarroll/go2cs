// InterfaceTypeTemplate.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Linq;
using System.Text;
using static go2cs.Common;
using static go2cs.Symbols;

namespace go2cs.Templates.InterfaceType;

internal class InterfaceTypeTemplate : TemplateBase
{
    // Template Parameters
    public required string InterfaceName;
    public required string[] OperatorConstraints;
    public required MethodInfo[] Methods;

    // Set for a non-generic, non-constraint, non-empty interface — NAMED or anonymous ("dyn") alike:
    // the two runtime duck-typing shells are emitted beside it and discovered through a
    // [GoInterfaceShell] stamp (see InterfaceShellEmitter). The shells are the ONLY duck-typing
    // surface a converted interface has — the marker-named ᴛAs conversion methods a dyn interface
    // once carried are retired, along with the second renderer that drove them.
    public bool EmitShells;

    private InterfaceShellEmitter? m_shells;
    private string? m_nonGenericInterfaceName;

    private const string Indent = "        ";

    // Define a type T variable that will not conflict with any method generic types
    private const string TypeT = $"{ShadowVarMarker}T";

    public override string Generate()
    {
        string[] RequiredUsings = ["using System.Diagnostics;", "using System.Reflection;", "using go.golib;"];

        UsingStatements = UsingStatements is null ?
            RequiredUsings :
            UsingStatements.Concat(RequiredUsings).ToArray();

        return base.Generate();
    }

    public override string TemplateBody =>
        $$"""
              {{GoTypeAttributePrefix}}{{ShellAttribute}}[{{GeneratedCodeAttribute}}]
              {{Scope}} partial interface {{InterfaceName}}{{AppliedOperatorConstraints}}
              {
              }{{RuntimeInterfaceShells}}
          """;

    private InterfaceShellEmitter? Shells => EmitShells ? m_shells ??= new InterfaceShellEmitter(NonGenericInterfaceName, Scope, Methods) : null;

    private string ShellAttribute => Shells?.Attribute ?? "";

    private string RuntimeInterfaceShells => Shells?.Shells ?? "";

    private string AppliedOperatorConstraints
    {
        get
        {
            if (OperatorConstraints.Length == 0)
                return string.Empty;

            StringBuilder implementation = new();
            string constraints = string.Join(",\r\n", OperatorConstraints.SelectMany(GetConstraintName));

            implementation.AppendLine(" :");
            implementation.AppendLine(constraints);
            implementation.AppendLine($"{Indent}where {TypeT} :");
            implementation.Append(constraints);

            return implementation.ToString();
        }
    }

    private static string[] GetConstraintName(string name)
    {
        return name switch
        {
            "Sum" => [$"{Indent}IAdditionOperators<{TypeT}, {TypeT}, {TypeT}>"],
            // Increment/decrement live in the numeric-only Arithmetic set (never the
            // string-including Sum set) — mirrors the converter's lifted constraint list in
            // constraintOperations.go getLiftedConstraints; keep the two in sync.
            "Arithmetic" => [$"{Indent}ISubtractionOperators<{TypeT}, {TypeT}, {TypeT}>", $"{Indent}IMultiplyOperators<{TypeT}, {TypeT}, {TypeT}>", $"{Indent}IDivisionOperators<{TypeT}, {TypeT}, {TypeT}>", $"{Indent}IIncrementOperators<{TypeT}>", $"{Indent}IDecrementOperators<{TypeT}>", $"{Indent}IUnaryNegationOperators<{TypeT}, {TypeT}>"],
            // Shift-count parameter is int, matching the BCL IShiftOperators<TSelf, int, TSelf>
            // shape (see the converter's lifted Integer constraint set in constraintOperations.go).
            "Integer" => [$"{Indent}IModulusOperators<{TypeT}, {TypeT}, {TypeT}>", $"{Indent}IBitwiseOperators<{TypeT}, {TypeT}, {TypeT}>", $"{Indent}IShiftOperators<{TypeT}, int, {TypeT}>"],
            "Comparable" => [$"{Indent}IEqualityOperators<{TypeT}, {TypeT}, bool>"],
            "Ordered" => [$"{Indent}IComparisonOperators<{TypeT}, {TypeT}, bool>"],
            _ => [$"{Indent}{name}"]
        };
    }

    private string NonGenericInterfaceName => m_nonGenericInterfaceName ??= GetNonGenericInterfaceName();

    private string GetNonGenericInterfaceName()
    {
        int startIndex = InterfaceName.IndexOf('<');
        return startIndex < 0 ? InterfaceName : InterfaceName[..startIndex];
    }
}
