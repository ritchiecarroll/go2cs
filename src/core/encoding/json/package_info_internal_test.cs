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
[assembly: GoDynamicTypeLift("7374727563747b4620737472696e6720226a736f6e3a5c222d2c5c22227d", "Δtypeᴛ4")]
[assembly: GoDynamicTypeLift("7374727563747b4620737472696e6720226a736f6e3a5c222d2c6f6d6974656d7074795c22227d", "Δtypeᴛ5")]
[assembly: GoDynamicTypeLift("7374727563747b4e20656e636f64696e672f6a736f6e2e4e756d62657220226a736f6e3a5c222c737472696e675c22227d", "Δtypeᴛ3")]
[assembly: GoDynamicTypeLift("7374727563747b4e20656e636f64696e672f6a736f6e2e4e756d6265727d", "Δtypeᴛ2")]
[assembly: GoDynamicTypeLift("7374727563747b5620696e747d", "NestedUnamed_F")]
[assembly: GoDynamicTypeLift("7374727563747b5820666c6f6174363420226a736f6e3a5c222c737472696e675c22227d", "Δtypeᴛ8")]
[assembly: GoDynamicTypeLift("7374727563747b5820696e74363420226a736f6e3a5c222c737472696e675c22227d", "Δtypeᴛ6")]
[assembly: GoDynamicTypeLift("7374727563747b582075696e74363420226a736f6e3a5c222c737472696e675c22227d", "Δtypeᴛ7")]
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
[assembly: go.GoPositionMap("encoding/json/bench_test.go", "bench_test.cs", "ADVgooKClJKCgoKmlICCtoCCtoKCgoKCgoKmtKSigoKCgpSCgoKAgsgACwaigoKCgpialIKCgoCCpICCyKaigoKCgpSCgoCCyAAIBqKCgoKCmJqUgoKAgqSAgsjmgqaqkoKAggAQDIKmrpqUkoKAgqSAguyilKaUpqKUppTWooLKkoKAguyigoKCgpSCgoKCgpSCgoKAgsimooKCgoKCgoKCgIKkuKKCgoKCgoKAgqSCgoKClIKApLTqooKCgoKUgoKCgILIpqKCgoKClIKCgoCCyKaigoKSgoKAguyigoKSgoKAguyigoKSgoKAguyigoKSkoKAguyigoKSgoKAggAKDKKCuoKCgILsooKCkoKCgILsooKCgriCyIKCuILKgoKigoKCgoKCkoKUpu6UgoKUkpKCAAsOopSqgpSCgIIACgyigoiSgoCC7KKCguiigoKC6KKCgoKCgII=", "91-98:1;118-128:1;139-145:1;165-174:1;187-193:1;214-223:1;255-261:1;271-285:1;338-345:1;356-363:1;370-377:1;383-390:1;396-403:1;409-416:1;422-429:1;439-445:1;451-458:1;480-482:1;488-504:2;495-500:2.1;515-521:3;516-520:3.1;533-541:1;550-556:1", "", "227=Repeat/1/1/3;256=Repeat/1/1/3;602=TypeFor/1/1/1")]
[assembly: go.GoPositionMap("encoding/json/decode_test.go", "decode_test.cs", "AD+AAYLKgpKAgqSCAAgS7gARJoKCAAoYktaCgoKUgqYACBKCppSCgpSCgpSCppaSkpSSkpQAcugByIKmgoKUgoKUgsqCpoLKgqaCgpSCgpSCyoKmgsqCpoKClIKClILKgqaCyoKmgoKUgoKUgsqCpoIAPSQAxwbaDYKCgpSCgoKmgoKUgoKCAAkIggAHGLKSgoLcgoKCgpSCguiCAB4+goKUgoK4goIACBSUpoKUgpTWgrKSgoKAgoK2gqaCgpSmlO6mgoKUgpSUgpSmgIKSpqSAgoKCyIKCgpSClIKCgpSAgqSCAAgSgoKCgIKkgoKUgoKCAAwKkgAEGLKSgoCCpICCkoKkgIKSguyCgoKUgoKUgoCCpIKCAAcQgoKigIKkgriCgpKAgqSCAAkIgoKCgoKUggANDKQABxyykoKCgoKCAEKeAQAnTgB15gEAWrIBAAkEggAFGoKUgoCCpIIACwyiioKCgKS0AAsMooyCgoKCgKS0tNiiAA0GgoIALWaykpKCgIKAgqSkggAmVgAIDgAhQAAePJSCgpSGpoKUgpSClIKUgpSClIKUgpSClIKmgpSClIIACQyCAAgKgsaChIKClIKCgpSC3rKEgoKUgoKClILcooSCgpSCgoKUggAJCIIABxiykoKAggAMDoKCAAkaspKCgIIAEyKCgpSCgoKUggAHEIKClIKClILWgoKApLTesoKUgoIACg7EAB9IspKCgoKUggAMDIIACByykoCktAAQEKKSAAISgoKmgoIAQxwACgIARewBspKCgpSCAAsMggAVMrKSgoKCgpSCAAgQgKSigoCCtoKiqKKCgpSAguyCgtqigoCCtoCCAAkIhJKAgqSAgqiagoKUgoCCpIK4lIKClIKAgqSCggAfCIIAGjysgsqWypbKksiysqKCgoKmgg==", "1334-1339:1;1398-1412:1;1425-1507:1;1543-1558:1;1638-1646:1;2093-2105:1;2351-2357:1;2377-2383:1;2492-2501:1;2521-2528:1;2686-2694:1;2725-2735:1;2744-2748:1;2866-2869:1;2872-2877:2;2880-2885:3;2888-2891:4;2896-2907:5", "", "473=Name/1/175/11,Name/2/175/12,Name/3/175/13,Name/4/175/14,Name/5/175/15,Name/6/175/16,Name/7/175/17,Name/8/175/18,Name/9/175/19,Name/10/175/20,Name/11/175/21,Name/12/175/22,Name/13/175/23,Name/14/175/24,TypeFor/1/28/24,Name/15/175/25,TypeFor/2/28/25,Name/16/175/26,Name/17/175/27,Name/18/175/28,Errorf/1/13/28,Name/19/175/29,TypeFor/3/28/29,Name/20/175/30,TypeFor/4/28/30,Name/21/175/31,Name/22/175/32,Name/23/175/33,Name/24/175/34,Name/25/175/37,Name/26/175/38,Name/27/175/39,Name/28/175/40,Name/29/175/41,Name/30/175/44,Name/31/175/45,Errorf/2/13/45,Name/32/175/47,Name/33/175/48,Errorf/3/13/48,Name/34/175/49,Name/35/175/50,Name/36/175/51,Errorf/4/13/51,Name/37/175/54,Name/38/175/55,Name/39/175/56,Name/40/175/57,Name/41/175/58,Name/42/175/59,Name/43/175/60,Name/44/175/63,Name/45/175/64,Name/46/175/65,Name/47/175/66,Name/48/175/67,Name/49/175/68,Name/50/175/69,Name/51/175/70,Name/52/175/73,Name/53/175/74,Name/54/175/75,Name/55/175/76,New/1/6/76,Name/56/175/79,Name/57/175/80,Name/58/175/81,Name/59/175/82,Name/60/175/85,Name/61/175/86,Name/62/175/87,Name/63/175/88,Name/64/175/89,Name/65/175/90,Name/66/175/91,Name/67/175/92,Name/68/175/95,Name/69/175/96,Name/70/175/97,Name/71/175/98,Name/72/175/99,Name/73/175/102,Name/74/175/103,Name/75/175/104,Name/76/175/105,Name/77/175/106,Name/78/175/110,Name/79/175/116,Name/80/175/122,Name/81/175/128,Name/82/175/134,Name/83/175/143,Name/84/175/149,Name/85/175/158,TypeFor/5/28/162,Name/86/175/165,TypeFor/6/28/169,Name/87/175/172,TypeFor/7/28/176,Name/88/175/179,TypeFor/8/28/183,Name/89/175/186,TypeFor/9/28/190,Name/90/175/193,TypeFor/10/28/197,Name/91/175/201,Name/92/175/203,Name/93/175/206,Name/94/175/262,Name/95/175/269,Name/96/175/275,Errorf/5/13/279,Name/97/175/283,Name/98/175/289,Errorf/6/13/293,Name/99/175/297,Name/100/175/305,Name/101/175/311,Name/102/175/317,Name/103/175/323,Name/104/175/329,Name/105/175/335,Name/106/175/341,Name/107/175/349,Date/1/1/352,Name/108/175/357,TypeFor/11/28/360,Name/109/175/363,TypeFor/12/28/366,Name/110/175/377,Name/111/175/383,Name/112/175/390,Name/113/175/396,Name/114/175/403,Name/115/175/409,Name/116/175/416,Name/117/175/422,Name/118/175/431,Name/119/175/438,Name/120/175/445,Name/121/175/452,Name/122/175/459,Name/123/175/460,Name/124/175/461,Name/125/175/462,Name/126/175/463,Name/127/175/464,Name/128/175/465,Name/129/175/466,Name/130/175/467,Name/131/175/468,Name/132/175/469,Name/133/175/472,TypeFor/13/28/479,Name/134/175/484,TypeFor/14/28/492,Name/135/175/498,TypeFor/15/28/506,Name/136/175/513,Name/137/175/514,Name/138/175/515,New/2/6/515,Name/139/175/516,New/3/6/516,Name/140/175/517,New/4/6/517,Name/141/175/518,Name/142/175/519,New/5/6/519,Name/143/175/520,New/6/6/520,Name/144/175/524,Errorf/7/13/570,Name/145/175/574,Errorf/8/13/620,Name/146/175/626,TypeFor/16/28/630,Name/147/175/633,TypeFor/17/28/637,Name/148/175/642,TypeFor/18/28/645,Name/149/175/648,TypeFor/19/28/651,Name/150/175/655,TypeFor/20/28/662,Name/151/175/667,TypeFor/21/28/675,Name/152/175/681,Name/153/175/690,Errorf/9/13/693,Name/154/175/696,Errorf/10/13/699,Name/155/175/702,Errorf/11/13/707,Name/156/175/710,Errorf/12/13/714,Name/157/175/718,Name/158/175/724,Name/159/175/730,Name/160/175/736,Name/161/175/742,Errorf/13/13/747,Name/162/175/750,Name/163/175/764,Name/164/175/770,TypeFor/22/28/774,Name/165/175/778,Name/166/175/788,Name/167/175/799,TypeFor/23/28/803,Name/168/175/806,TypeFor/24/28/810,Name/169/175/813,TypeFor/25/28/817,Name/170/175/822,Name/171/175/832,TypeFor/26/28/837,Name/172/175/840,Name/173/175/850,TypeFor/27/28/855,Name/174/175/858,Name/175/175/868,TypeFor/28/28/873;1342=Name/1/6/5,Name/2/6/6,Name/3/6/7,Name/4/6/8,Name/5/6/9,Name/6/6/10;1541=Elem/1/2/1,Interface/1/2/1,Elem/2/2/1,Interface/2/2/1,stripWhitespace/1/2/2,stripWhitespace/2/2/2;1580=Name/1/3/8,Name/2/3/9,Name/3/3/10;2147=Name/1/41/6,Name/2/41/7,Name/3/41/8,Name/4/41/9,Name/5/41/10,Name/6/41/11,Name/7/41/13,Name/8/41/14,Name/9/41/15,addr/1/53/15,Name/10/41/16,addr/2/53/16,Name/11/41/17,Name/12/41/18,addr/3/53/18,addr/4/53/18,Name/13/41/19,addr/5/53/19,addr/6/53/19,addr/7/53/19,Name/14/41/20,Name/15/41/21,addr/8/53/21,addr/9/53/21,Name/16/41/22,addr/10/53/22,addr/11/53/22,addr/12/53/22,Name/17/41/23,addr/13/53/23,addr/14/53/23,addr/15/53/23,addr/16/53/23,Name/18/41/25,Name/19/41/26,Name/20/41/27,Name/21/41/28,addr/17/53/28,addr/18/53/28,Name/22/41/29,addr/19/53/29,addr/20/53/29,Name/23/41/30,Name/24/41/31,addr/21/53/31,addr/22/53/31,addr/23/53/31,Name/25/41/32,addr/24/53/32,addr/25/53/32,addr/26/53/32,addr/27/53/32,Name/26/41/33,Name/27/41/34,addr/28/53/34,addr/29/53/34,addr/30/53/34,addr/31/53/34,Name/28/41/35,addr/32/53/35,addr/33/53/35,addr/34/53/35,addr/35/53/35,addr/36/53/35,Name/29/41/36,addr/37/53/36,addr/38/53/36,addr/39/53/36,addr/40/53/36,addr/41/53/36,addr/42/53/36,Name/30/41/38,Name/31/41/39,Name/32/41/40,Name/33/41/41,addr/43/53/41,Name/34/41/42,addr/44/53/42,Name/35/41/43,Name/36/41/44,addr/45/53/44,Name/37/41/45,addr/46/53/45,addr/47/53/45,Name/38/41/46,Name/39/41/47,addr/48/53/47,Name/40/41/48,addr/49/53/48,addr/50/53/48,Name/41/41/49,addr/51/53/49,addr/52/53/49,addr/53/53/49;2295=Unix/1/1/26,NewInt/1/1/27;2464=Name/1/6/5,Name/2/6/6,Name/3/6/7,Name/4/6/8,Name/5/6/9,Name/6/6/10;2496=Name/1/8/4,Name/2/8/5,Name/3/8/6,Name/4/8/7,Name/5/8/8,Name/6/8/9,Name/7/8/10,Name/8/8/11;2609=Name/1/6/6,Name/2/6/11,Name/3/6/16,Name/4/6/21,Name/5/6/26,Name/6/6/31;2667=Name/1/7/6,Name/2/7/7,TypeFor/1/5/7,Name/3/7/8,TypeFor/2/5/8,Name/4/7/9,Name/5/7/10,TypeFor/3/5/10,Name/6/7/11,TypeFor/4/5/11,Name/7/7/12,TypeFor/5/5/12;2895=Name/1/5/5,Name/2/5/9,Name/3/5/13,Name/4/5/17,Name/5/5/21;3083=Name/1/6/5,Repeat/1/12/6,Repeat/2/12/6,Name/2/6/9,Repeat/3/12/10,Repeat/4/12/10,Name/3/6/13,Repeat/5/12/14,Repeat/6/12/14,Name/4/6/17,Repeat/7/12/18,Repeat/8/12/18,Name/5/6/21,Repeat/9/12/22,Repeat/10/12/22,Name/6/6/25,Repeat/11/12/26,Repeat/12/12/26;3110=Name/1/4/4,Name/2/4/10,Name/3/4/18,Name/4/4/26")]
[assembly: go.GoPositionMap("encoding/json/encode_test.go", "encode_test.cs", "AENoggAAFoKCgpSCgpSAgvyC7oIAOFyCAAAggoKCgpSClJSCgoKUgoKUgIIAFgiCAAAcgoKClICCggAoRIIAABSCgoKClIKClICCABYYogAjRrKCgoKUgIK4koCCpIIADxaCgoKClIKClIKCgpSCAAcQ3AAHEoKCgoKWgoKClIKUgoKCgpSmgoCCyIKAggAJCIIACRqykoCCgIK2AAkOkriCgpSCgu6CpoKC3ILcgqaCgtyCABAGggALJoKCgpSAgv6C3ILmgoKCgoKUgIK2goKCgpSAggCHAQiCAAUUAAcgAAYgAAYWAAcYioIABxCKggAIEgAIIgAHIAAINgAFErKSgoKUggAePqKClAAHEKKClAAYCJIADiaykoCktAALDpK4goKUgoKCpqaCgpSCgoIAER6SuIKClIKCggAKFoIABxCCgpSCgoIACgiCqoCCAA8ItIKGmoKAgqaKgoKmgoCCpIK0pIKSgoKCggAKCpKYgoKUgJKkgoCktLQACQgAIkyCgoKCgpSCggAJDoD4gPiA+ICkooKC+oAACAaSAA4mspKCgpSSggAIDILcgpSCguqSuIKUgoK4pgAUKoKCgoKCgoKUgoKCgpSmgoKCgpSCgoKmgoKUgoKCgsqqgoKClIKCgoKCgpSCgpiCgoKC3IKCggAhBoIACR4AQY4BspKCgIKClLaAggAJEIDUooKAgraCogAJBIKGgoKUgoKCAA8IgoKClAAKHLKSgoIABxCCpoIACxiCggAEELKCgoKCgoLK", "304-321:1;420-428:1;562-572:1;578-589:2;594-600:3;605-611:4;617-625:5;631-639:6;646-656:7;663-673:8;679-699:9;706-712:10;717-725:11;795-802:1;1070-1079:1;1141-1179:1;1201-1203:2;1309-1321:1;1330-1334:1;1374-1379:1;1415-1422:1;1416-1421:1.1", "", "293=Name/1/2/5,Name/2/2/22;451=Name/1/8/4,NaN/1/1/4,Name/2/8/5,Inf/1/2/5,Name/3/8/6,Inf/2/2/6,Name/4/8/7,Name/5/8/8,Name/6/8/9,Name/7/8/10,Name/8/8/11;487=NaN/1/2/1,NaN/2/2/2;754=Name/1/10/8,Name/2/10/22,Name/3/10/40,Name/4/10/51,Name/5/10/63,Name/6/10/77,Name/7/10/92,Name/8/10/109,Name/9/10/125,Name/10/10/152;932=Name/1/13/5,Name/2/13/6,Name/3/13/7,Name/4/13/8,Name/5/13/9,Name/6/13/10,Name/7/13/11,Name/8/13/12,Name/9/13/13,Name/10/13/14,Name/11/13/15,Name/12/13/16,Name/13/13/17;1289=Name/1/13/5,Name/2/13/6,Name/3/13/7,Name/4/13/8,Name/5/13/9,Name/6/13/10,Name/7/13/11,Name/8/13/12,Name/9/13/13,Name/10/13/14,Name/11/13/15,Name/12/13/16,Name/13/13/17;1363=Invoke/1/17/1,Invoke/2/17/2,Invoke/3/17/3,Invoke/4/17/4,Invoke/5/17/5,Invoke/6/17/6,Invoke/7/17/7,Invoke/8/17/8,Invoke/9/17/9,Invoke/10/17/10,Invoke/11/17/11,Invoke/12/17/12,Invoke/13/17/13,Invoke/14/17/14,Invoke/15/17/15,Invoke/16/17/17,Invoke/17/17/18;1505=Name/1/54/7,Name/2/54/8,Name/3/54/9,Name/4/54/10,Name/5/54/11,Name/6/54/12,Name/7/54/13,Name/8/54/14,Name/9/54/15,Name/10/54/16,Name/11/54/17,Name/12/54/18,Name/13/54/19,Name/14/54/20,Name/15/54/21,Name/16/54/22,Name/17/54/23,Name/18/54/24,Name/19/54/27,Name/20/54/28,Name/21/54/29,Name/22/54/30,Name/23/54/31,Name/24/54/32,Name/25/54/33,Name/26/54/34,Name/27/54/35,Name/28/54/36,Name/29/54/37,Name/30/54/38,Name/31/54/39,Name/32/54/40,Name/33/54/41,Name/34/54/42,Name/35/54/43,Name/36/54/44,Name/37/54/51,Name/38/54/52,Name/39/54/53,Name/40/54/54,Name/41/54/55,Name/42/54/56,Name/43/54/57,Name/44/54/58,Name/45/54/59,Name/46/54/60,Name/47/54/61,Name/48/54/62,Name/49/54/63,Name/50/54/64,Name/51/54/65,Name/52/54/66,Name/53/54/67,Name/54/54/68;1660=Name/1/2/5,Errorf/1/2/6,String/1/2/7,Name/2/2/9,Errorf/2/2/10,String/2/2/11")]
[assembly: go.GoPositionMap("encoding/json/fold_test.go", "fold_test.cs", "AAwcogAcNpSCkoKCgg==", "44-44:1;45-51:2")]
[assembly: go.GoPositionMap("encoding/json/fuzz_test.go", "fuzz_test.cs", "AA0eogARIILKgoCCtoKCpoCC7KIAESCCgoKCgoKClA==", "32-52:1;34-34:1.1;35-35:1.2;36-36:1.3;72-84:1")]
[assembly: go.GoPositionMap("encoding/json/number_test.go", "number_test.cs", "AAwchKQAMGKCgqaCgIK2grgAFzCCgqaCgIK2gg==")]
[assembly: go.GoPositionMap("encoding/json/scanner_test.go", "scanner_test.cs", "ABEkgqaCgoKUAAkIggAHGLKSgIIADAyiABYygrKSgoCCkoCCxoKAgpKAgsaCgIKSgILGgoCCkoCCAAwMpr6ykoKAgpKAggAKEJKCgoCCpIKCgoL4ooKCgoCCpIKmqJKAgqSCgoKCuIKAgqSCgoKCAAkIggADELKSgoKAgoL+goKCgoKClILKgt6CgoKUgoKUpoKClKSmlKSklKaCgoKCgoKUlKaCgoKUgpSCgpSmgoKClIKUgoKU", "23-28:1;45-49:1;81-109:1;124-131:1;200-208:1", "", "38=Name/1/6/5,Name/2/6/6,Name/3/6/7,Name/4/6/8,Name/5/6/9,Name/6/6/10;70=Name/1/10/5,Name/2/10/6,Name/3/10/7,Name/4/10/8,Name/5/10/9,Name/6/10/10,Name/7/10/11,Name/8/10/12,Name/9/10/22,Name/10/10/23;159=Name/1/2/4,Name/2/2/5;262=Name/1/2/5,Name/2/2/6")]
[assembly: go.GoPositionMap("encoding/json/stream_test.go", "stream_test.cs", "ACREsoKC/KKCgsoAFiqCgoKUgoKCgIK2gJKCggAQCrSChpqUgoKAgqaKgIK2goCCpIK0ABUmgoKCgoKUgJKC7ILKggAcBoKCggAEEKyYABMyspKCgoCCpICCpIKCgIKkgIL+gozSgoKmgoKCgIK2goKCgqYADAqCgoaCgoKUgpSCgpSAksiCgpSCgoCCyAAOBoKKgoKCgpSClIKClIIADQiCjIKCgpSAkqSClIKClIIACAiCvrKSgpK4gIKkggAOEoIASZoBspKCooKUgpSUgIK2gIKClKSAgoKUkoKkggANELKUgpSSgoKUpKiCgoKUgriCgrT0ggARKoKCgoCCgpQAEQyCiOzsgpKCgoKUgoKUlJKCgoKUgoKUlIKCgoKAgpKCpIKClIKClII=", "218-236:1;368-380:1;467-500:1;508-510:1;591-602:1;603-614:2", "", "244=Name/1/6/6,Name/2/6/7,Name/3/6/8,Name/4/6/10,Name/5/6/15,Name/6/6/20;453=Name/1/2/4,Name/2/2/5;492=Name/1/19/6,Name/2/19/7,Name/3/19/9,Name/4/19/11,Name/5/19/13,Name/6/19/15,Name/7/19/20,Name/8/19/23,Name/9/19/29,Name/10/19/33,Name/11/19/37,Name/12/19/42,Name/13/19/47,Name/14/19/51,Name/15/19/57,Name/16/19/62,Repeat/1/2/62,Repeat/2/2/63,Name/17/19/66,Name/18/19/70,Name/19/19/73")]
[assembly: go.GoPositionMap("encoding/json/tagkey_test.go", "tagkey_test.cs", "AFKaAYIAES6ykoKClIKCgpSCgoCCtg==", "102-121:1", "", "84=Name/1/16/6,Name/2/16/7,Name/3/16/8,Name/4/16/9,Name/5/16/10,Name/6/16/11,Name/7/16/12,Name/8/16/13,Name/9/16/14,Name/10/16/15,Name/11/16/16,Name/12/16/17,Name/13/16/18,Name/14/16/19,Name/15/16/20,Name/16/16/21")]
[assembly: go.GoPositionMap("encoding/json/tags_test.go", "tags_test.cs", "ABMWgoKClAAEEII=")]
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
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸjson() => builtin.initPackage(typeof(go.encoding.json_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸimage() => builtin.initPackage(typeof(image_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtestenv() => builtin.initPackage(typeof(@internal.testenv_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸzstd() => builtin.initPackage(typeof(@internal.zstd_package));
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
    [GoInit] internal static void initᴛᴛimportꓸtestingꓸsynctest() => builtin.initPackage(typeof(go.testing.synctest_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
}
