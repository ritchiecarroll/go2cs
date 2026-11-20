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
using static go.github.com.itchyny.gojq_package;

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
[assembly: GoDynamicTypeLift("696e746572666163657b4c6f6164496e69744d6f64756c6573282920285b5d2a6769746875622e636f6d2f69746368796e792f676f6a712e51756572792c206572726f72297d", "Compile_type")]
[assembly: GoDynamicTypeLift("696e746572666163657b4c6f61644a534f4e28737472696e67292028616e792c206572726f72297d", "compileImport_typeᴛ1")]
[assembly: GoDynamicTypeLift("696e746572666163657b4c6f61644a534f4e576974684d65746128737472696e672c206d61705b737472696e675d616e79292028616e792c206572726f72297d", "compileImport_type")]
[assembly: GoDynamicTypeLift("696e746572666163657b4c6f61644d6f64756c6528737472696e672920282a6769746875622e636f6d2f69746368796e792f676f6a712e51756572792c206572726f72297d", "compileImport_typeᴛ3")]
[assembly: GoDynamicTypeLift("696e746572666163657b4c6f61644d6f64756c65576974684d65746128737472696e672c206d61705b737472696e675d616e792920282a6769746875622e636f6d2f69746368796e792f676f6a712e51756572792c206572726f72297d", "compileImport_typeᴛ2")]
[assembly: GoDynamicTypeLift("696e746572666163657b696f2e5772697465723b20696f2e427974655772697465723b20696f2e537472696e675772697465727d", "encoder_w")]
[assembly: GoDynamicTypeLift("7374727563747b737461746520696e743b20746f6b656e20696e743b206d736720737472696e677d", "yyErrorMessagesᴛ1")]
[assembly: GoTypeAlias("ModuleLoader", "object")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement(typeof(sliceIter<>), typeof(Iter), Pointer = true)]
[assembly: GoImplement<HaltError, ValueError>(Pointer = true)]
[assembly: GoImplement<ParseError, error>(Pointer = true)]
[assembly: GoImplement<arrayIndexNegativeError, error>(Pointer = true)]
[assembly: GoImplement<arrayIndexNotNumberError, error>(Pointer = true)]
[assembly: GoImplement<arrayIndexTooLargeError, error>(Pointer = true)]
[assembly: GoImplement<breakError, error>(Pointer = true)]
[assembly: GoImplement<bytes_package.Buffer, encoder_w>(Pointer = true)]
[assembly: GoImplement<emptyIter, Iter>]
[assembly: GoImplement<env, Iter>(Pointer = true)]
[assembly: GoImplement<expectedArrayError, error>(Pointer = true)]
[assembly: GoImplement<expectedObjectError, error>(Pointer = true)]
[assembly: GoImplement<expectedStartEndError, error>(Pointer = true)]
[assembly: GoImplement<func0TypeError, error>(Pointer = true)]
[assembly: GoImplement<func1TypeError, error>(Pointer = true)]
[assembly: GoImplement<func1WrapError, error>(Pointer = true)]
[assembly: GoImplement<funcNotFoundError, error>(Pointer = true)]
[assembly: GoImplement<inputNotAllowedError, error>(Pointer = true)]
[assembly: GoImplement<invalidPathError, error>(Pointer = true)]
[assembly: GoImplement<invalidPathIterError, error>(Pointer = true)]
[assembly: GoImplement<iteratorError, error>(Pointer = true)]
[assembly: GoImplement<jsonParseError, error>(Pointer = true)]
[assembly: GoImplement<lengthMismatchError, error>(Pointer = true)]
[assembly: GoImplement<lexer, yyLexer>(Pointer = true)]
[assembly: GoImplement<limitedWriter, encoder_w>(Pointer = true)]
[assembly: GoImplement<objectKeyNotStringError, error>(Pointer = true)]
[assembly: GoImplement<os_package.File, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<queryParseError, error>(Pointer = true)]
[assembly: GoImplement<strings_package.Builder, encoder_w>(Pointer = true)]
[assembly: GoImplement<strings_package.Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<timeArrayError, error>(Pointer = true)]
[assembly: GoImplement<tryEndError, error>(Pointer = true)]
[assembly: GoImplement<unitIter, Iter>(Pointer = true)]
[assembly: GoImplement<variableNameError, error>(Pointer = true)]
[assembly: GoImplement<variableNotFoundError, error>(Pointer = true)]
[assembly: GoImplement<yyParserImpl, yyParser>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
// </ImplicitConversions>

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: GoFieldAccess(typeof(ConstObject))]
[assembly: GoFieldAccess(typeof(ConstObjectKeyVal))]
[assembly: GoFieldAccess(typeof(ConstTerm))]
[assembly: GoFieldAccess(typeof(FuncDef))]
[assembly: GoFieldAccess(typeof(IfElif))]
[assembly: GoFieldAccess(typeof(Import))]
[assembly: GoFieldAccess(typeof(PatternObject))]
[assembly: GoFieldAccess(typeof(Query))]
[assembly: GoFieldAccess(typeof(Suffix))]
[assembly: GoFieldAccess(typeof(Term))]
[assembly: GoFieldAccess(typeof(breakError))]
[assembly: GoFieldAccess(typeof(code))]
[assembly: GoFieldAccess(typeof(compiler))]
[assembly: GoFieldAccess(typeof(funcBuiltins_funcNameArity))]
[assembly: GoFieldAccess(typeof(funcinfo))]
[assembly: GoFieldAccess(typeof(go.github.com.itchyny.gojq_package.Func))]
[assembly: GoFieldAccess(typeof(go.github.com.itchyny.gojq_package.Index))]
[assembly: GoFieldAccess(typeof(go.github.com.itchyny.gojq_package.String))]
[assembly: GoFieldAccess(typeof(lexer))]
[assembly: GoFieldAccess(typeof(listModuleDefs_funcNameArity))]
[assembly: GoFieldAccess(typeof(scopeStack))]
[assembly: GoFieldAccess(typeof(scopeinfo))]
[assembly: GoFieldAccess(typeof(sortItem))]
[assembly: GoFieldAccess(typeof(tryEndError))]
[assembly: GoFieldAccess(typeof(varinfo))]
[assembly: GoFieldAccess(typeof(yyParserImpl))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("github.com/itchyny/gojq@v0.12.19/builtin.go", "builtin.cs", "yoI=")]
[assembly: GoPositionMap("github.com/itchyny/gojq@v0.12.19/code.go", "code.cs", "AC5WgpSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKQ=")]
[assembly: GoPositionMap("github.com/itchyny/gojq@v0.12.19/compare.go", "compare.cs", "AAsagqaUpKTsgoKmpoKClIKCpgAEEIKmgqSkgpTkpKSk", "16-25:1;28-35:2;36-47:3;48-50:4")]
[assembly: GoPositionMap("github.com/itchyny/gojq@v0.12.19/compiler.go", "compiler.cs", "ACZOoqiygoKClAAcMKKCgpSCgoKWsoKUgpSChoKClIKCyoCCpIKCggAIDrKCgqaAgqSCABYGspKCgpSUgpSCgoaClIaCppSCgoKClIKAhoKUhoK2goKUgqbSgoKSgpSCkoK4goKmgoKm1oKmgoKCgqamgoKCgoKmgoKCgoK4poKCgoKCgriCgrimgoKCgqamooaCqJKCuIKCgqaCgpKCkoKCAAwI4oKClJSGgoKClIKCgpSUloKKgoKCgoKCgpSCpqKCgoKCgoKUlIKUgtaigoKmgpSUpIKUgpSkpKakAA0YAA0Y7qKGgpSGgtbCgoKChoKUgoKCgoaCgoKCgoKCAAgGspiEgoKCgpSCgoKUpO6CgoCCpIIAFSrCgoKCgIKkgoKCgoKCmIKygqaClIKGpoKUgpSUAAgGsoKCgoKCgoKCgrKCgoKmgoKCgoKSgoKClIKCgqaCgqaClIKElIKClIKmgoK4lLjSgoKCgoKAgqSCgpSUgoaCgIKkgoaCgpSCgoiGgoKmlNbSgoaCgpSCgoaCgoKUgtbSgoKCgoKAgqSCgoaAgqSAgqSCgoCCpIKCgoKCgtbSgoKCgoKAgqSCgoCCpICCpIKCgIKkgoKCgoKU1rKCgoLWgpKClIKCgsaCytKCgoKClJSkpIKkgqSCpKSkpKSCpKSkpKSkpKSkpIKkAAgIsoKClIKClIKClIKUgpSClKaygoKCgoKClIKUgoKCgoKCuIKCgqaCgoKCyoCCpIKCgoKClIKmgqS0tMaCpoCCpIKkgoCCpIKk7oKU7u6GgIKkgIKClKSCgqTuxoCCzJSClKQABhTCgoKytLIAI0bowoKCsrKyAC9eqMKCgtIAFSoACwSCioKCgriygqKC2rKCooLamIKClKaCgoKUpoKCgpSClIKChoKUhoKmgoKUgoIACQaCioKCppiCgpSmgoKCgoKUgoKUgoKUgpTGgqbSgoKClIKCgoKCgqaEgoKUgoamgoKUgoLWwoKCgpSCgqaCgoKmgoKCgoKmgoKUgoKChKaCgoKClJSCgoKmptKCgoKUgoKCgoaCgpSCgoKCgoKWgpSCgoamgoKUgoLWsoKCgpSClJSkpMiCgoLKgpTGgpSkpKSkpKSkpKSk6MKCgpSClIKigpSClKamwoKCgoKUgoKCgoSCgpSmlLiigoKUtLS0zJSClKaipqaCgpSCgoKUgoKCgpSClIKCgoKCtIKCgpSCgpSCgrSCgsaUgoKClLiClJSCpoKmoqaigoKAtqKCgoKClIKCxoKAhIKAgrSCpKSCgpSUpOiClAAICoKCgpSUgrSC5oKCgILW", "82-84:1;192-195:1;197-201:2;280-282:1;288-291:2;304-308:1;318-320:1;323-325:2;333-335:3;433-435:1;439-441:2;450-452:1;460-462:2;540-542:1;554-556:2;654-656:1;662-664:2;671-682:3;690-692:1;699-701:2;722-724:1;796-798:1;986-988:1;1197-1200:1;1259-1262:1;1396-1398:1;1646-1646:1", "", "498=getFunc/1/1/1;574=getFunc/1/1/2;589=getFunc/1/1/8;1318=funcBreak/1/1/36", "", "236:27,237:13,242:31,243:17,386:73,387:13,777:25,778:17")]
[assembly: GoPositionMap("github.com/itchyny/gojq@v0.12.19/encoder.go", "encoder.cs", "ABMygoKCpoKCgqaCABAYgpS0gpTGtLS0tLS0xMqCgoKUgoKClIKEgoKmqIKCgoKCgoKUgpSUtLS0tLS0tIKCgrSCgpSCgoKUgoKClJSClKaCgoKClJQACwaCgoqCgoKUloKClIKClA==", "180-182:1")]
[assembly: GoPositionMap("github.com/itchyny/gojq@v0.12.19/env.go", "env.cs", "ABcsgg==", "", "", "25=newStack/1/2/1,newStack/2/2/2,newScopeStack/1/1/3")]
[assembly: GoPositionMap("github.com/itchyny/gojq@v0.12.19/error.go", "error.cs", "ABMkggAKDoIACg6CAAoOggAKDoIACxCCAAoOggAKDoIACg6CAAoOggAJCoIACQqCAAoOggALEIIACxCCAAsQggAMEoIADBKCAAwSggALEIKClKaCpoIACBiCqoKoggAKDoIACQqCAAsQggALEIIACg6CAAoOggAKDoIACxCCAAkKggAKDoIACg6CAAoOggALEIKmggAKDoIACg6CAAoOggALEIKmggALEIKmgsaClKSk")]
[assembly: GoPositionMap("github.com/itchyny/gojq@v0.12.19/execute.go", "execute.cs", "AA0UwoKCgoKUgqbCgqKCgJKCgoKCgqSC2Mi0tIKCtIK0tLSClIKCgoKCgoKUgqa0grSCgpSClLSCgpSUgtS0gsSCpIKUtIKClJS0goKUgpS0goKClJSCgoKCtLSCtICCgtaClIKCgoKmgoCCgqSCgoKClMaClJSCtIKCgpSCgoKUgoKkgoKUtIKClOqCgpSCAAkK1oK0tIKCtIKSgoKUppSCgqaCgoKCgsaClIKCxoKUgoKktIKClIKUgoLGgoKUgpSCgoKClLiCgoKCgoKUgpTEgoKkgoKClIKCxrS0goK0gpSCgIKCpIK05oKClIKU1oKmgqaCgoKClKaCgoKCgoKmgoKChIKCgqaigoKClJQACg6CgrSkgtaCtqaigoKCgpSUgpQ=", "25-25:1;298-300:2", "", "", "", "39:21,40:13")]
[assembly: GoPositionMap("github.com/itchyny/gojq@v0.12.19/func.go", "func.cs", "AC9YguqCAKEBxAKCzoLOgs6CzoKCgoKUuIKCgoKUgoKUuIKCgoKUgoKUgoKUuIKUgpSkpIKUpIKUtLiClKSClKSkgpSkgpSkpKS0uIKCgpSmgpSCgpSkgoKUtLiCgoKCgpSCpoKUpIKClLS4gpSCxoKCxpSmgoKClKaCgoKUtJSCgoK0gtaUgoKCtILWlIK0gsaAgqSCgqaAgqSmgpS0lKSk5riC1LSClLS4gqaCgpSmgqaCgoKUgoKUpoLcgoKCgqa0poKUgoKmpoKUuoKmgqaCgoKUgoKmpoKCgpSCgqa4goKClIKCpriClKSUtNaClLS4goKClIKClKaCgoKUgoKUpoKCgpSCgpSmgoKClIKClKaCgoKUgoKUpoKCgpSmgoKClKaCgoKUpoKCgpSmgoKCgoKUpoKCgpSCgoKCgpSmpqaCgoKUgoKUgoKClKaCgoKUgpSSooKClIKU5KSC3IKCgpSCgpS4goKClIKClLiCpoKCgpSCgoKAgqSAgqSmgoKClIKCgpSmAAYQgqS0uIKktLiCpIKClLS4uoKqAAYQgqa6goKUqoKCgpSCgrSkxILIpoKktLiCpIKUgoKUtLiClJSktAAIBoKUpKS0xpSktMaClIKClIKClLSUpKTqgoKClKaCgoKCgoK4poKUpKS0uIKSgoKUpoKClKaUpoKSgoKClKaCgpSmlIKCgoK4lIKCgoK4lKaCgpSCgoKUuIKCgpSCgpSCgpSCpqaCgoKUpgAIDoKClIKCxoKC9ramgoKClKaCgoKUgoKUgpSmgoKClKaCgoKUgoKUgpSmgoKUooKCpu6CgoKClJSCgpSClIKClJamgqaCpoKCgpSCgpSmgoKClIKCgoKUpqaCpoKmgoKClIKCgoKmpoKCpoKmgoKClIKmgoKClIKUgqaCgqaCgoKUpoKClIKUpoKClIKUpoKmgqaCpoKmgoKmgoKmgqaCgoKClJSmgoKClAAEEoLGgoKmgoKClKaCgoKUxoSogqaCgoKUgoKUpoKogqaCgoKUgpqCgoKCgoKUgoKmpoKClJSUpKS0AAgGgpSkpLTGlKSktNaUpOqCgoKClJSCgpSCgpSCgoKmgoKCgoKUlIKCgpSCgpSUgqaCgpSSgoKClIKUlIKUgoKCpoKCgpSCgpSSgoKUpoKClKaUgoKUlIKClJSCgpSCgpSCpIKClIKUgpS0uIKUpIKClKakgoKCgqaClLS4goKClIKCtIKCxramgoKClIKUgoKCgpSCpoKCgoKClIKCpqaCgoKUloKUpoKClKaCgpSmgoIACxiCgoKUgoKUpoKmgoKUgoKUgoKUgoKUpoKClIKClIKClIKClKaCgoKUgoKUgoKUgpSmgoIACAS4gpSCgoKUlIKUpriCpoKCgpSUgoKCgoKmgoKUgoKUgoKUgpSCgpSUgoKCgoKCgpSC3JTu7qaCgoKUhpSClIKUgoKUgqaCgoKUgoKCgrimgoKUpoKmgoKCgoKmpoKUpKSCgqaClKS0uIKClKaCgpSClKaClKSkpIK0uKKClIKUpoKAgqSCgqaClIKU", "215-217:1;223-225:1;231-233:1;239-241:1;246-252:1;256-266:1;270-284:1;534-534:1;535-535:2;536-536:3;537-537:4;538-549:5;550-560:6;561-566:7;592-602:1;606-616:1;788-805:1;813-818:1;826-831:1;913-915:1;939-941:1;1294-1296:1;1833-1835:1;2051-2053:1", "", "57=argFunc0/1/53/1,argFunc1/1/27/2,argFunc0/2/53/3,argFunc0/3/53/4,argFunc0/4/53/5,argFunc0/5/53/6,argFunc1/2/27/7,argFunc0/6/53/8,argFunc0/7/53/9,argFunc0/8/53/10,argFunc0/9/53/11,argFunc1/3/27/12,argFunc0/10/53/13,argFunc0/11/53/14,argFunc0/12/53/15,argFunc0/13/53/16,argFunc0/14/53/17,argFunc0/15/53/18,argFunc1/4/27/19,argFunc1/5/27/20,argFunc1/6/27/21,argFunc1/7/27/22,argFunc1/8/27/23,argFunc1/9/27/24,argFunc1/10/27/25,argFunc1/11/27/26,argFunc1/12/27/27,argFunc1/13/27/28,argFunc0/16/53/29,argFunc0/17/53/30,argFunc0/18/53/31,argFunc0/19/53/32,argFunc0/20/53/33,argFunc1/14/27/34,argFunc1/15/27/35,argFunc0/21/53/36,argFunc0/22/53/37,argFunc0/23/53/38,argFunc0/24/53/39,argFunc1/16/27/40,argFunc0/25/53/41,argFunc0/26/53/42,argFunc0/27/53/43,argFunc0/28/53/44,argFunc0/29/53/45,argFunc0/30/53/46,argFunc0/31/53/47,argFunc0/32/53/48,argFunc2/1/14/49,argFunc3/1/2/50,argFunc0/33/53/51,argFunc0/34/53/52,argFunc2/2/14/53,argFunc2/3/14/54,argFunc2/4/14/55,argFunc2/5/14/56,argFunc2/6/14/57,argFunc2/7/14/58,argFunc2/8/14/59,argFunc2/9/14/60,argFunc2/10/14/61,argFunc2/11/14/62,argFunc2/12/14/63,argFunc2/13/14/64,argFunc0/35/53/67,argFunc1/17/27/68,argFunc0/36/53/69,argFunc1/18/27/70,argFunc0/37/53/71,argFunc1/19/27/72,argFunc1/20/27/73,argFunc0/38/53/74,argFunc1/21/27/75,mathFunc/1/40/76,mathFunc/2/40/77,mathFunc/3/40/78,mathFunc/4/40/79,mathFunc/5/40/80,mathFunc/6/40/81,mathFunc/7/40/82,mathFunc/8/40/83,mathFunc/9/40/84,mathFunc/10/40/85,mathFunc/11/40/86,mathFunc/12/40/87,mathFunc/13/40/88,mathFunc/14/40/89,mathFunc/15/40/90,mathFunc/16/40/91,mathFunc/17/40/92,mathFunc/18/40/93,mathFunc/19/40/94,mathFunc/20/40/95,mathFunc/21/40/96,mathFunc/22/40/97,mathFunc/23/40/98,mathFunc/24/40/99,mathFunc/25/40/100,mathFunc/26/40/101,argFunc0/39/53/102,argFunc0/40/53/103,mathFunc/27/40/104,mathFunc/28/40/105,mathFunc/29/40/106,mathFunc/30/40/107,mathFunc/31/40/108,mathFunc/32/40/109,mathFunc/33/40/110,mathFunc/34/40/111,mathFunc/35/40/112,mathFunc/36/40/113,mathFunc/37/40/114,mathFunc/38/40/115,mathFunc/39/40/116,mathFunc/40/40/117,mathFunc2/1/17/118,mathFunc2/2/17/119,mathFunc2/3/17/120,mathFunc2/4/17/121,mathFunc2/5/17/122,mathFunc2/6/17/123,mathFunc2/7/17/124,mathFunc2/8/17/125,mathFunc2/9/17/126,mathFunc2/10/17/127,mathFunc2/11/17/128,mathFunc2/12/17/129,mathFunc2/13/17/130,mathFunc2/14/17/131,mathFunc2/15/17/132,mathFunc2/16/17/133,mathFunc2/17/17/134,mathFunc3/1/1/135,argFunc0/41/53/136,argFunc0/42/53/137,argFunc0/43/53/138,argFunc0/44/53/139,argFunc0/45/53/140,argFunc0/46/53/141,argFunc2/14/14/142,argFunc1/22/27/143,argFunc1/23/27/144,argFunc0/47/53/145,argFunc1/24/27/146,argFunc0/48/53/147,argFunc0/49/53/148,argFunc0/50/53/149,argFunc1/25/27/150,argFunc1/26/27/151,argFunc1/27/27/152,argFunc0/51/53/153,argFunc3/2/2/154,argFunc0/52/53/155,argFunc0/53/53/157;2028=Year/1/1/1,Month/1/1/2,Day/1/1/3,Hour/1/1/4,Minute/1/1/5,Second/1/1/6,Nanosecond/1/1/6,Weekday/1/1/7,YearDay/1/1/8")]
[assembly: GoPositionMap("github.com/itchyny/gojq@v0.12.19/iter.go", "iter.cs", "AAoSopSkpIIACQyCAAsQgoKUggAICoKClIKC")]
[assembly: GoPositionMap("github.com/itchyny/gojq@v0.12.19/lexer.go", "lexer.cs", "ABQkgsoAGTDigJKCgpSCgoKUgoKClJSCgoKCgpSClKSCgoKClIKCpJSClIKCpIKCpIKCgoKUgoKk1oKCgoKCgpTGgoKCgsaCgoKCgpTGgoKCgsaCgoKCxoKCgoLGlIKCgqSCgoKCgpSCgtaCgoKCxoKCgoKUgoKkgoKCgsaCgoKClIKCpIKCgoKUgoKkgoKCxoKCpIKEgsbmgoKCgoKClIKCgsqCgpSkpLSC+KTqgoKUpoKClKaCgoKCgoKCgoKClKampoKClIIACxSCgpSAgpSUgoKUgrSClLS0goKU+ICCgoKUgpSkgrTqgoKUgrSCgoKClKaCgoKSgoKUlIKClJSClJSCgpSClKSCgoKCpoKkpIKClIKCgpSCgoKClKSCgtaCgoKCgpSUgoKCgoKUlIKCpIKUgtiCgsaCgoKUgoKCgoKUgoKCgoKClIKmAAoUgpSkpKSk6IKygpTGgpSkyIKqgqqC", "49-49:1;402-419:1", "", "677=isNumber/1/1/2;681=isNumber/1/1/2", "", "60:21,61:13")]
[assembly: GoPositionMap("github.com/itchyny/gojq@v0.12.19/module_loader.go", "module_loader.cs", "ABI0goKigqYACA6igrKClIKCgpSUgpSCgpSCgpSUtoKSgpSCgpSCgpSmwpKClIKClJKCgoKCgoKClIKUgoKUlJTWgoKAgoK2goKAgqSCgIK2xoKCgpSCgoKCgoKU7qaClKSCgpSkgoKUgoKUpA==", "", "", "", "104:21")]
[assembly: GoPositionMap("github.com/itchyny/gojq@v0.12.19/operator.go", "operator.cs", "ACpWgpSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKTasoCSlKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKQACQiClKSkpKSkpKSkpKSkpKSk+AAAEoCCpICCpJSUpKS0xpSkpLTGlKSktMaUtMaUtMaUtNa4gpSkpKS0uIKClKaClKSkpIKUtLiClIKUgtyClIKUgoKCpoKUgpSCgoKmgpSClLqClIKUgtyCgoKCgqa03oKUgpSClIIABxCCgqaCgqa6goKCooKCgriUpoKClIKClKaClJSkpIKUyIKUpoKUgoKUpoKUgoKClAAGEIKUlKSkyIKUgoKUpoKUAAcSgoKUpoKmgqaCpoKmgqaC", "100-100:1;309-315:1;316-316:2;317-317:3;318-318:4;319-330:5;331-342:6;343-351:7;357-363:1;364-364:2;365-365:3;366-366:4;367-379:5;380-380:6;381-381:7;387-396:1;397-397:2;398-398:3;399-399:4;400-400:5;402-414:6;447-459:1;460-465:2;466-475:3;476-486:4;487-487:5;488-488:6;489-489:7;495-504:1;505-514:2;515-520:3;521-521:4;522-522:5;523-523:6;524-524:7", "", "", "", "128:21,129:13")]
[assembly: GoPositionMap("github.com/itchyny/gojq@v0.12.19/option.go", "option.cs", "AAgUgoK+goK8gpIAAxqCAAIQgr6CgpSCgoKUgJKClLaClLgABRSCgg==", "11-13:1;20-22:1;28-30:1;53-55:1;64-84:1;74-79:1.1;93-95:1")]
[assembly: GoPositionMap("github.com/itchyny/gojq@v0.12.19/parser.go", "parser.cs", "9oKClKaCgoKCADZyAEKGAQAFkQIAEiYATJoBAB9A7gASJgASJgAfQAAfQAAPIAAHEAAJCAAW4QOCxoLKgoKCpqaCgoKmpoKUgqaCgriWloKCgoKUuIKCgqiCgoKUgpSogriCgpSUlKaygoKCgpSCgpSCgoKmgoKCgriCgpSClKaCxsKCgoKClIKCgoKChIKClIKUgpSClISCpoKCgoKUgpSCgoKUgpSCgpSCgoKCgoKClKaEgoKCqIKCgpSUgoKCpoKCpoSkgoKCgpSmloKCgoKCuoKUlqaClIKUgoLKgqaCgpSGgoKClJaCgpSClIKCqJYAAKMEgoKCgsaEgsaAgsaEgsaAgsaEgsaAgsaEgsaGgsaGgsaAgsaEgsaAgsaEgsaAgsaMgoKCxoCCxoCCxoCCxoCCxoaCxoCCxoCCxoCCxoCCxoCCxoCCxoCCxoCCxoCCxoCCxoSCxoCCxoSCxoCCxoCCxoSCxoCCxoSCxoCCxoSCxoCCxoCCxoCCxoSCxoCCxoCCxoCCgoKU2ICCxoCCxoCCxoCCxoCCxoCCxoCCxoCCxoCCxoCCxoCCxoCCxoCCxoCCxoCCxoCCxoCCxoCCxoCCxoCCxoCCxoCCxoCCxoCCxoCCxoCCxoCCxoCCxoCCxoCCxoSCxoCCxoSCxoCCxoCCgsYAABSCxoCCxoCCxoCCxoCCxoSCxoCCxoSCxoCCxoSCxoCCxoSCxoCCxoSCxoCCxoSCxoCCxoCCxoCCxoCCxo6CxoaCxoCCxoCCxoCCxoCCxoCCxoCCxoSCxoCCxoCCxoSCxoCCxoSCxoCCxoCCxoSCxoCCxoSCxoCCxg==", "168-173:1", "", "", "", "591:21,592:13")]
[assembly: GoPositionMap("github.com/itchyny/gojq@v0.12.19/preview.go", "preview.cs", "AAgagoKCgpS0tLS0goKUlKaygpKClIIADBCCgoKUpoKCgpSmgoKClKaC", "38-41:1", "", "", "", "46:21,47:13")]
[assembly: GoPositionMap("github.com/itchyny/gojq@v0.12.19/query.go", "query.cs", "AAkcgoKClAAPJIKogoKClKaCgoKmgoKCgpSClIKClIKCgoKClIKClJSClIKCuIKClKaCgpQADRaCgoKmgoKCgoKUgpSCgpQADBSCgoKmgoKCgoKCgpSUlIKCABowgoKCpoKUtLS0tLS0tLS0tLSCgoLGtLS0tLS0grSCgrSCuIKUpKSClKTIgpSCxoLGpIKCpqaCgpQACxKCgoKmgoKmgoKClAAMFIKCgqaCgoKCgoKClJSCgoKCgpSUAA4YgoKCpoKCgoKCgoKClIKCAA8agoKCpqKEgqaCpoKCgoKUgoKClIKCppS4goKCgoKUgpSSgoKClIKClIKmpoKClAALEoKCgqaCgoKCgoKUlAAMFIKCgqaCgoKUgoKCgpSCpgAKEIKCgqaCgoKUgoKClJQADRaCgoKmgoKCgoKCgoKUgoIACxKCgoKmgoKClAAMFIKCgqaCgoKUlIKCgriCgoKClLiCgpQADRaCgoKmgoKCgoKCgpSCgpQACxKCgoKmgoKCggALEoKCgqaCgoKCggAOGIKCgqaCgoKCgoKCgoIADhiCgoKmgoKCgoKCgoKCgoKUAAsSgoKCpoKCgoIAEByCgoKmgoKCgoKCgoKCgoKClLiCgoKCgoKCgoKCgoKUuIKElAAKEIKCgqaCgoKUgoKClJSoooKUgoKCgpSUAAwUgoKCpoKClJSCAAoQgoKCpoKCgoKUlKaCgoKU")]
[assembly: GoPositionMap("github.com/itchyny/gojq@v0.12.19/release.go", "release.cs", "AAsOlLSUlA==")]
[assembly: GoPositionMap("github.com/itchyny/gojq@v0.12.19/scope_stack.go", "scope_stack.cs", "ABEcgsaCgoKClLiCgoKmgqaygoKUpoI=")]
[assembly: GoPositionMap("github.com/itchyny/gojq@v0.12.19/stack.go", "stack.cs", "ABEcgsaCgoKClLiCgoKmgqaCprKCgpSmgg==")]
[assembly: GoPositionMap("github.com/itchyny/gojq@v0.12.19/term_type.go", "term_type.cs", "AB8+soCSlKSkpKSkpKSkpKSkpKSkpKSkpKSk", "32-32:1", "", "", "", "36:21,37:13")]
[assembly: GoPositionMap("github.com/itchyny/gojq@v0.12.19/type.go", "type.cs", "AAocgpSk1KSkpKQ=")]
// </GoSourcePositionMaps>

namespace go.github.com.itchyny;

[GoPackage("gojq", ImportPath = "github.com/itchyny/gojq")]
public static partial class gojq_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface Compile_type {}
    internal partial interface compileImport_type {}
    internal partial interface compileImport_typeᴛ1 {}
    internal partial interface compileImport_typeᴛ2 {}
    internal partial interface compileImport_typeᴛ3 {}
    internal partial interface encoder_w {}
    internal partial interface yyLexer {}
    internal partial interface yyParser {}
    internal partial struct allocator {}
    internal partial struct arrayIndexNegativeError {}
    internal partial struct arrayIndexNotNumberError {}
    internal partial struct arrayIndexTooLargeError {}
    internal partial struct binopTypeError {}
    internal partial struct block {}
    internal partial struct breakError {}
    internal partial struct code {}
    internal partial struct codeinfo {}
    [GoLocalName("varIndex")] [GoValueClone("index")] internal partial struct compileFuncDef_varIndex {}
    internal partial struct emptyIter {}
    [GoLocalName("keyVal")] internal partial struct encodeObject_keyVal {}
    [GoValueClone("buf")] internal partial struct encoder {}
    [GoValueClone("args")] internal partial struct env {}
    internal partial struct expectedArrayError {}
    internal partial struct expectedObjectError {}
    internal partial struct expectedStartEndError {}
    internal partial struct expectedVariableError {}
    internal partial struct flattenDepthError {}
    internal partial struct fork {}
    internal partial struct formatNotFoundError {}
    internal partial struct formatRowError {}
    internal partial struct func0TypeError {}
    internal partial struct func0WrapError {}
    internal partial struct func1TypeError {}
    internal partial struct func1WrapError {}
    internal partial struct func2TypeError {}
    internal partial struct func2WrapError {}
    [GoLocalName("funcNameArity")] internal partial struct funcBuiltins_funcNameArity {}
    internal partial struct funcNotFoundError {}
    internal partial struct funcinfo {}
    internal partial struct function {}
    internal partial struct inputNotAllowedError {}
    internal partial struct invalidPathError {}
    internal partial struct invalidPathIterError {}
    internal partial struct iteratorError {}
    internal partial struct jsonParseError {}
    internal partial struct lengthMismatchError {}
    internal partial struct lexer {}
    internal partial struct limitedWriter {}
    [GoLocalName("funcNameArity")] internal partial struct listModuleDefs_funcNameArity {}
    internal partial struct moduleLoader {}
    internal partial struct objectKeyNotStringError {}
    internal partial struct opcode {}
    internal partial struct pathValue {}
    internal partial struct queryParseError {}
    internal partial struct rangeIter {}
    internal partial struct repeatStringTooLargeError {}
    internal partial struct scope {}
    internal partial struct scopeBlock {}
    internal partial struct scopeStack {}
    internal partial struct scopeinfo {}
    internal partial struct sliceIter<T> {}
    internal partial struct sortItem {}
    internal partial struct stack {}
    internal partial struct stringIndexNotNumberError {}
    internal partial struct timeArrayError {}
    internal partial struct tooManyVariableValuesError {}
    internal partial struct tryEndError {}
    internal partial struct unaryTypeError {}
    internal partial struct unitIter {}
    internal partial struct variableNameError {}
    internal partial struct variableNotFoundError {}
    [GoValueClone("index")] internal partial struct varinfo {}
    internal partial struct yyErrorMessagesᴛ1 {}
    [GoValueClone("stack")] internal partial struct yyParserImpl {}
    internal partial struct yySymType {}
    internal partial struct zeroDivisionError {}
    internal partial struct zeroModuloError {}
    public partial interface Iter {}
    [GoLocalName("ModuleLoader")] public partial interface ModuleLoaderᴅ {}
    public partial interface ValueError {}
    public partial struct Array {}
    public partial struct Code {}
    public partial struct ConstArray {}
    public partial struct ConstObject {}
    public partial struct ConstObjectKeyVal {}
    public partial struct ConstTerm {}
    public partial struct Foreach {}
    public partial struct Func {}
    public partial struct FuncDef {}
    public partial struct HaltError {}
    public partial struct If {}
    public partial struct IfElif {}
    public partial struct Import {}
    public partial struct Index {}
    public partial struct Label {}
    public partial struct Object {}
    public partial struct ObjectKeyVal {}
    public partial struct Operator {}
    public partial struct ParseError {}
    public partial struct Pattern {}
    public partial struct PatternObject {}
    public partial struct Query {}
    public partial struct Reduce {}
    public partial struct String {}
    public partial struct Suffix {}
    public partial struct Term {}
    public partial struct TermType {}
    public partial struct Try {}
    public partial struct Unary {}
    [GoMemberRecord("moduleLoader", GoMemberFact.Descriptor, typeof(ModuleLoaderᴅ))] public partial struct compiler {}
    public partial struct exitCodeError {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸcontext() => builtin.initPackage(typeof(context_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸbase64() => builtin.initPackage(typeof(encoding.base64_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸjson() => builtin.initPackage(typeof(encoding.json_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸgithub_comꓸitchynyꓸtimefmt_go() => builtin.initPackage(typeof(go.github.com.itchyny.timefmt_go.timefmt_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸiter() => builtin.initPackage(typeof(iter_package));
    [GoInit] internal static void initᴛᴛimportꓸmaps() => builtin.initPackage(typeof(maps_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbig() => builtin.initPackage(typeof(go.math.big_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸurl() => builtin.initPackage(typeof(net.url_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸregexp() => builtin.initPackage(typeof(regexp_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸsort() => builtin.initPackage(typeof(sort_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    [GoInit] internal static void initᴛᴛimportꓸunicode() => builtin.initPackage(typeof(unicode_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(go.unicode.utf8_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(gojq_package.Array)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛArray;
[GoMethodHost(typeof(gojq_package.Code)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛCode;
[GoMethodHost(typeof(gojq_package.ConstArray)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛConstArray;
[GoMethodHost(typeof(gojq_package.ConstObject)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛConstObject;
[GoMethodHost(typeof(gojq_package.ConstObjectKeyVal)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛConstObjectKeyVal;
[GoMethodHost(typeof(gojq_package.ConstTerm)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛConstTerm;
[GoMethodHost(typeof(gojq_package.Foreach)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛForeach;
[GoMethodHost(typeof(gojq_package.Func)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛFunc;
[GoMethodHost(typeof(gojq_package.FuncDef)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛFuncDef;
[GoMethodHost(typeof(gojq_package.HaltError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛHaltError;
[GoMethodHost(typeof(gojq_package.If)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛIf;
[GoMethodHost(typeof(gojq_package.IfElif)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛIfElif;
[GoMethodHost(typeof(gojq_package.Import)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛImport;
[GoMethodHost(typeof(gojq_package.Index)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛIndex;
[GoMethodHost(typeof(gojq_package.Label)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛLabel;
[GoMethodHost(typeof(gojq_package.Object)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛObject;
[GoMethodHost(typeof(gojq_package.ObjectKeyVal)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛObjectKeyVal;
[GoMethodHost(typeof(gojq_package.Operator)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛOperator;
[GoMethodHost(typeof(gojq_package.ParseError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛParseError;
[GoMethodHost(typeof(gojq_package.Pattern)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛPattern;
[GoMethodHost(typeof(gojq_package.PatternObject)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛPatternObject;
[GoMethodHost(typeof(gojq_package.Query)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛQuery;
[GoMethodHost(typeof(gojq_package.Reduce)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛReduce;
[GoMethodHost(typeof(gojq_package.String)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛString;
[GoMethodHost(typeof(gojq_package.Suffix)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛSuffix;
[GoMethodHost(typeof(gojq_package.Term)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛTerm;
[GoMethodHost(typeof(gojq_package.TermType)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛTermType;
[GoMethodHost(typeof(gojq_package.Try)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛTry;
[GoMethodHost(typeof(gojq_package.Unary)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛUnary;
[GoMethodHost(typeof(gojq_package.allocator)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛallocator;
[GoMethodHost(typeof(gojq_package.arrayIndexNegativeError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛarrayIndexNegativeError;
[GoMethodHost(typeof(gojq_package.arrayIndexNotNumberError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛarrayIndexNotNumberError;
[GoMethodHost(typeof(gojq_package.arrayIndexTooLargeError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛarrayIndexTooLargeError;
[GoMethodHost(typeof(gojq_package.binopTypeError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛbinopTypeError;
[GoMethodHost(typeof(gojq_package.breakError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛbreakError;
[GoMethodHost(typeof(gojq_package.compiler)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛcompiler;
[GoMethodHost(typeof(gojq_package.emptyIter)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛemptyIter;
[GoMethodHost(typeof(gojq_package.encoder)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛencoder;
[GoMethodHost(typeof(gojq_package.env)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛenv;
[GoMethodHost(typeof(gojq_package.exitCodeError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛexitCodeError;
[GoMethodHost(typeof(gojq_package.expectedArrayError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛexpectedArrayError;
[GoMethodHost(typeof(gojq_package.expectedObjectError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛexpectedObjectError;
[GoMethodHost(typeof(gojq_package.expectedStartEndError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛexpectedStartEndError;
[GoMethodHost(typeof(gojq_package.expectedVariableError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛexpectedVariableError;
[GoMethodHost(typeof(gojq_package.flattenDepthError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛflattenDepthError;
[GoMethodHost(typeof(gojq_package.formatNotFoundError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛformatNotFoundError;
[GoMethodHost(typeof(gojq_package.formatRowError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛformatRowError;
[GoMethodHost(typeof(gojq_package.func0TypeError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛfunc0TypeError;
[GoMethodHost(typeof(gojq_package.func0WrapError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛfunc0WrapError;
[GoMethodHost(typeof(gojq_package.func1TypeError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛfunc1TypeError;
[GoMethodHost(typeof(gojq_package.func1WrapError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛfunc1WrapError;
[GoMethodHost(typeof(gojq_package.func2TypeError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛfunc2TypeError;
[GoMethodHost(typeof(gojq_package.func2WrapError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛfunc2WrapError;
[GoMethodHost(typeof(gojq_package.funcNotFoundError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛfuncNotFoundError;
[GoMethodHost(typeof(gojq_package.function)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛfunction;
[GoMethodHost(typeof(gojq_package.inputNotAllowedError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛinputNotAllowedError;
[GoMethodHost(typeof(gojq_package.invalidPathError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛinvalidPathError;
[GoMethodHost(typeof(gojq_package.invalidPathIterError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛinvalidPathIterError;
[GoMethodHost(typeof(gojq_package.iteratorError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛiteratorError;
[GoMethodHost(typeof(gojq_package.jsonParseError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛjsonParseError;
[GoMethodHost(typeof(gojq_package.lengthMismatchError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛlengthMismatchError;
[GoMethodHost(typeof(gojq_package.lexer)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛlexer;
[GoMethodHost(typeof(gojq_package.limitedWriter)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛlimitedWriter;
[GoMethodHost(typeof(gojq_package.moduleLoader)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛmoduleLoader;
[GoMethodHost(typeof(gojq_package.objectKeyNotStringError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛobjectKeyNotStringError;
[GoMethodHost(typeof(gojq_package.opcode)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛopcode;
[GoMethodHost(typeof(gojq_package.queryParseError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛqueryParseError;
[GoMethodHost(typeof(gojq_package.rangeIter)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛrangeIter;
[GoMethodHost(typeof(gojq_package.repeatStringTooLargeError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛrepeatStringTooLargeError;
[GoMethodHost(typeof(gojq_package.scopeStack)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛscopeStack;
[GoMethodHost(typeof(gojq_package.sliceIter<>)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛsliceIter;
[GoMethodHost(typeof(gojq_package.stack)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛstack;
[GoMethodHost(typeof(gojq_package.stringIndexNotNumberError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛstringIndexNotNumberError;
[GoMethodHost(typeof(gojq_package.timeArrayError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛtimeArrayError;
[GoMethodHost(typeof(gojq_package.tooManyVariableValuesError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛtooManyVariableValuesError;
[GoMethodHost(typeof(gojq_package.tryEndError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛtryEndError;
[GoMethodHost(typeof(gojq_package.unaryTypeError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛunaryTypeError;
[GoMethodHost(typeof(gojq_package.unitIter)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛunitIter;
[GoMethodHost(typeof(gojq_package.variableNameError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛvariableNameError;
[GoMethodHost(typeof(gojq_package.variableNotFoundError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛvariableNotFoundError;
[GoMethodHost(typeof(gojq_package.yyParserImpl)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛyyParserImpl;
[GoMethodHost(typeof(gojq_package.zeroDivisionError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛzeroDivisionError;
[GoMethodHost(typeof(gojq_package.zeroModuloError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gojqᴛzeroModuloError;
// </MethodHolders>
