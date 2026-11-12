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
using static go.fmt_package;

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
[assembly: GoImplement<os_package.File, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<pp, State>(Pointer = true)]
[assembly: GoImplement<readRune, io_package.RuneScanner>(Pointer = true)]
[assembly: GoImplement<ss, ScanState>(Pointer = true)]
[assembly: GoImplement<stringReader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<wrapError, error>(Pointer = true)]
[assembly: GoImplement<wrapErrors, error>(Pointer = true)]
// </InterfaceImplementations>

// An exported function recorded here is an sstring twin: a @string member and a prioritized
// sstring member, so it has no single method group. A func value names its canonical delegate
// `<Name>ᶠ` instead. Go spellings. The section exists only while there is a record to hold.
// <SStringTwins>
[assembly: GoSStringTwin("Appendf")]
[assembly: GoSStringTwin("Errorf")]
[assembly: GoSStringTwin("Fprintf")]
[assembly: GoSStringTwin("Fscanf")]
[assembly: GoSStringTwin("Printf")]
[assembly: GoSStringTwin("Sprintf")]
[assembly: GoSStringTwin("Sscanf")]
// </SStringTwins>

// <ImplicitConversions>
// </ImplicitConversions>

// This package's string literals are hoisted to `static readonly` fields (the standard
// library's form, or -hoist-literals), so no converted file passes a bare literal for
// go2cs-gen's literal interceptor to box. The stamp below tells the generator to skip
// the assembly.
[assembly: GoHoistedLiterals]

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: GoFieldAccess(typeof(pp))]
[assembly: GoFieldAccess(typeof(ss))]
[assembly: GoFieldAccess(typeof(strconv_package.NumError))]
[assembly: GoFieldAccess(typeof(wrapError))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("fmt/errors.go", "errors.cs", "AA4uAA8GgriosoKUgoKCgoKUtIKCtIKUgoKClIKmpIIACRCCpoIACxCCpoI=")]
[assembly: GoPositionMap("fmt/format.go", "format.cs", "ADJsgoKCpqKCqJKClIKClIKCppSCpoKClKiSgoKUgpSCpoK6koKClIKUgqaCAAkKkoKUupLKgoKUgoLKpoKCgoKCgoKCpoKCgoKUgoKUgoKCpoKCgpSCgoKokoKCpqaUgpTugoKUgoKCgoKUgoKC7qaUgoKCgsaCgoLGgoKCxoKCgsakgoKCgriCpoKCgrSCgtiCgoK2goKCgqaCgoKCgoKCgsqCgoKokoKCgoKCuKiSgoKCgoKUgqaokoKokoKokoKUpoKmgoKUgqaClKaClKaCpoKUlIKClIKUpoKUppSUgrqSqJKssoKCgpSCgpS8xoKClIKqooKClIKClLzUgqaCgpS4griCgpSClIKCuIKClJSCzKKUgpSCpKSCtIKCgpS0gqaCuJSClJSCgpSmuIKCgoKUgqY=")]
[assembly: GoPositionMap("fmt/print.go", "print.cs", "AFuiAQAIAoKCgoKmgIKkgIKCpIL8gqaCpoKmggAbPMqSgoKCgoLIAAoQgpSUgqaCgoKmlJSClKSkpKSUqqKCqqKCzgAIAoKCgoKqwqiygoKCgqrCgoKCggACEAAJAoKCgoKs0qrCgoKCgqzSgoKCggACFAALAoKCgoKs0qrCgoKCgqzSgoKCgqyygoKUqqKCqNKClIKClIKUxoKCgpSCgqaigoKCgpSCgrSCgrSkgqailLTMooKCgqiylIKUxrS0tLS0tLS0zMKUtLS0tM72lIKClIKCgrTIopSClMa0tLS0yKKUgoKCgpSCgoKUlJSCgoKUlMa0tLS0yKKClLSCppSCgoKCgpSUlIKU2LS0AAgI0riCgriUppSUgoKCgoKCgoKUtAAMBNKClJSCgoKmuICCgoKCyIKAgoKUguzclIKCgqaCgoLIAAoGooKUgpS0pMqUgqSCuJS0tLS0tLS0tLS0tLS0tLS0tNiCgoKm1qbe1IKCgqaClLSClJS02KSkpKSkpKSkgoKCgpSUlIKCgoKUpoKClIKUtoKUgoKCgpSmgoKCppSkgoKCgpSmtqaCgoKCyoKCpoK2goKCgpSCgoKUlJSCgoKUlNqCpIKCpqSk2tKCgoKUpIKCgsaCgoIACAyCgoKmAAIS9IK4goKCgpSmzLKClIKCgpSCpoKCgqaCgoKmooKCgoKCgoKCgpSClJS4poKCgoKUtLS0tNiCpIK2gpSClIKCgqb6poKClILKgoKClJSCgsqCgoKUgoKClIKClIKUlIKCgsqCpoKCpoKUlKSkpIK2gpSCgqSCAAgOgoKCgoKUgpSCgqa4ooKClIKUgrzCgoKUlA==")]
[assembly: GoPositionMap("fmt/scan.go", "scan.cs", "AD9+4qrCAAIUAAkC6rKCgoKUzvKq0q7yrgAIAoKCgqrigoKCrgAIAoKCggAkRLKmwoKCpoKCgoKUgpSmgoKUqsKCgoKUlKzSgoKUpoKCgoKmgqaCpuKCgpKUuIKUgoIACQoADBqCgpSCooKUgqaokgAPINKCgoKClIKClKrigoKCgpSCgpSCgpSClIKCgoKCgpSmgoKCpoLmgoKmgsbKwoKClJSCgoKCgoKCgsjEgoKmgpSCguyygoKClIKUgoKUgpSCggAEELKCpoKCgpSCgpSUxoKUgoKCpsqigoKUgoKUlIKUqJKCgpSmlIKUqqKokoKCpoLokoKCgqaUpKSClKSClJQAHB7CgoKClIK0grSCpOiSgoKCppSokoKCgoKClKyygpSUlIKkgqSCpPyigpSCgoKCgoKmgoKmgoKClIKCgpSqooKUgoKCgoKClIKUgoKClIKCgpSsspSCppSClIKCgoKmppS4lJSmrPSCgpSCpoKCgpSClMaCgoKmyMimgpSClJSClIKUlJSCgpTuwoKUgoKCgoIACArCgpSCgpS0tKTokoKCpoKCgpSUtoKCgoK4goKmgoKUpKTIkoKUpKSUAAgM4oKClIKCgpSCgoKU6JKCgoKClJSCgpQAEhqSgoKCAAkKsoKUgIKCgoKUlLaUtLS0tLS0tLS0tLS0tNiCgoLGgoKCxtjEgoKCgpSktLS0xoKClIKCgsaCgrS0AAgM0oKCgoKUxgAIBgAIAoKCgqaCgoKClIKCuAAPFgAKAoIABxCCgoKCgoKUlIKUgoKClIKmgoKmgpSCpoKUgqa4lIKmgoKUuIKCgpSUAAgKAAkCgpSCgoKCppSCppSWkoKCpoKUgpSCgpSCgqaCgpSUgoKUgpQ=", "248-256:1", "", "", "1351:21,1490:21", "307:21,308:13")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("fmt")]
public static partial class fmt_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct buffer {}
    [GoValueClone("intbuf")] internal partial struct fmt {}
    internal partial struct fmtFlags {}
    [GoValueClone("fmt")] internal partial struct pp {}
    [GoValueClone("buf", "pendBuf")] internal partial struct readRune {}
    internal partial struct scanError {}
    internal partial struct ss {}
    internal partial struct ssave {}
    internal partial struct stringReader {}
    internal partial struct wrapError {}
    internal partial struct wrapErrors {}
    public partial interface Formatter {}
    public partial interface GoStringer {}
    public partial interface ScanState {}
    public partial interface Scanner {}
    public partial interface State {}
    public partial interface Stringer {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸfmtsort() => builtin.initPackage(typeof(@internal.fmtsort_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸstringslite() => builtin.initPackage(typeof(@internal.stringslite_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(unicode.utf8_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(fmt_package.buffer)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class fmtᴛbuffer;
[GoMethodHost(typeof(fmt_package.fmt)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class fmtᴛfmt;
[GoMethodHost(typeof(fmt_package.pp)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class fmtᴛpp;
[GoMethodHost(typeof(fmt_package.readRune)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class fmtᴛreadRune;
[GoMethodHost(typeof(fmt_package.ss)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class fmtᴛss;
[GoMethodHost(typeof(fmt_package.stringReader)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class fmtᴛstringReader;
[GoMethodHost(typeof(fmt_package.wrapError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class fmtᴛwrapError;
[GoMethodHost(typeof(fmt_package.wrapErrors)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class fmtᴛwrapErrors;
// </MethodHolders>
