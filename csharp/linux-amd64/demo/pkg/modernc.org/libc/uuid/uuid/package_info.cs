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
using static go.modernc.org.libc.uuid.uuid_package;

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
[assembly: GoDynamicTypeLift("7374727563747b465f5f30207374727563747b465f5f777365712075696e7436347d3b20465f5f38207374727563747b465f5f67315f73746172742075696e7436347d3b20465f5f675f72656673205b325d75696e7433323b20465f5f675f73697a65205b325d75696e7433323b20465f5f67315f6f7269675f73697a652075696e7433323b20465f5f77726566732075696e7433323b20465f5f675f7369676e616c73205b325d75696e7433327d", "X__pthread_cond_sᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b465f5f6363676f5f70616431205b305d75696e7433323b20465f5f73697a65205b345d696e74387d", "Pthread_mutexattr_tᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b465f5f6363676f5f70616431205b305d75696e7436343b20465f5f73697a65205b33325d696e74387d", "Pthread_barrier_tᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b465f5f6363676f5f70616431205b305d75696e7436343b20465f5f73697a65205b35365d696e74387d", "Pthread_attr_t1ᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b465f5f6363676f5f70616431205b305d75696e7436343b20465f5f73697a65205b385d696e74387d", "Pthread_rwlockattr_tᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b465f5f64617461206d6f6465726e632e6f72672f6c6962632f757569642f757569642e585f5f707468726561645f636f6e645f737d", "Pthread_cond_tᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b465f5f64617461206d6f6465726e632e6f72672f6c6962632f757569642f757569642e585f5f707468726561645f6d757465785f737d", "Pthread_mutex_tᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b465f5f64617461206d6f6465726e632e6f72672f6c6962632f757569642f757569642e585f5f707468726561645f72776c6f636b5f617263685f747d", "Pthread_rwlock_tᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b465f5f6664735f62697473205b31365d6d6f6465726e632e6f72672f6c6962632f757569642f757569642e585f5f66645f6d61736b7d", "Fd_setᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b465f5f67315f73746172742075696e7436347d", "X__pthread_cond_sᴛ1_F__8")]
[assembly: GoDynamicTypeLift("7374727563747b465f5f6c6f63616c6573205b31335d75696e747074723b20465f5f63747970655f622075696e747074723b20465f5f63747970655f746f6c6f7765722075696e747074723b20465f5f63747970655f746f75707065722075696e747074723b20465f5f6e616d6573205b31335d75696e747074727d", "X__locale_structᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b465f5f6c6f636b20696e7433323b20465f5f636f756e742075696e7433323b20465f5f6f776e657220696e7433323b20465f5f6e75736572732075696e7433323b20465f5f6b696e6420696e7433323b20465f5f7370696e7320696e7431363b20465f5f656c6973696f6e20696e7431363b20465f5f6c697374206d6f6465726e632e6f72672f6c6962632f757569642f757569642e585f5f707468726561645f6c6973745f747d", "X__pthread_mutex_sᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b465f5f6e6578742075696e747074727d", "X__pthread_internal_slistᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b465f5f707265762075696e747074723b20465f5f6e6578742075696e747074727d", "X__pthread_internal_listᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b465f5f726561646572732075696e7433323b20465f5f777269746572732075696e7433323b20465f5f777270686173655f66757465782075696e7433323b20465f5f777269746572735f66757465782075696e7433323b20465f5f706164332075696e7433323b20465f5f706164342075696e7433323b20465f5f6375725f77726974657220696e7433323b20465f5f73686172656420696e7433323b20465f5f7277656c6973696f6e20696e74383b20465f5f70616431205b375d75696e74383b20465f5f706164322075696e7436343b20465f5f666c6167732075696e7433323b20465f5f6363676f5f70616431205b345d627974657d", "X__pthread_rwlock_arch_tᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b465f5f76616c205b31365d75696e7436347d", "X__sigset_tᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b465f5f76616c205b325d696e7433327d", "X__fsid_tᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b465f5f777365712075696e7436347d", "X__pthread_cond_sᴛ1_F__0")]
[assembly: GoDynamicTypeLift("7374727563747b4669745f696e74657276616c207374727563747b4674765f736563206d6f6465726e632e6f72672f6c6962632f757569642f757569642e585f5f74696d655f743b204674765f6e736563206d6f6465726e632e6f72672f6c6962632f757569642f757569642e585f5f73797363616c6c5f736c6f6e675f747d3b204669745f76616c7565207374727563747b4674765f736563206d6f6465726e632e6f72672f6c6962632f757569642f757569642e585f5f74696d655f743b204674765f6e736563206d6f6465726e632e6f72672f6c6962632f757569642f757569642e585f5f73797363616c6c5f736c6f6e675f747d7d", "Itimerspecᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b4669745f696e74657276616c207374727563747b4674765f736563206d6f6465726e632e6f72672f6c6962632f757569642f757569642e585f5f74696d655f743b204674765f75736563206d6f6465726e632e6f72672f6c6962632f757569642f757569642e585f5f73757365636f6e64735f747d3b204669745f76616c7565207374727563747b4674765f736563206d6f6465726e632e6f72672f6c6962632f757569642f757569642e585f5f74696d655f743b204674765f75736563206d6f6465726e632e6f72672f6c6962632f757569642f757569642e585f5f73757365636f6e64735f747d7d", "Itimervalᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b466c6f20696e7436343b2046686920696e7436347d", "X__int128_tᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b466c6f2075696e7436343b204668692075696e7436347d", "X__uint128_tᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b46746d5f73656320696e7433323b2046746d5f6d696e20696e7433323b2046746d5f686f757220696e7433323b2046746d5f6d64617920696e7433323b2046746d5f6d6f6e20696e7433323b2046746d5f7965617220696e7433323b2046746d5f7764617920696e7433323b2046746d5f7964617920696e7433323b2046746d5f697364737420696e7433323b20465f5f6363676f5f70616431205b345d627974653b2046746d5f676d746f666620696e7436343b2046746d5f7a6f6e652075696e747074727d", "Tmᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b4674765f736563206d6f6465726e632e6f72672f6c6962632f757569642f757569642e585f5f74696d655f743b204674765f6e736563206d6f6465726e632e6f72672f6c6962632f757569642f757569642e585f5f73797363616c6c5f736c6f6e675f747d", "Timespecᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b4674765f736563206d6f6465726e632e6f72672f6c6962632f757569642f757569642e585f5f74696d655f743b204674765f75736563206d6f6465726e632e6f72672f6c6962632f757569642f757569642e585f5f73757365636f6e64735f747d", "Timevalᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b46747a5f6d696e757465737765737420696e7433323b2046747a5f64737474696d6520696e7433327d", "Timezoneᴛ1")]
[assembly: GoTypeAlias("Blkcnt_t", "long")]
[assembly: GoTypeAlias("Blksize_t", "long")]
[assembly: GoTypeAlias("Caddr_t", "go.uintptr")]
[assembly: GoTypeAlias("Clock_t", "long")]
[assembly: GoTypeAlias("Clockid_t", "int")]
[assembly: GoTypeAlias("Daddr_t", "int")]
[assembly: GoTypeAlias("Dev_t", "ulong")]
[assembly: GoTypeAlias("Fd_mask", "long")]
[assembly: GoTypeAlias("Fd_set", "go.modernc.org.libc.uuid.uuid_package.Fd_setᴛ1")]
[assembly: GoTypeAlias("Fsblkcnt_t", "ulong")]
[assembly: GoTypeAlias("Fsfilcnt_t", "ulong")]
[assembly: GoTypeAlias("Fsid_t", "go.modernc.org.libc.uuid.uuid_package.X__fsid_tᴛ1")]
[assembly: GoTypeAlias("Gid_t", "uint")]
[assembly: GoTypeAlias("Id_t", "uint")]
[assembly: GoTypeAlias("Ino_t", "ulong")]
[assembly: GoTypeAlias("Int16_t", "short")]
[assembly: GoTypeAlias("Int32_t", "int")]
[assembly: GoTypeAlias("Int64_t", "long")]
[assembly: GoTypeAlias("Int8_t", "sbyte")]
[assembly: GoTypeAlias("Itimerspec", "go.modernc.org.libc.uuid.uuid_package.Itimerspecᴛ1")]
[assembly: GoTypeAlias("Itimerval", "go.modernc.org.libc.uuid.uuid_package.Itimervalᴛ1")]
[assembly: GoTypeAlias("Key_t", "int")]
[assembly: GoTypeAlias("Locale_t", "go.uintptr")]
[assembly: GoTypeAlias("Loff_t", "long")]
[assembly: GoTypeAlias("Mode_t", "uint")]
[assembly: GoTypeAlias("Nlink_t", "ulong")]
[assembly: GoTypeAlias("Off_t", "long")]
[assembly: GoTypeAlias("Pid_t", "int")]
[assembly: GoTypeAlias("Pthread_attr_t", "go.modernc.org.libc.uuid.uuid_package.Pthread_attr_t1ᴛ1")]
[assembly: GoTypeAlias("Pthread_attr_t1", "go.modernc.org.libc.uuid.uuid_package.Pthread_attr_t1ᴛ1")]
[assembly: GoTypeAlias("Pthread_barrier_t", "go.modernc.org.libc.uuid.uuid_package.Pthread_barrier_tᴛ1")]
[assembly: GoTypeAlias("Pthread_barrierattr_t", "go.modernc.org.libc.uuid.uuid_package.Pthread_mutexattr_tᴛ1")]
[assembly: GoTypeAlias("Pthread_cond_t", "go.modernc.org.libc.uuid.uuid_package.Pthread_cond_tᴛ1")]
[assembly: GoTypeAlias("Pthread_condattr_t", "go.modernc.org.libc.uuid.uuid_package.Pthread_mutexattr_tᴛ1")]
[assembly: GoTypeAlias("Pthread_key_t", "uint")]
[assembly: GoTypeAlias("Pthread_mutex_t", "go.modernc.org.libc.uuid.uuid_package.Pthread_mutex_tᴛ1")]
[assembly: GoTypeAlias("Pthread_mutexattr_t", "go.modernc.org.libc.uuid.uuid_package.Pthread_mutexattr_tᴛ1")]
[assembly: GoTypeAlias("Pthread_once_t", "int")]
[assembly: GoTypeAlias("Pthread_rwlock_t", "go.modernc.org.libc.uuid.uuid_package.Pthread_rwlock_tᴛ1")]
[assembly: GoTypeAlias("Pthread_rwlockattr_t", "go.modernc.org.libc.uuid.uuid_package.Pthread_rwlockattr_tᴛ1")]
[assembly: GoTypeAlias("Pthread_spinlock_t", "int")]
[assembly: GoTypeAlias("Pthread_t", "ulong")]
[assembly: GoTypeAlias("Ptrdiff_t", "long")]
[assembly: GoTypeAlias("Quad_t", "long")]
[assembly: GoTypeAlias("Register_t", "int")]
[assembly: GoTypeAlias("Sigset_t", "go.modernc.org.libc.uuid.uuid_package.X__sigset_tᴛ1")]
[assembly: GoTypeAlias("Size_t", "ulong")]
[assembly: GoTypeAlias("Ssize_t", "long")]
[assembly: GoTypeAlias("Suseconds_t", "long")]
[assembly: GoTypeAlias("Time_t", "long")]
[assembly: GoTypeAlias("Timer_t", "go.uintptr")]
[assembly: GoTypeAlias("Timespec", "go.modernc.org.libc.uuid.uuid_package.Timespecᴛ1")]
[assembly: GoTypeAlias("Timeval", "go.modernc.org.libc.uuid.uuid_package.Timevalᴛ1")]
[assembly: GoTypeAlias("Timezone", "go.modernc.org.libc.uuid.uuid_package.Timezoneᴛ1")]
[assembly: GoTypeAlias("Tm", "go.modernc.org.libc.uuid.uuid_package.Tmᴛ1")]
[assembly: GoTypeAlias("U_char", "byte")]
[assembly: GoTypeAlias("U_int", "uint")]
[assembly: GoTypeAlias("U_int16_t", "ushort")]
[assembly: GoTypeAlias("U_int32_t", "uint")]
[assembly: GoTypeAlias("U_int64_t", "ulong")]
[assembly: GoTypeAlias("U_int8_t", "byte")]
[assembly: GoTypeAlias("U_long", "ulong")]
[assembly: GoTypeAlias("U_quad_t", "ulong")]
[assembly: GoTypeAlias("U_short", "ushort")]
[assembly: GoTypeAlias("Uid_t", "uint")]
[assembly: GoTypeAlias("Uint", "uint")]
[assembly: GoTypeAlias("Ulong", "ulong")]
[assembly: GoTypeAlias("Ushort", "ushort")]
[assembly: GoTypeAlias("Uuid_t", "go.array<byte>")]
[assembly: GoTypeAlias("Wchar_t", "int")]
[assembly: GoTypeAlias("X__blkcnt64_t", "long")]
[assembly: GoTypeAlias("X__blkcnt_t", "long")]
[assembly: GoTypeAlias("X__blksize_t", "long")]
[assembly: GoTypeAlias("X__builtin_va_list", "go.uintptr")]
[assembly: GoTypeAlias("X__caddr_t", "go.uintptr")]
[assembly: GoTypeAlias("X__clock_t", "long")]
[assembly: GoTypeAlias("X__clockid_t", "int")]
[assembly: GoTypeAlias("X__daddr_t", "int")]
[assembly: GoTypeAlias("X__dev_t", "ulong")]
[assembly: GoTypeAlias("X__fd_mask", "long")]
[assembly: GoTypeAlias("X__float128", "double")]
[assembly: GoTypeAlias("X__fsblkcnt64_t", "ulong")]
[assembly: GoTypeAlias("X__fsblkcnt_t", "ulong")]
[assembly: GoTypeAlias("X__fsfilcnt64_t", "ulong")]
[assembly: GoTypeAlias("X__fsfilcnt_t", "ulong")]
[assembly: GoTypeAlias("X__fsid_t", "go.modernc.org.libc.uuid.uuid_package.X__fsid_tᴛ1")]
[assembly: GoTypeAlias("X__fsword_t", "long")]
[assembly: GoTypeAlias("X__gid_t", "uint")]
[assembly: GoTypeAlias("X__id_t", "uint")]
[assembly: GoTypeAlias("X__ino64_t", "ulong")]
[assembly: GoTypeAlias("X__ino_t", "ulong")]
[assembly: GoTypeAlias("X__int128_t", "go.modernc.org.libc.uuid.uuid_package.X__int128_tᴛ1")]
[assembly: GoTypeAlias("X__int16_t", "short")]
[assembly: GoTypeAlias("X__int32_t", "int")]
[assembly: GoTypeAlias("X__int64_t", "long")]
[assembly: GoTypeAlias("X__int8_t", "sbyte")]
[assembly: GoTypeAlias("X__int_least16_t", "short")]
[assembly: GoTypeAlias("X__int_least32_t", "int")]
[assembly: GoTypeAlias("X__int_least64_t", "long")]
[assembly: GoTypeAlias("X__int_least8_t", "sbyte")]
[assembly: GoTypeAlias("X__intmax_t", "long")]
[assembly: GoTypeAlias("X__intptr_t", "long")]
[assembly: GoTypeAlias("X__itimer_which_t", "int")]
[assembly: GoTypeAlias("X__key_t", "int")]
[assembly: GoTypeAlias("X__locale_struct", "go.modernc.org.libc.uuid.uuid_package.X__locale_structᴛ1")]
[assembly: GoTypeAlias("X__locale_t", "go.uintptr")]
[assembly: GoTypeAlias("X__loff_t", "long")]
[assembly: GoTypeAlias("X__mode_t", "uint")]
[assembly: GoTypeAlias("X__nlink_t", "ulong")]
[assembly: GoTypeAlias("X__off64_t", "long")]
[assembly: GoTypeAlias("X__off_t", "long")]
[assembly: GoTypeAlias("X__pid_t", "int")]
[assembly: GoTypeAlias("X__pthread_cond_s", "go.modernc.org.libc.uuid.uuid_package.X__pthread_cond_sᴛ1")]
[assembly: GoTypeAlias("X__pthread_internal_list", "go.modernc.org.libc.uuid.uuid_package.X__pthread_internal_listᴛ1")]
[assembly: GoTypeAlias("X__pthread_internal_slist", "go.modernc.org.libc.uuid.uuid_package.X__pthread_internal_slistᴛ1")]
[assembly: GoTypeAlias("X__pthread_list_t", "go.modernc.org.libc.uuid.uuid_package.X__pthread_internal_listᴛ1")]
[assembly: GoTypeAlias("X__pthread_mutex_s", "go.modernc.org.libc.uuid.uuid_package.X__pthread_mutex_sᴛ1")]
[assembly: GoTypeAlias("X__pthread_rwlock_arch_t", "go.modernc.org.libc.uuid.uuid_package.X__pthread_rwlock_arch_tᴛ1")]
[assembly: GoTypeAlias("X__pthread_slist_t", "go.modernc.org.libc.uuid.uuid_package.X__pthread_internal_slistᴛ1")]
[assembly: GoTypeAlias("X__quad_t", "long")]
[assembly: GoTypeAlias("X__rlim64_t", "ulong")]
[assembly: GoTypeAlias("X__rlim_t", "ulong")]
[assembly: GoTypeAlias("X__sig_atomic_t", "int")]
[assembly: GoTypeAlias("X__sigset_t", "go.modernc.org.libc.uuid.uuid_package.X__sigset_tᴛ1")]
[assembly: GoTypeAlias("X__socklen_t", "uint")]
[assembly: GoTypeAlias("X__ssize_t", "long")]
[assembly: GoTypeAlias("X__suseconds_t", "long")]
[assembly: GoTypeAlias("X__syscall_slong_t", "long")]
[assembly: GoTypeAlias("X__syscall_ulong_t", "ulong")]
[assembly: GoTypeAlias("X__time_t", "long")]
[assembly: GoTypeAlias("X__timer_t", "go.uintptr")]
[assembly: GoTypeAlias("X__u_char", "byte")]
[assembly: GoTypeAlias("X__u_int", "uint")]
[assembly: GoTypeAlias("X__u_long", "ulong")]
[assembly: GoTypeAlias("X__u_quad_t", "ulong")]
[assembly: GoTypeAlias("X__u_short", "ushort")]
[assembly: GoTypeAlias("X__uid_t", "uint")]
[assembly: GoTypeAlias("X__uint128_t", "go.modernc.org.libc.uuid.uuid_package.X__uint128_tᴛ1")]
[assembly: GoTypeAlias("X__uint16_t", "ushort")]
[assembly: GoTypeAlias("X__uint32_t", "uint")]
[assembly: GoTypeAlias("X__uint64_t", "ulong")]
[assembly: GoTypeAlias("X__uint8_t", "byte")]
[assembly: GoTypeAlias("X__uint_least16_t", "ushort")]
[assembly: GoTypeAlias("X__uint_least32_t", "uint")]
[assembly: GoTypeAlias("X__uint_least64_t", "ulong")]
[assembly: GoTypeAlias("X__uint_least8_t", "byte")]
[assembly: GoTypeAlias("X__uintmax_t", "ulong")]
[assembly: GoTypeAlias("X__useconds_t", "uint")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
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
[assembly: GoPositionMap("modernc.org/libc@v1.80.1/uuid/uuid/capi_linux_amd64.go", "capi_linux_amd64.cs", "yg==")]
// </GoSourcePositionMaps>

namespace go.modernc.org.libc.uuid;

[GoPackage("uuid", ImportPath = "modernc.org/libc/uuid/uuid")]
public static partial class uuid_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    [GoValueClone("F__fds_bits")] public partial struct Fd_setᴛ1 {}
    public partial struct Itimerspecᴛ1 {}
    public partial struct Itimervalᴛ1 {}
    [GoValueClone("F__size")] public partial struct Pthread_attr_t1ᴛ1 {}
    [GoValueClone("F__size")] public partial struct Pthread_barrier_tᴛ1 {}
    [GoValueClone("F__data")] public partial struct Pthread_cond_tᴛ1 {}
    public partial struct Pthread_mutex_tᴛ1 {}
    [GoValueClone("F__size")] public partial struct Pthread_mutexattr_tᴛ1 {}
    [GoValueClone("F__data")] public partial struct Pthread_rwlock_tᴛ1 {}
    [GoValueClone("F__size")] public partial struct Pthread_rwlockattr_tᴛ1 {}
    public partial struct Timespecᴛ1 {}
    public partial struct Timevalᴛ1 {}
    public partial struct Timezoneᴛ1 {}
    [GoValueClone("F__ccgo_pad1")] public partial struct Tmᴛ1 {}
    [GoValueClone("F__val")] public partial struct X__fsid_tᴛ1 {}
    public partial struct X__int128_tᴛ1 {}
    [GoValueClone("F__locales", "F__names")] public partial struct X__locale_structᴛ1 {}
    [GoValueClone("F__g_refs", "F__g_size", "F__g_signals")] public partial struct X__pthread_cond_sᴛ1 {}
    public partial struct X__pthread_cond_sᴛ1_F__0 {}
    public partial struct X__pthread_cond_sᴛ1_F__8 {}
    public partial struct X__pthread_internal_listᴛ1 {}
    public partial struct X__pthread_internal_slistᴛ1 {}
    public partial struct X__pthread_mutex_sᴛ1 {}
    [GoValueClone("F__pad1", "F__ccgo_pad1")] public partial struct X__pthread_rwlock_arch_tᴛ1 {}
    [GoValueClone("F__val")] public partial struct X__sigset_tᴛ1 {}
    public partial struct X__uint128_tᴛ1 {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸsyncꓸatomic() => builtin.initPackage(typeof(sync.atomic_package));
    // </ImportInitializers>
}
