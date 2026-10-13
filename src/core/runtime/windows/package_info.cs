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
global using abiꓸArrayType = go.@internal.abi_package.ΔArrayType;
global using abiꓸChanDir = go.@internal.abi_package.ΔChanDir;
global using abiꓸFuncType = go.@internal.abi_package.ΔFuncType;
global using abiꓸInterfaceType = go.@internal.abi_package.ΔInterfaceType;
global using abiꓸKind = go.@internal.abi_package.ΔKind;
global using abiꓸName = go.@internal.abi_package.ΔName;
global using abiꓸStructType = go.@internal.abi_package.ΔStructType;
global using mapsꓸMap = go.@internal.runtime.maps_package.ΔMap;
using abi = go.@internal.abi_package;
// </ImportedTypeAliases>

using go;
using static go.runtime_package;

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
[assembly: GoDynamicTypeLift("696e746572666163657b4628297d", "ifaceHash_i")]
[assembly: GoDynamicTypeLift("7374727563747b53697a652075696e7433323b204d616c6c6f63732075696e7436343b2046726565732075696e7436347d", "MemStats_BySize")]
[assembly: GoDynamicTypeLift("7374727563747b6172656e6148696e7473202a72756e74696d652e6172656e6148696e743b2071756172616e74696e654c6973742072756e74696d652e6d5370616e4c6973743b2072656164794c6973742072756e74696d652e6d5370616e4c6973747d", "mheap_userArena")]
[assembly: GoDynamicTypeLift("7374727563747b626173652075696e747074723b20656e642075696e747074727d", "mheap_curArena")]
[assembly: GoDynamicTypeLift("7374727563747b626c6f636b20626f6f6c3b206c6f636b2072756e74696d652e6d757465783b20712072756e74696d652e6751756575657d", "workType_strongFromWeak")]
[assembly: GoDynamicTypeLift("7374727563747b63676f636865636b20696e7433323b20636c6f626265726672656520696e7433323b2064697361626c6574687020696e7433323b20646f6e74667265657a65746865776f726c6420696e7433323b206566656e636520696e7433323b206763636865636b6d61726b20696e7433323b2067637061636572747261636520696e7433323b206763736872696e6b737461636b6f666620696e7433323b20676373746f70746865776f726c6420696e7433323b206763747261636520696e7433323b20696e76616c696470747220696e7433323b206d616476646f6e746e65656420696e7433323b2072756e74696d65436f6e74656e74696f6e537461636b7320696e7465726e616c2f72756e74696d652f61746f6d69632e496e7433323b2073636176747261636520696e7433323b20736368656464657461696c20696e7433323b207363686564747261636520696e7433323b2074726163656261636b616e636573746f727320696e7433323b206173796e63707265656d70746f666620696e7433323b20686172646465636f6d6d697420696e7433323b206164617074697665737461636b737461727420696e7433323b2074726163656670756e77696e646f666620696e7433323b207472616365616476616e6365706572696f6420696e7433323b207472616365436865636b537461636b4f776e65727368697020696e7433323b2070726f66737461636b646570746820696e7433323b2064617461696e646570656e64656e7474696d696e6720696e7433323b206d616c6c6f6320626f6f6c3b20696e6974747261636520696e7433323b207362726b20696e7433323b207472616365616c6c6f636672656520696e7465726e616c2f72756e74696d652f61746f6d69632e496e7433323b2070616e69636e696c20696e7465726e616c2f72756e74696d652f61746f6d69632e496e7433323b206173796e6374696d65726368616e20696e7465726e616c2f72756e74696d652f61746f6d69632e496e7433327d", "debugᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b656e61626c656420626f6f6c3b20706164205b335d627974653b20616c69676e6d652075696e7436347d", "writeBarrierᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b66696c6c2075696e7436343b2063617061636974792075696e7436347d", "gcCPULimiterState_bucket")]
[assembly: GoDynamicTypeLift("7374727563747b676350657263656e74476f616c20696e7465726e616c2f72756e74696d652f61746f6d69632e55696e7436343b206d656d6f72794c696d6974476f616c20696e7465726e616c2f72756e74696d652f61746f6d69632e55696e7436343b2061737369737454696d6520696e7465726e616c2f72756e74696d652f61746f6d69632e496e7436343b206261636b67726f756e6454696d6520696e7465726e616c2f72756e74696d652f61746f6d69632e496e7436347d", "Δscavengeᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e6465782072756e74696d652e73636176656e6765496e6465783b2072656c6561736564426720696e7465726e616c2f72756e74696d652f61746f6d69632e55696e747074723b2072656c6561736564456167657220696e7465726e616c2f72756e74696d652f61746f6d69632e55696e747074727d", "pageAlloc_scav")]
[assembly: GoDynamicTypeLift("7374727563747b6974656d2072756e74696d652e737461636b706f6f6c4974656d3b205f205b34305d627974657d", "stackpoolᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6c656e20696e743b20627566205b3132385d2a72756e74696d652e6d7370616e7d", "p_mspancache")]
[assembly: GoDynamicTypeLift("7374727563747b6c6f636b2072756e74696d652e6d757465783b2063747874205b323030305d72756e74696d652e77696e43616c6c6261636b3b20696e646578206d61705b72756e74696d652e77696e43616c6c6261636b4b65795d696e743b206e20696e747d", "cbsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6c6f636b2072756e74696d652e6d757465783b2064617461202a627974657d", "debugPtrmaskᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6c6f636b2072756e74696d652e6d757465783b2066726565202a72756e74696d652e6763426974734172656e613b206e657874202a72756e74696d652e6763426974734172656e613b2063757272656e74202a72756e74696d652e6763426974734172656e613b2070726576696f7573202a72756e74696d652e6763426974734172656e617d", "gcBitsArenasᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6c6f636b2072756e74696d652e6d757465783b2066726565205b33355d72756e74696d652e6d5370616e4c6973747d", "stackLargeᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6c6f636b2072756e74696d652e6d757465783b20667265652072756e74696d652e6d5370616e4c6973743b20627573792072756e74696d652e6d5370616e4c6973747d", "workType_wbufSpans")]
[assembly: GoDynamicTypeLift("7374727563747b6c6f636b2072756e74696d652e6d757465783b206c6973742072756e74696d652e674c6973747d", "workType_sweepWaiters")]
[assembly: GoDynamicTypeLift("7374727563747b6c6f636b2072756e74696d652e6d757465783b206e65776d2072756e74696d652e6d75696e747074723b2077616974696e6720626f6f6c3b2077616b652072756e74696d652e6e6f74653b206861766554656d706c6174655468726561642075696e7433327d", "newmHandoffᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6c6f636b2072756e74696d652e6d757465783b206e65787420696e7433323b206d206d61705b696e7433325d756e736166652e506f696e7465723b206d696e76206d61705b756e736166652e506f696e7465725d696e7433327d", "reflectOffsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6c6f636b2072756e74696d652e6d757465783b20712072756e74696d652e6751756575657d", "workType_assistQueue")]
[assembly: GoDynamicTypeLift("7374727563747b6c6f636b2072756e74696d652e6d757465783b2072656164696e67202a72756e74696d652e74726163654275663b20656d707479202a72756e74696d652e74726163654275663b2066756c6c205b325d72756e74696d652e747261636542756651756575653b20776f726b417661696c61626c6520696e7465726e616c2f72756e74696d652f61746f6d69632e426f6f6c3b2072656164657247656e20696e7465726e616c2f72756e74696d652f61746f6d69632e55696e747074723b20666c757368656447656e20696e7465726e616c2f72756e74696d652f61746f6d69632e55696e747074723b206865616465725772697474656e20626f6f6c3b20646f6e6553656d61205b325d75696e7433323b20737461636b546162205b325d72756e74696d652e7472616365537461636b5461626c653b20737472696e67546162205b325d72756e74696d652e7472616365537472696e675461626c653b2074797065546162205b325d72756e74696d652e7472616365547970655461626c653b206370754c6f6752656164205b325d2a72756e74696d652e70726f664275663b207369676e616c4c6f636b20696e7465726e616c2f72756e74696d652f61746f6d69632e55696e7433323b206370754c6f675772697465205b325d696e7465726e616c2f72756e74696d652f61746f6d69632e506f696e7465725b72756e74696d652e70726f664275665d3b20637075536c656570202a72756e74696d652e77616b6561626c65536c6565703b206370754c6f67446f6e65203c2d6368616e207374727563747b7d3b20637075427566205b325d2a72756e74696d652e74726163654275663b2072656164657220696e7465726e616c2f72756e74696d652f61746f6d69632e506f696e7465725b72756e74696d652e675d3b206d61726b576f726b65724c6162656c73205b325d5b345d72756e74696d652e74726163654172673b20676f53746f70526561736f6e73205b325d5b335d72756e74696d652e74726163654172673b20676f426c6f636b526561736f6e73205b325d5b31375d72756e74696d652e74726163654172673b20656e61626c656420626f6f6c3b20656e61626c656457697468416c6c6f634672656520626f6f6c3b2067656e20696e7465726e616c2f72756e74696d652f61746f6d69632e55696e747074723b206c6173744e6f6e5a65726f47656e2075696e747074723b2073687574646f776e20696e7465726e616c2f72756e74696d652f61746f6d69632e426f6f6c3b2065786974696e6753797363616c6c20696e7465726e616c2f72756e74696d652f61746f6d69632e496e7433323b2073657147432075696e7436343b206d696e5061676548656170416464722075696e7436343b2064656275674d616c6c6f6320626f6f6c7d", "Δtraceᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6c6f636b2072756e74696d652e6d757465783b207265757365205b5d72756e74696d652e6c697665557365724172656e614368756e6b3b206661756c74205b5d72756e74696d652e6c697665557365724172656e614368756e6b7d", "userArenaStateᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6c6f636b2072756e74696d652e6d757465783b2073656564205b33325d627974653b20737461746520696e7465726e616c2f6368616368613872616e642e53746174653b20696e697420626f6f6c7d", "globalRandᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6c6f636b2072756e74696d652e6d757465783b20737461636b2072756e74696d652e674c6973743b206e6f537461636b2072756e74696d652e674c6973743b206e20696e7433327d", "schedt_gFree")]
[assembly: GoDynamicTypeLift("7374727563747b6d63656e7472616c2072756e74696d652e6d63656e7472616c3b20706164205b32345d627974657d", "mheap_central")]
[assembly: GoDynamicTypeLift("7374727563747b6e6f74652072756e74696d652e6e6f74653b206d61736b205b335d75696e7433323b2077616e746564205b335d75696e7433323b2069676e6f726564205b335d75696e7433323b2072656376205b335d75696e7433323b20737461746520696e7465726e616c2f72756e74696d652f61746f6d69632e55696e7433323b2064656c69766572696e6720696e7465726e616c2f72756e74696d652f61746f6d69632e55696e7433323b20696e75736520626f6f6c7d", "sigᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b726f6f742072756e74696d652e73656d61526f6f743b20706164205b34305d627974657d", "semTableᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b72756e74696d652e674c6973743b206e20696e7433327d", "p_gFree")]
[assembly: GoDynamicTypeLift("7374727563747b72756e74696d652e6d757465783b2072756e74696d652e70657273697374656e74416c6c6f637d", "globalAllocᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b73656d612075696e7433323b2061637469766520626f6f6c3b206f666673657420696e7465726e616c2f72756e74696d652f61746f6d69632e496e7436343b207265636f726473205b5d696e7465726e616c2f70726f66696c657265636f72642e537461636b5265636f72643b206c6162656c73205b5d756e736166652e506f696e7465727d", "goroutineProfileᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7369676e616c4c6f636b20696e7465726e616c2f72756e74696d652f61746f6d69632e55696e7433323b20687a20696e7465726e616c2f72756e74696d652f61746f6d69632e496e7433327d", "profᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7370696e41667465725261676765644261727269657220696e7465726e616c2f72756e74696d652f61746f6d69632e426f6f6c3b20726573746172746564447565546f323739393320626f6f6c7d", "gcDebugMarkDoneᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7573657220626f6f6c3b2072756e6e61626c652072756e74696d652e6751756575653b206e20696e7433327d", "schedt_disable")]
[assembly: GoTypeAlias("Error", "ΔError")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<PanicNilError, ΔError>(Pointer = true)]
[assembly: GoImplement<TypeAssertionError, ΔError>(Pointer = true)]
[assembly: GoImplement<boundsError, ΔError>]
[assembly: GoImplement<errorAddressString, ΔError>]
[assembly: GoImplement<errorString, ΔError>]
[assembly: GoImplement<plainError, ΔError>]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<_type, ж<_type>>(Indirect = true)]
[assembly: GoImplicitConv<abi.RegArgs, ж<abi.RegArgs>>(Indirect = true)]
[assembly: GoImplicitConv<abi.Type, ж<abi.Type>>(Indirect = true)]
[assembly: GoImplicitConv<adjustinfo, ж<adjustinfo>>(Indirect = true)]
[assembly: GoImplicitConv<childInfo, ж<childInfo>>(Indirect = true)]
[assembly: GoImplicitConv<context, ж<context>>(Indirect = true)]
[assembly: GoImplicitConv<funcval, ж<funcval>>(Indirect = true)]
[assembly: GoImplicitConv<g, ж<g>>(Indirect = true)]
[assembly: GoImplicitConv<g, ж<g>>]
[assembly: GoImplicitConv<gcWork, ж<gcWork>>(Indirect = true)]
[assembly: GoImplicitConv<itab, ж<itab>>(Indirect = true)]
[assembly: GoImplicitConv<lfstack, Δhex>(Inverted = true, ValueType = "uint64")]
[assembly: GoImplicitConv<limiterEventStamp, limiterEventType>(Inverted = true, ValueType = "uint64")]
[assembly: GoImplicitConv<linknameIter, ж<linknameIter>>(Indirect = true)]
[assembly: GoImplicitConv<m, ж<m>>(Indirect = true)]
[assembly: GoImplicitConv<mspan, ж<mspan>>(Indirect = true)]
[assembly: GoImplicitConv<pollDesc, ж<pollDesc>>(Indirect = true)]
[assembly: GoImplicitConv<ptrtype, ж<ptrtype>>(Indirect = true)]
[assembly: GoImplicitConv<spanClass, traceArg>(Inverted = true, ValueType = "uint8")]
[assembly: GoImplicitConv<stackScanState, ж<stackScanState>>(Indirect = true)]
[assembly: GoImplicitConv<stkframe, ж<stkframe>>]
[assembly: GoImplicitConv<sudog, ж<sudog>>(Indirect = true)]
[assembly: GoImplicitConv<sweepClass, spanClass>(Inverted = true, ValueType = "uint32")]
[assembly: GoImplicitConv<traceGoStatus, traceArg>(Inverted = true, ValueType = "uint8")]
[assembly: GoImplicitConv<traceProcStatus, traceArg>(Inverted = true, ValueType = "uint8")]
[assembly: GoImplicitConv<Δp, ж<Δp>>(Indirect = true)]
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("runtime/alg.go", "alg.cs", "ABIogqaCpoKmgqiSgoIACDYAFASUAAEeAA4EgoIAAhDCgpSkpLiCgpSkpLiCgqaCgqaCgoKClILKlIKUAAMcAAoCgoKClJSUgpQAAzIAFwKUlKSkppSkpKSkpIKClKSCgpSkgoKClJTIuKKClKaigpSUpIKCgoKCgoKUlIKClIKmgqaClLaCgoCCtqSCgoKUgIK2trqSpoKkgqSCpIKkgqSCpIKkgqSCpIKkgqSCgoKkgoKCpKKClIKClLiUpKKClIKCgpSUlAACHAANAsaigqaixqIAFSqUqIKUgoKUgriClIKCupKCgpSmgoKClg==")]
[assembly: go.GoPositionMap("runtime/arena.go", "arena.cs", "AF3MAQAIAgAFEgAIAoKClIKCgoKCgqyyrLIAEYgBgoKUgpSCgqaCpqrqACKQAsKCgoKCgpSU6rSCgsqUgoKClKaClIKCgoKClIKUgoKmgoIAHjjSgoKUlJSClKaClNyCgpSClJSClIKCpoKCppSCgpSCpoKClJSCgpSClAAJFJSClNzSgoKUggADEPKClIKUgoKCAAcQAAcQgriWkoIAChaiyqamgpSqwpSCgsqCgqaCgoLugoKokoKUgoKClKrCyqaCgoKUgriCpoKCgpSCuO6UgoKCkoKClIKUvKKCgpSU/LKCAAgSpoKClIKUlpKClIKU3IKmlKaUpqaCgqaAgoKCpoKUtoKmgIK2gpQABxCCpgACGgAKAgAGEPKClIIADh7KytzcgoKCgoKmpoKmlIKCupKCgpQABhQACQKClILKgpSClILcpoKCgoKUgoK4gqaCgpQABRgACwKCpoKCgoKmgriUgoKU3IKCgpSUgpSClJQABxCCpoKCgoKCAAgSpoK4gpSCgqaCgqamuAAKFoKUpri4goKClA==", "762-764:1;910-915:1")]
[assembly: go.GoPositionMap("runtime/asan0.go", "asan0.cs", "ABAmkKKAooCigKKA")]
[assembly: go.GoPositionMap("runtime/atomic_pointer.go", "atomic_pointer.cs", "AA1AABUCgoKCrLKClIKUAAIQ0gACENKClIKUAAIQ6KKClIKUqKiigpSClIKoqKKClIKU")]
[assembly: go.GoPositionMap("runtime/auxv_none.go", "auxv_none.cs", "AAgS")]
[assembly: go.GoPositionMap("runtime/cgo.go", "cgo.cs", "AEGSAfAABRDgAAoUlIKCgpQ=")]
[assembly: go.GoPositionMap("runtime/cgocall.go", "cgocall.cs", "AGncAfKSggAJKgAQAoKmgqaCpoKmAA8g7pTulLiClLiUlLiCAAwagoKUAAIS4pS4poLKABUsgoKCjNIACBKCgoKmgtyygoKCpoKCgu6UuAAKFoKCgoKCgqaUgqbKgpSmpoKmppSmgoKCpqKUgoKmggAHEIKCgpSUgrjKyoKCuJKUgqamgqqikoKUgqaUyqKkgqaCgu6CgoIACBKU6pLYkgAnzAHylKaYgoKClIKUgoLspIKUgpSkgoLKgpSCgpSClKSCgoKClIKUgpSCgraCgpSCtoKCgpSClIKClLaCgoK4gpSCpgACyQEAA9oB8oKCgoKUgoKCgIKkgoKmpoKm3AACEuKCpoKmgoK4rsKs0oKmgoI=", "404-408:1")]
[assembly: go.GoPositionMap("runtime/cgocallback.go", "cgocallback.cs", "AAcWog==")]
[assembly: go.GoPositionMap("runtime/cgocheck.go", "cgocheck.cs", "AA8yAAgCuJSClILKgoLKgsqC3IKmgoIAAxgACgIAAhYACgKClIKUgpQAAhYACgKClIKUgpSCgoIAAxQACQSClICCtgACEuKCgoKCgoKCgoKClJSClIKCggAGHgAKAoK4gpSAgrY=", "65-68:1")]
[assembly: go.GoPositionMap("runtime/chan.go", "chan.cs", "ADp+ktaCgqbmgqaClIKmgoKuwqaU2ILGgraCgoKClJSClAACHAANAq7mlKassgAJHgAOAoKClIKmgqaCpoIAEiaCpoKCppSCgqaApoCStpSCgpSCgoKUgoKmgoK4goKCgriCgoKCgoKCyoKCgpTKpoKUgoKCgoKUgoKCgpSUAAISAAkCgoKUgoLKgoKCgpSmgoKUgoKCgoKUAAIQAAwMgpSCgoKCgoKClJSCAAIaAA8Mgqam2IKC5qKCpoKCgqaCgoKmlKaCgoKUgoKUgpSCgoKClLiCgoKUgoKUgoKCgpSUpoKCggADEPSCuIKUrLKosoIABRIADAiCpoKClIKmgqaCuAAJFMrKlIKUgpS4goKmlIKCgpSCgpTKgMqAksiUgoKUgpSCgoKUgoKmgoK4goKCgriCgpSCgoKCgoLugoKClKaClIKUgoKClIKCgoIAAiIAEQKCgpSCgpSU7oKCgqaCpoKCgpSUgoKCgoKClKb8uNyCAAImABACAAImABACqJKokqaigpSCgpS4lKaigpSCgoLKlKiSqJKokqiSpqKCgoKCgoKUgoKmgoKCgpSCgoKUgoIAChaClLi43KaCgoKCrAANEO6CgoKUgqaClA==", "232-232:1;604-604:1")]
[assembly: go.GoPositionMap("runtime/checkptr.go", "checkptr.cs", "AA4StILugsqCvKKCyoKUyuaCgtyCgqaCgrgAAigAEgSAAAgSyICCyIKClIK4")]
[assembly: go.GoPositionMap("runtime/complex.go", "complex.cs", "AAcSooKClKaCyIKCgoKUgoKCpqiClJSCtoKCgraCgoLI")]
[assembly: go.GoPositionMap("runtime/coro.go", "coro.cs", "ACNU0oKCgpKSgpKCuIKCgqaCgqzSgoKUgqKoooKCgq7CgoIABx4ADgKCgoKCAAcQgoKCgoKCAAkUlIKCuIKmpqbcgriCpriqogAKFoKClIKCgoIABxCC3ILcggAIEoKmpoK4goK4gqaCuA==", "47-60:1")]
[assembly: go.GoPositionMap("runtime/covercounter.go", "covercounter.cs", "AAsakoKCgoKUyg==")]
[assembly: go.GoPositionMap("runtime/covermeta.go", "covermeta.cs", "AA4cwoKClA==")]
[assembly: go.GoPositionMap("runtime/cpuflags_amd64.go", "cpuflags_amd64.cs", "ABMsAAcQgoKClII=")]
[assembly: go.GoPositionMap("runtime/cpuprof.go", "cpuprof.cs", "ADqIAQAIBIKUgqaCgoKCgqaCgoKCkoKCgoKUAAIUAAgElKaCgpTKpgACGgAQDpSmgoKCgpSmAAIQ5IKCgpSmgoK4gqaCgriCAAMcAAkCAAIaAAoCAAIoABECgoKCgoKUgoKCgpQ=", "", "", "178=FuncPCABIInternal/1/2/1,FuncPCABIInternal/2/2/2;188=FuncPCABIInternal/1/2/1,FuncPCABIInternal/2/2/2")]
[assembly: go.GoPositionMap("runtime/cputicks.go", "cputicks.cs", "AAgW")]
[assembly: go.GoPositionMap("runtime/create_file_nounix.go", "create_file_nounix.cs", "AA0WgoI=")]
[assembly: go.GoPositionMap("runtime/debug.go", "debug.cs", "AA1U0gAIOpIAAjAAFgSCgqaCAAMWAAgEgoKmggAGGuKCgpSU")]
[assembly: go.GoPositionMap("runtime/debugcall.go", "debugcall.cs", "ABYyks7kgpSAyrqigoKCgqaUAAMeAAcQgIKCyIKUgpSmAAUYAAkCgoIABxC4yJKCuLiCgtyClIKC3Li4lIKmgriUgoKU3NyCgoKmlAAHFKKCgoKmpoKCgriCgtyCuJSCgpSCgoKUgoKCgpS4pJKSlIKCgoKmgqI=", "49-95:1;123-161:1;164-188:2;222-259:1;269-274:1")]
[assembly: go.GoPositionMap("runtime/debuglog.go", "debuglog.cs", "ACt6ABIEqqIAHMIB5sSCgrimgrgAECiQprKClJSokKaSqJCmkqiQppKokKaSqJCmsoKCqJCmkqiQppKokKaSqJCmkqiQppKokKaygoKokKaygoKokNbCgoKUgpSkpqiQAARGkKaygoKokKaygoKClAAuZJKU7sySgoKokoKCgoKCgqiSgoKCgoKCgoKCqJKCgoKokoKCgoKCgrqSgoKUlKiSgoKCgpSCggALHqSClIKUgoKUgpSCqJKqkoKCgpSslIKCgpSCgpSCpoKCuIKUgoKCgoKCgqaClKb2goKmgoKUpoKCgoKCgoKmpoKCgoKUlAAEogGSgg==")]
[assembly: go.GoPositionMap("runtime/debuglog_off.go", "debuglog_off.cs", "AAwagtqCpoI=")]
[assembly: go.GoPositionMap("runtime/defs_windows_amd64.go", "defs_windows_amd64.cs", "AD9+gKKAppCipICigKKApIKCpoKCgoKCgoKCgoKCgoKCgoKCgoKCggANHII=")]
[assembly: go.GoPositionMap("runtime/env_posix.go", "env_posix.cs", "AAwSgoKClIKCpqqigoKCgpSUlKaCgpQAGDSSgpSSqJKClJKmgoKC")]
[assembly: go.GoPositionMap("runtime/error.go", "error.cs", "ACFA1IKCgpSCgpSCgoKUgpSmlAACEtKCgoKClILcpIIABxCkggACENIABxCkggAiSAAMGgAKFqSCgoKUgoKmgoKCuIKCgoKCgpSClLS2AAYeAAgClLS0tLS0tLS0tLS0tLS0tLS0xMqygpSUgoKkpKSkpKSkpKSkpKSkpKSkpLqSgoKClIKCgpSuwoK4goKUgoKUgoKClIKUgoI=")]
[assembly: go.GoPositionMap("runtime/extern.go", "extern.cs", "AKgC3AQADAKCgoKUggAJSAAIAoKClAAMILI=")]
[assembly: go.GoPositionMap("runtime/fastlog2.go", "fastlog2.cs", "AAcc8oKUpoKClII=")]
[assembly: go.GoPositionMap("runtime/fastlog2table.go", "fastlog2table.cs", "AAkS")]
[assembly: go.GoPositionMap("runtime/fds_nonunix.go", "fds_nonunix.cs", "AAgS")]
[assembly: go.GoPositionMap("runtime/float.go", "float.cs", "AAkSlqSokqiSAAIS4oKqooKokqqi")]
[assembly: go.GoPositionMap("runtime/hash64.go", "hash64.cs", "ABAqgpKClKSCgrSCtIK0grSCtIKCgoKCgoKClJSCgpSCtqaCgqaCgqaCgqaCpoI=")]
[assembly: go.GoPositionMap("runtime/heapdump.go", "heapdump.cs", "ADecAYKClIKCgqaCgoKUgriipoKCABIqkoKCgoKClIKCpoKClLqSgqaCgoK4gqiygsqCgpSClIKUgtyClKaCgoKCgIKUgoKCgoKkqJKCgoKmgoKCpqKCgoKCggAMHLKCgoL6sqaCgoKClLiUlIKClLiCgoKCgoKCgoKClKaCpoKCypSCgqSUgoKCpKaUpoKCgoKCgpSUpqKigoKClIKCpoKCgoKCgoKCgoKCgoKWkoKCgoKCgriCgoKCgoKCgpSUlJSCgoKCgoKCguiCpoKCAAQSrYIABBaigoKCgoKmlLiCgoKmgoKCpoKUgoKUgoLcAAkOlJSCgpSCgoKCpoKCuIKCgpTKgoKSgpSUgpKCgpSCgpSCgpSCuIKCgoKmooKCgoKmgqaCgoKCgrqSyoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKClKaCgoKCgoKCgoKCgoKCgoKClIKCgqaCgoKCgoKCgpSCgpSCgqaCppSUgoKClIKClIKCgoLKpIKmgoKmgoKCgoKCgoKCgoKmgpSCpqamgoKCpqiygtaUgoKClIKCgpSmgoKCgoKAgqSClA==", "411-424:1")]
[assembly: go.GoPositionMap("runtime/histogram.go", "histogram.cs", "AF7SAQAJBIKCAAESAAgCgIKClILIgoKmgqjCuIKClAAFEIKCpoKCrLKUlKamuIKUlMqmuIKC")]
[assembly: go.GoPositionMap("runtime/iface.go", "iface.cs", "ABUmggAJFJQABRgADAKCuIKClIKm3IKAgsiCgIKCyIKC3IKCgoKCgpSCAAcQqviCgoK4goKUgpSC7AAICoKmgsqC3IKCppSmqvaCgoKCgsqUyoKClIIAAxQACAKCgtyCgoKCgoKCgoKCgoKCgpSCgoKCgoKClIKCgpKClNy0gpSmgoKCgoKmrsKqwoKClKqiAB0ypIKCgoIAARgACwKClIKUgpSCgqS0gpSClIKmgoKmooKCgqaClKaigoKCpoKUAAIYAAsCgpSClAACGAALAoKUgpQAAhgADQSClIKUpoKUlKaCgpSs0oKCgqamgsqUppSmuMqUpqKmgoKC7oKmgoKCppKCgoKCgpSmgoKmlMqe8qaCpoKCgoK4gsq4ppS4uMqUrNKmgoKC7oKmgoKCppKCgoKCgoKUpoKCppTKAAEYAAoCqJKmpoKCgoIAChzC3LI=", "531-541:1;637-648:1")]
[assembly: go.GoPositionMap("runtime/lfstack.go", "lfstack.cs", "ABkwooKCgIKCpIKCgoLKgoKCgpSCgoLKguqigIKkgoKCuIKmgg==")]
[assembly: go.GoPositionMap("runtime/linkname_swiss.go", "linkname_swiss.cs", "AEe2AQAUAoKCppSCgpSCAAImABACAAIoABMCgoKmlIIAAigAEQIAAiIADgIAAiIADgI=")]
[assembly: go.GoPositionMap("runtime/lock_sema.go", "lock_sema.cs", "AA4msgALlAKCgoKUggAEIoKm")]
[assembly: go.GoPositionMap("runtime/lock_spinbit.go", "lock_spinbit.cs", "AEKSAZKClAAQHpKCgpQAAxgACAKAgpKAgpQABSIADQIABRCrAAUcggAEzAGC")]
[assembly: go.GoPositionMap("runtime/lockrank.go", "lockrank.cs", "AFawAQBDfoKClIKUgpQABxA=")]
[assembly: go.GoPositionMap("runtime/lockrank_off.go", "lockrank_off.cs", "ABEgpoKmgqyypoKssqzYuLi4uLg=")]
[assembly: go.GoPositionMap("runtime/malloc.go", "malloc.cs", "APwC6gWCgqamuJSUgoKUgoKUgoKUgoKUypSmgqaCgpSCggAHEIKCgoKUgoKmgpSCuIKmgriCgoKCgoKClKa2AClUgoLKgoLGtLSmlLTagoKUgoIAEiiCgoIACRQACBKCgpSClKbKgoKCgoKmgoLcgoK4AA4mABUClJTcgoKCyoKCgoKUlJKUlJSUgpSCggAHEIKUgqaCytyCgriCgoKCgriCgoKCkoKSgpSmgriCpoSSggAIEoKClIKUlKaClIKCgoKCyoKCgoKUgoKUgoLcgu7YgqbssoKClMqCgoKUpOyCgoKUgoCCpKa2goKCgoKUAAMiABAE3IKCgpSCuIKCgpQABhLCgoKCgoKClIKCgqYABxgADQKCgoKUgoKUgoKUpoKmgoKCgpQADToAFAKCgsqC3KaCgILMooKCuIKokoKCgoKUpIKUpriClIKUpoKClIK4goCC2oKU1rSCgoKUgpSCpgAePoKUgpLukoKSgpSUgoKCgoKmgoKCgpSCgqaUggAIEgAJFNyCAAsYgoKUgpSCgILIAAwalNa0goKClIKUgqaUgoKCgpSUgoKCgoKUgoIACBIACRTcggALGIKClIKUgoCCtua0goKClIKUgpSCppSCgoKCgoKClIKClKaUlAAHEAAJFNyCAAsYgoKUgpSCgIK21rSCgoKUgpSClIKmlIKCgoKClJSCgoKCgpSCgpSCggAHEAAJFNyCAAsYgoKUgpSCgIK2prSCgoKUgqaUpoKCgoKCAAgSAAkU3IIACxiCgpSCpoCC7JQAChaClO6CgqaCgoIABxCCkoKSgpSmlJSUpoKUpoKCgoIABBb0goK4lLgAAxgACAKEkoKClKaCgpS+0qiSAAIeAA4CqLIAAiAADwKClIKClAACJAAPAqiS3sKCgpSCggACFPKUlJSUlICCyKrGlLQAAhYACAKCgoKUgqq0goKUgpQAGEYADQKCgpQAEsoBgsqUgoKmgoKClIKAgpSCgpSkAA4aoqqilKSkpKSkpKQ=", "1921-1923:1")]
[assembly: go.GoPositionMap("runtime/map_fast32_swiss.go", "map_fast32_swiss.cs", "AA4kAAQWAAoYAAsWAAoG")]
[assembly: go.GoPositionMap("runtime/map_fast64_swiss.go", "map_fast64_swiss.cs", "AA4kAAQWAAoYAAsYAAsG")]
[assembly: go.GoPositionMap("runtime/map_faststr_swiss.go", "map_faststr_swiss.cs", "AA4kAAQWAAoYAAsG")]
[assembly: go.GoPositionMap("runtime/map_swiss.go", "map_swiss.cs", "ABg4kqaCgpQAAiAADQIAAiQADwKCpgACGgALFgAKBIKCgpSmgoKClAACJAAQFgALAoKCgoKUgpSCpq7igoKmgqrCgoKmqJKCgoKmAAUmABMEgrgAAhwACwKClJSokoKUlAACGAALAoKosoKokqiSAAIaAAwCgpSCgpSokqiygpSCgpQAAhDq0oKCpqKUgoKCpqzErMQ=")]
[assembly: go.GoPositionMap("runtime/mbarrier.go", "mbarrier.cs", "ABesAgCAAQKClLgACBKCggADFAALCAACEgAMCgACIAAPAoKClIKClIKClKiSqJIAAhoADQK4lKaCggAEHgAMAoKClILugoKCgpSCgpSCgqaCpoLugriCuIIAAiAADwKClAACHAANAriUAAIYAAkCqJKossqUqLKCppQAAiQAEASC")]
[assembly: go.GoPositionMap("runtime/mbitmap.go", "mbitmap.cs", "AGXgAQAKBgAYTgANAoKCgpQABRYACgKCgoKmgoKUlKiSlIK4gpSmggAFGAAJAoKClIKmggACLgAVBIKWkoKUppQAAhYACAKCgriCuIKClLiCuIKCggAEGAAIBIKClKaUgoKUyqaCgpSmpoKCpoLKgpSCgpQAAhDSAAVaACoCgpSClIKmgoKCpoKCgqaS7pSWkoKmgoKUlIKCgoCCpIKCpoKCgIKkgoKCggAEJgAOAoKUgpSClpKCpoKClJSCgoCCpIKCupKCgoKkgoIAByQADgKUgoKUgtyUlKyygoKCggACEuKCggAJFIKCgoKUgpSCgoKmlAAGFAALBKaCgoLKgpSCgpSm3IKCgoKClIKCgoKCpoKmgoKCgoKCgqYAFyyigpSCgpSmsoKClKaiADFkgoKUprLKgoKUgoKCgpSCgoKmgoKU1sSCgoKUgpSCgoKCgqaCgoKClIKCuIKCgoKUlIKCgoKCgoKCgIKCgqSClNaygoKClIKClIKCgoKCpoKCgoKClIKCuIKCgoKUlIKCgoKCgoKAgoKCpIKmgpSCgoKCgqaClKaU2MKClLiUgoKCgpKCgoKClIKmgoKCgoKCgoKSgoKCgqa4goKCgoKUpq74rviu+AACEgAJCAARJpKCrsKCgoKCgoKCgoKC3sKCgoKUgqaUgpSCgoKUlIKCuIKCgqaClNyClIIAAhDSgpSCAAUWAAgCppSCgpSssqaCgoKmgoKmgqiSqMiokqjI2LKCgpSokoKClJQAChAACxKCgoKCgpSUlIKCgpSCAAI2ABwCpoK4lNyAlIK4gpS2goKs6AAEGgAKAoKClIKCgoKUgpSUgoKCgpSCgoKmAAckABACgpSCgpSClIKCgoKCgpSUgoKCgoLOooLKyoKUqqKCgoKUlAALFrKCgoKCgpQAAioAFAKWkpSCgqaCgoK4goKClJSUgoKCgoKClICCgoKkuIKCgoKCgtyCgoKCgoKmAAgSgoKUgqaCgoKCgu6CgrjugoKUpoKClIKCuIKCgu6CgoKCgoKCyoKCgpTclIKAgoKCgsiCgoKCgqaAgoL6goKCgoKUpqKCgoKCgoKUgoKCgoKUgpSCgoKCgoKCuIKCgoKCgqaCAAQW4g==")]
[assembly: go.GoPositionMap("runtime/mcache.go", "mcache.cs", "AECUAbIACxaCgoKCgoKUgpSCAAIS4oKC7oKCAAMQ1IKCypSUAAcQ5JSClJSClKaCgqaCgpSmgqa4goKmgsqmAA4egoKUqJKClIKC3JSCgoK4goKCpqa4AAgSgoKmlIKUgoKCgoKCpoKCuJTcuIK4gqaCgoKm3AAMEIKCgpKCgpSCgg==", "88-93:1;108-120:1")]
[assembly: go.GoPositionMap("runtime/mcentral.go", "mcentral.cs", "ACxgkoKCgoKqoqqiqqKqotikgpSCgoKCAA8glIKmgoCCtoKUgoKClICUgoIACxaCgoKUgJSUgoKCgqbIlIKCgoK4goK4goKCgoKmgoKUgpS4lN7igqaCpu6muAAHEIKUlLjMkoKCgpSC")]
[assembly: go.GoPositionMap("runtime/mcheckmark.go", "mcheckmark.cs", "ACVMsqaCgpSUgoKUprjYkoKU/LKCgoKmppSCpoKCgoKUlKaC")]
[assembly: go.GoPositionMap("runtime/mem.go", "mem.cs", "AApiACgCgoIAAhDSggACGgAKAoKssqyyqqIAAiAADQKCggACGAAJAoIAAh4ADAKuwoI=")]
[assembly: go.GoPositionMap("runtime/mem_nonsbrk.go", "mem_nonsbrk.cs", "AAwagg==")]
[assembly: go.GoPositionMap("runtime/mem_windows.go", "mem_windows.cs", "ABq8AaamAAQelAAEIA==")]
[assembly: go.GoPositionMap("runtime/metrics.go", "metrics.cs", "ACp4soKmuIIACxiUlIKmgtyC3ILcgtyC3ILcgtyC3ILcgtyC3ILcgtyC3ILcgtyC3ILcgtyCpoLugtyC3IKmgu6C3ILcgsqCyoLcgtyC3ILKgtzKgtyCAAUQgtyC3ILcgtyC3ILcgtyC3ILcgtyC3ILcggAEEILKgsrKysrKyoLKgoK4pqKCyqKCqJKCgoKClJKCABEqsoKClKjCgoKUqMKCgpSokoKCpqiSAB1KsqaCgoKCgoKCgoKClIIAFzKygoKCgoKClIKCgoKCggAJFpIAECSSgoKCqqIADyTSgoKUgoKUlKSkpKYAHESygoKUgoKUgoKUABY0woKCgpSUgoKUlKyypqaC", "89-92:1;96-99:2;103-106:3;110-113:4;117-120:5;124-127:6;131-134:7;138-141:8;145-148:9;152-155:10;159-162:11;166-169:12;173-176:13;180-183:14;187-190:15;194-197:16;201-204:17;208-211:18;215-218:19;222-230:20;234-237:21;241-244:22;248-256:23;260-263:24;267-270:25;274-277:26;280-283:27;286-289:28;293-296:29;300-303:30;307-310:31;313-316:32;319-322:33;325-328:34;332-337:35;341-344:36;348-351:37;355-358:38;362-365:39;369-372:40;376-379:41;383-386:42;390-393:43;397-400:44;404-407:45;411-414:46;418-421:47;425-431:48;434-437:49;440-443:50;446-448:51;451-453:52;456-458:53;461-463:54;466-468:55;471-474:56;656-663:1", "507=runtime.metricReader.compute", "73=makeStatDepSet/1/42/8,makeStatDepSet/2/42/15,makeStatDepSet/3/42/22,makeStatDepSet/4/42/29,makeStatDepSet/5/42/36,makeStatDepSet/6/42/43,makeStatDepSet/7/42/50,makeStatDepSet/8/42/57,makeStatDepSet/9/42/64,makeStatDepSet/10/42/71,makeStatDepSet/11/42/78,makeStatDepSet/12/42/85,makeStatDepSet/13/42/92,makeStatDepSet/14/42/99,makeStatDepSet/15/42/106,makeStatDepSet/16/42/113,makeStatDepSet/17/42/120,makeStatDepSet/18/42/127,makeStatDepSet/19/42/134,makeStatDepSet/20/42/146,makeStatDepSet/21/42/153,makeStatDepSet/22/42/160,makeStatDepSet/23/42/172,makeStatDepSet/24/42/179,makeStatDepSet/25/42/186,makeStatDepSet/26/42/205,makeStatDepSet/27/42/212,makeStatDepSet/28/42/219,makeStatDepSet/29/42/244,makeStatDepSet/30/42/253,makeStatDepSet/31/42/260,makeStatDepSet/32/42/267,makeStatDepSet/33/42/274,makeStatDepSet/34/42/281,makeStatDepSet/35/42/288,makeStatDepSet/36/42/295,makeStatDepSet/37/42/302,makeStatDepSet/38/42/309,makeStatDepSet/39/42/316,makeStatDepSet/40/42/323,makeStatDepSet/41/42/330,makeStatDepSet/42/42/337")]
[assembly: go.GoPositionMap("runtime/mfixalloc.go", "mfixalloc.cs", "ADdwwoKUlIKCgoKCgoKCgtaCgoKmgoKCgoKUlIKCpoKClIKCgqaCgoKC")]
[assembly: go.GoPositionMap("runtime/mgc.go", "mgc.cs", "AJYB2gIAEgLWgoKmypSCgoKCgq7UgoKCgoIAJk6SggAmUgAGFNaCgoKUgqYAjQHKA6KUgoKUlJSCyoIAI1aygpSUgqSClIK2lAAFEgAMCoKAgoKkgpSAuIKCogAOHMqUgoLugoKSgriCuJSCgoK4goCCgsiUlIKmlIKClIKCgoK4poLKlLimyoIAECKUgu7uuO6mgoKCpu6CuIKmogAcWAAXBpSO4oLKuKaCpu6UgoK43IK47oKCgoKC7gAKFoKCgoKCgriClIKUpoKUgqa4pri4gsq4yqb6tJSClIKCgrjuggAJFIKCgsqCgoKCgoK4gqaClIKCgriUgri4poKCgoKCgoKCggAJFIK43IKmlIK4goKC3KbKAA0cgoKSgqYAChTcuKYADRyCgoKCgoKmlO7cgpSCgqaCgoKUgpSC7pSSgpSU3IKUgriCgoKCgriCgriCpoKmlAADEAAJDIIACRSCgpSCgoIACBK4vAALFIIAFySCyoKClJSUABEkpoKUgAAPIMimAAgSgpSCgqaCpoKCgoKmgoKCpgAOGoKYgtyAkoKC2qSkAAIpAAEuuIKCgoKUgsqCgpTuuLiClAAEENKClIKUgpTssoKUpoKCpqbcAAcQ3KaUpoKCgoKClJSClJSC3AAIEoKCgpS4AAUWAAgClIKmgoKCgoKCgpSUpoKClIKmpoLKgoK4goKClIIAAhoADAaCgsqCgoKCgqaCAAkmAAoCqJKotoKmgoKCypSCuIK4ggANFIKSgoKUgriGsoKClIKssoKCgoKCgoKUlIKokpSmgoKClIKCgpQAAhoACgKCAAuEAQAIAoKCgpSAgqSCgpSCpoI=", "648-650:1;725-727:2;733-735:3;802-809:4;896-912:1;936-938:2;956-964:3;969-976:4;1040-1049:1;1052-1071:2;1176-1187:3;1213-1223:4;1216-1220:4.1;1293-1295:5;1423-1450:1;1483-1523:2;1743-1746:1;1791-1793:1;1795-1800:2", "", "1182=itoaDiv/1/1/1;1210=Load/1/2/3,Load/2/2/4")]
[assembly: go.GoPositionMap("runtime/mgclimit.go", "mgclimit.cs", "AE+0AcIABhIACAKmlIKmgoIACRTSgtyAgqSCggAGEqKqoqqi3uK4lIKUgtiygpSUgqaCgriCgqYABxCCgoKkpIKkpLS0priCggAXMJSu4oLupoKCgoKClLiUppSCupLYkoKCAAkW4qamgpSCgoKCkoKUABpAkq7mgoKUqJIAByQACgKClIIAAhAACQSCgoKUlIK4lIKCpuyygoKCgoKUgqaC7qakpIKkpKQ=")]
[assembly: go.GoPositionMap("runtime/mgcmark.go", "mgcmark.cs", "ADN0wqaYgqaCgoKmgoIADyCCAAcQgpSCpoKCgoLqooKCAAcQgoKCpoKmpsoABhoACwSSgpSCgsiCgsiCpuzIyIKCgoKUuIKCytqCgoIACRSCgoKUgpSCgpSCyIKCgqYABRLilNyCgpSCgoKCuIKu1IKCgoKCuIKCgoKmuIKC7AATIqaCgqaCgpSCgpSCggAHELiAgoK2lILKgoK4lMqCuLaCpJSmAAQSAAoGgpSAgraAuIKCogAQIIKCpoKCggAHEILKptyCgoKCgoIACBKCgoKCgpSClJSUpoKCggAHEILKpqaCgoKmgpTuuILKgoKCpgAHEIKCAAsYgtyCgoIABxCCysYABRgADgaUAAYQggAHEIKUgoKCuLiClAAHEIK4goKUpsqUgoKCgoKUgoKCvrKCkoKuwriCgqaCgtyCgoKUgqaCAAIaAAoCyoKmgpSCgqaUgu6mgsqCuJSCgpQACSgAEwKCgqaapILbggACFIKssoKUlLiCgpSUpqaClIKmgtyCqJKC7qKmlKbKgraUAAcQgoKCgpSCgpSClJSCgoKCgpSClIKCgpTcgoKCgoKCgpS4gpSClKzigqaCgoKCAAoWgoKCyoCmtsrKlKamgoK4gri4goKCgoKUgpSUgpQACyCiqqKCgpSqogAFQgAgAoLKgoKCgpS4goKCgoKSgsqmgoKClIKCAA4e3IKmgoKCuIKmlJTKgoKCgpSClIKCgtyEkoKClAAGIgAPAoLKuIKUgqaCgoKmgriUgoKCgrimpoKCggAHEAACGAAOCIKUlIKCgpSClIKCgIKSgsiCAAgWAAwK7oKCgpSmpoKm3IKCAAcQgoKClKaCgoKAgoCCAAgOuLgACRSAgsiCAAUWAAoCgoKCgoKCgoK4goKmgoKUgoKUlKaCgoKC7oKUgpSCuKYACBKCuIKCuIKCuIIAAxLSgIKCAAgWAAsEgpSUgpSmgoKCgoK4gpSmgoLKgoIABxCUgryigoKCgpSCgIKUtoKCuJS4goKUgoKUgoKUlIIABhYACQKClJS4gqaCgqaCrLKUgoKClIKC", "62-64:1;131-144:1;214-247:1;437-439:1;560-564:2;1483-1506:1", "", "137=readgstatus/1/1/1")]
[assembly: go.GoPositionMap("runtime/mgcpacer.go", "mgcpacer.cs", "ANIC4gWigoKCggAGFNKCgoKCgoKCgoLugoKCgsqUlJS4goK4goKmyoLcysqClIKCggAIPgAYAoKmlIKCuLjcgoIAChaCAAgSgoKUlIiypgAJFIIACBK4gqYAChaCgoKs9qamlIKmAAgSlIKCAB08gt6CgoKCpoKUgoKCgoKCgoIAAxIADhaCpoKUgoKUgoKCgpSCgpSC/tKC7oKUgqbKpgAHELiCAAsYpoKCgoKmgsqmkpSC3IKUgqa4goKCgoKUAAIQ8oKCgoKCpoKCggAGFOKUgqSkgqS4goKCgpSCpqaCuLiigoKUgoKCuIKokoKuAAkEpoCCuoLK3AABHAAMAoK2qLSygoKCgoIAPn6mlpKCpsq4yoKmlIKUpoKUABg+AAkC7sruggAJFIKCAAsYgoKUlpKCgpSUgoKCgoKUAAIoABMCgqamytyCgILIgpQAGDKu4oKmgoKUgpSoxIKCgoLKgqbWgoKClICCpK7igqaCgqaoxIKCgqaClIKU5oKCgpSCgoKUAAUaAAoCgoKCppSCgpSCggAEGgAIAoKCqJKCgoKCgpSCggAEFNKCgoKCgpSCggAEHAAJApSmgsqCgoKmgoI=", "783-794:1;1272-1277:1;1319-1330:1", "", "433=Load/1/1/1")]
[assembly: go.GoPositionMap("runtime/mgcscavenge.go", "mgcscavenge.cs", "AJIBrAKSAAIgAA0C7rjcgpQACBKCgqaCyqYADBrKgpQAbPgB0oKUgpSCgpQAChYAChamgoKCgoKClIKmgpSogu6ygoKUgqiSrNKClKYAARDigpQABRQACQKCgqbcAAgSuJSC3IKmgqaU3IKUgrimgoKUlMoABxCWkoK4gqa8woKCAAYSAAoCgoKUlJSCAAIaAAwGAAES8oLclMqCpoKmypQAAhDSlIKUgoKCgpSCAAMaAAkCgoKCgpSClIKmAAIU8pSCzIKSgoKUggACIAATDoKCAAcQgoKmgqamlNyCppSmuIKClIKCgsqCgIKkgqaClLiClAAFFgAIAgAPILiUpLS0tLS0tAALFgAGLgAUAoKCkoKC7oKUppSUgoKmlNyCgpKmyoKCgoKU7oLu3oK4lJTKgrgAO4YB0oKCgoKs1oKAgqSqwoKClIKUuIKClJSCpoKmgu6mlKaCAAIWAAgCgtyu4oKCpoKC7oKCAAMS8oKCgpQAAhDSgoIACBSSqqIAGj6SAAcSkgAXPJKokqiSqqKUlJSUuLjYkoKClIKClIKU6pKCgpSCgpSmAA48AAwEgqaCuIKClIKSgriCgriCgqaokg==", "364-366:1;393-402:2;405-409:3;412-414:4;682-684:1;824-841:1", "", "395=Load/2/3/1,Load/3/3/1;710=load/1/2/3,load/2/2/4,heapRetained/1/1/4")]
[assembly: go.GoPositionMap("runtime/mgcstack.go", "mgcstack.cs", "AIgBkgKCgpSCAA4k1gAiTsKClIKClIKUgoKCkoKCgpSUgoKUggACEPKCgpSUgpSmgoKClKaCpoKClNiygpSCgoKUgpSUgoKCgpSCgoKClKyyAAIQAAgCgpSSgoKCgoKUgoKCqqKCgoKClIKClIKClA==")]
[assembly: go.GoPositionMap("runtime/mgcsweep.go", "mgcsweep.cs", "ADl+gqamgoIACBKCrsKssoKCgoKCgpSUpoK4ggAVQAAKAoKCgpSCAAgOooKUgoKClIKClIKClAAEFuKCgoKUgsySrLKssoIABRLi7gAHEILugoKCggAHEJSmgpSCgoKClAAAIgAQAoKCgoKmlJSCuIKUggASJKKCpoKmgpTaori4goKCuIKCgoKCgpSAuIKClKSAlIK4ypS2lAAOIIKCpoKmpoKCpqaCAAIS4tzogoLcgpSAgoKCpNyCgoKUAAwS9oKCpoKmpoKAgoK2goKCppSCABEkgoKUgoKCpoKCgpSCgqa4poKCgoK4yqaCgoLKgoKCpriCppSCgoKCgoKCgoKmgqaClIKUgqaCytyCgqaCgtyCgqaCpoKCgoLKgqaCuLiAgoKkggAMGpSCppS4gsqCuLaClIKCgpSmlMqCgoKmlNyUgqaClLaUAAYQgoKCpgAPIIKClJS4lAAFIgAQAoKCgoKCgoKCgpSUgpSUgoKUgoKCgpSUgpQAAigAEQKUpoKCgriCgoKCggAPIJSCgoKClJS4goKCvLSCAAMS0qaC7oK4gpSUgoKCgpSCuA==", "421-435:1;735-745:1")]
[assembly: go.GoPositionMap("runtime/mgcwork.go", "mgcwork.cs", "ABg0goIAPZAB4oKCgpSuwoKmgoKCgqSCgoKCgoKCgriC3IIAAxDCgoKmgoKuwoKmgoKCgqaCgoKCgoKUgoKmggADFvKCgoKmgoKCgoKCgpSCuIIAAhDSgoKmggACFPKAgoKUgpSUgoKUgpSkyoKUgoIAAxDCgpSAgoKCkoCCgpTGgr6yABQwwoLogoIAAxDCgoKCgsqCgoSSgoKCgoKUlIKClIKmgoK4goKCgoKUuK7iggACEPKCrsKCgoKUqMSCgoKCpoLcsoKCyoKCqqKCgoKClIKCgoKClIKmgoI=", "377-379:1;475-485:1")]
[assembly: go.GoPositionMap("runtime/mheap.go", "mheap.cs", "ANMC3gUADCCyrsIAYtQBgqbCgoKClAAFIAANAoKUlIKCgpSCgoKClIKCgpSCgoKmggANIIKokqiSAAIeAAwCqqIABBbippQAAxTigpQAAxTiAAIQ0oKClJSkAAVk4oIAAhLi3IKUquKCgoKCqLKClIKCgoKCgoKCAAgSuIKmAAISAA0OgtyUgoKCpoKClICCgpSUgpTIgpSCppSCuIKCpoKmgqaCgoKUAAISAA0MlIKCgoKUgoKmgoKCgoLKgoKCpoKCgoCCgoKClMr+gpSCgoKUgoKmggALHpIAAhLosqaClJQABSgAEQKClKqigoKCgoKCgpQABhwADAKCgpSCggAIEgAIEoKCuIKClJS43IKUAAIeAAwCpoKmgoIAAhoADAKUgpSmgoKClKaCggACGgAMApSUgoKCuAAFKAAVBIKCysqCgqaCgoK4goKCggAIEpSUAAkUgoKCgoKClIKCpoKmlIKCgoKCgpSCgrimlJQAABYACgKCgKamgoK2gAAIEoDKgoCCpILestyCppimgoKUuKaCpoKmgoKmgoKClKSkpJSmgoKCgqaq9oKClIKCgoK4goCCgoKUgpSUlMiCgoKCpsoACxgACRSU3IKmyq7i7pSmgoK4goKCgqamyoC4lIKCpoK2gu64gu64goK4goKokpSCgoKCuIKUgoKUlIKClIIAAx4ADgSCgoKCuIKCggAIBrKUlIK2gpSCgpSmgqTcgoKCpoKUpKSklKamggACFAAKCIKmlJSCupKCgLjEgoKCgoKCgoKCgoKCgoKCgoKCpoKokoLWsoKUlIKUlIKUlIKCpoLWsoKClIKmppSC1rKCgpSClKaUgqrSgriCuIKmgoKmABg4soKCgqiygoKCAAUSAAgCgoLcgpSClKaClIKCgqaCpoLcsoKC3IKUlIKUgoKCgpSClIKCrMSCgoKCgpSCgpSClJQADyLCgoKCgoKCgoLKgoKCpoK4gpS4goKCqJKCgpSCggALIMKCgoKCgoKClILKgqaUyoIAIkiSqJK4poK4poKCgsqClIKm3AAHEIKUyoKopJSCgqamprgAAhDSgpKC2KSApoK2goKUgoKCgsqCgqaCAAkUgoIACBKCgpSCgu6CgtaCgoLcgpSUlpKCgpSCuIIADBaygoKCgoKCABcwoqaCpoKCqqKCgoLawpSCgoKCpIKCgoKkgoKCgqSCtoKCpLiCgoKkggAJFpKqogAdOsKCpoKCpoLaooKmgoCC2riAgoLIuICmgoKC2oKCuIKUggACEuIAAiIADgKCgoKmgpSCpoKCgqqigoKCgoKUlIKClKaClJQ=", "964-971:1;1331-1333:1;1566-1591:1;1703-1703:1")]
[assembly: go.GoPositionMap("runtime/minmax.go", "minmax.cs", "AAkSgoKUpoKClKaAooCigKKA+KKClIK4pqKClIK4ptKUpJSm0pSklA==")]
[assembly: go.GoPositionMap("runtime/mpagealloc.go", "mpagealloc.cs", "AFK+AQAJAAAJFqKokqqiqJKmlLqSgpS8oqqiAAIQABASgoKssoIAeIACwriCgpSmpqampqassoKClKyy3uK4grimyoKSgpSCysqAggAIEIKEkoKClLiClMqU3AACIAAUEIKCgpSCgoKC7oKmAAQaAAgCpoK2poKCgpSSpqa4goKUgsrugoLKgpS4gqamgoKCgoKCAAUcAAgClIKSpIKUgoKCpoKCgoKCgoKClIKCgpSCAAIS4riCgoKCyqYACiYAEAIAGzgADh4ADR6CpoKSpoKCAAcQgpSClIK4ptyCgIIAAxoACwKCgqaCypSCuIKmgpS4goKClMqCgqaUpoKUlNyCgoKCgoKUAAsUgoKmgoKCuLiCggACHgAPAriCyoKUgoCCgoKCgpSCgtqCgtyUlITagpQAAhQACQKmgIKkgqaCgoKmkqSUgqaCgoKClIKmABQwkoKUrJKClKiSgpSokoKUqJKClK7qgpTKgu7ugpSm", "706-719:1")]
[assembly: go.GoPositionMap("runtime/mpagealloc_64bit.go", "mpagealloc_64bit.cs", "AB1CAA4eAAsYAAoW1oKmgoKCuJIABhwADAKCgtyCgtyCgoKCAAgSooIACximlNyCuO6ClIKmgriCgris0oKClAALGIKCgqaClIK4goK4poKClIKUgqassoKCgpKC", "109-112:1;117-125:2;130-133:3", "", "127=add/1/2/1,add/2/2/2")]
[assembly: go.GoPositionMap("runtime/mpagecache.go", "mpagecache.cs", "ABcykgACFgAIAoKUgoKCgoKUAAIS4oKClIKCgoIAAhgACwKUgpSCuIKCppSC3ICCpIIAAhgACQK4gpSCgoKUgoKClAAHEIKmgpSCggAIEoKCpqYACRSC", "", "", "141=chunkBase/1/1/1,alignDown/1/1/1,pages64/1/1/2,block64/1/1/3;158=alignDown/1/1/1,chunkPageIndex/1/2/2,pages64/1/1/2,chunkPageIndex/2/2/3,block64/1/1/3")]
[assembly: go.GoPositionMap("runtime/mpallocbits.go", "mpallocbits.cs", "AA4ekqiSqJKokoKUgqaCgoKUlIKCpqiSgryiqJKokoKUgqaCgoKUlIKUqJKqoqrCgpSCgoKUgoKClIIACRayooKCgoKCgpSCpoKClJSUhJKUlJS4goIABxCCgsqCgpSClIKClKaCgpSmuIKCgoKCgpTGAAIYAAkCgoKSgpSu4oKCgoKUlAACGgAMApKCgoKCuKaUgoKmgoKUlAACGgAMAqKCgoKClKaUgoKClIKClIKCgpSUgpSokqiSqJKokqiSrLKqoq74goKClIKmgoKUpsoADiLUgqrUgg==")]
[assembly: go.GoPositionMap("runtime/mprof.go", "mprof.cs", "AJgBtAKygoKCACJAsoKCqtKCgoKCgoLO2IKCgoKCggAOhAKCgpSCgqYAAhYACAIAAhTygoKmgoKCgoKuwoKCgoK4goIAAxAACQyUgoKCgoKoksq4gpSCgpSCgoLckrqSlIKUgoKCAAUW4oKCkoKmgoK4poKCpoKCvKKClAAEbsKCgpKCkoKClJSUlIKCgoKCgqSAgqTKgriUAEOIAYKUgoKUgqaCuIKUgoKmgoIADRyCgqa4gqamgqaAuIKUgoKCgpT+gqzSgpSAgsiipqaCAAkUlIKUgoKCgqaCgoKCkoKClIK4yoKUgoKAgoLIgoKUgoKCuKaCpoKClNyUgpKCgpSClAAFGPKClIKCqJKClIKCAAoaooKCpgAgRJCmkqqigoKmAAIwABUCgoIAAxwADQK4goKCgoKCgoKCgpSCpsqCgoKCgoKClIK4goKCgoLuuIKmooKCgoKClIKUgpSCqJKCgriCgoKCgpQACiQACgKCkoKUgpSmgoKCgoKCpoKCgqaCvuKCgoKUgoKCgu6ClKaCrNKCgoKUgpSC3IKokoKCAAMUAAkCgpKClIKUrOKCgoKUgoKCgsqmgqiSgoIAAxTigoKCvuKCgpSCgoKCpqiSgoK6kgALGAAXPoKmgqaCpqLKppSUggAHEIKCpriCgriCgrKUgpSC7oKCuIKCgqYADBqSpoKCgoKCuIKmgqYACxiC3sKClKyyyqaCgqaUpoIABxCCgoKUAAYWAAkCAAgSlIKCppS4AAoWkKSCuLKUtqaCpoKSgriCgtaCgqKUpoKCuLKCpqbckJKCgpS4gqaCAAISAAkCgoKClIKClKaCpgAKFoKmgoKCgg==", "463-465:1;581-589:1;815-819:1;1001-1004:1;1097-1100:1;1130-1133:1;1215-1218:1;1229-1232:1;1266-1269:1;1279-1283:1;1306-1309:1;1408-1410:1;1447-1449:2;1460-1462:3;1576-1576:1;1586-1590:1;1597-1601:2;1610-1612:3;1622-1642:4;1636-1636:4.1", "", "717=FuncPCABIInternal/1/1/2;938=stk/1/1/5;1058=stk/1/1/3;1146=stk/1/1/3")]
[assembly: go.GoPositionMap("runtime/mranges.go", "mranges.cs", "ACBCsoKClKiSgriokt7CgpKCkoKSgpSssoKClIKssoKClIKqooKUgpTMygAKGpKokqqiqqKqoqi2qJIAChqigoKClILOooKCgoKUggAEEsKqoqqigoKCgpQAGEiimrKSgoKmlLju3IKCpgACENKCgpSClIKUqJKCgpQABJABsoKUgoKCgoKClIKCqqKClIKClIKClICCgoKClIK2gg==")]
[assembly: go.GoPositionMap("runtime/msan0.go", "msan0.cs", "ABAmkKKAooCigKKA")]
[assembly: go.GoPositionMap("runtime/msize.go", "msize.cs", "AAscwoKUgriClKaCgpQ=")]
[assembly: go.GoPositionMap("runtime/mspanset.go", "mspanset.cs", "AEKUAdSCpoKCgoK4poKCgqaClIKClIKmlKaCAAoWuIKCyqqikoKCgoKUuILuyoKCgpSC/MrKgoK4ygANHJSmlAAHFPKCgoKUgtyCgpS4lLi4pqaCAAkasqyyAAcSkgALGqKAgqSokoIABhKiqJKokqiSAAcSkqiSqJKoktiSlIKClKiSAAcSkqiS")]
[assembly: go.GoPositionMap("runtime/mstats.go", "mstats.cs", "AKcCnAWCgIKCyICCggAUOrLcAAkWkqaCgoKWmoKmgoKCpoKCggAIEoK4goKUmpQAESSCgoKCgpSCgoKUgoKCgoKUgoKClIKCgsqCgoKCgpSCyoKCgoKCggAQIoKCgoKmgoKCgoK4goKCgoKCgoKCgtyokoIABhDClIK4lIKC7oKCgoKmgoKCggACENKUgoKClIIACTTSAAUQ0oKCggAbRPKCgoKCgpSCgoKClIKCggAvgAEAEwKAgoKUgqakggACIgAQAoCCgpSCpgAEEOKUggADEMKUggAXtAHCgoIAAhDW3KaCgoLcgqaCgoKmgoKmggAJFA==", "566-568:1", "", "392=load/4/9/1,load/5/9/1,load/6/9/1,load/7/9/2,load/8/9/2,load/9/9/2")]
[assembly: go.GoPositionMap("runtime/mwbbuf.go", "mwbbuf.cs", "AEucAZKCgriUpoIAAxLSqJIAAjIAFgKClIKCqqKClIKCAAIkABEIuILKggADHAANBIKCuJSUgpSCABAigoKC7pSCgriCgpSmgoKmgoKUgriU", "180-182:1")]
[assembly: go.GoPositionMap("runtime/netpoll.go", "netpoll.cs", "AHuQAoCigKKAooCmkgACFAAJAoKClIKUgpSmgoK+soKCgoKUgoKCggAYNsIABCCCrsLokoKCgoKUgoKUgpSUgoKCgoKCgoKCgpSCgoKU+LKClIKClIKClIKm1riCgpSUlIKCggACENKCgpSCkoKUAAIS4oKCpoKUgoKC3Ki2urKCgoKUgoKCgqamgpSClIKCgoKUgriCpIKCgpSCpoKCgqSCgoKUgsqCkoKUgpSCgpSClNiygoKUgoKCkoKCgoKCgpSCgpSCgpSClAACHAANAoKSgpSClIKUgpSmgoKClILKgpSmgoK4lKaC7sKCgriUgpSCyoCC/oKmgoKUAAIS4oKCpoKCgpSmlIKClIKCkoKUAAgKoqaCgpSUgpSCgoKClIKClIKCgpSCgpSCgpSClKaCpoKmgqiSqJKCuKKCgoKCgriCgoKCgoKCpoKCggACEPKCgoKokg==")]
[assembly: go.GoPositionMap("runtime/netpoll_windows.go", "netpoll_windows.cs", "ACNMtJSUgpSokoKUAAwgsoKUhJKClJSClAATJIKCgoK4gqaCgoKUppTWggAHMAAIAoKmgoKClKam7oKmpoKSgpKClJSSgpSClIKCgoKUgpSCgoKClIKUpoKCgpSkgpTqgqaqwqoABxCCyKSSgoKUgoCCggALEoKU")]
[assembly: go.GoPositionMap("runtime/os_nonopenbsd.go", "os_nonopenbsd.cs", "AAgWyg==")]
[assembly: go.GoPositionMap("runtime/os_windows.go", "os_windows.cs", "AJEBrAKCgoKatgAiTJKCpIKCpIKC3gAKDIKClIIADA6CgoKUgqiSpoKokoKCqJKCguaCgoKUlIKClIKUgoKUgoKmgvaChoqCgpSCgpSigoKmlLaSuIKSgoKCgoKCpoKokoKmgoKCAAUSAAsoAAoCuKaClAAOJsIABhIACAqCgoKCgsqCgoKUgoKCggAHEpK8koKCgtyClKaClJSUlIKUgpSU3KiSgoKUpriClIKClIKUgqaUgoKClKaYwoKU7tqCggACENKYgpSktpSCgoKCgriCgqaCpoKCAAYUoqaUkpSUgoKigoKUgoKUgoKCpoKCrLKCgpSC3viSzoKClIKCgoKClKaUlIKCuJSmpoK4goK4goK42LKCgoL8koKUgoKCgqaCgoKClIIABhT0uILKgpSCuAAFEgAJCNamqsi4usgACQqigoKCpoKCgqaCgoKCpoKCgoKUgoKCgqaYooKCggAHEJSCgoKUgoKUrLKCgoKClJQAAhLigoKUgoKUgoKUgoKUgoK6qrLakgACENKCgoKClIKmgpSCgpSokoKCgqrCgoKCqsKCgoKqwoKCgqrCgoKCqsKCgoKqwoKCgqrCgoKCqsKCgoKssqiSgrqSgpIABCyClJSkpKaCypSUqNiEkoKUgpSUpoKAgqSAgqSAgqTWgpSCgoKClKa4goKWkoKClNyUgpSmlIIAFEyigrimgriClIKCgpSCgoKUlpKCgu6mgoKCpoIACxiUpoKCgJSCqoKCgoIABxCCgoKCgu6CgoKCAAI9AAFCyKaUggACFPIACBIABBbi", "310-317:1;706-708:1;711-714:2;717-720:3;729-732:1;743-746:1;750-753:2;1069-1071:1", "", "327=efaceOf/1/1/1,compileCallback/1/1/1;704=getg/1/2/1,getg/2/2/1;797=FuncPCABI0/1/1/1")]
[assembly: go.GoPositionMap("runtime/panic.go", "panic.cs", "ACVwABIClLiCggADEvaCggAGRAAeAoKokoLcsoKokoKokoKokoKssoKokoKokoKkgoKkgoKkgoKokoKkgoKokoKkgoLYkoKqspKSkpKSkpKSkpKSkpKSkpTGkoKmxpKCpsSCgqbEgoKmxIKCpoKC2qKClKaCgoKCuAAHEMqSkpLGkpSkpKSUAAWMAQBDAoKUpoKCpoKCgriUgpSoktqigoKUgoKCgoKUgsqs0oKClIKUgoKCgqaClIKCgoKCpoIAAhIACAKClMqCgoIADBqCgpQABBTSgoKCgoKCgoKClJSAgoKCpIKUlJSCqJKCyoKmgqaCgoSigoKCgoKClJSUgoKCppSUgqyygpSCgoKClAAIROKCgujEtoKUtKS0qKKCgoKmgpSCgoKUAAIQ0oKCgoKCgpSCgoIAFiiAoqQABx4ADQKCgpS4goKCgoKmgoKCgpSCgoKCgoKClIKCgoKmgpSUgoKCgpQACBKC7pSCrNLcgpSClIColIKCtqaCAAgSgu7ilIKCpoKC3JSCggAJFIKCuKaClKaCgIKCgoKmyqaUtoLMwoKmgpKCgIK2goKCgoIABxCCpoKmpoKClKbWgoKCpoKmgoKCppSCgpQAAhYADg6CgoKClKiSqJKokqiSqJKokqiSqJIAGJoBAAkCgoKmABMogoKCppSUgqaClLgAI0iCgtyCgrjK6siCAAIQ0oKClILKkoKmlLimpgACENKCgoKmkt6Upqa4poKmAAIeAAwCgoLcuIKmtoKCgoKUgsiCgsiCgoK2ggAJEsKCgoKUlKaCgoKUgoKCkoKClIKCppTKgqaUrsKCyoKCpoKClIKCgpSCgpSCAAIQ0twAChamlKa4AAIU8oKClA==", "645-655:1;942-977:1;1276-1291:1;1308-1322:1;1331-1333:2")]
[assembly: go.GoPositionMap("runtime/pinner.go", "pinner.cs", "ACC2AaKClILKguaCgoKClICCpJSUAAg+woKCgrjKgoKUlJSCgoKUgoKUppKCuLiCgpSSgpSUuLimgoIACBiypoKmgqaCqqKCgpSClAAGFsKCgqaCrLKssqaCrNKCgqaC3IKCgriCgoKUvKKCgoKCgpSCgoKClJTawoKClIKCgoKClIKCgpQABiKC", "182-185:1;196-199:2")]
[assembly: go.GoPositionMap("runtime/plugin.go", "plugin.cs", "ABEasoKCgoKUlIKUgpSCpoKCgqaIgoKCgoKCgoKCpoKCgsqClIKUgoKUAAkUgoKCgoKClIKClJTmooKCgoKmgsqCgoKCgpSCpoK6kg==", "", "", "46=inRange/2/4/1,inRange/3/4/2,inRange/4/4/3")]
[assembly: go.GoPositionMap("runtime/preempt.go", "preempt.cs", "AE7SAQAfAoC4uJKmgoKCgoKAAAgoAAoUgriUgvyC3IKCAA0c6oK4griCgqaCgoKClO7cgoKCAAbVAdymggAL2gGClIKUgv6ilKaCgAACEKuCAAMQlAADEtIAAhYACQaSgoKClJTugoKCgpSCAAoWgrzUAAgmABICyoK4griCuIKUlAAIEpSCuJSAAAcQtoKCABAolLiClLaU", "", "", "455=HasPrefix/2/4/1,HasPrefix/3/4/2,HasPrefix/4/4/3")]
[assembly: go.GoPositionMap("runtime/print.go", "print.cs", "ABdO8pSUgoKCgrgABRrygoKCgpSmgoKCgryigpSC3IKCpoKmgqaC5oKClAAICIKUgqSCpIKmgoKCgoKCpoKCuIKClIKCuIKClIKCgsqCgoKClIKUgoKCgqaCgoKmgqaCgoKCgoKUlKaCgoKU+oKCgoKCgoKUlIKCgoKmgqSCpoKmooKCpoKmggACENKCgoKCgoKClKaCgoKmgoKCpoKCpoKC")]
[assembly: go.GoPositionMap("runtime/proc.go", "proc.cs", "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", "172-174:1;205-209:2;326-326:1;455-457:1;570-577:1;1217-1222:1;1235-1239:2;1255-1257:3;2056-2077:1;2266-2268:1;4833-4835:1;4855-4860:1;5062-5064:1;5080-5089:1;5311-5316:1;5320-5322:2;5743-5752:1;5965-5967:1;5972-5979:2;6066-6082:1;6507-6521:1;6904-6906:1")]
[assembly: go.GoPositionMap("runtime/profbuf.go", "profbuf.cs", "AG/oAYKmgqaCpoKmggACEOSokqiSrOKCgoKCgoKmgpSClKqigriUgoLcgpSC/qKAgv6ClIKUgpSUgoKCgoKqwoKmgriCgoKmlAACEPKCpoK4poKCpoKUgqaCpoKUAAfGAaKClIKssoKCgoKUgpQADxzCgqbugoKCgoKCgIK2poKCgoKmgpSmgoKCgoKUlJTKgsqUlIKmgoKUgoKUlIKUgoKCuIKClIKC7oKCgoKUgriUAAkUpg==")]
[assembly: go.GoPositionMap("runtime/proflabel.go", "proflabel.cs", "AAwoABsmgpQAAhgACQI=")]
[assembly: go.GoPositionMap("runtime/race0.go", "race0.cs", "ABAmkKKAooCAooCigICigKKAooCigKKAooCigKKAooCigKKAooCigKKAooCigKKAooCigICigKKA")]
[assembly: go.GoPositionMap("runtime/rand.go", "rand.cs", "AClMooKCpoK4gqa4gqaClKaCgoKAgoKklIKUpoIAAhAACQqCgoKCgoKUgpSCuIKCgpTYkoKClIKAgoKkvKKCgpSCrLIAAhQACwqCgsqCgpSCgrqSqLKCgpSCggACEOQAAiwAEwK4hoKCAAcQgoKCgoIAAioAEgIAAiYAEQQAAhYACAKokqiS", "", "", "51=allZero/1/2/3,allZero/2/2/3")]
[assembly: go.GoPositionMap("runtime/rdebug.go", "rdebug.cs", "AAkUsoKCqLKCgoI=")]
[assembly: go.GoPositionMap("runtime/runtime.go", "runtime.cs", "ABw+soKCggATVAAXBIKCuIKCgoKCuIKmlIKmlIKClKaU7pCmkKaQqKIADRKSgoKCqJKCggAKFqKCgoKClIKCgpSCpoKUpoKCgoCCpIKUgrqSgoKCgoK6koKCggADEMIADTjsgqYAFi6ClAAJLgANAA==")]
[assembly: go.GoPositionMap("runtime/runtime1.go", "runtime1.cs", "AB1KAA0CgoKCgoKSppSUAAUWsqaCgoKmgoKUgoK4uIKCpoKCuIIADw6CgoKClIKUgoKUgpSClIKClIKUgpSClIIAMAiCAA4eAAAQlIKUgpSClIKUgpSClIKUgpSClIKUgpSClIKUgpSCpoKmgoKClIKmgoKUgqaCgpSCpoKCgqaCgoKmgoKUgqaCgpSCpoKClIKmgoKUgqaUgqaCAEWOAQAkQpSCgoIACBKUlJSCgqaClIKSgtymlIKUgqqilJSUgoIABx4ACgKCgpSCgpS4goKUgpSmgoKUkoKUgtyCgIK2goKAgoKSggAIEIK6koKkpKSkpLSCgoKUtIKAgtqCppQAAhQACQKCgoKmpoKClJSClKyygoKokoKClAADJgAPAoKCgoKClAACHAALAgACIgAOAgACHAALAqyyrLKssoKCgoKUgoKCgoKUgqiSqJI=")]
[assembly: go.GoPositionMap("runtime/runtime_boring.go", "runtime_boring.cs", "AAkUpIKUqJA=")]
[assembly: go.GoPositionMap("runtime/rwmutex.go", "rwmutex.cs", "ABx4AB4ClILM+oKUlIKClIK4goKCgoIACA6ygIKCppSCgoKUtqikgpSUgpSSgoKCpurEgoKmgoKCgoKCuIKU", "80-96:1;131-136:1")]
[assembly: go.GoPositionMap("runtime/security_nonunix.go", "security_nonunix.cs", "AAgSgqY=")]
[assembly: go.GoPositionMap("runtime/select.go", "select.cs", "ABk0kpaCpoKCgoKCgsoACBKCgoKUuPy4AAYWwoLulJSClKaCAAgkABECgoLKgpSCgoKssoKClJKClKaCggALGIKCgqaCgqaCgqamgqaCgoKUgpSCpsqClIKCgoKUlIKCgoKCgoKClIKUgoKClJSmgoKCgtyUAAcWkoKCgoKCgoKUgoKClIKUgqaClIKUgoKUgsqCgoK4gpSCgoKCgoKCpoKCgpSUgpSClKaCysqCgpSUgoLcgoKClIKCgpSUgoKClJSCgoKCpoKClKaCgoKmgqaUgqaCgqamgoKSgqaCgpKCpoKCkoK4gpSEkoKUlIKUgpSCgoKUgoKClIKClISSgpSClIKUgoKClIKClISwkoKUgpSEkoKClIKUlISSlIKUgpSgkoKUlIKClJSEkqaiABMukoKUgoKSgoKClIKkgqSCpoK4griCgqaCgoKCgpSmpoKUlKaigoKClIKCgoKmgoKClJSCgoLKgoI=", "147-152:1;497-497:2;527-527:3")]
[assembly: go.GoPositionMap("runtime/sema.go", "sema.cs", "ADtwggACGgAKAqiSAAIaAAoCqJKokqiSqJKokqiSpoKClAAHFpIABHiCAASkAYKCgoKUgszCgoKCgpSCgoKUlIKCgoKCgoKUgqaCgoKUgoKUgoKCpoKUlIKCgqaUgoKUAA4egoKmgoKUgpQABBgACwKCgoKClIKUppSCgoKUgJSCgoKCgpSCgpSClJSCgsqCgoKCpoKClLiCgpSmlKSCgoKCgtrUgoKUgoKCgqaCgpKClIKU7NSCgpSCgoKCpoKCkoKUgpQAGDqiAAIQ9q7ipoKCuIKCgoKCgoKUgpSUgoKClKz2gsqCgoLcgqaCgoKCgpSCvvaCpqaCgoK4AA4egoKCgpSUgpSCgoKClIKm2JKCgrqS")]
[assembly: go.GoPositionMap("runtime/signal_windows.go", "signal_windows.cs", "ABMqgoKcwoIABBqikpKSlIKCppSCAAMQ4oK4lAACFAAMCIK4ABcEAAEaAAYc5IKClAAFFgAKAoKCpoKUpKSkAAIiAA4CgqKmlIIADRyUlIKCggACFgAMAoKmygAIEoKCggAOHoKCgoKCgoKUpoIAAhDygpQACxiCgpKCgoKUgoKmAAIWAAgCgpQAAhIACQK4AAgShKaCrPKUgpTKgoKUlIKCgpSUlIKUgoKCgqaCpuaCgoKmlIKUgpS4lJSkpKyUqrampqaCqJKu4oKCpsqCuMqC", "173-175:1", "", "366=ip/1/2/1,ip/2/2/1")]
[assembly: go.GoPositionMap("runtime/sigqueue.go", "sigqueue.cs", "AESOAbKCgqamgIKCyIKCgoKUgsqCgpiCyKSCgoKUggADHQAFKILewpSCgoLKgoKYgoKClIKCtoIAAxkABiaCAAQcAA4MgtyCvrKUgoKUuIKmgoKUgoKUrLKClJSCgqyygpSUgoKUgoIAAhDSgoKssoI=")]
[assembly: go.GoPositionMap("runtime/sigqueue_note.go", "sigqueue_note.cs", "AA8cgtaC1oI=")]
[assembly: go.GoPositionMap("runtime/sizeclasses.go", "sizeclasses.cs", "AFu+AZKSkpI=")]
[assembly: go.GoPositionMap("runtime/slice.go", "slice.cs", "AB84gtaCqsKSgoKCgpTKgqaCgoKCuILuuIKCgpSClIKmlAACGAALAoLcgoKUpqaCgoKmgoKmAAVeAC4CgoKClIKUgqaCpqamlILqgpSCgoKCtIKCgoK0gpSUlIKCgoKCtIKCgoKCABAggqaCgsqmgu6mlKiSgoKCpoKClLjcgtyClAACGAANBoLKgoKClIKmgqiSgqaCgqaCpoKCgoKClIKClIKCppSUlKiSgpSC")]
[assembly: go.GoPositionMap("runtime/softfloat64.go", "softfloat64.cs", "ABo64oKClJSCgpSCuIKCgoLcgpSm4oKClJSCgpSCuIKCgoLcgpSmgqKClIKClIKCgpSCgoKCgqaClIKUgoKmgoKCgpSClIKCgqamgqKClIKClIKCgpSCgoKCgqaClIKUgoKmgoKCgpSClIKCgqamgoKmlKampqamgpSmqIKmgoKCgoKClIKCpoKUpoKmgqaCgqaUpqampriCgoKCpoKCppSmpqamlNaCgqaCgoKUgoKUgqaCgoKClIKClKaCgpSUpqam3Ka4pqKUlKamgpSClKaCgpSCgpSCgpSmgoKCgpSkgoKCgsqCgoKCgqaqopiCgoKCgoKCgoKqopSCuIKCgqaCgoKCgoKClIKCgoKCuIKClIKCgoKCuKaCpoKmlKaCgqaCgqaCgqaCgqaCgqaCgqaCpoKmgqaCpoKCpoKCpoKCpoKCpoKCgpSCgqaCgoKUgoKmgoKmgoKCgqaCgqaCgoKC")]
[assembly: go.GoPositionMap("runtime/stack.go", "stack.cs", "AKYBzgKCgpSCgpSCgrqSgoKClPqigoKClIKClIKUgpSCgoKCgpSUgoKUgoKUlNiSgoKUlJSCgoIADyCCgoIAAxDCgqqigoKCgoKClIKCqJKClIKCgoKCgoKUgoKokoKUgoKCgoKClIKCAAgUAAkIgoKUgpSCpoKCgoKUrLKCgoKCgpSCyoKClIKCgoKUgpSUgoKmgoKClJSUlIKClIKUpoKCgoKmgpSClIKUgpQABxLigoKCgpSClIKClIKClJSUgoKCgqaClIKUgoKCgoKUgoKCgpSCgpSCgqaCgoKUpoLugoKC7pQAK2CigoKClIKCggALIMKC2sKCgoLcgoKCgqaCgoKCgoKCpoKClIKClIKCgqYACRCylJSCgriCgpSmgoKCgu6mxoKCuIKClMqCgoKCgoKUgriUgoKCAAkOooKClIKCgoKCpoKCuIKCytiCgoKCuKampoK4goK4goKCgoKmrLKCqJKCAAkUlLiasoKCgoK4goKClKb6woKUgoKUyqaCgpSCqJKCpoKCypQACBK4uMqCgoKCuIKCgpaSgriClKiSgoKUAA0eAAwClIKUgoKCgqaUlIKCgpKCgoKUyoKCpoKCgoLKAA0cgoKmgriClIKUlIK4goKCpoKClIKmpoKmgri4gsqAgoKCgoLIuKaCgpSUgsq4goKUgqiSqsKCgpSUrgAKCoLKgsqCAAkUgpQABIIBpIKCgoKCgoKCgpSUuIKCgoKCgoKmAA4k0oKCgoKC3ILewgAIEoKCAAEYAAoCgoKClIKUgoKUpoKUgqY=")]
[assembly: go.GoPositionMap("runtime/stkframe.go", "stkframe.cs", "AESQAZKCuIIABR4ADwKCgoKmypSCppQACxiCgpSUgriCgoKUgqaCgrjq4oKUpoKCyoKUuLiCgpSklIKCgoKCpoKUgpSCgqSCupKCpoKCgpSUgpSClMrMlIKCgoKCAAgSAAgKgqKmgoKCgoKmgpQ=", "", "", "299=alignUp/1/1/1,getGCMask/1/1/4")]
[assembly: go.GoPositionMap("runtime/string.go", "string.cs", "ABk00oKCgoKCgpSClIKClILcgpSCgoKUpoKmgqaCpoKssoKCgoKUlJSmgoKCgqamgqaCpoKmggACEgAIAriUgsqClIKUgoKClKaCgpSUgqqigoKmwoKClJQAAiIADgKCyoKUgpSmooKCgpSUgqbGgoKmgoKClKaCgoKUpoKCyoKUgpSCgoKUgoKUgpSUAA0cgqaygoKClJSClIIABBqygoKCpoKosoKUgoKCpoLYsoKmgqaClIKssoKClIKCrLKmgoKUgoIABRaygqaCgoKmgoKCgpSUlIKClJSmgpSCpoKCpqqigIKkqqKAgqQAAiAADgSCpoKCgoKUyoKmgJSCgpSSgsiClIKUtLS0xpSCgpSCgpSClJSClJSokoLcgoKCgpSu1ILIpIKUgIK2goK4goKUgoKClKiSkoKmgoKCgoKUgoKUgpSUgg==", "", "", "140=GetCallerPC/1/1/2,FuncPCABIInternal/1/1/3;207=GetCallerPC/1/1/2,FuncPCABIInternal/1/1/3;263=GetCallerPC/1/1/2,FuncPCABIInternal/1/1/3")]
[assembly: go.GoPositionMap("runtime/stubs.go", "stubs.cs", "AAswAAwCrAAEIAAPKAAWCKIAAkgAIgaSAAJCAB8GkgAFJAANLAAUAoKuwoIAAhgACgSUkpIAAVwALRoADxwACyIAEAakAAEYAAsEnurCkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpSasqyyrNaopJbWAAIQ5gACFgAKBJKSkpKSkpKWrA==")]
[assembly: go.GoPositionMap("runtime/stubs3.go", "stubs3.cs", "AAgU")]
[assembly: go.GoPositionMap("runtime/stubs_amd64.go", "stubs_amd64.cs", "AAkUopKSkpKSlqamwpKSkpKSkpKSkpKSkpKWpqzSmA==")]
[assembly: go.GoPositionMap("runtime/stubs_nonlinux.go", "stubs_nonlinux.cs", "AAgUkg==")]
[assembly: go.GoPositionMap("runtime/stubs_nonwasm.go", "stubs_nonwasm.cs", "AAgUkA==")]
[assembly: go.GoPositionMap("runtime/symtab.go", "symtab.cs", "AEWgAbKCggAEhgIACwIAAiAADQKClIKCAAIgAA8GgpSClIKUpoKU3KaUgoKmlKasspKUlKaCggAJFIKU7oKUAA0cgqaCptiCgoKClIKCgqYAd5YCAAoCgoKUAAIqABICgoKClIKCgoKCggAMGoKCgoK4ABs6goIADCAADQSChKa4gpSCgoKCgpSCgpSClLiCgoKCpoKCggAHLgASAoKCgpSCgqaCgqYAAhLigoKClJSUgpSCgriokoKUABWwAaKCgoKUAAIS4oKCpgAHEIKmgqiSAAIYAAkCqAACIgAPAoKClJSCgqaCgpSCpoKmggAMGoKClAACFPKClKgADyjC+Ja0gqyygoKCgsqCgtyCgoKCgpSCgsimlIKmgoKClJSCgoKCgoKCgoKU7oKCgqaCgoKCgoKC7oKmlMqCppSCgoKCgoKClKaCpoKClKaCgoKCgqaCgqamgoKCpoCCtgACGAAMAoKClIKClJSCpoLWgoKCgpSokoKCgoKCgoKCgpS4gqaCgpSCpoKClIKokoKUqqKClIKChqKClIKCqLaCgpSCgpSClIKCgpSCgqiSooKCgoKClJQACxTogpQ=", "", "", "188=gostring/1/2/3,gostring/2/2/4")]
[assembly: go.GoPositionMap("runtime/symtabinl.go", "symtabinl.cs", "ADKGAQAQAoKClIKCtoIABxCCqJKCgpSCqJIAAhwACwKClIIABxIAAhAACAKC", "", "", "79=pcdatavalue1/1/1/4")]
[assembly: go.GoPositionMap("runtime/synctest.go", "synctest.cs", "ACZMAA0SgoKUgqSCpoKUgqSCyoKmgoKCgpSCgriClIKUgoKCvMKCgtiygoKClIKCgrqSggAKFoKAlMim6ujSgqaCgpTKgoKCgoKSpoKUgoKCgoKUgpKUgoKClIKClIKUpoKCgpSmtKSigoKUlJSCqJKCgpS4goKUgoKUgoKClIKCuILoooKUlIKCqJKAgoKkqJKosoKClIKSlKI=", "173-175:1;188-190:2;297-299:1")]
[assembly: go.GoPositionMap("runtime/sys_nonppc64x.go", "sys_nonppc64x.cs", "AAgS")]
[assembly: go.GoPositionMap("runtime/sys_x86.go", "sys_x86.cs", "AAwgooKCgoKC")]
[assembly: go.GoPositionMap("runtime/syscall_windows.go", "syscall_windows.cs", "ABcqgsqCuIKClAAZOIKClIKClAASKqIACRSUgAAHELaUggAKFoLKgrim3O6CpsoAAhIACAKAtraCtoKCtoKCgoKmyAACENKClNyCAAcQAAEWAAkCgpjIqwABEAAHGgAIApSmgpSWkoK4gpSClIKUgLikuMqCgoKqooKmlKaAgoLIgpSCgoKUgoKClIIAFS6SgpaSgoKClKSCpNyCuO6ClLjewoKCgpQAAhAACAKCgoKUAAIQAAgCgoKClKqiqqKqoqqiqqKqogAJGMKosoKUgtyCgoKClLiC")]
[assembly: go.GoPositionMap("runtime/tagptr_64bit.go", "tagptr_64bit.cs", "ADZyooKClJSClKiSppSClIKUqJI=")]
[assembly: go.GoPositionMap("runtime/test_amd64.go", "test_amd64.cs", "/g==")]
[assembly: go.GoPositionMap("runtime/time.go", "time.cs", "AA8iwoCCgoKkqJKCgpQARaAB4oKCACJUgqYACRCUABo6goK4ooKUgoKCpoKUpoKC6pKC2LKmgqrCgsoABRQACQKCgoKClIKUgoKCppSCgoKCpgAJJPKCpoKCgoKCgpSUgoCClKSCgpSCuIKUAAMQ4oIADyDigoKCgpSCgoKCgqaAgqSCgq7igpQAAhDygpSClKyyAAUQAAgCpoKmgpSCgoKCAAMS8oIABxCAgoKUAAgS4oKCpoKCgpSCgoKCpoKUpu6CpoKCgoK42sKCgoKUgoKClIKCgpSClAAIEOKClIKUlIKmgoKUgoKCgoKCpoKCgoKCpoK4gIKmgsiUpu6CpoKCgpSmgpSCpurCgoKClJQABioAHRSCgoKCgpSUlIKCgoKCgoKCgoKClIKCgoK+su7igoKCgoLugu6CgIKCgoKCgoKUgraCgqaUpoKCgpQABxQACAKCgriCgoKClIKmgoKCgoIABhLygtyCgoKClAA1bIKUgoKCgoKmlKaClLaCgoKCgoKCgraCgqSmgpSUggADFAALDIKCgpQABRoADgKmgpSmgtyCgpSUlKaCgoKUgIKClKQABxCCgqaUAAUWAAoCgoKCgpSCgoKmlKaCgoKmgqaUgqaCggAJEgAIAoKCgpS4goKUpoKmgoKCgoKUgoKmlIKCgoKCgpSmgpSCuJSUgoKUpoKmlIKClIKmABImlLiCuILIlIKmgoKCpoKmgoL+0oKClLiCgoKmgIKCzMKCgpS8ooKCgpSCAAQQsqaCgqamgIK2lAAEHgAKAoKClIKCgpSCgoKUgpSCvKKCgoKUgpSCgoKUgoKClIKCgoKCpoKUgpSCvMaClIIABhDCAAgQ8oKCgoKUgoKClIK4lKamgoKClIKUgoIABhDCgoKUgoKCpsqCggAIEoKCggAGFOKCgpSCgoKUgriClA==", "460-462:1;1172-1172:1;1406-1408:1")]
[assembly: go.GoPositionMap("runtime/time_nofake.go", "time_nofake.cs", "ABBAAA8CABAosoKU")]
[assembly: go.GoPositionMap("runtime/timeasm.go", "timeasm.cs", "AAsc")]
[assembly: go.GoPositionMap("runtime/tls_windows_amd64.go", "tls_windows_amd64.cs", "AAcU")]
[assembly: go.GoPositionMap("runtime/trace.go", "trace.cs", "AJoBlAUACQLKgpSC7oYAABTulIKmkoKmAAoWgoKCgpSCgoKUgpSCuJQACRSCpoKCyoKCgrimAAgSgoKC7oKCgu6CABImgpKClIKClAABEOKUgpSCgoKUgoLcgoKCgqamgoKmgqamgoKCgqYACRSCgpTcgpSm7oKCgqaCgoIAChaCgpSUlKbugoIACRSCgoKClJS4goLKgoKClIKClIKmgoKCpoKUpgALGIKCgoKClAAHEJS4griCuJSssoKUgpSCAAheAAgClIKUpoDslLiCgqaAgoKCtoKCgsqClIKCgriC7oKC3LiUuMqC3JSUpoIAChaCgqaCgoIABhLigoKUrAARHoKUAAsYxIKCgpSmlLqSgoKCABIqkoKCgoKClIIAAhDygoKClIKClIKCrPaCgpTcAAgKgpQAAhQACgSCgpSCppSClII=", "360-406:1;374-402:1.1;426-439:2;508-558:3;594-598:4;639-645:5;657-680:6;797-797:1;957-966:1;1000-1002:1")]
[assembly: go.GoPositionMap("runtime/traceallocfree.go", "traceallocfree.cs", "ABQwwpiigoKUuIKCgqamkoK4goKmpoK4goKCgpTKkpSmooKUqLKosqiSqLKssqyyqJKokqiSgqiSgqiSqJLYkoKU", "77-79:1")]
[assembly: go.GoPositionMap("runtime/traceback.go", "traceback.cs", "AG30AQAKDubEgAANHLaCgoKCgqaCgoLKgoKCgsqCgoKUggAHENyCpoKCgoKUgpSClKYABxCCpoIABTAAFwKClIKmgriCypS4ytyC7oKCgoKCgoKC2AAHEIKUgoKCgraClMqUkgAZNIKClJSCgoKCpoKCyoKUABMogqYAChaCgoIAChYACAqigoKmgoKUgsqCgsqUgoKUgpSCgqaUgoKmgoKUuIKCgoKCuIKCgoKCgpKCuNyyAClUgoKCggADHAAKApSmrsKmpoKCgoKCpgACGAALAoKCgoKmgoKkggAHEIKUuIKmqJKCgqaCgoKCpoKClIKUgqaSlIKCgpSmgoK4goKCpoKCgoKCgpSkgoKCpKSCpIKkgoKCgoKm3sSCgpSCgpSClKiSgqqigoKUgqa0goKCuIKCgoKUgoKClIKCgpSmggACFgAKApSClKa07oKCgpSmlIKClKaCggAuXoKCgoKCgpTKkoKCgoKSppS4gpSUgpSCAAMSAAsGgpSUgoKUgqaCpoKCgoKCgoKCgoKCpoCCkoK2gsqCgoKUgpSCgoKClIKmuICCgoKCgoKAgpKCtoKCgriUgpSCyKqigoKCgqaCpoKmAAMQwoKCgoKCgpQABDSCgoKqooKClKqigpSmgqbugqaqtIKClLiCgpSCgpSCyqrGpgAKFqKUlIKWkoKUuIKokoKUgoKCgpSmgoKUgpSClICCpKaCpoKCgoIACRSSgpSCyoKClAAEENKClJSClIKmlIKUgqaClIK4goKUpKSUAAMYAAkEgoKUgpSmppSUAALIAgChAQKCpoamgoK4ggAhQpKCgoKUlKaAkoKCgpSUgqzSgoKAgpKCtoK4lJSCgpSCgsySgqaUgpSClKiSgpSCppTagpSClA==", "669-678:1;680-695:2;698-702:3;900-923:1;947-962:1;1265-1281:1;1318-1328:1;1584-1584:1", "", "216=guintptr/1/1/2;1702=noescape/1/1/2")]
[assembly: go.GoPositionMap("runtime/tracebuf.go", "tracebuf.cs", "AB5UAAsClIKCgqYAAhgACQIAAhIACwqmgoKUgqaCgoKUAAIS4qaUgqaCggAEGgAIAoKClAACEuKCgoKUlILYkoKCgpSCgoKUgoKCyoKClIKCpoKCuIKUgpSCgoKCAAcSsoKClJSokoKClIKClIKmggATNOKCAAIS4oKCgoKCgpSClAACFgAIAoKCAAIS4q7CAAUYAAkCgoKUlIKUggADEvIACRSCuII=", "143-149:1;156-172:1")]
[assembly: go.GoPositionMap("runtime/tracecpu.go", "tracecpu.cs", "ABAYsoK4ggAHEILewoKmgoKSAAoWlJKUlIKCgqaU3LKCAAkUgoKCgqamgoKCAAIgAA0ClIKCgpSClIK4goKCgIKkgoK4psqUpriCgoK4lpKClLimgoKCgoKUlKq0gIKSgoKCAAUQ8qaUpgAHEIKCgpSCpoKUporCppSUgpSmlKaAuLamgg==", "42-68:1;199-204:1")]
[assembly: go.GoPositionMap("runtime/traceevent.go", "traceevent.cs", "AGryAQANAoCCpICCpKiyrLIAAhDkAAUSsqyyqJI=", "", "", "155=startPCForTrace/1/1/2")]
[assembly: go.GoPositionMap("runtime/traceexp.go", "traceexp.cs", "AAcSogACHAALAg==")]
[assembly: go.GoPositionMap("runtime/tracemap.go", "tracemap.cs", "ADVyogACEuKClJSCgoKCggAPIIKUgsqUgoKmggAFMsKCgg==")]
[assembly: go.GoPositionMap("runtime/traceregion.go", "traceregion.cs", "AC1WsoKClIK4goKCgsq4goKCgoK4griCgsqCpoKCruKCgoKClICCgqQ=")]
[assembly: go.GoPositionMap("runtime/traceruntime.go", "traceruntime.cs", "ABEokgAcRLaCgoKCgoKCggACEuIAHUAAHUAABRKyrsKokgARLtKClAAFFAAKCKbKgoIACBKCggAKFoKCgoKUAAIQ0gACENKClIKCgqassqiSqJKssriotq7Cpq7Cpq7Cpqi2qLYABRQACQaCgpQAAhYACAKCgoKClAAGEtKCgpSCgpSokqiSqLKCgoKUrLKCgoKCgr6yqJKokqiSrsKoxIKq1IKCgoKUqsK4vsSCggACGAAJAoKCgoKUlAACEAAIBILupgAIEoKCgoKUqJKokoKUlAACEgAKBoIAAhIACAYAAhLigpSUgqyygpSUggACEuKClJSClLS0lIKssoKUlIIAAhLiAAgSgoKUgoKCgqaCuJSCgoI=", "695-708:1")]
[assembly: go.GoPositionMap("runtime/tracestack.go", "tracestack.cs", "AB5MAAwClpKCgriClMq0gpS0gsqmlILugoKSgriCgpIAECKCgpSCuIKUgpSCAAgWooKUgqyygoKAgqSCpqK4grie0oKCuIKCgoKCgoK4goKClJSqooKCgoKCggALGpKClIKCgpSCgoKClIKqoq7ilJSUqJIAAhgACQKCkqam/oKSgoKUqIKCgoKmgIKkpoKCgqSAgqTGrLKCgpSCgpQ=", "294-302:1")]
[assembly: go.GoPositionMap("runtime/tracestatus.go", "tracestatus.cs", "AC9s9IKCuIKUuIKUAAUYAAsCgpSClIKmtsqCtqSUggAFGAAKBIKCuKaClAAGFgAIBJKUpKQACRKCgraklAAWPOKClIKokoKCqJKqoqiSgg==")]
[assembly: go.GoPositionMap("runtime/tracestring.go", "tracestring.cs", "ABcyxIKClIKmqKSCgpQAAhAACASCpoKWkoKUuIKCgqaCruKCgoKClLg=", "31-33:1;42-44:1;87-91:1")]
[assembly: go.GoPositionMap("runtime/tracetime.go", "tracetime.cs", "ACxyAAsCgpSqopS4rLKmpoKmkoKC", "90-94:1")]
[assembly: go.GoPositionMap("runtime/tracetype.go", "tracetype.cs", "ABUy4oKmkqyygoCCpIKmooKmntKClLiCgoKCgqaCgoKUlA==")]
[assembly: go.GoPositionMap("runtime/type.go", "type.cs", "ACM4goKClKaCpoKClIKCgoKUtKSUrsKAgqSUgqSClAACEPKUlAAHFAALDpSCpoKSzoK2griCgoK4ggACKwALSNS4gqaUgoKClIKCgqaCpoKCgoKUgqaC6gAMEoKClJSCuJSCpoKUgoKCtoKCgoKClLiClJSUgoK2ABkwgoKCuIKClOaCgpSCgoKCgoKUyoKCgoKCgpSUpoLmgqaUgoKCgoKmgoKCgoKCgpSUlICCpIKCgpSmgtaCppSCgoKCgqaCgoKCgoKClJSUggACKoKClIKCgoKUgoKCqqKClJSCgpSCgoKClKaCgoKmxriCgoKCgoKCgoKmuAAIEoIABR4ADwKCgILslIKUgoKUgoKUgoKCgpSCgoKmgpSUpIKCpIKCpIKCgpSCgoKmgoKCpqSCgoKUgpSCgqaCgoKUgpSCgoKmpIKCgpSCgqSCgqSCgqSCgoKUgpSCgoKClIKUgpSClIKmpIKC", "131-133:1")]
[assembly: go.GoPositionMap("runtime/typekind.go", "typekind.cs", "AAoUkg==")]
[assembly: go.GoPositionMap("runtime/unsafe.go", "unsafe.cs", "AAwagoKmgoKUupKCgpTWgriC6ILWgqiSgqaCgriCgoKUupKCgpTWgriCuKbYkoKmptiSgqiS")]
[assembly: go.GoPositionMap("runtime/utf8.go", "utf8.cs", "ACRWkoKClAACGAAMApSCppSmgoKCguqCgoKC6oKCgoLaqrSApIKkgoKCpIKkgoKCgqSCgoKCgg==")]
[assembly: go.GoPositionMap("runtime/vdso_in_none.go", "vdso_in_none.cs", "AAgWkg==")]
[assembly: go.GoPositionMap("runtime/vgetrandom_unsupported.go", "vgetrandom_unsupported.cs", "AAoYkqak")]
[assembly: go.GoPositionMap("runtime/write_err.go", "write_err.cs", "AAgUkoI=")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("runtime")]
public static partial class runtime_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface floaty<ΔT> {}
    internal partial interface ifaceHash_i {}
    internal partial interface stringer {}
    internal partial struct _DISPATCHER_CONTEXT {}
    [GoValueClone("csdVersion")] internal partial struct _OSVERSIONINFOW {}
    internal partial struct _typePair {}
    internal partial struct abiDesc {}
    internal partial struct abiPart {}
    internal partial struct abiPartKind {}
    internal partial struct activeSweep {}
    internal partial struct addrRange {}
    internal partial struct addrRanges {}
    internal partial struct adjustinfo {}
    internal partial struct arenaHint {}
    internal partial struct arenaIdx {}
    internal partial struct argset {}
    internal partial struct atomicHeadTailIndex {}
    internal partial struct atomicMSpanPointer {}
    internal partial struct atomicOffAddr {}
    internal partial struct atomicScavChunkData {}
    internal partial struct atomicSpanSetSpinePointer {}
    internal partial struct bitCursor {}
    internal partial struct bitvector {}
    internal partial struct blockRecord {}
    internal partial struct boundsError {}
    internal partial struct boundsErrorCode {}
    internal partial struct bucket {}
    internal partial struct bucketType {}
    internal partial struct buckhashArray {}
    internal partial struct callbackArgs {}
    [GoValueClone("ctxt")] internal partial struct cbsᴛ1 {}
    internal partial struct cgoContextArg {}
    internal partial struct cgoSymbolizerArg {}
    internal partial struct cgoTracebackArg {}
    internal partial struct cgothreadstart {}
    [GoLocalName("x1t")] internal partial struct check_x1t {}
    [GoLocalName("y1t")] internal partial struct check_y1t {}
    [GoValueClone("b")] internal partial struct checkmarksMap {}
    internal partial struct childInfo {}
    internal partial struct chunkIdx {}
    [GoValueClone("stats")] internal partial struct consistentHeapStats {}
    [GoValueClone("anon0", "vectorregister")] internal partial struct context {}
    internal partial struct coro {}
    [GoValueClone("extra")] internal partial struct cpuProfile {}
    internal partial struct cpuStats {}
    internal partial struct cpuStatsAggregate {}
    internal partial struct dbgVar {}
    internal partial struct debugCallWrapArgs {}
    [GoValueClone("b")] internal partial struct debugLogBuf {}
    internal partial struct debugLogReader {}
    [GoValueClone("data", "buf")] internal partial struct debugLogWriter {}
    internal partial struct debugPtrmaskᴛ1 {}
    internal partial struct debugᴛ1 {}
    internal partial struct dlogPerM {}
    internal partial struct dloggerFake {}
    [GoValueClone("w")] internal partial struct dloggerImpl {}
    internal partial struct errorAddressString {}
    internal partial struct errorString {}
    internal partial struct exceptionpointers {}
    [GoValueClone("exceptioninformation")] internal partial struct exceptionrecord {}
    internal partial struct find_firstFree {}
    [GoValueClone("subbuckets")] internal partial struct findfuncbucket {}
    internal partial struct fixalloc {}
    internal partial struct functab {}
    internal partial struct gList {}
    internal partial struct gQueue {}
    internal partial struct gTraceState {}
    internal partial struct gcBgMarkWorkerNode {}
    internal partial struct gcBits {}
    [GoValueClone("bits")] internal partial struct gcBitsArena {}
    internal partial struct gcBitsArenasᴛ1 {}
    internal partial struct gcBitsHeader {}
    internal partial struct gcCPULimiterState {}
    internal partial struct gcCPULimiterState_bucket {}
    [GoValueClone("lastConsMark")] internal partial struct gcControllerState {}
    internal partial struct gcDebugMarkDoneᴛ1 {}
    internal partial struct gcDrainFlags {}
    internal partial struct gcMarkWorkerMode {}
    internal partial struct gcMode {}
    internal partial struct gcStatsAggregate {}
    internal partial struct gcTrigger {}
    internal partial struct gcTriggerKind {}
    internal partial struct gcWork {}
    internal partial struct gclink {}
    internal partial struct gclinkptr {}
    internal partial struct globalAllocᴛ1 {}
    [GoValueClone("seed", "state")] internal partial struct globalRandᴛ1 {}
    internal partial struct godebugInc {}
    internal partial struct goroutineProfileState {}
    internal partial struct goroutineProfileStateHolder {}
    internal partial struct goroutineProfileᴛ1 {}
    internal partial struct gsignalStack {}
    internal partial struct headTailIndex {}
    [GoValueClone("spans", "pageInUse", "pageMarks", "pageSpecials")] internal partial struct heapArena {}
    internal partial struct heapStatsAggregate {}
    [GoValueClone("smallAllocCount", "smallFreeCount")] internal partial struct heapStatsDelta {}
    internal partial struct initTask {}
    internal partial struct inlineFrame {}
    internal partial struct inlineUnwinder {}
    internal partial struct inlinedCall {}
    [GoValueClone("entries")] internal partial struct itabTableType {}
    internal partial struct lfstack {}
    internal partial struct limiterEvent {}
    internal partial struct limiterEventStamp {}
    internal partial struct limiterEventType {}
    internal partial struct linearAlloc {}
    internal partial struct linknameIter {}
    internal partial struct liveUserArenaChunk {}
    internal partial struct lockRank {}
    internal partial struct lockRankStruct {}
    internal partial struct lockTimer {}
    internal partial struct m128a {}
    internal partial struct mLockProfile {}
    internal partial struct mOS {}
    internal partial struct mProfCycleHolder {}
    internal partial struct mSpanList {}
    internal partial struct mSpanState {}
    internal partial struct mSpanStateBox {}
    [GoValueClone("buf")] internal partial struct mTraceState {}
    internal partial struct mWaitList {}
    internal partial struct markBits {}
    [GoValueClone("alloc", "stackcache")] internal partial struct mcache {}
    [GoValueClone("partial", "full")] internal partial struct mcentral {}
    [GoValueClone("future")] internal partial struct memRecord {}
    internal partial struct memRecordCycle {}
    internal partial struct memoryBasicInformation {}
    [GoValueClone("deps")] internal partial struct metricData {}
    internal partial struct metricFloat64Histogram {}
    internal partial struct metricKind {}
    internal partial struct metricName {}
    internal partial struct metricSample {}
    internal partial struct metricValue {}
    [GoValueClone("pages", "arenas", "central")] internal partial struct mheap {}
    [GoValueClone("mcentral", "pad")] internal partial struct mheap_central {}
    internal partial struct mheap_curArena {}
    internal partial struct mheap_userArena {}
    internal partial struct mlink {}
    internal partial struct moduledata {}
    internal partial struct modulehash {}
    [GoLocalName("_DEVICE_NOTIFY_SUBSCRIBE_PARAMETERS")] internal partial struct monitorSuspendResume__DEVICE_NOTIFY_SUBSCRIBE_PARAMETERS {}
    internal partial struct mspan {}
    [GoValueClone("heapStats", "pause_ns", "pause_end")] internal partial struct mstats {}
    internal partial struct neverCallThisFunction {}
    internal partial struct newmHandoffᴛ1 {}
    internal partial struct notInHeap {}
    internal partial struct notInHeapSlice {}
    internal partial struct note {}
    internal partial struct notifyList {}
    internal partial struct offAddr {}
    [GoValueClone("anon0")] internal partial struct overlapped {}
    internal partial struct overlappedEntry {}
    internal partial struct pMask {}
    internal partial struct pTraceState {}
    [GoValueClone("summary", "chunks")] internal partial struct pageAlloc {}
    internal partial struct pageAlloc_scav {}
    internal partial struct pageBits {}
    internal partial struct pageCache {}
    internal partial struct pallocBits {}
    [GoValueClone("scavenged")] internal partial struct pallocData {}
    internal partial struct pallocSum {}
    internal partial struct pcHeader {}
    [GoValueClone("entries")] internal partial struct pcvalueCache {}
    internal partial struct pcvalueCacheEnt {}
    internal partial struct persistentAlloc {}
    internal partial struct piController {}
    internal partial struct pinState {}
    [GoValueClone("refStore")] internal partial struct pinner {}
    internal partial struct pinnerBits {}
    internal partial struct plainError {}
    internal partial struct pollCache {}
    internal partial struct pollDesc {}
    internal partial struct pollInfo {}
    internal partial struct pollOperation {}
    internal partial struct profAtomic {}
    internal partial struct profBuf {}
    internal partial struct profIndex {}
    internal partial struct profᴛ1 {}
    internal partial struct ptabEntry {}
    internal partial struct randomEnum {}
    internal partial struct randomOrder {}
    internal partial struct reflectMethodValue {}
    internal partial struct reflectOffsᴛ1 {}
    internal partial struct runtimeSelect {}
    internal partial struct rwmutex {}
    internal partial struct scase {}
    internal partial struct scavChunkData {}
    internal partial struct scavChunkFlags {}
    internal partial struct scavengeIndex {}
    internal partial struct scavengerState {}
    internal partial struct selectDir {}
    internal partial struct semTable {}
    [GoValueClone("pad")] internal partial struct semTableᴛ1 {}
    internal partial struct semaProfileFlags {}
    internal partial struct semaRoot {}
    internal partial struct sigset {}
    [GoValueClone("mask", "wanted", "ignored", "recv")] internal partial struct sigᴛ1 {}
    internal partial struct sliceInterfacePtr {}
    internal partial struct spanAllocType {}
    internal partial struct spanClass {}
    internal partial struct spanSet {}
    [GoValueClone("spans")] internal partial struct spanSetBlock {}
    internal partial struct spanSetBlockAlloc {}
    internal partial struct spanSetSpinePointer {}
    internal partial struct special {}
    internal partial struct specialCleanup {}
    internal partial struct specialPinCounter {}
    internal partial struct specialReachable {}
    internal partial struct specialWeakHandle {}
    internal partial struct specialfinalizer {}
    internal partial struct specialprofile {}
    internal partial struct specialsIter {}
    [GoValueClone("free")] internal partial struct stackLargeᴛ1 {}
    internal partial struct stackObject {}
    [GoValueClone("obj")] internal partial struct stackObjectBuf {}
    internal partial struct stackObjectBufHdr {}
    internal partial struct stackObjectRecord {}
    internal partial struct stackScanState {}
    [GoValueClone("obj")] internal partial struct stackWorkBuf {}
    internal partial struct stackWorkBufHdr {}
    internal partial struct stackfreelist {}
    [GoValueClone("bytedata")] internal partial struct stackmap {}
    internal partial struct stackpoolItem {}
    internal partial struct stackpoolᴛ1 {}
    [GoValueClone("ensured")] internal partial struct statAggregate {}
    internal partial struct statDep {}
    internal partial struct statDepSet {}
    internal partial struct stdFunction {}
    internal partial struct stkframe {}
    internal partial struct stringInterfacePtr {}
    internal partial struct stringStruct {}
    internal partial struct stringStructDWARF {}
    internal partial struct stwReason {}
    internal partial struct suspendGState {}
    internal partial struct sweepClass {}
    internal partial struct sweepLocked {}
    internal partial struct sweepLocker {}
    internal partial struct sweepdata {}
    internal partial struct synctestGroup {}
    internal partial struct sysMemStat {}
    internal partial struct sysStatsAggregate {}
    internal partial struct sysmontick {}
    [GoValueClone("anon0")] internal partial struct systeminfo {}
    internal partial struct taggedPointer {}
    internal partial struct textsect {}
    internal partial struct throwType {}
    internal partial struct ticksType {}
    [GoValueClone("counts")] internal partial struct timeHistogram {}
    internal partial struct timeTimer {}
    internal partial struct timer {}
    internal partial struct timerWhen {}
    internal partial struct timers {}
    internal partial struct tmpBuf {}
    [GoLocalName("untracedG")] internal partial struct traceAdvance_untracedG {}
    internal partial struct traceAdvancerState {}
    internal partial struct traceArg {}
    internal partial struct traceBlockReason {}
    [GoValueClone("arr")] internal partial struct traceBuf {}
    internal partial struct traceBufHeader {}
    internal partial struct traceBufQueue {}
    internal partial struct traceEv {}
    internal partial struct traceEventWriter {}
    internal partial struct traceExperiment {}
    internal partial struct traceFrame {}
    internal partial struct traceGoStatus {}
    internal partial struct traceGoStopReason {}
    internal partial struct traceLocker {}
    internal partial struct traceMap {}
    [GoValueClone("children")] internal partial struct traceMapNode {}
    internal partial struct traceProcStatus {}
    internal partial struct traceRegionAlloc {}
    [GoValueClone("data")] internal partial struct traceRegionAllocBlock {}
    internal partial struct traceRegionAllocBlockHeader {}
    [GoValueClone("statusTraced", "seq")] internal partial struct traceSchedResourceState {}
    internal partial struct traceStackTable {}
    internal partial struct traceStringTable {}
    internal partial struct traceTime {}
    internal partial struct traceTypeTable {}
    internal partial struct traceWriter {}
    internal partial struct tracestat {}
    [GoValueClone("t")] internal partial struct typeCacheBucket {}
    internal partial struct typePointers {}
    internal partial struct uint16InterfacePtr {}
    internal partial struct uint32InterfacePtr {}
    internal partial struct uint64InterfacePtr {}
    internal partial struct unwindFlags {}
    internal partial struct unwinder {}
    internal partial struct userArena {}
    internal partial struct userArenaStateᴛ1 {}
    internal partial struct waitq {}
    internal partial struct wakeableSleep {}
    [GoValueClone("buf")] internal partial struct wbBuf {}
    internal partial struct winCallback {}
    internal partial struct winCallbackKey {}
    internal partial struct winlibcall {}
    internal partial struct workType {}
    internal partial struct workType_assistQueue {}
    internal partial struct workType_strongFromWeak {}
    internal partial struct workType_sweepWaiters {}
    internal partial struct workType_wbufSpans {}
    [GoValueClone("obj")] internal partial struct workbuf {}
    internal partial struct workbufhdr {}
    internal partial struct worldStop {}
    [GoValueClone("pad")] internal partial struct writeBarrierᴛ1 {}
    internal partial struct ΔcgoCallers {}
    internal partial struct ΔfuncInfo {}
    internal partial struct Δhchan {}
    internal partial struct Δhex {}
    internal partial struct Δrtype {}
    internal partial struct Δscavengeᴛ1 {}
    internal partial struct Δsliceᴛ {}
    internal partial struct ΔsrcFunc {}
    [GoValueClone("full", "doneSema", "stackTab", "stringTab", "typeTab", "cpuLogRead", "cpuLogWrite", "cpuBuf", "markWorkerLabels", "goStopReasons", "goBlockReasons")] internal partial struct Δtraceᴛ1 {}
    internal partial struct ΔwriteUserArenaHeapBits {}
    public partial interface ΔError {}
    public partial struct BlockProfileRecord {}
    public partial struct Frame {}
    [GoValueClone("frameStore")] public partial struct Frames {}
    public partial struct Func {}
    [GoValueClone("Stack0")] public partial struct MemProfileRecord {}
    [GoValueClone("PauseNs", "PauseEnd", "BySize")] public partial struct MemStats {}
    public partial struct MemStats_BySize {}
    public partial struct PanicNilError {}
    public partial struct Pinner {}
    [GoValueClone("Stack0")] public partial struct StackRecord {}
    public partial struct TypeAssertionError {}
    public partial struct profBufReadMode {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸabi() => builtin.initPackage(typeof(@internal.abi_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸbytealg() => builtin.initPackage(typeof(@internal.bytealg_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸgodebugs() => builtin.initPackage(typeof(@internal.godebugs_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸruntimeꓸmaps() => builtin.initPackage(typeof(@internal.runtime.maps_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸruntimeꓸsys() => builtin.initPackage(typeof(@internal.runtime.sys_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸstringslite() => builtin.initPackage(typeof(@internal.stringslite_package));
    // </ImportInitializers>
}
