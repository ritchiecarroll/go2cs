// ImplicitConvTemplate.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Linq;
using static go2cs.Common;
using static go2cs.Symbols;

namespace go2cs.Templates.ImplicitConv;

internal class ImplicitConvTemplate : TemplateBase
{
    // Template Parameters
    public required string SourceTypeName;
    public required string TargetTypeName;
    public required bool Inverted;
    public required bool Indirect;
    public required string? ValueType;
    public required List<(string typeName, string memberName)> StructMembers;

    // Optional overrides for the cross-package numeric inverse case: the source named-numeric type is
    // FOREIGN (declared in another assembly, e.g. `nameOff` aliasing `abi.NameOff`), so it can neither
    // host the operator here (the default `partial struct {SourceTypeName}` would be a phantom local
    // type → CS1729) nor be reached by a direct `ulong`→foreign-named cast (no cross-assembly route →
    // CS0030). When set, the operator is hosted in the LOCAL type and constructs the foreign type
    // through its underlying basic. Null preserves the default behavior byte-for-byte.
    public string? HostTypeNameOverride = null;
    public string? LHTypeNameOverride = null;
    public string? RHTypeNameOverride = null;
    public string? ConvExprOverride = null;

    public override string TemplateBody =>
        $$"""
             partial struct {{HostTypeName}}
             {
                 public static implicit operator {{LHTypeName}}({{RHTypeName}} src) => {{ConvExprValue}};
             }    
         """;

    private string HostTypeName => HostTypeNameOverride ?? SourceTypeName;

    public string LHTypeName => LHTypeNameOverride ?? (Inverted ? SourceTypeName : TargetTypeName);

    public string RHTypeName => RHTypeNameOverride ?? (Inverted ? TargetTypeName : SourceTypeName);

    private string ConvExprValue => ConvExprOverride ?? ConvExpr;

    // The operator never returns a box: ImplicitConvGenerator refuses a record whose operator would return
    // one from a value (GO2CS0008), since it could only box a copy, and a box parameter has no struct source.
    private string ConvExpr => $"new {LHTypeName}({ParamList})";

    private string ParamList => string.IsNullOrWhiteSpace(ValueType) ? 
        string.Join(", ", StructMembers.Select(GetParamExpr)) : 
        $"({ValueType})src.Value";

    private string GetParamExpr((string, string) member)
    {
        (string typeName, string memberName) = member;
        return Indirect && PointerExpr.IsMatch(typeName) ? $"src.{memberName}?.Value ?? default!" : $"src.{memberName}";
    }
}
