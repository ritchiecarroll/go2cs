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
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
global using tokenꓸFile = go.go.token_package.ΔFile;
global using tokenꓸPos = go.go.token_package.ΔPos;
global using tokenꓸPosition = go.go.token_package.ΔPosition;
using abi = go.@internal.abi_package;
// </ImportedTypeAliases>

using go;
using static global::go.reflect_test_package;

// <ExportedTypeAliases>
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
[assembly: GoImplement<timp, TestCallPanic_T1>]
[assembly: GoImplement<timp, TestCallPanic_t0>]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<abi.Type, ж<abi.Type>>(Indirect = true)]
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: global::go.GoPositionMap("reflect/abi_test.go", "abi_test.cs", "ACYw+AAGEIKCgpa4grqCgoKClIKWgoKCgpSClIKWgoKCgpSCloKCgoKUgpSCloKCkoKClIKUgrQAECCCgqaigqaCgqqigqqigqb4zIKCkqKCgpSClIKClIKCgoKU2NT4zIKCgpSUgoKSksKCkpSCgoKClLiO4oKClKiCgpaWgsbUACtm6KKqoqqiqqKqoqqiqqKqoqqiqqKqoqqiqqKqoqrCqsKqwqqiqqKqoqqiqqKqoqrCqsKqoqqiqqKqoqqiqqKqoqqiqqKqoqrCqsKqoqqiAAIQ0tYAKmCiqqKqoqqiqqKqoqqiqqKqoqqiqqKqoqqiqqKqoqrCqsKqwqqiqqKqoqqiqqKqoqrCqsKqoqqiqqKqoqqiqqKqoqqiqqKqoqrCqsKqoqqiAHfsAdaCgoKUpoKUgoKmpoKGgpSCgg==", "36-40:1;169-189:1;204-209:1;214-227:2;229-256:3;237-240:3.1;968-974:1;979-981:1")]
[assembly: global::go.GoPositionMap("reflect/all_test.go", "all_test.cs", "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kpKSlIKCpoK+soK4kpKigpSCABEcAAwegoKCgIIAGgqCAA02goKAggAKCoIADCKCgJLagoKCgoKCgpSmABUGgqQAN4wBgoKCggAIFNKChpKSogAWKoKCgoKCloKCgoIAEQiCAAMUgoLuooCkooSAgrjWgoKCgJLIgoKCgJIACAiClrKAkoKipoKCgJK4grKAkoKipoKCgJK4yoKCkoKCgoKClIKUggAGEKKClIL4lKKAkoKikqKAkoKikqKAkoKimKKEsoCSgqKSsoCSgqK4grKAkoKikrKAkoKikrKAkoKi9qaCgoKAksiUgoKCgoKAksiUgoKCgoKCgpSCvKKCgoKUgqaigpaCloKEggASCILMgpKSkpKUkpSCgoKClIKUkpSSloKCgpSClIKUgpiSgoKAgqSCgqiSkqaCgoKAgqSCgIK4gpKilKLKgoKCgIKkgIIADg6CgoCCpIKAgviCgoKCggAKEoKCgoKWgoKClrKCgoCCtoLUgoKUgoLogoKCgpaCgoKWsoKCgIK2gtSCgpSCggARCKKCgqKCoqKiAKEBzgKCgoKUgoIADRjE1ACAAYACooKSgIKUpoCClKSCloKWgIIADAqiggASLMSQpoCC2oKChIKCooKCgrSkAAsGgoKClICAlIKClIKCgoKmloCQkJQABRaCkpKCggAODIKCkpKCgqKEAAgcgpKSgIKkgIIADA6ykoSEgoKUgpaCgqiQlpKAppKAkoCmyqC+woSChIKEgpaCgoKCAAQUAAkCgoKCqISCog==", "377-383:1;559-576:1;737-737:1;747-747:1;764-780:2;767-771:2.1;782-793:3;795-809:4;881-881:1;882-882:2;926-944:1;945-963:2;1138-1145:1;1287-1296:1;1288-1292:1.1;1510-1510:1;1511-1511:2;1512-1512:3;1547-1554:4;1548-1552:4.1;1646-1646:1;1647-1647:2;1750-1754:1;1792-1792:1;1796-1808:2;1855-1855:3;1871-1871:4;2064-2072:1;2111-2117:1;2192-2192:1;2244-2248:1;2246-2246:1.1;2265-2265:1;2282-2282:1;2283-2285:2;2301-2301:1;2302-2302:2;2354-2356:1;2361-2364:2;2369-2371:3;2377-2379:4;2385-2391:1;2387-2389:1.1;2393-2400:2;2395-2398:2.1;2402-2409:3;2404-2407:3.1;2411-2419:4;2415-2417:4.1;2834-2839:1;2841-2850:2;2854-2858:3;2896-2896:4;3209-3222:1;3508-3511:1;3518-3527:1;3528-3536:2;3537-3545:3;3540-3540:3.1;3548-3552:4;3634-3634:1;3635-3635:2;3636-3636:3;3650-3650:4;3651-3651:5;3652-3652:6;3656-3656:7;3673-3673:1;3674-3674:2;3675-3675:3;3676-3676:4;3677-3677:5;3690-3690:6;3691-3691:7;3694-3694:8;3695-3695:9;3716-3716:1;3717-3717:2;3735-3737:1;3767-3767:1;3768-3768:2;3782-3782:3;3783-3783:4;3878-3878:1;3879-3879:2;3880-3880:3;3881-3881:4;3882-3882:5;3886-3886:1;3887-3887:2;3888-3888:3;3916-3916:4;3917-3917:5;3918-3918:6;3919-3919:7;3920-3920:8;3921-3921:9;3922-3922:10;3923-3923:11;3924-3924:12;3925-3925:13;3926-3926:14;3927-3927:15;3928-3928:16;3929-3929:17;3930-3930:18;3931-3931:19;3932-3932:20;3933-3933:21;3934-3934:22;3935-3935:23;3936-3936:24;3937-3937:25;3938-3938:26;3939-3939:27;3940-3940:28;3944-3944:29;3945-3945:30;3946-3946:31;3947-3947:32;3948-3948:33;3949-3949:34;3950-3950:35;3951-3951:36;3952-3952:37;3953-3953:38;3954-3954:39;3955-3955:40;3956-3956:41;3957-3957:42;3958-3958:43;3959-3959:44;3960-3960:45;3961-3961:46;3962-3962:47;3963-3963:48;3964-3964:49;3965-3965:50;3966-3966:51;3967-3967:52;3968-3968:53;4003-4003:1;4004-4004:2;4005-4005:3;4006-4006:4;4010-4010:5;4011-4011:6;4012-4012:7;4013-4013:8;4014-4014:9;4015-4015:10;4016-4016:11;4017-4017:12;4019-4019:13;4020-4020:14;4021-4021:15;4022-4022:16;4024-4024:17;4025-4025:18;4026-4026:19;4027-4027:20;4029-4029:21;4030-4030:22;4031-4031:23;4032-4032:24;4034-4034:25;4035-4035:26;4036-4036:27;4037-4037:28;4039-4039:29;4040-4040:30;4041-4041:31;4042-4042:32;4044-4044:33;4045-4045:34;4046-4046:35;4047-4047:36;4052-4052:1;4053-4053:2;4054-4054:3;4055-4055:4;4056-4056:5;4057-4057:6;4058-4058:7;4059-4059:8;4060-4060:9;4061-4061:10;4062-4062:11;4063-4063:12;4064-4064:13;4065-4065:14;4066-4066:15;4067-4067:16;4068-4068:17;4069-4069:18;4070-4070:19;4071-4071:20;4072-4072:21;4073-4073:22;4074-4074:23;4075-4075:24;4076-4076:25;4077-4077:26;4078-4078:27;4079-4079:28;4080-4080:29;4081-4081:30;4082-4082:31;4083-4083:32;4084-4084:33;4085-4085:34;4086-4086:35;4087-4087:36;4088-4088:37;4089-4089:38;4090-4090:39;4091-4091:40;4092-4092:41;4096-4118:1;4755-4757:1;4762-4764:2;4942-4942:1;4948-4948:2;4954-4954:3;4960-4960:4;4966-4966:5;4972-4972:6;4978-4978:7;4984-4984:8;4990-4990:9;4996-4996:10;5002-5008:11;5100-5100:1;5174-5176:1;5213-5218:1;5252-5257:1;5260-5265:2;5268-5272:3;5395-5400:1;5401-5406:2;5407-5412:3;5434-5445:1;5435-5443:1.1;5561-5579:2;5630-5630:1;5859-5862:1;5868-5871:2;5911-5918:3;5912-5916:3.1;5961-5963:4;5975-5977:5;5997-5999:6;6018-6020:7;6047-6049:1;6109-6129:1;6110-6127:1.1;6205-6211:1;6213-6215:2;6271-6271:1;6387-6396:1;6429-6429:2;6430-6430:3;6523-6523:1;6533-6533:1;6634-6634:1;6650-6650:1;6698-6704:1;6794-6794:1;6801-6805:2;6815-6819:1;6871-6871:1;6876-6878:1;6924-6926:1;6931-6932:1;6933-6936:2;6946-6946:1;6948-6965:2;6966-6971:3;6982-6986:1;7028-7030:1;7052-7052:2;7060-7060:3;7068-7068:4;7076-7076:5;7085-7085:6;7093-7093:7;7101-7101:8;7110-7110:9;7127-7155:10;7355-7359:1;7392-7394:1;7402-7409:2;7426-7429:1;7511-7511:1;7535-7540:1;7639-7641:1;7731-7735:1;7732-7732:1.1;7746-7750:2;7747-7747:2.1;7787-7790:3;7798-7802:1;7799-7799:1.1;7803-7807:2;7804-7804:2.1;7808-7812:3;7809-7809:3.1;7819-7823:4;7820-7820:4.1;7824-7828:5;7825-7825:5.1;7833-7837:6;7834-7834:6.1;7838-7842:7;7839-7839:7.1;7843-7847:8;7844-7844:8.1;7929-7931:1;7932-7934:2;7944-7946:3;7947-7949:4;7953-7955:5;7956-7958:6;7959-7961:7;7962-7964:8;7998-8000:9;8001-8003:10;8059-8068:1;8060-8065:1.1;8069-8072:2;8092-8101:1;8093-8098:1.1;8102-8105:2;8479-8479:1;8492-8492:2;8507-8513:1;8523-8523:1;8529-8537:2;8539-8539:3;8539-8539:3.1;8554-8559:4;8569-8569:1;8588-8595:2;8620-8620:1;8624-8624:2;8628-8628:3;8629-8629:4;8637-8637:5;8672-8677:1")]
[assembly: global::go.GoPositionMap("reflect/benchmark_test.go", "benchmark_test.cs", "ABgcABIyooK4ooK4ooK4ooK4ooK4ooK4ooK4ooK4ooK4ooK4grKSgoLcoqamggAsCIIAACyChIKCkpKCABoMggAAIoKCkpKSgqaigqaSgtyCgoKCgsqCkoKC3KKigpKCgu6CpqKChIKCAAgIgoYAEBSykoKCgqKCuIIACwikhIKCpoKUqJKCAAoWgoKSgsqCgpKCyoKCkoIACRSikpKCpqaCkpKCyoKCkoIAEQqChoKCgoKCgoKCgoSCgoSCgpYABBKykpKCgoK4koKCggAHEKKCgoKU", "101-106:1;151-155:1;181-185:1;186-190:2;191-195:3;210-215:1;220-220:1;222-227:2;247-249:1;254-254:2;255-255:3;256-256:4;257-257:5;258-258:6;261-270:7;265-269:7.1;292-296:1;307-311:1;316-320:1;325-329:1;339-343:1;349-353:1;358-362:1;399-416:1;400-407:1.1;408-415:1.2")]
[assembly: global::go.GoPositionMap("reflect/iter_test.go", "iter_test.cs", "ABYigtyCgpSC/IKCgpSUgriCgoKUlIK4goKClJSCuIKCgpSUgriCgoKUlIK4goKClJSCuIKCgpSUgriCgoKClJSCuIKCgIKklIK4gsqCgIKkgpSCuIKCuIKCgpSUgriCgoKUlIK4goKClIKCpoK4goIACQiC3PyCgoKUgoKmgriCgoKUgoKmgriCgoKUgoKmgtiCkoKCgoKUgpSClJSCtKSCgoKClIKUlIK4gqaCgoKUgoKmgriCgoKUgoKmgriCgoKUgpSCgqaCuIKCgpSCgpSCpoK4goIACBCCgoIACRKCgoI=", "34-45:1;46-57:2;58-69:3;70-81:4;82-93:5;94-105:6;106-117:7;118-130:8;131-142:9;143-160:10;161-167:11;167-178:12;179-190:13;191-205:14;225-239:1;240-254:2;255-269:3;270-290:4;291-306:5;307-311:6;311-325:7;326-340:8;341-358:9;359-376:10")]
[assembly: global::go.GoPositionMap("reflect/map_swiss_test.go", "map_swiss_test.cs", "AAwcrMKCuoI=")]
[assembly: global::go.GoPositionMap("reflect/set_test.go", "set_test.cs", "ABUktJSCgoKCgpSAgraUgoKCgoKUgIK2lIKCgoKClICCtpSCgoKCgpSAgraUgoKCgoKCgpSAgraUgoKCgoKCgpSAgraEhIKCgoKCgpSAgtyGooKCgoK4goKCgoCCyJSCgoKCuJSSgoKCggAJCAAOJICigKIACBCigoKCgILqAA4kooKCgoCC")]
[assembly: global::go.GoPositionMap("reflect/tostring_test.go", "tostring_test.cs", "ABQkooKClIKUpKSkgqSkgpS2goKClJSCpIKCgoKClJSCpIKCgoKCpIKkgoKCgoKClJSCpKSCpA==")]
[assembly: global::go.GoPositionMap("reflect/type_test.go", "type_test.cs", "ABQYggAHIIKCggALCoIABhiCmIKSAC0IogAQSLKSgIIACBCigriiggAJCIIADB6ykoCCAAwMggAMHrKSgII=", "56-58:1;99-103:1;127-127:1;128-128:2;138-142:3;152-152:1;153-153:2;163-167:3")]
[assembly: global::go.GoPositionMap("reflect/visiblefields_test.go", "visiblefields_test.cs", "AEMkAIECpgSCgpKSgoKAkqaCgoLKgpSCgrqCgpSCABIQko6CgpSC", "296-329:1")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("reflect_test")]
public static partial class reflect_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial class BenchmarkMap_V {}
    internal partial class TestArrayOfGC_T {}
    internal partial class TestChanOfGC_T {}
    internal partial class TestExported_BigP {}
    internal partial class TestExported_p {}
    internal partial class TestMapOfGCKeys_T {}
    internal partial class TestMapOfGCValues_T {}
    internal partial class TestPtrToGC_T {}
    internal partial class TestSliceOfGC_T {}
    internal partial class TestStructOfGC_T {}
    internal partial interface BenchmarkSetZero_type_Interface {}
    [GoLocalName("T1")] internal partial interface TestCallPanic_T1 {}
    [GoLocalName("t0")] internal partial interface TestCallPanic_t0 {}
    [GoLocalName("I")] internal partial interface TestMethodPkgPath_I {}
    internal partial interface TestMethodPkgPath_i {}
    internal partial interface TestMethod_x {}
    [GoLocalName("Named")] internal partial interface TestStructOfEmbeddedIfaceMethodCall_Named {}
    [GoLocalName("Iface")] internal partial interface TestStructOfWithInterface_Iface {}
    [GoLocalName("IfaceSet")] internal partial interface TestStructOfWithInterface_IfaceSet {}
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
    internal partial struct BenchmarkMap_S {}
    internal partial struct BenchmarkMap_tests {}
    [GoLocalName("T")] internal partial struct BenchmarkPtrTo_T {}
    [GoValueClone("Array")] internal partial struct BenchmarkSetZero_type {}
    internal partial struct TestAddr_p {}
    internal partial struct TestAddr_s {}
    [GoLocalName("T1")] internal partial struct TestAlignment_T1 {}
    [GoLocalName("T1inner")] internal partial struct TestAlignment_T1inner {}
    [GoLocalName("T2")] internal partial struct TestAlignment_T2 {}
    [GoLocalName("T2inner")] internal partial struct TestAlignment_T2inner {}
    internal partial struct TestAll_i {}
    internal partial struct TestAllocations_type {}
    internal partial struct TestAppend_i {}
    internal partial struct TestArrayOfDirectIface_T {}
    internal partial struct TestArrayOfDirectIface_Tᴛ1 {}
    internal partial struct TestArrayOf_T {}
    internal partial struct TestArrayOf_Tfloat {}
    internal partial struct TestArrayOf_Tint {}
    internal partial struct TestArrayOf_Tintᴛ1 {}
    internal partial struct TestArrayOf_Tintᴛ2 {}
    internal partial struct TestArrayOf_Tintᴛ3 {}
    internal partial struct TestArrayOf_Tstring {}
    [GoLocalName("Tstruct")] internal partial struct TestArrayOf_Tstruct {}
    [GoLocalName("TstructUV")] internal partial struct TestArrayOf_TstructUV {}
    [GoLocalName("TstructUV")] internal partial struct TestArrayOf_TstructUVᴛ1 {}
    [GoLocalName("Tstruct")] [GoValueClone("V")] internal partial struct TestArrayOf_Tstructᴛ1 {}
    [GoLocalName("Tstruct")] internal partial struct TestArrayOf_Tstructᴛ2 {}
    internal partial struct TestArrayOf_tests {}
    internal partial struct TestBigUnnamedStruct_b {}
    internal partial struct TestBytes_A {}
    internal partial struct TestBytes_AB {}
    internal partial struct TestBytes_B {}
    internal partial struct TestBytes_S {}
    internal partial struct TestBytes_SB {}
    [GoLocalName("T")] internal partial struct TestCallArgLive_T {}
    [GoLocalName("T")] internal partial struct TestCallPanic_T {}
    [GoLocalName("T2")] internal partial struct TestCallPanic_T2 {}
    internal partial struct TestCanIntUintFloatComplex_complex {}
    internal partial struct TestCanIntUintFloatComplex_float {}
    internal partial struct TestCanIntUintFloatComplex_integer {}
    [GoValueClone("want")] internal partial struct TestCanIntUintFloatComplex_type {}
    internal partial struct TestCanIntUintFloatComplex_typeᴛ1 {}
    internal partial struct TestCanIntUintFloatComplex_uinteger {}
    [GoLocalName("Embed")] internal partial struct TestCanSetField_Embed {}
    [GoLocalName("S1")] internal partial struct TestCanSetField_S1 {}
    [GoLocalName("S2")] internal partial struct TestCanSetField_S2 {}
    [GoLocalName("S3")] internal partial struct TestCanSetField_S3 {}
    [GoLocalName("S4")] internal partial struct TestCanSetField_S4 {}
    [GoLocalName("embed")] internal partial struct TestCanSetField_embed {}
    [GoLocalName("testCase")] internal partial struct TestCanSetField_testCase {}
    internal partial struct TestCanSetField_tests {}
    internal partial struct TestChanOfDir_T {}
    internal partial struct TestChanOfDir_T1 {}
    internal partial struct TestChanOf_T {}
    internal partial struct TestChanOf_T1 {}
    internal partial struct TestClear_tests {}
    internal partial struct TestConvertNaNs_myFloat32 {}
    internal partial struct TestExported_P {}
    internal partial struct TestExported_P2 {}
    [GoLocalName("exportTest")] internal partial struct TestExported_exportTest {}
    internal partial struct TestExported_p3 {}
    [GoLocalName("ΦExported")] internal partial struct TestExported_ΦExported {}
    [GoLocalName("φUnexported")] internal partial struct TestExported_φUnexported {}
    [GoLocalName("A")] internal partial struct TestFieldByIndexErr_A {}
    [GoLocalName("B")] internal partial struct TestFieldByIndexErr_B {}
    [GoLocalName("P")] internal partial struct TestFieldByIndexNil_P {}
    [GoLocalName("T")] internal partial struct TestFieldByIndexNil_T {}
    internal partial struct TestFieldPkgPath_i {}
    internal partial struct TestFieldPkgPath_localOtherPkgFields {}
    [GoLocalName("pkgpathTest")] internal partial struct TestFieldPkgPath_pkgpathTest {}
    internal partial struct TestFieldPkgPath_x {}
    [GoLocalName("S")] internal partial struct TestFuncLayout_S {}
    [GoLocalName("test")] internal partial struct TestFuncLayout_test {}
    internal partial struct TestFuncOf_K {}
    internal partial struct TestFuncOf_T1 {}
    internal partial struct TestFuncOf_V {}
    internal partial struct TestFuncOf_testCases {}
    internal partial struct TestImplicitMapConversion_MyBuffer {}
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
    internal partial struct TestIssue22031_sᴛ1 {}
    [GoLocalName("t1")] internal partial struct TestIssue22031_t1 {}
    [GoLocalName("t2")] internal partial struct TestIssue22031_t2 {}
    [GoLocalName("T")] internal partial struct TestMakeFuncInvalidReturnAssignments_T {}
    [GoLocalName("U")] internal partial struct TestMakeFuncInvalidReturnAssignments_U {}
    [GoLocalName("T")] internal partial struct TestMakeFuncValidReturnAssignments_T {}
    internal partial struct TestMakeFuncValidReturnAssignments_i {}
    [GoLocalName("KV")] internal partial struct TestMapOfGCBigKey_KV {}
    internal partial struct TestMapOf_K {}
    internal partial struct TestMapOf_V {}
    internal partial struct TestMap_S {}
    internal partial struct TestMethodPkgPath_tests {}
    internal partial struct TestMethodValue_type {}
    internal partial struct TestPtrToMethods_y {}
    internal partial struct TestSetBytes_B {}
    internal partial struct TestSetIter_i {}
    [GoLocalName("T")] internal partial struct TestSetPanic_T {}
    [GoLocalName("T2")] internal partial struct TestSetPanic_T2 {}
    [GoLocalName("t0")] internal partial struct TestSetPanic_t0 {}
    [GoLocalName("t1")] internal partial struct TestSetPanic_t1 {}
    internal partial struct TestSliceOf_T {}
    internal partial struct TestSliceOf_T1 {}
    internal partial struct TestSmallZero_T {}
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
    internal partial struct TestSwapper_I {}
    internal partial struct TestSwapper_S {}
    [GoLocalName("pair")] internal partial struct TestSwapper_pair {}
    [GoLocalName("pairPtr")] internal partial struct TestSwapper_pairPtr {}
    internal partial struct TestSwapper_tests {}
    internal partial struct TestTypeFieldOutOfRangePanic_i {}
    internal partial struct TestTypeFieldOutOfRangePanic_testIndices {}
    internal partial struct TestTypeFor_mystring {}
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
    internal partial struct TestZeroSet_T {}
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
    internal partial struct init_Int64 {}
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
