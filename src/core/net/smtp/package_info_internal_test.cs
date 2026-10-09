// go2cs metadata anchor for the INTERNAL (white-box bridge) test class: GoImplement /
// GoImplicitConv attributes whose GENERATED code must merge with a bridge-declared type
// anchor here — the source generators host output in the first class of the
// attribute-bearing file, and only this file's first class is the bridge. Records for
// production and external-test types stay in package_test_info.cs.

// <ImportedTypeAliases>
// </ImportedTypeAliases>

using go;
using static go.net.smtp_package;
using static go.net.smtp_internal_test_package;

// <ExportedTypeAliases>
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<TestClientAuthTrimSpace_fake, io_package.ReadWriter>]
[assembly: GoImplement<TestClientAuthTrimSpace_fake, io_package.Reader>(Promoted = true)]
[assembly: GoImplement<TestClientAuthTrimSpace_fake, io_package.Writer>(Promoted = true)]
[assembly: GoImplement<faker, io_package.ReadWriteCloser>]
[assembly: GoImplement<faker, io_package.ReadWriter>(Promoted = true)]
[assembly: GoImplement<faker, net_package.Conn>]
[assembly: GoImplement<toServerEmptyAuth, global::go.net.smtp_package.ΔAuth>]
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
[assembly: go.GoPositionMap("net/smtp/smtp_test.go", "smtp_test.cs", "ACI83IKCgoKClIKUgpSCgoKCgoKUgoIAEgykABtAgoKCgoKUggANDJKEgoK+goKUgoKCgoCSAA4UgqaC7oCigKKAooCigKKAABkEooKEgoKCgoSAgqSAgqSAgqaCgIKkgIKmgIKmgIKkgIKkgIK4goKAgqaAgqSAgqSAgqSAgqSOgoKUgIKkgIKmgIKmgoKCAD1UgoiKgoKCgoKChICCpICCpIKCggBjCKKyhIKCgoKElpIAARiEgIKkgoCCpICCpoKCgoKokgABGoSAgqSAgqSAgqSAgqSAgqaCgoKCqJIAARyEgIKkgIKkgIKkgIKkgIKmgoKCgqiSAAEchICCpICCpICCpICCpICCpoKCgoKokgABHoSAgqSCgIKkgIKkgIKkgIKmgoKCgsrCgoSCgpKClIKCgoKUkoCCpICCpICCpoKCtAASHMKChIKCgoKCgpSSgIKkgIKmgoKCtAAUIMKCgpaUgoKCgqiygoKClJSCgoK0lIKCgpSUgoKUgrQACASkgpaCgoKCgoKCgoKUkoKElIKClLSCgsa0goK0tIKCxrS0goKCgti0toKWgoKCxgAKEAASIgAWGqKCgoKCgoKUlpLUhIKCgpSUgoKCgoKUgpSCgoKCgoKClILYlIyClo6CloKCgoK0ACY0woKClJSCwoKCgoKUlIKCgoKClIKCgpSCgoK0lIyClIKUgoK05KKCgoKCgoKCgpSUgoKEgpKCloKCgrQAFCCigpSCkoKilIKClJKAgqSAgsSkooKSgoLCgoKCgpSSgILEksKCgoKClJKCgoCCgqSSgoKUgrSSgqLUgoKClIKU7oIACAiykoKCgpSCgqSCgoKUgoKCpLYACQaCkoKClKSkpIIACgyCpLamgoKCkuiCgoLuABIiABIigA==", "323-333:1;335-367:2;369-407:3;409-448:4;450-489:5;491-532:6;541-544:1;637-650:1;793-828:1;891-917:1;988-990:1;1009-1020:1;1021-1043:2;1126-1128:1", "1069=net/smtp.smtpSender.send;1096=net/smtp.smtpSender.send", "35=PlainAuth/1/2/1,PlainAuth/2/2/2,CRAMMD5Auth/1/1/3;141=NewReader/1/1/4")]
// </GoSourcePositionMaps>

namespace go.net;

[GoPackage("smtp")]
public static partial class smtp_internal_test_package
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
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸtls() => builtin.initPackage(typeof(crypto.tls_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸx509() => builtin.initPackage(typeof(crypto.x509_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtestenv() => builtin.initPackage(typeof(@internal.testenv_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸlog() => builtin.initPackage(typeof(log_package));
    [GoInit] internal static void initᴛᴛimportꓸnet() => builtin.initPackage(typeof(net_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸsmtp() => builtin.initPackage(typeof(go.net.smtp_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸtextproto() => builtin.initPackage(typeof(go.net.textproto_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
}
