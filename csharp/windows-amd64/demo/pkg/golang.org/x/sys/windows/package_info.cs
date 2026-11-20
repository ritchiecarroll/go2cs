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
using static go.golang.org.x.sys.windows_package;

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
[assembly: GoDynamicTypeLift("7374727563747b6f6e63652073796e632e4f6e63653b20616464722075696e747074723b20657272206572726f727d", "connectExFuncᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6f6e63652073796e632e4f6e63653b2073656e64416464722075696e747074723b2072656376416464722075696e747074723b20657272206572726f727d", "sendRecvMsgFuncᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b73796e632e4f6e63653b207620626f6f6c7d", "canDoSearchSystem32Onceᴛ1")]
[assembly: GoTypeAlias("Errno", "go.syscall_package.Errno")]
[assembly: GoTypeAlias("ResourceIDOrString", "object")]
[assembly: GoTypeAlias("Signal", "go.syscall_package.Signal")]
[assembly: GoTypeAlias("SysProcAttr", "go.syscall_package.SysProcAttr")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<CONFIGRET, error>]
[assembly: GoImplement<DLLError, error>(Pointer = true)]
[assembly: GoImplement<NTStatus, error>]
[assembly: GoImplement<SockaddrBth, Sockaddr>(Pointer = true)]
[assembly: GoImplement<SockaddrInet4, Sockaddr>(Pointer = true)]
[assembly: GoImplement<SockaddrInet6, Sockaddr>(Pointer = true)]
[assembly: GoImplement<SockaddrUnix, Sockaddr>(Pointer = true)]
// </InterfaceImplementations>

// The struct types whose arrays this package holds inline (the go.arrays family), as the converter
// admitted them: a `GoInlineArrayType` record per named type, a `GoInlineArrayKeys` record with the
// count and a digest of every admitted type (anonymous ones included), a `GoInlineArrayAnonymous`
// record per admitted anonymous type, a `GoInlineArraySeed` record per -inline-arrays seed, and a
// `GoInlineArrays` record naming the rule when it is not the default. A -tests conversion applies the
// same rule and seeds rather than deciding again, and refuses a production set that admits differently.
// The section exists only while there is a record to hold.
// <InlineArrays>
[assembly: go.golib.GoInlineArrayKeys(21, "a38d7502f80d57c6")]
[assembly: go.golib.GoInlineArrayType("DEVPROPGUID")]
[assembly: go.golib.GoInlineArrayType("DEVPROPKEY")]
[assembly: go.golib.GoInlineArrayType("DevInfoData")]
[assembly: go.golib.GoInlineArrayType("GUID")]
[assembly: go.golib.GoInlineArrayType("IPMreq")]
[assembly: go.golib.GoInlineArrayType("IpAddressPrefix")]
[assembly: go.golib.GoInlineArrayType("KNOWNFOLDERID")]
[assembly: go.golib.GoInlineArrayType("MibIfRow")]
[assembly: go.golib.GoInlineArrayType("MibIfRow2")]
[assembly: go.golib.GoInlineArrayType("MibIpForwardRow2")]
[assembly: go.golib.GoInlineArrayType("MibIpInterfaceRow")]
[assembly: go.golib.GoInlineArrayType("ModuleEntry32")]
[assembly: go.golib.GoInlineArrayType("OsVersionInfoEx")]
[assembly: go.golib.GoInlineArrayType("ProcessEntry32")]
[assembly: go.golib.GoInlineArrayType("RawSockaddr")]
[assembly: go.golib.GoInlineArrayType("RawSockaddrInet")]
[assembly: go.golib.GoInlineArrayType("SidIdentifierAuthority")]
[assembly: go.golib.GoInlineArrayType("Systemtime")]
[assembly: go.golib.GoInlineArrayType("Timezoneinformation")]
[assembly: go.golib.GoInlineArrayType("WSAProtocolChain")]
[assembly: go.golib.GoInlineArrayType("WSAProtocolInfo")]
// </InlineArrays>

// <ImplicitConversions>
// </ImplicitConversions>

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: GoFieldAccess(typeof(ACL))]
[assembly: GoFieldAccess(typeof(ClassInstallHeader))]
[assembly: GoFieldAccess(typeof(DLL))]
[assembly: GoFieldAccess(typeof(DevInfoData))]
[assembly: GoFieldAccess(typeof(DevInfoListDetailData))]
[assembly: GoFieldAccess(typeof(DevInstallParams))]
[assembly: GoFieldAccess(typeof(DrvInfoData))]
[assembly: GoFieldAccess(typeof(DrvInfoDetailData))]
[assembly: GoFieldAccess(typeof(LazyDLL))]
[assembly: GoFieldAccess(typeof(LazyProc))]
[assembly: GoFieldAccess(typeof(OsVersionInfoEx))]
[assembly: GoFieldAccess(typeof(ProcThreadAttributeListContainer))]
[assembly: GoFieldAccess(typeof(ProcessEntry32))]
[assembly: GoFieldAccess(typeof(RawSockaddrInet4))]
[assembly: GoFieldAccess(typeof(RawSockaddrInet6))]
[assembly: GoFieldAccess(typeof(RawSockaddrUnix))]
[assembly: GoFieldAccess(typeof(SECURITY_DESCRIPTOR))]
[assembly: GoFieldAccess(typeof(SockaddrBth))]
[assembly: GoFieldAccess(typeof(SockaddrInet4))]
[assembly: GoFieldAccess(typeof(SockaddrInet6))]
[assembly: GoFieldAccess(typeof(SockaddrUnix))]
[assembly: GoFieldAccess(typeof(mountPointReparseBuffer))]
[assembly: GoFieldAccess(typeof(reparseDataBuffer))]
[assembly: GoFieldAccess(typeof(symbolicLinkReparseBuffer))]
[assembly: GoFieldAccess(typeof(syscall_package.RawSockaddrAny))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("golang.org/x/sys@v0.49.0/windows/dll_windows.go", "dll_windows.cs", "ABAqxgAMEpQACBqigoKUooLcuKiCgoKUyrKCgpSigtzKqIKCgpSqopKCgtzKqIKCgpSoggAMGIIAAhiiAA4q1oKUgoKCrIKCgpSUgqqC2IKCgrqigqiCzoKsggALINaCgoKCgoKUgoKYptiCgoK+ooIAAhiyggAMEAAAEqaCgqaCgoKmAAIUgoKCgoKCioKClKaSgpQ=", "", "", "57=Error/1/1/3;92=Error/1/1/3;121=Error/1/1/3")]
[assembly: GoPositionMap("golang.org/x/sys@v0.49.0/windows/env_windows.go", "env_windows.cs", "AAwcgqaCpoKmgszigoKClIKChIKCpoKClAAJBoI=")]
[assembly: GoPositionMap("golang.org/x/sys@v0.49.0/windows/exec_windows.go", "exec_windows.cs", "AAwugoKUgoKClLTGgpSCpoKCgoKUgoKUgrSCtIKClIKCtJSCgoKUgpSsooIAAhKCgoKCjIKmgoKCgoKCiJSUlIaUpoKIlK7CgpSCgpSCgoKUlIKClAAGGrKCgqaCqKKCgpSCgoKCgpSCzIKCgoKClJSCgpaCgoKUyIKCqIKCgoKogg==")]
[assembly: GoPositionMap("golang.org/x/sys@v0.49.0/windows/race0.go", "race0.cs", "AAwepqam")]
[assembly: GoPositionMap("golang.org/x/sys@v0.49.0/windows/security_windows.go", "security_windows.cs", "ABxAgoKClIKCgoKClIKUggAfWJKSkpKSkgA4pAGigoKClIKClILcsoKUgoKUgoKCgqaCgoKCgoKCgpSClILsooKCgpSC2IKogoKCgoKUqIKogqqCgpSogqiCqIKssoKCgoKmgoKCgoKCgpSClIIAgQGKAoKqgoKCgoKCgpSClIIAc4wCggALEoIACg6CAAZKgoKCrIKsgqyCyIKogoKCgoKClIKUgsyCgoKUqIKCgpSsgoKClKqCgoKCgoKUgpSCzIKCgoKClKiCgoKCgpTKgoKCgoKClIKUAAMQgoKCgoKClIKUvIKCgoKCgpSClNqCgoKUqLKChJQA2wHGBIKsgqyCrIIAG3rCgqiCqLKCqKKswoKCgpSogqzCgoKClKiCqMKCqIKowoKogqiCqIKqooKCgpSC2LKCgpSCgpTClMikpIKCgqa8gpSClIKUggABEJSEmIKClIKUgpSClIKUgpSCgoKUqLKCgpSCgpSCgsikpIKUgqaCgoKCpoqCgoKCysKCgoKUgurCgoKClILswoKCgpSClILswoKCgoKUgoKUgoKUguiygoKqwoKClIKCgpSCgoI=")]
[assembly: GoPositionMap("golang.org/x/sys@v0.49.0/windows/setupapi_windows.go", "setupapi_windows.cs", "AJoB0gKChJSmgqaCgoKUggA9fIKmgoKClIIAS+QBgoKCAC90gqaCgoKUgqaCpoKCgpSCpoKmgoKClIKogoKUgqaClIKmgpSCpgASIIKElKaCpoKmgqaCgoKmpoKUgoKCgoKClLgADAaCgoqogoKClIKCgrgAe8ACgoKUpoKmgoKCgpQAcvQBsoKCgoKmrIKClMiCzLKCgqaCgoKCuIKUyILMgoKUyIKugqyCrILMgoKUyILMgoKUyIKsgsyCgoKCgoKCgpSClILagqyCzLKCgoKCpoKCgoKmzoKugsyygoKCgoKClIKUlIKCpL6ygoKCgoKClIKUuIKUgoKkgoKUgoKUgoKCgpSClMakpKSCgoKCgpSUgqSkAAoKgooACAiCiqaCgoKmyILMgsiCqKKCgpSCgsyCgpTIgsyCgoKCgoKUgpTagq6CrIKugsyylIKCgoK4goKmgqyCgoKmgoKCgriCgoKCgpSClL6CgpTIgq6CzIKCgpQAAhCCgoKUgoKCgIKkgoCCgoK2goKCgpSUgpSqgoKClA==")]
[assembly: GoPositionMap("golang.org/x/sys@v0.49.0/windows/str.go", "str.cs", "AAgSgoKUgoKCgoKUgg==")]
[assembly: GoPositionMap("golang.org/x/sys@v0.49.0/windows/syscall.go", "syscall.cs", "AA5KgoKUgoKsgoKClKqCgpSsooKUgqiCgqbugsaCxoLGgg==")]
[assembly: GoPositionMap("golang.org/x/sys@v0.49.0/windows/syscall_windows.go", "syscall_windows.cs", "AEGaAYKCgpSsgqqCrJqCgoKUrKKClIKogoKUppqCrIIAAqYFgqoAARCCqpiygoKClKaUgoKCgqaigpSCgpSClLS0tIKUgoKUgoKClIKUtLS0tLSCgpSCpoKCgoKElJSmgoKUgoKClKaigoKClJSmooKUgoKU2oKClLS0tIKEgoKUgoKUpoLIoqKWgoLKgoKCgpSmooKClKaigoKUpqKCgpSmooKClKaigoKUgoKUpoKSgoKClKaC5oKIgqaigoKCpoKClJKCgpSCgqaygpSCgpSmgpSCgoLmsoKUgoKUpoKUgoKC5oKmgoKClIKClIKUlKaCpoKmgqaKgoKUADLoAaKClIKCgoKCAA0UooKUgoKCgoKCABAaooKCgpSClIKCloKClISEpgAUJKKC3MailIKCjAACEIKClIKmgoKCgoKmgoKCgoKCpMaCgpSmgoKmooKClKaigoKUpqKCgpSmsoKCgpSmsoKCgpSmgqaCpqKCgoKCgqamggALEoKigoKClIKCAAgOkqaiAAgCgoKUpqaCgoKUgoKUAAwUgqKCgoKUgoLugpQACA6SpoKCgpQACAKClKaCgoKUAAgCgpQADx6UlJSUlJSUlAAIEpSigoKqlMKCgoKCgpSCpqKCgpSmABU6goKCgqaigqaipoKmgqaGgoKUgqaUAAMSgoKClKaigoKClKaigoKUgoKCgpSCgpSCgvqCgoKUqIKClIKCgpSCgoKClIKowpSClJSCgoKCpoKClIKCtIKCuKSU6oKCgoKUgoKUqIKCgoKUyqKCgoKUyoLKooKCgpSC+oKCioKqwoKCqIKogqiCqIKmgoKCgoKCgpSClIKClIKUgoKCgoKmuKKmgoLGgsa0goKCgpaUzoKCgpSCgpQACBCGpoLOgoKCgpaClILIgoKmgsiCkpKCgpSCgpSklJSCgpSCgpSCgoKmsoKClIKClIKClIIABhCCqIKqgqqCqIKqgqqCqoKogoKCpoIACRimqKY=", "1189-1204:1;1240-1265:1;1758-1770:1")]
[assembly: GoPositionMap("golang.org/x/sys@v0.49.0/windows/types_windows.go", "types_windows.cs", "ADFkAL8E/gqSkpQAECyCxqKCggAYMoSEhILGpISEgoIAHE6CgoKCgoKCgpaCANwG4A7+/gCHAtYEgoKCgpQA7QGaBII=")]
[assembly: GoPositionMap("golang.org/x/sys@v0.49.0/windows/zknownfolderids_windows.go", "zknownfolderids_windows.cs", "AAgQgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoI=")]
[assembly: GoPositionMap("golang.org/x/sys@v0.49.0/windows/zsyscall_windows.go", "zsyscall_windows.cs", "AA4ygpSkqqiCgoKCgoKCgoKCgoKCgoKCgoKCgoKChIKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKWosKCpqLigqai4oKmooKCpqKCgpTigpSmooKClOKClKaiwoKUpqIADgKClKaiooKUpqIADgKClKaiwoKUpqKCgpSmoqKClKai4oKUpqLCgpSmooKCgpSmouKClKaiwoKUpqLCgpSmooKClAAQAoKUprIAEAKCgpSmouKClKai4oKUpqKigpSmooKClKaigoKUpqKCgpSmosKClKai4oKUpqIACgKClKaiwoKmoqKClKaiwoKUpqKigqaigoKClKaiAAwCgpSmouKClKaigoKUgoKUAAgCgoKClKaigoKU4oKClKaiooKmooKClOKCgpSmosKClKaigoKUgoKUAAgCgoKClKaiAAoCgpSmoqKCpqKigqaiooKmosKClKaigoKUpqKigpSmooKClIKClMKClKaygoKClKaiooKmoqKCpqKigqaiAA4CgpSmogAOAoKUpqLigpSmogAWAoKUpqLigpSmoqKClKaiooKUprLCgoKUprKigoKUpqKCgpSigpSmosKClKaiwoKUpqKCgpSigpSmosKClKaiooKUpqLCgpSmooKClKaiAAwCgpSmooKClIKClIKClKaiwoKUpqIAFgKClKaiAAoCgpSmssKCgpSmsqKCgpSmosKClKaigoKUpqLigpSmoqKClKaigoKClKaiAAoCgpSmoqKClKaigoKUgoKUwoKUpqKCgpTCgpSmooKClMKClKaCwqaigoKUgoKUwoKUpqIACAKClKaiooKUpqKigpSmoqKClKaiooKUpqKigpSmosKClKaigoKUprKigoKUpqKigpSmoqKCprKigoKUprLCgoKUprLCgoKUpqLCgqaCoqaiooKUpqIACAKClKai4oKmsoKCgpSmsqKCgpSmouKClKaygoKUAAoCgoKUpqIACAKClKaiAAoCgpSmogAOAoKUpqIACgKClKaywoKClKaiwoKmooKCgpSmogAIAoKUpoKipqKigpSmoqKClKaigoKUpoKipqLCgpSmosKClKaiwoKUpqKigpSmoqKClKaiooKUpqKigpSmoqKClKaiooKUpqKigpSmoqKClKaiooKUpqKCgpTCgpSmooKClMKClKaigoKUwoKUprKigoKUpqKCgpSmooKClKaiooKUpqKCgpSmosKClKaigoKUpoKCpqKigpSmosKClKaywoKClKaywoKClKaywoKClKaywoKClKaiwoKUprKCgoKUprLCgoKUprLCgoKUprKCgpTCgoKUprLCgoKUpqLigpSmooKClAAQAoKUpqKigpSmosKClKaygoKClKaiwoKUpqKigpSmgqKmoqKClKaiAAgCgpSmooKClKaigoKUooKUpqKCgpSmgoKmssKCgpSmooKClKaigoKUprKCgoKUprKCgpSigoKUprLCgoKUprLCgoKUprKigoKUpqKCgpSmoqKClKaiooKUpqKigpSmsoKCgpSmooKClKaigoKUpqKCgpSmooKClKaigoKUprKCgpTCgoKUpqKigpSmooKClKaigoKUpqKCgqaigoKmoqKClKaiooKUpqKigpSmooKCpqLCgpSmosKClKaygoKClKaiooKUprKCgoKUpqKigpSmsqKCgpSmooKCpqKCgqaiAAgCgpSmoqKCprKCgoKUprLCgoKUpqKigpSmosKClKayooKClKaiooKUpqKigpSmouKClKaygoKClKayooKClKay4oKClKaigoKmooKClKayooKClKaygoKClKaywoKClKaigoKmsqKCgpSmosKClKaiooKUpqIACgKClKaiAAgCgpSmoqKClKaiooKUpqKCgpTCgpSmsoKCgpSmsoKCgpSmsqKCgpSmsoKCgpSmouKClKaiwoKUpqIACAKClKaC4qai4oKUprLCgoKUpoKiprKCgoKUprKigoKUpqLigpSmgqKmgqKmsqKCgpSmsqKCgpSmouKClKaigoKmsqKCgpSmouKClKaygoKClKaiAAoCgpSmogAMAoKUpqLCgpSmosKClKai4oKUprKigoKUpqLCgpSmooKCprKCgpSigoKUpqKCgpTCgpSmsoKCgpSmsqKCgpSmsoKCgpSmsqKCgpSmsoKCgpSmsoKCgpSmsoKCgpSmoqKClKaygoKClKaygoKClKaiooKUpqKigpSmosKClKaiwoKUprLCgoKUprKCgpSigoKUprKCgpSigoKUprKCgpSCgoKUprKCgpSCgoKUpqKigpSmoqKClKaiooKUpqKigpSmooKClKaigoKUprLCgoKUpqLCgpSmoqKClKai4oKUpqKCgpTigpSmooKClOKClKaiwoKUpqKCgpSmoqKClKaigoKUpqKCgpSmooKClKaygoKClKaigoKUpqKCgpSmoqKClKaiooKUpqKCgpSmooKClKaigoKUpqKCgpSmoqKClKaigoKUpqKCgoKUpqKigpSmooKClKaiwoKUpqKCgqaigoKUpqKigpSmooKClKaiooKUprKigoKUpqLigpSmooKClKaigoKUprKCgoKUpqLigpSmooKClKaigoKUgoKUpqKCgpSmooKClKaigoKUpqLCgpSmosKClKaigoKUprKCgoKUpqKCgpSCgqaigoKUpqKCgpSmoqKClKaiooKUpqKigpSmooKClKaiAAgCgpSmsoKCgpSmooKClKaigoKUpqKigpSmoqKClKaiooKUpqKigpSmooKClKaigoKmosKClKaygoKUgoKClKaygoKClKai4oKUpqKCgpTigpSmosKClKai4oKUpoIACgKmosKClKaiooKUpqLigpSmogAKAoKUpqLigpSmogAIAoKUpqIACAKClKaigoKUgoKUAAgCgpSmosKClKaiwoKUpqLCgpSmosKClKaiwoKUpqKigpSmoqKClKaiooKmoqKClKaiooKmogAIAoKUpqIACAKClKaigoKmguKmoqKClKaCwqaCwqaigoKmosKClKaiooKUpqIACAKClKaigoKUpoKipoKCpqLCgqaiwoKUpqLCgpSmosKClKaiooKUpqKigpSmoqKClKaigoKUpqKCgpSigpSmooKClIKmosKClKai4oKUpqKigpSmoqKClKaigoKUpqIACAKClKaiAAgCgpSmssKCgpSmogAIAoKUpqKCgpSmoqKClKaiooKUpqLCgpSmsuKCgpSmouKClKaiooKUpqLCgpSmouKClKaiAAoCgpSmogAIAoKUpqIACAKClKaiooKUpqLCgpSmsqKCgpSmosKClKaiwoKUpqLCgpSmoqKClKaiwoKUpqKigpSmssKCgpSmosKClKaiAAgCgpSmgqKmoqKClKaigoKUprKigoKUpqKCgqaigoKmoqKClKaigoKmooKCprKigoKUpqKCgqaigoKmooKCprKigoKUprLCgoKUpqLCgqaigoKUpqKCgpSigpSmoqKClKaiwoKUprKCgoKUprLCgoKUpqKCgoKUpqLCgpSmooKCgpSmogAIAoKUpqKCgpSmooKClKaiwoKUpoKipqIACAKClKaigoKUpqKigpSmsuKCgpSmooKClOKClKaiAAgCgpSmosKClKaigoKUpqLCgpSmogAKAoKUpqIADgKClKaiAAgCgpSmogAKAoKUprKigoKUpqKigpSmoqKClKaigoKUpqKigpSmsoKCgpSmsqKCgpSmosKClKaygoKClKayooKClKaygoKClIKCgpSmssKCgpSmosKClKaiwoKUpqKCgpSmooKCprKCgpTigoKUpqKCgpTCgpSmoqKClKaigoKUprKCgoKUpqLCgpSmgoKmoqKClA==")]
// </GoSourcePositionMaps>

namespace go.golang.org.x.sys;

[GoPackage("windows", ImportPath = "golang.org/x/sys/windows")]
public static partial class windows_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    [GoLocalName("_FILE_END_OF_FILE_INFO")] internal partial struct Ftruncate__FILE_END_OF_FILE_INFO {}
    internal partial struct bufToUTF16_sl {}
    internal partial struct canDoSearchSystem32Onceᴛ1 {}
    internal partial struct connectExFuncᴛ1 {}
    internal partial struct getBuf_sl {}
    [GoValueClone("PathBuffer")] internal partial struct mountPointReparseBuffer {}
    internal partial struct reparseDataBuffer {}
    internal partial struct sendRecvMsgFuncᴛ1 {}
    [GoValueClone("PathBuffer")] internal partial struct symbolicLinkReparseBuffer {}
    internal partial struct sysLinger {}
    internal partial struct utf16ToBuf_sl {}
    [GoValueClone("FileName", "AlternateFileName")] internal partial struct win32finddata1 {}
    public partial class Pointer {}
    [GoLocalName("ResourceIDOrString")] public partial interface ResourceIDOrStringᴅ {}
    public partial interface Sockaddr {}
    public partial struct ACCESS_ALLOWED_ACE {}
    public partial struct ACCESS_MASK {}
    public partial struct ACCESS_MODE {}
    public partial struct ACE_HEADER {}
    public partial struct ACL {}
    public partial struct AFProtocols {}
    public partial struct AddrinfoW {}
    public partial struct BIND_OPTS3 {}
    public partial struct BLOB {}
    public partial struct ByHandleFileInformation {}
    public partial struct COAUTHIDENTITY {}
    public partial struct COAUTHINFO {}
    public partial struct CONFIGRET {}
    public partial struct COSERVERINFO {}
    public partial struct CSAddrInfo {}
    public partial struct CURDIR {}
    public partial struct CertBlob {}
    public partial struct CertChainContext {}
    public partial struct CertChainElement {}
    public partial struct CertChainFindByIssuerPara {}
    public partial struct CertChainPara {}
    public partial struct CertChainPolicyPara {}
    public partial struct CertChainPolicyStatus {}
    public partial struct CertContext {}
    public partial struct CertEnhKeyUsage {}
    public partial struct CertExtension {}
    public partial struct CertInfo {}
    public partial struct CertNameBlob {}
    public partial struct CertPoliciesInfo {}
    public partial struct CertPolicyInfo {}
    public partial struct CertPolicyQualifierInfo {}
    public partial struct CertPublicKeyInfo {}
    public partial struct CertRdnValueBlob {}
    public partial struct CertRevocationCrlInfo {}
    public partial struct CertRevocationInfo {}
    public partial struct CertSimpleChain {}
    public partial struct CertStrongSignPara {}
    public partial struct CertTrustListInfo {}
    public partial struct CertTrustStatus {}
    public partial struct CertUsageMatch {}
    public partial struct ClassInstallHeader {}
    public partial struct ComStat {}
    public partial struct CommTimeouts {}
    public partial struct ConsoleScreenBufferInfo {}
    public partial struct Coord {}
    public partial struct CrlBlob {}
    public partial struct CryptAlgorithmIdentifier {}
    public partial struct CryptAttrBlob {}
    public partial struct CryptBitBlob {}
    public partial struct CryptDataBlob {}
    public partial struct CryptDerBlob {}
    public partial struct CryptDigestBlob {}
    public partial struct CryptHashBlob {}
    public partial struct CryptIntegerBlob {}
    public partial struct CryptObjidBlob {}
    public partial struct CryptProtectPromptStruct {}
    public partial struct CryptUintBlob {}
    public partial struct DCB {}
    public partial struct DEVINST {}
    public partial struct DEVPROPGUID {}
    public partial struct DEVPROPID {}
    public partial struct DEVPROPKEY {}
    public partial struct DEVPROPTYPE {}
    public partial struct DICD {}
    public partial struct DICS_FLAG {}
    public partial struct DICS_STATE {}
    public partial struct DIGCF {}
    public partial struct DIREG {}
    public partial struct DI_FLAGS {}
    public partial struct DI_FLAGSEX {}
    public partial struct DI_FUNCTION {}
    public partial struct DI_REMOVEDEVICE {}
    public partial struct DLL {}
    public partial struct DLLError {}
    public partial struct DNSMXData {}
    public partial struct DNSPTRData {}
    [GoValueClone("Data")] public partial struct DNSRecord {}
    public partial struct DNSSRVData {}
    [GoValueClone("StringArray")] public partial struct DNSTXTData {}
    public partial struct DataBlob {}
    public partial struct DevInfo {}
    public partial struct DevInfoData {}
    [GoValueClone("remoteMachineName")] public partial struct DevInfoListDetailData {}
    [GoValueClone("driverPath")] public partial struct DevInstallParams {}
    [GoValueClone("description", "mfgName", "providerName")] public partial struct DrvInfoData {}
    [GoValueClone("sectionName", "infFileName", "drvDescription", "hardwareID")] public partial struct DrvInfoDetailData {}
    public partial struct ENUM_SERVICE_STATUS {}
    public partial struct ENUM_SERVICE_STATUS_PROCESS {}
    public partial struct EXPLICIT_ACCESS {}
    public partial struct FileNotifyInformation {}
    public partial struct FileZeroDataInformation {}
    public partial struct Filetime {}
    public partial struct GUID {}
    public partial struct GUIThreadInfo {}
    public partial struct HSPFILEQ {}
    public partial struct HWND {}
    public partial struct Handle {}
    public partial struct Hostent {}
    [GoValueClone("Addr")] public partial struct IN6_PKTINFO {}
    [GoValueClone("Addr")] public partial struct IN_PKTINFO {}
    public partial struct IO_COUNTERS {}
    public partial struct IO_STATUS_BLOCK {}
    public partial struct IPMreq {}
    [GoValueClone("Multiaddr")] public partial struct IPv6Mreq {}
    [GoValueClone("Address", "BroadcastAddress", "Netmask")] public partial struct InterfaceInfo {}
    [GoValueClone("PhysicalAddress", "ZoneIndices", "Dhcpv6ClientDuid")] public partial struct IpAdapterAddresses {}
    public partial struct IpAdapterAnycastAddress {}
    [GoValueClone("String")] public partial struct IpAdapterDNSSuffix {}
    public partial struct IpAdapterDnsServerAdapter {}
    public partial struct IpAdapterGatewayAddress {}
    [GoValueClone("AdapterName", "Description", "Address", "IpAddressList", "GatewayList", "DhcpServer", "PrimaryWinsServer", "SecondaryWinsServer")] public partial struct IpAdapterInfo {}
    public partial struct IpAdapterMulticastAddress {}
    public partial struct IpAdapterPrefix {}
    public partial struct IpAdapterUnicastAddress {}
    public partial struct IpAdapterWinsServerAddress {}
    [GoValueClone("IpAddress", "IpMask")] public partial struct IpAddrString {}
    public partial struct IpAddressPrefix {}
    [GoValueClone("String")] public partial struct IpAddressString {}
    [GoValueClone("Value")] public partial struct IpMaskString {}
    public partial struct JOBOBJECT_BASIC_LIMIT_INFORMATION {}
    public partial struct JOBOBJECT_BASIC_UI_RESTRICTIONS {}
    public partial struct JOBOBJECT_EXTENDED_LIMIT_INFORMATION {}
    public partial struct KNOWNFOLDERID {}
    [GoValueClone("reserved1", "reserved2", "reserved3", "reserved4", "reserved5")] public partial struct LDR_DATA_TABLE_ENTRY {}
    public partial struct LIST_ENTRY {}
    public partial struct LUID {}
    public partial struct LUIDAndAttributes {}
    public partial struct LazyDLL {}
    public partial struct LazyProc {}
    public partial struct Linger {}
    public partial struct MULTIPLE_TRUSTEE_OPERATION {}
    public partial struct MemoryBasicInformation {}
    public partial struct MibIfRow {}
    public partial struct MibIfRow2 {}
    [GoValueClone("Table")] public partial struct MibIfTable2 {}
    public partial struct MibIpForwardRow2 {}
    [GoValueClone("Table")] public partial struct MibIpForwardTable2 {}
    public partial struct MibIpInterfaceRow {}
    [GoValueClone("Table")] public partial struct MibIpInterfaceTable {}
    [GoValueClone("Address")] public partial struct MibUnicastIpAddressRow {}
    [GoValueClone("Table")] public partial struct MibUnicastIpAddressTable {}
    public partial struct ModuleEntry32 {}
    public partial struct ModuleInfo {}
    public partial struct NTStatus {}
    public partial struct NTString {}
    public partial struct NTUnicodeString {}
    public partial struct OBJECTS_AND_NAME {}
    public partial struct OBJECTS_AND_SID {}
    public partial struct OBJECT_ATTRIBUTES {}
    public partial struct OsVersionInfoEx {}
    public partial struct Overlapped {}
    [GoValueClone("reserved1", "reserved4", "reserved9", "reserved10", "reserved11", "reserved12")] public partial struct PEB {}
    [GoValueClone("reserved1", "reserved2")] public partial struct PEB_LDR_DATA {}
    public partial struct PROCESS_BASIC_INFORMATION {}
    public partial struct PSAPI_WORKING_SET_EX_BLOCK {}
    public partial struct PSAPI_WORKING_SET_EX_INFORMATION {}
    public partial struct Proc {}
    public partial struct ProcThreadAttributeList {}
    public partial struct ProcThreadAttributeListContainer {}
    public partial struct ProcessEntry32 {}
    public partial struct ProcessInformation {}
    public partial struct PropChangeParams {}
    public partial struct Protoent {}
    public partial struct QUERY_SERVICE_CONFIG {}
    public partial struct QUERY_SERVICE_LOCK_STATUS {}
    public partial struct RTLP_CURDIR_REF {}
    public partial struct RTL_DRIVE_LETTER_CURDIR {}
    [GoValueClone("Modules")] public partial struct RTL_PROCESS_MODULES {}
    [GoValueClone("FullPathName")] public partial struct RTL_PROCESS_MODULE_INFORMATION {}
    public partial struct RTL_RELATIVE_NAME {}
    [GoValueClone("CurrentDirectories")] public partial struct RTL_USER_PROCESS_PARAMETERS {}
    public partial struct RUNTIME_FUNCTION {}
    public partial struct RawSockaddr {}
    [GoValueClone("Pad")] public partial struct RawSockaddrAny {}
    [GoValueClone("AddressFamily", "BtAddr", "ServiceClassId", "Port")] public partial struct RawSockaddrBth {}
    public partial struct RawSockaddrInet {}
    [GoValueClone("Addr", "Zero")] public partial struct RawSockaddrInet4 {}
    [GoValueClone("Addr")] public partial struct RawSockaddrInet6 {}
    [GoValueClone("Path")] public partial struct RawSockaddrUnix {}
    public partial struct Rect {}
    public partial struct RemoveDeviceParams {}
    public partial struct ResourceID {}
    public partial struct Rusage {}
    public partial struct SC_ACTION {}
    public partial struct SECURITY_DESCRIPTOR {}
    public partial struct SECURITY_DESCRIPTOR_CONTROL {}
    public partial struct SECURITY_INFORMATION {}
    public partial struct SECURITY_QUALITY_OF_SERVICE {}
    public partial struct SERVICE_DELAYED_AUTO_START_INFO {}
    public partial struct SERVICE_DESCRIPTION {}
    public partial struct SERVICE_FAILURE_ACTIONS {}
    public partial struct SERVICE_FAILURE_ACTIONS_FLAG {}
    public partial struct SERVICE_NOTIFY {}
    public partial struct SERVICE_STATUS {}
    public partial struct SERVICE_STATUS_PROCESS {}
    public partial struct SERVICE_TABLE_ENTRY {}
    public partial struct SE_OBJECT_TYPE {}
    public partial struct SID {}
    public partial struct SIDAndAttributes {}
    public partial struct SPDIT {}
    public partial struct SPDRP {}
    public partial struct SSLExtraCertChainPolicyPara {}
    public partial struct SUOI {}
    public partial struct SYSTEM_PROCESS_INFORMATION {}
    public partial struct SecurityAttributes {}
    public partial struct Servent {}
    public partial struct SidIdentifierAuthority {}
    public partial struct SmallRect {}
    [GoValueClone("raw")] public partial struct SockaddrBth {}
    public partial struct SockaddrGen {}
    [GoValueClone("Addr", "raw")] public partial struct SockaddrInet4 {}
    [GoValueClone("Addr", "raw")] public partial struct SockaddrInet6 {}
    [GoValueClone("raw")] public partial struct SockaddrUnix {}
    public partial struct SocketAddress {}
    public partial struct StartupInfo {}
    public partial struct StartupInfoEx {}
    public partial struct Systemtime {}
    public partial struct TCPKeepalive {}
    public partial struct TRUSTEE {}
    public partial struct TRUSTEE_FORM {}
    public partial struct TRUSTEE_TYPE {}
    public partial struct ThreadEntry32 {}
    public partial struct Timespec {}
    public partial struct Timeval {}
    public partial struct Timezoneinformation {}
    public partial struct Token {}
    [GoValueClone("Groups")] public partial struct Tokengroups {}
    public partial struct Tokenmandatorylabel {}
    public partial struct Tokenprimarygroup {}
    [GoValueClone("Privileges")] public partial struct Tokenprivileges {}
    public partial struct Tokenuser {}
    public partial struct TransmitFileBuffers {}
    public partial struct TrusteeValue {}
    public partial struct UserInfo10 {}
    public partial struct VS_FIXEDFILEINFO {}
    public partial struct WELL_KNOWN_SID_TYPE {}
    public partial struct WSABuf {}
    public partial struct WSACMSGHDR {}
    [GoValueClone("Description", "SystemStatus")] public partial struct WSAData {}
    public partial struct WSAMsg {}
    public partial struct WSAProtocolChain {}
    public partial struct WSAProtocolInfo {}
    public partial struct WSAQUERYSET {}
    public partial struct WSAVersion {}
    public partial struct WTSSESSION_NOTIFICATION {}
    public partial struct WTS_SESSION_INFO {}
    public partial struct WaitStatus {}
    public partial struct Win32FileAttributeData {}
    [GoValueClone("FileName", "AlternateFileName")] public partial struct Win32finddata {}
    public partial struct WinTrustData {}
    public partial struct WinTrustFileInfo {}
    public partial struct WinTrustSignatureSettings {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸbinary() => builtin.initPackage(typeof(encoding.binary_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸnet() => builtin.initPackage(typeof(net_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
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
[GoMethodHost(typeof(windows_package.CONFIGRET)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class windowsᴛCONFIGRET;
[GoMethodHost(typeof(windows_package.DLL)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class windowsᴛDLL;
[GoMethodHost(typeof(windows_package.DLLError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class windowsᴛDLLError;
[GoMethodHost(typeof(windows_package.DevInfo)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class windowsᴛDevInfo;
[GoMethodHost(typeof(windows_package.DevInfoListDetailData)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class windowsᴛDevInfoListDetailData;
[GoMethodHost(typeof(windows_package.DevInstallParams)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class windowsᴛDevInstallParams;
[GoMethodHost(typeof(windows_package.DrvInfoData)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class windowsᴛDrvInfoData;
[GoMethodHost(typeof(windows_package.DrvInfoDetailData)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class windowsᴛDrvInfoDetailData;
[GoMethodHost(typeof(windows_package.Filetime)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class windowsᴛFiletime;
[GoMethodHost(typeof(windows_package.GUID)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class windowsᴛGUID;
[GoMethodHost(typeof(windows_package.LazyDLL)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class windowsᴛLazyDLL;
[GoMethodHost(typeof(windows_package.LazyProc)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class windowsᴛLazyProc;
[GoMethodHost(typeof(windows_package.MibIpForwardTable2)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class windowsᴛMibIpForwardTable2;
[GoMethodHost(typeof(windows_package.NTStatus)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class windowsᴛNTStatus;
[GoMethodHost(typeof(windows_package.NTString)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class windowsᴛNTString;
[GoMethodHost(typeof(windows_package.NTUnicodeString)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class windowsᴛNTUnicodeString;
[GoMethodHost(typeof(windows_package.PSAPI_WORKING_SET_EX_BLOCK)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class windowsᴛPSAPI_WORKING_SET_EX_BLOCK;
[GoMethodHost(typeof(windows_package.Proc)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class windowsᴛProc;
[GoMethodHost(typeof(windows_package.ProcThreadAttributeListContainer)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class windowsᴛProcThreadAttributeListContainer;
[GoMethodHost(typeof(windows_package.RawSockaddrAny)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class windowsᴛRawSockaddrAny;
[GoMethodHost(typeof(windows_package.SECURITY_DESCRIPTOR)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class windowsᴛSECURITY_DESCRIPTOR;
[GoMethodHost(typeof(windows_package.SID)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class windowsᴛSID;
[GoMethodHost(typeof(windows_package.SockaddrBth)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class windowsᴛSockaddrBth;
[GoMethodHost(typeof(windows_package.SockaddrInet4)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class windowsᴛSockaddrInet4;
[GoMethodHost(typeof(windows_package.SockaddrInet6)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class windowsᴛSockaddrInet6;
[GoMethodHost(typeof(windows_package.SockaddrUnix)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class windowsᴛSockaddrUnix;
[GoMethodHost(typeof(windows_package.SocketAddress)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class windowsᴛSocketAddress;
[GoMethodHost(typeof(windows_package.Timespec)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class windowsᴛTimespec;
[GoMethodHost(typeof(windows_package.Timeval)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class windowsᴛTimeval;
[GoMethodHost(typeof(windows_package.Token)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class windowsᴛToken;
[GoMethodHost(typeof(windows_package.Tokengroups)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class windowsᴛTokengroups;
[GoMethodHost(typeof(windows_package.Tokenmandatorylabel)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class windowsᴛTokenmandatorylabel;
[GoMethodHost(typeof(windows_package.Tokenprivileges)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class windowsᴛTokenprivileges;
[GoMethodHost(typeof(windows_package.WaitStatus)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class windowsᴛWaitStatus;
// </MethodHolders>
