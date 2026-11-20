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
global using driverꓸValue = object;
// </ImportedTypeAliases>

using go;
using static go.github.com.google.uuid_package;

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
[assembly: GoImplement<invalidLengthError, error>]
// </InterfaceImplementations>

// <ImplicitConversions>
// </ImplicitConversions>

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: GoFieldAccess(typeof(NullUUID))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("github.com/google/uuid@v1.6.0/dce.go", "dce.cs", "ABRAgoKCgoKUroKugsqiqqLGgpSkpKQ=")]
[assembly: GoPositionMap("github.com/google/uuid@v1.6.0/hash.go", "hash.cs", "AA8e0tLS2AAFFqKCgoKCgoKCgq6irqI=")]
[assembly: GoPositionMap("github.com/google/uuid@v1.6.0/marshal.go", "marshal.cs", "AA0UooKCqIKCgpSCqKKogoKUgg==")]
[assembly: GoPositionMap("github.com/google/uuid@v1.6.0/node.go", "node.cs", "ABEqooKCAAUSooKC1oKCgoKCrIKCgpSqooKCgpSC3KKClIKCgoL6ooKC")]
[assembly: GoPositionMap("github.com/google/uuid@v1.6.0/node_net.go", "node_net.cs", "AAwmgoKCgoKmgoKm")]
[assembly: GoPositionMap("github.com/google/uuid@v1.6.0/null.go", "null.cs", "ABAcAAgqgoKipoKCgqaCqKKClqiigqaogoKUgoKoooKmqIKCgoKUgoKoooKmqKKCgpSCgg==")]
[assembly: GoPositionMap("github.com/google/uuid@v1.6.0/sql.go", "sql.cs", "AA8egpSogqiCgqa4gqqClMamrKI=")]
[assembly: GoPositionMap("github.com/google/uuid@v1.6.0/time.go", "time.cs", "ABo8urKCgoLMooKC1oKWgpSYgpSCAAIUooKC1oKClKqigoKipIKCgoKUgoKC3KKClIK0grSCgoK0qqI=")]
[assembly: GoPositionMap("github.com/google/uuid@v1.6.0/util.go", "util.cs", "AAoYgoK6ABIogoKC")]
[assembly: GoPositionMap("github.com/google/uuid@v1.6.0/uuid.go", "uuid.cs", "ACJQAAwSgsiCggACGIKCzIKUuLiCgoKCpqSogpTugoKUlKiCgsaClLS0goKCgqakqIKU7oKClJSqgoKClKqygqiigpQAAhKCzIKUuIKUuIKCgtqogoKUgoLKyqKCgqqigoKCxqKCgoKCgoKCgsiilKSkpMqixoKClMaClKSkpKSkAAQSgoKClAACHIIAAhKigoKCouyCgqKU")]
[assembly: GoPositionMap("github.com/google/uuid@v1.6.0/version1.go", "version1.cs", "AAsmgoKCgqaCgoKUgoKClIKClIKU")]
[assembly: GoPositionMap("github.com/google/uuid@v1.6.0/version4.go", "version4.cs", "AAoagr6CAAMigoKUqIKCgoKUgoKmgoKCgoKCgpSUgoKUgoI=")]
[assembly: GoPositionMap("github.com/google/uuid@v1.6.0/version6.go", "version6.cs", "AAsqgoKCggACIoKUgpSCgpSClA==")]
[assembly: GoPositionMap("github.com/google/uuid@v1.6.0/version7.go", "version7.cs", "AAsugoKClIKsgoKCpoKsAAAclJSCgoKCgpSCAAYcwoKUgoSCgoKCgpSC")]
// </GoSourcePositionMaps>

namespace go.github.com.google;

[GoPackage("uuid", ImportPath = "github.com/google/uuid")]
public static partial class uuid_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct invalidLengthError {}
    public partial struct Domain {}
    [GoValueClone("UUID")] public partial struct NullUUID {}
    public partial struct Time {}
    public partial struct UUID {}
    public partial struct UUIDs {}
    public partial struct Variant {}
    public partial struct Version {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸmd5() => builtin.initPackage(typeof(crypto.md5_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸrand() => builtin.initPackage(typeof(crypto.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸsha1() => builtin.initPackage(typeof(crypto.sha1_package));
    [GoInit] internal static void initᴛᴛimportꓸdatabaseꓸsqlꓸdriver() => builtin.initPackage(typeof(database.sql.driver_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸbinary() => builtin.initPackage(typeof(encoding.binary_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸhex() => builtin.initPackage(typeof(encoding.hex_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸjson() => builtin.initPackage(typeof(encoding.json_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸhash() => builtin.initPackage(typeof(hash_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸnet() => builtin.initPackage(typeof(net_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(uuid_package.Domain)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class uuidᴛDomain;
[GoMethodHost(typeof(uuid_package.NullUUID)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class uuidᴛNullUUID;
[GoMethodHost(typeof(uuid_package.Time)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class uuidᴛTime;
[GoMethodHost(typeof(uuid_package.UUID)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class uuidᴛUUID;
[GoMethodHost(typeof(uuid_package.UUIDs)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class uuidᴛUUIDs;
[GoMethodHost(typeof(uuid_package.Variant)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class uuidᴛVariant;
[GoMethodHost(typeof(uuid_package.Version)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class uuidᴛVersion;
[GoMethodHost(typeof(uuid_package.invalidLengthError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class uuidᴛinvalidLengthError;
// </MethodHolders>
