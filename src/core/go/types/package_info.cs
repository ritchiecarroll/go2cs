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
[assembly: GoPositionMap("go/types/alias.go", "alias.cs", "ADVwopSCypCUnsCYkJiQmKKCqpCYkL7SgpSmooKUgoK4gqqigsrSgoKCgoK4gqas0oKCgoKCgoLGpg==")]
[assembly: GoPositionMap("go/types/api.go", "api.cs", "AEaKAaIADBKCAHD4AcIAgAGOAoKqooKUgoKmAAIU8oKUAAISAAgCgoKUlIIADhqiqJKqoqyylJSqoqqiqqKqogAXKIKCgoKUlIKCAAQcAAsCgg==")]
[assembly: GoPositionMap("go/types/api_predicates.go", "api_predicates.cs", "AAwmAAkGgpQAAhDSgoIAAhDSgq7ClLiClK7CAAIiAA4CgqqigoI=")]
[assembly: GoPositionMap("go/types/array.go", "array.cs", "ABEkkLiQloCUgg==")]
[assembly: GoPositionMap("go/types/assignments.go", "assignments.cs", "ABYwAAgClJSkAAoOgoKmgtyCgoKCgpSCpoKCgoKUpoKCgpS0tKSCgpSCgpSCgtyCgoLcgqaCgIKClJQACAiygoKUuIKCgpSUpoKmgoKmrvKCgpSCuIKClIKCgoKUlKau9KaCgqyygoK4goLKgpSCpoLKlOiCgoKCgqaCpuzSgoKClJSmgpSCgqaCpoKUyMKClLjCgpQADRQACQKCgoKChLSkyri2poKmgpS0tpTGgoKUAAoGgoKClIKCgqYACAaCkoKCgoKClIKCgoIACg7igoKmyIKCyoKCgoKClIKUypSCgoKUuIKCpqaCgoKCuIKUyoKClLiCgt6yyIKCyoKClMqUgoKClKaCgoKCuIKUyoKUuLKCpoKCgoKCgoKUgoKmgoKCgoKU7oKUgpSClLiCgoKUuIKCuKaUgoIABxCCgg==")]
[assembly: GoPositionMap("go/types/badlinkname.go", "badlinkname.cs", "AAko")]
[assembly: GoPositionMap("go/types/basic.go", "basic.cs", "AEqYAYCWgJaAlII=")]
[assembly: GoPositionMap("go/types/builtins.go", "builtins.cs", "ACwuAAwCpoKCpoIABxCCgpSssoKmgoKCgriC2MiCgoKCgpSCgrgADyCCgoKqooKAgoKCgpSUlIKmlILslIKCuIKU7IKCpIKCgpTayoKCgpT6toLIgpSCpIL2pIK2lMiUgoKClJS4gqaCgsiUgqSUgpSmgoLagoKCgpSCgpSUlIKC2qaCgpSClOrGAAsUgoKCpoKUgtqCuIKCyoKCgpSkpKaUgoKCuIKUpoKmAA0agoKAgoKClKaC7KaCgoKUgpSCgpSUgoK4gpSC7IKCgoKCgpSCgpSClKaCgoKmgoIACBCCpoLulILugoKClKSkppSCgoKClIK4goKUpqaCpuyCgoKmgrSkpKaCgqaClMTGpIKCpoKCgoKCgqaCpoKCguyUgoKmgoKmgoK4goKCpoKCpoKCptyClIKCyoKmgoKClOyCgoKUtsaUgoKUpraCgoKC/pSClIKUpoKCpoKCzKKCgoKCgpS4goLagoKC2pSCgqaCgqaCgoLagoKmgoKCpoKm2oKCgoKCpoKCpoKCgpSC7IKUgoK4poKCgpTugoKCpoKCgpSCpsiCgqaCgoKmgoKClIKmyJSCgoKCpoKCpoKCgtqUgoKCgqaCgoLalIKCpoKCpoKCgtqUgoKmgoKC/oKClIKClIIACxaCgoKUgoKCgoKUgtq2gvrCgoKCgqaUpoKCgqasAAkIgIKClIKUgqLIpKSCgtjEpAAIFAAKAoCGooKClIKClJSuwpS0tLSkyoKClLbKwoKClIKCgoKUqqKCgqY=", "42-44:1;192-206:2;236-243:3;254-265:4;304-308:5;330-343:6;426-438:7;480-493:8;531-540:9;1025-1027:1;1059-1068:1", "", "", "", "76:31,77:17,1147:29,1148:21")]
[assembly: GoPositionMap("go/types/call.go", "call.cs", "ABVEABcClIKCgpSUlpKCgoKCgoKU3IKSlIKCpoKCAAIaAAoCgoKCgoKUpoK4goKCypSCgoKClIKUlKaCgoIACAbigpSCgoKCgu6CgoKmopSUgoKUlLjmwoKCuJSUgpS4lIKCuIKClIKCpLSCgoKClIKCgqbGgqSCuIKClJSClMq4goKUlIKCgoKUppaSgoKCgoKCgoKUppKCgoKCggAHEIKCpoLKgpSUuJS0gpSUtIKkgriCpqrCgoKUgoKCgqYAAhQADQKClIKClAAIEoKCgqaUgoKUgpSUuJSmgoKUgoK4ppSCgoKClIKUlLi4lKgABRwAEQIAChaCgqaCgoKUlIK4uIKCgoKClIKCpriUgtyCgoKCgpSWkoKUgoKCgoKotoKCgqSCtLiSgpSCppaSgpTuyoKCgoKUgqaClLim7oKCgsqClLiCuIKUyoKSgoKUgoLKgoKCuMYAGhbEzMqCgoKCgoKUgoK4gpSUpoKCpoKClJSUgoKUgpSUppSCuLiUgoKCtIK0goKCxoKCgoLGgoLEgqSCuIKmgrQAESKCgqaClIKmlIKmgoKUlKaCgpSClIK4gIK2lIKCgqaUgoKCqqKCAAcQgpSCgqaCgu6olIKClJTaAAcQgtyC3IIAChaCgoLcuKaCgpTG2oKUgoKCAAIQ8KrQpIKCgoKmpqKCgvqCmrKCgriCgriCgtak", "139-142:1;154-167:2;249-254:1;367-377:1", "", "178=describef/1/1/13;989=NewTuple/1/1/2", "", "163:25,164:17,421:25,423:17")]
[assembly: GoPositionMap("go/types/chan.go", "chan.cs", "ABk2ksiAloCUgg==")]
[assembly: GoPositionMap("go/types/check.go", "check.cs", "ABosAAsYABYsgoKCgoKUAB1KAAgCgoKmqJKCAB04woIARIwBkoKClIKUAAISAAgCgoKUgqiygoKokoKmgoKCgpQAAhLigoKokoKUgqiSgoKCggAKEqLK5IK4ggAJFJQAEh74goKUgoKCgoKC3IKmgoK0gpSUpqbcgoKUlIKCyrimAAoWyqam2IKClMiSqJIADQzi1sSCgoK4goKmgoLKxKbyuO6CggAbDgAKBoKClJSClKaCgoK4gpSClIKUgpSClIKUgpSClIKCpoKUlKaCpoKCgoKCgoKC2OYACQ6CgoKCgpSmgpSCpoKCqKSClKrc/KKClLTUpJSkuA==", "455-459:1;514-519:1")]
[assembly: GoPositionMap("go/types/const.go", "const.cs", "ABks0pS4gu6CgqiSgoKClIIABSAAEAKCpoKCppKCppSCgpSClIKUgqSCpIKkgqSkgIKkpIKkgqSCpKTIpIKkpMiCgpSUgpSCgoLGgpSCgoLGpNiCgpSUgpSCgoKCxoKUgoKCgsak2KampoKCgqaCgoKClKaCgqaCgoKUysKCgoKClIKu8oKCggAHEIKUppQACgaCgpS0pKjCgoKCgpSCgpSCgpSCgg==", "76-79:1")]
[assembly: GoPositionMap("go/types/context.go", "context.cs", "ADd0kgAHErKCgpSCuIKmpqrygpSCgpSUuN4ACQKUgpSCgpSUuNzY4oKCgoKCgpQ=")]
[assembly: GoPositionMap("go/types/conversions.go", "conversions.cs", "ABUowpSC6KSCgpSClJSmgoKmuIKCggAJEKSCgpSClIKUlJSUlMaCtoKClJSC3IIACBKUgoKClKYACiIAEwSCpoKCgoKCgriC3IKCgtyCuIK4griCyoKmgsqClIKCppSUxoKCgqaClP6CppKCgoKU3JSCsoKUgrKClIKClMiCsoKUgoKClLaigpSCgpS4xoKCpoKCpoKCpoKCgpQ=", "23-40:1;62-81:2;237-245:1;252-267:2;257-266:2.1;270-280:3;282-291:4")]
[assembly: GoPositionMap("go/types/cycles.go", "cycles.cs", "ABAcwoKCggAIMAAYAoKmgoKCpqamlpKCpoK4griCggAHEIKCuMqCpoKCAAQQ8pSCpoKCgpSClJSCuIKCpsqmgoLa")]
[assembly: GoPositionMap("go/types/decl.go", "decl.cs", "ABQg6oKCgoKCgpSUgtqSgoKClJQACAjSgoKUgpS4goKUgoKCggAgQoK0gsaCxgAJDraCpoLKgpSCpoKU7pSCtILGgsbExAAQCAAIBIKCgoKCgsqCgoKCgoKCgrTqgoIAAhgACQKClIKUpoIACQrogoKClJSCgpTcgtyC3IKmguj6qoKCgoKCpoLcgoKUlKaCgpSmgoKClMqikoKCpgAgKrKysrK0goK4gtiCgpS0poKUgrSCpIK0grTWxOjEyNKmgoKUgqamgoKmgpSClKiSgu6UlKLkoqaCAAsYgpSUpoKCgoKCppSCAAcQgoK4qJKCgpSCAAwG4qaUgoKCpqiSgrimgpSCpoK4koKCyoKCgqaCgriCggAHEJSCgqaCgpS4gqYAChaCkKSCgoKmgoK4goK05OK4goKC7gAIEoKCgqaCgqaCgsqCppSClLSkuIKUtKSCgpSClJSmAAkOgoKCpvqCgpSClsiCgriS3IKCgsqmgoKClJSmgsqCgoKCgoLKgoKmuIKCgu7CppSClIK4gpSCyoKiyqKUkpSmgoKClIKCprjcgoLIlIKCuIKCgqbGgrSCtoLcgoKmyriClMi4goKCgsQ=", "52-58:1;67-70:2;140-142:3;240-246:1;278-280:1;412-415:1;524-530:1;554-559:2;615-615:3;661-663:1;749-751:1;837-839:1;846-942:1", "", "684=Name/1/1/6,describef/1/1/6;938=describef/1/1/2;1031=describef/1/1/2", "", "71:25,73:17,86:25,87:17,169:36,170:13,313:25,314:17,547:59,548:13,715:29,716:21,777:21,778:13,838:21,839:13")]
[assembly: GoPositionMap("go/types/errors.go", "errors.cs", "ABgkgoKmgpQAFSqSgpQABBIACAKokri4gqaCgpTokoKmgoKCgoKCppQACgiSggAHEIKUgoK4gtyCgoKCgsqCgoKmuMiSlO6CgriCuIKCgpTcpoIACRTKgoKCpoKmgoKUABAagoKCpqKCgqaigoKCpqKCgoIACQyCAA8YgsyykoKUrLKUtKSCpIKClLSC", "", "", "219=stripAnnotations/1/1/3")]
[assembly: GoPositionMap("go/types/errsupport.go", "errsupport.cs", "ABYeAB82AAYSgoKCgoKCgoKCgoKUgoKUgriClKSkpKaClLSklKSkpLjKooKCpg==")]
[assembly: GoPositionMap("go/types/eval.go", "eval.cs", "ABEwAA0EgoKmpoIAAioAGASSgoKCgtyCgqaCgoKmgtyCgoKmkoKClA==")]
[assembly: GoPositionMap("go/types/expr.go", "expr.cs", "AD9+lAAJEKKCgoKmgpTKopSktLyilILGgramygAPEtKCgqaCuIKCgpSCgqaCgoKClIK4goKClIK2goKmgpSUgoKUgoKCpr7SgoKUgoKUgpSClKaCpoKCgqamgqamxoKmlJSUAAgYAAsCgoK4ABIkgoKUABEaAAgQgpS2gpSmpqaC2NqCgoLKlLiCgtyUgoKCyqiSgoIAAxQACAKCuJSClKakgoKClNyUgsaC/ILYgqakxoKCgpSClKaClNyCpoKU9IKmtJSo1IKCpoKmgriCgoKUuIKCuIKmpoKClMqCyIKCtoKC6pSCtILYyIK4yoLKgpSEkpSClKaCgpSmgpSUqqK0pqj2goKmyoKCAAUQopSCgoKCpqaCgoLKlIKC6oKCgsaCgriClIKUgpSWkoKCgoLcgqaCgoK4ABMogoKmgsqCgoKm6pQAEibClIKUgpSCgoKmgoKmgoKmgoKmpoKCgpSClKaCpoKCppSCgoK4goKCgoKCypSClKaClIKCgqa8AA4UlIKmgtyCpoKmgqaClIK4gpSmgoKClIKCggAVMKKCgqYACBQACwKCgoKCgriUggAIEoKUAAsM0oKUgrSCxoKCgpS2goKCvtKClIKCAAsOAAkGgpSUtsiCtoKCyIKCyIKC2oKCptaCgoKUlILIgoLIgoKmpoKUgoKUgoKUgoKUgoK2poKUtIK0goKCgoKUgoKUgpSUgtiCgpSCgsiCgtqCAAkIggAJEMiClIKCggAEFgAIAqSCgoKClIKkgoKClIKkgIKkgILGpJTIsoKCpoKCpgACENKCgqiSgoIAAhAACAKCgpSUgoKUuIKCgoKUgqassoKCgq7CgoIADArCgoKClIKUlLSCtIK0pIK6spSCgoI=", "198-213:1;442-448:1;885-922:1;976-979:1;1149-1161:1", "", "", "", "1086:25,1087:17")]
[assembly: GoPositionMap("go/types/exprstring.go", "exprstring.cs", "AA4ksoKCABQMAAoMpLa2goLItoKCtoKCgpS2goK2goLWgoKCgraCgoKUgoKUgoKCpraCgoK2goKCgpS2graCtoKCgoK2goKUgraCgraCtoKCtoKCgraClLS0pILIooKClIKClKaClIK4goKmooKCuKaCgriCptyigoKUuKKCgpQ=")]
[assembly: GoPositionMap("go/types/format.go", "format.cs", "ABYkoqKUtLS0goKCgpSUgrSCxrSCgoKCtLSCgoKCtIKCgoKCgpSUgrSCgoKCgoKUlIKklMjCgoKCgpSmsriCggAIFrKUpKTawqa0goKCgqaClJSqwoKUlIKCgpSUgtqSgpSCpoKU", "", "", "146=Repeat/1/1/3,sprintf/1/1/4")]
[assembly: GoPositionMap("go/types/gccgosizes.go", "gccgosizes.cs", "AAsa")]
[assembly: GoPositionMap("go/types/gcsizes.go", "gcsizes.cs", "ABQe0oLKyKQABxDcgoKCpgAIDIK2guaklIKmgpSClOaCgoKClIKmgoKCgpSmpqKkgoKCgqaCxoKCpoKClIKYooKUpKSCgpSCgoKCuIKmyILEpMyygpQ=", "16-18:1", "", "", "", "27:64,28:13")]
[assembly: GoPositionMap("go/types/index.go", "index.cs", "ABcm4qaUgriUgoKUppTagoK4goKUgoKCyoLIgoKClLaCgoKCyIKCtoKCgpSCgpSCgoKmgpailIKCkpSCgsaCgoLGgoLGtIKkgpSUgoK4gqaCpoKUpoKCgoKUgoKUgoKCuIKCyJSCgoKmgoKC3IKmguaygoKCqKKUgoKCgriCgriCgriCgoKmlKaCgriCgqaCpoKCgoKUgoKUgoK4gtqCgoKCgpS2goKCyMiCgoKmpoKCgqiSgoLKgoKUgtjGpMqCgoKCuIKCAAgU0oKClJSU7vKClIKCgqaCpoKmgoKCgrimooK4goK4goKmlIKCuIKCuAAXKILGgpQACBAABxA=", "112-161:1;222-249:1")]
[assembly: GoPositionMap("go/types/infer.go", "infer.cs", "ACBGABYKgpK4goKSyoKmpoLKgoLcgoKCAB4+goLclLSCuIKCgoKmgoKmlMqCgpSmutqCgqaCypSCpoKCgpQAChbKgoLcggARJIKCgoKUpoKCgoLKlJSCAAsYuILGgsoAESKCggABEOKCpILKgriCgtyCrLKSgoKCgpSCgpSCgoKUlMqCgoIACxiCgoIAGjaasoKCuIKCyoKCgoIADBqClIKCpu6CgoKCqAAFFAAkOIKmgoKCgqaCgqYACAq0gpSkpKiSgoKUlIKCrLK4AAkQ5IKUgoKm2qampqYABxAACRSmgoKCpqqmpoKC2ra25oKCgqbO4oKCgoKCgpSCgoKUlIKCgpSUypQAAhTygoIACxTCgriClMqUgpQACBC2trbugpSCyILIgpSCyIK2toLIgtjEpIKC", "41-43:1;48-50:2;119-154:3;305-305:4;567-569:1;615-617:2;659-670:1", "", "", "785:27", "58:68,59:17,66:25,67:17,611:21,612:13")]
[assembly: GoPositionMap("go/types/initorder.go", "initorder.cs", "ABso1riClIKCgoKUgoKCgqa4lIKCgoKmlAAIEoKUlILKggAIEoIACBKCgriCgoLugpSUgoKUgqaCgoKClN6ygpSWkoKUqIKClIK4yJKmgoKmgpSCgoKUAB44ogAICoKClMzkgpSC7pSUgoKCypKCgpQADRyGppSmlIKCpqaCyoKCpgALFJSCgoKmgqaCgoLKpoKmgoKCgoI=", "151-153:1;273-275:1", "", "84=Name/1/1/1,order/1/1/1")]
[assembly: GoPositionMap("go/types/instantiate.go", "instantiate.cs", "ABlsAB4CgoKUlIKCgoKUkriCAAQiABMM0oKUgpS4goLcsoKUyoKCuJTIgsqUgqa2lJSClIKUuIKCuILYyKyygpS0tKaCgoKmqJKClMqCgoKmAAgSAAgCgoKClIKmgoKmgoKCgoKUlJS4gtyCgsqCgpS4koKUuJKCuIK4lIKUgpSUgpTKgtyClIKUlKiSgriCgoKCppSUgoKUtLSklKbKopSCgtiCguiCtg==", "115-120:1;291-316:1;340-355:2")]
[assembly: GoPositionMap("go/types/interface.go", "interface.cs", "AB46sPae0oKClAACEuKCuIKCgsqUgoKUyLKCgpTOwqiAmJCWgJywloCWgJiQloCWgJiQloAAARIACQKClIKmgprCgoIACgbSgoKClKaCgoLKgoKCpoKCgoKUuIKCgpSokoKCppSCgsqUlIK43II=", "160-166:1;231-233:2", "", "264=describef/1/1/2")]
[assembly: GoPositionMap("go/types/iter.go", "iter.cs", "AAwoAAsCgoKCAAUUwoKCggAFErKCgoIABxKygoKCAAcSsoKCggAHErKCgoIABxKygoKCAAcSsoKCggAHErKCgoIABxKygoKCAAcSsoKCgg==", "21-27:1;35-41:1;48-54:1;61-67:1;74-80:1;87-93:1;100-106:1;113-119:1;126-132:1;139-145:1;152-158:1")]
[assembly: GoPositionMap("go/types/labels.go", "labels.cs", "ABkexJTcgoKCgoKCgpSClLiigoIADxzSgoKUgoKClKqigoKmqqKCgqbMspQABhCCgqaYtqaCwpSC2oCCgoKCgoKCpoKmgoKUgoKCAAkWgqaCpLaCuILKgoCC1MaCguyCgIKkxoKCyJSCyIK4goK2gsi2goLItra2tra2yIKm", "107-110:1;112-114:2;116-120:3;123-264:4", "", "182=Position/1/1/5")]
[assembly: GoPositionMap("go/types/literals.go", "literals.cs", "ABUq0oKCpoKClIKUgoKClIKClIK4spQAABQACQKCgoK2gsqCgqaC5sKApoKCuILKsqaClIIAFAjCkpTKuIKClILIgpSCgpTIlIK2gKSCyoKUgoKCgoKUpoKCgpSCgoKClIKClIKCgpSCgpS4goKClIKCgqaCgoKUgpSC7AAIEoLKgtrsgoKCgoKClIKCgpSCgoKCgoKCppSClIKCpoL8oriUpoKCgpSCgpSCgtiC7sKCkpSCgoKCgoKUpoKClLiCgpSUgoKokoKU", "99-101:1", "", "118=describef/1/1/2")]
[assembly: GoPositionMap("go/types/lookup.go", "lookup.cs", "AA1KABcCgoKUpLS0pAACRgAgAoKUqgANEICCgoKClMicwoKCgoK4AAIiABQGgtymgoLKnvaCpoK4gtyUpqaCgpSCgrimgoKCgoKUgoIAChaCuOyAgoKCgpSC6tyUgqampgAKHLKCpoKCgoKmgoKmppSAgraCgrgACxSCgoKmgoKmpoKCgoKmgpSCAAQYAAkCAAQYAAsCgoKmAAgWgoKUgoKilIKClIKmgoK4oqaClLS0goKCgoKmyLiCgoK4gqaCgsqCpoKmgqaUlLS01pK0tJKmlAAHEIKUtLS0tMgAAhLigpSCyqKCgpSCgoK4poKCyJKCgpSqwoKCgpSCgoKCAAISAAkIgqYAAhAACAiClMyylIKClJSUqqKCgqaqooKCqqKCgoK4qqKCgoK4")]
[assembly: GoPositionMap("go/types/map.go", "map.cs", "ABAgksiAloCUgg==")]
[assembly: GoPositionMap("go/types/methodset.go", "methodset.cs", "ABwsgoKmgoKClIKogJaAlrKCpoKCgpSCgoKmAAgaABEWgIK4pKaCuJ72gpaSlIK4gtyUlIK4lIKClNyCuNrsopSClIKU3IKCgpS4poKokoKCgriWAAkWsoKUgpSmooKUlMqCgqaC", "49-52:1;204-206:1")]
[assembly: GoPositionMap("go/types/mono.go", "mono.cs", "AFOsAQAIDoKClIKCuIKCpoKCgoKmgsqygtyCgoLcgsqCgpSCkoKUlLS0tsqigpSqooKCgpS6AAsSgriCgoKmqqKStLaCtoKmgoLI+LSCtLS2gsaCgqaCtILIqsKCgqaCgqaCpriCgoKCgoKm3IKUgqiygqaUgqaCpoKCgqaU", "201-208:1;213-263:2;251-255:2.1")]
[assembly: GoPositionMap("go/types/named.go", "named.cs", "ALcB/gKygpSCgoKUlAAELgAXAoLKgqaCuICkgoKUgpSClIKClJQACRSCgpSClIKClIKCuILYkqqilIKClIKmgqaCpoLswoKCpoKUAAISAAoClILugpSClIKUxsaClKqigpSqwoKUqpCYwoKokoKUqJIAAhwAEAKUgqaClIKUgoKmgoKCuIKCuNrmgpSUgqaCAAgSgqa4goKCgpSmpoKmgoKUpoKq8oKClIKmgpSCgpSCgqKq4oKCgoK+soKUgoKCuIKCuKzSgpQAAhDyyoKCgriCpqYAASYAEwKUgoKmgoKClLa2goK4goKmgpSUgoLGyJLCgsiCgrTIsoK4lKbIkoKUAApEACQCgoKCgpKCuIKUgqaClIKUgqaCgpSCgqbKgqaClIKCgpSUgqaCpqj+woKU", "241-241:1;676-686:1;748-751:1", "", "", "", "242:64,243:13,831:25,832:17")]
[assembly: GoPositionMap("go/types/object.go", "object.cs", "AEGKAYKCqsKC7qaClAAPIJCWgJiQloCWgJqgloCUgoKUoKKSgICilLSC3IKmgqYAAhoADQKCuIKUgriCgoKClLiClIKmAAsYosqQAAkUosiAlAAOJPKqooKCgsiylNqCAAcQpKS0ABgyAAsUgoKUyICYkL7SrsKssoKCqqLKkJaAloAAARIACQKClKYADhqigoLuyJKCAAkUqqKCgqygAAESAAkCgpSusJbagoIABxCmAAwUkgAJFIIAGxCigpSUgoKUpraCgoLIgpTIgoKClKaCtoK2gra2poKUlIKmgrikgraCgoKUgrjugoKmgqaigpSCgpSUgpSssoKCxrKysrKysrL0ooKCgoLKlJSCgoKmACQIkpSkpIKCgpTGlKSkpKSkxoKUxqSklIKU")]
[assembly: GoPositionMap("go/types/objset.go", "objset.cs", "ABYwwoKClIKUgg==")]
[assembly: GoPositionMap("go/types/operand.go", "operand.cs", "ACVOAB08tIKUABBOACUEgoKUpKS4griUgoKUlLS02oKCuILqgoKCgpTapoKCgsqCgoKC7oKCpIKUlNqClIKClIK4yoKmABQMspSkpKSkpKSkpKSkpOiCqJKClLS0tLS0toKCgoKUgoKokoKUAAcUAAkCgqaCgqaCpoKCgqaCgqaCgsqCpoLugtyAgoK4gpSC2oKUgpTugIKC2oKmkoKCgpTcgoKCooKUgoKClJTcgoKCgrKClIKCgoKUlKY=", "337-346:1;402-410:2;417-427:3;438-449:4")]
[assembly: GoPositionMap("go/types/package.go", "package.cs", "ABk4ooLIgJaAloCcsJzigpSqkJaAAAEWAAgAmJCUgg==")]
[assembly: GoPositionMap("go/types/pointer.go", "pointer.cs", "ABAggLaAlII=")]
[assembly: GoPositionMap("go/types/predicates.go", "predicates.cs", "AA8kgJyggoKCgoKCgoKasoKuoIKCgoKCgpqygIKkrLK0lKyypJSs1oKqoqrGgqiSgqiSqJKCrsKCgpSsxIKUgqiSAAgMsoKUgpSUtoIACg6CgtqCyIKUgoKUlLamqJKk9KSYqNSCpgAKEqIADBTCgpSCpoKmyoLsptqCAAgQgoKCgoim7ILsgoKCgoKCuNqCggAIEoLKgpSmgpSCgpSUgqaCgoK4gqaqpoKCggALFoKCgoKUgpSCggAWLoKCgpSUgoKUgoKCpuyC7IL+uIKCgpSCgqYADRS2yKSssoKmrNaClKSkpKSkpq7mgpSUgpSUqNKCqJKCgqY=", "59-59:1;218-220:1", "", "336=Tag/1/2/1,Tag/2/2/1,sameId/1/1/2,identical/1/1/3;418=identical/1/2/1,identical/2/2/2")]
[assembly: GoPositionMap("go/types/range.go", "range.cs", "ABw0AA8EAAkSgpSCgriCgpTKAAkWooKGlLS0tLSkyoLcgpSUhJKCgqiSlIKClIKmgpSmgpSCgqaCgpSCgoKClLiCgpKmlKSCgriCgqaCgriCpoKCgoKm7qaiABYQAAsCmJSClJSCppSClIKClMakpKSCpIKmlKSUlIKClIKUxraClLaUgpSClJQ=", "68-70:1;206-208:1;210-216:2", "", "73=MakeInt64/1/1/4")]
[assembly: GoPositionMap("go/types/recording.go", "recording.cs", "ABQkpqKClLS0grSklKaU+KKCpoKCgpS4goKCgpSCppSClKbKgoK0pMTeooKCgpSCgoKCgoKCqJSCgpSmAAIS4oKCgoK4goKCuIKCgoK4goKCgriigoKCuIKCgoI=", "", "", "135=newVar/1/2/1,newVar/2/2/2")]
[assembly: GoPositionMap("go/types/resolver.go", "resolver.cs", "ACVKoqiSgoKClAAIDgAIAoKCgqaCpoLGlIK42IIACgiCgoKUgpSCgoKmAAoK0riCgsqCgqaCgqiSgoKU5vyCgoK4goKUgoKWkoKCgoKCpoKCyoKClIKCpoKAgqSAgqSmyoK4gpS4ABQM0gABEOKClp6CgrjKkoKUgoLKlLKmgpSCgoKmgoK4goKClIK4goLcgoKmgpSUppS4gpSCptzcgoKCgpSC7tiClIKCpoLIisK4uIKClIKEkoKUpsaCtIKCgpSCpoKCgpSCgpSUppSCgoKmAAkUuIKUlILKguyCooKCgoKClJSUAAgSgqaCgpSCgoLMkoKCgoKUAAYcAA8EgoKCuKSCgoKCgpT4xKSClNoAAxD4sqaCgriCgriCuIKCyoK4gpSmppSClILugoLKqOSCgrgAECKCAAIQ0pSCgoKUpriSppIABxCoxILugoLKAAgOgoKClIKUAAUQwoKm", "261-460:1;510-512:1")]
[assembly: GoPositionMap("go/types/return.go", "return.cs", "AA8isqQAEBC4gsimgsimhsimpoKCgqimgsimlIKCpqaigoKCgpSCpgAEFPKkABEQpoKClILapoTIpoLIgsimgsiCyILIpoKCgqY=", "", "", "62=isTerminating/1/2/1,isTerminating/2/2/2;166=hasBreak/2/2/1")]
[assembly: GoPositionMap("go/types/scope.go", "scope.cs", "ACFKwpSCgpTIgJaAlpKCgoKClIKogJaAmKIACBKClKzSgoKCpgADEPKCgpTcgpQAAhQACQKClIKmgoKUAAYQ8oKUlIKCpoKCuKiSgoIADRyigIKSlIKUgqaClKakypCCgoKCgoKCgoKCgoKCgg==", "191-205:1")]
[assembly: GoPositionMap("go/types/scope2.go", "scope2.cs", "AA8wAA4CgoKmrrCCmrIAAhAACQaCgoLKgoKCppQ=")]
[assembly: GoPositionMap("go/types/selection.go", "selection.cs", "AFGoAYCWgJiQmKK4goKCgu6CgoKCgoKUgrgAAhoACQAAARLglAALFgALAoKUtLS0pIKCgoKCgoKUlA==")]
[assembly: GoPositionMap("go/types/signature.go", "signature.cs", "ACdiAA4CAAIkABECgoKClIKCgoKUgoIADRyUgoKCgqaCpoKCgpSUgoKUlAAEEtCWgJaAmJCWgJaAlIL8AAwCgoKCgpaSgpSCuIK4uIKUuIKmgoKUgpSCgoKCouoADA6WkoLcyoKCgoKCgpSssoKClNiC+raCAAgSgoKCuIKUuIKCgoKCgrimgsqCgoKUgqaCprqSgoKUqqKU3ILKoqbGgoKCgpQABRwACgKCgpS0poKClMQAChIACgKCppKCgoKCgpS4ppSCgqaUgpSmgoKCgriC7oKCgqaosoKCAAoO5IKCgsqUgoKUgraC5tikgsbEAAwKkg==", "351-353:1", "", "390=describef/1/1/2;612=File/1/1/1,Name/1/1/1,Base/1/1/1,HasPrefix/2/2/1")]
[assembly: GoPositionMap("go/types/sizes.go", "sizes.cs", "ADdq0oLKyKQABxDcgoKCpgAIDIK2guaklIKmgpSClAAIBoKCgpSCzIKCgoKUgqaCgoKClKbGABEgoqSCgoKCpoLGgoKmgoKUgqaCgoKmhJKClKSkgoKUgoKCgpTIgsSk2AAUNuKkgILWgILGyLSCgoKUgpSmooKUgoKUlIKmAAIQ0oKCgoKClIKClJSqooKClMyygg==", "54-56:1", "", "148=Pkg/1/3/1,Pkg/2/3/2,Path/1/2/2,Pkg/3/3/3,Path/2/2/3", "", "62:64,63:13")]
[assembly: GoPositionMap("go/types/slice.go", "slice.cs", "ABAggLaAlII=")]
[assembly: GoPositionMap("go/types/stmt.go", "stmt.cs", "ABgk8oKmgsqCgpTulJSCpoLKoqTChoKigoKmhoKmpoIAFyyCgtiCgoKmxoKCgoKCgoKU+KKCgoKUgsaC1qSCgpTcgoKCpoLGlIKUAAoGgoKCgpSCgoLGgrSkpMikgtyUgIKkgILWgILWlAARIqKCgoKCgpSCgqaCgoKUgqamgoKCgoKCpuykgoKUAAYwABgCgoKClIKClIKCyoKUgoKUgoKCgqaCgvyCgoK4gurSgoKChJKCgoKUgoKUuICUgoKUgoKCgqSCgvyCgoK4ggA2CPSClIKUyqaU+raCvLKCgoKUgpSCtIK0gqS2koKCgpSCyIKUtLSCpoKCgpSCgqaCgoKUtpSCgpSCptqCgpSCgoKUgoKClNi2tqa4goKCgoLKgoKUyIKClJSCxoLGgoKUtLSkxtiClLaClIKCgoKUpvi02IKClIKCgqaCgoLKgoKCppSCgoKCgpSCgoKClJSCyIKClAABFAAIAoKUtIKCpoKCgqaUgpSmxoK4goKCpoKCgoKCgoKClMqUgoKCgoKCpoKUgoKCuJSC7oKCkoKUlILalJSCgoK4goK0tILGyIKCuIKCpoKClILIgoKUgoKCgoKmpoK4lLaCxsQ=", "29-32:1;58-60:1;68-70:2;414-420:1", "", "", "", "43:49,44:13,561:39,563:17")]
[assembly: GoPositionMap("go/types/struct.go", "struct.cs", "ABg2woKCgqaClIKCyICWgJaSgpSmgpqiggAPCMKCgoKokpamkoKCgpSCpoKClIKCAAcQkoKCpoKCgpSC7oKCgoKCgoKUAAcQgpSigpSUpoLGtLiClIL+goLGgpS2gsakpJTGooKCgoKClKaigoKCppQ=", "85-101:1;107-111:2;146-172:3", "", "166=describef/1/1/26")]
[assembly: GoPositionMap("go/types/subst.go", "subst.cs", "ABAoooKCgpSqooKCgpTGgqaCgpQABBTylIK4lKS47gAOFqKmAAkQgoKCuILugILYgoLIgoLIgIKCgtiCgsgADyCUgoKCAA0agLjYgoKCgpSClIKCgoKCAA0cpoKUyIKCgsiCggAJEoKCgqaC7oDK2La2rLKClKaigoKmxqKCgoLGooKAgrbO4oKClIKUpsaigoKmxqKCgoLGooKUAAQSAAkCgoKCgriCgpSCgqY=")]
[assembly: GoPositionMap("go/types/termlist.go", "termlist.cs", "ABUq/JKClIKCgpSUqMiCgqaoyIKCpqjWgoKigpSCgoKU7oKUgqaUuJKosoKqooKCgriopKiSgoKmqJKCgqaokoK4goKm")]
[assembly: GoPositionMap("go/types/tuple.go", "tuple.cs", "ABMksoKUyLKClKiAlII=")]
[assembly: GoPositionMap("go/types/typelists.go", "typelists.cs", "ABIekJaAmtKClAAJDpKClMqQloCa0oKUzKKClIKClJQ=")]
[assembly: GoPositionMap("go/types/typeparam.go", "typeparam.cs", "ABQmkAANKOLI1IKCgpSCgriCgoKUyICYoqiSAAIS4oKUpq7CppqigqiSlpKUlMaUlMiCgoK4lJSClKassq7C")]
[assembly: GoPositionMap("go/types/typeset.go", "typeset.cs", "ACVIgJaAloCWkoKUvICYkJaS5oKUpKaClIKCgoKCpoKClJSClIKUgq6wlrCqsoKmgpSCgpSClIKmrLKClIKCgqb41uKCAAwagqa4gqaCgoKCAAgSlIKCgpSUAA8ggoKCkoCkgrSCgoKCAAkQgqKCgoKCAAgOgriCgoaigpSCgraClIKUgoKUtIKUgoKUgoLEgpSClOymgoKClJQABRLipoKUgoKCgoKmgoKmgqaCpoKmgoKUggAJFsKCuJSCooKClIKCgpSmlLiCgoKUgqaU", "49-51:1;184-187:1;223-252:2;242-249:2.1", "", "273=describef/1/1/7", "", "208:25,209:17")]
[assembly: GoPositionMap("go/types/typestring.go", "typestring.cs", "ACBGooKUgoKUvrKCgqyyrLIADRyCpoKCxoKCgpSClIKCuIKmgoKUACQGwoKClIKUlNqCgoKmtoKCgraCtoKCgsqClILcgoKUgoKCgpSCuKa2gra2gtqCgpSCgpSClMiCyoKUgoKmgoKCuJSCgoKUgoKUgoKUgoKUgqa2goKCtoKClJSCxrS0pIKClIKC7IKUgpSClMiCgpSAuJSCgtyCgrj8gpSClJSU/MSm0oKCgoKUgoKU2LSChJKClIKUgoKUyKKCgoKUlOaigoK4goKUgpSClJSClIKClKaigqaygoKCgqaCgpSCgoKCAAoWgqa4ptKCgoKCgqamlIKUpoKUgriixpKCgoKCgoKCgqY=", "39-44:1;484-486:1", "", "", "161:27", "580:29,581:21")]
[assembly: GoPositionMap("go/types/typeterm.go", "typeterm.cs", "ABgqopSkpKS61JSkuKjUlKSkpKS4ggAHEIKUqNSUpKS4ggAHEIKUqMSUpLiCgpSo1JSkpKS4ggAHEKrCgpSCgpSCgpQ=")]
[assembly: GoPositionMap("go/types/typexpr.go", "typexpr.cs", "ABso8oKUgpSCgoKUpILagoKmyoKCuJKUAA0cgoKClNyCppSCpoKClIKCgpSUlIK2goKU7IKUgoKUtoK2grbGtqqirLKCgqrUgtyCgoKCgpQABhbigoKCgpSCAAIUAAoCgoKCgpSmggAICqIADgrygoKCgoKCppSClMragpSU6LTYgpSU6LT4goLKpoKCgqamgoKUlIKC7LaCgqaCgsqClKaCgqaCgqaCggAHEJKCgoKUuKaUgti0tMiCgraCtoIAEgbigoKCgpS4goKClIKmgpSmgoLugoKCuNiUgoKUlIKClJTKAAwM+oKCgoKUgoK4goKCgpSmgoKCgtyCgpSUgqqigoKCgpSCpg==", "174-185:1;236-249:1;361-369:2;406-410:1;444-465:2", "", "202=describef/1/1/11;436=describef/1/1/8;551=describef/1/1/21", "", "292:25,293:17,513:25,514:17")]
[assembly: GoPositionMap("go/types/under.go", "under.cs", "AA0coq6igpQAAhLiggAMGKKClMqiAAQoABECkoKCgriCuIKCuIKCgrjYtJTKgqY=", "15-17:1;36-38:1")]
[assembly: GoPositionMap("go/types/unify.go", "unify.cs", "AFS2AcKCyoKCgpSUAB4qgpSkpKSUzLKmoqq0goKCgpSUgoKCgoKUgoKUggAICoKCurKClPq2AAgOpKqigoKmqqKCgoKCzJKqooKClKiSgoKCpq7CgoKUysKClM4ADAKCgpSCgpS4griCgpSClO6CgpQAEyiCgpSmlIIACRS2gqa4lJSCgoKmuIIACRSClMoAFSyC6saUyJSmgrjupoKmgoKCgriCABcwgoKClLiClIK4goKmgoKmuIKC3JSCgoKCpgAOHoKClMqCgriSpMqC7KbaggAIEIKCgoKIpuyC7IKCgoKCgrgAChSCzNyCgoKClIKUgoIAFi6CgoKUlIKClIKCgqbsgv6C7LiCgoKUgoKm2oIADRzugoLcAAsStg==", "290-295:1", "", "666=Tag/1/2/1,Tag/2/2/1,sameId/1/1/2,nify/1/1/3;707=nify/1/2/1,nify/2/2/2", "", "332:21,333:13")]
[assembly: GoPositionMap("go/types/union.go", "union.cs", "ABcuooKUxoKUguyAtIKCAA0UwoKUlIKCgriUgoKCpoKmgriC3KKCgqaCgoKCgqaCggAHEIKClLS0pMqCyuaigoKCgpTcgoKUlJSCgpSu4oKCgriCgqaCpqrSgoKClA==", "91-134:1", "", "108=describef/1/1/43")]
[assembly: GoPositionMap("go/types/universe.go", "universe.cs", "ACZUAB8+AA4KgoKUggASJoKmyoKCuIKCpoKCgqaClIKCuIKCppSCggAICAAFFIKCuIIALVQAIU6CgoKClP6ygpSCpoKCgpSCgoKUgoKCgoKssoKCgqaCpoKClJS0xLaC", "", "", "163=MakeBool/1/2/5,MakeBool/2/2/6,MakeInt64/1/1/7")]
[assembly: GoPositionMap("go/types/util.go", "util.cs", "ABI48JaAloCWkoKCpqiwpoCWgJaS")]
[assembly: GoPositionMap("go/types/validtype.go", "validtype.cs", "AA8gwgAGGgAQApSCkpSCgoK4uILIpoKC2oKC2oKCABs2ggAVLIKm7oKCgqYABxCCggAKFIKmpoLuuILs+qKCgpQ=", "39-41:1", "", "", "", "54:25,55:17")]
[assembly: GoPositionMap("go/types/version.go", "version.cs", "ABQoosiSqqLagoKCgoKCgoKCpryyqtKCgpQ=")]
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
[GoMethodHost(typeof(types_package.Alias)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛAlias;
[GoMethodHost(typeof(types_package.ArgumentError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛArgumentError;
[GoMethodHost(typeof(types_package.Array)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛArray;
[GoMethodHost(typeof(types_package.Basic)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛBasic;
[GoMethodHost(typeof(types_package.Builtin)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛBuiltin;
[GoMethodHost(typeof(types_package.Chan)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛChan;
[GoMethodHost(typeof(types_package.Checker)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛChecker;
[GoMethodHost(typeof(types_package.Config)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛConfig;
[GoMethodHost(typeof(types_package.Const)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛConst;
[GoMethodHost(typeof(types_package.Context)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛContext;
[GoMethodHost(typeof(types_package.Error)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛError;
[GoMethodHost(typeof(types_package.Func)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛFunc;
[GoMethodHost(typeof(types_package.Info)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛInfo;
[GoMethodHost(typeof(types_package.Initializer)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛInitializer;
[GoMethodHost(typeof(types_package.Interface)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛInterface;
[GoMethodHost(typeof(types_package.Label)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛLabel;
[GoMethodHost(typeof(types_package.Map)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛMap;
[GoMethodHost(typeof(types_package.MethodSet)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛMethodSet;
[GoMethodHost(typeof(types_package.Named)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛNamed;
[GoMethodHost(typeof(types_package.Nil)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛNil;
[GoMethodHost(typeof(types_package.Package)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛPackage;
[GoMethodHost(typeof(types_package.PkgName)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛPkgName;
[GoMethodHost(typeof(types_package.Pointer)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛPointer;
[GoMethodHost(typeof(types_package.Scope)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛScope;
[GoMethodHost(typeof(types_package.Selection)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛSelection;
[GoMethodHost(typeof(types_package.Signature)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛSignature;
[GoMethodHost(typeof(types_package.Slice)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛSlice;
[GoMethodHost(typeof(types_package.StdSizes)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛStdSizes;
[GoMethodHost(typeof(types_package.Struct)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛStruct;
[GoMethodHost(typeof(types_package.Term)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛTerm;
[GoMethodHost(typeof(types_package.Tuple)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛTuple;
[GoMethodHost(typeof(types_package.TypeAndValue)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛTypeAndValue;
[GoMethodHost(typeof(types_package.TypeList)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛTypeList;
[GoMethodHost(typeof(types_package.TypeName)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛTypeName;
[GoMethodHost(typeof(types_package.TypeParam)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛTypeParam;
[GoMethodHost(typeof(types_package.TypeParamList)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛTypeParamList;
[GoMethodHost(typeof(types_package.Union)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛUnion;
[GoMethodHost(typeof(types_package.Var)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛVar;
[GoMethodHost(typeof(types_package.VarKind)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛVarKind;
[GoMethodHost(typeof(types_package._TypeSet)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛ_TypeSet;
[GoMethodHost(typeof(types_package.action)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛaction;
[GoMethodHost(typeof(types_package.atPos)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛatPos;
[GoMethodHost(typeof(types_package.block)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛblock;
[GoMethodHost(typeof(types_package.comparer)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛcomparer;
[GoMethodHost(typeof(types_package.constDecl)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛconstDecl;
[GoMethodHost(typeof(types_package.cycleFinder)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛcycleFinder;
[GoMethodHost(typeof(types_package.declInfo)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛdeclInfo;
[GoMethodHost(typeof(types_package.environment)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛenvironment;
[GoMethodHost(typeof(types_package.error_)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛerror_;
[GoMethodHost(typeof(types_package.funcDecl)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛfuncDecl;
[GoMethodHost(typeof(types_package.gcSizes)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛgcSizes;
[GoMethodHost(typeof(types_package.goVersion)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛgoVersion;
[GoMethodHost(typeof(types_package.graphNode)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛgraphNode;
[GoMethodHost(typeof(types_package.ifacePair)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛifacePair;
[GoMethodHost(typeof(types_package.importDecl)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛimportDecl;
[GoMethodHost(typeof(types_package.indexedExpr)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛindexedExpr;
[GoMethodHost(typeof(types_package.instanceLookup)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛinstanceLookup;
[GoMethodHost(typeof(types_package.lazyObject)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛlazyObject;
[GoMethodHost(typeof(types_package.methodSet)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛmethodSet;
[GoMethodHost(typeof(types_package.monoGraph)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛmonoGraph;
[GoMethodHost(typeof(types_package.nodeQueue)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛnodeQueue;
[GoMethodHost(typeof(types_package.nodeSet)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛnodeSet;
[GoMethodHost(typeof(types_package.@object)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛobject;
[GoMethodHost(typeof(types_package.objset)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛobjset;
[GoMethodHost(typeof(types_package.operand)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛoperand;
[GoMethodHost(typeof(types_package.posSpan)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛposSpan;
[GoMethodHost(typeof(types_package.substMap)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛsubstMap;
[GoMethodHost(typeof(types_package.subster)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛsubster;
[GoMethodHost(typeof(types_package.term)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛterm;
[GoMethodHost(typeof(types_package.termlist)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛtermlist;
[GoMethodHost(typeof(types_package.tpWalker)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛtpWalker;
[GoMethodHost(typeof(types_package.typeDecl)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛtypeDecl;
[GoMethodHost(typeof(types_package.typeError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛtypeError;
[GoMethodHost(typeof(types_package.typeParamsById)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛtypeParamsById;
[GoMethodHost(typeof(types_package.typeWriter)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛtypeWriter;
[GoMethodHost(typeof(types_package.unifier)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛunifier;
[GoMethodHost(typeof(types_package.unifyMode)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛunifyMode;
[GoMethodHost(typeof(types_package.varDecl)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class typesᴛvarDecl;
// </MethodHolders>
