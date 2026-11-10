// MethodLimitAnalyzer.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace go2cs;

/// <summary>
/// GO2CS0012: no type of the compilation declares more methods than CoreCLR loads on it.
/// </summary>
/// <remarks>
/// <para>
/// CoreCLR numbers the slots of a type's method table with 16 bits and keeps ten in reserve, so a type has at most
/// <see cref="MaxSlots"/> (<c>MAX_SLOT_INDEX</c> in methodtablebuilder.h). A type that needs one more is refused when
/// it is first used: TypeLoadException, "contains more methods than the current implementation allows". The C#
/// compiler has no such limit, so the program builds and then throws.
/// </para>
/// <para>
/// A type's slots are the virtual slots it inherits, and one for each method it declares that does not override one
/// of those: static and instance methods, constructors, accessors and operators alike. System.Object brings four
/// (ToString, Equals, GetHashCode and its finalizer), so a static class, a struct, or a class that derives from
/// Object can declare 65,521 methods, and a class with a deeper base that many less the virtual methods its bases
/// add. A second limit is on the definitions themselves: no type loads with more than <see cref="MaxMethods"/>,
/// however few slots they need. It binds only where nothing is inherited that is not overridden: an interface, or
/// a class that overrides every virtual method it inherits.
/// </para>
/// <para>
/// MEASURED, not read off the constant (.NET 10.0.14 on linux-x64, 2026-11-10; each image written with
/// PersistedAssemblyBuilder and loaded from a stream). A static class, a struct, and a static class with a static
/// constructor load with 65,521 method definitions and fail with 65,522. A class whose base declares 1 and 7 new
/// virtual methods loads with 65,520 and 65,514 (its constructor included). A class that overrides 1 and 3 of Object's
/// methods loads with 65,522 and 65,524. A class that overrides all four loads with 65,524 too, where the slots
/// alone would allow 65,525, and an interface loads with 65,524 methods and fails with 65,525: the second limit.
/// GenTests' MethodLimitAnalyzerTests load a type at each edge on the runtime the tests run on.
/// </para>
/// <para>
/// A SECOND INSTRUMENT, independent of that one, reads the same edge on converted code: the TypeScript experiment's
/// generated program (N methods on a base struct embedded by M structs, and K package functions) put exactly
/// 2NM + 2N + 5 + K methods in its package class, which loaded with 65,521 and threw TypeLoadException with 65,522,
/// 65,523 and 66,005 (branch claude/i7-ts-readiness-rerun at 25df29a2b3,
/// docs/phase4/EXPERIMENT-ts-readiness-2026-11-07-data/methodcap-repro; the design is
/// docs/phase4/DESIGN-promoted-forwarder-placement.md).
/// </para>
/// <para>
/// A package class holds every function and method of its Go package, and go2cs-gen adds to it. The analyzer reads
/// each type after the generators have run, generated parts included, and counts the methods its symbol declares. A
/// partial method with no implementation has no row and is not counted. The methods the compiler adds without a
/// symbol (a local function, a lambda that captures only <c>this</c>, an implicit static constructor) are not
/// counted, so the count is a lower bound: a type the analyzer refuses cannot load, and a type within a few such
/// methods of the limit can still fail.
/// </para>
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MethodLimitAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The most method-table slots CoreCLR gives one type: 65,535 less its reserve of ten.
    /// </summary>
    public const int MaxSlots = 65_525;

    /// <summary>
    /// The most methods CoreCLR loads on one type whatever their slots: one fewer than <see cref="MaxSlots"/>.
    /// </summary>
    public const int MaxMethods = MaxSlots - 1;

    // GO2CS0012, declared with every other go2cs-gen code in GeneratorDiagnostics.
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [GeneratorDiagnostics.TooManyMethods];

    public override void Initialize(AnalysisContext context)
    {
        // Generated parts are where the methods come from, so they are read and reported like any other.
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(Analyze, SymbolKind.NamedType);
    }

    private static void Analyze(SymbolAnalysisContext context)
    {
        INamedTypeSymbol type = (INamedTypeSymbol)context.Symbol;

        if (type.TypeKind is not (TypeKind.Class or TypeKind.Struct or TypeKind.Interface))
            return;

        int methods = CountMethods(type);

        // Nothing inherits more than a few dozen slots, so a type this far below the limit needs no walk of its bases.
        if (methods <= MaxSlots / 2)
            return;

        int most = MostMethods(type);

        if (methods > most && type.Locations is [Location location, ..])
            context.ReportDiagnostic(Diagnostic.Create(GeneratorDiagnostics.TooManyMethods, location, type.ToDisplayString(), methods, most));
    }

    /// <summary>
    /// Counts the methods <paramref name="type"/>'s symbol declares that take a row in metadata.
    /// </summary>
    internal static int CountMethods(INamedTypeSymbol type)
    {
        int methods = 0;

        foreach (ISymbol member in type.GetMembers())
        {
            if (member is IMethodSymbol method && TakesRow(method, type))
                methods++;
        }

        return methods;
    }

    /// <summary>
    /// The most methods <paramref name="type"/> can declare and still load: every slot the runtime gives a type, less
    /// the virtual slots its bases bring, and one more for each method of its own that overrides into one of those;
    /// and never more than <see cref="MaxMethods"/>.
    /// </summary>
    internal static int MostMethods(INamedTypeSymbol type)
    {
        int overrides = 0;

        foreach (ISymbol member in type.GetMembers())
        {
            if (member is IMethodSymbol method && TakesRow(method, type) && Overrides(method, type))
                overrides++;
        }

        return System.Math.Min(MaxMethods, MaxSlots - InheritedVirtualSlots(type) + overrides);
    }

    /// <summary>
    /// Counts the virtual slots <paramref name="type"/>'s base types open: four for System.Object alone.
    /// </summary>
    internal static int InheritedVirtualSlots(INamedTypeSymbol type)
    {
        int slots = 0;

        for (INamedTypeSymbol? each = type.BaseType; each is not null; each = each.BaseType)
        {
            foreach (ISymbol member in each.GetMembers())
            {
                // A finalizer is virtual in metadata and the compiler shows it as neither virtual nor an override:
                // the root type's opens the slot, and every other type's overrides it.
                if (member is not IMethodSymbol method)
                    continue;

                if (method.MethodKind == MethodKind.Destructor ? each.BaseType is null : (method.IsVirtual || method.IsAbstract) && !method.IsOverride)
                    slots++;
            }
        }

        return slots;
    }

    private static bool Overrides(IMethodSymbol method, INamedTypeSymbol declaring) =>
        method.MethodKind == MethodKind.Destructor ? declaring.BaseType is not null : method.IsOverride;

    private static bool TakesRow(IMethodSymbol method, INamedTypeSymbol type)
    {
        // A partial method nobody implements is removed with its calls.
        if (method is { IsPartialDefinition: true, PartialImplementationPart: null })
            return false;

        // A struct's implicit parameterless constructor is the runtime's zero initialization, not a method.
        return !(method is { MethodKind: MethodKind.Constructor, IsImplicitlyDeclared: true } && type.TypeKind == TypeKind.Struct);
    }
}
