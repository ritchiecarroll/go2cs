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
global using osꓸDirEntry = go.io.fs_package.DirEntry;
global using osꓸFileInfo = go.io.fs_package.FileInfo;
global using osꓸFileMode = go.io.fs_package.FileMode;
global using osꓸPathError = go.io.fs_package.PathError;
global using osꓸSignal = go.os_package.ΔSignal;
global using runtimeꓸError = go.runtime_package.ΔError;
global using tokenꓸFile = go.go.token_package.ΔFile;
global using tokenꓸPos = go.go.token_package.ΔPos;
global using tokenꓸPosition = go.go.token_package.ΔPosition;
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
// </ImplicitConversions>

// This package's string literals are hoisted to `static readonly` fields (the standard
// library's form, or -hoist-literals), so no converted file passes a bare literal for
// go2cs-gen's literal interceptor to box. The stamp below tells the generator to skip
// the assembly.
[assembly: global::go.GoHoistedLiterals]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: global::go.GoPositionMap("go/types/alias.go", "alias.cs", "ADRwopSCqqCkgK7QqKCooKiigqqgqKCu0oCCpKaigpSCgriCqqKCqtKCgoKCgriCpqzSgoKCgoKCgqam")]
[assembly: global::go.GoPositionMap("go/types/api.go", "api.cs", "AEOKAaIACBKAooAAafgBwgB+jgKCqqKAgqSAgoCCxgACFPKAgqQAAhIACAKCgpSUggAKGqKokqqirLKUlKqiqqKqoqqiABMogoKCgpSUgoIAAhwACwKC")]
[assembly: global::go.GoPositionMap("go/types/api_predicates.go", "api_predicates.cs", "AAwmAAkGgpQAAhDSkoIAAhDSkq7ClLiClK7CAAIiAA4CgqqigoI=")]
[assembly: global::go.GoPositionMap("go/types/array.go", "array.cs", "AA8koKigppCkgKKA")]
[assembly: global::go.GoPositionMap("go/types/assignments.go", "assignments.cs", "ABMwAAgClJSkAAgOgoKmgtyCgoKCgqSCpoKCgoKUpoKCgpSkpJSCgpSCgpSCgtyAgoKC7IKmkoCCgpSU+LKCgpS4goKClJSmgqaCgqau8oKClIK4goKUgoKCgpSUpq70poKCrLKCgoC4gIKC6oKUgqaCypTYgIKCgoKCtoKm3NKCgoKUlKaClIKAgraCpoKUqMKClLjCgpQACBQACQKCgoKChLSkyri2poKmgIKktLaUpoKClOaCgoKUgoCCgrbmgpKCgoKSgpSCgoKC/uKCgqbIgoLKgoKCgoKUgpTKlIKCgpS4goKmpoKCgoK4gpTKgoKUuIKC3rLIgoLKgoKUypSCgoKUpoKCgoK4gpTKgpS4soKmgoKCgoKCgpSCgqaCgoKCgpTugIKUgIKUgqTIgoKClLiCgrimlIKCAAcQgoI=")]
[assembly: global::go.GoPositionMap("go/types/badlinkname.go", "badlinkname.cs", "AAko")]
[assembly: global::go.GoPositionMap("go/types/basic.go", "basic.cs", "AEaYAZCmkKaQpICigA==")]
[assembly: global::go.GoPositionMap("go/types/builtins.go", "builtins.cs", "AB8uAAwCpoKCpoIABxCCgpSssoIAAiAAAxmCgoKCuIIABBCCgoKSgpSCgrgADyCCgoKqooKAgoKCgoCklJSCtpSC7JSCgriClNyCgqSCgoKU2sqCgoKU+raCyIKUgqSC9qSCtpTIlIKCgpSUuIKmgoK4lIKklIKUpoKCyoKCgoKUgoKUlJSCgsqmgoKUgpTqxgALFIKCgqaClILagriCgsqCgoCClKSktpSCgoK4gpSmgqYADBqCgoCCgoKApKaC/KaCgoKUgpSCgpSUgoK4gpSC3IKCgoKCgpSCgpSClKaCgoKmgoIABxCCpoLulILugoKAgpSkpLaUgoKCgpSCuIKClKamgqbcgoKCpoK0pKSmgoKmgpTExqSCgqaCgoKCgoKmgqaCgoLclIKCpoKCpoKCuIKCgqaCgqaCgqbcgpSCgsqCpoKCgpTcgoKClLa2lIKClKamgoKCgu6UgpSClKaCgqaCgryigoKCgoKUuIKCyoKCgsqUgoKmgoKmgoKCyoKCpoKCgqaCpsqCgoKCgqaCgqaCgoKUguyClIKCuKaCkoKU7oKCgqaCgoKUgqa4goKmgoKCpoKCgpSCpriUgoKCgqaCgqaCgoLKlIKCgoKmgoKCypSCgqaCgqaCgoLKlIKCpoKCgu6CgpSCgpSCAAsWgoKClIKCgoKClILapoLawoKCgoKmlKaCkoKmrAAJCICCgIKkgpSCosikpIKC2MSkAAYUAAoCgIaigoKUgIKCpJSuwpSkpKSUyoKClLaqwoKClIKCgoKUqqKAgoCCxg==", "42-44:1;192-206:2;236-243:3;254-265:4;304-308:5;330-343:6;426-438:7;480-493:8;531-540:9;1025-1027:1;1059-1068:1")]
[assembly: global::go.GoPositionMap("go/types/call.go", "call.cs", "ABFEABcClIKCgpSUlpKCgoKCgoKU3IKSlIKCpoKCAAIaAAoCgoKCgoKUpoK4goKCypSCgoKClIKUlKaCgoLm4oKUgoKCgoLugoKCpqKUgJSCgpSUyObCgoK4lJSClLiUgoK4goKUgoKApLSCgoKClICCgoK2xoK0griCgpSUgpTKuIKAgqSUgoKCgpSmlpKCgoKCgoKCgpSmkoKCgoKCAAcQgoKmgsqClJS4lLSClJS0gqSCuIKmqsKAgpKUgoKCgrYAAhQADQKClIKCgJQACRKCgoKmlIKCgJSClJS4lKaCgoCUgoK41pSCgoKCgJSClJS4uKSoAAUcABECAAoWgoKmgoKClJSCuLiCgoKCgpSCgqa4lILcgoKCgoKUlpKClIKCgoKCqLaCgoKkgrS4koKUgqaWkoKUgO7KgoKCgpSCpoKUyKbugoKCyoKUuIK4gpTKgpKCgpSCgsqCgoK4pgASFsTMyoCCgoCCgoKClIKCuIKUlKaCgqaCgpSUlIKClIKUlKaUgri4lIKCgrSCtIKCgsaCgoKCxoKCxIKkgtiCpoKkABAigoKmgpSCppSCpoKClJSmgoKUgpSCuICCtpSCgoKmlIKCgqqiggAHEIKUgoKmgoLuqJSCgpSU2gAHEILcgtyAggALFoKAgoLcyKaSgpTG2oKUgoKCAAIQ8KrQpIKCgoKmpqKCgvqCmrKCgoC4gIKC2IKC1qQ=", "139-142:1;154-167:2;249-254:1;367-377:1", "", "172=describef/1/1/13;997=NewTuple/1/1/2")]
[assembly: global::go.GoPositionMap("go/types/chan.go", "chan.cs", "ABc2kqiQppCkgKKA")]
[assembly: global::go.GoPositionMap("go/types/check.go", "check.cs", "ABosAAsYABYsgoKCgoKUABtKAAgCgoCCtqiSggAZOMKCAECMAZKCgpSAgqQAAhIACAKCgpSCqLKCgqiSgqaCgoKClAACEuKCgqiSgpSCqJKCgoKCAAYSoqrkgriCAAkUlAAPHviCgpSCgoKCgoLcgqaCgoC0gpSUpqbsgoKUlIKCyrimgAAKFsqmtriCgpSokqiSAAkM0tbEgoKCuIKCpoKCysSm8rjugoIAEQ4ACgaCgpSUgpSmgoKCuIKUgpSClIKUgpSClIKUgpSCgqaClJSmgqaCgoKCgoKCgtjWAAkOgoKCgoKUpoKUgqaCgqikgpSq3NyigpS01KSUpLg=", "455-459:1;514-519:1")]
[assembly: global::go.GoPositionMap("go/types/const.go", "const.cs", "ABUs0pS4gu6CgqiSgoKClIIAAyAAEAKCpoKCppKCppSCgpSClICClIKkgqSCpIKkpICCpKSCpIKkgqSkyICkgqSk2IKClJSClIKCgraClIKCgrakyIKClJSClIKCgoK2gpSCgoKCtqTIpqamgoKCpoKCgoKUpoKCpoKCgpSqwoKCgoKUgq7ygpKCAAcQgpSmlPaCgpSklKjCgoKCgpSCgpSCgpSCgg==", "76-79:1")]
[assembly: global::go.GoPositionMap("go/types/context.go", "context.cs", "ADZ0kgAFErKCgpSCuIKmpqrygpSCgpSUuN4ACQKUgpSCgpSUuNzY4oKCgoKCgpQ=")]
[assembly: global::go.GoPositionMap("go/types/conversions.go", "conversions.cs", "ABIowpSCgOikgoCCpIKUpKaCgqa4goKCAAkQpIKClIKUgpSUlJSUxoK2goKUlILcggAIEqSCkoKUpgAGIgATBICCtoKCgoKCgriC3ICCgIKC/IK4griCuILKgqaCyoCClIKCppSUxoCCgoKmgpQACQ6CppKCgoKU3JSSsoKUgrKClIKClMiSsoKUgoKClLaigpSCgpS4poKCpoKCpoKCpoKAgoKk", "23-40:1;62-81:2;237-245:1;252-267:2;257-266:2.1;270-280:3;282-291:4")]
[assembly: global::go.GoPositionMap("go/types/cycles.go", "cycles.cs", "AA0cwoKCgIIACDAAGAKCpoKCgqamppaSgqaCuIK4goIABxCCgrjKgqaCggAEEPKUgqaAgoKCpIKUlIK4goKmyqaCgto=")]
[assembly: global::go.GoPositionMap("go/types/decl.go", "decl.cs", "ABEg6oKAgoKCgoKklIK6koKCgpSU2NKCgpSAgqS4goKUgoKCggAgQoCCtILGgsYACQ62graCyoKUgqaClO6UgrSCxoLGxMQACggACASCgoKCgoLKgoKCgoKCgoK06oKCAAIYAAkCgpSAgpS2ggAJCuiCgoKUlIKClNyC3ILcgqaC6PqqgoKCgoCCtoLcgoKUlKaCgpSmgoKClKqikoKCpgAeKoCigKKAooCigKSCgriC2IKClLSmgpSCtIKkgqSCpMbE6MTI0qaCgpSCpqaCgqaClIKUqJKC7pSUotSipoIACxiClJSmgoKCgoKmlIIABxCCgriokoKClIIACAbippSCgoCCtqiSgrimgpSCpoK4koKCyoKCgqaCgriAgoIACBCUgoKmgoKUuIKmAAoWgpCkgoKCpoKCuIKCtNTiuIKCgu4ACBKCgoKmgoKmgoLKgqaUgpS0pLiClLSkgoKUgIKklKYACQ6CgoKm+oKClIKWyIKCuJLcgoKCyqaCgIKClJS2gsqCgIKCgoKCyoKCgKa4goKCAAgOwqaUgpSCuIKUgsqCosqilJKUpoKCgpSCgqa43IKCyJSCgriCgoKmtoKkgqaC3IKCpsq4gpTIuIKCgoLE", "52-58:1;67-70:2;140-142:3;240-246:1;278-280:1;412-415:1;524-530:1;554-559:2;615-615:3;661-663:1;749-751:1;837-839:1;846-942:1", "", "668=Name/1/1/6,describef/1/1/6;927=describef/1/1/2;1026=describef/1/1/2")]
[assembly: global::go.GoPositionMap("go/types/errors.go", "errors.cs", "ABgkgoKmgIKkABMqkoKUAAISAAgCqJKAuMiCpoKClNiSgqaCgoKCgoKmlPiSggAHEIKUgoK4gtyCgoKCgsqCgoKmuKiSlO6CgriCuIKCgIKU7KaCAAkUyoKCgqaCpoKClAAKGoKCgqaigoKmooKCgqaigoKC3IIACxiCrLKSgpSsspS0pIKkgoKUtII=", "", "", "215=stripAnnotations/1/1/3")]
[assembly: global::go.GoPositionMap("go/types/errsupport.go", "errsupport.cs", "AA8eAB82AAYSgoKCgoKCkoKCkoKkgoKkgriClKSkpKaClLSklKSkpLiqooKCpg==")]
[assembly: global::go.GoPositionMap("go/types/eval.go", "eval.cs", "ABAwAA0EgoKmpoIAAioAGASSgoKSgtyCgIK2goKCpoLcgoKCppKCgpQ=")]
[assembly: global::go.GoPositionMap("go/types/expr.go", "expr.cs", "AD1+lAAHEKKAgoKCpoKkqqKUpLS8opSCxoK2psoACxLSgoKmgriAgoKCpIKCpoCCgoKCpIK4goKClIKmgoKmgpSUgoKUgoKCpr7SgoKUgoKUgpSClKaCpoKCgqamgqampoKmlJSUAAUYAAsCgoK4ABIkgoKUABEaAAgQgpS2gpS2pqaC2NqCgoLKlLiCgtyUkoKCyqiSgIKCAAQUAAgCgriUgIKkpqSCgoKU3JSCtoLsgsiCpqTGgoKClIKUpoKU3IKmgpT0gqa0lKjUgoKmgqaCuIKCgpS4goK4gqamgoKUyoLIgoK2goLalIK0gsi4grjKgsqClISSlIKUpoKClKaClJSqorSmqPaCgqbKgoIABRCilIKCgoKmpoKCgsqUgoLqgoKCxoKCuIKUgpSClJaSgoKCgtyCpoKCgrgAEyiAgoK2gsqCgoKmypQAECbClIKUgpSCgoKmgoKmgoKmgoKmpoKCgpSClKaCpoKCppSCgoK4goKCgoKCypSClKaClIKCgqa8AA4UlIKmgtyCpoKmgqaClIK4gpSmgoKClIKCggATMKKCgIK2AAUUAAsCgoKCgoK4lIIACBKClAAJDNKClIK0gsaCgoKUtoKCgr7SgpSCggAIDgAJBoKUlLbIgraCgsiCgsiCgtqCgqbWgoKClJSCyIKCyIKCpqaClIKClICCgqSCgpSCgramgpSkgqSCgoKCgpSCgpSClJSCyIKClIKCyIKC2oIACQiCAAkQyIKUgoKCAAIWAAgCpIKCgoKUgqSCgoKUgqSAgqSAgsaklKiygoKmgoKmAAIQ0oKCqJKCggACEAAIAoKClICUgoKUyIKCgoKUgqassoKCgq7CgoIACArCgoKClIKUlKSCpIKklIK6spSAgoKC", "198-213:1;442-448:1;885-922:1;976-979:1;1149-1161:1")]
[assembly: global::go.GoPositionMap("go/types/exprstring.go", "exprstring.cs", "AA4ksoKCAAwMAAoMpLa2goLItoKCtoKCgpS2goK2goLWgoKCgraCgoKUgoKUgoKCpraCgoK2goKCgpS2graCtoKCgoK2goKUgraCgraCtoKCtoKCgraClKSklILIooKClIKClKaClIK4goKmooKCuKaAgoLIgqbcooKClLiigoKU")]
[assembly: global::go.GoPositionMap("go/types/format.go", "format.cs", "ABQkoqKUtLS0goKCgpSUgrSCxrSCgoKCtLSCgoKCtIKCgoKCgpSUgrSCgoKCgoKUlIKklKjCgoKCgpSmsriCggAGFrKUpKS6wqa0goKCgqaClJSqwoKUlIKCgpSUgrqSgpSCpoKU", "", "", "142=Repeat/1/1/3,sprintf/1/1/4")]
[assembly: global::go.GoPositionMap("go/types/gccgosizes.go", "gccgosizes.cs", "AAsa")]
[assembly: global::go.GoPositionMap("go/types/gcsizes.go", "gcsizes.cs", "ABAe0oLKyKQABxDcgoKAgrYACAyCtoLmpJSCpoKUgpTmgoKCgpSCpoKCgoCClLamoqSCgoKAgraCxoKCpoKClIKYooKUpKSCgpSCgoKCuIKmyILEpKyygpQ=", "16-18:1")]
[assembly: global::go.GoPositionMap("go/types/index.go", "index.cs", "ABMm4qaUgriUgoKUpoCU2oKCuIKClIKCgsqCyIKCgpS2gIKCgoLYgoK2goKClIKClIKCgqaClqKUgoKSlIKCxoKCgsaAgoLWtIKkgpSUgoK4gqaCpoKUpoKCgoKUgoKUgoKCuIKCyJSCgoKmgoKC3IKmgtaygoKCqKKUgoKCgriCgriCgriCgoKmlKaCgriCgqaCpoKCgoKUgoKUgoK4gtqCgoKCgpS2gIKCgtjIgoKCpqaCgoKokoKCyoKClICC6MakyoKCgoK4goIACBTSgoKUlJTe8oKUgoKCpoKmgqaCgoKCuKaigriCgriCgqaUgoK4goK4ABMogqaClAAIEAAHEA==", "112-161:1;222-249:1")]
[assembly: global::go.GoPositionMap("go/types/infer.go", "infer.cs", "ABtGABYKgpK4goKSyoKmpoLKgoLcgoKCAB4+goLclLSCuIKCgoKmgoKmlMqCgpSmutqCgqaCypSCpoKCgqSAAAoW2oKC3IIAESSCgoKClKaCgoKCypSUggALGLiCxoLKABEigoIAARDigqSCyoK4goLcgqyykoKCgoKUgoKUgoKClJTKgoKCAAsYgoKCABo2mrKCgriCgsqCgoKCgAAMGoCCpIKCtu6CgoKCqAAFFAAkOIKmgoKCgqaCgqbatIKUpKSokoKClJSCgqyyyAAHEOSAgqSCgqbapqampgAHEAAJFKaCgoKmqqamgoLatrbmgoKCpq7igoKCgoKClIKCgpSUgoKClJSAyqQAAhTykoIACRTCgriAgoCU6pSClAAIELa2tu6ClILIgsiClILIgra2gsiAgujEpIKC", "41-43:1;48-50:2;119-154:3;305-305:4;567-569:1;615-617:2;659-670:1")]
[assembly: global::go.GoPositionMap("go/types/initorder.go", "initorder.cs", "ABgo1riSlIKCgoKUgIKCgoKmyJSCgoKCppQACBKClJSCyoIACBKCAAgSgoK4goKC7oKUlIKClIKmgoKCgpS+soKUlpKClKiCgpSAgsiokqaCgqaClIKCgpQAGjiiyoKClKzkgpSAgv6UlICCgoLakoKAgpQADhyGppSmlIKCpqaCyoKCpgAJFICkgoKCpoKmgoKCyqaCpoKCgoKC", "151-153:1;273-275:1", "", "84=Name/1/1/1,order/1/1/1")]
[assembly: global::go.GoPositionMap("go/types/instantiate.go", "instantiate.cs", "ABhsAB4CgoKUlIKCgoKUgJLIggACIgATDNKClIKUuIKC3LKClMqCgILIlMiCypSCpraUlIKUgpS4gpK4gtjIrLKClLS0poKCgqaokoKUyoKCgqYABhIACAKCgoKUgIK2goKmgoKCgoKUlJS4gtyCgsqCgpS4koKUuJKCuIK4lIKUgpSUgpTKgtyClIKUlKiSgriCkoKCppSUgoKUtLSklKaqopSCgtiCguiCtg==", "115-120:1;291-316:1;340-355:2")]
[assembly: global::go.GoPositionMap("go/types/interface.go", "interface.cs", "ABs6sNae0oKClAACEuKCuIKCgILalIKClKiygoKUrsKokKigppCswKaQppCooKaQppCooKaQAAISAAkCgpSCpoCigKrCgoLm0oKCgpSmgoKCyoKCgqaCgoKClLiCgoCCpKiSgoCCtpSCgsqUlIK43II=", "160-166:1;231-233:2", "", "284=describef/1/1/2")]
[assembly: global::go.GoPositionMap("go/types/iter.go", "iter.cs", "AAkoAAsCgoKCAAUUwoKCggAFErKCgoIABRKygoKCAAUSsoKCggAFErKCgoIABRKygoKCAAUSsoKCggAFErKCgoIABRKygoKCAAUSsoKCgg==", "21-27:1;35-41:1;48-54:1;61-67:1;74-80:1;87-93:1;100-106:1;113-119:1;126-132:1;139-145:1;152-158:1")]
[assembly: global::go.GoPositionMap("go/types/labels.go", "labels.cs", "ABMexJTcgoKCgoCCgoKUgqS4ooKAggAMHNKCgpSCgoKUqqKCgIK2qqKCgIK2rLKUAAYQgoKmmLamgsKUgILqgIKCgIKCgoKCpoK2goKUgoKCAAkWgqaCpLaCuILKgoCC1MaCgtyCgIKkxoKCuJSCuIK4goK2gsi2goLItra2tra2yIKm", "107-110:1;112-114:2;116-120:3;123-264:4", "", "182=Position/1/1/5")]
[assembly: global::go.GoPositionMap("go/types/literals.go", "literals.cs", "ABIq0oKCpoKClIKUgoKClIKClIK4spQAABQACQKCgoKmgsqCgqaC1sKApoKCuILKsqaClIIADQjCkpTKgLiCgqSCyIKUgoCCpMiUgraApILKgoCUgoKCgoKUpoKCgpSCgoKAgqSCgpSCgoKUgoKUuIKAgoKkgoKCpoKCgpSClIL8AAgSgsqC2uyCgoKCgoKUgoKClIKCgoKCgoKmlIKUgoKmgvyigLikpoKCgpSCgpSCgtiC3sKCkpSCgoCCgIKCgpS2koKUyIKClJSCgqiSgpQ=", "99-101:1", "", "113=describef/1/1/2")]
[assembly: global::go.GoPositionMap("go/types/lookup.go", "lookup.cs", "AAxKABcCgoKUpLS0pAACRgAgAoKUqgANEICCgIKCgIKk2JzCgoCCgoCC2AACIgAUBoLcpoKAgtqe9oKmgriAgoDcpKaApoKClIKC2KaCgoKCgpSCggAKFoK47ICCgoKClILq3ICUgrampgAKHLKCpoKCgoCCpoKCtqaUgIK2goK4AAkUgoKCpoKCpqaCgoKCpoKUggACGAAJAgACGAALAoKCpgAIFoKClICCgqKUgoKUgqaCgriipoKUtLSCgoKCgqbIuIKCgriCpoKC2oKmgqaCppSUtLTGkqS0kqaUAAcQgpS0pKSkuAACEuKClIKqooCCgpSCgoLIpoKCqJKCgIKkqsKCgoKUgoKCggACEgAJCIKmAAIQAAgIgpSssoCUgoKUlKSqooCCgILGqqKCgqqigoKCuKqigoKCuA==")]
[assembly: global::go.GoPositionMap("go/types/map.go", "map.cs", "AA4gkqiQppCkgKKA")]
[assembly: global::go.GoPositionMap("go/types/methodset.go", "methodset.cs", "ABgsgoKmgoKClIKokKaQprKCpoKCgpSCgoKmAAYaABEWgIK4pKaCuJ72gpaSlIK4gIKA3KSUgsiUgoKU3IK42uyigJSClIKU7IKAgoKUyKaCqJKCgoK4lgAHFrKClIKUpqKClJTKgIKCtoI=", "49-52:1;204-206:1")]
[assembly: global::go.GoPositionMap("go/types/mono.go", "mono.cs", "AFCsAQAIDoKClIKCuIKCpoKCgoKmgsqygtyCgoLcgsqCgpSCkoKUlLS0tqqigpSqooKCgpS6AAsSgriCgoKmqqKStLaCtoCCtoKCyPi0grS0toLGgoKmgrSCyKrCgoKmgoKmgIK2uIKCgIKAgoKCpvyClIKosoCCtpSAgraCpoKCgqaU", "201-208:1;213-263:2;251-255:2.1")]
[assembly: global::go.GoPositionMap("go/types/named.go", "named.cs", "ALUB/gKygpSCgoKUlAACLgAXAoLKgqaCuICkgoKUgpSClIKClJQACRSCgpSClIKClIKCuILYkqqilIKClIKmgqaCpoLMwoKCpoKUAAISAAoClILugpSClIKUpsaClKqigpSqwoKUqqCowoKokoKUqJIAAhwAEAKUgqaClIKUgoKmgoKCuIKCuNrmgpSUgqaCAAgSgqa4goKCgpSmppKmgoKUpoKq8oKClIKmgpSCgpSCgqKq4oKCgoK+soKUgoKCuIKCuKzSgpQAAhDyyoKCgriCpqaAAAImABMClIKCpoKCgpS2toKCuIKCpoKUlIKCxsiSwoLIgoK0yLKCuICUtqiSgpQABkQAJAKCgoKCkoK4gpSCpoKUgpSCpoKClIKCpsqAgoCmgpSCgoKUlIKmgqbI3sKAgqQ=", "241-241:1;676-686:1;748-751:1")]
[assembly: global::go.GoPositionMap("go/types/object.go", "object.cs", "ADyKAYKCqsKC7qaClAANIKCmkKigppCmkKqwppCkgKKAooCkoKKAooCAooCktILcgqaCpgACGgANAoK4gpSCuIKCgoKUuIKUgqYACRiiqqAACBSiqJCkAA0k8qqigoKCqLKU2oIABxCkpLQAFjIACRSCgpSokKigrtKuwqyygoKqoqqgppCmkAACEgAJAoKUpgANGqKCgu6okoIACRSqooKCrLAAAhIACQKClK7AptqAgoIACBCmAAsUkgAJFIIADxCigpSUgoCCpKa2goKCyIKUyIKCgpSmgraCtoK2tqaClJSCpoK4pIK2goKAgrSAgrj+goKmgqaigpSCgpSUgpSssoKCpoCigKKAooCigKKAooCigNSigoKAgoKAypSkgpKCtgARCJKUpKSCkoCClNaUpKSkpKTGgpTGpKSUgpQ=")]
[assembly: global::go.GoPositionMap("go/types/objset.go", "objset.cs", "ABIwwoKAgqSClII=")]
[assembly: global::go.GoPositionMap("go/types/operand.go", "operand.cs", "ACROABs8tIKUAAlOACUEgoKUpKS4griUgoKUlKSkyoKCuILqgoKCgpTapoKAgoLagoKCgu6CgqSClJTagpSCgpSCuMqCpgAMDLKUpKSkpKSkpKSkpKTIgqiSgpSkpKSkpKaCgoKClIKCqJKClAAGFAAJAoKmgoKmgqaCgoKmgoKmgoLKgqaC7oLcgIKCuIKUgtqAgpSClP6AgoCC6oKmkoKCgpTcgoKCooKUgoKClJTcgpKCgrKClIKCgoKUlKY=", "337-346:1;402-410:2;417-427:3;438-449:4")]
[assembly: global::go.GoPositionMap("go/types/package.go", "package.cs", "ABg4ooKokKaQppCswKzigpSqoKaQAAIWAAkAqKCkgg==")]
[assembly: global::go.GoPositionMap("go/types/pointer.go", "pointer.cs", "AA4gkKaQpICigA==")]
[assembly: global::go.GoPositionMap("go/types/predicates.go", "predicates.cs", "AA4kkKywooCigKKAooCigKKAooCigKqygq6wooCigKKAooCigKKAqrKAgqSssrSUrLKklKzWgqqiqsaCqJKCqJKokoKuwoCCgqSsxICCpIKokuyygpSClJS2ggAKDoKC2oLIgpSCgpSUtqaokqT0pJio1IKmAAgSogAIFMKClIKmgqbKgIL8gKbqgIIACRCAgoKCgoim/ICC/ICCgoKCgoK46oKCAAgSgsqClKaClIKClJSCpoKCgriCpqqApoKCggAMFoCCgoKClIKUgoIAFi6CgoKUlIKClIKCgqb8gIL8gIIACA6AuIKCgpSCgqYADhS2qKSssoKmrNaAgpSkpKSkpLau5oKUlIKUlKjSkqiSgoKm", "59-59:1;218-220:1", "", "386=Tag/1/2/1,Tag/2/2/1,sameId/1/1/2,identical/1/1/3;473=identical/1/2/1,identical/2/2/2")]
[assembly: global::go.GoPositionMap("go/types/range.go", "range.cs", "ABY0AA8EAAkSgpSCgIK4goKUygAKFqKChpS0tLS0pMqC3IKUlISSgoKokoCUgoKUgqaCpKaClIKCpoKClIKCgoKUuIKCkqakpIKCuIKCpoKCuIKmgoKCgrbupqIADBAACwKYlICCpJSCppSClIKClMakpKSCpIKmlKSUlIKClIKUxraClLaUgpSClJQ=", "68-70:1;206-208:1;210-216:2", "", "68=MakeInt64/1/1/4")]
[assembly: global::go.GoPositionMap("go/types/recording.go", "recording.cs", "ABAkpqKClKSkgqSUlKaU6KKCpoKCgpS4goKCgpSCppSAgqSmyoKCtKTE3qKCgoKUgoKAgoKCgoKolIKClLYAAhLigoKCgILIgoKAgsiCgoKAgsiCgoKAgsiigoKAgsiCgoKAgg==", "", "", "129=newVar/1/2/1,newVar/2/2/2")]
[assembly: global::go.GoPositionMap("go/types/resolver.go", "resolver.cs", "ACJKoqiSgoKClN4ACAKCgoKmgqaCxpSCuNiC+IKCgpSClIKCgqbq0riCgsqCgqaCgqiSgoCCpNb8goKCuIKClIKClpKAgpKAgoKCpoKC6oKClIKCpoKAgqSAgqSmyoK4gpS4AA0M0gABEOKClp6CgrjKkoCCpIKCypSypoKUgoKCpoKCuIKCgpSCuIKC3IKCpoKUlKaUuIKUgqbc3ICCgoKClIL+2IKUgoKmgsiKwri4goKUgoSSgpSmxoK0goKClIKmgoKClIKClICUtpSCgoKmAAkUuICCpJSCyoLsgqKAgoKCgIKClJSkAAkSgqaCgpSCgoLMkoKCgoKUAAUcAA8EgoCCgsikgoKCgoKU+MSkgpTaAAMQ+LKmgoK4goK4griCgsqCuIKUpqaAlIKUgv6Cgsqo5IKAgsgAECKCAAIQ0pSCgIKClKbIkqaSAAcQqMSC7oKCygAIDoKCgIKkgpQAAxDCgIK2", "261-460:1;510-512:1")]
[assembly: global::go.GoPositionMap("go/types/return.go", "return.cs", "AAwisqQAEBC4gILYpoLIpobIpqaCgoKopoLIppSCgIK2pqKCgoKClIKmAAIU8qQAERCmgoKUgtqmhMimgsiCyKaCyILIgsimgoKCpg==", "", "", "61=isTerminating/1/2/1,isTerminating/2/2/2;165=hasBreak/2/2/1")]
[assembly: global::go.GoPositionMap("go/types/scope.go", "scope.cs", "ACBKwpSCgpSokKaQppKCgoKClIKokKaQqKIACBKClKzSgoKCpgADEPKCgIKk3IKUAAIUAAkCgpSCpoKClAAFEPKClJSCgqaCgriokoKCAAscooCCkpSAgqSCpoKUpqSqoKKAooCigKKAooCigKKAooCigKKAooCigKKAooA=", "191-205:1")]
[assembly: global::go.GoPositionMap("go/types/scope2.go", "scope2.cs", "AAwwAA4CkoCCtq7AooCqsgACEAAJBoKCgILagoKCppQ=")]
[assembly: global::go.GoPositionMap("go/types/selection.go", "selection.cs", "AE6oAZCmkKigqKK4kpKCgu6SkoKCgoKUgrgAAhoACgAAAhLwpIAABxYACwKClKSkpJSCgoKCgoCCgpSk")]
[assembly: global::go.GoPositionMap("go/types/signature.go", "signature.cs", "ACZiAA4CAAIkABECgoKClIKCgoKUgoIADRyUgpKCgqaCpoKCgpSUgoKUlAACEuCmkKaQqKCmkKaQpICigNwADAKCgoKClpKClICCyIK4uIKUuIKmgoKUgpSCgoKCoqoADA6WkoLcyoKCgoCCgoKkrLKCgIKU2IL6toIACRKCgoK4gpS4goKCgoKCuKaCyoKCgpSCpoKmupKAgoKUuqKU3ILKoqamgoKCgpQAAxwACgKCgpS0poKClMQACBIACgKCppKCgoCCgoKUyKaUgoKmlIKUpoKCgoK4gu6CgoKmqLKCggAIDuSCgoLKlIKClIK2gubYpILGxAAICpI=", "351-353:1", "", "406=describef/1/1/2;618=File/1/1/1,Name/1/1/1,Base/1/1/1,HasPrefix/2/2/1")]
[assembly: global::go.GoPositionMap("go/types/sizes.go", "sizes.cs", "ADNq0oLKyKQABxDcgoKAgrYACAyCtoLmpJSCpoKUgpTmgoKClIKsgoKCgpSCpoKCgoCClLamAA8goqSCgoKAgraCxoKCpoKClIKmgoKCpoSSgpSkpIKClIKCgoKUyILEpLgAFDbilICCxoCCtsiUgoKClICCpKaigpSCgpSUgqYAAhDSgoKCgoKUgoKUlKqigoKUrLKC", "54-56:1", "", "144=Pkg/1/3/1,Pkg/2/3/2,Path/1/2/2,Pkg/3/3/3,Path/2/2/3")]
[assembly: global::go.GoPositionMap("go/types/slice.go", "slice.cs", "AA4gkKaQpICigA==")]
[assembly: global::go.GoPositionMap("go/types/stmt.go", "stmt.cs", "ABMk8oKmgsqCgpTulJSCpoLKoqTChoKigoCCtoaCpqaCABMsgoK4goKAgramgoKCgoKCgpToooKCgpSCxoLWpIKClNyCgoKmgqaUgpTmgoKCgpSCgoK2gqSklKikgtyUgIKkgILGgILGlAAPIqKCgoKCgpSCgqaSgoKUgqaApoKCgoKCgqb8pICCgqQAAjAAGAKCgoKUgoKUgoLKgpSCgpSCgoKCpoKC/IKCgriCqtKCgoKEkoKCgpSCgpS4gJSCgpSCgoKCpIKC/IKCgriCAB0I9IKUgIKkyqaU+raCvLKCgoIAARCCpIIAAhOClIIAARC2koKCgpSAgtiClKSkgqaCgoKUgoKmgoKClLaUgoKUgqbKgoKUgoKClIKCgpTItramuIKAgoKCgtqCgpTIgoKUlIK2graCgpS0tKS2yIKUtoKUgoKCgpSm+LTYgoKUgoKCpoKCgsqCgoKmlIKCgoKClIKCgoKUlILIgoKUAAEUAAgCgpS0goKmgoKCppSClKbGgriCgoKmgoKCgoKCkoKUypSCgoKCgoKmgpSCgoK4lILugoKSgpSUgtqUlIKCgriCgrS0gsbIgoCCyIKCpoKClILIgoKUgoKCgoKmpoCCuKS2gsbE", "29-32:1;58-60:1;68-70:2;414-420:1")]
[assembly: global::go.GoPositionMap("go/types/struct.go", "struct.cs", "ABc2woKCgqaClIKCqJCmkKaSgpSmgKKAqqKCAAkIwoKCgqiSlqaSgoKClIKmgoKUgoIABxCSgoKmgoKClILugoKCgoKCgpQABxCClKKClJSmgsa0uIKUgv6CgqaClLaAgtakpJSmooCCgoKCgqSmooKCgIK2lA==", "85-101:1;107-111:2;146-172:3", "", "166=describef/1/1/26")]
[assembly: global::go.GoPositionMap("go/types/subst.go", "subst.cs", "AA8oooKCgpSqooKCgpSmgqaCgIKkAAIU8pSCuJSkuP4AChaipgAJEIKCgriC7oCC2IKCyIKCyICCgoLYgoLIAA8glIKCggANGoC42IKCgoKUgpSCgoKCggANHKaClMiCgoLIgoIACRKCgoKmgu6Ayti2tqyygpSmooKAgramopKCgqaigoCCtq7igoCClIKUtqaigoCCtqaikoKCpqKAgqQAAhIACQKCgoKCuIKClJKCpg==")]
[assembly: global::go.GoPositionMap("go/types/termlist.go", "termlist.cs", "ABQqzJKClIKCgpSUqMiCgqaoyIKCpqjWgoKigpSCgoKUgO6ClIK2lLiSqLKCqqKCgoCCyKikqJKCgqaokoKCpqiSgriCgqY=")]
[assembly: global::go.GoPositionMap("go/types/tuple.go", "tuple.cs", "ABIksoKUqLKClKiQpICigA==")]
[assembly: global::go.GoPositionMap("go/types/typelists.go", "typelists.cs", "AA4eoKaQqtKClP6SgpSqoKaQqtKClKyigpSCgpSU")]
[assembly: global::go.GoPositionMap("go/types/typeparam.go", "typeparam.cs", "ABMmoAAOKOKo1JKCgpSCgriCkoKUqJCooqiSAAIS4oKUpq7CpoCqooKokpaSlJTGlJTIgoKCuJSUgIKkpqyyrsI=")]
[assembly: global::go.GoPositionMap("go/types/typeset.go", "typeset.cs", "ACJIkKaQppCmkoKUvJCooKaS1oKUpKaClIKCgoKCpoKClJSClIKUgq7AprCqsoKmgpSCgpSClIKmrLKClIKCgqbYxuKCAAwagqa4gqaCgoKCAAgSlIKCgpSUAA8ggoKCkoCkgrSCgoKCAAkQgqKCgoKCAAgOgriCgoaigpSCgraClIKUgoKUtIKUgoKUgoLEgpSClOymgoKClJQABRLipoKUgoKCgoKmgoKmgqaCpoKmgoKUggAJFsKAgsiUgqKCgoCUgpKClKaUyIKCgpSCppQ=", "49-51:1;184-187:1;223-252:2;242-249:2.1", "", "278=describef/1/1/7")]
[assembly: global::go.GoPositionMap("go/types/typestring.go", "typestring.cs", "AB9GooKUgoKUvrKCgqyyrLIADRyCpoKCpoKCgpSClIKCuIKmgoKUAAwGwoKClIKUlNqCgIKCtraCgoK2graCgoLKgpSC3IKClIKCgoKUgIK4traCtraC2oKClIKClIKUyILKgpSCgqaCgoK4lIKCgpSCgpSCgpSCgpSCpraCgoK2goKUlICCxqSklIKClIKC7IKUgpSSlMiCgpSAuJSCgtyCgrj8goCUkpSklPzEptKCgoKClIKClNi0goSSgpSClIKClMiigoKClJTWooKCuIKClIKUgpSUgpSCgpSmooKmsoKCgoKmgoKUgoKAgoIAChaCtrim0oKCgoKCpqaUgpSmgpSCuKKmkoKCgoKCgoKCpg==", "39-44:1;484-486:1")]
[assembly: global::go.GoPositionMap("go/types/typeterm.go", "typeterm.cs", "ABQqopSkpKS61JSkuKjUlKSkpKS4ggAHEIKUqNSUpKS4ggAHEIKUqMSUpLiCgpSo1JSkpKS4ggAHEKrCgpSCgpSCgpQ=")]
[assembly: global::go.GoPositionMap("go/types/typexpr.go", "typexpr.cs", "ABUo8oKUgpSCkoKUpILKgoKmyoKCuICSpAANHIKCgpTcgIK2lIKmgoKUgoKClJSUgraCgpTsgpSCgpS2graCtsa2qqKssoKCqtSC3IKAgoKCgpQABxbigoKCgpSCAAIUAAoCgoKCgpSmgtqiAAgK8oKCgoKCgqaUgpTK2oKUlNikyIKUlNik6IKCyqaCgoKmpoCCgpSkgoLstoKCpoKCyoKUpoKCpoKCpoKCAAcQkoKCgpS4ppSCyKSkuIKCtoK2ggANBuKCgoKClLiCgoKUgqaAgqSmgoLugoKCuNiUgoKUgJSCgpSU2gAKDPqAgoKCgpSAgoLYgoKCgpSmgoCCgoCC/IKClJSCqqKCgoKClIKm", "174-185:1;236-249:1;361-369:2;406-410:1;444-465:2", "", "200=describef/1/1/11;423=describef/1/1/8;531=describef/1/1/21")]
[assembly: global::go.GoPositionMap("go/types/under.go", "under.cs", "AAwcoq6igIKkAAIS4oIADBiigpSqogACKAARApKCgoCCyIK4goK4gIKAgoK42LSU6oKm", "15-17:1;36-38:1")]
[assembly: global::go.GoPositionMap("go/types/unify.go", "unify.cs", "AFO2AcKCyoKCgpSUABgqgpSkpKSUrLKmoqq0goKCgpSUgoKCgoKUgoKUgsqAooCigKqygpSA+rYACA60qqKAgoCCxqqigoKCgsySqqKCgpSokoKCgqauwoKClKrCgIKkrgAMAoKClIKClLiCuIKClIKU7oKClAATKICCgpSmlIIAChSAtoKmuICUlIKCgqa4ggAJFIKkygAVLILqxoCU2JS2gsjupoKmgoKCgriCABcwgoKClLiClIK4goKmgoCCtriCgtyUgoKCgIK2AA4egoKUyoKCuJKkyoCC/ICm6oCCAAkQgIKCgoKIpvyAgvyAgoKCgoKCuAALFICC3NyAgoKCgpSClIKCABYugoKClJSCgpSCgoKm/ICCAAgOgIL8gLiCgoKUgoKm6oIADRzugIKC3AAMErY=", "290-295:1", "", "683=Tag/1/2/1,Tag/2/2/1,sameId/1/1/2,nify/1/1/3;730=nify/1/2/1,nify/2/2/2")]
[assembly: global::go.GoPositionMap("go/types/union.go", "union.cs", "ABYuooKUpoCigKSAooDckKSAooCigAALFMKClJSCgoK4lIKCgqaCpoK4gtyigoKmgoKCgoKmgoIABxCCgpS0tKTKgILa1qKCgoCCgqTcgoKUlJSCgpSu4oKCgriCgqaCpqrSgIKCgqQ=", "91-134:1", "", "118=describef/1/1/43")]
[assembly: global::go.GoPositionMap("go/types/universe.go", "universe.cs", "ACZUAB4+AAgKgoKUggASJoKmyoKCuIKCpoKCgqaClIKCuIKCppSCggAICAAFFIKCuIIALVQAIU6CgoKClO6ygpSCpoKCgpSCgoKUgoKCgoKssoKCgqaAgraCgpSUtMS2gg==", "", "", "156=MakeBool/1/2/5,MakeBool/2/2/6,MakeInt64/1/1/7")]
[assembly: global::go.GoPositionMap("go/types/util.go", "util.cs", "ABI4AAgAppCmkKaSgoCCtqiwppCmkKaS")]
[assembly: global::go.GoPositionMap("go/types/validtype.go", "validtype.cs", "AAwgwgAFGgAQApSCgJKkgoKCuLiCyKaCgtqCgtqCggAbNoIAFSyCpu6CgoKmAAcQgoIAChSAgqamgu64gvzaooKClA==", "39-41:1")]
[assembly: global::go.GoPositionMap("go/types/version.go", "version.cs", "ABMooqiSqqK6goKCgoKCgoKCppyyqtKCgpQ=")]
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
    internal partial struct nodeQueue {}
    internal partial struct nodeSet {}
    internal partial struct objset {}
    internal partial struct opPredicates {}
    internal partial struct operand {}
    internal partial struct operandMode {}
    internal partial struct posSpan {}
    internal partial struct predeclaredConstsᴛ1 {}
    internal partial struct predeclaredFuncsᴛ1 {}
    internal partial struct stateMask {}
    internal partial struct stmtContext {}
    internal partial struct substMap {}
    internal partial struct subster {}
    internal partial struct target {}
    internal partial struct termlist {}
    internal partial struct tpWalker {}
    internal partial struct typeError {}
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
    public partial struct VarKind {}
    public partial struct ΔError {}
    public partial struct ΔInfo {}
    public partial struct ΔScope {}
    public partial struct ΔSignature {}
    public partial struct ΔTerm {}
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
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸgodebug() => builtin.initPackage(typeof(global::go.@internal.godebug_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtypesꓸerrors() => builtin.initPackage(typeof(global::go.@internal.types.errors_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸiter() => builtin.initPackage(typeof(iter_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
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
