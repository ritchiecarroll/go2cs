// go2cs metadata anchor for the INTERNAL (white-box bridge) test class: GoImplement /
// GoImplicitConv attributes whose GENERATED code must merge with a bridge-declared type
// anchor here — the source generators host output in the first class of the
// attribute-bearing file, and only this file's first class is the bridge. Records for
// production and external-test types stay in package_test_info.cs.

// <ImportedTypeAliases>
// </ImportedTypeAliases>

using go;
using static go.encoding.json_package;
using static go.encoding.json_internal_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b4120656e636f64696e672f6a736f6e2e4e756d62657220226a736f6e3a5c222c737472696e675c22227d", "Δtypeᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b4120656e636f64696e672f6a736f6e2e4e756d6265727d", "Δtype")]
[assembly: GoDynamicTypeLift("7374727563747b4e20656e636f64696e672f6a736f6e2e4e756d62657220226a736f6e3a5c222c737472696e675c22227d", "Δtypeᴛ3")]
[assembly: GoDynamicTypeLift("7374727563747b4e20656e636f64696e672f6a736f6e2e4e756d6265727d", "Δtypeᴛ2")]
[assembly: GoDynamicTypeLift("7374727563747b656e636f64696e672f6a736f6e2e436173654e616d653b20696e20737472696e673b2070747220616e793b206f757420616e793b20657272206572726f723b207573654e756d62657220626f6f6c3b20676f6c64656e20626f6f6c3b20646973616c6c6f77556e6b6e6f776e4669656c647320626f6f6c7d", "unmarshalTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e20737472696e673b206f757420737472696e677d", "encodeStringTestsᴛ1")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<NoPanicStruct, global::go.encoding.json_package.isZeroer>(Pointer = true)]
[assembly: GoImplement<nilJSONMarshaler, global::go.encoding.json_package.Marshaler>(Pointer = true)]
[assembly: GoImplement<nilTextMarshaler, encoding_package.TextMarshaler>(Pointer = true)]
[assembly: GoImplement<u8marshal, encoding_package.TextUnmarshaler>(Pointer = true)]
[assembly: GoImplement<unmarshalerText, encoding_package.TextUnmarshaler>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<byteWithPtrMarshalJSON, byteWithMarshalJSON>(Inverted = true, ValueType = "byte")]
[assembly: GoImplicitConv<byteWithPtrMarshalText, byteWithMarshalText>(Inverted = true, ValueType = "byte")]
[assembly: GoImplicitConv<intWithPtrMarshalJSON, intWithMarshalJSON>(Inverted = true, ValueType = "nint")]
[assembly: GoImplicitConv<intWithPtrMarshalText, intWithMarshalText>(Inverted = true, ValueType = "nint")]
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("encoding/json/bench_test.go", "bench_test.cs", "ADVcooKClJKCgpSCgpaEgIKmgIKmgoKCgoKCgqa0pKKCgoKClIKCgoCCyAALBqKCgoKCmJqEgoKCgIKkgILIpqKCgoKClIKCgILIAAgGooKCgoKYmoSCgoCCpICCyOaCpqqSgoCCABAMgqaumoSSgoCCpICC7KKUppSmopSmlNaigsqSgoCC7KKCgoKClIKCgoKClIKCgoCCyKaigoKCgoKCgoKAgqS4ooKCgoKCgoCCpIKCgoKUgoCktPqigoKCgpSCgoKAgsimooKCgoKUgoKCgILIpqKCgpKCgoCC7KKCgpKCgoCC7KKCgpKCgoCC7KKCgpKSgoCC7KKCgpKCgoCCAAoMooK6goKAguyigoKSgoKAguyigoKCqILIgoKogrqCgqKCgoKCgoKSgpSm3pSCgpSSkoIACw6ihJqChIKAggAKDKKCiJKCgILsooKC6KKCgoLoooKCgoKAgg==", "92-99:1;119-129:1;140-146:1;166-175:1;188-194:1;215-224:1;256-262:1;272-286:1;339-346:1;357-364:1;371-378:1;384-391:1;397-404:1;410-417:1;423-430:1;440-446:1;452-459:1;481-483:1;489-505:2;496-501:2.1;516-522:3;517-521:3.1;534-542:1;551-557:1", "", "224=Repeat/1/1/3;253=Repeat/1/1/3;598=TypeFor/1/1/1")]
[assembly: go.GoPositionMap("encoding/json/decode_test.go", "decode_test.cs", "AD56gsqCkoCCpIIACBLuABEmgoIAChiS1oKCgpSCpgAHEoKmlIKClIKClIKmlpKShJKShABy6AHIgqaCgpSCgpSCyoKmgsqCpoKClIKClILKgqaCyoKmgoKUgoKUgsqCpoLKgqaCgpSCgpSCyoKmggBSIAC5BZAMgoKClIKCgpaCgpSCgoIACQiCAAcYspKCgtyCgoKClIKC6IIAHj6CgpSCgriCggAIFKSWgpSClKaispKCgoCCgraCloKClJaE7paCgpSClICCkqakgIKCgriCgoKUgpSCgoKUgIKkggAIEoKCgoCCpIKClIKCggAMCpIABBiykoKAgqSAgpKCpICCkoLsgoKClIKClIKAgqSCggAHEIKCooCCpIK4goKSgIKkggAJCIKCgoKClIIADQykAAccspKCgoKCggBCngEAJ04AdeYBAFqyAQAJBIIABRqChIKAgqSCAAsMooqCgoCktAAMDKKMgoKCgoCktLToogANBoKCACpmspKSgoCCgIKkpIIAJlb+ACFAAB08hIKClIaWgpSClIKUgpSClIKUgpSClIKUgpaClIKUggAJDIIACAqCxoKEgoKUgoKClILesoSCgpSCgoKUgtyihIKClIKCgpSCAAkIggAJGLKSgoCCAAwOgoIACRqykoKAggATIoKChIKCgpSCAAUQgoKUgoKUgtaCgoCktO6ygoSCggAKDsQAH0iykoKCgpSCAAwMggAIHLKSgKS0ABEQopIAARKCgpaCggBDHAAKAgBF7AGykoKClIIACwyCABUyspKCgoKClIIACBCApKKCgIK2gqKoooKChICC7IKC2qKCgIKmgIIACQiEkoCCpICCqIqCgpSCgIKkgqiEgoKUgoCCpIKCAB8IggAZPKyCypbKlsqSuLKyooKCgqaC", "1228-1233:1;1292-1306:1;1319-1393:1;1429-1444:1;1524-1532:1;1979-1991:1;2237-2243:1;2263-2269:1;2378-2387:1;2407-2414:1;2572-2580:1;2611-2621:1;2630-2634:1;2752-2755:1;2758-2763:2;2766-2771:3;2774-2777:4;2782-2793:5", "", "490=Name/1/162/11,Name/2/162/12,Name/3/162/13,Name/4/162/14,Name/5/162/15,Name/6/162/16,Name/7/162/17,Name/8/162/18,Name/9/162/19,Name/10/162/20,Name/11/162/21,Name/12/162/22,Name/13/162/23,Name/14/162/24,TypeFor/1/22/24,Name/15/162/25,TypeFor/2/22/25,Name/16/162/26,Name/17/162/27,Name/18/162/28,Errorf/1/13/28,Name/19/162/29,TypeFor/3/22/29,Name/20/162/30,TypeFor/4/22/30,Name/21/162/31,Name/22/162/32,Name/23/162/33,Name/24/162/34,Name/25/162/37,Name/26/162/38,Name/27/162/39,Name/28/162/40,Name/29/162/41,Name/30/162/44,Name/31/162/45,Errorf/2/13/45,Name/32/162/47,Name/33/162/48,Errorf/3/13/48,Name/34/162/49,Name/35/162/50,Name/36/162/51,Errorf/4/13/51,Name/37/162/54,Name/38/162/55,Name/39/162/56,Name/40/162/57,Name/41/162/58,Name/42/162/61,Name/43/162/62,Name/44/162/63,Name/45/162/64,Name/46/162/65,Name/47/162/66,Name/48/162/67,Name/49/162/68,Name/50/162/71,Name/51/162/72,Name/52/162/73,Name/53/162/74,New/1/6/74,Name/54/162/77,Name/55/162/78,Name/56/162/79,Name/57/162/80,Name/58/162/83,Name/59/162/84,Name/60/162/85,Name/61/162/86,Name/62/162/87,Name/63/162/88,Name/64/162/89,Name/65/162/90,Name/66/162/93,Name/67/162/94,Name/68/162/95,Name/69/162/96,Name/70/162/97,Name/71/162/100,Name/72/162/101,Name/73/162/102,Name/74/162/103,Name/75/162/104,Name/76/162/108,Name/77/162/114,Name/78/162/120,Name/79/162/126,Name/80/162/132,Name/81/162/141,Name/82/162/147,Name/83/162/156,TypeFor/5/22/160,Name/84/162/163,TypeFor/6/22/167,Name/85/162/170,TypeFor/7/22/174,Name/86/162/177,TypeFor/8/22/181,Name/87/162/184,TypeFor/9/22/188,Name/88/162/191,TypeFor/10/22/195,Name/89/162/199,Name/90/162/201,Name/91/162/204,Name/92/162/260,Name/93/162/267,Name/94/162/273,Errorf/5/13/277,Name/95/162/281,Name/96/162/287,Errorf/6/13/291,Name/97/162/295,Name/98/162/303,Name/99/162/309,Name/100/162/315,Name/101/162/321,Name/102/162/327,Name/103/162/333,Name/104/162/339,Name/105/162/347,Date/1/1/350,Name/106/162/355,TypeFor/11/22/358,Name/107/162/361,TypeFor/12/22/364,Name/108/162/375,Name/109/162/381,Name/110/162/388,Name/111/162/394,Name/112/162/401,Name/113/162/407,Name/114/162/414,Name/115/162/420,Name/116/162/429,Name/117/162/436,Name/118/162/443,Name/119/162/450,Name/120/162/457,Name/121/162/458,Name/122/162/459,Name/123/162/460,Name/124/162/461,Name/125/162/462,Name/126/162/463,Name/127/162/464,Name/128/162/465,Name/129/162/466,Name/130/162/467,Name/131/162/470,TypeFor/13/22/477,Name/132/162/482,TypeFor/14/22/490,Name/133/162/496,TypeFor/15/22/504,Name/134/162/511,Name/135/162/512,Name/136/162/513,New/2/6/513,Name/137/162/514,New/3/6/514,Name/138/162/515,New/4/6/515,Name/139/162/516,Name/140/162/517,New/5/6/517,Name/141/162/518,New/6/6/518,Name/142/162/522,Errorf/7/13/568,Name/143/162/572,Errorf/8/13/618,Name/144/162/624,TypeFor/16/22/628,Name/145/162/631,TypeFor/17/22/635,Name/146/162/640,TypeFor/18/22/643,Name/147/162/646,TypeFor/19/22/649,Name/148/162/653,TypeFor/20/22/660,Name/149/162/665,TypeFor/21/22/673,Name/150/162/679,Name/151/162/688,Errorf/9/13/691,Name/152/162/694,Errorf/10/13/697,Name/153/162/700,Errorf/11/13/705,Name/154/162/708,Errorf/12/13/712,Name/155/162/716,Name/156/162/722,Name/157/162/728,Name/158/162/734,Name/159/162/740,Errorf/13/13/745,Name/160/162/748,Name/161/162/762,Name/162/162/768,TypeFor/22/22/772;1216=Name/1/6/5,Name/2/6/6,Name/3/6/7,Name/4/6/8,Name/5/6/9,Name/6/6/10;1401=Elem/1/2/1,Interface/1/2/1,Elem/2/2/1,Interface/2/2/1,stripWhitespace/1/2/2,stripWhitespace/2/2/2;1440=Name/1/3/8,Name/2/3/9,Name/3/3/10;2008=Name/1/41/6,Name/2/41/7,Name/3/41/8,Name/4/41/9,Name/5/41/10,Name/6/41/11,Name/7/41/13,Name/8/41/14,Name/9/41/15,addr/1/53/15,Name/10/41/16,addr/2/53/16,Name/11/41/17,Name/12/41/18,addr/3/53/18,addr/4/53/18,Name/13/41/19,addr/5/53/19,addr/6/53/19,addr/7/53/19,Name/14/41/20,Name/15/41/21,addr/8/53/21,addr/9/53/21,Name/16/41/22,addr/10/53/22,addr/11/53/22,addr/12/53/22,Name/17/41/23,addr/13/53/23,addr/14/53/23,addr/15/53/23,addr/16/53/23,Name/18/41/25,Name/19/41/26,Name/20/41/27,Name/21/41/28,addr/17/53/28,addr/18/53/28,Name/22/41/29,addr/19/53/29,addr/20/53/29,Name/23/41/30,Name/24/41/31,addr/21/53/31,addr/22/53/31,addr/23/53/31,Name/25/41/32,addr/24/53/32,addr/25/53/32,addr/26/53/32,addr/27/53/32,Name/26/41/33,Name/27/41/34,addr/28/53/34,addr/29/53/34,addr/30/53/34,addr/31/53/34,Name/28/41/35,addr/32/53/35,addr/33/53/35,addr/34/53/35,addr/35/53/35,addr/36/53/35,Name/29/41/36,addr/37/53/36,addr/38/53/36,addr/39/53/36,addr/40/53/36,addr/41/53/36,addr/42/53/36,Name/30/41/38,Name/31/41/39,Name/32/41/40,Name/33/41/41,addr/43/53/41,Name/34/41/42,addr/44/53/42,Name/35/41/43,Name/36/41/44,addr/45/53/44,Name/37/41/45,addr/46/53/45,addr/47/53/45,Name/38/41/46,Name/39/41/47,addr/48/53/47,Name/40/41/48,addr/49/53/48,addr/50/53/48,Name/41/41/49,addr/51/53/49,addr/52/53/49,addr/53/53/49;2152=Unix/1/1/26,NewInt/1/1/27;2317=Name/1/6/5,Name/2/6/6,Name/3/6/7,Name/4/6/8,Name/5/6/9,Name/6/6/10;2351=Name/1/8/4,Name/2/8/5,Name/3/8/6,Name/4/8/7,Name/5/8/8,Name/6/8/9,Name/7/8/10,Name/8/8/11;2461=Name/1/6/6,Name/2/6/11,Name/3/6/16,Name/4/6/21,Name/5/6/26,Name/6/6/31;2519=Name/1/7/6,Name/2/7/7,TypeFor/1/5/7,Name/3/7/8,TypeFor/2/5/8,Name/4/7/9,Name/5/7/10,TypeFor/3/5/10,Name/6/7/11,TypeFor/4/5/11,Name/7/7/12,TypeFor/5/5/12;2746=Name/1/5/5,Name/2/5/9,Name/3/5/13,Name/4/5/17,Name/5/5/21;2929=Name/1/6/5,Repeat/1/12/6,Repeat/2/12/6,Name/2/6/9,Repeat/3/12/10,Repeat/4/12/10,Name/3/6/13,Repeat/5/12/14,Repeat/6/12/14,Name/4/6/17,Repeat/7/12/18,Repeat/8/12/18,Name/5/6/21,Repeat/9/12/22,Repeat/10/12/22,Name/6/6/25,Repeat/11/12/26,Repeat/12/12/26;2955=Name/1/4/4,Name/2/4/10,Name/3/4/18,Name/4/4/26")]
[assembly: go.GoPositionMap("encoding/json/encode_test.go", "encode_test.cs", "AD1gggAAFoKCgoSCgpSAgvyC7oIAOFyCAAAggoKCgoSChISCgoKEgoKUgIIAFgiCAAAcgoKClICCggAoRIIAABSCgoKChIKClICCABYYogAkRrKCgoKUgIKokoCCpIIADhaCgoKClIKClIKCgpSCAAcQ3AAHEoKCgoKWgoKChIKEgoKCgpSmgoCCyIKAggAJCIIACRqykoCCgIK2AAkOkriCgpSCgt6CpoKCzILMgqaCgsyCABAGggALJoKCgpSAgu6CzILmgoKCgoKUgIKmgoKCgpSAggCHAQiCAAUUAAcgAAYgAAYWAAcYioIABxCKggAIEgAIIgAHIAAINgAEErKSgoKUggAbPqKClAAEEKKClAAYCJIADiaykoCktAAMDpK4goKUgoKCpqaCgpSCgoIAER6SuIKClIKCggAKFoIABxCCgpSCgoIACgiCmoCCAA8ItIKGmoKAgqaKgoKWgoCCpIK0pIKSgoKCggAKCpKYgoKUgJKkgoCktLQACwgAIkyCgoKCgpSCggAJDoD4gPiA+ICkooKC+oAACAaSAA4mspKCgpSSggAIDILcgpSCguqSuIKUgoK4ABQGABMqgoKCgoKCgpSCgoKClJaCgoKClJKCgpaCgpSCgoKCupqCgoKUgoKCgoKClIKCmIKCgoLcgoKCACEGggAIHgBFjgGykoKAgoKUtoCCAAkQgNSigoCCtoKiAAkEgoaCgpSCgoIADwiCgoKEAAkcspKCggAHEIKmggALGIKC", "300-317:1;416-424:1;558-568:1;574-585:2;590-596:3;601-607:4;613-621:5;627-635:6;642-652:7;659-669:8;675-695:9;702-708:10;713-721:11;791-798:1;1066-1075:1;1137-1175:1;1197-1199:2;1305-1317:1;1326-1330:1;1370-1375:1", "", "281=Name/1/2/5,Name/2/2/22;436=Name/1/8/4,NaN/1/1/4,Name/2/8/5,Inf/1/2/5,Name/3/8/6,Inf/2/2/6,Name/4/8/7,Name/5/8/8,Name/6/8/9,Name/7/8/10,Name/8/8/11;472=NaN/1/2/1,NaN/2/2/2;732=Name/1/10/8,Name/2/10/22,Name/3/10/40,Name/4/10/51,Name/5/10/63,Name/6/10/77,Name/7/10/92,Name/8/10/109,Name/9/10/125,Name/10/10/152;903=Name/1/13/5,Name/2/13/6,Name/3/13/7,Name/4/13/8,Name/5/13/9,Name/6/13/10,Name/7/13/11,Name/8/13/12,Name/9/13/13,Name/10/13/14,Name/11/13/15,Name/12/13/16,Name/13/13/17;1261=Name/1/13/5,Name/2/13/6,Name/3/13/7,Name/4/13/8,Name/5/13/9,Name/6/13/10,Name/7/13/11,Name/8/13/12,Name/9/13/13,Name/10/13/14,Name/11/13/15,Name/12/13/16,Name/13/13/17;1353=Invoke/1/17/1,Invoke/2/17/2,Invoke/3/17/3,Invoke/4/17/4,Invoke/5/17/5,Invoke/6/17/6,Invoke/7/17/7,Invoke/8/17/8,Invoke/9/17/9,Invoke/10/17/10,Invoke/11/17/11,Invoke/12/17/12,Invoke/13/17/13,Invoke/14/17/14,Invoke/15/17/15,Invoke/16/17/17,Invoke/17/17/18;1490=Name/1/54/7,Name/2/54/8,Name/3/54/9,Name/4/54/10,Name/5/54/11,Name/6/54/12,Name/7/54/13,Name/8/54/14,Name/9/54/15,Name/10/54/16,Name/11/54/17,Name/12/54/18,Name/13/54/19,Name/14/54/20,Name/15/54/21,Name/16/54/22,Name/17/54/23,Name/18/54/24,Name/19/54/27,Name/20/54/28,Name/21/54/29,Name/22/54/30,Name/23/54/31,Name/24/54/32,Name/25/54/33,Name/26/54/34,Name/27/54/35,Name/28/54/36,Name/29/54/37,Name/30/54/38,Name/31/54/39,Name/32/54/40,Name/33/54/41,Name/34/54/42,Name/35/54/43,Name/36/54/44,Name/37/54/51,Name/38/54/52,Name/39/54/53,Name/40/54/54,Name/41/54/55,Name/42/54/56,Name/43/54/57,Name/44/54/58,Name/45/54/59,Name/46/54/60,Name/47/54/61,Name/48/54/62,Name/49/54/63,Name/50/54/64,Name/51/54/65,Name/52/54/66,Name/53/54/67,Name/54/54/68;1648=Name/1/2/5,Errorf/1/2/6,String/1/2/7,Name/2/2/9,Errorf/2/2/10,String/2/2/11")]
[assembly: go.GoPositionMap("encoding/json/fold_test.go", "fold_test.cs", "AAsYogAcNpSCkoKCgg==", "42-42:1;43-49:2")]
[assembly: go.GoPositionMap("encoding/json/fuzz_test.go", "fuzz_test.cs", "AAwaogAQIILKgoCCpoKCloCC7KIAECCCgoKCgoKClA==", "30-50:1;32-32:1.1;33-33:1.2;34-34:1.3;70-82:1")]
[assembly: go.GoPositionMap("encoding/json/number_test.go", "number_test.cs", "AAsYhJQAL2KCgpaCgIKmgqgAFjCCgpaCgIKmgg==")]
[assembly: go.GoPositionMap("encoding/json/scanner_test.go", "scanner_test.cs", "ABAggqaCgoKUAAkIggAHGLKSgIIADAyiABYwgrKSgoCCkoCCtoKAgpKAgraCgIKSgIK2goCCkoCCAAwMpr6ykoKAgpKAggAKEJKCgoCCpIKCgoL4ooKCgoCCpIKmmJKAgqSCgoKCqIKAgqSCgoKCAAkIggADELKSgoKAgoL+goKCgoKClILKgt6CgoKUgoKUpoKClKS2lKSkpKaCgoKCgoKUlKaCgoKUgpSCgpSmgoKClIKUgoKU", "21-26:1;43-47:1;78-106:1;121-128:1;197-205:1", "", "37=Name/1/6/5,Name/2/6/6,Name/3/6/7,Name/4/6/8,Name/5/6/9,Name/6/6/10;69=Name/1/9/5,Name/2/9/6,Name/3/9/7,Name/4/9/8,Name/5/9/9,Name/6/9/10,Name/7/9/11,Name/8/9/12,Name/9/9/22;155=Name/1/2/4,Name/2/2/5;256=Name/1/2/5,Name/2/2/6")]
[assembly: go.GoPositionMap("encoding/json/stream_test.go", "stream_test.cs", "ACNAsoKC/KKCgtoAFiqCgoKUgoKCgIK2gJKCggAQCrSChpqEgoKAgqaKgIKmgoCCpIK0ABUmgoKCgoKUgJKC7ILKggAcBoKCggADEJyIABMyspKCgoCCpICCpIKCgIKkgIL+gozSgoKmgoKCgIK2goKCgqYADAqCgoaCgoKUgpSCgpSAksiCgpSCgoCCyAAOBoKKgoKCgpSClIKClIIADQiCjIKCgpSAkqSClIKClIIACAiCvrKSgpKogIKkggAOEoIARJQBspKCooKEgIKClKSAgoKUkoKkggANELKEgpSSgoKUlJiCgoKUgqiCgrQ=", "216-234:1;366-378:1;462-486:1;494-496:1", "", "239=Name/1/6/6,Name/2/6/7,Name/3/6/8,Name/4/6/10,Name/5/6/15,Name/6/6/20;448=Name/1/2/4,Name/2/2/5;486=Name/1/18/6,Name/2/18/7,Name/3/18/9,Name/4/18/11,Name/5/18/13,Name/6/18/15,Name/7/18/20,Name/8/18/23,Name/9/18/29,Name/10/18/33,Name/11/18/37,Name/12/18/42,Name/13/18/47,Name/14/18/51,Name/15/18/57,Name/16/18/62,Repeat/1/2/62,Repeat/2/2/63,Name/17/18/66,Name/18/18/70")]
[assembly: go.GoPositionMap("encoding/json/tagkey_test.go", "tagkey_test.cs", "AFGWAYIAES6ykoKClIKCgpSCgoCCtg==", "100-119:1", "", "83=Name/1/16/6,Name/2/16/7,Name/3/16/8,Name/4/16/9,Name/5/16/10,Name/6/16/11,Name/7/16/12,Name/8/16/13,Name/9/16/14,Name/10/16/15,Name/11/16/16,Name/12/16/17,Name/13/16/18,Name/14/16/19,Name/15/16/20,Name/16/16/21")]
[assembly: go.GoPositionMap("encoding/json/tags_test.go", "tags_test.cs", "ABISgoKClAAEEII=")]
// </GoSourcePositionMaps>

namespace go.encoding;

[GoPackage("json")]
public static partial class json_internal_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    [GoLocalName("Dummy")] partial struct BenchmarkCodeEncoderError_Dummy {}
    [GoLocalName("Dummy")] partial struct BenchmarkCodeMarshalError_Dummy {}
    [GoLocalName("T")] partial struct BenchmarkEncoderEncode_T {}
    [GoValueClone("Where")] partial struct CaseName {}
    [GoValueClone("pc")] partial struct CasePos {}
    [GoValueClone("Foo2")] partial struct OptionalsZero {}
    [GoLocalName("MyInt")] partial struct TestAnonymousFields_MyInt {}
    [GoLocalName("MyInt1")] partial struct TestAnonymousFields_MyInt1 {}
    [GoLocalName("MyInt2")] partial struct TestAnonymousFields_MyInt2 {}
    [GoLocalName("MyInt")] partial struct TestAnonymousFields_MyIntᴛ1 {}
    [GoLocalName("S")] partial struct TestAnonymousFields_S {}
    [GoLocalName("S1")] partial struct TestAnonymousFields_S1 {}
    [GoLocalName("S1")] partial struct TestAnonymousFields_S1ᴛ1 {}
    [GoLocalName("S2")] partial struct TestAnonymousFields_S2 {}
    [GoLocalName("S2")] partial struct TestAnonymousFields_S2ᴛ1 {}
    [GoLocalName("S2")] partial struct TestAnonymousFields_S2ᴛ2 {}
    [GoLocalName("S2")] partial struct TestAnonymousFields_S2ᴛ3 {}
    [GoLocalName("S2")] partial struct TestAnonymousFields_S2ᴛ4 {}
    [GoLocalName("S")] partial struct TestAnonymousFields_Sᴛ1 {}
    [GoLocalName("S")] partial struct TestAnonymousFields_Sᴛ2 {}
    [GoLocalName("S")] partial struct TestAnonymousFields_Sᴛ3 {}
    [GoLocalName("S")] partial struct TestAnonymousFields_Sᴛ4 {}
    [GoLocalName("S")] partial struct TestAnonymousFields_Sᴛ5 {}
    [GoLocalName("S")] partial struct TestAnonymousFields_Sᴛ6 {}
    [GoLocalName("S")] partial struct TestAnonymousFields_Sᴛ7 {}
    [GoLocalName("S")] partial struct TestAnonymousFields_Sᴛ8 {}
    [GoLocalName("S")] partial struct TestAnonymousFields_Sᴛ9 {}
    [GoLocalName("myInt")] partial struct TestAnonymousFields_myInt {}
    [GoLocalName("myInt")] partial struct TestAnonymousFields_myIntᴛ1 {}
    [GoLocalName("myInt")] partial struct TestAnonymousFields_myIntᴛ2 {}
    [GoLocalName("s1")] partial struct TestAnonymousFields_s1 {}
    [GoLocalName("s1")] partial struct TestAnonymousFields_s1ᴛ1 {}
    [GoLocalName("s1")] partial struct TestAnonymousFields_s1ᴛ2 {}
    [GoLocalName("s2")] partial struct TestAnonymousFields_s2 {}
    [GoLocalName("byteKind")] partial struct TestByteKind_byteKind {}
    [GoLocalName("T2")] partial struct TestEmptyString_T2 {}
    [GoLocalName("stringPointer")] partial struct TestEncodePointerString_stringPointer {}
    [GoLocalName("Data")] partial struct TestEncoderErrorAndReuseEncodeState_Data {}
    [GoLocalName("Dummy")] partial struct TestEncoderErrorAndReuseEncodeState_Dummy {}
    [GoLocalName("WrongString")] partial struct TestErrorMessageFromMisusedString_WrongString {}
    [GoValueClone("A")] partial struct TestInvalidStringOption_item {}
    [GoLocalName("Foo")] partial struct TestIssue10281_Foo {}
    [GoLocalName("Data")] partial struct TestMarshalErrorAndReuseEncodeState_Data {}
    [GoLocalName("Dummy")] partial struct TestMarshalErrorAndReuseEncodeState_Dummy {}
    [GoLocalName("T1")] partial struct TestMarshalRawMessageValue_T1 {}
    [GoLocalName("T2")] partial struct TestMarshalRawMessageValue_T2 {}
    [GoLocalName("T")] partial struct TestNullString_T {}
    [GoLocalName("S")] partial struct TestRefUnmarshal_S {}
    [GoLocalName("Uint8")] partial struct TestSliceOfCustomByte_Uint8 {}
    [GoLocalName("stringKind")] partial struct TestStringKind_stringKind {}
    [GoLocalName("S1")] partial struct TestUnmarshalEmbeddedUnexported_S1 {}
    [GoLocalName("S2")] partial struct TestUnmarshalEmbeddedUnexported_S2 {}
    [GoLocalName("S3")] partial struct TestUnmarshalEmbeddedUnexported_S3 {}
    [GoLocalName("S4")] partial struct TestUnmarshalEmbeddedUnexported_S4 {}
    [GoLocalName("S5")] partial struct TestUnmarshalEmbeddedUnexported_S5 {}
    [GoLocalName("S6")] partial struct TestUnmarshalEmbeddedUnexported_S6 {}
    [GoLocalName("S7")] partial struct TestUnmarshalEmbeddedUnexported_S7 {}
    [GoLocalName("S8")] partial struct TestUnmarshalEmbeddedUnexported_S8 {}
    [GoLocalName("S9")] partial struct TestUnmarshalEmbeddedUnexported_S9 {}
    [GoLocalName("embed1")] partial struct TestUnmarshalEmbeddedUnexported_embed1 {}
    [GoLocalName("embed2")] partial struct TestUnmarshalEmbeddedUnexported_embed2 {}
    [GoLocalName("embed3")] partial struct TestUnmarshalEmbeddedUnexported_embed3 {}
    [GoLocalName("T")] partial struct TestUnmarshalRescanLiteralMangledUnquote_T {}
    [GoLocalName("Dummy")] partial struct benchMarshalBytesError_Dummy {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸcompressꓸgzip() => builtin.initPackage(typeof(compress.gzip_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸjson() => builtin.initPackage(typeof(go.encoding.json_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸimage() => builtin.initPackage(typeof(image_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtestenv() => builtin.initPackage(typeof(@internal.testenv_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸlog() => builtin.initPackage(typeof(log_package));
    [GoInit] internal static void initᴛᴛimportꓸmaps() => builtin.initPackage(typeof(maps_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbig() => builtin.initPackage(typeof(go.math.big_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸrand() => builtin.initPackage(typeof(go.math.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸnet() => builtin.initPackage(typeof(net_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸhttp() => builtin.initPackage(typeof(go.net.http_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸhttpꓸhttptest() => builtin.initPackage(typeof(go.net.http.httptest_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸpath() => builtin.initPackage(typeof(path_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸregexp() => builtin.initPackage(typeof(regexp_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸruntimeꓸdebug() => builtin.initPackage(typeof(go.runtime.debug_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
}
