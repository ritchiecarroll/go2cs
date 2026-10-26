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
global using flagꓸErrorHandling = go.flag_package.ΔErrorHandling;
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
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
// </ImportedTypeAliases>

using go;
using static go.github.com.spf13.pflag_package;

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
[assembly: GoTypeAlias("ParseErrorsWhitelist", "go.github.com.spf13.pflag_package.ParseErrorsAllowlist")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<InvalidSyntaxError, error>(Pointer = true)]
[assembly: GoImplement<InvalidValueError, error>(Pointer = true)]
[assembly: GoImplement<NotExistError, error>(Pointer = true)]
[assembly: GoImplement<ValueRequiredError, error>(Pointer = true)]
[assembly: GoImplement<boolSliceValue, Value>(Pointer = true)]
[assembly: GoImplement<boolValue, Value>(Pointer = true)]
[assembly: GoImplement<boolfuncValue, Value>]
[assembly: GoImplement<bytesBase64Value, Value>(Pointer = true)]
[assembly: GoImplement<bytesHexValue, Value>(Pointer = true)]
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<countValue, Value>(Pointer = true)]
[assembly: GoImplement<durationSliceValue, Value>(Pointer = true)]
[assembly: GoImplement<durationValue, Value>(Pointer = true)]
[assembly: GoImplement<flagValueWrapper, Value>(Pointer = true)]
[assembly: GoImplement<float32SliceValue, Value>(Pointer = true)]
[assembly: GoImplement<float32Value, Value>(Pointer = true)]
[assembly: GoImplement<float64SliceValue, Value>(Pointer = true)]
[assembly: GoImplement<float64Value, Value>(Pointer = true)]
[assembly: GoImplement<funcValue, Value>]
[assembly: GoImplement<int16Value, Value>(Pointer = true)]
[assembly: GoImplement<int32SliceValue, Value>(Pointer = true)]
[assembly: GoImplement<int32Value, Value>(Pointer = true)]
[assembly: GoImplement<int64SliceValue, Value>(Pointer = true)]
[assembly: GoImplement<int64Value, Value>(Pointer = true)]
[assembly: GoImplement<int8Value, Value>(Pointer = true)]
[assembly: GoImplement<intSliceValue, Value>(Pointer = true)]
[assembly: GoImplement<intValue, Value>(Pointer = true)]
[assembly: GoImplement<ipMaskValue, Value>(Pointer = true)]
[assembly: GoImplement<ipNetSliceValue, Value>(Pointer = true)]
[assembly: GoImplement<ipNetValue, Value>(Pointer = true)]
[assembly: GoImplement<ipSliceValue, Value>(Pointer = true)]
[assembly: GoImplement<ipValue, Value>(Pointer = true)]
[assembly: GoImplement<stringArrayValue, Value>(Pointer = true)]
[assembly: GoImplement<stringSliceValue, Value>(Pointer = true)]
[assembly: GoImplement<stringToInt64Value, Value>(Pointer = true)]
[assembly: GoImplement<stringToIntValue, Value>(Pointer = true)]
[assembly: GoImplement<stringToStringValue, Value>(Pointer = true)]
[assembly: GoImplement<stringValue, Value>(Pointer = true)]
[assembly: GoImplement<strings_package.Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<textValue, Value>]
[assembly: GoImplement<timeValue, Value>(Pointer = true)]
[assembly: GoImplement<uint16Value, Value>(Pointer = true)]
[assembly: GoImplement<uint32Value, Value>(Pointer = true)]
[assembly: GoImplement<uint64Value, Value>(Pointer = true)]
[assembly: GoImplement<uint8Value, Value>(Pointer = true)]
[assembly: GoImplement<uintSliceValue, Value>(Pointer = true)]
[assembly: GoImplement<uintValue, Value>(Pointer = true)]
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
[assembly: go.GoPositionMap("github.com/spf13/pflag@v1.0.9/bool.go", "bool.cs", "AA4eooKmgoKCpoKmgKSApIK4goKClKqCqIKCqoKogoKqgqiCgoKqgqiCgg==")]
[assembly: go.GoPositionMap("github.com/spf13/pflag@v1.0.9/bool_func.go", "bool_func.cs", "7ICkgKSApICsgqiCgoKugqiC")]
[assembly: go.GoPositionMap("github.com/spf13/pflag@v1.0.9/bool_slice.go", "bool_slice.cs", "AA0egoKCgqqGloKCqIKCgoKUpoKUppSogqiEgoKmlKaCpoKmgoKClIKmgoKCgoKCpoKmgoKClKaChIKUgoKCgoKCpqiCgoKUqoKogqqCqIKqgpKCqIKSgqqCqII=")]
[assembly: go.GoPositionMap("github.com/spf13/pflag@v1.0.9/bytes.go", "bytes.cs", "AAwcgqiClIKmlKiCpqKCpoSUgqaogpSCpqqCqIKqgqiCqoKCgqiCgoKqgqiCzoKogpSCppSogqaigqaEgoKmqIKUgqaqgqiCqoKogqqCgoKogoKCqoKogg==")]
[assembly: go.GoPositionMap("github.com/spf13/pflag@v1.0.9/count.go", "count.cs", "AAgQooKmhIKClIKCpoKmgKSCgoKUqIKCgpSsgqiCgqiCqIKsgoKCqIKCgqyCqII=")]
[assembly: go.GoPositionMap("github.com/spf13/pflag@v1.0.9/duration.go", "duration.cs", "AAgUooKmgoKCpoKmgKSCuIKCgpSqgqiCqoKogqqCgoKogoKCqoKogg==")]
[assembly: go.GoPositionMap("github.com/spf13/pflag@v1.0.9/duration_slice.go", "duration_slice.cs", "AA0egoKCgqaCgoKCgoKCqIKUlIKmgqaCgoKUpoKmgqaCgoKUgqaCgoKCgoKmgqaCgoKUpoKEgpSCgoKCgoKoqIKCgpSqgqiCqoKogqqCkoKogpKCqoKogg==")]
[assembly: go.GoPositionMap("github.com/spf13/pflag@v1.0.9/errors.go", "errors.cs", "ABQ4gpSmpqamgqaqgqyCAAgYgoKCpqiCqoKsggAIGIKCgoKUlKiCqIKoggAGFIKqgg==")]
[assembly: go.GoPositionMap("github.com/spf13/pflag@v1.0.9/flag.go", "flag.cs", "ABT8AQA+ygGCgoKCgpSCgoKUAAIQooKCgoKClIKCgoCCgt6CgpSmooKqgoKUqIKqgqyCgqaCgoKUlKaCuoKqgoKCpqyCrIKCpoKCgpSUpoK+gqiirIKClIKCgpSCqIKogoKCgqaCgqaCgoKUrIKsgoKClIKUgoKsgoKClIKUgqqCgoKUgqqCqoKoooKCgqaCgu6CgpSClKaClKyigoKClIKUgqqChIKUqIKqgoKqgpSmAA0EpMTEpJSkpKSkAAQSxIKCgoKCgoKmuIKUpKSkpKSkpKSmroKCpoKClIKClKyCgqqUqIKCgpaCrIKYgpaClIKmroKUpIKCgqaCgpSmgoKUgpSkgraCtsyCgqaCgoKUpoKmpoKChKaqgqiCqKKCAAIWgoKogKaAqIKClKqCqICmgKaApoAAAhCCqITugqiCqLKUgoKCgpSCpoKClIKUgoKClIKUgoKCgoKUqqKClIKCAAQWgqiCqqKClKqigpKClL6ChKaChKiClKbCgpKCgqaCgpSClIKkqIKmpILYgoSShJKEgpa4poKClKbSlIKmgpSCgpSCgqSogoKmgqTK2IKEgpKEkoSCkoSClsqmgqaCgpSmsoKWgoKCuKaygoKCgoKCgpSCpoKCgoKUlJSCpq6igoKmlJSCppiCgpSkgpSCpLYABBSigpSCgpSkgpSCpLaogqqErISogqiCqJiC7qiCrIKCgg==", "271-271:1;711-764:1;913-917:1;1178-1180:1", "", "657=String/1/1/5")]
[assembly: go.GoPositionMap("github.com/spf13/pflag@v1.0.9/float32.go", "float32.cs", "AAgQooKmgoKCpoKmgKSCgoKUqIKCgpSqgqiCqoKogqqCgoKogoKCqoKogg==")]
[assembly: go.GoPositionMap("github.com/spf13/pflag@v1.0.9/float32_slice.go", "float32_slice.cs", "AA0egoKCgqaCgoKCgoKCgpSWgpSUgqaCpoKCgpSmgoKClKaCpoKCgpSCpoKCgoKCgqaCpoKCgpSmgoSClIKCgoKCgoKUlqiCgoKUqoKogqqCqIKqgpKCqIKSgqqCqII=")]
[assembly: go.GoPositionMap("github.com/spf13/pflag@v1.0.9/float64.go", "float64.cs", "AAgQooKmgoKCpoKmgKSCuIKCgpSqgqiCqoKogqqCgoKogoKCqoKogg==")]
[assembly: go.GoPositionMap("github.com/spf13/pflag@v1.0.9/float64_slice.go", "float64_slice.cs", "AA0egoKCgqaCgoKCgoKCqIKUlIKmgqaCgoKUpoKmgqaCgoKUgqaCgoKCgoKmgqaCgoKUpoKEgpSCgoKCgoKoqIKCgpSqgqiCqoKogqqCkoKogpKCqoKogg==")]
[assembly: go.GoPositionMap("github.com/spf13/pflag@v1.0.9/func.go", "func.cs", "7ICkgKSArIKogoKugqiC")]
[assembly: go.GoPositionMap("github.com/spf13/pflag@v1.0.9/golangflag.go", "golangflag.cs", "AA4egqaCAA0mhICCtriCgqaCpoKmgqaCrqQABRKClICCpKiigpSCqLKClIKUgpSugoKCgqaUtLS0tLS0xAAFEIKCgoKm", "108-110:1;122-146:1", "", "69=wrapFlagValue/1/1/3,String/1/1/6")]
[assembly: go.GoPositionMap("github.com/spf13/pflag@v1.0.9/int.go", "int.cs", "AAgQooKmgoKCpoKmgKSCuIKCgpSqgqiCqoKogqqCgoKogoKCqoKogg==")]
[assembly: go.GoPositionMap("github.com/spf13/pflag@v1.0.9/int16.go", "int16.cs", "AAgQooKmgoKCpoKmgKSCgoKUqIKCgpSqgqiCqoKogqqCgoKogoKCqoKogg==")]
[assembly: go.GoPositionMap("github.com/spf13/pflag@v1.0.9/int32.go", "int32.cs", "AAgQooKmgoKCpoKmgKSCgoKUqIKCgpSqgqiCqoKogqqCgoKogoKCqoKogg==")]
[assembly: go.GoPositionMap("github.com/spf13/pflag@v1.0.9/int32_slice.go", "int32_slice.cs", "AA0egoKCgqaCgoKCgoKCgpSWgpSUgqaCpoKCgpSmgoKClKaCpoKCgpSCpoKCgoKCgqaCpoKCgpSmgoSClIKCgoKCgoKUlqiCgoKUqoKogqqCqIKqgpKCqIKSgqqCqII=")]
[assembly: go.GoPositionMap("github.com/spf13/pflag@v1.0.9/int64.go", "int64.cs", "AAgQooKmgoKCpoKmgKSCuIKCgpSqgqiCqoKogqqCgoKogoKCqoKogg==")]
[assembly: go.GoPositionMap("github.com/spf13/pflag@v1.0.9/int64_slice.go", "int64_slice.cs", "AA0egoKCgqaCgoKCgoKCqIKUlIKmgqaCgoKUpoKmgqaCgoKUgqaCgoKCgoKmgqaCgoKUpoKEgpSCgoKCgoKoqIKCgpSqgqiCqoKogqqCkoKogpKCqoKogg==")]
[assembly: go.GoPositionMap("github.com/spf13/pflag@v1.0.9/int8.go", "int8.cs", "AAgQooKmgoKCpoKmgKSCgoKUqIKCgpSqgqiCqoKogqqCgoKogoKCqoKogg==")]
[assembly: go.GoPositionMap("github.com/spf13/pflag@v1.0.9/int_slice.go", "int_slice.cs", "AA0egoKCgqaCgoKCgoKCqIKUlIKmgqaCgoKUpoKCgpSCpoKCgoKCgqaCpoKCgpSmgoSClIKCgoKCgqiogoKClKqCqIKqgqiCqoKSgqiCkoKqgqiC")]
[assembly: go.GoPositionMap("github.com/spf13/pflag@v1.0.9/ip.go", "ip.cs", "AAoYooKmgKKCgpSCgpSCpoKmgoKClKiCgoKUqoKogqqCqIKqgoKCqIKCgqqCqII=")]
[assembly: go.GoPositionMap("github.com/spf13/pflag@v1.0.9/ip_slice.go", "ip_slice.cs", "AA4ggoKCgqqGloKCqIKCgoKUpoKUppSogqiEgoKmlKaCpoKmgoKClIKmgoKCgoKCpoKmgoKClKaChIKUgoKCgoKUlKiCgoKUqoKogqqCqIKqgpKCqIKSgqqCqII=")]
[assembly: go.GoPositionMap("github.com/spf13/pflag@v1.0.9/ipmask.go", "ipmask.cs", "AAoYooKmgKKCgoKUgqaCqqKCgoKYgoKCgoKUlIKCgqamgoKClKiCgoKUqoKogqqCqIKqgoKCqIKCgqqCqII=")]
[assembly: go.GoPositionMap("github.com/spf13/pflag@v1.0.9/ipnet.go", "ipnet.cs", "AAoYgpKmgoKClIKmgqaigqaCgoKUqIKCgpSqgqiCqoKogqqCgoKogoKCqoKogg==")]
[assembly: go.GoPositionMap("github.com/spf13/pflag@v1.0.9/ipnet_slice.go", "ipnet_slice.cs", "AA4ggoKCgqqGloKCqIKCgoKUpoKUppSogqiEgrKmgqaChIKUgoKCgoKUlKiCgoKUqoKogqqCqIKqgpKCqIKSgqqCqII=")]
[assembly: go.GoPositionMap("github.com/spf13/pflag@v1.0.9/string.go", "string.cs", "7KKCpoKCpIKmgKSCqIKCgpSqgqiCqoKogqqCgoKogoKCqoKogg==")]
[assembly: go.GoPositionMap("github.com/spf13/pflag@v1.0.9/string_array.go", "string_array.cs", "AAkSgoKCgqaCgoKUlKaCgqaCgoKUgqaCgoKUpoKmgoKmgoSClLiCgoKUrIKogqyCqIKsgpKCqIKSgqyCqII=")]
[assembly: go.GoPositionMap("github.com/spf13/pflag@v1.0.9/string_slice.go", "string_slice.cs", "AA8egoKCgqaCgpSCgqaCgoKCgpSCpoKCgpSClJSCpoKmgoKmgoKmgoKmgqaChIKUuIKCgpQAAhSCqIIAAhSCqIIAAhSCkoKogpKCAAIUgqiC")]
[assembly: go.GoPositionMap("github.com/spf13/pflag@v1.0.9/string_to_int.go", "string_to_int.cs", "AA4ggoKCgqiCgoKCgoKUgoKCpoKUgqaCpoKmgoKCgoKUgoKClKaChIKUgoKCgoKUgoKCpqiCgoKUrIKogqyCqIKsgpKCqIKSgqyCqII=")]
[assembly: go.GoPositionMap("github.com/spf13/pflag@v1.0.9/string_to_int64.go", "string_to_int64.cs", "AA4ggoKCgqiCgoKCgoKUgoKCpoKUgqaCpoKmgoKCgoKUgoKClKaChIKUgoKCgoKUgoKCpqiCgoKUrIKogqyCqIKsgpKCqIKSgqyCqII=")]
[assembly: go.GoPositionMap("github.com/spf13/pflag@v1.0.9/string_to_string.go", "string_to_string.cs", "ABEigoKCgqiigoKUpLSCgoKCyIKCgoKUlIKUgqaCpoKmsoKClJSCgoKmgoKAgqSCpoKEgpSCgoKUgoKCgpSUqIKCgpSsgqiCrIKogqyCkoKogpKCrIKogg==")]
[assembly: go.GoPositionMap("github.com/spf13/pflag@v1.0.9/text.go", "text.cs", "AAwYgoKClIKClIKUgqaCpoKmgoCCgILGqoKogoKClIKUqIKogqiCqII=")]
[assembly: go.GoPositionMap("github.com/spf13/pflag@v1.0.9/time.go", "time.cs", "AA0eooLegoKCgoKUgqaCgoKUpqiCpoKClLqCgoKCpoKCpoKCpqqCqIKqgqiCqoKogoKCqoKogg==")]
[assembly: go.GoPositionMap("github.com/spf13/pflag@v1.0.9/uint.go", "uint.cs", "AAgQooKmgoKCpoKmgKSCgoKUqIKCgpSqgqiCqoKogqqCgoKogoKCqoKogg==")]
[assembly: go.GoPositionMap("github.com/spf13/pflag@v1.0.9/uint16.go", "uint16.cs", "AAgQooKmgoKCpoKmgKSCgoKUqIKCgpSqgqiCqoKogqqCgoKogoKCqoKogg==")]
[assembly: go.GoPositionMap("github.com/spf13/pflag@v1.0.9/uint32.go", "uint32.cs", "AAgQooKmgoKCpoKmgKSCgoKUqIKCgpSqgqiCqoKogqqCgoKogoKCqoKogg==")]
[assembly: go.GoPositionMap("github.com/spf13/pflag@v1.0.9/uint64.go", "uint64.cs", "AAgQooKmgoKCpoKmgKSCgoKUqIKCgpSqgqiCqoKogqqCgoKogoKCqoKogg==")]
[assembly: go.GoPositionMap("github.com/spf13/pflag@v1.0.9/uint8.go", "uint8.cs", "AAgQooKmgoKCpoKmgKSCgoKUqIKCgpSqgqiCqoKogqqCgoKogoKCqoKogg==")]
[assembly: go.GoPositionMap("github.com/spf13/pflag@v1.0.9/uint_slice.go", "uint_slice.cs", "AA0egoKCgqaCgoKCgoKUlIKUlIKmgqaCgoKUpoKCgpSmgqaCgoKUgqaCgoKCgoKmgqaCgoKUpoKEgpSCgoKCgpSUqIKCgpSqgqiCqoKogqqCkoKogpKCqoKogg==")]
// </GoSourcePositionMaps>

namespace go.github.com.spf13;

[GoPackage("pflag", ImportPath = "github.com/spf13/pflag")]
public static partial class pflag_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface boolFlag {}
    internal partial interface goBoolFlag {}
    internal partial struct boolSliceValue {}
    internal partial struct boolValue {}
    internal partial struct bytesBase64Value {}
    internal partial struct bytesHexValue {}
    internal partial struct countValue {}
    internal partial struct durationSliceValue {}
    internal partial struct durationValue {}
    internal partial struct flagValueWrapper {}
    internal partial struct float32SliceValue {}
    internal partial struct float32Value {}
    internal partial struct float64SliceValue {}
    internal partial struct float64Value {}
    internal partial struct int16Value {}
    internal partial struct int32SliceValue {}
    internal partial struct int32Value {}
    internal partial struct int64SliceValue {}
    internal partial struct int64Value {}
    internal partial struct int8Value {}
    internal partial struct intSliceValue {}
    internal partial struct intValue {}
    internal partial struct ipMaskValue {}
    internal partial struct ipNetSliceValue {}
    internal partial struct ipNetValue {}
    internal partial struct ipSliceValue {}
    internal partial struct ipValue {}
    internal partial struct notExistErrorMessageType {}
    internal partial struct stringArrayValue {}
    internal partial struct stringSliceValue {}
    internal partial struct stringToInt64Value {}
    internal partial struct stringToIntValue {}
    internal partial struct stringToStringValue {}
    internal partial struct stringValue {}
    internal partial struct textValue {}
    internal partial struct timeValue {}
    internal partial struct uint16Value {}
    internal partial struct uint32Value {}
    internal partial struct uint64Value {}
    internal partial struct uint8Value {}
    internal partial struct uintSliceValue {}
    internal partial struct uintValue {}
    public partial interface SliceValue {}
    public partial interface Value {}
    public partial struct ErrorHandling {}
    public partial struct Flag {}
    public partial struct FlagSet {}
    public partial struct InvalidSyntaxError {}
    public partial struct InvalidValueError {}
    public partial struct NormalizedName {}
    public partial struct NotExistError {}
    public partial struct ParseErrorsAllowlist {}
    public partial struct ValueRequiredError {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸbase64() => builtin.initPackage(typeof(go.encoding.base64_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸcsv() => builtin.initPackage(typeof(go.encoding.csv_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸhex() => builtin.initPackage(typeof(go.encoding.hex_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸflag() => builtin.initPackage(typeof(flag_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸnet() => builtin.initPackage(typeof(net_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸsort() => builtin.initPackage(typeof(sort_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
}
