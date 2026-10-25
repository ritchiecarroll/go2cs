// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.

// <ImportedTypeAliases>
global using abiꓸArrayType = go.@internal.abi_package.ΔArrayType;
global using abiꓸChanDir = go.@internal.abi_package.ΔChanDir;
global using abiꓸFuncType = go.@internal.abi_package.ΔFuncType;
global using abiꓸInterfaceType = go.@internal.abi_package.ΔInterfaceType;
global using abiꓸKind = go.@internal.abi_package.ΔKind;
global using abiꓸMapType = go.@internal.abi_package.ΔMapType;
global using abiꓸName = go.@internal.abi_package.ΔName;
global using abiꓸStructType = go.@internal.abi_package.ΔStructType;
global using bigmodꓸNat = go.crypto.@internal.fips140.bigmod_package.ΔNat;
global using bigꓸInt = go.math.big_package.ΔInt;
global using bigꓸRat = go.math.big_package.ΔRat;
global using cryptoꓸDecrypterOpts = object;
global using cryptoꓸPrivateKey = object;
global using cryptoꓸPublicKey = object;
global using ecdhꓸPublicKey = go.crypto.@internal.fips140.ecdh_package.ΔPublicKey;
global using ecdsaꓸPublicKey = go.crypto.@internal.fips140.ecdsa_package.ΔPublicKey;
global using ed25519ꓸPublicKey = go.crypto.@internal.fips140.ed25519_package.ΔPublicKey;
global using flagꓸErrorHandling = go.flag_package.ΔErrorHandling;
global using mldsaꓸPublicKey = go.crypto.@internal.fips140.mldsa_package.ΔPublicKey;
global using osꓸDirEntry = go.io.fs_package.DirEntry;
global using osꓸFileInfo = go.io.fs_package.FileInfo;
global using osꓸFileMode = go.io.fs_package.FileMode;
global using osꓸPathError = go.io.fs_package.PathError;
global using osꓸSignal = go.os_package.ΔSignal;
global using rsaꓸPublicKey = go.crypto.@internal.fips140.rsa_package.ΔPublicKey;
global using runtimeꓸError = go.runtime_package.ΔError;
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
global using tls13ꓸExporterMasterSecret = go.crypto.@internal.fips140.tls13_package.ΔExporterMasterSecret;
global using tls13ꓸHandshakeSecret = go.crypto.@internal.fips140.tls13_package.ΔHandshakeSecret;
global using tls13ꓸMasterSecret = go.crypto.@internal.fips140.tls13_package.ΔMasterSecret;
// </ImportedTypeAliases>

using go;
using static global::go.crypto.@internal.fips140test.fipstest_internal_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b78205b5d627974653b2079205b5d627974653b20616e794f7665726c617020626f6f6c3b20696e65786163744f7665726c617020626f6f6c7d", "aliasingTestsᴛ1")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<bufio_package.Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<bytes_package.Buffer, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<go.crypto.@internal.fips140.nistec_package.P224Point, nistPoint<go.crypto.@internal.fips140.nistec_package.P224Point>>(ConstraintProxy = true)]
[assembly: GoImplement<go.crypto.@internal.fips140.nistec_package.P256Point, nistPoint<go.crypto.@internal.fips140.nistec_package.P256Point>>(ConstraintProxy = true)]
[assembly: GoImplement<go.crypto.@internal.fips140.nistec_package.P384Point, nistPoint<go.crypto.@internal.fips140.nistec_package.P384Point>>(ConstraintProxy = true)]
[assembly: GoImplement<go.crypto.@internal.fips140.nistec_package.P521Point, nistPoint<go.crypto.@internal.fips140.nistec_package.P521Point>>(ConstraintProxy = true)]
[assembly: GoImplement<go.crypto.@internal.fips140.sha256_package.Digest, hash_package.Hash>(Pointer = true)]
[assembly: GoImplement<go.crypto.@internal.fips140.sha3_package.Digest, hash_package.Hash>(Pointer = true)]
[assembly: GoImplement<go.crypto.@internal.fips140.sha512_package.Digest, hash_package.Hash>(Pointer = true)]
[assembly: GoImplement<os_package.File, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<testing_package.T, testing_package.TB>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
// </ImplicitConversions>

// This package's string literals are hoisted to `static readonly` fields (the standard
// library's form, or -hoist-literals), so no converted file passes a bare literal for
// go2cs-gen's literal interceptor to box. The stamp below tells the generator to skip
// the assembly.
[assembly: go.GoHoistedLiterals]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("crypto/internal/fips140test/acvp_fips140v1.26_test.go", "acvp_fips140v1.26_test.cs", "ABIogoKCgoKCgoKCpoKmlIKCuMqCpoKCgoKUgoKmgoKUgoKSgpKCkoKUyoKmyoKmgoKCgpSCgqaCgoKSgpTKgqY=", "35-45:1;52-88:1;95-124:1")]
[assembly: go.GoPositionMap("crypto/internal/fips140test/acvp_test.go", "acvp_test.cs", "AEmAAcrEgoKCpoKU6IKCgpSAgoIAJlYAygGSA7iCgoKSgqaCpoKCpoCCtoKCpoCCyNaO4oCCpIKmgoKm3IKClIKAgsiCgoCCpKamuIKAgraCgILIgoCCyKzWgoKmAAUc8qaCgoKUAAQmAA4CpoKUgIK2goKCgpSCgoKUgqbOoqaCzqKmgpSCgoKClIKClIKmAAQkAA0CpoKClIKCgqbKgqaUgpSCgpTKgqaCgpSCgqaCgqamgoKmgoKCgriCpoKUyoKmgoKClIKUgpTKgqaCgoKCgpSCpoKCypSCgqaCgoKCpsqmgoKmgoKCuIKUyoKmgoKCgsqCpoKCgpTKgqaClMqCpoKCgpTKgriCgqaCgoKUgoKmyoKmgqaCggALGMqCpoLcgIK2yoKmgqaCgpSCgpSCgoKCuIKCpqbKgqaCpoKCgpSClIKCgqaUpoKmyoKmgoKmgpSkpKSkpoKmgpQACBKCpoKCpoKUgqbO0pSClIKmgoKCgoKmsoKCpoKUpKSUgqamgqaCgqaUgoKmgoKClIKCgoKmgpSkpKSklIKmyoKmgoKmgoKmgoKClIKCgqaUlKSkpKSmgqbKooKCpqaClJSkpKSkpKSkpKSkpqaCppSCgrjKgqaClIKCpoKmlMqCpoKUgoKmgoKmyoKmlIKCuMqCpoKUgoKmgqaUyoKmgpSCgqaCgqbKgpSUpKSkpKamgqaCpoKCgpSCgqaCuIKmgoKUgpSCgpS4goKUgqaCpoKUuMqCpoKmgoKCgoKmgqaCgqaCgpTKgqaCgoKClIKCpoKCpoKClIKClKbKgqiCgoKClIKCpoKCppSCpoKCpsqCuIKClIKCpoKUgqbKgqaCgpSCgqaClIKmyoKmgoKCgpTKgraCgoKUgpSkpKSmgsqipoKClIKCgqaCgpSUgqaCgoKUgoKmyoKmgoKCgoKmggAJFIKCgpTKggAMHoIAGTiUgsqCuIKCuIKClIKCpoKSlICCpICCAAwYgoKClIKUqqKAgqSmgqaCgoKClIKUgpSC3IKUgqaCgpSUgoKUlIKClMqC3IKCgoKUgpSUgoKmgu6CpoKmgoKClJTKgqaUgoKmlIKUyoKmgpSCgqaCgpSCgoKCpoKCuIKClMqCpoKCgpSCgqSCgqbKgoKUgpSUgqbcmKKAgraCgqaCpoKmgoKClIKUgqaCgpSClMqCpoKClIKCpoKCkoKmyoKAgraCgqbKgqaClIKClJSCgoKCpoKCpoKCpgAXCoKUAAkYgILIlKaCuIKAgsiUgoKUgtwACBKCgqiCgoL4gqaAgqS4goKClIKC6IKmgIKkuIKUgpSCguiCgoKCpoKCpoLogoLKgoKCpoKCpoK4gpSUgoCCtqaCgoCCtoKCpg==", "456-458:1;472-479:1;500-524:1;533-536:1;545-564:1;584-596:1;603-614:1;621-660:1;667-680:1;687-744:1;751-757:1;764-771:1;778-783:1;790-797:1;805-822:1;829-849:1;856-869:1;876-906:1;913-942:1;949-981:1;988-1002:1;1051-1094:1;1101-1143:1;1161-1161:1;1163-1163:2;1165-1165:3;1167-1167:4;1169-1169:5;1171-1171:6;1173-1173:7;1175-1175:8;1177-1177:9;1179-1179:10;1190-1200:1;1207-1223:1;1230-1245:1;1252-1262:1;1269-1285:1;1292-1307:1;1333-1392:1;1399-1426:1;1433-1462:1;1469-1499:1;1506-1525:1;1532-1550:1;1557-1565:1;1572-1592:1;1599-1631:1;1638-1664:1;1671-1680:1;1687-1698:1;1784-1828:1;1835-1879:1;1886-1900:1;1907-1938:1;1945-1979:1;2005-2026:1;2033-2066:1;2073-2101:1;2179-2184:1;2201-2206:1", "", "147=cmdGetConfig/1/1/1,New224/1/4/3,cmdHashAft/1/10/3,New224/2/4/4,cmdHashMct/1/6/4,New/1/4/5,cmdHashAft/2/10/5,New/2/4/6,cmdHashMct/2/6/6,New384/1/4/7,cmdHashAft/3/10/7,New384/2/4/8,cmdHashMct/3/6/8,New/3/4/9,cmdHashAft/4/10/9,New/4/4/10,cmdHashMct/4/6/10,New512_224/1/2/11,cmdHashAft/5/10/11,New512_224/2/2/12,cmdHashMct/5/6/12,New512_256/1/2/13,cmdHashAft/6/10/13,New512_256/2/2/14,cmdHashMct/6/6/14,New256/1/2/16,cmdHashAft/7/10/16,New256/2/2/17,cmdSha3Mct/1/4/17,New224/3/4/18,cmdHashAft/8/10/18,New224/4/4/19,cmdSha3Mct/2/4/19,New384/3/4/20,cmdHashAft/9/10/20,New384/4/4/21,cmdSha3Mct/3/4/21,New512/1/2/22,cmdHashAft/10/10/22,New512/2/2/23,cmdSha3Mct/4/4/23,cmdEntropyHashEntropySha384Aft/1/1/30,cmdEntropyHashEntropySha384Mct/1/1/31,NewShake128/1/3/37,cmdShakeAftVot/1/4/37,NewShake128/2/3/38,cmdShakeAftVot/2/4/38,NewShake128/3/3/39,cmdShakeMct/1/2/39,NewShake256/1/3/40,cmdShakeAftVot/3/4/40,NewShake256/2/3/41,cmdShakeAftVot/4/4/41,NewShake256/3/3/42,cmdShakeMct/2/2/42,cmdCShakeAft/1/2/44,cmdCShakeMct/1/2/45,cmdCShakeAft/2/2/46,cmdCShakeMct/2/2/47,cmdHmacAft/1/10/49,cmdHmacAft/2/10/50,cmdHmacAft/3/10/51,cmdHmacAft/4/10/52,cmdHmacAft/5/10/53,cmdHmacAft/6/10/54,cmdHmacAft/7/10/55,cmdHmacAft/8/10/56,cmdHmacAft/9/10/57,cmdHmacAft/10/10/58,cmdHkdfAft/1/10/60,cmdHkdfAft/2/10/61,cmdHkdfAft/3/10/62,cmdHkdfAft/4/10/63,cmdHkdfAft/5/10/64,cmdHkdfAft/6/10/65,cmdHkdfAft/7/10/66,cmdHkdfAft/8/10/67,cmdHkdfAft/9/10/68,cmdHkdfAft/10/10/69,cmdHkdfExtractAft/1/2/71,cmdHkdfExtractAft/2/2/72,cmdHkdfExpandLabelAft/1/2/73,cmdHkdfExpandLabelAft/2/2/74,cmdPbkdf/1/1/76,cmdMlKem768KeyGenAft/1/1/78,cmdMlKem768EncapAft/1/1/79,cmdMlKem768DecapAft/1/1/80,cmdMlKem1024KeyGenAft/1/1/81,cmdMlKem1024EncapAft/1/1/82,cmdMlKem1024DecapAft/1/1/83,cmdHmacDrbgAft/1/10/85,cmdHmacDrbgAft/2/10/86,cmdHmacDrbgAft/3/10/87,cmdHmacDrbgAft/4/10/88,cmdHmacDrbgAft/5/10/89,cmdHmacDrbgAft/6/10/90,cmdHmacDrbgAft/7/10/91,cmdHmacDrbgAft/8/10/92,cmdHmacDrbgAft/9/10/93,cmdHmacDrbgAft/10/10/94,cmdEddsaKeyGenAft/1/1/96,cmdEddsaKeyVerAft/1/1/97,cmdEddsaSigGenAftBft/1/1/98,cmdEddsaSigVerAft/1/1/99,cmdEcdsaKeyGenAft/1/1/101,cmdEcdsaKeyVerAft/1/1/102,cmdEcdsaSigGenAft/1/2/103,cmdEcdsaSigVerAft/1/1/104,cmdEcdsaSigGenAft/2/2/105,cmdAesCbc/1/2/107,cmdAesCbc/2/2/108,cmdAesCtr/1/2/109,cmdAesCtr/2/2/110,cmdAesGcmSeal/1/2/111,cmdAesGcmOpen/1/2/112,cmdAesGcmSeal/2/2/113,cmdAesGcmOpen/2/2/114,cmdCmacAesAft/1/1/116,cmdCmacAesVerifyAft/1/1/117,cmdTlsKdf12Aft/1/3/121,cmdTlsKdf12Aft/2/3/122,cmdTlsKdf12Aft/3/3/123,cmdSshKdfAft/1/8/127,cmdSshKdfAft/2/8/128,cmdSshKdfAft/3/8/129,cmdSshKdfAft/4/8/130,cmdSshKdfAft/5/8/131,cmdSshKdfAft/6/8/132,cmdSshKdfAft/7/8/133,cmdSshKdfAft/8/8/134,P224/1/1/136,cmdEcdhAftVal/1/4/136,P256/1/1/137,cmdEcdhAftVal/2/4/137,P384/1/1/138,cmdEcdhAftVal/3/4/138,P521/1/1/139,cmdEcdhAftVal/4/4/139,cmdCtrDrbgAft/1/1/141,cmdCtrDrbgReseedAft/1/1/142,cmdRsaKeyGenAft/1/1/144,cmdRsaSigGenAft/1/8/146,cmdRsaSigGenAft/2/8/147,cmdRsaSigGenAft/3/8/148,cmdRsaSigGenAft/4/8/149,cmdRsaSigGenAft/5/8/150,cmdRsaSigGenAft/6/8/151,cmdRsaSigGenAft/7/8/152,cmdRsaSigGenAft/8/8/153,cmdRsaSigVerAft/1/8/155,cmdRsaSigVerAft/2/8/156,cmdRsaSigVerAft/3/8/157,cmdRsaSigVerAft/4/8/158,cmdRsaSigVerAft/5/8/159,cmdRsaSigVerAft/6/8/160,cmdRsaSigVerAft/7/8/161,cmdRsaSigVerAft/8/8/162,cmdKdfCounterAft/1/1/164,cmdKdfFeedbackAft/1/1/165,cmdOneStepNoCounterHmacAft/1/10/167,cmdOneStepNoCounterHmacAft/2/10/168,cmdOneStepNoCounterHmacAft/3/10/169,cmdOneStepNoCounterHmacAft/4/10/170,cmdOneStepNoCounterHmacAft/5/10/171,cmdOneStepNoCounterHmacAft/6/10/172,cmdOneStepNoCounterHmacAft/7/10/173,cmdOneStepNoCounterHmacAft/8/10/174,cmdOneStepNoCounterHmacAft/9/10/175,cmdOneStepNoCounterHmacAft/10/10/176,cmdKtsIfcInitiatorAft/1/10/178,cmdKtsIfcResponderAft/1/10/179,cmdKtsIfcInitiatorAft/2/10/180,cmdKtsIfcResponderAft/2/10/181,cmdKtsIfcInitiatorAft/3/10/182,cmdKtsIfcResponderAft/3/10/183,cmdKtsIfcInitiatorAft/4/10/184,cmdKtsIfcResponderAft/4/10/185,cmdKtsIfcInitiatorAft/5/10/186,cmdKtsIfcResponderAft/5/10/187,cmdKtsIfcInitiatorAft/6/10/188,cmdKtsIfcResponderAft/6/10/189,cmdKtsIfcInitiatorAft/7/10/190,cmdKtsIfcResponderAft/7/10/191,cmdKtsIfcInitiatorAft/8/10/192,cmdKtsIfcResponderAft/8/10/193,cmdKtsIfcInitiatorAft/9/10/194,cmdKtsIfcResponderAft/9/10/195,cmdKtsIfcInitiatorAft/10/10/196,cmdKtsIfcResponderAft/10/10/197;1018=Bytes/1/1/1;1423=BlockSize/1/1/1;2257=Executable/1/1/1;2263=Join/1/1/2,Executable/1/1/6")]
[assembly: go.GoPositionMap("crypto/internal/fips140test/alias_test.go", "alias_test.cs", "ABIcAA8mgoKClIKCuIKCgg==")]
[assembly: go.GoPositionMap("crypto/internal/fips140test/cast_test.go", "cast_test.cs", "AC1OABs4goIACQyCuIKCgpSClpKCgJKClIKUgoKUgpSUtoKC6pKUgoKmpoKCpqaCgqamgoKmpoKCppTmgoKUgoKCgoKmgpKClAAIDIKClIKmgpSCgoKCgoKClIKSgpQ=", "69-71:1;91-106:1;175-181:1;190-212:1")]
[assembly: go.GoPositionMap("crypto/internal/fips140test/check_test.go", "check_test.cs", "ACIwgoKCpoKmlIKCgoKU5oKClIKCuJSCgIK2lIKCgriCgoKCgoKUgpSCABIIgqaClIKUgpSClIKUgpSAgsiCgoKmgoCCpIKCgIKkppKCgoLcgoKCpoKCuIKClII=", "114-119:1;132-139:2")]
[assembly: go.GoPositionMap("crypto/internal/fips140test/cmac_test.go", "cmac_test.cs", "ABUclIIADiKCgpSCgoKCgoI=")]
[assembly: go.GoPositionMap("crypto/internal/fips140test/ctrdrbg_test.go", "ctrdrbg_test.cs", "AA0cpoKCgoKCgpiigpSUgpSCgg==")]
[assembly: go.GoPositionMap("crypto/internal/fips140test/edwards25519_test.go", "edwards25519_test.cs", "ABAegoKAgoKCgoKU", "17-23:1")]
[assembly: go.GoPositionMap("crypto/internal/fips140test/entropy_test.go", "entropy_test.cs", "ACg8kgAMBKKClIKUlIKClIKAgqSmgoKCgpSUgoKClIKAgraAgqSmgoLKgJaCgoCCpJaCyIKClIKogpSUgoKCqIKUABgssoKCgIKCpJSmgoKClIKCgqaCgoKCgoL6goKAgraCgoKCgIK2goCC+IKCgoKAgsiClIKAgqSAgviClIKCgKKClIKUgoKUgoKClP6CgILIlIKCuIKCgu6CgoKClA==", "226-241:1;258-260:1;264-267:2", "", "69=Format/1/1/1;86=Format/1/1/1;116=Run/1/1/2;125=CombinedOutput/1/1/2;138=Sprintf/1/1/1,CombinedOutput/1/1/3;149=Sprintf/1/1/1,CombinedOutput/1/1/3")]
[assembly: go.GoPositionMap("crypto/internal/fips140test/fips140v1.26_test.go", "fips140v1.26_test.cs", "AAwclJTWgpKClIKClII=", "22-32:1")]
[assembly: go.GoPositionMap("crypto/internal/fips140test/fips_test.go", "fips_test.cs", "AD5YgoKUpoKUgpSmgpTogoKClIKClKaCgIKkpoCCACwIgoKCpoKCgoKClJSygoKCgoKCgoKCgriygoKCgoKCgoKCgriigoKCgoKCgoKCgoK4koKCgoKmgoKCgoKCgqaSgoKUgoKCgoKmgoKCgoK4koKClIKUgoKCgriSgoKCgoKmkoKCgoKmgoKClIKUgoKCgriCgoKCgoKmkoKCgpSCppKCgoKUgqaCgoKCppKCgoKUgoKCgriSgoKCgqaSgoKCgqaSgoKCgqaSgoKCgqaSgoKCgqaSgoKCgqaSgoKCgqaSgoKCgqaSgoKCgqaSgoKCgqaSgoKCgoKmkoKCgoKCppKCgoKCgqaSgoKCgoKmkoKCppKCgqaSgoKmkoKCgqaSgoKmkoKCgoKCgriCgoKClMqCgoKClMqCgoK4goKCgoKU", "106-119:1;121-134:2;136-150:3;152-158:4;160-168:5;170-190:6;192-206:7;208-215:8;217-223:9;225-239:10;242-248:11;250-258:12;260-268:13;270-275:14;277-289:15;291-297:16;299-305:17;307-313:18;315-321:19;323-329:20;331-337:21;339-345:22;347-353:23;355-361:24;363-369:25;371-378:26;380-387:27;389-396:28;398-405:29;407-411:30;413-417:31;419-423:32;425-430:33;432-436:34;438-446:35;451-457:1;462-468:1")]
[assembly: go.GoPositionMap("crypto/internal/fips140test/indicator_test.go", "indicator_test.cs", "ABcYgoKCpoKCgqaCgoKCpoKCgqaCgoKCpoKCgoKmgoKSgoKClIKCpoKCgoKCgpSCgg==", "54-58:1;67-71:2")]
[assembly: go.GoPositionMap("crypto/internal/fips140test/mldsa_test.go", "mldsa_test.cs", "ABYoAAwWAAkCgqaCpoKmgoKCpoKCpoKCuIKCgqaCgqaCgtyCgoKClIKCgoKUgoKCgpSCgoKUgpSAgsiCggAICIKClIKUgriCgoKClIKClIK4isKUlIKAgoKCgoKUgpSCgoKUgoKCgpSAgrbI3IKUgpSCuIKCgoKUkpSCgoKAgqS+soKUgpSC6IKCgoKClIKCgoKUgILagoKCpoKCpoKCygC+Af4CAJUBrAI=", "32-35:1;36-39:2;40-43:3;45-49:4;50-54:5;55-59:6;62-66:7;67-71:8;72-76:9;118-120:1;121-123:2;124-126:3;155-179:1;190-192:1;193-195:2;196-198:3;207-209:1;224-226:1;227-229:2;230-232:3;255-259:1;260-264:2;265-269:3")]
[assembly: go.GoPositionMap("crypto/internal/fips140test/nistec_test.go", "nistec_test.cs", "ABokgoKCgIKCgoKCgoKAgqSCgIK2toKAgoKCgoKCgoCCpIKAgra2goCCgoKCgoKCgIKkgoCCtraCgIKCgoKCgoKAgqSCgIK2AA8egoKUgpSClIIACgiilIKCgoKUgoKCgoKCgoKClIKUgpSClIKUgriCgpSClIKUggAKCKKCkoKCgoKCpoKCgpSCpoKUgriCgoKCgoKCuIKCkJKSlJKUkJKSlJKCgpSClJKSgqaSksqSkg==", "20-38:1;21-35:1.1;39-57:2;40-54:2.1;58-76:3;59-73:3.1;77-95:4;78-92:4.1;109-111:1;112-114:2;115-117:3;118-120:4;161-163:1;164-166:2;167-169:3;170-172:4;177-212:1;216-216:2;217-219:3;220-222:4;223-223:5;224-226:6;227-231:7;236-239:8;242-244:9;249-251:10")]
[assembly: go.GoPositionMap("crypto/internal/fips140test/sshkdf_test.go", "sshkdf_test.cs", "AA8clIKCpoKCgoKClJSmgpSClIKUgpSClII=")]
[assembly: go.GoPositionMap("crypto/internal/fips140test/xaes_test.go", "xaes_test.cs", "ABgmgoKUgoCCgoKCgoKCgIK2yIKCgpSClIKCgoKClIKClIIAAxbigoKCgoKmgoKCgoKCpoKCgoKCgoCCpICCkoK2goKCgoCCpICCkoLIgoKUgoKCgoKCgoKCgoKClIKCgpSCppSAgg==", "24-34:1")]
// </GoSourcePositionMaps>

namespace go.crypto.@internal.fips140test;

[GoPackage("fipstest", ImportPath = "crypto/internal/fips140test")]
public static partial class fipstest_internal_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbufio() => builtin.initPackage(typeof(bufio_package));
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸcrypto() => builtin.initPackage(typeof(crypto_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸelliptic() => builtin.initPackage(typeof(go.crypto.elliptic_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸboring() => builtin.initPackage(typeof(go.crypto.@internal.boring_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸcryptotest() => builtin.initPackage(typeof(go.crypto.@internal.cryptotest_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸentropyꓸv1_0_0() => builtin.initPackage(typeof(go.crypto.@internal.entropy.v1_0_0.entropy_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140() => builtin.initPackage(typeof(go.crypto.@internal.fips140_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸaes() => builtin.initPackage(typeof(go.crypto.@internal.fips140.aes_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸaesꓸgcm() => builtin.initPackage(typeof(go.crypto.@internal.fips140.aes.gcm_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸbigmod() => builtin.initPackage(typeof(go.crypto.@internal.fips140.bigmod_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸcheck() => builtin.initPackage(typeof(go.crypto.@internal.fips140.check_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸcheckꓸchecktest() => builtin.initPackage(typeof(go.crypto.@internal.fips140.check.checktest_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸdrbg() => builtin.initPackage(typeof(go.crypto.@internal.fips140.drbg_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸecdh() => builtin.initPackage(typeof(go.crypto.@internal.fips140.ecdh_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸecdsa() => builtin.initPackage(typeof(go.crypto.@internal.fips140.ecdsa_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸed25519() => builtin.initPackage(typeof(go.crypto.@internal.fips140.ed25519_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸedwards25519() => builtin.initPackage(typeof(go.crypto.@internal.fips140.edwards25519_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸhkdf() => builtin.initPackage(typeof(go.crypto.@internal.fips140.hkdf_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸhmac() => builtin.initPackage(typeof(go.crypto.@internal.fips140.hmac_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸmldsa() => builtin.initPackage(typeof(go.crypto.@internal.fips140.mldsa_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸmlkem() => builtin.initPackage(typeof(go.crypto.@internal.fips140.mlkem_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸnistec() => builtin.initPackage(typeof(go.crypto.@internal.fips140.nistec_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸpbkdf2() => builtin.initPackage(typeof(go.crypto.@internal.fips140.pbkdf2_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸrsa() => builtin.initPackage(typeof(go.crypto.@internal.fips140.rsa_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸsha256() => builtin.initPackage(typeof(go.crypto.@internal.fips140.sha256_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸsha3() => builtin.initPackage(typeof(go.crypto.@internal.fips140.sha3_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸsha512() => builtin.initPackage(typeof(go.crypto.@internal.fips140.sha512_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸssh() => builtin.initPackage(typeof(go.crypto.@internal.fips140.ssh_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸsubtle() => builtin.initPackage(typeof(go.crypto.@internal.fips140.subtle_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸtls12() => builtin.initPackage(typeof(go.crypto.@internal.fips140.tls12_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸtls13() => builtin.initPackage(typeof(go.crypto.@internal.fips140.tls13_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸimpl() => builtin.initPackage(typeof(go.crypto.@internal.impl_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸrand() => builtin.initPackage(typeof(go.crypto.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸsha256() => builtin.initPackage(typeof(go.crypto.sha256_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸsha512() => builtin.initPackage(typeof(go.crypto.sha512_package));
    [GoInit] internal static void initᴛᴛimportꓸembed() => builtin.initPackage(typeof(embed_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸbinary() => builtin.initPackage(typeof(encoding.binary_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸhex() => builtin.initPackage(typeof(encoding.hex_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸflag() => builtin.initPackage(typeof(flag_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸhash() => builtin.initPackage(typeof(hash_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸabi() => builtin.initPackage(typeof(go.@internal.abi_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸgodebug() => builtin.initPackage(typeof(go.@internal.godebug_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtestenv() => builtin.initPackage(typeof(go.@internal.testenv_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸioꓸfs() => builtin.initPackage(typeof(go.io.fs_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbig() => builtin.initPackage(typeof(math.big_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸrand() => builtin.initPackage(typeof(math.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸregexp() => builtin.initPackage(typeof(regexp_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸruntimeꓸdebug() => builtin.initPackage(typeof(go.runtime.debug_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    [GoInit] internal static void initᴛᴛimportꓸunicode() => builtin.initPackage(typeof(unicode_package));
    // </ImportInitializers>
}
