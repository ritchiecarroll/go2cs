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
global using abiꓸArrayType = go.@internal.abi_package.ΔArrayType;
global using abiꓸChanDir = go.@internal.abi_package.ΔChanDir;
global using abiꓸFuncType = go.@internal.abi_package.ΔFuncType;
global using abiꓸInterfaceType = go.@internal.abi_package.ΔInterfaceType;
global using abiꓸKind = go.@internal.abi_package.ΔKind;
global using abiꓸMapType = go.@internal.abi_package.ΔMapType;
global using abiꓸName = go.@internal.abi_package.ΔName;
global using abiꓸStructType = go.@internal.abi_package.ΔStructType;
global using runtimeꓸError = go.runtime_package.ΔError;
// </ImportedTypeAliases>

using go;
using static go.syscall_package;

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
[assembly: GoTypeAlias("Signal", "ΔSignal")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<Errno, error>]
[assembly: GoImplement<InterfaceAddrMessage, RoutingMessage>(Pointer = true)]
[assembly: GoImplement<InterfaceMessage, RoutingMessage>(Pointer = true)]
[assembly: GoImplement<InterfaceMulticastAddrMessage, RoutingMessage>(Pointer = true)]
[assembly: GoImplement<RouteMessage, RoutingMessage>(Pointer = true)]
[assembly: GoImplement<SockaddrDatalink, Sockaddr>(Pointer = true)]
[assembly: GoImplement<SockaddrInet4, Sockaddr>(Pointer = true)]
[assembly: GoImplement<SockaddrInet6, Sockaddr>(Pointer = true)]
[assembly: GoImplement<SockaddrUnix, Sockaddr>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<WaitStatus, ΔSignal>(Inverted = true, ValueType = "uint32")]
[assembly: GoImplicitConv<_C_int, WaitStatus>(Inverted = true, ValueType = "int32")]
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("syscall/bpf_bsd.go", "bpf_bsd.cs", "AAsgkqiSqJKCgoKUqLKCgpSokoKCgpSosoKClKiygoKUqJKCgpQABxKSgoKClKiSgoKCgpSokoKCgpSokoKClKiSgoKClKiygoKUqJKCgoKCgpSokoKCgpSClKiSgoKClKiygoKU")]
[assembly: go.GoPositionMap("syscall/dirent.go", "dirent.cs", "ABQikoKUgpSmgpSkpKSkyIKUpKSkpAAEEOKCgoKCgpSCgoKCuJKUgoKClIKCgoK4gpSCgpQ=")]
[assembly: go.GoPositionMap("syscall/env_unix.go", "env_unix.cs", "ABlAtJKCgoKCgoCC3KTcooSChICCgqSC1sKCgpaChIKClIKCgqbmooKClIKCuIKCgrqChIKCgpSClIKC1qKEgoSClILWooKCgoKCgqY=")]
[assembly: go.GoPositionMap("syscall/exec_libc2.go", "exec_libc2.cs", "ACxQopIAARoADQYADxS6goKCgpSUqIKCgoKWlIK8goCCgsqCgoK6lIKCqKaCgoKClKiCgsyWgoKCuoCCgoKClIKCgqaCgpSCgsqCgoLMgoKUgoKUlIKUgpSCgpKUgpSCgpSUgpSCuoKCgpSmgoKUuIKCAAYQgqiCgoK6goKCuoKouoSSgro=")]
[assembly: go.GoPositionMap("syscall/exec_unix.go", "exec_unix.cs", "AE6QAdKCgpSCrLKCgoKUlIKCgoKCgpSmgKSigoKUgpSClJSCAB4wwoKCgoSClIKCqIKClIKClIKCloKWgoKCgqaCgoKCzIKUgpaWgIKCuIKCgoKClJaCgoKCpoKCgpSCuoKCqISoqJKowoKoogAJErKCgpSCgpSCgpSEgoKWgpTstpTagg==")]
[assembly: go.GoPositionMap("syscall/flock_bsd.go", "flock_bsd.cs", "AAoYkoI=")]
[assembly: go.GoPositionMap("syscall/forkpipe.go", "forkpipe.cs", "AAgWooKClIKClIKmgqaC")]
[assembly: go.GoPositionMap("syscall/rlimit.go", "rlimit.cs", "ABU8AA4CgoCCgpKCgsiCppQ=")]
[assembly: go.GoPositionMap("syscall/rlimit_darwin.go", "rlimit_darwin.cs", "AAsUtoKClII=")]
[assembly: go.GoPositionMap("syscall/route_bsd.go", "route_bsd.cs", "AA4okoKmyOyCpoKUqJKClIKClIKCgoIAFQoAAhqOgoKUgoKClKiSlIKUgqSClIKkAAcWABUogoK4lIKCpIKClJSkgoKkgpKUlAAEEtKUkoCCpIKUgoCCpAAaNoKCgoKCgpSClIKClIKkgoKUgoKkgoKUgrYACxiCgoKUgoKUggALGIKCgoKCgpSClIKClIKkgoKUgoKkgoKUgrau4pKCgoKCgpSAgpSkpoKUrsKCgpQ=")]
[assembly: go.GoPositionMap("syscall/route_darwin.go", "route_darwin.cs", "AAkSopSCpIKkgqSCpAALGIKCgoKClIKUgoKUgqSCgpSCpIKClIK2")]
[assembly: go.GoPositionMap("syscall/sockcmsg_unix.go", "sockcmsg_unix.cs", "AAwioqqipqIACBaigoKCgoKUgoKUpoKCgpSqwoKCgoKCgoKUqsKClIKUgoKClA==")]
[assembly: go.GoPositionMap("syscall/sockcmsg_unix_other.go", "sockcmsg_unix_other.cs", "AAockqiWqsK4kpaSuA==")]
[assembly: go.GoPositionMap("syscall/syscall.go", "syscall.cs", "ACBK8oKClKyygpSCggACENCqsoKClAAHEJKokqiSqJKqopai")]
[assembly: go.GoPositionMap("syscall/syscall_bsd.go", "syscall_bsd.cs", "ABIsgoKCgpSCgpQAAhQACAKCgpSCqIKWgoKClIKClKaCgpaCgpSmisIADyiApIKClKaApIKCgpSmgKSApICkgoKUpoCo0oKCgpQAAhoACwKClIKCgoKCgqaigpSCgoKCgoKCpqKCgoKUgoKClKaigpSCgoKCgoKCgqailIKCgoKCgoKCgoKmgoKUjILCpoKmgqaCgoKCgqaCgoKCgoKkpsKCkoKClMqClIKCgpSmsoKSgILIgoKUqrKCkoKmspKCpoKCkoKmgoKSgqaCgpKCpoKCkoKuAAgCgoKCgoKClIKUgoKUgpSCgoCCpIKCqsKCgoKCgoKUgpSCgpSClIKCgIKkgpSqkpKClIKUprSCgqiSgIKkgqiCgIK4gpSmtIKCqJKCgIKkgpSqkoKUpoKClIKCuMiqkoKUAA0cgqaC")]
[assembly: go.GoPositionMap("syscall/syscall_darwin.go", "syscall_darwin.cs", "ABIokpKSAA8iwgAAEvKUgoKCuoCCpKaCpoKmgqaAooCosoKUgoKCgpSmsoKCgoKUgoKClKYAASoADgAAAsIBAF4CprKCgoKUppjCgoKClKYADRCSggAHEoKClIKCgpSEgoKCgoKClIKUgoKClILKpoKCgriSgpboopKSkpI=")]
[assembly: go.GoPositionMap("syscall/syscall_darwin_amd64.go", "syscall_darwin_amd64.cs", "AAsYgqaCAAIYAAoCgoKmgqaCpoKmsqSEhIKUpprE")]
[assembly: go.GoPositionMap("syscall/syscall_unix.go", "syscall_unix.cs", "ACFEkoCCpAAKGOKCqIKCqJaCgoKC5tKCqIKCgoKCqICCpIIACCaCgoKCpqaClKSkpKSmgqaCAAoaopSkpKSkzqSCgoKCpqaygoKClIKmgpSClKaygpSCgoKmlIKUgpSClKaygoKClIKmgpSClKaygpSCgpSClIKUABw6ooKClKaigoKUprKCkoCCpKaigpKCpsKCkoCCpIKUprKCkoCCpIKCgoKmsoKSgIKkgoKCgoKm0oKCgpSCgoKCptKCgoKUgoKCgoKm4oKUgpSmooKmooKCgoKCpqaigoKUpqKCgpSmooKClKaigoKUpqKYgoKCpqaipoKipqKmgqaCpoKmgqaCgoKUpqKmsoKUgqaygoKCgpSmgoKU")]
[assembly: go.GoPositionMap("syscall/time_nofake.go", "time_nofake.cs", "AAoWlA==")]
[assembly: go.GoPositionMap("syscall/timestruct.go", "timestruct.cs", "AAgUkKaSgoKCgpSosKaSgoKCgoKU")]
[assembly: go.GoPositionMap("syscall/zsyscall_darwin_amd64.go", "zsyscall_darwin_amd64.cs", "AAwcwoKCgpSmnMKCgpSmnNKCgoKUppzSgoKClKacwoKClKacwoKClKac0oKCgpSmnMKCgpSmnMKCgpSmnMKCgpSmnMKCgpSmnMKCgpSmnMKCgpSmnNKCgpSUgoKClKacwoKClJSCgpSmnNKCgoKUppzSgoKClKac0oKCgpSmnMKCgoKUgoKUppzCgoKUppzSgoKClKac0oKCgpSqsoKClKacwoKClKqygoKUppzCgoKClIKClKacwoKClKacwoKCgpSCgpSmnMKCgpSmnMKCgoKUgoKUppzCgoKClIKClKacwoKCgpSCgpSmnMKCgoKUgoKUppzCgoKClIKClKacwoKClKacwoKClKac0oKCgpSmnMKCgpSmnMKCgoKUgoKClIKClKacwoKClKacwoKClKacwoKClKacwoKClKacwoKClKac0oKCgpSmnMKCgpSmnMKCgpSmnMKCgqacwoKCppzCgoKmnMKCgqac0oKCgpSmnMKCgqacwoKCppzCgoKmnNKCgoKUppzCgoKUppzCgoKUppzSgoKClKacwoKCppzCgoKmnNKCgoKUppzCgoKClIKClKacwoKCgpSCgoKUgoKUppzCgoKUppzCgoKClIKClKacwoKCgpSCgpSmnMKCgoKUgoKUppzCgoKUlIKClKacwoKClKacwoKClJSCgpSmnMKCgpSUgoKUppzCgoKUlIKClKacwoKClKac0oKCgpSCgoKUppzSgoKClIKCgpSmnNKCgpSUgoKClKac0oKClJSCgoKUppzSgoKUlIKCgpSmnMKCgqac0oKCgpSCgpSUgoKClKacwoKCgpSCgoKUgoKUppzCgoKClIKClKacwoKCgpSCgpSmnNKCgoKUppzCgoKUppzCgoKUppzCgoKUppzCgoKUppzCgoKClIKClKacwoKClKacwoKClKacwoKClKacwoKClKacwoKClKacwoKClKac0oKCgpSmnMKCgpSmnMKCgpSmnMKCgoKUgoKClIKClKacwoKClKacwoKCgpSCgpSmnMKCgqacwoKCgpSCgpSmnMKCgoKUgoKUppzCgoKClIKClKac0oKClJSCgoKUppzSgoKUlIKCgpSmnNKCgoKUppzCgoKUppzSgoKClKacwoKClKacwoKClKacwoKClJSCgpSmnMKCgoKUgoKUppzSgoKClIKCgpSmnNKCgpSUgoKClKacwoKClKacwoKClKacwoKClKacwoKCgpSCgpSmnMKCgoKUgoKUppzCgoKClIKClKacwoKCgpSCgpSmnuKClIKClKY=")]
// </GoSourcePositionMaps>

// Dynamically imported C entry points are recorded here, one `GoCgoImportDynamic` attribute
// per `//go:cgo_import_dynamic` pragma this package binds to a trampoline declaration, so
// that `abi.FuncPCABI0` of that trampoline resolves to the REAL address of the exported
// symbol rather than to a token. The value is dereferenced by design - the trampoline's
// caller jumps to it - which is why a stub carrying no record here is left a loud throw
// instead: an address that is merely plausible is fatal at the first call.

// <CgoDynamicImports>
[assembly: go.GoCgoImportDynamic("libc_accept_trampoline", "accept", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_access_trampoline", "access", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_adjtime_trampoline", "adjtime", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_bind_trampoline", "bind", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_chdir_trampoline", "chdir", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_chflags_trampoline", "chflags", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_chmod_trampoline", "chmod", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_chown_trampoline", "chown", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_chroot_trampoline", "chroot", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_close_trampoline", "close", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_closedir_trampoline", "closedir", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_connect_trampoline", "connect", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_dup2_trampoline", "dup2", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_dup_trampoline", "dup", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_exchangedata_trampoline", "exchangedata", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_execve_trampoline", "execve", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_exit_trampoline", "exit", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_fchdir_trampoline", "fchdir", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_fchflags_trampoline", "fchflags", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_fchmod_trampoline", "fchmod", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_fchown_trampoline", "fchown", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_fcntl_trampoline", "fcntl", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_fdopendir_trampoline", "fdopendir", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_flock_trampoline", "flock", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_fork_trampoline", "fork", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_fpathconf_trampoline", "fpathconf", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_fstat64_trampoline", "fstat64", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_fstatat64_trampoline", "fstatat64", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_fstatfs64_trampoline", "fstatfs64", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_fsync_trampoline", "fsync", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_ftruncate_trampoline", "ftruncate", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_futimes_trampoline", "futimes", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_getcwd_trampoline", "getcwd", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_getdtablesize_trampoline", "getdtablesize", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_getegid_trampoline", "getegid", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_geteuid_trampoline", "geteuid", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_getfsstat_trampoline", "getfsstat", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_getgid_trampoline", "getgid", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_getgroups_trampoline", "getgroups", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_getpeername_trampoline", "getpeername", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_getpgid_trampoline", "getpgid", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_getpgrp_trampoline", "getpgrp", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_getpid_trampoline", "getpid", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_getppid_trampoline", "getppid", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_getpriority_trampoline", "getpriority", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_getrlimit_trampoline", "getrlimit", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_getrusage_trampoline", "getrusage", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_getsid_trampoline", "getsid", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_getsockname_trampoline", "getsockname", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_getsockopt_trampoline", "getsockopt", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_gettimeofday_trampoline", "gettimeofday", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_getuid_trampoline", "getuid", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_ioctl_trampoline", "ioctl", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_issetugid_trampoline", "issetugid", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_kevent_trampoline", "kevent", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_kill_trampoline", "kill", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_kqueue_trampoline", "kqueue", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_lchown_trampoline", "lchown", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_link_trampoline", "link", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_listen_trampoline", "listen", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_lseek_trampoline", "lseek", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_lstat64_trampoline", "lstat64", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_mkdir_trampoline", "mkdir", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_mkfifo_trampoline", "mkfifo", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_mknod_trampoline", "mknod", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_mlock_trampoline", "mlock", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_mlockall_trampoline", "mlockall", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_mmap_trampoline", "mmap", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_mprotect_trampoline", "mprotect", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_msync_trampoline", "msync", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_munlock_trampoline", "munlock", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_munlockall_trampoline", "munlockall", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_munmap_trampoline", "munmap", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_open_trampoline", "open", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_openat_trampoline", "openat", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_pathconf_trampoline", "pathconf", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_pipe_trampoline", "pipe", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_pread_trampoline", "pread", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_ptrace_trampoline", "ptrace", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_pwrite_trampoline", "pwrite", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_read_trampoline", "read", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_readdir_r_trampoline", "readdir_r", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_readlink_trampoline", "readlink", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_recvfrom_trampoline", "recvfrom", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_recvmsg_trampoline", "recvmsg", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_rename_trampoline", "rename", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_revoke_trampoline", "revoke", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_rmdir_trampoline", "rmdir", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_select_trampoline", "select", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_sendfile_trampoline", "sendfile", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_sendmsg_trampoline", "sendmsg", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_sendto_trampoline", "sendto", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_setegid_trampoline", "setegid", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_seteuid_trampoline", "seteuid", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_setgid_trampoline", "setgid", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_setgroups_trampoline", "setgroups", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_setlogin_trampoline", "setlogin", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_setpgid_trampoline", "setpgid", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_setpriority_trampoline", "setpriority", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_setprivexec_trampoline", "setprivexec", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_setregid_trampoline", "setregid", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_setreuid_trampoline", "setreuid", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_setrlimit_trampoline", "setrlimit", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_setsid_trampoline", "setsid", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_setsockopt_trampoline", "setsockopt", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_settimeofday_trampoline", "settimeofday", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_setuid_trampoline", "setuid", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_shutdown_trampoline", "shutdown", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_socket_trampoline", "socket", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_socketpair_trampoline", "socketpair", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_stat64_trampoline", "stat64", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_statfs64_trampoline", "statfs64", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_symlink_trampoline", "symlink", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_sync_trampoline", "sync", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_sysctl_trampoline", "sysctl", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_truncate_trampoline", "truncate", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_umask_trampoline", "umask", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_undelete_trampoline", "undelete", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_unlink_trampoline", "unlink", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_unlinkat_trampoline", "unlinkat", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_unmount_trampoline", "unmount", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_utimensat_trampoline", "utimensat", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_utimes_trampoline", "utimes", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_wait4_trampoline", "wait4", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_write_trampoline", "write", "/usr/lib/libSystem.B.dylib")]
[assembly: go.GoCgoImportDynamic("libc_writev_trampoline", "writev", "/usr/lib/libSystem.B.dylib")]
// </CgoDynamicImports>

namespace go;

[GoPackage("syscall")]
public static partial class syscall_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct _C_int {}
    internal partial struct _C_long {}
    internal partial struct _C_long_long {}
    internal partial struct _C_short {}
    internal partial struct _Gid_t {}
    internal partial struct anyMessage {}
    [GoValueClone("name")] internal partial struct ivalue {}
    internal partial struct mmapper {}
    [GoLocalName("linkLayerAddr")] internal partial struct parseLinkLayerAddr_linkLayerAddr {}
    public partial interface Conn {}
    public partial interface RawConn {}
    public partial interface RoutingMessage {}
    public partial interface Sockaddr {}
    [GoValueClone("Pad_cgo_0")] public partial struct BpfHdr {}
    public partial struct BpfInsn {}
    [GoValueClone("Pad_cgo_0")] public partial struct BpfProgram {}
    public partial struct BpfStat {}
    public partial struct BpfVersion {}
    public partial struct Cmsghdr {}
    public partial struct Credential {}
    [GoValueClone("Name", "Pad_cgo_0")] public partial struct Dirent {}
    public partial struct Errno {}
    public partial struct Fbootstraptransfer_t {}
    [GoValueClone("Bits")] public partial struct FdSet {}
    public partial struct Flock_t {}
    [GoValueClone("Val")] public partial struct Fsid {}
    public partial struct Fstore_t {}
    [GoValueClone("Filt")] public partial struct ICMPv6Filter {}
    [GoValueClone("Multiaddr", "Interface")] public partial struct IPMreq {}
    [GoValueClone("Addr")] public partial struct IPv6MTUInfo {}
    [GoValueClone("Multiaddr")] public partial struct IPv6Mreq {}
    public partial struct IfData {}
    [GoValueClone("Pad_cgo_0")] public partial struct IfMsghdr {}
    [GoValueClone("Pad_cgo_0")] public partial struct IfaMsghdr {}
    [GoValueClone("Pad_cgo_0")] public partial struct IfmaMsghdr {}
    [GoValueClone("Pad_cgo_0")] public partial struct IfmaMsghdr2 {}
    [GoValueClone("Spec_dst", "Addr")] public partial struct Inet4Pktinfo {}
    [GoValueClone("Addr")] public partial struct Inet6Pktinfo {}
    [GoValueClone("Header")] public partial struct InterfaceAddrMessage {}
    [GoValueClone("Header")] public partial struct InterfaceMessage {}
    [GoValueClone("Header")] public partial struct InterfaceMulticastAddrMessage {}
    public partial struct Iovec {}
    public partial struct Kevent_t {}
    public partial struct Linger {}
    public partial struct Log2phys_t {}
    [GoValueClone("Pad_cgo_0", "Pad_cgo_1")] public partial struct Msghdr {}
    public partial struct ProcAttr {}
    [GoValueClone("Pad_cgo_0")] public partial struct Radvisory_t {}
    [GoValueClone("Data")] public partial struct RawSockaddr {}
    [GoValueClone("Addr", "Pad")] public partial struct RawSockaddrAny {}
    [GoValueClone("Data")] public partial struct RawSockaddrDatalink {}
    [GoValueClone("Addr", "Zero")] public partial struct RawSockaddrInet4 {}
    [GoValueClone("Addr")] public partial struct RawSockaddrInet6 {}
    [GoValueClone("Path")] public partial struct RawSockaddrUnix {}
    public partial struct Rlimit {}
    [GoValueClone("Header")] public partial struct RouteMessage {}
    [GoValueClone("Filler")] public partial struct RtMetrics {}
    [GoValueClone("Pad_cgo_0", "Rmx")] public partial struct RtMsghdr {}
    [GoValueClone("Utime", "Stime")] public partial struct Rusage {}
    [GoValueClone("Data", "raw")] public partial struct SockaddrDatalink {}
    [GoValueClone("Addr", "raw")] public partial struct SockaddrInet4 {}
    [GoValueClone("Addr", "raw")] public partial struct SockaddrInet6 {}
    [GoValueClone("raw")] public partial struct SockaddrUnix {}
    public partial struct SocketControlMessage {}
    [GoValueClone("Pad_cgo_0", "Qspare")] public partial struct Stat_t {}
    [GoValueClone("Fsid", "Fstypename", "Mntonname", "Mntfromname", "Reserved")] public partial struct Statfs_t {}
    public partial struct SysProcAttr {}
    [GoValueClone("Cc", "Pad_cgo_0")] public partial struct Termios {}
    public partial struct Timespec {}
    [GoValueClone("Pad_cgo_0")] public partial struct Timeval {}
    public partial struct Timeval32 {}
    public partial struct WaitStatus {}
    public partial struct _Socklen {}
    public partial struct ΔSignal {}
    // </TypeAccessibility>
}
