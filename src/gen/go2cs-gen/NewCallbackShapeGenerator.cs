// NewCallbackShapeGenerator.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace go2cs;

/// <summary>
/// Registers, from the call site, every func type a compilation passes to <c>syscall.NewCallback</c> or
/// <c>syscall.NewCallbackCDecl</c>, so the callback's generic binder is closed where Native AOT can see it
/// (docs/phase4/DESIGN-newcallback-native-aot.md).
/// </summary>
/// <remarks>
/// <para>
/// syscall builds a callback's shim with a binder generic over the func's parameter and result types. Under
/// the JIT it closes that binder with <c>MakeGenericType</c> over the types it reads from <c>Delegate.Method</c>;
/// Native AOT compiles no code for an instantiation it cannot see at build time, and the type arguments are
/// value types with no shared form. This generator emits one file-local module initializer per compilation
/// that calls <c>syscall_package.GoNewCallbackShapes.Register&lt;…&gt;</c> once per distinct delegate type,
/// a closed generic call ILC compiles. Nothing changes in the visible converted code.
/// </para>
/// <para>
/// A site's argument gives its type as the converter emits it: a lambda or a method group passes bare, and a
/// func that can be nil passes as <c>(f).OrTypedNilFunc()</c>, which returns <c>object</c>, so the type is
/// that call's <c>TDelegate</c>. A shape no binder takes (no result, an arity other than 0, 1, 2 and 4)
/// is registered as a refusal with its facts, so Go's text reads the same under both runtimes. A method
/// that passes its own <c>object</c> parameter to a site (x/sys/windows.NewCallback) is a FORWARDER: it is
/// marked with an assembly attribute, and in every compilation that references it its calls are sites.
/// </para>
/// <para>
/// Left unregistered, so no build that compiles today breaks: a delegate type naming a type parameter, a
/// compiler-made delegate type, a type the generated file cannot access, and a compilation whose syscall has
/// no <c>GoNewCallbackShapes</c>.
/// </para>
/// </remarks>
[Generator]
public class NewCallbackShapeGenerator : ISourceGenerator
{
    private const string ShapesName = "go.syscall_package+GoNewCallbackShapes";
    private const string ForwarderName = "go.syscall_package+GoNewCallbackForwarderAttribute";

    private static readonly SymbolDisplayFormat s_format = SymbolDisplayFormat.FullyQualifiedFormat
        .WithMiscellaneousOptions(SymbolDisplayFormat.FullyQualifiedFormat.MiscellaneousOptions
            & ~SymbolDisplayMiscellaneousOptions.UseSpecialTypes);

    public void Initialize(GeneratorInitializationContext context) { }

    public void Execute(GeneratorExecutionContext context)
    {
        string? source = Generate(context.Compilation);

        if (source is not null)
            context.AddGeneratedSource("go2cs.newcallback-shapes.g.cs", source);
    }

    // Separate from Execute so the shape of the output is testable without a driver.
    internal static string? Generate(Compilation compilation)
    {
        if (compilation.GetTypeByMetadataName(ShapesName) is not { } shapes ||
            !compilation.IsSymbolAccessibleWithin(shapes, compilation.Assembly) ||
            compilation.GetTypeByMetadataName(ForwarderName) is not { } forwarderAttribute)
            return null;

        INamedTypeSymbol syscall = shapes.ContainingType;

        // (holder, method name) -> the parameter index that is passed through.
        Dictionary<(ISymbol Holder, string Name), int> forwarders = new(ForwarderKeyComparer.Instance);

        foreach (IAssemblySymbol assembly in compilation.SourceModule.ReferencedAssemblySymbols)
        {
            foreach (AttributeData attribute in assembly.GetAttributes())
            {
                if (!SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, forwarderAttribute) ||
                    attribute.ConstructorArguments.Length != 3 ||
                    attribute.ConstructorArguments[0].Value is not INamedTypeSymbol holder ||
                    attribute.ConstructorArguments[1].Value is not string name ||
                    attribute.ConstructorArguments[2].Value is not int index)
                    continue;

                forwarders[(holder, name)] = index;
            }
        }

        // The names a call must carry to be a candidate: a syntactic filter, so no invocation in a compilation
        // that never calls NewCallback is bound.
        HashSet<string> names = ["NewCallback", "NewCallbackCDecl", .. forwarders.Keys.Select(key => key.Name)];
        List<(InvocationExpressionSyntax Call, string Name, SyntaxTree Tree)> calls = [];

        foreach (SyntaxTree tree in compilation.SyntaxTrees)
        {
            foreach (InvocationExpressionSyntax call in tree.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                if (CalledName(call) is { } called && call.ArgumentList.Arguments.Count > 0)
                    calls.Add((call, called, tree));
            }
        }

        Dictionary<SyntaxTree, SemanticModel> models = [];
        Dictionary<(ISymbol Holder, string Name), int> local = new(ForwarderKeyComparer.Instance);
        HashSet<InvocationExpressionSyntax> seen = [];
        List<(ExpressionSyntax Argument, SemanticModel Model)> sites = [];
        bool grew;

        // A forwarder found in this compilation makes calls of it sites too, so repeat until no name is added.
        do
        {
            grew = false;

            foreach ((InvocationExpressionSyntax call, string called, SyntaxTree tree) in calls)
            {
                if (!names.Contains(called) || seen.Contains(call))
                    continue;

                if (!models.TryGetValue(tree, out SemanticModel? model))
                    models[tree] = model = compilation.GetSemanticModel(tree);

                // A call whose argument binds only through another generator's output (a pointer method value)
                // does not resolve here; its one candidate is the method.
                SymbolInfo target = model.GetSymbolInfo(call);

                if ((target.Symbol ?? (target.CandidateSymbols.Length == 1 ? target.CandidateSymbols[0] : null))
                    is not IMethodSymbol method)
                    continue;

                int index;

                if (SymbolEqualityComparer.Default.Equals(method.ContainingType, syscall) &&
                    method.Name is "NewCallback" or "NewCallbackCDecl")
                    index = 0;
                else if (!forwarders.TryGetValue((method.ContainingType, method.Name), out index) &&
                         !local.TryGetValue((method.ContainingType, method.Name), out index))
                    continue;

                if (ArgumentAt(call, method, index) is not { } argument)
                    continue;

                // The argument is the enclosing method's own object parameter: that method forwards.
                if (Strip(argument) is IdentifierNameSyntax identifier &&
                    model.GetSymbolInfo(identifier).Symbol is IParameterSymbol { Type.SpecialType: SpecialType.System_Object } parameter &&
                    parameter.ContainingSymbol is IMethodSymbol { MethodKind: MethodKind.Ordinary } enclosing &&
                    SymbolEqualityComparer.Default.Equals(enclosing.ContainingAssembly, compilation.Assembly) &&
                    !local.ContainsKey((enclosing.ContainingType, enclosing.Name)))
                {
                    local[(enclosing.ContainingType, enclosing.Name)] = parameter.Ordinal;
                    names.Add(enclosing.Name);
                    grew = true;
                }

                seen.Add(call);
                sites.Add((argument, model));
            }
        }
        while (grew);

        SortedDictionary<string, string> registrations = new(System.StringComparer.Ordinal);

        foreach ((ExpressionSyntax argument, SemanticModel model) in sites)
        {
            if (DelegateTypeOf(argument, model, compilation) is { } type && Registration(type, compilation) is { } line)
                registrations[type.ToDisplayString(s_format)] = line;
        }

        List<string> marks = local
            .Where(pair => compilation.IsSymbolAccessibleWithin(pair.Key.Holder, compilation.Assembly))
            .Select(pair => "[assembly: global::go.syscall_package.GoNewCallbackForwarder(typeof(" +
                            ((ITypeSymbol)pair.Key.Holder).ToDisplayString(s_format) + "), \"" + pair.Key.Name + "\", " +
                            pair.Value + ")]")
            .OrderBy(mark => mark, System.StringComparer.Ordinal)
            .ToList();

        if (registrations.Count == 0 && marks.Count == 0)
            return null;

        StringBuilder text = new();
        text.Append("// <auto-generated/>\r\n");
        text.Append("#nullable disable\r\n");

        foreach (string mark in marks)
            text.Append(mark).Append("\r\n");

        text.Append("\r\nnamespace go;\r\n\r\n");
        text.Append("file static class ᴛNewCallbackShapes\r\n{\r\n");
        text.Append("    [global::System.Runtime.CompilerServices.ModuleInitializer]\r\n");
        text.Append("    internal static void ᴛRegister()\r\n    {\r\n");

        foreach (string line in registrations.Values)
            text.Append("        ").Append(line).Append("\r\n");

        text.Append("    }\r\n}\r\n");

        return text.ToString();
    }

    private static string? CalledName(InvocationExpressionSyntax call) => call.Expression switch
    {
        IdentifierNameSyntax name => name.Identifier.ValueText,
        MemberAccessExpressionSyntax { Name: var name } => name.Identifier.ValueText,
        _ => null
    };

    // `index` counts the parameters of the method's declared (static) form. A Go method converts to an extension
    // method whose receiver is its first parameter, and a call `p.NewCallback(f)` binds the REDUCED method, whose
    // parameters leave the receiver out, so the index moves down by one there.
    private static ExpressionSyntax? ArgumentAt(InvocationExpressionSyntax call, IMethodSymbol method, int index)
    {
        if (method.ReducedFrom is not null)
            index--;

        if (index < 0 || index >= method.Parameters.Length)
            return null;

        string name = method.Parameters[index].Name;

        foreach (ArgumentSyntax argument in call.ArgumentList.Arguments)
        {
            if (argument.NameColon?.Name.Identifier.ValueText == name)
                return argument.Expression;
        }

        return index < call.ArgumentList.Arguments.Count && call.ArgumentList.Arguments[index].NameColon is null
            ? call.ArgumentList.Arguments[index].Expression
            : null;
    }

    // Parentheses and casts to object are looked through; a cast to a delegate type is kept, since it names
    // the type.
    private static ExpressionSyntax Strip(ExpressionSyntax expression)
    {
        while (true)
        {
            switch (expression)
            {
                case ParenthesizedExpressionSyntax parenthesized:
                    expression = parenthesized.Expression;
                    continue;
                case CastExpressionSyntax { Type: PredefinedTypeSyntax { Keyword.ValueText: "object" } } cast:
                    expression = cast.Expression;
                    continue;
                default:
                    return expression;
            }
        }
    }

    internal static INamedTypeSymbol? DelegateTypeOf(ExpressionSyntax argument, SemanticModel model, Compilation compilation)
    {
        ExpressionSyntax expression = Strip(argument);

        // `(f).OrTypedNilFunc()`: the converter's carrier for a func that can be nil; it returns object, and
        // the func type is its type argument.
        if (expression is InvocationExpressionSyntax invocation &&
            model.GetSymbolInfo(invocation).Symbol is IMethodSymbol { Name: "OrTypedNilFunc", TypeArguments.Length: 1 } carrier &&
            (carrier.ReducedFrom ?? carrier).ContainingType is { Name: "NilFuncExtensions" })
            return carrier.TypeArguments[0] as INamedTypeSymbol is { TypeKind: TypeKind.Delegate } carried ? carried : null;

        if (model.GetTypeInfo(expression).Type is INamedTypeSymbol { TypeKind: TypeKind.Delegate } typed)
            return typed;

        // A lambda or a method group converted to object takes its natural type, the Func/Action C# gives it, and
        // that is the runtime type the callback sees. Only those shapes: an invocation returning object names a
        // method whose own signature is not the argument's.
        if (expression is not (LambdaExpressionSyntax or AnonymousMethodExpressionSyntax or IdentifierNameSyntax or
            MemberAccessExpressionSyntax or GenericNameSyntax))
            return null;

        SymbolInfo info = model.GetSymbolInfo(expression);

        if ((info.Symbol ?? (info.CandidateSymbols.Length == 1 ? info.CandidateSymbols[0] : null)) is IMethodSymbol method)
            return NaturalType(method, compilation);

        return expression is MemberAccessExpressionSyntax access ? PointerMethodValueType(access, model, compilation) : null;
    }

    // A method value on a pointer box, `w.wndProc` with `w` a `ж<window>`: the converter declares the method on
    // `this ref window` and go2cs-gen's RecvGenerator adds the `this ж<window>` overload the call binds. Generators
    // do not see each other's output, so here the method group does not resolve. The method is found where the
    // converter declares it, among the extension methods of the receiver type's package class, and its natural
    // type is the one the overload has: its parameters after the receiver.
    private static INamedTypeSymbol? PointerMethodValueType(MemberAccessExpressionSyntax access, SemanticModel model,
        Compilation compilation)
    {
        if (model.GetTypeInfo(access.Expression).Type is not INamedTypeSymbol { Name: "ж", TypeArguments.Length: 1 } box ||
            box.TypeArguments[0] is not INamedTypeSymbol receiver || receiver.ContainingType is not { } holder)
            return null;

        string name = access.Name.Identifier.ValueText;
        INamedTypeSymbol? found = null;

        foreach (IMethodSymbol candidate in holder.GetMembers(name).OfType<IMethodSymbol>())
        {
            if (!candidate.IsExtensionMethod || candidate.Parameters.Length == 0 ||
                !SymbolEqualityComparer.Default.Equals(candidate.Parameters[0].Type, receiver) &&
                !SymbolEqualityComparer.Default.Equals(candidate.Parameters[0].Type, box))
                continue;

            if (NaturalType(candidate, compilation, skip: 1) is not { } type ||
                found is not null && !SymbolEqualityComparer.Default.Equals(found, type))
                return null;

            found = type;
        }

        return found;
    }

    // The Func/Action C# gives a lambda or method group as its natural type; null where the compiler would make
    // an anonymous delegate type instead (a ref parameter, more than 16 parameters), which cannot be named. A
    // variadic (`params`) or optional-parameter method group also gets a compiler-made type, so the Func built here
    // never matches its runtime type: a missed registration (the reflective path, or the named refusal under
    // Native AOT), never a broken build.
    private static INamedTypeSymbol? NaturalType(IMethodSymbol method, Compilation compilation, int skip = 0)
    {
        IParameterSymbol[] parameters = [.. method.Parameters.Skip(skip)];

        if (parameters.Length > 16 || parameters.Any(parameter => parameter.RefKind != RefKind.None) ||
            method.ReturnsByRef || method.ReturnsByRefReadonly || method.IsVararg)
            return null;

        List<ITypeSymbol> arguments = [.. parameters.Select(parameter => parameter.Type)];

        if (!method.ReturnsVoid)
            arguments.Add(method.ReturnType);

        if (arguments.Count == 0)
            return compilation.GetTypeByMetadataName("System.Action");

        string name = method.ReturnsVoid ? "System.Action`" + arguments.Count : "System.Func`" + arguments.Count;

        return compilation.GetTypeByMetadataName(name)?.Construct([.. arguments]);
    }

    internal static string? Registration(INamedTypeSymbol type, Compilation compilation)
    {
        if (type.DelegateInvokeMethod is not { } invoke || IsCompilerMade(type) || MentionsTypeParameter(type) ||
            !IsAccessible(type, compilation) || !CanBeTypeArgument(invoke.ReturnType) ||
            invoke.Parameters.Any(parameter => !CanBeTypeArgument(parameter.Type)) ||
            invoke.Parameters.Any(parameter => parameter.RefKind != RefKind.None) || invoke.ReturnsByRef ||
            invoke.ReturnsByRefReadonly)
            return null;

        string typeName = type.ToDisplayString(s_format);
        int arity = invoke.Parameters.Length;
        bool hasResult = !invoke.ReturnsVoid;

        if (!hasResult || arity is not (0 or 1 or 2 or 4))
        {
            bool floatResult = hasResult && IsFloat(invoke.ReturnType);
            bool floatArgument = invoke.Parameters.Any(parameter => IsFloat(parameter.Type));

            return "global::go.syscall_package.GoNewCallbackShapes.Refusal(typeof(" + typeName + "), " + arity + ", " +
                   Bool(hasResult) + ", " + Bool(floatResult) + ", " + Bool(floatArgument) + ");";
        }

        List<string> typeArguments = [.. invoke.Parameters.Select(parameter => parameter.Type.ToDisplayString(s_format))];
        typeArguments.Add(invoke.ReturnType.ToDisplayString(s_format));

        string func = "global::System.Func<" + string.Join(", ", typeArguments) + ">";
        string adapter;

        if (type.ToDisplayString(s_format) == func)
        {
            adapter = "static d => (" + func + ")d";
        }
        else
        {
            string parameters = string.Join(", ", Enumerable.Range(1, arity).Select(i => "a" + i));
            adapter = "static d => { var f = (" + typeName + ")d; return (" + parameters + ") => f(" + parameters + "); }";
        }

        return "global::go.syscall_package.GoNewCallbackShapes.Register<" + string.Join(", ", typeArguments) +
               ">(typeof(" + typeName + "), " + adapter + ");";
    }

    // A pointer, a function pointer, a ref struct or a file-local type cannot be a type argument of the
    // registration (CS0306, CS9244) or be named from the generated file. Hand-written C# can pass one
    // (`static nuint cb(void* p)` in an *_impl.cs companion); it stays on the reflective path.
    private static bool CanBeTypeArgument(ITypeSymbol type) => type switch
    {
        IPointerTypeSymbol or IFunctionPointerTypeSymbol => false,
        { IsRefLikeType: true } => false,
        INamedTypeSymbol named => !named.IsFileLocal && named.TypeArguments.All(CanBeTypeArgument),
        IArrayTypeSymbol array => CanBeTypeArgument(array.ElementType),
        _ => true
    };

    private static string Bool(bool value) => value ? "true" : "false";

    private static bool IsFloat(ITypeSymbol type) => type.SpecialType is SpecialType.System_Single or SpecialType.System_Double;

    private static bool IsCompilerMade(INamedTypeSymbol type) =>
        type.IsImplicitlyDeclared || type.IsAnonymousType || !type.CanBeReferencedByName;

    private static bool MentionsTypeParameter(ITypeSymbol type) => type switch
    {
        ITypeParameterSymbol => true,
        IArrayTypeSymbol array => MentionsTypeParameter(array.ElementType),
        IPointerTypeSymbol pointer => MentionsTypeParameter(pointer.PointedAtType),
        INamedTypeSymbol named => named.TypeArguments.Any(MentionsTypeParameter) ||
                                  (named.ContainingType is { } outer && MentionsTypeParameter(outer)),
        _ => false
    };

    private static bool IsAccessible(INamedTypeSymbol type, Compilation compilation)
    {
        if (!compilation.IsSymbolAccessibleWithin(type, compilation.Assembly))
            return false;

        IMethodSymbol invoke = type.DelegateInvokeMethod!;

        return invoke.Parameters.All(parameter => compilation.IsSymbolAccessibleWithin(parameter.Type, compilation.Assembly)) &&
               compilation.IsSymbolAccessibleWithin(invoke.ReturnType, compilation.Assembly);
    }

    private sealed class ForwarderKeyComparer : IEqualityComparer<(ISymbol Holder, string Name)>
    {
        public static readonly ForwarderKeyComparer Instance = new();

        public bool Equals((ISymbol Holder, string Name) x, (ISymbol Holder, string Name) y) =>
            SymbolEqualityComparer.Default.Equals(x.Holder, y.Holder) && x.Name == y.Name;

        public int GetHashCode((ISymbol Holder, string Name) key) =>
            SymbolEqualityComparer.Default.GetHashCode(key.Holder) ^ key.Name.GetHashCode();
    }
}
