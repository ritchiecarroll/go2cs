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
using fs = go.io.fs_package;
using syscall = go.syscall_package;
using time = go.time_package;
using Δio = go.io_package;
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
[assembly: GoTypeAlias("PathError", "go.io.fs_package.PathError")]
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
[assembly: GoImplement<syscall_package.Signal, Signal>]
[assembly: GoImplement<unixDirent, go.io.fs_package.DirEntry>(Pointer = true)]
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
[assembly: GoFieldAccess(typeof(fileStat))]
[assembly: GoFieldAccess(typeof(io_package.LimitedReader))]
[assembly: GoFieldAccess(typeof(processHandle))]
[assembly: GoFieldAccess(typeof(root))]
[assembly: GoFieldAccess(typeof(syscall_package.ProcAttr))]
[assembly: GoFieldAccess(typeof(syscall_package.Rusage))]
[assembly: GoFieldAccess(typeof(syscall_package.Stat_t))]
[assembly: GoFieldAccess(typeof(unixDirent))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("os/dir.go", "dir.cs", "ABpQABECgpSCuJQAAiQAEgKClIK4lAACIgAKAoKUgpSUAAIQ8oKClKSChgAFKAARAqKCpoKClJSUpIKClKSCgpSSgoKUgoKmgoKUpA==", "122-124:1;146-189:1", "", "", "125:21,183:29")]
[assembly: GoPositionMap("os/dir_darwin.go", "dir_darwin.cs", "ABImgoKUggAExAGClKSkpKSkpJSs5g==")]
[assembly: GoPositionMap("os/eloop_other.go", "eloop_other.cs", "AAsekpSUlIKm")]
[assembly: GoPositionMap("os/env.go", "env.cs", "AAwgwpSCgoKClIKCpqaUlIKmgpSssqqilJSokqyylIKmgoKClKaklpKUrLKCggACENKC6qKCgpSokqiSqqI=")]
[assembly: GoPositionMap("os/error.go", "error.cs", "ABYmlIKCgpSClgABEuAACx6UlpKCrLKClAACEuIAAhLiAAIS4gACFgAIAoKmuIKCpoKokpSkpJQ=")]
[assembly: GoPositionMap("os/exec.go", "exec.cs", "ABImlJQANYQB4oKCgpSClILMsoKCgpSCgpTMkqaoktyUypSssqaCqsKCpoKClIKC7oKClKiSgpSosoKmACROgJaAAAEWAAkCAAIgAA0CgqwACQqCpriCgris0oKCgoKmgtzclKa+sgACENKqogACFgAIAqiSqJKssqqirLIAAhDS")]
[assembly: GoPositionMap("os/exec_nohandle.go", "exec_nohandle.cs", "AAgSgg==")]
[assembly: GoPositionMap("os/exec_posix.go", "exec_posix.cs", "ABw06IKCgoK4gsqCgoKmgoKmppSCuIKCgoK4poKm0oKUgpSklIKUAAwWkqaCpoKmgqaCAAgGooKUgoKUgoKUxrSCgsakgpSq1IKU")]
[assembly: GoPositionMap("os/exec_unix.go", "exec_unix.cs", "ABAqtJSm+AAIDpS4goKUpqaCppiWgpSCAAoOooKCuJSm+NKClIKmgpSUpKbWgoKUpqKCuIKmpqaCpoI=", "63-65:1")]
[assembly: GoPositionMap("os/executable.go", "executable.cs", "AAckAAsC")]
[assembly: GoPositionMap("os/executable_darwin.go", "executable_darwin.cs", "AA8e1IKCgpSCgpSUlJQ=")]
[assembly: GoPositionMap("os/file.go", "file.cs", "AD5+oAAIEpKSAB5IgqaCAAIkAA8C7OKClILuAAgCgqaCpoKCgoKUgoKU6MKClIKClAAHEqIAChaCrPKClIKClIKmlIKmpp4ACQKClIKmgqaCgoKClIKClKjCgpSCgpQABxKiAAoWggAGEPKClIKClIKUqqKCrLKCmIK4gpSCgriokoKClKqigoKUgoKCgpSUlIKCgqauwgACEuIAAhTygoKClJSmmrKCAAIS4gACENKqooKUAAYUsoKUgoKClAACGAAJAgAWIAANApSkgoLIgoKUtoKClLaCgoKClIKCyAAOIAANApSkgoLIgoKUtoKClLaCgoKClIKCyAAOFgAIApKktKSCpqSklAACKgARAJiQAAE0ABgCrsIAAhLi6qKClAADKAARAgACKAARAgAJFIKCgpSCyoKUrsKCgpSCgpSUlKqigoKUgoKUlJSmgoKClIKUgpSmgoKClIKUgpSmgoKClOiSgpSCgpSClK7igoKUpNaCgpQAAhDSmrKClJS4gqaCgoKCgoKU3IKCAAQU0oKClIKClA==", "329-331:1", "", "", "981:21")]
[assembly: GoPositionMap("os/file_open_unix.go", "file_open_unix.cs", "AAwcgoI=")]
[assembly: GoPositionMap("os/file_posix.go", "file_posix.cs", "ABAo4oKUqtKCgqzigoKq0oKCqtKCgqiygoKUgpSCpqiSgoaClOiygpSClAACFPKGgpQAAhLihoKUAAYQ8oKUgpTs0oKUgpTs0oKUgpQAAhTygoKUpoKCgoKUpoKCrNKAgqSClIKCgqbokoKU6JKClOiSgpSqooKUAAIU8oKCgsySgoKC", "79-81:1;107-109:1;123-125:1;190-196:1")]
[assembly: GoPositionMap("os/file_unix.go", "file_unix.cs", "ABYskqaCggAIEoKClIKCpoaClAAQJLKCAAcQgqaokoKCpoKClIKCAAIaAAoCgqYAGDSyAAoUAAkUgqSChgAHEILugtqCuJSClIKClAAKFoKCuIKmutKCAAgUooKCgri8goKUgriCyoKmgoKmgqqCgpSCpoKmgoKmooKUgpSCgoKUuIKu8qaUgoKssoaClKrqhoKUhoIACxiClAAKBoKCgoKUpqqihoKUrsKGgpSmgoKCkraClIKUggALGIKClIKClKaCpqIABxCCpoKCpoKC", "46-48:1;169-171:1;260-263:1;290-293:1;346-348:1;362-364:1;368-370:2;405-407:1;419-421:1;431-433:1")]
[assembly: GoPositionMap("os/getwd.go", "getwd.cs", "ABg0AAsCyoKqooKCgoKUgoIABxCCyoLKgoKCuIKCgoKCgtyClJSC3IKCgpSCgqaCgoLKgpSUgoKCgsqCgoKClIKmuIKClA==")]
[assembly: GoPositionMap("os/path.go", "path.cs", "AA0mAAgEgoKClAAHEIKClIKUgsqCgoLKgqaCgpSUAAIQ0qiSgpSClA==")]
[assembly: GoPositionMap("os/path_unix.go", "path_unix.cs", "AAsekqikpoKmpoK4poKCgpSUgrg=")]
[assembly: GoPositionMap("os/pidfd_other.go", "pidfd_other.cs", "AAoWgqaCpoKmgqaC")]
[assembly: GoPositionMap("os/pipe_unix.go", "pipe_unix.cs", "AA4aoqaCgoKClIKClA==")]
[assembly: GoPositionMap("os/proc.go", "proc.cs", "ABEkgpSUprrAmqCaoJqg3MKCAAIQ0rgABxCUpg==")]
[assembly: GoPositionMap("os/rawconn.go", "rawconn.cs", "ABQkgoKUgoLmgoKUgoLmgoKUgoKmgg==")]
[assembly: GoPositionMap("os/removeall_at.go", "removeall_at.cs", "AA0eoqbKgriCgsqUgsqUgpSUgpSkgoKClJTWlIKCAAgSgpSWkoKCpoKCgpSUlJSUgqaClJSCgoKUpoKCgoKClIKC7oIACBKmgsqCgqaClAACFgAIAoKClA==", "", "", "", "56:21")]
[assembly: GoPositionMap("os/removeall_unix.go", "removeall_unix.cs", "AAgSlA==")]
[assembly: GoPositionMap("os/root.go", "root.cs", "ABIyAAkCgoKUkgAwZrKCrLKqoqqiqqIABhDygpSCgoKUgqrCgqqiAAIS4oKUAAIQ0oKUqqKqoqqiqqKqoqrCgq7igqqirLIAAhYACAIAAhYACAKqwoKClJLaooKClIKClKaCpriCpgAHGgAMAoKUgqaUgoKUgqaCkoKUgpSUgoKUpoKUlJSUlJSUgq7CyoKCgpSCgpSmooKCAAgSgoKUkoKG1qKCgpSCgpSS1oKCgpSmgoKClKaCgoKUqJKClMqCpg==", "395-397:1", "", "", "34:21,278:21,427:21,448:21")]
[assembly: GoPositionMap("os/root_nonwindows.go", "root_nonwindows.cs", "AAgSgg==")]
[assembly: GoPositionMap("os/root_openat.go", "root_openat.cs", "AB1A0oKCgpSCgtbSgoKClILW0oKCgpSCgrSkgqaChoKUpoKGgpSmgoaClKaCpoKUpoKC7KSGgpSm3IKCgqSUgriCpqaCgoKCgpSC3IKCgoKCpsqklJSCpKSCgoKmpoKGgpSmgoaClKamgpSUlIaClIKUpoKCgoKUgoKCgpSCuIKCgoKCppSUgpSmgoKClJSGlIKUABRgAB0CgpSUgoKUgqaCgoKCAAMU4pSCgoKCgoKCgqbcgoKClIKClIKClIKClIKmgoKCgoLKtNaClILYggAIEoKCgriCgoKClLjGgoKUgoKClKaUpoKClJSCxoKClLSmAAkIgoKClO4=", "70-72:1;80-82:1;90-92:1;100-102:1;119-121:1;134-151:1;153-185:2;201-203:1;211-213:1;230-232:1;243-270:1;248-268:1.1;278-288:1;284-286:1.1;356-360:1", "", "", "", "370:21,371:13")]
[assembly: GoPositionMap("os/root_unix.go", "root_unix.cs", "ABgukoKCgoKUgpTqooKGgoKCyoKmyIKosqKCgoKUlJSClKiyooKCgqaCgriCppSUgpSCgqaCmIKCgoK4lJSmgoKCgpSClJSClKaChoKUAAIcAAsCgpSmgq6CroKqgqKigsqCqqaGgpSGgqaClKaCqoKqgqaCpoKmgqaCgoKUgq7CgoKUpoKCgpiSgpSClIKUgoLKgqyy", "25-29:1;40-42:1;65-74:1;66-72:1.1;83-103:1;84-101:1.1;117-128:1;133-142:1;150-152:1;178-182:1;179-181:1.1;186-190:1;187-189:1.1;194-196:1;200-205:1;201-204:1.1;209-211:1;217-219:1;223-225:2;237-239:1;243-245:1;292-295:1")]
[assembly: GoPositionMap("os/stat.go", "stat.cs", "AAoWooIAAhYACAKC")]
[assembly: GoPositionMap("os/stat_darwin.go", "stat_darwin.cs", "AA4aooKCkoKUtLS0tPikgpSClIK6kg==")]
[assembly: GoPositionMap("os/stat_unix.go", "stat_unix.cs", "ABAewoKUgoKClIKokoKGgpSCqJKChoKUgg==", "31-33:1;44-46:1")]
[assembly: GoPositionMap("os/statat.go", "statat.cs", "AAsaooKCgqaClA==")]
[assembly: GoPositionMap("os/statat_unix.go", "statat_unix.cs", "ABEaooKClII=")]
[assembly: GoPositionMap("os/sys.go", "sys.cs", "AAcQkg==")]
[assembly: GoPositionMap("os/sys_bsd.go", "sys_bsd.cs", "ABAWsoKClA==")]
[assembly: GoPositionMap("os/tempfile.go", "tempfile.cs", "AA4o9IIAAhgACQKCpoKClJSCgoKCgoKUlLiY0oKCpoKUlAACFgAIAoKmgoKUlIKCgoKClIKClJSCgqa4goKU")]
[assembly: GoPositionMap("os/types.go", "types.cs", "ABYagAAiXoIAARDigoKClA==")]
[assembly: GoPositionMap("os/types_unix.go", "types_unix.cs", "ABQugoKClII=")]
[assembly: GoPositionMap("os/wait_unimp.go", "wait_unimp.cs", "AAom8g==")]
[assembly: GoPositionMap("os/zero_copy_posix.go", "zero_copy_posix.cs", "AAsgooKUrLKUgoKmgg==")]
[assembly: GoPositionMap("os/zero_copy_stub.go", "zero_copy_stub.cs", "AAoWgqaC")]
// </GoSourcePositionMaps>

namespace go;

using static go.os_package;
using fs = go.io.fs_package;
using syscall = go.syscall_package;
using time = go.time_package;
using Δio = go.io_package;

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
    internal partial struct processHandle {}
    internal partial struct processStatus {}
    internal partial struct rawConn {}
    internal partial struct readdirMode {}
    internal partial struct root {}
    internal partial struct rootFS {}
    internal partial struct unixDirent {}
    public partial interface Signal {}
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
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸpoll() => builtin.initPackage(typeof(@internal.poll_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸstrconv() => builtin.initPackage(typeof(@internal.strconv_package));
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

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(os_package.File)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class osᴛFile;
[GoMethodHost(typeof(os_package.LinkError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class osᴛLinkError;
[GoMethodHost(typeof(os_package.Process)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class osᴛProcess;
[GoMethodHost(typeof(os_package.ProcessState)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class osᴛProcessState;
[GoMethodHost(typeof(os_package.Root)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class osᴛRoot;
[GoMethodHost(typeof(os_package.SyscallError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class osᴛSyscallError;
[GoMethodHost(typeof(os_package.dirFS)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class osᴛdirFS;
[GoMethodHost(typeof(os_package.dirInfo)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class osᴛdirInfo;
[GoMethodHost(typeof(os_package.errSymlink)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class osᴛerrSymlink;
[GoMethodHost(typeof(os_package.@file)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class osᴛfile;
[GoMethodHost(typeof(os_package.fileStat)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class osᴛfileStat;
[GoMethodHost(typeof(os_package.noReadFrom)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class osᴛnoReadFrom;
[GoMethodHost(typeof(os_package.noWriteTo)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class osᴛnoWriteTo;
[GoMethodHost(typeof(os_package.processHandle)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class osᴛprocessHandle;
[GoMethodHost(typeof(os_package.rawConn)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class osᴛrawConn;
[GoMethodHost(typeof(os_package.root)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class osᴛroot;
[GoMethodHost(typeof(os_package.rootFS)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class osᴛrootFS;
[GoMethodHost(typeof(os_package.unixDirent)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class osᴛunixDirent;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class osᴛFile {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (slice<FileInfo>, error) Readdir(this ж<File> Ꮡf, nint n) => os_package.Readdir⁔(Ꮡf, n);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (slice<@string> names, error err) Readdirnames(this ж<File> Ꮡf, nint n) => os_package.Readdirnames⁔(Ꮡf, n);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (slice<DirEntry>, error) ReadDir(this ж<File> Ꮡf, nint n) => os_package.ReadDir⁔(Ꮡf, n);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Name(this ref File f) => os_package.Name⁔(ref f);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint n, error err) Read(this ж<File> Ꮡf, slice<byte> b) => os_package.Read⁔(Ꮡf, b);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint n, error err) ReadAt(this ж<File> Ꮡf, slice<byte> b, int64 off) => os_package.ReadAt⁔(Ꮡf, b, off);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (int64 n, error err) ReadFrom(this ж<File> Ꮡf, Δio.Reader r) => os_package.ReadFrom⁔(Ꮡf, r);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint n, error err) Write(this ж<File> Ꮡf, slice<byte> b) => os_package.Write⁔(Ꮡf, b);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint n, error err) WriteAt(this ж<File> Ꮡf, slice<byte> b, int64 off) => os_package.WriteAt⁔(Ꮡf, b, off);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (int64 n, error err) WriteTo(this ж<File> Ꮡf, Δio.Writer w) => os_package.WriteTo⁔(Ꮡf, w);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (int64 ret, error err) Seek(this ж<File> Ꮡf, int64 offset, nint whence) => os_package.Seek⁔(Ꮡf, offset, whence);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint n, error err) WriteString(this ж<File> Ꮡf, @string s) => os_package.WriteString⁔(Ꮡf, s);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error wrapErr(this ref File f, @string op, error err) => os_package.wrapErr⁔(ref f, op, err);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Chmod(this ж<File> Ꮡf, FileMode mode) => os_package.Chmod⁔(Ꮡf, mode);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error SetDeadline(this ж<File> Ꮡf, time.Time t) => os_package.SetDeadline⁔(Ꮡf, t);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error SetReadDeadline(this ж<File> Ꮡf, time.Time t) => os_package.SetReadDeadline⁔(Ꮡf, t);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error SetWriteDeadline(this ж<File> Ꮡf, time.Time t) => os_package.SetWriteDeadline⁔(Ꮡf, t);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (syscall.RawConn, error) SyscallConn(this ж<File> Ꮡf) => os_package.SyscallConn⁔(Ꮡf);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static uintptr Fd(this ж<File> Ꮡf) => os_package.Fd⁔(Ꮡf);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Close(this ж<File> Ꮡf) => os_package.Close⁔(Ꮡf);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) read(this ж<File> Ꮡf, slice<byte> b) => os_package.read⁔(Ꮡf, b);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) pread(this ж<File> Ꮡf, slice<byte> b, int64 off) => os_package.pread⁔(Ꮡf, b, off);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) write(this ж<File> Ꮡf, slice<byte> b) => os_package.write⁔(Ꮡf, b);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) pwrite(this ж<File> Ꮡf, slice<byte> b, int64 off) => os_package.pwrite⁔(Ꮡf, b, off);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error chmod(this ж<File> Ꮡf, FileMode mode) => os_package.chmod⁔(Ꮡf, mode);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Chown(this ж<File> Ꮡf, nint uid, nint gid) => os_package.Chown⁔(Ꮡf, uid, gid);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Truncate(this ж<File> Ꮡf, int64 size) => os_package.Truncate⁔(Ꮡf, size);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Sync(this ж<File> Ꮡf) => os_package.Sync⁔(Ꮡf);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Chdir(this ж<File> Ꮡf) => os_package.Chdir⁔(Ꮡf);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error setDeadline(this ж<File> Ꮡf, time.Time t) => os_package.setDeadline⁔(Ꮡf, t);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error setReadDeadline(this ж<File> Ꮡf, time.Time t) => os_package.setReadDeadline⁔(Ꮡf, t);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error setWriteDeadline(this ж<File> Ꮡf, time.Time t) => os_package.setWriteDeadline⁔(Ꮡf, t);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error checkValid(this ж<File> Ꮡf, @string op) => os_package.checkValid⁔(Ꮡf, op);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uintptr fd(this ж<File> Ꮡf) => os_package.fd⁔(Ꮡf);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (int64 ret, error err) seek(this ж<File> Ꮡf, int64 offset, nint whence) => os_package.seek⁔(Ꮡf, offset, whence);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (FileInfo, error) Stat(this ж<File> Ꮡf) => os_package.Stat⁔(Ꮡf);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (FileInfo, error) lstatat(this ж<File> Ꮡf, @string name) => os_package.lstatat⁔(Ꮡf, name);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (FileInfo, error) lstatatNolog(this ж<File> Ꮡf, @string name) => os_package.lstatatNolog⁔(Ꮡf, name);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (int64 written, bool handled, error err) writeTo(this ref File f, Δio.Writer w) => os_package.writeTo⁔(ref f, w);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (int64 n, bool handled, error err) readFrom(this ref File f, Δio.Reader r) => os_package.readFrom⁔(ref f, r);
}
partial class osᴛLinkError {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Error(this ref LinkError e) => os_package.Error⁔(ref e);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Unwrap(this ref LinkError e) => os_package.Unwrap⁔(ref e);
}
partial class osᴛProcess {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (uintptr, processStatus) handleTransientAcquire(this ж<Process> Ꮡp) => os_package.handleTransientAcquire⁔(Ꮡp);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void handleTransientRelease(this ref Process p) => os_package.handleTransientRelease⁔(ref p);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static processStatus pidStatus(this ж<Process> Ꮡp) => os_package.pidStatus⁔(Ꮡp);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Release(this ж<Process> Ꮡp) => os_package.Release⁔(Ꮡp);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static processStatus doRelease(this ж<Process> Ꮡp, processStatus newStatus) => os_package.doRelease⁔(Ꮡp, newStatus);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Kill(this ж<Process> Ꮡp) => os_package.Kill⁔(Ꮡp);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<ProcessState>, error) Wait(this ж<Process> Ꮡp) => os_package.Wait⁔(Ꮡp);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Signal(this ж<Process> Ꮡp, Signal sig) => os_package.Signal⁔(Ꮡp, sig);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error WithHandle(this ж<Process> Ꮡp, Action<uintptr> f) => os_package.WithHandle⁔(Ꮡp, f);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error kill(this ж<Process> Ꮡp) => os_package.kill⁔(Ꮡp);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error withHandle(this ж<Process> Ꮡp, Action<uintptr> f) => os_package.withHandle⁔(Ꮡp, f);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<ProcessState> ps, error err) wait(this ж<Process> Ꮡp) => os_package.wait⁔(Ꮡp);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<ProcessState>, error) pidWait(this ж<Process> Ꮡp) => os_package.pidWait⁔(Ꮡp);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error signal(this ж<Process> Ꮡp, Signal sig) => os_package.signal⁔(Ꮡp, sig);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error pidSignal(this ж<Process> Ꮡp, syscall.Signal s) => os_package.pidSignal⁔(Ꮡp, s);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<ProcessState>, error) pidfdWait(this ref Process _) => os_package.pidfdWait⁔(ref _);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error pidfdSendSignal(this ref Process _Δp0, syscall.Signal _Δp1) => os_package.pidfdSendSignal⁔(ref _Δp0, _Δp1);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (bool, error) blockUntilWaitable(this ref Process p) => os_package.blockUntilWaitable⁔(ref p);
}
partial class osᴛProcessState {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static time.Duration UserTime(this ref ProcessState p) => os_package.UserTime⁔(ref p);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static time.Duration SystemTime(this ref ProcessState p) => os_package.SystemTime⁔(ref p);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Exited(this ref ProcessState p) => os_package.Exited⁔(ref p);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Success(this ref ProcessState p) => os_package.Success⁔(ref p);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static any Sys(this ref ProcessState p) => os_package.Sys⁔(ref p);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static any SysUsage(this ref ProcessState p) => os_package.SysUsage⁔(ref p);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint Pid(this ref ProcessState p) => os_package.Pid⁔(ref p);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool exited(this ref ProcessState p) => os_package.exited⁔(ref p);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool success(this ref ProcessState p) => os_package.success⁔(ref p);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static any sys(this ref ProcessState p) => os_package.sys⁔(ref p);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static any sysUsage(this ref ProcessState p) => os_package.sysUsage⁔(ref p);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ж<ProcessState> Ꮡp) => os_package.String⁔(Ꮡp);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint ExitCode(this ж<ProcessState> Ꮡp) => os_package.ExitCode⁔(Ꮡp);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static time.Duration userTime(this ref ProcessState p) => os_package.userTime⁔(ref p);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static time.Duration systemTime(this ref ProcessState p) => os_package.systemTime⁔(ref p);
}
partial class osᴛRoot {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Name(this ref Root r) => os_package.Name⁔(ref r);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Close(this ref Root r) => os_package.Close⁔(ref r);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<File>, error) Open(this ж<Root> Ꮡr, @string name) => os_package.Open⁔(Ꮡr, name);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<File>, error) Create(this ж<Root> Ꮡr, @string name) => os_package.Create⁔(Ꮡr, name);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<File>, error) OpenFile(this ж<Root> Ꮡr, @string name, nint flag, FileMode perm) => os_package.OpenFile⁔(Ꮡr, name, flag, perm);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<Root>, error) OpenRoot(this ж<Root> Ꮡr, @string name) => os_package.OpenRoot⁔(Ꮡr, name);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Chmod(this ж<Root> Ꮡr, @string name, FileMode mode) => os_package.Chmod⁔(Ꮡr, name, mode);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Mkdir(this ж<Root> Ꮡr, @string name, FileMode perm) => os_package.Mkdir⁔(Ꮡr, name, perm);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error MkdirAll(this ж<Root> Ꮡr, @string name, FileMode perm) => os_package.MkdirAll⁔(Ꮡr, name, perm);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Chown(this ж<Root> Ꮡr, @string name, nint uid, nint gid) => os_package.Chown⁔(Ꮡr, name, uid, gid);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Lchown(this ж<Root> Ꮡr, @string name, nint uid, nint gid) => os_package.Lchown⁔(Ꮡr, name, uid, gid);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Chtimes(this ж<Root> Ꮡr, @string name, time.Time atime, time.Time mtime) => os_package.Chtimes⁔(Ꮡr, name, atime, mtime);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Remove(this ж<Root> Ꮡr, @string name) => os_package.Remove⁔(Ꮡr, name);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error RemoveAll(this ж<Root> Ꮡr, @string name) => os_package.RemoveAll⁔(Ꮡr, name);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (FileInfo, error) Stat(this ж<Root> Ꮡr, @string name) => os_package.Stat⁔(Ꮡr, name);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (FileInfo, error) Lstat(this ж<Root> Ꮡr, @string name) => os_package.Lstat⁔(Ꮡr, name);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (@string, error) Readlink(this ж<Root> Ꮡr, @string name) => os_package.Readlink⁔(Ꮡr, name);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Rename(this ж<Root> Ꮡr, @string oldname, @string newname) => os_package.Rename⁔(Ꮡr, oldname, newname);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Link(this ж<Root> Ꮡr, @string oldname, @string newname) => os_package.Link⁔(Ꮡr, oldname, newname);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Symlink(this ж<Root> Ꮡr, @string oldname, @string newname) => os_package.Symlink⁔(Ꮡr, oldname, newname);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (slice<byte>, error) ReadFile(this ж<Root> Ꮡr, @string name) => os_package.ReadFile⁔(Ꮡr, name);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error WriteFile(this ж<Root> Ꮡr, @string name, slice<byte> data, FileMode perm) => os_package.WriteFile⁔(Ꮡr, name, data, perm);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void logOpen(this ref Root r, @string name) => os_package.logOpen⁔(ref r, name);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void logStat(this ref Root r, @string name) => os_package.logStat⁔(ref r, name);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static fs.FS FS(this ж<Root> Ꮡr) => os_package.FS⁔(Ꮡr);
}
partial class osᴛSyscallError {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Error(this ref SyscallError e) => os_package.Error⁔(ref e);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Unwrap(this ref SyscallError e) => os_package.Unwrap⁔(ref e);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Timeout(this ref SyscallError e) => os_package.Timeout⁔(ref e);
}
partial class osᴛdirFS {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (fs.File, error) Open(this dirFS dir, @string name) => os_package.Open⁔(dir, name);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (slice<byte>, error) ReadFile(this dirFS dir, @string name) => os_package.ReadFile⁔(dir, name);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (slice<DirEntry>, error) ReadDir(this dirFS dir, @string name) => os_package.ReadDir⁔(dir, name);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (fs.FileInfo, error) Stat(this dirFS dir, @string name) => os_package.Stat⁔(dir, name);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (fs.FileInfo, error) Lstat(this dirFS dir, @string name) => os_package.Lstat⁔(dir, name);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (@string, error) ReadLink(this dirFS dir, @string name) => os_package.ReadLink⁔(dir, name);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (@string, error) join(this dirFS dir, @string name) => os_package.join⁔(dir, name);
}
partial class osᴛdirInfo {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void close(this ref dirInfo d) => os_package.close⁔(ref d);
}
partial class osᴛerrSymlink {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string Error(this errSymlink _) => os_package.Error⁔(_);
}
partial class osᴛfile {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error close(this ж<@file> Ꮡfile) => os_package.close⁔(Ꮡfile);
}
partial class osᴛfileStat {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string Name(this ref fileStat fs) => os_package.Name⁔(ref fs);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool IsDir(this ref fileStat fs) => os_package.IsDir⁔(ref fs);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static int64 Size(this ref fileStat fs) => os_package.Size⁔(ref fs);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static FileMode Mode(this ref fileStat fs) => os_package.Mode⁔(ref fs);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static time.Time ModTime(this ref fileStat fs) => os_package.ModTime⁔(ref fs);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static any Sys(this ж<fileStat> Ꮡfs) => os_package.Sys⁔(Ꮡfs);
}
partial class osᴛnoReadFrom {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (int64, error) ReadFrom(this noReadFrom _Δp0, Δio.Reader _Δp1) => os_package.ReadFrom⁔(_Δp0, _Δp1);
}
partial class osᴛnoWriteTo {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (int64, error) WriteTo(this noWriteTo _Δp0, Δio.Writer _Δp1) => os_package.WriteTo⁔(_Δp0, _Δp1);
}
partial class osᴛprocessHandle {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (uintptr, bool) acquire(this ж<processHandle> Ꮡph) => os_package.acquire⁔(Ꮡph);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void release(this ж<processHandle> Ꮡph) => os_package.release⁔(Ꮡph);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void closeHandle(this ref processHandle ph) => os_package.closeHandle⁔(ref ph);
}
partial class osᴛrawConn {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Control(this ref rawConn c, Action<uintptr> f) => os_package.Control⁔(ref c, f);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Read(this ref rawConn c, Func<uintptr, bool> f) => os_package.Read⁔(ref c, f);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Write(this ref rawConn c, Func<uintptr, bool> f) => os_package.Write⁔(ref c, f);
}
partial class osᴛroot {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Close(this ж<root> Ꮡr) => os_package.Close⁔(Ꮡr);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error incref(this ж<root> Ꮡr) => os_package.incref⁔(Ꮡr);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void decref(this ж<root> Ꮡr) => os_package.decref⁔(Ꮡr);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string Name(this ref root r) => os_package.Name⁔(ref r);
}
partial class osᴛrootFS {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (fs.File, error) Open(this ж<rootFS> Ꮡrfs, @string name) => os_package.Open⁔(Ꮡrfs, name);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (slice<DirEntry>, error) ReadDir(this ж<rootFS> Ꮡrfs, @string name) => os_package.ReadDir⁔(Ꮡrfs, name);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (slice<byte>, error) ReadFile(this ж<rootFS> Ꮡrfs, @string name) => os_package.ReadFile⁔(Ꮡrfs, name);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (@string, error) ReadLink(this ж<rootFS> Ꮡrfs, @string name) => os_package.ReadLink⁔(Ꮡrfs, name);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (FileInfo, error) Stat(this ж<rootFS> Ꮡrfs, @string name) => os_package.Stat⁔(Ꮡrfs, name);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (FileInfo, error) Lstat(this ж<rootFS> Ꮡrfs, @string name) => os_package.Lstat⁔(Ꮡrfs, name);
}
partial class osᴛunixDirent {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string Name(this ref unixDirent d) => os_package.Name⁔(ref d);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool IsDir(this ref unixDirent d) => os_package.IsDir⁔(ref d);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static FileMode Type(this ref unixDirent d) => os_package.Type⁔(ref d);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (FileInfo, error) Info(this ref unixDirent d) => os_package.Info⁔(ref d);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string String(this ж<unixDirent> Ꮡd) => os_package.String⁔(Ꮡd);
}
// </MethodForwarders>
