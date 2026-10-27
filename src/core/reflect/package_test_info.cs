// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.reflect_package;
global using static global::go.reflect_internal_test_package;

// <ImportedTypeAliases>
global using abiꓸArrayType = go.@internal.abi_package.ΔArrayType;
global using abiꓸChanDir = go.@internal.abi_package.ΔChanDir;
global using abiꓸFuncType = go.@internal.abi_package.ΔFuncType;
global using abiꓸInterfaceType = go.@internal.abi_package.ΔInterfaceType;
global using abiꓸKind = go.@internal.abi_package.ΔKind;
global using abiꓸMapType = go.@internal.abi_package.ΔMapType;
global using abiꓸName = go.@internal.abi_package.ΔName;
global using abiꓸStructType = go.@internal.abi_package.ΔStructType;
global using flagꓸErrorHandling = go.flag_package.ΔErrorHandling;
global using jsonꓸToken = object;
global using jsonꓸΔToken = object;
global using mapsꓸMap = go.@internal.runtime.maps_package.ΔMap;
global using netꓸAddr = go.net_package.ΔAddr;
global using netꓸError = go.net_package.ΔError;
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
global using strconvꓸError = go.@internal.strconv_package.ΔError;
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
global using tokenꓸFile = go.go.token_package.ΔFile;
global using tokenꓸPos = go.go.token_package.ΔPos;
global using tokenꓸPosition = go.go.token_package.ΔPosition;
// </ImportedTypeAliases>

using go;
using static global::go.reflect_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("696e746572666163657b4128297d", "TestTypeAssert_type")]
[assembly: GoDynamicTypeLift("696e746572666163657b4469737428696e742920696e747d", "TestMethod_x")]
[assembly: GoDynamicTypeLift("696e746572666163657b4628297d", "Δtypeᴛ35")]
[assembly: GoDynamicTypeLift("696e746572666163657b476574282920696e747d", "TestStructOfWithInterface_Iface")]
[assembly: GoDynamicTypeLift("696e746572666163657b4e616d65282920737472696e677d", "TestStructOfEmbeddedIfaceMethodCall_Named")]
[assembly: GoDynamicTypeLift("696e746572666163657b53657428696e74297d", "TestStructOfWithInterface_IfaceSet")]
[assembly: GoDynamicTypeLift("696e746572666163657b537472696e67282920737472696e677d", "BenchmarkSetZero_type_Interface")]
[assembly: GoDynamicTypeLift("696e746572666163657b5728293b207728297d", "TestCallPanic_t0")]
[assembly: GoDynamicTypeLift("696e746572666163657b5828293b207828297d", "TestMethodPkgPath_I")]
[assembly: GoDynamicTypeLift("696e746572666163657b5928293b207928293b207265666c6563745f746573742e497d", "TestMethodPkgPath_i")]
[assembly: GoDynamicTypeLift("696e746572666163657b5928293b207928297d", "TestCallPanic_T1")]
[assembly: GoDynamicTypeLift("696e746572666163657b612866756e632866756e6328696e742920696e74292066756e632866756e6328696e74292920696e74293b206228297d", "typeᴛ33_x")]
[assembly: GoDynamicTypeLift("696e746572666163657b666f6f28297d", "TestTypeAssertAllocs_I")]
[assembly: GoDynamicTypeLift("7265666c6563745f746573742e49", "TestMethodPkgPath_I")]
[assembly: GoDynamicTypeLift("7265666c6563745f746573742e4966616365", "TestStructOfWithInterface_Iface")]
[assembly: GoDynamicTypeLift("7265666c6563745f746573742e4966616365536574", "TestStructOfWithInterface_IfaceSet")]
[assembly: GoDynamicTypeLift("7265666c6563745f746573742e4e616d6564", "TestStructOfEmbeddedIfaceMethodCall_Named")]
[assembly: GoDynamicTypeLift("7265666c6563745f746573742e5431", "TestCallPanic_T1")]
[assembly: GoDynamicTypeLift("7265666c6563745f746573742e7430", "TestCallPanic_t0")]
[assembly: GoDynamicTypeLift("7374727563747b2a7265666c6563745f746573742e53467d", "Δtypeᴛ43")]
[assembly: GoDynamicTypeLift("7374727563747b4120696e743b204220737472696e673b204320626f6f6c7d", "Δtypeᴛ37")]
[assembly: GoDynamicTypeLift("7374727563747b41207374727563747b5820696e747d7d", "Δtypeᴛ38")]
[assembly: GoDynamicTypeLift("7374727563747b426f6f6c20626f6f6c3b20496e7420696e7436343b2055696e742075696e7436343b20466c6f617420666c6f617436343b20436f6d706c657820636f6d706c65783132383b204172726179205b345d7265666c6563742e56616c75653b204368616e206368616e207265666c6563742e56616c75653b2046756e632066756e632829207265666c6563742e56616c75653b20496e7465726661636520696e746572666163657b537472696e67282920737472696e677d3b204d6170206d61705b737472696e675d7265666c6563742e56616c75653b20506f696e746572202a7265666c6563742e56616c75653b20536c696365205b5d7265666c6563742e56616c75653b20537472696e6720737472696e673b20537472756374207265666c6563742e56616c75657d", "BenchmarkSetZero_type")]
[assembly: GoDynamicTypeLift("7374727563747b426f6f6c207265666c6563742e56616c75653b20537472696e67207265666c6563742e56616c75653b204279746573207265666c6563742e56616c75653b204e616d65644279746573207265666c6563742e56616c75653b2042797465734172726179207265666c6563742e56616c75653b20536c696365416e79207265666c6563742e56616c75653b204d6170537472696e67416e79207265666c6563742e56616c75657d", "sourceAllᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b4578706f7274656420737472696e673b20756e6578706f7274656420737472696e673b207265666c6563742e4f74686572506b674669656c64733b20696e743b202a7265666c6563745f746573742e787d", "TestFieldPkgPath_i")]
[assembly: GoDynamicTypeLift("7374727563747b46207265666c6563745f746573742e7374727563744669656c64547970657d", "TestStructOf_yᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b473120627974653b204732205b305d2a627974657d", "TestStructOf_iᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b526177426f6f6c20626f6f6c3b20526177537472696e6720737472696e673b205261774279746573205b5d627974653b20526177496e7420696e747d", "sinkAllᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b537472696e6720737472696e673b205820627974653b20592075696e7436343b205a205b335d75696e7431367d", "TestStructOf_i")]
[assembly: GoDynamicTypeLift("7374727563747b546167207265666c6563742e5374727563745461673b204b657920737472696e673b2056616c756520737472696e677d", "tagGetTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b5820696e747d", "TestTypeFieldOutOfRangePanic_i")]
[assembly: GoDynamicTypeLift("7374727563747b59205b335d7374727563747b7d7d", "Struct15_X")]
[assembly: GoDynamicTypeLift("7374727563747b592075696e7436347d", "TestStructOf_y")]
[assembly: GoDynamicTypeLift("7374727563747b5f20696e7433327d", "TestIsRegularMemory_iᴛ5")]
[assembly: GoDynamicTypeLift("7374727563747b5f207265666c6563745f746573742e537d", "TestIsRegularMemory_iᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6120696e7431363b206220696e7433327d", "TestIsRegularMemory_iᴛ3")]
[assembly: GoDynamicTypeLift("7374727563747b6120696e743820227265666c6563743a5c226869205c5c78303074686572655c5c745c5c6e5c5c5c225c5c5c5c5c22227d", "typeᴛ31_x")]
[assembly: GoDynamicTypeLift("7374727563747b6120696e743820227265666c6563743a5c2268692074686572655c22227d", "typeᴛ30_x")]
[assembly: GoDynamicTypeLift("7374727563747b6120696e74383b206220696e7433327d", "typeᴛ25_x")]
[assembly: GoDynamicTypeLift("7374727563747b6120696e74383b206220696e74383b206320696e7433327d", "typeᴛ26_x")]
[assembly: GoDynamicTypeLift("7374727563747b6120696e74383b206220696e74383b206320696e74383b206420696e7433327d", "typeᴛ27_x")]
[assembly: GoDynamicTypeLift("7374727563747b6120696e74383b206220696e74383b206320696e74383b206420696e74383b206520696e7433327d", "typeᴛ28_x")]
[assembly: GoDynamicTypeLift("7374727563747b6120696e74383b206220696e74383b206320696e74383b206420696e74383b206520696e74383b206620696e7433327d", "typeᴛ29_x")]
[assembly: GoDynamicTypeLift("7374727563747b6120696e743b206220696e743b206320696e747d", "TestMakeFuncValidReturnAssignments_i")]
[assembly: GoDynamicTypeLift("7374727563747b63206368616e202a696e7433323b206420666c6f617433327d", "typeᴛ22_x")]
[assembly: GoDynamicTypeLift("7374727563747b632066756e63286368616e202a7265666c6563745f746573742e696e74656765722c202a696e7438297d", "typeᴛ24_x")]
[assembly: GoDynamicTypeLift("7374727563747b64205b5d75696e74333220227265666c6563743a5c225441475c22227d", "TestAll_i")]
[assembly: GoDynamicTypeLift("7374727563747b662066756e632861726773202e2e2e696e74297d", "typeᴛ32_x")]
[assembly: GoDynamicTypeLift("7374727563747b6620696e747d", "TestAllocations_type")]
[assembly: GoDynamicTypeLift("7374727563747b6920696e743b205f207265666c6563745f746573742e537d", "TestIsRegularMemory_iᴛ2")]
[assembly: GoDynamicTypeLift("7374727563747b6920696e743b2073207265666c6563745f746573742e537d", "TestIsRegularMemory_i")]
[assembly: GoDynamicTypeLift("7374727563747b6920696e747d", "TestCanIntUintFloatComplex_typeᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e207265666c6563742e56616c75653b206f7574207265666c6563742e56616c75657d", "convertTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e742022736f6d653a5c226261725c22227d", "MyStruct1_x")]
[assembly: GoDynamicTypeLift("7374727563747b696e742022736f6d653a5c22666f6f5c22227d", "MyStruct2_x")]
[assembly: GoDynamicTypeLift("7374727563747b696e7433323b20696e7436347d", "typeᴛ34_x")]
[assembly: GoDynamicTypeLift("7374727563747b6d206d61705b737472696e675d696e747d", "TestSetIter_i")]
[assembly: GoDynamicTypeLift("7374727563747b6f726967205b5d696e743b206578747261205b5d696e747d", "appendTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7265666c6563745f746573742e4d7d", "Δtypeᴛ44")]
[assembly: GoDynamicTypeLift("7374727563747b7265666c6563745f746573742e5346473b207265666c6563745f746573742e53467d", "Δtypeᴛ41")]
[assembly: GoDynamicTypeLift("7374727563747b7265666c6563745f746573742e53464748333b207265666c6563745f746573742e5347313b207265666c6563745f746573742e534647323b207265666c6563745f746573742e5346323b204c20696e747d", "Δtypeᴛ42")]
[assembly: GoDynamicTypeLift("7374727563747b7265666c6563745f746573742e5346477d", "Δtypeᴛ39")]
[assembly: GoDynamicTypeLift("7374727563747b7265666c6563745f746573742e7346477d", "Δtypeᴛ40")]
[assembly: GoDynamicTypeLift("7374727563747b73796e632e4d757465783b206f6e63652073796e632e4f6e63653b206e6f772074696d652e54696d653b20696e666f205b5d7265666c6563745f746573742e63617365496e666f7d", "selectWatchᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b746573744e616d6520737472696e673b2076616c20616e793b20657870656374205b5d7265666c6563745f746573742e7374727563744669656c647d", "fieldsTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b747970207265666c6563742e547970653b206f6b20626f6f6c7d", "comparableTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b78202a2a696e74387d", "Δtypeᴛ12")]
[assembly: GoDynamicTypeLift("7374727563747b78202a2a7265666c6563745f746573742e696e74656765727d", "Δtypeᴛ13")]
[assembly: GoDynamicTypeLift("7374727563747b78203c2d6368616e203c2d6368616e20737472696e677d", "Δtypeᴛ20")]
[assembly: GoDynamicTypeLift("7374727563747b78205b33325d696e7433327d", "Δtypeᴛ14")]
[assembly: GoDynamicTypeLift("7374727563747b78205b5d696e74387d", "Δtypeᴛ15")]
[assembly: GoDynamicTypeLift("7374727563747b78205b5d696e747d", "TestAppend_i")]
[assembly: GoDynamicTypeLift("7374727563747b7820616e793b207420616e793b206220626f6f6c7d", "implementsTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7820616e793b207920616e797d", "deepEqualPerfTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b78206368616e20283c2d6368616e20737472696e67297d", "Δtypeᴛ21")]
[assembly: GoDynamicTypeLift("7374727563747b78206368616e3c2d203c2d6368616e20737472696e677d", "Δtypeᴛ19")]
[assembly: GoDynamicTypeLift("7374727563747b78206368616e3c2d206368616e20737472696e677d", "Δtypeᴛ18")]
[assembly: GoDynamicTypeLift("7374727563747b78206368616e3c2d20737472696e677d", "Δtypeᴛ17")]
[assembly: GoDynamicTypeLift("7374727563747b7820666c6f617433327d", "Δtypeᴛ10")]
[assembly: GoDynamicTypeLift("7374727563747b7820666c6f617436347d", "Δtypeᴛ11")]
[assembly: GoDynamicTypeLift("7374727563747b782066756e63286120696e74382c206220696e743332297d", "Δtypeᴛ23")]
[assembly: GoDynamicTypeLift("7374727563747b7820696e742022736f6d653a5c226261725c22227d", "iᴛ2")]
[assembly: GoDynamicTypeLift("7374727563747b7820696e742022736f6d653a5c22666f6f5c22227d", "iᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7820696e7431367d", "Δtypeᴛ2")]
[assembly: GoDynamicTypeLift("7374727563747b7820696e7433323b207920696e7431367d", "TestIsRegularMemory_iᴛ4")]
[assembly: GoDynamicTypeLift("7374727563747b7820696e7433327d", "Δtypeᴛ3")]
[assembly: GoDynamicTypeLift("7374727563747b7820696e7436347d", "Δtypeᴛ4")]
[assembly: GoDynamicTypeLift("7374727563747b7820696e74387d", "Δtypeᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7820696e746572666163657b612866756e632866756e6328696e742920696e74292066756e632866756e6328696e74292920696e74293b206228297d7d", "Δtypeᴛ33")]
[assembly: GoDynamicTypeLift("7374727563747b7820696e747d", "Δtype")]
[assembly: GoDynamicTypeLift("7374727563747b78206d61705b737472696e675d696e7433327d", "Δtypeᴛ16")]
[assembly: GoDynamicTypeLift("7374727563747b78207374727563747b6120696e743820227265666c6563743a5c226869205c5c78303074686572655c5c745c5c6e5c5c5c225c5c5c5c5c22227d7d", "Δtypeᴛ31")]
[assembly: GoDynamicTypeLift("7374727563747b78207374727563747b6120696e743820227265666c6563743a5c2268692074686572655c22227d7d", "Δtypeᴛ30")]
[assembly: GoDynamicTypeLift("7374727563747b78207374727563747b6120696e74383b206220696e7433327d7d", "Δtypeᴛ25")]
[assembly: GoDynamicTypeLift("7374727563747b78207374727563747b6120696e74383b206220696e74383b206320696e7433327d7d", "Δtypeᴛ26")]
[assembly: GoDynamicTypeLift("7374727563747b78207374727563747b6120696e74383b206220696e74383b206320696e74383b206420696e7433327d7d", "Δtypeᴛ27")]
[assembly: GoDynamicTypeLift("7374727563747b78207374727563747b6120696e74383b206220696e74383b206320696e74383b206420696e74383b206520696e7433327d7d", "Δtypeᴛ28")]
[assembly: GoDynamicTypeLift("7374727563747b78207374727563747b6120696e74383b206220696e74383b206320696e74383b206420696e74383b206520696e74383b206620696e7433327d7d", "Δtypeᴛ29")]
[assembly: GoDynamicTypeLift("7374727563747b78207374727563747b63206368616e202a696e7433323b206420666c6f617433327d7d", "Δtypeᴛ22")]
[assembly: GoDynamicTypeLift("7374727563747b78207374727563747b632066756e63286368616e202a7265666c6563745f746573742e696e74656765722c202a696e7438297d7d", "Δtypeᴛ24")]
[assembly: GoDynamicTypeLift("7374727563747b78207374727563747b662066756e632861726773202e2e2e696e74297d7d", "Δtypeᴛ32")]
[assembly: GoDynamicTypeLift("7374727563747b78207374727563747b696e7433323b20696e7436347d7d", "Δtypeᴛ34")]
[assembly: GoDynamicTypeLift("7374727563747b782075696e7431367d", "Δtypeᴛ7")]
[assembly: GoDynamicTypeLift("7374727563747b782075696e7433327d", "Δtypeᴛ8")]
[assembly: GoDynamicTypeLift("7374727563747b782075696e7436347d", "Δtypeᴛ9")]
[assembly: GoDynamicTypeLift("7374727563747b782075696e74387d", "Δtypeᴛ6")]
[assembly: GoDynamicTypeLift("7374727563747b782075696e747d", "Δtypeᴛ5")]
[assembly: GoTypeAlias("Bool", "const:ΔBool")]
[assembly: GoTypeAlias("ChanDir", "ΔChanDir")]
[assembly: GoTypeAlias("Int", "const:ΔInt")]
[assembly: GoTypeAlias("Interface", "const:ΔInterface")]
[assembly: GoTypeAlias("Kind", "ΔKind")]
[assembly: GoTypeAlias("Loopy", "object")]
[assembly: GoTypeAlias("M", "ΔM")]
[assembly: GoTypeAlias("Method", "ΔMethod")]
[assembly: GoTypeAlias("Pointer", "const:ΔPointer")]
[assembly: GoTypeAlias("Slice", "const:ΔSlice")]
[assembly: GoTypeAlias("String", "const:ΔString")]
[assembly: GoTypeAlias("Tint2", "go.reflect_test_package.Tint")]
[assembly: GoTypeAlias("Type", "ΔType")]
[assembly: GoTypeAlias("Uint", "const:ΔUint")]
[assembly: GoTypeAlias("UnsafePointer", "const:ΔUnsafePointer")]
[assembly: GoTypeAlias("Value", "ΔValue")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<Point, TestMethod_x>]
[assembly: GoImplement<TestCallPanic_T, TestCallPanic_T1>(Promoted = true)]
[assembly: GoImplement<TestCallPanic_T, TestCallPanic_t0>(Promoted = true)]
[assembly: GoImplement<TestCallPanic_T2, TestCallPanic_T1>(Promoted = true)]
[assembly: GoImplement<TestCallPanic_T2, TestCallPanic_t0>(Promoted = true)]
[assembly: GoImplement<WC, io_package.WriteCloser>(Pointer = true)]
[assembly: GoImplement<bytes_package.Buffer, io_package.ReadWriter>(Pointer = true)]
[assembly: GoImplement<bytes_package.Buffer, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<strings_package.Builder, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<strings_package.Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<testTypeWithMethod, fmt_package.Stringer>(Pointer = true)]
[assembly: GoImplement<testTypeWithMethod, fmt_package.Stringer>]
[assembly: GoImplement<testTypeWithMethod, packIfaceValueIntoEmptyIface_type>]
[assembly: GoImplement<timp, TestCallPanic_T1>]
[assembly: GoImplement<timp, TestCallPanic_t0>]
// </InterfaceImplementations>

// <ImplicitConversions>
// </ImplicitConversions>

// This package's string literals are hoisted to `static readonly` fields (the standard
// library's form, or -hoist-literals), so no converted file passes a bare literal for
// go2cs-gen's literal interceptor to box. The stamp below tells the generator to skip
// the assembly.
[assembly: global::go.GoHoistedLiterals]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: global::go.GoPositionMap("reflect/abi_test.go", "abi_test.cs", "ACcw2AAHEIKCgqa4gsqCgoKClIKmgoKCgpSClIKmgoKCgpSCpoKCgoKUgpSCpoKCkoKClIKUgrQAECCCgqaigqaCgqqigqqigqbY3IKCkqKCgpSClIKClIKCgoKU2NTY3IKCgpSUgoKSktKCkpSCgoKClLiO4oKClLiCgqamgsakAC5m6KKqoqqiqqKqoqqiqqKqoqqiqqKqoqqiqqKqoqrCqsKqwqqiqqKqoqqiqqKqoqrCqsKqoqqiqqKqoqqiqqKqoqqiqqKqoqrCqsKqoqqiAAIQ0qYALWCiqqKqoqqiqqKqoqqiqqKqoqqiqqKqoqqiqqKqoqrCqsKqwqqiqqKqoqqiqqKqoqrCqsKqoqqiqqKqoqqiqqKqoqqiqqKqoqrCqsKqoqqiAHfsAdaCgoKUpoKUgoKmpoKGgpSCgg==", "36-40:1;169-189:1;204-209:1;214-227:2;229-256:3;237-240:3.1;968-974:1;979-981:1")]
[assembly: global::go.GoPositionMap("reflect/all_test.go", "all_test.cs", "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kpKigpSCABEcAAwegoKCgIIAGgqCAA42goKAggAKCoIADSKCgJLagoKSgoKClKYAFQaCpAAAEgA4eoKCgoIACBTSgoaSkqIAFiqCgoKCgqaCgoKCABEIggAEFIKC7qKApIKUgILI1oKCgoCSyIKCgoCSAAgIgqaygJKCoraCgoCSyIKygJKCoraCgoCSyMqCgpKCgoKCgpSClIIABxCigpSC+JSigJKCopKigJKCopKigJKCoqiilLKAkoKikrKAkoKiyIKygJKCopKygJKCopKygJKCovamgoKCgJLIlIKCgoKCgJLItIKCgoKCgoKUgrzCgoKClIKmgoKmgqaClIIAEgiC3IKSkpKSlJKUgoKCgpSClJKUkqaCgoKUgpSClIKokoKCgIKkgoK4kpKmgoKCgIKkgoCCyIKSopSiyoKCgoCCpICCAA4OgoKAgqSCgIL4goKCgoIAChKCgoKCpoKCgqaygoKAgraC1IKClIKC6IKCgoKmgoKCprKCgoCCtoLUgoKUgoIAEQiigoKigqKiogCiAc4CgoKClIKCAA0YxMQAgAGAAqKCkoCClLaAgpSkgqaCpoCCAAwKooIAFizEkLaAgtqCgpSCgqKCgrSkAAsGgoKClICApIKClIKCgoKmpoCQkKQABhaykoKCAA4MgoKSkoKCopQACRyykoCCpICCAAwOkpKUlIKClIKmgoK4kKaSgLaSgJKAtsqgvsKUgpSClIKmgoKCggAEFAAJAoKCgriUgqIACQSCgoKCgoKClJKUlIKUkoKCgoKCgoKUgoKClIKClIKCgqailIKCuIKC/ICi1IKCgoKmgoIACAiCooCSgqKSooCSgqIACgaCgpSClJKCgoKUgoSogoKSlIK4goKUgoKClJSmgpKC", "378-384:1;560-577:1;738-738:1;748-748:1;765-781:2;768-772:2.1;783-794:3;796-810:4;882-882:1;883-883:2;927-945:1;946-964:2;1139-1146:1;1284-1293:1;1285-1289:1.1;1507-1507:1;1508-1508:2;1509-1509:3;1544-1551:4;1545-1549:4.1;1643-1643:1;1644-1644:2;1747-1751:1;1789-1789:1;1793-1805:2;1852-1852:3;1868-1868:4;2061-2069:1;2108-2114:1;2189-2189:1;2241-2245:1;2243-2243:1.1;2262-2262:1;2279-2279:1;2280-2282:2;2298-2298:1;2299-2299:2;2351-2353:1;2358-2361:2;2366-2368:3;2374-2376:4;2382-2388:1;2384-2386:1.1;2390-2397:2;2392-2395:2.1;2399-2406:3;2401-2404:3.1;2408-2416:4;2412-2414:4.1;2831-2836:1;2838-2847:2;2851-2855:3;2893-2893:4;3206-3219:1;3505-3508:1;3515-3524:1;3525-3533:2;3534-3542:3;3537-3537:3.1;3545-3549:4;3631-3631:1;3632-3632:2;3633-3633:3;3647-3647:4;3648-3648:5;3649-3649:6;3653-3653:7;3670-3670:1;3671-3671:2;3672-3672:3;3673-3673:4;3674-3674:5;3687-3687:6;3688-3688:7;3691-3691:8;3692-3692:9;3713-3713:1;3714-3714:2;3732-3734:1;3764-3764:1;3765-3765:2;3779-3779:3;3780-3780:4;3875-3875:1;3876-3876:2;3877-3877:3;3878-3878:4;3879-3879:5;3883-3883:1;3884-3884:2;3885-3885:3;3913-3913:4;3914-3914:5;3915-3915:6;3916-3916:7;3917-3917:8;3918-3918:9;3919-3919:10;3920-3920:11;3921-3921:12;3922-3922:13;3923-3923:14;3924-3924:15;3925-3925:16;3926-3926:17;3927-3927:18;3928-3928:19;3929-3929:20;3930-3930:21;3931-3931:22;3932-3932:23;3933-3933:24;3934-3934:25;3935-3935:26;3936-3936:27;3937-3937:28;3941-3941:29;3942-3942:30;3943-3943:31;3944-3944:32;3945-3945:33;3946-3946:34;3947-3947:35;3948-3948:36;3949-3949:37;3950-3950:38;3951-3951:39;3952-3952:40;3953-3953:41;3954-3954:42;3955-3955:43;3956-3956:44;3957-3957:45;3958-3958:46;3959-3959:47;3960-3960:48;3961-3961:49;3962-3962:50;3963-3963:51;3964-3964:52;3965-3965:53;4000-4000:1;4001-4001:2;4002-4002:3;4003-4003:4;4007-4007:5;4008-4008:6;4009-4009:7;4010-4010:8;4011-4011:9;4012-4012:10;4013-4013:11;4014-4014:12;4016-4016:13;4017-4017:14;4018-4018:15;4019-4019:16;4021-4021:17;4022-4022:18;4023-4023:19;4024-4024:20;4026-4026:21;4027-4027:22;4028-4028:23;4029-4029:24;4031-4031:25;4032-4032:26;4033-4033:27;4034-4034:28;4036-4036:29;4037-4037:30;4038-4038:31;4039-4039:32;4041-4041:33;4042-4042:34;4043-4043:35;4044-4044:36;4049-4049:1;4050-4050:2;4051-4051:3;4052-4052:4;4053-4053:5;4054-4054:6;4055-4055:7;4056-4056:8;4057-4057:9;4058-4058:10;4059-4059:11;4060-4060:12;4061-4061:13;4062-4062:14;4063-4063:15;4064-4064:16;4065-4065:17;4066-4066:18;4067-4067:19;4068-4068:20;4069-4069:21;4070-4070:22;4071-4071:23;4072-4072:24;4073-4073:25;4074-4074:26;4075-4075:27;4076-4076:28;4077-4077:29;4078-4078:30;4079-4079:31;4080-4080:32;4081-4081:33;4082-4082:34;4083-4083:35;4084-4084:36;4085-4085:37;4086-4086:38;4087-4087:39;4088-4088:40;4089-4089:41;4093-4115:1;4752-4754:1;4759-4761:2;4939-4939:1;4945-4945:2;4951-4951:3;4957-4957:4;4963-4963:5;4969-4969:6;4975-4975:7;4981-4981:8;4987-4987:9;4993-4993:10;4999-5005:11;5097-5097:1;5171-5173:1;5210-5215:1;5249-5254:1;5257-5262:2;5265-5269:3;5392-5397:1;5398-5403:2;5404-5409:3;5431-5442:1;5432-5440:1.1;5558-5576:2;5627-5627:1;5856-5859:1;5865-5868:2;5908-5915:3;5909-5913:3.1;5958-5960:4;5972-5974:5;5994-5996:6;6015-6017:7;6044-6046:1;6106-6126:1;6107-6124:1.1;6255-6255:1;6371-6380:1;6413-6413:2;6414-6414:3;6507-6507:1;6517-6517:1;6618-6618:1;6634-6634:1;6682-6688:1;6778-6778:1;6785-6789:2;6799-6803:1;6855-6855:1;6860-6862:1;6908-6910:1;6915-6916:1;6917-6920:2;6930-6930:1;6932-6949:2;6950-6955:3;6966-6970:1;7012-7014:1;7036-7036:2;7044-7044:3;7052-7052:4;7060-7060:5;7069-7069:6;7077-7077:7;7085-7085:8;7094-7094:9;7111-7139:10;7340-7344:1;7377-7379:1;7387-7394:2;7411-7414:1;7496-7496:1;7519-7524:1;7623-7625:1;7715-7719:1;7716-7716:1.1;7730-7734:2;7731-7731:2.1;7771-7774:3;7782-7786:1;7783-7783:1.1;7787-7791:2;7788-7788:2.1;7792-7796:3;7793-7793:3.1;7803-7807:4;7804-7804:4.1;7808-7812:5;7809-7809:5.1;7817-7821:6;7818-7818:6.1;7822-7826:7;7823-7823:7.1;7827-7831:8;7828-7828:8.1;7913-7915:1;7916-7918:2;7928-7930:3;7931-7933:4;7937-7939:5;7940-7942:6;7943-7945:7;7946-7948:8;7982-7984:9;7985-7987:10;8043-8052:1;8044-8049:1.1;8053-8056:2;8076-8085:1;8077-8082:1.1;8086-8089:2;8463-8463:1;8476-8476:2;8491-8496:1;8506-8506:1;8512-8520:2;8522-8522:3;8522-8522:3.1;8536-8541:4;8551-8551:1;8569-8576:2;8601-8601:1;8605-8605:2;8609-8609:3;8610-8610:4;8618-8618:5;8653-8658:1;8742-8746:1;8743-8743:1.1;8747-8751:2;8748-8748:2.1;8776-8778:1;8799-8803:1", "", "931=Field/1/1/2;1380=NaN/1/10/40,NaN/2/10/40,NaN/3/10/41,NaN/4/10/41,NaN/5/10/42,NaN/6/10/43,NaN/7/10/43,NaN/8/10/44,NaN/9/10/45,NaN/10/10/46;2419=ValueOf/1/2/2,ValueOf/2/2/3;2429=ValueOf/1/1/2;2440=ValueOf/1/2/2,ValueOf/2/2/3;2574=ValueOf/1/1/2;2581=ValueOf/1/1/2;2697=ValueOf/2/8/1,ValueOf/3/8/2,ValueOf/4/8/3,ValueOf/5/8/4,ValueOf/6/8/5,ValueOf/7/8/6,ValueOf/8/8/7;2923=Interface/1/1/2;2928=Interface/1/1/3;2938=Interface/1/1/2;2941=Interface/1/1/2;2957=Interface/1/1/2;2963=Interface/1/1/3;2974=Interface/1/1/3;2983=Interface/1/1/2;3837=TypeOf/1/29/4,Elem/1/4/4,TypeOf/2/29/5,TypeOf/3/29/6,TypeOf/4/29/7,TypeOf/5/29/8,TypeOf/6/29/9,TypeOf/7/29/10,TypeOf/8/29/11,TypeOf/9/29/12,TypeOf/10/29/13,TypeOf/11/29/14,TypeOf/12/29/15,TypeOf/13/29/16,TypeOf/14/29/17,TypeOf/15/29/18,TypeOf/16/29/19,TypeOf/17/29/20,TypeOf/18/29/21,TypeOf/19/29/22,TypeOf/20/29/23,TypeOf/21/29/24,TypeOf/22/29/25,Elem/2/4/25,TypeOf/23/29/26,TypeOf/24/29/27,TypeOf/25/29/28,TypeOf/26/29/29,TypeOf/27/29/30,Elem/3/4/30,TypeOf/28/29/31,TypeOf/29/29/32,Elem/4/4/32;5581=Invoke/1/734/36,Invoke/2/734/36,Invoke/3/734/37,Invoke/4/734/37,Invoke/5/734/38,Invoke/6/734/38,Invoke/7/734/39,Invoke/8/734/39,Invoke/9/734/40,Invoke/10/734/40,Invoke/11/734/41,Invoke/12/734/41,Invoke/13/734/42,Invoke/14/734/42,Invoke/15/734/43,Invoke/16/734/43,Invoke/17/734/44,Invoke/18/734/44,Invoke/19/734/45,Invoke/20/734/45,Invoke/21/734/46,Invoke/22/734/46,Invoke/23/734/47,Invoke/24/734/47,Invoke/25/734/48,Invoke/26/734/48,Invoke/27/734/49,Invoke/28/734/49,Invoke/29/734/50,Invoke/30/734/50,Invoke/31/734/51,Invoke/32/734/51,Invoke/33/734/52,Invoke/34/734/52,Invoke/35/734/53,Invoke/36/734/53,Invoke/37/734/54,Invoke/38/734/54,Invoke/39/734/55,Invoke/40/734/55,Invoke/41/734/56,Invoke/42/734/56,Invoke/43/734/57,Invoke/44/734/57,Invoke/45/734/58,Invoke/46/734/58,Invoke/47/734/59,Invoke/48/734/59,Invoke/49/734/60,Invoke/50/734/60,Invoke/51/734/61,Invoke/52/734/61,Invoke/53/734/62,Invoke/54/734/62,Invoke/55/734/63,Invoke/56/734/63,Invoke/57/734/64,Invoke/58/734/64,Invoke/59/734/65,Invoke/60/734/65,Invoke/61/734/66,Invoke/62/734/66,Invoke/63/734/67,Invoke/64/734/67,Invoke/65/734/68,Invoke/66/734/68,Invoke/67/734/69,Invoke/68/734/69,Invoke/69/734/70,Invoke/70/734/70,Invoke/71/734/71,Invoke/72/734/71,Invoke/73/734/72,Invoke/74/734/72,Invoke/75/734/73,Invoke/76/734/73,Invoke/77/734/74,Invoke/78/734/74,Invoke/79/734/75,Invoke/80/734/75,Invoke/81/734/76,Invoke/82/734/76,Invoke/83/734/77,Invoke/84/734/77,Invoke/85/734/78,Invoke/86/734/78,Invoke/87/734/79,Invoke/88/734/79,Invoke/89/734/80,Invoke/90/734/80,Invoke/91/734/81,Invoke/92/734/81,Invoke/93/734/82,Invoke/94/734/82,Invoke/95/734/83,Invoke/96/734/83,Invoke/97/734/84,Invoke/98/734/84,Invoke/99/734/85,Invoke/100/734/85,Invoke/101/734/86,Invoke/102/734/86,Invoke/103/734/87,Invoke/104/734/87,Invoke/105/734/88,Invoke/106/734/88,Invoke/107/734/89,Invoke/108/734/89,Invoke/109/734/90,Invoke/110/734/90,Invoke/111/734/91,Invoke/112/734/91,Invoke/113/734/92,Invoke/114/734/92,Invoke/115/734/93,Invoke/116/734/93,Invoke/117/734/94,Invoke/118/734/94,Invoke/119/734/95,Invoke/120/734/95,Invoke/121/734/96,Invoke/122/734/96,Invoke/123/734/97,Invoke/124/734/97,Invoke/125/734/98,Invoke/126/734/98,Invoke/127/734/99,Invoke/128/734/99,Invoke/129/734/100,Invoke/130/734/100,Invoke/131/734/101,Invoke/132/734/101,Invoke/133/734/102,Invoke/134/734/102,Invoke/135/734/103,Invoke/136/734/103,Invoke/137/734/104,Invoke/138/734/104,Invoke/139/734/105,Invoke/140/734/105,Invoke/141/734/106,Invoke/142/734/106,Invoke/143/734/107,Invoke/144/734/107,Invoke/145/734/108,Invoke/146/734/108,Invoke/147/734/109,Invoke/148/734/109,Invoke/149/734/110,Invoke/150/734/110,Invoke/151/734/111,Invoke/152/734/111,Invoke/153/734/112,Invoke/154/734/112,Invoke/155/734/113,Invoke/156/734/113,Invoke/157/734/114,Invoke/158/734/114,Invoke/159/734/115,Invoke/160/734/115,Invoke/161/734/116,Invoke/162/734/116,Invoke/163/734/117,Invoke/164/734/117,Invoke/165/734/118,Invoke/166/734/118,Invoke/167/734/119,Invoke/168/734/119,Invoke/169/734/120,Invoke/170/734/120,Invoke/171/734/121,Invoke/172/734/121,Invoke/173/734/122,Invoke/174/734/122,Invoke/175/734/123,Invoke/176/734/123,Invoke/177/734/124,Invoke/178/734/124,Invoke/179/734/125,Invoke/180/734/125,Invoke/181/734/126,Invoke/182/734/126,Invoke/183/734/127,Invoke/184/734/127,Invoke/185/734/128,Invoke/186/734/128,Invoke/187/734/129,Invoke/188/734/129,Invoke/189/734/130,Invoke/190/734/130,Invoke/191/734/131,Invoke/192/734/131,Invoke/193/734/132,Invoke/194/734/132,Invoke/195/734/133,Invoke/196/734/133,Invoke/197/734/134,Invoke/198/734/134,Invoke/199/734/135,Invoke/200/734/135,Invoke/201/734/136,Invoke/202/734/136,Invoke/203/734/137,Invoke/204/734/137,Invoke/205/734/138,Invoke/206/734/138,Invoke/207/734/139,Invoke/208/734/139,Invoke/209/734/140,Invoke/210/734/140,Invoke/211/734/141,Invoke/212/734/141,Invoke/213/734/142,Invoke/214/734/142,Invoke/215/734/143,Invoke/216/734/143,Invoke/217/734/144,Invoke/218/734/144,Invoke/219/734/145,Invoke/220/734/145,Invoke/221/734/146,Invoke/222/734/146,Invoke/223/734/147,Invoke/224/734/147,Invoke/225/734/148,Invoke/226/734/148,Invoke/227/734/149,Invoke/228/734/149,Invoke/229/734/150,Invoke/230/734/150,Invoke/231/734/151,Invoke/232/734/151,Invoke/233/734/152,Invoke/234/734/152,Invoke/235/734/153,Invoke/236/734/153,Invoke/237/734/154,Invoke/238/734/154,Invoke/239/734/155,Invoke/240/734/155,Invoke/241/734/156,Invoke/242/734/156,Invoke/243/734/157,Invoke/244/734/157,Invoke/245/734/158,Invoke/246/734/158,Invoke/247/734/159,Invoke/248/734/159,Invoke/249/734/160,Invoke/250/734/160,Invoke/251/734/161,Invoke/252/734/161,Invoke/253/734/162,Invoke/254/734/162,Invoke/255/734/163,Invoke/256/734/163,Invoke/257/734/164,Invoke/258/734/164,Invoke/259/734/165,Invoke/260/734/165,Invoke/261/734/166,Invoke/262/734/166,Invoke/263/734/167,Invoke/264/734/167,Invoke/265/734/168,Invoke/266/734/168,Invoke/267/734/169,Invoke/268/734/169,Invoke/269/734/170,Invoke/270/734/170,Invoke/271/734/171,Invoke/272/734/171,Invoke/273/734/172,Invoke/274/734/172,Invoke/275/734/173,Invoke/276/734/173,Invoke/277/734/174,Invoke/278/734/174,Invoke/279/734/175,Invoke/280/734/175,Invoke/281/734/176,Invoke/282/734/176,Invoke/283/734/177,Invoke/284/734/177,Invoke/285/734/178,Invoke/286/734/178,Invoke/287/734/179,Invoke/288/734/179,Invoke/289/734/180,Invoke/290/734/180,Invoke/291/734/181,Invoke/292/734/181,Invoke/293/734/182,Invoke/294/734/182,Invoke/295/734/183,Invoke/296/734/183,Invoke/297/734/184,Invoke/298/734/184,Invoke/299/734/185,Invoke/300/734/185,Invoke/301/734/186,Invoke/302/734/186,Invoke/303/734/187,Invoke/304/734/187,Invoke/305/734/188,Invoke/306/734/188,Invoke/307/734/189,Invoke/308/734/189,Invoke/309/734/190,Invoke/310/734/190,Invoke/311/734/191,Invoke/312/734/191,Invoke/313/734/192,Invoke/314/734/192,Invoke/315/734/193,Invoke/316/734/193,Invoke/317/734/194,Invoke/318/734/194,Invoke/319/734/195,Invoke/320/734/195,Invoke/321/734/196,Invoke/322/734/196,Invoke/323/734/197,Invoke/324/734/197,Invoke/325/734/198,Invoke/326/734/198,Invoke/327/734/199,Invoke/328/734/199,Invoke/329/734/200,Invoke/330/734/200,Invoke/331/734/201,Invoke/332/734/201,Invoke/333/734/202,Invoke/334/734/202,Invoke/335/734/203,Invoke/336/734/203,Invoke/337/734/204,Invoke/338/734/204,Invoke/339/734/207,Invoke/340/734/207,Invoke/341/734/210,Invoke/342/734/210,Invoke/343/734/211,Invoke/344/734/211,Invoke/345/734/212,Invoke/346/734/212,Invoke/347/734/213,Invoke/348/734/213,Invoke/349/734/216,Invoke/350/734/216,Invoke/351/734/217,Invoke/352/734/217,Invoke/353/734/218,Invoke/354/734/218,Invoke/355/734/219,Invoke/356/734/219,Invoke/357/734/220,Invoke/358/734/220,Invoke/359/734/221,Invoke/360/734/221,Invoke/361/734/222,Invoke/362/734/222,Invoke/363/734/223,Invoke/364/734/223,Invoke/365/734/224,Invoke/366/734/224,Invoke/367/734/225,Invoke/368/734/225,Invoke/369/734/226,Invoke/370/734/226,Invoke/371/734/227,Invoke/372/734/227,Invoke/373/734/228,Invoke/374/734/228,Invoke/375/734/229,Invoke/376/734/229,Invoke/377/734/230,Invoke/378/734/230,Invoke/379/734/231,Invoke/380/734/231,Invoke/381/734/232,Invoke/382/734/232,Invoke/383/734/233,Invoke/384/734/233,Invoke/385/734/234,Invoke/386/734/234,Invoke/387/734/235,Invoke/388/734/235,Invoke/389/734/236,Invoke/390/734/236,Invoke/391/734/237,Invoke/392/734/237,Invoke/393/734/238,Invoke/394/734/238,Invoke/395/734/239,Invoke/396/734/239,Invoke/397/734/240,Invoke/398/734/240,Invoke/399/734/241,Invoke/400/734/241,Invoke/401/734/242,Invoke/402/734/242,Invoke/403/734/243,Invoke/404/734/243,Invoke/405/734/244,Invoke/406/734/244,Invoke/407/734/245,Invoke/408/734/245,Invoke/409/734/248,Invoke/410/734/248,Invoke/411/734/249,Invoke/412/734/249,Invoke/413/734/250,Invoke/414/734/250,Invoke/415/734/251,Invoke/416/734/251,Invoke/417/734/252,Invoke/418/734/252,Invoke/419/734/253,Invoke/420/734/253,Invoke/421/734/254,Invoke/422/734/254,Invoke/423/734/255,Invoke/424/734/255,Invoke/425/734/256,Invoke/426/734/256,Invoke/427/734/257,Invoke/428/734/257,Invoke/429/734/258,Invoke/430/734/258,Invoke/431/734/259,Invoke/432/734/259,Invoke/433/734/260,Invoke/434/734/260,Invoke/435/734/261,Invoke/436/734/261,Invoke/437/734/262,Invoke/438/734/262,Invoke/439/734/263,Invoke/440/734/263,Invoke/441/734/264,Invoke/442/734/264,Invoke/443/734/265,Invoke/444/734/265,Invoke/445/734/266,Invoke/446/734/266,Invoke/447/734/267,Invoke/448/734/267,Invoke/449/734/268,Invoke/450/734/268,Invoke/451/734/269,Invoke/452/734/269,Invoke/453/734/270,Invoke/454/734/270,Invoke/455/734/271,Invoke/456/734/271,Invoke/457/734/272,Invoke/458/734/272,Invoke/459/734/273,Invoke/460/734/273,Invoke/461/734/274,Invoke/462/734/274,Invoke/463/734/275,Invoke/464/734/275,Invoke/465/734/276,Invoke/466/734/276,Invoke/467/734/277,Invoke/468/734/277,Invoke/469/734/278,Invoke/470/734/278,Invoke/471/734/279,Invoke/472/734/279,Invoke/473/734/282,Invoke/474/734/282,Invoke/475/734/283,Invoke/476/734/283,Invoke/477/734/284,Invoke/478/734/284,Invoke/479/734/285,Invoke/480/734/285,Invoke/481/734/286,Invoke/482/734/286,Invoke/483/734/289,Invoke/484/734/289,Invoke/485/734/290,Invoke/486/734/290,Invoke/487/734/291,Invoke/488/734/291,Invoke/489/734/292,Invoke/490/734/292,Invoke/491/734/293,Invoke/492/734/293,Invoke/493/734/296,Invoke/494/734/296,Invoke/495/734/297,Invoke/496/734/297,Invoke/497/734/298,Invoke/498/734/298,Invoke/499/734/299,Invoke/500/734/299,Invoke/501/734/300,Invoke/502/734/300,Invoke/503/734/301,Invoke/504/734/301,Invoke/505/734/302,Invoke/506/734/302,Invoke/507/734/303,Invoke/508/734/303,Invoke/509/734/304,Invoke/510/734/304,Invoke/511/734/305,Invoke/512/734/305,Invoke/513/734/306,Invoke/514/734/306,Invoke/515/734/307,Invoke/516/734/307,Invoke/517/734/308,Invoke/518/734/308,Invoke/519/734/309,Invoke/520/734/309,Invoke/521/734/310,Invoke/522/734/310,Invoke/523/734/311,Invoke/524/734/311,Invoke/525/734/312,Invoke/526/734/312,Invoke/527/734/315,Invoke/528/734/315,Invoke/529/734/316,Invoke/530/734/316,Invoke/531/734/317,Invoke/532/734/317,Invoke/533/734/318,Invoke/534/734/318,Invoke/535/734/319,Invoke/536/734/319,Invoke/537/734/320,Invoke/538/734/320,Invoke/539/734/321,Invoke/540/734/321,Invoke/541/734/322,Invoke/542/734/322,Invoke/543/734/323,Invoke/544/734/323,Invoke/545/734/324,Invoke/546/734/324,Invoke/547/734/325,Invoke/548/734/325,Invoke/549/734/327,Invoke/550/734/327,Invoke/551/734/328,Invoke/552/734/328,Invoke/553/734/329,Invoke/554/734/329,Invoke/555/734/330,Invoke/556/734/330,Invoke/557/734/331,Invoke/558/734/331,Invoke/559/734/332,Invoke/560/734/332,Invoke/561/734/333,Invoke/562/734/333,Invoke/563/734/334,Invoke/564/734/334,Invoke/565/734/335,Invoke/566/734/335,Invoke/567/734/336,Invoke/568/734/336,Invoke/569/734/339,Invoke/570/734/339,Invoke/571/734/340,Invoke/572/734/340,Invoke/573/734/341,Invoke/574/734/341,Invoke/575/734/342,Invoke/576/734/342,Invoke/577/734/343,Invoke/578/734/343,Invoke/579/734/344,Invoke/580/734/344,Invoke/581/734/345,Invoke/582/734/345,Invoke/583/734/346,Invoke/584/734/346,Invoke/585/734/347,Invoke/586/734/347,Invoke/587/734/348,Invoke/588/734/348,Invoke/589/734/349,Invoke/590/734/349,Invoke/591/734/352,Invoke/592/734/354,Invoke/593/734/358,Invoke/594/734/360,Invoke/595/734/364,Invoke/596/734/364,Invoke/597/734/368,Invoke/598/734/370,Invoke/599/734/372,Invoke/600/734/372,Invoke/601/734/376,Invoke/602/734/378,Invoke/603/734/380,Invoke/604/734/380,Invoke/605/734/381,Invoke/606/734/381,Invoke/607/734/384,Invoke/608/734/384,Invoke/609/734/385,Invoke/610/734/385,Invoke/611/734/388,Invoke/612/734/388,Invoke/613/734/389,Invoke/614/734/389,Invoke/615/734/390,Invoke/616/734/390,Invoke/617/734/391,Invoke/618/734/391,Invoke/619/734/394,Invoke/620/734/394,Invoke/621/734/395,Invoke/622/734/395,Invoke/623/734/396,Invoke/624/734/396,Invoke/625/734/397,Invoke/626/734/397,Invoke/627/734/398,Invoke/628/734/398,Invoke/629/734/399,Invoke/630/734/399,Invoke/631/734/400,Invoke/632/734/400,Invoke/633/734/401,Invoke/634/734/401,Invoke/635/734/402,Invoke/636/734/402,Invoke/637/734/403,Invoke/638/734/403,Invoke/639/734/404,Invoke/640/734/404,Invoke/641/734/405,Invoke/642/734/405,Invoke/643/734/408,Invoke/644/734/408,Invoke/645/734/409,Invoke/646/734/409,Invoke/647/734/410,Invoke/648/734/410,Invoke/649/734/411,Invoke/650/734/411,Invoke/651/734/412,Invoke/652/734/412,Invoke/653/734/413,Invoke/654/734/413,Invoke/655/734/414,Invoke/656/734/414,Invoke/657/734/415,Invoke/658/734/415,Invoke/659/734/416,Invoke/660/734/416,Invoke/661/734/417,Invoke/662/734/417,Invoke/663/734/418,Invoke/664/734/418,Invoke/665/734/419,Invoke/666/734/419,Invoke/667/734/420,Invoke/668/734/420,Invoke/669/734/421,Invoke/670/734/421,Invoke/671/734/424,Invoke/672/734/424,Invoke/673/734/425,Invoke/674/734/425,Invoke/675/734/426,Invoke/676/734/426,Invoke/677/734/427,Invoke/678/734/427,Invoke/679/734/428,Invoke/680/734/428,Invoke/681/734/429,Invoke/682/734/429,Invoke/683/734/430,Invoke/684/734/430,Invoke/685/734/431,Invoke/686/734/431,Invoke/687/734/432,Invoke/688/734/432,Invoke/689/734/433,Invoke/690/734/433,Invoke/691/734/434,Invoke/692/734/434,Invoke/693/734/435,Invoke/694/734/435,Invoke/695/734/436,Invoke/696/734/436,Invoke/697/734/437,Invoke/698/734/437,Invoke/699/734/438,Invoke/700/734/438,Invoke/701/734/439,Invoke/702/734/439,Invoke/703/734/440,Invoke/704/734/440,Invoke/705/734/441,Invoke/706/734/441,Invoke/707/734/442,Invoke/708/734/442,Invoke/709/734/443,Invoke/710/734/443,Invoke/711/734/444,Invoke/712/734/444,Invoke/713/734/445,Invoke/714/734/445,Invoke/715/734/446,Invoke/716/734/446,Invoke/717/734/447,Invoke/718/734/447,Invoke/719/734/450,Invoke/720/734/450,Invoke/721/734/451,Invoke/722/734/451,Invoke/723/734/452,Invoke/724/734/452,Invoke/725/734/453,Invoke/726/734/453,Invoke/727/734/454,Invoke/728/734/454,Invoke/729/734/455,Invoke/730/734/455,Invoke/731/734/458,EmptyInterfaceV/1/2/458,Invoke/732/734/459,EmptyInterfaceV/2/2/459,Invoke/733/734/460,ReaderV/1/2/460,ReadWriterV/1/2/461,ReaderV/2/2/461,Invoke/734/734/462,ReadWriterV/2/2/462;6213=TypeOf/1/15/4,TypeOf/2/15/5,TypeOf/3/15/6,TypeOf/4/15/7,TypeOf/5/15/8,TypeOf/6/15/9,TypeOf/7/15/10,TypeOf/8/15/11,TypeOf/9/15/12,TypeOf/10/15/13,TypeOf/11/15/14,TypeOf/12/15/15,TypeOf/13/15/16,TypeOf/14/15/17,TypeOf/15/15/18,Elem/1/1/18;6522=Interface/1/1/2;6528=Interface/1/1/2;6787=TypeOf/1/2/1,TypeOf/2/2/2;6795=TypeOf/1/2/1,TypeOf/2/2/2;6803=TypeOf/1/1/1;6809=TypeOf/1/3/3,TypeOf/2/3/7,TypeOf/3/3/11;6863=TypeOf/1/4/4,TypeOf/2/4/9,TypeOf/3/4/13,TypeOf/4/4/17;6924=TypeOf/1/2/3,TypeOf/2/2/7;6954=TypeOf/1/2/1,TypeOf/2/2/2;6960=TypeOf/1/2/1,TypeOf/2/2/2;6966=TypeOf/1/2/1,TypeOf/2/2/2;7020=TypeOf/1/27/6,TypeOf/2/27/10,TypeOf/3/27/14,TypeOf/4/27/18,TypeOf/5/27/26,TypeOf/6/27/30,TypeOf/7/27/34,TypeOf/8/27/38,TypeOf/9/27/42,TypeOf/10/27/46,TypeOf/11/27/49,TypeOf/12/27/52,TypeOf/13/27/56,TypeOf/14/27/60,TypeOf/15/27/64,TypeOf/16/27/68,TypeOf/17/27/72,TypeOf/18/27/76,TypeOf/19/27/80,TypeOf/20/27/84,TypeOf/21/27/87,TypeOf/22/27/90,TypeOf/23/27/93,TypeOf/24/27/96,TypeOf/25/27/100,TypeOf/26/27/104,TypeOf/27/27/108;7225=TypeOf/1/2/1,TypeOf/2/2/2;7234=TypeOf/1/20/15,TypeOf/2/20/16,StructOf/1/7/13,TypeOf/3/20/24,TypeOf/4/20/25,TypeOf/5/20/26,StructOf/2/7/22,TypeOf/6/20/34,TypeOf/7/20/35,StructOf/3/7/32,TypeOf/8/20/43,TypeOf/9/20/44,TypeOf/10/20/45,StructOf/4/7/41,TypeOf/11/20/53,TypeOf/12/20/54,TypeOf/13/20/55,StructOf/5/7/51,TypeOf/14/20/63,TypeOf/15/20/64,TypeOf/16/20/65,StructOf/6/7/61,TypeOf/17/20/73,TypeOf/18/20/74,TypeOf/19/20/75,TypeOf/20/20/76,StructOf/7/7/71;7372=TypeOf/1/1/3,ArrayOf/1/1/3,Interface/1/1/5;7396=TypeOf/1/1/3,ArrayOf/1/1/3,Interface/1/1/5;7513=TypeOf/1/1/3;7584=TypeOf/1/1/3,PointerTo/1/1/3;7601=TypeOf/1/1/3,PointerTo/1/1/3;7617=TypeOf/1/1/4,PointerTo/1/1/4,StructOf/1/1/9;7638=TypeOf/1/1/4,StructOf/1/1/9;7663=TypeOf/1/1/1;7678=TypeOf/1/2/4,TypeOf/2/2/9;7708=ArrayOf/1/2/1,ArrayOf/2/2/2;7713=ArrayOf/1/3/5,ArrayOf/2/3/12,ArrayOf/3/3/26;10282=ValueOf/1/31/6,ValueOf/2/31/11,ValueOf/3/31/16,ValueOf/4/31/21,ValueOf/5/31/26,ValueOf/6/31/31,ValueOf/7/31/36,ValueOf/8/31/41,ValueOf/9/31/46,ValueOf/10/31/51,ValueOf/11/31/56,ValueOf/12/31/61,ValueOf/13/31/66,ValueOf/14/31/71,ValueOf/15/31/76,ValueOf/16/31/81,ValueOf/17/31/86,ValueOf/18/31/91,ValueOf/19/31/101,ValueOf/20/31/106,ValueOf/21/31/111,ValueOf/22/31/116,ValueOf/23/31/121,ValueOf/24/31/126,ValueOf/25/31/131,ValueOf/26/31/136,ValueOf/27/31/141,ValueOf/28/31/146,ValueOf/29/31/151,ValueOf/30/31/156,ValueOf/31/31/161;10476=ValueOf/1/1/122,Field/1/1/122;10653=ValueOf/1/11/2,ValueOf/2/11/3,ValueOf/3/11/6,ValueOf/4/11/7,ValueOf/5/11/10,ValueOf/6/11/11,ValueOf/7/11/14,ValueOf/8/11/17,ValueOf/9/11/18,ValueOf/10/11/19,ValueOf/11/11/20;10757=ValueOf/1/4/5,ValueOf/2/4/6,ValueOf/3/4/7,ValueOf/4/4/8;10800=ValueOf/1/7/5,ValueOf/2/7/6,ValueOf/3/7/7,ValueOf/4/7/8,ValueOf/5/7/9,ValueOf/6/7/10,ValueOf/7/7/11")]
[assembly: global::go.GoPositionMap("reflect/benchmark_test.go", "benchmark_test.cs", "ABkeABEyooK4ooK4ooK4ooK4ooK4ooK4ooK4ooK4ooK4ooK4grKSgoLcoqamggAsCIIAACyClLKSggAaDIIAASKygpKSgqaigqaSggAeErIAACSAkKQAChaykoK4spKC3IKCgoKCyoKSgoLcoqKCkoKC7oKmooKUgoIACAiChgAQFLKSgoKCooK4ggALCKSEgoKmgpS4koIAChaCgpKCyoKCkoLKgoKSggAJFKKSkoKmpoKSkoLKgoKSggARCoKGgoKCgoKCgoKClIKClIKCpgAFErKSkoKCgriSgoKCAAcQooKCgpQ=", "102-107:1;150-154:1;179-183:1;184-188:2;189-193:3;216-218:1;219-219:2;233-237:3;240-244:4;259-264:1;269-269:1;271-276:2;296-298:1;303-303:2;304-304:3;305-305:4;306-306:5;307-307:6;310-319:7;314-318:7.1;341-345:1;356-360:1;365-369:1;374-378:1;388-392:1;398-402:1;407-411:1;448-465:1;449-456:1.1;457-464:1.2", "", "26=ValueOf/1/7/9,Elem/1/7/9,ValueOf/2/7/10,Elem/2/7/10,ValueOf/3/7/11,Elem/3/7/11,ValueOf/4/7/12,Elem/4/7/12,ValueOf/5/7/13,Elem/5/7/13,ValueOf/6/7/14,Elem/6/7/14,ValueOf/7/7/15,Elem/7/7/15;234=Elem/1/1/15;301=Invoke/1/9/1,Invoke/2/9/2,Invoke/3/9/3,Invoke/4/9/4,Invoke/5/9/5,Invoke/6/9/6,Invoke/7/9/7,Invoke/8/9/8,Invoke/9/9/8;341=ValueOf/1/1/2;398=ValueOf/1/5/4,Invoke/1/5/4,ValueOf/2/5/5,Invoke/2/5/5,ValueOf/3/5/6,Invoke/3/5/6,ValueOf/4/5/7,Invoke/4/5/7,ValueOf/5/5/8,Invoke/5/5/8;580=ValueOf/1/6/4,ValueOf/2/6/4,ValueOf/3/6/5,ValueOf/4/6/5,ValueOf/5/6/6,ValueOf/6/6/6")]
[assembly: global::go.GoPositionMap("reflect/iter_test.go", "iter_test.cs", "ABkigtyCgpSC/IKCgpSUgriCgoKUlIK4goKClJSCuIKCgpSUgriCgoKUlIK4goKClJSCuIKCgpSUgriCgoKClJSCuIKCgIKklIK4gsqCgIKkgpSCuIKCuIKCgpSUgriCgoKUlIK4goKClIKCpoK4goIADAiC3PyCgoKUgoKmgriCgoKUgoKmgriCgoKUgoKmgtiCkoKCgoKUgpSClJSCtKSCgoKClIKUlIK4gqaCgoKUgoKmgriCgoKUgoKmgriCgoKUgpSCgqaCuIKCgpSCgpSCpoK4goIACBCCgoLMAAgMgoKCzA==", "34-45:1;46-57:2;58-69:3;70-81:4;82-93:5;94-105:6;106-117:7;118-130:8;131-142:9;143-160:10;161-167:11;167-178:12;179-190:13;191-205:14;225-239:1;240-254:2;255-269:3;270-290:4;291-306:5;307-311:6;311-325:7;326-340:8;341-358:9;359-376:10", "", "38=ValueOf/1/13/5,ValueOf/2/13/17,ValueOf/3/13/29,ValueOf/4/13/41,ValueOf/5/13/53,ValueOf/6/13/65,ValueOf/7/13/77,ValueOf/8/13/89,ValueOf/9/13/102,ValueOf/10/13/114,ValueOf/11/13/132,ValueOf/12/13/150,MethodByName/1/1/150,ValueOf/13/13/162;244=ValueOf/1/9/5,ValueOf/2/9/20,ValueOf/3/9/35,ValueOf/4/9/50,ValueOf/5/9/71,ValueOf/6/9/87,ValueOf/7/9/106,MethodByName/1/1/106,ValueOf/8/9/121,ValueOf/9/9/139")]
[assembly: global::go.GoPositionMap("reflect/map_test.go", "map_test.cs", "AAsakoLKgg==")]
[assembly: global::go.GoPositionMap("reflect/set_test.go", "set_test.cs", "ABYktJSCgoKCgpSAgraUgoKCgoKUgIK2lIKCgoKClICCtpSCgoKCgpSAgraUgoKCgoKCgpSAgraUgoKCgoKCgpSAgraEhIKCgoKCgpSAgtyGooKCgoK4goKCgoCCyJSCgoKCuJSSgoKCggAICAAOJICigKIACBCigoKCgILaAA8kooKCgoCC")]
[assembly: global::go.GoPositionMap("reflect/tostring_test.go", "tostring_test.cs", "ABQkooKClIKUpKSkgqSkgpS2goKClJSCpIKCgoKClJSCpIKCgoKCpIKkgoKCgoKClJSCpKSCpA==")]
[assembly: global::go.GoPositionMap("reflect/type_test.go", "type_test.cs", "ABQYggAHIIKCggALCoIABxiCqIKSAC0IogAQSLKSgIIACBCigriiggAMCIIADiKykoCCAA8MggAOIrKSgII=", "56-58:1;99-103:1;127-127:1;128-128:2;140-144:3;154-154:1;155-155:2;167-171:3", "", "55=TypeFor/1/1/1,ValueOf/1/1/1;170=TypeOf/1/9/5,TypeOf/2/9/6,ValueOf/1/2/7,MethodByName/1/2/7,Type/1/2/7,ValueOf/2/2/8,MethodByName/2/2/8,Type/2/2/8,TypeOf/3/9/9,TypeOf/4/9/10,TypeOf/5/9/11,TypeOf/6/9/12,TypeOf/7/9/13,TypeOf/8/9/14,TypeOf/9/9/15;210=TypeOf/1/9/5,TypeOf/2/9/6,ValueOf/1/2/7,MethodByName/1/2/7,Type/1/2/7,ValueOf/2/2/8,MethodByName/2/2/8,Type/2/2/8,TypeOf/3/9/9,TypeOf/4/9/10,TypeOf/5/9/11,TypeOf/6/9/12,TypeOf/7/9/13,TypeOf/8/9/14,TypeOf/9/9/15")]
[assembly: global::go.GoPositionMap("reflect/visiblefields_test.go", "visiblefields_test.cs", "ADokAIECpgSCspKCgoCStoKCgsqClIKCyoKClIIAEhCSjoKClII=", "295-328:1")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("reflect_test")]
public static partial class reflect_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    [GoLocalName("V")] internal partial class BenchmarkMap_V {}
    [GoLocalName("T")] internal partial class TestArrayOfGC_T {}
    [GoLocalName("T")] internal partial class TestChanOfGC_T {}
    [GoLocalName("BigP")] internal partial class TestExported_BigP {}
    [GoLocalName("p")] internal partial class TestExported_p {}
    [GoLocalName("T")] internal partial class TestMapOfGCKeys_T {}
    [GoLocalName("T")] internal partial class TestMapOfGCValues_T {}
    [GoLocalName("T")] internal partial class TestPtrToGC_T {}
    [GoLocalName("T")] internal partial class TestSliceOfGC_T {}
    [GoLocalName("T")] internal partial class TestStructOfGC_T {}
    internal partial interface BenchmarkSetZero_type_Interface {}
    [GoLocalName("T1")] internal partial interface TestCallPanic_T1 {}
    [GoLocalName("t0")] internal partial interface TestCallPanic_t0 {}
    [GoLocalName("I")] internal partial interface TestMethodPkgPath_I {}
    internal partial interface TestMethodPkgPath_i {}
    internal partial interface TestMethod_x {}
    [GoLocalName("Named")] internal partial interface TestStructOfEmbeddedIfaceMethodCall_Named {}
    [GoLocalName("Iface")] internal partial interface TestStructOfWithInterface_Iface {}
    [GoLocalName("IfaceSet")] internal partial interface TestStructOfWithInterface_IfaceSet {}
    [GoLocalName("I")] internal partial interface TestTypeAssertAllocs_I {}
    internal partial interface TestTypeAssert_type {}
    internal partial interface notASTExpr {}
    internal partial interface tinter {}
    internal partial interface typeᴛ33_x {}
    internal partial interface unexpI {}
    [GoValueClone("A")] internal partial struct @private {}
    internal partial struct BenchmarkCallArgCopy_sizes {}
    [GoLocalName("Int1024")] [GoValueClone("a")] internal partial struct BenchmarkIsZero_Int1024 {}
    [GoLocalName("Int4")] internal partial struct BenchmarkIsZero_Int4 {}
    [GoLocalName("Int512")] [GoValueClone("a1", "a2", "a3", "a4", "a5", "a6", "a7", "a8", "a9", "a10", "a11", "a12", "a13", "a14", "a15", "a16")] internal partial struct BenchmarkIsZero_Int512 {}
    [GoValueClone("ArrayComparable", "ArrayIncomparable", "StructIncomparable", "ArrayInt_4", "ArrayInt_1024", "ArrayInt_1024_NoZero", "ArrayStruct4Int_1024", "ArrayChanInt_1024", "StructInt_512")] internal partial struct BenchmarkIsZero_s {}
    [GoLocalName("S")] internal partial struct BenchmarkMap_S {}
    internal partial struct BenchmarkMap_tests {}
    [GoLocalName("T")] internal partial struct BenchmarkPtrTo_T {}
    [GoValueClone("Array")] internal partial struct BenchmarkSetZero_type {}
    [GoLocalName("Big")] [GoValueClone("D")] internal partial struct BenchmarkZero_Big {}
    [GoLocalName("Small")] internal partial struct BenchmarkZero_Small {}
    [GoLocalName("bm")] internal partial struct BenchmarkZero_bm {}
    internal partial struct TestAddr_p {}
    internal partial struct TestAddr_s {}
    [GoLocalName("T1")] internal partial struct TestAlignment_T1 {}
    [GoLocalName("T1inner")] internal partial struct TestAlignment_T1inner {}
    [GoLocalName("T2")] internal partial struct TestAlignment_T2 {}
    [GoLocalName("T2inner")] internal partial struct TestAlignment_T2inner {}
    internal partial struct TestAll_i {}
    internal partial struct TestAllocations_type {}
    internal partial struct TestAppend_i {}
    [GoLocalName("T")] internal partial struct TestArrayOfDirectIface_T {}
    [GoLocalName("T")] internal partial struct TestArrayOfDirectIface_Tᴛ1 {}
    [GoLocalName("T")] internal partial struct TestArrayOf_T {}
    [GoLocalName("Tfloat")] internal partial struct TestArrayOf_Tfloat {}
    [GoLocalName("Tint")] internal partial struct TestArrayOf_Tint {}
    [GoLocalName("Tint")] internal partial struct TestArrayOf_Tintᴛ1 {}
    [GoLocalName("Tint")] internal partial struct TestArrayOf_Tintᴛ2 {}
    [GoLocalName("Tint")] internal partial struct TestArrayOf_Tintᴛ3 {}
    [GoLocalName("Tstring")] internal partial struct TestArrayOf_Tstring {}
    [GoLocalName("Tstruct")] internal partial struct TestArrayOf_Tstruct {}
    [GoLocalName("TstructUV")] internal partial struct TestArrayOf_TstructUV {}
    [GoLocalName("TstructUV")] internal partial struct TestArrayOf_TstructUVᴛ1 {}
    [GoLocalName("Tstruct")] [GoValueClone("V")] internal partial struct TestArrayOf_Tstructᴛ1 {}
    [GoLocalName("Tstruct")] internal partial struct TestArrayOf_Tstructᴛ2 {}
    internal partial struct TestArrayOf_tests {}
    internal partial struct TestBigUnnamedStruct_b {}
    [GoLocalName("A")] internal partial struct TestBytes_A {}
    [GoLocalName("AB")] internal partial struct TestBytes_AB {}
    [GoLocalName("B")] internal partial struct TestBytes_B {}
    [GoLocalName("S")] internal partial struct TestBytes_S {}
    [GoLocalName("SB")] internal partial struct TestBytes_SB {}
    [GoLocalName("T")] internal partial struct TestCallArgLive_T {}
    [GoLocalName("T")] internal partial struct TestCallPanic_T {}
    [GoLocalName("T2")] internal partial struct TestCallPanic_T2 {}
    [GoLocalName("complex")] internal partial struct TestCanIntUintFloatComplex_complex {}
    [GoLocalName("float")] internal partial struct TestCanIntUintFloatComplex_float {}
    [GoLocalName("integer")] internal partial struct TestCanIntUintFloatComplex_integer {}
    [GoValueClone("want")] internal partial struct TestCanIntUintFloatComplex_type {}
    internal partial struct TestCanIntUintFloatComplex_typeᴛ1 {}
    [GoLocalName("uinteger")] internal partial struct TestCanIntUintFloatComplex_uinteger {}
    [GoLocalName("Embed")] internal partial struct TestCanSetField_Embed {}
    [GoLocalName("S1")] internal partial struct TestCanSetField_S1 {}
    [GoLocalName("S2")] internal partial struct TestCanSetField_S2 {}
    [GoLocalName("S3")] internal partial struct TestCanSetField_S3 {}
    [GoLocalName("S4")] internal partial struct TestCanSetField_S4 {}
    [GoLocalName("embed")] internal partial struct TestCanSetField_embed {}
    [GoLocalName("testCase")] internal partial struct TestCanSetField_testCase {}
    internal partial struct TestCanSetField_tests {}
    [GoLocalName("T")] internal partial struct TestChanOfDir_T {}
    [GoLocalName("T1")] internal partial struct TestChanOfDir_T1 {}
    [GoLocalName("T")] internal partial struct TestChanOf_T {}
    [GoLocalName("T1")] internal partial struct TestChanOf_T1 {}
    internal partial struct TestClear_tests {}
    [GoLocalName("myFloat32")] internal partial struct TestConvertNaNs_myFloat32 {}
    [GoLocalName("P")] internal partial struct TestExported_P {}
    [GoLocalName("P2")] internal partial struct TestExported_P2 {}
    [GoLocalName("exportTest")] internal partial struct TestExported_exportTest {}
    [GoLocalName("p3")] internal partial struct TestExported_p3 {}
    [GoLocalName("ΦExported")] internal partial struct TestExported_ΦExported {}
    [GoLocalName("φUnexported")] internal partial struct TestExported_φUnexported {}
    [GoLocalName("A")] internal partial struct TestFieldByIndexErr_A {}
    [GoLocalName("B")] internal partial struct TestFieldByIndexErr_B {}
    [GoLocalName("P")] internal partial struct TestFieldByIndexNil_P {}
    [GoLocalName("T")] internal partial struct TestFieldByIndexNil_T {}
    internal partial struct TestFieldPkgPath_i {}
    [GoLocalName("localOtherPkgFields")] internal partial struct TestFieldPkgPath_localOtherPkgFields {}
    [GoLocalName("pkgpathTest")] internal partial struct TestFieldPkgPath_pkgpathTest {}
    [GoLocalName("x")] internal partial struct TestFieldPkgPath_x {}
    [GoLocalName("S")] internal partial struct TestFuncLayout_S {}
    [GoLocalName("test")] internal partial struct TestFuncLayout_test {}
    [GoLocalName("K")] internal partial struct TestFuncOf_K {}
    [GoLocalName("T1")] internal partial struct TestFuncOf_T1 {}
    [GoLocalName("V")] internal partial struct TestFuncOf_V {}
    internal partial struct TestFuncOf_testCases {}
    [GoLocalName("MyBuffer")] internal partial struct TestImplicitMapConversion_MyBuffer {}
    internal partial struct TestImportPath_tests {}
    internal partial struct TestInterfaceExtraction_s {}
    internal partial struct TestInterfaceGet_inter {}
    internal partial struct TestInterfaceSet_s {}
    internal partial struct TestInterfaceValue_inter {}
    [GoLocalName("T")] internal partial struct TestInvalid_T {}
    internal partial struct TestIsNil_doNil {}
    internal partial struct TestIsNil_doNilᴛ1 {}
    internal partial struct TestIsNil_doNilᴛ2 {}
    internal partial struct TestIsNil_doNilᴛ3 {}
    internal partial struct TestIsNil_doNilᴛ4 {}
    internal partial struct TestIsNil_doNilᴛ5 {}
    internal partial struct TestIsNil_doNilᴛ6 {}
    internal partial struct TestIsNil_fi {}
    internal partial struct TestIsNil_mi {}
    [GoLocalName("S")] internal partial struct TestIsRegularMemory_S {}
    [GoLocalName("args")] internal partial struct TestIsRegularMemory_args {}
    internal partial struct TestIsRegularMemory_i {}
    internal partial struct TestIsRegularMemory_iᴛ1 {}
    internal partial struct TestIsRegularMemory_iᴛ2 {}
    internal partial struct TestIsRegularMemory_iᴛ3 {}
    internal partial struct TestIsRegularMemory_iᴛ4 {}
    internal partial struct TestIsRegularMemory_iᴛ5 {}
    internal partial struct TestIsRegularMemory_tests {}
    internal partial struct TestIsZero_type {}
    internal partial struct TestIsZero_typeᴛ1 {}
    internal partial struct TestIsZero_typeᴛ2 {}
    internal partial struct TestIsZero_typeᴛ3 {}
    internal partial struct TestIsZero_typeᴛ4 {}
    internal partial struct TestIsZero_typeᴛ5 {}
    [GoValueClone("a")] internal partial struct TestIsZero_typeᴛ6 {}
    [GoValueClone("a")] internal partial struct TestIsZero_typeᴛ7 {}
    [GoValueClone("a")] internal partial struct TestIsZero_typeᴛ8 {}
    internal partial struct TestIssue22031_s {}
    [GoLocalName("s")] internal partial struct TestIssue22031_sᴛ1 {}
    [GoLocalName("t1")] internal partial struct TestIssue22031_t1 {}
    [GoLocalName("t2")] internal partial struct TestIssue22031_t2 {}
    [GoLocalName("T")] internal partial struct TestMakeFuncInvalidReturnAssignments_T {}
    [GoLocalName("U")] internal partial struct TestMakeFuncInvalidReturnAssignments_U {}
    [GoLocalName("T")] internal partial struct TestMakeFuncValidReturnAssignments_T {}
    internal partial struct TestMakeFuncValidReturnAssignments_i {}
    [GoLocalName("KV")] internal partial struct TestMapOfGCBigKey_KV {}
    [GoLocalName("K")] internal partial struct TestMapOf_K {}
    [GoLocalName("V")] internal partial struct TestMapOf_V {}
    [GoLocalName("S")] internal partial struct TestMap_S {}
    internal partial struct TestMethodPkgPath_tests {}
    internal partial struct TestMethodValue_type {}
    internal partial struct TestPtrToMethods_y {}
    [GoLocalName("B")] internal partial struct TestSetBytes_B {}
    internal partial struct TestSetIter_i {}
    [GoLocalName("T")] internal partial struct TestSetPanic_T {}
    [GoLocalName("T2")] internal partial struct TestSetPanic_T2 {}
    [GoLocalName("t0")] internal partial struct TestSetPanic_t0 {}
    [GoLocalName("t1")] internal partial struct TestSetPanic_t1 {}
    [GoLocalName("T")] internal partial struct TestSliceOf_T {}
    [GoLocalName("T1")] internal partial struct TestSliceOf_T1 {}
    [GoLocalName("T")] internal partial struct TestSmallZero_T {}
    [GoLocalName("padded")] internal partial struct TestStructArg_padded {}
    internal partial struct TestStructOfAnonymous_type {}
    [GoLocalName("T")] [GoValueClone("X")] internal partial struct TestStructOfDirectIface_T {}
    [GoLocalName("T")] [GoValueClone("X")] internal partial struct TestStructOfDirectIface_Tᴛ1 {}
    [GoLocalName("S1")] internal partial struct TestStructOfExportRules_S1 {}
    [GoLocalName("s2")] internal partial struct TestStructOfExportRules_s2 {}
    internal partial struct TestStructOfExportRules_tests {}
    [GoLocalName("ΦType")] internal partial struct TestStructOfExportRules_ΦType {}
    [GoLocalName("φType")] internal partial struct TestStructOfExportRules_φType {}
    internal partial struct TestStructOfGenericAlg_tests {}
    [GoLocalName("test")] internal partial struct TestStructOfTooLarge_test {}
    internal partial struct TestStructOfWithInterface_tests {}
    [GoValueClone("Z")] internal partial struct TestStructOf_i {}
    [GoValueClone("G2")] internal partial struct TestStructOf_iᴛ1 {}
    internal partial struct TestStructOf_y {}
    internal partial struct TestStructOf_yᴛ1 {}
    [GoLocalName("I")] internal partial struct TestSwapper_I {}
    [GoLocalName("S")] internal partial struct TestSwapper_S {}
    [GoLocalName("pair")] internal partial struct TestSwapper_pair {}
    [GoLocalName("pairPtr")] internal partial struct TestSwapper_pairPtr {}
    internal partial struct TestSwapper_tests {}
    internal partial struct TestTypeFieldOutOfRangePanic_i {}
    internal partial struct TestTypeFieldOutOfRangePanic_testIndices {}
    [GoLocalName("mystring")] internal partial struct TestTypeFor_mystring {}
    internal partial struct TestTypeFor_testcases {}
    [GoLocalName("T")] internal partial struct TestTypeOfTypeOf_T {}
    [GoLocalName("stringTest")] internal partial struct TestTypeStrings_stringTest {}
    internal partial struct TestType_CanSeq2_tests {}
    internal partial struct TestType_CanSeq_tests {}
    internal partial struct TestUnaddressableField_localBuffer {}
    internal partial struct TestValuePointerAndUnsafePointer_tests {}
    internal partial struct TestValueSeq2_tests {}
    internal partial struct TestValueSeq_tests {}
    internal partial struct TestValue_Comparable_type {}
    internal partial struct TestValue_Comparable_typeᴛ1 {}
    internal partial struct TestValue_Comparable_typeᴛ2 {}
    internal partial struct TestValue_EqualNonComparable_type {}
    [GoLocalName("S")] [GoValueClone("T")] internal partial struct TestZeroSet_S {}
    [GoLocalName("T")] internal partial struct TestZeroSet_T {}
    [GoValueClone("b")] internal partial struct _Complex {}
    internal partial struct appendTestsᴛ1 {}
    internal partial struct big {}
    internal partial struct caseInfo {}
    internal partial struct choice {}
    internal partial struct comparableTestsᴛ1 {}
    internal partial struct convertTestsᴛ1 {}
    internal partial struct deepEqualPerfTestsᴛ1 {}
    internal partial struct emptyStruct {}
    internal partial struct exhaustive {}
    internal partial struct fieldsTestsᴛ1 {}
    internal partial struct implementsTestsᴛ1 {}
    [GoLocalName("Bigptrscalar")] internal partial struct init_Bigptrscalar {}
    [GoLocalName("Int64")] internal partial struct init_Int64 {}
    [GoLocalName("Ptr")] internal partial struct init_Ptr {}
    [GoLocalName("Ptrscalar")] internal partial struct init_Ptrscalar {}
    [GoLocalName("Scalar")] internal partial struct init_Scalar {}
    [GoLocalName("Scalarptr")] internal partial struct init_Scalarptr {}
    internal partial struct inner {}
    internal partial struct integer {}
    internal partial struct iᴛ1 {}
    internal partial struct iᴛ2 {}
    internal partial struct methodIter {}
    internal partial struct methodIter2 {}
    internal partial struct myint {}
    internal partial struct nameTest {}
    internal partial struct namedBool {}
    internal partial struct namedBytes {}
    internal partial struct nonEmptyStruct {}
    internal partial struct notAnExpr {}
    internal partial struct outer {}
    internal partial struct pair {}
    internal partial struct sFG {}
    internal partial struct selectWatchᴛ1 {}
    internal partial struct self {}
    internal partial struct sinkAllᴛ1 {}
    internal partial struct sourceAllᴛ1 {}
    internal partial struct structField {}
    internal partial struct structWithSelfPtr {}
    internal partial struct tagGetTestsᴛ1 {}
    internal partial struct testTypeWithMethod {}
    internal partial struct timp {}
    internal partial struct two {}
    internal partial struct typeᴛ22_x {}
    internal partial struct typeᴛ24_x {}
    internal partial struct typeᴛ25_x {}
    internal partial struct typeᴛ26_x {}
    internal partial struct typeᴛ27_x {}
    internal partial struct typeᴛ28_x {}
    internal partial struct typeᴛ29_x {}
    internal partial struct typeᴛ30_x {}
    internal partial struct typeᴛ31_x {}
    internal partial struct typeᴛ32_x {}
    internal partial struct typeᴛ34_x {}
    internal partial struct unexp {}
    public partial class IntPtr {}
    public partial class IntPtr1 {}
    public partial class Loop {}
    public partial class MyBytesArrayPtr {}
    public partial class MyBytesArrayPtr0 {}
    [GoLocalName("Loopy")] public partial interface Loopyᴅ {}
    public partial interface Tinter {}
    public partial interface Δtypeᴛ35 {}
    public partial struct A {}
    public partial struct B1 {}
    public partial struct B<T> {}
    public partial struct Basic {}
    public partial struct BytesChan {}
    public partial struct BytesChanRecv {}
    public partial struct BytesChanSend {}
    public partial struct Ch {}
    public partial struct ComparableStruct {}
    public partial struct D1 {}
    public partial struct D2 {}
    public partial struct DeepEqualTest {}
    public partial struct DirectIfaceT {}
    public partial struct Empty {}
    public partial struct FTest {}
    public partial struct Impl {}
    public partial struct Inner {}
    public partial struct InnerInt {}
    public partial struct IntChan {}
    public partial struct IntChanRecv {}
    public partial struct IntChanSend {}
    public partial struct KeepMethodLive {}
    public partial struct MagicLastTypeNameForTestingRegisterABI {}
    public partial struct MyByte {}
    public partial struct MyBytes {}
    public partial struct MyBytesArray {}
    public partial struct MyBytesArray0 {}
    public partial struct MyRunes {}
    public partial struct MyString {}
    public partial struct MyStruct {}
    public partial struct MyStruct1 {}
    public partial struct MyStruct1_x {}
    public partial struct MyStruct2 {}
    public partial struct MyStruct2_x {}
    public partial struct N {}
    public partial struct NonComparableStruct {}
    public partial struct NonExportedFirst {}
    public partial struct NotBasic {}
    public partial struct Outer {}
    public partial struct OuterInt {}
    public partial struct Point {}
    public partial struct Private {}
    public partial struct Public {}
    public partial struct R0 {}
    public partial struct R1 {}
    public partial struct R10 {}
    public partial struct R11 {}
    public partial struct R12 {}
    public partial struct R13 {}
    public partial struct R14 {}
    public partial struct R15 {}
    public partial struct R16 {}
    public partial struct R17 {}
    public partial struct R18 {}
    public partial struct R19 {}
    public partial struct R2 {}
    public partial struct R20 {}
    public partial struct R21 {}
    public partial struct R22 {}
    public partial struct R23 {}
    public partial struct R24 {}
    public partial struct R3 {}
    public partial struct R4 {}
    public partial struct R5 {}
    public partial struct R6 {}
    public partial struct R7 {}
    public partial struct R8 {}
    public partial struct R9 {}
    public partial struct RS1 {}
    public partial struct RS2 {}
    public partial struct RS3 {}
    public partial struct Rec1 {}
    public partial struct Rec2 {}
    public partial struct Recursive {}
    public partial struct S {}
    public partial struct S0 {}
    public partial struct S1 {}
    public partial struct S10 {}
    public partial struct S11 {}
    public partial struct S12 {}
    public partial struct S13 {}
    public partial struct S14 {}
    public partial struct S15 {}
    public partial struct S16 {}
    public partial struct S1x {}
    public partial struct S1y {}
    public partial struct S2 {}
    public partial struct S3 {}
    public partial struct S4 {}
    public partial struct S5 {}
    public partial struct S6 {}
    public partial struct S7 {}
    public partial struct S8 {}
    public partial struct S9 {}
    public partial struct SF {}
    public partial struct SF1 {}
    public partial struct SF2 {}
    public partial struct SFG {}
    public partial struct SFG1 {}
    public partial struct SFG2 {}
    public partial struct SFGH {}
    public partial struct SFGH1 {}
    public partial struct SFGH2 {}
    public partial struct SFGH3 {}
    public partial struct SG {}
    public partial struct SG1 {}
    public partial struct SettablePointer {}
    public partial struct SettableStruct {}
    public partial struct Struct1 {}
    public partial struct Struct10 {}
    public partial struct Struct11 {}
    public partial struct Struct12 {}
    public partial struct Struct13 {}
    [GoValueClone("X")] public partial struct Struct14 {}
    [GoValueClone("X")] public partial struct Struct15 {}
    [GoValueClone("Y")] public partial struct Struct15_X {}
    [GoValueClone("D")] public partial struct Struct2 {}
    [GoValueClone("D")] public partial struct Struct3 {}
    public partial struct Struct4 {}
    public partial struct Struct5 {}
    public partial struct Struct6 {}
    public partial struct Struct7 {}
    public partial struct Struct8 {}
    public partial struct Struct9 {}
    public partial struct StructFewRegs {}
    public partial struct StructFillRegs {}
    public partial struct StructI {}
    public partial struct StructIPtr {}
    public partial struct StructWithMethods {}
    public partial struct T {}
    public partial struct T1 {}
    public partial struct Talias1 {}
    public partial struct Talias2 {}
    public partial struct Tbigp {}
    public partial struct Tbigv {}
    public partial struct TheNameOfThisTypeIsExactly255BytesLongSoWhenTheCompilerPrependsTheReflectTestPackageNameAndExtraStarTheLinkerRuntimeAndReflectPackagesWillHaveToCorrectlyDecodeTheSecondLengthByte0123456789_0123456789_0123456789_0123456789_0123456789_012345678 {}
    public partial struct Tint {}
    public partial struct Tm1 {}
    public partial struct Tm2 {}
    public partial struct Tm3 {}
    public partial struct Tm4 {}
    public partial struct Tsmallp {}
    public partial struct Tsmallv {}
    public partial struct Twordp {}
    public partial struct Twordv {}
    public partial struct UnExportedFirst {}
    public partial struct UnexpT {}
    public partial struct ValueEqualTest {}
    public partial struct WC {}
    public partial struct XM {}
    public partial struct Xbigptrscalar {}
    public partial struct Xptr {}
    public partial struct Xptrscalar {}
    public partial struct Xscalar {}
    public partial struct Xscalarptr {}
    public partial struct typeᴛ38_A {}
    public partial struct ΔM {}
    public partial struct Δtype {}
    public partial struct Δtypeᴛ1 {}
    public partial struct Δtypeᴛ10 {}
    public partial struct Δtypeᴛ11 {}
    public partial struct Δtypeᴛ12 {}
    public partial struct Δtypeᴛ13 {}
    [GoValueClone("x")] public partial struct Δtypeᴛ14 {}
    public partial struct Δtypeᴛ15 {}
    public partial struct Δtypeᴛ16 {}
    public partial struct Δtypeᴛ17 {}
    public partial struct Δtypeᴛ18 {}
    public partial struct Δtypeᴛ19 {}
    public partial struct Δtypeᴛ2 {}
    public partial struct Δtypeᴛ20 {}
    public partial struct Δtypeᴛ21 {}
    public partial struct Δtypeᴛ22 {}
    public partial struct Δtypeᴛ23 {}
    public partial struct Δtypeᴛ24 {}
    public partial struct Δtypeᴛ25 {}
    public partial struct Δtypeᴛ26 {}
    public partial struct Δtypeᴛ27 {}
    public partial struct Δtypeᴛ28 {}
    public partial struct Δtypeᴛ29 {}
    public partial struct Δtypeᴛ3 {}
    public partial struct Δtypeᴛ30 {}
    public partial struct Δtypeᴛ31 {}
    public partial struct Δtypeᴛ32 {}
    public partial struct Δtypeᴛ33 {}
    public partial struct Δtypeᴛ34 {}
    public partial struct Δtypeᴛ36 {}
    public partial struct Δtypeᴛ37 {}
    public partial struct Δtypeᴛ38 {}
    public partial struct Δtypeᴛ39 {}
    public partial struct Δtypeᴛ4 {}
    public partial struct Δtypeᴛ40 {}
    public partial struct Δtypeᴛ41 {}
    public partial struct Δtypeᴛ42 {}
    public partial struct Δtypeᴛ43 {}
    public partial struct Δtypeᴛ44 {}
    public partial struct Δtypeᴛ5 {}
    public partial struct Δtypeᴛ6 {}
    public partial struct Δtypeᴛ7 {}
    public partial struct Δtypeᴛ8 {}
    public partial struct Δtypeᴛ9 {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸbase64() => builtin.initPackage(typeof(encoding.base64_package));
    [GoInit] internal static void initᴛᴛimportꓸflag() => builtin.initPackage(typeof(flag_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸast() => builtin.initPackage(typeof(global::go.go.ast_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸtoken() => builtin.initPackage(typeof(global::go.go.token_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸabi() => builtin.initPackage(typeof(@internal.abi_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸrace() => builtin.initPackage(typeof(@internal.race_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtestenv() => builtin.initPackage(typeof(@internal.testenv_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸiter() => builtin.initPackage(typeof(iter_package));
    [GoInit] internal static void initᴛᴛimportꓸmaps() => builtin.initPackage(typeof(maps_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸrand() => builtin.initPackage(typeof(global::go.math.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸnet() => builtin.initPackage(typeof(net_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸruntimeꓸdebug() => builtin.initPackage(typeof(global::go.runtime.debug_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸsyncꓸatomic() => builtin.initPackage(typeof(global::go.sync.atomic_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtestingꓸquick() => builtin.initPackage(typeof(global::go.testing.quick_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.reflect_package));
    }
}
