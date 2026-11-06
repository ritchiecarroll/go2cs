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
global using tokenꓸPos = go.go.token_package.ΔPos;
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

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: GoFieldAccess(typeof(Alias))]
[assembly: GoFieldAccess(typeof(Basic))]
[assembly: GoFieldAccess(typeof(Builtin))]
[assembly: GoFieldAccess(typeof(Chan))]
[assembly: GoFieldAccess(typeof(Checker))]
[assembly: GoFieldAccess(typeof(Config))]
[assembly: GoFieldAccess(typeof(Const))]
[assembly: GoFieldAccess(typeof(Interface))]
[assembly: GoFieldAccess(typeof(Label))]
[assembly: GoFieldAccess(typeof(Map))]
[assembly: GoFieldAccess(typeof(Named))]
[assembly: GoFieldAccess(typeof(Package))]
[assembly: GoFieldAccess(typeof(PkgName))]
[assembly: GoFieldAccess(typeof(Pointer))]
[assembly: GoFieldAccess(typeof(Selection))]
[assembly: GoFieldAccess(typeof(Slice))]
[assembly: GoFieldAccess(typeof(Struct))]
[assembly: GoFieldAccess(typeof(TypeName))]
[assembly: GoFieldAccess(typeof(TypeParam))]
[assembly: GoFieldAccess(typeof(Union))]
[assembly: GoFieldAccess(typeof(Var))]
[assembly: GoFieldAccess(typeof(_TypeSet))]
[assembly: GoFieldAccess(typeof(action))]
[assembly: GoFieldAccess(typeof(actionDesc))]
[assembly: GoFieldAccess(typeof(block))]
[assembly: GoFieldAccess(typeof(collectObjects_methodInfo))]
[assembly: GoFieldAccess(typeof(declInfo))]
[assembly: GoFieldAccess(typeof(errorDesc))]
[assembly: GoFieldAccess(typeof(error_))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.ArrayType))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.AssignStmt))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.BinaryExpr))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.BlockStmt))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.BranchStmt))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.CallExpr))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.CaseClause))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.ChanType))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.CommClause))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.CompositeLit))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.DeclStmt))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.DeferStmt))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.Ellipsis))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.ExprStmt))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.Field))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.FieldList))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.File))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.ForStmt))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.FuncDecl))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.FuncLit))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.FuncType))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.GenDecl))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.GoStmt))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.Ident))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.IfStmt))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.ImportSpec))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.IncDecStmt))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.IndexExpr))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.IndexListExpr))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.InterfaceType))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.KeyValueExpr))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.LabeledStmt))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.MapType))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.ParenExpr))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.RangeStmt))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.ReturnStmt))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.SelectStmt))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.SelectorExpr))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.SendStmt))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.SliceExpr))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.StarExpr))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.StructType))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.SwitchStmt))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.TypeAssertExpr))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.TypeSpec))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.TypeSwitchStmt))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.UnaryExpr))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.ValueSpec))]
[assembly: GoFieldAccess(typeof(go.go.types_package.Array))]
[assembly: GoFieldAccess(typeof(go.go.types_package.Func))]
[assembly: GoFieldAccess(typeof(go.go.types_package.Tuple))]
[assembly: GoFieldAccess(typeof(graphNode))]
[assembly: GoFieldAccess(typeof(indexedExpr))]
[assembly: GoFieldAccess(typeof(lazyObject))]
[assembly: GoFieldAccess(typeof(monoVertex))]
[assembly: GoFieldAccess(typeof(operand))]
[assembly: GoFieldAccess(typeof(typeWriter))]
[assembly: GoFieldAccess(typeof(ΔInfo))]
[assembly: GoFieldAccess(typeof(ΔScope))]
[assembly: GoFieldAccess(typeof(ΔSignature))]
[assembly: GoFieldAccess(typeof(Δinstance))]
[assembly: GoFieldAccess(typeof(Δterm))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("go/types/alias.go", "alias.cs", "ADRwopSCqqCkgK7QqKCooKiigqqgqKCu0oKUpqKClIKCuIKqooKq0oKCgoKCuIKmrNKCgoKCgoKCpqY=")]
[assembly: GoPositionMap("go/types/api.go", "api.cs", "AEOKAaIACBKAooAAb/gBwgB+jgKCqqKClIKCpgACFPKClAACEgAIAoKClJSCAAoaoqiSqqKsspSUqqKqoqqiqqIAEyiCgoKClJSCggACHAALAoI=")]
[assembly: GoPositionMap("go/types/api_predicates.go", "api_predicates.cs", "AAwmAAkGgpQAAhDSgoIAAhDSgq7ClLiClK7CAAIiAA4CgqqigoI=")]
[assembly: GoPositionMap("go/types/array.go", "array.cs", "AA8koKigppCkgKKA")]
[assembly: GoPositionMap("go/types/assignments.go", "assignments.cs", "ABMwAAgClJSkAAoOgoKmgtyCgoKCgpSCpoKCgoKUpoKCgpS0tKSCgpSCgpSCgtyCgoLcgqaCgIKClJQACAiygoKUuIKCgpSUpoKmgoKmrvKCgpSCuIKClIKCgoKUlKau9KaCgqyygoK4goLKgpSCpoLKlOiCgoKCgqaCpuzSgoKClJSmgpSCgqaCpoKUqMKClLjCgpQACxQACQKCgoKChLSkyri2poKmgpS0tpSmgoKUAAgGgoKClIKCgqYACAaCkoKCgoKClIKCgoIACg7igoKmyIKCyoKCgoKClIKUypSCgoKUuIKCpqaCgoKCuIKUyoKClLiCgt6yyIKCyoKClMqUgoKClKaCgoKCuIKUyoKUuLKCpoKCgoKCgoKUgoKmgoKCgoKU7oKUgpSClLiCgoKUuIKCuKaUgoIABxCCgg==")]
[assembly: GoPositionMap("go/types/badlinkname.go", "badlinkname.cs", "AAko")]
[assembly: GoPositionMap("go/types/basic.go", "basic.cs", "AEaYAZCmkKaQpICigA==")]
[assembly: GoPositionMap("go/types/builtins.go", "builtins.cs", "ACkuAAwCpoKCpoIABxCCgpSssoKmgoKCgriC2MiCgoKCgpSCgrgADyCCgoKqooKAgoKCgpSUlIKmlILslIKCuIKU7IKCpIKCgpTayoKCgpT6toLIgpSCpIL2pIK2lMiUgoKClJS4gqaCgsiUgqSUgpSmgoLagoKCgpSCgpSUlIKC2qaCgpSClOrGAAsUgoKCpoKUgtqCuIKCyoKCgpSkpKaUgoKCuIKUpoKmAA0agoKAgoKClKaC7KaCgoKUgpSCgpSUgoK4gpSC7IKCgoKCgpSCgpSClKaCgoKmgoIACBCCpoLulILugoKClKSkppSCgoKClIK4goKUpqaCpuyCgoKmgrSkpKaCgqaClMTGpIKCpoKCgoKCgqaCpoKCguyUgoKmgoKmgoK4goKCpoKCpoKCptyClIKCyoKmgoKClOyCgoKUtsaUgoKUpraCgoKC/pSClIKUpoKCpoKCzKKCgoKCgpS4goLagoKC2pSCgqaCgqaCgoLagoKmgoKCpoKm2oKCgoKCpoKCpoKCgpSC7IKUgoK4poKCgpTugoKCpoKCgpSCpsiCgqaCgoKmgoKClIKmyJSCgoKCpoKCpoKCgtqUgoKCgqaCgoLalIKCpoKCpoKCgtqUgoKmgoKC/oKClIKClIIACxaCgoKUgoKCgoKUgtq2gtrCgoKCgqaUpoKCgqasAAkIgIKClIKUgqLIpKSCgtjEpAAGFAAKAoCGooKClIKClJSuwpS0tLSkyoKClLaqwoKClIKCgoKUqqKCgqY=", "42-44:1;192-206:2;236-243:3;254-265:4;304-308:5;330-343:6;426-438:7;480-493:8;531-540:9;1025-1027:1;1059-1068:1")]
[assembly: GoPositionMap("go/types/call.go", "call.cs", "ABFEABcClIKCgpSUlpKCgoKCgoKU3IKSlIKCpoKCAAIaAAoCgoKCgoKUpoK4goKCypSCgoKClIKUlKaCgoIACAbigpSCgoKCgu6CgoKmopSUgoKUlLjmwoKCuJSUgpS4lIKCuIKClIKCpLSCgoKClIKCgqbGgqSCuIKClJSClMq4goKUlIKCgoKUppaSgoKCgoKCgoKUppKCgoKCggAHEIKCpoLKgpSUuJS0gpSUtIKkgriCpqrCgoKUgoKCgqYAAhQADQKClIKClAAIEoKCgqaUgoKUgpSUuJSmgoKUgoK4ppSCgoKClIKUlLi4lKgABRwAEQIAChaCgqaCgoKUlIK4uIKCgoKClIKCpriUgtyCgoKCgpSWkoKUgoKCgoKotoKCgqSCtLiSgpSCppaSgpTuyoKCgoKUgqaClLim7oKCgsqClLiCuIKUyoKSgoKUgoLKgoKCuKYAGBbEzMqCgoKCgoKUgoK4gpSUpoKCpoKClJSUgoKUgpSUppSCuLiUgoKCtIK0goKCxoKCgoLGgoLEgqSCuIKmgrQAESKCgqaClIKmlIKmgoKUlKaCgpSClIK4gIK2lIKCgqaUgoKCqqKCAAcQgpSCgqaCgu6olIKClJTaAAcQgtyC3IIAChaCgoLcuKaCgpTG2oKUgoKCAAIQ8KrQpIKCgoKmpqKCgvqCmrKCgriCgriCgtak", "139-142:1;154-167:2;249-254:1;367-377:1", "", "174=describef/1/1/13;981=NewTuple/1/1/2")]
[assembly: GoPositionMap("go/types/chan.go", "chan.cs", "ABc2kqiQppCkgKKA")]
[assembly: GoPositionMap("go/types/check.go", "check.cs", "ABosAAsYABYsgoKCgoKUABtKAAgCgoKmqJKCABk4woIAQIwBkoKClIKUAAISAAgCgoKUgqiygoKokoKmgoKCgpQAAhLigoKokoKUgqiSgoKCggAGEqKq5IK4ggAJFJQAEB74goKUgoKCgoKC3IKmgoK0gpSUpqbcgoKUlIKCyrimAAoWyqamuIKClKiSqJIACQzi1sSCgoK4goKmgoLKxKbyuO6CggAbDgAKBoKClJSClKaCgoK4gpSClIKUgpSClIKUgpSClIKCpoKUlKaCpoKCgoKCgoKC2OYACQ6CgoKCgpSmgpSCpoKCqKSClKrc3KKClLTUpJSkuA==", "455-459:1;514-519:1")]
[assembly: GoPositionMap("go/types/const.go", "const.cs", "ABYs0pS4gu6CgqiSgoKClIIAAyAAEAKCpoKCppKCppSCgpSClIKUgqSCpIKkgqSkgIKkpIKkgqSCpKTIpIKkpMiCgpSUgpSCgoLGgpSCgoLGpNiCgpSUgpSCgoKCxoKUgoKCgsak2KampoKCgqaCgoKClKaCgqaCgoKUqsKCgoKClIKu8oKCggAHEIKUppQACgaCgpS0pKjCgoKCgpSCgpSCgpSCgg==", "76-79:1")]
[assembly: GoPositionMap("go/types/context.go", "context.cs", "ADZ0kgAFErKCgpSCuIKmpqrygpSCgpSUuN4ACQKUgpSCgpSUuNzY4oKCgoKCgpQ=")]
[assembly: GoPositionMap("go/types/conversions.go", "conversions.cs", "ABIowpSC6KSCgpSClJSmgoKmuIKCggAJEKSCgpSClIKUlJSUlMaCtoKClJSC3IIACBKUgoKClKYACCIAEwSCpoKCgoKCgriC3IKCgtyCuIK4griCyoKmgsqClIKCppSUxoKCgqaClP6CppKCgoKU3JSCsoKUgrKClIKClMiCsoKUgoKClLaigpSCgpS4poKCpoKCpoKCpoKCgpQ=", "23-40:1;62-81:2;237-245:1;252-267:2;257-266:2.1;270-280:3;282-291:4")]
[assembly: GoPositionMap("go/types/cycles.go", "cycles.cs", "AA0cwoKCggAIMAAYAoKmgoKCpqamlpKCpoK4griCggAHEIKCuMqCpoKCAAQQ8pSCpoKCgpSClJSCuIKCpsqmgoLa")]
[assembly: GoPositionMap("go/types/decl.go", "decl.cs", "ABEg6oKCgoKCgpSUgrqSgoKClJTo0oKClIKUuIKClIKCgoIAIEKCtILGgsYACQ62gqaCyoKUgqaClO6UgrSCxoLGxMQAEAgACASCgoKCgoLKgoKCgoKCgoK06oKCAAIYAAkCgpSClKaCAAkK6IKCgpSUgoKU3ILcgtyCpoLo+qqCgoKCgqaC3IKClJSmgoKUpoKCgpSqopKCgqYAHiqAooCigKKAooCkgoK4gtiCgpS0poKUgrSCpIK0grTWxOjEyNKmgoKUgqamgoKmgpSClKiSgu6UlKLkoqaCAAsYgpSUpoKCgoKCppSCAAcQgoK4qJKCgpSCAAwG4qaUgoKCpqiSgrimgpSCpoK4koKCyoKCgqaCgriCggAHEJSCgqaCgpS4gqYAChaCkKSCgoKmgoK4goK05OK4goKC7gAIEoKCgqaCgqaCgsqCppSClLSkuIKUtKSCgpSClJSmAAkOgoKCpvqCgpSClsiCgriS3IKCgsqmgoKClJSmgsqCgoKCgoLKgoKmuIKCgu7CppSClIK4gpSCyoKiyqKUkpSmgoKClIKCprjcgoLIlIKCuIKCgqbGgrSCtoLcgoKmyriClMi4goKCgsQ=", "52-58:1;67-70:2;140-142:3;240-246:1;278-280:1;412-415:1;524-530:1;554-559:2;615-615:3;661-663:1;749-751:1;837-839:1;846-942:1", "", "673=Name/1/1/6,describef/1/1/6;927=describef/1/1/2;1020=describef/1/1/2")]
[assembly: GoPositionMap("go/types/errors.go", "errors.cs", "ABckgoKmgpQAEyqSgpQAAhIACAKokri4gqaCgpTokoKmgoKCgoKCppQACgiSggAHEIKUgoK4gtyCgoKCgsqCgoKmuKiSlO6CgriCuIKCgpTcpoIACRTKgoKCpoKmgoKUAAwagoKCpqKCgqaigoKCpqKCgoLcggALGIKsspKClKyylLSkgqSCgpS0gg==", "", "", "212=stripAnnotations/1/1/3")]
[assembly: GoPositionMap("go/types/errsupport.go", "errsupport.cs", "ABIeAB82AAYSgoKCgoKCgoKCgoKUgoKUgriClKSkpKaClLSklKSkpLiqooKCpg==")]
[assembly: GoPositionMap("go/types/eval.go", "eval.cs", "ABEwAA0EgoKmpoIAAioAGASSgoKCgtyCgqaCgoKmgtyCgoKmkoKClA==")]
[assembly: GoPositionMap("go/types/expr.go", "expr.cs", "AD1+lAAHEKKCgoKmgpSqopSktLyilILGgramygANEtKCgqaCuIKCgpSCgqaCgoKClIK4goKClIK2goKmgpSUgoKUgoKCpr7SgoKUgoKUgpSClKaCpoKCgqamgqampoKmlJSUAAYYAAsCgoK4ABIkgoKUABEaAAgQgpS2gpSmpqaC2NqCgoLKlLiCgtyUgoKCyqiSgoIAAxQACAKCuJSClKakgoKClNyUgsaC/ILYgqakxoKCgpSClKaClNyCpoKU9IKmtJSo1IKCpoKmgriCgoKUuIKCuIKmpoKClMqCyIKCtoKC6pSCtILYyIK4yoLKgpSEkpSClKaCgpSmgpSUqqK0pqj2goKmyoKCAAUQopSCgoKCpqaCgoLKlIKC6oKCgsaCgriClIKUgpSWkoKCgoLcgqaCgoK4ABMogoKmgsqCgoKmypQAECbClIKUgpSCgoKmgoKmgoKmgoKmpoKCgpSClKaCpoKCppSCgoK4goKCgoKCypSClKaClIKCgqa8AA4UlIKmgtyCpoKmgqaClIK4gpSmgoKClIKCggATMKKCgqYABhQACwKCgoKCgriUggAIEoKUAAsM0oKUgrSCxoKCgpS2goKCvtKClIKCAAsOAAkGgpSUtsiCtoKCyIKCyIKC2oKCptaCgoKUlILIgoLIgoKmpoKUgoKUgoKUgoKUgoK2poKUtIK0goKCgoKUgoKUgpSUgtiCgpSCgsiCgtqCAAkIggAJEMiClIKCggACFgAIAqSCgoKClIKkgoKClIKkgIKkgILGpJSosoKCpoKCpgACENKCgqiSgoIAAhAACAKCgpSUgoKUuIKCgoKUgqassoKCgq7CgoIADArCgoKClIKUlLSCtIK0pIK6spSCgoI=", "198-213:1;442-448:1;885-922:1;976-979:1;1149-1161:1")]
[assembly: GoPositionMap("go/types/exprstring.go", "exprstring.cs", "AA4ksoKCABQMAAoMpLa2goLItoKCtoKCgpS2goK2goLWgoKCgraCgoKUgoKUgoKCpraCgoK2goKCgpS2graCtoKCgoK2goKUgraCgraCtoKCtoKCgraClLS0pILIooKClIKClKaClIK4goKmooKCuKaCgriCptyigoKUuKKCgpQ=")]
[assembly: GoPositionMap("go/types/format.go", "format.cs", "ABUkoqKUtLS0goKCgpSUgrSCxrSCgoKCtLSCgoKCtIKCgoKCgpSUgrSCgoKCgoKUlIKklKjCgoKCgpSmsriCggAGFrKUpKS6wqa0goKCgqaClJSqwoKUlIKCgpSUgrqSgpSCpoKU", "", "", "143=Repeat/1/1/3,sprintf/1/1/4")]
[assembly: GoPositionMap("go/types/gccgosizes.go", "gccgosizes.cs", "AAsa")]
[assembly: GoPositionMap("go/types/gcsizes.go", "gcsizes.cs", "ABAe0oLKyKQABxDcgoKCpgAIDIK2guaklIKmgpSClOaCgoKClIKmgoKCgpSmpqKkgoKCgqaCxoKCpoKClIKYooKUpKSCgpSCgoKCuIKmyILEpKyygpQ=", "16-18:1")]
[assembly: GoPositionMap("go/types/index.go", "index.cs", "ABQm4qaUgriUgoKUppTagoK4goKUgoKCyoLIgoKClLaCgoKCyIKCtoKCgpSCgpSCgoKmgpailIKCkpSCgsaCgoLGgoLGtIKkgpSUgoK4gqaCpoKUpoKCgoKUgoKUgoKCuIKCyJSCgoKmgoKC3IKmguaygoKCqKKUgoKCgriCgriCgriCgoKmlKaCgriCgqaCpoKCgoKUgoKUgoK4gtqCgoKCgpS2goKCyMiCgoKmpoKCgqiSgoLKgoKUgtjGpMqCgoKCuIKCAAgU0oKClJSU7vKClIKCgqaCpoKmgoKCgrimooK4goK4goKmlIKCuIKCuAATKIKmgpQACBAABxA=", "112-161:1;222-249:1")]
[assembly: GoPositionMap("go/types/infer.go", "infer.cs", "ABxGABYKgpK4goKSyoKmpoLKgoLcgoKCAB4+goLclLSCuIKCgoKmgoKmlMqCgpSmutqCgqaCypSCpoKCgpQAChbKgoLcggARJIKCgoKUpoKCgoLKlJSCAAsYuILGgsoAESKCggABEOKCpILKgriCgtyCrLKSgoKCgpSCgpSCgoKUlMqCgoIACxiCgoIAGjaasoKCuIKCyoKCgoIADBqClIKCpu6CgoKCqAAFFAAkOIKmgoKCgqaCgqbqtIKUpKSokoKClJSCgqyyuAAHEOSClIKCptqmpqamAAcQAAkUpoKCgqaqpqaCgtq2tuaCgoKmruKCgoKCgoKUgoKClJSCgoKUlMqUAAIU8oKCAAkUwoK4gpTKlIKUAAgQtra27oKUgsiCyIKUgsiCtraCyILYxKSCgg==", "41-43:1;48-50:2;119-154:3;305-305:4;567-569:1;615-617:2;659-670:1", "", "", "773:27")]
[assembly: GoPositionMap("go/types/initorder.go", "initorder.cs", "ABgo1riClIKCgoKUgoKCgqa4lIKCgoKmlAAIEoKUlILKggAIEoIACBKCgriCgoLugpSUgoKUgqaCgoKClL6ygpSWkoKUqIKClIK4qJKmgoKmgpSCgoKUABo4osqCgpSs5IKUgu6UlIKCgsqSgoKUAA0chqaUppSCgqamgsqCgqYACRSApIKCgqaCpoKCgsqmgqaCgoKCgg==", "151-153:1;273-275:1", "", "81=Name/1/1/1,order/1/1/1")]
[assembly: GoPositionMap("go/types/instantiate.go", "instantiate.cs", "ABhsAB4CgoKUlIKCgoKUkriCAAIiABMM0oKUgpS4goLcsoKUyoKCuJTIgsqUgqa2lJSClIKUuIKCuILYyKyygpS0tKaCgoKmqJKClMqCgoKmAAgSAAgCgoKClIKmgoKmgoKCgoKUlJS4gtyCgsqCgpS4koKUuJKCuIK4lIKUgpSUgpTKgtyClIKUlKiSgriCgoKCppSUgoKUtLSklKaqopSCgtiCguiCtg==", "115-120:1;291-316:1;340-355:2")]
[assembly: GoPositionMap("go/types/interface.go", "interface.cs", "ABs6sNae0oKClAACEuKCuIKCgsqUgoKUqLKCgpSuwqiQqKCmkKzAppCmkKigppCmkKigppAAAhIACQKClIKmgKKAqsKCggAIBtKCgoKUpoKCgsqCgoKmgoKCgpS4goKClKiSgoKmlIKCypSUgrjcgg==", "160-166:1;231-233:2", "", "280=describef/1/1/2")]
[assembly: GoPositionMap("go/types/iter.go", "iter.cs", "AAkoAAsCgoKCAAUUwoKCggAFErKCgoIABRKygoKCAAUSsoKCggAFErKCgoIABRKygoKCAAUSsoKCggAFErKCgoIABRKygoKCAAUSsoKCgg==", "21-27:1;35-41:1;48-54:1;61-67:1;74-80:1;87-93:1;100-106:1;113-119:1;126-132:1;139-145:1;152-158:1")]
[assembly: GoPositionMap("go/types/labels.go", "labels.cs", "ABYexJTcgoKCgoKCgpSClLiigoIACxzSgoKUgoKClKqigoKmqqKCgqasspQABhCCgqaYtqaCwpSC2oCCgoKCgoKCpoKmgoKUgoKCAAkWgqaCpLaCuILKgoCC1MaCguyCgIKkxoKCyJSCyIK4goK2gsi2goLItra2tra2yIKm", "107-110:1;112-114:2;116-120:3;123-264:4", "", "173=Position/1/1/5")]
[assembly: GoPositionMap("go/types/literals.go", "literals.cs", "ABIq0oKCpoKClIKUgoKClIKClIK4spQAABQACQKCgoK2gsqCgqaC5sKApoKCuILKsqaClIIAFAjCkpTKuIKClILIgpSCgpTIlIK2gKSCyoKUgoKCgoKUpoKCgpSCgoKClIKClIKCgpSCgpS4goKClIKCgqaCgoKUgpSC7AAIEoLKgtrsgoKCgoKClIKCgpSCgoKCgoKCppSClIKCpoL8oriUpoKCgpSCgpSCgtiC7sKCkpSCgoKCgoKUpoKClLiCgpSUgoKokoKU", "99-101:1", "", "115=describef/1/1/2")]
[assembly: GoPositionMap("go/types/lookup.go", "lookup.cs", "AAxKABcCgoKUpLS0pAACRgAgAoKUqgANEICCgoKClMicwoKCgoK4AAIiABQGgtymgoLKnvaCpoK4gtyUpqaCgpSCgrimgoKCgoKUgoIAChaCuOyAgoKCgpSC6tyUgqampgAKHLKCpoKCgoKmgoKmppSAgraCgrgACRSCgoKmgoKmpoKCgoKmgpSCAAIYAAkCAAIYAAsCgoKmAAgWgoKUgoKilIKClIKmgoK4oqaClLS0goKCgoKmyLiCgoK4gqaCgsqCpoKmgqaUlLS01pK0tJKmlAAHEIKUtLS0tMgAAhLigpSCqqKCgpSCgoK4poKCqJKCgpSqwoKCgpSCgoKCAAISAAkIgqYAAhAACAiClKyylIKClJSUqqKCgqaqooKCqqKCgoK4qqKCgoK4")]
[assembly: GoPositionMap("go/types/map.go", "map.cs", "AA4gkqiQppCkgKKA")]
[assembly: GoPositionMap("go/types/methodset.go", "methodset.cs", "ABksgoKmgoKClIKokKaQprKCpoKCgpSCgoKmAAYaABEWgIK4pKaCuJ72gpaSlIK4gtyUlIK4lIKClNyCuNrsopSClIKU3IKCgpS4poKokoKCgriWAAcWsoKUgpSmooKUlMqCgqaC", "49-52:1;204-206:1")]
[assembly: GoPositionMap("go/types/mono.go", "mono.cs", "AFCsAQAIDoKClIKCuIKCpoKCgoKmgsqygtyCgoLcgsqCgpSCkoKUlLS0tqqigpSqooKCgpS6AAsSgriCgoKmqqKStLaCtoKmgoLI+LSCtLS2gsaCgqaCtILIqsKCgqaCgqaCpriCgoKCgoKm3IKUgqiygqaUgqaCpoKCgqaU", "201-208:1;213-263:2;251-255:2.1")]
[assembly: GoPositionMap("go/types/named.go", "named.cs", "ALUB/gKygpSCgoKUlAACLgAXAoLKgqaCuICkgoKUgpSClIKClJQACRSCgpSClIKClIKCuILYkqqilIKClIKmgqaCpoLMwoKCpoKUAAISAAoClILugpSClIKUpsaClKqigpSqwoKUqqCowoKokoKUqJIAAhwAEAKUgqaClIKUgoKmgoKCuIKCuNrmgpSUgqaCAAgSgqa4goKCgpSmpoKmgoKUpoKq8oKClIKmgpSCgpSCgqKq4oKCgoK+soKUgoKCuIKCuKzSgpQAAhDyyoKCgriCpqaAAAImABMClIKCpoKCgpS2toKCuIKCpoKUlIKCxsiSwoLIgoK0yLKCuJSmqJKClAAIRAAkAoKCgoKSgriClIKmgpSClIKmgoKUgoKmyoKmgpSCgoKUlIKmgqao3sKClA==", "241-241:1;676-686:1;748-751:1")]
[assembly: GoPositionMap("go/types/object.go", "object.cs", "AD+KAYKCqsKC7qaClAANIKCmkKigppCmkKqwppCkgKKAooCkoKKAooCAooCktILcgqaCpgACGgANAoK4gpSCuIKCgoKUuIKUgqYACRiiqqAACBSiqJCkAA0k8qqigoKCqLKU2oIABxCkpLQAFjIACRSCgpSokKigrtKuwqyygoKqoqqgppCmkAACEgAJAoKUpgANGqKCgu6okoIACRSqooKCrLAAAhIACQKClK7AptqCggAHEKYACxSSAAkUggAVEKKClJSCgpSmtoKCgsiClMiCgoKUpoK2graCtramgpSUgqaCuKSCtoKCgpSCuO6CgqaCpqKClIKClJSClKyygoKmgKKAooCigKKAooCigKKA5KKCgoKCypSUgoKCpgAeCJKUpKSCgoKUxpSkpKSkpMaClMakpJSClA==")]
[assembly: GoPositionMap("go/types/objset.go", "objset.cs", "ABIwwoKClIKUgg==")]
[assembly: GoPositionMap("go/types/operand.go", "operand.cs", "ACROABs8tIKUAA5OACUEgoKUpKS4griUgoKUlLS02oKCuILqgoKCgpTapoKCgsqCgoKC7oKCpIKUlNqClIKClIK4yoKmABQMspSkpKSkpKSkpKSkpMiCqJKClLS0tLS0toKCgoKUgoKokoKUAAcUAAkCgqaCgqaCpoKCgqaCgqaCgsqCpoLugtyAgoK4gpSC2oKUgpTugIKC2oKmkoKCgpTcgoKCooKUgoKClJTcgoKCgrKClIKCgoKUlKY=", "337-346:1;402-410:2;417-427:3;438-449:4")]
[assembly: GoPositionMap("go/types/package.go", "package.cs", "ABg4ooKokKaQppCswKzigpSqoKaQAAIWAAkAqKCkgg==")]
[assembly: GoPositionMap("go/types/pointer.go", "pointer.cs", "AA4gkKaQpICigA==")]
[assembly: GoPositionMap("go/types/predicates.go", "predicates.cs", "AA4kkKywooCigKKAooCigKKAooCigKqygq6wooCigKKAooCigKKAqrKAgqSssrSUrLKklKzWgqqiqsaCqJKCqJKokoKuwoKClKzEgpSCqJIACAyygpSClJS2ggAKDoKC2oLIgpSCgpSUtqaokqT0pJio1IKmAAgSogAIFMKClIKmgqbKguym2oIACBCCgoKCiKbsguyCgoKCgoK42oKCAAgSgsqClKaClIKClJSCpoKCgriCpqqmgoKCAAsWgoKCgpSClIKCABYugoKClJSCgpSCgoKm7ILsgv64goKClIKCpgANFLaopKyygqas1oKUpKSkpKSmruaClJSClJSo0oKokoKCpg==", "59-59:1;218-220:1", "", "377=Tag/1/2/1,Tag/2/2/1,sameId/1/1/2,identical/1/1/3;459=identical/1/2/1,identical/2/2/2")]
[assembly: GoPositionMap("go/types/range.go", "range.cs", "ABk0AA8EAAkSgpSCgriCgpTKAAkWooKGlLS0tLSkyoLcgpSUhJKCgqiSlIKClIKmgpSmgpSCgqaCgpSCgoKClLiCgpKmlKSCgriCgqaCgriCpoKCgoKm7qaiABQQAAsCmJSClJSCppSClIKClMakpKSCpIKmlKSUlIKClIKUxraClLaUgpSClJQ=", "68-70:1;206-208:1;210-216:2", "", "70=MakeInt64/1/1/4")]
[assembly: GoPositionMap("go/types/recording.go", "recording.cs", "ABAkpqKClLS0grSklKaU+KKCpoKCgpS4goKCgpSCppSClKbKgoK0pMTeooKCgpSCgoKCgoKCqJSCgpSmAAIS4oKCgoK4goKCuIKCgoK4goKCgriigoKCuIKCgoI=", "", "", "131=newVar/1/2/1,newVar/2/2/2")]
[assembly: GoPositionMap("go/types/resolver.go", "resolver.cs", "ACJKoqiSgoKClO4ACAKCgoKmgqaCxpSCuNiCAAgIgoKClIKUgoKCpgAICtK4goLKgoKmgoKokoKClOb8goKCuIKClIKClpKCgoKCgqaCgsqCgpSCgqaCgIKkgIKkpsqCuIKUuAAQDNIAARDigpaegoK4ypKClIKCypSypoKUgoKCpoKCuIKCgpSCuIKC3IKCpoKUlKaUuIKUgqbc3IKCgoKUgu7YgpSCgqaCyIrCuLiCgpSChJKClKbGgrSCgoKUgqaCgoKUgoKUlKaUgoKCpgAJFLiClJSCyoLsgqKCgoKCgpSUlAAIEoKmgoKUgoKCzJKCgoKClAAGHAAPBIKCgrikgoKCgoKU+MSkgpTaAAMQ+LKmgoK4goK4griCgsqCuIKUpqaUgpSC7oKCyqjkgoK4ABAiggACENKUgoKClKa4kqaSAAcQqMSC7oKCygAIDoKCgpSClAADEMKCpg==", "261-460:1;510-512:1")]
[assembly: GoPositionMap("go/types/return.go", "return.cs", "AAwisqQAEBC4gsimgsimhsimpoKCgqimgsimlIKCpqaigoKCgpSCpgACFPKkABEQpoKClILapoTIpoLIgsimgsiCyILIpoKCgqY=", "", "", "59=isTerminating/1/2/1,isTerminating/2/2/2;161=hasBreak/2/2/1")]
[assembly: GoPositionMap("go/types/scope.go", "scope.cs", "ACBKwpSCgpSokKaQppKCgoKClIKokKaQqKIACBKClKzSgoKCpgADEPKCgpTcgpQAAhQACQKClIKmgoKUAAYQ8oKUlIKCpoKCuKiSgoIACxyigIKSlIKUgqaClKakqqCigKKAooCigKKAooCigKKAooCigKKAooCigKKA", "191-205:1")]
[assembly: GoPositionMap("go/types/scope2.go", "scope2.cs", "AAwwAA4CkoKmrsCigKqyAAIQAAkGgoKCyoKCgqaU")]
[assembly: GoPositionMap("go/types/selection.go", "selection.cs", "AE6oAZCmkKigqKK4goKCgu6CgoKCgoKUgrgAAhoACgAAAhLwpIAAChYACwKClLS0tKSCgoKCgoKClJQ=")]
[assembly: GoPositionMap("go/types/signature.go", "signature.cs", "ACZiAA4CAAIkABECgoKClIKCgoKUgoIADRyUgoKCgqaCpoKCgpSUgoKUlAACEuCmkKaQqKCmkKaQpICigOwADAKCgoKClpKClIK4gri4gpS4gqaCgpSClIKCgoKiqgAMDpaSgtzKgoKCgoKClKyygoKU2IL6toIACBKCgoK4gpS4goKCgoKCuKaCyoKCgpSCpoKmupKCgpSqopTcgsqipqaCgoKClAADHAAKAoKClLSmgoKUxAAKEgAKAoKmkoKCgoKClLimlIKCppSClKaCgoKCuILugoKCpqiygoIACg7kgoKCypSCgpSCtoLm2KSCxsQACgqS", "351-353:1", "", "398=describef/1/1/2;614=File/1/1/1,Name/1/1/1,Base/1/1/1,HasPrefix/2/2/1")]
[assembly: GoPositionMap("go/types/sizes.go", "sizes.cs", "ADNq0oLKyKQABxDcgoKCpgAIDIK2guaklIKmgpSClOaCgoKUgqyCgoKClIKmgoKCgpSmpgAPIKKkgoKCgqaCxoKCpoKClIKmgoKCpoSSgpSkpIKClIKCgoKUyILEpLgAFDbipICC1oCCxsiUgoKClIKUpqKClIKClJSCpgACENKCgoKCgpSCgpSUqqKCgpSssoI=", "54-56:1", "", "142=Pkg/1/3/1,Pkg/2/3/2,Path/1/2/2,Pkg/3/3/3,Path/2/2/3")]
[assembly: GoPositionMap("go/types/slice.go", "slice.cs", "AA4gkKaQpICigA==")]
[assembly: GoPositionMap("go/types/stmt.go", "stmt.cs", "ABQk8oKmgsqCgpTulJSCpoLKoqTChoKigoKmhoKmpoIAEyyCgriCgoKmpoKCgoKCgoKU+KKCgoKUgsaC1qSCgpTcgoKCpoKmlIKUAAgGgoKCgpSCgoLGgrSkpKikgtyUgIKkgILWgILWlAAPIqKCgoKCgpSCgqaCgoKUgqamgoKCgoKCpuykgoKUAAIwABgCgoKClIKClIKCyoKUgoKUgoKCgqaCgvyCgoK4gqrSgoKChJKCgoKUgoKUuICUgoKUgoKCgqSCgvyCgoK4ggA2CPSClIKUyqaU+raCvLKCgoKUgpSCtIK0gqS2koKCgpSCyIKUtLSCpoKCgpSCgqaCgoKUtpSCgpSCptqCgpSCgoKUgoKClNi2tqa4goKCgoLKgoKUyIKClJSCxoLGgoKUtLSkxtiClLaClIKCgoKUpvi02IKClIKCgqaCgoLKgoKCppSCgoKCgpSCgoKClJSCyIKClAABFAAIAoKUtIKCpoKCgqaUgpSmxoK4goKCpoKCgoKCgoKClMqUgoKCgoKCpoKUgoKCuJSC7oKCkoKUlILalJSCgoK4goK0tILGyIKCuIKCpoKClILIgoKUgoKCgoKmpoK4lLaCxsQ=", "29-32:1;58-60:1;68-70:2;414-420:1")]
[assembly: GoPositionMap("go/types/struct.go", "struct.cs", "ABc2woKCgqaClIKCqJCmkKaSgpSmgKKAqqKCAA0IwoKCgqiSlqaSgoKClIKmgoKUgoIABxCSgoKmgoKClILugoKCgoKCgpQABxCClKKClJSmgsa0uIKUgv6CgqaClLaCxqSklKaigoKCgoKUpqKCgoKmlA==", "85-101:1;107-111:2;146-172:3", "", "170=describef/1/1/26")]
[assembly: GoPositionMap("go/types/subst.go", "subst.cs", "AA8oooKCgpSqooKCgpSmgqaCgpQAAhTylIK4lKS47gAKFqKmAAkQgoKCuILugILYgoLIgoLIgIKCgtiCgsgADyCUgoKCAA0agLjYgoKCgpSClIKCgoKCAA0cpoKUyIKCgsiCggAJEoKCgqaC7oDK2La2rLKClKaigoKmpqKCgoKmooKAgrau4oKClIKUpqaigoKmpqKCgoKmooKUAAISAAkCgoKCgriCgpSCgqY=")]
[assembly: GoPositionMap("go/types/termlist.go", "termlist.cs", "ABQq3JKClIKCgpSUqMiCgqaoyIKCpqjWgoKigpSCgoKU7oKUgqaUuJKosoKqooKCgriopKiSgoKmqJKCgqaokoK4goKm")]
[assembly: GoPositionMap("go/types/tuple.go", "tuple.cs", "ABIksoKUqLKClKiQpICigA==")]
[assembly: GoPositionMap("go/types/typelists.go", "typelists.cs", "AA4eoKaQqtKClP6SgpSqoKaQqtKClKyigpSCgpSU")]
[assembly: GoPositionMap("go/types/typeparam.go", "typeparam.cs", "ABMmoAAOKOKo1IKCgpSCgriCgoKUqJCooqiSAAIS4oKUpq7CpoCqooKokpaSlJTGlJTIgoKCuJSUgpSmrLKuwg==")]
[assembly: GoPositionMap("go/types/typeset.go", "typeset.cs", "ACJIkKaQppCmkoKUvJCooKaS5oKUpKaClIKCgoKCpoKClJSClIKUgq7AprCqsoKmgpSCgpSClIKmrLKClIKCgqbY1uKCAAwagqa4gqaCgoKCAAgSlIKCgpSUAA8ggoKCkoCkgrSCgoKCAAkQgqKCgoKCAAgOgriCgoaigpSCgraClIKUgoKUtIKUgoKUgoLEgpSClOymgoKClJQABRLipoKUgoKCgoKmgoKmgqaCpoKmgoKUggAJFsKCuJSCooKClIKCgpSmlLiCgoKUgqaU", "49-51:1;184-187:1;223-252:2;242-249:2.1", "", "280=describef/1/1/7")]
[assembly: GoPositionMap("go/types/typestring.go", "typestring.cs", "AB9GooKUgoKUvrKCgqyyrLIADRyCpoKCpoKCgpSClIKCuIKmgoKUABQGwoKClIKUlNqCgoKmtoKCgraCtoKCgsqClILcgoKUgoKCgpSCuKa2gra2gtqCgpSCgpSClMiCyoKUgoKmgoKCuJSCgoKUgoKUgoKUgoKUgqa2goKCtoKClJSCxrS0pIKClIKC7IKUgpSClMiCgpSAuJSCgtyCgrj8gpSClJSU/MSm0oKCgoKUgoKU2LSChJKClIKUgoKUyKKCgoKUlOaigoK4goKUgpSClJSClIKClKaigqaygoKCgqaCgpSCgoKCAAoWgqa4ptKCgoKCgqamlIKUpoKUgriippKCgoKCgoKCgqY=", "39-44:1;484-486:1", "", "", "142:27")]
[assembly: GoPositionMap("go/types/typeterm.go", "typeterm.cs", "ABQqopSkpKS61JSkuKjUlKSkpKS4ggAHEIKUqNSUpKS4ggAHEIKUqMSUpLiCgpSo1JSkpKS4ggAHEKrCgpSCgpSCgpQ=")]
[assembly: GoPositionMap("go/types/typexpr.go", "typexpr.cs", "ABco8oKUgpSCgoKUpILagoKmyoKCuJKUAA0cgoKClNyCppSCpoKClIKCgpSUlIK2goKU7IKUgoKUtoK2grbGtqqirLKCgqrUgtyCgoKCgpQABhbigoKCgpSCAAIUAAoCgoKCgpSmguqiAAwK8oKCgoKCgqaUgpTK2oKUlOi02IKUlOi0+IKCyqaCgoKmpoKClJSCguy2goKmgoLKgpSmgoKmgoKmgoIABxCSgoKClLimlILYtLTIgoK2graCAA4G4oKCgoKUuIKCgpSCpoKUpoKC7oKCgrjYlIKClJSCgpSUygAMDPqCgoKClIKCuIKCgoKUpoKCgoLcgoKUlIKqooKCgoKUgqY=", "174-185:1;236-249:1;361-369:2;406-410:1;444-465:2", "", "198=describef/1/1/11;428=describef/1/1/8;539=describef/1/1/21")]
[assembly: GoPositionMap("go/types/under.go", "under.cs", "AAwcoq6igpQAAhLiggAMGKKClKqiAAIoABECkoKCgriCuIKCuIKCgrjYtJTKgqY=", "15-17:1;36-38:1")]
[assembly: GoPositionMap("go/types/unify.go", "unify.cs", "AFO2AcKCyoKCgpSUABwqgpSkpKSUrLKmoqq0goKCgpSUgoKCgoKUgoKUgsqAooCigKqygpT6tgAIDqSqooKCpqqigoKCgsySqqKCgpSokoKCgqauwoKClKrCgpSuAAwCgoKUgoKUuIK4goKUgpTugoKUABMogoKUppSCAAkUtoKmuJSUgoKCpriCAAkUgpTKABUsgurGlMiUpoK47qaCpoKCgoK4ggAXMIKCgpS4gpSCuIKCpoKCpriCgtyUgoKCgqYADh6CgpTKgoK4kqTKguym2oIACBCCgoKCiKbsguyCgoKCgoK4AAoUgszcgoKCgpSClIKCABYugoKClJSCgpSCgoKm7IL+guy4goKClIKCptqCAA0c7oKC3AALErY=", "290-295:1", "", "659=Tag/1/2/1,Tag/2/2/1,sameId/1/1/2,nify/1/1/3;700=nify/1/2/1,nify/2/2/2")]
[assembly: GoPositionMap("go/types/union.go", "union.cs", "ABYuooKUpoCigKSAooDckKSAooCigAAMFMKClJSCgoK4lIKCgqaCpoK4gtyigoKmgoKCgoKmgoIABxCCgpS0tKTKgsrmooKCgoKU3IKClJSUgoKUruKCgoK4goKmgqaq0oKCgpQ=", "91-134:1", "", "119=describef/1/1/43")]
[assembly: GoPositionMap("go/types/universe.go", "universe.cs", "ACZUAB8+AAoKgoKUggASJoKmyoKCuIKCpoKCgqaClIKCuIKCppSCggAICAAFFIKCuIIALVQAIU6CgoKClP6ygpSCpoKCgpSCgoKUgoKCgoKssoKCgqaCpoKClJS0xLaC", "", "", "159=MakeBool/1/2/5,MakeBool/2/2/6,MakeInt64/1/1/7")]
[assembly: GoPositionMap("go/types/util.go", "util.cs", "ABI4AAgAppCmkKaSgoKmqLCmkKaQppI=")]
[assembly: GoPositionMap("go/types/validtype.go", "validtype.cs", "AAwgwgAGGgAQApSCkpSCgoK4uILIpoKC2oKC2oKCABs2ggAVLIKm7oKCgqYABxCCggAKFIKmpoLuuILs2qKCgpQ=", "39-41:1")]
[assembly: GoPositionMap("go/types/version.go", "version.cs", "ABMooqiSqqK6goKCgoKCgoKCppyyqtKCgpQ=")]
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
