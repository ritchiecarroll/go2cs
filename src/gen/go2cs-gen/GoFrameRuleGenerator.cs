// GoFrameRuleGenerator.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;

namespace go2cs;

/// <summary>
/// GO2CS0016: golib's <c>GoFrame</c>, a converted function's defer list, is never copied, stored or captured.
/// </summary>
/// <remarks>
/// <para>
/// <c>GoFrame</c> is a plain struct so that an M:N twin's state machine can hold it across an await, and so the compiler
/// no longer refuses a frame that escapes. Its defer slots hold delegates, so a copied frame runs its deferred calls twice
/// or loses them. The ref struct never refused that copy; this rule does (docs/phase4/DATA-mn-spike-2.md 9.8). A frame
/// may be a local initialised to <c>default</c>, a <c>ref</c> parameter or <c>ref</c> argument, or the receiver of its
/// own members. Everything else is refused: a by-value, <c>in</c>, <c>out</c> or <c>ref readonly</c> parameter, a
/// return, a field or property, an array element, a pointer target, a generic argument (written or inferred), a copy,
/// an assignment, boxing, a method group, its address, a ref local, and a use inside a lambda or local function that did
/// not declare it.
/// </para>
/// <para>
/// The rule is reported from a SOURCE GENERATOR, not an analyzer, so it still runs in a project that sets
/// <c>RunAnalyzers</c> to false, and its descriptor is <c>NotConfigurable</c> (10.6). It reads every file of the
/// compilation, converted or hand-written, that names <c>GoFrame</c>. A frame VALUE can reach a file that never names the
/// type in one way only: a typeless lambda parameter with a modifier, <c>(ref f) =&gt; ...</c>, whose delegate is
/// declared elsewhere, so such a file is read too. A <c>global using</c> alias of <c>GoFrame</c> would be the other way,
/// and it is refused at the alias. What the rule cannot see: code another generator writes (a generator never reads
/// another's output, so a twin body is checked only as the plain body it was rewritten from), and F# or Visual Basic.
/// </para>
/// </remarks>
[Generator]
public sealed class GoFrameRuleGenerator : IIncrementalGenerator
{
    private const string GoFrameMetadataName = "go.GoFrame";

    private const string GoFrameName = "GoFrame";

    /// <summary>The tracking name of the semantic transform.</summary>
    public const string TransformStepName = "GoFrameRuleTransform";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<FileRefusals> files = context.SyntaxProvider.CreateSyntaxProvider(
                static (node, _) => IsTransformInput(node),
                static (syntaxContext, _) => (CompilationUnitSyntax)syntaxContext.Node)
            .Combine(context.CompilationProvider)
            .Select(static (input, cancellationToken) => Check(input.Right, input.Left, cancellationToken))
            .WithTrackingName(TransformStepName)
            .Where(static output => output.Records.Length > 0);

        context.RegisterSourceOutput(files, static (productionContext, output) =>
        {
            foreach (DiagnosticRecord record in output.Records)
                productionContext.ReportDiagnostic(record.ToDiagnostic());
        });
    }

    /// <summary>
    /// The syntax provider's predicate: a file that names <c>GoFrame</c>, or that declares a typeless lambda parameter with
    /// a modifier. It reads tokens only, so a file it refuses pays no semantic query.
    /// </summary>
    public static bool IsTransformInput(SyntaxNode node)
    {
        if (node is not CompilationUnitSyntax root)
            return false;

        foreach (SyntaxToken token in root.DescendantTokens())
        {
            if (token.IsKind(SyntaxKind.IdentifierToken) && token.ValueText == GoFrameName)
                return true;

            if (token.Parent is ParameterSyntax { Type: null } parameter && parameter.Modifiers.Contains(token))
                return true;
        }

        return false;
    }

    private static FileRefusals Check(Compilation compilation, CompilationUnitSyntax root, CancellationToken cancellationToken)
    {
        INamedTypeSymbol? frame = compilation.GetTypeByMetadataName(GoFrameMetadataName);

        if (frame is null)
            return FileRefusals.Empty;

        return new Checker(compilation.GetSemanticModel(root.SyntaxTree), frame, cancellationToken).Run(root);
    }

    private sealed class Checker(SemanticModel model, INamedTypeSymbol frame, CancellationToken cancellationToken)
    {
        // The simple names that can spell the type in this file: GoFrame, and any file-local alias of it.
        private readonly HashSet<string> m_typeNames = [GoFrameName];

        // The names of the frame locals and parameters this file declares: the only names a frame VALUE can have here,
        // since a field, property or return that holds one is refused at its declaration.
        private readonly HashSet<string> m_valueNames = [];

        private readonly List<DiagnosticRecord> m_records = [];

        private readonly HashSet<(TextSpan, string)> m_reported = [];

        public FileRefusals Run(CompilationUnitSyntax root)
        {
            foreach (UsingDirectiveSyntax directive in root.DescendantNodes().OfType<UsingDirectiveSyntax>())
                CheckAlias(directive);

            List<SyntaxNode> nodes = [.. root.DescendantNodes()];

            foreach (SyntaxNode node in nodes)
            {
                cancellationToken.ThrowIfCancellationRequested();
                CheckDeclaration(node);
            }

            if (m_valueNames.Count > 0)
            {
                foreach (IdentifierNameSyntax identifier in nodes.OfType<IdentifierNameSyntax>())
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (m_valueNames.Contains(identifier.Identifier.ValueText))
                        CheckUse(identifier);
                }
            }

            return m_records.Count == 0 ? FileRefusals.Empty : new FileRefusals([.. m_records]);
        }

        private bool IsFrame(ITypeSymbol? type) => SymbolEqualityComparer.Default.Equals(type, frame);

        private bool MayNameFrame(TypeSyntax? type) => RightmostName(type) is { } name && m_typeNames.Contains(name);

        private static string? RightmostName(TypeSyntax? type) => type switch
        {
            IdentifierNameSyntax identifier => identifier.Identifier.ValueText,
            QualifiedNameSyntax qualified => qualified.Right.Identifier.ValueText,
            AliasQualifiedNameSyntax aliased => aliased.Name.Identifier.ValueText,
            RefTypeSyntax reference => RightmostName(reference.Type),
            ScopedTypeSyntax scoped => RightmostName(scoped.Type),
            _ => null
        };

        private void Report(SyntaxNode node, string use) => Report(node.GetLocation(), node.SyntaxTree.FilePath, use);

        private void Report(SyntaxToken token, string use) => Report(token.GetLocation(), token.SyntaxTree?.FilePath ?? string.Empty, use);

        private void Report(Location location, string filePath, string use)
        {
            if (m_reported.Add((location.SourceSpan, use)))
                m_records.Add(new DiagnosticRecord(filePath, location.SourceSpan, location.GetLineSpan().Span, use));
        }

        private void CheckAlias(UsingDirectiveSyntax directive)
        {
            if (directive.Alias is null || RightmostName(directive.NamespaceOrType) != GoFrameName)
                return;

            if (model.GetSymbolInfo(directive.NamespaceOrType, cancellationToken).Symbol is not ITypeSymbol type || !IsFrame(type))
                return;

            string alias = directive.Alias.Name.Identifier.ValueText;

            if (directive.GlobalKeyword.IsKind(SyntaxKind.GlobalKeyword))
                Report(directive, $"The global using alias '{alias}' names GoFrame, so a file of the project can hold a frame without naming the type");
            else
                m_typeNames.Add(alias);
        }

        private void CheckDeclaration(SyntaxNode node)
        {
            switch (node)
            {
                case TypeSyntax type when MayNameFrame(type) && ContainerUse(type) is { } container:
                    if (IsFrame(model.GetTypeInfo(type, cancellationToken).Type))
                        Report(type, $"A GoFrame is used as {container}");
                    break;

                case FieldDeclarationSyntax field when MayNameFrame(field.Declaration.Type):
                    foreach (VariableDeclaratorSyntax variable in field.Declaration.Variables)
                    {
                        if (model.GetDeclaredSymbol(variable, cancellationToken) is IFieldSymbol symbol && IsFrame(symbol.Type))
                            Report(variable.Identifier, $"The field '{symbol.Name}' stores a GoFrame");
                    }
                    break;

                case PropertyDeclarationSyntax property when MayNameFrame(property.Type):
                    if (model.GetDeclaredSymbol(property, cancellationToken) is { } propertySymbol && IsFrame(propertySymbol.Type))
                        Report(property.Identifier, $"The property '{propertySymbol.Name}' stores a GoFrame");
                    break;

                case IndexerDeclarationSyntax indexer when MayNameFrame(indexer.Type):
                    if (model.GetDeclaredSymbol(indexer, cancellationToken) is { } indexerSymbol && IsFrame(indexerSymbol.Type))
                        Report(indexer.ThisKeyword, "The indexer stores a GoFrame");
                    break;

                case MethodDeclarationSyntax method when MayNameFrame(method.ReturnType):
                    CheckReturn(model.GetDeclaredSymbol(method, cancellationToken), method.Identifier);
                    break;

                case LocalFunctionStatementSyntax function when MayNameFrame(function.ReturnType):
                    CheckReturn(model.GetDeclaredSymbol(function, cancellationToken) as IMethodSymbol, function.Identifier);
                    break;

                case OperatorDeclarationSyntax @operator when MayNameFrame(@operator.ReturnType):
                    CheckReturn(model.GetDeclaredSymbol(@operator, cancellationToken), @operator.OperatorToken);
                    break;

                case ConversionOperatorDeclarationSyntax conversion when MayNameFrame(conversion.Type):
                    CheckReturn(model.GetDeclaredSymbol(conversion, cancellationToken), conversion.Type.GetFirstToken());
                    break;

                case DelegateDeclarationSyntax @delegate when MayNameFrame(@delegate.ReturnType):
                    CheckReturn(model.GetDeclaredSymbol(@delegate, cancellationToken)?.DelegateInvokeMethod, @delegate.Identifier);
                    break;

                case ParenthesizedLambdaExpressionSyntax { ReturnType: { } returnType } lambda when MayNameFrame(returnType):
                    CheckReturn(model.GetSymbolInfo(lambda, cancellationToken).Symbol as IMethodSymbol, returnType.GetFirstToken());
                    break;

                case ParameterSyntax parameter when parameter.Type is null || MayNameFrame(parameter.Type):
                    CheckParameter(parameter);
                    break;

                case VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax declaration } declarator
                    when declaration.Parent is not FieldDeclarationSyntax and not EventFieldDeclarationSyntax &&
                         (declaration.Type.IsVar || MayNameFrame(declaration.Type)):
                    CheckLocal(declarator);
                    break;

                case SingleVariableDesignationSyntax designation:
                    if (model.GetDeclaredSymbol(designation, cancellationToken) is ILocalSymbol designated && IsFrame(designated.Type))
                    {
                        m_valueNames.Add(designated.Name);
                        Report(designation, $"The local '{designated.Name}' is a pattern, out or deconstruction variable that copies a GoFrame");
                    }
                    break;

                case ForEachStatementSyntax forEach:
                    if (model.GetDeclaredSymbol(forEach, cancellationToken) is { } iteration && IsFrame(iteration.Type))
                    {
                        m_valueNames.Add(iteration.Name);
                        Report(forEach.Identifier, $"The foreach variable '{iteration.Name}' copies a GoFrame");
                    }
                    break;
            }
        }

        // What a type in this position makes the frame, or null where the position is not a container.
        private static string? ContainerUse(TypeSyntax type) => type.Parent switch
        {
            TypeArgumentListSyntax => "a generic argument",
            ArrayTypeSyntax array when array.ElementType == type => "an array element",
            NullableTypeSyntax => "a generic argument (Nullable<GoFrame>)",
            PointerTypeSyntax => "a pointer target",
            TupleElementSyntax => "a tuple element",
            _ => null
        };

        private void CheckReturn(IMethodSymbol? method, SyntaxToken site)
        {
            if (method is not null && IsFrame(method.ReturnType))
                Report(site, $"'{(method.MethodKind == MethodKind.AnonymousFunction ? "the lambda" : method.Name)}' returns a GoFrame");
        }

        private void CheckParameter(ParameterSyntax parameter)
        {
            if (model.GetDeclaredSymbol(parameter, cancellationToken) is not { } symbol || !IsFrame(symbol.Type))
                return;

            m_valueNames.Add(symbol.Name);

            string? how = symbol.RefKind switch
            {
                RefKind.Ref => null,
                RefKind.None => "by value",
                RefKind.Out => "as 'out'",
                RefKind.In => "as 'in'",
                _ => "as 'ref readonly'"
            };

            if (how is not null)
                Report(parameter.Identifier, $"The parameter '{symbol.Name}' takes a GoFrame {how}");
        }

        private void CheckLocal(VariableDeclaratorSyntax declarator)
        {
            if (model.GetDeclaredSymbol(declarator, cancellationToken) is not ILocalSymbol local || !IsFrame(local.Type))
                return;

            m_valueNames.Add(local.Name);

            if (local.IsRef)
            {
                Report(declarator.Identifier, $"The local '{local.Name}' is a ref local");
                return;
            }

            ExpressionSyntax? value = declarator.Initializer?.Value;

            if (value is LiteralExpressionSyntax { RawKind: (int)SyntaxKind.DefaultLiteralExpression } or DefaultExpressionSyntax)
                return;

            // `var copy = ᒐ;` is reported once, as the copy, by the use check.
            if (value is IdentifierNameSyntax source && model.GetSymbolInfo(source, cancellationToken).Symbol is ILocalSymbol or IParameterSymbol)
                return;

            Report(declarator.Identifier, $"The local '{local.Name}' is not initialised to default");
        }

        private void CheckUse(IdentifierNameSyntax identifier)
        {
            ISymbol? symbol = model.GetSymbolInfo(identifier, cancellationToken).Symbol;

            ITypeSymbol? type = symbol switch
            {
                ILocalSymbol local => local.Type,
                IParameterSymbol parameter => parameter.Type,
                _ => null
            };

            if (!IsFrame(type) || model.GetOperation(identifier, cancellationToken) is not { } operation)
                return;

            ISymbol? enclosing = model.GetEnclosingSymbol(identifier.SpanStart, cancellationToken);

            if (operation.Parent is not INameOfOperation && !SymbolEqualityComparer.Default.Equals(enclosing, symbol!.ContainingSymbol))
            {
                Report(identifier, $"The frame '{symbol.Name}' is captured by a lambda or local function");
                return;
            }

            string? use = operation.Parent switch
            {
                IInvocationOperation invocation when invocation.Instance == operation => null,
                IPropertyReferenceOperation property when property.Instance == operation => null,
                IFieldReferenceOperation field when field.Instance == operation => null,
                INameOfOperation => null,
                IArgumentOperation { Parent: IInvocationOperation invocation } when invocation.TargetMethod.TypeArguments.Any(IsFrame) => "is passed as a generic argument",
                IArgumentOperation { Parent: IObjectCreationOperation { Type: INamedTypeSymbol created } } when created.TypeArguments.Any(IsFrame) => "is passed as a generic argument",
                IArgumentOperation argument when argument.Parameter?.RefKind == RefKind.Ref => null,
                IArgumentOperation => "is passed by value",
                ISimpleAssignmentOperation assignment when assignment.Target == operation => "is assigned",
                IConversionOperation conversion when !IsFrame(conversion.Type) => "is boxed",
                IReturnOperation => "is returned",
                IAddressOfOperation => "has its address taken",
                IMethodReferenceOperation method when method.Instance == operation => "gives a method group, whose delegate holds a copy",
                IVariableInitializerOperation { Parent: IVariableDeclaratorOperation { Symbol.IsRef: true } } => null,
                _ => "is copied"
            };

            if (use is not null)
                Report(identifier, $"The frame '{symbol!.Name}' {use}");
        }
    }

    // One file's refusals, equal by value so the incremental pipeline re-reports only a file whose refusals changed.
    private sealed class FileRefusals(ImmutableArray<DiagnosticRecord> records) : IEquatable<FileRefusals>
    {
        public static readonly FileRefusals Empty = new([]);

        public ImmutableArray<DiagnosticRecord> Records { get; } = records;

        public bool Equals(FileRefusals? other) => other is not null && Records.SequenceEqual(other.Records);

        public override bool Equals(object? obj) => Equals(obj as FileRefusals);

        public override int GetHashCode() => Records.Length;
    }

    // A diagnostic as plain data (a Location holds a syntax tree, which the incremental cache must not retain).
    private sealed record DiagnosticRecord(string FilePath, TextSpan Span, LinePositionSpan LineSpan, string Use)
    {
        public Diagnostic ToDiagnostic() =>
            Diagnostic.Create(GeneratorDiagnostics.GoFrameEscapes, Location.Create(FilePath, Span, LineSpan), Use);
    }
}
