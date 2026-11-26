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
using ast = go.go.ast_package;
using constant = go.go.constant_package;
using errors = go.@internal.types.errors_package;
using io = go.io_package;
using iter = go.iter_package;
using token = go.go.token_package;
// </ImportedTypeAliases>

using go;
using static go.go.types_package;
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
[assembly: GoDynamicTypeLift("696e746572666163657b4f626a2829202a676f2f74797065732e547970654e616d657d", "instantiatedType_type")]
[assembly: GoDynamicTypeLift("7374727563747b6e616d6520737472696e673b206b696e6420676f2f74797065732e42617369634b696e643b2076616c20676f2f636f6e7374616e742e56616c75657d", "predeclaredConstsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6e616d6520737472696e673b206e6172677320696e743b20766172696164696320626f6f6c3b206b696e6420676f2f74797065732e657870724b696e647d", "predeclaredFuncsᴛ1")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<Alias, cleaner>(Pointer = true)]
[assembly: GoImplement<Alias, go.go.types_package.Type>(Pointer = true)]
[assembly: GoImplement<ArgumentError, error>(Pointer = true)]
[assembly: GoImplement<Basic, go.go.types_package.Type>(Pointer = true)]
[assembly: GoImplement<Builtin, go.go.types_package.Object>(Pointer = true)]
[assembly: GoImplement<Chan, go.go.types_package.Type>(Pointer = true)]
[assembly: GoImplement<Const, go.go.types_package.Object>(Pointer = true)]
[assembly: GoImplement<Error, error>]
[assembly: GoImplement<Interface, cleaner>(Pointer = true)]
[assembly: GoImplement<Interface, go.go.types_package.Type>(Pointer = true)]
[assembly: GoImplement<Label, go.go.types_package.Object>(Pointer = true)]
[assembly: GoImplement<Label, positioner>(Pointer = true)]
[assembly: GoImplement<Map, go.go.types_package.Type>(Pointer = true)]
[assembly: GoImplement<Named, cleaner>(Pointer = true)]
[assembly: GoImplement<Named, genericType>(Pointer = true)]
[assembly: GoImplement<Named, go.go.types_package.Type>(Pointer = true)]
[assembly: GoImplement<Nil, go.go.types_package.Object>(Pointer = true)]
[assembly: GoImplement<PkgName, go.go.types_package.Object>(Pointer = true)]
[assembly: GoImplement<PkgName, positioner>(Pointer = true)]
[assembly: GoImplement<Pointer, go.go.types_package.Type>(Pointer = true)]
[assembly: GoImplement<Signature, genericType>(Pointer = true)]
[assembly: GoImplement<Signature, go.go.types_package.Type>(Pointer = true)]
[assembly: GoImplement<Slice, go.go.types_package.Type>(Pointer = true)]
[assembly: GoImplement<StdSizes, Sizes>(Pointer = true)]
[assembly: GoImplement<Struct, go.go.types_package.Type>(Pointer = true)]
[assembly: GoImplement<TypeName, go.go.types_package.Object>(Pointer = true)]
[assembly: GoImplement<TypeName, positioner>(Pointer = true)]
[assembly: GoImplement<TypeParam, cleaner>(Pointer = true)]
[assembly: GoImplement<TypeParam, go.go.types_package.Type>(Pointer = true)]
[assembly: GoImplement<Union, go.go.types_package.Type>(Pointer = true)]
[assembly: GoImplement<Var, go.go.types_package.Object>(Pointer = true)]
[assembly: GoImplement<Var, positioner>(Pointer = true)]
[assembly: GoImplement<atPos, positioner>]
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<constDecl, decl>]
[assembly: GoImplement<funcDecl, decl>]
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
[assembly: GoImplement<go.go.types_package.Array, go.go.types_package.Type>(Pointer = true)]
[assembly: GoImplement<go.go.types_package.Func, go.go.types_package.Object>(Pointer = true)]
[assembly: GoImplement<go.go.types_package.Func, positioner>(Pointer = true)]
[assembly: GoImplement<go.go.types_package.Object, positioner>]
[assembly: GoImplement<go.go.types_package.Tuple, go.go.types_package.Type>(Pointer = true)]
[assembly: GoImplement<importDecl, decl>]
[assembly: GoImplement<indexedExpr, positioner>(Pointer = true)]
[assembly: GoImplement<lazyObject, go.go.types_package.Object>(Pointer = true)]
[assembly: GoImplement<nodeQueue, go.container.heap_package.Interface>(Pointer = true)]
[assembly: GoImplement<operand, positioner>(Pointer = true)]
[assembly: GoImplement<posSpan, positioner>]
[assembly: GoImplement<strings_package.Builder, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<typeDecl, decl>]
[assembly: GoImplement<typeParamsById, sort_package.Interface>]
[assembly: GoImplement<varDecl, decl>]
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
[assembly: GoFieldAccess(typeof(Info))]
[assembly: GoFieldAccess(typeof(Interface))]
[assembly: GoFieldAccess(typeof(Label))]
[assembly: GoFieldAccess(typeof(Map))]
[assembly: GoFieldAccess(typeof(Named))]
[assembly: GoFieldAccess(typeof(Package))]
[assembly: GoFieldAccess(typeof(PkgName))]
[assembly: GoFieldAccess(typeof(Pointer))]
[assembly: GoFieldAccess(typeof(Scope))]
[assembly: GoFieldAccess(typeof(Selection))]
[assembly: GoFieldAccess(typeof(Signature))]
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
[assembly: GoFieldAccess(typeof(instance))]
[assembly: GoFieldAccess(typeof(lazyObject))]
[assembly: GoFieldAccess(typeof(monoVertex))]
[assembly: GoFieldAccess(typeof(operand))]
[assembly: GoFieldAccess(typeof(term))]
[assembly: GoFieldAccess(typeof(typeWriter))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("go/types/alias.go", "alias.cs", "ADRwopSCqpCUnsCYkJiQmKKCqpCYkJ7SgpSmooKUgoK4gqqigqrSgoKCgoK4gqas0oKCgoKCgoKmpg==")]
[assembly: GoPositionMap("go/types/api.go", "api.cs", "AEOKAaIACBKCAG74AcIAfo4CgqqigpSCgqYAAhTygpQAAhIACAKCgpSUggAKGqKokqqirLKUlKqiqqKqoqqiABMogoKCgpSUgoIAAhwACwKC")]
[assembly: GoPositionMap("go/types/api_predicates.go", "api_predicates.cs", "AAwmAAkGgpQAAhDSgoIAAhDSgq7ClLiClK7CAAIiAA4CgqqigoI=")]
[assembly: GoPositionMap("go/types/array.go", "array.cs", "AA8kkJiQloCUgg==")]
[assembly: GoPositionMap("go/types/assignments.go", "assignments.cs", "ABMwAAgClJSkAAoOgoKmgtyCgoKCgpSCpoKCgoKUpoKCgpS0tKSCgpSCgpSCgtyCgoLcgqaCgIKClJQACAiygoKUuIKCgpSUpoKmgoKmrvKCgpSCuIKClIKCgoKUlKau9KaCgqyygoK4goLKgpSCpoLKlOiCgoKCgqaCpuzSgoKClJSmgpSCgqaCpoKUqMKClLjCgpQACxQACQKCgoKChLSkyri2poKmgpS0tpSmgoKUAAgGgoKClIKCgqYACAaCkoKCgoKClIKCgoIACg7igoKmyIKCyoKCgoKClIKUypSCgoKUuIKCpqaCgoKCuIKUyoKClLiCgt6yyIKCyoKClMqUgoKClKaCgoKCuIKUyoKUuLKCpoKCgoKCgoKUgoKmgoKCgoKU7oKUgpSClLiCgoKUuIKCuKaUgoIABxCCgg==")]
[assembly: GoPositionMap("go/types/badlinkname.go", "badlinkname.cs", "AAko")]
[assembly: GoPositionMap("go/types/basic.go", "basic.cs", "AEaYAYCWgJaAlII=")]
[assembly: GoPositionMap("go/types/builtins.go", "builtins.cs", "ACkuAAwCpoKCpoIABxCCgpSssoKmgoKCgriC2MiCgoKCgpSCgrgADyCCgoKqooKAgoKCgpSUlIKmlILslIKCuIKU7IKCpIKCgpTayoKCgpT6toLIgpSCpIL2pIK2lMiUgoKClJS4gqaCgsiUgqSUgpSmgoLagoKCgpSCgpSUlIKC2qaCgpSClOrGAAsUgoKCpoKUgtqCuIKCyoKCgpSkpKaUgoKCuIKUpoKmAA0agoKAgoKClKaC7KaCgoKUgpSCgpSUgoK4gpSC7IKCgoKCgpSCgpSClKaCgoKmgoIACBCCpoLulILugoKClKSkppSCgoKClIK4goKUpqaCpuyCgoKmgrSkpKaCgqaClMTGpIKCpoKCgoKCgqaCpoKCguyUgoKmgoKmgoK4goKCpoKCpoKCptyClIKCyoKmgoKClOyCgoKUtsaUgoKUpraCgoKC/pSClIKUpoKCpoKCzKKCgoKCgpS4goLagoKC2pSCgqaCgqaCgoLagoKmgoKCpoKm2oKCgoKCpoKCpoKCgpSC7IKUgoK4poKCgpTugoKCpoKCgpSCpsiCgqaCgoKmgoKClIKmyJSCgoKCpoKCpoKCgtqUgoKCgqaCgoLalIKCpoKCpoKCgtqUgoKmgoKC/oKClIKClIIACxaCgoKUgoKCgoKUgtq2gtrCgoKCgqaUpoKCgqasAAkIgIKClIKUgqLIpKSCgtjEpAAGFAAKAoCGooKClIKClJSuwpS0tLSkyoKClLaqwoKClIKCgoKUqqKCgqY=", "42-44:1;192-206:2;236-243:3;254-265:4;304-308:5;330-343:6;426-438:7;480-493:8;531-540:9;1025-1027:1;1059-1068:1", "", "", "", "73:68,74:17,1142:58,1143:21")]
[assembly: GoPositionMap("go/types/call.go", "call.cs", "ABFEABcClIKCgpSUlpKCgoKCgoKU3IKSlIKCpoKCAAIaAAoCgoKCgoKUpoK4goKCypSCgoKClIKUlKaCgoIACAbigpSCgoKCgu6CgoKmopSUgoKUlLjmwoKCuJSUgpS4lIKCuIKClIKCpLSCgoKClIKCgqbGgqSCuIKClJSClMq4goKUlIKCgoKUppaSgoKCgoKCgoKUppKCgoKCggAHEIKCpoLKgpSUuJS0gpSUtIKkgriCpqrCgoKUgoKCgqYAAhQADQKClIKClAAIEoKCgqaUgoKUgpSUuJSmgoKUgoK4ppSCgoKClIKUlLi4lKgABRwAEQIAChaCgqaCgoKUlIK4uIKCgoKClIKCpriUgtyCgoKCgpSWkoKUgoKCgoKotoKCgqSCtLiSgpSCppaSgpTuyoKCgoKUgqaClLim7oKCgsqClLiCuIKUyoKSgoKUgoLKgoKCuKYAGBbEzMqCgoKCgoKUgoK4gpSUpoKCpoKClJSUgoKUgpSUppSCuLiUgoKCtIK0goKCxoKCgoLGgoLEgqSCuIKmgrQAESKCgqaClIKmlIKmgoKUlKaCgpSClIK4gIK2lIKCgqaUgoKCqqKCAAcQgpSCgqaCgu6olIKClJTaAAcQgtyC3IIAChaCgoLcuKaCgpTG2oKUgoKCAAIQ8KrQpIKCgoKmpqKCgvqCmrKCgriCgriCgtak", "139-142:1;154-167:2;249-254:1;367-377:1", "", "174=describef/1/1/13;981=NewTuple/1/1/2", "", "159:75,160:17,417:70,419:17")]
[assembly: GoPositionMap("go/types/chan.go", "chan.cs", "ABc2kqiAloCUgg==")]
[assembly: GoPositionMap("go/types/check.go", "check.cs", "ABksAAsYABYsgoKCgoKUABtKAAgCgoKmqJKCABk4woIAQIwBkoKClIKUAAISAAgCgoKUgqiygoKokoKmgoKCgpQAAhLigoKokoKUgqiSgoKCggAGEqKq5IK4ggAJFJQAEB74goKUgoKCgoKC3IKmgoK0gpSUpqbcgoKUlIKCyrimAAoWyqamuIKClKiSqJIACQzi1sSCgoK4goKmgoLKxKbyuO6CggAbDgAKBoKClJSClKaCgoK4gpSClIKUgpSClIKUgpSClIKCpoKUlKaCpoKCgoKCgoKC2OYACQ6CgoKCgpSmgpSCpoKCqKSClKrc3KKClLTUpJSkuA==", "455-459:1;514-519:1")]
[assembly: GoPositionMap("go/types/const.go", "const.cs", "ABYs0pS4gu6CgqiSgoKClIIAAyAAEAKCpoKCppKCppSCgpSClIKUgqSCpIKkgqSkgIKkpIKkgqSCpKTIpIKkpMiCgpSUgpSCgoLGgpSCgoLGpNiCgpSUgpSCgoKCxoKUgoKCgsak2KampoKCgqaCgoKClKaCgqaCgoKUqsKCgoKClIKu8oKCggAHEIKUppQACgaCgpS0pKjCgoKCgpSCgpSCgpSCgg==", "76-79:1")]
[assembly: GoPositionMap("go/types/context.go", "context.cs", "ADZ0kgAFErKCgpSCuIKmpqrygpSCgpSUuN4ACQKUgpSCgpSUuNzY4oKCgoKCgpQ=")]
[assembly: GoPositionMap("go/types/conversions.go", "conversions.cs", "ABIowpSC6KSCgpSClJSmgoKmuIKCggAJEKSCgpSClIKUlJSUlMaCtoKClJSC3IIACBKUgoKClKYACCIAEwSCpoKCgoKCgriC3IKCgtyCuIK4griCyoKmgsqClIKCppSUxoKCgqaClP6CppKCgoKU3JSCsoKUgrKClIKClMiCsoKUgoKClLaigpSCgpS4poKCpoKCpoKCpoKCgpQ=", "23-40:1;62-81:2;237-245:1;252-267:2;257-266:2.1;270-280:3;282-291:4")]
[assembly: GoPositionMap("go/types/cycles.go", "cycles.cs", "AA0cwoKCggAIMAAYAoKmgoKCpqamlpKCpoK4griCggAHEIKCuMqCpoKCAAQQ8pSCpoKCgpSClJSCuIKCpsqmgoLa")]
[assembly: GoPositionMap("go/types/decl.go", "decl.cs", "ABEg6oKCgoKCgpSUgrqSgoKClJTo0oKClIKUuIKClIKCgoIAIEKCtILGgsYACQ62gqaCyoKUgqaClO6UgrSCxoLGxMQAEAgACASCgoKCgoLKgoKCgoKCgoK06oKCAAIYAAkCgpSClKaCAAkK6IKCgpSUgoKU3ILcgtyCpoLo+qqCgoKCgqaC3IKClJSmgoKUpoKCgpSqopKCgqYAHiqCgoKClIKCuILYgoKUtKaClIK0gqSCtIK01sToxMjSpoKClIKmpoKCpoKUgpSokoLulJSi5KKmggALGIKUlKaCgoKCgqaUggAHEIKCuKiSgoKUggAMBuKmlIKCgqaokoK4poKUgqaCuJKCgsqCgoKmgoK4goIABxCUgoKmgoKUuIKmAAoWgpCkgoKCpoKCuIKCtOTiuIKCgu4ACBKCgoKmgoKmgoLKgqaUgpS0pLiClLSkgoKUgpSUpgAJDoKCgqb6goKUgpbIgoK4ktyCgoLKpoKCgpSUpoLKgoKCgoKCyoKCpriCgoLuwqaUgpSCuIKUgsqCosqilJKUpoKCgpSCgqa43IKCyJSCgriCgoKmxoK0graC3IKCpsq4gpTIuIKCgoLE", "52-58:1;67-70:2;140-142:3;240-246:1;278-280:1;412-415:1;524-530:1;554-559:2;615-615:3;661-663:1;749-751:1;837-839:1;846-942:1", "", "659=Name/1/1/6,describef/1/1/6;913=describef/1/1/2;1006=describef/1/1/2", "", "64:62,66:17,79:62,80:17,162:73,163:13,306:65,307:17,522:98,523:13,690:67,691:21,752:59,753:13,813:68,814:13")]
[assembly: GoPositionMap("go/types/errors.go", "errors.cs", "ABckgoKmgpQAEyqSgpQAAhIACAKokri4gqaCgpTokoKmgoKCgoKCppQACgiSggAHEIKUgoK4gtyCgoKCgsqCgoKmuKiSlO6CgriCuIKCgpTcpoIACRTKgoKCpoKmgoKUAAwagoKCpqKCgqaigoKCpqKCgoLcggALGIKsspKClKyylLSkgqSCgpS0gg==", "", "", "212=stripAnnotations/1/1/3")]
[assembly: GoPositionMap("go/types/errsupport.go", "errsupport.cs", "ABIeAB82AAYSgoKCgoKCgoKCgoKUgoKUgriClKSkpKaClLSklKSkpLiqooKCpg==")]
[assembly: GoPositionMap("go/types/eval.go", "eval.cs", "ABEwAA0EgoKmpoIAAioAGASSgoKCgtyCgqaCgoKmgtyCgoKmkoKClA==")]
[assembly: GoPositionMap("go/types/expr.go", "expr.cs", "AD1+lAAHEKKCgoKmgpSqopSktLyilILGgramygANEtKCgqaCuIKCgpSCgqaCgoKClIK4goKClIK2goKmgpSUgoKUgoKCpr7SgoKUgoKUgpSClKaCpoKCgqamgqampoKmlJSUAAYYAAsCgoK4ABIkgoKUABEaAAgQgpS2gpSmpqaC2NqCgoLKlLiCgtyUgoKCyqiSgoIAAxQACAKCuJSClKakgoKClNyUgsaC/ILYgqakxoKCgpSClKaClNyCpoKU9IKmtJSo1IKCpoKmgriCgoKUuIKCuIKmpoKClMqCyIKCtoKC6pSCtILYyIK4yoLKgpSEkpSClKaCgpSmgpSUqqK0pqj2goKmyoKCAAUQopSCgoKCpqaCgoLKlIKC6oKCgsaCgriClIKUgpSWkoKCgoLcgqaCgoK4ABMogoKmgsqCgoKmypQAECbClIKUgpSCgoKmgoKmgoKmgoKmpoKCgpSClKaCpoKCppSCgoK4goKCgoKCypSClKaClIKCgqa8AA4UlIKmgtyCpoKmgqaClIK4gpSmgoKClIKCggATMKKCgqYABhQACwKCgoKCgriUggAIEoKUAAsM0oKUgrSCxoKCgpS2goKCvtKClIKCAAsOAAkGgpSUtsiCtoKCyIKCyIKC2oKCptaCgoKUlILIgoLIgoKmpoKUgoKUgoKUgoKUgoK2poKUtIK0goKCgoKUgoKUgpSUgtiCgpSCgsiCgtqCAAkIggAJEMiClIKCggACFgAIAqSCgoKClIKkgoKClIKkgIKkgILGpJSosoKCpoKCpgACENKCgqiSgoIAAhAACAKCgpSUgoKUuIKCgoKUgqassoKCgq7CgoIADArCgoKClIKUlLSCtIK0pIK6spSCgoI=", "198-213:1;442-448:1;885-922:1;976-979:1;1149-1161:1", "", "", "", "1066:62,1067:17")]
[assembly: GoPositionMap("go/types/exprstring.go", "exprstring.cs", "AA4ksoKCABQMAAoMpLa2goLItoKCtoKCgpS2goK2goLWgoKCgraCgoKUgoKUgoKCpraCgoK2goKCgpS2graCtoKCgoK2goKUgraCgraCtoKCtoKCgraClLS0pILIooKClIKClKaClIK4goKmooKCuKaCgriCptyigoKUuKKCgpQ=")]
[assembly: GoPositionMap("go/types/format.go", "format.cs", "ABUkoqKUtLS0goKCgpSUgrSCxrSCgoKCtLSCgoKCtIKCgoKCgpSUgrSCgoKCgoKUlIKklKjCgoKCgpSmsriCggAGFrKUpKS6wqa0goKCgqaClJSqwoKUlIKCgpSUgrqSgpSCpoKU", "", "", "143=Repeat/1/1/3,sprintf/1/1/4")]
[assembly: GoPositionMap("go/types/gccgosizes.go", "gccgosizes.cs", "AAsa")]
[assembly: GoPositionMap("go/types/gcsizes.go", "gcsizes.cs", "ABAe0oLKyKQABxDcgoKCpgAIDIK2guaklIKmgpSClOaCgoKClIKmgoKCgpSmpqKkgoKCgqaCxoKCpoKClIKYooKUpKSCgpSCgoKCuIKmyILEpKyygpQ=", "16-18:1", "", "", "", "23:100,24:13")]
[assembly: GoPositionMap("go/types/index.go", "index.cs", "ABQm4qaUgriUgoKUppTagoK4goKUgoKCyoLIgoKClLaCgoKCyIKCtoKCgpSCgpSCgoKmgpailIKCkpSCgsaCgoLGgoLGtIKkgpSUgoK4gqaCpoKUpoKCgoKUgoKUgoKCuIKCyJSCgoKmgoKC3IKmguaygoKCqKKUgoKCgriCgriCgriCgoKmlKaCgriCgqaCpoKCgoKUgoKUgoK4gtqCgoKCgpS2goKCyMiCgoKmpoKCgqiSgoLKgoKUgtjGpMqCgoKCuIKCAAgU0oKClJSU7vKClIKCgqaCpoKmgoKCgrimooK4goK4goKmlIKCuIKCuAATKIKmgpQACBAABxA=", "112-161:1;222-249:1")]
[assembly: GoPositionMap("go/types/infer.go", "infer.cs", "ABxGABYKgpK4goKSyoKmpoLKgoLcgoKCAB4+goLclLSCuIKCgoKmgoKmlMqCgpSmutqCgqaCypSCpoKCgpQAChbKgoLcggARJIKCgoKUpoKCgoLKlJSCAAsYuILGgsoAESKCggABEOKCpILKgriCgtyCrLKSgoKCgpSCgpSCgoKUlMqCgoIACxiCgoIAGjaasoKCuIKCyoKCgoIADBqClIKCpu6CgoKCqAAFFAAkOIKmgoKCgqaCgqbqtIKUpKSokoKClJSCgqyyuAAHEOSClIKCptqmpqamAAcQAAkUpoKCgqaqpqaCgtq2tuaCgoKmruKCgoKCgoKUgoKClJSCgoKUlMqUAAIU8oKCAAkUwoK4gpTKlIKUAAgQtra27oKUgsiCyIKUgsiCtraCyILYxKSCgg==", "41-43:1;48-50:2;119-154:3;305-305:4;567-569:1;615-617:2;659-670:1", "", "", "773:27", "54:102,55:17,62:60,63:17,603:67,604:13")]
[assembly: GoPositionMap("go/types/initorder.go", "initorder.cs", "ABgo1riClIKCgoKUgoKCgqa4lIKCgoKmlAAIEoKUlILKggAIEoIACBKCgriCgoLugpSUgoKUgqaCgoKClL6ygpSWkoKUqIKClIK4qJKmgoKmgpSCgoKUABo4osqCgpSs5IKUgu6UlIKCgsqSgoKUAA0chqaUppSCgqamgsqCgqYACRSUgoKCpoKmgoKCyqaCpoKCgoKC", "151-153:1;273-275:1", "", "81=Name/1/1/1,order/1/1/1")]
[assembly: GoPositionMap("go/types/instantiate.go", "instantiate.cs", "ABhsAB4CgoKUlIKCgoKUkriCAAIiABMM0oKUgpS4goLcsoKUyoKCuJTIgsqUgqa2lJSClIKUuIKCuILYyKyygpS0tKaCgoKmqJKClMqCgoKmAAgSAAgCgoKClIKmgoKmgoKCgoKUlJS4gtyCgsqCgpS4koKUuJKCuIK4lIKUgpSUgpTKgtyClIKUlKiSgriCgoKCppSUgoKUtLSklKaqopSCgtiCguiCtg==", "115-120:1;291-316:1;340-355:2")]
[assembly: GoPositionMap("go/types/interface.go", "interface.cs", "ABs6sNae0oKClAACEuKCuIKCgsqUgoKUqLKCgpSuwqiAmJCWgJywloCWgJiQloCWgJiQloAAARIACQKClIKmgprCgoIACAbSgoKClKaCgoLKgoKCpoKCgoKUuIKCgpSokoKCppSCgsqUlIK43II=", "160-166:1;231-233:2", "", "253=describef/1/1/2")]
[assembly: GoPositionMap("go/types/iter.go", "iter.cs", "AAkoAAsCgoKCAAUUwoKCggAFErKCgoIABRKygoKCAAUSsoKCggAFErKCgoIABRKygoKCAAUSsoKCggAFErKCgoIABRKygoKCAAUSsoKCgg==", "21-27:1;35-41:1;48-54:1;61-67:1;74-80:1;87-93:1;100-106:1;113-119:1;126-132:1;139-145:1;152-158:1")]
[assembly: GoPositionMap("go/types/labels.go", "labels.cs", "ABYexJTcgoKCgoKCgpSClLiigoIACxzSgoKUgoKClKqigoKmqqKCgqasspQABhCCgqaYtqaCwpSC2oCCgoKCgoKCpoKmgoKUgoKCAAkWgqaCpLaCuILKgoCC1MaCguyCgIKkxoKCyJSCyIK4goK2gsi2goLItra2tra2yIKm", "107-110:1;112-114:2;116-120:3;123-264:4", "", "173=Position/1/1/5")]
[assembly: GoPositionMap("go/types/literals.go", "literals.cs", "ABIq0oKCpoKClIKUgoKClIKClIK4spQAABQACQKCgoK2gsqCgqaC5sKApoKCuILKsqaClIIAFAjCkpTKuIKClILIgpSCgpTIlIK2gKSCyoKUgoKCgoKUpoKCgpSCgoKClIKClIKCgpSCgpS4goKClIKCgqaCgoKUgpSC7AAIEoLKgtrsgoKCgoKClIKCgpSCgoKCgoKCppSClIKCpoL8oriUpoKCgpSCgpSCgtiC7sKCkpSCgoKCgoKUpoKClLiCgpSUgoKokoKU", "99-101:1", "", "115=describef/1/1/2")]
[assembly: GoPositionMap("go/types/lookup.go", "lookup.cs", "AAxKABcCgoKUpLS0pAACRgAgAoKUqgANEICCgoKClMicwoKCgoK4AAIiABQGgtymgoLKnvaCpoK4gtyUpqaCgpSCgrimgoKCgoKUgoIAChaCuOyAgoKCgpSC6tyUgqampgAKHLKCpoKCgoKmgoKmppSAgraCgrgACRSCgoKmgoKmpoKCgoKmgpSCAAIYAAkCAAIYAAsCgoKmAAgWgoKUgoKilIKClIKmgoK4oqaClLS0goKCgoKmyLiCgoK4gqaCgsqCpoKmgqaUlLS01pK0tJKmlAAHEIKUtLS0tMgAAhLigpSCqqKCgpSCgoK4poKCqJKCgpSqwoKCgpSCgoKCAAISAAkIgqYAAhAACAiClKyylIKClJSUqqKCgqaqooKCqqKCgoK4qqKCgoK4")]
[assembly: GoPositionMap("go/types/map.go", "map.cs", "AA4gkqiAloCUgg==")]
[assembly: GoPositionMap("go/types/methodset.go", "methodset.cs", "ABksgoKmgoKClIKogJaAlrKCpoKCgpSCgoKmAAYaABEWgIK4pKaCuJ72gpaSlIK4gtyUlIK4lIKClNyCuNrsopSClIKU3IKCgpS4poKokoKCgriWAAcWsoKUgpSmooKUlMqCgqaC", "49-52:1;204-206:1")]
[assembly: GoPositionMap("go/types/mono.go", "mono.cs", "AFCsAQAIDoKClIKCuIKCpoKCgoKmgsqygtyCgoLcgsqCgpSCkoKUlLS0tqqigpSqooKCgpS6AAsSgriCgoKmqqKStLaCtoKmgoLI+LSCtLS2gsaCgqaCtILIqsKCgqaCgqaCpriCgoKCgoKm3IKUgqiygqaUgqaCpoKCgqaU", "201-208:1;213-263:2;251-255:2.1")]
[assembly: GoPositionMap("go/types/named.go", "named.cs", "ALUB/gKygpSCgoKUlAACLgAXAoLKgqaCuICkgoKUgpSClIKClJQACRSCgpSClIKClIKCuILYkqqilIKClIKmgqaCpoLMwoKCpoKUAAISAAoClILugpSClIKUpsaClKqigpSqwoKUqpCYwoKokoKUqJIAAhwAEAKUgqaClIKUgoKmgoKCuIKCuNrmgpSUgqaCAAgSgqa4goKCgpSmpoKmgoKUpoKq8oKClIKmgpSCgpSCgqKq4oKCgoK+soKUgoKCuIKCuKzSgpQAAhDyyoKCgriCpqYAASYAEwKUgoKmgoKClLa2goK4goKmgpSUgoLGyJLCgsiCgrTIsoK4lKaokoKUAAhEACQCgoKCgpKCuIKUgqaClIKUgqaCgpSCgqbKgqaClIKCgpSUgqaCpqjewoKU", "241-241:1;676-686:1;748-751:1", "", "724=Invoke/1/1/10", "", "238:97,239:13,819:62,820:17")]
[assembly: GoPositionMap("go/types/object.go", "object.cs", "AD+KAYKCqsKC7qaClAANIJCWgJiQloCWgJqgloCUgoKUoKKSgICilLSC3IKmgqYAAhoADQKCuIKUgriCgoKClLiClIKmAAkYoqqQAAcUoqiAlAAMJPKqooKCgqiylNqCAAcQpKS0ABYyAAkUgoKUqICYkJ7SrsKssoKCqqKqkJaAloAAARIACQKClKYADBqigoLuqJKCAAkUqqKCgqygAAESAAkCgpSusJbagoIABxCmAAoUkgAJFIIAFRCigpSUgoKUpraCgoLIgpTIgoKClKaCtoK2gra2poKUlIKmgrikgraCgoKUgrjugoKmgqaigpSCgpSUgpSssoKCpoKCgoKCgoLUooKCgoLKlJSCgoKmAB4IkpSkpIKCgpTGlKSkpKSkxoKUxqSklIKU")]
[assembly: GoPositionMap("go/types/objset.go", "objset.cs", "ABIwwoKClIKUgg==")]
[assembly: GoPositionMap("go/types/operand.go", "operand.cs", "ACROABs8tIKUAA5OACUEgoKUpKS4griUgoKUlLS02oKCuILqgoKCgpTapoKCgsqCgoKC7oKCpIKUlNqClIKClIK4yoKmABQMspSkpKSkpKSkpKSkpMiCqJKClLS0tLS0toKCgoKUgoKokoKUAAcUAAkCgqaCgqaCpoKCgqaCgqaCgsqCpoLugtyAgoK4gpSC2oKUgpTugIKC2oKmkoKCgpTcgoKCooKUgoKClJTcgoKCgrKClIKCgoKUlKY=", "337-346:1;402-410:2;417-427:3;438-449:4")]
[assembly: GoPositionMap("go/types/package.go", "package.cs", "ABg4ooKogJaAloCcsJzigpSqkJaAAAEWAAgAmJCUgg==")]
[assembly: GoPositionMap("go/types/pointer.go", "pointer.cs", "AA4ggJaAlII=")]
[assembly: GoPositionMap("go/types/predicates.go", "predicates.cs", "AA4kgJyggoKCgoKCgoKasoKuoIKCgoKCgpqygIKkrLK0lKyypJSs1oKqoqrGgqiSgqiSqJKCrsKCgpSsxIKUgqiSAAgMsoKUgpSUtoIACg6CgtqCyIKUgoKUlLamqJKk9KSYqNSCpgAIEqIACBTCgpSCpoKmyoLsptqCAAgQgoKCgoim7ILsgoKCgoKCuNqCggAIEoLKgpSmgpSCgpSUgqaCgoK4gqaqpoKCggALFoKCgoKUgpSCggAWLoKCgpSUgoKUgoKCpuyC7IL+uIKCgpSCgqYADRS2qKSssoKmrNaClKSkpKSkpq7mgpSUgpSUqNKCqJKCgqY=", "59-59:1;218-220:1", "", "329=Tag/1/2/1,Tag/2/2/1,sameId/1/1/2,identical/1/1/3;411=identical/1/2/1,identical/2/2/2")]
[assembly: GoPositionMap("go/types/range.go", "range.cs", "ABk0AA8EAAkSgpSCgriCgpTKAAkWooKGlLS0tLSkyoLcgpSUhJKCgqiSlIKClIKmgpSmgpSCgqaCgpSCgoKClLiCgpKmlKSCgriCgqaCgriCpoKCgoKm7qaiABQQAAsCmJSClJSCppSClIKClMakpKSCpIKmlKSUlIKClIKUxraClLaUgpSClJQ=", "68-70:1;206-208:1;210-216:2", "", "70=MakeInt64/1/1/4")]
[assembly: GoPositionMap("go/types/recording.go", "recording.cs", "ABAkpqKClLS0grSklKaU+KKCpoKCgpS4goKCgpSCppSClKbKgoK0pMTeooKCgpSCgoKCgoKCqJSCgpSmAAIS4oKCgoK4goKCuIKCgoK4goKCgriigoKCuIKCgoI=", "", "", "131=newVar/1/2/1,newVar/2/2/2")]
[assembly: GoPositionMap("go/types/resolver.go", "resolver.cs", "ACJKoqiSgoKClO4ACAKCgoKmgqaCxpSCuNiCAAgIgoKClIKUgoKCpgAICtK4goLKgoKmgoKokoKClOb8goKCuIKClIKClpKCgoKCgqaCgsqCgpSCgqaCgIKkgIKkpsqCuIKUuAAQDNIAARDigpaegoK4ypKClIKCypSypoKUgoKCpoKCuIKCgpSCuIKC3IKCpoKUlKaUuIKUgqbc3IKCgoKUgu7YgpSCgqaCyIrCuLiCgpSChJKClKbGgrSCgoKUgqaCgoKUgoKUlKaUgoKCpgAJFLiClJSCyoLsgqKCgoKCgpSUlAAIEoKmgoKUgoKCzJKCgoKClAAGHAAPBIKCgrikgoKCgoKU+MSkgpTaAAMQ+LKmgoK4goK4griCgsqCuIKUpqaUgpSC7oKCyqjkgoK4ABAiggACENKUgoKClKa4kqaSAAcQqMSC7oKCygAIDoKCgpSClAADEMKCpg==", "261-460:1;510-512:1")]
[assembly: GoPositionMap("go/types/return.go", "return.cs", "AAwisqQAEBC4gsimgsimhsimpoKCgqimgsimlIKCpqaigoKCgpSCpgACFPKkABEQpoKClILapoTIpoLIgsimgsiCyILIpoKCgqY=", "", "", "59=isTerminating/1/2/1,isTerminating/2/2/2;161=hasBreak/2/2/1")]
[assembly: GoPositionMap("go/types/scope.go", "scope.cs", "ACBKwpSCgpSogJaAlpKCgoKClIKogJaAmKIACBKClKzSgoKCpgADEPKCgpTcgpQAAhQACQKClIKmgoKUAAYQ8oKUlIKCpoKCuKiSgoIACxyigIKSlIKUgqaClKakqpCCgoKCgoKCgoKCgoKCgg==", "191-205:1")]
[assembly: GoPositionMap("go/types/scope2.go", "scope2.cs", "AAwwAA4CgoKmrrCCmrIAAhAACQaCgoLKgoKCppQ=")]
[assembly: GoPositionMap("go/types/selection.go", "selection.cs", "AE6oAYCWgJiQmKK4goKCgu6CgoKCgoKUgrgAAhoACQAAARLglAAJFgALAoKUtLS0pIKCgoKCgoKUlA==")]
[assembly: GoPositionMap("go/types/signature.go", "signature.cs", "ACZiAA4CAAIkABECgoKClIKCgoKUgoIADRyUgoKCgqaCpoKCgpSUgoKUlAACEtCWgJaAmJCWgJaAlILcAAwCgoKCgpaSgpSCuIK4uIKUuIKmgoKUgpSCgoKCoqoADA6WkoLcyoKCgoKCgpSssoKClNiC+raCAAgSgoKCuIKUuIKCgoKCgrimgsqCgoKUgqaCprqSgoKUqqKU3ILKoqamgoKCgpQAAxwACgKCgpS0poKClMQAChIACgKCppKCgoKCgpS4ppSCgqaUgpSmgoKCgriC7oKCgqaosoKCAAoO5IKCgsqUgoKUgraC5tikgsbEAAoKkg==", "351-353:1", "", "381=describef/1/1/2;597=File/1/1/1,Name/1/1/1,Base/1/1/1,HasPrefix/2/2/1")]
[assembly: GoPositionMap("go/types/sizes.go", "sizes.cs", "ADNq0oLKyKQABxDcgoKCpgAIDIK2guaklIKmgpSClOaCgoKUgqyCgoKClIKmgoKCgpSmpgAPIKKkgoKCgqaCxoKCpoKClIKmgoKCpoSSgpSkpIKClIKCgoKUyILEpLgAFDbilICC1oCCxsiUgoKClIKUpqKClIKClJSCpgACENKCgoKCgpSCgpSUqqKCgpSssoI=", "54-56:1", "", "142=Pkg/1/3/1,Pkg/2/3/2,Path/1/2/2,Pkg/3/3/3,Path/2/2/3", "", "58:101,59:13")]
[assembly: GoPositionMap("go/types/slice.go", "slice.cs", "AA4ggJaAlII=")]
[assembly: GoPositionMap("go/types/stmt.go", "stmt.cs", "ABQk8oKmgsqCgpTulJSCpoLKoqTChoKigoKmhoKmpoIAEyyCgriCgoKmpoKCgoKCgoKU+KKCgoKUgsaC1qSCgpTcgoKCpoKmlIKUAAgGgoKCgpSCgoLGgrSkpKikgtyUgIKkgILWgILWlAAPIqKCgoKCgpSCgqaCgoKUgqamgoKCgoKCpuykgoKUAAIwABgCgoKClIKClIKCyoKUgoKUgoKCgqaCgvyCgoK4gqrSgoKChJKCgoKUgoKUuICUgoKUgoKCgqSCgvyCgoK4ggA2CPSClIKUyqaU+raCvLKCgoKUgpSCtIK0gqS2koKCgpSCyIKUtLSCpoKCgpSCgqaCgoKUtpSCgpSCptqCgpSCgoKUgoKClNi2tqa4goKCgoLKgoKUyIKClJSCxoLGgoKUtLSkxtiClLaClIKCgoKUpvi02IKClIKCgqaCgoLKgoKCppSCgoKCgpSCgoKClJSCyIKClAABFAAIAoKUtIKCpoKCgqaUgpSmxoK4goKCpoKCgoKCgoKClMqUgoKCgoKCpoKUgoKCuJSC7oKCkoKUlILalJSCgoK4goK0tILGyIKCuIKCpoKClILIgoKUgoKCgoKmpoK4lLaCxsQ=", "29-32:1;58-60:1;68-70:2;414-420:1", "", "", "", "39:87,40:13,533:73,535:17")]
[assembly: GoPositionMap("go/types/struct.go", "struct.cs", "ABc2woKCgqaClIKCqICWgJaSgpSmgpqiggANCMKCgoKokpamkoKCgpSCpoKClIKCAAcQkoKCpoKCgpSC7oKCgoKCgoKUAAcQgpSigpSUpoLGtLiClIL+goKmgpS2gsakpJSmooKCgoKClKaigoKCppQ=", "85-101:1;107-111:2;146-172:3", "", "161=describef/1/1/26")]
[assembly: GoPositionMap("go/types/subst.go", "subst.cs", "AA8oooKCgpSqooKCgpSmgqaCgpQAAhTylIK4lKS47gAKFqKmAAkQgoKCuILugILYgoLIgoLIgIKCgtiCgsgADyCUgoKCAA0agLjYgoKCgpSClIKCgoKCAA0cpoKUyIKCgsiCggAJEoKCgqaC7oDK2La2rLKClKaigoKmpqKCgoKmooKAgrau4oKClIKUpqaigoKmpqKCgoKmooKUAAISAAkCgoKCgriCgpSCgqY=")]
[assembly: GoPositionMap("go/types/termlist.go", "termlist.cs", "ABQq3JKClIKCgpSUqMiCgqaoyIKCpqjWgoKigpSCgoKU7oKUgqaUuJKosoKqooKCgriopKiSgoKmqJKCgqaokoK4goKm")]
[assembly: GoPositionMap("go/types/tuple.go", "tuple.cs", "ABIksoKUqLKClKiAlII=")]
[assembly: GoPositionMap("go/types/typelists.go", "typelists.cs", "AA4ekJaAmtKClP6SgpSqkJaAmtKClKyigpSCgpSU")]
[assembly: GoPositionMap("go/types/typeparam.go", "typeparam.cs", "ABMmkAANKOKo1IKCgpSCgriCgoKUqICYoqiSAAIS4oKUpq7CppqigqiSlpKUlMaUlMiCgoK4lJSClKassq7C")]
[assembly: GoPositionMap("go/types/typeset.go", "typeset.cs", "ACJIgJaAloCWkoKUvICYkJaS5oKUpKaClIKCgoKCpoKClJSClIKUgq6wlrCqsoKmgpSCgpSClIKmrLKClIKCgqbY1uKCAAwagqa4gqaCgoKCAAgSlIKCgpSUAA8ggoKCkoCkgrSCgoKCAAkQgqKCgoKCAAgOgriCgoaigpSCgraClIKUgoKUtIKUgoKUgoLEgpSClOymgoKClJQABRLipoKUgoKCgoKmgoKmgqaCpoKmgoKUggAJFsKCuJSCooKClIKCgpSmlLiCgoKUgqaU", "49-51:1;184-187:1;223-252:2;242-249:2.1", "", "268=describef/1/1/7", "", "203:67,204:17")]
[assembly: GoPositionMap("go/types/typestring.go", "typestring.cs", "AB9GooKUgoKUvrKCgqyyrLIADRyCpoKCpoKCgpSClIKCuIKmgoKUABQGwoKClIKUlNqCgoKmtoKCgraCtoKCgsqClILcgoKUgoKCgpSCuKa2gra2gtqCgpSCgpSClMiCyoKUgoKmgoKCuJSCgoKUgoKUgoKUgoKUgqa2goKCtoKClJSCxrS0pIKClIKC7IKUgpSClMiCgpSAuJSCgtyCgrj8gpSClJSU/MSm0oKCgoKUgoKU2LSChJKClIKUgoKUyKKCgoKUlOaigoK4goKUgpSClJSClIKClKaigqaygoKCgqaCgpSCgoKCAAoWgqa4ptKCgoKCgqamlIKUpoKUgriippKCgoKCgoKCgqY=", "39-44:1;484-486:1", "", "", "142:27", "561:71,562:21")]
[assembly: GoPositionMap("go/types/typeterm.go", "typeterm.cs", "ABQqopSkpKS61JSkuKjUlKSkpKS4ggAHEIKUqNSUpKS4ggAHEIKUqMSUpLiCgpSo1JSkpKS4ggAHEKrCgpSCgpSCgpQ=")]
[assembly: GoPositionMap("go/types/typexpr.go", "typexpr.cs", "ABco8oKUgpSCgoKUpILagoKmyoKCuJKUAA0cgoKClNyCppSCpoKClIKCgpSUlIK2goKU7IKUgoKUtoK2grbGtqqirLKCgqrUgtyCgoKCgpQABhbigoKCgpSCAAIUAAoCgoKCgpSmguqiAAwK8oKCgoKCgqaUgpTK2oKUlOi02IKUlOi0+IKCyqaCgoKmpoKClJSCguy2goKmgoLKgpSmgoKmgoKmgoIABxCSgoKClLimlILYtLTIgoK2graCAA4G4oKCgoKUuIKCgpSCpoKUpoKC7oKCgrjYlIKClJSCgpSUygAMDPqCgoKClIKCuIKCgoKUpoKCgoLcgoKUlIKqooKCgoKUgqY=", "174-185:1;236-249:1;361-369:2;406-410:1;444-465:2", "", "198=describef/1/1/11;428=describef/1/1/8;539=describef/1/1/21", "", "284:66,285:17,501:71,502:17")]
[assembly: GoPositionMap("go/types/under.go", "under.cs", "AAwcoq6igpQAAhLiggAMGKKClKqiAAIoABECkoKCgriCuIKCuIKCgrjYtJTKgqY=", "15-17:1;36-38:1")]
[assembly: GoPositionMap("go/types/unify.go", "unify.cs", "AFO2AcKCyoKCgpSUABwqgpSkpKSUrLKmoqq0goKCgpSUgoKCgoKUgoKUgsqCgpqygpT6tgAIDqSqooKCpqqigoKCgsySqqKCgpSokoKCgqauwoKClKrCgpSuAAwCgoKUgoKUuIK4goKUgpTugoKUABMogoKUppSCAAkUtoKmuJSUgoKCpriCAAkUgpTKABUsgurGlMiUpoK47qaCpoKCgoK4ggAXMIKCgpS4gpSCuIKCpoKCpriCgtyUgoKCgqYADh6CgpTKgoK4kqTKguym2oIACBCCgoKCiKbsguyCgoKCgoK4AAoUgszcgoKCgpSClIKCABYugoKClJSCgpSCgoKm7IL+guy4goKClIKCptqCAA0c7oKC3AALErY=", "290-295:1", "", "651=Tag/1/2/1,Tag/2/2/1,sameId/1/1/2,nify/1/1/3;692=nify/1/2/1,nify/2/2/2", "", "317:55,318:13")]
[assembly: GoPositionMap("go/types/union.go", "union.cs", "ABYuooKUpoKUgsyAlIKCAAsUwoKUlIKCgriUgoKCpoKmgriC3KKCgqaCgoKCgqaCggAHEIKClLS0pMqCyuaigoKCgpTcgoKUlJSCgpSu4oKCgriCgqaCpqrSgoKClA==", "91-134:1", "", "99=describef/1/1/43")]
[assembly: GoPositionMap("go/types/universe.go", "universe.cs", "ACZUAB8+AAoKgoKUggASJoKmyoKCuIKCpoKCgqaClIKCuIKCppSCggAICAAFFIKCuIIALVQAIU6CgoKClP6ygpSCpoKCgpSCgoKUgoKCgoKssoKCgqaCpoKClJS0xLaC", "", "", "159=MakeBool/1/2/5,MakeBool/2/2/6,MakeInt64/1/1/7")]
[assembly: GoPositionMap("go/types/util.go", "util.cs", "ABI48JaAloCWkoKCpqiwpoCWgJaS")]
[assembly: GoPositionMap("go/types/validtype.go", "validtype.cs", "AAwgwgAGGgAQApSCkpSCgoK4uILIpoKC2oKC2oKCABs2ggAVLIKm7oKCgqYABxCCggAKFIKmpoLuuILs2qKCgpQ=", "39-41:1", "", "", "", "51:65,52:17")]
[assembly: GoPositionMap("go/types/version.go", "version.cs", "ABMooqiSqqK6goKCgoKCgoKCppyyqtKCgpQ=")]
// </GoSourcePositionMaps>

namespace go.go;

using static global::go.go.types_package;
using ast = global::go.go.ast_package;
using constant = global::go.go.constant_package;
using errors = global::go.@internal.types.errors_package;
using io = global::go.io_package;
using iter = global::go.iter_package;
using token = global::go.go.token_package;
using ꓸꓸꓸany = variadic<any>;
using ꓸꓸꓸastꓸExpr = variadic<global::go.go.ast_package.Expr>;

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
    internal partial interface genericType {}
    internal partial interface instantiatedType_type {}
    internal partial interface positioner {}
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
    internal partial struct constDecl {}
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
    internal partial struct funcDecl {}
    internal partial struct gcSizes {}
    internal partial struct goVersion {}
    internal partial struct graphNode {}
    internal partial struct ifacePair {}
    internal partial struct importDecl {}
    internal partial struct importKey {}
    internal partial struct indexedExpr {}
    internal partial struct instance {}
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
    internal partial struct typeDecl {}
    internal partial struct typeError {}
    internal partial struct typeParamsById {}
    internal partial struct typeWriter {}
    internal partial struct unifier {}
    internal partial struct unifyMode {}
    internal partial struct valueMap {}
    internal partial struct valueType {}
    internal partial struct varDecl {}
    public partial interface Importer {}
    public partial interface ImporterFrom {}
    public partial interface Object {}
    public partial interface Sizes {}
    public partial interface Type {}
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
    public partial struct Error {}
    public partial struct Func {}
    public partial struct ImportMode {}
    public partial struct Info {}
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
    public partial struct Scope {}
    public partial struct Selection {}
    public partial struct SelectionKind {}
    public partial struct Signature {}
    public partial struct Slice {}
    public partial struct StdSizes {}
    public partial struct Struct {}
    public partial struct Term {}
    public partial struct Tuple {}
    public partial struct TypeAndValue {}
    public partial struct TypeList {}
    public partial struct TypeName {}
    public partial struct TypeParam {}
    public partial struct TypeParamList {}
    public partial struct Union {}
    public partial struct Var {}
    public partial struct VarKind {}
    public partial struct term {}
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

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(types_package.Alias)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛAlias;
[GoMethodHost(typeof(types_package.ArgumentError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛArgumentError;
[GoMethodHost(typeof(types_package.Array)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛArray;
[GoMethodHost(typeof(types_package.Basic)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛBasic;
[GoMethodHost(typeof(types_package.Builtin)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛBuiltin;
[GoMethodHost(typeof(types_package.Chan)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛChan;
[GoMethodHost(typeof(types_package.Checker)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛChecker;
[GoMethodHost(typeof(types_package.Config)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛConfig;
[GoMethodHost(typeof(types_package.Const)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛConst;
[GoMethodHost(typeof(types_package.Context)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛContext;
[GoMethodHost(typeof(types_package.Error)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛError;
[GoMethodHost(typeof(types_package.Func)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛFunc;
[GoMethodHost(typeof(types_package.Info)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛInfo;
[GoMethodHost(typeof(types_package.Initializer)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛInitializer;
[GoMethodHost(typeof(types_package.Interface)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛInterface;
[GoMethodHost(typeof(types_package.Label)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛLabel;
[GoMethodHost(typeof(types_package.Map)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛMap;
[GoMethodHost(typeof(types_package.MethodSet)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛMethodSet;
[GoMethodHost(typeof(types_package.Named)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛNamed;
[GoMethodHost(typeof(types_package.Nil)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛNil;
[GoMethodHost(typeof(types_package.Package)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛPackage;
[GoMethodHost(typeof(types_package.PkgName)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛPkgName;
[GoMethodHost(typeof(types_package.Pointer)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛPointer;
[GoMethodHost(typeof(types_package.Scope)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛScope;
[GoMethodHost(typeof(types_package.Selection)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛSelection;
[GoMethodHost(typeof(types_package.Signature)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛSignature;
[GoMethodHost(typeof(types_package.Slice)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛSlice;
[GoMethodHost(typeof(types_package.StdSizes)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛStdSizes;
[GoMethodHost(typeof(types_package.Struct)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛStruct;
[GoMethodHost(typeof(types_package.Term)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛTerm;
[GoMethodHost(typeof(types_package.Tuple)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛTuple;
[GoMethodHost(typeof(types_package.TypeAndValue)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛTypeAndValue;
[GoMethodHost(typeof(types_package.TypeList)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛTypeList;
[GoMethodHost(typeof(types_package.TypeName)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛTypeName;
[GoMethodHost(typeof(types_package.TypeParam)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛTypeParam;
[GoMethodHost(typeof(types_package.TypeParamList)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛTypeParamList;
[GoMethodHost(typeof(types_package.Union)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛUnion;
[GoMethodHost(typeof(types_package.Var)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛVar;
[GoMethodHost(typeof(types_package.VarKind)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛVarKind;
[GoMethodHost(typeof(types_package._TypeSet)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛ_TypeSet;
[GoMethodHost(typeof(types_package.action)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛaction;
[GoMethodHost(typeof(types_package.atPos)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛatPos;
[GoMethodHost(typeof(types_package.block)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛblock;
[GoMethodHost(typeof(types_package.comparer)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛcomparer;
[GoMethodHost(typeof(types_package.constDecl)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛconstDecl;
[GoMethodHost(typeof(types_package.cycleFinder)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛcycleFinder;
[GoMethodHost(typeof(types_package.declInfo)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛdeclInfo;
[GoMethodHost(typeof(types_package.environment)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛenvironment;
[GoMethodHost(typeof(types_package.error_)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛerror_;
[GoMethodHost(typeof(types_package.funcDecl)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛfuncDecl;
[GoMethodHost(typeof(types_package.gcSizes)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛgcSizes;
[GoMethodHost(typeof(types_package.goVersion)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛgoVersion;
[GoMethodHost(typeof(types_package.graphNode)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛgraphNode;
[GoMethodHost(typeof(types_package.ifacePair)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛifacePair;
[GoMethodHost(typeof(types_package.importDecl)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛimportDecl;
[GoMethodHost(typeof(types_package.indexedExpr)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛindexedExpr;
[GoMethodHost(typeof(types_package.instanceLookup)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛinstanceLookup;
[GoMethodHost(typeof(types_package.lazyObject)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛlazyObject;
[GoMethodHost(typeof(types_package.methodSet)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛmethodSet;
[GoMethodHost(typeof(types_package.monoGraph)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛmonoGraph;
[GoMethodHost(typeof(types_package.nodeQueue)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛnodeQueue;
[GoMethodHost(typeof(types_package.nodeSet)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛnodeSet;
[GoMethodHost(typeof(types_package.@object)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛobject;
[GoMethodHost(typeof(types_package.objset)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛobjset;
[GoMethodHost(typeof(types_package.operand)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛoperand;
[GoMethodHost(typeof(types_package.posSpan)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛposSpan;
[GoMethodHost(typeof(types_package.substMap)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛsubstMap;
[GoMethodHost(typeof(types_package.subster)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛsubster;
[GoMethodHost(typeof(types_package.term)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛterm;
[GoMethodHost(typeof(types_package.termlist)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛtermlist;
[GoMethodHost(typeof(types_package.tpWalker)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛtpWalker;
[GoMethodHost(typeof(types_package.typeDecl)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛtypeDecl;
[GoMethodHost(typeof(types_package.typeError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛtypeError;
[GoMethodHost(typeof(types_package.typeParamsById)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛtypeParamsById;
[GoMethodHost(typeof(types_package.typeWriter)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛtypeWriter;
[GoMethodHost(typeof(types_package.unifier)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛunifier;
[GoMethodHost(typeof(types_package.unifyMode)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛunifyMode;
[GoMethodHost(typeof(types_package.varDecl)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class typesᴛvarDecl;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class typesᴛAlias {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<TypeName> Obj(this ref Alias a) => types_package.Obj⁔(ref a);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ж<Alias> Ꮡa) => types_package.String⁔(Ꮡa);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Type Underlying(this ж<Alias> Ꮡa) => types_package.Underlying⁔(Ꮡa);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Alias> Origin(this ref Alias a) => types_package.Origin⁔(ref a);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<TypeParamList> TypeParams(this ref Alias a) => types_package.TypeParams⁔(ref a);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void SetTypeParams(this ref Alias a, slice<ж<TypeParam>> tparams) => types_package.SetTypeParams⁔(ref a, tparams);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<TypeList> TypeArgs(this ref Alias a) => types_package.TypeArgs⁔(ref a);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Type Rhs(this ref Alias a) => types_package.Rhs⁔(ref a);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void cleanup(this ж<Alias> Ꮡa) => types_package.cleanup⁔(Ꮡa);
}
partial class typesᴛArgumentError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Error(this ref ArgumentError e) => types_package.Error⁔(ref e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Unwrap(this ref ArgumentError e) => types_package.Unwrap⁔(ref e);
}
partial class typesᴛArray {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static int64 Len(this ref Array a) => types_package.Len⁔(ref a);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Type Elem(this ref Array a) => types_package.Elem⁔(ref a);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Type Underlying(this ж<Array> Ꮡa) => types_package.Underlying⁔(Ꮡa);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ж<Array> Ꮡa) => types_package.String⁔(Ꮡa);
}
partial class typesᴛBasic {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static BasicKind Kind(this ref Basic b) => types_package.Kind⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static BasicInfo Info(this ref Basic b) => types_package.Info⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Name(this ref Basic b) => types_package.Name⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Type Underlying(this ж<Basic> Ꮡb) => types_package.Underlying⁔(Ꮡb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ж<Basic> Ꮡb) => types_package.String⁔(Ꮡb);
}
partial class typesᴛBuiltin {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ж<Builtin> Ꮡobj) => types_package.String⁔(Ꮡobj);
}
partial class typesᴛChan {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ChanDir Dir(this ref Chan c) => types_package.Dir⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Type Elem(this ref Chan c) => types_package.Elem⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Type Underlying(this ж<Chan> Ꮡc) => types_package.Underlying⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ж<Chan> Ꮡc) => types_package.String⁔(Ꮡc);
}
partial class typesᴛChecker {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Alias> newAlias(this ж<Checker> Ꮡcheck, ж<TypeName> Ꮡobj, Type rhs) => types_package.newAlias⁔(Ꮡcheck, Ꮡobj, rhs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Alias> newAliasInstance(this ж<Checker> Ꮡcheck, token.Pos pos, ж<Alias> Ꮡorig, slice<Type> targs, ж<Named> Ꮡexpanding, ж<Context> Ꮡctxt) => types_package.newAliasInstance⁔(Ꮡcheck, pos, Ꮡorig, targs, Ꮡexpanding, Ꮡctxt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void assignment(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, Type T, @string context) => types_package.assignment⁔(Ꮡcheck, Ꮡx, T, context);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void initConst(this ж<Checker> Ꮡcheck, ж<Const> Ꮡlhs, ж<operand> Ꮡx) => types_package.initConst⁔(Ꮡcheck, Ꮡlhs, Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void initVar(this ж<Checker> Ꮡcheck, ж<Var> Ꮡlhs, ж<operand> Ꮡx, @string context) => types_package.initVar⁔(Ꮡcheck, Ꮡlhs, Ꮡx, context);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Type lhsVar(this ж<Checker> Ꮡcheck, ast.Expr lhs) => types_package.lhsVar⁔(Ꮡcheck, lhs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void assignVar(this ж<Checker> Ꮡcheck, ast.Expr lhs, ast.Expr rhs, ж<operand> Ꮡx, @string context) => types_package.assignVar⁔(Ꮡcheck, lhs, rhs, Ꮡx, context);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string typesSummary(this ж<Checker> Ꮡcheck, slice<Type> list, bool variadic, bool hasDots) => types_package.typesSummary⁔(Ꮡcheck, list, variadic, hasDots);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void assignError(this ж<Checker> Ꮡcheck, slice<ast.Expr> rhs, nint l, nint r) => types_package.assignError⁔(Ꮡcheck, rhs, l, r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void returnError(this ж<Checker> Ꮡcheck, positioner at, slice<ж<Var>> lhs, slice<ж<operand>> rhs) => types_package.returnError⁔(Ꮡcheck, at, lhs, rhs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void initVars(this ж<Checker> Ꮡcheck, slice<ж<Var>> lhs, slice<ast.Expr> orig_rhs, ast.Stmt returnStmt) => types_package.initVars⁔(Ꮡcheck, lhs, orig_rhs, returnStmt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void assignVars(this ж<Checker> Ꮡcheck, slice<ast.Expr> lhs, slice<ast.Expr> orig_rhs) => types_package.assignVars⁔(Ꮡcheck, lhs, orig_rhs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void shortVarDecl(this ж<Checker> Ꮡcheck, positioner pos, slice<ast.Expr> lhs, slice<ast.Expr> rhs) => types_package.shortVarDecl⁔(Ꮡcheck, pos, lhs, rhs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool /*_*/ Δbuiltin(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, ж<ast.CallExpr> Ꮡcall, builtinId id) => types_package.Δbuiltin⁔(Ꮡcheck, Ꮡx, Ꮡcall, id);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Type applyTypeFunc(this ж<Checker> Ꮡcheck, Func<Type, Type> f, ж<operand> Ꮡx, builtinId id) => types_package.applyTypeFunc⁔(Ꮡcheck, f, Ꮡx, id);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<Type> funcInst(this ж<Checker> Ꮡcheck, ж<target> ᏑT, token.Pos pos, ж<operand> Ꮡx, ж<indexedExpr> Ꮡix, bool infer) => types_package.funcInst⁔(Ꮡcheck, ᏑT, pos, Ꮡx, Ꮡix, infer);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Signature> /*res*/ instantiateSignature(this ж<Checker> Ꮡcheck, token.Pos pos, ast.Expr expr, ж<Signature> Ꮡtyp, slice<Type> targs, slice<ast.Expr> xlist) => types_package.instantiateSignature⁔(Ꮡcheck, pos, expr, Ꮡtyp, targs, xlist);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static exprKind callExpr(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, ж<ast.CallExpr> Ꮡcall) => types_package.callExpr⁔(Ꮡcheck, Ꮡx, Ꮡcall);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<ж<operand>> /*xlist*/ exprList(this ж<Checker> Ꮡcheck, slice<ast.Expr> elist) => types_package.exprList⁔(Ꮡcheck, elist);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<ж<operand>> resList, slice<slice<Type>> targsList) genericExprList(this ж<Checker> Ꮡcheck, slice<ast.Expr> elist) => types_package.genericExprList⁔(Ꮡcheck, elist);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Signature> /*rsig*/ arguments(this ж<Checker> Ꮡcheck, ж<ast.CallExpr> Ꮡcall, ж<Signature> Ꮡsig, slice<Type> targs, slice<ast.Expr> xlist, slice<ж<operand>> args, slice<slice<Type>> atargs) => types_package.arguments⁔(Ꮡcheck, Ꮡcall, Ꮡsig, targs, xlist, args, atargs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void selector(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, ж<ast.SelectorExpr> Ꮡe, bool wantType) => types_package.selector⁔(Ꮡcheck, Ꮡx, Ꮡe, wantType);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool use(this ж<Checker> Ꮡcheck, params ꓸꓸꓸastꓸExpr argsʗp) => types_package.use⁔(Ꮡcheck, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool useLHS(this ж<Checker> Ꮡcheck, params ꓸꓸꓸastꓸExpr argsʗp) => types_package.useLHS⁔(Ꮡcheck, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool useN(this ж<Checker> Ꮡcheck, slice<ast.Expr> args, bool lhs) => types_package.useN⁔(Ꮡcheck, args, lhs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool use1(this ж<Checker> Ꮡcheck, ast.Expr e, bool lhs) => types_package.use1⁔(Ꮡcheck, e, lhs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void addDeclDep(this ref Checker check, Object to) => types_package.addDeclDep⁔(ref check, to);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void brokenAlias(this ref Checker check, ж<TypeName> Ꮡalias) => types_package.brokenAlias⁔(ref check, Ꮡalias);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void validAlias(this ref Checker check, ж<TypeName> Ꮡalias, Type typ) => types_package.validAlias⁔(ref check, Ꮡalias, typ);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool isBrokenAlias(this ref Checker check, ж<TypeName> Ꮡalias) => types_package.isBrokenAlias⁔(ref check, Ꮡalias);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void rememberUntyped(this ref Checker check, ast.Expr e, bool lhs, operandMode mode, ж<Basic> Ꮡtyp, constant.Value val) => types_package.rememberUntyped⁔(ref check, e, lhs, mode, Ꮡtyp, val);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<action> later(this ref Checker check, Action f) => types_package.later⁔(ref check, f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void push(this ref Checker check, Object obj) => types_package.push⁔(ref check, obj);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void pop(this ref Checker check) => types_package.pop⁔(ref check);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void needsCleanup(this ref Checker check, cleaner c) => types_package.needsCleanup⁔(ref check, c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void initFiles(this ж<Checker> Ꮡcheck, slice<ж<ast.File>> files) => types_package.initFiles⁔(Ꮡcheck, files);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void pushPos(this ref Checker check, positioner pos) => types_package.pushPos⁔(ref check, pos);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void popPos(this ref Checker check) => types_package.popPos⁔(ref check);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void handleBailout(this ж<Checker> Ꮡcheck, ж<error> Ꮡerr) => types_package.handleBailout⁔(Ꮡcheck, Ꮡerr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error /*err*/ Files(this ж<Checker> Ꮡcheck, slice<ж<ast.File>> files) => types_package.Files⁔(Ꮡcheck, files);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void checkFiles(this ж<Checker> Ꮡcheck, slice<ж<ast.File>> files) => types_package.checkFiles⁔(Ꮡcheck, files);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void processDelayed(this ж<Checker> Ꮡcheck, nint top) => types_package.processDelayed⁔(Ꮡcheck, top);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void cleanup(this ref Checker check) => types_package.cleanup⁔(ref check);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void recordTypeAndValueInSyntax(this ref Checker check, ast.Expr x, operandMode mode, Type typ, constant.Value val) => types_package.recordTypeAndValueInSyntax⁔(ref check, x, mode, typ, val);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void recordCommaOkTypesInSyntax(this ref Checker check, ast.Expr x, Type t0, Type t1) => types_package.recordCommaOkTypesInSyntax⁔(ref check, x, t0, t1);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void overflow(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, token.Pos opPos) => types_package.overflow⁔(Ꮡcheck, Ꮡx, opPos);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void representable(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, ж<Basic> Ꮡtyp) => types_package.representable⁔(Ꮡcheck, Ꮡx, Ꮡtyp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (constant.Value, errors.Code) representation(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, ж<Basic> Ꮡtyp) => types_package.representation⁔(Ꮡcheck, Ꮡx, Ꮡtyp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void invalidConversion(this ж<Checker> Ꮡcheck, errors.Code code, ж<operand> Ꮡx, Type target) => types_package.invalidConversion⁔(Ꮡcheck, code, Ꮡx, target);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void convertUntyped(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, Type target) => types_package.convertUntyped⁔(Ꮡcheck, Ꮡx, target);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void conversion(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, Type T) => types_package.conversion⁔(Ꮡcheck, Ꮡx, T);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void directCycles(this ж<Checker> Ꮡcheck) => types_package.directCycles⁔(Ꮡcheck);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void directCycle(this ж<Checker> Ꮡcheck, ж<TypeName> Ꮡtname, map<ж<TypeName>, nint> pathIdx) => types_package.directCycle⁔(Ꮡcheck, Ꮡtname, pathIdx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool finiteSize(this ж<Checker> Ꮡcheck, Type t) => types_package.finiteSize⁔(Ꮡcheck, t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void declare(this ж<Checker> Ꮡcheck, ж<Scope> Ꮡscope, ж<ast.Ident> Ꮡid, Object obj, token.Pos pos) => types_package.declare⁔(Ꮡcheck, Ꮡscope, Ꮡid, obj, pos);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void objDecl(this ж<Checker> Ꮡcheck, Object obj) => types_package.objDecl⁔(Ꮡcheck, obj);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool /*valid*/ validCycle(this ж<Checker> Ꮡcheck, Object obj) => types_package.validCycle⁔(Ꮡcheck, obj);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void cycleError(this ж<Checker> Ꮡcheck, slice<Object> cycle, nint start) => types_package.cycleError⁔(Ꮡcheck, cycle, start);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void walkDecls(this ж<Checker> Ꮡcheck, slice<ast.Decl> decls, Action<decl> f) => types_package.walkDecls⁔(Ꮡcheck, decls, f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void walkDecl(this ж<Checker> Ꮡcheck, ast.Decl d, Action<decl> f) => types_package.walkDecl⁔(Ꮡcheck, d, f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void constDecl(this ж<Checker> Ꮡcheck, ж<Const> Ꮡobj, ast.Expr typ, ast.Expr init, bool inherited) => types_package.constDecl⁔(Ꮡcheck, Ꮡobj, typ, init, inherited);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void varDecl(this ж<Checker> Ꮡcheck, ж<Var> Ꮡobj, slice<ж<Var>> lhs, ast.Expr typ, ast.Expr init) => types_package.varDecl⁔(Ꮡcheck, Ꮡobj, lhs, typ, init);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool isImportedConstraint(this ref Checker check, Type typ) => types_package.isImportedConstraint⁔(ref check, typ);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void typeDecl(this ж<Checker> Ꮡcheck, ж<TypeName> Ꮡobj, ж<ast.TypeSpec> Ꮡtdecl) => types_package.typeDecl⁔(Ꮡcheck, Ꮡobj, Ꮡtdecl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void collectTypeParams(this ж<Checker> Ꮡcheck, ж<ж<TypeParamList>> Ꮡdst, ж<ast.FieldList> Ꮡlist) => types_package.collectTypeParams⁔(Ꮡcheck, Ꮡdst, Ꮡlist);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Type bound(this ж<Checker> Ꮡcheck, ast.Expr x) => types_package.bound⁔(Ꮡcheck, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<TypeParam> declareTypeParam(this ж<Checker> Ꮡcheck, ж<ast.Ident> Ꮡname, token.Pos scopePos) => types_package.declareTypeParam⁔(Ꮡcheck, Ꮡname, scopePos);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void collectMethods(this ж<Checker> Ꮡcheck, ж<TypeName> Ꮡobj) => types_package.collectMethods⁔(Ꮡcheck, Ꮡobj);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void checkFieldUniqueness(this ж<Checker> Ꮡcheck, ж<Named> Ꮡbase) => types_package.checkFieldUniqueness⁔(Ꮡcheck, Ꮡbase);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void funcDecl(this ж<Checker> Ꮡcheck, ж<Func> Ꮡobj, ж<declInfo> Ꮡdecl) => types_package.funcDecl⁔(Ꮡcheck, Ꮡobj, Ꮡdecl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void declStmt(this ж<Checker> Ꮡcheck, ast.Decl d) => types_package.declStmt⁔(Ꮡcheck, d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<error_> newError(this ж<Checker> Ꮡcheck, errors.Code code) => types_package.newError⁔(Ꮡcheck, code);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void handleError(this ref Checker check, nint index, positioner posn, errors.Code code, @string msg, bool soft) => types_package.handleError⁔(ref check, index, posn, code, msg, soft);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void error(this ж<Checker> Ꮡcheck, positioner at, errors.Code code, @string msg) => types_package.error⁔(Ꮡcheck, at, code, msg);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void errorf(this ж<Checker> Ꮡcheck, positioner at, errors.Code code, @string format, params ꓸꓸꓸany argsʗp) => types_package.errorf⁔(Ꮡcheck, at, code, format, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void softErrorf(this ж<Checker> Ꮡcheck, positioner at, errors.Code code, @string format, params ꓸꓸꓸany argsʗp) => types_package.softErrorf⁔(Ꮡcheck, at, code, format, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void versionErrorf(this ж<Checker> Ꮡcheck, positioner at, goVersion v, @string format, params ꓸꓸꓸany argsʗp) => types_package.versionErrorf⁔(Ꮡcheck, at, v, format, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string lookupError(this ж<Checker> Ꮡcheck, Type typ, @string sel, Object obj, bool structLit) => types_package.lookupError⁔(Ꮡcheck, typ, sel, obj, structLit);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool op(this ж<Checker> Ꮡcheck, opPredicates m, ж<operand> Ꮡx, token.Token op) => types_package.op⁔(Ꮡcheck, m, Ꮡx, op);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void unary(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, ж<ast.UnaryExpr> Ꮡe) => types_package.unary⁔(Ꮡcheck, Ꮡx, Ꮡe);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Type chanElem(this ж<Checker> Ꮡcheck, positioner pos, ж<operand> Ꮡx, bool recv) => types_package.chanElem⁔(Ꮡcheck, pos, Ꮡx, recv);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void updateExprType(this ж<Checker> Ꮡcheck, ast.Expr x, Type typ, bool final) => types_package.updateExprType⁔(Ꮡcheck, x, typ, final);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void updateExprVal(this ref Checker check, ast.Expr x, constant.Value val) => types_package.updateExprVal⁔(ref check, x, val);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (Type, constant.Value, errors.Code) implicitTypeAndValue(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, Type target) => types_package.implicitTypeAndValue⁔(Ꮡcheck, Ꮡx, target);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void comparison(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, ж<operand> Ꮡy, token.Token op, bool switchCase) => types_package.comparison⁔(Ꮡcheck, Ꮡx, Ꮡy, op, switchCase);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string incomparableCause(this ж<Checker> Ꮡcheck, Type typ) => types_package.incomparableCause⁔(Ꮡcheck, typ);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void shift(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, ж<operand> Ꮡy, ast.Expr e, token.Token op) => types_package.shift⁔(Ꮡcheck, Ꮡx, Ꮡy, e, op);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void binary(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, ast.Expr e, ast.Expr lhs, ast.Expr rhs, token.Token op, token.Pos opPos) => types_package.binary⁔(Ꮡcheck, Ꮡx, e, lhs, rhs, op, opPos);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void matchTypes(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, ж<operand> Ꮡy) => types_package.matchTypes⁔(Ꮡcheck, Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static exprKind rawExpr(this ж<Checker> Ꮡcheck, ж<target> ᏑT, ж<operand> Ꮡx, ast.Expr e, Type hint, bool allowGeneric) => types_package.rawExpr⁔(Ꮡcheck, ᏑT, Ꮡx, e, hint, allowGeneric);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void nonGeneric(this ж<Checker> Ꮡcheck, ж<target> ᏑT, ж<operand> Ꮡx) => types_package.nonGeneric⁔(Ꮡcheck, ᏑT, Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void pendingType(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx) => types_package.pendingType⁔(Ꮡcheck, Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static exprKind exprInternal(this ж<Checker> Ꮡcheck, ж<target> ᏑT, ж<operand> Ꮡx, ast.Expr e, Type hint) => types_package.exprInternal⁔(Ꮡcheck, ᏑT, Ꮡx, e, hint);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void typeAssertion(this ж<Checker> Ꮡcheck, ast.Expr e, ж<operand> Ꮡx, Type T, bool typeSwitch) => types_package.typeAssertion⁔(Ꮡcheck, e, Ꮡx, T, typeSwitch);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void expr(this ж<Checker> Ꮡcheck, ж<target> ᏑT, ж<operand> Ꮡx, ast.Expr e) => types_package.expr⁔(Ꮡcheck, ᏑT, Ꮡx, e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void genericExpr(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, ast.Expr e) => types_package.genericExpr⁔(Ꮡcheck, Ꮡx, e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<ж<operand>> list, bool commaOk) multiExpr(this ж<Checker> Ꮡcheck, ast.Expr e, bool allowCommaOk) => types_package.multiExpr⁔(Ꮡcheck, e, allowCommaOk);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void exprWithHint(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, ast.Expr e, Type hint) => types_package.exprWithHint⁔(Ꮡcheck, Ꮡx, e, hint);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void exprOrType(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, ast.Expr e, bool allowGeneric) => types_package.exprOrType⁔(Ꮡcheck, Ꮡx, e, allowGeneric);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void exclude(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, nuint modeset) => types_package.exclude⁔(Ꮡcheck, Ꮡx, modeset);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void singleValue(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx) => types_package.singleValue⁔(Ꮡcheck, Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string sprintf(this ж<Checker> Ꮡcheck, @string format, params ꓸꓸꓸany argsʗp) => types_package.sprintf⁔(Ꮡcheck, format, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void trace(this ж<Checker> Ꮡcheck, token.Pos pos, @string format, params ꓸꓸꓸany argsʗp) => types_package.trace⁔(Ꮡcheck, pos, format, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void dump(this ж<Checker> Ꮡcheck, @string format, params ꓸꓸꓸany argsʗp) => types_package.dump⁔(Ꮡcheck, format, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string qualifier(this ref Checker check, ж<Package> Ꮡpkg) => types_package.qualifier⁔(ref check, Ꮡpkg);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void markImports(this ref Checker check, ж<Package> Ꮡpkg) => types_package.markImports⁔(ref check, Ꮡpkg);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool /*isFuncInst*/ indexExpr(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, ж<indexedExpr> Ꮡe) => types_package.indexExpr⁔(Ꮡcheck, Ꮡx, Ꮡe);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void sliceExpr(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, ж<ast.SliceExpr> Ꮡe) => types_package.sliceExpr⁔(Ꮡcheck, Ꮡx, Ꮡe);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ast.Expr singleIndex(this ж<Checker> Ꮡcheck, ж<indexedExpr> Ꮡexpr) => types_package.singleIndex⁔(Ꮡcheck, Ꮡexpr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (Type typ, int64 val) index(this ж<Checker> Ꮡcheck, ast.Expr index, int64 max) => types_package.index⁔(Ꮡcheck, index, max);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool isValidIndex(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, errors.Code code, @string what, bool allowNegative) => types_package.isValidIndex⁔(Ꮡcheck, Ꮡx, code, what, allowNegative);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<Type> /*inferred*/ infer(this ж<Checker> Ꮡcheck, positioner posn, slice<ж<TypeParam>> tparams, slice<Type> targs, ж<Tuple> Ꮡparams, slice<ж<operand>> args, bool reverse, ж<error_> Ꮡerr) => types_package.infer⁔(Ꮡcheck, posn, tparams, targs, Ꮡparams, args, reverse, Ꮡerr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<ж<TypeParam>>, Type) renameTParams(this ж<Checker> Ꮡcheck, token.Pos pos, slice<ж<TypeParam>> tparams, Type typ) => types_package.renameTParams⁔(Ꮡcheck, pos, tparams, typ);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void initOrder(this ж<Checker> Ꮡcheck) => types_package.initOrder⁔(Ꮡcheck);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void reportCycle(this ж<Checker> Ꮡcheck, slice<Object> cycle) => types_package.reportCycle⁔(Ꮡcheck, cycle);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Type /*res*/ instance(this ж<Checker> Ꮡcheck, token.Pos pos, genericType orig, slice<Type> targs, ж<Named> Ꮡexpanding, ж<Context> Ꮡctxt) => types_package.instance⁔(Ꮡcheck, pos, orig, targs, Ꮡexpanding, Ꮡctxt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool validateTArgLen(this ж<Checker> Ꮡcheck, token.Pos pos, @string name, nint want, nint got) => types_package.validateTArgLen⁔(Ꮡcheck, pos, name, want, got);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) verify(this ж<Checker> Ꮡcheck, token.Pos pos, slice<ж<TypeParam>> tparams, slice<Type> targs, ж<Context> Ꮡctxt) => types_package.verify⁔(Ꮡcheck, pos, tparams, targs, Ꮡctxt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool implements(this ж<Checker> Ꮡcheck, Type V, Type T, bool constraint, ж<@string> Ꮡcause) => types_package.implements⁔(Ꮡcheck, V, T, constraint, Ꮡcause);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Interface> newInterface(this ж<Checker> Ꮡcheck) => types_package.newInterface⁔(Ꮡcheck);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void interfaceType(this ж<Checker> Ꮡcheck, ж<Interface> Ꮡityp, ж<ast.InterfaceType> Ꮡiface, ж<TypeName> Ꮡdef) => types_package.interfaceType⁔(Ꮡcheck, Ꮡityp, Ꮡiface, Ꮡdef);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void labels(this ж<Checker> Ꮡcheck, ж<ast.BlockStmt> Ꮡbody) => types_package.labels⁔(Ꮡcheck, Ꮡbody);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<ж<ast.BranchStmt>> blockBranches(this ж<Checker> Ꮡcheck, ж<Scope> Ꮡall, ж<block> Ꮡparent, ж<ast.LabeledStmt> Ꮡlstmt, slice<ast.Stmt> list) => types_package.blockBranches⁔(Ꮡcheck, Ꮡall, Ꮡparent, Ꮡlstmt, list);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void langCompat(this ж<Checker> Ꮡcheck, ж<ast.BasicLit> Ꮡlit) => types_package.langCompat⁔(Ꮡcheck, Ꮡlit);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void basicLit(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, ж<ast.BasicLit> Ꮡe) => types_package.basicLit⁔(Ꮡcheck, Ꮡx, Ꮡe);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void funcLit(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, ж<ast.FuncLit> Ꮡe) => types_package.funcLit⁔(Ꮡcheck, Ꮡx, Ꮡe);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void compositeLit(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, ж<ast.CompositeLit> Ꮡe, Type hint) => types_package.compositeLit⁔(Ꮡcheck, Ꮡx, Ꮡe, hint);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int64 indexedElts(this ж<Checker> Ꮡcheck, slice<ast.Expr> elts, Type typ, int64 length) => types_package.indexedElts⁔(Ꮡcheck, elts, typ, length);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<Func> method, bool wrongType) missingMethod(this ж<Checker> Ꮡcheck, Type V, Type T, bool @static, Func<Type, Type, bool> equivalent, ж<@string> Ꮡcause) => types_package.missingMethod⁔(Ꮡcheck, V, T, @static, equivalent, Ꮡcause);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool hasAllMethods(this ж<Checker> Ꮡcheck, Type V, Type T, bool @static, Func<Type, Type, bool> equivalent, ж<@string> Ꮡcause) => types_package.hasAllMethods⁔(Ꮡcheck, V, T, @static, equivalent, Ꮡcause);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string interfacePtrError(this ж<Checker> Ꮡcheck, Type T) => types_package.interfacePtrError⁔(Ꮡcheck, T);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string funcString(this ж<Checker> Ꮡcheck, ж<Func> Ꮡf, bool pkgInfo) => types_package.funcString⁔(Ꮡcheck, Ꮡf, pkgInfo);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool assertableTo(this ж<Checker> Ꮡcheck, Type V, Type T, ж<@string> Ꮡcause) => types_package.assertableTo⁔(Ꮡcheck, V, T, Ꮡcause);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool newAssertableTo(this ж<Checker> Ꮡcheck, Type V, Type T, ж<@string> Ꮡcause) => types_package.newAssertableTo⁔(Ꮡcheck, V, T, Ꮡcause);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void monomorph(this ж<Checker> Ꮡcheck) => types_package.monomorph⁔(Ꮡcheck);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void reportInstanceLoop(this ж<Checker> Ꮡcheck, nint v) => types_package.reportInstanceLoop⁔(Ꮡcheck, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Named> newNamed(this ж<Checker> Ꮡcheck, ж<TypeName> Ꮡobj, Type fromRHS, slice<ж<Func>> methods) => types_package.newNamed⁔(Ꮡcheck, Ꮡobj, fromRHS, methods);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Named> newNamedInstance(this ж<Checker> Ꮡcheck, token.Pos pos, ж<Named> Ꮡorig, slice<Type> targs, ж<Named> Ꮡexpanding) => types_package.newNamedInstance⁔(Ꮡcheck, pos, Ꮡorig, targs, Ꮡexpanding);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Context> context(this ref Checker check) => types_package.context⁔(ref check);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void rangeStmt(this ж<Checker> Ꮡcheck, stmtContext inner, ж<ast.RangeStmt> ᏑrangeStmt, positioner noNewVarPos, ast.Expr sKey, ast.Expr sValue, ast.Expr sExtra, ast.Expr rangeVar, bool isDef) => types_package.rangeStmt⁔(Ꮡcheck, inner, ᏑrangeStmt, noNewVarPos, sKey, sValue, sExtra, rangeVar, isDef);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void record(this ref Checker check, ж<operand> Ꮡx) => types_package.record⁔(ref check, Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void recordUntyped(this ж<Checker> Ꮡcheck) => types_package.recordUntyped⁔(Ꮡcheck);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void recordTypeAndValue(this ref Checker check, ast.Expr x, operandMode mode, Type typ, constant.Value val) => types_package.recordTypeAndValue⁔(ref check, x, mode, typ, val);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void recordBuiltinType(this ref Checker check, ast.Expr f, ж<Signature> Ꮡsig) => types_package.recordBuiltinType⁔(ref check, f, Ꮡsig);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void recordCommaOkTypes(this ref Checker check, ast.Expr x, slice<ж<operand>> a) => types_package.recordCommaOkTypes⁔(ref check, x, a);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void recordInstance(this ref Checker check, ast.Expr expr, slice<Type> targs, Type typ) => types_package.recordInstance⁔(ref check, expr, targs, typ);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void recordDef(this ref Checker check, ж<ast.Ident> Ꮡid, Object obj) => types_package.recordDef⁔(ref check, Ꮡid, obj);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void recordUse(this ref Checker check, ж<ast.Ident> Ꮡid, Object obj) => types_package.recordUse⁔(ref check, Ꮡid, obj);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void recordImplicit(this ref Checker check, ast.Node node, Object obj) => types_package.recordImplicit⁔(ref check, node, obj);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void recordSelection(this ref Checker check, ж<ast.SelectorExpr> Ꮡx, SelectionKind kind, Type recv, Object obj, slice<nint> index, bool indirect) => types_package.recordSelection⁔(ref check, Ꮡx, kind, recv, obj, index, indirect);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void recordScope(this ref Checker check, ast.Node node, ж<Scope> Ꮡscope) => types_package.recordScope⁔(ref check, node, Ꮡscope);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void arityMatch(this ж<Checker> Ꮡcheck, ж<ast.ValueSpec> Ꮡs, ж<ast.ValueSpec> Ꮡinit) => types_package.arityMatch⁔(Ꮡcheck, Ꮡs, Ꮡinit);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void declarePkgObj(this ж<Checker> Ꮡcheck, ж<ast.Ident> Ꮡident, Object obj, ж<declInfo> Ꮡd) => types_package.declarePkgObj⁔(Ꮡcheck, Ꮡident, obj, Ꮡd);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string filename(this ref Checker check, nint fileNo) => types_package.filename⁔(ref check, fileNo);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Package> importPackage(this ж<Checker> Ꮡcheck, positioner at, @string path, @string dir) => types_package.importPackage⁔(Ꮡcheck, at, path, dir);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void collectObjects(this ж<Checker> Ꮡcheck) => types_package.collectObjects⁔(Ꮡcheck);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void sortObjects(this ref Checker check) => types_package.sortObjects⁔(ref check);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (bool ptr, ast.Expr @base, slice<ж<ast.Ident>> tparams) unpackRecv(this ж<Checker> Ꮡcheck, ast.Expr rtyp, bool unpackParams) => types_package.unpackRecv⁔(Ꮡcheck, rtyp, unpackParams);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (bool ptr_, ж<TypeName> @base) resolveBaseTypeName(this ref Checker check, bool ptr, ж<ast.Ident> Ꮡname) => types_package.resolveBaseTypeName⁔(ref check, ptr, Ꮡname);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void packageObjects(this ж<Checker> Ꮡcheck) => types_package.packageObjects⁔(Ꮡcheck);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void unusedImports(this ж<Checker> Ꮡcheck) => types_package.unusedImports⁔(Ꮡcheck);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void errorUnusedPkg(this ж<Checker> Ꮡcheck, ж<PkgName> Ꮡobj) => types_package.errorUnusedPkg⁔(Ꮡcheck, Ꮡobj);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool isTerminating(this ref Checker check, ast.Stmt s, @string label) => types_package.isTerminating⁔(ref check, s, label);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool isTerminatingList(this ref Checker check, slice<ast.Stmt> list, @string label) => types_package.isTerminatingList⁔(ref check, list, label);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool isTerminatingSwitch(this ref Checker check, ж<ast.BlockStmt> Ꮡbody, @string label) => types_package.isTerminatingSwitch⁔(ref check, Ꮡbody, label);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void funcType(this ж<Checker> Ꮡcheck, ж<Signature> Ꮡsig, ж<ast.FieldList> ᏑrecvPar, ж<ast.FuncType> Ꮡftyp) => types_package.funcType⁔(Ꮡcheck, Ꮡsig, ᏑrecvPar, Ꮡftyp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<Var>, ж<TypeParamList>) collectRecv(this ж<Checker> Ꮡcheck, ж<ast.Field> Ꮡrparam, token.Pos scopePos) => types_package.collectRecv⁔(Ꮡcheck, Ꮡrparam, scopePos);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void recordParenthesizedRecvTypes(this ref Checker check, ast.Expr expr, Type typ) => types_package.recordParenthesizedRecvTypes⁔(ref check, expr, typ);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<ж<ast.Ident>> names, slice<ж<Var>> @params, bool variadic) collectParams(this ж<Checker> Ꮡcheck, VarKind kind, ж<ast.FieldList> Ꮡlist) => types_package.collectParams⁔(Ꮡcheck, kind, Ꮡlist);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void declareParams(this ж<Checker> Ꮡcheck, slice<ж<ast.Ident>> names, slice<ж<Var>> @params, token.Pos scopePos) => types_package.declareParams⁔(Ꮡcheck, names, @params, scopePos);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void validRecv(this ж<Checker> Ꮡcheck, positioner pos, ж<Var> Ꮡrecv) => types_package.validRecv⁔(Ꮡcheck, pos, Ꮡrecv);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void funcBody(this ж<Checker> Ꮡcheck, ж<declInfo> Ꮡdecl, @string name, ж<Signature> Ꮡsig, ж<ast.BlockStmt> Ꮡbody, constant.Value iota) => types_package.funcBody⁔(Ꮡcheck, Ꮡdecl, name, Ꮡsig, Ꮡbody, iota);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void usage(this ж<Checker> Ꮡcheck, ж<Scope> Ꮡscope) => types_package.usage⁔(Ꮡcheck, Ꮡscope);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void simpleStmt(this ж<Checker> Ꮡcheck, ast.Stmt s) => types_package.simpleStmt⁔(Ꮡcheck, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void stmtList(this ж<Checker> Ꮡcheck, stmtContext ctxt, slice<ast.Stmt> list) => types_package.stmtList⁔(Ꮡcheck, ctxt, list);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void multipleDefaults(this ж<Checker> Ꮡcheck, slice<ast.Stmt> list) => types_package.multipleDefaults⁔(Ꮡcheck, list);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void openScope(this ref Checker check, ast.Node node, @string comment) => types_package.openScope⁔(ref check, node, comment);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void closeScope(this ref Checker check) => types_package.closeScope⁔(ref check);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void suspendedCall(this ж<Checker> Ꮡcheck, @string keyword, ж<ast.CallExpr> Ꮡcall) => types_package.suspendedCall⁔(Ꮡcheck, keyword, Ꮡcall);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void caseValues(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, slice<ast.Expr> values, valueMap seen) => types_package.caseValues⁔(Ꮡcheck, Ꮡx, values, seen);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool isNil(this ref Checker check, ast.Expr e) => types_package.isNil⁔(ref check, e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Type caseTypes(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, slice<ast.Expr> types, map<Type, ast.Expr> seen) => types_package.caseTypes⁔(Ꮡcheck, Ꮡx, types, seen);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Type caseTypes_currently_unused(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, ж<Interface> Ꮡxtyp, slice<ast.Expr> types, map<@string, ast.Expr> seen) => types_package.caseTypes_currently_unused⁔(Ꮡcheck, Ꮡx, Ꮡxtyp, types, seen);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void stmt(this ж<Checker> Ꮡcheck, stmtContext ctxt, ast.Stmt s) => types_package.stmt⁔(Ꮡcheck, ctxt, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void structType(this ж<Checker> Ꮡcheck, ж<Struct> Ꮡstyp, ж<ast.StructType> Ꮡe) => types_package.structType⁔(Ꮡcheck, Ꮡstyp, Ꮡe);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool declareInSet(this ж<Checker> Ꮡcheck, ж<objset> Ꮡoset, token.Pos pos, Object obj) => types_package.declareInSet⁔(Ꮡcheck, Ꮡoset, pos, obj);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string tag(this ж<Checker> Ꮡcheck, ж<ast.BasicLit> Ꮡt) => types_package.tag⁔(Ꮡcheck, Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Type subst(this ж<Checker> Ꮡcheck, token.Pos pos, Type typ, substMap smap, ж<Named> Ꮡexpanding, ж<Context> Ꮡctxt) => types_package.subst⁔(Ꮡcheck, pos, typ, smap, Ꮡexpanding, Ꮡctxt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<TypeParam> newTypeParam(this ж<Checker> Ꮡcheck, ж<TypeName> Ꮡobj, Type constraint) => types_package.newTypeParam⁔(Ꮡcheck, Ꮡobj, constraint);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void ident(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, ж<ast.Ident> Ꮡe, bool wantType) => types_package.ident⁔(Ꮡcheck, Ꮡx, Ꮡe, wantType);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Type typ(this ж<Checker> Ꮡcheck, ast.Expr e) => types_package.typ⁔(Ꮡcheck, e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Type varType(this ж<Checker> Ꮡcheck, ast.Expr e) => types_package.varType⁔(Ꮡcheck, e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void validVarType(this ж<Checker> Ꮡcheck, ast.Expr e, Type typ) => types_package.validVarType⁔(Ꮡcheck, e, typ);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Type declaredType(this ж<Checker> Ꮡcheck, ast.Expr e, ж<TypeName> Ꮡdef) => types_package.declaredType⁔(Ꮡcheck, e, Ꮡdef);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Type genericType(this ж<Checker> Ꮡcheck, ast.Expr e, ж<@string> Ꮡcause) => types_package.genericType⁔(Ꮡcheck, e, Ꮡcause);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Type /*T*/ typInternal(this ж<Checker> Ꮡcheck, ast.Expr e0, ж<TypeName> Ꮡdef) => types_package.typInternal⁔(Ꮡcheck, e0, Ꮡdef);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Type /*res*/ instantiatedType(this ж<Checker> Ꮡcheck, ж<indexedExpr> Ꮡix) => types_package.instantiatedType⁔(Ꮡcheck, Ꮡix);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int64 arrayLength(this ж<Checker> Ꮡcheck, ast.Expr e) => types_package.arrayLength⁔(Ꮡcheck, e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<Type> typeList(this ж<Checker> Ꮡcheck, slice<ast.Expr> list) => types_package.typeList⁔(Ꮡcheck, list);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void validType(this ж<Checker> Ꮡcheck, ж<Named> Ꮡtyp) => types_package.validType⁔(Ꮡcheck, Ꮡtyp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool validType0(this ж<Checker> Ꮡcheck, token.Pos pos, Type typ, slice<ж<Named>> nest, slice<ж<Named>> path) => types_package.validType0⁔(Ꮡcheck, pos, typ, nest, path);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool allowVersion(this ref Checker check, goVersion want) => types_package.allowVersion⁔(ref check, want);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool verifyVersionf(this ж<Checker> Ꮡcheck, positioner at, goVersion v, @string format, params ꓸꓸꓸany argsʗp) => types_package.verifyVersionf⁔(Ꮡcheck, at, v, format, argsʗp);
}
partial class typesᴛConfig {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<Package>, error) Check(this ж<Config> Ꮡconf, @string path, ж<token.FileSet> Ꮡfset, slice<ж<ast.File>> files, ж<Info> Ꮡinfo) => types_package.Check⁔(Ꮡconf, path, Ꮡfset, files, Ꮡinfo);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int64 alignof(this ref Config conf, Type T) => types_package.alignof⁔(ref conf, T);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<int64> offsetsof(this ref Config conf, ж<Struct> ᏑT) => types_package.offsetsof⁔(ref conf, ᏑT);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int64 offsetof(this ref Config conf, Type T, slice<nint> index) => types_package.offsetof⁔(ref conf, T, index);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int64 @sizeof(this ref Config conf, Type T) => types_package.sizeof⁔(ref conf, T);
}
partial class typesᴛConst {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static constant.Value Val(this ref Const obj) => types_package.Val⁔(ref obj);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void isDependency(this ref Const _) => types_package.isDependency⁔(ref _);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ж<Const> Ꮡobj) => types_package.String⁔(Ꮡobj);
}
partial class typesᴛContext {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string instanceHash(this ж<Context> Ꮡctxt, Type orig, slice<Type> targs) => types_package.instanceHash⁔(Ꮡctxt, orig, targs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Type lookup(this ж<Context> Ꮡctxt, @string h, Type orig, slice<Type> targs) => types_package.lookup⁔(Ꮡctxt, h, orig, targs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Type update(this ж<Context> Ꮡctxt, @string h, Type orig, slice<Type> targs, Type inst) => types_package.update⁔(Ꮡctxt, h, orig, targs, inst);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint getID(this ж<Context> Ꮡctxt, Type t) => types_package.getID⁔(Ꮡctxt, t);
}
partial class typesᴛError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Error(this Error err) => types_package.Error⁔(err);
}
partial class typesᴛFunc {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Signature> Signature(this ref Func obj) => types_package.Signature⁔(ref obj);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string FullName(this ж<Func> Ꮡobj) => types_package.FullName⁔(Ꮡobj);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Scope> Scope(this ref Func obj) => types_package.Scope⁔(ref obj);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Func> Origin(this ж<Func> Ꮡobj) => types_package.Origin⁔(Ꮡobj);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Package> Pkg(this ref Func obj) => types_package.Pkg⁔(ref obj);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool hasPtrRecv(this ref Func obj) => types_package.hasPtrRecv⁔(ref obj);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void isDependency(this ref Func _) => types_package.isDependency⁔(ref _);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ж<Func> Ꮡobj) => types_package.String⁔(Ꮡobj);
}
partial class typesᴛInfo {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool recordTypes(this ref Info info) => types_package.recordTypes⁔(ref info);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Type TypeOf(this ref Info info, ast.Expr e) => types_package.TypeOf⁔(ref info, e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Object ObjectOf(this ref Info info, ж<ast.Ident> Ꮡid) => types_package.ObjectOf⁔(ref info, Ꮡid);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<PkgName> PkgNameOf(this ref Info info, ж<ast.ImportSpec> Ꮡimp) => types_package.PkgNameOf⁔(ref info, Ꮡimp);
}
partial class typesᴛInitializer {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ref Initializer init) => types_package.String⁔(ref init);
}
partial class typesᴛInterface {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<_TypeSet> typeSet(this ж<Interface> Ꮡt) => types_package.typeSet⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void MarkImplicit(this ref Interface t) => types_package.MarkImplicit⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint NumExplicitMethods(this ref Interface t) => types_package.NumExplicitMethods⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Func> ExplicitMethod(this ref Interface t, nint i) => types_package.ExplicitMethod⁔(ref t, i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint NumEmbeddeds(this ref Interface t) => types_package.NumEmbeddeds⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Named> Embedded(this ref Interface t, nint i) => types_package.Embedded⁔(ref t, i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Type EmbeddedType(this ref Interface t, nint i) => types_package.EmbeddedType⁔(ref t, i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint NumMethods(this ж<Interface> Ꮡt) => types_package.NumMethods⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Func> Method(this ж<Interface> Ꮡt, nint i) => types_package.Method⁔(Ꮡt, i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Empty(this ж<Interface> Ꮡt) => types_package.Empty⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsComparable(this ж<Interface> Ꮡt) => types_package.IsComparable⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsMethodSet(this ж<Interface> Ꮡt) => types_package.IsMethodSet⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsImplicit(this ref Interface t) => types_package.IsImplicit⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Interface> Complete(this ж<Interface> Ꮡt) => types_package.Complete⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Type Underlying(this ж<Interface> Ꮡt) => types_package.Underlying⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ж<Interface> Ꮡt) => types_package.String⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void cleanup(this ж<Interface> Ꮡt) => types_package.cleanup⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static iter.Seq<ж<Func>> Methods(this ж<Interface> Ꮡt) => types_package.Methods⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static iter.Seq<ж<Func>> ExplicitMethods(this ж<Interface> Ꮡt) => types_package.ExplicitMethods⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static iter.Seq<Type> EmbeddedTypes(this ж<Interface> Ꮡt) => types_package.EmbeddedTypes⁔(Ꮡt);
}
partial class typesᴛLabel {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ж<Label> Ꮡobj) => types_package.String⁔(Ꮡobj);
}
partial class typesᴛMap {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Type Key(this ref Map m) => types_package.Key⁔(ref m);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Type Elem(this ref Map m) => types_package.Elem⁔(ref m);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Type Underlying(this ж<Map> Ꮡt) => types_package.Underlying⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ж<Map> Ꮡt) => types_package.String⁔(Ꮡt);
}
partial class typesᴛMethodSet {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static iter.Seq<ж<Selection>> Methods(this ж<MethodSet> Ꮡs) => types_package.Methods⁔(Ꮡs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ref MethodSet s) => types_package.String⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Len(this ref MethodSet s) => types_package.Len⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Selection> At(this ref MethodSet s, nint i) => types_package.At⁔(ref s, i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Selection> Lookup(this ж<MethodSet> Ꮡs, ж<Package> Ꮡpkg, @string name) => types_package.Lookup⁔(Ꮡs, Ꮡpkg, name);
}
partial class typesᴛNamed {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static iter.Seq<ж<Func>> Methods(this ж<Named> Ꮡt) => types_package.Methods⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Named> unpack(this ж<Named> Ꮡn) => types_package.unpack⁔(Ꮡn);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool stateHas(this ж<Named> Ꮡn, stateMask m) => types_package.stateHas⁔(Ꮡn, m);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void setState(this ж<Named> Ꮡn, stateMask m) => types_package.setState⁔(Ꮡn, m);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void cleanup(this ж<Named> Ꮡn) => types_package.cleanup⁔(Ꮡn);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<TypeName> Obj(this ref Named t) => types_package.Obj⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Named> Origin(this ж<Named> Ꮡt) => types_package.Origin⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<TypeParamList> TypeParams(this ж<Named> Ꮡt) => types_package.TypeParams⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void SetTypeParams(this ж<Named> Ꮡt, slice<ж<TypeParam>> tparams) => types_package.SetTypeParams⁔(Ꮡt, tparams);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<TypeList> TypeArgs(this ref Named t) => types_package.TypeArgs⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint NumMethods(this ж<Named> Ꮡt) => types_package.NumMethods⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Func> Method(this ж<Named> Ꮡt, nint i) => types_package.Method⁔(Ꮡt, i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Func> expandMethod(this ж<Named> Ꮡt, nint i) => types_package.expandMethod⁔(Ꮡt, i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void SetUnderlying(this ж<Named> Ꮡt, Type u) => types_package.SetUnderlying⁔(Ꮡt, u);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void AddMethod(this ж<Named> Ꮡt, ж<Func> Ꮡm) => types_package.AddMethod⁔(Ꮡt, Ꮡm);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint methodIndex(this ref Named t, @string name, bool foldCase) => types_package.methodIndex⁔(ref t, name, foldCase);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Type rhs(this ж<Named> Ꮡn) => types_package.rhs⁔(Ꮡn);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Type Underlying(this ж<Named> Ꮡn) => types_package.Underlying⁔(Ꮡn);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ж<Named> Ꮡt) => types_package.String⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void resolveUnderlying(this ж<Named> Ꮡn) => types_package.resolveUnderlying⁔(Ꮡn);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, ж<Func>) lookupMethod(this ж<Named> Ꮡn, ж<Package> Ꮡpkg, @string name, bool foldCase) => types_package.lookupMethod⁔(Ꮡn, Ꮡpkg, name, foldCase);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Type /*rhs*/ expandRHS(this ж<Named> Ꮡn) => types_package.expandRHS⁔(Ꮡn);
}
partial class typesᴛNil {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ж<Nil> Ꮡobj) => types_package.String⁔(Ꮡobj);
}
partial class typesᴛPackage {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Path(this ref Package pkg) => types_package.Path⁔(ref pkg);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Name(this ref Package pkg) => types_package.Name⁔(ref pkg);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void SetName(this ref Package pkg, @string name) => types_package.SetName⁔(ref pkg, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string GoVersion(this ref Package pkg) => types_package.GoVersion⁔(ref pkg);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Scope> Scope(this ж<Package> Ꮡpkg) => types_package.Scope⁔(Ꮡpkg);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Complete(this ref Package pkg) => types_package.Complete⁔(ref pkg);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void MarkComplete(this ref Package pkg) => types_package.MarkComplete⁔(ref pkg);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<ж<Package>> Imports(this ref Package pkg) => types_package.Imports⁔(ref pkg);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void SetImports(this ref Package pkg, slice<ж<Package>> list) => types_package.SetImports⁔(ref pkg, list);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ref Package pkg) => types_package.String⁔(ref pkg);
}
partial class typesᴛPkgName {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Package> Imported(this ref PkgName obj) => types_package.Imported⁔(ref obj);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ж<PkgName> Ꮡobj) => types_package.String⁔(Ꮡobj);
}
partial class typesᴛPointer {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Type Elem(this ref Pointer p) => types_package.Elem⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Type Underlying(this ж<Pointer> Ꮡp) => types_package.Underlying⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ж<Pointer> Ꮡp) => types_package.String⁔(Ꮡp);
}
partial class typesᴛScope {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static iter.Seq<ж<Scope>> Children(this ж<Scope> Ꮡs) => types_package.Children⁔(Ꮡs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Scope> Parent(this ref Scope s) => types_package.Parent⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Len(this ref Scope s) => types_package.Len⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<@string> Names(this ref Scope s) => types_package.Names⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint NumChildren(this ref Scope s) => types_package.NumChildren⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Scope> Child(this ref Scope s, nint i) => types_package.Child⁔(ref s, i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Object Lookup(this ref Scope s, @string name) => types_package.Lookup⁔(ref s, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<Object> lookupIgnoringCase(this ref Scope s, @string name, bool exported) => types_package.lookupIgnoringCase⁔(ref s, name, exported);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Object Insert(this ж<Scope> Ꮡs, Object obj) => types_package.Insert⁔(Ꮡs, obj);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool _InsertLazy(this ж<Scope> Ꮡs, @string name, Func<Object> resolve) => types_package._InsertLazy⁔(Ꮡs, name, resolve);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void insert(this ref Scope s, @string name, Object obj) => types_package.insert⁔(ref s, name, obj);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void WriteTo(this ж<Scope> Ꮡs, io.Writer w, nint n, bool recurse) => types_package.WriteTo⁔(Ꮡs, w, n, recurse);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ж<Scope> Ꮡs) => types_package.String⁔(Ꮡs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<Scope>, Object) LookupParent(this ж<Scope> Ꮡs, @string name, token.Pos pos) => types_package.LookupParent⁔(Ꮡs, name, pos);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos Pos(this ref Scope s) => types_package.Pos⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos End(this ref Scope s) => types_package.End⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Contains(this ref Scope s, token.Pos pos) => types_package.Contains⁔(ref s, pos);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Scope> Innermost(this ж<Scope> Ꮡs, token.Pos pos) => types_package.Innermost⁔(Ꮡs, pos);
}
partial class typesᴛSelection {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static SelectionKind Kind(this ref Selection s) => types_package.Kind⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Type Recv(this ref Selection s) => types_package.Recv⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Object Obj(this ref Selection s) => types_package.Obj⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Type Type(this ref Selection s) => types_package.Type⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<nint> Index(this ref Selection s) => types_package.Index⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Indirect(this ref Selection s) => types_package.Indirect⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ж<Selection> Ꮡs) => types_package.String⁔(Ꮡs);
}
partial class typesᴛSignature {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Var> Recv(this ref Signature s) => types_package.Recv⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<TypeParamList> TypeParams(this ref Signature s) => types_package.TypeParams⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<TypeParamList> RecvTypeParams(this ref Signature s) => types_package.RecvTypeParams⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Tuple> Params(this ref Signature s) => types_package.Params⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Tuple> Results(this ref Signature s) => types_package.Results⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Variadic(this ref Signature s) => types_package.Variadic⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Type Underlying(this ж<Signature> Ꮡs) => types_package.Underlying⁔(Ꮡs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ж<Signature> Ꮡs) => types_package.String⁔(Ꮡs);
}
partial class typesᴛSlice {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Type Elem(this ref Slice s) => types_package.Elem⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Type Underlying(this ж<Slice> Ꮡs) => types_package.Underlying⁔(Ꮡs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ж<Slice> Ꮡs) => types_package.String⁔(Ꮡs);
}
partial class typesᴛStdSizes {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static int64 /*result*/ Alignof(this ж<StdSizes> Ꮡs, Type T) => types_package.Alignof⁔(Ꮡs, T);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<int64> Offsetsof(this ж<StdSizes> Ꮡs, slice<ж<Var>> fields) => types_package.Offsetsof⁔(Ꮡs, fields);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static int64 Sizeof(this ж<StdSizes> Ꮡs, Type T) => types_package.Sizeof⁔(Ꮡs, T);
}
partial class typesᴛStruct {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static iter.Seq<ж<Var>> Fields(this ж<Struct> Ꮡs) => types_package.Fields⁔(Ꮡs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint NumFields(this ref Struct s) => types_package.NumFields⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Var> Field(this ref Struct s, nint i) => types_package.Field⁔(ref s, i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Tag(this ref Struct s, nint i) => types_package.Tag⁔(ref s, i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Type Underlying(this ж<Struct> Ꮡt) => types_package.Underlying⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ж<Struct> Ꮡt) => types_package.String⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void markComplete(this ref Struct s) => types_package.markComplete⁔(ref s);
}
partial class typesᴛTerm {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Tilde(this ref Term t) => types_package.Tilde⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Type Type(this ref Term t) => types_package.Type⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ж<Term> Ꮡt) => types_package.String⁔(Ꮡt);
}
partial class typesᴛTuple {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static iter.Seq<ж<Var>> Variables(this ж<Tuple> Ꮡt) => types_package.Variables⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Len(this ж<Tuple> Ꮡt) => types_package.Len⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Var> At(this ref Tuple t, nint i) => types_package.At⁔(ref t, i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Type Underlying(this ж<Tuple> Ꮡt) => types_package.Underlying⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ж<Tuple> Ꮡt) => types_package.String⁔(Ꮡt);
}
partial class typesᴛTypeAndValue {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsVoid(this TypeAndValue tv) => types_package.IsVoid⁔(tv);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsType(this TypeAndValue tv) => types_package.IsType⁔(tv);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsBuiltin(this TypeAndValue tv) => types_package.IsBuiltin⁔(tv);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsValue(this TypeAndValue tv) => types_package.IsValue⁔(tv);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsNil(this TypeAndValue tv) => types_package.IsNil⁔(tv);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Addressable(this TypeAndValue tv) => types_package.Addressable⁔(tv);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Assignable(this TypeAndValue tv) => types_package.Assignable⁔(tv);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool HasOk(this TypeAndValue tv) => types_package.HasOk⁔(tv);
}
partial class typesᴛTypeList {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static iter.Seq<Type> Types(this ж<TypeList> Ꮡl) => types_package.Types⁔(Ꮡl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Len(this ж<TypeList> Ꮡl) => types_package.Len⁔(Ꮡl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Type At(this ref TypeList l, nint i) => types_package.At⁔(ref l, i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<Type> list(this ж<TypeList> Ꮡl) => types_package.list⁔(Ꮡl);
}
partial class typesᴛTypeName {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsAlias(this ж<TypeName> Ꮡobj) => types_package.IsAlias⁔(Ꮡobj);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ж<TypeName> Ꮡobj) => types_package.String⁔(Ꮡobj);
}
partial class typesᴛTypeParam {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<TypeName> Obj(this ref TypeParam t) => types_package.Obj⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Index(this ref TypeParam t) => types_package.Index⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Type Constraint(this ref TypeParam t) => types_package.Constraint⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void SetConstraint(this ref TypeParam t, Type bound) => types_package.SetConstraint⁔(ref t, bound);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Type Underlying(this ref TypeParam t) => types_package.Underlying⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ж<TypeParam> Ꮡt) => types_package.String⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void cleanup(this ref TypeParam t) => types_package.cleanup⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Interface> iface(this ref TypeParam t) => types_package.iface⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool @is(this ref TypeParam t, Func<ж<term>, bool> f) => types_package.is⁔(ref t, f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool typeset(this ref TypeParam t, Func<Type, Type, bool> f) => types_package.typeset⁔(ref t, f);
}
partial class typesᴛTypeParamList {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static iter.Seq<ж<TypeParam>> TypeParams(this ж<TypeParamList> Ꮡl) => types_package.TypeParams⁔(Ꮡl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Len(this ж<TypeParamList> Ꮡl) => types_package.Len⁔(Ꮡl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<TypeParam> At(this ref TypeParamList l, nint i) => types_package.At⁔(ref l, i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<ж<TypeParam>> list(this ж<TypeParamList> Ꮡl) => types_package.list⁔(Ꮡl);
}
partial class typesᴛUnion {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static iter.Seq<ж<Term>> Terms(this ж<Union> Ꮡu) => types_package.Terms⁔(Ꮡu);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Len(this ref Union u) => types_package.Len⁔(ref u);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Term> Term(this ref Union u, nint i) => types_package.Term⁔(ref u, i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Type Underlying(this ж<Union> Ꮡu) => types_package.Underlying⁔(Ꮡu);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ж<Union> Ꮡu) => types_package.String⁔(Ꮡu);
}
partial class typesᴛVar {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static VarKind Kind(this ref Var v) => types_package.Kind⁔(ref v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void SetKind(this ref Var v, VarKind kind) => types_package.SetKind⁔(ref v, kind);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Anonymous(this ref Var obj) => types_package.Anonymous⁔(ref obj);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Embedded(this ref Var obj) => types_package.Embedded⁔(ref obj);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsField(this ref Var obj) => types_package.IsField⁔(ref obj);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Var> Origin(this ж<Var> Ꮡobj) => types_package.Origin⁔(Ꮡobj);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void isDependency(this ref Var _) => types_package.isDependency⁔(ref _);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ж<Var> Ꮡobj) => types_package.String⁔(Ꮡobj);
}
partial class typesᴛVarKind {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this VarKind kind) => types_package.String⁔(kind);
}
partial class typesᴛ_TypeSet {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool IsEmpty(this ref _TypeSet s) => types_package.IsEmpty⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool IsAll(this ref _TypeSet s) => types_package.IsAll⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool IsMethodSet(this ref _TypeSet s) => types_package.IsMethodSet⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool IsComparable(this ref _TypeSet s, map<Type, bool> seen) => types_package.IsComparable⁔(ref s, seen);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint NumMethods(this ref _TypeSet s) => types_package.NumMethods⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Func> Method(this ref _TypeSet s, nint i) => types_package.Method⁔(ref s, i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, ж<Func>) LookupMethod(this ref _TypeSet s, ж<Package> Ꮡpkg, @string name, bool foldCase) => types_package.LookupMethod⁔(ref s, Ꮡpkg, name, foldCase);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string String(this ref _TypeSet s) => types_package.String⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool hasTerms(this ref _TypeSet s) => types_package.hasTerms⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool subsetOf(this ref _TypeSet s1, ж<_TypeSet> Ꮡs2) => types_package.subsetOf⁔(ref s1, Ꮡs2);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool all(this ref _TypeSet s, Func<Type, Type, bool> f) => types_package.all⁔(ref s, f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool @is(this ref _TypeSet s, Func<ж<term>, bool> f) => types_package.is⁔(ref s, f);
}
partial class typesᴛaction {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void describef(this ref action a, positioner pos, @string format, params ꓸꓸꓸany argsʗp) => types_package.describef⁔(ref a, pos, format, argsʗp);
}
partial class typesᴛatPos {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static token.Pos Pos(this atPos s) => types_package.Pos⁔(s);
}
partial class typesᴛblock {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void insert(this ж<block> Ꮡb, ж<ast.LabeledStmt> Ꮡs) => types_package.insert⁔(Ꮡb, Ꮡs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<ast.LabeledStmt> gotoTarget(this ж<block> Ꮡb, @string name) => types_package.gotoTarget⁔(Ꮡb, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<ast.LabeledStmt> enclosingTarget(this ж<block> Ꮡb, @string name) => types_package.enclosingTarget⁔(Ꮡb, name);
}
partial class typesᴛcomparer {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool identical(this ref comparer c, Type x, Type y, ж<ifacePair> Ꮡp) => types_package.identical⁔(ref c, x, y, Ꮡp);
}
partial class typesᴛconstDecl {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ast.Node node(this constDecl d) => types_package.node⁔(d);
}
partial class typesᴛcycleFinder {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void typ(this ж<cycleFinder> Ꮡw, Type typ) => types_package.typ⁔(Ꮡw, typ);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void varList(this ж<cycleFinder> Ꮡw, slice<ж<Var>> list) => types_package.varList⁔(Ꮡw, list);
}
partial class typesᴛdeclInfo {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool hasInitializer(this ref declInfo d) => types_package.hasInitializer⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void addDep(this ref declInfo d, Object obj) => types_package.addDep⁔(ref d, obj);
}
partial class typesᴛenvironment {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<Scope>, Object) lookupScope(this ref environment env, @string name) => types_package.lookupScope⁔(ref env, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Object lookup(this ref environment env, @string name) => types_package.lookup⁔(ref env, name);
}
partial class typesᴛerror_ {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void addf(this ref error_ err, positioner at, @string format, params ꓸꓸꓸany argsʗp) => types_package.addf⁔(ref err, at, format, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void addAltDecl(this ref error_ err, Object obj) => types_package.addAltDecl⁔(ref err, obj);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool empty(this ref error_ err) => types_package.empty⁔(ref err);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static positioner posn(this ref error_ err) => types_package.posn⁔(ref err);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string msg(this ref error_ err) => types_package.msg⁔(ref err);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void report(this ref error_ err) => types_package.report⁔(ref err);
}
partial class typesᴛfuncDecl {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ast.Node node(this funcDecl d) => types_package.node⁔(d);
}
partial class typesᴛgcSizes {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int64 /*result*/ Alignof(this ж<gcSizes> Ꮡs, Type T) => types_package.Alignof⁔(Ꮡs, T);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<int64> Offsetsof(this ж<gcSizes> Ꮡs, slice<ж<Var>> fields) => types_package.Offsetsof⁔(Ꮡs, fields);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int64 Sizeof(this ж<gcSizes> Ꮡs, Type T) => types_package.Sizeof⁔(Ꮡs, T);
}
partial class typesᴛgoVersion {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool isValid(this goVersion v) => types_package.isValid⁔(v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint cmp(this goVersion x, goVersion y) => types_package.cmp⁔(x, y);
}
partial class typesᴛgraphNode {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint cost(this ref graphNode n) => types_package.cost⁔(ref n);
}
partial class typesᴛifacePair {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool identical(this ref ifacePair p, ж<ifacePair> Ꮡq) => types_package.identical⁔(ref p, Ꮡq);
}
partial class typesᴛimportDecl {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ast.Node node(this importDecl d) => types_package.node⁔(d);
}
partial class typesᴛindexedExpr {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static token.Pos Pos(this ref indexedExpr x) => types_package.Pos⁔(ref x);
}
partial class typesᴛinstanceLookup {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Named> lookup(this ref instanceLookup l, ж<Named> Ꮡinst) => types_package.lookup⁔(ref l, Ꮡinst);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void add(this ref instanceLookup l, ж<Named> Ꮡinst) => types_package.add⁔(ref l, Ꮡinst);
}
partial class typesᴛlazyObject {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Scope> Parent(this ref lazyObject _) => types_package.Parent⁔(ref _);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static token.Pos Pos(this ref lazyObject _) => types_package.Pos⁔(ref _);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Package> Pkg(this ref lazyObject _) => types_package.Pkg⁔(ref _);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Name(this ref lazyObject _) => types_package.Name⁔(ref _);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Type Type(this ref lazyObject _) => types_package.Type⁔(ref _);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool Exported(this ref lazyObject _) => types_package.Exported⁔(ref _);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Id(this ref lazyObject _) => types_package.Id⁔(ref _);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string String(this ref lazyObject _) => types_package.String⁔(ref _);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint32 order(this ref lazyObject _) => types_package.order⁔(ref _);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void setType(this ref lazyObject _Δp0, Type _Δp1) => types_package.setType⁔(ref _Δp0, _Δp1);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void setOrder(this ref lazyObject _Δp0, uint32 _Δp1) => types_package.setOrder⁔(ref _Δp0, _Δp1);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void setParent(this ref lazyObject _Δp0, ж<Scope> _Δp1) => types_package.setParent⁔(ref _Δp0, _Δp1);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool sameId(this ref lazyObject _Δp0, ж<Package> _Δp1, @string _Δp2, bool _Δp3) => types_package.sameId⁔(ref _Δp0, _Δp1, _Δp2, _Δp3);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static token.Pos scopePos(this ref lazyObject _) => types_package.scopePos⁔(ref _);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void setScopePos(this ref lazyObject _Δp0, token.Pos _Δp1) => types_package.setScopePos⁔(ref _Δp0, _Δp1);
}
partial class typesᴛmethodSet {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static methodSet add(this methodSet s, slice<ж<Func>> list, slice<nint> index, bool indirect, bool multiples) => types_package.add⁔(s, list, index, indirect, multiples);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static methodSet addOne(this methodSet s, ж<Func> Ꮡf, slice<nint> index, bool indirect, bool multiples) => types_package.addOne⁔(s, Ꮡf, index, indirect, multiples);
}
partial class typesᴛmonoGraph {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void recordCanon(this ref monoGraph w, ж<TypeParam> Ꮡmpar, ж<TypeParam> Ꮡtpar) => types_package.recordCanon⁔(ref w, Ꮡmpar, Ꮡtpar);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void recordInstance(this ж<monoGraph> Ꮡw, ж<Package> Ꮡpkg, token.Pos pos, slice<ж<TypeParam>> tparams, slice<Type> targs, slice<ast.Expr> xlist) => types_package.recordInstance⁔(Ꮡw, Ꮡpkg, pos, tparams, targs, xlist);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void assign(this ж<monoGraph> Ꮡw, ж<Package> Ꮡpkg, token.Pos pos, ж<TypeParam> Ꮡtpar, Type targ) => types_package.assign⁔(Ꮡw, Ꮡpkg, pos, Ꮡtpar, targ);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint localNamedVertex(this ref monoGraph w, ж<Package> Ꮡpkg, ж<Named> Ꮡnamed) => types_package.localNamedVertex⁔(ref w, Ꮡpkg, Ꮡnamed);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint typeParamVertex(this ref monoGraph w, ж<TypeParam> Ꮡtpar) => types_package.typeParamVertex⁔(ref w, Ꮡtpar);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void addEdge(this ref monoGraph w, nint dst, nint src, nint weight, token.Pos pos, Type typ) => types_package.addEdge⁔(ref w, dst, src, weight, pos, typ);
}
partial class typesᴛnodeQueue {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint Len(this nodeQueue a) => types_package.Len⁔(a);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void Swap(this nodeQueue a, nint i, nint j) => types_package.Swap⁔(a, i, j);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool Less(this nodeQueue a, nint i, nint j) => types_package.Less⁔(a, i, j);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void Push(this ref nodeQueue a, any x) => types_package.Push⁔(ref a, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static any Pop(this ref nodeQueue a) => types_package.Pop⁔(ref a);
}
partial class typesᴛnodeSet {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void add(this ref nodeSet s, ж<graphNode> Ꮡp) => types_package.add⁔(ref s, Ꮡp);
}
partial class typesᴛobject {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Scope> Parent(this ref @object obj) => types_package.Parent⁔(ref obj);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static token.Pos Pos(this ref @object obj) => types_package.Pos⁔(ref obj);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Package> Pkg(this ref @object obj) => types_package.Pkg⁔(ref obj);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Name(this ref @object obj) => types_package.Name⁔(ref obj);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Type Type(this ref @object obj) => types_package.Type⁔(ref obj);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool Exported(this ref @object obj) => types_package.Exported⁔(ref obj);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Id(this ref @object obj) => types_package.Id⁔(ref obj);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string String(this ref @object obj) => types_package.String⁔(ref obj);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint32 order(this ref @object obj) => types_package.order⁔(ref obj);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static token.Pos scopePos(this ref @object obj) => types_package.scopePos⁔(ref obj);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void setParent(this ref @object obj, ж<Scope> Ꮡparent) => types_package.setParent⁔(ref obj, Ꮡparent);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void setType(this ref @object obj, Type typ) => types_package.setType⁔(ref obj, typ);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void setOrder(this ref @object obj, uint32 order) => types_package.setOrder⁔(ref obj, order);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void setScopePos(this ref @object obj, token.Pos pos) => types_package.setScopePos⁔(ref obj, pos);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool sameId(this ref @object obj, ж<Package> Ꮡpkg, @string name, bool foldCase) => types_package.sameId⁔(ref obj, Ꮡpkg, name, foldCase);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint cmp(this ж<@object> Ꮡa, ж<@object> Ꮡb) => types_package.cmp⁔(Ꮡa, Ꮡb);
}
partial class typesᴛobjset {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Object insert(this ref objset s, Object obj) => types_package.insert⁔(ref s, obj);
}
partial class typesᴛoperand {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool convertibleTo(this ж<operand> Ꮡx, ж<Checker> Ꮡcheck, Type T, ж<@string> Ꮡcause) => types_package.convertibleTo⁔(Ꮡx, Ꮡcheck, T, Ꮡcause);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static token.Pos Pos(this ref operand x) => types_package.Pos⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string String(this ж<operand> Ꮡx) => types_package.String⁔(Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void setConst(this ref operand x, token.Token k, @string lit) => types_package.setConst⁔(ref x, k, lit);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool isNil(this ref operand x) => types_package.isNil⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (bool, errors.Code) assignableTo(this ж<operand> Ꮡx, ж<Checker> Ꮡcheck, Type T, ж<@string> Ꮡcause) => types_package.assignableTo⁔(Ꮡx, Ꮡcheck, T, Ꮡcause);
}
partial class typesᴛposSpan {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static token.Pos Pos(this posSpan e) => types_package.Pos⁔(e);
}
partial class typesᴛsubstMap {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool empty(this substMap m) => types_package.empty⁔(m);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Type lookup(this substMap m, ж<TypeParam> Ꮡtpar) => types_package.lookup⁔(m, Ꮡtpar);
}
partial class typesᴛsubster {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Type typ(this ж<subster> Ꮡsubst, Type typ) => types_package.typ⁔(Ꮡsubst, typ);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Type typOrNil(this ж<subster> Ꮡsubst, Type typ) => types_package.typOrNil⁔(Ꮡsubst, typ);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Var> var_(this ж<subster> Ꮡsubst, ж<Var> Ꮡv) => types_package.var_⁔(Ꮡsubst, Ꮡv);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Tuple> tuple(this ж<subster> Ꮡsubst, ж<Tuple> Ꮡt) => types_package.tuple⁔(Ꮡsubst, Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Func> func_(this ж<subster> Ꮡsubst, ж<Func> Ꮡf) => types_package.func_⁔(Ꮡsubst, Ꮡf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Term> term(this ж<subster> Ꮡsubst, ж<Term> Ꮡt) => types_package.term⁔(Ꮡsubst, Ꮡt);
}
partial class typesᴛterm {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ж<term> Ꮡx) => types_package.String⁔(Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool equal(this ж<term> Ꮡx, ж<term> Ꮡy) => types_package.equal⁔(Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<term>, ж<term>) union(this ж<term> Ꮡx, ж<term> Ꮡy) => types_package.union⁔(Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<term> intersect(this ж<term> Ꮡx, ж<term> Ꮡy) => types_package.intersect⁔(Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool includes(this ж<term> Ꮡx, Type t) => types_package.includes⁔(Ꮡx, t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool subsetOf(this ж<term> Ꮡx, ж<term> Ꮡy) => types_package.subsetOf⁔(Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool disjoint(this ref term x, ж<term> Ꮡy) => types_package.disjoint⁔(ref x, Ꮡy);
}
partial class typesᴛtermlist {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string String(this termlist xl) => types_package.String⁔(xl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool isEmpty(this termlist xl) => types_package.isEmpty⁔(xl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool isAll(this termlist xl) => types_package.isAll⁔(xl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static termlist norm(this termlist xl) => types_package.norm⁔(xl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static termlist union(this termlist xl, termlist yl) => types_package.union⁔(xl, yl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static termlist intersect(this termlist xl, termlist yl) => types_package.intersect⁔(xl, yl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool equal(this termlist xl, termlist yl) => types_package.equal⁔(xl, yl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool includes(this termlist xl, Type t) => types_package.includes⁔(xl, t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool supersetOf(this termlist xl, ж<term> Ꮡy) => types_package.supersetOf⁔(xl, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool subsetOf(this termlist xl, termlist yl) => types_package.subsetOf⁔(xl, yl);
}
partial class typesᴛtpWalker {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool /*res*/ isParameterized(this ж<tpWalker> Ꮡw, Type typ) => types_package.isParameterized⁔(Ꮡw, typ);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool varList(this ж<tpWalker> Ꮡw, slice<ж<Var>> list) => types_package.varList⁔(Ꮡw, list);
}
partial class typesᴛtypeDecl {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ast.Node node(this typeDecl d) => types_package.node⁔(d);
}
partial class typesᴛtypeError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string format(this ref typeError err, ж<Checker> Ꮡcheck) => types_package.format⁔(ref err, Ꮡcheck);
}
partial class typesᴛtypeParamsById {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint Len(this typeParamsById s) => types_package.Len⁔(s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool Less(this typeParamsById s, nint i, nint j) => types_package.Less⁔(s, i, j);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void Swap(this typeParamsById s, nint i, nint j) => types_package.Swap⁔(s, i, j);
}
partial class typesᴛtypeWriter {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void @byte(this ref typeWriter w, byte b) => types_package.byte⁔(ref w, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void @string(this ref typeWriter w, @string s) => types_package.string⁔(ref w, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void error(this ref typeWriter w, @string msg) => types_package.error⁔(ref w, msg);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void typ(this ж<typeWriter> Ꮡw, Type typ) => types_package.typ⁔(Ꮡw, typ);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void typeSet(this ж<typeWriter> Ꮡw, ж<_TypeSet> Ꮡs) => types_package.typeSet⁔(Ꮡw, Ꮡs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void typeList(this ж<typeWriter> Ꮡw, slice<Type> list) => types_package.typeList⁔(Ꮡw, list);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void tParamList(this ж<typeWriter> Ꮡw, slice<ж<TypeParam>> list) => types_package.tParamList⁔(Ꮡw, list);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void typeName(this ref typeWriter w, ж<TypeName> Ꮡobj) => types_package.typeName⁔(ref w, Ꮡobj);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void tuple(this ж<typeWriter> Ꮡw, ж<Tuple> Ꮡtup, bool variadic) => types_package.tuple⁔(Ꮡw, Ꮡtup, variadic);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void signature(this ж<typeWriter> Ꮡw, ж<Signature> Ꮡsig) => types_package.signature⁔(Ꮡw, Ꮡsig);
}
partial class typesᴛunifier {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool unify(this ж<unifier> Ꮡu, Type x, Type y, unifyMode mode) => types_package.unify⁔(Ꮡu, x, y, mode);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void tracef(this ref unifier u, @string format, params ꓸꓸꓸany argsʗp) => types_package.tracef⁔(ref u, format, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string String(this ref unifier u) => types_package.String⁔(ref u);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool join(this ref unifier u, ж<TypeParam> Ꮡx, ж<TypeParam> Ꮡy) => types_package.join⁔(ref u, Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<TypeParam> asBoundTypeParam(this ref unifier u, Type x) => types_package.asBoundTypeParam⁔(ref u, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void setHandle(this ref unifier u, ж<TypeParam> Ꮡx, ж<Type> Ꮡh) => types_package.setHandle⁔(ref u, Ꮡx, Ꮡh);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Type at(this ref unifier u, ж<TypeParam> Ꮡx) => types_package.at⁔(ref u, Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void set(this ref unifier u, ж<TypeParam> Ꮡx, Type t) => types_package.set⁔(ref u, Ꮡx, t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint unknowns(this ref unifier u) => types_package.unknowns⁔(ref u);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<Type> inferred(this ref unifier u, slice<ж<TypeParam>> tparams) => types_package.inferred⁔(ref u, tparams);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool /*result*/ nify(this ж<unifier> Ꮡu, Type xʗp, Type yʗp, unifyMode mode, ж<ifacePair> Ꮡp) => types_package.nify⁔(Ꮡu, xʗp, yʗp, mode, Ꮡp);
}
partial class typesᴛunifyMode {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string String(this unifyMode m) => types_package.String⁔(m);
}
partial class typesᴛvarDecl {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ast.Node node(this varDecl d) => types_package.node⁔(d);
}
// </MethodForwarders>
