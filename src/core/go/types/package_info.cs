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
global using constantꓸKind = go.go.constant_package.ΔKind;
global using runtimeꓸError = go.runtime_package.ΔError;
global using tokenꓸFile = go.go.token_package.ΔFile;
global using tokenꓸPos = go.go.token_package.ΔPos;
global using tokenꓸPosition = go.go.token_package.ΔPosition;
using ast = go.go.ast_package;
using bytes = go.bytes_package;
using token = go.go.token_package;
// </ImportedTypeAliases>

using go;
using static go.go.types_package;

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
[assembly: GoDynamicTypeLift("696e746572666163657b4f626a2829202a676f2f74797065732e547970654e616d657d", "instantiatedType_type")]
[assembly: GoDynamicTypeLift("7374727563747b6e616d6520737472696e673b206b696e6420676f2f74797065732e42617369634b696e643b2076616c20676f2f636f6e7374616e742e56616c75657d", "predeclaredConstsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6e616d6520737472696e673b206e6172677320696e743b20766172696164696320626f6f6c3b206b696e6420676f2f74797065732e657870724b696e647d", "predeclaredFuncsᴛ1")]
[assembly: GoTypeAlias("Error", "ΔError")]
[assembly: GoTypeAlias("Info", "ΔInfo")]
[assembly: GoTypeAlias("Scope", "ΔScope")]
[assembly: GoTypeAlias("Signature", "ΔSignature")]
[assembly: GoTypeAlias("String", "const:ΔString")]
[assembly: GoTypeAlias("Term", "ΔTerm")]
[assembly: GoTypeAlias("Type", "ΔType")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<Alias, cleaner>(Pointer = true)]
[assembly: GoImplement<Alias, ΔType>(Pointer = true)]
[assembly: GoImplement<ArgumentError, error>(Pointer = true)]
[assembly: GoImplement<Basic, ΔType>(Pointer = true)]
[assembly: GoImplement<Builtin, go.go.types_package.Object>(Pointer = true)]
[assembly: GoImplement<Chan, ΔType>(Pointer = true)]
[assembly: GoImplement<Const, go.go.types_package.Object>(Pointer = true)]
[assembly: GoImplement<Interface, cleaner>(Pointer = true)]
[assembly: GoImplement<Interface, ΔType>(Pointer = true)]
[assembly: GoImplement<Label, go.go.types_package.Object>(Pointer = true)]
[assembly: GoImplement<Label, positioner>(Pointer = true)]
[assembly: GoImplement<Map, ΔType>(Pointer = true)]
[assembly: GoImplement<Named, cleaner>(Pointer = true)]
[assembly: GoImplement<Named, ΔType>(Pointer = true)]
[assembly: GoImplement<Named, ΔgenericType>(Pointer = true)]
[assembly: GoImplement<Nil, go.go.types_package.Object>(Pointer = true)]
[assembly: GoImplement<PkgName, go.go.types_package.Object>(Pointer = true)]
[assembly: GoImplement<PkgName, positioner>(Pointer = true)]
[assembly: GoImplement<Pointer, ΔType>(Pointer = true)]
[assembly: GoImplement<Slice, ΔType>(Pointer = true)]
[assembly: GoImplement<StdSizes, Sizes>(Pointer = true)]
[assembly: GoImplement<Struct, ΔType>(Pointer = true)]
[assembly: GoImplement<TypeName, go.go.types_package.Object>(Pointer = true)]
[assembly: GoImplement<TypeName, positioner>(Pointer = true)]
[assembly: GoImplement<TypeParam, cleaner>(Pointer = true)]
[assembly: GoImplement<TypeParam, ΔType>(Pointer = true)]
[assembly: GoImplement<Union, ΔType>(Pointer = true)]
[assembly: GoImplement<Var, go.go.types_package.Object>(Pointer = true)]
[assembly: GoImplement<Var, positioner>(Pointer = true)]
[assembly: GoImplement<atPos, positioner>]
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<gcSizes, Sizes>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.AssignStmt, positioner>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.BasicLit, positioner>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.BranchStmt, positioner>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.CallExpr, positioner>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.ChanType, positioner>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.CompositeLit, positioner>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.Decl, positioner>]
[assembly: GoImplement<go.go.ast_package.Ellipsis, positioner>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.Expr, positioner>]
[assembly: GoImplement<go.go.ast_package.Field, positioner>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.FieldList, positioner>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.File, positioner>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.FuncLit, positioner>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.Ident, positioner>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.ImportSpec, positioner>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.InterfaceType, positioner>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.KeyValueExpr, positioner>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.Node, positioner>]
[assembly: GoImplement<go.go.ast_package.ReturnStmt, positioner>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.SelectorExpr, positioner>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.Spec, positioner>]
[assembly: GoImplement<go.go.ast_package.Stmt, positioner>]
[assembly: GoImplement<go.go.ast_package.TypeAssertExpr, positioner>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.TypeSpec, positioner>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.TypeSwitchStmt, positioner>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.UnaryExpr, positioner>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.ValueSpec, positioner>(Pointer = true)]
[assembly: GoImplement<go.go.types_package.Array, ΔType>(Pointer = true)]
[assembly: GoImplement<go.go.types_package.Func, go.go.types_package.Object>(Pointer = true)]
[assembly: GoImplement<go.go.types_package.Func, positioner>(Pointer = true)]
[assembly: GoImplement<go.go.types_package.Object, positioner>]
[assembly: GoImplement<go.go.types_package.Tuple, ΔType>(Pointer = true)]
[assembly: GoImplement<importDecl, decl>]
[assembly: GoImplement<indexedExpr, positioner>(Pointer = true)]
[assembly: GoImplement<lazyObject, go.go.types_package.Object>(Pointer = true)]
[assembly: GoImplement<nodeQueue, go.container.heap_package.Interface>(Pointer = true)]
[assembly: GoImplement<operand, positioner>(Pointer = true)]
[assembly: GoImplement<posSpan, positioner>]
[assembly: GoImplement<strings_package.Builder, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<typeParamsById, sort_package.Interface>]
[assembly: GoImplement<ΔError, error>]
[assembly: GoImplement<ΔSignature, ΔType>(Pointer = true)]
[assembly: GoImplement<ΔSignature, ΔgenericType>(Pointer = true)]
[assembly: GoImplement<ΔconstDecl, decl>]
[assembly: GoImplement<ΔfuncDecl, decl>]
[assembly: GoImplement<ΔtypeDecl, decl>]
[assembly: GoImplement<ΔvarDecl, decl>]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<Alias, ж<Alias>>(Indirect = true)]
[assembly: GoImplicitConv<Basic, ж<Basic>>(Indirect = true)]
[assembly: GoImplicitConv<Chan, ж<Chan>>(Indirect = true)]
[assembly: GoImplicitConv<Const, ж<Const>>(Indirect = true)]
[assembly: GoImplicitConv<Context, ж<Context>>(Indirect = true)]
[assembly: GoImplicitConv<Interface, ж<Interface>>(Indirect = true)]
[assembly: GoImplicitConv<Label, ж<Label>>(Indirect = true)]
[assembly: GoImplicitConv<Map, ж<Map>>(Indirect = true)]
[assembly: GoImplicitConv<Named, ж<Named>>(Indirect = true)]
[assembly: GoImplicitConv<Package, ж<Package>>(Indirect = true)]
[assembly: GoImplicitConv<PkgName, ж<PkgName>>(Indirect = true)]
[assembly: GoImplicitConv<Pointer, ж<Pointer>>(Indirect = true)]
[assembly: GoImplicitConv<Slice, ж<Slice>>(Indirect = true)]
[assembly: GoImplicitConv<Struct, ж<Struct>>(Indirect = true)]
[assembly: GoImplicitConv<TypeName, ж<TypeName>>(Indirect = true)]
[assembly: GoImplicitConv<TypeParam, ж<TypeParam>>(Indirect = true)]
[assembly: GoImplicitConv<Union, ж<Union>>(Indirect = true)]
[assembly: GoImplicitConv<Var, ж<Var>>(Indirect = true)]
[assembly: GoImplicitConv<ast.BasicLit, ж<ast.BasicLit>>(Indirect = true)]
[assembly: GoImplicitConv<ast.BinaryExpr, ж<ast.BinaryExpr>>(Indirect = true)]
[assembly: GoImplicitConv<ast.BlockStmt, ж<ast.BlockStmt>>(Indirect = true)]
[assembly: GoImplicitConv<ast.CallExpr, ж<ast.CallExpr>>(Indirect = true)]
[assembly: GoImplicitConv<ast.CompositeLit, ж<ast.CompositeLit>>(Indirect = true)]
[assembly: GoImplicitConv<ast.FieldList, ж<ast.FieldList>>(Indirect = true)]
[assembly: GoImplicitConv<ast.FuncLit, ж<ast.FuncLit>>(Indirect = true)]
[assembly: GoImplicitConv<ast.FuncType, ж<ast.FuncType>>(Indirect = true)]
[assembly: GoImplicitConv<ast.Ident, ж<ast.Ident>>(Indirect = true)]
[assembly: GoImplicitConv<ast.InterfaceType, ж<ast.InterfaceType>>(Indirect = true)]
[assembly: GoImplicitConv<ast.LabeledStmt, ж<ast.LabeledStmt>>(Indirect = true)]
[assembly: GoImplicitConv<ast.SelectorExpr, ж<ast.SelectorExpr>>(Indirect = true)]
[assembly: GoImplicitConv<ast.SliceExpr, ж<ast.SliceExpr>>(Indirect = true)]
[assembly: GoImplicitConv<ast.StructType, ж<ast.StructType>>(Indirect = true)]
[assembly: GoImplicitConv<ast.TypeSpec, ж<ast.TypeSpec>>(Indirect = true)]
[assembly: GoImplicitConv<ast.UnaryExpr, ж<ast.UnaryExpr>>(Indirect = true)]
[assembly: GoImplicitConv<block, ж<block>>(Indirect = true)]
[assembly: GoImplicitConv<declInfo, ж<declInfo>>(Indirect = true)]
[assembly: GoImplicitConv<go.go.types_package.Array, ж<go.go.types_package.Array>>(Indirect = true)]
[assembly: GoImplicitConv<go.go.types_package.Func, ж<go.go.types_package.Func>>(Indirect = true)]
[assembly: GoImplicitConv<go.go.types_package.Tuple, ж<go.go.types_package.Tuple>>(Indirect = true)]
[assembly: GoImplicitConv<indexedExpr, ж<indexedExpr>>(Indirect = true)]
[assembly: GoImplicitConv<operand, ж<operand>>(Indirect = true)]
[assembly: GoImplicitConv<token.FileSet, ж<token.FileSet>>(Indirect = true)]
[assembly: GoImplicitConv<ΔInfo, ж<ΔInfo>>(Indirect = true)]
[assembly: GoImplicitConv<ΔSignature, ж<ΔSignature>>(Indirect = true)]
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: global::go.GoPositionMap("go/types/alias.go", "alias.cs", "ADVyopSCqqCkgK7QqKCooKiigqqgqKCswoCCpKaigpSCgpSCAAcQgqaqooKq0oKCgoKCgriCpqzSgoKCgoKCgqbGlKY=")]
[assembly: global::go.GoPositionMap("go/types/api.go", "api.cs", "AD6AAaIACBKAooAAafgBwgB7iAKCqqKAgqSAgoCCxgACFPKAgqQAAhIACAKCgpSUggAKGqKokqqirLKUlKqiqqKqoqqiABMogoKCgpSUgoIAAhwACwKC")]
[assembly: global::go.GoPositionMap("go/types/api_predicates.go", "api_predicates.cs", "AAwmAAkGgpQAAhDSkoIAAhDSkq7ClLiClK7CAAIiAA4CgqqigoI=")]
[assembly: global::go.GoPositionMap("go/types/array.go", "array.cs", "AA8koKigppCkgKKA")]
[assembly: global::go.GoPositionMap("go/types/assignments.go", "assignments.cs", "ABMwAAgClJSkAAgOgoKmgtyCgoKCgqSCpoKCgoKUpoKCgpSkpJSCgpSCgpSCgtyAgoKC7IKmkoCCgpSU+LKCgpS4goKClJSmgqaCgqau8oKClIK4goKUgoKCgpSUpq70poKCrLKCgoC4gIKC6oKUgqaCypTYgIKCgoKCtoKm3NKCgoKUlKaClIKAgraCpoKUqLKClKiygpQABxTygoKCgoS0pMq4tqaCpoCCpLS2lKaCgpTmgoKClIKAgoK25oKSgoKCkoKUgoKCgv7igoKmyIKCyoKCgoKClIKUypSCgoKUuIKCpqaCgoKCuIKUyoKClLiCgt6yyIKCyoKClMqUgoKClKaCgoKCuIKUyoKUuKKCpoKCgoKCgoKUgoKmgoKCgoKU7oCClICClIKkyIKCgpS4goK4ppSCggAHEIKC")]
[assembly: global::go.GoPositionMap("go/types/badlinkname.go", "badlinkname.cs", "AAko")]
[assembly: global::go.GoPositionMap("go/types/basic.go", "basic.cs", "AEaYAZCmkKaQpICigA==")]
[assembly: global::go.GoPositionMap("go/types/builtins.go", "builtins.cs", "AB4uAAoCpoKCpoIABxCCgpSssoIAAiAAAxmCgoKCuIIABBCCgoKSgpSCgrj+goKAgpSClLSAgpTWtoLsgoCCgoCCgoKClIKC/IKCpoKCguyCgqSCgoKU2sqCgoKU+raCyIKUgqSC9qSCtpTIlIKCgpSUuIKmgoK4lIKklIKUpoKCyoKCgoKUgoKUlJSCgsqmgoKUgpTqxgALFIKCgqaClILagriCgsqCgoCClKSktpSCgoK4gpSmgqa4lIKCgpSUgoKmgoKmgpSC3IKCgoKCgpSCgpSClKaCgoKmgoIABhCCpoLulILugoKAgpSkpLaUgoKCgpSCuIKClKamgqbcgoKCpoKUxLSCpIKUgoKmgoKCgoKCpoKmgoKC3JSCgqaCgqaCgriCgoKmgoKmgoKm3IKUgoLKgqaCgoKU3IKCpoKCgu6UgpSClKaCgqaCgryigoKCgoKUuIKCyoKCgsqUgoKmgoKmgoKCyoKCpoKCgqaCpsqCgoKCgqaCgqaCgoKUguyClIKCuKaCkoKU7oKCgqaCgoKUgqa4goKmgoKCpoKCgpSCpriUgoKCpoKCpoKCgsqUgoKCpoKCgsqUgoKmgoKmgoKCypSCgqaCgoLugoKUgoKUggALFoKCgpSCgoKCgpSC2qaC3AAJCICCgIKkgpSCosikpIKC2MSkAAYUAAoCgIaigoKUgIKCpJSuwpSkpKSUyoKClLaqwoKClIKCgoKUqqKAgoCCxg==", "42-44:1;190-204:2;234-241:3;252-263:4;302-306:5;328-341:6;394-406:7;448-461:8;936-938:1;970-979:1")]
[assembly: global::go.GoPositionMap("go/types/call.go", "call.cs", "ABFGABgClIKCgpSUlpKCgoKCgoKU3IKSlIKCpoKCAAIaAAoCgoKCgoKUpoK4goKCypSCgoKClIKUlKaCgoLm4oKUgoKCgoLugoKCpqKUgJSCgpSUyObCgoK4lJSClLiUgoK4goKUgoKApLSCgoKClICCgoK2xoK0griCgpSUgpTKpoKCgoKCuJaSgoKCgoKCgoKUppKCgoKCggAHEIKCpoLKgpSUuJS0gpSUtIKkgriCpqrCgIKSlIKCgoK2AAIUAA4CgpSUgoKAlAAJEoKCgqaUgoKAlIKUgpS4lKaCgoCUgoK41pSCgoKCgoCUgpSClLi4pKgABR4AEQIAChaCgqaCgoKUlIK4uIKCgoKClIKCpriUgtyCgoKCgpSWkoKUgoKCgoKotoKCgqSCtLiSgpSCppaSgpSA7sqCgoKClIKmgpTIpu6CgoLKgpS4griClMqCgoKClIKCyoKCgrimABIW1MzKgIKCgIKCgoKUgoK4gpSUpoKCpoKClJSUgoKClJSCuLiUgoKCtIK0goKCxoKCgoLGgoLEgqSC2IKmgoLIgqQAECKCgqaClIKmlIKmgoKUlKaCgpSClIK4gIK2lIKCgqaUgoKCqqKCAAcQgpSCgqaCgu6olIKClJTaAAcQgtyC3ICCAAsWgoCCgtzIppKClMbagpSCggACEPCq0KSCgoKCpqaigoL6gpqygoKAuICCgtiCgtak", "140-143:1;155-168:2;361-373:1", "", "173=describef/1/1/13;995=NewTuple/1/1/2")]
[assembly: global::go.GoPositionMap("go/types/chan.go", "chan.cs", "ABc2kqiQppCkgKKA")]
[assembly: global::go.GoPositionMap("go/types/check.go", "check.cs", "ABgqAAgSABYsgoKCgoKUABtKAAgCgoCCtqiSggAZOMKCAD2GAZKCgpSAgqQAAhIACAKCgpSCqLKCgqiSgqaCgoKClAACEuKCgqiSgqiSgoKCggAGEqKq5IK4ggAJFJQADx74goKUgoKCgoLcgqaCgoC0gpSUpqbsgoKUlIKCyrimgAAKFsqmtriCgpTs0tbWxKbyuO6CggAPDgAKBoKClJSClKaCgoK4gpSClIKUgpSClIKUgoKmgpSUpoKmgoKCgoKCgoLY1gAJDoKCgoKClKaClIKmgoKopIKUqtzcooKUtNSklKS4", "475-480:1")]
[assembly: global::go.GoPositionMap("go/types/const.go", "const.cs", "ABUs0pS4gu6CgqiSgoKClIIAAyAAEAKCpoKCppKCppSCgpSClICClIKkgqSCpIKkpICCpKSCpIKkgqSkyICkgqSk2IKClJSClIKCgraClIKCgrakyIKClJSClIKCgoK2gpSCgoKCtqTIpqamgoKCpoKCgoKUpoKCpoKCgpSqwoKCgoKUgq7ygpKCAAcQgpSmlPaCgpSklKjCgoKCgpSCgpSCgpSCgg==", "76-79:1")]
[assembly: global::go.GoPositionMap("go/types/context.go", "context.cs", "ADZ0kgAFErKCgpSCuIKmpqrygpSCgpSUuN4ACQKUgpSCgpSUuNzY4oKCgoKCgpQ=")]
[assembly: global::go.GoPositionMap("go/types/conversions.go", "conversions.cs", "ABIowpSCgOikgoCCpIKUpKaCgqa4goKCAAkQpIKClIKUgpSUlJSUxoK2goKUlILcggAIEqSCkoKUpgAGIgATBICCtoKCgoKCgriC3ICCgIKC/IK4griCuILKgqaCyoCClIKCppSUxoCCgoKmgpQACQ6CppKCgoKU3JSSsoKUgrKClIKClMiSsoKUgoKClLaigpSCgpS4poKCpoKCpoKCpoKAgoKk", "23-40:1;62-81:2;237-245:1;252-267:2;257-266:2.1;270-280:3;282-291:4")]
[assembly: global::go.GoPositionMap("go/types/decl.go", "decl.cs", "ABIi6oKAgoKCgoKklIK6koKCgpSU6uKCgpSCgoKCAB4+goKmtLiCgriCuAAMGpSCyILI3MgACxSkgqaCgoK4gpTulIK0gsaCxsTEAAoIAAgEgoKCgoKCyoKCgoKCgoKCtOqCggACGAAJAoKUgIKUtoIACQrogoKClJSCgpTKuILcgriC6PqqgoKmgsqCgpSUpoKClKaCgoKUqqKSgoKmAB4qgKKAooCigKKApIKCuILYgoKUtKaClIK0gqSCpIKkxsToxMjSpoKClIKmpoKCpoKUgpSokoLulJSi1KKmggALGIKUlKaCgoKCgqaUggAHEIKCuKiSgoKUggAIBgAIAqaUgoKAgraokoK4poKUgqYACBKCpoKCgpSCgqaCgoLulIKCpoKClLiCpoKUgoKCuIKCuIIABxCCgrTU4riCgoLuAAgSgoKCpoKCpoKCyoKmlIKUtKS4gpS0pIKClICCpJSmAAkOgoKCpvqCgpSClsiCgriS3IKCgsqmgoCCgpSUtoLKgoCCgoKCgsqCgoCmuIKCggAIDsKmlIIABxCCgoKCuIKUgsqCosqilJKUpoKCgpSCgqa43IKCyJSCgriCgoKmtoKkgqaC3IKCpsq4gpTIuIKUgoLE", "58-61:1;103-105:2;171-173:3;271-277:1;306-308:1;436-439:1;548-554:1;682-684:1;770-772:1;867-869:1;876-973:1", "", "687=Name/1/1/6,describef/1/1/6;937=describef/1/1/2;1045=describef/1/1/2")]
[assembly: global::go.GoPositionMap("go/types/errors.go", "errors.cs", "ABgkgoKmgIKkABMqkoKUAAISAAgCqJKAuMiCpoKClNiSgqaCgoKCgoKmlPiSggAHEIKUgoK4gtyCgoKCgsqCgoKmuKiSlO6CgriCuIKCgIKU7KaCAAkUyoKCgqaCpoKClAAKGoKCgqaigoKmooKCgqaigoKC3IIACxiCrLKSgpSsspS0pIKkgoKUtII=", "", "", "215=stripAnnotations/1/1/3")]
[assembly: global::go.GoPositionMap("go/types/errsupport.go", "errsupport.cs", "AA8eAB42AAYSgoKCgoKCkoKCkoKkgoKkgriClKSkpKaClLSklKSkpLiqooKCpg==")]
[assembly: global::go.GoPositionMap("go/types/eval.go", "eval.cs", "ABAwAA0EgoKmpoIAAioAGASSgoKSgtyCgIK2goKCpoLcgoKCppKCgpQ=")]
[assembly: global::go.GoPositionMap("go/types/expr.go", "expr.cs", "AD5+lAAHEKKAgoKCpoKkqqKUpLS8opSCxoK2psoACxLSgoKmgriAgoKCpIKCpoKCgoKUgoKCgpSCgoKmgoKCuIKCgpSCpoKCpoKUlIKClIKCgqa4gqaUlJQABRgACwKCgrgAEiSCgpQAERoACBCClLaClLampoLY2oKCgsqUuIKC3JSSgoLKqJKAgoIABBQACAKCuJSAgqSmpIKCgpTclIK2guyCyIKmpMaCgoKUgpSmgpTcgqaClPSCprSUqNSCgqaCpoK4goKClLiCgriCpqaCgpTKgsiCgraCgtqUgrSCyLiCuMqCyoKUhJKUgpSmgoKUpoKUlKqitJaSkpSo5oKCpsqCggAFEKKUgoKCgqamgoKCypSCguqCgoLGgoK4gpSClIKUlpKCgoKC3IKmgoKCuAATKICCgraCyoKCgqbKlAAOJsKUgpSClIKCgqaCgqaCgqaCgqamgoKClIKUpoKmgoKmlIKCgriCgoKCgoLKlIKUpoKUgoKCprwADhSUgtyCpoKmgqaClIK4gpSmgoKClIKCggATMKKCgIK2AAUUAAsCgoKCgoK4lIKmlAAJDNKClIK0gsaCgoKUtoKCggAIDgAJBoKUlLbagraCgsiCgsiCgtqCgqbWgoKClJSCyIKCyIKCpqaClIKClICCgqSCgpSCgramgpSkgqSCgoKCgpSCgpSClJSCyIKClIKCyIKC2oIACQiCAAkQyIKUgoKCAAIWAAgCpIKCgoKUgqSCgoKUgqSAgqSAgsaklKiygoKmgoKmAAIQ0oKCqJKCggACEAAIAoKClICUgoKUyIKCgoKUgqassoKCgq7CgoIACArCgoKClIKUlKSCpIKklIK6spSAgoKC", "412-418:1;584-586:1;859-892:1;946-949:1;1100-1112:1")]
[assembly: global::go.GoPositionMap("go/types/exprstring.go", "exprstring.cs", "AA4ksoKCAAwMAAkMpLa2goLItoKCtoKCgpS2goK2goLWgoKCgraCgoKUgoKUgoKCpraCgoK2goKCgpS2graCtoKCgoK2goKUgraCgraCtoKCtoKCgraClKSklILIooKClIKClKaClIK4goKmooKCuKaAgoLIgqbcooKClLiigoKU")]
[assembly: global::go.GoPositionMap("go/types/format.go", "format.cs", "ABQkoqKUtLS0gsa0goKCgrS0goKCgrSCgoKCgoKUlIK0goKCgoKClJSCpJSowoKCgoKUprIABRDCprSCgoKCpoKUlKrCgpSUgoKClJSCupKClIKmgpQ=", "", "", "123=Position/1/1/1,Repeat/1/1/2,sprintf/1/1/3")]
[assembly: global::go.GoPositionMap("go/types/gccgosizes.go", "gccgosizes.cs", "AAsa")]
[assembly: global::go.GoPositionMap("go/types/gcsizes.go", "gcsizes.cs", "ABAe0oLKyKQABxDcgoKAgrYACAyCtoLmpJSCpoKUgpTmgoKCgpSCpoKCgoCClLamoqSCgoKAgraCxoKCpoKClIKYooKUpKSCgpSCgoKCuIKmyILEpKyygpQ=", "16-18:1")]
[assembly: global::go.GoPositionMap("go/types/index.go", "index.cs", "ABIm4qaUgriUgoKUpoCU2oKCuIKClIKCgsqCyIKCgpS2gIKCgoLYgoK2goKClIKClIKCgqaClqKUgoKSlIKCxoKCgsaAgoLWtIKkgpSUgoK4gqaCpoKUpoKCgoKUgoKUgoKCuIKCyJSCgoKmgoKC3IKmgtaygoKCpoKClIKCpoKCgoKUgoKUgoK4gtqCgoKCgpS2gIKCgtjIgoKCpqaCgoKokoKCyoKClICC6MakyoKCgoK4goIACBTSgoKUlJTe8oKUgoKCpoKmgqaCgoKCuKaigriCgriCgqaUgoK4goK4ABMogqaClAAIEAAHEA==", "112-161:1")]
[assembly: global::go.GoPositionMap("go/types/infer.go", "infer.cs", "ABxGABQKgpK4goKSyoKmpoLKgoLcgoKCAB4+goLclLSCuIKCgoKmgoKmlMqCgpSmutqCgqaCypSCpoKCgqSAAAoW2oKC3IIAESSCgoKClKaCgoKCypSUggALGLiCxoLKABEigoIAARDigqSCyoK4goLcgqyygoKCgoKUgoKUgoKClJTKgoKCAAsYgoKCABo2mrKCgriCgsqCgoKCgAAMGoCCpIKCtu6CgoKCqAAFFAAkOIKmgoKCgqaCgqbatIKUpKSokoKClJSCgqyyyAAHEOSAgqSCgqbapqampgAHEAAJFKaCgoKmqqamgoLatrbmgoKCpq7igoKCgoKClIKCgpSUgoKUlIDKpAACFPKSggAJFMKCuICCgJTqlIKUAAcQtra27oKUgsiCyIKUgsiCtraCyICC6MSkgoI=", "41-43:1;48-50:2;119-154:3;305-305:4;567-569:1;615-617:2;659-670:1")]
[assembly: global::go.GoPositionMap("go/types/initorder.go", "initorder.cs", "ABgo1riSlIKCgoKUgIKCgoKmyJSCgoKCppQACBKClJSCyoIACBKCAAgSgoK4goKC7oKUlIKClIKmgoKCgpS+soKUlpKClKiCgpSAgsiokqaCgqaClIKCgpQAGjiiyoKClKzEgpSAgv6UlICCgoLakoKAgpQADhyGppSmlIKCpqaCyoKCpgAJFICkgoKCpoKmgoKCyqaCpoKCgoKC", "151-153:1;273-275:1", "", "84=Name/1/1/1,order/1/1/1")]
[assembly: global::go.GoPositionMap("go/types/instantiate.go", "instantiate.cs", "ABpuAB4CgoKUlIKCgoKUgJLIggACIgASDNKClIKUuIKC3LKClMqCgILIlLaCuILKlIKmtpSUgpSClLiCkriC2MissoKUtLSmgoKCpqiSgpTKgoKCpgAGEgAIAoKCgpSAgraCgqaCgoKCgpSUlLiC3IKCyoKClLiSgpS4koK4griUgpSClJSClMqC3IKUgpSUqJKCuIKSgoKmlJSCgpS0tKSUpqqilIKC2IKC6IK2", "116-121:1;296-321:1;345-360:2")]
[assembly: global::go.GoPositionMap("go/types/interface.go", "interface.cs", "ABs6sNae0oKClAACEuKCuIKCgILalIKClKiygoKUrsKokKigppCswKaQppCooKaQppCooKaQAAISAAkCgpSCpoCigKrCgoLm0oKCgpSmgoKCyoKCgqaCgoKClLiCgoCCpKiSgoCCtpSCgsqUlIK43II=", "160-166:1;231-233:2", "", "284=describef/1/1/2")]
[assembly: global::go.GoPositionMap("go/types/iter.go", "iter.cs", "AAkoAAsCgoKCAAUUwoKCggAFErKCgoIABRKygoKCAAUSsoKCggAFErKCgoIABRKygoKCAAUSsoKCggAFErKCgoIABRKygoKCAAUSsoKCgg==", "21-27:1;35-41:1;48-54:1;61-67:1;74-80:1;87-93:1;100-106:1;113-119:1;126-132:1;139-145:1;152-158:1")]
[assembly: global::go.GoPositionMap("go/types/labels.go", "labels.cs", "ABMexJTcgoKCgoCCgoKUgqS4ooKAggAMHNKCgpSCgoKUqqKCgIK2qqKCgIK2rLKUAAYQgoKmmLamgsKUgILqgIKCgIKCgoKCpoK2goKUgoKCAAkWgqaCpLaCuILKgoCC1MaCgtyCgIKkxoKCuJSCuIK4goK2gsi2goLItra2tra2yIKm", "107-110:1;112-114:2;116-120:3;123-264:4", "", "182=Position/1/1/5")]
[assembly: global::go.GoPositionMap("go/types/literals.go", "literals.cs", "ABIq0oKCpoKClIKUgoKClIKClIK4spQAABQACQKCgoKmgsqCgqaC1sKApoKCuILKsqaClIIADgjCkpTKgLiCgqSCyIKUgIKkyJSCtsiCgoKUgsqCgJSCgoKCgpSmgoKClIKCgoCCpIKClIKCgpSCgpS4goCCgqSCgoKmgoKClIKUggAKEoKCgpQACBKCyoL+goKClNqCgoLKgoKCgoKClIKCgpSCgoKCgoKCppSClIKCpoL8ooC4pKaCgoKUgoKUgoLIgt7CgpKUgoKAgoCCgoKUtpKClMiCgpSUgoKokoKU", "99-101:1", "", "113=describef/1/1/2")]
[assembly: global::go.GoPositionMap("go/types/lookup.go", "lookup.cs", "AAxYAB4CgpSqAA0QgIKAgoKAgqTYnMKCgIKCgILYAAIiABMGgtymgoCC2p72gqaCuICCgNykpoCmgoKUgoLYpoKCgoKClIKCAAoWgrjsgIKCgoKUgurcgJSCtqamAAocsoKmgoKCgIKmgoK2ppSAgraCgrgACRSCgoKmgoKmpoKCgoKmgpSCAAIYAAkCAAIYAAsCgoKmAAgWgoKUgIKCopSCgpSCpoKCuKKmgpS0tIKCgoKCpsi4goKCuIKmgoLagqaCpoKmlJS0tMaSpLSSppQABxCClLSkpKS4AAIS4oKUgqqigIKClIKCgsimgoKokoKAgqSqwoKCgpSCgoKCAAISAAkIgqYAAhAACAiClKyygJSCgpSUpKqigIKAgsaqooKCqqKCgoK4qqKCgoK4")]
[assembly: global::go.GoPositionMap("go/types/map.go", "map.cs", "AA4gkqiQppCkgKKA")]
[assembly: global::go.GoPositionMap("go/types/methodset.go", "methodset.cs", "ABgsgoKmgoKClIKokKaQprKCpoKCgpSCgoKmAAYaAA8WgIK4pKaCuJ72gpaSlIK4gIKA3KSUgsiUgoKU3IK42uyigJSClIKU7IKAgoKUyKaCqJKCgoK4lgAHFrKClIKUpqKClJTKgIKCtoI=", "49-52:1;204-206:1")]
[assembly: global::go.GoPositionMap("go/types/mono.go", "mono.cs", "AFCsAf6CgpSCgriCgqaCgoKCpoLKooLcgoKC3ILKgoKUgoKClJS0tLaqooKUqqKCgoKUugALEoK4goKCpqqikrS2graAgraCgsj4tIK0tLaCxoKCpoK0gsiqwoKCpoKCpoCCtriCgoCCgIKCgqb8gpSCqLKAgraUgIK2gqaCgoKmlA==", "201-208:1;213-263:2;251-255:2.1")]
[assembly: global::go.GoPositionMap("go/types/named.go", "named.cs", "AJMBvAKygpQAAhQADAKCyoKUgqaCgpSCgpSCgpSCgpSUAAkUgoKUlIKCgoKmgtiSqqKowoKCpoKUAAISAAoClILugpSClIKUpqIACBKUgtakqqKClKrCgpSqoKjCgqiSgpSokgACHAAQApSCpoKUgpSCgqaCgoK4goK42uaClJSCpoIACBKCpriCgoKClKamkqaCgpSmgqrCgoKUgpSCgr7igoKCgr6ygpSCgoK4goK4AAIQ5KaAAAI0ABoCmrK41ra2gsqClIKUgoKCgoKAlIKCpIKUgsTG6MqClKamsoK4gJS2qJKClOrigoKCgpKCuIKCpoKUpqaUyoKClIKCgpS4gIKAuIKCgoKCgoKUgqaC6t7CgIKk", "637-640:1")]
[assembly: global::go.GoPositionMap("go/types/object.go", "object.cs", "AECWAYKCqsKC7qaClAAcNIKUpKS8ooKUqqCmkKigppCmkKqwppCkgKKAooCigKSgooCigICigICigKS0gtyCpoKmAAIaAA0CgriClIK4goKCgpS4gpSCpgAJGKKqoAAIFKKokKQADSTyqqKCgqiylNqCAAcQpKS0AAweoqiSrLKqoKaQppAAAhIACQKClKYADRqigoLuqJKCAAkUqqKCgqywAAISAAkCgpSuwKbagIKCAAgQpgALFJIACRSCAA8QooKUlIKAgqSmtoKCgsiClMiCgoKUpoK2graCtramgpSUgqaCuKSCtoKCgIK0gIK4/oKCpoKmooKUgoKUlIKUrLKCgqaAooCigKKAooCigKKAooDUooKCgIKCgMqUpIKSgrY=")]
[assembly: global::go.GoPositionMap("go/types/objset.go", "objset.cs", "ABIwwoKAgqSClII=")]
[assembly: global::go.GoPositionMap("go/types/operand.go", "operand.cs", "ACROABs8tIKUAAlOACUEgoKUpKS4griUgoKUlKSkyoKCuILqgoKCgpTapoKAgoLagoKCgu6CgqSClJTagpSCgpSCuMqCpgAMDLKUpKSkpKSkpKSkpKTIgqiSgpSkpKSkpKaCgoKClIKCqJKClAAGFAAJAoKmgoKmgqaCgoKmgoKmgoLKgqaC7oLcgIKCuIKUgtqAgpSClP6AgoCC6oKmkoKCgpTcgoKCooKUgoKClJTcgpKCgrKClIKCgoKUlKY=", "337-346:1;402-410:2;417-427:3;438-449:4")]
[assembly: global::go.GoPositionMap("go/types/package.go", "package.cs", "ABg4ooKokKaQppCswKzigpSqoKaQAAIWAAkAqKCkgg==")]
[assembly: global::go.GoPositionMap("go/types/pointer.go", "pointer.cs", "AA4gkKaQpICigA==")]
[assembly: global::go.GoPositionMap("go/types/predicates.go", "predicates.cs", "AA4kkKywooCigKKAooCigKKAooCigKqyggACEtCigKKAooCigKKAooCswoCCpKyytJSssqSUrNaCqqKqxoKokoKokqiSgq7CgIKCpKzEgIKkgqiSqqKClIKUlMjEpIKCgpSmpIKClJSkgpSCgpTaqJKk9KSYqNSCpgAIEqIACBSygpSCpoKmyoCC/ICm6oCCAAkQgIKCgoKIpvyAgvyAgoKCgoKCuOqCggAIEoLKgpSmgpSCgpSUgqaCgoK4gqaqgKaCgoIADBaAgoKCgpSClIKCABYugoKClJSCgpSCgoKm/ICC/ICCAAgOgLiCgoKUgoKmAA4UtqikrLKCpqzWgIKUpKSkpKS2ruaClJSClJSo0pKokoKCpg==", "62-62:1;220-222:1", "", "383=Tag/1/2/1,Tag/2/2/1,sameId/1/1/2,identical/1/1/3;470=identical/1/2/1,identical/2/2/2")]
[assembly: global::go.GoPositionMap("go/types/recording.go", "recording.cs", "ABAkpqKClKSkgqSUlKaU6KKCpoKCgpS4goKCgpSCppSAgqSmyoKCtKTE3qKCgoKUgoKAgoKCgoKolIKClLYAAhLigoKCgILIgoKAgsiCgoKAgsiCgoKAgsiigoKAgsiCgoKAgg==", "", "", "129=NewVar/1/2/1,NewVar/2/2/2")]
[assembly: global::go.GoPositionMap("go/types/resolver.go", "resolver.cs", "ACJKoqiSgoKClN4ACAKCgoKmgqaCxpSCuNiC+IKCgpSClIKCgqbq0riCgsqCgqaCgqiSgoCCpNb8goKCuIKClIKClpKAgpKAgoKCpoKC6oKClIKCpoKAgqSAgqSmyoK4gpS4AA0M0gABEOKClp6CgrjKkoCCpIKCypSypoKUgoKCpoKCuIKCgpSCuIKC3IKCpoKUlKaUuIKUgqbM3ICCgoKClIL+2IKUgoKmgsiKwri4goKUgoSSgpSmxoK0goKClIKmgoKClIKClICUtpSClJSmAAcUuICCpJSCyoLsgqKAgoKCgIKClJSkAAkSgqaCgpSCgoIABxwADgSCgIKCyKSCgoKCgpT4xKSClNoAAhD4sqaCgriCgriCuIKCyoK4gpSmpoCUgpSC/oKCyqjEgoKCgpSqgoCCyAAQIoIAAhDSlIKAgoKUpsiCpoIABxCoxILugoLKAAgOgoKAgqSClAADEMKAgrY=", "261-462:1;633-635:1")]
[assembly: global::go.GoPositionMap("go/types/return.go", "return.cs", "AAwisqQAEBC4gILYpoLIpobIpqaCgoKopoLIppSCgIK2pqKCgoKClIKmAAIU8qQAERCmgoKUgtqmhMimgsiCyKaCyILIgsimgoKCpg==", "", "", "61=isTerminating/1/2/1,isTerminating/2/2/2;165=hasBreak/2/2/1")]
[assembly: global::go.GoPositionMap("go/types/scope.go", "scope.cs", "ACBKwpSCgpSokKaQppKCgoKClIKokKaQqKIACBKClAACEPKCgIKk3IKUAAIUAAkCgpSCpoKClAAFEPKClJSCgqaCgriokoKCAAscooCCkpSAgqSCpoKUpqSqoKKAooCigKKAooCigKKAooCigKKAooCigKKAooCigKKA", "178-192:1")]
[assembly: global::go.GoPositionMap("go/types/scope2.go", "scope2.cs", "AAwwAA4CkoCCtq7AooCqsgACEAAJBoKCgILagoKCppQ=")]
[assembly: global::go.GoPositionMap("go/types/selection.go", "selection.cs", "AE6oAZCmkKigqKK4kpKCgu6SkoKCgoKUgrgAAhoACgAAAhLwpIAABxYACwKClKSkpJSCgoKCgoCCgpSk")]
[assembly: global::go.GoPositionMap("go/types/signature.go", "signature.cs", "ACVS4gACEgAIAoKCgpSCgIK2goKClJSCgpSUAAIS4KaQppCmkKaQppCkgKKA3AAMAoKCgoKWkoKUgILIgri4gpS4gqaCgpSClIKCgoKi2gAMDpaSgtzKgoKCgIKCgqSssoKAgpTYgvq2ggAJEoKCgriClLiCgoKCgoK4poLKgoKClIKmgqa6koCCgpS6opTcgsqipqaCgoKClAADHAAKAoKClLSmgoKUxAAIEAAJAoKmkoKCgIKCgpTIppSCgqaUgpSmgoKCgriC7oKCgqaosoKCAAgO5IKCgsqUgoKUgraC5tikgsbEAAgKkg==", "303-305:1", "", "361=describef/1/1/2;572=File/1/1/1,Name/1/1/1,Base/1/1/1,HasPrefix/2/2/1")]
[assembly: global::go.GoPositionMap("go/types/sizes.go", "sizes.cs", "ADNq0oLKyKQABxDcgoKAgrYACAyCtoLmpJSCpoKUgpTmgoKClIKsgoKCgpSCpoKCgoCClLamAA8goqSCgoKAgraCxoKCpoKClIKmgoKCpoSSgpSkpIKClIKCgoKUyILEpLgAFDbilICCxoCCtsiUgoKClICCpKaigpSCgpSUgqYAAhDSgoKCgoKUgoKUlKqigoKUrLKC", "54-56:1", "", "144=Pkg/1/3/1,Pkg/2/3/2,Path/1/2/2,Pkg/3/3/3,Path/2/2/3")]
[assembly: global::go.GoPositionMap("go/types/slice.go", "slice.cs", "AA4gkKaQpICigA==")]
[assembly: global::go.GoPositionMap("go/types/stmt.go", "stmt.cs", "ABom8oKmgsqCgpTulJSCpoLKoqSygqKCgIK2hoKmpoIAEyyCgriCgoCCtqaCgoKCgoKClOiigoKClILGgtakgoKU3IKCgqaCppSClOaCgoKClIKCgraCpKSUqKSC3JSAgqSAgsaAgsaUAA8iooKCgoKClIKCppKCgpSCpoCmgoKCgoKCpvykgIKCpAACMAAYAoKCgpSCgpSCgsqClIKClIKCgoKmgoL8goKCuIKq0oKCgoSSgoKClIKClLiAlIKClIKCgoKkgoL8goKCuIIAHQjkgpSAgqTKppT6toK8soKCggABEIKkggACE4KUggABELaSgoKClIKCgpSCgoKUgoKUtoKUpKSCpoKCgpSCgqaCgoKUtpSCgpSCpsqCgpSCgoKUgoKClMi2tqa4goCCgoKC2oKClMiCgpSUgraCtoKClLS0pLbIgpS2gpSCgoKClKb4tNiCgpSCgoKmgoKCyoKCgqaUgoKCgoKUgoKCgpSUgsiCgpQAARQACAKClLSCgqaCgoKmlIKUpsaCuIKCgqaCgoKCgoKSgpTKlIKCgoKCgqaClIKCgriUgu6CgoKClJSC2pSUgoKCuIKCtLSCxsiCgILIgoKmgoKUgsiCgpSCgoKCgqamgIK4pLaCxsT05IaCgoKCupKWopSGlLS0tLSkyoLcgpSUhJKCgqiSgJSCgpSCpoKkpoKUgoKmgoKUgoKCgpS4goKCpqSkgoK4goKmgoK4gqaCgoKCtu6mogAODgAKApiClKSClIKClMakpKSClKSCppSklJSClKS2gpS2lIKUgpSU", "30-33:1;66-68:1;412-418:1;879-879:1;896-898:2;1033-1035:1")]
[assembly: global::go.GoPositionMap("go/types/struct.go", "struct.cs", "ABc2woKCgqaClIKCqJCmkKaSgpSmgKKAqqKCAAkIwoKCgqiSlqaSgoKClIKmgoKUgoIABxCSgoKmgoKClILugoKCgoKCgpQABxCClKKClJSmgsa0uIKUgv6CgqaClLaAgtakpJSmooCCgoKCgqSmooKCgIK2lA==", "85-101:1;107-111:2;146-172:3", "", "166=describef/1/1/26")]
[assembly: global::go.GoPositionMap("go/types/subst.go", "subst.cs", "AA8oooKCgpSqooKCgpSmgqaCgIKkAAIU8pSCuJSkuP4AChaipgAJEIKCgriC7oCC2IKCyIKCyICCgoLYgoLIAA8glIKCggANGoC42IKCgoKUgpSCgoKCggANHKaClMiCgoLIgoIACRKCgoKmgu6Ayti2tqyygpSmooKAgramopKCgqaigoCCtq7igoCClIKUtqaigoCCtqaikoKCpqKAgqQAAhIACQKCgoKCuIKClJKCpg==")]
[assembly: global::go.GoPositionMap("go/types/termlist.go", "termlist.cs", "ABQqzJKClIKCgpSUqMiCgqaoyIKCpqi2goKigpSCgoKUgO6ClIK2lKiSqJKCqqKCgoCCyKikqJKCgqaokoKCpqiSgriCgqY=")]
[assembly: global::go.GoPositionMap("go/types/tuple.go", "tuple.cs", "ABIksoKUqLKClKiQpICigA==")]
[assembly: global::go.GoPositionMap("go/types/typelists.go", "typelists.cs", "AA4eoKaQqtKClP6SgpSqoKaQqtKClKyigpSCgpSU")]
[assembly: global::go.GoPositionMap("go/types/typeparam.go", "typeparam.cs", "ABMmoAAOKOKo1JKCgpSCgriCkoKUqJCooqiSAAIS4oKUpq7CpoCqooKokpaSlJTGlJTIgoKCuJSUgIKkpqyyrsI=")]
[assembly: global::go.GoPositionMap("go/types/typeset.go", "typeset.cs", "ACJIkKaQppCmkoKUvJCooKaS1oKUpKaClIKCgoKCpoKClJSClIKUgq7AprCqsoKCpoKUgoKUgpSCAAQQsoKUgoKCptjG4oIADBqCpriCpoKCgoIACBKUgoKClJQADyCCgoKSgKSCtIKCgoIACRCCooKCgoIACA6CuIKChqKClIKCtoKUgpSCgpS0gpSCgpSCgsSClIKU7KaCgoKUlAAFEuKmgpSCgoKCgqaCgqaCpoKmgqaCgpSCAAkWwoCCyJSCooKCgJSCkoKUppTIgoKClIKmlA==", "49-51:1;184-187:1;223-252:2;242-249:2.1", "", "278=describef/1/1/7")]
[assembly: global::go.GoPositionMap("go/types/typestring.go", "typestring.cs", "AB9GooKUgoKUvrKCgqyyrLIADRyCpoKCpoKCgpSClIKCuIKmgoKUAAwGwoKClIKUlNqCgIKCtraCgoK2graCgoLKgpSC3IKClIKCgoKUgIK4traCtraC2oKClIKClIKUyILKgpSCgqaCgoK4lIKCgpSCgpSCgpSCgpSCpraCgoK2goKUlICCxqSklIKClIKC7IKUgpSSlMiCgpSAuJSCgtyCgrj8goCUkpSklPzEpsKCgoKClIKClNi0goSSgpSClIKClMiigoKClJTWooKCuIKClIKUgpSUgpSCgpSmooLWsoKCgoKmgoKUgoKAgoK4gIKCpIKCtqam0oKCgoKCpqaUgpSmgpSCuKKmkoKCgoKCgoKCpg==", "39-44:1;481-483:1")]
[assembly: global::go.GoPositionMap("go/types/typeterm.go", "typeterm.cs", "ABQqopSkpKS61JSkuKjUlKSkpKS4ggAHEIKUqNSUpKS4ggAHEIKUqMSUpLiCgpSo1JSkpKS4ggAHEKrCgpSCgpSCgpQ=")]
[assembly: global::go.GoPositionMap("go/types/typexpr.go", "typexpr.cs", "ABUqAAgCgpSClIKSgpSkgsqCgqbKgoKmgJKkAA0cgoKClNyAgraUgqaCgpSCgoKUlJSCtoKClOyClIKClLaCtoK2xraqoqyygoKq1ILcgoCCgoKClAAHFuKCgoKClIIAAhQACgKCgoKClKaC2qIACArygoKCgoKCppSClMragpSUgoLYpMiClJSCgtik6IKCyqaCgoKCpoKmgIKClKSCgv6CtoKCgqaCgoLKgpSmgoKCpoKCgqaClIIABxCSgoKClLimgpSCyKSkuIKCtoK2goLmooKUtLTEAAwK4oKCgoKUuIKmgoKClIKmgIKkpoKC7oKCgrjYlIKClICUgoKUlNoACgz6gIKCgoKUgIKC2IKCgoKUpoKAgoKAgvyCgpSUgqqigoKCgpSCpg==", "174-185:1;236-249:1;375-383:2;437-441:1;444-446:2;479-500:3", "", "200=describef/1/1/11;437=describef/1/1/8;576=describef/1/1/21")]
[assembly: global::go.GoPositionMap("go/types/under.go", "under.cs", "AAkcwoCCpKqigoKClAACEuKAgoKkAAIS4oKCgpSCgoKCuIKUrNaCpoKCgoKUgoKUgoKCgoK4gpSClAACEOSCuICCgKaUpPw=", "25-28:1;54-68:1;84-103:1")]
[assembly: global::go.GoPositionMap("go/types/unify.go", "unify.cs", "AFO2AcKCyoKCgpSUABgqgpSkpKSUrLKmoqq0goKCgpSUgoKCgoKUgoKUgsqAooCigKqygpSA+rYACA60qqKAgoCCxqqigoKCgsySqqKCgpSokoKCgqauwoKClKrCgIKkrgAMAoKClIKClLiCuIKClIKU7oKClAATKICCgpSmlIIAChSAtoKmuICUlIKCgqa4ggAJFIKkygAVLILqxoCU2JS2gsjupoKmgoKCgriCABcwgoKClLiClIK4goKmgoCCtriCgtyUgoKCgIK2AA4egoKUyoKCuJKkyoCC/ICm6oCCAAkQgIKCgoKIpvyAgvyAgoKCgoKCuAALFICC3NyAgoKCgpSClIKCABYugoKClJSCgpSCgoKm/ICCAAgOgIL8gLiCgoKUgoKm6oIADRzugIKC3AAMErY=", "290-295:1", "", "683=Tag/1/2/1,Tag/2/2/1,sameId/1/1/2,nify/1/1/3;730=nify/1/2/1,nify/2/2/2")]
[assembly: global::go.GoPositionMap("go/types/union.go", "union.cs", "ABYuooKUpoCigKSAooDckKSAooCigAALFMKClJSCgoK4lIKCgqaCpoK4gtyigoKmgoKCgoKmgoIABxCCgpS0tKTKgILa1qKCgoCCgqTcgoKUlJSCgpSu4oKCgriCgqaCpqrSgIKCgqQ=", "91-134:1", "", "118=describef/1/1/43")]
[assembly: global::go.GoPositionMap("go/types/universe.go", "universe.cs", "ACZUABw+AAgKgoKUggASJoKCpsqCgoK4goKCpoKCgqaClIK4goKCppSCAAgIAAUUgoK4ggAtVAAfToKCgoKU7rKClIKmgoKClIKCgpSCgoKCgqyygoKCpoCCtoKClJS0xLaC", "", "", "156=MakeBool/1/2/5,MakeBool/2/2/6,MakeInt64/1/1/7")]
[assembly: global::go.GoPositionMap("go/types/util.go", "util.cs", "ABI4AAgAppCmkKaSgoCCtqiwppCmkKaS")]
[assembly: global::go.GoPositionMap("go/types/validtype.go", "validtype.cs", "AAwgwgAFGgAOApSCgJKkgoKCuLiCyKaCgtqCgtqCggAbOILKggAVLIKCgu6CgoKmAAcQggAKFICCpqaC7riC/NqigoKU", "39-41:1")]
[assembly: global::go.GoPositionMap("go/types/version.go", "version.cs", "ABMooqiSqqK6goKCgoKCgoKmnLKq0oKClA==")]
// </GoSourcePositionMaps>

namespace go.go;

[GoPackage("types")]
public static partial class types_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface cleaner {}
    internal partial interface decl {}
    internal partial interface dependency {}
    internal partial interface instantiatedType_type {}
    internal partial interface positioner {}
    internal partial interface ΔgenericType {}
    internal partial struct @object {}
    internal partial struct _TypeSet {}
    internal partial struct action {}
    internal partial struct actionDesc {}
    internal partial struct atPos {}
    internal partial struct bailout {}
    internal partial struct block {}
    internal partial struct builtinId {}
    [GoLocalName("methodInfo")] internal partial struct collectObjects_methodInfo {}
    internal partial struct comparer {}
    internal partial struct ctxtEntry {}
    internal partial struct cycleFinder {}
    internal partial struct declInfo {}
    internal partial struct dotImportKey {}
    internal partial struct embeddedType {}
    internal partial struct environment {}
    internal partial struct errorDesc {}
    internal partial struct error_ {}
    internal partial struct exprInfo {}
    internal partial struct exprKind {}
    internal partial struct gcSizes {}
    internal partial struct goVersion {}
    internal partial struct graphNode {}
    internal partial struct ifacePair {}
    internal partial struct importDecl {}
    internal partial struct importKey {}
    internal partial struct indexedExpr {}
    [GoValueClone("buf")] internal partial struct instanceLookup {}
    internal partial struct lazyObject {}
    internal partial struct methodSet {}
    internal partial struct monoEdge {}
    internal partial struct monoGraph {}
    internal partial struct monoVertex {}
    internal partial struct namedState {}
    internal partial struct nodeQueue {}
    internal partial struct nodeSet {}
    internal partial struct objset {}
    internal partial struct opPredicates {}
    internal partial struct operand {}
    internal partial struct operandMode {}
    internal partial struct posSpan {}
    internal partial struct predeclaredConstsᴛ1 {}
    internal partial struct predeclaredFuncsᴛ1 {}
    internal partial struct stmtContext {}
    internal partial struct substMap {}
    internal partial struct subster {}
    internal partial struct target {}
    internal partial struct termlist {}
    internal partial struct tpWalker {}
    internal partial struct typeParamsById {}
    internal partial struct typeWriter {}
    internal partial struct unifier {}
    internal partial struct unifyMode {}
    internal partial struct valueMap {}
    internal partial struct valueType {}
    internal partial struct ΔconstDecl {}
    internal partial struct ΔfuncDecl {}
    internal partial struct Δinstance {}
    internal partial struct ΔtypeDecl {}
    internal partial struct ΔvarDecl {}
    public partial interface Importer {}
    public partial interface ImporterFrom {}
    public partial interface Object {}
    public partial interface Sizes {}
    public partial interface ΔType {}
    public partial struct Alias {}
    public partial struct ArgumentError {}
    public partial struct Array {}
    public partial struct Basic {}
    public partial struct BasicInfo {}
    public partial struct BasicKind {}
    public partial struct Builtin {}
    public partial struct Chan {}
    public partial struct ChanDir {}
    public partial struct Checker {}
    public partial struct Config {}
    public partial struct Const {}
    public partial struct Context {}
    public partial struct Func {}
    public partial struct ImportMode {}
    public partial struct Initializer {}
    public partial struct Instance {}
    public partial struct Interface {}
    public partial struct Label {}
    public partial struct Map {}
    public partial struct MethodSet {}
    public partial struct Named {}
    public partial struct Nil {}
    public partial struct Package {}
    public partial struct PkgName {}
    public partial struct Pointer {}
    public partial struct Selection {}
    public partial struct SelectionKind {}
    public partial struct Slice {}
    public partial struct StdSizes {}
    public partial struct Struct {}
    public partial struct Tuple {}
    public partial struct TypeAndValue {}
    public partial struct TypeList {}
    public partial struct TypeName {}
    public partial struct TypeParam {}
    public partial struct TypeParamList {}
    public partial struct Union {}
    public partial struct Var {}
    public partial struct ΔError {}
    public partial struct ΔInfo {}
    public partial struct ΔScope {}
    public partial struct ΔSignature {}
    public partial struct ΔTerm {}
    public partial struct Δcolor {}
    public partial struct Δterm {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸcontainerꓸheap() => builtin.initPackage(typeof(container.heap_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸast() => builtin.initPackage(typeof(global::go.go.ast_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸconstant() => builtin.initPackage(typeof(global::go.go.constant_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸparser() => builtin.initPackage(typeof(global::go.go.parser_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸtoken() => builtin.initPackage(typeof(global::go.go.token_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸversion() => builtin.initPackage(typeof(global::go.go.version_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸbuildcfg() => builtin.initPackage(typeof(@internal.buildcfg_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸgodebug() => builtin.initPackage(typeof(@internal.godebug_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtypesꓸerrors() => builtin.initPackage(typeof(@internal.types.errors_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸiter() => builtin.initPackage(typeof(iter_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸsort() => builtin.initPackage(typeof(sort_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸsyncꓸatomic() => builtin.initPackage(typeof(global::go.sync.atomic_package));
    [GoInit] internal static void initᴛᴛimportꓸunicode() => builtin.initPackage(typeof(unicode_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(global::go.unicode.utf8_package));
    // </ImportInitializers>
}
