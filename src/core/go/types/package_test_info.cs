// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.go.types_package;
global using static global::go.go.types_internal_test_package;

// <ImportedTypeAliases>
global using constantꓸKind = go.go.constant_package.ΔKind;
global using flagꓸErrorHandling = go.flag_package.ΔErrorHandling;
global using osꓸDirEntry = go.io.fs_package.DirEntry;
global using osꓸFileInfo = go.io.fs_package.FileInfo;
global using osꓸFileMode = go.io.fs_package.FileMode;
global using osꓸPathError = go.io.fs_package.PathError;
global using osꓸSignal = go.os_package.ΔSignal;
global using reflectꓸChanDir = go.reflect_package.ΔChanDir;
global using reflectꓸKind = go.reflect_package.ΔKind;
global using reflectꓸMethod = go.reflect_package.ΔMethod;
global using reflectꓸType = go.reflect_package.ΔType;
global using reflectꓸValue = go.reflect_package.ΔValue;
global using runtimeꓸError = go.runtime_package.ΔError;
global using scannerꓸError = go.go.scanner_package.ΔError;
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
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
[assembly: GoImplicitConv<global::go.go.types_package.Basic, ж<global::go.go.types_package.Basic>>(Indirect = true)]
[assembly: GoImplicitConv<global::go.go.types_package.Interface, ж<global::go.go.types_package.Interface>>(Indirect = true)]
[assembly: GoImplicitConv<global::go.go.types_package.Tuple, ж<global::go.go.types_package.Tuple>>(Indirect = true)]
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: global::go.GoPositionMap("go/types/api_test.go", "api_test.cs", "ACA4gqaCgoKUprSCgoLqpoKCgpTYkoKAgoKCgIKkpAAJBoIAPpwBgraGkoKCgqaCgpSWgIKCuIKAgraCAA4MpJQAsAKYBYKSgoKCgoKUgqaYkoKCgqaCgqiAggAeCoKOAJcB0AKygpKCgrKUgpSClIKE1IKAkriCgoKUhIKAgqSClIKAgraAgsqCgoKUgoKUggALGKKClIYACAaCAA8ugraGkoKCgqaCgpaAggALCqIAJV6CtoaSgoKClKaCgpaAggARCoIAABCCgtqCgoKWloKCmKKSgoKClIKUgoKAgoKCgoKUgpSUpMa2uIKUgpSClIIADQiihAAbPIK2loKCmJKClLS0xKSoggAgCoKEAAAoAAYWgoLIgoKCgqiCgoCCuIKCgpSCgpSCuoCCABAIgoKCgoKUqIKCgoKCgoKChIKUAAsGgoQAM4IBgpKWgpSCgqiCABUKooQAQooBgpKWgqiCgpS0tLS0tLS0tLTIgoIACgyiAI8BkAKCkpaCgqiCgoKCAA8MgoKCur6CgIKkgILagt6CgoKChIKAgrqSgoKmgJLsgoCCpABhBqKogoKSsoKClJYAABAAAJoBAB5CgoSChIKChIKClLiCgpS6goKCgoKkgriCABcIooKCorailo6OggATBqKCgqK2opaKjILWtIKCgoCCtqIACQSMAChggoSCgoCCgriCgoKUpoKUggAbDJIAACCCgoKAgqaCqqKC9qIADiSAggAKCqIADCCAggANCrQAFT6CgoKCgpSAggAKCrQABRKAggANCqKCgu6SgoKCgoKUlJQACBiCgoCC2t6ChLqmgoKmgoKC1oKCgoLWpoKCgIKClLbssgAPGIKCgoCCpqKCgoKUgoKUgILKgpYAGwyiAAAqgoSmgIKmgqaClLqClILGkriClLQAFw6ihAAAFIKCloKCtoKUuqaCgoKWgoKClIKCgqiCgoCCgqYACg6EkpaCgILKgoKogIIAFQiCAAAahMyCmIKCgoKyhIKCtKSEgoKUgoCSAAwMggAFFoKChISCgpaCgpaC+oK4goKUggAICIKCkpKCgoKCgpSUgoKCgoKCAC0KsgAAQqaCgpKCgpaShAASNoKSgIKkloKSsoCCpIKCgpSAgqSClIKUgpSClIKUgt6SlKSUADcGogAATIKCooSSAA0iABpAgoCCyoKCgoCCpICCAB0KggAAKoSCuoKClpKCgrqWlpYAGQaCgoaYgoKYmoKC6JSC3JKCggAKCoIAUWCCgpSEkoKCgoSCgoKClJSCAA8OogAHFIKClISCtKSCgoKCqpKCgoKmgoIAFA6SggAAEIKEkoIADQiCgo6ChJKC7MKCgoKCgoKigoKCgILEpP7CgoKCgoCCAAsMgoKU/rKCloKAgqS6goKAggAJGgAJAoKWgsyCgoKCgoI=", "46-46:1;746-754:1;758-804:2;817-819:1;989-1008:1;1173-1180:1;1634-1640:1;1808-1808:1;1811-1813:2;1837-1837:1;1840-1842:2;1861-1867:1;2110-2121:1;2176-2176:1;2216-2229:1;2331-2337:1;2431-2438:1;2505-2514:1;2571-2571:1;2602-2607:2;2611-2642:3;2697-2697:1;2700-2700:2;2794-2796:1;2804-2809:2;2964-2964:1;3032-3047:1;3037-3044:1.1;3055-3061:1;3081-3086:1", "", "52=defaultImporter/1/1/2;1895=String/1/1/1,Index/1/1/2,Sprintf/1/1/2;2297=NewTerm/1/5/1,NewTerm/2/5/2,NewTerm/3/5/3,NewTerm/4/5/4,NewTerm/5/5/5;2344=NewFunc/1/2/1,NewFunc/2/2/2;2348=NewFunc/1/1/1;2556=For/1/1/8;2650=Scope/1/2/1,Lookup/1/2/1,Type/1/2/1,Underlying/1/2/1,Scope/2/2/2,Lookup/2/2/2,Type/2/2/2,Underlying/2/2/2,Elem/1/1/2;2699=NewSlice/1/2/7,NewSlice/2/2/8;2831=Invoke/1/10/5,Underlying/1/5/5,Field/1/2/5,Lookup/1/1/6,Type/1/1/6,Underlying/2/5/6,Field/2/2/6,Invoke/2/10/9,Method/1/5/9,Invoke/3/10/10,Method/2/5/10,Signature/1/3/10,Recv/1/1/10,Invoke/4/10/11,Method/3/5/11,Signature/2/3/11,Params/1/3/11,At/1/6/11,Invoke/5/10/12,Method/4/5/12,Signature/3/3/12,Results/1/3/12,At/2/6/12,Invoke/6/10/15,Underlying/3/5/15,Method/5/5/15,Invoke/7/10/18,Underlying/4/5/18,Params/2/3/18,At/3/6/18,Invoke/8/10/19,Underlying/5/5/19,Results/2/3/19,At/4/6/19,Invoke/9/10/22,Params/3/3/22,At/5/6/22,Invoke/10/10/23,Results/3/3/23,At/6/6/23")]
[assembly: global::go.GoPositionMap("go/types/builtins_test.go", "builtins_test.cs", "ACskAG6mAqKEgoKCqIKAgraCgILagoSCgpaCgoKAgoK2goKolIKCgpSAgoLKlIKCgpSCgoKUgoKUpri2kg==")]
[assembly: global::go.GoPositionMap("go/types/check_test.go", "check_test.cs", "ADxwgpaUgoKCgoKClIKCgIKCpsimgpSklKiSgpTc1JKClIKAgqSCgoKWAAIcAA4EgoKCAA8I0oKokoKClIKCgoKqkoKCooKUgoK4grSmgpiigoKCgoKAgqaCgriCqAALGIKCqIKCgIK6koKWgoKCgoKogoKCgoKCgqaCgoKUgoKmgoKClIKmlIKCuoKCgoCCupKCqICUgqa4grqCgoKCgurUgoKqooKqooKuwoKClJKCkAAHIgAOAoSCgpaCgpaCgoKUlOiCgoKmgoLusoLWhqLWooKClISCotKA0oDSgNKApIKChIKCgpaCloKUgtyChIKCgpaCgpaSuIKCgoKClJQ=", "167-180:1;360-360:1;408-410:1;430-432:1;460-462:1;481-483:1")]
[assembly: global::go.GoPositionMap("go/types/commentMap_test.go", "commentMap_test.cs", "ABU+AAwCgoSCgoSCgqSkgpSCgoKCgpS4kpSkABUKggAAEoKCgoKCgqiCgoKClKiCgg==")]
[assembly: global::go.GoPositionMap("go/types/errorcalls_test.go", "errorcalls_test.cs", "ABUssoKCgpaCkoKClIKClIK4gIKCpIKSgIKAgoK2pJTKgoCCpKaCgoKClIK0tLS0xoKClJQ=", "30-61:1;49-59:1.1")]
[assembly: global::go.GoPositionMap("go/types/eval_test.go", "eval_test.cs", "ABYugoKCgpSCgqiUgoK4goKCuoKClIK4goKCuIKCguiCAAkUgoLoggAAEABRigGCgoKCgsyUgriWkoKCloKCgoKCgoIABxCWkoIAIwaCjAAENoKCgpaSgoKWooKClriAgqSUgILWgIKkgILGloKCgoKCgoKCgpSCABIQgoqChIKCgpaCgJSAgraAgg==", "265-292:1", "", "330=Position/1/1/1")]
[assembly: global::go.GoPositionMap("go/types/example_test.go", "example_test.cs", "ACNCtIKCABMgzJKCgpqigoIAFEoAIgQAARSCgoLMkoKCqIKCgoKUqIKCABgiAA4EAAMegrragoKCqKiCgoKCgpSCgoK4lIKChIKCgoKCgoKmppSCAAd2ADcClKSkpKSClKSkyIKCgg==", "", "", "236=RelativeTo/1/1/1,ObjectString/1/1/1,Pos/1/1/2,Position/1/1/2,Join/1/1/3;255=exprString/1/1/1,mode/1/1/2")]
[assembly: global::go.GoPositionMap("go/types/exprstring_test.go", "exprstring_test.cs", "ABYcAFDWAYKCgoKClICC", "", "", "23=dup/1/75/2,dup/2/75/3,dup/3/75/4,dup/4/75/5,dup/5/75/6,dup/6/75/7,dup/7/75/8,dup/8/75/9,dup/9/75/10,dup/10/75/18,dup/11/75/19,dup/12/75/20,dup/13/75/21,dup/14/75/22,dup/15/75/23,dup/16/75/24,dup/17/75/25,dup/18/75/26,dup/19/75/28,dup/20/75/29,dup/21/75/30,dup/22/75/31,dup/23/75/34,dup/24/75/35,dup/25/75/38,dup/26/75/39,dup/27/75/40,dup/28/75/41,dup/29/75/44,dup/30/75/45,dup/31/75/46,dup/32/75/49,dup/33/75/50,dup/34/75/51,dup/35/75/53,dup/36/75/54,dup/37/75/55,dup/38/75/56,dup/39/75/57,dup/40/75/58,dup/41/75/60,dup/42/75/62,dup/43/75/63,dup/44/75/65,dup/45/75/66,dup/46/75/68,dup/47/75/69,dup/48/75/70,dup/49/75/71,dup/50/75/72,dup/51/75/73,dup/52/75/75,dup/53/75/76,dup/54/75/77,dup/55/75/79,dup/56/75/81,dup/57/75/82,dup/58/75/83,dup/59/75/84,dup/60/75/85,dup/61/75/86,dup/62/75/88,dup/63/75/89,dup/64/75/90,dup/65/75/91,dup/66/75/92,dup/67/75/93,dup/68/75/96,dup/69/75/97,dup/70/75/98,dup/71/75/99,dup/72/75/101,dup/73/75/102,dup/74/75/103,dup/75/75/104")]
[assembly: global::go.GoPositionMap("go/types/generate_test.go", "generate_test.cs", "AB40AAQU1oKCgpSCqIK4lIKCgqiWgIKokoKCgoCCpJaCgoKogIKCgoCCtgAPDqaCgtyCgoKCpoKC/saiooC2gKKkgoKmgqTIgoKkgKKAuoKCytyCgoKkgLS0gOqAAAoSgAAFFOKCgoKClJSosoCCzMKAgoKAgoKClILuwoKSlIKUurKCkpSClL7CgpKUgpS6soKSlIKUgpS8woKAgoK2qqKCkpaSlIKmtoCigsa6koKUgoCCgti+soKUlIKCgoLYgIKWkpKCgoK4koKCgriSgoKC+ryigpSAgoKAlIKSgoIACAz6koKUgIKCloCSkoKCAAkMupKClICCgpaAkpKCggAJDL6ygpaSgra6koKUkoK2upKAgqSokqiSgoKokoKCgg==", "247-254:1;260-267:1;275-282:1;288-298:1;317-337:1;342-353:1;360-404:1;410-428:1;433-452:1;457-476:1;483-493:1;498-507:1")]
[assembly: global::go.GoPositionMap("go/types/hilbert_test.go", "hilbert_test.cs", "ABUqgpaUgoKCloKmgoQAFiaCgoKCgoKE7qIACwaChoKCgoKUlIKCgoKUppQACgaChoKCgoKUppQACwaChoKCgoKClJSUlAAMBoKGgoKClJSCgpSUlJaCAAgIgoKCgoKClJSUAAoGgoaCgpSCpgAMBoKKgpQ=")]
[assembly: global::go.GoPositionMap("go/types/instantiate_test.go", "instantiate_test.cs", "ABseooIATr4BsoSihIKCgpaCgoKWgIIACQyCgoKmgoKCgoKUgoKUgpSCABMIgo4ACBqCgoKCgoKClICCABQKggAAEIKCgoK6goCCypKClJKmgpQ=", "115-133:1", "", "32=NewSlice/1/4/22,NewSlice/2/4/23,NewFunc/1/2/30,NewInterfaceType/1/5/30,NewFunc/2/2/36,NewInterfaceType/2/5/36,NewInterfaceType/3/5/33,NewTerm/1/4/47,NewTerm/2/4/47,NewUnion/1/2/47,NewInterfaceType/4/5/46,NewTerm/3/4/52,NewTerm/4/4/52,NewUnion/2/2/52,NewInterfaceType/5/5/51,NewSlice/3/4/65,NewSlice/4/4/66")]
[assembly: global::go.GoPositionMap("go/types/issues_test.go", "issues_test.cs", "AB8ugoKCggASCIIAABaChIKClJSkpKSk1oK2ggAQCoIAABCChIKCgoCCgpS4ggAMCIKKhIKCgoKWgoKEggAhDtIAAhaChAABGJCSgoKCgIKmgoKCgqaCgpSEgoIAPhTihAAAGgABIgACHoKChIKCgoKCpoKUgqiCgtaChIKQkoKMguiCpgAIEoSigoKCgIK4koCCgIKCgILo+qbOkoKoAAoSgoKYkoKCgpiSgoKCpoKUqIKCgoIACQyUgoKUgoKCpoKClIKC+IKCloKCgoCCtgAEEJKCgoCCpJaCgoCCpICCpISCAAkIgoKEhJIABxCCgpSClAACEgAIAoKAgri6goKStKQACRSCgoKCgoKCgpaCAAgGgoSCgoKWgoKUlIKCggA7CLaKAAlcgtySloKCAAsGgoKEgpSCloKUgpaClIK4lIKCgrqWloKCqIKCqIKCgoKCAAgIigAQJIKCgoSCgoCUgoKClICCtoLKggALCqQAhwHIAYKSgoKCkoKCkoCCkoK2ggBDCIKGAAEeAAFQggAUCIIAABSCgoKWggAMBoKOkpaSABYGggAAHLaWgoKYkoKCgqaCqIIAEwiCjoKEgoKWgoKCAA4IpqKIooQAG0qUkoKCgpSUgqiCloIAEgqCAAIYkgATBoIAAByCxKSEhIIAEgiCAAASgoKCgoKCAA8IgoyCgqKSloKWgoKUgg==", "155-155:1;236-254:1;265-265:1;293-293:1;302-312:2;406-414:1;477-481:1;529-534:1;603-605:1;640-644:1;831-844:1;909-911:1;947-947:1;1126-1126:1;1166-1166:1", "", "712=NewFileSet/1/1/3,defaultImporter/1/1/3")]
[assembly: global::go.GoPositionMap("go/types/main_test.go", "main_test.cs", "ABEigoI=")]
[assembly: global::go.GoPositionMap("go/types/methodset_test.go", "methodset_test.cs", "ABgkogABEAAnYAAJGoKEgoKClIKCgpaCgJKCpIKCgJKkgIKkgILKgpaCABcKkgAAHIKCgpSCgIKmggAPBoKMgoKCloKCgqiCgqaCgoKmgoKC", "88-119:1")]
[assembly: global::go.GoPositionMap("go/types/mono_test.go", "mono_test.cs", "ABEegoSCosaCgpSmgoKAgtqCgoCClNoAChg=", "20-20:1", "", "21=defaultImporter/1/1/2")]
[assembly: global::go.GoPositionMap("go/types/named_test.go", "named_test.cs", "ADEeggAAJoS+AAUUkrKkgoKCzJKypIKCgu6igoKUABcIkgAAJIKCgoCCpoiCgoKClIIAEA7SAAAUkpSCloKSqJSCgriAgoKkgoCC", "54-65:1;56-63:1.1;67-78:2;69-76:2.1;116-118:1")]
[assembly: global::go.GoPositionMap("go/types/object_test.go", "object_test.cs", "ABsggoKAgsqCgoCCyoKCgoKCABUe7KKCloKCgqiCgoKoggALCAAWPIKEsrKCgpKWgoKCloKCloKClIKWgoCCgoKmuICCxMiigoKCpg==", "17-21:1;116-156:1")]
[assembly: global::go.GoPositionMap("go/types/resolver_test.go", "resolver_test.cs", "ABkwgqaCgpSCgpSCgpSC5qKEAEuGAcyCgoKogpKCgoKCqIKCuoKigIKAgoKCgpSAgoKkpKSogoKqkoKygIKCgIKCpICCgqSCkoKUpLqCgJK4gpSC", "145-162:1;174-193:2")]
[assembly: global::go.GoPositionMap("go/types/scope2_test.go", "scope2_test.cs", "AEUqwoKCkoKygoKCqO4AAFaCgoKCgoKUgoKUlKaCgpSUgoCCgqSCAAcUgoKClN6CloKEkoLcgqaEAAcQ", "26-32:1;143-153:2", "", "108=Pos/1/1/1,Position/1/1/1;117=Pos/1/1/1,Position/1/1/1;129=Pos/1/1/1,Position/1/1/1;140=Pos/1/1/1,Position/1/1/1;156=ScopeComment/1/1/2,Pos/1/1/3,End/1/1/4,Names/1/1/5;167=Pos/1/2/1,Position/1/1/1,ScopeComment/1/1/2,Pos/2/2/4,ObjectScopePos/1/1/7")]
[assembly: global::go.GoPositionMap("go/types/self_test.go", "self_test.cs", "ABkogoSCgoKWkoKCAAsIgoTugoKCgoKUgoKUggAKEKKCgoKmgoKCloKCgsiCggAIEoCCtoKmgoKCloKCgoKUlg==", "46-62:1;53-60:1.1;54-56:1.1.1;57-59:1.1.2;74-77:1", "", "98=defaultImporter/1/1/2")]
[assembly: global::go.GoPositionMap("go/types/sizes_test.go", "sizes_test.cs", "ABEkkqaCgoKCgIK2ggAQCJIAABSCkoCCpIKAggAQCpIAABCCgpSClIKCAA4Igo6S2oKCggASCpKEAAAYgoKS2oKUgpaCggAKFgAwVIKCgpKSgto=", "119-136:1;191-199:1", "", "113=NewFileSet/1/1/2,defaultImporter/1/1/2;149=NewFileSet/1/1/2,defaultImporter/1/1/2,SizesFor/1/1/3;231=NewFileSet/1/1/2,defaultImporter/1/1/2,SizesFor/1/1/3")]
[assembly: global::go.GoPositionMap("go/types/stdlib_test.go", "stdlib_test.cs", "AC9Q5KKClpaCgpKWugAGEIKEgoSCgsKSgpaCgrSmhIIAEiiCpoKUloKCloKmlK7igoKUpoKCgoLKlKaClAAGEPKCgpSUgoSCgpKCgpaSlIKClIK6AA8KwoK4gIKAgsaWgoKWgpSCqIKCgoCCgpSClKSCgsqClIKC/oKCyJaCgqaCAA4MooSClgAyFrKEgpYAO0qChLgADxqShpKCgoKWloKCgoKUmJKkuJKCgoK8goKCgoKClLrYkpKCgoKAgqSUgpSmlIKClIKCpqaCggAHEIKCgoK8goKClIKogoI=", "52-54:1;77-87:2;78-81:2.1;397-399:1")]
[assembly: global::go.GoPositionMap("go/types/typestring_test.go", "typestring_test.cs", "ABUqkgAPCAA8ngHelISCgoSCgoKCgpSCgoKUgoCCAA8KgoKEggAKGJKClJSAgg==", "156-161:1", "", "40=dup/1/36/2,dup/2/36/3,dup/3/36/4,dup/4/36/7,dup/5/36/10,dup/6/36/11,dup/7/36/14,dup/8/36/15,dup/9/36/26,dup/10/36/27,dup/11/36/28,dup/12/36/31,dup/13/36/32,dup/14/36/35,dup/15/36/38,dup/16/36/40,dup/17/36/43,dup/18/36/44,dup/19/36/45,dup/20/36/48,dup/21/36/49,dup/22/36/50,dup/23/36/54,dup/24/36/55,dup/25/36/56,dup/26/36/57,dup/27/36/58,dup/28/36/59,dup/29/36/60,dup/30/36/66,dup/31/36/70,dup/32/36/71,dup/33/36/72,dup/34/36/73,dup/35/36/74,dup/36/36/75;101=dup/1/2/2,dup/2/2/3")]
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
