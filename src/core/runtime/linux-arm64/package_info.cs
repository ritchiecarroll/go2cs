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
[assembly: GoDynamicTypeLift("7374727563747b6974656d2072756e74696d652e737461636b706f6f6c4974656d3b205f205b3130345d627974657d", "stackpoolᴛ1")]
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
[assembly: GoDynamicTypeLift("7374727563747b6d63656e7472616c2072756e74696d652e6d63656e7472616c3b20706164205b38385d627974657d", "mheap_central")]
[assembly: GoDynamicTypeLift("7374727563747b6e6f74652072756e74696d652e6e6f74653b206d61736b205b335d75696e7433323b2077616e746564205b335d75696e7433323b2069676e6f726564205b335d75696e7433323b2072656376205b335d75696e7433323b20737461746520696e7465726e616c2f72756e74696d652f61746f6d69632e55696e7433323b2064656c69766572696e6720696e7465726e616c2f72756e74696d652f61746f6d69632e55696e7433323b20696e75736520626f6f6c7d", "sigᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b70656e64696e6720696e7465726e616c2f72756e74696d652f61746f6d69632e426f6f6c3b20656e61626c656420626f6f6c3b20646f6e6520626f6f6c3b20636f756e7420696e747d", "workType_goroutineLeak")]
[assembly: GoDynamicTypeLift("7374727563747b726f6f742072756e74696d652e73656d61526f6f743b20706164205b3130345d627974657d", "semTableᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b72756e74696d652e6d757465783b2072756e74696d652e70657273697374656e74416c6c6f637d", "globalAllocᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b73656d612075696e7433323b2061637469766520626f6f6c3b206f666673657420696e7465726e616c2f72756e74696d652f61746f6d69632e496e7436343b207265636f726473205b5d696e7465726e616c2f70726f66696c657265636f72642e537461636b5265636f72643b206c6162656c73205b5d756e736166652e506f696e7465727d", "goroutineProfileᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7369676e616c4c6f636b20696e7465726e616c2f72756e74696d652f61746f6d69632e55696e7433323b20687a20696e7465726e616c2f72756e74696d652f61746f6d69632e496e7433327d", "profᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7370696e41667465725261676765644261727269657220696e7465726e616c2f72756e74696d652f61746f6d69632e426f6f6c3b20726573746172746564447565546f323739393320626f6f6c7d", "gcDebugMarkDoneᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b737461746573205b5d75696e747074723b207374617465734c6f636b2072756e74696d652e6d757465783b20737461746553697a652075696e747074723b206d6d617050726f7420696e7433323b206d6d6170466c61677320696e7433327d", "vgetrandomAllocᴛ1")]
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
[assembly: GoHoistedLiterals]

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: GoFieldAccess(typeof(Frames))]
[assembly: GoFieldAccess(typeof(_type))]
[assembly: GoFieldAccess(typeof(activeSweep))]
[assembly: GoFieldAccess(typeof(arenaHint))]
[assembly: GoFieldAccess(typeof(atomicHeadTailIndex))]
[assembly: GoFieldAccess(typeof(atomicMSpanPointer))]
[assembly: GoFieldAccess(typeof(atomicOffAddr))]
[assembly: GoFieldAccess(typeof(atomicScavChunkData))]
[assembly: GoFieldAccess(typeof(atomicSpanSetSpinePointer))]
[assembly: GoFieldAccess(typeof(blockRecord))]
[assembly: GoFieldAccess(typeof(bucket))]
[assembly: GoFieldAccess(typeof(chantype))]
[assembly: GoFieldAccess(typeof(checkmarksMap))]
[assembly: GoFieldAccess(typeof(childInfo))]
[assembly: GoFieldAccess(typeof(consistentHeapStats))]
[assembly: GoFieldAccess(typeof(coro))]
[assembly: GoFieldAccess(typeof(cpuProfile))]
[assembly: GoFieldAccess(typeof(dbgVar))]
[assembly: GoFieldAccess(typeof(debugCallWrapArgs))]
[assembly: GoFieldAccess(typeof(debugLogBuf))]
[assembly: GoFieldAccess(typeof(dloggerImpl))]
[assembly: GoFieldAccess(typeof(elfDyn))]
[assembly: GoFieldAccess(typeof(elfPhdr))]
[assembly: GoFieldAccess(typeof(elfSym))]
[assembly: GoFieldAccess(typeof(elfVerdaux))]
[assembly: GoFieldAccess(typeof(elfVerdef))]
[assembly: GoFieldAccess(typeof(findfuncbucket))]
[assembly: GoFieldAccess(typeof(functype))]
[assembly: GoFieldAccess(typeof(gcBgMarkWorkerNode))]
[assembly: GoFieldAccess(typeof(gcBgMarkWorkerNodePadded))]
[assembly: GoFieldAccess(typeof(gcBits))]
[assembly: GoFieldAccess(typeof(gcBitsArena))]
[assembly: GoFieldAccess(typeof(gcCPULimiterState))]
[assembly: GoFieldAccess(typeof(gcControllerState))]
[assembly: GoFieldAccess(typeof(gcWork))]
[assembly: GoFieldAccess(typeof(gclink))]
[assembly: GoFieldAccess(typeof(getOrAddWeakHandle_weakHandleBox))]
[assembly: GoFieldAccess(typeof(go.@internal.abi_package.Imethod))]
[assembly: GoFieldAccess(typeof(go.@internal.abi_package.InterfaceSwitch))]
[assembly: GoFieldAccess(typeof(go.@internal.abi_package.InterfaceSwitchCache))]
[assembly: GoFieldAccess(typeof(go.@internal.abi_package.Method))]
[assembly: GoFieldAccess(typeof(go.@internal.abi_package.RegArgs))]
[assembly: GoFieldAccess(typeof(go.@internal.abi_package.StructField))]
[assembly: GoFieldAccess(typeof(go.@internal.abi_package.Type))]
[assembly: GoFieldAccess(typeof(go.@internal.abi_package.TypeAssert))]
[assembly: GoFieldAccess(typeof(go.@internal.abi_package.TypeAssertCache))]
[assembly: GoFieldAccess(typeof(go.@internal.abi_package.UncommonType))]
[assembly: GoFieldAccess(typeof(go.@internal.abi_package.ΔInterfaceType))]
[assembly: GoFieldAccess(typeof(go.@internal.abi_package.ΔStructType))]
[assembly: GoFieldAccess(typeof(go.@internal.runtime.syscall.linux_package.EpollEvent))]
[assembly: GoFieldAccess(typeof(godebugInc))]
[assembly: GoFieldAccess(typeof(heapArena))]
[assembly: GoFieldAccess(typeof(heapStatsAggregate))]
[assembly: GoFieldAccess(typeof(heapStatsDelta))]
[assembly: GoFieldAccess(typeof(immortalWeakHandle))]
[assembly: GoFieldAccess(typeof(immortalWeakHandleMap))]
[assembly: GoFieldAccess(typeof(inlinedCall))]
[assembly: GoFieldAccess(typeof(interfacetype))]
[assembly: GoFieldAccess(typeof(itab))]
[assembly: GoFieldAccess(typeof(itabTableType))]
[assembly: GoFieldAccess(typeof(limiterEvent))]
[assembly: GoFieldAccess(typeof(listNode))]
[assembly: GoFieldAccess(typeof(listNodeManual))]
[assembly: GoFieldAccess(typeof(mLockProfile))]
[assembly: GoFieldAccess(typeof(mProfCycleHolder))]
[assembly: GoFieldAccess(typeof(mSpanList))]
[assembly: GoFieldAccess(typeof(mSpanStateBox))]
[assembly: GoFieldAccess(typeof(mTraceState))]
[assembly: GoFieldAccess(typeof(mcache))]
[assembly: GoFieldAccess(typeof(memRecord))]
[assembly: GoFieldAccess(typeof(memRecordCycle))]
[assembly: GoFieldAccess(typeof(metricFloat64Histogram))]
[assembly: GoFieldAccess(typeof(metricValue))]
[assembly: GoFieldAccess(typeof(mheap))]
[assembly: GoFieldAccess(typeof(mlink))]
[assembly: GoFieldAccess(typeof(moduledata))]
[assembly: GoFieldAccess(typeof(mspan))]
[assembly: GoFieldAccess(typeof(mstats))]
[assembly: GoFieldAccess(typeof(notifyList))]
[assembly: GoFieldAccess(typeof(pageAlloc))]
[assembly: GoFieldAccess(typeof(pallocData))]
[assembly: GoFieldAccess(typeof(pcHeader))]
[assembly: GoFieldAccess(typeof(pcvalueCache))]
[assembly: GoFieldAccess(typeof(pcvalueCacheEnt))]
[assembly: GoFieldAccess(typeof(pollCache))]
[assembly: GoFieldAccess(typeof(pollDesc))]
[assembly: GoFieldAccess(typeof(profBuf))]
[assembly: GoFieldAccess(typeof(ptrtype))]
[assembly: GoFieldAccess(typeof(reflectMethodValue))]
[assembly: GoFieldAccess(typeof(rwmutex))]
[assembly: GoFieldAccess(typeof(scase))]
[assembly: GoFieldAccess(typeof(scavengeIndex))]
[assembly: GoFieldAccess(typeof(scavengerState))]
[assembly: GoFieldAccess(typeof(semaRoot))]
[assembly: GoFieldAccess(typeof(sigTabT))]
[assembly: GoFieldAccess(typeof(sigcontext))]
[assembly: GoFieldAccess(typeof(siginfo))]
[assembly: GoFieldAccess(typeof(spanInlineMarkBits))]
[assembly: GoFieldAccess(typeof(spanQueue))]
[assembly: GoFieldAccess(typeof(spanSPMC))]
[assembly: GoFieldAccess(typeof(spanSet))]
[assembly: GoFieldAccess(typeof(spanSetBlockAlloc))]
[assembly: GoFieldAccess(typeof(special))]
[assembly: GoFieldAccess(typeof(specialBubble))]
[assembly: GoFieldAccess(typeof(specialCheckFinalizer))]
[assembly: GoFieldAccess(typeof(specialCleanup))]
[assembly: GoFieldAccess(typeof(specialPinCounter))]
[assembly: GoFieldAccess(typeof(specialReachable))]
[assembly: GoFieldAccess(typeof(specialSecret))]
[assembly: GoFieldAccess(typeof(specialTinyBlock))]
[assembly: GoFieldAccess(typeof(specialWeakHandle))]
[assembly: GoFieldAccess(typeof(specialfinalizer))]
[assembly: GoFieldAccess(typeof(specialprofile))]
[assembly: GoFieldAccess(typeof(stackObject))]
[assembly: GoFieldAccess(typeof(stackObjectBuf))]
[assembly: GoFieldAccess(typeof(stackObjectRecord))]
[assembly: GoFieldAccess(typeof(stackScanState))]
[assembly: GoFieldAccess(typeof(stackWorkBuf))]
[assembly: GoFieldAccess(typeof(stackmap))]
[assembly: GoFieldAccess(typeof(stackpoolItem))]
[assembly: GoFieldAccess(typeof(stackt))]
[assembly: GoFieldAccess(typeof(statAggregate))]
[assembly: GoFieldAccess(typeof(stkframe))]
[assembly: GoFieldAccess(typeof(stringStruct))]
[assembly: GoFieldAccess(typeof(structtype))]
[assembly: GoFieldAccess(typeof(sweepdata))]
[assembly: GoFieldAccess(typeof(synctestBubble))]
[assembly: GoFieldAccess(typeof(sysmontick))]
[assembly: GoFieldAccess(typeof(ticksType))]
[assembly: GoFieldAccess(typeof(timeHistogram))]
[assembly: GoFieldAccess(typeof(timeTimer))]
[assembly: GoFieldAccess(typeof(timer))]
[assembly: GoFieldAccess(typeof(timerWhen))]
[assembly: GoFieldAccess(typeof(timers))]
[assembly: GoFieldAccess(typeof(traceBuf))]
[assembly: GoFieldAccess(typeof(traceMap))]
[assembly: GoFieldAccess(typeof(traceMapNode))]
[assembly: GoFieldAccess(typeof(traceRegionAlloc))]
[assembly: GoFieldAccess(typeof(traceRegionAllocBlock))]
[assembly: GoFieldAccess(typeof(traceStackTable))]
[assembly: GoFieldAccess(typeof(traceStringTable))]
[assembly: GoFieldAccess(typeof(traceTypeTable))]
[assembly: GoFieldAccess(typeof(typeCacheBucket))]
[assembly: GoFieldAccess(typeof(uncommontype))]
[assembly: GoFieldAccess(typeof(unwinder))]
[assembly: GoFieldAccess(typeof(updateMaxProcsGState))]
[assembly: GoFieldAccess(typeof(waitq))]
[assembly: GoFieldAccess(typeof(wakeableSleep))]
[assembly: GoFieldAccess(typeof(workType))]
[assembly: GoFieldAccess(typeof(workbuf))]
[assembly: GoFieldAccess(typeof(xRegState))]
[assembly: GoFieldAccess(typeof(Δhchan))]
[assembly: GoFieldAccess(typeof(Δsliceᴛ))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("runtime/alg.go", "alg.cs", "ABUugsqUpoKmgqaCpoKokoKCAAg2ABQElAABHgAOBIKCAAIQwoKUpKS4goKUpKS4goKmgoKmgoKCgpSCypSClAADHAAKAoKCgpSUlIKUAAMyABcClJSkpKaUpKSkpKSCgpSkgoKUpIKCgpSUyMqSpoKUgpSClIKUgpSClIKUgpSClIKUgpSCgoKUgoKClKKClIKClLiUlKKClIKCgpSUlAACHAANAsaigqaixqIAFSqUqIKUgoKUgriClIKCupKCgpSmgoKClA==")]
[assembly: GoPositionMap("runtime/arena.go", "arena.cs", "AF3MAQAIAgAGEgAIAoKClIKCgoKCgqyyrLIAEYgBgoKUgpSCgqaCpqrqACKQAsKCgoKCgpSUAAgKtIKCypSCgoKUpoKUgoKCgoKUgpSCgqaCggAiONKCgpSUlIKUpoKU3IKClIKUlIKUgoKmgoKmlIKClIKmgoKUlIKClIKUAAkUlIKU7NKCgpSCAAMQ8oKUgpSCgoIABxAABxCCuJaSggAKFqLKpqaClKrClIKCyoKCpoKCgu6CgqiSgpSCgoKUqsLKpoKCgpSCuIKmgoKClIK47pSCgoKCgoKUgpS8ooKClJQACgyyggAIEoK4goKUgpSWkoKUgpTcgqaUppSmpoKCpoKCgqaClKaCpoKmgpQABxCCpgACGgAKAgAIEPKClIIADh7KytzcgoKCgoKmpoKmlIKCupKCgpQACBQACQKClILKgpSClIKUgtymgoKCgpSCgriCpoKClAAIGAALAoKmgoKCgqaCuJSCgpTcgoKClJSClIKUlAAHEIKmyoKCgoIACBKmgriClIKCpoKCpqa4AAoWgpSmuLiCgoKU", "764-766:1;912-917:1")]
[assembly: GoPositionMap("runtime/asan0.go", "asan0.cs", "ABIogIKCgoKCgoI=")]
[assembly: GoPositionMap("runtime/atomic_pointer.go", "atomic_pointer.cs", "AA1AABUCgoKCrLKClIKUAAIQ0gACENKClIKUAAIQ6KKClIKUqKiigpSClIKoqKKClIKU")]
[assembly: GoPositionMap("runtime/cgo.go", "cgo.cs", "AEaaAeAABRDQAAkUlIKCgpQ=")]
[assembly: GoPositionMap("runtime/cgo_mmap.go", "cgo_mmap.cs", "ABc+4ozSgpSClJSmgoKAkpSoqLao", "39-41:1;52-52:1")]
[assembly: GoPositionMap("runtime/cgo_sigaction.go", "cgo_sigaction.cs", "ABMuAAoOgpSClIIABxCUlIKClIKm6gALFILIgri4gpSCAAMQ", "72-74:1")]
[assembly: GoPositionMap("runtime/cgocall.go", "cgocall.cs", "AGncAfKCggALKgAQAoKmgqaCpoKmAA8g7pTulLiClLiUlLiCpqaCgoKCggANHIKClAACEuKUuKaCygAVLIKCgozSAAgSgoKCpoLssoKCgqaCgoLulLgAChaCgoKCgoKmlIKmyoKUpqaCpqaUpoKCgqailIKCpoIABxCCgoKUlIK4ysqCgriClJKmgriCgoKmgqqigoKUgqaUgpSCgoLcoqSCpoKC7oKCggAIEpT6kuiSACnUAfKUppS0goKClIKUgoL8tIKUgpS0goLKgpSCgpSClLSCgoKClIKUgpSCgsaCgpSCxoKCgpSClIKClMaCgoK4gpSCpgAEEPKCgoKClIKCgoKUgoKmpoKm3AACEuKCpoKmgoK4rsKs0oKmgoKClAAYCqKSgoKmlLS0tLS0tLS0tMiCAAcQgoKUpoKUypSCgrS2", "416-420:1", "", "", "456:21", "432:34,434:17")]
[assembly: GoPositionMap("runtime/cgocallback.go", "cgocallback.cs", "AAcWog==")]
[assembly: GoPositionMap("runtime/cgocheck.go", "cgocheck.cs", "ABAyAAgCuJSClILKgoLKgsqC3IKmgoIAAxgACgIAAhYACgKClIKUgpQAAhYACgKClIKUgpSCgoIAAxQACQSClIKmAAIS4oKCgoKCgoKCgoKUlIKUgoKC", "65-68:1")]
[assembly: GoPositionMap("runtime/cgroup_linux.go", "cgroup_linux.cs", "ACVOnLKClKaUgoLugoKCuKqiggADEAAOFoKClJSmqJKCgoKCgqY=")]
[assembly: GoPositionMap("runtime/chan.go", "chan.cs", "ADp+kuaCgqYACAaCpoKUgqaCgq7CppTYgsaCtoKCgoKUlIKUAAIcAA0CruaUpqyyAA4eAA4CgoKUgqaCpoKmggASJoKmgoKmlIKCpoCmgJK2lIKClIKCgpSCgqaCgriCgoKCuIKCgoKCgoLKgoKClMqmgpSCgoKCgpSCgoKClJQAAhIACQKCgpSCgsqCgoKClKaCgpSCgoKCgpQAAhAADAyClIKCgoKCgoKUlIIAAhoAEQyCpqbogoIACgaigpSCpoKCgqaCgoKmlKaCgoKUgoKUgpSCgoKClLiCgoKUgoKUgoKCgpSUpoKCggADEPSCuIKUrLKosoIABhIADQiCpoKClIKmgqaCuAAJFMrKlIKUgpS4goKmlIKCgpSCgpTKgMqAksiUgoKUgpSCgoKUgoKmgoK4goKCgriCgpSCgoKCgoLugoKClKaClIKUgoKClIKCgoIAAiIAEQKCgpSCgpSU7oKCgqaCpoKCgpSUgoKCgoKClKb8uNyCAAImABACAAImABACqJKokqaigpSCgpS4lKaigpSCgoLKlKiSqJKokqiSpqKCgoKCgoKUgoKmgoKCgpSCgoKUgoIAChaClLi43KaCgoKCrAANEO6CgoKUgqaClA==", "232-232:1;607-607:1")]
[assembly: GoPositionMap("runtime/checkptr.go", "checkptr.cs", "ABAStILugsqCvKKCyoKUygAIBoKC3IKCpoKCuAACKAASBAAIEriCuIKClIK4")]
[assembly: GoPositionMap("runtime/complex.go", "complex.cs", "AAcSooKClKaCyIKCgoKUgoKCpriClJSCtoKCgraCgoLI")]
[assembly: GoPositionMap("runtime/coro.go", "coro.cs", "ACNU0oKCgoKSgoKCuIKCgqaCgqzSgoKUgqKoooKCgq7CgoIACh4ADgKCgoKCAAcQgoKCgoKCAAkUlIKCuIKmpqbcgriCpriqogAKFoKClIKCgoIABxCC3ILcggAIEoKmpoK4goK4gqaCuA==", "47-60:1")]
[assembly: GoPositionMap("runtime/covercounter.go", "covercounter.cs", "AAsasoKCgoKUyg==")]
[assembly: GoPositionMap("runtime/covermeta.go", "covermeta.cs", "AA8cwoKClA==")]
[assembly: GoPositionMap("runtime/cpuflags_arm64.go", "cpuflags_arm64.cs", "AAwagoI=")]
[assembly: GoPositionMap("runtime/cpuprof.go", "cpuprof.cs", "ADqIAQAIBIKUgqaCgoKCgqaCgoKCgoKCgoKUAAIUAAgElKaCgpTKpgACGgAQDpSmgoKCgpSmAAIQ5IKCgpSmgoK4gqaCgriCAAMcAAkCAAIaAAoCAAIoABECgoKCgoKUgoKCgpQ=", "", "", "177=FuncPCABIInternal/1/2/1,FuncPCABIInternal/2/2/2;187=FuncPCABIInternal/1/2/1,FuncPCABIInternal/2/2/2")]
[assembly: GoPositionMap("runtime/create_file_unix.go", "create_file_unix.cs", "AAoYkg==")]
[assembly: GoPositionMap("runtime/debug.go", "debug.cs", "AA3qAQANCpSCgoKUpqbcgoKClAACENIACDqSAAIwABYEgoKmggADFgAIBIKCpoIABhrigoKUlA==")]
[assembly: GoPositionMap("runtime/debugcall.go", "debugcall.cs", "ABoygt7kgpTKqqKCgoKCppQABB4ABxCCgriClIKUpgAGGAAJAoKCAAcQuMiCgri4goLcgpSCgty4uJSCpoK4lIKClNzcgoKCppQABxSigoKCpqaCgoK4goLcgriUgoKUgoKClIKCgoKUuKSSgpSCgoKCpoKi", "49-95:1;123-161:1;164-188:2;222-259:1;269-274:1", "", "", "", "272:21,273:13")]
[assembly: GoPositionMap("runtime/debuglog.go", "debuglog.cs", "ACx8ABIEqqIAHMIB5sSCgrimgrgAESqAlrKClJSogJaSqICWkqiAlpKogJaSqICWsoKCqICWkqiAlpKogJaSqICWkqiAlpKogJaygoKogJaygoKogNbCgoKUgpS0tqiAAANGgJaygoKCgoKmgoKClIKCpqiAlrKCgqiAlrKCgoKUAC9kkpTuzJKCgqiSgoKCgoKCqJKCgqiSgoKCqJKCgoKCgoK6koKClJSokoKCgoKUgoIACx6kgpSClIKClIKUgqiSqpKCgoKUppSCgoKUgoKUgqaCgriClIKCgoKCgoKmgpSm9oKCpoKClKaCgoKCgoKCpqaCgoKClJQABMwBkoI=")]
[assembly: GoPositionMap("runtime/debuglog_off.go", "debuglog_off.cs", "AAwagtqCpoI=")]
[assembly: GoPositionMap("runtime/defs_linux_arm64.go", "defs_linux_arm64.cs", "AGXQAZKCAAcQgg==")]
[assembly: GoPositionMap("runtime/dit.go", "dit.cs", "AAsakoKCgqiSgoKC")]
[assembly: GoPositionMap("runtime/env_posix.go", "env_posix.cs", "AA0SgoKClIKCpqqigoKCgpSUlKaCgpQAGDSSgpSCqJKClIKmgoKC")]
[assembly: GoPositionMap("runtime/error.go", "error.cs", "ACtUlIKCgpSCgpSCgoKUgpSmlAACEtKCgoKClILclIIABxCUggACENIABxCUggARJAAMGgAKFpSCgoKUgoKmooKCuIKCgoKCgpSClLS2AAYeAAgClLS0tLS0tLS0tLS0tLS0tLS0xMqygpSUgoK0tLS0tLS0tLS0tLS0tLS0tMqSgoKClIKCgpSuwoK4goKUgoKUgoKClIKUgoI=")]
[assembly: GoPositionMap("runtime/extern.go", "extern.cs", "AK4C6gQADAKCgoKUggAKSAAIAoKClAAMILI=")]
[assembly: GoPositionMap("runtime/fastlog2.go", "fastlog2.cs", "AAcc8oKUpoKClII=")]
[assembly: GoPositionMap("runtime/fastlog2table.go", "fastlog2table.cs", "AAkS")]
[assembly: GoPositionMap("runtime/fds_unix.go", "fds_unix.cs", "AAwSgrimAAUQgoKCgqaCgqaCgoKCgg==")]
[assembly: GoPositionMap("runtime/fipsbypass.go", "fipsbypass.cs", "AAkUkqiSqJI=")]
[assembly: GoPositionMap("runtime/float.go", "float.cs", "AA0elqSokqiSAAIS4oKqooKokqqiAAIaAAoCgpSCgoKUlIIAAhoACgIAAhoADQKClIKklKaCpoKUgoI=")]
[assembly: GoPositionMap("runtime/hash64.go", "hash64.cs", "ABAqgpKClKSCgrSCtIK0grSCtIKCgoKCgoKClJSCgpSCtqaCgqaCgqaCgqaCpoI=")]
[assembly: GoPositionMap("runtime/heapdump.go", "heapdump.cs", "ADeeAYKClIKCgqaCgoKUgriipoKCABIqkoKCgoKClIKCpoKClLqSgqaCgoK4gqiygsqCgpSClIKUgtyClKaCgoKCgpSCgoKCgpSokoKCgqaCgoKmooKCgoKCAAwcsoKCggAICrKmgoKCgpS4lJSCgpS4goKCgoKCgoKCgpSmgqaCgsqUgoKUlIKCgpSmlKaCgoKCgoKUlKaiooKCgpSCgqaCgoKCgoKCgoKCgoKClpKCgoKCgoK4goKCgoKCgoKUlJSUgoKCgoKCgoL4gqaCgtqClKSC3MqigoKCgoKmlLiCgoKmgoKCpoKUgoKUgoLcAAoOlJSCgpSCgoKCpoKCuIKCgpTKgoKCgpSUgpKCgpSCgpSCgpSCuIKCgoKmooKCgoKmgqaCgoKCgrqSyoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKClKaCgoKCgoKCgoKCgoKCgoKClIKCgqaCgoKCgoKCgpSCgpSCgqaCppSUgoKClIKClIKCgoLKpIKmgoKmgoKCgoKCgoKCgoKmgpSCpqamgoKCpqiyggAIBpSCgoKUgoKClKaCgoKCgoKUgpQ=", "412-433:1")]
[assembly: GoPositionMap("runtime/hexdump.go", "hexdump.cs", "ADOoAYKCuIKCgoKClIK4goK4poKCAAkIgpSCgoKmgpSCgriUgoKUuMqUgoKmAAoGgpSCgoaigoKClIKCgqaUlIKmgpSCgoK4gpSCgqaClIKCgpSmgoKmpoKCgoKUlIK4goLqooKCgoKUlLiCgoKUgoKUgg==", "175-179:1")]
[assembly: GoPositionMap("runtime/histogram.go", "histogram.cs", "AF7SAQAJBIKCAAESAAgCgoKUgriCgqaCqMK4goKUAAUQgoKmgoKsspSUpqa4gpSUyqa4goI=")]
[assembly: GoPositionMap("runtime/iface.go", "iface.cs", "ABUmggAJFJQABhgADAKCuIKClIKm3IKCuIKCgriCgtyCgoKCgoKUggAHEKr4goKCuIKClIKUgvwACAqCpoLKgtyCgqaUpqr2goKCgoLKlMqCgpSCAAMeAA0CgoLcgoKCuIKCgoKCgoKCgoKUgoKCgoKCgpSCgoKCgpTctIKUpoKCgoKCpq7CqsKCgpSqogAdMqSCgoKCAAEYAAsCgpSClIKUgoKUtIKUgpSCpoKCpqKCgoKmgpSmooKCgqaClAACGAALAoKUgpQAAhgACwKClIKUAAIYAA0EgpSClKaClJSmgoKUrNKCgoKmpoLKlKaUprjKlKaipoKCgu6CpoKCgqaSgoKCgoKUpoKCppTKnvKmgqaCgoKCuILKuKaUuLjKlKzSpoKCgu6CpoKCgqaSgoKCgoKClKaCgqaUygABGAAKAqiSpqaCgoKCAAocwuyy", "539-549:1;645-656:1")]
[assembly: GoPositionMap("runtime/lfstack.go", "lfstack.cs", "ABYwooKCgoKCgsqClIKUgoKClIKCggAHEJS6guqigpSmgqaC")]
[assembly: GoPositionMap("runtime/linkname_shim.go", "linkname_shim.cs", "AEWwAQAUAoKCppSCgpSCAAImABACAAIoABMCgoKmlIIAAigAEQIAAiIADgIAAiIADgI=")]
[assembly: GoPositionMap("runtime/list.go", "list.cs", "ABQyooIAGi6CgqaClKiSqJKoyIKmgoKmqJKCuKaCpqaCgqaokpSCpoKCpoKmgqaC")]
[assembly: GoPositionMap("runtime/list_manual.go", "list_manual.cs", "ABtAooIAFi6CgqaClKiSqJKoyIKmgoKmqJKCuKaCpqaCgqaokpSCpoKCpoKmgqaC")]
[assembly: GoPositionMap("runtime/lock_futex.go", "lock_futex.cs", "AAwisqiyAAymAYKCgqYABCKCppamkpSCgpSCgoKU3pKCgg==")]
[assembly: GoPositionMap("runtime/lock_spinbit.go", "lock_spinbit.cs", "AEKUAZKClAASIJKCgpQAAxgACAKCgoKUAAMiAA0ClOwABRCCAATYAYIABIgBooKmgoKU")]
[assembly: GoPositionMap("runtime/lockrank.go", "lockrank.cs", "AFy8AQBMigGCgpSClIKUAAcQ")]
[assembly: GoPositionMap("runtime/lockrank_off.go", "lockrank_off.cs", "AA8gpoKmgqyypoKssqzYuLi4uLg=")]
[assembly: GoPositionMap("runtime/malloc.go", "malloc.cs", "AIcDlgaCgpSCgoIAHgaCgqamuJSUgoKUgoKUgoKUgoKUypSmgqaCgpSCggAHEIKCgoKUgoKmgpSCuIKmgriCgoKCgoKClKamADt4gsqCppSCgoKmgtyCgoK4goKUgoL8goLGgrS0tLSmlLTagoKUgoIAEyiCgoIACRQACBKCgpSClKbKgoKCgoKmgoLcgoK4AB4kABUClJTcgoKCyoKCgoKUlIKUlJSUgpSCggAHEIKUgqaCytyCgriCgoKCgriCgoKCgoKCgpSmgriCpoKCgriCggAIEoKClIKUlKaClIKCgoKCyoKCgpSCgpSCgtyC3LiCpgACIAAQBNyCgoKUgriCgoKUAAYSwoKCgoKCgpSCgoKmAAoYAA0CgoKClIKClIKClKaCpoKCgoKUAB9YABQCgoLKgqaCgpSClO6mgoK8ooKCyoKokoKUgoKUgpSmpoK4goKUpoKUgpS4uIKmuIKUgpSmgoKUgpSCuIKCyoKU5rSCgoKUgpSCpgAePoKUgoLugoKCgpSUgoKCgoKmgoKCgpSCgqaUggAIEpTKAAkUAAsYgoKUgpSCgrgADBqU5rSCgoKUgpSCppSCgoKClJSCgqaUgoKCpoKClIKCAAgSlMoACRQACxiCgpSClIKCpqr2ggAHEILKgrgADRzKlAAIBrSCgoKUgpSClIKmlIKCgoKCgoKUgoKUppSUAAcQlMoACRQACxiCgpSClIKCpua0goKClIKUgpSCppSCgoKCgpSUgoKCgoKUgoKUgoIABxCUygAJFAALGIKClIKUgoKmprSCgoKUgqaUpoKCgoKCAAgSlMoACRQACxiCgpSCpoLclAAKFoKU7oKCpoKCggAHEIKCgoKClKaUlJSmgpSmgoKCggAIEoIAAxDUgoK4lLi+xIKCpgAabAApAoKUpqaCyoKClIKUlJSCgqYADRyCgpSCuILKgoKmgrimqqKClKaClIKClLiCptyClOrCgriCgpSUgpSC3IKUlLiClKYADArGgoKUgpSClIKClIKmgqaCgsySgpQAAhYACAKEkoKClKaCgpS+0qiSAAIeAA4CqLIAAiAADwKClIKClAACJAAPAqiS7sKCgpSCggACFPKUlJSUqsaUtAACFgAIAoKCgpSCABhGAA0CgoKUABLKAYLKlIKCpoKCgpSCgpSCgpSUAA4agqqilKSkpKSkpKQ=", "2331-2333:1")]
[assembly: GoPositionMap("runtime/malloc_generated.go", "malloc_generated.cs", "ABMapIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmhIKCpoKCuJSCgriCppSUgoKUlIKUgoKEgoKCgpSEgoKCgoKChIKmgoK4gpS6gpSCpoKCgoKCgoKCgoK8goKUgoKUhIKmlISWpoKClIKUgoK4hJSCpoKCuIKUpoSCgqaCgriUgoK4gqaUlIKClJSClIKChIKCgoKUhIKCgoKCgoSCpoKCuIKUuoKUgqaCgoKCgoKCgoKCvIKClIKClISCppSElqaCgpSClIKCuISUgqaCgriClKaEgoKmgoK4lIKCuIKmlJSCgpSUgpSCgoSCgoKClISCgoKCgoKEgqaCgriClLqClIKmgoKCgoKCgoKCgryCgpSCgpSEgqaUhJamgoKUgpSCgriElIKmgoK4gpSmhIKCpoKCuJSCgriCppSUgoKUlIKUgoKEgoKCgpSEgoKCgoKChIKmgoK4gpS6gpSCpoKCgoKCgoKCgoK8goKUgoKUhIKmlISWpoKClIKUgoK4hJSCpoKCuIKUpoSCgqaCgriUgoK4gqaUlIKClJSClIKChIKCgoKUhIKCgoKCgoSCpoKCuIKUuoKUgqaCgoKCgoKCgoKCvIKClIKClISCppSElqaCgpSClIKCuISUgqaCgriClKaEgoKmgoK4lIKCuIKmlJSCgpSUgpSCgoSCgoKClISCgoKCgoKEgqaCgriClLqClIKmgoKCgoKCgoKCgryCgpSCgpSEgqaUhJamgoKUgpSCgriElIKmgoK4gpSmhIKCpoKCuJSCgriCppSUgoKUlIKUgoKEgoKCgpSEgoKCgoKChIKmgoK4gpS6gpSCpoKCgoKCgoKCgoK8goKUgoKUhIKmlISWpoKClIKUgoK4hJSCpoKCuIKUpoSCgqaCgriUgoK4gqaUlIKClJSClIKChIKCgoKUhIKCgoKCgoSCpoKCuIKUuoKUgqaCgoKCgoKCgoKCvIKClIKClISCppSElqaCgpSClIKCuISUgqaCgriClKaEgoKmgoK4lIKCuIKmlJSCgpSUgpSCgoSCgoKClISCgoKCgoKEgqaCgriClLqClIKmgoKCgoKCgoKCgryCgpSCgpSEgqaUhJamgoKUgpSCgriElIKmgoK4gpSmhIKCpoKCuJSCgriCppSUgoKUlIKUgoKEgoKCgpSEgoKCgoKChIKmgoK4gpS6gpSCpoKCgoKCgoKCgoK8goKUgoKUhIKmlISWpoKClIKUgoK4hJSCpoKCuIKUpoSCgqaCgriUgoK4gqaUlIKClJSClIKChIKCgoKUhIKCgoKCgoSCpoKCuIKUuoKUgqaCgoKCgoKCgoKCvIKClIKClISCppSElqaCgpSClIKCuISUgqaCgriClKaEgoKmgoK4lIKCuIKmlJSCgpSUgpSCgoSCgoKClISCgoKCgoKEgqaCgriClLqClIKmgoKCgoKCgoKCgryCgpSCgpSEgqaUhJamgoKUgpSCgriElIKmgoK4gpSmhIKCpoKCuJSCgriCppSUgoKUlIKUgoKEgoKCgpSEgoKCgoKChIKmgoK4gpS6gpSCpoKCgoKCgoKCgoK8goKUgoKUhIKmlISWpoKClIKUgoK4hJSCpoKCuIKUpoSCgqaCgriUgoK4gqaUlIKClJSClIKChIKCgoKUhIKCgoKCgoSCpoKCuIKUuoKUgqaCgoKCgoKCgoKCvIKClIKClISCppSElqaCgpSClIKCuISUgqaCgriClKaEgoKmgoK4lIKCuIKmlJSCgpSUgpSCgoSCgoKClISCgoKCgoKEgqaCgriClLqClIKmgoKCgoKCgoKCgryCgpSCgpSEgqaUhJamgoKUgpSCgriElIKmgoK4gpSmhIKCuJSCgriCppSUgoKUlIKCgpSEgoKCgoSChKaCpoKCuIKUuoKCgoKCgoKCgoK8goKUgoKmlISWpoKClIKUgoKmgoSmgqaCgriClKaEgoK4lIKCuIKmlJSCgpSUgoKClISCgoKChIKEpoKmgoK4gpS6goKCgoKCgoKCgryCgpSCgqaUhJamgoKUgpSCgqaChKaCpoKCuIKUpoSCgriUgoK4gqaUlIKClJSCgoKUhIKCgoKEgoSmgqaCgriClLqCgoKCgoKCgoKCvIKClIKCppSElqaCgpSClIKCpoKEpoKmgoK4gpSmhIKCuJSCgriCppSUgoKUlIKCgpSEgoKCgoSChKaCpoKCuIKUuoKCgoKCgoKCgoK8goKUgoKmlISWpoKClIKUgoKmgoSmgqaCgriClKaEgoK4lIKCuIKmlJSCgpSUgoKClISCgoKChIKEpoKmgoK4gpS6goKCgoKCgoKCgryCgpSCgqaUhJamgoKUgpSCgqaChKaCpoKCuIKUpoSCgriUgoK4gqaUlIKClJSCgoKUhIKCgoKEgoSmgqaCgriClLqCgoKCgoKCgoKCvIKClIKCppSElqaCgpSClIKCpoKEpoKmgoK4gpSmhIKCuJSCgriCppSUgoKUlIKCgpSEgoKCgoSChKaCpoKCuIKUuoKCgoKCgoKCgoK8goKUgoKmlISWpoKClIKUgoKmgoSmgqaCgriClKaEgoK4lIKCuIKmlJSCgpSUgoKClISCgoKChIKEpoKmgoK4gpS6goKCgoKCgoKCgryCgpSCgqaUhJamgoKUgpSCgqaChKaCpoKCuIKUpoSCgriUgoK4gqaUlIKClJSCgoKUhIKCgoKEgoSmgqaCgriClLqCgoKCgoKCgoKCvIKClIKCppSElqaCgpSClIKCpoKEpoKmgoK4gpSmhIKCuJSCgriCppSUgoKUlIKCgpSEgoKCgoSChKaCpoKCuIKUuoKCgoKCgoKCgoK8goKUgoKmlISWpoKClIKUgoKmgoSmgqaCgriClKaEgoK4lIKCuIKmlJSCgpSUgoKClISCgoKChIKEpoKmgoK4gpS6goKCgoKCgoKCgryCgpSCgqaUhJamgoKUgpSCgqaChKaCpoKCuIKUpoSCgriUgoK4gqaUlIKClJSCgoKUhIKCgoKEgoSmgqaCgriClLqCgoKCgoKCgoKCvIKClIKCppSElqaCgpSClIKCpoKEpoKmgoK4gpSmhIKCuJSCgriCppSUgoKUlIKCgpSEgoKCgoSChKaCpoKCuIKUuoKCgoKCgoKCgoK8goKUgoKmlISWpoKClIKUgoKmgoSmgqaCgriClKaEgoK4lIKCuIKmlJSCgpSUgoKClISCgoKChIKEpoKmgoK4gpS6goKCgoKCgoKCgryCgpSCgqaUhJamgoKUgpSCgqaChKaCpoKCuIKUpoSCgriUgoK4gqaUlIKClJSCgoKUhIKCgoKEgoSmgqaCgriClLqCgoKCgoKCgoKCvIKClIKCppSElqaCgpSClIKCpoKEpoKmgoK4gpSmhIKCuJSCgriCppSUgoKUlIKCgpSEgoKCgoSChKaCpoKCuIKUuoKCgoKCgoKCgoK8goKUgoKmlISWpoKClIKUgoKmgoSmgqaCgriClKaEgoK4lIKCuIKmlJSCgpSUgoKClISCgoKChIKEpoKmgoK4gpS6goKCgoKCgoKCgryCgpSCgqaUhJamgoKUgpSCgqaChKaCpoKCuIKUpoSCgriUgoK4gqaUlIKClJSCgoKUhIKCgoKEgoSmgqaCgriClLqCgoKCgoKCgoKCvIKClIKCppSElqaCgpSClIKCpoKEpoKmgoK4gpSmhIKCuJSCgriCppSUgoKUlIKCgpSEgoKCgoSChKaCpoKCuIKUuoKCgoKCgoKCgoK8goKUgoKmlISWpoKClIKUgoKmgoSmgqaCgriClKaEgoK4lIKCuIKmlJSCgpSUgoKClISCgoKChIKEpoKmgoK4gpS6goKCgoKCgoKCgryCgpSCgqaUhJamgoKUgpSCgqaChKaCpoKCuIKUpoSCgriUgoK4gqaUlIKClJSCgoKUhIKCgoKEgoSmgqaCgriClLqCgoKCgoKCgoKCvIKClIKCppSElqaCgpSClIKCpoKEpoKmgoK4gpSmhIKCuJSCgriCppSUgoKUlIKCgpSEgoKCgoSChKaCpoKCuIKUuoKCgoKCgoKCgoK8goKUgoKmlISWpoKClIKUgoKmgoSmgqaCgriClKaEgoK4lIKCuIKmlJSCgpSUgoKClISCgoKChIKEpoKmgoK4gpS6goKCgoKCgoKCgryCgpSCgqaUhJamgoKUgpSCgqaChKaCpoKCuIKUpoSCgriUgoK4gqaUlIKClJSCgoKUhIKCgoKEgoSmgqaCgriClLqCgoKCgoKCgoKCvIKClIKCppSElqaCgpSClIKCpoKEpoKmgoK4gpSmhIKCuJSCgriCppSUgoKUlIKCgpSEgoKCgoSChKaCpoKCuIKUuoKCgoKCgoKCgoK8goKUgoKmlISWpoKClIKUgoKmgoSmgqaCgriClA==", "", "", "66=base/1/1/2;223=base/1/1/2;380=base/1/1/2;537=base/1/1/2;694=base/1/1/2;851=base/1/1/2;1008=base/1/1/2;1165=base/1/1/2;1322=base/1/1/2;1479=base/1/1/2;1636=base/1/1/2;1793=base/1/1/2;1950=base/1/1/2;2107=base/1/1/2;2264=base/1/1/2;2421=base/1/1/2;2578=base/1/1/2;2735=base/1/1/2;2892=base/1/1/2;3049=base/1/1/2;3206=base/1/1/2;3363=base/1/1/2;3520=base/1/1/2;3677=base/1/1/2;3834=base/1/1/2;3991=base/1/1/2;4190=base/1/1/2;4335=base/1/1/2;4480=base/1/1/2;4625=base/1/1/2;4770=base/1/1/2;4915=base/1/1/2;5060=base/1/1/2;5205=base/1/1/2;5350=base/1/1/2;5495=base/1/1/2;5640=base/1/1/2;5785=base/1/1/2;5930=base/1/1/2;6075=base/1/1/2;6220=base/1/1/2;6349=base/1/1/2;6475=base/1/1/2;6601=base/1/1/2;6727=base/1/1/2;6853=base/1/1/2;6979=base/1/1/2;7105=base/1/1/2;7231=base/1/1/2;7357=base/1/1/2;7483=base/1/1/2;7609=base/1/1/2;7735=base/1/1/2;7861=base/1/1/2;7987=base/1/1/2;8113=base/1/1/2;8239=base/1/1/2;8365=base/1/1/2;8491=base/1/1/2;8617=base/1/1/2;8743=base/1/1/2;8869=base/1/1/2;8995=base/1/1/2;9121=base/1/1/2;9247=base/1/1/2;9373=base/1/1/2")]
[assembly: GoPositionMap("runtime/malloc_stubs.go", "malloc_stubs.cs", "AClUgoKCyqaCqqTcgoKmgoLupoKC3IK4lIKCpu6CuIKCyoKUAAIS4qaigpSClIKUgriCgqaCgpSUgoKCgoKClIKClKaUgoKCAAgSlMoACRQACxiCgpSClIKCuKaigpSClIK4jNKmgoKUlIKCgqaUgoIABxCmgoKUgoIACBKUygAJFAALGIKClIKUgoKmpqKClIKUgriCgqaCgpQAHj6ClIKC7oKCgoKUlIKCgoKCpoKCgoKUgoKmlIIACBKUygAJFAALGIKClIKUgoK4AAwalKrCgoKCgoKCgoKCgoKCuKaigoKCgoKCgoKCgrimgoKUgoKUAAIUAAsElKaCgoLKgpSCgtyCgoKCggAAEgAIApSCgoKCgqaCpoKClOaigoKCgoI=")]
[assembly: GoPositionMap("runtime/malloc_tables_generated.go", "malloc_tables_generated.cs", "AAgQAIMEiAg=")]
[assembly: GoPositionMap("runtime/map.go", "map.cs", "ABUugoKUAAIgAA0CAAIkAA8CgqYAAhoACxYACgSCgoKUpoKCgpQAAiQAEBYACwKCgoKClIKUgqau4oKCpoKqwoKCpqiSgoKCpgAGJgATBIK4AAIcAAsCgpSUqJKClJQAAhgACwKCqLKCqJKokgACGgAMAoKUgoKUqJKosoKUgoKUAAIQ6tKCgoKCgg==")]
[assembly: GoPositionMap("runtime/map_fast32.go", "map_fast32.cs", "AA0gAAQWAAoYAAsWAAoG")]
[assembly: GoPositionMap("runtime/map_fast64.go", "map_fast64.cs", "AA0gAAQWAAoYAAsYAAsG")]
[assembly: GoPositionMap("runtime/map_faststr.go", "map_faststr.cs", "AA0gAAQWAAoYAAsG")]
[assembly: GoPositionMap("runtime/mbarrier.go", "mbarrier.cs", "ABesAgCAAQKClLgACBKCggADFAALCAACEgAMCgACIAAPAoKClIKClIKClKiSqJIAAhoADQK4lKaCggAEHgAMAoKClILugoKCgpSCgpSCgqaCpoLugriCuIIAAiAADwKClAACHAANAriUAAIYAAkCqJKossqUqLKCppQAAiQAEASC")]
[assembly: GoPositionMap("runtime/mbitmap.go", "mbitmap.cs", "AEOYAQAKBgAYTgANAoKCgpQABhYACgKCgoKmgoKUlKiSlIK4gpSmggAGGAAJAoKClIKmggACLgAVBIKWkoKUlJSmpgACFgAIAoKCuIK4goKUuIK4goKCAAQYAAgEgoKUppSCgpTKpoKClKamgoKmgsqClIKClAACENIABloAKgKClIKUgqaCgoKmgoKCpoLulJaSgqaCgpSUgoKCgpSCgqaCgoKUgoKCggAEJgAOAoKUgpSClpKCpoKClJSCgoKUgoK6koKCgpSCgpSCAAkkAA4ClIKClILclJSssoKCgoKowoKCgpQAAhLiggAJFIKCgoKUgpSCgoKmlAACFAALBKaCgoLKgpSCgpSm3IKCgoKCgpSCgoKCgqaCpoKCgoKCgoKmABQsooKUgoKU5rIAESSUgoKUpqIAMWSCgpSmssqCgpSCgoKClIKCgqaCgpTmxIKCgpSClIKCgoKCpoKCgoKUgoK4goKCgpSUgoKCgoKCgoKCgoKUgpTmsoKCgpSCgpSCgoKCgqaCgoKCgpSCgriCgoKClJSCgoKCgoKCgoKClIKmgpSCgoKCgqaClKaU6MKClLiUgoKCgpKCgoKClIKmgoKCgoKCgoKSgoKCgqa4goKCgoKUpq74rviu+AACEgAJCAARJpKCrsKCgoKCgoKCgoKC7sKCgoKUgqaUgpSCgoKUlIKCuIKCgqaClNyClIIAAhgACQKClIIAAiAADQKClIIABhYACAKmlIKClKyypoKCgqiSqMiokqjI6LKCgpSssoKCggALGKiSgoKUlAAMEAALEoKCgoKClJSUgoKClIIAAjYAHAKmgriU3JSCuIKUpoKCrOgABBoACgKCgpSCgoKClIKUlIKCgoKUgoKCpgAJJAAQAoKUgoKUgpSCgoKCgoKUlIKCgoKCzqKCysqClKqigoKClJQADBaygoKCgoKUAAIqABQClpKUgoKmgoKCuIKCgpSUlIKCgoKCgpSCgoKUuIKCgoKCgtyCgoKCgoKmAAgSgoKUgqaCgoKCgu6CgrjugoKUpoKClIKCuIKCgu6CgoKCgoKCyoKCgpTclIKCgoKCuIKCgoKCpoKC6oKCgoKClKaigoKCgoKClIKCgoKClIKUgoKCgoKCgriCgoKCgoKmggAEFuI=")]
[assembly: GoPositionMap("runtime/mcache.go", "mcache.cs", "AEuqAbIACxaCgoKCgoKUgpSUAAIS4oKC7oKCAAMQ1IKCypSUAAwQ5JSCyoKUgqaUgpSmgoKmgoKUpoKmuIKCpoLKpgAOHoKClKiSgpSCgtyUgoKCuIKCgqamuAAIEoKCppSClIKCgoKCgqaCgriU3LiCuIKmgoKC7qbsAAwQgoKCgoKClIKCqqKCuIKCqqKClA==", "99-104:1;120-132:1")]
[assembly: GoPositionMap("runtime/mcentral.go", "mcentral.cs", "AC1ikoKCgoKqoqqiqqKqouikgpSCgoKCAA8glIKmgoKmgpSCgoKUlIKCAAoWgoKClJSUgoKCgqa4lIKCgoK4goK4goKCgoKmgoKUgpS4lO7igqaCpu6muAAHEIKUlLjMkoKCgpSC")]
[assembly: GoPositionMap("runtime/mcheckmark.go", "mcheckmark.cs", "ACdMsqaCgpSUgoKUpqaClIK46JKClAAKDLKCgoKmppSClIKClJSUgqaygoKUlIKCgoIAAhDS7oKCgoKCgoKCggARCqKUnIyCgpSUgoK4griClIKUtLaC7oKClIKClIKUgoKClIKUpoKCgoKCgoKCgpSCyoKClIKCgpSmgpSCgpS4lIKCgoKClKaCgoKCgpSmlIKClAADEtKmgqaUgoKUgoIABxC4goI=", "42-57:1;167-219:1;183-190:1.1")]
[assembly: GoPositionMap("runtime/mem.go", "mem.cs", "AApiACgCgoK4gqYAAhDSgqqiAAIaAAoCgqyyrLKqogACKAAXDoKmgoIAAhgACQKCAAIcAAsCuIKmAAgQ0oKClMqCgoKUpOyCgoKUgoKUpraCggAHEIKClIKClAADHAAQDoKmrsKC")]
[assembly: GoPositionMap("runtime/mem_linux.go", "mem_linux.cs", "AA4qwoKCgoKUgoKUlIIADw6CuKaCgpS0gpSC6IKUgvyCgriCgoKUAAsIgoKCgpSClIK4gpSUlIIACAqCppTmgqaUggAOHq7CpoKCpoKCgpSC5oKCgpSCgpTcgriC")]
[assembly: GoPositionMap("runtime/mem_nonsbrk.go", "mem_nonsbrk.cs", "AAwagg==")]
[assembly: GoPositionMap("runtime/metrics.go", "metrics.cs", "ACx8soKmuIIACxiUlIKmgtyC3ILcgtyC3ILcgtyC3ILcgtyC3ILcgtyC3ILcgtyC3ILcgtyC3ILcgtyC3IKmgu6C3ILcgqaC7oLcgtyCyoLKgtyC3ILcgsqC3MqC3ILegtyC3ILcgtyC3ILcgtyC3ILcgtyC3ILcggAEEILcgtyC3ILcgtyC3ILKysrKytyCyoLKgoK4pqKCyqKCqJKCgoKClJKCABMusoKClKjCgoKUqMKCgpSokoKCpqiSAB1KsqaCgoKCgoKCgoKClIIAFzKygoKCgoKClIKCgoKCggAJFpIAECSSgoKCAAsakoIAELQBogARKMKCgpSCgpSUtLS0tLS2ABxEsoKClIKClIKClAAWNMKCgoKUlIKClJSssqamgg==", "91-94:1;98-101:2;105-108:3;112-115:4;119-122:5;126-129:6;133-136:7;140-143:8;147-150:9;154-157:10;161-164:11;168-171:12;175-178:13;182-185:14;189-192:15;196-199:16;203-206:17;210-213:18;217-220:19;224-227:20;231-234:21;238-241:22;245-248:23;252-260:24;264-267:25;271-274:26;278-286:27;290-293:28;297-300:29;304-307:30;310-313:31;316-319:32;323-326:33;330-333:34;337-340:35;343-346:36;349-352:37;355-358:38;362-366:39;370-373:40;377-380:41;384-387:42;391-394:43;398-401:44;405-408:45;412-415:46;419-422:47;426-429:48;433-436:49;440-443:50;447-450:51;454-460:52;463-466:53;470-473:54;477-480:55;484-487:56;491-494:57;498-501:58;505-508:59;511-513:60;516-518:61;521-523:62;526-528:63;531-533:64;537-540:65;543-546:66;730-737:1", "579=runtime.metricReader.compute", "75=makeStatDepSet/1/53/8,makeStatDepSet/2/53/15,makeStatDepSet/3/53/22,makeStatDepSet/4/53/29,makeStatDepSet/5/53/36,makeStatDepSet/6/53/43,makeStatDepSet/7/53/50,makeStatDepSet/8/53/57,makeStatDepSet/9/53/64,makeStatDepSet/10/53/71,makeStatDepSet/11/53/78,makeStatDepSet/12/53/85,makeStatDepSet/13/53/92,makeStatDepSet/14/53/99,makeStatDepSet/15/53/106,makeStatDepSet/16/53/113,makeStatDepSet/17/53/120,makeStatDepSet/18/53/127,makeStatDepSet/19/53/134,makeStatDepSet/20/53/141,makeStatDepSet/21/53/148,makeStatDepSet/22/53/155,makeStatDepSet/23/53/162,makeStatDepSet/24/53/174,makeStatDepSet/25/53/181,makeStatDepSet/26/53/188,makeStatDepSet/27/53/200,makeStatDepSet/28/53/207,makeStatDepSet/29/53/214,makeStatDepSet/30/53/233,makeStatDepSet/31/53/240,makeStatDepSet/32/53/247,makeStatDepSet/33/53/272,makeStatDepSet/34/53/280,makeStatDepSet/35/53/287,makeStatDepSet/36/53/294,makeStatDepSet/37/53/301,makeStatDepSet/38/53/308,makeStatDepSet/39/53/315,makeStatDepSet/40/53/322,makeStatDepSet/41/53/329,makeStatDepSet/42/53/336,makeStatDepSet/43/53/343,makeStatDepSet/44/53/350,makeStatDepSet/45/53/357,makeStatDepSet/46/53/364,makeStatDepSet/47/53/380,makeStatDepSet/48/53/387,makeStatDepSet/49/53/394,makeStatDepSet/50/53/401,makeStatDepSet/51/53/408,makeStatDepSet/52/53/415,makeStatDepSet/53/53/447")]
[assembly: GoPositionMap("runtime/mfixalloc.go", "mfixalloc.cs", "ADhwwoKUlIKCgoKCgoKCguaCgoKmgoKCgoKUlIKCpoKClIKCgqaCgoKC")]
[assembly: GoPositionMap("runtime/mgc.go", "mgc.cs", "AJcB3gIAEgLmgoKmyqaCpoKCgoKCgoKCrtSCgoKCggAmTpKCACZSAAYU1oKCgpSCpgC8AaYE1gAlTIK8opSCgpSUlILKggAjVrKClJSCpIKUgraUAAYSAAwKgoCCgqSClIC4goKiAA4cypSCgu6CgoKCuIK4lIKCgriUgoKmgriUlIKmlIKClIKCgoK4lAANHMqCAAcQgsqUuKbKgriCgqYAECKUgu7uuO6mgoKCpu6CuIKmogAdWgAYBpSO4oLKuKaCpu6mgoK43IK47oKCgoKC7gAKFoKCgoKCgu6ClNymgpSmgpSCprimuLiCyrjKpq70lKa8zIKCpgAEEIK2rLKc5IKCgpSCgoKCgsqCgoKUrLKUpriC2IIABhCilIKCggAEEsLKlLiUuJSCgu6UgsaC2oKCAAoKtJSClIKCgrjuggAJFIKCgoKApIK4gqaClIKCgriUgri4poKCgoKCgoKCggAJFIK43IKmlIK4goKC3KbKAA0cgoKCgqaUpoKkgoIAChbcuKYADRyClIKCgoKCgqaClJTu3IKUgoKmgpSCgoKClIKUgu6UgoKUlNyClJSClNyCgpSClLiCgoKCgriCgriCpoKmlAADEAAJDIIACRSCgpSCgoIACBK4vAALFIIAHzCCyu6ClJSUABEkpoKUAA8guKYACBKClIKCpoKmgoKCgqaUAAsUgpS0gtyCgoLKtLSkuIKCgoKUgu64uIKUzsKClKiS6qKCgoL+soKCgpQACAyygpSmgoKmptwABxDcppSmgoKCgoKUlIKUlILcAAgSgoKClLgABhYACAKUgqaCgoKCgoKClJSmgoKUgqamgpS4goK4goKClIIAAhoADAaCgsqCgoKCgqaCAAYkAAoCqJKmlIK4gu6CkoKClIK4hrKCgpSCrLKCgoKCgoKClJSCqJKUpoKCgpSCgoKUAAIaAAoCggAOhAEACAKCgoKUgpSCgpSCpoI=", "752-754:1;834-836:2;869-871:3;947-954:4;1042-1059:1;1083-1085:2;1103-1111:3;1128-1135:4;1260-1276:1;1284-1289:1;1365-1374:1;1377-1389:2;1380-1380:2.1;1500-1516:3;1545-1558:4;1548-1552:4.1;1649-1651:5;1791-1818:1;1847-1884:2;2116-2119:1", "", "1526=itoaDiv/1/1/1;1557=Load/1/2/3,Load/2/2/4", "", "676:29,677:21")]
[assembly: GoPositionMap("runtime/mgclimit.go", "mgclimit.cs", "AE+0AcIACBIACAKmlIKmgoIAChTSgtyClIKCAAYSoqqiqqLu4riUgpSC6LKClJSCpoKCuIKCpgAHEIKCgpSCtLS0tLa4goIAFzCUruKC7qaCgoKCgpS4lKaUgrqS6JKCggAKFuKmpoKUgoKCgoKClAAaQJKu5oKClKiSAAckAAoCgpSCAAIQAAkEgoKClJSCuJSCgqYACAyygoKCgoKUgqaC7qaUgrS0tA==")]
[assembly: GoPositionMap("runtime/mgcmark.go", "mgcmark.cs", "ADV60oIAAhLiuIKmlqKmgoKUggAIEq7CppiCpoKCgqaCggAPIIIABxCmpoKmlIKmgoKCggAICqKCggAHEIKCgqaCpqbKAAgaAAsEkoKUgoLIgoLIgoLstqaC2siCgoKClLiCgsragoKCAAkUgoKClIKUgoKUgsiCgoKmAAYS4pTcgoKUgoKCgriCrtSCgoKCgqiSgoKCgoKCgrimgoIACAwAEyKmgoKmgoKUgoKUgoIABxC4goKmlILKgoKUxoK0tsz4yoK4qKSuAAoGgpSCpoC4goKiABAggoKmgoKCAAcQgsqm3IKCgoKCggAIEoKCgoKClIKUlJSmgoKCAAcQgsqmpoKCgqaClO64gsqCgoKmAAcQgoIACxiC3IKCggAHEILKxgACGAAOBpQABxCCAAgSgpSmuIKUAAcQgrjKlIKCgoKClIKCgr6ygoKCrsK4goKmgoLcgoKClIKmggACGgAKAsqCpoKUgoKmlILupoLKgriUgoKUAA4oABMCgoKmlIK0pIL6gqyygpSUuIKClJSmpoKUgqaC3IKokoLuoqaUpsqCtpQABxCCgoKClIKClIKUlIKCgoKClIKUgoKClNyCgoKCgoKClLiClIKUrOKCpoKCgoIAChaCgoKC3JTKpqbKypSmpoKCuIK4uIKCgoKClIKUlIKUAAsgoqqigoKUqqKuwpSCuJSUpgAGQgAgAoLKgoKCgpS4goKCgoKCgrimgoKClIKCuIKCggAPINyCqJKCgoKCuIKCgu6CgoKmuIKCgu6CgoKClIKUgoKC3ISSgoKUAAciAA8Cgsq4gpSCqJKCgoKCuIKClIKCgoKm7oKCgqa4goKCuIKCggAIEgACGAAOCIKUlIKCgpSClIKCgpSCgtyCAAgaAAoCgoKCgoKCgoK4goKCgqaCgpSCgpSClKaCgoKC7oKUgpSCuKYACBKCuIKC7oKCuIKCAAQU0oKCggAKGAALBIKUlIKUlIKmgoKCgoK4gpSmgoLcgoLulIK8ooKCgoKUgoKUpoKCuJS4goKUgoKUgoKUlIIABxYACQKClJS4goKUuIKCpoKsspSCgoKUgoKC", "113-115:1;191-204:1;280-313:1;515-517:1;638-642:2;1536-1562:1", "", "200=readgstatus/1/1/1", "", "558:29,559:21")]
[assembly: GoPositionMap("runtime/mgcmark_greenteagc.go", "mgcmark_greenteagc.cs", "AFS2AdKmAAkUgoKClOaiABEklMqUqqK2gsrIlAACFgAIAqyy6JKCpuyygpSCgoKCgoKUlIK4rLKClKaigpSUgoKmgoKUqqIAAhLiqsKCuIKClIKCgriCgqaCgpSmgoK4goK4gpSUpqiSqLKAgraAgsiClAAbPJKClIKC7AAFFgARFJSClLiCgoKmuIKClKiygoKUrOSClKzEgoKClJTospS4goK4ggADKgARAoKClIKClILs0oKUgoKCgoKUgoKUqqKClIKCrLKCgpTupoKmpu6CpgAHFAAIAoKokoKCgoKCgpSClIKClILKgqYACBIACAKCgpSCpqaCgoKmgoKUADh2woKCgqaCgpSCpoKCgqyygoKosoKCgoKCgoKokoKoABIkgoKClIKCgpSCgpSUrLKUgoKUgoKUggAIEpQACQ6SgpSmgqaC6sKCgoKClKamgoKCgpSCgoKUgoK4goKClJaSgoKUyoKCgpSCAAcQ3pSUgriCgoIABRbygoKmgsqCyoKCgoKCgoKUgqaCgoKUppSmooKCgoKCgoKCgrimgriigoKUgoLcooKigpSCgoKCgpSCgpSCgoKCuKaC3KKCgpSCgtwACBKCgoKClIKUgoKCppQAAhLiggAKHsLOgoKCgoKUgoKCgoKClIKCgpSUgriigoKCgoKUrsKClIKUgpQACBgADwrugoKClKamgqbcgoIABxCCgoKUpoKCgoKC7ri4AAkUgoLKgoKC", "", "", "978=sizeclass/1/1/4,spanPtrMaskUnsafe/1/1/5")]
[assembly: GoPositionMap("runtime/mgcpacer.go", "mgcpacer.cs", "ANQC5gWigoKCggAGFNKCgoKCgoKCgoLugoKCgsqUlJS4goK4goKmyoLcysqClIKCggAIPgAYAoKmlIKCuLjcgoIAChaCAAgSgoKUlIiypgAJFIIACBK4gqYAChaCgoKs9qamlIKmAAgSlIKCAB08gt6CgoKCpoKUgoKCgoKCgoIAAxoACQLcgriC3IKC3IK4gpSCgpSCgoKClIKClIIACCIADwKCpoKmyqYABxC4ggALGKaCgoKCpoLKpoKUgtyClIKmpoIAAh4AEgqClIK4goKCuIKmgoKCgoKUAAISAAgCgoKmgoIAAhDygoKCgoKmgoKCAAcU4pSCtLSCtMiCgoKClIKmpoK4uKKCgpSCgoK4gqiSgq4ACQSmgsqCytwAARwADAKCpqi0soKCgoKCAD5+ppaSgqbKuMqCppSClKaClAAZPgAJAu7K7oIACRSCggALGIKClJaSgoKUlIKCgoKClAACKAATAoKmpsrcgoK4gpQAGDKu4oKmgoKUgpSoxIKCgoLKgqbmgoKClIKUruKCpoKCpqjEgoKCpoKUgpQACAaCgoKUgoKClAAGGgAKAoKCgqaUgoKUgoIABBoACAKCgqiSgoKCgoKUgoIABBTSgoKCgoKUgoIABBwACQKUpoLKgoKCpoKC", "809-820:1;1354-1359:1;1401-1412:1", "", "435=Load/1/1/1")]
[assembly: GoPositionMap("runtime/mgcscavenge.go", "mgcscavenge.cs", "AJIBrAKSAAIgAA0C7rjcgpQACBKCgqaCyqYADBrKgpQAbfgB0oKUgpSCgpQAChYAChamgoKCgoKClIKmgpSogv6ygoKUgqiSrNKClKYAARDigpQABhQACQKCgqbcAAgSuJSC3IKmgqaU3IKUgrimgoKUlMoABxCWkoK4gqa8woKCAAgSAAoCgoKUlJSCAAIaAAwGAAES8oLclMqCpoKmypQAAhDSlIKUgoKCgpSCAAMaAAkCgoKCgpSClIKmAAIU8pSCzIKCgoKUggACIAATDoKCAAcQgoKmgqamlNyCppSmuIKClIKCgsqCgpSCpoKUuIKUAAYWAAgCAA8guJSktLS0tLS0AAsWAAguABQCgoKCgoLugpSmlJSCgqaU3IKCkqbKgoKCgpTugu7ugriUlMqCuAA7hgHSgoKCgqzWgoKUqsKCgpSClLiCgpSUgqaCpoLuppSmggACFgAIAoLcruKCgqaCgu6CggADEvKCgoKUAAIQ0oKCAAgUkqqiABo+kgAHEpIAFzySqJKokqqilJSUlLi46JKCgpSCgpSClPqSgoKUgoKUpgAOPAAMBIKmgriCgpSCgoK4goK4goKmqJI=", "364-366:1;393-402:2;405-409:3;412-414:4;682-684:1;824-841:1", "", "396=Load/2/3/1,Load/3/3/1;715=load/1/2/3,load/2/2/4,heapRetained/1/1/4")]
[assembly: GoPositionMap("runtime/mgcstack.go", "mgcstack.cs", "AIgBkgKCgpSCAA4k1gAjTsKClIKClIKUgoKCgoKCgpSUgoKUggACEPKCgpSUgpSmgoKClKaCpoKClOiygpSCgoKUgpSUgoKCgpSCgoKClKyyAAIQAAgCgpSSgoKCgoKUgoKCqqKCgoKClIKClIKClA==")]
[assembly: GoPositionMap("runtime/mgcsweep.go", "mgcsweep.cs", "ADl+gqamgoIACBKCrsKssoKCgoKCgpSUpoK4ggAVQAAKAoKCgpSCAAoOooKUgoKClIKCpoKCpoIABBbigoKClILMkqyyrLKCAAYS4u4ABxCC7oKCgoIABxCUpoKUgoKCgpQAACIAEAKCgoKCppSUgoK4uIKUggATJKKCpoKmgpTqori4goKCuIKCgoKCgpS4goKUlJSCuMqUppQADyCCgqaCpqaCgqamggACEuLs6IKC3IKUgoKClNyCgoKUABMS9oKCpoKmpoKAgoK2goKCppSCABEkgoKUgoKCpoKCgpSCgqa4poKCgoK4yqaCgoLKgoKCpriCppSCgoKCgoKCgoKmgqaClIKUgpSCpoLKgrjcgoKmgoLcgoKmgqaCgoKCyoKmgri4gIKCpIIADBqUgqaUuILKgri2gpSCgoKUppTKgoKCppTclIKmgpSmlAAHEIKCgqYADyCCgpSUuJQABiIAEAKCgoKCgoKCgoKUlIKUlIKClIKCgoKUlIKUAAIoABEClKaCgoK4goKCgoIADyCUgoKCgpSUuIKCgry0ggADEtKmgu6CuIKUlIKCgoKUgrg=", "429-443:1;751-761:1")]
[assembly: GoPositionMap("runtime/mgcwork.go", "mgcwork.cs", "ABo4goIAZOgB4oKCgpSuwoKmgoKCgpSCgoKCgoKCgriC3IKClAAEEsKCgqaCgq7CgqaCgoKCpoKCgoKCgpSCgqaCgpQABBjygoKCpoKCgoKCgoKUgriCAAIQ0oKCpoIAAhQACQKCgpSClJSCgpSClJSCAAkUgpTKgpSCggADEMKClIKCgoKCgpSmgoKUAAQQ0gAVMMKC+IKCAAMQwoKCgoLKgoKEkoKCgoKClJSCgpSCpoKCuIKCgoKClLiu4oIAAhDygq7CgoKClKjEgoKCgqaC7LKCgsqCgqqigoKCgpSCgoKCgpSCpoKC", "449-451:1;547-557:1")]
[assembly: GoPositionMap("runtime/mheap.go", "mheap.cs", "AOoClAYADCCyrsIAW8YBgqbCgoKClAAIIAANAoKUlIKCgpSCgoKClIKCgpSCgoKmggANIIKokqiSAAIeAAwCqqIABBbippQAAxTigpQAAxTiAAIQ0oKClJSkAAVk4oIAAhLi3IKUquKCgoKCqJKClIK4gqaCgpSosoKUgoKCgoKCgoKCgoKCggAIEriCppQAAhIADg6C3JSCgoKmgoKUgoKUlIKUuIKUgqaUgriCgqaCpoKmgoKClAACEgANDJSCgoKClIKCpoKCgoKCyoKCgqaCgoKCgoKClMrugpSCgoKUgoKmggAKHJIAAhLosqaClJQABigAEQKClKqigoKCgoKCgpQABxwADAKCgpSCggAIEgAIEoKCuIKClJS43IKUAAIeAAwCpoKmgoIAAhoADAKUgpSmgoKClKaCggACGgAMApSUgoKCuAAGKAAVBIKCysqCgqaCgoK4goKCggAIEpSUAAkUgoKCgoKClIKCpoKmlIKCgoKCgpSCgrimlJQAABYACgKCpqaCgqYACBLKgoKUgr6y3IKmmKaCgpS4lIK4gqaCpoKCpoKCgpS0tKSmgoKCgqaq9qa4gpSCgoKCuIKCgoKUgoKClJSUlJSUlKa4goKCgqbKAAsYAAkUlNyCpoK4yq7ilO6UpoKCuIKCgoKmpsq4lIKCpoKmgpTKgoIABxC4gu64goK4gpQABxCCgoK4qJKUgoKCgriClIKClJSCgpSCgpSCAAMeAA4EgoKCgriCgoKClIIADAaylJSCxoKUgoKUpoKmgsbsgoKCpoKUtLSkpqaCAAIUAAoIgqaUlIK6koKAuMSCgoKCgoKCgoKCgoKCgoKCgoKmggAJFpKC5rKClJSClJSClJSCgqaC5rKCgpSCpqaUguaygoKUgpSmlIKq0oK4griCpoKCpgAlUrKCgoKosoKCggAGEgAIAoKC3IKUgpSmgpSCgoKmgqaC7LKCgtyClJSClIKCgoKUgpSCgqzEgoKCgoKUgoKUgpSUAA8iwoKCgoKCgoKCyoKCgqaCuIKUuIKCgqiSgoKUgoIACyAACAqCgoKCgoKClILKppTKggAMHqKs0oKCgoKCgoKUgoKCpoKUgoKUgoKCgoKClJSWgoKUpqamqJKopIKCyoKUlIKUgoKCgpSUloKClKampoKUgpSClIKCAAke0oKCgpSCgoKqopSCgoKUgoIAIkiSqJK4poK4poKCgsqCyoKCpoKm3AAHEIKUyoKopJSCgqamprgAAhDSgoKCAA8IksqCgrimgqaCgoyKgoKCyoKCpoIACRSCggAIEoKClIKC7oKC5oKCgoKU3IKUlJaSgoKUgriCABYyyKrCgoKSgoKCAA8ggoKUgsqUgpSCAA4YsoKCgoKCggAfQKKmgqaCgqqigoKCAAYWAAoClIKCgoK0goKCgrSCgoKCtILGgoK0uIKCgrSCgoK0goKCtIKCgrQACxiCgoK0ggAKFpKqogAcOsKCpoKCpoLqooKmgoLKuIKCuLimgoKCyoKCuIKUggACEuIAAiIADgKCgoKmgpSCpoKCguqigoKCgoKUlIKClKaClJQ=", "1011-1018:1;1378-1380:1;1667-1696:1;1815-1815:1")]
[assembly: GoPositionMap("runtime/minmax.go", "minmax.cs", "AAkSgoKUpoKClKaCgoLoooKUgrimooKUgrim0pS0pKbSlLSk")]
[assembly: GoPositionMap("runtime/mpagealloc.go", "mpagealloc.cs", "AFrKAQAIAAAIFqKokqqiqJKmlLqSgpS8oqqiAAIQABASgoKssoIAeoICwriCgpSmpqampqassoKClKyy7uK4grimyoKSgpSCysqCAAcQgoSSgoKUuIKUypTcAAIgABQQgoKClIKCgoLugqYABBoACAKmgramgoKClIKmpriCgpSCyu6CgsqClLiCpqaCgoKCgoIABRwACAKUgpKkgpSCgoKmgoKCgoKCgoKUgoKClIIAAhLiuIKCgoLKpgAMJgAQAgAbOAAOHgAMHoKmgoKmgoIABxCClIKUgrim3IKCAAIaAAsCgoKmgsqUgriCpoKUuIKCgpTKgoKmlKaClJTcgoKCgoKClAALFIKCpoKCgri4goIAAh4ADwK4gsqClIKCgoKCgpSCgsqCgtyUlITagpQAAhQACQKmgpSCpoKCgqaSpJSCpoKCgoKUgqYAAhgACQKUgpKSgoKCgpSCgoKCgoKCgpSCgoKUABQwkoKUrJKClKiSgpSokoKUqJKClK7qgpTKgu7ugpSm", "713-726:1")]
[assembly: GoPositionMap("runtime/mpagealloc_64bit.go", "mpagealloc_64bit.cs", "AB1CAA4eAAsYAA0W1oKmgoKCuIIACRwADAKCgtyCgtyCgoKCAAgSooIACximlNyCuO6ClIKmgriCgv7SgoKUAAsYgoKCpoKUgriCgrimgoKUgpSCpqyygoKCgoI=", "109-112:1;117-125:2;130-133:3", "", "132=add/1/2/1,add/2/2/2")]
[assembly: GoPositionMap("runtime/mpagecache.go", "mpagecache.cs", "ABcykgACFgAIAoKUgoKCgoKUAAIS4oKClIKCgoIAAhgACwKUgpSCuIKCppSC3IKUggACGAAJAriClIKCgpSCgoKUAAcQgqaClIKCAAgSgoKmpgAJFII=", "", "", "139=chunkBase/1/1/1,alignDown/1/1/1,pages64/1/1/2,block64/1/1/3;156=alignDown/1/1/1,chunkPageIndex/1/2/2,pages64/1/1/2,chunkPageIndex/2/2/3,block64/1/1/3")]
[assembly: GoPositionMap("runtime/mpallocbits.go", "mpallocbits.cs", "AA4ekqiSqJKokoKUgqaCgoKUlIKCpqiSgryiqJKokoKUgqaCgoKUlIKUqJKqoqrCgpSCgoKUgoKClIIACRayooKCgoKCgpSCpoKClJSUhJKUlJS4goIABxCCgsqCgpSClIKClKaCgpSmuIKCgoKCgpTGAAIYAAkCgoKCgpSu4oKCgoKUlAACGgAMApKCgoKCuKaUgoKmgoKUlAACGgAMAqKCgoKClKaUgoKClIKClIKCgpSUgpSokqiSqJKokqiSrLKqoq74goKClIKmgoKUpsoADiLUgqrUgg==")]
[assembly: GoPositionMap("runtime/mprof.go", "mprof.cs", "AJcBtAKygoKCACJAsoKCqtKCgoKCgoLO2IKCgoKCggAOhAKCgpSCgqYAAhYACAIAAhTygoKmgoKCgoKuwoKCgoK4goIAAxAACQyUgoKCgoKoksq4gpSCgpSCgoLckrqSlIKUgoKCAAUW4oKCgoKmgoK4poKCpoKCvKKClAAEbsKCgpKCgoKClJSUlIKCgoKCgpSClMqCuJQAKlaCgpSmgoIAAxQACAKCpriCpriClIKCgoKUpoKmoqamggAJFJSUlIKCgoKSgoKUggADEtKCuMqClIKCgoK4goKUgoKCuKaCpoKClNyUgoKCgpSClAAFGPKClIKCqJKClIKCAAoaooKCpgAgRICWkqqigoKmAAIwABUCgoIAAxwADQK4goKCgoKCgoKCgpSCpsqCgoKCgoKClIK4goKCgoLuuIKmooKCgoKClIKUgpSCqJKCgriCgoKCgpQACiQACgKCkoKUgpSmgoKCgoKCpoKCgqaCvuKCgoKUgoKCgu6ClKaCrNKCgoKUgpSC3IKokoKCAAMUAAkCgpKClIKUrOKCgoKUgoKCgsqmgqiSgoIAAxTigoKCvuKCgpSCgoKCpqiSgoK6kgAEGpKokoKmAAkGABc+gqaCpoKmoqamppSmqJKygqCSgpS4gqamosqmlJSCAAcQgoKUlLiCgriCgrKUgpSC7oKCggAMGpKmgoKCgoK4gqaCpgALGILuwoKUrLLKpoKCppSmggAHEIKCgpQABxYACQIACBIADBqCgqaUuAAKFpCkgriylLaClIKUgpSmgqaCkoK4goLWgoKilKaCgriygqam3JCSgoKUuIKmggACEgAJAoKCgpSCgpSmgqYAChaCpoKCgoI=", "463-465:1;581-589:1;740-744:1;937-940:1;1033-1036:1;1066-1069:1;1151-1154:1;1165-1168:1;1202-1205:1;1215-1219:1;1242-1245:1;1339-1347:1;1341-1341:1.1;1396-1398:1;1426-1428:2;1439-1441:3;1566-1566:1;1576-1589:1;1596-1600:2;1609-1611:3;1621-1641:4;1635-1635:4.1", "", "641=FuncPCABIInternal/1/1/2;859=stk/1/1/5;979=stk/1/1/3;1067=stk/1/1/3")]
[assembly: GoPositionMap("runtime/mranges.go", "mranges.cs", "ACFCsoKClKiSgrioku7CgoKCgoKCgpSssoKClIKssoKClIKqooKUgpTMygAKGpKokqqiqqKqoqi2qJIAChqigoKClILOooKCgoKUggAEEsKqoqqigoKCgpQAGEiimrKSgoKmlLju3IKCpgACENKCgpSClIKUqJKCgpQABJABsoKUgoKCgoKClIKCqqKClIKClIKClICCgoKClIK2gg==")]
[assembly: GoPositionMap("runtime/msan0.go", "msan0.cs", "ABEmgIKCgoI=")]
[assembly: GoPositionMap("runtime/msize.go", "msize.cs", "AA4gwoKUgriClKaCgpQ=")]
[assembly: GoPositionMap("runtime/mspanset.go", "mspanset.cs", "AEumAdSCpoKCgoK4poKCgqaClIKClIKmlKaCAAoWuIKCyqqikpSClIKCgoKUuILuyoKCgu6UgoL8ysqCgrjKAA0clKaUAAoU8oKCgpSC3IKClLiUuLimpoIACRqyrLIABxKSAAsaooKUqJKCAAYSoqiSqJKokgAHEpKokqiSqJLokpSCgpSokgAHEpKokg==")]
[assembly: GoPositionMap("runtime/mstats.go", "mstats.cs", "AKkCogWCgoK4goIAGTqy3AAJFpKmgoKClpqCpoKCgqaCgoIACBKCuIKUmpQAESSCgoKCgpSCgoKUgoKCgoKUgoKClIKCgsqCgoKCgpSCyoKCgoKCggAQIoKCgoKmgoKCgoK4goKCgoKCgoKCgtyokoIABxDClIK4lIKC7oKCgoKmgoKCggACENKUgoKClIIACTTSAAYQ0oKCggAaQvKCgoKClIKCgoKUgoKCADCAAQATAoKClIKmlIIAAiIAEAKCgpSCpgADEOKUggADEMKUABayAcKCggACENbcpoKCgtyCpoKCgqaCgqaCAAkU", "568-570:1", "", "395=load/4/9/1,load/5/9/1,load/6/9/1,load/7/9/2,load/8/9/2,load/9/9/2")]
[assembly: GoPositionMap("runtime/mwbbuf.go", "mwbbuf.cs", "AEycAZKCgriUpoIAAxLSqJIAAjIAFgKClIKCqqKClIKCAAIkABIIuILKggADHAANBIKCuJSUgpSCABAigoKC7pSClIKCuIKClKaCgqaCgpSCuJQ=", "180-182:1")]
[assembly: GoPositionMap("runtime/nbpipe_pipe2.go", "nbpipe_pipe2.cs", "AAgSgg==")]
[assembly: GoPositionMap("runtime/netpoll.go", "netpoll.cs", "AHuQAoKCgpaSAAIUAAkCgoKUgpSClKaCgr6ygoKCgpSCgoKCABg2wgAEIIKuwgAICJKCgoKClIKClIKUlIKCgoKCgoKCgoKUgoKClAAKCLKClIKClIKClIKm1riCgpSUlIKCggACENKCgpSCgoKUAAIS4oKCpoKUgoKC3Ki2urKCgoKUgoKCgqamgpSClIKCgoKUgriClIKCgpSCpoKCgpSCgoKUgsqCkoKUgpSCgpSClOiygoKUgoKCkoKCgoKCgpSCgpSCgpSClAACHAANAoKSgpSClIKUgpSmgoKClILKgpSmgoK4lKaCAAgOwoKCuJSClILKgu6CpoKClAACEuKCgqaCgoKUppSCgpSCgoKClAAKCqKmgoKUlIKUgoKCgpSCgpSCgoKUgoKUgoKUgpSmgqaCpoKokqiSggAICKKCgoqCgriCgoKCgoKCpoKCggACEPKCgoKokg==")]
[assembly: GoPositionMap("runtime/netpoll_epoll.go", "netpoll_epoll.cs", "ABMywqqiggAKBoKCgoKClIKCgpSmgoKCgpSmgqaCgoKCpoKC5oIACkgACAKClIKCgoKCgoKCuJSCgoKCgoK4gpSUgoKCgoKmlIKCgpSM0pKUpoKClIKUgoKCuA==")]
[assembly: GoPositionMap("runtime/nonwindows_stub.go", "nonwindows_stub.cs", "ABAmuA==")]
[assembly: GoPositionMap("runtime/os_linux.go", "os_linux.cs", "ADWEAQANDIKCpoKCrLKCgtyCpqYAABDygoKClIKigoKmgpQAKFDq0riCqqKCgqaClJSUgoKClP6ygoKClIKCggALHNjIgqaCuKaUgoLKgoiygoKUgoKCgoKmgpSCpoKCgriCgtyGooKCAAcQtra2gpSmpIKCgoKUgoKCgpSCgoKUlJSmgoKCgqakpoKCgqaCAAIS4qrCgqaYosqssoIAAhIACA6ygqampqamqKKmgpaSlpKmlpKCAAUUooKCyoKUgoKUpoKqooKCgpSCqqKCgqyyqPyyAAsYlIIABRIABBQACgKCgriClpIAAhQACQKCgpS4pgAMGsrcuAAhhgTigoKmgoKUlIKCgqYABhrCgqyygqiSogAHGvKCgpSCgoKUAAIQAA8MkoKClIKUgoKUgoKUgqaCgpSClIKUgqqi", "94-96:1;183-191:1;549-551:1;974-992:1")]
[assembly: GoPositionMap("runtime/os_linux64.go", "os_linux64.cs", "AAoalqY=")]
[assembly: GoPositionMap("runtime/os_linux_arm64.go", "os_linux_arm64.cs", "AAsWgpTKpA==")]
[assembly: GoPositionMap("runtime/os_linux_generic.go", "os_linux_generic.cs", "ABMsqKKmgqiS")]
[assembly: GoPositionMap("runtime/os_nonopenbsd.go", "os_nonopenbsd.cs", "AAgWyg==")]
[assembly: GoPositionMap("runtime/os_unix.go", "os_unix.cs", "AA0ikg==")]
[assembly: GoPositionMap("runtime/panic.go", "panic.cs", "ACRwABIClLiCggADEvaCggAHQgAdAoKokoLssoKokoKokoKokoKssoKokoKokoKUgoKUgoKUgoKokoKUgoKokoKUgoLokoKokqSigpSUgpSmgoKCgpSUgpSUppKClJSClKaCgoKCgpSmlIKUlKaSgpSUgpSmgoKClIKClJSClJSm1pKCptaSgqbUgoKm1IKCptSCgqaCgqbUgoLqooKUpoKCgoK4poKCgtaSlLS0tKQAAowBAEMCgpSmgoKCuJSClKiS6qKCgpSCgoKCgpSCAAQQ0oKClIKUgoKCgqaClIKCgoKCpoIAAhIACAKClNyCgoIAChaCggACENKCgoKCgoKCgoKUlIKCgpSClJSUgqiSgsqCpoKmgoKEooKCgoKCgpSUlIKCgqaUlIKssoKUgoKCgpQACUTigoLoxLaCpoKClJS0pLSoooKCgpSCpoKUgoKCgoKUAAIQ0oKCgoKCgpSCgoIAFyiClAALGAAKAoKClLiCgoKCgqaCgoKClIKCgoKCgoKUgoKCgqaCgpSUgoKCgpQACBKC7pSCrNLcgpSClLiUgoKmpoIACBKCAAgO4pSCgqaCgriCggAJFIKCuKaClKaCgoKCgqaUppSmgszCgqaCkoKCpoKCgoKCAAcQgqaCpqaCgpSm5oKCgqaCpoKCgqaUgoKUqJKCgoIALVyCooKCgoKCgoKCgpS0gpS0goIACAyClIKokqiSqJKokqiSqJKokqiSqJIABn7qgqamgpQAGSwACQKCgsqCgpSmABMogoKCppSUgqaClLgAI0iCgriCgrjK6qQAAhDSgoKUgsqSgqaUuKamAAIQ0oKCgqaS7pSqooKmprimgqYAAh4ADAKCgty4gqa2goKCgpSCyIKCyIKCgraCAAgU4oKCgpSUpoKCgpSCgoKCgoKUgoKCgqa+lMqCppSuwoLKgoKmgoKUgoKClIKClIIAAhDS3AAKFqaUprgAAhTygoKU", "703-713:1;1003-1038:1;1120-1146:1;1431-1446:1;1463-1484:1;1493-1495:2;1597-1599:1", "", "", "", "749:21,750:13")]
[assembly: GoPositionMap("runtime/pinner.go", "pinner.cs", "ADLeAaKClILKggAIBoKCgoKUgpSUlAAKPsKCgoK4yoKClJSUgoKClIKClKaSgri4goKUkoKUlLi4poKCAAgYsqaCpoKmgqqigoKUgpQABhbCgoKmgqyyrLKmgqzSgoKmgtyCgoK4goKClLyigoKCgoKUgoKCgpSU6sKCgpSCgoKCgpSCgoKUAAYigg==", "202-205:1;216-219:2")]
[assembly: GoPositionMap("runtime/plugin.go", "plugin.cs", "ABUasoKCgoKUlIKUgpSCpoKCgqaIgoKCgoKCgoKCpoKCgsqClIKUgoKUAAkUgoKCgoKClIKClJQACAaigoKCgqaCyoKCgoKClIKmgrqS", "", "", "50=inRange/2/4/1,inRange/3/4/2,inRange/4/4/3")]
[assembly: GoPositionMap("runtime/preempt.go", "preempt.cs", "AFHUAQAfAriokqaCgoKCggAIKAAKFIK4lIL8gtyCggANHOqCuIK4goKmgoKCgpTu3IKCggAG1QHcpoIACtoBgpSClIIACA6ilKaCpIK8tpQAAxLSAAIeAA0YAAwGgriUgpTuuJTugoKCgoKClIKmgrzUAAomABICyoK4griCAA4egriClJQACBKUgriUAAcQpoKCABAmlLiClLaU", "320-333:1", "", "496=HasPrefix/2/3/1,HasPrefix/3/3/2")]
[assembly: GoPositionMap("runtime/preempt_nonwindows.go", "preempt_nonwindows.cs", "AAgUpg==")]
[assembly: GoPositionMap("runtime/preempt_xreg.go", "preempt_xreg.cs", "ADx8goIABhDSlKiSgpSCpoKCAAcQpgAGHAANAoKmgpSSpoKmgoKCgoI=", "122-124:1")]
[assembly: GoPositionMap("runtime/print.go", "print.cs", "ABtW8pSUgoKCgrgABRrygoKCgpSmgoKCgryigpSC3IKCpoKmgqaCAAgGgoKU7oKC3IKC3IKC3IKCporCgqbKgoKClIKCgoKUAAgKgoKCgoKCgpSUgoKCgpSmgqaCgoKClIK0grSCgrSCtoKCgoKCgoKUgqaCpoKUgqaCpqKCgqaCpoI=")]
[assembly: GoPositionMap("runtime/proc.go", "proc.cs", "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", "174-176:1;207-211:2;353-353:1;482-484:1;597-604:1;1279-1284:1;1297-1301:2;1317-1319:3;2112-2128:1;2310-2316:1;5277-5282:1;5298-5307:1;5534-5543:1;5547-5552:2;5981-5990:1;6302-6304:1;6309-6316:2;6407-6423:1;6977-6991:1;7580-7582:1", "", "", "", "225:21,226:13")]
[assembly: GoPositionMap("runtime/profbuf.go", "profbuf.cs", "AHPwAYKmgqaCpoKmggACEOSokqiSrOKCgoKCgoKmgpSClKqigriUgoLcgpSCAAgOooLugpSClIKUlIKCgoKCqsKCpoK4goKCppQAAhDygqaCuKaCgqaClIKmgqaClAAI6AGigpSCrLKCgsqCgpSClAARHMKCpu6CgoKCgoKCpqaCgoKCpoKUpoKCgoKClJSUyoLKlJSCpoKClIKClJSClIKCgriCgpSCgu6CgoKClIK4lAAJFKY=")]
[assembly: GoPositionMap("runtime/proflabel.go", "proflabel.cs", "AAwoABsmgpQAAhgACQI=")]
[assembly: GoPositionMap("runtime/race0.go", "race0.cs", "ABEmgIKSgICikoCAooKCgoKCgoKCgoKCgoKCgoKCkoCAopKAgKI=")]
[assembly: GoPositionMap("runtime/rand.go", "rand.cs", "ACpMooKCpoK4gqa4gqaClKaCgoKCgpSUgpSmggACEAAJCoKCgoKCgpSClIK4goKClOiSgoKUgoKClLyigoKUgqyyAAIUAAsKgoLKgoKUgoK6kqiygoKUgoIAAhDkAAIsABMCuIaCggAHEIKCgoKCAAIqABICAAImABEEAAIWAAgCqJKokg==", "", "", "52=allZero/1/2/3,allZero/2/2/3")]
[assembly: GoPositionMap("runtime/rdebug.go", "rdebug.cs", "AAkUsoKCqLKCgoI=")]
[assembly: GoPositionMap("runtime/retry.go", "retry.cs", "AAgc0oKCgpSU")]
[assembly: GoPositionMap("runtime/runtime.go", "runtime.cs", "ABw+soKCggATVAAXBIKCuIKCgoKCuIKmlIKmlIKClKaU3oCWgJaAmKIAChKSgoKCqJKCgoIAChaigoKCgpSCgoKUgqaClKaCgoKClIKUgrqSgoKCgoK6koKCggADEMIADTjsgqYAFi6ClAAJHsA=")]
[assembly: GoPositionMap("runtime/runtime1.go", "runtime1.cs", "AB1MAA0CgoKCgoKCppSUAAUWsqaCgoKmgoKUgoK4uIKCpoKCuIIAFA6CgoKClIKUgoKUgpSClIKClIKUgpSClIIAUAiCAA4eAAAQlIKUgpSClIKUgpSClIKUgpSClIKUgpSClIKUgpSCpoKCgpSCpoKClIKmgoKUgqaCgoKmgoKCpoKClIKmgoKUgqaCgpSCpoKClIKmlIKmggBLnAEAJEqUgoKCAAgSlKaClIKCgsqmlIIADh6C+IKCgpSCqqKUlJSCggAIHgAKAoKClIKClLiCgpSClKaCgpSSgpSC3IKCpoKCgoKCggAHEIK6koKkpKSkpLSCgoKUtIKCyoKmlKyygoKokoKClAADJgASAoKCgoKClAAEHAALAgACIgAOAgACHAALAqyyrLKssoKCgoKUgoKCgoKUgqiSqJI=")]
[assembly: GoPositionMap("runtime/runtime_boring.go", "runtime_boring.cs", "AAkUpIKU")]
[assembly: GoPositionMap("runtime/runtime_clearenv.go", "runtime_clearenv.cs", "AA0ckoKUqJKUgoI=")]
[assembly: GoPositionMap("runtime/rwmutex.go", "rwmutex.cs", "ABx4AB4ClILM+oKUlIKClIK4goKCgoIACQ6ygoKmlIKCgpSmqKSClJSClJKCgoKm+sSCgqaCgoKCgoK4gpQ=", "80-96:1;131-136:1")]
[assembly: GoPositionMap("runtime/secret.go", "secret.cs", "AAwekqiSgqiSgqjsgriCpsqCzvyCgoKCggACENIAAhYACAKCrgAIBoKCgoKUgoKC", "39-54:1")]
[assembly: GoPositionMap("runtime/secret_asm.go", "secret_asm.cs", "AAgS")]
[assembly: GoPositionMap("runtime/security_linux.go", "security_linux.cs", "AAkSuII=")]
[assembly: GoPositionMap("runtime/security_unix.go", "security_unix.cs", "AAsagpSCAAgSAAgGgoKCgoKmgg==")]
[assembly: GoPositionMap("runtime/select.go", "select.cs", "ABk0kpaCpoKCgoKCgsoACBKCgoKUuPy4AAYWwoLulJSClKaCAAwkAA8CgoLKgpSCgoKssoKClJKClKaCggALGIKCgqaCgqaCgqamgqaCgoKUgpSCpsqClIKCgoKUlIKCgoKCgoKClIKUgoKClJSmgoKCgtyUAAcWkoKCgoKCgoKUgoKClIKUgqaClIKUgoKUgsqCgoK4gpSCgoKCgoKCpoKCgpSUgpSClKaCysqCgpSUgoLcgoKClIKCgpSUgoKClJSCgoKCpoKClKaCgoKmgqaUgqaCgqamgoKCgqaCgoKCpoKCgoK4gpSEkoKUlIKUgpSCgoKUgoKClIKClISSgpSClIKUgoKClIKClISwkoKUgpSEkoKClIKUlISSlIKUgpSgkoKUlIKClJSEkqaCABMukoKUgoKSgoKClIK0grSCtoK4griCgqaCgoKCgpSmpoKUlKaigoKClIKCgoKmgoKClJSCgoLKgoI=", "147-152:1;497-497:2;527-527:3")]
[assembly: GoPositionMap("runtime/sema.go", "sema.cs", "ADtwggACGgAKAqiSAAIaAAoCqJKokqiSqJKCgpSokqiSpoKClAAHFpIABHiCAASuAYKCgoKUgszCgqaCgoKUgoKClJSCgoKCgoKClIKmgoKClIKClIKCgqaClJSCgoKmlIKClAAOHoKCpoKClIKUAAQYAAsCgpSCgqaClKaUgoKClJSCgoKCgpSCgpSClJSCgsqCgoKCpoKClLiCgpSmlKaCgoKCgoLq1IKClIKCgoKmgoKCgpSClPzUgoKUgoKCgqaCgoKClIKUABg6ogACEPau4qaCgriCpoKCgoKCgoKUgpSUgoKCuIKC7PaCyoKCgtyCpoKCgoKClIK+9oKmpoKCgrgADh6CgoKClJSClIKCgoKUgqbokoKCupI=")]
[assembly: GoPositionMap("runtime/signal_arm64.go", "signal_arm64.cs", "AA8gooKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgqqQlIKClgAJDoKC3JSUlLiCpsqCgrimgg==")]
[assembly: GoPositionMap("runtime/signal_linux_arm64.go", "signal_linux_arm64.cs", "ABAmkJSCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCmJCUgpSClIKCgpSCpoKCgoKCgpSCgrSCgg==", "78-89:1")]
[assembly: GoPositionMap("runtime/signal_unix.go", "signal_unix.cs", "ABo4kqaCgpQAPH6mgoIABRbSlMqCpoKCgsqUpoKCgpSmgrzYlILIuKaCgsqCpgAIqAHygoIABBLClIIADRyClIKmgoKCgpTugoKCAAUSsqaCgpSo5oKUyoKUggAFGAAIBoKmgoIABxCmggADEvKk7oKCgoKUtgACHAALAoKUgoKCgqaClJQABxCClJTKgpSUgpSmlpKCgqaCpoKUgqaCggAKLAAMAoKmgoKClLgAAhLigrgAAxYACgKCgqaCgoKCgqYAChaCgoLKgraWkoKCgpSUABEikqiSAAIcAA8GgoIACxiUpoKUpoKmgqbKgqaU3IKClO6UppQACBKCgoKUgqaCgriCpoKmgu6CpoKUgqaUgoKCgpSCgoKClKaCgpSCgpSUAAwagpS4goKCgpSUgriCgoKCgqaCppSmsoKUpoKmgoKUgoKmlIrC7oKClIKCgpSUggAKKAARAoKCppSCpoKUgrSCpoKUuJSUtJS0pLaUlAAETtKUpoKCgpSCuIIAChaCAAsYgqbKAAcQqJKqooKUgoKCxoLKgoKCpoKCtILGgraCAAsQsoLssoKCggAGENKCAAYQ0riCgpSCppSoAAIS8oKUgqaUgriCgoKmgtyCuIKmyoLugoK4gqYAAhYACAIAAhYACgLqgsoADRyCgqYAAhoACgKCgpQAAhTygoKqooIAAhoACgKCgoKCgpSCAAMYAAgCgoKCpq7CgoIABxAAAxoACQKCgpSClIKUAAsoAAoCgoKCgoKUgoKCggACEPKCgoKCrNKCggACEuKCgoKC", "1074-1104:1", "", "485=FuncPCABIInternal/1/1/2")]
[assembly: GoPositionMap("runtime/sigqueue.go", "sigqueue.cs", "AEWOAbKCgqamgoK4goKCgpSCyoKClLSC2LSCgoKUgvqC7sKUgoKCyoKClLSCgoKUgoLGggAIDIIABBwADgyC3IK+spSCgpS4gqaCgpSCgpSssoKUlIKCrLKClJSCgpSCggACENKCgqyygg==")]
[assembly: GoPositionMap("runtime/sigqueue_note.go", "sigqueue_note.cs", "ABAcguaC5oI=")]
[assembly: GoPositionMap("runtime/sigtab_linux_generic.go", "sigtab_linux_generic.cs", "AAgS")]
[assembly: GoPositionMap("runtime/slice.go", "slice.cs", "ACE6guaCqsKSgoKCgpTKgqaCgoKCuILuuIKCgpSClIKmlAACGAALAoLcgoKUpqaCgoKmgoKmAAZeAC4CgoKClIKUgqaCpqamlILqgpSCgoKCtIKCgoK0gpSUlIKCgoKCtIKCgoKCABAggqaCgsqmgu6mlAACIgAQAoKCAAsYgqaokoKCgqaCgpS43ILcgpQAAhgADQaCyoKCgpSCpoKokoKmgoKmgqaCgoKCgpSCgpSCgqaUlJSokoKUgqrCgoKUlIKCgoKqooKClJSCgoKs0oKClJSCgoKCgoKokoKClJSCgoKCgpSqwpSUgqYACxjugqaqwoIACBKClA==")]
[assembly: GoPositionMap("runtime/softfloat64.go", "softfloat64.cs", "ABo64oKClJSCgpSCuIKCgoLsgqSm4oKClJSCgpSCuIKCgoLsgqSmgqKClIKClIKCgpSCgoKCgqaClIKUgoKmgoKCgpSClIKCgqamgqKClIKClIKCgpSCgoKCgqaClIKUgoKmgoKCgpSClIKCgqamgoKmlKampqamgpSmqIKmgoKCgoKClIKCpoKUpoKmgqaCgqaUpqampriCgoKCpoKCppSmpqamlNaCgqaCgoKUgoKUgqaCgoKClIKClKaCgpSUpqam3Ka4pqKUlKamgpSClKaCgpSCgpSCgpSmgoKCgpSUgoKCgsqCgoKCgqaqopiCgoKCgoKCgoKqopSCuIKCgqaCgoKCgoKClIKCgoKCuIKClIKCgoKCuKaCpoKmlKaCgqaCgqaCgqaCgqaCgqaCgqaCpoKmgqaCpoKCpoKCpoKCpoKCpoKCgpSCgqaCgoKUgoKmgoKmgoKCgqaCgqaCgoKC")]
[assembly: GoPositionMap("runtime/stack.go", "stack.cs", "AKsB1AKCgpSCgpSCgrqSgpQACgqigoKClIKClIKUgpSCgoKCypSClJSCgpSCgpSU6JKCgpSUlIKCggAPIIKCggADEMKCqqKCgoKCgoKUgoKokoKUgoKCgoKCgpSCgqiSgpSCgoKCgoKUgoIADRQACQiCgpSClIKmgoKCgpSssoKCgoKClILKgoKUgoKCgpSClLiUlIKCpoKCgpSUlJSCgpSClKaCgoKCpoKUgpSClIKUgpQAChLigoKCgpSClIKClIKClJSUgoKCgqaClIKUgpSCgoKCgpSCgoKmlIKUgoKUuJSCgqaCgoKUpoLugoKC7pQAK2CigoKClAAHEJSCgoIACyDCgurCgoKC3IKCgoKmgoKCgoKCgqaCgpSCgpSCgoKmAAoQxIKClKaCgoKC7pQAGDKmuJSCgqbGgoK4goKUyoKCgoKCgpSCuJSCgoIACg6igoKUgoKCgoKmgoK4goLK2IKCgoK4pqamgoK4goK4goKCgoKmrLKCqJKCAAkUlLiasoKCgoK4goKClKYACgrCgpSCgpTKpoKClIKokoKmgoLKlAAIEri4yoKCgoK4goKClpKCpoKClMrclJSClKiSgoKUABYeAAwClIKUgoKCgqaC3KaUgoKCkoKCgpTKgoKmgoKCgsoADRyCgqaCuIKUgpSUgriCgoKmgoKUgqamgriUgri4gsqAgoKCgoLIuKaCgpSUgsq4goKUgqiSqsKCgpSUrgAKCoLKgsqCAAwagpQABHakgoKCgoKCgoKClJS4goKCgoKCgqYADiTSgoKUgoKmgoLcgu7CAAgSgoIAARgACgKCgoKUgpSCgpSmgpSCpg==")]
[assembly: GoPositionMap("runtime/stkframe.go", "stkframe.cs", "AESQAZKCuIIABh4ADwKCgoKm2pSCppQACxiCgpSUgriCgoKUgqaCgsgACArigpSmgoLKgpS4uIKClLSkgoKCgoKmgpSClIKClIK6koKmgoKClJSClIKUysyUgoKCgoIACBIACQqCoqaCgoKCgqaClA==", "", "", "306=alignUp/1/1/1,getGCMask/1/1/4")]
[assembly: GoPositionMap("runtime/string.go", "string.cs", "ABw40oKCgoKCgpSClIKClILcgpSCgoKUrLKmgqaCpoKs0oKCgoKUlJSmgoKClJSCgoKmrLKmgqaCpoIAAhIACAK4lILKgpSClIKCgpSmgoKUlIKqooKCpsKCgpSUAAIiAA4CgsqClIKUpqKCgoKUlIKmxoKCpoKCgpSmgoKClKaCgsqClIKUgoKClIKClIKUlAANHIKmsoKCgpSUgpSCAAQasoKCgqaCqLKClIKCgqaC6LKCpoKmgpSCrLKCgpSCgqyypoKClIKCAAIgAA4EgqaCgoKClMqCppSCgpSCgriClIKUtLS0xpSCgpSCgpSClJSClJSokoLcgoKCgpSu1IK4lIKUgqaCgriCgpSCgoKUqJKCgqaCgoKCgpSCgpSClJSC", "", "", "157=GetCallerPC/1/1/2,FuncPCABIInternal/1/1/3;224=GetCallerPC/1/1/2,FuncPCABIInternal/1/1/3;280=GetCallerPC/1/1/2,FuncPCABIInternal/1/1/3")]
[assembly: GoPositionMap("runtime/stubs.go", "stubs.cs", "AAswAAwCrAAEIAAPKAAXCKIAAkgAIgaSAAJCAB8GkgAFJAANLAAUAoKuwoIAAhgACgSUgoIAAVwALRwADAKClKgABRwACyIAEAakAAEYAAsEmrKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKClJqyrLKs1qikltYAAhDmAAIWAAoEgoKCgoKCgpas")]
[assembly: GoPositionMap("runtime/stubs2.go", "stubs2.cs", "AAwgtJSClpKu1qas")]
[assembly: GoPositionMap("runtime/stubs3.go", "stubs3.cs", "AAgU")]
[assembly: GoPositionMap("runtime/stubs_arm64.go", "stubs_arm64.cs", "AAkUkpaknMKY")]
[assembly: GoPositionMap("runtime/stubs_linux.go", "stubs_linux.cs", "AAoWntKC")]
[assembly: GoPositionMap("runtime/stubs_nonwasm.go", "stubs_nonwasm.cs", "AAgUgA==")]
[assembly: GoPositionMap("runtime/symtab.go", "symtab.cs", "AEWgAbKCggAEhgIACwIAAiAADQKClIKCAAIgAA8GgpSClIKUpoKU3KaUgoKmlKYAAhDSgpSUpoKCAAkUgpTugpQADRyCpoKmuIKCgoKUgoKCpgB8pAIACgKCgpQAAioAEgKCgoKUgoKCgoKCAAwagoKCgrgAGzqCggARIAANBIKElLiClIKCgoKClIKClIKUuIKCgoKmlIKCpoKCggAILgASAoKCgpSCgqaCgqamlAAGEuKCppSCgoKUgpSUlIKUgoK4qJKClAAVsAGigoKClAACEuKCgqYABxCCpoKokgACGAAJAqgAAiIADwKCgpSUgoKmgqaUgpSCpoKmggAMGoKClAACFPKClKgADyjCAAoIlrakgqyygoKCgsqCgtyCgoKCgpSCgrimlIKmgoKClJSCgoKCgoKCgoKU7oKCgqaCgoKCgoKC7oKmlMqCppSCgoKCgoKClKaCpoKClKaCgoKCgqaCgqamgoKCpoKmAAIYAAwCgoKUgoKUlIKmguaCgoKClKiSgoKCgoKCgoKClLiCpoKClIKmgoKUgqiSgpSqooKUgoKGooKUgoKotoKClIKClIKUgoKClIKCqJKigoKCgoKUlAAMFOiClA==", "", "", "189=gostring/1/2/3,gostring/2/2/4")]
[assembly: GoPositionMap("runtime/symtabinl.go", "symtabinl.cs", "ADKGAQAQAoKClIKCtoIABxCCqJKCgpSCqJIAAhwACwKClIIABxIAAhAACAKC", "", "", "79=pcdatavalue1/1/1/4")]
[assembly: GoPositionMap("runtime/synctest.go", "synctest.cs", "AC1WAA0SgoKUgrSCtoKUgoKCxoLagqaCgoKClIKmuIKUgpSCgoK8woKC6LKCgoKUgoKCurKCAAoWgoKUlJS4puoADwzSgqaCgpTcgoKClIKSuIKigoKCgqaCgoKCxoKCgpSCgoKUgoKUgpSUlKaCgqaUgoKClJSUprT+gqaigoKUlJSCqJKCgpS4goKUgoKUgoKClIKCuIL4ooKUlIKCqJKokoKClKiSqLKCgpSCkpSiAA4wAAoCgqbcgpSUpoKmgoKUlKaCgoKCgoKCgoKmpoKUrsKssqyy", "192-194:1;198-204:2;210-217:3;353-355:1", "", "221=Add/1/1/1", "", "234:21,235:13,413:21,414:13")]
[assembly: GoPositionMap("runtime/sys_arm64.go", "sys_arm64.cs", "AA0WooKUgoI=")]
[assembly: GoPositionMap("runtime/sys_nonppc64x.go", "sys_nonppc64x.cs", "AAgS")]
[assembly: GoPositionMap("runtime/tagptr_64bit.go", "tagptr_64bit.cs", "AD1+ooKCgpSokqaUgpSokg==")]
[assembly: GoPositionMap("runtime/test_stubs.go", "test_stubs.cs", "AAgS")]
[assembly: GoPositionMap("runtime/time.go", "time.cs", "AA8iwoKC7pSokoKClKiSAEaiAeKCggAhUpKUpNq4gqYACRCUABo6goK4ooKUgoKCpoKUpoKC+pKC6LKmgqrCgsoABhQACQKCgoKClIKUgoKCppSCgoKCpgAJJPKCpoKCgoKCgpSUgoKUlIKClIK4gpQAAxDiggARIOKCgoKClIKCgoKCpoKUgoLu4oKUAAYQ8oKUgpSssgAGEAAIAqaCpoKUgoKCggADEvKCAAcQgIKClAAJEuKCgqaCgoKUgoKCgqaClKbugqaCgoKCuOrCgoKClIKCgpSCgoKUgpQADRDigpSClJSCpoKClIKCgoKCgqaCgoKCgqaCuIKmgriUAAoWgoKUgpKClJS4pu6CpoKCgpSmgpSCpgAICsKCgoKUlAAIKgAdFIKCgoKClJSUgoKCgoKCgriUgoKCgpSCgoKCvrIACA7igoKCgoLugu6CgoKCgoKCgpSCpoKCppSmgoKClAAIFAAIAoKCuIKCgoKUgqaCgoKCggAHEvKC3IKCgoKUADVsgpSCgoKCgqaUpoKUtoKCgoKCgoKCtoKCpKaClJSCAAMUAAsMgoKClAAGGgAOAqaClKaC3IKClJSUpoKCgpSCgpSUAAcQgoKmlAAGFgAKAoKCgoKUgoKCppSmgoKCpoKmlIKmgoIADhIACQKCgoKUuIKCgpSUgpSmgqaCgoKCgpSCgqaUgoKCgoKClKaClIK4lJSCgpSClLiCppSCgpSCpgASJpS4griCyJSCpoKCgqaUpoKmgoIACQ7SgoKUuIKCgqaCgrzCgoKUvKKCgoKUggAEELKmgoKmpoKmlAAEHgAKAoKClIKClIKCgpSClIK8ooKCgpSClIKClIKCgpSCgoKCgqaClIKUgrzGgpSCAAcQwgAKEAAIApSUpqaCgoKUgpSClIKCAAcQwoKUpoKCgqbKgoIACBKCgoIABxTigoKCgpSCuIKU", "485-487:1;638-643:1;1239-1239:1;1465-1467:1")]
[assembly: GoPositionMap("runtime/time_nofake.go", "time_nofake.cs", "ABBAAA8CABAosoKU")]
[assembly: GoPositionMap("runtime/timestub.go", "timestub.cs", "AAw0AA8Cgg==")]
[assembly: GoPositionMap("runtime/timestub2.go", "timestub2.cs", "AAgU")]
[assembly: GoPositionMap("runtime/tls_stub.go", "tls_stub.cs", "AAgU")]
[assembly: GoPositionMap("runtime/trace.go", "trace.cs", "AMECrAcACQLKgpSCmAAAFO6UgqaCgqYAChaCgoKClIKCgpSClIK4lMoACBLcgoKClAAJFILcpoKCyoKCgrimAAkUgoKCgu6CgoLuggASJoKCgpSCgpQAARDilIKUgoKClIKC3IKCgoKmpoKCpoKmpoKCgoKmAAkUgoKU3IKUpu6CgoKmgoKCppSm7oKCyoKCyoKCgpSCgpSCpoKCgqaClKYACxiCgoKCgpQABxCUuIK4griUrLKClIKUggAJWgAIApSClKaA7JS4goKmgoKCpoKCgsqClIKCgriC7pSCgoK4poLcuJS47oLclJSmggAKFoKCpoKCggAGEuKCgpSsAA8agpQACxjEgoKClKaUupKCgoIAEiqSgoKCgoKUggACEPKCgoKUgoKUgoKs9oKClNwACAqClAACFAAKBIKClIKmlIKUgg==", "494-540:1;508-536:1.1;564-570:2;591-604:3;675-725:4;761-765:5;793-816:6;931-931:1;1102-1111:1;1145-1147:1", "", "", "", "754:25,755:17")]
[assembly: GoPositionMap("runtime/traceallocfree.go", "traceallocfree.cs", "ABg0wpiigoKUuIKCgqamgoK4goKmpoK4goKCgpTKkpSmooKUqLKosqiSqLKssqyyqJKokqiSgqiSgqiSqJLokoKU", "79-81:1")]
[assembly: GoPositionMap("runtime/traceback.go", "traceback.cs", "AG/2AQAKDgAIBsQADRymgoKCgoKmgoKCyoKCgoLKgoKClIIABxDcgqaCgoKClIKUgpSmAAcQgqaCAAYwABcCgpSCpoK4gsqUuMrcgu6CgoKCgoKCgtgABxCClIKCgoK2gpTKlIIAGTSCgpSUgoKCgqaCgsqClAATKIKmAAoWgoKCAAoWyoIACAaigoKmgoKUgsqCgsqUgoKUgpSCgqaUgoKmgoKUuIKCgoKCuIKUgoKCgoKCuOyyAClUgoKCggADHAAKApSmrsKmpoKCgoKCpgACGAALAoKCgoKmgoKUggAHEIKUuIKmqJKCgqaCgoKCpoKClIKUgqaSlIKCgpSmgoK4goKCpoKCgoKCgpS0goKCtLSCtIK0goKCgoK23sSCgpSCgpSClKiSgqqigoKUgqa0goKCuIKCgoKUgoKClIKCgpSmggACFgAKApSClKa07oKCgpSmlIKClKaCggAuXoKCgoKCgpTKgoKCgoKCppS4gpSUgpSCAAMSAAsGgpSUgoKUgqaCpoKCgoKCgoKCgoKCpoKCgqaCyoKCgpSClIKCgoKUgqa4goKCgoKCgoKCpoKCgriUgpSCuKqigoKCgqaCpoKmAAMQwoKCgoKCgpQABDSCgoKqooKClKqigpSmggAOHoKm7oKmqrSCgpS4goKUgoKUgsqqxqYAEBqilJSClpKClLiCqJKClIKCgoKUpoKClIKUgKykgpSClICCpIKCgoKCgoKmpqaCpoKmgoKCggAJFJKClIKUgpSClIIADRyCgpQABBDSgpSUgpSCppSClIKmgpSCuIKCgoKUgoKUgoIABBwADASCgpSClKamlJSmppSUAAjIAgChAQKCpoamgoKUggAEFAAIEKampgAaOJKCgoKUlKaAkoKCgpSUggACEPKCgoKCgqaCuJSUgoKUgoIABBCygqaUgpSClKyygqaUyoKUgpQ=", "675-684:1;686-701:2;704-708:3;906-929:1;953-968:1;1296-1296:1;1317-1351:1;1388-1401:1;1688-1688:1", "", "218=guintptr/1/1/2;1807=noescape/1/1/2")]
[assembly: GoPositionMap("runtime/tracebuf.go", "tracebuf.cs", "ACBWAAsClIKCgqYAAhgACQIAAhIADAqmgoKUgqaCgoKUAAIS4qaUgqaCggAEGgAIAoKClAACEuKCgoKUlIIACAiSgoKClIKCgpSCgoLKgoKUgoKmgoK4gpSClIKCgoKqogACHAALAgAHErKCgpSUqJKCgpSCgpSCpoIAEzTiggACEuKCgoKCgoKUgpQAAhYACAKCggACEuKuwgAGGAAJAoKClJSClIIAAxLyAAkUgriC", "144-150:1;157-173:1")]
[assembly: GoPositionMap("runtime/tracecpu.go", "tracecpu.cs", "ABIcsoK4ggAHEILuwoKmgoKSAAoWlIKUlIKCgqaU7LKCAAkUgoKCgqamgoKCAAIgAA0ClIKCgpSClIK4goKCgpSCgrimypSmuIKCgriWkoKUuKaCgoKCgpSUqrSAgpKCgoIABRDyppSmAAcQgoKClIKmgpSmisKmlJSClKaUprimpoI=", "44-70:1;201-206:1")]
[assembly: GoPositionMap("runtime/traceevent.go", "traceevent.cs", "ABxQAA0CgpSClKyyqLKssgACEOQABRKyrLKokg==", "", "", "80=startPCForTrace/1/1/2")]
[assembly: GoPositionMap("runtime/tracemap.go", "tracemap.cs", "ADyAAaIAAhLigpSUgoKCgoIADyCClILKlIKCpoIABTLCgoI=")]
[assembly: GoPositionMap("runtime/traceregion.go", "traceregion.cs", "ADJWkoKClIK4goKCgrqSpriCgoKCgoK4griCgsqCpoKUruKCgoKClIKClA==", "63-98:1")]
[assembly: GoPositionMap("runtime/traceruntime.go", "traceruntime.cs", "ABMqkgAdRraCgoKCgoKCggACEuIAHUAAHUAABRKyrsKokgARLtKClAACFAAKCKbKgoIACBIACRSCgoKClIIAAhDSAAIQ0oKUlKyyqJKokqyyuKi2rsKmrsKmrsKmqLaotgAGFAAJBoKClAACFgAIAoKCgoKUAAcS0oKClIKClKiSqJKosoKCgpSssoKCgoKCvrKokqiSqJKuwqjEgqrUgoKCgpSqwri+xIKCAAIYAAkCgoKCgpSUrOSC7qYACBKCgoKClKiSqJKmlJQAAhIACgbKrtSC3AACEuKClJSCrLKClJSCAAIS4oKUlIKUtLSUgqyygpSUggAGEuIACBKClIKCgoKCpoK4lA==", "699-712:1")]
[assembly: GoPositionMap("runtime/tracestack.go", "tracestack.cs", "ABtAAAoClpKCgriClMq0gpS0gsqmlILugoKCgriCgoIAECKCgpSC7oKUlJTKgpSClIIACRjGgpSCrLKCgoKUgqaiuIK4ntKCgriCgoKCgoKCuIKCgpSUqqKCgoKCgoIACxqSgpSCgoKUgoKCgpSCqqKu4pSUlKiSAAIYAAkCgoKmpv6CgoKClKiCgoKCpoKUpoKCgpSClMassoKClIKClA==", "306-314:1")]
[assembly: GoPositionMap("runtime/tracestatus.go", "tracestatus.cs", "ABEo9IKCuIKUuIKUAAYYAAsCgpSClIKmxtyCxqSCAAYYAAoEgoK4poKUAAgWAAgEkpS0tAAKEoKCxrSkABY84oKUgqiSgoKokqqiqJKC")]
[assembly: GoPositionMap("runtime/tracestring.go", "tracestring.cs", "ABo2xIKClIKmqJKCpoKClAACEAAIBIKmgpaSgpS4goKCpoKu4oKCgoKUuA==", "33-35:1;47-49:1;92-96:1")]
[assembly: GoPositionMap("runtime/tracetime.go", "tracetime.cs", "AC50AAsCgpQABCKCpqamgqaCgoKUgoKCgoKCppKCgg==", "107-111:1")]
[assembly: GoPositionMap("runtime/tracetype.go", "tracetype.cs", "ABc04oKmkqyygoKUgqaigqae0oKUuIKCgoKCpoKCgpSU")]
[assembly: GoPositionMap("runtime/type.go", "type.cs", "AB0ikgAJLIKmgoKUgoKCgpS0pJSuwoKUlIKkgpQAAhDylJQABxQACw6UgqaCgpTagsaCuIKCgriCAAsc1LiCppSCgoKUgoKCpoKmgoKCgpSCpoIACAoADBKCgpSUgriUgqaClIKCgsaCgoKCgpS4gpSUlIKCxgAaMIKCgriCgpQACAaCgpSCgoKCgoKUyoKCgoKCgpSUpoIACAaCppSCgoKCgqaCgoKCgoKClJSUgpSCgoKUpoLmgqaUgoKCgoKmgoKCgoKCgpSUlIIAAiqCgpSCgoKClIKCgqqigpSUgoKUgoKCgpSmgoKCpsa4goKCgoKCgoKCprgACBKCAAYeAA8CgoLclIKUgoKUgoKUgoKCgpSCgoKmgpSUpIKCpIKCpIKCgpSCgoKmgoKCpqSCgoKUgpSCgqaCgoKUgpSCgoKmpIKCpIKCpIKCpIKCgpSClIKCgoKUgpSClIKUgqakgoI=", "135-137:1")]
[assembly: GoPositionMap("runtime/unsafe.go", "unsafe.cs", "AAwagoKmgoKUupKCgpTmgriC+ILmgqiSgqaCgriCgoKUupKCgpTmgriCuKbokoKmpuiSgqiS")]
[assembly: GoPositionMap("runtime/utf8.go", "utf8.cs", "ACRWkoKClAACGAAMApSCppSmgoKCguqCgoKC6oKCgoLaqrSkgqSCgoKkgqSCgoKCpIKCgoKC")]
[assembly: GoPositionMap("runtime/valgrind0.go", "valgrind0.cs", "AA0egoKCgoKCgoKCgg==")]
[assembly: GoPositionMap("runtime/vdso_linux.go", "vdso_linux.cs", "ADmCAYCCACFMsoKUmKKCgoKUgoKCgsjIgsqSgoKCgoKCgpS0tLS0tMiCpoKmlIKCgoKCpoKCgrimooKmgoKCgoK4gpSmpqKCpoKCgpSClIKmgqaCppSCgoKCyriCgoKUgoKUgqaU3IKUlJSmgoLOsg==", "216-234:1")]
[assembly: GoPositionMap("runtime/vdso_linux_arm64.go", "vdso_linux_arm64.cs", "AAwctA==")]
[assembly: GoPositionMap("runtime/vgetrandom_linux.go", "vgetrandom_linux.cs", "AA0gABMUgoKmjIKUgoKU5oKCgoKCgoKCgoKUgoKClIKClIKmgoKCrLKCpoKCrLKCyoKCAAwalIKCgoKClJQ=")]
[assembly: GoPositionMap("runtime/write_err.go", "write_err.cs", "AAgUkoI=")]
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
    [GoLocalName("cgoSetTracebackFunctionsArg")] internal partial struct SetCgoTraceback_cgoSetTracebackFunctionsArg {}
    internal partial struct _typePair {}
    internal partial struct activeSweep {}
    internal partial struct addrRange {}
    internal partial struct addrRanges {}
    internal partial struct adjustinfo {}
    [GoLocalName("pollDescPadded")] [GoValueClone("pad")] internal partial struct alloc_pollDescPadded {}
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
    [GoValueClone("stats")] internal partial struct consistentHeapStats {}
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
    internal partial struct elfDyn {}
    [GoValueClone("e_ident")] internal partial struct elfEhdr {}
    internal partial struct elfPhdr {}
    internal partial struct elfShdr {}
    internal partial struct elfSym {}
    internal partial struct elfVerdaux {}
    internal partial struct elfVerdef {}
    internal partial struct errorAddressString {}
    internal partial struct errorString {}
    internal partial struct finalStatsAggregate {}
    internal partial struct find_firstFree {}
    [GoValueClone("subbuckets")] internal partial struct findfuncbucket {}
    internal partial struct fixalloc {}
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
    internal partial struct itimerspec {}
    internal partial struct itimerval {}
    internal partial struct kernelVersion {}
    internal partial struct lfstack {}
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
    internal partial struct mOS {}
    internal partial struct mProfCycleHolder {}
    internal partial struct mSpanList {}
    internal partial struct mSpanState {}
    internal partial struct mSpanStateBox {}
    [GoValueClone("buf")] internal partial struct mTraceState {}
    internal partial struct mWaitList {}
    internal partial struct markBits {}
    [GoValueClone("alloc", "reusableNoscan", "stackcache")] internal partial struct mcache {}
    [GoValueClone("partial", "full")] internal partial struct mcentral {}
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
    internal partial struct perThreadSyscallArgs {}
    internal partial struct persistentAlloc {}
    internal partial struct piController {}
    internal partial struct pinState {}
    [GoValueClone("refStore")] internal partial struct pinner {}
    internal partial struct pinnerBits {}
    internal partial struct plainError {}
    internal partial struct pollCache {}
    internal partial struct pollDesc {}
    internal partial struct pollInfo {}
    internal partial struct profAtomic {}
    internal partial struct profBuf {}
    internal partial struct profIndex {}
    internal partial struct profᴛ1 {}
    internal partial struct ptabEntry {}
    internal partial struct quoted {}
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
    internal partial struct schedStatsAggregate {}
    internal partial struct selectDir {}
    internal partial struct semTable {}
    [GoValueClone("pad")] internal partial struct semTableᴛ1 {}
    internal partial struct semaProfileFlags {}
    internal partial struct semaRoot {}
    internal partial struct sigTabT {}
    internal partial struct sigactiont {}
    [GoValueClone("regs", "_pad", "__reserved")] internal partial struct sigcontext {}
    internal partial struct sigctxt {}
    internal partial struct sigevent {}
    internal partial struct sigeventFields {}
    internal partial struct siginfo {}
    internal partial struct siginfoFields {}
    internal partial struct sigset {}
    [GoValueClone("mask", "wanted", "ignored", "recv")] internal partial struct sigᴛ1 {}
    internal partial struct sizeClassScanStats {}
    internal partial struct sliceInterfacePtr {}
    [GoValueClone("path")] internal partial struct sockaddr_un {}
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
    internal partial struct timeval {}
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
    [GoValueClone("uc_stack", "_pad", "_pad2", "uc_mcontext")] internal partial struct ucontext {}
    internal partial struct uint16InterfacePtr {}
    internal partial struct uint32InterfacePtr {}
    internal partial struct uint64InterfacePtr {}
    internal partial struct unwindFlags {}
    internal partial struct unwinder {}
    internal partial struct updateMaxProcsGState {}
    internal partial struct userArena {}
    internal partial struct userArenaStateᴛ1 {}
    [GoValueClone("__val")] internal partial struct usigset {}
    internal partial struct vdsoInfo {}
    internal partial struct vdsoSymbolKey {}
    internal partial struct vdsoVersionKey {}
    internal partial struct vgetrandomAllocᴛ1 {}
    [GoValueClone("reserved")] internal partial struct vgetrandomInit_params {}
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
    [GoValueClone("V0", "V1", "V2", "V3", "V4", "V5", "V6", "V7", "V8", "V9", "V10", "V11", "V12", "V13", "V14", "V15", "V16", "V17", "V18", "V19", "V20", "V21", "V22", "V23", "V24", "V25", "V26", "V27", "V28", "V29", "V30", "V31")] internal partial struct xRegs {}
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
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸruntimeꓸcgroup() => builtin.initPackage(typeof(@internal.runtime.cgroup_package));
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
