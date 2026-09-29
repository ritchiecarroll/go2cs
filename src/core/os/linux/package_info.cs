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
global using runtimeꓸError = go.runtime_package.ΔError;
global using syscallꓸSignal = go.syscall_package.ΔSignal;
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
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
[assembly: GoDynamicTypeLift("696e746572666163657b4e6574776f726b282920696e7465726e616c2f706f6c6c2e537472696e673b20506f6c6c46442829202a696e7465726e616c2f706f6c6c2e46447d", "getPollFDAndNetwork_type")]
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
[assembly: GoImplement<dirFS, go.io.fs_package.FS>]
[assembly: GoImplement<errSymlink, error>]
[assembly: GoImplement<fileStat, go.io.fs_package.FileInfo>(Pointer = true)]
[assembly: GoImplement<fileWithoutReadFrom, io_package.Writer>]
[assembly: GoImplement<fileWithoutWriteTo, io_package.Reader>]
[assembly: GoImplement<go.io.fs_package.File, io_package.Reader>]
[assembly: GoImplement<rawConn, syscall_package.RawConn>(Pointer = true)]
[assembly: GoImplement<rootFS, go.io.fs_package.FS>(Pointer = true)]
[assembly: GoImplement<syscall_package.ΔSignal, ΔSignal>]
[assembly: GoImplement<unixDirent, go.io.fs_package.DirEntry>(Pointer = true)]
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
[assembly: go.GoPositionMap("os/dir.go", "dir.cs", "ABdQABECgpSCuJQAAiQAEgKClIK4lAACIgAKAoKUgpSUAAcY8oKClJSChgAFLgAUAqKCloKClJKCzIKWgoKUkoKClIKCloCCgqQ=", "126-128:1;153-193:1")]
[assembly: go.GoPositionMap("os/dir_unix.go", "dir_unix.cs", "AB5AloLIgoKCuAAJBIKCgpSCkoIAChiClpSCgoKCgoKUlIKCuoKCgpSCgoKC7oKUgoKClIKCgoK4gpSSlIKkgqaUgpSUgqaUgpSogpTokoKUgpSmgpSkpKSkyIKUpKSkpA==")]
[assembly: go.GoPositionMap("os/dirent_linux.go", "dirent_linux.cs", "AAsYgqaCpoKCgpSmgoKClIKUpKSkpKSkpA==")]
[assembly: go.GoPositionMap("os/eloop_other.go", "eloop_other.cs", "AAsekpSklIKm")]
[assembly: go.GoPositionMap("os/env.go", "env.cs", "AAwgopSCgoKClIKC7JSUgqaClKyyqqKUpKiSrLKUgpaSgoKUpqSmkpSssoKCAAIQ0oLaooKClKiSqJKqog==")]
[assembly: go.GoPositionMap("os/error.go", "error.cs", "ABImlJKSkpSSloAAAhLwAAwegKSAppKCrLKClAACEuIAAhLiAAIS4gACFgAIAoKmuIKCpoKokpSkpJQ=")]
[assembly: go.GoPositionMap("os/exec.go", "exec.cs", "ABIkADmuAYK4gqaCyoKCpoLcgoKmooKWgoKClIKClLiigpaCgoKCuJTclIKClIKUAAMS8oKWgoKC3JSmlIKClIKUuKKClqaiggAJFgAkTpCmkAACFgAJAgACIAANAoKsABUirLIAAhDSqqKokqiSrLKqoqyyAAIQ0g==")]
[assembly: go.GoPositionMap("os/exec_linux.go", "exec_linux.cs", "AAkWgg==")]
[assembly: go.GoPositionMap("os/exec_posix.go", "exec_posix.cs", "ABw06IKAgoKCuILKgoKCpoKClpaEgqiCgoKCqKaCAAkWkqaCpoKmgqaC5qKClIKClIKSpMa0goLGtIKUqtSClA==")]
[assembly: go.GoPositionMap("os/exec_unix.go", "exec_unix.cs", "ABAstJampPgACA6UuIKClKamgpaYloKUggAJDqKCgqiWpqQACAjygpSCloKElKSm1oKClKbYhJ6mAAkGgqaigrjIpqaCpoI=", "67-69:1")]
[assembly: go.GoPositionMap("os/executable.go", "executable.cs", "AAckAAsC")]
[assembly: go.GoPositionMap("os/executable_procfs.go", "executable_procfs.cs", "ABEegoKYo6io")]
[assembly: go.GoPositionMap("os/file.go", "file.cs", "AD18sAAJEqKiACVIgqaC3OKAgqSC3gAIAoCCpoKWgoKCgpSCgpTYwoCCpIKClAAHEqIAChaCrPKAgqSCgpSCloSClqaeAAkCgIKkgpaCloKCgoKUgoKUqMKAgqSCgpQABxKiAAoWggAFEPKAgqSCgpSClKqigqyygoiCqIKEgoKoqJKCgpSqooCCgqSCgoKClJSUgIKCgrauwgACEuIAAhTygoKClISmmrKCuAACEOIAAhDSqqKClAAGFLKClIKklAACGAAJAgANIAANAoSUgoK4goKUpoKClKaCkoKClKS4AAkgAA0ChJSCgriCgpSmgoKUpoKSgoKUpLgACRYACAKSlKSkgIK2lKSkAAIqABIAqKAAAjQAGAKuwgACEuLaooCCpAADKAARAsqCgoKUgsqClK7CgoKUgoKAlKSUqqKCgpSCgoCUpJSmgoKClIKUgpTYkoKUgoKUgpSu4oKClJLWgoKAgoKCtsyCloKCgoKCgpSWgoIABBTSgoKUgoCCpA==", "309-311:1")]
[assembly: go.GoPositionMap("os/file_open_unix.go", "file_open_unix.cs", "AAwcgoI=")]
[assembly: go.GoPositionMap("os/file_posix.go", "file_posix.cs", "AA8m4oKUqtKCgqzigoKq0oKCqtKCgqiygoKUgpSCpqiSgoaClNiygIKkgIKkAAIU8oaClAACEuKGgpQABRDygIKkgIKk3NKAgqSAgqTc0oCCpICCpAACFPKCgoKUpoKCgIKkrNKAgqSAgqTYkoCCpNiSgIKk2JKAgqSqooKUAAIU8oKCgsySgoKC", "78-80:1;106-108:1;122-124:1;181-187:1")]
[assembly: go.GoPositionMap("os/file_unix.go", "file_unix.cs", "ABYskqaCggAIEoCCgIKkpLaGgpQADzgADgKCAAYQgpYAAhgACQKCgpaCgpSCggACGgAKAoKWAA00sgAJFAAIFIKUgoYAABDynsLKgriUgrSCgpQAChaAgoCCyIKmutKCAAcUooKCgIK4vIKClIKogrqCloKCpoKqgoKUgpaCloKCpqKClICCpIKAgoKUuIKu8oCmpIKCrLKGgpSq6oaClIaCAAoYgpT2goKCgpSmqqKGgpSuwoaClKaCgoKStoKUgpSCAAsYgKKAooCkgoKUpoKmgsqCloKCloKC", "46-48:1;187-189:1;278-281:1;308-311:1;364-366:1;380-382:1;386-388:2;423-425:1;437-439:1;449-451:1")]
[assembly: go.GoPositionMap("os/getwd.go", "getwd.cs", "ABc0AAsCyoKaooKCgoKUgoIABhCCyoK6goKCuIKCgoKCgsyClJSCzIKCkpSCgpaCgoLKgpSUgoKCgrqCgoKClIKmqIKChA==")]
[assembly: go.GoPositionMap("os/path.go", "path.cs", "AA0mAAgEgoKClAAFEIKClIKUgrqAgoKCyoKmgoKUlAACENKokoKUgpQ=")]
[assembly: go.GoPositionMap("os/path_unix.go", "path_unix.cs", "AAsekqikloKWloKoloKCgpSUgqg=")]
[assembly: go.GoPositionMap("os/pidfd_linux.go", "pidfd_linux.cs", "ABk80oKWhIK4gpKClqqigpaCgoKClJSmgoKWgoKU1gAKEIKa1KSEmIaCuIIACQ6ygpSkpITWgrYABhIACwaCgrqCgpSWmIKogIIABxCAgqbcysY=", "105-107:1;164-166:1")]
[assembly: go.GoPositionMap("os/pipe2_unix.go", "pipe2_unix.cs", "AA0aooSCgpY=")]
[assembly: go.GoPositionMap("os/proc.go", "proc.cs", "ABAkgpSUprrQqrCqsKqw3MKCAAIQ0rgABhCEpg==")]
[assembly: go.GoPositionMap("os/rawconn.go", "rawconn.cs", "ABMkgoCCpIKC1oKAgqSCgtaCgIKkgoKmgg==")]
[assembly: go.GoPositionMap("os/removeall_at.go", "removeall_at.cs", "AA4eoqa6gqiCgrqEgpSUgpSUgIKAgoKkpNailIaCAAcSgpSGkoKCloKCgpSUlIKWgoSUgoKClJaCgoKCgIKkgoLeggAHEpaCuoaCloKUAAIWAAgChoKWgqg=", "61-63:1;147-149:2;169-171:1")]
[assembly: go.GoPositionMap("os/root.go", "root.cs", "ABEwAAkCgoKUkgAoWqKCrLKqoqqiqqIABRDygpSCgoKUgqrCggACEuKClKqiqsKCruKCpoKApsiCgKYABxoADAKClIKWlIKClIKWgpKClIKUlIKClKaClJSUlJSUgq7CyoKCgpSCgpSmooKCAAcSgoKUkoKG1qKCgpSCgpSS1oKCgpSokoKUyoKm", "282-284:1")]
[assembly: go.GoPositionMap("os/root_nonwindows.go", "root_nonwindows.cs", "AAgSgg==")]
[assembly: go.GoPositionMap("os/root_openat.go", "root_openat.cs", "ABs80oKCgpSCgtbSgoKClILW0oKCgpSCgrSkgqaChoKUpoKGgpQAAhYADAKAgqSEgoKWgoKCggACFOKEgoKCgoKCgpbcgoKClIKClIKClIKClIKW7oKAgraCgoKClKKCuICCgoKUgoKU7pSmgoKUlIKmAAkQgA==", "68-70:1;78-80:1;108-112:1")]
[assembly: go.GoPositionMap("os/root_unix.go", "root_unix.cs", "ABImkoKCgoKUgpTaooKGgoKCuoKWyIKosqKCgoKUlJSClKiyooKCgpSUlIKUgqaCmIKCgtqUlKaCgoKAgqSCgpSUgpSmgqqmhoKUhoKmgpSuwoKClKaCgoKYkoKUgpSClIKUgg==", "21-25:1;36-38:1;61-70:1;62-68:1.1;79-88:1;80-86:1.1;101-112:1;117-127:1;135-137:1;143-145:1;149-151:2;181-184:1")]
[assembly: go.GoPositionMap("os/stat.go", "stat.cs", "AAoWooIAAhYACAKC")]
[assembly: go.GoPositionMap("os/stat_linux.go", "stat_linux.cs", "AA4aooKCkoKUpKSkpMi0gpSClIK6kg==")]
[assembly: go.GoPositionMap("os/stat_unix.go", "stat_unix.cs", "AA8ewoKUgoKClIKokoKGgpSCqJKChoKUgg==", "31-33:1;44-46:1")]
[assembly: go.GoPositionMap("os/sys.go", "sys.cs", "AAcQkg==")]
[assembly: go.GoPositionMap("os/sys_linux.go", "sys_linux.cs", "AA4YxqKEgoKCgoLKgpSCgpSWgoKUlIKCloKU")]
[assembly: go.GoPositionMap("os/tempfile.go", "tempfile.cs", "AA4o9IIAAhgACQKCloKClISCgoKCgoCCpJS4mNKCgqaAgpSkAAIWAAgCgpaCgpSEgoKCgoKUgoCCpJSCgIK2uIKClA==")]
[assembly: go.GoPositionMap("os/types.go", "types.cs", "AA4akAAsXoCigAACEOKCgoKU")]
[assembly: go.GoPositionMap("os/types_unix.go", "types_unix.cs", "ABQugKKAooCigKSC")]
[assembly: go.GoPositionMap("os/zero_copy_linux.go", "zero_copy_linux.cs", "AAwcktbClIKWgoKWkoKWgpamAAoMgpaCgpTWwpiAgqYABxCCloSClubCmICCpoKUtMSWppaCgpT8soKClIKClIiClKaClKQ=", "30-33:1")]
[assembly: go.GoPositionMap("os/zero_copy_posix.go", "zero_copy_posix.cs", "AAsgooCCpKyyhIKCloI=")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("os")]
public static partial class os_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface getPollFDAndNetwork_type {}
    internal partial interface timeout {}
    internal partial struct @file {}
    internal partial struct dirFS {}
    internal partial struct dirInfo {}
    internal partial struct errSymlink {}
    [GoValueClone("sys")] internal partial struct fileStat {}
    internal partial struct fileWithoutReadFrom {}
    internal partial struct fileWithoutWriteTo {}
    internal partial struct getwdCacheᴛ1 {}
    internal partial struct newFileKind {}
    internal partial struct noReadFrom {}
    internal partial struct noWriteTo {}
    internal partial struct processMode {}
    internal partial struct rawConn {}
    internal partial struct readdirMode {}
    internal partial struct root {}
    internal partial struct rootFS {}
    internal partial struct unixDirent {}
    public partial interface ΔSignal {}
    public partial struct File {}
    public partial struct LinkError {}
    public partial struct ProcAttr {}
    public partial struct Process {}
    public partial struct ProcessState {}
    public partial struct Root {}
    public partial struct SyscallError {}
    public partial struct processStatus {}
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
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸpoll() => builtin.initPackage(typeof(@internal.poll_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸstringslite() => builtin.initPackage(typeof(@internal.stringslite_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸsyscallꓸexecenv() => builtin.initPackage(typeof(@internal.syscall.execenv_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸsyscallꓸunix() => builtin.initPackage(typeof(@internal.syscall.unix_package));
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
