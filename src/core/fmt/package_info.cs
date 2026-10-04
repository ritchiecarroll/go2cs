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

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("fmt/errors.go", "errors.cs", "AAssAAwCgoKCkoKUtIKCtIKUgoKClICCtrSCAAcQgqaCAAcQgqaC")]
[assembly: go.GoPositionMap("fmt/format.go", "format.cs", "AC1sgoKCpqKCqJKSlIKClIKCppSCpoKClKiSgoKUgpSCpoK6koKClIKUgqaC+pKClLqSuoKClIKCupaCgoKCgoKCgqaCgoKClIKClIKCgqaCgoKEgoKCqJKCgpamlIKU3oKClIKCgoKCpJKCgt6mlIKCgoLGgoKCxoKCgsaCgoLGtIKCgoKogpaCgoK0koLIgoKC1oKCgoKWgoKSgoKSgoK6goKCqJKCgoKCgriokoKCgoKClIKClKaokoKokoKokoKUpoKmgoKUgqaSlLaClKaCpoKUlIKClIKUpoKUppSUgrqSqJKssoKCgpSCgpS8xoKClIKqooKClIKClLy0gqaCgpS4griCgpSClIKCuIKClISSzKKEgpSCpKSCtIKCgpS0gpaSyJSClJSCgpSmuIKCgoKUgqY=")]
[assembly: go.GoPositionMap("fmt/print.go", "print.cs", "AFeiAeKCgpKSpoCCpICCgqSCzIKmgqaCpoIAGTzKkoKCgoKCqAAKEIKUlIKWgoKCpoCkgKSClKSkpKSkqqKCqqKCrgAIAoKCgoKqwqiygoKCgqrCgoKCggACEAAJAoKCgoKs0qrCgoKCgqrCgoKCggACFAALAoKCgoKs0qrCgoKCgqzSgoKCgqyygoKUqqKCqNKClIKClIKUpoKCgpSCgqaigoKCgpSCgrSCgrS0gqailLTcooKCgqiylIKUxrS0tLS0tLS03MKUtLS0tN72lIKChIKSgrTYopSClMa0tLS02KKUgoKCgpSCgoKUlJSCgoKUlMa0tLS02KKClKSCppSCgoKCgpSUlIKU2LS0AAgIwoC4gIKCyJSWlISCgoKCgoKCgoTEAAgE0oKUlIKCgqaogIKCgoK4goCCgpSC7JyUgoKCpoKCgtgACQaigoSClLS0upSCpIK4lLS0tLS0tLS0tLS0tLS0tLS0uKKCgqbGtt7UgoKCpoKEgLSClJS06KSkpKSkpKSkgoKCgpSUlIKCgoKUpoKClIKUtoKUgoKCgpSmgoCCgraUpIKCgoKUpraWgpKCgsqCgqaCxoKCgoKUgoKClJSUgoKClJS6ooCkgoLGpKTa0oKCgpSApIKCgraCgoIACAyCgoKmAAIS9IKogoKCgpSmrLKClIKCgpSCpoKCgqaCgoKmooKCgoKCgoKCgpSClJSoloKCgoKUtLS0tLiipIKmgoSCxIKCgpYACAqWgoKEgrqCgoKUlIKSuoKCkpSCgoKUgoKUgpSUgoKCuoKWgoKWkoKUhJSkpKSipoKEgoKkogAIDoKCgoKClIKUgoKmuKKCgpSClIK8woKClJQ=")]
[assembly: go.GoPositionMap("fmt/scan.go", "scan.cs", "ADN+4qrCAAIUAAkCyrKCgoKUrvKq0q7yrgAIAoKCgqrigoKCrgAIAoKCggAhRLKmwoKCloKCgoKkgpSmgoKUqsKCgoKUlKzSgoKUpoKCgoKmgqaCpuKCgIKAkpTYgpSCgvoADBqCgpSCooKUgqaokgANINKCgoKClIKClKrigoKCgpSCgpSSgpSClIKCgoKCgpSmgpKCpoLWgoKmgqbKwoKAgpSkgoKCgoKCgoKoxIKCpoKUgoLcsoKCgpSClIKClIKUgoIABBCygqaCgoKUgoKUlKaSlIKCgqaqooKClIKClJSClKiSgoKUppSAgqSqoqiSgoKmgtiSgoKCppSkpIKUpIKUpAAVHsKCgoKUgrSCtIK02JKCgoKmlKiSgoKCgoKUrLKClJSUgqSCpIKk/KKClIKCgoKCgqaCgqaCgoKUgoKClKqigpSCgoKCgoKkgpSCgoKUgoKClKyylIKmlIKUgoKCgqamlLiUlKas9IKClIKmgoKClIKUpoKCgqaoyICmgpSAgqSUgpSAgqSUpIKClN7CgpSCgoKCgurCgpSCgpS0tLTYkoKClpKCgpSUpoKSgoK4koKmgoKUpLSokoKUpKSk3OKCgpSCgoKUgoKClNiSgoKCgpSUgoKUAAoakoKCgvqygpSAgoKCgpSUppS0tLS0tLS0tLS0tLS0uKKCgsaCgoLGuMSCgqKClICkpKSkpoKSlIKCgraCgqSk/LKAgoCSkoCSlPbWAAgCgoKCpoKCgoKUgoK4AAsWAAoCggAGEIKCgoKCgpSUgpSCgoKUgqaCgqaClIKmgpSCpqiUgqaCgpSogoKClJTqAAkCgpSCgoKCppSCppSGkoKCloKEgpSCgpSCgIKmkoKUhIKClIKU", "248-256:1")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("fmt")]
public static partial class fmt_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
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
