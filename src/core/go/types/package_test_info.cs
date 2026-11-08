// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.go.types_package;
global using static global::go.go.types_internal_test_package;

// <ImportedTypeAliases>
global using flagꓸErrorHandling = go.flag_package.ΔErrorHandling;
global using reflectꓸValue = go.reflect_package.ΔValue;
global using scannerꓸError = go.go.scanner_package.ΔError;
global using tokenꓸFile = go.go.token_package.ΔFile;
global using tokenꓸPos = go.go.token_package.ΔPos;
global using tokenꓸPosition = go.go.token_package.ΔPosition;
global using typesꓸError = go.go.types_package.ΔError;
global using typesꓸInfo = go.go.types_package.ΔInfo;
global using typesꓸScope = go.go.types_package.ΔScope;
global using typesꓸSignature = go.go.types_package.ΔSignature;
global using typesꓸTerm = go.go.types_package.ΔTerm;
global using typesꓸType = go.go.types_package.ΔType;
// </ImportedTypeAliases>

using go;
using static global::go.go.types_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("696e746572666163657b54797065506172616d732829202a676f2f74797065732e54797065506172616d4c6973747d", "TestInstanceInfo_typeᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6e616d6520737472696e673b2073726320737472696e673b2073696720737472696e677d", "builtinCallsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b73726320737472696e673b206f626a20737472696e673b2077616e7420737472696e673b20616c69617320626f6f6c7d", "testObjectsᴛ1")]
[assembly: GoTypeAlias("Error", "ΔError")]
[assembly: GoTypeAlias("Info", "ΔInfo")]
[assembly: GoTypeAlias("Scope", "ΔScope")]
[assembly: GoTypeAlias("Signature", "ΔSignature")]
[assembly: GoTypeAlias("String", "const:ΔString")]
[assembly: GoTypeAlias("Term", "ΔTerm")]
[assembly: GoTypeAlias("Type", "ΔType")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<global::go.go.types_package.atPos, global::go.go.types_package.positioner>]
[assembly: GoImplement<global::go.go.types_package.ΔSignature, global::go.go.types_package.ΔType>(Pointer = true)]
[assembly: GoImplement<go.go.scanner_package.ΔError, error>(Pointer = true)]
[assembly: GoImplement<importHelper, go.go.types_package.Importer>]
[assembly: GoImplement<resolveTestImporter, go.go.types_package.Importer>(Pointer = true)]
[assembly: GoImplement<stdlibChecker, go.go.types_package.Importer>(Pointer = true)]
[assembly: GoImplement<strings_package.Builder, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<testImporter, go.go.types_package.Importer>]
[assembly: GoImplement<testing_package.B, testing_package.TB>(Pointer = true)]
[assembly: GoImplement<testing_package.T, testing_package.TB>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
// </ImplicitConversions>

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: GoFieldAccess(typeof(futurePackage))]
[assembly: GoFieldAccess(typeof(gen))]
[assembly: GoFieldAccess(typeof(global::go.go.types_package.Package))]
[assembly: GoFieldAccess(typeof(global::go.go.types_package.Δterm))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.ArrayType))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.BasicLit))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.BlockStmt))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.CallExpr))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.Comment))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.CommentGroup))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.CompositeLit))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.Field))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.FieldList))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.File))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.FuncDecl))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.FuncType))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.GenDecl))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.Ident))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.ImportSpec))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.ParenExpr))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.ReturnStmt))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.SelectorExpr))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.TypeSpec))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.ValueSpec))]
[assembly: GoFieldAccess(typeof(go.go.build_package.Package))]
[assembly: GoFieldAccess(typeof(go.go.scanner_package.ΔError))]
[assembly: GoFieldAccess(typeof(go.go.types_package.ArgumentError))]
[assembly: GoFieldAccess(typeof(go.go.types_package.Config))]
[assembly: GoFieldAccess(typeof(go.go.types_package.Initializer))]
[assembly: GoFieldAccess(typeof(go.go.types_package.ΔInfo))]
[assembly: GoFieldAccess(typeof(resolveTestImporter))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("go/types/alias_test.go", "alias_test.cs", "ACcegpQAARyCgoKmgoKCpoKCgoKCgpSmAAoGgpSYgoKCpoKClIKClIKUlA==")]
[assembly: GoPositionMap("go/types/api_test.go", "api_test.cs", "ACI6gqaCgoKUprSCgoLapoKCgpTokoKCgoKClJQACQaCAEicAYKmlpKCgoKmgoKUpoCCgsiCgqaCAA8MhKQAzAKiBYKCgoKCgoKUgqaokoKCgqaCgriCAB4Kgo6MAJ4BxAKygoKCgrKUgpSClIKU1IKAksiCgoKUlIKAgqSClIKCpoCC2oKCgpSCgpSCAAsYooKUhgAIBoIAEi6CppaSgoKCpoKCpoIACgqCACpegqaWkoKCgpSmgoKmggARCoIAABCCgsqCgoKmpoKCqKKSgoKClIKkgoKCgoKCgpSClKS0tsbIgpSClIKUggAQCIKUABo8gqamgoKokoKUtLTEpLiCACEKgpQAASgABxaCgriCgoKCuIKCgriCgoKUgoKUgsqAggAaCIKCgoKClLiCgoKCgoKCgpSClAAMBoKUADyCAYKCpoKUgoK4ggAfCoKUAEGKAYKCpoK4goKUtLS0tLS0tLS0yIKCAAoMggCOAZACgoKmgoK4goKCggASDIKCgsq+goKUgsqC7oKCgoKUgoK6koKCppLcgoKUAGQGgriCgoKygoKUpgAAEAABmgEAIEKClIKUgoKUgoKUuIKClMqCgoKCgpSCuIIAGQiCgoKStqKmnp6CABUGgoKCkraippqcgua0goKCgqaiAAkEjAAvYIKUgoKCgriCgoKUpoKUggAeDJIAASCCgoKCpoKqooL2ogAOJIIACQqiAAwgggAMCpQAGz6CgoKCgpSCAAkKlAAFEoIADQqCgoLukoKCgoKClJSUAAgYgoKCyu6ClMqmgoKmgoKC5oKCgoK2poKCgoKUpuyyAAwYgoKCgqaigoKClIKClILKgqYAHAyiAAEqgpSmgqaCpoKUyoKUgsaCyIKUtAAYDqKUAAAUgoKmgoKmgpTKpoKCgqaCgoKUgoKCuIKCgoKmAAoOhJKmgoLKgoK4ggAVCIIAABqU3IKYgpKCspSCgrSklIKClIKSAAsMggAGFoKClJSCgqaCgqaCAAgKgriCgpSCAAsIgoKCkoKCgoKClJSCgoKCgoIALgqSAABCpoKCgoKCppKUABc2gpKClKaysoKUgoKClIKUgpSClIKUgpSClILugpSklAA8BoIAAUyCgpKUkgAOIgAbQIKAgtqCgoKClICCAB4KggABKpSCyoKCppKCgsqmpqYAHwaCgpaYgoKomoKC+JSC3IKCggAKCoIAK2CCgpSUgoKCgpSCgoKClJSCABIOogAFFIKClJSCpKSCgoKCupKCgoKmgoIAFg6SggAAEIKUkoIADgiCgp6ClJKC/KKCgoKCgoKigoKCgrSkAAgOooKCgoKCAAkMgoKUAAgOsoKmgoKUyoKCggAJGgAJAoKmgtyCgoKCgoIAFwiCggABIoKCgoKUgoKCpoIACBKCABUIgo6ClIKCgoIACRSCgoC22oKCgoCS", "47-47:1;752-760:1;764-810:2;823-825:1;995-1014:1;1179-1186:1;1640-1646:1;1814-1814:1;1817-1819:2;1843-1843:1;1846-1848:2;1867-1873:1;2116-2127:1;2182-2182:1;2222-2235:1;2337-2343:1;2436-2443:1;2510-2519:1;2576-2576:1;2607-2612:2;2615-2646:3;2701-2701:1;2704-2704:2;2798-2800:1;2808-2813:2;2968-2968:1;3036-3051:1;3041-3048:1.1;3059-3065:1;3085-3090:1", "", "54=defaultImporter/1/1/2;1993=String/1/1/1,Index/1/1/2,Sprintf/1/1/2;2401=NewTerm/1/5/1,NewTerm/2/5/2,NewTerm/3/5/3,NewTerm/4/5/4,NewTerm/5/5/5;2448=NewFunc/1/2/1,NewFunc/2/2/2;2453=NewFunc/1/1/1;2660=For/1/1/8;2756=Scope/1/2/1,Lookup/1/2/1,Type/1/2/1,Underlying/1/2/1,Scope/2/2/2,Lookup/2/2/2,Type/2/2/2,Underlying/2/2/2,Elem/1/1/2;2806=NewSlice/1/2/7,NewSlice/2/2/8;2946=Invoke/1/10/5,Underlying/1/5/5,Field/1/2/5,Lookup/1/1/6,Type/1/1/6,Underlying/2/5/6,Field/2/2/6,Invoke/2/10/9,Method/1/5/9,Invoke/3/10/10,Method/2/5/10,Signature/1/3/10,Recv/1/1/10,Invoke/4/10/11,Method/3/5/11,Signature/2/3/11,Params/1/3/11,At/1/6/11,Invoke/5/10/12,Method/4/5/12,Signature/3/3/12,Results/1/3/12,At/2/6/12,Invoke/6/10/15,Underlying/3/5/15,Method/5/5/15,Invoke/7/10/18,Underlying/4/5/18,Params/2/3/18,At/3/6/18,Invoke/8/10/19,Underlying/5/5/19,Results/2/3/19,At/4/6/19,Invoke/9/10/22,Params/3/3/22,At/5/6/22,Invoke/10/10/23,Results/3/3/23,At/6/6/23", "", "2118:21,2119:13")]
[assembly: GoPositionMap("go/types/builtins_test.go", "builtins_test.cs", "ABQkAJABpgKClIKCgriCgqaCgsqClIKCpoKCgoKCpoKCuJSCgoKUgoLKlIKCgpSCgoKUgoKUpsi2gg==")]
[assembly: GoPositionMap("go/types/check_test.go", "check_test.cs", "ADxwgpaUooKCgoKClIKCgoKmuLaClKSUqJKClOy0koKUgoKUgoKCpgACHAAOBIKCggAYCNKCuJKCgpSCgoKCupKCgqKClIKCuIK0toKoooKCgoKCgIK2goLIgrgACxiCgriCgoK6koKmgoKCgoK4goKCgoKCgqaCgoKUgoKmgoKClIKmlIKCyoKCgoK6koK4lIKmuILKgoKCgoLq5IKCqqKCqqKCrsKCgpSCgpAACCIADgKUgoKmgoKmgoKClJT4goKCpoKC/rKC5oai5oKC1MLCwpSCgpSCgoKmgqaClILcgpSCgoKmgoKmkriCgoKCgpSU", "167-180:1;360-360:1;408-410:1;454-456:1;475-477:1", "", "", "212:33")]
[assembly: GoPositionMap("go/types/commentMap_test.go", "commentMap_test.cs", "ABU+AAwCgpSCgpSCgqSkgpSCgoKCgpTIgpSkABYKggAAEoKCgoKCgriCgoKClLiCgg==")]
[assembly: GoPositionMap("go/types/errorcalls_test.go", "errorcalls_test.cs", "ABcssoKCgqaCkoKClIKClIK4goKUgpKCgoKmlJTKgoKUpqKCgoKUgrS0tLS2goKUlA==", "30-61:1;49-59:1.1")]
[assembly: GoPositionMap("go/types/eval_test.go", "eval_test.cs", "ABYugoKCgpSCgriUgoK4goKCyoKClIK4goKCuIKCgviCAAkUgoL4ogABEABSigGCgoKCgtykgsimgoKCpoKCgoKCgoIABxCWkoIAJQaCnAAFNoKCgqaCgoKmooKCpriClJSAgtaClICCxqaCgoKCgoKCgoKUggAUEIKagpSCgoKmgpSCpoI=", "265-292:1", "", "345=Position/1/1/1")]
[assembly: GoPositionMap("go/types/example_test.go", "example_test.cs", "ACVC1IKCABMg3IKCgqqigoIAOEqUAAEUgoKC3IKCgriCgoKClLiCggAlItQAAx6CysqCgoK4uIKCgoKClIKCgriUgoKUgoKCgoKCgqamlIIAQnaClKSkpKSClKSkuIKCgg==", "", "", "252=RelativeTo/1/1/1,ObjectString/1/1/1,Pos/1/1/2,Position/1/1/2,Join/1/1/3;272=exprString/1/1/1,mode/1/1/2")]
[assembly: GoPositionMap("go/types/exprstring_test.go", "exprstring_test.cs", "AA4cAGrWAYKCgoKClII=", "", "", "15=dup/1/75/2,dup/2/75/3,dup/3/75/4,dup/4/75/5,dup/5/75/6,dup/6/75/7,dup/7/75/8,dup/8/75/9,dup/9/75/10,dup/10/75/18,dup/11/75/19,dup/12/75/20,dup/13/75/21,dup/14/75/22,dup/15/75/23,dup/16/75/24,dup/17/75/25,dup/18/75/26,dup/19/75/28,dup/20/75/29,dup/21/75/30,dup/22/75/31,dup/23/75/34,dup/24/75/35,dup/25/75/38,dup/26/75/39,dup/27/75/40,dup/28/75/41,dup/29/75/44,dup/30/75/45,dup/31/75/46,dup/32/75/49,dup/33/75/50,dup/34/75/51,dup/35/75/53,dup/36/75/54,dup/37/75/55,dup/38/75/56,dup/39/75/57,dup/40/75/58,dup/41/75/60,dup/42/75/62,dup/43/75/63,dup/44/75/65,dup/45/75/66,dup/46/75/68,dup/47/75/69,dup/48/75/70,dup/49/75/71,dup/50/75/72,dup/51/75/73,dup/52/75/75,dup/53/75/76,dup/54/75/77,dup/55/75/79,dup/56/75/81,dup/57/75/82,dup/58/75/83,dup/59/75/84,dup/60/75/85,dup/61/75/86,dup/62/75/88,dup/63/75/89,dup/64/75/90,dup/65/75/91,dup/66/75/92,dup/67/75/93,dup/68/75/96,dup/69/75/97,dup/70/75/98,dup/71/75/99,dup/72/75/101,dup/73/75/102,dup/74/75/103,dup/75/75/104")]
[assembly: GoPositionMap("go/types/generate_test.go", "generate_test.cs", "AB40AAYU9oKCgpSCuIK4lIKCgrimgqiSgoKCgpSmgoKCuIKCgoKm7qaCgtyCgoKCpoKCAAcQgoKkxqKigMaAoqSCgqaC3IKCpICigNqCgsrcgoKmgoKCpIC0tIDqgAAKEoAACRSygoKCgpSUqLKCvMKCgoKCgpSCzsKCkpSClLqygpKUgpS+woKSlIKUurKCkpSClIKUvMKCgoKmqqKCkqaClIK2xoKCtrqSgpSCgoLIvrKClJSCgoKC2IK2goKCgoLYgoKCgtiCgoKC6ryigpSCgpSCgoKC7AAJCpKClIKCtoKCgoL8upKClIKCtoKCgoL8vrKCpoKCtrqSgpSSgra6koKUqJKokoKCqJKCgoI=", "257-264:1;270-277:1;285-292:1;298-308:1;327-347:1;352-363:1;370-414:1;420-438:1;443-462:1;467-486:1;493-503:1;508-517:1")]
[assembly: GoPositionMap("go/types/hilbert_test.go", "hilbert_test.cs", "ABUqgpaUgoKCpoKmgpQAFiaCgoKCgoKU7qIADgaChoKCgoKUlIKCgoKUppQADAaChoKCgoKUppQADgaChoKCgoKClJSUlAAQBoKGgoKClJSCgpSUlKaCAAsIgoKCgoKClJSUAAwGgoaCgpSCpgAOBoKKgpQ=")]
[assembly: GoPositionMap("go/types/instantiate_test.go", "instantiate_test.cs", "ABsegoIAT74BspSilIKCgqaCgoKmggAJDIKCgqaCgoKCgpSCgpSClIIAFAiCjgAJGoKCgoKCgpSCABUKggAAEIKCgoLKgoK6koKUgqaClA==", "115-133:1", "", "30=NewSlice/1/4/22,NewSlice/2/4/23,NewFunc/1/2/30,NewInterfaceType/1/5/30,NewFunc/2/2/36,NewInterfaceType/2/5/36,NewInterfaceType/3/5/33,NewTerm/1/4/47,NewTerm/2/4/47,NewUnion/1/2/47,NewInterfaceType/4/5/46,NewTerm/3/4/52,NewTerm/4/4/52,NewUnion/2/2/52,NewInterfaceType/5/5/51,NewSlice/3/4/65,NewSlice/4/4/66")]
[assembly: GoPositionMap("go/types/issues_test.go", "issues_test.cs", "ACAugoKCggATCIIAABaClIKClKS0tLS01oK2ggARCoIAABCClIKCgoKClLiCAA0IgoqUgoKCgqaCgpSCACQO0gACFoKUAAIYgJKCgoKCpoKCgoKmgoKUlIKCAEQU4pQAABoAASIAAh6CgpSCgoKCgqaClIK4goLmgpSCgJKCjIL4gqYACBKUkoKCgoK4koKCgoK4+qbekoK4AAoSgoKokoKCgqiSgoKCpoKUuIKCgoIACwyUgoKUgoKCpoKClIKCAAkIgoKmgoKCgqYABhCSgoKAgqSmgoKAgqSAgqSUggALCIKClJSCAAcQgoKUgpQAAhIACAKCgrjKgoKCtKQACRSCgoKCgoKCgqaCAAwGgpSCgoKmgoKUlIKCggA/CKaaAAhcgtySpoKCAAwGgoKUgpSCpoKUgqaClIK4pqKCgoKClIK4goKiyKamgoK4goK4goKCgoLKpoKCuIKCAAgIigASJIKCgpSCgpSCgoKUgqaCyoIACwqEAIcByAGCkoKCgoKCgoKAgoKCtoIARgiClgACHgABUIIAFQiCAAAUgoKCpoIADQaCjpKmkgAXBoIAARympoKCqJKCgoKmgriCABUIgp6ClIKCpoKCggAQCIaimKKUACFKlIKCgoKUlIK4gqaCABMKggACGIIAFAaCAAEcgqSklISCABQIggABEoKCgoKCggAQCIKcgoKSgqaCpoKClII=", "155-155:1;236-254:1;265-265:1;293-293:1;302-312:2;406-414:1;477-481:1;529-534:1;603-605:1;641-656:1;643-651:1.1;859-872:1;937-939:1;975-975:1;1154-1154:1;1194-1194:1", "", "757=NewFileSet/1/1/3,defaultImporter/1/1/3", "", "604:21,605:13,816:29,817:21")]
[assembly: GoPositionMap("go/types/main_test.go", "main_test.cs", "ABEigoI=")]
[assembly: GoPositionMap("go/types/methodset_test.go", "methodset_test.cs", "ABgkggABEAAvYAAMGoKUgoKClIKCgqaCgJKCpIKCgJKkgIKkgILagqaCABkKkgABHIKCgpSCgqaCABAGgpyCgoKmgoKCuIKCpoKCgqaCgoI=", "88-119:1")]
[assembly: GoPositionMap("go/types/mono_test.go", "mono_test.cs", "ABEegpSCkraCgpSmgoKCyoKCgpTKAAoY", "20-20:1", "", "22=defaultImporter/1/1/2")]
[assembly: GoPositionMap("go/types/named_test.go", "named_test.cs", "ADUeggAAJpTOAAYUkrKkgoKC3JKypIKCgu6igoKUABgIkgABJIKCgoKmmIKCgoKUggARDrIAABSSlIKmgoK4lIKCuICCgqSCgg==", "54-65:1;56-63:1.1;67-78:2;69-76:2.1;116-118:1")]
[assembly: GoPositionMap("go/types/object_test.go", "object_test.cs", "AB0ggoKCyoKCgsqCgoKCggALHvyigqaCgoK4goKCuIIACQgAFzqClLKSgqaCgoKmgoKmgoKUgqaCgoKCpriC3KKCgoKm", "17-21:1;115-153:1")]
[assembly: GoPositionMap("go/types/resolver_test.go", "resolver_test.cs", "ABkwgqaCgpSCgpSCgpSCAAgGopQATIYB3IKCgriCgoKCgoK4goLKgqKCgoKCgpSCgpSUlLiCgrqSgrKCgoKClIKClIKCgpSUyoKSuIKUgg==", "145-162:1;174-193:2")]
[assembly: GoPositionMap("go/types/scope2_test.go", "scope2_test.cs", "AEkqooKCgoKygoKCuO4AAVaCgoKCgoKUgoKUlLaCgpSUgoKClIIACRSCgoKU7oKmgoSSgtyCppQABxA=", "26-32:1;143-153:2", "", "111=Pos/1/1/1,Position/1/1/1;121=Pos/1/1/1,Position/1/1/1;131=Pos/1/1/1,Position/1/1/1;144=Pos/1/1/1,Position/1/1/1;162=ScopeComment/1/1/2,Pos/1/1/3,End/1/1/4,Names/1/1/5;174=Pos/1/2/1,Position/1/1/1,ScopeComment/1/1/2,Pos/2/2/4,ObjectScopePos/1/1/7")]
[assembly: GoPositionMap("go/types/self_test.go", "self_test.cs", "ABoogpSCgoKmgoKCABEIgpTugoKCgoKUgoKUggALEKKCgoKmgoKCpoKCgriCggAIEoKmgqaigoKmgoKCgpSm", "46-62:1;53-60:1.1;54-56:1.1.1;57-59:1.1.2;74-77:1", "", "109=defaultImporter/1/1/2")]
[assembly: GoPositionMap("go/types/sizes_test.go", "sizes_test.cs", "ABEkkqaCgoKCgqaCABEIkgAAFIKCgIKkgoCCABEKsgAAEIKClIKUgoIADwiCjoLKgoKCABMKkpQAARiCgpLKgpSCpoKCAAoWADFUgoKykoLK", "119-136:1;190-198:1", "", "114=NewFileSet/1/1/2,defaultImporter/1/1/2;152=NewFileSet/1/1/2,defaultImporter/1/1/2,SizesFor/1/1/3;236=NewFileSet/1/1/2,defaultImporter/1/1/2,SizesFor/1/1/3")]
[assembly: GoPositionMap("go/types/stdlib_test.go", "stdlib_test.cs", "AC1Q9IKCpqaCgpKmygAHEIKUgoKCwpKCpoKCtLaUggASKIKmgpSmgoKmgqaUruKCgpSmgoKCgsqUpoKUAAgQ8oKClKSClIKCgoKCpoKUgoKUgtoAEQqigriCgqamgoKmgpSCuIKCgoKClIKktIKCyoKUgoL+goK4poKCpoIAFQyClIKmAFYWgpSCpgArUoKUuAASILKWkoKCgqamgoKCgpSokpS4goKCgtyCgoKCgoKUyuiSgoKCgoKUlIKUppSCgpSCgqamgoIABxCCgoKC3IKCgpSCuIKC", "52-54:1;75-85:2;76-79:2.1;402-404:1", "", "", "208:21", "94:29,95:21")]
[assembly: GoPositionMap("go/types/typestring_test.go", "typestring_test.cs", "ABYqkrgAUp4B7pSUgoKUgoKCgoKUgoKClIKCABAKgoKUggAKGJKClJSC", "156-161:1", "", "29=dup/1/36/2,dup/2/36/3,dup/3/36/4,dup/4/36/7,dup/5/36/10,dup/6/36/11,dup/7/36/14,dup/8/36/15,dup/9/36/26,dup/10/36/27,dup/11/36/28,dup/12/36/31,dup/13/36/32,dup/14/36/35,dup/15/36/38,dup/16/36/40,dup/17/36/43,dup/18/36/44,dup/19/36/45,dup/20/36/48,dup/21/36/49,dup/22/36/50,dup/23/36/54,dup/24/36/55,dup/25/36/56,dup/26/36/57,dup/27/36/58,dup/28/36/59,dup/29/36/60,dup/30/36/66,dup/31/36/70,dup/32/36/71,dup/33/36/72,dup/34/36/73,dup/35/36/74,dup/36/36/75;112=dup/1/2/2,dup/2/2/3")]
// </GoSourcePositionMaps>

namespace go.go;

[GoPackage("types_test")]
public static partial class types_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface TestInstanceInfo_typeᴛ1 {}
    internal partial struct BenchmarkNamed_tests {}
    internal partial struct TestAssignableTo_type {}
    internal partial struct TestCompositeLitTypes_type {}
    internal partial struct TestConvertibleTo_type {}
    internal partial struct TestDefsInfo_type {}
    internal partial struct TestFileVersions_type {}
    internal partial struct TestIdenticalUnions_type {}
    internal partial struct TestIdentical_issue15173_type {}
    internal partial struct TestIdentical_tests {}
    internal partial struct TestImplements_tests {}
    internal partial struct TestImplicitsInfo_type {}
    internal partial struct TestInitOrderInfo_type {}
    [GoLocalName("testInst")] internal partial struct TestInstanceInfo_testInst {}
    internal partial struct TestInstanceInfo_type {}
    internal partial struct TestInstantiateEquality_tests {}
    internal partial struct TestInstantiateErrors_tests {}
    internal partial struct TestInstantiatedObjects_tests {}
    internal partial struct TestIsAlias_type {}
    internal partial struct TestIssue51093_type {}
    internal partial struct TestIssue54258_tests {}
    internal partial struct TestIssue59831_tests {}
    internal partial struct TestLookupFieldOrMethod_type {}
    internal partial struct TestMethodInstantiation_tests {}
    internal partial struct TestMultiFileInitOrder_type {}
    [GoLocalName("method")] internal partial struct TestNewMethodSet_method {}
    internal partial struct TestPkgNameOf_type {}
    internal partial struct TestPredicatesInfo_type {}
    internal partial struct TestQualifiedTypeString_type {}
    internal partial struct TestScopesInfo_type {}
    internal partial struct TestTooNew_type {}
    internal partial struct TestTypesInfo_type {}
    internal partial struct TestUsesInfo_type {}
    internal partial struct TestValuesInfo_type {}
    internal partial struct builtinCallsᴛ1 {}
    internal partial struct comment {}
    internal partial struct futurePackage {}
    internal partial struct gcSizeTest {}
    internal partial struct gen {}
    internal partial struct importHelper {}
    internal partial struct recordedInstance {}
    internal partial struct renameMap {}
    internal partial struct resolveTestImporter {}
    internal partial struct stdlibChecker {}
    internal partial struct testEntry {}
    internal partial struct testImporter {}
    internal partial struct testObjectsᴛ1 {}
    internal partial struct walker {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸflag() => builtin.initPackage(typeof(flag_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸast() => builtin.initPackage(typeof(global::go.go.ast_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸbuild() => builtin.initPackage(typeof(global::go.go.build_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸformat() => builtin.initPackage(typeof(global::go.go.format_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸimporter() => builtin.initPackage(typeof(global::go.go.importer_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸparser() => builtin.initPackage(typeof(global::go.go.parser_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸscanner() => builtin.initPackage(typeof(global::go.go.scanner_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸtoken() => builtin.initPackage(typeof(global::go.go.token_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸtypes() => builtin.initPackage(typeof(global::go.go.types_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸbuildcfg() => builtin.initPackage(typeof(global::go.@internal.buildcfg_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸdiff() => builtin.initPackage(typeof(global::go.@internal.diff_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸgodebug() => builtin.initPackage(typeof(global::go.@internal.godebug_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtestenv() => builtin.initPackage(typeof(global::go.@internal.testenv_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtypesꓸerrors() => builtin.initPackage(typeof(global::go.@internal.types.errors_package));
    [GoInit] internal static void initᴛᴛimportꓸlog() => builtin.initPackage(typeof(log_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸpath() => builtin.initPackage(typeof(path_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(global::go.path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸregexp() => builtin.initPackage(typeof(regexp_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸsort() => builtin.initPackage(typeof(sort_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.go.types_package));
    }
}
