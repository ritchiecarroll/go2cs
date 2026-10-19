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

    // Go prints the static interface type an assertion started from ("io.Reader is *os.File, not
    // *bytes.Buffer"). The converted call is `x._<T>()` and golib's object form cannot see x's static
    // type, so an interface carries an extension overload of its own: C# overload resolution prefers
    // the identity conversion to the exact interface over the reference conversion to object, and the
    // call site stays exactly as Go reads.
    //
    // The overload CANNOT be a member of the package class: `_` would then bind to it, as a method
    // group, wherever the class's own code writes `_ = x` or `out _`, and those stop being discards
    // (CS1656, CS1657). It lives in a top-level `<pkg>_package_asserts` class instead (an extension
    // method cannot be nested), still one declaration per interface in its own package.
    //
    // An interface this class cannot serve is spelled by the converter instead, `x._<I, T>()`, which binds
    // golib's `_<TSource, T>` and names the interface exactly:
    //   - an unexported interface is a private nested type of the package class, which a sibling class
    //     cannot name in a signature, so it gets no overload here;
    //   - a generic interface: `_<T>` has one explicit type argument, so it takes no overload;
    //   - an exported one whose class C# extension lookup cannot reach from the asserting file: lookup
    //     tries a namespace's own types before its using directives, and golib's object form sits in the
    //     root namespace and applies to everything, so a file in the root namespace never reaches an
    //     overload in a nested one.
    // Residuals, which keep golib's object form and so the text "interface {}": an unexported interface
    // of ANOTHER package, and an interface declared inside a function, neither of which the asserting
    // file can name. The converter casts the receiver to object there when the interface embeds others,
    // since the bare call would bind an embedded interface's overload and name the wrong interface
    // (or be ambiguous, CS0121); see assertSourceFormFor in the converter.
    public bool EmitAssertOverload;

    private bool HasAssertOverload => EmitAssertOverload && Scope is "public" or "internal";

    public override string TemplateFooter => base.TemplateFooter + AssertSourceOverloadClass;

    private string AssertSourceOverloadClass => !HasAssertOverload ? "" :
        $$"""


        public static partial class {{PackageName}}_package_asserts
        {
            [{{GeneratedCodeAttribute}}]
            {{Scope}} static {{TypeT}} _<[global::System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(
                global::System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicMethods |
                global::System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicConstructors |
                global::System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicFields |
                global::System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.NonPublicFields)] {{TypeT}}>(this {{PackageName}}_package.{{InterfaceName}} source) =>
                source._<{{PackageName}}_package.{{InterfaceName}}, {{TypeT}}>();
        }
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
