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
[assembly: GoDynamicTypeLift("7374727563747b676350657263656e74476f616c20696e7465726e616c2f72756e74696d652f61746f6d69632e55696e7436343b206d656d6f72794c696d6974476f616c20696e7465726e616c2f72756e74696d652f61746f6d69632e55696e7436343b2061737369737454696d6520696e7465726e616c2f72756e74696d652f61746f6d69632e496e7436343b206261636b67726f756e6454696d6520696e7465726e616c2f72756e74696d652f61746f6d69632e496e7436347d", "scavengeᴛ1")]
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
[assembly: GoDynamicTypeLift("7374727563747b6c6f636b2072756e74696d652e6d757465783b2072656164696e67202a72756e74696d652e74726163654275663b20656d707479202a72756e74696d652e74726163654275663b2066756c6c205b325d72756e74696d652e747261636542756651756575653b20776f726b417661696c61626c6520696e7465726e616c2f72756e74696d652f61746f6d69632e426f6f6c3b2072656164657247656e20696e7465726e616c2f72756e74696d652f61746f6d69632e55696e747074723b20666c757368656447656e20696e7465726e616c2f72756e74696d652f61746f6d69632e55696e747074723b206865616465725772697474656e20626f6f6c3b20656e644f6647656e65726174696f6e5772697474656e20626f6f6c3b20646f6e6553656d61205b325d75696e7433323b20737461636b546162205b325d72756e74696d652e7472616365537461636b5461626c653b20737472696e67546162205b325d72756e74696d652e7472616365537472696e675461626c653b2074797065546162205b325d72756e74696d652e7472616365547970655461626c653b206370754c6f6752656164205b325d2a72756e74696d652e70726f664275663b207369676e616c4c6f636b20696e7465726e616c2f72756e74696d652f61746f6d69632e55696e7433323b206370754c6f675772697465205b325d696e7465726e616c2f72756e74696d652f61746f6d69632e506f696e7465725b72756e74696d652e70726f664275665d3b20637075536c656570202a72756e74696d652e77616b6561626c65536c6565703b206370754c6f67446f6e65203c2d6368616e207374727563747b7d3b20637075427566205b325d2a72756e74696d652e74726163654275663b2072656164657220696e7465726e616c2f72756e74696d652f61746f6d69632e506f696e7465725b72756e74696d652e675d3b206d61726b576f726b65724c6162656c73205b325d5b345d72756e74696d652e74726163654172673b20676f53746f70526561736f6e73205b325d5b335d72756e74696d652e74726163654172673b20676f426c6f636b526561736f6e73205b325d5b31375d72756e74696d652e74726163654172673b20656e61626c656420626f6f6c3b20656e61626c656457697468416c6c6f634672656520626f6f6c3b2067656e20696e7465726e616c2f72756e74696d652f61746f6d69632e55696e747074723b206c6173744e6f6e5a65726f47656e2075696e747074723b2073687574646f776e20696e7465726e616c2f72756e74696d652f61746f6d69632e426f6f6c3b2065786974696e6753797363616c6c20696e7465726e616c2f72756e74696d652f61746f6d69632e496e7433323b2073657147432075696e7436343b206d696e5061676548656170416464722075696e7436343b2064656275674d616c6c6f6320626f6f6c7d", "traceᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6c6f636b2072756e74696d652e6d757465783b207265757365205b5d72756e74696d652e6c697665557365724172656e614368756e6b3b206661756c74205b5d72756e74696d652e6c697665557365724172656e614368756e6b7d", "userArenaStateᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6c6f636b2072756e74696d652e6d757465783b2073656564205b33325d627974653b20737461746520696e7465726e616c2f6368616368613872616e642e53746174653b20696e697420626f6f6c7d", "globalRandᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6c6f636b2072756e74696d652e6d757465783b20737461636b2072756e74696d652e674c6973743b206e6f537461636b2072756e74696d652e674c6973747d", "schedt_gFree")]
[assembly: GoDynamicTypeLift("7374727563747b6d63656e7472616c2072756e74696d652e6d63656e7472616c3b20706164205b32345d627974657d", "mheap_central")]
[assembly: GoDynamicTypeLift("7374727563747b6e6f74652072756e74696d652e6e6f74653b206d61736b205b335d75696e7433323b2077616e746564205b335d75696e7433323b2069676e6f726564205b335d75696e7433323b2072656376205b335d75696e7433323b20737461746520696e7465726e616c2f72756e74696d652f61746f6d69632e55696e7433323b2064656c69766572696e6720696e7465726e616c2f72756e74696d652f61746f6d69632e55696e7433323b20696e75736520626f6f6c7d", "sigᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b70656e64696e6720696e7465726e616c2f72756e74696d652f61746f6d69632e426f6f6c3b20656e61626c656420626f6f6c3b20646f6e6520626f6f6c3b20636f756e7420696e747d", "workType_goroutineLeak")]
[assembly: GoDynamicTypeLift("7374727563747b726f6f742072756e74696d652e73656d61526f6f743b20706164205b34305d627974657d", "semTableᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b72756e74696d652e6d757465783b2072756e74696d652e70657273697374656e74416c6c6f637d", "globalAllocᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b73656d612075696e7433323b2061637469766520626f6f6c3b206f666673657420696e7465726e616c2f72756e74696d652f61746f6d69632e496e7436343b207265636f726473205b5d696e7465726e616c2f70726f66696c657265636f72642e537461636b5265636f72643b206c6162656c73205b5d756e736166652e506f696e7465727d", "goroutineProfileᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7369676e616c4c6f636b20696e7465726e616c2f72756e74696d652f61746f6d69632e55696e7433323b20687a20696e7465726e616c2f72756e74696d652f61746f6d69632e496e7433327d", "profᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7370696e41667465725261676765644261727269657220696e7465726e616c2f72756e74696d652f61746f6d69632e426f6f6c3b20726573746172746564447565546f323739393320626f6f6c7d", "gcDebugMarkDoneᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b737461746573205b5d75696e747074723b207374617465734c6f636b2072756e74696d652e6d757465783b20737461746553697a652075696e747074723b206d6d617050726f7420696e7433323b206d6d6170466c61677320696e7433327d", "vgetrandomAllocᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7573657220626f6f6c3b2072756e6e61626c652072756e74696d652e6751756575657d", "schedt_disable")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<PanicNilError, Error>(Pointer = true)]
[assembly: GoImplement<TypeAssertionError, Error>(Pointer = true)]
[assembly: GoImplement<boundsError, Error>]
[assembly: GoImplement<errorAddressString, Error>]
[assembly: GoImplement<errorString, Error>]
[assembly: GoImplement<plainError, Error>]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<gclinkptr, hex>(Inverted = true, ValueType = "uintptr")]
[assembly: GoImplicitConv<hex, abi.PCLnTabMagic>(Inverted = false, ValueType = "uint32")]
[assembly: GoImplicitConv<lfstack, hex>(Inverted = true, ValueType = "uint64")]
[assembly: GoImplicitConv<limiterEventStamp, limiterEventType>(Inverted = true, ValueType = "uint64")]
[assembly: GoImplicitConv<spanClass, traceArg>(Inverted = true, ValueType = "uint8")]
[assembly: GoImplicitConv<sweepClass, spanClass>(Inverted = true, ValueType = "uint32")]
[assembly: GoImplicitConv<taggedPointer, hex>(Inverted = true, ValueType = "uint64")]
[assembly: GoImplicitConv<traceArg, tracev2.GoStatus>(Inverted = false, ValueType = "uint8")]
[assembly: GoImplicitConv<traceArg, tracev2.ProcStatus>(Inverted = false, ValueType = "uint8")]
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
[assembly: GoFieldAccess(typeof(go.@internal.abi_package.InterfaceType))]
[assembly: GoFieldAccess(typeof(go.@internal.abi_package.Method))]
[assembly: GoFieldAccess(typeof(go.@internal.abi_package.RegArgs))]
[assembly: GoFieldAccess(typeof(go.@internal.abi_package.StructField))]
[assembly: GoFieldAccess(typeof(go.@internal.abi_package.StructType))]
[assembly: GoFieldAccess(typeof(go.@internal.abi_package.Type))]
[assembly: GoFieldAccess(typeof(go.@internal.abi_package.TypeAssert))]
[assembly: GoFieldAccess(typeof(go.@internal.abi_package.TypeAssertCache))]
[assembly: GoFieldAccess(typeof(go.@internal.abi_package.UncommonType))]
[assembly: GoFieldAccess(typeof(go.@internal.runtime.syscall.linux_package.EpollEvent))]
[assembly: GoFieldAccess(typeof(godebugInc))]
[assembly: GoFieldAccess(typeof(hchan))]
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
[assembly: GoFieldAccess(typeof(ucontext))]
[assembly: GoFieldAccess(typeof(uncommontype))]
[assembly: GoFieldAccess(typeof(unwinder))]
[assembly: GoFieldAccess(typeof(updateMaxProcsGState))]
[assembly: GoFieldAccess(typeof(waitq))]
[assembly: GoFieldAccess(typeof(wakeableSleep))]
[assembly: GoFieldAccess(typeof(workType))]
[assembly: GoFieldAccess(typeof(workbuf))]
[assembly: GoFieldAccess(typeof(xRegState))]
[assembly: GoFieldAccess(typeof(Δslice))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("runtime/alg.go", "alg.cs", "ABUugsqUpoKmgqaCpoKokoKCAAg2ABQElAABHgAOBIKCAAIQwoKUpKS4goKUpKS4goKmgoKmgoKCgpSCypSClAADHAAKAoKCgpSUlIKUAAMyABcClJSkpKaUpKSkpKSCgpSkgoKUpIKCgpSUyMqSpoKUgpSClIKUgpSClIKUgpSClIKUgpSCgoKUgoKClKKClIKClLiUlKKClIKCgpSUlAACHAANAsaigqaixqIAFSqUqIKUgoKUgriClIKCupKCgpSmgoKClA==")]
[assembly: GoPositionMap("runtime/arena.go", "arena.cs", "AF7MAQAIAgAGEgAIAoKClIKCgoKCgqyyrLIAEYgBgoKUgpSCgqaCpqrqACSQAsKCgoKCgpSUAAgKtIKCypSCgoKUpoKUgoKCgoKUgpSCgqaCggAoONKCgpSUlIKUpoKU3IKClIKUlIKUgoKmgoKmlIKClIKmgoKUlIKClIKUAAkUlIKUAAgM0oKClIIAAxDygpSClIKCggAHEAAHEIK4lpKCAAwWosqmpoKUysKUgoLKgoKmgoKC7oKCqJKClIKCgpSqwsqmgoKClIK4gqaCgoKUgrjulIKCgoKCgpSClNyigoKUlAAQDLKCAAgSgriCgpSClJaSgpSClNyCppSmlKamgoKmgoKCpoKUpoKmgqaClAAHEIKmAAQaAAoCAAgQ8oKUggAOHsrK3NyCgoKCgqamgqaUgoLakoKClAAIFAAJAoKUgsqClIKUgpSC3KaCgoKClIKCuIKmgoKUAAoYAAsCgqaCgoKCpoK4lIKClNyCgoKUlIKUgpSUAAcQgqbKgoKCggAIEqaCuIKUgoKmgoKmprgAChaClKa4uIKCgpQ=", "764-766:1;912-917:1")]
[assembly: GoPositionMap("runtime/asan0.go", "asan0.cs", "ABIogIKCgoKCgoI=")]
[assembly: GoPositionMap("runtime/atomic_pointer.go", "atomic_pointer.cs", "AA1AABUCgoKCrLKClIKUAAIQ0gACENKClIKUAAIQ6KKClIKUqKiigpSClIKoqKKClIKU")]
[assembly: GoPositionMap("runtime/cgo.go", "cgo.cs", "AEaaAeAABRDQAAkUlIKCgpQ=")]
[assembly: GoPositionMap("runtime/cgo_mmap.go", "cgo_mmap.cs", "ABc+4ozSgpSClJSmgoKAkpSoqLao", "39-41:1;52-52:1")]
[assembly: GoPositionMap("runtime/cgo_sigaction.go", "cgo_sigaction.cs", "ABMuAAoOgpSClIIABxCUlIKClIKm6gALFILIgri4gpSCAAMQ", "72-74:1")]
[assembly: GoPositionMap("runtime/cgocall.go", "cgocall.cs", "AGncAfKCggALKgAQAoKmgqaCpoKmAA8g7pTulLiClLiUlLiCpqaCgoKCggANHIKClAACEuKUuKaCygAVLIKCgozSAAgSgoKCpoLssoKCgqaCgoLulLgAChaCgoKCgoKmlIKmyoKUpqaCpqaUpoKCgqailIKCpoIABxCCgoKUlIK4ysqCgriClJKmgriCgoKmgqqigoKUgqaUgpSCgoLcoqSCpoKC7oKCggAIEpT6kuiSACnUAfKUppS0goKClIKUgoL8tIKUgpS0goLKgpSCgpSClLSCgoKClIKUgpSCgsaCgpSCxoKCgpSClIKClMaCgoK4gpSCpgAEEPKCgoKClIKCgoKUgoKmpoKm3AACEuKCpoKmgoK4rsKs0oKmgoKClAAYCqKSgoKmlLS0tLS0tLS0tMiCAAcQgoKUpoKUypSCgrS2", "416-420:1", "", "", "456:21", "432:34,434:17")]
[assembly: GoPositionMap("runtime/cgocallback.go", "cgocallback.cs", "AAcWog==")]
[assembly: GoPositionMap("runtime/cgocheck.go", "cgocheck.cs", "ABAyAAgCuJSClILKgoLKgsqC3IKmgoIAAxgACgIAAhYACgKClIKUgpQAAhYACgKClIKUgpSCgoIAAxQACQSClIKmAAIS4oKCgoKCgoKCgoKUlIKUgoKC", "65-68:1")]
[assembly: GoPositionMap("runtime/cgroup_linux.go", "cgroup_linux.cs", "ACVOnLKClKaUgoLugoKCuKqiggADEAAOFoKClJSmqJKCgoKCgqY=")]
[assembly: GoPositionMap("runtime/chan.go", "chan.cs", "ADx+kuaCgqYACAaCpoKUgqaCgq7CppTYgsaCtoKCgoKUlIKUAAIcAA0CruaUpqyyAA4eAA4CgoKUgqaCpoKmggASJoKmgoKmlIKCpoCmgJK2lIKClIKCgpSCgqaCgriCgoKCuIKCgoKCgoLKgoKClMqmgpSCgoKCgpSCgoKClJQAAhIACQKCgpSCgsqCgoKClKaCgpSCgoKCgpQAAhAADAyClIKCgoKCgoKUlIIAAhoAEQyCpqbogoIACgaigpSCpoKCgqaCgoKmlKaCgoKUgoKUgpSCgoKClLiCgoKUgoKUgoKCgpSUpoKCggADEPSCuIKUrLKosoIABhIADQiCpoKClIKmgqaCuAAJFMrKlIKUgpS4goKmlIKCgpSCgpTKgMqAksiUgoKUgpSCgoKUgoKmgoK4goKCgriCgpSCgoKCgoLugoKClKaClIKUgoKClIKCgoIAAiIAEQKCgpSCgpSU7oKCgqaCpoKCgpSUgoKCgoKClKb8uNyCAAImABACAAImABACqJKokqaigpSCgpS4lKaigpSCgoLKlKiSqJKokqiSxqKCgoKCgoKUgoKmgoKCgpSCgoKUgoIAChaClLjY3MaCgoKCrAANEO6CgoKUgqaClA==", "232-232:1;607-607:1")]
[assembly: GoPositionMap("runtime/checkptr.go", "checkptr.cs", "ABAStILugsqCvKKCyoKUygAIBoKC3IKCpoKCuAACKAASBAAIEriCuIKClIK4")]
[assembly: GoPositionMap("runtime/complex.go", "complex.cs", "AAcSooKClKaCyIKCgoKUgoKCpriClJSCtoKCgraCgoLI")]
[assembly: GoPositionMap("runtime/coro.go", "coro.cs", "ACNU0oKCgoKSgoKCuIKCgqaCgqzSgoKUgqKoooKCgq7CgoIACh4ADgKCgoKCAAcQgoKCgoKCAAkUlIKCuIKmpqbcgriCpriqogAKFoKClIKCgoIABxCC3ILcggAIEoKmpoK4goK4gqaCuA==", "47-60:1")]
[assembly: GoPositionMap("runtime/covercounter.go", "covercounter.cs", "AAsasoKCgoKUyg==")]
[assembly: GoPositionMap("runtime/covermeta.go", "covermeta.cs", "AA8cwoKClA==")]
[assembly: GoPositionMap("runtime/cpuflags_amd64.go", "cpuflags_amd64.cs", "ABMsAAcQgoKClII=")]
[assembly: GoPositionMap("runtime/cpuprof.go", "cpuprof.cs", "ADuIAQAIBIKUgqaCgoKCgqaCgoKCgoKCgoKUAAQUAAgElKaCgpTKpgACGgAQDpSmgoKCgpSmAAIQ5IKCgpSmgoK4gqaCgriCAAUcAAkCAAIaAAoCAAIoABECgoKCgoKUgoKCgpQ=", "", "", "180=FuncPCABIInternal/1/2/1,FuncPCABIInternal/2/2/2;190=FuncPCABIInternal/1/2/1,FuncPCABIInternal/2/2/2")]
[assembly: GoPositionMap("runtime/cputicks.go", "cputicks.cs", "AAgW")]
[assembly: GoPositionMap("runtime/create_file_unix.go", "create_file_unix.cs", "AAoYkg==")]
[assembly: GoPositionMap("runtime/debug.go", "debug.cs", "AA3qAQANCpSCgoKUpqbcgoKClAACENIACDqSAAIwABYEgoKmggADFgAIBIKCpoIABhrigoKUlA==")]
[assembly: GoPositionMap("runtime/debugcall.go", "debugcall.cs", "ABoygt7kgpTKqqKCgoKCppQAAx4ABxCCgriClIKUpgAGGAAJAoKCAAcQuMiCgri4goLcgpSCgty4uJSCpoK4lIKClNzcgoKCppQABxSigoKCpqaCgoK4goLcgriUgoKUgoKClIKCgoKUuKSSgpSCgoKCpoKi", "49-95:1;123-161:1;164-188:2;222-259:1;269-274:1", "", "", "", "271:21,272:13")]
[assembly: GoPositionMap("runtime/debuglog.go", "debuglog.cs", "AC18ABIEqqIAHsIBAAgGxIKCuKaCuAAVKoC2soKUlMiAtpLIgLaSyIC2ksiAtpLIgLaygoLIgLaSyIC2ksiAtpLIgLaSyIC2ksiAtrKCgsiAtrKCgsiA9sKCgpSClLS2yIAAB0aAtrKCgoKCgqaCgoKUgoKmyIC2soKCyIC2soKCgpQAM2SSlO7MkoKCqJKCgoKCgoKokoKCqJKCgoKokoKCgoKCgrqSgoKUlKiSgoKCgpSCggAPHqSClIKUgoKUgpSCqJKqkoKCgpSmlIKCgpSCgpSCpoKCuIKUgoKCgoKCgqaClKb2goKmgoKUpoKCgoKCgoKmpoKCgoKUlAAGzAGSgg==")]
[assembly: GoPositionMap("runtime/debuglog_off.go", "debuglog_off.cs", "AAwagtqCpoI=")]
[assembly: GoPositionMap("runtime/defs_linux_amd64.go", "defs_linux_amd64.cs", "AGjQAZKCAAsQgg==")]
[assembly: GoPositionMap("runtime/dit.go", "dit.cs", "AAsakoKCgqiSgoKC")]
[assembly: GoPositionMap("runtime/env_posix.go", "env_posix.cs", "AA0SgoKClIKCpqqigoKCgpSUlKaCgpQAGDSSgpSCqJKClIKmgoKC")]
[assembly: GoPositionMap("runtime/error.go", "error.cs", "AC5U1IKCgpSCgpSCgoKUgpSmlAAEEtKCgoKClIL8lIIACxCUggACENIACxCUggATJAAMGgAMFrSCgoKUgoLGooKCuIKCgoKCgpSClLS2AAgeAAgClLS0tLS0tLS0tLS0tLS0tLS0xMqygpSUgoK0tLS0tLS0tLS0tLS0tLS0tMqSgoKClIKCgpSuwoK4goKUgoKUgoKClIKUgoI=")]
[assembly: GoPositionMap("runtime/extern.go", "extern.cs", "AK4C6gQADAKCgoKUggAKSAAIAoKClAAMILI=")]
[assembly: GoPositionMap("runtime/fastlog2.go", "fastlog2.cs", "AAcc8oKUpoKClII=")]
[assembly: GoPositionMap("runtime/fastlog2table.go", "fastlog2table.cs", "AAkS")]
[assembly: GoPositionMap("runtime/fds_unix.go", "fds_unix.cs", "AAwSgrimAAUQgoKCgqaCgqaCgoKCgg==")]
[assembly: GoPositionMap("runtime/fipsbypass.go", "fipsbypass.cs", "AAkUkqiSqJI=")]
[assembly: GoPositionMap("runtime/float.go", "float.cs", "AA0elqSokqiSAAIS4oKqooKokqqiAAIaAAoCgpSCgoKUlIIAAhoACgIAAhoADQKClIKklKaCpoKUgoI=")]
[assembly: GoPositionMap("runtime/hash64.go", "hash64.cs", "ABAqgpKClKSCgrSCtIK0grSCtIKCgoKCgoKClJSCgpSCtqaCgqaCgqaCgqaCpoI=")]
[assembly: GoPositionMap("runtime/heapdump.go", "heapdump.cs", "ADeeAYKClIKCgqaCgoKUgriipoKCABIqkoKCgoKClIKCpoKClLqSgqaCgoK4gqiygsqCgpSClIKUgtyClKaCgoKCgpSCgoKCgpSokoKCgqaCgoKmooKCgoKCAAwcsoKCggAICrKmgoKCgpS4lJSCgpS4goKCgoKCgoKCgpSmgqaCgsqUgoKUlIKCgpSmlKaCgoKCgoKUlKaiooKCgpSCgqaCgoKCgoKCgoKCgoKClpKCgoKCgoK4goKCgoKCgoKUlJSUgoKCgoKCgoL4gqaCgsqClKSC3MqigoKCgoKmlLiCgoKmgoKCpoKUgoKUgoLcAAoOlJSCgpSCgoKCpoKCuIKCgpTKgoKCgpSUgpKCgpSCgpSCgpSCuIKCgoKmooKCgoKmgqaCgoKCgrqSyoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKClKaCgoKCgoKCgoKCgoKCgoKClIKCgqaCgoKCgoKCgpSCgpSCgqaCppSUgoKClIKClIKCgoLKpIKmgoKmgoKCgoKCgoKCgoKmgpSCpqamgoKCpqiyggAIBpSCgoKUgoKClKaCgoKCgoKUgpQ=", "412-433:1")]
[assembly: GoPositionMap("runtime/hexdump.go", "hexdump.cs", "ADaoAYKCuIKCgoKClIK4goK4poKCAAkIgpSCgoKmgpSCgriUgoKUuMqUgoKmAAoGgpSCgoaigoKClIKCgqaUlIKmgpSCgoK4gpSCgqaClIKCgpSmgoKmpoKCgoKUlIK4goLqooKCgoKUlNiCgoKUgoKUgg==", "175-179:1")]
[assembly: GoPositionMap("runtime/histogram.go", "histogram.cs", "AGHSAQAJBIKCAAESAAgCgoKUgriCgqaCqMK4goKUAAcQgoKmgoKsspSUpqa4gpSUyqa4goI=")]
[assembly: GoPositionMap("runtime/iface.go", "iface.cs", "ABcmggAJFJQABhgADAKCuIKClIKm3IKCuIKCgriCgtyCgoKCgoKUggAHEMr4goKCuIKClIKUggAJDAAICoKmgsqC3IKCppSmyvaCgoKCgsqUyoKClIIABR4ADQKCgtyCgoK4goKCgoKCgoKCgpSCgoKCgoKClIKCgoKClNy0gpSmgoKCgoKmrsKqwoKClKqiAB0ypIKCgoIAARgACwKClIKUgpSCgpS0gpSClIKmgoKmooKCgqaClKaigoKCpoKUAAIYAAsCgpSClAACGAALAoKUgpQAAhgADQSClIKUpoKUlKaCgpSs0oKCgqamgsqUppSmuMqUpqKmgoKC7oKmgoKCppKCgoKCgpSmgoKmlMqe8qaCpoKCgoK4gsq4ppS4uMqUrNKmgoKC7oKmgoKCppKCgoKCgoKUpoKCppTKAAEYAAoCqJKmpoKCgoIAChzC7LI=", "539-549:1;645-656:1")]
[assembly: GoPositionMap("runtime/lfstack.go", "lfstack.cs", "ABkwooKCgoKCgsqClIKUgoKClIKCggAHEJS6ggAICqKClKaCpoI=")]
[assembly: GoPositionMap("runtime/linkname_shim.go", "linkname_shim.cs", "AEWwAQAUAoKCppSCgpSCAAImABACAAIoABMCgoKmlIIAAigAEQIAAiIADgIAAiIADgI=")]
[assembly: GoPositionMap("runtime/list.go", "list.cs", "ABcyooIAHi6CgqaClKiSqJKoyIKmgoKmqJKCuKaCpqaCgqaokpSCpoKCpoKmgqaC")]
[assembly: GoPositionMap("runtime/list_manual.go", "list_manual.cs", "AB5AooIAHi6CgqaClKiSqJKoyIKmgoKmqJKCuKaCpqaCgqaokpSCpoKCpoKmgqaC")]
[assembly: GoPositionMap("runtime/lock_futex.go", "lock_futex.cs", "AAwisqiyAAymAYKCgqYABCKCppamkpSCgpSCgoKU3pKCgg==")]
[assembly: GoPositionMap("runtime/lock_spinbit.go", "lock_spinbit.cs", "AEKUAZKClAASIJKCgpQAAxgACAKCgoKUAAMiAA0ClOwABRCCAATYAYIABIgBooKmgoKU")]
[assembly: GoPositionMap("runtime/lockrank.go", "lockrank.cs", "AF68AQBOigGCgpSClIKUAAkQ")]
[assembly: GoPositionMap("runtime/lockrank_off.go", "lockrank_off.cs", "AA8gpoKmgqyypoKssqzYuLi4uLg=")]
[assembly: GoPositionMap("runtime/malloc.go", "malloc.cs", "AIgDlgaCgpSCgoIAHgaCgqamuJSUgoKUgoKUgoKUgoKUypSmgqaCgpSCggAHEIKCgoKUgoKmgpSCuIKmgriCgoKCgoKClKamADt4gsqCppSCgoKmgtyCgoK4goKUgoL8goLGgrS0tLSmlLTagoKUgoIAEyiCgoIACRQACBKCgpSClKbKgoKCgoKmgoLcgoK4ACIkABUClJTcgoKCyoKCgoKUlIKUlJSUgpSCggAHEIKUgqaCytyCgriCgoKCgriCgoKCgoKCgpSmgriCpoKCgriCggAIEoKClIKUlKaClIKCgoKCyoKCgpSCgpSCgtyC3LiCpgACIAAQBNyCgoKUgriCgoKUAAgSwoKCgoKCgpSCgoKmAAwYAA0CgoKClIKClIKClKaCpoKCgoKUACFYABQCgoLKgqaCgpSClO6mgoK8ooKCyoKokoKUgoKUgpSmpoK4goKUpoKUgpS4uIKmuIKUgpSmgoKUgpSCuIKCyoKU5rSCgoKUgpSCpgAePoKUgoLugoKCgpSUgoKCgoKmgoKCgpSCgqaUggAIEpTKAAkUAAsYgoKUgpSCgrgADBqU5rSCgoKUgpSCppSCgoKClJSCgqaUgoKCpoKClIKCAAgSlMoACRQACxiCgpSClIKCpqr2ggAHEILKgrgADRzKlAAIBrSCgoKUgpSClIKmlIKCgoKCgoKUgoKUppSUAAcQlMoACRQACxiCgpSClIKCpua0goKClIKUgpSCppSCgoKCgpSUgoKCgoKUgoKUgoIABxCUygAJFAALGIKClIKUgoKmprSCgoKUgqaUpoKCgoKCAAgSlMoACRQACxiCgpSCpoLclAAKFoKU7oKCpoKCggAHEIKCgoKClKaUlJSmgpSmgoKCggAIEoIAAxDUgoK4lLi+xIKCpgAabAApAoKUpqaCyoKClIKUlJSCgqYADRyCgpSCuILKgoKmgrimqqKClKaClIKClLiCptyClAAICsKCuIKClJSClILcgpSUuIKUpgAOCsaCgpSClIKUgoKUgqaCpoKCzJKClAACFgAIAoSSgoKUpoKClL7SqJIAAh4ADgKosgACIAAPAoKUgoKUAAIkAA8CqJLuwoKClIKCAAIU8pSUlJSqxpS0AAIWAAgCgoKClIIAGEYADQKCgpQAFMoBgsqUgoKmgoKClIKClIKClJQAEhqCyqKUpKSkpKSkpA==", "2331-2333:1")]
[assembly: GoPositionMap("runtime/malloc_generated.go", "malloc_generated.cs", "ABMapIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmhIKCpoKCuJSCgriCppSUgoKUlIKUgoKEgoKCgpSEgoKCgoKChIKmgoK4gpS6gpSCpoKCgoKCgoKCgoK8goKUgoKUhIKmlISWpoKClIKUgoK4hJSCpoKCuIKUpoSCgqaCgriUgoK4gqaUlIKClJSClIKChIKCgoKUhIKCgoKCgoSCpoKCuIKUuoKUgqaCgoKCgoKCgoKCvIKClIKClISCppSElqaCgpSClIKCuISUgqaCgriClKaEgoKmgoK4lIKCuIKmlJSCgpSUgpSCgoSCgoKClISCgoKCgoKEgqaCgriClLqClIKmgoKCgoKCgoKCgryCgpSCgpSEgqaUhJamgoKUgpSCgriElIKmgoK4gpSmhIKCpoKCuJSCgriCppSUgoKUlIKUgoKEgoKCgpSEgoKCgoKChIKmgoK4gpS6gpSCpoKCgoKCgoKCgoK8goKUgoKUhIKmlISWpoKClIKUgoK4hJSCpoKCuIKUpoSCgqaCgriUgoK4gqaUlIKClJSClIKChIKCgoKUhIKCgoKCgoSCpoKCuIKUuoKUgqaCgoKCgoKCgoKCvIKClIKClISCppSElqaCgpSClIKCuISUgqaCgriClKaEgoKmgoK4lIKCuIKmlJSCgpSUgpSCgoSCgoKClISCgoKCgoKEgqaCgriClLqClIKmgoKCgoKCgoKCgryCgpSCgpSEgqaUhJamgoKUgpSCgriElIKmgoK4gpSmhIKCpoKCuJSCgriCppSUgoKUlIKUgoKEgoKCgpSEgoKCgoKChIKmgoK4gpS6gpSCpoKCgoKCgoKCgoK8goKUgoKUhIKmlISWpoKClIKUgoK4hJSCpoKCuIKUpoSCgqaCgriUgoK4gqaUlIKClJSClIKChIKCgoKUhIKCgoKCgoSCpoKCuIKUuoKUgqaCgoKCgoKCgoKCvIKClIKClISCppSElqaCgpSClIKCuISUgqaCgriClKaEgoKmgoK4lIKCuIKmlJSCgpSUgpSCgoSCgoKClISCgoKCgoKEgqaCgriClLqClIKmgoKCgoKCgoKCgryCgpSCgpSEgqaUhJamgoKUgpSCgriElIKmgoK4gpSmhIKCpoKCuJSCgriCppSUgoKUlIKUgoKEgoKCgpSEgoKCgoKChIKmgoK4gpS6gpSCpoKCgoKCgoKCgoK8goKUgoKUhIKmlISWpoKClIKUgoK4hJSCpoKCuIKUpoSCgqaCgriUgoK4gqaUlIKClJSClIKChIKCgoKUhIKCgoKCgoSCpoKCuIKUuoKUgqaCgoKCgoKCgoKCvIKClIKClISCppSElqaCgpSClIKCuISUgqaCgriClKaEgoKmgoK4lIKCuIKmlJSCgpSUgpSCgoSCgoKClISCgoKCgoKEgqaCgriClLqClIKmgoKCgoKCgoKCgryCgpSCgpSEgqaUhJamgoKUgpSCgriElIKmgoK4gpSmhIKCpoKCuJSCgriCppSUgoKUlIKUgoKEgoKCgpSEgoKCgoKChIKmgoK4gpS6gpSCpoKCgoKCgoKCgoK8goKUgoKUhIKmlISWpoKClIKUgoK4hJSCpoKCuIKUpoSCgqaCgriUgoK4gqaUlIKClJSClIKChIKCgoKUhIKCgoKCgoSCpoKCuIKUuoKUgqaCgoKCgoKCgoKCvIKClIKClISCppSElqaCgpSClIKCuISUgqaCgriClKaEgoKmgoK4lIKCuIKmlJSCgpSUgpSCgoSCgoKClISCgoKCgoKEgqaCgriClLqClIKmgoKCgoKCgoKCgryCgpSCgpSEgqaUhJamgoKUgpSCgriElIKmgoK4gpSmhIKCuJSCgriCppSUgoKUlIKCgpSEgoKCgoSChKaCpoKCuIKUuoKCgoKCgoKCgoK8goKUgoKmlISWpoKClIKUgoKmgoSmgqaCgriClKaEgoK4lIKCuIKmlJSCgpSUgoKClISCgoKChIKEpoKmgoK4gpS6goKCgoKCgoKCgryCgpSCgqaUhJamgoKUgpSCgqaChKaCpoKCuIKUpoSCgriUgoK4gqaUlIKClJSCgoKUhIKCgoKEgoSmgqaCgriClLqCgoKCgoKCgoKCvIKClIKCppSElqaCgpSClIKCpoKEpoKmgoK4gpSmhIKCuJSCgriCppSUgoKUlIKCgpSEgoKCgoSChKaCpoKCuIKUuoKCgoKCgoKCgoK8goKUgoKmlISWpoKClIKUgoKmgoSmgqaCgriClKaEgoK4lIKCuIKmlJSCgpSUgoKClISCgoKChIKEpoKmgoK4gpS6goKCgoKCgoKCgryCgpSCgqaUhJamgoKUgpSCgqaChKaCpoKCuIKUpoSCgriUgoK4gqaUlIKClJSCgoKUhIKCgoKEgoSmgqaCgriClLqCgoKCgoKCgoKCvIKClIKCppSElqaCgpSClIKCpoKEpoKmgoK4gpSmhIKCuJSCgriCppSUgoKUlIKCgpSEgoKCgoSChKaCpoKCuIKUuoKCgoKCgoKCgoK8goKUgoKmlISWpoKClIKUgoKmgoSmgqaCgriClKaEgoK4lIKCuIKmlJSCgpSUgoKClISCgoKChIKEpoKmgoK4gpS6goKCgoKCgoKCgryCgpSCgqaUhJamgoKUgpSCgqaChKaCpoKCuIKUpoSCgriUgoK4gqaUlIKClJSCgoKUhIKCgoKEgoSmgqaCgriClLqCgoKCgoKCgoKCvIKClIKCppSElqaCgpSClIKCpoKEpoKmgoK4gpSmhIKCuJSCgriCppSUgoKUlIKCgpSEgoKCgoSChKaCpoKCuIKUuoKCgoKCgoKCgoK8goKUgoKmlISWpoKClIKUgoKmgoSmgqaCgriClKaEgoK4lIKCuIKmlJSCgpSUgoKClISCgoKChIKEpoKmgoK4gpS6goKCgoKCgoKCgryCgpSCgqaUhJamgoKUgpSCgqaChKaCpoKCuIKUpoSCgriUgoK4gqaUlIKClJSCgoKUhIKCgoKEgoSmgqaCgriClLqCgoKCgoKCgoKCvIKClIKCppSElqaCgpSClIKCpoKEpoKmgoK4gpSmhIKCuJSCgriCppSUgoKUlIKCgpSEgoKCgoSChKaCpoKCuIKUuoKCgoKCgoKCgoK8goKUgoKmlISWpoKClIKUgoKmgoSmgqaCgriClKaEgoK4lIKCuIKmlJSCgpSUgoKClISCgoKChIKEpoKmgoK4gpS6goKCgoKCgoKCgryCgpSCgqaUhJamgoKUgpSCgqaChKaCpoKCuIKUpoSCgriUgoK4gqaUlIKClJSCgoKUhIKCgoKEgoSmgqaCgriClLqCgoKCgoKCgoKCvIKClIKCppSElqaCgpSClIKCpoKEpoKmgoK4gpSmhIKCuJSCgriCppSUgoKUlIKCgpSEgoKCgoSChKaCpoKCuIKUuoKCgoKCgoKCgoK8goKUgoKmlISWpoKClIKUgoKmgoSmgqaCgriClKaEgoK4lIKCuIKmlJSCgpSUgoKClISCgoKChIKEpoKmgoK4gpS6goKCgoKCgoKCgryCgpSCgqaUhJamgoKUgpSCgqaChKaCpoKCuIKUpoSCgriUgoK4gqaUlIKClJSCgoKUhIKCgoKEgoSmgqaCgriClLqCgoKCgoKCgoKCvIKClIKCppSElqaCgpSClIKCpoKEpoKmgoK4gpSmhIKCuJSCgriCppSUgoKUlIKCgpSEgoKCgoSChKaCpoKCuIKUuoKCgoKCgoKCgoK8goKUgoKmlISWpoKClIKUgoKmgoSmgqaCgriClKaEgoK4lIKCuIKmlJSCgpSUgoKClISCgoKChIKEpoKmgoK4gpS6goKCgoKCgoKCgryCgpSCgqaUhJamgoKUgpSCgqaChKaCpoKCuIKUpoSCgriUgoK4gqaUlIKClJSCgoKUhIKCgoKEgoSmgqaCgriClLqCgoKCgoKCgoKCvIKClIKCppSElqaCgpSClIKCpoKEpoKmgoK4gpSmhIKCuJSCgriCppSUgoKUlIKCgpSEgoKCgoSChKaCpoKCuIKUuoKCgoKCgoKCgoK8goKUgoKmlISWpoKClIKUgoKmgoSmgqaCgriClKaEgoK4lIKCuIKmlJSCgpSUgoKClISCgoKChIKEpoKmgoK4gpS6goKCgoKCgoKCgryCgpSCgqaUhJamgoKUgpSCgqaChKaCpoKCuIKUpoSCgriUgoK4gqaUlIKClJSCgoKUhIKCgoKEgoSmgqaCgriClLqCgoKCgoKCgoKCvIKClIKCppSElqaCgpSClIKCpoKEpoKmgoK4gpSmhIKCuJSCgriCppSUgoKUlIKCgpSEgoKCgoSChKaCpoKCuIKUuoKCgoKCgoKCgoK8goKUgoKmlISWpoKClIKUgoKmgoSmgqaCgriClA==", "", "", "66=base/1/1/2;223=base/1/1/2;380=base/1/1/2;537=base/1/1/2;694=base/1/1/2;851=base/1/1/2;1008=base/1/1/2;1165=base/1/1/2;1322=base/1/1/2;1479=base/1/1/2;1636=base/1/1/2;1793=base/1/1/2;1950=base/1/1/2;2107=base/1/1/2;2264=base/1/1/2;2421=base/1/1/2;2578=base/1/1/2;2735=base/1/1/2;2892=base/1/1/2;3049=base/1/1/2;3206=base/1/1/2;3363=base/1/1/2;3520=base/1/1/2;3677=base/1/1/2;3834=base/1/1/2;3991=base/1/1/2;4190=base/1/1/2;4335=base/1/1/2;4480=base/1/1/2;4625=base/1/1/2;4770=base/1/1/2;4915=base/1/1/2;5060=base/1/1/2;5205=base/1/1/2;5350=base/1/1/2;5495=base/1/1/2;5640=base/1/1/2;5785=base/1/1/2;5930=base/1/1/2;6075=base/1/1/2;6220=base/1/1/2;6349=base/1/1/2;6475=base/1/1/2;6601=base/1/1/2;6727=base/1/1/2;6853=base/1/1/2;6979=base/1/1/2;7105=base/1/1/2;7231=base/1/1/2;7357=base/1/1/2;7483=base/1/1/2;7609=base/1/1/2;7735=base/1/1/2;7861=base/1/1/2;7987=base/1/1/2;8113=base/1/1/2;8239=base/1/1/2;8365=base/1/1/2;8491=base/1/1/2;8617=base/1/1/2;8743=base/1/1/2;8869=base/1/1/2;8995=base/1/1/2;9121=base/1/1/2;9247=base/1/1/2;9373=base/1/1/2")]
[assembly: GoPositionMap("runtime/malloc_stubs.go", "malloc_stubs.cs", "AClUgoKCyqaCqqTcgoKmgoLupoKC3IK4lIKCpu6CuIKCyoKUAAIS4qaigpSClIKUgriCgqaCgpSUgoKCgoKClIKClKaUgoKCAAgSlMoACRQACxiCgpSClIKCuKaigpSClIK4jNKmgoKUlIKCgqaUgoIABxCmgoKUgoIACBKUygAJFAALGIKClIKUgoKmpqKClIKUgriCgqaCgpQAHj6ClIKC7oKCgoKUlIKCgoKCpoKCgoKUgoKmlIIACBKUygAJFAALGIKClIKUgoK4AAwalKrCgoKCgoKCgoKCgoKCuKaigoKCgoKCgoKCgrimgoKUgoKUAAIUAAsElKaCgoLKgpSCgtyCgoKCggAAEgAIApSCgoKCgqaCpoKClOaigoKCgoI=")]
[assembly: GoPositionMap("runtime/malloc_tables_generated.go", "malloc_tables_generated.cs", "AAgQAIMEiAg=")]
[assembly: GoPositionMap("runtime/map.go", "map.cs", "ABUugoKUAAIgAA0CAAIkAA8CgqYAAhoACxYACgSCgoKUpoKCgpQAAiQAEBYACwKCgoKClIKUgqau4oKCpoKqwoKCpqiSgoKCpgAGJgATBIK4AAIcAAsCgpSUqJKClJQAAhgACwKCqLKCqJKokgACGgAMAoKUgoKUqJKosoKUgoKUAAIQ6tKCgoKCgg==")]
[assembly: GoPositionMap("runtime/map_fast32.go", "map_fast32.cs", "AA0gAAQWAAoYAAsWAAoG")]
[assembly: GoPositionMap("runtime/map_fast64.go", "map_fast64.cs", "AA0gAAQWAAoYAAsYAAsG")]
[assembly: GoPositionMap("runtime/map_faststr.go", "map_faststr.cs", "AA0gAAQWAAoYAAsG")]
[assembly: GoPositionMap("runtime/mbarrier.go", "mbarrier.cs", "ABesAgCAAQKClLgACBKCggADFAALCAACEgAMCgACIAAPAoKClIKClIKClKiSqJIAAhoADQK4lKaCggAEHgAMAoKClILugoKCgpSCgpSCgqaCpoLugriCuIIAAiAADwKClAACHAANAriUAAIYAAkCqJKossqUqLKCppQAAiQAEASC")]
[assembly: GoPositionMap("runtime/mbitmap.go", "mbitmap.cs", "AESYAQAKBgAaTgANAoKCgpQABhYACgKCgoKmgoKUlKiSlIK4gpSmggAGGAAJAoKClIKmggAELgAVBIKWkoKUlJSmpgACFgAIAoKCuIK4goKUuIK4goKCAAQYAAgEgoKUppSCgpTKpoKClKamgoKmgsqClIKClAAEENIACFoAKgKClIKUgqaCgoKmgoKCpoLulJaSgqaCgpSUgoKCgpSCgqaCgoKUgoKCggAEJgAOAoKUgpSClpKCpoKClJSCgoKUgoLakoKCgpSCgpSCAAkkAA4ClIKClILclJTMsoKCgoKowoKCgpQABBLiggAJFIKCgoKUgpSCgoKmlAAIFAALBKaCgoLKgpSCgpSm3IKCgoKCgpSCgoKCgqaCpoKCgoKCgoKmABYsooKUgoKU5rIAESSUgoKUpqIAMWSCgpSmssqCgpSCgoKClIKCgqaCgpTmxIKCgpSClIKCgoKCpoKCgoKUgoK4goKCgpSUgoKCgoKCgoKCgoKUgpTmsoKCgpSCgpSCgoKCgqaCgoKCgpSCgriCgoKClJSCgoKCgoKCgoKClIKmgpSCgoKCgqaClKaU6MKClLiUgoKCgpKCgoKClIKmgoKCgoKCgoKSgoKCgqa4goKCgoKUpq74rviu+AACEgAJCAATJpKCrsKCgoKCgoKCgoKC7sKCgoKUgqaUgpSCgoKUlIKCuIKCgqaClNyClIIAAhgACQKClIIAAiAADQKClIIABhYACAKmlIKClKyyxoKCgsiSqMiokqjIAAgIsoKClKyygoKCAAsYyJKCgpSUAA4QAAsSgoKCgoKUlJSCgoKUggACNgAcAqaCuJTclIK4gpSmgoKs6AAEGgAKAoKClIKCgoKUgpSUgoKCgpSCgoKmAAkkABACgpSCgpSClIKCgoKCgpSUgoKCgoLuooLKyoKUyqKCgoKUlAAMFrKCgoKCgpQAAioAFAKWkpSCgqaCgoK4goKClJSUgoKCgoKClIKCgpS4goKCgoKC3IKCgoKCgqYACBKCgpSCpoKCgoKC7oKCuO6CgpSmgoKUgoK4goKC7oKCgoKCgoLKgoKClNyUgoKCgoK4goKCgoKmgoLqgoKCgoKUpqKCgoKCgoKUgoKCgoKUgpSCgoKCgoKCuIKCgoKCgqaCAAQW4g==")]
[assembly: GoPositionMap("runtime/mcache.go", "mcache.cs", "AE6qAbIADRaCgoKCgoKUgpSUAAIS4oKC7oKCAAMQ1IKCypSUABAQ5JSCyoKUgqaUgpSmgoKmgoKUpoKmuIKCpoLKpgAOHoKClKiSgpSCgtyUgoKCuIKCgqamuAAIEoKCppSClIKCgoKCgqaCgriU3LiCuIKmgoKC7qbsAAwQgoKCgoKClIKCqqKCuIKCqqKClA==", "99-104:1;120-132:1")]
[assembly: GoPositionMap("runtime/mcentral.go", "mcentral.cs", "ADBikoKCgoKqoqqiqqKqouikgpSCgoKCAA8glIKmgoKmgpSCgoKUlIKCAAoWgoKClJSUgoKCgqa4lIKCgoK4goK4goKCgoKmgoKUgpS4lO7igqaCpu6muAAHEIKUlLjMkoKCgpSC")]
[assembly: GoPositionMap("runtime/mcheckmark.go", "mcheckmark.cs", "ACdMsqaCgpSUgoKUpqaClIK46JKClAAKDLKCgoKmppSClIKClJSUgqaygoKUlIKCgoIAAhDS7oKCgoKCgoKCggARCqKUnIyCgpSUgoK4griClIKUtLaC7oKClIKClIKUgoKClIKUpoKCgoKCgoKCgpSCyoKClIKCgpSmgpSCgpS4lIKCgoKClKaCgoKCgpSmlIKClAADEtKmgqaUgoKUgoIABxC4goI=", "42-57:1;167-219:1;183-190:1.1")]
[assembly: GoPositionMap("runtime/mem.go", "mem.cs", "AApiACgCgoK4gqYAAhDSgqqiAAIaAAoCgqyyrLKqogACKAAXDoKmgoIAAhgACQKCAAIcAAsCuIKmAAgQ0oKClMqCgoKUpOyCgoKUgoKUpraCggAHEIKClIKClAADHAAQDoKmrsKC")]
[assembly: GoPositionMap("runtime/mem_linux.go", "mem_linux.cs", "AA4qwoKCgoKUgoKUlIIADw6CuKaCgpSkgpSC6IKUgvyCgriCgoKUAAsIgoKCgpSClIK4gpSUlIIACAqCppTmgqaUggAOHq7CpoKCpoKCgpSC5oKCgpSCgpTcgriC")]
[assembly: GoPositionMap("runtime/mem_nonsbrk.go", "mem_nonsbrk.cs", "AAwagg==")]
[assembly: GoPositionMap("runtime/metrics.go", "metrics.cs", "AC18soKmuIIACxiUlIKmgtyC3ILcgtyC3ILcgtyC3ILcgtyC3ILcgtyC3ILcgtyC3ILcgtyC3ILcgtyC3IKmgu6C3ILcgqaC7oLcgtyCyoLKgtyC3ILcgsqC3MqC3ILegtyC3ILcgtyC3ILcgtyC3ILcgtyC3ILcggAEEILcgtyC3ILcgtyC3ILKysrKytyCyoLKgoK4pqKC6qKCyJKCgoKClJKCABMusoKClMjCgoKUqMKCgpSokoKCpqiSACFKsqaCgoKCgoKCgoKClIIAGzKygoKCgoKClIKCgoKCggANFpIAFCSSgoKCAA8akoIAFrQBogATKMKCgpSCgpSUtLS0tLS2ACBEsoKClIKClIKClAAYNMKCgoKUlIKClJSssqamgg==", "91-94:1;98-101:2;105-108:3;112-115:4;119-122:5;126-129:6;133-136:7;140-143:8;147-150:9;154-157:10;161-164:11;168-171:12;175-178:13;182-185:14;189-192:15;196-199:16;203-206:17;210-213:18;217-220:19;224-227:20;231-234:21;238-241:22;245-248:23;252-260:24;264-267:25;271-274:26;278-286:27;290-293:28;297-300:29;304-307:30;310-313:31;316-319:32;323-326:33;330-333:34;337-340:35;343-346:36;349-352:37;355-358:38;362-366:39;370-373:40;377-380:41;384-387:42;391-394:43;398-401:44;405-408:45;412-415:46;419-422:47;426-429:48;433-436:49;440-443:50;447-450:51;454-460:52;463-466:53;470-473:54;477-480:55;484-487:56;491-494:57;498-501:58;505-508:59;511-513:60;516-518:61;521-523:62;526-528:63;531-533:64;537-540:65;543-546:66;730-737:1", "579=runtime.metricReader.compute", "76=makeStatDepSet/1/53/8,makeStatDepSet/2/53/15,makeStatDepSet/3/53/22,makeStatDepSet/4/53/29,makeStatDepSet/5/53/36,makeStatDepSet/6/53/43,makeStatDepSet/7/53/50,makeStatDepSet/8/53/57,makeStatDepSet/9/53/64,makeStatDepSet/10/53/71,makeStatDepSet/11/53/78,makeStatDepSet/12/53/85,makeStatDepSet/13/53/92,makeStatDepSet/14/53/99,makeStatDepSet/15/53/106,makeStatDepSet/16/53/113,makeStatDepSet/17/53/120,makeStatDepSet/18/53/127,makeStatDepSet/19/53/134,makeStatDepSet/20/53/141,makeStatDepSet/21/53/148,makeStatDepSet/22/53/155,makeStatDepSet/23/53/162,makeStatDepSet/24/53/174,makeStatDepSet/25/53/181,makeStatDepSet/26/53/188,makeStatDepSet/27/53/200,makeStatDepSet/28/53/207,makeStatDepSet/29/53/214,makeStatDepSet/30/53/233,makeStatDepSet/31/53/240,makeStatDepSet/32/53/247,makeStatDepSet/33/53/272,makeStatDepSet/34/53/280,makeStatDepSet/35/53/287,makeStatDepSet/36/53/294,makeStatDepSet/37/53/301,makeStatDepSet/38/53/308,makeStatDepSet/39/53/315,makeStatDepSet/40/53/322,makeStatDepSet/41/53/329,makeStatDepSet/42/53/336,makeStatDepSet/43/53/343,makeStatDepSet/44/53/350,makeStatDepSet/45/53/357,makeStatDepSet/46/53/364,makeStatDepSet/47/53/380,makeStatDepSet/48/53/387,makeStatDepSet/49/53/394,makeStatDepSet/50/53/401,makeStatDepSet/51/53/408,makeStatDepSet/52/53/415,makeStatDepSet/53/53/447")]
[assembly: GoPositionMap("runtime/mfixalloc.go", "mfixalloc.cs", "ADtwwoKUlIKCgoKCgoKCguaCgoKmgoKCgoKUlIKCpoKClIKCgqaCgoKC")]
[assembly: GoPositionMap("runtime/mgc.go", "mgc.cs", "AJgB3gIAEgLmgoKmyqaCpoKCgoKCgoKCrtSCgoKCggAmTpKCACZSAAYU1oKCgpSCpgC8AaYE1gAlTIK8opSCgpSUlILKggAlVrKClJSCpIKUgraUAAgSAAwKgoCCgqSClIC4goKiAA4cypSCgu6CgoKCuIK4lIKCgriUgoKmgriUlIKmlIKClIKCgoK4lAANHMqCAAcQgsqUuKbKgriCgqYAECKUgu7uuO6mgoKCpu6CuIKmogAdWgAYBpSO4oLKuKaCpu6mgoK43IK47oKCgoKC7gAKFoKCgoKCgu6ClNymgpSmgpSCprimuLiCyrjKps70lKa8zIKCpgAEEIK2zLKc5IKCgpSCgoKCgsqCgoKUrLKUpriC2IIABhCilIKCggAEEsLKlLiUuJSCgu6UgsaC2oKCAAoKtJSClIKCgrjuggAJFIKCgoKApIK4gqaClIKCgriUgri4poKCgoKCgoKCggAJFIK43IKmlIK4goKC3KbKAA0cgoKCgqaUpoKkgoIAChbcuKYADRyClIKCgoKCgqaClJTu3IKUgoKmgpSCgoKClIKUgu6UgoKUlNyClJSClNyCgpSClLiCgoKCgriCgriCpoKmlAADEAAJDIIACRSCgpSCgoIACBK4vAALFIIAHzCCyu6ClJSUABEkpoKUAA8guKYACBKClIKCpoKmgoKCgqaUAAsUgpS0gtyCgoLKtLSkuIKCgoKUgu64uIKUzsKClKiS6qKCgoL+soKCgpQACAyygpSmgoKmptwABxDcppSmgoKCgoKUlIKUlILcAAgSgoKClLgABhYACAKUgqaCgoKCgoKClJSmgoKUgqamgpS4goK4goKClIIAAhoADAaCgsqCgoKCgqaCAAYkAAoCqJKmlIK4gu6CkoKClIK4hrKCgpSCrLKCgoKCgoKClJSCqJKUpoKCgpSCgoKUAAIaAAoCggAQhAEACAKCgoKUgpSCgpSCpoI=", "752-754:1;834-836:2;869-871:3;947-954:4;1042-1059:1;1083-1085:2;1103-1111:3;1128-1135:4;1260-1276:1;1284-1289:1;1365-1374:1;1377-1389:2;1380-1380:2.1;1500-1516:3;1545-1558:4;1548-1552:4.1;1649-1651:5;1791-1818:1;1847-1884:2;2116-2119:1", "", "1535=itoaDiv/1/1/1;1566=Load/1/2/3,Load/2/2/4", "", "681:29,682:21")]
[assembly: GoPositionMap("runtime/mgclimit.go", "mgclimit.cs", "AFK0AcIACBIACAKmlIKmgoIAChTSgtyClIKCAAoSoqqiqqLu4riUgpSC6LKClJSCpoKCuIKCpgAHEIKCgpSCtLS0tLa4goIAFzCUruKC7qaCgoKCgpS4lKaUgrqS6JKCggAOFuKmpoKUgoKCgoKClAAcQJLO5oKClKiSAAskAAoCgpSCAAIQAAkEgoKClJSCuJSCgqYACAyygoKCgoKUgqaC7qaUgrS0tA==")]
[assembly: GoPositionMap("runtime/mgcmark.go", "mgcmark.cs", "ADh60oIABBLiuIKmlqKmgoKUggAIEq7CppiCpoKCgqaCggAPIIIABxCmpoKmlIKmgoKCggAICqKCggAHEIKCgqaCpqbKAAgaAAsEkoKUgoLIgoLIgoLstqaC2siCgoKClLiCgsragoKCAAkUgoKClIKUgoKUgsiCgoKmAAYS4pTcgoKUgoKCgriCrtSCgoKCgqiSgoKCgoKCgrimgoIACAwAEyKmgoKmgoKUgoKUgoIABxC4goKmlILKgoKUxoK0tsz4yoK4qKSuAAoGgpSCpoC4goKiABAggoKmgoKCAAcQgsqm3IKCgoKCggAIEoKCgoKClIKUlJSmgoKCAAcQgsqmpoKCgqaClO64gsqCgoKmAAcQgoIACxiC3IKCggAHEILKxgACGAAOBpQABxCCAAgSgpSmuIKUAAcQgrjKlIKCgoKClIKCgr6ygoKCrsK4goKmgoLcgoKClIKmggACGgAKAsqCpoKUgoKmlILupoLKgriUgoKUAA4oABMCgoKmlIK0pIL6gqyygpSUuIKClJSmpoKUgqaC3IKokoLuoqaUpsqCtpQABxCCgoKClIKClIKUlIKCgoKClIKUgoKClNyCgoKCgoKClLiClIKUrOKCpoKCgoIAChaCgoKC3JTKpqbKypSmpoKCuIK4uIKCgoKClIKUlIKUAAsgoqqigoKUqqKuwpSCuJSUpgAGQgAgAoLKgoKCgpS4goKCgoKCgrimgoKClIKCuIKCggAPINyCqJKCgoKCuIKCgu6CgoKmuIKCgu6CgoKClIKUgoKC3ISSgoKUAAciAA8Cgsq4gpSCqJKCgoKCuIKClIKCgoKm7oKCgqa4goKCuIKCggAIEgACGAAOCIKUlIKCgpSClIKCgpSCgtyCAAgaAAoCgoKCgoKCgoK4goKCgqaCgpSCgpSClKaCgoKC7oKUgpSCuKYACBKCuIKC7oKCuIKCAAQU0oKCggAKGAALBIKUlIKUlIKmgoKCgoK4gpSmgoLcgoLulIK8ooKCgoKUgoKUpoKCuJS4goKUgoKUgoKUlIIABxYACQKClJS4goKUuIKCpoKsspSCgoKUgoKC", "113-115:1;191-204:1;280-313:1;515-517:1;638-642:2;1536-1562:1", "", "205=readgstatus/1/1/1", "", "563:29,564:21")]
[assembly: GoPositionMap("runtime/mgcmark_greenteagc.go", "mgcmark_greenteagc.cs", "AFe2AdKmAAkUgoKClAAIBqIAESSUypSqoraCysiUAAIWAAgCzLIACAiSgqbssoKUgoKCgoKClJSCuKyygpSmooKUlIKCpoKClKqiAAQS4qrCgriCgpSCgoK4goKmgoKUpoKCuIKCuIKUlKbIkqiygIK2gILIgpQAHzySgpSCguwABRYAERSUgpS4goKCpriCgpSosoKClKzkgpSsxIKCgpSU6LKUuIKCuIIAAyoAEQKCgpSCgpSC7NKClIKCgoKClIKClKqigpSCgqyygoKU7qaCpqbugqYABxQACAKCqJKCgoKCgoKUgpSCgpSCyoKmAAgSAAgCgoKUgqamgoKCpoKClAA6dsKCgoKmgoKUgqaCgoLMsoKCqLKCgoKCgoKCqJKCyAASJIKCgpSCgoKUgoKUlMyylIKClIKClIIACBKUAAsOkoKUxoKmggAICsKCgoKClKamgoKCgpSCgoKUgoK4goKClJaSgoKUyoKCgpSCAAcQ3pSUgriCgoIABRbygoKmgsqCyoKCgoKCgoKUgqaCgoKUppSmooKCgoKCgoKCgrimgriigoKUgoLcooKigpSCgoKCgpSCgpSCgoKCuKaC3KKCgpSCgtwACBKCgoKClIKUgoKCppQAAhLiggAKHsLOgoKCgoKUgoKCgoKClIKCgpSUgtiigoKCgoKUzsKClIKUgpQACBgADwrugoKClKamgqbcgoIABxCCgoKUpoKCgoKC7ri4AAkUgoLKgoKC", "", "", "1009=sizeclass/1/1/4,spanPtrMaskUnsafe/1/1/5")]
[assembly: GoPositionMap("runtime/mgcpacer.go", "mgcpacer.cs", "ANcC5gWigoKCggAGFNKCgoKCgoKCgoLugoKCgsqUlJS4goK4goKmyoLcysqClIKCggAIPgAYAoKmlIKCuLjcgoIAChaCAAgSgoKUlIiypgAJFIIACBK4gqYAChaCgoKs9qamlIKmAAgSlIKCAB08gt6CgoKCpoKUgoKCgoKCgoIAAxoACQLcgriC3IKC3IK4gpSCgpSCgoKClIKClIIACCIADwKCpoKmyqYABxC4ggALGKaCgoKCpoLKpoKUgtyClIKmpoIAAh4AEgqClIK4goKCuIKmgoKCgoKUAAISAAgCgoKmgoIAAhDygoKCgoKmgoKCAAcU4pSCtLSCtMiCgoKClIKmpoK4uKKCgpSCgoK4gqiSgq4ACQSmgsqCytwAARwADAKCpqi0soKCgoKCAD5+ppaSgqbKuMqCppSClKaClAAdPgAJAu7K7oIACRSCggALGIKClJaSgoKUlIKCgoKClAACKAATAoKmpsrcgoK4gpQAGDKu4oKmgoKUgpTIxIKCgoLKgqbmgoKClIKUzuKCpoKCpsjEgoKCpoKUgpQACAaCgoKUgoKClAAIGgAKAoKCgqaUgoKUgoIABBoACAKCgqiSgoKCgoKUgoIABBTSgoKCgoKUgoIABhwACQKUpoLKgoKCpoKC", "809-820:1;1354-1359:1;1401-1412:1", "", "438=Load/1/1/1")]
[assembly: GoPositionMap("runtime/mgcscavenge.go", "mgcscavenge.cs", "AJMBrAKSAAIgAA0C7rjcgpQACBKCgqaCyqYADBrKgpQAb/gB0oKUgpSCgpQAChYAChamgoKCgoKClIKmgpSogv6ygoKUgqiSrNKClKYAARDigpQABhQACQKCgqbcAAgSuJSC3IKmgqaU3IKUgrimgoKUlMoABxCWkoK4gqa8woKCAAgSAAoCgoKUlJSCAAIaAAwGAAES8oLclMqCpoKmypQABBDSlIKUgoKCgpSCAAUaAAkCgoKCgpSClIKmAAQU8pSCzIKCgoKUggAEIAATDoKCAAcQgoKmgqamlNyCppSmuIKClIKCgsqCgpSCpoKUuIKUAAgWAAgCAA8guJSktLS0tLS0AAsWAAouABQCgoKCgoLugpSmlJSCgqaU3IKCkqbKgoKCgpTugu7ugriUlMqCuAA/hgHSgoKCgqzWgoKUqsKCgpSClLiCgpSUgqaCpoLuppSmggACFgAIAoLcruKCgqaCgu6CggADEvKCgoKUAAIQ0oKCAAwUkqqiABw+kgAJEpIAGzySqJKoksqilJSUlLi46JKCgpSCgpSClPqSgoKUgoKUpgASPAAMBIKmgriCgpSCgoK4goK4goKmqJI=", "364-366:1;393-402:2;405-409:3;412-414:4;682-684:1;824-841:1", "", "399=Load/2/3/1,Load/3/3/1;724=load/1/2/3,load/2/2/4,heapRetained/1/1/4")]
[assembly: GoPositionMap("runtime/mgcstack.go", "mgcstack.cs", "AIkBkgKCgpSCABAk1gAnTsKClIKClIKUgoKCgoKCgpSUgoKUggACEPKCgpSUgpSmgoKClKaCpoKClOiygpSCgoKUgpSUgoKCgpSCgoKClKyyAAQQAAgCgpSSgoKCgoKUgoKCyqKCgoKClIKClIKClA==")]
[assembly: GoPositionMap("runtime/mgcsweep.go", "mgcsweep.cs", "ADx+gqamgoIACBKCrsLMsoKCgoKCgpSUpoK4ggAZQAAKAoKCgpSCAAoOooKUgoKClIKCpoKCpoIABBbigoKClILMkqyyrLKCAAgS4u4ABxCC7oKCgoIABxCUpoKUgoKCgpQAACIAEAKCgoKCppSUgoK4uIKUggAVJKKCpoKmgpQACAqiuLiCgoK4goKCgoKClLiCgpSUlIK4ypSmlAAPIIKCpoKmpoKCpqaCAAIS4gAIDOiCgtyClIKCgpTcgoKClAAVEvaCgqaCpqaCgIKCtoKCgqaUggARJIKClIKCgqaCgoKUgoKmuKaCgoKCuMqmgoKCyoKCgqa4gqaUgoKCgoKCgoKCpoKmgpSClIKUgqaCyoK43IKCpoKC3IKCpoKmgoKCgsqCpoK4uICCgqSCAAwalIKmlLiCyoK4toKUgoKClKaUyoKCgqaU3JSCpoKUppQABxCCgoKmAA8ggoKUlLiUAAgiABACgoKCgoKCgoKClJSClJSCgpSCgoKClJSClAAEKAARApSmgoKCuIKCgoKCAA8glIKCgoKUlLiCgoK8tIIAAxLSpoLugriClJSCgoKClIK4", "429-443:1;751-761:1")]
[assembly: GoPositionMap("runtime/mgcwork.go", "mgcwork.cs", "ABs4goIAZugB4oKCgpSuwoKmgoKCgpSCgoKCgoKCgriC3IKClAAEEsKCgqaCgq7CgqaCgoKCpoKCgoKCgpSCgqaCgpQABBjygoKCpoKCgoKCgoKUgriCAAIQ0oKCpoIAAhQACQKCgpSClJSCgpSClJSCAAkUgpTKgpSCggADEMKClIKCgoKCgpSmgoKUAAQQ0gAZMMKC+IKCAAUQwoKCgoLKgoKEkoKCgoKClJSCgpSCpoKCuIKCgoKClLiu4oIAAhDygq7CgoKClKjEgoKCgqaC7LKCgsqCgqqigoKCgpSCgoKCgpSCpoKC", "449-451:1;547-557:1")]
[assembly: GoPositionMap("runtime/mheap.go", "mheap.cs", "AO0ClAYADiCyrsIAX8YBgqbCgoKClAAKIAANAoKUlIKCgpSCgoKClIKCgpSCgoKmggANIILIkqiSAAQeAAwCqqIABhbippQAAxTigpQABRTiAAIQ0oKClJSkAAVk4oIAAhLi3IKUquKCgoKCqJKClIK4gqaCgpTIsoKUgoKCgoKCgoKCgoKCggAIEriCppQAAhIADg6C3JSCgoKmgoKUgoKUlIKUuIKUgqaUgriCgqaCpoKmgoKClAACEgANDJSCgoKClIKCpoKCgoKCyoKCgqaCgoKCgoKClMrugpSCgoKUgoKmggAOHJIABBLosqaClJQABigAEQKClKqigoKCgoKCgpQABxwADAKCgpSCggAIEgAIEoKCuIKClJS43IKUAAIeAAwCpoKmgoIAAhoADAKUgpSmgoKClKaCggACGgAMApSUgoKCuAAGKAAVBIKCysqCgqaCgoK4goKCggAIEpSUAAkUgoKCgoKClIKCpoKmlIKCgoKCgpSCgrimlJQAABYACgKCpqaCgqYACBLKgoKUgr6y3IKmmKaCgpS4lIK4gqaCpoKCpoKCgpS0tKSmgoKCgqaq9qa4gpSCgoKCuIKCgoKUgoKClJSUlJSUlKa4goKCgqbKAAsYAAkUlNyCpoK4yq7ilO6UpoKCuIKCgoKmpsq4lIKCpoKmgpTKgoIABxC4gu64goK4gpQABxCCgoK4qJKUgoKCgriClIKClJSCgpSCgpSCAAMeAA4EgoKCgriCgoKClIIADAaylJSCxoKUgoKUpoKmgsbsgoKCpoKUtLSkpqaCAAIUAAoIgqaUlILakoKA2MSCgoKCgoKCgoKCgoKCgoKCgoKmggANFpKC5rKClJSClJSClJSCgqaC5rKCgpSCpqaUguaygoKUgpSmlIKq0oK4griCpoKCpgAnUrKCgoKosoKCggAGEgAIAoKC3IKUgpSmgpSCgoKmgqaC7LKCgtyClJSClIKCgoKUgpSCgszEgoKCgoKUgoKUgpSUABEiwoKCgoKCgoKCyoKCgqaCuIKUuIKCgqiSgoKUgoIACyAACAqCgoKCgoKClILKppTKggAMHqKs0oKCgoKCgoKUgoKCpoKUgoKUgoKCgoKClJSWgoKUpqamqJKopIKCyoKUlIKUgoKCgpSUloKClKampoKUgpSClIKCAAke0oKCgpSCgoKqopSCgoKUgoIAIkiSqJK4poK4poKCgsqCyoKCpoKm3AAHEIKUyoKopJSCgqamprgAAhDSgoKCAA8IksqCgrimgqaCgoyKgoKCyoKCpoIACRSCggAIEoKClIKC7oKC5oKCgoKU3IKUlJaSgoKUgriCABgyyMrCgoKSgoKCAA8ggoKUgsqUgpSCABAYsoKCgoKCggAfQKLGgqaCgqqigoKCAAgWAAoClIKCgoK0goKCgrSCgoKCtILGgoK0uIKCgrSCgoK0goKCtIKCgrQACxiCgoK0ggAMFpKqogAgOsKCpoKCpoIACAqigqaCgsq4goK4uKaCgoLKgoK4gpSCAAIS4gACIgAOAoKCgqaClIKmgoKC6qKCgoKCgpSUgoKUpoKUlA==", "1011-1018:1;1378-1380:1;1667-1696:1;1815-1815:1")]
[assembly: GoPositionMap("runtime/minmax.go", "minmax.cs", "AAkSgoKUpoKClKaCgoLoooKUgrimooKUgrim0pS0pKbSlLSk")]
[assembly: GoPositionMap("runtime/mpagealloc.go", "mpagealloc.cs", "AFvKAQAIAAAIFqKokqqiyJKmlLqSgpTcoqqiAAIQABASgoKssoIAfIICwriCgpSmpqampqassoKClKyy7uK4grimyoKSgpSCysqCAAcQgoSSgoKUuIKUypTcAAIgABQQgoKClIKCgoLugqYABBoACAKmgramgoKClIKmpriCgpSCyu6CgsqClLiCpqaCgoKCgoIABRwACAKUgpKkgpSCgoKmgoKCgoKCgoKUgoKClIIAAhLiuIKCgoLKpgAQJgAQAgAbOAAOHgAMHoKmgoKmgoIABxCClIKUgrim3IKCAAIaAAsCgoKmgsqUgriCpoKUuIKCgpTKgoKmlKaClJTcgoKCgoKClAALFIKCpoKCgri4goIAAh4ADwK4gsqClIKCgoKCgpSCgsqCgtyUlITagpQAAhQACQKmgpSCpoKCgqaSpJSCpoKCgoKUgqYAAhgACQKUgpKSgoKCgpSCgoKCgoKCgpSCgoKUABYwkoKUzJKClKiSgpSokoKUqJKClM7qgpTKgu7ugpSm", "713-726:1")]
[assembly: GoPositionMap("runtime/mpagealloc_64bit.go", "mpagealloc_64bit.cs", "AB5CAA4eAAsYAA8W1oKmgoKCuIIACRwADAKCgtyCgtyCgoKCAAgSooIACximlNyCuO6ClIKmgriCggALDtKCgpQACxiCgoKmgpSCuIKCuKaCgpSClIKmrLKCgoKCgg==", "109-112:1;117-125:2;130-133:3", "", "135=add/1/2/1,add/2/2/2")]
[assembly: GoPositionMap("runtime/mpagecache.go", "mpagecache.cs", "ABoykgACFgAIAoKUgoKCgoKUAAIS4oKClIKCgoIAAhgACwKUgpSCuIKCppSC3IKUggAEGAAJAriClIKCgpSCgoKUAAcQgqaClIKCAAgSgoKmpgAJFII=", "", "", "144=chunkBase/1/1/1,alignDown/1/1/1,pages64/1/1/2,block64/1/1/3;161=alignDown/1/1/1,chunkPageIndex/1/2/2,pages64/1/1/2,chunkPageIndex/2/2/3,block64/1/1/3")]
[assembly: GoPositionMap("runtime/mpallocbits.go", "mpallocbits.cs", "ABEekqiSqJKokoKUgqaCgoKUlIKCpqiSgryiqJKokoKUgqaCgoKUlIKUqJKqoqrCgpSCgoKUgoKClIIADRayooKCgoKCgpSCpoKClJSUhJKUlJS4goIABxCCgsqCgpSClIKClKaCgpSmuIKCgoKCgpTGAAIYAAkCgoKCgpSu4oKCgoKUlAACGgAMApKCgoKCuKaUgoKmgoKUlAACGgAMAqKCgoKClKaUgoKClIKClIKCgpSUgpSokqiSqJKokqiSrLKqos74goKClIKmgoKUpsoAECLUgqrUgg==")]
[assembly: GoPositionMap("runtime/mprof.go", "mprof.cs", "AJoBtAKygoKCACZAsoKCqtKCgoKCgoLO2IKCgoKCggAUhAKCgpSCgqYAAhYACAIAAhTygoKmgoKCgoKuwoKCgoK4goIAAxAACQyUgoKCgoKoksq4gpSCgpSCgoLckrqSlIKUgoKCAAUW4oKCgoKmgoK4poKCpoKCvKKClAAEbsKCgpKCgoKClJSUlIKCgoKCgpSClMqCuJQALFaCgpSmgoIAAxQACAKCpriCpriClIKCgoKUpoKmoqamggAJFJSUlIKCgoKSgoKUggADEtKCuMqClIKCgoK4goKUgoKCuKaCxoKClNyUgoKCgpSClAAFGPKClIKCqJKClIKCAAwaooKCpgAkRICWkqqigoKmAAQwABUCgoIAAxwADQK4goKCgoKCgoKCgpSCpsqCgoKCgoKClIK4goKCgoLuuIKmooKCgoKClIKUgpSCqJKCgriCgoKCgpQACiQACgKCkoKUgpSmgoKCgoKCpoKCgqaCvuKCgoKUgoKCgu6ClKaCrNKCgoKUgpSC3IKokoKCAAMUAAkCgpKClIKUrOKCgoKUgoKCgsqmgqiSgoIAAxTigoKCvuKCgpSCgoKCpqiSgoK6kgAEGpKokoKmAAkGABk+gqaCpoLGoqamppSmqJKygqCSgpS4gqamosqmlJSCAAcQgoKUlLiCgriCgrKUgpSC7oKCggAMGpKmgoKCgoK4gqaCpgALGILuwoKUrLLKpoKCppSmggAHEIKCgpQABxYACQIACBIADBqCgqaUuAAKFpCkgriylLaClIKUgpSmgqaCkoK4goLWgoKilKaCgriygqam3JCSgoKUuIKmggACEgAJAoKCgpSCgpSmgqYAChaCpoKCgoI=", "463-465:1;581-589:1;740-744:1;937-940:1;1033-1036:1;1066-1069:1;1151-1154:1;1165-1168:1;1202-1205:1;1215-1219:1;1242-1245:1;1339-1347:1;1341-1341:1.1;1396-1398:1;1426-1428:2;1439-1441:3;1566-1566:1;1576-1589:1;1596-1600:2;1609-1611:3;1621-1641:4;1635-1635:4.1", "", "656=FuncPCABIInternal/1/1/2;884=stk/1/1/5;1004=stk/1/1/3;1092=stk/1/1/3")]
[assembly: GoPositionMap("runtime/mranges.go", "mranges.cs", "ACJCsoKClMiSgrioku7CgoKCgoKCgpSssoKClIKssoKClIKqooKUgpTsygAMGpKokqqiqqKqoqi2qJIADhqigoKClILOooKCgoKUggAEEsKqoqqigoKCgpQAHEiimrKSgoKmlLju3IKCpgACENKCgpSClIKUqJKCgpQABJABsoKUgoKCgoKClIKCqqKClIKClIKClICCgoKClIK2gg==")]
[assembly: GoPositionMap("runtime/msan0.go", "msan0.cs", "ABEmgIKCgoI=")]
[assembly: GoPositionMap("runtime/msize.go", "msize.cs", "AA4gwoKUgriClKaCgpQ=")]
[assembly: GoPositionMap("runtime/mspanset.go", "mspanset.cs", "AE6mAdSCpoKCgoK4poKCgqaClIKClIKmlKaCAAoWuIKCyqqikpSClIKCgoKUuILuyoKCgu6UgoL8ysqCgrjKAA0clKaUAAoU8oKCgpSC3IKClLiUuLimpoIADRqyrLIACxKSAA8aooKUqJKCAAgSosiSqJKokgALEpKokqiSqJLokpSCgpSokgALEpKokg==")]
[assembly: GoPositionMap("runtime/mstats.go", "mstats.cs", "AKoCogWCgoK4goIAGTqy3AAJFpKmgoKClpqCpoKCgqaCgoIACBKCuIKUmpQAESSCgoKCgpSCgoKUgoKCgoKUgoKClIKCgsqCgoKCgpSCyoKCgoKCggAQIoKCgoKmgoKCgoK4goKCgoKCgoKCgtyokoIABxDClIK4lIKC7oKCgoKmgoKCggACENKUgoKClIIACzTSAAYQ0oKCggAeQvKCgoKClIKCgoKUgoKCADSAAQATAoKClIKmlIIAAiIAEAKCgpSCpgADEOKUggADEMKUABqyAcKCggACENbcpoKCgtyCpoKCgqaCgqaCAAkU", "568-570:1", "", "396=load/4/9/1,load/5/9/1,load/6/9/1,load/7/9/2,load/8/9/2,load/9/9/2")]
[assembly: GoPositionMap("runtime/mwbbuf.go", "mwbbuf.cs", "AE+cAZKCgriUpoIAAxLSqJIAAjIAFgKClIKCqqKClIKCAAQkABIIuILKggADHAANBIKCuJSUgpSCABAigoKC7pSClIKCuIKClKaCgqaCgpSCuJQ=", "180-182:1")]
[assembly: GoPositionMap("runtime/nbpipe_pipe2.go", "nbpipe_pipe2.cs", "AAgSgg==")]
[assembly: GoPositionMap("runtime/netpoll.go", "netpoll.cs", "AH6QAoKCgraSAAIUAAkCgoKUgpSClKaCgr6ygoKCgpSCgoKCABo2wgAEIIKuwgAICJKCgoKClIKClIKUlIKCgoKCgoKCgoKUgoKClAAKCLKClIKClIKClILG1riCgpSUlIKCggAEENKCgpSCgoKUAAIS4oKCpoKUgoKC3Ki2urKCgoKUgoKCgqamgpSClIKCgoKUgriClIKCgpSCpoKCgpSCgoKUgsqCkoKUgpSCgpSClOiygoKUgoKCkoKCgoKCgpSCgpSCgpSClAACHAANAoKSgpSClIKUgpSmgoKClILKgpSmgoK4lKaCAAgOwoKCuJSClILKgu6CpoKClAACEuKCgqaCgoKUppSCgpSCgoKClAAKCqKmgoKUlIKUgoKCgpSCgpSCgoKUgoKUgoKUgpSmgqaCpoKokqiSggAKCKKCgoqCgriCgoKCgoKCpoKCggAEEPKCgoLIkg==")]
[assembly: GoPositionMap("runtime/netpoll_epoll.go", "netpoll_epoll.cs", "ABMywqqiggAKBoKCgoKClIKCgpSmgoKCgpSmgqaCgoKCpoKC5oIACkgACAKClIKCgoKCgoKCuJSCgoKCgoK4gpSUgoKCgoKmlIKCgpSM0pKUpoKClIKUgoKCuA==")]
[assembly: GoPositionMap("runtime/nonwindows_stub.go", "nonwindows_stub.cs", "ABAmuA==")]
[assembly: GoPositionMap("runtime/os_linux.go", "os_linux.cs", "ADaEAQANDIKCpoKCrLKCgtyCpqYAABDygoKClIKigoKmgpQAKFDq0riCqqKCgqaClJSUgoKClP6ygoKClIKCggALHNjIgqaCuKaUgoLKgoiygoKUgoKCgoKmgpSCpoKCgriCgtyGooKCAAcQtra2gpSmpIKCgoKUgoKCgpSCgoKUlJSmgoKCgqakpoKCgqaCAAIS4qrCgqaYosqssoIAAhIACA6ygqampqamqKKmgpaSlpKmlpKCAAUUooKCyoKUgoKUpoKqooKCgpSCqqKCgqyyyAAJDLIACxiUggAFEgAEFAAKAoKCuIKWkgACFAAJAoKClLimAAwayty4ACGGBOKCgqaCgpSUgoKCpgAIGsKCrLKCyJKiAAca8oKClIKCgpQAAhAADwySgoKUgpSCgpSCgpSCpoKClIKUgpSCyqI=", "94-96:1;183-191:1;549-551:1;974-992:1")]
[assembly: GoPositionMap("runtime/os_linux64.go", "os_linux64.cs", "AAoalqY=")]
[assembly: GoPositionMap("runtime/os_linux_generic.go", "os_linux_generic.cs", "ABMsqKKmgqiS")]
[assembly: GoPositionMap("runtime/os_linux_noauxv.go", "os_linux_noauxv.cs", "AAgS")]
[assembly: GoPositionMap("runtime/os_nonopenbsd.go", "os_nonopenbsd.cs", "AAgWyg==")]
[assembly: GoPositionMap("runtime/os_unix.go", "os_unix.cs", "AA0ikg==")]
[assembly: GoPositionMap("runtime/panic.go", "panic.cs", "ACVwABIClLiCggADEvaCggAHQgAdAoKokoLssoKokoKokoKokoKssoKokoKokoKUgoKUgoKUgoKokoKUgoKokoKUgoLokoKokqSigpSUgpSmgoKCgpSUgpSUppKClJSClKaCgoKCgpSmlIKUlKaSgpSUgpSmgoKClIKClJSClJSm1pKCptaSgqbUgoKm1IKCptSCgqaCgqbUgoLqooKUpoKCgoK4poKCgtaSlLS0tKQAAowBAEMCgpSmgoKCuJSClKiS6qKCgpSCgoKCgpSCAAQQ0oKClIKUgoKCgqaClIKCgoKCpoIAAhIACAKClNyCgoIAChaCggACENKCgoKCgoKCgoKUlIKCgpSClJSUgqiSgsqCpoKmgoKEooKCgoKCgpSUlIKCgqaUlIKssoKUgoKCgpQACUTigoLoxLaCpoKClJS0pLSoooKCgpSCpoKUgoKCgoKUAAIQ0oKCgoKCgpSCgoIAGSiCtAALGAAKAoKClLiCgoKCgqaCgoKClIKCgoKCgoKUgoKCgqaCgpSUgoKCgpQACBKC7pSCzNLcgpSClLiUgoKmpoIACBKCAAgO4pSCgqaCgriCggAJFIKCuKaClKaCgoKCgqaUppSmgszCgqaCkoKCpoKCgoKCAAcQgqaCpqaCgpSm5oKCgqaCpoKCgqaUgoKUyJKCgoIALVyCooKCgoKCgoKCgpS0gpS0goIACAyClIKokqiSqJKokqiSqJKokqiSqJIABn7qgqamgpQAGSwACQKCgsqCgpSmABMogoKCppSUgqaClLgAI0iCgriCgrjK6qQAAhDSgoKUgsqSgqaUuKamAAIQ0oKCgqaS7pSqooKmprimgqYAAh4ADAKCgty4gqa2goKCgpSCyIKCyIKCgraCAAgU4oKCgpSUpoKCgpSCgoKCgoKUgoKCgqa+lMqCppSuwoLKgoKmgoKUgoKClIKClIIAAhDS3AAKFqaUprgAAhTygoKU", "703-713:1;1003-1038:1;1120-1146:1;1431-1446:1;1463-1484:1;1493-1495:2;1597-1599:1", "", "", "", "750:21,751:13")]
[assembly: GoPositionMap("runtime/pinner.go", "pinner.cs", "ADneAaKClILKggAKBoKCgoKUgpSUlAAKPsKCgoK4yoKClJSUgoKClIKClKaSgri4goKUkoKUlLi4poKCAAoYsqaCpoKmgqqigoKUgpQAChbCgoLGgqyyrLKmgqzSgoKmgtyCgoK4goKClLyigoKCgoKUgoKCgpSU6sKCgpSCgoKCgpSCgoKUAAgigg==", "202-205:1;216-219:2")]
[assembly: GoPositionMap("runtime/plugin.go", "plugin.cs", "ABUasoKCgoKUlIKUgpSCpoKCgqaIgoKCgoKCgoKCpoKCgsqClIKUgoKUAAkUgoKCgoKClIKClJQACAaigoKCgqaCyoKCgoKClIKmgrqS", "", "", "50=inRange/2/4/1,inRange/3/4/2,inRange/4/4/3")]
[assembly: GoPositionMap("runtime/preempt.go", "preempt.cs", "AFHUAQAfAriokqaCgoKCggAIKAAKFIK4lIL8gtyCggANHOqCuIK4goKmgoKCgpTu3IKCggAG1QHcpoIACtoBgpSClIIACA6ilKaCpIK8tpQAAxLSAAIeAA0YAAwGgriUgpTuuJTugoKCgoKClIKmgrzUAAomABICyoK4griCAA4egriClJQACBKUgriUAAcQpoKCABAmlLiClLaU", "320-333:1", "", "496=HasPrefix/2/3/1,HasPrefix/3/3/2")]
[assembly: GoPositionMap("runtime/preempt_nonwindows.go", "preempt_nonwindows.cs", "AAgUpg==")]
[assembly: GoPositionMap("runtime/preempt_xreg.go", "preempt_xreg.cs", "AD18goIABhDSlKiSgpSCpoKCAAcQpgAGHAANAoKmgpSSpoLGgoKCgoI=", "122-124:1")]
[assembly: GoPositionMap("runtime/print.go", "print.cs", "ABtW8pSUgoKCgrgABRrygoKCgpSmgoKCgryigpSC3IKCpoKmgqaCAAgGgoKU7oKC3IKC3IKC3IKCporCgqbKgoKClIKCgoKUAAgKgoKCgoKCgpSUgoKCgpSmgqaCgoKClIK0grSCgrSCtoKCgoKCgoKUgqaCpoKUgqaCpqKCgqaCpoI=")]
[assembly: GoPositionMap("runtime/proc.go", "proc.cs", "AIYBkgIAHRiyuMqClNymlIKCAAkUlILKgoKmgoKmpoKCgriClIKCgqaClIKClIKmgriClLiCAAkUgoKCgtyUlIKUpgAHEIKUlIIACBKCgoKC7pSCgpSmgpSClIKmgoKC7rKCgriCuIKCgqaCqJLmgoKCgoKClIKUgqYABSLCAAIQ0qaClAAEaqIAAhoACgKC+gAJEoKCgpSCgoKClJSCpoKCgoKClIIAEgiygpSClIKUgpSClIKUgoKUgoKEooKCgoKCgpSUlIKCgpSC6JLmguaCAAgKooKCpoKSgoKClPyi6JIADyIACAKCgoKUlIKmgoIABxKkkoIAIjqCgqaCgoKUgqyy7qiSgoKokqyygoKUrsKCgoKUAAYWorikgoKCtra2goK2AAgS0oKC2oKCpoKClIKCpraUAAoWAAgCgoKCgoKCgoKCgoKCgoK4lJS4goKmgqaUgoKUgoKCgoKCgoKCgoKUgpSCgoKCppSCuIKC3IK4gpSCgoKCggAJFJSClKaUppSmuKKCguiSAAkUgoKCAAcQwpSClIKCgqiypoKmlIKUppSUgoLKuIKmgpSuwqaUgqwACQ6qkJaAtIKCggACFPKCAAIS4qaCABtmsoIAGDKC3JSClIKUpoKCrsIACA7CppSCggADEIK2goKClOqinoKCgpS4goIADRoACAKCpoK4lpK4goKmpoKUgoKmgriCgrimgpSUggAHEMqCgrSU7oKCpJSUpoLYguqCgs7kggAGFPKClO7CgpSCAAka0oKUgoKUgpQAHDqCpoL8ACW2AbKCrLKCABlcACwalIKCgpS4gqaCgoKCpoKCpoKCgsqCgoKClIKClIKmgpSCgpS4goKClO6CgoKUgoKUgpSCpsqClIKmppQACxzilIKCkoKUlIKCgpSCgoKClJSUgoKCgoKCgpSCpsqClIKClJSCgoLclJSqoqSUqqKklKoAAxYACQKUggAIEoKClIK4poKmuJTuwpSCAAgSgoKUgriCpoKmgqaCgpQAAhIACQiCgpSssoKCAAgaAAoClAALGIKCgoKCgqaCpoKCgoLcuLimgoKCgqaCAAAeAA0CgoKUgqbcgoKClKaC3JSUuO4AAhoACgKCAAsYgoIACxoACQKClIKClIKmgoKmAAcQgoKCuIKmuIKUlIKCgsqCyoKClKaClIKCuIKCggACHAALAriClIKCgoKUAA0uAAkCypSCgsqCgoKkgoKCgoKC2oLKuJKCgoK4pIKmgoK4gpSUlIKmgoIABlQAJwLuggACFgAIAoLKAAgSpri4goLKpoKe0oK4ggAJFJSCgoKmrLKUgr6ygoKClJS67IKCgoKCgoKCgriCgoKUgoKCgoKCptymAAJIACQInMKCuIKCgpSCgoIAECK4ygAHEIKClNyCgpS4goKCgoKUlAAILgAUAoKUgoKUggADHgALAgAVNvKUgoKCgoKUgriClIKUgpSCupKCAAIUAAkCgoKCrsKCrNKCggAtZAARFpSCgoIACxiCgpSCgoKClLiClIKmooKCgpSCgoKClIKUgoKClIKCrsKCyoKCgpSCAAIeAAwCgoKClIKCgoKCgoKCgoKUlIKCggALDKKUgpSClIKmgoKCgoKmlAAMKAAjIoKClIK4lIKCgpSCpoIADh6ClIKUlJSCuIKUgpSClIKUgqaCgqauAAoKgoKmgoKmgoK4goKClIKCgoKCgpSClIKCgoKmgoKCuIKCgsqCgpSCAAciAA8GggAHEJSCgoKCgpSCgtyUlAAICqKUgpSUgpSUgoKCgoKUggAIDsKCgpSCpoKCgoIACAqilIKUgqaCpoKCgoKCgpSCAAIYAAsClLi4goKCgoKCgoKmgqaCgtyCyoKCpoKCgqYAEA7i3IjEgoKClILupoKCgoKCgoKUyoKCgpTcgoKCgoLKgoLKgqaCuIK4goKSgoKClAAMGoKSgoKCgoKCgoKClAAIEoKCpoKUlKamgpQABxCCgoKClIKCgoKUlO6CgoKCgoKUlIIAChamgqaCgoKUgpKCgpSClJSUgoKUgpSCACVMgoKCggANHIKCgoKSgoKUgpSCgqaUgoKCgriCgoKmgoKCgoKUAAgS7oKCgpSClIKCgpSCgqaUlKSCgoKmgpSCgoKCgpSCgoKCgoKCgoKUlIKUlIKCgqaCrsKClIKClIKSgoKmAAISAAgClJSCgpSClJSCggAPIIKCgoKUAAgSgpTKgoIACBIAAhDSgoKCgpSClILKrLKCgoKCyq4ACg6ClIIAEyiCgoKCuIKCgqaCgoKCgqaUrLLKgoLKggAICoKCgpSCgoLKAAzKAaKUgqaCgsqCpoKCyoKmpsoACBKUAAcQgtyCpriCppSCgtyClKaCpgACFPKUgqaCgqiylNyCgqa4uIKCppSCgoKCgoKCgpSCgpS4gqbmooKCgoKClLiClKaCgqaCppSCgqaCpqiSqLKClKaCAAgM0oKCgqa4goKUgoIACRQAFSyCgtymgoK4AAIiAA4CgqaigoK4lIKClIKCqJKCgpSUgoKClKiypriCAAgGooKUgoKClIKCgoKCgoKCgoKCgoKCgpS4goKCppSCgqaCgoKUlILugrgACBoACAKU3KaCgoK4ggAMpgKCgoKClKaClIKUgoKClIKUgoKCgqYAB4IE9IKCgoKCyoKCgoKClIKCgqYAAhIACAKCgoKCyoKUgoKCgoKUgoLugoKClIKCgpTKgpSCAAISAAkI3AADFAAJCIIAAyAADALKgoLcAAIeAAwCppSUAAY0ABcKlLiUAAgWxLiCggAEELKokoKCgpKCgqaCppSssoKCkpSClIIADBDigqaCgoKCgoKUgqaCpoKCgpSClJSmgoKCgoKCgoKCgoKCpoKClNy4goKUuIKCgoKUuIKClIKCgoKCgrimpoKCgqamgpSs5IKUgoKUgoKUgpSCgoKC3IKC6tKCppSUgoKCgoKCuIKCmIKCgpSmgoKCvMKCgpSUgoKCgqaUgpSCgpS4koKCgoKCgriUkoKCppSClIKUgqaospiCgoKUpoKCgqiSAAIQ0oKUgoIABlzSgpSCgpSCAApIggAEMMKmggALFoKCgoKCgtzygtyCAAgSgoKCgqa4ggAJFJSCgoKC3IKClIK4gqaCppSUlKaCgoKUlIKClLiIsoKUlIKCgoKClIKUlKq0gsqCypSClIKClJSCgpSCpgAICsKCgoKCgoKCgoK4lKaCgoKUprim7uKCppSClJSCgri4goKmlIKCgoKCgpSUgoKClIKCgoLcgoKUgpSUgpSCgoKCgoIACBYACAKClIKClIKCgriCgpSmpoKClKaCpoKCgriCgoKUgqaClILugoK4goKUlIKCgoKCgoKCyqaCgsqCgoKCgoKokoKCgoKClIKCgoIAAhTylJSUgoKUgoK4goKUgoKCABo2goKUgpQACRSCgoKCyoKCgqaCgoKUlAACEvSmgoKCvuTuuAAIEgAIApSmgqamgoKClIKmgoKokoKCgpQACAiSlIKUgoKCuJSCgoKmgoKCgpQAEAyy7oLugq7CgqaCgpSCgoKmgoKClIKWuIKCtoKCuIKCpoKmgpSCpoLKgoKCgsqCgriCAA8ksoKCgpSCgoKUgoKCgpSClAAQIoKCgoKCgoKCpoKClIKClIKClIKClIKCpqampoKmgoKCkgAHEIKCgriCgpSUuIKUpoKCgoKCgpSCgpQADh6CprgACBKCppSC3IKCgoKCyrgACBKmgpS4goKCgtyCgsqCgqa4goKUggALNAAQAoIADBqClJSClJTKlJSUlJSClM7ClIKCqqLssoKUgoKCgoKClIKokgAEENKCgoKUgqYAAhoACgKCgpSCgpSUptymgoKmyoKCgqaCgoLKgoKCgoKCgpSUuIKClIKCypSCgoKUpoKCpoKCgoKUlIKClJSCgpSUpoKCgpSUgoKUlJQADB4AOHayAAsYgqbmgoKCgoKClIKmpoKCgoKC7oKCgpS4lKaCgoKCgoK4goKU3IKCgoKCgoIAAxLSgoKClIKCgoKCgqYAAxDClIKUAAIQ0pSCggACENKUgoKUAAIUAAkClJSmgpQAAhDSlAACENKUAAIS4pSCqqKUgqaq0pSCppSClIKClP6SgoKokoKCqJKCgqiSgoKmrsKUgpSUggAKHAANApSClIKUgpSCgoKCgpQAAhTylIKUgpSCgoKClAACGgAKApSCuIKmAA9IAAgCAAgSlIKmgoKCgpSCpqaCgoKCgoKUgqbqwqaCgoKUgpSClJSCgoLKgqamgoKCrOKClIKCgoKCgoKmgoaCgriUrvS4gqaCgoKClIKCztKCgqaCgoKCgpSCpoIACRSCgpSu8oKCgoKCgpSCgoIAEyiCysqClIKmlIKUgoKUggAIENKCgoKUgoKClIKClIIADBiSqLKCgoKUqLKCgpSUgqqigpSCgpSUgqqigoKCgpSUqJKCggANFpKosoKCqJKCgoK6koKCgpTIsoKCgpSUgoIAAh4ADAKClIIAAh4ADAKCqqKqoqqiqqKuAAkMgpSClKqiAAIiAA4CAAIeAAwCABcqgoKCgoLKggAIDoKmgoKmgsaCgpQAFiyCggAJCKKUpLSUqoKUppSmgoKCgqaClJSClIKCgoKCgqY=", "174-176:1;207-211:2;353-353:1;482-484:1;597-604:1;1279-1284:1;1297-1301:2;1317-1319:3;2112-2128:1;2310-2316:1;5277-5282:1;5298-5307:1;5534-5543:1;5547-5552:2;5981-5990:1;6302-6304:1;6309-6316:2;6407-6423:1;6977-6991:1;7580-7582:1", "", "", "", "226:21,227:13")]
[assembly: GoPositionMap("runtime/profbuf.go", "profbuf.cs", "AHbwAYKmgqaCxoKmggAEEOTIksiSrOKCgoKCgoKmgpSClKqigriUgoLcgpSCAAoOooLugpSClIKUlIKCgoKCysKCpoK4goKCppQAAhDygqaCuKaCgqaClIKmgqaClAAI6AGigpSCrLKCgsqCgpSClAAVHMKCpu6CgoKCgoKCpqaCgoKCpoKUpoKCgoKClJSUyoLKlJSCpoKClIKClJSClIKCgriCgpSCgu6CgoKClIK4lAAJFKY=")]
[assembly: GoPositionMap("runtime/proflabel.go", "proflabel.cs", "AAwoABsmgpQAAhgACQI=")]
[assembly: GoPositionMap("runtime/race0.go", "race0.cs", "ABEmgIKSgICikoCAooKCgoKCgoKCgoKCgoKCgoKCkoCAopKAgKI=")]
[assembly: GoPositionMap("runtime/rand.go", "rand.cs", "ACpMooKCpoK4gqa4gqaClKaCgoKCgpSUgpSmggACEAAJCoKCgoKCgpSClIK4goKClOiSgoKUgoKClLyigoKUgqyyAAIUAAsKgoLKgoKUgoK6kqiygoKUgoIAAhDkAAIsABMCuIaCggAHEIKCgoKCAAIqABICAAImABEEAAIWAAgCqJKokg==", "", "", "52=allZero/1/2/3,allZero/2/2/3")]
[assembly: GoPositionMap("runtime/rdebug.go", "rdebug.cs", "AAkUsoKCqLKCgoI=")]
[assembly: GoPositionMap("runtime/retry.go", "retry.cs", "AAgc0oKCgpSU")]
[assembly: GoPositionMap("runtime/runtime.go", "runtime.cs", "AB8+soKCggAVVAAXBIKCuIKCgoKCuIKmlIKmlIKClKaU3oCWgJaAmKIAChKSgoKCqJKCgoIADBaigoKCgpSCgoKUgqaClMaCgoKClIKUgrqSgoKCgoK6koKCggADEMIADTjsgqYAFi6ClAAJHsA=")]
[assembly: GoPositionMap("runtime/runtime1.go", "runtime1.cs", "AB1MAA0CgoKCgoKCppSUAAUWsqaCgoKmgoKUgoK4uIKCpoKCuIIAFA6CgoKClIKUgoKUgpSClIKClIKUgpSClIIAUAiCAA4eAAAQlIKUgpSClIKUgpSClIKUgpSClIKUgpSClIKUgpSCpoKCgpSCpoKClIKmgoKUgqaCgoKmgoKCpoKClIKmgoKUgqaCgpSCpoKClIKmlIKmggBLnAEAJEqUgoKCAAgSlKaClIKCgsqmlIIADh6C+IKCgpSCqqKUlJSCggAIHgAKAoKClIKClLiCgpSClKaCgpSSgpSC3IKCpoKCgoKCggAHEIK6koKUpKSkpLSCgoKUtIKCyoKmlKyygoKokoKClAADJgASAoKCgoKClAAEHAALAgACIgAOAgACHAALAqyyrLKssoKCgoKUgoKCgoKUgqiSqJI=")]
[assembly: GoPositionMap("runtime/runtime_boring.go", "runtime_boring.cs", "AAkUpIKU")]
[assembly: GoPositionMap("runtime/runtime_clearenv.go", "runtime_clearenv.cs", "AA0ckoKUqJKUgoI=")]
[assembly: GoPositionMap("runtime/rwmutex.go", "rwmutex.cs", "ACB4AB4ClIIACAz6gpSUgoKUgriCgoKCggAJDrKCgqaUgoKClKaopIKUlIKUkoKCgqb6xIKCpoKCgoKCgriClA==", "80-96:1;131-136:1")]
[assembly: GoPositionMap("runtime/secret.go", "secret.cs", "AAwekqiSgqiSgqjsgriCpsqCzvyCgoKCggACENIAAhYACAKCrgAIBoKCgoKUgoKC", "39-54:1")]
[assembly: GoPositionMap("runtime/secret_asm.go", "secret_asm.cs", "AAgS")]
[assembly: GoPositionMap("runtime/security_linux.go", "security_linux.cs", "AAkSuII=")]
[assembly: GoPositionMap("runtime/security_unix.go", "security_unix.cs", "AAsagpSCAAgSAAgGgoKCgoKmgg==")]
[assembly: GoPositionMap("runtime/select.go", "select.cs", "ABo0kpaCpoKCgoKCgsoACBKCgoKUuPy4AAYWwoLulJSClKaCAAwkAA8CgoLKgpSCgoKssoKClJKClKaCggALGIKCgqaCgqaCgqamgqaCgoKUgpSCpsqClIKCgoKUlIKCgoKCgoKClIKUgoKClJSmgoKCgtyUAAcWkoKCgoKCgoKUgoKClIKUgqaClIKUgoKUgsqCgoK4gpSCgoKCgoKCpoKCgpSUgpSClKaCysqCgpSUgoLcgoKClIKCgpSUgoKClJSCgoKCpoKClKaCgoKmgqaUgqaCgqamgoKCgqaCgoKCpoKCgoK4gpSEkoKUlIKUgpSCgoKUgoKClIKClISSgpSClIKUgoKClIKClISwkoKUgpSEkoKClIKUlISSlIKUgpSgkoKUlIKClJSEksaCABUukoKUgoKSgoKClIK0grSCtoK4griCgqaCgoKCgpSmpoKUlMaigoKClIKCgoKmgoKClJSCgoLKgoI=", "147-152:1;497-497:2;527-527:3")]
[assembly: GoPositionMap("runtime/sema.go", "sema.cs", "AD5wggAEGgAKAqiSAAIaAAoCqJKokqiSqJKCgpSokqiSpoKClAAHFpIABHiCAASuAYKCgoKUguzCgqaCgoKUgoKClJSCgoKCgoKClIKmgoKClIKClIKCgqaClJSCgoKmlIKClAAOHoKCpoKClIKUAAQYAAsCgpSCgqaClKaUgoKClJSCgoKCgpSCgpSClJSCgsqCgoKCpoKClLiCgpSmlKaCgoKCgoLq1IKClIKCgoKmgoKCgpSClPzUgoKUgoKCgqaCgoKClIKUABo6ogACEPau4qaCgriCpoKCgoKCgoKUgpSUgoKCuIKC7PaCyoKCgtyCpoKCgoKClIK+9oKmpoKCgrgADh6CgoKClJSClIKCgoKUgqbokoKCupI=")]
[assembly: GoPositionMap("runtime/signal_amd64.go", "signal_amd64.cs", "AA4eooKCgoKCgoKCgoKCgoKCgoKCgoKCypCUgoKCluiCgoKCgoKUgriCyoKmuJSCgoKC")]
[assembly: GoPositionMap("runtime/signal_linux_amd64.go", "signal_linux_amd64.cs", "ABMmoqaCgoKCgoKCgoKCgoKCgoKYkJSCgoKCgpSCgoKCyqKCgoKCgoKCgpSCgrSCgrSCgg==", "68-83:1")]
[assembly: GoPositionMap("runtime/signal_unix.go", "signal_unix.cs", "ABo4kqaCgpQAPH6mgoIABRbSlMqCpoKCgsqUpoKCgpSmgrzYlILIuKaCgsqCpgAIqAHygoIABBLClIIADRyClIKmgoKCgpTugoKCAAUSsqaCgpSo5oKUyoKUggAFGAAIBoKmgoIABxCmggADEvKk7oKCgoKUtgACHAALAoKUgoKCgqaClJQABxCClJTKgpSUgpSmlpKCgqaCpoKUgqaCggAKLAAMAoKmgoKClLgAAhLigrgAAxYACgKCgqaCgoKCgqYAChaCgoLKgraWkoKCgpSUABEikqiSAAIcAA8GgoIACxiUpoKUpoKmgqbKgqaU3IKClO6UppQACBKCgoKUgqaCgriCpoKmgu6CpoKUgqaUgoKCgpSCgoKClKaCgpSCgpSUAAwagpS4goKCgpSUgriCgoKCgqaCppSmsoKUpoKmgoKUgoKmlIrC7oKClIKCgpSUggAKKAARAoKCppSCpoKUgrSCpoKUuJSUtJS0pLaUlAAETtKUpoKCgpSCuIIAChaCAAsYgqbKAAcQqJKqooKUgoKCxoLKgoKCpoKCtILGgraCAAsQsoLssoKCggAGENKCAAYQ0riCgpSCppSoAAIS8oKUgqaUgriCgoKmgtyCuIKmyoLugoK4gqYAAhYACAIAAhYACgLqgsoADRyCgqYAAhoACgKCgpQAAhTygoKqooIAAhoACgKCgoKCgpSCAAMYAAgCgoKCpq7CgoIABxAAAxoACQKCgpSClIKUAAsoAAoCgoKCgoKUgoKCggACEPKCgoKCrNKCggACEuKCgoKC", "1074-1104:1", "", "485=FuncPCABIInternal/1/1/2")]
[assembly: GoPositionMap("runtime/sigqueue.go", "sigqueue.cs", "AEWOAbKCgqamgoK4goKCgpSCyoKClLSC2LSCgoKUgvqC7sKUgoKCyoKClLSCgoKUgoLGggAIDIIABBwADgyC3IK+spSCgpS4gqaCgpSCgpSssoKUlIKCrLKClJSCgpSCggACENKCgqyygg==")]
[assembly: GoPositionMap("runtime/sigqueue_note.go", "sigqueue_note.cs", "ABAcguaC5oI=")]
[assembly: GoPositionMap("runtime/sigtab_linux_generic.go", "sigtab_linux_generic.cs", "AAgS")]
[assembly: GoPositionMap("runtime/slice.go", "slice.cs", "ACE6guaCqsKSgoKCgpTKgqaCgoKCuILuuIKCgpSClIKmlAACGAALAoLcgoKUpqaCgoKmgoKmAAZeAC4CgoKClIKUgqaCpqamlILqgpSCgoKCtIKCgoK0gpSUlIKCgoKCtIKCgoKCABAggqaCgsqmgu6mlAACIgAQAoKCAAsYgqaokoKCgqaCgpS43ILcgpQAAhgADQaCyoKCgpSCpoKokoKmgoKmgqaCgoKCgpSCgpSCgqaUlJSokoKUgqrCgoKUlIKCgoKqooKClJSCgoKs0oKClJSCgoKCgoKokoKClJSCgoKCgpSqwpSUgqYACxjugqaqwoIACBKClA==")]
[assembly: GoPositionMap("runtime/softfloat64.go", "softfloat64.cs", "ABo64oKClJSCgpSCuIKCgoLsgqSm4oKClJSCgpSCuIKCgoLsgqSmgqKClIKClIKCgpSCgoKCgqaClIKUgoKmgoKCgpSClIKCgqamgqKClIKClIKCgpSCgoKCgqaClIKUgoKmgoKCgpSClIKCgqamgoKmlKampqamgpSmqIKmgoKCgoKClIKCpoKUpoKmgqaCgqaUpqampriCgoKCpoKCppSmpqamlNaCgqaCgoKUgoKUgqaCgoKClIKClKaCgpSUpqam3Ka4pqKUlKamgpSClKaCgpSCgpSCgpSmgoKCgpSUgoKCgsqCgoKCgqaqopiCgoKCgoKCgoKqopSCuIKCgqaCgoKCgoKClIKCgoKCuIKClIKCgoKCuKaCpoKmlKaCgqaCgqaCgqaCgqaCgqaCgqaCpoKmgqaCpoKCpoKCpoKCpoKCpoKCgpSCgqaCgoKUgoKmgoKmgoKCgqaCgqaCgoKC")]
[assembly: GoPositionMap("runtime/stack.go", "stack.cs", "AKwB1AKCgpSCgpSCgrqSgpQACgqigoKClIKClIKUgpSCgoKCypSClJSCgpSCgpSU6JKCgpSUlIKCggAPIIKCggADEMKCqqKCgoKCgoKUgoKokoKUgoKCgoKCgpSCgqiSgpSCgoKCgoKUgoIADRQACQiCgpSClIKmgoKCgpSssoKCgoKClILKgoKUgoKCgpSClLiUlIKCpoKCgpSUlJSCgpSClKaCgoKCpoKUgpSClIKUgpQAChLigoKCgpSClIKClIKClJSUgoKCgqaClIKUgpSCgoKCgpSCgoKmlIKUgoKUuJSCgqaCgoKUpoLugoKC7pQAK2CigoKClAAHEJSCgoIADSDCggAICsKCgoLcgoKCgqaCgoKCgoKCpoKClIKClIKCgqYAChDEgoKUpoKCgoLulAAYMqa4lIKCpsaCgriCgpTKgoKCgoKClIK4lIKCggAKDqKCgpSCgoKCgqaCgriCgsrYgoKCgrimpqaCgriCgriCgoKCgqassoKokoIACRSUuJqygoKCgriCgoKUpgAKCsKClIKClMqmgoKUgqiSgqaCgsqUAAgSuLjKgoKCgriCgoKWkoKmgoKUytyUlIKUqJKCgpQAFh4ADAKUgpSCgoKCpoLcppSCgoKSgoKClMqCgqaCgoKCygANHIKCpoK4gpSClJSCuIKCgqaCgpSCpqaCuJSCuLiCyoCCgoKCgsi4poKClJSCyriCgpSCqJKqwoKClJSuAAoKgsqCyoIADBqClAAEdqSCgoKCgoKCgoKUlLiCgoKCgoKCpgAQJNKCgpSCgqaCgtyCAAgOwgAIEoKCAAEYAAoCgoKClIKUgoKUpoKUgqY=")]
[assembly: GoPositionMap("runtime/stkframe.go", "stkframe.cs", "AEeQAZKCuIIABh4ADwKCgoKm2pSCppQACxiCgpSUgriCgoKUgqaCgsgACArigpSmgoLKgpS4uIKClLSkgoKCgoKmgpSClIKClIK6koKmgoKClJSClIKUysyUgoKCgoIACBIACwqCoqaCgoKCgqaClA==", "", "", "311=alignUp/1/1/1,getGCMask/1/1/4")]
[assembly: GoPositionMap("runtime/string.go", "string.cs", "ABw40oKCgoKCgpSClIKClILcgpSCgoKUrLKmgqaCpoKs0oKCgoKUlJSmgoKClJSCgoKmrLKmgqaCpoIAAhIACAK4lILKgpSClIKCgpSmgoKUlIKqooKCpsKCgpSUAAIiAA4CgsqClIKUpqKCgoKUlIKmxoKCpoKCgpSmgoKClKaCgsqClIKUgoKClIKClIKUlAANHIKmsoKCgpSUgpSCAAQasoKCgqaCqLKClIKCgqaC6LKCpoKmgpSCrLKCgpSCgqyypoKClIKCAAIgAA4EgqaCgoKClMqCppSCgpSCgriClIKUtLS0xpSCgpSCgpSClJSClJSokoLcgoKCgpSu1IK4lIKUgqaCgriCgpSCgoKUqJKCgqaCgoKCgpSCgpSClJSC", "", "", "157=GetCallerPC/1/1/2,FuncPCABIInternal/1/1/3;224=GetCallerPC/1/1/2,FuncPCABIInternal/1/1/3;280=GetCallerPC/1/1/2,FuncPCABIInternal/1/1/3")]
[assembly: GoPositionMap("runtime/stubs.go", "stubs.cs", "AAswAAwCrAAEIAAPKAAXCKIAAkgAIgaSAAJCAB8GkgAFJAANLAAUAoKuwoIAAhgACgSUgoIAAVwALRwADAKClKgABRwACyIAEAakAAEYAAsEmrKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKClJqyrLKs1qikltYAAhDmAAIWAAoEgoKCgoKCgpas")]
[assembly: GoPositionMap("runtime/stubs2.go", "stubs2.cs", "AAwgtJSClpKu1qas")]
[assembly: GoPositionMap("runtime/stubs3.go", "stubs3.cs", "AAgU")]
[assembly: GoPositionMap("runtime/stubs_amd64.go", "stubs_amd64.cs", "AAkUkoKCgoKClqamsoKCgoKCgoKCgoKCgoKWpqzCmA==")]
[assembly: GoPositionMap("runtime/stubs_linux.go", "stubs_linux.cs", "AAoWntKC")]
[assembly: GoPositionMap("runtime/stubs_nonwasm.go", "stubs_nonwasm.cs", "AAgUgA==")]
[assembly: GoPositionMap("runtime/symtab.go", "symtab.cs", "AEagAbKCggAIhgIACwIAAiAADQKClIKCAAIgAA8GgpSClIKUpoKU3KaUgoKmlKYAAhDSgpSUpoKCAAkUgpTugpQADxyCpoLGuIKCgoKUgoKCpgB+pAIACgKCgpQAAioAEgKCgoKUgoKCgoKCAAwagoKCgrgAGzqCggARIAANBIKElLiClIKCgoKClIKClIKUuIKCgoKmlIKCpoKCggAKLgASAoKCgpSCgqaCgqamlAAGEuKCppSCgoKUgpSUlIKUgoK4qJKClAAZsAGigoKClAAEEuKCgqYACRCCpoLIkgAEGAAJAsgAAiIADwKCgpSUgoKmgqaUgpSCpoKmggAOGoKClAAEFPKClMgADyjCAAoIlrakgqyygoKCgsqCgtyCgoKCgpSCgrimlIKmgoKClJSCgoKCgoKCgoKU7oKCgqaCgoKCgoKC7oKmlMqCppSCgoKCgoKClKaCpoKClKaCgoKCgqaCgqamgoKCpoKmAAIYAAwCgoKUgoKUlIKmguaCgoKClKiSgoKCgoKCgoKClLiCpoKClIKmgoKUgqiSgpSqooKUgoKGooKUgoKotoKClIKClIKUgoKClIKCqJKigoKCgoKUlAAMFOiClA==", "", "", "194=gostring/1/2/3,gostring/2/2/4")]
[assembly: GoPositionMap("runtime/symtabinl.go", "symtabinl.cs", "ADOGAQAQAoKClIKC1oIACRCCyJKCgpSCqJIAAhwACwKClIIACRIABBAACAKC", "", "", "82=pcdatavalue1/1/1/4")]
[assembly: GoPositionMap("runtime/synctest.go", "synctest.cs", "ADBWAA0SgoKUgrSCtoKUgoKCxoLagqaCgoKClIKmuIKUgpSCgoK8woKC6LKCgoKUgoKCurKCAAoWgoKUlJS4puoAEwzSgqaCgpTcgoKClIKSuIKigoKCgqaCgoKCxoKCgpSCgoKUgoKUgpSUlKaCgqaUgoKClJSUprQACQ6CxqKCgpSUlIKokoKClLiCgpSCgpSCgoKUgoK4gviigpSUgoKokqiSgoKUqJKosoKClIKSlKIADjAACgKCptyClJSmgqaCgpSUpoKCgoKCgoKCgqamgpSuwqyyrLI=", "192-194:1;198-204:2;210-217:3;353-355:1", "", "228=Add/1/1/1", "", "241:21,242:13,424:21,425:13")]
[assembly: GoPositionMap("runtime/sys_nonppc64x.go", "sys_nonppc64x.cs", "AAgS")]
[assembly: GoPositionMap("runtime/sys_x86.go", "sys_x86.cs", "AAwgooKCgoKC")]
[assembly: GoPositionMap("runtime/tagptr_64bit.go", "tagptr_64bit.cs", "AD5+ooKCgpTIkqaUgpSokg==")]
[assembly: GoPositionMap("runtime/test_amd64.go", "test_amd64.cs", "/g==")]
[assembly: GoPositionMap("runtime/time.go", "time.cs", "ABEiwoKC7pSokoKClKiSAEiiAeKCggAlUpKUpNrYgqYACRCUAB46goK4ooKUgoKCpoKUxoKCAAkKkoLosqaCqsKCygAGFAAJAoKCgoKUgpSCgoKmlIKCgoKmAAsk8oKmgoKCgoKClJSCgpSUgoKUgriClAADEOKCABEg4oKCgoKUgoKCgoKmgpSCgu7igpQABhDygpSClKyyAAgQAAgCpoKmgpSCgoKCAAUS8oIABxCAgoKUAAkS4oKCpoKCgpSCgoKCpoKUpu6CpoKCgoK4AAgKwoKCgpSCgoKUgoKClIKUAA8Q4oKUgpSUgqaCgpSCgoKCgoKmgoKCgoKmgriCpoK4lAAKFoKClIKSgpSUuKbugqaCgoKUpoKUgqYACArCgoKClJQACCoAHRSCgoKCgpSUlIKCgoKCgoK4lIKCgoKUgoKCgr6yAAoO4oKCgoKC7oLugoKCgoKCgoKUgqaCgqaUpoKCgpQACBQACAKCgriCgoKClIKmgoKCgoIABxLygtyCgoKClAA1bIKUgoKCgoKmlKaClLaCgoKCgoKCgraCgqSmgpSUggADFAALDIKCgpQABhoADgKmgpSmgtyCgpSUlKaCgoKUgoKUlAAHEIKCppQABhYACgKCgoKClIKCgqaUpoKCgqaCppSCpoKCABASAAkCgoKClLiCgoKUlIKUpoKmgoKCgoKUgoKmlIKCgoKCgpSmgpSCuJSUgoKUgpS4gqaUgoKUgqYAEiaUuIK4gsiUgqaCgoKmlKaCpoKCAAsO0oKClLiCgoKmgoK8woKClLyigoKClIIABhCypoKCpqaCppQABh4ACgKCgpSCgpSCgoKUgpSCvKKCgoKUgpSCgpSCgoKUgoKCgoKmgpSClIK8xoKUggAJEMIADBAACAKUlKamgoKClIKUgpSCggALEMKClKaCgoKmyoKCAAgSgoKCAAcU4oKCgoKUgriClA==", "485-487:1;638-643:1;1239-1239:1;1465-1467:1")]
[assembly: GoPositionMap("runtime/time_nofake.go", "time_nofake.cs", "ABBAAA8CABAosoKU")]
[assembly: GoPositionMap("runtime/timeasm.go", "timeasm.cs", "AAsc")]
[assembly: GoPositionMap("runtime/tls_stub.go", "tls_stub.cs", "AAgU")]
[assembly: GoPositionMap("runtime/trace.go", "trace.cs", "AMICrAcACQLKgpSCmAAAFO6UgqaCgqYAChaCgoKClIKCgpSClIK4lMoACBLcgoKClAAJFILcpoKCyoKCgrimAAkUgoKCgu6CgoLuggASJoKCgpSCgpQAARDilIKUgoKClIKC3IKCgoKmpoKCpoKmpoKCgoKmAAkUgoKU3IKUpu6CgoKmgoKCppSm7oKCyoKCyoKCgpSCgpSCpoKCgqaClKYACxiCgoKCgpQABxCUuIK4griUrLKClIKUggAJWgAIApSClKaA7JS4goKmgoKCpoKCgsqClIKCgriC7pSCgoK4poLcuJS47oLclJSmggAKFoKCpoKCggAGEuKCgpSsAA8agpQADRjEgoKClKaUupKCgoIAFCqSgoKCgoKUggAEEPKCgoKUgoKUgoKs9oKClNwACAqClAACFAAKBIKClIKmlIKUgg==", "494-540:1;508-536:1.1;564-570:2;591-604:3;675-725:4;761-765:5;793-816:6;931-931:1;1102-1111:1;1145-1147:1", "", "", "", "755:25,756:17")]
[assembly: GoPositionMap("runtime/traceallocfree.go", "traceallocfree.cs", "ABk0wpiigoKUuIKCgqamgoK4goKmpoK4goKCgpTKkpSmooKUyLKosqiSyLLMsqyyqJLIksiSgqiSgqiSyJLokoKU", "79-81:1")]
[assembly: GoPositionMap("runtime/traceback.go", "traceback.cs", "AHL2AQAKDgAIBsQADRymgoKCgoKmgoKCyoKCgoLKgoKClIIABxDcgqaCgoKClIKUgpSmAAcQgqaCAAYwABcCgpSCpoK4gsqUuMrcgu6CgoKCgoKCgtgABxCClIKCgoK2gpTKlIIAGTSCgpSUgoKCgqaCgsqClAATKIKmAAoWgoKCAAoW6oIACgaigoKmgoKUgsqCgsqUgoKUgpSCgqaUgoKmgoKUuIKCgoKCuIKUgoKCgoKCuOyyAClUgoKCggADHAAKApSmrsKmpoKCgoKCpgAEGAALAoKCgoKmgoKUggAHEIKUuIKmqJKCgqaCgoKCpoKClIKUgqaSlIKCgpSmgoK4goKCpoKCgoKCgpS0goKCtLSCtIK0goKCgoK23sSCgpSCgpSClKiSgqqigoKUgqa0goKCuIKCgoKUgoKClIKCgpSmggACFgAKApSClKa07oKCgpSmlIKClKaCggAuXoKCgoKCgpTKgoKCgoKCppS4gpSUgpSCAAMSAAsGgpSUgoKUgqaCpoKCgoKCgoKCgoKCpoKCgqaCyoKCgpSClIKCgoKUgqa4goKCgoKCgoKCpoKCgriUgpSCuKqigoKCgqaCpoKmAAMQwoKCgoKCgpQABDSCgoKqooKClKqigpSmggAOHoKm7oKmqrSCgpS4goKUgoKUgsqqxqYAEBqilJSClpKClLiCqJKClIKCgoKUpoKClIKUgKykgpSClICCpIKCgoKCgoKmpqaCpoKmgoKCggAJFJKClIKUgpSClIIADRyCgpQABBDSgpSUgpSCppSClIKmgpSCuIKCgoKUgoKUgoIABBwADASCgpSClKamlJSmppSUAAjIAgChAQKCpoamgoKUggAEFAAIEKampgAaOJKCgoKUlKaAkoKCgpSUggACEPKCgoKCgqaCuJSUgoKUgoIABBCygqaUgpSClKyygqaUyoKUgpQ=", "675-684:1;686-701:2;704-708:3;906-929:1;953-968:1;1296-1296:1;1317-1351:1;1388-1401:1;1688-1688:1", "", "221=guintptr/1/1/2;1816=noescape/1/1/2")]
[assembly: GoPositionMap("runtime/tracebuf.go", "tracebuf.cs", "ACNWAAsClIKCgqYABBgACQIABBIADAqmgoKUgqaCgoKUAAIS4qaUgqaCggAEGgAIAoKClAACEuKCgoKUlIIACAiSgoKClIKCgpSCgoLKgoKUgoKmgoK4gpSClIKCgoLKogAEHAALAgAJErKCgpSUqJKCgpSCgpSCpoIAFzTiggACEuKCgoKCgoKUgpQAAhYACAKCggACEuKuwgAGGAAJAoKClJSClIIABRLyAAkUgriC", "144-150:1;157-173:1")]
[assembly: GoPositionMap("runtime/tracecpu.go", "tracecpu.cs", "ABIcsoK4ggAHEILuwoKmgoKSAAoWlIKUlIKCgqaU7LKCAAkUgoKCgqamgoKCAAIgAA0ClIKCgpSClIK4goKCgpSCgrimypSmuIKCgriWkoKUuKaCgoKCgpSUqrSAgpKCgoIABRDyppSmAAcQgoKClIKmgpSmisKmlJSClKaUprimpoI=", "44-70:1;201-206:1")]
[assembly: GoPositionMap("runtime/traceevent.go", "traceevent.cs", "AB9QAA0CgpSClKyyyLLMsgACEOQABRKyrLKokg==", "", "", "87=startPCForTrace/1/1/2")]
[assembly: GoPositionMap("runtime/tracemap.go", "tracemap.cs", "AD+AAaIAAhLigpSUgoKCgoIADyCClILKlIKCpoIABTLCgoI=")]
[assembly: GoPositionMap("runtime/traceregion.go", "traceregion.cs", "ADVWkoKClIK4goKCgrqSpriCgoKCgoK4griCgsqCpoKUruKCgoKClIKClA==", "63-98:1")]
[assembly: GoPositionMap("runtime/traceruntime.go", "traceruntime.cs", "ABYqkgAfRraCgoKCgoKCggACEuIAHUAAHUAABRKyrsKokgARLtKClAACFAAKCKbKgoIACBIACRSCgoKClIIABBDSAAQQ0oKUlKyyqJLIkqyyuKi2rsKmrsKmrsKmqLaotgAGFAAJBoKClAACFgAIAoKCgoKUAAcS0oKClIKClKiSqJKosoKCgpSssoKCgoKCvrKokqiSqJKuwqjEgqrUgoKCgpSqwri+xIKCAAIYAAkCgoKCgpSUrOSC7qYACBKCgoKClKiSqJKmlJQAAhIACgbKrtSC3AAEEuKClJSCrLKClJSCAAIS4oKUlIKUtLSUgqyygpSUggAGEuIACBKClIKCgoKCpoK4lA==", "699-712:1")]
[assembly: GoPositionMap("runtime/tracestack.go", "tracestack.cs", "ABxAAAoClpKCgriClMq0gpS0gsqmlILugoKCgriCgoIAECKCgpSC7oKUlJTKgpSClIIACxjGgpSCrLKCgoKUgsaiuIK4ntKCgriCgoKCgoKCuIKCgpSUqqKCgoKCgoIACxqSgpSCgoKUgoKCgpSCqqKu4pSUlKiSAAIYAAkCgoKmpv6CgoKClKiCgoKCpoKUpoKCgpSClMassoKClIKClA==", "306-314:1")]
[assembly: GoPositionMap("runtime/tracestatus.go", "tracestatus.cs", "ABQo9IKCuIKUuIKUAAYYAAsCgpSClIKmxtyCxqSCAAYYAAoEgoK4poKUAAoWAAgEkpS0tAAKEoKCxrSkABg84oKUgqiSgoKokqqiqJKC")]
[assembly: GoPositionMap("runtime/tracestring.go", "tracestring.cs", "AB02xIKClIKmqJKCpoKClAACEAAIBIKmgpaSgpS4goKCpoKu4oKCgoKUuA==", "33-35:1;47-49:1;92-96:1")]
[assembly: GoPositionMap("runtime/tracetime.go", "tracetime.cs", "AC50AAsCgpQABCKCpqamgqaCgoKUgoKCgoKCppKCgg==", "107-111:1")]
[assembly: GoPositionMap("runtime/tracetype.go", "tracetype.cs", "ABo04oKmkqyygoKUgsaigqae0oKUuIKCgoKCpoKCgpSU")]
[assembly: GoPositionMap("runtime/type.go", "type.cs", "AB4ikgALLIKmgoKUgoKCgpS0pJSuwoKUlIKkgpQABBDylJQABxQACw6UgqaCgpTagsaCuIKCgriCAA0c1LiCppSCgoKUgoKCpoKmgoKCgpSCpoIACgoADBKCgpSUgriUgqaClIKCgsaCgoKCgpS4gpSUlIKCxgAaMIKCgriCgpQACAaCgpSCgoKCgoKUyoKCgoKCgpSUxoIACgaCppSCgoKCgqaCgoKCgoKClJSUgpSCgoKUxoLmgqaUgoKCgoKmgoKCgoKCgpSUlIIABCqCgpSCgoKClIKCgqqigpSUgoKUgoKCgpSmgoKCpsa4goKCgoKCgoKCprgACBKCAAYeAA8CgoLclIKUgoKUgoKUgoKCgpSCgoKmgpSUpIKCpIKCpIKCgpSCgoKmgoKCpqSCgoKUgpSCgqaCgoKUgpSCgoKmpIKCpIKCpIKCpIKCgpSClIKCgoKUgpSClIKUgqakgoI=", "135-137:1")]
[assembly: GoPositionMap("runtime/unsafe.go", "unsafe.cs", "AAwagoKmgoKUupKCgpTmgriC+ILmgqiSgqaCgriCgoKUupKCgpTmgriCuKbokoKmpuiSgqiS")]
[assembly: GoPositionMap("runtime/utf8.go", "utf8.cs", "ACRWkoKClAACGAAMApSCppSmgoKCguqCgoKC6oKCgoLaqrSkgqSCgoKkgqSCgoKCpIKCgoKC")]
[assembly: GoPositionMap("runtime/valgrind0.go", "valgrind0.cs", "AA0egoKCgoKCgoKCgg==")]
[assembly: GoPositionMap("runtime/vdso_linux.go", "vdso_linux.cs", "ADmCAYCCACFMsoKUmKKCgoKUgoKCgsjIgsqSgoKCgoKCgpS0tLS0tMiCpoKmlIKCgoKCpoKCgrimooKmgoKCgoK4gpSmpqKCpoKCgpSClIKmgqaCppSCgoKCyriCgoKUgoKUgqaU3IKUlJSmgoLOsg==", "216-234:1")]
[assembly: GoPositionMap("runtime/vdso_linux_amd64.go", "vdso_linux_amd64.cs", "AA0etA==")]
[assembly: GoPositionMap("runtime/vgetrandom_linux.go", "vgetrandom_linux.cs", "AA0gABMUgoKmjIKUgoKU5oKCgoKCgoKCgoKUgoKClIKClIKmgoKCrLKCpoKCrLKCyoKCAAwalIKCgoKClJQ=")]
[assembly: GoPositionMap("runtime/write_err.go", "write_err.cs", "AAgUkoI=")]
// </GoSourcePositionMaps>

namespace go;

// <GlyphForwarderUsings> (scratch prototype: the using directives the forwarders' signatures read)
using abi = @internal.abi_package;
using goarch = @internal.goarch_package;
using atomic = @internal.runtime.atomic_package;
using Δmath = @internal.runtime.math_package;
using sys = @internal.runtime.sys_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;
using static runtime_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using byteorder = @internal.byteorder_package;
using bytealg = @internal.bytealg_package;
using runtime = runtime_package;
using goexperiment = @internal.goexperiment_package;
using goos = @internal.goos_package;
using gc = @internal.runtime.gc_package;
using godebugs = @internal.godebugs_package;
using ꓸꓸꓸstatDep = Span<runtime_package.statDep>;
using cpu = @internal.cpu_package;
using scan = @internal.runtime.gc.scan_package;
using @internal.runtime.gc;
using strconv = @internal.strconv_package;
using FieldOffsetAttribute = global::System.Runtime.InteropServices.FieldOffsetAttribute;
using LayoutKind = global::System.Runtime.InteropServices.LayoutKind;
using StructLayoutAttribute = global::System.Runtime.InteropServices.StructLayoutAttribute;
using profilerecord = @internal.profilerecord_package;
using linux = @internal.runtime.syscall.linux_package;
using @internal.runtime.syscall;
using stringslite = @internal.stringslite_package;
using exithook = @internal.runtime.exithook_package;
using bits = go.math.bits_package;
using go.math;
using tracev2 = @internal.trace.tracev2_package;
using @internal.trace;
using label = @internal.runtime.pprof.label_package;
using @internal.runtime.pprof;
using ꓸꓸꓸtraceArg = Span<runtime_package.traceArg>;
// </GlyphForwarderUsings>

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
    internal partial struct cgoCallers {}
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
    [GoValueClone("significand")] internal partial struct fpreg1 {}
    [GoValueClone("_st", "_xmm", "padding")] internal partial struct fpstate {}
    [GoValueClone("_st", "_xmm", "padding")] internal partial struct fpstate1 {}
    [GoValueClone("significand", "padding")] internal partial struct fpxreg {}
    [GoValueClone("significand", "padding")] internal partial struct fpxreg1 {}
    internal partial struct funcInfo {}
    internal partial struct functab {}
    internal partial struct gList {}
    internal partial struct gQueue {}
    [GoValueClone("traceSchedResourceState")] internal partial struct gTraceState {}
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
    internal partial struct hchan {}
    internal partial struct headTailIndex {}
    [GoValueClone("spans", "pageInUse", "pageMarks", "pageSpecials", "pageUseSpanInlineMarkBits")] internal partial struct heapArena {}
    [GoValueClone("heapStatsDelta")] internal partial struct heapStatsAggregate {}
    [GoValueClone("smallAllocCount", "smallFreeCount")] internal partial struct heapStatsDelta {}
    internal partial struct hex {}
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
    [GoValueClone("gregs", "__reserved1")] internal partial struct mcontext {}
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
    [GoValueClone("traceSchedResourceState")] internal partial struct pTraceState {}
    [GoValueClone("summary", "chunks")] internal partial struct pageAlloc {}
    internal partial struct pageAlloc_scav {}
    internal partial struct pageBits {}
    internal partial struct pageCache {}
    internal partial struct pallocBits {}
    [GoValueClone("pallocBits", "scavenged")] internal partial struct pallocData {}
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
    internal partial struct rtype {}
    internal partial struct runtimeSelect {}
    internal partial struct rwmutex {}
    internal partial struct scase {}
    internal partial struct scavChunkData {}
    internal partial struct scavChunkFlags {}
    internal partial struct scavengeIndex {}
    internal partial struct scavengerState {}
    internal partial struct scavengeᴛ1 {}
    internal partial struct schedStatsAggregate {}
    internal partial struct selectDir {}
    internal partial struct semTable {}
    [GoValueClone("pad")] internal partial struct semTableᴛ1 {}
    internal partial struct semaProfileFlags {}
    internal partial struct semaRoot {}
    internal partial struct sigTabT {}
    internal partial struct sigactiont {}
    [GoValueClone("__reserved1")] internal partial struct sigcontext {}
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
    [GoValueClone("spanSetBlockHeader2", "spans")] internal partial struct spanSetBlock {}
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
    internal partial struct srcFunc {}
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
    [GoValueClone("ensured", "heapStats")] internal partial struct statAggregate {}
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
    [GoValueClone("full", "doneSema", "stackTab", "stringTab", "typeTab", "cpuLogRead", "cpuLogWrite", "cpuBuf", "markWorkerLabels", "goStopReasons", "goBlockReasons")] internal partial struct traceᴛ1 {}
    [GoValueClone("t")] internal partial struct typeCacheBucket {}
    internal partial struct typePointers {}
    [GoValueClone("uc_stack", "uc_mcontext", "uc_sigmask", "__fpregs_mem")] internal partial struct ucontext {}
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
    internal partial struct writeUserArenaHeapBits {}
    internal partial struct xRegAllocᴛ1 {}
    internal partial struct xRegPerG {}
    [GoValueClone("scratch")] internal partial struct xRegPerP {}
    [GoValueClone("regs")] internal partial struct xRegState {}
    [GoValueClone("Z0", "Z1", "Z2", "Z3", "Z4", "Z5", "Z6", "Z7", "Z8", "Z9", "Z10", "Z11", "Z12", "Z13", "Z14", "Z15", "Z16", "Z17", "Z18", "Z19", "Z20", "Z21", "Z22", "Z23", "Z24", "Z25", "Z26", "Z27", "Z28", "Z29", "Z30", "Z31")] internal partial struct xRegs {}
    [GoValueClone("element")] internal partial struct xmmreg {}
    [GoValueClone("element")] internal partial struct xmmreg1 {}
    internal partial struct Δslice {}
    public partial interface Error {}
    [GoValueClone("StackRecord")] public partial struct BlockProfileRecord {}
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

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(runtime_package.Cleanup)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛCleanup;
[GoMethodHost(typeof(runtime_package.Frames)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛFrames;
[GoMethodHost(typeof(runtime_package.Func)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛFunc;
[GoMethodHost(typeof(runtime_package.MemProfileRecord)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛMemProfileRecord;
[GoMethodHost(typeof(runtime_package.PanicNilError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛPanicNilError;
[GoMethodHost(typeof(runtime_package.Pinner)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛPinner;
[GoMethodHost(typeof(runtime_package.StackRecord)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛStackRecord;
[GoMethodHost(typeof(runtime_package.TypeAssertionError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛTypeAssertionError;
[GoMethodHost(typeof(runtime_package._func)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛ_func;
[GoMethodHost(typeof(runtime_package._panic)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛ_panic;
[GoMethodHost(typeof(runtime_package.activeSweep)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛactiveSweep;
[GoMethodHost(typeof(runtime_package.addrRange)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛaddrRange;
[GoMethodHost(typeof(runtime_package.addrRanges)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛaddrRanges;
[GoMethodHost(typeof(runtime_package.arenaIdx)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛarenaIdx;
[GoMethodHost(typeof(runtime_package.atomicHeadTailIndex)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛatomicHeadTailIndex;
[GoMethodHost(typeof(runtime_package.atomicMSpanPointer)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛatomicMSpanPointer;
[GoMethodHost(typeof(runtime_package.atomicOffAddr)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛatomicOffAddr;
[GoMethodHost(typeof(runtime_package.atomicScavChunkData)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛatomicScavChunkData;
[GoMethodHost(typeof(runtime_package.atomicSpanSetSpinePointer)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛatomicSpanSetSpinePointer;
[GoMethodHost(typeof(runtime_package.bitCursor)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛbitCursor;
[GoMethodHost(typeof(runtime_package.bitvector)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛbitvector;
[GoMethodHost(typeof(runtime_package.boundsError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛboundsError;
[GoMethodHost(typeof(runtime_package.bucket)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛbucket;
[GoMethodHost(typeof(runtime_package.chunkIdx)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛchunkIdx;
[GoMethodHost(typeof(runtime_package.cleanupBlock)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛcleanupBlock;
[GoMethodHost(typeof(runtime_package.cleanupQueue)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛcleanupQueue;
[GoMethodHost(typeof(runtime_package.consistentHeapStats)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛconsistentHeapStats;
[GoMethodHost(typeof(runtime_package.cpuProfile)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛcpuProfile;
[GoMethodHost(typeof(runtime_package.cpuStats)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛcpuStats;
[GoMethodHost(typeof(runtime_package.cpuStatsAggregate)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛcpuStatsAggregate;
[GoMethodHost(typeof(runtime_package.debugLogReader)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛdebugLogReader;
[GoMethodHost(typeof(runtime_package.debugLogWriter)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛdebugLogWriter;
[GoMethodHost(typeof(runtime_package.dloggerFake)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛdloggerFake;
[GoMethodHost(typeof(runtime_package.dloggerImpl)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛdloggerImpl;
[GoMethodHost(typeof(runtime_package.errorAddressString)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛerrorAddressString;
[GoMethodHost(typeof(runtime_package.errorString)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛerrorString;
[GoMethodHost(typeof(runtime_package.finalStatsAggregate)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛfinalStatsAggregate;
[GoMethodHost(typeof(runtime_package.fixalloc)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛfixalloc;
[GoMethodHost(typeof(runtime_package.funcInfo)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛfuncInfo;
[GoMethodHost(typeof(runtime_package.g)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛg;
[GoMethodHost(typeof(runtime_package.gList)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛgList;
[GoMethodHost(typeof(runtime_package.gQueue)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛgQueue;
[GoMethodHost(typeof(runtime_package.gTraceState)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛgTraceState;
[GoMethodHost(typeof(runtime_package.gcBits)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛgcBits;
[GoMethodHost(typeof(runtime_package.gcBitsArena)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛgcBitsArena;
[GoMethodHost(typeof(runtime_package.gcCPULimiterState)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛgcCPULimiterState;
[GoMethodHost(typeof(runtime_package.gcControllerState)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛgcControllerState;
[GoMethodHost(typeof(runtime_package.gcStatsAggregate)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛgcStatsAggregate;
[GoMethodHost(typeof(runtime_package.gcTrigger)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛgcTrigger;
[GoMethodHost(typeof(runtime_package.gcWork)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛgcWork;
[GoMethodHost(typeof(runtime_package.gclinkptr)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛgclinkptr;
[GoMethodHost(typeof(runtime_package.godebugInc)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛgodebugInc;
[GoMethodHost(typeof(runtime_package.goroutineProfileStateHolder)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛgoroutineProfileStateHolder;
[GoMethodHost(typeof(runtime_package.guintptr)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛguintptr;
[GoMethodHost(typeof(runtime_package.hchan)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛhchan;
[GoMethodHost(typeof(runtime_package.headTailIndex)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛheadTailIndex;
[GoMethodHost(typeof(runtime_package.heapStatsAggregate)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛheapStatsAggregate;
[GoMethodHost(typeof(runtime_package.heapStatsDelta)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛheapStatsDelta;
[GoMethodHost(typeof(runtime_package.hexdumpMarker)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛhexdumpMarker;
[GoMethodHost(typeof(runtime_package.hexdumper)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛhexdumper;
[GoMethodHost(typeof(runtime_package.immortalWeakHandle)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛimmortalWeakHandle;
[GoMethodHost(typeof(runtime_package.immortalWeakHandleMap)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛimmortalWeakHandleMap;
[GoMethodHost(typeof(runtime_package.inlineFrame)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛinlineFrame;
[GoMethodHost(typeof(runtime_package.inlineUnwinder)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛinlineUnwinder;
[GoMethodHost(typeof(runtime_package.itabTableType)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛitabTableType;
[GoMethodHost(typeof(runtime_package.kernelVersion)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛkernelVersion;
[GoMethodHost(typeof(runtime_package.lfstack)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛlfstack;
[GoMethodHost(typeof(runtime_package.limiterEvent)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛlimiterEvent;
[GoMethodHost(typeof(runtime_package.limiterEventStamp)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛlimiterEventStamp;
[GoMethodHost(typeof(runtime_package.linearAlloc)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛlinearAlloc;
[GoMethodHost(typeof(runtime_package.listHead)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛlistHead;
[GoMethodHost(typeof(runtime_package.listHeadManual)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛlistHeadManual;
[GoMethodHost(typeof(runtime_package.lockRank)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛlockRank;
[GoMethodHost(typeof(runtime_package.m)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛm;
[GoMethodHost(typeof(runtime_package.mLockProfile)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛmLockProfile;
[GoMethodHost(typeof(runtime_package.mProfCycleHolder)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛmProfCycleHolder;
[GoMethodHost(typeof(runtime_package.mSpanList)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛmSpanList;
[GoMethodHost(typeof(runtime_package.mSpanStateBox)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛmSpanStateBox;
[GoMethodHost(typeof(runtime_package.mWeakPointer)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛmWeakPointer;
[GoMethodHost(typeof(runtime_package.markBits)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛmarkBits;
[GoMethodHost(typeof(runtime_package.maybeTraceableChan)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛmaybeTraceableChan;
[GoMethodHost(typeof(runtime_package.maybeTraceablePtr)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛmaybeTraceablePtr;
[GoMethodHost(typeof(runtime_package.mcache)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛmcache;
[GoMethodHost(typeof(runtime_package.mcentral)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛmcentral;
[GoMethodHost(typeof(runtime_package.memRecordCycle)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛmemRecordCycle;
[GoMethodHost(typeof(runtime_package.metricReader)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛmetricReader;
[GoMethodHost(typeof(runtime_package.metricValue)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛmetricValue;
[GoMethodHost(typeof(runtime_package.mheap)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛmheap;
[GoMethodHost(typeof(runtime_package.moduledata)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛmoduledata;
[GoMethodHost(typeof(runtime_package.mspan)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛmspan;
[GoMethodHost(typeof(runtime_package.muintptr)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛmuintptr;
[GoMethodHost(typeof(runtime_package.notInHeap)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛnotInHeap;
[GoMethodHost(typeof(runtime_package.objptr)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛobjptr;
[GoMethodHost(typeof(runtime_package.offAddr)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛoffAddr;
[GoMethodHost(typeof(runtime_package.p)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛp;
[GoMethodHost(typeof(runtime_package.pMask)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛpMask;
[GoMethodHost(typeof(runtime_package.pageAlloc)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛpageAlloc;
[GoMethodHost(typeof(runtime_package.pageBits)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛpageBits;
[GoMethodHost(typeof(runtime_package.pageCache)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛpageCache;
[GoMethodHost(typeof(runtime_package.pallocBits)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛpallocBits;
[GoMethodHost(typeof(runtime_package.pallocData)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛpallocData;
[GoMethodHost(typeof(runtime_package.pallocSum)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛpallocSum;
[GoMethodHost(typeof(runtime_package.piController)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛpiController;
[GoMethodHost(typeof(runtime_package.pinState)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛpinState;
[GoMethodHost(typeof(runtime_package.pinner)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛpinner;
[GoMethodHost(typeof(runtime_package.pinnerBits)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛpinnerBits;
[GoMethodHost(typeof(runtime_package.plainError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛplainError;
[GoMethodHost(typeof(runtime_package.pollCache)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛpollCache;
[GoMethodHost(typeof(runtime_package.pollDesc)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛpollDesc;
[GoMethodHost(typeof(runtime_package.pollInfo)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛpollInfo;
[GoMethodHost(typeof(runtime_package.profAtomic)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛprofAtomic;
[GoMethodHost(typeof(runtime_package.profBuf)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛprofBuf;
[GoMethodHost(typeof(runtime_package.profIndex)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛprofIndex;
[GoMethodHost(typeof(runtime_package.puintptr)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛpuintptr;
[GoMethodHost(typeof(runtime_package.randomEnum)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛrandomEnum;
[GoMethodHost(typeof(runtime_package.randomOrder)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛrandomOrder;
[GoMethodHost(typeof(runtime_package.rtype)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛrtype;
[GoMethodHost(typeof(runtime_package.rwmutex)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛrwmutex;
[GoMethodHost(typeof(runtime_package.scavChunkData)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛscavChunkData;
[GoMethodHost(typeof(runtime_package.scavChunkFlags)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛscavChunkFlags;
[GoMethodHost(typeof(runtime_package.scavengeIndex)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛscavengeIndex;
[GoMethodHost(typeof(runtime_package.scavengerState)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛscavengerState;
[GoMethodHost(typeof(runtime_package.schedStatsAggregate)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛschedStatsAggregate;
[GoMethodHost(typeof(runtime_package.semTable)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛsemTable;
[GoMethodHost(typeof(runtime_package.semaRoot)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛsemaRoot;
[GoMethodHost(typeof(runtime_package.sigctxt)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛsigctxt;
[GoMethodHost(typeof(runtime_package.spanAllocType)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛspanAllocType;
[GoMethodHost(typeof(runtime_package.spanClass)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛspanClass;
[GoMethodHost(typeof(runtime_package.spanInlineMarkBits)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛspanInlineMarkBits;
[GoMethodHost(typeof(runtime_package.spanQueue)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛspanQueue;
[GoMethodHost(typeof(runtime_package.spanSPMC)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛspanSPMC;
[GoMethodHost(typeof(runtime_package.spanScanOwnership)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛspanScanOwnership;
[GoMethodHost(typeof(runtime_package.spanSet)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛspanSet;
[GoMethodHost(typeof(runtime_package.spanSetBlockAlloc)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛspanSetBlockAlloc;
[GoMethodHost(typeof(runtime_package.spanSetSpinePointer)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛspanSetSpinePointer;
[GoMethodHost(typeof(runtime_package.specialsIter)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛspecialsIter;
[GoMethodHost(typeof(runtime_package.srcFunc)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛsrcFunc;
[GoMethodHost(typeof(runtime_package.stackObject)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛstackObject;
[GoMethodHost(typeof(runtime_package.stackObjectRecord)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛstackObjectRecord;
[GoMethodHost(typeof(runtime_package.stackScanState)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛstackScanState;
[GoMethodHost(typeof(runtime_package.statAggregate)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛstatAggregate;
[GoMethodHost(typeof(runtime_package.statDepSet)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛstatDepSet;
[GoMethodHost(typeof(runtime_package.stkframe)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛstkframe;
[GoMethodHost(typeof(runtime_package.stwReason)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛstwReason;
[GoMethodHost(typeof(runtime_package.sweepClass)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛsweepClass;
[GoMethodHost(typeof(runtime_package.sweepLocked)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛsweepLocked;
[GoMethodHost(typeof(runtime_package.sweepLocker)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛsweepLocker;
[GoMethodHost(typeof(runtime_package.synctestBubble)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛsynctestBubble;
[GoMethodHost(typeof(runtime_package.synctestDeadlockError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛsynctestDeadlockError;
[GoMethodHost(typeof(runtime_package.sysMemStat)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛsysMemStat;
[GoMethodHost(typeof(runtime_package.sysStatsAggregate)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛsysStatsAggregate;
[GoMethodHost(typeof(runtime_package.syscallingThread)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛsyscallingThread;
[GoMethodHost(typeof(runtime_package.taggedPointer)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛtaggedPointer;
[GoMethodHost(typeof(runtime_package.ticksType)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛticksType;
[GoMethodHost(typeof(runtime_package.timeHistogram)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛtimeHistogram;
[GoMethodHost(typeof(runtime_package.timer)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛtimer;
[GoMethodHost(typeof(runtime_package.timerWhen)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛtimerWhen;
[GoMethodHost(typeof(runtime_package.timers)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛtimers;
[GoMethodHost(typeof(runtime_package.timespec)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛtimespec;
[GoMethodHost(typeof(runtime_package.timeval)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛtimeval;
[GoMethodHost(typeof(runtime_package.traceAdvancerState)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛtraceAdvancerState;
[GoMethodHost(typeof(runtime_package.traceBuf)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛtraceBuf;
[GoMethodHost(typeof(runtime_package.traceBufQueue)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛtraceBufQueue;
[GoMethodHost(typeof(runtime_package.traceEventWriter)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛtraceEventWriter;
[GoMethodHost(typeof(runtime_package.traceLocker)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛtraceLocker;
[GoMethodHost(typeof(runtime_package.traceMap)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛtraceMap;
[GoMethodHost(typeof(runtime_package.traceRegionAlloc)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛtraceRegionAlloc;
[GoMethodHost(typeof(runtime_package.traceSchedResourceState)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛtraceSchedResourceState;
[GoMethodHost(typeof(runtime_package.traceStackTable)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛtraceStackTable;
[GoMethodHost(typeof(runtime_package.traceStringTable)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛtraceStringTable;
[GoMethodHost(typeof(runtime_package.traceTypeTable)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛtraceTypeTable;
[GoMethodHost(typeof(runtime_package.traceWriter)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛtraceWriter;
[GoMethodHost(typeof(runtime_package.typePointers)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛtypePointers;
[GoMethodHost(typeof(runtime_package.unwinder)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛunwinder;
[GoMethodHost(typeof(runtime_package.userArena)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛuserArena;
[GoMethodHost(typeof(runtime_package.waitReason)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛwaitReason;
[GoMethodHost(typeof(runtime_package.waitq)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛwaitq;
[GoMethodHost(typeof(runtime_package.wakeableSleep)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛwakeableSleep;
[GoMethodHost(typeof(runtime_package.wbBuf)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛwbBuf;
[GoMethodHost(typeof(runtime_package.workbuf)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛworkbuf;
[GoMethodHost(typeof(runtime_package.writeUserArenaHeapBits)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛwriteUserArenaHeapBits;
[GoMethodHost(typeof(runtime_package.xRegPerP)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class runtimeᴛxRegPerP;
// </MethodHolders>

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies in the package class)
partial class runtimeᴛuserArena {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @unsafe.Pointer alloc(this ref userArena a, ж<_type> typ, nint cap) => runtime_package.allocᴍ(ref a, typ, cap);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<mspan> refill(this ref userArena a) => runtime_package.refillᴍ(ref a);
}
partial class runtimeᴛmspan {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @unsafe.Pointer userArenaNextFree(this ж<mspan> Ꮡs, ж<_type> Ꮡtyp, nint cap) => runtime_package.userArenaNextFreeᴍ(Ꮡs, Ꮡtyp, cap);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static writeUserArenaHeapBits /*h*/ writeUserArenaHeapBits(this ref mspan s, uintptr addr) => runtime_package.writeUserArenaHeapBitsᴍ(ref s, addr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool isUnusedUserArenaChunk(this ref mspan s) => runtime_package.isUnusedUserArenaChunkᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void setUserArenaChunkToFault(this ж<mspan> Ꮡs) => runtime_package.setUserArenaChunkToFaultᴍ(Ꮡs);
}
partial class runtimeᴛwriteUserArenaHeapBits {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static writeUserArenaHeapBits write(this writeUserArenaHeapBits h, ж<mspan> Ꮡs, uintptr bits, uintptr valid) => runtime_package.writeᴍ(h, Ꮡs, bits, valid);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static writeUserArenaHeapBits pad(this writeUserArenaHeapBits h, ж<mspan> s, uintptr size) => runtime_package.padᴍ(h, s, size);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void flush(this writeUserArenaHeapBits h, ж<mspan> Ꮡs, uintptr addr, uintptr size) => runtime_package.flushᴍ(h, Ꮡs, addr, size);
}
partial class runtimeᴛmheap {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<mspan> allocUserArenaChunk(this ж<mheap> Ꮡh) => runtime_package.allocUserArenaChunkᴍ(Ꮡh);
}
partial class runtimeᴛwaitq {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void enqueue(this ref waitq q, ж<sudog> Ꮡsgp) => runtime_package.enqueueᴍ(ref q, Ꮡsgp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<sudog> dequeue(this ref waitq q) => runtime_package.dequeueᴍ(ref q);
}
partial class runtimeᴛhchan {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @unsafe.Pointer raceaddr(this ж<hchan> Ꮡc) => runtime_package.raceaddrᴍ(Ꮡc);
}
partial class runtimeᴛcpuProfile {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void add(this ref cpuProfile p, ж<@unsafe.Pointer> tagPtr, slice<uintptr> stk) => runtime_package.addᴍ(ref p, tagPtr, stk);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void addNonGo(this ref cpuProfile p, slice<uintptr> stk) => runtime_package.addNonGoᴍ(ref p, stk);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void addExtra(this ref cpuProfile p) => runtime_package.addExtraᴍ(ref p);
}
partial class runtimeᴛdloggerFake {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void end(this dloggerFake l) => runtime_package.endᴍ(l);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static dloggerFake b(this dloggerFake l, bool x) => runtime_package.bᴍ(l, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static dloggerFake i(this dloggerFake l, nint x) => runtime_package.iᴍ(l, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static dloggerFake i8(this dloggerFake l, int8 x) => runtime_package.i8ᴍ(l, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static dloggerFake i16(this dloggerFake l, int16 x) => runtime_package.i16ᴍ(l, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static dloggerFake i32(this dloggerFake l, int32 x) => runtime_package.i32ᴍ(l, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static dloggerFake i64(this dloggerFake l, int64 x) => runtime_package.i64ᴍ(l, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static dloggerFake u(this dloggerFake l, nuint x) => runtime_package.uᴍ(l, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static dloggerFake uptr(this dloggerFake l, uintptr x) => runtime_package.uptrᴍ(l, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static dloggerFake u8(this dloggerFake l, uint8 x) => runtime_package.u8ᴍ(l, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static dloggerFake u16(this dloggerFake l, uint16 x) => runtime_package.u16ᴍ(l, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static dloggerFake u32(this dloggerFake l, uint32 x) => runtime_package.u32ᴍ(l, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static dloggerFake u64(this dloggerFake l, uint64 x) => runtime_package.u64ᴍ(l, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static dloggerFake hex(this dloggerFake l, uint64 x) => runtime_package.hexᴍ(l, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static dloggerFake p(this dloggerFake l, any x) => runtime_package.pᴍ(l, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static dloggerFake s(this dloggerFake l, @string x) => runtime_package.sᴍ(l, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static dloggerFake hexdump(this dloggerFake l, @unsafe.Pointer p, uintptr bytes) => runtime_package.hexdumpᴍ(l, p, bytes);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static dloggerFake pc(this dloggerFake l, uintptr x) => runtime_package.pcᴍ(l, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static dloggerFake traceback(this dloggerFake l, slice<uintptr> x) => runtime_package.tracebackᴍ(l, x);
}
partial class runtimeᴛdloggerImpl {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void end(this ж<dloggerImpl> Ꮡl) => runtime_package.endᴍ(Ꮡl);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<dloggerImpl> b(this ж<dloggerImpl> Ꮡl, bool x) => runtime_package.bᴍ(Ꮡl, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<dloggerImpl> i(this ж<dloggerImpl> Ꮡl, nint x) => runtime_package.iᴍ(Ꮡl, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<dloggerImpl> i8(this ж<dloggerImpl> Ꮡl, int8 x) => runtime_package.i8ᴍ(Ꮡl, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<dloggerImpl> i16(this ж<dloggerImpl> Ꮡl, int16 x) => runtime_package.i16ᴍ(Ꮡl, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<dloggerImpl> i32(this ж<dloggerImpl> Ꮡl, int32 x) => runtime_package.i32ᴍ(Ꮡl, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<dloggerImpl> i64(this ж<dloggerImpl> Ꮡl, int64 x) => runtime_package.i64ᴍ(Ꮡl, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<dloggerImpl> u(this ж<dloggerImpl> Ꮡl, nuint x) => runtime_package.uᴍ(Ꮡl, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<dloggerImpl> uptr(this ж<dloggerImpl> Ꮡl, uintptr x) => runtime_package.uptrᴍ(Ꮡl, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<dloggerImpl> u8(this ж<dloggerImpl> Ꮡl, uint8 x) => runtime_package.u8ᴍ(Ꮡl, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<dloggerImpl> u16(this ж<dloggerImpl> Ꮡl, uint16 x) => runtime_package.u16ᴍ(Ꮡl, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<dloggerImpl> u32(this ж<dloggerImpl> Ꮡl, uint32 x) => runtime_package.u32ᴍ(Ꮡl, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<dloggerImpl> u64(this ж<dloggerImpl> Ꮡl, uint64 x) => runtime_package.u64ᴍ(Ꮡl, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<dloggerImpl> hex(this ж<dloggerImpl> Ꮡl, uint64 x) => runtime_package.hexᴍ(Ꮡl, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<dloggerImpl> p(this ж<dloggerImpl> Ꮡl, any xʗp) => runtime_package.pᴍ(Ꮡl, xʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<dloggerImpl> hexdump(this ж<dloggerImpl> Ꮡl, @unsafe.Pointer p, uintptr bytes) => runtime_package.hexdumpᴍ(Ꮡl, p, bytes);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<dloggerImpl> pc(this ж<dloggerImpl> Ꮡl, uintptr x) => runtime_package.pcᴍ(Ꮡl, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<dloggerImpl> traceback(this ж<dloggerImpl> Ꮡl, slice<uintptr> x) => runtime_package.tracebackᴍ(Ꮡl, x);
}
partial class runtimeᴛdebugLogWriter {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void ensure(this ref debugLogWriter l, uint64 n) => runtime_package.ensureᴍ(ref l, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool writeFrameAt(this ref debugLogWriter l, uint64 pos, uint64 size) => runtime_package.writeFrameAtᴍ(ref l, pos, size);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void writeSync(this ref debugLogWriter l, uint64 tick, uint64 nano) => runtime_package.writeSyncᴍ(ref l, tick, nano);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void writeUint64LE(this ref debugLogWriter l, uint64 x) => runtime_package.writeUint64LEᴍ(ref l, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void @byte(this ref debugLogWriter l, byte x) => runtime_package.@byteᴍ(ref l, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void bytes(this ref debugLogWriter l, slice<byte> x) => runtime_package.bytesᴍ(ref l, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void varint(this ref debugLogWriter l, int64 x) => runtime_package.varintᴍ(ref l, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void uvarint(this ref debugLogWriter l, uint64 u) => runtime_package.uvarintᴍ(ref l, u);
}
partial class runtimeᴛdebugLogReader {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 skip(this ref debugLogReader r) => runtime_package.skipᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint16 readUint16LEAt(this ref debugLogReader r, uint64 pos) => runtime_package.readUint16LEAtᴍ(ref r, pos);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 readUint64LEAt(this ref debugLogReader r, uint64 pos) => runtime_package.readUint64LEAtᴍ(ref r, pos);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 /*tick*/ peek(this ref debugLogReader r) => runtime_package.peekᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (uint64 end, uint64 tick, uint64 nano, nint p) header(this ref debugLogReader r) => runtime_package.headerᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 uvarint(this ref debugLogReader r) => runtime_package.uvarintᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static int64 varint(this ref debugLogReader r) => runtime_package.varintᴍ(ref r);
}
partial class runtimeᴛtimespec {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void setNsec(this ref timespec ts, int64 ns) => runtime_package.setNsecᴍ(ref ts, ns);
}
partial class runtimeᴛtimeval {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void set_usec(this ref timeval tv, int32 x) => runtime_package.set_usecᴍ(ref tv, x);
}
partial class runtimeᴛTypeAssertionError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void RuntimeError(this ref TypeAssertionError _) => runtime_package.RuntimeErrorᴍ(ref _);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Error(this ref TypeAssertionError e) => runtime_package.Errorᴍ(ref e);
}
partial class runtimeᴛerrorString {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void RuntimeError(this errorString e) => runtime_package.RuntimeErrorᴍ(e);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string Error(this errorString e) => runtime_package.Errorᴍ(e);
}
partial class runtimeᴛerrorAddressString {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void RuntimeError(this errorAddressString e) => runtime_package.RuntimeErrorᴍ(e);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string Error(this errorAddressString e) => runtime_package.Errorᴍ(e);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uintptr Addr(this errorAddressString e) => runtime_package.Addrᴍ(e);
}
partial class runtimeᴛplainError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void RuntimeError(this plainError e) => runtime_package.RuntimeErrorᴍ(e);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string Error(this plainError e) => runtime_package.Errorᴍ(e);
}
partial class runtimeᴛboundsError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void RuntimeError(this boundsError e) => runtime_package.RuntimeErrorᴍ(e);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string Error(this boundsError e) => runtime_package.Errorᴍ(e);
}
partial class runtimeᴛhexdumper {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void write(this ref hexdumper h, slice<byte> data) => runtime_package.writeᴍ(ref h, data);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void close(this ref hexdumper h) => runtime_package.closeᴍ(ref h);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void init(this ref hexdumper h) => runtime_package.initᴍ(ref h);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void flushLine(this ref hexdumper h, slice<byte> data) => runtime_package.flushLineᴍ(ref h, data);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void fmtHex(this ref hexdumper h, slice<byte> buf, uint64 v) => runtime_package.fmtHexᴍ(ref h, buf, v);
}
partial class runtimeᴛhexdumpMarker {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void start(this hexdumpMarker m) => runtime_package.startᴍ(m);
}
partial class runtimeᴛtimeHistogram {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void record(this ж<timeHistogram> Ꮡh, int64 duration) => runtime_package.recordᴍ(Ꮡh, duration);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void write(this ж<timeHistogram> Ꮡh, ж<metricValue> Ꮡout) => runtime_package.writeᴍ(Ꮡh, Ꮡout);
}
partial class runtimeᴛitabTableType {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<itab> find(this ж<itabTableType> Ꮡt, ж<interfacetype> inter, ж<_type> typ) => runtime_package.findᴍ(Ꮡt, inter, typ);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void add(this ж<itabTableType> Ꮡt, ж<itab> Ꮡm) => runtime_package.addᴍ(Ꮡt, Ꮡm);
}
partial class runtimeᴛlfstack {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void push(this ж<lfstack> Ꮡhead, ж<lfnode> Ꮡnode) => runtime_package.pushᴍ(Ꮡhead, Ꮡnode);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @unsafe.Pointer pop(this ж<lfstack> Ꮡhead) => runtime_package.popᴍ(Ꮡhead);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool empty(this ж<lfstack> Ꮡhead) => runtime_package.emptyᴍ(Ꮡhead);
}
partial class runtimeᴛlistHead {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void init(this ref listHead head, uintptr off) => runtime_package.initᴍ(ref head, off);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<listNode> getNode(this ref listHead head, @unsafe.Pointer p) => runtime_package.getNodeᴍ(ref head, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool empty(this ref listHead head) => runtime_package.emptyᴍ(ref head);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @unsafe.Pointer head(this ref listHead head) => runtime_package.headᴍ(ref head);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void push(this ref listHead head, @unsafe.Pointer p) => runtime_package.pushᴍ(ref head, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @unsafe.Pointer pop(this ref listHead head) => runtime_package.popᴍ(ref head);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void remove(this ref listHead head, @unsafe.Pointer p) => runtime_package.removeᴍ(ref head, p);
}
partial class runtimeᴛlistHeadManual {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void init(this ref listHeadManual head, uintptr off) => runtime_package.initᴍ(ref head, off);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<listNodeManual> getNode(this ref listHeadManual head, @unsafe.Pointer p) => runtime_package.getNodeᴍ(ref head, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool empty(this ref listHeadManual head) => runtime_package.emptyᴍ(ref head);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @unsafe.Pointer head(this ref listHeadManual head) => runtime_package.headᴍ(ref head);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void push(this ref listHeadManual head, @unsafe.Pointer p) => runtime_package.pushᴍ(ref head, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @unsafe.Pointer pop(this ref listHeadManual head) => runtime_package.popᴍ(ref head);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void remove(this ref listHeadManual head, @unsafe.Pointer p) => runtime_package.removeᴍ(ref head, p);
}
partial class runtimeᴛlockRank {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string String(this lockRank rank) => runtime_package.Stringᴍ(rank);
}
partial class runtimeᴛmheap {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (@unsafe.Pointer v, uintptr size) sysAlloc(this ж<mheap> Ꮡh, uintptr n, ж<ж<arenaHint>> ᏑhintList, ж<slice<arenaIdx>> ᏑarenaList) => runtime_package.sysAllocᴍ(Ꮡh, n, ᏑhintList, ᏑarenaList);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void enableMetadataHugePages(this ж<mheap> Ꮡh) => runtime_package.enableMetadataHugePagesᴍ(Ꮡh);
}
partial class runtimeᴛmcache {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (gclinkptr v, ж<mspan> s, bool checkGCTrigger) nextFree(this ref mcache c, spanClass spc) => runtime_package.nextFreeᴍ(ref c, spc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (gclinkptr, ж<mspan>) nextReusableNoScan(this ref mcache c, ж<mspan> Ꮡs, spanClass spc) => runtime_package.nextReusableNoScanᴍ(ref c, Ꮡs, spc);
}
partial class runtimeᴛlinearAlloc {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void init(this ref linearAlloc l, uintptr @base, uintptr size, bool mapMemory) => runtime_package.initᴍ(ref l, @base, size, mapMemory);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @unsafe.Pointer alloc(this ref linearAlloc l, uintptr size, uintptr align, ж<sysMemStat> sysStat, @string vmaName) => runtime_package.allocᴍ(ref l, size, align, sysStat, vmaName);
}
partial class runtimeᴛnotInHeap {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<notInHeap> add(this ж<notInHeap> Ꮡp, uintptr bytes) => runtime_package.addᴍ(Ꮡp, bytes);
}
partial class runtimeᴛmspan {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static typePointers typePointersOf(this ж<mspan> Ꮡspan, uintptr addr, uintptr size) => runtime_package.typePointersOfᴍ(Ꮡspan, addr, size);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static typePointers typePointersOfUnchecked(this ж<mspan> Ꮡspan, uintptr addr) => runtime_package.typePointersOfUncheckedᴍ(Ꮡspan, addr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static typePointers typePointersOfType(this ref mspan span, ж<abi.Type> typ, uintptr addr) => runtime_package.typePointersOfTypeᴍ(ref span, typ, addr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uintptr objBase(this ref mspan span, uintptr addr) => runtime_package.objBaseᴍ(ref span, addr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void initHeapBits(this ref mspan s) => runtime_package.initHeapBitsᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<uintptr> heapBits(this ref mspan span) => runtime_package.heapBitsᴍ(ref span);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uintptr heapBitsSmallForAddr(this ref mspan span, uintptr addr) => runtime_package.heapBitsSmallForAddrᴍ(ref span, addr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uintptr /*scanSize*/ writeHeapBitsSmall(this ref mspan span, uintptr x, uintptr dataSize, ж<_type> Ꮡtyp) => runtime_package.writeHeapBitsSmallᴍ(ref span, x, dataSize, Ꮡtyp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static markBits allocBitsForIndex(this ref mspan s, uintptr allocBitIndex) => runtime_package.allocBitsForIndexᴍ(ref s, allocBitIndex);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void refillAllocCache(this ref mspan s, uint16 whichByte) => runtime_package.refillAllocCacheᴍ(ref s, whichByte);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint16 nextFreeIndex(this ref mspan s) => runtime_package.nextFreeIndexᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool isFree(this ref mspan s, uintptr index) => runtime_package.isFreeᴍ(ref s, index);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool isFreeOrNewlyAllocated(this ref mspan s, uintptr index) => runtime_package.isFreeOrNewlyAllocatedᴍ(ref s, index);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uintptr divideByElemSize(this ref mspan s, uintptr n) => runtime_package.divideByElemSizeᴍ(ref s, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uintptr objIndex(this ref mspan s, uintptr p) => runtime_package.objIndexᴍ(ref s, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint countAlloc(this ref mspan s) => runtime_package.countAllocᴍ(ref s);
}
partial class runtimeᴛtypePointers {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (typePointers, uintptr) nextFast(this typePointers tp) => runtime_package.nextFastᴍ(tp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (typePointers, uintptr) next(this typePointers tp, uintptr limit) => runtime_package.nextᴍ(tp, limit);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static typePointers fastForward(this typePointers tp, uintptr n, uintptr limit) => runtime_package.fastForwardᴍ(tp, n, limit);
}
partial class runtimeᴛmarkBits {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool isMarked(this markBits m) => runtime_package.isMarkedᴍ(m);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void setMarked(this markBits m) => runtime_package.setMarkedᴍ(m);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void setMarkedNonAtomic(this markBits m) => runtime_package.setMarkedNonAtomicᴍ(m);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void clearMarked(this markBits m) => runtime_package.clearMarkedᴍ(m);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void advance(this ref markBits m) => runtime_package.advanceᴍ(ref m);
}
partial class runtimeᴛgclinkptr {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<gclink> ptr(this gclinkptr p) => runtime_package.ptrᴍ(p);
}
partial class runtimeᴛmcache {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void refill(this ref mcache c, spanClass spc) => runtime_package.refillᴍ(ref c, spc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<mspan> allocLarge(this ref mcache c, uintptr size, bool noscan) => runtime_package.allocLargeᴍ(ref c, size, noscan);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void releaseAll(this ref mcache c) => runtime_package.releaseAllᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void prepareForSweep(this ж<mcache> Ꮡc) => runtime_package.prepareForSweepᴍ(Ꮡc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void addReusableNoscan(this ref mcache c, spanClass spc, uintptr ptr) => runtime_package.addReusableNoscanᴍ(ref c, spc, ptr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool hasReusableNoscan(this ref mcache c, spanClass spc) => runtime_package.hasReusableNoscanᴍ(ref c, spc);
}
partial class runtimeᴛmcentral {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void init(this ref mcentral c, spanClass spc) => runtime_package.initᴍ(ref c, spc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<spanSet> partialUnswept(this ref mcentral c, uint32 sweepgen) => runtime_package.partialUnsweptᴍ(ref c, sweepgen);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<spanSet> partialSwept(this ref mcentral c, uint32 sweepgen) => runtime_package.partialSweptᴍ(ref c, sweepgen);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<spanSet> fullUnswept(this ref mcentral c, uint32 sweepgen) => runtime_package.fullUnsweptᴍ(ref c, sweepgen);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<spanSet> fullSwept(this ref mcentral c, uint32 sweepgen) => runtime_package.fullSweptᴍ(ref c, sweepgen);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<mspan> cacheSpan(this ref mcentral c) => runtime_package.cacheSpanᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void uncacheSpan(this ref mcentral c, ж<mspan> Ꮡs) => runtime_package.uncacheSpanᴍ(ref c, Ꮡs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<mspan> grow(this ref mcentral c) => runtime_package.growᴍ(ref c);
}
partial class runtimeᴛmetricReader {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void compute(this metricReader f, ж<statAggregate> _, ж<metricValue> Ꮡout) => runtime_package.computeᴍ(f, _, Ꮡout);
}
partial class runtimeᴛstatDepSet {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static statDepSet difference(this statDepSet s, statDepSet b) => runtime_package.differenceᴍ(s, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static statDepSet union(this statDepSet s, statDepSet b) => runtime_package.unionᴍ(s, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool empty(this ref statDepSet s) => runtime_package.emptyᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool has(this ref statDepSet s, statDep d) => runtime_package.hasᴍ(ref s, d);
}
partial class runtimeᴛheapStatsAggregate {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void compute(this ж<heapStatsAggregate> Ꮡa) => runtime_package.computeᴍ(Ꮡa);
}
partial class runtimeᴛsysStatsAggregate {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void compute(this ж<sysStatsAggregate> Ꮡa) => runtime_package.computeᴍ(Ꮡa);
}
partial class runtimeᴛcpuStatsAggregate {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void compute(this ref cpuStatsAggregate a) => runtime_package.computeᴍ(ref a);
}
partial class runtimeᴛgcStatsAggregate {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void compute(this ref gcStatsAggregate a) => runtime_package.computeᴍ(ref a);
}
partial class runtimeᴛfinalStatsAggregate {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void compute(this ref finalStatsAggregate a) => runtime_package.computeᴍ(ref a);
}
partial class runtimeᴛstatAggregate {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void ensure(this ж<statAggregate> Ꮡa, ж<statDepSet> deps) => runtime_package.ensureᴍ(Ꮡa, deps);
}
partial class runtimeᴛmetricValue {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<metricFloat64Histogram> float64HistOrInit(this ref metricValue v, slice<float64> buckets) => runtime_package.float64HistOrInitᴍ(ref v, buckets);
}
partial class runtimeᴛfixalloc {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void init(this ref fixalloc f, uintptr size, Action<@unsafe.Pointer, @unsafe.Pointer> first, @unsafe.Pointer arg, ж<sysMemStat> Ꮡstat) => runtime_package.initᴍ(ref f, size, first, arg, Ꮡstat);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @unsafe.Pointer alloc(this ref fixalloc f) => runtime_package.allocᴍ(ref f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void free(this ref fixalloc f, @unsafe.Pointer p) => runtime_package.freeᴍ(ref f, p);
}
partial class runtimeᴛgcTrigger {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool test(this gcTrigger t) => runtime_package.testᴍ(t);
}
partial class runtimeᴛg {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool isMaybeRunnable(this ж<g> Ꮡgp) => runtime_package.isMaybeRunnableᴍ(Ꮡgp);
}
partial class runtimeᴛgcCPULimiterState {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool limiting(this ж<gcCPULimiterState> Ꮡl) => runtime_package.limitingᴍ(Ꮡl);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void startGCTransition(this ж<gcCPULimiterState> Ꮡl, bool enableGC, int64 now) => runtime_package.startGCTransitionᴍ(Ꮡl, enableGC, now);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void finishGCTransition(this ж<gcCPULimiterState> Ꮡl, int64 now) => runtime_package.finishGCTransitionᴍ(Ꮡl, now);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool needUpdate(this ж<gcCPULimiterState> Ꮡl, int64 now) => runtime_package.needUpdateᴍ(Ꮡl, now);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void addAssistTime(this ж<gcCPULimiterState> Ꮡl, int64 t) => runtime_package.addAssistTimeᴍ(Ꮡl, t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void addIdleTime(this ж<gcCPULimiterState> Ꮡl, int64 t) => runtime_package.addIdleTimeᴍ(Ꮡl, t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void update(this ж<gcCPULimiterState> Ꮡl, int64 now) => runtime_package.updateᴍ(Ꮡl, now);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void updateLocked(this ж<gcCPULimiterState> Ꮡl, int64 now) => runtime_package.updateLockedᴍ(Ꮡl, now);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void accumulate(this ж<gcCPULimiterState> Ꮡl, int64 mutatorTime, int64 gcTime) => runtime_package.accumulateᴍ(Ꮡl, mutatorTime, gcTime);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool tryLock(this ж<gcCPULimiterState> Ꮡl) => runtime_package.tryLockᴍ(Ꮡl);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void unlock(this ж<gcCPULimiterState> Ꮡl) => runtime_package.unlockᴍ(Ꮡl);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void resetCapacity(this ж<gcCPULimiterState> Ꮡl, int64 now, int32 nprocs) => runtime_package.resetCapacityᴍ(Ꮡl, now, nprocs);
}
partial class runtimeᴛlimiterEventStamp {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static int64 duration(this limiterEventStamp s, int64 now) => runtime_package.durationᴍ(s, now);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static limiterEventType typ(this limiterEventStamp s) => runtime_package.typᴍ(s);
}
partial class runtimeᴛlimiterEvent {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool start(this ж<limiterEvent> Ꮡe, limiterEventType typ, int64 now) => runtime_package.startᴍ(Ꮡe, typ, now);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (limiterEventType typ, int64 duration) consume(this ж<limiterEvent> Ꮡe, int64 now) => runtime_package.consumeᴍ(Ꮡe, now);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void stop(this ж<limiterEvent> Ꮡe, limiterEventType typ, int64 now) => runtime_package.stopᴍ(Ꮡe, typ, now);
}
partial class runtimeᴛg {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool internalBlocked(this ref g gp) => runtime_package.internalBlockedᴍ(ref gp);
}
partial class runtimeᴛspanScanOwnership {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static spanScanOwnership load(this ж<spanScanOwnership> Ꮡo) => runtime_package.loadᴍ(Ꮡo);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static spanScanOwnership or(this ж<spanScanOwnership> Ꮡo, spanScanOwnership v) => runtime_package.orᴍ(Ꮡo, v);
}
partial class runtimeᴛspanInlineMarkBits {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void init(this ж<spanInlineMarkBits> Ꮡimb, spanClass @class, bool needzero) => runtime_package.initᴍ(Ꮡimb, @class, needzero);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool tryAcquire(this ж<spanInlineMarkBits> Ꮡimb) => runtime_package.tryAcquireᴍ(Ꮡimb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static spanScanOwnership release(this ж<spanInlineMarkBits> Ꮡimb) => runtime_package.releaseᴍ(Ꮡimb);
}
partial class runtimeᴛmspan {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void initInlineMarkBits(this ref mspan s) => runtime_package.initInlineMarkBitsᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void moveInlineMarks(this ref mspan s, ж<gcBits> dst) => runtime_package.moveInlineMarksᴍ(ref s, dst);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<spanInlineMarkBits> inlineMarkBits(this ref mspan s) => runtime_package.inlineMarkBitsᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static markBits /*bits*/ markBitsForIndex(this ref mspan s, uintptr objIndex) => runtime_package.markBitsForIndexᴍ(ref s, objIndex);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static markBits markBitsForBase(this ref mspan s) => runtime_package.markBitsForBaseᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static markBits scannedBitsForIndex(this ref mspan s, uintptr objIndex) => runtime_package.scannedBitsForIndexᴍ(ref s, objIndex);
}
partial class runtimeᴛgcWork {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static objptr tryGetSpanFast(this ref gcWork w) => runtime_package.tryGetSpanFastᴍ(ref w);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static objptr tryGetSpan(this ж<gcWork> Ꮡw) => runtime_package.tryGetSpanᴍ(Ꮡw);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static objptr tryStealSpan(this ref gcWork w) => runtime_package.tryStealSpanᴍ(ref w);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void flushScanStats(this ref gcWork w, /*[68]*/ ж<array<sizeClassScanStats>> Ꮡdst) => runtime_package.flushScanStatsᴍ(ref w, Ꮡdst);
}
partial class runtimeᴛspanQueue {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool /*ok*/ putFast(this ref spanQueue q, objptr s) => runtime_package.putFastᴍ(ref q, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool put(this ж<spanQueue> Ꮡq, objptr s) => runtime_package.putᴍ(Ꮡq, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void flush(this ж<spanQueue> Ꮡq) => runtime_package.flushᴍ(Ꮡq);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool empty(this ж<spanQueue> Ꮡq) => runtime_package.emptyᴍ(Ꮡq);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool chainEmpty(this ж<spanQueue> Ꮡq) => runtime_package.chainEmptyᴍ(Ꮡq);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void drain(this ж<spanQueue> Ꮡq, uint32 n) => runtime_package.drainᴍ(Ꮡq, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool tryDrain(this ref spanQueue q, ж<spanSPMC> Ꮡr, uint32 n) => runtime_package.tryDrainᴍ(ref q, Ꮡr, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static objptr tryGetFast(this ref spanQueue q) => runtime_package.tryGetFastᴍ(ref q);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static objptr steal(this ref spanQueue q, ж<spanQueue> q2) => runtime_package.stealᴍ(ref q, q2);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static objptr refill(this ref spanQueue q, ж<spanSPMC> Ꮡr) => runtime_package.refillᴍ(ref q, Ꮡr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void destroy(this ж<spanQueue> Ꮡq) => runtime_package.destroyᴍ(Ꮡq);
}
partial class runtimeᴛspanSPMC {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool empty(this ж<spanSPMC> Ꮡr) => runtime_package.emptyᴍ(Ꮡr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void deinit(this ж<spanSPMC> Ꮡr) => runtime_package.deinitᴍ(Ꮡr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<objptr> slot(this ref spanSPMC r, uint32 i) => runtime_package.slotᴍ(ref r, i);
}
partial class runtimeᴛobjptr {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uintptr spanBase(this objptr p) => runtime_package.spanBaseᴍ(p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint16 objIndex(this objptr p) => runtime_package.objIndexᴍ(p);
}
partial class runtimeᴛgcControllerState {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void init(this ж<gcControllerState> Ꮡc, int32 gcPercent, int64 memoryLimit) => runtime_package.initᴍ(Ꮡc, gcPercent, memoryLimit);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void startCycle(this ж<gcControllerState> Ꮡc, int64 markStartTime, nint procs, gcTrigger trigger) => runtime_package.startCycleᴍ(Ꮡc, markStartTime, procs, trigger);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void revise(this ж<gcControllerState> Ꮡc) => runtime_package.reviseᴍ(Ꮡc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void endCycle(this ж<gcControllerState> Ꮡc, int64 now, nint procs, bool userForced) => runtime_package.endCycleᴍ(Ꮡc, now, procs, userForced);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void enlistWorker(this ж<gcControllerState> Ꮡc) => runtime_package.enlistWorkerᴍ(Ꮡc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (bool, int64) assignWaitingGCWorker(this ж<gcControllerState> Ꮡc, ж<p> Ꮡpp, int64 now) => runtime_package.assignWaitingGCWorkerᴍ(Ꮡc, Ꮡpp, now);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<g>, int64) findRunnableGCWorker(this ж<gcControllerState> Ꮡc, ж<p> Ꮡpp, int64 now) => runtime_package.findRunnableGCWorkerᴍ(Ꮡc, Ꮡpp, now);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void releaseNextGCMarkWorker(this ж<gcControllerState> Ꮡc, ж<p> Ꮡpp) => runtime_package.releaseNextGCMarkWorkerᴍ(Ꮡc, Ꮡpp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void resetLive(this ж<gcControllerState> Ꮡc, uint64 bytesMarked) => runtime_package.resetLiveᴍ(Ꮡc, bytesMarked);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void markWorkerStop(this ж<gcControllerState> Ꮡc, gcMarkWorkerMode mode, int64 duration) => runtime_package.markWorkerStopᴍ(Ꮡc, mode, duration);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void update(this ж<gcControllerState> Ꮡc, int64 dHeapLive, int64 dHeapScan) => runtime_package.updateᴍ(Ꮡc, dHeapLive, dHeapScan);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void addScannableStack(this ж<gcControllerState> Ꮡc, ж<p> Ꮡpp, int64 amount) => runtime_package.addScannableStackᴍ(Ꮡc, Ꮡpp, amount);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void addGlobals(this ж<gcControllerState> Ꮡc, int64 amount) => runtime_package.addGlobalsᴍ(Ꮡc, amount);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 heapGoal(this ж<gcControllerState> Ꮡc) => runtime_package.heapGoalᴍ(Ꮡc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (uint64 goal, uint64 minTrigger) heapGoalInternal(this ж<gcControllerState> Ꮡc) => runtime_package.heapGoalInternalᴍ(Ꮡc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 memoryLimitHeapGoal(this ж<gcControllerState> Ꮡc) => runtime_package.memoryLimitHeapGoalᴍ(Ꮡc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (uint64, uint64) trigger(this ж<gcControllerState> Ꮡc) => runtime_package.triggerᴍ(Ꮡc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void commit(this ж<gcControllerState> Ꮡc, bool isSweepDone) => runtime_package.commitᴍ(Ꮡc, isSweepDone);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static int32 setGCPercent(this ж<gcControllerState> Ꮡc, int32 @in) => runtime_package.setGCPercentᴍ(Ꮡc, @in);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static int64 setMemoryLimit(this ж<gcControllerState> Ꮡc, int64 @in) => runtime_package.setMemoryLimitᴍ(Ꮡc, @in);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool addIdleMarkWorker(this ж<gcControllerState> Ꮡc) => runtime_package.addIdleMarkWorkerᴍ(Ꮡc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool needIdleMarkWorker(this ж<gcControllerState> Ꮡc) => runtime_package.needIdleMarkWorkerᴍ(Ꮡc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void removeIdleMarkWorker(this ж<gcControllerState> Ꮡc) => runtime_package.removeIdleMarkWorkerᴍ(Ꮡc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void setMaxIdleMarkWorkers(this ж<gcControllerState> Ꮡc, int32 max) => runtime_package.setMaxIdleMarkWorkersᴍ(Ꮡc, max);
}
partial class runtimeᴛscavengerState {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void init(this ж<scavengerState> Ꮡs) => runtime_package.initᴍ(Ꮡs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void park(this ж<scavengerState> Ꮡs) => runtime_package.parkᴍ(Ꮡs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void ready(this ж<scavengerState> Ꮡs) => runtime_package.readyᴍ(Ꮡs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void wake(this ж<scavengerState> Ꮡs) => runtime_package.wakeᴍ(Ꮡs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void sleep(this ж<scavengerState> Ꮡs, float64 worked) => runtime_package.sleepᴍ(Ꮡs, worked);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void controllerFailed(this ж<scavengerState> Ꮡs) => runtime_package.controllerFailedᴍ(Ꮡs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (uintptr released, float64 worked) run(this ж<scavengerState> Ꮡs) => runtime_package.runᴍ(Ꮡs);
}
partial class runtimeᴛpageAlloc {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uintptr scavenge(this ж<pageAlloc> Ꮡp, uintptr nbytes, Func<bool> shouldStop, bool force) => runtime_package.scavengeᴍ(Ꮡp, nbytes, shouldStop, force);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uintptr scavengeOne(this ref pageAlloc p, chunkIdx ci, nuint searchIdx, uintptr max) => runtime_package.scavengeOneᴍ(ref p, ci, searchIdx, max);
}
partial class runtimeᴛpallocData {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nuint, nuint) findScavengeCandidate(this ref pallocData m, nuint searchIdx, uintptr minimum, uintptr max) => runtime_package.findScavengeCandidateᴍ(ref m, searchIdx, minimum, max);
}
partial class runtimeᴛscavengeIndex {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uintptr init(this ж<scavengeIndex> Ꮡs, bool test, ж<sysMemStat> sysStat) => runtime_package.initᴍ(Ꮡs, test, sysStat);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uintptr grow(this ж<scavengeIndex> Ꮡs, uintptr @base, uintptr limit, ж<sysMemStat> sysStat) => runtime_package.growᴍ(Ꮡs, @base, limit, sysStat);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (chunkIdx, nuint) find(this ж<scavengeIndex> Ꮡs, bool force) => runtime_package.findᴍ(Ꮡs, force);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void alloc(this ref scavengeIndex s, chunkIdx ci, nuint npages) => runtime_package.allocᴍ(ref s, ci, npages);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void free(this ж<scavengeIndex> Ꮡs, chunkIdx ci, nuint page, nuint npages) => runtime_package.freeᴍ(Ꮡs, ci, page, npages);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void nextGen(this ж<scavengeIndex> Ꮡs) => runtime_package.nextGenᴍ(Ꮡs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void setEmpty(this ref scavengeIndex s, chunkIdx ci) => runtime_package.setEmptyᴍ(ref s, ci);
}
partial class runtimeᴛatomicScavChunkData {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static scavChunkData load(this ж<atomicScavChunkData> Ꮡsc) => runtime_package.loadᴍ(Ꮡsc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void store(this ж<atomicScavChunkData> Ꮡsc, scavChunkData ssc) => runtime_package.storeᴍ(Ꮡsc, ssc);
}
partial class runtimeᴛscavChunkData {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 pack(this scavChunkData sc) => runtime_package.packᴍ(sc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool shouldScavenge(this scavChunkData sc, uint32 currGen, bool force) => runtime_package.shouldScavengeᴍ(sc, currGen, force);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void alloc(this ref scavChunkData sc, nuint npages, uint32 newGen) => runtime_package.allocᴍ(ref sc, npages, newGen);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void free(this ref scavChunkData sc, nuint npages, uint32 newGen) => runtime_package.freeᴍ(ref sc, npages, newGen);
}
partial class runtimeᴛscavChunkFlags {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool isEmpty(this ref scavChunkFlags sc) => runtime_package.isEmptyᴍ(ref sc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void setEmpty(this ref scavChunkFlags sc) => runtime_package.setEmptyᴍ(ref sc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void setNonEmpty(this ref scavChunkFlags sc) => runtime_package.setNonEmptyᴍ(ref sc);
}
partial class runtimeᴛpiController {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (float64, bool) next(this ref piController c, float64 input, float64 setpoint, float64 period) => runtime_package.nextᴍ(ref c, input, setpoint, period);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void reset(this ref piController c) => runtime_package.resetᴍ(ref c);
}
partial class runtimeᴛstackObject {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void setRecord(this ж<stackObject> Ꮡobj, ж<stackObjectRecord> r) => runtime_package.setRecordᴍ(Ꮡobj, r);
}
partial class runtimeᴛstackScanState {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void putPtr(this ж<stackScanState> Ꮡs, uintptr p, bool conservative) => runtime_package.putPtrᴍ(Ꮡs, p, conservative);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (uintptr p, bool conservative) getPtr(this ж<stackScanState> Ꮡs) => runtime_package.getPtrᴍ(Ꮡs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void addObject(this ref stackScanState s, uintptr addr, ж<stackObjectRecord> Ꮡr) => runtime_package.addObjectᴍ(ref s, addr, Ꮡr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void buildIndex(this ref stackScanState s) => runtime_package.buildIndexᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<stackObject> findObject(this ref stackScanState s, uintptr a) => runtime_package.findObjectᴍ(ref s, a);
}
partial class runtimeᴛsweepClass {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static sweepClass load(this ж<sweepClass> Ꮡs) => runtime_package.loadᴍ(Ꮡs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void update(this ж<sweepClass> Ꮡs, sweepClass sNew) => runtime_package.updateᴍ(Ꮡs, sNew);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void clear(this ж<sweepClass> Ꮡs) => runtime_package.clearᴍ(Ꮡs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (spanClass spc, bool full) split(this sweepClass s) => runtime_package.splitᴍ(s);
}
partial class runtimeᴛmheap {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<mspan> nextSpanForSweep(this ref mheap h) => runtime_package.nextSpanForSweepᴍ(ref h);
}
partial class runtimeᴛactiveSweep {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static sweepLocker begin(this ж<activeSweep> Ꮡa) => runtime_package.beginᴍ(Ꮡa);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void end(this ж<activeSweep> Ꮡa, sweepLocker sl) => runtime_package.endᴍ(Ꮡa, sl);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool markDrained(this ж<activeSweep> Ꮡa) => runtime_package.markDrainedᴍ(Ꮡa);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint32 sweepers(this ж<activeSweep> Ꮡa) => runtime_package.sweepersᴍ(Ꮡa);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool isDone(this ж<activeSweep> Ꮡa) => runtime_package.isDoneᴍ(Ꮡa);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void reset(this ж<activeSweep> Ꮡa) => runtime_package.resetᴍ(Ꮡa);
}
partial class runtimeᴛsweepLocker {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (sweepLocked, bool) tryAcquire(this ref sweepLocker l, ж<mspan> s) => runtime_package.tryAcquireᴍ(ref l, s);
}
partial class runtimeᴛmspan {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void ensureSwept(this ж<mspan> Ꮡs) => runtime_package.ensureSweptᴍ(Ꮡs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void reportZombies(this ж<mspan> Ꮡs) => runtime_package.reportZombiesᴍ(Ꮡs);
}
partial class runtimeᴛsweepLocked {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool sweep(this ref sweepLocked sl, bool preserve) => runtime_package.sweepᴍ(ref sl, preserve);
}
partial class runtimeᴛgcWork {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void init(this ref gcWork w) => runtime_package.initᴍ(ref w);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void putObj(this ref gcWork w, uintptr obj) => runtime_package.putObjᴍ(ref w, obj);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool putObjFast(this ref gcWork w, uintptr obj) => runtime_package.putObjFastᴍ(ref w, obj);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void putObjBatch(this ref gcWork w, slice<uintptr> obj) => runtime_package.putObjBatchᴍ(ref w, obj);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uintptr tryGetObj(this ref gcWork w) => runtime_package.tryGetObjᴍ(ref w);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uintptr tryGetObjFast(this ref gcWork w) => runtime_package.tryGetObjFastᴍ(ref w);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void dispose(this ж<gcWork> Ꮡw) => runtime_package.disposeᴍ(Ꮡw);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void balance(this ref gcWork w) => runtime_package.balanceᴍ(ref w);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool empty(this ж<gcWork> Ꮡw) => runtime_package.emptyᴍ(Ꮡw);
}
partial class runtimeᴛworkbuf {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void checknonempty(this ref workbuf b) => runtime_package.checknonemptyᴍ(ref b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void checkempty(this ref workbuf b) => runtime_package.checkemptyᴍ(ref b);
}
partial class runtimeᴛmSpanStateBox {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void set(this ж<mSpanStateBox> Ꮡb, mSpanState s) => runtime_package.setᴍ(Ꮡb, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static mSpanState get(this ж<mSpanStateBox> Ꮡb) => runtime_package.getᴍ(Ꮡb);
}
partial class runtimeᴛmspan {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uintptr @base(this ref mspan s) => runtime_package.@baseᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (uintptr size, uintptr n, uintptr total) layout(this ref mspan s) => runtime_package.layoutᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void init(this ж<mspan> Ꮡspan, uintptr @base, uintptr npages) => runtime_package.initᴍ(Ꮡspan, @base, npages);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool inList(this ref mspan span) => runtime_package.inListᴍ(ref span);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<ж<special>>, bool) specialFindSplicePoint(this ж<mspan> Ꮡspan, uintptr offset, byte kind) => runtime_package.specialFindSplicePointᴍ(Ꮡspan, offset, kind);
}
partial class runtimeᴛspanClass {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static int8 sizeclass(this spanClass sc) => runtime_package.sizeclassᴍ(sc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool noscan(this spanClass sc) => runtime_package.noscanᴍ(sc);
}
partial class runtimeᴛarenaIdx {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nuint l1(this arenaIdx i) => runtime_package.l1ᴍ(i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nuint l2(this arenaIdx i) => runtime_package.l2ᴍ(i);
}
partial class runtimeᴛmheap {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void init(this ж<mheap> Ꮡh) => runtime_package.initᴍ(Ꮡh);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void reclaim(this ж<mheap> Ꮡh, uintptr npage) => runtime_package.reclaimᴍ(Ꮡh, npage);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uintptr reclaimChunk(this ж<mheap> Ꮡh, slice<arenaIdx> arenas, uintptr pageIdx, uintptr n) => runtime_package.reclaimChunkᴍ(Ꮡh, arenas, pageIdx, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<mspan> alloc(this ж<mheap> Ꮡh, uintptr npages, spanClass spanclass) => runtime_package.allocᴍ(Ꮡh, npages, spanclass);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<mspan> allocManual(this ж<mheap> Ꮡh, uintptr npages, spanAllocType typ) => runtime_package.allocManualᴍ(Ꮡh, npages, typ);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void setSpans(this ref mheap h, uintptr @base, uintptr npage, ж<mspan> s) => runtime_package.setSpansᴍ(ref h, @base, npage, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool /*needZero*/ allocNeedsZero(this ref mheap h, uintptr @base, uintptr npage) => runtime_package.allocNeedsZeroᴍ(ref h, @base, npage);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<mspan> tryAllocMSpan(this ref mheap h) => runtime_package.tryAllocMSpanᴍ(ref h);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<mspan> allocMSpanLocked(this ж<mheap> Ꮡh) => runtime_package.allocMSpanLockedᴍ(Ꮡh);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void freeMSpanLocked(this ж<mheap> Ꮡh, ж<mspan> s) => runtime_package.freeMSpanLockedᴍ(Ꮡh, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<mspan> /*s*/ allocSpan(this ж<mheap> Ꮡh, uintptr npages, spanAllocType typ, spanClass spanclass) => runtime_package.allocSpanᴍ(Ꮡh, npages, typ, spanclass);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void initSpan(this ж<mheap> Ꮡh, ж<mspan> Ꮡs, spanAllocType typ, spanClass spanclass, uintptr @base, uintptr npages, uintptr scav) => runtime_package.initSpanᴍ(Ꮡh, Ꮡs, typ, spanclass, @base, npages, scav);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (uintptr, bool) grow(this ж<mheap> Ꮡh, uintptr npage) => runtime_package.growᴍ(Ꮡh, npage);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void freeSpan(this ж<mheap> Ꮡh, ж<mspan> s) => runtime_package.freeSpanᴍ(Ꮡh, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void freeManual(this ж<mheap> Ꮡh, ж<mspan> Ꮡs, spanAllocType typ) => runtime_package.freeManualᴍ(Ꮡh, Ꮡs, typ);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void freeSpanLocked(this ж<mheap> Ꮡh, ж<mspan> Ꮡs, spanAllocType typ) => runtime_package.freeSpanLockedᴍ(Ꮡh, Ꮡs, typ);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void scavengeAll(this ж<mheap> Ꮡh) => runtime_package.scavengeAllᴍ(Ꮡh);
}
partial class runtimeᴛspanAllocType {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool manual(this spanAllocType s) => runtime_package.manualᴍ(s);
}
partial class runtimeᴛmSpanList {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void init(this ref mSpanList list) => runtime_package.initᴍ(ref list);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void remove(this ж<mSpanList> Ꮡlist, ж<mspan> Ꮡspan) => runtime_package.removeᴍ(Ꮡlist, Ꮡspan);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool isEmpty(this ref mSpanList list) => runtime_package.isEmptyᴍ(ref list);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void insert(this ж<mSpanList> Ꮡlist, ж<mspan> Ꮡspan) => runtime_package.insertᴍ(Ꮡlist, Ꮡspan);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void insertBack(this ж<mSpanList> Ꮡlist, ж<mspan> Ꮡspan) => runtime_package.insertBackᴍ(Ꮡlist, Ꮡspan);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void takeAll(this ж<mSpanList> Ꮡlist, ж<mSpanList> Ꮡother) => runtime_package.takeAllᴍ(Ꮡlist, Ꮡother);
}
partial class runtimeᴛimmortalWeakHandle {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<atomic.Uintptr> handle(this ж<immortalWeakHandle> Ꮡh) => runtime_package.handleᴍ(Ꮡh);
}
partial class runtimeᴛimmortalWeakHandleMap {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<atomic.Uintptr> getOrAdd(this ж<immortalWeakHandleMap> Ꮡtab, uintptr pʗp) => runtime_package.getOrAddᴍ(Ꮡtab, pʗp);
}
partial class runtimeᴛspecialsIter {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool valid(this ref specialsIter i) => runtime_package.validᴍ(ref i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void next(this ref specialsIter i) => runtime_package.nextᴍ(ref i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<special> unlinkAndNext(this ref specialsIter i) => runtime_package.unlinkAndNextᴍ(ref i);
}
partial class runtimeᴛgcBits {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<uint8> bytep(this ж<gcBits> Ꮡb, uintptr n) => runtime_package.bytepᴍ(Ꮡb, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<uint8> bytep, uint8 mask) bitp(this ж<gcBits> Ꮡb, uintptr n) => runtime_package.bitpᴍ(Ꮡb, n);
}
partial class runtimeᴛgcBitsArena {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<gcBits> tryAlloc(this ж<gcBitsArena> Ꮡb, uintptr bytes) => runtime_package.tryAllocᴍ(Ꮡb, bytes);
}
partial class runtimeᴛchunkIdx {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nuint l1(this chunkIdx i) => runtime_package.l1ᴍ(i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nuint l2(this chunkIdx i) => runtime_package.l2ᴍ(i);
}
partial class runtimeᴛpageAlloc {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void init(this ж<pageAlloc> Ꮡp, ж<mutex> ᏑmheapLock, ж<sysMemStat> ᏑsysStat, bool test) => runtime_package.initᴍ(Ꮡp, ᏑmheapLock, ᏑsysStat, test);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<pallocData> tryChunkOf(this ref pageAlloc p, chunkIdx ci) => runtime_package.tryChunkOfᴍ(ref p, ci);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<pallocData> chunkOf(this ref pageAlloc p, chunkIdx ci) => runtime_package.chunkOfᴍ(ref p, ci);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void grow(this ж<pageAlloc> Ꮡp, uintptr @base, uintptr size) => runtime_package.growᴍ(Ꮡp, @base, size);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void enableChunkHugePages(this ref pageAlloc p) => runtime_package.enableChunkHugePagesᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void update(this ref pageAlloc p, uintptr @base, uintptr npages, bool contig, bool alloc) => runtime_package.updateᴍ(ref p, @base, npages, contig, alloc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uintptr allocRange(this ref pageAlloc p, uintptr @base, uintptr npages) => runtime_package.allocRangeᴍ(ref p, @base, npages);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static offAddr findMappedAddr(this ref pageAlloc p, offAddr addr) => runtime_package.findMappedAddrᴍ(ref p, addr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (uintptr, offAddr) find(this ref pageAlloc p, uintptr npages) => runtime_package.findᴍ(ref p, npages);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (uintptr addr, uintptr scav) alloc(this ref pageAlloc p, uintptr npages) => runtime_package.allocᴍ(ref p, npages);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void free(this ж<pageAlloc> Ꮡp, uintptr @base, uintptr npages) => runtime_package.freeᴍ(Ꮡp, @base, npages);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void markRandomPaddingPages(this ref pageAlloc p, uintptr @base, uintptr npages) => runtime_package.markRandomPaddingPagesᴍ(ref p, @base, npages);
}
partial class runtimeᴛpallocSum {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nuint start(this pallocSum p) => runtime_package.startᴍ(p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nuint max(this pallocSum p) => runtime_package.maxᴍ(p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nuint end(this pallocSum p) => runtime_package.endᴍ(p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nuint, nuint, nuint) unpack(this pallocSum p) => runtime_package.unpackᴍ(p);
}
partial class runtimeᴛpageAlloc {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void sysInit(this ref pageAlloc p, bool test) => runtime_package.sysInitᴍ(ref p, test);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void sysGrow(this ж<pageAlloc> Ꮡp, uintptr @base, uintptr limit) => runtime_package.sysGrowᴍ(Ꮡp, @base, limit);
}
partial class runtimeᴛscavengeIndex {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uintptr sysGrow(this ж<scavengeIndex> Ꮡs, uintptr @base, uintptr limit, ж<sysMemStat> sysStat) => runtime_package.sysGrowᴍ(Ꮡs, @base, limit, sysStat);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uintptr sysInit(this ref scavengeIndex s, bool test, ж<sysMemStat> sysStat) => runtime_package.sysInitᴍ(ref s, test, sysStat);
}
partial class runtimeᴛpageCache {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool empty(this ref pageCache c) => runtime_package.emptyᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (uintptr, uintptr) alloc(this ref pageCache c, uintptr npages) => runtime_package.allocᴍ(ref c, npages);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (uintptr, uintptr) allocN(this ref pageCache c, uintptr npages) => runtime_package.allocNᴍ(ref c, npages);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void flush(this ref pageCache c, ж<pageAlloc> Ꮡp) => runtime_package.flushᴍ(ref c, Ꮡp);
}
partial class runtimeᴛpageAlloc {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static pageCache allocToCache(this ref pageAlloc p) => runtime_package.allocToCacheᴍ(ref p);
}
partial class runtimeᴛpageBits {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nuint get(this ref pageBits b, nuint i) => runtime_package.getᴍ(ref b, i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 block64(this ref pageBits b, nuint i) => runtime_package.block64ᴍ(ref b, i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void set(this ref pageBits b, nuint i) => runtime_package.setᴍ(ref b, i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void setRange(this ref pageBits b, nuint i, nuint n) => runtime_package.setRangeᴍ(ref b, i, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void setAll(this ref pageBits b) => runtime_package.setAllᴍ(ref b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void setBlock64(this ref pageBits b, nuint i, uint64 v) => runtime_package.setBlock64ᴍ(ref b, i, v);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void clear(this ref pageBits b, nuint i) => runtime_package.clearᴍ(ref b, i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void clearRange(this ref pageBits b, nuint i, nuint n) => runtime_package.clearRangeᴍ(ref b, i, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void clearAll(this ref pageBits b) => runtime_package.clearAllᴍ(ref b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void clearBlock64(this ref pageBits b, nuint i, uint64 v) => runtime_package.clearBlock64ᴍ(ref b, i, v);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nuint /*s*/ popcntRange(this ref pageBits b, nuint i, nuint n) => runtime_package.popcntRangeᴍ(ref b, i, n);
}
partial class runtimeᴛpallocBits {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static pallocSum summarize(this ж<pallocBits> Ꮡb) => runtime_package.summarizeᴍ(Ꮡb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nuint, nuint) find(this ж<pallocBits> Ꮡb, uintptr npages, nuint searchIdx) => runtime_package.findᴍ(Ꮡb, npages, searchIdx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nuint find1(this ж<pallocBits> Ꮡb, nuint searchIdx) => runtime_package.find1ᴍ(Ꮡb, searchIdx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nuint, nuint) findSmallN(this ж<pallocBits> Ꮡb, uintptr npages, nuint searchIdx) => runtime_package.findSmallNᴍ(Ꮡb, npages, searchIdx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nuint, nuint) findLargeN(this ж<pallocBits> Ꮡb, uintptr npages, nuint searchIdx) => runtime_package.findLargeNᴍ(Ꮡb, npages, searchIdx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void allocRange(this ж<pallocBits> Ꮡb, nuint i, nuint n) => runtime_package.allocRangeᴍ(Ꮡb, i, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void allocAll(this ж<pallocBits> Ꮡb) => runtime_package.allocAllᴍ(Ꮡb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void free1(this ж<pallocBits> Ꮡb, nuint i) => runtime_package.free1ᴍ(Ꮡb, i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void free(this ж<pallocBits> Ꮡb, nuint i, nuint n) => runtime_package.freeᴍ(Ꮡb, i, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void freeAll(this ж<pallocBits> Ꮡb) => runtime_package.freeAllᴍ(Ꮡb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 pages64(this ж<pallocBits> Ꮡb, nuint i) => runtime_package.pages64ᴍ(Ꮡb, i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void allocPages64(this ж<pallocBits> Ꮡb, nuint i, uint64 alloc) => runtime_package.allocPages64ᴍ(Ꮡb, i, alloc);
}
partial class runtimeᴛpallocData {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void allocRange(this ж<pallocData> Ꮡm, nuint i, nuint n) => runtime_package.allocRangeᴍ(Ꮡm, i, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void allocAll(this ж<pallocData> Ꮡm) => runtime_package.allocAllᴍ(Ꮡm);
}
partial class runtimeᴛmemRecordCycle {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void add(this ref memRecordCycle a, ж<memRecordCycle> Ꮡb) => runtime_package.addᴍ(ref a, Ꮡb);
}
partial class runtimeᴛmProfCycleHolder {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint32 /*cycle*/ read(this ж<mProfCycleHolder> Ꮡc) => runtime_package.readᴍ(Ꮡc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (uint32 cycle, bool alreadyFlushed) setFlushed(this ж<mProfCycleHolder> Ꮡc) => runtime_package.setFlushedᴍ(Ꮡc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void increment(this ж<mProfCycleHolder> Ꮡc) => runtime_package.incrementᴍ(Ꮡc);
}
partial class runtimeᴛmLockProfile {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static int64 start(this ref mLockProfile prof) => runtime_package.startᴍ(ref prof);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void end(this ж<mLockProfile> Ꮡprof, int64 start) => runtime_package.endᴍ(Ꮡprof, start);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void recordUnlock(this ж<mLockProfile> Ꮡprof, int64 cycles) => runtime_package.recordUnlockᴍ(Ꮡprof, cycles);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void captureStack(this ж<mLockProfile> Ꮡprof) => runtime_package.captureStackᴍ(Ꮡprof);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void store(this ref mLockProfile prof) => runtime_package.storeᴍ(ref prof);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void storeSlow(this ref mLockProfile prof) => runtime_package.storeSlowᴍ(ref prof);
}
partial class runtimeᴛStackRecord {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<uintptr> Stack(this ref StackRecord r) => runtime_package.Stackᴍ(ref r);
}
partial class runtimeᴛMemProfileRecord {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static int64 InUseBytes(this ref MemProfileRecord r) => runtime_package.InUseBytesᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static int64 InUseObjects(this ref MemProfileRecord r) => runtime_package.InUseObjectsᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<uintptr> Stack(this ref MemProfileRecord r) => runtime_package.Stackᴍ(ref r);
}
partial class runtimeᴛgoroutineProfileStateHolder {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static goroutineProfileState Load(this ж<goroutineProfileStateHolder> Ꮡp) => runtime_package.Loadᴍ(Ꮡp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void Store(this ж<goroutineProfileStateHolder> Ꮡp, goroutineProfileState value) => runtime_package.Storeᴍ(Ꮡp, value);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool CompareAndSwap(this ж<goroutineProfileStateHolder> Ꮡp, goroutineProfileState old, goroutineProfileState @new) => runtime_package.CompareAndSwapᴍ(Ꮡp, old, @new);
}
partial class runtimeᴛaddrRange {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uintptr size(this addrRange a) => runtime_package.sizeᴍ(a);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool contains(this addrRange a, uintptr addr) => runtime_package.containsᴍ(a, addr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static addrRange subtract(this addrRange a, addrRange b) => runtime_package.subtractᴍ(a, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (uintptr, bool) takeFromFront(this ref addrRange a, uintptr len, uint8 align) => runtime_package.takeFromFrontᴍ(ref a, len, align);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (uintptr, bool) takeFromBack(this ref addrRange a, uintptr len, uint8 align) => runtime_package.takeFromBackᴍ(ref a, len, align);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static addrRange removeGreaterEqual(this addrRange a, uintptr addr) => runtime_package.removeGreaterEqualᴍ(a, addr);
}
partial class runtimeᴛoffAddr {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static offAddr add(this offAddr l, uintptr bytes) => runtime_package.addᴍ(l, bytes);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static offAddr sub(this offAddr l, uintptr bytes) => runtime_package.subᴍ(l, bytes);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uintptr diff(this offAddr l1, offAddr l2) => runtime_package.diffᴍ(l1, l2);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool lessThan(this offAddr l1, offAddr l2) => runtime_package.lessThanᴍ(l1, l2);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool lessEqual(this offAddr l1, offAddr l2) => runtime_package.lessEqualᴍ(l1, l2);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool equal(this offAddr l1, offAddr l2) => runtime_package.equalᴍ(l1, l2);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uintptr addr(this offAddr l) => runtime_package.addrᴍ(l);
}
partial class runtimeᴛatomicOffAddr {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void Clear(this ж<atomicOffAddr> Ꮡb) => runtime_package.Clearᴍ(Ꮡb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void StoreMin(this ж<atomicOffAddr> Ꮡb, uintptr addr) => runtime_package.StoreMinᴍ(Ꮡb, addr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void StoreUnmark(this ж<atomicOffAddr> Ꮡb, uintptr markedAddr, uintptr newAddr) => runtime_package.StoreUnmarkᴍ(Ꮡb, markedAddr, newAddr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void StoreMarked(this ж<atomicOffAddr> Ꮡb, uintptr addr) => runtime_package.StoreMarkedᴍ(Ꮡb, addr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (uintptr, bool) Load(this ж<atomicOffAddr> Ꮡb) => runtime_package.Loadᴍ(Ꮡb);
}
partial class runtimeᴛaddrRanges {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint findSucc(this ref addrRanges a, uintptr addr) => runtime_package.findSuccᴍ(ref a, addr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (uintptr, bool) findAddrGreaterEqual(this ref addrRanges a, uintptr addr) => runtime_package.findAddrGreaterEqualᴍ(ref a, addr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool contains(this ref addrRanges a, uintptr addr) => runtime_package.containsᴍ(ref a, addr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static addrRange removeLast(this ref addrRanges a, uintptr nBytes) => runtime_package.removeLastᴍ(ref a, nBytes);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void removeGreaterEqual(this ref addrRanges a, uintptr addr) => runtime_package.removeGreaterEqualᴍ(ref a, addr);
}
partial class runtimeᴛspanSet {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void push(this ж<spanSet> Ꮡb, ж<mspan> s) => runtime_package.pushᴍ(Ꮡb, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<mspan> pop(this ж<spanSet> Ꮡb) => runtime_package.popᴍ(Ꮡb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void reset(this ж<spanSet> Ꮡb) => runtime_package.resetᴍ(Ꮡb);
}
partial class runtimeᴛatomicSpanSetSpinePointer {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static spanSetSpinePointer Load(this ж<atomicSpanSetSpinePointer> Ꮡs) => runtime_package.Loadᴍ(Ꮡs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void StoreNoWB(this ж<atomicSpanSetSpinePointer> Ꮡs, spanSetSpinePointer p) => runtime_package.StoreNoWBᴍ(Ꮡs, p);
}
partial class runtimeᴛspanSetSpinePointer {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<atomic.Pointer<spanSetBlock>> lookup(this spanSetSpinePointer s, uintptr idx) => runtime_package.lookupᴍ(s, idx);
}
partial class runtimeᴛspanSetBlockAlloc {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<spanSetBlock> alloc(this ж<spanSetBlockAlloc> Ꮡp) => runtime_package.allocᴍ(Ꮡp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void free(this ж<spanSetBlockAlloc> Ꮡp, ж<spanSetBlock> block) => runtime_package.freeᴍ(Ꮡp, block);
}
partial class runtimeᴛheadTailIndex {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint32 head(this headTailIndex h) => runtime_package.headᴍ(h);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint32 tail(this headTailIndex h) => runtime_package.tailᴍ(h);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (uint32 head, uint32 tail) split(this headTailIndex h) => runtime_package.splitᴍ(h);
}
partial class runtimeᴛatomicHeadTailIndex {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static headTailIndex load(this ж<atomicHeadTailIndex> Ꮡh) => runtime_package.loadᴍ(Ꮡh);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool cas(this ж<atomicHeadTailIndex> Ꮡh, headTailIndex old, headTailIndex @new) => runtime_package.casᴍ(Ꮡh, old, @new);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static headTailIndex incHead(this ж<atomicHeadTailIndex> Ꮡh) => runtime_package.incHeadᴍ(Ꮡh);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static headTailIndex decHead(this ж<atomicHeadTailIndex> Ꮡh) => runtime_package.decHeadᴍ(Ꮡh);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static headTailIndex incTail(this ж<atomicHeadTailIndex> Ꮡh) => runtime_package.incTailᴍ(Ꮡh);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void reset(this ж<atomicHeadTailIndex> Ꮡh) => runtime_package.resetᴍ(Ꮡh);
}
partial class runtimeᴛatomicMSpanPointer {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<mspan> Load(this ж<atomicMSpanPointer> Ꮡp) => runtime_package.Loadᴍ(Ꮡp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void StoreNoWB(this ж<atomicMSpanPointer> Ꮡp, ж<mspan> s) => runtime_package.StoreNoWBᴍ(Ꮡp, s);
}
partial class runtimeᴛsysMemStat {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 load(this ж<sysMemStat> Ꮡs) => runtime_package.loadᴍ(Ꮡs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void add(this ж<sysMemStat> Ꮡs, int64 n) => runtime_package.addᴍ(Ꮡs, n);
}
partial class runtimeᴛheapStatsDelta {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void merge(this ref heapStatsDelta a, ж<heapStatsDelta> Ꮡb) => runtime_package.mergeᴍ(ref a, Ꮡb);
}
partial class runtimeᴛconsistentHeapStats {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<heapStatsDelta> acquire(this ж<consistentHeapStats> Ꮡm) => runtime_package.acquireᴍ(Ꮡm);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void release(this ж<consistentHeapStats> Ꮡm) => runtime_package.releaseᴍ(Ꮡm);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void unsafeRead(this ref consistentHeapStats m, ж<heapStatsDelta> Ꮡout) => runtime_package.unsafeReadᴍ(ref m, Ꮡout);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void unsafeClear(this ref consistentHeapStats m) => runtime_package.unsafeClearᴍ(ref m);
}
partial class runtimeᴛcpuStats {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void accumulateGCPauseTime(this ref cpuStats s, int64 dt, int32 maxProcs) => runtime_package.accumulateGCPauseTimeᴍ(ref s, dt, maxProcs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void accumulate(this ref cpuStats s, int64 now, bool gcMarkPhase) => runtime_package.accumulateᴍ(ref s, now, gcMarkPhase);
}
partial class runtimeᴛwbBuf {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void reset(this ref wbBuf b) => runtime_package.resetᴍ(ref b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void discard(this ref wbBuf b) => runtime_package.discardᴍ(ref b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool empty(this ref wbBuf b) => runtime_package.emptyᴍ(ref b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<array<uintptr>> get1(this ref wbBuf b) => runtime_package.get1ᴍ(ref b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<array<uintptr>> get2(this ref wbBuf b) => runtime_package.get2ᴍ(ref b);
}
partial class runtimeᴛpollInfo {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool closing(this pollInfo i) => runtime_package.closingᴍ(i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool eventErr(this pollInfo i) => runtime_package.eventErrᴍ(i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool expiredReadDeadline(this pollInfo i) => runtime_package.expiredReadDeadlineᴍ(i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool expiredWriteDeadline(this pollInfo i) => runtime_package.expiredWriteDeadlineᴍ(i);
}
partial class runtimeᴛpollDesc {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static pollInfo info(this ж<pollDesc> Ꮡpd) => runtime_package.infoᴍ(Ꮡpd);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void publishInfo(this ж<pollDesc> Ꮡpd) => runtime_package.publishInfoᴍ(Ꮡpd);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void setEventErr(this ж<pollDesc> Ꮡpd, bool b, uintptr seq) => runtime_package.setEventErrᴍ(Ꮡpd, b, seq);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static any /*i*/ makeArg(this ж<pollDesc> Ꮡpd) => runtime_package.makeArgᴍ(Ꮡpd);
}
partial class runtimeᴛpollCache {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void free(this ж<pollCache> Ꮡc, ж<pollDesc> Ꮡpd) => runtime_package.freeᴍ(Ꮡc, Ꮡpd);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<pollDesc> alloc(this ж<pollCache> Ꮡc) => runtime_package.allocᴍ(Ꮡc);
}
partial class runtimeᴛsigctxt {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void fixsigcode(this ref sigctxt c, uint32 sig) => runtime_package.fixsigcodeᴍ(ref c, sig);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool sigFromUser(this ref sigctxt c) => runtime_package.sigFromUserᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool sigFromSeccomp(this ref sigctxt c) => runtime_package.sigFromSeccompᴍ(ref c);
}
partial class runtimeᴛkernelVersion {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool GE(this kernelVersion kv, nint x, nint y) => runtime_package.GEᴍ(kv, x, y);
}
partial class runtimeᴛPanicNilError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Error(this ref PanicNilError _) => runtime_package.Errorᴍ(ref _);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void RuntimeError(this ref PanicNilError _) => runtime_package.RuntimeErrorᴍ(ref _);
}
partial class runtimeᴛ_panic {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void start(this ж<_panic> Ꮡp, uintptr pc, @unsafe.Pointer sp) => runtime_package.startᴍ(Ꮡp, pc, sp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (Action, bool) nextDefer(this ж<_panic> Ꮡp) => runtime_package.nextDeferᴍ(Ꮡp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool /*ok*/ nextFrame(this ж<_panic> Ꮡp) => runtime_package.nextFrameᴍ(Ꮡp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool initOpenCodedDefers(this ref _panic p, funcInfo fn, @unsafe.Pointer varp) => runtime_package.initOpenCodedDefersᴍ(ref p, fn, varp);
}
partial class runtimeᴛpinner {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void unpin(this ж<pinner> Ꮡp) => runtime_package.unpinᴍ(Ꮡp);
}
partial class runtimeᴛpinState {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool isPinned(this ref pinState v) => runtime_package.isPinnedᴍ(ref v);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool isMultiPinned(this ref pinState v) => runtime_package.isMultiPinnedᴍ(ref v);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void setPinned(this ref pinState v, bool val) => runtime_package.setPinnedᴍ(ref v, val);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void setMultiPinned(this ref pinState v, bool val) => runtime_package.setMultiPinnedᴍ(ref v, val);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void set(this ref pinState v, bool val, bool multipin) => runtime_package.setᴍ(ref v, val, multipin);
}
partial class runtimeᴛpinnerBits {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static pinState ofObject(this ж<pinnerBits> Ꮡp, uintptr n) => runtime_package.ofObjectᴍ(Ꮡp, n);
}
partial class runtimeᴛmspan {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uintptr pinnerBitSize(this ref mspan s) => runtime_package.pinnerBitSizeᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<pinnerBits> newPinnerBits(this ref mspan s) => runtime_package.newPinnerBitsᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<pinnerBits> getPinnerBits(this ж<mspan> Ꮡs) => runtime_package.getPinnerBitsᴍ(Ꮡs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void setPinnerBits(this ж<mspan> Ꮡs, ж<pinnerBits> p) => runtime_package.setPinnerBitsᴍ(Ꮡs, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void refreshPinnerBits(this ж<mspan> Ꮡs) => runtime_package.refreshPinnerBitsᴍ(Ꮡs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void incPinCounter(this ж<mspan> Ꮡspan, uintptr offset) => runtime_package.incPinCounterᴍ(Ꮡspan, offset);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool decPinCounter(this ж<mspan> Ꮡspan, uintptr offset) => runtime_package.decPinCounterᴍ(Ꮡspan, offset);
}
partial class runtimeᴛxRegPerP {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void free(this ref xRegPerP xRegs) => runtime_package.freeᴍ(ref xRegs);
}
partial class runtimeᴛm {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void becomeSpinning(this ref m mp) => runtime_package.becomeSpinningᴍ(ref mp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<ж<p>> snapshotAllp(this ref m mp) => runtime_package.snapshotAllpᴍ(ref mp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void clearAllpSnapshot(this ref m mp) => runtime_package.clearAllpSnapshotᴍ(ref mp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool hasCgoOnStack(this ref m mp) => runtime_package.hasCgoOnStackᴍ(ref mp);
}
partial class runtimeᴛstwReason {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string String(this stwReason r) => runtime_package.Stringᴍ(r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool isGC(this stwReason r) => runtime_package.isGCᴍ(r);
}
partial class runtimeᴛp {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void init(this ж<p> Ꮡpp, int32 id) => runtime_package.initᴍ(Ꮡpp, id);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void destroy(this ж<p> Ꮡpp) => runtime_package.destroyᴍ(Ꮡpp);
}
partial class runtimeᴛsyscallingThread {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void gcstopP(this syscallingThread s) => runtime_package.gcstopPᴍ(s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void takeP(this syscallingThread s) => runtime_package.takePᴍ(s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void releaseP(this syscallingThread s, uint32 state) => runtime_package.releasePᴍ(s, state);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void resume(this syscallingThread s) => runtime_package.resumeᴍ(s);
}
partial class runtimeᴛpMask {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool read(this pMask p, uint32 id) => runtime_package.readᴍ(p, id);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void set(this pMask p, int32 id) => runtime_package.setᴍ(p, id);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void clear(this pMask p, int32 id) => runtime_package.clearᴍ(p, id);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool any(this pMask p) => runtime_package.anyᴍ(p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static pMask resize(this pMask p, int32 nprocs) => runtime_package.resizeᴍ(p, nprocs);
}
partial class runtimeᴛgQueue {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool empty(this ref gQueue q) => runtime_package.emptyᴍ(ref q);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void push(this ref gQueue q, ж<g> Ꮡgp) => runtime_package.pushᴍ(ref q, Ꮡgp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void pushBack(this ref gQueue q, ж<g> Ꮡgp) => runtime_package.pushBackᴍ(ref q, Ꮡgp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void pushBackAll(this ref gQueue q, gQueue q2) => runtime_package.pushBackAllᴍ(ref q, q2);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<g> pop(this ref gQueue q) => runtime_package.popᴍ(ref q);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static gList popList(this ref gQueue q) => runtime_package.popListᴍ(ref q);
}
partial class runtimeᴛgList {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool empty(this ref gList l) => runtime_package.emptyᴍ(ref l);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void push(this ref gList l, ж<g> Ꮡgp) => runtime_package.pushᴍ(ref l, Ꮡgp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void pushAll(this ref gList l, gQueue q) => runtime_package.pushAllᴍ(ref l, q);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<g> pop(this ref gList l) => runtime_package.popᴍ(ref l);
}
partial class runtimeᴛrandomOrder {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void reset(this ref randomOrder ord, uint32 count) => runtime_package.resetᴍ(ref ord, count);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static randomEnum start(this ref randomOrder ord, uint32 i) => runtime_package.startᴍ(ref ord, i);
}
partial class runtimeᴛrandomEnum {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool done(this ref randomEnum @enum) => runtime_package.doneᴍ(ref @enum);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void next(this ref randomEnum @enum) => runtime_package.nextᴍ(ref @enum);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint32 position(this ref randomEnum @enum) => runtime_package.positionᴍ(ref @enum);
}
partial class runtimeᴛprofAtomic {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static profIndex load(this ж<profAtomic> Ꮡx) => runtime_package.loadᴍ(Ꮡx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void store(this ж<profAtomic> Ꮡx, profIndex @new) => runtime_package.storeᴍ(Ꮡx, @new);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool cas(this ж<profAtomic> Ꮡx, profIndex old, profIndex @new) => runtime_package.casᴍ(Ꮡx, old, @new);
}
partial class runtimeᴛprofIndex {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint32 dataCount(this profIndex x) => runtime_package.dataCountᴍ(x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint32 tagCount(this profIndex x) => runtime_package.tagCountᴍ(x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static profIndex addCountsAndClearFlags(this profIndex x, nint data, nint tag) => runtime_package.addCountsAndClearFlagsᴍ(x, data, tag);
}
partial class runtimeᴛprofBuf {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool hasOverflow(this ж<profBuf> Ꮡb) => runtime_package.hasOverflowᴍ(Ꮡb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (uint32 count, uint64 time) takeOverflow(this ж<profBuf> Ꮡb) => runtime_package.takeOverflowᴍ(Ꮡb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void incrementOverflow(this ж<profBuf> Ꮡb, int64 now) => runtime_package.incrementOverflowᴍ(Ꮡb, now);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool canWriteRecord(this ж<profBuf> Ꮡb, nint nstk) => runtime_package.canWriteRecordᴍ(Ꮡb, nstk);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool canWriteTwoRecords(this ж<profBuf> Ꮡb, nint nstk1, nint nstk2) => runtime_package.canWriteTwoRecordsᴍ(Ꮡb, nstk1, nstk2);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void close(this ж<profBuf> Ꮡb) => runtime_package.closeᴍ(Ꮡb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void wakeupExtra(this ж<profBuf> Ꮡb) => runtime_package.wakeupExtraᴍ(Ꮡb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (slice<uint64> data, slice<@unsafe.Pointer> tags, bool eof) read(this ж<profBuf> Ꮡb, profBufReadMode mode) => runtime_package.readᴍ(Ꮡb, mode);
}
partial class runtimeᴛticksType {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void init(this ref ticksType t) => runtime_package.initᴍ(ref t);
}
partial class runtimeᴛgodebugInc {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void IncNonDefault(this ж<godebugInc> Ꮡg) => runtime_package.IncNonDefaultᴍ(Ꮡg);
}
partial class runtimeᴛrwmutex {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void init(this ж<rwmutex> Ꮡrw, lockRank readRank, lockRank readRankInternal, lockRank writeRank) => runtime_package.initᴍ(Ꮡrw, readRank, readRankInternal, writeRank);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void rlock(this ж<rwmutex> Ꮡrw) => runtime_package.rlockᴍ(Ꮡrw);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void runlock(this ж<rwmutex> Ꮡrw) => runtime_package.runlockᴍ(Ꮡrw);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void @lock(this ж<rwmutex> Ꮡrw) => runtime_package.@lockᴍ(Ꮡrw);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void unlock(this ж<rwmutex> Ꮡrw) => runtime_package.unlockᴍ(Ꮡrw);
}
partial class runtimeᴛhchan {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uintptr sortkey(this ж<hchan> Ꮡc) => runtime_package.sortkeyᴍ(Ꮡc);
}
partial class runtimeᴛwaitq {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void dequeueSudoG(this ref waitq q, ж<sudog> Ꮡsgp) => runtime_package.dequeueSudoGᴍ(ref q, Ꮡsgp);
}
partial class runtimeᴛsemTable {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<semaRoot> rootFor(this ref semTable t, ж<uint32> addr) => runtime_package.rootForᴍ(ref t, addr);
}
partial class runtimeᴛsemaRoot {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void queue(this ж<semaRoot> Ꮡroot, ж<uint32> addr, ж<sudog> Ꮡs, bool lifo) => runtime_package.queueᴍ(Ꮡroot, addr, Ꮡs, lifo);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<sudog> found, int64 now, int64 tailtime) dequeue(this ж<semaRoot> Ꮡroot, ж<uint32> addr) => runtime_package.dequeueᴍ(Ꮡroot, addr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void rotateLeft(this ref semaRoot root, ж<sudog> Ꮡx) => runtime_package.rotateLeftᴍ(ref root, Ꮡx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void rotateRight(this ref semaRoot root, ж<sudog> Ꮡy) => runtime_package.rotateRightᴍ(ref root, Ꮡy);
}
partial class runtimeᴛsigctxt {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uintptr sigpc(this ref sigctxt c) => runtime_package.sigpcᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void setsigpc(this ref sigctxt c, uint64 x) => runtime_package.setsigpcᴍ(ref c, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uintptr sigsp(this ref sigctxt c) => runtime_package.sigspᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uintptr siglr(this ref sigctxt c) => runtime_package.siglrᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uintptr fault(this ref sigctxt c) => runtime_package.faultᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void preparePanic(this ref sigctxt c, uint32 sig, ж<g> Ꮡgp) => runtime_package.preparePanicᴍ(ref c, sig, Ꮡgp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void pushCall(this ref sigctxt c, uintptr targetPC, uintptr resumePC) => runtime_package.pushCallᴍ(ref c, targetPC, resumePC);
}
partial class runtimeᴛsigctxt {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<sigcontext> regs(this ref sigctxt c) => runtime_package.regsᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 rax(this ref sigctxt c) => runtime_package.raxᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 rbx(this ref sigctxt c) => runtime_package.rbxᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 rcx(this ref sigctxt c) => runtime_package.rcxᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 rdx(this ref sigctxt c) => runtime_package.rdxᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 rdi(this ref sigctxt c) => runtime_package.rdiᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 rsi(this ref sigctxt c) => runtime_package.rsiᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 rbp(this ref sigctxt c) => runtime_package.rbpᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 rsp(this ref sigctxt c) => runtime_package.rspᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 r8(this ref sigctxt c) => runtime_package.r8ᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 r9(this ref sigctxt c) => runtime_package.r9ᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 r10(this ref sigctxt c) => runtime_package.r10ᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 r11(this ref sigctxt c) => runtime_package.r11ᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 r12(this ref sigctxt c) => runtime_package.r12ᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 r13(this ref sigctxt c) => runtime_package.r13ᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 r14(this ref sigctxt c) => runtime_package.r14ᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 r15(this ref sigctxt c) => runtime_package.r15ᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 rip(this ref sigctxt c) => runtime_package.ripᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 rflags(this ref sigctxt c) => runtime_package.rflagsᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 cs(this ref sigctxt c) => runtime_package.csᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 fs(this ref sigctxt c) => runtime_package.fsᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 gs(this ref sigctxt c) => runtime_package.gsᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 sigcode(this ref sigctxt c) => runtime_package.sigcodeᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 sigaddr(this ref sigctxt c) => runtime_package.sigaddrᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void set_rip(this ref sigctxt c, uint64 x) => runtime_package.set_ripᴍ(ref c, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void set_rsp(this ref sigctxt c, uint64 x) => runtime_package.set_rspᴍ(ref c, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void set_sigcode(this ref sigctxt c, uint64 x) => runtime_package.set_sigcodeᴍ(ref c, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void set_sigaddr(this ref sigctxt c, uint64 x) => runtime_package.set_sigaddrᴍ(ref c, x);
}
partial class runtimeᴛbitvector {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint8 ptrbit(this ref bitvector bv, uintptr i) => runtime_package.ptrbitᴍ(ref bv, i);
}
partial class runtimeᴛstackObjectRecord {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (uintptr, ж<byte>) gcdata(this ж<stackObjectRecord> Ꮡr) => runtime_package.gcdataᴍ(Ꮡr);
}
partial class runtimeᴛstkframe {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uintptr argBytes(this ref stkframe frame) => runtime_package.argBytesᴍ(ref frame);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (bitvector argMap, bool hasReflectStackObj) argMapInternal(this ref stkframe frame) => runtime_package.argMapInternalᴍ(ref frame);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (bitvector locals, bitvector args, slice<stackObjectRecord> objs) getStackMap(this ref stkframe frame, bool debug) => runtime_package.getStackMapᴍ(ref frame, debug);
}
partial class runtimeᴛFunc {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<_func> raw(this ж<Func> Ꮡf) => runtime_package.rawᴍ(Ꮡf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static funcInfo funcInfo(this ж<Func> Ꮡf) => runtime_package.funcInfoᴍ(Ꮡf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static int32 startLine(this ж<Func> Ꮡf) => runtime_package.startLineᴍ(Ꮡf);
}
partial class runtimeᴛ_func {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static funcInfo funcInfo(this ж<_func> Ꮡf) => runtime_package.funcInfoᴍ(Ꮡf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool isInlined(this ref _func f) => runtime_package.isInlinedᴍ(ref f);
}
partial class runtimeᴛmoduledata {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uintptr textAddr(this ref moduledata md, uint32 off32) => runtime_package.textAddrᴍ(ref md, off32);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (uint32, bool) textOff(this ref moduledata md, uintptr pc) => runtime_package.textOffᴍ(ref md, pc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string funcName(this ref moduledata md, int32 nameOff) => runtime_package.funcNameᴍ(ref md, nameOff);
}
partial class runtimeᴛfuncInfo {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool valid(this funcInfo f) => runtime_package.validᴍ(f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Func> _Func(this funcInfo f) => runtime_package._Funcᴍ(f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uintptr entry(this funcInfo f) => runtime_package.entryᴍ(f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static srcFunc srcFunc(this funcInfo f) => runtime_package.srcFuncᴍ(f);
}
partial class runtimeᴛsrcFunc {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string name(this srcFunc s) => runtime_package.nameᴍ(s);
}
partial class runtimeᴛinlineUnwinder {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static inlineFrame resolveInternal(this ref inlineUnwinder u, uintptr pc) => runtime_package.resolveInternalᴍ(ref u, pc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static inlineFrame next(this ref inlineUnwinder u, inlineFrame uf) => runtime_package.nextᴍ(ref u, uf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool isInlined(this ref inlineUnwinder u, inlineFrame uf) => runtime_package.isInlinedᴍ(ref u, uf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static srcFunc srcFunc(this ref inlineUnwinder u, inlineFrame uf) => runtime_package.srcFuncᴍ(ref u, uf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (@string @file, nint line) fileLine(this ref inlineUnwinder u, inlineFrame uf) => runtime_package.fileLineᴍ(ref u, uf);
}
partial class runtimeᴛinlineFrame {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool valid(this inlineFrame uf) => runtime_package.validᴍ(uf);
}
partial class runtimeᴛsynctestBubble {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void changegstatus(this ж<synctestBubble> Ꮡbubble, ж<g> Ꮡgp, uint32 oldval, uint32 newval) => runtime_package.changegstatusᴍ(Ꮡbubble, Ꮡgp, oldval, newval);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void incActive(this ж<synctestBubble> Ꮡbubble) => runtime_package.incActiveᴍ(Ꮡbubble);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void decActive(this ж<synctestBubble> Ꮡbubble) => runtime_package.decActiveᴍ(Ꮡbubble);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<g> maybeWakeLocked(this ж<synctestBubble> Ꮡbubble) => runtime_package.maybeWakeLockedᴍ(Ꮡbubble);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @unsafe.Pointer raceaddr(this ж<synctestBubble> Ꮡbubble) => runtime_package.raceaddrᴍ(Ꮡbubble);
}
partial class runtimeᴛsynctestDeadlockError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string Error(this synctestDeadlockError e) => runtime_package.Errorᴍ(e);
}
partial class runtimeᴛtaggedPointer {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @unsafe.Pointer pointer(this taggedPointer tp) => runtime_package.pointerᴍ(tp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uintptr tag(this taggedPointer tp) => runtime_package.tagᴍ(tp);
}
partial class runtimeᴛtimer {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void init(this ж<timer> Ꮡt, Action<any, uintptr, int64> f, any arg) => runtime_package.initᴍ(Ꮡt, f, arg);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void trace(this ж<timer> Ꮡt, @string op) => runtime_package.traceᴍ(Ꮡt, op);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void trace1(this ж<timer> Ꮡt, @string op) => runtime_package.trace1ᴍ(Ꮡt, op);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void @lock(this ж<timer> Ꮡt) => runtime_package.@lockᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void unlock(this ж<timer> Ꮡt) => runtime_package.unlockᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<hchan> hchan(this ж<timer> Ꮡt) => runtime_package.hchanᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool /*updated*/ updateHeap(this ж<timer> Ꮡt) => runtime_package.updateHeapᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void maybeRunAsync(this ж<timer> Ꮡt) => runtime_package.maybeRunAsyncᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool stop(this ж<timer> Ꮡt) => runtime_package.stopᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool modify(this ж<timer> Ꮡt, int64 when, int64 period, Action<any, uintptr, int64> f, any arg, uintptr seq) => runtime_package.modifyᴍ(Ꮡt, when, period, f, arg, seq);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool needsAdd(this ж<timer> Ꮡt) => runtime_package.needsAddᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void maybeAdd(this ж<timer> Ꮡt) => runtime_package.maybeAddᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool reset(this ж<timer> Ꮡt, int64 when, int64 period) => runtime_package.resetᴍ(Ꮡt, when, period);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void unlockAndRun(this ж<timer> Ꮡt, int64 now, ж<synctestBubble> Ꮡbubble) => runtime_package.unlockAndRunᴍ(Ꮡt, now, Ꮡbubble);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void maybeRunChan(this ж<timer> Ꮡt, ж<hchan> Ꮡc) => runtime_package.maybeRunChanᴍ(Ꮡt, Ꮡc);
}
partial class runtimeᴛtimerWhen {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool less(this timerWhen tw, timerWhen other) => runtime_package.lessᴍ(tw, other);
}
partial class runtimeᴛtimers {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void @lock(this ж<timers> Ꮡts) => runtime_package.@lockᴍ(Ꮡts);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void unlock(this ж<timers> Ꮡts) => runtime_package.unlockᴍ(Ꮡts);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void trace(this ж<timers> Ꮡts, @string op) => runtime_package.traceᴍ(Ꮡts, op);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void addHeap(this ж<timers> Ꮡts, ж<timer> Ꮡt) => runtime_package.addHeapᴍ(Ꮡts, Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void deleteMin(this ж<timers> Ꮡts) => runtime_package.deleteMinᴍ(Ꮡts);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void cleanHead(this ж<timers> Ꮡts) => runtime_package.cleanHeadᴍ(Ꮡts);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void take(this ж<timers> Ꮡts, ж<timers> Ꮡsrc) => runtime_package.takeᴍ(Ꮡts, Ꮡsrc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void adjust(this ж<timers> Ꮡts, int64 now, bool force) => runtime_package.adjustᴍ(Ꮡts, now, force);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static int64 wakeTime(this ж<timers> Ꮡts) => runtime_package.wakeTimeᴍ(Ꮡts);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (int64 rnow, int64 pollUntil, bool ran) check(this ж<timers> Ꮡts, int64 now, ж<synctestBubble> bubble) => runtime_package.checkᴍ(Ꮡts, now, bubble);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static int64 run(this ж<timers> Ꮡts, int64 now, ж<synctestBubble> bubble) => runtime_package.runᴍ(Ꮡts, now, bubble);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void verify(this ж<timers> Ꮡts) => runtime_package.verifyᴍ(Ꮡts);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void updateMinWhenHeap(this ж<timers> Ꮡts) => runtime_package.updateMinWhenHeapᴍ(Ꮡts);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void updateMinWhenModified(this ж<timers> Ꮡts, int64 when) => runtime_package.updateMinWhenModifiedᴍ(Ꮡts, when);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void siftUp(this ref timers ts, nint i) => runtime_package.siftUpᴍ(ref ts, i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void siftDown(this ref timers ts, nint i) => runtime_package.siftDownᴍ(ref ts, i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void initHeap(this ref timers ts) => runtime_package.initHeapᴍ(ref ts);
}
partial class runtimeᴛtraceAdvancerState {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void start(this ж<traceAdvancerState> Ꮡs) => runtime_package.startᴍ(Ꮡs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void stop(this ref traceAdvancerState s) => runtime_package.stopᴍ(ref s);
}
partial class runtimeᴛwakeableSleep {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void sleep(this ж<wakeableSleep> Ꮡs, int64 ns) => runtime_package.sleepᴍ(Ꮡs, ns);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void wake(this ж<wakeableSleep> Ꮡs) => runtime_package.wakeᴍ(Ꮡs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void close(this ж<wakeableSleep> Ꮡs) => runtime_package.closeᴍ(Ꮡs);
}
partial class runtimeᴛtraceLocker {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void SpanExists(this traceLocker tl, ж<mspan> Ꮡs) => runtime_package.SpanExistsᴍ(tl, Ꮡs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void SpanAlloc(this traceLocker tl, ж<mspan> Ꮡs) => runtime_package.SpanAllocᴍ(tl, Ꮡs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void SpanFree(this traceLocker tl, ж<mspan> s) => runtime_package.SpanFreeᴍ(tl, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void HeapObjectExists(this traceLocker tl, uintptr addr, ж<abi.Type> typ) => runtime_package.HeapObjectExistsᴍ(tl, addr, typ);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void HeapObjectAlloc(this traceLocker tl, uintptr addr, ж<abi.Type> typ) => runtime_package.HeapObjectAllocᴍ(tl, addr, typ);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void HeapObjectFree(this traceLocker tl, uintptr addr) => runtime_package.HeapObjectFreeᴍ(tl, addr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void GoroutineStackExists(this traceLocker tl, uintptr @base, uintptr size) => runtime_package.GoroutineStackExistsᴍ(tl, @base, size);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void GoroutineStackAlloc(this traceLocker tl, uintptr @base, uintptr size) => runtime_package.GoroutineStackAllocᴍ(tl, @base, size);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void GoroutineStackFree(this traceLocker tl, uintptr @base) => runtime_package.GoroutineStackFreeᴍ(tl, @base);
}
partial class runtimeᴛunwinder {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void init(this ж<unwinder> Ꮡu, ж<g> gp, unwindFlags flags) => runtime_package.initᴍ(Ꮡu, gp, flags);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void initAt(this ж<unwinder> Ꮡu, uintptr pc0, uintptr sp0, uintptr lr0, ж<g> Ꮡgp, unwindFlags flags) => runtime_package.initAtᴍ(Ꮡu, pc0, sp0, lr0, Ꮡgp, flags);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool valid(this ref unwinder u) => runtime_package.validᴍ(ref u);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void resolveInternal(this ж<unwinder> Ꮡu, bool innermost, bool isSyscall) => runtime_package.resolveInternalᴍ(Ꮡu, innermost, isSyscall);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void next(this ж<unwinder> Ꮡu) => runtime_package.nextᴍ(Ꮡu);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void finishInternal(this ref unwinder u) => runtime_package.finishInternalᴍ(ref u);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uintptr symPC(this ref unwinder u) => runtime_package.symPCᴍ(ref u);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint cgoCallers(this ref unwinder u, slice<uintptr> pcBuf) => runtime_package.cgoCallersᴍ(ref u, pcBuf);
}
partial class runtimeᴛtraceLocker {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static traceWriter writer(this traceLocker tl) => runtime_package.writerᴍ(tl);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static traceWriter expWriter(this traceLocker tl, tracev2.Experiment exp) => runtime_package.expWriterᴍ(tl, exp);
}
partial class runtimeᴛtraceWriter {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static traceWriter @event(this traceWriter w, tracev2.EventType ev, params ꓸꓸꓸtraceArg argsʗp) => runtime_package.@eventᴍ(w, ev, argsʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void end(this traceWriter w) => runtime_package.endᴍ(w);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (traceWriter, bool) ensure(this traceWriter w, nint maxSize) => runtime_package.ensureᴍ(w, maxSize);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static traceWriter flush(this traceWriter w) => runtime_package.flushᴍ(w);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static traceWriter refill(this traceWriter w) => runtime_package.refillᴍ(w);
}
partial class runtimeᴛtraceBufQueue {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void push(this ref traceBufQueue q, ж<traceBuf> Ꮡbuf) => runtime_package.pushᴍ(ref q, Ꮡbuf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<traceBuf> pop(this ref traceBufQueue q) => runtime_package.popᴍ(ref q);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool empty(this ref traceBufQueue q) => runtime_package.emptyᴍ(ref q);
}
partial class runtimeᴛtraceBuf {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void @byte(this ref traceBuf buf, byte v) => runtime_package.@byteᴍ(ref buf, v);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void varint(this ref traceBuf buf, uint64 v) => runtime_package.varintᴍ(ref buf, v);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint varintReserve(this ref traceBuf buf) => runtime_package.varintReserveᴍ(ref buf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void stringData(this ref traceBuf buf, @string s) => runtime_package.stringDataᴍ(ref buf, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool available(this ref traceBuf buf, nint size) => runtime_package.availableᴍ(ref buf, size);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void varintAt(this ref traceBuf buf, nint pos, uint64 v) => runtime_package.varintAtᴍ(ref buf, pos, v);
}
partial class runtimeᴛtraceLocker {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static traceEventWriter eventWriter(this traceLocker tl, tracev2.GoStatus goStatus, tracev2.ProcStatus procStatus) => runtime_package.eventWriterᴍ(tl, goStatus, procStatus);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static traceEventWriter rawEventWriter(this traceLocker tl) => runtime_package.rawEventWriterᴍ(tl);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static traceArg stack(this traceLocker tl, nint skip) => runtime_package.stackᴍ(tl, skip);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static traceArg startPC(this traceLocker tl, uintptr pc) => runtime_package.startPCᴍ(tl, pc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static traceArg @string(this traceLocker tl, @string s) => runtime_package.@stringᴍ(tl, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static traceArg uniqueString(this traceLocker tl, @string s) => runtime_package.uniqueStringᴍ(tl, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static traceArg rtype(this traceLocker tl, ж<abi.Type> typ) => runtime_package.rtypeᴍ(tl, typ);
}
partial class runtimeᴛtraceEventWriter {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void @event(this traceEventWriter e, tracev2.EventType ev, params ꓸꓸꓸtraceArg argsʗp) => runtime_package.@eventᴍ(e, ev, argsʗp);
}
partial class runtimeᴛtraceMap {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 stealID(this ж<traceMap> Ꮡtab) => runtime_package.stealIDᴍ(Ꮡtab);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (uint64, bool) put(this ж<traceMap> Ꮡtab, @unsafe.Pointer data, uintptr size) => runtime_package.putᴍ(Ꮡtab, data, size);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void reset(this ж<traceMap> Ꮡtab) => runtime_package.resetᴍ(Ꮡtab);
}
partial class runtimeᴛtraceRegionAlloc {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<notInHeap> alloc(this ж<traceRegionAlloc> Ꮡa, uintptr n) => runtime_package.allocᴍ(Ꮡa, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void drop(this ж<traceRegionAlloc> Ꮡa) => runtime_package.dropᴍ(Ꮡa);
}
partial class runtimeᴛgTraceState {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void reset(this ref gTraceState s) => runtime_package.resetᴍ(ref s);
}
partial class runtimeᴛtraceLocker {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool ok(this traceLocker tl) => runtime_package.okᴍ(tl);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void Gomaxprocs(this traceLocker tl, int32 procs) => runtime_package.Gomaxprocsᴍ(tl, procs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void ProcStart(this traceLocker tl) => runtime_package.ProcStartᴍ(tl);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void ProcStop(this traceLocker tl, ж<p> pp) => runtime_package.ProcStopᴍ(tl, pp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void GCActive(this traceLocker tl) => runtime_package.GCActiveᴍ(tl);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void GCStart(this traceLocker tl) => runtime_package.GCStartᴍ(tl);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void GCDone(this traceLocker tl) => runtime_package.GCDoneᴍ(tl);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void STWStart(this traceLocker tl, stwReason reason) => runtime_package.STWStartᴍ(tl, reason);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void STWDone(this traceLocker tl) => runtime_package.STWDoneᴍ(tl);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void GCSweepStart(this traceLocker tl) => runtime_package.GCSweepStartᴍ(tl);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void GCSweepSpan(this traceLocker tl, uintptr bytesSwept) => runtime_package.GCSweepSpanᴍ(tl, bytesSwept);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void GCSweepDone(this traceLocker tl) => runtime_package.GCSweepDoneᴍ(tl);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void GCMarkAssistStart(this traceLocker tl) => runtime_package.GCMarkAssistStartᴍ(tl);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void GCMarkAssistDone(this traceLocker tl) => runtime_package.GCMarkAssistDoneᴍ(tl);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void GoCreate(this traceLocker tl, ж<g> Ꮡnewg, uintptr pc, bool blocked) => runtime_package.GoCreateᴍ(tl, Ꮡnewg, pc, blocked);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void GoStart(this traceLocker tl) => runtime_package.GoStartᴍ(tl);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void GoEnd(this traceLocker tl) => runtime_package.GoEndᴍ(tl);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void GoSched(this traceLocker tl) => runtime_package.GoSchedᴍ(tl);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void GoPreempt(this traceLocker tl) => runtime_package.GoPreemptᴍ(tl);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void GoStop(this traceLocker tl, traceGoStopReason reason) => runtime_package.GoStopᴍ(tl, reason);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void GoPark(this traceLocker tl, traceBlockReason reason, nint skip) => runtime_package.GoParkᴍ(tl, reason, skip);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void GoUnpark(this traceLocker tl, ж<g> Ꮡgp, nint skip) => runtime_package.GoUnparkᴍ(tl, Ꮡgp, skip);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void GoSwitch(this traceLocker tl, ж<g> Ꮡnextg, bool destroy) => runtime_package.GoSwitchᴍ(tl, Ꮡnextg, destroy);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void emitUnblockStatus(this traceLocker tl, ж<g> Ꮡgp, uintptr gen) => runtime_package.emitUnblockStatusᴍ(tl, Ꮡgp, gen);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void GoSysCall(this traceLocker tl) => runtime_package.GoSysCallᴍ(tl);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void GoSysExit(this traceLocker tl, bool lostP) => runtime_package.GoSysExitᴍ(tl, lostP);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void ProcSteal(this traceLocker tl, ж<p> Ꮡpp) => runtime_package.ProcStealᴍ(tl, Ꮡpp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void HeapAlloc(this traceLocker tl, uint64 live) => runtime_package.HeapAllocᴍ(tl, live);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void HeapGoal(this traceLocker tl) => runtime_package.HeapGoalᴍ(tl);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void GoCreateSyscall(this traceLocker tl, ж<g> Ꮡgp) => runtime_package.GoCreateSyscallᴍ(tl, Ꮡgp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void GoDestroySyscall(this traceLocker tl) => runtime_package.GoDestroySyscallᴍ(tl);
}
partial class runtimeᴛtraceStackTable {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 put(this ж<traceStackTable> Ꮡt, slice<uintptr> pcs) => runtime_package.putᴍ(Ꮡt, pcs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void dump(this ж<traceStackTable> Ꮡt, uintptr gen) => runtime_package.dumpᴍ(Ꮡt, gen);
}
partial class runtimeᴛtraceWriter {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static traceWriter writeGoStatus(this traceWriter w, uint64 goid, int64 mid, tracev2.GoStatus status, bool markAssist, uint64 stackID) => runtime_package.writeGoStatusᴍ(w, goid, mid, status, markAssist, stackID);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static traceWriter writeProcStatusForP(this traceWriter w, ж<p> Ꮡpp, bool inSTW) => runtime_package.writeProcStatusForPᴍ(w, Ꮡpp, inSTW);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static traceWriter writeProcStatus(this traceWriter w, uint64 pid, tracev2.ProcStatus status, bool inSweep) => runtime_package.writeProcStatusᴍ(w, pid, status, inSweep);
}
partial class runtimeᴛtraceSchedResourceState {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool acquireStatus(this ref traceSchedResourceState r, uintptr gen) => runtime_package.acquireStatusᴍ(ref r, gen);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void readyNextGen(this ref traceSchedResourceState r, uintptr gen) => runtime_package.readyNextGenᴍ(ref r, gen);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool statusWasTraced(this ref traceSchedResourceState r, uintptr gen) => runtime_package.statusWasTracedᴍ(ref r, gen);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void setStatusTraced(this ref traceSchedResourceState r, uintptr gen) => runtime_package.setStatusTracedᴍ(ref r, gen);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static traceArg nextSeq(this ref traceSchedResourceState r, uintptr gen) => runtime_package.nextSeqᴍ(ref r, gen);
}
partial class runtimeᴛtraceStringTable {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 put(this ж<traceStringTable> Ꮡt, uintptr gen, @string sʗp) => runtime_package.putᴍ(Ꮡt, gen, sʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 emit(this ж<traceStringTable> Ꮡt, uintptr gen, @string s) => runtime_package.emitᴍ(Ꮡt, gen, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void writeString(this ж<traceStringTable> Ꮡt, uintptr gen, uint64 id, @string s) => runtime_package.writeStringᴍ(Ꮡt, gen, id, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void reset(this ж<traceStringTable> Ꮡt, uintptr gen) => runtime_package.resetᴍ(Ꮡt, gen);
}
partial class runtimeᴛtraceTypeTable {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 put(this ж<traceTypeTable> Ꮡt, ж<abi.Type> Ꮡtyp) => runtime_package.putᴍ(Ꮡt, Ꮡtyp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void dump(this ж<traceTypeTable> Ꮡt, uintptr gen) => runtime_package.dumpᴍ(Ꮡt, gen);
}
partial class runtimeᴛrtype {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<uncommontype> uncommon(this rtype t) => runtime_package.uncommonᴍ(t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string name(this rtype t) => runtime_package.nameᴍ(t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string pkgpath(this rtype t) => runtime_package.pkgpathᴍ(t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static name nameOff(this rtype t, nameOff off) => runtime_package.nameOffᴍ(t, off);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<_type> typeOff(this rtype t, typeOff off) => runtime_package.typeOffᴍ(t, off);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @unsafe.Pointer textOff(this rtype t, textOff off) => runtime_package.textOffᴍ(t, off);
}
partial class runtimeᴛbitCursor {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void write(this bitCursor b, ж<byte> Ꮡdata, uintptr cnt) => runtime_package.writeᴍ(b, Ꮡdata, cnt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bitCursor offset(this bitCursor b, uintptr cnt) => runtime_package.offsetᴍ(b, cnt);
}
// </GlyphForwarders>
