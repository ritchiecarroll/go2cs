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
// </ImportedTypeAliases>

using go;
using static go.os_package;

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
[assembly: GoDynamicTypeLift("7374727563747b73796e632e4d757465783b2064697220737472696e677d", "getwdCacheᴛ1")]
[assembly: GoTypeAlias("DirEntry", "go.io.fs_package.DirEntry")]
[assembly: GoTypeAlias("FileInfo", "go.io.fs_package.FileInfo")]
[assembly: GoTypeAlias("FileMode", "go.io.fs_package.FileMode")]
[assembly: GoTypeAlias("Kill", "const:ΔKill")]
[assembly: GoTypeAlias("PathError", "go.io.fs_package.PathError")]
[assembly: GoTypeAlias("Signal", "ΔSignal")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<File, go.io.fs_package.File>(Pointer = true)]
[assembly: GoImplement<File, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<LinkError, error>(Pointer = true)]
[assembly: GoImplement<SyscallError, error>(Pointer = true)]
[assembly: GoImplement<dirEntry, go.io.fs_package.DirEntry>]
[assembly: GoImplement<dirFS, go.io.fs_package.ReadDirFS>]
[assembly: GoImplement<dirFS, go.io.fs_package.ReadFileFS>]
[assembly: GoImplement<dirFS, go.io.fs_package.ReadLinkFS>]
[assembly: GoImplement<dirFS, go.io.fs_package.StatFS>]
[assembly: GoImplement<errSymlink, error>]
[assembly: GoImplement<fileStat, go.io.fs_package.FileInfo>(Pointer = true)]
[assembly: GoImplement<fileWithoutReadFrom, io_package.Writer>]
[assembly: GoImplement<fileWithoutWriteTo, io_package.Reader>]
[assembly: GoImplement<go.io.fs_package.File, io_package.Reader>]
[assembly: GoImplement<rawConn, syscall_package.RawConn>(Pointer = true)]
[assembly: GoImplement<rootFS, go.io.fs_package.FS>(Pointer = true)]
[assembly: GoImplement<syscall_package.ΔSignal, ΔSignal>]
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
[assembly: GoFieldAccess(typeof(@file))]
[assembly: GoFieldAccess(typeof(File))]
[assembly: GoFieldAccess(typeof(LinkError))]
[assembly: GoFieldAccess(typeof(PathError))]
[assembly: GoFieldAccess(typeof(Process))]
[assembly: GoFieldAccess(typeof(Root))]
[assembly: GoFieldAccess(typeof(SyscallError))]
[assembly: GoFieldAccess(typeof(dirInfo))]
[assembly: GoFieldAccess(typeof(fileStat))]
[assembly: GoFieldAccess(typeof(go.@internal.syscall.windows_package.OBJECT_ATTRIBUTES))]
[assembly: GoFieldAccess(typeof(io_package.LimitedReader))]
[assembly: GoFieldAccess(typeof(processHandle))]
[assembly: GoFieldAccess(typeof(root))]
[assembly: GoFieldAccess(typeof(syscall_package.ProcAttr))]
[assembly: GoFieldAccess(typeof(syscall_package.Rusage))]
[assembly: GoFieldAccess(typeof(syscall_package.Win32FileAttributeData))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("os/dir.go", "dir.cs", "ABpQABECgpSCuJQAAiQAEgKClIK4lAACIgAKAoKUgpSUAAIQ8oKClKSChgAFKAARAqKCpoKClJSUpIKClKSCgpSSgoKUgoKmgIKCpKQ=", "122-124:1;146-189:1", "", "", "125:21,183:29")]
[assembly: GoPositionMap("os/dir_windows.go", "dir_windows.cs", "ACdSpoLIgoKCggAJEqKCAAMQwoKCppSUlKbKAAmGAoCigKKAooCkgg==")]
[assembly: GoPositionMap("os/env.go", "env.cs", "AAwgwpSCgoKClIKCpqaUlIKmgpSssqqilJSokqyylIKmgoKClKaklpKUrLKCggACENKC6qKCgpSokqiSqqI=")]
[assembly: GoPositionMap("os/error.go", "error.cs", "ABYmlIKCgpSCloAAAhLwAAwegKSAppKCrLKClAACEuIAAhLiAAIS4gACFgAIAoKmuIKCpoKokpSkpJQ=")]
[assembly: GoPositionMap("os/exec.go", "exec.cs", "ABImlJQANYQB4oKCgpSClILMsoKCgpSCgpTMkqaoktyUypSssqaCqsKCpoKClIKC7oKClKiSgpSosoKmACROkKaQAAIWAAkCAAIgAA0CgqwACQqCpriCgris0oKCgoKmgtzclKa+sgACENKqogACFgAIAqiSqJKssqqirLIAAhDS")]
[assembly: GoPositionMap("os/exec_posix.go", "exec_posix.cs", "ABw06IKAgoKCyILKgoKCpoKCpqaUgriCgoKCuKaCptKClIKUpJSClAAMFpKmgqaCpoKmggAIBqKClIKClIKClMa0goLGpIKUqtSClA==")]
[assembly: GoPositionMap("os/exec_windows.go", "exec_windows.cs", "ABgkAAgCgpSklJSClLSklIKCgpSCgoLKlAAMBqKClKSUlIKCgoKUgoKCptaC5oKUgoKUpoKCgoKUupKClKqigoKCgoKUgsaCuIKClJSUgrSCpIKClKzSgoKCgpSCgpS2ooKmgqaC")]
[assembly: GoPositionMap("os/executable.go", "executable.cs", "AAckAAsC")]
[assembly: GoPositionMap("os/executable_windows.go", "executable_windows.cs", "AAsYgoKCgoKCgpSCppSmgg==")]
[assembly: GoPositionMap("os/file.go", "file.cs", "AD5+sAAJEpKSAB5IgqaCAAIkAA8C7OKAgqSC7gAIAoCCtoKmgoKCgpSCgpTowoCCpIKClAAHEqIAChaCrPKAgqSCgpSCppSCpqaeAAkCgIKkgqaCpoKCgoKUgoKUqMKAgqSCgpQABxKiAAoWggAGEPKAgqSCgpSClKqigqyygpiCuIKUgoK4qJKCgpSqooCCgqSCgoKClJSUgIKCgrauwgACEuIAAhTygoKClJSmmrKCAAIS4gACENKqooKUAAYUsoKUgoKClAACGAAJAgAWIAANApSkgoLIgoKUtoKClLaCgoKClIKCyAAOIAANApSkgoLIgoKUtoKClLaCgoKClIKCyAAOFgAIApKktKSAgrakpJQAAioAEgCooAACNAAYAq7CAAIS4uqigIKkAAMoABECAAIoABECAAwUgoKClILKgpSuwoKClIKCgJSklKqigoKUgoKAlKSUpoKCgpSClIKUpoKCgpSClIKUpoKCgpTokoKUgoKUgpSu4oKClKTWgoCCpAACENKasoKUlLiCpoKCgoKCgpTcgoIABBTSgoKUgoCCpA==", "329-331:1", "", "", "1016:21")]
[assembly: GoPositionMap("os/file_posix.go", "file_posix.cs", "ABAo4oKUqtKCgqzigoKq0oKCqtKCgqiygoKUgpSCpqiSgoaClOiygIKkgIKkAAIU8oaClAACEuKGgpQABhDygIKkgIKk7NKAgqSAgqTs0oCCpICCpAACFPKCgIKkpoKCgoKUpoKCrNKAgqSAgqSAgoKCtuiSgIKk6JKAgqTokoCCpKqigpQAAhTygoKCzJKCgoI=", "79-81:1;107-109:1;123-125:1;190-196:1")]
[assembly: GoPositionMap("os/file_windows.go", "file_windows.cs", "ACVGsoLcggAcMLKClIKCgoKUlIIACBKCxrS0tOwACRK4gqiSqJKCgpQAAhDSgpSmAAcQkoKUgoKClIKmgqaigpSAgqSCgIKClMiCrvKApqSCgqrCgoKUkoKClNqigoLKgoKUgoK4goKClIKCgoCCgIL8poKCgpSssoKCgqammIKCgpSCgoKCgpSUlJS8ooKClIKClIKClK7UpoKAgpSApsjIgpSCgpSCggAHEJSCpoKClIKmgoKCpqyygoKUpoKCgpSmAAEUAAoClKaClKS4gpSUgoKUlIKCgoKUgpSUgoKClJSUAAlMgoKClA==", "", "", "", "255:21")]
[assembly: GoPositionMap("os/getwd.go", "getwd.cs", "ABg0AAsCyoKqooKCgoKUgoIABxCCyoLKgoKCuIKCgoKCgtyClJSC3IKCgpSCgqaCgoLKgpSUgoKCgsqCgoKClIKmuIKClA==")]
[assembly: GoPositionMap("os/path.go", "path.cs", "AA0mAAgEgoKClAAHEIKClIKUgsqAgoKC2oKmgoKUlAACENKokoKUgpQ=")]
[assembly: GoPositionMap("os/path_windows.go", "path_windows.cs", "ABAmpKiSgsqCgqaCprimgpSmgpSCgriClIKCpqaCgoKClIKCgpSClAACGAAJAoKUqJKClJSUAA4eggAHEIKUlIKmpqaSgpSmpoKUgrimgoLKgoKCgoKClIKCppSUgg==")]
[assembly: GoPositionMap("os/pidfd_other.go", "pidfd_other.cs", "AAoWgqaCpoKmgqaC")]
[assembly: GoPositionMap("os/proc.go", "proc.cs", "ABEkgpSUprrQqrCqsKqw7MKCAAIQ0rgABxCUpg==")]
[assembly: GoPositionMap("os/rawconn.go", "rawconn.cs", "ABQkgoCCpIKC5oKAgqSCguaCgIKkgoKmgg==")]
[assembly: GoPositionMap("os/removeall_at.go", "removeall_at.cs", "AA0eoqbKgriCgsqUgsqUgpSUgpSkgIKAgoKkpNaUgoIACBKClJaSgoKmgoKClJSUgJSkgqaClJSCgoKUpoKCgoKAgqSCgu6CAAgSpoLKgoKmgpQAAhYACAKCgpQ=", "", "", "", "56:21")]
[assembly: GoPositionMap("os/removeall_windows.go", "removeall_windows.cs", "AAoWgg==")]
[assembly: GoPositionMap("os/root.go", "root.cs", "ABIyAAkCgoKUkgAwZrKCrLKqoqqiqqIABhDygpSCgoKUgqrCgqqiAAIS4oKUAAIQ0oKUqqKqoqqiqqKqoqrCgq7igqqirLIAAhYACAIAAhYACAKqwoKClJLaooKClIKAgqSmgoCmyIKApgAIGgAMAoKUgqaUgoKUgqaCkoKUgpSUgoKUpoKUlJSUlJSUgq7CyoKCgpSCgpSmooKCAAgSgoKUkoKG1qKCgpSCgpSS1oKCgpSmgoKClKaCgoKUqJKClMqCpg==", "395-397:1", "", "", "34:21,278:21,433:21,454:21")]
[assembly: GoPositionMap("os/root_openat.go", "root_openat.cs", "AB1A0oKCgpSCgtbSgoKClILW0oKCgpSCgrSkgqaChoKUpoKGgpSmgoaClKaCpoKUpoKC7KSGgpSm3IKCgqSUgriAgramgoKCgoKUgtyCgoKCgqbKpJSUgqSkgoKAgramgoaClKaChoKUpqaClJSUhoKUgpSmgoKCgpSCgoKClIK4goKAgoKCtpSUgpSmgoKClJSGlIKUABRgAB0CgIKklIKClIKmgoKCggADFOKUgoKCgoKCgoKm3IKCgpSCgpSCgpSCgpSCpoKCgoKCyrTWgpSC2IIACBKCgoK4goKCgpS4xoKClIKCgpSmlKaCgpSUgsaCgpS0pgAJCIKCgpTugA==", "70-72:1;80-82:1;90-92:1;100-102:1;119-121:1;134-151:1;153-185:2;201-203:1;211-213:1;230-232:1;243-270:1;248-268:1.1;278-288:1;284-286:1.1;356-360:1")]
[assembly: GoPositionMap("os/root_windows.go", "root_windows.cs", "AB9OABYEgqaCgoKClIKCgoKUlIKCpoKClIKCpoKmAAISkoKUgoKClOqqwoKCgqbIgqiyhoKUqLKCgpSUgpSCpoKCgoCCtqaigoKUpoKUgoIACxqClILWgoIADh6UpoKC3JSigoKUgoKClIKCgpSUxIKUpoKCyoKCgryiuIKAgqSCgtyCpoaClKYAEySCgpSUgoCCpJSClJSCpoKC1oKmgqaCpoKmgqaCpqKCgoCCtoKUgoKCgpSClNaCpoKuwoKClKaigoKUgtaigoKUgoKClNiSgqyygg==", "122-124:1;133-138:1;221-239:1;283-285:1")]
[assembly: GoPositionMap("os/stat.go", "stat.cs", "AAoWooIAAhYACAKC")]
[assembly: GoPositionMap("os/stat_windows.go", "stat_windows.cs", "ABAiwoKUqLKClIKCqqKCgpSmgoCCpMqCgoKClIKUgoCCpAADEMKUypS4poKCuIKUlIKU1oKSgpSUlIKClILokuiSgtyU")]
[assembly: GoPositionMap("os/sys.go", "sys.cs", "AAcQkg==")]
[assembly: GoPositionMap("os/sys_windows.go", "sys_windows.cs", "AA8YhKSCgoKCgpSCyoI=")]
[assembly: GoPositionMap("os/tempfile.go", "tempfile.cs", "AA4o9IIAAhgACQKCpoKClJSCgoKCgoCCpJS4mNKCgqaAgpSkAAIWAAgCgqaCgpSUgoKCgoKUgoCCpJSCgIK2uIKClA==")]
[assembly: GoPositionMap("os/types.go", "types.cs", "ABYakAAjXoCigAACEOKCgoKU")]
[assembly: GoPositionMap("os/types_windows.go", "types_windows.cs", "ACpcwoKCgqaCgoKCgpSmABEoogAJGAAICgAMHsIAChqiAAcQypQAAhDkpoKmlIKCgoKCgqamooKUAAsYgoKmlLTIgpS0ABMgtq7igpSUhJSClJS0pIKClIKmuKaCqJIACRTCgoKUlIKClJSCggAQIpSCgpSCgoKClIKCgoLaooKCgoKCpoKmsoKClIKClKiS", "", "", "64=Base/1/1/1")]
[assembly: GoPositionMap("os/zero_copy_posix.go", "zero_copy_posix.cs", "AAsgooCCpKyylIKCpoI=")]
[assembly: GoPositionMap("os/zero_copy_stub.go", "zero_copy_stub.cs", "AAoWgqaC")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("os")]
public static partial class os_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface timeout {}
    internal partial struct @file {}
    internal partial struct dirEntry {}
    internal partial struct dirFS {}
    internal partial struct dirInfo {}
    internal partial struct errSymlink {}
    internal partial struct fileStat {}
    internal partial struct fileWithoutReadFrom {}
    internal partial struct fileWithoutWriteTo {}
    internal partial struct getwdCacheᴛ1 {}
    internal partial struct newFileKind {}
    internal partial struct noReadFrom {}
    internal partial struct noWriteTo {}
    internal partial struct processHandle {}
    internal partial struct processStatus {}
    internal partial struct rawConn {}
    internal partial struct readdirMode {}
    internal partial struct root {}
    internal partial struct rootFS {}
    public partial interface ΔSignal {}
    public partial struct File {}
    public partial struct LinkError {}
    public partial struct ProcAttr {}
    public partial struct Process {}
    public partial struct ProcessState {}
    public partial struct Root {}
    public partial struct SyscallError {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸbytealg() => builtin.initPackage(typeof(@internal.bytealg_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸfilepathlite() => builtin.initPackage(typeof(@internal.filepathlite_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸgodebug() => builtin.initPackage(typeof(@internal.godebug_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸpoll() => builtin.initPackage(typeof(@internal.poll_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸstrconv() => builtin.initPackage(typeof(@internal.strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸstringslite() => builtin.initPackage(typeof(@internal.stringslite_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸsyscallꓸexecenv() => builtin.initPackage(typeof(@internal.syscall.execenv_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸsyscallꓸwindows() => builtin.initPackage(typeof(@internal.syscall.windows_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtestlog() => builtin.initPackage(typeof(@internal.testlog_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸioꓸfs() => builtin.initPackage(typeof(go.io.fs_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸsyncꓸatomic() => builtin.initPackage(typeof(go.sync.atomic_package));
    [GoInit] internal static void initᴛᴛimportꓸsyscall() => builtin.initPackage(typeof(syscall_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
}
