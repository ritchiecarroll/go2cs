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
global using abiꓸInterfaceType = go.@internal.abi_package.ΔInterfaceType;
global using abiꓸKind = go.@internal.abi_package.ΔKind;
global using abiꓸMapType = go.@internal.abi_package.ΔMapType;
global using abiꓸName = go.@internal.abi_package.ΔName;
global using mapsꓸMap = go.@internal.runtime.maps_package.ΔMap;
using abi = go.@internal.abi_package;
using tracev2 = go.@internal.trace.tracev2_package;
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
[assembly: GoDynamicTypeLift("7374727563747b63676f636865636b20696e7433323b20636c6f626265726672656520696e7433323b20636f6e7461696e65726d617870726f637320696e7433323b206465636f726174656d617070696e677320696e7433323b2064697361626c6574687020696e7433323b20646f6e74667265657a65746865776f726c6420696e7433323b206566656e636520696e7433323b206763636865636b6d61726b20696e7433323b2067637061636572747261636520696e7433323b206763736872696e6b737461636b6f666620696e7433323b20676373746f70746865776f726c6420696e7433323b206763747261636520696e7433323b20696e76616c696470747220696e7433323b206d616476646f6e746e65656420696e7433323b2073636176747261636520696e7433323b20736368656464657461696c20696e7433323b207363686564747261636520696e7433323b2074726163656261636b616e636573746f727320696e7433323b207570646174656d617870726f637320696e7433323b206173796e63707265656d70746f666620696e7433323b20686172646465636f6d6d697420696e7433323b206164617074697665737461636b737461727420696e7433323b2074726163656670756e77696e646f666620696e7433323b207472616365616476616e6365706572696f6420696e7433323b207472616365436865636b537461636b4f776e65727368697020696e7433323b2070726f66737461636b646570746820696e7433323b2064617461696e646570656e64656e7474696d696e6720696e7433323b206d616c6c6f6320626f6f6c3b20696e6974747261636520696e7433323b207362726b20696e7433323b20636865636b66696e616c697a65727320696e7433323b207472616365616c6c6f636672656520696e7465726e616c2f72756e74696d652f61746f6d69632e496e7433323b2070616e69636e696c20696e7465726e616c2f72756e74696d652f61746f6d69632e496e7433323b206173796e6374696d65726368616e20696e7465726e616c2f72756e74696d652f61746f6d69632e496e7433323b2074726163656261636b6c6162656c7320696e7465726e616c2f72756e74696d652f61746f6d69632e496e7433327d", "debugᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b656e61626c656420626f6f6c3b20706164205b335d627974653b20616c69676e6d652075696e7436347d", "writeBarrierᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b66696c6c2075696e7436343b2063617061636974792075696e7436347d", "gcCPULimiterState_bucket")]
[assembly: GoDynamicTypeLift("7374727563747b676350657263656e74476f616c20696e7465726e616c2f72756e74696d652f61746f6d69632e55696e7436343b206d656d6f72794c696d6974476f616c20696e7465726e616c2f72756e74696d652f61746f6d69632e55696e7436343b2061737369737454696d6520696e7465726e616c2f72756e74696d652f61746f6d69632e496e7436343b206261636b67726f756e6454696d6520696e7465726e616c2f72756e74696d652f61746f6d69632e496e7436347d", "Δscavengeᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b68656164202a72756e74696d652e7370616e53504d433b207461696c20696e7465726e616c2f72756e74696d652f61746f6d69632e556e73616665506f696e7465727d", "spanQueue_chain")]
[assembly: GoDynamicTypeLift("7374727563747b696e6465782072756e74696d652e73636176656e6765496e6465783b2072656c6561736564426720696e7465726e616c2f72756e74696d652f61746f6d69632e55696e747074723b2072656c6561736564456167657220696e7465726e616c2f72756e74696d652f61746f6d69632e55696e747074727d", "pageAlloc_scav")]
[assembly: GoDynamicTypeLift("7374727563747b6974656d2072756e74696d652e737461636b706f6f6c4974656d3b205f205b34305d627974657d", "stackpoolᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6c656e20696e743b20627566205b3132385d2a72756e74696d652e6d7370616e7d", "p_mspancache")]
[assembly: GoDynamicTypeLift("7374727563747b6c6f636b2072756e74696d652e6d757465783b20616c6c6f632072756e74696d652e666978616c6c6f637d", "xRegAllocᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6c6f636b2072756e74696d652e6d757465783b2064617461202a627974657d", "debugPtrmaskᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6c6f636b2072756e74696d652e6d757465783b2066726565202a72756e74696d652e6763426974734172656e613b206e657874202a72756e74696d652e6763426974734172656e613b2063757272656e74202a72756e74696d652e6763426974734172656e613b2070726576696f7573202a72756e74696d652e6763426974734172656e617d", "gcBitsArenasᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6c6f636b2072756e74696d652e6d757465783b2066726565205b33355d72756e74696d652e6d5370616e4c6973747d", "stackLargeᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6c6f636b2072756e74696d652e6d757465783b20667265652072756e74696d652e6d5370616e4c6973743b20627573792072756e74696d652e6d5370616e4c6973747d", "workType_wbufSpans")]
[assembly: GoDynamicTypeLift("7374727563747b6c6f636b2072756e74696d652e6d757465783b206c6973742072756e74696d652e674c6973747d", "workType_sweepWaiters")]
[assembly: GoDynamicTypeLift("7374727563747b6c6f636b2072756e74696d652e6d757465783b206c6973742072756e74696d652e6c697374486561644d616e75616c7d", "workType_spanSPMCs")]
[assembly: GoDynamicTypeLift("7374727563747b6c6f636b2072756e74696d652e6d757465783b206e65776d2072756e74696d652e6d75696e747074723b2077616974696e6720626f6f6c3b2077616b652072756e74696d652e6e6f74653b206861766554656d706c6174655468726561642075696e7433327d", "newmHandoffᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6c6f636b2072756e74696d652e6d757465783b206e65787420696e7433323b206d206d61705b696e7433325d756e736166652e506f696e7465723b206d696e76206d61705b756e736166652e506f696e7465725d696e7433327d", "reflectOffsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6c6f636b2072756e74696d652e6d757465783b20712072756e74696d652e6751756575657d", "workType_assistQueue")]
[assembly: GoDynamicTypeLift("7374727563747b6c6f636b2072756e74696d652e6d757465783b2072656164696e67202a72756e74696d652e74726163654275663b20656d707479202a72756e74696d652e74726163654275663b2066756c6c205b325d72756e74696d652e747261636542756651756575653b20776f726b417661696c61626c6520696e7465726e616c2f72756e74696d652f61746f6d69632e426f6f6c3b2072656164657247656e20696e7465726e616c2f72756e74696d652f61746f6d69632e55696e747074723b20666c757368656447656e20696e7465726e616c2f72756e74696d652f61746f6d69632e55696e747074723b206865616465725772697474656e20626f6f6c3b20656e644f6647656e65726174696f6e5772697474656e20626f6f6c3b20646f6e6553656d61205b325d75696e7433323b20737461636b546162205b325d72756e74696d652e7472616365537461636b5461626c653b20737472696e67546162205b325d72756e74696d652e7472616365537472696e675461626c653b2074797065546162205b325d72756e74696d652e7472616365547970655461626c653b206370754c6f6752656164205b325d2a72756e74696d652e70726f664275663b207369676e616c4c6f636b20696e7465726e616c2f72756e74696d652f61746f6d69632e55696e7433323b206370754c6f675772697465205b325d696e7465726e616c2f72756e74696d652f61746f6d69632e506f696e7465725b72756e74696d652e70726f664275665d3b20637075536c656570202a72756e74696d652e77616b6561626c65536c6565703b206370754c6f67446f6e65203c2d6368616e207374727563747b7d3b20637075427566205b325d2a72756e74696d652e74726163654275663b2072656164657220696e7465726e616c2f72756e74696d652f61746f6d69632e506f696e7465725b72756e74696d652e675d3b206d61726b576f726b65724c6162656c73205b325d5b345d72756e74696d652e74726163654172673b20676f53746f70526561736f6e73205b325d5b335d72756e74696d652e74726163654172673b20676f426c6f636b526561736f6e73205b325d5b31375d72756e74696d652e74726163654172673b20656e61626c656420626f6f6c3b20656e61626c656457697468416c6c6f634672656520626f6f6c3b2067656e20696e7465726e616c2f72756e74696d652f61746f6d69632e55696e747074723b206c6173744e6f6e5a65726f47656e2075696e747074723b2073687574646f776e20696e7465726e616c2f72756e74696d652f61746f6d69632e426f6f6c3b2065786974696e6753797363616c6c20696e7465726e616c2f72756e74696d652f61746f6d69632e496e7433323b2073657147432075696e7436343b206d696e5061676548656170416464722075696e7436343b2064656275674d616c6c6f6320626f6f6c7d", "Δtraceᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6c6f636b2072756e74696d652e6d757465783b207265757365205b5d72756e74696d652e6c697665557365724172656e614368756e6b3b206661756c74205b5d72756e74696d652e6c697665557365724172656e614368756e6b7d", "userArenaStateᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6c6f636b2072756e74696d652e6d757465783b2073656564205b33325d627974653b20737461746520696e7465726e616c2f6368616368613872616e642e53746174653b20696e697420626f6f6c7d", "globalRandᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6c6f636b2072756e74696d652e6d757465783b20737461636b2072756e74696d652e674c6973743b206e6f537461636b2072756e74696d652e674c6973747d", "schedt_gFree")]
[assembly: GoDynamicTypeLift("7374727563747b6d63656e7472616c2072756e74696d652e6d63656e7472616c3b20706164205b32345d627974657d", "mheap_central")]
[assembly: GoDynamicTypeLift("7374727563747b6e6f74652072756e74696d652e6e6f74653b206d61736b205b315d75696e7433323b2077616e746564205b315d75696e7433323b2069676e6f726564205b315d75696e7433323b2072656376205b315d75696e7433323b20737461746520696e7465726e616c2f72756e74696d652f61746f6d69632e55696e7433323b2064656c69766572696e6720696e7465726e616c2f72756e74696d652f61746f6d69632e55696e7433323b20696e75736520626f6f6c7d", "sigᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b70656e64696e6720696e7465726e616c2f72756e74696d652f61746f6d69632e426f6f6c3b20656e61626c656420626f6f6c3b20646f6e6520626f6f6c3b20636f756e7420696e747d", "workType_goroutineLeak")]
[assembly: GoDynamicTypeLift("7374727563747b726f6f742072756e74696d652e73656d61526f6f743b20706164205b34305d627974657d", "semTableᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b72756e74696d652e6d757465783b2072756e74696d652e70657273697374656e74416c6c6f637d", "globalAllocᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b73656d612075696e7433323b2061637469766520626f6f6c3b206f666673657420696e7465726e616c2f72756e74696d652f61746f6d69632e496e7436343b207265636f726473205b5d696e7465726e616c2f70726f66696c657265636f72642e537461636b5265636f72643b206c6162656c73205b5d756e736166652e506f696e7465727d", "goroutineProfileᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7369676e616c4c6f636b20696e7465726e616c2f72756e74696d652f61746f6d69632e55696e7433323b20687a20696e7465726e616c2f72756e74696d652f61746f6d69632e496e7433327d", "profᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7370696e41667465725261676765644261727269657220696e7465726e616c2f72756e74696d652f61746f6d69632e426f6f6c3b20726573746172746564447565546f323739393320626f6f6c7d", "gcDebugMarkDoneᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7573657220626f6f6c3b2072756e6e61626c652072756e74696d652e6751756575657d", "schedt_disable")]
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
[assembly: GoImplicitConv<gclinkptr, Δhex>(Inverted = true, ValueType = "uintptr")]
[assembly: GoImplicitConv<lfstack, Δhex>(Inverted = true, ValueType = "uint64")]
[assembly: GoImplicitConv<limiterEventStamp, limiterEventType>(Inverted = true, ValueType = "uint64")]
[assembly: GoImplicitConv<spanClass, traceArg>(Inverted = true, ValueType = "uint8")]
[assembly: GoImplicitConv<sweepClass, spanClass>(Inverted = true, ValueType = "uint32")]
[assembly: GoImplicitConv<taggedPointer, Δhex>(Inverted = true, ValueType = "uint64")]
[assembly: GoImplicitConv<traceArg, tracev2.GoStatus>(Inverted = false, ValueType = "uint8")]
[assembly: GoImplicitConv<traceArg, tracev2.ProcStatus>(Inverted = false, ValueType = "uint8")]
[assembly: GoImplicitConv<Δhex, abi.PCLnTabMagic>(Inverted = false, ValueType = "uint32")]
// </ImplicitConversions>

// This package's string literals are hoisted to `static readonly` fields (the standard
// library's form, or -hoist-literals), so no converted file passes a bare literal for
// go2cs-gen's literal interceptor to box. The stamp below tells the generator to skip
// the assembly.
[assembly: go.GoHoistedLiterals]

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: go.GoFieldAccess(typeof(Frames))]
[assembly: go.GoFieldAccess(typeof(_type))]
[assembly: go.GoFieldAccess(typeof(arenaHint))]
[assembly: go.GoFieldAccess(typeof(blockRecord))]
[assembly: go.GoFieldAccess(typeof(bucket))]
[assembly: go.GoFieldAccess(typeof(chantype))]
[assembly: go.GoFieldAccess(typeof(checkmarksMap))]
[assembly: go.GoFieldAccess(typeof(coro))]
[assembly: go.GoFieldAccess(typeof(dbgVar))]
[assembly: go.GoFieldAccess(typeof(debugCallWrapArgs))]
[assembly: go.GoFieldAccess(typeof(debugLogBuf))]
[assembly: go.GoFieldAccess(typeof(findfuncbucket))]
[assembly: go.GoFieldAccess(typeof(functype))]
[assembly: go.GoFieldAccess(typeof(gcBgMarkWorkerNode))]
[assembly: go.GoFieldAccess(typeof(gcBitsArena))]
[assembly: go.GoFieldAccess(typeof(gcWork))]
[assembly: go.GoFieldAccess(typeof(gclink))]
[assembly: go.GoFieldAccess(typeof(getOrAddWeakHandle_weakHandleBox))]
[assembly: go.GoFieldAccess(typeof(go.@internal.abi_package.Imethod))]
[assembly: go.GoFieldAccess(typeof(go.@internal.abi_package.InterfaceSwitchCache))]
[assembly: go.GoFieldAccess(typeof(go.@internal.abi_package.Method))]
[assembly: go.GoFieldAccess(typeof(go.@internal.abi_package.StructField))]
[assembly: go.GoFieldAccess(typeof(go.@internal.abi_package.Type))]
[assembly: go.GoFieldAccess(typeof(go.@internal.abi_package.TypeAssertCache))]
[assembly: go.GoFieldAccess(typeof(go.@internal.abi_package.UncommonType))]
[assembly: go.GoFieldAccess(typeof(go.@internal.abi_package.ΔInterfaceType))]
[assembly: go.GoFieldAccess(typeof(go.@internal.abi_package.ΔStructType))]
[assembly: go.GoFieldAccess(typeof(heapArena))]
[assembly: go.GoFieldAccess(typeof(immortalWeakHandle))]
[assembly: go.GoFieldAccess(typeof(inlinedCall))]
[assembly: go.GoFieldAccess(typeof(interfacetype))]
[assembly: go.GoFieldAccess(typeof(itab))]
[assembly: go.GoFieldAccess(typeof(itabTableType))]
[assembly: go.GoFieldAccess(typeof(keventt))]
[assembly: go.GoFieldAccess(typeof(libcCallInfo))]
[assembly: go.GoFieldAccess(typeof(listNode))]
[assembly: go.GoFieldAccess(typeof(listNodeManual))]
[assembly: go.GoFieldAccess(typeof(mSpanList))]
[assembly: go.GoFieldAccess(typeof(mcache))]
[assembly: go.GoFieldAccess(typeof(memRecord))]
[assembly: go.GoFieldAccess(typeof(memRecordCycle))]
[assembly: go.GoFieldAccess(typeof(metricFloat64Histogram))]
[assembly: go.GoFieldAccess(typeof(metricValue))]
[assembly: go.GoFieldAccess(typeof(mheap))]
[assembly: go.GoFieldAccess(typeof(mlink))]
[assembly: go.GoFieldAccess(typeof(moduledata))]
[assembly: go.GoFieldAccess(typeof(mspan))]
[assembly: go.GoFieldAccess(typeof(pcHeader))]
[assembly: go.GoFieldAccess(typeof(pcvalueCache))]
[assembly: go.GoFieldAccess(typeof(pcvalueCacheEnt))]
[assembly: go.GoFieldAccess(typeof(pollDesc))]
[assembly: go.GoFieldAccess(typeof(profBuf))]
[assembly: go.GoFieldAccess(typeof(reflectMethodValue))]
[assembly: go.GoFieldAccess(typeof(regs64))]
[assembly: go.GoFieldAccess(typeof(scase))]
[assembly: go.GoFieldAccess(typeof(sigTabT))]
[assembly: go.GoFieldAccess(typeof(siginfo))]
[assembly: go.GoFieldAccess(typeof(spanInlineMarkBits))]
[assembly: go.GoFieldAccess(typeof(spanSPMC))]
[assembly: go.GoFieldAccess(typeof(special))]
[assembly: go.GoFieldAccess(typeof(specialBubble))]
[assembly: go.GoFieldAccess(typeof(specialCheckFinalizer))]
[assembly: go.GoFieldAccess(typeof(specialCleanup))]
[assembly: go.GoFieldAccess(typeof(specialPinCounter))]
[assembly: go.GoFieldAccess(typeof(specialReachable))]
[assembly: go.GoFieldAccess(typeof(specialSecret))]
[assembly: go.GoFieldAccess(typeof(specialTinyBlock))]
[assembly: go.GoFieldAccess(typeof(specialWeakHandle))]
[assembly: go.GoFieldAccess(typeof(specialfinalizer))]
[assembly: go.GoFieldAccess(typeof(specialprofile))]
[assembly: go.GoFieldAccess(typeof(stackObject))]
[assembly: go.GoFieldAccess(typeof(stackObjectBuf))]
[assembly: go.GoFieldAccess(typeof(stackObjectRecord))]
[assembly: go.GoFieldAccess(typeof(stackWorkBuf))]
[assembly: go.GoFieldAccess(typeof(stackmap))]
[assembly: go.GoFieldAccess(typeof(statAggregate))]
[assembly: go.GoFieldAccess(typeof(stkframe))]
[assembly: go.GoFieldAccess(typeof(stringStruct))]
[assembly: go.GoFieldAccess(typeof(structtype))]
[assembly: go.GoFieldAccess(typeof(synctestBubble))]
[assembly: go.GoFieldAccess(typeof(sysmontick))]
[assembly: go.GoFieldAccess(typeof(timer))]
[assembly: go.GoFieldAccess(typeof(timerWhen))]
[assembly: go.GoFieldAccess(typeof(timers))]
[assembly: go.GoFieldAccess(typeof(traceBuf))]
[assembly: go.GoFieldAccess(typeof(traceMapNode))]
[assembly: go.GoFieldAccess(typeof(traceRegionAllocBlock))]
[assembly: go.GoFieldAccess(typeof(typeCacheBucket))]
[assembly: go.GoFieldAccess(typeof(uncommontype))]
[assembly: go.GoFieldAccess(typeof(wakeableSleep))]
[assembly: go.GoFieldAccess(typeof(workbuf))]
[assembly: go.GoFieldAccess(typeof(xRegState))]
[assembly: go.GoFieldAccess(typeof(Δhchan))]
[assembly: go.GoFieldAccess(typeof(Δsliceᴛ))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("runtime/alg.go", "alg.cs", "ABUugsqUpoKmgqaCpoKokoKCAAg2ABQElAABHgAOBIKCAAIQwoKUpKS4goKUpKS4goKmgoKmgoKCgpSCypSClAADHAAKAoKCgpSUlIKUAAMyABcClJSkpKaUpKSkpKSCgpSkgoKUpIKCgpSUyMqSpoKkgqSCpIKkgqSCpIKkgqSCpIKkgqSCgoKkgoKCpKKClIKClLiUpKKClIKCgpSUlAACHAANAsaigqaixqIAFSqUqIKUgoKUgriClIKCupKCgpSmgoKClA==")]
[assembly: go.GoPositionMap("runtime/arena.go", "arena.cs", "AF3MAQAIAgAFEgAIAoKClIKCgoKCgqyyrLIAEYgBgoKUgpSCgqaCpqrqACKQAsKCgoKCgpSU6rSCgsqUgoKClKaClIKCgoKClIKUgoKmgoIAHjjSgoKUlJSClKaClNyCgpSClJSClIKCpoKCppSCgpSCpoKClJSCgpSClAAJFJSClNzSgoKUggADEPKClIKUgoKCAAcQAAcQgriWkoIAChaiyqamgpSqwpSCgsqCgqaCgoLugoKokoKUgoKClKrCyqaCgoKUgriCpoKCgpSCuO6UgoKCgoKClIKUvKKCgpSU/LKCAAgSgriCgpSClJaSgpSClNyCppSmlKamgoKmgIKCgqaClLaCpoCCtoKUAAcQgqYAAhoACgIABhDygpSCAA4eysrc3IKCgoKCpqaCppSCgrqSgoKUAAYUAAkCgpSCyoKUgpSClILcpoKCgoKUgoK4gqaCgpQABhgACwKCpoKCgoKmgriUgoKU3IKCgpSUgpSClJQABxCCpsqCgoKCAAgSpoK4gpSCgqaCgqamuAAKFoKUpri4goKClA==", "764-766:1;912-917:1")]
[assembly: go.GoPositionMap("runtime/asan0.go", "asan0.cs", "ABIokKKAooCigKKAooCigKKA")]
[assembly: go.GoPositionMap("runtime/atomic_pointer.go", "atomic_pointer.cs", "AA1AABUCgoKCrLKClIKUAAIQ0gACENKClIKUAAIQ6KKClIKUqKiigpSClIKoqKKClIKU")]
[assembly: go.GoPositionMap("runtime/cgo.go", "cgo.cs", "AEWaAfAABRDgAAoUlIKCgpQ=")]
[assembly: go.GoPositionMap("runtime/cgocall.go", "cgocall.cs", "AGncAfKCggAJKgAQAoKmgqaCpoKmAA8g7pTulLiClLiUlLiCpqaCgoKCggANHIKClAACEuKUuKaCygAVLIKCgozSAAgSgoKCpoLcsoKCgqaCgoLulLgAChaCgoKCgoKmlIKmyoKUpqaCpqaUpoKCgqailIKCpoIABxCCgoKUlIK4ysqCgriClJKmgriCgoKmgqqigoKUgqaUgpSCgoLcoqSCpoKC7oKCggAIEpTqktiSACjUAfKUppS0goKClIKUgoL8tIKUgpS0goLKgpSCgpSClLSCgoKClIKUgpSCgsaCgpSCxoKCgpSClIKClMaCgoK4gpSCpgAEEPKCgoKClIKCgoCCpIKCpqaCptwAAhLigqaCpoKCuK7CrNKCpoKCgpQADgqikoKCppS0tLS0tLS0tLTIggAHEIKClKaClMqUgoK0tg==", "416-420:1", "", "", "453:21")]
[assembly: go.GoPositionMap("runtime/cgocallback.go", "cgocallback.cs", "AAcWog==")]
[assembly: go.GoPositionMap("runtime/cgocheck.go", "cgocheck.cs", "AA8yAAgCuJSClILKgoLKgsqC3IKmgoIAAxgACgIAAhYACgKClIKUgpQAAhYACgKClIKUgpSCgoIAAxQACQSClICCtgACEuKCgoKCgoKCgoKClJSClIKCgg==", "65-68:1")]
[assembly: go.GoPositionMap("runtime/cgroup_stubs.go", "cgroup_stubs.cs", "AAgSoqTugoKU")]
[assembly: go.GoPositionMap("runtime/chan.go", "chan.cs", "ADp+ktaCgqbmgqaClIKmgoKuwqaU2ILGgraCgoKAgqSUgpQAAhwADQKu5pSmrLIACR4ADgKCgpSCpoKmgqaCABImgqaCgqaUgoKmgKaAkraUgoKUgoKClIKCpoKCuIKCgoK4goKCgoKCgsqCgoKUyqaClIKCgoKClIKCgoKUlAACEgAJAoKClIKCyoKCgoKUpoKClIKCgoKClAACEAAMDIKUgoKCgoKCgpSUggACGgARDIKmpuiCgvaigpSCpoKCgqaCgoKmlKaCgoKUgoKUgpSCgoKClLiCgoKUgoKUgoKCgpSUpoKCggADEPSCuIKUrLKosoIABRIADQiCpoKClIKmgqaCuAAJFMrKlIKUgpS4goKmlIKCgpSCgpTKgMqAksiUgoKUgpSCgoKUgoKmgoK4goKCgriCgpSCgoKCgoLugoKClKaClIKUgoKClIKCgoIAAiIAEQKCgpSCgpSU7oKCgqaCpoKCgpSUgoKCgoKClKb8uNyCAAImABACAAImABACqJKokqaigpSCgpS4lKaigpSCgoLKlKiSqJKokqiSpqKCgoKCgoKUgoKmgoKCgpSCgoKUgoIAChaClLi43KaCgoKCrAANEO6CgoKUgqaClA==", "232-232:1;607-607:1")]
[assembly: go.GoPositionMap("runtime/checkptr.go", "checkptr.cs", "AA4StILugsqCvKKCyoKUyuaCgtyCgqaCgrgAAigAEgSAAAgSyICCyIKClIK4")]
[assembly: go.GoPositionMap("runtime/complex.go", "complex.cs", "AAcSooKClKaCyIKCgoKUgoKCpriClJSCtoKCgraCgoLI")]
[assembly: go.GoPositionMap("runtime/coro.go", "coro.cs", "ACNU0oKCgoKSgoKCuIKCgqaCgqzSgoKUgqKoooKCgq7CgoIABx4ADgKCgoKCAAcQgoKCgoKCAAkUlIKCuIKmpqbcgriCpriqogAKFoKClIKCgoIABxCC3ILcggAIEoKmpoK4goK4gqaCuA==", "47-60:1")]
[assembly: go.GoPositionMap("runtime/covercounter.go", "covercounter.cs", "AAsasoKCgoKUyg==")]
[assembly: go.GoPositionMap("runtime/covermeta.go", "covermeta.cs", "AA4cwoKClA==")]
[assembly: go.GoPositionMap("runtime/cpuflags_amd64.go", "cpuflags_amd64.cs", "ABMsAAcQgoKClII=")]
[assembly: go.GoPositionMap("runtime/cpuprof.go", "cpuprof.cs", "ADqIAQAIBIKUgqaCgoKCgqaCgoKCgoKCgoKUAAIUAAgElKaCgpTKpgACGgAQDpSmgoKCgpSmAAIQ5IKCgpSmgoK4gqaCgriCAAMcAAkCAAIaAAoCAAIoABECgoKCgoKUgoKCgpQ=", "", "", "177=FuncPCABIInternal/1/2/1,FuncPCABIInternal/2/2/2;187=FuncPCABIInternal/1/2/1,FuncPCABIInternal/2/2/2")]
[assembly: go.GoPositionMap("runtime/cputicks.go", "cputicks.cs", "AAgW")]
[assembly: go.GoPositionMap("runtime/create_file_unix.go", "create_file_unix.cs", "AAoYkg==")]
[assembly: go.GoPositionMap("runtime/debug.go", "debug.cs", "AA3qAQANCpSCgoKUpqbcgoKClAACENIACDqSAAIwABYEgoKmggADFgAIBIKCpoIABhrigoKUlA==")]
[assembly: go.GoPositionMap("runtime/debugcall.go", "debugcall.cs", "ABYyks7kgpSAyrqigoKCgqaUAAQeAAcQgIKCyIKUgpSmAAUYAAkCgoIABxC4yIKCuLiCgtyClIKC3Li4lIKmgriUgoKU3NyCgoKmlAAHFKKCgoKmpoKCgriCgtyCuJSCgpSCgoKUgoKCgpS4pJKClIKCgoKmgqI=", "49-95:1;123-161:1;164-188:2;222-259:1;269-274:1")]
[assembly: go.GoPositionMap("runtime/debuglog.go", "debuglog.cs", "ACx8ABIEqqIAHMIB5sSCgrimgrgAESqQprKClJSokKaSqJCmkqiQppKokKaSqJCmsoKCqJCmkqiQppKokKaSqJCmkqiQppKokKaygoKokKaygoKokNbCgoKUgpS0tqiQAARGkKaygoKCgoKmgoKClIKCpqiQprKCgqiQprKCgoKUAC5kkpTuzJKCgqiSgoKCgoKCqJKCgqiSgoKCqJKCgoKCgoK6koKClJSokoKCgoKUgoIACx6kgpSClIKClIKUgqiSqpKCgoKUppSCgoKUgoKUgqaCgriClIKCgoKCgoKmgpSm9oKCpoKClKaCgoKCgoKCpqaCgoKClJQABMwBkoI=")]
[assembly: go.GoPositionMap("runtime/debuglog_off.go", "debuglog_off.cs", "AAwagtqCpoI=")]
[assembly: go.GoPositionMap("runtime/defs_darwin_amd64.go", "defs_darwin_amd64.cs", "AJcBsgKCAAwckoI=")]
[assembly: go.GoPositionMap("runtime/dit.go", "dit.cs", "AAsakoKCgqiSgoKC")]
[assembly: go.GoPositionMap("runtime/env_posix.go", "env_posix.cs", "AAwSgoKClIKCpqqigoKCgpSUlKaCgpQAGDSSgpSCqJKClIKmgoKC")]
[assembly: go.GoPositionMap("runtime/error.go", "error.cs", "ACtUpIKCgpSCgpSCgoKUgpSmlAACEtKCgoKClILcpIIABxCkggACENIABxCkggARJAAMGgAKFqSCgoKUgoKmooKCuIKCgoKCgpSClLS2AAYeAAgClLS0tLS0tLS0tLS0tLS0tLS0xMqygpSUgoK0tLS0tLS0tLS0tLS0tLS0tMqSgoKClIKCgpSuwoK4goKUgoKUgoKClIKUgoI=")]
[assembly: go.GoPositionMap("runtime/extern.go", "extern.cs", "AK8C6gQADAKCgoKUggAJSAAIAoKClAAMILI=")]
[assembly: go.GoPositionMap("runtime/fastlog2.go", "fastlog2.cs", "AAcc8oKUpoKClII=")]
[assembly: go.GoPositionMap("runtime/fastlog2table.go", "fastlog2table.cs", "AAkS")]
[assembly: go.GoPositionMap("runtime/fds_unix.go", "fds_unix.cs", "AAsSgrimAAUQgoKCgqaCgqaAgoKCgoI=")]
[assembly: go.GoPositionMap("runtime/fipsbypass.go", "fipsbypass.cs", "AAkUkqiSqJI=")]
[assembly: go.GoPositionMap("runtime/float.go", "float.cs", "AA0elqSokqiSAAIS4oKqooKokqqiAAIaAAoCgpSCgoKUlIIAAhoACgIAAhoADQKClIKklKaCpoKUgoI=")]
[assembly: go.GoPositionMap("runtime/hash64.go", "hash64.cs", "ABAqgpKClKSCgrSCtIK0grSCtIKCgoKCgoKClJSCgpSCtqaCgqaCgqaCgqaCpoI=")]
[assembly: go.GoPositionMap("runtime/heapdump.go", "heapdump.cs", "ADmeAYKClIKCgqaCgoKUgriipoKCABIqkoKCgoKClIKCpoKClLqSgqaCgoK4gqiygsqCgpSClIKUgtyClKaCgoKCgIKUgoKCgoKkqJKCgoKmgoKCpqKCgoKCggAMHLKCgoL6sqaCgoKClLiUlIKClLiCgoKCgoKCgoKClKaCpoKCypSCgpSUgoKClKaUpoKCgoKCgpSUpqKigoKClIKCpoKCgoKCgoKCgoKCgoKWkoKCgoKCgriCgoKCgoKCgpSUlJSCgoKCgoKCguiCpoKC2oKUpILcyqKCgoKCgqaUuIKCgqaCgoKmgpSCgpSCgtwACQ6UlIKClIKCgoKmgoK4goKClMqCgoKClJSCkoKClIKClIKClIK4goKCgqaigoKCgqaCpoKCgoKCupLKgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKUpoKCgoKCgoKCgoKCgoKCgoKUgoKCpoKCgoKCgoKClIKClIKCpoKmlJSCgoKUgoKUgoKCgsqkgqaCgqaCgoKCgoKCgoKCgqaClIKmpqaCgoKmqLKC5pSCgoKUgoKClKaCgoKCgoCCpIKU", "412-433:1")]
[assembly: go.GoPositionMap("runtime/hexdump.go", "hexdump.cs", "ADOoAYKCuIKCgoKClIK4goK4poKC+IKUgoKCpoKUgoK4lIKClLjKlIKCpgAIBoKUgoKGooKCgpSCgoKmlJSCpoKUgoKCuIKUgoKmgpSCgoKUpoKCpqaCgoKClJSCuIKC2qKCgoKClJS4goKClIKClII=", "175-179:1")]
[assembly: go.GoPositionMap("runtime/histogram.go", "histogram.cs", "AF7SAQAJBIKCAAESAAgCgIKClILIgoKmgqjCuIKClAAFEIKCpoKCrLKUlKamuIKUlMqmuIKC")]
[assembly: go.GoPositionMap("runtime/iface.go", "iface.cs", "ABUmggAJFJQABRgADAKCuIKClIKm3IKAgsiCgIKCyIKC3IKCgoKCgpSCAAcQqviCgoK4goKUgpSC7AAICoKmgsqC3IKCppSmqvaCgoKCgsqUyoKClIIAAx4ADQKCgtyCgoK4goKCgoKCgoKCgpSCgoKCgoKClIKCgoKClNy0gpSmgoKCgoKmrsKqwoKClKqiAB0ypIKCgoIAARgACwKClIKUgpSCgqS0gpSClIKmgoKmooKCgqaClKaigoKCpoKUAAIYAAsCgpSClAACGAALAoKUgpQAAhgADQSClIKUpoKUlKaCgpSs0oKCgqamgsqUppSmuMqUpqKmgoKC7oKmgoKCppKCgoKCgpSmgoKmlMqe8qaCpoKCgoK4gsq4ppS4uMqUrNKmgoKC7oKmgoKCppKCgoKCgoKUpoKCppTKAAEYAAoCqJKmpoKCgoIAChzC3LI=", "539-549:1;645-656:1")]
[assembly: go.GoPositionMap("runtime/lfstack.go", "lfstack.cs", "ABYwooKCgoKCgsqClIKUgoKClIKCggAHEJS6gtqigIKkpoKmgg==")]
[assembly: go.GoPositionMap("runtime/linkname_shim.go", "linkname_shim.cs", "AEWwAQAUAoKCppSCgpSCAAImABACAAIoABMCgoKmlIIAAigAEQIAAiIADgIAAiIADgI=")]
[assembly: go.GoPositionMap("runtime/list.go", "list.cs", "ABQyooIAGS6CgqaClKiSqJKoyIKmgoKmqJKCuKaCpqaCgqaokpSCpoKCpoKmgqaC")]
[assembly: go.GoPositionMap("runtime/list_manual.go", "list_manual.cs", "ABtAooIAFi6CgqaClKiSqJKoyIKmgoKmqJKCuKaCpqaCgqaokpSCpoKCpoKmgqaC")]
[assembly: go.GoPositionMap("runtime/lock_sema.go", "lock_sema.cs", "AA4msgALlAKCgoKUggAEIoKm")]
[assembly: go.GoPositionMap("runtime/lock_spinbit.go", "lock_spinbit.cs", "AEOUAZKClAARIJKCgpQAAxgACAKAgoKAgpQABSIADQKU7AAFEIIABNgBggAEiAGigIKmgoKU")]
[assembly: go.GoPositionMap("runtime/lockrank.go", "lockrank.cs", "AFy8AQBJigGCgpSClIKUAAcQ")]
[assembly: go.GoPositionMap("runtime/lockrank_off.go", "lockrank_off.cs", "ABEgpoKmgqyypoKssqzYuLi4uLg=")]
[assembly: go.GoPositionMap("runtime/malloc.go", "malloc.cs", "AIYDlgaCgpSCgoIAEQaCgqamuJSUgoKUgoKUgoKUgoKUypSmgqaCgpSCggAHEIKCgoKUgoKmgpSCuIKmgriCgoKCgoKClKamADt4gsqCppSCgoKmgtyCgoK4goKUgoL8goLGgrS0tLSmlLTagoKUgoIAEyiCgoIACRQACBKCgpSClKbKgoKCgoKmgoLcgoK4ABEkABUClJTcgoKCyoKCgoKUlIKUlJSUgpSCggAHEIKUgqaCytyCgriCgoKCgriCgoKCgoKCgpSmgriCpoKCgriCggAIEoKClIKUlKaClIKCgoKCyoKCgpSCgpSCgtyC3LiCpgACIAAQBNyCgoKUgriCgoKUAAYSwoKCgoKCgpSCgoKmAAcYAA0CgoKClIKClIKClKaCpoKCgoKUAB1YABQCgoLKgqaCgpSClO6mgoCCzKKCgsqCqJKClIKClIKUpqaCuIKClKaClIKUuLiCpriClIKUpoKClIKUgriCgILagpTWtIKCgpSClIKmAB4+gpSCgu6CgoKClJSCgoKCgqaCgoKClIKCppSCAAgSlMoACRQACxiCgpSClIKAgsgADBqU1rSCgoKUgpSCppSCgoKClJSCgqaUgoKCpoKClIKCAAgSlMoACRQACxiCgpSClIKAgraq9oIABxCCyoK4AA0cypTmtIKCgpSClIKUgqaUgoKCgoKCgpSCgpSmlJQABxCUygAJFAALGIKClIKUgoCCtta0goKClIKUgpSCppSCgoKCgpSUgoKCgoKUgoKUgoIABxCUygAJFAALGIKClIKUgoCCtqa0goKClIKmlKaCgoKCggAIEpTKAAkUAAsYgoKUgqaAguyUAAoWgpTugoKmgoKCAAcQgoKCgoKUppSUlKaClKaCgoKCAAgSggADENSCgriUuL7EgoKmABNsACkCgpSmpoLKgoKUgpSUlIKCpgANHIKClICCyILKgoKmgrimqqKClKaClIKClLiCptyClNrCgriCgpSUgpSC3IKUlLiClKYACArGgoKUgIKkgpSCgpSCpoKmgoLMkoKUAAIWAAgChJKCgpSmgoKUvtKokgACHgAOAqiyAAIgAA8CgpSCgpQAAiQADwKokt7CgoKUgoIAAhTylJSUlKrGlLQAAhYACAKCgoKUggAYRgANAoKClAASygGCypSCgqaCgoKUgoCClIKClKQADhqiqqKUpKSkpKSkpA==", "2331-2333:1")]
[assembly: go.GoPositionMap("runtime/malloc_generated.go", "malloc_generated.cs", "ABEapIKCuJSCgILIgqaUlIKClJSCgoKUgoKCgoKCgoKCgryCgpSCgpSElIKUgqaUlIKCgpaClIKCuIKCgoKClIKEgoKCgoKWgqaCgpSClKaUhJamgoKUgpSCgIK2goSmgqaCgILIgpSmpIKCuJSCgILIgqaUlIKClJSCgoKUgoKCgoKCgoKCgryCgpSCgpSElIKUgqaUlIKCgpaClIKCuIKCgoKClIKEgoKCgoKWgqaCgpSClKaUhJamgoKUgpSCgIK2goSmgqaCgILIgpSmpIKCuJSCgILIgqaUlIKClJSCgoKUgoKCgoKCgoKCgryCgpSCgpSElIKUgqaUlIKCgpaClIKCuIKCgoKClIKEgoKCgoKWgqaCgpSClKaUhJamgoKUgpSCgIK2goSmgqaCgILIgpSmpIKCuJSCgILIgqaUlIKClJSCgoKUgoKCgoKCgoKCgryCgpSCgpSElIKUgqaUlIKCgpaClIKCuIKCgoKClIKEgoKCgoKWgqaCgpSClKaUhJamgoKUgpSCgIK2goSmgqaCgILIgpSmpIKCuJSCgILIgqaUlIKClJSCgoKUgoKCgoKCgoKCgryCgpSCgpSElIKUgqaUlIKCgpaClIKCuIKCgoKClIKEgoKCgoKWgqaCgpSClKaUhJamgoKUgpSCgIK2goSmgqaCgILIgpSmpIKCuJSCgILIgqaUlIKClJSCgoKUgoKCgoKCgoKCgryCgpSCgpSElIKUgqaUlIKCgpaClIKCuIKCgoKClIKEgoKCgoKWgqaCgpSClKaUhJamgoKUgpSCgIK2goSmgqaCgILIgpSmpIKCuJSCgILIgqaUlIKClJSCgoKUgoKCgoKCgoKCgryCgpSCgpSElIKUgqaUlIKCgpaClIKCuIKCgoKClIKEgoKCgoKWgqaCgpSClKaUhJamgoKUgpSCgIK2goSmgqaCgILIgpSmpIKCuJSCgILIgqaUlIKClJSCgoKUgoKCgoKCgoKCgryCgpSCgpSElIKUgqaUlIKCgpaClIKCuIKCgoKClIKEgoKCgoKWgqaCgpSClKaUhJamgoKUgpSCgIK2goSmgqaCgILIgpSmpIKCuJSCgILIgqaUlIKClJSCgoKUgoKCgoKCgoKCgryCgpSCgpSElIKUgqaUlIKCgpaClIKCuIKCgoKClIKEgoKCgoKWgqaCgpSClKaUhJamgoKUgpSCgIK2goSmgqaCgILIgpSmpIKCuJSCgILIgqaUlIKClJSCgoKUgoKCgoKCgoKCgryCgpSCgpSElIKUgqaUlIKCgpaClIKCuIKCgoKClIKEgoKCgoKWgqaCgpSClKaUhJamgoKUgpSCgIK2goSmgqaCgILIgpSmpIKCuJSCgILIgqaUlIKClJSCgoKUgoKCgoKCgoKCgryCgpSCgpSElIKUgqaUlIKCgpaClIKCuIKCgoKClIKEgoKCgoKWgqaCgpSClKaUhJamgoKUgpSCgIK2goSmgqaCgILIgpSmpIKCuJSCgILIgqaUlIKClJSCgoKUgoKCgoKCgoKCgryCgpSCgpSElIKUgqaUlIKCgpaClIKCuIKCgoKClIKEgoKCgoKWgqaCgpSClKaUhJamgoKUgpSCgIK2goSmgqaCgILIgpSmpIKCuJSCgILIgqaUlIKClJSCgoKUgoKCgoKCgoKCgryCgpSCgpSElIKUgqaUlIKCgpaClIKCuIKCgoKClIKEgoKCgoKWgqaCgpSClKaUhJamgoKUgpSCgIK2goSmgqaCgILIgpSmpIKCuJSCgILIgqaUlIKClJSCgoKUgoKCgoKCgoKCgryCgpSCgpSElIKUgqaUlIKCgpaClIKCuIKCgoKClIKEgoKCgoKWgqaCgpSClKaUhJamgoKUgpSCgIK2goSmgqaCgILIgpSmpIKCuJSCgILIgqaUlIKClJSCgoKUgoKCgoKCgoKCgryCgpSCgpSElIKUgqaUlIKCgpaClIKCuIKCgoKClIKEgoKCgoKWgqaCgpSClKaUhJamgoKUgpSCgIK2goSmgqaCgILIgpSmpIKCuJSCgILIgqaUlIKClJSCgoKUgoKCgoKCgoKCgryCgpSCgpSElIKUgqaUlIKCgpaClIKCuIKCgoKClIKEgoKCgoKWgqaCgpSClKaUhJamgoKUgpSCgIK2goSmgqaCgILIgpSmpIKCuJSCgILIgqaUlIKClJSCgoKUgoKCgoKCgoKCgryCgpSCgpSElIKUgqaUlIKCgpaClIKCuIKCgoKClIKEgoKCgoKWgqaCgpSClKaUhJamgoKUgpSCgIK2goSmgqaCgILIgpSmpIKCuJSCgILIgqaUlIKClJSCgoKUgoKCgoKCgoKCgryCgpSCgpSElIKUgqaUlIKCgpaClIKCuIKCgoKClIKEgoKCgoKWgqaCgpSClKaUhJamgoKUgpSCgIK2goSmgqaCgILIgpSmpIKCuJSCgILIgqaUlIKClJSCgoKUgoKCgoKCgoKCgryCgpSCgpSElIKUgqaUlIKCgpaClIKCuIKCgoKClIKEgoKCgoKWgqaCgpSClKaUhJamgoKUgpSCgIK2goSmgqaCgILIgpSmpIKCuJSCgILIgqaUlIKClJSCgoKUgoKCgoKCgoKCgryCgpSCgpSElIKUgqaUlIKCgpaClIKCuIKCgoKClIKEgoKCgoKWgqaCgpSClKaUhJamgoKUgpSCgIK2goSmgqaCgILIgpSmpIKCuJSCgILIgqaUlIKClJSCgoKUgoKCgoKCgoKCgryCgpSCgpSElIKUgqaUlIKCgpaClIKCuIKCgoKClIKEgoKCgoKWgqaCgpSClKaUhJamgoKUgpSCgIK2goSmgqaCgILIgpSmpIKCuJSCgILIgqaUlIKClJSCgoKUgoKCgoKCgoKCgryCgpSCgpSElIKUgqaUlIKCgpaClIKCuIKCgoKClIKEgoKCgoKWgqaCgpSClKaUhJamgoKUgpSCgIK2goSmgqaCgILIgpSmpIKCuJSCgILIgqaUlIKClJSCgoKUgoKCgoKCgoKCgryCgpSCgpSElIKUgqaUlIKCgpaClIKCuIKCgoKClIKEgoKCgoKWgqaCgpSClKaUhJamgoKUgpSCgIK2goSmgqaCgILIgpSmpIKCuJSCgILIgqaUlIKClJSCgoKUgoKCgoKCgoKCgryCgpSCgpSElIKUgqaUlIKCgpaClIKCuIKCgoKClIKEgoKCgoKWgqaCgpSClKaUhJamgoKUgpSCgIK2goSmgqaCgILIgpSmpIKCuJSCgILIgqaUlIKClJSCgoKUgoKCgoKCgoKCgryCgpSCgpSElIKUgqaUlIKCgpaClIKCuIKCgoKClIKEgoKCgoKWgqaCgpSClKaUhJamgoKUgpSCgIK2goSmgqaCgILIgpSmpIKCuJSCgILIgqaUlIKClJSCgoKUgoKCgoKCgoKCgryCgpSCgpSElIKUgqaUlIKCgpaClIKCuIKCgoKClIKEgoKCgoKWgqaCgpSClKaUhJamgoKUgpSCgIK2goSmgqaCgILIgpSmhIKCpoKCuJSCgILIgqaUlIKClJSClIKChIKCgoKUhIKCgoKCgoSCpoKAgsiClLqClIKmgoKCgoKCgoKCgryCgpSCgpSEgqaUhJamgoKUgpSCgILIhJSCpoKAgsiClKaEgoKmgoK4lIKAgsiCppSUgoKUlIKUgoKEgoKCgpSEgoKCgoKChIKmgoCCyIKUuoKUgqaCgoKCgoKCgoKCvIKClIKClISCppSElqaCgpSClIKAgsiElIKmgoCCyIKUpoSCgqaCgriUgoCCyIKmlJSCgpSUgpSCgoSCgoKClISCgoKCgoKEgqaCgILIgpS6gpSCpoKCgoKCgoKCgoK8goKUgoKUhIKmlISWpoKClIKUgoCCyISUgqaCgILIgpSmhIKCpoKCuJSCgILIgqaUlIKClJSClIKChIKCgoKUhIKCgoKCgoSCpoKAgsiClLqClIKmgoKCgoKCgoKCgryCgpSCgpSEgqaUhJamgoKUgpSCgILIhJSCpoKAgsiClKaEgoKmgoK4lIKAgsiCppSUgoKUlIKUgoKEgoKCgpSEgoKCgoKChIKmgoCCyIKUuoKUgqaCgoKCgoKCgoKCvIKClIKClISCppSElqaCgpSClIKAgsiElIKmgoCCyIKUpoSCgqaCgriUgoCCyIKmlJSCgpSUgpSCgoSCgoKClISCgoKCgoKEgqaCgILIgpS6gpSCpoKCgoKCgoKCgoK8goKUgoKUhIKmlISWpoKClIKUgoCCyISUgqaCgILIgpSmhIKCpoKCuJSCgILIgqaUlIKClJSClIKChIKCgoKUhIKCgoKCgoSCpoKAgsiClLqClIKmgoKCgoKCgoKCgryCgpSCgpSEgqaUhJamgoKUgpSCgILIhJSCpoKAgsiClKaEgoKmgoK4lIKAgsiCppSUgoKUlIKUgoKEgoKCgpSEgoKCgoKChIKmgoCCyIKUuoKUgqaCgoKCgoKCgoKCvIKClIKClISCppSElqaCgpSClIKAgsiElIKmgoCCyIKUpoSCgqaCgriUgoCCyIKmlJSCgpSUgpSCgoSCgoKClISCgoKCgoKEgqaCgILIgpS6gpSCpoKCgoKCgoKCgoK8goKUgoKUhIKmlISWpoKClIKUgoCCyISUgqaCgILIgpSmhIKCpoKCuJSCgILIgqaUlIKClJSClIKChIKCgoKUhIKCgoKCgoSCpoKAgsiClLqClIKmgoKCgoKCgoKCgryCgpSCgpSEgqaUhJamgoKUgpSCgILIhJSCpoKAgsiClKaEgoKmgoK4lIKAgsiCppSUgoKUlIKUgoKEgoKCgpSEgoKCgoKChIKmgoCCyIKUuoKUgqaCgoKCgoKCgoKCvIKClIKClISCppSElqaCgpSClIKAgsiElIKmgoCCyIKUpoSCgqaCgriUgoCCyIKmlJSCgpSUgpSCgoSCgoKClISCgoKCgoKEgqaCgILIgpS6gpSCpoKCgoKCgoKCgoK8goKUgoKUhIKmlISWpoKClIKUgoCCyISUgqaCgILIgpSmhIKCpoKCuJSCgILIgqaUlIKClJSClIKChIKCgoKUhIKCgoKCgoSCpoKAgsiClLqClIKmgoKCgoKCgoKCgryCgpSCgpSEgqaUhJamgoKUgpSCgILIhJSCpoKAgsiClKaEgoKmgoK4lIKAgsiCppSUgoKUlIKUgoKEgoKCgpSEgoKCgoKChIKmgoCCyIKUuoKUgqaCgoKCgoKCgoKCvIKClIKClISCppSElqaCgpSClIKAgsiElIKmgoCCyIKUpoSCgqaCgriUgoCCyIKmlJSCgpSUgpSCgoSCgoKClISCgoKCgoKEgqaCgILIgpS6gpSCpoKCgoKCgoKCgoK8goKUgoKUhIKmlISWpoKClIKUgoCCyISUgqaCgILIgpSmhIKCuJSCgILIgqaUlIKClJSCgoKUhIKCgoKEgoSmgqaCgILIgpS6goKCgoKCgoKCgryCgpSCgqaUhJamgoKUgpSCgIK2goSmgqaCgILIgpSmhIKCuJSCgILIgqaUlIKClJSCgoKUhIKCgoKEgoSmgqaCgILIgpS6goKCgoKCgoKCgryCgpSCgqaUhJamgoKUgpSCgIK2goSmgqaCgILIgpSmhIKCuJSCgILIgqaUlIKClJSCgoKUhIKCgoKEgoSmgqaCgILIgpS6goKCgoKCgoKCgryCgpSCgqaUhJamgoKUgpSCgIK2goSmgqaCgILIgpSmhIKCuJSCgILIgqaUlIKClJSCgoKUhIKCgoKEgoSmgqaCgILIgpS6goKCgoKCgoKCgryCgpSCgqaUhJamgoKUgpSCgIK2goSmgqaCgILIgpSmhIKCuJSCgILIgqaUlIKClJSCgoKUhIKCgoKEgoSmgqaCgILIgpS6goKCgoKCgoKCgryCgpSCgqaUhJamgoKUgpSCgIK2goSmgqaCgILIgpSmhIKCuJSCgILIgqaUlIKClJSCgoKUhIKCgoKEgoSmgqaCgILIgpS6goKCgoKCgoKCgryCgpSCgqaUhJamgoKUgpSCgIK2goSmgqaCgILIgpSmhIKCuJSCgILIgqaUlIKClJSCgoKUhIKCgoKEgoSmgqaCgILIgpS6goKCgoKCgoKCgryCgpSCgqaUhJamgoKUgpSCgIK2goSmgqaCgILIgpSmhIKCuJSCgILIgqaUlIKClJSCgoKUhIKCgoKEgoSmgqaCgILIgpS6goKCgoKCgoKCgryCgpSCgqaUhJamgoKUgpSCgIK2goSmgqaCgILIgpSmhIKCuJSCgILIgqaUlIKClJSCgoKUhIKCgoKEgoSmgqaCgILIgpS6goKCgoKCgoKCgryCgpSCgqaUhJamgoKUgpSCgIK2goSmgqaCgILIgpSmhIKCuJSCgILIgqaUlIKClJSCgoKUhIKCgoKEgoSmgqaCgILIgpS6goKCgoKCgoKCgryCgpSCgqaUhJamgoKUgpSCgIK2goSmgqaCgILIgpSmhIKCuJSCgILIgqaUlIKClJSCgoKUhIKCgoKEgoSmgqaCgILIgpS6goKCgoKCgoKCgryCgpSCgqaUhJamgoKUgpSCgIK2goSmgqaCgILIgpSmhIKCuJSCgILIgqaUlIKClJSCgoKUhIKCgoKEgoSmgqaCgILIgpS6goKCgoKCgoKCgryCgpSCgqaUhJamgoKUgpSCgIK2goSmgqaCgILIgpSmhIKCuJSCgILIgqaUlIKClJSCgoKUhIKCgoKEgoSmgqaCgILIgpS6goKCgoKCgoKCgryCgpSCgqaUhJamgoKUgpSCgIK2goSmgqaCgILIgpSmhIKCuJSCgILIgqaUlIKClJSCgoKUhIKCgoKEgoSmgqaCgILIgpS6goKCgoKCgoKCgryCgpSCgqaUhJamgoKUgpSCgIK2goSmgqaCgILIgpSmhIKCuJSCgILIgqaUlIKClJSCgoKUhIKCgoKEgoSmgqaCgILIgpS6goKCgoKCgoKCgryCgpSCgqaUhJamgoKUgpSCgIK2goSmgqaCgILIgpSmhIKCuJSCgILIgqaUlIKClJSCgoKUhIKCgoKEgoSmgqaCgILIgpS6goKCgoKCgoKCgryCgpSCgqaUhJamgoKUgpSCgIK2goSmgqaCgILIgpSmhIKCuJSCgILIgqaUlIKClJSCgoKUhIKCgoKEgoSmgqaCgILIgpS6goKCgoKCgoKCgryCgpSCgqaUhJamgoKUgpSCgIK2goSmgqaCgILIgpSmhIKCuJSCgILIgqaUlIKClJSCgoKUhIKCgoKEgoSmgqaCgILIgpS6goKCgoKCgoKCgryCgpSCgqaUhJamgoKUgpSCgIK2goSmgqaCgILIgpSmhIKCuJSCgILIgqaUlIKClJSCgoKUhIKCgoKEgoSmgqaCgILIgpS6goKCgoKCgoKCgryCgpSCgqaUhJamgoKUgpSCgIK2goSmgqaCgILIgpSmhIKCuJSCgILIgqaUlIKClJSCgoKUhIKCgoKEgoSmgqaCgILIgpS6goKCgoKCgoKCgryCgpSCgqaUhJamgoKUgpSCgIK2goSmgqaCgILIgpSmhIKCuJSCgILIgqaUlIKClJSCgoKUhIKCgoKEgoSmgqaCgILIgpS6goKCgoKCgoKCgryCgpSCgqaUhJamgoKUgpSCgIK2goSmgqaCgILIgpSmhIKCuJSCgILIgqaUlIKClJSCgoKUhIKCgoKEgoSmgqaCgILIgpS6goKCgoKCgoKCgryCgpSCgqaUhJamgoKUgpSCgIK2goSmgqaCgILIgpSmhIKCuJSCgILIgqaUlIKClJSCgoKUhIKCgoKEgoSmgqaCgILIgpS6goKCgoKCgoKCgryCgpSCgqaUhJamgoKUgpSCgIK2goSmgqaCgILIgpSmhIKCuJSCgILIgqaUlIKClJSCgoKUhIKCgoKEgoSmgqaCgILIgpS6goKCgoKCgoKCgryCgpSCgqaUhJamgoKUgpSCgIK2goSmgqaCgILIgpSmhIKCuJSCgILIgqaUlIKClJSCgoKUhIKCgoKEgoSmgqaCgILIgpS6goKCgoKCgoKCgryCgpSCgqaUhJamgoKUgpSCgIK2goSmgqaCgILIgpQ=", "", "", "66=base/1/1/2;229=base/1/1/2;392=base/1/1/2;555=base/1/1/2;718=base/1/1/2;881=base/1/1/2;1044=base/1/1/2;1207=base/1/1/2;1370=base/1/1/2;1533=base/1/1/2;1696=base/1/1/2;1859=base/1/1/2;2022=base/1/1/2;2185=base/1/1/2;2348=base/1/1/2;2511=base/1/1/2;2674=base/1/1/2;2837=base/1/1/2;3000=base/1/1/2;3163=base/1/1/2;3326=base/1/1/2;3489=base/1/1/2;3652=base/1/1/2;3815=base/1/1/2;3978=base/1/1/2;4141=base/1/1/2;4348=base/1/1/2;4501=base/1/1/2;4654=base/1/1/2;4807=base/1/1/2;4960=base/1/1/2;5113=base/1/1/2;5266=base/1/1/2;5419=base/1/1/2;5572=base/1/1/2;5725=base/1/1/2;5878=base/1/1/2;6031=base/1/1/2;6184=base/1/1/2;6337=base/1/1/2;6490=base/1/1/2;6627=base/1/1/2;6761=base/1/1/2;6895=base/1/1/2;7029=base/1/1/2;7163=base/1/1/2;7297=base/1/1/2;7431=base/1/1/2;7565=base/1/1/2;7699=base/1/1/2;7833=base/1/1/2;7967=base/1/1/2;8101=base/1/1/2;8235=base/1/1/2;8369=base/1/1/2;8503=base/1/1/2;8637=base/1/1/2;8771=base/1/1/2;8905=base/1/1/2;9039=base/1/1/2;9173=base/1/1/2;9307=base/1/1/2;9441=base/1/1/2;9575=base/1/1/2;9709=base/1/1/2;9843=base/1/1/2")]
[assembly: go.GoPositionMap("runtime/malloc_stubs.go", "malloc_stubs.cs", "AC1UgoKCyqaCqqTcgoKmgoLupoKAguyCuJSCgqbugriCgILagpQAAhLipqKClIKUgpSCuIKCpoKClJSCgoKCgoKUgoKUppSCgoIACBKUygAJFAALGIKClIKUgoCCyKaigpSClIK4jNKmgoKUlIKCgqaUgoIABxCmgoKUgoIACBKUygAJFAALGIKClIKUgoCCtqaigpSClIK4goKmgoKUAB4+gpSCgu6CgoKClJSCgoKCgqaCgoKClIKCppSCAAgSlMoACRQACxiCgpSClIKAgsgADBqUqsKCgoKCgoKCgoKCgoK4pqKCgoKCgoKCgoKCuKaCgpSCgpQAAhQACwSUpoKCgsqClIKC3IKCgoKCAAASAAgClIKCgoKCpoKmgoKU1qKCgoKCgg==")]
[assembly: go.GoPositionMap("runtime/malloc_tables_generated.go", "malloc_tables_generated.cs", "AAgQAIMEiAg=")]
[assembly: go.GoPositionMap("runtime/map.go", "map.cs", "ABUugoKUAAIgAA0CAAIkAA8CgqYAAhoACxYACgSCgoKUpoKCgpQAAiQAEBYACwKCgoKClIKUgqau4oKCpoKqwoKCpqiSgoKCpgAFJgATBIK4AAIcAAsCgpSUqJKClJQAAhgACwKCqLKCqJKokgACGgAMAoKUgoKUqJKosoKUgoKUAAIQ6tKCgoKCgg==")]
[assembly: go.GoPositionMap("runtime/map_fast32.go", "map_fast32.cs", "AA0gAAQWAAoYAAsWAAoG")]
[assembly: go.GoPositionMap("runtime/map_fast64.go", "map_fast64.cs", "AA0gAAQWAAoYAAsYAAsG")]
[assembly: go.GoPositionMap("runtime/map_faststr.go", "map_faststr.cs", "AA0gAAQWAAoYAAsG")]
[assembly: go.GoPositionMap("runtime/mbarrier.go", "mbarrier.cs", "ABesAgCAAQKClLgACBKCggADFAALCAACEgAMCgACIAAPAoKClIKClIKClKiSqJIAAhoADQK4lKaCggAEHgAMAoKClILugoKCgpSCgpSCgqaCpoLugriCuIIAAiAADwKClAACHAANAriUAAIYAAkCqJKossqUqLKCppQAAiQAEASC")]
[assembly: go.GoPositionMap("runtime/mbitmap.go", "mbitmap.cs", "AEOYAQAKBgAYTgANAoKCgpQABRYACgKCgoKmgoKUlKiSlIK4gpSmggAFGAAJAoKClIKmggACLgAVBIKWkoKUlJSmpgACFgAIAoKCuIK4goKUuIK4goKCAAQYAAgEgoKUppSCgpTKpoKClKamgoKmgsqClIKClAACENIABVoAKgKClIKUgqaCgoKmgoKCpoLulJaSgqaCgpSUgoKCgIKkgoKmgoKAgqSCgoKCAAQmAA4CgpSClIKWkoKmgoKUlIKCgIKkgoK6koKCgpSCgpSCAAckAA4ClIKClILclJSssoKCgoKowoKCgpQAAhLiggAJFIKCgoKUgpSCgoKmlAACFAALBKaCgoLKgpSCgpSm3IKCgoKCgpSCgoKCgqaCpoKCgoKCgoKmABQsooKUgoKU1rIAESSUgoKUpqIAMWSCgpSmssqCgpSCgoKClIKCgqaCgpTWxIKCgpSClIKCgoKCpoKCgoKUgoK4goKCgpSUgoKCgoKCgoKAgoKCpIKU1rKCgoKUgoKUgoKCgoKmgoKCgoKUgoK4goKCgpSUgoKCgoKCgoCCgoKkgqaClIKCgoKCpoKUppTYwoKUuJSCgoKCkoKCgoKUgqaCgoKCgoKCgpKCgoKCpriCgoKCgpSmrviu+K74AAISAAkIABEmkoKuwoKCgoKCgoKCgoLewoKCgpSCppSClIKCgpSUgoK4goKCpoKU3IKUggACGAAJAoKUggACIAANAoKUggAFFgAIAqaUgoKUrLKmgoKCqJKoyKiSqMjYsoKClKyygoKCAAsYqJKCgpSUAAoQAAsSgoKCgoKUlJSCgoKUggACNgAcAqaCuJTcgJSCuIKUtoKCrOgABBoACgKCgpSCgoKClIKUlIKCgoKUgoKCpgAHJAAQAoKUgoKUgpSCgoKCgoKUlIKCgoKCzqKCysqClKqigoKClJQACxaygoKCgoKUAAIqABQClpKUgoKmgoKCuIKCgpSUlIKCgoKCgpSAgoKCpLiCgoKCgoLcgoKCgoKCpgAIEoKClIKmgoKCgoLugoK47oKClKaCgpSCgriCgoLugoKCgoKCgsqCgoKU3JSCgIKCgoLIgoKCgoKmgIKC+oKCgoKClKaigoKCgoKClIKCgoKClIKUgoKCgoKCgriCgoKCgoKmggAEFuI=")]
[assembly: go.GoPositionMap("runtime/mcache.go", "mcache.cs", "AEuqAbIACxaCgoKCgoKUgpSUAAIS4oKC7oKCAAMQ1IKCypSUAAgQ5JSCyoKUgqaUgpSmgoKmgoKUpoKmuIKCpoLKpgAOHoKClKiSgpSCgtyUgoKCuIKCgqamuAAIEoKCppSClIKCgoKCgqaCgriU3LiCuIKmgoKC7qbcAAwQgoKCgoKClIKCqqKCuIKCqqKClA==", "99-104:1;120-132:1")]
[assembly: go.GoPositionMap("runtime/mcentral.go", "mcentral.cs", "AC1ikoKCgoKqoqqiqqKqotikgpSCgoKCAA8glIKmgoCCtoKUgoKClICUgoIACxaCgoKUgJSUgoKCgqbIlIKCgoK4goK4goKCgoKmgoKUgpS4lN7igqaCpu6muAAHEIKUlLjMkoKCgpSC")]
[assembly: go.GoPositionMap("runtime/mcheckmark.go", "mcheckmark.cs", "ACZMsqaCgpSUgoKUpqaClIK42JKClPyygoKCpqaUgpSCgpSUlIKmsoKClJSCgoKCAAIQ0u6CgoKCgoKCgoIADgqilJyMgoKUlIKCuIK4gpSClLS2gu6CgpSCgpSClIKCgpSClKaCgoKCgoKCgoKUgsqCgpSCgoKUpoKUgoKUuJSCgoKCgpSmgoKCgoKUppSCgpQAAxLSpoKmlIKClIKCAAcQuIKC", "42-57:1;167-219:1;183-190:1.1")]
[assembly: go.GoPositionMap("runtime/mem.go", "mem.cs", "AApiACgCgoK4gqYAAhDSgqqiAAIaAAoCgqyyrLKqogACKAAXDoKmgoIAAhgACQKCAAIcAAsCuIKmAAYQ0oKClMqCgoKUpOyCgoKUgoCCpKa2goIABxCCgpSCgpQAAxwAEA6Cpq7Cgg==")]
[assembly: go.GoPositionMap("runtime/mem_darwin.go", "mem_darwin.cs", "AAkewoKClKamprimpqauwqaCpoKCgpQACAqCgoKUgoK4gg==")]
[assembly: go.GoPositionMap("runtime/mem_nonsbrk.go", "mem_nonsbrk.cs", "AAwagg==")]
[assembly: go.GoPositionMap("runtime/metrics.go", "metrics.cs", "ACx8soKmuIIACxiUlIKmgtyC3ILcgtyC3ILcgtyC3ILcgtyC3ILcgtyC3ILcgtyC3ILcgtyC3ILcgtyC3IKmgu6C3ILcgqaC7oLcgtyCyoLKgtyC3ILcgsqC3MqC3ILegtyC3ILcgtyC3ILcgtyC3ILcgtyC3ILcggAEEILcgtyC3ILcgtyC3ILKysrKytyCyoLKgoK4pqKCyqKCqJKCgoKClJKCABMusoKClKjCgoKUqMKCgpSokoKCpqiSAB1KsqaCgoKCgoKCgoKClIIAFzKygoKCgoKClIKCgoKCggAJFpIAECSSgoKCAAsakoIAELQBogARKNKCgpSCgpSUtLS0tLS2ABxEsoKClIKClIKClAAWNMKCgoKUlIKClJSssqamgg==", "91-94:1;98-101:2;105-108:3;112-115:4;119-122:5;126-129:6;133-136:7;140-143:8;147-150:9;154-157:10;161-164:11;168-171:12;175-178:13;182-185:14;189-192:15;196-199:16;203-206:17;210-213:18;217-220:19;224-227:20;231-234:21;238-241:22;245-248:23;252-260:24;264-267:25;271-274:26;278-286:27;290-293:28;297-300:29;304-307:30;310-313:31;316-319:32;323-326:33;330-333:34;337-340:35;343-346:36;349-352:37;355-358:38;362-366:39;370-373:40;377-380:41;384-387:42;391-394:43;398-401:44;405-408:45;412-415:46;419-422:47;426-429:48;433-436:49;440-443:50;447-450:51;454-460:52;463-466:53;470-473:54;477-480:55;484-487:56;491-494:57;498-501:58;505-508:59;511-513:60;516-518:61;521-523:62;526-528:63;531-533:64;537-540:65;543-546:66;730-737:1", "579=runtime.metricReader.compute", "75=makeStatDepSet/1/53/8,makeStatDepSet/2/53/15,makeStatDepSet/3/53/22,makeStatDepSet/4/53/29,makeStatDepSet/5/53/36,makeStatDepSet/6/53/43,makeStatDepSet/7/53/50,makeStatDepSet/8/53/57,makeStatDepSet/9/53/64,makeStatDepSet/10/53/71,makeStatDepSet/11/53/78,makeStatDepSet/12/53/85,makeStatDepSet/13/53/92,makeStatDepSet/14/53/99,makeStatDepSet/15/53/106,makeStatDepSet/16/53/113,makeStatDepSet/17/53/120,makeStatDepSet/18/53/127,makeStatDepSet/19/53/134,makeStatDepSet/20/53/141,makeStatDepSet/21/53/148,makeStatDepSet/22/53/155,makeStatDepSet/23/53/162,makeStatDepSet/24/53/174,makeStatDepSet/25/53/181,makeStatDepSet/26/53/188,makeStatDepSet/27/53/200,makeStatDepSet/28/53/207,makeStatDepSet/29/53/214,makeStatDepSet/30/53/233,makeStatDepSet/31/53/240,makeStatDepSet/32/53/247,makeStatDepSet/33/53/272,makeStatDepSet/34/53/280,makeStatDepSet/35/53/287,makeStatDepSet/36/53/294,makeStatDepSet/37/53/301,makeStatDepSet/38/53/308,makeStatDepSet/39/53/315,makeStatDepSet/40/53/322,makeStatDepSet/41/53/329,makeStatDepSet/42/53/336,makeStatDepSet/43/53/343,makeStatDepSet/44/53/350,makeStatDepSet/45/53/357,makeStatDepSet/46/53/364,makeStatDepSet/47/53/380,makeStatDepSet/48/53/387,makeStatDepSet/49/53/394,makeStatDepSet/50/53/401,makeStatDepSet/51/53/408,makeStatDepSet/52/53/415,makeStatDepSet/53/53/447")]
[assembly: go.GoPositionMap("runtime/mfixalloc.go", "mfixalloc.cs", "ADdwwoKUlIKCgoKCgoKCgtaCgoKmgoKCgoKUlIKCpoKClIKCgqaCgoKC")]
[assembly: go.GoPositionMap("runtime/mgc.go", "mgc.cs", "AJgB3gIAEgLWgoKmyqaCpoKCgoKCgoKCrtSCgoKCggAmTpKCACZSAAYU1oKCgpSCpgC8AaYE1gAlTIK8opSCgpSUlILKggAjVrKClJSCpIKUgraUAAUSAAwKgoCCgqSClIC4goKiAA4cypSCgu6CgoKCuIK4lIKCgriUgIKCtoK4lJSCppSCgpSCgoKCuJQADRzKggAHEILKlLimyoK4goKmABAilILu7rjupoKCgqbugriCpqIAHFoAGAaUjuKCyrimgqbupoKCuNyCuO6CgoKCgu4AChaCgoKCgoLugpTcpoKUpoKUgqa4pri4gsq4yqau9JSmvMyCgqYABBCCtqyynOSCgoKUgoCCgoKC2oKCgpSsspSmuILYggAGEKKUgoKCAAQSwsqUuJS4lIKC7pSCxoLagoL6tJSClIKCgrjuggAJFIKCgoKApIK4gqaClIKCgriUgri4poKCgoKCgoKCggAJFIK43IKmlIK4goKC3KbKAA0cgoKCgqaUpoKkgoIAChbcuKYADRyClIKCgoKCgqaClJTu3IKUgoKmgpSCgoKClIKUgu6UgoKUlNyClJSClNyCgpSClLiCgoKCgriCgriCpoKmlAADEAAJDIIACRSCgpSCgoIACBK4vAALFIIAHDCCyu6ClJSUABEkpoKUgAAPIMimAAgSgpSCgqaCpoKCgoKmlAALFIKUtILcgIKCgtq0tKS4goKCgpSC7ri4gpTOwoKUqJLaooKCgu6ygoKClOyygpSmgIKCtqbcAAcQ3KaUpoKCgoKClJSClJSC3AAIEoKCgpS4AAUWAAgClIKmgoKCgoKCgpSUpoKClIKmpoKUuIKCuIKCgpSCAAIaAAwGgoLKgoKCgoKmggAHJAAKAqiSppSCuILugpKCgpSCuIaygoKUgqyygoKCgoKCgpSUgqiSlKaCgoKUgoKClAACGgAKAoIACoQBAAgCgoKClICCpIKClIKmgg==", "752-754:1;834-836:2;869-871:3;947-954:4;1042-1059:1;1083-1085:2;1103-1111:3;1128-1135:4;1260-1276:1;1284-1289:1;1365-1374:1;1377-1389:2;1380-1380:2.1;1500-1516:3;1545-1558:4;1548-1552:4.1;1649-1651:5;1791-1818:1;1847-1884:2;2116-2119:1", "", "1525=itoaDiv/1/1/1;1556=Load/1/2/3,Load/2/2/4")]
[assembly: go.GoPositionMap("runtime/mgclimit.go", "mgclimit.cs", "AE+0AcIABhIACAKmlIKmgoIACRTSgtyAgqSCggAGEqKqoqqi3uK4lIKUgtiygpSUgqaCgriCgqYABxCCgoKUgrS0tLS2uIKCABcwlK7igu6mgoKCgoKUuJSmlIK6ktiSgoIACRbipqaClIKCgoKCgpQAGkCSruaCgpSokgAHJAAKAoKUggACEAAJBIKCgpSUgriUgoKm7LKCgoKCgpSCpoLuppSCtLS0")]
[assembly: go.GoPositionMap("runtime/mgcmark.go", "mgcmark.cs", "ADV60oIAAhLiuIKmlqKmgIKClIIACRKuwqaYgqaCgoKmgoIADyCCAAcQpqaCppSCpoKCgoLqooCCggAIEIKCgqaCpqbKAAYaAAsEkoKUgoLIgoLIgoLstqaC2siCgoKClLiCgsragoKCAAkUgoKClIKUgoKUgsiCgoKmAAUS4pTcgoKUgoKCgriCrtSCgoKCgqiSgoKCgoKCgrimgoLsABMipoKCpoKClIKClIKCAAcQuICCgraUgsqCgpTGgrS2zPjKgriopK4ACgaClICCtoC4goKiABAggoKmgoKCAAcQgsqm3IKCgoKCggAIEoKCgoKClIKUlJSmgoKCAAcQgsqmpoKCgqaClO64gsqCgoKmAAcQgoIACxiC3IKCggAHEILKxgACGAAOBpQABxCCAAgSgpSmuIKUAAcQgrjKlIKCgoKClIKCgr6ygoKCrsK4goKmgoLcgoKClIKmggACGgAKAsqCpoKUgoKmlILupoLKgriUgoKUAAkoABMCgoKmlIK0pIL6gqyygpSUuIKClJSmpoKUgqaC3IKokoLuoqaUpsqCtpQABxCCgoKClIKClIKUlIKCgoKClIKUgoKClNyCgoKCgoKClLiClIKUrOKCpoKCgoIAChaCgoKC3JTKgKa2ysqUpqaCgriCuLiCgoKCgpSClJSClAALIKKqooKClKqirsKUgriUlKYABUIAIAKCyoKCgoKUuIKCgoKCgoK4poKCgpSCgriCgoIADyDcgqiSgoCCgIKAgoC4goCCgIIADA6CgoKmuIKCgu6CgoKClIKUgoKC3ISSgoKUAAYiAA8Cgsq4gpSCqJKCgIKAgoCCgLiCgIKAlIKCgoKmAAwOgoKCpriCgoK4goKCAAgSAAIYAA4IgpSUgoKClIKUgoKClIKAguyCAAcaAAoCgoKCgoKCgoK4goKCgqaCgpSCgpSClKaCgoKC7oKUgpSCuKYACBKCuIKC7oKCuIKCAAQU0oKCgIIACRgACwSClJSClJSCpoKCgoKCuIKUpoKC3IKC7pSCvKKCgoKClIKAgpS2goK4lLiCgpSCgpSCgpSUggAGFgAJAoKUlLiCgpS4goKmgqyylIKCgpSCgoI=", "113-115:1;191-204:1;280-313:1;515-517:1;638-642:2;1536-1562:1", "", "202=readgstatus/1/1/1")]
[assembly: go.GoPositionMap("runtime/mgcmark_greenteagc.go", "mgcmark_greenteagc.cs", "AFS2AdKmAAsUgoKClNaiABEklMqUqqK2gsrIlAACFgAIAqyy2JKCptyygpSCgoKCgoKUlIK4rLKClKaigpSUgoKmgoKUqqIAAhLiqsKCuIKClIKCgriCgqaCgpSmgoK4goK4gpSUpqiSqLKAgraAgsiClAAbPJKClIKC3AAFFgARFJSClLiCgoKmuIKClKiygoKUrOSClKzEgoKClJTYspS4goK4ggADKgARAoKClIKClILc0oKUgoKCgoKUgoKUqqKClIKCrLKCgpTupoCCtqbugqYABhQACAKCqJKCgoKCgoKUgpSCgpSCyoKmAAYSAAgCgoKUgqamgoKCpoKClAA2dsKCgoKmgoKUgqaCgoKssoKCqLKCgoKCgoKCqJKCqAASJIKCgpSCgoKUgoKUlKyylIKClIKClICCAAkSlAAIDpKClKaCpoLawoKCgoKUpqaCgoKClIKCgpSCgriCgoKUlpKCgpTKgoKClIIABxDelJSCuIKCgIIABhbygoKmgsqCyoKCgoKCgoKUgqaCgoKUppSmooKCgoKCgoKCgrimgriigoKUgoCC7KKCooKUgoKCgoKUgoKUgoKCgrimgtyigoKUgoCC7AAIEoKCgoKUgpSCgoKmlAACEuKCAAoews6CgoKCgpSCgoKCgoKUgoKClJSCuKKCgoKCgpSuwoKUgpSClAAGGAAPCu6CgoKUpqaCptyCggAHEIKCgpSmgoKCgIKAggAIDri4AAkUgoCC2oKCgg==", "", "", "971=sizeclass/1/1/4,spanPtrMaskUnsafe/1/1/5")]
[assembly: go.GoPositionMap("runtime/mgcpacer.go", "mgcpacer.cs", "ANQC5gWigoKCggAGFNKCgoKCgoKCgoLugoKCgsqUlJS4goK4goKmyoLcysqClIKCggAIPgAYAoKmlIKCuLjcgoIAChaCAAgSgoKUlIiypgAJFIIACBK4gqYAChaCgoKs9qamlIKmAAgSlIKCAB08gt6CgoKCpoKUgoKCgoKCgoIAAxoACQLcgriC3IKC3IK4gpSCgpSCgoKClIKClIIAByIADwKCpoKmyqYABxC4ggALGKaCgoKCpoLKpoKUgtyClIKmpoIAAh4AEgqClIK4goKCuIKmgoKCgoKUAAISAAgCgoKmgoIAAhDygoKCgoKmgoKCAAYU4pSCtLSCtMiCgoKClIKmpoK4uKKCgpSCgoK4gqiSgq4ACQSmgILKgsrcAAEcAAwCgraotLKCgoKCggA+fqaWkoKmyrjKgqaUgpSmgpQAGD4ACQLuyu6CAAkUgoIACxiCgpSWkoKClJSCgoKCgpQAAigAEwKCpqbK3IKAgsiClAAYMq7igqaCgpSClKjEgoKCgsqCptaCgoKUgIKkruKCpoKCpqjEgoKCpoKUgpTmgoKClIKCgpQABRoACgKCgoKmlIKClIKCAAQaAAgCgoKokoKCgoKClIKCAAQU0oKCgoKClIKCAAQcAAkClKaCyoKCgqaCgg==", "809-820:1;1354-1359:1;1401-1412:1", "", "435=Load/1/1/1")]
[assembly: go.GoPositionMap("runtime/mgcscavenge.go", "mgcscavenge.cs", "AJIBrAKSAAIgAA0C7rjcgpQACBKCgqaCyqYADBrKgpQAbPgB0oKUgpSCgpQAChYAChamgoKCgoKClIKmgpSogu6ygoKUgqiSrNKClKYAARDigpQABRQACQKCgqbcAAgSuJSC3IKmgqaU3IKUgrimgoKUlMoABxCWkoK4gqa8woKCAAYSAAoCgoKUlJSCAAIaAAwGAAES8oLclMqCpoKmypQAAhDSlIKUgoKCgpSCAAMaAAkCgoKCgpSClIKmAAIU8pSCzIKCgoKUggACIAATDoKCAAcQgoKmgqamlNyCppSmuIKClIKCgsqCgIKkgqaClLiClAAFFgAIAgAPILiUpLS0tLS0tAALFgAGLgAUAoKCgoKC7oKUppSUgoKmlNyCgpKmyoKCgoKU7oLu7oK4lJTKgrgAO4YB0oKCgoKs1oKAgqSqwoKClIKUuIKClJSCpoKmgu6mlKaCAAIWAAgCgtyu4oKCpoKC7oKCAAMS8oKCgpQAAhDSgoIACBSSqqIAGj6SAAcSkgAXPJKokqiSqqKUlJSUuLjYkoKClIKClIKU6pKCgpSCgpSmAA48AAwEgqaCuIKClIKCgriCgriCgqaokg==", "364-366:1;393-402:2;405-409:3;412-414:4;682-684:1;824-841:1", "", "395=Load/2/3/1,Load/3/3/1;710=load/1/2/3,load/2/2/4,heapRetained/1/1/4")]
[assembly: go.GoPositionMap("runtime/mgcstack.go", "mgcstack.cs", "AIgBkgKCgpSCAA4k1gAiTsKClIKClIKUgoKCgoKCgpSUgoKUggACEPKCgpSUgpSmgoKClKaCpoKClNiygpSCgoKUgpSUgoKCgpSCgoKClKyyAAIQAAgCgpSSgoKCgoKUgoKCqqKCgoKClIKClIKClA==")]
[assembly: go.GoPositionMap("runtime/mgcsweep.go", "mgcsweep.cs", "ADl+gqamgoIACBKCrsKssoKCgoKCgpSUpoK4ggAVQAAKAoKCgpSCAAgOooKUgoKClIKCpoKCpoIABBbigoKClILMkqyyrLKCAAUS4u4ABxCC7oKCgoIABxCUpoKUgoKCgpQAACIAEAKCgoKCppSUgoK4uIKUggASJKKCpoKmgpTaori4goKCuIKCgoKCgpSAuIKClKSAlIK4ypS2lAAPIIKCpoKmpoKCpqaCAAIS4tzogoLcgpSAgoKCpNyCgoKUAAwS9oKCpoKmpoKAgoK2goKCppSCABEkgoKUgoKCpoKCgpSCgqa4poKCgoK4yqaCgoLKgoKCpriCppSCgoKCgoKCgoKmgqaClIKUgpSCpoLKgrjcgoKmgoLcgoKmgqaCgoKCyoKmgri4gIKCpIIADBqUgqaUuILKgri2gpSCgoKUppTKgoKCppTclIKmgpSmlAAHEIKCgqYADyCCgpSUuJQABSIAEAKCgoKCgoKCgoKUlIKUlIKClIKCgoKUlIKUAAIoABEClKaCgoK4goKCgoIADyCUgoKCgpSUuIKCgry0ggADEtKmgu6CuIKUlIKCgoKUgrg=", "429-443:1;751-761:1")]
[assembly: go.GoPositionMap("runtime/mgcwork.go", "mgcwork.cs", "ABo4goIAZOgB4oKCgpSuwoKmgoKCgpSCgoKCgoKCgriC3IKClAAEEsKCgqaCgq7CgqaCgoKCpoKCgoKCgpSCgqaCgpQABBjygoKCpoKCgoKCgoKUgriCAAIQ0oKCpoIAAhQACQKAgoKUgpSUgoKUgpSkggAJFIKUyoKUgoIAAxDCgpSAgoKCgoCCgpTGgoKUAAQQ0gAUMMKC6IKCAAMQwoKCgoLKgoKEkoKCgoKClJSCgpSCpoKCuIKCgoKClLiu4oIAAhDygq7CgoKClKjEgoKCgqaC3LKCgsqCgqqigoKCgpSCgoKCgpSCpoKC", "449-451:1;547-557:1")]
[assembly: go.GoPositionMap("runtime/mheap.go", "mheap.cs", "AOoClAYADCCyrsIAW8YBgqbCgoKClAAGIAANAoKUlIKCgpSCgoKClIKCgpSCgoKmggANIIKokqiSAAIeAAwCqqIABBbippQAAxTigpQAAxTiAAIQ0oKClJSkAAVk4oIAAhLi3IKUquKCgoKCqJKClIK4gqaCgpSosoKUgoKCgoKCgoKCgoKCggAIEriCppQAAhIADg6C3JSCgoKmgoKUgIKClJSClMiClIKmlIK4goKmgqaCpoKCgpQAAhIADQyUgoKCgpSCgqaCgoKCgsqCgoKmgoKCgIKCgoKUyv6ClIKCgpSCgqaCAAockgACEuiypoKUlAAFKAARAoKUqqKCgoKCgoKClAAGHAAMAoKClIKCAAgSAAgSgoK4goKUlLjcgpQAAh4ADAKmgqaCggACGgAMApSClKaCgoKUpoKCAAIaAAwClJSCgoK4AAUoABUEgoLKyoKCpoKCgriCgoKCAAgSlJQACRSCgoKCgoKUgoKmgqaUgoKCgoKClIKCuKaUlAAAFgAKAoKApqaCgraAAAgSgMqCgIKkgt6y3IKmmKaCgpS4lIK4gqaCpoKCpoKCgpS0tKSmgoKCgqaq9qa4gpSCgoKCuIKAgoKClIKCgpSUlJSUlJSmyIKCgoKmygALGAAJFJTcgqaCuMqu4pTulKaCgriCgoKCpqbKgLiUgoKmgraClMqCggAHELiC7riCgriClAAHEIKCgriokpSCgoKCuIKUgoKUlIKClIKClIIAAx4ADgSCgoKCuIKCgoKUggAIBrKUlILGgpSCgpSmgqaCxuyCgoKmgpS0tKSmpoIAAhQACgiCppSUgrqSgoC4xIKCgoKCgoKCgoKCgoKCgoKCgqaCAAkWkoLWsoKUlIKUlIKUlIKCpoLWsoKClIKmppSC1rKCgpSClKaUgqrSgriCuIKmgoKmACVSsoKCgqiygoKCAAUSAAgCgoLcgpSClKaClIKCgqaCpoLcsoKC3IKUlIKUgoKCgpSClIKCrMSCgoKCgpSCgpSClJQADyLCgoKCgoKCgoLKgoKCpoK4gpS4goKCqJKCgpSCggALIAAICoKCgoKCgoKUgsqmlMqCAAweoqzSgoKCgoKCgpSCgoKmgpSCgpSCgoKCgoKUlJaCgpSmpqaokqikgoLKgpSUgpSCgoKClJSWgoKUpqamgpSClIKUgoIACR7SgoKClIKCgqqilIKCgpSCggAiSJKokrimgrimgoKCyoLKgoKmgqbcAAcQgpTKgqiklIKCpqamuAACENKCgoIADgiSyoKCuICmgraCgoyKgoKCyoKCpoIACRSCggAIEoKClIKC7oKC1oKCgoKU3IKUlJaSgoKUgriCABYyyKrCgoKSgoKCAA8ggoKUgsqUgpSCAA0YsoKCgoKCggAfQKKmgqaCgqqigoKCAAUWAAoClIKCgoK0goKCgrSCgoKCtILGgoK0uIKCgrSCgoK0goKCtIKCgrQACxiCgoK0ggAKFpKqogAdOsKCpoKCpoLaooKmgoCC2riAgoLIuICmgoKC2oKCuIKUggACEuIAAiIADgKCgoKmgpSCpoKCgtqigoKCgoKUlIKClKaClJQ=", "1011-1018:1;1378-1380:1;1667-1696:1;1815-1815:1")]
[assembly: go.GoPositionMap("runtime/minmax.go", "minmax.cs", "AAkSgoKUpoKClKaAooCigKKA+KKClIK4pqKClIK4ptKUtKSm0pS0pA==")]
[assembly: go.GoPositionMap("runtime/mpagealloc.go", "mpagealloc.cs", "AFnKAQAJAAAJFqKokqqiqJKmlLqSgpS8oqqiAAIQABASgoKssoIAeYICwriCgpSmpqampqassoKClKyy3uK4grimyoKSgpSCysqAggAIEIKEkoKClLiClMqU3AACIAAUEIKCgpSCgoKC7oKmAAQaAAgCpoK2poKCgpSCpqa4goKUgsrugoLKgpS4gqamgoKCgoKCAAUcAAgClIKSpIKUgoKCpoKCgoKCgoKClIKCgpSCAAIS4riCgoKCyqYACiYAEAIAGzgADh4ADB6CpoKCpoKCAAcQgpSClIK4ptyCgIIAAxoACwKCgqaCypSCuIKmgpS4goKClMqCgqaUpoKUlNyCgoKCgoKUAAsUgoKmgoKCuLiCggACHgAPAriCyoKUgoCCgoKCgpSCgtqCgtyUlITagpQAAhQACQKmgIKkgqaCgoKmkqSUgqaCgoKClIKmAAIYAAkClIKSkoKCgoKUgoKCgoKCgoKUgoKClAAUMJKClKySgpSokoKUqJKClKiSgpSu6oKUyoLu7oKUpg==", "713-726:1")]
[assembly: go.GoPositionMap("runtime/mpagealloc_64bit.go", "mpagealloc_64bit.cs", "AB1CAA4eAAsYAAsW1oKmgoKCuIIABxwADAKCgtyCgtyCgoKCAAgSooIACximlNyCuO6ClIKmgriCgu7SgoKUAAsYgoKCpoKUgriCgrimgoKUgpSCpqyygoKCgoI=", "109-112:1;117-125:2;130-133:3", "", "128=add/1/2/1,add/2/2/2")]
[assembly: go.GoPositionMap("runtime/mpagecache.go", "mpagecache.cs", "ABcykgACFgAIAoKUgoKCgoKUAAIS4oKClIKCgoIAAhgACwKUgpSCuIKCppSC3ICCpIIAAhgACQK4gpSCgoKUgoKClAAHEIKmgpSCggAIEoKCpqYACRSC", "", "", "141=chunkBase/1/1/1,alignDown/1/1/1,pages64/1/1/2,block64/1/1/3;158=alignDown/1/1/1,chunkPageIndex/1/2/2,pages64/1/1/2,chunkPageIndex/2/2/3,block64/1/1/3")]
[assembly: go.GoPositionMap("runtime/mpallocbits.go", "mpallocbits.cs", "AA4ekqiSqJKokoKUgqaCgoKUlIKCpqiSgryiqJKokoKUgqaCgoKUlIKUqJKqoqrCgpSCgoKUgoKClIIACRayooKCgoKCgpSCpoKClJSUhJKUlJS4goIABxCCgsqCgpSClIKClKaCgpSmuIKCgoKCgpTGAAIYAAkCgoKCgpSu4oKCgoKUlAACGgAMApKCgoKCuKaUgoKmgoKUlAACGgAMAqKCgoKClKaUgoKClIKClIKCgpSUgpSokqiSqJKokqiSrLKqoq74goKClIKmgoKUpsoADiLUgqrUgg==")]
[assembly: go.GoPositionMap("runtime/mprof.go", "mprof.cs", "AJgBtAKygoKCACJAsoKCqtKCgoKCgoLO2IKCgoKCggAOhAKCgpSCgqYAAhYACAIAAhTygoKmgoKCgoKuwoKCgoK4goIAAxAACQyUgoKCgoKoksq4gpSCgpSCgoLckrqSlIKUgoKCAAUW4oKCgoKmgoK4poKCpoKCvKKClAAEbsKCgpKCgoKClJSUlIKCgoKCgpSAgqTKgriUACpWgoKUpoKCAAMUAAgCgqa4gqaAuIKUgoKCgpS2gqaipqaCAAkUlJSUgoKCgpKCgpSCAAMS0oCCyMqClIKCgIKCyIKClIKCgrimgqaCgpTclIKCgoKUgpQABRjygpSCgqiSgpSCggAKGqKCgqYAIESQppKqooKCpgACMAAVAoKCAAMcAA0CuIKCgoKCgoKCgoKUgqbKgoKCgoKCgpSCuIKCgoKC7riCpqKCgoKCgpSClIKUgqiSgoK4goKCgoKUAAokAAoCgpKClIKUpoKCgoKCgqaCgoKmgr7igoKClIKCgoLugpSmgqzSgoKClIKUgtyCqJKCggADFAAJAoKSgpSClKzigoKClIKCgoLKpoKokoKCAAMU4oKCgr7igoKUgoKCgqaokoKCupIABBqSqJKCpgAJBgAXPoKmgqaCpqKmpqaUpqiSsoKgkoKUuIKmpqLKppSUggAHEIKClJS4goK4goKylIKUgu6CgoIADBqSpoKCgoKCuIKmgqYACxiC3sKClKyygMq2goKmlKaCAAcQgoKClAAGFgAJAgAIEgAMGoKCppS4AAoWkKSCuLKUtoKUgIKkgpSmgqaCkoK4goLWgoKilKaCgriygqam3JCSgoKUuIKmggACEgAJAoKCgpSCgpSmgqYAChaCpoKCgoI=", "463-465:1;581-589:1;740-744:1;937-940:1;1033-1036:1;1066-1069:1;1151-1154:1;1165-1168:1;1202-1205:1;1215-1219:1;1242-1245:1;1339-1347:1;1341-1341:1.1;1396-1398:1;1426-1428:2;1439-1441:3;1566-1566:1;1576-1589:1;1596-1600:2;1609-1611:3;1621-1641:4;1635-1635:4.1", "", "650=FuncPCABIInternal/1/1/2;870=stk/1/1/5;990=stk/1/1/3;1078=stk/1/1/3")]
[assembly: go.GoPositionMap("runtime/mranges.go", "mranges.cs", "ACBCsoKClKiSgriokt7CgoKCgoKCgpSssoKClIKssoKClIKqooKUgpTMygAKGpKokqqiqqKqoqi2qJIAChqigoKClILOooKCgoKUggAEEsKqoqqigoKCgpQAGEiimrKSgoKmlLju3IKCpgACENKCgpSClIKUqJKCgpQABJABsoKUgoKCgoKClIKCqqKClIKClIKClICCgoKClIK2gg==")]
[assembly: go.GoPositionMap("runtime/msan0.go", "msan0.cs", "ABAmkKKAooCigKKA")]
[assembly: go.GoPositionMap("runtime/msize.go", "msize.cs", "AA4gwoKUgriClKaCgpQ=")]
[assembly: go.GoPositionMap("runtime/mspanset.go", "mspanset.cs", "AEumAdSCpoKCgoK4poKCgqaClIKClIKmlKaCAAoWuIKCyqqikpSClIKCgoKUuILuyoKCgu6UgoL8ysqCgrjKAA0clKaUAAcU8oKCgpSC3IKClLiUuLimpoIACRqyrLIABxKSAAsaooCCpKiSggAGEqKokqiSqJIABxKSqJKokqiS2JKUgoKUqJIABxKSqJI=")]
[assembly: go.GoPositionMap("runtime/mstats.go", "mstats.cs", "AKkCogWCgIKCyICCggAUOrLcAAkWkqaCgoKWmoKmgoKCpoKCggAIEoK4gpSalAARJIKCgoKClIKCgpSCgoKCgpSCgoKUgoKCyoKCgoKClILKgoKCgoKCABAigoKCgqaCgoKCgriCgoKCgoKCgoKC3KiSggAGEMKUgriUgoLugoKCgqaCgoKCAAIQ0pSCgoKUggAJNNIABRDSgoKCABpC8oKCgoKUgoKCgpSCgoIAL4ABABMCgIKClIKmpIIAAiIAEAKAgoKUgqYABBDilIIAAxDClAAWsgHCgoIAAhDW3KaCgoLcgqaCgoKmgoKmggAJFA==", "568-570:1", "", "393=load/4/9/1,load/5/9/1,load/6/9/1,load/7/9/2,load/8/9/2,load/9/9/2")]
[assembly: go.GoPositionMap("runtime/mwbbuf.go", "mwbbuf.cs", "AEucAZKCgriUpoIAAxLSqJIAAjIAFgKClIKCqqKClIKCAAIkABIIuILKggADHAANBIKCuJSUgpSCABAigoKC7pSClIKCuIKClKaCgqaCgpSCuJQ=", "180-182:1")]
[assembly: go.GoPositionMap("runtime/nbpipe_pipe.go", "nbpipe_pipe.cs", "AAgSwoKClIKCgoI=")]
[assembly: go.GoPositionMap("runtime/netpoll.go", "netpoll.cs", "AHuQAoCigKKAooCmkgACFAAJAoKClIKUgpSmgoK+soKCgoKUgoKCggAYNsIABCCCrsLokoKCgoKUgoKUgpSUgoKCgoKCgoKCgpSCgoKU+LKClIKClIKClIKm1riCgpSUlIKCggACENKCgpSCgoKUAAIS4oKCpoKUgoKC3Ki2urKCgoKUgoKCgqamgpSClIKCgoKUgriClIKCgpSCpoKCgpSCgoKUgsqCkoKUgpSCgpSClNiygoKUgoKCkoKCgoKCgpSCgpSCgpSClAACHAANAoKSgpSClIKUgpSmgoKClILKgpSmgoK4lKaC7sKCgriUgpSCyoCC/oKmgoKUAAIS4oKCpoKCgpSmlIKClIKCgoKUAAgKoqaCgpSUgpSCgoKClIKClIKCgpSCgpSCgpSClKaCpoKmgqiSqJKCAAgIooKCioKCuIKCgoKCgoKmgoKCAAIQ8oKCgqiS")]
[assembly: go.GoPositionMap("runtime/netpoll_kqueue.go", "netpoll_kqueue.cs", "ABYsgoKCgpSCpoiygoKCgpTKlIKUgoKCgpSmptaCAAcqAAgCgpSCgoKCgpSClJSUgoKCuIKCuIKUlIKCgpSCgoKmlKaClAAKFoLGpIKCgqaClIKCgoKmgqY=")]
[assembly: go.GoPositionMap("runtime/netpoll_kqueue_event.go", "netpoll_kqueue_event.cs", "ABEegsqCgoKUuJSC6ILKgoKClJSUguiCgoKUgpSmgpS4gg==")]
[assembly: go.GoPositionMap("runtime/nonwindows_stub.go", "nonwindows_stub.cs", "ABAmyA==")]
[assembly: go.GoPositionMap("runtime/os_darwin.go", "os_darwin.cs", "ABcygoLosoKUgoCCpICC+pKCgpSCgpSUgoKCgoKUgoKCgpSCgoKCgqb8soCCpIKCgpQACiAACgKUlIKCgpSC3KiSgqiSgoKCgoKCzMiClKaCgoKCpoKCgqiSqJIABhKUgoKCgoKUppSCgoKCgpSokoKmgqzSgoKokoKCgoKokoKClKaCgqqigoaCgoK6AAIQAAgEkoKCgoIABxCCgpSCpoKCqqKCgoKCggADFOKqwoKCuLzGgpSCgqzWgpQAAhIACAiSqJIADyiigoKCgoKUpoKqspiigoKCgpSCgoKCqqKCgqyyqqKmggAGGJIACAyUgoKUpqaCAAkQkg==", "257-259:1")]
[assembly: go.GoPositionMap("runtime/os_nonopenbsd.go", "os_nonopenbsd.cs", "AAgWyg==")]
[assembly: go.GoPositionMap("runtime/os_unix.go", "os_unix.cs", "AA0ikg==")]
[assembly: go.GoPositionMap("runtime/os_unix_nonlinux.go", "os_unix_nonlinux.cs", "AAgawqyy")]
[assembly: go.GoPositionMap("runtime/panic.go", "panic.cs", "ACVwABIClLiCggADEvaCggAGQgAdAoKokoLcsoKokoKokoKokoKssoKokoKokoKkgoKkgoKkgoKokoKkgoKokoKkgoLYkoKooqSigpSUgpSmgoKCgpSUgpSUppKClJSClKaCgoKCgpSmlIKUlKaSgpSUgpSmgoKClIKClJSClJSmxpKCpsaSgqbEgoKmxIKCpsSCgqaCgqbEgoLaooKUpoKCgoK4ppKSksaSlLS0tKQAAowBAEMCgpSmgoKCuJSClKiS2qKCgpSCgoKCgpSCAAQQ0oKClIKUgoKCgqaClIKCgoKCpoIAAhIACAKClNyCgoIAChaCggACENKCgoKCgoKCgoKUlICCgoKkgpSUlIKokoLKgqaCpoKChKKCgoKCgoKUlJSCgoKmlJSCrLKClIKCgoKUAAhE4oKC6MS2gqaCgpSUtKS0qKKCgoKUgqaClIKCgoKClAACENKCgoKCgoKUgoKCABYogKKkAAcYAAoCgoKUuIKCgoKCpoKCgoKUgoKCgoKCgpSCgoKCpoKClJSCgoKClAAIEoLulIKs0tyClIKUgLiUgoK2poIACBKC7uKUgoKmgoK4goIACRSCgrimgpSmgoCCgoKCppSmlLaCzMKCpoKSgoCCtoKCgoKCAAcQgqaCpqaCgpSm1oKCgqaCpoKCgqaUgoKUqJKCgoIALVyCooKCgoKCgoKCgpS0gpS0goIACAyClIKokqiSqJKokqiSqJKokqiSqJIABn7qgqamgpQAFiwACQKCgsqCgpSmABMogoKCppSUgqaClLgAI0iCgriCgrjK6qQAAhDSgoKUgsqSgqaUuKamAAIQ0oKCgqaS7pSqooCCtqa4poKmAAIeAAwCgoLcuIKmtoKCgoKUgsiCgsiCgoK2ggAJFOKCgoKUlKaCgoKUgoKCgoKClIKCgoKmvpTKgqaUrsKCyoKCpoKClIKCgpSCgpSCAAIQ0twAChamlKa4AAIU8oKClA==", "703-713:1;1003-1038:1;1120-1146:1;1431-1446:1;1463-1484:1;1493-1495:2;1597-1599:1")]
[assembly: go.GoPositionMap("runtime/pinner.go", "pinner.cs", "ADLeAaKClILKguaCgoKClICCpJSUAAg+woKCgrjKgoKUlJSCgoKUgoKUppKCuLiCgpSSgpSUuLimgoIACBiypoKmgqaCqqKCgpSClAAGFsKCgqaCrLKssqaCrNKCgqaC3IKCgriCgoKUvKKCgoKCgpSCgoKClJTawoKClIKCgoKClIKCgpQABiKC", "202-205:1;216-219:2")]
[assembly: go.GoPositionMap("runtime/plugin.go", "plugin.cs", "ABEasoKCgoKUlIKUgpSCpoKCgqaIgoKCgoKCgoKCpoKCgsqClIKUgoKUAAkUgoKCgoKClIKClJTmooKCgoKmgsqCgoKCgpSCpoK6kg==", "", "", "46=inRange/2/4/1,inRange/3/4/2,inRange/4/4/3")]
[assembly: go.GoPositionMap("runtime/preempt.go", "preempt.cs", "AE/UAQAfAoC4uJKmgoKCgoKAAAgoAAoUgriUgvyC3IKCAA0c6oK4griCgqaCgoKClO7cgoKCAAbVAdymggAL2gGClIKUgv6ilKaCgKSCvMaUAAMS0gACHgANGAAMBoK4lIKU7riU7oKCgoKCgpSCpoK81AAHJgASAsqCuIK4ggAOHoK4gpSUAAgSlIK4lIAABxC2goIAECaUuIKUtpQ=", "320-333:1", "", "498=HasPrefix/2/3/1,HasPrefix/3/3/2")]
[assembly: go.GoPositionMap("runtime/preempt_nonwindows.go", "preempt_nonwindows.cs", "AAgUtg==")]
[assembly: go.GoPositionMap("runtime/preempt_xreg.go", "preempt_xreg.cs", "ADx8goIABRDSlKiSgpSCpoKCAAcQpgAFHAANAoKmgpSSpoKmgoKCgoI=", "122-124:1")]
[assembly: go.GoPositionMap("runtime/print.go", "print.cs", "ABtW8pSUgoKCgrgABRrygoKCgpSmgoKCgryigpSC3IKCpoKmgqaC5oKClO6CgtyCgtyCgtyCgqaKwoKmyoKCgpSCgoKClPqCgoKCgoKClJSCgoKClKaCpoKCgoKUgrSCtIKCtIK2goKCgoKCgpSCpoKmgqSCpoKmooKCpoKmgg==")]
[assembly: go.GoPositionMap("runtime/proc.go", "proc.cs", "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", "174-176:1;207-211:2;353-353:1;482-484:1;597-604:1;1279-1284:1;1297-1301:2;1317-1319:3;2112-2128:1;2310-2316:1;5277-5282:1;5298-5307:1;5534-5543:1;5547-5552:2;5981-5990:1;6302-6304:1;6309-6316:2;6407-6423:1;6977-6991:1;7580-7582:1")]
[assembly: go.GoPositionMap("runtime/profbuf.go", "profbuf.cs", "AHPwAYKmgqaCpoKmggACEOSokqiSrOKCgoKCgoKmgpSClKqigriUgoLcgpSC/qKAgv6ClIKUgpSUgoKCgoKqwoKmgriCgoKmlAACEPKCpoK4poKCpoKUgqaCpoKUAAfoAaKClIKssoKCyoKClIKUAA8cwoKm7oKCgoKCgoCCtqaCgoKCpoKUpoKCgoKClJSUyoLKlJSCpoKClIKClJSClIKCgriCgpSCgu6CgoKClIK4lAAJFKY=")]
[assembly: go.GoPositionMap("runtime/proflabel.go", "proflabel.cs", "AAwoABsmgpQAAhgACQI=")]
[assembly: go.GoPositionMap("runtime/race0.go", "race0.cs", "ABAmkKKAooCAooCigICigKKAooCigKKAooCigKKAooCigKKAooCigKKAooCigKKAooCigICigKKAgKKA")]
[assembly: go.GoPositionMap("runtime/rand.go", "rand.cs", "AClMooKCpoK4gqa4gqaClKaCgoKAgoKklIKUpoIAAhAACQqCgoKCgoKUgpSCuIKCgpTYkoKClIKAgoKkvKKCgpSCrLIAAhQACwqCgsqCgpSCgrqSqLKCgpSCggACEOQAAiwAEwK4hoKCAAcQgoKCgoIAAioAEgIAAiYAEQQAAhYACAKokqiS", "", "", "51=allZero/1/2/3,allZero/2/2/3")]
[assembly: go.GoPositionMap("runtime/rdebug.go", "rdebug.cs", "AAkUsoKCqLKCgoI=")]
[assembly: go.GoPositionMap("runtime/retry.go", "retry.cs", "AAgc0oKCgpSU")]
[assembly: go.GoPositionMap("runtime/runtime.go", "runtime.cs", "ABw+soKCggATVAAXBIKCuIKCgoKCuIKmlIKmlIKClKaU7pCmkKaQqKIADRKSgoKCqJKCgoIAChaigoKCgpSCgoKUgqaClKaCgoKAgqSClIK6koKCgoKCupKCgoIAAxDCAA047IKmABYugpQACR7Q")]
[assembly: go.GoPositionMap("runtime/runtime1.go", "runtime1.cs", "AB5MAA0CgoKCgoKCppSUAAUWsqaCgoKmgoKUgoK4uIKCpoKCuIIADw6CgoKClIKUgoKUgpSClIKClIKUgpSClIIALwiCAA4eAAAQlIKUgpSClIKUgpSClIKUgpSClIKUgpSClIKUgpSCpoKCgpSCpoKClIKmgoKUgqaCgoKmgoKCpoKClIKmgoKUgqaCgpSCpoKClIKmlIKmggBLnAEAJEqUgoKCAAgSlKaClIKCgsqmlIIADh6C6IKCgpSCqqKUlJSCggAHHgAKAoKClIKClLiCgpSClKaCgpSSgpSC3IKAgraCgoCCgoKCAAgQgrqSgqSkpKSktIKCgpS0goCC2oKmlKyygoKokoKClAADJgASAoKCgoKClAAEHAALAgACIgAOAgACHAALAqyyrLKssoKCgoKUgoKCgoKUgqiSqJI=")]
[assembly: go.GoPositionMap("runtime/runtime_boring.go", "runtime_boring.cs", "AAkUpIKU")]
[assembly: go.GoPositionMap("runtime/runtime_noclearenv.go", "runtime_noclearenv.cs", "AAoYtoI=")]
[assembly: go.GoPositionMap("runtime/rwmutex.go", "rwmutex.cs", "ABx4AB4ClILM+oKUlIKClIK4goKCgoIACA6ygIKCppSCgoKUtqikgpSUgpSSgoKCpurEgoKmgoKCgoKCuIKU", "80-96:1;131-136:1")]
[assembly: go.GoPositionMap("runtime/secret_asm.go", "secret_asm.cs", "AAgS")]
[assembly: go.GoPositionMap("runtime/secret_nosecret.go", "secret_nosecret.cs", "AAocsKa2trSmkKQ=")]
[assembly: go.GoPositionMap("runtime/security_issetugid.go", "security_issetugid.cs", "AAwagqaC")]
[assembly: go.GoPositionMap("runtime/security_unix.go", "security_unix.cs", "AAsagpSCAAgS5oKCgoKCpoI=")]
[assembly: go.GoPositionMap("runtime/select.go", "select.cs", "ABk0kpaCpoKCgoKCgsoACBKCgoKUuPy4AAYWwoLulJSClKaCAAgkABECgoLKgpSCgoKssoKClJKClKaCggALGIKCgqaCgqaCgqamgqaCgoKUgpSCpsqClIKCgoKUlIKCgoKCgoKClIKUgoKClJSmgoKCgtyUAAcWkoKCgoKCgoKUgoKClIKUgqaClIKUgoKUgsqCgoK4gpSCgoKCgoKCpoKCgpSUgpSClKaCysqCgpSUgoLcgoKClIKCgpSUgoKClJSCgoKCpoKClKaCgoKmgqaUgqaCgqamgoKCgqaCgoKCpoKCgoK4gpSEkoKUlIKUgpSCgoKUgoKClIKClISSgpSClIKUgoKClIKClISwkoKUgpSEkoKClIKUlISSlIKUgpSgkoKUlIKClJSEkqaiABMukoKUgoKSgoKClIK0grSCtoK4griCgqaCgoKCgpSmpoKUlKaigoKClIKCgoKmgoKClJSCgoLKgoI=", "147-152:1;497-497:2;527-527:3")]
[assembly: go.GoPositionMap("runtime/sema.go", "sema.cs", "ADtwggACGgAKAqiSAAIaAAoCqJKokqiSqJKCgpSokqiSpoKClAAHFpIABHiCAASuAYKCgoKUgszCgqaCgoKUgoKClJSCgoKCgoKClIKmgoKClIKClIKCgqaClJSCgoKmlIKClAAOHoKCpoKClIKUAAQYAAsCgpSCgqaClKaUgoKClICUgoKCgoKUgoKUgpSUgoLKgoKCgqaCgpS4goKUppS2goKCgoKC2tSCgpSCgoKCpoKCgoKUgpTs1IKClIKCgoKmgoKCgpSClAAYOqIAAhD2ruKmgoK4gqaCgoKCgoKClIKUlIKCgriCgtz2gsqCgoLcgqaCgoKCgpSCvvaCpqaCgoK4AA4egoKCgpSUgpSCgoKClIKm2JKCgrqS")]
[assembly: go.GoPositionMap("runtime/set_vma_name_stub.go", "set_vma_name_stub.cs", "AAoYtIA=")]
[assembly: go.GoPositionMap("runtime/signal_amd64.go", "signal_amd64.cs", "AA0eooKCgoKCgoKCgoKCgoKCgoKCgoKCqqCkgKKAooCigKbogoKCgoKClIK4gsqCpriUgoKCgg==")]
[assembly: go.GoPositionMap("runtime/signal_darwin.go", "signal_darwin.cs", "/g==")]
[assembly: go.GoPositionMap("runtime/signal_darwin_amd64.go", "signal_darwin_amd64.cs", "AA4goKSAooCigKKAooCigKKAooCigKKAooCigKKAooCigKKAqKCkgKKAooCigKKAooCkgKKAooCigKaSAAkUpoKUABYsgoI=")]
[assembly: go.GoPositionMap("runtime/signal_unix.go", "signal_unix.cs", "ABs4kqaCgpQAPH6mgoIABRbSlMqCpoKCgsqUpoKCgpSmgrzYlILIuKaCgsqCpgAIqAHygoIABBLClIIADRyClIKmgoKCgpTugoKCAAUSsqaCgpSo5oKAlNqClIIABRgACAaCpoKCAAcQpoIAAxLypO6CgoKClLYAAhwACwKClIKCgoKmgpSUAAcQgpSUyoKUlIKUppaSgoKmgqaClIKmgoIACywADAKCpoKCgpS4AAIS4oK4AAMWAAoCgoKmgoKCgoKmAAoWgoKCyoK2lpKCgoKUlAASIpKokgACHAAPBoKCAAsYlKaClKaCpoKmyoKmlNyCgpTulKaUAAgSgoKClIKmgoK4gqaCpoLugqaClIKmlIKCgoKUgoKCgpSmgoKUgoKUlAAMGoKUuIKCgoKUgJSCyIKCgoKCpoKmlKaygpSmgqaCgpSCgqaUisLugoKUgoKClJSCAAcoABECgoKmlIKmgpSCtIKmgpS4lJS0lLSktpSUAAIS4pSC7oKCpoKUgoKmrNKUpoKCgpSCuIIAChaCAAsYgqbKAAcQqJKqooKUgoKCxoLKgoKCpoKCtILGgraCAAoQsoLcsoKCggAFENKCAAUQ0riCgpSCppSoAAIS8oKUgqaUgriCgoKmgtyCuIKmyoLugoK4gqYAAhYACAIAAhYACgLqgsoADRyCgqYAAhoACgKCgpQAAhTygoKqooIAAhoACgKCgoKCgpSCAAMYAAgCgoKCpq7CgoIABxAAAxoACQKCgpSClIKUAAsoAAoCgoKCgoKUgoKCggACEPKCgoKCrNKCggACEuKCgoKC", "1074-1104:1", "", "489=FuncPCABIInternal/1/1/2")]
[assembly: go.GoPositionMap("runtime/sigqueue.go", "sigqueue.cs", "AESOAbKCgqamgIKCyIKCgoKUgsqCgpS0gti0goKClIL6gt7ClIKCgsqCgpS0goKClIKCxoIACAyCAAQcAA4MgtyCvrKUgoKUuIKmgoKUgoKUrLKClJSCgqyygpSUgoKUgoIAAhDSgoKssoI=")]
[assembly: go.GoPositionMap("runtime/slice.go", "slice.cs", "ACA6gtaCqsKSgoKCgpTKgqaCgoKCuILuuIKCgpSClIKmlAACGAALAoLcgoKUpqaCgoKmgoKmAAVeAC4CgoKClIKUgqaCpqamlILqgpSCgoKCtIKCgoK0gpSUlIKCgoKCtIKCgoKCABAggqaCgsqmgu6mlAACIgAQAoKCgAALGIK2qJKCgoKmgoKUuNyC3IKUAAIYAA0GgsqCgoKUgqaCqJKCpoKCpoKmgoKCgoKUgoKUgoKmlJSUqJKClIKqwoKClJSCgoKCqqKCgpSUgoKCrNKCgpSUgoKCgoKCqJKCgpSUgoKCgoKUqsKUlIKmAAsY7oKmqsKCAAgSgpQ=")]
[assembly: go.GoPositionMap("runtime/softfloat64.go", "softfloat64.cs", "ABo64oKClJSCgpSCuIKCgoLsgqSm4oKClJSCgpSCuIKCgoLsgqSmgqKClIKClIKCgpSCgoKCgqaClIKUgoKmgoKCgpSClIKCgqamgqKClIKClIKCgpSCgoKCgqaClIKUgoKmgoKCgpSClIKCgqamgoKmlKampqamgpSmqIKmgoKCgoKClIKCpoKUpoKmgqaCgqaUpqampriCgoKCpoKCppSmpqamlNaCgqaCgoKUgoKUgqaCgoKClIKClKaCgpSUpqam3Ka4pqKUlKamgpSClKaCgpSCgpSCgpSmgoKCgpSkgoKCgsqCgoKCgqaqopiCgoKCgoKCgoKqopSCuIKCgqaCgoKCgoKClIKCgoKCuIKClIKCgoKCuKaCpoKmlKaCgqaCgqaCgqaCgqaCgqaCgqaCpoKmgqaCpoKCpoKCpoKCpoKCpoKCgpSCgqaCgoKUgoKmgoKmgoKCgqaCgqaCgoKC")]
[assembly: go.GoPositionMap("runtime/stack.go", "stack.cs", "AKoB1AKCgpSCgpSCgrqSgpT6ooKCgpSCgpSClIKUgoKCgsqUgpSUgoKUgoKUlNiSgoKUlJSCgoIADyCCgoIAAxDCgqqigoKCgoKClIKCqJKClIKCgoKCgoKUgoKokoKUgoKCgoKClIKCAAkUAAkIgoKUgpSCpoKCgoKUrLKCgoKCgpSCyoKClIKCgoKUgpS4lJSCgqaCgoKUlJSUgoKUgpSmgoKCgqaClIKUgpSClIKUAAcS4oKCgoKUgpSCgpSCgpSUlIKCgoKmgpSClIKUgoKCgoKUgoKCppSClIKClLiUgoKmgoKClKaC7oKCgu6UACtgooKCgpQABxCUgoKCAAsgwoLawoKCgtyCgoKCpoKCgoKCgoKmgoKUgoKUgoKCpgAJEMSCgpSmgoKCgu6UABgypriUgoKmxoKCuIKClMqCgoKCgoKUgriUgoKCAAkOooKClIKCgoKCpoKCuIKCytiCgoKCuKampoKCuIKCuIKCgoKCpqyygqiSggAJFJS4mrKCgoKCuIKCgpSm+sKClIKClMqmgoKUgqiSgqaCgsqUAAgSuLjKgoKCgriCgoKWkoKmgoKUytyUlIKUqJKCgpQADR4ADAKUgpSCgoKCpoLcppSCgoKSgoKClMqCgqaCgoKCygANHIKCpoK4gpSClJSCuIKCgqaCgpSCpqaCuJSCuLiCyoCCgoKCgsi4poKClJSCyriCgpSCqJKqwoKClJSuAAoKgsqCyoIADBqClAAEdqSCgoKCgoKCgoKUlLiCgoKCgoKCpgAOJNKCgpSCgqaCgtyC3sIACBKCggABGAAKAoKCgpSClIKClKaClIKm")]
[assembly: go.GoPositionMap("runtime/stkframe.go", "stkframe.cs", "AESQAZKCuIIABR4ADwKCgoKm2pSCppQACxiCgpSUgriCgoKUgqaCgsjq4oKUpoKCyoKUuLiCgpS0pIKCgoKCpoKUgpSCgpSCupKCpoKCgpSUgpSClMrMlIKCgoKCAAgSAAgKgqKmgoKCgoKmgpQ=", "", "", "302=alignUp/1/1/1,getGCMask/1/1/4")]
[assembly: go.GoPositionMap("runtime/string.go", "string.cs", "ABs40oKCgoKCgpSClIKClILcgpSCgoKUrLKmgqaCpoKs0oKCgoKUlJSmgoKClJSCgoKmrLKmgqaCpoIAAhIACAK4lILKgpSClIKCgpSmgoKUlIKqooKCpsKCgpSUAAIiAA4CgsqClIKUpqKCgoKUlIKmxoKCpoKCgpSmgoKClKaCgsqClIKUgoKClIKClIKUlAANHIKmsoKCgpSUgpSCAAQasoKCgqaCqLKClIKCgqaC2LKCpoKmgpSCrLKCgpSCgqyypoKClIKCAAIgAA4EgqaCgoKClMqCpoCUgoKUgoLIgpSClLS0tMaUgoKUgoKUgpSUgpSUqJKC3IKCgoKUrtSCuJSClICCtoKCuIKClIKCgpSokoKCpoKCgoKClIKClIKUlII=", "", "", "156=GetCallerPC/1/1/2,FuncPCABIInternal/1/1/3;223=GetCallerPC/1/1/2,FuncPCABIInternal/1/1/3;279=GetCallerPC/1/1/2,FuncPCABIInternal/1/1/3")]
[assembly: go.GoPositionMap("runtime/stubs.go", "stubs.cs", "AAswAAwCrAAEIAAPKAAWCKIAAkgAIgaSAAJCAB8GkgAFJAANLAAUAoKuwoIAAhgACgSUkpIAAVwALRwADAKClKgABRwACyIAEAakAAEYAAsEmsKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSlJqyrLKs1qikltYAAhDmAAIWAAoEkpKSkpKSkpas")]
[assembly: go.GoPositionMap("runtime/stubs_amd64.go", "stubs_amd64.cs", "AAkUopKSkpKSlqamwpKSkpKSkpKSkpKSkpKWpqzSmA==")]
[assembly: go.GoPositionMap("runtime/stubs_nonlinux.go", "stubs_nonlinux.cs", "AAgUkg==")]
[assembly: go.GoPositionMap("runtime/stubs_nonwasm.go", "stubs_nonwasm.cs", "AAgUkA==")]
[assembly: go.GoPositionMap("runtime/symtab.go", "symtab.cs", "AEWgAbKCggAEhgIACwIAAiAADQKClIKCAAIgAA8GgpSClIKUpoKU3KaUgoKmlKYAAhDSgpSUpoKCAAkUgpTugpQADRyCpoKm2IKCgoKUgoKCpgB8pAIACgKCgpQAAioAEgKCgoKUgoKCgoKCAAwagoKCgrgAGzqCggAMIAANBIKElLiClIKCgoKClIKClIKUuIKCgoKmlIKCpoKCggAHLgASAoKCgpSCgqaCgqamlAAFEuKCppSCgoKUgpSUlIKUgoK4qJKClAAVsAGigoKClAACEuKCgqYABxCCpoKokgACGAAJAqgAAiIADwKCgpSUgoKmgqaUgpSCpoKmggAMGoKClAACFPKClKgADyjC+Ja2pIKssoKCgoLKgoLcgoKCgoKUgoK4ppSCpoKCgpSUgoKCgoKCgoKClO6CgoKmgoKCgoKCgu6CppTKgqaUgoKCgoKCgpSmgqaCgpSmgoKCgoKmgoKmpoKCgqaAgrYAAhgADAKCgpSCgpSUgqaC1oKCgoKUqJKCgoKCgoKCgoKUuIKmgoKUgqaCgpSCqJKClKqigpSCgoaigpSCgqi2goKUgoKUgpSCgoKUgoKokqKCgoKCgpSUAAsU6IKU", "", "", "189=gostring/1/2/3,gostring/2/2/4")]
[assembly: go.GoPositionMap("runtime/symtabinl.go", "symtabinl.cs", "ADKGAQAQAoKClIKCtoIABxCCqJKCgpSCqJIAAhwACwKClIIABxIAAhAACAKC", "", "", "79=pcdatavalue1/1/1/4")]
[assembly: go.GoPositionMap("runtime/synctest.go", "synctest.cs", "ACtWAA0SgoKUgrSCtoKUgoKCxoLagqaCgoKClIKmuIKUgpSCgoK8woKC2LKCgoKUgoKCurKCAAoWgoKUlICUyKbqAAsM0oKmgoKU3IKCgpSCkriCooKCgoKmgoKCgsaCgoKUgoKClIKClIKUlJSmgoKmlIKCgpSUlKa0/oKmooKClJSUgqiSgoKUuIKClIKClIKCgpSCgriC6KKClJSCgqiSqJKAgoKkqJKosoKClIKSlKIADjAACgKCptyClJSmgqaCgpSUpoKCgoKCgoKCgqamgpSuwqyyrLI=", "192-194:1;198-204:2;210-217:3;353-355:1", "", "216=Add/1/1/1")]
[assembly: go.GoPositionMap("runtime/sys_darwin.go", "sys_darwin.cs", "AA4aAAswAAoCgoKCAAIWAAsCuIKUgoCClIKCpKb84oqCgoKk3vKCgqTowoKCgqTowoKCpPjCgoKCpNjCpJjCgqTowoKkAAFGACECgpSkAAMmAAMQ+MKCpOjCgqQAAxK+2MKk3vKk2MKkmMIABBT40oKCpAADLAABGgALAoKCpAADEgADEujSypSCgqTYwqT4woKCpAAKCMKCgoKCgqQACQjCgoKCgoKk6KKIgqSYooKkAAoIwoKCgoKk6MKCgoKk2MKCgqTYwoKCpOjCgoKCpOjCgoKCpPjCgoKCgqTYwoKCpOjUgqTGkqiSgoK4gqQACg4ABRIACQKCAAcepAAIBpIABRak")]
[assembly: go.GoPositionMap("runtime/sys_nonppc64x.go", "sys_nonppc64x.cs", "AAgS")]
[assembly: go.GoPositionMap("runtime/sys_x86.go", "sys_x86.cs", "AAwgooKCgoKC")]
[assembly: go.GoPositionMap("runtime/tagptr_64bit.go", "tagptr_64bit.cs", "ADx+ooKCgpSokqaUgpSokg==")]
[assembly: go.GoPositionMap("runtime/test_amd64.go", "test_amd64.cs", "/g==")]
[assembly: go.GoPositionMap("runtime/time.go", "time.cs", "AA8iwoCCgu6kqJKCgpSokgBGogHigoIAIVKSlKTauIKmAAkQlAAaOoKCuKKClIKCgqaClKaCguqSgtiypoKqwoLKAAUUAAkCgoKCgpSClIKCgqaUgoKCgqYACSTygqaCgoKCgoKUlIKAgpSkgoKUgriClAADEOKCAA8g4oKCgoKUgoKCgoKmgIKkgoLe4oKUAAUQ8oKUgpSssgAFEAAIAqaCpoKUgoKCggADEvKCAAcQgIKClAAIEuKCgqaCgoKUgoKCgqaClKbugqaCgoKCuNrCgoKClIKCgpSCgoKUgpQACRDigpSClJSCpoKClIKCgoKCgqaCgoKCgqaCuICCpoLIlAAKFoKClIKSgpSUuKbugqaCgoKUpoKUgqbqwoKCgpSUAAYqAB0UgoKCgoKUlJSCgoKCgoKCuJSCgoKClIKCgoK+su7igoKCgoLugu6CgIKCgoKCgoKUgraCgqaUpoKCgpQABxQACAKCgriCgoKClIKmgoKCgoIABhLygtyCgoKClAA1bIKUgoKCgoKmlKaClLaCgoKCgoKCgraCgqSmgpSUggADFAALDIKCgpQABRoADgKmgpSmgtyCgpSUlKaCgoKUgIKClKQABxCCgqaUAAUWAAoCgoKCgpSCgoKmlKaCgoKmgqaUgqaCggAJEgAJAoKCgpS4goKClJSClKaCpoKCgoKClIKCppSCgoKCgoKUpoKUgriUlIKClIKUuIKmlIKClIKmABImlLiCuILIlIKmgoKCppSmgqaCgv7SgoKUuIKCgqaAgoLMwoKClLyigoKClIIABBCypoKCpqaAgraUAAQeAAoCgoKUgoKUgoKClIKUgryigoKClIKUgoKUgoKClIKCgoKCpoKUgpSCvMaClIIABhDCAAcQAAgClJSmpoKCgpSClIKUgoIABhDCgpSmgoKCpsqCggAIEoKCggAGFOKCgoKClIK4gpQ=", "485-487:1;638-643:1;1239-1239:1;1465-1467:1")]
[assembly: go.GoPositionMap("runtime/time_nofake.go", "time_nofake.cs", "ABBAAA8CABAosoKU")]
[assembly: go.GoPositionMap("runtime/timestub.go", "timestub.cs", "AAw0AA8Cgg==")]
[assembly: go.GoPositionMap("runtime/tls_stub.go", "tls_stub.cs", "AAgU")]
[assembly: go.GoPositionMap("runtime/trace.go", "trace.cs", "AL8CrAcACQLKgpSCmAAAFO6UgqaCgqYAChaCgoKClIKCgpSClIK4lMoACBLcgoKClAAJFILcpoKCyoKCgrimAAkUgoKCgu6CgoLuggASJoKCgpSCgpQAARDilIKUgoKClIKC3IKCgoKmpoKCpoKmpoKCgoKmAAkUgoKU3IKUpu6CgoKmgoKCppSm7oKCyoKCyoKCgpSCgpSCpoKCgqaClKYACxiCgoKCgpQABxCUuIK4griUrLKClIKUggAIWgAIApSClKaA7JS4goKmgIKCgraCgoLKgpSCgoK4gu6UgoKCuKaC3LiUuO6C3JSUpoIAChaCgqaCgoIABhLigoKUrAAPGoKUAAsYxIKCgpSmlLqSgoKCABIqkoKCgoKClIIAAhDygoKClIKClIKCrPaCgpTcAAgKgpQAAhQACgSCgpSCppSClII=", "494-540:1;508-536:1.1;564-570:2;591-604:3;675-725:4;761-765:5;793-816:6;931-931:1;1102-1111:1;1145-1147:1")]
[assembly: go.GoPositionMap("runtime/traceallocfree.go", "traceallocfree.cs", "ABc0wpiigoKUuIKCgqamgoK4goKmpoK4goKCgpTKkpSmooKUqLKosqiSqLKssqyyqJKokqiSgqiSgqiSqJLYkoKU", "79-81:1")]
[assembly: go.GoPositionMap("runtime/traceback.go", "traceback.cs", "AG/2AQAKDubEgAANHLaCgoKCgqaCgoLKgoKCgsqCgoKUggAHENyCpoKCgoKUgpSClKYABxCCpoIABTAAFwKClIKmgriCypS4ytyC7oKCgoKCgoKC2AAHEIKUgoKCgraClMqUggAZNIKClJSCgoKCpoKCyoKUABMogqYAChaCgoIAChbKguaigoKmgoKUgsqCgsqUgoKUgpSCgqaUgoKmgoKUuIKCgoKCuIKUgoKCgoKCuNyyAClUgoKCggADHAAKApSmrsKmpoKCgoKCpgACGAALAoKCgoKmgoKUggAHEIKUuIKmqJKCgqaCgoKCpoKClIKUgqaSlIKCgpSmgoK4goKCpoKCgoKCgpS0goKCtLSCtIK0goKCgoK23sSCgpSCgpSClKiSgqqigoKUgqa0goKCuIKCgoKUgoKClIKCgpSmggACFgAKApSClKa07oKCgpSmlIKClKaCggAuXoKCgoKCgpTKgoKCgoKCppS4gpSUgpSCAAMSAAsGgpSUgoKUgqaCpoKCgoKCgoKCgoKCpoCCgoK2gsqCgoKUgpSCgoKClIKmuICCgoKCgoKAgoKCtoKCgriUgpSCyKqigoKCgqaCpoKmAAMQwoKCgoKCgpQABDSCgoKqooKClKqigpSmggAOHoKm7oKmqrSCgpS4goKUgoKUgsqqxqYADxqilJSClpKClLiCqJKClIKCgoKUpoKClIKUgKykgpSClICCpIKCgoKCgoKmpqaCpoKmgoKCggAJFJKClICCpIKUgpSCAA0cgoKUAAQQ0oKUlIKUgqaUgpSCpoKUgriCgoKClIKClIKCAAQcAAwEgoKUgpSmppSUpqaUlAAIyAIAoQECgqaGpoKClIIABBQAChCmpqYAGjiSgoKClJSmgJKCgoKUlIIAAhDygoKAgoKCtoK4lJSCgpSCggAEELKCppSClIKUrLKCppTKgpSClA==", "675-684:1;686-701:2;704-708:3;906-929:1;953-968:1;1296-1296:1;1317-1351:1;1388-1401:1;1688-1688:1", "", "218=guintptr/1/1/2;1814=noescape/1/1/2")]
[assembly: go.GoPositionMap("runtime/tracebuf.go", "tracebuf.cs", "ACBWAAsClIKCgqYAAhgACQIAAhIADAqmgoKUgqaCgoKUAAIS4qaUgqaCggAEGgAIAoKClAACEuKCgoKUlILokoKCgpSCgoKUgoKCyoKClIKCpoKCuIKUgpSCgoKCqqIAAhwACwIABxKygoKUlKiSgoKUgoKUgqaCABM04oIAAhLigoKCgoKClIKUAAIWAAgCgoIAAhLirsIABRgACQKCgpSUgpSCAAMS8gAJFIK4gg==", "144-150:1;157-173:1")]
[assembly: go.GoPositionMap("runtime/tracecpu.go", "tracecpu.cs", "ABIcsoK4ggAHEILewoKmgoKSAAoWlIKUlIKCgqaU3LKCAAkUgoKCgqamgoKCAAIgAA0ClIKCgpSClIK4goKCgIKkgoK4psqUpriCgoK4lpKClLimgoKCgoKUlKq0gIKSgoKCAAUQ8qaUpgAHEIKCgpSCpoKUporCppSUgpSmlKaAuLamgg==", "44-70:1;201-206:1")]
[assembly: go.GoPositionMap("runtime/traceevent.go", "traceevent.cs", "ABxQAA0CgIKkgIKkrLKosqyyAAIQ5AAFErKssqiS", "", "", "84=startPCForTrace/1/1/2")]
[assembly: go.GoPositionMap("runtime/tracemap.go", "tracemap.cs", "ADyAAaIAAhLigpSUgoKCgoIADyCClILKlIKCpoIABTLCgoI=")]
[assembly: go.GoPositionMap("runtime/traceregion.go", "traceregion.cs", "AC5WkoKClIK4goKCgrqSpriCgoKCgoK4griCgsqCpoKUruKCgoKClICCgqQ=", "63-98:1")]
[assembly: go.GoPositionMap("runtime/traceruntime.go", "traceruntime.cs", "ABMqkgAdRraCgoKCgoKCggACEuIAHUAAHUAABRKyrsKokgARLtKClAACFAAKCKbKgoIACBIACRSCgoKClIIAAhDSAAIQ0oKUlKyyqJKokqyyuKi2rsKmrsKmrsKmqLaotgAFFAAJBoKClAACFgAIAoKCgoKUAAYS0oKClIKClKiSqJKosoKCgpSssoKCgoKCvrKokqiSqJKuwqjEgqrUgoKCgpSqwri+xIKCAAIYAAkCgoKCgpSUrOSC7qYACBKCgoKClKiSqJKmlJQAAhIACgbKrtSAguwAAhLigpSUgqyygpSUggACEuKClJSClLS0lIKssoKUlIIABRLiAAgSgpSCgoKCgqaCuJQ=", "699-712:1")]
[assembly: go.GoPositionMap("runtime/tracestack.go", "tracestack.cs", "ABtAAAoClpKCgriClMq0gpS0gsqmlILugoKCgriCgoIAECKCgpSC7oKUlJTKgpSClIIACRjGgpSCrLKCgoCCpIKmoriCuJ7SgoK4goKCgoKCgriCgoKUlKqigoKCgoKCAAsakoKUgoKClIKCgoKUgqqiruKUlJSokgACGAAJAoKCpqb+goKCgpSogoKCgqaAgqSmgoKClICCpMassoKClIKClA==", "306-314:1")]
[assembly: go.GoPositionMap("runtime/tracestatus.go", "tracestatus.cs", "ABAo9IKCuIKUuIKUAAUYAAsCgpSClIKmxtyCxqSCAAUYAAoEgoK4poKUAAYWAAgEkpS0tAAKEoKCxrSkABY84oKUgqiSgoKokqqiqJKC")]
[assembly: go.GoPositionMap("runtime/tracestring.go", "tracestring.cs", "ABo2xIKClIKmqJKCpoKClAACEAAIBIKmgpaSgpS4goKCpoKu4oKCgoKUuA==", "33-35:1;47-49:1;92-96:1")]
[assembly: go.GoPositionMap("runtime/tracetime.go", "tracetime.cs", "AC50AAsCgpQABCKCpqamgqaCgoKUgoKCgoKCppKCgg==", "107-111:1")]
[assembly: go.GoPositionMap("runtime/tracetype.go", "tracetype.cs", "ABc04oKmkqyygoCCpIKmooKmntKClLiCgoKCgqaCgoKUlA==")]
[assembly: go.GoPositionMap("runtime/type.go", "type.cs", "AB0ikgAJLIKmgoKUgoKCgpS0pJSuwoCCpJSCpIKUAAIQ8pSUAAcUAAsOlIKmgoKU2oLGgriCgoK4ggALHNS4gqaUgoKClIKCgqaCpoKCgoKUgqaC6gAMEoKClJSCuJSCpoKUgoKCxoKCgoKClLiClJSUgoLGABowgoKCuIKClOaCgpSCgoKCgoKUyoKCgoKCgpSUpoLmgqaUgoKCgoKmgoKCgoKCgpSUlICCpIKCgpSmgtaCppSCgoKCgqaCgoKCgoKClJSUggACKoKClIKCgoKUgoKCqqKClJSCgpSCgoKClKaCgoKmxriCgoKCgoKCgoKmuAAIEoIABR4ADwKCgILslIKUgoKUgoKUgoKCgpSCgoKmgpSUpIKCpIKCpIKCgpSCgoKmgoKCpqSCgoKUgpSCgqaCgoKUgpSCgoKmpIKCpIKCpIKCpIKCgpSClIKCgoKUgpSClIKUgqakgoI=", "135-137:1")]
[assembly: go.GoPositionMap("runtime/unsafe.go", "unsafe.cs", "AAwagoKmgoKUupKCgpTWgriC6ILWgqiSgqaCgriCgoKUupKCgpTWgriCuKbYkoKmptiSgqiS")]
[assembly: go.GoPositionMap("runtime/utf8.go", "utf8.cs", "ACRWkoKClAACGAAMApSCppSmgoKCguqCgoKC6oKCgoLaqrSApIKkgoKCpIKkgoKCgqSCgoKCgg==")]
[assembly: go.GoPositionMap("runtime/valgrind0.go", "valgrind0.cs", "AA0egKKioqKioqKioqI=")]
[assembly: go.GoPositionMap("runtime/vdso_in_none.go", "vdso_in_none.cs", "AAgWkg==")]
[assembly: go.GoPositionMap("runtime/vgetrandom_unsupported.go", "vgetrandom_unsupported.cs", "AAoYkqak")]
[assembly: go.GoPositionMap("runtime/write_err.go", "write_err.cs", "AAgUkoI=")]
// </GoSourcePositionMaps>

// Dynamically imported C entry points are recorded here, one `GoCgoImportDynamic` attribute
// per `//go:cgo_import_dynamic` pragma this package binds to a trampoline declaration, so
// that `abi.FuncPCABI0` of that trampoline resolves to the REAL address of the exported
// symbol rather than to a token. The value is dereferenced by design - the trampoline's
// caller jumps to it - which is why a stub carrying no record here is left a loud throw
// instead: an address that is merely plausible is fatal at the first call.

// <CgoDynamicImports>
[assembly: go.GoCgoImportDynamic("arc4random_buf_trampoline", "arc4random_buf", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("close_trampoline", "close", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("exit_trampoline", "_exit", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("fcntl_trampoline", "fcntl", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("issetugid_trampoline", "issetugid", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("kevent_trampoline", "kevent", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("kqueue_trampoline", "kqueue", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_error_trampoline", "__error", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("mach_vm_region_trampoline", "mach_vm_region", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("madvise_trampoline", "madvise", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("mlock_trampoline", "mlock", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("mmap_trampoline", "mmap", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("munmap_trampoline", "munmap", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("open_trampoline", "open", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("pipe_trampoline", "pipe", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("proc_regionfilename_trampoline", "proc_regionfilename", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("pthread_attr_getstacksize_trampoline", "pthread_attr_getstacksize", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("pthread_attr_init_trampoline", "pthread_attr_init", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("pthread_attr_setdetachstate_trampoline", "pthread_attr_setdetachstate", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("pthread_cond_init_trampoline", "pthread_cond_init", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("pthread_cond_signal_trampoline", "pthread_cond_signal", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("pthread_cond_timedwait_relative_np_trampoline", "pthread_cond_timedwait_relative_np", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("pthread_cond_wait_trampoline", "pthread_cond_wait", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("pthread_create_trampoline", "pthread_create", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("pthread_kill_trampoline", "pthread_kill", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("pthread_mutex_init_trampoline", "pthread_mutex_init", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("pthread_mutex_lock_trampoline", "pthread_mutex_lock", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("pthread_mutex_unlock_trampoline", "pthread_mutex_unlock", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("pthread_self_trampoline", "pthread_self", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("raise_trampoline", "raise", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("read_trampoline", "read", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("setitimer_trampoline", "setitimer", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("sigaction_trampoline", "sigaction", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("sigaltstack_trampoline", "sigaltstack", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("sysctl_trampoline", "sysctl", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("sysctlbyname_trampoline", "sysctlbyname", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("usleep_trampoline", "usleep", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("write_trampoline", "write", "/usr/lib/libSystem.B.dylib")]
// </CgoDynamicImports>

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
    internal partial class machVMRegionInfo {}
    internal partial interface floaty<ΔT> {}
    internal partial interface ifaceHash_i {}
    internal partial interface stringer {}
    [GoLocalName("cgoSetTracebackFunctionsArg")] internal partial struct SetCgoTraceback_cgoSetTracebackFunctionsArg {}
    internal partial struct _typePair {}
    internal partial struct activeSweep {}
    internal partial struct addrRange {}
    internal partial struct addrRanges {}
    internal partial struct adjustinfo {}
    [GoLocalName("pollDescPadded")] [GoValueClone("pad")] internal partial struct alloc_pollDescPadded {}
    internal partial struct arc4random_buf_args {}
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
    internal partial struct bucket {}
    internal partial struct bucketType {}
    internal partial struct buckhashArray {}
    internal partial struct cgoContextArg {}
    internal partial struct cgoErrorMsg {}
    internal partial struct cgoSymbolizerArg {}
    internal partial struct cgoTracebackArg {}
    internal partial struct cgothreadstart {}
    [GoLocalName("report")] internal partial struct checkFinalizersAndCleanups_report {}
    [GoLocalName("x1t")] internal partial struct check_x1t {}
    [GoLocalName("y1t")] internal partial struct check_y1t {}
    [GoValueClone("b")] internal partial struct checkmarksMap {}
    internal partial struct childInfo {}
    internal partial struct chunkIdx {}
    internal partial struct closefd_args {}
    [GoValueClone("stats")] internal partial struct consistentHeapStats {}
    internal partial struct coro {}
    [GoValueClone("extra")] internal partial struct cpuProfile {}
    internal partial struct cpuStats {}
    internal partial struct cpuStatsAggregate {}
    internal partial struct crypto_x509_syscall_args {}
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
    internal partial struct exceptionstate32 {}
    internal partial struct exceptionstate64 {}
    internal partial struct exit_args {}
    internal partial struct fcntl_args {}
    internal partial struct finalStatsAggregate {}
    internal partial struct find_firstFree {}
    [GoValueClone("subbuckets")] internal partial struct findfuncbucket {}
    internal partial struct fixalloc {}
    [GoValueClone("fpu_reserved", "fpu_fcw", "fpu_fsw", "fpu_stmm0", "fpu_stmm1", "fpu_stmm2", "fpu_stmm3", "fpu_stmm4", "fpu_stmm5", "fpu_stmm6", "fpu_stmm7", "fpu_xmm0", "fpu_xmm1", "fpu_xmm2", "fpu_xmm3", "fpu_xmm4", "fpu_xmm5", "fpu_xmm6", "fpu_xmm7", "fpu_rsrv4")] internal partial struct floatstate32 {}
    [GoValueClone("fpu_reserved", "fpu_fcw", "fpu_fsw", "fpu_stmm0", "fpu_stmm1", "fpu_stmm2", "fpu_stmm3", "fpu_stmm4", "fpu_stmm5", "fpu_stmm6", "fpu_stmm7", "fpu_xmm0", "fpu_xmm1", "fpu_xmm2", "fpu_xmm3", "fpu_xmm4", "fpu_xmm5", "fpu_xmm6", "fpu_xmm7", "fpu_xmm8", "fpu_xmm9", "fpu_xmm10", "fpu_xmm11", "fpu_xmm12", "fpu_xmm13", "fpu_xmm14", "fpu_xmm15", "fpu_rsrv4")] internal partial struct floatstate64 {}
    [GoValueClone("pad_cgo_0")] internal partial struct fpcontrol {}
    [GoValueClone("pad_cgo_0")] internal partial struct fpstatus {}
    internal partial struct functab {}
    internal partial struct gList {}
    internal partial struct gQueue {}
    internal partial struct gTraceState {}
    internal partial struct gcBgMarkWorkerNode {}
    [GoValueClone("pad")] internal partial struct gcBgMarkWorkerNodePadded {}
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
    [GoValueClone("spanq", "stats")] internal partial struct gcWork {}
    internal partial struct gclink {}
    internal partial struct gclinkptr {}
    [GoLocalName("weakHandleBox")] internal partial struct getOrAddWeakHandle_weakHandleBox {}
    internal partial struct globalAllocᴛ1 {}
    [GoValueClone("seed", "state")] internal partial struct globalRandᴛ1 {}
    internal partial struct godebugInc {}
    internal partial struct goroutineProfileState {}
    internal partial struct goroutineProfileStateHolder {}
    internal partial struct goroutineProfileᴛ1 {}
    internal partial struct gsignalStack {}
    internal partial struct headTailIndex {}
    [GoValueClone("spans", "pageInUse", "pageMarks", "pageSpecials", "pageUseSpanInlineMarkBits")] internal partial struct heapArena {}
    internal partial struct heapStatsAggregate {}
    [GoValueClone("smallAllocCount", "smallFreeCount")] internal partial struct heapStatsDelta {}
    internal partial struct hexdumpMarker {}
    [GoValueClone("dataBuf", "toPos")] internal partial struct hexdumper {}
    [GoValueClone("children")] internal partial struct immortalWeakHandle {}
    internal partial struct immortalWeakHandleMap {}
    internal partial struct initTask {}
    internal partial struct inlineFrame {}
    internal partial struct inlineUnwinder {}
    internal partial struct inlinedCall {}
    [GoValueClone("entries")] internal partial struct itabTableType {}
    [GoValueClone("it_interval", "it_value")] internal partial struct itimerval {}
    internal partial struct kevent_args {}
    internal partial struct keventt {}
    internal partial struct lfstack {}
    internal partial struct libcCallInfo {}
    internal partial struct limiterEvent {}
    internal partial struct limiterEventStamp {}
    internal partial struct limiterEventType {}
    internal partial struct linearAlloc {}
    internal partial struct linknameIter {}
    internal partial struct listHead {}
    internal partial struct listHeadManual {}
    internal partial struct listNode {}
    internal partial struct listNodeManual {}
    internal partial struct liveUserArenaChunk {}
    internal partial struct lockRank {}
    internal partial struct lockRankStruct {}
    internal partial struct mLockProfile {}
    [GoValueClone("mutex", "cond")] internal partial struct mOS {}
    internal partial struct mProfCycleHolder {}
    internal partial struct mSpanList {}
    internal partial struct mSpanState {}
    internal partial struct mSpanStateBox {}
    [GoValueClone("buf")] internal partial struct mTraceState {}
    internal partial struct mWaitList {}
    internal partial struct machMsgTypeNumber {}
    internal partial struct machPort {}
    internal partial struct machTimebaseInfo {}
    internal partial struct machVMAddress {}
    internal partial struct machVMMapRead {}
    internal partial struct machVMRegionFlavour {}
    internal partial struct machVMSize {}
    internal partial struct mach_vm_region_args {}
    internal partial struct madvise_args {}
    internal partial struct markBits {}
    [GoValueClone("alloc", "reusableNoscan", "stackcache")] internal partial struct mcache {}
    [GoValueClone("partial", "full")] internal partial struct mcentral {}
    [GoValueClone("fs")] internal partial struct mcontext32 {}
    [GoValueClone("fs", "pad_cgo_0")] internal partial struct mcontext64 {}
    [GoValueClone("future")] internal partial struct memRecord {}
    internal partial struct memRecordCycle {}
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
    internal partial struct mlock_args {}
    internal partial struct moduledata {}
    internal partial struct modulehash {}
    internal partial struct mspan {}
    [GoValueClone("heapStats", "pause_ns", "pause_end", "lastScanStats")] internal partial struct mstats {}
    internal partial struct neverCallThisFunction {}
    internal partial struct newmHandoffᴛ1 {}
    internal partial struct notInHeap {}
    internal partial struct notInHeapSlice {}
    internal partial struct note {}
    internal partial struct notifyList {}
    internal partial struct objptr {}
    internal partial struct offAddr {}
    internal partial struct open_args {}
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
    internal partial struct proc_regionfilename_args {}
    internal partial struct profAtomic {}
    internal partial struct profBuf {}
    internal partial struct profIndex {}
    internal partial struct profᴛ1 {}
    internal partial struct ptabEntry {}
    internal partial struct pthread {}
    internal partial struct pthread_attr_getstacksize_args {}
    internal partial struct pthread_attr_init_args {}
    internal partial struct pthread_attr_setdetachstate_args {}
    internal partial struct pthread_cond_init_args {}
    internal partial struct pthread_cond_signal_args {}
    internal partial struct pthread_cond_timedwait_relative_np_args {}
    internal partial struct pthread_cond_wait_args {}
    internal partial struct pthread_create_args {}
    internal partial struct pthread_kill_args {}
    internal partial struct pthread_mutex_init_args {}
    internal partial struct pthread_mutex_lock_args {}
    internal partial struct pthread_mutex_unlock_args {}
    [GoValueClone("X__opaque")] internal partial struct pthreadattr {}
    [GoValueClone("X__opaque")] internal partial struct pthreadcond {}
    [GoValueClone("X__opaque")] internal partial struct pthreadcondattr {}
    [GoValueClone("X__opaque")] internal partial struct pthreadmutex {}
    [GoValueClone("X__opaque")] internal partial struct pthreadmutexattr {}
    internal partial struct quoted {}
    internal partial struct raise_args {}
    internal partial struct raiseproc_args {}
    internal partial struct randomEnum {}
    internal partial struct randomOrder {}
    internal partial struct reflectMethodValue {}
    internal partial struct reflectOffsᴛ1 {}
    [GoValueClone("mmst_reg", "mmst_rsrv")] internal partial struct regmmst {}
    internal partial struct regs32 {}
    internal partial struct regs64 {}
    [GoValueClone("xmm_reg")] internal partial struct regxmm {}
    internal partial struct runtimeSelect {}
    internal partial struct rwmutex {}
    internal partial struct scase {}
    internal partial struct scavChunkData {}
    internal partial struct scavChunkFlags {}
    internal partial struct scavengeIndex {}
    internal partial struct scavengerState {}
    internal partial struct schedStatsAggregate {}
    internal partial struct selectDir {}
    internal partial struct semTable {}
    [GoValueClone("pad")] internal partial struct semTableᴛ1 {}
    internal partial struct semaProfileFlags {}
    internal partial struct semaRoot {}
    internal partial struct setitimer_args {}
    internal partial struct sigTabT {}
    [GoValueClone("__sigaction_u")] internal partial struct sigactiont {}
    internal partial struct sigaltstack_args {}
    internal partial struct sigctxt {}
    [GoValueClone("si_value", "__pad")] internal partial struct siginfo {}
    internal partial struct sigset {}
    [GoValueClone("mask", "wanted", "ignored", "recv")] internal partial struct sigᴛ1 {}
    internal partial struct sizeClassScanStats {}
    internal partial struct sliceInterfacePtr {}
    internal partial struct spanAllocType {}
    internal partial struct spanClass {}
    [GoValueClone("scans", "marks")] internal partial struct spanInlineMarkBits {}
    [GoValueClone("ring")] internal partial struct spanQueue {}
    internal partial struct spanQueue_chain {}
    internal partial struct spanSPMC {}
    internal partial struct spanScanOwnership {}
    internal partial struct spanSet {}
    [GoValueClone("spans")] internal partial struct spanSetBlock {}
    internal partial struct spanSetBlockAlloc {}
    internal partial struct spanSetBlockHeader {}
    [GoValueClone("pad")] internal partial struct spanSetBlockHeader2 {}
    internal partial struct spanSetSpinePointer {}
    internal partial struct special {}
    internal partial struct specialBubble {}
    internal partial struct specialCheckFinalizer {}
    internal partial struct specialCleanup {}
    internal partial struct specialPinCounter {}
    internal partial struct specialReachable {}
    internal partial struct specialSecret {}
    internal partial struct specialTinyBlock {}
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
    [GoValueClone("pad_cgo_0")] internal partial struct stackt {}
    [GoValueClone("ensured")] internal partial struct statAggregate {}
    internal partial struct statDep {}
    internal partial struct statDepSet {}
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
    internal partial struct synctestBubble {}
    internal partial struct synctestDeadlockError {}
    internal partial struct sysMemStat {}
    internal partial struct sysStatsAggregate {}
    internal partial struct syscallingThread {}
    internal partial struct sysctl_args {}
    internal partial struct sysctlbyname_args {}
    internal partial struct sysmontick {}
    internal partial struct taggedPointer {}
    internal partial struct textsect {}
    internal partial struct throwType {}
    internal partial struct ticksType {}
    [GoValueClone("counts")] internal partial struct timeHistogram {}
    internal partial struct timeTimer {}
    internal partial struct timer {}
    internal partial struct timerWhen {}
    internal partial struct timers {}
    internal partial struct timespec {}
    [GoValueClone("pad_cgo_0")] internal partial struct timeval {}
    internal partial struct tmpBuf {}
    [GoLocalName("untracedG")] internal partial struct traceAdvance_untracedG {}
    internal partial struct traceAdvancerState {}
    internal partial struct traceArg {}
    internal partial struct traceBlockReason {}
    [GoValueClone("arr")] internal partial struct traceBuf {}
    internal partial struct traceBufHeader {}
    internal partial struct traceBufQueue {}
    internal partial struct traceEventWriter {}
    internal partial struct traceFrame {}
    internal partial struct traceGoStopReason {}
    internal partial struct traceLocker {}
    internal partial struct traceMap {}
    [GoValueClone("children")] internal partial struct traceMapNode {}
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
    [GoValueClone("uc_stack")] internal partial struct ucontext {}
    internal partial struct uint16InterfacePtr {}
    internal partial struct uint32InterfacePtr {}
    internal partial struct uint64InterfacePtr {}
    internal partial struct unwindFlags {}
    internal partial struct unwinder {}
    internal partial struct updateMaxProcsGState {}
    internal partial struct userArena {}
    internal partial struct userArenaStateᴛ1 {}
    [GoValueClone("__sigaction_u")] internal partial struct usigactiont {}
    internal partial struct usleep_args {}
    internal partial struct waitq {}
    internal partial struct wakeableSleep {}
    [GoValueClone("buf")] internal partial struct wbBuf {}
    internal partial struct winlibcall {}
    internal partial struct workType {}
    internal partial struct workType_assistQueue {}
    internal partial struct workType_goroutineLeak {}
    internal partial struct workType_spanSPMCs {}
    internal partial struct workType_strongFromWeak {}
    internal partial struct workType_sweepWaiters {}
    internal partial struct workType_wbufSpans {}
    [GoValueClone("obj")] internal partial struct workbuf {}
    internal partial struct workbufhdr {}
    internal partial struct worldStop {}
    [GoValueClone("pad")] internal partial struct writeBarrierᴛ1 {}
    internal partial struct xRegAllocᴛ1 {}
    internal partial struct xRegPerG {}
    [GoValueClone("scratch")] internal partial struct xRegPerP {}
    [GoValueClone("regs")] internal partial struct xRegState {}
    [GoValueClone("Z0", "Z1", "Z2", "Z3", "Z4", "Z5", "Z6", "Z7", "Z8", "Z9", "Z10", "Z11", "Z12", "Z13", "Z14", "Z15", "Z16", "Z17", "Z18", "Z19", "Z20", "Z21", "Z22", "Z23", "Z24", "Z25", "Z26", "Z27", "Z28", "Z29", "Z30", "Z31")] internal partial struct xRegs {}
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
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸruntimeꓸgc() => builtin.initPackage(typeof(@internal.runtime.gc_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸruntimeꓸgcꓸscan() => builtin.initPackage(typeof(@internal.runtime.gc.scan_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸruntimeꓸmaps() => builtin.initPackage(typeof(@internal.runtime.maps_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸruntimeꓸsys() => builtin.initPackage(typeof(@internal.runtime.sys_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸstrconv() => builtin.initPackage(typeof(@internal.strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸstringslite() => builtin.initPackage(typeof(@internal.stringslite_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtraceꓸtracev2() => builtin.initPackage(typeof(@internal.trace.tracev2_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbits() => builtin.initPackage(typeof(go.math.bits_package));
    // </ImportInitializers>
}
