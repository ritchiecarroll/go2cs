// InterfaceAdapterImplTemplate.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Collections.Generic;
using System.Text;
using static go2cs.Common;

namespace go2cs.Templates.InterfaceImpl;

/// <summary>
/// Generates an interface-sourced implementation adapter for a
/// <c>GoImplement&lt;TSourceInterface, TTargetInterface&gt;</c> attribute.
/// </summary>
/// <remarks>
/// Go interface assignability is structural, so an interface value whose method set is a superset
/// of another interface can flow into the smaller interface even when the declarations live in
/// different packages. C# only permits nominal interface conversion, so this adapter wraps the
/// source interface value and forwards the target interface methods to it.
/// </remarks>
internal class InterfaceAdapterImplTemplate : TemplateBase
{
    // Template Parameters
    public required string SourceInterfaceName;
    public required string InterfaceName;
    public required string AdapterName;
    public required string AdapterScope;
    public bool ImplementsFormattable;
    public required List<MethodInfo> Methods;

    public override string TemplateBody =>
        $$"""
             /// <summary>
             /// Interface-sourced '{{GetSimpleName(InterfaceName)}}' implementation adapter for '{{SourceInterfaceName}}'.
             /// </summary>
             [{{NonUserCodeAttribute}}]
             {{AdapterScope}} sealed class {{AdapterName}} : {{InterfaceName}}, IInterfaceAdapter
             {
                 private readonly {{SourceInterfaceName}} m_value;

                 public {{AdapterName}}({{SourceInterfaceName}} value) => m_value = value;

                 public object? Value => m_value;

                 {{MethodsImplementation}}

                 public override bool Equals(object? obj) => obj switch
                 {
                     IInterfaceAdapter adapter => object.Equals(m_value, adapter.Value),
                     _ => object.Equals(m_value, obj)
                 };

                 public override int GetHashCode() => m_value?.GetHashCode() ?? 0;

                 public override string? ToString() => m_value?.ToString();{{FormattableImplementation}}
             }
         """;

    private string FormattableImplementation =>
        ImplementsFormattable
            ? "\r\n\r\n        string System.IFormattable.ToString(string? format, System.IFormatProvider? formatProvider) => m_value?.ToString() ?? \"\";"
            : string.Empty;

    private string MethodsImplementation
    {
        get
        {
            StringBuilder result = new();

            foreach (MethodInfo method in Methods)
            {
                string simpleMethodName = GetSimpleName(method.Name);

                if (result.Length > 0)
                    result.Append("\r\n\r\n        ");

                if (method.IsInaccessibleMarker)
                {
                    result.Append($"{method.ReturnType} {method.GetSignature()}{(method.ReturnType == "void" ? " { }" : " => default!;")}");
                    continue;
                }

                result.Append($"{method.ReturnType} {method.GetSignature()} => m_value.{simpleMethodName}{method.GetGenericSignature()}({method.CallParameters});");
            }

            return result.ToString();
        }
    }
}