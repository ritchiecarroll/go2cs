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
global using jsonꓸToken = object;
global using jsonꓸΔToken = object;
global using osꓸDirEntry = go.io.fs_package.DirEntry;
global using osꓸFileInfo = go.io.fs_package.FileInfo;
global using osꓸFileMode = go.io.fs_package.FileMode;
global using osꓸPathError = go.io.fs_package.PathError;
global using osꓸSignal = go.os_package.ΔSignal;
global using runtimeꓸError = go.runtime_package.ΔError;
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
// </ImportedTypeAliases>

using go;
using static go.crypto.@internal.cryptotest_package;

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
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<go.math.rand_package.Rand, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<testing_package.T, testing_package.TB>(Pointer = true)]
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
[assembly: go.GoPositionMap("crypto/internal/cryptotest/aead.go", "aead.cs", "AA4cABAUwoKClqaCgpKEgoSCgoSChILepoKCkpKEgoSCgoSChIKCqJKEgoSCgqiCgoSCggAGEKaCkpSCkpKEgqiCgoSClpKSsKaCgrCmkoSCloKCgoKolpKCkrCmgsqCgrDupoKClJKEgoSCgoKWgoKCgpaCloKUgILckoSChIKCgpaCgoKChJaClpSCAAcSkoKmgoKShIKEgoKEloKCgoSC3qaCgoKWkoSChIKChJaCgoKEgt6mgoSShIKEgoKEloKCgoSCAAcUwoSEhKiClqa4goKCgoKssoSCgpY=", "30-54:1;35-51:1.1;56-105:2;61-102:2.1;62-79:2.1.1;81-101:2.1.2;107-179:3;115-176:3.1;116-140:3.1.1;134-134:3.1.1.1;139-139:3.1.1.2;142-175:3.1.2;163-163:3.1.2.1;174-174:3.1.2.2;181-256:4;186-253:4.1;188-219:4.1.1;221-252:4.1.2;258-289:5;265-286:5.1;291-324:6;300-321:6.1;326-356:7;332-353:7.1")]
[assembly: go.GoPositionMap("crypto/internal/cryptotest/allocations.go", "allocations.cs", "ABcmtIKogqiCuoKo")]
[assembly: go.GoPositionMap("crypto/internal/cryptotest/block.go", "block.cs", "ABMitIKChIKCloSSlpLMkpaEhIKEgqikhIKEggAODKKCqIKChISCgqiChJaCqIKCqIKEgpaCkoKCloKCqIKClIK6goKC3oKEgoKCloKUgoKEgoKCuoKCqIKWgoKEloKWhIKCqIKEgpaSkoCmgoKApoKCgNy4lpCSkKaQkpCSkMiihIKEhIKmog==", "30-32:1;34-36:2;41-67:3;72-87:1;89-104:2;106-141:3;146-174:4;176-197:5;199-219:6;208-208:6.1;213-213:6.2;218-218:6.3;224-238:7;228-228:7.1;231-231:7.2;232-232:7.3;235-235:7.4;236-236:7.5;237-237:7.6;244-252:1")]
[assembly: go.GoPositionMap("crypto/internal/cryptotest/blockmode.go", "blockmode.cs", "AA8mooKChKaCkpaSlpKEgoCCpoKEgoKCAA0KooSCgpCmkoSCgoSCgqiShISCgoSCgrqShISUgqiCgrqihJKWgpKCgqaCgqiCgpSCuoKCgqiChJKWgpSCzJKEgoKCloKUgoKEgoSCqJKEgpaSkpCmgoKQpoKCkLikgoKCsLiShIKEkoKEhII=", "28-30:1;32-34:2;36-52:3;58-61:1;60-60:1.1;63-74:2;76-90:3;92-109:4;111-161:5;151-153:5.1;165-187:6;189-209:7;198-198:7.1;203-203:7.2;208-208:7.3;212-219:8;217-217:8.1;221-236:9")]
[assembly: go.GoPositionMap("crypto/internal/cryptotest/fetchmodule.go", "fetchmodule.cs", "AB4k0oKogoKUgoCCgILGgpSWhIKClIaAgqY=")]
[assembly: go.GoPositionMap("crypto/internal/cryptotest/hash.go", "hash.cs", "ABco1oKChIKCgpaWgoSWgqiAgriAktyCgoSCgoKWhIKogoKEloKCgoKEgrqCgoKEgoKChJaClIKChIKEgrqCgoSCgpaCgoSWgpaCzJKEgoSCgpaCurKEgoSCloKWpoKCgg==", "23-56:1;59-74:2;76-92:3;95-121:4;124-147:5")]
[assembly: go.GoPositionMap("crypto/internal/cryptotest/implementations.go", "implementations.cs", "ABYk5IKCloKCgpaAlIKAgpSmgoKUgpSUAAYQgg==", "31-31:1;37-51:2")]
[assembly: go.GoPositionMap("crypto/internal/cryptotest/stream.go", "stream.cs", "ABEkAAsYxIKmqISCgoSChIKEgoKCAAYQhIKChIKWgpSWgoKC3oKEgoKEgoKogoKChISyhIKCzIKEhJSCqIKCupKEgoKEgoSioJSCzIKEgoSCgpakkpKApoKCgKaCgoDKgoSCgpaEgoKUhJaCgoKClISCAAkUxIKEgoKCkKaCgoKEyIKEgpQ=", "32-87:1;40-59:1.1;43-57:1.1.1;63-86:1.2;66-84:1.2.1;89-100:2;102-118:3;109-116:3.1;120-137:4;139-157:5;149-155:5.1;150-150:5.1.1;159-187:6;170-185:6.1;174-174:6.1.1;179-179:6.1.2;184-184:6.1.3;189-220:7;229-236:1;235-235:1.1;238-244:2;243-243:2.1")]
// </GoSourcePositionMaps>

namespace go.crypto.@internal;

[GoPackage("cryptotest")]
public static partial class cryptotest_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct FetchModule_j {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸcipher() => builtin.initPackage(typeof(go.crypto.cipher_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸboring() => builtin.initPackage(typeof(go.crypto.@internal.boring_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸimpl() => builtin.initPackage(typeof(go.crypto.@internal.impl_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸsubtle() => builtin.initPackage(typeof(go.crypto.subtle_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸjson() => builtin.initPackage(typeof(encoding.json_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸhash() => builtin.initPackage(typeof(hash_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸrace() => builtin.initPackage(typeof(go.@internal.race_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtestenv() => builtin.initPackage(typeof(go.@internal.testenv_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸrand() => builtin.initPackage(typeof(math.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
}
