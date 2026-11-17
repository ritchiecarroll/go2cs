// go2cs code converter defines `global using` statements here for imported type
// aliases as package references are encountered via `import' statements. Exported
// type aliases that need a `global using` declaration will be loaded from the
// referenced package by parsing its 'package_info.cs' source file and reading its
// defined `GoTypeAlias` attributes.

// Package name separator "dot" used in imported type aliases is extended Unicode
// character '\uA4F8' which is a valid character in a C# identifier name. This is
// used to simulate Go's package level type aliases since C# does not yet support
// importing type aliases at a namespace level.

// <ImportedTypeAliases>
// </ImportedTypeAliases>

using go;
using static go.main_package;

// For encountered type alias declarations, e.g., `type Table = map[string]int`,
// go2cs code converter will generate a `global using` statement for the alias in
// the converted source, e.g.: `global using Table = go.map<go.@string, nint>;`.
// Although scope of `global using` is available to all files in the project, all
// converted Go code for the project targets the same package, so `global using`
// statements will effectively have package level scope.

// Additionally, `GoTypeAlias` attributes will be generated here for exported type
// aliases. This allows the type alias to be imported and used from other packages
// when referenced.

// <ExportedTypeAliases>
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<aIf, selfer>(Pointer = true)]
[assembly: GoImplement<aStmt, selfer>(Pointer = true)]
[assembly: GoImplement<bBoth, namer>(Pointer = true)]
[assembly: GoImplement<bFirst, namer>(Pointer = true)]
[assembly: GoImplement<bFirstOuter, namer>(Pointer = true)]
[assembly: GoImplement<bIf, flowData>(Pointer = true)]
[assembly: GoImplement<bOuter, namer>(Pointer = true)]
[assembly: GoImplement<bStmt, flowData>(Pointer = true)]
[assembly: GoImplement<cBase, visitor>(Pointer = true)]
[assembly: GoImplement<cStmt, visitor>(Pointer = true)]
[assembly: GoImplement<dAll, tagger>(Pointer = true)]
[assembly: GoImplement<dOuter, tagger>(Pointer = true)]
[assembly: GoImplement<eFork, picker>(Pointer = true)]
[assembly: GoImplement<eOuter, picker>(Pointer = true)]
[assembly: GoImplement<gIf, gData>(Pointer = true)]
[assembly: GoImplement<gRef, gData>(Pointer = true)]
[assembly: GoImplement<gToken, gData>(Pointer = true)]
[assembly: GoImplement<gUnion, gData>(Pointer = true)]
[assembly: GoImplement<gVarStmt, gData>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
// </ImplicitConversions>

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: GoFieldAccess(typeof(aDefault))]
[assembly: GoFieldAccess(typeof(aIf))]
[assembly: GoFieldAccess(typeof(aNode))]
[assembly: GoFieldAccess(typeof(aStmt))]
[assembly: GoFieldAccess(typeof(bFlow))]
[assembly: GoFieldAccess(typeof(bIf))]
[assembly: GoFieldAccess(typeof(bStmt))]
[assembly: GoFieldAccess(typeof(cBase))]
[assembly: GoFieldAccess(typeof(cDefault))]
[assembly: GoFieldAccess(typeof(cNode))]
[assembly: GoFieldAccess(typeof(cStmt))]
[assembly: GoFieldAccess(typeof(eOuter))]
[assembly: GoFieldAccess(typeof(gDefault))]
[assembly: GoFieldAccess(typeof(gFlowBase))]
[assembly: GoFieldAccess(typeof(gIf))]
[assembly: GoFieldAccess(typeof(gNode))]
[assembly: GoFieldAccess(typeof(gVarStmt))]
[assembly: GoFieldAccess(typeof(gVisitor))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("main.go", "main.cs", "ABAgggAKDqKCAA4OABYkAAwMAB8wAAgIABokAAkIgoIAFRwACAgADRIAFSAADAwAFR4ACAgADRIAKUaCgoKCAAkIgqKClIKCgoKClIIADg4ADRKCAA4SggAUIgAQGoKCgoKmhIKCgoKCgoKCloKCgoKUgoKClIKCgoKCgpaCgoKCgoKWgoKCgoKWgoKCgpaCgoKWgoKCgoKCgoKCgoKUgpSU", "", "", "518=Modifiers/1/1/1,propagateSubtreeFacts/1/1/1,subtreeFactsWorker/1/1/1;521=computeSubtreeFacts/1/1/1,subtreeFactsWorker/1/1/1;523=propagateSubtreeFacts/1/1/1;525=subtreeFactsWorker/1/1/1,Modifiers/1/1/1")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("main")]
[GoTestMatchingConsoleOutput]
public static partial class main_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface flowData {}
    internal partial interface gData {}
    internal partial interface namer {}
    internal partial interface picker {}
    internal partial interface selfer {}
    internal partial interface tagger {}
    internal partial interface tier {}
    internal partial interface visitor {}
    internal partial struct aBase {}
    internal partial struct aDefault {}
    internal partial struct aIf {}
    internal partial struct aNode {}
    internal partial struct aSide {}
    internal partial struct aStmt {}
    internal partial struct bBoth {}
    internal partial struct bDeep {}
    internal partial struct bFirst {}
    internal partial struct bFirstOuter {}
    internal partial struct bFlat {}
    internal partial struct bFlow {}
    internal partial struct bIf {}
    internal partial struct bMid {}
    internal partial struct bNode {}
    internal partial struct bNodeBase {}
    internal partial struct bOuter {}
    internal partial struct bStmt {}
    internal partial struct cBase {}
    internal partial struct cDefault {}
    internal partial struct cNode {}
    internal partial struct cStmt {}
    internal partial struct dAll {}
    internal partial struct dLeft {}
    internal partial struct dOuter {}
    internal partial struct dPair {}
    internal partial struct dRight {}
    internal partial struct dTop {}
    internal partial struct eFar {}
    internal partial struct eFork {}
    internal partial struct eNear {}
    internal partial struct eOuter {}
    internal partial struct eVia {}
    internal partial struct fAll {}
    internal partial struct fBoth {}
    internal partial struct fDeep {}
    internal partial struct fOne {}
    internal partial struct fTwo {}
    internal partial struct fWrap {}
    internal partial struct gBase {}
    internal partial struct gDefault {}
    internal partial struct gFacts {}
    internal partial struct gIf {}
    internal partial struct gModsBase {}
    internal partial struct gRef {}
    internal partial struct gStmtBase {}
    internal partial struct gToken {}
    internal partial struct gTypeArgsBase {}
    internal partial struct gTypeBase {}
    internal partial struct gTypeSyntax {}
    internal partial struct gUnion {}
    internal partial struct gVarStmt {}
    public partial struct gFlowBase {}
    public partial struct gNode {}
    public partial struct gVisitor {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(main_package.aDefault)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛaDefault;
[GoMethodHost(typeof(main_package.aNode)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛaNode;
[GoMethodHost(typeof(main_package.aSide)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛaSide;
[GoMethodHost(typeof(main_package.bDeep)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛbDeep;
[GoMethodHost(typeof(main_package.bFlat)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛbFlat;
[GoMethodHost(typeof(main_package.bFlow)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛbFlow;
[GoMethodHost(typeof(main_package.bNode)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛbNode;
[GoMethodHost(typeof(main_package.cDefault)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛcDefault;
[GoMethodHost(typeof(main_package.cNode)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛcNode;
[GoMethodHost(typeof(main_package.dLeft)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛdLeft;
[GoMethodHost(typeof(main_package.dRight)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛdRight;
[GoMethodHost(typeof(main_package.dTop)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛdTop;
[GoMethodHost(typeof(main_package.eFar)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛeFar;
[GoMethodHost(typeof(main_package.eNear)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛeNear;
[GoMethodHost(typeof(main_package.fDeep)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛfDeep;
[GoMethodHost(typeof(main_package.fOne)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛfOne;
[GoMethodHost(typeof(main_package.fTwo)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛfTwo;
[GoMethodHost(typeof(main_package.gDefault)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛgDefault;
[GoMethodHost(typeof(main_package.gFlowBase)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛgFlowBase;
[GoMethodHost(typeof(main_package.gModsBase)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛgModsBase;
[GoMethodHost(typeof(main_package.gNode)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛgNode;
[GoMethodHost(typeof(main_package.gTypeSyntax)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛgTypeSyntax;
[GoMethodHost(typeof(main_package.gUnion)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛgUnion;
// </MethodHolders>
