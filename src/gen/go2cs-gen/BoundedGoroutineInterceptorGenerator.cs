// BoundedGoroutineInterceptorGenerator.cs - Gbtc
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
using Microsoft.CodeAnalysis.Text;
using static go2cs.Common;

namespace go2cs;

/// <summary>
/// Starts a <c>go</c> statement the converter proved cannot recurse on golib's bounded stack reservation: for every
/// <c>[assembly: GoBoundedGoroutine(file, line, column)]</c> record, an interceptor of the <c>goǃ</c> call starting at that
/// position forwards it to the same rung's <c>GoStack</c>-marked twin.
/// </summary>
/// <remarks>
/// <para>
/// A goroutine runs on a thread of its own, whose stack is a fixed reservation that ends the process on overflow, so golib
/// reserves a Go-scale stack by default. The converter proves, per <c>go</c> statement, that some goroutines cannot recurse
/// (src/go2cs/goroutineStackAnalysis.go), and those start on a much smaller reservation, which is what lets many of them
/// park at once. The statement itself is emitted as any other, <c>goǃ(f, x)</c>, so it reads like the Go <c>go f(x)</c>;
/// the mark is the record in the package's info file, and this generator applies it.
/// </para>
/// <para>
/// The failure that must not happen is an UNPROVED goroutine on the small reservation: a .NET stack overflow cannot be
/// caught. So every record must land exactly: on the first token of an invocation of an unmarked <c>goǃ</c> rung of
/// <c>go.builtin</c>, in a file of this compilation, with an interceptable location. Anything else (a stale record whose
/// line now holds another statement, a file of that name absent or present twice, a call of another method or of a
/// marked rung) is GO2CS0010, an error at the record, never a skipped record. A <c>goǃ</c> call no record names is left
/// alone and keeps the default reservation, as does every launch in a compilation this generator does not run on.
/// </para>
/// <para>
/// The records are found through their attribute's syntax index (ForAttributeWithMetadataName), so a compilation with
/// none pays nothing beyond that index. One interceptor per rung carries every location of that rung; a generic rung
/// gets a generic interceptor with the rung's own type parameters, so no argument's type has to be nameable here. An
/// interceptor is a real frame: it carries <c>[GeneratedCode("go2cs-gen")]</c>, <c>[StackTraceHidden]</c> and
/// <c>[DebuggerStepThrough]</c>, it lives in namespace <c>go2cs.Interceptors</c>, which go.lib.targets admits
/// (<c>InterceptorsNamespaces</c>), and golib's goroutine creator walk skips that namespace, so the goroutine's
/// <c>created by</c> line still names the function holding the <c>go</c> statement.
/// </para>
/// </remarks>
[Generator]
public sealed class BoundedGoroutineInterceptorGenerator : IIncrementalGenerator
{
    private const string GoBoundedGoroutineAttributeName = "go.GoBoundedGoroutineAttribute";

    private const string LauncherName = "goǃ";

    /// <summary>The tracking name of the transform, which GenTests counts.</summary>
    public const string TransformStepName = "BoundedGoroutineTransform";

    /// <summary>The generated file's hint name.</summary>
    public const string HintName = "BoundedGoroutines.Interceptors.g.cs";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValueProvider<ImmutableArray<Record>> records = context.SyntaxProvider.ForAttributeWithMetadataName(
                GoBoundedGoroutineAttributeName,
                static (node, _) => node is CompilationUnitSyntax,
                static (attributeContext, _) => ReadRecords(attributeContext.Attributes))
            .SelectMany(static (fileRecords, _) => fileRecords)
            .Collect();

        IncrementalValueProvider<Output> output = records
            .Combine(context.CompilationProvider)
            .Select(static (input, cancellationToken) => Transform(input.Left, input.Right, cancellationToken))
            .WithTrackingName(TransformStepName);

        context.RegisterSourceOutput(output, static (productionContext, generated) =>
        {
            foreach (DiagnosticRecord record in generated.Diagnostics)
                productionContext.ReportDiagnostic(record.ToDiagnostic());

            if (generated.Source.Length > 0)
                productionContext.AddSource(HintName, generated.Source);
        });
    }

    // One record as plain data: the position it names and where the record itself is written, for its diagnostic.
    private sealed record Record(string File, int Line, int Column, string RecordPath, TextSpan RecordSpan, LinePositionSpan RecordLineSpan);

    private static ImmutableArray<Record> ReadRecords(ImmutableArray<AttributeData> attributes)
    {
        ImmutableArray<Record>.Builder records = ImmutableArray.CreateBuilder<Record>();

        foreach (AttributeData attribute in attributes)
        {
            if (attribute.ApplicationSyntaxReference?.GetSyntax() is not { } syntax)
                continue;

            Location location = syntax.GetLocation();
            ImmutableArray<TypedConstant> arguments = attribute.ConstructorArguments;

            string file = arguments.Length > 0 && arguments[0].Value is string text ? text : string.Empty;
            int line = arguments.Length > 1 && arguments[1].Value is int lineValue ? lineValue : 0;
            int column = arguments.Length > 2 && arguments[2].Value is int columnValue ? columnValue : 0;

            records.Add(new Record(file, line, column, syntax.SyntaxTree.FilePath, location.SourceSpan, location.GetLineSpan().Span));
        }

        return records.ToImmutable();
    }

    private static Output Transform(ImmutableArray<Record> records, Compilation compilation, CancellationToken cancellationToken)
    {
        if (records.IsDefaultOrEmpty)
            return Output.Empty;

        List<DiagnosticRecord> diagnostics = [];
        INamedTypeSymbol? builtin = compilation.GetTypeByMetadataName("go.builtin");
        INamedTypeSymbol? goStack = compilation.GetTypeByMetadataName("go.GoStack");

        if (builtin is null || goStack is null)
        {
            foreach (Record record in records)
                diagnostics.Add(DiagnosticRecord.Create(record, "names a goǃ call, but this compilation references no golib with GoStack-marked goǃ rungs"));

            return new Output(string.Empty, [.. diagnostics]);
        }

        Dictionary<string, List<SyntaxTree>> treesByName = new(StringComparer.OrdinalIgnoreCase);

        foreach (SyntaxTree tree in compilation.SyntaxTrees)
        {
            string name = Path.GetFileName(tree.FilePath ?? string.Empty);

            if (!treesByName.TryGetValue(name, out List<SyntaxTree>? trees))
                treesByName[name] = trees = [];

            trees.Add(tree);
        }

        // Rungs in first-use order, each with its locations.
        Dictionary<IMethodSymbol, List<InterceptableLocation>> rungs = new(SymbolEqualityComparer.Default);
        List<IMethodSymbol> order = [];

        foreach (Record record in records.OrderBy(record => record.File, StringComparer.Ordinal).ThenBy(record => record.Line).ThenBy(record => record.Column))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!treesByName.TryGetValue(record.File, out List<SyntaxTree>? trees) || trees.Count == 0)
            {
                diagnostics.Add(DiagnosticRecord.Create(record, "names no file of this compilation"));
                continue;
            }

            if (trees.Count > 1)
            {
                diagnostics.Add(DiagnosticRecord.Create(record, $"names a file this compilation holds {trees.Count.ToString(CultureInfo.InvariantCulture)} of"));
                continue;
            }

            SyntaxTree source = trees[0];

            if (Launch(source, record, cancellationToken) is not { } invocation)
            {
                diagnostics.Add(DiagnosticRecord.Create(record, "does not land on the first token of a goǃ call"));
                continue;
            }

            SemanticModel semanticModel = compilation.GetSemanticModel(source);
            SymbolInfo symbolInfo = semanticModel.GetSymbolInfo(invocation, cancellationToken);
            IMethodSymbol? method = symbolInfo.Symbol as IMethodSymbol ?? (symbolInfo.CandidateSymbols is [IMethodSymbol only] ? only : null);
            string? undecided = null;

            // The call does not bind in THIS compilation when the goroutine's body or an argument names a member another
            // generator adds (a GoFieldAccess field property, a TypeGenerator member): generators all read the same input.
            // The rung the final compilation binds is then read from the call's own shape.
            if (method is null && symbolInfo.CandidateReason == CandidateReason.OverloadResolutionFailure)
                method = RungByShape(semanticModel, invocation, symbolInfo.CandidateSymbols, builtin, goStack, out undecided, cancellationToken);

            if (method is null)
            {
                diagnostics.Add(DiagnosticRecord.Create(record, undecided ?? $"lands on a goǃ call that binds no single method ({symbolInfo.CandidateReason}, {symbolInfo.CandidateSymbols.Length.ToString(CultureInfo.InvariantCulture)} candidates)"));
                continue;
            }

            IMethodSymbol rung = method.OriginalDefinition;

            if (!SymbolEqualityComparer.Default.Equals(rung.ContainingType, builtin) || rung.Name != LauncherName ||
                rung.Parameters.Any(parameter => SymbolEqualityComparer.Default.Equals(parameter.Type, goStack)) || !EndsWithCallerInfo(rung))
            {
                diagnostics.Add(DiagnosticRecord.Create(record, $"lands on a call of {method.ToDisplayString()}, not of an unmarked goǃ rung"));
                continue;
            }

            if (semanticModel.GetInterceptableLocation(invocation, cancellationToken) is not { } location)
            {
                diagnostics.Add(DiagnosticRecord.Create(record, "lands on a goǃ call with no interceptable location"));
                continue;
            }

            if (!rungs.TryGetValue(rung, out List<InterceptableLocation>? locations))
            {
                rungs[rung] = locations = [];
                order.Add(rung);
            }

            locations.Add(location);
        }

        return new Output(order.Count == 0 ? string.Empty : Emit(order, rungs), [.. diagnostics]);
    }

    // The goǃ invocation whose first token starts exactly at the record's position, or null.
    private static InvocationExpressionSyntax? Launch(SyntaxTree tree, Record record, CancellationToken cancellationToken)
    {
        SourceText text = tree.GetText(cancellationToken);

        if (record.Line < 1 || record.Line > text.Lines.Count || record.Column < 1)
            return null;

        TextLine line = text.Lines[record.Line - 1];

        if (record.Column - 1 >= line.Span.Length)
            return null;

        int position = line.Start + record.Column - 1;
        SyntaxToken token = tree.GetRoot(cancellationToken).FindToken(position);

        if (token.SpanStart != position || token.Parent is not IdentifierNameSyntax name || name.Identifier.ValueText != LauncherName)
            return null;

        return name.Parent is InvocationExpressionSyntax invocation && invocation.Expression == name ? invocation : null;
    }

    // The rung a goǃ call that does not bind here will bind in the final compilation, read from the call's shape. The
    // candidates narrow to the unmarked rungs taking the call's argument count, which leaves one, or a delegate rung and
    // its result-returning twin (Action<T…> and Func<T…, TResult>); C# binds the twin exactly when the goroutine's
    // function returns a value, which the delegate argument says without the member it is missing: a lambda's block body
    // returns a value or not, an expression body and a method group have a type or a method of their own. Anything else
    // stays undecided, and the record is GO2CS0010 rather than an interception of a guessed rung.
    private static IMethodSymbol? RungByShape(SemanticModel semanticModel, InvocationExpressionSyntax invocation, ImmutableArray<ISymbol> candidates,
        INamedTypeSymbol builtin, INamedTypeSymbol goStack, out string? undecided, CancellationToken cancellationToken)
    {
        int arguments = invocation.ArgumentList.Arguments.Count;

        IMethodSymbol[] rungs =
        [
            .. candidates.OfType<IMethodSymbol>()
                .Select(candidate => candidate.OriginalDefinition)
                .Where(rung => SymbolEqualityComparer.Default.Equals(rung.ContainingType, builtin) && rung.Name == LauncherName && EndsWithCallerInfo(rung) &&
                               !rung.Parameters.Any(parameter => SymbolEqualityComparer.Default.Equals(parameter.Type, goStack)) &&
                               rung.Parameters.Length - 2 == arguments)
                .Distinct<IMethodSymbol>(SymbolEqualityComparer.Default)
        ];

        undecided = null;

        if (rungs.Length == 1)
            return rungs[0];

        if (rungs.Length == 0 || arguments == 0)
        {
            undecided = $"lands on a goǃ call that binds none of the {rungs.Length.ToString(CultureInfo.InvariantCulture)} unmarked rungs taking its {arguments.ToString(CultureInfo.InvariantCulture)} arguments";
            return null;
        }

        bool? returnsValue = ReturnsValue(semanticModel, invocation.ArgumentList.Arguments[0].Expression, cancellationToken);

        if (returnsValue is null)
        {
            undecided = "lands on a goǃ call whose goroutine function cannot be read as returning a value or not here";
            return null;
        }

        // A lambda says how many parameters its delegate takes (Action against WaitCallback), and so does a method group
        // whose methods agree on it.
        ExpressionSyntax function = invocation.ArgumentList.Arguments[0].Expression;
        int? parameterCount = function switch
        {
            ParenthesizedLambdaExpressionSyntax lambda => lambda.ParameterList.Parameters.Count,
            SimpleLambdaExpressionSyntax => 1,
            IdentifierNameSyntax or MemberAccessExpressionSyntax or GenericNameSyntax => GroupParameterCount(semanticModel, function, cancellationToken),
            _ => null
        };

        IMethodSymbol[] matching =
        [
            .. rungs.Where(rung => rung.Parameters[0].Type is INamedTypeSymbol { DelegateInvokeMethod: { } invoke } &&
                                   invoke.ReturnsVoid != returnsValue.Value && (parameterCount is null || invoke.Parameters.Length == parameterCount))
        ];

        if (matching.Length == 1)
            return matching[0];

        undecided = $"lands on a goǃ call that {matching.Length.ToString(CultureInfo.InvariantCulture)} unmarked rungs could bind";
        return null;
    }

    // Whether a goroutine function (goǃ's delegate argument) returns a value, or null when this compilation cannot say.
    private static bool? ReturnsValue(SemanticModel semanticModel, ExpressionSyntax function, CancellationToken cancellationToken)
    {
        switch (function)
        {
            case AnonymousFunctionExpressionSyntax { Block: { } block }:
                // A block body returns a value when a return statement of its own (not a nested function's) has one.
                return block.DescendantNodes(node => node is not (AnonymousFunctionExpressionSyntax or LocalFunctionStatementSyntax))
                    .OfType<ReturnStatementSyntax>()
                    .Any(statement => statement.Expression is not null);

            case AnonymousFunctionExpressionSyntax { ExpressionBody: { } body }:
            {
                if (body is ThrowExpressionSyntax)
                    return false;

                ITypeSymbol? type = semanticModel.GetTypeInfo(body, cancellationToken).Type;

                if (type is not null && type.TypeKind != TypeKind.Error)
                    return type.SpecialType != SpecialType.System_Void;

                return MethodsReturnValue(semanticModel.GetSymbolInfo(body, cancellationToken)) ??
                       (body is InvocationExpressionSyntax call ? MethodsNamedReturnValue(semanticModel, call.Expression, cancellationToken) : null);
            }

            case IdentifierNameSyntax or MemberAccessExpressionSyntax or GenericNameSyntax:
                return MethodsReturnValue(semanticModel.GetSymbolInfo(function, cancellationToken)) ??
                       MethodsNamedReturnValue(semanticModel, function, cancellationToken);
        }

        return null;
    }

    // Whether the methods a method group's NAME can mean all return a value or all return none, for a group that does not
    // resolve here: its receiver or the overload it binds comes from a sibling generator (a pointer receiver's box
    // overload, `Ꮡt.loop`; a field property, `r.root.release`). The candidates are every method of that name the group can
    // reach in this compilation: its own member group, the receiver type's members and extension methods (and, through a
    // box `ж<T>`, T's), and the names in scope, where a package class's methods and their converted receiver forms live.
    // A generated overload of a converted method returns what that method returns, so agreement decides; any disagreement
    // or an empty set leaves it undecided.
    private static bool? MethodsNamedReturnValue(SemanticModel semanticModel, ExpressionSyntax group, CancellationToken cancellationToken)
    {
        List<IMethodSymbol> methods = MethodsNamed(semanticModel, group, cancellationToken);

        if (methods.Count == 0)
            return null;

        bool first = !methods[0].ReturnsVoid;

        return methods.All(method => !method.ReturnsVoid == first) ? first : null;
    }

    // How many parameters the delegate a method group converts to takes, when every method it can mean agrees: the
    // bound method's own, or the candidates of MethodsNamed, an extension method found unreduced counting its receiver
    // out when the group names it through one (`Ꮡt.loop`).
    private static int? GroupParameterCount(SemanticModel semanticModel, ExpressionSyntax group, CancellationToken cancellationToken)
    {
        if (semanticModel.GetSymbolInfo(group, cancellationToken).Symbol is IMethodSymbol bound)
            return bound.Parameters.Length;

        bool throughReceiver = group is MemberAccessExpressionSyntax;
        int[] counts =
        [
            .. MethodsNamed(semanticModel, group, cancellationToken)
                .Select(method => method.Parameters.Length - (throughReceiver && method.IsExtensionMethod && method.ReducedFrom is null ? 1 : 0))
                .Distinct()
        ];

        return counts is [var only] ? only : null;
    }

    // Every method a method group's name can mean in this compilation (see MethodsNamedReturnValue).
    private static List<IMethodSymbol> MethodsNamed(SemanticModel semanticModel, ExpressionSyntax group, CancellationToken cancellationToken)
    {
        SimpleNameSyntax? name = group switch
        {
            MemberAccessExpressionSyntax access => access.Name,
            SimpleNameSyntax simple => simple,
            _ => null
        };

        if (name is null)
            return [];

        string identifier = name.Identifier.ValueText;
        int position = group.SpanStart;
        List<IMethodSymbol> methods = [.. semanticModel.GetMemberGroup(group, cancellationToken).OfType<IMethodSymbol>()];

        if (group is MemberAccessExpressionSyntax { Expression: { } receiver } &&
            semanticModel.GetTypeInfo(receiver, cancellationToken).Type is INamedTypeSymbol { TypeKind: not TypeKind.Error } receiverType)
        {
            methods.AddRange(semanticModel.LookupSymbols(position, receiverType, identifier, includeReducedExtensionMethods: true).OfType<IMethodSymbol>());

            if (receiverType is { Name: "ж", TypeArguments: [INamedTypeSymbol { TypeKind: not TypeKind.Error } boxed] })
                methods.AddRange(semanticModel.LookupSymbols(position, boxed, identifier, includeReducedExtensionMethods: true).OfType<IMethodSymbol>());
        }

        methods.AddRange(semanticModel.LookupSymbols(position, name: identifier).OfType<IMethodSymbol>());

        return methods;
    }

    // Whether the method a call or a method group names returns a value, when every candidate agrees.
    private static bool? MethodsReturnValue(SymbolInfo symbolInfo)
    {
        IMethodSymbol[] methods = symbolInfo.Symbol is IMethodSymbol bound ? [bound] : [.. symbolInfo.CandidateSymbols.OfType<IMethodSymbol>()];

        if (methods.Length == 0)
            return null;

        bool first = !methods[0].ReturnsVoid;

        return methods.All(method => !method.ReturnsVoid == first) ? first : null;
    }

    // Every goǃ rung ends with the caller-information pair golib's creator cache keys on.
    private static bool EndsWithCallerInfo(IMethodSymbol rung) =>
        rung.Parameters.Length >= 3 &&
        rung.Parameters[rung.Parameters.Length - 2].Type.SpecialType == SpecialType.System_String &&
        rung.Parameters[rung.Parameters.Length - 1].Type.SpecialType == SpecialType.System_Int32;

    // The generated file: one interceptor per rung, carrying every location of that rung, forwarding to the rung's
    // GoStack-marked twin, whose parameters are the rung's with GoStack.Bounded before the caller information.
    private static string Emit(List<IMethodSymbol> order, Dictionary<IMethodSymbol, List<InterceptableLocation>> rungs)
    {
        StringBuilder source = new();

        source.Append("// <auto-generated/>\r\n");
        source.Append("#nullable disable\r\n\r\n");
        source.Append("namespace System.Runtime.CompilerServices\r\n{\r\n");
        source.Append("    [global::System.AttributeUsage(global::System.AttributeTargets.Method, AllowMultiple = true)]\r\n");
        source.Append("    file sealed class InterceptsLocationAttribute : global::System.Attribute\r\n    {\r\n");
        source.Append("        public InterceptsLocationAttribute(int version, string data) { }\r\n    }\r\n}\r\n\r\n");
        source.Append("namespace go2cs.Interceptors\r\n{\r\n");
        source.Append("file static class ˢBoundedGoroutines\r\n{\r\n");

        for (int ordinal = 0; ordinal < order.Count; ordinal++)
        {
            IMethodSymbol rung = order[ordinal];

            if (ordinal > 0)
                source.Append("\r\n");

            foreach (InterceptableLocation location in rungs[rung])
                source.Append($"    [global::System.Runtime.CompilerServices.InterceptsLocation({location.Version}, \"{location.Data}\")]\r\n");

            string typeParameters = rung.TypeParameters.Length == 0 ? string.Empty : "<" + string.Join(", ", rung.TypeParameters.Select(parameter => parameter.Name)) + ">";
            string parameters = string.Join(", ", rung.Parameters.Select(parameter => $"{parameter.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} {Escape(parameter.Name)}"));

            List<string> arguments = [.. rung.Parameters.Take(rung.Parameters.Length - 2).Select(parameter => Escape(parameter.Name))];
            arguments.Add("global::go.GoStack.Bounded");
            arguments.AddRange(rung.Parameters.Skip(rung.Parameters.Length - 2).Select(parameter => Escape(parameter.Name)));

            source.Append($"    [{GeneratedCodeAttribute}, global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough]\r\n");
            source.Append($"    public static void ˢB{ordinal.ToString(CultureInfo.InvariantCulture)}{typeParameters}({parameters}) =>\r\n");
            source.Append($"        global::go.builtin.{LauncherName}({string.Join(", ", arguments)});\r\n");
        }

        source.Append("}\r\n}\r\n");

        return source.ToString();
    }

    private static string Escape(string name) =>
        SyntaxFacts.GetKeywordKind(name) != SyntaxKind.None ? "@" + name : name;

    // The pipeline value, equal by content (the diagnostics element by element), so an unchanged compilation's output is
    // cached.
    private sealed class Output(string source, ImmutableArray<DiagnosticRecord> diagnostics) : IEquatable<Output>
    {
        public static readonly Output Empty = new(string.Empty, []);

        public string Source { get; } = source;

        public ImmutableArray<DiagnosticRecord> Diagnostics { get; } = diagnostics;

        public bool Equals(Output? other) =>
            other is not null && Source == other.Source && Diagnostics.SequenceEqual(other.Diagnostics);

        public override bool Equals(object? obj) => Equals(obj as Output);

        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Source) ^ Diagnostics.Length;
    }

    // A diagnostic as plain data, at the record (a Location holds a syntax tree, which the incremental cache must not retain).
    private sealed record DiagnosticRecord(string FilePath, TextSpan Span, LinePositionSpan LineSpan, string File, int Line, int Column, string Reason)
    {
        public static DiagnosticRecord Create(Record record, string reason) =>
            new(record.RecordPath, record.RecordSpan, record.RecordLineSpan, record.File, record.Line, record.Column, reason);

        public Diagnostic ToDiagnostic() =>
            Diagnostic.Create(GeneratorDiagnostics.BoundedGoroutineRecordMismatch, Location.Create(FilePath, Span, LineSpan),
                File, Line.ToString(CultureInfo.InvariantCulture), Column.ToString(CultureInfo.InvariantCulture), Reason);
    }
}
