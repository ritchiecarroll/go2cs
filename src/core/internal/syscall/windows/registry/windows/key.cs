// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build windows

// Package registry provides access to the Windows registry.
//
// Here is a simple example, opening a registry key and reading a string value from it.
//
//	k, err := registry.OpenKey(registry.LOCAL_MACHINE, `SOFTWARE\Microsoft\Windows NT\CurrentVersion`, registry.QUERY_VALUE)
//	if err != nil {
//		log.Fatal(err)
//	}
//	defer k.Close()
//
//	s, _, err := k.GetStringValue("SystemRoot")
//	if err != nil {
//		log.Fatal(err)
//	}
//	fmt.Printf("Windows system root is %q\n", s)
//
// NOTE: This package is a copy of golang.org/x/sys/windows/registry
// with KeyInfo.ModTime removed to prevent dependency cycles.
namespace go.@internal.syscall.windows;

using runtime = runtime_package;
using syscall = syscall_package;

partial class registry_package {

public static UntypedInt ALL_ACCESS => 0xf003f;
public static UntypedInt CREATE_LINK => 0x00020;
public static UntypedInt CREATE_SUB_KEY => 0x00004;
public static UntypedInt ENUMERATE_SUB_KEYS => 0x00008;
public static UntypedInt EXECUTE => 0x20019;
public static UntypedInt NOTIFY => 0x00010;
public static UntypedInt QUERY_VALUE => 0x00001;
public static UntypedInt READ => 0x20019;
public static UntypedInt SET_VALUE => 0x00002;
public static UntypedInt WOW64_32KEY => 0x00200;
public static UntypedInt WOW64_64KEY => 0x00100;
public static UntypedInt WRITE => 0x20006;

[GoType("global::go.syscall_package.ΔHandle")] partial struct Key;

public static Key CLASSES_ROOT => /* Key(syscall.HKEY_CLASSES_ROOT) */ unchecked((Key)(syscallꓸHandle)2147483648);
public static Key CURRENT_USER => /* Key(syscall.HKEY_CURRENT_USER) */ unchecked((Key)(syscallꓸHandle)2147483649);
public static Key LOCAL_MACHINE => /* Key(syscall.HKEY_LOCAL_MACHINE) */ unchecked((Key)(syscallꓸHandle)2147483650);
public static Key USERS => /* Key(syscall.HKEY_USERS) */ unchecked((Key)(syscallꓸHandle)2147483651);
public static Key CURRENT_CONFIG => /* Key(syscall.HKEY_CURRENT_CONFIG) */ unchecked((Key)(syscallꓸHandle)2147483653);

// Close closes open key k.
public static error Close(this Key k) {
    return syscall.RegCloseKey(((syscallꓸHandle)k));
}

// OpenKey opens a new key with path name relative to key k.
// It accepts any open key, including CURRENT_USER and others,
// and returns the new key and an error.
// The access parameter specifies desired access rights to the
// key to be opened.
public static (Key, error) OpenKey(Key k, @string path, uint32 access) {
    var (p, err) = syscall.UTF16PtrFromString(path);
    if (err != default!) {
        return ((Key)(syscallꓸHandle)(0), err);
    }
    ref var subkey = ref heap(new syscallꓸHandle(), out var Ꮡsubkey);
    err = syscall.RegOpenKeyEx(((syscallꓸHandle)k), p, 0, access, Ꮡsubkey);
    if (err != default!) {
        return ((Key)(syscallꓸHandle)(0), err);
    }
    return (((Key)subkey), default!);
}

// ReadSubKeyNames returns the names of subkeys of key k.
public static (slice<@string>, error) ReadSubKeyNames(this Key k) {
    GoFrame ᒐ = default;
    try {
        // RegEnumKeyEx must be called repeatedly and to completion.
        // During this time, this goroutine cannot migrate away from
        // its current thread. See #49320.
        runtime.LockOSThread();
        defer(runtime.UnlockOSThread, ref ᒐ);
        var names = new slice<@string>(0);
        // Registry key size limit is 255 bytes and described there:
        // https://learn.microsoft.com/en-us/windows/win32/sysinfo/registry-element-size-limits
        var buf = new slice<uint16>(256); //plus extra room for terminating zero byte
loopItems:
        for (var i = (uint32)0; ᐧ ; i++) {
            ref var l = ref heap<uint32>(out var Ꮡl);
            l = (uint32)len(buf);
            while (ᐧ) {
                var err = syscall.RegEnumKeyEx(((syscallꓸHandle)k), i, Ꮡ(buf, 0), Ꮡl, nil, nil, nil, nil);
                if (err == default!) {
                    break;
                }
                if (AreEqual(err, syscall.ERROR_MORE_DATA)) {
                    // Double buffer size and try again.
                    l = (uint32)(2 * len(buf));
                    buf = new slice<uint16>((nint)(l));
                    continue;
                }
                if (AreEqual(err, _ERROR_NO_MORE_ITEMS)) {
                    goto break_loopItems;
                }
                return (names, err);
            }
            names = append(names, syscall.UTF16ToString(buf.slice(0, (nint)(l))));
continue_loopItems:;
        }
break_loopItems:;
        return (names, default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// CreateKey creates a key named path under open key k.
// CreateKey returns the new key and a boolean flag that reports
// whether the key already existed.
// The access parameter specifies the access rights for the key
// to be created.
public static (Key newk, bool openedExisting, error err) CreateKey(Key k, @string path, uint32 access) {
    error err = default!;

    ref var h = ref heap(new syscallꓸHandle(), out var Ꮡh);
    ref var d = ref heap(new uint32(), out var Ꮡd);
    err = regCreateKeyEx(((syscallꓸHandle)k), syscall.StringToUTF16Ptr(path),
        0, nil, _REG_OPTION_NON_VOLATILE, access, nil, Ꮡh, Ꮡd);
    if (err != default!) {
        return ((Key)(syscallꓸHandle)(0), false, err);
    }
    return (((Key)h), d == _REG_OPENED_EXISTING_KEY, default!);
}

// DeleteKey deletes the subkey path of key k and its values.
public static error DeleteKey(Key k, @string path) {
    return regDeleteKey(((syscallꓸHandle)k), syscall.StringToUTF16Ptr(path));
}

// A KeyInfo describes the statistics of a key. It is returned by Stat.
[GoType] partial struct KeyInfo {
    public uint32 SubKeyCount;
    public uint32 MaxSubKeyLen; // size of the key's subkey with the longest name, in Unicode characters, not including the terminating zero byte
    public uint32 ValueCount;
    public uint32 MaxValueNameLen; // size of the key's longest value name, in Unicode characters, not including the terminating zero byte
    public uint32 MaxValueLen; // longest data component among the key's values, in bytes
    internal syscall.Filetime lastWriteTime;
}

// Stat retrieves information about the open key k.
public static (ж<KeyInfo>, error) Stat(this Key k) {
    ref var ki = ref heap(new KeyInfo(), out var Ꮡki);
    var err = syscall.RegQueryInfoKey(((syscallꓸHandle)k), nil, nil, nil,
        Ꮡki.of(KeyInfo.ᏑSubKeyCount), Ꮡki.of(KeyInfo.ᏑMaxSubKeyLen), nil, Ꮡki.of(KeyInfo.ᏑValueCount),
        Ꮡki.of(KeyInfo.ᏑMaxValueNameLen), Ꮡki.of(KeyInfo.ᏑMaxValueLen), nil, Ꮡki.of(KeyInfo.ᏑlastWriteTime));
    if (err != default!) {
        return (default!, err);
    }
    return (Ꮡki, default!);
}

} // end registry_package
