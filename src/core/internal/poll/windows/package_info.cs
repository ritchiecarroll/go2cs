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
global using syscallꓸHandle = go.syscall_package.ΔHandle;
global using syscallꓸSignal = go.syscall_package.ΔSignal;
global using syscallꓸSockaddr = go.syscall_package.ΔSockaddr;
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
// </ImportedTypeAliases>

using go;
using static go.@internal.poll_package;

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
[assembly: GoImplement<DeadlineExceededError, error>(Pointer = true)]
[assembly: GoImplement<errNetClosing, error>]
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
[assembly: go.GoPositionMap("internal/poll/errno_windows.go", "errno_windows.cs", "AA4oopSk2g==")]
[assembly: go.GoPositionMap("internal/poll/fd.go", "fd.cs", "ABcwwKSAooDIuLiWkoKUygAIEsCigKKAyJaSgoKCgpSCgso=")]
[assembly: go.GoPositionMap("internal/poll/fd_fsync_windows.go", "fd_fsync_windows.cs", "AAkU4oCCpII=")]
[assembly: go.GoPositionMap("internal/poll/fd_mutex.go", "fd_mutex.cs", "AC5qAA4CgoKClIKClIIABkyigoKClIKCAAisAaKYwoKUrLKClKrCgpSs0oK8woKUrNKC")]
[assembly: go.GoPositionMap("internal/poll/fd_poll_runtime.go", "fd_poll_runtime.cs", "AA8oxJKSkpKSkpKSAAgQooKCgpSCpoKClIKokoKUpoKClIKmgqaC1oKClIKmgqaCpoKClKaCAAwYgpSkpKSkgqiSqJKokqbCgoKCgqaAgqSCgpSCAAUeAAwC")]
[assembly: go.GoPositionMap("internal/poll/fd_posix.go", "fd_posix.cs", "AAsgooKUqOKAgqSC2MKAgqSC3MKAgqSC3vKAgqSCggAFFPKCgoLMkoKCgg==", "38-40:1;49-51:1")]
[assembly: go.GoPositionMap("internal/poll/fd_windows.go", "fd_windows.cs", "ACBIwoKClJKCkoKClIKCptyCgoKClAAXNIKCgoK4ooKUlKKCgpSCgpSCzqKClKaigoKEgoSCgoKCAAYQ4oKWlIKCpoKWpLqk1oKClJSClJSmgrjGlJSmgoKCkpTKADOGAfKClpSkpKqkpISCAAsYlIKUgpSUgpakgoLKlJKSgoKCtoKCgoKCgqaigriCgpaktIKCqsKClIKmgqaCAAcS8oCCpISCloKCgoKClKSCuLaCpoKChoKmgpTWmrKClriCloKCgpSCgoKUgoKCgoKCgpSCgpSUgoK4lIKCgqiCgoKCkoKUlJSCqNKUuICCpISCloKCgoKUgsiCgoKCgqaClNjigpSClICCpIKCgoKClIKUgoKUgtjigpSClICCpIKCgoKClIKUgoKUgtjigpSClICCpIKCgoKClIKUgoKUgtjygIKkgoKCloKCgoKUgoKClKSCuLaCpoKUgoKYgoKUlNqigoKCgpaCgoKUgoKasoKCgpSCgoKCgoKClKao0pS4gIKkhIKCgoKUhIKCgoKUgsiCgoKUgpTY8oKUgIKkgoKUgoKGgoKC2OKAgqSElIKCgoaWgoKCgpSCgoKGgoKUlNjigIKkhJSCgoaWgoKCgpSCgoaCgpSU2OKAgqSElIKChpaCgoKClIKChoKClJTc0oKChubEgoKWgoKogoKClqrSgIKkhIKCgoKCloKCAAYQgoKUuAAIDAAJAoKUgIKkhIKE2OKAgqSEgoCCpIKClJSCloKC2OKAgqSC2OKAgqSC2OKAgqSC2OKAgqSCgoK6goKClIa2/OKAgqSEgqgADVKClIKkgrS64oCCpISCloKCgpSCgoKGgoKClNjigIKkhIKWgoKClIKCgoaCgpTY4oCCpISCloKCgpSCgoKGgoKUAAgI4oKWgIKkhIKCgoKUgoKUgpSG2OKCloCCpISCgoKUgoKChtjigpaAgqSEgoKClIKCgoYACQaCgoKWgoKAgqQ=", "438-440:1;581-587:1;610-616:1;639-645:1;695-697:1;808-810:1;829-831:1;844-846:2;867-869:1;881-883:2;904-906:1;918-920:2;936-938:1;946-948:1;1090-1092:1;1186-1188:1;1216-1218:1;1245-1247:1;1279-1281:1;1304-1306:1;1329-1331:1")]
[assembly: go.GoPositionMap("internal/poll/hook_windows.go", "hook_windows.cs", "AAoUpqY=")]
[assembly: go.GoPositionMap("internal/poll/sendfile.go", "sendfile.cs", "/g==")]
[assembly: go.GoPositionMap("internal/poll/sendfile_windows.go", "sendfile_windows.cs", "AAoaAAgCgpSUlICCpoCCpISCloKClqSCgqaAgr60goKCloKChIaClrqAgqaClg==", "14-16:1;66-68:2")]
[assembly: go.GoPositionMap("internal/poll/sockopt.go", "sockopt.cs", "AAoY4oCCpILY8oCCpILY4oCCpILY4oCCpII=")]
[assembly: go.GoPositionMap("internal/poll/sockopt_windows.go", "sockopt_windows.cs", "AAkU4oCCpII=")]
[assembly: go.GoPositionMap("internal/poll/sockoptip.go", "sockoptip.cs", "AAoY4oCCpILY4oCCpII=")]
// </GoSourcePositionMaps>

namespace go.@internal;

[GoPackage("poll")]
public static partial class poll_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct fdMutex {}
    internal partial struct fileKind {}
    internal partial struct operation {}
    internal partial struct pollDesc {}
    public partial struct DeadlineExceededError {}
    public partial struct FD {}
    public partial struct String {}
    public partial struct errNetClosing {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸrace() => builtin.initPackage(typeof(go.@internal.race_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸsyscallꓸwindows() => builtin.initPackage(typeof(go.@internal.syscall.windows_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(go.sync_package));
    [GoInit] internal static void initᴛᴛimportꓸsyncꓸatomic() => builtin.initPackage(typeof(go.sync.atomic_package));
    [GoInit] internal static void initᴛᴛimportꓸsyscall() => builtin.initPackage(typeof(syscall_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(unicode.utf8_package));
    // </ImportInitializers>
}
