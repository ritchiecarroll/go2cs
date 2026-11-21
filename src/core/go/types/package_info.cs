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
[assembly: GoPositionMap("go/types/sizes.go", "sizes.cs", "ADdq0oLKyKQABxDcgoKCpgAIDIK2guaklIKmgpSClAAIBoKCgpSCzIKCgoKUgqaCgoKClKbGABEgoqSCgoKCpoLGgoKmgoKUgqaCgoKmhJKClKSkgoKUgoKCgpTIgsSk2AAUNuKUgILWgILGyLSCgoKUgpSmooKUgoKUlIKmAAIQ0oKCgoKClIKClJSqooKClMyygg==", "54-56:1", "", "148=Pkg/1/3/1,Pkg/2/3/2,Path/1/2/2,Pkg/3/3/3,Path/2/2/3", "", "62:64,63:13")]
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

// <GlyphForwarderUsings> (scratch prototype: the using directives the forwarders' signatures read)
using token = global::go.go.token_package;
using global::go.go;
using static types_package;
using ast = global::go.go.ast_package;
using constant = global::go.go.constant_package;
using static global::go.@internal.types.errors_package;
using errors = global::go.@internal.types.errors_package;
using iter = iter_package;
using ꓸꓸꓸastꓸExpr = Span<global::go.go.ast_package.Expr>;
using types = types_package;
using global::go.@internal;
using global::go.sync;
using io = io_package;
using ꓸꓸꓸany = Span<any>;
using cmp = cmp_package;
using container;
using global::go.unicode;
using path;
// </GlyphForwarderUsings>

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

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies in the package class)
partial class typesᴛAlias {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<TypeName> Obj(this ref Alias a) => types_package.Objᴍ(ref a);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ж<Alias> Ꮡa) => types_package.Stringᴍ(Ꮡa);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Type Underlying(this ж<Alias> Ꮡa) => types_package.Underlyingᴍ(Ꮡa);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Alias> Origin(this ref Alias a) => types_package.Originᴍ(ref a);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<TypeParamList> TypeParams(this ref Alias a) => types_package.TypeParamsᴍ(ref a);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void SetTypeParams(this ref Alias a, slice<ж<TypeParam>> tparams) => types_package.SetTypeParamsᴍ(ref a, tparams);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<TypeList> TypeArgs(this ref Alias a) => types_package.TypeArgsᴍ(ref a);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Type Rhs(this ref Alias a) => types_package.Rhsᴍ(ref a);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void cleanup(this ж<Alias> Ꮡa) => types_package.cleanupᴍ(Ꮡa);
}
partial class typesᴛChecker {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Alias> newAlias(this ж<Checker> Ꮡcheck, ж<TypeName> Ꮡobj, Type rhs) => types_package.newAliasᴍ(Ꮡcheck, Ꮡobj, rhs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Alias> newAliasInstance(this ж<Checker> Ꮡcheck, token.Pos pos, ж<Alias> Ꮡorig, slice<Type> targs, ж<Named> expanding, ж<Context> ctxt) => types_package.newAliasInstanceᴍ(Ꮡcheck, pos, Ꮡorig, targs, expanding, ctxt);
}
partial class typesᴛError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Error(this Error err) => types_package.Errorᴍ(err);
}
partial class typesᴛArgumentError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Error(this ref ArgumentError e) => types_package.Errorᴍ(ref e);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Unwrap(this ref ArgumentError e) => types_package.Unwrapᴍ(ref e);
}
partial class typesᴛInfo {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool recordTypes(this ref Info info) => types_package.recordTypesᴍ(ref info);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Type TypeOf(this ref Info info, ast.Expr e) => types_package.TypeOfᴍ(ref info, e);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Object ObjectOf(this ref Info info, ж<ast.Ident> id) => types_package.ObjectOfᴍ(ref info, id);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<PkgName> PkgNameOf(this ref Info info, ж<ast.ImportSpec> Ꮡimp) => types_package.PkgNameOfᴍ(ref info, Ꮡimp);
}
partial class typesᴛTypeAndValue {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool IsVoid(this TypeAndValue tv) => types_package.IsVoidᴍ(tv);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool IsType(this TypeAndValue tv) => types_package.IsTypeᴍ(tv);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool IsBuiltin(this TypeAndValue tv) => types_package.IsBuiltinᴍ(tv);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool IsValue(this TypeAndValue tv) => types_package.IsValueᴍ(tv);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool IsNil(this TypeAndValue tv) => types_package.IsNilᴍ(tv);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Addressable(this TypeAndValue tv) => types_package.Addressableᴍ(tv);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Assignable(this TypeAndValue tv) => types_package.Assignableᴍ(tv);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool HasOk(this TypeAndValue tv) => types_package.HasOkᴍ(tv);
}
partial class typesᴛInitializer {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ref Initializer init) => types_package.Stringᴍ(ref init);
}
partial class typesᴛConfig {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<Package>, error) Check(this ж<Config> Ꮡconf, @string path, ж<token.FileSet> fset, slice<ж<ast.File>> files, ж<Info> info) => types_package.Checkᴍ(Ꮡconf, path, fset, files, info);
}
partial class typesᴛArray {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static int64 Len(this ref Array a) => types_package.Lenᴍ(ref a);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Type Elem(this ref Array a) => types_package.Elemᴍ(ref a);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Type Underlying(this ж<Array> Ꮡa) => types_package.Underlyingᴍ(Ꮡa);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ж<Array> Ꮡa) => types_package.Stringᴍ(Ꮡa);
}
partial class typesᴛChecker {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void assignment(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, Type T, @string context) => types_package.assignmentᴍ(Ꮡcheck, Ꮡx, T, context);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void initConst(this ж<Checker> Ꮡcheck, ж<Const> Ꮡlhs, ж<operand> Ꮡx) => types_package.initConstᴍ(Ꮡcheck, Ꮡlhs, Ꮡx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void initVar(this ж<Checker> Ꮡcheck, ж<Var> Ꮡlhs, ж<operand> Ꮡx, @string context) => types_package.initVarᴍ(Ꮡcheck, Ꮡlhs, Ꮡx, context);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Type lhsVar(this ж<Checker> Ꮡcheck, ast.Expr lhs) => types_package.lhsVarᴍ(Ꮡcheck, lhs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void assignVar(this ж<Checker> Ꮡcheck, ast.Expr lhs, ast.Expr rhs, ж<operand> Ꮡx, @string context) => types_package.assignVarᴍ(Ꮡcheck, lhs, rhs, Ꮡx, context);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string typesSummary(this ж<Checker> Ꮡcheck, slice<Type> list, bool variadic, bool hasDots) => types_package.typesSummaryᴍ(Ꮡcheck, list, variadic, hasDots);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void assignError(this ж<Checker> Ꮡcheck, slice<ast.Expr> rhs, nint l, nint r) => types_package.assignErrorᴍ(Ꮡcheck, rhs, l, r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void returnError(this ж<Checker> Ꮡcheck, positioner at, slice<ж<Var>> lhs, slice<ж<operand>> rhs) => types_package.returnErrorᴍ(Ꮡcheck, at, lhs, rhs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void initVars(this ж<Checker> Ꮡcheck, slice<ж<Var>> lhs, slice<ast.Expr> orig_rhs, ast.Stmt returnStmt) => types_package.initVarsᴍ(Ꮡcheck, lhs, orig_rhs, returnStmt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void assignVars(this ж<Checker> Ꮡcheck, slice<ast.Expr> lhs, slice<ast.Expr> orig_rhs) => types_package.assignVarsᴍ(Ꮡcheck, lhs, orig_rhs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void shortVarDecl(this ж<Checker> Ꮡcheck, positioner pos, slice<ast.Expr> lhs, slice<ast.Expr> rhs) => types_package.shortVarDeclᴍ(Ꮡcheck, pos, lhs, rhs);
}
partial class typesᴛBasic {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static BasicKind Kind(this ref Basic b) => types_package.Kindᴍ(ref b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static BasicInfo Info(this ref Basic b) => types_package.Infoᴍ(ref b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Name(this ref Basic b) => types_package.Nameᴍ(ref b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Type Underlying(this ж<Basic> Ꮡb) => types_package.Underlyingᴍ(Ꮡb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ж<Basic> Ꮡb) => types_package.Stringᴍ(Ꮡb);
}
partial class typesᴛChecker {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool /*_*/ Δbuiltin(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, ж<ast.CallExpr> Ꮡcall, builtinId id) => types_package.Δbuiltinᴍ(Ꮡcheck, Ꮡx, Ꮡcall, id);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Type applyTypeFunc(this ж<Checker> Ꮡcheck, Func<Type, Type> f, ж<operand> Ꮡx, builtinId id) => types_package.applyTypeFuncᴍ(Ꮡcheck, f, Ꮡx, id);
}
partial class typesᴛChecker {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<Type> funcInst(this ж<Checker> Ꮡcheck, ж<target> ᏑT, token.Pos pos, ж<operand> Ꮡx, ж<indexedExpr> Ꮡix, bool infer) => types_package.funcInstᴍ(Ꮡcheck, ᏑT, pos, Ꮡx, Ꮡix, infer);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Signature> /*res*/ instantiateSignature(this ж<Checker> Ꮡcheck, token.Pos pos, ast.Expr expr, ж<Signature> Ꮡtyp, slice<Type> targs, slice<ast.Expr> xlist) => types_package.instantiateSignatureᴍ(Ꮡcheck, pos, expr, Ꮡtyp, targs, xlist);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static exprKind callExpr(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, ж<ast.CallExpr> Ꮡcall) => types_package.callExprᴍ(Ꮡcheck, Ꮡx, Ꮡcall);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<ж<operand>> /*xlist*/ exprList(this ж<Checker> Ꮡcheck, slice<ast.Expr> elist) => types_package.exprListᴍ(Ꮡcheck, elist);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (slice<ж<operand>> resList, slice<slice<Type>> targsList) genericExprList(this ж<Checker> Ꮡcheck, slice<ast.Expr> elist) => types_package.genericExprListᴍ(Ꮡcheck, elist);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Signature> /*rsig*/ arguments(this ж<Checker> Ꮡcheck, ж<ast.CallExpr> Ꮡcall, ж<Signature> Ꮡsig, slice<Type> targs, slice<ast.Expr> xlist, slice<ж<operand>> args, slice<slice<Type>> atargs) => types_package.argumentsᴍ(Ꮡcheck, Ꮡcall, Ꮡsig, targs, xlist, args, atargs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void selector(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, ж<ast.SelectorExpr> Ꮡe, bool wantType) => types_package.selectorᴍ(Ꮡcheck, Ꮡx, Ꮡe, wantType);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool use(this ж<Checker> Ꮡcheck, params ꓸꓸꓸastꓸExpr argsʗp) => types_package.useᴍ(Ꮡcheck, argsʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool useLHS(this ж<Checker> Ꮡcheck, params ꓸꓸꓸastꓸExpr argsʗp) => types_package.useLHSᴍ(Ꮡcheck, argsʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool useN(this ж<Checker> Ꮡcheck, slice<ast.Expr> args, bool lhs) => types_package.useNᴍ(Ꮡcheck, args, lhs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool use1(this ж<Checker> Ꮡcheck, ast.Expr e, bool lhs) => types_package.use1ᴍ(Ꮡcheck, e, lhs);
}
partial class typesᴛChan {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ChanDir Dir(this ref Chan c) => types_package.Dirᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Type Elem(this ref Chan c) => types_package.Elemᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Type Underlying(this ж<Chan> Ꮡc) => types_package.Underlyingᴍ(Ꮡc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ж<Chan> Ꮡc) => types_package.Stringᴍ(Ꮡc);
}
partial class typesᴛenvironment {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<Scope>, Object) lookupScope(this ref environment env, @string name) => types_package.lookupScopeᴍ(ref env, name);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Object lookup(this ref environment env, @string name) => types_package.lookupᴍ(ref env, name);
}
partial class typesᴛaction {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void describef(this ref action a, positioner pos, @string format, params ꓸꓸꓸany argsʗp) => types_package.describefᴍ(ref a, pos, format, argsʗp);
}
partial class typesᴛChecker {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void addDeclDep(this ref Checker check, Object to) => types_package.addDeclDepᴍ(ref check, to);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void brokenAlias(this ref Checker check, ж<TypeName> Ꮡalias) => types_package.brokenAliasᴍ(ref check, Ꮡalias);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void validAlias(this ref Checker check, ж<TypeName> Ꮡalias, Type typ) => types_package.validAliasᴍ(ref check, Ꮡalias, typ);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool isBrokenAlias(this ref Checker check, ж<TypeName> alias) => types_package.isBrokenAliasᴍ(ref check, alias);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void rememberUntyped(this ref Checker check, ast.Expr e, bool lhs, operandMode mode, ж<Basic> typ, constant.Value val) => types_package.rememberUntypedᴍ(ref check, e, lhs, mode, typ, val);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<action> later(this ref Checker check, Action f) => types_package.laterᴍ(ref check, f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void push(this ref Checker check, Object obj) => types_package.pushᴍ(ref check, obj);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void pop(this ref Checker check) => types_package.popᴍ(ref check);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void needsCleanup(this ref Checker check, cleaner c) => types_package.needsCleanupᴍ(ref check, c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void initFiles(this ж<Checker> Ꮡcheck, slice<ж<ast.File>> files) => types_package.initFilesᴍ(Ꮡcheck, files);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void pushPos(this ref Checker check, positioner pos) => types_package.pushPosᴍ(ref check, pos);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void popPos(this ref Checker check) => types_package.popPosᴍ(ref check);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void handleBailout(this ж<Checker> Ꮡcheck, ж<error> Ꮡerr) => types_package.handleBailoutᴍ(Ꮡcheck, Ꮡerr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error /*err*/ Files(this ж<Checker> Ꮡcheck, slice<ж<ast.File>> files) => types_package.Filesᴍ(Ꮡcheck, files);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void checkFiles(this ж<Checker> Ꮡcheck, slice<ж<ast.File>> files) => types_package.checkFilesᴍ(Ꮡcheck, files);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void processDelayed(this ж<Checker> Ꮡcheck, nint top) => types_package.processDelayedᴍ(Ꮡcheck, top);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void cleanup(this ref Checker check) => types_package.cleanupᴍ(ref check);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void recordTypeAndValueInSyntax(this ref Checker check, ast.Expr x, operandMode mode, Type typ, constant.Value val) => types_package.recordTypeAndValueInSyntaxᴍ(ref check, x, mode, typ, val);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void recordCommaOkTypesInSyntax(this ref Checker check, ast.Expr x, Type t0, Type t1) => types_package.recordCommaOkTypesInSyntaxᴍ(ref check, x, t0, t1);
}
partial class typesᴛChecker {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void overflow(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, token.Pos opPos) => types_package.overflowᴍ(Ꮡcheck, Ꮡx, opPos);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void representable(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, ж<Basic> typ) => types_package.representableᴍ(Ꮡcheck, Ꮡx, typ);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (constant.Value, errors.Code) representation(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, ж<Basic> Ꮡtyp) => types_package.representationᴍ(Ꮡcheck, Ꮡx, Ꮡtyp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void invalidConversion(this ж<Checker> Ꮡcheck, errors.Code code, ж<operand> x, Type target) => types_package.invalidConversionᴍ(Ꮡcheck, code, x, target);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void convertUntyped(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, Type target) => types_package.convertUntypedᴍ(Ꮡcheck, Ꮡx, target);
}
partial class typesᴛContext {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string instanceHash(this ж<Context> Ꮡctxt, Type orig, slice<Type> targs) => types_package.instanceHashᴍ(Ꮡctxt, orig, targs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Type lookup(this ж<Context> Ꮡctxt, @string h, Type orig, slice<Type> targs) => types_package.lookupᴍ(Ꮡctxt, h, orig, targs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Type update(this ж<Context> Ꮡctxt, @string h, Type orig, slice<Type> targs, Type inst) => types_package.updateᴍ(Ꮡctxt, h, orig, targs, inst);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint getID(this ж<Context> Ꮡctxt, Type t) => types_package.getIDᴍ(Ꮡctxt, t);
}
partial class typesᴛChecker {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void conversion(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, Type T) => types_package.conversionᴍ(Ꮡcheck, Ꮡx, T);
}
partial class typesᴛoperand {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool convertibleTo(this ж<operand> Ꮡx, ж<Checker> Ꮡcheck, Type T, ж<@string> Ꮡcause) => types_package.convertibleToᴍ(Ꮡx, Ꮡcheck, T, Ꮡcause);
}
partial class typesᴛChecker {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void directCycles(this ж<Checker> Ꮡcheck) => types_package.directCyclesᴍ(Ꮡcheck);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void directCycle(this ж<Checker> Ꮡcheck, ж<TypeName> Ꮡtname, map<ж<TypeName>, nint> pathIdx) => types_package.directCycleᴍ(Ꮡcheck, Ꮡtname, pathIdx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool finiteSize(this ж<Checker> Ꮡcheck, Type t) => types_package.finiteSizeᴍ(Ꮡcheck, t);
}
partial class typesᴛChecker {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void declare(this ж<Checker> Ꮡcheck, ж<Scope> scope, ж<ast.Ident> id, Object obj, token.Pos pos) => types_package.declareᴍ(Ꮡcheck, scope, id, obj, pos);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void objDecl(this ж<Checker> Ꮡcheck, Object obj) => types_package.objDeclᴍ(Ꮡcheck, obj);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool /*valid*/ validCycle(this ж<Checker> Ꮡcheck, Object obj) => types_package.validCycleᴍ(Ꮡcheck, obj);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void cycleError(this ж<Checker> Ꮡcheck, slice<Object> cycle, nint start) => types_package.cycleErrorᴍ(Ꮡcheck, cycle, start);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void walkDecls(this ж<Checker> Ꮡcheck, slice<ast.Decl> decls, Action<decl> f) => types_package.walkDeclsᴍ(Ꮡcheck, decls, f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void walkDecl(this ж<Checker> Ꮡcheck, ast.Decl d, Action<decl> f) => types_package.walkDeclᴍ(Ꮡcheck, d, f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void constDecl(this ж<Checker> Ꮡcheck, ж<Const> Ꮡobj, ast.Expr typ, ast.Expr init, bool inherited) => types_package.constDeclᴍ(Ꮡcheck, Ꮡobj, typ, init, inherited);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void varDecl(this ж<Checker> Ꮡcheck, ж<Var> Ꮡobj, slice<ж<Var>> lhs, ast.Expr typ, ast.Expr init) => types_package.varDeclᴍ(Ꮡcheck, Ꮡobj, lhs, typ, init);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool isImportedConstraint(this ref Checker check, Type typ) => types_package.isImportedConstraintᴍ(ref check, typ);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void typeDecl(this ж<Checker> Ꮡcheck, ж<TypeName> Ꮡobj, ж<ast.TypeSpec> Ꮡtdecl) => types_package.typeDeclᴍ(Ꮡcheck, Ꮡobj, Ꮡtdecl);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void collectTypeParams(this ж<Checker> Ꮡcheck, ж<ж<TypeParamList>> Ꮡdst, ж<ast.FieldList> Ꮡlist) => types_package.collectTypeParamsᴍ(Ꮡcheck, Ꮡdst, Ꮡlist);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Type bound(this ж<Checker> Ꮡcheck, ast.Expr x) => types_package.boundᴍ(Ꮡcheck, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<TypeParam> declareTypeParam(this ж<Checker> Ꮡcheck, ж<ast.Ident> Ꮡname, token.Pos scopePos) => types_package.declareTypeParamᴍ(Ꮡcheck, Ꮡname, scopePos);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void collectMethods(this ж<Checker> Ꮡcheck, ж<TypeName> Ꮡobj) => types_package.collectMethodsᴍ(Ꮡcheck, Ꮡobj);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void checkFieldUniqueness(this ж<Checker> Ꮡcheck, ж<Named> @base) => types_package.checkFieldUniquenessᴍ(Ꮡcheck, @base);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void funcDecl(this ж<Checker> Ꮡcheck, ж<Func> Ꮡobj, ж<declInfo> Ꮡdecl) => types_package.funcDeclᴍ(Ꮡcheck, Ꮡobj, Ꮡdecl);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void declStmt(this ж<Checker> Ꮡcheck, ast.Decl d) => types_package.declStmtᴍ(Ꮡcheck, d);
}
partial class typesᴛimportDecl {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ast.Node node(this importDecl d) => types_package.nodeᴍ(d);
}
partial class typesᴛconstDecl {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ast.Node node(this constDecl d) => types_package.nodeᴍ(d);
}
partial class typesᴛvarDecl {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ast.Node node(this varDecl d) => types_package.nodeᴍ(d);
}
partial class typesᴛtypeDecl {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ast.Node node(this typeDecl d) => types_package.nodeᴍ(d);
}
partial class typesᴛfuncDecl {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ast.Node node(this funcDecl d) => types_package.nodeᴍ(d);
}
partial class typesᴛChecker {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<error_> newError(this ж<Checker> Ꮡcheck, errors.Code code) => types_package.newErrorᴍ(Ꮡcheck, code);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void handleError(this ref Checker check, nint index, positioner posn, errors.Code code, @string msg, bool soft) => types_package.handleErrorᴍ(ref check, index, posn, code, msg, soft);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void error(this ж<Checker> Ꮡcheck, positioner at, errors.Code code, @string msg) => types_package.errorᴍ(Ꮡcheck, at, code, msg);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void errorf(this ж<Checker> Ꮡcheck, positioner at, errors.Code code, @string format, params ꓸꓸꓸany argsʗp) => types_package.errorfᴍ(Ꮡcheck, at, code, format, argsʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void softErrorf(this ж<Checker> Ꮡcheck, positioner at, errors.Code code, @string format, params ꓸꓸꓸany argsʗp) => types_package.softErrorfᴍ(Ꮡcheck, at, code, format, argsʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void versionErrorf(this ж<Checker> Ꮡcheck, positioner at, goVersion v, @string format, params ꓸꓸꓸany argsʗp) => types_package.versionErrorfᴍ(Ꮡcheck, at, v, format, argsʗp);
}
partial class typesᴛerror_ {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void addf(this ref error_ err, positioner at, @string format, params ꓸꓸꓸany argsʗp) => types_package.addfᴍ(ref err, at, format, argsʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void addAltDecl(this ref error_ err, Object obj) => types_package.addAltDeclᴍ(ref err, obj);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool empty(this ref error_ err) => types_package.emptyᴍ(ref err);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static positioner posn(this ref error_ err) => types_package.posnᴍ(ref err);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string msg(this ref error_ err) => types_package.msgᴍ(ref err);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void report(this ref error_ err) => types_package.reportᴍ(ref err);
}
partial class typesᴛatPos {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static token.Pos Pos(this atPos s) => types_package.Posᴍ(s);
}
partial class typesᴛposSpan {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static token.Pos Pos(this posSpan e) => types_package.Posᴍ(e);
}
partial class typesᴛChecker {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string lookupError(this ж<Checker> Ꮡcheck, Type typ, @string sel, Object obj, bool structLit) => types_package.lookupErrorᴍ(Ꮡcheck, typ, sel, obj, structLit);
}
partial class typesᴛChecker {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool op(this ж<Checker> Ꮡcheck, opPredicates m, ж<operand> Ꮡx, token.Token op) => types_package.opᴍ(Ꮡcheck, m, Ꮡx, op);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void unary(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, ж<ast.UnaryExpr> Ꮡe) => types_package.unaryᴍ(Ꮡcheck, Ꮡx, Ꮡe);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Type chanElem(this ж<Checker> Ꮡcheck, positioner pos, ж<operand> Ꮡx, bool recv) => types_package.chanElemᴍ(Ꮡcheck, pos, Ꮡx, recv);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void updateExprType(this ж<Checker> Ꮡcheck, ast.Expr x, Type typ, bool final) => types_package.updateExprTypeᴍ(Ꮡcheck, x, typ, final);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void updateExprVal(this ref Checker check, ast.Expr x, constant.Value val) => types_package.updateExprValᴍ(ref check, x, val);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (Type, constant.Value, errors.Code) implicitTypeAndValue(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, Type target) => types_package.implicitTypeAndValueᴍ(Ꮡcheck, Ꮡx, target);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void comparison(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, ж<operand> Ꮡy, token.Token op, bool switchCase) => types_package.comparisonᴍ(Ꮡcheck, Ꮡx, Ꮡy, op, switchCase);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string incomparableCause(this ж<Checker> Ꮡcheck, Type typ) => types_package.incomparableCauseᴍ(Ꮡcheck, typ);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void shift(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, ж<operand> Ꮡy, ast.Expr e, token.Token op) => types_package.shiftᴍ(Ꮡcheck, Ꮡx, Ꮡy, e, op);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void binary(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, ast.Expr e, ast.Expr lhs, ast.Expr rhs, token.Token op, token.Pos opPos) => types_package.binaryᴍ(Ꮡcheck, Ꮡx, e, lhs, rhs, op, opPos);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void matchTypes(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, ж<operand> Ꮡy) => types_package.matchTypesᴍ(Ꮡcheck, Ꮡx, Ꮡy);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static exprKind rawExpr(this ж<Checker> Ꮡcheck, ж<target> T, ж<operand> x, ast.Expr e, Type hint, bool allowGeneric) => types_package.rawExprᴍ(Ꮡcheck, T, x, e, hint, allowGeneric);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void nonGeneric(this ж<Checker> Ꮡcheck, ж<target> T, ж<operand> Ꮡx) => types_package.nonGenericᴍ(Ꮡcheck, T, Ꮡx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void pendingType(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx) => types_package.pendingTypeᴍ(Ꮡcheck, Ꮡx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static exprKind exprInternal(this ж<Checker> Ꮡcheck, ж<target> ᏑT, ж<operand> Ꮡx, ast.Expr e, Type hint) => types_package.exprInternalᴍ(Ꮡcheck, ᏑT, Ꮡx, e, hint);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void typeAssertion(this ж<Checker> Ꮡcheck, ast.Expr e, ж<operand> Ꮡx, Type T, bool typeSwitch) => types_package.typeAssertionᴍ(Ꮡcheck, e, Ꮡx, T, typeSwitch);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void expr(this ж<Checker> Ꮡcheck, ж<target> T, ж<operand> x, ast.Expr e) => types_package.exprᴍ(Ꮡcheck, T, x, e);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void genericExpr(this ж<Checker> Ꮡcheck, ж<operand> x, ast.Expr e) => types_package.genericExprᴍ(Ꮡcheck, x, e);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (slice<ж<operand>> list, bool commaOk) multiExpr(this ж<Checker> Ꮡcheck, ast.Expr e, bool allowCommaOk) => types_package.multiExprᴍ(Ꮡcheck, e, allowCommaOk);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void exprWithHint(this ж<Checker> Ꮡcheck, ж<operand> x, ast.Expr e, Type hint) => types_package.exprWithHintᴍ(Ꮡcheck, x, e, hint);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void exprOrType(this ж<Checker> Ꮡcheck, ж<operand> x, ast.Expr e, bool allowGeneric) => types_package.exprOrTypeᴍ(Ꮡcheck, x, e, allowGeneric);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void exclude(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, nuint modeset) => types_package.excludeᴍ(Ꮡcheck, Ꮡx, modeset);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void singleValue(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx) => types_package.singleValueᴍ(Ꮡcheck, Ꮡx);
}
partial class typesᴛChecker {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string sprintf(this ж<Checker> Ꮡcheck, @string format, params ꓸꓸꓸany argsʗp) => types_package.sprintfᴍ(Ꮡcheck, format, argsʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void trace(this ж<Checker> Ꮡcheck, token.Pos pos, @string format, params ꓸꓸꓸany argsʗp) => types_package.traceᴍ(Ꮡcheck, pos, format, argsʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void dump(this ж<Checker> Ꮡcheck, @string format, params ꓸꓸꓸany argsʗp) => types_package.dumpᴍ(Ꮡcheck, format, argsʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string qualifier(this ref Checker check, ж<Package> Ꮡpkg) => types_package.qualifierᴍ(ref check, Ꮡpkg);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void markImports(this ref Checker check, ж<Package> Ꮡpkg) => types_package.markImportsᴍ(ref check, Ꮡpkg);
}
partial class typesᴛgcSizes {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static int64 /*result*/ Alignof(this ж<gcSizes> Ꮡs, Type T) => types_package.Alignofᴍ(Ꮡs, T);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<int64> Offsetsof(this ж<gcSizes> Ꮡs, slice<ж<Var>> fields) => types_package.Offsetsofᴍ(Ꮡs, fields);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static int64 Sizeof(this ж<gcSizes> Ꮡs, Type T) => types_package.Sizeofᴍ(Ꮡs, T);
}
partial class typesᴛChecker {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool /*isFuncInst*/ indexExpr(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, ж<indexedExpr> Ꮡe) => types_package.indexExprᴍ(Ꮡcheck, Ꮡx, Ꮡe);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void sliceExpr(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, ж<ast.SliceExpr> Ꮡe) => types_package.sliceExprᴍ(Ꮡcheck, Ꮡx, Ꮡe);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ast.Expr singleIndex(this ж<Checker> Ꮡcheck, ж<indexedExpr> Ꮡexpr) => types_package.singleIndexᴍ(Ꮡcheck, Ꮡexpr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (Type typ, int64 val) index(this ж<Checker> Ꮡcheck, ast.Expr index, int64 max) => types_package.indexᴍ(Ꮡcheck, index, max);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool isValidIndex(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, errors.Code code, @string what, bool allowNegative) => types_package.isValidIndexᴍ(Ꮡcheck, Ꮡx, code, what, allowNegative);
}
partial class typesᴛindexedExpr {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static token.Pos Pos(this ref indexedExpr x) => types_package.Posᴍ(ref x);
}
partial class typesᴛChecker {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<Type> /*inferred*/ infer(this ж<Checker> Ꮡcheck, positioner posn, slice<ж<TypeParam>> tparams, slice<Type> targs, ж<Tuple> Ꮡparams, slice<ж<operand>> args, bool reverse, ж<error_> Ꮡerr) => types_package.inferᴍ(Ꮡcheck, posn, tparams, targs, Ꮡparams, args, reverse, Ꮡerr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (slice<ж<TypeParam>>, Type) renameTParams(this ж<Checker> Ꮡcheck, token.Pos pos, slice<ж<TypeParam>> tparams, Type typ) => types_package.renameTParamsᴍ(Ꮡcheck, pos, tparams, typ);
}
partial class typesᴛtpWalker {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool /*res*/ isParameterized(this ж<tpWalker> Ꮡw, Type typ) => types_package.isParameterizedᴍ(Ꮡw, typ);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool varList(this ж<tpWalker> Ꮡw, slice<ж<Var>> list) => types_package.varListᴍ(Ꮡw, list);
}
partial class typesᴛcycleFinder {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void typ(this ж<cycleFinder> Ꮡw, Type typ) => types_package.typᴍ(Ꮡw, typ);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void varList(this ж<cycleFinder> Ꮡw, slice<ж<Var>> list) => types_package.varListᴍ(Ꮡw, list);
}
partial class typesᴛChecker {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void initOrder(this ж<Checker> Ꮡcheck) => types_package.initOrderᴍ(Ꮡcheck);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void reportCycle(this ж<Checker> Ꮡcheck, slice<Object> cycle) => types_package.reportCycleᴍ(Ꮡcheck, cycle);
}
partial class typesᴛgraphNode {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint cost(this ref graphNode n) => types_package.costᴍ(ref n);
}
partial class typesᴛnodeSet {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void add(this ref nodeSet s, ж<graphNode> p) => types_package.addᴍ(ref s, p);
}
partial class typesᴛnodeQueue {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint Len(this nodeQueue a) => types_package.Lenᴍ(a);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void Swap(this nodeQueue a, nint i, nint j) => types_package.Swapᴍ(a, i, j);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool Less(this nodeQueue a, nint i, nint j) => types_package.Lessᴍ(a, i, j);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void Push(this ref nodeQueue a, any x) => types_package.Pushᴍ(ref a, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static any Pop(this ref nodeQueue a) => types_package.Popᴍ(ref a);
}
partial class typesᴛInterface {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<_TypeSet> typeSet(this ж<Interface> Ꮡt) => types_package.typeSetᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void MarkImplicit(this ref Interface t) => types_package.MarkImplicitᴍ(ref t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint NumExplicitMethods(this ref Interface t) => types_package.NumExplicitMethodsᴍ(ref t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Func> ExplicitMethod(this ref Interface t, nint i) => types_package.ExplicitMethodᴍ(ref t, i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint NumEmbeddeds(this ref Interface t) => types_package.NumEmbeddedsᴍ(ref t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Named> Embedded(this ref Interface t, nint i) => types_package.Embeddedᴍ(ref t, i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Type EmbeddedType(this ref Interface t, nint i) => types_package.EmbeddedTypeᴍ(ref t, i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint NumMethods(this ж<Interface> Ꮡt) => types_package.NumMethodsᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Func> Method(this ж<Interface> Ꮡt, nint i) => types_package.Methodᴍ(Ꮡt, i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Empty(this ж<Interface> Ꮡt) => types_package.Emptyᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool IsComparable(this ж<Interface> Ꮡt) => types_package.IsComparableᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool IsMethodSet(this ж<Interface> Ꮡt) => types_package.IsMethodSetᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool IsImplicit(this ref Interface t) => types_package.IsImplicitᴍ(ref t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Interface> Complete(this ж<Interface> Ꮡt) => types_package.Completeᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Type Underlying(this ж<Interface> Ꮡt) => types_package.Underlyingᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ж<Interface> Ꮡt) => types_package.Stringᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void cleanup(this ж<Interface> Ꮡt) => types_package.cleanupᴍ(Ꮡt);
}
partial class typesᴛChecker {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Interface> newInterface(this ж<Checker> Ꮡcheck) => types_package.newInterfaceᴍ(Ꮡcheck);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void interfaceType(this ж<Checker> Ꮡcheck, ж<Interface> Ꮡityp, ж<ast.InterfaceType> Ꮡiface, ж<TypeName> Ꮡdef) => types_package.interfaceTypeᴍ(Ꮡcheck, Ꮡityp, Ꮡiface, Ꮡdef);
}
partial class typesᴛInterface {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static iter.Seq<ж<Func>> Methods(this ж<Interface> Ꮡt) => types_package.Methodsᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static iter.Seq<ж<Func>> ExplicitMethods(this ж<Interface> Ꮡt) => types_package.ExplicitMethodsᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static iter.Seq<Type> EmbeddedTypes(this ж<Interface> Ꮡt) => types_package.EmbeddedTypesᴍ(Ꮡt);
}
partial class typesᴛNamed {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static iter.Seq<ж<Func>> Methods(this ж<Named> Ꮡt) => types_package.Methodsᴍ(Ꮡt);
}
partial class typesᴛScope {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static iter.Seq<ж<Scope>> Children(this ж<Scope> Ꮡs) => types_package.Childrenᴍ(Ꮡs);
}
partial class typesᴛStruct {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static iter.Seq<ж<Var>> Fields(this ж<Struct> Ꮡs) => types_package.Fieldsᴍ(Ꮡs);
}
partial class typesᴛTuple {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static iter.Seq<ж<Var>> Variables(this ж<Tuple> Ꮡt) => types_package.Variablesᴍ(Ꮡt);
}
partial class typesᴛMethodSet {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static iter.Seq<ж<Selection>> Methods(this ж<MethodSet> Ꮡs) => types_package.Methodsᴍ(Ꮡs);
}
partial class typesᴛUnion {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static iter.Seq<ж<Term>> Terms(this ж<Union> Ꮡu) => types_package.Termsᴍ(Ꮡu);
}
partial class typesᴛTypeParamList {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static iter.Seq<ж<TypeParam>> TypeParams(this ж<TypeParamList> Ꮡl) => types_package.TypeParamsᴍ(Ꮡl);
}
partial class typesᴛTypeList {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static iter.Seq<Type> Types(this ж<TypeList> Ꮡl) => types_package.Typesᴍ(Ꮡl);
}
partial class typesᴛChecker {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void labels(this ж<Checker> Ꮡcheck, ж<ast.BlockStmt> Ꮡbody) => types_package.labelsᴍ(Ꮡcheck, Ꮡbody);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<ж<ast.BranchStmt>> blockBranches(this ж<Checker> Ꮡcheck, ж<Scope> all, ж<block> parent, ж<ast.LabeledStmt> lstmt, slice<ast.Stmt> list) => types_package.blockBranchesᴍ(Ꮡcheck, all, parent, lstmt, list);
}
partial class typesᴛblock {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void insert(this ж<block> Ꮡb, ж<ast.LabeledStmt> Ꮡs) => types_package.insertᴍ(Ꮡb, Ꮡs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<ast.LabeledStmt> gotoTarget(this ж<block> Ꮡb, @string name) => types_package.gotoTargetᴍ(Ꮡb, name);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<ast.LabeledStmt> enclosingTarget(this ж<block> Ꮡb, @string name) => types_package.enclosingTargetᴍ(Ꮡb, name);
}
partial class typesᴛChecker {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void langCompat(this ж<Checker> Ꮡcheck, ж<ast.BasicLit> Ꮡlit) => types_package.langCompatᴍ(Ꮡcheck, Ꮡlit);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void basicLit(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, ж<ast.BasicLit> Ꮡe) => types_package.basicLitᴍ(Ꮡcheck, Ꮡx, Ꮡe);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void funcLit(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, ж<ast.FuncLit> Ꮡe) => types_package.funcLitᴍ(Ꮡcheck, Ꮡx, Ꮡe);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void compositeLit(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, ж<ast.CompositeLit> Ꮡe, Type hint) => types_package.compositeLitᴍ(Ꮡcheck, Ꮡx, Ꮡe, hint);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static int64 indexedElts(this ж<Checker> Ꮡcheck, slice<ast.Expr> elts, Type typ, int64 length) => types_package.indexedEltsᴍ(Ꮡcheck, elts, typ, length);
}
partial class typesᴛinstanceLookup {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Named> lookup(this ref instanceLookup l, ж<Named> inst) => types_package.lookupᴍ(ref l, inst);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void add(this ref instanceLookup l, ж<Named> inst) => types_package.addᴍ(ref l, inst);
}
partial class typesᴛChecker {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<Func> method, bool wrongType) missingMethod(this ж<Checker> Ꮡcheck, Type V, Type T, bool @static, Func<Type, Type, bool> equivalent, ж<@string> Ꮡcause) => types_package.missingMethodᴍ(Ꮡcheck, V, T, @static, equivalent, Ꮡcause);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool hasAllMethods(this ж<Checker> Ꮡcheck, Type V, Type T, bool @static, Func<Type, Type, bool> equivalent, ж<@string> cause) => types_package.hasAllMethodsᴍ(Ꮡcheck, V, T, @static, equivalent, cause);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string interfacePtrError(this ж<Checker> Ꮡcheck, Type T) => types_package.interfacePtrErrorᴍ(Ꮡcheck, T);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string funcString(this ж<Checker> Ꮡcheck, ж<Func> Ꮡf, bool pkgInfo) => types_package.funcStringᴍ(Ꮡcheck, Ꮡf, pkgInfo);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool assertableTo(this ж<Checker> Ꮡcheck, Type V, Type T, ж<@string> cause) => types_package.assertableToᴍ(Ꮡcheck, V, T, cause);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool newAssertableTo(this ж<Checker> Ꮡcheck, Type V, Type T, ж<@string> cause) => types_package.newAssertableToᴍ(Ꮡcheck, V, T, cause);
}
partial class typesᴛMap {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Type Key(this ref Map m) => types_package.Keyᴍ(ref m);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Type Elem(this ref Map m) => types_package.Elemᴍ(ref m);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Type Underlying(this ж<Map> Ꮡt) => types_package.Underlyingᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ж<Map> Ꮡt) => types_package.Stringᴍ(Ꮡt);
}
partial class typesᴛMethodSet {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ref MethodSet s) => types_package.Stringᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint Len(this ref MethodSet s) => types_package.Lenᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Selection> At(this ref MethodSet s, nint i) => types_package.Atᴍ(ref s, i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Selection> Lookup(this ж<MethodSet> Ꮡs, ж<Package> pkg, @string name) => types_package.Lookupᴍ(Ꮡs, pkg, name);
}
partial class typesᴛmethodSet {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static methodSet add(this methodSet s, slice<ж<Func>> list, slice<nint> index, bool indirect, bool multiples) => types_package.addᴍ(s, list, index, indirect, multiples);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static methodSet addOne(this methodSet s, ж<Func> Ꮡf, slice<nint> index, bool indirect, bool multiples) => types_package.addOneᴍ(s, Ꮡf, index, indirect, multiples);
}
partial class typesᴛChecker {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void monomorph(this ж<Checker> Ꮡcheck) => types_package.monomorphᴍ(Ꮡcheck);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void reportInstanceLoop(this ж<Checker> Ꮡcheck, nint v) => types_package.reportInstanceLoopᴍ(Ꮡcheck, v);
}
partial class typesᴛmonoGraph {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void recordCanon(this ref monoGraph w, ж<TypeParam> mpar, ж<TypeParam> tpar) => types_package.recordCanonᴍ(ref w, mpar, tpar);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void recordInstance(this ж<monoGraph> Ꮡw, ж<Package> pkg, token.Pos pos, slice<ж<TypeParam>> tparams, slice<Type> targs, slice<ast.Expr> xlist) => types_package.recordInstanceᴍ(Ꮡw, pkg, pos, tparams, targs, xlist);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void assign(this ж<monoGraph> Ꮡw, ж<Package> pkg, token.Pos pos, ж<TypeParam> Ꮡtpar, Type targ) => types_package.assignᴍ(Ꮡw, pkg, pos, Ꮡtpar, targ);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint localNamedVertex(this ref monoGraph w, ж<Package> pkg, ж<Named> Ꮡnamed) => types_package.localNamedVertexᴍ(ref w, pkg, Ꮡnamed);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint typeParamVertex(this ref monoGraph w, ж<TypeParam> Ꮡtpar) => types_package.typeParamVertexᴍ(ref w, Ꮡtpar);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void addEdge(this ref monoGraph w, nint dst, nint src, nint weight, token.Pos pos, Type typ) => types_package.addEdgeᴍ(ref w, dst, src, weight, pos, typ);
}
partial class typesᴛNamed {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Named> unpack(this ж<Named> Ꮡn) => types_package.unpackᴍ(Ꮡn);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool stateHas(this ж<Named> Ꮡn, stateMask m) => types_package.stateHasᴍ(Ꮡn, m);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void setState(this ж<Named> Ꮡn, stateMask m) => types_package.setStateᴍ(Ꮡn, m);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void cleanup(this ж<Named> Ꮡn) => types_package.cleanupᴍ(Ꮡn);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<TypeName> Obj(this ref Named t) => types_package.Objᴍ(ref t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Named> Origin(this ж<Named> Ꮡt) => types_package.Originᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<TypeParamList> TypeParams(this ж<Named> Ꮡt) => types_package.TypeParamsᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void SetTypeParams(this ж<Named> Ꮡt, slice<ж<TypeParam>> tparams) => types_package.SetTypeParamsᴍ(Ꮡt, tparams);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<TypeList> TypeArgs(this ref Named t) => types_package.TypeArgsᴍ(ref t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint NumMethods(this ж<Named> Ꮡt) => types_package.NumMethodsᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Func> Method(this ж<Named> Ꮡt, nint i) => types_package.Methodᴍ(Ꮡt, i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Func> expandMethod(this ж<Named> Ꮡt, nint i) => types_package.expandMethodᴍ(Ꮡt, i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void SetUnderlying(this ж<Named> Ꮡt, Type u) => types_package.SetUnderlyingᴍ(Ꮡt, u);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void AddMethod(this ж<Named> Ꮡt, ж<Func> Ꮡm) => types_package.AddMethodᴍ(Ꮡt, Ꮡm);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint methodIndex(this ref Named t, @string name, bool foldCase) => types_package.methodIndexᴍ(ref t, name, foldCase);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Type rhs(this ж<Named> Ꮡn) => types_package.rhsᴍ(Ꮡn);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Type Underlying(this ж<Named> Ꮡn) => types_package.Underlyingᴍ(Ꮡn);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ж<Named> Ꮡt) => types_package.Stringᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void resolveUnderlying(this ж<Named> Ꮡn) => types_package.resolveUnderlyingᴍ(Ꮡn);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint, ж<Func>) lookupMethod(this ж<Named> Ꮡn, ж<Package> Ꮡpkg, @string name, bool foldCase) => types_package.lookupMethodᴍ(Ꮡn, Ꮡpkg, name, foldCase);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Type /*rhs*/ expandRHS(this ж<Named> Ꮡn) => types_package.expandRHSᴍ(Ꮡn);
}
partial class typesᴛChecker {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Named> newNamed(this ж<Checker> Ꮡcheck, ж<TypeName> Ꮡobj, Type fromRHS, slice<ж<Func>> methods) => types_package.newNamedᴍ(Ꮡcheck, Ꮡobj, fromRHS, methods);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Named> newNamedInstance(this ж<Checker> Ꮡcheck, token.Pos pos, ж<Named> Ꮡorig, slice<Type> targs, ж<Named> Ꮡexpanding) => types_package.newNamedInstanceᴍ(Ꮡcheck, pos, Ꮡorig, targs, Ꮡexpanding);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Context> context(this ref Checker check) => types_package.contextᴍ(ref check);
}
partial class typesᴛobject {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Scope> Parent(this ref @object obj) => types_package.Parentᴍ(ref obj);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static token.Pos Pos(this ref @object obj) => types_package.Posᴍ(ref obj);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Package> Pkg(this ref @object obj) => types_package.Pkgᴍ(ref obj);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string Name(this ref @object obj) => types_package.Nameᴍ(ref obj);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Type Type(this ref @object obj) => types_package.Typeᴍ(ref obj);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool Exported(this ref @object obj) => types_package.Exportedᴍ(ref obj);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string Id(this ref @object obj) => types_package.Idᴍ(ref obj);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string String(this ref @object obj) => types_package.Stringᴍ(ref obj);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint32 order(this ref @object obj) => types_package.orderᴍ(ref obj);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static token.Pos scopePos(this ref @object obj) => types_package.scopePosᴍ(ref obj);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void setParent(this ref @object obj, ж<Scope> Ꮡparent) => types_package.setParentᴍ(ref obj, Ꮡparent);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void setType(this ref @object obj, Type typ) => types_package.setTypeᴍ(ref obj, typ);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void setOrder(this ref @object obj, uint32 order) => types_package.setOrderᴍ(ref obj, order);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void setScopePos(this ref @object obj, token.Pos pos) => types_package.setScopePosᴍ(ref obj, pos);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool sameId(this ref @object obj, ж<Package> Ꮡpkg, @string name, bool foldCase) => types_package.sameIdᴍ(ref obj, Ꮡpkg, name, foldCase);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint cmp(this ж<@object> Ꮡa, ж<@object> Ꮡb) => types_package.cmpᴍ(Ꮡa, Ꮡb);
}
partial class typesᴛPkgName {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Package> Imported(this ref PkgName obj) => types_package.Importedᴍ(ref obj);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ж<PkgName> Ꮡobj) => types_package.Stringᴍ(Ꮡobj);
}
partial class typesᴛConst {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static constant.Value Val(this ref Const obj) => types_package.Valᴍ(ref obj);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void isDependency(this ref Const _) => types_package.isDependencyᴍ(ref _);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ж<Const> Ꮡobj) => types_package.Stringᴍ(Ꮡobj);
}
partial class typesᴛTypeName {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool IsAlias(this ж<TypeName> Ꮡobj) => types_package.IsAliasᴍ(Ꮡobj);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ж<TypeName> Ꮡobj) => types_package.Stringᴍ(Ꮡobj);
}
partial class typesᴛVarKind {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this VarKind kind) => types_package.Stringᴍ(kind);
}
partial class typesᴛVar {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static VarKind Kind(this ref Var v) => types_package.Kindᴍ(ref v);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void SetKind(this ref Var v, VarKind kind) => types_package.SetKindᴍ(ref v, kind);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Anonymous(this ref Var obj) => types_package.Anonymousᴍ(ref obj);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Embedded(this ref Var obj) => types_package.Embeddedᴍ(ref obj);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool IsField(this ref Var obj) => types_package.IsFieldᴍ(ref obj);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Var> Origin(this ж<Var> Ꮡobj) => types_package.Originᴍ(Ꮡobj);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void isDependency(this ref Var _) => types_package.isDependencyᴍ(ref _);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ж<Var> Ꮡobj) => types_package.Stringᴍ(Ꮡobj);
}
partial class typesᴛFunc {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Signature> Signature(this ref Func obj) => types_package.Signatureᴍ(ref obj);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string FullName(this ж<Func> Ꮡobj) => types_package.FullNameᴍ(Ꮡobj);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Scope> Scope(this ref Func obj) => types_package.Scopeᴍ(ref obj);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Func> Origin(this ж<Func> Ꮡobj) => types_package.Originᴍ(Ꮡobj);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Package> Pkg(this ref Func obj) => types_package.Pkgᴍ(ref obj);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool hasPtrRecv(this ref Func obj) => types_package.hasPtrRecvᴍ(ref obj);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void isDependency(this ref Func _) => types_package.isDependencyᴍ(ref _);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ж<Func> Ꮡobj) => types_package.Stringᴍ(Ꮡobj);
}
partial class typesᴛLabel {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ж<Label> Ꮡobj) => types_package.Stringᴍ(Ꮡobj);
}
partial class typesᴛBuiltin {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ж<Builtin> Ꮡobj) => types_package.Stringᴍ(Ꮡobj);
}
partial class typesᴛNil {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ж<Nil> Ꮡobj) => types_package.Stringᴍ(Ꮡobj);
}
partial class typesᴛobjset {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Object insert(this ref objset s, Object obj) => types_package.insertᴍ(ref s, obj);
}
partial class typesᴛoperand {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static token.Pos Pos(this ref operand x) => types_package.Posᴍ(ref x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string String(this ж<operand> Ꮡx) => types_package.Stringᴍ(Ꮡx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void setConst(this ref operand x, token.Token k, @string lit) => types_package.setConstᴍ(ref x, k, lit);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool isNil(this ref operand x) => types_package.isNilᴍ(ref x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (bool, errors.Code) assignableTo(this ж<operand> Ꮡx, ж<Checker> check, Type T, ж<@string> Ꮡcause) => types_package.assignableToᴍ(Ꮡx, check, T, Ꮡcause);
}
partial class typesᴛPackage {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Path(this ref Package pkg) => types_package.Pathᴍ(ref pkg);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Name(this ref Package pkg) => types_package.Nameᴍ(ref pkg);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void SetName(this ref Package pkg, @string name) => types_package.SetNameᴍ(ref pkg, name);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string GoVersion(this ref Package pkg) => types_package.GoVersionᴍ(ref pkg);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Scope> Scope(this ж<Package> Ꮡpkg) => types_package.Scopeᴍ(Ꮡpkg);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Complete(this ref Package pkg) => types_package.Completeᴍ(ref pkg);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void MarkComplete(this ref Package pkg) => types_package.MarkCompleteᴍ(ref pkg);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<ж<Package>> Imports(this ref Package pkg) => types_package.Importsᴍ(ref pkg);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void SetImports(this ref Package pkg, slice<ж<Package>> list) => types_package.SetImportsᴍ(ref pkg, list);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ref Package pkg) => types_package.Stringᴍ(ref pkg);
}
partial class typesᴛPointer {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Type Elem(this ref Pointer p) => types_package.Elemᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Type Underlying(this ж<Pointer> Ꮡp) => types_package.Underlyingᴍ(Ꮡp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ж<Pointer> Ꮡp) => types_package.Stringᴍ(Ꮡp);
}
partial class typesᴛifacePair {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool identical(this ref ifacePair p, ж<ifacePair> Ꮡq) => types_package.identicalᴍ(ref p, Ꮡq);
}
partial class typesᴛcomparer {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool identical(this ref comparer c, Type x, Type y, ж<ifacePair> Ꮡp) => types_package.identicalᴍ(ref c, x, y, Ꮡp);
}
partial class typesᴛChecker {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void rangeStmt(this ж<Checker> Ꮡcheck, stmtContext inner, ж<ast.RangeStmt> ᏑrangeStmt, positioner noNewVarPos, ast.Expr sKey, ast.Expr sValue, ast.Expr sExtra, ast.Expr rangeVar, bool isDef) => types_package.rangeStmtᴍ(Ꮡcheck, inner, ᏑrangeStmt, noNewVarPos, sKey, sValue, sExtra, rangeVar, isDef);
}
partial class typesᴛChecker {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void record(this ref Checker check, ж<operand> Ꮡx) => types_package.recordᴍ(ref check, Ꮡx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void recordUntyped(this ж<Checker> Ꮡcheck) => types_package.recordUntypedᴍ(Ꮡcheck);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void recordTypeAndValue(this ref Checker check, ast.Expr x, operandMode mode, Type typ, constant.Value val) => types_package.recordTypeAndValueᴍ(ref check, x, mode, typ, val);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void recordBuiltinType(this ref Checker check, ast.Expr f, ж<Signature> sig) => types_package.recordBuiltinTypeᴍ(ref check, f, sig);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void recordCommaOkTypes(this ref Checker check, ast.Expr x, slice<ж<operand>> a) => types_package.recordCommaOkTypesᴍ(ref check, x, a);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void recordInstance(this ref Checker check, ast.Expr expr, slice<Type> targs, Type typ) => types_package.recordInstanceᴍ(ref check, expr, targs, typ);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void recordDef(this ref Checker check, ж<ast.Ident> id, Object obj) => types_package.recordDefᴍ(ref check, id, obj);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void recordUse(this ref Checker check, ж<ast.Ident> id, Object obj) => types_package.recordUseᴍ(ref check, id, obj);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void recordImplicit(this ref Checker check, ast.Node node, Object obj) => types_package.recordImplicitᴍ(ref check, node, obj);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void recordSelection(this ref Checker check, ж<ast.SelectorExpr> Ꮡx, SelectionKind kind, Type recv, Object obj, slice<nint> index, bool indirect) => types_package.recordSelectionᴍ(ref check, Ꮡx, kind, recv, obj, index, indirect);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void recordScope(this ref Checker check, ast.Node node, ж<Scope> scope) => types_package.recordScopeᴍ(ref check, node, scope);
}
partial class typesᴛdeclInfo {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool hasInitializer(this ref declInfo d) => types_package.hasInitializerᴍ(ref d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void addDep(this ref declInfo d, Object obj) => types_package.addDepᴍ(ref d, obj);
}
partial class typesᴛChecker {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void arityMatch(this ж<Checker> Ꮡcheck, ж<ast.ValueSpec> Ꮡs, ж<ast.ValueSpec> Ꮡinit) => types_package.arityMatchᴍ(Ꮡcheck, Ꮡs, Ꮡinit);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void declarePkgObj(this ж<Checker> Ꮡcheck, ж<ast.Ident> Ꮡident, Object obj, ж<declInfo> d) => types_package.declarePkgObjᴍ(Ꮡcheck, Ꮡident, obj, d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string filename(this ref Checker check, nint fileNo) => types_package.filenameᴍ(ref check, fileNo);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Package> importPackage(this ж<Checker> Ꮡcheck, positioner at, @string path, @string dir) => types_package.importPackageᴍ(Ꮡcheck, at, path, dir);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void collectObjects(this ж<Checker> Ꮡcheck) => types_package.collectObjectsᴍ(Ꮡcheck);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void sortObjects(this ref Checker check) => types_package.sortObjectsᴍ(ref check);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (bool ptr, ast.Expr @base, slice<ж<ast.Ident>> tparams) unpackRecv(this ж<Checker> Ꮡcheck, ast.Expr rtyp, bool unpackParams) => types_package.unpackRecvᴍ(Ꮡcheck, rtyp, unpackParams);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (bool ptr_, ж<TypeName> @base) resolveBaseTypeName(this ref Checker check, bool ptr, ж<ast.Ident> Ꮡname) => types_package.resolveBaseTypeNameᴍ(ref check, ptr, Ꮡname);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void packageObjects(this ж<Checker> Ꮡcheck) => types_package.packageObjectsᴍ(Ꮡcheck);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void unusedImports(this ж<Checker> Ꮡcheck) => types_package.unusedImportsᴍ(Ꮡcheck);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void errorUnusedPkg(this ж<Checker> Ꮡcheck, ж<PkgName> Ꮡobj) => types_package.errorUnusedPkgᴍ(Ꮡcheck, Ꮡobj);
}
partial class typesᴛChecker {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool isTerminating(this ref Checker check, ast.Stmt s, @string label) => types_package.isTerminatingᴍ(ref check, s, label);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool isTerminatingList(this ref Checker check, slice<ast.Stmt> list, @string label) => types_package.isTerminatingListᴍ(ref check, list, label);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool isTerminatingSwitch(this ref Checker check, ж<ast.BlockStmt> Ꮡbody, @string label) => types_package.isTerminatingSwitchᴍ(ref check, Ꮡbody, label);
}
partial class typesᴛScope {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Scope> Parent(this ref Scope s) => types_package.Parentᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint Len(this ref Scope s) => types_package.Lenᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<@string> Names(this ref Scope s) => types_package.Namesᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint NumChildren(this ref Scope s) => types_package.NumChildrenᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Scope> Child(this ref Scope s, nint i) => types_package.Childᴍ(ref s, i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Object Lookup(this ref Scope s, @string name) => types_package.Lookupᴍ(ref s, name);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<Object> lookupIgnoringCase(this ref Scope s, @string name, bool exported) => types_package.lookupIgnoringCaseᴍ(ref s, name, exported);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Object Insert(this ж<Scope> Ꮡs, Object obj) => types_package.Insertᴍ(Ꮡs, obj);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool _InsertLazy(this ж<Scope> Ꮡs, @string name, Func<Object> resolve) => types_package._InsertLazyᴍ(Ꮡs, name, resolve);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void insert(this ref Scope s, @string name, Object obj) => types_package.insertᴍ(ref s, name, obj);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void WriteTo(this ж<Scope> Ꮡs, io.Writer w, nint n, bool recurse) => types_package.WriteToᴍ(Ꮡs, w, n, recurse);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ж<Scope> Ꮡs) => types_package.Stringᴍ(Ꮡs);
}
partial class typesᴛlazyObject {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Scope> Parent(this ref lazyObject _) => types_package.Parentᴍ(ref _);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static token.Pos Pos(this ref lazyObject _) => types_package.Posᴍ(ref _);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Package> Pkg(this ref lazyObject _) => types_package.Pkgᴍ(ref _);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string Name(this ref lazyObject _) => types_package.Nameᴍ(ref _);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Type Type(this ref lazyObject _) => types_package.Typeᴍ(ref _);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool Exported(this ref lazyObject _) => types_package.Exportedᴍ(ref _);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string Id(this ref lazyObject _) => types_package.Idᴍ(ref _);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string String(this ref lazyObject _) => types_package.Stringᴍ(ref _);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint32 order(this ref lazyObject _) => types_package.orderᴍ(ref _);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void setType(this ref lazyObject _Δp0, Type _Δp1) => types_package.setTypeᴍ(ref _Δp0, _Δp1);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void setOrder(this ref lazyObject _Δp0, uint32 _Δp1) => types_package.setOrderᴍ(ref _Δp0, _Δp1);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void setParent(this ref lazyObject _Δp0, ж<Scope> _Δp1) => types_package.setParentᴍ(ref _Δp0, _Δp1);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool sameId(this ref lazyObject _Δp0, ж<Package> _Δp1, @string _Δp2, bool _Δp3) => types_package.sameIdᴍ(ref _Δp0, _Δp1, _Δp2, _Δp3);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static token.Pos scopePos(this ref lazyObject _) => types_package.scopePosᴍ(ref _);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void setScopePos(this ref lazyObject _Δp0, token.Pos _Δp1) => types_package.setScopePosᴍ(ref _Δp0, _Δp1);
}
partial class typesᴛScope {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<Scope>, Object) LookupParent(this ж<Scope> Ꮡs, @string name, token.Pos pos) => types_package.LookupParentᴍ(Ꮡs, name, pos);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static token.Pos Pos(this ref Scope s) => types_package.Posᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static token.Pos End(this ref Scope s) => types_package.Endᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Contains(this ref Scope s, token.Pos pos) => types_package.Containsᴍ(ref s, pos);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Scope> Innermost(this ж<Scope> Ꮡs, token.Pos pos) => types_package.Innermostᴍ(Ꮡs, pos);
}
partial class typesᴛSelection {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static SelectionKind Kind(this ref Selection s) => types_package.Kindᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Type Recv(this ref Selection s) => types_package.Recvᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Object Obj(this ref Selection s) => types_package.Objᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Type Type(this ref Selection s) => types_package.Typeᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<nint> Index(this ref Selection s) => types_package.Indexᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Indirect(this ref Selection s) => types_package.Indirectᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ж<Selection> Ꮡs) => types_package.Stringᴍ(Ꮡs);
}
partial class typesᴛSignature {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Var> Recv(this ref Signature s) => types_package.Recvᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<TypeParamList> TypeParams(this ref Signature s) => types_package.TypeParamsᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<TypeParamList> RecvTypeParams(this ref Signature s) => types_package.RecvTypeParamsᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Tuple> Params(this ref Signature s) => types_package.Paramsᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Tuple> Results(this ref Signature s) => types_package.Resultsᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Variadic(this ref Signature s) => types_package.Variadicᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Type Underlying(this ж<Signature> Ꮡs) => types_package.Underlyingᴍ(Ꮡs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ж<Signature> Ꮡs) => types_package.Stringᴍ(Ꮡs);
}
partial class typesᴛChecker {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void funcType(this ж<Checker> Ꮡcheck, ж<Signature> Ꮡsig, ж<ast.FieldList> ᏑrecvPar, ж<ast.FuncType> Ꮡftyp) => types_package.funcTypeᴍ(Ꮡcheck, Ꮡsig, ᏑrecvPar, Ꮡftyp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<Var>, ж<TypeParamList>) collectRecv(this ж<Checker> Ꮡcheck, ж<ast.Field> Ꮡrparam, token.Pos scopePos) => types_package.collectRecvᴍ(Ꮡcheck, Ꮡrparam, scopePos);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void recordParenthesizedRecvTypes(this ref Checker check, ast.Expr expr, Type typ) => types_package.recordParenthesizedRecvTypesᴍ(ref check, expr, typ);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (slice<ж<ast.Ident>> names, slice<ж<Var>> @params, bool variadic) collectParams(this ж<Checker> Ꮡcheck, VarKind kind, ж<ast.FieldList> Ꮡlist) => types_package.collectParamsᴍ(Ꮡcheck, kind, Ꮡlist);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void declareParams(this ж<Checker> Ꮡcheck, slice<ж<ast.Ident>> names, slice<ж<Var>> @params, token.Pos scopePos) => types_package.declareParamsᴍ(Ꮡcheck, names, @params, scopePos);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void validRecv(this ж<Checker> Ꮡcheck, positioner pos, ж<Var> Ꮡrecv) => types_package.validRecvᴍ(Ꮡcheck, pos, Ꮡrecv);
}
partial class typesᴛStdSizes {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static int64 /*result*/ Alignof(this ж<StdSizes> Ꮡs, Type T) => types_package.Alignofᴍ(Ꮡs, T);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<int64> Offsetsof(this ж<StdSizes> Ꮡs, slice<ж<Var>> fields) => types_package.Offsetsofᴍ(Ꮡs, fields);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static int64 Sizeof(this ж<StdSizes> Ꮡs, Type T) => types_package.Sizeofᴍ(Ꮡs, T);
}
partial class typesᴛConfig {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static int64 alignof(this ref Config conf, Type T) => types_package.alignofᴍ(ref conf, T);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<int64> offsetsof(this ref Config conf, ж<Struct> ᏑT) => types_package.offsetsofᴍ(ref conf, ᏑT);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static int64 offsetof(this ref Config conf, Type T, slice<nint> index) => types_package.offsetofᴍ(ref conf, T, index);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static int64 @sizeof(this ref Config conf, Type T) => types_package.@sizeofᴍ(ref conf, T);
}
partial class typesᴛSlice {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Type Elem(this ref Slice s) => types_package.Elemᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Type Underlying(this ж<Slice> Ꮡs) => types_package.Underlyingᴍ(Ꮡs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ж<Slice> Ꮡs) => types_package.Stringᴍ(Ꮡs);
}
partial class typesᴛChecker {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void funcBody(this ж<Checker> Ꮡcheck, ж<declInfo> decl, @string name, ж<Signature> Ꮡsig, ж<ast.BlockStmt> Ꮡbody, constant.Value iota) => types_package.funcBodyᴍ(Ꮡcheck, decl, name, Ꮡsig, Ꮡbody, iota);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void usage(this ж<Checker> Ꮡcheck, ж<Scope> Ꮡscope) => types_package.usageᴍ(Ꮡcheck, Ꮡscope);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void simpleStmt(this ж<Checker> Ꮡcheck, ast.Stmt s) => types_package.simpleStmtᴍ(Ꮡcheck, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void stmtList(this ж<Checker> Ꮡcheck, stmtContext ctxt, slice<ast.Stmt> list) => types_package.stmtListᴍ(Ꮡcheck, ctxt, list);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void multipleDefaults(this ж<Checker> Ꮡcheck, slice<ast.Stmt> list) => types_package.multipleDefaultsᴍ(Ꮡcheck, list);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void openScope(this ref Checker check, ast.Node node, @string comment) => types_package.openScopeᴍ(ref check, node, comment);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void closeScope(this ref Checker check) => types_package.closeScopeᴍ(ref check);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void suspendedCall(this ж<Checker> Ꮡcheck, @string keyword, ж<ast.CallExpr> call) => types_package.suspendedCallᴍ(Ꮡcheck, keyword, call);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void caseValues(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, slice<ast.Expr> values, valueMap seen) => types_package.caseValuesᴍ(Ꮡcheck, Ꮡx, values, seen);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool isNil(this ref Checker check, ast.Expr e) => types_package.isNilᴍ(ref check, e);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Type caseTypes(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, slice<ast.Expr> types, map<Type, ast.Expr> seen) => types_package.caseTypesᴍ(Ꮡcheck, Ꮡx, types, seen);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Type caseTypes_currently_unused(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, ж<Interface> xtyp, slice<ast.Expr> types, map<@string, ast.Expr> seen) => types_package.caseTypes_currently_unusedᴍ(Ꮡcheck, Ꮡx, xtyp, types, seen);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void stmt(this ж<Checker> Ꮡcheck, stmtContext ctxt, ast.Stmt s) => types_package.stmtᴍ(Ꮡcheck, ctxt, s);
}
partial class typesᴛStruct {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint NumFields(this ref Struct s) => types_package.NumFieldsᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Var> Field(this ref Struct s, nint i) => types_package.Fieldᴍ(ref s, i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Tag(this ref Struct s, nint i) => types_package.Tagᴍ(ref s, i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Type Underlying(this ж<Struct> Ꮡt) => types_package.Underlyingᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ж<Struct> Ꮡt) => types_package.Stringᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void markComplete(this ref Struct s) => types_package.markCompleteᴍ(ref s);
}
partial class typesᴛChecker {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void structType(this ж<Checker> Ꮡcheck, ж<Struct> Ꮡstyp, ж<ast.StructType> Ꮡe) => types_package.structTypeᴍ(Ꮡcheck, Ꮡstyp, Ꮡe);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool declareInSet(this ж<Checker> Ꮡcheck, ж<objset> Ꮡoset, token.Pos pos, Object obj) => types_package.declareInSetᴍ(Ꮡcheck, Ꮡoset, pos, obj);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string tag(this ж<Checker> Ꮡcheck, ж<ast.BasicLit> Ꮡt) => types_package.tagᴍ(Ꮡcheck, Ꮡt);
}
partial class typesᴛsubstMap {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool empty(this substMap m) => types_package.emptyᴍ(m);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Type lookup(this substMap m, ж<TypeParam> tpar) => types_package.lookupᴍ(m, tpar);
}
partial class typesᴛChecker {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Type subst(this ж<Checker> Ꮡcheck, token.Pos pos, Type typ, substMap smap, ж<Named> expanding, ж<Context> ctxt) => types_package.substᴍ(Ꮡcheck, pos, typ, smap, expanding, ctxt);
}
partial class typesᴛsubster {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Type typ(this ж<subster> Ꮡsubst, Type typ) => types_package.typᴍ(Ꮡsubst, typ);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Type typOrNil(this ж<subster> Ꮡsubst, Type typ) => types_package.typOrNilᴍ(Ꮡsubst, typ);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Var> var_(this ж<subster> Ꮡsubst, ж<Var> Ꮡv) => types_package.var_ᴍ(Ꮡsubst, Ꮡv);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Tuple> tuple(this ж<subster> Ꮡsubst, ж<Tuple> Ꮡt) => types_package.tupleᴍ(Ꮡsubst, Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Func> func_(this ж<subster> Ꮡsubst, ж<Func> Ꮡf) => types_package.func_ᴍ(Ꮡsubst, Ꮡf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Term> term(this ж<subster> Ꮡsubst, ж<Term> Ꮡt) => types_package.termᴍ(Ꮡsubst, Ꮡt);
}
partial class typesᴛtermlist {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string String(this termlist xl) => types_package.Stringᴍ(xl);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool isEmpty(this termlist xl) => types_package.isEmptyᴍ(xl);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool isAll(this termlist xl) => types_package.isAllᴍ(xl);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static termlist norm(this termlist xl) => types_package.normᴍ(xl);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static termlist union(this termlist xl, termlist yl) => types_package.unionᴍ(xl, yl);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static termlist intersect(this termlist xl, termlist yl) => types_package.intersectᴍ(xl, yl);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool equal(this termlist xl, termlist yl) => types_package.equalᴍ(xl, yl);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool includes(this termlist xl, Type t) => types_package.includesᴍ(xl, t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool supersetOf(this termlist xl, ж<term> y) => types_package.supersetOfᴍ(xl, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool subsetOf(this termlist xl, termlist yl) => types_package.subsetOfᴍ(xl, yl);
}
partial class typesᴛTuple {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint Len(this ж<Tuple> Ꮡt) => types_package.Lenᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Var> At(this ref Tuple t, nint i) => types_package.Atᴍ(ref t, i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Type Underlying(this ж<Tuple> Ꮡt) => types_package.Underlyingᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ж<Tuple> Ꮡt) => types_package.Stringᴍ(Ꮡt);
}
partial class typesᴛTypeParamList {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint Len(this ж<TypeParamList> Ꮡl) => types_package.Lenᴍ(Ꮡl);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<TypeParam> At(this ref TypeParamList l, nint i) => types_package.Atᴍ(ref l, i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<ж<TypeParam>> list(this ж<TypeParamList> Ꮡl) => types_package.listᴍ(Ꮡl);
}
partial class typesᴛTypeList {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint Len(this ж<TypeList> Ꮡl) => types_package.Lenᴍ(Ꮡl);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Type At(this ref TypeList l, nint i) => types_package.Atᴍ(ref l, i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<Type> list(this ж<TypeList> Ꮡl) => types_package.listᴍ(Ꮡl);
}
partial class typesᴛChecker {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<TypeParam> newTypeParam(this ж<Checker> Ꮡcheck, ж<TypeName> Ꮡobj, Type constraint) => types_package.newTypeParamᴍ(Ꮡcheck, Ꮡobj, constraint);
}
partial class typesᴛTypeParam {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<TypeName> Obj(this ref TypeParam t) => types_package.Objᴍ(ref t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint Index(this ref TypeParam t) => types_package.Indexᴍ(ref t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Type Constraint(this ref TypeParam t) => types_package.Constraintᴍ(ref t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void SetConstraint(this ref TypeParam t, Type bound) => types_package.SetConstraintᴍ(ref t, bound);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Type Underlying(this ref TypeParam t) => types_package.Underlyingᴍ(ref t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ж<TypeParam> Ꮡt) => types_package.Stringᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void cleanup(this ref TypeParam t) => types_package.cleanupᴍ(ref t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Interface> iface(this ref TypeParam t) => types_package.ifaceᴍ(ref t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool @is(this ref TypeParam t, Func<ж<term>, bool> f) => types_package.@isᴍ(ref t, f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool typeset(this ref TypeParam t, Func<Type, Type, bool> f) => types_package.typesetᴍ(ref t, f);
}
partial class typesᴛ_TypeSet {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool IsEmpty(this ref _TypeSet s) => types_package.IsEmptyᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool IsAll(this ref _TypeSet s) => types_package.IsAllᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool IsMethodSet(this ref _TypeSet s) => types_package.IsMethodSetᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool IsComparable(this ref _TypeSet s, map<Type, bool> seen) => types_package.IsComparableᴍ(ref s, seen);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint NumMethods(this ref _TypeSet s) => types_package.NumMethodsᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Func> Method(this ref _TypeSet s, nint i) => types_package.Methodᴍ(ref s, i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint, ж<Func>) LookupMethod(this ref _TypeSet s, ж<Package> pkg, @string name, bool foldCase) => types_package.LookupMethodᴍ(ref s, pkg, name, foldCase);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string String(this ref _TypeSet s) => types_package.Stringᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool hasTerms(this ref _TypeSet s) => types_package.hasTermsᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool subsetOf(this ref _TypeSet s1, ж<_TypeSet> Ꮡs2) => types_package.subsetOfᴍ(ref s1, Ꮡs2);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool all(this ref _TypeSet s, Func<Type, Type, bool> f) => types_package.allᴍ(ref s, f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool @is(this ref _TypeSet s, Func<ж<term>, bool> f) => types_package.@isᴍ(ref s, f);
}
partial class typesᴛtypeWriter {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void @byte(this ref typeWriter w, byte b) => types_package.@byteᴍ(ref w, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void @string(this ref typeWriter w, @string s) => types_package.@stringᴍ(ref w, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void error(this ref typeWriter w, @string msg) => types_package.errorᴍ(ref w, msg);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void typ(this ж<typeWriter> Ꮡw, Type typ) => types_package.typᴍ(Ꮡw, typ);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void typeSet(this ж<typeWriter> Ꮡw, ж<_TypeSet> Ꮡs) => types_package.typeSetᴍ(Ꮡw, Ꮡs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void typeList(this ж<typeWriter> Ꮡw, slice<Type> list) => types_package.typeListᴍ(Ꮡw, list);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void tParamList(this ж<typeWriter> Ꮡw, slice<ж<TypeParam>> list) => types_package.tParamListᴍ(Ꮡw, list);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void typeName(this ref typeWriter w, ж<TypeName> Ꮡobj) => types_package.typeNameᴍ(ref w, Ꮡobj);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void tuple(this ж<typeWriter> Ꮡw, ж<Tuple> Ꮡtup, bool variadic) => types_package.tupleᴍ(Ꮡw, Ꮡtup, variadic);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void signature(this ж<typeWriter> Ꮡw, ж<Signature> Ꮡsig) => types_package.signatureᴍ(Ꮡw, Ꮡsig);
}
partial class typesᴛterm {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ж<term> Ꮡx) => types_package.Stringᴍ(Ꮡx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool equal(this ж<term> Ꮡx, ж<term> Ꮡy) => types_package.equalᴍ(Ꮡx, Ꮡy);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<term>, ж<term>) union(this ж<term> Ꮡx, ж<term> Ꮡy) => types_package.unionᴍ(Ꮡx, Ꮡy);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<term> intersect(this ж<term> Ꮡx, ж<term> Ꮡy) => types_package.intersectᴍ(Ꮡx, Ꮡy);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool includes(this ж<term> Ꮡx, Type t) => types_package.includesᴍ(Ꮡx, t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool subsetOf(this ж<term> Ꮡx, ж<term> Ꮡy) => types_package.subsetOfᴍ(Ꮡx, Ꮡy);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool disjoint(this ref term x, ж<term> Ꮡy) => types_package.disjointᴍ(ref x, Ꮡy);
}
partial class typesᴛChecker {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void ident(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, ж<ast.Ident> Ꮡe, bool wantType) => types_package.identᴍ(Ꮡcheck, Ꮡx, Ꮡe, wantType);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Type typ(this ж<Checker> Ꮡcheck, ast.Expr e) => types_package.typᴍ(Ꮡcheck, e);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Type varType(this ж<Checker> Ꮡcheck, ast.Expr e) => types_package.varTypeᴍ(Ꮡcheck, e);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void validVarType(this ж<Checker> Ꮡcheck, ast.Expr e, Type typ) => types_package.validVarTypeᴍ(Ꮡcheck, e, typ);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Type declaredType(this ж<Checker> Ꮡcheck, ast.Expr e, ж<TypeName> def) => types_package.declaredTypeᴍ(Ꮡcheck, e, def);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Type genericType(this ж<Checker> Ꮡcheck, ast.Expr e, ж<@string> Ꮡcause) => types_package.genericTypeᴍ(Ꮡcheck, e, Ꮡcause);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Type /*T*/ typInternal(this ж<Checker> Ꮡcheck, ast.Expr e0, ж<TypeName> def) => types_package.typInternalᴍ(Ꮡcheck, e0, def);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Type /*res*/ instantiatedType(this ж<Checker> Ꮡcheck, ж<indexedExpr> Ꮡix) => types_package.instantiatedTypeᴍ(Ꮡcheck, Ꮡix);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static int64 arrayLength(this ж<Checker> Ꮡcheck, ast.Expr e) => types_package.arrayLengthᴍ(Ꮡcheck, e);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<Type> typeList(this ж<Checker> Ꮡcheck, slice<ast.Expr> list) => types_package.typeListᴍ(Ꮡcheck, list);
}
partial class typesᴛtypeError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string format(this ref typeError err, ж<Checker> check) => types_package.formatᴍ(ref err, check);
}
partial class typesᴛunifyMode {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string String(this unifyMode m) => types_package.Stringᴍ(m);
}
partial class typesᴛunifier {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool unify(this ж<unifier> Ꮡu, Type x, Type y, unifyMode mode) => types_package.unifyᴍ(Ꮡu, x, y, mode);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void tracef(this ref unifier u, @string format, params ꓸꓸꓸany argsʗp) => types_package.tracefᴍ(ref u, format, argsʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string String(this ref unifier u) => types_package.Stringᴍ(ref u);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool join(this ref unifier u, ж<TypeParam> x, ж<TypeParam> y) => types_package.joinᴍ(ref u, x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<TypeParam> asBoundTypeParam(this ref unifier u, Type x) => types_package.asBoundTypeParamᴍ(ref u, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void setHandle(this ref unifier u, ж<TypeParam> x, ж<Type> h) => types_package.setHandleᴍ(ref u, x, h);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Type at(this ref unifier u, ж<TypeParam> x) => types_package.atᴍ(ref u, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void set(this ref unifier u, ж<TypeParam> x, Type t) => types_package.setᴍ(ref u, x, t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint unknowns(this ref unifier u) => types_package.unknownsᴍ(ref u);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<Type> inferred(this ref unifier u, slice<ж<TypeParam>> tparams) => types_package.inferredᴍ(ref u, tparams);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool /*result*/ nify(this ж<unifier> Ꮡu, Type xʗp, Type yʗp, unifyMode mode, ж<ifacePair> Ꮡp) => types_package.nifyᴍ(Ꮡu, xʗp, yʗp, mode, Ꮡp);
}
partial class typesᴛtypeParamsById {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint Len(this typeParamsById s) => types_package.Lenᴍ(s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool Less(this typeParamsById s, nint i, nint j) => types_package.Lessᴍ(s, i, j);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void Swap(this typeParamsById s, nint i, nint j) => types_package.Swapᴍ(s, i, j);
}
partial class typesᴛUnion {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint Len(this ref Union u) => types_package.Lenᴍ(ref u);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Term> Term(this ref Union u, nint i) => types_package.Termᴍ(ref u, i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Type Underlying(this ж<Union> Ꮡu) => types_package.Underlyingᴍ(Ꮡu);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ж<Union> Ꮡu) => types_package.Stringᴍ(Ꮡu);
}
partial class typesᴛTerm {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Tilde(this ref Term t) => types_package.Tildeᴍ(ref t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Type Type(this ref Term t) => types_package.Typeᴍ(ref t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ж<Term> Ꮡt) => types_package.Stringᴍ(Ꮡt);
}
partial class typesᴛChecker {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void validType(this ж<Checker> Ꮡcheck, ж<Named> typ) => types_package.validTypeᴍ(Ꮡcheck, typ);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool validType0(this ж<Checker> Ꮡcheck, token.Pos pos, Type typ, slice<ж<Named>> nest, slice<ж<Named>> path) => types_package.validType0ᴍ(Ꮡcheck, pos, typ, nest, path);
}
partial class typesᴛgoVersion {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool isValid(this goVersion v) => types_package.isValidᴍ(v);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint cmp(this goVersion x, goVersion y) => types_package.cmpᴍ(x, y);
}
partial class typesᴛChecker {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool allowVersion(this ref Checker check, goVersion want) => types_package.allowVersionᴍ(ref check, want);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool verifyVersionf(this ж<Checker> Ꮡcheck, positioner at, goVersion v, @string format, params ꓸꓸꓸany argsʗp) => types_package.verifyVersionfᴍ(Ꮡcheck, at, v, format, argsʗp);
}
// </GlyphForwarders>
