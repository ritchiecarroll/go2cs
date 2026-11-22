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
using time = go.time_package;
// </ImportedTypeAliases>

using go;
using static go.net.mail_package;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

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
[assembly: GoImplement<bufio_package.Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<charsetError, error>]
// </InterfaceImplementations>

// <ImplicitConversions>
// </ImplicitConversions>

// This package's string literals are hoisted to `static readonly` fields (the standard
// library's form, or -hoist-literals), so no converted file passes a bare literal for
// go2cs-gen's literal interceptor to box. The stamp below tells the generator to skip
// the assembly.
[assembly: GoHoistedLiterals]

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: GoFieldAccess(typeof(go.net.textproto_package.Reader))]
[assembly: GoFieldAccess(typeof(mime_package.WordDecoder))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("net/mail/message.go", "message.cs", "AClOooIACRrSlIKCpgAFGgAJAqaAgoKClLaCgoK4goKUpoK4lJSC7qaCgoKUlIKUgoKCgoKC7gAKCKSCgqaCuIKClILKgoK4poKmgpSCgoKmAAUY4qbWkoKClKiSgoKUAAoYkqiSAAgWoqqi7MSCkqaUuIKCgpS4gqaClIKolIK4gqaCgqaC3IKUAAsQooKCpoKmgoKUlIKUgpSCuIKUgqYACAaigoKUgpSClIKUgpQADAiygoKCAAkUgoKCgoKCgrjKgpaSgoKCppSCgoK4goKCgoKmpsqUgoKUgpSU3KKUgoKClKaClIKClJSClIKClJSCpgAMCPKUgpKCupKCgpSUgoKCuIKUgoKmgqiSgoKmlIKCuIKCuOjShKiUgoKokoKClIKUyoKCuIKUgpS0goK0yIK4goKUgujEgpSUgoKUlKbKgqaC2ra2tqi0ggAMDuKUgoKClKa26oKUgoKClIKUgqYACAikgriCgoKClIKmgoKUgqaClKaCuIKm5oKClIKCuIKCgoKUgrimgoKUgqiSpoKmgqaCqJKUgoKmgqamppSUgoKCpoKCgoKClIKUpqaygoIACBKCgpKCgpSCgpSUgoIABxCCuKYABhCCqqKUuJSopIKUqJKCgoKCgoKCpoKopKqiqqKopIKU", "517-521:1;784-784:1;882-892:1", "", "", "", "589:21,590:13")]
// </GoSourcePositionMaps>

namespace go.net;

using static go.net.mail_package;
using time = go.time_package;
using ꓸꓸꓸany = Span<any>;

[GoPackage("mail")]
public static partial class mail_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct addrParser {}
    internal partial struct charsetError {}
    internal partial struct debugT {}
    public partial struct Address {}
    public partial struct AddressParser {}
    public partial struct Header {}
    public partial struct Message {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbufio() => builtin.initPackage(typeof(bufio_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸlog() => builtin.initPackage(typeof(log_package));
    [GoInit] internal static void initᴛᴛimportꓸmime() => builtin.initPackage(typeof(mime_package));
    [GoInit] internal static void initᴛᴛimportꓸnet() => builtin.initPackage(typeof(net_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸtextproto() => builtin.initPackage(typeof(go.net.textproto_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(unicode.utf8_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(mail_package.Address)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mailᴛAddress;
[GoMethodHost(typeof(mail_package.AddressParser)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mailᴛAddressParser;
[GoMethodHost(typeof(mail_package.Header)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mailᴛHeader;
[GoMethodHost(typeof(mail_package.addrParser)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mailᴛaddrParser;
[GoMethodHost(typeof(mail_package.charsetError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mailᴛcharsetError;
[GoMethodHost(typeof(mail_package.debugT)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mailᴛdebugT;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class mailᴛAddress {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ref Address a) => mail_package.String⁔(ref a);
}
partial class mailᴛAddressParser {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<Address>, error) Parse(this ref AddressParser p, @string address) => mail_package.Parse⁔(ref p, address);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<ж<Address>>, error) ParseList(this ref AddressParser p, @string list) => mail_package.ParseList⁔(ref p, list);
}
partial class mailᴛHeader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Get(this Header h, @string key) => mail_package.Get⁔(h, key);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (time.Time, error) Date(this Header h) => mail_package.Date⁔(h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<ж<Address>>, error) AddressList(this Header h, @string key) => mail_package.AddressList⁔(h, key);
}
partial class mailᴛaddrParser {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<ж<Address>>, error) parseAddressList(this ж<addrParser> Ꮡp) => mail_package.parseAddressList⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<Address>, error) parseSingleAddress(this ж<addrParser> Ꮡp) => mail_package.parseSingleAddress⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<ж<Address>>, error) parseAddress(this ж<addrParser> Ꮡp, bool handleGroup) => mail_package.parseAddress⁔(Ꮡp, handleGroup);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<ж<Address>>, error) consumeGroupList(this ж<addrParser> Ꮡp) => mail_package.consumeGroupList⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (@string spec, error err) consumeAddrSpec(this ж<addrParser> Ꮡp) => mail_package.consumeAddrSpec⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (@string phrase, error err) consumePhrase(this ref addrParser p) => mail_package.consumePhrase⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (@string qs, error err) consumeQuotedString(this ref addrParser p) => mail_package.consumeQuotedString⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (@string atom, error err) consumeAtom(this ref addrParser p, bool dot, bool permissive) => mail_package.consumeAtom⁔(ref p, dot, permissive);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (@string, error) consumeDomainLiteral(this ref addrParser p) => mail_package.consumeDomainLiteral⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (@string, error) consumeDisplayNameComment(this ref addrParser p) => mail_package.consumeDisplayNameComment⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool consume(this ref addrParser p, byte c) => mail_package.consume⁔(ref p, c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void skipSpace(this ref addrParser p) => mail_package.skipSpace⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static byte peek(this ref addrParser p) => mail_package.peek⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool empty(this ref addrParser p) => mail_package.empty⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint len(this ref addrParser p) => mail_package.len⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool skipCFWS(this ref addrParser p) => mail_package.skipCFWS⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (@string, bool) consumeComment(this ref addrParser p) => mail_package.consumeComment⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (@string word, bool isEncoded, error err) decodeRFC2047Word(this ref addrParser p, @string s) => mail_package.decodeRFC2047Word⁔(ref p, s);
}
partial class mailᴛcharsetError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Error(this charsetError e) => mail_package.Error⁔(e);
}
partial class mailᴛdebugT {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void Printf(this debugT d, @string format, params ꓸꓸꓸany argsʗp) => mail_package.Printf⁔(d, format, argsʗp);
}
// </MethodForwarders>
