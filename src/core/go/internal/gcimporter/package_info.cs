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
global using pkgbitsꓸRelElemIdx = go.@internal.pkgbits_package.Index;
using pkgbits = go.@internal.pkgbits_package;
using token = go.go.token_package;
using types = go.go.types_package;
// </ImportedTypeAliases>

using go;
using static go.go.@internal.gcimporter_package;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

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
[assembly: GoImplement<os_package.File, io_package.ReadCloser>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
// </ImplicitConversions>

// This package's string literals are hoisted to `static readonly` fields (the standard
// library's form, or -hoist-literals), so no converted file passes a bare literal for
// go2cs-gen's literal interceptor to box. The stamp below tells the generator to skip
// the assembly.
[assembly: GoHoistedLiterals]

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: GoFieldAccess(typeof(fileInfo))]
[assembly: GoFieldAccess(typeof(pkgReader))]
[assembly: GoFieldAccess(typeof(reader))]
[assembly: GoFieldAccess(typeof(readerDict))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("go/internal/gcimporter/gcimporter.go", "gcimporter.cs", "ABUs8oKCpoKUpoKUgoKUlIKCgoKUuIK4koKUgpSmlKSCgoKClJSChA==", "62-67:1", "", "", "79:21", "70:50,71:17")]
[assembly: GoPositionMap("go/internal/gcimporter/support.go", "support.cs", "ABAggoK4ogAPIO6CgoKmgpSCuKaCgoKCpoIAFyySgoKUgoKClA==", "64-69:1")]
[assembly: GoPositionMap("go/internal/gcimporter/ureader.go", "ureader.cs", "ACVUkqriABQelIKCgqamgoKUkqaUgqaCqJKCgqaGlIIAGzqC3ILcgqqygoK4goKCpqK2ooKmgoLulKaCgqa0lIKCqrKCtsaCpoKCpqKClLSkpoKmlIKUqrK2goKClKaygoKCgpSmgqaCgoKUgoKmgqaCpqKkgra2goKCgpSmtoKkgqSkpKSkpKS4goKCgoKCgoKUgoKClKamgoKClKaigoKUgoKCgqaCpoKCAAkUlKaClIKClKaClIKCpqaClIKClIKCqrKUgqaClIKCpqakgoKCgpSClIKmgoK4gqaClIKUkqaUtoKCgpSCtoKCgraCgoK2lIKClJTKgoKCgpSCgqaCgqaCgqaUgsiCgsimpJSCgoKmgoK2goKCkoK4uKai7oIABxCCgoKUgraCggAPHoKigrimgoKClIKUgqaCgpSCgq7CgpTotqSCgoKUgg==", "100-102:1;483-485:1;638-642:1", "", "50=NumElems/1/3/11,NumElems/2/3/12,NumElems/3/3/13;141=NewDecoder/1/1/1;148=TempDecoder/1/1/1")]
// </GoSourcePositionMaps>

namespace go.go.@internal;

using static global::go.go.@internal.gcimporter_package;
using pkgbits = global::go.@internal.pkgbits_package;
using token = global::go.go.token_package;
using types = global::go.go.types_package;

[GoPackage("gcimporter")]
public static partial class gcimporter_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct derivedInfo {}
    internal partial struct fakeFileSet {}
    internal partial struct fileInfo {}
    [GoValueClone("PkgDecoder")] internal partial struct pkgReader {}
    internal partial struct reader {}
    internal partial struct readerDict {}
    internal partial struct typeInfo {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbufio() => builtin.initPackage(typeof(bufio_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸtoken() => builtin.initPackage(typeof(global::go.go.token_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸtypes() => builtin.initPackage(typeof(global::go.go.types_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸexportdata() => builtin.initPackage(typeof(global::go.@internal.exportdata_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸgodebug() => builtin.initPackage(typeof(global::go.@internal.godebug_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸpkgbits() => builtin.initPackage(typeof(global::go.@internal.pkgbits_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(gcimporter_package.fakeFileSet)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class gcimporterᴛfakeFileSet;
[GoMethodHost(typeof(gcimporter_package.pkgReader)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class gcimporterᴛpkgReader;
[GoMethodHost(typeof(gcimporter_package.reader)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class gcimporterᴛreader;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class gcimporterᴛfakeFileSet {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static token.Pos pos(this ref fakeFileSet s, @string @file, nint line, nint column) => gcimporter_package.pos⁔(ref s, @file, line, column);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void setLines(this ref fakeFileSet s) => gcimporter_package.setLines⁔(ref s);
}
partial class gcimporterᴛpkgReader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void later(this ref pkgReader pr, Action fn) => gcimporter_package.later⁔(ref pr, fn);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<reader> newReader(this ж<pkgReader> Ꮡpr, pkgbits.SectionKind k, pkgbits.Index idx, pkgbits.SyncMarker marker) => gcimporter_package.newReader⁔(Ꮡpr, k, idx, marker);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<reader> tempReader(this ж<pkgReader> Ꮡpr, pkgbits.SectionKind k, pkgbits.Index idx, pkgbits.SyncMarker marker) => gcimporter_package.tempReader⁔(Ꮡpr, k, idx, marker);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void retireReader(this ref pkgReader pr, ж<reader> Ꮡr) => gcimporter_package.retireReader⁔(ref pr, Ꮡr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string posBaseIdx(this ж<pkgReader> Ꮡpr, pkgbits.Index idx) => gcimporter_package.posBaseIdx⁔(Ꮡpr, idx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<types.Package> pkgIdx(this ж<pkgReader> Ꮡpr, pkgbits.Index idx) => gcimporter_package.pkgIdx⁔(Ꮡpr, idx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static types.Type typIdx(this ж<pkgReader> Ꮡpr, typeInfo info, ж<readerDict> Ꮡdict) => gcimporter_package.typIdx⁔(Ꮡpr, info, Ꮡdict);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<types.Package>, @string) objIdx(this ж<pkgReader> Ꮡpr, pkgbits.Index idx) => gcimporter_package.objIdx⁔(Ꮡpr, idx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<readerDict> objDictIdx(this ж<pkgReader> Ꮡpr, pkgbits.Index idx) => gcimporter_package.objDictIdx⁔(Ꮡpr, idx);
}
partial class gcimporterᴛreader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static token.Pos pos(this ж<reader> Ꮡr) => gcimporter_package.pos⁔(Ꮡr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string posBase(this ж<reader> Ꮡr) => gcimporter_package.posBase⁔(Ꮡr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<types.Package> pkg(this ж<reader> Ꮡr) => gcimporter_package.pkg⁔(Ꮡr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<types.Package> doPkg(this ж<reader> Ꮡr) => gcimporter_package.doPkg⁔(Ꮡr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static types.Type typ(this ж<reader> Ꮡr) => gcimporter_package.typ⁔(Ꮡr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static typeInfo typInfo(this ж<reader> Ꮡr) => gcimporter_package.typInfo⁔(Ꮡr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static types.Type /*res*/ doTyp(this ж<reader> Ꮡr) => gcimporter_package.doTyp⁔(Ꮡr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<types.Struct> structType(this ж<reader> Ꮡr) => gcimporter_package.structType⁔(Ꮡr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<types.Union> unionType(this ж<reader> Ꮡr) => gcimporter_package.unionType⁔(Ꮡr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<types.Interface> interfaceType(this ж<reader> Ꮡr) => gcimporter_package.interfaceType⁔(Ꮡr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<types.Signature> signature(this ж<reader> Ꮡr, ж<types.Var> Ꮡrecv, slice<ж<types.TypeParam>> rtparams, slice<ж<types.TypeParam>> tparams) => gcimporter_package.signature⁔(Ꮡr, Ꮡrecv, rtparams, tparams);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<types.Tuple> @params(this ж<reader> Ꮡr, types.VarKind kind) => gcimporter_package.params⁔(Ꮡr, kind);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<types.Var> param(this ж<reader> Ꮡr, types.VarKind kind) => gcimporter_package.param⁔(Ꮡr, kind);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (types.Object, slice<types.Type>) obj(this ж<reader> Ꮡr) => gcimporter_package.obj⁔(Ꮡr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<ж<types.TypeParam>> typeParamNames(this ж<reader> Ꮡr) => gcimporter_package.typeParamNames⁔(Ꮡr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<types.Func> method(this ж<reader> Ꮡr) => gcimporter_package.method⁔(Ꮡr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<types.Package>, @string) qualifiedIdent(this ж<reader> Ꮡr) => gcimporter_package.qualifiedIdent⁔(Ꮡr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<types.Package>, @string) localIdent(this ж<reader> Ꮡr) => gcimporter_package.localIdent⁔(Ꮡr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<types.Package>, @string) selector(this ж<reader> Ꮡr) => gcimporter_package.selector⁔(Ꮡr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<types.Package>, @string) ident(this ж<reader> Ꮡr, pkgbits.SyncMarker marker) => gcimporter_package.ident⁔(Ꮡr, marker);
}
// </MethodForwarders>
