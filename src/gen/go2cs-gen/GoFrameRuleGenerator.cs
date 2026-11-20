// GoFrameRuleGenerator.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text;
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
/// and it is refused at the alias.
/// </para>
/// <para>
/// SYNTAX FIRST. A generator binds the compilation WITHOUT the other generators' output, and converted code names their
/// members (a field box, <c>g.ᏑcgoCtxt</c>), so a call that is sound can fail to bind here. The three allowed shapes are
/// positions, so they are read from syntax: a <c>ref</c> argument, the receiver of a member access, and <c>nameof</c>.
/// Binding only narrows them, to a refusal, where it binds: a <c>ref</c> argument that infers a generic argument, a
/// receiver that is a by-value extension's argument or a method group. A use in any other position is refused, named
/// from binding where it binds and as a copy where it does not. An allowed-by-syntax use whose call does not bind is
/// DEFERRED: the generator lists it in a comment-only generated file (<see cref="DeferredHintPrefix"/>), and
/// <see cref="GoFrameRuleAnalyzer"/>, which sees every generator's output, binds it fully and reports it if it refuses.
/// A project that sets <c>RunAnalyzers</c> to false loses that second net, not the rule.
/// </para>
/// <para>
/// What the rule does not see: code another generator writes (a twin body is TwinGenerator's rewrite of a plain body this
/// rule checked, and a frame hoisted into a twin's state machine is 9.8's sanctioned home for it across an await), and
/// F# or Visual Basic.
/// </para>
/// </remarks>
[Generator]
public sealed class GoFrameRuleGenerator : IIncrementalGenerator
{
    internal const string GoFrameMetadataName = "go.GoFrame";

    private const string GoFrameName = "GoFrame";

    /// <summary>The hint-name prefix of the comment-only files that list the uses the generator could not bind.</summary>
    public const string DeferredHintPrefix = "GoFrameRule.Deferred.";

    // The prefix of each listed use in such a file: `// GO2CS0016-deferred <start> <length> <key>`.
    internal const string DeferredLinePrefix = "// GO2CS0016-deferred ";

    /// <summary>
    /// A source file's key in the deferred lists and their hint names: its file name's letters and digits and an FNV-1a
    /// hash of its full path. The path itself is never written: with EmitCompilerGeneratedFiles the lists land on disk.
    /// </summary>
    internal static string DeferredKey(string filePath)
    {
        uint hash = 2166136261;

        foreach (char c in filePath)
            hash = (hash ^ c) * 16777619;

        string name = new([.. System.IO.Path.GetFileNameWithoutExtension(filePath).Where(char.IsLetterOrDigit)]);
        return name + "." + hash.ToString("x8", CultureInfo.InvariantCulture);
    }

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
            .Where(static output => output.Records.Length > 0 || output.Deferred.Length > 0);

        context.RegisterSourceOutput(files, static (productionContext, output) =>
        {
            foreach (DiagnosticRecord record in output.Records)
                productionContext.ReportDiagnostic(record.ToDiagnostic());

            if (output.Deferred.Length > 0)
                productionContext.AddGeneratedSource(output.DeferredHintName, output.DeferredSource());
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

    // What binding says of one use of a frame: Known is false where the use's operation is missing or sits under an
    // invalid operation (the generator's partial view); Use is null where binding allows it, else what it does.
    internal readonly record struct Bound(bool Known, string? Use)
    {
        public static readonly Bound Unknown = new(false, null);
    }

    /// <summary>
    /// The syntax half of the rule: whether <paramref name="identifier"/>, a use of a frame, stands where 9.8 allows one.
    /// </summary>
    internal static bool IsAllowedPosition(IdentifierNameSyntax identifier) => identifier.Parent switch
    {
        ArgumentSyntax argument => argument.RefKindKeyword.IsKind(SyntaxKind.RefKeyword) || IsNameOfArgument(argument),
        MemberAccessExpressionSyntax access => access.Expression == identifier,
        _ => false
    };

    private static bool IsNameOfArgument(ArgumentSyntax argument) =>
        argument.Parent is ArgumentListSyntax { Parent: InvocationExpressionSyntax { Expression: IdentifierNameSyntax { Identifier.ValueText: "nameof" } } };

    /// <summary>
    /// The binding half of the rule, shared with <see cref="GoFrameRuleAnalyzer"/>: what <paramref name="identifier"/>, a
    /// use of a frame, does by its operation.
    /// </summary>
    internal static Bound Classify(SemanticModel model, IdentifierNameSyntax identifier, INamedTypeSymbol frame, CancellationToken cancellationToken)
    {
        if (model.GetOperation(identifier, cancellationToken) is not { Parent: { } parent } operation)
            return Bound.Unknown;

        // A use binds or not by the operations up to its own lambda or local function: an invalid call ABOVE that body
        // (a lambda passed to a call that names a generated member) says nothing of how the body's own statements bind.
        for (IOperation? ancestor = parent; ancestor is not null and not IAnonymousFunctionOperation and not ILocalFunctionOperation; ancestor = ancestor.Parent)
        {
            if (ancestor is IInvalidOperation)
                return Bound.Unknown;
        }

        bool IsFrame(ITypeSymbol? type) => SymbolEqualityComparer.Default.Equals(type, frame);

        // A method's own type arguments, and those of every type it is a member of (`RefAction<GoFrame>.Invoke`).
        bool InfersFrame(IMethodSymbol method)
        {
            if (method.TypeArguments.Any(IsFrame))
                return true;

            for (INamedTypeSymbol? type = method.ContainingType; type is not null; type = type.ContainingType)
            {
                if (type.TypeArguments.Any(IsFrame))
                    return true;
            }

            return false;
        }

        return new Bound(true, parent switch
        {
            IInvocationOperation invocation when invocation.Instance == operation => null,
            IPropertyReferenceOperation property when property.Instance == operation => null,
            IFieldReferenceOperation field when field.Instance == operation => null,
            INameOfOperation => null,
            IArgumentOperation { Parent: IInvocationOperation invocation } when InfersFrame(invocation.TargetMethod) => "is passed as a generic argument",
            IArgumentOperation { Parent: IObjectCreationOperation { Constructor: { } constructor } } when InfersFrame(constructor) => "is passed as a generic argument",
            IArgumentOperation argument when argument.Parameter?.RefKind == RefKind.Ref => null,
            IArgumentOperation => "is passed by value",
            ISimpleAssignmentOperation assignment when assignment.Target == operation => "is assigned",
            IConversionOperation conversion when !IsFrame(conversion.Type) => "is boxed",
            IReturnOperation => "is returned",
            IAddressOfOperation => "has its address taken",
            IMethodReferenceOperation method when method.Instance == operation => "gives a method group, whose delegate holds a copy",
            IVariableInitializerOperation { Parent: IVariableDeclaratorOperation { Symbol.IsRef: true } } => null,
            _ => "is copied"
        });
    }

    internal static string Describe(string name, string use) => $"The frame '{name}' {use}";

    private sealed class Checker(SemanticModel model, INamedTypeSymbol frame, CancellationToken cancellationToken)
    {
        // The simple names that can spell the type in this file: GoFrame, and any file-local alias of it.
        private readonly HashSet<string> m_typeNames = [GoFrameName];

        // The names of the frame locals and parameters this file declares: the only names a frame VALUE can have here,
        // since a field, property or return that holds one is refused at its declaration.
        private readonly HashSet<string> m_valueNames = [];

        private readonly List<DiagnosticRecord> m_records = [];

        private readonly List<TextSpan> m_deferred = [];

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

            return m_records.Count == 0 && m_deferred.Count == 0
                ? FileRefusals.Empty
                : new FileRefusals([.. m_records], [.. m_deferred], root.SyntaxTree.FilePath);
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

            if (symbol is null || !IsFrame(type))
                return;

            bool inNameOf = identifier.Parent is ArgumentSyntax argument && IsNameOfArgument(argument);

            if (!inNameOf && !SymbolEqualityComparer.Default.Equals(model.GetEnclosingSymbol(identifier.SpanStart, cancellationToken), symbol.ContainingSymbol))
            {
                Report(identifier, Describe(symbol.Name, "is captured by a lambda or local function"));
                return;
            }

            // `ref GoFrame alias = ref ᒐ;` is refused at the ref local's declaration.
            if (identifier.Parent is RefExpressionSyntax)
                return;

            bool allowed = IsAllowedPosition(identifier);

            Bound bound = Classify(model, identifier, frame, cancellationToken);

            if (allowed)
            {
                // Syntax allows it; binding narrows it to a refusal where it binds, and defers it where it does not.
                if (!bound.Known)
                    m_deferred.Add(identifier.Span);
                else if (bound.Use is not null)
                    Report(identifier, Describe(symbol.Name, bound.Use));
            }
            else if (!bound.Known || bound.Use is not null)
            {
                Report(identifier, Describe(symbol.Name, bound.Use ?? "is copied"));
            }
        }
    }

    // One file's refusals and deferred uses, equal by value so the incremental pipeline re-reports only a file that changed.
    private sealed class FileRefusals(ImmutableArray<DiagnosticRecord> records, ImmutableArray<TextSpan> deferred, string filePath) : IEquatable<FileRefusals>
    {
        public static readonly FileRefusals Empty = new([], [], string.Empty);

        public ImmutableArray<DiagnosticRecord> Records { get; } = records;

        public ImmutableArray<TextSpan> Deferred { get; } = deferred;

        public string FilePath { get; } = filePath;

        // One file per source file, named by its key (DeferredKey), so two files of one name stay apart.
        public string DeferredHintName => DeferredHintPrefix + DeferredKey(FilePath) + ".g.cs";

        // Comments only: the file adds no type and no member, and only GoFrameRuleAnalyzer reads it.
        public string DeferredSource()
        {
            StringBuilder source = new();
            source.Append("// <auto-generated/>\r\n");
            source.Append("// go2cs-gen GoFrameRuleGenerator: uses of a GoFrame in an allowed position whose call did not bind in the\r\n");
            source.Append("// generator's view (it does not see the other generators' output). GoFrameRuleAnalyzer binds each one fully.\r\n");

            foreach (TextSpan span in Deferred)
                source.Append(DeferredLinePrefix).Append(span.Start.ToString(CultureInfo.InvariantCulture)).Append(' ')
                    .Append(span.Length.ToString(CultureInfo.InvariantCulture)).Append(' ').Append(DeferredKey(FilePath)).Append("\r\n");

            return source.ToString();
        }

        public bool Equals(FileRefusals? other) =>
            other is not null && FilePath == other.FilePath && Records.SequenceEqual(other.Records) && Deferred.SequenceEqual(other.Deferred);

        public override bool Equals(object? obj) => Equals(obj as FileRefusals);

        public override int GetHashCode() => Records.Length ^ (Deferred.Length << 16);
    }

    // A diagnostic as plain data (a Location holds a syntax tree, which the incremental cache must not retain).
    private sealed record DiagnosticRecord(string FilePath, TextSpan Span, LinePositionSpan LineSpan, string Use)
    {
        public Diagnostic ToDiagnostic() =>
            Diagnostic.Create(GeneratorDiagnostics.GoFrameEscapes, Location.Create(FilePath, Span, LineSpan), Use);
    }
}
