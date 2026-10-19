// LiteralInterceptorGenerator.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;
using static go2cs.Common;

namespace go2cs;

/// <summary>
/// Boxes a bare C# string literal in an <c>any</c> slot of converted code as the Go string it is: for every
/// invocation that passes one to an <c>object</c> parameter or a <c>params Span&lt;object&gt;</c> element, an
/// interceptor writes a cached, pre-boxed <c>go.@string</c> into the slot and forwards the call.
/// </summary>
/// <remarks>
/// <para>
/// Outside the standard library the converter renders Go's <c>fmt.Println("hello")</c> as the bare C# literal
/// (owner ruling, 2026-10-17, part 2). A C# string reaches an <c>object</c> parameter by a reference conversion,
/// so without this generator the callee would see <c>System.String</c>, which no Go type switch, assertion or
/// comparison matches. An interceptor replaces the method the call binds, not the argument, so the literal's own
/// conversion (string to object) costs nothing and the box the interceptor substitutes is allocated once per
/// literal per file.
/// </para>
/// <para>
/// The converter emits a bare literal in an <c>any</c> slot only where this generator can intercept the call
/// (hoistedLiteralOperations.go <c>interceptableAnyArg</c>): an ordinary invocation of a non-generic method. Every
/// candidate this generator cannot intercept is GO2CS0005, an error, so a bare literal never reaches an
/// <c>object</c> slot silently: a delegate invocation, a generic method or a method of a generic type, a method
/// the generated file cannot call, a call with no interceptable location, a by-reference return, and a literal
/// <c>u8</c> cannot encode (a lone UTF-16 surrogate). GO2CS0004 (StringLiteralBindingAnalyzer) stays the end-to-end
/// net: it reads an intercepted literal as bound to a Go string, and every other boxed literal as an error, so
/// the build fails even when this generator does not run.
/// </para>
/// <para>
/// GO2CS0006 guards the binding itself. Go has one callee per call, but a C# method group can hold more: a bare
/// literal converts to <c>go.@string</c> and to <c>object</c>, and overload resolution prefers the
/// <c>@string</c> member. Where the bound member does not take an <c>object</c> slot at the literal's position
/// but another applicable member of the group does, a convenience overload added beside a converted callee has
/// captured the slot, and the call no longer reaches the Go function. That is an error by name, never a silent
/// rebind.
/// </para>
/// <para>
/// Only a converted file that holds a call with a bare string-literal argument reaches the transform: the syntax
/// provider's predicate reads that from the syntax alone, and a predicate re-runs only for a changed tree, so a file
/// with no candidate pays no semantic query on any edit. A file that has one re-binds its calls on every compilation
/// change, as any semantic transform does (a probe with ForAttributeWithMetadataName measured the same re-run), and
/// that is what lets GO2CS0006 see an overload added in another file. Each file still gets its own generated file,
/// and only a file whose output changed is re-emitted. The cache class and the interceptor class are
/// <c>file</c>-local in namespace <c>go2cs.Interceptors</c>, which go.lib.targets admits
/// (<c>InterceptorsNamespaces</c>). Interceptors that write the same literals into the same slots of the same
/// method share one body. An interceptor is a real frame: it carries <c>[GeneratedCode("go2cs-gen")]</c> and
/// <c>[StackTraceHidden]</c>, which the Go traceback (runtime managed_impl.cs, <c>classifyGoSourceFrame</c>) and
/// .NET's stack trace skip, and <c>[DebuggerStepThrough]</c>.
/// </para>
/// </remarks>
[Generator]
public sealed class LiteralInterceptorGenerator : IIncrementalGenerator
{
    private const string GoTakesCSharpStringAttributeName = "go.GoTakesCSharpStringAttribute";


    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<FileOutput> files = context.SyntaxProvider.CreateSyntaxProvider(
                static (node, _) => IsTransformInput(node),
                static (syntaxContext, cancellationToken) => Transform(syntaxContext.SemanticModel, (CompilationUnitSyntax)syntaxContext.Node, cancellationToken))
            .Where(static output => output is not null)!;

        context.RegisterSourceOutput(files, static (productionContext, output) =>
        {
            foreach (DiagnosticRecord record in output.Diagnostics)
                productionContext.ReportDiagnostic(record.ToDiagnostic());

            if (output.Source.Length > 0)
                productionContext.AddSource(output.HintName, output.Source);
        });
    }

    // A call with a bare C# string literal among its arguments: the only shape the transform reads, decided from syntax.
    private static bool IsCandidate(InvocationExpressionSyntax invocation) =>
        invocation.ArgumentList.Arguments.Any(argument => argument.Expression.IsKind(SyntaxKind.StringLiteralExpression));

    /// <summary>
    /// The syntax provider's predicate: a converted file holding at least one call with a bare string-literal argument.
    /// It reads syntax only, so it re-runs only for a changed tree, and a file it refuses pays no semantic query.
    /// </summary>
    public static bool IsTransformInput(SyntaxNode node) =>
        node is CompilationUnitSyntax root && IsConvertedSourceFile(root.SyntaxTree) &&
        root.DescendantNodes().OfType<InvocationExpressionSyntax>().Any(IsCandidate);

    private static FileOutput? Transform(SemanticModel semanticModel, CompilationUnitSyntax root, CancellationToken cancellationToken)
    {
        SyntaxTree tree = root.SyntaxTree;

        if (IsHandOwnedAssembly(semanticModel.Compilation) || semanticModel.Compilation.GetTypeByMetadataName("go.string") is null)
            return null;

        INamedTypeSymbol? marker = semanticModel.Compilation.GetTypeByMetadataName(GoTakesCSharpStringAttributeName);
        List<DiagnosticRecord> diagnostics = [];
        List<(InterceptableLocation location, IMethodSymbol method, List<Slot> slots)> sites = [];

        foreach (InvocationExpressionSyntax invocation in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!IsCandidate(invocation))
                continue;

            IMethodSymbol method;
            List<Slot> slots;

            if (semanticModel.GetOperation(invocation, cancellationToken) is IInvocationOperation operation)
            {
                CheckOverloadDrift(semanticModel, invocation, operation, marker, diagnostics, cancellationToken);
                method = operation.TargetMethod;
                slots = BoxedLiteralSlots(operation, marker);
            }
            else if (SoleCandidate(semanticModel, invocation, cancellationToken) is { } candidate)
            {
                // The call does not bind in THIS compilation: an argument names a member another generator adds (a
                // TypeGenerator property of a defined type, `a.Value[1]`), and generators all read the same input. With
                // one candidate in the method group, the final binding can only be that member.
                method = candidate.method;
                slots = PositionalSlots(invocation, candidate.method, candidate.receiverShift, marker);
            }
            else
            {
                continue;
            }

            if (slots.Count == 0)
                continue;
            string? refusal = Refusal(semanticModel, method, slots);
            InterceptableLocation? location = null;

            if (refusal is null)
            {
                location = semanticModel.GetInterceptableLocation(invocation, cancellationToken);
                refusal = location is null ? "the call has no interceptable location" : null;
            }

            if (refusal is not null)
            {
                foreach (Slot slot in slots)
                    diagnostics.Add(DiagnosticRecord.Create(GeneratorDiagnostics.LiteralNotIntercepted, slot.Literal, slot.LiteralText, method.ToDisplayString(), refusal));

                continue;
            }

            sites.Add((location!, method, slots));
        }

        string source = sites.Count == 0 ? string.Empty : Emit(sites);

        if (source.Length == 0 && diagnostics.Count == 0)
            return null;

        return new FileOutput(HintName(tree.FilePath), source, [.. diagnostics]);
    }

    // One boxed-literal slot of an invocation: the parameter it reaches, the element of a params span (-1 for a fixed
    // parameter) and the literal's value.
    private sealed record Slot(int Ordinal, int Element, string Value, SyntaxNode Literal, string LiteralText);

    // Every argument that is a C# string literal converted to object by reference (no operator): a fixed object
    // parameter, or an element of an expanded params span/array of object. A parameter marked [GoTakesCSharpString]
    // takes the C# string on purpose (panic normalizes it) and is left alone.
    private static List<Slot> BoxedLiteralSlots(IInvocationOperation operation, INamedTypeSymbol? marker)
    {
        List<Slot> slots = [];

        foreach (IArgumentOperation argument in operation.Arguments)
        {
            if (argument.Parameter is not { } parameter || IsMarked(parameter, marker))
                continue;

            if (BoxedLiteral(argument.Value) is { } fixedLiteral)
            {
                slots.Add(new Slot(parameter.Ordinal, -1, fixedLiteral.value, fixedLiteral.syntax, fixedLiteral.syntax.ToString()));
                continue;
            }

            IEnumerable<IOperation> elements = argument.Value switch
            {
                ICollectionExpressionOperation collection => collection.Elements,
                IArrayCreationOperation { Initializer: { } initializer } => initializer.ElementValues,
                _ => []
            };

            int index = 0;

            foreach (IOperation element in elements)
            {
                if (BoxedLiteral(element) is { } elementLiteral)
                    slots.Add(new Slot(parameter.Ordinal, index, elementLiteral.value, elementLiteral.syntax, elementLiteral.syntax.ToString()));

                index++;
            }
        }

        return slots;
    }

    // The one method a call that does not bind can resolve to, read from its method group: exactly one member, and not
    // generic (a generic one is inferred from the arguments, which this compilation cannot see). receiverShift is 1
    // for an extension method called through its receiver (the group reports it reduced; the interceptor takes the
    // unreduced definition, whose parameter 0 is the receiver).
    private static (IMethodSymbol method, int receiverShift)? SoleCandidate(SemanticModel semanticModel, InvocationExpressionSyntax invocation, CancellationToken cancellationToken)
    {
        IMethodSymbol[] group = [.. semanticModel.GetMemberGroup(invocation.Expression, cancellationToken).OfType<IMethodSymbol>()];

        if (group is not [{ } only] || only.IsGenericMethod)
            return null;

        return only.ReducedFrom is { } definition ? (definition, 1) : (only, 0);
    }

    // The boxed-literal slots of a call that does not bind, by argument position: a C# string literal (positional, no
    // ref kind) whose parameter is object, or which falls in a trailing params span or array of object.
    private static List<Slot> PositionalSlots(InvocationExpressionSyntax invocation, IMethodSymbol method, int receiverShift, INamedTypeSymbol? marker)
    {
        List<Slot> slots = [];
        ImmutableArray<IParameterSymbol> parameters = method.Parameters;
        SeparatedSyntaxList<ArgumentSyntax> arguments = invocation.ArgumentList.Arguments;

        if (arguments.Any(argument => argument.NameColon is not null))
            return slots;

        for (int position = 0; position < arguments.Count; position++)
        {
            ArgumentSyntax argument = arguments[position];

            if (argument.Expression is not LiteralExpressionSyntax { RawKind: (int)SyntaxKind.StringLiteralExpression } literal || !argument.RefKindKeyword.IsKind(SyntaxKind.None))
                continue;

            int index = position + receiverShift;

            if (parameters.Length > 0 && parameters[parameters.Length - 1] is { IsParams: true } tail && index >= parameters.Length - 1)
            {
                if (IsObjectSequence(tail.Type) && !IsMarked(tail, marker))
                    slots.Add(new Slot(tail.Ordinal, index - (parameters.Length - 1), literal.Token.ValueText, literal, literal.ToString()));
            }
            else if (index < parameters.Length && parameters[index].Type.SpecialType == SpecialType.System_Object && !IsMarked(parameters[index], marker))
            {
                slots.Add(new Slot(index, -1, literal.Token.ValueText, literal, literal.ToString()));
            }
        }

        return slots;
    }

    private static (string value, SyntaxNode syntax)? BoxedLiteral(IOperation operation)
    {
        if (operation is IConversionOperation { OperatorMethod: null, Type.SpecialType: SpecialType.System_Object, Operand: ILiteralOperation literal } &&
            literal.Type?.SpecialType == SpecialType.System_String &&
            literal.ConstantValue is { HasValue: true, Value: string value })
        {
            return (value, literal.Syntax);
        }

        return null;
    }

    // Why an invocation with boxed-literal slots cannot be intercepted, or null when it can.
    private static string? Refusal(SemanticModel semanticModel, IMethodSymbol method, List<Slot> slots)
    {
        if (method.MethodKind == MethodKind.DelegateInvoke)
            return "it invokes a delegate, which no interceptor can replace";

        for (INamedTypeSymbol? type = method.ContainingType; type is not null; type = type.ContainingType)
        {
            if (type.IsGenericType)
                return "it calls a method of a generic type, whose interceptor would itself be generic (deferred)";
        }

        if (method.IsGenericMethod)
            return "it calls a generic method, whose interceptor would itself be generic (deferred)";

        if (method.ReturnsByRef || method.ReturnsByRefReadonly)
            return "it returns by reference";

        if (!semanticModel.Compilation.IsSymbolAccessibleWithin(method, semanticModel.Compilation.Assembly))
            return "the method is not accessible to generated code in its assembly";

        foreach (Slot slot in slots)
        {
            if (HasLoneSurrogate(slot.Value))
                return $"the literal {slot.LiteralText} holds a lone UTF-16 surrogate, which a Go string cannot carry";
        }

        return null;
    }

    private static bool HasLoneSurrogate(string value)
    {
        for (int i = 0; i < value.Length; i++)
        {
            if (char.IsHighSurrogate(value[i]) && i + 1 < value.Length && char.IsLowSurrogate(value[i + 1]))
            {
                i++;
                continue;
            }

            if (char.IsSurrogate(value[i]))
                return true;
        }

        return false;
    }

    // GO2CS0006: a bare literal argument binds a member whose parameter at the literal's position is not an object
    // slot, while another applicable member of the same method group takes it as one. The bound member is read in
    // its call-site form (an extension method called through its receiver is the REDUCED method, whose parameters
    // start after the receiver), which is the form GetMemberGroup reports its candidates in.
    private static void CheckOverloadDrift(SemanticModel semanticModel, InvocationExpressionSyntax invocation, IInvocationOperation operation, INamedTypeSymbol? marker, List<DiagnosticRecord> diagnostics, CancellationToken cancellationToken)
    {
        if (!MayHaveOverloads(operation.TargetMethod))
            return;

        IMethodSymbol bound = semanticModel.GetSymbolInfo(invocation, cancellationToken).Symbol as IMethodSymbol ?? operation.TargetMethod;
        IMethodSymbol boundDefinition = (bound.ReducedFrom ?? bound).OriginalDefinition;
        SeparatedSyntaxList<ArgumentSyntax> arguments = invocation.ArgumentList.Arguments;
        ImmutableArray<ISymbol>? group = null;

        for (int position = 0; position < arguments.Count; position++)
        {
            ArgumentSyntax argument = arguments[position];

            if (!argument.Expression.IsKind(SyntaxKind.StringLiteralExpression) || argument.NameColon is not null || !argument.RefKindKeyword.IsKind(SyntaxKind.None))
                continue;

            if (SlotTakesObject(bound, position) || SlotParameter(bound, position) is { } boundParameter && IsMarked(boundParameter, marker))
                continue;

            // The member group is bound only for a literal that does NOT reach an object slot: the one shape that can
            // have drifted (an fmt.Println argument, which does, never needs it).
            group ??= semanticModel.GetMemberGroup(invocation.Expression, cancellationToken);

            foreach (IMethodSymbol candidate in group.Value.OfType<IMethodSymbol>())
            {
                IMethodSymbol definition = (candidate.ReducedFrom ?? candidate).OriginalDefinition;

                if (SymbolEqualityComparer.Default.Equals(definition, boundDefinition) || definition.IsGenericMethod)
                    continue;

                if (!SlotTakesObject(candidate, position) || !Applicable(semanticModel, candidate, arguments, position, cancellationToken))
                    continue;

                diagnostics.Add(DiagnosticRecord.Create(GeneratorDiagnostics.LiteralOverloadDrift, argument.Expression, argument.Expression.ToString(), bound.ToDisplayString(), candidate.ToDisplayString()));
                break;
            }
        }
    }

    // Whether the method group of a call bound to method can hold another member: always for an extension method (the
    // group spans every extension of that name in scope), and for any other method when its type or a base type or
    // interface declares another member of the same name. Go has one callee per name, so a converted callee usually
    // answers false here and the group is never bound.
    private static bool MayHaveOverloads(IMethodSymbol method)
    {
        if (method.IsExtensionMethod)
            return true;

        for (INamedTypeSymbol? type = method.ContainingType; type is not null; type = type.BaseType)
        {
            if (type.GetMembers(method.Name).Length > (SymbolEqualityComparer.Default.Equals(type, method.ContainingType) ? 1 : 0))
                return true;
        }

        return method.ContainingType.AllInterfaces.Any(iface => !iface.GetMembers(method.Name).IsEmpty);
    }

    // The parameter a call-site argument position reaches in method (the trailing params parameter for any position at
    // or past it), or null past the end.
    private static IParameterSymbol? SlotParameter(IMethodSymbol method, int position)
    {
        ImmutableArray<IParameterSymbol> parameters = method.Parameters;

        if (parameters.Length == 0)
            return null;

        if (position >= parameters.Length - 1 && parameters[parameters.Length - 1].IsParams)
            return parameters[parameters.Length - 1];

        return position < parameters.Length ? parameters[position] : null;
    }

    // Whether method's parameter at a call-site argument position (method in its call-site form) takes an object slot:
    // an object parameter, or an element of a trailing params array or span of object.
    private static bool SlotTakesObject(IMethodSymbol method, int position) => SlotParameter(method, position) switch
    {
        { IsParams: true } parameter => IsObjectSequence(parameter.Type),
        { } parameter => parameter.Type.SpecialType == SpecialType.System_Object,
        null => false
    };

    private static bool IsObjectSequence(ITypeSymbol type) => type switch
    {
        IArrayTypeSymbol array => array.ElementType.SpecialType == SpecialType.System_Object,
        INamedTypeSymbol { IsGenericType: true, TypeArguments: [{ SpecialType: SpecialType.System_Object }] } named =>
            named.ConstructedFrom.ToDisplayString() is "System.Span<T>" or "System.ReadOnlySpan<T>",
        _ => false
    };

    // Whether candidate (as the member group reports it: reduced when it is an extension called through a receiver)
    // could take this call's arguments: the count fits, and every argument other than the literal at position
    // converts implicitly to its parameter.
    private static bool Applicable(SemanticModel semanticModel, IMethodSymbol candidate, SeparatedSyntaxList<ArgumentSyntax> arguments, int position, CancellationToken cancellationToken)
    {
        ImmutableArray<IParameterSymbol> parameters = candidate.Parameters;
        bool hasParams = parameters.Length > 0 && parameters[parameters.Length - 1].IsParams;

        if (arguments.Count > parameters.Length && !hasParams)
            return false;

        if (arguments.Count < parameters.Count(parameter => !parameter.IsOptional && !parameter.IsParams))
            return false;

        for (int i = 0; i < arguments.Count; i++)
        {
            if (i == position)
                continue;

            ITypeSymbol? argumentType = semanticModel.GetTypeInfo(arguments[i].Expression, cancellationToken).Type;

            if (argumentType is null)
                return false;

            ITypeSymbol parameterType = i < parameters.Length - (hasParams ? 1 : 0) || !hasParams
                ? parameters[Math.Min(i, parameters.Length - 1)].Type
                : ElementType(parameters[parameters.Length - 1].Type);

            if (!semanticModel.Compilation.ClassifyCommonConversion(argumentType, parameterType).IsImplicit)
                return false;
        }

        return true;
    }

    private static ITypeSymbol ElementType(ITypeSymbol type) => type switch
    {
        IArrayTypeSymbol array => array.ElementType,
        INamedTypeSymbol { IsGenericType: true, TypeArguments: [var element] } => element,
        _ => type
    };

    private static bool IsMarked(IParameterSymbol parameter, INamedTypeSymbol? marker) =>
        marker is not null &&
        parameter.OriginalDefinition.GetAttributes().Any(attribute => SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, marker));

    // The generated file: one file-local cache of pre-boxed Go strings (one per distinct literal, by first use) and one
    // file-local class of interceptors (one per method and slot pattern, sharing its InterceptsLocation attributes).
    private static string Emit(List<(InterceptableLocation location, IMethodSymbol method, List<Slot> slots)> sites)
    {
        Dictionary<string, string> cacheFields = new(StringComparer.Ordinal);
        StringBuilder cache = new();

        foreach ((_, _, List<Slot> slots) in sites)
        {
            foreach (Slot slot in slots)
            {
                if (cacheFields.ContainsKey(slot.Value))
                    continue;

                string field = "ˢ" + cacheFields.Count.ToString(CultureInfo.InvariantCulture);
                cacheFields[slot.Value] = field;
                cache.Append($"    internal static readonly object {field} = (global::go.@string){SymbolDisplay.FormatLiteral(slot.Value, quote: true)}u8;\r\n");
            }
        }

        // Group sites by (method, slot pattern): one interceptor body per group.
        Dictionary<string, (IMethodSymbol method, List<Slot> slots, List<InterceptableLocation> locations)> groups = new(StringComparer.Ordinal);
        List<string> order = [];

        foreach ((InterceptableLocation location, IMethodSymbol method, List<Slot> slots) in sites)
        {
            string key = method.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) + "|" +
                         method.Parameters.Length.ToString(CultureInfo.InvariantCulture) + "|" +
                         string.Join(",", slots.Select(slot => $"{slot.Ordinal}:{slot.Element}:{cacheFields[slot.Value]}"));

            if (!groups.TryGetValue(key, out var group))
            {
                group = (method, slots, []);
                groups[key] = group;
                order.Add(key);
            }

            group.locations.Add(location);
        }

        StringBuilder source = new();

        source.Append("// <auto-generated/>\r\n");
        source.Append("#nullable disable\r\n\r\n");
        source.Append("namespace System.Runtime.CompilerServices\r\n{\r\n");
        source.Append("    [global::System.AttributeUsage(global::System.AttributeTargets.Method, AllowMultiple = true)]\r\n");
        source.Append("    file sealed class InterceptsLocationAttribute : global::System.Attribute\r\n    {\r\n");
        source.Append("        public InterceptsLocationAttribute(int version, string data) { }\r\n    }\r\n}\r\n\r\n");
        source.Append("namespace go2cs.Interceptors\r\n{\r\n");
        source.Append("file static class ˢLiterals\r\n{\r\n");
        source.Append(cache);
        source.Append("}\r\n\r\n");
        source.Append("file static class ˢInterceptors\r\n{\r\n");

        int ordinal = 0;

        foreach (string key in order)
        {
            (IMethodSymbol method, List<Slot> slots, List<InterceptableLocation> locations) = groups[key];

            if (ordinal > 0)
                source.Append("\r\n");

            foreach (InterceptableLocation location in locations)
                source.Append($"    [global::System.Runtime.CompilerServices.InterceptsLocation({location.Version}, \"{location.Data}\")]\r\n");

            source.Append($"    [{GeneratedCodeAttribute}, global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough]\r\n");
            source.Append($"    public static {ReturnType(method)} ˢI{ordinal.ToString(CultureInfo.InvariantCulture)}({Parameters(method)})\r\n    {{\r\n");

            foreach (Slot slot in slots)
            {
                string name = ParameterName(method.Parameters[slot.Ordinal]);
                string target = slot.Element < 0 ? name : $"{name}[{slot.Element.ToString(CultureInfo.InvariantCulture)}]";
                source.Append($"        {target} = ˢLiterals.{cacheFields[slot.Value]};\r\n");
            }

            source.Append($"        {(method.ReturnsVoid ? "" : "return ")}{Forward(method)};\r\n    }}\r\n");
            ordinal++;
        }

        source.Append("}\r\n}\r\n");

        return source.ToString();
    }

    private static string ReturnType(IMethodSymbol method) =>
        method.ReturnsVoid ? "void" : method.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

    // The interceptor's parameters: the target's own, with their ref kinds; an instance method's receiver first, by
    // reference for a struct (`in` for a readonly member), as the interceptor signature rule requires.
    private static string Parameters(IMethodSymbol method)
    {
        List<string> parameters = [];

        if (!method.IsStatic)
        {
            INamedTypeSymbol receiver = method.ContainingType;
            string byRef = receiver.IsValueType ? method.IsReadOnly || receiver.IsReadOnly ? "in " : "ref " : "";
            parameters.Add($"this {byRef}{receiver.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} ˢreceiver");
        }

        foreach (IParameterSymbol parameter in method.Parameters)
        {
            string modifiers = (method.IsExtensionMethod && parameter.Ordinal == 0 ? "this " : "") +
                               (parameter.IsParams ? "params " : "") +
                               RefKindText(parameter.RefKind);

            parameters.Add($"{modifiers}{parameter.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} {ParameterName(parameter)}");
        }

        return string.Join(", ", parameters);
    }

    private static string Forward(IMethodSymbol method)
    {
        IEnumerable<string> arguments = method.Parameters.Select(parameter => ArgumentRefKindText(parameter.RefKind) + ParameterName(parameter));
        string callee = method.IsStatic
            ? $"{method.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}.{EscapeIdentifier(method.Name)}"
            : $"ˢreceiver.{EscapeIdentifier(method.Name)}";

        return $"{callee}({string.Join(", ", arguments)})";
    }

    private static string RefKindText(RefKind refKind) => refKind switch
    {
        RefKind.Ref => "ref ",
        RefKind.Out => "out ",
        RefKind.In => "in ",
        RefKind.RefReadOnlyParameter => "ref readonly ",
        _ => ""
    };

    private static string ArgumentRefKindText(RefKind refKind) => refKind switch
    {
        RefKind.Ref or RefKind.RefReadOnlyParameter => "ref ",
        RefKind.Out => "out ",
        RefKind.In => "in ",
        _ => ""
    };

    private static string ParameterName(IParameterSymbol parameter) => EscapeIdentifier(parameter.Name);

    private static string EscapeIdentifier(string name) =>
        SyntaxFacts.GetKeywordKind(name) != SyntaxKind.None ? "@" + name : name;

    // A stable, unique hint name per source file: the file's name and a hash of its full path, so two files of one
    // name in different directories do not collide.
    private static string HintName(string filePath)
    {
        uint hash = 2166136261;

        foreach (char c in filePath)
        {
            hash ^= c;
            hash *= 16777619;
        }

        string name = Path.GetFileNameWithoutExtension(filePath);

        if (name.Length == 0)
            name = "source";

        return $"{name}.{hash.ToString("x8", CultureInfo.InvariantCulture)}.Interceptors.g.cs";
    }

    // The per-file pipeline value, equal by content (the diagnostics element by element), so an unchanged file's
    // output is cached.
    private sealed class FileOutput(string hintName, string source, ImmutableArray<DiagnosticRecord> diagnostics) : IEquatable<FileOutput>
    {
        public string HintName { get; } = hintName;

        public string Source { get; } = source;

        public ImmutableArray<DiagnosticRecord> Diagnostics { get; } = diagnostics;

        public bool Equals(FileOutput? other) =>
            other is not null && HintName == other.HintName && Source == other.Source && Diagnostics.SequenceEqual(other.Diagnostics);

        public override bool Equals(object? obj) => Equals(obj as FileOutput);

        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(HintName) ^ StringComparer.Ordinal.GetHashCode(Source) ^ Diagnostics.Length;
    }

    // A diagnostic as plain data (a Location holds a syntax tree, which the incremental cache must not retain). The
    // message arguments travel as one string, so the record is equal by value.
    private sealed record DiagnosticRecord(DiagnosticDescriptor Descriptor, string FilePath, TextSpan Span, LinePositionSpan LineSpan, string Arguments)
    {
        private const char Separator = '\u0001';

        public static DiagnosticRecord Create(DiagnosticDescriptor descriptor, SyntaxNode node, params string[] arguments)
        {
            Location location = node.GetLocation();

            return new DiagnosticRecord(descriptor, node.SyntaxTree.FilePath, location.SourceSpan, location.GetLineSpan().Span, string.Join(Separator.ToString(), arguments));
        }

        public Diagnostic ToDiagnostic() =>
            Diagnostic.Create(Descriptor, Location.Create(FilePath, Span, LineSpan), Arguments.Split(Separator).Cast<object>().ToArray());
    }
}
