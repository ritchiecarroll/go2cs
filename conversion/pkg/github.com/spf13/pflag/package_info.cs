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

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: GoFieldAccess(typeof(Flag))]
[assembly: GoFieldAccess(typeof(FlagSet))]
[assembly: GoFieldAccess(typeof(boolSliceValue))]
[assembly: GoFieldAccess(typeof(durationSliceValue))]
[assembly: GoFieldAccess(typeof(flagValueWrapper))]
[assembly: GoFieldAccess(typeof(float32SliceValue))]
[assembly: GoFieldAccess(typeof(float64SliceValue))]
[assembly: GoFieldAccess(typeof(int32SliceValue))]
[assembly: GoFieldAccess(typeof(int64SliceValue))]
[assembly: GoFieldAccess(typeof(intSliceValue))]
[assembly: GoFieldAccess(typeof(ipNetSliceValue))]
[assembly: GoFieldAccess(typeof(ipSliceValue))]
[assembly: GoFieldAccess(typeof(stringArrayValue))]
[assembly: GoFieldAccess(typeof(stringSliceValue))]
[assembly: GoFieldAccess(typeof(stringToInt64Value))]
[assembly: GoFieldAccess(typeof(stringToIntValue))]
[assembly: GoFieldAccess(typeof(stringToStringValue))]
[assembly: GoFieldAccess(typeof(timeValue))]
[assembly: GoFieldAccess(typeof(uintSliceValue))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("github.com/spf13/pflag@v1.0.9/bool.go", "bool.cs", "ABIeooLGgoKCpoKmlLSC2IKCgpSqgqiCgsqCqIKCyoKogoKCyoKogoI=")]
[assembly: GoPositionMap("github.com/spf13/pflag@v1.0.9/bool_func.go", "bool_func.cs", "AA0MlJSUvIKogoKCzoKogg==")]
[assembly: GoPositionMap("github.com/spf13/pflag@v1.0.9/bool_slice.go", "bool_slice.cs", "ABEegoKCgsqGloKCqIKCgoKUpoKUppSogqiEgoKmlKaCpoKmgoKClIKmgoKCgoKCpoKmgoKClMaChIKUgoKCgoKCpsiCgoKUqoKogsqCqILKgoKCqIKCgsqCqII=")]
[assembly: GoPositionMap("github.com/spf13/pflag@v1.0.9/bytes.go", "bytes.cs", "ABIcgqiClIKmlKiCxqKCpoSUgqbIgpSCpqqCqILKgqiCyoKCgqiCgoLKgqiC7oKogpSCppSogsaigqaEgoKmyIKUgqaqgqiCyoKogsqCgoKogoKCyoKogg==")]
[assembly: GoPositionMap("github.com/spf13/pflag@v1.0.9/count.go", "count.cs", "AAwQooLGhIKClIKCpoKmtIKCgpTIgoKClKyCqIKCyIKogsyCgoKogoKCzIKogg==")]
[assembly: GoPositionMap("github.com/spf13/pflag@v1.0.9/duration.go", "duration.cs", "AAwUooLGgoKCpoKmtILYgoKClKqCqILKgqiCyoKCgqiCgoLKgqiC")]
[assembly: GoPositionMap("github.com/spf13/pflag@v1.0.9/duration_slice.go", "duration_slice.cs", "ABEegoKCgsaCgoKCgoKCqIKUlIKmgqaCgoKUpoKmgqaCgoKUgqaCgoKCgoKmgqaCgoKUxoKEgpSCgoKCgoKoyIKCgpSqgqiCyoKogsqCgoKogoKCyoKogg==")]
[assembly: GoPositionMap("github.com/spf13/pflag@v1.0.9/errors.go", "errors.cs", "ABo4gpSmpqamgqaqgqyCAAwYgoKCpqiCqoKsggAMGIKCgoKUlKiCqIKoggAKFIKqgg==")]
[assembly: GoPositionMap("github.com/spf13/pflag@v1.0.9/flag.go", "flag.cs", "ABj8AQA+ygGCgoKCgpSCgoKUAAQQooKCgoKClIKCgoKCzoKClKaigqqCgpSogqqCrIKCpoKCgpSUpoK6gqqCgoKmzILMgoKmgoKClJSmgt6CyKKsgoKUgoKClIKogqiCgoKCpoKCpoKCgpSsgqyCgoKUgpSCgqyCgoKUgpSCqoKCgpSCyoKqgsiigoKCpoKC7oKClIKUpoKUrKKCgoKUgpSCqoKEgpTIgsqCgsqClKYADQSkxMSkpKSkpKQABhLEgoKCgoKCgqa4gpS0tLS0tLS0tLaugoKmgoKUgoKUrIKCqpSogoKCloKsgpiCloKUgqbOgpSUgoKCpoKClKaCgpSCpLSCxoLG3IKCpoKCgpSmgqamgoKEpqqCyIKoooIAAhaCgti2uIKClMqCyLa2tgADEIKohO6CqIKospSCgoKClIKmgoKUgpSCgoKUgpSCgoKCgpSqooKUgoIABhaCqILKooKUqqKCgoKU3oKEpoKEqIKUxsKCgoKCpoKClIKUgqSogqakgtiChIKEgoSClrimgoKUptKUgqaClIKClIKCpKiCgqaCpMrYgoSCgoSChIKChIKWyqaCpoKClKaygpaCgoK4prKCgoKCgoKClIKmgoKCgpSUlIKmrqKCgqaUlIKmmIKClKSClIK0xgAIFKKClIKClKSClIK0xqiCyoSshKiCqIKomILuyIKsgoKC", "271-271:1;711-764:1;913-917:1;1178-1180:1", "", "706=String/1/1/5")]
[assembly: GoPositionMap("github.com/spf13/pflag@v1.0.9/float32.go", "float32.cs", "AAwQooLGgoKCpoKmtIKCgpTIgoKClKqCqILKgqiCyoKCgqiCgoLKgqiC")]
[assembly: GoPositionMap("github.com/spf13/pflag@v1.0.9/float32_slice.go", "float32_slice.cs", "ABEegoKCgsaCgoKCgoKCgpSWgpSUgqaCpoKCgpSmgoKClKaCpoKCgpSCpoKCgoKCgqaCpoKCgpTGgoSClIKCgoKCgoKUlsiCgoKUqoKogsqCqILKgoKCqIKCgsqCqII=")]
[assembly: GoPositionMap("github.com/spf13/pflag@v1.0.9/float64.go", "float64.cs", "AAwQooLGgoKCpoKmtILYgoKClKqCqILKgqiCyoKCgqiCgoLKgqiC")]
[assembly: GoPositionMap("github.com/spf13/pflag@v1.0.9/float64_slice.go", "float64_slice.cs", "ABEegoKCgsaCgoKCgoKCqIKUlIKmgqaCgoKUpoKmgqaCgoKUgqaCgoKCgoKmgqaCgoKUxoKEgpSCgoKCgoKoyIKCgpSqgqiCyoKogsqCgoKogoKCyoKogg==")]
[assembly: GoPositionMap("github.com/spf13/pflag@v1.0.9/func.go", "func.cs", "AA0MlJS8gqiCgs6CqII=")]
[assembly: GoPositionMap("github.com/spf13/pflag@v1.0.9/golangflag.go", "golangflag.cs", "ABIegqaCAA0mhIKmuIKCpoLGgqaCpoLOpAAFEoKUgpTIooKUgqiygpSClIKUroKCgoKmlLS0tLS0tMQABxCCgoKCpg==", "108-110:1;122-146:1", "", "75=wrapFlagValue/1/1/3,String/1/1/6")]
[assembly: GoPositionMap("github.com/spf13/pflag@v1.0.9/int.go", "int.cs", "AAwQooLGgoKCpoKmtILYgoKClKqCqILKgqiCyoKCgqiCgoLKgqiC")]
[assembly: GoPositionMap("github.com/spf13/pflag@v1.0.9/int16.go", "int16.cs", "AAwQooLGgoKCpoKmtIKCgpTIgoKClKqCqILKgqiCyoKCgqiCgoLKgqiC")]
[assembly: GoPositionMap("github.com/spf13/pflag@v1.0.9/int32.go", "int32.cs", "AAwQooLGgoKCpoKmtIKCgpTIgoKClKqCqILKgqiCyoKCgqiCgoLKgqiC")]
[assembly: GoPositionMap("github.com/spf13/pflag@v1.0.9/int32_slice.go", "int32_slice.cs", "ABEegoKCgsaCgoKCgoKCgpSWgpSUgqaCpoKCgpSmgoKClKaCpoKCgpSCpoKCgoKCgqaCpoKCgpTGgoSClIKCgoKCgoKUlsiCgoKUqoKogsqCqILKgoKCqIKCgsqCqII=")]
[assembly: GoPositionMap("github.com/spf13/pflag@v1.0.9/int64.go", "int64.cs", "AAwQooLGgoKCpoKmtILYgoKClKqCqILKgqiCyoKCgqiCgoLKgqiC")]
[assembly: GoPositionMap("github.com/spf13/pflag@v1.0.9/int64_slice.go", "int64_slice.cs", "ABEegoKCgsaCgoKCgoKCqIKUlIKmgqaCgoKUpoKmgqaCgoKUgqaCgoKCgoKmgqaCgoKUxoKEgpSCgoKCgoKoyIKCgpSqgqiCyoKogsqCgoKogoKCyoKogg==")]
[assembly: GoPositionMap("github.com/spf13/pflag@v1.0.9/int8.go", "int8.cs", "AAwQooLGgoKCpoKmtIKCgpTIgoKClKqCqILKgqiCyoKCgqiCgoLKgqiC")]
[assembly: GoPositionMap("github.com/spf13/pflag@v1.0.9/int_slice.go", "int_slice.cs", "ABEegoKCgsaCgoKCgoKCqIKUlIKmgqaCgoKUpoKCgpSCpoKCgoKCgqaCpoKCgpTGgoSClIKCgoKCgqjIgoKClKqCqILKgqiCyoKCgqiCgoLKgqiC")]
[assembly: GoPositionMap("github.com/spf13/pflag@v1.0.9/ip.go", "ip.cs", "AA4YooLGgoKClIKClIKmgsaCgoKUyIKCgpSqgqiCyoKogsqCgoKogoKCyoKogg==")]
[assembly: GoPositionMap("github.com/spf13/pflag@v1.0.9/ip_slice.go", "ip_slice.cs", "ABIggoKCgsqGloKCqIKCgoKUpoKUppSogqiEgoKmlKaCpoKmgoKClIKmgoKCgoKCpoKmgoKClMaChIKUgoKCgoKUlMiCgoKUqoKogsqCqILKgoKCqIKCgsqCqII=")]
[assembly: GoPositionMap("github.com/spf13/pflag@v1.0.9/ipmask.go", "ipmask.cs", "AA4YooLGgoKCgpSCpoLKooKCgpiCgoKCgpSUgoKCpqaCgoKUyIKCgpSqgqiCyoKogsqCgoKogoKCyoKogg==")]
[assembly: GoPositionMap("github.com/spf13/pflag@v1.0.9/ipnet.go", "ipnet.cs", "ABAYgoKmgoKClIKmgsaigqaCgoKUyIKCgpSqgqiCyoKogsqCgoKogoKCyoKogg==")]
[assembly: GoPositionMap("github.com/spf13/pflag@v1.0.9/ipnet_slice.go", "ipnet_slice.cs", "ABIggoKCgsqGloKCqIKCgoKUpoKUppSogqiEgrKmgsaChIKUgoKCgoKUlMiCgoKUqoKogsqCqILKgoKCqIKCgsqCqII=")]
[assembly: GoPositionMap("github.com/spf13/pflag@v1.0.9/string.go", "string.cs", "AAsMooLGgoKUgqa0gsiCgoKUqoKogsqCqILKgoKCqIKCgsqCqII=")]
[assembly: GoPositionMap("github.com/spf13/pflag@v1.0.9/string_array.go", "string_array.cs", "AA4SgoKCgsaCgoKUlKaCgqaCgoKUgqaCgoKUpoKmgoLGgoSClNiCgoKUrIKogsyCqILMgoKCqIKCgsyCqII=")]
[assembly: GoPositionMap("github.com/spf13/pflag@v1.0.9/string_slice.go", "string_slice.cs", "ABMegoKCgqaCgpSCgqaCgoKCgpSCxoKCgpSClJSCpoKmgoKmgoKmgoKmgsaChIKU2IKCgpQAAhSCqIIABBSCqIIABBSCgoKogoKCAAQUgqiC")]
[assembly: GoPositionMap("github.com/spf13/pflag@v1.0.9/string_to_int.go", "string_to_int.cs", "ABIggoKCgsiCgoKCgoKUgoKCpoKUgqaCpoKmgoKCgoKUgoKClMaChIKUgoKCgoKUgoKCpsiCgoKUrIKogsyCqILMgoKCqIKCgsyCqII=")]
[assembly: GoPositionMap("github.com/spf13/pflag@v1.0.9/string_to_int64.go", "string_to_int64.cs", "ABIggoKCgsiCgoKCgoKUgoKCpoKUgqaCpoKmgoKCgoKUgoKClMaChIKUgoKCgoKUgoKCpsiCgoKUrIKogsyCqILMgoKCqIKCgsyCqII=")]
[assembly: GoPositionMap("github.com/spf13/pflag@v1.0.9/string_to_string.go", "string_to_string.cs", "ABUigoKCgsiigoKUpLSCgoKCyIKCgoKUlIKUgqaCpoKmsoKClJSCgoKmgoKClILGgoSClIKCgpSCgoKClJTIgoKClKyCqILMgqiCzIKCgqiCgoLMgqiC")]
[assembly: GoPositionMap("github.com/spf13/pflag@v1.0.9/text.go", "text.cs", "ABAYgoKClIKClIKUgsaCpoKmgoKCpqqCyIKCgpSClKiCqILIgqiC")]
[assembly: GoPositionMap("github.com/spf13/pflag@v1.0.9/time.go", "time.cs", "ABEeooL+goKCgoKUgqaCgoKUpqiCpoKClNqCgoKCpoKCpoKCpqqCqILKgqiCyoKogoKCyoKogg==")]
[assembly: GoPositionMap("github.com/spf13/pflag@v1.0.9/uint.go", "uint.cs", "AAwQooLGgoKCpoKmtIKCgpTIgoKClKqCqILKgqiCyoKCgqiCgoLKgqiC")]
[assembly: GoPositionMap("github.com/spf13/pflag@v1.0.9/uint16.go", "uint16.cs", "AAwQooLGgoKCpoKmtIKCgpTIgoKClKqCqILKgqiCyoKCgqiCgoLKgqiC")]
[assembly: GoPositionMap("github.com/spf13/pflag@v1.0.9/uint32.go", "uint32.cs", "AAwQooLGgoKCpoKmtIKCgpTIgoKClKqCqILKgqiCyoKCgqiCgoLKgqiC")]
[assembly: GoPositionMap("github.com/spf13/pflag@v1.0.9/uint64.go", "uint64.cs", "AAwQooLGgoKCpoKmtIKCgpTIgoKClKqCqILKgqiCyoKCgqiCgoLKgqiC")]
[assembly: GoPositionMap("github.com/spf13/pflag@v1.0.9/uint8.go", "uint8.cs", "AAwQooLGgoKCpoKmtIKCgpTIgoKClKqCqILKgqiCyoKCgqiCgoLKgqiC")]
[assembly: GoPositionMap("github.com/spf13/pflag@v1.0.9/uint_slice.go", "uint_slice.cs", "ABEegoKCgsaCgoKCgoKUlIKUlIKmgqaCgoKUpoKCgpSmgqaCgoKUgqaCgoKCgoKmgqaCgoKUxoKEgpSCgoKCgpSUyIKCgpSqgqiCyoKogsqCgoKogoKCyoKogg==")]
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

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(pflag_package.Flag)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pflagᴛFlag;
[GoMethodHost(typeof(pflag_package.FlagSet)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pflagᴛFlagSet;
[GoMethodHost(typeof(pflag_package.InvalidSyntaxError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pflagᴛInvalidSyntaxError;
[GoMethodHost(typeof(pflag_package.InvalidValueError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pflagᴛInvalidValueError;
[GoMethodHost(typeof(pflag_package.NotExistError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pflagᴛNotExistError;
[GoMethodHost(typeof(pflag_package.ValueRequiredError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pflagᴛValueRequiredError;
[GoMethodHost(typeof(pflag_package.boolSliceValue)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pflagᴛboolSliceValue;
[GoMethodHost(typeof(pflag_package.boolValue)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pflagᴛboolValue;
[GoMethodHost(typeof(pflag_package.boolfuncValue)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pflagᴛboolfuncValue;
[GoMethodHost(typeof(pflag_package.bytesBase64Value)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pflagᴛbytesBase64Value;
[GoMethodHost(typeof(pflag_package.bytesHexValue)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pflagᴛbytesHexValue;
[GoMethodHost(typeof(pflag_package.countValue)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pflagᴛcountValue;
[GoMethodHost(typeof(pflag_package.durationSliceValue)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pflagᴛdurationSliceValue;
[GoMethodHost(typeof(pflag_package.durationValue)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pflagᴛdurationValue;
[GoMethodHost(typeof(pflag_package.flagValueWrapper)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pflagᴛflagValueWrapper;
[GoMethodHost(typeof(pflag_package.float32SliceValue)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pflagᴛfloat32SliceValue;
[GoMethodHost(typeof(pflag_package.float32Value)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pflagᴛfloat32Value;
[GoMethodHost(typeof(pflag_package.float64SliceValue)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pflagᴛfloat64SliceValue;
[GoMethodHost(typeof(pflag_package.float64Value)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pflagᴛfloat64Value;
[GoMethodHost(typeof(pflag_package.funcValue)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pflagᴛfuncValue;
[GoMethodHost(typeof(pflag_package.int16Value)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pflagᴛint16Value;
[GoMethodHost(typeof(pflag_package.int32SliceValue)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pflagᴛint32SliceValue;
[GoMethodHost(typeof(pflag_package.int32Value)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pflagᴛint32Value;
[GoMethodHost(typeof(pflag_package.int64SliceValue)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pflagᴛint64SliceValue;
[GoMethodHost(typeof(pflag_package.int64Value)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pflagᴛint64Value;
[GoMethodHost(typeof(pflag_package.int8Value)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pflagᴛint8Value;
[GoMethodHost(typeof(pflag_package.intSliceValue)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pflagᴛintSliceValue;
[GoMethodHost(typeof(pflag_package.intValue)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pflagᴛintValue;
[GoMethodHost(typeof(pflag_package.ipMaskValue)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pflagᴛipMaskValue;
[GoMethodHost(typeof(pflag_package.ipNetSliceValue)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pflagᴛipNetSliceValue;
[GoMethodHost(typeof(pflag_package.ipNetValue)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pflagᴛipNetValue;
[GoMethodHost(typeof(pflag_package.ipSliceValue)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pflagᴛipSliceValue;
[GoMethodHost(typeof(pflag_package.ipValue)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pflagᴛipValue;
[GoMethodHost(typeof(pflag_package.stringArrayValue)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pflagᴛstringArrayValue;
[GoMethodHost(typeof(pflag_package.stringSliceValue)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pflagᴛstringSliceValue;
[GoMethodHost(typeof(pflag_package.stringToInt64Value)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pflagᴛstringToInt64Value;
[GoMethodHost(typeof(pflag_package.stringToIntValue)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pflagᴛstringToIntValue;
[GoMethodHost(typeof(pflag_package.stringToStringValue)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pflagᴛstringToStringValue;
[GoMethodHost(typeof(pflag_package.stringValue)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pflagᴛstringValue;
[GoMethodHost(typeof(pflag_package.textValue)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pflagᴛtextValue;
[GoMethodHost(typeof(pflag_package.timeValue)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pflagᴛtimeValue;
[GoMethodHost(typeof(pflag_package.uint16Value)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pflagᴛuint16Value;
[GoMethodHost(typeof(pflag_package.uint32Value)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pflagᴛuint32Value;
[GoMethodHost(typeof(pflag_package.uint64Value)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pflagᴛuint64Value;
[GoMethodHost(typeof(pflag_package.uint8Value)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pflagᴛuint8Value;
[GoMethodHost(typeof(pflag_package.uintSliceValue)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pflagᴛuintSliceValue;
[GoMethodHost(typeof(pflag_package.uintValue)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pflagᴛuintValue;
// </MethodHolders>
