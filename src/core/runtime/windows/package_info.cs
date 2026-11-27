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
using atomic = go.@internal.runtime.atomic_package;
using tracev2 = go.@internal.trace.tracev2_package;
// </ImportedTypeAliases>

using go;
using static go.runtime_package;
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
[assembly: GoDynamicTypeLift("7374727563747b6c6f636b2072756e74696d652e6d757465783b2063747874205b323030305d72756e74696d652e77696e43616c6c6261636b3b20696e646578206d61705b72756e74696d652e77696e43616c6c6261636b4b65795d696e743b206e20696e747d", "cbsᴛ1")]
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
[assembly: GoFieldAccess(typeof(go.@internal.runtime.syscall.windows_package.Context))]
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
[assembly: GoFieldAccess(typeof(overlappedEntry))]
[assembly: GoFieldAccess(typeof(pageAlloc))]
[assembly: GoFieldAccess(typeof(pallocData))]
[assembly: GoFieldAccess(typeof(pcHeader))]
[assembly: GoFieldAccess(typeof(pcvalueCache))]
[assembly: GoFieldAccess(typeof(pcvalueCacheEnt))]
[assembly: GoFieldAccess(typeof(pollCache))]
[assembly: GoFieldAccess(typeof(pollDesc))]
[assembly: GoFieldAccess(typeof(pollOperation))]
[assembly: GoFieldAccess(typeof(profBuf))]
[assembly: GoFieldAccess(typeof(ptrtype))]
[assembly: GoFieldAccess(typeof(reflectMethodValue))]
[assembly: GoFieldAccess(typeof(rwmutex))]
[assembly: GoFieldAccess(typeof(scase))]
[assembly: GoFieldAccess(typeof(scavengeIndex))]
[assembly: GoFieldAccess(typeof(scavengerState))]
[assembly: GoFieldAccess(typeof(semaRoot))]
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
[assembly: GoFieldAccess(typeof(Δslice))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("runtime/alg.go", "alg.cs", "ABUugsqUpoKmgqaCpoKokoKCAAg2ABQElAABHgAOBIKCAAIQwoKUpKS4goKUpKS4goKmgoKmgoKCgpSCypSClAADHAAKAoKCgpSUlIKUAAMyABcClJSkpKaUpKSkpKSCgpSkgoKkpIKCgpSUyMqSpoKUgpSClIKUgpSClIKUgpSClIKUgpSCgoKUgoKClKKClIKClLiUlKKClIKCgpSUlAACHAANAsaigqaixqIAFSqUqIKUgoKUgriClIKCupKCgpSmgoKClA==")]
[assembly: GoPositionMap("runtime/arena.go", "arena.cs", "AF3MAQAIAgAGEgAIAoKClIKCgoKCgqyyrLIAEYgBgoKUgpSCgqaCpqrqACKQAsKCgoKCgpSUAAgKtIKCypSCgoKUpoKUgoKCgoKUgpSCgqaCggAiONKCgpSUlIKUpoKU3IKClJKUlIKUgoKmgoKmlIKClIKmgoKUlIKClIKUAAkUlIKU7NKCgpSCAAMQ8oKUgpSCgoIABxAABxCCuJaSggAKFqLKpqaClKrClIKCyoKCpoKCgu6CgqiSgpSCgoKUqsLKpoKCgpSCuIKmgoKClIK47pSCgoKCgoKUgpS8ooKClJQACgyyggAIEoK4goKUkpSWkoKUgpTcgraUppSmpoKCpoKCgqaClKaCpoKmgpQABxCCpgACGgAKAgAIEPKClIIADh7aytzcgoKCgoKmpoKmlIKCupKCgpQACBQACQKClILKgpSClIKUgtymgoKCgpSCgriCpoKClAAIGAALAoKmgoKCgqaCuJSCgpTcgoKClJSClIKUlAAHEIKmyoKCkoIACBKmgsiClIKCpoKCpqa4AAoWgpSmuLiCgoKU", "764-766:1;912-917:1")]
[assembly: GoPositionMap("runtime/asan0.go", "asan0.cs", "ABIogIKCgoKCgoI=")]
[assembly: GoPositionMap("runtime/atomic_pointer.go", "atomic_pointer.cs", "AA1AABMCgoKCrLKClIKUAAIQ0gACENKClIKUAAIQ6KKClIKUqKiigpSClIKoqKKClIKU")]
[assembly: GoPositionMap("runtime/auxv_none.go", "auxv_none.cs", "AAgS")]
[assembly: GoPositionMap("runtime/cgo.go", "cgo.cs", "AEaaAeAABRDQAAkUlIKCgpQ=")]
[assembly: GoPositionMap("runtime/cgocall.go", "cgocall.cs", "AGncAfKCggALKgAQAoKmgqaCpoKmAA8g7pTulLiClLiUlLiCpqaCgoKCggANHIKClAACEuKUuKaCygAVLIKCgozSAAgSgoKCpoLssoKCgqaCgoLulLgAChaCgoKCgoKmlIKmyoKUpqaCpqaUpoKCgqailIKCpoIABxCCgoKUlIK4ysqCgriClJKmgriCgoKmgqqigoKUgqaUgpSCgoLcoqSCpoKC/oKCggAIEpT6kuiSACnUAfKUppS0goKClJKUgoL8tIKUgpS0goLKgpSCgpSClLSCgoKClIKUgpSCgsaCgpSCxoKCgpSSlIKClMaCgoK4gpSCpgAEEPKCgoKClIKCgoKUgoKmpoKm3AACEuKCpoKmgoK4rsKs0oKmgoKClAAZCqKSgoKmlLS0tLS0tLS0tMiCAAcQgoKUpoKUypSCgrS2", "416-420:1", "", "", "456:21", "432:66,434:17")]
[assembly: GoPositionMap("runtime/cgocallback.go", "cgocallback.cs", "AAcWog==")]
[assembly: GoPositionMap("runtime/cgocheck.go", "cgocheck.cs", "ABAyAAgCuJSClILKgoLKgsqC3IKmgoIAAxgACgIAAhYACgKClIKUgpQAAhYACgKClIKUgpSCgoIAAxQACQSClIKmAAIS4oKCgoKCgoKCgoKUlIKUgoKC", "65-68:1")]
[assembly: GoPositionMap("runtime/cgroup_stubs.go", "cgroup_stubs.cs", "AAgSgpTugoKU")]
[assembly: GoPositionMap("runtime/chan.go", "chan.cs", "ADp+kuaCgqYACAaCpoKUgqaCgq7CppTYgsaCtoKCgoKUlIKUAAIcAA0CruaUpqyyAA4eAA4CgoKUgqaCpoKmggASJoKmgoKmlIKCpoCmgJK2lIKClIKCgpSCgqaCgriCgoKCuIKCgoKCgoLKgoKClMqmgpSCgoKCgpSCgoKClJQAAhIACQKCgpSCgsqCgoKClKaCgpSCgoKCgpQAAhAADAyClIKCgoKSgoKUlIIAAhoAEQyCpqbogoIACgaigpSCpoKCgqaCgoKmlKaCgoKUgpKUgpSCgoKClLiCgoKUgoKUgoKCgpSUpoKCggADEPSCuIKUrLKosoIABhIADQiCpoKClIKmgqaCuAAJFMrKlIKUgpS4goKmlIKCgpSCgpTKgMqAksiUgoKUgpSCgoKUgoKmgoK4goKCgriCgpSCgoKCgoLugoKClKaClIKUgoKClIKCgoIAAiIAEQKCgpSCgpSU7oKCgqaCppKCgpSUgoKCgoKClKb8uNyCAAImABACAAImABACqJKokqaigpSCgpS4lKaigpSCgoLKlKiSqJKokqiSpqKCgoKCgoKUgoKmgoKCgpSCgoKUgoIAChaClLi43KaCgpKSrAANEO6CgoKUgqaClA==", "232-232:1;607-607:1")]
[assembly: GoPositionMap("runtime/checkptr.go", "checkptr.cs", "ABAStILugsqCvKKCyoKUygAIBoKC3IKCpoKCuAACKAASBAAIEriCuIKClIK4")]
[assembly: GoPositionMap("runtime/complex.go", "complex.cs", "AAcSooKClKaCyIKCgoKUgoKCpriClJSCtoKCgraCgoLI")]
[assembly: GoPositionMap("runtime/coro.go", "coro.cs", "ACNU0oKCgoKSgoKCuIKCgqaCgqzSgoKUgqKoooKCgq7CgoIACh4ADgKCgoKCAAcQgoKCgoKCAAkUlIKCuIKmpqbcgriCpriqogAKFoKClIKCgoIABxCC3ILcggAIEoKmpoK4goK4gqaCuA==", "47-60:1")]
[assembly: GoPositionMap("runtime/covercounter.go", "covercounter.cs", "AAsasoKCgoKUyg==")]
[assembly: GoPositionMap("runtime/covermeta.go", "covermeta.cs", "AA8cwoKClA==")]
[assembly: GoPositionMap("runtime/cpuflags_amd64.go", "cpuflags_amd64.cs", "ABMsAAcQgoKClII=")]
[assembly: GoPositionMap("runtime/cpuprof.go", "cpuprof.cs", "ADqIAQAIBIKUgqaCgoKCgqaCgoKSgoKCgoKUAAIUAAgElKaCgpTKtgACGgAQDpSmgoKCgpSmAAIQ5IKCgpSmgoK4gqaCgriCAAMcAAkCAAIaAAoCAAIoABECgoKCgoKUgoKCgpQ=", "", "", "179=FuncPCABIInternal/1/2/1,FuncPCABIInternal/2/2/2;189=FuncPCABIInternal/1/2/1,FuncPCABIInternal/2/2/2")]
[assembly: GoPositionMap("runtime/cputicks.go", "cputicks.cs", "AAgW")]
[assembly: GoPositionMap("runtime/create_file_nounix.go", "create_file_nounix.cs", "AA4WgoI=")]
[assembly: GoPositionMap("runtime/debug.go", "debug.cs", "AA+0AtIACDqSAAIwABYEgoKmggADFgAIBIKCpoIABhrigoKUlA==")]
[assembly: GoPositionMap("runtime/debugcall.go", "debugcall.cs", "ABoygt7kgpTKqqKCgoKCppQAAx4ABxCCgriSlIKUpgAGGAAJAoKCAAcQuMiCgri4goLcgpSCgty4uJSCpoK4lIKClNzcgoKCppQABxSigoKCpqaCgoK4goLcgriUgoKUgoKClIKCgoKUuKSSgpSCgoKCpoKi", "49-95:1;123-161:1;164-188:2;222-259:1;269-274:1", "", "", "", "272:54,273:13")]
[assembly: GoPositionMap("runtime/debuglog.go", "debuglog.cs", "ACx8ABIEqqIAHMIB5sSCgrimgrgAESqAlrKClJSogJaSqICWkqiAlpKogJaSqICWsoKCqICWkqiAlpKogJaSqICWkqiAlpKogJaygoKogJaygoKogNbCgoKUgpS0tqiAAANGgJaygoKCgoKmgoKClIKCpqiAlrKCgqiAlrKCgoKUAC9kkpTuzJKCgqiSgoKCgoKCqJKCgqiSgoKCqJKCgoKCgoK6koKClJSokoKCgoKUgoIACx6kgpSClIKClIKUgqiSqpKCgoKUppSCgoKUgoKUgqaCgriClIKCgoKCgoKmgpSm9oKCpoKClKaCgoKCgoKCpqaCgoKClJQABMwBkoI=")]
[assembly: GoPositionMap("runtime/debuglog_off.go", "debuglog_off.cs", "AAwagtqCpoI=")]
[assembly: GoPositionMap("runtime/dit.go", "dit.cs", "AAsakoKCgqiSgoKC")]
[assembly: GoPositionMap("runtime/env_posix.go", "env_posix.cs", "AA0SgoKClIKCpqqigoKCgpSUlKaCgpQAGDSSgpSCqJKClIKmgoKC")]
[assembly: GoPositionMap("runtime/error.go", "error.cs", "ACtUlIKCgpSCgpSCgoKUgpSmlAACEtKCgoKClILclIIABxCUggACENIABxCUggARJAAMGgAKFpSCgoKUgoKmooKCuIKCgoKCgpSClLS2AAYeAAgClLS0tLS0tLS0tLS0tLS0tLS0xMqygpSUgoK0tLS0tLS0tLS0tLS0tLS0tMqSgoKClIKCgpSuwoK4goKUgoKUgoKClIKUgoI=")]
[assembly: GoPositionMap("runtime/extern.go", "extern.cs", "AK4C6gQADAKCgoKUggAKSAAIAoKClAAMILI=")]
[assembly: GoPositionMap("runtime/fastlog2.go", "fastlog2.cs", "AAcc8oKUpoKClII=")]
[assembly: GoPositionMap("runtime/fastlog2table.go", "fastlog2table.cs", "AAkS")]
[assembly: GoPositionMap("runtime/fds_nonunix.go", "fds_nonunix.cs", "AAgS")]
[assembly: GoPositionMap("runtime/fipsbypass.go", "fipsbypass.cs", "AAkUkqiSqJI=")]
[assembly: GoPositionMap("runtime/float.go", "float.cs", "AA0elqSokqiSAAIS4oKqooKokqqiAAIaAAoCgpSCgoKUlIIAAhoACgIAAhoADQKClIKklKaCpoKUgoI=")]
[assembly: GoPositionMap("runtime/hash64.go", "hash64.cs", "ABAqgpKClKSCgrSCtIK0grSCtIKCgoKCssLClJSCspSCtqaCgqaCgqaCgqaCpoI=")]
[assembly: GoPositionMap("runtime/heapdump.go", "heapdump.cs", "ADeeAYKClIKCgqaCgoKUgriipoKCABIqkoKCgoKClIKCpoKClLqSgqaCgoK4gqiygsqCgpSClIKUgtyClKaCgoKCgpSCgoKCgpSokoKCgqaCgoKmooKCgoKCAAwcsoKCggAICrKmgoKCgpS4lJSCgpS4goKCgoKCgoKCgpSmgqaCgsqUgoKUlIKCgpSmlKaCgoKCgoKUlKaiooKCgpSCgqaCgoKCgoKCgoKCgoKClpKCgoKCgoK4goKCgoKCgoKUlJSUgoKCgoKCgoL4gqaCgsqClKSC3MqigoKCgoKmlLiCgoKmgoKCpoKUgoKUgoLcAAoOlJSCgpSCgoKCpoKCuIKCgpTKgoKCgpSUgpKCgpSCgpSCgpSCuIKCgoKmooKCgoKmgqaCgoKCgrqSyoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKClKaCgoKCgoKCgoKCgoKCgoKClIKCgqaCgoKCgoKCgpSCgpSCgqaCppSUgoKClIKClIKCgoLKpIKmgoKmgoKCgoKCgoKCgoKmgpSCpqamgoKCpqiyggAIBpSCgoKUgoKClKaCgoKCgoKUgpQ=", "412-433:1")]
[assembly: GoPositionMap("runtime/hexdump.go", "hexdump.cs", "ADOoAYKCuIKCgoKClIK4goK4poKCAAkIgpSCgoKmgpSCgriUgoKUuMqUgoKmAAoGgpSCgoaigoKClIKCgqaUlIKmgpSCgoK4gpSCgqaClIKCgpSmgoKmpoKCgoKUlIK4goLqooKCgoKUlLiCgoKUgoKUgg==", "175-179:1")]
[assembly: GoPositionMap("runtime/histogram.go", "histogram.cs", "AF7SAQAJBIKCAAESAAgCgoKUgriCgqaCqMK4koKkAAcQgoKmgoKsspSUpqa4gpSUyqa4goI=")]
[assembly: GoPositionMap("runtime/iface.go", "iface.cs", "ABUmggAJFJQABhgADAKCuIKClIKm3IKCuIKCgriCgtyCgoKCgoKUggAHEKr4goKCuIKClIKUgvwACAqCpoLKgtyCgqaUpqr2goKCgoLKlMqCgpSCAAMeAA0CgoLcgoKCuIKCgoKCgoKCgoKUgoKCgoKCgpSCgoKCgpTctIKUpoKCgoKCpq7CqsKCgpSqogAdMqSCgoKCAAEYAAsCgpSClIKUgoKUtIKUgpSCpoKCpqKCgoKmgpSmooKCgqaClAACGAALAoKUgpQAAhgACwKClIKUAAIYAA0EgpSClKaClJSmgoKUrNKCgoKmpoLKlKaUprjKlKaipoKCgu6CpoKCgqaSgoKCgoKUpoKCppTKnvKmgqaCgoKCuILKuKaUuLjKlKzSpoKCgu6CpoKCgqaSgoKCgoKClKaCgqaUygABGAAKAqiSpqaCgoKCAAocwuyy", "539-549:1;645-656:1")]
[assembly: GoPositionMap("runtime/lfstack.go", "lfstack.cs", "ABYwooKCgoKCgsqClIKUgoKClIKCggAHEJS6guqigpSmgqaC")]
[assembly: GoPositionMap("runtime/linkname_shim.go", "linkname_shim.cs", "AEWwAQAUAoKCppSCgpSCAAImABACAAIoABMCgoKmlIIAAigAEQIAAiIADgIAAiIADgI=")]
[assembly: GoPositionMap("runtime/list.go", "list.cs", "ABQyooIAGi6CgqaClKiSqJKoyIKmgoKmqJKCuKaCpqaCgqaokpSCpoKCpoKmgqaC")]
[assembly: GoPositionMap("runtime/list_manual.go", "list_manual.cs", "ABtAooIAFi6CgqaClKiSqJKoyIKmgoKmqJKCuKaCpqaCgqaokpSCpoKCpoKmgqaC")]
[assembly: GoPositionMap("runtime/lock_sema.go", "lock_sema.cs", "AA4msgAMlAKCgoKUggAEIoKm")]
[assembly: GoPositionMap("runtime/lock_spinbit.go", "lock_spinbit.cs", "AEKUAZKClAASIJKCgpQAAxgACAKCgoKUAAMiAA0ClOwABRCCAATYAYIABIgBooKmgoKU")]
[assembly: GoPositionMap("runtime/lockrank.go", "lockrank.cs", "AFy8AQBMigGCgpSClIKUAAcQ")]
[assembly: GoPositionMap("runtime/lockrank_off.go", "lockrank_off.cs", "AA8gpoKmgqyypoKssqzYuLi4uLg=")]
[assembly: GoPositionMap("runtime/malloc.go", "malloc.cs", "AIcDlgaCgpSCgoIAHgaCgqamuJSUgoKUgoKUgoKUgoKUypSmgqaCgpSCggAHEIKCgoKUgoKmgpSCuIKmgriCgoKCgoKClKamADt4gsqCppSCgoKmgtyCgoK4goKUgoL8goLGgrS0tLSmlLTagoKUgoIAEyiCgoIACRQACBKCgpSClKbKgoKCgoKmgoLcgoK4AB4kABUClJTcgoKCyoKCgoKUlIKUlJSUgpSCggAHEIKUgqaCytyCgriCgoKCgriCgoKCgoKCgpSmgriCpoKCgriCkgAIEoKClIKUlKaClIKCgoKCyoKCgpSCgpSCgtyC3LiCpgACIAAQBNyCgoKUgriCgoKUAAYSwoKCgoKCgpSCgoK2AAoYAA0CgoKClIKClIKClKaCppKCgoKUAB9YABQCgoLKgqaCgpSClO6mgoK8ooKCyoKokoKUgoKUgpSmpoK4goKUpoKUgpS4uIKmuIKUgpSmgoKUgpSCuIKCyoKU5rSCgoKUkpSCpgAePoKUgoLugoKCgpSUgoKCgoKmgoKCgpSCgqaUggAIEpTKAAkUAAsYgoKUgpSCgrgADBqU5rSCgoKUkpSCppSCgoKCpKSCgqaUgoKCpoKClIKCAAgSlMoACRQACxiCgpSClIKCpqr2ggAHEILKgrgADRzKlAAIBrSCgoKUkpSClIKmlIKCkoKCgoKUgoKUppSkAAcQlMoACRQACxiCgpSClIKCpua0goKClJKUgpSCppSCgoKCgqSkgoKCgoKUgoKUgoIACBCUygAJFAALGIKClIKUgoKmprSCgoKUkqaUpoKCgoKCAAgSlMoACRQACxiCgpSCpoLclAAKFoKUAAgOgoKmgoKCAAcQgoKCgoKUppSUlKaClKaCgoKCAAgSggADENSCgriUuL7EgoKmABpsACkCgpSmpoLKgoKUkpSUlIKCpgANHIKClIK4gsqCgqaCuKaqooKktoKUgoKUuIKm3IKU6sKCuIKClJSClILcgpSUuIKUpgAMCsaCgpSClIKUgoKUgqaCpoKCzJKClAACFgAIAoSSgoKUpoKClL7SqJIAAh4ADgKosgACIAAPAoKUgoKUAAIkAA8CqJLuwoKClIKCAAIU8pSUlJSqxpS0AAIWAAgCgoKClIIAGEYADQKCgpQAEsoBgsqUgoKmgoKClIKClIKClJQADhqCqqKUpKSkpKSkpA==", "2331-2333:1")]
[assembly: GoPositionMap("runtime/malloc_generated.go", "malloc_generated.cs", "ABMapIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmpIKCuJSCgriCppSUgoKUlIKCgpSCgoKCgoKCgoKCvIKClIKClISUgpSCppSUgoKCloKUgoK4goKCgoKUgoSCgoKCgpaCpoKClIKUppSElqaCgpSClIKCpoKEpoKmgoK4gpSmhIKCpoKCuJSCgriCppSUgoKUlIKUgoKEgoKCgpSEgoKCgoKChIKmgoK4gpS6gpSCpoKCgoKCgoKCgoK8goKUgoKUhIKmlISWpoKClIKUgoK4hJSCpoKCuIKUpoSCgqaCgriUgoK4gqaUlIKClJSClIKChIKCgoKUhIKCgoKCgoSCpoKCuIKUuoKUgqaCgoKCgoKCgoKCvIKClIKClISCppSElqaCgpSClIKCuISUgqaCgriClKaEgoKmgoK4lIKCuIKmlJSCgpSUgpSCgoSCgoKClISCgoKCgoKEgqaCgriClLqClIKmgoKCgoKCgoKCgryCgpSCgpSEgqaUhJamgoKUgpSCgriElIKmgoK4gpSmhIKCpoKCuJSCgriCppSUgoKUlIKUgoKEgoKCgpSEgoKCgoKChIKmgoK4gpS6gpSCpoKCgoKCgoKCgoK8goKUgoKUhIKmlISWpoKClIKUgoK4hJSCpoKCuIKUpoSCgqaCgriUgoK4gqaUlIKClJSClIKChIKCgoKUhIKCgoKCgoSCpoKCuIKUuoKUgqaCgoKCgoKCgoKCvIKClIKClISCppSElqaCgpSClIKCuISUgqaCgriClKaEgoKmgoK4lIKCuIKmlJSCgpSUgpSCgoSCgoKClISCgoKCgoKEgqaCgriClLqClIKmgoKCgoKCgoKCgryCgpSCgpSEgqaUhJamgoKUgpSCgriElIKmgoK4gpSmhIKCpoKCuJSCgriCppSUgoKUlIKUgoKEgoKCgpSEgoKCgoKChIKmgoK4gpS6gpSCpoKCgoKCgoKCgoK8goKUgoKUhIKmlISWpoKClIKUgoK4hJSCpoKCuIKUpoSCgqaCgriUgoK4gqaUlIKClJSClIKChIKCgoKUhIKCgoKCgoSCpoKCuIKUuoKUgqaCgoKCgoKCgoKCvIKClIKClISCppSElqaCgpSClIKCuISUgqaCgriClKaEgoKmgoK4lIKCuIKmlJSCgpSUgpSCgoSCgoKClISCgoKCgoKEgqaCgriClLqClIKmgoKCgoKCgoKCgryCgpSCgpSEgqaUhJamgoKUgpSCgriElIKmgoK4gpSmhIKCpoKCuJSCgriCppSUgoKUlIKUgoKEgoKCgpSEgoKCgoKChIKmgoK4gpS6gpSCpoKCgoKCgoKCgoK8goKUgoKUhIKmlISWpoKClIKUgoK4hJSCpoKCuIKUpoSCgqaCgriUgoK4gqaUlIKClJSClIKChIKCgoKUhIKCgoKCgoSCpoKCuIKUuoKUgqaCgoKCgoKCgoKCvIKClIKClISCppSElqaCgpSClIKCuISUgqaCgriClKaEgoKmgoK4lIKCuIKmlJSCgpSUgpSCgoSCgoKClISCgoKCgoKEgqaCgriClLqClIKmgoKCgoKCgoKCgryCgpSCgpSEgqaUhJamgoKUgpSCgriElIKmgoK4gpSmhIKCpoKCuJSCgriCppSUgoKUlIKUgoKEgoKCgpSEgoKCgoKChIKmgoK4gpS6gpSCpoKCgoKCgoKCgoK8goKUgoKUhIKmlISWpoKClIKUgoK4hJSCpoKCuIKUpoSCgqaCgriUgoK4gqaUlIKClJSClIKChIKCgoKUhIKCgoKCgoSCpoKCuIKUuoKUgqaCgoKCgoKCgoKCvIKClIKClISCppSElqaCgpSClIKCuISUgqaCgriClKaEgoKmgoK4lIKCuIKmlJSCgpSUgpSCgoSCgoKClISCgoKCgoKEgqaCgriClLqClIKmgoKCgoKCgoKCgryCgpSCgpSEgqaUhJamgoKUgpSCgriElIKmgoK4gpSmhIKCuJSCgriCppSUgoKUlIKCgpSEgoKCgoSChKaCpoKCuIKUuoKCgoKCgoKCgoK8goKUgoKmlISWpoKClIKUgoKmgoSmgqaCgriClKaEgoK4lIKCuIKmlJSCgpSUgoKClISCgoKChIKEpoKmgoK4gpS6goKCgoKCgoKCgryCgpSCgqaUhJamgoKUgpSCgqaChKaCpoKCuIKUpoSCgriUgoK4gqaUlIKClJSCgoKUhIKCgoKEgoSmgqaCgriClLqCgoKCgoKCgoKCvIKClIKCppSElqaCgpSClIKCpoKEpoKmgoK4gpSmhIKCuJSCgriCppSUgoKUlIKCgpSEgoKCgoSChKaCpoKCuIKUuoKCgoKCgoKCgoK8goKUgoKmlISWpoKClIKUgoKmgoSmgqaCgriClKaEgoK4lIKCuIKmlJSCgpSUgoKClISCgoKChIKEpoKmgoK4gpS6goKCgoKCgoKCgryCgpSCgqaUhJamgoKUgpSCgqaChKaCpoKCuIKUpoSCgriUgoK4gqaUlIKClJSCgoKUhIKCgoKEgoSmgqaCgriClLqCgoKCgoKCgoKCvIKClIKCppSElqaCgpSClIKCpoKEpoKmgoK4gpSmhIKCuJSCgriCppSUgoKUlIKCgpSEgoKCgoSChKaCpoKCuIKUuoKCgoKCgoKCgoK8goKUgoKmlISWpoKClIKUgoKmgoSmgqaCgriClKaEgoK4lIKCuIKmlJSCgpSUgoKClISCgoKChIKEpoKmgoK4gpS6goKCgoKCgoKCgryCgpSCgqaUhJamgoKUgpSCgqaChKaCpoKCuIKUpoSCgriUgoK4gqaUlIKClJSCgoKUhIKCgoKEgoSmgqaCgriClLqCgoKCgoKCgoKCvIKClIKCppSElqaCgpSClIKCpoKEpoKmgoK4gpSmhIKCuJSCgriCppSUgoKUlIKCgpSEgoKCgoSChKaCpoKCuIKUuoKCgoKCgoKCgoK8goKUgoKmlISWpoKClIKUgoKmgoSmgqaCgriClKaEgoK4lIKCuIKmlJSCgpSUgoKClISCgoKChIKEpoKmgoK4gpS6goKCgoKCgoKCgryCgpSCgqaUhJamgoKUgpSCgqaChKaCpoKCuIKUpoSCgriUgoK4gqaUlIKClJSCgoKUhIKCgoKEgoSmgqaCgriClLqCgoKCgoKCgoKCvIKClIKCppSElqaCgpSClIKCpoKEpoKmgoK4gpSmhIKCuJSCgriCppSUgoKUlIKCgpSEgoKCgoSChKaCpoKCuIKUuoKCgoKCgoKCgoK8goKUgoKmlISWpoKClIKUgoKmgoSmgqaCgriClKaEgoK4lIKCuIKmlJSCgpSUgoKClISCgoKChIKEpoKmgoK4gpS6goKCgoKCgoKCgryCgpSCgqaUhJamgoKUgpSCgqaChKaCpoKCuIKUpoSCgriUgoK4gqaUlIKClJSCgoKUhIKCgoKEgoSmgqaCgriClLqCgoKCgoKCgoKCvIKClIKCppSElqaCgpSClIKCpoKEpoKmgoK4gpSmhIKCuJSCgriCppSUgoKUlIKCgpSEgoKCgoSChKaCpoKCuIKUuoKCgoKCgoKCgoK8goKUgoKmlISWpoKClIKUgoKmgoSmgqaCgriClKaEgoK4lIKCuIKmlJSCgpSUgoKClISCgoKChIKEpoKmgoK4gpS6goKCgoKCgoKCgryCgpSCgqaUhJamgoKUgpSCgqaChKaCpoKCuIKUpoSCgriUgoK4gqaUlIKClJSCgoKUhIKCgoKEgoSmgqaCgriClLqCgoKCgoKCgoKCvIKClIKCppSElqaCgpSClIKCpoKEpoKmgoK4gpSmhIKCuJSCgriCppSUgoKUlIKCgpSEgoKCgoSChKaCpoKCuIKUuoKCgoKCgoKCgoK8goKUgoKmlISWpoKClIKUgoKmgoSmgqaCgriClKaEgoK4lIKCuIKmlJSCgpSUgoKClISCgoKChIKEpoKmgoK4gpS6goKCgoKCgoKCgryCgpSCgqaUhJamgoKUgpSCgqaChKaCpoKCuIKUpoSCgriUgoK4gqaUlIKClJSCgoKUhIKCgoKEgoSmgqaCgriClLqCgoKCgoKCgoKCvIKClIKCppSElqaCgpSClIKCpoKEpoKmgoK4gpSmhIKCuJSCgriCppSUgoKUlIKCgpSEgoKCgoSChKaCpoKCuIKUuoKCgoKCgoKCgoK8goKUgoKmlISWpoKClIKUgoKmgoSmgqaCgriClKaEgoK4lIKCuIKmlJSCgpSUgoKClISCgoKChIKEpoKmgoK4gpS6goKCgoKCgoKCgryCgpSCgqaUhJamgoKUgpSCgqaChKaCpoKCuIKUpoSCgriUgoK4gqaUlIKClJSCgoKUhIKCgoKEgoSmgqaCgriClLqCgoKCgoKCgoKCvIKClIKCppSElqaCgpSClIKCpoKEpoKmgoK4gpSmhIKCuJSCgriCppSUgoKUlIKCgpSEgoKCgoSChKaCpoKCuIKUuoKCgoKCgoKCgoK8goKUgoKmlISWpoKClIKUgoKmgoSmgqaCgriClA==", "", "", "66=base/1/1/2;223=base/1/1/2;380=base/1/1/2;537=base/1/1/2;694=base/1/1/2;851=base/1/1/2;1008=base/1/1/2;1165=base/1/1/2;1322=base/1/1/2;1479=base/1/1/2;1636=base/1/1/2;1793=base/1/1/2;1950=base/1/1/2;2107=base/1/1/2;2264=base/1/1/2;2421=base/1/1/2;2578=base/1/1/2;2735=base/1/1/2;2892=base/1/1/2;3049=base/1/1/2;3206=base/1/1/2;3363=base/1/1/2;3520=base/1/1/2;3677=base/1/1/2;3834=base/1/1/2;3991=base/1/1/2;4190=base/1/1/2;4335=base/1/1/2;4480=base/1/1/2;4625=base/1/1/2;4770=base/1/1/2;4915=base/1/1/2;5060=base/1/1/2;5205=base/1/1/2;5350=base/1/1/2;5495=base/1/1/2;5640=base/1/1/2;5785=base/1/1/2;5930=base/1/1/2;6075=base/1/1/2;6220=base/1/1/2;6349=base/1/1/2;6475=base/1/1/2;6601=base/1/1/2;6727=base/1/1/2;6853=base/1/1/2;6979=base/1/1/2;7105=base/1/1/2;7231=base/1/1/2;7357=base/1/1/2;7483=base/1/1/2;7609=base/1/1/2;7735=base/1/1/2;7861=base/1/1/2;7987=base/1/1/2;8113=base/1/1/2;8239=base/1/1/2;8365=base/1/1/2;8491=base/1/1/2;8617=base/1/1/2;8743=base/1/1/2;8869=base/1/1/2;8995=base/1/1/2;9121=base/1/1/2;9247=base/1/1/2;9373=base/1/1/2")]
[assembly: GoPositionMap("runtime/malloc_stubs.go", "malloc_stubs.cs", "AClUgoKCyqaCqqTcgoKmgoLupoKC3IK4lIKCpu6CuIKCyoKUAAIS4qaigpSSlIKUgriCgqaCgpSUgoKCgoKClIKClKaUgoKCAAgSlMoACRQACxiCgpSClIKCuKaigpSSlIK4jNKmgoKUlIKCgqaUgoIABxCmgoKUgoIACBKUygAJFAALGIKClIKUgoKmpqKClJKUgriCgqaCgpQAHj6ClIKC7oKCgoKUlIKCgoKCpoKCgoKUgoKmlIIACBKUygAJFAALGIKClIKUgoK4AAwalKrCgoKCgoKCgoKCgoKCuKaigoKCgoKCgoKCgrimgoKUgoKUAAIUAAsElKaCgoLKgpSCgtyCgoKCggAAEgAIApSCgoKCgqaCpoKClOaigoKCgoI=")]
[assembly: GoPositionMap("runtime/malloc_tables_generated.go", "malloc_tables_generated.cs", "AAgQAIMEiAg=")]
[assembly: GoPositionMap("runtime/map.go", "map.cs", "ABUugoKUAAIgAA0CAAIkAA8CgqYAAhoACxYACgSCgoKUpoKCgpQAAiQAEBYACwKCgoKClIKUgqau4oKCpoKqwoKCpqiSgoKCpgAGJgATBIK4AAIcAAsCgpSUqJKClJQAAhgACwKCqLKCqJKokgACGgAMAoKUgoKUqJKosoKUgoKUAAIQ6tKCgoKCgg==")]
[assembly: GoPositionMap("runtime/map_fast32.go", "map_fast32.cs", "AA0gAAQWAAoYAAsWAAoG")]
[assembly: GoPositionMap("runtime/map_fast64.go", "map_fast64.cs", "AA0gAAQWAAoYAAsYAAsG")]
[assembly: GoPositionMap("runtime/map_faststr.go", "map_faststr.cs", "AA0gAAQWAAoYAAsG")]
[assembly: GoPositionMap("runtime/mbarrier.go", "mbarrier.cs", "ABesAgCAAQKClLgACBKCggADFAALCAACEgAMCgACIAAPAoKClIKClIKClKiSqJIAAhoADQK4lKaCggAEHgAMAoKClILugoKCgpSCgpSCgqaCpoLugriCuIIAAiAADwKClAACHAANAriUAAIYAAkCqJKossqUqLKCppQAAiQAEASC")]
[assembly: GoPositionMap("runtime/mbitmap.go", "mbitmap.cs", "AEOYAQAKBgAYTgANAoKCgpQABhYACgKCgoKmgoKUlKiSlIK4gpSmggAGGAAJAoKClIKmggACLgAVBIKWkoKUlJSmpgACFgAIAoKCuIK4goKUuIK4goKCAAQYAAgEgoKUppSCgpTKpoKClKamgoKmgsqClIKClAACENIABloAKgKClIKUgqaCgoKmgoKCpoLulJaSgqaCgpSUgoKCgpSCgqaCgoKUgoKCggAEJgAOAoKUgpSClpKCpoKClJSCgoKUgoK6koKCgpSCgpSCAAkkAA4ClIKClILclJSssoKCgoKowoKCgpQAAhLiggAJFIKCgoKUgpSCgoKmlAACFAALBKaCgoLKgpSCgpSm3IKCgoKCgpSCgoKCgqaCpoKCgoKCgoKmABQsooKUgoKU5rIAESSUgoKUpqIAMWSCgpSmssqCgpSCgoKClIKCgqaCgpTmxIKCgpSClIKCgoKCpoKCgoKUgoK4goKCgpSUgoKCgoKCgoKCgoKUgpTmsoKCgpSCgpSCgoKCgqaCgoKCgpSCgriCgoKClJSCgoKCgoKCgoKClIKmgpSCgoKCgqaClKaU6MKClLiUgoKCgpKCgoKClIKmgoKCgoKCgoKSgoKCgqa4goKCgoKUpq74rviu+AACEgAJCAARJpKCrsKCgoKCgoKCgoKC7sKCgoKUgqaUgpSCgoKUlIKCuIKCgqaClNyClIIAAhgACQKClIIAAiAADQKClIIABhYACAKmlIKClKyypoKCgqiSqMiokqjI6LKCgpSssoKCggALGKiSgoKUlAAMEAALEoKCgoKClJSUgoKClIIAAjYAHAKmgriU3JSCuIKUpoKCrOgABBoACgKCgpSCgoKClIKUlIKCgoKUgoKCpgAJJAAQAoKUgoKUgpSCgoKCgoKUlIKCgoKCzqKCysqClKqigoKClJQADBaygoKCgoKUAAIqABQClpKUgoKmgoKCuIKCgpSUlIKCgoKCgpSCgoKUuIKCgoKCgtyCgoKCgoKmAAgSgoKUgqaCgoKCgu6CgrjugoKUpoKClIKCuIKCgu6CgoKCgoKCyoKCgpTclIKCgoKCuIKCgoKCpoKC6oKCgoKClKaigoKCgoKClIKCgoKClIKUgoKCgoKCgriCgoKCgoKmggAEFuI=")]
[assembly: GoPositionMap("runtime/mcache.go", "mcache.cs", "AEuqAbIACxaCgoKCgoKUgpSkAAIS4oKC7oKCAAMQ1IKCypSUAAwQ5JSCyoKUgqaUgpSmgoKmgoKUpoKmuIKCpoLKpgAOHoKClKiSgpSCgtyUgoKCuIKCgqamuAAIEoKCppSClIKCgoKCgqaCgriU3LiCuIKmgoKC7qbsAAwQgoKCgoKClIKCqqKCuIKCqqKClA==", "99-104:1;120-132:1")]
[assembly: GoPositionMap("runtime/mcentral.go", "mcentral.cs", "AC1ikoKCgoKqoqqiqqKqouikkpSCgoKCAA8glIKmgoKmgpSCgoKUlIKCAAoWgoKClJSUgoKCgqa4lIKCgoK4goK4goKCgoKmgoKUgpS4lO7igqaCpu6muAAHEIKUlLjMkpKCgpSC")]
[assembly: GoPositionMap("runtime/mcheckmark.go", "mcheckmark.cs", "ACdMsqaCopSUgoKUpqaClIK46JKClAAKDLKCgoKmppSClIKClJSUgqaygqKUlIKCgoIAAhDS7oKCgoKCgoKCggARCqKUnIyCgpSUgoK4griClIKUtLaC7oKClIKClIKUgoKClIKUpoKCgoKCgoKCgpSCyoKClIKCgpSmgpSCgpS4lIKCgoKCpKaCgoKCgqSmlIKClAADEtKmgsaUgoKUgoIABxC4goI=", "42-57:1;167-219:1;183-190:1.1")]
[assembly: GoPositionMap("runtime/mem.go", "mem.cs", "AApiACgCgoK4gqYAAhDSgqqiAAIaAAoCgqyyrLKqogACKAAXDoKmgoIAAhgACQKCAAIcAAsCuIKmAAgQ0oKClMqCgoKUpOyCgoKUgoKUpraCggAHEIKClIKClAADHAAQDoKmrsKC")]
[assembly: GoPositionMap("runtime/mem_nonsbrk.go", "mem_nonsbrk.cs", "AAwagg==")]
[assembly: GoPositionMap("runtime/mem_windows.go", "mem_windows.cs", "ABq8AaamAAQelAAEIKaC")]
[assembly: GoPositionMap("runtime/metrics.go", "metrics.cs", "ACx8soKmuIIACxiUpIKmgtyC3ILcgtyC3ILcgtyC3ILcgtyC3ILcgtyC3ILcgtyC3ILcgtyC3ILcgtyC3IKmgu6C3ILcgqaC7oLcgtyCyoLKgtyC3ILcgsqC3MqC3ILegtyC3ILcgtyC3ILcgtyC3ILcgtyC3ILcggAEEILcgtyC3ILcgtyC3ILKysrKytyCyoLKgoK4pqKCyqKCqJKCgoKClJKCABMusoKClKjCgoKUqMKCgpSokoKCpqiSAB1KsqaCgoKCgoKCgoKClIIAFzKykpKSkpKClIKCkoKSggAJFpIAECSSgoKCAAsakoIAELQBogARKMKCgpSCgpSUtLS0tLS2ABxEsoKClIKClIKClAAWNMKCgoKUlIKClJSssqamgg==", "91-94:1;98-101:2;105-108:3;112-115:4;119-122:5;126-129:6;133-136:7;140-143:8;147-150:9;154-157:10;161-164:11;168-171:12;175-178:13;182-185:14;189-192:15;196-199:16;203-206:17;210-213:18;217-220:19;224-227:20;231-234:21;238-241:22;245-248:23;252-260:24;264-267:25;271-274:26;278-286:27;290-293:28;297-300:29;304-307:30;310-313:31;316-319:32;323-326:33;330-333:34;337-340:35;343-346:36;349-352:37;355-358:38;362-366:39;370-373:40;377-380:41;384-387:42;391-394:43;398-401:44;405-408:45;412-415:46;419-422:47;426-429:48;433-436:49;440-443:50;447-450:51;454-460:52;463-466:53;470-473:54;477-480:55;484-487:56;491-494:57;498-501:58;505-508:59;511-513:60;516-518:61;521-523:62;526-528:63;531-533:64;537-540:65;543-546:66;730-737:1", "579=runtime.metricReader.compute", "76=makeStatDepSet/1/53/8,makeStatDepSet/2/53/15,makeStatDepSet/3/53/22,makeStatDepSet/4/53/29,makeStatDepSet/5/53/36,makeStatDepSet/6/53/43,makeStatDepSet/7/53/50,makeStatDepSet/8/53/57,makeStatDepSet/9/53/64,makeStatDepSet/10/53/71,makeStatDepSet/11/53/78,makeStatDepSet/12/53/85,makeStatDepSet/13/53/92,makeStatDepSet/14/53/99,makeStatDepSet/15/53/106,makeStatDepSet/16/53/113,makeStatDepSet/17/53/120,makeStatDepSet/18/53/127,makeStatDepSet/19/53/134,makeStatDepSet/20/53/141,makeStatDepSet/21/53/148,makeStatDepSet/22/53/155,makeStatDepSet/23/53/162,makeStatDepSet/24/53/174,makeStatDepSet/25/53/181,makeStatDepSet/26/53/188,makeStatDepSet/27/53/200,makeStatDepSet/28/53/207,makeStatDepSet/29/53/214,makeStatDepSet/30/53/233,makeStatDepSet/31/53/240,makeStatDepSet/32/53/247,makeStatDepSet/33/53/272,makeStatDepSet/34/53/280,makeStatDepSet/35/53/287,makeStatDepSet/36/53/294,makeStatDepSet/37/53/301,makeStatDepSet/38/53/308,makeStatDepSet/39/53/315,makeStatDepSet/40/53/322,makeStatDepSet/41/53/329,makeStatDepSet/42/53/336,makeStatDepSet/43/53/343,makeStatDepSet/44/53/350,makeStatDepSet/45/53/357,makeStatDepSet/46/53/364,makeStatDepSet/47/53/380,makeStatDepSet/48/53/387,makeStatDepSet/49/53/394,makeStatDepSet/50/53/401,makeStatDepSet/51/53/408,makeStatDepSet/52/53/415,makeStatDepSet/53/53/447")]
[assembly: GoPositionMap("runtime/mfixalloc.go", "mfixalloc.cs", "ADhwwoKUlIKCgoKCgoKCguaCgoKmgoKCgoKUlIKCpoKClIKCgqaCgoKC")]
[assembly: GoPositionMap("runtime/mgc.go", "mgc.cs", "AJcB3gIAEgLmgoKmyqaCpoKCgoKCgoKCrtSCgoKCggAmTpKCACZSAAYU1oKCgpSCpgC8AaYE1gAlTIK8opSCgpSUlILKggAjVrKClJSCpIKUgraUAAYSAAwKgoCCgqSClIC4goKiAA4cypSCgu6CgoKCuIK4lIKCgriUgoKmgriUlIKmlIKClIKCgoK4lAANHMqCAAcQgsqUuKbKgriCgqYAECKUgu7uuO6mgoKCpu6CuIKmogAdWgAYBpSO4oLKuKaCpu6mgoK43IK47oKCgoKC7gAKFoKCgoKCgu6ClNymgpSmgpSCprimuLiCyrjKpq70lKa8zIKCpgAEEIK2rLKc5IKCgpSCgoKCgsqCgoKUrLKUpriC2IIABhCilIKCggAEEsLKlLiUuJSCgu6UgsaC2oKCAAoKtJSClIKCgrjuggAJFIKCgoKApIK4gqaClIKCgriUgri4poKCgoKCgoKCggAJFIK43IKmlIK4goKC3KbKAA0cgoKCgqaUpoKkgoIAChbcuKYADRyClIKCgoKCgqaClJTu3IKUgoLGgpSCgoKClIKUggAKDpSCgpSU/IKUlIKU3IKClIKUuIKCgoKCuIKCuIKmgqaUAAMQAAkMggAJFIKClIKCggAIEri8AAsUggAfMILK7oKUlJQAESSmgpQADyC4pgAIEoKUgoKmgqaCgoKCppQACxSClLSC3IKCgsq0tKS4goKCgpSC7ri4gpTOwoKUqJLqooKCgv6ygoKClAAIDLKClKaCgqam3AAHENymlKaCgoKCgpSUgpSUgtwACBKCgoKUuAAGFgAIApSCpoKCgoKCgoKUlKaCgpSCpqaClLiCgriCgoKUggACGgAMBoKCyoKCgoKipoIABiQACgKokqaUgriC7oKSgoKUgriGsoKClIKssoKCgoKCgoKUlIKokpSmgoKClIKCgpQAAhoACgKCAA6EAQAIAoKCgpSClIKClIKmgg==", "752-754:1;834-836:2;869-871:3;947-954:4;1042-1059:1;1083-1085:2;1103-1111:3;1128-1135:4;1260-1276:1;1284-1289:1;1365-1374:1;1377-1389:2;1380-1380:2.1;1500-1516:3;1545-1558:4;1548-1552:4.1;1649-1651:5;1791-1818:1;1847-1884:2;2116-2119:1", "", "1526=itoaDiv/1/1/1;1563=Load/1/2/3,Load/2/2/4", "", "676:55,677:21")]
[assembly: GoPositionMap("runtime/mgclimit.go", "mgclimit.cs", "AE+0AcIACBIACAKmlIKmgoIAChTSgtyClIKCAAYSoqqiqqLu4riUgpSC6LKClJSCpoKCuIKCpgAHEIKCgpSCtLS0tLa4goIAFzCUruKC7qaCgoKCgpS4lKaUgrqS6JKCggAKFuKmpoKUgoKCgoKClAAaQJKu5oKClKiSAAckAAoCgpSCAAIQAAkEgoKClJSCuJSCgqYACAyygoKCgoKUgqaC7qaUgrS0tA==")]
[assembly: GoPositionMap("runtime/mgcmark.go", "mgcmark.cs", "ADV60oIAAhLiuIKmlqKmgoKUggAIEq7CppiCpoKCgqaCggAPIIIABxCmpoKmlIKmgoKCggAICqKCggAHEIKCgqaCtqbKAAgaAAsEkoKUgoLYgoLYgoLstqaC2siCgoKClLiCgsragoKCAAkUgoKClIKUkoKUgsiCgoKmAAYS4pTcgoKUgoKCgriCrtSCgoKCgqiSgoKCgoKCgri2goIACAwAEyKmgqKmgoKUgoKUgoIABxC4goKmlILKgoKUxoK0tsz4yoK4qKSuAAoGgpSCpoC4goKiABAggoKmgoKCAAcQgsqm3IKCgoKCggAIEoKCgoKClIKUlJSmgoKCAAcQgsqmpoKCgqaClO64gsqCgoKmAAcQgoIACxiC3IKCggAHEILKxgACGAAOBpQABxCCAAgSgpSmuIKUAAcQgrjKlIKCgoKClIKCgr6ygoKCrsK4goKmgoLcgoKClIKmggACGgAKAsqCpoKUgoKmlILupoLKgriUgoKUAA4oABMCgpKmlJK0pJL6gqyygpSUuIKClJSmpoKUgqaC3IKokoLuoqaUpsqCtpQABxCCgoKClIKClIKUlIKCgoKClIKUgoKClNyCgoKCgoKClLiClIKUrOKCpoKCgoIAChaCgoKC3JTKpqbKypSmpoKCuIK4uIKCgoKClIKUlIKUAAsgoqqigoKUqqKuwpSCuJSUpgAGQgAgAoLKgoKCgpS4goKCgoKCgrimgoKClIKCuIKCggAPINyCqJKCgoKCuIKCgu6CgoKmuIKCgu6CgoKClIKUgoKC3ISSgoKUAAciAA8Cgsq4gpSCqJKCgoKCuIKClIKCgoKm7oKCgqa4goKCuIKCggAIEgACGAAOCIKUlIKCgpSClIKCgpSCgtyCAAgaAAoCgoKCgoKCgoK4goKCgqaCgpSCgpSClKaCgoKC7oKUgpSCuKYACBKCuIKC7oKCuIKCAAQU0oKCggAKGAALBIKUlIKUlIKmgoKCgoK4gpSmgoLcgoLulIK8ooKCgoKUgoKUpoKCuJS4goKUgoKUgoKUlIIABxYACQKClJS4goKUuIKCpoKsspSCgoKUgoKC", "113-115:1;191-204:1;280-313:1;515-517:1;638-642:2;1536-1562:1", "", "200=readgstatus/1/1/1", "", "565:61,566:21")]
[assembly: GoPositionMap("runtime/mgcmark_greenteagc.go", "mgcmark_greenteagc.cs", "AFS2AdKmAAkUgoKClOaiABEklMqUqqK2gsrIlAACFgAIAqyy6JKCpuyygpSCgoKCgoKUtIK4rLKClKaigpSUgoKmgoKUqqIAAhLiqsKCuIKClIKCgriCgraCgpSmgpK4goK4gpSUpqiSqLKAgraAgsiClAAbPJKClIKC7AAFFgARFJSClLiCgoKmuIKClKiygoKUrOSClKzEgoKClJTospS4goK4ggADKgARAoKClIKClILs0oKUgoKCgoKUgoKUqqKClIKCrLKCgpTupoKmpu6CpgAHFAAIAoKokoKCgoKCgpSClIKClILKgqYACBIACAKCgpSCpqaCgoKmgoKUADh2woKCgqaCgpSCpoKCgqyygoKosoKCgoKCgoKokoKoABIkgoKClIKCgpSCgpSUrLKUgpKUkoKUggAIEqQACQ6SgpSmgqaC6sKCgoKClLamgoKCgpSCgoKUgoK4koKClJaSgoKUyoKCgqSCAAcQ/pSUgsiCgoIABRbygoKmgsqCyoKCgoKCgoKUgqaCgoKUppSmooKCgpKCgoKCgrimgriigoKUgoLcooKigpSCgoKCgpSCgqSCgoKCuKaC3KKCgpSCgtwACBKCgoKClIKUgoKCppQAAhLiggAKHsLOgoKCgoKUgoKCgoKClIKCgpSUgriigoKCgoKUrsKClIKUgpQACBgADwrugoKClKamgqbcgoIABxCCgoKUpoKCgoKC7ri4AAkUgoLKgoKC", "", "", "988=sizeclass/1/1/4,spanPtrMaskUnsafe/1/1/5")]
[assembly: GoPositionMap("runtime/mgcpacer.go", "mgcpacer.cs", "ANQC5gWigoKCggAGFNKCgoKCgoKCgoLugoKCgsqUlJS4goK4goKmyoLcysqClIKCggAIPgAYAoKmlIKCuLj8goIAChaCAAgSgoKUlIiypgAJFIIACBK4gqYAChaCgoKs9qamlIKmAAgSlIKCAB08gt6CgoKCpoKUgoKCgtKCgoIAAxoACQLcgriC3IKC3IK4gpSCgpSCgoKClIKClIIACCIADwKCpoKmyqYABxC4ggALGKaCgoKCpoLKpoKUgtyClIKmpoIAAh4AEgqClIK4goKCuIKmgoKCgoKUAAISAAgCgoKmgoIAAhDygoKCgoKmgoKCAAcU4pSCtLSCtMiCgoKClIKmpoK4uKKCgpSCgoK4gqiSgq4ACQSmgsqCytwAARwADAKCpqi0soKCgoKCAD5+ppaSgqbKuMqCppSClKaClAAZPgAJAu7K7oIACRSCggALGIKClJaSgoKUlIKCgoKClAACKAATAoKmpsrcgoLYgpQAGDLO4oKmgoKUgpSoxIKCgoLKgqbmgoKClIKUruKCpoKCpqjEgoKCpoKUgpQACAaCgoKUgoKClAAGGgAKAoKCgqaUgoKUgoIABBoACAKCgqiSgoKCgoKUgoIABBTSgoKCgoKUgoIABBwACQKUpoLKgoKCpoKC", "809-820:1;1354-1359:1;1401-1412:1", "", "435=Load/1/1/1")]
[assembly: GoPositionMap("runtime/mgcscavenge.go", "mgcscavenge.cs", "AJIBrAKSAAIgAA0C7rjcgpQACBKCgqaCyqYADBrKgpQAbfgB0oKUgpSCgpQAChYAChamgoKCgoKClIKmgpSogv6ygoKUgqiSrNKClKYAARDigpQABhQACQKCgqbcAAgSuJSC3IKmgqaU3IKUgrimgoKUlMoABxCWkoK4gqa8woKCAAgSAAoCgoKUlJSCAAIaAAwGAAES8oLclMqCpoKmypQAAhDSlIKUgoKCgpSCAAMaAAkCgoKCgpSCpIKmAAIU8pSCzIKCgoKUggACIAATDoKCAAcQgoKmgqamlNyCppSmuIKClIKCgsqCgpSCpoKUuIKUAAYWAAgCAA8guJSktLS0tLS0AAsWAAguABQCgoKCgoLugpSmlJSCgqaU3IKCkqbKgoKCgpTugu7ugriUlMqCuAA7hgHSgoKCgqzWgoKUqsKCgpSClLiCgpSUgqaCpoLuppSmggACFgAIAoLcruKCgqaCgu6CggADEvKCgoKUAAIQ0oKCAAgUkqqiABo+kgAHEpIAFzySqJKokqqilJSUlLi46JKCgpSCgpSClPqSgoKUgoKUpgAOPAAMBIKmgriCgpSCgoK4goK4goKmqJI=", "364-366:1;393-402:2;405-409:3;412-414:4;682-684:1;824-841:1", "", "396=Load/2/3/1,Load/3/3/1;716=load/1/2/3,load/2/2/4,heapRetained/1/1/4")]
[assembly: GoPositionMap("runtime/mgcstack.go", "mgcstack.cs", "AIgBkgKCgpSCAA4k1gAjTsKClIKClIKUgoKCgoKCgpSUgoKUggACEPKCgpSUgpSmgoKClKaCpoKClOiygpSCgoKUgpSUgoKCgpSCgoKClKyyAAIQAAgCgpSSgoKCgoKUgoKCqqKCgoKClIKClIKClA==")]
[assembly: GoPositionMap("runtime/mgcsweep.go", "mgcsweep.cs", "ADl+gqamgoIACBKCrsKssoKCgoKCgpSUpoK4ggAVQAAKAoKCgpSCAAoOooKUgoKClIKCpoKCtoIABBbigoKClILMkqyyrLKCAAYS4u4ABxCC7oKCgoIABxCUpoKUgoKCgpQAACIAEAKCgoKCppSUgoK4uIKUggATJKKCpoKmgpTqori4goKCuIKCgoKCgpS4goKUlJSCuMqUppQADyCCgqaCpqaCgqamggACEuLs6IKC3IKUgoKClNyCgoKUABMS9oKCpoKmpoKAgoK2goKCppSCABEkgoKUgoKCpoKCgpSCgqa4poKCgoK4yqaCgoLKgoKCpriCppSCgoKCgoKCgoKmgqaClIKUgpSCpoLKgrjcgqKmgqLcgoKmgqaCgoKCyoK2gri4gIKCpIIADBqUgqaUuILKgri2gpSCgoKUppTKgoKCppTclIKmgpSmlAAHEIKCgqYADyCCgpSUuJQABiIAEAKCgoKCgoKCgoKUlIKUlIKClIKCgoKUlIKUAAIoABEClKaCgoK4goKCgoIADyCUgoKCgpSUuIKCgry0ggADEtKmgu6CuIKUlIKCgoKUgrg=", "429-443:1;751-761:1")]
[assembly: GoPositionMap("runtime/mgcwork.go", "mgcwork.cs", "ABo4goIAZOgB4oKCgpSuwoKmgoKCgpSCgoKCgoKCgriC3IKClAAEEsKCgqaCgq7CgqaCgoKCpoKCgoKSgpSCgqaCgpQABBjygoKCpoKCgoKCgoKUgriCAAIQ0oKCpoIAAhQACQKCgpSClJSCgpSClJSCAAkUgpTKgpSCggADEMKClIKCgoKCgpSmgoKUAAQQ0gAVMMKC+IKCAAMQwoKCgoLKgoKEkoKCgoKClJSCgpSCpoKCuIKCgoKClLiu4oIAAhDygq7CgoKClKjEgoKCgqaC7LKCgsqCgqqigoKCgpSCgoKCgpSCpoKC", "449-451:1;547-557:1")]
[assembly: GoPositionMap("runtime/mheap.go", "mheap.cs", "AOoClAYADCCyrsIAW8YBgqbCgoKClAAIIAANAoKUlIKCgpSCgoKClIKCgpSCgoKmggANIIKokqiSAAIeAAwCqqIABBbippQAAxTigpQAAxTiAAIQ0oKClJSkAAVk4oIABBLi3IKUquKCooKCqJKClIK4gqaSgpSosoKUgoKCgoKCgoKCgoKCggAIEriCppQAAhIADg6C3JSCgoKmgoKUgoKUlIKUuIKUgqaUgriCgqaCpoKmgoKClAACEgANDJSCgoKClIKCxoKCgoKCyoKCgqaCgoKCgoKClMrugpSCgoKUgoKmggAKHJIAAhLosqaClJQABigAEQKClKqigoKigoKCgrQABxwADAKCgrSCggAIEgAIEoKCuIKClJS43IKUAAIeAAwCpoKmgoIAAhoADAKUgpSmgoKClKaCggACGgAMApSUgoKCuAAGKAAVBIKCysqCgqaCgoK4goKCggAIEpSUAAkUgoKCgoKClIKCpoKmlIKCgoKCgpSCgrimlJQAABYACgKCpqaCgqYACBLKgoKUgr6y3IKmmKaCgpS4lIK4gqaCpoKCpoKCgpS0tKSmgoKCgqaq9qa4gpSCgoKCuIKCgoKUgoKClJSUlJSUlKa4goKCgqbKAAsYAAkUlNyCpoK4yq7ilO6UpoKCuIKCgoKmpsq4lIKCpoKmgpTKgoIABxC4gu64goK4gpQABxCCgoK4qJKUgoKCgriClIKClJSCgpSCgpSCAAMeAA4EgoKCgriCgoKClIIADAaylJSCxoKUgoKUpoKmgsbsgoKCpoKUtLSkpqaCAAIUAAoIgqaUlIK6koKAuMSCgoKCgoKCgoKCgoKCgoKCgoKmggAJFpKC5rKClJSClJSClJSCgqaC5rKCgpSCpqaUguaygoKUgpSmlIKq0oK4griCpoKCpgAlUrKCgqKosoKCogAGEgAIAoKC3IKUgpSmgpSCgoKmgqaC7LKCgtyClJSClIKCgoKUgpSCgqzEgoKCgoKUgoKUgpSUAA8iwoKCgoKCgoKCyoKCgqaCuIKUuIKCgqiSgoKUgoIACyAACAqCgoKCgoKClILKppTKggAMHqKs0oKCgoKCgoKUgoKCpoKUgoKUgoKCgoKClJSWgoKUpqamqJKopIKCyoKUlIKUgoKCgpSUloKClKampoKUgpSClIKCAAke0oKCgpSCgoKqopSCgoKUgoIAIkiSqJK4poK4poKCgsqCyoKCpoKm3AAHEIKUyoKopJSCgqamprgAAhDSgoKCAA8IksqCgrimgqaCgoyKgoKCyoKCpoIACRSCggAIEoKClIKC7oKC5oKCgoKU3IKUlJaSgoKUgriCABYyyKrCgoKSgoKCAA8ggoKUgsqUgpSCAA4YsoKCgoKCggAfQKKmgqaCgqqigoKCAAYWAAoClIKCgoK0goKCgrSCgoKCtILGgoK0uIKCgrSCgoK0goKCtIKCgrQACxiCgoK0ggAKFpKqogAcOsKCpoKCpoLqooKmgoLKuIKCuLimgoKCyoKCuIKUggACEuIAAiIADgKCgoKmgpSCpoKCguqigoKCgoKUlIKClKaClJQ=", "1011-1018:1;1378-1380:1;1667-1696:1;1815-1815:1")]
[assembly: GoPositionMap("runtime/minmax.go", "minmax.cs", "AAkSgoKUpoKClKaCgoLoooKUgrimooKUgrim0pS0pKbSlLSk")]
[assembly: GoPositionMap("runtime/mpagealloc.go", "mpagealloc.cs", "AFrKAQAIAAAIFqKokqqiqJKmlLqSgpS8oqqiAAIQABASgoKssoIAeoICwriCgpSmpqamprasspKClKyyAAgO4riCuKbagpKClILKyoIABxCClJKCgpS4gpTKlNwAAiAAFBCCgoKUgoKCgu6CpgAEGgAIAqaCtqaCgoKUgqamuIKClILK7oKCyoKUuIKmpoKCgoKCggAFHAAIApSCkqSClIKCgqaCgoKCgoKCgpSCgoKUggACEuK4goKCgsqmAAwmABACABs4AA4eAAwegqaCgqaCggAHEIKUgpSCuKbcgoIAAhoACwKCgqaCypSCuIKmgpS4goKClMqCgqaUpoKUlNyCgoKCgoKUAAsUgoKmgoKCuLiCggACHgAPAriCyoKUgoKCgoKClIKCyoKC3JSUhNqClAACFAAJAqaClIKmgoKCppKklIKmgoKCgpSCpgACGAAJApSCkpKCgoKClIKCgoKCgoKClIKCgpQAFDCSgpSskoKUqJKClKiSgpSokoKUruqClMqC7u6ClKY=", "713-726:1")]
[assembly: GoPositionMap("runtime/mpagealloc_64bit.go", "mpagealloc_64bit.cs", "AB1CAA4eAAsYAA0W1oKmgoKCuIIACRwADAKCgtyCgtyCgoKCAAgSooIACximlNyCuO6ClIKmgriCggAIDtKCgpQACxiCgoKmgpSCuIKCuKaCgpSClIKmrLKCgoKCgg==", "109-112:1;117-125:2;130-133:3", "", "132=add/1/2/1,add/2/2/2")]
[assembly: GoPositionMap("runtime/mpagecache.go", "mpagecache.cs", "ABcykgACFgAIAoKUgoKCgoKUAAIS4oKClIKCgoIAAhgACwKUgpSCuIKCppSC3IKUggACGAAJAriClIKCgpSCgoKUAAcQgqaClIKCAAgSgoKmpgAJFII=", "", "", "139=chunkBase/1/1/1,alignDown/1/1/1,pages64/1/1/2,block64/1/1/3;156=alignDown/1/1/1,chunkPageIndex/1/2/2,pages64/1/1/2,chunkPageIndex/2/2/3,block64/1/1/3")]
[assembly: GoPositionMap("runtime/mpallocbits.go", "mpallocbits.cs", "AA4ekqiSqJKokoKUgqaCgoKUlIKCpqiSgryiqJKokoKUgqaCgoKUlIKUqJKqoqrCgpSCgoKUgoKClIIACRayooKCgoKCgpSCpoKClJSUhJKUlJS4goIABxCCgsqCgpSClIKClKaCgpSmuIKCgoKCgpTGAAIYAAkCgoKCgpSu4oKCgoKUlAACGgAMApKCgoKCuKaUgoKmgoKUlAACGgAMAqKCgoKClKaUgoKClIKClIKCgpSUgpSokqiSqJKokqiSrLKqoq74goKClIKmgoKUpsoADiLUgqrUgg==")]
[assembly: GoPositionMap("runtime/mprof.go", "mprof.cs", "AJcBtAKygoKCACJAsoKCqtKCgoKCgoLO2IKCgoKCggAOhAKCgpSCgqYAAhYACAIAAhTygoKmgoKCgoKuwoKCgoK4goIAAxAACQyUgoKCgoKoksq4gpSCgpSCgoLckrqSlIKUgoKCAAUW4oKCgoKmgoK4poKCpoKCvKKClAAEbsKCgpKCgoKClJSUlIKCgoKCgpSClMqCuJQAKlaCgpSmgoIAAxQACAKCpriCpriClIKCgoKUpoKmoqamggAJFJSUlIKCgoKSgoKUggADEtKCuMqClIKCgoK4goKUgoKCuKaCpoKClNyUgoKCgpSClAAFGPKClIKCqJKClIKCAAoaooKCpgAgRICWkqqigoKmAAIwABUCgoIAAxwADQK4goKCgoKCgoKCgpSCpsqCgoKCgoKClIK4goKCgoL+uIKmooKCgoKClIKUgpSCqJKCgriCgoKCgqQACiQACgKCkoKUgpSmgoKCgoKCpoKCgqaCvuKCgoKUgoKCgv6ClKaCrNKCgoKUgpSC3IKokoKCAAMUAAkCgpKClIKUrOKCgoKUgoKCgtqmgqiSgoIAAxTigoKCvuKCgpSCgoKCpqiSgoK6kgAEGpKokoKmAAkGABc+gqaCpoKmoqamppSmqJKygqCSgpS4gqamosqmlJSCAAcQgoKUlLiCgriCgrKUgpSC7oKCggAMGpKmgoKCgoK4gqaCpgALGILuwoKUrLLKpoKCppSmggAHEIKCgpQABxYACQIACBIADBqCgqaUuAAKFpCkgriylLaClIKUgpSmgqaCkoK4goLWgoKilKaCgriygqam3JCSgoKUuIKmggACEgAJAoKCgpSCgpSmgqYAChaCpoKCgoI=", "463-465:1;581-589:1;740-744:1;937-940:1;1033-1036:1;1066-1069:1;1151-1154:1;1165-1168:1;1202-1205:1;1215-1219:1;1242-1245:1;1339-1347:1;1341-1341:1.1;1396-1398:1;1426-1428:2;1439-1441:3;1566-1566:1;1576-1589:1;1596-1600:2;1609-1611:3;1621-1641:4;1635-1635:4.1", "", "641=FuncPCABIInternal/1/1/2;859=stk/1/1/5;981=stk/1/1/3;1070=stk/1/1/3")]
[assembly: GoPositionMap("runtime/mranges.go", "mranges.cs", "ACFCsoKClKiSgrioku7CgoKCgoKCgpSssoKClIKssoKClIKqooKUgpTMygAKGpKokqqiqqKqoqi2qJIAChqigoKClILOooKCgoKUggAEEsKqoqqigoKCgpQAGEiimrKSgoK2lLju3IKCpgACENKCgpSClIKUqJKCgpQABJABsoKUgoKCgoKClIKCqqKClIKClIKClICCgoKClIK2gg==")]
[assembly: GoPositionMap("runtime/msan0.go", "msan0.cs", "ABEmgIKCgoI=")]
[assembly: GoPositionMap("runtime/msize.go", "msize.cs", "AA4gwoKUgriCpLaCgpQ=")]
[assembly: GoPositionMap("runtime/mspanset.go", "mspanset.cs", "AEumAdSCpoKCgoK4poKCgqaClIKClIKmlKaCAAoWuIKCyqqikpSClIKCgoKUuILuyoKCgu6UgoL8ysqCgrjKAA0clKaUAAoU8oKCgpSC3IKClLiUuLimpoIACRqyrLIABxKSAAsaooKUqJKCAAYSoqiSqJKokgAHEpKokqiSqJLokpSCgpSokgAHEpKokg==")]
[assembly: GoPositionMap("runtime/mstats.go", "mstats.cs", "AKkCogWCgoK4goIAGTqy3AAJFpKmgoKClpqCpoKCgqaCgoIACBKCuIKUmpQAESSCgoKCgpSCgoKUgoKCgoKUgoKClIKCgsqCgoKCgpSCyoKCgoKCggAQIoKCgoKmgoKCgoK4goKCgoKCgoKCgtyokoIABxDClIK4lIKC7oKCgoKmgoKCggACENKUgoKClIIACTTSAAYQ0oKCggAaQvKCgoKClIKCgoKUgoKCADCAAQATAoKClIKmlIIAAiIAEAKCgpSCpgADEOKUggADEMKUABayAcKCggACENbcpoKCgtyCpoKCgqaCgqaCAAoU", "568-570:1", "", "395=load/4/9/1,load/5/9/1,load/6/9/1,load/7/9/2,load/8/9/2,load/9/9/2")]
[assembly: GoPositionMap("runtime/mwbbuf.go", "mwbbuf.cs", "AEycAZKCgriUpoIAAxLSqJIAAjIAFgKClIKCqqKClIKCAAIkABIIuILKggADHAANBIKCuJSUgpSCABAigoKC7pSClIKCuIKClKaCgqaCgpSCuJQ=", "180-182:1")]
[assembly: GoPositionMap("runtime/netpoll.go", "netpoll.cs", "AHuQAoKCgpaSAAIUAAkCgoKUgpSClKaCgr6ygoKCgpSCgoKCABg2wgAEIIKuwgAICJKCgoKClIKClIKUlIKCgoKCgoKCgoKUgoKClAAKCLKClIKClIKClIKm1riCgpSUlIKCggACENKCgpSCgoKUAAIS4oKCpoKUgoKC3Ki2urKCgoKUgoKCgqamgpSClIKCgoKUgriSlIKCgqSCpoKCkpSCgoKkgsqCkoKUgpSCgpSClOiygoKUgoKCkoKCgoKCgpSCgpSCgpSClAACHAANAoKSgpSClIKUgpSmgoKClILKgpSmgoK4lKaCAAgOwoKCuJSClILKgu6CpoKClAACEuKCgqaCgoKUppSCgpSCgoKClAAKCqKmgoKUlIKUgoKCgpSCgpSCgoKUgoKUgoKUgpSmgqaCpoKokqiSggAICKKCgoqCgriCgoKCgoKCpoKCggACEPKCgoKokg==")]
[assembly: GoPositionMap("runtime/netpoll_windows.go", "netpoll_windows.cs", "ACJGtJSUgpSokoKUAAwgsoKUhJKClJSClAAUJIKCgoK4gqaCgoKUppTmggAIMAAIAoKmgoKClKam7oKmpoKCgoKClJSCgpSClIKCgoKUgpSCgoKClIKUpoKCgpS0gpQACAqCtqrCAAcQgrikgoKClIKCggAKEoKU")]
[assembly: GoPositionMap("runtime/os_nonopenbsd.go", "os_nonopenbsd.cs", "AAgWyg==")]
[assembly: GoPositionMap("runtime/os_windows.go", "os_windows.cs", "AJEBqAKCgoKatgAkUpKClIKClIKCAA0SgoKUggALDoKCgpSCqJKmgoIADCKCgoKUlIKClIKUgoKUgoKmgvaChoqCgpSCgpSigoKmlKaCAAUygoKCqAALKAAKArimgpQADibmAAoKgoKCgoLKgoKClIKCgoIABxKSvIKCgoLcgpSmgpSUlJSClIKUlNyokoKClKa4gpSCgpSClIKmlIKSgpSmmMKClO7agoIAAhDSmIKUtMakgoKCgoK4goKmgqaCggAGFKKmlIKUlIKCooKClIKClIKCgqaCgqyygoKUgt4ACgiSzoKCpIKCgoKClMaUlIKCuJSmpoLIgoLIgoLI6LKCgoIACAySgpSCgoKCpoKCgoKUggAHFPTIgsqClIK4AAYS4qr0pqrIuLrIAA0KooKCgqaCgoKmgoKCgqaCgoKClIKCgoKmmKKCgoIABxCUgoKClIKClKyygoKCgpSUAAIU8oKClIKClIKClIKClIKCAAMS8riClIIAAhQACQKCgoKCgpSClIKmgpSCgpSssqiSgrqSggAELIKUlLS0poLKlJSo2ISSgpSClJSmgoCCpICCpICCpOaClIKCgoKUpriCgpaSgoKU3JSClKaUggAmbqKCuKaCuIKUgoKClIKCgpSWkoKC7qamgoKCpoKCAAsYlKaCgpSCgrimlIKUAAIU8gAIEgAEFuI=", "308-315:1;692-694:1;697-700:2;703-706:3;715-718:1;729-732:1;736-739:2;984-986:1", "", "316=efaceOf/1/1/1,compileCallback/1/1/1;671=getg/1/2/1,getg/2/2/1;772=FuncPCABI0/1/1/1")]
[assembly: GoPositionMap("runtime/panic.go", "panic.cs", "ACRwABIClLiCggADEvaCggAHQgAdAoKokoLssoKokoKokoKokoKssoKokoKokoKUgoKUgoKUgoKokoKUgoKokoKUgoLokoKokqSigpSUgpSmgoKCgpSUgpSUppKClJSClKaCgoKCgpSmlIKUlKaSgpSUgpSmgoKClIKClJSClJSm1pKCptaSgqbUgoKm1IKCptSCgqaCgqbUgoLqooKUpoKCgoK4poKCgtaSlLS0tKQAAowBAEMCgpSmgoKCuJSClKiS6qKCgpSCgoKCkpSCAAQQ0oKClIKUgoKCgqaClIKCgoKCpoIAAhIACAKClNyCgoIAChaCggACENKCgoKCgoKCgoKUlIKCgpSClJSUgqiSgsqCpoKmgoKEooKCgoKCgpSUlIKCgqaUlIKssoKUgoKCgpQACUTigoLoxLaCpoKClJS0pLSoooKCgpSCpoKUgoKCgoKUAAIQ0oKCgoKCgpSCgoIAFyiClAALGAAKAoKClLiCgoKCgqaCgoKClIKCgoKCgoKUgoKCgqaCgpSUgoKCgpQACBKC7pSCrNLcgpSClLiUgoKmpoIACBKCAAgO4pSCgqaCgriCggAJFIKCuKaClKaCgoKCgqaUppSmgszCgqaCkoKCpoKCgoKCAAcQgqaCpqaCgpSm5oKCgqaCpoKCgqaUgoKUqJKCgoIALVyCooKCgoKCgoKCgpS0gpS0goIACAyClIKokqiSqJKokqiSqJKokqiSqJIABn7qgqamgpQAGSwACQKCgsqCgpSmABMogoKCppSUgqaClLgAI0iCgriCgrjK6qQAAhDSgoKUgsqSgqaUuKamAAIQ0oKCgqaS7pSqooKmprimgqYAAh4ADAKCgty4gqa2goKCgpSCyIKCyIKCgraCAAgU4oKCgpSUpoKCgpSCgoKCgoKUgoKCgqa+lMqCppSuwoLKgoKmgoKUgoKClIKClIIAAhDS3AAKFqaUprgAAhTygoKU", "703-713:1;1003-1038:1;1120-1146:1;1431-1446:1;1463-1484:1;1493-1495:2;1597-1599:1", "", "", "", "750:54,751:13")]
[assembly: GoPositionMap("runtime/pinner.go", "pinner.cs", "ADLeAaKClILKggAIBoKCgoKUgpSUlAAKPsKCgoK4yoKClJSUgoKClIKClKaSgri4goKUkoKUlLi4poKCAAgYsqaCpoKmgqqigoKUgpQABhbCgoKmgqyyrLKmgqzSgoKmgtyCgoK4goKClLyigoKCgoKUgoKCgpSU6sKCgpSCgoKCgpSCgoKUAAYigg==", "202-205:1;216-219:2")]
[assembly: GoPositionMap("runtime/plugin.go", "plugin.cs", "ABUasoKCgoKUlIKUgpSCpoKCgqaIgoKCgoKCgoKCpoKCgsqClIKUgoKUAAkUgoKCgoKClIKClJQACAaigoKCgqaCyoKCgoKClIKmgrqS", "", "", "50=inRange/2/4/1,inRange/3/4/2,inRange/4/4/3")]
[assembly: GoPositionMap("runtime/preempt.go", "preempt.cs", "AFHUAQAfAriokqaCgoKCggAIKAAKFIK4lIL8gtyCggANHOqCuIK4goKmgoKCgpTu3IKCggAG1QHcpoIACtoBgpSClIIACA6ilKaCpIK8tpQAAxLSAAIeAA0YAAwGgriUgpTuuJTugoKCgoKClIKmgrzUAAomABICyoK4griCAA4egriClJQACBKUgriUAAcQpoKCABAmlLiClLaU", "320-333:1", "", "496=HasPrefix/2/3/1,HasPrefix/3/3/2")]
[assembly: GoPositionMap("runtime/preempt_xreg.go", "preempt_xreg.cs", "ADx8goIABhDSlKiSgpSCpoKCAAcQpgAGHAANAoKmgpSSpoKmgoKCgoI=", "122-124:1")]
[assembly: GoPositionMap("runtime/print.go", "print.cs", "ABtW8pSUgoKCgrgABRrygoKCgpSmgoKCgryigpSC3IKCpoKmgqaCAAgGgoKU7oKC3IKC3IKC3IKCporCgqbKgoKClIKCgoKUAAgKgoKCgoKCgpSUgoKCgpSmgqaCgoKClIK0grSCgrSCtoKCgoKCgoKUgqaCpoKUgqaCpqKCgqaCpoI=")]
[assembly: GoPositionMap("runtime/proc.go", "proc.cs", "AIUBkgIAHRiyuMqClNymlIKCAAkUlILKgoKmgoKmpoKCgriClIKCgqaClIKClIKmgriClLiCAAkUgoKCgtyUlIKUpgAHEIKUlIIACBKCgoKC7pSCgpSmgpSClIKmgoKC7rKCgriCuIKCgqaCqJLmgoKCgoKClIKUgqYABSLCAAIQ0qaClAAEaqIAAhoACgKC+gAJEoKCgpSCgoKClJSCpoKCgoKClIIAEgiygpSClIKUgpSClIKUgoKUgoKEooKCgoKCgpSUlIKCgpSC6JLmguaCAAgKooKCpoKSgoKClPyi6JIADyIACAKCgoKUlIKmgoIABxKkkoIAIjqCgqaCgoKUgqyy7qiSgoKokqyygoKUrsKCgoKUAAYWorikgoKCtra2goK2AAgS0oKC2oKCpoKClIKCpraUAAoWAAgCgoKCgoKCgoKCgoKCgoK4lJS4goKmgqaUgoKUgoKCgoKCgoKCgoKUgpSCgoKCppSCuIKC3IK4gpSCgoKCggAJFJSClKaUppSmuKKCkviSAAkUgoKCAAcQwpSClIKCgqiypoKmlIKUppSUgoLKuIKmgpSuwqaUgqwACQ6qkJaAlIKCggACFPKCAAIS4qaCABlmsoIAGDKC3JSClIKUpoKCrsIACA7CppSCggADEIK2goKClOqinoKCgpS4goIADRoACAKCpoK4lpK4goKmpoKUgoKmgriCgrimgpSUggAHEMqCgrSU7oKCpJSUpoLYguqCgs7kggAGFPKClO7CgpSCAAka0oKUgoKUgpQAGjqCpoLcACW2AbKCrLKCABlcACwalIKCgpS4gqaCgoKCpoKCpoKCgsqCgoKClIKClIKmgpSCgpS4goKClO6CgoKUgoKUgpSCpsqClIKmppQACxzilIKCkoKUlIKCgpSCgoKClJSUgoKCgoKCgpSCpsqClIKClJSCgoLclJSqoqSUqqKklKoAAxYACQKUggAIEoKClIK4poKmuJTuwpSCAAgSgoKUgriCpoKmgqaCgpQAAhIACQiCgpSssoKCAAgaAAoClAALGIKCgoKCgqaCpoKCgoLcuLimgoKCgqaCAAAeAA0CgoKUgqbcgoKClKaC3JSUuO4AAhoACgKCAAsYgoIACxoACQKClIKClIKmgoKmAAcQgoKCuIKmuIKUlIKCgsqCyoKClKaClIKCuIKCggACHAALAriClIKCgoKUAA0uAAkCypSCgsqCgoKkgoKCgoKC2oLKuJKCgoK4pIKmgoK4gpSUlIKmgoIABlQAJwLuggACFgAIAoLKAAgSpri4goLKpoKe0oK4ggAJFJSCgoKmrLKUgr6ygoKClJS67IKCgoKCgoKCgriCgoKUgoKCgoKCptymAAJIACQInMKCuIKCgpSCgoIAECK4ygAHEIKClNyCgpS4goKCgoKUlAAILgAUAoKUgoKUggADHgALAgAVNvKUgoKCgoKUgriClIKUgpSCupKCAAIUAAkCgoKCrsKCrNKCggAtZAARFpSCgoIACxiCgpSCgoKClLiClIKmooKCgpSCgoKClIKUgoKClIKCrsKCyoKCgpSCAAIeAAwCgoKClIKCgoKCgoKCgoKUlIKCggALDKKUgpSClIKmgoKCgoKmlAAMKAAjIoKClIK4lIKCgpSCpoIADh6ClIKUlJSCuIKUgpSClIKUgqaCgqauAAoKgoKmgoKmgoK4goKClIKCgoKCgpSClIKCgoKmgoKCuIKCgsqCgpSCAAciAA8GggAHEJSCgoKCgpSCgtyUlAAICqKUgpSUgpSUgoKCgoKUggAIDsKCgpSCpoKCgoIACAqilIKUgqaCpoKCgoKCgpSCAAIYAAsClLi4goKCgoKCgoKmgqaCgtyCyoKCpoKCgqYAEA7i3IjEgoKClILupoKCgoKCgoKUyoKCgpTcgoKCgoLKgoLKgqaCuIK4goKSgoKClAAMGoKSgoKCgoKCgoKClAAIEoKCpoKUlKamgpQABxCCgoKClIKCgoKUlO6CgoKCgoKUlIIAChamgqaCgoKUgpKCgpSClJSUgoKUgpSCACVMgoKCggANHIKCgoKSgoKUgpSCgqaUgoKCgriCgoKmgoKCgoKUAAgS7oKCgpSClIKCgpSCgqaUlKSCgoKmgpSCgoKCgpSCgoKCgoKCgoKUlIKUlIKCgqaCrsKClIKClIKSgoKmAAISAAgClJSCgpSClJSSggAPIIKCgoKUAAgSgpTKkoIACBIAAhDSgoKCgpSClILKrLKCgoKCyq4ACg6ClIIAEyiCgoKCuIKCgqaCgoKCgqaUrLLKgoLKggAICoKCgpSCgoLKAAzKAaKUgqaCgsqCpoKCyoKmpsoACBKUAAcQgtyCpriCppSCgtyClKaCpgACFPKUgqaCgqiylNyCgqa4uIKCppSCgoKCgoKCgpSCgpS4gqbmooKCgoKClLiClKaCgqaCppSCgqaCpqiSqLKClKaCAAgM0oKCgqa4goKUgoIACRQAFSyCgtymgoK4AAIiAA4CgqaigoK4lIKClIKCqJKCgpSUgoKClKiypriCAAgGooKUgoKClIKCgoKCgoKCgoKCgoKCgpS4goKCppSCgqaCgoKUlILugrgACBoACAKU3KaCgoK4ggAMpgKCgoKClKaClIKUgoKClIKUgoKCgqYAB4IE9IKCgoKCyoKCgoKClIKCgqYAAhIACAKCgoKCyoKUgoKCgoKUgoLugoKClIKCgpTKgpSCAAISAAkI3AADFAAJCIIAAyAADALKgoLcAAIeAAwCppSUAAY0ABcKlLiUAAgWxLiCggAEELKokoKCgpKSgraCppSssoKCkpSClIIADBDigqaCgoKCgoKUgqaCpoKCgpSClJSmgoKCgoKCgoKCgoKCpoKClNy4goKUuIKCgoKUuIKClIKCgoKCgrimpoKCgqamgpSs5IKUgoKUgoKUgpSCgoKC3IKC6tKCppSUgoKCgoKCuIKCmIKCgpSmgoKCvMKCgpSUgoKCgqaUgpSCgpS4koKCgoKCgriUkpKCtpSClIKUgqaospiCgoKUpoKCgqiSAAIQ0oKUgoIABlzSgpSCgpSCAApIggAEMMKmggALFoKCgoKCgtzygtyCAAgSgoKCgqa4ggAJFJSCgoKC3IKClIK4gqaSppSUlKaCgoKUlIKClLiIsoKUlIKCgoKClIKUlKq0gsqCypSClIKClJSCgpSCpurCgoKCgoKCgoKCuJSmgoKClKa4pu7igqaUgpSUgoK4uIKCppSCgoKCgoKUlIKCgpSCgoKC3IKClIKUlIKUgoKCgoKCAAYWAAgCgpSCgpSCgoK4goKUpqaCgpSmgqaCgoK4goKClIKmgpSC7oKCuIKClJSCgoKCgoKCgsqmgoLKgoKCgoKCqJKCgoKCgpSCgoKCAAIU8pSUlIKClIKCuIKClIKCggAaNoKClIKUAAkUgoKCgsqCgoKmgoKClJQAAhL0poKCgr7k7rgACBIACAKUpoKmpoKCgpSCpoKCqJKCgoKUAAgIkpSClIKCkriUgoKCpoKCgoKUABAMsu6C7oKuwoKmgoKUgpKCpoKCgpSClriCgraCgriCgqaCpoKUgqaCyoKCgoLKgoK4ggAPJLKCgoKUgoKClIKCgoKUgpQAECKCgoKCgoKCgqaCgpSCgpSCgpSCgpSCgqampqaCpoKCgpIABxCCgoK4goKUlLiClKaCgoKCgoKUgoKUAA4egqa4AAgSgqaUgtyCgoKCgsq4AAgSpoKUuIKCgoLcgoLKgoKmuIKClIIACzQAEAKCAAwagpSUgpSUypSUlJSUgpSuwpSCgqqi7LKClIKCgoKCgpSCqJIAAhDSgoKClIKmAAIaAAoCgoKUgoKUlKbcpoKCpsqCgoKmgsKC6oKCgoKCgoKUlLiCgpSCgsqUgoKClKaCgqaCgoKClJSCgpSUgoKUlKaCooKUlIKClJSUAAweADh2sgALGIKm5oKCgoKCgpSCpqaCgoKCgu6CgoKUuJSmgoKCgoKCuIKClNyCgoKCgoKCAAMS0oKCgpSCgoKCgoKmAAMQwpSClAACENKUgoIAAhDSlIKClAACFAAJApSUpoKUAAIQ0pQAAhDSlAACEuKUgqqilIKmqtKUgqaUgpSCgpTekoKCqJKCgqiSgoKokoKCpq7ClIKUlIIACBwADQKUgpSClIKUgoKCgoKUAAIU8pSClIKUgoKCgpQAAhoACgKUgriCpgAPSAAIAgAIEpSCpoKCgoKUgqamgoKCgoKClIKm6sKmgoKClIKUgpSUgoKCyoKmpoKCgqzigpSCgoKCgoKCpoKGgoL4lK70uIKmgoKCgpSCgs7SgoKmgoKCgoKUgqaCAAkUgoKUrvKCgoKCgoKUgoKCABMogsrKgpSCppSClIKClIIACBDSgoKClIKCgpSCgpSCAAoYkqiygoKClKiygoKUlIKqooKUgoKUlIKqooKCgoKUlKiSgoIACRaSqLKCgqiSgoKCupKCgoKUqLKCgoKUlIKCAAIeAAwCgpSCAAIeAAwCgqqiqqKqoqqirgAJDIKUgpSqogACIgAOAgACHgAMAgAVKoKCgoKCyoLugqaCgqaCpoKClAAWLIKCAAkIopSktJSqgpSmlKaCgoKCpoKUlIKUgoKCgoKCpg==", "174-176:1;207-211:2;353-353:1;482-484:1;597-604:1;1279-1284:1;1297-1301:2;1317-1319:3;2112-2128:1;2310-2316:1;5277-5282:1;5298-5307:1;5534-5543:1;5547-5552:2;5981-5990:1;6302-6304:1;6309-6316:2;6407-6423:1;6977-6991:1;7580-7582:1", "", "", "", "225:44,226:13")]
[assembly: GoPositionMap("runtime/profbuf.go", "profbuf.cs", "AHPwAYKmgqaCpoKmggACEOSokqiSrOKCgoKCgoKmgpSClKqigriUgoLcgpSCAAgOooLugpSClIKUlIKCgoKCqsKCpoK4goKCppQAAhDygqaCuKaCgqaClIKmgqaClAAI6AGigpSCrLKCgsqCgpSClAARHMKCpu6CgoKCgoKCpqaCgoKCpoKUpoKCgoKClJSUyoLKlJSCpoKClJKClJSClIKCgriCgpSSgu6CgoKClIK4lAAJFKY=")]
[assembly: GoPositionMap("runtime/proflabel.go", "proflabel.cs", "AAwoABsmgpQAAhgACQI=")]
[assembly: GoPositionMap("runtime/race0.go", "race0.cs", "ABEmgIKSgICikoCAooKCgoKCgoKCgoKCgoKCgoKCkoCAopKAgKI=")]
[assembly: GoPositionMap("runtime/rand.go", "rand.cs", "ACpMooKCpoK4gqa4gqaClKaCgoKCgpSUgpSmggACEAAJCoKCgoKCgpSClIK4goKClOiSgoKUgoKClLyigoKUgqyyAAIUAAsKgoLKgoKUgoK6kqiygoKUgoIAAhDkAAIsABMCuIaCggAHEIKCgoKCAAIqABICAAImABEEAAIWAAgCqJKokg==", "", "", "52=allZero/1/2/3,allZero/2/2/3")]
[assembly: GoPositionMap("runtime/rdebug.go", "rdebug.cs", "AAkUsoKCqLKCgoI=")]
[assembly: GoPositionMap("runtime/runtime.go", "runtime.cs", "ABw+soKCggATVAAXBIKCuIKCgoKCuIKmlIKmlIKClKaU3oCWgJaAmKIAChKSgoKCqJKCgoIAChaigoKCgpSCgoKUgqaClKaCgoKClIKUgrqSgoKCgoK6koKCggADEMIADTjsgqYAFi6ClAAJHsA=")]
[assembly: GoPositionMap("runtime/runtime1.go", "runtime1.cs", "AB1MAA0CgoKCgoKCppSUAAUWsqaCgoKmgoKUgoLYuIKCpoKC2IIAFA6CgoKClIKUgoKUgpSClIKClIKUgpSClIIAUAiCAA4eAAAQlIKUgpSClIKUgpSClIKUgpSClIKUgpSClIKUgpSCpoKCgpSCpoKClIKmgoKUgqaCgoKmgoKCpoKClIKmgoKUgqaCgpSCpoKClIKmlIKmggBLnAEAJEqUgoKCAAgSlKaClIKCgsqmlIIADh6C+IKCgpSCqqKUlJSCggAIHgAKAoKClIKClLiCgpSClKaCgpSSgpSC3IKCpoKCgoKCggAHEIK6koKUpKSkpLSCgoKUtIKCyoKmlKyygoKokoKClAADJgASAoKCgoKClAAEHAALAgACIgAOAgACHAALAqyyrLKssoKCgoKUgoKCgoKUgqiSqJI=")]
[assembly: GoPositionMap("runtime/runtime_boring.go", "runtime_boring.cs", "AAkUpIKU")]
[assembly: GoPositionMap("runtime/runtime_noclearenv.go", "runtime_noclearenv.cs", "AAoYtoI=")]
[assembly: GoPositionMap("runtime/rwmutex.go", "rwmutex.cs", "ABx4AB4ClILM+oKklIKClIK4goKCgoIACQ6ygoKmlIKCgpSmqKSClJSClJKCgoKm+sSCgqaCgoKCgoK4gpQ=", "80-96:1;131-136:1")]
[assembly: GoPositionMap("runtime/secret_asm.go", "secret_asm.cs", "AAgS")]
[assembly: GoPositionMap("runtime/secret_nosecret.go", "secret_nosecret.cs", "AAocoJampqSWgJQ=")]
[assembly: GoPositionMap("runtime/security_nonunix.go", "security_nonunix.cs", "AAgSgqY=")]
[assembly: GoPositionMap("runtime/select.go", "select.cs", "ABk0kpaCpoKCgoKCgsoACBKCgoKUuPy4AAYWwoLulJSClKaCAAwkAA8CgoLKgpSCgoKssoKClJKClKaCggALGIKCgqaCgqaCgqamgqaCgoKUgpSCpsqClIKCgoKUlIKCgoKCgoKClIKUgoKClJSmgoKCgtyUAAcWkoKCgoKCgoKUgoKClIKUgqaClIKUgoKUgsqCgoK4gpSCgoKCgoKCpoKCgpSUgpSClKaCysqCgpSUgoLcgoKClIKCgpSUgoKClJSCgoKCpoKClKaCgoKmgqaUgqaCgqamgoKSgraCgoKCpoKCgoK4gpSEkoKklIKUgpSCgoKUgoKClIKClISSgqSClIKUkoKClIKClISwkoKUgpSEkoKClIKUlISSpIKUgpSgkoKUlIKClJSEkqaCABMukoKUgoKSgoKClIK0grSCtoK4griCgqaCgoKCgpSmpoKUlKaigoKClIKCgoKmgoKClJSCgoLKgoI=", "147-152:1;497-497:2;527-527:3")]
[assembly: GoPositionMap("runtime/sema.go", "sema.cs", "ADtwggACGgAKAqiSAAIaAAoCqJKokqiSqJKCgpSokqiSpoKClAAHFpIABHiCAASuAYKCgoKUgszCgqaCgoKUgoKClJSCgoKCgoKClIKmgoKClIKClIKCgqaClJSCgoKmlIKClAAOHoKCpoKClIKUAAQYAAsCgpSCgqaClKaUgoKClJSCgoKCgpSCgpSClJSCgsqCgoKCpoKClLiCgpSmlKaCgoKCgoLq1IKClIKCgoKmgoKCgpSClPzUgoKUgoKCgqaCgoKClIKUABg6ogACEPau4qaCgriCpoKCgoKCgoKUgpSUgoKCuIKC7PaCyoKCgtyCpoKCgoKClIK+9oKmpoKCgrgADh6CgoKClJSClIKCgoKUgqbokoKCupI=")]
[assembly: GoPositionMap("runtime/set_vma_name_stub.go", "set_vma_name_stub.cs", "AAoYpA==")]
[assembly: GoPositionMap("runtime/signal_windows.go", "signal_windows.cs", "AA4agoKcwoIABBqSgoKClIKSpqSSAAQQ4oKmlAACFAAMCIK4lAAiGgAGHOSCgpQABhYACgKCgqaClLS0tAADIgAOAoKippSCAA0clJSCgpIAAhYADAKCpsoACBKCgoIADh6CppQAAhDygpQACxiCgpKCkoKUkoKmAAIWAAgCgpQAAhIACQK4AAgShKaCrPKUgpTKgoKUpIKCgpSUlIKUgoKCgqaCpgAIBoKCgqaUgpSClLiUlLS0vKSqtqampoKokq7igoKmyoK4yg==", "164-166:1", "", "372=PC/1/2/1,PC/2/2/1")]
[assembly: GoPositionMap("runtime/signal_windows_amd64.go", "signal_windows_amd64.cs", "AAoSgoKmgoKCgoKCgoKCgoKCgoKCgoKCgoKC")]
[assembly: GoPositionMap("runtime/sigqueue.go", "sigqueue.cs", "AEWOAbKCgqamgoK4goKCgpSCyoKClLSC2LSCgoKUgvqC7sKUgoKCyoKClLSCgoKUgoLGggAIDIIABBwADgyC3IK+spSCgpS4gqaCgpSCgpSssoKUlIKCrLKClJSCgpSCggACENKCgqyygg==")]
[assembly: GoPositionMap("runtime/sigqueue_note.go", "sigqueue_note.cs", "ABAcguaC5oI=")]
[assembly: GoPositionMap("runtime/slice.go", "slice.cs", "ACE6guaCqsKSgoKCgpTKgqaCgoKCuILuuIKCgpSClIKmlAACGAALAoLcgoKUpqaCgoKmgoKmAAZeAC4CgoKCpIKUgqaCpqamlILqgpSCgoKCtIKCgoK0gpSUlIKCgoKCtIKCgoKCABAggqaCgsqmgu6mlAACIgAQAoKCAAsYgqaokoKCgqaCgpS43ILcgpQAAhgADQaCyoKCgpSCpoKokoKmgoKmgqaCgoKCgpSCgpSCgqaUlJSokoKUgqrCgoKUlIKCgoKqooKClJSCgoKs0oKClJSCgoKCgoKokoKClJSCgoKCgpSqwpSUgqYACxj+gqaqwoIACBKClA==")]
[assembly: GoPositionMap("runtime/softfloat64.go", "softfloat64.cs", "ABo64oKClJSCgpSCuIKCgoLsgqSm4oKClJSCgpSCuIKCgoLsgqSmgqKClIKClIKCgpSCgoKCgqaClIKUgoKmgoKCgpSClIKCgqamgqKClIKClIKCgpSCgoKCgqaClIKUgoKmgoKCgpSClIKCgqamgoKmlKampqamgpSmqIKmgoKCgoKClIKCpoKUpoKmgqaCgqaUpqampriCgoKCpoKCppSmpqamlNaCgqaCgoKUgoKUgqaCgoKClIKClKaCgpSUpqam3Ka4pqKUlKamgpSClKaCgpSCgpSCgpSmgoKCgpSUgoKCgsqCgoKCgqaqopiCgoKCgoKCgoKqopSCuIKCgqaCgoKCgoKClIKCgoKCuIKClIKCgoKCuKaCpoKmlKaCgqaCgqaCgqaCgqaCgqaCgqaCpoKmgqaCpoKCpoKCpoKCpoKCpoKCgpSCgqaCgoKUgoKmgoKmgoKCgqaCgqaCgoKC")]
[assembly: GoPositionMap("runtime/stack.go", "stack.cs", "AKsB1AKCgpSCgpSCgrqSgpQACgqigoKClIKClIKUgpSCgoKCypSClJSCgpSCgpSU6JKCgpSUlIKCggAPIIKCggADEMKCqqKCgoKCgoKUgoKokoKUgoKCgoKCgpSCgqiSgpSCgoKCgoKUgoIADRQACQiCgpSClIKmgoKCgpSssoKCgoKClILKgoKUgoKCgpSClLiUlIKCpoKCgpSUlJSCgpSClKaCgoKCpoKUgpSClIKUgpQAChLigoKCgpSClIKClIKClJSUgoKCgqaClIKUgpSCgoKCgpSCgoKmlIKUgoKUuJSCgqaCgoKUpoLugoKC7pQAK2CigoKClAAHEJSCgoIACyDCgurCgoKC3IKCgoLmgoKCgoKCgqaCgpSCgpSCgoKmAAoQxIKClKaCgoKC7pQAGDKmuJSCgqbGgoK4goKUyoKCgoKCgpSCuJSCgoIACg6igoKUgoKCgoKmgoK4goLK2IKCgoK4pqamgoK4goK4goKCgoKmrLKCqJKCAAkUlLiasoKCgoK4goKClKYACgrCgpSCgpTKpoKClIKokoKmgoLKlAAIEri4yoKCgoK4goKClpKCpoKClMrclJSClKiSgoKUABYeAAwClIKUgoKCgqaC3KaUgoKCkoKCgqTKgoKmgoKCgsoADRyCgqaCuIKUgpSUgriCkoKmgoKUgqamgriUgri4gsqAgoKCgoLIuKaCgpSUgsq4goKUgqiSqsKCgpSUrgAKCoLKgsqCAAwagpQABHakgoKCgoKCgoKClJS4goKCgoKCgqYADiTSgoKUgoKmgoLcgu7CAAgSgoIAARgACgKCgoKUgpSCgpSmgpSCpg==")]
[assembly: GoPositionMap("runtime/stkframe.go", "stkframe.cs", "AESQAZKCuIIABh4ADwKCgoKm2pSCppQACxiSopSUgriCkoKUgqaCgsgACArigpSmgoLKgpS4uIKClLSkgoKCgoKmgpSSlIKClIK6koKmgoKClJSSlIKUysyUgoKCgoIACBIACQqCoqaCgoKCgqaClA==", "", "", "312=alignUp/1/1/1,getGCMask/1/1/4")]
[assembly: GoPositionMap("runtime/string.go", "string.cs", "ABw40oKCgoKCgpSClIKClILcgpSCgoKUrLKmgqaCpoKs0oKCgoKUlJSmgoKClJSCgoKmrLKmgqaCpoIAAhIACAK4lILKgpSClIKCgpSmgoKUlIKqooKCpsKCgpSUAAIiAA4CgsqClIKUpqKCgoKUlIKmxoKCpoKCgpSmgoKClKaCgsqClIKUgoKClIKClIKUlAANHIKmsoKCgpSUgpSCAAQasoKCgqaCqLKClIKCgqaC6LKCpoKmgpSCrLKCgpSCgqyypoKClIKCAAIgAA4EgqaCgoKClMqCppSCgpSCgriClIKUtLS0xpSCgpSCgpSClJSClJSokoLcgoKCgpSu1IK4lIKUgqaCgriCgpSCgoKUqJKCgqaCgoKCgpSCgpSClJSC", "", "", "157=GetCallerPC/1/1/2,FuncPCABIInternal/1/1/3;224=GetCallerPC/1/1/2,FuncPCABIInternal/1/1/3;280=GetCallerPC/1/1/2,FuncPCABIInternal/1/1/3")]
[assembly: GoPositionMap("runtime/stubs.go", "stubs.cs", "AAswAAwCrAAEIAAPKAAXCKIAAkgAIgaSAAJCAB8GkgAFJAANLAAUAoKuwoIAAhgACgSUgoIAAVwALRwADAKClKgABRwACyIAEAakAAEYAAsEmrKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKClJqyrLKs1qikltYAAhDmAAIWAAoEgoKCgoKCgpas")]
[assembly: GoPositionMap("runtime/stubs3.go", "stubs3.cs", "AAgU")]
[assembly: GoPositionMap("runtime/stubs_amd64.go", "stubs_amd64.cs", "AAkUkoKCgoKClqamsoKCgoKCgoKCgoKCgoKWpqzCmA==")]
[assembly: GoPositionMap("runtime/stubs_nonlinux.go", "stubs_nonlinux.cs", "AAgUkg==")]
[assembly: GoPositionMap("runtime/stubs_nonwasm.go", "stubs_nonwasm.cs", "AAgUgA==")]
[assembly: GoPositionMap("runtime/symtab.go", "symtab.cs", "AEWgAbKCggAEhgIACwIAAiAADQKClIKCAAIgAA8GgpSClIKUpoKU3KaUgoKmlKYAAhDSgpSUpoKCAAkUgpTugpQADRyCpoKmuIKCgoKUgoKCpgB8pAIACgKCgpQAAioAEgKCgoKUgoKCgoKCAAwagoKCgrgAGzqCggARIAANBIKElLiClIKCgoKClJKCpIKUuIKCgoKmlIKCpoKCggAILgASAoKCgpSCgqaCgqamlAAGEuKCppSCgoKUgpSUlIKUgoK4qJKClAAVsAGigoKClAACEuKCgqYABxCCpoKokgACGAAJAqgAAiIADwKCgpSUgoKmgqaUgpSCpoKmggAMGoKClAACFPKClKgADyjCAAoIlrakgqyygoKCgsqCgtyCgoKCgpSCgrimlIKmgoKClJSCgoKCgoKCgoKU7oKCgqaCgoKCgoKC7oKmlMqCppSCgoKCgoKClKaCpoKClKaCgoKCgqaCgqamgoKCpoKmAAIYAAwCgoKUgoKUlIKmguaCgoKClKiSgoKCgoKCgoKClLiCpoKClIKmgoKUgqiSgpSqooKUgoKGooKUgoKotoKClIKClIKUgoKClIKCqJKigoKCgoKUlAAMFOiClA==", "", "", "189=gostring/1/2/3,gostring/2/2/4")]
[assembly: GoPositionMap("runtime/symtabinl.go", "symtabinl.cs", "ADKGAQAQAoKClIKCtoIABxCCqJKCgpSCqJIAAhwACwKClIIABxIAAhAACAKC", "", "", "79=pcdatavalue1/1/1/4")]
[assembly: GoPositionMap("runtime/synctest.go", "synctest.cs", "AC1WAA0SgoKUgrSCtoKUgoKCxoLagqaCgoKClIKmuIKUgpSCgoK8woKC6LKCgoKUgoKCurKCAAoWgoKUlJS4puoADwzSgqaCgpTcgoKClIKSuIKigpKCgqaCgoKCxoKCgpSCgoKUgoKUgpSUlKaCgqakgoKClJSUprT+gqaigoKUlJSCqJKCgpS4goKUgoKUgoKClIKCuIL4ooKUlIKCqJKokoKClKiSqLKCgpSCkpSiAA4wAAoCgqbcgpSUpoKmgoKUlKaCgoKCgoKCgoKmpoKUrsKssqyy", "192-194:1;198-204:2;210-217:3;353-355:1", "", "221=Add/1/1/1", "", "234:51,235:13,415:57,416:13")]
[assembly: GoPositionMap("runtime/sys_nonppc64x.go", "sys_nonppc64x.cs", "AAgS")]
[assembly: GoPositionMap("runtime/sys_x86.go", "sys_x86.cs", "AAwgooKCgoKC")]
[assembly: GoPositionMap("runtime/syscall_windows.go", "syscall_windows.cs", "ABksgsqCuIKClAAZOIKClIKClAASKqLulNymlIIAChaCyoK4ptzugqbKAAISAAgCtraCxoKCxoKCgoKmuAACENKClNyCAAcQAAEWAAkCgqS02KQACBoACAKUpoKUlpKCuIKUgpSClLiUuMqCgoKqooKmlKaCgriClIKCgpSCgoKUggARJpKClpKCgoKUtIK07IK47oKUuAADGAAKAoKUgtyCgoKClLiC")]
[assembly: GoPositionMap("runtime/tagptr_64bit.go", "tagptr_64bit.cs", "AD1+ooKCgpSokqaUgpSokg==")]
[assembly: GoPositionMap("runtime/test_amd64.go", "test_amd64.cs", "/g==")]
[assembly: GoPositionMap("runtime/time.go", "time.cs", "AA8iwoKC7pSokoKClKiSAEaiAeKCggAhUpKUpNq4gqYACRCUABo6goK4ooKUgoKCpoKUpoKC+pKC6LKmgqrCgsoABhQACQKCgoKClIKUgoKCppSCgoKCpgAJJPKCpoKCgoKCgpSUgoKUlIKClIK4gpQAAxDiggARIOKCgoKClIKCgoKCpoKUgoLu4oKUAAYQ8oKUgpSssgAGEAAIAqaCpoKUgoKCggADEvKCAAcQgIKClAAJEuKCgqaCgoKUgoKCgqaClKbugqaCgoKCuOrCgoKClIKCgpSCgoKUgpQADRDigpSClJSCpoKClIKCgoKCgqaCgoKCgqaCuIKmgriUAAoWgoKUgpKClJS4pu6CpoKCgpSmgpSCpgAICsKCgoKUlAAIKgAdFIKCgoKClJSUgoKCgoKCgriUgoKCgpSCgoKCvrIACA7igoKCgoLugu6CgoKCgoKCgpSCpoKCppSmgoKClAAIFAAIAoKCuIKCgoKUgqaCgoKCggAHEvKC3IKCgoKUADVsgpSCgoKCgqaUpoKUtoKCgoKCgoKCtoKCpKaClJSCAAMUAAsMgoKClAAGGgAOAqaClKaC3IKClJSUpoKCgpSCgpSUAAcQgoKmlAAGFgAKAoKCgoKUgoKCppSmgoKCpoKmlIKmgoIADhIACQKCgoKUuIKCgpSUgpSmgqaCgoKCgpSCgqaUgoKCgoKClKaClIK4lJSCgpSClLiCppSCgpSCpgASJpS4griCyJSCpoKCgqaUpoKmgoIACQ7SgoKUuIKCgqaCgrzCgoKUvKKCgoKUggAEELKmgoKmpoKmlAAEHgAKAoKClIKClIKCgpSClIK8ooKCgpSClIKClIKCgpSCgoKCgqaClIKUgrzGgpSCAAcQwgAKEAAIApSUpqaCgoKUgpSClIKCAAcQwoKUpoKCgqbKgoIACBKCgoIABxTigoKCgpSCuIKU", "485-487:1;638-643:1;1239-1239:1;1465-1467:1")]
[assembly: GoPositionMap("runtime/time_nofake.go", "time_nofake.cs", "ABBAAA8CABAosoKU")]
[assembly: GoPositionMap("runtime/timeasm.go", "timeasm.cs", "AAsc")]
[assembly: GoPositionMap("runtime/tls_windows_amd64.go", "tls_windows_amd64.cs", "AAcU")]
[assembly: GoPositionMap("runtime/trace.go", "trace.cs", "AMECrAcACQLKgpSCmAAAFO6UgqaCgqYAChaCgoKClIKCgpSClIK4lMoACBLcgoKClAAJFILcpoKCyoKCgrimAAkUgoKCgu6CgoLuggASJoKCgpSCgpQAARDilIKUgoKClIKC3IKCgoKmpoKCpoKmpoKCgoKmAAkUgoKU3IKUpu6CgoKmgoKCppSm7oKCyoKCyoKCgpSCgpSCpoKCgqaClKYACxiCgoKCgpQABxCUuIK4griUrLKClIKUggAJWgAIApSClKaA7JS4goKmgoKCpoKCgsqClIKCgriC7pSCgoK4poLcuJS47oLclJSmggAKFoKCpoKCggAGEuKCgpSsAA8agpQACxjEgpKClKaUupKCgoIAEiqSgoKCgoKUggACEPKSgoKUgoKUgoKs9oKClNwACAqClAACFAAKBIKClIKmlIKUgg==", "494-540:1;508-536:1.1;564-570:2;591-604:3;675-725:4;761-765:5;793-816:6;931-931:1;1102-1111:1;1145-1147:1", "", "", "", "754:54,755:17")]
[assembly: GoPositionMap("runtime/traceallocfree.go", "traceallocfree.cs", "ABg0wpiigoKUuIKCgqamgoK4goKmpoK4goKCgqTKkpSmooKUqLLYstiSqLKssqyyqJKokqiSgqiSgqiSqJLokoKU", "79-81:1")]
[assembly: GoPositionMap("runtime/traceback.go", "traceback.cs", "AG/2AQAKDgAIBsQADRymgoKCgoKmgoKCyoKCgoLKgoKClIIABxDcgqaCgoKClIKUgpSmAAgQgqaCAAYwABcCgpSCpoK4gsqUuMrcgu6CgoKCgoKCgtgABxCClIKCgoK2kpTKlIIAGTSCgpSUgoKCgqaCgsqClAATKIKmAAoWgoKCAAoWyoIACAaigoKmgoKUgsqCgsqUgpKUgpSCgqaUgoKmgoKUuIKCgoKCuIKUkoKCgoKCuOyyAClUgoKCggADHAAKApSmrsKmpoKCgoKCpgACGAALAoKCgoKmgoKUggAHEIKUuIKmqJKCgqaCgoKCpoKClIKUgqaSlIKCgpSmgoK4goKCpoKCgoKCgpS0goKCtLSCtIK0goKCgoK23sSCgpSCgpSClKiSgqqigoKUgqa0goKCuIKCgoKUgoKClIKCgpSmggACFgAKApSSlKa07oKCgpSmlIKClKaCggAuXoKCgoKCgpTKgoKCgoKCppS4gpSUgpSCAAMSAAsGgpSUgoKUgqaCpoKCgoKCgoKCgoKCpoKCgqaCyoKCgpSClIKCgpKkgqa4goKCgoKCgoKCpoKCgriUgpSCuKqigoKCgqaCpoKmAAMQwoKCgoKCgpQABDSCgoKqooKClKqigpSmggAOHoKm7oKmqrSCgpS4goKUgoKUgsqqxqYAEBqilJSClpKClLiCqJKClIKCgoKUpoKClIKUgKykgpSClICCpIKCgoKCgoKmpqaCpoKmgoKCggAJFJKClIKUgpSClIIADRyCgpQABBDSgpSUgpSCppSClIKmgpSCuIKCgoKUgoKUgoIABBwADASCgpSClKamlJSmppSUAAjIAgChAQKCpoamgoKUggAEFAAJEKampgAaOJKCgoKUlKaAkoKCgpSUggACEPKCgoKCgqaCuJSUgoKUgoIABBCygqaUgpSClLyygqaUyoKUgpQ=", "675-684:1;686-701:2;704-708:3;906-929:1;953-968:1;1296-1296:1;1317-1351:1;1388-1401:1;1688-1688:1", "", "218=guintptr/1/1/2;1816=noescape/1/1/2")]
[assembly: GoPositionMap("runtime/tracebuf.go", "tracebuf.cs", "ACBWAAsClIKCgqYAAhgACQIAAhIADAqmgoKUgqaCgoKUAAIS4qaUgqaCggAEGgAIAoKClAACEuKCgoKUlIIACAiSgoKClIKCgpSCgoLKgoKUgoKmgoK4gpSClIKCgpKqogACHAALAgAHErKCgpSUqJKCgpSCgpSCpoIAEzTiggACEuKCgoKCgoKUgpQAAhYACAKCggACEuKuwgAGGAAJAoKClJSClIIAAxLyAAkUgriC", "144-150:1;157-173:1")]
[assembly: GoPositionMap("runtime/tracecpu.go", "tracecpu.cs", "ABIcsoK4ggAHEILuwoKmgoKSAAoWlIKUlIKCgqaU7LKCAAkUgoKCgqamgoKCAAIgAA0ClIKCgpSClIK4goKCgpSCgrimypSmuIKCgriWkoKUuKaCgoKCgpSUqrSAgpKCgoIABRDyppSmAAcQgoKClIKmgpSmisKmlJSClKaUprimpoI=", "44-70:1;201-206:1")]
[assembly: GoPositionMap("runtime/traceevent.go", "traceevent.cs", "ABxQAA0CgpSClKyyqLKssgACEOQABRKyrLKokg==", "", "", "80=startPCForTrace/1/1/2")]
[assembly: GoPositionMap("runtime/tracemap.go", "tracemap.cs", "ADyAAaIAAhLigpSUgoKCgoIADyCClILKlIKCpoIABTLCgoI=")]
[assembly: GoPositionMap("runtime/traceregion.go", "traceregion.cs", "ADJWkoKClIK4goKCgrqSpriCgoKCgoK4griCgsqCpoKUruKCgoKClIKClA==", "63-98:1")]
[assembly: GoPositionMap("runtime/traceruntime.go", "traceruntime.cs", "ABMqkgAdRraCgoKCgoKCggACEuIAHUAAHUAABRKyrsKokgARLtKClAACFAAKCKbKgoIACBIACRSCgoKClIIAAhDSAAIQ0oKUlKyyqJKokqyyuMi2rsKmrsLGrsKmqLaotgAGFAAJBoKClAACFgAIAoKCgoKUAAcS0oKClIKClKiSqJKosoKCgpTcsoKCgpKCzrKokqiSqJLOwsjEgtrUgoKCgpS6wri+xIKCAAIYAAkCgoKCgpSUrOSC7qYACBKCgoKClMiSqJKmlJQAAhIACgbKrtSC3AACEuKClJSCrLKClJSCAAIS4oKUlIKUtLSUgqyygpSUggAGEuIACBKClIKCgoKCpoK4lA==", "699-712:1")]
[assembly: GoPositionMap("runtime/tracestack.go", "tracestack.cs", "ABtAAAoClpKCgriClMq0gpS0gsqmlILugoKCgriCgoIAECKCgpSC7oKUlJTKgpSClIIACRjGgpSCrLKCgoKUgqaiuIK4ntKCgriCgoKCgoKCuIKCgpSUqqKCgoKCgoIACxqSgpSCgoKUgoKCgpSCqqKu4pSUlKiSAAIYAAkCgoKmpv6CgoKClKiCgoKCpoKUpoKCgpSClMassoKClIKClA==", "306-314:1")]
[assembly: GoPositionMap("runtime/tracestatus.go", "tracestatus.cs", "ABEo9IKCuIKUuIKUAAYYAAsCgpSClIKmxtyCxqSCAAYYAAoEgoK4poKUAAgWAAgEkpS0tAAKEoKCxrSkABY84oKUgqiSgoKokqqiqJKC")]
[assembly: GoPositionMap("runtime/tracestring.go", "tracestring.cs", "ABo2xIKClIKmqJKCpoKClAACEAAIBIKmgpaSgpS4goKCpoKu4oKCgoKUuA==", "33-35:1;47-49:1;92-96:1")]
[assembly: GoPositionMap("runtime/tracetime.go", "tracetime.cs", "AC50AAsCgpQABCKCpqamgqaCgoKUgoKCgoKCppKCgg==", "107-111:1")]
[assembly: GoPositionMap("runtime/tracetype.go", "tracetype.cs", "ABc04oKmkqyygoKUgqaigqae0oKUuIKCgoKCpoKCgpSU")]
[assembly: GoPositionMap("runtime/type.go", "type.cs", "AB0ikgAJLIKmgoKUgoKCgpS0pJSuwoKUlIKkgpQAAhDylJQABxQACw6UgqaCgpTagsaCuIKCgriCAAsc1LiCppSCgoKUgoKCpoKmgoKCgpSCpoIACAoADBKCgpSUgriUgqaClIKCgsaCgoKCgpS4gpSUlIKCxgAaMIKCgriCgpQACAaCgpSCgoKCgoKUyoKCgoKCgpSUpoIACAaCppSCgoKCgqaCgoKCgoKClJSUgpSCgoKUpoLmgqaUgoKCgoKmgoKCgoKCgpSUlIIAAiqCgpSCgoKClIKCgqqigpSUgoKUgoKCgpSmgoKCpsa4goKCgoKCgoKCprgACBKCAAYeAA8CgoLclIKUgoKUgoKUgoKCgpSCgoKmgpSUpIKCpIKCpIKCgpSCgoKmgoKCpqSCgoKUgpSCgqaCgoKUgpSCgoKmpIKCpIKCpIKCpIKCgpSClIKCgoKUgpSClIKUgqakgoI=", "135-137:1")]
[assembly: GoPositionMap("runtime/unsafe.go", "unsafe.cs", "AAwagoKmgoKUupKCgpTmgriC+ILmgqiSgqaCgriCgoKUupKCgpTmgriCuKbokoKmpuiSgqiS")]
[assembly: GoPositionMap("runtime/utf8.go", "utf8.cs", "ACRWkoKClAACGAAMApSCppSmgoKCguqCgoKC6oKCgoLaqrSkgqSCgoKkgqSCgoKCpIKCgoKC")]
[assembly: GoPositionMap("runtime/valgrind0.go", "valgrind0.cs", "AA0egoKCgoKCgoKCgg==")]
[assembly: GoPositionMap("runtime/vdso_in_none.go", "vdso_in_none.cs", "AAgWkg==")]
[assembly: GoPositionMap("runtime/vgetrandom_unsupported.go", "vgetrandom_unsupported.cs", "AAoYkqaU")]
[assembly: GoPositionMap("runtime/write_err.go", "write_err.cs", "AAgUkoI=")]
// </GoSourcePositionMaps>

namespace go;

using static go.runtime_package;
using abi = go.@internal.abi_package;
using atomic = go.@internal.runtime.atomic_package;
using tracev2 = go.@internal.trace.tracev2_package;
using ꓸꓸꓸtraceArg = variadic<runtime_package.traceArg>;

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
    internal partial struct abiDesc {}
    internal partial struct abiPart {}
    internal partial struct abiPartKind {}
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
    internal partial struct callbackArgs {}
    [GoValueClone("ctxt")] internal partial struct cbsᴛ1 {}
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
    internal partial struct errorAddressString {}
    internal partial struct errorString {}
    internal partial struct finalStatsAggregate {}
    internal partial struct find_firstFree {}
    [GoValueClone("subbuckets")] internal partial struct findfuncbucket {}
    internal partial struct fixalloc {}
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
    [GoLocalName("_DEVICE_NOTIFY_SUBSCRIBE_PARAMETERS")] internal partial struct monitorSuspendResume__DEVICE_NOTIFY_SUBSCRIBE_PARAMETERS {}
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
    internal partial struct overlappedEntry {}
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
    [GoValueClone("ensured", "heapStats")] internal partial struct statAggregate {}
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
    internal partial struct uint16InterfacePtr {}
    internal partial struct uint32InterfacePtr {}
    internal partial struct uint64InterfacePtr {}
    internal partial struct unwindFlags {}
    internal partial struct unwinder {}
    internal partial struct updateMaxProcsGState {}
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
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸruntimeꓸgc() => builtin.initPackage(typeof(@internal.runtime.gc_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸruntimeꓸgcꓸscan() => builtin.initPackage(typeof(@internal.runtime.gc.scan_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸruntimeꓸmaps() => builtin.initPackage(typeof(@internal.runtime.maps_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸruntimeꓸsys() => builtin.initPackage(typeof(@internal.runtime.sys_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸruntimeꓸsyscallꓸwindows() => builtin.initPackage(typeof(@internal.runtime.syscall.windows_package));
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
[GoMethodHost(typeof(runtime_package.Cleanup)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛCleanup;
[GoMethodHost(typeof(runtime_package.Frames)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛFrames;
[GoMethodHost(typeof(runtime_package.Func)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛFunc;
[GoMethodHost(typeof(runtime_package.MemProfileRecord)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛMemProfileRecord;
[GoMethodHost(typeof(runtime_package.PanicNilError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛPanicNilError;
[GoMethodHost(typeof(runtime_package.Pinner)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛPinner;
[GoMethodHost(typeof(runtime_package.StackRecord)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛStackRecord;
[GoMethodHost(typeof(runtime_package.TypeAssertionError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛTypeAssertionError;
[GoMethodHost(typeof(runtime_package._func)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛ_func;
[GoMethodHost(typeof(runtime_package._panic)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛ_panic;
[GoMethodHost(typeof(runtime_package.abiDesc)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛabiDesc;
[GoMethodHost(typeof(runtime_package.abiPart)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛabiPart;
[GoMethodHost(typeof(runtime_package.activeSweep)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛactiveSweep;
[GoMethodHost(typeof(runtime_package.addrRange)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛaddrRange;
[GoMethodHost(typeof(runtime_package.addrRanges)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛaddrRanges;
[GoMethodHost(typeof(runtime_package.arenaIdx)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛarenaIdx;
[GoMethodHost(typeof(runtime_package.atomicHeadTailIndex)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛatomicHeadTailIndex;
[GoMethodHost(typeof(runtime_package.atomicMSpanPointer)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛatomicMSpanPointer;
[GoMethodHost(typeof(runtime_package.atomicOffAddr)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛatomicOffAddr;
[GoMethodHost(typeof(runtime_package.atomicScavChunkData)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛatomicScavChunkData;
[GoMethodHost(typeof(runtime_package.atomicSpanSetSpinePointer)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛatomicSpanSetSpinePointer;
[GoMethodHost(typeof(runtime_package.bitCursor)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛbitCursor;
[GoMethodHost(typeof(runtime_package.bitvector)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛbitvector;
[GoMethodHost(typeof(runtime_package.boundsError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛboundsError;
[GoMethodHost(typeof(runtime_package.bucket)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛbucket;
[GoMethodHost(typeof(runtime_package.chunkIdx)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛchunkIdx;
[GoMethodHost(typeof(runtime_package.cleanupBlock)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛcleanupBlock;
[GoMethodHost(typeof(runtime_package.cleanupQueue)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛcleanupQueue;
[GoMethodHost(typeof(runtime_package.consistentHeapStats)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛconsistentHeapStats;
[GoMethodHost(typeof(runtime_package.cpuProfile)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛcpuProfile;
[GoMethodHost(typeof(runtime_package.cpuStats)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛcpuStats;
[GoMethodHost(typeof(runtime_package.cpuStatsAggregate)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛcpuStatsAggregate;
[GoMethodHost(typeof(runtime_package.debugLogReader)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛdebugLogReader;
[GoMethodHost(typeof(runtime_package.debugLogWriter)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛdebugLogWriter;
[GoMethodHost(typeof(runtime_package.dloggerFake)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛdloggerFake;
[GoMethodHost(typeof(runtime_package.dloggerImpl)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛdloggerImpl;
[GoMethodHost(typeof(runtime_package.errorAddressString)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛerrorAddressString;
[GoMethodHost(typeof(runtime_package.errorString)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛerrorString;
[GoMethodHost(typeof(runtime_package.finalStatsAggregate)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛfinalStatsAggregate;
[GoMethodHost(typeof(runtime_package.fixalloc)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛfixalloc;
[GoMethodHost(typeof(runtime_package.funcInfo)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛfuncInfo;
[GoMethodHost(typeof(runtime_package.g)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛg;
[GoMethodHost(typeof(runtime_package.gList)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛgList;
[GoMethodHost(typeof(runtime_package.gQueue)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛgQueue;
[GoMethodHost(typeof(runtime_package.gTraceState)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛgTraceState;
[GoMethodHost(typeof(runtime_package.gcBits)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛgcBits;
[GoMethodHost(typeof(runtime_package.gcBitsArena)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛgcBitsArena;
[GoMethodHost(typeof(runtime_package.gcCPULimiterState)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛgcCPULimiterState;
[GoMethodHost(typeof(runtime_package.gcControllerState)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛgcControllerState;
[GoMethodHost(typeof(runtime_package.gcStatsAggregate)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛgcStatsAggregate;
[GoMethodHost(typeof(runtime_package.gcTrigger)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛgcTrigger;
[GoMethodHost(typeof(runtime_package.gcWork)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛgcWork;
[GoMethodHost(typeof(runtime_package.gclinkptr)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛgclinkptr;
[GoMethodHost(typeof(runtime_package.godebugInc)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛgodebugInc;
[GoMethodHost(typeof(runtime_package.goroutineProfileStateHolder)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛgoroutineProfileStateHolder;
[GoMethodHost(typeof(runtime_package.guintptr)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛguintptr;
[GoMethodHost(typeof(runtime_package.hchan)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛhchan;
[GoMethodHost(typeof(runtime_package.headTailIndex)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛheadTailIndex;
[GoMethodHost(typeof(runtime_package.heapStatsAggregate)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛheapStatsAggregate;
[GoMethodHost(typeof(runtime_package.heapStatsDelta)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛheapStatsDelta;
[GoMethodHost(typeof(runtime_package.hexdumpMarker)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛhexdumpMarker;
[GoMethodHost(typeof(runtime_package.hexdumper)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛhexdumper;
[GoMethodHost(typeof(runtime_package.immortalWeakHandle)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛimmortalWeakHandle;
[GoMethodHost(typeof(runtime_package.immortalWeakHandleMap)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛimmortalWeakHandleMap;
[GoMethodHost(typeof(runtime_package.inlineFrame)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛinlineFrame;
[GoMethodHost(typeof(runtime_package.inlineUnwinder)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛinlineUnwinder;
[GoMethodHost(typeof(runtime_package.itabTableType)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛitabTableType;
[GoMethodHost(typeof(runtime_package.lfstack)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛlfstack;
[GoMethodHost(typeof(runtime_package.limiterEvent)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛlimiterEvent;
[GoMethodHost(typeof(runtime_package.limiterEventStamp)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛlimiterEventStamp;
[GoMethodHost(typeof(runtime_package.linearAlloc)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛlinearAlloc;
[GoMethodHost(typeof(runtime_package.listHead)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛlistHead;
[GoMethodHost(typeof(runtime_package.listHeadManual)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛlistHeadManual;
[GoMethodHost(typeof(runtime_package.lockRank)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛlockRank;
[GoMethodHost(typeof(runtime_package.m)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛm;
[GoMethodHost(typeof(runtime_package.mLockProfile)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛmLockProfile;
[GoMethodHost(typeof(runtime_package.mProfCycleHolder)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛmProfCycleHolder;
[GoMethodHost(typeof(runtime_package.mSpanList)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛmSpanList;
[GoMethodHost(typeof(runtime_package.mSpanStateBox)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛmSpanStateBox;
[GoMethodHost(typeof(runtime_package.mWeakPointer)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛmWeakPointer;
[GoMethodHost(typeof(runtime_package.markBits)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛmarkBits;
[GoMethodHost(typeof(runtime_package.maybeTraceableChan)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛmaybeTraceableChan;
[GoMethodHost(typeof(runtime_package.maybeTraceablePtr)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛmaybeTraceablePtr;
[GoMethodHost(typeof(runtime_package.mcache)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛmcache;
[GoMethodHost(typeof(runtime_package.mcentral)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛmcentral;
[GoMethodHost(typeof(runtime_package.memRecordCycle)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛmemRecordCycle;
[GoMethodHost(typeof(runtime_package.metricReader)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛmetricReader;
[GoMethodHost(typeof(runtime_package.metricValue)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛmetricValue;
[GoMethodHost(typeof(runtime_package.mheap)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛmheap;
[GoMethodHost(typeof(runtime_package.moduledata)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛmoduledata;
[GoMethodHost(typeof(runtime_package.mspan)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛmspan;
[GoMethodHost(typeof(runtime_package.muintptr)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛmuintptr;
[GoMethodHost(typeof(runtime_package.notInHeap)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛnotInHeap;
[GoMethodHost(typeof(runtime_package.objptr)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛobjptr;
[GoMethodHost(typeof(runtime_package.offAddr)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛoffAddr;
[GoMethodHost(typeof(runtime_package.p)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛp;
[GoMethodHost(typeof(runtime_package.pMask)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛpMask;
[GoMethodHost(typeof(runtime_package.pageAlloc)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛpageAlloc;
[GoMethodHost(typeof(runtime_package.pageBits)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛpageBits;
[GoMethodHost(typeof(runtime_package.pageCache)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛpageCache;
[GoMethodHost(typeof(runtime_package.pallocBits)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛpallocBits;
[GoMethodHost(typeof(runtime_package.pallocData)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛpallocData;
[GoMethodHost(typeof(runtime_package.pallocSum)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛpallocSum;
[GoMethodHost(typeof(runtime_package.piController)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛpiController;
[GoMethodHost(typeof(runtime_package.pinState)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛpinState;
[GoMethodHost(typeof(runtime_package.pinner)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛpinner;
[GoMethodHost(typeof(runtime_package.pinnerBits)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛpinnerBits;
[GoMethodHost(typeof(runtime_package.plainError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛplainError;
[GoMethodHost(typeof(runtime_package.pollCache)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛpollCache;
[GoMethodHost(typeof(runtime_package.pollDesc)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛpollDesc;
[GoMethodHost(typeof(runtime_package.pollInfo)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛpollInfo;
[GoMethodHost(typeof(runtime_package.profAtomic)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛprofAtomic;
[GoMethodHost(typeof(runtime_package.profBuf)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛprofBuf;
[GoMethodHost(typeof(runtime_package.profIndex)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛprofIndex;
[GoMethodHost(typeof(runtime_package.puintptr)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛpuintptr;
[GoMethodHost(typeof(runtime_package.randomEnum)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛrandomEnum;
[GoMethodHost(typeof(runtime_package.randomOrder)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛrandomOrder;
[GoMethodHost(typeof(runtime_package.rtype)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛrtype;
[GoMethodHost(typeof(runtime_package.rwmutex)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛrwmutex;
[GoMethodHost(typeof(runtime_package.scavChunkData)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛscavChunkData;
[GoMethodHost(typeof(runtime_package.scavChunkFlags)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛscavChunkFlags;
[GoMethodHost(typeof(runtime_package.scavengeIndex)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛscavengeIndex;
[GoMethodHost(typeof(runtime_package.scavengerState)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛscavengerState;
[GoMethodHost(typeof(runtime_package.schedStatsAggregate)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛschedStatsAggregate;
[GoMethodHost(typeof(runtime_package.semTable)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛsemTable;
[GoMethodHost(typeof(runtime_package.semaRoot)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛsemaRoot;
[GoMethodHost(typeof(runtime_package.spanAllocType)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛspanAllocType;
[GoMethodHost(typeof(runtime_package.spanClass)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛspanClass;
[GoMethodHost(typeof(runtime_package.spanInlineMarkBits)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛspanInlineMarkBits;
[GoMethodHost(typeof(runtime_package.spanQueue)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛspanQueue;
[GoMethodHost(typeof(runtime_package.spanSPMC)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛspanSPMC;
[GoMethodHost(typeof(runtime_package.spanScanOwnership)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛspanScanOwnership;
[GoMethodHost(typeof(runtime_package.spanSet)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛspanSet;
[GoMethodHost(typeof(runtime_package.spanSetBlockAlloc)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛspanSetBlockAlloc;
[GoMethodHost(typeof(runtime_package.spanSetSpinePointer)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛspanSetSpinePointer;
[GoMethodHost(typeof(runtime_package.specialsIter)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛspecialsIter;
[GoMethodHost(typeof(runtime_package.srcFunc)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛsrcFunc;
[GoMethodHost(typeof(runtime_package.stackObject)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛstackObject;
[GoMethodHost(typeof(runtime_package.stackObjectRecord)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛstackObjectRecord;
[GoMethodHost(typeof(runtime_package.stackScanState)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛstackScanState;
[GoMethodHost(typeof(runtime_package.statAggregate)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛstatAggregate;
[GoMethodHost(typeof(runtime_package.statDepSet)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛstatDepSet;
[GoMethodHost(typeof(runtime_package.stkframe)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛstkframe;
[GoMethodHost(typeof(runtime_package.stwReason)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛstwReason;
[GoMethodHost(typeof(runtime_package.sweepClass)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛsweepClass;
[GoMethodHost(typeof(runtime_package.sweepLocked)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛsweepLocked;
[GoMethodHost(typeof(runtime_package.sweepLocker)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛsweepLocker;
[GoMethodHost(typeof(runtime_package.synctestBubble)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛsynctestBubble;
[GoMethodHost(typeof(runtime_package.synctestDeadlockError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛsynctestDeadlockError;
[GoMethodHost(typeof(runtime_package.sysMemStat)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛsysMemStat;
[GoMethodHost(typeof(runtime_package.sysStatsAggregate)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛsysStatsAggregate;
[GoMethodHost(typeof(runtime_package.syscallingThread)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛsyscallingThread;
[GoMethodHost(typeof(runtime_package.taggedPointer)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛtaggedPointer;
[GoMethodHost(typeof(runtime_package.ticksType)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛticksType;
[GoMethodHost(typeof(runtime_package.timeHistogram)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛtimeHistogram;
[GoMethodHost(typeof(runtime_package.timer)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛtimer;
[GoMethodHost(typeof(runtime_package.timerWhen)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛtimerWhen;
[GoMethodHost(typeof(runtime_package.timers)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛtimers;
[GoMethodHost(typeof(runtime_package.traceAdvancerState)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛtraceAdvancerState;
[GoMethodHost(typeof(runtime_package.traceBuf)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛtraceBuf;
[GoMethodHost(typeof(runtime_package.traceBufQueue)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛtraceBufQueue;
[GoMethodHost(typeof(runtime_package.traceEventWriter)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛtraceEventWriter;
[GoMethodHost(typeof(runtime_package.traceLocker)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛtraceLocker;
[GoMethodHost(typeof(runtime_package.traceMap)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛtraceMap;
[GoMethodHost(typeof(runtime_package.traceRegionAlloc)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛtraceRegionAlloc;
[GoMethodHost(typeof(runtime_package.traceSchedResourceState)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛtraceSchedResourceState;
[GoMethodHost(typeof(runtime_package.traceStackTable)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛtraceStackTable;
[GoMethodHost(typeof(runtime_package.traceStringTable)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛtraceStringTable;
[GoMethodHost(typeof(runtime_package.traceTypeTable)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛtraceTypeTable;
[GoMethodHost(typeof(runtime_package.traceWriter)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛtraceWriter;
[GoMethodHost(typeof(runtime_package.typePointers)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛtypePointers;
[GoMethodHost(typeof(runtime_package.unwinder)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛunwinder;
[GoMethodHost(typeof(runtime_package.userArena)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛuserArena;
[GoMethodHost(typeof(runtime_package.waitReason)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛwaitReason;
[GoMethodHost(typeof(runtime_package.waitq)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛwaitq;
[GoMethodHost(typeof(runtime_package.wakeableSleep)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛwakeableSleep;
[GoMethodHost(typeof(runtime_package.wbBuf)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛwbBuf;
[GoMethodHost(typeof(runtime_package.workbuf)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛworkbuf;
[GoMethodHost(typeof(runtime_package.writeUserArenaHeapBits)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛwriteUserArenaHeapBits;
[GoMethodHost(typeof(runtime_package.xRegPerP)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class runtimeᴛxRegPerP;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class runtimeᴛFunc {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<_func> raw(this ж<Func> Ꮡf) => runtime_package.raw⁔(Ꮡf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static funcInfo funcInfo(this ж<Func> Ꮡf) => runtime_package.funcInfo⁔(Ꮡf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int32 startLine(this ж<Func> Ꮡf) => runtime_package.startLine⁔(Ꮡf);
}
partial class runtimeᴛMemProfileRecord {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static int64 InUseBytes(this ref MemProfileRecord r) => runtime_package.InUseBytes⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static int64 InUseObjects(this ref MemProfileRecord r) => runtime_package.InUseObjects⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<uintptr> Stack(this ref MemProfileRecord r) => runtime_package.Stack⁔(ref r);
}
partial class runtimeᴛPanicNilError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Error(this ref PanicNilError _) => runtime_package.Error⁔(ref _);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void RuntimeError(this ref PanicNilError _) => runtime_package.RuntimeError⁔(ref _);
}
partial class runtimeᴛStackRecord {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<uintptr> Stack(this ref StackRecord r) => runtime_package.Stack⁔(ref r);
}
partial class runtimeᴛTypeAssertionError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void RuntimeError(this ref TypeAssertionError _) => runtime_package.RuntimeError⁔(ref _);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Error(this ref TypeAssertionError e) => runtime_package.Error⁔(ref e);
}
partial class runtimeᴛ_func {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static funcInfo funcInfo(this ж<_func> Ꮡf) => runtime_package.funcInfo⁔(Ꮡf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool isInlined(this ref _func f) => runtime_package.isInlined⁔(ref f);
}
partial class runtimeᴛ_panic {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void start(this ж<_panic> Ꮡp, uintptr pc, unsafe_package.Pointer sp) => runtime_package.start⁔(Ꮡp, pc, sp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (Action, bool) nextDefer(this ж<_panic> Ꮡp) => runtime_package.nextDefer⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool /*ok*/ nextFrame(this ж<_panic> Ꮡp) => runtime_package.nextFrame⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool initOpenCodedDefers(this ref _panic p, funcInfo fn, unsafe_package.Pointer varp) => runtime_package.initOpenCodedDefers⁔(ref p, fn, varp);
}
partial class runtimeᴛabiDesc {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void assignArg(this ref abiDesc p, ж<_type> Ꮡt) => runtime_package.assignArg⁔(ref p, Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool tryRegAssignArg(this ref abiDesc p, ж<_type> Ꮡt, uintptr offset) => runtime_package.tryRegAssignArg⁔(ref p, Ꮡt, offset);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool assignReg(this ref abiDesc p, uintptr size, uintptr offset) => runtime_package.assignReg⁔(ref p, size, offset);
}
partial class runtimeᴛabiPart {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool tryMerge(this ref abiPart a, abiPart b) => runtime_package.tryMerge⁔(ref a, b);
}
partial class runtimeᴛactiveSweep {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static sweepLocker begin(this ж<activeSweep> Ꮡa) => runtime_package.begin⁔(Ꮡa);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void end(this ж<activeSweep> Ꮡa, sweepLocker sl) => runtime_package.end⁔(Ꮡa, sl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool markDrained(this ж<activeSweep> Ꮡa) => runtime_package.markDrained⁔(Ꮡa);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint32 sweepers(this ж<activeSweep> Ꮡa) => runtime_package.sweepers⁔(Ꮡa);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool isDone(this ж<activeSweep> Ꮡa) => runtime_package.isDone⁔(Ꮡa);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void reset(this ж<activeSweep> Ꮡa) => runtime_package.reset⁔(Ꮡa);
}
partial class runtimeᴛaddrRange {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uintptr size(this addrRange a) => runtime_package.size⁔(a);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool contains(this addrRange a, uintptr addr) => runtime_package.contains⁔(a, addr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static addrRange subtract(this addrRange a, addrRange b) => runtime_package.subtract⁔(a, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (uintptr, bool) takeFromFront(this ref addrRange a, uintptr len, uint8 align) => runtime_package.takeFromFront⁔(ref a, len, align);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (uintptr, bool) takeFromBack(this ref addrRange a, uintptr len, uint8 align) => runtime_package.takeFromBack⁔(ref a, len, align);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static addrRange removeGreaterEqual(this addrRange a, uintptr addr) => runtime_package.removeGreaterEqual⁔(a, addr);
}
partial class runtimeᴛaddrRanges {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint findSucc(this ref addrRanges a, uintptr addr) => runtime_package.findSucc⁔(ref a, addr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (uintptr, bool) findAddrGreaterEqual(this ref addrRanges a, uintptr addr) => runtime_package.findAddrGreaterEqual⁔(ref a, addr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool contains(this ref addrRanges a, uintptr addr) => runtime_package.contains⁔(ref a, addr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static addrRange removeLast(this ref addrRanges a, uintptr nBytes) => runtime_package.removeLast⁔(ref a, nBytes);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void removeGreaterEqual(this ref addrRanges a, uintptr addr) => runtime_package.removeGreaterEqual⁔(ref a, addr);
}
partial class runtimeᴛarenaIdx {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nuint l1(this arenaIdx i) => runtime_package.l1⁔(i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nuint l2(this arenaIdx i) => runtime_package.l2⁔(i);
}
partial class runtimeᴛatomicHeadTailIndex {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static headTailIndex load(this ж<atomicHeadTailIndex> Ꮡh) => runtime_package.load⁔(Ꮡh);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool cas(this ж<atomicHeadTailIndex> Ꮡh, headTailIndex old, headTailIndex @new) => runtime_package.cas⁔(Ꮡh, old, @new);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static headTailIndex incHead(this ж<atomicHeadTailIndex> Ꮡh) => runtime_package.incHead⁔(Ꮡh);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static headTailIndex decHead(this ж<atomicHeadTailIndex> Ꮡh) => runtime_package.decHead⁔(Ꮡh);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static headTailIndex incTail(this ж<atomicHeadTailIndex> Ꮡh) => runtime_package.incTail⁔(Ꮡh);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void reset(this ж<atomicHeadTailIndex> Ꮡh) => runtime_package.reset⁔(Ꮡh);
}
partial class runtimeᴛatomicMSpanPointer {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<mspan> Load(this ж<atomicMSpanPointer> Ꮡp) => runtime_package.Load⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void StoreNoWB(this ж<atomicMSpanPointer> Ꮡp, ж<mspan> Ꮡs) => runtime_package.StoreNoWB⁔(Ꮡp, Ꮡs);
}
partial class runtimeᴛatomicOffAddr {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void Clear(this ж<atomicOffAddr> Ꮡb) => runtime_package.Clear⁔(Ꮡb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void StoreMin(this ж<atomicOffAddr> Ꮡb, uintptr addr) => runtime_package.StoreMin⁔(Ꮡb, addr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void StoreUnmark(this ж<atomicOffAddr> Ꮡb, uintptr markedAddr, uintptr newAddr) => runtime_package.StoreUnmark⁔(Ꮡb, markedAddr, newAddr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void StoreMarked(this ж<atomicOffAddr> Ꮡb, uintptr addr) => runtime_package.StoreMarked⁔(Ꮡb, addr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (uintptr, bool) Load(this ж<atomicOffAddr> Ꮡb) => runtime_package.Load⁔(Ꮡb);
}
partial class runtimeᴛatomicScavChunkData {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static scavChunkData load(this ж<atomicScavChunkData> Ꮡsc) => runtime_package.load⁔(Ꮡsc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void store(this ж<atomicScavChunkData> Ꮡsc, scavChunkData ssc) => runtime_package.store⁔(Ꮡsc, ssc);
}
partial class runtimeᴛatomicSpanSetSpinePointer {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static spanSetSpinePointer Load(this ж<atomicSpanSetSpinePointer> Ꮡs) => runtime_package.Load⁔(Ꮡs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void StoreNoWB(this ж<atomicSpanSetSpinePointer> Ꮡs, spanSetSpinePointer p) => runtime_package.StoreNoWB⁔(Ꮡs, p);
}
partial class runtimeᴛbitCursor {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void write(this bitCursor b, ж<byte> Ꮡdata, uintptr cnt) => runtime_package.write⁔(b, Ꮡdata, cnt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bitCursor offset(this bitCursor b, uintptr cnt) => runtime_package.offset⁔(b, cnt);
}
partial class runtimeᴛbitvector {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint8 ptrbit(this ref bitvector bv, uintptr i) => runtime_package.ptrbit⁔(ref bv, i);
}
partial class runtimeᴛboundsError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void RuntimeError(this boundsError e) => runtime_package.RuntimeError⁔(e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Error(this boundsError e) => runtime_package.Error⁔(e);
}
partial class runtimeᴛchunkIdx {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nuint l1(this chunkIdx i) => runtime_package.l1⁔(i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nuint l2(this chunkIdx i) => runtime_package.l2⁔(i);
}
partial class runtimeᴛconsistentHeapStats {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<heapStatsDelta> acquire(this ж<consistentHeapStats> Ꮡm) => runtime_package.acquire⁔(Ꮡm);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void release(this ж<consistentHeapStats> Ꮡm) => runtime_package.release⁔(Ꮡm);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void unsafeRead(this ref consistentHeapStats m, ж<heapStatsDelta> Ꮡout) => runtime_package.unsafeRead⁔(ref m, Ꮡout);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void unsafeClear(this ref consistentHeapStats m) => runtime_package.unsafeClear⁔(ref m);
}
partial class runtimeᴛcpuProfile {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void add(this ref cpuProfile p, ж<unsafe_package.Pointer> ᏑtagPtr, slice<uintptr> stk) => runtime_package.add⁔(ref p, ᏑtagPtr, stk);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void addNonGo(this ref cpuProfile p, slice<uintptr> stk) => runtime_package.addNonGo⁔(ref p, stk);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void addExtra(this ref cpuProfile p) => runtime_package.addExtra⁔(ref p);
}
partial class runtimeᴛcpuStats {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void accumulateGCPauseTime(this ref cpuStats s, int64 dt, int32 maxProcs) => runtime_package.accumulateGCPauseTime⁔(ref s, dt, maxProcs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void accumulate(this ref cpuStats s, int64 now, bool gcMarkPhase) => runtime_package.accumulate⁔(ref s, now, gcMarkPhase);
}
partial class runtimeᴛcpuStatsAggregate {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void compute(this ref cpuStatsAggregate a) => runtime_package.compute⁔(ref a);
}
partial class runtimeᴛdebugLogReader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint64 skip(this ref debugLogReader r) => runtime_package.skip⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint16 readUint16LEAt(this ref debugLogReader r, uint64 pos) => runtime_package.readUint16LEAt⁔(ref r, pos);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint64 readUint64LEAt(this ref debugLogReader r, uint64 pos) => runtime_package.readUint64LEAt⁔(ref r, pos);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint64 /*tick*/ peek(this ref debugLogReader r) => runtime_package.peek⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (uint64 end, uint64 tick, uint64 nano, nint p) header(this ref debugLogReader r) => runtime_package.header⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint64 uvarint(this ref debugLogReader r) => runtime_package.uvarint⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int64 varint(this ref debugLogReader r) => runtime_package.varint⁔(ref r);
}
partial class runtimeᴛdebugLogWriter {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void ensure(this ref debugLogWriter l, uint64 n) => runtime_package.ensure⁔(ref l, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool writeFrameAt(this ref debugLogWriter l, uint64 pos, uint64 size) => runtime_package.writeFrameAt⁔(ref l, pos, size);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void writeSync(this ref debugLogWriter l, uint64 tick, uint64 nano) => runtime_package.writeSync⁔(ref l, tick, nano);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void writeUint64LE(this ref debugLogWriter l, uint64 x) => runtime_package.writeUint64LE⁔(ref l, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void @byte(this ref debugLogWriter l, byte x) => runtime_package.byte⁔(ref l, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void bytes(this ref debugLogWriter l, slice<byte> x) => runtime_package.bytes⁔(ref l, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void varint(this ref debugLogWriter l, int64 x) => runtime_package.varint⁔(ref l, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void uvarint(this ref debugLogWriter l, uint64 u) => runtime_package.uvarint⁔(ref l, u);
}
partial class runtimeᴛdloggerFake {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void end(this dloggerFake l) => runtime_package.end⁔(l);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static dloggerFake b(this dloggerFake l, bool x) => runtime_package.b⁔(l, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static dloggerFake i(this dloggerFake l, nint x) => runtime_package.i⁔(l, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static dloggerFake i8(this dloggerFake l, int8 x) => runtime_package.i8⁔(l, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static dloggerFake i16(this dloggerFake l, int16 x) => runtime_package.i16⁔(l, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static dloggerFake i32(this dloggerFake l, int32 x) => runtime_package.i32⁔(l, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static dloggerFake i64(this dloggerFake l, int64 x) => runtime_package.i64⁔(l, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static dloggerFake u(this dloggerFake l, nuint x) => runtime_package.u⁔(l, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static dloggerFake uptr(this dloggerFake l, uintptr x) => runtime_package.uptr⁔(l, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static dloggerFake u8(this dloggerFake l, uint8 x) => runtime_package.u8⁔(l, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static dloggerFake u16(this dloggerFake l, uint16 x) => runtime_package.u16⁔(l, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static dloggerFake u32(this dloggerFake l, uint32 x) => runtime_package.u32⁔(l, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static dloggerFake u64(this dloggerFake l, uint64 x) => runtime_package.u64⁔(l, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static dloggerFake hex(this dloggerFake l, uint64 x) => runtime_package.hex⁔(l, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static dloggerFake p(this dloggerFake l, any x) => runtime_package.p⁔(l, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static dloggerFake s(this dloggerFake l, @string x) => runtime_package.s⁔(l, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static dloggerFake hexdump(this dloggerFake l, unsafe_package.Pointer p, uintptr bytes) => runtime_package.hexdump⁔(l, p, bytes);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static dloggerFake pc(this dloggerFake l, uintptr x) => runtime_package.pc⁔(l, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static dloggerFake traceback(this dloggerFake l, slice<uintptr> x) => runtime_package.traceback⁔(l, x);
}
partial class runtimeᴛdloggerImpl {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void end(this ж<dloggerImpl> Ꮡl) => runtime_package.end⁔(Ꮡl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<dloggerImpl> b(this ж<dloggerImpl> Ꮡl, bool x) => runtime_package.b⁔(Ꮡl, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<dloggerImpl> i(this ж<dloggerImpl> Ꮡl, nint x) => runtime_package.i⁔(Ꮡl, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<dloggerImpl> i8(this ж<dloggerImpl> Ꮡl, int8 x) => runtime_package.i8⁔(Ꮡl, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<dloggerImpl> i16(this ж<dloggerImpl> Ꮡl, int16 x) => runtime_package.i16⁔(Ꮡl, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<dloggerImpl> i32(this ж<dloggerImpl> Ꮡl, int32 x) => runtime_package.i32⁔(Ꮡl, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<dloggerImpl> i64(this ж<dloggerImpl> Ꮡl, int64 x) => runtime_package.i64⁔(Ꮡl, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<dloggerImpl> u(this ж<dloggerImpl> Ꮡl, nuint x) => runtime_package.u⁔(Ꮡl, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<dloggerImpl> uptr(this ж<dloggerImpl> Ꮡl, uintptr x) => runtime_package.uptr⁔(Ꮡl, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<dloggerImpl> u8(this ж<dloggerImpl> Ꮡl, uint8 x) => runtime_package.u8⁔(Ꮡl, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<dloggerImpl> u16(this ж<dloggerImpl> Ꮡl, uint16 x) => runtime_package.u16⁔(Ꮡl, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<dloggerImpl> u32(this ж<dloggerImpl> Ꮡl, uint32 x) => runtime_package.u32⁔(Ꮡl, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<dloggerImpl> u64(this ж<dloggerImpl> Ꮡl, uint64 x) => runtime_package.u64⁔(Ꮡl, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<dloggerImpl> hex(this ж<dloggerImpl> Ꮡl, uint64 x) => runtime_package.hex⁔(Ꮡl, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<dloggerImpl> p(this ж<dloggerImpl> Ꮡl, any xʗp) => runtime_package.p⁔(Ꮡl, xʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<dloggerImpl> hexdump(this ж<dloggerImpl> Ꮡl, unsafe_package.Pointer p, uintptr bytes) => runtime_package.hexdump⁔(Ꮡl, p, bytes);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<dloggerImpl> pc(this ж<dloggerImpl> Ꮡl, uintptr x) => runtime_package.pc⁔(Ꮡl, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<dloggerImpl> traceback(this ж<dloggerImpl> Ꮡl, slice<uintptr> x) => runtime_package.traceback⁔(Ꮡl, x);
}
partial class runtimeᴛerrorAddressString {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void RuntimeError(this errorAddressString e) => runtime_package.RuntimeError⁔(e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Error(this errorAddressString e) => runtime_package.Error⁔(e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uintptr Addr(this errorAddressString e) => runtime_package.Addr⁔(e);
}
partial class runtimeᴛerrorString {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void RuntimeError(this errorString e) => runtime_package.RuntimeError⁔(e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Error(this errorString e) => runtime_package.Error⁔(e);
}
partial class runtimeᴛfinalStatsAggregate {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void compute(this ref finalStatsAggregate a) => runtime_package.compute⁔(ref a);
}
partial class runtimeᴛfixalloc {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void init(this ref fixalloc f, uintptr size, Action<unsafe_package.Pointer, unsafe_package.Pointer> first, unsafe_package.Pointer arg, ж<sysMemStat> Ꮡstat) => runtime_package.init⁔(ref f, size, first, arg, Ꮡstat);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static unsafe_package.Pointer alloc(this ref fixalloc f) => runtime_package.alloc⁔(ref f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void free(this ref fixalloc f, unsafe_package.Pointer p) => runtime_package.free⁔(ref f, p);
}
partial class runtimeᴛfuncInfo {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool valid(this funcInfo f) => runtime_package.valid⁔(f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Func> _Func(this funcInfo f) => runtime_package._Func⁔(f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uintptr entry(this funcInfo f) => runtime_package.entry⁔(f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static srcFunc srcFunc(this funcInfo f) => runtime_package.srcFunc⁔(f);
}
partial class runtimeᴛg {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool isMaybeRunnable(this ж<g> Ꮡgp) => runtime_package.isMaybeRunnable⁔(Ꮡgp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool internalBlocked(this ref g gp) => runtime_package.internalBlocked⁔(ref gp);
}
partial class runtimeᴛgList {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool empty(this ref gList l) => runtime_package.empty⁔(ref l);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void push(this ref gList l, ж<g> Ꮡgp) => runtime_package.push⁔(ref l, Ꮡgp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void pushAll(this ref gList l, gQueue q) => runtime_package.pushAll⁔(ref l, q);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<g> pop(this ref gList l) => runtime_package.pop⁔(ref l);
}
partial class runtimeᴛgQueue {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool empty(this ref gQueue q) => runtime_package.empty⁔(ref q);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void push(this ref gQueue q, ж<g> Ꮡgp) => runtime_package.push⁔(ref q, Ꮡgp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void pushBack(this ref gQueue q, ж<g> Ꮡgp) => runtime_package.pushBack⁔(ref q, Ꮡgp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void pushBackAll(this ref gQueue q, gQueue q2) => runtime_package.pushBackAll⁔(ref q, q2);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<g> pop(this ref gQueue q) => runtime_package.pop⁔(ref q);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static gList popList(this ref gQueue q) => runtime_package.popList⁔(ref q);
}
partial class runtimeᴛgTraceState {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void reset(this ref gTraceState s) => runtime_package.reset⁔(ref s);
}
partial class runtimeᴛgcBits {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<uint8> bytep(this ж<gcBits> Ꮡb, uintptr n) => runtime_package.bytep⁔(Ꮡb, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<uint8> bytep, uint8 mask) bitp(this ж<gcBits> Ꮡb, uintptr n) => runtime_package.bitp⁔(Ꮡb, n);
}
partial class runtimeᴛgcBitsArena {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<gcBits> tryAlloc(this ж<gcBitsArena> Ꮡb, uintptr bytes) => runtime_package.tryAlloc⁔(Ꮡb, bytes);
}
partial class runtimeᴛgcCPULimiterState {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool limiting(this ж<gcCPULimiterState> Ꮡl) => runtime_package.limiting⁔(Ꮡl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void startGCTransition(this ж<gcCPULimiterState> Ꮡl, bool enableGC, int64 now) => runtime_package.startGCTransition⁔(Ꮡl, enableGC, now);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void finishGCTransition(this ж<gcCPULimiterState> Ꮡl, int64 now) => runtime_package.finishGCTransition⁔(Ꮡl, now);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool needUpdate(this ж<gcCPULimiterState> Ꮡl, int64 now) => runtime_package.needUpdate⁔(Ꮡl, now);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void addAssistTime(this ж<gcCPULimiterState> Ꮡl, int64 t) => runtime_package.addAssistTime⁔(Ꮡl, t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void addIdleTime(this ж<gcCPULimiterState> Ꮡl, int64 t) => runtime_package.addIdleTime⁔(Ꮡl, t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void update(this ж<gcCPULimiterState> Ꮡl, int64 now) => runtime_package.update⁔(Ꮡl, now);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void updateLocked(this ж<gcCPULimiterState> Ꮡl, int64 now) => runtime_package.updateLocked⁔(Ꮡl, now);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void accumulate(this ж<gcCPULimiterState> Ꮡl, int64 mutatorTime, int64 gcTime) => runtime_package.accumulate⁔(Ꮡl, mutatorTime, gcTime);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool tryLock(this ж<gcCPULimiterState> Ꮡl) => runtime_package.tryLock⁔(Ꮡl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void unlock(this ж<gcCPULimiterState> Ꮡl) => runtime_package.unlock⁔(Ꮡl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void resetCapacity(this ж<gcCPULimiterState> Ꮡl, int64 now, int32 nprocs) => runtime_package.resetCapacity⁔(Ꮡl, now, nprocs);
}
partial class runtimeᴛgcControllerState {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void init(this ж<gcControllerState> Ꮡc, int32 gcPercent, int64 memoryLimit) => runtime_package.init⁔(Ꮡc, gcPercent, memoryLimit);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void startCycle(this ж<gcControllerState> Ꮡc, int64 markStartTime, nint procs, gcTrigger trigger) => runtime_package.startCycle⁔(Ꮡc, markStartTime, procs, trigger);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void revise(this ж<gcControllerState> Ꮡc) => runtime_package.revise⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void endCycle(this ж<gcControllerState> Ꮡc, int64 now, nint procs, bool userForced) => runtime_package.endCycle⁔(Ꮡc, now, procs, userForced);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void enlistWorker(this ж<gcControllerState> Ꮡc) => runtime_package.enlistWorker⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (bool, int64) assignWaitingGCWorker(this ж<gcControllerState> Ꮡc, ж<p> Ꮡpp, int64 now) => runtime_package.assignWaitingGCWorker⁔(Ꮡc, Ꮡpp, now);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<g>, int64) findRunnableGCWorker(this ж<gcControllerState> Ꮡc, ж<p> Ꮡpp, int64 now) => runtime_package.findRunnableGCWorker⁔(Ꮡc, Ꮡpp, now);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void releaseNextGCMarkWorker(this ж<gcControllerState> Ꮡc, ж<p> Ꮡpp) => runtime_package.releaseNextGCMarkWorker⁔(Ꮡc, Ꮡpp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void resetLive(this ж<gcControllerState> Ꮡc, uint64 bytesMarked) => runtime_package.resetLive⁔(Ꮡc, bytesMarked);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void markWorkerStop(this ж<gcControllerState> Ꮡc, gcMarkWorkerMode mode, int64 duration) => runtime_package.markWorkerStop⁔(Ꮡc, mode, duration);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void update(this ж<gcControllerState> Ꮡc, int64 dHeapLive, int64 dHeapScan) => runtime_package.update⁔(Ꮡc, dHeapLive, dHeapScan);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void addScannableStack(this ж<gcControllerState> Ꮡc, ж<p> Ꮡpp, int64 amount) => runtime_package.addScannableStack⁔(Ꮡc, Ꮡpp, amount);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void addGlobals(this ж<gcControllerState> Ꮡc, int64 amount) => runtime_package.addGlobals⁔(Ꮡc, amount);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint64 heapGoal(this ж<gcControllerState> Ꮡc) => runtime_package.heapGoal⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (uint64 goal, uint64 minTrigger) heapGoalInternal(this ж<gcControllerState> Ꮡc) => runtime_package.heapGoalInternal⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint64 memoryLimitHeapGoal(this ж<gcControllerState> Ꮡc) => runtime_package.memoryLimitHeapGoal⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (uint64, uint64) trigger(this ж<gcControllerState> Ꮡc) => runtime_package.trigger⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void commit(this ж<gcControllerState> Ꮡc, bool isSweepDone) => runtime_package.commit⁔(Ꮡc, isSweepDone);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int32 setGCPercent(this ж<gcControllerState> Ꮡc, int32 @in) => runtime_package.setGCPercent⁔(Ꮡc, @in);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int64 setMemoryLimit(this ж<gcControllerState> Ꮡc, int64 @in) => runtime_package.setMemoryLimit⁔(Ꮡc, @in);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool addIdleMarkWorker(this ж<gcControllerState> Ꮡc) => runtime_package.addIdleMarkWorker⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool needIdleMarkWorker(this ж<gcControllerState> Ꮡc) => runtime_package.needIdleMarkWorker⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void removeIdleMarkWorker(this ж<gcControllerState> Ꮡc) => runtime_package.removeIdleMarkWorker⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void setMaxIdleMarkWorkers(this ж<gcControllerState> Ꮡc, int32 max) => runtime_package.setMaxIdleMarkWorkers⁔(Ꮡc, max);
}
partial class runtimeᴛgcStatsAggregate {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void compute(this ref gcStatsAggregate a) => runtime_package.compute⁔(ref a);
}
partial class runtimeᴛgcTrigger {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool test(this gcTrigger t) => runtime_package.test⁔(t);
}
partial class runtimeᴛgcWork {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static objptr tryGetSpanFast(this ref gcWork w) => runtime_package.tryGetSpanFast⁔(ref w);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static objptr tryGetSpan(this ж<gcWork> Ꮡw) => runtime_package.tryGetSpan⁔(Ꮡw);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static objptr tryStealSpan(this ref gcWork w) => runtime_package.tryStealSpan⁔(ref w);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void flushScanStats(this ref gcWork w, /*[68]*/ ж<array<sizeClassScanStats>> Ꮡdst) => runtime_package.flushScanStats⁔(ref w, Ꮡdst);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void init(this ref gcWork w) => runtime_package.init⁔(ref w);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void putObj(this ref gcWork w, uintptr obj) => runtime_package.putObj⁔(ref w, obj);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool putObjFast(this ref gcWork w, uintptr obj) => runtime_package.putObjFast⁔(ref w, obj);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void putObjBatch(this ref gcWork w, slice<uintptr> obj) => runtime_package.putObjBatch⁔(ref w, obj);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uintptr tryGetObj(this ref gcWork w) => runtime_package.tryGetObj⁔(ref w);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uintptr tryGetObjFast(this ref gcWork w) => runtime_package.tryGetObjFast⁔(ref w);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void dispose(this ж<gcWork> Ꮡw) => runtime_package.dispose⁔(Ꮡw);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void balance(this ref gcWork w) => runtime_package.balance⁔(ref w);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool empty(this ж<gcWork> Ꮡw) => runtime_package.empty⁔(Ꮡw);
}
partial class runtimeᴛgclinkptr {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<gclink> ptr(this gclinkptr p) => runtime_package.ptr⁔(p);
}
partial class runtimeᴛgodebugInc {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void IncNonDefault(this ж<godebugInc> Ꮡg) => runtime_package.IncNonDefault⁔(Ꮡg);
}
partial class runtimeᴛgoroutineProfileStateHolder {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static goroutineProfileState Load(this ж<goroutineProfileStateHolder> Ꮡp) => runtime_package.Load⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void Store(this ж<goroutineProfileStateHolder> Ꮡp, goroutineProfileState value) => runtime_package.Store⁔(Ꮡp, value);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool CompareAndSwap(this ж<goroutineProfileStateHolder> Ꮡp, goroutineProfileState old, goroutineProfileState @new) => runtime_package.CompareAndSwap⁔(Ꮡp, old, @new);
}
partial class runtimeᴛhchan {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static unsafe_package.Pointer raceaddr(this ж<hchan> Ꮡc) => runtime_package.raceaddr⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uintptr sortkey(this ж<hchan> Ꮡc) => runtime_package.sortkey⁔(Ꮡc);
}
partial class runtimeᴛheadTailIndex {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint32 head(this headTailIndex h) => runtime_package.head⁔(h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint32 tail(this headTailIndex h) => runtime_package.tail⁔(h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (uint32 head, uint32 tail) split(this headTailIndex h) => runtime_package.split⁔(h);
}
partial class runtimeᴛheapStatsAggregate {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void compute(this ж<heapStatsAggregate> Ꮡa) => runtime_package.compute⁔(Ꮡa);
}
partial class runtimeᴛheapStatsDelta {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void merge(this ref heapStatsDelta a, ж<heapStatsDelta> Ꮡb) => runtime_package.merge⁔(ref a, Ꮡb);
}
partial class runtimeᴛhexdumpMarker {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void start(this hexdumpMarker m) => runtime_package.start⁔(m);
}
partial class runtimeᴛhexdumper {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void write(this ref hexdumper h, slice<byte> data) => runtime_package.write⁔(ref h, data);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void close(this ref hexdumper h) => runtime_package.close⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void init(this ref hexdumper h) => runtime_package.init⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void flushLine(this ref hexdumper h, slice<byte> data) => runtime_package.flushLine⁔(ref h, data);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void fmtHex(this ref hexdumper h, slice<byte> buf, uint64 v) => runtime_package.fmtHex⁔(ref h, buf, v);
}
partial class runtimeᴛimmortalWeakHandle {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<atomic.Uintptr> handle(this ж<immortalWeakHandle> Ꮡh) => runtime_package.handle⁔(Ꮡh);
}
partial class runtimeᴛimmortalWeakHandleMap {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<atomic.Uintptr> getOrAdd(this ж<immortalWeakHandleMap> Ꮡtab, uintptr pʗp) => runtime_package.getOrAdd⁔(Ꮡtab, pʗp);
}
partial class runtimeᴛinlineFrame {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool valid(this inlineFrame uf) => runtime_package.valid⁔(uf);
}
partial class runtimeᴛinlineUnwinder {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static inlineFrame resolveInternal(this ref inlineUnwinder u, uintptr pc) => runtime_package.resolveInternal⁔(ref u, pc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static inlineFrame next(this ref inlineUnwinder u, inlineFrame uf) => runtime_package.next⁔(ref u, uf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool isInlined(this ref inlineUnwinder u, inlineFrame uf) => runtime_package.isInlined⁔(ref u, uf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static srcFunc srcFunc(this ref inlineUnwinder u, inlineFrame uf) => runtime_package.srcFunc⁔(ref u, uf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (@string @file, nint line) fileLine(this ref inlineUnwinder u, inlineFrame uf) => runtime_package.fileLine⁔(ref u, uf);
}
partial class runtimeᴛitabTableType {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<itab> find(this ж<itabTableType> Ꮡt, ж<interfacetype> Ꮡinter, ж<_type> Ꮡtyp) => runtime_package.find⁔(Ꮡt, Ꮡinter, Ꮡtyp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void add(this ж<itabTableType> Ꮡt, ж<itab> Ꮡm) => runtime_package.add⁔(Ꮡt, Ꮡm);
}
partial class runtimeᴛlfstack {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void push(this ж<lfstack> Ꮡhead, ж<lfnode> Ꮡnode) => runtime_package.push⁔(Ꮡhead, Ꮡnode);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static unsafe_package.Pointer pop(this ж<lfstack> Ꮡhead) => runtime_package.pop⁔(Ꮡhead);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool empty(this ж<lfstack> Ꮡhead) => runtime_package.empty⁔(Ꮡhead);
}
partial class runtimeᴛlimiterEvent {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool start(this ж<limiterEvent> Ꮡe, limiterEventType typ, int64 now) => runtime_package.start⁔(Ꮡe, typ, now);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (limiterEventType typ, int64 duration) consume(this ж<limiterEvent> Ꮡe, int64 now) => runtime_package.consume⁔(Ꮡe, now);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void stop(this ж<limiterEvent> Ꮡe, limiterEventType typ, int64 now) => runtime_package.stop⁔(Ꮡe, typ, now);
}
partial class runtimeᴛlimiterEventStamp {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int64 duration(this limiterEventStamp s, int64 now) => runtime_package.duration⁔(s, now);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static limiterEventType typ(this limiterEventStamp s) => runtime_package.typ⁔(s);
}
partial class runtimeᴛlinearAlloc {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void init(this ref linearAlloc l, uintptr @base, uintptr size, bool mapMemory) => runtime_package.init⁔(ref l, @base, size, mapMemory);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static unsafe_package.Pointer alloc(this ref linearAlloc l, uintptr size, uintptr align, ж<sysMemStat> ᏑsysStat, @string vmaName) => runtime_package.alloc⁔(ref l, size, align, ᏑsysStat, vmaName);
}
partial class runtimeᴛlistHead {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void init(this ref listHead head, uintptr off) => runtime_package.init⁔(ref head, off);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<listNode> getNode(this ref listHead head, unsafe_package.Pointer p) => runtime_package.getNode⁔(ref head, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool empty(this ref listHead head) => runtime_package.empty⁔(ref head);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static unsafe_package.Pointer head(this ref listHead head) => runtime_package.head⁔(ref head);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void push(this ref listHead head, unsafe_package.Pointer p) => runtime_package.push⁔(ref head, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static unsafe_package.Pointer pop(this ref listHead head) => runtime_package.pop⁔(ref head);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void remove(this ref listHead head, unsafe_package.Pointer p) => runtime_package.remove⁔(ref head, p);
}
partial class runtimeᴛlistHeadManual {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void init(this ref listHeadManual head, uintptr off) => runtime_package.init⁔(ref head, off);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<listNodeManual> getNode(this ref listHeadManual head, unsafe_package.Pointer p) => runtime_package.getNode⁔(ref head, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool empty(this ref listHeadManual head) => runtime_package.empty⁔(ref head);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static unsafe_package.Pointer head(this ref listHeadManual head) => runtime_package.head⁔(ref head);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void push(this ref listHeadManual head, unsafe_package.Pointer p) => runtime_package.push⁔(ref head, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static unsafe_package.Pointer pop(this ref listHeadManual head) => runtime_package.pop⁔(ref head);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void remove(this ref listHeadManual head, unsafe_package.Pointer p) => runtime_package.remove⁔(ref head, p);
}
partial class runtimeᴛlockRank {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string String(this lockRank rank) => runtime_package.String⁔(rank);
}
partial class runtimeᴛm {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void becomeSpinning(this ref m mp) => runtime_package.becomeSpinning⁔(ref mp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<ж<p>> snapshotAllp(this ref m mp) => runtime_package.snapshotAllp⁔(ref mp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void clearAllpSnapshot(this ref m mp) => runtime_package.clearAllpSnapshot⁔(ref mp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool hasCgoOnStack(this ref m mp) => runtime_package.hasCgoOnStack⁔(ref mp);
}
partial class runtimeᴛmLockProfile {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int64 start(this ref mLockProfile prof) => runtime_package.start⁔(ref prof);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void end(this ж<mLockProfile> Ꮡprof, int64 start) => runtime_package.end⁔(Ꮡprof, start);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void recordUnlock(this ж<mLockProfile> Ꮡprof, int64 cycles) => runtime_package.recordUnlock⁔(Ꮡprof, cycles);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void captureStack(this ж<mLockProfile> Ꮡprof) => runtime_package.captureStack⁔(Ꮡprof);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void store(this ref mLockProfile prof) => runtime_package.store⁔(ref prof);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void storeSlow(this ref mLockProfile prof) => runtime_package.storeSlow⁔(ref prof);
}
partial class runtimeᴛmProfCycleHolder {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint32 /*cycle*/ read(this ж<mProfCycleHolder> Ꮡc) => runtime_package.read⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (uint32 cycle, bool alreadyFlushed) setFlushed(this ж<mProfCycleHolder> Ꮡc) => runtime_package.setFlushed⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void increment(this ж<mProfCycleHolder> Ꮡc) => runtime_package.increment⁔(Ꮡc);
}
partial class runtimeᴛmSpanList {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void init(this ref mSpanList list) => runtime_package.init⁔(ref list);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void remove(this ж<mSpanList> Ꮡlist, ж<mspan> Ꮡspan) => runtime_package.remove⁔(Ꮡlist, Ꮡspan);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool isEmpty(this ref mSpanList list) => runtime_package.isEmpty⁔(ref list);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void insert(this ж<mSpanList> Ꮡlist, ж<mspan> Ꮡspan) => runtime_package.insert⁔(Ꮡlist, Ꮡspan);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void insertBack(this ж<mSpanList> Ꮡlist, ж<mspan> Ꮡspan) => runtime_package.insertBack⁔(Ꮡlist, Ꮡspan);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void takeAll(this ж<mSpanList> Ꮡlist, ж<mSpanList> Ꮡother) => runtime_package.takeAll⁔(Ꮡlist, Ꮡother);
}
partial class runtimeᴛmSpanStateBox {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void set(this ж<mSpanStateBox> Ꮡb, mSpanState s) => runtime_package.set⁔(Ꮡb, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static mSpanState get(this ж<mSpanStateBox> Ꮡb) => runtime_package.get⁔(Ꮡb);
}
partial class runtimeᴛmarkBits {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool isMarked(this markBits m) => runtime_package.isMarked⁔(m);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void setMarked(this markBits m) => runtime_package.setMarked⁔(m);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void setMarkedNonAtomic(this markBits m) => runtime_package.setMarkedNonAtomic⁔(m);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void clearMarked(this markBits m) => runtime_package.clearMarked⁔(m);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void advance(this ref markBits m) => runtime_package.advance⁔(ref m);
}
partial class runtimeᴛmcache {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (gclinkptr v, ж<mspan> s, bool checkGCTrigger) nextFree(this ref mcache c, spanClass spc) => runtime_package.nextFree⁔(ref c, spc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (gclinkptr, ж<mspan>) nextReusableNoScan(this ref mcache c, ж<mspan> Ꮡs, spanClass spc) => runtime_package.nextReusableNoScan⁔(ref c, Ꮡs, spc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void refill(this ref mcache c, spanClass spc) => runtime_package.refill⁔(ref c, spc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<mspan> allocLarge(this ref mcache c, uintptr size, bool noscan) => runtime_package.allocLarge⁔(ref c, size, noscan);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void releaseAll(this ref mcache c) => runtime_package.releaseAll⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void prepareForSweep(this ж<mcache> Ꮡc) => runtime_package.prepareForSweep⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void addReusableNoscan(this ref mcache c, spanClass spc, uintptr ptr) => runtime_package.addReusableNoscan⁔(ref c, spc, ptr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool hasReusableNoscan(this ref mcache c, spanClass spc) => runtime_package.hasReusableNoscan⁔(ref c, spc);
}
partial class runtimeᴛmcentral {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void init(this ref mcentral c, spanClass spc) => runtime_package.init⁔(ref c, spc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<spanSet> partialUnswept(this ref mcentral c, uint32 sweepgen) => runtime_package.partialUnswept⁔(ref c, sweepgen);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<spanSet> partialSwept(this ref mcentral c, uint32 sweepgen) => runtime_package.partialSwept⁔(ref c, sweepgen);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<spanSet> fullUnswept(this ref mcentral c, uint32 sweepgen) => runtime_package.fullUnswept⁔(ref c, sweepgen);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<spanSet> fullSwept(this ref mcentral c, uint32 sweepgen) => runtime_package.fullSwept⁔(ref c, sweepgen);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<mspan> cacheSpan(this ref mcentral c) => runtime_package.cacheSpan⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void uncacheSpan(this ref mcentral c, ж<mspan> Ꮡs) => runtime_package.uncacheSpan⁔(ref c, Ꮡs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<mspan> grow(this ref mcentral c) => runtime_package.grow⁔(ref c);
}
partial class runtimeᴛmemRecordCycle {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void add(this ref memRecordCycle a, ж<memRecordCycle> Ꮡb) => runtime_package.add⁔(ref a, Ꮡb);
}
partial class runtimeᴛmetricReader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void compute(this metricReader f, ж<statAggregate> _, ж<metricValue> Ꮡout) => runtime_package.compute⁔(f, _, Ꮡout);
}
partial class runtimeᴛmetricValue {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<metricFloat64Histogram> float64HistOrInit(this ref metricValue v, slice<float64> buckets) => runtime_package.float64HistOrInit⁔(ref v, buckets);
}
partial class runtimeᴛmheap {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<mspan> allocUserArenaChunk(this ж<mheap> Ꮡh) => runtime_package.allocUserArenaChunk⁔(Ꮡh);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (unsafe_package.Pointer v, uintptr size) sysAlloc(this ж<mheap> Ꮡh, uintptr n, ж<ж<arenaHint>> ᏑhintList, ж<slice<arenaIdx>> ᏑarenaList) => runtime_package.sysAlloc⁔(Ꮡh, n, ᏑhintList, ᏑarenaList);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void enableMetadataHugePages(this ж<mheap> Ꮡh) => runtime_package.enableMetadataHugePages⁔(Ꮡh);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<mspan> nextSpanForSweep(this ref mheap h) => runtime_package.nextSpanForSweep⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void init(this ж<mheap> Ꮡh) => runtime_package.init⁔(Ꮡh);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void reclaim(this ж<mheap> Ꮡh, uintptr npage) => runtime_package.reclaim⁔(Ꮡh, npage);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uintptr reclaimChunk(this ж<mheap> Ꮡh, slice<arenaIdx> arenas, uintptr pageIdx, uintptr n) => runtime_package.reclaimChunk⁔(Ꮡh, arenas, pageIdx, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<mspan> alloc(this ж<mheap> Ꮡh, uintptr npages, spanClass spanclass) => runtime_package.alloc⁔(Ꮡh, npages, spanclass);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<mspan> allocManual(this ж<mheap> Ꮡh, uintptr npages, spanAllocType typ) => runtime_package.allocManual⁔(Ꮡh, npages, typ);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void setSpans(this ref mheap h, uintptr @base, uintptr npage, ж<mspan> Ꮡs) => runtime_package.setSpans⁔(ref h, @base, npage, Ꮡs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool /*needZero*/ allocNeedsZero(this ref mheap h, uintptr @base, uintptr npage) => runtime_package.allocNeedsZero⁔(ref h, @base, npage);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<mspan> tryAllocMSpan(this ref mheap h) => runtime_package.tryAllocMSpan⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<mspan> allocMSpanLocked(this ж<mheap> Ꮡh) => runtime_package.allocMSpanLocked⁔(Ꮡh);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void freeMSpanLocked(this ж<mheap> Ꮡh, ж<mspan> Ꮡs) => runtime_package.freeMSpanLocked⁔(Ꮡh, Ꮡs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<mspan> /*s*/ allocSpan(this ж<mheap> Ꮡh, uintptr npages, spanAllocType typ, spanClass spanclass) => runtime_package.allocSpan⁔(Ꮡh, npages, typ, spanclass);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void initSpan(this ж<mheap> Ꮡh, ж<mspan> Ꮡs, spanAllocType typ, spanClass spanclass, uintptr @base, uintptr npages, uintptr scav) => runtime_package.initSpan⁔(Ꮡh, Ꮡs, typ, spanclass, @base, npages, scav);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (uintptr, bool) grow(this ж<mheap> Ꮡh, uintptr npage) => runtime_package.grow⁔(Ꮡh, npage);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void freeSpan(this ж<mheap> Ꮡh, ж<mspan> Ꮡs) => runtime_package.freeSpan⁔(Ꮡh, Ꮡs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void freeManual(this ж<mheap> Ꮡh, ж<mspan> Ꮡs, spanAllocType typ) => runtime_package.freeManual⁔(Ꮡh, Ꮡs, typ);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void freeSpanLocked(this ж<mheap> Ꮡh, ж<mspan> Ꮡs, spanAllocType typ) => runtime_package.freeSpanLocked⁔(Ꮡh, Ꮡs, typ);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void scavengeAll(this ж<mheap> Ꮡh) => runtime_package.scavengeAll⁔(Ꮡh);
}
partial class runtimeᴛmoduledata {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uintptr textAddr(this ref moduledata md, uint32 off32) => runtime_package.textAddr⁔(ref md, off32);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (uint32, bool) textOff(this ref moduledata md, uintptr pc) => runtime_package.textOff⁔(ref md, pc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string funcName(this ref moduledata md, int32 nameOff) => runtime_package.funcName⁔(ref md, nameOff);
}
partial class runtimeᴛmspan {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static unsafe_package.Pointer userArenaNextFree(this ж<mspan> Ꮡs, ж<_type> Ꮡtyp, nint cap) => runtime_package.userArenaNextFree⁔(Ꮡs, Ꮡtyp, cap);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static writeUserArenaHeapBits /*h*/ writeUserArenaHeapBits(this ref mspan s, uintptr addr) => runtime_package.writeUserArenaHeapBits⁔(ref s, addr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool isUnusedUserArenaChunk(this ref mspan s) => runtime_package.isUnusedUserArenaChunk⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void setUserArenaChunkToFault(this ж<mspan> Ꮡs) => runtime_package.setUserArenaChunkToFault⁔(Ꮡs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static typePointers typePointersOf(this ж<mspan> Ꮡspan, uintptr addr, uintptr size) => runtime_package.typePointersOf⁔(Ꮡspan, addr, size);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static typePointers typePointersOfUnchecked(this ж<mspan> Ꮡspan, uintptr addr) => runtime_package.typePointersOfUnchecked⁔(Ꮡspan, addr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static typePointers typePointersOfType(this ref mspan span, ж<abi.Type> Ꮡtyp, uintptr addr) => runtime_package.typePointersOfType⁔(ref span, Ꮡtyp, addr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uintptr objBase(this ref mspan span, uintptr addr) => runtime_package.objBase⁔(ref span, addr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void initHeapBits(this ref mspan s) => runtime_package.initHeapBits⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<uintptr> heapBits(this ref mspan span) => runtime_package.heapBits⁔(ref span);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uintptr heapBitsSmallForAddr(this ref mspan span, uintptr addr) => runtime_package.heapBitsSmallForAddr⁔(ref span, addr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uintptr /*scanSize*/ writeHeapBitsSmall(this ref mspan span, uintptr x, uintptr dataSize, ж<_type> Ꮡtyp) => runtime_package.writeHeapBitsSmall⁔(ref span, x, dataSize, Ꮡtyp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static markBits allocBitsForIndex(this ref mspan s, uintptr allocBitIndex) => runtime_package.allocBitsForIndex⁔(ref s, allocBitIndex);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void refillAllocCache(this ref mspan s, uint16 whichByte) => runtime_package.refillAllocCache⁔(ref s, whichByte);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint16 nextFreeIndex(this ref mspan s) => runtime_package.nextFreeIndex⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool isFree(this ref mspan s, uintptr index) => runtime_package.isFree⁔(ref s, index);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool isFreeOrNewlyAllocated(this ref mspan s, uintptr index) => runtime_package.isFreeOrNewlyAllocated⁔(ref s, index);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uintptr divideByElemSize(this ref mspan s, uintptr n) => runtime_package.divideByElemSize⁔(ref s, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uintptr objIndex(this ref mspan s, uintptr p) => runtime_package.objIndex⁔(ref s, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint countAlloc(this ref mspan s) => runtime_package.countAlloc⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void initInlineMarkBits(this ref mspan s) => runtime_package.initInlineMarkBits⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void moveInlineMarks(this ref mspan s, ж<gcBits> Ꮡdst) => runtime_package.moveInlineMarks⁔(ref s, Ꮡdst);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<spanInlineMarkBits> inlineMarkBits(this ref mspan s) => runtime_package.inlineMarkBits⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static markBits /*bits*/ markBitsForIndex(this ref mspan s, uintptr objIndex) => runtime_package.markBitsForIndex⁔(ref s, objIndex);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static markBits markBitsForBase(this ref mspan s) => runtime_package.markBitsForBase⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static markBits scannedBitsForIndex(this ref mspan s, uintptr objIndex) => runtime_package.scannedBitsForIndex⁔(ref s, objIndex);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void ensureSwept(this ж<mspan> Ꮡs) => runtime_package.ensureSwept⁔(Ꮡs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void reportZombies(this ж<mspan> Ꮡs) => runtime_package.reportZombies⁔(Ꮡs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uintptr @base(this ref mspan s) => runtime_package.base⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (uintptr size, uintptr n, uintptr total) layout(this ref mspan s) => runtime_package.layout⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void init(this ж<mspan> Ꮡspan, uintptr @base, uintptr npages) => runtime_package.init⁔(Ꮡspan, @base, npages);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool inList(this ref mspan span) => runtime_package.inList⁔(ref span);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<ж<special>>, bool) specialFindSplicePoint(this ж<mspan> Ꮡspan, uintptr offset, byte kind) => runtime_package.specialFindSplicePoint⁔(Ꮡspan, offset, kind);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uintptr pinnerBitSize(this ref mspan s) => runtime_package.pinnerBitSize⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<pinnerBits> newPinnerBits(this ref mspan s) => runtime_package.newPinnerBits⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<pinnerBits> getPinnerBits(this ж<mspan> Ꮡs) => runtime_package.getPinnerBits⁔(Ꮡs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void setPinnerBits(this ж<mspan> Ꮡs, ж<pinnerBits> Ꮡp) => runtime_package.setPinnerBits⁔(Ꮡs, Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void refreshPinnerBits(this ж<mspan> Ꮡs) => runtime_package.refreshPinnerBits⁔(Ꮡs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void incPinCounter(this ж<mspan> Ꮡspan, uintptr offset) => runtime_package.incPinCounter⁔(Ꮡspan, offset);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool decPinCounter(this ж<mspan> Ꮡspan, uintptr offset) => runtime_package.decPinCounter⁔(Ꮡspan, offset);
}
partial class runtimeᴛnotInHeap {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<notInHeap> add(this ж<notInHeap> Ꮡp, uintptr bytes) => runtime_package.add⁔(Ꮡp, bytes);
}
partial class runtimeᴛobjptr {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uintptr spanBase(this objptr p) => runtime_package.spanBase⁔(p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint16 objIndex(this objptr p) => runtime_package.objIndex⁔(p);
}
partial class runtimeᴛoffAddr {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static offAddr add(this offAddr l, uintptr bytes) => runtime_package.add⁔(l, bytes);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static offAddr sub(this offAddr l, uintptr bytes) => runtime_package.sub⁔(l, bytes);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uintptr diff(this offAddr l1, offAddr l2) => runtime_package.diff⁔(l1, l2);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool lessThan(this offAddr l1, offAddr l2) => runtime_package.lessThan⁔(l1, l2);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool lessEqual(this offAddr l1, offAddr l2) => runtime_package.lessEqual⁔(l1, l2);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool equal(this offAddr l1, offAddr l2) => runtime_package.equal⁔(l1, l2);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uintptr addr(this offAddr l) => runtime_package.addr⁔(l);
}
partial class runtimeᴛp {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void init(this ж<p> Ꮡpp, int32 id) => runtime_package.init⁔(Ꮡpp, id);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void destroy(this ж<p> Ꮡpp) => runtime_package.destroy⁔(Ꮡpp);
}
partial class runtimeᴛpMask {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool read(this pMask p, uint32 id) => runtime_package.read⁔(p, id);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void set(this pMask p, int32 id) => runtime_package.set⁔(p, id);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void clear(this pMask p, int32 id) => runtime_package.clear⁔(p, id);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool any(this pMask p) => runtime_package.any⁔(p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static pMask resize(this pMask p, int32 nprocs) => runtime_package.resize⁔(p, nprocs);
}
partial class runtimeᴛpageAlloc {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uintptr scavenge(this ж<pageAlloc> Ꮡp, uintptr nbytes, Func<bool> shouldStop, bool force) => runtime_package.scavenge⁔(Ꮡp, nbytes, shouldStop, force);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uintptr scavengeOne(this ref pageAlloc p, chunkIdx ci, nuint searchIdx, uintptr max) => runtime_package.scavengeOne⁔(ref p, ci, searchIdx, max);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void init(this ж<pageAlloc> Ꮡp, ж<mutex> ᏑmheapLock, ж<sysMemStat> ᏑsysStat, bool test) => runtime_package.init⁔(Ꮡp, ᏑmheapLock, ᏑsysStat, test);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<pallocData> tryChunkOf(this ref pageAlloc p, chunkIdx ci) => runtime_package.tryChunkOf⁔(ref p, ci);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<pallocData> chunkOf(this ref pageAlloc p, chunkIdx ci) => runtime_package.chunkOf⁔(ref p, ci);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void grow(this ж<pageAlloc> Ꮡp, uintptr @base, uintptr size) => runtime_package.grow⁔(Ꮡp, @base, size);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void enableChunkHugePages(this ref pageAlloc p) => runtime_package.enableChunkHugePages⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void update(this ref pageAlloc p, uintptr @base, uintptr npages, bool contig, bool alloc) => runtime_package.update⁔(ref p, @base, npages, contig, alloc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uintptr allocRange(this ref pageAlloc p, uintptr @base, uintptr npages) => runtime_package.allocRange⁔(ref p, @base, npages);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static offAddr findMappedAddr(this ref pageAlloc p, offAddr addr) => runtime_package.findMappedAddr⁔(ref p, addr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (uintptr, offAddr) find(this ref pageAlloc p, uintptr npages) => runtime_package.find⁔(ref p, npages);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (uintptr addr, uintptr scav) alloc(this ref pageAlloc p, uintptr npages) => runtime_package.alloc⁔(ref p, npages);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void free(this ж<pageAlloc> Ꮡp, uintptr @base, uintptr npages) => runtime_package.free⁔(Ꮡp, @base, npages);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void markRandomPaddingPages(this ref pageAlloc p, uintptr @base, uintptr npages) => runtime_package.markRandomPaddingPages⁔(ref p, @base, npages);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void sysInit(this ref pageAlloc p, bool test) => runtime_package.sysInit⁔(ref p, test);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void sysGrow(this ж<pageAlloc> Ꮡp, uintptr @base, uintptr limit) => runtime_package.sysGrow⁔(Ꮡp, @base, limit);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static pageCache allocToCache(this ref pageAlloc p) => runtime_package.allocToCache⁔(ref p);
}
partial class runtimeᴛpageBits {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nuint get(this ref pageBits b, nuint i) => runtime_package.get⁔(ref b, i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint64 block64(this ref pageBits b, nuint i) => runtime_package.block64⁔(ref b, i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void set(this ref pageBits b, nuint i) => runtime_package.set⁔(ref b, i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void setRange(this ref pageBits b, nuint i, nuint n) => runtime_package.setRange⁔(ref b, i, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void setAll(this ref pageBits b) => runtime_package.setAll⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void setBlock64(this ref pageBits b, nuint i, uint64 v) => runtime_package.setBlock64⁔(ref b, i, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void clear(this ref pageBits b, nuint i) => runtime_package.clear⁔(ref b, i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void clearRange(this ref pageBits b, nuint i, nuint n) => runtime_package.clearRange⁔(ref b, i, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void clearAll(this ref pageBits b) => runtime_package.clearAll⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void clearBlock64(this ref pageBits b, nuint i, uint64 v) => runtime_package.clearBlock64⁔(ref b, i, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nuint /*s*/ popcntRange(this ref pageBits b, nuint i, nuint n) => runtime_package.popcntRange⁔(ref b, i, n);
}
partial class runtimeᴛpageCache {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool empty(this ref pageCache c) => runtime_package.empty⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (uintptr, uintptr) alloc(this ref pageCache c, uintptr npages) => runtime_package.alloc⁔(ref c, npages);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (uintptr, uintptr) allocN(this ref pageCache c, uintptr npages) => runtime_package.allocN⁔(ref c, npages);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void flush(this ref pageCache c, ж<pageAlloc> Ꮡp) => runtime_package.flush⁔(ref c, Ꮡp);
}
partial class runtimeᴛpallocBits {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static pallocSum summarize(this ж<pallocBits> Ꮡb) => runtime_package.summarize⁔(Ꮡb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nuint, nuint) find(this ж<pallocBits> Ꮡb, uintptr npages, nuint searchIdx) => runtime_package.find⁔(Ꮡb, npages, searchIdx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nuint find1(this ж<pallocBits> Ꮡb, nuint searchIdx) => runtime_package.find1⁔(Ꮡb, searchIdx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nuint, nuint) findSmallN(this ж<pallocBits> Ꮡb, uintptr npages, nuint searchIdx) => runtime_package.findSmallN⁔(Ꮡb, npages, searchIdx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nuint, nuint) findLargeN(this ж<pallocBits> Ꮡb, uintptr npages, nuint searchIdx) => runtime_package.findLargeN⁔(Ꮡb, npages, searchIdx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void allocRange(this ж<pallocBits> Ꮡb, nuint i, nuint n) => runtime_package.allocRange⁔(Ꮡb, i, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void allocAll(this ж<pallocBits> Ꮡb) => runtime_package.allocAll⁔(Ꮡb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void free1(this ж<pallocBits> Ꮡb, nuint i) => runtime_package.free1⁔(Ꮡb, i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void free(this ж<pallocBits> Ꮡb, nuint i, nuint n) => runtime_package.free⁔(Ꮡb, i, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void freeAll(this ж<pallocBits> Ꮡb) => runtime_package.freeAll⁔(Ꮡb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint64 pages64(this ж<pallocBits> Ꮡb, nuint i) => runtime_package.pages64⁔(Ꮡb, i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void allocPages64(this ж<pallocBits> Ꮡb, nuint i, uint64 alloc) => runtime_package.allocPages64⁔(Ꮡb, i, alloc);
}
partial class runtimeᴛpallocData {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nuint, nuint) findScavengeCandidate(this ref pallocData m, nuint searchIdx, uintptr minimum, uintptr max) => runtime_package.findScavengeCandidate⁔(ref m, searchIdx, minimum, max);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void allocRange(this ж<pallocData> Ꮡm, nuint i, nuint n) => runtime_package.allocRange⁔(Ꮡm, i, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void allocAll(this ж<pallocData> Ꮡm) => runtime_package.allocAll⁔(Ꮡm);
}
partial class runtimeᴛpallocSum {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nuint start(this pallocSum p) => runtime_package.start⁔(p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nuint max(this pallocSum p) => runtime_package.max⁔(p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nuint end(this pallocSum p) => runtime_package.end⁔(p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nuint, nuint, nuint) unpack(this pallocSum p) => runtime_package.unpack⁔(p);
}
partial class runtimeᴛpiController {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (float64, bool) next(this ref piController c, float64 input, float64 setpoint, float64 period) => runtime_package.next⁔(ref c, input, setpoint, period);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void reset(this ref piController c) => runtime_package.reset⁔(ref c);
}
partial class runtimeᴛpinState {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool isPinned(this ref pinState v) => runtime_package.isPinned⁔(ref v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool isMultiPinned(this ref pinState v) => runtime_package.isMultiPinned⁔(ref v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void setPinned(this ref pinState v, bool val) => runtime_package.setPinned⁔(ref v, val);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void setMultiPinned(this ref pinState v, bool val) => runtime_package.setMultiPinned⁔(ref v, val);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void set(this ref pinState v, bool val, bool multipin) => runtime_package.set⁔(ref v, val, multipin);
}
partial class runtimeᴛpinner {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void unpin(this ж<pinner> Ꮡp) => runtime_package.unpin⁔(Ꮡp);
}
partial class runtimeᴛpinnerBits {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static pinState ofObject(this ж<pinnerBits> Ꮡp, uintptr n) => runtime_package.ofObject⁔(Ꮡp, n);
}
partial class runtimeᴛplainError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void RuntimeError(this plainError e) => runtime_package.RuntimeError⁔(e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Error(this plainError e) => runtime_package.Error⁔(e);
}
partial class runtimeᴛpollCache {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void free(this ж<pollCache> Ꮡc, ж<pollDesc> Ꮡpd) => runtime_package.free⁔(Ꮡc, Ꮡpd);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<pollDesc> alloc(this ж<pollCache> Ꮡc) => runtime_package.alloc⁔(Ꮡc);
}
partial class runtimeᴛpollDesc {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static pollInfo info(this ж<pollDesc> Ꮡpd) => runtime_package.info⁔(Ꮡpd);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void publishInfo(this ж<pollDesc> Ꮡpd) => runtime_package.publishInfo⁔(Ꮡpd);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void setEventErr(this ж<pollDesc> Ꮡpd, bool b, uintptr seq) => runtime_package.setEventErr⁔(Ꮡpd, b, seq);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static any /*i*/ makeArg(this ж<pollDesc> Ꮡpd) => runtime_package.makeArg⁔(Ꮡpd);
}
partial class runtimeᴛpollInfo {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool closing(this pollInfo i) => runtime_package.closing⁔(i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool eventErr(this pollInfo i) => runtime_package.eventErr⁔(i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool expiredReadDeadline(this pollInfo i) => runtime_package.expiredReadDeadline⁔(i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool expiredWriteDeadline(this pollInfo i) => runtime_package.expiredWriteDeadline⁔(i);
}
partial class runtimeᴛprofAtomic {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static profIndex load(this ж<profAtomic> Ꮡx) => runtime_package.load⁔(Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void store(this ж<profAtomic> Ꮡx, profIndex @new) => runtime_package.store⁔(Ꮡx, @new);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool cas(this ж<profAtomic> Ꮡx, profIndex old, profIndex @new) => runtime_package.cas⁔(Ꮡx, old, @new);
}
partial class runtimeᴛprofBuf {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool hasOverflow(this ж<profBuf> Ꮡb) => runtime_package.hasOverflow⁔(Ꮡb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (uint32 count, uint64 time) takeOverflow(this ж<profBuf> Ꮡb) => runtime_package.takeOverflow⁔(Ꮡb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void incrementOverflow(this ж<profBuf> Ꮡb, int64 now) => runtime_package.incrementOverflow⁔(Ꮡb, now);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool canWriteRecord(this ж<profBuf> Ꮡb, nint nstk) => runtime_package.canWriteRecord⁔(Ꮡb, nstk);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool canWriteTwoRecords(this ж<profBuf> Ꮡb, nint nstk1, nint nstk2) => runtime_package.canWriteTwoRecords⁔(Ꮡb, nstk1, nstk2);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void close(this ж<profBuf> Ꮡb) => runtime_package.close⁔(Ꮡb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void wakeupExtra(this ж<profBuf> Ꮡb) => runtime_package.wakeupExtra⁔(Ꮡb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<uint64> data, slice<unsafe_package.Pointer> tags, bool eof) read(this ж<profBuf> Ꮡb, profBufReadMode mode) => runtime_package.read⁔(Ꮡb, mode);
}
partial class runtimeᴛprofIndex {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint32 dataCount(this profIndex x) => runtime_package.dataCount⁔(x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint32 tagCount(this profIndex x) => runtime_package.tagCount⁔(x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static profIndex addCountsAndClearFlags(this profIndex x, nint data, nint tag) => runtime_package.addCountsAndClearFlags⁔(x, data, tag);
}
partial class runtimeᴛrandomEnum {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool done(this ref randomEnum @enum) => runtime_package.done⁔(ref @enum);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void next(this ref randomEnum @enum) => runtime_package.next⁔(ref @enum);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint32 position(this ref randomEnum @enum) => runtime_package.position⁔(ref @enum);
}
partial class runtimeᴛrandomOrder {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void reset(this ref randomOrder ord, uint32 count) => runtime_package.reset⁔(ref ord, count);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static randomEnum start(this ref randomOrder ord, uint32 i) => runtime_package.start⁔(ref ord, i);
}
partial class runtimeᴛrtype {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<uncommontype> uncommon(this rtype t) => runtime_package.uncommon⁔(t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string name(this rtype t) => runtime_package.name⁔(t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string pkgpath(this rtype t) => runtime_package.pkgpath⁔(t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static name nameOff(this rtype t, nameOff off) => runtime_package.nameOff⁔(t, off);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<_type> typeOff(this rtype t, typeOff off) => runtime_package.typeOff⁔(t, off);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static unsafe_package.Pointer textOff(this rtype t, textOff off) => runtime_package.textOff⁔(t, off);
}
partial class runtimeᴛrwmutex {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void init(this ж<rwmutex> Ꮡrw, lockRank readRank, lockRank readRankInternal, lockRank writeRank) => runtime_package.init⁔(Ꮡrw, readRank, readRankInternal, writeRank);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void rlock(this ж<rwmutex> Ꮡrw) => runtime_package.rlock⁔(Ꮡrw);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void runlock(this ж<rwmutex> Ꮡrw) => runtime_package.runlock⁔(Ꮡrw);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void @lock(this ж<rwmutex> Ꮡrw) => runtime_package.lock⁔(Ꮡrw);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void unlock(this ж<rwmutex> Ꮡrw) => runtime_package.unlock⁔(Ꮡrw);
}
partial class runtimeᴛscavChunkData {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint64 pack(this scavChunkData sc) => runtime_package.pack⁔(sc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool shouldScavenge(this scavChunkData sc, uint32 currGen, bool force) => runtime_package.shouldScavenge⁔(sc, currGen, force);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void alloc(this ref scavChunkData sc, nuint npages, uint32 newGen) => runtime_package.alloc⁔(ref sc, npages, newGen);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void free(this ref scavChunkData sc, nuint npages, uint32 newGen) => runtime_package.free⁔(ref sc, npages, newGen);
}
partial class runtimeᴛscavChunkFlags {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool isEmpty(this ref scavChunkFlags sc) => runtime_package.isEmpty⁔(ref sc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void setEmpty(this ref scavChunkFlags sc) => runtime_package.setEmpty⁔(ref sc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void setNonEmpty(this ref scavChunkFlags sc) => runtime_package.setNonEmpty⁔(ref sc);
}
partial class runtimeᴛscavengeIndex {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uintptr init(this ж<scavengeIndex> Ꮡs, bool test, ж<sysMemStat> ᏑsysStat) => runtime_package.init⁔(Ꮡs, test, ᏑsysStat);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uintptr grow(this ж<scavengeIndex> Ꮡs, uintptr @base, uintptr limit, ж<sysMemStat> ᏑsysStat) => runtime_package.grow⁔(Ꮡs, @base, limit, ᏑsysStat);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (chunkIdx, nuint) find(this ж<scavengeIndex> Ꮡs, bool force) => runtime_package.find⁔(Ꮡs, force);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void alloc(this ref scavengeIndex s, chunkIdx ci, nuint npages) => runtime_package.alloc⁔(ref s, ci, npages);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void free(this ж<scavengeIndex> Ꮡs, chunkIdx ci, nuint page, nuint npages) => runtime_package.free⁔(Ꮡs, ci, page, npages);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void nextGen(this ж<scavengeIndex> Ꮡs) => runtime_package.nextGen⁔(Ꮡs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void setEmpty(this ref scavengeIndex s, chunkIdx ci) => runtime_package.setEmpty⁔(ref s, ci);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uintptr sysGrow(this ж<scavengeIndex> Ꮡs, uintptr @base, uintptr limit, ж<sysMemStat> ᏑsysStat) => runtime_package.sysGrow⁔(Ꮡs, @base, limit, ᏑsysStat);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uintptr sysInit(this ref scavengeIndex s, bool test, ж<sysMemStat> ᏑsysStat) => runtime_package.sysInit⁔(ref s, test, ᏑsysStat);
}
partial class runtimeᴛscavengerState {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void init(this ж<scavengerState> Ꮡs) => runtime_package.init⁔(Ꮡs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void park(this ж<scavengerState> Ꮡs) => runtime_package.park⁔(Ꮡs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void ready(this ж<scavengerState> Ꮡs) => runtime_package.ready⁔(Ꮡs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void wake(this ж<scavengerState> Ꮡs) => runtime_package.wake⁔(Ꮡs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void sleep(this ж<scavengerState> Ꮡs, float64 worked) => runtime_package.sleep⁔(Ꮡs, worked);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void controllerFailed(this ж<scavengerState> Ꮡs) => runtime_package.controllerFailed⁔(Ꮡs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (uintptr released, float64 worked) run(this ж<scavengerState> Ꮡs) => runtime_package.run⁔(Ꮡs);
}
partial class runtimeᴛsemTable {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<semaRoot> rootFor(this ref semTable t, ж<uint32> Ꮡaddr) => runtime_package.rootFor⁔(ref t, Ꮡaddr);
}
partial class runtimeᴛsemaRoot {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void queue(this ж<semaRoot> Ꮡroot, ж<uint32> Ꮡaddr, ж<sudog> Ꮡs, bool lifo) => runtime_package.queue⁔(Ꮡroot, Ꮡaddr, Ꮡs, lifo);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<sudog> found, int64 now, int64 tailtime) dequeue(this ж<semaRoot> Ꮡroot, ж<uint32> Ꮡaddr) => runtime_package.dequeue⁔(Ꮡroot, Ꮡaddr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void rotateLeft(this ref semaRoot root, ж<sudog> Ꮡx) => runtime_package.rotateLeft⁔(ref root, Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void rotateRight(this ref semaRoot root, ж<sudog> Ꮡy) => runtime_package.rotateRight⁔(ref root, Ꮡy);
}
partial class runtimeᴛspanAllocType {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool manual(this spanAllocType s) => runtime_package.manual⁔(s);
}
partial class runtimeᴛspanClass {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int8 sizeclass(this spanClass sc) => runtime_package.sizeclass⁔(sc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool noscan(this spanClass sc) => runtime_package.noscan⁔(sc);
}
partial class runtimeᴛspanInlineMarkBits {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void init(this ж<spanInlineMarkBits> Ꮡimb, spanClass @class, bool needzero) => runtime_package.init⁔(Ꮡimb, @class, needzero);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool tryAcquire(this ж<spanInlineMarkBits> Ꮡimb) => runtime_package.tryAcquire⁔(Ꮡimb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static spanScanOwnership release(this ж<spanInlineMarkBits> Ꮡimb) => runtime_package.release⁔(Ꮡimb);
}
partial class runtimeᴛspanQueue {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool /*ok*/ putFast(this ref spanQueue q, objptr s) => runtime_package.putFast⁔(ref q, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool put(this ж<spanQueue> Ꮡq, objptr s) => runtime_package.put⁔(Ꮡq, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void flush(this ж<spanQueue> Ꮡq) => runtime_package.flush⁔(Ꮡq);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool empty(this ж<spanQueue> Ꮡq) => runtime_package.empty⁔(Ꮡq);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool chainEmpty(this ж<spanQueue> Ꮡq) => runtime_package.chainEmpty⁔(Ꮡq);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void drain(this ж<spanQueue> Ꮡq, uint32 n) => runtime_package.drain⁔(Ꮡq, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool tryDrain(this ref spanQueue q, ж<spanSPMC> Ꮡr, uint32 n) => runtime_package.tryDrain⁔(ref q, Ꮡr, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static objptr tryGetFast(this ref spanQueue q) => runtime_package.tryGetFast⁔(ref q);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static objptr steal(this ref spanQueue q, ж<spanQueue> Ꮡq2) => runtime_package.steal⁔(ref q, Ꮡq2);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static objptr refill(this ref spanQueue q, ж<spanSPMC> Ꮡr) => runtime_package.refill⁔(ref q, Ꮡr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void destroy(this ж<spanQueue> Ꮡq) => runtime_package.destroy⁔(Ꮡq);
}
partial class runtimeᴛspanSPMC {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool empty(this ж<spanSPMC> Ꮡr) => runtime_package.empty⁔(Ꮡr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void deinit(this ж<spanSPMC> Ꮡr) => runtime_package.deinit⁔(Ꮡr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<objptr> slot(this ref spanSPMC r, uint32 i) => runtime_package.slot⁔(ref r, i);
}
partial class runtimeᴛspanScanOwnership {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static spanScanOwnership load(this ж<spanScanOwnership> Ꮡo) => runtime_package.load⁔(Ꮡo);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static spanScanOwnership or(this ж<spanScanOwnership> Ꮡo, spanScanOwnership v) => runtime_package.or⁔(Ꮡo, v);
}
partial class runtimeᴛspanSet {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void push(this ж<spanSet> Ꮡb, ж<mspan> Ꮡs) => runtime_package.push⁔(Ꮡb, Ꮡs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<mspan> pop(this ж<spanSet> Ꮡb) => runtime_package.pop⁔(Ꮡb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void reset(this ж<spanSet> Ꮡb) => runtime_package.reset⁔(Ꮡb);
}
partial class runtimeᴛspanSetBlockAlloc {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<spanSetBlock> alloc(this ж<spanSetBlockAlloc> Ꮡp) => runtime_package.alloc⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void free(this ж<spanSetBlockAlloc> Ꮡp, ж<spanSetBlock> Ꮡblock) => runtime_package.free⁔(Ꮡp, Ꮡblock);
}
partial class runtimeᴛspanSetSpinePointer {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<atomic.Pointer<spanSetBlock>> lookup(this spanSetSpinePointer s, uintptr idx) => runtime_package.lookup⁔(s, idx);
}
partial class runtimeᴛspecialsIter {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool valid(this ref specialsIter i) => runtime_package.valid⁔(ref i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void next(this ref specialsIter i) => runtime_package.next⁔(ref i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<special> unlinkAndNext(this ref specialsIter i) => runtime_package.unlinkAndNext⁔(ref i);
}
partial class runtimeᴛsrcFunc {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string name(this srcFunc s) => runtime_package.name⁔(s);
}
partial class runtimeᴛstackObject {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void setRecord(this ж<stackObject> Ꮡobj, ж<stackObjectRecord> Ꮡr) => runtime_package.setRecord⁔(Ꮡobj, Ꮡr);
}
partial class runtimeᴛstackObjectRecord {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (uintptr, ж<byte>) gcdata(this ж<stackObjectRecord> Ꮡr) => runtime_package.gcdata⁔(Ꮡr);
}
partial class runtimeᴛstackScanState {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void putPtr(this ж<stackScanState> Ꮡs, uintptr p, bool conservative) => runtime_package.putPtr⁔(Ꮡs, p, conservative);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (uintptr p, bool conservative) getPtr(this ж<stackScanState> Ꮡs) => runtime_package.getPtr⁔(Ꮡs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void addObject(this ref stackScanState s, uintptr addr, ж<stackObjectRecord> Ꮡr) => runtime_package.addObject⁔(ref s, addr, Ꮡr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void buildIndex(this ref stackScanState s) => runtime_package.buildIndex⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<stackObject> findObject(this ref stackScanState s, uintptr a) => runtime_package.findObject⁔(ref s, a);
}
partial class runtimeᴛstatAggregate {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void ensure(this ж<statAggregate> Ꮡa, ж<statDepSet> Ꮡdeps) => runtime_package.ensure⁔(Ꮡa, Ꮡdeps);
}
partial class runtimeᴛstatDepSet {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static statDepSet difference(this statDepSet s, statDepSet b) => runtime_package.difference⁔(s, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static statDepSet union(this statDepSet s, statDepSet b) => runtime_package.union⁔(s, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool empty(this ref statDepSet s) => runtime_package.empty⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool has(this ref statDepSet s, statDep d) => runtime_package.has⁔(ref s, d);
}
partial class runtimeᴛstkframe {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uintptr argBytes(this ref stkframe frame) => runtime_package.argBytes⁔(ref frame);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (bitvector argMap, bool hasReflectStackObj) argMapInternal(this ref stkframe frame) => runtime_package.argMapInternal⁔(ref frame);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (bitvector locals, bitvector args, slice<stackObjectRecord> objs) getStackMap(this ref stkframe frame, bool debug) => runtime_package.getStackMap⁔(ref frame, debug);
}
partial class runtimeᴛstwReason {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string String(this stwReason r) => runtime_package.String⁔(r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool isGC(this stwReason r) => runtime_package.isGC⁔(r);
}
partial class runtimeᴛsweepClass {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static sweepClass load(this ж<sweepClass> Ꮡs) => runtime_package.load⁔(Ꮡs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void update(this ж<sweepClass> Ꮡs, sweepClass sNew) => runtime_package.update⁔(Ꮡs, sNew);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void clear(this ж<sweepClass> Ꮡs) => runtime_package.clear⁔(Ꮡs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (spanClass spc, bool full) split(this sweepClass s) => runtime_package.split⁔(s);
}
partial class runtimeᴛsweepLocked {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool sweep(this ref sweepLocked sl, bool preserve) => runtime_package.sweep⁔(ref sl, preserve);
}
partial class runtimeᴛsweepLocker {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (sweepLocked, bool) tryAcquire(this ref sweepLocker l, ж<mspan> Ꮡs) => runtime_package.tryAcquire⁔(ref l, Ꮡs);
}
partial class runtimeᴛsynctestBubble {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void changegstatus(this ж<synctestBubble> Ꮡbubble, ж<g> Ꮡgp, uint32 oldval, uint32 newval) => runtime_package.changegstatus⁔(Ꮡbubble, Ꮡgp, oldval, newval);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void incActive(this ж<synctestBubble> Ꮡbubble) => runtime_package.incActive⁔(Ꮡbubble);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void decActive(this ж<synctestBubble> Ꮡbubble) => runtime_package.decActive⁔(Ꮡbubble);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<g> maybeWakeLocked(this ж<synctestBubble> Ꮡbubble) => runtime_package.maybeWakeLocked⁔(Ꮡbubble);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static unsafe_package.Pointer raceaddr(this ж<synctestBubble> Ꮡbubble) => runtime_package.raceaddr⁔(Ꮡbubble);
}
partial class runtimeᴛsynctestDeadlockError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Error(this synctestDeadlockError e) => runtime_package.Error⁔(e);
}
partial class runtimeᴛsysMemStat {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint64 load(this ж<sysMemStat> Ꮡs) => runtime_package.load⁔(Ꮡs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void add(this ж<sysMemStat> Ꮡs, int64 n) => runtime_package.add⁔(Ꮡs, n);
}
partial class runtimeᴛsysStatsAggregate {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void compute(this ж<sysStatsAggregate> Ꮡa) => runtime_package.compute⁔(Ꮡa);
}
partial class runtimeᴛsyscallingThread {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void gcstopP(this syscallingThread s) => runtime_package.gcstopP⁔(s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void takeP(this syscallingThread s) => runtime_package.takeP⁔(s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void releaseP(this syscallingThread s, uint32 state) => runtime_package.releaseP⁔(s, state);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void resume(this syscallingThread s) => runtime_package.resume⁔(s);
}
partial class runtimeᴛtaggedPointer {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static unsafe_package.Pointer pointer(this taggedPointer tp) => runtime_package.pointer⁔(tp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uintptr tag(this taggedPointer tp) => runtime_package.tag⁔(tp);
}
partial class runtimeᴛticksType {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void init(this ref ticksType t) => runtime_package.init⁔(ref t);
}
partial class runtimeᴛtimeHistogram {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void record(this ж<timeHistogram> Ꮡh, int64 duration) => runtime_package.record⁔(Ꮡh, duration);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void write(this ж<timeHistogram> Ꮡh, ж<metricValue> Ꮡout) => runtime_package.write⁔(Ꮡh, Ꮡout);
}
partial class runtimeᴛtimer {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void init(this ж<timer> Ꮡt, Action<any, uintptr, int64> f, any arg) => runtime_package.init⁔(Ꮡt, f, arg);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void trace(this ж<timer> Ꮡt, @string op) => runtime_package.trace⁔(Ꮡt, op);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void trace1(this ж<timer> Ꮡt, @string op) => runtime_package.trace1⁔(Ꮡt, op);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void @lock(this ж<timer> Ꮡt) => runtime_package.lock⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void unlock(this ж<timer> Ꮡt) => runtime_package.unlock⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<hchan> hchan(this ж<timer> Ꮡt) => runtime_package.hchan⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool /*updated*/ updateHeap(this ж<timer> Ꮡt) => runtime_package.updateHeap⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void maybeRunAsync(this ж<timer> Ꮡt) => runtime_package.maybeRunAsync⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool stop(this ж<timer> Ꮡt) => runtime_package.stop⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool modify(this ж<timer> Ꮡt, int64 when, int64 period, Action<any, uintptr, int64> f, any arg, uintptr seq) => runtime_package.modify⁔(Ꮡt, when, period, f, arg, seq);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool needsAdd(this ж<timer> Ꮡt) => runtime_package.needsAdd⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void maybeAdd(this ж<timer> Ꮡt) => runtime_package.maybeAdd⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool reset(this ж<timer> Ꮡt, int64 when, int64 period) => runtime_package.reset⁔(Ꮡt, when, period);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void unlockAndRun(this ж<timer> Ꮡt, int64 now, ж<synctestBubble> Ꮡbubble) => runtime_package.unlockAndRun⁔(Ꮡt, now, Ꮡbubble);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void maybeRunChan(this ж<timer> Ꮡt, ж<hchan> Ꮡc) => runtime_package.maybeRunChan⁔(Ꮡt, Ꮡc);
}
partial class runtimeᴛtimerWhen {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool less(this timerWhen tw, timerWhen other) => runtime_package.less⁔(tw, other);
}
partial class runtimeᴛtimers {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void @lock(this ж<timers> Ꮡts) => runtime_package.lock⁔(Ꮡts);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void unlock(this ж<timers> Ꮡts) => runtime_package.unlock⁔(Ꮡts);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void trace(this ж<timers> Ꮡts, @string op) => runtime_package.trace⁔(Ꮡts, op);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void addHeap(this ж<timers> Ꮡts, ж<timer> Ꮡt) => runtime_package.addHeap⁔(Ꮡts, Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void deleteMin(this ж<timers> Ꮡts) => runtime_package.deleteMin⁔(Ꮡts);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void cleanHead(this ж<timers> Ꮡts) => runtime_package.cleanHead⁔(Ꮡts);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void take(this ж<timers> Ꮡts, ж<timers> Ꮡsrc) => runtime_package.take⁔(Ꮡts, Ꮡsrc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void adjust(this ж<timers> Ꮡts, int64 now, bool force) => runtime_package.adjust⁔(Ꮡts, now, force);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int64 wakeTime(this ж<timers> Ꮡts) => runtime_package.wakeTime⁔(Ꮡts);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (int64 rnow, int64 pollUntil, bool ran) check(this ж<timers> Ꮡts, int64 now, ж<synctestBubble> Ꮡbubble) => runtime_package.check⁔(Ꮡts, now, Ꮡbubble);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int64 run(this ж<timers> Ꮡts, int64 now, ж<synctestBubble> Ꮡbubble) => runtime_package.run⁔(Ꮡts, now, Ꮡbubble);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void verify(this ж<timers> Ꮡts) => runtime_package.verify⁔(Ꮡts);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void updateMinWhenHeap(this ж<timers> Ꮡts) => runtime_package.updateMinWhenHeap⁔(Ꮡts);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void updateMinWhenModified(this ж<timers> Ꮡts, int64 when) => runtime_package.updateMinWhenModified⁔(Ꮡts, when);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void siftUp(this ref timers ts, nint i) => runtime_package.siftUp⁔(ref ts, i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void siftDown(this ref timers ts, nint i) => runtime_package.siftDown⁔(ref ts, i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void initHeap(this ref timers ts) => runtime_package.initHeap⁔(ref ts);
}
partial class runtimeᴛtraceAdvancerState {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void start(this ж<traceAdvancerState> Ꮡs) => runtime_package.start⁔(Ꮡs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void stop(this ref traceAdvancerState s) => runtime_package.stop⁔(ref s);
}
partial class runtimeᴛtraceBuf {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void @byte(this ref traceBuf buf, byte v) => runtime_package.byte⁔(ref buf, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void varint(this ref traceBuf buf, uint64 v) => runtime_package.varint⁔(ref buf, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint varintReserve(this ref traceBuf buf) => runtime_package.varintReserve⁔(ref buf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void stringData(this ref traceBuf buf, @string s) => runtime_package.stringData⁔(ref buf, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool available(this ref traceBuf buf, nint size) => runtime_package.available⁔(ref buf, size);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void varintAt(this ref traceBuf buf, nint pos, uint64 v) => runtime_package.varintAt⁔(ref buf, pos, v);
}
partial class runtimeᴛtraceBufQueue {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void push(this ref traceBufQueue q, ж<traceBuf> Ꮡbuf) => runtime_package.push⁔(ref q, Ꮡbuf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<traceBuf> pop(this ref traceBufQueue q) => runtime_package.pop⁔(ref q);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool empty(this ref traceBufQueue q) => runtime_package.empty⁔(ref q);
}
partial class runtimeᴛtraceEventWriter {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void @event(this traceEventWriter e, tracev2.EventType ev, params ꓸꓸꓸtraceArg argsʗp) => runtime_package.event⁔(e, ev, argsʗp);
}
partial class runtimeᴛtraceLocker {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void SpanExists(this traceLocker tl, ж<mspan> Ꮡs) => runtime_package.SpanExists⁔(tl, Ꮡs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void SpanAlloc(this traceLocker tl, ж<mspan> Ꮡs) => runtime_package.SpanAlloc⁔(tl, Ꮡs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void SpanFree(this traceLocker tl, ж<mspan> Ꮡs) => runtime_package.SpanFree⁔(tl, Ꮡs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void HeapObjectExists(this traceLocker tl, uintptr addr, ж<abi.Type> Ꮡtyp) => runtime_package.HeapObjectExists⁔(tl, addr, Ꮡtyp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void HeapObjectAlloc(this traceLocker tl, uintptr addr, ж<abi.Type> Ꮡtyp) => runtime_package.HeapObjectAlloc⁔(tl, addr, Ꮡtyp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void HeapObjectFree(this traceLocker tl, uintptr addr) => runtime_package.HeapObjectFree⁔(tl, addr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void GoroutineStackExists(this traceLocker tl, uintptr @base, uintptr size) => runtime_package.GoroutineStackExists⁔(tl, @base, size);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void GoroutineStackAlloc(this traceLocker tl, uintptr @base, uintptr size) => runtime_package.GoroutineStackAlloc⁔(tl, @base, size);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void GoroutineStackFree(this traceLocker tl, uintptr @base) => runtime_package.GoroutineStackFree⁔(tl, @base);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static traceWriter writer(this traceLocker tl) => runtime_package.writer⁔(tl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static traceWriter expWriter(this traceLocker tl, tracev2.Experiment exp) => runtime_package.expWriter⁔(tl, exp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static traceEventWriter eventWriter(this traceLocker tl, tracev2.GoStatus goStatus, tracev2.ProcStatus procStatus) => runtime_package.eventWriter⁔(tl, goStatus, procStatus);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static traceEventWriter rawEventWriter(this traceLocker tl) => runtime_package.rawEventWriter⁔(tl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static traceArg stack(this traceLocker tl, nint skip) => runtime_package.stack⁔(tl, skip);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static traceArg startPC(this traceLocker tl, uintptr pc) => runtime_package.startPC⁔(tl, pc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static traceArg @string(this traceLocker tl, @string s) => runtime_package.string⁔(tl, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static traceArg uniqueString(this traceLocker tl, @string s) => runtime_package.uniqueString⁔(tl, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static traceArg rtype(this traceLocker tl, ж<abi.Type> Ꮡtyp) => runtime_package.rtype⁔(tl, Ꮡtyp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool ok(this traceLocker tl) => runtime_package.ok⁔(tl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void Gomaxprocs(this traceLocker tl, int32 procs) => runtime_package.Gomaxprocs⁔(tl, procs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void ProcStart(this traceLocker tl) => runtime_package.ProcStart⁔(tl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void ProcStop(this traceLocker tl, ж<p> Ꮡpp) => runtime_package.ProcStop⁔(tl, Ꮡpp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void GCActive(this traceLocker tl) => runtime_package.GCActive⁔(tl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void GCStart(this traceLocker tl) => runtime_package.GCStart⁔(tl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void GCDone(this traceLocker tl) => runtime_package.GCDone⁔(tl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void STWStart(this traceLocker tl, stwReason reason) => runtime_package.STWStart⁔(tl, reason);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void STWDone(this traceLocker tl) => runtime_package.STWDone⁔(tl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void GCSweepStart(this traceLocker tl) => runtime_package.GCSweepStart⁔(tl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void GCSweepSpan(this traceLocker tl, uintptr bytesSwept) => runtime_package.GCSweepSpan⁔(tl, bytesSwept);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void GCSweepDone(this traceLocker tl) => runtime_package.GCSweepDone⁔(tl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void GCMarkAssistStart(this traceLocker tl) => runtime_package.GCMarkAssistStart⁔(tl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void GCMarkAssistDone(this traceLocker tl) => runtime_package.GCMarkAssistDone⁔(tl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void GoCreate(this traceLocker tl, ж<g> Ꮡnewg, uintptr pc, bool blocked) => runtime_package.GoCreate⁔(tl, Ꮡnewg, pc, blocked);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void GoStart(this traceLocker tl) => runtime_package.GoStart⁔(tl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void GoEnd(this traceLocker tl) => runtime_package.GoEnd⁔(tl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void GoSched(this traceLocker tl) => runtime_package.GoSched⁔(tl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void GoPreempt(this traceLocker tl) => runtime_package.GoPreempt⁔(tl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void GoStop(this traceLocker tl, traceGoStopReason reason) => runtime_package.GoStop⁔(tl, reason);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void GoPark(this traceLocker tl, traceBlockReason reason, nint skip) => runtime_package.GoPark⁔(tl, reason, skip);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void GoUnpark(this traceLocker tl, ж<g> Ꮡgp, nint skip) => runtime_package.GoUnpark⁔(tl, Ꮡgp, skip);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void GoSwitch(this traceLocker tl, ж<g> Ꮡnextg, bool destroy) => runtime_package.GoSwitch⁔(tl, Ꮡnextg, destroy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void emitUnblockStatus(this traceLocker tl, ж<g> Ꮡgp, uintptr gen) => runtime_package.emitUnblockStatus⁔(tl, Ꮡgp, gen);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void GoSysCall(this traceLocker tl) => runtime_package.GoSysCall⁔(tl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void GoSysExit(this traceLocker tl, bool lostP) => runtime_package.GoSysExit⁔(tl, lostP);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void ProcSteal(this traceLocker tl, ж<p> Ꮡpp) => runtime_package.ProcSteal⁔(tl, Ꮡpp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void HeapAlloc(this traceLocker tl, uint64 live) => runtime_package.HeapAlloc⁔(tl, live);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void HeapGoal(this traceLocker tl) => runtime_package.HeapGoal⁔(tl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void GoCreateSyscall(this traceLocker tl, ж<g> Ꮡgp) => runtime_package.GoCreateSyscall⁔(tl, Ꮡgp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void GoDestroySyscall(this traceLocker tl) => runtime_package.GoDestroySyscall⁔(tl);
}
partial class runtimeᴛtraceMap {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint64 stealID(this ж<traceMap> Ꮡtab) => runtime_package.stealID⁔(Ꮡtab);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (uint64, bool) put(this ж<traceMap> Ꮡtab, unsafe_package.Pointer data, uintptr size) => runtime_package.put⁔(Ꮡtab, data, size);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void reset(this ж<traceMap> Ꮡtab) => runtime_package.reset⁔(Ꮡtab);
}
partial class runtimeᴛtraceRegionAlloc {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<notInHeap> alloc(this ж<traceRegionAlloc> Ꮡa, uintptr n) => runtime_package.alloc⁔(Ꮡa, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void drop(this ж<traceRegionAlloc> Ꮡa) => runtime_package.drop⁔(Ꮡa);
}
partial class runtimeᴛtraceSchedResourceState {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool acquireStatus(this ref traceSchedResourceState r, uintptr gen) => runtime_package.acquireStatus⁔(ref r, gen);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void readyNextGen(this ref traceSchedResourceState r, uintptr gen) => runtime_package.readyNextGen⁔(ref r, gen);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool statusWasTraced(this ref traceSchedResourceState r, uintptr gen) => runtime_package.statusWasTraced⁔(ref r, gen);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void setStatusTraced(this ref traceSchedResourceState r, uintptr gen) => runtime_package.setStatusTraced⁔(ref r, gen);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static traceArg nextSeq(this ref traceSchedResourceState r, uintptr gen) => runtime_package.nextSeq⁔(ref r, gen);
}
partial class runtimeᴛtraceStackTable {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint64 put(this ж<traceStackTable> Ꮡt, slice<uintptr> pcs) => runtime_package.put⁔(Ꮡt, pcs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void dump(this ж<traceStackTable> Ꮡt, uintptr gen) => runtime_package.dump⁔(Ꮡt, gen);
}
partial class runtimeᴛtraceStringTable {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint64 put(this ж<traceStringTable> Ꮡt, uintptr gen, @string sʗp) => runtime_package.put⁔(Ꮡt, gen, sʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint64 emit(this ж<traceStringTable> Ꮡt, uintptr gen, @string s) => runtime_package.emit⁔(Ꮡt, gen, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void writeString(this ж<traceStringTable> Ꮡt, uintptr gen, uint64 id, @string s) => runtime_package.writeString⁔(Ꮡt, gen, id, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void reset(this ж<traceStringTable> Ꮡt, uintptr gen) => runtime_package.reset⁔(Ꮡt, gen);
}
partial class runtimeᴛtraceTypeTable {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint64 put(this ж<traceTypeTable> Ꮡt, ж<abi.Type> Ꮡtyp) => runtime_package.put⁔(Ꮡt, Ꮡtyp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void dump(this ж<traceTypeTable> Ꮡt, uintptr gen) => runtime_package.dump⁔(Ꮡt, gen);
}
partial class runtimeᴛtraceWriter {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static traceWriter @event(this traceWriter w, tracev2.EventType ev, params ꓸꓸꓸtraceArg argsʗp) => runtime_package.event⁔(w, ev, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void end(this traceWriter w) => runtime_package.end⁔(w);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (traceWriter, bool) ensure(this traceWriter w, nint maxSize) => runtime_package.ensure⁔(w, maxSize);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static traceWriter flush(this traceWriter w) => runtime_package.flush⁔(w);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static traceWriter refill(this traceWriter w) => runtime_package.refill⁔(w);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static traceWriter writeGoStatus(this traceWriter w, uint64 goid, int64 mid, tracev2.GoStatus status, bool markAssist, uint64 stackID) => runtime_package.writeGoStatus⁔(w, goid, mid, status, markAssist, stackID);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static traceWriter writeProcStatusForP(this traceWriter w, ж<p> Ꮡpp, bool inSTW) => runtime_package.writeProcStatusForP⁔(w, Ꮡpp, inSTW);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static traceWriter writeProcStatus(this traceWriter w, uint64 pid, tracev2.ProcStatus status, bool inSweep) => runtime_package.writeProcStatus⁔(w, pid, status, inSweep);
}
partial class runtimeᴛtypePointers {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (typePointers, uintptr) nextFast(this typePointers tp) => runtime_package.nextFast⁔(tp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (typePointers, uintptr) next(this typePointers tp, uintptr limit) => runtime_package.next⁔(tp, limit);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static typePointers fastForward(this typePointers tp, uintptr n, uintptr limit) => runtime_package.fastForward⁔(tp, n, limit);
}
partial class runtimeᴛunwinder {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void init(this ж<unwinder> Ꮡu, ж<g> Ꮡgp, unwindFlags flags) => runtime_package.init⁔(Ꮡu, Ꮡgp, flags);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void initAt(this ж<unwinder> Ꮡu, uintptr pc0, uintptr sp0, uintptr lr0, ж<g> Ꮡgp, unwindFlags flags) => runtime_package.initAt⁔(Ꮡu, pc0, sp0, lr0, Ꮡgp, flags);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool valid(this ref unwinder u) => runtime_package.valid⁔(ref u);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void resolveInternal(this ж<unwinder> Ꮡu, bool innermost, bool isSyscall) => runtime_package.resolveInternal⁔(Ꮡu, innermost, isSyscall);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void next(this ж<unwinder> Ꮡu) => runtime_package.next⁔(Ꮡu);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void finishInternal(this ref unwinder u) => runtime_package.finishInternal⁔(ref u);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uintptr symPC(this ref unwinder u) => runtime_package.symPC⁔(ref u);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint cgoCallers(this ref unwinder u, slice<uintptr> pcBuf) => runtime_package.cgoCallers⁔(ref u, pcBuf);
}
partial class runtimeᴛuserArena {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static unsafe_package.Pointer alloc(this ref userArena a, ж<_type> Ꮡtyp, nint cap) => runtime_package.alloc⁔(ref a, Ꮡtyp, cap);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<mspan> refill(this ref userArena a) => runtime_package.refill⁔(ref a);
}
partial class runtimeᴛwaitq {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void enqueue(this ref waitq q, ж<sudog> Ꮡsgp) => runtime_package.enqueue⁔(ref q, Ꮡsgp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<sudog> dequeue(this ref waitq q) => runtime_package.dequeue⁔(ref q);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void dequeueSudoG(this ref waitq q, ж<sudog> Ꮡsgp) => runtime_package.dequeueSudoG⁔(ref q, Ꮡsgp);
}
partial class runtimeᴛwakeableSleep {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void sleep(this ж<wakeableSleep> Ꮡs, int64 ns) => runtime_package.sleep⁔(Ꮡs, ns);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void wake(this ж<wakeableSleep> Ꮡs) => runtime_package.wake⁔(Ꮡs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void close(this ж<wakeableSleep> Ꮡs) => runtime_package.close⁔(Ꮡs);
}
partial class runtimeᴛwbBuf {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void reset(this ref wbBuf b) => runtime_package.reset⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void discard(this ref wbBuf b) => runtime_package.discard⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool empty(this ref wbBuf b) => runtime_package.empty⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<array<uintptr>> get1(this ref wbBuf b) => runtime_package.get1⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<array<uintptr>> get2(this ref wbBuf b) => runtime_package.get2⁔(ref b);
}
partial class runtimeᴛworkbuf {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void checknonempty(this ref workbuf b) => runtime_package.checknonempty⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void checkempty(this ref workbuf b) => runtime_package.checkempty⁔(ref b);
}
partial class runtimeᴛwriteUserArenaHeapBits {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static writeUserArenaHeapBits write(this writeUserArenaHeapBits h, ж<mspan> Ꮡs, uintptr bits, uintptr valid) => runtime_package.write⁔(h, Ꮡs, bits, valid);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static writeUserArenaHeapBits pad(this writeUserArenaHeapBits h, ж<mspan> Ꮡs, uintptr size) => runtime_package.pad⁔(h, Ꮡs, size);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void flush(this writeUserArenaHeapBits h, ж<mspan> Ꮡs, uintptr addr, uintptr size) => runtime_package.flush⁔(h, Ꮡs, addr, size);
}
partial class runtimeᴛxRegPerP {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void free(this ref xRegPerP xRegs) => runtime_package.free⁔(ref xRegs);
}
// </MethodForwarders>
