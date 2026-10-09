// go2cs metadata anchor for the INTERNAL (white-box bridge) test class: GoImplement /
// GoImplicitConv attributes whose GENERATED code must merge with a bridge-declared type
// anchor here — the source generators host output in the first class of the
// attribute-bearing file, and only this file's first class is the bridge. Records for
// production and external-test types stay in package_test_info.cs.

// <ImportedTypeAliases>
using testing = go.testing_package;
using Δrand = go.math.rand_package;
// </ImportedTypeAliases>

using go;
using static go.math.big_package;
using static go.math.big_internal_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b6120696e7436343b206220696e7436343b206f757420737472696e677d", "setFrac64Testsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6120696e7436343b206220696e7436343b2070726f6420737472696e677d", "mulRangesZᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b612075696e7436343b20622075696e7436343b2070726f6420737472696e677d", "mulRangesNᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6420737472696e673b207820737472696e673b207920737472696e673b206120737472696e673b206220737472696e677d", "gcdTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b656c656d656e7420737472696e673b206d6f64756c757320737472696e677d", "modInverseTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e20737472696e673b206f757420696e747d", "bitLenTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e20737472696e673b206f757420737472696e673b206261736520696e743b2076616c20696e7436343b206f6b20626f6f6c7d", "stringTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e20737472696e673b206f757420737472696e677d", "notTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e20737472696e673b206f75742075696e747d", "tzbTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e20737472696e673b207072656320696e743b206f757420737472696e677d", "floatStringTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e70757420737472696e673b20666f726d617420737472696e673b206f757470757420737472696e673b2072656d61696e696e6720696e747d", "scanTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e70757420737472696e673b20666f726d617420737472696e673b206f757470757420737472696e677d", "formatTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7261743120737472696e673b207261743220737472696e673b206f757420696e747d", "ratCmpTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7320737472696e673b206261736520696e743b206672616320626f6f6c3b2078206d6174682f6269672e6e61743b206220696e743b20636f756e7420696e743b20657272206572726f723b206e6578742072756e657d", "natScanTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7320737472696e673b2062617365326f6b20626f6f6c3b207365704f6b20626f6f6c3b207820696e7436343b206220696e743b20657272206572726f723b206e6578742072756e657d", "exponentTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7820696e7436343b207920696e7436343b207120696e7436343b207220696e7436343b206420696e7436343b206d20696e7436347d", "divisionSignsTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b78206d6174682f6269672e576f72643b2079206d6174682f6269672e576f72643b2063206d6174682f6269672e576f72643b2071206d6174682f6269672e576f72643b2072206d6174682f6269672e576f72647d", "mulAddWWWTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b78206d6174682f6269672e576f72643b2079206d6174682f6269672e576f72643b2071206d6174682f6269672e576f72643b2072206d6174682f6269672e576f72647d", "mulWWTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b78206d6174682f6269672e6e61743b206220696e743b207320737472696e677d", "strTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b78206d6174682f6269672e6e61743b2079206d6174682f6269672e6e61743b207220696e747d", "cmpTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7820737472696e673b206920696e743b20622075696e747d", "bitsetTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7820737472696e673b20692075696e743b2077616e742075696e747d", "bitTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7820737472696e673b207920737472696e673b20616e6420737472696e673b206f7220737472696e673b20786f7220737472696e673b20616e644e6f7420737472696e677d", "bitwiseTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7820737472696e673b207920737472696e673b206d20737472696e673b206b302075696e7436343b206f7574333220737472696e673b206f7574363420737472696e677d", "montgomeryTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7820737472696e673b207920737472696e673b206d20737472696e673b206f757420737472696e677d", "expTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7820737472696e673b207920737472696e673b206e2075696e743b207a20737472696e677d", "subMod2NTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7820737472696e673b207920737472696e673b207120737472696e673b207220737472696e677d", "quoTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7820737472696e673b207920737472696e673b2073756d20737472696e673b2070726f6420737472696e677d", "ratBinTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7831206d6174682f6269672e576f72643b207830206d6174682f6269672e576f72643b2079206d6174682f6269672e576f72643b2071206d6174682f6269672e576f72643b2072206d6174682f6269672e576f72647d", "divWWTestsᴛ1")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
// </InterfaceImplementations>

// <ImplicitConversions>
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("math/big/arith_test.go", "arith_test.cs", "ABIgAAgQAAsYgoKCgoKCpoIADQiCgoKChIKChIKChIKCypSCpoKCgpSmlIKCgpSCgoKygoLcgoKClIKCgrKCggANGgAKFgAMHgAMHoKCgoKCgqaCuJSCgoSCgoKmgriCggANCIKCgoKEgoKWgoKCgoKWgoKCgoK6koKClAAKDLailIKCgoKCloKCloKCgpaCggAOHpKSkrQAEiqSkpK0ABIqlIKCgoKCgoKCpoK4goKCloKCuISSgoKCgoLcgoKCgoKCgriCgoKUgoKCooKC3pKCgpSCgoKigoLcgoKClIKCgqKCgt6SgoKUgoKCooKCAA0aABk0goKCgoKCpoIADyCCgoKCgoKmggAICIKCgoKEgoIACgoAAxCCgoKCAAwKAAQWgoKCggAJCgAGFIKCgoKCgriCgoKCgpSCgoKCyoKCgpSCgoKCooKC3IKCgpSCgoKigoLagoKClIKCgqKCgtyCgoKUgoKCooKigqaigg==", "95-100:1;112-117:1;201-203:1;403-408:1;421-426:1;438-443:1;456-461:1;633-638:1;650-655:1;666-671:1;683-695:1;685-689:1.1;690-694:1.2")]
[assembly: go.GoPositionMap("math/big/bits_test.go", "bits_test.cs", "ABE4gqaCgoKCpuaCAAgWgoKCAAQSsoKigoKUlIKCgqaC5oIABxSCgoIABBCihqKCgpSCpoKCqrKCgoKUtLaSlNqCgpSUgpSsxIKqkoKCuoKClIKClKiCgIKmpPaCABEmgoCC")]
[assembly: go.GoPositionMap("math/big/calibrate_test.go", "calibrate_test.cs", "ABg6AAkQgoKWlpSCgpSUgpS4gqqigoKC5oKCloKWgoKEgpSWhJaCgqiCgpSEloKWuJSUlKSkpoKCgJKUhISmgoKCgoKCgoKCgoKCgpSCgpSWgpSU", "135-135:1")]
[assembly: go.GoPositionMap("math/big/decimal_test.go", "decimal_test.cs", "ABAYggAKFoCCAAsKggAMIoKCgIIACwqCABU+hIKCgoCCpoKCgIKmgoKAgv6igoKCgsqCgoKCkoKC", "127-132:1")]
[assembly: go.GoPositionMap("math/big/float_test.go", "float_test.cs", "ABAilIKCgpSmgoKClAAIBoSSgIK4gIK4goKCppQAEjKCgoKClILOkoKClAAJBoIAEjSCgoKUgIKkgJIACgqCggAMIIKAggAKCoIABxaCgoLMwqaUppQACAaCAAoegoKCgoLKgoKAgqSAggAKCIIAFSyCgoKCgqaCggAKCoIABxiCgIKkgIKkgIIACAqiAA8ggoKAgtqCgoKUpoKmhJKUpIKUtoKUtqSkpIKogpS0yJaCgoKCuN6Cgsy6goLMAAgKkgBBmAGCgoKCgoSCgoKEgoSCvrKCgoKCgoKCyoIACRSCgoCCupKCgoKCgsqCAAsUgoKUgoKAgsySgoKCgoLKogATJIKClIKCgILMkoKCgoKCuoKAkraClKKkggAHEIKCgoKUhIKWgpSAgriCgs6SAAoWgoKCgpSEkoKCloKUgIKmgoIACgqCggAFFIKAggALCoIAECqCgoIACgqCABo+goKCAAoKggBapAGUooKCgqiAgqaCgoKYkoKCAAsMgoIATZYBlKKCgoKogIKmgoKCmJKCggAODIIAEi6CgoKClIK6goKCgIIACgqCAA8mgoKCgpSCgpSCgqiCgoLMgoKCgILaggAIEoKCgpaCgoLKggAIEoKCgoKClILKgoKCgpSSgoKClILcAAsIAAog4oKUgoKChIKCgoKCgqiCgoIACBjSgoKCgoKUgoKCpoKCAAUStJKCgoKCloKChIKCgoKWgoKCggAFEKSSgoKCgpaCgoSCgoKCloKCgoIACQyCAAUSgoKCgoKWgoKCggAEFOKClIKCgoSCgoKCgoKogpSCgoIADBaiAAkYgoKClIKUgpaCgoSCgoKCloKUgoKCgtyCgoKCgoKEgoKWgoKWgoKEgpSClILKtIKElIKClIKUgpSWhIKqgILKgoKCgoKCAAcQmKKCgpaCgoKCgpaCgpaCgoKChIKCgpSCAAcWsoKCgoKCgoKCuICCpIKCqpSCgrSCgrSCgrSCgrS0lNKCgIKCtqKSgoKUlIKClIKCAA4OggAnSoKCgpS0tLS0tICCAA4e8gANLKKCgoKUtLS0tLSAggAGFLKCgoKCgoK4gIKkgoKCgpS0tILugoKChIKCgoSygoLcgoKChIKCgoSygoI=", "48-55:1;645-649:1;1641-1649:1;1642-1647:1.1;1830-1835:1;1849-1854:1", "", "436=toBinary/1/3/1,toBinary/2/3/2,toBinary/3/3/3;448=toBinary/1/4/1,int64/1/1/2,toBinary/2/4/2,toBinary/3/4/3,toBinary/4/4/4;459=toBinary/1/4/1,int64/1/1/2,toBinary/2/4/2,toBinary/3/4/3,toBinary/4/4/4;1847=Text/1/2/2,Text/2/2/2,Acc/1/1/2")]
[assembly: go.GoPositionMap("math/big/floatconv_test.go", "floatconv_test.cs", "ABgkgoKEAIQBqgKCgoKUgpSmgoKUgoKCyoAADg6iAHy0AoKCgoKWgpSCqIKCzJKAlKQACwaihAB+tAKCgoKUgpaCgt6CgoKClIKCAAsMggBp6gGClLS0xKaAgtqiggASJoKCgtyiggAWLoKCggANDKIADzCCgoKCgoKCgpSWgpaClII=")]
[assembly: go.GoPositionMap("math/big/floatmarsh_test.go", "floatmarsh_test.cs", "ABAgAA0cgoKCgoKCgoKChIKCgoKogpaAgoKmgoCCgqaCgpaCloKWggAKEIKCgoCCpISCgIKmgIKmgoCCyIKCgoKClIKCgoKClIKCgpSCgoCCgqSCAAkOgriCggAJCoIACRqCgsqCgoKCgpSCgoKCgpSCgoKClIKCgIKCpILugoKCgoI=")]
[assembly: go.GoPositionMap("math/big/gcd_test.go", "gcd_test.cs", "AA4gkoKC9oKClIKUgriigoKCgpKCgpSCgriAooCigKKAooCigKKAooCigKKAooCigKKAooCigA==", "26-28:1;29-31:2")]
[assembly: go.GoPositionMap("math/big/hilbert_test.go", "hilbert_test.cs", "ABMqgoKUpoKClKaCgpSCgoKCpoKCgoKCgpSmpoKCgoKmpoKCgoKCgoKEgoKChIKWpqaigpSCgoKCgpSmpqKClIKCgrimgoKCgpSUpoKCgoKCgoKUgoKCuIKmooI=")]
[assembly: go.GoPositionMap("math/big/int_test.go", "int_test.cs", "ABcoooKmAAgQAAkSAAYQgoKCgoKCyoKCgoKClILKgoKCgoKCgoKUgsqCgoKClIIACQiCgoKCgoSChIKEgviCgoKChIK+spaCgoKCgoKCgoKUlKiCgpamgqKCgoSChKaCgIIAEwgAFj6ClJSClIKCuIKCggAJCoKCABQwgILaooKCAAoKAAgagoKCgoKCgoSCgoKUgpSCloKClIKUgpaCgoKUgpSCloKClIKUgsqCgoKUpoIABxCCgoKCgpaCloKCyoKChIKWgpamgoCCyKaClIKmgoCCyIKChIKWgoSCloKChPYADiSCgIKmgoKCgoSChIIAEAqogoSCgoKCgoKEuKKCgoKEgoIACAgADiSCgoKCgpaAggASCgBBoAGCgoKEkpSClJaClJaCgpaCgpSClqaCgoLcooKCgoKCAAgIgoKCABAogoKCwoKC3KKCgoKCgriCgoKChIKCgoSs4oKogoSChIKEgoKEloKCgpaCgoKUpoKChIKWgoSmgoKCgoSCloKE9gAPKOKCgpSCgpaCgpSClIKogoKCgpSClIKWgoKCgpSClIKWgoKCgpSClIKWgoKCgpSClIK4goKCgoKChIKCgpaAgqaAgqaAggAKFAAaNoKCgoKEgpSCyoKCgoKEgpSCygAVLIKCgoKEgpSCyoKCgoKEgpSCyoKCgoKEgpSCpoKCgoSClILcAA0WgoKCgoKClIKUgpaClIKSgoKClIKWgoKUtLSC7oKCgoKUgoKC6gAQKIKCgoKCgpaCgoKCppSWgpaCguoADiSigoKCgoKWgpSCgqaUloKWgoIACQoAKlyCgoSCgriCgoKChIKCuIKCgoKUpoKCgpSkpKaigoKCgoKCgpSCgoKUgpSCgoKUgpSCgoKmggAJCAAOJoKCgoKCgoKUgoKCgoKmgoKCAAgIAAkagoKCgoSCyqKCgoKCuKKCgoKCuKKCgoKCuKKCgoKCvKKCgoKCgqaigoKCgriigoKCgriigoKCgoKCuKKCgoKCgoIACAiCgoKCgoSCgoKCgoKCAAgIAAsegoKCgoKCgoKClIKCAAoKAAcWgrKCgoKCgoKCgriCgoKCgoKUgoKCgtyigoKCgoKCvMKigoKCgqiCgoKUgoKCqIKCqIKUgoKmgsKCkoKWgoKCgqiCqIKogoKClJaCgoKUqIKCgoIACgyCABIulIKCgoKCgvqigoKCgpSUgpSC1pSCpoKCgoKClIKCgoKogoKCgoK6goKCvKKCgoKEqIKCgILaooKCgoK4ooKCgoKCuIKCgpSCyqKCgoKClIKEgoK4griCgoLKgoKCgoKmsoCSgtYACRSigoKCqIKSloKClJaCgpCS7pSCgILIgoKCgsiUggAKCoIAHTiCgoKogoKogoI=", "102-102:1;103-103:2;120-120:1;700-705:1;1756-1762:1;1852-1854:1;1881-1883:1;1888-1894:1;1895-1899:2;1896-1896:2.1;1911-1937:3;1933-1933:3.1;1953-1958:1", "", "40=NewInt/1/18/1,NewInt/2/18/1,NewInt/3/18/1,NewInt/4/18/2,NewInt/5/18/2,NewInt/6/18/2,NewInt/7/18/3,NewInt/8/18/3,NewInt/9/18/3,NewInt/10/18/4,NewInt/11/18/4,NewInt/12/18/4,NewInt/13/18/5,NewInt/14/18/5,NewInt/15/18/5,NewInt/16/18/6,NewInt/17/18/6,NewInt/18/18/6;50=NewInt/1/12/1,NewInt/2/12/1,NewInt/3/12/1,NewInt/4/12/2,NewInt/5/12/2,NewInt/6/12/2,NewInt/7/12/3,NewInt/8/12/3,NewInt/9/12/3,NewInt/10/12/4,NewInt/11/12/4,NewInt/12/12/4;517=String/3/4/1,String/4/4/1")]
[assembly: go.GoPositionMap("math/big/intconv_test.go", "intconv_test.cs", "ABwaADyYAYKCgoKWgoKCloKCloCC2oKCgoKCloKCgpaCgpaCgoCC2oKUpKSkpoKCgoKUhIKAgriCgoKCpoLcooKmgoKCgoKClIKClJSCgpSWgpSCloKUggALCgCGAbICgoKCgoKCgqaCggALCgARLoKCgoKCgoCCpIKUgg==")]
[assembly: go.GoPositionMap("math/big/intmarsh_test.go", "intmarsh_test.cs", "ABAeAAkUgoKCgoKCgoKCgoCCgqSCgIKCpIIABRCigoKEooKClIKCgpSClIKCuIKCgoKCgoKCgpSCgIKCpIIACAyCgoKClIKCgriCgoKCgoKCgoKUgoCCgqSC3IKCgoKCgoKCgoKUgoCCgqSC3IKCgoKC")]
[assembly: go.GoPositionMap("math/big/link_test.go", "link_test.cs", "AB0kooKUgoKCgtiAgqSCgoCCpoKCgoKUgpSUyoKCpoI=")]
[assembly: go.GoPositionMap("math/big/nat_test.go", "nat_test.cs", "ABQeABAogoKCggAKFAAQEgAZQoKCgpSmgoKCgsqCgoIACwiCgoKEgoSChIKWgoKEggANCAAUNIKCgoLMkoKCgoKCrNKCgoKilIKAgsSqsqiSgoKUpqKCgoKCgriigoKCgoK4lIKCgpSCyoKCgoKUAAoUAAgSgoKCgoKCguwACRSCgoKCgoKCAAsMgoSSgoKmkoKokoKCppKCAAoWuLiCgoKChIKCyoKClIIACQgAQo4BooKCgoKCgoKUgpaCgpSCgpaCgpS8goKCgoKogoKCgoKogoKCygAfRoKCgoKEgoKWgoLKgoKClIKCgoKCgsqCgqaCgoLcgpSkpIKCgoKClKYADRyCgoKCgsqigoKCgoKCAAkIABEygoKCgILaAA4ugoKCgIKklIKAgv6CgoKCgoK4goKClIKUgsqigoKCgrjcgoKClIIADQoACSKCgoKCgoKCgoKClKSkgoKUgpSC7oKC3IKCgqKC3IK4goKClIKmgoKCpoKEkoKClIIABRLogoSCgoKAgs6ygoKC", "191-193:1;243-245:1;313-318:1;319-323:2;325-330:3;331-335:4;558-570:1;578-583:1;757-759:1;782-807:1;822-826:1", "", "70=natFromString/1/9/11,natFromString/2/9/12,natFromString/3/9/13,Repeat/1/7/19,natFromString/4/9/19,Repeat/2/7/20,Repeat/3/7/20,natFromString/5/9/20,Repeat/4/7/21,natFromString/6/9/21,Repeat/5/7/27,natFromString/7/9/27,Repeat/6/7/28,natFromString/8/9/28,Repeat/7/7/29,natFromString/9/9/29;677=Repeat/1/4/19,Repeat/2/4/20,Repeat/3/4/21,Repeat/4/4/22;706=Repeat/1/2/19,Repeat/2/2/20")]
[assembly: go.GoPositionMap("math/big/natconv_test.go", "natconv_test.cs", "ABIggoK+sqaUlLS4gpaWgoKCgpYACAYADCKEkqKClKKSgpaCgoKWgoKUgpSCABgKAGLyAYKCgoKClIKUgpSClIKCgpSCAAZ2koKCgpSAgsiCgoKCkoKmgriigoK4goKCgpSSgsqCgoKCgpSCgoKEgoCCpISC7oKCgoKClIKCgoKChILugpKAtoKAyKKCgoKChIKCgoKChIKogoKCpoKCgoKCyoKCgoKClIKCgoKmgg==", "76-81:1;77-79:1.1;322-325:1;345-349:1;359-373:1;385-395:1;402-402:1;406-406:2")]
[assembly: go.GoPositionMap("math/big/prime_test.go", "prime_test.cs", "ABUcABxAAEGQAYKClKaCgoKUgoKCqKKCgoK6goKygoKmgrQACAiCgoKSgrqSgqaSgvqCAAgMgtyCgoKCgpSCpoKCgpKClIKmgg==", "148-157:1;149-153:1.1;164-168:1;171-175:2;176-180:3;185-185:1;192-192:1")]
[assembly: go.GoPositionMap("math/big/rat_test.go", "rat_test.cs", "AA0YoqKEgpaAgqaAgqaCgIKmgoCCpoKAgriCgIK2oqSCgoKSgpSCgoIACQoACx6CgoKEgoLKgoKCgoKUgoKCyoKCgoKClIKClIKCyoKCgoKClIKCgsqCgoKCgpSClIKCggAKFIKCgoKEggAICAAeMIKCgoSChIKEgoSChIKEgoKWgoLKgoKCgoKCloKCgoKCloKCgoIACAgACRqCgoKCypSCgoKCqIKCgqiCgoKogoKCqIKCgoKCuIKCgoKWgoKCqIKCgqiCgoKogoKCgoKogoKogoKCqIKCgoKogoKCqIKCgoKCgriIAAsSkoKWgoKCgoKUgoKClJSChJSo7ogACxKSgpaCgoKCgpSCgoKUlIKElKgABhKigoKAgt6igpSCgoKUgoLOooKUgoKClIKCzJKCrNKmqIKWgoKCgoKUgoKUgoKUgoKUrNKmqIKWgoKCgoKUgoKUgoKUgoKUpoCigKSCAAcQgoKmyoKCyoIACBKCgoKClIKClIKCyoLcgoKCgpSCgpSCgsqigoIACAzCmKKioqKi7IKCgILagoKCgoLGgoKmgg==", "44-48:1;711-711:1;712-712:2;713-713:3;714-714:4;715-715:5;716-716:6;735-741:1", "", "438=Ldexp/1/1/1;484=Ldexp/1/1/1;645=NaN/1/1/1,Inf/1/2/2,Inf/2/2/3")]
[assembly: go.GoPositionMap("math/big/ratconv_test.go", "ratconv_test.cs", "ACEkAB9egoKCgoKUgpSClIKCgpSCAA4UADBWACRggoKChIKEgoKSgqaCkoLcgoKCgriCgoKCgoSCgoKUlJSCAAoKABMwgoKEggAyDgCKAYwDotaiooKClJaCgoKUuoKCuvzogqiUqJaAgtqiooKClJaCgoKUuoK6/OiCqJSoloCC2oKCyoKCggAJCoKCAAwggoIACwiiADpSgpaCgoK6goKCgpSCgqaCgtyClIKChpKEgoKCgu6ClIKChpKEgoKCgu6ClIKCgoaShIKCgoI=", "711-718:1;733-740:1;756-763:1", "", "320=Repeat/1/2/165,Repeat/2/2/175")]
[assembly: go.GoPositionMap("math/big/ratmarsh_test.go", "ratmarsh_test.cs", "ABAegoKCgoKCgoKAgoKkgoCCgqSCzqKCgoSigoKUgoKClIKUgoK4AAsY7oKCgoKCgoKClIKAgoKkgtyCgoKCgoKCgpSCgIKCpILcgsqCgsqCgoKCgoKCgoKUgoCCgqSC")]
[assembly: go.GoPositionMap("math/big/sqrt_test.go", "sqrt_test.cs", "AA4gooKClISCgoKCAAkKggAQLIKChIKCgoIADSCCgpaChIIACw6CAAQQgoIABRKSgoKCooKC", "119-124:1")]
// </GoSourcePositionMaps>

namespace go.math;

[GoPackage("big")]
public static partial class big_internal_test_package
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
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸrand() => builtin.initPackage(typeof(crypto.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸgob() => builtin.initPackage(typeof(encoding.gob_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸhex() => builtin.initPackage(typeof(encoding.hex_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸjson() => builtin.initPackage(typeof(encoding.json_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸxml() => builtin.initPackage(typeof(encoding.xml_package));
    [GoInit] internal static void initᴛᴛimportꓸflag() => builtin.initPackage(typeof(flag_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtestenv() => builtin.initPackage(typeof(@internal.testenv_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbig() => builtin.initPackage(typeof(go.math.big_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbits() => builtin.initPackage(typeof(go.math.bits_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸrand() => builtin.initPackage(typeof(go.math.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸosꓸexec() => builtin.initPackage(typeof(go.os.exec_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtestingꓸquick() => builtin.initPackage(typeof(go.testing.quick_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    [GoInit] internal static void initᴛᴛimportꓸunicode() => builtin.initPackage(typeof(unicode_package));
    // </ImportInitializers>
}
